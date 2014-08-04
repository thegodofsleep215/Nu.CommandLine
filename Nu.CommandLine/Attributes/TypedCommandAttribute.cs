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

        public Usage GetUsage(MethodInfo methodInfo, IMethodExecution methodExecution)
        {
            var par = methodInfo.GetParameters();
            var sb = new StringBuilder();
            sb.Append(Command + " ");
            foreach (var p in par)
            {
                var temp = p.ParameterType.ToString().Split('.');
                sb.Append(string.Format("<{0} {1}>, ", temp[temp.Length - 1], p.Name));
            }

            string u = sb.ToString().TrimEnd(',', ' ');

            var nop = methodInfo.GetParameters();
            return new Usage(u, HelpText, nop.Length, methodExecution);
        }

        public TypedCommandAttribute(string command, string helpText)
        {
            Command = command;
            HelpText = helpText;
        }

        public TypedCommandAttribute()
        {
        }
    }
}