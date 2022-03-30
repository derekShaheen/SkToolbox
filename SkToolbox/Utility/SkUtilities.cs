using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace SkToolbox.Utility
{
    public static class SkUtilities
    {
        public static bool ConvertInternalWarningsErrors = false; // Should we allow output of warnings and errors from SkToolbox, or suppress them all to regular log output? // True = suppress
        public static BindingFlags BindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;

        /// <summary>
        /// Uses reflection to get the field value from an object.
        /// </summary>
        ///
        /// <param name="type">The instance type.</param>
        /// <param name="instance">The instance object.</param>
        /// <param name="fieldName">The field's name which is to be fetched.</param>
        ///
        /// <returns>The field value from the object.</returns>
        //internal static object GetInstanceField(System.Type type, object instance, string fieldName)
        //{
        //    FieldInfo field = type.GetField(fieldName, BindFlags);
        //    return field.GetValue(instance);
        //}

        //internal static object SetInstanceField(System.Type type, object instance, string fieldName, object fieldValue)
        //{
        //    FieldInfo field = type.GetField(fieldName, BindFlags);
        //    field.SetValue(instance, fieldValue);

        //    return field.GetValue(instance);
        //}
        public static void GameobjectSetPrivateField(this string objName, string componentName, string fieldName, object value)
        {
            GameObject GameObj = GameObject.Find(objName);
            object obj = GameObj.GetComponent(componentName);
            var prop = obj.GetType().GetField(fieldName, BindFlags);
            try
            {
                object objVal = Convert.ChangeType(value, prop.FieldType);
                prop.SetValue(obj, objVal);
            } catch (ArgumentException)
            {
                float.TryParse(value.ToString(), out float newValue);
                prop.SetValue(obj, newValue);
            }
        }

        public static T GameobjectGetPrivateField<T>(this string objName, string componentName, string fieldName)
        {
            GameObject GameObj = GameObject.Find(objName);
            object obj = GameObj.GetComponent(componentName);
            var prop = obj.GetType().GetField(fieldName, BindFlags);
            var value = prop.GetValue(obj);
            return (T)value;
        }

        public static void SetPrivateField(this object obj, string fieldName, object value)
        {
            var prop = obj.GetType().GetField(fieldName, BindFlags);
            prop.SetValue(obj, value);
        }

        public static T GetPrivateField<T>(this object obj, string fieldName)
        {
            var prop = obj.GetType().GetField(fieldName, BindFlags);
            var value = prop.GetValue(obj);
            return (T)value;
        }

        public static void SetPrivateProperty(this object obj, string propertyName, object value)
        {
            var prop = obj.GetType()
                .GetProperty(propertyName, BindFlags);
            prop.SetValue(obj, value, null);
        }

        public static void InvokePrivateMethod(this object obj, string methodName, object[] methodParams)
        {
            MethodInfo dynMethod = obj.GetType().GetMethod(methodName, BindFlags);
            dynMethod.Invoke(obj, methodParams);
        }

        public static Component CopyComponent(Component original, Type originalType, Type overridingType,
            GameObject destination)
        {
            var copy = destination.AddComponent(overridingType);
            var fields = originalType.GetFields(BindFlags);
            foreach (var field in fields)
            {
                var value = field.GetValue(original);
                field.SetValue(copy, value);
            }

            return copy;
        }

        public static T ParseEnum<T>(string value)
        {
            return (T)Enum.Parse(typeof(T), value, true);
        }

        public static string GetAllProperiesOfObject(string thisObject, string componentName)
        {
            GameObject GameObj = GameObject.Find(thisObject);
            object obj = GameObj.GetComponent(componentName);

            if (obj == null)
            {
                return "Component not found. Check component name, character case matters.";
            }

            string result = string.Empty;
            try
            {
                // get all public static properties of MyClass type
                FieldInfo[] propertyInfos;
                propertyInfos = obj.GetType().GetFields(BindFlags);//By default, it will return only public properties.
                                                                                                              // sort properties by name
                Array.Sort(propertyInfos,
                           (propertyInfo1, propertyInfo2) => propertyInfo1.Name.CompareTo(propertyInfo2.Name));

                // write property names
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                foreach (FieldInfo propertyInfo in propertyInfos)
                {
                    sb.Append("\n");
                    sb.AppendFormat("Name: {0} | Value: {1}", propertyInfo.Name, propertyInfo.GetValue(obj));
                }
                result = sb.ToString();
            }
            catch (Exception)
            {
                // to do log it
            }

            return result;
        }

        public static string GetAllComponentsOnGameobject(string thisObject)
        {
            GameObject GameObj = GameObject.Find(thisObject);
            Component[] components = GameObj.GetComponents(typeof(Component));

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            foreach (Component component in components)
            {
                sb.Append("\n");
                sb.AppendFormat("Component: {0}", component.GetType());
            }
            return sb.ToString();
        }

        public delegate object Command(params object[] args);

        public enum Status
        {
            Initialized,
            Loading,
            Ready,
            Error,
            Unload
        }

        /// <summary>
        /// Used for logging to the console in a controlled manner<br>Example Usage: SkUtilities.Logz(new string[] { "CMD", "REQ" }, new string[] { "Submenu Created" });</br>
        /// </summary>
        /// <param name="categories"></param>
        /// <param name="messages"></param>
        /// <param name="callerClass"></param>
        /// <param name="callerMethod"></param>
        public static void Logz(string[] categories, string[] messages, LogType logType = LogType.Log)
        {
            string strBuild = string.Empty;
            if (categories != null)
            {
                foreach (string cat in categories)
                {
                    strBuild = strBuild + " (" + cat + ") → ";
                }
            }
            if (messages != null)
            {
                foreach (string msg in messages)
                {
                    if (msg != null)
                    {
                        strBuild = strBuild + msg + " | ";
                    }
                    else
                    {
                        strBuild = strBuild + "NULL" + " | ";
                    }
                }
                strBuild = strBuild.Remove(strBuild.Length - 2, 1);
            }
            //Get the class that called the log
            if (!ConvertInternalWarningsErrors)
            {
                switch (logType)
                {
                    case LogType.Error:
                        Debug.LogError("(SkToolbox) → " + strBuild);
                        break;
                    case LogType.Warning:
                        Debug.LogWarning("(SkToolbox) → " + strBuild);
                        break;
                    default:
                        Debug.Log("(SkToolbox) → " + strBuild);
                        break;
                }
            }
            else
            {
                Debug.Log("(SkToolbox) → " + strBuild);
            }
        }

        public static string Logr(string[] categories, string[] messages)
        {
            string strBuild = string.Empty;
            if (categories != null)
            {
                foreach (string cat in categories)
                {
                    strBuild = strBuild + " (" + cat + ") → ";
                }
            }
            if (messages != null)
            {
                foreach (string msg in messages)
                {
                    if (msg != null)
                    {
                        strBuild = strBuild + msg + " | ";
                    }
                    else
                    {
                        strBuild = strBuild + "NULL" + " | ";
                    }
                }
                strBuild = strBuild.Remove(strBuild.Length - 2, 1);
            }
            return "(SkToolbox) → " + strBuild;
        }

        /// <summary>
        /// Used for logging to the console in a controlled manner
        /// </summary>
        /// <param name="message"></param>
        /// <param name="callerClass"></param>
        /// <param name="callerMethod"></param>
        public static void Logz(string message)
        {
            string strBuild = string.Empty;

            strBuild += " (OUT) → ";
            strBuild = $"{strBuild}{message} ";

            Debug.Log("(SkToolbox) → " + strBuild);
        }

        // GUI Items
        public static void RectFilled(float x, float y, float width, float height, Texture2D text)
        {
            GUI.DrawTexture(new Rect(x, y, width, height), text);
        }

        public static void RectOutlined(float x, float y, float width, float height, Texture2D text, float thickness = 1f)
        {
            RectFilled(x, y, thickness, height, text);
            RectFilled(x + width - thickness, y, thickness, height, text);
            RectFilled(x + thickness, y, width - thickness * 2f, thickness, text);
            RectFilled(x + thickness, y + height - thickness, width - thickness * 2f, thickness, text);
        }

        public static List<T> FindImplementationsByType<T>(Type type) // Compatible down to .net 3.5
        {
            List<T> list = new List<T>();
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < assemblies.Length; i++)
            {
                foreach (Type typeInfo in assemblies[i].GetTypes())
                {
                    if (typeInfo.IsClass && typeInfo.IsPublic && !typeInfo.IsAbstract && typeInfo.FullName != null)
                    {
                        Type typeFromHandle = Type.GetTypeFromHandle(typeInfo.TypeHandle);
                        if (type.IsAssignableFrom(typeFromHandle))
                        {
                            try
                            {
                                list.Add((T)((object)Activator.CreateInstance(typeFromHandle)));
                            }
                            catch (Exception rootCause)
                            {
                                throw new Exception("Failed to instantiate type '" + typeInfo.Name + "'.", rootCause);
                            }
                        }
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// <strong>Must call from OnGUI().</strong> Creates a box with a given texture on the UI.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="text"></param>
        /// <param name="thickness"></param>
        public static void Box(float x, float y, float width, float height, Texture2D text, float thickness = 1f)
        {
            RectOutlined(x - width / 2f, y - height, width, height, text, thickness);
        }
    }

    public static class Dot
    {
        private static class d
        {
            public static Texture2D lineTex = new Texture2D(1, 1);
            public static Vector2 vectorA = Vector2.zero;
            public static Rect rectA = new Rect(0, 0, 0, 0);
            public static Color bColor;
            public static float tOffset = 0f;
            public static Texture2D patternTexture = new Texture2D(1, 1);
            public static Vector2 Crosshair2dCenter_sw = new Vector2(Screen.width / 2f - 2f, Screen.height / 2f - 2f);
            public static Vector2 Crosshair2dCenter_ac = new Vector2(Screen.width / 2f - 1f, Screen.height / 2f - 1f);
            public static Vector2 Crosshair2dVector = Vector2.zero;
        }
        public static void DrawCenterCrosshair()
        {
            Dot.Draw(d.Crosshair2dCenter_sw, Color.black, 4f);
            Dot.Draw(d.Crosshair2dCenter_ac, Color.white, 2f);
        }
        public static void DrawVectorCrosshair(Vector3 vector)
        {
            if (vector.x == 0f && vector.y == 0f)
                return; // if [0,0] - return;
            d.Crosshair2dVector.x = vector.x - 2f;
            d.Crosshair2dVector.y = Screen.height - vector.y - 1f;
            Dot.Draw(d.Crosshair2dVector, Color.black, 4f);
            d.Crosshair2dVector.x += 1f;
            d.Crosshair2dVector.y += 1f;
            Dot.Draw(d.Crosshair2dVector, Color.white, 2f);
        }
        public static void Draw(Vector2 Position, Color color, float thickness)
        {
            if (!d.lineTex) { d.lineTex = d.patternTexture; }
            d.tOffset = Mathf.Ceil(thickness / 2f);
            d.bColor = GUI.color;
            d.rectA.x = Position.x;
            d.rectA.y = Position.y - d.tOffset;
            d.rectA.width = thickness;
            d.rectA.height = thickness;
            GUI.color = color;
            GUI.DrawTexture(d.rectA, d.lineTex);
            GUI.color = d.bColor;
        }
    }
    public static class Text
    {
        private static class d
        {
            public static Vector2 vectorA = Vector2.zero;
            public static GUIStyle bStyle;
            public static GUIContent bContent = new GUIContent();
            public static GUIStyle cStyle = new GUIStyle();
        }
        public static void Draw(Rect rect, string content, Color txtColor, bool shadow = true)
        {
            if (!shadow)
            {
                d.bStyle = d.cStyle;
                d.bStyle.normal.textColor = txtColor;
                GUI.Label(rect, content, d.bStyle);
                return;
            }
            d.bContent.text = content;
            d.vectorA.x = 1f;
            d.vectorA.y = 1f;
            DrawShadowed(rect, d.bContent, d.cStyle, txtColor, Color.black, d.vectorA);

        }
        public static void DrawShadowed(Rect rect, GUIContent content, GUIStyle style, Color txtColor, Color shadowColor, Vector2 direction)
        {
            d.bStyle = style;
            style.normal.textColor = shadowColor;
            rect.x += direction.x;
            rect.y += direction.y;
            GUI.Label(rect, content, style);
            style.normal.textColor = txtColor;
            rect.x -= direction.x;
            rect.y -= direction.y;
            GUI.Label(rect, content, style);
            style = d.bStyle;
        }
    }

    public static class Line
    {
        // temporal iCorpse used only in this scope to remove ram overuse and speed up code
        private static class d
        {
            public static Texture2D lineTex = new Texture2D(1, 1);
            public static Texture2D patternTexture = new Texture2D(1, 1);
            public static Matrix4x4 i_M4x4 = Matrix4x4.zero;
            public static Vector2 vectorA = Vector2.zero;
            public static Vector2 vectorB = Vector2.zero;
            public static float Angle = 0f;
            public static Color bColor;
            public static Rect rectA = new Rect(0, 0, 0, 0);
        }
        // add more overloads below if needed
        public static void Draw(Rect rect, Color color, float width)
        {
            d.vectorA.x = rect.x;
            d.vectorA.y = rect.y;
            d.vectorB.x = rect.x + rect.width;
            d.vectorB.y = rect.y + rect.height;
            Draw(d.vectorA, d.vectorB, color, width);
        }
        public static void Draw(Vector2 pointA, Vector2 pointB, Color color) => Draw(pointA, pointB, color, 1.0f);
        // main drawing function
        public static void Draw(Vector2 pointA, Vector2 pointB, Color color, float width)
        {
            d.i_M4x4 = GUI.matrix;
            if (!d.lineTex) { d.lineTex = d.patternTexture; }
            d.bColor = GUI.color;
            GUI.color = color;
            d.Angle = Vector3.Angle(pointB - pointA, Vector2.right);
            if (pointA.y > pointB.y) { d.Angle = -d.Angle; }
            d.vectorA.x = (pointB - pointA).magnitude;
            d.vectorA.y = width;
            d.vectorB.x = pointA.x;
            d.vectorB.y = pointA.y + 0.5f;
            GUIUtility.ScaleAroundPivot(d.vectorA, d.vectorB);
            GUIUtility.RotateAroundPivot(d.Angle, pointA);
            d.rectA.x = pointA.x;
            d.rectA.y = pointA.y;
            d.rectA.width = 1f;
            d.rectA.height = 1f;
            GUI.DrawTexture(d.rectA, d.lineTex);
            GUI.matrix = d.i_M4x4;
            GUI.color = d.bColor;
        }
    }

    public static class Box
    {
        public static void RectFilled(float x, float y, float width, float height, Texture2D text) => GUI.DrawTexture(new Rect(x, y, width, height), text);

        public static void RectOutlined(float x, float y, float width, float height, Texture2D text, float thickness = 1f)
        {
            RectFilled(x, y, thickness, height, text);
            RectFilled(x + width - thickness, y, thickness, height, text);
            RectFilled(x + thickness, y, width - thickness * 2f, thickness, text);
            RectFilled(x + thickness, y + height - thickness, width - thickness * 2f, thickness, text);
        }

        /// <summary>
        /// <strong>Must call from OnGUI().</strong> Creates a box with a given texture on the UI.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="text"></param>
        /// <param name="thickness"></param>
        public static void Draw(float x, float y, float width, float height, Texture2D text, float thickness = 1f)
        {
            RectOutlined(x - width / 2f, y - height, width, height, text, thickness);
        }
    }

    public static class Circle
    {
        private static class d
        {
            public static float[] cos = new float[] {
                    0.871147401032343f,
                    0.517795588650813f,
                    0.0310051616059934f,
                    -0.463775456747515f,
                    -0.839038729222366f,
                    -0.998077359907573f,
                    -0.899906267003044f,
                    -0.569824651437267f,
                    -0.0928962612844285f,
                    0.407971978270164f,
                    0.803703718412583f,
                    0.99231683272014f,
                    0.92520474123701f
                };
            public static float[] sin = new float[] {
                    -0.491021593898469f,
                    -0.855504370750821f,
                    -0.999519224404306f,
                    -0.88595277849253f,
                    -0.544071696437995f,
                    -0.0619805101619054f,
                    0.43608337575359f,
                    0.821766309004207f,
                    0.995675792936323f,
                    0.912994449570384f,
                    0.595029690864067f,
                    0.123722687896238f,
                    -0.37946829484498f
                };
            public static Vector2 vectorA = Vector2.zero;
            public static Vector2 vectorB = Vector2.zero;
        }

        public static void Draw(int X, int Y, float radius, float thickness = 1f) => Draw(X, Y, radius, Color.green, thickness);
        public static void Draw(int X, int Y, float radius, Color color) => Draw(X, Y, radius, color, 1f);
        public static void Draw(int X, int Y, float radius, Color color, float thickness = 1f)
        {
            for (int i = 0; i < 12; i++)
            {
                d.vectorA.x = X + d.cos[i] * radius;
                d.vectorA.y = Y + d.sin[i] * radius;
                d.vectorB.x = X + d.cos[i + 1] * radius;
                d.vectorB.y = Y + d.sin[i + 1] * radius;
                Line.Draw(d.vectorA, d.vectorB, color, thickness);
            }
        }
    }
}

