using Nu.CommandLine;
using Nu.CommandLine.Attributes;
using Nu.ConsoleArguments;

namespace ExampleCommandLine
{
    class Program
    {
        static void Main(string[] args)
        {
            var parsedArgs = ConsoleArguments.Parse(args, namedDelimiter: '=');
            Console.WriteLine(CommandProcessor.Run(parsedArgs.UnnamedArguments[0], parsedArgs.NamedArguments));
        }
    }

    class ExampleCommands : IActionContainer
    {
        [TypedCommand("echo", "returns what is passed.")]
        public string Echo(string msg)
        {
            return msg;
        }

        [TypedCommand("echo", "returns what is passed.")]
        public string Echo(int a, int b)
        {
            return (a + b).ToString();
        }

        [TypedCommand]
        public string roll(int sides, int count = 1)
        {
            var rand = new Random(Environment.TickCount);
            return string.Join(", ", Enumerable.Range(0, count).Select(x => rand.Next(1, sides + 1).ToString()));
        }

    }
}
