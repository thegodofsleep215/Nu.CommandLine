using System.Collections.Generic;

namespace Nu.CommandLine.Commands
{
    public interface IMethodExecution
    {
        /// <summary>
        /// This is the default method name. This can be over ridden  by the TypeCommandAttribute.
        /// </summary>
        string DefaultMethodName { get; }

        string Execute(object[] args);

        string[] AllParameterNames { get; }

        string[] RequiredParameterNames { get; }

        string[] OptionalParameterNames { get; }

        string Execute(Dictionary<string, object> parameters);

        bool CanExecute(Dictionary<string, object> parameters, out object[] castedParams, out string error);
    }
}