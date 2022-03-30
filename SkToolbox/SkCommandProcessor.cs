using SkToolbox.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SkToolbox
{
    public class SkCommandProcessor
    {
        private static SkCommandProcessor _instance;

        public static SkCommandProcessor Instance
        {
            get
            {
                if (_instance == null) _instance = new SkCommandProcessor();
                return _instance;
            }
        }

        public List<SkCommand> CommandList { get => commandList; set => commandList = value; }

        private List<SkCommand> commandList = new List<SkCommand>();

        public void AddCommand(SkCommand newCommand, bool sort = true)
        {
            if (CommandExists(newCommand))
            {
                Utility.SkUtilities.Logz(new string[] { "CONSOLE", "ADDCMD", "ERR" }, new string[] { "Command '" + newCommand.Command + "' already exists.", newCommand.GetType().ToString() });
                return;
            }

            if (newCommand.Command.Contains("\""))
            {
                Utility.SkUtilities.Logz(new string[] { "CONSOLE", "ADDCMD", "ERR" }, new string[] { "Command cannot contain a quote in the name.", newCommand.Command });
                return;
            }

            if (newCommand.Command.Contains(" "))
            {
                Utility.SkUtilities.Logz(new string[] { "CONSOLE", "ADDCMD", "ERR" }, new string[] { "Command cannot contain a space in the name.", newCommand.Command });
                return;
            }

            CommandList.Add(newCommand);
            if (sort)
            {
                CommandList.Sort((cmdA, cmdB) =>
                        cmdA.Command.CompareTo(cmdB.Command));
            }
        }

        public void SortCommands()
        {
            CommandList.Sort((cmdA, cmdB) =>
                    cmdA.Command.CompareTo(cmdB.Command));
        }

        public bool CommandExists(SkCommand command)
        {
            if (CommandList.Exists(cmd => cmd.Command == command.Command))
            {
                return true;
            }
            return false;
        }

        public void ExecuteCommand(string commandString)
        {
            if (string.IsNullOrEmpty(commandString))
            {
                return;
            }

            string[] commandsSpl = commandString.Split(';');
            foreach (string command in commandsSpl)
            {
                string commandTrimmed = command.Trim();
                if (!string.IsNullOrEmpty(commandTrimmed))
                {
                    string[] commandSpl = commandTrimmed.Split(' ');

                    SkCommand processCommand = SkCommandProcessor.Instance.CommandList.FirstOrDefault(cmd =>
                                                    cmd.Command.Equals(commandSpl[0], StringComparison.InvariantCultureIgnoreCase));

                    if (processCommand != null)
                    {
                        string[] paramStringArray = Regex.Matches(commandTrimmed, @"""[^""]+""|\S+")
                                                            .Cast<Match>()
                                                            .Select(p => p.Value.Trim('"'))
                                                            .Skip(1)
                                                            .ToArray();
                        try
                        {
                            processCommand.Execute(paramStringArray);
                        }
                        catch (Exception ex)
                        {
                            Utility.SkUtilities.Logz(new string[] { "CONTROLLER", "ERROR" }, new string[] { "Command executed but failed.", ex.Message });
                        }
                    }
                }
            }
        }

        public void DiscoverCommands()
        {
            List<Commands.SkCommand> foundCommands = Utility.SkUtilities.FindImplementationsByType<Commands.SkCommand>(typeof(Commands.SkCommand));
            foreach (Commands.SkCommand command in foundCommands)
            {
                if (command.Enabled)
                {
                    SkCommandProcessor.Instance.AddCommand(command, false);
                }
            }
            SkCommandProcessor.Instance.SortCommands();
        }
    }
}