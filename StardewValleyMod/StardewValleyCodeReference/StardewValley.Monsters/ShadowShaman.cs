using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Extensions;
using StardewValley.Pathfinding;
using StardewValley.Projectiles;

namespace StardewValley.Monsters;

public class ShadowShaman : Monster
{
	public const int visionDistance = 8;

	public const int spellCooldown = 1500;

	[XmlIgnore]
	public bool spottedPlayer;

	[XmlIgnore]
	public readonly NetBool casting = new NetBool();

	[XmlIgnore]
	public int coolDown = 1500;

	[XmlIgnore]
	public float rotationTimer;

	public ShadowShaman()
	{
	}

	public ShadowShaman(Vector2 position)
		: base("Shadow Shaman", position)
	{
		if (Game1.MasterPlayer.friendshipData.TryGetValue("???", out var friendship) && friendship.Points >= 1250)
		{
			base.DamageToFarmer = 0;
		}
	}

	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(this.casting, "casting");
	}

	/// <inheritdoc />
	public override void reloadSprite(bool onlyAppearance = false)
	{
		this.Sprite = new AnimatedSprite("Characters\\Monsters\\Shadow Shaman");
	}

	public override void draw(SpriteBatch b)
	{
		base.draw(b);
		if (this.casting.Value)
		{
			for (int i = 0; i < 8; i++)
			{
				b.Draw(Projectile.projectileSheet, Game1.GlobalToLocal(Game1.viewport, base.getStandingPosition()), new Rectangle(119, 6, 3, 3), Color.White * 0.7f, this.rotationTimer + (float)i * (float)Math.PI / 4f, new Vector2(8f, 48f), 6f, SpriteEffects.None, 0.95f);
			}
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
			if (this.casting.Value && Game1.random.NextBool())
			{
				this.coolDown += 200;
			}
			else
			{
				base.setTrajectory(xTrajectory, yTrajectory);
				base.currentLocation.playSound("shadowHit");
			}
			if (base.Health <= 0)
			{
				base.currentLocation.playSound("shadowDie");
				base.deathAnimation();
			}
		}
		return actualDamage;
	}

	protected override void sharedDeathAnimation()
	{
		Point standingPixel = base.StandingPixel;
		Game1.createRadialDebris(base.currentLocation, this.Sprite.textureName.Value, new Rectangle(this.Sprite.SourceRect.X, this.Sprite.SourceRect.Y, 16, 5), 16, standingPixel.X, standingPixel.Y - 32, 1, standingPixel.Y / 64, Color.White);
		Game1.createRadialDebris(base.currentLocation, this.Sprite.textureName.Value, new Rectangle(this.Sprite.SourceRect.X + 2, this.Sprite.SourceRect.Y + 5, 16, 5), 10, standingPixel.X, standingPixel.Y - 32, 1, standingPixel.Y / 64, Color.White);
		Game1.createRadialDebris(base.currentLocation, this.Sprite.textureName.Value, new Rectangle(0, 10, 16, 5), 16, standingPixel.X, standingPixel.Y - 32, 1, standingPixel.Y / 64, Color.White);
	}

	protected override void localDeathAnimation()
	{
		Utility.makeTemporarySpriteJuicier(new TemporaryAnimatedSprite(45, base.Position, Color.White, 10), base.currentLocation);
		for (int i = 1; i < 3; i++)
		{
			base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(6, base.Position + new Vector2(1f, 1f) * 64f * i, Color.Gray * 0.75f, 10)
			{
				delayBeforeAnimationStart = i * 159
			});
			base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(6, base.Position + new Vector2(1f, -1f) * 64f * i, Color.Gray * 0.75f, 10)
			{
				delayBeforeAnimationStart = i * 159
			});
			base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(6, base.Position + new Vector2(-1f, 1f) * 64f * i, Color.Gray * 0.75f, 10)
			{
				delayBeforeAnimationStart = i * 159
			});
			base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(6, base.Position + new Vector2(-1f, -1f) * 64f * i, Color.Gray * 0.75f, 10)
			{
				delayBeforeAnimationStart = i * 159
			});
		}
	}

	protected override void updateMonsterSlaveAnimation(GameTime time)
	{
		if (this.casting.Value)
		{
			this.Sprite.Animate(time, 16, 4, 200f);
			this.rotationTimer = (float)((double)((float)time.TotalGameTime.Milliseconds * ((float)Math.PI / 128f) / 24f) % (Math.PI * 1024.0));
		}
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
		else
		{
			this.Sprite.StopAnimation();
		}
	}

	public override void behaviorAtGameTick(GameTime time)
	{
		base.behaviorAtGameTick(time);
		if (base.timeBeforeAIMovementAgain <= 0f)
		{
			base.IsInvisible = false;
		}
		if (!this.spottedPlayer && Utility.couldSeePlayerInPeripheralVision(base.Player, this) && Utility.doesPointHaveLineOfSightInMine(base.currentLocation, base.Tile, base.Player.Tile, 8))
		{
			base.controller = null;
			this.spottedPlayer = true;
			this.Halt();
			base.facePlayer(base.Player);
			if (Game1.random.NextDouble() < 0.3)
			{
				base.currentLocation.playSound("shadowpeep");
			}
		}
		else if (this.casting.Value)
		{
			base.IsWalkingTowardPlayer = false;
			this.Sprite.Animate(time, 16, 4, 200f);
			this.rotationTimer = (float)((double)((float)time.TotalGameTime.Milliseconds * ((float)Math.PI / 128f) / 24f) % (Math.PI * 1024.0));
			this.coolDown -= time.ElapsedGameTime.Milliseconds;
			if (this.coolDown > 0)
			{
				return;
			}
			base.Scale = 1f;
			Rectangle monsterBounds = this.GetBoundingBox();
			Vector2 velocityTowardPlayer = Utility.getVelocityTowardPlayer(monsterBounds.Center, 15f, base.Player);
			if (base.Player.Attack >= 0 && Game1.random.NextDouble() < 0.6)
			{
				base.currentLocation.projectiles.Add(new DebuffingProjectile("14", 7, 4, 4, (float)Math.PI / 16f, velocityTowardPlayer.X, velocityTowardPlayer.Y, new Vector2(monsterBounds.X, monsterBounds.Y), base.currentLocation, this));
			}
			else
			{
				List<Monster> monstersNearPlayer = new List<Monster>();
				foreach (NPC character in base.currentLocation.characters)
				{
					if (character is Monster monster && monster.withinPlayerThreshold(6))
					{
						monstersNearPlayer.Add(monster);
					}
				}
				Monster lowestHealthMonster = null;
				double lowestHealth = 1.0;
				foreach (Monster m in monstersNearPlayer)
				{
					if ((double)m.Health / (double)m.MaxHealth <= lowestHealth)
					{
						lowestHealthMonster = m;
						lowestHealth = (double)m.Health / (double)m.MaxHealth;
					}
				}
				if (lowestHealthMonster != null)
				{
					int amountToHeal = (base.isHardModeMonster.Value ? 250 : 60);
					lowestHealthMonster.Health = Math.Min(lowestHealthMonster.MaxHealth, lowestHealthMonster.Health + amountToHeal);
					base.currentLocation.playSound("healSound");
					Game1.multiplayer.broadcastSprites(base.currentLocation, new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 256, 64, 64), 40f, 8, 0, lowestHealthMonster.Position + new Vector2(32f, 64f), flicker: false, flipped: false));
					base.currentLocation.debris.Add(new Debris(amountToHeal, new Vector2(lowestHealthMonster.GetBoundingBox().Center.X, lowestHealthMonster.GetBoundingBox().Center.Y), Color.Green, 1f, lowestHealthMonster));
				}
			}
			this.casting.Value = false;
			this.coolDown = 1500;
			base.IsWalkingTowardPlayer = true;
		}
		else if (this.spottedPlayer)
		{
			if (this.withinPlayerThreshold(8))
			{
				if (base.Health < 30)
				{
					base.IsWalkingTowardPlayer = false;
					Point monsterPixel = base.StandingPixel;
					Point playerPixel = base.Player.StandingPixel;
					if (Math.Abs(playerPixel.Y - monsterPixel.Y) > 192)
					{
						if (playerPixel.X - monsterPixel.X > 0)
						{
							this.SetMovingLeft(b: true);
						}
						else
						{
							this.SetMovingRight(b: true);
						}
					}
					else if (playerPixel.Y - monsterPixel.Y > 0)
					{
						this.SetMovingUp(b: true);
					}
					else
					{
						this.SetMovingDown(b: true);
					}
				}
				else if (base.controller == null && !Utility.doesPointHaveLineOfSightInMine(base.currentLocation, base.Tile, base.Player.Tile, 8))
				{
					base.controller = new PathFindController(this, base.currentLocation, base.Player.TilePoint, -1, null, 300);
					if (base.controller?.pathToEndPoint == null || base.controller.pathToEndPoint.Count == 0)
					{
						this.spottedPlayer = false;
						this.Halt();
						base.controller = null;
						this.addedSpeed = 0f;
					}
				}
				else if (this.coolDown <= 0 && Game1.random.NextDouble() < 0.02)
				{
					this.casting.Value = true;
					base.controller = null;
					base.IsWalkingTowardPlayer = false;
					this.Halt();
					this.coolDown = 500;
				}
				this.coolDown -= time.ElapsedGameTime.Milliseconds;
			}
			else
			{
				base.IsWalkingTowardPlayer = false;
				this.spottedPlayer = false;
				base.controller = null;
				this.addedSpeed = 0f;
			}
		}
		else
		{
			this.defaultMovementBehavior(time);
		}
	}
}
