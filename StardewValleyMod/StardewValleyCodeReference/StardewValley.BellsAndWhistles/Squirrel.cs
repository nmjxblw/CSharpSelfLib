using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.TerrainFeatures;

namespace StardewValley.BellsAndWhistles;

public class Squirrel : Critter
{
	private int nextNibbleTimer = 1000;

	private int treeRunTimer;

	private int characterCheckTimer = 200;

	private bool running;

	private Tree climbed;

	private Vector2 treeTile;

	public Squirrel(Vector2 position, bool flip)
	{
		base.position = position * 64f;
		base.flip = flip;
		base.baseFrame = 60;
		base.sprite = new AnimatedSprite(Critter.critterTexture, base.baseFrame, 32, 32);
		base.sprite.loop = false;
		base.startingPosition = position;
	}

	private void doneNibbling(Farmer who)
	{
		this.nextNibbleTimer = Game1.random.Next(2000);
	}

	public override void draw(SpriteBatch b)
	{
		base.sprite.draw(b, Game1.GlobalToLocal(Game1.viewport, base.position + new Vector2(-64 + ((this.treeRunTimer > 0) ? (base.flip ? 224 : (-16)) : 0), -64f + base.yJumpOffset + base.yOffset + (float)((this.treeRunTimer > 0) ? ((!base.flip) ? 128 : 0) : 0))), (base.position.Y + 64f + (float)((this.treeRunTimer > 0) ? 128 : 0)) / 10000f + base.position.X / 1000000f, 0, 0, Color.White, base.flip, 4f, (this.treeRunTimer > 0) ? ((float)((double)(base.flip ? 1 : (-1)) * Math.PI / 2.0)) : 0f);
		if (this.treeRunTimer <= 0)
		{
			b.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, base.position + new Vector2(0f, 60f)), Game1.shadowTexture.Bounds, Color.White, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 3f + Math.Max(-3f, (base.yJumpOffset + base.yOffset) / 16f), SpriteEffects.None, (base.position.Y - 1f) / 10000f);
		}
	}

	public override bool update(GameTime time, GameLocation environment)
	{
		this.nextNibbleTimer -= time.ElapsedGameTime.Milliseconds;
		if (base.sprite.CurrentAnimation == null && this.nextNibbleTimer <= 0)
		{
			int nibbles = Game1.random.Next(2, 8);
			List<FarmerSprite.AnimationFrame> anim = new List<FarmerSprite.AnimationFrame>();
			for (int i = 0; i < nibbles; i++)
			{
				anim.Add(new FarmerSprite.AnimationFrame(base.baseFrame, 200));
				anim.Add(new FarmerSprite.AnimationFrame(base.baseFrame + 1, 200));
			}
			anim.Add(new FarmerSprite.AnimationFrame(base.baseFrame, 200, secondaryArm: false, flip: false, doneNibbling));
			base.sprite.setCurrentAnimation(anim);
		}
		this.characterCheckTimer -= time.ElapsedGameTime.Milliseconds;
		if (this.characterCheckTimer <= 0 && !this.running)
		{
			if (Utility.isThereAFarmerOrCharacterWithinDistance(base.position / 64f, 12, environment) != null)
			{
				this.running = true;
				base.sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
				{
					new FarmerSprite.AnimationFrame(base.baseFrame + 2, 50),
					new FarmerSprite.AnimationFrame(base.baseFrame + 3, 50),
					new FarmerSprite.AnimationFrame(base.baseFrame + 4, 50),
					new FarmerSprite.AnimationFrame(base.baseFrame + 5, 120),
					new FarmerSprite.AnimationFrame(base.baseFrame + 6, 80),
					new FarmerSprite.AnimationFrame(base.baseFrame + 7, 50)
				});
				base.sprite.loop = true;
			}
			this.characterCheckTimer = 200;
		}
		if (this.running)
		{
			if (this.treeRunTimer > 0)
			{
				base.position.Y -= 4f;
			}
			else
			{
				base.position.X += (base.flip ? (-4) : 4);
			}
		}
		if (this.running && this.characterCheckTimer <= 0 && this.treeRunTimer <= 0)
		{
			this.characterCheckTimer = 100;
			Vector2 v = new Vector2((int)(base.position.X / 64f), (int)base.position.Y / 64);
			if (environment.terrainFeatures.TryGetValue(v, out var terrainFeature) && terrainFeature is Tree tree)
			{
				this.treeRunTimer = 700;
				this.climbed = tree;
				this.treeTile = v;
				base.position = v * 64f;
				return false;
			}
		}
		if (this.treeRunTimer > 0)
		{
			this.treeRunTimer -= time.ElapsedGameTime.Milliseconds;
			if (this.treeRunTimer <= 0)
			{
				this.climbed.performUseAction(this.treeTile);
				return true;
			}
		}
		return base.update(time, environment);
	}
}
