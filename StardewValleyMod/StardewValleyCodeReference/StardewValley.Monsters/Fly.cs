using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Extensions;

namespace StardewValley.Monsters;

public class Fly : Monster
{
	public const float rotationIncrement = (float)Math.PI / 64f;

	public const int volumeTileRange = 16;

	public const int spawnTime = 1000;

	[XmlIgnore]
	public int spawningCounter = 1000;

	[XmlIgnore]
	public int wasHitCounter;

	[XmlIgnore]
	public float targetRotation;

	public static ICue buzz;

	[XmlIgnore]
	public bool turningRight;

	public bool hard;

	public Fly()
	{
	}

	public Fly(Vector2 position)
		: this(position, hard: false)
	{
	}

	public Fly(Vector2 position, bool hard)
		: base("Fly", position)
	{
		base.Slipperiness = 24 + Game1.random.Next(-10, 10);
		this.Halt();
		base.IsWalkingTowardPlayer = false;
		this.hard = hard;
		if (hard)
		{
			base.DamageToFarmer *= 2;
			base.MaxHealth *= 3;
			base.Health = base.MaxHealth;
		}
		base.HideShadow = true;
	}

	public void setHard()
	{
		this.hard = true;
		if (this.hard)
		{
			base.DamageToFarmer = 12;
			base.MaxHealth = 66;
			base.Health = base.MaxHealth;
		}
	}

