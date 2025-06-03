using StardewModdingAPI;

namespace Pathoschild.Stardew.Common.Integrations.CustomBush;

internal class CustomBushIntegration : BaseIntegration<ICustomBushApi>
{
	public CustomBushIntegration(IModRegistry modRegistry, IMonitor monitor)
		: base("CustomBush", "furyx639.CustomBush", "1.2.0", modRegistry, monitor)
	{
	}
}
