using System;
using System.Collections.Generic;
using System.Linq;
using Nu.CommandLine.Commands;

namespace Nu.CommandLine
{
    public class CommandProcessor
    {
        private readonly CommandContainer commandContainer;

        private readonly string manual;

        public static string Run(string command, Dictionary<string, string> arguments)
        {
            var cp = new CommandProcessor();

            var t = typeof(IActionContainer);
            var actionContainers = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes()).Where(x => t.IsAssignableFrom(x)).Where(x => x.IsClass).ToList();
            var badContainers = actionContainers.Where(ac => ac.GetConstructor(new Type[] { }) == null).ToList();

            if (badContainers.Any())
            {
                throw new CommandLineException($"ActionContainers must have an empty public constructor. The following do not, {string.Join(", ", badContainers.Select(x => x.Name))}");
            }

            actionContainers.ForEach(x => cp.RegisterObject(Activator.CreateInstance(x.UnderlyingSystemType)));


            var dict = arguments.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value);
            return cp.ProcessCommandNamedParameters(command, dict);
        }


        public CommandProcessor(string manual = null)
        {
            this.manual = manual;
            commandContainer = new CommandContainer();
            RegisterObject(this);
            if (!string.IsNullOrEmpty(manual))
            {
                commandContainer.AddCommand("help", new Usage(new DelegateExecutionMethod(() => manual, "help")));
            }
        }

        private string ProcessCommandOrderedParameters(string command, List<object> parameters)
        {
            if (commandContainer.HasCommand(command))
            {
                if (commandContainer.HasUsageOrderedParameters(command, parameters))
                {
                    return commandContainer.Invoke(command, parameters, out string output) ? output : "An unkown error occurrerd while executing the commnad.";
                }
                return string.IsNullOrEmpty(manual) ? "Invalid Parameters." : manual;
            }
            return string.IsNullOrEmpty(manual) ? "Bad command." : manual;
        }

        private string ProcessCommandNamedParameters(string command, Dictionary<string, object> parameters)
        {
            if (commandContainer.HasCommand(command))
            {
                if (commandContainer.HasUsage(command, parameters))
                {
                    string output = commandContainer.Invoke(command, parameters, out output) ? output : "An unknown error occurred while executing the command.";
                    return output;
                }
                return string.IsNullOrEmpty(manual) ? "Invalid Parameters." : manual;
            }
            return string.IsNullOrEmpty(manual) ? "Bad Command." : manual;
        }


        public void RegisterObject(object obj)
        {
            commandContainer.RegisterObject(obj);
        }
    }


}