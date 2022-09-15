using System;
using UnityEngine;

namespace SkToolbox
{
    public static class StandardCommands
    {
        [Command("quit", "Exit the application.", "  Base", false)]
        public static void ClearScreen()
        {
            Application.Quit();
        }

        [Command("conSetPos", "Set the position of the console. " +
            "TopCentered, LeftCentered, RightCentered, BottomCentered, " +
            "Centered, TopLeft, TopRight, BottomLeft, BottomRight", "  Base", false)]
        public static void ConSetPosition(string consolePosition)
        {
            Settings.Console.ConsolePos newPos = Settings.Console.Position;
            Enum.TryParse(consolePosition, out newPos);

            Settings.Console.Position = newPos;

            Logger.Submit("Position set to " + Settings.Console.Position.ToString());
        }

        [Command("conSetSize", 
            "Set the size of the console. Positive in pixels. " +
            "Negative numbers will divide the screen by the number given. " +
            "Ex. -2 will divide the screen in half, -3 in thirds, etc..", "  Base", false)]
        public static void ConSetSize(int Width, int Height)
        {
            
            Settings.Console.Width = Width;
            Settings.Console.Height = Height;

            Logger.Submit("Size set to " + Settings.Console.Width.ToString() + "x" + Settings.Console.Height);
        }

        [Command("conDisplay", "Enable/disable the view of the panel or console.", "  Base", false)]
        public static void ConDisplay(bool displayPanel = true, bool displayConsole = true)
        {

            Settings.Console.ShowConsole = displayConsole;

            if(!Settings.Console.ShowConsole) { displayPanel = true; } // if the console is hidden, we must show the panel
            Settings.Console.ShowPanel = displayPanel;

            Logger.Submit("Panel set: " + Settings.Console.ShowPanel + ", Console set: " + Settings.Console.ShowConsole);
        }

        [Command("OpenConsole", "Enables the console.", "  Base", false, 0)]
        public static void ConEnable()
        {
            Settings.Console.ShowConsole = true;

            Logger.Submit("Console set: " + Settings.Console.ShowConsole);
        }

        [Command("Time", "Sets or gets the timescale. [0.1f - 10.0f]", "  Base", false)]
        public static void SetTime(string setOrGet = "get", float timeScale = 1.0f)
        {
            if (setOrGet.Equals("set"))
            {
                timeScale = Mathf.Clamp(timeScale, 0.1f, 10.0f);
                Time.timeScale = timeScale;

                Logger.Submit("Timescale set: " + Time.timeScale);
            } else
            {
                Logger.Submit("Timescale is currently: " + Time.timeScale);
            }
        }
    }
}
