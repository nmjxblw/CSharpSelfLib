using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Extensions;
using StardewValley.Locations;
using xTile.Dimensions;
using xTile.Layers;
using xTile.Tiles;

namespace StardewValley.Monsters;

[XmlInclude(typeof(AngryRoger))]
[XmlInclude(typeof(Bat))]
[XmlInclude(typeof(BigSlime))]
[XmlInclude(typeof(BlueSquid))]
[XmlInclude(typeof(Bug))]
[XmlInclude(typeof(DinoMonster))]
[XmlInclude(typeof(Duggy))]
[XmlInclude(typeof(DustSpirit))]
[XmlInclude(typeof(DwarvishSentry))]
[XmlInclude(typeof(Fly))]
[XmlInclude(typeof(Ghost))]
[XmlInclude(typeof(GreenSlime))]
[XmlInclude(typeof(Grub))]
[XmlInclude(typeof(HotHead))]
[XmlInclude(typeof(LavaLurk))]
[XmlInclude(typeof(Leaper))]
[XmlInclude(typeof(MetalHead))]
[XmlInclude(typeof(Mummy))]
[XmlInclude(typeof(RockCrab))]
[XmlInclude(typeof(RockGolem))]
[XmlInclude(typeof(Serpent))]
[XmlInclude(typeof(ShadowBrute))]
[XmlInclude(typeof(ShadowGirl))]
[XmlInclude(typeof(ShadowGuy))]
[XmlInclude(typeof(ShadowShaman))]
[XmlInclude(typeof(Shooter))]
[XmlInclude(typeof(Skeleton))]
[XmlInclude(typeof(Spiker))]
[XmlInclude(typeof(SquidKid))]
public class Monster : NPC
{
	public delegate void collisionBehavior(GameLocation location);

	public const int index_health = 0;

	public const int index_damageToFarmer = 1;

	public const int index_isGlider = 4;

	public const int index_drops = 6;

	public const int index_resilience = 7;

	public const int index_jitteriness = 8;

	public const int index_distanceThresholdToMoveTowardsPlayer = 9;

	public const int index_speed = 10;

	public const int index_missChance = 11;

	public const int index_isMineMonster = 12;

	public const int index_experiencePoints = 13;

	public const int index_displayName = 14;

	public const int defaultInvincibleCountdown = 450;

	public float timeBeforeAIMovementAgain;

	[XmlElement("damageToFarmer")]
	public readonly NetInt damageToFarmer = new NetInt();

	[XmlElement("health")]
	public readonly NetIntDelta health = new NetIntDelta();

	[XmlElement("maxHealth")]
	public readonly NetInt maxHealth = new NetInt();

	[XmlElement("resilience")]
	public readonly NetInt resilience = new NetInt();

	[XmlElement("slipperiness")]
	public readonly NetInt slipperiness = new NetInt(2);

	[XmlElement("experienceGained")]
	public readonly NetInt experienceGained = new NetInt();

	[XmlElement("jitteriness")]
	public readonly NetDouble jitteriness = new NetDouble();

	[XmlElement("missChance")]
	public readonly NetDouble missChance = new NetDouble();

	[XmlElement("isGlider")]
	public readonly NetBool isGlider = new NetBool();

	[XmlElement("mineMonster")]
	public readonly NetBool mineMonster = new NetBool();

	[XmlElement("hasSpecialItem")]
	public readonly NetBool hasSpecialItem = new NetBool();

	[XmlIgnore]
	public readonly NetFloat synchedRotation = new NetFloat().Interpolated(interpolate: true, wait: true);

	[XmlArrayItem("int")]
	public readonly NetStringList objectsToDrop = new NetStringList();

	[XmlIgnore]
	public int skipHorizontal;

	[XmlIgnore]
	public int invincibleCountdown;

	[XmlIgnore]
	public readonly NetInt defaultAnimationInterval = new NetInt(175);

	public readonly NetInt stunTime = new NetInt(0);

	[XmlElement("initializedForLocation")]
	public bool initializedForLocation;

	[XmlIgnore]
	public readonly NetBool netFocusedOnFarmers = new NetBool();

	[XmlIgnore]
	public readonly NetBool netWildernessFarmMonster = new NetBool();

	private readonly NetEvent1<ParryEventArgs> parryEvent = new NetEvent1<ParryEventArgs>
	{
		InterpolationWait = false
	};

	private readonly NetEvent1Field<Vector2, NetVector2> trajectoryEvent = new NetEvent1Field<Vector2, NetVector2>
	{
		InterpolationWait = false
	};

	[XmlIgnore]
	private readonly NetEvent0 deathAnimEvent = new NetEvent0();

	[XmlElement("ignoreDamageLOS")]
	public readonly NetBool ignoreDamageLOS = new NetBool();

	[XmlIgnore]
	public collisionBehavior onCollision;

	[XmlElement("isHardModeMonster")]
	public NetBool isHardModeMonster = new NetBool(value: false);

	private int slideAnimationTimer;

	[XmlIgnore]
	public Farmer Player => this.findPlayer();

	[XmlIgnore]
	public int DamageToFarmer
	{
		get
		{
			return this.damageToFarmer.Value;
		}
		set
		{
			this.damageToFarmer.Value = value;
		}
	}

	[XmlIgnore]
	public int Health
	{
		get
		{
			return this.health.Value;
		}
		set
		{
			this.health.Value = value;
		}
	}

	[XmlIgnore]
	public int MaxHealth
	{
		get
		{
			return this.maxHealth.Value;
		}
		set
		{
			this.maxHealth.Value = value;
		}
	}

	[XmlIgnore]
	public int ExperienceGained
	{
		get
		{
			return this.experienceGained.Value;
		}
		set
		{
			this.experienceGained.Value = value;
		}
	}

