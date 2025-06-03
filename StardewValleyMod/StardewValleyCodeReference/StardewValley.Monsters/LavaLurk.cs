using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.Projectiles;

namespace StardewValley.Monsters;

public class LavaLurk : Monster
{
	public enum State
	{
		Submerged,
		Lurking,
		Emerged,
		Firing,
		Diving
	}

	[XmlIgnore]
	public List<FarmerSprite.AnimationFrame> submergedAnimation = new List<FarmerSprite.AnimationFrame>();

	[XmlIgnore]
	public List<FarmerSprite.AnimationFrame> lurkAnimation = new List<FarmerSprite.AnimationFrame>();

	[XmlIgnore]
	public List<FarmerSprite.AnimationFrame> emergeAnimation = new List<FarmerSprite.AnimationFrame>();

	[XmlIgnore]
	public List<FarmerSprite.AnimationFrame> diveAnimation = new List<FarmerSprite.AnimationFrame>();

	[XmlIgnore]
	public List<FarmerSprite.AnimationFrame> resubmergeAnimation = new List<FarmerSprite.AnimationFrame>();

	[XmlIgnore]
	public List<FarmerSprite.AnimationFrame> idleAnimation = new List<FarmerSprite.AnimationFrame>();

	[XmlIgnore]
	public List<FarmerSprite.AnimationFrame> fireAnimation = new List<FarmerSprite.AnimationFrame>();

	[XmlIgnore]
	public List<FarmerSprite.AnimationFrame> locallyPlayingAnimation;

	[XmlIgnore]
	public bool approachFarmer;

	[XmlIgnore]
	public Vector2 velocity = Vector2.Zero;

	[XmlIgnore]
	public int swimSpeed;

	[XmlIgnore]
	public Farmer targettedFarmer;

	[XmlIgnore]
	public NetEnum<State> currentState = new NetEnum<State>();

	[XmlIgnore]
	public float stateTimer;

	[XmlIgnore]
	public float fireTimer;

	public LavaLurk()
	{
		this.Initialize();
	}

	public LavaLurk(Vector2 position)
		: base("Lava Lurk", position)
	{
		this.Sprite.SpriteWidth = 16;
		this.Sprite.SpriteHeight = 16;
		this.Sprite.UpdateSourceRect();
		this.Initialize();
		base.ignoreDamageLOS.Value = true;
		this.SetRandomMovement();
		this.stateTimer = Utility.RandomFloat(3f, 5f);
	}

	/// <inheritdoc />
	public override void reloadSprite(bool onlyAppearance = false)
	{
		base.reloadSprite(onlyAppearance);
		this.Sprite.SpriteWidth = 16;
		this.Sprite.SpriteHeight = 16;
		this.Sprite.UpdateSourceRect();
	}

	public virtual void Initialize()
	{
		base.HideShadow = true;
		this.submergedAnimation.AddRange(new FarmerSprite.AnimationFrame[2]
		{
			new FarmerSprite.AnimationFrame(0, 750),
			new FarmerSprite.AnimationFrame(1, 1000)
		});
		this.lurkAnimation.AddRange(new FarmerSprite.AnimationFrame[2]
		{
			new FarmerSprite.AnimationFrame(2, 250),
			new FarmerSprite.AnimationFrame(3, 250)
		});
		this.resubmergeAnimation.AddRange(new FarmerSprite.AnimationFrame[3]
		{
			new FarmerSprite.AnimationFrame(3, 250),
			new FarmerSprite.AnimationFrame(2, 250),
			new FarmerSprite.AnimationFrame(1, 250, secondaryArm: false, flip: false, OnDiveAnimationEnd)
		});
		this.emergeAnimation.AddRange(new FarmerSprite.AnimationFrame[4]
		{
			new FarmerSprite.AnimationFrame(2, 150),
			new FarmerSprite.AnimationFrame(3, 150),
			new FarmerSprite.AnimationFrame(4, 150),
			new FarmerSprite.AnimationFrame(5, 150, secondaryArm: false, flip: false, OnEmergeAnimationEnd, behaviorAtEndOfFrame: true)
		});
		this.diveAnimation.AddRange(new FarmerSprite.AnimationFrame[4]
		{
			new FarmerSprite.AnimationFrame(5, 150),
			new FarmerSprite.AnimationFrame(4, 150),
			new FarmerSprite.AnimationFrame(3, 150),
			new FarmerSprite.AnimationFrame(2, 150, secondaryArm: false, flip: false, OnDiveAnimationEnd, behaviorAtEndOfFrame: true)
		});
		this.idleAnimation.AddRange(new FarmerSprite.AnimationFrame[2]
		{
			new FarmerSprite.AnimationFrame(5, 500),
			new FarmerSprite.AnimationFrame(6, 500)
		});
		this.fireAnimation.AddRange(new FarmerSprite.AnimationFrame[1]
		{
			new FarmerSprite.AnimationFrame(7, 500)
		});
	}