	/// <inheritdoc />
	public override void reloadSprite(bool onlyAppearance = false)
	{
		this.Sprite = new AnimatedSprite("Characters\\Monsters\\Fly");
		base.HideShadow = true;
		if (!onlyAppearance)
		{
			Fly.buzz = Game1.soundBank.GetCue("flybuzzing");
		}
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
			base.setTrajectory(xTrajectory / 3, yTrajectory / 3);
			this.wasHitCounter = 500;
			base.currentLocation?.playSound("hitEnemy");
			if (base.Health <= 0)
			{
				if (base.currentLocation != null)
				{
					base.currentLocation.playSound("monsterdead");
					Utility.makeTemporarySpriteJuicier(new TemporaryAnimatedSprite(44, base.Position, Color.HotPink, 10)
					{
						interval = 70f
					}, base.currentLocation);
				}
				Fly.buzz?.Stop(AudioStopOptions.AsAuthored);
			}
		}
		this.addedSpeed = Game1.random.Next(-1, 1);
		return actualDamage;
	}

	public override void drawAboveAllLayers(SpriteBatch b)
	{
		if (Utility.isOnScreen(base.Position, 128))
		{
			int boundsHeight = this.GetBoundingBox().Height;
			int standingY = base.StandingPixel.Y;
			b.Draw(this.Sprite.Texture, base.getLocalPosition(Game1.viewport) + new Vector2(32f, boundsHeight / 2 - 32), this.Sprite.SourceRect, this.hard ? Color.Lime : Color.White, base.rotation, new Vector2(8f, 16f), Math.Max(0.2f, base.scale.Value) * 4f, base.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, base.drawOnTop ? 0.991f : ((float)(standingY + 8) / 10000f)));
			b.Draw(Game1.shadowTexture, base.getLocalPosition(Game1.viewport) + new Vector2(32f, boundsHeight / 2), Game1.shadowTexture.Bounds, Color.White, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 4f, SpriteEffects.None, (float)(standingY - 1) / 10000f);
			if (base.isGlowing)
			{
				b.Draw(this.Sprite.Texture, base.getLocalPosition(Game1.viewport) + new Vector2(32f, boundsHeight / 2 - 32), this.Sprite.SourceRect, base.glowingColor * base.glowingTransparency, base.rotation, new Vector2(8f, 16f), Math.Max(0.2f, base.scale.Value) * 4f, base.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, base.drawOnTop ? 0.99f : ((float)standingY / 10000f + 0.001f)));
			}
		}
	}

	public override void drawAboveAlwaysFrontLayer(SpriteBatch b)
	{
		if (base.currentLocation != null && base.currentLocation.treatAsOutdoors.Value)
		{
			this.drawAboveAllLayers(b);
		}
	}

	protected override void updateAnimation(GameTime time)
	{
		if ((Fly.buzz == null || !Fly.buzz.IsPlaying) && (base.currentLocation == null || base.currentLocation.Equals(Game1.currentLocation)))
		{
			Game1.playSound("flybuzzing", out Fly.buzz);
			Fly.buzz.SetVariable("Volume", 0f);
		}
		if ((double)Game1.fadeToBlackAlpha > 0.8 && Game1.fadeIn && Fly.buzz != null)
		{
			Fly.buzz.Stop(AudioStopOptions.AsAuthored);
		}
		else if (Fly.buzz != null)
		{
			Fly.buzz.SetVariable("Volume", Math.Max(0f, Fly.buzz.GetVariable("Volume") - 1f));
			float volume = Math.Max(0f, 100f - Vector2.Distance(base.Position, base.Player.Position) / 64f / 16f * 100f);
			if (volume > Fly.buzz.GetVariable("Volume"))
			{
				Fly.buzz.SetVariable("Volume", volume);
			}
		}
		if (this.wasHitCounter >= 0)
		{
			this.wasHitCounter -= time.ElapsedGameTime.Milliseconds;
		}
		this.Sprite.Animate(time, (this.FacingDirection == 0) ? 8 : ((this.FacingDirection != 2) ? (this.FacingDirection * 4) : 0), 4, 75f);
		if (this.spawningCounter >= 0)
		{
			this.spawningCounter -= time.ElapsedGameTime.Milliseconds;
			base.Scale = 1f - (float)this.spawningCounter / 1000f;
		}
		else if ((this.withinPlayerThreshold() || Utility.isOnScreen(base.Position, 256)) && base.invincibleCountdown <= 0)
		{
			this.faceDirection(0);
			Point monsterPixel = base.StandingPixel;
			Point standingPixel = base.Player.StandingPixel;
			float xSlope = -(standingPixel.X - monsterPixel.X);
			float ySlope = standingPixel.Y - monsterPixel.Y;
			float t = Math.Max(1f, Math.Abs(xSlope) + Math.Abs(ySlope));
			if (t < 64f)
			{
				base.xVelocity = Math.Max(-7f, Math.Min(7f, base.xVelocity * 1.1f));
				base.yVelocity = Math.Max(-7f, Math.Min(7f, base.yVelocity * 1.1f));
			}
			xSlope /= t;
			ySlope /= t;
			if (this.wasHitCounter <= 0)
			{
				this.targetRotation = (float)Math.Atan2(0f - ySlope, xSlope) - (float)Math.PI / 2f;
				if ((double)(Math.Abs(this.targetRotation) - Math.Abs(base.rotation)) > Math.PI * 7.0 / 8.0 && Game1.random.NextBool())
				{
					this.turningRight = true;
				}
				else if ((double)(Math.Abs(this.targetRotation) - Math.Abs(base.rotation)) < Math.PI / 8.0)
				{
					this.turningRight = false;
				}
				if (this.turningRight)
				{
					base.rotation -= (float)Math.Sign(this.targetRotation - base.rotation) * ((float)Math.PI / 64f);
				}
				else
				{
					base.rotation += (float)Math.Sign(this.targetRotation - base.rotation) * ((float)Math.PI / 64f);
				}
				base.rotation %= (float)Math.PI * 2f;
				this.wasHitCounter = 5 + Game1.random.Next(-1, 2);
			}
			float maxAccel = Math.Min(7f, Math.Max(2f, 7f - t / 64f / 2f));
			xSlope = (float)Math.Cos((double)base.rotation + Math.PI / 2.0);
			ySlope = 0f - (float)Math.Sin((double)base.rotation + Math.PI / 2.0);
			base.xVelocity += (0f - xSlope) * maxAccel / 6f + (float)Game1.random.Next(-10, 10) / 100f;
			base.yVelocity += (0f - ySlope) * maxAccel / 6f + (float)Game1.random.Next(-10, 10) / 100f;
			if (Math.Abs(base.xVelocity) > Math.Abs((0f - xSlope) * 7f))
			{
				base.xVelocity -= (0f - xSlope) * maxAccel / 6f;
			}
			if (Math.Abs(base.yVelocity) > Math.Abs((0f - ySlope) * 7f))
			{
				base.yVelocity -= (0f - ySlope) * maxAccel / 6f;
			}
		}
		base.resetAnimationSpeed();
	}

	public override void behaviorAtGameTick(GameTime time)
	{
		base.behaviorAtGameTick(time);
		if (double.IsNaN(base.xVelocity) || double.IsNaN(base.yVelocity))
		{
			base.Health = -500;
		}
		if (base.Position.X <= -640f || base.Position.Y <= -640f || base.Position.X >= (float)(base.currentLocation.Map.Layers[0].LayerWidth * 64 + 640) || base.Position.Y >= (float)(base.currentLocation.Map.Layers[0].LayerHeight * 64 + 640))
		{
			base.Health = -500;
		}
	}

	public override void Removed()
	{
		base.Removed();
		Fly.buzz?.Stop(AudioStopOptions.AsAuthored);
	}
}
