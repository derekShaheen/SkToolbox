using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
/// <summary>
/// Based on the Gungnir code by Zambony. Accessed 9/11/22
/// https://github.com/zambony/Gungnir/
/// </summary>
namespace SkToolbox
{
    [Serializable]
    internal class TooManyValuesException : Exception
    {
        public int expected;
        public int actual;

        public TooManyValuesException(int expected, int actual) : base($"Too many values found, expected {expected}, got {actual}")
        {
            this.expected = expected;
            this.actual = actual;
        }
    }

    [Serializable]
    internal class NoMatchFoundException : Exception
    {
        public string key;

        public NoMatchFoundException(string key) : base($"No match found for key: {key}")
        {
            this.key = key;
        }
    }

    /// <summary>
    /// Collection of utility functions to find players, trim text, and do basic tasks.
    /// </summary>
    public static class Util
    {
        public enum DisplayOptions
        {
            All = 0,
            ConsoleOnly = 1,
            PanelOnly = 2,
        }

        private static readonly Regex s_tagStripPattern = new Regex(@"<((?:b)|(?:i)|(?:size)|(?:color)|(?:quad)|(?:material)).*?>(.*?)<\/\1>");
        private static readonly Regex s_colorTagPattern = new Regex(@"<color=#(\w{6})(\w{2})?>");
        private const string s_commandPattern = @"[^\s""']+|""([^""]*)""|'([^']*)'";
        private static Dictionary<string, GameObject> s_cachedPrefabs = new Dictionary<string, GameObject>();

        public static string CommandPattern => s_commandPattern;

        /// <summary>
        /// Multiplies the alpha byte of all color tags in a rich text string by <paramref name="alpha"/> and
        /// returns a new string with the new alpha.
        /// </summary>
        /// <param name="text">RichText with color tags that need their alpha modified.</param>
        /// <param name="alpha">Multiplier for the alpha.</param>
        /// <returns>A <see langword="string"/> with all color tag alphas modified.</returns>
        public static string MultiplyColorTagAlpha(string text, float alpha)
        {
            return s_colorTagPattern.Replace(text, m =>
            {
                string oldAlpha = m.Groups[2].Value;
                if (string.IsNullOrEmpty(oldAlpha))
                    oldAlpha = "FF";

                string newAlpha = ((int)(alpha * Convert.ToByte(oldAlpha, 16))).ToString("X2");
                return $"<color=#{m.Groups[1].Value}{newAlpha}>";
            });
        }

        /// <summary>
        /// Split a string up into individual words. Phrases surrounded by quotes will count as a single item.
        /// </summary>
        /// <param name="text">Text to separate</param>
        /// <returns>A <see cref="List{string}"/> containing the separated pieces.</returns>
        public static List<string> SplitByQuotes(string text)
        {
            return Regex.Matches(text, s_commandPattern)
                .OfType<Match>()
                .Select(m => {
                    if (!string.IsNullOrEmpty(m.Groups[1].Value))
                        return m.Groups[1].Value;
                    else if (!string.IsNullOrEmpty(m.Groups[2].Value))
                        return m.Groups[2].Value;
                    else
                        return m.Value;
                })
                .ToList();
        }

        public static Match[] SplitArgs(string inputString)
        {
            return Regex.Matches(inputString, Util.CommandPattern).OfType<Match>().Skip(1).ToArray();
        }

        /// <summary>
        /// Generic version of <see cref="SplitByQuotes(string)"/>.
        /// </summary>
        /// <param name="text">Text to separate.</param>
        /// <param name="pattern">Pattern to use.</param>
        /// <returns>A <see cref="List{string}"/> containing the separated pieces.</returns>
        public static List<string> SplitByPattern(string text, string pattern)
        {
            return Regex.Matches(text, pattern)
                .OfType<Match>()
                .Select(m => m.Groups[0].Value)
                .ToList();
        }

        /// <summary>
        /// Split a string by a separator character, except if the separator is enclosed in quotes.
        /// </summary>
        /// <param name="text">Text to split.</param>
        /// <param name="separator">Character to separate sections with.</param>
        /// <param name="keepEmpty">True to keep empty sections, false otherwise.</param>
        /// <returns>A <see cref="List{string}"/> of all the separated sections, not including the separator character.</returns>
        public static List<string> SplitEscaped(this string text, char separator = ',', bool keepEmpty = false)
        {
            List<string> result = new List<string>();
            bool escaped = false;
            int start = 0;
            int end = 0;

            for (int i = 0; i < text.Length; ++i)
            {
                if (text[i] == '"')
                {
                    escaped = !escaped;
                }
                else if (text[i] == separator && !escaped)
                {
                    end = i - 1;
                    result.Add(text.Substring(start, end - start + 1));
                    start = i + 1;
                }

                if (i == text.Length - 1)
                {
                    result.Add(text.Substring(start, text.Length - start));
                }
            }

            if (!keepEmpty)
                result.RemoveAll(string.IsNullOrEmpty);

            return result;
        }

