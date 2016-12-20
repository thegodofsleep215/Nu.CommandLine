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

        public CommandProcessor(ICommandCommunicator communicator = null)
        {
            commandContainer = new CommandContainer();
            this.communicator = communicator ?? new InteractiveCommandLineCommunicator("cmd");
            this.communicator.ProcessCommand += ProcessCommand;
            this.communicator.ProcessCommandNamedArguments += ProcessCommandNamedArguments ;
            this.communicator.GetCommandsCallBack = commandContainer.GetCommands;
            RegisterObject(this);
            commandContainer.RegisterObject(communicator);

        }

        private string ProcessCommandNamedArguments(string command, Dictionary<string, object> parameters)
        {
            if (commandContainer.HasCommand(command))
            {
                if (commandContainer.HasUsage(command, parameters))
                {
                    string output = commandContainer.Invoke(command, parameters, out output) ? output : "An unknown error occurred while executing the command.";
                    return output;
                }
                return "Invalid parameters.";
            }
            return "Bad Command.";
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

        [TypedCommand("help", "Displays help for all commands.")]
        private string Help()
        {
            string helptext = "";

            helptext += "\n HELP FORMAT:\n";
            helptext += " <command>\n";
            helptext += "   <Num Of Param For Overload>: <Help Text>\n";
            helptext += "              -- OR --\n";
            helptext += "   <Calling Convention>: <Help Text>\n";

            helptext = commandContainer.GetCommands()
                .Aggregate(helptext, (current, c) => current + PrintHelp(commandContainer.GetCommand(c), c));

            return helptext;
        }

        [TypedCommand("help", "Displays help for <command>")]
        private string Help(string command)
        {
            string helptext = "";


            if (commandContainer.HasCommand(command))
            {
                helptext += "\n HELP FORMAT:\n";
                helptext += " <command>\n";
                helptext += "   <Num Of Param For Overload>: <Help Text>\n";
                helptext += "              -- OR --\n";
                helptext += "   <Calling Convention>: <Help Text>\n";

                helptext += PrintHelp(commandContainer.GetCommand(command), command);
            }
            else
                helptext = $"Could not find command ({command}).";
            return helptext;
        }

        private string PrintHelp(Command cc, string c)
        {
            string text = $"\n {c}\n";
            foreach (Usage usage in cc.Usages)
            {
                text += $"   {usage.Use}: ";
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

        [TypedCommand("list", "List all commands.")]
        private string List()
        {
            return string.Join("\n", commandContainer.GetCommands());
        }

        [TypedCommand("list", "List all commands matching <str>")]
        private string List(string str)
        {
                return string.Join("\n", commandContainer.GetCommands().Where(name => name.ToUpper().Contains(str.ToUpper())));
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