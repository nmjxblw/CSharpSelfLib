using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Extensions;

namespace StardewValley.Monsters;

public class DwarvishSentry : Monster
{
	[XmlIgnore]
	public new int yOffset;

	[XmlIgnore]
	public float pauseTimer;

	public DwarvishSentry()
	{
	}

	public DwarvishSentry(Vector2 position)
		: base("Dwarvish Sentry", position)
	{
		this.Sprite.SpriteHeight = 16;
		base.IsWalkingTowardPlayer = false;
		this.Sprite.UpdateSourceRect();
		base.HideShadow = true;
		base.isGlider.Value = true;
		base.Slipperiness = 1;
		this.pauseTimer = 10000f;
		DelayedAction.playSoundAfterDelay("DwarvishSentry", 500);
	}

	/// <inheritdoc />
	public override void reloadSprite(bool onlyAppearance = false)
	{
		this.Sprite = new AnimatedSprite("Characters\\Monsters\\Dwarvish Sentry");
	}

	public override int takeDamage(int damage, int xTrajectory, int yTrajectory, bool isBomb, double addedPrecision, Farmer who)
	{
		int actualDamage = Math.Max(1, damage - base.resilience.Value);
		if (Game1.random.NextDouble() < base.missChance.Value - base.missChance.Value * addedPrecision)
		{
			actualDamage = -1;
		}
		else
		{
			base.Health -= actualDamage;
			base.currentLocation?.playSound("clank");
			if (base.Health <= 0)
			{
				base.deathAnimation();
			}
		}
		return actualDamage;
	}

	protected override void localDeathAnimation()
	{
		base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(this.Sprite.textureName.Value, new Rectangle(0, 64, 16, 16), 70f, 7, 0, base.Position + new Vector2(0f, -32f), flicker: false, flipped: false)
		{
			scale = 4f
		});
		base.currentLocation.localSound("fireball");
		base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(362, 30f, 6, 1, base.Position + new Vector2(-16 + Game1.random.Next(64), Game1.random.Next(64) - 32), flicker: false, Game1.random.NextBool())
		{
			delayBeforeAnimationStart = 100
		});
		base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(362, 30f, 6, 1, base.Position + new Vector2(-16 + Game1.random.Next(64), Game1.random.Next(64) - 32), flicker: false, Game1.random.NextBool())
		{
			delayBeforeAnimationStart = 200
		});
		base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(362, 30f, 6, 1, base.Position + new Vector2(-16 + Game1.random.Next(64), Game1.random.Next(64) - 32), flicker: false, Game1.random.NextBool())
		{
			delayBeforeAnimationStart = 300
		});
		base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(362, 30f, 6, 1, base.Position + new Vector2(-16 + Game1.random.Next(64), Game1.random.Next(64) - 32), flicker: false, Game1.random.NextBool())
		{
			delayBeforeAnimationStart = 400
		});
	}

	public override void drawAboveAllLayers(SpriteBatch b)
	{
		b.Draw(Game1.mouseCursors, base.getLocalPosition(Game1.viewport) + new Vector2(50f, 80 + this.yOffset), new Rectangle(536 + (int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 350.0 / 70.0) * 8, 1945, 8, 8), Color.White * 0.75f, 0f, new Vector2(8f, 16f), 4f, SpriteEffects.FlipVertically, 0.99f - base.position.X / 10000f);
		b.Draw(this.Sprite.Texture, base.getLocalPosition(Game1.viewport) + new Vector2(32f, 21 + this.yOffset), this.Sprite.SourceRect, Color.White, 0f, new Vector2(8f, 16f), Math.Max(0.2f, base.scale.Value) * 4f, base.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 1f - base.position.X / 10000f);
		b.Draw(Game1.shadowTexture, base.getLocalPosition(Game1.viewport) + new Vector2(32f, 64f), Game1.shadowTexture.Bounds, Color.White, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 3f + (float)this.yOffset / 20f, SpriteEffects.None, (float)(base.StandingPixel.Y - 1) / 10000f);
	}

	protected override void updateAnimation(GameTime time)
	{
		base.updateAnimation(time);
		this.yOffset = (int)(Math.Sin((double)((float)time.TotalGameTime.Milliseconds / 2000f) * (Math.PI * 2.0)) * 7.0);
		if (this.Sprite.currentFrame % 4 != 0 && Game1.random.NextDouble() < 0.1)
		{
			this.Sprite.currentFrame -= this.Sprite.currentFrame % 4;
		}
		if (Game1.random.NextDouble() < 0.01)
		{
			this.Sprite.currentFrame++;
		}
		base.resetAnimationSpeed();
	}

	public override void behaviorAtGameTick(GameTime time)
	{
		base.behaviorAtGameTick(time);
		base.faceGeneralDirection(base.Player.Position);
		this.pauseTimer += (int)time.ElapsedGameTime.TotalMilliseconds;
		if (this.pauseTimer < 10000f)
		{
			this.setTrajectory(Utility.getVelocityTowardPoint(base.Position, base.Player.Position, 1f) * new Vector2(1f, -1f));
		}
		else if (Game1.random.NextDouble() < 0.01)
		{
			this.pauseTimer = Game1.random.Next(5000);
		}
	}
}
