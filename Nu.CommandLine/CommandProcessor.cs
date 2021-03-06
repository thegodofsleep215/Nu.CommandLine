using System;
using System.Collections.Generic;
using System.Linq;
using Nu.CommandLine.Attributes;
using Nu.CommandLine.Commands;
using Nu.CommandLine.Communication;

namespace Nu.CommandLine
{

    public class CommandProcessor
    {
        private readonly ICommandCommunicator communicator;

        private readonly CommandContainer commandContainer;

        private readonly string manual;

        public static CommandProcessor GenerateCommandProcessor(ICommandCommunicator communicator, string manual = null) {
            var cp = new CommandProcessor(communicator, manual);
            var t = typeof(IActionContainer);
            var actionContainers = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes()).Where(x => t.IsAssignableFrom(x)).Where(x => x.IsClass).ToList();
            var badContainers = actionContainers.Where(ac => ac.GetConstructor(new Type[] { }) == null).ToList();

            if (badContainers.Any())
            {
                throw new CommandLineException($"ActionContainers must have an empty public constructor. The following do not, {string.Join(", ", badContainers.Select(x => x.Name))}");
            }

            actionContainers.ForEach(x => cp.RegisterObject(Activator.CreateInstance(x.UnderlyingSystemType)));

            return cp;
        }

        public CommandProcessor(ICommandCommunicator communicator, string manual = null)
        {
            this.manual = manual;
            commandContainer = new CommandContainer();
            this.communicator = communicator ?? new InteractiveCommandLineCommunicator("cmd");
            this.communicator.ProcessCommandNamedArguments += ProcessCommandNamedParameters;
            this.communicator.ProcessCommand += ProcessCommandOrderedParameters;
            this.communicator.GetCommandsCallBack = commandContainer.GetCommands;
            RegisterObject(this);
            if (!string.IsNullOrEmpty(manual))
            {
                commandContainer.AddCommand("help", new Usage(new DelegateExecutionMethod(() => manual, "help")));
            }
            commandContainer.RegisterObject(communicator);
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


        public void Start()
        {
            communicator.Start();
        }

        public void Stop()
        {
            communicator.Stop();
        }


        public void RegisterObject(object obj)
        {
            commandContainer.RegisterObject(obj);
        }
    }


}