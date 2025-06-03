using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using StardewValley.GameData;
using StardewValley.Locations;

namespace StardewValley.Minigames;

public class Darts : IMinigame
{
	public enum GameState
	{
		Aiming,
		Charging,
		Firing,
		ShowScore,
		Scoring,
		GameOver
	}

	public GameState currentGameState;

	public float stateTimer;

	public float pixelScale = 4f;

	public bool gamePaused;

	public Vector2 upperLeft;

	private int screenWidth;

	private int screenHeight;

	private Texture2D texture;

	public Vector2 cursorPosition = new Vector2(0f, 0f);

	public Vector2 aimPosition = new Vector2(0f, 0f);

	public Vector2 dartBoardCenter = Vector2.Zero;

	protected bool canCancelShot = true;

	public float chargeTime;

	public float chargeDirection = 1f;

	public float hangTime;

	public int previousPoints;

	public int points;

	public float nextPointTransferTime;

	public static ICue chargeSound;

	public Vector2 throwStartPosition;

	public Vector2 dartPosition;

	public float dartTime = -1f;

	public string lastHitString = "";

	public int lastHitAmount;

	public bool shakeScore;

	public int startingDartCount = 20;

	public int dartCount = 20;

	public int throwsCount;

	public string alternateTextString = "";

	public string gameOverString = "";

	public bool lastHitWasDouble;

	public bool overrideFreeMouseMovement()
	{
		return false;
	}

	public Darts(int dart_count = 20)
	{
		this.startingDartCount = (this.dartCount = dart_count);
		this.changeScreenSize();
		this.texture = Game1.content.Load<Texture2D>("Minigames\\Darts");
		this.points = 301;
		this.SetGameState(GameState.Aiming);
	}

	public virtual void SetGameState(GameState new_state)
	{
		switch (this.currentGameState)
		{
		case GameState.Scoring:
			this.previousPoints = this.points;
			this.shakeScore = false;
			this.alternateTextString = "";
			break;
		case GameState.Charging:
			if (Darts.chargeSound != null)
			{
				Darts.chargeSound.Stop(AudioStopOptions.Immediate);
				Darts.chargeSound = null;
			}
			break;
		}
		this.currentGameState = new_state;
		switch (this.currentGameState)
		{
		case GameState.Aiming:
			this.dartTime = -1f;
			if (Game1.options.gamepadControls)
			{
				Game1.setMousePosition(Utility.Vector2ToPoint(this.TransformDraw(new Vector2(this.screenWidth / 2, this.screenHeight / 2))));
			}
			break;
		case GameState.Charging:
			if (Darts.chargeSound == null)
			{
				Game1.playSound("SinWave", out Darts.chargeSound);
			}
			this.chargeTime = 1f;
			this.chargeDirection = -1f;
			this.canCancelShot = true;
			break;
		case GameState.Firing:
			this.throwStartPosition = this.dartBoardCenter + new Vector2(Utility.RandomFloat(-64f, 64f), 200f);
			Game1.playSound("FishHit");
			this.hangTime = 0.25f;
			break;
		case GameState.ShowScore:
			this.stateTimer = 1f;
			break;
		case GameState.GameOver:
			if (this.points == 0)
			{
				this.gameOverString = Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11943");
				Game1.playSound("yoba");
			}
			else
			{
				this.gameOverString = Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11946");
				Game1.playSound("slimedead");
			}
			this.stateTimer = 3f;
			break;
		case GameState.Scoring:
			break;
		}
	}

	public bool WasButtonHeld()
	{
		if (Game1.input.GetMouseState().LeftButton == ButtonState.Pressed)
		{
			return true;
		}
		if (Game1.input.GetGamePadState().IsButtonDown(Buttons.A))
		{
			return true;
		}
		if (Game1.input.GetGamePadState().IsButtonDown(Buttons.X))
		{
			return true;
		}
		if (Game1.isOneOfTheseKeysDown(Game1.input.GetKeyboardState(), Game1.options.actionButton))
		{
			return true;
		}
		if (Game1.isOneOfTheseKeysDown(Game1.input.GetKeyboardState(), Game1.options.actionButton))
		{
			return true;
		}
		return false;
	}

