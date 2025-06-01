using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using xTile;
using xTile.Dimensions;
using xTile.Layers;
using xTile.ObjectModel;
using xTile.Tiles;

namespace StardewValley.Extensions;

/// <summary>Provides utility extension methods on MonoGame and xTile types.</summary>
public static class FrameworkExtensions
{
	/// <summary>Get a subset of the screen viewport that's guaranteed to be visible on a lower-quality display.</summary>
	/// <param name="viewport">The viewport pixel area.</param>
	public static Microsoft.Xna.Framework.Rectangle GetTitleSafeArea(this Viewport viewport)
	{
		return viewport.Bounds;
	}

	/// <summary>Get the point positions within the rectangle.</summary>
	/// <param name="rect">The rectangle area.</param>
	public static IEnumerable<Point> GetPoints(this Microsoft.Xna.Framework.Rectangle rect)
	{
		int right = rect.Right;
		int bottom = rect.Bottom;
		for (int y = rect.Y; y < bottom; y++)
		{
			for (int x = rect.X; x < right; x++)
			{
				yield return new Point(x, y);
			}
		}
	}

	/// <summary>Get the integer <see cref="T:Microsoft.Xna.Framework.Vector2" /> positions within the rectangle.</summary>
	/// <param name="rect">The rectangle area.</param>
	public static IEnumerable<Vector2> GetVectors(this Microsoft.Xna.Framework.Rectangle rect)
	{
		int right = rect.Right;
		int bottom = rect.Bottom;
		for (int y = rect.Y; y < bottom; y++)
		{
			for (int x = rect.X; x < right; x++)
			{
				yield return new Vector2(x, y);
			}
		}
	}

	/// <summary>Get a new rectangle with the same values as this instance.</summary>
	/// <param name="rect">The rectangle to clone.</param>
	public static Microsoft.Xna.Framework.Rectangle Clone(this Microsoft.Xna.Framework.Rectangle rect)
	{
		return new Microsoft.Xna.Framework.Rectangle(rect.X, rect.Y, rect.Width, rect.Height);
	}

	public static Vector2 Size(this Viewport vp)
	{
		return new Vector2(vp.Width, vp.Height);
	}

	public static int GetElementCount(this Texture2D texture)
	{
		return texture.ActualWidth * texture.ActualHeight;
	}

	public static int GetActualWidth(this Texture2D texture)
	{
		return texture.ActualWidth;
	}

	public static int GetActualHeight(this Texture2D texture)
	{
		return texture.ActualHeight;
	}

	public static void SetContentSize(this Texture2D texture, int width, int height)
	{
		texture.SetImageSize(width, height);
	}

	/// <summary>Get a property value as a string, if it exists.</summary>
	/// <param name="properties">The property collection to search.</param>
	/// <param name="key">The property key to fetch.</param>
	/// <param name="value">The property value, if found.</param>
	/// <returns>Returns whether the property value was found.</returns>
	public static bool TryGetValue(this IPropertyCollection properties, string key, out string value)
	{
		if (!properties.TryGetValue(key, out var propertyValue))
		{
			value = null;
			return false;
		}
		value = propertyValue;
		return true;
	}

	/// <summary>Add a property if the key isn't already present.</summary>
	/// <param name="properties">The properties to modify.</param>
	/// <param name="key">The key of the property to add.</param>
	/// <param name="value">The value of the property to add.</param>
	/// <returns>Returns whether the value was successfully added.</returns>
	public static bool TryAdd(this IPropertyCollection properties, string key, string value)
	{
		if (properties.ContainsKey(key))
		{
			return false;
		}
		properties.Add(key, new PropertyValue(value));
		return true;
	}

	/// <summary>Get a map layer by ID, or throw an exception if it's not found.</summary>
	/// <param name="map">The map whose layer to get.</param>
	/// <param name="layerId">The layer ID.</param>
	/// <exception cref="T:System.Collections.Generic.KeyNotFoundException">The <paramref name="layerId" /> doesn't match a layer in the map.</exception>
	public static Layer RequireLayer(this Map map, string layerId)
	{
		return map.GetLayer(layerId) ?? throw new KeyNotFoundException($"The '{map.assetPath}' map doesn't have required layer '{layerId}'.");
	}

