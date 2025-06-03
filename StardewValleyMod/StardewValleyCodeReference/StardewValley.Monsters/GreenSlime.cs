using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Extensions;
using StardewValley.Projectiles;
using StardewValley.SpecialOrders;

namespace StardewValley.Monsters;

public class GreenSlime : Monster
{
	public const float mutationFactor = 0.25f;

	public const int matingInterval = 120000;

	public const int childhoodLength = 120000;

	public const int durationOfMating = 2000;

	public const double chanceToMate = 0.001;

	public static int matingRange = 192;

	public const int AQUA_SLIME = 9999899;

	public NetIntDelta stackedSlimes = new NetIntDelta(0)
	{
		Minimum = 0
	};

	public float randomStackOffset;

	[XmlIgnore]
	public NetEvent1Field<Vector2, NetVector2> attackedEvent = new NetEvent1Field<Vector2, NetVector2>();

	[XmlElement("leftDrift")]
	public readonly NetBool leftDrift = new NetBool();

	[XmlElement("cute")]
	public readonly NetBool cute = new NetBool(value: true);

	[XmlIgnore]
	public int readyToJump = -1;

	[XmlIgnore]
	public int matingCountdown;

	[XmlIgnore]
	public new int yOffset;

	[XmlIgnore]
	public int wagTimer;

	public int readyToMate = 120000;

	[XmlElement("ageUntilFullGrown")]
	public readonly NetInt ageUntilFullGrown = new NetInt();

	public int animateTimer;

	public int timeSinceLastJump;

	[XmlElement("specialNumber")]
	public readonly NetInt specialNumber = new NetInt();

	[XmlElement("firstGeneration")]
	public readonly NetBool firstGeneration = new NetBool();

	[XmlElement("color")]
	public readonly NetColor color = new NetColor();

	private readonly NetBool pursuingMate = new NetBool();

	private readonly NetBool avoidingMate = new NetBool();

	private GreenSlime mate;

	public readonly NetBool prismatic = new NetBool();

	private readonly NetVector2 facePosition = new NetVector2();

