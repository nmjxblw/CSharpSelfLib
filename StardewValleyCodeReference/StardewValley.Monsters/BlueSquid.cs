using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Extensions;
using StardewValley.Projectiles;

namespace StardewValley.Monsters;

public class BlueSquid : Monster
{
	public float nextFire;

	public int squidYOffset;

	public float canMoveTimer;

	public NetFloat projectileIntroTimer = new NetFloat();

	public NetFloat projectileOutroTimer = new NetFloat();

	public NetBool nearFarmer = new NetBool();

	public NetFloat lastRotation = new NetFloat();

	[XmlIgnore]
	public bool justThrust;

	public BlueSquid()
	{
	}

	public BlueSquid(Vector2 position)
		: base("Blue Squid", position)
	{
		this.Sprite.SpriteHeight = 24;
		this.Sprite.SpriteWidth = 24;
		base.IsWalkingTowardPlayer = true;
		this.reloadSprite();
		this.Sprite.UpdateSourceRect();
		base.HideShadow = true;
		base.slipperiness.Value = Game1.random.Next(6, 9);
		this.canMoveTimer = Game1.random.Next(500);
		base.isHardModeMonster.Value = true;
	}

	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(this.projectileIntroTimer, "projectileIntroTimer").AddField(this.projectileOutroTimer, "projectileOutroTimer").AddField(this.lastRotation, "lastRotation")
			.AddField(this.nearFarmer, "nearFarmer");
		this.lastRotation.Interpolated(interpolate: false, wait: false);
		this.projectileIntroTimer.Interpolated(interpolate: false, wait: false);
		this.projectileOutroTimer.Interpolated(interpolate: false, wait: false);
	}

	/// <inheritdoc />
	public override void reloadSprite(bool onlyAppearance = false)
	{
		this.Sprite = new AnimatedSprite("Characters\\Monsters\\Blue Squid", 0, 24, 24);
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
			this.projectileOutroTimer.Value = 0f;
			this.projectileIntroTimer.Value = 0f;
			base.shakeTimer = 250;
			base.setTrajectory(xTrajectory, yTrajectory);
			this.lastRotation.Value = (float)Math.Atan2(0f - base.yVelocity, base.xVelocity) + (float)Math.PI / 2f;
			DelayedAction.playSoundAfterDelay("squid_hit", 80, base.currentLocation);
			base.currentLocation.playSound("slimeHit");
			if (base.Health <= 0)
			{
				base.deathAnimation();
			}
		}
		return actualDamage;
	}

	protected override void sharedDeathAnimation()
	{
		base.currentLocation.localSound("slimedead");
		if (this.Sprite.Texture.Height > this.Sprite.getHeight() * 4)
		{
			Point standingPixel = base.StandingPixel;
			Game1.createRadialDebris(base.currentLocation, this.Sprite.textureName.Value, new Rectangle(0, 48, 16, 16), 8, standingPixel.X, standingPixel.Y, 6, base.TilePoint.Y, Color.White, 4f * base.scale.Value);
		}
	}

	protected override void localDeathAnimation()
	{
		Game1.multiplayer.broadcastSprites(base.currentLocation, new TemporaryAnimatedSprite(44, base.Position, Color.HotPink * 0.86f, 10)
		{
			interval = 70f,
			holdLastFrame = true,
			alphaFade = 0.01f
		});
		Game1.multiplayer.broadcastSprites(base.currentLocation, new TemporaryAnimatedSprite(44, base.Position + new Vector2(-16f, 0f), Color.HotPink * 0.86f, 10)
		{
			interval = 70f,
			delayBeforeAnimationStart = 0,
			holdLastFrame = true,
			alphaFade = 0.01f
		});
		Game1.multiplayer.broadcastSprites(base.currentLocation, new TemporaryAnimatedSprite(44, base.Position + new Vector2(0f, -16f), Color.HotPink * 0.86f, 10)
		{
			interval = 70f,
			delayBeforeAnimationStart = 100,
			holdLastFrame = true,
			alphaFade = 0.01f
		});
		Game1.multiplayer.broadcastSprites(base.currentLocation, new TemporaryAnimatedSprite(44, base.Position + new Vector2(16f, 0f), Color.HotPink * 0.86f, 10)
		{
			interval = 70f,
			delayBeforeAnimationStart = 200,
			holdLastFrame = true,
			alphaFade = 0.01f
		});
	}

	public override Rectangle GetBoundingBox()
	{
		if (this.Sprite == null)
		{
			return Rectangle.Empty;
		}
		Vector2 position = base.Position;
		int width = base.GetSpriteWidthForPositioning() * 4 * 3 / 4;
		return new Rectangle((int)position.X, (int)position.Y + 16, width, 64);
	}

	public override void drawAboveAllLayers(SpriteBatch b)
	{
		int standingY = base.StandingPixel.Y;
		b.Draw(Game1.shadowTexture, base.getLocalPosition(Game1.viewport) + new Vector2(32f, 96f), Game1.shadowTexture.Bounds, Color.White, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), Math.Min(4f, 4f + (float)this.squidYOffset / 20f), SpriteEffects.None, (float)(standingY - 32) / 10000f);
		b.Draw(this.Sprite.Texture, base.getLocalPosition(Game1.viewport) + new Vector2(32f, 21 + this.squidYOffset) + new Vector2((base.shakeTimer > 0) ? Game1.random.Next(-2, 3) : 0, (base.shakeTimer > 0) ? Game1.random.Next(-2, 3) : 0), this.Sprite.SourceRect, Color.White, this.lastRotation.Value, new Vector2(12f, 12f), Math.Max(0.2f, base.scale.Value) * 4f, base.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, base.drawOnTop ? 0.991f : ((float)standingY / 10000f)));
	}

	protected override void updateAnimation(GameTime time)
	{
		if (this.Sprite.CurrentFrame != 2)
		{
			this.justThrust = false;
		}
		if (this.projectileIntroTimer.Value > 0f)
		{
			base.shakeTimer = 10;
			this.Sprite.CurrentFrame = 6;
			this.squidYOffset--;
			if (this.squidYOffset < 0)
			{
				this.squidYOffset = 0;
			}
		}
		else if (this.projectileOutroTimer.Value > 0f)
		{
			this.Sprite.CurrentFrame = 5;
			this.squidYOffset += 2;
		}
		else
		{
			this.squidYOffset = (int)(Math.Sin((double)((float)time.TotalGameTime.TotalMilliseconds / 2000f) * Math.PI * 2.0) * 30.0);
			this.Sprite.currentFrame = Math.Abs(this.squidYOffset - 24) / 12;
			if (this.squidYOffset < 0)
			{
				this.Sprite.CurrentFrame = 2;
			}
		}
		this.Sprite.UpdateSourceRect();
	}

	public override void noMovementProgressNearPlayerBehavior()
	{
	}

	public override void behaviorAtGameTick(GameTime time)
	{
		base.behaviorAtGameTick(time);
		this.nearFarmer.Value = this.withinPlayerThreshold(10) || base.focusedOnFarmers;
		if (this.projectileIntroTimer.Value <= 0f && this.projectileOutroTimer.Value <= 0f)
		{
			if (Math.Abs(base.xVelocity) <= 1f && Math.Abs(base.yVelocity) <= 1f && this.nearFarmer.Value)
			{
				Vector2 trajFinder = Utility.getVelocityTowardPoint(this.findPlayer().position.Value, base.position.Value, Game1.random.Next(25, 50));
				trajFinder.X *= -1f;
				if (this.canMoveTimer > 0f)
				{
					this.canMoveTimer -= (float)time.ElapsedGameTime.TotalMilliseconds;
				}
				if (!this.justThrust && this.Sprite.CurrentFrame == 2 && this.canMoveTimer <= 0f)
				{
					this.justThrust = true;
					Vector2 traj = Utility.getVelocityTowardPoint(this.findPlayer().position.Value, base.position.Value + new Vector2(Game1.random.Next(-64, 64)), Game1.random.Next(25, 50));
					traj.X *= -1f;
					this.setTrajectory(traj);
					this.lastRotation.Value = (float)Math.Atan2(0f - base.yVelocity, base.xVelocity) + (float)Math.PI / 2f;
					base.currentLocation.playSound("squid_move");
					this.canMoveTimer = 500f;
				}
			}
			else if (!this.nearFarmer.Value)
			{
				this.lastRotation.Value = 0f;
			}
		}
		if ((Math.Abs(base.xVelocity) >= 10f || Math.Abs(base.yVelocity) >= 10f) && Game1.random.NextDouble() < 0.25)
		{
			Game1.multiplayer.broadcastSprites(base.currentLocation, new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Rectangle(Game1.random.Choose(135, 140), 234, 5, 5), base.Position + new Vector2(32f, 32 + Game1.random.Next(-8, 8)), flipped: false, 0.01f, Color.White)
			{
				interval = 9999f,
				holdLastFrame = true,
				alphaFade = 0.01f,
				motion = new Vector2(0f, -1f),
				xPeriodic = true,
				xPeriodicLoopTime = Game1.random.Next(800, 1200),
				xPeriodicRange = Game1.random.Next(8, 20),
				scale = 4f,
				drawAboveAlwaysFront = true
			});
		}
		if (this.projectileIntroTimer.Value > 0f)
		{
			this.projectileIntroTimer.Value -= (float)time.ElapsedGameTime.TotalMilliseconds;
			base.shakeTimer = 10;
			if (Game1.random.NextDouble() < 0.25)
			{
				Game1.multiplayer.broadcastSprites(base.currentLocation, new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Rectangle(Game1.random.Choose(135, 140), 234, 5, 5), base.Position + new Vector2(21 + Game1.random.Next(-21, 21), this.squidYOffset / 2 + 32 + Game1.random.Next(-32, 32)), flipped: false, 0.01f, Color.White)
				{
					interval = 9999f,
					holdLastFrame = true,
					alphaFade = 0.01f,
					motion = new Vector2(0f, -1f),
					xPeriodic = true,
					xPeriodicLoopTime = Game1.random.Next(800, 1200),
					xPeriodicRange = Game1.random.Next(8, 20),
					scale = 4f,
					drawAboveAlwaysFront = true
				});
			}
			if (this.projectileIntroTimer.Value < 0f)
			{
				this.projectileOutroTimer.Value = 500f;
				base.IsWalkingTowardPlayer = false;
				this.Halt();
				Point standingPixel = base.StandingPixel;
				Vector2 trajectory = Utility.getVelocityTowardPlayer(standingPixel, 8f, base.Player);
				DebuffingProjectile projectile = new DebuffingProjectile("27", 8, 3, 4, 0f, trajectory.X, trajectory.Y, Utility.PointToVector2(standingPixel) - new Vector2(32f, -this.squidYOffset), base.currentLocation, this);
				projectile.height.Value = 48f;
				base.currentLocation.projectiles.Add(projectile);
				base.currentLocation.playSound("debuffSpell");
				this.nextFire = Game1.random.Next(1200, 3500);
			}
		}
		else if (this.projectileOutroTimer.Value > 0f)
		{
			this.projectileOutroTimer.Value -= (float)time.ElapsedGameTime.TotalMilliseconds;
		}
		this.nextFire = Math.Max(0f, this.nextFire - (float)time.ElapsedGameTime.Milliseconds);
		if (this.withinPlayerThreshold(6) && this.nextFire == 0f && this.projectileIntroTimer.Value <= 0f && Math.Abs(base.xVelocity) < 1f && Math.Abs(base.yVelocity) < 1f && Game1.random.NextDouble() < 0.003 && this.canMoveTimer <= 0f && base.currentLocation.hasTileAt(base.TilePoint.X, base.TilePoint.Y, "Back") && !base.currentLocation.hasTileAt(base.TilePoint.X, base.TilePoint.Y, "Buildings") && !base.currentLocation.hasTileAt(base.TilePoint.X, base.TilePoint.Y, "Front"))
		{
			this.projectileIntroTimer.Value = 1000f;
			this.lastRotation.Value = 0f;
			base.currentLocation.playSound("squid_bubble");
		}
	}
}
