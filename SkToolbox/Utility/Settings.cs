﻿using SkToolbox.Controllers;
using SkToolbox.Loaders;
using UnityEngine;

namespace SkToolbox.Settings
{
    public class Console
    {
        private static KeyCode consoleToggleKey = KeyCode.None;

        //
        public static string OutputPrefix = "SkToolbox → "; // "(SkToolbox)"
        public static Color OutputPrefixColor = new Color(255, 51, 51);
        public static int MaxOutputEntries
        {
            get { return SettingsController.Get<int>("ConsoleMaxOutput"); }
            set
            {
                SettingsController.Set("ConsoleMaxOutput", value);
            }
        }
        public static int FontSize
        {
            get { return SettingsController.Get<int>("ConsoleFontSize"); }
            set
            {
                value = Mathf.Clamp(value, 10, 24);
                SettingsController.Set("ConsoleFontSize", value);
            }
        }
        public static Color Theme
        {
            get { return SettingsController.Get<Color>("ConsoleTheme"); }
            set
            {
                SettingsController.Set("ConsoleTheme", value);
            }
        }

        public static bool ShowPanel
        {
            get { return SettingsController.Get<bool>("PanelEnabled"); }
            set
            {
                SettingsController.Set("PanelEnabled", value);
                SkBepInExLoader.Console.HandlePositioning();
            }
        }

        public static bool ShowConsole
        {
            get { return SettingsController.Get<bool>("ConsoleEnabled"); }
            set
            {
                SettingsController.Set("ConsoleEnabled", value);
                SkBepInExLoader.Console.HandlePositioning(-1, true);
            }
        }
        public static bool DarkenBackground
        {
            get { return SettingsController.Get<bool>("ConsoleDarkenBackground"); }
            set
            {
                SettingsController.Set("ConsoleDarkenBackground", value);
            }
        }

        // Console Controls
        public static KeyCode KeyToggleWindow
        {
            get 
            { 
                if(consoleToggleKey == KeyCode.None)
                {
                    var configKey = SettingsController.Get<string>("DisplayToggleKey");
                    consoleToggleKey = (KeyCode) System.Enum.Parse(typeof(KeyCode), configKey);
                }
                return consoleToggleKey; 
            }
            set
            {
                SettingsController.Set("DisplayToggleKey", value);
                consoleToggleKey = value;
            }
        }

        public static KeyCode KeyAutoComplete = KeyCode.Tab;

        //Console Sizes
        public static ConsolePos Position
        {
            get { return SettingsController.Get<ConsolePos>("ConsolePosition"); }
            set
            {
                SettingsController.Set("ConsolePosition", value);
                SkBepInExLoader.Console.HandlePositioning();
            }
        }
        // Positive in pixels. Negative numbers will divide the screen by the number given. Ex. -2 will divide the screen in half, -3 in thirds, etc..
        public static int Width 
        { 
            get { return SettingsController.Get<int>("ConsoleWidth"); }
            set 
            { 
                SettingsController.Set("ConsoleWidth", value);
                SkBepInExLoader.Console.HandlePositioning();
            } 
        }
        // Positive in pixels. Negative numbers will divide the screen by the number given. Ex. -2 will divide the screen in half, -3 in thirds, etc..
        public static int Height
        {
            get { return SettingsController.Get<int>("ConsoleHeight"); }
            set 
            { 
                SettingsController.Set("ConsoleHeight", value);
                SkBepInExLoader.Console.HandlePositioning();
            }
        }
        public static int Margin = 15;  // Default 15


        public enum ConsolePos
        {
            TopCentered = 0,
            LeftCentered = 1,
            RightCentered = 2,
            BottomCentered = 3,
            Centered = 4,
            TopLeft = 5,
            TopRight = 6,
            BottomLeft = 7,
            BottomRight = 8,
        }

        public static void SetDefault(string key)
        {
            switch (key)
            {
                case "ConsoleMaxOutput":
                    MaxOutputEntries = 999;
                    break;
                case "ConsoleFontSize":
                    FontSize = 16;
                    break;
                case "ConsoleTheme":
                    Theme = Color.grey;
                    break;
                case "ConsoleDarkenBackground":
                    DarkenBackground = false;
                    break;
                case "ConsolePosition":
                    Position = ConsolePos.TopCentered;
                    break;
                case "ConsoleWidth":
                    Width = -1;
                    break;
                case "ConsoleHeight":
                    Height = -2;
                    break;
                case "DisplayToggleKey":
                    KeyToggleWindow = KeyCode.BackQuote;
                    break;
                case "PanelEnabled":
                    ShowPanel = true;
                    break;
                case "ConsoleEnabled":
                    ShowConsole = true;
                    break;
            }
        }

        public static void SetDefaultAll()
        {
            SetDefault("ConsoleMaxOutput");
            SetDefault("ConsoleFontSize");
            SetDefault("ConsoleTheme");
            SetDefault("ConsoleDarkenBackground");
            SetDefault("ConsolePosition");
            SetDefault("ConsoleWidth");
            SetDefault("ConsoleHeight");
            SetDefault("DisplayToggleKey");
            SetDefault("PanelEnabled");
            SetDefault("ConsoleEnabled");
        }
    }
}
