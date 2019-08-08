using System;
using System.Collections.Generic;

namespace Nu.CommandLine.Commands
{
    public class DelegateExecutionMethod :IMethodExecution
    {
        private readonly Func<string> callback;

        public DelegateExecutionMethod(Func<string> callback, string defaultMethodName)
        {
            this.callback = callback;
            DefaultMethodName = defaultMethodName;
            AllParameterNames = new string[] { };
            RequiredParameterNames = new string[] { };
            OptionalParameterNames = new string[] { };
        }

        public string DefaultMethodName { get; }

        public string Execute(object[] args)
        {
            return callback();
        }

        public string[] AllParameterNames { get; }
        public string[] RequiredParameterNames { get; }
        public string[] OptionalParameterNames { get; }

        public string Execute(Dictionary<string, object> parameters)
        {
            return callback();
        }

        public bool CanExecute(Dictionary<string, object> parameters, out object[] castedParams, out string error)
        {
            castedParams = new string[] { };
            error = "";
            return true;
        }

        public bool CanExecute(List<object> parameters, out object[] castedParameters, out string error)
        {
            castedParameters = new string[] { };
            error = "";
            return true;
        }
    }
}