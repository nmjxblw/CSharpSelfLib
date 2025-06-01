using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Pathfinding;
using StardewValley.TerrainFeatures;

namespace StardewValley.Monsters;

public class DustSpirit : Monster
{
	[XmlIgnore]
	public bool seenFarmer;

	[XmlIgnore]
	public bool runningAwayFromFarmer;

	[XmlIgnore]
	public bool chargingFarmer;

	public byte voice;

	[XmlIgnore]
	public ICue meep;

	public DustSpirit()
	{
	}

	public DustSpirit(Vector2 position)
		: base("Dust Spirit", position)
	{
		base.IsWalkingTowardPlayer = false;
		this.Sprite.interval = 45f;
		base.Scale = (float)Game1.random.Next(75, 101) / 100f;
		this.voice = (byte)Game1.random.Next(1, 24);
		base.HideShadow = true;
	}

	public DustSpirit(Vector2 position, bool chargingTowardFarmer)
		: base("Dust Spirit", position)
	{
		base.IsWalkingTowardPlayer = false;
		if (chargingTowardFarmer)
		{
			this.chargingFarmer = true;
			this.seenFarmer = true;
		}
		this.Sprite.interval = 45f;
		base.Scale = (float)Game1.random.Next(75, 101) / 100f;
		base.HideShadow = true;
	}

