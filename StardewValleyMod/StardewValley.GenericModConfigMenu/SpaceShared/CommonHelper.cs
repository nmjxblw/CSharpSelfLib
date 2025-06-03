using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using xTile.Dimensions;

namespace SpaceShared;

internal static class CommonHelper
{
	public static IEnumerable<GameLocation> GetLocations(bool includeTempLevels = false)
	{
		IEnumerable<GameLocation> locations = Game1.locations.Concat(from location in Game1.locations
			from building in (IEnumerable<Building>)location.buildings
			where ((NetFieldBase<GameLocation, NetRef<GameLocation>>)(object)building.indoors).Value != null
			select ((NetFieldBase<GameLocation, NetRef<GameLocation>>)(object)building.indoors).Value);
		if (includeTempLevels)
		{
			locations = locations.Concat((IEnumerable<GameLocation>)MineShaft.activeMines).Concat((IEnumerable<GameLocation>)VolcanoDungeon.activeLevels);
		}
		return locations;
	}

	public static Vector2 GetPositionFromAnchor(int offsetX, int offsetY, int width, int height, PositionAnchor anchor, bool? uiScale = null, int zoom = 4)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		Rectangle screen = ((uiScale ?? Game1.uiMode) ? Game1.uiViewport : Game1.viewport);
		if ((anchor == PositionAnchor.BottomRight || anchor == PositionAnchor.TopRight) ? true : false)
		{
			offsetX = ((Rectangle)(ref screen)).Width - width * zoom - offsetX;
		}
		if ((uint)anchor <= 1u)
		{
			offsetY = ((Rectangle)(ref screen)).Height - height * zoom - offsetY;
		}
		return new Vector2((float)offsetX, (float)offsetY);
	}
}
