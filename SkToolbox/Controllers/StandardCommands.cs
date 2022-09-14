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

        [Command("conSetPos", "Set the position of the console. TopCentered, LeftCentered, RightCentered, BottomCentered, Centered, TopLeft, TopRight, BottomLeft, BottomRight", "  Base", false)]
        public static void ConSetPosition(string consolePosition)
        {
            Settings.Console.ConsolePos newPos = Settings.Console.Position;
            Enum.TryParse(consolePosition, out newPos);

            Settings.Console.Position = newPos;

            Logger.Submit("Position set to " + Settings.Console.Position.ToString());
        }

        [Command("conSetSize", "Set the size of the console. Positive in pixels. Negative numbers will divide the screen by the number given. Ex. -2 will divide the screen in half, -3 in thirds, etc..", "  Base", false)]
        public static void ConSetSize(int Width, int Height)
        {
            
            Settings.Console.Width = Width;
            Settings.Console.Height = Height;

            Logger.Submit("Size set to " + Settings.Console.Width.ToString() + "x" + Settings.Console.Height);
        }
    }
}
