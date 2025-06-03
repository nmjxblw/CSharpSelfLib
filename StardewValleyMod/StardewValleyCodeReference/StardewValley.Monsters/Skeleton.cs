using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.Extensions;
using StardewValley.Pathfinding;
using StardewValley.Projectiles;

namespace StardewValley.Monsters;

public class Skeleton : Monster
{
	[XmlIgnore]
	public bool spottedPlayer;

	[XmlIgnore]
	public readonly NetBool throwing = new NetBool();

	public readonly NetBool isMage = new NetBool();

	private int controllerAttemptTimer;

	public Skeleton()
	{
	}

	public Skeleton(Vector2 position, bool isMage = false)
		: base("Skeleton", position, Game1.random.Next(4))
	{
		this.isMage.Value = isMage;
		this.reloadSprite();
		this.Sprite.SpriteHeight = 32;
		this.Sprite.UpdateSourceRect();
		base.IsWalkingTowardPlayer = false;
		base.jitteriness.Value = 0.0;
	}

	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(this.throwing, "throwing").AddField(this.isMage, "isMage");
		base.position.Field.AxisAlignedMovement = true;
	}

	/// <inheritdoc />
	public override void reloadSprite(bool onlyAppearance = false)
	{
		this.Sprite = new AnimatedSprite("Characters\\Monsters\\Skeleton" + (this.isMage.Value ? " Mage" : ""));
		this.Sprite.SpriteHeight = 32;
		this.Sprite.UpdateSourceRect();
	}

	public override List<Item> getExtraDropItems()
	{
		List<Item> extra = new List<Item>();
		if (Game1.random.NextDouble() < 0.04)
		{
			extra.Add(ItemRegistry.Create("(W)5"));
		}
		return extra;
	}

	public override int takeDamage(int damage, int xTrajectory, int yTrajectory, bool isBomb, double addedPrecision, Farmer who)
	{
		base.currentLocation.playSound("skeletonHit");
		base.Slipperiness = 3;
		if (this.throwing.Value)
		{
			this.throwing.Value = false;
			this.Halt();
		}
		if (base.Health - damage <= 0)
		{
			Game1.multiplayer.broadcastSprites(base.currentLocation, new TemporaryAnimatedSprite(46, base.Position, Color.White, 10, flipped: false, 70f));
			Game1.multiplayer.broadcastSprites(base.currentLocation, new TemporaryAnimatedSprite(46, base.Position + new Vector2(-16f, 0f), Color.White, 10, flipped: false, 70f)
			{
				delayBeforeAnimationStart = 100
			});
			Game1.multiplayer.broadcastSprites(base.currentLocation, new TemporaryAnimatedSprite(46, base.Position + new Vector2(16f, 0f), Color.White, 10, flipped: false, 70f)
			{
				delayBeforeAnimationStart = 200
			});
		}
		return base.takeDamage(damage, xTrajectory, yTrajectory, isBomb, addedPrecision, who);
	}

	public override void shedChunks(int number)
	{
		Point standingPixel = base.StandingPixel;
		Game1.createRadialDebris(base.currentLocation, this.Sprite.textureName.Value, new Rectangle(0, 128, 16, 16), 8, standingPixel.X, standingPixel.Y, number, base.TilePoint.Y, Color.White, 4f);
	}

	public override void BuffForAdditionalDifficulty(int additional_difficulty)
	{
		base.BuffForAdditionalDifficulty(additional_difficulty);
		if (!this.isMage.Value)
		{
			base.MaxHealth += 300;
			base.Health += 300;
		}
	}

	protected override void sharedDeathAnimation()
	{
		Point standingPixel = base.StandingPixel;
		base.currentLocation.playSound("skeletonDie");
		this.shedChunks(20);
		Game1.createRadialDebris(base.currentLocation, this.Sprite.textureName.Value, new Rectangle(3, Game1.random.Choose(3, 35), 10, 10), 11, standingPixel.X, standingPixel.Y, 1, base.TilePoint.Y, Color.White, 4f);
	}

	public override void update(GameTime time, GameLocation location)
	{
		if (!this.throwing.Value)
		{
			base.update(time, location);
			return;
		}
		if (Game1.IsMasterGame)
		{
			this.behaviorAtGameTick(time);
		}
		this.updateAnimation(time);
	}

	protected override void updateMonsterSlaveAnimation(GameTime time)
	{
		if (this.throwing.Value)
		{
			if (base.invincibleCountdown > 0)
			{
				base.invincibleCountdown -= time.ElapsedGameTime.Milliseconds;
				if (base.invincibleCountdown <= 0)
				{
					base.stopGlowing();
				}
			}
			if (this.Sprite.Animate(time, 20, 4, 150f))
			{
				this.Sprite.currentFrame = 23;
			}
		}
		else if (this.isMoving())
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
		else
		{
			this.Sprite.StopAnimation();
		}
	}

	public override void behaviorAtGameTick(GameTime time)
	{
		if (!this.throwing.Value)
		{
			base.behaviorAtGameTick(time);
		}
		if (!this.spottedPlayer && !base.wildernessFarmMonster && Utility.doesPointHaveLineOfSightInMine(base.currentLocation, base.Tile, base.Player.Tile, 8))
		{
			base.controller = new PathFindController(this, base.currentLocation, base.Player.TilePoint, -1, null, 200);
			this.spottedPlayer = true;
			if (base.controller?.pathToEndPoint == null || base.controller.pathToEndPoint.Count == 0)
			{
				this.Halt();
				base.facePlayer(base.Player);
			}
			base.currentLocation.playSound("skeletonStep");
			base.IsWalkingTowardPlayer = true;
		}
		else if (this.throwing.Value)
		{
			if (base.invincibleCountdown > 0)
			{
				base.invincibleCountdown -= time.ElapsedGameTime.Milliseconds;
				if (base.invincibleCountdown <= 0)
				{
					base.stopGlowing();
				}
			}
			if (this.Sprite.Animate(time, 20, 4, 150f))
			{
				this.throwing.Value = false;
				this.Sprite.currentFrame = 0;
				this.faceDirection(2);
				Vector2 v = Utility.getVelocityTowardPlayer(new Point((int)base.Position.X, (int)base.Position.Y), 8f, base.Player);
				if (this.isMage.Value)
				{
					if (Game1.random.NextBool())
					{
						base.currentLocation.projectiles.Add(new DebuffingProjectile("19", 14, 4, 4, (float)Math.PI / 16f, v.X, v.Y, new Vector2(base.Position.X, base.Position.Y), base.currentLocation, this));
					}
					else
					{
						base.currentLocation.projectiles.Add(new BasicProjectile(base.DamageToFarmer * 2, 9, 0, 4, 0f, v.X, v.Y, new Vector2(base.Position.X, base.Position.Y), "flameSpellHit", "flameSpell", null, explode: false, damagesMonsters: false, base.currentLocation, this));
					}
				}
				else
				{
					base.currentLocation.projectiles.Add(new BasicProjectile(base.DamageToFarmer, 4, 0, 0, (float)Math.PI / 16f, v.X, v.Y, new Vector2(base.Position.X, base.Position.Y), "skeletonHit", "skeletonStep", null, explode: false, damagesMonsters: false, base.currentLocation, this));
				}
			}
		}
		else if (this.spottedPlayer && base.controller == null && Game1.random.NextDouble() < (this.isMage.Value ? 0.009 : 0.003) && !base.wildernessFarmMonster && Utility.doesPointHaveLineOfSightInMine(base.currentLocation, base.Tile, base.Player.Tile, 8))
		{
			this.throwing.Value = true;
			this.Halt();
			this.Sprite.currentFrame = 20;
			base.shake(750);
		}
		else if (this.withinPlayerThreshold(2))
		{
			base.controller = null;
		}
		else if (this.spottedPlayer && base.controller == null && this.controllerAttemptTimer <= 0)
		{
			base.controller = new PathFindController(this, base.currentLocation, base.Player.TilePoint, -1, null, 200);
			this.controllerAttemptTimer = (base.wildernessFarmMonster ? 2000 : 1000);
			if (base.controller?.pathToEndPoint == null || base.controller.pathToEndPoint.Count == 0)
			{
				this.Halt();
			}
		}
		else if (base.wildernessFarmMonster)
		{
			this.spottedPlayer = true;
			base.IsWalkingTowardPlayer = true;
		}
		this.controllerAttemptTimer -= time.ElapsedGameTime.Milliseconds;
	}
}
