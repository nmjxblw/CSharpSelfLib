using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using StardewValley.Extensions;
using StardewValley.GameData.Shops;
using StardewValley.Internal;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Locations;
using StardewValley.Objects;
using StardewValley.TokenizableStrings;
using StardewValley.Triggers;

namespace StardewValley.Menus;

public class ShopMenu : IClickableMenu
{
	/// <summary>A callback to invoke when the player purchases an item.</summary>
	/// <param name="salable">The shop entry that was purchased. The entry's <see cref="P:StardewValley.Item.Stack" /> represents the number purchased per click, *not* the number bought (which is <paramref name="countTaken" />) or remaining stock (which is <paramref name="stock" />).</param>
	/// <param name="who">The player who purchased the item.</param>
	/// <param name="countTaken">The stack size that was taken out of the shop.</param>
	/// <param name="stock">The remaining stock in the shop. The <paramref name="countTaken" /> is already deducted from this value.</param>
	/// <returns>Returns whether to immediately exit the menu.</returns>
	public delegate bool OnPurchaseDelegate(ISalable salable, Farmer who, int countTaken, ItemStockInformation stock);

	/// <summary>A cached visual theme for the <see cref="T:StardewValley.Menus.ShopMenu" />.</summary>
	public class ShopCachedTheme
	{
		/// <summary>The visual theme data from <c>Data/Shops</c>, if applicable.</summary>
		public ShopThemeData ThemeData { get; }

		/// <summary>The texture for the shop window border.</summary>
		public Texture2D WindowBorderTexture { get; }

		/// <summary>The pixel area within the <see cref="P:StardewValley.Menus.ShopMenu.ShopCachedTheme.WindowBorderSourceRect" /> for the shop window border. This should be an 18x18 pixel area.</summary>
		public Rectangle WindowBorderSourceRect { get; }

		/// <summary>The texture for the NPC portrait background.</summary>
		public Texture2D PortraitBackgroundTexture { get; }

		/// <summary>The pixel area within the <see cref="P:StardewValley.Menus.ShopMenu.ShopCachedTheme.PortraitBackgroundTexture" /> for the NPC portrait background. This should be a 74x47 pixel area.</summary>
		public Rectangle PortraitBackgroundSourceRect { get; }

		/// <summary>The texture for the NPC dialogue background.</summary>
		public Texture2D DialogueBackgroundTexture { get; }

		/// <summary>The pixel area within the <see cref="P:StardewValley.Menus.ShopMenu.ShopCachedTheme.DialogueBackgroundTexture" /> for the NPC dialogue background. This should be a 60x60 pixel area.</summary>
		public Rectangle DialogueBackgroundSourceRect { get; }

		/// <summary>The sprite text color for the dialogue text, or <c>null</c> for the default color.</summary>
		public Color? DialogueColor { get; }

		/// <summary>The sprite text shadow color for the dialogue text, or <c>null</c> for the default color.</summary>
		public Color? DialogueShadowColor { get; }

		/// <summary>The texture for the item row background.</summary>
		public Texture2D ItemRowBackgroundTexture { get; }

		/// <summary>The pixel area within the <see cref="P:StardewValley.Menus.ShopMenu.ShopCachedTheme.ItemRowBackgroundTexture" /> for the item row background. This should be a 15x15 pixel area.</summary>
		public Rectangle ItemRowBackgroundSourceRect { get; }

		/// <summary>The color tint to apply to the item row background when the cursor is hovering over it</summary>
		public Color ItemRowBackgroundHoverColor { get; }

		/// <summary>The sprite text color for the item text, or <c>null</c> for the default color.</summary>
		public Color? ItemRowTextColor { get; }

		/// <summary>The texture for the box behind the item icons.</summary>
		public Texture2D ItemIconBackgroundTexture { get; }

		/// <summary>The pixel area within the <see cref="P:StardewValley.Menus.ShopMenu.ShopCachedTheme.ItemIconBackgroundTexture" /> for the item icon background. This should be an 18x18 pixel area.</summary>
		public Rectangle ItemIconBackgroundSourceRect { get; }

		/// <summary>The texture for the scroll up icon.</summary>
		public Texture2D ScrollUpTexture { get; }

		/// <summary>The pixel area within the <see cref="P:StardewValley.Menus.ShopMenu.ShopCachedTheme.ScrollUpTexture" /> for the scroll up icon. This should be an 11x12 pixel area.</summary>
		public Rectangle ScrollUpSourceRect { get; }

		/// <summary>The texture for the scroll down icon.</summary>
		public Texture2D ScrollDownTexture { get; }

		/// <summary>The pixel area within the <see cref="P:StardewValley.Menus.ShopMenu.ShopCachedTheme.ScrollDownTexture" /> for the scroll down icon. This should be an 11x12 pixel area.</summary>
		public Rectangle ScrollDownSourceRect { get; }

		/// <summary>The texture for the scrollbar foreground texture.</summary>
		public Texture2D ScrollBarFrontTexture { get; }

		/// <summary>The pixel area within the <see cref="P:StardewValley.Menus.ShopMenu.ShopCachedTheme.ScrollBarFrontTexture" /> for the scroll foreground. This should be a 6x10 pixel area.</summary>
		public Rectangle ScrollBarFrontSourceRect { get; }

		/// <summary>The texture for the scrollbar background texture.</summary>
		public Texture2D ScrollBarBackTexture { get; }

		/// <summary>The pixel area within the <see cref="P:StardewValley.Menus.ShopMenu.ShopCachedTheme.ScrollBarBackTexture" /> for the scroll background. This should be a 6x6 pixel area.</summary>
		public Rectangle ScrollBarBackSourceRect { get; }

		/// <summary>Construct an instance.</summary>
		/// <param name="theme">The visual theme data, or <c>null</c> for the default shop theme.</param>
		public ShopCachedTheme(ShopThemeData theme)
		{
			this.ThemeData = theme;
			this.WindowBorderTexture = this.LoadThemeTexture(theme?.WindowBorderTexture, Game1.mouseCursors);
			this.WindowBorderSourceRect = theme?.WindowBorderSourceRect ?? new Rectangle(384, 373, 18, 18);
			this.PortraitBackgroundTexture = this.LoadThemeTexture(theme?.PortraitBackgroundTexture, Game1.mouseCursors);
			this.PortraitBackgroundSourceRect = theme?.PortraitBackgroundSourceRect ?? new Rectangle(603, 414, 74, 74);
			this.DialogueBackgroundTexture = this.LoadThemeTexture(theme?.DialogueBackgroundTexture, Game1.menuTexture);
			this.DialogueBackgroundSourceRect = theme?.DialogueBackgroundSourceRect ?? new Rectangle(0, 256, 60, 60);
			this.DialogueColor = Utility.StringToColor(theme?.DialogueColor);
			this.DialogueShadowColor = Utility.StringToColor(theme?.DialogueShadowColor);
			this.ItemRowBackgroundTexture = this.LoadThemeTexture(theme?.ItemRowBackgroundTexture, Game1.mouseCursors);
			this.ItemRowBackgroundSourceRect = theme?.ItemRowBackgroundSourceRect ?? new Rectangle(384, 396, 15, 15);
			this.ItemRowBackgroundHoverColor = Utility.StringToColor(theme?.ItemRowBackgroundHoverColor) ?? Color.Wheat;
			this.ItemRowTextColor = Utility.StringToColor(theme?.ItemRowTextColor);
			this.ItemIconBackgroundTexture = this.LoadThemeTexture(theme?.ItemIconBackgroundTexture, Game1.mouseCursors);
			this.ItemIconBackgroundSourceRect = theme?.ItemIconBackgroundSourceRect ?? new Rectangle(296, 363, 18, 18);
			this.ScrollUpTexture = this.LoadThemeTexture(theme?.ScrollUpTexture, Game1.mouseCursors);
			this.ScrollUpSourceRect = theme?.ScrollUpSourceRect ?? new Rectangle(421, 459, 11, 12);
			this.ScrollDownTexture = this.LoadThemeTexture(theme?.ScrollDownTexture, Game1.mouseCursors);
			this.ScrollDownSourceRect = theme?.ScrollDownSourceRect ?? new Rectangle(421, 472, 11, 12);
			this.ScrollBarFrontTexture = this.LoadThemeTexture(theme?.ScrollBarFrontTexture, Game1.mouseCursors);
			this.ScrollBarFrontSourceRect = theme?.ScrollBarFrontSourceRect ?? new Rectangle(435, 463, 6, 10);
			this.ScrollBarBackTexture = this.LoadThemeTexture(theme?.ScrollBarBackTexture, Game1.mouseCursors);
			this.ScrollBarBackSourceRect = theme?.ScrollBarBackSourceRect ?? new Rectangle(403, 383, 6, 6);
		}

		/// <summary>Load a theme texture if it's non-null and exists, else get the default texture.</summary>
		/// <param name="customTextureName">The custom texture asset name to load.</param>
		/// <param name="defaultTexture">The default texture.</param>
		private Texture2D LoadThemeTexture(string customTextureName, Texture2D defaultTexture)
		{
			if (customTextureName == null || !Game1.content.DoesAssetExist<Texture2D>(customTextureName))
			{
				return defaultTexture;
			}
			return Game1.content.Load<Texture2D>(customTextureName);
		}
	}

	/// <summary>A clickable component representing a shop tab, which applies a filter to the list of displayed shop items when clicked.</summary>
	public class ShopTabClickableTextureComponent : ClickableTextureComponent
	{
		/// <summary>Matches items to show when this tab is selected.</summary>
		public Func<ISalable, bool> Filter;

		public ShopTabClickableTextureComponent(string name, Rectangle bounds, string label, string hoverText, Texture2D texture, Rectangle sourceRect, float scale, bool drawShadow = false)
			: base(name, bounds, label, hoverText, texture, sourceRect, scale, drawShadow)
		{
		}

		public ShopTabClickableTextureComponent(Rectangle bounds, Texture2D texture, Rectangle sourceRect, float scale, bool drawShadow = false)
			: base(bounds, texture, sourceRect, scale, drawShadow)
		{
		}
	}

	public const int region_shopButtonModifier = 3546;

	public const int region_upArrow = 97865;

	public const int region_downArrow = 97866;

	public const int region_tabStartIndex = 99999;

	public const int infiniteStock = int.MaxValue;

	public const int itemsPerPage = 4;

	public const int numberRequiredForExtraItemTrade = 5;

	public string hoverText = "";

	public string boldTitleText = "";

	/// <summary>The sound played when the shop menu is opened.</summary>
	public string openMenuSound = "dwop";

	/// <summary>The sound played when an item is purchased normally.</summary>
	public string purchaseSound = "purchaseClick";

	/// <summary>The repeating sound played when accumulating a stack to purchase (e.g. by holding right-click on PC).</summary>
	public string purchaseRepeatSound = "purchaseRepeat";

	/// <summary>A key which identifies the current shop. This may be the unique shop ID in <c>Data/Shops</c> for a standard shop, <c>Dresser</c> or <c>FishTank</c> for furniture, etc.</summary>
	public string ShopId;

	/// <summary>The underlying shop data, if this is a standard shop from <c>Data/Shops</c>.</summary>
	public ShopData ShopData;

	public InventoryMenu inventory;

	public ISalable heldItem;

	public ISalable hoveredItem;

