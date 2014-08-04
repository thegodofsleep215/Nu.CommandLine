using Nu.CommandLine.Communication;

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
        /// Method to execute.
        /// </summary>
        public IMethodExecution Method { get; set; }

        public virtual bool MatchesUsage(params object[] args)
        {
            return true;
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

        public Usage(string usage, string desc, int nop, CommandDelegate method)
        {
            Use = usage;
            Desc = desc;
            NumberOfParams = nop;
            Method = new CommandDelegateExecption(method);
        }
    }
}