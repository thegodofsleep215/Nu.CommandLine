namespace Nu.CommandLine.Commands
{
    /// <summary>
    /// Command delegate used to store individual commands for InteractiveCommandLineCommunicator
    /// </summary>
    /// <param name="commandName"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public delegate string CommandDelegate(string commandName, string[] parameters);
}