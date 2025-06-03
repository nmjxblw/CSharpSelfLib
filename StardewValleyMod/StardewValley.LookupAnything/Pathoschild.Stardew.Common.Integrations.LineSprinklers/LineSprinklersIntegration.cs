using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewModdingAPI;

namespace Pathoschild.Stardew.Common.Integrations.LineSprinklers;

internal class LineSprinklersIntegration : BaseIntegration<ILineSprinklersApi>
{
	public int MaxRadius { get; }

	public LineSprinklersIntegration(IModRegistry modRegistry, IMonitor monitor)
		: base("Line Sprinklers", "hootless.LineSprinklers", "1.1.0", modRegistry, monitor)
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
