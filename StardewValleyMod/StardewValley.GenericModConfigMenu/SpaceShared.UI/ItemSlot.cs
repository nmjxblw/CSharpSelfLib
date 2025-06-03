using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace SpaceShared.UI;

internal class ItemSlot : ItemWithBorder
{
	public Item Item { get; set; }

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
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		if (base.BoxColor.HasValue)
		{
			if (base.BoxIsThin)
			{
				b.Draw(Game1.menuTexture, base.Position, (Rectangle?)Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 10, -1, -1), base.BoxColor.Value, 0f, Vector2.Zero, 1f, (SpriteEffects)0, 0.5f);
			}
			else
			{
				IClickableMenu.drawTextureBox(b, (int)base.Position.X, (int)base.Position.Y, this.Width, this.Height, base.BoxColor.Value);
			}
		}
		if (this.Item != null)
		{
			this.Item.drawInMenu(b, base.Position + (Vector2)(base.BoxIsThin ? Vector2.Zero : new Vector2(16f, 16f)), 1f, 1f, 1f);
		}
		else if (base.ItemDisplay != null)
		{
			base.ItemDisplay.drawInMenu(b, base.Position + (Vector2)(base.BoxIsThin ? Vector2.Zero : new Vector2(16f, 16f)), 1f, base.TransparentItemDisplay ? 0.5f : 1f, 1f);
		}
	}
}
