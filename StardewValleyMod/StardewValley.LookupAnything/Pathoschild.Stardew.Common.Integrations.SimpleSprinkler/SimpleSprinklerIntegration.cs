using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewModdingAPI;

namespace Pathoschild.Stardew.Common.Integrations.SimpleSprinkler;

internal class SimpleSprinklerIntegration : BaseIntegration<ISimplerSprinklerApi>
{
	public SimpleSprinklerIntegration(IModRegistry modRegistry, IMonitor monitor)
		: base("Simple Sprinklers", "tZed.SimpleSprinkler", "1.6.0", modRegistry, monitor)
	{
	}

	public IDictionary<int, Vector2[]> GetNewSprinklerTiles()
	{
		this.AssertLoaded();
		return base.ModApi.GetNewSprinklerCoverage();
	}
}
