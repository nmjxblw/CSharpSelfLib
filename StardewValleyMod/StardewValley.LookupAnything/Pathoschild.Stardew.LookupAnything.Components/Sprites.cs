using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.Common;
using StardewValley;

namespace Pathoschild.Stardew.LookupAnything.Components;

internal static class Sprites
{
	public static class Letter
	{
		public static readonly Rectangle Sprite = new Rectangle(0, 0, 320, 180);

		public static Texture2D Sheet => Game1.content.Load<Texture2D>("LooseSprites\\letterBG");
	}

	public static class Textbox
	{
		public static Texture2D Sheet => Game1.content.Load<Texture2D>("LooseSprites\\textBox");
	}

	public static readonly Texture2D Pixel = CommonHelper.Pixel;
}
