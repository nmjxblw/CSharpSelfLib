using StardewModdingAPI;

namespace Pathoschild.Stardew.Common.Commands;

internal abstract class BaseCommand : ICommand
{
	protected readonly IMonitor Monitor;

	public string Name { get; }

	public string Description => this.GetDescription();

	public abstract string GetDescription();

	public abstract void Handle(string[] args);

	protected BaseCommand(IMonitor monitor, string name)
	{
		this.Monitor = monitor;
		this.Name = name;
	}
}
