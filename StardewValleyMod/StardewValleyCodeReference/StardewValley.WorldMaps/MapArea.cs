using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.GameData.WorldMaps;
using StardewValley.Internal;
using StardewValley.TokenizableStrings;

namespace StardewValley.WorldMaps;

/// <summary>A smaller section of the map which is linked to one or more in-game locations. The map area might be edited/swapped depending on the context, have its own tooltip(s), or have its own player marker positions.</summary>
public class MapArea
{
	/// <summary>The cached value for <see cref="M:StardewValley.WorldMaps.MapArea.GetTextures" />.</summary>
	protected MapAreaTexture[] CachedTextures;

	/// <summary>The cached value for <see cref="M:StardewValley.WorldMaps.MapArea.GetTooltips" />.</summary>
	protected MapAreaTooltip[] CachedTooltips;

	/// <summary>The cached value for <see cref="M:StardewValley.WorldMaps.MapArea.GetWorldPositions" />.</summary>
	protected MapAreaPosition[] CachedWorldPositions;

	/// <summary>The cached value for <see cref="M:StardewValley.WorldMaps.MapArea.GetScrollText" />.</summary>
	protected string CachedScrollText;

	/// <summary>The unique identifier for the area.</summary>
	public string Id { get; }

	/// <summary>The large-scale part of the world (like the Valley) which contains this area.</summary>
	public MapRegion Region { get; }

	/// <summary>The underlying data.</summary>
	public WorldMapAreaData Data { get; }

	/// <summary>Construct an instance.</summary>
	/// <param name="region">The large-scale part of the world (like the Valley) which contains this area.</param>
	/// <param name="data">The underlying data.</param>
	public MapArea(MapRegion region, WorldMapAreaData data)
	{
		this.Data = data;
		this.Id = data.Id;
		this.Region = region;
	}

	/// <summary>Get the textures to draw onto the map (adjusted for pixel zoom), if any.</summary>
	public MapAreaTexture[] GetTextures()
	{
		if (this.CachedTextures == null)
		{
			if (this.Data.Textures.Count > 0)
			{
				List<MapAreaTexture> textures = new List<MapAreaTexture>();
				foreach (WorldMapTextureData entry in this.Data.Textures)
				{
					if (!GameStateQuery.CheckConditions(entry.Condition))
					{
						continue;
					}
					Texture2D texture = null;
					if (entry.Condition == "IS_CUSTOM_FARM_TYPE")
					{
						string textureName = Game1.whichModFarm?.WorldMapTexture;
						if (textureName == null)
						{
							continue;
						}
						texture = this.GetTexture(textureName);
						if (texture.Width <= 200)
						{
							entry.SourceRect = texture.Bounds;
						}
					}
					else
					{
						texture = this.GetTexture(entry.Texture);
					}
					Rectangle sourceRect = entry.SourceRect;
					if (sourceRect.IsEmpty)
					{
						sourceRect = new Rectangle(0, 0, texture.Width, texture.Height);
					}
					Rectangle mapPixelArea = entry.MapPixelArea;
					if (mapPixelArea.IsEmpty)
					{
						mapPixelArea = this.Data.PixelArea;
					}
					textures.Add(new MapAreaTexture(mapPixelArea: new Rectangle(mapPixelArea.X * 4, mapPixelArea.Y * 4, mapPixelArea.Width * 4, mapPixelArea.Height * 4), texture: texture, sourceRect: sourceRect));
				}
				this.CachedTextures = textures.ToArray();
			}
			else
			{
				this.CachedTextures = LegacyShims.EmptyArray<MapAreaTexture>();
			}
		}
		return this.CachedTextures;
	}

	/// <summary>Get the tooltips to draw onto the map, if any.</summary>
	public MapAreaTooltip[] GetTooltips()
	{
		if (this.CachedTooltips == null)
		{
			List<WorldMapTooltipData> tooltips = this.Data.Tooltips;
			if (tooltips != null && tooltips.Count > 0)
			{
				List<MapAreaTooltip> tooltips2 = new List<MapAreaTooltip>();
				foreach (WorldMapTooltipData entry in this.Data.Tooltips)
				{
					if (GameStateQuery.CheckConditions(entry.Condition))
					{
						string text = (GameStateQuery.CheckConditions(entry.KnownCondition) ? TokenParser.ParseText(Utility.TrimLines(entry.Text)) : "???");
						if (!string.IsNullOrWhiteSpace(text))
						{
							tooltips2.Add(new MapAreaTooltip(this, entry, text));
						}
					}
				}
				this.CachedTooltips = tooltips2.ToArray();
			}
			else
			{
				this.CachedTooltips = LegacyShims.EmptyArray<MapAreaTooltip>();
			}
		}
		return this.CachedTooltips;
	}

	/// <summary>Get all valid world positions in this area.</summary>
	public IEnumerable<MapAreaPosition> GetWorldPositions()
	{
		if (this.CachedWorldPositions == null)
		{
			List<MapAreaPosition> positions = new List<MapAreaPosition>();
			foreach (WorldMapAreaPositionData entry in this.Data.WorldPositions)
			{
				if (GameStateQuery.CheckConditions(entry.Condition))
				{
					positions.Add(new MapAreaPosition(this, entry));
				}
			}
			this.CachedWorldPositions = positions.ToArray();
		}
		return this.CachedWorldPositions;
	}

	/// <summary>Get a valid world position matching the given values, if any.</summary>
	/// <param name="locationName">The location name containing the tile.</param>
	/// <param name="contextName">The location's context name.</param>
	/// <param name="tile">The tile coordinate to match.</param>
	public MapAreaPosition GetWorldPosition(string locationName, string contextName, Point tile)
	{
		return this.GetWorldPosition(locationName, contextName, tile, null);
	}

	/// <summary>Get a valid world position matching the given values, if any.</summary>
	/// <param name="locationName">The location name containing the tile.</param>
	/// <param name="contextName">The location's context name.</param>
	/// <param name="tile">The tile coordinate to match.</param>
	/// <param name="log">The detailed log to update with the steps used to match the position, if set.</param>
	internal MapAreaPosition GetWorldPosition(string locationName, string contextName, Point tile, LogBuilder log)
	{
		LogBuilder subLog = log?.GetIndentedLog();
		foreach (MapAreaPosition position in this.GetWorldPositions())
		{
			log?.AppendLine("Checking position '" + position.Data.Id + "'...");
			if (position.Matches(locationName, contextName, tile, subLog))
			{
				return position;
			}
		}
		return null;
	}

	/// <summary>Get the translated tooltip text to display when hovering the cursor over the map area.</summary>
	public virtual string GetScrollText()
	{
		if (this.CachedScrollText == null)
		{
			this.CachedScrollText = TokenParser.ParseText(Utility.TrimLines(this.Data.ScrollText));
		}
		return this.CachedScrollText;
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