	public bool WasButtonPressed()
	{
		if (Game1.input.GetMouseState().LeftButton == ButtonState.Pressed && Game1.oldMouseState.LeftButton == ButtonState.Released)
		{
			return true;
		}
		if (Game1.input.GetGamePadState().IsButtonDown(Buttons.A) && Game1.oldPadState.IsButtonUp(Buttons.A))
		{
			return true;
		}
		if (Game1.input.GetGamePadState().IsButtonDown(Buttons.X) && Game1.oldPadState.IsButtonUp(Buttons.X))
		{
			return true;
		}
		if (Game1.isOneOfTheseKeysDown(Game1.input.GetKeyboardState(), Game1.options.actionButton) && !Game1.isOneOfTheseKeysDown(Game1.oldKBState, Game1.options.actionButton))
		{
			return true;
		}
		if (Game1.isOneOfTheseKeysDown(Game1.input.GetKeyboardState(), Game1.options.actionButton) && !Game1.isOneOfTheseKeysDown(Game1.oldKBState, Game1.options.actionButton))
		{
			return true;
		}
		return false;
	}

	public bool tick(GameTime time)
	{
		if (this.stateTimer > 0f)
		{
			this.stateTimer -= (float)time.ElapsedGameTime.TotalSeconds;
			if (this.stateTimer <= 0f)
			{
				this.stateTimer = 0f;
				switch (this.currentGameState)
				{
				case GameState.ShowScore:
					if (this.lastHitAmount == 0)
					{
						if (this.dartCount <= 0)
						{
							this.SetGameState(GameState.Scoring);
						}
						else
						{
							this.SetGameState(GameState.Aiming);
						}
					}
					else
					{
						this.nextPointTransferTime = 0.5f;
						this.SetGameState(GameState.Scoring);
					}
					break;
				case GameState.GameOver:
					this.QuitGame();
					return true;
				}
			}
		}
		if (this.currentGameState == GameState.GameOver && this.WasButtonPressed())
		{
			this.QuitGame();
			return true;
		}
		this.cursorPosition = (Utility.PointToVector2(Game1.getMousePosition()) - this.upperLeft) / this.GetPixelScale();
		switch (this.currentGameState)
		{
		case GameState.Aiming:
			this.chargeTime = 1f;
			this.aimPosition = this.cursorPosition;
			this.aimPosition.X += (float)Math.Sin(time.TotalGameTime.TotalSeconds * 0.75) * 32f;
			this.aimPosition.Y += (float)Math.Sin(time.TotalGameTime.TotalSeconds * 1.5) * 32f;
			if (this.WasButtonPressed() && this.IsAiming())
			{
				this.SetGameState(GameState.Charging);
			}
			break;
		case GameState.Charging:
			if (Darts.chargeSound != null)
			{
				Game1.sounds.SetPitch(Darts.chargeSound, 2400f * (1f - this.chargeTime));
			}
			this.chargeTime += (float)time.ElapsedGameTime.TotalSeconds * this.chargeDirection;
			if (this.chargeDirection < 0f && this.chargeTime < 0f)
			{
				this.canCancelShot = false;
				this.chargeTime = 0f;
				this.chargeDirection = 1f;
			}
			else if (this.chargeDirection > 0f && this.chargeTime >= 1f)
			{
				this.chargeTime = 1f;
				this.chargeDirection = -1f;
			}
			if (!this.WasButtonHeld())
			{
				if (this.chargeTime > 0.8f && this.canCancelShot)
				{
					this.SetGameState(GameState.Aiming);
					this.chargeTime = 0f;
				}
				else
				{
					this.dartCount--;
					this.throwsCount++;
					this.FireDart(this.chargeTime);
				}
			}
			break;
		case GameState.Firing:
			if (this.hangTime > 0f)
			{
				this.hangTime -= (float)time.ElapsedGameTime.TotalSeconds;
				if (this.hangTime <= 0f)
				{
					float random_angle = Utility.RandomFloat(0f, (float)Math.PI * 2f);
					this.aimPosition += new Vector2((float)Math.Sin(random_angle), (float)Math.Cos(random_angle)) * Utility.RandomFloat(0f, this.GetRadiusFromCharge() * 32f);
					Game1.playSound("cast");
					this.dartTime = 0f;
					this.dartPosition = this.throwStartPosition;
				}
			}
			else if (this.dartTime >= 0f)
			{
				this.dartTime += (float)time.ElapsedGameTime.TotalSeconds / 0.75f;
				this.dartPosition.X = Utility.Lerp(this.throwStartPosition.X, this.aimPosition.X, this.dartTime);
				this.dartPosition.Y = Utility.Lerp(this.throwStartPosition.Y, this.aimPosition.Y, this.dartTime);
				if (this.dartTime >= 1f)
				{
					Game1.playSound("Cowboy_gunshot");
					this.lastHitAmount = this.GetPointsForAim();
					this.SetGameState(GameState.ShowScore);
				}
			}
			break;
		case GameState.Scoring:
			if (this.lastHitAmount > 0)
			{
				if (!(this.nextPointTransferTime > 0f))
				{
					break;
				}
				this.nextPointTransferTime -= (float)time.ElapsedGameTime.TotalSeconds;
				if (this.nextPointTransferTime < 0f)
				{
					this.shakeScore = true;
					int transfer_amount = 1;
					if (this.lastHitAmount > 10 && this.points > 10)
					{
						transfer_amount = 10;
					}
					this.points -= transfer_amount;
					this.lastHitAmount -= transfer_amount;
					Game1.playSound("moneyDial");
					this.nextPointTransferTime = 0.05f;
					if (this.points < 0)
					{
						this.alternateTextString = Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11947");
						Game1.playSound("fishEscape");
						this.nextPointTransferTime = 1f;
						this.lastHitAmount = 0;
					}
				}
				break;
			}
			if (this.nextPointTransferTime > 0f)
			{
				this.nextPointTransferTime -= (float)time.ElapsedGameTime.TotalSeconds;
			}
			if (!(this.nextPointTransferTime <= 0f))
			{
				break;
			}
			this.nextPointTransferTime = 0f;
			if (this.points == 0)
			{
				this.SetGameState(GameState.GameOver);
				break;
			}
			if (this.points < 0)
			{
				this.points = this.previousPoints;
			}
			if (this.dartCount <= 0)
			{
				this.SetGameState(GameState.GameOver);
			}
			else
			{
				this.SetGameState(GameState.Aiming);
			}
			break;
		}
		if (this.IsAiming() || this.currentGameState == GameState.Charging)
		{
			Game1.mouseCursorTransparency = 0f;
		}
		else
		{
			Game1.mouseCursorTransparency = 1f;
		}
		return false;
	}

