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
    }
}
