using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.Extensions;
using StardewValley.Tools;

namespace StardewValley.Monsters;

public class RockCrab : Monster
{
	[XmlIgnore]
	public bool waiter;

	[XmlIgnore]
	public readonly NetBool shellGone = new NetBool();

	[XmlIgnore]
	public readonly NetInt shellHealth = new NetInt(5);

	[XmlIgnore]
	public readonly NetBool isStickBug = new NetBool();

	public RockCrab()
	{
	}

	public RockCrab(Vector2 position)
		: base("Rock Crab", position)
	{
		this.waiter = Game1.random.NextDouble() < 0.4;
		base.moveTowardPlayerThreshold.Value = 3;
	}

	/// <inheritdoc />
	public override void reloadSprite(bool onlyAppearance = false)
	{
		base.reloadSprite(onlyAppearance);
		this.Sprite.UpdateSourceRect();
	}

	/// <summary>
	/// constructor for Lava Crab
	/// </summary>
	/// <param name="position"></param>
	/// <param name="name"></param>
	public RockCrab(Vector2 position, string name)
		: base(name, position)
	{
		this.waiter = Game1.random.NextDouble() < 0.4;
		base.moveTowardPlayerThreshold.Value = 3;
		switch (name)
		{
		case "Truffle Crab":
			this.waiter = false;
			base.moveTowardPlayerThreshold.Value = 1;
			break;
		case "Iridium Crab":
			this.waiter = true;
			base.moveTowardPlayerThreshold.Value = 1;
			break;
		case "False Magma Cap":
			this.waiter = false;
			break;
		}
	}