	public virtual bool IsAiming()
	{
		if (this.currentGameState == GameState.Aiming && this.cursorPosition.X > 0f && this.cursorPosition.X < 320f && this.cursorPosition.Y > 0f && this.cursorPosition.Y < 320f)
		{
			return true;
		}
		return false;
	}

	public float GetRadiusFromCharge()
	{
		return (float)Math.Pow(this.chargeTime, 0.5);
	}

	public void receiveLeftClick(int x, int y, bool playSound = true)
	{
	}

	public void releaseLeftClick(int x, int y)
	{
	}

	public virtual int GetPointsForAim()
	{
		Vector2 hit_point = this.aimPosition;
		Vector2 offset = this.dartBoardCenter - hit_point;
		float radius = offset.Length();
		if (radius < 5f)
		{
			Game1.playSound("parrot");
			this.lastHitWasDouble = true;
			this.lastHitString = Game1.content.LoadString("Strings\\UI:Darts_Bullseye");
			return 50;
		}
		if (radius < 12f)
		{
			Game1.playSound("parrot");
			this.lastHitString = Game1.content.LoadString("Strings\\UI:Darts_Bull");
			return 25;
		}
		if (radius > 88f)
		{
			Game1.playSound("fishEscape");
			this.lastHitString = Game1.content.LoadString("Strings\\UI:Darts_OffTheIsland");
			return 0;
		}
		float angle = (float)(Math.Atan2(offset.Y, offset.X) * (180.0 / Math.PI));
		angle -= 81f;
		if (angle < 0f)
		{
			angle += 360f;
		}
		int region = (int)(angle / 18f);
		int[] points = new int[20]
		{
			20, 1, 18, 4, 13, 6, 10, 15, 2, 17,
			3, 19, 7, 16, 8, 11, 14, 9, 12, 5
		};
		int base_points = 0;
		if (region < points.Length)
		{
			base_points = points[region];
		}
		if (radius >= 46f && radius < 55f)
		{
			Game1.playSound("parrot");
			this.lastHitString = base_points + "x3";
			return base_points * 3;
		}
		if (radius >= 79f)
		{
			this.lastHitWasDouble = true;
			Game1.playSound("parrot");
			this.lastHitString = base_points + "x2";
			return base_points * 2;
		}
		this.lastHitString = base_points.ToString() ?? "";
		return base_points;
	}

