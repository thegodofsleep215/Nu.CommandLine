using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Nu.ConsoleArguments
{
    public class ConsoleArguments
    {
        public string[] UnnamedArguments { get; protected set; }

        public Dictionary<string, string> NamedArguments { get; protected set; }

        public ConsoleArguments()
        {
            UnnamedArguments = new string[0];
            NamedArguments = new Dictionary<string, string>();
        }

        public static ConsoleArguments Parse(string[] args, char nameDenoter = '-', char namedDelimiter = ' ')
        {
            var parsedArgs = new ConsoleArguments();
            List<string> unnamed = new List<string>();
            var nameRegex = new Regex(@"^.(?<name>\w+)$"); // not putting nameDenoter in the regex so I don't have to worry about escaping it.
            var nameWithValRegex = new Regex($"^.(?<name>\\w+){namedDelimiter}(?<value>\\w*)$");
            for (int i = 0; i < args.Length; i++)
            {
                var arg = args[i];
                Match match;
                if ((match = nameRegex.Match(arg)).Success && arg.StartsWith(nameDenoter.ToString()))
                {
                    var name = match.Groups["name"].Value;
                    var value = CleanValue(args[i + 1]); 
                    parsedArgs.NamedArguments[name] = value;
                    i++;
                }
                else if((match = nameWithValRegex.Match(arg)).Success && arg.StartsWith(nameDenoter.ToString()))
                {
                    var name = match.Groups["name"].Value;
                    var value = CleanValue(match.Groups["value"].Value);
                    parsedArgs.NamedArguments[name] = value;
                }
                else
                {
                    unnamed.Add(arg);
                }
            }
            parsedArgs.UnnamedArguments = unnamed.ToArray();
            return parsedArgs;
        }

        private static string CleanValue(string value)
        {
            if (value.StartsWith("\"") && value.EndsWith("\""))
            {
                return value.Substring(1, value.Length - 2);
            }
            return value;
        }
    }
}
