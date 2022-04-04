using SkToolbox.Utility;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SkToolbox.Commands
{
    public class CmdStdHelp : SkCommand
    {
        public override string Command => "help";

        public override string Description => "Show all commands or optionally search for commands.";

        public override SkCommandEnum.VisiblityFlag VisibilityFlag => SkCommandEnum.VisiblityFlag.Visible;

        public override bool Enabled => true;

        public override string[] Hints => new string[] { "Command" };

        public override void Execute(string[] args)
        {
            String hintList = String.Empty;
            if (args.Length > 0) // Search for specific commands
            {
                List<SkCommand> foundCmdList = SkCommandProcessor.Instance.CommandList.FindAll(cmd => cmd.Command.StartsWith(args[0].Trim(), StringComparison.InvariantCultureIgnoreCase));
                foreach (SkCommand foundCmd in foundCmdList)
                {
                    if (foundCmd != null && foundCmd.Enabled && !foundCmd.VisibilityFlag.HasFlag(SkCommandEnum.VisiblityFlag.FullHidden))
                    {
                        hintList = String.Empty;
                        if (foundCmd.Hints != null && foundCmd.Hints.Length > 0)
                        {
                            foreach (string hint in foundCmd.Hints)
                            {
                                hintList += "[" + hint + "] ";
                            }
                        }
                        SkUtilities.Logz(new string[] { "HELP" }, new string[] { foundCmd.Command + " " + hintList + "- " + foundCmd.Description });
                    }
                    else
                    {
                        SkUtilities.Logz(new string[] { "HELP" }, new string[] { "No similar command found." });
                    }
                }
            }
            else // Search for all commands
            {
                foreach (SkCommand cmd in SkCommandProcessor.Instance.CommandList)
                {
                    if (cmd != null && cmd.Enabled && !cmd.VisibilityFlag.HasFlag(SkCommandEnum.VisiblityFlag.Hidden) && !cmd.VisibilityFlag.HasFlag(SkCommandEnum.VisiblityFlag.FullHidden))
                    {
                        hintList = String.Empty;
                        if (cmd.Hints != null && cmd.Hints.Length > 0)
                        {
                            foreach (string hint in cmd.Hints)
                            {
                                hintList += "[" + hint + "] ";
                            }
                        }
                        SkUtilities.Logz(new string[] { "HELP" }, new string[] { cmd.Command + " " + hintList + "- " + cmd.Description });
                    }
                }
            }
        }
    }

    public class CmdStdCls : SkCommand
    {
        public override string Command => "cls";

        public override string Description => "Clear the console.";

        public override SkCommandEnum.VisiblityFlag VisibilityFlag => SkCommandEnum.VisiblityFlag.Visible;

        public override bool Enabled => true;

        public override string[] Hints => null;

        public override void Execute(string[] args)
        {
            SkConsole console;
            console = GameObject.FindObjectOfType<SkConsole>();
            if (console != null)
            {
                console.ClearConsole();
                return;
            }
            SkUtilities.Logz(new string[] { "CLS", "ERR" }, new string[] { "Could not clear the console." });
        }
    }

    public class CmdQuit : SkCommand
    {
        public override string Command => "quit";

        public override string Description => "Exit the game.";

        public override SkCommandEnum.VisiblityFlag VisibilityFlag => SkCommandEnum.VisiblityFlag.Visible;

        public override bool Enabled => true;

        public override string[] Hints => null;

        public override void Execute(string[] args)
        {
            SkUtilities.Logz(new string[] { "QUIT" }, new string[] { "Exiting the game now..." }, LogType.Error);
            Application.Quit();
        }
    }

    public class CmdStdDiscover : SkCommand
    {
        public override string Command => "discover";

        public override string Description => "Search for console commands.";

        public override SkCommandEnum.VisiblityFlag VisibilityFlag => SkCommandEnum.VisiblityFlag.Visible;

        public override bool Enabled => false;

        public override string[] Hints => null;

        public override void Execute(string[] args)
        {
            SkToolbox.SkCommandProcessor.Instance.DiscoverCommands();
        }
    }

    public class CmdStdConReloadToolbox : SkCommand
    {
        public override string Command => "creloadtoolbox";

        public override string Description => "Reload the toolbox.";

        public override SkCommandEnum.VisiblityFlag VisibilityFlag => SkCommandEnum.VisiblityFlag.Hidden;

        public override bool Enabled => true;

        public override string[] Hints => null;

        public override void Execute(string[] args)
        {
            Loaders.SkLoader.Reload();
        }
    }

    public class CmdStdConUnloadToolbox : SkCommand
    {
        public override string Command => "cunloadtoolbox";

        public override string Description => "Unload the toolbox.";

        public override SkCommandEnum.VisiblityFlag VisibilityFlag => SkCommandEnum.VisiblityFlag.Hidden;

        public override bool Enabled => true;

        public override string[] Hints => null;

        public override void Execute(string[] args)
        {
            Loaders.SkLoader.SelfDestruct();
        }
    }

    public class CmdStdConSetFontSize : SkCommand
    {
        public override string Command => "csetfontsize";

        public override string Description => "Set the size of the console font. No parameter provided will report current size.";

        public override SkCommandEnum.VisiblityFlag VisibilityFlag => SkCommandEnum.VisiblityFlag.Hidden;

        public override bool Enabled => true;

        public override string[] Hints => new string[] { "Font Size" };

        public override void Execute(string[] args)
        {
            SkConsole console;
            console = GameObject.FindObjectOfType<SkConsole>();
            if (console == null)
            {
                SkUtilities.Logz(new string[] { "CONSETFONTSIZE", "ERR" }, new string[] { "Couldn't find the console object." });
                return;
            }

            if (args.Length == 0)
            {
                SkUtilities.Logz(new string[] { "CONSETFONTSIZE" }, new string[] { "Current font size: " + console.logFontSize });
                return;
            }

            if (args.Length == 1)
            {
                if (int.TryParse(args[0], out int newValue))
                {
                    SkUtilities.Logz(new string[] { "CONSETFONTSIZE" }, new string[] { "Set console font size: " + console.logFontSize + " → " + newValue });
                    console.logFontSize = newValue;
                    return;
                }
            }
            else
            {
                SkUtilities.Logz(new string[] { "CONSETFONTSIZE", "ERR" }, new string[] { "ConSetFontSize only accepts one parameter." });
            }
        }
    }

    public class CmdStdTimescale : SkCommand
    {
        public override string Command => "timescale";

        public override string Description => "Set a new timescale. Use no parameter to check current timescale.";

        public override SkCommandEnum.VisiblityFlag VisibilityFlag => SkCommandEnum.VisiblityFlag.Visible;

        public override bool Enabled => true;

        public override string[] Hints => new string[] { "Value" };

        public override void Execute(string[] args)
        {
            if (args.Length == 0)
            {
                SkUtilities.Logz(new string[] { "TIMESCALE" }, new string[] { "Current timescale: " + Time.timeScale.ToString() });
                return;
            }

            if (args.Length == 1)
            {
                if (float.TryParse(args[0], out float newValue))
                {
                    SkUtilities.Logz(new string[] { "TIMESCALE" }, new string[] { "Set timescale: " + Time.timeScale.ToString() + " → " + newValue });
                    Time.timeScale = newValue;
                }
            }
            else
            {
                SkUtilities.Logz(new string[] { "TIMESCALE", "ERR" }, new string[] { "timescale [Value] - Timescale only accepts one parameter." });
            }
        }
    }

    public class CmdStdObjectVar : SkCommand
    {
        public override string Command => "objectvar";

        public override string Description => "Get/Set the value of a variable attached to a gameobject." +
                                                "\nNo component name parameter will report all components attached to this object." +
                                                "\nNo field name parameter provided will report all fields for this object/component." +
                                                "\nNo value parameter provided will report current value of that field.";

        public override SkCommandEnum.VisiblityFlag VisibilityFlag => SkCommandEnum.VisiblityFlag.Visible;

        public override bool Enabled => true;

        public override string[] Hints => new string[] { "Gameobject Name", "Component Name", "Field Name", "Value" };

        public override void Execute(string[] args)
        {
            if (args.Length > 0)
            {
                if (GameObject.Find(args[0]) == null)
                {
                    SkUtilities.Logz(new string[] { "OBJECTVAR", "ERR" }, new string[] { "Object not found. Check object name, character case matters." });
                    return;
                }
            }

            if (args.Length == 1)
            {
                SkUtilities.Logz(new string[] { "OBJECTVAR", "GET" }, new string[] { SkUtilities.GetAllComponentsOnGameobject(args[0]) });
                return;
            }
            else if (args.Length == 2)
            {
                SkUtilities.Logz(new string[] { "OBJECTVAR", "GET" }, new string[] { SkUtilities.GetAllProperiesOfObject(args[0], args[1]) });
                return;
            }
            else if (args.Length == 3)
            {
                SkUtilities.Logz(new string[] { "OBJECTVAR" }, new string[] { "Current value: " + SkUtilities.GameobjectGetPrivateField<object>(args[0], args[1], args[2]) });
                return;
            }
            else if (args.Length == 4)
            {
                SkUtilities.Logz(new string[] { "OBJECTVAR" }, new string[] { "Setting variable " + args[0] + "." + args[1] + "." + args[2] + " " + SkUtilities.GameobjectGetPrivateField<object>(args[0], args[1], args[2]) + " → " + args[3] });
                SkUtilities.GameobjectSetPrivateField(args[0], args[1], args[2], args[3]);
                return;
            }
            else
            {
                SkUtilities.Logz(new string[] { "OBJECTVAR" }, new string[] { "SetObjectVar [Gameobject Name] [Component Name] [Field Name] [Value]" });
            }
        }
    }
}
