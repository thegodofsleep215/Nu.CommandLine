using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Nu.CommandLine.Attributes;
using Nu.CommandLine.Commands;
using Nu.CommandLine.Utilies;

namespace Nu.CommandLine
{
    public class CommandContainer
    {

        #region Fields

        // Matches an add commmand, 'commandName (param1, param2, param2) some help text'
        private static readonly Regex CommandRegex = new Regex(@"^(?<name>\S+)\s*\(((?<params>[^\s,]+)[\s,]*)*\)\s*(?<help>.*)$");

        /// <summary>
        /// All commands.
        /// </summary>
        private readonly ConcurrentDictionary<string, Command> commands = new ConcurrentDictionary<string, Command>();

        #endregion


        /// <summary>
        /// Adds commands functions decorated with InteractiveCommandLineCommunicator.CommandAttribute
        /// </summary>
        /// <param name="commandObject"></param>
        public void RegisterObject(Object commandObject)
        {
            var typedMethodInfos = Reflection.GetMethodWithAttrbute<TypedCommandAttribute>(commandObject);
            foreach (var method in typedMethodInfos.Keys)
            {
                try
                {
                    if (!CheckReturnType(method))
                    {
                        continue;
                    }

                    foreach (var methodAttribute in typedMethodInfos[method])
                    {
                        methodAttribute.ResolveCommandName(method);
                        var usage = new Usage(GetMethodExectuion(methodAttribute.Command, method, commandObject), methodAttribute);
                        AddCommand(methodAttribute.Command, usage);
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Unable to register typed method '{method.Name}'", ex);
                }
            }
        }


        private bool CheckReturnType(MethodInfo method)
        {
            if (method.ReturnType.Name != "String")
            {
                throw new Exception($"Unable to register method '{method.Name}': doesn't match a return type.");
            }
            return true;
        }



        /// <summary>
        /// Adds a command  to commands.
        /// </summary>
        /// <param name="commandName"></param>
        /// <param name="usage"></param>
        public void AddCommand(string commandName, Usage usage)
        {
            if (!commands.ContainsKey(commandName))
            {
                commands.TryAdd(commandName, new Command(commandName, usage));
            }
            else
            {
                var matchingUsages = (from u in commands[commandName].Usages
                                      where u.NumberOfParams == usage.NumberOfParams
                                      select u);
                if (!matchingUsages.Any())
                {
                    commands[commandName].Usages.Add(usage);
                }
                else if (usage.Method is MethodInfoExecution)
                {
                    commands[commandName].Usages.Add(usage);
                }
            }
        }


        /// <summary>
        /// Removes a command  to commands.
        /// </summary>
        /// <param name="commandName"></param>
        public void RemoveCommand(string commandName)
        {
            Command garbage;
            commands.TryRemove(commandName, out garbage);
        }

        /// <summary>
        /// Checks the existence of commandName in commands.
        /// </summary>
        /// <param name="commandName"></param>
        /// <returns></returns>
        public bool HasCommand(string commandName)
        {
            return commands.ContainsKey(commandName);
        }


        public bool HasUsage(string commandName, Dictionary<string, object> parameters)
        {
            if (commands.TryGetValue(commandName, out Command com))
            {
                return com.Usages.Any(u => u.MatchesUsage(parameters.Keys.ToArray()));
            }
            return false;
        }

        internal bool HasUsageOrderedParameters(string commandName, List<object> parameters)
        {
            if (commands.TryGetValue(commandName, out Command com))
            {
                return com.Usages.Any(u => u.MatchesUsageOrderedParameters(parameters.ToArray()));
            }
            return false;
        }

        public IMethodExecution GetMethodExectuion(string commandName, MethodInfo method, Object commandObject)
        {
            Command com;
            if (commands.TryGetValue(commandName, out com))
            {
                foreach (var u in com.Usages)
                {
                    MethodInfoExecution mie;
                    if ((mie = u.Method as MethodInfoExecution) != null)
                    {
                        if (method == mie.Method)
                        {
                            return mie;
                        }
                    }
                }
            }
            return new MethodInfoExecution(method, commandObject);
        }


        public bool Invoke(string commandName, Dictionary<string, object> parameters, out string output)
        {
            Command com;
            output = "";
            if (commands.TryGetValue(commandName, out com))
            {
                var usage = com.Usages.First(u => u.MatchesUsage(parameters.Keys.ToArray()));

                object[] temp;
                string error;
                output = usage.Method.CanExecute(parameters, out temp, out error) ? usage.Method.Execute(temp) : error;
                return true;
            }
            return false;
        }

        public bool Invoke(string commandName, List<object> parameters, out string output)
        {
            output = "Bad command.";
            if (commands.TryGetValue(commandName, out Command com))
            {
                var usages = com.Usages.Where(u => u.MatchesUsageOrderedParameters(parameters.ToArray()));
                foreach (var usage in usages)
                {
                    if (usage.Method.CanExecute(parameters, out object[] castedParameters, out string error))
                    {
                        output = usage.Method.Execute(castedParameters);
                        return true;
                    }
                    output = error;
                }
            }
            return false;
        }



        /// <summary>
        /// Gets a list of all commands.
        /// </summary>
        /// <returns></returns>
        public List<string> GetCommands()
        {
            return commands.Keys.ToList();
        }

        /// <summary>
        /// Gets a specific command.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public Command GetCommand(string command)
        {
            Command result;
            commands.TryGetValue(command, out result);
            return result;
        }

    }
}