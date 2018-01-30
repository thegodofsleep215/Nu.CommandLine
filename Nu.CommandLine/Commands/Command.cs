using System.Collections.Generic;

namespace Nu.CommandLine.Commands
{
    /// <summary>
    /// Used to store commands for InteractiveCommandLineCommunicator and their usages.
    /// </summary>
    public class Command
    {

        /// <summary>
        /// The name of the command.
        /// </summary>
        public string CommandName { get; private set; }

        /// <summary>
        /// A list of usages for the command.
        /// </summary>
        public List<Usage> Usages{get; set;}

        /// <summary>
        /// Constructor that takes in one usage.
        /// </summary>
        public Command(string cName, Usage usage)
        {
            Usages = new List<Usage> {usage};
            CommandName = cName;
        }

        /// <summary>
        /// Constructor that takes in an array of usages.
        /// </summary>
        public Command(string cName, IEnumerable<Usage> aUsages)
        {
            CommandName = cName;
            Usages = new List<Usage>(aUsages);
        }
    }
}