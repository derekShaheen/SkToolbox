using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
/// <summary>
/// Based on the Gungnir code by Zambony. Accessed 9/11/22
/// https://github.com/zambony/Gungnir/
/// </summary>
namespace SkToolbox
{
    /// <summary>
    /// Attribute to be applied to a method for use by the command handler.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class Command : Attribute
    {
        public readonly string keyword;
        public readonly string description;
        //public readonly string autoCompleteTarget;
        public readonly string category;
        public readonly bool displayOnPanel;
        public readonly int sortPriority;

        public Command(string keyword, string description, string category = "zzzzz", bool displayOnPanel = true, int sortPriority = 100) //string autoComplete = null)
        {
            this.keyword = keyword;
            this.description = description;
            if (category == null)
            {
                category = string.Empty;
            }
            this.category = category;
            this.displayOnPanel = displayOnPanel;
            this.sortPriority = sortPriority;
            //this.autoCompleteTarget = autoComplete;
        }
    }

    /// <summary>
    /// Stores metadata information about a command, including a reference to the method in question.
    /// </summary>
    public class CommandMeta
    {
        //public delegate List<string> AutoCompleteDelegate();

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
        /// <summary>
        /// Delegate to return potential autocomplete topics.
        /// </summary>
        //public readonly AutoCompleteDelegate AutoComplete;

        public CommandMeta(Command data, MethodBase method, List<ParameterInfo> arguments)//, AutoCompleteDelegate autoCompleteDelegate = null)
        {
            this.data = data;
            this.method = method;
            this.arguments = arguments;
            //this.AutoComplete = autoCompleteDelegate;

            // If we have any arguments, attempt to build the argument hint string.
            if (arguments.Count > 0)
            {
                StringBuilder builder = new StringBuilder();

                foreach (ParameterInfo info in arguments)
                {
                    bool optional = info.HasDefaultValue;

                    requiredArguments += optional ? 0 : 1;

                    // Required parameters use chevrons, and optionals use brackets.
                    if (!optional)
                    {
                        builder.Append($"<{Util.GetSimpleTypeName(info.ParameterType)} {info.Name}> ");
                    }
                    else
                    {
                        string defaultValue = info.DefaultValue == null ? "none" : info.DefaultValue.ToString();
                        builder.Append($"[{Util.GetSimpleTypeName(info.ParameterType)} {info.Name}={defaultValue}] ");
                    }
                }

                // Remove trailing space.
                builder.Remove(builder.Length - 1, 1);

                hint = builder.ToString();
            }
            else
            {
                requiredArguments = 0;
            }
        }
    }

    /// <summary>
    /// This class is responsible for parsing and running commands. All commands are
    /// defined here as public methods and annotated with the <see cref="Command"/> attribute.
    /// </summary>
    public class CommandHandler
    {
        private Dictionary<string, CommandMeta> m_actions = new Dictionary<string, CommandMeta>();
        private Dictionary<string, string> m_aliases = new Dictionary<string, string>();

        private Controllers.MainConsole m_console = null;

        private const BindingFlags s_bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;

        internal Controllers.MainConsole Console { get => m_console; set => m_console = value; }
        public Dictionary<string, string> Aliases { get => m_aliases; set => m_aliases = value; }

        [Command("help", "Prints the command list, looks up the syntax of a specific command, or by partial command name.", "  Base")]
        public void Help(string command = null)
        {
            if (string.IsNullOrEmpty(command))
            {
                Console.Submit("Features" +
                    "\n  ▪ Auto-complete your command inputs by pressing tab" +
                    "\n  ▪ Pressing tab with empty input will populate the last input command with no parameters" +
                    "\n  ▪ Press the up and down arrow keys to cycle commands" +
                    "\n  ▪ Chain your commands by separating them with a semi-colon (;)");
                var cmds =
                    m_actions.Values.ToList()
                    .OrderBy(m => m.data.keyword);

                foreach (CommandMeta meta in cmds)
                {
                    string fullCommand = meta.data.keyword;

                    if (meta.arguments.Count > 0)
                        Console.Submit($"{fullCommand.WithColor(Color.yellow)} {meta.hint.WithColor(Color.cyan)}\n{meta.data.description}", false);
                    else
                        Console.Submit($"{fullCommand.WithColor(Color.yellow)}\n{meta.data.description}", false);
                }
            }
            else
            {
                var cmds = m_actions.Values.Where(cmd =>
                                            cmd.data.keyword.ToLower().StartsWith(command.ToLower())).ToList()
                                            .OrderBy(cmd => cmd.data.keyword);

                foreach (CommandMeta meta in cmds)
                {
                    string fullCommand = meta.data.keyword;

                    if (meta.arguments.Count > 0)
                        Console.Submit($"{fullCommand.WithColor(Color.yellow)} {meta.hint.WithColor(Color.cyan)}\n{meta.data.description}", false);
                    else
                        Console.Submit($"{fullCommand.WithColor(Color.yellow)}\n{meta.data.description}", false);
                }
            }
        }

