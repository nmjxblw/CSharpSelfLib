using System;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.BellsAndWhistles;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Quests;

namespace StardewValley.Menus;

public class DayTimeMoneyBox : IClickableMenu
{
	public new const int width = 300;

	public new const int height = 284;

	public Vector2 position;

	private Rectangle sourceRect;

	public MoneyDial moneyDial = new MoneyDial(8);

	public int timeShakeTimer;

	public int moneyShakeTimer;

	public int questPulseTimer;

	public int whenToPulseTimer;

	public ClickableTextureComponent questButton;

	public ClickableTextureComponent zoomOutButton;

	public ClickableTextureComponent zoomInButton;

	private StringBuilder _hoverText = new StringBuilder();

	private StringBuilder _timeText = new StringBuilder();

	private StringBuilder _dateText = new StringBuilder();

	private StringBuilder _hours = new StringBuilder();

	private StringBuilder _padZeros = new StringBuilder();

	private StringBuilder _temp = new StringBuilder();

	private int _lastDayOfMonth = -1;

	private string _lastDayOfMonthString;

	private string _amString;

	private string _pmString;

	private int questNotificationTimer;

	private Texture2D questPingTexture;

	private Rectangle questPingSourceRect;

	private string questPingString;

	private int goldCoinCounter;

	private int goldCoinTimer;

	private string goldCoinString;

	private LocalizedContentManager.LanguageCode _languageCode = (LocalizedContentManager.LanguageCode)(-1);

	public bool questsDirty;

	public int questPingTimer;

	public static int Width => 300;

