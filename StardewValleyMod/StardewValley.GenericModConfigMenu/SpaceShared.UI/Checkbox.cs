using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace SpaceShared.UI;

internal class Checkbox : Element, ISingleTexture
{
	public Texture2D Texture { get; set; }

	public Rectangle CheckedTextureRect { get; set; }

	public Rectangle UncheckedTextureRect { get; set; }

	public Action<Element> Callback { get; set; }

	public bool Checked { get; set; } = true;

	public override int Width => this.CheckedTextureRect.Width * 4;

	public override int Height => this.CheckedTextureRect.Height * 4;

	public override string ClickedSound => "drumkit6";

	public Checkbox()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		this.Texture = Game1.mouseCursors;
		this.CheckedTextureRect = OptionsCheckbox.sourceRectChecked;
		this.UncheckedTextureRect = OptionsCheckbox.sourceRectUnchecked;
	}

	public override void Update(bool isOffScreen = false)
	{
		base.Update(isOffScreen);
		if (base.Clicked && this.Callback != null)
		{
			this.Checked = !this.Checked;
			this.Callback(this);
		}
	}

	public override void Draw(SpriteBatch b)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		if (!base.IsHidden())
		{
			b.Draw(this.Texture, base.Position, (Rectangle?)(this.Checked ? this.CheckedTextureRect : this.UncheckedTextureRect), Color.White, 0f, Vector2.Zero, 4f, (SpriteEffects)0, 0f);
			IClickableMenu activeClickableMenu = Game1.activeClickableMenu;
			if (activeClickableMenu != null)
			{
				activeClickableMenu.drawMouse(b, false, -1);
			}
		}
	}
}