        [Command("cls", "Clears the screen", "  Base")]
        public void ClearScreen()
        {
            Console.Clear();
        }

        [Command("cmdRegister", "Searches and registers all commands.", "  Base", false)]
        public void CmdRegister()
        {
            Logger.Submit("Searching and registering commands...");
            Register();
            Logger.Submit("Complete.");
        }

        public CommandHandler(Controllers.MainConsole mainConsole) : base()
        {
            Console = mainConsole;
            Register();
        }

        /// <summary>
        /// Helper method to create a <see cref="CommandMeta.AutoCompleteDelegate"/> function for autocomplete handlers.
        /// </summary>
        /// <param name="method">Name of the method attached to the <see cref="CommandHandler"/> class that will provide autocomplete values.</param>
        /// <returns>A <see cref="CommandMeta.AutoCompleteDelegate"/> if the <paramref name="method"/> exists, otherwise <see langword="null"/>.</returns>
        //private static CommandMeta.AutoCompleteDelegate MakeAutoCompleteDelegate(string method)
        //{
        //    if (string.IsNullOrEmpty(method))
        //        return null;

        //    var query =
        //        from assemblies in Assembly.GetExecutingAssembly().GetTypes()
        //        from methods in assemblies.GetMethods(s_bindingFlags)
        //        where methods.GetType().Name == method
        //        select methods;

        //    if (query.Count() > 0)
        //        return query.First().CreateDelegate(typeof(CommandMeta.AutoCompleteDelegate)) as CommandMeta.AutoCompleteDelegate;

        //    return null;
        //}

