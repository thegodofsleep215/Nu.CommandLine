using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Nu.CommandLine.Commands;
using Nu.CommandLine.Communication;

namespace Nu.CommandLine
{
    public class CommandProcessor
    {
        private readonly ICommandCommunicator communicator;

        private readonly CommandContainer commandContainer;

        public CommandProcessor(ICommandCommunicator communicator = null)
        {
            commandContainer = new CommandContainer();
            this.communicator = communicator ?? new InteractiveCommandLineCommunicator("cmd");
            this.communicator.ProcessCommand += ProcessCommand;
            this.communicator.GetCommandsCallBack = commandContainer.GetCommands;
            commandContainer.AddCommand(GetType().FullName, "exit", new Usage("exit", "Exits the session of the shell.", 0, Exit));
            commandContainer.AddCommand(GetType().FullName, "help", new[] { new Usage("help", "Displays help for all commands", 0, Help), 
                new Usage("help <client_command>", "Dispalys the help for <command>.", 1, Help) });
            commandContainer.AddCommand(GetType().FullName, "list", new Usage("list", "Displays all commands.", 0, List));
            commandContainer.AddCommand(GetType().FullName, "list", new Usage("list <partial_command_name>|-i|-e", "If <partial_command_name> is provided then all commands containing it will be displayed. If -i is provded all internal commands will be displayed, and if -e is provided all external commands will be displayed.", 1, List));
            commandContainer.AddCommand(GetType().FullName, "list", new Usage("list -i|-e <partial_command_name>", "Lists all internal (-i) or external (-e) commands that containt <partial_command_name>", 2, List));

            commandContainer.RegisterObject(communicator);

        }

        string ProcessCommand(string command, List<object> parameters)
        {
            if (commandContainer.HasCommand(command))
            {
                if (commandContainer.HasUsage(command, parameters.Count))
                {
                    string output = commandContainer.Invoke(command, parameters.ToArray(), out output) ? output : "An unknown error occured while executing the command.";
                    return output;
                }
                return "Invalid number of parameters.";
            }
            return "Bad command.";
        }


        /// <summary>
        /// Removes a list of commands.
        /// </summary>
        /// <param name="commands"></param>
        public void RemoveCommands(string[] commands)
        {
            foreach (string com in commands)
            {
                RemoveCommand(com);
            }
        }       

        /// <summary>
        /// Removes a command.
        /// </summary>
        /// <param name="command"></param>
        public void RemoveCommand(string command)
        {
            commandContainer.RemoveCommand(command);
        }

        
        #region Client Command Methods

        private string Exit(string commandName, string[] parameters)
        {
            Stop();
            return "Shutting Down.";
        }

        private string Help(string commandName, string[] parameters)
        {
            string helptext = "";

            if (!parameters.Any())
            {
                helptext += "\n HELP FORMAT:\n";
                helptext += " <command>\n";
                helptext += "   <Num Of Param For Overload>: <Help Text>\n";
                helptext += "              -- OR --\n";
                helptext += "   <Calling Convention>: <Help Text>\n";

                helptext = commandContainer.GetCommands().Aggregate(helptext, (current, c) => current + PrintHelp(commandContainer.GetCommand(c), c));
            }
            else if (parameters.Count() == 1)
            {

                if (commandContainer.HasCommand(parameters[0]))
                {
                    helptext += "\n HELP FORMAT:\n";
                    helptext += " <command>\n";
                    helptext += "   <Num Of Param For Overload>: <Help Text>\n";
                    helptext += "              -- OR --\n";
                    helptext += "   <Calling Convention>: <Help Text>\n";

                    helptext += PrintHelp(commandContainer.GetCommand(parameters[0]), parameters[0]);
                }
                else
                    helptext = String.Format("Could not find command ({0}).", parameters[0]);
            }
            return helptext;
        }

        private string PrintHelp(Command cc, string c)
        {
            string text = String.Format("\n {0}\n", c);
            foreach (Usage usage in cc.Usages)
            {
                text += String.Format("   {0}: ", usage.Use);
                text += PrintHelpText(usage.Desc, "       ");
                text += "\n";
            }
            return text;
        }

        private string PrintHelpText(string help, string offset)
        {
            string text = "";
            string[] words = help.Split(' ');
            for (int i = 0; i < words.Count(); i++)
            {
                if (Console.CursorLeft + words[i].Length + 1 >= Console.WindowWidth)
                {
                    text += "\n" + offset;
                }
                text += words[i] + " ";
            }
            return text;
        }

        private string List(string commandName, string[] parameters)
        {
            if (parameters.Count() == 2)
            {
                return string.Join("\n", commandContainer.GetCommands().Where(name => name.ToUpper().Contains(parameters[1].ToUpper())));
            }
            if(parameters.Count() == 1)
            {
                return string.Join("\n", commandContainer.GetCommands().Where(name => name.ToUpper().Contains(parameters[0].ToUpper())));
            }
            return string.Join("\n", commandContainer.GetCommands());
        }
        
        #endregion

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