using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.Buildings;
using StardewValley.Objects;

namespace StardewValley.Menus;

public class ItemGrabMenu : MenuWithInventory
{
	public delegate void behaviorOnItemSelect(Item item, Farmer who);

	public class TransferredItemSprite
	{
		public Item item;

		public Vector2 position;

		public float age;

		public float alpha = 1f;

		public TransferredItemSprite(Item transferred_item, int start_x, int start_y)
		{
			this.item = transferred_item;
			this.position.X = start_x;
			this.position.Y = start_y;
		}

		public bool Update(GameTime time)
		{
			float life_time = 0.15f;
			this.position.Y -= (float)time.ElapsedGameTime.TotalSeconds * 128f;
			this.age += (float)time.ElapsedGameTime.TotalSeconds;
			this.alpha = 1f - this.age / life_time;
			if (this.age >= life_time)
			{
				return true;
			}
			return false;
		}

		public void Draw(SpriteBatch b)
		{
			this.item.drawInMenu(b, this.position, 1f, this.alpha, 0.9f, StackDrawType.Hide, Color.White, drawShadow: false);
		}
	}

	public const int region_organizationButtons = 15923;

	public const int region_itemsToGrabMenuModifier = 53910;

	public const int region_fillStacksButton = 12952;

	public const int region_organizeButton = 106;

	public const int region_colorPickToggle = 27346;

	public const int region_specialButton = 12485;

	public const int region_lastShippedHolder = 12598;

	/// <summary>The <see cref="F:StardewValley.Menus.ItemGrabMenu.source" /> value when a specific value doesn't apply.</summary>
	public const int source_none = 0;

	/// <summary>The <see cref="F:StardewValley.Menus.ItemGrabMenu.source" /> value when collecting items from a chest.</summary>
	public const int source_chest = 1;

	/// <summary>The <see cref="F:StardewValley.Menus.ItemGrabMenu.source" /> value when collecting items which couldn't be added directly to the player's inventory (e.g. from NPC dialogue).</summary>
	public const int source_gift = 2;

	/// <summary>The <see cref="F:StardewValley.Menus.ItemGrabMenu.source" /> value when collecting treasure found while fishing.</summary>
	public const int source_fishingChest = 3;

	/// <summary>The <see cref="F:StardewValley.Menus.ItemGrabMenu.source" /> value when collecting items which couldn't be added directly to the player's inventory via <see cref="M:StardewValley.Farmer.addItemByMenuIfNecessary(StardewValley.Item,StardewValley.Menus.ItemGrabMenu.behaviorOnItemSelect,System.Boolean)" />.</summary>
	public const int source_overflow = 4;

	public const int specialButton_junimotoggle = 1;

	/// <summary>The inventory from which the player can collect items.</summary>
	public InventoryMenu ItemsToGrabMenu;

	public TemporaryAnimatedSprite poof;

	public bool reverseGrab;

	public bool showReceivingMenu = true;

	public bool drawBG = true;

	public bool destroyItemOnClick;

	public bool canExitOnKey;

	public bool playRightClickSound;

	public bool allowRightClick;

	public bool shippingBin;

	public string message;

	/// <summary>The callback invoked when taking something out of the player inventory (e.g. putting something in the Luau soup), if any.</summary>
	public behaviorOnItemSelect behaviorFunction;

	/// <summary>The callback invoked when taking something from the menu (e.g. to put in the player's inventory), if any.</summary>
	public behaviorOnItemSelect behaviorOnItemGrab;

	/// <summary>The item for which the item menu was opened (e.g. the chest or storage furniture item being checked), if applicable.</summary>
	public Item sourceItem;

	public ClickableTextureComponent fillStacksButton;

	public ClickableTextureComponent organizeButton;

	public ClickableTextureComponent colorPickerToggleButton;

	public ClickableTextureComponent specialButton;

	public ClickableTextureComponent lastShippedHolder;

	public List<ClickableComponent> discreteColorPickerCC;

	/// <summary>The reason this menu was opened, usually matching a constant like <see cref="F:StardewValley.Menus.ItemGrabMenu.source_chest" />.</summary>
	public int source;

	public int whichSpecialButton;

	/// <summary>A contextual value for what opened the menu. This may be a chest, event, fishing rod, location, etc.</summary>
	public object context;

	public bool snappedtoBottom;

	public DiscreteColorPicker chestColorPicker;

	public bool essential;

	public bool superEssential;

	public int storageSpaceTopBorderOffset;

	/// <summary>Whether <see cref="M:StardewValley.Menus.ItemGrabMenu.update(Microsoft.Xna.Framework.GameTime)" /> has run at least once yet.</summary>
	private bool HasUpdateTicked;

	public List<TransferredItemSprite> _transferredItemSprites = new List<TransferredItemSprite>();

	/// <summary>Whether the source item was placed in the current location when the menu is opened.</summary>
	public bool _sourceItemInCurrentLocation;

	public ClickableTextureComponent junimoNoteIcon;

	public int junimoNotePulser;

	/// <summary>Construct an instance.</summary>
	/// <param name="inventory">The items that can be collected by the player.</param>
	/// <param name="context">A contextual value for what opened the menu. This may be a chest, event, fishing rod, location, etc.</param>
	public ItemGrabMenu(IList<Item> inventory, object context = null)
		: base(null, okButton: true, trashCan: true)
	{
		this.context = context;
		this.ItemsToGrabMenu = new InventoryMenu(base.xPositionOnScreen + 32, base.yPositionOnScreen, playerInventory: false, inventory);
		base.trashCan.myID = 106;
		this.ItemsToGrabMenu.populateClickableComponentList();
		for (int i = 0; i < this.ItemsToGrabMenu.inventory.Count; i++)
		{
			if (this.ItemsToGrabMenu.inventory[i] != null)
			{
				this.ItemsToGrabMenu.inventory[i].myID += 53910;
				this.ItemsToGrabMenu.inventory[i].upNeighborID += 53910;
				this.ItemsToGrabMenu.inventory[i].rightNeighborID += 53910;
				this.ItemsToGrabMenu.inventory[i].downNeighborID = -7777;
				this.ItemsToGrabMenu.inventory[i].leftNeighborID += 53910;
				this.ItemsToGrabMenu.inventory[i].fullyImmutable = true;
				if (i % (this.ItemsToGrabMenu.capacity / this.ItemsToGrabMenu.rows) == 0)
				{
					this.ItemsToGrabMenu.inventory[i].leftNeighborID = base.dropItemInvisibleButton.myID;
				}
				if (i % (this.ItemsToGrabMenu.capacity / this.ItemsToGrabMenu.rows) == this.ItemsToGrabMenu.capacity / this.ItemsToGrabMenu.rows - 1)
				{
					this.ItemsToGrabMenu.inventory[i].rightNeighborID = base.trashCan.myID;
				}
			}
		}
		for (int j = 0; j < this.GetColumnCount(); j++)
		{
			if (base.inventory?.inventory?.Count >= this.GetColumnCount())
			{
				base.inventory.inventory[j].upNeighborID = (this.shippingBin ? 12598 : (-7777));
			}
		}
		if (!this.shippingBin)
		{
			for (int k = 0; k < this.GetColumnCount() * 3; k++)
			{
				InventoryMenu inventoryMenu = base.inventory;
				if (inventoryMenu != null && inventoryMenu.inventory?.Count > k)
				{
					base.inventory.inventory[k].upNeighborID = -7777;
					base.inventory.inventory[k].upNeighborImmutable = true;
				}
			}
		}
		if (base.trashCan != null)
		{
			base.trashCan.leftNeighborID = 11;
		}
		if (base.okButton != null)
		{
			base.okButton.leftNeighborID = 11;
		}
		this.populateClickableComponentList();
		if (Game1.options.SnappyMenus)
		{
			this.snapToDefaultClickableComponent();
		}
		base.inventory.showGrayedOutSlots = true;
		this.SetupBorderNeighbors();
	}

