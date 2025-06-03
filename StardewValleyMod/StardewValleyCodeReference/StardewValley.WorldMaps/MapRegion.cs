using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.GameData.WorldMaps;
using StardewValley.Internal;
using StardewValley.Locations;

namespace StardewValley.WorldMaps;

/// <inheritdoc cref="T:StardewValley.GameData.WorldMaps.WorldMapRegionData" />
public class MapRegion
{
	/// <summary>The cached value for <see cref="M:StardewValley.WorldMaps.MapRegion.GetMapPixelBounds" />.</summary>
	protected Rectangle? CachedPixelBounds;

	/// <summary>The cached value for <see cref="M:StardewValley.WorldMaps.MapRegion.GetAreas" />.</summary>
	protected MapArea[] CachedMapAreas;

	/// <summary>The cached value for <see cref="M:StardewValley.WorldMaps.MapRegion.GetBaseTexture" />.</summary>
	protected MapAreaTexture CachedBaseTexture;

	/// <summary>The unique identifier for the region.</summary>
	public string Id { get; }

	/// <summary>The underlying data.</summary>
	public WorldMapRegionData Data { get; }

	/// <summary>Construct an instance.</summary>
	/// <param name="id">The area ID.</param>
	/// <param name="data">The underlying data.</param>
	public MapRegion(string id, WorldMapRegionData data)
	{
		this.Id = id;
		this.Data = data;
	}

	/// <summary>Get a pixel area on screen which contains all the map areas being drawn, centered on-screen.</summary>
	public Rectangle GetMapPixelBounds()
	{
		Rectangle? cachedPixelBounds = this.CachedPixelBounds;
		if (!cachedPixelBounds.HasValue)
		{
			MapAreaTexture baseTexture = this.GetBaseTexture();
			MapArea[] mapAreas = this.GetAreas();
			int maxWidth = baseTexture?.MapPixelArea.Width ?? 0;
			int maxHeight = baseTexture?.MapPixelArea.Height ?? 0;
			MapArea[] array = mapAreas;
			for (int i = 0; i < array.Length; i++)
			{
				MapAreaTexture[] textures = array[i].GetTextures();
				foreach (MapAreaTexture overlay in textures)
				{
					maxWidth = Math.Max(maxWidth, overlay.MapPixelArea.Width);
					maxHeight = Math.Max(maxHeight, overlay.MapPixelArea.Height);
				}
			}
			Vector2 topLeft = Utility.getTopLeftPositionForCenteringOnScreen(maxWidth, maxHeight);
			this.CachedPixelBounds = new Rectangle((int)topLeft.X, (int)topLeft.Y, maxWidth / 4, maxHeight / 4);
		}
		return this.CachedPixelBounds.Value;
	}

	/// <summary>Get the base texture to draw under the map areas (adjusted for pixel zoom), if any.</summary>
	public MapAreaTexture GetBaseTexture()
	{
		if (this.CachedBaseTexture == null)
		{
			if (this.Data.BaseTexture.Count > 0)
			{
				foreach (WorldMapTextureData entry in this.Data.BaseTexture)
				{
					if (GameStateQuery.CheckConditions(entry.Condition))
					{
						Texture2D texture = this.GetTexture(entry.Texture);
						Rectangle sourceRect = entry.SourceRect;
						if (sourceRect.IsEmpty)
						{
							sourceRect = new Rectangle(0, 0, texture.Width, texture.Height);
						}
						Rectangle mapPixelArea = entry.MapPixelArea;
						if (mapPixelArea.IsEmpty)
						{
							mapPixelArea = sourceRect;
						}
						this.CachedBaseTexture = new MapAreaTexture(mapPixelArea: new Rectangle(mapPixelArea.X * 4, mapPixelArea.Y * 4, mapPixelArea.Width * 4, mapPixelArea.Height * 4), texture: texture, sourceRect: sourceRect);
						break;
					}
				}
			}
			if (this.CachedBaseTexture == null)
			{
				this.CachedBaseTexture = new MapAreaTexture(null, Rectangle.Empty, Rectangle.Empty);
			}
		}
		if (this.CachedBaseTexture.Texture == null)
		{
			return null;
		}
		return this.CachedBaseTexture;
	}

	/// <summary>Get all areas that are part of the region.</summary>
	public MapArea[] GetAreas()
	{
		if (this.CachedMapAreas == null)
		{
			List<MapArea> areas = new List<MapArea>();
			foreach (WorldMapAreaData area in this.Data.MapAreas)
			{
				if (GameStateQuery.CheckConditions(area.Condition))
				{
					areas.Add(new MapArea(this, area));
				}
			}
			this.CachedMapAreas = areas.ToArray();
		}
		return this.CachedMapAreas;
	}

	/// <summary>Get the map position which contains a given location and tile coordinate, if any.</summary>
	/// <param name="location">The in-game location.</param>
	/// <param name="tile">The tile coordinate within the location.</param>
	public MapAreaPosition GetPositionData(GameLocation location, Point tile)
	{
		return this.GetPositionData(location, tile, null);
	}

	/// <summary>Get the map position which contains a given location and tile coordinate, if any.</summary>
	/// <param name="location">The in-game location.</param>
	/// <param name="tile">The tile coordinate within the location.</param>
	/// <param name="log">The detailed log to update with the steps used to match the position, if set.</param>
	internal MapAreaPosition GetPositionData(GameLocation location, Point tile, LogBuilder log)
	{
		if (location == null)
		{
			log?.AppendLine("Skipped: location is null.");
			return null;
		}
		string locationName = this.GetLocationName(location);
		string contextId = location.GetLocationContextId();
		LogBuilder subLog = log?.GetIndentedLog();
		MapArea[] areas = this.GetAreas();
		foreach (MapArea mapArea in areas)
		{
			log?.AppendLine("Checking map area '" + mapArea.Id + "'...");
			MapAreaPosition position = mapArea.GetWorldPosition(locationName, contextId, tile, subLog);
			if (position != null)
			{
				return position;
			}
		}
		return null;
	}

	/// <summary>Get a location's name as it appears in <c>Data/WorldMap</c>.</summary>
	/// <param name="location">The location whose name to get.</param>
	/// <remarks>For example, mine levels have internal names like <c>UndergroundMine14</c>, but they're all covered by <c>Mines</c> or <c>SkullCave</c> in <c>Data/Maps</c>.</remarks>
	protected string GetLocationName(GameLocation location)
	{
		string locationName = ((location.IsTemporary && !string.IsNullOrEmpty(location.Map.Id)) ? location.Map.Id : location.Name);
		if (locationName == "Mine")
		{
			return "Mines";
		}
		if (location is MineShaft shaft)
		{
			if (shaft.mineLevel <= 120 || shaft.mineLevel == 77377)
			{
				return "Mines";
			}
			return "SkullCave";
		}
		if (VolcanoDungeon.IsGeneratedLevel(location.Name))
		{
			return "VolcanoDungeon";
		}
		return locationName;
	}

	/// <summary>Get the texture to load for an asset name.</summary>
	/// <param name="assetName">The asset name to load.</param>
	private Texture2D GetTexture(string assetName)
	{
		if (Game1.season != Season.Spring)
		{
			string seasonalName = assetName + "_" + Game1.currentSeason.ToLower();
			if (Game1.content.DoesAssetExist<Texture2D>(seasonalName))
			{
				return Game1.content.Load<Texture2D>(seasonalName);
			}
		}
		return Game1.content.Load<Texture2D>(assetName);
	}
}
