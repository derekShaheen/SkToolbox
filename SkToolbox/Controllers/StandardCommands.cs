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

        [Command("conSetPos", "Set the position of the console. Center, TopLeft, BottomRight, TopCentered, etc", "  Base", false)]
        public static void ConSetPosition(string consolePosition)
        {
            Settings.Console.ConsolePos newPos = Settings.Console.Position;
            Enum.TryParse(consolePosition, out newPos);

            Settings.Console.Position = newPos;

            Logger.Submit("Position set to " + Settings.Console.Position.ToString());
        }
    }
}
