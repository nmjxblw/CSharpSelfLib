using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;

namespace Pathoschild.Stardew.Common.Integrations.Automate;

internal class AutomateIntegration : BaseIntegration<IAutomateApi>
{
	public AutomateIntegration(IModRegistry modRegistry, IMonitor monitor)
		: base("Automate", "Pathoschild.Automate", "1.11.0", modRegistry, monitor)
	{
	}

	public IDictionary<Vector2, int> GetMachineStates(GameLocation location, Rectangle tileArea)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		this.AssertLoaded();
		return base.ModApi.GetMachineStates(location, tileArea);
	}
}