	[XmlIgnore]
	public int Slipperiness
	{
		get
		{
			return this.slipperiness.Value;
		}
		set
		{
			this.slipperiness.Value = value;
		}
	}

	[XmlIgnore]
	public bool focusedOnFarmers
	{
		get
		{
			return this.netFocusedOnFarmers.Value;
		}
		set
		{
			this.netFocusedOnFarmers.Value = value;
		}
	}

	[XmlIgnore]
	public bool wildernessFarmMonster
	{
		get
		{
			return this.netWildernessFarmMonster.Value;
		}
		set
		{
			this.netWildernessFarmMonster.Value = value;
		}
	}

	/// <inheritdoc />
	public override bool IsMonster => true;

	/// <inheritdoc />
	[XmlIgnore]
	public override bool IsVillager => false;

	public Monster()
	{
	}

	public Monster(string name, Vector2 position)
		: this(name, position, 2)
	{
		base.Breather = false;
	}

	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(this.damageToFarmer, "damageToFarmer").AddField(this.health, "health").AddField(this.maxHealth, "maxHealth")
			.AddField(this.resilience, "resilience")
			.AddField(this.slipperiness, "slipperiness")
			.AddField(this.experienceGained, "experienceGained")
			.AddField(this.jitteriness, "jitteriness")
			.AddField(this.missChance, "missChance")
			.AddField(this.isGlider, "isGlider")
			.AddField(this.mineMonster, "mineMonster")
			.AddField(this.hasSpecialItem, "hasSpecialItem")
			.AddField(this.objectsToDrop, "objectsToDrop")
			.AddField(this.defaultAnimationInterval, "defaultAnimationInterval")
			.AddField(this.netFocusedOnFarmers, "netFocusedOnFarmers")
			.AddField(this.netWildernessFarmMonster, "netWildernessFarmMonster")
			.AddField(this.deathAnimEvent, "deathAnimEvent")
			.AddField(this.parryEvent, "parryEvent")
			.AddField(this.trajectoryEvent, "trajectoryEvent")
			.AddField(this.ignoreDamageLOS, "ignoreDamageLOS")
			.AddField(this.synchedRotation, "synchedRotation")
			.AddField(this.isHardModeMonster, "isHardModeMonster")
			.AddField(this.stunTime, "stunTime");
		base.position.Field.AxisAlignedMovement = false;
		this.parryEvent.onEvent += handleParried;
		this.deathAnimEvent.onEvent += localDeathAnimation;
		this.trajectoryEvent.onEvent += doSetTrajectory;
	}

	protected override Farmer findPlayer()
	{
		if (base.currentLocation == null)
		{
			return Game1.player;
		}
		Farmer bestFarmer = Game1.player;
		double bestPriority = double.MaxValue;
		foreach (Farmer f in base.currentLocation.farmers)
		{
			if (!f.hidden.Value)
			{
				double priority = this.findPlayerPriority(f);
				if (priority < bestPriority)
				{
					bestPriority = priority;
					bestFarmer = f;
				}
			}
		}
		return bestFarmer;
	}

	protected virtual double findPlayerPriority(Farmer f)
	{
		return (f.Position - base.Position).LengthSquared();
	}

	public virtual void onDealContactDamage(Farmer who)
	{
	}

	public virtual List<Item> getExtraDropItems()
	{
		return new List<Item>();
	}

	public override bool withinPlayerThreshold()
	{
		if (!this.focusedOnFarmers)
		{
			return this.withinPlayerThreshold(base.moveTowardPlayerThreshold.Value);
		}
		return true;
	}

	public Monster(string name, Vector2 position, int facingDir)
		: base(new AnimatedSprite("Characters\\Monsters\\" + name), position, facingDir, name)
	{
		this.parseMonsterInfo(name);
		base.Breather = false;
	}

	public virtual bool ShouldMonsterBeRemoved()
	{
		return this.Health <= 0;
	}

	public virtual void drawAboveAllLayers(SpriteBatch b)
	{
	}

	public override void draw(SpriteBatch b)
	{
		if (!this.isGlider.Value)
		{
			base.draw(b);
		}
	}

	public virtual bool isInvincible()
	{
		return this.invincibleCountdown > 0;
	}

	public void setInvincibleCountdown(int time)
	{
		this.invincibleCountdown = time;
		base.startGlowing(new Color(255, 0, 0), border: false, 0.25f);
		base.glowingTransparency = 1f;
	}

	protected int maxTimesReachedMineBottom()
	{
		int result = 0;
		foreach (Farmer farmer in Game1.getOnlineFarmers())
		{
			result = Math.Max(result, farmer.timesReachedMineBottom);
		}
		return result;
	}

	public virtual Debris ModifyMonsterLoot(Debris debris)
	{
		return debris;
	}

	public virtual int GetBaseDifficultyLevel()
	{
		return 0;
	}

	public virtual void BuffForAdditionalDifficulty(int additional_difficulty)
	{
		int target;
		if (this.DamageToFarmer != 0)
		{
			this.DamageToFarmer = (int)((float)this.DamageToFarmer * (1f + (float)additional_difficulty * 0.25f));
			target = 20 + (additional_difficulty - 1) * 20;
			if (this.DamageToFarmer < target)
			{
				this.DamageToFarmer = (int)Utility.Lerp(this.DamageToFarmer, target, 0.5f);
			}
		}
		this.MaxHealth = (int)((float)this.MaxHealth * (1f + (float)additional_difficulty * 0.5f));
		target = 500 + (additional_difficulty - 1) * 300;
		if (this.MaxHealth < target)
		{
			this.MaxHealth = (int)Utility.Lerp(this.MaxHealth, target, 0.5f);
		}
		this.Health = this.MaxHealth;
		this.resilience.Value += additional_difficulty * this.resilience.Value;
		this.isHardModeMonster.Value = true;
	}

	protected void parseMonsterInfo(string name)
	{
		string[] monsterInfo = DataLoader.Monsters(Game1.content)[name].Split('/');
		this.Health = Convert.ToInt32(monsterInfo[0]);
		this.MaxHealth = this.Health;
		this.DamageToFarmer = Convert.ToInt32(monsterInfo[1]);
		this.isGlider.Value = Convert.ToBoolean(monsterInfo[4]);
		string[] objectsSplit = ArgUtility.SplitBySpace(monsterInfo[6]);
		this.objectsToDrop.Clear();
		for (int i = 0; i < objectsSplit.Length; i += 2)
		{
			if (Game1.random.NextDouble() < Convert.ToDouble(objectsSplit[i + 1]))
			{
				this.objectsToDrop.Add(objectsSplit[i]);
			}
		}
		this.resilience.Value = Convert.ToInt32(monsterInfo[7]);
		this.jitteriness.Value = Convert.ToDouble(monsterInfo[8]);
		base.willDestroyObjectsUnderfoot = false;
		base.moveTowardPlayer(Convert.ToInt32(monsterInfo[9]));
		base.speed = Convert.ToInt32(monsterInfo[10]);
		this.missChance.Value = Convert.ToDouble(monsterInfo[11]);
		this.mineMonster.Value = Convert.ToBoolean(monsterInfo[12]);
		if (this.maxTimesReachedMineBottom() >= 1 && this.mineMonster.Value)
		{
			this.resilience.Value += this.resilience.Value / 2;
			this.missChance.Value *= 2.0;
			this.Health += Game1.random.Next(0, this.Health);
			this.DamageToFarmer += Game1.random.Next(0, this.DamageToFarmer / 2);
		}
		try
		{
			this.ExperienceGained = Convert.ToInt32(monsterInfo[13]);
		}
		catch (Exception)
		{
			this.ExperienceGained = 1;
		}
		this.displayName = monsterInfo[14];
	}

	/// <summary>Get the translated display name for a monster from the underlying data, if any.</summary>
	/// <param name="name">The monster's internal name.</param>
	public new static string GetDisplayName(string name)
	{
		if (name == null || !DataLoader.Monsters(Game1.content).TryGetValue(name, out var rawData))
		{
			return name;
		}
		return rawData.Split('/')[14];
	}

	public virtual void InitializeForLocation(GameLocation location)
	{
		if (this.initializedForLocation)
		{
			return;
		}
		if (this.mineMonster.Value && this.maxTimesReachedMineBottom() >= 1)
		{
			double additional_chance = 0.0;
			if (location is MineShaft mine)
			{
				additional_chance = (double)mine.GetAdditionalDifficulty() * 0.001;
			}
			if (Game1.random.NextDouble() < 0.001 + additional_chance)
			{
				this.objectsToDrop.Add(Game1.random.Choose("72", "74"));
			}
		}
		if (Game1.player.team.SpecialOrderRuleActive("DROP_QI_BEANS") && Game1.random.NextDouble() < ((base.name.Value == "Dust Spirit") ? 0.02 : 0.05))
		{
			this.objectsToDrop.Add("890");
		}
		if (location is MineShaft { mineLevel: >120 } mineShaft && !mineShaft.isSideBranch())
		{
			int floor = mineShaft.mineLevel - 121;
			if (Utility.GetDayOfPassiveFestival("DesertFestival") > 0)
			{
				float chance = 0.02f;
				chance += (float)(Game1.player.team.calicoEggSkullCavernRating.Value * 5 + 1 + floor) * 0.002f;
				if (chance > 0.5f)
				{
					chance = 0.5f;
				}
				if (Game1.random.NextBool(chance))
				{
					int count = Game1.random.Next(1, 4);
					for (int i = 0; i < count; i++)
					{
						this.objectsToDrop.Add("CalicoEgg");
					}
				}
			}
		}
		this.initializedForLocation = true;
	}

	/// <inheritdoc />
	public override void reloadSprite(bool onlyAppearance = false)
	{
		this.Sprite = new AnimatedSprite("Characters\\Monsters\\" + base.Name, 0, 16, 16);
	}

	/// <inheritdoc />
	public override void ChooseAppearance(LocalizedContentManager content = null)
	{
		if (this.Sprite?.Texture == null)
		{
			this.reloadSprite(onlyAppearance: true);
		}
	}

	public virtual void shedChunks(int number)
	{
		this.shedChunks(number, 0.75f);
	}

	public virtual void shedChunks(int number, float scale)
	{
		if (this.Sprite.Texture.Height > this.Sprite.getHeight() * 4)
		{
			Point standingPixel = base.StandingPixel;
			Game1.createRadialDebris(base.currentLocation, this.Sprite.textureName.Value, new Microsoft.Xna.Framework.Rectangle(0, this.Sprite.getHeight() * 4 + 16, 16, 16), 8, standingPixel.X, standingPixel.Y, number, base.TilePoint.Y, Color.White, 4f * scale);
		}
	}

	public void deathAnimation()
	{
		this.sharedDeathAnimation();
		this.deathAnimEvent.Fire();
	}

	protected virtual void sharedDeathAnimation()
	{
		this.shedChunks(Game1.random.Next(4, 9), 0.75f);
	}

	protected virtual void localDeathAnimation()
	{
	}

	public void parried(int damage, Farmer who)
	{
		this.parryEvent.Fire(new ParryEventArgs(damage, who));
	}

	private void handleParried(ParryEventArgs args)
	{
		int damage = args.damage;
		Farmer who = args.who;
		if (Game1.IsMasterGame)
		{
			float oldXVel = base.xVelocity;
			float oldYVel = base.yVelocity;
			if (base.xVelocity != 0f || base.yVelocity != 0f)
			{
				base.currentLocation.damageMonster(this.GetBoundingBox(), damage / 2, damage / 2 + 1, isBomb: false, 0f, 0, 0f, 0f, triggerMonsterInvincibleTimer: false, who);
			}
			base.xVelocity = 0f - oldXVel;
			base.yVelocity = 0f - oldYVel;
			base.xVelocity *= (this.isGlider.Value ? 2f : 3.5f);
			base.yVelocity *= (this.isGlider.Value ? 2f : 3.5f);
		}
		this.setInvincibleCountdown(450);
	}

	public virtual int takeDamage(int damage, int xTrajectory, int yTrajectory, bool isBomb, double addedPrecision, Farmer who)
	{
		return this.takeDamage(damage, xTrajectory, yTrajectory, isBomb, addedPrecision, "hitEnemy");
	}

	public int takeDamage(int damage, int xTrajectory, int yTrajectory, bool isBomb, double addedPrecision, string hitSound)
	{
		int actualDamage = Math.Max(1, damage - this.resilience.Value);
		this.slideAnimationTimer = 0;
		if (Game1.random.NextDouble() < this.missChance.Value - this.missChance.Value * addedPrecision)
		{
			actualDamage = -1;
		}
		else
		{
			this.Health -= actualDamage;
			base.currentLocation.playSound(hitSound);
			base.setTrajectory(xTrajectory / 3, yTrajectory / 3);
			if (this.Health <= 0)
			{
				this.deathAnimation();
			}
		}
		return actualDamage;
	}

	public override void setTrajectory(Vector2 trajectory)
	{
		this.trajectoryEvent.Fire(trajectory);
	}

	private void doSetTrajectory(Vector2 trajectory)
	{
		if (Game1.IsMasterGame)
		{
			if (Math.Abs(trajectory.X) > Math.Abs(base.xVelocity))
			{
				base.xVelocity = trajectory.X;
			}
			if (Math.Abs(trajectory.Y) > Math.Abs(base.yVelocity))
			{
				base.yVelocity = trajectory.Y;
			}
		}
	}

	public virtual void behaviorAtGameTick(GameTime time)
	{
		if (this.timeBeforeAIMovementAgain > 0f)
		{
			this.timeBeforeAIMovementAgain -= time.ElapsedGameTime.Milliseconds;
		}
		if (!this.Player.isRafting || !this.withinPlayerThreshold(4))
		{
			return;
		}
		base.IsWalkingTowardPlayer = false;
		Point monsterPixel = base.StandingPixel;
		Point playerPixel = this.Player.StandingPixel;
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
		this.MovePosition(time, Game1.viewport, base.currentLocation);
	}

	public override bool shouldCollideWithBuildingLayer(GameLocation location)
	{
		return true;
	}

	public override void update(GameTime time, GameLocation location)
	{
		if (Game1.IsMasterGame && !this.initializedForLocation && location != null)
		{
			this.InitializeForLocation(location);
			this.initializedForLocation = true;
		}
		this.parryEvent.Poll();
		this.trajectoryEvent.Poll();
		this.deathAnimEvent.Poll();
		base.position.UpdateExtrapolation((float)base.speed + this.addedSpeed);
		if (this.invincibleCountdown > 0)
		{
			this.invincibleCountdown -= time.ElapsedGameTime.Milliseconds;
			if (this.invincibleCountdown <= 0)
			{
				base.stopGlowing();
			}
		}
		if (!location.farmers.Any())
		{
			return;
		}
		if (!this.Player.isRafting || !this.withinPlayerThreshold(4))
		{
			base.update(time, location);
		}
		if (Game1.IsMasterGame)
		{
			if (this.stunTime.Value <= 0)
			{
				this.behaviorAtGameTick(time);
			}
			else
			{
				this.stunTime.Value -= (int)time.ElapsedGameTime.TotalMilliseconds;
				if (this.stunTime.Value < 0)
				{
					this.stunTime.Value = 0;
				}
			}
		}
		this.updateAnimation(time);
		if (Game1.IsMasterGame)
		{
			this.synchedRotation.Value = base.rotation;
		}
		else
		{
			base.rotation = this.synchedRotation.Value;
		}
		Layer backLayer = location.map.RequireLayer("Back");
		if (base.controller != null && this.withinPlayerThreshold(3))
		{
			base.controller = null;
		}
		if (!this.isGlider.Value && (base.Position.X < 0f || base.Position.X > (float)(backLayer.LayerWidth * 64) || base.Position.Y < 0f || base.Position.Y > (float)(backLayer.LayerHeight * 64)))
		{
			location.characters.Remove(this);
		}
		else if (this.isGlider.Value && base.Position.X < -2000f)
		{
			this.Health = -500;
		}
	}

	protected void resetAnimationSpeed()
	{
		if (!base.ignoreMovementAnimations)
		{
			this.Sprite.interval = (float)this.defaultAnimationInterval.Value - ((float)base.speed + this.addedSpeed - 2f) * 20f;
		}
	}

	protected virtual void updateAnimation(GameTime time)
	{
		if (!Game1.IsMasterGame)
		{
			this.updateMonsterSlaveAnimation(time);
		}
		this.resetAnimationSpeed();
	}

	protected override void updateSlaveAnimation(GameTime time)
	{
	}

	protected virtual void updateMonsterSlaveAnimation(GameTime time)
	{
		this.Sprite.animateOnce(time);
	}

	public virtual bool ShouldActuallyMoveAwayFromPlayer()
	{
		return false;
	}

	private void checkHorizontalMovement(ref bool success, ref bool setMoving, ref bool scootSuccess, Farmer who, GameLocation location)
	{
		if (who.Position.X > base.Position.X + 16f)
		{
			if (this.ShouldActuallyMoveAwayFromPlayer())
			{
				base.SetMovingOnlyLeft();
			}
			else
			{
				base.SetMovingOnlyRight();
			}
			setMoving = true;
			if (!location.isCollidingPosition(this.nextPosition(1), Game1.viewport, isFarmer: false, this.DamageToFarmer, this.isGlider.Value, this))
			{
				success = true;
			}
			else
			{
				this.MovePosition(Game1.currentGameTime, Game1.viewport, location);
				if (!base.Position.Equals(base.lastPosition))
				{
					scootSuccess = true;
				}
			}
		}
		if (success || !(who.Position.X < base.Position.X - 16f))
		{
			return;
		}
		if (this.ShouldActuallyMoveAwayFromPlayer())
		{
			base.SetMovingOnlyRight();
		}
		else
		{
			base.SetMovingOnlyLeft();
		}
		setMoving = true;
		if (!location.isCollidingPosition(this.nextPosition(3), Game1.viewport, isFarmer: false, this.DamageToFarmer, this.isGlider.Value, this))
		{
			success = true;
			return;
		}
		this.MovePosition(Game1.currentGameTime, Game1.viewport, location);
		if (!base.Position.Equals(base.lastPosition))
		{
			scootSuccess = true;
		}
	}

	private void checkVerticalMovement(ref bool success, ref bool setMoving, ref bool scootSuccess, Farmer who, GameLocation location)
	{
		if (!success && who.Position.Y < base.Position.Y - 16f)
		{
			if (this.ShouldActuallyMoveAwayFromPlayer())
			{
				base.SetMovingOnlyDown();
			}
			else
			{
				base.SetMovingOnlyUp();
			}
			setMoving = true;
			if (!location.isCollidingPosition(this.nextPosition(0), Game1.viewport, isFarmer: false, this.DamageToFarmer, this.isGlider.Value, this))
			{
				success = true;
			}
			else
			{
				this.MovePosition(Game1.currentGameTime, Game1.viewport, location);
				if (!base.Position.Equals(base.lastPosition))
				{
					scootSuccess = true;
				}
			}
		}
		if (success || !(who.Position.Y > base.Position.Y + 16f))
		{
			return;
		}
		if (this.ShouldActuallyMoveAwayFromPlayer())
		{
			base.SetMovingOnlyUp();
		}
		else
		{
			base.SetMovingOnlyDown();
		}
		setMoving = true;
		if (!location.isCollidingPosition(this.nextPosition(2), Game1.viewport, isFarmer: false, this.DamageToFarmer, this.isGlider.Value, this))
		{
			success = true;
			return;
		}
		this.MovePosition(Game1.currentGameTime, Game1.viewport, location);
		if (!base.Position.Equals(base.lastPosition))
		{
			scootSuccess = true;
		}
	}

	public override void updateMovement(GameLocation location, GameTime time)
	{
		if (base.IsWalkingTowardPlayer)
		{
			if ((base.moveTowardPlayerThreshold.Value == -1 || this.withinPlayerThreshold()) && this.timeBeforeAIMovementAgain <= 0f && this.IsMonster && !this.isGlider.Value)
			{
				Tile playerTile = location.map.RequireLayer("Back").Tiles[this.Player.TilePoint.X, this.Player.TilePoint.Y];
				if (playerTile == null || playerTile.Properties.ContainsKey("NPCBarrier"))
				{
					return;
				}
				if (this.skipHorizontal <= 0)
				{
					if (base.lastPosition.Equals(base.Position) && Game1.random.NextDouble() < 0.001)
					{
						switch (this.FacingDirection)
						{
						case 1:
						case 3:
							if (Game1.random.NextBool())
							{
								base.SetMovingOnlyUp();
							}
							else
							{
								base.SetMovingOnlyDown();
							}
							break;
						case 0:
						case 2:
							if (Game1.random.NextBool())
							{
								base.SetMovingOnlyRight();
							}
							else
							{
								base.SetMovingOnlyLeft();
							}
							break;
						}
						this.skipHorizontal = 700;
						return;
					}
					bool success = false;
					bool setMoving = false;
					bool scootSuccess = false;
					if (base.lastPosition.X == base.Position.X)
					{
						this.checkHorizontalMovement(ref success, ref setMoving, ref scootSuccess, this.Player, location);
						this.checkVerticalMovement(ref success, ref setMoving, ref scootSuccess, this.Player, location);
					}
					else
					{
						this.checkVerticalMovement(ref success, ref setMoving, ref scootSuccess, this.Player, location);
						this.checkHorizontalMovement(ref success, ref setMoving, ref scootSuccess, this.Player, location);
					}
					if (success)
					{
						this.skipHorizontal = 500;
					}
					else if (!setMoving)
					{
						this.Halt();
						base.faceGeneralDirection(this.Player.getStandingPosition());
					}
					if (scootSuccess)
					{
						return;
					}
				}
				else
				{
					this.skipHorizontal -= time.ElapsedGameTime.Milliseconds;
				}
			}
		}
		else
		{
			this.defaultMovementBehavior(time);
		}
		this.MovePosition(time, Game1.viewport, location);
		if (base.Position.Equals(base.lastPosition) && base.IsWalkingTowardPlayer && this.withinPlayerThreshold())
		{
			this.noMovementProgressNearPlayerBehavior();
		}
	}

	public virtual void noMovementProgressNearPlayerBehavior()
	{
		this.Halt();
		base.faceGeneralDirection(this.Player.getStandingPosition());
	}

	public virtual void defaultMovementBehavior(GameTime time)
	{
		if (Game1.random.NextDouble() < this.jitteriness.Value * 1.8 && this.skipHorizontal <= 0)
		{
			switch (Game1.random.Next(6))
			{
			case 0:
				base.SetMovingOnlyUp();
				break;
			case 1:
				base.SetMovingOnlyRight();
				break;
			case 2:
				base.SetMovingOnlyDown();
				break;
			case 3:
				base.SetMovingOnlyLeft();
				break;
			default:
				this.Halt();
				break;
			}
		}
	}

	public virtual bool TakesDamageFromHitbox(Microsoft.Xna.Framework.Rectangle area_of_effect)
	{
		return this.GetBoundingBox().Intersects(area_of_effect);
	}

	public virtual bool OverlapsFarmerForDamage(Farmer who)
	{
		return this.GetBoundingBox().Intersects(who.GetBoundingBox());
	}

	public override void Halt()
	{
		int old_speed = base.speed;
		base.Halt();
		base.speed = old_speed;
	}

	public override void MovePosition(GameTime time, xTile.Dimensions.Rectangle viewport, GameLocation currentLocation)
	{
		if (this.stunTime.Value > 0)
		{
			return;
		}
		base.lastPosition = base.Position;
		if (base.xVelocity != 0f || base.yVelocity != 0f)
		{
			if (double.IsNaN(base.xVelocity) || double.IsNaN(base.yVelocity))
			{
				base.xVelocity = 0f;
				base.yVelocity = 0f;
			}
			Microsoft.Xna.Framework.Rectangle nextPosition = this.GetBoundingBox();
			int start_x = nextPosition.X;
			int start_y = nextPosition.Y;
			int end_x = nextPosition.X + (int)base.xVelocity;
			int end_y = nextPosition.Y - (int)base.yVelocity;
			int steps = 1;
			bool found_collision = false;
			bool isGroundedGlider = this is SquidKid;
			if (!this.isGlider.Value || isGroundedGlider)
			{
				if (nextPosition.Width > 0 && Math.Abs((int)base.xVelocity) > nextPosition.Width)
				{
					steps = (int)Math.Max(steps, Math.Ceiling((float)Math.Abs((int)base.xVelocity) / (float)nextPosition.Width));
				}
				if (nextPosition.Height > 0 && Math.Abs((int)base.yVelocity) > nextPosition.Height)
				{
					steps = (int)Math.Max(steps, Math.Ceiling((float)Math.Abs((int)base.yVelocity) / (float)nextPosition.Height));
				}
			}
			for (int i = 1; i <= steps; i++)
			{
				nextPosition.X = (int)Utility.Lerp(start_x, end_x, (float)i / (float)steps);
				nextPosition.Y = (int)Utility.Lerp(start_y, end_y, (float)i / (float)steps);
				bool isGliderForCollisions = this.isGlider.Value && !isGroundedGlider;
				if (currentLocation != null && currentLocation.isCollidingPosition(nextPosition, viewport, isFarmer: false, this.DamageToFarmer, isGliderForCollisions, this))
				{
					found_collision = true;
					break;
				}
			}
			if (!found_collision)
			{
				base.position.X += base.xVelocity;
				base.position.Y -= base.yVelocity;
				if (this.Slipperiness < 1000)
				{
					base.xVelocity -= base.xVelocity / (float)this.Slipperiness;
					base.yVelocity -= base.yVelocity / (float)this.Slipperiness;
					if (Math.Abs(base.xVelocity) <= 0.05f)
					{
						base.xVelocity = 0f;
					}
					if (Math.Abs(base.yVelocity) <= 0.05f)
					{
						base.yVelocity = 0f;
					}
				}
				if (!this.isGlider.Value && this.invincibleCountdown > 0)
				{
					this.slideAnimationTimer -= time.ElapsedGameTime.Milliseconds;
					if (this.slideAnimationTimer < 0 && (Math.Abs(base.xVelocity) >= 3f || Math.Abs(base.yVelocity) >= 3f))
					{
						this.slideAnimationTimer = 100 - (int)(Math.Abs(base.xVelocity) * 2f + Math.Abs(base.yVelocity) * 2f);
						Game1.multiplayer.broadcastSprites(currentLocation, new TemporaryAnimatedSprite(6, base.getStandingPosition() + new Vector2(-32f, -32f), Color.White * 0.75f, 8, Game1.random.NextBool(), 20f)
						{
							scale = 0.75f
						});
					}
				}
			}
			else if (this.isGlider.Value || this.Slipperiness >= 8)
			{
				if (this.isGlider.Value)
				{
					bool[] array = Utility.horizontalOrVerticalCollisionDirections(nextPosition, this);
					if (array[0])
					{
						base.xVelocity = 0f - base.xVelocity;
						base.position.X += Math.Sign(base.xVelocity);
						base.rotation += (float)(Math.PI + (double)Game1.random.Next(-10, 11) * Math.PI / 500.0);
					}
					if (array[1])
					{
						base.yVelocity = 0f - base.yVelocity;
						base.position.Y -= Math.Sign(base.yVelocity);
						base.rotation += (float)(Math.PI + (double)Game1.random.Next(-10, 11) * Math.PI / 500.0);
					}
				}
				if (this.Slipperiness < 1000)
				{
					base.xVelocity -= base.xVelocity / (float)this.Slipperiness / 4f;
					base.yVelocity -= base.yVelocity / (float)this.Slipperiness / 4f;
					if (Math.Abs(base.xVelocity) <= 0.05f)
					{
						base.xVelocity = 0f;
					}
					if (Math.Abs(base.yVelocity) <= 0.051f)
					{
						base.yVelocity = 0f;
					}
				}
			}
			else
			{
				base.xVelocity -= base.xVelocity / (float)this.Slipperiness;
				base.yVelocity -= base.yVelocity / (float)this.Slipperiness;
				if (Math.Abs(base.xVelocity) <= 0.05f)
				{
					base.xVelocity = 0f;
				}
				if (Math.Abs(base.yVelocity) <= 0.05f)
				{
					base.yVelocity = 0f;
				}
			}
			if (this.isGlider.Value)
			{
				return;
			}
		}
		if (base.moveUp)
		{
			if (((!Game1.eventUp || Game1.IsMultiplayer) && !currentLocation.isCollidingPosition(this.nextPosition(0), viewport, isFarmer: false, this.DamageToFarmer, this.isGlider.Value, this)) || base.isCharging)
			{
				base.position.Y -= (float)base.speed + this.addedSpeed;
				if (!base.ignoreMovementAnimations)
				{
					this.Sprite.AnimateUp(time);
				}
				this.FacingDirection = 0;
				this.faceDirection(0);
			}
			else
			{
				Microsoft.Xna.Framework.Rectangle tmp = this.nextPosition(0);
				tmp.Width /= 4;
				bool leftCorner = currentLocation.isCollidingPosition(tmp, viewport, isFarmer: false, this.DamageToFarmer, this.isGlider.Value, this);
				tmp.X += tmp.Width * 3;
				bool rightCorner = currentLocation.isCollidingPosition(tmp, viewport, isFarmer: false, this.DamageToFarmer, this.isGlider.Value, this);
				if (leftCorner && !rightCorner && !currentLocation.isCollidingPosition(this.nextPosition(1), viewport, isFarmer: false, this.DamageToFarmer, this.isGlider.Value, this))
				{
					base.position.X += (float)base.speed * ((float)time.ElapsedGameTime.Milliseconds / 64f);
				}
				else if (rightCorner && !leftCorner && !currentLocation.isCollidingPosition(this.nextPosition(3), viewport, isFarmer: false, this.DamageToFarmer, this.isGlider.Value, this))
				{
					base.position.X -= (float)base.speed * ((float)time.ElapsedGameTime.Milliseconds / 64f);
				}
				if (!currentLocation.isTilePassable(this.nextPosition(0), viewport) || !base.willDestroyObjectsUnderfoot)
				{
					this.Halt();
				}
				else if (base.willDestroyObjectsUnderfoot)
				{
					if (currentLocation.characterDestroyObjectWithinRectangle(this.nextPosition(0), showDestroyedObject: true))
					{
						currentLocation.playSound("stoneCrack");
						base.position.Y -= (float)base.speed + this.addedSpeed;
					}
					else
					{
						base.blockedInterval += time.ElapsedGameTime.Milliseconds;
					}
				}
				this.onCollision?.Invoke(currentLocation);
			}
		}
		else if (base.moveRight)
		{
			if (((!Game1.eventUp || Game1.IsMultiplayer) && !currentLocation.isCollidingPosition(this.nextPosition(1), viewport, isFarmer: false, this.DamageToFarmer, this.isGlider.Value, this)) || base.isCharging)
			{
				base.position.X += (float)base.speed + this.addedSpeed;
				if (!base.ignoreMovementAnimations)
				{
					this.Sprite.AnimateRight(time);
				}
				this.FacingDirection = 1;
				this.faceDirection(1);
			}
			else
			{
				Microsoft.Xna.Framework.Rectangle tmp2 = this.nextPosition(1);
				tmp2.Height /= 4;
				bool topCorner = currentLocation.isCollidingPosition(tmp2, viewport, isFarmer: false, this.DamageToFarmer, this.isGlider.Value, this);
				tmp2.Y += tmp2.Height * 3;
				bool bottomCorner = currentLocation.isCollidingPosition(tmp2, viewport, isFarmer: false, this.DamageToFarmer, this.isGlider.Value, this);
				if (topCorner && !bottomCorner && !currentLocation.isCollidingPosition(this.nextPosition(2), viewport, isFarmer: false, this.DamageToFarmer, this.isGlider.Value, this))
				{
					base.position.Y += (float)base.speed * ((float)time.ElapsedGameTime.Milliseconds / 64f);
				}
				else if (bottomCorner && !topCorner && !currentLocation.isCollidingPosition(this.nextPosition(0), viewport, isFarmer: false, this.DamageToFarmer, this.isGlider.Value, this))
				{
					base.position.Y -= (float)base.speed * ((float)time.ElapsedGameTime.Milliseconds / 64f);
				}
				if (!currentLocation.isTilePassable(this.nextPosition(1), viewport) || !base.willDestroyObjectsUnderfoot)
				{
					this.Halt();
				}
				else if (base.willDestroyObjectsUnderfoot)
				{
					if (currentLocation.characterDestroyObjectWithinRectangle(this.nextPosition(1), showDestroyedObject: true))
					{
						currentLocation.playSound("stoneCrack");
						base.position.X += (float)base.speed + this.addedSpeed;
					}
					else
					{
						base.blockedInterval += time.ElapsedGameTime.Milliseconds;
					}
				}
				this.onCollision?.Invoke(currentLocation);
			}
		}
		else if (base.moveDown)
		{
			if (((!Game1.eventUp || Game1.IsMultiplayer) && !currentLocation.isCollidingPosition(this.nextPosition(2), viewport, isFarmer: false, this.DamageToFarmer, this.isGlider.Value, this)) || base.isCharging)
			{
				base.position.Y += (float)base.speed + this.addedSpeed;
				if (!base.ignoreMovementAnimations)
				{
					this.Sprite.AnimateDown(time);
				}
				this.FacingDirection = 2;
				this.faceDirection(2);
			}
			else
			{
				Microsoft.Xna.Framework.Rectangle tmp3 = this.nextPosition(2);
				tmp3.Width /= 4;
				bool leftCorner2 = currentLocation.isCollidingPosition(tmp3, viewport, isFarmer: false, this.DamageToFarmer, this.isGlider.Value, this);
				tmp3.X += tmp3.Width * 3;
				bool rightCorner2 = currentLocation.isCollidingPosition(tmp3, viewport, isFarmer: false, this.DamageToFarmer, this.isGlider.Value, this);
				if (leftCorner2 && !rightCorner2 && !currentLocation.isCollidingPosition(this.nextPosition(1), viewport, isFarmer: false, this.DamageToFarmer, this.isGlider.Value, this))
				{
					base.position.X += (float)base.speed * ((float)time.ElapsedGameTime.Milliseconds / 64f);
				}
				else if (rightCorner2 && !leftCorner2 && !currentLocation.isCollidingPosition(this.nextPosition(3), viewport, isFarmer: false, this.DamageToFarmer, this.isGlider.Value, this))
				{
					base.position.X -= (float)base.speed * ((float)time.ElapsedGameTime.Milliseconds / 64f);
				}
				if (!currentLocation.isTilePassable(this.nextPosition(2), viewport) || !base.willDestroyObjectsUnderfoot)
				{
					this.Halt();
				}
				else if (base.willDestroyObjectsUnderfoot)
				{
					if (currentLocation.characterDestroyObjectWithinRectangle(this.nextPosition(2), showDestroyedObject: true))
					{
						currentLocation.playSound("stoneCrack");
						base.position.Y += (float)base.speed + this.addedSpeed;
					}
					else
					{
						base.blockedInterval += time.ElapsedGameTime.Milliseconds;
					}
				}
				this.onCollision?.Invoke(currentLocation);
			}
		}
		else if (base.moveLeft)
		{
			if (((!Game1.eventUp || Game1.IsMultiplayer) && !currentLocation.isCollidingPosition(this.nextPosition(3), viewport, isFarmer: false, this.DamageToFarmer, this.isGlider.Value, this)) || base.isCharging)
			{
				base.position.X -= (float)base.speed + this.addedSpeed;
				this.FacingDirection = 3;
				if (!base.ignoreMovementAnimations)
				{
					this.Sprite.AnimateLeft(time);
				}
				this.faceDirection(3);
			}
			else
			{
				Microsoft.Xna.Framework.Rectangle tmp4 = this.nextPosition(3);
				tmp4.Height /= 4;
				bool topCorner2 = currentLocation.isCollidingPosition(tmp4, viewport, isFarmer: false, this.DamageToFarmer, this.isGlider.Value, this);
				tmp4.Y += tmp4.Height * 3;
				bool bottomCorner2 = currentLocation.isCollidingPosition(tmp4, viewport, isFarmer: false, this.DamageToFarmer, this.isGlider.Value, this);
				if (topCorner2 && !bottomCorner2 && !currentLocation.isCollidingPosition(this.nextPosition(2), viewport, isFarmer: false, this.DamageToFarmer, this.isGlider.Value, this))
				{
					base.position.Y += (float)base.speed * ((float)time.ElapsedGameTime.Milliseconds / 64f);
				}
				else if (bottomCorner2 && !topCorner2 && !currentLocation.isCollidingPosition(this.nextPosition(0), viewport, isFarmer: false, this.DamageToFarmer, this.isGlider.Value, this))
				{
					base.position.Y -= (float)base.speed * ((float)time.ElapsedGameTime.Milliseconds / 64f);
				}
				if (!currentLocation.isTilePassable(this.nextPosition(3), viewport) || !base.willDestroyObjectsUnderfoot)
				{
					this.Halt();
				}
				else if (base.willDestroyObjectsUnderfoot)
				{
					if (currentLocation.characterDestroyObjectWithinRectangle(this.nextPosition(3), showDestroyedObject: true))
					{
						currentLocation.playSound("stoneCrack");
						base.position.X -= (float)base.speed + this.addedSpeed;
					}
					else
					{
						base.blockedInterval += time.ElapsedGameTime.Milliseconds;
					}
				}
				this.onCollision?.Invoke(currentLocation);
			}
		}
		else if (!base.ignoreMovementAnimations)
		{
			if (base.moveUp)
			{
				this.Sprite.AnimateUp(time);
			}
			else if (base.moveRight)
			{
				this.Sprite.AnimateRight(time);
			}
			else if (base.moveDown)
			{
				this.Sprite.AnimateDown(time);
			}
			else if (base.moveLeft)
			{
				this.Sprite.AnimateLeft(time);
			}
		}
		if (base.blockedInterval >= 5000)
		{
			base.speed = 4;
			base.isCharging = true;
			base.blockedInterval = 0;
		}
		if (this.DamageToFarmer <= 0 || !(Game1.random.NextDouble() < 0.0003333333333333333))
		{
			return;
		}
		string text = base.Name;
		if (!(text == "Shadow Guy"))
		{
			if (text == "Ghost")
			{
				currentLocation.playSound("ghost");
			}
		}
		else if (Game1.random.NextDouble() < 0.3)
		{
			if (Game1.random.NextBool())
			{
				currentLocation.playSound("grunt");
			}
			else
			{
				currentLocation.playSound("shadowpeep");
			}
		}
	}

	/// <summary>Auto-generate a default light source ID for this monster.</summary>
	/// <param name="identifier">A unique ID for this monster instance.</param>
	protected virtual string GenerateLightSourceId(int identifier)
	{
		return $"{base.GetType().Name}_{identifier}";
	}
}
