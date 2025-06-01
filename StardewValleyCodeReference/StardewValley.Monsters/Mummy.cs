using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.Enchantments;
using StardewValley.Tools;

namespace StardewValley.Monsters;

public class Mummy : Monster
{
	public NetInt reviveTimer = new NetInt(0);

	public const int revivalTime = 10000;

	protected int _damageToFarmer;

	private readonly NetEvent1Field<bool, NetBool> crumbleEvent = new NetEvent1Field<bool, NetBool>();

	public Mummy()
	{
	}

	public Mummy(Vector2 position)
		: base("Mummy", position)
	{
		this.Sprite.SpriteHeight = 32;
		this.Sprite.ignoreStopAnimation = true;
		this.Sprite.UpdateSourceRect();
		this._damageToFarmer = base.damageToFarmer.Value;
	}

	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(this.crumbleEvent, "crumbleEvent").AddField(this.reviveTimer, "reviveTimer");
		this.crumbleEvent.onEvent += performCrumble;
		base.position.Field.AxisAlignedMovement = true;
	}

	/// <inheritdoc />
	public override void reloadSprite(bool onlyAppearance = false)
	{
		this.Sprite = new AnimatedSprite("Characters\\Monsters\\Mummy");
		this.Sprite.SpriteHeight = 32;
		this.Sprite.UpdateSourceRect();
		this.Sprite.ignoreStopAnimation = true;
	}

	public override int takeDamage(int damage, int xTrajectory, int yTrajectory, bool isBomb, double addedPrecision, Farmer who)
	{
		int actualDamage = Math.Max(1, damage - base.resilience.Value);
		if (this.reviveTimer.Value > 0)
		{
			if (isBomb)
			{
				base.Health = 0;
				Utility.makeTemporarySpriteJuicier(new TemporaryAnimatedSprite(44, base.Position, Color.BlueViolet, 10)
				{
					holdLastFrame = true,
					alphaFade = 0.01f,
					interval = 70f
				}, base.currentLocation);
				base.currentLocation.playSound("ghost");
				return 999;
			}
			return -1;
		}
		if (Game1.random.NextDouble() < base.missChance.Value - base.missChance.Value * addedPrecision)
		{
			actualDamage = -1;
		}
		else
		{
			base.Slipperiness = 2;
			base.Health -= actualDamage;
			base.setTrajectory(xTrajectory, yTrajectory);
			base.currentLocation.playSound("shadowHit");
			base.currentLocation.playSound("skeletonStep");
			base.IsWalkingTowardPlayer = true;
			if (base.Health <= 0)
			{
				if (!isBomb && who.CurrentTool is MeleeWeapon weapon && weapon.hasEnchantmentOfType<CrusaderEnchantment>())
				{
					Utility.makeTemporarySpriteJuicier(new TemporaryAnimatedSprite(44, base.Position, Color.BlueViolet, 10)
					{
						holdLastFrame = true,
						alphaFade = 0.01f,
						interval = 70f
					}, base.currentLocation);
					base.currentLocation.playSound("ghost");
				}
				else
				{
					this.reviveTimer.Value = 10000;
					base.Health = base.MaxHealth;
					base.deathAnimation();
				}
			}
		}
		return actualDamage;
	}

	public override void defaultMovementBehavior(GameTime time)
	{
		if (this.reviveTimer.Value <= 0)
		{
			base.defaultMovementBehavior(time);
		}
	}

	public override List<Item> getExtraDropItems()
	{
		List<Item> items = new List<Item>();
		if (Game1.random.NextDouble() < 0.002)
		{
			items.Add(ItemRegistry.Create("(O)485"));
		}
		return items;
	}

	protected override void sharedDeathAnimation()
	{
		this.Halt();
		this.crumble();
		base.collidesWithOtherCharacters.Value = false;
		base.IsWalkingTowardPlayer = false;
		base.moveTowardPlayerThreshold.Value = -1;
	}

	protected override void localDeathAnimation()
	{
	}

	public override void update(GameTime time, GameLocation location)
	{
		this.crumbleEvent.Poll();
		if (this.reviveTimer.Value > 0 && this.Sprite.CurrentAnimation == null && this.Sprite.currentFrame != 19)
		{
			this.Sprite.currentFrame = 19;
		}
		base.update(time, location);
	}

	private void crumble(bool reverse = false)
	{
		this.crumbleEvent.Fire(reverse);
	}

	private void performCrumble(bool reverse)
	{
		this.Sprite.setCurrentAnimation(this.getCrumbleAnimation(reverse));
		if (!reverse)
		{
			if (Game1.IsMasterGame)
			{
				base.damageToFarmer.Value = 0;
			}
			this.reviveTimer.Value = 10000;
			base.currentLocation.localSound("monsterdead");
		}
		else
		{
			if (Game1.IsMasterGame)
			{
				base.damageToFarmer.Value = this._damageToFarmer;
			}
			this.reviveTimer.Value = 0;
			base.currentLocation.localSound("skeletonDie");
		}
	}

	private List<FarmerSprite.AnimationFrame> getCrumbleAnimation(bool reverse = false)
	{
		List<FarmerSprite.AnimationFrame> animation = new List<FarmerSprite.AnimationFrame>();
		if (!reverse)
		{
			animation.Add(new FarmerSprite.AnimationFrame(16, 100, 0, secondaryArm: false, flip: false));
		}
		else
		{
			animation.Add(new FarmerSprite.AnimationFrame(16, 100, 0, secondaryArm: false, flip: false, behaviorAfterRevival, behaviorAtEndOfFrame: true));
		}
		animation.Add(new FarmerSprite.AnimationFrame(17, 100, 0, secondaryArm: false, flip: false));
		animation.Add(new FarmerSprite.AnimationFrame(18, 100, 0, secondaryArm: false, flip: false));
		if (!reverse)
		{
			animation.Add(new FarmerSprite.AnimationFrame(19, 100, 0, secondaryArm: false, flip: false, behaviorAfterCrumble));
		}
		else
		{
			animation.Add(new FarmerSprite.AnimationFrame(19, 100, 0, secondaryArm: false, flip: false));
		}
		if (reverse)
		{
			animation.Reverse();
		}
		return animation;
	}

	public override void behaviorAtGameTick(GameTime time)
	{
		if (this.reviveTimer.Value <= 0 && this.withinPlayerThreshold())
		{
			base.IsWalkingTowardPlayer = true;
		}
		base.behaviorAtGameTick(time);
	}

	protected override void updateAnimation(GameTime time)
	{
		if (this.Sprite.CurrentAnimation != null)
		{
			if (this.Sprite.animateOnce(time))
			{
				this.Sprite.CurrentAnimation = null;
			}
		}
		else if (this.reviveTimer.Value > 0)
		{
			this.reviveTimer.Value -= time.ElapsedGameTime.Milliseconds;
			if (this.reviveTimer.Value < 2000)
			{
				base.shake(this.reviveTimer.Value);
			}
			if (this.reviveTimer.Value <= 0)
			{
				if (Game1.IsMasterGame)
				{
					this.crumble(reverse: true);
					base.IsWalkingTowardPlayer = true;
				}
				else
				{
					this.reviveTimer.Value = 1;
				}
			}
		}
		else if (!Game1.IsMasterGame)
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
		}
		base.resetAnimationSpeed();
	}

	private void behaviorAfterCrumble(Farmer who)
	{
		this.Halt();
		this.Sprite.currentFrame = 19;
		this.Sprite.CurrentAnimation = null;
	}

	private void behaviorAfterRevival(Farmer who)
	{
		base.IsWalkingTowardPlayer = true;
		base.collidesWithOtherCharacters.Value = true;
		this.Sprite.currentFrame = 0;
		this.Sprite.oldFrame = 0;
		base.moveTowardPlayerThreshold.Value = 8;
		this.Sprite.CurrentAnimation = null;
	}
}
