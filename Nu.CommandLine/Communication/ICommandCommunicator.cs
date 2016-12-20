using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace Nu.CommandLine.Communication
{
    public interface ICommandCommunicator
    {
        event Func<string, List<object>, string> ProcessCommand;

        event Func<string, Dictionary<string, object>, string> ProcessCommandNamedArguments ;

        Func<List<string>> GetCommandsCallBack { get; set; }
        void Start();
        void Stop();

    }
}