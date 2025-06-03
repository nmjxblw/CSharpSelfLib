using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewValley.TerrainFeatures;

namespace StardewValley.BellsAndWhistles;

public class Opossum : Critter
{
	private int characterCheckTimer = 1500;

	private bool running;

	private int jumpTimer = -1;

	public Opossum(GameLocation location, Vector2 position, bool flip)
	{
		this.characterCheckTimer = Game1.random.Next(500, 3000);
		base.position = position * 64f;
		position.Y += 48f;
		base.flip = flip;
		base.baseFrame = 150;
		base.sprite = new AnimatedSprite(Critter.critterTexture, 150, 32, 32);
		base.sprite.loop = true;
		base.sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
		{
			new FarmerSprite.AnimationFrame(base.baseFrame, 500),
			new FarmerSprite.AnimationFrame(base.baseFrame + 1, 50),
			new FarmerSprite.AnimationFrame(base.baseFrame + 2, 500),
			new FarmerSprite.AnimationFrame(base.baseFrame + 1, 50),
			new FarmerSprite.AnimationFrame(base.baseFrame, 1000),
			new FarmerSprite.AnimationFrame(base.baseFrame + 1, 50),
			new FarmerSprite.AnimationFrame(base.baseFrame + 2, 700),
			new FarmerSprite.AnimationFrame(base.baseFrame + 1, 50)
		});
		base.startingPosition = position;
	}

	public override bool update(GameTime time, GameLocation environment)
	{
		this.characterCheckTimer -= (int)time.ElapsedGameTime.TotalMilliseconds;
		if (Utility.isThereAFarmerOrCharacterWithinDistance(base.position / 64f, 8, environment) != null)
		{
			this.characterCheckTimer = 0;
		}
		if (this.jumpTimer > -1)
		{
			this.jumpTimer -= (int)time.ElapsedGameTime.TotalMilliseconds;
			base.yJumpOffset = (0f - (float)Math.Sin((double)((600f - (float)this.jumpTimer) / 600f) * Math.PI)) * 4f * 16f;
			if (this.jumpTimer <= -1)
			{
				this.running = true;
				base.sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
				{
					new FarmerSprite.AnimationFrame(base.baseFrame + 5, 40),
					new FarmerSprite.AnimationFrame(base.baseFrame + 6, 40),
					new FarmerSprite.AnimationFrame(base.baseFrame + 7, 40),
					new FarmerSprite.AnimationFrame(base.baseFrame + 8, 40)
				});
				base.sprite.loop = true;
			}
		}
		else if (this.characterCheckTimer <= 0 && !this.running)
		{
			if (Utility.isOnScreen(base.position, -32) && this.jumpTimer == -1)
			{
				this.jumpTimer = 600;
				base.sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
				{
					new FarmerSprite.AnimationFrame(base.baseFrame + 4, 20)
				});
			}
			this.characterCheckTimer = 200;
		}
		if (this.running)
		{
			base.position.X += (base.flip ? (-6) : 6);
		}
		if (this.running && this.characterCheckTimer <= 0)
		{
			this.characterCheckTimer = 200;
			if (environment.largeTerrainFeatures != null)
			{
				Rectangle tileRect = new Rectangle((int)base.position.X + 32, (int)base.position.Y - 32, 4, 192);
				foreach (LargeTerrainFeature f in environment.largeTerrainFeatures)
				{
					if (f is Bush bush && f.getBoundingBox().Intersects(tileRect))
					{
						bush.performUseAction(f.Tile);
						return true;
					}
				}
			}
		}
		return base.update(time, environment);
	}
}
