using System;
using xTile;
using xTile.Dimensions;
using xTile.Layers;

namespace Pathoschild.Stardew.Common;

internal static class MapExtensions
{
	public static Size GetSizeInTiles(this Map map)
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		int width = 1;
		int height = 1;
		foreach (Layer layer in map.Layers)
		{
			width = Math.Max(width, layer.LayerWidth);
			height = Math.Max(height, layer.LayerHeight);
		}
		return new Size(width, height);
	}
}
