using System;
using System.Linq;
using Nu.CommandLine;
using Nu.CommandLine.Attributes;
using Nu.CommandLine.Communication;
using Nu.ConsoleArguments;

namespace ExampleCommandLine
{
    class Program
    {
        static void Main(string[] args)
        {
            StartInteractive();
            //StartConsole(args);
        }

        private static void StartConsole(string[] args)
        {
            var parsedArgs = ConsoleArguments.Parse(args, namedDelimiter:'=');
            var console = new ConsoleCommunicator();
            var cp = CommandProcessor.GenerateCommandProcessor(console, "help text");
            //var cp = new CommandProcessor(console);
            //cp.RegisterObject(new ExampleCommands());
            cp.Start();
            Console.WriteLine(console.SendCommand(parsedArgs.UnnamedArguments[0], parsedArgs.NamedArguments));

        }

        private static void StartInteractive()
        {
            var cp = CommandProcessor.GenerateCommandProcessor(new InteractiveCommandLineCommunicator("ic"));
            cp.Start();
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
        public string roll(int sides, int count=1)
        {
            var rand = new Random(Environment.TickCount);
            return string.Join(", ", Enumerable.Range(0, count).Select(x => rand.Next(1, sides + 1).ToString()));
        }

    }
}
