using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Nu.ConsoleArguments
{
    public class ConsoleArguments
    {
        public string[] UnnamedArguments { get; protected set; }

        public Dictionary<string, string> NamedArguments { get; protected set; }

        public List<string> Flags { get; set; }

        public ConsoleArguments()
        {
            UnnamedArguments = new string[0];
            NamedArguments = new Dictionary<string, string>();
            Flags = new List<string>();
        }


        public static ConsoleArguments Parse(string[] args, char nameDenoter = '-', char namedDelimiter = ' ')
        {
            var parsedArgs = new ConsoleArguments();
            List<string> unnamed = new List<string>();
            var nameRegex = new Regex($"^{Regex.Escape(nameDenoter.ToString())}(?<name>\\w+)$"); 
            var nameWithValRegex = new Regex($"^{Regex.Escape(nameDenoter.ToString())}(?<name>\\w+){namedDelimiter}(?<value>.+)$");
            for (int i = 0; i < args.Length; i++)
            {
                var arg = args[i];
                Match match;
                if ((match = nameRegex.Match(arg)).Success)
                {
                    if (namedDelimiter == ' ' && i < args.Length - 1 && !nameRegex.IsMatch(args[i + 1]))
                    {
                        var name = match.Groups["name"].Value;
                        var value = CleanValue(args[i + 1]);
                        parsedArgs.NamedArguments[name] = value;
                        i++;
                    }
                    else
                    {
                        var flag = match.Groups["name"].Value;
                        parsedArgs.Flags.Add(flag);
                    }

                }
                else if((match = nameWithValRegex.Match(arg)).Success)
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
