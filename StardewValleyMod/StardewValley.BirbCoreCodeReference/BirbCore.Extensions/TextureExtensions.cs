using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;

namespace BirbCore.Extensions;

public static class TextureExtensions
{
	public static Texture2D GetTextureRect(this IRawTextureData texture, int x, int y, int width, int height)
	{
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Expected O, but got Unknown
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		if (x < 0 || y < 0 || width <= 0 || height <= 0 || x + width > texture.Width || y + height > texture.Height)
		{
			throw new ArgumentOutOfRangeException("texture", "subtexture rect must be within texture");
		}
		Color[] color = (Color[])(object)new Color[width * height];
		int i = 0;
		for (int dy = y; dy < y + height; dy++)
		{
			for (int dx = x; dx < x + width; dx++)
			{
				color[i] = texture.Data[dx + dy * texture.Width];
				i++;
			}
		}
		Texture2D result = new Texture2D(Game1.graphics.GraphicsDevice, width, height);
		result.SetData<Color>(color);
		return result;
	}

	public static Color GetColor(this IRawTextureData texture, int x, int y)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		if (x < 0 || y < 0 || x > texture.Width || y > texture.Height)
		{
			throw new ArgumentOutOfRangeException("texture", "pixel must be within texture");
		}
		return texture.Data[x + y * texture.Width];
	}
}
