using StardewModdingAPI;

namespace Pathoschild.Stardew.Common.Integrations.SpaceCore;

internal class SpaceCoreIntegration : BaseIntegration<ISpaceCoreApi>
{
	public SpaceCoreIntegration(IModRegistry modRegistry, IMonitor monitor)
		: base("SpaceCore", "spacechase0.SpaceCore", "1.25.0", modRegistry, monitor)
	{
	}
}
