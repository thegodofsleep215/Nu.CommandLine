using System;

namespace Nu.CommandLine.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class CommandAttribute : Attribute
    {
        public string Command { get; set; }
        public int NumberOfParameters { get; set; }
        public string HelpText { get; set; }
        public string CommandUsage { get; set; }

        public CommandAttribute(string command, int nop, string helpText, string commandUsage)
        {
            Command = command;
            NumberOfParameters = nop;
            HelpText = helpText;
            CommandUsage = commandUsage;
        }

        public CommandAttribute()
        {
        }
    }
}