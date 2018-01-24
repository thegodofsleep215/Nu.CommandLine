using System;
using System.Reflection;
using System.Text;
using Nu.CommandLine.Commands;

namespace Nu.CommandLine.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class TypedCommandAttribute : Attribute
    {   
        public string Command { get; set; }
        public string HelpText { get; set; }


        public TypedCommandAttribute(string command, string helpText)
        {
            Command = command;
            HelpText = helpText;
        }

        public TypedCommandAttribute()
        {
            
        }

        public void ResolveCommandName(MethodInfo method)
        {
            var name = string.IsNullOrEmpty(Command) ? method.Name : Command;
            Command = name;
        }
    }
}