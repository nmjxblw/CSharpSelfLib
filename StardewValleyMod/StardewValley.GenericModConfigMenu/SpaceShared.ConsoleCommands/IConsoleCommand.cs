using StardewModdingAPI;

namespace SpaceShared.ConsoleCommands;

internal interface IConsoleCommand
{
	string Name { get; }

	string Description { get; }

	void Handle(IMonitor monitor, string command, string[] args);
}
