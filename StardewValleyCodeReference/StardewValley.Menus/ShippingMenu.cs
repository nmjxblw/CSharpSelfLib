using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using StardewValley.Extensions;

namespace StardewValley.Menus;

public class ShippingMenu : IClickableMenu
{
	public const int region_okbutton = 101;

	public const int region_forwardButton = 102;

	public const int region_backButton = 103;

	public const int farming_category = 0;

	public const int foraging_category = 1;

	public const int fishing_category = 2;

	public const int mining_category = 3;

	public const int other_category = 4;

	public const int total_category = 5;

	public const int timePerIntroCategory = 500;

	public const int outroFadeTime = 800;

	public const int smokeRate = 100;

	public const int categorylabelHeight = 25;

	public int itemsPerCategoryPage = 9;

	public int currentPage = -1;

	public int currentTab;

	public List<ClickableTextureComponent> categories = new List<ClickableTextureComponent>();

	public ClickableTextureComponent okButton;

	public ClickableTextureComponent forwardButton;

	public ClickableTextureComponent backButton;

	private List<int> categoryTotals = new List<int>();

	private List<MoneyDial> categoryDials = new List<MoneyDial>();

	private Dictionary<Item, int> itemValues = new Dictionary<Item, int>();

	private Dictionary<Item, int> singleItemValues = new Dictionary<Item, int>();

	private List<List<Item>> categoryItems = new List<List<Item>>();

	private int categoryLabelsWidth;

	private int plusButtonWidth;

	private int itemSlotWidth;

	private int itemAndPlusButtonWidth;

	private int totalWidth;

	private int centerX;

	private int centerY;

	private int introTimer = 3500;

	private int outroFadeTimer;

	private int outroPauseBeforeDateChange;

	private int finalOutroTimer;

	private int smokeTimer;

	private int dayPlaqueY;

	private int moonShake = -1;

	private int timesPokedMoon;

	private float weatherX;

	private bool outro;

	private bool newDayPlaque;

	private bool savedYet;

	public TemporaryAnimatedSpriteList animations = new TemporaryAnimatedSpriteList();

	private SaveGameMenu saveGameMenu;

	protected bool _hasFinished;

	public bool _activated;

	private bool wasGreenRain;

	public ShippingMenu(IList<Item> items)
		: base(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height)
	{
		this._activated = false;
		this.parseItems(items);
		if (!Game1.wasRainingYesterday)
		{
			Game1.changeMusicTrack(Game1.IsSummer ? "nightTime" : "none");
		}
		this.wasGreenRain = Utility.isGreenRainDay(Game1.dayOfMonth - 1, Game1.season);
		this.categoryLabelsWidth = 512;
		this.plusButtonWidth = 40;
		this.itemSlotWidth = 96;
		this.itemAndPlusButtonWidth = this.plusButtonWidth + this.itemSlotWidth + 8;
		this.totalWidth = this.categoryLabelsWidth + this.itemAndPlusButtonWidth;
		this.centerX = Game1.uiViewport.Width / 2;
		this.centerY = Game1.uiViewport.Height / 2;
		this._hasFinished = false;
		int xOffset = ((Game1.content.GetCurrentLanguage() == LocalizedContentManager.LanguageCode.ru) ? 64 : 0);
		int lastVisible = -1;
		for (int i = 0; i < 6; i++)
		{
			this.categories.Add(new ClickableTextureComponent("", new Rectangle(this.centerX + xOffset + this.totalWidth / 2 - this.plusButtonWidth, this.centerY - 300 + i * 27 * 4, this.plusButtonWidth, 44), "", this.getCategoryName(i), Game1.mouseCursors, new Rectangle(392, 361, 10, 11), 4f)
			{
				visible = (i < 5 && this.categoryItems[i].Count > 0),
				myID = i,
				downNeighborID = ((i < 4) ? (i + 1) : 101),
				upNeighborID = ((i > 0) ? lastVisible : (-1)),
				upNeighborImmutable = true
			});
			lastVisible = ((i < 5 && this.categoryItems[i].Count > 0) ? i : lastVisible);
		}
		this.dayPlaqueY = this.categories[0].bounds.Y - 128;
		this.okButton = new ClickableTextureComponent(bounds: new Rectangle(this.centerX + xOffset + this.totalWidth / 2 - this.itemAndPlusButtonWidth + 32, this.centerY + 300 - 64, 64, 64), name: Game1.content.LoadString("Strings\\StringsFromCSFiles:ShippingMenu.cs.11382"), label: null, hoverText: Game1.content.LoadString("Strings\\StringsFromCSFiles:ShippingMenu.cs.11382"), texture: Game1.mouseCursors, sourceRect: new Rectangle(128, 256, 64, 64), scale: 1f)
		{
			myID = 101,
			upNeighborID = lastVisible
		};
		this.backButton = new ClickableTextureComponent("", new Rectangle(base.xPositionOnScreen + 32, base.yPositionOnScreen + base.height - 64, 48, 44), null, "", Game1.mouseCursors, new Rectangle(352, 495, 12, 11), 4f)
		{
			myID = 103,
			rightNeighborID = -7777
		};
		this.forwardButton = new ClickableTextureComponent("", new Rectangle(base.xPositionOnScreen + base.width - 32 - 48, base.yPositionOnScreen + base.height - 64, 48, 44), null, "", Game1.mouseCursors, new Rectangle(365, 495, 12, 11), 4f)
		{
			myID = 102,
			leftNeighborID = 103
		};
		if (Game1.dayOfMonth == 25 && Game1.season == Season.Winter)
		{
			Vector2 startingPosition = new Vector2(Game1.uiViewport.Width, Game1.random.Next(0, 200));
			Rectangle sourceRect = new Rectangle(640, 800, 32, 16);
			int loops = 1000;
			TemporaryAnimatedSprite t = new TemporaryAnimatedSprite("LooseSprites\\Cursors", sourceRect, 80f, 2, loops, startingPosition, flicker: false, flipped: false, 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true)
			{
				motion = new Vector2(-4f, 0f),
				delayBeforeAnimationStart = 3000
			};
			this.animations.Add(t);
		}
		Game1.stats.checkForShippingAchievements();
		this.RepositionItems();
		this.populateClickableComponentList();
		if (Game1.options.SnappyMenus)
		{
			this.snapToDefaultClickableComponent();
		}
	}

