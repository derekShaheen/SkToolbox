using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.AccessControl;
using System.Text;
using UnityEngine;
namespace SkToolbox
{
    /// <summary>
    /// Attribute to be applied to a method for use by the command handler.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class Command : Attribute
    {
        public string Keyword { get; }
        public string Description { get; }
        public string Category { get; }
        public Util.DisplayOptions DisplayOptions { get; }
        public int SortPriority { get; }

        public Command(string keyword, string description, string category = "zzBottom", Util.DisplayOptions displayOptions = Util.DisplayOptions.All, int sortPriority = 100)
        {
            Keyword = keyword;
            Description = description;
            Category = category ?? string.Empty;
            DisplayOptions = displayOptions;
            SortPriority = sortPriority;
        }

        public Command(string keyword, string description, Util.DisplayOptions displayOptions = Util.DisplayOptions.All, int sortPriority = 100)
        {
            Keyword = keyword;
            Description = description;
            Category = "zzBottom";
            DisplayOptions = displayOptions;
            SortPriority = sortPriority;
        }

        public Command(string keyword, string description, string category, int sortPriority = 100)
        {
            Keyword = keyword;
            Description = description;
            Category = category ?? "zzBottom";
            DisplayOptions = Util.DisplayOptions.All;
            SortPriority = sortPriority;
        }

        public Command(string keyword, string description, string category)
        {
            Keyword = keyword;
            Description = description;
            Category = category ?? "zzBottom";
            DisplayOptions = Util.DisplayOptions.All;
            SortPriority = 100;
        }

        public Command(string keyword, string description)
        {
            Keyword = keyword;
            Description = description;
            Category = "zzBottom";
            DisplayOptions = Util.DisplayOptions.All;
            SortPriority = 100;
        }

        public override string ToString()
        {
            return $"Command: {Keyword} - {Description}";
        }

        public static string GetKeyword(MethodBase method)
        {
            return method.GetCustomAttributes(typeof(Command), false).Cast<Command>().FirstOrDefault()?.Keyword;
        }

        public static string GetDescription(MethodBase method)
        {
            return method.GetCustomAttributes(typeof(Command), false).Cast<Command>().FirstOrDefault()?.Description;
        }

        public static string GetCategory(MethodBase method)
        {
            return method.GetCustomAttributes(typeof(Command), false).Cast<Command>().FirstOrDefault()?.Category;
        }

        public static Util.DisplayOptions GetDisplayOptions(MethodBase method)
        {
            return method.GetCustomAttributes(typeof(Command), false).Cast<Command>().FirstOrDefault()?.DisplayOptions ?? Util.DisplayOptions.All;
        }

        public static int GetSortPriority(MethodBase method)
        {
            return method.GetCustomAttributes(typeof(Command), false).Cast<Command>().FirstOrDefault()?.SortPriority ?? 100;
        }

        public static bool IsCommand(MethodBase method)
        {
            return method.GetCustomAttributes(typeof(Command), false).Length > 0;
        }

        public static string GetHelpText(MethodBase method)
        {
            Command command = method.GetCustomAttributes(typeof(Command), false).Cast<Command>().FirstOrDefault();
            if (command == null)
                return string.Empty;
            string arguments = string.Empty;
            foreach (ParameterInfo info in method.GetParameters())
            {
                bool optional = info.HasDefaultValue;
                if (!optional)
                    arguments += $"<{Util.GetSimpleTypeName(info.ParameterType)} {info.Name}> ";
                else
                {
                    string defaultValue = info.DefaultValue == null ? "none" : info.DefaultValue.ToString();
                    arguments += $"[{Util.GetSimpleTypeName(info.ParameterType)} {info.Name}={defaultValue}] ";
                }
            }
            if (!string.IsNullOrEmpty(arguments))
                arguments = arguments.Substring(0, arguments.Length - 1);
            return $"{command.Keyword} {arguments} - {command.Description}";
        }
    }

    /// <summary>
    /// Stores metadata information about a command, including a reference to the method in question.
    /// </summary>
    public class CommandMeta
    {
        /// <summary>
        /// <see cref="Command"/> attribute data to access the command name and description.
        /// </summary>
        public readonly Command data;
        /// <summary>
        /// The actual command function.
        /// </summary>
        public readonly MethodBase method;
        /// <summary>
        /// A <see cref="List{ParameterInfo}"/> of argument information.
        /// </summary>
        public readonly List<ParameterInfo> arguments;
        /// <summary>
        /// A <see langword="string"/> representing argument types, names, and whether they are required
        /// parameters, e.g. <c>&lt;number amount&gt; [Player player]</c>
        /// </summary>
        public readonly string hint;
        /// <summary>
        /// Number of required arguments for the command to run.
        /// </summary>
        public readonly int requiredArguments;
        public bool isStandard = false;

