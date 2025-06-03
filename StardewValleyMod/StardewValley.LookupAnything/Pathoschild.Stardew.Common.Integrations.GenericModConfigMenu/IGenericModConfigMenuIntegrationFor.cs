using StardewModdingAPI;

namespace Pathoschild.Stardew.Common.Integrations.GenericModConfigMenu;

internal interface IGenericModConfigMenuIntegrationFor<TConfig> where TConfig : class, new()
{
	void Register(GenericModConfigMenuIntegration<TConfig> menu, IMonitor monitor);
}
