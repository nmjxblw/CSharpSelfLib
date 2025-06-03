using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Pathoschild.Stardew.Common.Integrations.BetterSprinklersPlus;

public interface IBetterSprinklersPlusApi
{
	int GetMaxGridSize();

	IDictionary<int, Vector2[]> GetSprinklerCoverage();
}