        public CommandMeta(Command data, MethodBase method, List<ParameterInfo> arguments)
        {
            this.data = data;
            this.method = method;
            this.arguments = arguments;

            if (arguments.Count == 0)
            {
                requiredArguments = 0;
                return;
            }

            requiredArguments = 0;
            StringBuilder builder = new StringBuilder(arguments.Count);

            foreach (ParameterInfo info in arguments)
            {
                bool optional = info.HasDefaultValue;

                if (!optional)
                {
                    requiredArguments++;
                    builder.Append($"<{Util.GetSimpleTypeName(info.ParameterType)} {info.Name}> ");
                }
                else
                {
                    string defaultValue = info.DefaultValue == null ? "none" : info.DefaultValue.ToString();
                    builder.Append($"[{Util.GetSimpleTypeName(info.ParameterType)} {info.Name}={defaultValue}] ");
                }
            }

            hint = builder.ToString(0, builder.Length - 1);
        }

        public override string ToString()
        {
            return $"{data.Keyword} {hint}";
        }
    }

    /// <summary>
    /// This class is responsible for parsing and running commands. All commands are
    /// defined here as public methods and annotated with the <see cref="Command"/> attribute.
    /// </summary>
    public class CommandHandler
    {
        private Dictionary<string, CommandMeta> m_actions = new Dictionary<string, CommandMeta>(StringComparer.OrdinalIgnoreCase);
        private Dictionary<string, string> m_aliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        private Controllers.MainConsole m_console = null;

        private bool m_isSearching = false;
        public bool IsSearching { get => m_isSearching; }

        private const BindingFlags s_bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;

        internal Controllers.MainConsole Console { get => m_console; set => m_console = value; }
        public Dictionary<string, string> Aliases { get => m_aliases; set => m_aliases = value; }

        List<CommandMeta> query = new List<CommandMeta>();

        [Command("help", "Prints the command list, looks up the syntax of a specific command, or by partial command name.", "  Base")]
        public void Help(string command = null, bool displayDescriptions = true)
        {
            IReadOnlyCollection<CommandMeta> cmds = m_actions.Values
                .Where(cmd => string.IsNullOrEmpty(command) || cmd.data.Keyword.StartsWith(command, StringComparison.OrdinalIgnoreCase))
                .OrderBy(cmd => cmd.data.Keyword)
                .ToList();

            StringBuilder sb = new StringBuilder();

            foreach (CommandMeta meta in cmds)
            {
                string fullCommand = meta.data.Keyword;
                string hint = meta.arguments.Count > 0 ? $" {meta.hint.WithColor(Color.cyan)}" : "";

                //sb.Append(meta.isStandard ? "[Standard] " : "")
                    sb.Append(meta.isStandard ? fullCommand.WithColor(Color.blue + Color.yellow / 2f) : fullCommand.WithColor(Color.yellow))
                    .Append(hint);

                if (displayDescriptions && !string.IsNullOrEmpty(meta.data.Description))
                {
                    sb.AppendLine().Append(meta.data.Description);
                }

                Console.Submit(sb.ToString(), false);
                sb.Clear();
            }
        }

        [Command("cls", "Clears the screen", "  Base", Util.DisplayOptions.ConsoleOnly)]
        public void ClearScreen()
        {
            Console.Clear();
        }

        [Command("cmdRegister", "Searches and registers all commands.", "  Base", Util.DisplayOptions.ConsoleOnly)]
        public void CmdRegister()
        {
            //Logger.Submit("Searching and registering commands...");
            Register();
            //Logger.Submit("Complete.");
        }

        public CommandHandler(Controllers.MainConsole mainConsole) : base()
        {
            Console = mainConsole;
            Register();
        }

