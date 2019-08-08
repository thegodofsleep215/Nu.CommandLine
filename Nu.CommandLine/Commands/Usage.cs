using System.Linq;
using System.Text;
using Nu.CommandLine.Attributes;

namespace Nu.CommandLine.Commands
{
    /// <summary>
    /// Specifies a usage for a command
    /// </summary>
    public class Usage
    {
        /// <summary>
        /// Syntax.
        /// </summary>
        public string Use;

        /// <summary>
        /// Description/Help text.
        /// </summary>
        public string Desc;

        /// <summary>
        /// Number of parameters.
        /// </summary>
        public int NumberOfParams;

        /// <summary>
        /// Number of default parameters
        /// </summary>
        public int NumberOfDefaultParams;

        /// <summary>
        /// Method to execute.
        /// </summary>
        public IMethodExecution Method { get; set; }

        public virtual bool MatchesUsage(string[] parameterNames)
        {
            if (parameterNames.Length < Method.RequiredParameterNames.Length || 
                parameterNames.Length > Method.AllParameterNames.Length) return false;

            var req =  Method.RequiredParameterNames.All(parameterNames.Contains);

            var optional = parameterNames.Length == Method.RequiredParameterNames.Length || 
                           parameterNames.Where(x => !Method.RequiredParameterNames.Contains(x)) .All(x => Method.OptionalParameterNames.Contains(x));

            return req &&  optional;
        }

        public virtual bool MatchesUsageOrderedParameters(object[] parameters)
        {
            return !(parameters.Length < Method.RequiredParameterNames.Length ||
                parameters.Length > Method.AllParameterNames.Length);
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="usage"></param>
        /// <param name="desc"></param>
        /// <param name="nop"></param>
        /// <param name="method"></param>
        public Usage(string usage, string desc, int nop, IMethodExecution method)
        {
            Use = usage;
            Desc = desc;
            NumberOfParams = nop;
            Method = method;
        }

        public Usage(IMethodExecution methodExecution, TypedCommandAttribute commandAttribute = null)
        {
            var sb = new StringBuilder();
            var name = commandAttribute == null ? methodExecution.DefaultMethodName :
                                                    string.IsNullOrEmpty(commandAttribute.Command) ? methodExecution.DefaultMethodName : commandAttribute.Command;
            sb.Append($"{name} ");
            sb.Append(string.Join(" ", methodExecution.RequiredParameterNames));
            sb.Append(string.Join(" ", $"[{methodExecution.OptionalParameterNames}]"));

            string u = sb.ToString().TrimEnd(',', ' ');

            Use = u;
            Desc = "";
            NumberOfParams = methodExecution.AllParameterNames.Length;
            NumberOfDefaultParams = methodExecution.OptionalParameterNames.Length;
            Method = methodExecution;
        }
    }
}