	/// <summary>How to draw stack size numbers in the shop list by default. If set, this overrides <see cref="F:StardewValley.GameData.Shops.ShopData.StackSizeVisibility" />.</summary>
	public StackDrawType? DefaultStackDrawType;

	private TemporaryAnimatedSprite poof;

	private Rectangle scrollBarRunner;

	/// <summary>The items sold in the shop.</summary>
	public List<ISalable> forSale = new List<ISalable>();

	public List<ClickableComponent> forSaleButtons = new List<ClickableComponent>();

	public List<int> categoriesToSellHere = new List<int>();

	public List<List<string>> tagsToSellHere = new List<List<string>>();

	/// <summary>The stock info for each item in <see cref="F:StardewValley.Menus.ShopMenu.forSale" />.</summary>
	public Dictionary<ISalable, ItemStockInformation> itemPriceAndStock = new Dictionary<ISalable, ItemStockInformation>();

	private float sellPercentage = 1f;

	private TemporaryAnimatedSpriteList animations = new TemporaryAnimatedSpriteList();

	public int hoverPrice = -1;

	public int currentItemIndex;

	/// <summary>The currency in which all items in the shop should be priced. The valid values are 0 (money), 1 (star tokens), 2 (Qi coins), and 4 (Qi gems).</summary>
	public int currency;

	public ClickableTextureComponent upArrow;

	public ClickableTextureComponent downArrow;

	public ClickableTextureComponent scrollBar;

	public Texture2D portraitTexture;

	public string potraitPersonDialogue;

	public object source;

	private bool scrolling;

	/// <summary>A callback to invoke when the player purchases an item, if any.</summary>
	public OnPurchaseDelegate onPurchase;

	/// <summary>A callback to invoke when the player sells an item, if any.</summary>
	public Func<ISalable, bool> onSell;

	public Func<int, bool> canPurchaseCheck;

	public List<ShopTabClickableTextureComponent> tabButtons = new List<ShopTabClickableTextureComponent>();

	protected int currentTab;

	protected bool _isStorageShop;

	public bool readOnly;

	public HashSet<ISalable> buyBackItems = new HashSet<ISalable>();

	public Dictionary<ISalable, ISalable> buyBackItemsToResellTomorrow = new Dictionary<ISalable, ISalable>();

	/// <summary>The number of milliseconds until the menu will allow buying or selling items, to help avoid doing so accidentally.</summary>
	public int safetyTimer = 250;

	/// <summary>The visual theme applied to the shop UI.</summary>
	/// <remarks>This can be set via <see cref="M:StardewValley.Menus.ShopMenu.SetVisualTheme(StardewValley.GameData.Shops.ShopThemeData)" />.</remarks>
	public ShopCachedTheme VisualTheme { get; private set; }

	/// <summary>Construct an instance.</summary>
	/// <param name="shopId">The unique shop ID in <c>Data\Shops</c>.</param>
	/// <param name="shopData">The shop data from <c>Data/Shops</c>.</param>
	/// <param name="ownerData">The owner entry for the shop portrait and dialogue, or <c>null</c> to disable those.</param>
	/// <param name="owner">The NPC matching <paramref name="ownerData" /> whose portrait to show, if applicable.</param>
	/// <param name="onPurchase">A callback to invoke when the player purchases an item, if any.</param>
	/// <param name="onSell">A callback to invoke when the player sells an item, if any.</param>
	/// <param name="playOpenSound">Whether to play the open-menu sound.</param>
	public ShopMenu(string shopId, ShopData shopData, ShopOwnerData ownerData, NPC owner = null, OnPurchaseDelegate onPurchase = null, Func<ISalable, bool> onSell = null, bool playOpenSound = true)
	{
		this.ShopId = shopId ?? throw new ArgumentNullException("shopId");
		foreach (KeyValuePair<ISalable, ItemStockInformation> pair in ShopBuilder.GetShopStock(shopId, shopData))
		{
			this.AddForSale(pair.Key, pair.Value);
		}
		this.ShopData = shopData;
		if (shopData.SalableItemTags != null)
		{
			foreach (string salableItemTag in shopData.SalableItemTags)
			{
				List<string> list = new List<string>();
				string[] array = salableItemTag.Split(',');
				foreach (string tag in array)
				{
					list.Add(tag.Trim());
				}
				this.tagsToSellHere.Add(list);
			}
		}
		this.openMenuSound = shopData.OpenSound ?? this.openMenuSound;
		this.purchaseSound = shopData.PurchaseSound ?? this.purchaseSound;
		this.purchaseRepeatSound = shopData.PurchaseRepeatSound ?? this.purchaseRepeatSound;
		this.SetVisualTheme(shopData.VisualTheme?.FirstOrDefault((ShopThemeData theme) => GameStateQuery.CheckConditions(theme.Condition)));
		this.SetUpShopOwner(ownerData, owner);
		this.Initialize(shopData.Currency, onPurchase, onSell, playOpenSound);
	}

	/// <summary>Construct an instance.</summary>
	/// <param name="shopId">A key which identifies the current shop.</param>
	/// <param name="itemPriceAndStock">The items to sell in the shop.</param>
	/// <param name="currency">The currency in which all items in the shop should be priced. The valid values are 0 (money), 1 (star tokens), 2 (Qi coins), and 4 (Qi gems).</param>
	/// <param name="who">The internal name for the NPC running the shop, if any.</param>
	/// <param name="on_purchase">A callback to invoke when the player purchases an item, if any.</param>
	/// <param name="on_sell">A callback to invoke when the player sells an item, if any.</param>
	/// <param name="playOpenSound">Whether to play the open-menu sound.</param>
	public ShopMenu(string shopId, Dictionary<ISalable, ItemStockInformation> itemPriceAndStock, int currency = 0, string who = null, OnPurchaseDelegate on_purchase = null, Func<ISalable, bool> on_sell = null, bool playOpenSound = true)
	{
		this.ShopId = shopId ?? throw new ArgumentNullException("shopId");
		foreach (KeyValuePair<ISalable, ItemStockInformation> pair in itemPriceAndStock)
		{
			this.AddForSale(pair.Key, pair.Value);
		}
		this.SetVisualTheme(null);
		this.setUpShopOwner(who, shopId);
		this.Initialize(currency, on_purchase, on_sell, playOpenSound);
	}

	/// <summary>Construct an instance.</summary>
	/// <param name="shopId">A key which identifies the current shop.</param>
	/// <param name="itemsForSale">The items to sell in the shop.</param>
	/// <param name="currency">The currency in which all items in the shop should be priced. The valid values are 0 (money), 1 (star tokens), 2 (Qi coins), and 4 (Qi gems).</param>
	/// <param name="who">The internal name for the NPC running the shop, if any.</param>
	/// <param name="on_purchase">A callback to invoke when the player purchases an item, if any.</param>
	/// <param name="on_sell">A callback to invoke when the player sells an item, if any.</param>
	/// <param name="playOpenSound">Whether to play the open-menu sound.</param>
	public ShopMenu(string shopId, List<ISalable> itemsForSale, int currency = 0, string who = null, OnPurchaseDelegate on_purchase = null, Func<ISalable, bool> on_sell = null, bool playOpenSound = true)
		: base(Game1.uiViewport.Width / 2 - (800 + IClickableMenu.borderWidth * 2) / 2, Game1.uiViewport.Height / 2 - (600 + IClickableMenu.borderWidth * 2) / 2, 1000 + IClickableMenu.borderWidth * 2, 600 + IClickableMenu.borderWidth * 2, showUpperRightCloseButton: true)
	{
		this.ShopId = shopId ?? throw new ArgumentNullException("shopId");
		foreach (ISalable item in itemsForSale)
		{
			this.AddForSale(item);
		}
		this.SetVisualTheme(null);
		this.setUpShopOwner(who, shopId);
		this.Initialize(currency, on_purchase, on_sell, playOpenSound);
	}

	/// <summary>Set the visual theme for the shop menu.</summary>
	/// <param name="theme">The visual theme to display, or <c>null</c> for the default theme.</param>
	/// <remarks>The visual theme is usually set in <c>Data/Shops</c> instead of calling this method directly.</remarks>
	public void SetVisualTheme(ShopThemeData theme)
	{
		this.VisualTheme = new ShopCachedTheme(theme);
		if (this.upArrow != null)
		{
			Rectangle bounds = new Rectangle(Game1.uiViewport.X, Game1.uiViewport.Y, Game1.uiViewport.Width, Game1.uiViewport.Height);
			this.gameWindowSizeChanged(bounds, bounds);
		}
	}

