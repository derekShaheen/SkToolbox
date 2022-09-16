using SkToolbox.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SkToolbox.Settings
{
    public class Console
    {
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
                Logger.MainConsole.HandlePositioning();
            }
        }

        public static bool ShowConsole
        {
            get { return SettingsController.Get<bool>("ConsoleEnabled"); }
            set
            {
                SettingsController.Set("ConsoleEnabled", value);
                Logger.MainConsole.HandlePositioning(-1, true);
            }
        }

        // Console Controls
        public static KeyCode KeyToggleWindow = KeyCode.BackQuote;
        public static KeyCode KeyAutoComplete = KeyCode.Tab;

        //Console Sizes
        public static ConsolePos Position
        {
            get { return SettingsController.Get<ConsolePos>("ConsolePosition"); }
            set
            {
                SettingsController.Set("ConsolePosition", value);
                Logger.MainConsole.HandlePositioning();
            }
        }
        // Positive in pixels. Negative numbers will divide the screen by the number given. Ex. -2 will divide the screen in half, -3 in thirds, etc..
        public static int Width 
        { 
            get { return SettingsController.Get<int>("ConsoleWidth"); }
            set 
            { 
                SettingsController.Set("ConsoleWidth", value);
                Logger.MainConsole.HandlePositioning();
            } 
        }
        // Positive in pixels. Negative numbers will divide the screen by the number given. Ex. -2 will divide the screen in half, -3 in thirds, etc..
        public static int Height
        {
            get { return SettingsController.Get<int>("ConsoleHeight"); }
            set 
            { 
                SettingsController.Set("ConsoleHeight", value);
                Logger.MainConsole.HandlePositioning();
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
    }
}
