using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley.BellsAndWhistles;

public abstract class Critter
{
	public const int spriteWidth = 32;

	public const int spriteHeight = 32;

	public const float gravity = 0.25f;

	public static string critterTexture = "TileSheets\\critters";

	public Vector2 position;

	public Vector2 startingPosition;

	public int baseFrame;

	public AnimatedSprite sprite;

	public bool flip;

	public float gravityAffectedDY;

	public float yOffset;

	public float yJumpOffset;

	public Critter()
	{
	}

	public Critter(int baseFrame, Vector2 position)
	{
		this.baseFrame = baseFrame;
		this.position = position;
		this.sprite = new AnimatedSprite(Critter.critterTexture, baseFrame, 32, 32);
		this.startingPosition = position;
	}

	public virtual Rectangle getBoundingBox(int xOffset, int yOffset)
	{
		return new Rectangle((int)this.position.X - 32 + xOffset, (int)this.position.Y - 16 + yOffset, 64, 32);
	}

	public virtual bool update(GameTime time, GameLocation environment)
	{
		this.sprite.animateOnce(time);
		if (this.gravityAffectedDY < 0f || this.yJumpOffset < 0f)
		{
			this.yJumpOffset += this.gravityAffectedDY;
			this.gravityAffectedDY += 0.25f;
		}
		if (this.position.X < -128f || this.position.Y < -128f || this.position.X > (float)environment.map.DisplayWidth || this.position.Y > (float)environment.map.DisplayHeight)
		{
			return true;
		}
		return false;
	}

	public virtual void draw(SpriteBatch b)
	{
		if (this.sprite != null)
		{
			this.sprite.draw(b, Game1.GlobalToLocal(Game1.viewport, this.position + new Vector2(-64f, -128f + this.yJumpOffset + this.yOffset)), this.position.Y / 10000f + this.position.X / 1000000f, 0, 0, Color.White, this.flip, 4f);
			b.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, this.position + new Vector2(0f, -4f)), Game1.shadowTexture.Bounds, Color.White * (1f - Math.Min(1f, Math.Abs((this.yJumpOffset + this.yOffset) / 64f))), 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 3f + Math.Max(-3f, (this.yJumpOffset + this.yOffset) / 64f), SpriteEffects.None, (this.position.Y - 1f) / 10000f);
		}
	}

	public virtual void drawAboveFrontLayer(SpriteBatch b)
	{
	}

	/// <summary>Auto-generate a default light source ID for this critter.</summary>
	/// <param name="identifier">A unique ID for this critter instance.</param>
	protected virtual string GenerateLightSourceId(int identifier)
	{
		return $"{base.GetType().Name}_{identifier}";
	}
}
