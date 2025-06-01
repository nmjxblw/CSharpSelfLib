using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Extensions;
using StardewValley.Locations;

namespace StardewValley.Monsters;

public class BigSlime : Monster
{
	[XmlElement("c")]
	public readonly NetColor c = new NetColor();

	[XmlElement("heldObject")]
	public readonly NetRef<Item> heldItem = new NetRef<Item>();

	private float heldObjectBobTimer;

	public BigSlime()
	{
	}

	public BigSlime(Vector2 position, MineShaft mine)
		: this(position, mine.getMineArea())
	{
		this.Sprite.ignoreStopAnimation = true;
		base.ignoreMovementAnimations = true;
		base.HideShadow = true;
	}

	public BigSlime(Vector2 position, int mineArea)
		: base("Big Slime", position)
	{
		base.ignoreMovementAnimations = true;
		this.Sprite.ignoreStopAnimation = true;
		this.Sprite.SpriteWidth = 32;
		this.Sprite.SpriteHeight = 32;
		this.Sprite.UpdateSourceRect();
		this.Sprite.framesPerAnimation = 8;
		this.c.Value = Color.White;
		switch (mineArea)
		{
		case 0:
		case 10:
			this.c.Value = Color.Lime;
			break;
		case 40:
			this.c.Value = Color.Turquoise;
			base.Health *= 2;
			base.ExperienceGained *= 2;
			break;
		case 80:
			this.c.Value = Color.Red;
			base.Health *= 3;
			base.DamageToFarmer *= 2;
			base.ExperienceGained *= 3;
			break;
		case 121:
			this.c.Value = Color.BlueViolet;
			base.Health *= 4;
			base.DamageToFarmer *= 3;
			base.ExperienceGained *= 3;
			break;
		}
		int r = this.c.R;
		int g = this.c.G;
		int b = this.c.B;
		r += Game1.random.Next(-20, 21);
		g += Game1.random.Next(-20, 21);
		b += Game1.random.Next(-20, 21);
		this.c.R = (byte)Math.Max(Math.Min(255, r), 0);
		this.c.G = (byte)Math.Max(Math.Min(255, g), 0);
		this.c.B = (byte)Math.Max(Math.Min(255, b), 0);
		this.c.Value *= (float)Game1.random.Next(7, 11) / 10f;
		this.Sprite.interval = 300f;
		base.HideShadow = true;
		if (Game1.random.NextDouble() < 0.01 && mineArea >= 40)
		{
			this.heldItem.Value = ItemRegistry.Create("(O)221");
		}
		if (Game1.mine != null && Game1.mine.GetAdditionalDifficulty() > 0)
		{
			if (Game1.random.NextDouble() < 0.1)
			{
				this.heldItem.Value = ItemRegistry.Create("(O)858");
			}
			else if (Game1.random.NextDouble() < 0.005)
			{
				this.heldItem.Value = ItemRegistry.Create("(O)896");
			}
		}
		if (Game1.random.NextBool() && Game1.player.team.SpecialOrderRuleActive("SC_NO_FOOD"))
		{
			this.heldItem.Value = ItemRegistry.Create("(O)930");
		}
	}

	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(this.c, "c").AddField(this.heldItem, "heldItem");
	}

	/// <inheritdoc />
	public override void reloadSprite(bool onlyAppearance = false)
	{
		base.reloadSprite(onlyAppearance);
		this.Sprite.SpriteWidth = 32;
		this.Sprite.SpriteHeight = 32;
		this.Sprite.interval = 300f;
		this.Sprite.ignoreStopAnimation = true;
		base.ignoreMovementAnimations = true;
		base.HideShadow = true;
		this.Sprite.UpdateSourceRect();
		this.Sprite.framesPerAnimation = 8;
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
			base.Slipperiness = 3;
			base.Health -= actualDamage;
			base.setTrajectory(xTrajectory, yTrajectory);
			base.currentLocation.playSound("hitEnemy");
			base.IsWalkingTowardPlayer = true;
			if (base.Health <= 0)
			{
				base.deathAnimation();
				Game1.stats.SlimesKilled++;
				if (Game1.gameMode == 3 && Game1.random.NextDouble() < 0.75)
				{
					int toCreate = Game1.random.Next(2, 5);
					for (int i = 0; i < toCreate; i++)
					{
						base.currentLocation.characters.Add(new GreenSlime(base.Position, Game1.CurrentMineLevel));
						base.currentLocation.characters[base.currentLocation.characters.Count - 1].setTrajectory(xTrajectory / 8 + Game1.random.Next(-2, 3), yTrajectory / 8 + Game1.random.Next(-2, 3));
						base.currentLocation.characters[base.currentLocation.characters.Count - 1].willDestroyObjectsUnderfoot = false;
						base.currentLocation.characters[base.currentLocation.characters.Count - 1].moveTowardPlayer(4);
						base.currentLocation.characters[base.currentLocation.characters.Count - 1].Scale = 0.75f + (float)Game1.random.Next(-5, 10) / 100f;
						base.currentLocation.characters[base.currentLocation.characters.Count - 1].currentLocation = base.currentLocation;
					}
				}
			}
		}
		return actualDamage;
	}

	protected override void localDeathAnimation()
	{
		base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(44, base.Position, this.c.Value, 10, flipped: false, 70f));
		base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(44, base.Position + new Vector2(-32f, 0f), this.c.Value, 10, flipped: false, 70f)
		{
			delayBeforeAnimationStart = 100
		});
		base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(44, base.Position + new Vector2(32f, 0f), this.c.Value, 10, flipped: false, 70f)
		{
			delayBeforeAnimationStart = 200
		});
		base.currentLocation.localSound("slimedead");
		base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(44, base.Position + new Vector2(0f, -32f), this.c.Value, 10)
		{
			delayBeforeAnimationStart = 300
		});
	}

	protected override void updateAnimation(GameTime time)
	{
		int currentIndex = this.Sprite.currentFrame;
		this.Sprite.AnimateDown(time);
		if (this.isMoving())
		{
			this.Sprite.interval = 100f;
			this.heldObjectBobTimer += (float)time.ElapsedGameTime.TotalMilliseconds * 0.007853982f;
		}
		else
		{
			this.Sprite.interval = 200f;
			this.heldObjectBobTimer += (float)time.ElapsedGameTime.TotalMilliseconds * 0.003926991f;
		}
		if (Utility.isOnScreen(base.Position, 128) && this.Sprite.currentFrame == 0 && currentIndex == 7)
		{
			base.currentLocation.localSound("slimeHit");
		}
	}

	public override List<Item> getExtraDropItems()
	{
		if (this.heldItem.Value != null)
		{
			return new List<Item> { this.heldItem.Value };
		}
		return base.getExtraDropItems();
	}

	public override void draw(SpriteBatch b)
	{
		if (!base.IsInvisible && Utility.isOnScreen(base.Position, 128))
		{
			int standingY = base.StandingPixel.Y;
			this.heldItem.Value?.drawInMenu(b, base.getLocalPosition(Game1.viewport) + new Vector2(28f, -16f + (float)Math.Sin(this.heldObjectBobTimer + 1f) * 4f), 1f, 1f, (float)(standingY - 1) / 10000f, StackDrawType.Hide, Color.White, drawShadow: false);
			b.Draw(this.Sprite.Texture, base.getLocalPosition(Game1.viewport) + new Vector2(56f, 16 + base.yJumpOffset), this.Sprite.SourceRect, this.c.Value, base.rotation, new Vector2(16f, 16f), Math.Max(0.2f, base.scale.Value) * 4f, base.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, base.drawOnTop ? 0.991f : ((float)standingY / 10000f)));
			if (base.isGlowing)
			{
				b.Draw(this.Sprite.Texture, base.getLocalPosition(Game1.viewport) + new Vector2(56f, 16 + base.yJumpOffset), this.Sprite.SourceRect, base.glowingColor * base.glowingTransparency, 0f, new Vector2(16f, 16f), 4f * Math.Max(0.2f, base.scale.Value), base.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, base.drawOnTop ? 0.991f : ((float)standingY / 10000f + 0.001f)));
			}
		}
	}

	public override Rectangle GetBoundingBox()
	{
		Vector2 position = base.Position;
		return new Rectangle((int)position.X + 8, (int)position.Y, this.Sprite.SpriteWidth * 4 * 3 / 4, 64);
	}

	public override void shedChunks(int number, float scale)
	{
	}
}
