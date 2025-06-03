using StardewModdingAPI;

namespace Pathoschild.Stardew.Common.Integrations.ExtraMachineConfig;

internal class ExtraMachineConfigIntegration : BaseIntegration<IExtraMachineConfigApi>
{
	public ExtraMachineConfigIntegration(IModRegistry modRegistry, IMonitor monitor)
		: base("ExtraMachineConfig", "selph.ExtraMachineConfig", "1.4.0", modRegistry, monitor)
	{
	}
}
