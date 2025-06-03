using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Pathoschild.Stardew.Common.Integrations.BetterSprinklers;

public interface IBetterSprinklersApi
{
	int GetMaxGridSize();

	IDictionary<int, Vector2[]> GetSprinklerCoverage();
}