        /// <summary>
        /// Uses reflection to find all of the methods in this class annotated with the <see cref="Command"/> attribute
        /// and registers them for execution.
        /// </summary>
        public System.Collections.IEnumerator Register()
        {
            m_actions.Clear();

            Logger.Debug($"Searching for commands...");
            m_isSearching = true;

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                try
                {
                    var typesWithCommands = assembly.GetTypes()
                        .Where(type => type.GetMethods().Any(method => method.GetCustomAttribute<Command>() != null));
                    foreach (var type in typesWithCommands)
                    {
                        foreach (var method in type.GetMethods().Where(m => m.GetCustomAttribute<Command>() != null))
                        {
                            try
                            {
                                var commandAttribute = method.GetCustomAttribute<Command>();
                                var parameters = method.GetParameters().ToList();
                                var commandMeta = new CommandMeta(commandAttribute, method, parameters);

                                if (assembly == Assembly.GetExecutingAssembly())
                                {
                                    commandMeta.isStandard = true;
                                }
                                query.Add(commandMeta);
                            }
                            catch (Exception ex)
                            {
                                Logger.Debug("Failed to register a command.");
                                Debug.LogWarning(ex.ToString());
                            }
                        }
                    }
                }
                catch (Exception ex) when (!(ex is IndexOutOfRangeException))
                {
                    Logger.Submit($"An SkToolbox extension failed to load from assembly {assembly.FullName}. One or more commands may not appear! Verify your SkToolbox extensions are up to date or notify the author of the failed extension.");
                    Debug.LogWarning(ex.Message);
                }
                yield return null;
            }

            // Sort commands by category, sort priority, then keyword
            foreach (CommandMeta command in query.OrderBy(m => m.data.Category)
                                                 .ThenBy(n => n.data.SortPriority)
                                                 .ThenBy(o => o.data.Keyword))
            {
                try
                {
                    m_actions.Add(command.data.Keyword, command);
                }
                catch (ArgumentException)
                {
                    Logger.Debug($"WARNING: Duplicate command found. Only adding the first instance of '{command.data.Keyword}'!");
                    continue;
                }

                string helpText;

                if (command.arguments.Count > 0)
                    helpText = $"{command.hint} - {command.data.Description}";
                else
                    helpText = command.data.Description;

                //Debug.Log($"Registered command {command.data.keyword}");
            }
            Logger.Debug($"{query.Count()} commands have been registered!");
            m_isSearching = false;
            yield return null;
        }

