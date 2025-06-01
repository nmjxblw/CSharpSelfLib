namespace StardewValley;

public class WaterTiles
{
	public struct WaterTileData
	{
		public bool isWater;

		public bool isVisible;

		public WaterTileData(bool is_water, bool is_visible)
		{
			this.isWater = is_water;
			this.isVisible = is_visible;
		}
	}

	/// <summary>The water data for each tile in the grid.</summary>
	public WaterTileData[,] waterTiles;

	/// <summary>Get or set whether a tile is water.</summary>
	/// <param name="x">The tile's X tile position within the grid.</param>
	/// <param name="y">The tile's Y tile position within the grid.</param>
	public bool this[int x, int y]
	{
		get
		{
			return this.waterTiles[x, y].isWater;
		}
		set
		{
			this.waterTiles[x, y] = new WaterTileData(value, is_visible: true);
		}
	}

	/// <summary>Construct an instance.</summary>
	/// <param name="source">The grid of tiles to represent, where each value indicates whether it's water.</param>
	public WaterTiles(bool[,] source)
	{
		int width = source.GetLength(0);
		int height = source.GetLength(1);
		this.waterTiles = new WaterTileData[width, height];
		for (int x = 0; x < width; x++)
		{
			for (int y = 0; y < height; y++)
			{
				this.waterTiles[x, y] = new WaterTileData(source[x, y], is_visible: true);
			}
		}
	}

	/// <summary>Construct an instance.</summary>
	/// <param name="width">The width of the tile grid.</param>
	/// <param name="height">The height of the tile grid.</param>
	public WaterTiles(int width, int height)
	{
		this.waterTiles = new WaterTileData[width, height];
		for (int x = 0; x < width; x++)
		{
			for (int y = 0; y < height; y++)
			{
				this.waterTiles[x, y] = new WaterTileData(is_water: false, is_visible: true);
			}
		}
	}
}
