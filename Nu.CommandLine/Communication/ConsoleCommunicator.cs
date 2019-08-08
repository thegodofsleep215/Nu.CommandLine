using System;
using System.Collections.Generic;
using System.Linq;

namespace Nu.CommandLine.Communication
{
    public class ConsoleCommunicator : ICommandCommunicator
    {
        public event Func<string, List<object>, string> ProcessCommand;
        public event Func<string, Dictionary<string, object>, string> ProcessCommandNamedArguments;
        public Func<List<string>> GetCommandsCallBack { get; set; }
        public void Start()
        {
        }

        public void Stop()
        {
        }

        public string SendCommand(string command, Dictionary<string, string> parameters)
        {
            var dict = parameters.ToDictionary(kvp => kvp.Key, kvp => (object) kvp.Value);
            Func<string, Dictionary<string, object>, string> handler = ProcessCommandNamedArguments;
            if (handler != null) return handler(command, dict);
            return "Process Command Named arguments is not wired up.";
        }

        public string SendCommandOrderedParameters(string command, List<object> parameters)
        {
            if(ProcessCommand != null)
            {
                return ProcessCommand(command, parameters);
            }
            return "Process command ordered parameters is not wired up.";
        }

    }
}