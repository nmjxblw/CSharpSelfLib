using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewValley.Extensions;

namespace StardewValley.BellsAndWhistles;

public class Crow : Critter
{
	public const int flyingSpeed = 6;

	public const int pecking = 0;

	public const int flyingAway = 1;

	public const int sleeping = 2;

	public const int stopped = 3;

	private int state;

	public Crow(int tileX, int tileY)
		: base(14, new Vector2(tileX * 64, tileY * 64))
	{
		base.flip = Game1.random.NextBool();
		base.position.X += 32f;
		base.position.Y += 32f;
		base.startingPosition = base.position;
		this.state = 0;
	}

	public void hop(Farmer who)
	{
		base.gravityAffectedDY = -4f;
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
		Farmer f = Utility.isThereAFarmerWithinDistance(base.position / 64f, 4, environment);
		if (base.yJumpOffset < 0f && this.state != 1)
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
		if (f != null && this.state != 1)
		{
			if (Game1.random.NextDouble() < 0.85)
			{
				Game1.playSound("crow");
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
				new FarmerSprite.AnimationFrame((short)(base.baseFrame + 6), 40),
				new FarmerSprite.AnimationFrame((short)(base.baseFrame + 7), 40),
				new FarmerSprite.AnimationFrame((short)(base.baseFrame + 8), 40),
				new FarmerSprite.AnimationFrame((short)(base.baseFrame + 9), 40),
				new FarmerSprite.AnimationFrame((short)(base.baseFrame + 10), 40, secondaryArm: false, base.flip, playFlap),
				new FarmerSprite.AnimationFrame((short)(base.baseFrame + 7), 40),
				new FarmerSprite.AnimationFrame((short)(base.baseFrame + 9), 40),
				new FarmerSprite.AnimationFrame((short)(base.baseFrame + 8), 40),
				new FarmerSprite.AnimationFrame((short)(base.baseFrame + 7), 40)
			});
			base.sprite.loop = true;
		}
		switch (this.state)
		{
		case 0:
			if (base.sprite.CurrentAnimation == null)
			{
				List<FarmerSprite.AnimationFrame> peckAnim = new List<FarmerSprite.AnimationFrame>
				{
					new FarmerSprite.AnimationFrame((short)base.baseFrame, 480),
					new FarmerSprite.AnimationFrame((short)(base.baseFrame + 1), 170, secondaryArm: false, base.flip),
					new FarmerSprite.AnimationFrame((short)(base.baseFrame + 2), 170, secondaryArm: false, base.flip)
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
			base.yOffset -= 2f;
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
		case 3:
			if (Game1.random.NextDouble() < 0.008 && base.sprite.CurrentAnimation == null && base.yJumpOffset >= 0f)
			{
				switch (Game1.random.Next(5))
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
					this.state = 1;
					base.sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
					{
						new FarmerSprite.AnimationFrame((short)(base.baseFrame + 6), 50),
						new FarmerSprite.AnimationFrame((short)(base.baseFrame + 7), 50),
						new FarmerSprite.AnimationFrame((short)(base.baseFrame + 8), 50),
						new FarmerSprite.AnimationFrame((short)(base.baseFrame + 9), 50),
						new FarmerSprite.AnimationFrame((short)(base.baseFrame + 10), 50, secondaryArm: false, base.flip, playFlap),
						new FarmerSprite.AnimationFrame((short)(base.baseFrame + 7), 50),
						new FarmerSprite.AnimationFrame((short)(base.baseFrame + 9), 50),
						new FarmerSprite.AnimationFrame((short)(base.baseFrame + 8), 50),
						new FarmerSprite.AnimationFrame((short)(base.baseFrame + 7), 50)
					});
					base.sprite.loop = true;
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
