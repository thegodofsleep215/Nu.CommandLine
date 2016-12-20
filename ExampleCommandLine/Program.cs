using System;
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
            //StartInteractive();
            StartConsole(args);
        }

        private static void StartConsole(string[] args)
        {
            var parsedArgs = ConsoleArguments.Parse(args);
            var console = new ConsoleCommunicator();
            var cp = new CommandProcessor(console);
            cp.RegisterObject(new ExampleCommands());
            cp.Start();
            Console.WriteLine(console.SendCommand(parsedArgs.UnnamedArguments[0], parsedArgs.NamedArguments));

        }

        private static void StartInteractive()
        {
            var cp = new CommandProcessor(new InteractiveCommandLineCommunicator("ic"));
            cp.RegisterObject(new ExampleCommands());
            cp.Start();
        }
    }

    class ExampleCommands
    {
        [TypedCommand("echo", "returns what is passed.")]
        public string Echo(string msg)
        {
            return msg;
        }

        [TypedCommand]
        public string roll(int sides)
        {
            var rand = new Random(Environment.TickCount);
            return rand.Next(1, sides + 1).ToString();
        }
    }
}
