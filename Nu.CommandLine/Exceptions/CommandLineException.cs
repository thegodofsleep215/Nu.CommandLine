using System;

namespace Nu.CommandLine
{
    public class CommandLineException : Exception
    {
        public CommandLineException(string msg) : base(msg) { }
    }


}