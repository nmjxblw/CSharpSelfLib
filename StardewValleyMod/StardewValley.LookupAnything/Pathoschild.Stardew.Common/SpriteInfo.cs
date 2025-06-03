using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pathoschild.Stardew.Common;

internal class SpriteInfo
{
	public Texture2D Spritesheet { get; }

	public Rectangle SourceRectangle { get; }

	public SpriteInfo(Texture2D spritesheet, Rectangle sourceRectangle)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		this.Spritesheet = spritesheet;
		this.SourceRectangle = sourceRectangle;
	}

	public virtual void Draw(SpriteBatch spriteBatch, int x, int y, Vector2 size, Color? color = null)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		spriteBatch.DrawSpriteWithin(this.Spritesheet, this.SourceRectangle, x, y, size, (Color)(((_003F?)color) ?? Color.White));
	}
}
