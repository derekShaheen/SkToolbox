using BepInEx.Configuration;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Settings manager with code based on https://github.com/zambony/Gungnir/blob/master/src/ConfigManager.cs. Accessed 9/13/22
/// </summary>
namespace SkToolbox.Controllers
{
    public class SettingsController
    {
        private static Dictionary<string, ConfigEntryBase> s_config = new Dictionary<string, ConfigEntryBase>();

        public static string CategoryGeneral        = "0 - General";
        public static string CategoryConsoleDraw    = "1 - Console Specific";

        /// <summary>
        /// Initialize the config file and values.
        /// </summary>
        /// <param name="config">Reference to the global <see cref="ConfigFile"/> created by BepInEx for this plugin.</param>
        public static void Init(ConfigFile config)
        {
            s_config.Add("- Index", config.Bind("- Index", "ThisIsJustAnIndex-NotASetting", true, "Config sections:" +
                                                        "\n" + CategoryGeneral +
                                                        "\n" + CategoryConsoleDraw +
                                                        // ...
                                                        "\n"));

            s_config.Add("ConsoleEnabled", config.Bind(CategoryGeneral, "ConsoleEnabled", true, "Whether the console should be enabled or not. Defaults to true."));
            s_config.Add("PanelEnabled", config.Bind(CategoryGeneral, "PanelEnabled", true, "Whether the panel should be enabled or not. Defaults to true."));

            s_config.Add("ConsoleTheme", config.Bind(CategoryConsoleDraw, "ConsoleTheme", Color.grey, "Sets the overall color theme. Accepts hex and some standard color words."));
            s_config.Add("ConsolePosition", config.Bind(CategoryConsoleDraw, "ConsolePosition", Settings.Console.ConsolePos.TopCentered));
            s_config.Add("ConsoleWidth", config.Bind(CategoryConsoleDraw, "ConsoleWidth", -1, "Positive in pixels. Negative numbers will divide the screen by the number given. Ex. -2 will divide the screen in half, -3 in thirds, etc.."));
            s_config.Add("ConsoleHeight", config.Bind(CategoryConsoleDraw, "ConsoleHeight", -2, "Positive in pixels. Negative numbers will divide the screen by the number given. Ex. -2 will divide the screen in half, -3 in thirds, etc.."));
            s_config.Add("ConsoleMaxOutput", config.Bind(CategoryConsoleDraw, "ConsoleMaxOutput", 999, "Maximum number of lines to display in output window."));
            s_config.Add("ConsoleFontSize", config.Bind(CategoryConsoleDraw, "ConsoleFontSize", 16, "Size of the font within the console window."));
            s_config.Add("ConsoleDarkenBackground", config.Bind(CategoryConsoleDraw, "ConsoleDarkenBackground", false, "Darken the rest of the screen around the console when visible."));

            if (!Settings.Console.ShowConsole)
            {
                Settings.Console.ShowPanel = true; // Show panel if console is disabled
            }
        }

        /// <summary>
        /// Retrieve a specific config value by name.
        /// </summary>
        /// <typeparam name="T">Type of value stored at the given <paramref name="key"/></typeparam>
        /// <param name="key">Config value to lookup.</param>
        /// <returns>The value contained at that key, otherwise a default value is given.</returns>
        public static T Get<T>(string key)
        {
            ConfigEntryBase obj;
            ConfigEntry<T> value;

            if (!s_config.TryGetValue(key, out obj))
            {
                Debug.Log($"Attempt to access nonexistent config value '{key}'.");
                return default;
            }

            value = obj as ConfigEntry<T>;

            return value.Value;
        }

        /// <summary>
        /// Set a specific config value by name.
        /// </summary>
        /// <typeparam name="T">Type of value stored at the given <paramref name="key"/></typeparam>
        /// <param name="key">Config value to set.</param>
        /// <param name="value">Value to set the key to.</param>
        /// <returns></returns>
        public static void Set<T>(string key, T value)
        {
            if (!s_config.TryGetValue(key, out ConfigEntryBase obj))
            {
                Debug.Log($"Attempt to access nonexistent config value '{key}'.");
                return;
            }

            var converted = obj as ConfigEntry<T>;
            converted.Value = value;
        }
    }
}
