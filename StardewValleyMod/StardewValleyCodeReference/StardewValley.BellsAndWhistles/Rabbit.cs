using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewValley.TerrainFeatures;

namespace StardewValley.BellsAndWhistles;

public class Rabbit : Critter
{
	private int characterCheckTimer = 200;

	private bool running;

	public Rabbit(GameLocation location, Vector2 position, bool flip)
	{
		bool isWinter = location.IsWinterHere();
		base.position = position * 64f;
		position.Y += 48f;
		base.flip = flip;
		base.baseFrame = (isWinter ? 74 : 54);
		base.sprite = new AnimatedSprite(Critter.critterTexture, isWinter ? 69 : 68, 32, 32);
		base.sprite.loop = true;
		base.startingPosition = position;
	}

	public override bool update(GameTime time, GameLocation environment)
	{
		this.characterCheckTimer -= time.ElapsedGameTime.Milliseconds;
		if (this.characterCheckTimer <= 0 && !this.running)
		{
			if (Utility.isOnScreen(base.position, -32))
			{
				this.running = true;
				base.sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
				{
					new FarmerSprite.AnimationFrame(base.baseFrame, 40),
					new FarmerSprite.AnimationFrame(base.baseFrame + 1, 40),
					new FarmerSprite.AnimationFrame(base.baseFrame + 2, 40),
					new FarmerSprite.AnimationFrame(base.baseFrame + 3, 100),
					new FarmerSprite.AnimationFrame(base.baseFrame + 5, 70),
					new FarmerSprite.AnimationFrame(base.baseFrame + 5, 40)
				});
				base.sprite.loop = true;
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