        /// <summary>
        /// Trims leading and trailing spaces, and collapses repeating spaces to a single space.
        /// </summary>
        /// <param name="value"><see langword="string"/> to clean up.</param>
        /// <returns>Simplified version of the string.</returns>
        public static string Simplified(this string value)
        {
            return Regex.Replace(value.Trim(), @"\s{2,}", " ");
        }

        /// <summary>
        /// Some of Valheim's prefabs are not listed in the ZNetScene manager. If you know the name of them,
        /// use this function to find them in the scene hierarchy and instantiate them.
        /// </summary>
        /// <param name="name">Prefab name.</param>
        /// <returns>The prefab.</returns>
        public static GameObject GetHiddenPrefab(string name)
        {
            if (s_cachedPrefabs.TryGetValue(name.ToLower(), out GameObject ret))
                return ret;

            var objects = Resources.FindObjectsOfTypeAll<Transform>()
                .Where(t => t.parent == null)
                .Select(x => x.gameObject);

            foreach (var prefab in objects)
            {
                if (prefab.name.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    s_cachedPrefabs.Add(name.ToLower(), prefab);
                    return prefab;
                }
            }

            return null;
        }

        /// <summary>
        /// Converts a text value to the corresponding type and returns it as a generic <see langword="object"/>.
        /// </summary>
        /// <param name="value">Text to convert to some object.</param>
        /// <param name="toType"><see cref="Type"/> value to convert to.</param>
        /// <param name="noThrow">Whether to throw conversion exceptions or not.</param>
        /// <returns>An <see langword="object"/> reference to the converted type.</returns>
        /// <exception cref="InvalidCastException"/>
        /// <exception cref="FormatException"/>
        /// <exception cref="OverflowException"/>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="TooManyValuesException"/>
        /// <exception cref="NoMatchFoundException"/>
        public static object StringToObject(string value, Type toType, bool noThrow = false)
        {
            try
            {
                if (Nullable.GetUnderlyingType(toType) != null)
                    toType = Nullable.GetUnderlyingType(toType);

                if (toType == typeof(string))
                    return value;
                else if (toType == typeof(bool))
                {
                    if (value.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                        value.Equals("1", StringComparison.OrdinalIgnoreCase) ||
                        value.Equals("yes", StringComparison.OrdinalIgnoreCase))
                        return true;
                    else
                        return false;
                }

                return Convert.ChangeType(value, toType);
            }
            catch (Exception)
            {
                if (!noThrow)
                    throw;

                return null;
            }
        }

        public static object[] StringsToObjects(string[] values, Type toType, bool noThrow = false)
        {
            object[] result = new object[values.Length];

            for (int i = 0; i < result.Length; ++i)
            {
                result[i] = StringToObject(values[i], toType, noThrow);
            }

            return result;
        }

        /// <summary>
        /// Translates a <see cref="Type"/> to a nice user-friendly name.
        /// </summary>
        /// <param name="type"><see cref="Type"/> to translate</param>
        /// <returns><see langword="string"/> containing the type name.</returns>
        public static string GetSimpleTypeName(Type type)
        {
            if (Nullable.GetUnderlyingType(type) != null)
                type = Nullable.GetUnderlyingType(type);

            if (type.IsArray)
                return GetSimpleTypeName(type.GetElementType()) + "[]";

            switch (type.Name)
            {
                case nameof(Int32):
                case nameof(Int64):
                    {
                        return "Number";
                    }
                case nameof(UInt32):
                case nameof(UInt64):
                    {
                        return "(+)Number";
                    }
                case nameof(Single):
                case nameof(Double):
                    {
                        return "Decimal";
                    }
                default:
                    return type.Name;
            }
        }

        /// <summary>
        /// Strips away any markdown tags such as b, i, color, etc. from Text label input.
        /// </summary>
        /// <param name="input">Text to sanitize.</param>
        /// <returns>A <see langword="string"/> containing the sanitized text.</returns>
        public static string StripTags(string input)
        {
            return s_tagStripPattern.Replace(input, (Match match) =>
            {
                return match.Groups[2].Value;
            });
        }

