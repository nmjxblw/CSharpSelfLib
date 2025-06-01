using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.Locations;

namespace StardewValley.Menus;

public class GameMenu : IClickableMenu
{
	public static readonly int inventoryTab = 0;

	public static readonly int skillsTab = 1;

	public static readonly int socialTab = 2;

	public static readonly int mapTab = 3;

	public static readonly int craftingTab = 4;

	public static readonly int animalsTab = 5;

	public static readonly int powersTab = 6;

	public static readonly int collectionsTab = 7;

	public static readonly int optionsTab = 8;

	public static readonly int exitTab = 9;

	public const int region_inventoryTab = 12340;

	public const int region_skillsTab = 12341;

	public const int region_socialTab = 12342;

	public const int region_mapTab = 12343;

	public const int region_craftingTab = 12344;

	public const int region_animalsTab = 12345;

	public const int region_powersTab = 12346;

	public const int region_collectionsTab = 12347;

	public const int region_optionsTab = 12348;

	public const int region_exitTab = 12349;

	public static readonly int numberOfTabs = 9;

	public int currentTab;

	public int lastOpenedNonMapTab = GameMenu.inventoryTab;

	public string hoverText = "";

	public string descriptionText = "";

	public List<ClickableComponent> tabs = new List<ClickableComponent>();

	public List<IClickableMenu> pages = new List<IClickableMenu>();

	public bool invisible;

	public static bool forcePreventClose;

	public static bool bundleItemHovered;

	/// <summary>The translation keys for tab names.</summary>
	private static readonly Dictionary<int, string> TabTranslationKeys = new Dictionary<int, string>
	{
		[GameMenu.inventoryTab] = "Strings\\UI:GameMenu_Inventory",
		[GameMenu.skillsTab] = "Strings\\UI:GameMenu_Skills",
		[GameMenu.socialTab] = "Strings\\UI:GameMenu_Social",
		[GameMenu.mapTab] = "Strings\\UI:GameMenu_Map",
		[GameMenu.craftingTab] = "Strings\\UI:GameMenu_Crafting",
		[GameMenu.powersTab] = "Strings\\1_6_Strings:GameMenu_Powers",
		[GameMenu.exitTab] = "Strings\\UI:GameMenu_Exit",
		[GameMenu.collectionsTab] = "Strings\\UI:GameMenu_Collections",
		[GameMenu.optionsTab] = "Strings\\UI:GameMenu_Options",
		[GameMenu.exitTab] = "Strings\\UI:GameMenu_Exit"
	};