	/// <summary>Initialize the shop menu after the stock has been constructed.</summary>
	/// <param name="currency">The currency in which all items in the shop should be priced. The valid values are 0 (money), 1 (star tokens), 2 (Qi coins), and 4 (Qi gems).</param>
	/// <param name="onPurchase">A callback to invoke when the player purchases an item, if any.</param>
	/// <param name="onSell">A callback to invoke when the player sells an item, if any.</param>
	/// <param name="playOpenSound">Whether to play the open-menu sound.</param>
	private void Initialize(int currency, OnPurchaseDelegate onPurchase, Func<ISalable, bool> onSell, bool playOpenSound)
	{
		ShopCachedTheme theme = this.VisualTheme;
		this.updatePosition();
		base.upperRightCloseButton = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen + base.width - 36, base.yPositionOnScreen - 8, 48, 48), Game1.mouseCursors, new Rectangle(337, 494, 12, 12), 4f);
		this.currency = currency;
		this.onPurchase = onPurchase;
		this.onSell = onSell;
		Game1.player.forceCanMove();
		if (playOpenSound)
		{
			this.PlayOpenSound();
		}
		this.inventory = new InventoryMenu(base.xPositionOnScreen + base.width, base.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth + 320 + 40, playerInventory: false, null, highlightItemToSell)
		{
			showGrayedOutSlots = true
		};
		this.inventory.movePosition(-this.inventory.width - 32, 0);
		this.upArrow = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen + base.width + 16, base.yPositionOnScreen + 16, 44, 48), theme.ScrollUpTexture, theme.ScrollUpSourceRect, 4f)
		{
			myID = 97865,
			downNeighborID = 106,
			leftNeighborID = 3546
		};
		this.downArrow = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen + base.width + 16, base.yPositionOnScreen + base.height - 64, 44, 48), theme.ScrollDownTexture, theme.ScrollDownSourceRect, 4f)
		{
			myID = 106,
			upNeighborID = 97865,
			leftNeighborID = 3546
		};
		this.scrollBar = new ClickableTextureComponent(new Rectangle(this.upArrow.bounds.X + 12, this.upArrow.bounds.Y + this.upArrow.bounds.Height + 4, 24, 40), theme.ScrollBarFrontTexture, theme.ScrollBarFrontSourceRect, 4f);
		this.scrollBarRunner = new Rectangle(this.scrollBar.bounds.X, this.upArrow.bounds.Y + this.upArrow.bounds.Height + 4, this.scrollBar.bounds.Width, base.height - 64 - this.upArrow.bounds.Height - 28);
		for (int i = 0; i < 4; i++)
		{
			this.forSaleButtons.Add(new ClickableComponent(new Rectangle(base.xPositionOnScreen + 16, base.yPositionOnScreen + 16 + i * ((base.height - 256) / 4), base.width - 32, (base.height - 256) / 4 + 4), i.ToString() ?? "")
			{
				myID = i + 3546,
				rightNeighborID = 97865,
				fullyImmutable = true
			});
		}
		this.updateSaleButtonNeighbors();
		this.setUpStoreForContext();
		if (this.tabButtons.Count > 0)
		{
			foreach (ClickableComponent forSaleButton in this.forSaleButtons)
			{
				forSaleButton.leftNeighborID = -99998;
			}
		}
		this.applyTab();
		foreach (ClickableComponent item in this.inventory.GetBorder(InventoryMenu.BorderSide.Top))
		{
			item.upNeighborID = -99998;
		}
		if (Game1.options.snappyMenus && Game1.options.gamepadControls)
		{
			this.populateClickableComponentList();
			this.snapToDefaultClickableComponent();
		}
		if (currency == 4)
		{
			int tickOpened = Game1.ticks;
			Game1.specialCurrencyDisplay.ShowCurrency("qiGems", () => Game1.ticks == tickOpened || (Game1.activeClickableMenu == this && this.currency == 4), 0f);
		}
	}

	/// <summary>Add an item to sell in the menu.</summary>
	/// <param name="item">The item instance to sell.</param>
	/// <param name="stock">The stock information, or <c>null</c> to create it automatically.</param>
	public void AddForSale(ISalable item, ItemStockInformation stock = null)
	{
		if (item.IsRecipe)
		{
			if (Game1.player.knowsRecipe(item.Name))
			{
				return;
			}
			item.Stack = 1;
		}
		this.forSale.Add(item);
		this.itemPriceAndStock.Add(item, stock ?? new ItemStockInformation(item.salePrice(), item.Stack));
	}

	public void updateSaleButtonNeighbors()
	{
		ClickableComponent lastValidButton = this.forSaleButtons[0];
		for (int i = 0; i < this.forSaleButtons.Count; i++)
		{
			ClickableComponent button = this.forSaleButtons[i];
			button.upNeighborImmutable = true;
			button.downNeighborImmutable = true;
			button.upNeighborID = ((i > 0) ? (i + 3546 - 1) : (-7777));
			button.downNeighborID = ((i < 3 && i < this.forSale.Count - 1) ? (i + 3546 + 1) : (-7777));
			if (i >= this.forSale.Count)
			{
				if (button == base.currentlySnappedComponent)
				{
					base.currentlySnappedComponent = lastValidButton;
					if (Game1.options.SnappyMenus)
					{
						this.snapCursorToCurrentSnappedComponent();
					}
				}
			}
			else
			{
				lastValidButton = button;
			}
		}
	}

	public virtual void setUpStoreForContext()
	{
		this.tabButtons = null;
		switch (this.ShopId)
		{
		case "JunimoFurnitureCatalogue":
		case "WizardFurnitureCatalogue":
		case "RetroFurnitureCatalogue":
		case "TrashFurnitureCatalogue":
		case "Furniture Catalogue":
		case "JojaFurnitureCatalogue":
			this.UseFurnitureCatalogueTabs();
			break;
		case "Catalogue":
			this.UseCatalogueTabs();
			break;
		case "ReturnedDonations":
			this.UseNoTabs();
			this._isStorageShop = true;
			break;
		case "FishTank":
			this.UseNoTabs();
			this._isStorageShop = true;
			break;
		case "Dresser":
			this.categoriesToSellHere.AddRange(new int[4] { -95, -100, -97, -96 });
			this.UseDresserTabs();
			this._isStorageShop = true;
			break;
		default:
			this.UseNoTabs();
			break;
		}
		if (this._isStorageShop)
		{
			this.purchaseSound = null;
			this.purchaseRepeatSound = null;
		}
	}

	/// <summary>Remove the filter tabs, if any.</summary>
	public void UseNoTabs()
	{
		this.tabButtons = new List<ShopTabClickableTextureComponent>();
		this.repositionTabs();
	}

	/// <summary>Add the filter tabs for a furniture catalogue (e.g. tables, seats, paintings, etc).</summary>
	public void UseFurnitureCatalogueTabs()
	{
		this.tabButtons = new List<ShopTabClickableTextureComponent>
		{
			new ShopTabClickableTextureComponent(new Rectangle(0, 0, 64, 64), Game1.mouseCursors2, new Rectangle(96, 48, 16, 16), 4f)
			{
				myID = 99999,
				upNeighborID = -99998,
				downNeighborID = -99998,
				rightNeighborID = 3546,
				Filter = (ISalable _) => true
			},
			new ShopTabClickableTextureComponent(new Rectangle(0, 0, 64, 64), Game1.mouseCursors2, new Rectangle(80, 48, 16, 16), 4f)
			{
				myID = 100000,
				upNeighborID = -99998,
				downNeighborID = -99998,
				rightNeighborID = 3546,
				Filter = (ISalable item) => item is Furniture furniture && (furniture.IsTable() || furniture.furniture_type.Value == 4)
			},
			new ShopTabClickableTextureComponent(new Rectangle(0, 0, 64, 64), Game1.mouseCursors2, new Rectangle(64, 48, 16, 16), 4f)
			{
				myID = 100001,
				upNeighborID = -99998,
				downNeighborID = -99998,
				rightNeighborID = 3546,
				Filter = (ISalable item) => item is Furniture furniture && (furniture.furniture_type.Value == 0 || furniture.furniture_type.Value == 1 || furniture.furniture_type.Value == 2 || furniture.furniture_type.Value == 3)
			},
			new ShopTabClickableTextureComponent(new Rectangle(0, 0, 64, 64), Game1.mouseCursors2, new Rectangle(64, 64, 16, 16), 4f)
			{
				myID = 100002,
				upNeighborID = -99998,
				downNeighborID = -99998,
				rightNeighborID = 3546,
				Filter = (ISalable item) => item is Furniture furniture && (furniture.furniture_type.Value == 6 || furniture.furniture_type.Value == 13)
			},
			new ShopTabClickableTextureComponent(new Rectangle(0, 0, 64, 64), Game1.mouseCursors2, new Rectangle(96, 64, 16, 16), 4f)
			{
				myID = 100003,
				upNeighborID = -99998,
				downNeighborID = -99998,
				rightNeighborID = 3546,
				Filter = (ISalable item) => item is Furniture furniture && furniture.furniture_type.Value == 12
			},
			new ShopTabClickableTextureComponent(new Rectangle(0, 0, 64, 64), Game1.mouseCursors2, new Rectangle(80, 64, 16, 16), 4f)
			{
				myID = 100004,
				upNeighborID = -99998,
				downNeighborID = -99998,
				rightNeighborID = 3546,
				Filter = (ISalable item) => item is Furniture furniture && (furniture.furniture_type.Value == 7 || furniture.furniture_type.Value == 17 || furniture.furniture_type.Value == 10 || furniture.furniture_type.Value == 8 || furniture.furniture_type.Value == 9 || furniture.furniture_type.Value == 14)
			}
		};
		this.repositionTabs();
	}

	/// <summary>Add the filter tabs for a catalogue (e.g. flooring and wallpaper).</summary>
	public void UseCatalogueTabs()
	{
		this.tabButtons = new List<ShopTabClickableTextureComponent>
		{
			new ShopTabClickableTextureComponent(new Rectangle(0, 0, 64, 64), Game1.mouseCursors2, new Rectangle(96, 48, 16, 16), 4f)
			{
				myID = 99999,
				upNeighborID = -99998,
				downNeighborID = -99998,
				rightNeighborID = 3546,
				Filter = (ISalable item) => true
			},
			new ShopTabClickableTextureComponent(new Rectangle(0, 0, 64, 64), Game1.mouseCursors2, new Rectangle(48, 64, 16, 16), 4f)
			{
				myID = 100000,
				upNeighborID = -99998,
				downNeighborID = -99998,
				rightNeighborID = 3546,
				Filter = (ISalable item) => item is Wallpaper wallpaper && wallpaper.isFloor.Value
			},
			new ShopTabClickableTextureComponent(new Rectangle(0, 0, 64, 64), Game1.mouseCursors2, new Rectangle(32, 64, 16, 16), 4f)
			{
				myID = 100001,
				upNeighborID = -99998,
				downNeighborID = -99998,
				rightNeighborID = 3546,
				Filter = (ISalable item) => item is Wallpaper wallpaper && !wallpaper.isFloor.Value
			}
		};
		this.repositionTabs();
	}

	/// <summary>Add the filter tabs for a dresser (e.g. hats, shirts, pants, etc).</summary>
	public void UseDresserTabs()
	{
		this.tabButtons = new List<ShopTabClickableTextureComponent>
		{
			new ShopTabClickableTextureComponent(new Rectangle(0, 0, 64, 64), Game1.mouseCursors2, new Rectangle(0, 48, 16, 16), 4f)
			{
				myID = 99999,
				upNeighborID = -99998,
				downNeighborID = -99998,
				rightNeighborID = 3546,
				Filter = (ISalable item) => true
			},
			new ShopTabClickableTextureComponent(new Rectangle(0, 0, 64, 64), Game1.mouseCursors2, new Rectangle(16, 48, 16, 16), 4f)
			{
				myID = 100000,
				upNeighborID = -99998,
				downNeighborID = -99998,
				rightNeighborID = 3546,
				Filter = (ISalable salable) => salable is Item item && item.Category == -95
			},
			new ShopTabClickableTextureComponent(new Rectangle(0, 0, 64, 64), Game1.mouseCursors2, new Rectangle(32, 48, 16, 16), 4f)
			{
				myID = 100001,
				upNeighborID = -99998,
				downNeighborID = -99998,
				rightNeighborID = 3546,
				Filter = (ISalable salable) => salable is Clothing clothing && clothing.clothesType.Value == Clothing.ClothesType.SHIRT
			},
			new ShopTabClickableTextureComponent(new Rectangle(0, 0, 64, 64), Game1.mouseCursors2, new Rectangle(48, 48, 16, 16), 4f)
			{
				myID = 100002,
				upNeighborID = -99998,
				downNeighborID = -99998,
				rightNeighborID = 3546,
				Filter = (ISalable salable) => salable is Clothing clothing && clothing.clothesType.Value == Clothing.ClothesType.PANTS
			},
			new ShopTabClickableTextureComponent(new Rectangle(0, 0, 64, 64), Game1.mouseCursors2, new Rectangle(0, 64, 16, 16), 4f)
			{
				myID = 100003,
				upNeighborID = -99998,
				downNeighborID = -99998,
				rightNeighborID = 3546,
				Filter = (ISalable salable) => salable is Item item && item.Category == -97
			},
			new ShopTabClickableTextureComponent(new Rectangle(0, 0, 64, 64), Game1.mouseCursors2, new Rectangle(16, 64, 16, 16), 4f)
			{
				myID = 100004,
				upNeighborID = -99998,
				downNeighborID = -99998,
				rightNeighborID = 3546,
				Filter = (ISalable salable) => salable is Item item && item.Category == -96
			}
		};
		this.repositionTabs();
	}

	public void repositionTabs()
	{
		for (int i = 0; i < this.tabButtons.Count; i++)
		{
			if (i == this.currentTab)
			{
				this.tabButtons[i].bounds.X = base.xPositionOnScreen - 56;
			}
			else
			{
				this.tabButtons[i].bounds.X = base.xPositionOnScreen - 64;
			}
			this.tabButtons[i].bounds.Y = base.yPositionOnScreen + i * 16 * 4 + 16;
		}
	}

	protected override void customSnapBehavior(int direction, int oldRegion, int oldID)
	{
		switch (direction)
		{
		case 2:
		{
			if (this.currentItemIndex < Math.Max(0, this.forSale.Count - 4))
			{
				this.downArrowPressed();
				break;
			}
			int emptySlot = -1;
			for (int i = 0; i < 12; i++)
			{
				this.inventory.inventory[i].upNeighborID = oldID;
				if (emptySlot == -1 && this.heldItem != null)
				{
					IList<Item> actualInventory = this.inventory.actualInventory;
					if (actualInventory != null && actualInventory.Count > i && this.inventory.actualInventory[i] == null)
					{
						emptySlot = i;
					}
				}
			}
			base.currentlySnappedComponent = base.getComponentWithID((emptySlot != -1) ? emptySlot : 0);
			this.snapCursorToCurrentSnappedComponent();
			break;
		}
		case 0:
			if (this.currentItemIndex > 0)
			{
				this.upArrowPressed();
				base.currentlySnappedComponent = base.getComponentWithID(3546);
				this.snapCursorToCurrentSnappedComponent();
			}
			break;
		}
	}

	public override void snapToDefaultClickableComponent()
	{
		base.currentlySnappedComponent = base.getComponentWithID(3546);
		this.snapCursorToCurrentSnappedComponent();
	}

	public void setUpShopOwner(string who, string shopId)
	{
		if (!DataLoader.Shops(Game1.content).TryGetValue(shopId, out var shopData))
		{
			return;
		}
		foreach (ShopOwnerData owner in ShopBuilder.GetCurrentOwners(shopData))
		{
			if (owner.IsValid(who))
			{
				this.SetUpShopOwner(owner);
				break;
			}
		}
	}

	/// <summary>Set the shop portrait and dialogue.</summary>
	/// <param name="ownerData">The owner entry in the shop data.</param>
	/// <param name="owner">The specific NPC which matches the <paramref name="ownerData" />, if set.</param>
	public void SetUpShopOwner(ShopOwnerData ownerData, NPC owner = null)
	{
		if (ownerData == null)
		{
			this.portraitTexture = null;
			this.potraitPersonDialogue = null;
			return;
		}
		string dialogueText = null;
		bool disableDialogue = false;
		if (ownerData.Dialogues != null)
		{
			Random random = (ownerData.RandomizeDialogueOnOpen ? Game1.random : Utility.CreateRandom(Game1.uniqueIDForThisGame, Game1.stats.DaysPlayed));
			foreach (ShopDialogueData dialogue in ownerData.Dialogues)
			{
				if (GameStateQuery.CheckConditions(dialogue.Condition))
				{
					string rawText = dialogue.Dialogue;
					List<string> randomDialogue = dialogue.RandomDialogue;
					if (randomDialogue != null && randomDialogue.Any())
					{
						rawText = random.ChooseFrom(dialogue.RandomDialogue);
					}
					dialogueText = TokenParser.ParseText(rawText, random, ParseDialogueSubstitution);
					break;
				}
			}
			if (string.IsNullOrWhiteSpace(dialogueText))
			{
				disableDialogue = true;
			}
		}
		this.portraitTexture = this.TryLoadPortrait(ownerData, owner);
		if (!disableDialogue)
		{
			this.potraitPersonDialogue = Game1.parseText(dialogueText ?? Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11457"), Game1.dialogueFont, 304);
		}
	}

	/// <summary>Get the portrait to show for the selected NPC, if any.</summary>
	/// <param name="ownerData">The shop owner data.</param>
	/// <param name="owner">The specific NPC which matches the <paramref name="ownerData" />, if set.</param>
	public Texture2D TryLoadPortrait(ShopOwnerData ownerData, NPC owner)
	{
		if (ownerData.Type == ShopOwnerType.None)
		{
			return null;
		}
		if (ownerData.Portrait != null)
		{
			if (!string.IsNullOrWhiteSpace(ownerData.Portrait))
			{
				if (Game1.content.DoesAssetExist<Texture2D>(ownerData.Portrait))
				{
					return Game1.content.Load<Texture2D>(ownerData.Portrait);
				}
				NPC npc = Game1.getCharacterFromName(ownerData.Portrait);
				if (npc?.Portrait != null)
				{
					return npc.Portrait;
				}
			}
			return null;
		}
		if (owner?.Portrait != null)
		{
			return owner.Portrait;
		}
		if (ownerData.Type == ShopOwnerType.NamedNpc && !string.IsNullOrWhiteSpace(ownerData.Name))
		{
			NPC npc2 = Game1.getCharacterFromName(ownerData.Name);
			if (npc2?.Portrait != null)
			{
				return npc2.Portrait;
			}
		}
		return null;
	}

	public bool ParseDialogueSubstitution(string[] query, out string replacement, Random random, Farmer player)
	{
		if (query[0] == "SuggestedItem")
		{
			string interval = ArgUtility.Get(query, 1, "day");
			string syncKey = ArgUtility.Get(query, 2, this.ShopId);
			if (!Utility.TryCreateIntervalRandom(interval, syncKey, out random, out var error))
			{
				Game1.log.Error($"Failed parsing [SuggestedItem {string.Join(" ", query)}] in dialogue shop '{this.ShopId}': {error}");
				random = Utility.CreateRandom(Game1.ticks);
			}
			if (Utility.TryGetRandom(this.itemPriceAndStock, out var suggestedItem, out var _, random))
			{
				replacement = suggestedItem.DisplayName;
				return true;
			}
		}
		replacement = null;
		return false;
	}

	public bool highlightItemToSell(Item i)
	{
		if (this.heldItem != null)
		{
			return this.heldItem.canStackWith(i);
		}
		if (this.categoriesToSellHere.Contains(i.Category))
		{
			return true;
		}
		foreach (List<string> item in this.tagsToSellHere)
		{
			bool fail = false;
			foreach (string tag in item)
			{
				if (!i.HasContextTag(tag))
				{
					fail = true;
					break;
				}
			}
			if (!fail)
			{
				return true;
			}
		}
		return false;
	}

	public static int getPlayerCurrencyAmount(Farmer who, int currencyType)
	{
		return currencyType switch
		{
			0 => who.Money, 
			1 => who.festivalScore, 
			2 => who.clubCoins, 
			4 => who.QiGems, 
			_ => 0, 
		};
	}

	/// <inheritdoc />
	public override void leftClickHeld(int x, int y)
	{
		base.leftClickHeld(x, y);
		if (this.scrolling)
		{
			int y2 = this.scrollBar.bounds.Y;
			this.scrollBar.bounds.Y = Math.Min(base.yPositionOnScreen + base.height - 64 - 12 - this.scrollBar.bounds.Height, Math.Max(y, base.yPositionOnScreen + this.upArrow.bounds.Height + 20));
			float percentage = (float)(y - this.scrollBarRunner.Y) / (float)this.scrollBarRunner.Height;
			this.currentItemIndex = Math.Min(Math.Max(0, this.forSale.Count - 4), Math.Max(0, (int)((float)this.forSale.Count * percentage)));
			this.setScrollBarToCurrentIndex();
			this.updateSaleButtonNeighbors();
			if (y2 != this.scrollBar.bounds.Y)
			{
				Game1.playSound("shiny4");
			}
		}
	}

	/// <inheritdoc />
	public override void releaseLeftClick(int x, int y)
	{
		base.releaseLeftClick(x, y);
		this.scrolling = false;
	}

	private void setScrollBarToCurrentIndex()
	{
		if (this.forSale.Count > 0)
		{
			float percentage = (float)this.scrollBarRunner.Height / (float)Math.Max(1, this.forSale.Count - 4 + 1);
			this.scrollBar.bounds.Y = (int)(percentage * (float)this.currentItemIndex + (float)this.upArrow.bounds.Bottom + 4f);
			if (this.currentItemIndex == this.forSale.Count - 4)
			{
				this.scrollBar.bounds.Y = this.downArrow.bounds.Y - this.scrollBar.bounds.Height - 4;
			}
		}
	}

	/// <inheritdoc />
	public override void receiveScrollWheelAction(int direction)
	{
		base.receiveScrollWheelAction(direction);
		if (direction > 0 && this.currentItemIndex > 0)
		{
			this.upArrowPressed();
			Game1.playSound("shiny4");
		}
		else if (direction < 0 && this.currentItemIndex < Math.Max(0, this.forSale.Count - 4))
		{
			this.downArrowPressed();
			Game1.playSound("shiny4");
		}
	}

	private void downArrowPressed()
	{
		this.downArrow.scale = this.downArrow.baseScale;
		this.currentItemIndex++;
		this.setScrollBarToCurrentIndex();
		this.updateSaleButtonNeighbors();
	}

	private void upArrowPressed()
	{
		this.upArrow.scale = this.upArrow.baseScale;
		this.currentItemIndex--;
		this.setScrollBarToCurrentIndex();
		this.updateSaleButtonNeighbors();
	}

	/// <inheritdoc />
	public override void receiveKeyPress(Keys key)
	{
		if (Game1.options.doesInputListContain(Game1.options.menuButton, key) && this.heldItem is Item item)
		{
			this.heldItem = null;
			if (Utility.CollectOrDrop(item))
			{
				Game1.playSound("stoneStep");
			}
			else
			{
				Game1.playSound("throwDownITem");
			}
		}
		else
		{
			base.receiveKeyPress(key);
		}
	}

	/// <inheritdoc />
	public override void receiveLeftClick(int x, int y, bool playSound = true)
	{
		base.receiveLeftClick(x, y);
		if (Game1.activeClickableMenu == null)
		{
			return;
		}
		Vector2 snappedPosition = this.inventory.snapToClickableComponent(x, y);
		if (this.downArrow.containsPoint(x, y) && this.currentItemIndex < Math.Max(0, this.forSale.Count - 4))
		{
			this.downArrowPressed();
			Game1.playSound("shwip");
		}
		else if (this.upArrow.containsPoint(x, y) && this.currentItemIndex > 0)
		{
			this.upArrowPressed();
			Game1.playSound("shwip");
		}
		else if (this.scrollBar.containsPoint(x, y))
		{
			this.scrolling = true;
		}
		else if (!this.downArrow.containsPoint(x, y) && x > base.xPositionOnScreen + base.width && x < base.xPositionOnScreen + base.width + 128 && y > base.yPositionOnScreen && y < base.yPositionOnScreen + base.height)
		{
			this.scrolling = true;
			this.leftClickHeld(x, y);
			this.releaseLeftClick(x, y);
		}
		for (int i = 0; i < this.tabButtons.Count; i++)
		{
			if (this.tabButtons[i].containsPoint(x, y))
			{
				this.switchTab(i);
			}
		}
		this.currentItemIndex = Math.Max(0, Math.Min(this.forSale.Count - 4, this.currentItemIndex));
		if (this.safetyTimer <= 0)
		{
			if (this.heldItem == null && !this.readOnly)
			{
				Item toSell = this.inventory.leftClick(x, y, null, playSound: false);
				if (toSell != null)
				{
					if (this.onSell != null)
					{
						this.onSell(toSell);
					}
					else
					{
						int sell_unit_price = (int)((float)toSell.sellToStorePrice(-1L) * this.sellPercentage);
						ShopMenu.chargePlayer(Game1.player, this.currency, -sell_unit_price * toSell.Stack);
						int coins = toSell.Stack / 8 + 2;
						for (int j = 0; j < coins; j++)
						{
							this.animations.Add(new TemporaryAnimatedSprite("TileSheets\\debris", new Rectangle(Game1.random.Next(2) * 16, 64, 16, 16), 9999f, 1, 999, snappedPosition + new Vector2(32f, 32f), flicker: false, flipped: false)
							{
								alphaFade = 0.025f,
								motion = new Vector2(Game1.random.Next(-3, 4), -4f),
								acceleration = new Vector2(0f, 0.5f),
								delayBeforeAnimationStart = j * 25,
								scale = 2f
							});
							this.animations.Add(new TemporaryAnimatedSprite("TileSheets\\debris", new Rectangle(Game1.random.Next(2) * 16, 64, 16, 16), 9999f, 1, 999, snappedPosition + new Vector2(32f, 32f), flicker: false, flipped: false)
							{
								scale = 4f,
								alphaFade = 0.025f,
								delayBeforeAnimationStart = j * 50,
								motion = Utility.getVelocityTowardPoint(new Point((int)snappedPosition.X + 32, (int)snappedPosition.Y + 32), new Vector2(base.xPositionOnScreen - 36, base.yPositionOnScreen + base.height - this.inventory.height - 16), 8f),
								acceleration = Utility.getVelocityTowardPoint(new Point((int)snappedPosition.X + 32, (int)snappedPosition.Y + 32), new Vector2(base.xPositionOnScreen - 36, base.yPositionOnScreen + base.height - this.inventory.height - 16), 0.5f)
							});
						}
						ISalable buyback_item = null;
						if (this.CanBuyback())
						{
							buyback_item = this.AddBuybackItem(toSell, sell_unit_price, toSell.Stack);
						}
						if (toSell is Object sellObj && sellObj.edibility.Value != -300)
						{
							Item stackClone = sellObj.getOne();
							stackClone.Stack = sellObj.Stack;
							if (buyback_item != null && this.buyBackItemsToResellTomorrow.TryGetValue(buyback_item, out var soldTomorrowItem))
							{
								soldTomorrowItem.Stack += sellObj.Stack;
							}
							else if (Game1.currentLocation is ShopLocation shopLocation)
							{
								if (buyback_item != null)
								{
									this.buyBackItemsToResellTomorrow[buyback_item] = stackClone;
								}
								shopLocation.itemsToStartSellingTomorrow.Add(stackClone);
							}
						}
						Game1.playSound("sell");
						Game1.playSound("purchase");
						if (this.inventory.getItemAt(x, y) == null)
						{
							this.animations.Add(new TemporaryAnimatedSprite(5, snappedPosition + new Vector2(32f, 32f), Color.White)
							{
								motion = new Vector2(0f, -0.5f)
							});
						}
					}
					this.updateSaleButtonNeighbors();
				}
			}
			else
			{
				this.heldItem = this.inventory.leftClick(x, y, this.heldItem as Item);
			}
			for (int k = 0; k < this.forSaleButtons.Count; k++)
			{
				if (this.currentItemIndex + k >= this.forSale.Count || !this.forSaleButtons[k].containsPoint(x, y))
				{
					continue;
				}
				int index = this.currentItemIndex + k;
				if (this.forSale[index] != null)
				{
					int toBuy = ((!Game1.oldKBState.IsKeyDown(Keys.LeftShift)) ? 1 : Math.Min(Math.Min((!Game1.oldKBState.IsKeyDown(Keys.LeftControl)) ? 5 : (Game1.oldKBState.IsKeyDown(Keys.D1) ? 999 : 25), ShopMenu.getPlayerCurrencyAmount(Game1.player, this.currency) / Math.Max(1, this.itemPriceAndStock[this.forSale[index]].Price)), Math.Max(1, this.itemPriceAndStock[this.forSale[index]].Stock)));
					if (this.ShopId == "ReturnedDonations")
					{
						toBuy = this.itemPriceAndStock[this.forSale[index]].Stock;
					}
					toBuy = Math.Min(toBuy, this.forSale[index].maximumStackSize());
					if (toBuy == -1)
					{
						toBuy = 1;
					}
					if (this.canPurchaseCheck != null && !this.canPurchaseCheck(index))
					{
						return;
					}
					if (toBuy > 0 && this.tryToPurchaseItem(this.forSale[index], this.heldItem, toBuy, x, y))
					{
						this.itemPriceAndStock.Remove(this.forSale[index]);
						this.forSale.RemoveAt(index);
					}
					else if (toBuy <= 0)
					{
						if (this.itemPriceAndStock[this.forSale[index]].Price > 0)
						{
							Game1.dayTimeMoneyBox.moneyShakeTimer = 1000;
						}
						Game1.playSound("cancel");
					}
					if (this.heldItem != null && (this._isStorageShop || Game1.options.SnappyMenus || (Game1.oldKBState.IsKeyDown(Keys.LeftShift) && (this.heldItem.maximumStackSize() == 1 || this.heldItem.Stack == 999))) && Game1.activeClickableMenu is ShopMenu && Game1.player.addItemToInventoryBool(this.heldItem as Item))
					{
						this.heldItem = null;
						DelayedAction.playSoundAfterDelay("coin", 100);
					}
				}
				this.currentItemIndex = Math.Max(0, Math.Min(this.forSale.Count - 4, this.currentItemIndex));
				this.updateSaleButtonNeighbors();
				this.setScrollBarToCurrentIndex();
				return;
			}
		}
		if (this.readyToClose() && (x < base.xPositionOnScreen - 64 || y < base.yPositionOnScreen - 64 || x > base.xPositionOnScreen + base.width + 128 || y > base.yPositionOnScreen + base.height + 64))
		{
			base.exitThisMenu();
		}
	}

	public virtual bool CanBuyback()
	{
		return true;
	}

	public virtual void BuyBuybackItem(ISalable bought_item, int price, int stack)
	{
		Game1.player.totalMoneyEarned -= (uint)price;
		if (Game1.player.useSeparateWallets)
		{
			Game1.player.stats.IndividualMoneyEarned -= (uint)price;
		}
		if (this.buyBackItemsToResellTomorrow.TryGetValue(bought_item, out var sold_tomorrow_item))
		{
			sold_tomorrow_item.Stack -= stack;
			if (sold_tomorrow_item.Stack <= 0)
			{
				this.buyBackItemsToResellTomorrow.Remove(bought_item);
				(Game1.currentLocation as ShopLocation).itemsToStartSellingTomorrow.Remove(sold_tomorrow_item as Item);
			}
		}
	}

	public virtual ISalable AddBuybackItem(ISalable sold_item, int sell_unit_price, int stack)
	{
		ISalable target = null;
		while (stack > 0)
		{
			target = null;
			foreach (ISalable buyback_item in this.buyBackItems)
			{
				if (buyback_item.canStackWith(sold_item) && buyback_item.Stack < buyback_item.maximumStackSize())
				{
					target = buyback_item;
					break;
				}
			}
			if (target == null)
			{
				target = sold_item.GetSalableInstance();
				int amount_to_deposit = Math.Min(stack, target.maximumStackSize());
				this.buyBackItems.Add(target);
				this.itemPriceAndStock.Add(target, new ItemStockInformation(sell_unit_price, amount_to_deposit));
				target.Stack = 1;
				stack -= amount_to_deposit;
			}
			else
			{
				int amount_to_deposit2 = Math.Min(stack, target.maximumStackSize() - target.Stack);
				ItemStockInformation stock_data = this.itemPriceAndStock[target];
				stock_data.Stock += amount_to_deposit2;
				this.itemPriceAndStock[target] = stock_data;
				target.Stack = 1;
				stack -= amount_to_deposit2;
			}
		}
		this.forSale = this.itemPriceAndStock.Keys.ToList();
		return target;
	}

	public override bool IsAutomaticSnapValid(int direction, ClickableComponent a, ClickableComponent b)
	{
		if (direction == 1 && this.tabButtons.Contains(a) && this.tabButtons.Contains(b))
		{
			return false;
		}
		return base.IsAutomaticSnapValid(direction, a, b);
	}

	public virtual void switchTab(int new_tab)
	{
		this.currentTab = new_tab;
		Game1.playSound("shwip");
		this.applyTab();
		if (Game1.options.snappyMenus && Game1.options.gamepadControls)
		{
			this.snapCursorToCurrentSnappedComponent();
		}
	}

	public virtual void applyTab()
	{
		if (this.currentTab < 0 || this.currentTab >= this.tabButtons.Count)
		{
			this.forSale = this.itemPriceAndStock.Keys.ToList();
			return;
		}
		ShopTabClickableTextureComponent tab = this.tabButtons[this.currentTab];
		if (tab.Filter == null)
		{
			tab.Filter = (ISalable _) => true;
		}
		this.forSale.Clear();
		foreach (ISalable item in this.itemPriceAndStock.Keys)
		{
			if (tab.Filter(item))
			{
				this.forSale.Add(item);
			}
		}
		this.currentItemIndex = 0;
		this.setScrollBarToCurrentIndex();
		this.updateSaleButtonNeighbors();
	}

	public override bool readyToClose()
	{
		if (this.heldItem == null)
		{
			return this.animations.Count == 0;
		}
		return false;
	}

	public override void emergencyShutDown()
	{
		base.emergencyShutDown();
		if (this.heldItem != null)
		{
			Game1.player.addItemToInventoryBool(this.heldItem as Item);
			Game1.playSound("coin");
		}
	}

	/// <summary>Play the open-menu sound.</summary>
	public void PlayOpenSound()
	{
		Game1.playSound(this.openMenuSound);
	}

	/// <summary>Get whether all items in the shop have been purchased.</summary>
	public bool IsOutOfStock()
	{
		if (!this._isStorageShop)
		{
			return this.forSale.Count == 0;
		}
		return false;
	}

	public static void chargePlayer(Farmer who, int currencyType, int amount)
	{
		switch (currencyType)
		{
		case 0:
			who.Money -= amount;
			break;
		case 1:
			who.festivalScore -= amount;
			break;
		case 2:
			who.clubCoins -= amount;
			break;
		case 4:
			who.QiGems -= amount;
			break;
		case 3:
			break;
		}
	}

	public virtual void HandleSynchedItemPurchase(ISalable item, Farmer who, int number_purchased)
	{
		if (this.itemPriceAndStock.ContainsKey(item))
		{
			who.team.synchronizedShopStock.OnItemPurchased(this.ShopId, item, this.itemPriceAndStock, number_purchased);
		}
	}

	private bool tryToPurchaseItem(ISalable item, ISalable held_item, int stockToBuy, int x, int y)
	{
		if (this.readOnly)
		{
			return false;
		}
		ItemStockInformation stock = this.itemPriceAndStock[item];
		if (held_item == null)
		{
			if (stock.Stock == 0)
			{
				this.hoveredItem = null;
				return true;
			}
			if (stockToBuy > item.GetSalableInstance().maximumStackSize())
			{
				stockToBuy = Math.Max(1, item.GetSalableInstance().maximumStackSize());
			}
			int price = stock.Price * stockToBuy;
			string extraTradeItem = null;
			int extraTradeItemCount = 5;
			int stacksToBuy = stockToBuy * item.Stack;
			if (stock.TradeItem != null)
			{
				extraTradeItem = stock.TradeItem;
				if (stock.TradeItemCount.HasValue)
				{
					extraTradeItemCount = stock.TradeItemCount.Value;
				}
				extraTradeItemCount *= stockToBuy;
			}
			if (ShopMenu.getPlayerCurrencyAmount(Game1.player, this.currency) >= price && (extraTradeItem == null || this.HasTradeItem(extraTradeItem, extraTradeItemCount)))
			{
				this.heldItem = item.GetSalableInstance();
				this.heldItem.Stack = stacksToBuy;
				if (!this.heldItem.CanBuyItem(Game1.player) && !item.IsInfiniteStock() && !item.IsRecipe)
				{
					Game1.playSound("smallSelect");
					this.heldItem = null;
					return false;
				}
				if (this.CanBuyback() && this.buyBackItems.Contains(item))
				{
					this.BuyBuybackItem(item, price, stacksToBuy);
				}
				ShopMenu.chargePlayer(Game1.player, this.currency, price);
				if (!string.IsNullOrEmpty(extraTradeItem))
				{
					this.ConsumeTradeItem(extraTradeItem, extraTradeItemCount);
				}
				if (!this._isStorageShop && item.actionWhenPurchased(this.ShopId))
				{
					if (item.IsRecipe)
					{
						(item as Item)?.LearnRecipe();
						Game1.playSound("newRecipe");
					}
					held_item = null;
					this.heldItem = null;
				}
				else
				{
					if ((this.heldItem as Item)?.QualifiedItemId == "(O)858")
					{
						Game1.player.team.addQiGemsToTeam.Fire(this.heldItem.Stack);
						this.heldItem = null;
					}
					if (Game1.mouseClickPolling > 300)
					{
						if (this.purchaseRepeatSound != null)
						{
							Game1.playSound(this.purchaseRepeatSound);
						}
					}
					else if (this.purchaseSound != null)
					{
						Game1.playSound(this.purchaseSound);
					}
				}
				if (stock.Stock != int.MaxValue && !item.IsInfiniteStock())
				{
					this.HandleSynchedItemPurchase(item, Game1.player, stockToBuy);
					if (stock.ItemToSyncStack != null)
					{
						stock.ItemToSyncStack.Stack = stock.Stock;
					}
				}
				List<string> actionsOnPurchase = stock.ActionsOnPurchase;
				if (actionsOnPurchase != null && actionsOnPurchase.Count > 0)
				{
					foreach (string action in stock.ActionsOnPurchase)
					{
						if (!TriggerActionManager.TryRunAction(action, out var error, out var ex))
						{
							Game1.log.Error($"Shop {this.ShopId} ignored invalid action '{action}' on purchase of item '{item.QualifiedItemId}': {error}", ex);
						}
					}
				}
				if (this.onPurchase != null && this.onPurchase(item, Game1.player, stockToBuy, stock))
				{
					base.exitThisMenu();
				}
			}
			else
			{
				if (price > 0)
				{
					Game1.dayTimeMoneyBox.moneyShakeTimer = 1000;
				}
				Game1.playSound("cancel");
			}
		}
		else if (held_item.canStackWith(item))
		{
			stockToBuy = Math.Min(stockToBuy, (held_item.maximumStackSize() - held_item.Stack) / item.Stack);
			int stacksToBuy2 = stockToBuy * item.Stack;
			if (stockToBuy > 0)
			{
				int price2 = stock.Price * stockToBuy;
				string extraTradeItem2 = null;
				int extraTradeItemCount2 = 5;
				if (stock.TradeItem != null)
				{
					extraTradeItem2 = stock.TradeItem;
					if (stock.TradeItemCount.HasValue)
					{
						extraTradeItemCount2 = stock.TradeItemCount.Value;
					}
					extraTradeItemCount2 *= stockToBuy;
				}
				ISalable salableInstance = item.GetSalableInstance();
				salableInstance.Stack = stacksToBuy2;
				if (!salableInstance.CanBuyItem(Game1.player))
				{
					Game1.playSound("cancel");
					return false;
				}
				if (ShopMenu.getPlayerCurrencyAmount(Game1.player, this.currency) >= price2 && (extraTradeItem2 == null || this.HasTradeItem(extraTradeItem2, extraTradeItemCount2)))
				{
					this.heldItem.Stack += stacksToBuy2;
					if (this.CanBuyback() && this.buyBackItems.Contains(item))
					{
						this.BuyBuybackItem(item, price2, stacksToBuy2);
					}
					ShopMenu.chargePlayer(Game1.player, this.currency, price2);
					if (Game1.mouseClickPolling > 300)
					{
						if (this.purchaseRepeatSound != null)
						{
							Game1.playSound(this.purchaseRepeatSound);
						}
					}
					else if (this.purchaseSound != null)
					{
						Game1.playSound(this.purchaseSound);
					}
					if (extraTradeItem2 != null)
					{
						this.ConsumeTradeItem(extraTradeItem2, extraTradeItemCount2);
					}
					if (!this._isStorageShop && item.actionWhenPurchased(this.ShopId))
					{
						this.heldItem = null;
					}
					if (stock.Stock != int.MaxValue && !item.IsInfiniteStock())
					{
						this.HandleSynchedItemPurchase(item, Game1.player, stockToBuy);
						if (stock.ItemToSyncStack != null)
						{
							stock.ItemToSyncStack.Stack = stock.Stock;
						}
					}
					if (this.onPurchase != null && this.onPurchase(item, Game1.player, stockToBuy, stock))
					{
						base.exitThisMenu();
					}
				}
				else
				{
					if (price2 > 0)
					{
						Game1.dayTimeMoneyBox.moneyShakeTimer = 1000;
					}
					Game1.playSound("cancel");
				}
			}
		}
		if (stock.Stock <= 0)
		{
			this.buyBackItems.Remove(item);
			this.hoveredItem = null;
			return true;
		}
		return false;
	}

	/// <summary>Get whether the player's inventory contains a minimum number of a trade item.</summary>
	/// <param name="itemId">The qualified or unqualified item ID to find.</param>
	/// <param name="count">The number needed.</param>
	public bool HasTradeItem(string itemId, int count)
	{
		itemId = ItemRegistry.QualifyItemId(itemId);
		if (!(itemId == "(O)858"))
		{
			if (itemId == "(O)73")
			{
				return Game1.netWorldState.Value.GoldenWalnuts >= count;
			}
			return Game1.player.Items.ContainsId(itemId, count);
		}
		return Game1.player.QiGems >= count;
	}

	/// <summary>Reduce the number of an item held by the player.</summary>
	/// <param name="itemId">The qualified or unqualified item ID.</param>
	/// <param name="count">The number to remove.</param>
	public void ConsumeTradeItem(string itemId, int count)
	{
		itemId = ItemRegistry.QualifyItemId(itemId);
		if (!(itemId == "(O)858"))
		{
			if (itemId == "(O)73")
			{
				Game1.netWorldState.Value.GoldenWalnuts = Math.Max(0, Game1.netWorldState.Value.GoldenWalnuts - count);
			}
			else
			{
				Game1.player.Items.ReduceId(itemId, count);
			}
		}
		else
		{
			Game1.player.QiGems = Math.Max(0, Game1.player.QiGems - count);
		}
	}

	/// <inheritdoc />
	public override void receiveRightClick(int x, int y, bool playSound = true)
	{
		Vector2 snappedPosition = this.inventory.snapToClickableComponent(x, y);
		if (this.safetyTimer > 0)
		{
			return;
		}
		if (this.heldItem == null && !this.readOnly)
		{
			ISalable toSell = this.inventory.rightClick(x, y, null, playSound: false);
			if (toSell != null)
			{
				if (this.onSell != null)
				{
					this.onSell(toSell);
				}
				else
				{
					int sell_unit_price = (int)((float)toSell.sellToStorePrice(-1L) * this.sellPercentage);
					int sell_stack = toSell.Stack;
					ISalable sold_item = toSell;
					ShopMenu.chargePlayer(Game1.player, this.currency, -sell_unit_price * sell_stack);
					ISalable buyback_item = null;
					if (this.CanBuyback())
					{
						buyback_item = this.AddBuybackItem(toSell, sell_unit_price, sell_stack);
					}
					toSell = null;
					if (Game1.mouseClickPolling > 300)
					{
						if (this.purchaseRepeatSound != null)
						{
							Game1.playSound(this.purchaseRepeatSound);
						}
					}
					else if (this.purchaseSound != null)
					{
						Game1.playSound(this.purchaseSound);
					}
					int coins = 2;
					for (int i = 0; i < coins; i++)
					{
						this.animations.Add(new TemporaryAnimatedSprite("TileSheets\\debris", new Rectangle(Game1.random.Next(2) * 16, 64, 16, 16), 9999f, 1, 999, snappedPosition + new Vector2(32f, 32f), flicker: false, flipped: false)
						{
							alphaFade = 0.025f,
							motion = new Vector2(Game1.random.Next(-3, 4), -4f),
							acceleration = new Vector2(0f, 0.5f),
							delayBeforeAnimationStart = i * 25,
							scale = 2f
						});
						this.animations.Add(new TemporaryAnimatedSprite("TileSheets\\debris", new Rectangle(Game1.random.Next(2) * 16, 64, 16, 16), 9999f, 1, 999, snappedPosition + new Vector2(32f, 32f), flicker: false, flipped: false)
						{
							scale = 4f,
							alphaFade = 0.025f,
							delayBeforeAnimationStart = i * 50,
							motion = Utility.getVelocityTowardPoint(new Point((int)snappedPosition.X + 32, (int)snappedPosition.Y + 32), new Vector2(base.xPositionOnScreen - 36, base.yPositionOnScreen + base.height - this.inventory.height - 16), 8f),
							acceleration = Utility.getVelocityTowardPoint(new Point((int)snappedPosition.X + 32, (int)snappedPosition.Y + 32), new Vector2(base.xPositionOnScreen - 36, base.yPositionOnScreen + base.height - this.inventory.height - 16), 0.5f)
						});
					}
					if (buyback_item != null && this.buyBackItemsToResellTomorrow.TryGetValue(buyback_item, out var soldTomorrowItem))
					{
						soldTomorrowItem.Stack += sell_stack;
					}
					else if (sold_item is Object obj && obj.edibility.Value != -300 && Game1.random.NextDouble() < 0.03999999910593033 && Game1.currentLocation is ShopLocation shopLocation)
					{
						ISalable sell_back_instance = sold_item.GetSalableInstance();
						if (buyback_item != null)
						{
							this.buyBackItemsToResellTomorrow[buyback_item] = sell_back_instance;
						}
						shopLocation.itemsToStartSellingTomorrow.Add(sell_back_instance as Item);
					}
					if (this.inventory.getItemAt(x, y) == null)
					{
						Game1.playSound("sell");
						this.animations.Add(new TemporaryAnimatedSprite(5, snappedPosition + new Vector2(32f, 32f), Color.White)
						{
							motion = new Vector2(0f, -0.5f)
						});
					}
				}
			}
		}
		else
		{
			this.heldItem = this.inventory.rightClick(x, y, this.heldItem as Item);
		}
		for (int j = 0; j < this.forSaleButtons.Count; j++)
		{
			if (this.currentItemIndex + j >= this.forSale.Count || !this.forSaleButtons[j].containsPoint(x, y))
			{
				continue;
			}
			int index = this.currentItemIndex + j;
			if (this.forSale[index] == null)
			{
				break;
			}
			int toBuy = 1;
			if (this.itemPriceAndStock[this.forSale[index]].Price > 0)
			{
				toBuy = ((!Game1.oldKBState.IsKeyDown(Keys.LeftShift)) ? 1 : Math.Min(Math.Min((!Game1.oldKBState.IsKeyDown(Keys.LeftControl)) ? 5 : (Game1.oldKBState.IsKeyDown(Keys.OemTilde) ? 999 : 25), ShopMenu.getPlayerCurrencyAmount(Game1.player, this.currency) / this.itemPriceAndStock[this.forSale[index]].Price), this.itemPriceAndStock[this.forSale[index]].Stock));
			}
			if (this.canPurchaseCheck == null || this.canPurchaseCheck(index))
			{
				if (toBuy > 0 && this.tryToPurchaseItem(this.forSale[index], this.heldItem, toBuy, x, y))
				{
					this.itemPriceAndStock.Remove(this.forSale[index]);
					this.forSale.RemoveAt(index);
				}
				if (this.heldItem != null && (this._isStorageShop || Game1.options.SnappyMenus) && Game1.activeClickableMenu is ShopMenu && Game1.player.addItemToInventoryBool(this.heldItem as Item))
				{
					this.heldItem = null;
					DelayedAction.playSoundAfterDelay("coin", 100);
				}
				this.setScrollBarToCurrentIndex();
			}
			break;
		}
	}

	/// <inheritdoc />
	public override void performHoverAction(int x, int y)
	{
		base.performHoverAction(x, y);
		this.hoverText = "";
		this.hoveredItem = null;
		this.hoverPrice = -1;
		this.boldTitleText = "";
		this.upArrow.tryHover(x, y);
		this.downArrow.tryHover(x, y);
		this.scrollBar.tryHover(x, y);
		if (this.scrolling)
		{
			return;
		}
		for (int i = 0; i < this.forSaleButtons.Count; i++)
		{
			if (this.currentItemIndex + i < this.forSale.Count && this.forSaleButtons[i].containsPoint(x, y))
			{
				ISalable item = this.forSale[this.currentItemIndex + i];
				if (this.canPurchaseCheck == null || this.canPurchaseCheck(this.currentItemIndex + i))
				{
					this.hoverText = item.getDescription();
					this.boldTitleText = item.DisplayName;
					if (!this._isStorageShop)
					{
						this.hoverPrice = ((this.itemPriceAndStock != null && this.itemPriceAndStock.TryGetValue(item, out var stock)) ? stock.Price : item.salePrice());
					}
					this.hoveredItem = item;
					this.forSaleButtons[i].scale = Math.Min(this.forSaleButtons[i].scale + 0.03f, 1.1f);
				}
			}
			else
			{
				this.forSaleButtons[i].scale = Math.Max(1f, this.forSaleButtons[i].scale - 0.03f);
			}
		}
		if (this.heldItem != null)
		{
			return;
		}
		foreach (ClickableComponent c in this.inventory.inventory)
		{
			if (!c.containsPoint(x, y))
			{
				continue;
			}
			Item i2 = this.inventory.getItemFromClickableComponent(c);
			if (i2 == null || (this.inventory.highlightMethod != null && !this.inventory.highlightMethod(i2)))
			{
				continue;
			}
			if (this._isStorageShop)
			{
				this.hoverText = i2.getDescription();
				this.boldTitleText = i2.DisplayName;
				this.hoveredItem = i2;
				continue;
			}
			this.hoverText = i2.DisplayName + " x" + i2.Stack;
			if (i2 is Object hovered_object && hovered_object.needsToBeDonated())
			{
				this.hoverText = this.hoverText + "\n\n" + i2.getDescription() + "\n";
			}
			this.hoverPrice = (int)((float)i2.sellToStorePrice(-1L) * this.sellPercentage) * i2.Stack;
		}
	}

	/// <inheritdoc />
	public override void update(GameTime time)
	{
		base.update(time);
		if (this.safetyTimer > 0)
		{
			this.safetyTimer -= time.ElapsedGameTime.Milliseconds;
		}
		if (this.poof != null && this.poof.update(time))
		{
			this.poof = null;
		}
		this.repositionTabs();
	}

	public void drawCurrency(SpriteBatch b)
	{
		if (!this._isStorageShop && this.currency == 0)
		{
			Game1.dayTimeMoneyBox.drawMoneyBox(b, base.xPositionOnScreen - 36, base.yPositionOnScreen + base.height - this.inventory.height - 12);
		}
	}

	/// <inheritdoc />
	public override void receiveGamePadButton(Buttons button)
	{
		base.receiveGamePadButton(button);
		if (button != Buttons.RightTrigger && button != Buttons.LeftTrigger)
		{
			return;
		}
		ClickableComponent clickableComponent = base.currentlySnappedComponent;
		if (clickableComponent != null && clickableComponent.myID >= 3546)
		{
			int emptySlot = -1;
			for (int i = 0; i < 12; i++)
			{
				this.inventory.inventory[i].upNeighborID = 3546 + this.forSaleButtons.Count - 1;
				if (emptySlot == -1 && this.heldItem != null)
				{
					IList<Item> actualInventory = this.inventory.actualInventory;
					if (actualInventory != null && actualInventory.Count > i && this.inventory.actualInventory[i] == null)
					{
						emptySlot = i;
					}
				}
			}
			base.currentlySnappedComponent = base.getComponentWithID((emptySlot != -1) ? emptySlot : 0);
			this.snapCursorToCurrentSnappedComponent();
		}
		else
		{
			this.snapToDefaultClickableComponent();
		}
		Game1.playSound("shiny4");
	}

	private string getHoveredItemExtraItemIndex()
	{
		if (this.hoveredItem != null && this.itemPriceAndStock != null && this.itemPriceAndStock.TryGetValue(this.hoveredItem, out var stock) && stock.TradeItem != null)
		{
			return stock.TradeItem;
		}
		return null;
	}

	private int getHoveredItemExtraItemAmount()
	{
		if (this.hoveredItem != null && this.itemPriceAndStock != null && this.itemPriceAndStock.TryGetValue(this.hoveredItem, out var stock) && stock.TradeItem != null && stock.TradeItemCount.HasValue)
		{
			return stock.TradeItemCount.Value;
		}
		return 5;
	}

	public void updatePosition()
	{
		base.width = 1000 + IClickableMenu.borderWidth * 2;
		base.height = 600 + IClickableMenu.borderWidth * 2;
		base.xPositionOnScreen = Game1.uiViewport.Width / 2 - (800 + IClickableMenu.borderWidth * 2) / 2;
		base.yPositionOnScreen = Game1.uiViewport.Height / 2 - (600 + IClickableMenu.borderWidth * 2) / 2;
		int portraitDrawPosition = base.xPositionOnScreen - 320;
		if ((this.portraitTexture == null && string.IsNullOrEmpty(this.potraitPersonDialogue)) || portraitDrawPosition <= 0 || !Game1.options.showMerchantPortraits)
		{
			base.xPositionOnScreen = Game1.uiViewport.Width / 2 - (1000 + IClickableMenu.borderWidth * 2) / 2;
			base.yPositionOnScreen = Game1.uiViewport.Height / 2 - (600 + IClickableMenu.borderWidth * 2) / 2;
		}
	}

	/// <inheritdoc />
	public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
	{
		ShopCachedTheme theme = this.VisualTheme;
		this.updatePosition();
		base.initializeUpperRightCloseButton();
		Game1.player.forceCanMove();
		this.inventory = new InventoryMenu(base.xPositionOnScreen + base.width, base.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth + 320 + 40, playerInventory: false, null, highlightItemToSell)
		{
			showGrayedOutSlots = true
		};
		this.inventory.movePosition(-this.inventory.width - 32, 0);
		this.upArrow = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen + base.width + 16, base.yPositionOnScreen + 16, 44, 48), theme.ScrollUpTexture, theme.ScrollUpSourceRect, 4f);
		this.downArrow = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen + base.width + 16, base.yPositionOnScreen + base.height - 64, 44, 48), theme.ScrollDownTexture, theme.ScrollDownSourceRect, 4f);
		this.scrollBar = new ClickableTextureComponent(new Rectangle(this.upArrow.bounds.X + 12, this.upArrow.bounds.Y + this.upArrow.bounds.Height + 4, 24, 40), theme.ScrollBarFrontTexture, theme.ScrollBarFrontSourceRect, 4f);
		this.scrollBarRunner = new Rectangle(this.scrollBar.bounds.X, this.upArrow.bounds.Y + this.upArrow.bounds.Height + 4, this.scrollBar.bounds.Width, base.height - 64 - this.upArrow.bounds.Height - 28);
		this.forSaleButtons.Clear();
		for (int i = 0; i < 4; i++)
		{
			this.forSaleButtons.Add(new ClickableComponent(new Rectangle(base.xPositionOnScreen + 16, base.yPositionOnScreen + 16 + i * ((base.height - 256) / 4), base.width - 32, (base.height - 256) / 4 + 4), i.ToString() ?? ""));
		}
		if (this.tabButtons.Count > 0)
		{
			foreach (ClickableComponent forSaleButton in this.forSaleButtons)
			{
				forSaleButton.leftNeighborID = -99998;
			}
		}
		this.repositionTabs();
		foreach (ClickableComponent item in this.inventory.GetBorder(InventoryMenu.BorderSide.Top))
		{
			item.upNeighborID = -99998;
		}
	}

	public void setItemPriceAndStock(Dictionary<ISalable, ItemStockInformation> new_stock)
	{
		this.itemPriceAndStock = new_stock;
		this.forSale = this.itemPriceAndStock.Keys.ToList();
		this.applyTab();
	}

	/// <inheritdoc />
	public override void draw(SpriteBatch b)
	{
		if (!Game1.options.showMenuBackground && !Game1.options.showClearBackgrounds)
		{
			b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
		}
		ShopCachedTheme theme = this.VisualTheme;
		IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(384, 373, 18, 18), base.xPositionOnScreen + base.width - this.inventory.width - 32 - 24, base.yPositionOnScreen + base.height - 256 + 40, this.inventory.width + 56, base.height - 448 + 20, Color.White, 4f);
		IClickableMenu.drawTextureBox(b, theme.WindowBorderTexture, theme.WindowBorderSourceRect, base.xPositionOnScreen, base.yPositionOnScreen, base.width, base.height - 256 + 32 + 4, Color.White, 4f);
		this.drawCurrency(b);
		for (int i = 0; i < this.forSaleButtons.Count; i++)
		{
			ClickableComponent forSaleButton = this.forSaleButtons[i];
			if (this.currentItemIndex + i >= this.forSale.Count)
			{
				continue;
			}
			bool failedCanPurchaseCheck = this.canPurchaseCheck != null && !this.canPurchaseCheck(this.currentItemIndex + i);
			ISalable item = this.forSale[this.currentItemIndex + i];
			ItemStockInformation stock = this.itemPriceAndStock[item];
			StackDrawType stackDrawType = this.GetStackDrawType(stock, item);
			string displayName = item.DisplayName;
			IClickableMenu.drawTextureBox(b, theme.ItemRowBackgroundTexture, theme.ItemRowBackgroundSourceRect, forSaleButton.bounds.X, forSaleButton.bounds.Y, forSaleButton.bounds.Width, forSaleButton.bounds.Height, (forSaleButton.containsPoint(Game1.getOldMouseX(), Game1.getOldMouseY()) && !this.scrolling) ? theme.ItemRowBackgroundHoverColor : Color.White, 4f, drawShadow: false);
			if (item.Stack > 1)
			{
				displayName = displayName + " x" + item.Stack;
			}
			if (item.ShouldDrawIcon())
			{
				b.Draw(theme.ItemIconBackgroundTexture, new Vector2(forSaleButton.bounds.X + 32 - 12, forSaleButton.bounds.Y + 24 - 4), theme.ItemIconBackgroundSourceRect, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
				Vector2 drawPos = new Vector2(forSaleButton.bounds.X + 32 - 8, forSaleButton.bounds.Y + 24);
				Color color = Color.White * ((!failedCanPurchaseCheck) ? 1f : 0.25f);
				int drawnStack = stock.Stock;
				item.drawInMenu(b, drawPos, 1f, 1f, 0.9f, StackDrawType.HideButShowQuality, color, drawShadow: true);
				if (drawnStack != int.MaxValue && this.ShopId != "ClintUpgrade" && ((stackDrawType == StackDrawType.Draw && drawnStack > 1) || stackDrawType == StackDrawType.Draw_OneInclusive))
				{
					Utility.drawTinyDigits(drawnStack, b, drawPos + new Vector2(64 - Utility.getWidthOfTinyDigitString(drawnStack, 3f) + 3, 47f), 3f, 1f, color);
				}
				if (this.buyBackItems.Contains(item))
				{
					b.Draw(Game1.mouseCursors2, new Vector2(forSaleButton.bounds.X + 32 - 8, forSaleButton.bounds.Y + 24), new Rectangle(64, 240, 16, 16), Color.White * ((!failedCanPurchaseCheck) ? 1f : 0.25f), 0f, new Vector2(8f, 8f), 4f, SpriteEffects.None, 1f);
				}
				string formattedDisplayName = displayName;
				bool hasPrice = stock.Price > 0;
				if (SpriteText.getWidthOfString(formattedDisplayName) > base.width - (hasPrice ? (150 + SpriteText.getWidthOfString(stock.Price + " ")) : 100) && formattedDisplayName.Length > (hasPrice ? 27 : 37))
				{
					formattedDisplayName = formattedDisplayName.Substring(0, hasPrice ? 27 : 37);
					formattedDisplayName += "...";
				}
				SpriteText.drawString(b, formattedDisplayName, forSaleButton.bounds.X + 96 + 8, forSaleButton.bounds.Y + 28, 999999, -1, 999999, failedCanPurchaseCheck ? 0.5f : 1f, 0.88f, junimoText: false, -1, "", theme.ItemRowTextColor);
			}
			else
			{
				SpriteText.drawString(b, displayName, forSaleButton.bounds.X + 32 + 8, forSaleButton.bounds.Y + 28, 999999, -1, 999999, failedCanPurchaseCheck ? 0.5f : 1f, 0.88f, junimoText: false, -1, "", theme.ItemRowTextColor);
			}
			int right = forSaleButton.bounds.Right;
			int tradeIconDrawY = forSaleButton.bounds.Y + 28 - 4;
			int tradeTextDrawY = forSaleButton.bounds.Y + 44;
			if (stock.Price > 0)
			{
				SpriteText.drawString(b, stock.Price + " ", right - SpriteText.getWidthOfString(stock.Price + " ") - 60, forSaleButton.bounds.Y + 28, 999999, -1, 999999, (ShopMenu.getPlayerCurrencyAmount(Game1.player, this.currency) >= stock.Price && !failedCanPurchaseCheck) ? 1f : 0.5f, 0.88f, junimoText: false, -1, "", theme.ItemRowTextColor);
				Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2(forSaleButton.bounds.Right - 52, forSaleButton.bounds.Y + 40 - 4), new Rectangle(193 + this.currency * 9, 373, 9, 10), Color.White * ((!failedCanPurchaseCheck) ? 1f : 0.25f), 0f, Vector2.Zero, 4f, flipped: false, -1f, -1, -1, (!failedCanPurchaseCheck) ? 0.35f : 0f);
				right -= SpriteText.getWidthOfString(stock.Price + " ") + 96;
				tradeIconDrawY = forSaleButton.bounds.Y + 20;
				tradeTextDrawY = forSaleButton.bounds.Y + 28;
			}
			if (stock.TradeItem != null)
			{
				int requiredItemCount = 5;
				string requiredItem = stock.TradeItem;
				if (requiredItem != null && stock.TradeItemCount.HasValue)
				{
					requiredItemCount = stock.TradeItemCount.Value;
				}
				bool hasEnoughToTrade = this.HasTradeItem(requiredItem, requiredItemCount);
				if (this.canPurchaseCheck != null && !this.canPurchaseCheck(this.currentItemIndex + i))
				{
					hasEnoughToTrade = false;
				}
				float textWidth = SpriteText.getWidthOfString("x" + requiredItemCount);
				ParsedItemData dataOrErrorItem = ItemRegistry.GetDataOrErrorItem(requiredItem);
				Texture2D texture = dataOrErrorItem.GetTexture();
				Rectangle sourceRect = dataOrErrorItem.GetSourceRect();
				Utility.drawWithShadow(b, texture, new Vector2((float)(right - 88) - textWidth, tradeIconDrawY), sourceRect, Color.White * (hasEnoughToTrade ? 1f : 0.25f), 0f, Vector2.Zero, -1f, flipped: false, -1f, -1, -1, hasEnoughToTrade ? 0.35f : 0f);
				SpriteText.drawString(b, "x" + requiredItemCount, right - (int)textWidth - 16, tradeTextDrawY, 999999, -1, 999999, hasEnoughToTrade ? 1f : 0.5f, 0.88f, junimoText: false, -1, "", theme.ItemRowTextColor);
			}
		}
		if (this.IsOutOfStock())
		{
			string text = Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11583");
			SpriteText.drawString(b, text, base.xPositionOnScreen + base.width / 2 - SpriteText.getWidthOfString(text) / 2, base.yPositionOnScreen + base.height / 2 - 128, 999999, -1, 999999, 1f, 0.88f, junimoText: false, -1, "", theme?.ItemRowTextColor);
		}
		this.inventory.draw(b);
		for (int i2 = this.animations.Count - 1; i2 >= 0; i2--)
		{
			if (this.animations[i2].update(Game1.currentGameTime))
			{
				this.animations.RemoveAt(i2);
			}
			else
			{
				this.animations[i2].draw(b, localPosition: true);
			}
		}
		this.poof?.draw(b);
		this.upArrow.draw(b);
		this.downArrow.draw(b);
		foreach (ShopTabClickableTextureComponent tabButton in this.tabButtons)
		{
			tabButton.draw(b);
		}
		if (this.forSale.Count > 4)
		{
			IClickableMenu.drawTextureBox(b, theme.ScrollBarBackTexture, theme.ScrollBarBackSourceRect, this.scrollBarRunner.X, this.scrollBarRunner.Y, this.scrollBarRunner.Width, this.scrollBarRunner.Height, Color.White, 4f);
			this.scrollBar.draw(b);
		}
		if (this.hoverText != "")
		{
			Item actualItem = this.hoveredItem as Item;
			ISalable salable = this.hoveredItem;
			if (salable != null && salable.IsRecipe)
			{
				IClickableMenu.drawToolTip(b, " ", this.boldTitleText, actualItem, this.heldItem != null, -1, this.currency, this.getHoveredItemExtraItemIndex(), this.getHoveredItemExtraItemAmount(), new CraftingRecipe(actualItem?.BaseName ?? this.hoveredItem.Name), (this.hoverPrice > 0) ? this.hoverPrice : (-1));
			}
			else
			{
				IClickableMenu.drawToolTip(b, this.hoverText, this.boldTitleText, actualItem, this.heldItem != null, -1, this.currency, this.getHoveredItemExtraItemIndex(), this.getHoveredItemExtraItemAmount(), null, (this.hoverPrice > 0) ? this.hoverPrice : (-1));
			}
		}
		this.heldItem?.drawInMenu(b, new Vector2(Game1.getOldMouseX() + 8, Game1.getOldMouseY() + 8), 1f, 1f, 0.9f, StackDrawType.Draw, Color.White, drawShadow: true);
		base.draw(b);
		int portrait_draw_position = base.xPositionOnScreen - 320;
		if (portrait_draw_position > 0 && Game1.options.showMerchantPortraits)
		{
			if (this.portraitTexture != null)
			{
				Utility.drawWithShadow(b, theme.PortraitBackgroundTexture, new Vector2(portrait_draw_position, base.yPositionOnScreen), theme.PortraitBackgroundSourceRect, Color.White, 0f, Vector2.Zero, 4f, flipped: false, 0.91f);
				if (this.portraitTexture != null)
				{
					b.Draw(this.portraitTexture, new Vector2(portrait_draw_position + 20, base.yPositionOnScreen + 20), new Rectangle(0, 0, 64, 64), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.92f);
				}
			}
			if (this.potraitPersonDialogue != null)
			{
				portrait_draw_position = base.xPositionOnScreen - (int)Game1.dialogueFont.MeasureString(this.potraitPersonDialogue).X - 64;
				if (portrait_draw_position > 0)
				{
					IClickableMenu.drawHoverText(b, this.potraitPersonDialogue, Game1.dialogueFont, 0, 0, -1, null, -1, null, null, 0, null, -1, portrait_draw_position, base.yPositionOnScreen + ((this.portraitTexture != null) ? 312 : 0), 1f, null, null, theme.DialogueBackgroundTexture, theme.DialogueBackgroundSourceRect, theme.DialogueColor, theme.DialogueShadowColor);
				}
			}
		}
		base.drawMouse(b);
	}

	/// <summary>Get how the stack size for a shop entry should be drawn.</summary>
	/// <param name="stockInfo">The shop entry's stock information.</param>
	/// <param name="item">The spawned item instance.</param>
	public StackDrawType GetStackDrawType(ItemStockInformation stockInfo, ISalable item)
	{
		if (item.IsRecipe)
		{
			return StackDrawType.Hide;
		}
		if (stockInfo.StackDrawType.HasValue)
		{
			return stockInfo.StackDrawType.Value;
		}
		if (stockInfo.Stock == int.MaxValue)
		{
			return StackDrawType.HideButShowQuality;
		}
		if (this.DefaultStackDrawType.HasValue)
		{
			return this.DefaultStackDrawType.Value;
		}
		ShopData shopData = this.ShopData;
		if (shopData != null && shopData.StackSizeVisibility.HasValue)
		{
			return this.ShopData.StackSizeVisibility switch
			{
				StackSizeVisibility.Hide => StackDrawType.HideButShowQuality, 
				StackSizeVisibility.ShowIfMultiple => StackDrawType.Draw, 
				_ => StackDrawType.Draw_OneInclusive, 
			};
		}
		if (!this._isStorageShop)
		{
			return StackDrawType.Draw_OneInclusive;
		}
		return StackDrawType.Draw;
	}
}