	public virtual void FireDart(float radius)
	{
		this.SetGameState(GameState.Firing);
	}

	public void releaseRightClick(int x, int y)
	{
	}

	public void receiveRightClick(int x, int y, bool playSound = true)
	{
	}

	public void receiveKeyPress(Keys k)
	{
		if (Game1.input.GetGamePadState().IsButtonDown(Buttons.Back) || k.Equals(Keys.Escape))
		{
			this.QuitGame();
		}
	}

	public void receiveKeyRelease(Keys k)
	{
	}

	public void QuitGame()
	{
		this.unload();
		Game1.playSound("bigDeSelect");
		Game1.currentMinigame = null;
		if (this.currentGameState != GameState.GameOver)
		{
			return;
		}
		if (this.points == 0)
		{
			bool perfect_game = this.IsPerfectVictory();
			if (perfect_game)
			{
				Game1.multiplayer.globalChatInfoMessage("DartsWinPerfect", Game1.player.Name);
			}
			else
			{
				Game1.multiplayer.globalChatInfoMessage("DartsWin", Game1.player.Name, this.throwsCount.ToString());
			}
			if (!(Game1.currentLocation is IslandSouthEastCave))
			{
				return;
			}
			string text = Game1.content.LoadString("Strings\\StringsFromMaps:Pirates7_Win");
			if (perfect_game)
			{
				text = Game1.content.LoadString("Strings\\StringsFromMaps:Pirates7_Win_Perfect");
			}
			text += "#";
			int won_dart_nuts = Game1.player.team.GetDroppedLimitedNutCount("Darts");
			if ((this.startingDartCount == 20 && won_dart_nuts == 0) || (this.startingDartCount == 15 && won_dart_nuts == 1) || (this.startingDartCount == 10 && won_dart_nuts == 2))
			{
				text += Game1.content.LoadString("Strings\\StringsFromMaps:Pirates7_WinPrize");
				Game1.afterDialogues = (Game1.afterFadeFunction)Delegate.Combine(Game1.afterDialogues, (Game1.afterFadeFunction)delegate
				{
					Game1.player.team.RequestLimitedNutDrops("Darts", Game1.currentLocation, 1984, 512, 3);
				});
			}
			else
			{
				text += Game1.content.LoadString("Strings\\StringsFromMaps:Pirates7_WinNoPrize");
			}
			Game1.drawDialogueNoTyping(text);
		}
		else if (Game1.currentLocation is IslandSouthEastCave)
		{
			Game1.drawDialogueNoTyping(Game1.content.LoadString("Strings\\StringsFromMaps:Pirates7_Lose"));
		}
	}