        /// <summary>
        /// Uses reflection to find all of the methods in this class annotated with the <see cref="Command"/> attribute
        /// and registers them for execution.
        /// </summary>
        public System.Collections.IEnumerator Register()
        {
            m_actions.Clear();
            try
            {
                Debug.Log($"{Settings.Console.OutputPrefix} Searching for commands...");

                IEnumerable<CommandMeta> query =
                    from assemblies in AppDomain.CurrentDomain.GetAssemblies()
                    from assembly in assemblies.GetTypes()
                    from method in assembly.GetMethods()
                    from attribute in method.GetCustomAttributes().OfType<Command>()
                    select new CommandMeta(
                        attribute,
                        method,
                        method.GetParameters().ToList()//,
                        //MakeAutoCompleteDelegate(attribute.autoCompleteTarget)
                    );

                Debug.Log($"{Settings.Console.OutputPrefix} Registering {query.Count()} commands...");

                // Sort commands by category, sort priority, then keyword
                foreach (CommandMeta command in query.OrderBy(m => m.data.category)
                                                     .ThenBy(n => n.data.sortPriority)
                                                     .ThenBy(o => o.data.keyword))
                {
                    try
                    {
                        m_actions.Add(command.data.keyword, command);
                    } catch (ArgumentException)
                    {
                        Debug.Log($"{Settings.Console.OutputPrefix} WARNING: Duplicate command found. Only adding the first instance of '{command.data.keyword}'!");
                        break;
                    }

                    string helpText;

                    if (command.arguments.Count > 0)
                        helpText = $"{command.hint} - {command.data.description}";
                    else
                        helpText = command.data.description;

                    //Debug.Log($"Registered command {command.data.keyword}");
                }

                Debug.Log($"{Settings.Console.OutputPrefix} Finished registering commands!");
            }
            catch (Exception ex)
            {
                Debug.LogError("Failed to register commands. \n\n" + ex.ToString());
            }
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
            string[] textSplit = text.Split(';');
            foreach (string line in textSplit)
            {
                // Remove garbage.
                string workLine = line.Simplified();

                // Split the text using our pattern. Splits by spaces but preserves quote groups.
                List<string> args = Util.SplitByQuotes(workLine);

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

                // Loop through each argument type of the command object
                // and attempt to convert the corresponding text value to that type.
                // We'll unpack the converted args list into the function call which will automatically
                // cast from object -> the parameter type.
                for (int i = 0; i < command.arguments.Count; ++i)
                {
                    // If there is a user supplied value, try to convert it.
                    if (i < args.Count)
                    {
                        Type argType = command.arguments[i].ParameterType;

                        string arg = args[i];

                        object converted = null;

                        try
                        {
                            if (command.arguments[i].GetCustomAttribute(typeof(ParamArrayAttribute)) != null)
                            {
                                argType = argType.GetElementType();
                                converted = Util.StringsToObjects(args.Skip(i).ToArray(), argType);
                            }
                            else
                            {
                                converted = Util.StringToObject(arg, argType);
                            }
                        }
                        catch (SystemException e)
                        {
                            Logger.Submit($"System error while converting <color=#EEEEEE>{arg}</color> to <color=#EEEEEE>{argType.Name}</color>: {e.Message}");
                            break;
                        }
                        catch (TooManyValuesException)
                        {
                            Logger.Submit($"Found more than one {Util.GetSimpleTypeName(argType)} with the text <color=#EEEEEE>{arg}</color>.");
                            break;
                        }
                        catch (NoMatchFoundException)
                        {
                            Logger.Submit($"Couldn't find a {Util.GetSimpleTypeName(argType)} with the text <color=#EEEEEE>{arg}</color>.");
                            break;
                        }

                        // Couldn't convert, oh well!
                        if (converted == null)
                        {
                            Logger.Submit($"Error while converting arguments for command <color=#EEEEEE>{commandName}</color>.");
                            break;
                        }

                        if (converted.GetType().IsArray)
                        {
                            object[] arr = converted as object[];
                            var things = Array.CreateInstance(argType, arr.Length);
                            Array.Copy(arr, things, arr.Length);
                            convertedArgs.Add(things);
                        }
                        else
                            convertedArgs.Add(converted);
                    }
                    // Otherwise, if we're still iterating, there's parameters they left unfilled.
                    // This will only execute if they are optional parameters, due to our required arg count check earlier.
                    else
                    {
                        // Since Invoke requires all parameters to be filled, we have to manually insert the function's default value.
                        // Very silly.
                        convertedArgs.Add(command.arguments[i].DefaultValue);
                    }
                }

                //Debug.Log("Running command " + command.data.keyword);
                // Invoke the method, which will expand all the arguments automagically.
                try
                {
                    command.method.Invoke(this, convertedArgs.ToArray());
                }
                catch (Exception)
                {
                    Debug.Log($"Something happened while running {command.data.keyword.WithColor(Color.white)}, check the BepInEx console for more details.");
                    throw;
                }
            }
        }

        public CommandMeta GetCommand(string commandName)
        {
            if (!m_actions.TryGetValue(commandName, out CommandMeta command))
                return null;

            return command;
        }

        public IEnumerable<KeyValuePair<string, CommandMeta>> GetPossibleCommands(string commandPartial)
        {
            return m_actions.Where(cmd => cmd.Value.data.keyword.ToLower().StartsWith(commandPartial.ToLower()
                        , StringComparison.InvariantCultureIgnoreCase));
        }

        public KeyValuePair<string, CommandMeta> GetLikelyCommand(string commandPartial, int skip = 0)
        {
            return m_actions.Where(cmd => cmd.Value.data.keyword.ToLower().StartsWith(commandPartial.ToLower()
                        , StringComparison.InvariantCultureIgnoreCase)).Skip(skip).FirstOrDefault();
        }

        public Dictionary<string, CommandMeta> GetAllCommands()
        {
            return m_actions;
        }
    }
}