	public virtual void OnEmergeAnimationEnd(Farmer who)
	{
		this.PlayAnimation(this.idleAnimation, loop: true);
	}

	public virtual void OnDiveAnimationEnd(Farmer who)
	{
		this.PlayAnimation(this.submergedAnimation, loop: true);
	}

	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(this.currentState, "currentState");
	}

	protected override void sharedDeathAnimation()
	{
		base.currentLocation.playSound("skeletonDie");
		base.currentLocation.playSound("grunt");
		Rectangle bounds = this.GetBoundingBox();
		for (int i = 0; i < 16; i++)
		{
			Game1.createRadialDebris(base.currentLocation, "Characters\\Monsters\\Pepper Rex", new Rectangle(64, 128, 16, 16), 16, (int)Utility.Lerp(bounds.Left, bounds.Right, (float)Game1.random.NextDouble()), (int)Utility.Lerp(bounds.Bottom, bounds.Top, (float)Game1.random.NextDouble()), 1, base.TilePoint.Y, Color.White, 4f);
		}
	}

	protected override void updateAnimation(GameTime time)
	{
		base.updateAnimation(time);
		switch (this.currentState.Value)
		{
		case State.Submerged:
			this.PlayAnimation(this.submergedAnimation, loop: true);
			break;
		case State.Lurking:
			if (this.PlayAnimation(this.lurkAnimation, loop: false) && base.currentLocation == Game1.currentLocation && Utility.isOnScreen(base.Position, 64))
			{
				Game1.playSound("waterSlosh");
			}
			break;
		case State.Emerged:
			if (this.locallyPlayingAnimation != this.emergeAnimation && this.locallyPlayingAnimation != this.idleAnimation)
			{
				if (base.currentLocation == Game1.currentLocation && Utility.isOnScreen(base.Position, 64))
				{
					Game1.playSound("waterSlosh");
				}
				this.PlayAnimation(this.emergeAnimation, loop: false);
			}
			break;
		case State.Firing:
			this.PlayAnimation(this.fireAnimation, loop: true);
			break;
		case State.Diving:
			if (this.locallyPlayingAnimation != this.diveAnimation && this.locallyPlayingAnimation != this.submergedAnimation && this.locallyPlayingAnimation != this.resubmergeAnimation)
			{
				if (base.currentLocation == Game1.currentLocation && Utility.isOnScreen(base.Position, 64))
				{
					Game1.playSound("waterSlosh");
				}
				if (this.locallyPlayingAnimation == this.lurkAnimation)
				{
					this.PlayAnimation(this.resubmergeAnimation, loop: false);
				}
				else
				{
					this.PlayAnimation(this.diveAnimation, loop: false);
				}
			}
			break;
		}
		this.Sprite.animateOnce(time);
	}

	public virtual bool PlayAnimation(List<FarmerSprite.AnimationFrame> animation_to_play, bool loop)
	{
		if (this.locallyPlayingAnimation != animation_to_play)
		{
			this.locallyPlayingAnimation = animation_to_play;
			this.Sprite.setCurrentAnimation(animation_to_play);
			this.Sprite.loop = loop;
			if (!loop)
			{
				this.Sprite.oldFrame = animation_to_play.Last().frame;
			}
			return true;
		}
		return false;
	}

	public virtual bool TargetInRange()
	{
		if (this.targettedFarmer == null)
		{
			return false;
		}
		if (Math.Abs(this.targettedFarmer.Position.X - base.Position.X) <= 640f && Math.Abs(this.targettedFarmer.Position.Y - base.Position.Y) <= 640f)
		{
			return true;
		}
		return false;
	}

	public virtual void SetRandomMovement()
	{
		this.velocity = new Vector2((Game1.random.Next(2) != 1) ? 1 : (-1), (Game1.random.Next(2) != 1) ? 1 : (-1));
	}

	protected override void updateMonsterSlaveAnimation(GameTime time)
	{
	}

	public override int takeDamage(int damage, int xTrajectory, int yTrajectory, bool isBomb, double addedPrecision, Farmer who)
	{
		if (this.currentState.Value == State.Submerged)
		{
			return -1;
		}
		return base.takeDamage(damage, xTrajectory, yTrajectory, isBomb, addedPrecision, who);
	}

	public override void behaviorAtGameTick(GameTime time)
	{
		if (this.targettedFarmer == null || this.targettedFarmer.currentLocation != base.currentLocation)
		{
			this.targettedFarmer = null;
			this.targettedFarmer = this.findPlayer();
		}
		if (this.stateTimer > 0f)
		{
			this.stateTimer -= (float)time.ElapsedGameTime.TotalSeconds;
			if (this.stateTimer <= 0f)
			{
				this.stateTimer = 0f;
			}
		}
		switch (this.currentState.Value)
		{
		case State.Submerged:
			this.swimSpeed = 2;
			if (this.stateTimer == 0f)
			{
				this.currentState.Value = State.Lurking;
				this.stateTimer = 1f;
			}
			break;
		case State.Lurking:
			this.swimSpeed = 1;
			if (this.stateTimer == 0f)
			{
				if (this.TargetInRange())
				{
					this.currentState.Value = State.Emerged;
					this.stateTimer = 1f;
					this.swimSpeed = 0;
				}
				else
				{
					this.currentState.Value = State.Diving;
					this.stateTimer = 1f;
				}
			}
			break;
		case State.Emerged:
			if (this.stateTimer == 0f)
			{
				this.currentState.Value = State.Firing;
				this.stateTimer = 1f;
				this.fireTimer = 0.25f;
			}
			break;
		case State.Firing:
			if (this.stateTimer == 0f)
			{
				this.currentState.Value = State.Diving;
				this.stateTimer = 1f;
			}
			if (!(this.fireTimer > 0f))
			{
				break;
			}
			this.fireTimer -= (float)time.ElapsedGameTime.TotalSeconds;
			if (this.fireTimer <= 0f)
			{
				this.fireTimer = 0.25f;
				if (this.targettedFarmer != null)
				{
					Vector2 shot_origin = base.Position + new Vector2(0f, -32f);
					Vector2 shot_velocity = this.targettedFarmer.Position - shot_origin;
					shot_velocity.Normalize();
					shot_velocity *= 7f;
					base.currentLocation.playSound("fireball");
					BasicProjectile projectile = new BasicProjectile(25, 10, 0, 3, (float)Math.PI / 16f, shot_velocity.X, shot_velocity.Y, shot_origin, null, null, null, explode: false, damagesMonsters: false, base.currentLocation, this);
					projectile.ignoreLocationCollision.Value = true;
					projectile.ignoreTravelGracePeriod.Value = true;
					projectile.maxTravelDistance.Value = 640;
					base.currentLocation.projectiles.Add(projectile);
				}
			}
			break;
		case State.Diving:
			if (this.stateTimer == 0f)
			{
				this.currentState.Value = State.Submerged;
				this.stateTimer = Utility.RandomFloat(3f, 5f);
				this.approachFarmer = !this.approachFarmer;
				if (this.approachFarmer)
				{
					this.targettedFarmer = this.findPlayer();
				}
				this.SetRandomMovement();
			}
			break;
		}
		if (this.targettedFarmer != null && this.approachFarmer)
		{
			Point curTile = base.TilePoint;
			Point playerTile = this.targettedFarmer.TilePoint;
			if (curTile.X > playerTile.X)
			{
				this.velocity.X = -1f;
			}
			else if (curTile.X < playerTile.X)
			{
				this.velocity.X = 1f;
			}
			if (curTile.Y > playerTile.Y)
			{
				this.velocity.Y = -1f;
			}
			else if (curTile.Y < playerTile.Y)
			{
				this.velocity.Y = 1f;
			}
		}
		if (this.velocity.X != 0f || this.velocity.Y != 0f)
		{
			Rectangle next_bounds = this.GetBoundingBox();
			Vector2 next_position = base.Position;
			next_bounds.Inflate(48, 48);
			next_bounds.X += (int)this.velocity.X * this.swimSpeed;
			next_position.X += (int)this.velocity.X * this.swimSpeed;
			if (!this.CheckInWater(next_bounds))
			{
				this.velocity.X *= -1f;
				next_bounds.X += (int)this.velocity.X * this.swimSpeed;
				next_position.X += (int)this.velocity.X * this.swimSpeed;
			}
			next_bounds.Y += (int)this.velocity.Y * this.swimSpeed;
			next_position.Y += (int)this.velocity.Y * this.swimSpeed;
			if (!this.CheckInWater(next_bounds))
			{
				this.velocity.Y *= -1f;
				next_bounds.Y += (int)this.velocity.Y * this.swimSpeed;
				next_position.Y += (int)this.velocity.Y * this.swimSpeed;
			}
			if (base.Position != next_position)
			{
				base.Position = next_position;
			}
		}
	}

	public static bool IsLavaTile(GameLocation location, int x, int y)
	{
		return location.isWaterTile(x, y);
	}

	public bool CheckInWater(Rectangle position)
	{
		for (int x = position.Left / 64; x <= position.Right / 64; x++)
		{
			for (int y = position.Top / 64; y <= position.Bottom / 64; y++)
			{
				if (!LavaLurk.IsLavaTile(base.currentLocation, x, y))
				{
					return false;
				}
			}
		}
		return true;
	}

	public override void updateMovement(GameLocation location, GameTime time)
	{
	}

	public override Debris ModifyMonsterLoot(Debris debris)
	{
		if (debris != null)
		{
			debris.chunksMoveTowardPlayer = true;
		}
		return debris;
	}
}
