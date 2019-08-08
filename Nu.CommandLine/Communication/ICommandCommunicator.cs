using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace Nu.CommandLine.Communication
{
    public interface ICommandCommunicator
    {
        event Func<string, Dictionary<string, object>, string> ProcessCommandNamedArguments;

        event Func<string, List<object>, string> ProcessCommand;

        Func<List<string>> GetCommandsCallBack { get; set; }

        void Start();

        void Stop();

    }
}