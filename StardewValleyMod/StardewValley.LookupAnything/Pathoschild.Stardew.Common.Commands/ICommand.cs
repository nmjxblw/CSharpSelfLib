namespace Pathoschild.Stardew.Common.Commands;

internal interface ICommand
{
	string Name { get; }

	string Description { get; }

	void Handle(string[] args);
}
