using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Extensions;
using StardewValley.Projectiles;
using xTile.Dimensions;
using xTile.Layers;

namespace StardewValley.Monsters;

public class Ghost : Monster
{
	public enum GhostVariant
	{
		Normal,
		Putrid
	}

	public const float rotationIncrement = (float)Math.PI / 64f;

	[XmlIgnore]
	public int wasHitCounter;

	[XmlIgnore]
	public float targetRotation;

	[XmlIgnore]
	public bool turningRight;

	[XmlIgnore]
	public int identifier = Game1.random.Next(-99999, 99999);

	[XmlIgnore]
	public new int yOffset;

	[XmlIgnore]
	public int yOffsetExtra;

	[XmlIgnore]
	public string lightSourceId;

	public NetInt currentState = new NetInt(0);

	public float stateTimer = -1f;

	public float nextParticle;

	public NetEnum<GhostVariant> variant = new NetEnum<GhostVariant>(GhostVariant.Normal);

	public Ghost()
	{
		this.lightSourceId = this.GenerateLightSourceId(this.identifier);
	}

	public Ghost(Vector2 position)
		: base("Ghost", position)
	{
		this.lightSourceId = this.GenerateLightSourceId(this.identifier);
		base.Slipperiness = 8;
		base.isGlider.Value = true;
		base.HideShadow = true;
	}

