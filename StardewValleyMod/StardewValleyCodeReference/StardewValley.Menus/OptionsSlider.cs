using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace StardewValley.Menus;

public class OptionsSlider : OptionsElement
{
	public const int pixelsWide = 48;

	public const int pixelsHigh = 6;

	public const int sliderButtonWidth = 10;

	public const int sliderMaxValue = 100;

	public int value;

	public static Rectangle sliderBGSource = new Rectangle(403, 383, 6, 6);

	public static Rectangle sliderButtonRect = new Rectangle(420, 441, 10, 6);

	public OptionsSlider(string label, int whichOption, int x = -1, int y = -1)
		: base(label, x, y, 192, 24, whichOption)
	{
		Game1.options.setSliderToProperValue(this);
	}

	/// <inheritdoc />
	public override void leftClickHeld(int x, int y)
	{
		if (!base.greyedOut)
		{
			base.leftClickHeld(x, y);
			if (x < base.bounds.X)
			{
				this.value = 0;
			}
			else if (x > base.bounds.Right - 40)
			{
				this.value = 100;
			}
			else
			{
				this.value = (int)((float)(x - base.bounds.X) / (float)(base.bounds.Width - 40) * 100f);
			}
			Game1.options.changeSliderOption(base.whichOption, this.value);
		}
	}

	/// <inheritdoc />
	public override void receiveLeftClick(int x, int y)
	{
		if (!base.greyedOut)
		{
			base.receiveLeftClick(x, y);
			this.leftClickHeld(x, y);
		}
	}

	/// <inheritdoc />
	public override void receiveKeyPress(Keys key)
	{
		base.receiveKeyPress(key);
		if (Game1.options.snappyMenus && Game1.options.gamepadControls && !base.greyedOut)
		{
			if (Game1.options.doesInputListContain(Game1.options.moveRightButton, key))
			{
				this.value = Math.Min(this.value + 10, 100);
				Game1.options.changeSliderOption(base.whichOption, this.value);
			}
			else if (Game1.options.doesInputListContain(Game1.options.moveLeftButton, key))
			{
				this.value = Math.Max(this.value - 10, 0);
				Game1.options.changeSliderOption(base.whichOption, this.value);
			}
		}
	}

	/// <inheritdoc />
	public override void draw(SpriteBatch b, int slotX, int slotY, IClickableMenu context = null)
	{
		base.draw(b, slotX, slotY, context);
		IClickableMenu.drawTextureBox(b, Game1.mouseCursors, OptionsSlider.sliderBGSource, slotX + base.bounds.X, slotY + base.bounds.Y, base.bounds.Width, base.bounds.Height, Color.White, 4f, drawShadow: false);
		b.Draw(Game1.mouseCursors, new Vector2((float)(slotX + base.bounds.X) + (float)(base.bounds.Width - 40) * ((float)this.value / 100f), slotY + base.bounds.Y), OptionsSlider.sliderButtonRect, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.9f);
	}
}
