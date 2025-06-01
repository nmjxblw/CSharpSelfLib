using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;

namespace StardewValley.Menus;

public class OptionsElement : IScreenReadable
{
	public enum Style
	{
		Default,
		OptionLabel
	}

	public const int defaultX = 8;

	public const int defaultY = 4;

	public const int defaultPixelWidth = 9;

	public Rectangle bounds;

	public string label;

	public int whichOption;

	public bool greyedOut;

	public Vector2 labelOffset = Vector2.Zero;

	public Style style;

	/// <inheritdoc />
	public string ScreenReaderText { get; set; }

	/// <inheritdoc />
	public string ScreenReaderDescription { get; set; }

	/// <inheritdoc />
	public bool ScreenReaderIgnore { get; set; }

	public OptionsElement(string label)
	{
		this.label = label;
		this.bounds = new Rectangle(32, 16, 36, 36);
		this.whichOption = -1;
	}

	public OptionsElement(string label, int x, int y, int width, int height, int whichOption = -1)
	{
		if (x == -1)
		{
			x = 32;
		}
		if (y == -1)
		{
			y = 16;
		}
		this.bounds = new Rectangle(x, y, width, height);
		this.label = label;
		this.whichOption = whichOption;
	}

	public OptionsElement(string label, Rectangle bounds, int whichOption)
	{
		this.whichOption = whichOption;
		this.label = label;
		this.bounds = bounds;
	}

	/// <summary>Handle a user left-click on the element (including a 'click' through controller selection).</summary>
	/// <param name="x">The pixel X coordinate that was clicked.</param>
	/// <param name="y">The pixel Y coordinate that was clicked.</param>
	public virtual void receiveLeftClick(int x, int y)
	{
	}

	/// <summary>Handle the left-click button being held down (including a button resulting in a 'click' through controller selection). This is called each tick that it's held.</summary>
	/// <param name="x">The cursor's current pixel X coordinate.</param>
	/// <param name="y">The cursor's current pixel Y coordinate.</param>
	public virtual void leftClickHeld(int x, int y)
	{
	}

	/// <summary>Handle the left-click button being released (including a button resulting in a 'click' through controller selection).</summary>
	/// <param name="x">The cursor's current pixel X coordinate.</param>
	/// <param name="y">The cursor's current pixel Y coordinate.</param>
	public virtual void leftClickReleased(int x, int y)
	{
	}

	/// <summary>Handle a keyboard button pressed.</summary>
	/// <param name="key">The keyboard button that was pressed.</param>
	public virtual void receiveKeyPress(Keys key)
	{
	}

	/// <summary>Render the element.</summary>
	/// <param name="b">The sprite batch being drawn.</param>
	/// <param name="slotX">The pixel X position at which to draw, relative to the bounds.</param>
	/// <param name="slotY">The pixel Y position at which to draw, relative to the bounds.</param>
	/// <param name="context">The menu which contains this element, if applicable.</param>
	public virtual void draw(SpriteBatch b, int slotX, int slotY, IClickableMenu context = null)
	{
		if (this.style == Style.OptionLabel)
		{
			Utility.drawTextWithShadow(b, this.label, Game1.dialogueFont, new Vector2(slotX + this.bounds.X + (int)this.labelOffset.X, slotY + this.bounds.Y + (int)this.labelOffset.Y + 12), this.greyedOut ? (Game1.textColor * 0.33f) : Game1.textColor, 1f, 0.1f);
			return;
		}
		if (this.whichOption == -1)
		{
			SpriteText.drawString(b, this.label, slotX + this.bounds.X + (int)this.labelOffset.X, slotY + this.bounds.Y + (int)this.labelOffset.Y + 56 - SpriteText.getHeightOfString(this.label), 999, -1, 999, 1f, 0.1f);
			return;
		}
		int label_start_x = slotX + this.bounds.X + this.bounds.Width + 8 + (int)this.labelOffset.X;
		int label_start_y = slotY + this.bounds.Y + (int)this.labelOffset.Y;
		string displayed_text = this.label;
		SpriteFont font = Game1.dialogueFont;
		if (context != null)
		{
			int max_width = context.width - 64;
			int menu_start_x = context.xPositionOnScreen;
			if (font.MeasureString(this.label).X + (float)label_start_x > (float)(max_width + menu_start_x))
			{
				int allowed_space = max_width + menu_start_x - label_start_x;
				font = Game1.smallFont;
				displayed_text = Game1.parseText(this.label, font, allowed_space);
				label_start_y -= (int)((font.MeasureString(displayed_text).Y - font.MeasureString("T").Y) / 2f);
			}
		}
		Utility.drawTextWithShadow(b, displayed_text, font, new Vector2(label_start_x, label_start_y), this.greyedOut ? (Game1.textColor * 0.33f) : Game1.textColor, 1f, 0.1f);
	}
}