	public void makeStickBug()
	{
		this.isStickBug.Value = true;
		this.waiter = false;
		base.Name = "Stick Bug";
		base.DamageToFarmer = 20;
		base.MaxHealth = 700;
		base.Health = 700;
		base.reloadSprite();
		base.HideShadow = true;
		this.Sprite.SpriteHeight = 24;
		this.Sprite.UpdateSourceRect();
		base.objectsToDrop.Clear();
		base.objectsToDrop.Add("858");
		while (Game1.random.NextBool())
		{
			base.objectsToDrop.Add("858");
		}
		base.objectsToDrop.Add("829");
	}

	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(this.shellGone, "shellGone").AddField(this.shellHealth, "shellHealth").AddField(this.isStickBug, "isStickBug");
		base.position.Field.AxisAlignedMovement = true;
	}

	public override bool hitWithTool(Tool t)
	{
		if (this.isStickBug.Value)
		{
			return false;
		}
		if (t is Pickaxe && t.getLastFarmerToUse() != null && this.shellHealth.Value > 0)
		{
			base.currentLocation.playSound("hammer");
			this.shellHealth.Value--;
			base.shake(500);
			this.waiter = false;
			base.moveTowardPlayerThreshold.Value = 3;
			this.setTrajectory(Utility.getAwayFromPlayerTrajectory(this.GetBoundingBox(), t.getLastFarmerToUse()));
			if (this.shellHealth.Value <= 0)
			{
				Point tile = base.TilePoint;
				this.shellGone.Value = true;
				base.moveTowardPlayer(-1);
				base.currentLocation.playSound("stoneCrack");
				Game1.createRadialDebris(base.currentLocation, 14, tile.X, tile.Y, Game1.random.Next(2, 7), resource: false);
				Game1.createRadialDebris(base.currentLocation, 14, tile.X, tile.Y, Game1.random.Next(2, 7), resource: false);
			}
			return true;
		}
		return base.hitWithTool(t);
	}

	public override void shedChunks(int number)
	{
		Point standingPixel = base.StandingPixel;
		Game1.createRadialDebris(base.currentLocation, this.Sprite.textureName.Value, new Rectangle(0, 120, 16, 16), 8, standingPixel.X, standingPixel.Y, number, base.TilePoint.Y, Color.White, 4f * base.scale.Value);
	}

	public override int takeDamage(int damage, int xTrajectory, int yTrajectory, bool isBomb, double addedPrecision, Farmer who)
	{
		int actualDamage = Math.Max(1, damage - base.resilience.Value);
		if (isBomb && !this.isStickBug.Value)
		{
			this.shellGone.Value = true;
			this.waiter = false;
			base.moveTowardPlayer(-1);
		}
		if (Game1.random.NextDouble() < base.missChance.Value - base.missChance.Value * addedPrecision)
		{
			actualDamage = -1;
		}
		else if (this.Sprite.currentFrame % 4 == 0 && !this.shellGone.Value)
		{
			actualDamage = 0;
			base.currentLocation.playSound("crafting");
		}
		else
		{
			base.Health -= actualDamage;
			base.Slipperiness = 3;
			base.setTrajectory(xTrajectory, yTrajectory);
			base.currentLocation.playSound("hitEnemy");
			base.glowingColor = Color.Cyan;
			if (base.Health <= 0)
			{
				base.currentLocation.playSound("monsterdead");
				base.deathAnimation();
				Utility.makeTemporarySpriteJuicier(new TemporaryAnimatedSprite(44, base.Position, Color.Red, 10)
				{
					holdLastFrame = true,
					alphaFade = 0.01f
				}, base.currentLocation);
			}
		}
		return actualDamage;
	}

	public override void update(GameTime time, GameLocation location)
	{
		if (!location.farmers.Any())
		{
			return;
		}
		if (!this.shellGone.Value && !base.Player.isRafting)
		{
			base.update(time, location);
		}
		else if (!base.Player.isRafting)
		{
			if (Game1.IsMasterGame)
			{
				this.behaviorAtGameTick(time);
			}
			this.updateAnimation(time);
		}
	}

	public override void behaviorAtGameTick(GameTime time)
	{
		if (this.waiter && this.shellHealth.Value > 4)
		{
			base.moveTowardPlayerThreshold.Value = 0;
			return;
		}
		base.behaviorAtGameTick(time);
		if (this.isMoving() && this.Sprite.currentFrame % 4 == 0)
		{
			this.Sprite.currentFrame++;
			this.Sprite.UpdateSourceRect();
		}
		if (!this.withinPlayerThreshold() && !this.shellGone.Value)
		{
			this.Halt();
		}
		else if (this.withinPlayerThreshold() && !this.shellGone.Value && base.name.Equals("Truffle Crab"))
		{
			this.shellGone.Value = true;
		}
		else
		{
			if (!this.shellGone.Value)
			{
				return;
			}
			base.updateGlow();
			if (base.invincibleCountdown > 0)
			{
				base.glowingColor = Color.Cyan;
				base.invincibleCountdown -= time.ElapsedGameTime.Milliseconds;
				if (base.invincibleCountdown <= 0)
				{
					base.stopGlowing();
				}
			}
			base.IsWalkingTowardPlayer = false;
			_ = base.StandingPixel;
			_ = base.Player.StandingPixel;
			this.FacingDirection = base.getGeneralDirectionTowards(base.Player.getStandingPosition(), 0, opposite: true, useTileCalculations: false);
			base.moveUp = false;
			base.moveDown = false;
			base.moveRight = false;
			base.moveLeft = false;
			base.setMovingInFacingDirection();
			this.MovePosition(time, Game1.viewport, base.currentLocation);
			this.Sprite.CurrentFrame = 16 + this.Sprite.currentFrame % 4;
		}
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
		else
		{
			this.Sprite.StopAnimation();
		}
		if (this.isMoving() && this.Sprite.currentFrame % 4 == 0)
		{
			this.Sprite.currentFrame++;
			this.Sprite.UpdateSourceRect();
		}
		if (!this.shellGone.Value)
		{
			return;
		}
		base.updateGlow();
		if (base.invincibleCountdown > 0)
		{
			base.glowingColor = Color.Cyan;
			base.invincibleCountdown -= time.ElapsedGameTime.Milliseconds;
			if (base.invincibleCountdown <= 0)
			{
				base.stopGlowing();
			}
		}
		this.Sprite.currentFrame = 16 + this.Sprite.currentFrame % 4;
	}
}
