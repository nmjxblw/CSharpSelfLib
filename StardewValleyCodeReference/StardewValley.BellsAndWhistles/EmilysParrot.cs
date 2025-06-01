using System;
using Microsoft.Xna.Framework;
using StardewValley.Extensions;

namespace StardewValley.BellsAndWhistles;

public class EmilysParrot : TemporaryAnimatedSprite
{
	public const int flappingPhase = 1;

	public const int hoppingPhase = 0;

	public const int lookingSidewaysPhase = 2;

	public const int nappingPhase = 3;

	public const int headBobbingPhase = 4;

	private int currentFrame;

	private int currentFrameTimer;

	private int currentPhaseTimer;

	private int currentPhase;

	private int shakeTimer;

	public EmilysParrot(Vector2 location)
	{
		base.texture = Game1.mouseCursors;
		base.sourceRect = new Rectangle(92, 148, 9, 16);
		base.sourceRectStartingPos = new Vector2(92f, 149f);
		base.position = location;
		base.initialPosition = base.position;
		base.scale = 4f;
		base.id = 5858585;
	}

	public void doAction()
	{
		Game1.playSound("parrot");
		this.shakeTimer = 800;
	}

	public override bool update(GameTime time)
	{
		this.currentPhaseTimer -= time.ElapsedGameTime.Milliseconds;
		if (this.currentPhaseTimer <= 0)
		{
			this.currentPhase = Game1.random.Next(5);
			this.currentPhaseTimer = Game1.random.Next(4000, 16000);
			if (this.currentPhase == 1)
			{
				this.currentPhaseTimer /= 2;
				this.updateFlappingPhase();
			}
			else
			{
				base.position = base.initialPosition;
			}
		}
		if (this.shakeTimer > 0)
		{
			base.shakeIntensity = 1f;
			this.shakeTimer -= time.ElapsedGameTime.Milliseconds;
		}
		else
		{
			base.shakeIntensity = 0f;
		}
		this.currentFrameTimer -= time.ElapsedGameTime.Milliseconds;
		if (this.currentFrameTimer <= 0)
		{
			switch (this.currentPhase)
			{
			case 0:
				if (this.currentFrame == 7)
				{
					this.currentFrame = 0;
					this.currentFrameTimer = 600;
				}
				else if (Game1.random.NextBool())
				{
					this.currentFrame = 7;
					this.currentFrameTimer = 300;
				}
				break;
			case 4:
				if (this.currentFrame == 1 && Game1.random.NextDouble() < 0.1)
				{
					this.currentFrame = 2;
				}
				else if (this.currentFrame == 2)
				{
					this.currentFrame = 1;
				}
				else
				{
					this.currentFrame = Game1.random.Next(2);
				}
				this.currentFrameTimer = 500;
				break;
			case 3:
				if (this.currentFrame == 5)
				{
					this.currentFrame = 6;
				}
				else
				{
					this.currentFrame = 5;
				}
				this.currentFrameTimer = 1000;
				break;
			case 2:
				this.currentFrame = Game1.random.Next(3, 5);
				this.currentFrameTimer = 1000;
				break;
			case 1:
				this.updateFlappingPhase();
				this.currentFrameTimer = 0;
				break;
			}
		}
		if (this.currentPhase == 1 && this.currentFrame != 0)
		{
			base.sourceRect.X = 38 + this.currentFrame * 13;
			base.sourceRect.Width = 13;
		}
		else
		{
			base.sourceRect.X = 92 + this.currentFrame * 9;
			base.sourceRect.Width = 9;
		}
		return false;
	}

	private void updateFlappingPhase()
	{
		this.currentFrame = 6 - this.currentPhaseTimer % 1000 / 166;
		this.currentFrame = 3 - Math.Abs(this.currentFrame - 3);
		base.position.Y = base.initialPosition.Y - (float)(4 * (3 - this.currentFrame));
		if (this.currentFrame == 0)
		{
			base.position.X = base.initialPosition.X;
		}
		else
		{
			base.position.X = base.initialPosition.X - 8f;
		}
	}
}
