using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Pathoschild.Stardew.Common.Integrations.SimpleSprinkler;

public interface ISimplerSprinklerApi
{
	IDictionary<int, Vector2[]> GetNewSprinklerCoverage();
}
