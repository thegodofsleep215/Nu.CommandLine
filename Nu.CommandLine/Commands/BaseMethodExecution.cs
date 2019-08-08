using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Nu.CommandLine.Commands
{
    public abstract class BaseMethodExecution : IMethodExecution
    {
        protected MethodInfo method;

        protected BaseMethodExecution(MethodInfo method)
        {
            DefaultMethodName = method.Name;
            this.method = method;

            var parameters = method.GetParameters();
            AllParameterNames = parameters.Select(x => x.Name).ToArray();
            RequiredParameterNames = parameters.Where(x => !x.IsOptional).Select(x => x.Name).ToArray();
            OptionalParameterNames = parameters.Where(x => x.IsOptional).Select(x => x.Name).ToArray();
        }

        public string DefaultMethodName { get; }

        public abstract string Execute(object[] args);

        public string[] AllParameterNames { get; }

        public string[] RequiredParameterNames { get; }

        public string[] OptionalParameterNames { get; }

        public abstract string Execute(Dictionary<string, object> parameters);


        public bool CanExecute(Dictionary<string, object> parameters, out object[] castedParams, out string error)
        {
            error = "";
            var mParams = method.GetParameters();
            castedParams = new object[mParams.Count()];

            bool result = true;
            for (int i = 0; i < mParams.Count() && result; i++)
            {
                ParameterInfo pInfo = mParams[i];
                if (pInfo.IsOptional)
                {
                    if (parameters.ContainsKey(pInfo.Name))
                    {
                        result = AddValue(pInfo, castedParams, i, ref error);
                    }
                    else
                    {
                        castedParams[i] = pInfo.DefaultValue;
                    }
                }
                else
                {
                    result = AddValue(pInfo, castedParams, i, ref error);
                }
            }

            return result;

            bool AddValue(ParameterInfo pInfo, object[] final, int i, ref string paramError)
            {
                var value = parameters[pInfo.Name];
                try
                {
                    if (pInfo.ParameterType.BaseType != null && pInfo.ParameterType.BaseType.Name == "Enum")
                    {
                        // Do not parse Enums if they were passed as ints.
                        if (int.TryParse((string)value, out _))
                        {
                            paramError = "Type Error: Cannot convert an interger type to an Enum, please use the string version of the enum.";
                            return false;
                        }
                        final[i] = Enum.Parse(pInfo.ParameterType, (string)value, true);
                    }
                    else if (pInfo.ParameterType.Name == value.GetType().Name + "&")
                    {
                        final[i] = value;
                    }
                    else
                    {
                        final[i] = Convert.ChangeType(value, pInfo.ParameterType);
                    }
                }
                catch
                {
                    paramError = $"Type Error: Cannot convert '{value}' to a(n) '{pInfo.ParameterType.Name}'";
                    return false;
                }
                return true;
            }
        }


        public bool CanExecute(List<object> parameters, out object[] castedParams, out string error)
        {
            error = "";
            var mParams = method.GetParameters();
            castedParams = new object[mParams.Count()];

            bool result = true;
            for (int i = 0; i < mParams.Count() && result; i++)
            {
                ParameterInfo pInfo = mParams[i];
                if (pInfo.IsOptional)
                {
                    if (i < parameters.Count)
                    {
                        result = AddValue(pInfo, castedParams, i, ref error);
                    }
                    else
                    {
                        castedParams[i] = pInfo.DefaultValue;
                    }
                }
                else
                {
                    result = AddValue(pInfo, castedParams, i, ref error);
                }
            }

            return result;

            bool AddValue(ParameterInfo pInfo, object[] final, int i, ref string paramError)
            {
                var value = parameters[i];
                try
                {
                    if (pInfo.ParameterType.BaseType != null && pInfo.ParameterType.BaseType.Name == "Enum")
                    {
                        // Do not parse Enums if they were passed as ints.
                        if (int.TryParse((string)value, out _))
                        {
                            paramError = "Type Error: Cannot convert an interger type to an Enum, please use the string version of the enum.";
                            return false;
                        }
                        final[i] = Enum.Parse(pInfo.ParameterType, (string)value, true);
                    }
                    else if (pInfo.ParameterType.Name == value.GetType().Name + "&")
                    {
                        final[i] = value;
                    }
                    else
                    {
                        final[i] = Convert.ChangeType(value, pInfo.ParameterType);
                    }
                }
                catch
                {
                    paramError = $"Type Error: Cannot convert '{value}' to a(n) '{pInfo.ParameterType.Name}'";
                    return false;
                }
                return true;
            }
        }
    }
}