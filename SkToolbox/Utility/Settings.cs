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
        public static int FontSize = 16;

        //
        public static string OutputPrefix = "SkToolbox → "; // "(SkToolbox)"
        public static Color OutputPrefixColor = new Color(255, 51, 51);
        public static int MaxOutputEntries = 999;
        public static bool ShowPanel = true;
        public static bool ShowConsole = true;

        // Console Controls
        public static KeyCode KeyToggleWindow = KeyCode.BackQuote;
        public static KeyCode KeyAutoComplete = KeyCode.Tab;

        //Console Sizes
        private static ConsolePos position = ConsolePos.TopCentered;
        public static ConsolePos Position
        {
            get
            {

                return position;
            }
            set
            {
                position = value;
                Logger.MainConsole.HandlePositioning();
            }
        }

        public static int Width = -2;   // Positive in pixels. Negative numbers will divide the screen by the number given. Ex. -2 will divide the screen in half, -3 in thirds, etc..
        public static int Height = -2;  // Positive in pixels. Negative numbers will divide the screen by the number given. Ex. -2 will divide the screen in half, -3 in thirds, etc..
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
