using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Extensions;

namespace StardewValley.BellsAndWhistles;

public class Cloud : Critter
{
	public const int width = 147;

	public const int height = 100;

	public int zoom = 5;

	private bool verticalFlip;

	private bool horizontalFlip;

	public Cloud()
	{
	}

	public Cloud(Vector2 position)
	{
		base.position = position * 64f;
		base.startingPosition = position;
		this.verticalFlip = Game1.random.NextBool();
		this.horizontalFlip = Game1.random.NextBool();
		this.zoom = Game1.random.Next(4, 7);
	}

	public override bool update(GameTime time, GameLocation environment)
	{
		base.position.Y -= (float)time.ElapsedGameTime.TotalMilliseconds * 0.02f;
		base.position.X -= (float)time.ElapsedGameTime.TotalMilliseconds * 0.02f;
		if (base.position.X < (float)(-147 * this.zoom) || base.position.Y < (float)(-100 * this.zoom))
		{
			return true;
		}
		return false;
	}

	public override Rectangle getBoundingBox(int xOffset, int yOffset)
	{
		return new Rectangle((int)base.position.X, (int)base.position.Y, 147 * this.zoom, 100 * this.zoom);
	}

	public override void draw(SpriteBatch b)
	{
	}

	public override void drawAboveFrontLayer(SpriteBatch b)
	{
		b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(base.position), new Rectangle(128, 0, 146, 99), Color.White, (this.verticalFlip && this.horizontalFlip) ? ((float)Math.PI) : 0f, Vector2.Zero, this.zoom, (this.verticalFlip && !this.horizontalFlip) ? SpriteEffects.FlipVertically : ((this.horizontalFlip && !this.verticalFlip) ? SpriteEffects.FlipHorizontally : SpriteEffects.None), 1f);
	}
}
