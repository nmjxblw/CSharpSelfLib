using StardewModdingAPI;

namespace SpaceShared.ConsoleCommands;

internal abstract class ConsoleCommand : IConsoleCommand
{
	public string Name { get; }

	public string Description { get; }

	public abstract void Handle(IMonitor monitor, string command, string[] args);

	protected ConsoleCommand(string name, string description)
	{
		this.Name = name;
		this.Description = description;
	}

	protected void LogUsageError(IMonitor monitor, string error)
	{
		monitor.Log(error + " Type 'help " + this.Name + "' for usage.", (LogLevel)4);
	}
}