	public GameMenu(bool playOpeningSound = true)
		: base(Game1.uiViewport.Width / 2 - (800 + IClickableMenu.borderWidth * 2) / 2, Game1.uiViewport.Height / 2 - (600 + IClickableMenu.borderWidth * 2) / 2, 800 + IClickableMenu.borderWidth * 2, 600 + IClickableMenu.borderWidth * 2, showUpperRightCloseButton: true)
	{
		this.tabs.Add(new ClickableComponent(new Rectangle(base.xPositionOnScreen + 64, base.yPositionOnScreen + IClickableMenu.tabYPositionRelativeToMenuY + 64, 64, 64), "inventory", Game1.content.LoadString("Strings\\UI:GameMenu_Inventory"))
		{
			myID = 12340,
			downNeighborID = 0,
			rightNeighborID = 12341,
			tryDefaultIfNoDownNeighborExists = true,
			fullyImmutable = true
		});
		this.pages.Add(new InventoryPage(base.xPositionOnScreen, base.yPositionOnScreen, base.width, base.height));
		this.tabs.Add(new ClickableComponent(new Rectangle(base.xPositionOnScreen + 128, base.yPositionOnScreen + IClickableMenu.tabYPositionRelativeToMenuY + 64, 64, 64), "skills", Game1.content.LoadString("Strings\\UI:GameMenu_Skills"))
		{
			myID = 12341,
			downNeighborID = 1,
			rightNeighborID = 12342,
			leftNeighborID = 12340,
			tryDefaultIfNoDownNeighborExists = true,
			fullyImmutable = true
		});
		this.pages.Add(new SkillsPage(base.xPositionOnScreen, base.yPositionOnScreen, base.width + ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ru || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.it) ? 64 : 0), base.height));
		this.tabs.Add(new ClickableComponent(new Rectangle(base.xPositionOnScreen + 192, base.yPositionOnScreen + IClickableMenu.tabYPositionRelativeToMenuY + 64, 64, 64), "social", Game1.content.LoadString("Strings\\UI:GameMenu_Social"))
		{
			myID = 12342,
			downNeighborID = 2,
			rightNeighborID = 12343,
			leftNeighborID = 12341,
			tryDefaultIfNoDownNeighborExists = true,
			fullyImmutable = true
		});
		this.pages.Add(new SocialPage(base.xPositionOnScreen, base.yPositionOnScreen, base.width + 36, base.height));
		this.tabs.Add(new ClickableComponent(new Rectangle(base.xPositionOnScreen + 256, base.yPositionOnScreen + IClickableMenu.tabYPositionRelativeToMenuY + 64, 64, 64), "map", Game1.content.LoadString("Strings\\UI:GameMenu_Map"))
		{
			myID = 12343,
			downNeighborID = 3,
			rightNeighborID = 12344,
			leftNeighborID = 12342,
			tryDefaultIfNoDownNeighborExists = true,
			fullyImmutable = true
		});
		this.pages.Add(new MapPage(base.xPositionOnScreen, base.yPositionOnScreen, base.width, base.height));
		this.tabs.Add(new ClickableComponent(new Rectangle(base.xPositionOnScreen + 320, base.yPositionOnScreen + IClickableMenu.tabYPositionRelativeToMenuY + 64, 64, 64), "crafting", Game1.content.LoadString("Strings\\UI:GameMenu_Crafting"))
		{
			myID = 12344,
			downNeighborID = 4,
			rightNeighborID = 12345,
			leftNeighborID = 12343,
			tryDefaultIfNoDownNeighborExists = true,
			fullyImmutable = true
		});
		this.pages.Add(new CraftingPage(base.xPositionOnScreen, base.yPositionOnScreen, base.width, base.height));
		this.tabs.Add(new ClickableComponent(new Rectangle(base.xPositionOnScreen + 384, base.yPositionOnScreen + IClickableMenu.tabYPositionRelativeToMenuY + 64, 64, 64), "animals", Game1.content.LoadString("Strings\\1_6_Strings:GameMenu_Animals"))
		{
			myID = 12345,
			downNeighborID = 5,
			rightNeighborID = 12346,
			leftNeighborID = 12344,
			tryDefaultIfNoDownNeighborExists = true,
			fullyImmutable = true
		});
		this.pages.Add(new AnimalPage(base.xPositionOnScreen, base.yPositionOnScreen, base.width - 64 - 16, base.height));
		this.tabs.Add(new ClickableComponent(new Rectangle(base.xPositionOnScreen + 448, base.yPositionOnScreen + IClickableMenu.tabYPositionRelativeToMenuY + 64, 64, 64), "powers", Game1.content.LoadString("Strings\\1_6_Strings:GameMenu_Powers"))
		{
			myID = 12346,
			downNeighborID = 6,
			rightNeighborID = 12347,
			leftNeighborID = 12345,
			tryDefaultIfNoDownNeighborExists = true,
			fullyImmutable = true
		});
		this.pages.Add(new PowersTab(base.xPositionOnScreen, base.yPositionOnScreen, base.width - 64 - 16, base.height));
		this.tabs.Add(new ClickableComponent(new Rectangle(base.xPositionOnScreen + 512, base.yPositionOnScreen + IClickableMenu.tabYPositionRelativeToMenuY + 64, 64, 64), "collections", Game1.content.LoadString("Strings\\UI:GameMenu_Collections"))
		{
			myID = 12347,
			downNeighborID = 7,
			rightNeighborID = 12348,
			leftNeighborID = 12346,
			tryDefaultIfNoDownNeighborExists = true,
			fullyImmutable = true
		});
		this.pages.Add(new CollectionsPage(base.xPositionOnScreen, base.yPositionOnScreen, base.width - 64 - 16, base.height));
		this.tabs.Add(new ClickableComponent(new Rectangle(base.xPositionOnScreen + 576, base.yPositionOnScreen + IClickableMenu.tabYPositionRelativeToMenuY + 64, 64, 64), "options", Game1.content.LoadString("Strings\\UI:GameMenu_Options"))
		{
			myID = 12348,
			downNeighborID = 8,
			rightNeighborID = 12349,
			leftNeighborID = 12347,
			tryDefaultIfNoDownNeighborExists = true,
			fullyImmutable = true
		});
		int extraWidth = ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ru) ? 96 : ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.tr || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.fr) ? 192 : 0));
		this.pages.Add(new OptionsPage(base.xPositionOnScreen, base.yPositionOnScreen, base.width + extraWidth, base.height));
		this.tabs.Add(new ClickableComponent(new Rectangle(base.xPositionOnScreen + 640, base.yPositionOnScreen + IClickableMenu.tabYPositionRelativeToMenuY + 64, 64, 64), "exit", Game1.content.LoadString("Strings\\UI:GameMenu_Exit"))
		{
			myID = 12349,
			downNeighborID = 9,
			leftNeighborID = 12348,
			tryDefaultIfNoDownNeighborExists = true,
			fullyImmutable = true
		});
		this.pages.Add(new ExitPage(base.xPositionOnScreen, base.yPositionOnScreen, base.width - 64 - 16, base.height));
		if (Game1.activeClickableMenu == null && playOpeningSound)
		{
			Game1.playSound("bigSelect");
		}
		GameMenu.forcePreventClose = false;
		Game1.RequireLocation<CommunityCenter>("CommunityCenter").refreshBundlesIngredientsInfo();
		this.pages[this.currentTab].populateClickableComponentList();
		this.AddTabsToClickableComponents(this.pages[this.currentTab]);
		if (Game1.options.SnappyMenus)
		{
			this.snapToDefaultClickableComponent();
		}
	}

	public void AddTabsToClickableComponents(IClickableMenu menu)
	{
		menu.allClickableComponents.AddRange(this.tabs);
	}

	public GameMenu(int startingTab, int extra = -1, bool playOpeningSound = true)
		: this(playOpeningSound)
	{
		this.changeTab(startingTab, playSound: false);
		if (startingTab == GameMenu.optionsTab && extra != -1)
		{
			(this.pages[GameMenu.optionsTab] as OptionsPage).currentItemIndex = extra;
		}
	}

	public override void automaticSnapBehavior(int direction, int oldRegion, int oldID)
	{
		if (this.GetCurrentPage() != null)
		{
			this.GetCurrentPage().automaticSnapBehavior(direction, oldRegion, oldID);
		}
		else
		{
			base.automaticSnapBehavior(direction, oldRegion, oldID);
		}
	}

	public override void snapToDefaultClickableComponent()
	{
		if (this.currentTab < this.pages.Count)
		{
			this.pages[this.currentTab].snapToDefaultClickableComponent();
		}
	}

	/// <inheritdoc />
	public override void receiveGamePadButton(Buttons button)
	{
		base.receiveGamePadButton(button);
		switch (button)
		{
		case Buttons.RightTrigger:
			if (this.currentTab == GameMenu.mapTab)
			{
				Game1.activeClickableMenu = new GameMenu(GameMenu.mapTab + 1);
				Game1.playSound("smallSelect");
			}
			else if (this.currentTab < GameMenu.numberOfTabs && this.pages[this.currentTab].readyToClose())
			{
				this.changeTab(this.currentTab + 1);
			}
			break;
		case Buttons.LeftTrigger:
			if (this.currentTab == GameMenu.mapTab)
			{
				Game1.activeClickableMenu = new GameMenu(GameMenu.mapTab - 1);
				Game1.playSound("smallSelect");
			}
			else if (this.currentTab > 0 && this.pages[this.currentTab].readyToClose())
			{
				this.changeTab(this.currentTab - 1);
			}
			break;
		default:
			this.pages[this.currentTab].receiveGamePadButton(button);
			break;
		}
	}

	public override void setUpForGamePadMode()
	{
		base.setUpForGamePadMode();
		if (this.pages.Count > this.currentTab)
		{
			this.pages[this.currentTab].setUpForGamePadMode();
		}
	}

	public override ClickableComponent getCurrentlySnappedComponent()
	{
		return this.pages[this.currentTab].getCurrentlySnappedComponent();
	}

	public override void setCurrentlySnappedComponentTo(int id)
	{
		this.pages[this.currentTab].setCurrentlySnappedComponentTo(id);
	}

	/// <inheritdoc />
	public override void receiveLeftClick(int x, int y, bool playSound = true)
	{
		if ((this.pages[this.currentTab] as CollectionsPage)?.letterviewerSubMenu == null)
		{
			base.receiveLeftClick(x, y, playSound);
		}
		if (!this.invisible && !GameMenu.forcePreventClose)
		{
			for (int i = 0; i < this.tabs.Count; i++)
			{
				if (this.tabs[i].containsPoint(x, y) && this.currentTab != i && this.pages[this.currentTab].readyToClose())
				{
					this.changeTab(this.getTabNumberFromName(this.tabs[i].name));
					return;
				}
			}
		}
		this.pages[this.currentTab].receiveLeftClick(x, y);
	}

	public static string getLabelOfTabFromIndex(int index)
	{
		if (!GameMenu.TabTranslationKeys.TryGetValue(index, out var translationKey))
		{
			return "";
		}
		return Game1.content.LoadString(translationKey);
	}

	/// <inheritdoc />
	public override void receiveRightClick(int x, int y, bool playSound = true)
	{
		this.pages[this.currentTab].receiveRightClick(x, y);
	}

	/// <inheritdoc />
	public override void receiveScrollWheelAction(int direction)
	{
		base.receiveScrollWheelAction(direction);
		this.pages[this.currentTab].receiveScrollWheelAction(direction);
	}

	/// <inheritdoc />
	public override void performHoverAction(int x, int y)
	{
		base.performHoverAction(x, y);
		this.hoverText = "";
		this.pages[this.currentTab].performHoverAction(x, y);
		foreach (ClickableComponent c in this.tabs)
		{
			if (c.containsPoint(x, y))
			{
				this.hoverText = c.label;
				break;
			}
		}
	}

	public int getTabNumberFromName(string name)
	{
		int whichTab = -1;
		switch (name)
		{
		case "inventory":
			whichTab = GameMenu.inventoryTab;
			break;
		case "skills":
			whichTab = GameMenu.skillsTab;
			break;
		case "social":
			whichTab = GameMenu.socialTab;
			break;
		case "map":
			whichTab = GameMenu.mapTab;
			break;
		case "crafting":
			whichTab = GameMenu.craftingTab;
			break;
		case "collections":
			whichTab = GameMenu.collectionsTab;
			break;
		case "options":
			whichTab = GameMenu.optionsTab;
			break;
		case "exit":
			whichTab = GameMenu.exitTab;
			break;
		case "powers":
			whichTab = GameMenu.powersTab;
			break;
		case "animals":
			whichTab = GameMenu.animalsTab;
			break;
		}
		return whichTab;
	}

	/// <inheritdoc />
	public override void update(GameTime time)
	{
		base.update(time);
		this.pages[this.currentTab].update(time);
	}

	/// <inheritdoc />
	public override void releaseLeftClick(int x, int y)
	{
		base.releaseLeftClick(x, y);
		this.pages[this.currentTab].releaseLeftClick(x, y);
	}

	/// <inheritdoc />
	public override void leftClickHeld(int x, int y)
	{
		base.leftClickHeld(x, y);
		this.pages[this.currentTab].leftClickHeld(x, y);
	}

	public override bool readyToClose()
	{
		if (!GameMenu.forcePreventClose)
		{
			return this.pages[this.currentTab].readyToClose();
		}
		return false;
	}

	public void changeTab(int whichTab, bool playSound = true)
	{
		this.currentTab = this.getTabNumberFromName(this.tabs[whichTab].name);
		if (this.currentTab == GameMenu.mapTab)
		{
			this.invisible = true;
			base.width += 128;
			base.initializeUpperRightCloseButton();
		}
		else
		{
			this.lastOpenedNonMapTab = this.currentTab;
			base.width = 800 + IClickableMenu.borderWidth * 2;
			base.initializeUpperRightCloseButton();
			this.invisible = false;
		}
		if (playSound)
		{
			Game1.playSound("smallSelect");
		}
		this.pages[this.currentTab].populateClickableComponentList();
		this.AddTabsToClickableComponents(this.pages[this.currentTab]);
		this.setTabNeighborsForCurrentPage();
		if (Game1.options.SnappyMenus)
		{
			this.snapToDefaultClickableComponent();
		}
	}

	public IClickableMenu GetCurrentPage()
	{
		if (this.currentTab >= this.pages.Count || this.currentTab < 0)
		{
			return null;
		}
		return this.pages[this.currentTab];
	}

	public void setTabNeighborsForCurrentPage()
	{
		if (this.currentTab == GameMenu.inventoryTab)
		{
			for (int i = 0; i < this.tabs.Count; i++)
			{
				this.tabs[i].downNeighborID = i;
			}
		}
		else if (this.currentTab == GameMenu.exitTab)
		{
			for (int j = 0; j < this.tabs.Count; j++)
			{
				this.tabs[j].downNeighborID = 535;
			}
		}
		else
		{
			for (int k = 0; k < this.tabs.Count; k++)
			{
				this.tabs[k].downNeighborID = -99999;
			}
		}
	}

	/// <inheritdoc />
	public override void draw(SpriteBatch b)
	{
		if (!this.invisible)
		{
			if (!Game1.options.showMenuBackground && !Game1.options.showClearBackgrounds)
			{
				b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.4f);
			}
			Game1.drawDialogueBox(base.xPositionOnScreen, base.yPositionOnScreen, this.pages[this.currentTab].width, this.pages[this.currentTab].height, speaker: false, drawOnlyBox: true);
			b.End();
			b.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp);
			foreach (ClickableComponent c in this.tabs)
			{
				int sheetIndex = -1;
				switch (c.name)
				{
				case "inventory":
					sheetIndex = 0;
					break;
				case "skills":
					sheetIndex = 1;
					break;
				case "social":
					sheetIndex = 2;
					break;
				case "map":
					sheetIndex = 3;
					break;
				case "crafting":
					sheetIndex = 4;
					break;
				case "catalogue":
					sheetIndex = 7;
					break;
				case "collections":
					sheetIndex = 5;
					break;
				case "options":
					sheetIndex = 6;
					break;
				case "exit":
					sheetIndex = 7;
					break;
				case "coop":
					sheetIndex = 1;
					break;
				case "powers":
					b.Draw(Game1.mouseCursors_1_6, new Vector2(c.bounds.X, c.bounds.Y + ((this.currentTab == this.getTabNumberFromName(c.name)) ? 8 : 0)), new Rectangle(216, 494, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.0001f);
					break;
				case "animals":
					b.Draw(Game1.mouseCursors_1_6, new Vector2(c.bounds.X, c.bounds.Y + ((this.currentTab == this.getTabNumberFromName(c.name)) ? 8 : 0)), new Rectangle(257, 246, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.0001f);
					break;
				}
				if (sheetIndex != -1)
				{
					b.Draw(Game1.mouseCursors, new Vector2(c.bounds.X, c.bounds.Y + ((this.currentTab == this.getTabNumberFromName(c.name)) ? 8 : 0)), new Rectangle(sheetIndex * 16, 368, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.0001f);
				}
				if (c.name.Equals("skills"))
				{
					Game1.player.FarmerRenderer.drawMiniPortrat(b, new Vector2(c.bounds.X + 8, c.bounds.Y + 12 + ((this.currentTab == this.getTabNumberFromName(c.name)) ? 8 : 0)), 0.00011f, 3f, 2, Game1.player);
				}
			}
			b.End();
			b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
			this.pages[this.currentTab].draw(b);
			if (!this.hoverText.Equals(""))
			{
				IClickableMenu.drawHoverText(b, this.hoverText, Game1.smallFont);
			}
		}
		else
		{
			this.pages[this.currentTab].draw(b);
		}
		if (!GameMenu.forcePreventClose && this.pages[this.currentTab].shouldDrawCloseButton())
		{
			base.draw(b);
		}
		if ((!Game1.options.SnappyMenus || (this.pages[this.currentTab] as CollectionsPage)?.letterviewerSubMenu == null) && !Game1.options.hardwareCursor)
		{
			base.drawMouse(b, ignore_transparency: true);
		}
	}

	public override bool areGamePadControlsImplemented()
	{
		return false;
	}

	/// <inheritdoc />
	public override void receiveKeyPress(Keys key)
	{
		if (Game1.options.menuButton.Contains(new InputButton(key)) && this.readyToClose())
		{
			Game1.exitActiveMenu();
			Game1.playSound("bigDeSelect");
		}
		this.pages[this.currentTab].receiveKeyPress(key);
	}

	public override void emergencyShutDown()
	{
		base.emergencyShutDown();
		this.pages[this.currentTab].emergencyShutDown();
	}

	/// <inheritdoc />
	protected override void cleanupBeforeExit()
	{
		base.cleanupBeforeExit();
		if (Game1.options.optionsDirty)
		{
			Game1.options.SaveDefaultOptions();
		}
	}
}
