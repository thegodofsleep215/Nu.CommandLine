using Nu.CommandLine.Communication;

namespace Nu.CommandLine.Commands
{
    class CommandDelegateExecption : IMethodExecution
    {
        /// <summary>
        /// Method to execute.
        /// </summary>
        public CommandDelegate Method { get; set; }

        #region MethodExecution Members

        public string Execute(object[] args)
        {
            var command = (string)args[0];
            var parameters = (string[])args[1];
            return Method(command, parameters);
        }

        #endregion

        public CommandDelegateExecption(CommandDelegate method)
        {
            Method = method;
        }
    }
}