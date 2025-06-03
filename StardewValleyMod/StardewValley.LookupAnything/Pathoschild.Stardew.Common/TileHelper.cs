using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewValley;
using xTile;
using xTile.Dimensions;
using xTile.Layers;

namespace Pathoschild.Stardew.Common;

internal static class TileHelper
{
	public static IEnumerable<Vector2> GetTiles(this GameLocation? location)
	{
		object obj;
		if (location == null)
		{
			obj = null;
		}
		else
		{
			Map map = location.Map;
			obj = ((map != null) ? map.Layers : null);
		}
		if (obj == null)
		{
			return Array.Empty<Vector2>();
		}
		Layer layer = location.Map.Layers[0];
		return TileHelper.GetTiles(0, 0, layer.LayerWidth, layer.LayerHeight);
	}

	public static IEnumerable<Vector2> GetTiles(this Rectangle area)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		return TileHelper.GetTiles(area.X, area.Y, area.Width, area.Height);
	}

	public static Rectangle Expand(this Rectangle area, int distance)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		return new Rectangle(area.X - distance, area.Y - distance, area.Width + distance * 2, area.Height + distance * 2);
	}

	public static IEnumerable<Vector2> GetSurroundingTiles(this Vector2 tile)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		return Utility.getSurroundingTileLocationsArray(tile);
	}

	public static IEnumerable<Vector2> GetSurroundingTiles(this Rectangle area)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		for (int x = area.X - 1; x <= area.X + area.Width; x++)
		{
			for (int y = area.Y - 1; y <= area.Y + area.Height; y++)
			{
				if (!((Rectangle)(ref area)).Contains(x, y))
				{
					yield return new Vector2((float)x, (float)y);
				}
			}
		}
	}

	public static IEnumerable<Vector2> GetAdjacentTiles(this Vector2 tile)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		return Utility.getAdjacentTileLocationsArray(tile);
	}

	public static IEnumerable<Vector2> GetTiles(int x, int y, int width, int height)
	{
		int curX = x;
		for (int maxX = x + width - 1; curX <= maxX; curX++)
		{
			int curY = y;
			for (int maxY = y + height - 1; curY <= maxY; curY++)
			{
				yield return new Vector2((float)curX, (float)curY);
			}
		}
	}

	public static IEnumerable<Vector2> GetVisibleTiles(int expand = 0)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return TileHelper.GetVisibleArea(expand).GetTiles();
	}

	public static Rectangle GetVisibleArea(int expand = 0)
	{
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		return new Rectangle(((Rectangle)(ref Game1.viewport)).X / 64 - expand, ((Rectangle)(ref Game1.viewport)).Y / 64 - expand, (int)Math.Ceiling((decimal)((Rectangle)(ref Game1.viewport)).Width / 64m) + expand * 2, (int)Math.Ceiling((decimal)((Rectangle)(ref Game1.viewport)).Height / 64m) + expand * 2);
	}

	public static Vector2 GetTileFromCursor()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		return TileHelper.GetTileFromScreenPosition(Game1.getMouseX(), Game1.getMouseY());
	}

	public static Vector2 GetTileFromScreenPosition(float x, float y)
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		float screenX = (float)((Rectangle)(ref Game1.viewport)).X + x;
		float screenY = (float)((Rectangle)(ref Game1.viewport)).Y + y;
		int tileX = (int)Math.Floor(screenX / 64f);
		int tileY = (int)Math.Floor(screenY / 64f);
		return new Vector2((float)tileX, (float)tileY);
	}
}
