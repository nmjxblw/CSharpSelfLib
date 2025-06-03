using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewValley.Extensions;

namespace StardewValley.BellsAndWhistles;

public class Seagull : Critter
{
	public const int walkingSpeed = 2;

	public const int flyingSpeed = 4;

	public const int walking = 0;

	public const int flyingAway = 1;

	public const int flyingToLand = 4;

	public const int swimming = 2;

	public const int stopped = 3;

	private int state;

	private int characterCheckTimer = 200;

	private bool moveLeft;

	public Seagull(Vector2 position, int startingState)
		: base(0, position)
	{
		this.moveLeft = Game1.random.NextBool();
		base.startingPosition = position;
		this.state = startingState;
	}

	public void hop(Farmer who)
	{
		base.gravityAffectedDY = -4f;
	}

	public override bool update(GameTime time, GameLocation environment)
	{
		this.characterCheckTimer -= time.ElapsedGameTime.Milliseconds;
		if (this.characterCheckTimer < 0)
		{
			Character f = Utility.isThereAFarmerOrCharacterWithinDistance(base.position / 64f, 4, environment);
			this.characterCheckTimer = 200;
			if (f != null && this.state != 1)
			{
				if (Game1.random.NextDouble() < 0.25)
				{
					Game1.playSound("seagulls");
				}
				this.state = 1;
				if (f.Position.X > base.position.X)
				{
					this.moveLeft = true;
				}
				else
				{
					this.moveLeft = false;
				}
				base.sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
				{
					new FarmerSprite.AnimationFrame((short)(base.baseFrame + 10), 80),
					new FarmerSprite.AnimationFrame((short)(base.baseFrame + 11), 80),
					new FarmerSprite.AnimationFrame((short)(base.baseFrame + 12), 80),
					new FarmerSprite.AnimationFrame((short)(base.baseFrame + 13), 100)
				});
				base.sprite.loop = true;
			}
		}
		switch (this.state)
		{
		case 0:
		{
			int delta = (this.moveLeft ? (-2) : 2);
			if (!environment.isCollidingPosition(this.getBoundingBox(delta, 0), Game1.viewport, isFarmer: false, 0, glider: false, null, pathfinding: false, projectile: false, ignoreCharacterRequirement: true))
			{
				base.position.X += delta;
			}
			if (Game1.random.NextDouble() < 0.005)
			{
				this.state = 3;
				base.sprite.loop = false;
				base.sprite.CurrentAnimation = null;
				base.sprite.currentFrame = 0;
			}
			break;
		}
		case 2:
		{
			base.sprite.currentFrame = base.baseFrame + 9;
			float tmpY = base.yOffset;
			if ((time.TotalGameTime.TotalMilliseconds + (double)((int)base.position.X * 4)) % 2000.0 < 1000.0)
			{
				base.yOffset = 2f;
			}
			else
			{
				base.yOffset = 0f;
			}
			if (base.yOffset > tmpY)
			{
				environment.temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 0, 64, 64), 150f, 8, 0, new Vector2(base.position.X - 32f, base.position.Y - 32f), flicker: false, Game1.random.NextBool(), 0.001f, 0.01f, Color.White, 1f, 0.003f, 0f, 0f));
			}
			break;
		}
		case 1:
			if (this.moveLeft)
			{
				base.position.X -= 4f;
			}
			else
			{
				base.position.X += 4f;
			}
			base.yOffset -= 2f;
			break;
		case 3:
			if (Game1.random.NextDouble() < 0.003 && base.sprite.CurrentAnimation == null)
			{
				base.sprite.loop = false;
				switch (Game1.random.Next(4))
				{
				case 0:
				{
					List<FarmerSprite.AnimationFrame> frames = new List<FarmerSprite.AnimationFrame>
					{
						new FarmerSprite.AnimationFrame((short)(base.baseFrame + 2), 100),
						new FarmerSprite.AnimationFrame((short)(base.baseFrame + 3), 100),
						new FarmerSprite.AnimationFrame((short)(base.baseFrame + 4), 200),
						new FarmerSprite.AnimationFrame((short)(base.baseFrame + 5), 200)
					};
					int extra = Game1.random.Next(5);
					for (int j = 0; j < extra; j++)
					{
						frames.Add(new FarmerSprite.AnimationFrame((short)(base.baseFrame + 4), 200));
						frames.Add(new FarmerSprite.AnimationFrame((short)(base.baseFrame + 5), 200));
					}
					base.sprite.setCurrentAnimation(frames);
					break;
				}
				case 1:
					base.sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
					{
						new FarmerSprite.AnimationFrame(6, (short)Game1.random.Next(500, 4000))
					});
					break;
				case 2:
				{
					List<FarmerSprite.AnimationFrame> frames = new List<FarmerSprite.AnimationFrame>
					{
						new FarmerSprite.AnimationFrame((short)(base.baseFrame + 6), 500),
						new FarmerSprite.AnimationFrame((short)(base.baseFrame + 7), 100, secondaryArm: false, flip: false, hop),
						new FarmerSprite.AnimationFrame((short)(base.baseFrame + 8), 100)
					};
					int extra = Game1.random.Next(3);
					for (int i = 0; i < extra; i++)
					{
						frames.Add(new FarmerSprite.AnimationFrame((short)(base.baseFrame + 7), 100));
						frames.Add(new FarmerSprite.AnimationFrame((short)(base.baseFrame + 8), 100));
					}
					base.sprite.setCurrentAnimation(frames);
					break;
				}
				case 3:
					this.state = 0;
					base.sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
					{
						new FarmerSprite.AnimationFrame((short)base.baseFrame, 200),
						new FarmerSprite.AnimationFrame((short)(base.baseFrame + 1), 200)
					});
					base.sprite.loop = true;
					this.moveLeft = Game1.random.NextBool();
					if (Game1.random.NextDouble() < 0.33)
					{
						if (base.position.X > base.startingPosition.X)
						{
							this.moveLeft = true;
						}
						else
						{
							this.moveLeft = false;
						}
					}
					break;
				}
			}
			else if (base.sprite.CurrentAnimation == null)
			{
				base.sprite.currentFrame = base.baseFrame;
			}
			break;
		}
		base.flip = !this.moveLeft;
		return base.update(time, environment);
	}
}
