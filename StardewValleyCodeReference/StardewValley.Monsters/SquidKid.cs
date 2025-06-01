using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Extensions;
using StardewValley.Projectiles;

namespace StardewValley.Monsters;

public class SquidKid : Monster
{
	[XmlIgnore]
	public float lastFireball;

	[XmlIgnore]
	public new int yOffset;

	private readonly NetEvent0 fireballEvent = new NetEvent0();

	private readonly NetEvent0 hurtAnimationEvent = new NetEvent0();

	private int numFireballsLeft;

	private float firingTimer;

	public SquidKid()
	{
	}

	public SquidKid(Vector2 position)
		: base("Squid Kid", position)
	{
		this.Sprite.SpriteHeight = 16;
		base.IsWalkingTowardPlayer = false;
		this.Sprite.UpdateSourceRect();
		base.HideShadow = true;
	}

	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(this.fireballEvent, "fireballEvent").AddField(this.hurtAnimationEvent, "hurtAnimationEvent");
		this.fireballEvent.onEvent += delegate
		{
			if (!Game1.IsMasterGame)
			{
				this.fireballFired();
			}
		};
		this.hurtAnimationEvent.onEvent += delegate
		{
			this.Sprite.currentFrame = this.Sprite.currentFrame - this.Sprite.currentFrame % 4 + 3;
		};
	}

	/// <inheritdoc />
	public override void reloadSprite(bool onlyAppearance = false)
	{
		this.Sprite = new AnimatedSprite("Characters\\Monsters\\Squid Kid");
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
			base.setTrajectory(xTrajectory, yTrajectory);
			base.currentLocation.playSound("hitEnemy");
			this.hurtAnimationEvent.Fire();
			if (base.Health <= 0)
			{
				base.deathAnimation();
			}
		}
		return actualDamage;
	}

	protected override void sharedDeathAnimation()
	{
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
		int standingY = base.StandingPixel.Y;
		b.Draw(this.Sprite.Texture, base.getLocalPosition(Game1.viewport) + new Vector2(32f, 21 + this.yOffset), this.Sprite.SourceRect, Color.White, 0f, new Vector2(8f, 16f), Math.Max(0.2f, base.scale.Value) * 4f, base.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, base.drawOnTop ? 0.991f : ((float)standingY / 10000f)));
		b.Draw(Game1.shadowTexture, base.getLocalPosition(Game1.viewport) + new Vector2(32f, 64f), Game1.shadowTexture.Bounds, Color.White, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 3f + (float)this.yOffset / 20f, SpriteEffects.None, (float)(standingY - 1) / 10000f);
	}

	protected override void updateAnimation(GameTime time)
	{
		base.updateAnimation(time);
		this.yOffset = (int)(Math.Sin((double)((float)time.TotalGameTime.Milliseconds / 2000f) * (Math.PI * 2.0)) * 15.0);
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

	protected override void updateMonsterSlaveAnimation(GameTime time)
	{
		if (this.isMoving())
		{
			switch (this.FacingDirection)
			{
			case 0:
				this.Sprite.AnimateUp(time);
				break;
			case 3:
				this.Sprite.AnimateLeft(time);
				break;
			case 1:
				this.Sprite.AnimateRight(time);
				break;
			case 2:
				this.Sprite.AnimateDown(time);
				break;
			}
		}
		base.faceGeneralDirection(base.Player.Position);
	}

	private Vector2 fireballFired()
	{
		switch (this.FacingDirection)
		{
		case 0:
			this.Sprite.currentFrame = 3;
			return Vector2.Zero;
		case 1:
			this.Sprite.currentFrame = 7;
			return new Vector2(64f, 0f);
		case 2:
			this.Sprite.currentFrame = 11;
			return new Vector2(0f, 32f);
		case 3:
			this.Sprite.currentFrame = 15;
			return new Vector2(-32f, 0f);
		default:
			return Vector2.Zero;
		}
	}

	public override void update(GameTime time, GameLocation location)
	{
		base.update(time, location);
		this.fireballEvent.Poll();
	}

	public override void behaviorAtGameTick(GameTime time)
	{
		base.behaviorAtGameTick(time);
		base.faceGeneralDirection(base.Player.Position);
		this.lastFireball = Math.Max(0f, this.lastFireball - (float)time.ElapsedGameTime.Milliseconds);
		if (base.isHardModeMonster.Value)
		{
			if ((this.numFireballsLeft <= 0 && !this.withinPlayerThreshold()) || !(this.lastFireball <= 0f))
			{
				return;
			}
			if (this.lastFireball <= 0f && this.numFireballsLeft <= 0)
			{
				this.numFireballsLeft = 4;
				this.firingTimer = 0f;
			}
			this.firingTimer -= (float)time.ElapsedGameTime.TotalMilliseconds;
			if (this.firingTimer <= 0f && this.numFireballsLeft > 0)
			{
				Rectangle playerBounds = base.Player.GetBoundingBox();
				this.numFireballsLeft--;
				base.IsWalkingTowardPlayer = false;
				this.Halt();
				this.fireballEvent.Fire();
				this.fireballFired();
				this.Sprite.UpdateSourceRect();
				Vector2 standingPixel = base.getStandingPosition();
				Vector2 trajectory = Utility.getVelocityTowardPoint(standingPixel, new Vector2(playerBounds.X, playerBounds.Y) + new Vector2(Game1.random.Next(-128, 128)), 8f);
				BasicProjectile projectile = new BasicProjectile(15, 10, 2, 4, 0f, trajectory.X, trajectory.Y, standingPixel - new Vector2(32f, 0f), null, null, null, explode: true, damagesMonsters: false, base.currentLocation, this);
				projectile.height.Value = 48f;
				base.currentLocation.projectiles.Add(projectile);
				base.currentLocation.playSound("fireball");
				this.firingTimer = 400f;
				if (this.numFireballsLeft <= 0)
				{
					this.lastFireball = Game1.random.Next(3000, 6500);
				}
			}
		}
		else if (this.withinPlayerThreshold() && this.lastFireball == 0f && Game1.random.NextDouble() < 0.01)
		{
			base.IsWalkingTowardPlayer = false;
			this.Halt();
			this.fireballEvent.Fire();
			this.fireballFired();
			this.Sprite.UpdateSourceRect();
			Point standingPixel2 = base.StandingPixel;
			Vector2 trajectory2 = Utility.getVelocityTowardPlayer(standingPixel2, 8f, base.Player);
			BasicProjectile projectile2 = new BasicProjectile(15, 10, 3, 4, 0f, trajectory2.X, trajectory2.Y, new Vector2(standingPixel2.X - 32, standingPixel2.Y), null, null, null, explode: true, damagesMonsters: false, base.currentLocation, this);
			projectile2.height.Value = 48f;
			base.currentLocation.projectiles.Add(projectile2);
			base.currentLocation.playSound("fireball");
			this.lastFireball = Game1.random.Next(1200, 3500);
		}
		else if (this.lastFireball != 0f && Game1.random.NextDouble() < 0.02)
		{
			this.Halt();
			if (this.withinPlayerThreshold())
			{
				base.Slipperiness = 8;
				Point standingTile = base.StandingPixel;
				base.setTrajectory((int)Utility.getVelocityTowardPlayer(standingTile, 8f, base.Player).X, (int)(0f - Utility.getVelocityTowardPlayer(standingTile, 8f, base.Player).Y));
			}
		}
	}
}
