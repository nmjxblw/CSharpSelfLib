using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley.BellsAndWhistles;

public class Frog : Critter
{
	private bool waterLeaper;

	private bool leapingIntoWater;

	private bool splash;

	private int characterCheckTimer = 200;

	private int beforeFadeTimer;

	private float alpha = 1f;

	public Frog(Vector2 position, bool waterLeaper = false, bool forceFlip = false)
	{
		this.waterLeaper = waterLeaper;
		base.position = position * 64f;
		base.sprite = new AnimatedSprite(Critter.critterTexture, waterLeaper ? 300 : 280, 16, 16);
		base.sprite.loop = true;
		if (!base.flip && forceFlip)
		{
			base.flip = true;
		}
		if (waterLeaper)
		{
			base.sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
			{
				new FarmerSprite.AnimationFrame(300, 600),
				new FarmerSprite.AnimationFrame(304, 100),
				new FarmerSprite.AnimationFrame(305, 100),
				new FarmerSprite.AnimationFrame(306, 300),
				new FarmerSprite.AnimationFrame(305, 100),
				new FarmerSprite.AnimationFrame(304, 100)
			});
		}
		else
		{
			base.sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
			{
				new FarmerSprite.AnimationFrame(280, 60),
				new FarmerSprite.AnimationFrame(281, 70),
				new FarmerSprite.AnimationFrame(282, 140),
				new FarmerSprite.AnimationFrame(283, 90)
			});
			this.beforeFadeTimer = 1000;
			base.flip = base.position.X + 4f < Game1.player.Position.X;
		}
		base.startingPosition = position;
	}

	public void startSplash(Farmer who)
	{
		this.splash = true;
	}

	public override bool update(GameTime time, GameLocation environment)
	{
		if (this.waterLeaper)
		{
			if (!this.leapingIntoWater)
			{
				this.characterCheckTimer -= time.ElapsedGameTime.Milliseconds;
				if (this.characterCheckTimer <= 0)
				{
					if (Utility.isThereAFarmerOrCharacterWithinDistance(base.position / 64f, 6, environment) != null)
					{
						this.leapingIntoWater = true;
						base.sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
						{
							new FarmerSprite.AnimationFrame(300, 100),
							new FarmerSprite.AnimationFrame(301, 100),
							new FarmerSprite.AnimationFrame(302, 100),
							new FarmerSprite.AnimationFrame(303, 1500, secondaryArm: false, flip: false, startSplash, behaviorAtEndOfFrame: true)
						});
						base.sprite.loop = false;
						base.sprite.oldFrame = 303;
						base.gravityAffectedDY = -6f;
					}
					else if (Game1.random.NextDouble() < 0.01)
					{
						Game1.playSound("croak");
					}
					this.characterCheckTimer = 200;
				}
			}
			else
			{
				base.position.X += (base.flip ? (-4) : 4);
				if (base.gravityAffectedDY >= 0f && base.yJumpOffset >= 0f)
				{
					base.sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
					{
						new FarmerSprite.AnimationFrame(300, 100),
						new FarmerSprite.AnimationFrame(301, 100),
						new FarmerSprite.AnimationFrame(302, 100),
						new FarmerSprite.AnimationFrame(303, 1500, secondaryArm: false, flip: false, startSplash, behaviorAtEndOfFrame: true)
					});
					base.sprite.loop = false;
					base.sprite.oldFrame = 303;
					base.gravityAffectedDY = -6f;
					base.yJumpOffset = 0f;
					if (environment.isWaterTile((int)base.position.X / 64, (int)base.position.Y / 64))
					{
						this.splash = true;
					}
				}
			}
		}
		else
		{
			base.position.X += (base.flip ? (-3) : 3);
			this.beforeFadeTimer -= time.ElapsedGameTime.Milliseconds;
			if (this.beforeFadeTimer <= 0)
			{
				this.alpha -= 0.001f * (float)time.ElapsedGameTime.Milliseconds;
				if (this.alpha <= 0f)
				{
					return true;
				}
			}
			if (environment.isWaterTile((int)base.position.X / 64, (int)base.position.Y / 64))
			{
				this.splash = true;
			}
		}
		if (this.splash)
		{
			environment.TemporarySprites.Add(new TemporaryAnimatedSprite(28, 50f, 2, 1, base.position, flicker: false, flipped: false));
			Game1.playSound("dropItemInWater");
			return true;
		}
		return base.update(time, environment);
	}

	public override void draw(SpriteBatch b)
	{
		base.sprite.draw(b, Game1.GlobalToLocal(Game1.viewport, Utility.snapDrawPosition(base.position + new Vector2(0f, -20f + base.yJumpOffset + base.yOffset))), (base.position.Y + 64f) / 10000f, 0, 0, Color.White * this.alpha, base.flip, 4f);
		b.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, base.position + new Vector2(32f, 40f)), Game1.shadowTexture.Bounds, Color.White * this.alpha, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 3f + Math.Max(-3f, (base.yJumpOffset + base.yOffset) / 16f), SpriteEffects.None, (base.position.Y - 1f) / 10000f);
	}

	public override void drawAboveFrontLayer(SpriteBatch b)
	{
	}
}
