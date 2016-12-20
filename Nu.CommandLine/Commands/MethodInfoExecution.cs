using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Nu.CommandLine.Commands
{
    internal class MethodInfoExecution : BaseMethodExecution
    {

        public MethodInfo Method { get; set; }

        public object CommandObject { get; set; }

        #region MethodExecution Members

        public override string Execute(object[] args)
        {
            return (string)Method.Invoke(CommandObject, args);
        }

        public override string Execute(Dictionary<string, object> parameters)
        {
            object[] args = ParameterNames.Select(name => parameters[name]).ToArray();
            return Execute(args);
        }


        #endregion

        public MethodInfoExecution(MethodInfo method, Object commandObject)
            :base(method)
        {
            Method = method;
            CommandObject = commandObject;
        }

    
    }
}