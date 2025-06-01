using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Extensions;
using xTile.Layers;
using xTile.Tiles;

namespace StardewValley.Monsters;

public class Duggy : Monster
{
	public Duggy()
	{
		base.HideShadow = true;
	}

	public Duggy(Vector2 position)
		: base("Duggy", position)
	{
		base.IsWalkingTowardPlayer = false;
		base.IsInvisible = true;
		base.DamageToFarmer = 0;
		this.Sprite.currentFrame = 0;
		base.HideShadow = true;
	}

	public Duggy(Vector2 position, bool magmaDuggy)
		: base("Magma Duggy", position)
	{
		base.IsWalkingTowardPlayer = false;
		base.IsInvisible = true;
		base.DamageToFarmer = 0;
		this.Sprite.currentFrame = 0;
		base.HideShadow = true;
	}

	protected override void initNetFields()
	{
		base.initNetFields();
		base.position.Field.Interpolated(interpolate: false, wait: true);
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
			base.currentLocation.playSound("hitEnemy");
			if (base.Health <= 0)
			{
				base.deathAnimation();
			}
		}
		return actualDamage;
	}

	protected override void localDeathAnimation()
	{
		base.currentLocation.localSound("monsterdead");
		Utility.makeTemporarySpriteJuicier(new TemporaryAnimatedSprite(44, base.Position, Color.DarkRed, 10)
		{
			holdLastFrame = true,
			alphaFade = 0.01f,
			interval = 70f
		}, base.currentLocation);
	}

	protected override void sharedDeathAnimation()
	{
	}

	public override void update(GameTime time, GameLocation location)
	{
		if (base.invincibleCountdown > 0)
		{
			base.glowingColor = Color.Cyan;
			base.invincibleCountdown -= time.ElapsedGameTime.Milliseconds;
			if (base.invincibleCountdown <= 0)
			{
				base.stopGlowing();
			}
		}
		if (location.farmers.Any())
		{
			this.behaviorAtGameTick(time);
			Layer backLayer = location.map.RequireLayer("Back");
			if (base.Position.X < 0f || base.Position.X > (float)(backLayer.LayerWidth * 64) || base.Position.Y < 0f || base.Position.Y > (float)(backLayer.LayerHeight * 64))
			{
				location.characters.Remove(this);
			}
			base.updateGlow();
			if (base.stunTime.Value > 0)
			{
				base.stunTime.Value -= (int)time.ElapsedGameTime.TotalMilliseconds;
			}
		}
	}

	public override void draw(SpriteBatch b)
	{
		if (!base.IsInvisible && Utility.isOnScreen(base.Position, 128))
		{
			Rectangle bounds = this.GetBoundingBox();
			int standingY = base.StandingPixel.Y;
			b.Draw(this.Sprite.Texture, base.getLocalPosition(Game1.viewport) + new Vector2(32f, bounds.Height / 2 + base.yJumpOffset), this.Sprite.SourceRect, Color.White, base.rotation, new Vector2(8f, 16f), Math.Max(0.2f, base.scale.Value) * 4f, base.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, base.drawOnTop ? 0.991f : ((float)standingY / 10000f)));
			if (base.isGlowing)
			{
				b.Draw(this.Sprite.Texture, base.getLocalPosition(Game1.viewport) + new Vector2(32f, bounds.Height / 2 + base.yJumpOffset), this.Sprite.SourceRect, base.glowingColor * base.glowingTransparency, base.rotation, new Vector2(8f, 16f), Math.Max(0.2f, base.scale.Value) * 4f, base.flip ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, base.drawOnTop ? 0.991f : ((float)standingY / 10000f + 0.001f)));
			}
		}
	}

	public override void behaviorAtGameTick(GameTime time)
	{
		base.behaviorAtGameTick(time);
		base.isEmoting = false;
		this.Sprite.loop = false;
		if (base.stunTime.Value > 0)
		{
			return;
		}
		Rectangle r = this.GetBoundingBox();
		if (this.Sprite.currentFrame < 4)
		{
			r.Inflate(128, 128);
			if (!base.IsInvisible || r.Contains(base.Player.StandingPixel))
			{
				if (base.IsInvisible)
				{
					Tile tile = base.currentLocation.map.RequireLayer("Back").Tiles[base.Player.TilePoint.X, base.Player.TilePoint.Y];
					if (tile.Properties.ContainsKey("NPCBarrier") || (!tile.TileIndexProperties.ContainsKey("Diggable") && tile.TileIndex != 0))
					{
						return;
					}
					base.Position = new Vector2(base.Player.Position.X, base.Player.Position.Y + (float)base.Player.Sprite.SpriteHeight - (float)this.Sprite.SpriteHeight);
					base.currentLocation.localSound("Duggy");
					base.Position = base.Player.Tile * 64f;
				}
				base.IsInvisible = false;
				this.Sprite.interval = 100f;
				this.Sprite.AnimateDown(time);
			}
		}
		if (this.Sprite.currentFrame >= 4 && this.Sprite.currentFrame < 8)
		{
			r.Inflate(-128, -128);
			base.currentLocation.isCollidingPosition(r, Game1.viewport, isFarmer: false, 8, glider: false, this);
			this.Sprite.AnimateRight(time);
			this.Sprite.interval = 220f;
			base.DamageToFarmer = 8;
		}
		if (this.Sprite.currentFrame >= 8)
		{
			this.Sprite.AnimateUp(time);
		}
		if (this.Sprite.currentFrame >= 10)
		{
			base.IsInvisible = true;
			this.Sprite.currentFrame = 0;
			Point tile2 = base.TilePoint;
			base.currentLocation.map.RequireLayer("Back").Tiles[tile2.X, tile2.Y].TileIndex = 0;
			base.currentLocation.removeObjectsAndSpawned(tile2.X, tile2.Y, 1, 1);
			base.DamageToFarmer = 0;
		}
	}
}
