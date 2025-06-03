using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewModdingAPI;

namespace Pathoschild.Stardew.Common.Integrations.BetterSprinklersPlus;

internal class BetterSprinklersPlusIntegration : BaseIntegration<IBetterSprinklersPlusApi>
{
	public int MaxRadius { get; }

	public BetterSprinklersPlusIntegration(IModRegistry modRegistry, IMonitor monitor)
		: base("Better Sprinklers Plus", "com.CodesThings.BetterSprinklersPlus", "2.6.0", modRegistry, monitor)
	{
		if (base.IsLoaded)
		{
			this.MaxRadius = base.ModApi.GetMaxGridSize();
		}
	}

	public IDictionary<int, Vector2[]> GetSprinklerTiles()
	{
		this.AssertLoaded();
		return base.ModApi.GetSprinklerCoverage();
	}
}
