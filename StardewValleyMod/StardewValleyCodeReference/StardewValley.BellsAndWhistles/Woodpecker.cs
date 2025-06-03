using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.TerrainFeatures;

namespace StardewValley.BellsAndWhistles;

public class Woodpecker : Critter
{
	public const int flyingSpeed = 6;

	private bool flyingAway;

	private Tree tree;

	private int peckTimer;

	private int characterCheckTimer = 200;

	public Woodpecker(Tree tree, Vector2 position)
	{
		this.tree = tree;
		position *= 64f;
		base.position = position;
		base.position.X += 32f;
		base.position.Y += 0f;
		base.startingPosition = position;
		base.baseFrame = 320;
		base.sprite = new AnimatedSprite(Critter.critterTexture, 320, 16, 16);
	}

	public override void drawAboveFrontLayer(SpriteBatch b)
	{
		base.sprite.draw(b, Game1.GlobalToLocal(Game1.viewport, base.position + new Vector2(-80f, -64f + base.yJumpOffset + base.yOffset)), 1f, 0, 0, Color.White, base.flip, 4f);
	}

	public override void draw(SpriteBatch b)
	{
		b.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, base.position + new Vector2(0f, -4f)), Game1.shadowTexture.Bounds, Color.White, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 3f + Math.Max(-3f, (base.yJumpOffset + base.yOffset) / 16f), SpriteEffects.None, (base.position.Y - 1f) / 10000f);
	}

	private void donePecking(Farmer who)
	{
		this.peckTimer = Game1.random.Next(1000, 3000);
	}

	private void playFlap(Farmer who)
	{
		if (Utility.isOnScreen(base.position, 64))
		{
			Game1.playSound("batFlap");
		}
	}

	private void playPeck(Farmer who)
	{
		if (Utility.isOnScreen(base.position, 64))
		{
			Game1.playSound("Cowboy_gunshot");
		}
	}

	public override bool update(GameTime time, GameLocation environment)
	{
		if (environment == null || base.sprite == null || this.tree == null)
		{
			return true;
		}
		if (base.yJumpOffset < 0f && !this.flyingAway)
		{
			if (!base.flip && !environment.isCollidingPosition(this.getBoundingBox(-2, 0), Game1.viewport, isFarmer: false, 0, glider: false, null, pathfinding: false, projectile: false, ignoreCharacterRequirement: true))
			{
				base.position.X -= 2f;
			}
			else if (!environment.isCollidingPosition(this.getBoundingBox(2, 0), Game1.viewport, isFarmer: false, 0, glider: false, null, pathfinding: false, projectile: false, ignoreCharacterRequirement: true))
			{
				base.position.X += 2f;
			}
		}
		this.peckTimer -= time.ElapsedGameTime.Milliseconds;
		if (!this.flyingAway && this.peckTimer <= 0 && base.sprite.CurrentAnimation == null)
		{
			int nibbles = Game1.random.Next(2, 8);
			List<FarmerSprite.AnimationFrame> anim = new List<FarmerSprite.AnimationFrame>();
			for (int i = 0; i < nibbles; i++)
			{
				anim.Add(new FarmerSprite.AnimationFrame(base.baseFrame, 100));
				anim.Add(new FarmerSprite.AnimationFrame(base.baseFrame + 1, 100, secondaryArm: false, flip: false, playPeck));
			}
			anim.Add(new FarmerSprite.AnimationFrame(base.baseFrame, 200, secondaryArm: false, flip: false, donePecking));
			base.sprite.setCurrentAnimation(anim);
			base.sprite.loop = false;
		}
		this.characterCheckTimer -= time.ElapsedGameTime.Milliseconds;
		if (this.characterCheckTimer < 0)
		{
			Character f = Utility.isThereAFarmerOrCharacterWithinDistance(base.position / 64f, 6, environment);
			this.characterCheckTimer = 200;
			if ((f != null || this.tree.stump.Value) && !this.flyingAway)
			{
				this.flyingAway = true;
				if (f != null && f.Position.X > base.position.X)
				{
					base.flip = false;
				}
				else
				{
					base.flip = true;
				}
				base.sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
				{
					new FarmerSprite.AnimationFrame((short)(base.baseFrame + 2), 70),
					new FarmerSprite.AnimationFrame((short)(base.baseFrame + 3), 60, secondaryArm: false, base.flip, playFlap),
					new FarmerSprite.AnimationFrame((short)(base.baseFrame + 4), 70),
					new FarmerSprite.AnimationFrame((short)(base.baseFrame + 3), 60)
				});
				base.sprite.loop = true;
			}
		}
		if (this.flyingAway)
		{
			if (!base.flip)
			{
				base.position.X -= 6f;
			}
			else
			{
				base.position.X += 6f;
			}
			base.yOffset -= 1f;
		}
		return base.update(time, environment);
	}
}
