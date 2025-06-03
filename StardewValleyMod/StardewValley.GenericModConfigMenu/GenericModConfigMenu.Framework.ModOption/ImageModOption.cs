using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GenericModConfigMenu.Framework.ModOption;

internal class ImageModOption : ReadOnlyModOption
{
	public Func<Texture2D> Texture { get; }

	public Rectangle? TexturePixelArea { get; }

	public int Scale { get; }

	public ImageModOption(Func<Texture2D> texture, Rectangle? texturePixelArea, int scale, ModConfig mod)
		: base(() => "", null, mod)
	{
		this.Texture = texture;
		this.TexturePixelArea = texturePixelArea;
		this.Scale = scale;
	}
}
