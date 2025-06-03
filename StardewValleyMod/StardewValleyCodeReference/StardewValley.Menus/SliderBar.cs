using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley.Menus;

public class SliderBar
{
	public static int defaultWidth = 128;

	public const int defaultHeight = 20;

	public int value;

	public Rectangle bounds;

	public SliderBar(int x, int y, int initialValue)
	{
		this.bounds = new Rectangle(x, y, SliderBar.defaultWidth, 20);
		this.value = initialValue;
	}

	public int click(int x, int y)
	{
		if (this.bounds.Contains(x, y))
		{
			x -= this.bounds.X;
			this.value = (int)((float)x / (float)this.bounds.Width * 100f);
		}
		return this.value;
	}

	public void changeValueBy(int amount)
	{
		this.value += amount;
		this.value = Math.Max(0, Math.Min(100, this.value));
	}

	public void release(int x, int y)
	{
	}

	public void draw(SpriteBatch b)
	{
		b.Draw(Game1.staminaRect, new Rectangle(this.bounds.X, this.bounds.Center.Y - 2, this.bounds.Width, 4), Color.DarkGray);
		b.Draw(Game1.mouseCursors, new Vector2(this.bounds.X + (int)((float)this.value / 100f * (float)this.bounds.Width) + 4, this.bounds.Center.Y), new Rectangle(64, 256, 32, 32), Color.White, 0f, new Vector2(16f, 9f), 1f, SpriteEffects.None, 0.86f);
	}
}
