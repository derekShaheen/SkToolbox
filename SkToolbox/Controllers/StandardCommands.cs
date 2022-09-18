using System;
using System.Collections.Generic;
using UnityEngine;
using static SkToolbox.Util;

namespace SkToolbox
{
    public static class StandardCommands
    {

        [Command("alias", "Create a shortcut or alternate name for a command, or sequence of commands.", "  Base", DisplayOptions.ConsoleOnly)]
        public static void Alias(string name, params string[] commandText)
        {
            if (name.Contains(" "))
            {
                Logger.Submit("An alias cannot contain spaces.", true);
                return;
            }

            if (Loaders.SkBepInExLoader.Console.GetCommandHandler().GetActions().ContainsKey(name))
            {
                Logger.Submit($"{name.WithColor(Color.white)} already exists.", true);
                return;
            }

            if (Loaders.SkBepInExLoader.Console.GetCommandHandler().GetAliases().ContainsKey(name))
                Loaders.SkBepInExLoader.Console.GetCommandHandler().GetAliases().Remove(name);

            string cmd = string.Join(" ", commandText);
            Loaders.SkBepInExLoader.Console.GetCommandHandler().GetAliases().Add(name, cmd);
            Loaders.SkBepInExLoader.Loader.SaveAliases();

            Logger.Submit($"Alias {name.WithColor(Color.yellow)} created for {cmd.WithColor(Color.yellow)}", true);
        }

        [Command("bind", "Bind a console command to a key. See the Unity documentation for KeyCode names.", "  Base", DisplayOptions.ConsoleOnly)]
        public static void Bind(string keyCode, params string[] commandText)
        {
            if (!Enum.TryParse(keyCode, true, out KeyCode result))
            {
                Logger.Submit($"Couldn't find a key code named {keyCode.WithColor(Color.white)}.", true);
                return;
            }

            if (Loaders.SkBepInExLoader.Loader.Binds.ContainsKey(result))
                Loaders.SkBepInExLoader.Loader.Binds.Remove(result);

            string cmd = string.Join(" ", commandText);
            Loaders.SkBepInExLoader.Loader.Binds.Add(result, cmd);

            Loaders.SkBepInExLoader.Loader.SaveBinds();

            Logger.Submit($"Bound {result.ToString().WithColor(Color.yellow)} to {cmd.WithColor(Color.yellow)}.", true);
        }

        [Command("unalias", "Remove an alias you've created.", "  Base", DisplayOptions.ConsoleOnly)]
        public static void Unalias(string alias)
        {
            if (Loaders.SkBepInExLoader.Console.GetCommandHandler().GetAliases().ContainsKey(alias))
            {
                Loaders.SkBepInExLoader.Console.GetCommandHandler().GetAliases().Remove(alias);
                Logger.Submit($"Alias {alias.WithColor(Color.cyan)} deleted.", true);

                return;
            }

            Logger.Submit($"No alias named {alias.WithColor(Color.white)} exists.", true);
        }

        [Command("unaliasall", "Removes all of your custom command aliases. Requires a true/1/yes as parameter to confirm you mean it.", "  Base", DisplayOptions.ConsoleOnly)]
        public static void UnaliasAll(bool confirm)
        {
            if (!confirm)
            {
                Logger.Submit("Your aliases are safe.", true);
                return;
            }

            Loaders.SkBepInExLoader.Console.GetCommandHandler().GetAliases().Clear();
            Loaders.SkBepInExLoader.Loader.SaveAliases();

            Logger.Submit("All of your aliases have been cleared.", true);
        }

        [Command("listaliases", "List all of your custom aliases, or check what a specific alias does.", "  Base", DisplayOptions.ConsoleOnly)]
        public static void ListAliases(string alias = null)
        {
            if (Loaders.SkBepInExLoader.Console.GetCommandHandler().GetAliases().Count == 0)
            {
                Logger.Submit($"You have no aliases currently set. Use {"/alias".WithColor(Color.white)} to add some.");
                return;
            }

            if (string.IsNullOrEmpty(alias))
            {
                foreach (var pair in Loaders.SkBepInExLoader.Console.GetCommandHandler().GetAliases())
                    Logger.Submit($"{pair.Key} = <color=cyan>{pair.Value}</color>");

                return;
            }

            if (!Loaders.SkBepInExLoader.Console.GetCommandHandler().GetAliases().TryGetValue(alias, out string cmd))
            {
                Logger.Submit($"The alias {alias.WithColor(Color.white)} does not exist.", true);
                return;
            }

            Logger.Submit($"{alias} = {cmd.WithColor(Color.yellow)}");
        }

        [Command("listbinds", "List all of your custom keybinds, or check what an individual keycode is bound to.", "  Base", DisplayOptions.ConsoleOnly)]
        public static void ListBinds(string keyCode = null)
        {
            if (Loaders.SkBepInExLoader.Loader.Binds.Count == 0)
            {
                Logger.Submit($"You have no keybinds currently set. Use the {"bind".WithColor(Color.white)} to add some.", true);
                return;
            }

            if (string.IsNullOrEmpty(keyCode))
            {
                foreach (var pair in Loaders.SkBepInExLoader.Loader.Binds)
                    Logger.Submit($"{pair.Key} = {pair.Value.WithColor(Color.yellow)}");

                return;
            }

            if (!Enum.TryParse(keyCode, true, out KeyCode result))
            {
                Logger.Submit($"Couldn't find a key code named {keyCode.WithColor(Color.white)}.", true);
                return;
            }

            if (!Loaders.SkBepInExLoader.Loader.Binds.TryGetValue(result, out string cmd))
            {
                Logger.Submit($"{keyCode.ToString().WithColor(Color.white)} is not bound to anything.", true);
                return;
            }

            Logger.Submit($"{result} = {cmd.WithColor(Color.yellow)}");
        }

