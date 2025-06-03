using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewValley;

namespace Pathoschild.Stardew.Common.Integrations.Automate;

public interface IAutomateApi
{
	IDictionary<Vector2, int> GetMachineStates(GameLocation location, Rectangle tileArea);
}
