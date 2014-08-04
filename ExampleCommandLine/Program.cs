using Nu.CommandLine;
using Nu.CommandLine.Attributes;
using Nu.CommandLine.Communication;

namespace ExampleCommandLine
{
    class Program
    {
        static void Main(string[] args)
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
    }
}