        [Command("unbind", "Removes a custom keybind.", "  Base", DisplayOptions.ConsoleOnly)]
        public static void Unbind(string keyCode)
        {
            if (!Enum.TryParse(keyCode, true, out KeyCode result))
            {
                Logger.Submit($"Couldn't find a key code named {keyCode.WithColor(Color.white)}.", true);
                return;
            }

            if (!Loaders.SkBepInExLoader.Loader.Binds.ContainsKey(result))
            {
                Logger.Submit($"{result.ToString().WithColor(Color.white)} is not bound to anything.", true);
                return;
            }

            Loaders.SkBepInExLoader.Loader.Binds.Remove(result);
            Loaders.SkBepInExLoader.Loader.SaveBinds();

            Logger.Submit($"Unbound {result.ToString().WithColor(Color.cyan)}.", true);
        }

        [Command("unbindall", "Unbinds ALL of your custom keybinds. Requires a true/1/yes as parameter to confirm you mean it.", "  Base", DisplayOptions.ConsoleOnly)]
        public static void UnbindAll(bool confirm)
        {
            if (!confirm)
            {
                Logger.Submit("Your binds are safe.", true);
                return;
            }

            Loaders.SkBepInExLoader.Loader.Binds.Clear();
            Loaders.SkBepInExLoader.Loader.SaveBinds();

            Logger.Submit("All of your binds have been cleared.", true);
        }

        [Command("quit", "Exit the application.", "  Base", DisplayOptions.ConsoleOnly)]
        public static void ClearScreen()
        {
            Application.Quit();
        }

        [Command("echo", "Outputs a message to this console. Intended for use with aliases and key binds.", "  Base", DisplayOptions.ConsoleOnly)]
        public static void CmdEcho(string inputMessage)
        {
            Logger.Submit(inputMessage);
        }

        [Command("con_setpos", "Set the position of the console. " +
            "TopCentered, LeftCentered, RightCentered, BottomCentered, " +
            "Centered, TopLeft, TopRight, BottomLeft, BottomRight", "  Base", DisplayOptions.ConsoleOnly)]
        public static void ConSetPosition(string consolePosition)
        {
            Settings.Console.ConsolePos newPos = Settings.Console.Position;
            Enum.TryParse(consolePosition, out newPos);

            Settings.Console.Position = newPos;

            Logger.Submit("Position set to " + Settings.Console.Position.ToString());
        }

        [Command("con_setsize", 
            "Set the size of the console. Positive in pixels. " +
            "Negative numbers will divide the screen by the number given. " +
            "Ex. -2 will divide the screen in half, -3 in thirds, etc..", "  Base", DisplayOptions.ConsoleOnly)]
        public static void ConSetSize(int Width, int Height)
        {
            
            Settings.Console.Width = Width;
            Settings.Console.Height = Height;

            Logger.Submit("Size set to " + Settings.Console.Width.ToString() + "x" + Settings.Console.Height);
        }

        [Command("con_setfontsize", "Set the size of the font in the console. [10 - 24]", "  Base", DisplayOptions.ConsoleOnly)]
        public static void ConFontSetSize(int fontSize = 16)
        {
            fontSize = Mathf.Clamp(fontSize, 10, 24);
            Settings.Console.FontSize = fontSize;
            Loaders.SkBepInExLoader.Console.font = Font.CreateDynamicFontFromOSFont("Consolas", fontSize);
            Logger.Submit("Font size set to " + Settings.Console.FontSize);
        }

        [Command("con_settheme", "Set the theme of the console. Accepts hex values and some color names or clear (ex. #RRGGBBAA or blue).", "  Base", DisplayOptions.ConsoleOnly)]
        public static void ConSetTheme(string color = "grey", bool darkenBackground = false)
        {
            Color setColor;
            if (color.Equals("clear"))
            {
                setColor = Color.clear;
            }
            else
            {
                setColor = Settings.Console.Theme;
                ColorUtility.TryParseHtmlString(color, out setColor);
            }
            
            Settings.Console.Theme = setColor;

            Settings.Console.DarkenBackground = darkenBackground;

            Logger.Submit("Theme set to #" + ColorUtility.ToHtmlStringRGB(Settings.Console.Theme));
        }

        [Command("con_display", "Enable/disable the view of the panel or console.", "  Base", DisplayOptions.ConsoleOnly)]
        public static void ConDisplay(bool displayPanel = true, bool displayConsole = true)
        {

            Settings.Console.ShowConsole = displayConsole;

            if(!Settings.Console.ShowConsole) { displayPanel = true; } // if the console is hidden, we must show the panel
            Settings.Console.ShowPanel = displayPanel;

            Logger.Submit("Panel set: " + Settings.Console.ShowPanel + ", Console set: " + Settings.Console.ShowConsole);
        }

        [Command("OpenConsole", "Enables the console.", "  Base", DisplayOptions.ConsoleOnly, 0)]
        public static void ConEnable()
        {
            Settings.Console.ShowConsole = true;

            Logger.Submit("Console set: " + Settings.Console.ShowConsole);
        }

        [Command("time", "Sets or gets the timescale. [Set or Get] [0.1f - 10.0f]", "  Base", DisplayOptions.ConsoleOnly)]
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
