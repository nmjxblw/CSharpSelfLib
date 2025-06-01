using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace StardewValley.Minigames;

public class FantasyBoardGame : IMinigame
{
	public int borderSourceWidth = 138;

	public int borderSourceHeight = 74;

	public int slideSourceWidth = 128;

	public int slideSourceHeight = 64;

	private LocalizedContentManager content;

	private Texture2D slides;

	private Texture2D border;

	public int whichSlide;

	public int shakeTimer;

	public int endTimer;

	private string grade = "";

	public FantasyBoardGame()
	{
		this.content = Game1.content.CreateTemporary();
		this.slides = this.content.Load<Texture2D>("LooseSprites\\boardGame");
		this.border = this.content.Load<Texture2D>("LooseSprites\\boardGameBorder");
		Game1.globalFadeToClear();
	}

	public bool overrideFreeMouseMovement()
	{
		return Game1.options.SnappyMenus;
	}

	public bool tick(GameTime time)
	{
		if (this.shakeTimer > 0)
		{
			this.shakeTimer -= time.ElapsedGameTime.Milliseconds;
		}
		Game1.currentLocation.currentEvent.Update(Game1.currentLocation, time);
		if (Game1.activeClickableMenu != null)
		{
			Game1.PushUIMode();
			Game1.activeClickableMenu.update(time);
			Game1.PopUIMode();
		}
		if (this.endTimer > 0)
		{
			this.endTimer -= time.ElapsedGameTime.Milliseconds;
			if (this.endTimer <= 0 && this.whichSlide == -1)
			{
				Game1.globalFadeToBlack(end);
			}
		}
		if (Game1.activeClickableMenu != null)
		{
			Game1.PushUIMode();
			Game1.activeClickableMenu.performHoverAction(Game1.getOldMouseX(), Game1.getOldMouseY());
			Game1.PopUIMode();
		}
		return false;
	}

	public void end()
	{
		this.unload();
		Game1.currentLocation.currentEvent.CurrentCommand++;
		Game1.currentMinigame = null;
	}

	public void receiveLeftClick(int x, int y, bool playSound = true)
	{
		if (Game1.activeClickableMenu != null)
		{
			Game1.PushUIMode();
			Game1.activeClickableMenu.receiveLeftClick(x, y);
			Game1.PopUIMode();
		}
	}

	public void leftClickHeld(int x, int y)
	{
	}

	public void receiveRightClick(int x, int y, bool playSound = true)
	{
		Game1.pressActionButton(Game1.GetKeyboardState(), Game1.input.GetMouseState(), Game1.input.GetGamePadState());
		if (Game1.activeClickableMenu != null)
		{
			Game1.PushUIMode();
			Game1.activeClickableMenu.receiveRightClick(x, y);
			Game1.PopUIMode();
		}
	}

	public void releaseLeftClick(int x, int y)
	{
	}

	public void releaseRightClick(int x, int y)
	{
	}

	public void receiveKeyPress(Keys k)
	{
		if (Game1.isQuestion)
		{
			if (Game1.options.doesInputListContain(Game1.options.moveUpButton, k))
			{
				Game1.currentQuestionChoice = Math.Max(Game1.currentQuestionChoice - 1, 0);
				Game1.playSound("toolSwap");
			}
			else if (Game1.options.doesInputListContain(Game1.options.moveDownButton, k))
			{
				Game1.currentQuestionChoice = Math.Min(Game1.currentQuestionChoice + 1, Game1.questionChoices.Count - 1);
				Game1.playSound("toolSwap");
			}
		}
		else if (Game1.activeClickableMenu != null)
		{
			Game1.PushUIMode();
			Game1.activeClickableMenu.receiveKeyPress(k);
			Game1.PopUIMode();
		}
	}

	public void receiveKeyRelease(Keys k)
	{
	}

	public void draw(SpriteBatch b)
	{
		b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
		if (this.whichSlide >= 0)
		{
			Vector2 offset = default(Vector2);
			if (this.shakeTimer > 0)
			{
				offset = new Vector2(Game1.random.Next(-2, 2), Game1.random.Next(-2, 2));
			}
			b.Draw(this.border, offset + new Vector2(Game1.viewport.Width / 2 - this.borderSourceWidth * 4 / 2, Game1.viewport.Height / 2 - this.borderSourceHeight * 4 / 2 - 128), new Rectangle(0, 0, this.borderSourceWidth, this.borderSourceHeight), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0f);
			b.Draw(this.slides, offset + new Vector2(Game1.viewport.Width / 2 - this.slideSourceWidth * 4 / 2, Game1.viewport.Height / 2 - this.slideSourceHeight * 4 / 2 - 128), new Rectangle(this.whichSlide % 2 * this.slideSourceWidth, this.whichSlide / 2 * this.slideSourceHeight, this.slideSourceWidth, this.slideSourceHeight), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.01f);
		}
		else
		{
			string s = Game1.content.LoadString("Strings\\StringsFromCSFiles:FantasyBoardGame.cs.11980", this.grade);
			float yOffset = (float)Math.Sin(this.endTimer / 1000) * 8f;
			Game1.drawWithBorder(s, Game1.textColor, Color.Purple, new Vector2((float)(Game1.viewport.Width / 2) - Game1.dialogueFont.MeasureString(s).X / 2f, yOffset + (float)(Game1.viewport.Height / 2)));
		}
		b.End();
		if (Game1.activeClickableMenu != null)
		{
			Game1.PushUIMode();
			b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
			Game1.activeClickableMenu.draw(b);
			b.End();
			Game1.PopUIMode();
		}
	}

	public void changeScreenSize()
	{
	}

	public void unload()
	{
		this.content.Unload();
	}

	public void afterFade()
	{
		this.whichSlide = -1;
		int score = 0;
		if (Game1.player.mailReceived.Contains("savedFriends"))
		{
			score++;
		}
		if (Game1.player.mailReceived.Contains("destroyedPods"))
		{
			score++;
		}
		if (Game1.player.mailReceived.Contains("killedSkeleton"))
		{
			score++;
		}
		switch (score)
		{
		case 0:
			this.grade = "D";
			break;
		case 1:
			this.grade = "C";
			break;
		case 2:
			this.grade = "B";
			break;
		case 3:
			this.grade = "A";
			break;
		}
		Game1.playSound("newArtifact");
		this.endTimer = 5500;
	}

	public void receiveEventPoke(int data)
	{
		switch (data)
		{
		case -1:
			this.shakeTimer = 1000;
			break;
		case -2:
			Game1.globalFadeToBlack(afterFade);
			break;
		default:
			this.whichSlide = data;
			break;
		}
	}

	public string minigameId()
	{
		return "FantasyBoardGame";
	}

	public bool doMainGameUpdates()
	{
		return false;
	}

	public bool forceQuit()
	{
		return false;
	}
}
