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
        /// Denotes if the command is internal to InteractiveCommandLineCommunicator.InteractiveCommandLineCommunicator.
        /// </summary>
        public string Origin { get; set; }

        /// <summary>
        /// Constructor that takes in one usage.
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="cName"></param>
        /// <param name="usage"></param>
        public Command(string origin, string cName, Usage usage)
        {
            Usages = new List<Usage> {usage};
            CommandName = cName;
            Origin = origin;
        }

        /// <summary>
        /// Constructor that takes in an array of usages.
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="cName"></param>
        /// <param name="aUsages"></param>
        public Command(string origin, string cName, IEnumerable<Usage> aUsages)
        {
            CommandName = cName;
            Usages = new List<Usage>(aUsages);
            Origin = origin;
        }
    }
}