	public override void draw(SpriteBatch b)
	{
		if (!base.IsInvisible && Utility.isOnScreen(base.Position, 128))
		{
			int standingY = base.StandingPixel.Y;
			b.Draw(this.Sprite.Texture, base.getLocalPosition(Game1.viewport) + new Vector2(32 + ((base.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), 64 + base.yJumpOffset), this.Sprite.SourceRect, Color.White, base.rotation, new Vector2(8f, 16f), new Vector2(base.scale.Value + (float)Math.Max(-0.1, (double)(base.yJumpOffset + 32) / 128.0), base.scale.Value - Math.Max(-0.1f, (float)base.yJumpOffset / 256f)) * 4f, base.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, base.drawOnTop ? 0.991f : ((float)standingY / 10000f)));
			if (base.isGlowing)
			{
				b.Draw(this.Sprite.Texture, base.getLocalPosition(Game1.viewport) + new Vector2(32f, 64 + base.yJumpOffset), this.Sprite.SourceRect, base.glowingColor * base.glowingTransparency, base.rotation, new Vector2(8f, 16f), Math.Max(0.2f, base.scale.Value) * 4f, base.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, base.drawOnTop ? 0.99f : ((float)standingY / 10000f + 0.001f)));
			}
			b.Draw(Game1.shadowTexture, base.getLocalPosition(Game1.viewport) + new Vector2(32f, 80f), Game1.shadowTexture.Bounds, Color.White, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 4f + (float)base.yJumpOffset / 64f, SpriteEffects.None, (float)(standingY - 1) / 10000f);
		}
	}

	protected override void sharedDeathAnimation()
	{
	}

	protected override void localDeathAnimation()
	{
		base.currentLocation.localSound("dustMeep");
		base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(44, base.Position, new Color(50, 50, 80), 10));
		base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(44, base.Position + new Vector2(Game1.random.Next(-32, 32), Game1.random.Next(-32, 32)), new Color(50, 50, 80), 10)
		{
			delayBeforeAnimationStart = 150,
			scale = 0.5f
		});
		base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(44, base.Position + new Vector2(Game1.random.Next(-32, 32), Game1.random.Next(-32, 32)), new Color(50, 50, 80), 10)
		{
			delayBeforeAnimationStart = 300,
			scale = 0.5f
		});
		base.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(44, base.Position + new Vector2(Game1.random.Next(-32, 32), Game1.random.Next(-32, 32)), new Color(50, 50, 80), 10)
		{
			delayBeforeAnimationStart = 450,
			scale = 0.5f
		});
	}

	public override void shedChunks(int number, float scale)
	{
		Point standingPixel = base.StandingPixel;
		Game1.createRadialDebris(base.currentLocation, this.Sprite.textureName.Value, new Rectangle(0, 16, 16, 16), 8, standingPixel.X, standingPixel.Y, number, base.TilePoint.Y, Color.White, (base.Health <= 0) ? 4f : 2f);
	}

	public void offScreenBehavior(Character c, GameLocation l)
	{
	}

	public virtual bool CaughtInWeb()
	{
		if (base.currentLocation != null && base.currentLocation.terrainFeatures.TryGetValue(base.Tile, out var terrainFeature) && terrainFeature is Grass grass)
		{
			return grass.grassType.Value == 6;
		}
		return false;
	}

	protected override void updateAnimation(GameTime time)
	{
		if (base.yJumpOffset == 0)
		{
			if (base.isHardModeMonster.Value && this.CaughtInWeb())
			{
				this.Sprite.Animate(time, 5, 3, 200f);
				return;
			}
			this.jumpWithoutSound();
			base.yJumpVelocity = (float)Game1.random.Next(50, 70) / 10f;
			if (Game1.random.NextDouble() < 0.1 && (this.meep == null || !this.meep.IsPlaying) && Utility.isOnScreen(base.Position, 64) && Game1.currentLocation == base.currentLocation)
			{
				Game1.playSound("dustMeep", this.voice * 100 + Game1.random.Next(-100, 100), out this.meep);
			}
		}
		this.Sprite.AnimateDown(time);
		base.resetAnimationSpeed();
	}

	public override void behaviorAtGameTick(GameTime time)
	{
		base.behaviorAtGameTick(time);
		if (base.yJumpOffset == 0)
		{
			if (Game1.random.NextDouble() < 0.01)
			{
				Vector2 standingPixel = base.getStandingPosition();
				Game1.multiplayer.broadcastSprites(base.currentLocation, new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 128, 64, 64), 40f, 4, 0, standingPixel + new Vector2(-21f, 0f), flicker: false, flipped: false)
				{
					layerDepth = (standingPixel.Y - 10f) / 10000f
				});
				foreach (Vector2 v in Utility.getAdjacentTileLocations(base.Tile))
				{
					if (base.currentLocation.objects.TryGetValue(v, out var obj) && (obj.IsBreakableStone() || obj.IsTwig()))
					{
						base.currentLocation.destroyObject(v, null);
					}
				}
				base.yJumpVelocity *= 2f;
			}
			if (!this.chargingFarmer)
			{
				base.xVelocity = (float)Game1.random.Next(-20, 21) / 5f;
			}
		}
		if (this.chargingFarmer)
		{
			base.Slipperiness = 10;
			Vector2 v2 = Utility.getAwayFromPlayerTrajectory(this.GetBoundingBox(), base.Player);
			base.xVelocity += (0f - v2.X) / 150f + ((Game1.random.NextDouble() < 0.01) ? ((float)Game1.random.Next(-50, 50) / 10f) : 0f);
			if (Math.Abs(base.xVelocity) > 5f)
			{
				base.xVelocity = Math.Sign(base.xVelocity) * 5;
			}
			base.yVelocity += (0f - v2.Y) / 150f + ((Game1.random.NextDouble() < 0.01) ? ((float)Game1.random.Next(-50, 50) / 10f) : 0f);
			if (Math.Abs(base.yVelocity) > 5f)
			{
				base.yVelocity = Math.Sign(base.yVelocity) * 5;
			}
			if (Game1.random.NextDouble() < 0.0001)
			{
				base.controller = new PathFindController(this, base.currentLocation, base.Player.TilePoint, Game1.random.Next(4), null, 300);
				this.chargingFarmer = false;
			}
			if (base.isHardModeMonster.Value && this.CaughtInWeb())
			{
				base.xVelocity = 0f;
				base.yVelocity = 0f;
				if (base.shakeTimer <= 0 && Game1.random.NextDouble() < 0.05)
				{
					base.shakeTimer = 200;
				}
			}
		}
		else if (!this.seenFarmer && Utility.doesPointHaveLineOfSightInMine(base.currentLocation, base.getStandingPosition() / 64f, base.Player.getStandingPosition() / 64f, 8))
		{
			this.seenFarmer = true;
		}
		else if (this.seenFarmer && base.controller == null && !this.runningAwayFromFarmer)
		{
			this.addedSpeed = 2f;
			base.controller = new PathFindController(this, base.currentLocation, Utility.isOffScreenEndFunction, -1, offScreenBehavior, 350, Point.Zero);
			this.runningAwayFromFarmer = true;
		}
		else if (base.controller == null && this.runningAwayFromFarmer)
		{
			this.chargingFarmer = true;
		}
	}
}