	/// <summary>Get a map tilesheet by ID, or throw an exception if it's not found.</summary>
	/// <param name="map">The map whose layer to get.</param>
	/// <param name="tilesheetId">The tilesheet ID to get.</param>
	/// <exception cref="T:System.Collections.Generic.KeyNotFoundException">The <paramref name="tilesheetId" /> doesn't match a tilesheet in the map.</exception>
	public static TileSheet RequireTileSheet(this Map map, string tilesheetId)
	{
		return map.GetTileSheet(tilesheetId) ?? throw new KeyNotFoundException($"The '{map.assetPath}' map doesn't have required tile sheet '{tilesheetId}'.");
	}

	/// <summary>Get a map tilesheet by ID, or throw an exception if it's not found.</summary>
	/// <param name="map">The map whose layer to get.</param>
	/// <param name="expectedIndex">The expected index in the map's tilesheet array which should contain the tilesheet. If this doesn't match the tilesheet name, it'll fallback to getting the tilesheet by name.</param>
	/// <param name="tilesheetId">The tilesheet ID to get.</param>
	/// <exception cref="T:System.Collections.Generic.KeyNotFoundException">The <paramref name="tilesheetId" /> doesn't match a tilesheet in the map.</exception>
	public static TileSheet RequireTileSheet(this Map map, int expectedIndex, string tilesheetId)
	{
		if (map.TileSheets.Count > expectedIndex)
		{
			TileSheet tilesheet = map.TileSheets[expectedIndex];
			if (tilesheet.Id == tilesheetId)
			{
				return tilesheet;
			}
		}
		return map.GetTileSheet(tilesheetId) ?? throw new KeyNotFoundException($"The '{map.assetPath}' map doesn't have required tile sheet '{tilesheetId}'.");
	}

	/// <summary>Get whether a tile exists at the given coordinate.</summary>
	/// <param name="map">The map whose tiles to check.</param>
	/// <param name="tile">The tile coordinate.</param>
	/// <param name="layerId">The layer whose tiles to check.</param>
	/// <param name="tilesheetId">The tilesheet ID to check, or <c>null</c> for any tilesheet. If the tile doesn't use this tilesheet, it'll be ignored.</param>
	public static bool HasTileAt(this Map map, Location tile, string layerId, string tilesheetId = null)
	{
		return map?.GetLayer(layerId)?.HasTileAt(tile.X, tile.Y, tilesheetId) == true;
	}

	/// <summary>Get whether a tile exists at the given coordinate.</summary>
	/// <param name="map">The map whose tiles to check.</param>
	/// <param name="x">The tile X coordinate.</param>
	/// <param name="y">The tile Y coordinate.</param>
	/// <param name="layerId">The layer whose tiles to check.</param>
	/// <param name="tilesheetId">The tilesheet ID to check, or <c>null</c> for any tilesheet. If the tile doesn't use this tilesheet, it'll be ignored.</param>
	public static bool HasTileAt(this Map map, int x, int y, string layerId, string tilesheetId = null)
	{
		return map?.GetLayer(layerId)?.HasTileAt(x, y, tilesheetId) == true;
	}

	/// <summary>Get the tile index at the given layer coordinate.</summary>
	/// <param name="map">The map whose tiles to check.</param>
	/// <param name="x">The tile X coordinate.</param>
	/// <param name="y">The tile Y coordinate.</param>
	/// <param name="layerId">The layer whose tiles to check.</param>
	/// <param name="tilesheetId">The tilesheet ID for which to get a tile index, or <c>null</c> for any tilesheet. If the tile doesn't use this tilesheet, it'll be ignored.</param>
	/// <returns>Returns the matching tile's index, or <c>-1</c> if no tile was found.</returns>
	public static int GetTileIndexAt(this Map map, int x, int y, string layerId, string tilesheetId = null)
	{
		return map?.GetLayer(layerId)?.GetTileIndexAt(x, y, tilesheetId) ?? (-1);
	}

