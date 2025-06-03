using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewValley.Buildings;
using StardewValley.GameData.WorldMaps;
using StardewValley.Internal;

namespace StardewValley.WorldMaps;

/// <summary>Manages data related to the world map shown in the game menu.</summary>
public static class WorldMapManager
{
	/// <summary>The <see cref="F:StardewValley.Game1.ticks" /> value when cached data should be reset.</summary>
	private static int NextClearCacheTick;

	/// <summary>The maximum update ticks before any cached data should be refreshed.</summary>
	private static int MaxCacheTicks;

	/// <summary>The cached map regions.</summary>
	private static readonly List<MapRegion> Regions;

	/// <summary>Initialize before the class is first accessed.</summary>
	static WorldMapManager()
	{
		WorldMapManager.MaxCacheTicks = 3600;
		WorldMapManager.Regions = new List<MapRegion>();
		WorldMapManager.ReloadData();
	}

	/// <summary>Load the raw world map data.</summary>
	public static void ReloadData()
	{
		WorldMapManager.Regions.Clear();
		foreach (KeyValuePair<string, WorldMapRegionData> pair in DataLoader.WorldMap(Game1.content))
		{
			WorldMapManager.Regions.Add(new MapRegion(pair.Key, pair.Value));
		}
		WorldMapManager.NextClearCacheTick = Game1.ticks + WorldMapManager.MaxCacheTicks;
	}

	/// <summary>Get all map regions in the underlying data which are currently valid.</summary>
	public static IEnumerable<MapRegion> GetMapRegions()
	{
		WorldMapManager.ReloadDataIfStale();
		return WorldMapManager.Regions;
	}

	/// <summary>Get the map position which contains a given location and tile coordinate, if any.</summary>
	/// <param name="location">The in-game location.</param>
	/// <param name="tile">The tile coordinate within the location.</param>
	public static MapAreaPositionWithContext? GetPositionData(GameLocation location, Point tile)
	{
		return WorldMapManager.GetPositionData(location, tile, null);
	}

	/// <summary>Get the map position which contains a given location and tile coordinate, if any.</summary>
	/// <param name="location">The in-game location.</param>
	/// <param name="tile">The tile coordinate within the location.</param>
	/// <param name="log">The detailed log to update with the steps used to match the position, if set.</param>
	internal static MapAreaPositionWithContext? GetPositionData(GameLocation location, Point tile, LogBuilder log)
	{
		if (location == null)
		{
			log?.AppendLine("Skipped: location is null.");
			return null;
		}
		LogBuilder subLog = log?.GetIndentedLog();
		log?.AppendLine("Searching for the player position...");
		MapAreaPosition position = WorldMapManager.GetPositionDataWithoutFallback(location, tile, subLog);
		if (position != null)
		{
			log?.AppendLine("Found match: position '" + position.Data.Id + "'.");
			return new MapAreaPositionWithContext(position, location, tile);
		}
		Building building = location.ParentBuilding;
		GameLocation buildingLocation = building?.GetParentLocation();
		if (buildingLocation != null)
		{
			log?.AppendLine("");
			log?.AppendLine($"Searching for the exterior position of the '{building.buildingType.Value}' building in {buildingLocation.NameOrUniqueName}...");
			Point buildingTile = new Point(building.tileX.Value + building.tilesWide.Value / 2, building.tileY.Value + building.tilesHigh.Value / 2);
			position = WorldMapManager.GetPositionDataWithoutFallback(buildingLocation, buildingTile, subLog);
			if (position != null)
			{
				log?.AppendLine("Found match: position '" + position.Data.Id + "'.");
				return new MapAreaPositionWithContext(position, buildingLocation, buildingTile);
			}
		}
		log?.AppendLine("");
		log?.AppendLine("No match found.");
		return null;
	}

	/// <summary>Get the map position which contains a given location and tile coordinate, if any, without checking for parent buildings or locations.</summary>
	/// <param name="location">The in-game location.</param>
	/// <param name="tile">The tile coordinate within the location.</param>
	public static MapAreaPosition GetPositionDataWithoutFallback(GameLocation location, Point tile)
	{
		return WorldMapManager.GetPositionDataWithoutFallback(location, tile, null);
	}

	/// <summary>Get the map position which contains a given location and tile coordinate, if any, without checking for parent buildings or locations.</summary>
	/// <param name="location">The in-game location.</param>
	/// <param name="tile">The tile coordinate within the location.</param>
	/// <param name="log">The detailed log to update with the steps used to match the position, if set.</param>
	internal static MapAreaPosition GetPositionDataWithoutFallback(GameLocation location, Point tile, LogBuilder log)
	{
		if (location == null)
		{
			log?.AppendLine("Skipped: location is null.");
			return null;
		}
		LogBuilder subLog = log?.GetIndentedLog();
		foreach (MapRegion region in WorldMapManager.GetMapRegions())
		{
			log?.AppendLine("Checking region '" + region.Id + "'...");
			MapAreaPosition position = region.GetPositionData(location, tile, subLog);
			if (position != null)
			{
				return position;
			}
		}
		return null;
	}

	/// <summary>Update the world map data if needed.</summary>
	private static void ReloadDataIfStale()
	{
		if (Game1.ticks >= WorldMapManager.NextClearCacheTick)
		{
			WorldMapManager.ReloadData();
		}
	}
}