	public void draw(SpriteBatch b)
	{
		b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, new RasterizerState());
		b.Draw(this.texture, this.TransformDraw(new Rectangle(0, 0, 320, 320)), new Rectangle(0, 0, 320, 320), Color.White, 0f, Vector2.Zero, SpriteEffects.None, 0f);
		if (this.IsAiming() || this.currentGameState == GameState.Charging)
		{
			b.Draw(this.texture, this.TransformDraw(this.aimPosition), new Rectangle(0, 320, 64, 64), Color.White * 0.5f, 0f, new Vector2(32f, 32f), this.GetPixelScale() * this.GetRadiusFromCharge(), SpriteEffects.None, 0f);
		}
		if (this.dartTime >= 0f)
		{
			Rectangle dart_rect = new Rectangle(0, 384, 16, 32);
			if (this.dartTime > 0.65f)
			{
				dart_rect.X = 16;
			}
			if (this.dartTime > 0.9f)
			{
				dart_rect.X = 32;
			}
			float y_offset = (float)Math.Sin((double)this.dartTime * Math.PI) * 200f;
			float rotation = (float)Math.Atan2(this.aimPosition.X - this.throwStartPosition.X, this.throwStartPosition.Y - this.aimPosition.Y);
			b.Draw(this.texture, this.TransformDraw(this.dartPosition - new Vector2(0f, y_offset)), dart_rect, Color.White, rotation, new Vector2(8f, 16f), this.GetPixelScale(), SpriteEffects.None, 0.02f);
		}
		Vector2 score_position = this.TransformDraw(new Vector2(160f, 16f));
		Vector2 score_shake = Vector2.Zero;
		if (this.shakeScore)
		{
			score_shake = new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2));
		}
		if (this.alternateTextString != "")
		{
			SpriteText.drawStringWithScrollCenteredAt(b, this.alternateTextString, (int)(score_position.X + score_shake.X), (int)(score_position.Y + score_shake.Y), "", 1f, SpriteText.color_Red);
		}
		else if (this.points >= 0)
		{
			string points_string = Game1.content.LoadString("Strings\\UI:Darts_PointsToGo", this.points);
			if (this.points == 1)
			{
				points_string = Game1.content.LoadString("Strings\\UI:Darts_PointToGo", this.points);
			}
			SpriteText.drawStringWithScrollCenteredAt(b, points_string, (int)(score_position.X + score_shake.X), (int)(score_position.Y + score_shake.Y));
			if (this.currentGameState == GameState.ShowScore || this.currentGameState == GameState.Scoring)
			{
				if (this.shakeScore)
				{
					score_shake = new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2));
				}
				score_position.Y += 64f;
				string string_to_draw = ((this.currentGameState == GameState.ShowScore) ? (" " + this.lastHitString + " ") : (" " + this.lastHitAmount + " "));
				SpriteText.drawStringWithScrollCenteredAt(b, string_to_draw, (int)(score_position.X + score_shake.X), (int)(score_position.Y + score_shake.Y), "", 1f, SpriteText.color_Blue, 2);
			}
		}
		for (int i = 0; i < this.dartCount; i++)
		{
			b.Draw(position: this.TransformDraw(new Vector2(7 + i * 10, 317f)), texture: this.texture, sourceRectangle: new Rectangle(64, 384, 16, 32), color: Color.White, rotation: 0f, origin: new Vector2(0f, 32f), scale: this.GetPixelScale(), effects: SpriteEffects.None, layerDepth: 0.02f);
		}
		if (this.gameOverString != "")
		{
			b.Draw(Game1.staminaRect, this.TransformDraw(new Rectangle(0, 0, this.screenWidth, this.screenHeight)), null, Color.Black * 0.75f, 0f, Vector2.Zero, SpriteEffects.None, 0f);
			if (this.points == 0)
			{
				score_position = this.TransformDraw(new Vector2(160f, 144f));
				SpriteText.drawStringWithScrollCenteredAt(b, this.gameOverString, (int)score_position.X, (int)score_position.Y);
				score_position = this.TransformDraw(new Vector2(160f, 176f));
				if (this.IsPerfectVictory())
				{
					SpriteText.drawStringWithScrollCenteredAt(b, Game1.content.LoadString("Strings\\UI:Darts_WinTextPerfect", this.throwsCount), (int)(score_position.X + score_shake.X), (int)(score_position.Y + score_shake.Y), "", 1f, SpriteText.color_Blue, 2);
				}
				else
				{
					SpriteText.drawStringWithScrollCenteredAt(b, Game1.content.LoadString("Strings\\UI:Darts_WinText", this.throwsCount), (int)(score_position.X + score_shake.X), (int)(score_position.Y + score_shake.Y), "", 1f, SpriteText.color_Blue, 2);
				}
			}
			else
			{
				score_position = this.TransformDraw(new Vector2(160f, 160f));
				SpriteText.drawStringWithScrollCenteredAt(b, this.gameOverString, (int)score_position.X, (int)score_position.Y);
			}
		}
		if (Game1.options.gamepadControls && !Game1.options.hardwareCursor)
		{
			b.Draw(Game1.mouseCursors, new Vector2(Game1.getMouseX(), Game1.getMouseY()), Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, (Game1.options.snappyMenus && Game1.options.gamepadControls) ? 44 : 0, 16, 16), Color.White * Game1.mouseCursorTransparency, 0f, Vector2.Zero, 4f + Game1.dialogueButtonScale / 150f, SpriteEffects.None, 1f);
		}
		b.End();
	}

	public float GetPixelScale()
	{
		return this.pixelScale;
	}

	public Rectangle TransformDraw(Rectangle dest)
	{
		dest.X = (int)Math.Round((float)dest.X * this.pixelScale) + (int)this.upperLeft.X;
		dest.Y = (int)Math.Round((float)dest.Y * this.pixelScale) + (int)this.upperLeft.Y;
		dest.Width = (int)((float)dest.Width * this.pixelScale);
		dest.Height = (int)((float)dest.Height * this.pixelScale);
		return dest;
	}

	public Vector2 TransformDraw(Vector2 dest)
	{
		dest.X = (int)Math.Round(dest.X * this.pixelScale) + (int)this.upperLeft.X;
		dest.Y = (int)Math.Round(dest.Y * this.pixelScale) + (int)this.upperLeft.Y;
		return dest;
	}

	public bool IsPerfectVictory()
	{
		if (this.points == 0)
		{
			return this.throwsCount <= 6;
		}
		return false;
	}

	public void changeScreenSize()
	{
		this.screenWidth = 320;
		this.screenHeight = 320;
		float pixel_zoom_adjustment = 1f / Game1.options.zoomLevel;
		int viewport_width = Game1.game1.localMultiplayerWindow.Width;
		int viewport_height = Game1.game1.localMultiplayerWindow.Height;
		this.pixelScale = Math.Min(5f, Math.Min((float)viewport_width * pixel_zoom_adjustment / (float)this.screenWidth, (float)viewport_height * pixel_zoom_adjustment / (float)this.screenHeight));
		float snap = 0.1f;
		this.pixelScale = (float)(int)(this.pixelScale / snap) * snap;
		this.upperLeft = new Vector2((float)(viewport_width / 2) * pixel_zoom_adjustment, (float)(viewport_height / 2) * pixel_zoom_adjustment);
		this.upperLeft.X -= (float)(this.screenWidth / 2) * this.pixelScale;
		this.upperLeft.Y -= (float)(this.screenHeight / 2) * this.pixelScale;
		this.dartBoardCenter = new Vector2(160f, 160f);
	}

	public void unload()
	{
		if (Darts.chargeSound != null)
		{
			Darts.chargeSound.Stop(AudioStopOptions.Immediate);
			Darts.chargeSound = null;
		}
		Game1.stopMusicTrack(MusicContext.MiniGame);
		Game1.player.faceDirection(0);
	}

	public bool forceQuit()
	{
		this.unload();
		return true;
	}

	public void leftClickHeld(int x, int y)
	{
	}

	public void receiveEventPoke(int data)
	{
		throw new NotImplementedException();
	}

	public string minigameId()
	{
		return "Darts";
	}

	public bool doMainGameUpdates()
	{
		return false;
	}
}
