using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Extensions;
using xTile.Dimensions;

namespace StardewValley.BellsAndWhistles;

public class CrabCritter : Critter
{
	public Microsoft.Xna.Framework.Rectangle movementRectangle;

	public float nextCharacterCheck = 2f;

	public float nextFrameChange;

	public float nextMovementChange;

	public bool moving;

	public bool diving;

	public bool skittering;

	protected float skitterTime = 5f;

	protected Microsoft.Xna.Framework.Rectangle _baseSourceRectangle = new Microsoft.Xna.Framework.Rectangle(0, 272, 18, 18);

	protected int _currentFrame;

	protected int _crabVariant;

	protected Vector2 movementDirection = Vector2.Zero;

	public Microsoft.Xna.Framework.Rectangle movementBounds;

	public CrabCritter()
	{
		base.sprite = new AnimatedSprite(Critter.critterTexture, 0, 18, 18);
		base.sprite.SourceRect = this._baseSourceRectangle;
		base.sprite.ignoreSourceRectUpdates = true;
		this._crabVariant = 1;
		this.UpdateSpriteRectangle();
	}

	public CrabCritter(Vector2 start_position)
		: this()
	{
		base.position = start_position;
		float movement_rectangle_width = 256f;
		this.movementBounds = new Microsoft.Xna.Framework.Rectangle((int)(start_position.X - movement_rectangle_width / 2f), (int)start_position.Y, (int)movement_rectangle_width, 0);
	}

	public override bool update(GameTime time, GameLocation environment)
	{
		this.nextFrameChange -= (float)time.ElapsedGameTime.TotalSeconds;
		if (this.skittering)
		{
			this.skitterTime -= (float)time.ElapsedGameTime.TotalSeconds;
		}
		if (this.nextFrameChange <= 0f && (this.moving || this.skittering))
		{
			this._currentFrame++;
			if (this._currentFrame >= 4)
			{
				this._currentFrame = 0;
			}
			if (this.skittering)
			{
				this.nextFrameChange = Utility.RandomFloat(0.025f, 0.05f);
			}
			else
			{
				this.nextFrameChange = Utility.RandomFloat(0.05f, 0.15f);
			}
		}
		if (this.skittering)
		{
			if (base.yJumpOffset >= 0f)
			{
				if (!this.diving)
				{
					if (Game1.random.Next(0, 4) == 0)
					{
						base.gravityAffectedDY = -4f;
					}
					else
					{
						base.gravityAffectedDY = -2f;
					}
				}
				else
				{
					if (environment.isWaterTile((int)base.position.X / 64, (int)base.position.Y / 64))
					{
						environment.TemporarySprites.Add(new TemporaryAnimatedSprite(28, 50f, 2, 1, base.position, flicker: false, flipped: false));
						Game1.playSound("dropItemInWater");
						return true;
					}
					base.gravityAffectedDY = -4f;
				}
			}
		}
		else
		{
			this.nextCharacterCheck -= (float)time.ElapsedGameTime.TotalSeconds;
			if (this.nextCharacterCheck <= 0f)
			{
				Character f = Utility.isThereAFarmerOrCharacterWithinDistance(base.position / 64f, 7, environment);
				if (f != null)
				{
					this._crabVariant = 0;
					this.skittering = true;
					if (f.position.X > base.position.X)
					{
						this.movementDirection.X = -3f;
					}
					else
					{
						this.movementDirection.X = 3f;
					}
				}
				this.nextCharacterCheck = 0.25f;
			}
			if (!this.skittering)
			{
				if (this.moving && base.yJumpOffset >= 0f)
				{
					base.gravityAffectedDY = -1f;
				}
				this.nextMovementChange -= (float)time.ElapsedGameTime.TotalSeconds;
				if (this.nextMovementChange <= 0f)
				{
					this.moving = !this.moving;
					if (this.moving)
					{
						if (!Game1.random.NextBool())
						{
							this.movementDirection.X = 1f;
						}
						else
						{
							this.movementDirection.X = -1f;
						}
					}
					else
					{
						this.movementDirection = Vector2.Zero;
					}
					if (this.moving)
					{
						this.nextMovementChange = Utility.RandomFloat(0.15f, 0.5f);
					}
					else
					{
						this.nextMovementChange = Utility.RandomFloat(0.2f, 1f);
					}
				}
			}
		}
		base.position += this.movementDirection;
		if (!this.diving && !environment.isTilePassable(new Location((int)(base.position.X / 64f), (int)(base.position.Y / 64f)), Game1.viewport))
		{
			base.position -= this.movementDirection;
			this.movementDirection *= -1f;
		}
		if (!this.skittering)
		{
			if (base.position.X < (float)this.movementBounds.Left)
			{
				base.position.X = this.movementBounds.Left;
				this.movementDirection *= -1f;
			}
			if (base.position.X > (float)this.movementBounds.Right)
			{
				base.position.X = this.movementBounds.Right;
				this.movementDirection *= -1f;
			}
		}
		else if (!this.diving && environment.isWaterTile((int)(base.position.X / 64f + (float)Math.Sign(this.movementDirection.X) * 1f), (int)base.position.Y / 64))
		{
			if (base.yJumpOffset >= 0f)
			{
				base.gravityAffectedDY = -7f;
			}
			this.diving = true;
		}
		this.UpdateSpriteRectangle();
		if (this.skitterTime <= 0f)
		{
			return true;
		}
		return base.update(time, environment);
	}

	public virtual void UpdateSpriteRectangle()
	{
		Microsoft.Xna.Framework.Rectangle source_rectangle = this._baseSourceRectangle;
		source_rectangle.Y += this._crabVariant * 18;
		int drawn_frame = this._currentFrame;
		if (drawn_frame == 3)
		{
			drawn_frame = 1;
		}
		source_rectangle.X += drawn_frame * 18;
		base.sprite.SourceRect = source_rectangle;
	}

	public override void draw(SpriteBatch b)
	{
		float alpha = this.skitterTime;
		if (alpha > 1f)
		{
			alpha = 1f;
		}
		if (alpha < 0f)
		{
			alpha = 0f;
		}
		base.sprite.draw(b, Game1.GlobalToLocal(Game1.viewport, Utility.snapDrawPosition(base.position + new Vector2(0f, -20f + base.yJumpOffset + base.yOffset))), (base.position.Y + 64f - 32f) / 10000f, 0, 0, Color.White * alpha, base.flip, 4f);
		b.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, base.position + new Vector2(32f, 40f)), Game1.shadowTexture.Bounds, Color.White * alpha, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 3f + Math.Max(-3f, (base.yJumpOffset + base.yOffset) / 16f), SpriteEffects.None, (base.position.Y - 1f) / 10000f);
	}

	public override void drawAboveFrontLayer(SpriteBatch b)
	{
	}
}