        /// <summary>
        /// Attempts to run a command string that looks similar to <c>/cmd arg1 arg2 "arg with spaces"</c>.
        /// Will split arguments by spaces and quotation marks, attempt to convert each argument to the command's
        /// parameters, then run the command.
        /// </summary>
        /// <param name="text">Command string to evaluate.</param>
        public void Run(string text)
        {
            List<string> textSplit = text.SplitEscaped(';');

            foreach (string line in textSplit)
            {
                // Remove garbage.
                string workLine = line.Simplified();

                workLine = ReplaceAlias(workLine);

                string[] workLineSplit = new string[] { workLine };

                foreach (string distilledWorkLine in workLineSplit)
                {
                    // Split the text using our pattern. Splits by spaces but preserves quote groups.
                    List<string> args = Util.SplitByQuotes(distilledWorkLine);

                    // Store command ID and remove it from our arguments list.
                    string commandName = args[0];
                    args.RemoveAt(0);

                    // Look up the command value, fail if it doesn't exist.
                    if (!m_actions.TryGetValue(commandName, out CommandMeta command))
                    {
                        Console.Submit($"Unknown command <color=yellow>{commandName}</color>.");
                        continue;
                        //return;
                    }

                    if (args.Count < command.requiredArguments)
                    {
                        Console.Submit($"Missing required number of arguments for <color=yellow>{commandName}</color>.");
                        continue;
                        //return;
                    }

                    List<object> convertedArgs = new List<object>();

                    /// Iterates through the arguments of a command and converts them to their expected types.
                    /// If a required argument is missing, or an error occurs during conversion, logs an error message and returns.
                    /// If an argument has a default value, it is filled in automatically.
                    /// If an argument is marked with the ParamArray attribute, any remaining arguments are packed into an array of the expected type.
                    for (int i = 0; i < command.arguments.Count; ++i)
                    {
                        var argument = command.arguments[i];
                        var parameterType = argument.ParameterType;
                        var isParamArray = argument.GetCustomAttribute(typeof(ParamArrayAttribute)) != null;

                        if (i >= args.Count && !argument.HasDefaultValue)
                        {
                            Logger.Submit($"Missing required argument for command <color=#EEEEEE>{commandName}</color>");
                            return;
                        }

                        object convertedArg = null;

                        if (i < args.Count)
                        {
                            object arg;
                            arg = args[i];

                            try
                            {
                                convertedArg = isParamArray
                                    ? Util.StringsToObjects(args.Skip(i).ToArray(), parameterType.GetElementType())
                                    : Util.StringToObject(args[i], parameterType);
                            }
                            catch (SystemException e)
                            {
                                Logger.Submit($"System error while converting <color=#EEEEEE>{arg}</color> to <color=#EEEEEE>{parameterType.Name}</color>: {e.Message}");
                                return;
                            }
                            catch (TooManyValuesException)
                            {
                                Logger.Submit($"Found more than one {Util.GetSimpleTypeName(parameterType)} with the text <color=#EEEEEE>{arg}</color>.");
                                return;
                            }
                            catch (NoMatchFoundException)
                            {
                                Logger.Submit($"Couldn't find a {Util.GetSimpleTypeName(parameterType)} with the text <color=#EEEEEE>{arg}</color>.");
                                return;
                            }

                            if (isParamArray)
                            {
                                var elementType = parameterType.GetElementType();
                                var values = ((object[])convertedArg).Cast<object>().ToArray();
                                var newArray = Array.CreateInstance(elementType, values.Length);

                                for (int j = 0; j < values.Length; j++)
                                {
                                    newArray.SetValue(values[j], j);
                                }

                                convertedArg = newArray;
                            }
                        }
                        else
                        {
                            convertedArg = argument.DefaultValue;
                        }

                        convertedArgs.Add(convertedArg);
                    }

                    //Debug.Log("Running command " + command.data.keyword);
                    // Invoke the method, which will expand all the arguments automagically.
                    try
                    {
                        command.method.Invoke(this, convertedArgs.ToArray());
                    }
                    catch (Exception ex)
                    {
                        Logger.Debug(ex.Message);
                        Logger.Debug(ex.Source);
                        Logger.Debug(ex.StackTrace);
                        Logger.Submit($"Something happened while running {command.data.Keyword.WithColor(Color.white)}, check the BepInEx console for more details.");
                        throw;
                    }
                }
            }
        }

        public string GetAlias(string input)
        {
            if (m_aliases.TryGetValue(input, out string value))
                return value;

            return null;
        }

        public string ReplaceAlias(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            string[] split = input.Split();

            string alias = GetAlias(split[0]);

            if (!string.IsNullOrEmpty(alias))
            {
                split[0] = alias;

                return string.Join(" ", split);
            }

            return input;
        }

        public Dictionary<string, CommandMeta> GetActions()
        {
            return m_actions;
        }

        public Dictionary<string, string> GetAliases()
        {
            return m_aliases;
        }

        public CommandMeta GetCommand(string commandName)
        {
            if (!m_actions.TryGetValue(commandName, out CommandMeta command))
                return null;

            return command;
        }

        public IEnumerable<KeyValuePair<string, CommandMeta>> GetPossibleCommands(string commandPartial)
        {
            return m_actions.Where(cmd => cmd.Value.data.Keyword.ToLower().StartsWith(commandPartial.ToLower()
                        , StringComparison.InvariantCultureIgnoreCase));
        }

        public KeyValuePair<string, CommandMeta> GetLikelyCommand(string commandPartial, int skip = 0)
        {
            return m_actions.Where(cmd => cmd.Value.data.Keyword.ToLower().StartsWith(commandPartial.ToLower()
                        , StringComparison.InvariantCultureIgnoreCase)).Skip(skip).FirstOrDefault();
        }

        public IEnumerable<KeyValuePair<string, string>> GetPossibleAliasCommands(string commandPartial)
        {
            return m_aliases.Where(cmd => cmd.Value.ToLower().StartsWith(commandPartial.ToLower()
                        , StringComparison.InvariantCultureIgnoreCase));
        }

        public KeyValuePair<string, string> GetLikelyAliasCommand(string commandPartial, int skip = 0)
        {
            return m_aliases.Where(cmd => cmd.Value.ToLower().StartsWith(commandPartial.ToLower()
                        , StringComparison.InvariantCultureIgnoreCase)).Skip(skip).FirstOrDefault();
        }

        public Dictionary<string, CommandMeta> GetAllCommands()
        {
            return m_actions;
        }
    }
}
