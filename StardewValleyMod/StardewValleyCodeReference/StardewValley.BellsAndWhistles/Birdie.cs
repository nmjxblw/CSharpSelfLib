using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Extensions;

namespace StardewValley.BellsAndWhistles;

public class Birdie : Critter
{
	public const int brownBird = 25;

	public const int blueBird = 45;

	public const int flyingSpeed = 6;

	public const int walkingSpeed = 1;

	public const int pecking = 0;

	public const int flyingAway = 1;

	public const int sleeping = 2;

	public const int stopped = 3;

	public const int walking = 4;

	private int state;

	private float flightOffset;

	private bool stationary;

	private int characterCheckTimer = 200;

	private int walkTimer;

	public Birdie(int tileX, int tileY, int startingIndex = 25)
		: base(startingIndex, new Vector2(tileX * 64, tileY * 64))
	{
		base.flip = Game1.random.NextBool();
		base.position.X += 32f;
		base.position.Y += 32f;
		base.startingPosition = base.position;
		this.flightOffset = (float)Game1.random.NextDouble() - 0.5f;
		this.state = 0;
	}

	public Birdie(Vector2 position, float yOffset, int startingIndex = 25, bool stationary = false)
		: base(startingIndex, position)
	{
		base.yOffset = yOffset;
		base.flip = Game1.random.NextBool();
		base.startingPosition = position;
		this.stationary = stationary;
		this.state = Game1.random.Next(2, 5);
		this.flightOffset = (float)Game1.random.NextDouble() - 0.5f;
	}

	public void hop(Farmer who)
	{
		base.gravityAffectedDY = -2f;
	}

	public override void drawAboveFrontLayer(SpriteBatch b)
	{
		if (this.state == 1)
		{
			base.draw(b);
		}
	}

	public override void draw(SpriteBatch b)
	{
		if (this.state != 1)
		{
			base.draw(b);
		}
	}

	private void donePecking(Farmer who)
	{
		this.state = Game1.random.Choose(0, 3);
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
			Game1.playSound("shiny4");
		}
	}

	public override bool update(GameTime time, GameLocation environment)
	{
		if (base.yJumpOffset < 0f && this.state != 1 && !this.stationary)
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
		this.characterCheckTimer -= time.ElapsedGameTime.Milliseconds;
		if (this.characterCheckTimer < 0)
		{
			Character f = Utility.isThereAFarmerOrCharacterWithinDistance(base.position / 64f, 4, environment);
			this.characterCheckTimer = 200;
			if (f != null && this.state != 1)
			{
				if (Game1.random.NextDouble() < 0.85)
				{
					Game1.playSound("SpringBirds");
				}
				this.state = 1;
				if (f.Position.X > base.position.X)
				{
					base.flip = false;
				}
				else
				{
					base.flip = true;
				}
				base.sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
				{
					new FarmerSprite.AnimationFrame((short)(base.baseFrame + 6), 70),
					new FarmerSprite.AnimationFrame((short)(base.baseFrame + 7), 60, secondaryArm: false, base.flip, playFlap),
					new FarmerSprite.AnimationFrame((short)(base.baseFrame + 8), 70),
					new FarmerSprite.AnimationFrame((short)(base.baseFrame + 7), 60)
				});
				base.sprite.loop = true;
			}
		}
		switch (this.state)
		{
		case 0:
			if (base.sprite.CurrentAnimation == null)
			{
				List<FarmerSprite.AnimationFrame> peckAnim = new List<FarmerSprite.AnimationFrame>
				{
					new FarmerSprite.AnimationFrame((short)(base.baseFrame + 2), 480),
					new FarmerSprite.AnimationFrame((short)(base.baseFrame + 3), 170, secondaryArm: false, base.flip),
					new FarmerSprite.AnimationFrame((short)(base.baseFrame + 4), 170, secondaryArm: false, base.flip)
				};
				int pecks = Game1.random.Next(1, 5);
				for (int i = 0; i < pecks; i++)
				{
					peckAnim.Add(new FarmerSprite.AnimationFrame((short)(base.baseFrame + 3), 70));
					peckAnim.Add(new FarmerSprite.AnimationFrame((short)(base.baseFrame + 4), 100, secondaryArm: false, base.flip, playPeck));
				}
				peckAnim.Add(new FarmerSprite.AnimationFrame((short)(base.baseFrame + 3), 100));
				peckAnim.Add(new FarmerSprite.AnimationFrame((short)(base.baseFrame + 2), 70, secondaryArm: false, base.flip));
				peckAnim.Add(new FarmerSprite.AnimationFrame((short)(base.baseFrame + 1), 70, secondaryArm: false, base.flip));
				peckAnim.Add(new FarmerSprite.AnimationFrame((short)base.baseFrame, 500, secondaryArm: false, base.flip, donePecking));
				base.sprite.loop = false;
				base.sprite.setCurrentAnimation(peckAnim);
			}
			break;
		case 1:
			if (!base.flip)
			{
				base.position.X -= 6f;
			}
			else
			{
				base.position.X += 6f;
			}
			base.yOffset -= 2f + this.flightOffset;
			break;
		case 2:
			if (base.sprite.CurrentAnimation == null)
			{
				base.sprite.currentFrame = base.baseFrame + 5;
			}
			if (Game1.random.NextDouble() < 0.003 && base.sprite.CurrentAnimation == null)
			{
				this.state = 3;
			}
			break;
		case 4:
			if (!this.stationary)
			{
				int delta = (base.flip ? 1 : (-1));
				if (!environment.isCollidingPosition(this.getBoundingBox(delta, 0), Game1.viewport, isFarmer: false, 0, glider: false, null, pathfinding: false, projectile: false, ignoreCharacterRequirement: true))
				{
					base.position.X += delta;
				}
			}
			else
			{
				float delta2 = (base.flip ? 0.5f : (-0.5f));
				if (Math.Abs(base.position.X + delta2 - base.startingPosition.X) < 8f)
				{
					base.position.X += delta2;
				}
				else
				{
					base.flip = !base.flip;
				}
			}
			this.walkTimer -= time.ElapsedGameTime.Milliseconds;
			if (this.walkTimer < 0)
			{
				this.state = 3;
				base.sprite.loop = false;
				base.sprite.CurrentAnimation = null;
				base.sprite.currentFrame = base.baseFrame;
			}
			break;
		case 3:
			if (Game1.random.NextDouble() < 0.008 && base.sprite.CurrentAnimation == null && base.yJumpOffset >= 0f)
			{
				switch (Game1.random.Next(6))
				{
				case 0:
					this.state = 2;
					break;
				case 1:
					this.state = 0;
					break;
				case 2:
					this.hop(null);
					break;
				case 3:
					base.flip = !base.flip;
					this.hop(null);
					break;
				case 4:
				case 5:
					this.state = 4;
					base.sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
					{
						new FarmerSprite.AnimationFrame((short)base.baseFrame, 100),
						new FarmerSprite.AnimationFrame((short)(base.baseFrame + 1), 100)
					});
					base.sprite.loop = true;
					if (base.position.X >= base.startingPosition.X)
					{
						base.flip = false;
					}
					else
					{
						base.flip = true;
					}
					this.walkTimer = Game1.random.Next(5, 15) * 100;
					break;
				}
			}
			else if (base.sprite.CurrentAnimation == null)
			{
				base.sprite.currentFrame = base.baseFrame;
			}
			break;
		}
		return base.update(time, environment);
	}
}