	public void RepositionItems()
	{
		this.centerX = Game1.uiViewport.Width / 2;
		this.centerY = Game1.uiViewport.Height / 2;
		int boxwidth = Game1.uiViewport.Width;
		int boxheight = Game1.uiViewport.Height;
		boxwidth = Math.Min(base.width, 1280);
		boxheight = Math.Min(base.height, 920);
		int xOffset = ((Game1.content.GetCurrentLanguage() == LocalizedContentManager.LanguageCode.ru) ? 64 : 0);
		for (int i = 0; i < 6; i++)
		{
			this.categories[i].bounds = new Rectangle(this.centerX + xOffset + this.totalWidth / 2 - this.plusButtonWidth, this.centerY - 300 + i * 27 * 4, this.plusButtonWidth, 44);
		}
		this.dayPlaqueY = this.categories[0].bounds.Y - 128;
		if (this.dayPlaqueY < 0)
		{
			this.dayPlaqueY = -64;
		}
		this.backButton.bounds.X = this.centerX - boxwidth / 2 - 64;
		this.backButton.bounds.Y = this.centerY + boxheight / 2 - 48;
		if (this.backButton.bounds.X < 0)
		{
			this.backButton.bounds.X = base.xPositionOnScreen + 32;
		}
		if (this.backButton.bounds.Y > Game1.uiViewport.Height - 32)
		{
			this.backButton.bounds.Y = Game1.uiViewport.Height - 80;
		}
		this.forwardButton.bounds.X = this.centerX + boxwidth / 2 + 8;
		this.forwardButton.bounds.Y = this.centerY + boxheight / 2 - 48;
		if (this.forwardButton.bounds.X > Game1.uiViewport.Width - 32)
		{
			this.forwardButton.bounds.X = base.xPositionOnScreen + base.width - 32 - 48;
		}
		if (this.forwardButton.bounds.Y > Game1.uiViewport.Height - 32)
		{
			this.forwardButton.bounds.Y = Game1.uiViewport.Height - 80;
		}
		Rectangle okRect = new Rectangle(this.centerX + xOffset + this.totalWidth / 2 - this.itemAndPlusButtonWidth + 32, this.centerY + 300 - 64, 64, 64);
		this.okButton.bounds = okRect;
		int spaceHeight = Math.Min(base.height, 920);
		float item_space = base.yPositionOnScreen + spaceHeight - 64 - (base.yPositionOnScreen + 32);
		this.itemsPerCategoryPage = (int)(item_space / 68f);
		if (this.currentPage >= 0)
		{
			this.currentTab = Utility.Clamp(this.currentTab, 0, (this.categoryItems[this.currentPage].Count - 1) / this.itemsPerCategoryPage);
		}
	}

	protected override void customSnapBehavior(int direction, int oldRegion, int oldID)
	{
		if (oldID == 103 && direction == 1 && this.showForwardButton())
		{
			base.currentlySnappedComponent = base.getComponentWithID(102);
			this.snapCursorToCurrentSnappedComponent();
		}
	}

	public override void snapToDefaultClickableComponent()
	{
		if (this.currentPage != -1)
		{
			base.currentlySnappedComponent = base.getComponentWithID(103);
		}
		else
		{
			base.currentlySnappedComponent = base.getComponentWithID(101);
		}
		this.snapCursorToCurrentSnappedComponent();
	}