	/// <summary>Get the tile index at the given layer coordinate.</summary>
	/// <param name="map">The map whose tiles to check.</param>
	/// <param name="tile">The tile coordinates.</param>
	/// <param name="layerId">The layer whose tiles to check.</param>
	/// <param name="tilesheetId">The tilesheet ID for which to get a tile index, or <c>null</c> for any tilesheet. If the tile doesn't use this tilesheet, it'll be ignored.</param>
	/// <returns>Returns the matching tile's index, or <c>-1</c> if no tile was found.</returns>
	public static int GetTileIndexAt(this Map map, Location tile, string layerId, string tilesheetId = null)
	{
		return map?.GetLayer(layerId)?.GetTileIndexAt(tile.X, tile.Y, tilesheetId) ?? (-1);
	}

	/// <summary>Get whether a tile exists at the given coordinate.</summary>
	/// <param name="layer">The layer whose tiles to check.</param>
	/// <param name="tile">The tile coordinate.</param>
	/// <param name="tilesheetId">The tilesheet ID to check, or <c>null</c> for any tilesheet. If the tile doesn't use this tilesheet, it'll be ignored.</param>
	public static bool HasTileAt(this Layer layer, Location tile, string tilesheetId = null)
	{
		return layer.HasTileAt(tile.X, tile.Y, tilesheetId);
	}

	/// <summary>Get whether a tile exists at the given coordinate.</summary>
	/// <param name="layer">The layer whose tiles to check.</param>
	/// <param name="x">The tile X coordinate.</param>
	/// <param name="y">The tile Y coordinate.</param>
	/// <param name="tilesheetId">The tilesheet ID for which to get a tile index, or <c>null</c> for any tilesheet. If the tile doesn't use this tilesheet, it'll be ignored.</param>
	/// <returns>Returns the matching tile's index, or <c>-1</c> if no tile was found.</returns>
	public static bool HasTileAt(this Layer layer, int x, int y, string tilesheetId = null)
	{
		return layer.GetTileIndexAt(x, y, tilesheetId) != -1;
	}

	/// <summary>Get the tile index at the given layer coordinate.</summary>
	/// <param name="layer">The layer whose tiles to check.</param>
	/// <param name="tile">The tile coordinates.</param>
	/// <param name="tilesheetId">The tilesheet ID for which to get a tile index, or <c>null</c> for any tilesheet. If the tile doesn't use this tilesheet, it'll be ignored.</param>
	/// <returns>Returns the matching tile's index, or <c>-1</c> if no tile was found.</returns>
	public static int GetTileIndexAt(this Layer layer, Location tile, string tilesheetId = null)
	{
		return layer?.GetTileIndexAt(tile.X, tile.Y, tilesheetId) ?? (-1);
	}

	/// <summary>Get the tile index at the given layer coordinate.</summary>
	/// <param name="layer">The layer whose tiles to check.</param>
	/// <param name="x">The tile X coordinate.</param>
	/// <param name="y">The tile Y coordinate.</param>
	/// <param name="tilesheetId">The tilesheet ID for which to get a tile index, or <c>null</c> for any tilesheet. If the tile doesn't use this tilesheet, it'll be ignored.</param>
	/// <returns>Returns the matching tile's index, or <c>-1</c> if no tile was found.</returns>
	public static int GetTileIndexAt(this Layer layer, int x, int y, string tilesheetId = null)
	{
		Tile tile = layer?.Tiles[x, y];
		if (tile == null)
		{
			return -1;
		}
		if (tilesheetId != null && !(tile.TileSheet?.Id).EqualsIgnoreCase(tilesheetId))
		{
			return -1;
		}
		return tile.TileIndex;
	}

	public static Microsoft.Xna.Framework.Rectangle ToXna(this xTile.Dimensions.Rectangle xrect)
	{
		return new Microsoft.Xna.Framework.Rectangle(xrect.X, xrect.Y, xrect.Width, xrect.Height);
	}
}
