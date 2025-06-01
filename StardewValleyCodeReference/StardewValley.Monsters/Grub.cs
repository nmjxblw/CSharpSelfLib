using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Extensions;
using StardewValley.Locations;

namespace StardewValley.Monsters;

public class Grub : Monster
{
	public const int healthToRunAway = 8;

	[XmlIgnore]
	public readonly NetBool leftDrift = new NetBool();

	[XmlIgnore]
	public readonly NetBool pupating = new NetBool();

	[XmlElement("hard")]
	public readonly NetBool hard = new NetBool();

	[XmlIgnore]
	public int metamorphCounter = 2000;

	[XmlIgnore]
	public readonly NetFloat targetRotation = new NetFloat();

	public Grub()
	{
	}

	public Grub(Vector2 position)
		: this(position, hard: false)
	{
	}

	public Grub(Vector2 position, bool hard)
		: base("Grub", position)
	{
		if (Game1.random.NextBool())
		{
			this.leftDrift.Value = true;
		}
		this.FacingDirection = Game1.random.Next(4);
		this.targetRotation.Value = (base.rotation = (float)Game1.random.Next(4) / (float)Math.PI);
		this.hard.Value = hard;
		if (hard)
		{
			base.DamageToFarmer *= 3;
			base.Health *= 5;
			base.MaxHealth = base.Health;
			base.ExperienceGained *= 3;
			if (Game1.random.NextDouble() < 0.1)
			{
				base.objectsToDrop.Add("456");
			}
		}
	}

	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(this.leftDrift, "leftDrift").AddField(this.pupating, "pupating").AddField(this.hard, "hard")
			.AddField(this.targetRotation, "targetRotation");
		base.position.Field.AxisAlignedMovement = true;
	}

	/// <inheritdoc />
	public override void reloadSprite(bool onlyAppearance = false)
	{
		base.reloadSprite(onlyAppearance);
		this.Sprite.SpriteHeight = 24;
		this.Sprite.UpdateSourceRect();
	}

	public void setHard()
	{
		this.hard.Value = true;
		if (this.hard.Value)
		{
			base.DamageToFarmer = 12;
			base.Health = 100;
			base.MaxHealth = base.Health;
			base.ExperienceGained = 10;
			if (Game1.random.NextDouble() < 0.1)
			{
				base.objectsToDrop.Add("456");
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
			base.currentLocation.playSound("slimeHit");
			if (this.pupating.Value)
			{
				base.currentLocation.playSound("crafting");
				base.setTrajectory(xTrajectory / 2, yTrajectory / 2);
				return 0;
			}
			base.Slipperiness = 4;
			base.Health -= actualDamage;
			base.setTrajectory(xTrajectory, yTrajectory);
			if (base.Health <= 0)
			{
				base.currentLocation.playSound("slimedead");
				Utility.makeTemporarySpriteJuicier(new TemporaryAnimatedSprite(44, base.Position, base.isHardModeMonster.Value ? Color.LimeGreen : Color.Orange, 10)
				{
					holdLastFrame = true,
					alphaFade = 0.01f,
					interval = 50f
				}, base.currentLocation);
			}
		}
		return actualDamage;
	}

	public override void defaultMovementBehavior(GameTime time)
	{
		base.Scale = 1f + (float)(0.125 * Math.Sin(time.TotalGameTime.TotalMilliseconds / (double)(500f + base.Position.X / 100f)));
	}

	public override void BuffForAdditionalDifficulty(int additional_difficulty)
	{
		base.BuffForAdditionalDifficulty(additional_difficulty);
		base.rotation = 0f;
		this.targetRotation.Value = 0f;
	}

	public override void update(GameTime time, GameLocation location)
	{
		if ((base.Health > 8 || (this.hard.Value && base.Health >= base.MaxHealth)) && !this.pupating.Value)
		{
			base.update(time, location);
			return;
		}
		if (base.invincibleCountdown > 0)
		{
			base.invincibleCountdown -= time.ElapsedGameTime.Milliseconds;
			if (base.invincibleCountdown <= 0)
			{
				base.stopGlowing();
			}
		}
		if (Game1.IsMasterGame)
		{
			this.behaviorAtGameTick(time);
		}
		this.updateAnimation(time);
	}

	public override void draw(SpriteBatch b)
	{
		b.Draw(this.Sprite.Texture, base.getLocalPosition(Game1.viewport) + new Vector2(this.Sprite.SpriteWidth * 4 / 2, this.GetBoundingBox().Height / 2) + ((base.shakeTimer > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero), this.Sprite.SourceRect, this.hard.Value ? Color.Lime : Color.White, base.rotation, new Vector2(this.Sprite.SpriteWidth / 2, (float)this.Sprite.SpriteHeight * 3f / 4f), Math.Max(0.2f, base.scale.Value) * 4f, (base.flip || (this.Sprite.CurrentAnimation != null && this.Sprite.CurrentAnimation[this.Sprite.currentAnimationIndex].flip)) ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, base.drawOnTop ? 0.991f : ((float)base.StandingPixel.Y / 10000f)));
	}

	protected override void updateMonsterSlaveAnimation(GameTime time)
	{
		if (this.pupating.Value)
		{
			base.Scale = 1f + (float)Math.Sin((float)time.TotalGameTime.Milliseconds * ((float)Math.PI / 8f)) / 12f;
			this.metamorphCounter -= time.ElapsedGameTime.Milliseconds;
		}
		else if (base.Health <= 8 || (this.hard.Value && base.Health < base.MaxHealth))
		{
			this.metamorphCounter -= time.ElapsedGameTime.Milliseconds;
			if (this.metamorphCounter <= 0)
			{
				this.Sprite.Animate(time, 16, 4, 125f);
				if (this.Sprite.currentFrame == 19)
				{
					this.metamorphCounter = 4500;
				}
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
			base.rotation = 0f;
			base.Scale = 1f;
		}
		else if (!this.withinPlayerThreshold())
		{
			this.Halt();
			base.rotation = this.targetRotation.Value;
		}
	}

	public override void behaviorAtGameTick(GameTime time)
	{
		base.behaviorAtGameTick(time);
		if (this.pupating.Value)
		{
			base.Scale = 1f + (float)Math.Sin((float)time.TotalGameTime.Milliseconds * ((float)Math.PI / 8f)) / 12f;
			this.metamorphCounter -= time.ElapsedGameTime.Milliseconds;
			if (this.metamorphCounter <= 0)
			{
				Point standingPixel = base.StandingPixel;
				base.Health = -500;
				Game1.createRadialDebris(base.currentLocation, this.Sprite.textureName.Value, new Rectangle(208, 424, 32, 40), 4, standingPixel.X, standingPixel.Y, 25, base.TilePoint.Y);
				Game1.createRadialDebris(base.currentLocation, this.Sprite.textureName.Value, new Rectangle(208, 424, 32, 40), 8, standingPixel.X, standingPixel.Y, 15, base.TilePoint.Y);
				if (base.currentLocation is MineShaft mine)
				{
					base.currentLocation.characters.Add(mine.BuffMonsterIfNecessary(new Fly(base.Position, this.hard.Value)
					{
						currentLocation = base.currentLocation
					}));
				}
				else
				{
					base.currentLocation.characters.Add(new Fly(base.Position, this.hard.Value)
					{
						currentLocation = base.currentLocation
					});
				}
			}
		}
		else if (base.Health <= base.MaxHealth / 2 - 2 || (this.hard.Value && base.Health < base.MaxHealth))
		{
			this.metamorphCounter -= time.ElapsedGameTime.Milliseconds;
			if (this.metamorphCounter <= 0)
			{
				this.Sprite.Animate(time, 16, 4, 125f);
				if (this.Sprite.currentFrame == 19)
				{
					this.pupating.Value = true;
					this.metamorphCounter = 4500;
				}
				return;
			}
			Point monsterPixel = base.StandingPixel;
			Point playerPixel = base.Player.StandingPixel;
			if (Math.Abs(playerPixel.Y - monsterPixel.Y) > 128)
			{
				if (playerPixel.X > monsterPixel.X)
				{
					this.SetMovingLeft(b: true);
				}
				else
				{
					this.SetMovingRight(b: true);
				}
			}
			else if (Math.Abs(playerPixel.X - monsterPixel.X) > 128)
			{
				if (playerPixel.Y > monsterPixel.Y)
				{
					this.SetMovingUp(b: true);
				}
				else
				{
					this.SetMovingDown(b: true);
				}
			}
			this.MovePosition(time, Game1.viewport, base.currentLocation);
		}
		else if (this.withinPlayerThreshold())
		{
			base.Scale = 1f;
			base.rotation = 0f;
		}
		else if (this.isMoving())
		{
			this.Halt();
			this.faceDirection(Game1.random.Next(4));
			this.targetRotation.Value = (base.rotation = (float)Game1.random.Next(4) / (float)Math.PI);
		}
	}
}