	/// <summary>Drop any remaining items that weren't grabbed by the player onto the ground at their feet.</summary>
	public virtual void DropRemainingItems()
	{
		if (this.ItemsToGrabMenu?.actualInventory == null)
		{
			return;
		}
		foreach (Item item in this.ItemsToGrabMenu.actualInventory)
		{
			if (item != null)
			{
				Game1.createItemDebris(item, Game1.player.getStandingPosition(), Game1.player.FacingDirection);
			}
		}
		this.ItemsToGrabMenu.actualInventory.Clear();
	}

	/// <summary>Construct an instance.</summary>
	/// <param name="menu">The menu whose state to copy.</param>
	public ItemGrabMenu(ItemGrabMenu menu)
		: this(menu.ItemsToGrabMenu.actualInventory, menu.reverseGrab, menu.showReceivingMenu, menu.inventory.highlightMethod, menu.behaviorFunction, menu.message, menu.behaviorOnItemGrab, snapToBottom: false, menu.canExitOnKey, menu.playRightClickSound, menu.allowRightClick, menu.organizeButton != null, menu.source, menu.sourceItem, menu.whichSpecialButton, menu.context, menu.HeldItemExitBehavior, menu.AllowExitWithHeldItem)
	{
		this.setEssential(menu.essential);
		if (menu.currentlySnappedComponent != null)
		{
			this.setCurrentlySnappedComponentTo(menu.currentlySnappedComponent.myID);
			if (Game1.options.SnappyMenus)
			{
				this.snapCursorToCurrentSnappedComponent();
			}
		}
		base.heldItem = menu.heldItem;
	}