	public void parseItems(IList<Item> items)
	{
		Utility.consolidateStacks(items);
		for (int i = 0; i < 6; i++)
		{
			this.categoryItems.Add(new List<Item>());
			this.categoryTotals.Add(0);
			this.categoryDials.Add(new MoneyDial(7, i == 5));
		}
		foreach (Item item in items)
		{
			if (item != null)
			{
				int category = this.getCategoryIndexForObject(item);
				this.categoryItems[category].Add(item);
				int sellToStorePrice = item.sellToStorePrice(-1L);
				int price = sellToStorePrice * item.Stack;
				this.categoryTotals[category] += price;
				this.itemValues[item] = price;
				this.singleItemValues[item] = sellToStorePrice;
				Game1.stats.ItemsShipped += (uint)item.Stack;
				if (item.Category == -75 || item.Category == -79)
				{
					Game1.stats.CropsShipped += (uint)item.Stack;
				}
				if (item is Object o && o.countsForShippedCollection())
				{
					Game1.player.shippedBasic(o.ItemId, o.stack.Value);
				}
			}
		}
		for (int j = 0; j < 5; j++)
		{
			this.categoryTotals[5] += this.categoryTotals[j];
			this.categoryItems[5].AddRange(this.categoryItems[j]);
			this.categoryDials[j].currentValue = this.categoryTotals[j];
			this.categoryDials[j].previousTargetValue = this.categoryDials[j].currentValue;
		}
		this.categoryDials[5].currentValue = this.categoryTotals[5];
		Game1.setRichPresence("earnings", this.categoryTotals[5]);
	}

	public int getCategoryIndexForObject(Item item)
	{
		switch (item.QualifiedItemId)
		{
		case "(O)396":
		case "(O)406":
		case "(O)296":
		case "(O)402":
		case "(O)418":
		case "(O)414":
		case "(O)410":
			return 1;
		default:
			if (item is Object o && (o.preserve.Value == Object.PreserveType.SmokedFish || o.preserve.Value == Object.PreserveType.AgedRoe || o.preserve.Value == Object.PreserveType.Roe))
			{
				return 2;
			}
			switch (item.Category)
			{
			case -80:
			case -79:
			case -75:
			case -26:
			case -14:
			case -6:
			case -5:
				return 0;
			case -21:
			case -20:
			case -4:
				return 2;
			case -81:
			case -27:
			case -23:
				return 1;
			case -15:
			case -12:
			case -2:
				return 3;
			default:
				return 4;
			}
		}
	}

	public string getCategoryName(int index)
	{
		return index switch
		{
			0 => Game1.content.LoadString("Strings\\StringsFromCSFiles:ShippingMenu.cs.11389"), 
			1 => Game1.content.LoadString("Strings\\StringsFromCSFiles:ShippingMenu.cs.11390"), 
			2 => Game1.content.LoadString("Strings\\StringsFromCSFiles:ShippingMenu.cs.11391"), 
			3 => Game1.content.LoadString("Strings\\StringsFromCSFiles:ShippingMenu.cs.11392"), 
			4 => Game1.content.LoadString("Strings\\StringsFromCSFiles:ShippingMenu.cs.11393"), 
			5 => Game1.content.LoadString("Strings\\StringsFromCSFiles:ShippingMenu.cs.11394"), 
			_ => "", 
		};
	}

