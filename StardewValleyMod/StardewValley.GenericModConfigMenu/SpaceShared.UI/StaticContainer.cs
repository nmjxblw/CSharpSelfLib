using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Menus;

namespace SpaceShared.UI;

internal class StaticContainer : Container
{
	public Vector2 Size { get; set; }

	public Color? OutlineColor { get; set; }

	public override int Width => (int)this.Size.X;

	public override int Height => (int)this.Size.Y;

	public override void Draw(SpriteBatch b)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		if (!base.IsHidden())
		{
			if (this.OutlineColor.HasValue)
			{
				IClickableMenu.drawTextureBox(b, (int)base.Position.X - 12, (int)base.Position.Y - 12, this.Width + 24, this.Height + 24, this.OutlineColor.Value);
			}
			base.Draw(b);
		}
	}
}
