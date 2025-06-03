using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using StardewValley.Extensions;
using StardewValley.Locations;
using StardewValley.Menus;

namespace StardewValley.Minigames;

public class CalicoJack : IMinigame
{
	public const int cardState_flipped = -1;

	public const int cardState_up = 0;

	public const int cardState_transitioning = 400;

	public const int bet = 100;

	public const int cardWidth = 96;

	public const int dealTime = 1000;

	public const int playingTo = 21;

	public const int passNumber = 18;

	public const int dealerTurnDelay = 1000;

	public List<int[]> playerCards;

	public List<int[]> dealerCards;

	private Random r;

	private int currentBet;

	private int startTimer;

	private int dealerTurnTimer = -1;

	private int bustTimer;

	private ClickableComponent hit;

	private ClickableComponent stand;

	private ClickableComponent doubleOrNothing;

	private ClickableComponent playAgain;

	private ClickableComponent quit;

	private ClickableComponent currentlySnappedComponent;

	private bool showingResultsScreen;

	private bool playerWon;

	private bool highStakes;

	private string endMessage = "";

	private string endTitle = "";

	private string coinBuffer;

	public CalicoJack(int toBet = -1, bool highStakes = false)
	{
		this.coinBuffer = ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ru) ? "     " : ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.zh) ? "\u3000\u3000" : "  "));
		this.highStakes = highStakes;
		this.startTimer = 1000;
		this.playerCards = new List<int[]>();
		this.dealerCards = new List<int[]>();
		if (toBet == -1)
		{
			this.currentBet = (highStakes ? 1000 : 100);
		}
		else
		{
			this.currentBet = toBet;
		}
		Club.timesPlayedCalicoJack++;
		this.r = Utility.CreateRandom(Club.timesPlayedCalicoJack, Game1.stats.DaysPlayed, Game1.uniqueIDForThisGame);
		this.hit = new ClickableComponent(new Rectangle((int)((float)Game1.graphics.GraphicsDevice.Viewport.Width / Game1.options.zoomLevel - 128f - (float)SpriteText.getWidthOfString(Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11924"))), Game1.graphics.GraphicsDevice.Viewport.Height / 2 - 64, SpriteText.getWidthOfString(Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11924") + "  "), 64), "", " " + Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11924") + " ");
		this.stand = new ClickableComponent(new Rectangle((int)((float)Game1.graphics.GraphicsDevice.Viewport.Width / Game1.options.zoomLevel - 128f - (float)SpriteText.getWidthOfString(Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11927"))), Game1.graphics.GraphicsDevice.Viewport.Height / 2 + 32, SpriteText.getWidthOfString(Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11927") + "  "), 64), "", " " + Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11927") + " ");
		this.doubleOrNothing = new ClickableComponent(new Rectangle((int)((float)(Game1.graphics.GraphicsDevice.Viewport.Width / 2) / Game1.options.zoomLevel) - SpriteText.getWidthOfString(Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11930")) / 2, (int)((float)(Game1.graphics.GraphicsDevice.Viewport.Height / 2) / Game1.options.zoomLevel), SpriteText.getWidthOfString(Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11930")) + 64, 64), "", Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11930"));
		this.playAgain = new ClickableComponent(new Rectangle((int)((float)(Game1.graphics.GraphicsDevice.Viewport.Width / 2) / Game1.options.zoomLevel) - SpriteText.getWidthOfString(Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11933")) / 2, (int)((float)(Game1.graphics.GraphicsDevice.Viewport.Height / 2) / Game1.options.zoomLevel) + 64 + 16, SpriteText.getWidthOfString(Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11933")) + 64, 64), "", Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11933"));
		this.quit = new ClickableComponent(new Rectangle((int)((float)(Game1.graphics.GraphicsDevice.Viewport.Width / 2) / Game1.options.zoomLevel) - SpriteText.getWidthOfString(Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11936")) / 2, (int)((float)(Game1.graphics.GraphicsDevice.Viewport.Height / 2) / Game1.options.zoomLevel) + 64 + 96, SpriteText.getWidthOfString(Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11936")) + 64, 64), "", Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11936"));
		this.RepositionButtons();
		if (Game1.options.SnappyMenus)
		{
			this.currentlySnappedComponent = this.hit;
			this.currentlySnappedComponent.snapMouseCursorToCenter();
		}
	}

	public void RepositionButtons()
	{
		this.hit.bounds = new Rectangle((int)((float)Game1.game1.localMultiplayerWindow.Width / Game1.options.zoomLevel - 128f - (float)SpriteText.getWidthOfString(Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11924"))), Game1.viewport.Height / 2 - 64, SpriteText.getWidthOfString(Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11924") + "  "), 64);
		this.stand.bounds = new Rectangle((int)((float)Game1.game1.localMultiplayerWindow.Width / Game1.options.zoomLevel - 128f - (float)SpriteText.getWidthOfString(Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11927"))), Game1.viewport.Height / 2 + 32, SpriteText.getWidthOfString(Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11927") + "  "), 64);
		this.doubleOrNothing.bounds = new Rectangle((int)((float)(Game1.game1.localMultiplayerWindow.Width / 2) / Game1.options.zoomLevel) - (SpriteText.getWidthOfString(Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11930")) + 64) / 2, (int)((float)(Game1.game1.localMultiplayerWindow.Height / 2) / Game1.options.zoomLevel), SpriteText.getWidthOfString(Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11930")) + 64, 64);
		this.playAgain.bounds = new Rectangle((int)((float)(Game1.game1.localMultiplayerWindow.Width / 2) / Game1.options.zoomLevel) - (SpriteText.getWidthOfString(Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11933")) + 64) / 2, (int)((float)(Game1.game1.localMultiplayerWindow.Height / 2) / Game1.options.zoomLevel) + 64 + 16, SpriteText.getWidthOfString(Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11933")) + 64, 64);
		this.quit.bounds = new Rectangle((int)((float)(Game1.game1.localMultiplayerWindow.Width / 2) / Game1.options.zoomLevel) - (SpriteText.getWidthOfString(Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11936")) + 64) / 2, (int)((float)(Game1.game1.localMultiplayerWindow.Height / 2) / Game1.options.zoomLevel) + 64 + 96, SpriteText.getWidthOfString(Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11936")) + 64, 64);
	}

	public bool overrideFreeMouseMovement()
	{
		return Game1.options.SnappyMenus;
	}

	public bool playButtonsActive()
	{
		if (this.startTimer <= 0 && this.dealerTurnTimer < 0)
		{
			return !this.showingResultsScreen;
		}
		return false;
	}

	public bool tick(GameTime time)
	{
		for (int i = 0; i < this.playerCards.Count; i++)
		{
			if (this.playerCards[i][1] > 0)
			{
				this.playerCards[i][1] -= time.ElapsedGameTime.Milliseconds;
				if (this.playerCards[i][1] <= 0)
				{
					this.playerCards[i][1] = 0;
				}
			}
		}
		for (int j = 0; j < this.dealerCards.Count; j++)
		{
			if (this.dealerCards[j][1] > 0)
			{
				this.dealerCards[j][1] -= time.ElapsedGameTime.Milliseconds;
				if (this.dealerCards[j][1] <= 0)
				{
					this.dealerCards[j][1] = 0;
				}
			}
		}
		if (this.startTimer > 0)
		{
			int oldTimer = this.startTimer;
			this.startTimer -= time.ElapsedGameTime.Milliseconds;
			if (oldTimer % 250 < this.startTimer % 250)
			{
				switch (oldTimer / 250)
				{
				case 4:
					this.dealerCards.Add(new int[2]
					{
						this.r.Next(1, 12),
						-1
					});
					break;
				case 3:
					this.dealerCards.Add(new int[2]
					{
						this.r.Next(1, 10),
						400
					});
					break;
				case 2:
					this.playerCards.Add(new int[2]
					{
						this.r.Next(1, 12),
						400
					});
					break;
				case 1:
					this.playerCards.Add(new int[2]
					{
						this.r.Next(1, 10),
						400
					});
					break;
				}
				Game1.playSound("shwip");
			}
		}
		else if (this.bustTimer > 0)
		{
			this.bustTimer -= time.ElapsedGameTime.Milliseconds;
			if (this.bustTimer <= 0)
			{
				this.endGame();
			}
		}
		else if (this.dealerTurnTimer > 0 && !this.showingResultsScreen)
		{
			this.dealerTurnTimer -= time.ElapsedGameTime.Milliseconds;
			if (this.dealerTurnTimer <= 0)
			{
				int dealerTotal = 0;
				foreach (int[] i2 in this.dealerCards)
				{
					dealerTotal += i2[0];
				}
				int playertotal = 0;
				foreach (int[] i3 in this.playerCards)
				{
					playertotal += i3[0];
				}
				if (this.dealerCards[0][1] == -1)
				{
					this.dealerCards[0][1] = 400;
					Game1.playSound("shwip");
				}
				else if (dealerTotal < 18 || (dealerTotal < playertotal && playertotal <= 21))
				{
					int nextCard = this.r.Next(1, 10);
					int dealerDistance = 21 - dealerTotal;
					if (playertotal == 20 && this.r.NextBool())
					{
						nextCard = dealerDistance + this.r.Next(1, 4);
					}
					else if (playertotal == 19 && this.r.NextDouble() < 0.25)
					{
						nextCard = dealerDistance + this.r.Next(1, 4);
					}
					else if (playertotal == 18 && this.r.NextDouble() < 0.1)
					{
						nextCard = dealerDistance + this.r.Next(1, 4);
					}
					if (this.r.NextDouble() < Math.Max(0.0005, 0.001 + Game1.player.DailyLuck / 20.0 + (double)((float)Game1.player.LuckLevel * 0.002f)))
					{
						nextCard = 999;
						this.currentBet *= 3;
					}
					this.dealerCards.Add(new int[2] { nextCard, 400 });
					dealerTotal += this.dealerCards.Last()[0];
					Game1.playSound((nextCard == 999) ? "batScreech" : "shwip");
					if (dealerTotal > 21)
					{
						this.bustTimer = 2000;
					}
				}
				else
				{
					this.bustTimer = 50;
				}
				this.dealerTurnTimer = 1000;
			}
		}
		if (this.playButtonsActive())
		{
			this.hit.scale = (this.hit.bounds.Contains(Game1.getOldMouseX(), Game1.getOldMouseY()) ? 1.25f : 1f);
			this.stand.scale = (this.stand.bounds.Contains(Game1.getOldMouseX(), Game1.getOldMouseY()) ? 1.25f : 1f);
		}
		else if (this.showingResultsScreen)
		{
			this.doubleOrNothing.scale = (this.doubleOrNothing.bounds.Contains(Game1.getOldMouseX(), Game1.getOldMouseY()) ? 1.25f : 1f);
			this.playAgain.scale = (this.playAgain.bounds.Contains(Game1.getOldMouseX(), Game1.getOldMouseY()) ? 1.25f : 1f);
			this.quit.scale = (this.quit.bounds.Contains(Game1.getOldMouseX(), Game1.getOldMouseY()) ? 1.25f : 1f);
		}
		return false;
	}

	public void endGame()
	{
		if (Game1.options.SnappyMenus)
		{
			this.currentlySnappedComponent = this.quit;
			this.currentlySnappedComponent.snapMouseCursorToCenter();
		}
		this.showingResultsScreen = true;
		int playertotal = 0;
		foreach (int[] i in this.playerCards)
		{
			playertotal += i[0];
		}
		if (playertotal == 21)
		{
			Game1.playSound("reward");
			this.playerWon = true;
			this.endTitle = Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11943");
			this.endMessage = Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11944");
			Game1.player.clubCoins += this.currentBet;
			return;
		}
		if (playertotal > 21)
		{
			Game1.playSound("fishEscape");
			this.endTitle = Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11946");
			this.endMessage = Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11947");
			Game1.player.clubCoins -= this.currentBet;
			if (Game1.player.clubCoins < 0)
			{
				Game1.player.clubCoins = 0;
			}
			return;
		}
		int dealerTotal = 0;
		foreach (int[] i2 in this.dealerCards)
		{
			dealerTotal += i2[0];
		}
		if (dealerTotal > 21)
		{
			Game1.playSound("reward");
			this.playerWon = true;
			this.endTitle = Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11943");
			this.endMessage = Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11950");
			Game1.player.clubCoins += this.currentBet;
			return;
		}
		if (playertotal == dealerTotal)
		{
			this.endTitle = Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11951");
			this.endMessage = Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11952");
			return;
		}
		if (playertotal > dealerTotal)
		{
			Game1.playSound("reward");
			this.endTitle = Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11943");
			this.endMessage = Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11955", 21);
			Game1.player.clubCoins += this.currentBet;
			this.playerWon = true;
			return;
		}
		Game1.playSound("fishEscape");
		this.endTitle = Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11946");
		this.endMessage = Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11958", 21);
		Game1.player.clubCoins -= this.currentBet;
		if (Game1.player.clubCoins < 0)
		{
			Game1.player.clubCoins = 0;
		}
	}

	public void receiveLeftClick(int x, int y, bool playSound = true)
	{
		if (this.playButtonsActive() && this.bustTimer <= 0)
		{
			if (this.hit.bounds.Contains(x, y))
			{
				int playertotal = 0;
				foreach (int[] i in this.playerCards)
				{
					playertotal += i[0];
				}
				int nextCard = this.r.Next(1, 10);
				int distance = 21 - playertotal;
				if (distance > 1 && distance < 6 && this.r.NextDouble() < (double)(1f / (float)distance))
				{
					nextCard = this.r.Choose(distance, distance - 1);
				}
				this.playerCards.Add(new int[2] { nextCard, 400 });
				Game1.playSound("shwip");
				int total = 0;
				foreach (int[] i2 in this.playerCards)
				{
					total += i2[0];
				}
				if (total == 21)
				{
					this.bustTimer = 1000;
				}
				else if (total > 21)
				{
					this.bustTimer = 1000;
				}
			}
			if (this.stand.bounds.Contains(x, y))
			{
				this.dealerTurnTimer = 1000;
				Game1.playSound("coin");
			}
		}
		else if (this.showingResultsScreen)
		{
			if (this.playerWon && this.doubleOrNothing.containsPoint(x, y))
			{
				Game1.currentMinigame = new CalicoJack(this.currentBet * 2, this.highStakes);
				Game1.playSound("bigSelect");
			}
			if (Game1.player.clubCoins >= this.currentBet && this.playAgain.containsPoint(x, y))
			{
				Game1.currentMinigame = new CalicoJack(-1, this.highStakes);
				Game1.playSound("smallSelect");
			}
			if (this.quit.containsPoint(x, y))
			{
				Game1.currentMinigame = null;
				Game1.playSound("bigDeSelect");
			}
		}
	}

	public void leftClickHeld(int x, int y)
	{
	}

	public void receiveRightClick(int x, int y, bool playSound = true)
	{
	}

	public void releaseLeftClick(int x, int y)
	{
	}

	public void releaseRightClick(int x, int y)
	{
	}

	public void receiveKeyPress(Keys k)
	{
		if (!Game1.options.SnappyMenus || this.currentlySnappedComponent == null)
		{
			return;
		}
		if (Game1.options.doesInputListContain(Game1.options.moveUpButton, k))
		{
			if (this.currentlySnappedComponent.Equals(this.stand))
			{
				this.currentlySnappedComponent = this.hit;
			}
			else if (this.currentlySnappedComponent.Equals(this.playAgain) && this.playerWon)
			{
				this.currentlySnappedComponent = this.doubleOrNothing;
			}
			else if (this.currentlySnappedComponent.Equals(this.quit) && Game1.player.clubCoins >= this.currentBet)
			{
				this.currentlySnappedComponent = this.playAgain;
			}
		}
		else if (Game1.options.doesInputListContain(Game1.options.moveDownButton, k))
		{
			if (this.currentlySnappedComponent.Equals(this.hit))
			{
				this.currentlySnappedComponent = this.stand;
			}
			else if (this.currentlySnappedComponent.Equals(this.doubleOrNothing))
			{
				this.currentlySnappedComponent = this.playAgain;
			}
			else if (this.currentlySnappedComponent.Equals(this.playAgain))
			{
				this.currentlySnappedComponent = this.quit;
			}
		}
		this.currentlySnappedComponent.snapMouseCursorToCenter();
	}

	public void receiveKeyRelease(Keys k)
	{
	}

	public void draw(SpriteBatch b)
	{
		b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
		b.Draw(Game1.staminaRect, new Rectangle(0, 0, Game1.graphics.GraphicsDevice.Viewport.Width, Game1.graphics.GraphicsDevice.Viewport.Height), this.highStakes ? new Color(130, 0, 82) : Color.DarkGreen);
		Vector2 coin_draw_pos = new Vector2(Game1.graphics.GraphicsDevice.Viewport.Width - 192, 32f);
		SpriteText.drawStringWithScrollBackground(b, this.coinBuffer + Game1.player.clubCoins, (int)coin_draw_pos.X, (int)coin_draw_pos.Y);
		Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2(coin_draw_pos.X + 4f, coin_draw_pos.Y + 4f), new Rectangle(211, 373, 9, 10), Color.White, 0f, Vector2.Zero, 4f, flipped: false, 1f);
		if (this.showingResultsScreen)
		{
			SpriteText.drawStringWithScrollCenteredAt(b, this.endMessage, Game1.graphics.GraphicsDevice.Viewport.Width / 2, 48);
			SpriteText.drawStringWithScrollCenteredAt(b, this.endTitle, Game1.graphics.GraphicsDevice.Viewport.Width / 2, 128);
			if (!this.endTitle.Equals(Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11951")))
			{
				SpriteText.drawStringWithScrollCenteredAt(b, Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11965", (this.playerWon ? "" : "-") + this.currentBet + "   "), Game1.graphics.GraphicsDevice.Viewport.Width / 2, 256);
				Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2(Game1.graphics.GraphicsDevice.Viewport.Width / 2 - 32 + SpriteText.getWidthOfString(Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11965", (this.playerWon ? "" : "-") + this.currentBet + "   ")) / 2, 260f) + new Vector2(8f, 0f), new Rectangle(211, 373, 9, 10), Color.White, 0f, Vector2.Zero, 4f, flipped: false, 1f);
			}
			if (this.playerWon)
			{
				IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 373, 9, 9), this.doubleOrNothing.bounds.X, this.doubleOrNothing.bounds.Y, this.doubleOrNothing.bounds.Width, this.doubleOrNothing.bounds.Height, Color.White, 4f * this.doubleOrNothing.scale);
				SpriteText.drawString(b, this.doubleOrNothing.label, this.doubleOrNothing.bounds.X + 32, this.doubleOrNothing.bounds.Y + 8);
			}
			if (Game1.player.clubCoins >= this.currentBet)
			{
				IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 373, 9, 9), this.playAgain.bounds.X, this.playAgain.bounds.Y, this.playAgain.bounds.Width, this.playAgain.bounds.Height, Color.White, 4f * this.playAgain.scale);
				SpriteText.drawString(b, this.playAgain.label, this.playAgain.bounds.X + 32, this.playAgain.bounds.Y + 8);
			}
			IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 373, 9, 9), this.quit.bounds.X, this.quit.bounds.Y, this.quit.bounds.Width, this.quit.bounds.Height, Color.White, 4f * this.quit.scale);
			SpriteText.drawString(b, this.quit.label, this.quit.bounds.X + 32, this.quit.bounds.Y + 8);
		}
		else
		{
			Vector2 start = new Vector2(128f, Game1.graphics.GraphicsDevice.Viewport.Height - 320);
			int total = 0;
			foreach (int[] i in this.playerCards)
			{
				int cardHeight = 144;
				if (i[1] > 0)
				{
					cardHeight = (int)(Math.Abs((float)i[1] - 200f) / 200f * 144f);
				}
				IClickableMenu.drawTextureBox(b, Game1.mouseCursors, (i[1] > 200 || i[1] == -1) ? new Rectangle(399, 396, 15, 15) : new Rectangle(384, 396, 15, 15), (int)start.X, (int)start.Y + 72 - cardHeight / 2, 96, cardHeight, Color.White, 4f);
				if (i[1] == 0)
				{
					SpriteText.drawStringHorizontallyCenteredAt(b, i[0].ToString() ?? "", (int)start.X + 48 - 8 + 4, (int)start.Y + 72 - 16);
				}
				start.X += 112f;
				if (i[1] == 0)
				{
					total += i[0];
				}
			}
			SpriteText.drawStringWithScrollBackground(b, Game1.player.Name + ": " + total, 160, (int)start.Y + 144 + 32);
			start.X = 128f;
			start.Y = 128f;
			total = 0;
			foreach (int[] i2 in this.dealerCards)
			{
				int cardHeight2 = 144;
				if (i2[1] > 0)
				{
					cardHeight2 = (int)(Math.Abs((float)i2[1] - 200f) / 200f * 144f);
				}
				IClickableMenu.drawTextureBox(b, Game1.mouseCursors, (i2[1] > 200 || i2[1] == -1) ? new Rectangle(399, 396, 15, 15) : new Rectangle(384, 396, 15, 15), (int)start.X, (int)start.Y + 72 - cardHeight2 / 2, 96, cardHeight2, Color.White, 4f);
				if (i2[1] == 0)
				{
					if (i2[0] == 999)
					{
						b.Draw(Game1.objectSpriteSheet, new Vector2(start.X + 48f - 32f, start.Y + 72f - 32f), new Rectangle(16, 592, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.9f);
					}
					else
					{
						SpriteText.drawStringHorizontallyCenteredAt(b, i2[0].ToString() ?? "", (int)start.X + 48 - 8 + 4, (int)start.Y + 72 - 16);
					}
				}
				start.X += 112f;
				switch (i2[1])
				{
				case 0:
					total += i2[0];
					break;
				case -1:
					total = -99999;
					break;
				}
			}
			SpriteText.drawStringWithScrollBackground(b, Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11970", (total >= 999) ? "!!!" : ((total > 0) ? (total.ToString() ?? "") : "?")), 160, 32);
			SpriteText.drawStringWithScrollBackground(b, Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11972", this.currentBet + this.coinBuffer), 160, Game1.graphics.GraphicsDevice.Viewport.Height / 2 - 48);
			Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2(172 + SpriteText.getWidthOfString(Game1.content.LoadString("Strings\\StringsFromCSFiles:CalicoJack.cs.11972", this.currentBet)), Game1.graphics.GraphicsDevice.Viewport.Height / 2 + 4 - 48), new Rectangle(211, 373, 9, 10), Color.White, 0f, Vector2.Zero, 4f, flipped: false, 1f);
			if (this.playButtonsActive())
			{
				IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 373, 9, 9), this.hit.bounds.X, this.hit.bounds.Y, this.hit.bounds.Width, this.hit.bounds.Height, Color.White, 4f * this.hit.scale);
				SpriteText.drawString(b, this.hit.label, this.hit.bounds.X + 8, this.hit.bounds.Y + 8);
				IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 373, 9, 9), this.stand.bounds.X, this.stand.bounds.Y, this.stand.bounds.Width, this.stand.bounds.Height, Color.White, 4f * this.stand.scale);
				SpriteText.drawString(b, this.stand.label, this.stand.bounds.X + 8, this.stand.bounds.Y + 8);
			}
		}
		if (Game1.IsMultiplayer)
		{
			Utility.drawTextWithColoredShadow(b, Game1.getTimeOfDayString(Game1.timeOfDay), Game1.dialogueFont, new Vector2((float)Game1.graphics.GraphicsDevice.Viewport.Width - Game1.dialogueFont.MeasureString(Game1.getTimeOfDayString(Game1.timeOfDay)).X - 16f, (float)Game1.graphics.GraphicsDevice.Viewport.Height - Game1.dialogueFont.MeasureString(Game1.getTimeOfDayString(Game1.timeOfDay)).Y - 10f), Color.White, Color.Black * 0.2f);
		}
		if (!Game1.options.hardwareCursor)
		{
			b.Draw(Game1.mouseCursors, new Vector2(Game1.getMouseX(), Game1.getMouseY()), Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 0, 16, 16), Color.White, 0f, Vector2.Zero, 4f + Game1.dialogueButtonScale / 150f, SpriteEffects.None, 1f);
		}
		b.End();
	}

	public void changeScreenSize()
	{
		this.RepositionButtons();
	}

	public void unload()
	{
	}

	public void receiveEventPoke(int data)
	{
	}

	public string minigameId()
	{
		return "CalicoJack";
	}

	public bool doMainGameUpdates()
	{
		return false;
	}

	public bool forceQuit()
	{
		return true;
	}
}
