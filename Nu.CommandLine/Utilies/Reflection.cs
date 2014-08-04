using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Nu.CommandLine.Utilies
{
    internal static class Reflection
    {
        public static Dictionary<MethodInfo, T[]> GetMethodWithAttrbute<T>(Object reflectedObject) where T: Attribute, new()
        {
            var result = new Dictionary<MethodInfo, T[]>();
            Type t = reflectedObject.GetType();
            Type attType = typeof(T);
            MethodInfo[] methods = t.GetMethods();
            foreach (var method in methods)
            {
                object[] methodAttributres = method.GetCustomAttributes(attType, false);

                if (methodAttributres.Length <= 0)
                {
                    continue;
                }

                result.Add(method, methodAttributres.Cast<T>().ToArray());
            }
            return result;
        }
    }
}
