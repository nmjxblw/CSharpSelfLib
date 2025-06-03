using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;

namespace SpaceShared.UI;

internal class ItemWithBorder : Element
{
	public static ItemWithBorder HoveredElement { get; private set; }

	public Item ItemDisplay { get; set; }

	public bool TransparentItemDisplay { get; set; }

	public Color? BoxColor { get; set; } = Color.White;

	public bool BoxIsThin { get; set; }

	public Action<Element> Callback { get; set; }

	public Action<Element> SecondaryCallback { get; set; }

	public override int Width => 64 + ((!this.BoxIsThin) ? 16 : 0) * 2;

	public override int Height => 64 + ((!this.BoxIsThin) ? 16 : 0) * 2;

	public override void Update(bool hidden = false)
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Invalid comparison between Unknown and I4
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Invalid comparison between Unknown and I4
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		base.Update(hidden);
		if (base.Hover)
		{
			ItemWithBorder.HoveredElement = this;
		}
		else if (ItemWithBorder.HoveredElement == this)
		{
			ItemWithBorder.HoveredElement = null;
		}
		if (base.Clicked && this.Callback != null)
		{
			this.Callback(this);
		}
		MouseState mouseState = Game1.input.GetMouseState();
		int num;
		if ((int)((MouseState)(ref mouseState)).RightButton != 1 || (int)((MouseState)(ref Game1.oldMouseState)).RightButton != 0)
		{
			if (Game1.options.gamepadControls)
			{
				GamePadState gamePadState = Game1.input.GetGamePadState();
				num = ((((GamePadState)(ref gamePadState)).IsButtonDown((Buttons)8192) && !((GamePadState)(ref Game1.oldPadState)).IsButtonDown((Buttons)8192)) ? 1 : 0);
			}
			else
			{
				num = 0;
			}
		}
		else
		{
			num = 1;
		}
		bool SecondaryClickGestured = (byte)num != 0;
		if (base.Hover && SecondaryClickGestured && this.SecondaryCallback != null)
		{
			this.SecondaryCallback(this);
		}
	}

	public override void Draw(SpriteBatch b)
	{
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		if (this.BoxColor.HasValue)
		{
			if (this.BoxIsThin)
			{
				b.Draw(Game1.menuTexture, base.Position, (Rectangle?)Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 10, -1, -1), this.BoxColor.Value, 0f, Vector2.Zero, 1f, (SpriteEffects)0, 0.5f);
			}
			else
			{
				IClickableMenu.drawTextureBox(b, (int)base.Position.X, (int)base.Position.Y, this.Width, this.Height, this.BoxColor.Value);
			}
		}
		if (this.ItemDisplay != null)
		{
			this.ItemDisplay.drawInMenu(b, base.Position + (Vector2)(this.BoxIsThin ? Vector2.Zero : new Vector2(16f, 16f)), 1f, this.TransparentItemDisplay ? 0.5f : 1f, 1f);
		}
	}
}
