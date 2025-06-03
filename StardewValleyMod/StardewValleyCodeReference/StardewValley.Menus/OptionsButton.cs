using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley.Menus;

public class OptionsButton : OptionsElement
{
	private Action action;

	public OptionsButton(string label, Action action)
		: base(label)
	{
		this.action = action;
		int width = (int)Game1.dialogueFont.MeasureString(label).X + 64;
		int height = 68;
		base.bounds = new Rectangle(32, 0, width, height);
	}

	/// <inheritdoc />
	public override void receiveLeftClick(int x, int y)
	{
		if (!base.greyedOut && base.bounds.Contains(x, y) && this.action != null)
		{
			this.action();
		}
		base.receiveLeftClick(x, y);
	}

	/// <inheritdoc />
	public override void draw(SpriteBatch b, int slotX, int slotY, IClickableMenu context = null)
	{
		float draw_layer = 0.8f - (float)(slotY + base.bounds.Y) * 1E-06f;
		IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(432, 439, 9, 9), slotX + base.bounds.X, slotY + base.bounds.Y, base.bounds.Width, base.bounds.Height, Color.White * (base.greyedOut ? 0.33f : 1f), 4f, drawShadow: true, draw_layer);
		Vector2 string_center = Game1.dialogueFont.MeasureString(base.label) / 2f;
		string_center.X = (int)(string_center.X / 4f) * 4;
		string_center.Y = (int)(string_center.Y / 4f) * 4;
		Utility.drawTextWithShadow(b, base.label, Game1.dialogueFont, new Vector2(slotX + base.bounds.Center.X, slotY + base.bounds.Center.Y) - string_center, Game1.textColor * (base.greyedOut ? 0.33f : 1f), 1f, draw_layer + 1E-06f, -1, -1, 0f);
	}
}