	public DayTimeMoneyBox()
		: base(Game1.uiViewport.Width - 300 + 32, 8, 300, 284)
	{
		this.position = new Vector2(base.xPositionOnScreen, base.yPositionOnScreen);
		this.sourceRect = new Rectangle(333, 431, 71, 43);
		this.questButton = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen + 220, base.yPositionOnScreen + 240, 44, 46), Game1.mouseCursors, new Rectangle(383, 493, 11, 14), 4f);
		this.zoomOutButton = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen + 92, base.yPositionOnScreen + 244, 28, 32), Game1.mouseCursors, new Rectangle(177, 345, 7, 8), 4f);
		this.zoomInButton = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen + 124, base.yPositionOnScreen + 244, 28, 32), Game1.mouseCursors, new Rectangle(184, 345, 7, 8), 4f);
		this.questButton = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen + 220, base.yPositionOnScreen + 240, 44, 46), Game1.mouseCursors, new Rectangle(383, 493, 11, 14), 4f);
		this.zoomOutButton = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen + 92, base.yPositionOnScreen + 244, 28, 32), Game1.mouseCursors, new Rectangle(177, 345, 7, 8), 4f);
		this.zoomInButton = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen + 124, base.yPositionOnScreen + 244, 28, 32), Game1.mouseCursors, new Rectangle(184, 345, 7, 8), 4f);
	}

	public override bool isWithinBounds(int x, int y)
	{
		if (Game1.options.zoomButtons && (this.zoomInButton.containsPoint(x, y) || this.zoomOutButton.containsPoint(x, y)))
		{
			return true;
		}
		if (Game1.player.hasVisibleQuests && this.questButton.containsPoint(x, y))
		{
			return true;
		}
		return false;
	}

	/// <inheritdoc />
	public override void receiveLeftClick(int x, int y, bool playSound = true)
	{
		if (Game1.player.hasVisibleQuests && this.questButton.containsPoint(x, y) && Game1.player.CanMove && !Game1.dialogueUp && !Game1.eventUp && Game1.farmEvent == null)
		{
			Game1.activeClickableMenu = new QuestLog();
		}
		if (Game1.options.zoomButtons)
		{
			if (this.zoomInButton.containsPoint(x, y) && Game1.options.desiredBaseZoomLevel < 2f)
			{
				int zoom = (int)Math.Round(Game1.options.desiredBaseZoomLevel * 100f);
				zoom -= zoom % 5;
				zoom += 5;
				Game1.options.desiredBaseZoomLevel = Math.Min(2f, (float)zoom / 100f);
				Game1.forceSnapOnNextViewportUpdate = true;
				Game1.playSound("drumkit6");
			}
			else if (this.zoomOutButton.containsPoint(x, y) && Game1.options.desiredBaseZoomLevel > 0.75f)
			{
				int zoom2 = (int)Math.Round(Game1.options.desiredBaseZoomLevel * 100f);
				zoom2 -= zoom2 % 5;
				zoom2 -= 5;
				Game1.options.desiredBaseZoomLevel = Math.Max(0.75f, (float)zoom2 / 100f);
				Game1.forceSnapOnNextViewportUpdate = true;
				Program.gamePtr.refreshWindowSettings();
				Game1.playSound("drumkit6");
			}
		}
	}

	public void gotGoldCoin(int amount)
	{
		this.goldCoinCounter += amount;
		this.goldCoinTimer = 4000;
		this.goldCoinString = "+" + Game1.content.LoadString("Strings\\StringsFromCSFiles:LoadGameMenu.cs.11020", this.goldCoinCounter);
	}

	public void pingQuest(Quest quest)
	{
		if (!quest.dailyQuest.Value)
		{
			return;
		}
		this.questNotificationTimer = 3000;
		this.questPingString = null;
		if (!(quest is SlayMonsterQuest monsterQuest))
		{
			if (!(quest is ResourceCollectionQuest resourceQuest))
			{
				if (!(quest is FishingQuest fishingQuest))
				{
					if (quest is SocializeQuest socializeQuest)
					{
						this.questPingTexture = Game1.mouseCursors_1_6;
						this.questPingSourceRect = new Rectangle(298, 237, 12, 12);
						if (socializeQuest.whoToGreet.Count != 0)
						{
							this.questPingString = socializeQuest.total.Value - socializeQuest.whoToGreet.Count + "/" + socializeQuest.total.Value;
						}
					}
					else
					{
						this.questNotificationTimer = 0;
					}
				}
				else
				{
					ParsedItemData data = ItemRegistry.GetData(fishingQuest.ItemId.Value);
					this.questPingTexture = data.GetTexture();
					this.questPingSourceRect = data.GetSourceRect();
					if (fishingQuest.numberFished.Value != fishingQuest.numberToFish.Value)
					{
						this.questPingString = fishingQuest.numberFished.Value + "/" + fishingQuest.numberToFish.Value;
					}
				}
			}
			else
			{
				ParsedItemData data2 = ItemRegistry.GetData(resourceQuest.ItemId.Value);
				this.questPingTexture = data2.GetTexture();
				this.questPingSourceRect = data2.GetSourceRect();
				if (resourceQuest.numberCollected.Value != resourceQuest.number.Value)
				{
					this.questPingString = resourceQuest.numberCollected.Value + "/" + resourceQuest.number.Value;
				}
			}
		}
		else if (monsterQuest.monster.Value?.Sprite?.Texture != null && monsterQuest.monsterName != null)
		{
			this.questPingTexture = monsterQuest.monster.Value.Sprite.Texture;
			this.questPingSourceRect = new Rectangle(0, 5, 16, 16);
			if (monsterQuest.monsterName.Equals("Green Slime"))
			{
				this.questPingSourceRect = new Rectangle(0, 264, 16, 16);
			}
			else if (monsterQuest.monsterName.Value.Contains("Frost"))
			{
				this.questPingSourceRect = new Rectangle(16, 264, 16, 16);
			}
			else if (monsterQuest.monsterName.Value.Contains("Sludge"))
			{
				this.questPingSourceRect = new Rectangle(32, 264, 16, 16);
			}
			else if (monsterQuest.monsterName.Value.Equals("Dust Spirit"))
			{
				this.questPingSourceRect.Y = 9;
			}
			else if (monsterQuest.monsterName.Value.Contains("Crab"))
			{
				this.questPingSourceRect = new Rectangle(48, 106, 16, 16);
			}
			else if (monsterQuest.monsterName.Value.Contains("Duggy"))
			{
				this.questPingSourceRect = new Rectangle(0, 32, 16, 16);
			}
			else if (monsterQuest.monsterName.Equals("Squid Kid"))
			{
				this.questPingSourceRect = new Rectangle(0, 0, 16, 16);
			}
			if (monsterQuest.numberToKill.Value != monsterQuest.numberKilled.Value)
			{
				this.questPingString = monsterQuest.numberKilled.Value + "/" + monsterQuest.numberToKill.Value;
			}
		}
		else
		{
			this.questNotificationTimer = 0;
		}
	}

	/// <inheritdoc />
	public override void receiveRightClick(int x, int y, bool playSound = true)
	{
		this.updatePosition();
	}

	/// <inheritdoc />
	public override void performHoverAction(int x, int y)
	{
		this.updatePosition();
		if (Game1.player.hasVisibleQuests && this.questButton.containsPoint(x, y))
		{
			this._hoverText.Clear();
			if (Game1.options.gamepadControls)
			{
				this._hoverText.Append(Game1.content.LoadString("Strings\\UI:QuestButton_Hover_Console"));
			}
			else
			{
				this._hoverText.Append(Game1.content.LoadString("Strings\\UI:QuestButton_Hover", Game1.options.journalButton[0].ToString()));
			}
		}
		if (Game1.options.zoomButtons)
		{
			if (this.zoomInButton.containsPoint(x, y))
			{
				this._hoverText.Clear();
				this._hoverText.Append(Game1.content.LoadString("Strings\\UI:ZoomInButton_Hover"));
			}
			else if (this.zoomOutButton.containsPoint(x, y))
			{
				this._hoverText.Clear();
				this._hoverText.Append(Game1.content.LoadString("Strings\\UI:ZoomOutButton_Hover"));
			}
		}
	}

	public void drawMoneyBox(SpriteBatch b, int overrideX = -1, int overrideY = -1)
	{
		this.updatePosition();
		b.Draw(Game1.mouseCursors, ((overrideY != -1) ? new Vector2((overrideX == -1) ? this.position.X : ((float)overrideX), overrideY - 172) : this.position) + new Vector2(28 + ((this.moneyShakeTimer > 0) ? Game1.random.Next(-3, 4) : 0), 172 + ((this.moneyShakeTimer > 0) ? Game1.random.Next(-3, 4) : 0)), new Rectangle(340, 472, 65, 17), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.9f);
		this.moneyDial.draw(b, ((overrideY != -1) ? new Vector2((overrideX == -1) ? this.position.X : ((float)overrideX), overrideY - 172) : this.position) + new Vector2(68 + ((this.moneyShakeTimer > 0) ? Game1.random.Next(-3, 4) : 0), 196 + ((this.moneyShakeTimer > 0) ? Game1.random.Next(-3, 4) : 0)), Game1.player.Money);
		if (this.moneyShakeTimer > 0)
		{
			this.moneyShakeTimer -= Game1.currentGameTime.ElapsedGameTime.Milliseconds;
		}
	}

	/// <inheritdoc />
	public override void update(GameTime time)
	{
		base.update(time);
		if (this._languageCode != LocalizedContentManager.CurrentLanguageCode)
		{
			this._languageCode = LocalizedContentManager.CurrentLanguageCode;
			this._amString = Game1.content.LoadString("Strings\\StringsFromCSFiles:DayTimeMoneyBox.cs.10370");
			this._pmString = Game1.content.LoadString("Strings\\StringsFromCSFiles:DayTimeMoneyBox.cs.10371");
		}
		if (this.questPingTimer > 0)
		{
			this.questPingTimer -= (int)time.ElapsedGameTime.TotalMilliseconds;
		}
		if (this.questPingTimer < 0)
		{
			this.questPingTimer = 0;
		}
		if (this.questNotificationTimer > 0)
		{
			this.questNotificationTimer -= (int)time.ElapsedGameTime.TotalMilliseconds;
		}
		if (this.goldCoinTimer > 0)
		{
			this.goldCoinTimer -= (int)time.ElapsedGameTime.TotalMilliseconds;
			if (this.goldCoinTimer <= 0)
			{
				this.goldCoinCounter = 0;
			}
		}
		if (this.questsDirty)
		{
			if (Game1.player.hasPendingCompletedQuests)
			{
				this.PingQuestLog();
			}
			this.questsDirty = false;
		}
	}

	public virtual void PingQuestLog()
	{
		this.questPingTimer = 6000;
	}

	public virtual void DismissQuestPing()
	{
		this.questPingTimer = 0;
	}

	/// <inheritdoc />
	public override void draw(SpriteBatch b)
	{
		SpriteFont font = ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ko) ? Game1.smallFont : Game1.dialogueFont);
		this.updatePosition();
		if (this.timeShakeTimer > 0)
		{
			this.timeShakeTimer -= Game1.currentGameTime.ElapsedGameTime.Milliseconds;
		}
		if (this.questPulseTimer > 0)
		{
			this.questPulseTimer -= Game1.currentGameTime.ElapsedGameTime.Milliseconds;
		}
		if (this.whenToPulseTimer >= 0)
		{
			this.whenToPulseTimer -= Game1.currentGameTime.ElapsedGameTime.Milliseconds;
			if (this.whenToPulseTimer <= 0)
			{
				this.whenToPulseTimer = 3000;
				if (Game1.player.hasNewQuestActivity())
				{
					this.questPulseTimer = 1000;
				}
			}
		}
		b.Draw(Game1.mouseCursors, this.position, this.sourceRect, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.9f);
		if (Game1.dayOfMonth != this._lastDayOfMonth)
		{
			this._lastDayOfMonth = Game1.dayOfMonth;
			this._lastDayOfMonthString = Game1.shortDayDisplayNameFromDayOfSeason(this._lastDayOfMonth);
		}
		this._dateText.Clear();
		switch (LocalizedContentManager.CurrentLanguageCode)
		{
		case LocalizedContentManager.LanguageCode.ja:
			this._dateText.AppendEx(Game1.dayOfMonth);
			this._dateText.Append("日 (");
			this._dateText.Append(this._lastDayOfMonthString);
			this._dateText.Append(")");
			break;
		case LocalizedContentManager.LanguageCode.zh:
			this._dateText.AppendEx(Game1.dayOfMonth);
			this._dateText.Append("日 ");
			this._dateText.Append(this._lastDayOfMonthString);
			break;
		case LocalizedContentManager.LanguageCode.mod:
			this._dateText.Append(LocalizedContentManager.CurrentModLanguage.ClockDateFormat.Replace("[DAY_OF_WEEK]", this._lastDayOfMonthString).Replace("[DAY_OF_MONTH]", Game1.dayOfMonth.ToString()));
			break;
		default:
			this._dateText.Append(this._lastDayOfMonthString);
			this._dateText.Append(". ");
			this._dateText.AppendEx(Game1.dayOfMonth);
			break;
		}
		Vector2 daySize = font.MeasureString(this._dateText);
		Vector2 dayPosition = new Vector2((float)this.sourceRect.X * 0.5625f - daySize.X / 2f, (float)this.sourceRect.Y * (LocalizedContentManager.CurrentLanguageLatin ? 0.1f : 0.1f) - daySize.Y / 2f);
		Utility.drawTextWithShadow(b, this._dateText, font, this.position + dayPosition, Game1.textColor);
		b.Draw(Game1.mouseCursors, this.position + new Vector2(212f, 68f), new Rectangle(406, 441 + Game1.seasonIndex * 8, 12, 8), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.9f);
		if (Game1.weatherIcon == 999)
		{
			b.Draw(Game1.mouseCursors_1_6, this.position + new Vector2(116f, 68f), new Rectangle(243, 293, 12, 8), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.9f);
		}
		else
		{
			b.Draw(Game1.mouseCursors, this.position + new Vector2(116f, 68f), new Rectangle(317 + 12 * Game1.weatherIcon, 421, 12, 8), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.9f);
		}
		this._padZeros.Clear();
		if (Game1.timeOfDay % 100 == 0)
		{
			this._padZeros.Append("0");
		}
		this._hours.Clear();
		switch (LocalizedContentManager.CurrentLanguageCode)
		{
		case LocalizedContentManager.LanguageCode.ru:
		case LocalizedContentManager.LanguageCode.zh:
		case LocalizedContentManager.LanguageCode.pt:
		case LocalizedContentManager.LanguageCode.es:
		case LocalizedContentManager.LanguageCode.de:
		case LocalizedContentManager.LanguageCode.th:
		case LocalizedContentManager.LanguageCode.fr:
		case LocalizedContentManager.LanguageCode.tr:
		case LocalizedContentManager.LanguageCode.hu:
			this._temp.Clear();
			this._temp.AppendEx(Game1.timeOfDay / 100 % 24);
			if (Game1.timeOfDay / 100 % 24 <= 9)
			{
				this._hours.Append("0");
			}
			this._hours.AppendEx(this._temp);
			break;
		default:
			if (Game1.timeOfDay / 100 % 12 == 0)
			{
				if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ja)
				{
					this._hours.Append("0");
				}
				else
				{
					this._hours.Append("12");
				}
			}
			else
			{
				this._hours.AppendEx(Game1.timeOfDay / 100 % 12);
			}
			break;
		}
		this._timeText.Clear();
		this._timeText.AppendEx(this._hours);
		this._timeText.Append(":");
		this._timeText.AppendEx(Game1.timeOfDay % 100);
		this._timeText.AppendEx(this._padZeros);
		switch (LocalizedContentManager.CurrentLanguageCode)
		{
		case LocalizedContentManager.LanguageCode.en:
		case LocalizedContentManager.LanguageCode.it:
			this._timeText.Append(" ");
			if (Game1.timeOfDay < 1200 || Game1.timeOfDay >= 2400)
			{
				this._timeText.Append(this._amString);
			}
			else
			{
				this._timeText.Append(this._pmString);
			}
			break;
		case LocalizedContentManager.LanguageCode.ko:
			if (Game1.timeOfDay < 1200 || Game1.timeOfDay >= 2400)
			{
				this._timeText.Append(this._amString);
			}
			else
			{
				this._timeText.Append(this._pmString);
			}
			break;
		case LocalizedContentManager.LanguageCode.ja:
			this._temp.Clear();
			this._temp.AppendEx(this._timeText);
			this._timeText.Clear();
			if (Game1.timeOfDay < 1200 || Game1.timeOfDay >= 2400)
			{
				this._timeText.Append(this._amString);
				this._timeText.Append(" ");
				this._timeText.AppendEx(this._temp);
			}
			else
			{
				this._timeText.Append(this._pmString);
				this._timeText.Append(" ");
				this._timeText.AppendEx(this._temp);
			}
			break;
		case LocalizedContentManager.LanguageCode.mod:
			this._timeText.Clear();
			this._timeText.Append(LocalizedContentManager.FormatTimeString(Game1.timeOfDay, LocalizedContentManager.CurrentModLanguage.ClockTimeFormat));
			break;
		}
		Vector2 txtSize = font.MeasureString(this._timeText);
		Vector2 timePosition = new Vector2((float)this.sourceRect.X * 0.55f - txtSize.X / 2f + (float)((this.timeShakeTimer > 0) ? Game1.random.Next(-2, 3) : 0), (float)this.sourceRect.Y * (LocalizedContentManager.CurrentLanguageLatin ? 0.31f : 0.31f) - txtSize.Y / 2f + (float)((this.timeShakeTimer > 0) ? Game1.random.Next(-2, 3) : 0));
		bool nofade = Game1.shouldTimePass() || Game1.fadeToBlack || Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 2000.0 > 1000.0;
		Utility.drawTextWithShadow(b, this._timeText, font, this.position + timePosition, (Game1.timeOfDay >= 2400) ? Color.Red : (Game1.textColor * (nofade ? 1f : 0.5f)));
		int adjustedTime = (int)((float)(Game1.timeOfDay - Game1.timeOfDay % 100) + (float)(Game1.timeOfDay % 100 / 10) * 16.66f);
		if (Game1.player.hasVisibleQuests)
		{
			this.questButton.draw(b);
			if (this.questPulseTimer > 0)
			{
				float scaleMult = 1f / (Math.Max(300f, Math.Abs(this.questPulseTimer % 1000 - 500)) / 500f);
				b.Draw(Game1.mouseCursors, new Vector2(this.questButton.bounds.X + 24, this.questButton.bounds.Y + 32) + ((scaleMult > 1f) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero), new Rectangle(395, 497, 3, 8), Color.White, 0f, new Vector2(2f, 4f), 4f * scaleMult, SpriteEffects.None, 0.99f);
			}
			if (this.questPingTimer > 0)
			{
				b.Draw(Game1.mouseCursors, new Vector2(Game1.dayTimeMoneyBox.questButton.bounds.Left - 16, Game1.dayTimeMoneyBox.questButton.bounds.Bottom + 8), new Rectangle(128 + ((this.questPingTimer / 200 % 2 != 0) ? 16 : 0), 208, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.9f);
			}
		}
		if (Game1.options.zoomButtons)
		{
			this.zoomInButton.draw(b, Color.White * ((Game1.options.desiredBaseZoomLevel >= 2f) ? 0.5f : 1f), 1f);
			this.zoomOutButton.draw(b, Color.White * ((Game1.options.desiredBaseZoomLevel <= 0.75f) ? 0.5f : 1f), 1f);
		}
		this.drawMoneyBox(b);
		if (this._hoverText.Length > 0 && this.isWithinBounds(Game1.getOldMouseX(), Game1.getOldMouseY()))
		{
			IClickableMenu.drawHoverText(b, this._hoverText, Game1.dialogueFont);
		}
		b.Draw(Game1.mouseCursors, this.position + new Vector2(88f, 88f), new Rectangle(324, 477, 7, 19), Color.White, (float)(Math.PI + Math.Min(Math.PI, (double)(((float)adjustedTime + (float)Game1.gameTimeInterval / (float)Game1.realMilliSecondsPerGameTenMinutes * 16.6f - 600f) / 2000f) * Math.PI)), new Vector2(3f, 17f), 4f, SpriteEffects.None, 0.9f);
		if (this.questNotificationTimer > 0)
		{
			Vector2 basePosition = this.position + new Vector2(27f, 76f) * 4f;
			b.Draw(Game1.mouseCursors_1_6, basePosition, new Rectangle(257, 228, 39, 18), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.9f);
			b.Draw(this.questPingTexture, basePosition + new Vector2(1f, 1f) * 4f, this.questPingSourceRect, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.91f);
			if (this.questPingString != null)
			{
				Utility.drawTextWithShadow(b, this.questPingString, Game1.smallFont, basePosition + new Vector2(27f, 9.5f) * 4f - Game1.smallFont.MeasureString(this.questPingString) * 0.5f, Game1.textColor);
			}
			else
			{
				b.Draw(Game1.mouseCursors_1_6, basePosition + new Vector2(22f, 5f) * 4f, new Rectangle(297, 229, 9, 8), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.91f);
			}
		}
		if (this.goldCoinTimer > 0)
		{
			SpriteText.drawSmallTextBubble(b, this.goldCoinString, this.position + new Vector2(5f, 73f) * 4f, -1, 0.99f, drawPointerOnTop: true);
		}
	}

	private void updatePosition()
	{
		this.position = new Vector2(Game1.uiViewport.Width - 300, 8f);
		if (Game1.isOutdoorMapSmallerThanViewport())
		{
			this.position = new Vector2(Math.Min(this.position.X, -Game1.uiViewport.X + Game1.currentLocation.map.Layers[0].LayerWidth * 64 - 300), 8f);
		}
		Utility.makeSafe(ref this.position, 300, 284);
		base.xPositionOnScreen = (int)this.position.X;
		base.yPositionOnScreen = (int)this.position.Y;
		this.questButton.bounds = new Rectangle(base.xPositionOnScreen + 212, base.yPositionOnScreen + 240, 44, 46);
		this.zoomOutButton.bounds = new Rectangle(base.xPositionOnScreen + 92, base.yPositionOnScreen + 244, 28, 32);
		this.zoomInButton.bounds = new Rectangle(base.xPositionOnScreen + 124, base.yPositionOnScreen + 244, 28, 32);
	}
}
