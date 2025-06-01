using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace StardewValley.Menus;

public class OptionsDropDown : OptionsElement
{
	public const int pixelsHigh = 11;

	[InstancedStatic]
	public static OptionsDropDown selected;

	public List<string> dropDownOptions = new List<string>();

	public List<string> dropDownDisplayOptions = new List<string>();

	public int selectedOption;

	public int recentSlotY;

	public int startingSelected;

	private bool clicked;

	public Rectangle dropDownBounds;

	public static Rectangle dropDownBGSource = new Rectangle(433, 451, 3, 3);

	public static Rectangle dropDownButtonSource = new Rectangle(437, 450, 10, 11);

	public OptionsDropDown(string label, int whichOption, int x = -1, int y = -1)
		: base(label, x, y, (int)Game1.smallFont.MeasureString("Windowed Borderless Mode   ").X + 48, 44, whichOption)
	{
		Game1.options.setDropDownToProperValue(this);
		this.RecalculateBounds();
	}

	public virtual void RecalculateBounds()
	{
		foreach (string displayed_option in this.dropDownDisplayOptions)
		{
			float text_width = Game1.smallFont.MeasureString(displayed_option).X;
			if (text_width >= (float)(base.bounds.Width - 48))
			{
				base.bounds.Width = (int)(text_width + 64f);
			}
		}
		this.dropDownBounds = new Rectangle(base.bounds.X, base.bounds.Y, base.bounds.Width - 48, base.bounds.Height * this.dropDownOptions.Count);
	}

	/// <inheritdoc />
	public override void leftClickHeld(int x, int y)
	{
		if (!base.greyedOut)
		{
			base.leftClickHeld(x, y);
			this.clicked = true;
			this.dropDownBounds.Y = Math.Min(this.dropDownBounds.Y, Game1.uiViewport.Height - this.dropDownBounds.Height - this.recentSlotY);
			if (!Game1.options.SnappyMenus)
			{
				this.selectedOption = (int)Math.Max(Math.Min((float)(y - this.dropDownBounds.Y) / (float)base.bounds.Height, this.dropDownOptions.Count - 1), 0f);
			}
		}
	}

	/// <inheritdoc />
	public override void receiveLeftClick(int x, int y)
	{
		if (!base.greyedOut)
		{
			base.receiveLeftClick(x, y);
			this.startingSelected = this.selectedOption;
			if (!this.clicked)
			{
				Game1.playSound("shwip");
			}
			this.leftClickHeld(x, y);
			OptionsDropDown.selected = this;
		}
	}

	/// <inheritdoc />
	public override void leftClickReleased(int x, int y)
	{
		if (!base.greyedOut && this.dropDownOptions.Count > 0)
		{
			base.leftClickReleased(x, y);
			if (this.clicked)
			{
				Game1.playSound("drumkit6");
			}
			this.clicked = false;
			OptionsDropDown.selected = this;
			if (this.dropDownBounds.Contains(x, y) || (Game1.options.gamepadControls && !Game1.lastCursorMotionWasMouse))
			{
				Game1.options.changeDropDownOption(base.whichOption, this.dropDownOptions[this.selectedOption]);
			}
			else
			{
				this.selectedOption = this.startingSelected;
			}
			OptionsDropDown.selected = null;
		}
	}

	/// <inheritdoc />
	public override void receiveKeyPress(Keys key)
	{
		base.receiveKeyPress(key);
		if (!Game1.options.SnappyMenus || base.greyedOut)
		{
			return;
		}
		if (!this.clicked)
		{
			if (Game1.options.doesInputListContain(Game1.options.moveRightButton, key))
			{
				this.selectedOption++;
				if (this.selectedOption >= this.dropDownOptions.Count)
				{
					this.selectedOption = 0;
				}
				OptionsDropDown.selected = this;
				Game1.options.changeDropDownOption(base.whichOption, this.dropDownOptions[this.selectedOption]);
				OptionsDropDown.selected = null;
			}
			else if (Game1.options.doesInputListContain(Game1.options.moveLeftButton, key))
			{
				this.selectedOption--;
				if (this.selectedOption < 0)
				{
					this.selectedOption = this.dropDownOptions.Count - 1;
				}
				OptionsDropDown.selected = this;
				Game1.options.changeDropDownOption(base.whichOption, this.dropDownOptions[this.selectedOption]);
				OptionsDropDown.selected = null;
			}
		}
		else if (Game1.options.doesInputListContain(Game1.options.moveDownButton, key))
		{
			Game1.playSound("shiny4");
			this.selectedOption++;
			if (this.selectedOption >= this.dropDownOptions.Count)
			{
				this.selectedOption = 0;
			}
		}
		else if (Game1.options.doesInputListContain(Game1.options.moveUpButton, key))
		{
			Game1.playSound("shiny4");
			this.selectedOption--;
			if (this.selectedOption < 0)
			{
				this.selectedOption = this.dropDownOptions.Count - 1;
			}
		}
	}

	/// <inheritdoc />
	public override void draw(SpriteBatch b, int slotX, int slotY, IClickableMenu context = null)
	{
		this.recentSlotY = slotY;
		base.draw(b, slotX, slotY, context);
		float alpha = (base.greyedOut ? 0.33f : 1f);
		if (this.clicked)
		{
			IClickableMenu.drawTextureBox(b, Game1.mouseCursors, OptionsDropDown.dropDownBGSource, slotX + this.dropDownBounds.X, slotY + this.dropDownBounds.Y, this.dropDownBounds.Width, this.dropDownBounds.Height, Color.White * alpha, 4f, drawShadow: false, 0.97f);
			for (int i = 0; i < this.dropDownDisplayOptions.Count; i++)
			{
				if (i == this.selectedOption)
				{
					b.Draw(Game1.staminaRect, new Rectangle(slotX + this.dropDownBounds.X, slotY + this.dropDownBounds.Y + i * base.bounds.Height, this.dropDownBounds.Width, base.bounds.Height), new Rectangle(0, 0, 1, 1), Color.Wheat, 0f, Vector2.Zero, SpriteEffects.None, 0.975f);
				}
				b.DrawString(Game1.smallFont, this.dropDownDisplayOptions[i], new Vector2(slotX + this.dropDownBounds.X + 4, slotY + this.dropDownBounds.Y + 8 + base.bounds.Height * i), Game1.textColor * alpha, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.98f);
			}
			b.Draw(Game1.mouseCursors, new Vector2(slotX + base.bounds.X + base.bounds.Width - 48, slotY + base.bounds.Y), OptionsDropDown.dropDownButtonSource, Color.Wheat * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.981f);
		}
		else
		{
			IClickableMenu.drawTextureBox(b, Game1.mouseCursors, OptionsDropDown.dropDownBGSource, slotX + base.bounds.X, slotY + base.bounds.Y, base.bounds.Width - 48, base.bounds.Height, Color.White * alpha, 4f, drawShadow: false);
			b.DrawString(Game1.smallFont, (this.selectedOption < this.dropDownDisplayOptions.Count && this.selectedOption >= 0) ? this.dropDownDisplayOptions[this.selectedOption] : "", new Vector2(slotX + base.bounds.X + 4, slotY + base.bounds.Y + 8), Game1.textColor * alpha, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.88f);
			b.Draw(Game1.mouseCursors, new Vector2(slotX + base.bounds.X + base.bounds.Width - 48, slotY + base.bounds.Y), OptionsDropDown.dropDownButtonSource, Color.White * alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
		}
	}
}
