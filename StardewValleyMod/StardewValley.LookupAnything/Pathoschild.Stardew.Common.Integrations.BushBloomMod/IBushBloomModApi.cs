using Microsoft.Xna.Framework;
using StardewValley;

namespace Pathoschild.Stardew.Common.Integrations.BushBloomMod;

public interface IBushBloomModApi
{
	(string, WorldDate, WorldDate)[] GetActiveSchedules(string season, int dayofMonth, int? year = null, GameLocation? location = null, Vector2? tile = null);

	bool IsReady();
}
