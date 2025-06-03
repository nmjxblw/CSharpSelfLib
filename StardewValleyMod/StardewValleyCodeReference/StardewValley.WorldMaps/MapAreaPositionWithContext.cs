using Microsoft.Xna.Framework;

namespace StardewValley.WorldMaps;

/// <summary>The data which maps an in-game location and tile position to a parent <see cref="T:StardewValley.WorldMaps.MapArea" />, along with the location and tile that matched it.</summary>
public readonly struct MapAreaPositionWithContext
{
	/// <summary>The data which maps an in-game location and tile position to a parent <see cref="T:StardewValley.WorldMaps.MapArea" />.</summary>
	public MapAreaPosition Data { get; }

	/// <summary>The location for which the <see cref="P:StardewValley.WorldMaps.MapAreaPositionWithContext.Data" /> was selected.</summary>
	public GameLocation Location { get; }

	/// <summary>The location for which the <see cref="P:StardewValley.WorldMaps.MapAreaPositionWithContext.Tile" /> was selected.</summary>
	public Point Tile { get; }

	/// <summary>Construct an instance.</summary>
	/// <param name="data"><inheritdoc cref="P:StardewValley.WorldMaps.MapAreaPositionWithContext.Data" path="/summary" /></param>
	/// <param name="location"><inheritdoc cref="P:StardewValley.WorldMaps.MapAreaPositionWithContext.Location" path="/summary" /></param>
	/// <param name="tile"><inheritdoc cref="P:StardewValley.WorldMaps.MapAreaPositionWithContext.Tile" path="/summary" /></param>
	public MapAreaPositionWithContext(MapAreaPosition data, GameLocation location, Point tile)
	{
		this.Data = data;
		this.Location = location;
		this.Tile = tile;
	}

	/// <summary>Get the pixel position within the world map which corresponds to the in-game location's tile within the map area, adjusted for pixel zoom.</summary>
	public Vector2 GetMapPixelPosition()
	{
		return this.Data.GetMapPixelPosition(this.Location, this.Tile);
	}

	/// <summary>Get the player's position as a percentage along the X and Y axes.</summary>
	public Vector2? GetPositionRatioIfValid()
	{
		return this.Data.GetPositionRatioIfValid(this.Location, this.Tile);
	}

	/// <summary>Get the translated display name to show when the player is in this position.</summary>
	public string GetScrollText()
	{
		return this.Data.GetScrollText(this.Tile);
	}
}