	/// <summary>
	/// constructor for non-default ghosts
	/// </summary>
	/// <param name="position"></param>
	/// <param name="name"></param>
	public Ghost(Vector2 position, string name)
		: base(name, position)
	{
		this.lightSourceId = this.GenerateLightSourceId(this.identifier);
		base.Slipperiness = 8;
		base.isGlider.Value = true;
		base.HideShadow = true;
		if (name == "Putrid Ghost")
		{
			this.variant.Value = GhostVariant.Putrid;
		}
	}

	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(this.variant, "variant").AddField(this.currentState, "currentState");
		this.currentState.fieldChangeVisibleEvent += delegate
		{
			this.stateTimer = -1f;
		};
	}

	/// <inheritdoc />
	public override void reloadSprite(bool onlyAppearance = false)
	{
		this.Sprite = new AnimatedSprite("Characters\\Monsters\\" + base.name.Value);
	}

	public override int GetBaseDifficultyLevel()
	{
		if (this.variant.Value == GhostVariant.Putrid)
		{
			return 1;
		}
		return base.GetBaseDifficultyLevel();
	}

	public override List<Item> getExtraDropItems()
	{
		if (Game1.random.NextDouble() < 0.095 && Game1.player.team.SpecialOrderActive("Wizard") && !Game1.MasterPlayer.hasOrWillReceiveMail("ectoplasmDrop"))
		{
			Object o = ItemRegistry.Create<Object>("(O)875");
			o.specialItem = true;
			o.questItem.Value = true;
			return new List<Item> { o };
		}
		return base.getExtraDropItems();
	}

	public override void drawAboveAllLayers(SpriteBatch b)
	{
		int standingY = base.StandingPixel.Y;
		b.Draw(this.Sprite.Texture, base.getLocalPosition(Game1.viewport) + new Vector2(32f, 21 + this.yOffset), this.Sprite.SourceRect, Color.White, 0f, new Vector2(8f, 16f), Math.Max(0.2f, base.scale.Value) * 4f, base.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, base.drawOnTop ? 0.991f : ((float)standingY / 10000f)));
		b.Draw(Game1.shadowTexture, base.getLocalPosition(Game1.viewport) + new Vector2(32f, 64f), Game1.shadowTexture.Bounds, Color.White, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 3f + (float)this.yOffset / 20f, SpriteEffects.None, (float)(standingY - 1) / 10000f);
	}

	public override int takeDamage(int damage, int xTrajectory, int yTrajectory, bool isBomb, double addedPrecision, Farmer who)
	{
		if (this.variant.Value == GhostVariant.Putrid && this.currentState.Value <= 2)
		{
			this.currentState.Value = 0;
		}
		int actualDamage = Math.Max(1, damage - base.resilience.Value);
		base.Slipperiness = 8;
		Utility.addSprinklesToLocation(base.currentLocation, base.TilePoint.X, base.TilePoint.Y, 2, 2, 101, 50, Color.LightBlue);
		if (Game1.random.NextDouble() < base.missChance.Value - base.missChance.Value * addedPrecision)
		{
			actualDamage = -1;
		}
		else
		{
			base.Health -= actualDamage;
			if (base.Health <= 0)
			{
				base.deathAnimation();
			}
			base.setTrajectory(xTrajectory, yTrajectory);
		}
		this.addedSpeed = -1f;
		Utility.removeLightSource(this.lightSourceId);
		return actualDamage;
	}

	protected override void localDeathAnimation()
	{
		base.currentLocation.localSound("ghost");
		base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(this.Sprite.textureName.Value, new Microsoft.Xna.Framework.Rectangle(0, 96, 16, 24), 100f, 4, 0, base.Position, flicker: false, flipped: false, 0.9f, 0.001f, Color.White, 4f, 0.01f, 0f, (float)Math.PI / 64f));
	}

	protected override void sharedDeathAnimation()
	{
	}

	protected override void updateAnimation(GameTime time)
	{
		this.nextParticle -= (float)time.ElapsedGameTime.TotalSeconds;
		if (this.nextParticle <= 0f)
		{
			this.nextParticle = 1f;
			if (this.variant.Value == GhostVariant.Putrid)
			{
				if (base.currentLocationRef.Value != null)
				{
					int standingY = base.StandingPixel.Y;
					TemporaryAnimatedSprite drip = new TemporaryAnimatedSprite(this.Sprite.textureName.Value, new Microsoft.Xna.Framework.Rectangle(Game1.random.Next(4) * 16, 168, 16, 24), 100f, 1, 10, base.Position + new Vector2(Utility.RandomFloat(-16f, 16f), Utility.RandomFloat(-16f, 0f) - (float)this.yOffset), flicker: false, flipped: false, (float)standingY / 10000f, 0.01f, Color.White, 4f, -0.01f, 0f, 0f);
					drip.acceleration = new Vector2(0f, 0.025f);
					base.currentLocation.temporarySprites.Add(drip);
				}
				this.nextParticle = Utility.RandomFloat(0.3f, 0.5f);
			}
		}
		this.yOffset = (int)(Math.Sin((double)((float)time.TotalGameTime.Milliseconds / 1000f) * (Math.PI * 2.0)) * 20.0) - this.yOffsetExtra;
		if (base.currentLocation == Game1.currentLocation)
		{
			if (Game1.currentLightSources.TryGetValue(this.lightSourceId, out var light))
			{
				light.position.Value = new Vector2(base.Position.X + 32f, base.Position.Y + 64f + (float)this.yOffset);
			}
			else if (base.name.Value == "Carbon Ghost")
			{
				Game1.currentLightSources.Add(new LightSource(this.lightSourceId, 4, new Vector2(base.Position.X + 8f, base.Position.Y + 64f), 1f, new Color(80, 30, 0), LightSource.LightContext.None, 0L, Game1.currentLocation.NameOrUniqueName));
			}
			else
			{
				Game1.currentLightSources.Add(new LightSource(this.lightSourceId, 5, new Vector2(base.Position.X + 8f, base.Position.Y + 64f), 1f, Color.White * 0.7f, LightSource.LightContext.None, 0L, Game1.currentLocation.NameOrUniqueName));
			}
		}
		if (this.variant.Value == GhostVariant.Putrid && this.UpdateVariantAnimation(time))
		{
			return;
		}
		Point monsterPixel = base.StandingPixel;
		Point standingPixel = base.Player.StandingPixel;
		float xSlope = -(standingPixel.X - monsterPixel.X);
		float ySlope = standingPixel.Y - monsterPixel.Y;
		float t = 400f;
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
			this.wasHitCounter = 0;
		}
		float maxAccel = Math.Min(4f, Math.Max(1f, 5f - t / 64f / 2f));
		xSlope = (float)Math.Cos((double)base.rotation + Math.PI / 2.0);
		ySlope = 0f - (float)Math.Sin((double)base.rotation + Math.PI / 2.0);
		base.xVelocity += (0f - xSlope) * maxAccel / 6f + (float)Game1.random.Next(-10, 10) / 100f;
		base.yVelocity += (0f - ySlope) * maxAccel / 6f + (float)Game1.random.Next(-10, 10) / 100f;
		if (Math.Abs(base.xVelocity) > Math.Abs((0f - xSlope) * 5f))
		{
			base.xVelocity -= (0f - xSlope) * maxAccel / 6f;
		}
		if (Math.Abs(base.yVelocity) > Math.Abs((0f - ySlope) * 5f))
		{
			base.yVelocity -= (0f - ySlope) * maxAccel / 6f;
		}
		base.faceGeneralDirection(base.Player.getStandingPosition(), 0, opposite: false, useTileCalculations: false);
		base.resetAnimationSpeed();
	}

	public virtual bool UpdateVariantAnimation(GameTime time)
	{
		if (this.variant.Value == GhostVariant.Putrid)
		{
			if (this.currentState.Value == 0)
			{
				if (this.Sprite.CurrentFrame >= 20)
				{
					this.Sprite.CurrentFrame = 0;
				}
				return false;
			}
			if (this.currentState.Value >= 1 && this.currentState.Value <= 3)
			{
				base.shakeTimer = 250;
				if (base.Player != null)
				{
					base.faceGeneralDirection(base.Player.getStandingPosition(), 0, opposite: false, useTileCalculations: false);
				}
				switch (this.FacingDirection)
				{
				case 2:
					this.Sprite.CurrentFrame = 20;
					break;
				case 1:
					this.Sprite.CurrentFrame = 21;
					break;
				case 0:
					this.Sprite.CurrentFrame = 22;
					break;
				case 3:
					this.Sprite.CurrentFrame = 23;
					break;
				}
			}
			else if (this.currentState.Value >= 4)
			{
				base.shakeTimer = 250;
				switch (this.FacingDirection)
				{
				case 2:
					this.Sprite.CurrentFrame = 24;
					break;
				case 1:
					this.Sprite.CurrentFrame = 25;
					break;
				case 0:
					this.Sprite.CurrentFrame = 26;
					break;
				case 3:
					this.Sprite.CurrentFrame = 27;
					break;
				}
			}
			return true;
		}
		return false;
	}

	public override void noMovementProgressNearPlayerBehavior()
	{
	}

	public override void behaviorAtGameTick(GameTime time)
	{
		if (this.stateTimer > 0f)
		{
			this.stateTimer -= (float)time.ElapsedGameTime.TotalSeconds;
			if (this.stateTimer <= 0f)
			{
				this.stateTimer = 0f;
			}
		}
		if (this.variant.Value == GhostVariant.Putrid)
		{
			Farmer player = base.Player;
			switch (this.currentState.Value)
			{
			case 0:
				if (this.stateTimer == -1f)
				{
					this.stateTimer = Utility.RandomFloat(1f, 2f);
				}
				if (player != null && this.stateTimer == 0f && Math.Abs(player.Position.X - base.Position.X) < 448f && Math.Abs(player.Position.Y - base.Position.Y) < 448f)
				{
					this.currentState.Value = 1;
					base.currentLocation.playSound("croak");
					this.stateTimer = 0.5f;
				}
				break;
			case 1:
				base.xVelocity = 0f;
				base.yVelocity = 0f;
				if (this.stateTimer <= 0f)
				{
					this.currentState.Value = 2;
				}
				break;
			case 2:
			{
				if (player == null)
				{
					this.currentState.Value = 0;
					break;
				}
				if (Math.Abs(player.Position.X - base.Position.X) < 80f && Math.Abs(player.Position.Y - base.Position.Y) < 80f)
				{
					this.currentState.Value = 3;
					this.stateTimer = 0.05f;
					base.xVelocity = 0f;
					base.yVelocity = 0f;
					break;
				}
				Vector2 offset = player.getStandingPosition() - base.getStandingPosition();
				if (offset.LengthSquared() == 0f)
				{
					this.currentState.Value = 3;
					this.stateTimer = 0.15f;
					break;
				}
				offset.Normalize();
				offset *= 10f;
				base.xVelocity = offset.X;
				base.yVelocity = 0f - offset.Y;
				break;
			}
			case 3:
				base.xVelocity = 0f;
				base.yVelocity = 0f;
				if (this.stateTimer <= 0f)
				{
					this.currentState.Value = 4;
					this.stateTimer = 1f;
					Vector2 shot_velocity = this.FacingDirection switch
					{
						0 => new Vector2(0f, -1f), 
						3 => new Vector2(-1f, 0f), 
						1 => new Vector2(1f, 0f), 
						2 => new Vector2(0f, 1f), 
						_ => Vector2.Zero, 
					};
					shot_velocity *= 6f;
					base.currentLocation.playSound("fishSlap");
					BasicProjectile projectile = new BasicProjectile(base.DamageToFarmer, 7, 0, 1, (float)Math.PI / 32f, shot_velocity.X, shot_velocity.Y, base.Position, null, null, null, explode: false, damagesMonsters: false, base.currentLocation, this);
					projectile.debuff.Value = "25";
					projectile.scaleGrow.Value = 0.05f;
					projectile.ignoreTravelGracePeriod.Value = true;
					projectile.IgnoreLocationCollision = true;
					projectile.maxTravelDistance.Value = 192;
					base.currentLocation.projectiles.Add(projectile);
				}
				break;
			case 4:
				if (this.stateTimer <= 0f)
				{
					base.xVelocity = 0f;
					base.yVelocity = 0f;
					this.currentState.Value = 0;
					this.stateTimer = Utility.RandomFloat(3f, 4f);
				}
				break;
			}
		}
		base.behaviorAtGameTick(time);
		Microsoft.Xna.Framework.Rectangle playerBounds = base.Player.GetBoundingBox();
		if (!this.GetBoundingBox().Intersects(playerBounds) || !base.Player.temporarilyInvincible || this.currentState.Value != 0)
		{
			return;
		}
		Layer backLayer = base.currentLocation.map.RequireLayer("Back");
		Point playerCenter = playerBounds.Center;
		int attempts = 0;
		Vector2 attemptedPosition = new Vector2(playerCenter.X / 64 + Game1.random.Next(-12, 12), playerCenter.Y / 64 + Game1.random.Next(-12, 12));
		for (; attempts < 3; attempts++)
		{
			if (!(attemptedPosition.X >= (float)backLayer.LayerWidth) && !(attemptedPosition.Y >= (float)backLayer.LayerHeight) && !(attemptedPosition.X < 0f) && !(attemptedPosition.Y < 0f) && backLayer.Tiles[(int)attemptedPosition.X, (int)attemptedPosition.Y] != null && base.currentLocation.isTilePassable(new Location((int)attemptedPosition.X, (int)attemptedPosition.Y), Game1.viewport) && !attemptedPosition.Equals(new Vector2(playerCenter.X / 64, playerCenter.Y / 64)))
			{
				break;
			}
			attemptedPosition = new Vector2(playerCenter.X / 64 + Game1.random.Next(-12, 12), playerCenter.Y / 64 + Game1.random.Next(-12, 12));
		}
		if (attempts < 3)
		{
			base.Position = new Vector2(attemptedPosition.X * 64f, attemptedPosition.Y * 64f - 32f);
			this.Halt();
		}
	}
}
