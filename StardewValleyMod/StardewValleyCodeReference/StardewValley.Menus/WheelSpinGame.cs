using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using StardewValley.Extensions;

namespace StardewValley.Menus;

public class WheelSpinGame : IClickableMenu
{
	public new const int width = 640;

	public new const int height = 448;

	public double arrowRotation;

	public double arrowRotationVelocity;

	public double arrowRotationDeceleration;

	private int timerBeforeStart;

	private int wager;

	private SparklingText resultText;

	private bool doneSpinning;

	public WheelSpinGame(int wager)
		: base(Game1.uiViewport.Width / 2 - 320, Game1.uiViewport.Height / 2 - 224, 640, 448)
	{
		this.timerBeforeStart = 1000;
		this.arrowRotationVelocity = Math.PI / 16.0;
		this.arrowRotationVelocity += (double)Game1.random.Next(0, 15) * Math.PI / 256.0;
		this.arrowRotationDeceleration = -0.0006283185307179586;
		if (Game1.random.NextBool())
		{
			this.arrowRotationVelocity += Math.PI / 64.0;
		}
		this.wager = wager;
		Game1.player.Halt();
	}

	/// <inheritdoc />
	public override void receiveLeftClick(int x, int y, bool playSound = true)
	{
	}

	/// <inheritdoc />
	public override void update(GameTime time)
	{
		base.update(time);
		if (this.timerBeforeStart <= 0)
		{
			double oldVelocity = this.arrowRotationVelocity;
			this.arrowRotationVelocity += this.arrowRotationDeceleration;
			if (this.arrowRotationVelocity <= Math.PI / 80.0 && oldVelocity > Math.PI / 80.0)
			{
				bool colorChoiceGreen = Game1.currentLocation.currentEvent.specialEventVariable2;
				if (this.arrowRotation > Math.PI / 2.0 && this.arrowRotation <= 4.319689898685965 && Game1.random.NextDouble() < (double)((float)Game1.player.LuckLevel / 15f))
				{
					if (colorChoiceGreen)
					{
						this.arrowRotationVelocity = Math.PI / 48.0;
						Game1.playSound("dwop");
					}
				}
				else if ((this.arrowRotation + Math.PI) % (Math.PI * 2.0) <= 4.319689898685965 && !colorChoiceGreen && Game1.random.NextDouble() < (double)((float)Game1.player.LuckLevel / 20f))
				{
					this.arrowRotationVelocity = Math.PI / 48.0;
					Game1.playSound("dwop");
				}
			}
			if (this.arrowRotationVelocity <= 0.0 && !this.doneSpinning)
			{
				this.doneSpinning = true;
				this.arrowRotationDeceleration = 0.0;
				this.arrowRotationVelocity = 0.0;
				bool colorChoiceGreen2 = Game1.currentLocation.currentEvent.specialEventVariable2;
				bool won = false;
				if (this.arrowRotation > Math.PI / 2.0 && this.arrowRotation <= 4.71238898038469)
				{
					if (!colorChoiceGreen2)
					{
						won = true;
					}
				}
				else if (colorChoiceGreen2)
				{
					won = true;
				}
				if (won)
				{
					Game1.playSound("reward");
					this.resultText = new SparklingText(Game1.dialogueFont, Game1.content.LoadString("Strings\\StringsFromCSFiles:WheelSpinGame.cs.11829"), Color.Lime, Color.White);
					Game1.player.festivalScore += this.wager;
				}
				else
				{
					this.resultText = new SparklingText(Game1.dialogueFont, Game1.content.LoadString("Strings\\StringsFromCSFiles:WheelSpinGame.cs.11830"), Color.Red, Color.Transparent);
					Game1.playSound("fishEscape");
					Game1.player.festivalScore -= this.wager;
				}
			}
			double num = this.arrowRotation;
			this.arrowRotation += this.arrowRotationVelocity;
			if (num % (Math.PI / 2.0) > this.arrowRotation % (Math.PI / 2.0))
			{
				Game1.playSound("Cowboy_gunshot");
			}
			this.arrowRotation %= Math.PI * 2.0;
		}
		else
		{
			this.timerBeforeStart -= time.ElapsedGameTime.Milliseconds;
			if (this.timerBeforeStart <= 0)
			{
				Game1.playSound("cowboy_monsterhit");
			}
		}
		if (this.resultText != null && this.resultText.update(time))
		{
			this.resultText = null;
		}
		if (this.doneSpinning && this.resultText == null)
		{
			Game1.exitActiveMenu();
			Game1.player.canMove = true;
		}
	}

	/// <inheritdoc />
	public override void performHoverAction(int x, int y)
	{
	}

	/// <inheritdoc />
	public override void receiveKeyPress(Keys key)
	{
	}

	/// <inheritdoc />
	public override void draw(SpriteBatch b)
	{
		b.Draw(Game1.fadeToBlackRect, new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), Color.Black * 0.5f);
		b.Draw(Game1.mouseCursors, new Vector2(base.xPositionOnScreen, base.yPositionOnScreen), new Rectangle(128, 1184, 160, 112), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.95f);
		b.Draw(Game1.mouseCursors, new Vector2(base.xPositionOnScreen + 320, base.yPositionOnScreen + 224 + 4), new Rectangle(120, 1234, 8, 16), Color.White, (float)this.arrowRotation, new Vector2(4f, 15f), 4f, SpriteEffects.None, 0.96f);
		this.resultText?.draw(b, new Vector2((float)(base.xPositionOnScreen + 320) - this.resultText.textWidth, base.yPositionOnScreen - 64));
	}
}