	public ItemGrabMenu(IList<Item> inventory, bool reverseGrab, bool showReceivingMenu, InventoryMenu.highlightThisItem highlightFunction, behaviorOnItemSelect behaviorOnItemSelectFunction, string message, behaviorOnItemSelect behaviorOnItemGrab = null, bool snapToBottom = false, bool canBeExitedWithKey = false, bool playRightClickSound = true, bool allowRightClick = true, bool showOrganizeButton = false, int source = 0, Item sourceItem = null, int whichSpecialButton = -1, object context = null, ItemExitBehavior heldItemExitBehavior = ItemExitBehavior.ReturnToPlayer, bool allowExitWithHeldItem = false)
		: base(highlightFunction, okButton: true, trashCan: true, 0, 0, 64, heldItemExitBehavior, allowExitWithHeldItem)
	{
		this.source = source;
		this.message = message;
		this.reverseGrab = reverseGrab;
		this.showReceivingMenu = showReceivingMenu;
		this.playRightClickSound = playRightClickSound;
		this.allowRightClick = allowRightClick;
		base.inventory.showGrayedOutSlots = true;
		this.sourceItem = sourceItem;
		this.whichSpecialButton = whichSpecialButton;
		this.context = context;
		if (sourceItem != null && Game1.currentLocation.objects.Values.Contains(sourceItem))
		{
			this._sourceItemInCurrentLocation = true;
		}
		else
		{
			this._sourceItemInCurrentLocation = false;
		}
		if (sourceItem is Chest sourceChest)
		{
			if (this.CanHaveColorPicker())
			{
				Chest itemToDrawColored = new Chest(playerChest: true, sourceItem.ItemId);
				this.chestColorPicker = new DiscreteColorPicker(base.xPositionOnScreen, base.yPositionOnScreen - 64 - IClickableMenu.borderWidth * 2, sourceChest.playerChoiceColor.Value, itemToDrawColored);
				itemToDrawColored.playerChoiceColor.Value = DiscreteColorPicker.getColorFromSelection(this.chestColorPicker.colorSelection);
				this.colorPickerToggleButton = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen + base.width, base.yPositionOnScreen + base.height / 3 - 64 + -160, 64, 64), Game1.mouseCursors, new Rectangle(119, 469, 16, 16), 4f)
				{
					hoverText = Game1.content.LoadString("Strings\\UI:Toggle_ColorPicker"),
					myID = 27346,
					downNeighborID = -99998,
					leftNeighborID = 53921,
					region = 15923
				};
			}
			if (source == 1 && (sourceChest.SpecialChestType == Chest.SpecialChestTypes.None || sourceChest.SpecialChestType == Chest.SpecialChestTypes.BigChest) && InventoryPage.ShouldShowJunimoNoteIcon())
			{
				this.junimoNoteIcon = new ClickableTextureComponent("", new Rectangle(base.xPositionOnScreen + base.width, base.yPositionOnScreen + base.height / 3 - 64 + -216, 64, 64), "", Game1.content.LoadString("Strings\\UI:GameMenu_JunimoNote_Hover"), Game1.mouseCursors, new Rectangle(331, 374, 15, 14), 4f)
				{
					myID = 898,
					leftNeighborID = 11,
					downNeighborID = 106
				};
			}
		}
		if (whichSpecialButton == 1)
		{
			this.specialButton = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen + base.width, base.yPositionOnScreen + base.height / 3 - 64 + -160, 64, 64), Game1.mouseCursors, new Rectangle(108, 491, 16, 16), 4f)
			{
				myID = 12485,
				downNeighborID = (showOrganizeButton ? 12952 : 5948),
				region = 15923,
				leftNeighborID = 53921
			};
			if (context is JunimoHut hut)
			{
				this.specialButton.sourceRect.X = (hut.noHarvest.Value ? 124 : 108);
			}
		}
		if (snapToBottom)
		{
			base.movePosition(0, Game1.uiViewport.Height - (base.yPositionOnScreen + base.height - IClickableMenu.spaceToClearTopBorder));
			this.snappedtoBottom = true;
		}
		if (source == 1 && sourceItem is Chest chest && chest.GetActualCapacity() != 36)
		{
			int capacity = chest.GetActualCapacity();
			int rows = ((capacity >= 70) ? 5 : 3);
			if (capacity < 9)
			{
				rows = 1;
			}
			int containerWidth = 64 * (capacity / rows);
			this.ItemsToGrabMenu = new InventoryMenu(Game1.uiViewport.Width / 2 - containerWidth / 2, base.yPositionOnScreen + ((capacity < 70) ? 64 : (-21)), playerInventory: false, inventory, highlightFunction, capacity, rows);
			if (chest.SpecialChestType == Chest.SpecialChestTypes.MiniShippingBin)
			{
				base.inventory.moveItemSound = "Ship";
			}
			if (rows > 3)
			{
				base.yPositionOnScreen += 42;
				base.inventory.SetPosition(base.inventory.xPositionOnScreen, base.inventory.yPositionOnScreen + 38 + 4);
				this.ItemsToGrabMenu.SetPosition(this.ItemsToGrabMenu.xPositionOnScreen - 32 + 8, this.ItemsToGrabMenu.yPositionOnScreen);
				this.storageSpaceTopBorderOffset = 20;
				base.trashCan.bounds.X = this.ItemsToGrabMenu.width + this.ItemsToGrabMenu.xPositionOnScreen + IClickableMenu.borderWidth * 2;
				base.okButton.bounds.X = this.ItemsToGrabMenu.width + this.ItemsToGrabMenu.xPositionOnScreen + IClickableMenu.borderWidth * 2;
			}
		}
		else
		{
			this.ItemsToGrabMenu = new InventoryMenu(base.xPositionOnScreen + 32, base.yPositionOnScreen, playerInventory: false, inventory, highlightFunction);
		}
		this.ItemsToGrabMenu.populateClickableComponentList();
		for (int i = 0; i < this.ItemsToGrabMenu.inventory.Count; i++)
		{
			if (this.ItemsToGrabMenu.inventory[i] != null)
			{
				this.ItemsToGrabMenu.inventory[i].myID += 53910;
				this.ItemsToGrabMenu.inventory[i].upNeighborID += 53910;
				this.ItemsToGrabMenu.inventory[i].rightNeighborID += 53910;
				this.ItemsToGrabMenu.inventory[i].downNeighborID = -7777;
				this.ItemsToGrabMenu.inventory[i].leftNeighborID += 53910;
				this.ItemsToGrabMenu.inventory[i].fullyImmutable = true;
			}
		}
		this.behaviorFunction = behaviorOnItemSelectFunction;
		this.behaviorOnItemGrab = behaviorOnItemGrab;
		this.canExitOnKey = canBeExitedWithKey;
		if (showOrganizeButton)
		{
			this.fillStacksButton = new ClickableTextureComponent("", new Rectangle(base.xPositionOnScreen + base.width, base.yPositionOnScreen + base.height / 3 - 64 - 64 - 16, 64, 64), "", Game1.content.LoadString("Strings\\UI:ItemGrab_FillStacks"), Game1.mouseCursors, new Rectangle(103, 469, 16, 16), 4f)
			{
				myID = 12952,
				upNeighborID = ((this.colorPickerToggleButton != null) ? 27346 : ((this.specialButton != null) ? 12485 : (-500))),
				downNeighborID = 106,
				leftNeighborID = 53921,
				region = 15923
			};
			this.organizeButton = new ClickableTextureComponent("", new Rectangle(base.xPositionOnScreen + base.width, base.yPositionOnScreen + base.height / 3 - 64, 64, 64), "", Game1.content.LoadString("Strings\\UI:ItemGrab_Organize"), Game1.mouseCursors, new Rectangle(162, 440, 16, 16), 4f)
			{
				myID = 106,
				upNeighborID = 12952,
				downNeighborID = 5948,
				leftNeighborID = 53921,
				region = 15923
			};
		}
		this.RepositionSideButtons();
		if (this.chestColorPicker != null)
		{
			this.discreteColorPickerCC = new List<ClickableComponent>();
			for (int j = 0; j < DiscreteColorPicker.totalColors; j++)
			{
				List<ClickableComponent> list = this.discreteColorPickerCC;
				ClickableComponent obj = new ClickableComponent(new Rectangle(this.chestColorPicker.xPositionOnScreen + IClickableMenu.borderWidth / 2 + j * 9 * 4, this.chestColorPicker.yPositionOnScreen + IClickableMenu.borderWidth / 2, 36, 28), "")
				{
					myID = j + 4343,
					rightNeighborID = ((j < DiscreteColorPicker.totalColors - 1) ? (j + 4343 + 1) : (-1)),
					leftNeighborID = ((j > 0) ? (j + 4343 - 1) : (-1))
				};
				InventoryMenu itemsToGrabMenu = this.ItemsToGrabMenu;
				obj.downNeighborID = ((itemsToGrabMenu != null && itemsToGrabMenu.inventory.Count > 0) ? 53910 : 0);
				list.Add(obj);
			}
		}
		if (this.organizeButton != null)
		{
			foreach (ClickableComponent item in this.ItemsToGrabMenu.GetBorder(InventoryMenu.BorderSide.Right))
			{
				item.rightNeighborID = this.organizeButton.myID;
			}
		}
		if (base.trashCan != null && base.inventory.inventory.Count >= 12 && base.inventory.inventory[11] != null)
		{
			base.inventory.inventory[11].rightNeighborID = 5948;
		}
		if (base.trashCan != null)
		{
			base.trashCan.leftNeighborID = 11;
		}
		if (base.okButton != null)
		{
			base.okButton.leftNeighborID = 11;
		}
		ClickableComponent top_right = this.ItemsToGrabMenu.GetBorder(InventoryMenu.BorderSide.Right).FirstOrDefault();
		if (top_right != null)
		{
			if (this.organizeButton != null)
			{
				this.organizeButton.leftNeighborID = top_right.myID;
			}
			if (this.specialButton != null)
			{
				this.specialButton.leftNeighborID = top_right.myID;
			}
			if (this.fillStacksButton != null)
			{
				this.fillStacksButton.leftNeighborID = top_right.myID;
			}
			if (this.junimoNoteIcon != null)
			{
				this.junimoNoteIcon.leftNeighborID = top_right.myID;
			}
		}
		this.populateClickableComponentList();
		if (Game1.options.SnappyMenus)
		{
			this.snapToDefaultClickableComponent();
		}
		this.SetupBorderNeighbors();
	}

	/// <summary>Create an item grab menu to collect items which couldn't be added to the player's inventory directly.</summary>
	/// <param name="items">The items to collect.</param>
	/// <param name="onCollectItem">The callback to invoke when an item is retrieved.</param>
	public static ItemGrabMenu CreateOverflowMenu(IList<Item> items, behaviorOnItemSelect onCollectItem = null)
	{
		ItemGrabMenu itemGrabMenu = new ItemGrabMenu(items).setEssential(essential: true);
		itemGrabMenu.inventory.showGrayedOutSlots = true;
		itemGrabMenu.inventory.onAddItem = onCollectItem;
		itemGrabMenu.source = 4;
		return itemGrabMenu;
	}

	/// <summary>Position the buttons that appear on the right side of the screen (e.g. to organize or fill stacks), and update their neighbor IDs.</summary>
	public virtual void RepositionSideButtons()
	{
		List<ClickableComponent> side_buttons = new List<ClickableComponent>();
		int slotsPerRow = this.ItemsToGrabMenu.capacity / this.ItemsToGrabMenu.rows;
		if (this.organizeButton != null)
		{
			this.organizeButton.leftNeighborID = slotsPerRow - 1 + 53910;
			side_buttons.Add(this.organizeButton);
		}
		if (this.fillStacksButton != null)
		{
			this.fillStacksButton.leftNeighborID = slotsPerRow - 1 + 53910;
			side_buttons.Add(this.fillStacksButton);
		}
		if (this.colorPickerToggleButton != null)
		{
			this.colorPickerToggleButton.leftNeighborID = slotsPerRow - 1 + 53910;
			side_buttons.Add(this.colorPickerToggleButton);
		}
		if (this.specialButton != null)
		{
			side_buttons.Add(this.specialButton);
		}
		if (this.junimoNoteIcon != null)
		{
			this.junimoNoteIcon.leftNeighborID = slotsPerRow - 1;
			side_buttons.Add(this.junimoNoteIcon);
		}
		int step_size = 80;
		if (side_buttons.Count >= 4)
		{
			step_size = 72;
		}
		for (int i = 0; i < side_buttons.Count; i++)
		{
			ClickableComponent button = side_buttons[i];
			if (i > 0 && side_buttons.Count > 1)
			{
				button.downNeighborID = side_buttons[i - 1].myID;
			}
			if (i < side_buttons.Count - 1 && side_buttons.Count > 1)
			{
				button.upNeighborID = side_buttons[i + 1].myID;
			}
			button.bounds.X = this.ItemsToGrabMenu.xPositionOnScreen + this.ItemsToGrabMenu.width + IClickableMenu.borderWidth * 2;
			button.bounds.Y = this.ItemsToGrabMenu.yPositionOnScreen + base.height / 3 - 64 - step_size * i;
		}
	}

	public void SetupBorderNeighbors()
	{
		List<ClickableComponent> border = base.inventory.GetBorder(InventoryMenu.BorderSide.Right);
		foreach (ClickableComponent item in border)
		{
			item.rightNeighborID = -99998;
			item.rightNeighborImmutable = true;
		}
		border = this.ItemsToGrabMenu.GetBorder(InventoryMenu.BorderSide.Right);
		bool has_organizational_buttons = false;
		foreach (ClickableComponent allClickableComponent in base.allClickableComponents)
		{
			if (allClickableComponent.region == 15923)
			{
				has_organizational_buttons = true;
				break;
			}
		}
		foreach (ClickableComponent slot in border)
		{
			if (has_organizational_buttons)
			{
				slot.rightNeighborID = -99998;
				slot.rightNeighborImmutable = true;
			}
			else
			{
				slot.rightNeighborID = -1;
			}
		}
		for (int i = 0; i < this.GetColumnCount(); i++)
		{
			InventoryMenu inventoryMenu = base.inventory;
			ClickableComponent clickableComponent;
			int upNeighborID;
			if (inventoryMenu != null && inventoryMenu.inventory?.Count >= 12)
			{
				clickableComponent = base.inventory.inventory[i];
				if (!this.shippingBin)
				{
					if (this.discreteColorPickerCC != null)
					{
						InventoryMenu itemsToGrabMenu = this.ItemsToGrabMenu;
						if (itemsToGrabMenu != null && itemsToGrabMenu.inventory.Count <= i && Game1.player.showChestColorPicker)
						{
							upNeighborID = 4343;
							goto IL_01b0;
						}
					}
					upNeighborID = ((this.ItemsToGrabMenu.inventory.Count > i) ? (53910 + i) : 53910);
				}
				else
				{
					upNeighborID = 12598;
				}
				goto IL_01b0;
			}
			goto IL_01b5;
			IL_01b5:
			if (this.discreteColorPickerCC != null)
			{
				InventoryMenu itemsToGrabMenu2 = this.ItemsToGrabMenu;
				if (itemsToGrabMenu2 != null && itemsToGrabMenu2.inventory.Count > i && Game1.player.showChestColorPicker)
				{
					this.ItemsToGrabMenu.inventory[i].upNeighborID = 4343;
					continue;
				}
			}
			this.ItemsToGrabMenu.inventory[i].upNeighborID = -1;
			continue;
			IL_01b0:
			clickableComponent.upNeighborID = upNeighborID;
			goto IL_01b5;
		}
		if (this.shippingBin)
		{
			return;
		}
		for (int j = 0; j < 36; j++)
		{
			InventoryMenu inventoryMenu2 = base.inventory;
			if (inventoryMenu2 != null && inventoryMenu2.inventory?.Count > j)
			{
				base.inventory.inventory[j].upNeighborID = -7777;
				base.inventory.inventory[j].upNeighborImmutable = true;
			}
		}
	}

	/// <summary>Get whether the chest can display a color picker (regardless of whether it's currently shown).</summary>
	public virtual bool CanHaveColorPicker()
	{
		if (this.source == 1 && this.sourceItem is Chest chest && (chest.SpecialChestType == Chest.SpecialChestTypes.None || chest.SpecialChestType == Chest.SpecialChestTypes.BigChest))
		{
			return !chest.fridge.Value;
		}
		return false;
	}

	public virtual int GetColumnCount()
	{
		return this.ItemsToGrabMenu.capacity / this.ItemsToGrabMenu.rows;
	}

	/// <summary>Set whether to rescue items from the menu when it's force-closed (e.g. from passing out at 2am). Rescued items will be added to the player's inventory if possible, else dropped onto the ground at their feet.</summary>
	/// <param name="essential">Whether to rescue items on force-close.</param>
	/// <param name="superEssential">Whether to rescue items on normal close.</param>
	public ItemGrabMenu setEssential(bool essential, bool superEssential = false)
	{
		this.essential = essential || superEssential;
		this.superEssential = superEssential;
		return this;
	}

	public void initializeShippingBin()
	{
		this.shippingBin = true;
		this.lastShippedHolder = new ClickableTextureComponent("", new Rectangle(base.xPositionOnScreen + base.width / 2 - 48, base.yPositionOnScreen + base.height / 2 - 80 - 64, 96, 96), "", Game1.content.LoadString("Strings\\UI:ShippingBin_LastItem"), Game1.mouseCursors, new Rectangle(293, 360, 24, 24), 4f)
		{
			myID = 12598,
			region = 12598
		};
		for (int i = 0; i < this.GetColumnCount(); i++)
		{
			if (base.inventory?.inventory?.Count >= this.GetColumnCount())
			{
				base.inventory.inventory[i].upNeighborID = -7777;
				if (i == 11)
				{
					base.inventory.inventory[i].rightNeighborID = 5948;
				}
			}
		}
		this.populateClickableComponentList();
		if (Game1.options.SnappyMenus)
		{
			this.snapToDefaultClickableComponent();
		}
	}

	protected override void customSnapBehavior(int direction, int oldRegion, int oldID)
	{
		switch (direction)
		{
		case 2:
		{
			for (int j = 0; j < 12; j++)
			{
				if (base.inventory?.inventory?.Count >= this.GetColumnCount() && this.shippingBin)
				{
					base.inventory.inventory[j].upNeighborID = (this.shippingBin ? 12598 : (Math.Min(j, this.ItemsToGrabMenu.inventory.Count - 1) + 53910));
				}
			}
			if (!this.shippingBin && oldID >= 53910)
			{
				int index = oldID - 53910;
				if (index + this.GetColumnCount() <= this.ItemsToGrabMenu.inventory.Count - 1)
				{
					base.currentlySnappedComponent = base.getComponentWithID(index + this.GetColumnCount() + 53910);
					this.snapCursorToCurrentSnappedComponent();
					break;
				}
			}
			if (base.inventory != null)
			{
				int inventoryRowLength2 = base.inventory.capacity / base.inventory.rows;
				int diff2 = this.GetColumnCount() - inventoryRowLength2;
				base.currentlySnappedComponent = base.getComponentWithID((oldRegion != 12598) ? Math.Max(0, Math.Min((oldID - 53910) % this.GetColumnCount() - diff2 / 2, base.inventory.capacity / base.inventory.rows - diff2 / 2)) : 0);
			}
			else
			{
				base.currentlySnappedComponent = base.getComponentWithID((oldRegion != 12598) ? ((oldID - 53910) % this.GetColumnCount()) : 0);
			}
			this.snapCursorToCurrentSnappedComponent();
			break;
		}
		case 0:
		{
			if (this.shippingBin && Game1.getFarm().lastItemShipped != null && oldID < 12)
			{
				base.currentlySnappedComponent = base.getComponentWithID(12598);
				base.currentlySnappedComponent.downNeighborID = oldID;
				this.snapCursorToCurrentSnappedComponent();
				break;
			}
			if (oldID < 53910 && oldID >= 12)
			{
				base.currentlySnappedComponent = base.getComponentWithID(oldID - 12);
				break;
			}
			int id = oldID + this.GetColumnCount() * (this.ItemsToGrabMenu.rows - 1);
			for (int i = 0; i < 3; i++)
			{
				if (this.ItemsToGrabMenu.inventory.Count > id)
				{
					break;
				}
				id -= this.GetColumnCount();
			}
			if (this.showReceivingMenu)
			{
				if (id < 0)
				{
					if (this.ItemsToGrabMenu.inventory.Count > 0)
					{
						base.currentlySnappedComponent = base.getComponentWithID(53910 + this.ItemsToGrabMenu.inventory.Count - 1);
					}
					else if (this.discreteColorPickerCC != null)
					{
						base.currentlySnappedComponent = base.getComponentWithID(4343);
					}
				}
				else
				{
					int inventoryRowLength = base.inventory.capacity / base.inventory.rows;
					int diff = this.GetColumnCount() - inventoryRowLength;
					base.currentlySnappedComponent = base.getComponentWithID(id + 53910 + diff / 2);
					if (base.currentlySnappedComponent == null)
					{
						base.currentlySnappedComponent = base.getComponentWithID(53910);
					}
				}
			}
			this.snapCursorToCurrentSnappedComponent();
			break;
		}
		}
	}

	public override void snapToDefaultClickableComponent()
	{
		if (this.shippingBin)
		{
			base.currentlySnappedComponent = base.getComponentWithID(0);
		}
		else if (this.source == 1 && this.sourceItem is Chest { SpecialChestType: Chest.SpecialChestTypes.MiniShippingBin })
		{
			base.currentlySnappedComponent = base.getComponentWithID(0);
		}
		else
		{
			base.currentlySnappedComponent = base.getComponentWithID((this.ItemsToGrabMenu.inventory.Count > 0 && this.showReceivingMenu) ? 53910 : 0);
		}
		this.snapCursorToCurrentSnappedComponent();
	}

	public void setSourceItem(Item item)
	{
		this.sourceItem = item;
		this.chestColorPicker = null;
		this.colorPickerToggleButton = null;
		if (this.CanHaveColorPicker() && this.sourceItem is Chest chest)
		{
			Chest itemToDrawColored = new Chest(playerChest: true, this.sourceItem.ItemId);
			this.chestColorPicker = new DiscreteColorPicker(base.xPositionOnScreen, base.yPositionOnScreen - 64 - IClickableMenu.borderWidth * 2, chest.playerChoiceColor.Value, itemToDrawColored);
			if (chest.SpecialChestType == Chest.SpecialChestTypes.BigChest)
			{
				this.chestColorPicker.yPositionOnScreen -= 42;
			}
			itemToDrawColored.playerChoiceColor.Value = DiscreteColorPicker.getColorFromSelection(this.chestColorPicker.colorSelection);
			this.colorPickerToggleButton = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen + base.width, base.yPositionOnScreen + base.height / 3 - 64 + -160, 64, 64), Game1.mouseCursors, new Rectangle(119, 469, 16, 16), 4f)
			{
				hoverText = Game1.content.LoadString("Strings\\UI:Toggle_ColorPicker")
			};
		}
		this.RepositionSideButtons();
	}

	public override bool IsAutomaticSnapValid(int direction, ClickableComponent a, ClickableComponent b)
	{
		if (direction == 1 && this.ItemsToGrabMenu.inventory.Contains(a) && base.inventory.inventory.Contains(b))
		{
			return false;
		}
		return base.IsAutomaticSnapValid(direction, a, b);
	}

	public void setBackgroundTransparency(bool b)
	{
		this.drawBG = b;
	}

	public void setDestroyItemOnClick(bool b)
	{
		this.destroyItemOnClick = b;
	}

	/// <inheritdoc />
	public override void receiveRightClick(int x, int y, bool playSound = true)
	{
		if (!this.allowRightClick)
		{
			base.receiveRightClickOnlyToolAttachments(x, y);
			return;
		}
		base.receiveRightClick(x, y, playSound && this.playRightClickSound);
		if (base.heldItem == null && this.showReceivingMenu)
		{
			base.heldItem = this.ItemsToGrabMenu.rightClick(x, y, base.heldItem, playSound: false);
			if (base.heldItem != null && this.behaviorOnItemGrab != null)
			{
				this.behaviorOnItemGrab(base.heldItem, Game1.player);
				if (Game1.activeClickableMenu is ItemGrabMenu itemGrabMenu)
				{
					itemGrabMenu.setSourceItem(this.sourceItem);
					if (Game1.options.SnappyMenus)
					{
						itemGrabMenu.currentlySnappedComponent = base.currentlySnappedComponent;
						itemGrabMenu.snapCursorToCurrentSnappedComponent();
					}
				}
			}
			if (base.heldItem?.QualifiedItemId == "(O)326")
			{
				base.heldItem = null;
				Game1.player.canUnderstandDwarves = true;
				this.poof = new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 320, 64, 64), 50f, 8, 0, new Vector2(x - x % 64 + 16, y - y % 64 + 16), flicker: false, flipped: false);
				Game1.playSound("fireball");
			}
			else if (base.heldItem is Object obj && obj?.QualifiedItemId == "(O)434")
			{
				base.heldItem = null;
				base.exitThisMenu(playSound: false);
				Game1.player.eatObject(obj, overrideFullness: true);
			}
			else if (base.heldItem != null && base.heldItem.IsRecipe)
			{
				base.heldItem.LearnRecipe();
				this.poof = new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 320, 64, 64), 50f, 8, 0, new Vector2(x - x % 64 + 16, y - y % 64 + 16), flicker: false, flipped: false);
				Game1.playSound("newRecipe");
				base.heldItem = null;
			}
			else if (Game1.player.addItemToInventoryBool(base.heldItem))
			{
				base.heldItem = null;
				Game1.playSound("coin");
			}
		}
		else if (this.reverseGrab || this.behaviorFunction != null)
		{
			this.behaviorFunction(base.heldItem, Game1.player);
			if (Game1.activeClickableMenu is ItemGrabMenu itemGrabMenu2)
			{
				itemGrabMenu2.setSourceItem(this.sourceItem);
			}
			if (this.destroyItemOnClick)
			{
				base.heldItem = null;
			}
		}
	}

	/// <inheritdoc />
	public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
	{
		if (this.snappedtoBottom)
		{
			base.movePosition((newBounds.Width - oldBounds.Width) / 2, Game1.uiViewport.Height - (base.yPositionOnScreen + base.height - IClickableMenu.spaceToClearTopBorder));
		}
		else
		{
			base.movePosition((newBounds.Width - oldBounds.Width) / 2, (newBounds.Height - oldBounds.Height) / 2);
		}
		this.ItemsToGrabMenu?.gameWindowSizeChanged(oldBounds, newBounds);
		this.RepositionSideButtons();
		if (this.CanHaveColorPicker() && this.sourceItem is Chest chest)
		{
			this.chestColorPicker = new DiscreteColorPicker(base.xPositionOnScreen, base.yPositionOnScreen - 64 - IClickableMenu.borderWidth * 2, chest.playerChoiceColor.Value, new Chest(playerChest: true, this.sourceItem.ItemId));
		}
	}

	/// <inheritdoc />
	public override void receiveLeftClick(int x, int y, bool playSound = true)
	{
		base.receiveLeftClick(x, y, !this.destroyItemOnClick);
		if (this.shippingBin && this.lastShippedHolder.containsPoint(x, y))
		{
			if (Game1.getFarm().lastItemShipped == null)
			{
				return;
			}
			Game1.getFarm().getShippingBin(Game1.player).Remove(Game1.getFarm().lastItemShipped);
			if (Game1.player.addItemToInventoryBool(Game1.getFarm().lastItemShipped))
			{
				Game1.playSound("coin");
				Game1.getFarm().lastItemShipped = null;
				if (Game1.player.ActiveObject != null)
				{
					Game1.player.showCarrying();
					Game1.player.Halt();
				}
			}
			else
			{
				Game1.getFarm().getShippingBin(Game1.player).Add(Game1.getFarm().lastItemShipped);
			}
			return;
		}
		if (this.chestColorPicker != null)
		{
			this.chestColorPicker.receiveLeftClick(x, y);
			if (this.sourceItem is Chest chest)
			{
				chest.playerChoiceColor.Value = DiscreteColorPicker.getColorFromSelection(this.chestColorPicker.colorSelection);
			}
		}
		if (this.colorPickerToggleButton != null && this.colorPickerToggleButton.containsPoint(x, y))
		{
			Game1.player.showChestColorPicker = !Game1.player.showChestColorPicker;
			this.chestColorPicker.visible = Game1.player.showChestColorPicker;
			try
			{
				Game1.playSound("drumkit6");
			}
			catch (Exception)
			{
			}
			this.SetupBorderNeighbors();
			return;
		}
		if (this.whichSpecialButton != -1 && this.specialButton != null && this.specialButton.containsPoint(x, y))
		{
			Game1.playSound("drumkit6");
			if (this.whichSpecialButton == 1 && this.context is JunimoHut hut)
			{
				hut.noHarvest.Value = !hut.noHarvest.Value;
				this.specialButton.sourceRect.X = (hut.noHarvest.Value ? 124 : 108);
			}
			return;
		}
		if (base.heldItem == null && this.showReceivingMenu)
		{
			base.heldItem = this.ItemsToGrabMenu.leftClick(x, y, base.heldItem, playSound: false);
			if (base.heldItem != null && this.behaviorOnItemGrab != null)
			{
				this.behaviorOnItemGrab(base.heldItem, Game1.player);
				if (Game1.activeClickableMenu is ItemGrabMenu itemGrabMenu)
				{
					itemGrabMenu.setSourceItem(this.sourceItem);
					if (Game1.options.SnappyMenus)
					{
						itemGrabMenu.currentlySnappedComponent = base.currentlySnappedComponent;
						itemGrabMenu.snapCursorToCurrentSnappedComponent();
					}
				}
			}
			string text = base.heldItem?.QualifiedItemId;
			if (!(text == "(O)326"))
			{
				if (text == "(O)102")
				{
					base.heldItem = null;
					Game1.player.foundArtifact("102", 1);
					this.poof = new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 320, 64, 64), 50f, 8, 0, new Vector2(x - x % 64 + 16, y - y % 64 + 16), flicker: false, flipped: false);
					Game1.playSound("fireball");
				}
			}
			else
			{
				base.heldItem = null;
				Game1.player.canUnderstandDwarves = true;
				this.poof = new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 320, 64, 64), 50f, 8, 0, new Vector2(x - x % 64 + 16, y - y % 64 + 16), flicker: false, flipped: false);
				Game1.playSound("fireball");
			}
			if (base.heldItem is Object stardrop && stardrop?.QualifiedItemId == "(O)434")
			{
				base.heldItem = null;
				base.exitThisMenu(playSound: false);
				Game1.player.eatObject(stardrop, overrideFullness: true);
			}
			else if (base.heldItem != null && base.heldItem.IsRecipe)
			{
				base.heldItem.LearnRecipe();
				this.poof = new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 320, 64, 64), 50f, 8, 0, new Vector2(x - x % 64 + 16, y - y % 64 + 16), flicker: false, flipped: false);
				Game1.playSound("newRecipe");
				base.heldItem = null;
			}
			else if (Game1.player.addItemToInventoryBool(base.heldItem))
			{
				base.heldItem = null;
				Game1.playSound("coin");
			}
		}
		else if ((this.reverseGrab || this.behaviorFunction != null) && this.isWithinBounds(x, y))
		{
			this.behaviorFunction(base.heldItem, Game1.player);
			if (Game1.activeClickableMenu is ItemGrabMenu itemGrabMenu2)
			{
				itemGrabMenu2.setSourceItem(this.sourceItem);
				if (Game1.options.SnappyMenus)
				{
					itemGrabMenu2.currentlySnappedComponent = base.currentlySnappedComponent;
					itemGrabMenu2.snapCursorToCurrentSnappedComponent();
				}
			}
			if (this.destroyItemOnClick)
			{
				base.heldItem = null;
				return;
			}
		}
		if (this.organizeButton != null && this.organizeButton.containsPoint(x, y))
		{
			ItemGrabMenu.organizeItemsInList(this.ItemsToGrabMenu.actualInventory);
			Game1.activeClickableMenu = new ItemGrabMenu(this);
			Game1.playSound("Ship");
		}
		else if (this.fillStacksButton != null && this.fillStacksButton.containsPoint(x, y))
		{
			this.FillOutStacks();
			Game1.playSound("Ship");
		}
		else if (this.junimoNoteIcon != null && this.junimoNoteIcon.containsPoint(x, y))
		{
			if (this.readyToClose())
			{
				Game1.activeClickableMenu = new JunimoNoteMenu(fromGameMenu: true)
				{
					menuToReturnTo = this
				};
			}
		}
		else if (base.heldItem != null && !this.isWithinBounds(x, y) && base.heldItem.canBeTrashed())
		{
			this.DropHeldItem();
		}
	}

	/// <summary>Merge any items from the player inventory into an equivalent stack in the chest where possible.</summary>
	public void FillOutStacks()
	{
		IList<Item> playerInventory = base.inventory.actualInventory;
		IList<Item> chestInventory = this.ItemsToGrabMenu.actualInventory;
		HashSet<int> affectedChestSlots = new HashSet<int>();
		ILookup<string, Item> prevChestItemsById = chestInventory.Where((Item item) => item != null).ToLookup((Item item) => item.QualifiedItemId);
		if (prevChestItemsById.Count == 0)
		{
			return;
		}
		for (int playerIndex = 0; playerIndex < playerInventory.Count; playerIndex++)
		{
			Item playerItem = playerInventory[playerIndex];
			if (playerItem == null)
			{
				continue;
			}
			bool canStack = false;
			foreach (Item item in prevChestItemsById[playerItem.QualifiedItemId])
			{
				canStack = item.canStackWith(playerItem);
				if (canStack)
				{
					break;
				}
			}
			if (!canStack)
			{
				continue;
			}
			Item originalPlayerItem = playerItem;
			bool mergedAny = false;
			int firstEmptyChestIndex = -1;
			for (int chestIndex = 0; chestIndex < chestInventory.Count; chestIndex++)
			{
				Item chestSlot = chestInventory[chestIndex];
				if (chestSlot == null)
				{
					if (firstEmptyChestIndex == -1)
					{
						firstEmptyChestIndex = chestIndex;
					}
				}
				else
				{
					if (!chestSlot.canStackWith(playerItem))
					{
						continue;
					}
					int toRemove = playerItem.Stack - chestSlot.addToStack(playerItem);
					if (toRemove > 0)
					{
						mergedAny = true;
						affectedChestSlots.Add(chestIndex);
						playerItem = playerItem.ConsumeStack(toRemove);
						if (playerItem == null)
						{
							playerInventory[playerIndex] = null;
							break;
						}
					}
				}
			}
			if (playerItem != null)
			{
				if (firstEmptyChestIndex == -1 && chestInventory.Count < this.ItemsToGrabMenu.capacity)
				{
					firstEmptyChestIndex = chestInventory.Count;
					chestInventory.Add(null);
				}
				if (firstEmptyChestIndex > -1)
				{
					mergedAny = true;
					affectedChestSlots.Add(firstEmptyChestIndex);
					playerItem.onDetachedFromParent();
					chestInventory[firstEmptyChestIndex] = playerItem;
					playerInventory[playerIndex] = null;
				}
			}
			if (mergedAny)
			{
				TransferredItemSprite itemSprite = new TransferredItemSprite(originalPlayerItem.getOne(), base.inventory.inventory[playerIndex].bounds.X, base.inventory.inventory[playerIndex].bounds.Y);
				this._transferredItemSprites.Add(itemSprite);
			}
		}
		foreach (int slotIndex in affectedChestSlots)
		{
			this.ItemsToGrabMenu.ShakeItem(slotIndex);
		}
	}

	/// <summary>Consolidate and sort item stacks in an item list.</summary>
	/// <param name="items">The item list to change.</param>
	public static void organizeItemsInList(IList<Item> items)
	{
		List<Item> copy = new List<Item>(items);
		List<Item> tools = new List<Item>();
		copy.RemoveAll(delegate(Item item)
		{
			if (item == null)
			{
				return true;
			}
			if (item is Tool)
			{
				tools.Add(item);
				return true;
			}
			return false;
		});
		for (int i = 0; i < copy.Count; i++)
		{
			Item current_item = copy[i];
			if (current_item.getRemainingStackSpace() <= 0)
			{
				continue;
			}
			for (int j = i + 1; j < copy.Count; j++)
			{
				Item other_item = copy[j];
				if (current_item.canStackWith(other_item))
				{
					other_item.Stack = current_item.addToStack(other_item);
					if (other_item.Stack == 0)
					{
						copy.RemoveAt(j);
						j--;
					}
				}
			}
		}
		copy.Sort();
		copy.InsertRange(0, tools);
		for (int i2 = 0; i2 < items.Count; i2++)
		{
			items[i2] = null;
		}
		for (int i3 = 0; i3 < copy.Count; i3++)
		{
			items[i3] = copy[i3];
		}
	}

	public bool areAllItemsTaken()
	{
		for (int i = 0; i < this.ItemsToGrabMenu.actualInventory.Count; i++)
		{
			if (this.ItemsToGrabMenu.actualInventory[i] != null)
			{
				return false;
			}
		}
		return true;
	}

	/// <inheritdoc />
	public override void receiveGamePadButton(Buttons button)
	{
		base.receiveGamePadButton(button);
		switch (button)
		{
		case Buttons.Back:
			if (this.organizeButton != null)
			{
				ItemGrabMenu.organizeItemsInList(Game1.player.Items);
				Game1.playSound("Ship");
			}
			break;
		case Buttons.RightShoulder:
		{
			ClickableComponent fill_stacks_component = base.getComponentWithID(12952);
			if (fill_stacks_component != null)
			{
				this.setCurrentlySnappedComponentTo(fill_stacks_component.myID);
				this.snapCursorToCurrentSnappedComponent();
				break;
			}
			int highest_y = -1;
			ClickableComponent highest_component = null;
			foreach (ClickableComponent component2 in base.allClickableComponents)
			{
				if (component2.region == 15923 && (highest_y == -1 || component2.bounds.Y < highest_y))
				{
					highest_y = component2.bounds.Y;
					highest_component = component2;
				}
			}
			if (highest_component != null)
			{
				this.setCurrentlySnappedComponentTo(highest_component.myID);
				this.snapCursorToCurrentSnappedComponent();
			}
			break;
		}
		case Buttons.LeftShoulder:
		{
			if (this.shippingBin)
			{
				break;
			}
			ClickableComponent component = base.getComponentWithID(53910);
			if (component != null)
			{
				this.setCurrentlySnappedComponentTo(component.myID);
				this.snapCursorToCurrentSnappedComponent();
				break;
			}
			component = base.getComponentWithID(0);
			if (component != null)
			{
				this.setCurrentlySnappedComponentTo(0);
				this.snapCursorToCurrentSnappedComponent();
			}
			break;
		}
		}
	}

	/// <inheritdoc />
	public override void receiveKeyPress(Keys key)
	{
		if (Game1.options.snappyMenus && Game1.options.gamepadControls)
		{
			base.applyMovementKey(key);
		}
		if ((this.canExitOnKey || this.areAllItemsTaken()) && Game1.options.doesInputListContain(Game1.options.menuButton, key) && this.readyToClose())
		{
			base.exitThisMenu();
			Event currentEvent = Game1.currentLocation.currentEvent;
			if (currentEvent != null && currentEvent.CurrentCommand > 0)
			{
				Game1.currentLocation.currentEvent.CurrentCommand++;
			}
		}
		else if (Game1.options.doesInputListContain(Game1.options.menuButton, key) && base.heldItem != null)
		{
			Game1.setMousePosition(base.trashCan.bounds.Center);
		}
		if (key == Keys.Delete && base.heldItem != null && base.heldItem.canBeTrashed())
		{
			Utility.trashItem(base.heldItem);
			base.heldItem = null;
		}
	}

	/// <inheritdoc />
	public override void update(GameTime time)
	{
		base.update(time);
		if (!this.HasUpdateTicked)
		{
			this.HasUpdateTicked = true;
			if (this.source == 4)
			{
				IList<Item> items = this.ItemsToGrabMenu.actualInventory;
				for (int i = 0; i < items.Count; i++)
				{
					if (items[i]?.QualifiedItemId == "(O)434")
					{
						List<Item> remainingItems = new List<Item>(items);
						remainingItems.RemoveAt(i);
						remainingItems.RemoveAll((Item p) => p == null);
						if (remainingItems.Count > 0)
						{
							Game1.nextClickableMenu.Insert(0, ItemGrabMenu.CreateOverflowMenu(remainingItems, base.inventory.onAddItem));
						}
						this.essential = false;
						this.superEssential = false;
						base.exitThisMenu(playSound: false);
						Game1.player.eatObject(items[i] as Object, overrideFullness: true);
						return;
					}
				}
			}
		}
		if (this.poof != null && this.poof.update(time))
		{
			this.poof = null;
		}
		this.chestColorPicker?.update(time);
		if (this.sourceItem is Chest chest && this._sourceItemInCurrentLocation)
		{
			Vector2 tileLocation = chest.tileLocation.Value;
			if (tileLocation != Vector2.Zero && !Game1.currentLocation.objects.ContainsKey(tileLocation))
			{
				if (Game1.activeClickableMenu != null)
				{
					Game1.activeClickableMenu.emergencyShutDown();
				}
				Game1.exitActiveMenu();
			}
		}
		this._transferredItemSprites.RemoveAll((TransferredItemSprite sprite) => sprite.Update(time));
	}

	/// <inheritdoc />
	public override void performHoverAction(int x, int y)
	{
		base.hoveredItem = null;
		base.hoverText = "";
		base.performHoverAction(x, y);
		if (this.colorPickerToggleButton != null)
		{
			this.colorPickerToggleButton.tryHover(x, y, 0.25f);
			if (this.colorPickerToggleButton.containsPoint(x, y))
			{
				base.hoverText = this.colorPickerToggleButton.hoverText;
			}
		}
		if (this.organizeButton != null)
		{
			this.organizeButton.tryHover(x, y, 0.25f);
			if (this.organizeButton.containsPoint(x, y))
			{
				base.hoverText = this.organizeButton.hoverText;
			}
		}
		if (this.fillStacksButton != null)
		{
			this.fillStacksButton.tryHover(x, y, 0.25f);
			if (this.fillStacksButton.containsPoint(x, y))
			{
				base.hoverText = this.fillStacksButton.hoverText;
			}
		}
		this.specialButton?.tryHover(x, y, 0.25f);
		if (this.showReceivingMenu)
		{
			Item item_grab_hovered_item = this.ItemsToGrabMenu.hover(x, y, base.heldItem);
			if (item_grab_hovered_item != null)
			{
				base.hoveredItem = item_grab_hovered_item;
			}
		}
		if (this.junimoNoteIcon != null)
		{
			this.junimoNoteIcon.tryHover(x, y);
			if (this.junimoNoteIcon.containsPoint(x, y))
			{
				base.hoverText = this.junimoNoteIcon.hoverText;
			}
			if (GameMenu.bundleItemHovered)
			{
				this.junimoNoteIcon.scale = this.junimoNoteIcon.baseScale + (float)Math.Sin((float)this.junimoNotePulser / 100f) / 4f;
				this.junimoNotePulser += (int)Game1.currentGameTime.ElapsedGameTime.TotalMilliseconds;
			}
			else
			{
				this.junimoNotePulser = 0;
				this.junimoNoteIcon.scale = this.junimoNoteIcon.baseScale;
			}
		}
		if (base.hoverText != null)
		{
			return;
		}
		if (this.organizeButton != null)
		{
			base.hoverText = null;
			this.organizeButton.tryHover(x, y);
			if (this.organizeButton.containsPoint(x, y))
			{
				base.hoverText = this.organizeButton.hoverText;
			}
		}
		if (this.shippingBin)
		{
			base.hoverText = null;
			if (this.lastShippedHolder.containsPoint(x, y) && Game1.getFarm().lastItemShipped != null)
			{
				base.hoverText = this.lastShippedHolder.hoverText;
			}
		}
		this.chestColorPicker?.performHoverAction(x, y);
	}

	/// <inheritdoc />
	public override void draw(SpriteBatch b)
	{
		if (this.drawBG && !Game1.options.showClearBackgrounds)
		{
			b.Draw(Game1.fadeToBlackRect, new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), Color.Black * 0.5f);
		}
		base.draw(b, drawUpperPortion: false, drawDescriptionArea: false);
		if (this.showReceivingMenu)
		{
			b.Draw(Game1.mouseCursors, new Vector2(base.xPositionOnScreen - 64, base.yPositionOnScreen + base.height / 2 + 64 + 16), new Rectangle(16, 368, 12, 16), Color.White, 4.712389f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
			b.Draw(Game1.mouseCursors, new Vector2(base.xPositionOnScreen - 64, base.yPositionOnScreen + base.height / 2 + 64 - 16), new Rectangle(21, 368, 11, 16), Color.White, 4.712389f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
			b.Draw(Game1.mouseCursors, new Vector2(base.xPositionOnScreen - 40, base.yPositionOnScreen + base.height / 2 + 64 - 44), new Rectangle(4, 372, 8, 11), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
			Game1.drawDialogueBox(this.ItemsToGrabMenu.xPositionOnScreen - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder, this.ItemsToGrabMenu.yPositionOnScreen - IClickableMenu.borderWidth - IClickableMenu.spaceToClearTopBorder + this.storageSpaceTopBorderOffset, this.ItemsToGrabMenu.width + IClickableMenu.borderWidth * 2 + IClickableMenu.spaceToClearSideBorder * 2, this.ItemsToGrabMenu.height + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth * 2 - this.storageSpaceTopBorderOffset, speaker: false, drawOnlyBox: true);
			if ((this.source != 1 || !(this.sourceItem is Chest chest) || (chest.SpecialChestType != Chest.SpecialChestTypes.MiniShippingBin && chest.SpecialChestType != Chest.SpecialChestTypes.JunimoChest && chest.SpecialChestType != Chest.SpecialChestTypes.Enricher)) && this.source != 0)
			{
				b.Draw(Game1.mouseCursors, new Vector2(this.ItemsToGrabMenu.xPositionOnScreen - 100, base.yPositionOnScreen + 64 + 16), new Rectangle(16, 368, 12, 16), Color.White, 4.712389f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
				b.Draw(Game1.mouseCursors, new Vector2(this.ItemsToGrabMenu.xPositionOnScreen - 100, base.yPositionOnScreen + 64 - 16), new Rectangle(21, 368, 11, 16), Color.White, 4.712389f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
				Rectangle sourceRect = new Rectangle(127, 412, 10, 11);
				switch (this.source)
				{
				case 3:
					sourceRect.X += 10;
					break;
				case 4:
					sourceRect.X += 20;
					break;
				}
				b.Draw(Game1.mouseCursors, new Vector2(this.ItemsToGrabMenu.xPositionOnScreen - 80, base.yPositionOnScreen + 64 - 44), sourceRect, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
			}
			this.ItemsToGrabMenu.draw(b);
		}
		else if (this.message != null)
		{
			Game1.drawDialogueBox(Game1.uiViewport.Width / 2, this.ItemsToGrabMenu.yPositionOnScreen + this.ItemsToGrabMenu.height / 2, speaker: false, drawOnlyBox: false, this.message);
		}
		this.poof?.draw(b, localPosition: true);
		foreach (TransferredItemSprite transferredItemSprite in this._transferredItemSprites)
		{
			transferredItemSprite.Draw(b);
		}
		if (this.shippingBin && Game1.getFarm().lastItemShipped != null)
		{
			this.lastShippedHolder.draw(b);
			Game1.getFarm().lastItemShipped.drawInMenu(b, new Vector2(this.lastShippedHolder.bounds.X + 16, this.lastShippedHolder.bounds.Y + 16), 1f);
			b.Draw(Game1.mouseCursors, new Vector2(this.lastShippedHolder.bounds.X + -8, this.lastShippedHolder.bounds.Bottom - 100), new Rectangle(325, 448, 5, 14), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
			b.Draw(Game1.mouseCursors, new Vector2(this.lastShippedHolder.bounds.X + 84, this.lastShippedHolder.bounds.Bottom - 100), new Rectangle(325, 448, 5, 14), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
			b.Draw(Game1.mouseCursors, new Vector2(this.lastShippedHolder.bounds.X + -8, this.lastShippedHolder.bounds.Bottom - 44), new Rectangle(325, 452, 5, 13), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
			b.Draw(Game1.mouseCursors, new Vector2(this.lastShippedHolder.bounds.X + 84, this.lastShippedHolder.bounds.Bottom - 44), new Rectangle(325, 452, 5, 13), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
		}
		if (this.colorPickerToggleButton != null)
		{
			this.colorPickerToggleButton.draw(b);
		}
		else
		{
			this.specialButton?.draw(b);
		}
		this.chestColorPicker?.draw(b);
		this.organizeButton?.draw(b);
		this.fillStacksButton?.draw(b);
		this.junimoNoteIcon?.draw(b);
		if (base.hoverText != null && (base.hoveredItem == null || this.ItemsToGrabMenu == null))
		{
			if (base.hoverAmount > 0)
			{
				IClickableMenu.drawToolTip(b, base.hoverText, "", null, heldItem: true, -1, 0, null, -1, null, base.hoverAmount);
			}
			else
			{
				IClickableMenu.drawHoverText(b, base.hoverText, Game1.smallFont);
			}
		}
		if (base.hoveredItem != null)
		{
			IClickableMenu.drawToolTip(b, base.hoveredItem.getDescription(), base.hoveredItem.DisplayName, base.hoveredItem, base.heldItem != null);
		}
		else if (base.hoveredItem != null && this.ItemsToGrabMenu != null)
		{
			IClickableMenu.drawToolTip(b, this.ItemsToGrabMenu.descriptionText, this.ItemsToGrabMenu.descriptionTitle, base.hoveredItem, base.heldItem != null);
		}
		base.heldItem?.drawInMenu(b, new Vector2(Game1.getOldMouseX() + 8, Game1.getOldMouseY() + 8), 1f);
		Game1.mouseCursorTransparency = 1f;
		base.drawMouse(b);
	}

	/// <inheritdoc />
	protected override void cleanupBeforeExit()
	{
		base.cleanupBeforeExit();
		if (this.superEssential)
		{
			this.DropRemainingItems();
		}
	}

	public override void emergencyShutDown()
	{
		base.emergencyShutDown();
		if (!this.essential)
		{
			return;
		}
		foreach (Item item in this.ItemsToGrabMenu.actualInventory)
		{
			if (item != null)
			{
				Item leftOver = Game1.player.addItemToInventory(item);
				if (leftOver != null)
				{
					Game1.createItemDebris(leftOver, Game1.player.getStandingPosition(), Game1.player.FacingDirection);
				}
			}
		}
	}
}
