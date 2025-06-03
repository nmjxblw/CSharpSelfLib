using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley.Menus;

public class OptionsPlusMinusButton : OptionsPlusMinus
{
	protected Rectangle _buttonBounds;

	protected Rectangle _buttonRect;

	protected Texture2D _buttonTexture;

	protected Action<string> _buttonAction;

	public OptionsPlusMinusButton(string label, int whichOptions, List<string> options, List<string> displayOptions, Texture2D buttonTexture, Rectangle buttonRect, Action<string> buttonAction, int x = -1, int y = -1)
		: base(label, whichOptions, options, displayOptions, x, y)
	{
		this._buttonRect = buttonRect;
		this._buttonBounds = new Rectangle(base.bounds.Left, 4 - this._buttonRect.Height / 2 + 8, this._buttonRect.Width * 4, this._buttonRect.Height * 4);
		this._buttonTexture = buttonTexture;
		this._buttonAction = buttonAction;
		int offset = 8;
		base.plusButton.X += this._buttonBounds.Width + offset * 4;
		base.minusButton.X += this._buttonBounds.Width + offset * 4;
		base.bounds.Width += this._buttonBounds.Width + offset * 4;
		int height_adjustment = this._buttonBounds.Height - base.bounds.Height;
		if (height_adjustment > 0)
		{
			base.bounds.Y -= height_adjustment / 2;
			base.bounds.Height += height_adjustment;
			base.labelOffset.Y += height_adjustment / 2;
		}
	}

	/// <inheritdoc />
	public override void draw(SpriteBatch b, int slotX, int slotY, IClickableMenu context = null)
	{
		b.Draw(this._buttonTexture, new Vector2(slotX + this._buttonBounds.X, slotY + this._buttonBounds.Y), this._buttonRect, Color.White * (base.greyedOut ? 0.33f : 1f), 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.4f);
		base.draw(b, slotX, slotY, context);
	}

	/// <inheritdoc />
	public override void receiveLeftClick(int x, int y)
	{
		if (!base.greyedOut && this._buttonBounds.Contains(x, y))
		{
			if (this._buttonAction != null)
			{
				string selection = "";
				if (base.selected >= 0 && base.selected < base.options.Count)
				{
					selection = base.options[base.selected];
				}
				this._buttonAction(selection);
			}
		}
		else
		{
			base.receiveLeftClick(x, y);
		}
	}
}
