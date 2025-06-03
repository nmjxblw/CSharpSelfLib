using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Pathoschild.Stardew.Common.Integrations.LineSprinklers;

public interface ILineSprinklersApi
{
	int GetMaxGridSize();

	IDictionary<int, Vector2[]> GetSprinklerCoverage();
}
