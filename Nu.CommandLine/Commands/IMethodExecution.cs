using System.Collections.Generic;

namespace Nu.CommandLine.Commands
{
    public interface IMethodExecution
    {
        string Execute(object[] args);

        string[] ParameterNames { get; }

        string Execute(Dictionary<string, object> parameters);

        bool CanExecute(Dictionary<string, object> parameters, out object[] finalParams, out string error);

        bool CanExecute(object[] args, out object[] finalParams, out string error);
    }
}