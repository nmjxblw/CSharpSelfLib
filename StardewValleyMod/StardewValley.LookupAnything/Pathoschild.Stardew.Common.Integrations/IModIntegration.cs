namespace Pathoschild.Stardew.Common.Integrations;

internal interface IModIntegration
{
	string Label { get; }

	bool IsLoaded { get; }
}
