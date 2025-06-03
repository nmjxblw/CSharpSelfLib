using StardewModdingAPI;

namespace Pathoschild.Stardew.Common.Integrations.BushBloomMod;

internal class BushBloomModIntegration : BaseIntegration<IBushBloomModApi>
{
	public BushBloomModIntegration(IModRegistry modRegistry, IMonitor monitor)
		: base("CustomBush", "NCarigon.BushBloomMod", "1.2.4", modRegistry, monitor)
	{
	}
}
