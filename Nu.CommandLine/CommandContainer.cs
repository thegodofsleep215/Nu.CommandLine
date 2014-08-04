using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Nu.CommandLine.Attributes;
using Nu.CommandLine.Commands;
using Nu.CommandLine.Communication;
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

        #region Static Methods

        /// <summary>
        /// Adds commands functions decorated with InteractiveCommandLineCommunicator.CommandAttribute
        /// </summary>
        /// <param name="commandObject"></param>
        public void RegisterObject(Object commandObject)
        {
            Dictionary<MethodInfo, CommandAttribute[]> methodInfos = Reflection.GetMethodWithAttrbute<CommandAttribute>(commandObject);
            foreach (var method in methodInfos.Keys)
            {
                try
                {
                    if (!CheckReturnType(method))
                    {
                        continue;
                    }

                    foreach (var methodAttribute in methodInfos[method])
                    {
                        var cDel = (CommandDelegate)Delegate.CreateDelegate(typeof(CommandDelegate), method);
                        AddCommand(commandObject.GetType().FullName, methodAttribute.Command, methodAttribute.CommandUsage, methodAttribute.NumberOfParameters, methodAttribute.HelpText, cDel);
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("Unable to register method '{0}'", method.Name), ex);
                }
            }

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
                        var usage = methodAttribute.GetUsage(method, GetMethodExectuion(methodAttribute.Command, method, commandObject));
                        AddCommand(commandObject.GetType().FullName, methodAttribute.Command, usage);
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("Unable to register typed method '{0}'", method.Name), ex);
                }
            }
        }

        private string GetTypedCommandUsage(MethodInfo method, string command)
        {
            var par = method.GetParameters();
            var sb = new StringBuilder();
            sb.Append(command + " ");
            foreach (var p in par)
            {
                var temp = p.ParameterType.ToString().Split('.');
                sb.Append(string.Format("<{0} {1}>, ", temp[temp.Length - 1], p.Name));
            }
            return sb.ToString().TrimEnd(',', ' ');
        }

        private bool CheckReturnType(MethodInfo method)
        {
            if (method.ReturnType.Name != "String")
            {
                throw new Exception(string.Format("Unable to register method '{0}': doesn't match a return type.", method.Name));
            }
            return true;
        }

        /// <summary>
        /// Adds a command.
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="command"></param>
        /// <param name="method"></param>
        public void AddCommand(string origin, string command, CommandDelegate method)
        {
            Match match = CommandRegex.Match(command);
            if (match.Success)
            {
                string name = match.Groups["name"].Value;
                var temp = (from Capture cap in match.Groups["params"].Captures select cap.Value).ToList();
                string help = match.Groups["help"].Value;

                var usage = new Usage(string.Join(" ", temp.ToArray()), help, temp.Count, method);
                AddCommand(origin, name, usage);
            }
        }

        /// <summary>
        /// Adds a command.
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="commandName"></param>
        /// <param name="commandUsage"></param>
        /// <param name="numberOfParameters"></param>
        /// <param name="help"></param>
        /// <param name="method"></param>
        public void AddCommand(string origin, string commandName, string commandUsage, int numberOfParameters, string help, CommandDelegate method)
        {
            IMethodExecution mex = GetMethodExectuion(commandName, method);
            var usage = new Usage(commandUsage, help, numberOfParameters, mex);
            AddCommand(origin, commandName, usage);
        }

        public void AddCommand(string origin, string commandName, string commandUsage, int numberOfParameters, string help, MethodInfo method, Object commandObject)
        {
            IMethodExecution mex = GetMethodExectuion(commandName, method, commandObject);
            var usage = new Usage(commandUsage, help, numberOfParameters, mex);
            AddCommand(origin, commandName, usage);
        }

        /// <summary>
        /// Adds a command  to commands.
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="commandName"></param>
        /// <param name="usage"></param>
        public void AddCommand(string origin, string commandName, Usage usage)
        {
            if (!commands.ContainsKey(commandName))
            {
                commands.TryAdd(commandName, new Command(origin, commandName, usage));
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
                else if (usage.Method is CommandDelegateExecption)
                {
                    if (commands[commandName].Usages.Any(u => u.Method != usage.Method))
                    {
                        throw new Exception("If two untyped commands have the same number of paramters, they must use the same method.");
                    }
                    commands[commandName].Usages.Add(usage);
                }
                else if (usage.Method is MethodInfoExecution)
                {
                    commands[commandName].Usages.Add(usage);
                }
            }
        }

        /// <summary>
        /// Adds a command  to commands.
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="commandName"></param>
        /// <param name="usage"></param>
        public void AddCommand(string origin, string commandName, Usage[] usage)
        {
            commands[commandName] = new Command(origin, commandName, usage);
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

        /// <summary>
        /// Checks to see if a usage with numberOfParameters exists in commandName for commands.
        /// </summary>
        /// <param name="commandName"></param>
        /// <param name="numberOfParameters"></param>
        /// <returns></returns>
        public bool HasUsage(string commandName, int numberOfParameters, params object[] args)
        {
            Command com;
            if (commands.TryGetValue(commandName, out com))
            {
                return (from u in com.Usages
                    where (u.NumberOfParams == numberOfParameters || u.NumberOfParams < 0) && u.MatchesUsage(args)
                    select u).Any();

            }
            return false;
        }

        public IMethodExecution GetMethodExectuion(string commandName, CommandDelegate method)
        {
            Command com;
            if (commands.TryGetValue(commandName, out com))
            {
                foreach (var u in com.Usages)
                {
                    CommandDelegateExecption cde;
                    if ((cde = u.Method as CommandDelegateExecption) != null)
                    {
                        if (method == cde.Method)
                        {
                            return cde;
                        }
                    }
                }
            }
            return new CommandDelegateExecption(method);
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

        /// <summary>
        /// Executes commandName.
        /// </summary>
        /// <param name="commandName"></param>
        /// <param name="parameters"></param>
        /// <param name="output"></param>
        /// <returns></returns>
        public bool Invoke(string commandName, object[] parameters, out string output)
        {
            Command com;
            output = "";
            if (commands.TryGetValue(commandName, out com))
            {
                var args = new object[0];
                List<IMethodExecution> method = (from u in commands[commandName].Usages
                    where u.NumberOfParams == parameters.Count() || u.NumberOfParams == -1
                    select u.Method).Distinct().ToList();
                string error;
                if (method.Count == 1)
                {
                    if (method[0] is CommandDelegateExecption)
                    {
                        var prmts = new string[parameters.Count()];
                        for (int i = 0; i < parameters.Count(); i++)
                        {
                            string temp;
                            if ((temp = parameters[i] as string) != null)
                            {
                                prmts[i] = temp;
                            }
                            else
                            {
                                output = "Cannot use non-string values in typeless method.";
                                return false;
                            }
                        }
                        output = method[0].Execute(new object[] { commandName, prmts });
                    }
                    else if (method[0] is MethodInfoExecution)
                    {
                        var m = method[0] as MethodInfoExecution;
                        output = m.CanExecute(parameters, out args, out error) ? m.Execute(args) : error;
                    }
                }
                else
                {
                    List<MethodInfoExecution> mexes = (from mex in method
                        where mex is MethodInfoExecution
                        select mex as MethodInfoExecution).ToList();

                    MethodInfoExecution valid = null;
                    foreach (var mif in mexes)
                    {
                        object[] temp;
                        if (mif.CanExecute(parameters, out temp, out error))
                        {
                            args = temp;
                            valid = mif;
                        }
                    }

                    output = valid != null ? valid.Execute(args) : "Could not find a method to execute command.";
                }
                return true;
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

        public List<string> GetCommands(string origin)
        {
            return commands.Where(kvp => kvp.Value.Origin == origin).Select(kvp => kvp.Key).ToList();
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

        #endregion
    }
}