	/// <inheritdoc />
	public override void update(GameTime time)
	{
		base.update(time);
		if (!this._activated)
		{
			this._activated = true;
			Game1.player.team.endOfNightStatus.UpdateState("shipment");
		}
		if (this._hasFinished)
		{
			if (Game1.PollForEndOfNewDaySync())
			{
				base.exitThisMenu(playSound: false);
			}
			return;
		}
		if (this.saveGameMenu != null)
		{
			this.saveGameMenu.update(time);
			if (this.saveGameMenu.quit)
			{
				this.saveGameMenu = null;
				this.savedYet = true;
			}
		}
		this.weatherX += (float)time.ElapsedGameTime.Milliseconds * 0.03f;
		this.animations.RemoveWhere((TemporaryAnimatedSprite animation) => animation.update(time));
		if (this.outro)
		{
			if (this.outroFadeTimer > 0)
			{
				this.outroFadeTimer -= time.ElapsedGameTime.Milliseconds;
			}
			else if (this.outroFadeTimer <= 0 && this.dayPlaqueY < this.centerY - 64)
			{
				if (this.animations.Count > 0)
				{
					this.animations.Clear();
				}
				this.dayPlaqueY += (int)Math.Ceiling((float)time.ElapsedGameTime.Milliseconds * 0.35f);
				if (this.dayPlaqueY >= this.centerY - 64)
				{
					this.outroPauseBeforeDateChange = 700;
				}
			}
			else if (this.outroPauseBeforeDateChange > 0)
			{
				this.outroPauseBeforeDateChange -= time.ElapsedGameTime.Milliseconds;
				if (this.outroPauseBeforeDateChange <= 0)
				{
					this.newDayPlaque = true;
					Game1.playSound("newRecipe");
					if (Game1.season != Season.Winter && Game1.game1.IsMainInstance)
					{
						DelayedAction.playSoundAfterDelay(Game1.IsRainingHere() ? "rainsound" : "rooster", 1500);
					}
					this.finalOutroTimer = 2000;
					this.animations.Clear();
					if (!this.savedYet)
					{
						if (this.saveGameMenu == null)
						{
							this.saveGameMenu = new SaveGameMenu();
						}
						return;
					}
				}
			}
			else if (this.finalOutroTimer > 0 && this.savedYet)
			{
				this.finalOutroTimer -= time.ElapsedGameTime.Milliseconds;
				if (this.finalOutroTimer <= 0)
				{
					this._hasFinished = true;
				}
			}
		}
		if (this.introTimer >= 0)
		{
			int num = this.introTimer;
			this.introTimer -= time.ElapsedGameTime.Milliseconds * ((Game1.oldMouseState.LeftButton != ButtonState.Pressed) ? 1 : 3);
			if (num % 500 < this.introTimer % 500 && this.introTimer <= 3000)
			{
				int categoryThatPoppedUp = 4 - this.introTimer / 500;
				if (categoryThatPoppedUp < 6 && categoryThatPoppedUp > -1)
				{
					if (this.categoryItems[categoryThatPoppedUp].Count > 0)
					{
						Game1.playSound(this.getCategorySound(categoryThatPoppedUp));
						this.categoryDials[categoryThatPoppedUp].currentValue = 0;
						this.categoryDials[categoryThatPoppedUp].previousTargetValue = 0;
					}
					else
					{
						Game1.playSound("stoneStep");
					}
				}
			}
			if (this.introTimer < 0)
			{
				if (Game1.options.SnappyMenus)
				{
					this.snapToDefaultClickableComponent();
				}
				Game1.playSound("money");
				this.categoryDials[5].currentValue = 0;
				this.categoryDials[5].previousTargetValue = 0;
			}
		}
		else if (Game1.dayOfMonth != 28 && !this.outro)
		{
			if (!Game1.wasRainingYesterday)
			{
				Vector2 startingPosition = new Vector2(Game1.uiViewport.Width, Game1.random.Next(200));
				Rectangle sourceRect = new Rectangle(640, 752, 16, 16);
				int rows = Game1.random.Next(1, 4);
				if (Game1.random.NextDouble() < 0.001)
				{
					bool flip = Game1.random.NextBool();
					if (Game1.random.NextBool())
					{
						this.animations.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(640, 826, 16, 8), 40f, 4, 0, new Vector2(Game1.random.Next(this.centerX * 2), Game1.random.Next(this.centerY)), flicker: false, flip)
						{
							rotation = (float)Math.PI,
							scale = 4f,
							motion = new Vector2(flip ? (-8) : 8, 8f),
							local = true
						});
					}
					else
					{
						this.animations.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(258, 1680, 16, 16), 40f, 4, 0, new Vector2(Game1.random.Next(this.centerX * 2), Game1.random.Next(this.centerY)), flicker: false, flip)
						{
							scale = 4f,
							motion = new Vector2(flip ? (-8) : 8, 8f),
							local = true
						});
					}
				}
				else if (Game1.random.NextDouble() < 0.0002)
				{
					TemporaryAnimatedSprite bird = new TemporaryAnimatedSprite(position: new Vector2(Game1.uiViewport.Width, Game1.random.Next(4, 256)), textureName: "", sourceRect: new Rectangle(0, 0, 1, 1), animationInterval: 9999f, animationLength: 1, numberOfLoops: 10000, flicker: false, flipped: false, layerDepth: 0.01f, alphaFade: 0f, color: Color.White * (0.25f + (float)Game1.random.NextDouble()), scale: 4f, scaleChange: 0f, rotation: 0f, rotationChange: 0f, local: true);
					bird.motion = new Vector2(-0.25f, 0f);
					this.animations.Add(bird);
				}
				else if (Game1.random.NextDouble() < 5E-05)
				{
					startingPosition = new Vector2(Game1.uiViewport.Width, Game1.uiViewport.Height - 192);
					for (int i = 0; i < rows; i++)
					{
						TemporaryAnimatedSprite bird2 = new TemporaryAnimatedSprite("LooseSprites\\Cursors", sourceRect, Game1.random.Next(60, 101), 4, 100, startingPosition + new Vector2((i + 1) * Game1.random.Next(15, 18), (i + 1) * -20), flicker: false, flipped: false, 0.01f, 0f, Color.Black, 4f, 0f, 0f, 0f, local: true);
						bird2.motion = new Vector2(-1f, 0f);
						this.animations.Add(bird2);
						bird2 = new TemporaryAnimatedSprite("LooseSprites\\Cursors", sourceRect, Game1.random.Next(60, 101), 4, 100, startingPosition + new Vector2((i + 1) * Game1.random.Next(15, 18), (i + 1) * 20), flicker: false, flipped: false, 0.01f, 0f, Color.Black, 4f, 0f, 0f, 0f, local: true);
						bird2.motion = new Vector2(-1f, 0f);
						this.animations.Add(bird2);
					}
				}
				else if (Game1.random.NextDouble() < 1E-05)
				{
					sourceRect = new Rectangle(640, 784, 16, 16);
					TemporaryAnimatedSprite t = new TemporaryAnimatedSprite("LooseSprites\\Cursors", sourceRect, 75f, 4, 1000, startingPosition, flicker: false, flipped: false, 0.01f, 0f, Color.White, 4f, 0f, 0f, 0f, local: true);
					t.motion = new Vector2(-3f, 0f);
					t.yPeriodic = true;
					t.yPeriodicLoopTime = 1000f;
					t.yPeriodicRange = 8f;
					t.shakeIntensity = 0.5f;
					this.animations.Add(t);
				}
			}
			this.smokeTimer -= time.ElapsedGameTime.Milliseconds;
			if (this.smokeTimer <= 0)
			{
				this.smokeTimer = 50;
				this.animations.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(684, 1075, 1, 1), 1000f, 1, 1000, new Vector2(188f, Game1.uiViewport.Height - 128 + 20), flicker: false, flipped: false)
				{
					color = (Game1.wasRainingYesterday ? Color.SlateGray : Color.White),
					scale = 4f,
					scaleChange = 0f,
					alphaFade = 0.0025f,
					motion = new Vector2(0f, (float)(-Game1.random.Next(25, 75)) / 100f / 4f),
					acceleration = new Vector2(-0.001f, 0f)
				});
			}
		}
		if (this.moonShake > 0)
		{
			this.moonShake -= time.ElapsedGameTime.Milliseconds;
		}
	}

	public string getCategorySound(int which)
	{
		switch (which)
		{
		case 0:
			if ((!((this.categoryItems[0][0] as Object)?.isAnimalProduct())) ?? true)
			{
				return "harvest";
			}
			return "cluck";
		case 2:
			return "button1";
		case 3:
			return "hammer";
		case 1:
			return "leafrustle";
		case 4:
			return "coin";
		case 5:
			return "money";
		default:
			return "stoneStep";
		}
	}

	public override void applyMovementKey(int direction)
	{
		if (this.CanReceiveInput())
		{
			base.applyMovementKey(direction);
		}
	}

	/// <inheritdoc />
	public override void performHoverAction(int x, int y)
	{
		if (!this.CanReceiveInput())
		{
			return;
		}
		base.performHoverAction(x, y);
		if (this.currentPage == -1)
		{
			this.okButton.tryHover(x, y);
			{
				foreach (ClickableTextureComponent c in this.categories)
				{
					if (c.containsPoint(x, y))
					{
						c.sourceRect.X = 402;
					}
					else
					{
						c.sourceRect.X = 392;
					}
				}
				return;
			}
		}
		this.backButton.tryHover(x, y, 0.5f);
		this.forwardButton.tryHover(x, y, 0.5f);
	}

	public bool CanReceiveInput()
	{
		if (this.introTimer > 0)
		{
			return false;
		}
		if (this.saveGameMenu != null)
		{
			return false;
		}
		if (this.outro)
		{
			return false;
		}
		return true;
	}

	/// <inheritdoc />
	public override void receiveKeyPress(Keys key)
	{
		if (!this.CanReceiveInput())
		{
			return;
		}
		if (this.introTimer <= 0 && !Game1.options.gamepadControls && (key.Equals(Keys.Escape) || Game1.options.doesInputListContain(Game1.options.menuButton, key)))
		{
			if (this.currentPage == -1)
			{
				this.receiveLeftClick(this.okButton.bounds.Center.X, this.okButton.bounds.Center.Y);
			}
			else
			{
				this.receiveLeftClick(this.backButton.bounds.Center.X, this.backButton.bounds.Center.Y);
			}
		}
		else if (this.introTimer <= 0 && (!Game1.options.gamepadControls || !Game1.options.doesInputListContain(Game1.options.menuButton, key)))
		{
			base.receiveKeyPress(key);
		}
	}

	/// <inheritdoc />
	public override void receiveGamePadButton(Buttons button)
	{
		if (!this.CanReceiveInput())
		{
			return;
		}
		base.receiveGamePadButton(button);
		if (button != Buttons.Start && button != Buttons.B)
		{
			return;
		}
		if (button == Buttons.B && this.currentPage != -1)
		{
			if (this.currentTab == 0)
			{
				if (Game1.options.SnappyMenus)
				{
					base.currentlySnappedComponent = base.getComponentWithID(this.currentPage);
					this.snapCursorToCurrentSnappedComponent();
				}
				this.currentPage = -1;
			}
			else
			{
				this.currentTab--;
			}
			Game1.playSound("shwip");
		}
		else if (this.currentPage == -1 && !this.outro)
		{
			if (this.introTimer <= 0)
			{
				this.okClicked();
			}
			else
			{
				this.introTimer -= Game1.currentGameTime.ElapsedGameTime.Milliseconds * 2;
			}
		}
	}

	private void okClicked()
	{
		this.outro = true;
		this.outroFadeTimer = 800;
		Game1.playSound("bigDeSelect");
		Game1.changeMusicTrack("none");
	}

	/// <inheritdoc />
	public override void receiveLeftClick(int x, int y, bool playSound = true)
	{
		if (!this.CanReceiveInput() || (this.outro && !this.savedYet) || this.savedYet)
		{
			return;
		}
		base.receiveLeftClick(x, y, playSound);
		if (this.currentPage == -1 && this.introTimer <= 0 && this.okButton.containsPoint(x, y))
		{
			this.okClicked();
		}
		if (this.currentPage == -1)
		{
			for (int i = 0; i < this.categories.Count; i++)
			{
				if (this.categories[i].visible && this.categories[i].containsPoint(x, y))
				{
					this.currentPage = i;
					Game1.playSound("shwip");
					if (Game1.options.SnappyMenus)
					{
						base.currentlySnappedComponent = base.getComponentWithID(103);
						this.snapCursorToCurrentSnappedComponent();
					}
					break;
				}
			}
			if (Game1.dayOfMonth == 28 && this.timesPokedMoon <= 10 && new Rectangle(Game1.uiViewport.Width - 176, 4, 172, 172).Contains(x, y))
			{
				this.moonShake = 100;
				this.timesPokedMoon++;
				if (this.timesPokedMoon > 10)
				{
					Game1.playSound("shadowDie");
				}
				else
				{
					Game1.playSound("thudStep");
				}
			}
		}
		else if (this.backButton.containsPoint(x, y))
		{
			if (this.currentTab == 0)
			{
				if (Game1.options.SnappyMenus)
				{
					base.currentlySnappedComponent = base.getComponentWithID(this.currentPage);
					this.snapCursorToCurrentSnappedComponent();
				}
				this.currentPage = -1;
			}
			else
			{
				this.currentTab--;
			}
			Game1.playSound("shwip");
		}
		else if (this.showForwardButton() && this.forwardButton.containsPoint(x, y))
		{
			this.currentTab++;
			Game1.playSound("shwip");
		}
	}

	public bool showForwardButton()
	{
		return this.categoryItems[this.currentPage].Count > this.itemsPerCategoryPage * (this.currentTab + 1);
	}

	/// <inheritdoc />
	public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
	{
		base.initialize(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height);
		this.RepositionItems();
	}

	/// <inheritdoc />
	public override void draw(SpriteBatch b)
	{
		bool isWinter = Game1.season == Season.Winter;
		if (Game1.wasRainingYesterday)
		{
			b.Draw(Game1.mouseCursors, new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), new Rectangle(this.wasGreenRain ? 640 : 639, 858, 1, 184), (isWinter ? Color.LightSlateGray : (this.wasGreenRain ? Color.LightGreen : Color.SlateGray)) * (1f - (float)this.introTimer / 3500f));
			if (this.wasGreenRain)
			{
				b.Draw(Game1.mouseCursors, new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), new Rectangle(this.wasGreenRain ? 640 : 639, 858, 1, 184), Color.DimGray * 0.8f * (1f - (float)this.introTimer / 3500f));
			}
			for (int x = -244; x < Game1.uiViewport.Width + 244; x += 244)
			{
				b.Draw(Game1.mouseCursors, new Vector2((float)x + this.weatherX / 2f % 244f, 32f), new Rectangle(643, 1142, 61, 53), Color.DarkSlateGray * 1f * (1f - (float)this.introTimer / 3500f), 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
			}
			for (int i = 0; i < base.width; i += 639)
			{
				b.Draw(Game1.mouseCursors, new Vector2(i * 4, Game1.uiViewport.Height - 192), new Rectangle(0, isWinter ? 1034 : 737, 639, 48), (isWinter ? (Color.White * 0.25f) : new Color(30, 62, 50)) * (0.5f - (float)this.introTimer / 3500f), 0f, Vector2.Zero, 4f, SpriteEffects.FlipHorizontally, 1f);
				b.Draw(Game1.mouseCursors, new Vector2(i * 4, Game1.uiViewport.Height - 128), new Rectangle(0, isWinter ? 1034 : 737, 639, 32), (isWinter ? (Color.White * 0.5f) : new Color(30, 62, 50)) * (1f - (float)this.introTimer / 3500f), 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
			}
			b.Draw(Game1.mouseCursors, new Vector2(160f, Game1.uiViewport.Height - 128 + 16 + 8), new Rectangle(653, 880, 10, 10), Color.White * (1f - (float)this.introTimer / 3500f), 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
			for (int j = -244; j < Game1.uiViewport.Width + 244; j += 244)
			{
				b.Draw(Game1.mouseCursors, new Vector2((float)j + this.weatherX % 244f, -32f), new Rectangle(643, 1142, 61, 53), Color.SlateGray * 0.85f * (1f - (float)this.introTimer / 3500f), 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.9f);
			}
			foreach (TemporaryAnimatedSprite animation in this.animations)
			{
				animation.draw(b, localPosition: true);
			}
			for (int k = -244; k < Game1.uiViewport.Width + 244; k += 244)
			{
				b.Draw(Game1.mouseCursors, new Vector2((float)k + this.weatherX * 1.5f % 244f, -128f), new Rectangle(643, 1142, 61, 53), Color.LightSlateGray * (1f - (float)this.introTimer / 3500f), 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.9f);
			}
		}
		else
		{
			b.Draw(Game1.mouseCursors, new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), new Rectangle(639, 858, 1, 184), Color.White * (1f - (float)this.introTimer / 3500f));
			for (int l = 0; l < base.width; l += 639)
			{
				b.Draw(Game1.mouseCursors, new Vector2(l * 4, 0f), new Rectangle(0, 1453, 639, 195), Color.White * (1f - (float)this.introTimer / 3500f), 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
			}
			if (Game1.dayOfMonth == 28)
			{
				b.Draw(Game1.mouseCursors, new Vector2(Game1.uiViewport.Width - 176, 4f) + ((this.moonShake > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero), new Rectangle(642, 835, 43, 43), Color.White * (1f - (float)this.introTimer / 3500f), 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
				if (this.timesPokedMoon > 10)
				{
					b.Draw(Game1.mouseCursors, new Vector2(Game1.uiViewport.Width - 136, 48f) + ((this.moonShake > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero), new Rectangle(685, 844 + ((Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 4000.0 < 200.0 || (Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 8000.0 > 7600.0 && Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 8000.0 < 7800.0)) ? 21 : 0), 19, 21), Color.White * (1f - (float)this.introTimer / 3500f), 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
				}
			}
			b.Draw(Game1.mouseCursors, new Vector2(0f, Game1.uiViewport.Height - 192), new Rectangle(0, isWinter ? 1034 : 737, 639, 48), (isWinter ? (Color.White * 0.25f) : new Color(0, 20, 40)) * (0.65f - (float)this.introTimer / 3500f), 0f, Vector2.Zero, 4f, SpriteEffects.FlipHorizontally, 1f);
			b.Draw(Game1.mouseCursors, new Vector2(2556f, Game1.uiViewport.Height - 192), new Rectangle(0, isWinter ? 1034 : 737, 639, 48), (isWinter ? (Color.White * 0.25f) : new Color(0, 20, 40)) * (0.65f - (float)this.introTimer / 3500f), 0f, Vector2.Zero, 4f, SpriteEffects.FlipHorizontally, 1f);
			b.Draw(Game1.mouseCursors, new Vector2(0f, Game1.uiViewport.Height - 128), new Rectangle(0, isWinter ? 1034 : 737, 639, 32), (isWinter ? (Color.White * 0.5f) : new Color(0, 32, 20)) * (1f - (float)this.introTimer / 3500f), 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
			b.Draw(Game1.mouseCursors, new Vector2(2556f, Game1.uiViewport.Height - 128), new Rectangle(0, isWinter ? 1034 : 737, 639, 32), (isWinter ? (Color.White * 0.5f) : new Color(0, 32, 20)) * (1f - (float)this.introTimer / 3500f), 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
			b.Draw(Game1.mouseCursors, new Vector2(160f, Game1.uiViewport.Height - 128 + 16 + 8), new Rectangle(653, 880, 10, 10), Color.White * (1f - (float)this.introTimer / 3500f), 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
		}
		if (!this.outro && !Game1.wasRainingYesterday)
		{
			foreach (TemporaryAnimatedSprite animation2 in this.animations)
			{
				animation2.draw(b, localPosition: true);
			}
		}
		if (this.wasGreenRain)
		{
			b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Green * 0.1f);
		}
		if (this.currentPage == -1)
		{
			int scroll_draw_y = this.categories[0].bounds.Y - 128;
			if (scroll_draw_y >= 0)
			{
				SpriteText.drawStringWithScrollCenteredAt(b, Utility.getYesterdaysDate(), Game1.uiViewport.Width / 2, scroll_draw_y);
			}
			int extraWidth = ((Game1.content.GetCurrentLanguage() == LocalizedContentManager.LanguageCode.ru) ? 64 : 0);
			int yOffset = -20;
			int i2 = 0;
			foreach (ClickableTextureComponent c in this.categories)
			{
				if (this.introTimer < 2500 - i2 * 500)
				{
					Vector2 start = c.getVector2() + new Vector2(12 - extraWidth, -8f);
					if (c.visible)
					{
						c.draw(b);
						b.Draw(Game1.mouseCursors, start + new Vector2(-104 + extraWidth, yOffset + 4), new Rectangle(293, 360, 24, 24), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
						this.categoryItems[i2][0].drawInMenu(b, start + new Vector2(-88 + extraWidth, yOffset + 16), 1f, 1f, 0.9f, StackDrawType.Hide);
					}
					IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(384, 373, 18, 18), (int)(start.X + (float)(-this.itemSlotWidth) - (float)this.categoryLabelsWidth - 12f), (int)(start.Y + (float)yOffset), this.categoryLabelsWidth + extraWidth, 104, Color.White, 4f, drawShadow: false);
					SpriteText.drawString(b, c.hoverText, (int)start.X - this.itemSlotWidth - this.categoryLabelsWidth + 8, (int)start.Y + 4);
					for (int m = 0; m < 6; m++)
					{
						b.Draw(Game1.mouseCursors, start + new Vector2(-this.itemSlotWidth + extraWidth - 192 - 24 + m * 6 * 4, 12f), new Rectangle(355, 476, 7, 11), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
					}
					this.categoryDials[i2].draw(b, start + new Vector2(-this.itemSlotWidth + extraWidth - 192 - 48 + 4, 20f), this.categoryTotals[i2]);
					b.Draw(Game1.mouseCursors, start + new Vector2(-this.itemSlotWidth + extraWidth - 64 - 4, 12f), new Rectangle(408, 476, 9, 11), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.88f);
				}
				i2++;
			}
			if (this.introTimer <= 0)
			{
				this.okButton.draw(b);
			}
		}
		else
		{
			int boxwidth = Game1.uiViewport.Width;
			int boxheight = Game1.uiViewport.Height;
			boxwidth = Math.Min(base.width, 1280);
			boxheight = Math.Min(base.height, 920);
			int xPos = Game1.uiViewport.Width / 2 - boxwidth / 2;
			int yPos = Game1.uiViewport.Height / 2 - boxheight / 2;
			IClickableMenu.drawTextureBox(b, xPos, yPos, boxwidth, boxheight, Color.White);
			Vector2 position = new Vector2(xPos + 32, yPos + 32);
			for (int n = this.currentTab * this.itemsPerCategoryPage; n < this.currentTab * this.itemsPerCategoryPage + this.itemsPerCategoryPage; n++)
			{
				if (this.categoryItems[this.currentPage].Count > n)
				{
					Item item = this.categoryItems[this.currentPage][n];
					item.drawInMenu(b, position, 1f, 1f, 1f, StackDrawType.Draw);
					string subtotalStr = item.DisplayName + " x" + Game1.content.LoadString("Strings\\StringsFromCSFiles:LoadGameMenu.cs.11020", this.singleItemValues[item]);
					string totalStr = Game1.content.LoadString("Strings\\StringsFromCSFiles:LoadGameMenu.cs.11020", Utility.getNumberWithCommas(this.itemValues[item]));
					string dotsAndName = subtotalStr;
					int totalPosX = (int)position.X + boxwidth - 64 - SpriteText.getWidthOfString(totalStr);
					while (SpriteText.getWidthOfString(dotsAndName + totalStr) < boxwidth - 192)
					{
						dotsAndName += " .";
					}
					if (SpriteText.getWidthOfString(dotsAndName + totalStr) >= boxwidth)
					{
						dotsAndName = dotsAndName.Remove(dotsAndName.Length - 1);
					}
					SpriteText.drawString(b, dotsAndName, (int)position.X + 64 + 12, (int)position.Y + 12);
					SpriteText.drawString(b, totalStr, totalPosX, (int)position.Y + 12);
					position.Y += 68f;
				}
			}
			this.backButton.draw(b);
			if (this.showForwardButton())
			{
				this.forwardButton.draw(b);
			}
		}
		if (this.outro)
		{
			b.Draw(Game1.mouseCursors, new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), new Rectangle(639, 858, 1, 184), Color.Black * (1f - (float)this.outroFadeTimer / 800f));
			SpriteText.drawStringWithScrollCenteredAt(b, this.newDayPlaque ? Utility.getDateString() : Utility.getYesterdaysDate(), Game1.uiViewport.Width / 2, this.dayPlaqueY);
			foreach (TemporaryAnimatedSprite animation3 in this.animations)
			{
				animation3.draw(b, localPosition: true);
			}
			if (this.finalOutroTimer > 0 || this._hasFinished)
			{
				b.Draw(Game1.staminaRect, new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), new Rectangle(0, 0, 1, 1), Color.Black * (1f - (float)this.finalOutroTimer / 2000f));
			}
		}
		this.saveGameMenu?.draw(b);
		if (!Game1.options.SnappyMenus || (this.introTimer <= 0 && !this.outro))
		{
			Game1.mouseCursorTransparency = 1f;
			base.drawMouse(b);
		}
	}
}
