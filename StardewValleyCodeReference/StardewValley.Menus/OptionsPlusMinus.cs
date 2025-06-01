using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace StardewValley.Menus;

public class OptionsPlusMinus : OptionsElement
{
	public const int pixelsWide = 7;

	public List<string> options = new List<string>();

	public List<string> displayOptions = new List<string>();

	public int selected;

	public bool isChecked;

	[InstancedStatic]
	public static bool snapZoomPlus;

	[InstancedStatic]
	public static bool snapZoomMinus;

	public Rectangle minusButton;

	public Rectangle plusButton;

	public static Rectangle minusButtonSource = new Rectangle(177, 345, 7, 8);

	public static Rectangle plusButtonSource = new Rectangle(184, 345, 7, 8);

	public OptionsPlusMinus(string label, int whichOption, List<string> options, List<string> displayOptions, int x = -1, int y = -1)
		: base(label, x, y, 28, 28, whichOption)
	{
		this.options = options;
		this.displayOptions = displayOptions;
		Game1.options.setPlusMinusToProperValue(this);
		if (x == -1)
		{
			x = 32;
		}
		if (y == -1)
		{
			y = 16;
		}
		int txtSize = (int)Game1.dialogueFont.MeasureString(options[0]).X + 28;
		foreach (string displayOption in displayOptions)
		{
			txtSize = Math.Max((int)Game1.dialogueFont.MeasureString(displayOption).X + 28, txtSize);
		}
		base.bounds = new Rectangle(x, y, 56 + txtSize, 32);
		base.label = label;
		base.whichOption = whichOption;
		this.minusButton = new Rectangle(x, 16, 28, 32);
		this.plusButton = new Rectangle(base.bounds.Right - 32, 16, 28, 32);
	}

	/// <inheritdoc />
	public override void receiveLeftClick(int x, int y)
	{
		if (!base.greyedOut && this.options.Count > 0)
		{
			int num = this.selected;
			if (this.minusButton.Contains(x, y) && this.selected != 0)
			{
				this.selected--;
				OptionsPlusMinus.snapZoomMinus = true;
				Game1.playSound("drumkit6");
			}
			else if (this.plusButton.Contains(x, y) && this.selected != this.options.Count - 1)
			{
				this.selected++;
				OptionsPlusMinus.snapZoomPlus = true;
				Game1.playSound("drumkit6");
			}
			if (this.selected < 0)
			{
				this.selected = 0;
			}
			else if (this.selected >= this.options.Count)
			{
				this.selected = this.options.Count - 1;
			}
			if (num != this.selected)
			{
				Game1.options.changeDropDownOption(base.whichOption, this.options[this.selected]);
			}
		}
	}

	/// <inheritdoc />
	public override void receiveKeyPress(Keys key)
	{
		base.receiveKeyPress(key);
		if (Game1.options.snappyMenus && Game1.options.gamepadControls)
		{
			if (Game1.options.doesInputListContain(Game1.options.moveRightButton, key))
			{
				this.receiveLeftClick(this.plusButton.Center.X, this.plusButton.Center.Y);
			}
			else if (Game1.options.doesInputListContain(Game1.options.moveLeftButton, key))
			{
				this.receiveLeftClick(this.minusButton.Center.X, this.minusButton.Center.Y);
			}
		}
	}

	/// <inheritdoc />
	public override void draw(SpriteBatch b, int slotX, int slotY, IClickableMenu context = null)
	{
		b.Draw(Game1.mouseCursors, new Vector2(slotX + this.minusButton.X, slotY + this.minusButton.Y), OptionsPlusMinus.minusButtonSource, Color.White * (base.greyedOut ? 0.33f : 1f) * ((this.selected == 0) ? 0.5f : 1f), 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.4f);
		b.DrawString(Game1.dialogueFont, (this.selected < this.displayOptions.Count && this.selected != -1) ? this.displayOptions[this.selected] : "", new Vector2(slotX + this.minusButton.X + this.minusButton.Width + 4, slotY + this.minusButton.Y), Game1.textColor);
		b.Draw(Game1.mouseCursors, new Vector2(slotX + this.plusButton.X, slotY + this.plusButton.Y), OptionsPlusMinus.plusButtonSource, Color.White * (base.greyedOut ? 0.33f : 1f) * ((this.selected == this.displayOptions.Count - 1) ? 0.5f : 1f), 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.4f);
		if (!Game1.options.snappyMenus && Game1.options.gamepadControls)
		{
			if (OptionsPlusMinus.snapZoomMinus)
			{
				Game1.setMousePosition(slotX + this.minusButton.Center.X, slotY + this.minusButton.Center.Y);
				OptionsPlusMinus.snapZoomMinus = false;
			}
			else if (OptionsPlusMinus.snapZoomPlus)
			{
				Game1.setMousePosition(slotX + this.plusButton.Center.X, slotY + this.plusButton.Center.Y);
				OptionsPlusMinus.snapZoomPlus = false;
			}
		}
		base.draw(b, slotX, slotY, context);
	}
}