	private readonly NetEvent1Field<Vector2, NetVector2> jumpEvent = new NetEvent1Field<Vector2, NetVector2>
	{
		InterpolationWait = false
	};

	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(this.leftDrift, "leftDrift").AddField(this.cute, "cute").AddField(this.ageUntilFullGrown, "ageUntilFullGrown")
			.AddField(this.specialNumber, "specialNumber")
			.AddField(this.firstGeneration, "firstGeneration")
			.AddField(this.color, "color")
			.AddField(this.pursuingMate, "pursuingMate")
			.AddField(this.avoidingMate, "avoidingMate")
			.AddField(this.facePosition, "facePosition")
			.AddField(this.jumpEvent, "jumpEvent")
			.AddField(this.prismatic, "prismatic")
			.AddField(this.stackedSlimes, "stackedSlimes")
			.AddField(this.attackedEvent.NetFields, "attackedEvent.NetFields");
		this.attackedEvent.onEvent += OnAttacked;
		this.jumpEvent.onEvent += doJump;
	}

	public GreenSlime()
	{
	}

	public GreenSlime(Vector2 position)
		: base("Green Slime", position)
	{
		if (Game1.random.NextBool())
		{
			this.leftDrift.Value = true;
		}
		base.Slipperiness = 4;
		this.readyToMate = Game1.random.Next(1000, 120000);
		int green = Game1.random.Next(200, 256);
		this.color.Value = new Color(green / Game1.random.Next(2, 10), Game1.random.Next(180, 256), (Game1.random.NextDouble() < 0.1) ? 255 : (255 - green));
		this.firstGeneration.Value = true;
		base.flip = Game1.random.NextBool();
		this.cute.Value = Game1.random.NextDouble() < 0.49;
		base.HideShadow = true;
	}

	public GreenSlime(Vector2 position, int mineLevel)
		: base("Green Slime", position)
	{
		this.randomStackOffset = Utility.RandomFloat(0f, 100f);
		this.cute.Value = Game1.random.NextDouble() < 0.49;
		base.flip = Game1.random.NextBool();
		this.specialNumber.Value = Game1.random.Next(100);
		if (mineLevel < 40)
		{
			base.parseMonsterInfo("Green Slime");
			int green = Game1.random.Next(200, 256);
			this.color.Value = new Color(green / Game1.random.Next(2, 10), green, (Game1.random.NextDouble() < 0.01) ? 255 : (255 - green));
			if (Game1.random.NextDouble() < 0.01 && mineLevel % 5 != 0 && mineLevel % 5 != 1)
			{
				this.color.Value = new Color(205, 255, 0) * 0.7f;
				base.hasSpecialItem.Value = true;
				base.Health *= 3;
				base.DamageToFarmer *= 2;
			}
			if (Game1.random.NextDouble() < 0.01 && Game1.MasterPlayer.mailReceived.Contains("slimeHutchBuilt"))
			{
				base.objectsToDrop.Add("680");
			}
		}
		else if (mineLevel < 80)
		{
			base.Name = "Frost Jelly";
			base.parseMonsterInfo("Frost Jelly");
			int blue = Game1.random.Next(200, 256);
			this.color.Value = new Color((Game1.random.NextDouble() < 0.01) ? 180 : (blue / Game1.random.Next(2, 10)), (Game1.random.NextDouble() < 0.1) ? 255 : (255 - blue / 3), blue);
			if (Game1.random.NextDouble() < 0.01 && mineLevel % 5 != 0 && mineLevel % 5 != 1)
			{
				this.color.Value = new Color(0, 0, 0) * 0.7f;
				base.hasSpecialItem.Value = true;
				base.Health *= 3;
				base.DamageToFarmer *= 2;
			}
			if (Game1.random.NextDouble() < 0.01 && Game1.MasterPlayer.mailReceived.Contains("slimeHutchBuilt"))
			{
				base.objectsToDrop.Add("413");
			}
		}
		else if (mineLevel >= 77377 && mineLevel < 77387)
		{
			base.Name = "Sludge";
			base.parseMonsterInfo("Sludge");
		}
		else if (mineLevel > 120)
		{
			base.Name = "Sludge";
			base.parseMonsterInfo("Sludge");
			this.color.Value = Color.BlueViolet;
			base.Health *= 2;
			int r = this.color.R;
			int g = this.color.G;
			int b = this.color.B;
			r += Game1.random.Next(-20, 21);
			g += Game1.random.Next(-20, 21);
			b += Game1.random.Next(-20, 21);
			this.color.R = (byte)Math.Max(Math.Min(255, r), 0);
			this.color.G = (byte)Math.Max(Math.Min(255, g), 0);
			this.color.B = (byte)Math.Max(Math.Min(255, b), 0);
			while (Game1.random.NextDouble() < 0.08)
			{
				base.objectsToDrop.Add("386");
			}
			if (Game1.random.NextDouble() < 0.009)
			{
				base.objectsToDrop.Add("337");
			}
			if (Game1.random.NextDouble() < 0.01 && Game1.MasterPlayer.mailReceived.Contains("slimeHutchBuilt"))
			{
				base.objectsToDrop.Add("439");
			}
		}
		else
		{
			base.Name = "Sludge";
			base.parseMonsterInfo("Sludge");
			int green2 = Game1.random.Next(200, 256);
			this.color.Value = new Color(green2, (Game1.random.NextDouble() < 0.01) ? 255 : (255 - green2), green2 / Game1.random.Next(2, 10));
			if (Game1.random.NextDouble() < 0.01 && mineLevel % 5 != 0 && mineLevel % 5 != 1)
			{
				this.color.Value = new Color(50, 10, 50) * 0.7f;
				base.hasSpecialItem.Value = true;
				base.Health *= 3;
				base.DamageToFarmer *= 2;
			}
			if (Game1.random.NextDouble() < 0.01 && Game1.MasterPlayer.mailReceived.Contains("slimeHutchBuilt"))
			{
				base.objectsToDrop.Add("437");
			}
		}
		if (this.cute.Value)
		{
			base.Health += base.Health / 4;
			base.DamageToFarmer++;
		}
		if (Game1.random.NextBool())
		{
			this.leftDrift.Value = true;
		}
		base.Slipperiness = 3;
		this.readyToMate = Game1.random.Next(1000, 120000);
		if (Game1.random.NextDouble() < 0.001)
		{
			this.color.Value = new Color(255, 255, 50);
			base.objectsToDrop.Add("GoldCoin");
			double extraChance = (double)(int)(Game1.stats.DaysPlayed / 28) * 0.08;
			extraChance = Math.Min(extraChance, 0.55);
			while (Game1.random.NextDouble() < 0.1 + extraChance)
			{
				base.objectsToDrop.Add("GoldCoin");
			}
		}
		if (mineLevel == 9999899)
		{
			this.color.Value = new Color(0, 255, 200);
			base.Health *= 2;
			base.objectsToDrop.Clear();
			if (Game1.random.NextDouble() < 0.02)
			{
				base.objectsToDrop.Add("394");
			}
			if (Game1.random.NextDouble() < 0.02)
			{
				base.objectsToDrop.Add("60");
			}
			if (Game1.random.NextDouble() < 0.02)
			{
				base.objectsToDrop.Add("62");
			}
			if (Game1.random.NextDouble() < 0.01)
			{
				base.objectsToDrop.Add("797");
			}
			if (Game1.random.NextDouble() < 0.03 && Game1.MasterPlayer.mailReceived.Contains("slimeHutchBuilt"))
			{
				base.objectsToDrop.Add("413");
			}
			while (Game1.random.NextBool())
			{
				base.objectsToDrop.Add("766");
			}
		}
		this.firstGeneration.Value = true;
		base.HideShadow = true;
	}

	public GreenSlime(Vector2 position, Color color)
		: base("Green Slime", position)
	{
		this.color.Value = color;
		this.firstGeneration.Value = true;
		base.HideShadow = true;
	}

	public void makeTigerSlime(bool onlyAppearance = false)
	{
		string oldName = base.Name;
		try
		{
			base.Name = "Tiger Slime";
			base.reloadSprite();
		}
		finally
		{
			if (onlyAppearance)
			{
				base.Name = oldName;
			}
		}
		this.Sprite.SpriteHeight = 24;
		this.Sprite.UpdateSourceRect();
		this.color.Value = Color.White;
		if (!onlyAppearance)
		{
			base.parseMonsterInfo("Tiger Slime");
		}
	}

	public void makePrismatic()
	{
		this.prismatic.Value = true;
		base.Name = "Prismatic Slime";
		base.Health = 1000;
		base.damageToFarmer.Value = 35;
		base.hasSpecialItem.Value = false;
	}

	/// <inheritdoc />
	public override void reloadSprite(bool onlyAppearance = false)
	{
		if (base.Name == "Tiger Slime")
		{
			this.makeTigerSlime(onlyAppearance);
			return;
		}
		string oldName = base.name.Value;
		try
		{
			base.Name = "Green Slime";
			base.reloadSprite(onlyAppearance);
		}
		finally
		{
			base.Name = oldName;
		}
		this.Sprite.SpriteHeight = 24;
		this.Sprite.UpdateSourceRect();
		base.HideShadow = true;
	}

	public virtual void OnAttacked(Vector2 trajectory)
	{
		if (Game1.IsMasterGame && this.stackedSlimes.Value > 0)
		{
			this.stackedSlimes.Value--;
			if (trajectory.LengthSquared() == 0f)
			{
				trajectory = new Vector2(0f, -1f);
			}
			else
			{
				trajectory.Normalize();
			}
			trajectory *= 16f;
			BasicProjectile projectile = new BasicProjectile(base.DamageToFarmer / 3 * 2, 13, 3, 0, (float)Math.PI / 16f, trajectory.X, trajectory.Y, base.Position, null, null, null, explode: true, damagesMonsters: false, base.currentLocation, this);
			projectile.height.Value = 24f;
			projectile.color.Value = this.color.Value;
			projectile.ignoreMeleeAttacks.Value = true;
			projectile.hostTimeUntilAttackable = 0.1f;
			if (Game1.random.NextBool())
			{
				projectile.debuff.Value = "13";
			}
			base.currentLocation.projectiles.Add(projectile);
		}
	}

	public override int takeDamage(int damage, int xTrajectory, int yTrajectory, bool isBomb, double addedPrecision, Farmer who)
	{
		if (this.stackedSlimes.Value > 0)
		{
			this.attackedEvent.Fire(new Vector2(xTrajectory, -yTrajectory));
			xTrajectory = 0;
			yTrajectory = 0;
			damage = 1;
		}
		int actualDamage = Math.Max(1, damage - base.resilience.Value);
		if (Game1.random.NextDouble() < base.missChance.Value - base.missChance.Value * addedPrecision)
		{
			actualDamage = -1;
		}
		else
		{
			if (Game1.random.NextDouble() < 0.025 && this.cute.Value)
			{
				if (!base.focusedOnFarmers)
				{
					base.DamageToFarmer += base.DamageToFarmer / 2;
					base.shake(1000);
				}
				base.focusedOnFarmers = true;
			}
			base.Slipperiness = 3;
			base.Health -= actualDamage;
			base.setTrajectory(xTrajectory, yTrajectory);
			base.currentLocation.playSound("slimeHit");
			this.readyToJump = -1;
			base.IsWalkingTowardPlayer = true;
			if (base.Health <= 0)
			{
				base.currentLocation.playSound("slimedead");
				Game1.stats.SlimesKilled++;
				if (this.mate != null)
				{
					this.mate.mate = null;
				}
				if (Game1.gameMode == 3 && base.scale.Value > 1.8f)
				{
					base.Health = 10;
					int toCreate = ((!(base.scale.Value > 1.8f)) ? 1 : Game1.random.Next(3, 5));
					base.Scale *= 2f / 3f;
					Rectangle bounds = this.GetBoundingBox();
					for (int i = 0; i < toCreate; i++)
					{
						GreenSlime slime = new GreenSlime(base.Position + new Vector2(i * bounds.Width, 0f), Game1.CurrentMineLevel);
						slime.setTrajectory(xTrajectory + Game1.random.Next(-20, 20), yTrajectory + Game1.random.Next(-20, 20));
						slime.willDestroyObjectsUnderfoot = false;
						slime.moveTowardPlayer(4);
						slime.Scale = 0.75f + (float)Game1.random.Next(-5, 10) / 100f;
						base.currentLocation.characters.Add(slime);
					}
				}
				else
				{
					Game1.multiplayer.broadcastSprites(base.currentLocation, new TemporaryAnimatedSprite(44, base.Position, this.color.Value * 0.66f, 10)
					{
						interval = 70f,
						holdLastFrame = true,
						alphaFade = 0.01f
					});
					Game1.multiplayer.broadcastSprites(base.currentLocation, new TemporaryAnimatedSprite(44, base.Position + new Vector2(-16f, 0f), this.color.Value * 0.66f, 10)
					{
						interval = 70f,
						delayBeforeAnimationStart = 0,
						holdLastFrame = true,
						alphaFade = 0.01f
					});
					Game1.multiplayer.broadcastSprites(base.currentLocation, new TemporaryAnimatedSprite(44, base.Position + new Vector2(0f, 16f), this.color.Value * 0.66f, 10)
					{
						interval = 70f,
						delayBeforeAnimationStart = 100,
						holdLastFrame = true,
						alphaFade = 0.01f
					});
					Game1.multiplayer.broadcastSprites(base.currentLocation, new TemporaryAnimatedSprite(44, base.Position + new Vector2(16f, 0f), this.color.Value * 0.66f, 10)
					{
						interval = 70f,
						delayBeforeAnimationStart = 200,
						holdLastFrame = true,
						alphaFade = 0.01f
					});
				}
			}
		}
		return actualDamage;
	}

	public override void shedChunks(int number, float scale)
	{
		Point standingPixel = base.StandingPixel;
		Game1.createRadialDebris(base.currentLocation, this.Sprite.textureName.Value, new Rectangle(0, 120, 16, 16), 8, standingPixel.X + 32, standingPixel.Y, number, base.TilePoint.Y, this.color.Value, 4f * scale);
	}

	public override void collisionWithFarmerBehavior()
	{
		base.farmerPassesThrough = base.Player.isWearingRing("520");
	}

	public override void onDealContactDamage(Farmer who)
	{
		if (Game1.random.NextDouble() < 0.3 && base.Player == Game1.player && !base.Player.temporarilyInvincible && !base.Player.isWearingRing("520") && Game1.random.Next(11) >= who.Immunity && !base.Player.hasBuff("28") && !base.Player.hasTrinketWithID("BasiliskPaw"))
		{
			base.Player.applyBuff("13");
			base.currentLocation.playSound("slime");
		}
		base.onDealContactDamage(who);
	}

	public override void draw(SpriteBatch b)
	{
		if (base.IsInvisible || !Utility.isOnScreen(base.Position, 128))
		{
			return;
		}
		int boundsHeight = this.GetBoundingBox().Height;
		int standingY = base.StandingPixel.Y;
		for (int i = 0; i <= this.stackedSlimes.Value; i++)
		{
			bool top_slime = i == this.stackedSlimes.Value;
			Vector2 stack_adjustment = Vector2.Zero;
			if (this.stackedSlimes.Value > 0)
			{
				stack_adjustment = new Vector2((float)Math.Sin((double)this.randomStackOffset + Game1.currentGameTime.TotalGameTime.TotalSeconds * Math.PI * 2.0 + (double)(i * 30)) * 8f, -30 * i);
			}
			b.Draw(this.Sprite.Texture, base.getLocalPosition(Game1.viewport) + new Vector2(32f, boundsHeight / 2 + this.yOffset) + stack_adjustment, this.Sprite.SourceRect, this.prismatic.Value ? Utility.GetPrismaticColor(348 + this.specialNumber.Value, 5f) : this.color.Value, 0f, new Vector2(8f, 16f), 4f * Math.Max(0.2f, base.scale.Value - 0.4f * ((float)this.ageUntilFullGrown.Value / 120000f)), SpriteEffects.None, Math.Max(0f, base.drawOnTop ? 0.991f : ((float)(standingY + i * 2) / 10000f)));
			b.Draw(Game1.shadowTexture, base.getLocalPosition(Game1.viewport) + new Vector2(32f, (float)(boundsHeight / 2 * 7) / 4f + (float)this.yOffset + 8f * base.scale.Value - (float)((this.ageUntilFullGrown.Value > 0) ? 8 : 0)) + stack_adjustment, Game1.shadowTexture.Bounds, Color.White, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 3f + base.scale.Value - (float)this.ageUntilFullGrown.Value / 120000f - ((this.Sprite.currentFrame % 4 % 3 != 0 || i != 0) ? 1f : 0f) + (float)this.yOffset / 30f, SpriteEffects.None, (float)(standingY - 1 + i * 2) / 10000f);
			if (this.ageUntilFullGrown.Value <= 0)
			{
				if (top_slime && (this.cute.Value || base.hasSpecialItem.Value))
				{
					int xDongleSource = ((this.isMoving() || this.wagTimer > 0) ? (16 * Math.Min(7, Math.Abs(((this.wagTimer > 0) ? (992 - this.wagTimer) : (Game1.currentGameTime.TotalGameTime.Milliseconds % 992)) - 496) / 62) % 64) : 48);
					int yDongleSource = ((this.isMoving() || this.wagTimer > 0) ? (24 * Math.Min(1, Math.Max(1, Math.Abs(((this.wagTimer > 0) ? (992 - this.wagTimer) : (Game1.currentGameTime.TotalGameTime.Milliseconds % 992)) - 496) / 62) / 4)) : 24);
					if (base.hasSpecialItem.Value)
					{
						yDongleSource += 48;
					}
					b.Draw(this.Sprite.Texture, base.getLocalPosition(Game1.viewport) + stack_adjustment + new Vector2(32f, boundsHeight - 16 + ((this.readyToJump <= 0) ? (4 * (-2 + Math.Abs(this.Sprite.currentFrame % 4 - 2))) : (4 + 4 * (this.Sprite.currentFrame % 4 % 3))) + this.yOffset) * base.scale.Value, new Rectangle(xDongleSource, 168 + yDongleSource, 16, 24), base.hasSpecialItem.Value ? Color.White : this.color.Value, 0f, new Vector2(8f, 16f), 4f * Math.Max(0.2f, base.scale.Value - 0.4f * ((float)this.ageUntilFullGrown.Value / 120000f)), base.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, base.drawOnTop ? 0.991f : ((float)standingY / 10000f + 0.0001f)));
				}
				b.Draw(this.Sprite.Texture, base.getLocalPosition(Game1.viewport) + stack_adjustment + (new Vector2(32f, boundsHeight / 2 + ((this.readyToJump <= 0) ? (4 * (-2 + Math.Abs(this.Sprite.currentFrame % 4 - 2))) : (4 - 4 * (this.Sprite.currentFrame % 4 % 3))) + this.yOffset) + this.facePosition.Value) * Math.Max(0.2f, base.scale.Value - 0.4f * ((float)this.ageUntilFullGrown.Value / 120000f)), new Rectangle(32 + ((this.readyToJump > 0 || base.focusedOnFarmers) ? 16 : 0), 120 + ((this.readyToJump < 0 && (base.focusedOnFarmers || base.invincibleCountdown > 0)) ? 24 : 0), 16, 24), Color.White * ((this.FacingDirection == 0) ? 0.5f : 1f), 0f, new Vector2(8f, 16f), 4f * Math.Max(0.2f, base.scale.Value - 0.4f * ((float)this.ageUntilFullGrown.Value / 120000f)), SpriteEffects.None, Math.Max(0f, base.drawOnTop ? 0.991f : ((float)(standingY + i * 2) / 10000f + 0.0001f)));
			}
			if (base.isGlowing)
			{
				b.Draw(this.Sprite.Texture, base.getLocalPosition(Game1.viewport) + stack_adjustment + new Vector2(32f, boundsHeight / 2 + this.yOffset), this.Sprite.SourceRect, base.glowingColor * base.glowingTransparency, 0f, new Vector2(8f, 16f), 4f * Math.Max(0.2f, base.scale.Value), SpriteEffects.None, Math.Max(0f, base.drawOnTop ? 0.99f : ((float)standingY / 10000f + 0.001f)));
			}
		}
		if (this.pursuingMate.Value)
		{
			b.Draw(this.Sprite.Texture, base.getLocalPosition(Game1.viewport) + new Vector2(32f, -32 + this.yOffset), new Rectangle(16, 120, 8, 8), Color.White, 0f, new Vector2(3f, 3f), 4f, SpriteEffects.None, Math.Max(0f, base.drawOnTop ? 0.991f : ((float)base.StandingPixel.Y / 10000f)));
		}
		else if (this.avoidingMate.Value)
		{
			b.Draw(this.Sprite.Texture, base.getLocalPosition(Game1.viewport) + new Vector2(32f, -32 + this.yOffset), new Rectangle(24, 120, 8, 8), Color.White, 0f, new Vector2(4f, 4f), 4f, SpriteEffects.None, Math.Max(0f, base.drawOnTop ? 0.991f : ((float)base.StandingPixel.Y / 10000f)));
		}
	}

	public void moveTowardOtherSlime(GreenSlime other, bool moveAway, GameTime time)
	{
		Point curPixel = base.StandingPixel;
		Point otherPixel = other.StandingPixel;
		int xToGo = Math.Abs(otherPixel.X - curPixel.X);
		int yToGo = Math.Abs(otherPixel.Y - curPixel.Y);
		if (xToGo > 4 || yToGo > 4)
		{
			int dx = ((otherPixel.X > curPixel.X) ? 1 : (-1));
			int dy = ((otherPixel.Y > curPixel.Y) ? 1 : (-1));
			if (moveAway)
			{
				dx = -dx;
				dy = -dy;
			}
			double chanceForX = (double)xToGo / (double)(xToGo + yToGo);
			if (Game1.random.NextDouble() < chanceForX)
			{
				base.tryToMoveInDirection((dx > 0) ? 1 : 3, isFarmer: false, base.DamageToFarmer, glider: false);
			}
			else
			{
				base.tryToMoveInDirection((dy > 0) ? 2 : 0, isFarmer: false, base.DamageToFarmer, glider: false);
			}
		}
		this.Sprite.AnimateDown(time);
		if (base.invincibleCountdown > 0)
		{
			base.invincibleCountdown -= time.ElapsedGameTime.Milliseconds;
			if (base.invincibleCountdown <= 0)
			{
				base.stopGlowing();
			}
		}
	}

	public void doneMating()
	{
		this.readyToMate = 120000;
		this.matingCountdown = 2000;
		this.mate = null;
		this.pursuingMate.Value = false;
		this.avoidingMate.Value = false;
	}

	public override void noMovementProgressNearPlayerBehavior()
	{
		base.faceGeneralDirection(base.Player.getStandingPosition());
	}

	public void mateWith(GreenSlime mateToPursue, GameLocation location)
	{
		if (location.canSlimeMateHere())
		{
			GreenSlime baby = new GreenSlime(Vector2.Zero);
			Utility.recursiveFindPositionForCharacter(baby, location, base.Tile, 30);
			Random r = Utility.CreateRandom(Game1.stats.DaysPlayed, (double)Game1.uniqueIDForThisGame / 10.0, (double)base.scale.Value * 100.0, (double)mateToPursue.scale.Value * 100.0);
			switch (r.Next(4))
			{
			case 0:
				baby.color.Value = new Color(Math.Min(255, Math.Max(0, this.color.R + r.Next((int)((float)(-this.color.R) * 0.25f), (int)((float)(int)this.color.R * 0.25f)))), Math.Min(255, Math.Max(0, this.color.G + r.Next((int)((float)(-this.color.G) * 0.25f), (int)((float)(int)this.color.G * 0.25f)))), Math.Min(255, Math.Max(0, this.color.B + r.Next((int)((float)(-this.color.B) * 0.25f), (int)((float)(int)this.color.B * 0.25f)))));
				break;
			case 1:
			case 2:
				baby.color.Value = Utility.getBlendedColor(this.color.Value, mateToPursue.color.Value);
				break;
			case 3:
				baby.color.Value = new Color(Math.Min(255, Math.Max(0, mateToPursue.color.R + r.Next((int)((float)(-mateToPursue.color.R) * 0.25f), (int)((float)(int)mateToPursue.color.R * 0.25f)))), Math.Min(255, Math.Max(0, mateToPursue.color.G + r.Next((int)((float)(-mateToPursue.color.G) * 0.25f), (int)((float)(int)mateToPursue.color.G * 0.25f)))), Math.Min(255, Math.Max(0, mateToPursue.color.B + r.Next((int)((float)(-mateToPursue.color.B) * 0.25f), (int)((float)(int)mateToPursue.color.B * 0.25f)))));
				break;
			}
			int red = baby.color.R;
			int green = baby.color.G;
			int blue = baby.color.B;
			baby.Name = base.name.Value;
			if (baby.Name == "Tiger Slime")
			{
				baby.makeTigerSlime();
			}
			else if (red > 100 && blue > 100 && green < 50)
			{
				baby.parseMonsterInfo("Sludge");
				while (r.NextDouble() < 0.1)
				{
					baby.objectsToDrop.Add("386");
				}
				if (r.NextDouble() < 0.01)
				{
					baby.objectsToDrop.Add("337");
				}
			}
			else if (red >= 200 && green < 75)
			{
				baby.parseMonsterInfo("Sludge");
			}
			else if (blue >= 200 && red < 100)
			{
				baby.parseMonsterInfo("Frost Jelly");
			}
			baby.Health = r.Choose(base.Health, mateToPursue.Health);
			baby.Health = Math.Max(1, base.Health + r.Next(-4, 5));
			baby.DamageToFarmer = r.Choose(base.DamageToFarmer, mateToPursue.DamageToFarmer);
			baby.DamageToFarmer = Math.Max(0, base.DamageToFarmer + r.Next(-1, 2));
			baby.resilience.Value = r.Choose(base.resilience.Value, mateToPursue.resilience.Value);
			baby.resilience.Value = Math.Max(0, base.resilience.Value + r.Next(-1, 2));
			baby.missChance.Value = r.Choose(base.missChance.Value, mateToPursue.missChance.Value);
			baby.missChance.Value = Math.Max(0.0, base.missChance.Value + (double)((float)r.Next(-1, 2) / 100f));
			baby.Scale = r.Choose(base.scale.Value, mateToPursue.scale.Value);
			baby.Scale = Math.Max(0.6f, Math.Min(1.5f, base.scale.Value + (float)r.Next(-2, 3) / 100f));
			baby.Slipperiness = 8;
			base.speed = r.Choose(base.speed, mateToPursue.speed);
			if (r.NextDouble() < 0.015)
			{
				base.speed = Math.Max(1, Math.Min(6, base.speed + r.Next(-1, 2)));
			}
			baby.setTrajectory(Utility.getAwayFromPositionTrajectory(baby.GetBoundingBox(), base.getStandingPosition()) / 2f);
			baby.ageUntilFullGrown.Value = 120000;
			baby.Halt();
			baby.firstGeneration.Value = false;
			if (Utility.isOnScreen(base.Position, 128))
			{
				base.currentLocation.playSound("slime");
			}
		}
		mateToPursue.doneMating();
		this.doneMating();
	}

	public override List<Item> getExtraDropItems()
	{
		List<Item> extra = new List<Item>();
		if (base.name.Value != "Tiger Slime")
		{
			if (this.color.R >= 50 && this.color.R <= 100 && this.color.G >= 25 && this.color.G <= 50 && this.color.B <= 25)
			{
				extra.Add(ItemRegistry.Create("(O)388", Game1.random.Next(3, 7)));
				if (Game1.random.NextDouble() < 0.1)
				{
					extra.Add(ItemRegistry.Create("(O)709"));
				}
			}
			else if (this.color.R < 80 && this.color.G < 80 && this.color.B < 80)
			{
				extra.Add(ItemRegistry.Create("(O)382"));
				Random random = Utility.CreateRandom((double)base.Position.X * 777.0, (double)base.Position.Y * 77.0, Game1.stats.DaysPlayed);
				if (random.NextDouble() < 0.05)
				{
					extra.Add(ItemRegistry.Create("(O)553"));
				}
				if (random.NextDouble() < 0.05)
				{
					extra.Add(ItemRegistry.Create("(O)539"));
				}
			}
			else if (this.color.R > 200 && this.color.G > 180 && this.color.B < 50)
			{
				extra.Add(ItemRegistry.Create("(O)384", 2));
			}
			else if (this.color.R > 220 && this.color.G > 90 && this.color.G < 150 && this.color.B < 50)
			{
				extra.Add(ItemRegistry.Create("(O)378", 2));
			}
			else if (this.color.R > 230 && this.color.G > 230 && this.color.B > 230)
			{
				if (this.color.R % 2 == 1)
				{
					extra.Add(ItemRegistry.Create("(O)338"));
					if (this.color.G % 2 == 1)
					{
						extra.Add(ItemRegistry.Create("(O)338"));
					}
				}
				else
				{
					extra.Add(ItemRegistry.Create("(O)380"));
				}
				if ((this.color.R % 2 == 0 && this.color.G % 2 == 0 && this.color.B % 2 == 0) || this.color.Equals(Color.White))
				{
					extra.Add(new Object("72", 1));
				}
			}
			else if (this.color.R > 150 && this.color.G > 150 && this.color.B > 150)
			{
				extra.Add(ItemRegistry.Create("(O)390", 2));
			}
			else if (this.color.R > 150 && this.color.B > 180 && this.color.G < 50 && this.specialNumber.Value % (this.firstGeneration.Value ? 4 : 2) == 0)
			{
				extra.Add(ItemRegistry.Create("(O)386", 2));
				if (this.firstGeneration.Value && Game1.random.NextDouble() < 0.005)
				{
					extra.Add(ItemRegistry.Create("(O)485"));
				}
			}
		}
		if (Game1.MasterPlayer.mailReceived.Contains("slimeHutchBuilt") && this.specialNumber.Value == 1)
		{
			switch (base.Name)
			{
			case "Green Slime":
				extra.Add(ItemRegistry.Create("(O)680"));
				break;
			case "Frost Jelly":
				extra.Add(ItemRegistry.Create("(O)413"));
				break;
			case "Tiger Slime":
				extra.Add(ItemRegistry.Create("(O)857"));
				break;
			}
		}
		if (base.Name == "Tiger Slime")
		{
			if (Game1.random.NextDouble() < 0.001)
			{
				extra.Add(ItemRegistry.Create("(H)91"));
			}
			if (Game1.random.NextDouble() < 0.1)
			{
				extra.Add(ItemRegistry.Create("(O)831"));
				while (Game1.random.NextBool())
				{
					extra.Add(ItemRegistry.Create("(O)831"));
				}
			}
			else if (Game1.random.NextDouble() < 0.1)
			{
				extra.Add(ItemRegistry.Create("(O)829"));
			}
			else if (Game1.random.NextDouble() < 0.02)
			{
				extra.Add(ItemRegistry.Create("(O)833"));
				while (Game1.random.NextBool())
				{
					extra.Add(ItemRegistry.Create("(O)833"));
				}
			}
			else if (Game1.random.NextDouble() < 0.006)
			{
				extra.Add(ItemRegistry.Create("(O)835"));
			}
		}
		if (this.prismatic.Value && Game1.player.team.specialOrders.Where((SpecialOrder x) => x.questKey.Value == "Wizard2") != null)
		{
			Object o = ItemRegistry.Create<Object>("(O)876");
			o.specialItem = true;
			o.questItem.Value = true;
			return new List<Item> { o };
		}
		return extra;
	}

	public override void dayUpdate(int dayOfMonth)
	{
		if (this.ageUntilFullGrown.Value > 0)
		{
			this.ageUntilFullGrown.Value /= 2;
		}
		if (this.readyToMate > 0)
		{
			this.readyToMate /= 2;
		}
		base.dayUpdate(dayOfMonth);
	}

	protected override void updateAnimation(GameTime time)
	{
		if (this.wagTimer > 0)
		{
			this.wagTimer -= (int)time.ElapsedGameTime.TotalMilliseconds;
		}
		if (base.stunTime.Value > 0)
		{
			this.yOffset = 0;
		}
		else
		{
			this.yOffset = Math.Max(this.yOffset - (int)Math.Abs(base.xVelocity + base.yVelocity) / 2, -64);
			if (this.yOffset < 0)
			{
				this.yOffset = Math.Min(0, this.yOffset + 4 + (int)((this.yOffset <= -64) ? ((float)(-this.yOffset) / 8f) : ((float)(-this.yOffset) / 16f)));
			}
			this.timeSinceLastJump += time.ElapsedGameTime.Milliseconds;
		}
		if (Game1.random.NextDouble() < 0.01 && this.wagTimer <= 0)
		{
			this.wagTimer = 992;
		}
		if (Math.Abs(base.xVelocity) >= 0.5f || Math.Abs(base.yVelocity) >= 0.5f)
		{
			this.Sprite.AnimateDown(time);
		}
		else if (!base.Position.Equals(base.lastPosition))
		{
			this.animateTimer = 500;
		}
		if (this.animateTimer > 0 && this.readyToJump <= 0)
		{
			this.animateTimer -= time.ElapsedGameTime.Milliseconds;
			this.Sprite.AnimateDown(time);
		}
		base.resetAnimationSpeed();
	}

	public override void update(GameTime time, GameLocation location)
	{
		base.update(time, location);
		this.jumpEvent.Poll();
		this.attackedEvent.Poll();
	}

	public override void behaviorAtGameTick(GameTime time)
	{
		if (this.mate == null)
		{
			this.pursuingMate.Value = false;
			this.avoidingMate.Value = false;
		}
		switch (this.FacingDirection)
		{
		case 2:
			if (this.facePosition.X > 0f)
			{
				this.facePosition.X -= 2f;
			}
			else if (this.facePosition.X < 0f)
			{
				this.facePosition.X += 2f;
			}
			if (this.facePosition.Y < 0f)
			{
				this.facePosition.Y += 2f;
			}
			break;
		case 1:
			if (this.facePosition.X < 8f)
			{
				this.facePosition.X += 2f;
			}
			if (this.facePosition.Y < 0f)
			{
				this.facePosition.Y += 2f;
			}
			break;
		case 3:
			if (this.facePosition.X > -8f)
			{
				this.facePosition.X -= 2f;
			}
			if (this.facePosition.Y < 0f)
			{
				this.facePosition.Y += 2f;
			}
			break;
		case 0:
			if (this.facePosition.X > 0f)
			{
				this.facePosition.X -= 2f;
			}
			else if (this.facePosition.X < 0f)
			{
				this.facePosition.X += 2f;
			}
			if (this.facePosition.Y > -8f)
			{
				this.facePosition.Y -= 2f;
			}
			break;
		}
		if (this.stackedSlimes.Value <= 0)
		{
			if (this.ageUntilFullGrown.Value <= 0)
			{
				this.readyToMate -= time.ElapsedGameTime.Milliseconds;
			}
			else
			{
				this.ageUntilFullGrown.Value -= time.ElapsedGameTime.Milliseconds;
			}
		}
		if (this.pursuingMate.Value && this.mate != null)
		{
			if (this.readyToMate <= -35000)
			{
				this.mate.doneMating();
				this.doneMating();
				return;
			}
			this.moveTowardOtherSlime(this.mate, moveAway: false, time);
			if (this.mate.mate != null && this.mate.pursuingMate.Value && !this.mate.mate.Equals(this))
			{
				this.doneMating();
				return;
			}
			Vector2 curStandingPosition = base.getStandingPosition();
			Vector2 mateStandingPosition = this.mate.getStandingPosition();
			if (Vector2.Distance(curStandingPosition, mateStandingPosition) < (float)(this.GetBoundingBox().Width + 4))
			{
				if (this.mate.mate != null && this.mate.avoidingMate.Value && this.mate.mate.Equals(this))
				{
					this.mate.avoidingMate.Value = false;
					this.mate.matingCountdown = 2000;
					this.mate.pursuingMate.Value = true;
				}
				this.matingCountdown -= time.ElapsedGameTime.Milliseconds;
				if (base.currentLocation != null && this.matingCountdown <= 0 && this.pursuingMate.Value && (!base.currentLocation.isOutdoors.Value || Utility.getNumberOfCharactersInRadius(base.currentLocation, Utility.Vector2ToPoint(base.Position), 1) <= 4))
				{
					this.mateWith(this.mate, base.currentLocation);
				}
			}
			else if (Vector2.Distance(curStandingPosition, mateStandingPosition) > (float)(GreenSlime.matingRange * 2))
			{
				this.mate.mate = null;
				this.mate.avoidingMate.Value = false;
				this.mate = null;
			}
			return;
		}
		if (this.avoidingMate.Value && this.mate != null)
		{
			this.moveTowardOtherSlime(this.mate, moveAway: true, time);
			return;
		}
		if (this.readyToMate < 0 && this.cute.Value)
		{
			this.readyToMate = -1;
			if (Game1.random.NextDouble() < 0.001)
			{
				Point standingPixel = base.StandingPixel;
				GreenSlime newMate = (GreenSlime)Utility.checkForCharacterWithinArea(base.GetType(), base.Position, base.currentLocation, new Rectangle(standingPixel.X - GreenSlime.matingRange, standingPixel.Y - GreenSlime.matingRange, GreenSlime.matingRange * 2, GreenSlime.matingRange * 2));
				if (newMate != null && newMate.readyToMate <= 0 && !newMate.cute.Value && newMate.stackedSlimes.Value <= 0)
				{
					this.matingCountdown = 2000;
					this.mate = newMate;
					this.pursuingMate.Value = true;
					newMate.mate = this;
					newMate.avoidingMate.Value = true;
					this.addedSpeed = 1f;
					this.mate.addedSpeed = 1f;
					return;
				}
			}
		}
		else if (!base.isGlowing)
		{
			this.addedSpeed = 0f;
		}
		base.behaviorAtGameTick(time);
		if (this.readyToJump != -1)
		{
			this.Halt();
			base.IsWalkingTowardPlayer = false;
			this.readyToJump -= time.ElapsedGameTime.Milliseconds;
			this.Sprite.currentFrame = 16 + (800 - this.readyToJump) / 200;
			if (this.readyToJump <= 0)
			{
				this.timeSinceLastJump = this.timeSinceLastJump;
				base.Slipperiness = 10;
				base.IsWalkingTowardPlayer = true;
				this.readyToJump = -1;
				base.invincibleCountdown = 0;
				Vector2 trajectory = Utility.getAwayFromPlayerTrajectory(this.GetBoundingBox(), base.Player);
				trajectory.X = (0f - trajectory.X) / 2f;
				trajectory.Y = (0f - trajectory.Y) / 2f;
				this.jumpEvent.Fire(trajectory);
				base.setTrajectory((int)trajectory.X, (int)trajectory.Y);
			}
		}
		else if (Game1.random.NextDouble() < 0.1 && !base.focusedOnFarmers)
		{
			if (this.FacingDirection == 0 || this.FacingDirection == 2)
			{
				if (this.leftDrift.Value && !base.currentLocation.isCollidingPosition(this.nextPosition(3), Game1.viewport, isFarmer: false, 1, glider: false, this))
				{
					base.position.X -= base.speed;
				}
				else if (!this.leftDrift.Value && !base.currentLocation.isCollidingPosition(this.nextPosition(1), Game1.viewport, isFarmer: false, 1, glider: false, this))
				{
					base.position.X += base.speed;
				}
			}
			else if (this.leftDrift.Value && !base.currentLocation.isCollidingPosition(this.nextPosition(0), Game1.viewport, isFarmer: false, 1, glider: false, this))
			{
				base.position.Y -= base.speed;
			}
			else if (!this.leftDrift.Value && !base.currentLocation.isCollidingPosition(this.nextPosition(2), Game1.viewport, isFarmer: false, 1, glider: false, this))
			{
				base.position.Y += base.speed;
			}
			if (Game1.random.NextDouble() < 0.08)
			{
				this.leftDrift.Value = !this.leftDrift.Value;
			}
		}
		else if (this.withinPlayerThreshold() && this.timeSinceLastJump > (base.focusedOnFarmers ? 1000 : 4000) && Game1.random.NextDouble() < 0.01 && this.stackedSlimes.Value <= 0)
		{
			if (base.Name.Equals("Frost Jelly") && Game1.random.NextDouble() < 0.25)
			{
				this.addedSpeed = 2f;
				base.startGlowing(Color.Cyan, border: false, 0.15f);
			}
			else
			{
				this.addedSpeed = 0f;
				base.stopGlowing();
				this.readyToJump = 800;
			}
		}
	}

	private void doJump(Vector2 trajectory)
	{
		if (Utility.isOnScreen(base.Position, 128))
		{
			base.currentLocation.localSound("slime");
		}
		this.Sprite.currentFrame = 1;
	}
}
