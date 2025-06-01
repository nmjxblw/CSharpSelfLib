using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using StardewValley.Locations;
using StardewValley.Menus;

namespace StardewValley.Minigames;

public class Slots : IMinigame
{
	public const float slotTurnRate = 0.008f;

	public const int numberOfIcons = 8;

	public const int defaultBet = 10;

	private string coinBuffer;

	private List<float> slots;

	private List<float> slotResults;

	private ClickableComponent spinButton10;

	private ClickableComponent spinButton100;

	private ClickableComponent doneButton;

	public bool spinning;

	public bool showResult;

	public float payoutModifier;

	public int currentBet;

	public int spinsCount;

	public int slotsFinished;

	public int endTimer;

	public ClickableComponent currentlySnappedComponent;

	public Slots(int toBet = -1, bool highStakes = false)
	{
		this.coinBuffer = ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.zh) ? "\u3000\u3000" : "  ");
		this.currentBet = toBet;
		if (this.currentBet == -1)
		{
			this.currentBet = 10;
		}
		this.slots = new List<float> { 0f, 0f, 0f };
		this.slotResults = new List<float> { 0f, 0f, 0f };
		Game1.playSound("newArtifact");
		this.setSlotResults(this.slots);
		int buttonTopY = 44;
		this.spinButton10 = this.CreateSpinButton(32, buttonTopY, "Strings\\StringsFromCSFiles:Slots.cs.12117");
		this.spinButton100 = this.CreateSpinButton(37, buttonTopY + 64, "Strings\\StringsFromCSFiles:Slots.cs.12118");
		this.doneButton = this.CreateSpinButton(30, buttonTopY + 128, "Strings\\StringsFromCSFiles:NameSelect.cs.3864");
		if (Game1.isAnyGamePadButtonBeingPressed())
		{
			Game1.setMousePosition(this.spinButton10.bounds.Center);
			if (Game1.options.SnappyMenus)
			{
				this.currentlySnappedComponent = this.spinButton10;
			}
		}
	}

	private ClickableComponent CreateSpinButton(int baseWidth, int yOffset, string nameTranslationKey)
	{
		int extraButtonWidth = this.GetButtonSizeOffset();
		int width = (baseWidth + extraButtonWidth) * 4;
		Vector2 pos = Utility.getTopLeftPositionForCenteringOnScreen(Game1.viewport, width, 52, -16, yOffset);
		return new ClickableComponent(new Rectangle((int)pos.X, (int)pos.Y, width, 52), Game1.content.LoadString(nameTranslationKey));
	}

	public void setSlotResults(List<float> toSet)
	{
		double d = Game1.random.NextDouble();
		double modifier = 1.0 + Game1.player.DailyLuck * 2.0 + (double)Game1.player.LuckLevel * 0.08;
		if (d < 0.001 * modifier)
		{
			this.set(toSet, 5);
			this.payoutModifier = 2500f;
			return;
		}
		if (d < 0.0016 * modifier)
		{
			this.set(toSet, 6);
			this.payoutModifier = 1000f;
			return;
		}
		if (d < 0.0025 * modifier)
		{
			this.set(toSet, 7);
			this.payoutModifier = 500f;
			return;
		}
		if (d < 0.005 * modifier)
		{
			this.set(toSet, 4);
			this.payoutModifier = 200f;
			return;
		}
		if (d < 0.007 * modifier)
		{
			this.set(toSet, 3);
			this.payoutModifier = 120f;
			return;
		}
		if (d < 0.01 * modifier)
		{
			this.set(toSet, 2);
			this.payoutModifier = 80f;
			return;
		}
		if (d < 0.02 * modifier)
		{
			this.set(toSet, 1);
			this.payoutModifier = 30f;
			return;
		}
		if (d < 0.12 * modifier)
		{
			int whereToPutNonStar = Game1.random.Next(3);
			for (int i = 0; i < 3; i++)
			{
				toSet[i] = ((i == whereToPutNonStar) ? Game1.random.Next(7) : 7);
			}
			this.payoutModifier = 3f;
			return;
		}
		if (d < 0.2 * modifier)
		{
			this.set(toSet, 0);
			this.payoutModifier = 5f;
			return;
		}
		if (d < 0.4 * modifier)
		{
			int whereToPutStar = Game1.random.Next(3);
			for (int j = 0; j < 3; j++)
			{
				toSet[j] = ((j == whereToPutStar) ? 7 : Game1.random.Next(7));
			}
			this.payoutModifier = 2f;
			return;
		}
		this.payoutModifier = 0f;
		int[] used = new int[8];
		for (int k = 0; k < 3; k++)
		{
			int next = Game1.random.Next(6);
			while (used[next] > 1)
			{
				next = Game1.random.Next(6);
			}
			toSet[k] = next;
			used[next]++;
		}
	}

	private void set(List<float> toSet, int number)
	{
		toSet[0] = number;
		toSet[1] = number;
		toSet[2] = number;
	}

	public bool tick(GameTime time)
	{
		if (this.spinning && this.endTimer <= 0)
		{
			for (int i = this.slotsFinished; i < this.slots.Count; i++)
			{
				float old = this.slots[i];
				this.slots[i] += (float)time.ElapsedGameTime.Milliseconds * 0.008f * (1f - (float)i * 0.05f);
				this.slots[i] %= 8f;
				if (i == 2)
				{
					if (old % (0.25f + (float)this.slotsFinished * 0.5f) > this.slots[i] % (0.25f + (float)this.slotsFinished * 0.5f))
					{
						Game1.playSound("shiny4");
					}
					if (old > this.slots[i])
					{
						this.spinsCount++;
					}
				}
				if (this.spinsCount > 0 && i == this.slotsFinished && Math.Abs(this.slots[i] - this.slotResults[i]) <= (float)time.ElapsedGameTime.Milliseconds * 0.008f)
				{
					this.slots[i] = this.slotResults[i];
					this.slotsFinished++;
					this.spinsCount--;
					Game1.playSound("Cowboy_gunshot");
				}
			}
			if (this.slotsFinished >= 3)
			{
				this.endTimer = ((this.payoutModifier == 0f) ? 600 : 1000);
			}
		}
		if (this.endTimer > 0)
		{
			this.endTimer -= time.ElapsedGameTime.Milliseconds;
			if (this.endTimer <= 0)
			{
				this.spinning = false;
				this.spinsCount = 0;
				this.slotsFinished = 0;
				if (this.payoutModifier > 0f)
				{
					this.showResult = true;
					Game1.playSound((!(this.payoutModifier >= 5f)) ? "newArtifact" : ((this.payoutModifier >= 10f) ? "reward" : "money"));
				}
				else
				{
					Game1.playSound("breathout");
				}
				Game1.player.clubCoins += (int)((float)this.currentBet * this.payoutModifier);
				if (this.payoutModifier == 2500f)
				{
					Game1.multiplayer.globalChatInfoMessage("Jackpot", Game1.player.Name);
				}
			}
		}
		this.spinButton10.scale = ((!this.spinning && this.spinButton10.bounds.Contains(Game1.getOldMouseX(), Game1.getOldMouseY())) ? 1.05f : 1f);
		this.spinButton100.scale = ((!this.spinning && this.spinButton100.bounds.Contains(Game1.getOldMouseX(), Game1.getOldMouseY())) ? 1.05f : 1f);
		this.doneButton.scale = ((!this.spinning && this.doneButton.bounds.Contains(Game1.getOldMouseX(), Game1.getOldMouseY())) ? 1.05f : 1f);
		return false;
	}

	public void receiveLeftClick(int x, int y, bool playSound = true)
	{
		if (!this.spinning && Game1.player.clubCoins >= 10 && this.spinButton10.bounds.Contains(x, y))
		{
			Club.timesPlayedSlots++;
			this.setSlotResults(this.slotResults);
			this.spinning = true;
			Game1.playSound("bigSelect");
			this.currentBet = 10;
			this.slotsFinished = 0;
			this.spinsCount = 0;
			this.showResult = false;
			Game1.player.clubCoins -= 10;
		}
		if (!this.spinning && Game1.player.clubCoins >= 100 && this.spinButton100.bounds.Contains(x, y))
		{
			Club.timesPlayedSlots++;
			this.setSlotResults(this.slotResults);
			Game1.playSound("bigSelect");
			this.spinning = true;
			this.slotsFinished = 0;
			this.spinsCount = 0;
			this.showResult = false;
			this.currentBet = 100;
			Game1.player.clubCoins -= 100;
		}
		if (!this.spinning && this.doneButton.bounds.Contains(x, y))
		{
			Game1.playSound("bigDeSelect");
			Game1.currentMinigame = null;
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

	public bool overrideFreeMouseMovement()
	{
		return Game1.options.SnappyMenus;
	}

	public void receiveKeyPress(Keys k)
	{
		if (!this.spinning && (k.Equals(Keys.Escape) || Game1.options.doesInputListContain(Game1.options.menuButton, k)))
		{
			this.unload();
			Game1.playSound("bigDeSelect");
			Game1.currentMinigame = null;
		}
		else
		{
			if (this.spinning || this.currentlySnappedComponent == null)
			{
				return;
			}
			if (Game1.options.doesInputListContain(Game1.options.moveDownButton, k))
			{
				if (this.currentlySnappedComponent.Equals(this.spinButton10))
				{
					this.currentlySnappedComponent = this.spinButton100;
					Game1.setMousePosition(this.currentlySnappedComponent.bounds.Center);
				}
				else if (this.currentlySnappedComponent.Equals(this.spinButton100))
				{
					this.currentlySnappedComponent = this.doneButton;
					Game1.setMousePosition(this.currentlySnappedComponent.bounds.Center);
				}
			}
			else if (Game1.options.doesInputListContain(Game1.options.moveUpButton, k))
			{
				if (this.currentlySnappedComponent.Equals(this.doneButton))
				{
					this.currentlySnappedComponent = this.spinButton100;
					Game1.setMousePosition(this.currentlySnappedComponent.bounds.Center);
				}
				else if (this.currentlySnappedComponent.Equals(this.spinButton100))
				{
					this.currentlySnappedComponent = this.spinButton10;
					Game1.setMousePosition(this.currentlySnappedComponent.bounds.Center);
				}
			}
		}
	}

	public void receiveKeyRelease(Keys k)
	{
	}

	public int getIconIndex(int index)
	{
		return index switch
		{
			0 => 24, 
			1 => 186, 
			2 => 138, 
			3 => 392, 
			4 => 254, 
			5 => 434, 
			6 => 72, 
			7 => 638, 
			_ => 24, 
		};
	}

	public void draw(SpriteBatch b)
	{
		b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
		b.Draw(Game1.staminaRect, new Rectangle(0, 0, Game1.graphics.GraphicsDevice.Viewport.Width, Game1.graphics.GraphicsDevice.Viewport.Height), new Color(38, 0, 7));
		b.Draw(Game1.mouseCursors, Utility.getTopLeftPositionForCenteringOnScreen(Game1.viewport, 228, 52, 0, -256), new Rectangle(441, 424, 66, 13), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.99f);
		int minSlotX = Game1.graphics.GraphicsDevice.Viewport.Width / 2 - 112;
		for (int i = 0; i < 3; i++)
		{
			Vector2 topLeft = new Vector2(minSlotX + i * 104, Game1.graphics.GraphicsDevice.Viewport.Height / 2 - 128);
			b.Draw(Game1.mouseCursors, topLeft, new Rectangle(306, 320, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.99f);
			float faceValue = (this.slots[i] + 1f) % 8f;
			int previous = this.getIconIndex(((int)faceValue + 8 - 1) % 8);
			int current = this.getIconIndex((previous + 1) % 8);
			b.Draw(Game1.objectSpriteSheet, topLeft - new Vector2(0f, -64f * (faceValue % 1f)), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, previous, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.99f);
			b.Draw(Game1.objectSpriteSheet, topLeft - new Vector2(0f, 64f - 64f * (faceValue % 1f)), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, current, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.99f);
			b.Draw(Game1.mouseCursors, new Vector2(Game1.graphics.GraphicsDevice.Viewport.Width / 2 - 132 + i * 26 * 4, Game1.graphics.GraphicsDevice.Viewport.Height / 2 - 192), new Rectangle(415, 385, 26, 48), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.99f);
		}
		int slotMiddle = minSlotX + 136;
		this.spinButton10.bounds.X = slotMiddle - this.spinButton10.bounds.Width / 2;
		this.spinButton100.bounds.X = slotMiddle - this.spinButton100.bounds.Width / 2;
		this.doneButton.bounds.X = slotMiddle - this.doneButton.bounds.Width / 2;
		int extraButtonWidth = this.GetButtonSizeOffset();
		b.Draw(Game1.mouseCursors, new Vector2(this.spinButton10.bounds.X, this.spinButton10.bounds.Y), new Rectangle(441, 385, 32 + extraButtonWidth, 13), Color.White * ((!this.spinning && Game1.player.clubCoins >= 10) ? 1f : 0.5f), 0f, Vector2.Zero, 4f * this.spinButton10.scale, SpriteEffects.None, 0.99f);
		b.Draw(Game1.mouseCursors, new Vector2(this.spinButton100.bounds.X, this.spinButton100.bounds.Y), new Rectangle(441, 398, 37 + extraButtonWidth, 13), Color.White * ((!this.spinning && Game1.player.clubCoins >= 100) ? 1f : 0.5f), 0f, Vector2.Zero, 4f * this.spinButton100.scale, SpriteEffects.None, 0.99f);
		b.Draw(Game1.mouseCursors, new Vector2(this.doneButton.bounds.X, this.doneButton.bounds.Y), new Rectangle(441, 411, 30 + extraButtonWidth, 13), Color.White * ((!this.spinning) ? 1f : 0.5f), 0f, Vector2.Zero, 4f * this.doneButton.scale, SpriteEffects.None, 0.99f);
		SpriteText.drawStringWithScrollBackground(b, this.coinBuffer + Game1.player.clubCoins, Game1.graphics.GraphicsDevice.Viewport.Width / 2 - 376, Game1.graphics.GraphicsDevice.Viewport.Height / 2 - 120);
		Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2(Game1.graphics.GraphicsDevice.Viewport.Width / 2 - 376 + 4, Game1.graphics.GraphicsDevice.Viewport.Height / 2 - 120 + 4), new Rectangle(211, 373, 9, 10), Color.White, 0f, Vector2.Zero, 4f, flipped: false, 1f);
		if (this.showResult)
		{
			SpriteText.drawString(b, "+" + this.payoutModifier * (float)this.currentBet, Game1.graphics.GraphicsDevice.Viewport.Width / 2 - 372, this.spinButton10.bounds.Y - 64 + 8, 9999, -1, 9999, 1f, 1f, junimoText: false, -1, "", SpriteText.color_White);
		}
		Vector2 basePos = new Vector2(Game1.graphics.GraphicsDevice.Viewport.Width / 2 + 200, Game1.graphics.GraphicsDevice.Viewport.Height / 2 - 352);
		IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(375, 357, 3, 3), (int)basePos.X, (int)basePos.Y, 384, 704, Color.White, 4f);
		b.Draw(Game1.objectSpriteSheet, basePos + new Vector2(8f, 8f), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, this.getIconIndex(7), 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.99f);
		SpriteText.drawString(b, "x2", (int)basePos.X + 192 + 16, (int)basePos.Y + 24, 9999, -1, 99999, 1f, 0.88f, junimoText: false, -1, "", SpriteText.color_White);
		b.Draw(Game1.objectSpriteSheet, basePos + new Vector2(8f, 76f), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, this.getIconIndex(7), 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.99f);
		b.Draw(Game1.objectSpriteSheet, basePos + new Vector2(76f, 76f), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, this.getIconIndex(7), 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.99f);
		SpriteText.drawString(b, "x3", (int)basePos.X + 192 + 16, (int)basePos.Y + 68 + 24, 9999, -1, 99999, 1f, 0.88f, junimoText: false, -1, "", SpriteText.color_White);
		for (int j = 0; j < 8; j++)
		{
			int which = j;
			switch (j)
			{
			case 5:
				which = 7;
				break;
			case 7:
				which = 5;
				break;
			}
			b.Draw(Game1.objectSpriteSheet, basePos + new Vector2(8f, 8 + (j + 2) * 68), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, this.getIconIndex(which), 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.99f);
			b.Draw(Game1.objectSpriteSheet, basePos + new Vector2(76f, 8 + (j + 2) * 68), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, this.getIconIndex(which), 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.99f);
			b.Draw(Game1.objectSpriteSheet, basePos + new Vector2(144f, 8 + (j + 2) * 68), Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, this.getIconIndex(which), 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.99f);
			int payout = 0;
			switch (j)
			{
			case 0:
				payout = 5;
				break;
			case 1:
				payout = 30;
				break;
			case 2:
				payout = 80;
				break;
			case 3:
				payout = 120;
				break;
			case 4:
				payout = 200;
				break;
			case 5:
				payout = 500;
				break;
			case 6:
				payout = 1000;
				break;
			case 7:
				payout = 2500;
				break;
			}
			SpriteText.drawString(b, "x" + payout, (int)basePos.X + 192 + 16, (int)basePos.Y + (j + 2) * 68 + 24, 9999, -1, 99999, 1f, 0.88f, junimoText: false, -1, "", SpriteText.color_White);
		}
		IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(379, 357, 3, 3), (int)basePos.X - 640, (int)basePos.Y, 1024, 704, Color.Red, 4f, drawShadow: false);
		for (int k = 1; k < 8; k++)
		{
			IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(379, 357, 3, 3), (int)basePos.X - 640 - 4 * k, (int)basePos.Y - 4 * k, 1024 + 8 * k, 704 + 8 * k, Color.Red * (1f - (float)k * 0.15f), 4f, drawShadow: false);
		}
		for (int l = 0; l < 17; l++)
		{
			IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(147, 472, 3, 3), (int)basePos.X - 640 + 8, (int)basePos.Y + l * 4 * 3 + 12, (int)(608f - (float)(l * 64) * 1.2f + (float)(l * l * 4) * 0.7f), 4, new Color(l * 25, (l > 8) ? (l * 10) : 0, 255 - l * 25), 4f, drawShadow: false);
		}
		if (Game1.IsMultiplayer)
		{
			Utility.drawTextWithColoredShadow(b, Game1.getTimeOfDayString(Game1.timeOfDay), Game1.dialogueFont, new Vector2(basePos.X + 416f - Game1.dialogueFont.MeasureString(Game1.getTimeOfDayString(Game1.timeOfDay)).X, basePos.Y - 72f), Color.Purple, Color.Black * 0.2f);
		}
		if (!Game1.options.hardwareCursor)
		{
			b.Draw(Game1.mouseCursors, new Vector2(Game1.getMouseX(), Game1.getMouseY()), Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 0, 16, 16), Color.White, 0f, Vector2.Zero, 4f + Game1.dialogueButtonScale / 150f, SpriteEffects.None, 1f);
		}
		b.End();
	}

	public void changeScreenSize()
	{
	}

	public void unload()
	{
	}

	public void receiveEventPoke(int data)
	{
	}

	public string minigameId()
	{
		return "Slots";
	}

	public bool doMainGameUpdates()
	{
		return false;
	}

	public bool forceQuit()
	{
		if (this.spinning)
		{
			Game1.player.clubCoins += this.currentBet;
		}
		this.unload();
		return true;
	}

	/// <summary>Get the pixel amount to add to the spin button widths to account for longer translated text.</summary>
	public int GetButtonSizeOffset()
	{
		return Game1.content.GetCurrentLanguage() switch
		{
			LocalizedContentManager.LanguageCode.de => 3, 
			LocalizedContentManager.LanguageCode.fr => 6, 
			LocalizedContentManager.LanguageCode.hu => 4, 
			LocalizedContentManager.LanguageCode.it => 2, 
			LocalizedContentManager.LanguageCode.pt => 10, 
			LocalizedContentManager.LanguageCode.ru => 9, 
			_ => 0, 
		};
	}
}