        /// <summary>
        /// Extension method to convert a list object to a nicely formatted string, since <see cref="List{T}.ToString"/> isn't helpful.
        /// </summary>
        /// <typeparam name="T">List type.</typeparam>
        /// <param name="input">List to convert to text.</param>
        /// <returns>A formatted <see langword="string"/> of the list's contents and type.</returns>
        public static string AsText<T>(this List<T> input)
        {
            string value = $"List<{typeof(T).Name}>(";

            foreach (var item in input)
                value += item.ToString() + ",";

            value = value.Remove(value.Length - 1);
            value += ")";

            return value;
        }

        /// <summary>
        /// Helper to find a partial match from an enumerable collection.
        /// </summary>
        /// <param name="input">Collection to search through.</param>
        /// <param name="key">Needle to find in the collection.</param>
        /// <param name="noThrow">Whether exceptions should be thrown for specificity.</param>
        /// <returns>A <see langword="string"/> with the found item, or <see langword="null"/> if nothing.</returns>
        /// <exception cref="NoMatchFoundException"></exception>
        /// <exception cref="TooManyValuesException"></exception>
        public static string GetPartialMatch(IEnumerable<string> input, string key, bool noThrow = false)
        {
            IEnumerable<string> query =
                            from item in input
                            where item.StartsWith(key, StringComparison.OrdinalIgnoreCase)
                            orderby item.Length
                            select item;

            int count = query.Count();

            if (count <= 0)
            {
                if (!noThrow)
                    throw new NoMatchFoundException(key);
            }
            else
            {
                string first = query.First();

                // Test if we have just one result, or the first result is an exact match.
                if (count == 1 || first.Equals(key, StringComparison.OrdinalIgnoreCase))
                    return first;
                else if (!noThrow)
                    throw new TooManyValuesException(1, count);
            }

            return null;
        }

        /// <summary>
        /// Convert text from something like "testCommand" to "Test Command"
        /// </summary>
        /// <param name="inputText"></param>
        /// <returns></returns>
        public static string ConvertCamelToHuman(string inputText)
        {
            inputText = inputText.Simplified();
            if (inputText.Substring(0, 1).Equals(inputText.Substring(0, 1).ToLower()))
            {
                inputText = inputText.Substring(0, 1).ToUpper() + inputText.Substring(1);
            }
            byte[] asciiBytes = System.Text.Encoding.ASCII.GetBytes(inputText);
            for (int i = 1; i < asciiBytes.Length; i++)
            {
                if (asciiBytes[i] > 64 && asciiBytes[i] < 91)
                {
                    inputText = inputText.Insert(i, " ");
                }
            }
            return inputText;
        }

        /// <summary>
        /// Strip all non word characters except periods,hyphens, and @ symbols
        /// </summary>
        /// <param name="strIn"></param>
        /// <returns></returns>
        public static string CleanInput(string strIn)
        {
            // Replace invalid characters with empty strings.
            try
            {
                return Regex.Replace(strIn, @"[^\w\.@-]", "",
                                     RegexOptions.None, TimeSpan.FromSeconds(1.5));
            }
            // If we timeout when replacing invalid characters,
            // we should return Empty.
            catch (RegexMatchTimeoutException)
            {
                return String.Empty;
            }
        }

        /// <summary>
        /// Extension method to create markup color tags around a string.
        /// </summary>
        /// <param name="text">Text to wrap with color tags.</param>
        /// <param name="color"><see cref="Color"/> to use for the tag. Alpha is ignored.</param>
        /// <returns>A <see langword="string"/> wrapped with color tags.</returns>
        public static string WithColor(this string text, Color color)
        {
            return $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{text}</color>";
        }

        public static T GetPrivateField<T>(this object self, string field)
        {
            return (T)self.GetType().GetField(field, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static).GetValue(self);
        }

        public static T GetPrivateStaticField<T>(Type type, string field)
        {
            return (T)type.GetField(field, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
        }

        public static void SetPrivateField<T>(this object self, string field, T value)
        {
            self.GetType().GetField(field, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static).SetValue(self, value);
        }

        public static void SetPrivateStaticField<T>(Type type, string field, T value)
        {
            type.GetField(field, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static).SetValue(null, value);
        }

        public static T StaticInvokePrivate<T>(Type type, string method, params object[] args)
        {
            return (T)type.GetMethod(method, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, args);
        }

        public static T InvokePrivate<T>(this object self, string method, params object[] args)
        {
            return (T)self.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static).Invoke(self, args);
        }

        public static float Distance2D(float x1, float y1, float x2, float y2)
        {
            float a = x2 - x1;
            float b = y2 - y1;
            return Mathf.Sqrt(a * a + b * b);
        }
    }
}
