using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Enchantments;
using StardewValley.Locations;
using StardewValley.Tools;

namespace StardewValley.Monsters;

public class Bug : Monster
{
	[XmlElement("isArmoredBug")]
	public readonly NetBool isArmoredBug = new NetBool(value: false);

	public Bug()
	{
	}

	public Bug(Vector2 position, int facingDirection, string specialType)
		: this(position, 0)
	{
		this.faceDirection(facingDirection);
		if (specialType.Contains("Assassin"))
		{
			this.Sprite.LoadTexture("Characters\\Monsters\\Assassin Bug");
			base.DamageToFarmer = 50;
			base.Health = 500;
			base.speed++;
		}
	}

	public Bug(Vector2 position, int areaType)
		: base("Bug", position)
	{
		this.Sprite.SpriteHeight = 16;
		this.Sprite.UpdateSourceRect();
		base.onCollision = collide;
		base.yOffset = -32f;
		base.IsWalkingTowardPlayer = false;
		base.setMovingInFacingDirection();
		base.defaultAnimationInterval.Value = 40;
		base.collidesWithOtherCharacters.Value = false;
		if (areaType == 121)
		{
			this.isArmoredBug.Value = true;
			this.Sprite.LoadTexture("Characters\\Monsters\\Armored Bug");
			base.DamageToFarmer *= 2;
			base.Slipperiness = -1;
			base.Health = 150;
		}
		base.HideShadow = true;
	}

	public Bug(Vector2 position, int facingDirection, MineShaft mine)
		: this(position, mine.getMineArea())
	{
		this.faceDirection(facingDirection);
		base.HideShadow = true;
	}

	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(this.isArmoredBug, "isArmoredBug");
	}

	protected override void updateMonsterSlaveAnimation(GameTime time)
	{
		this.Sprite.faceDirection(this.FacingDirection);
		this.Sprite.animateOnce(time);
	}

	/// <inheritdoc />
	public override void reloadSprite(bool onlyAppearance = false)
	{
		base.reloadSprite(onlyAppearance);
		this.Sprite.SpriteHeight = 16;
		this.Sprite.UpdateSourceRect();
	}

	private void collide(GameLocation location)
	{
		Rectangle bb = this.nextPosition(this.FacingDirection);
		foreach (Farmer farmer in location.farmers)
		{
			if (farmer.GetBoundingBox().Intersects(bb))
			{
				return;
			}
		}
		this.FacingDirection = (this.FacingDirection + 2) % 4;
		base.setMovingInFacingDirection();
	}

	public override void BuffForAdditionalDifficulty(int additional_difficulty)
	{
		this.FacingDirection = Math.Abs((this.FacingDirection + Game1.random.Next(-1, 2)) % 4);
		this.Halt();
		base.setMovingInFacingDirection();
		base.BuffForAdditionalDifficulty(additional_difficulty);
	}

	public override int takeDamage(int damage, int xTrajectory, int yTrajectory, bool isBomb, double addedPrecision, Farmer who)
	{
		int actualDamage = Math.Max(1, damage - base.resilience.Value);
		if (this.isArmoredBug.Value && (isBomb || !(who.CurrentTool is MeleeWeapon weapon) || !weapon.hasEnchantmentOfType<BugKillerEnchantment>()))
		{
			base.currentLocation.playSound("crafting");
			return 0;
		}
		if (Game1.random.NextDouble() < base.missChance.Value - base.missChance.Value * addedPrecision)
		{
			actualDamage = -1;
		}
		else
		{
			base.Health -= actualDamage;
			base.currentLocation.playSound("hitEnemy");
			base.setTrajectory(xTrajectory / 3, yTrajectory / 3);
			if (base.isHardModeMonster.Value)
			{
				this.FacingDirection = Math.Abs((this.FacingDirection + Game1.random.Next(-1, 2)) % 4);
				this.Halt();
				base.setMovingInFacingDirection();
			}
			if (base.Health <= 0)
			{
				base.deathAnimation();
			}
		}
		return actualDamage;
	}

	public override List<Item> getExtraDropItems()
	{
		if (this.isArmoredBug.Value)
		{
			List<Item> additional_drops = new List<Item>();
			if (Game1.random.NextDouble() <= 0.1)
			{
				additional_drops.Add(ItemRegistry.Create("(O)874"));
			}
			return additional_drops;
		}
		return base.getExtraDropItems();
	}

	public override void draw(SpriteBatch b)
	{
		if (!base.IsInvisible && Utility.isOnScreen(base.Position, 128))
		{
			Vector2 offset = default(Vector2);
			if (this.FacingDirection % 2 == 0)
			{
				offset.X = (float)(Math.Sin((double)((float)Game1.currentGameTime.TotalGameTime.Milliseconds / 1000f) * (Math.PI * 2.0)) * 10.0);
			}
			else
			{
				offset.Y = (float)(Math.Sin((double)((float)Game1.currentGameTime.TotalGameTime.Milliseconds / 1000f) * (Math.PI * 2.0)) * 10.0);
			}
			int standingY = base.StandingPixel.Y;
			b.Draw(Game1.shadowTexture, base.getLocalPosition(Game1.viewport) + new Vector2((float)(this.Sprite.SpriteWidth * 4) / 2f + offset.X, this.GetBoundingBox().Height * 5 / 2 - 48), Game1.shadowTexture.Bounds, Color.White, 0f, Utility.PointToVector2(Game1.shadowTexture.Bounds.Center), (4f + (float)base.yJumpOffset / 40f) * base.scale.Value, SpriteEffects.None, Math.Max(0f, (float)standingY / 10000f) - 1E-06f);
			b.Draw(this.Sprite.Texture, base.getLocalPosition(Game1.viewport) + new Vector2(32f, base.yJumpOffset) + offset, this.Sprite.SourceRect, Color.White, base.rotation, new Vector2(8f, 16f), 4f, base.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, base.drawOnTop ? 0.991f : ((float)standingY / 10000f)));
		}
	}

	protected override void localDeathAnimation()
	{
		base.localDeathAnimation();
		base.currentLocation.localSound("slimedead");
		Utility.makeTemporarySpriteJuicier(new TemporaryAnimatedSprite(44, base.Position + new Vector2(0f, -32f), Color.Violet, 10)
		{
			holdLastFrame = true,
			alphaFade = 0.01f,
			interval = 70f
		}, base.currentLocation);
	}

	public override void shedChunks(int number, float scale)
	{
		Point standingPixel = base.StandingPixel;
		Game1.createRadialDebris(base.currentLocation, this.Sprite.textureName.Value, new Rectangle(0, this.Sprite.getHeight() * 4, 16, 16), 8, standingPixel.X, standingPixel.Y, number, base.TilePoint.Y, Color.White, 4f);
	}
}
