using System;
using System.Collections.Generic;
using UnityEngine;

namespace SkToolbox
{
    public static class StandardCommands
    {
        private static Controllers.MainConsole m_Console = null;
        public static Controllers.MainConsole MainConsole
        {
            get
            {
                if (m_Console == null)
                {
                    m_Console = GameObject.FindObjectOfType<Controllers.MainConsole>();
                }
                return m_Console;
            }
            set => m_Console = value;
        }

        [Command("alias", "Create a shortcut or alternate name for a command, or sequence of commands.", "  Base", false)]
        public static void Alias(string name, params string[] commandText)
        {
            if (name.Contains(" "))
            {
                Logger.Submit("An alias cannot contain spaces.", true);
                return;
            }

            if (MainConsole.GetCommandHandler().GetActions().ContainsKey(name))
            {
                Logger.Submit($"{name.WithColor(Color.white)} already exists.", true);
                return;
            }

            if (MainConsole.GetCommandHandler().GetAliases().ContainsKey(name))
                MainConsole.GetCommandHandler().GetAliases().Remove(name);

            string cmd = string.Join(" ", commandText);
            MainConsole.GetCommandHandler().GetAliases().Add(name, cmd);
            MainConsole.SkBepInExLoader.SaveAliases();

            Logger.Submit($"Alias {name.WithColor(Color.yellow)} created for {cmd.WithColor(Color.yellow)}", true);
        }

        [Command("bind", "Bind a console command to a key. See the Unity documentation for KeyCode names.", "  Base", false)]
        public static void Bind(string keyCode, params string[] commandText)
        {
            if (!Enum.TryParse(keyCode, true, out KeyCode result))
            {
                Logger.Submit($"Couldn't find a key code named {keyCode.WithColor(Color.white)}.", true);
                return;
            }

            if (MainConsole.SkBepInExLoader.Binds.ContainsKey(result))
                MainConsole.SkBepInExLoader.Binds.Remove(result);

            string cmd = string.Join(" ", commandText);
            MainConsole.SkBepInExLoader.Binds.Add(result, cmd);

            MainConsole.SkBepInExLoader.SaveBinds();

            Logger.Submit($"Bound {result.ToString().WithColor(Color.yellow)} to {cmd.WithColor(Color.yellow)}.", true);
        }

        [Command("unalias", "Remove an alias you've created.", "  Base", false)]
        public static void Unalias(string alias)
        {
            if (MainConsole.GetCommandHandler().GetActions().ContainsKey(alias))
            {
                MainConsole.GetCommandHandler().GetActions().Remove(alias);
                Logger.Submit($"Alias {alias.WithColor(Color.cyan)} deleted.", true);

                return;
            }

            Logger.Submit($"No alias named {alias.WithColor(Color.white)} exists.", true);
        }

        [Command("unaliasall", "Removes all of your custom command aliases. Requires a true/1/yes as parameter to confirm you mean it.", "  Base", false)]
        public static void UnaliasAll(bool confirm)
        {
            if (!confirm)
            {
                Logger.Submit("Your aliases are safe.", true);
                return;
            }

            MainConsole.GetCommandHandler().GetActions().Clear();
            MainConsole.SkBepInExLoader.SaveAliases();

            Logger.Submit("All of your aliases have been cleared.", true);
        }

        [Command("listaliases", "List all of your custom aliases, or check what a specific alias does.", "  Base", false)]
        public static void ListAliases(string alias = null)
        {
            if (MainConsole.GetCommandHandler().GetAliases().Count == 0)
            {
                Logger.Submit($"You have no aliases currently set. Use {"/alias".WithColor(Color.white)} to add some.");
                return;
            }

            if (string.IsNullOrEmpty(alias))
            {
                foreach (var pair in MainConsole.GetCommandHandler().GetAliases())
                    Logger.Submit($"{pair.Key} = <color=cyan>{pair.Value}</color>");

                return;
            }

            if (!MainConsole.GetCommandHandler().GetAliases().TryGetValue(alias, out string cmd))
            {
                Logger.Submit($"The alias {alias.WithColor(Color.white)} does not exist.", true);
                return;
            }

            Logger.Submit($"{alias} = {cmd.WithColor(Color.yellow)}");
        }

        [Command("listbinds", "List all of your custom keybinds, or check what an individual keycode is bound to.", "  Base", false)]
        public static void ListBinds(string keyCode = null)
        {
            if (MainConsole.SkBepInExLoader.Binds.Count == 0)
            {
                Logger.Submit($"You have no keybinds currently set. Use the {"bind".WithColor(Color.white)} to add some.", true);
                return;
            }

            if (string.IsNullOrEmpty(keyCode))
            {
                foreach (var pair in MainConsole.SkBepInExLoader.Binds)
                    Logger.Submit($"{pair.Key} = {pair.Value.WithColor(Color.yellow)}");

                return;
            }

            if (!Enum.TryParse(keyCode, true, out KeyCode result))
            {
                Logger.Submit($"Couldn't find a key code named {keyCode.WithColor(Color.white)}.", true);
                return;
            }

            if (!MainConsole.SkBepInExLoader.Binds.TryGetValue(result, out string cmd))
            {
                Logger.Submit($"{keyCode.ToString().WithColor(Color.white)} is not bound to anything.", true);
                return;
            }

            Logger.Submit($"{result} = {cmd.WithColor(Color.yellow)}");
        }

        [Command("unbind", "Removes a custom keybind.", "  Base", false)]
        public static void Unbind(string keyCode)
        {
            if (!Enum.TryParse(keyCode, true, out KeyCode result))
            {
                Logger.Submit($"Couldn't find a key code named {keyCode.WithColor(Color.white)}.", true);
                return;
            }

            if (!MainConsole.SkBepInExLoader.Binds.ContainsKey(result))
            {
                Logger.Submit($"{result.ToString().WithColor(Color.white)} is not bound to anything.", true);
                return;
            }

            MainConsole.SkBepInExLoader.Binds.Remove(result);
            MainConsole.SkBepInExLoader.SaveBinds();

            Logger.Submit($"Unbound {result.ToString().WithColor(Color.cyan)}.", true);
        }

        [Command("unbindall", "Unbinds ALL of your Gungnir-related keybinds. Requires a true/1/yes as parameter to confirm you mean it.", "  Base", false)]
        public static void UnbindAll(bool confirm)
        {
            if (!confirm)
            {
                Logger.Submit("Your binds are safe.", true);
                return;
            }

            MainConsole.SkBepInExLoader.Binds.Clear();
            MainConsole.SkBepInExLoader.SaveBinds();

            Logger.Submit("All of your binds have been cleared.", true);
        }

        [Command("quit", "Exit the application.", "  Base", false)]
        public static void ClearScreen()
        {
            Application.Quit();
        }

        [Command("echo", "Outputs a message to this console. Intended for use with aliases and key binds.", "  Base", false)]
        public static void CmdEcho(string inputMessage)
        {
            Logger.Submit(inputMessage);
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

        [Command("conSetFontSize", "Set the size of the font in the console. [10 - 24]", "  Base", false)]
        public static void ConFontSetSize(int fontSize = 16)
        {

            Settings.Console.FontSize = fontSize;
            Logger.Submit("Font size set to " + Settings.Console.FontSize);
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

        [Command("Time", "Sets or gets the timescale. [Set or Get] [0.1f - 10.0f]", "  Base", false)]
        public static void SetTime(string setOrGet = "get", float timeScale = 1.0f)
        {
            if (setOrGet.ToLower().Equals("set"))
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
