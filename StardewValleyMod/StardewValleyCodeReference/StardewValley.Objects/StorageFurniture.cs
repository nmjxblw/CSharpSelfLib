using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.Delegates;
using StardewValley.GameData.Shops;
using StardewValley.Internal;
using StardewValley.Menus;
using StardewValley.Network;

namespace StardewValley.Objects;

[XmlInclude(typeof(FishTankFurniture))]
public class StorageFurniture : Furniture
{
	[XmlElement("heldItems")]
	public readonly NetObjectList<Item> heldItems = new NetObjectList<Item>();

	[XmlIgnore]
	public readonly NetMutex mutex = new NetMutex();

	public StorageFurniture()
	{
	}

	public StorageFurniture(string itemId, Vector2 tile, int initialRotations)
		: base(itemId, tile, initialRotations)
	{
	}

	public StorageFurniture(string itemId, Vector2 tile)
		: base(itemId, tile)
	{
	}

	/// <inheritdoc />
	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(this.heldItems, "heldItems").AddField(this.mutex.NetFields, "mutex.NetFields");
	}

	public override bool canBeRemoved(Farmer who)
	{
		if (this.mutex.IsLocked())
		{
			return false;
		}
		return base.canBeRemoved(who);
	}

	/// <inheritdoc />
	public override bool checkForAction(Farmer who, bool justCheckingForActivity = false)
	{
		if (justCheckingForActivity)
		{
			return true;
		}
		this.mutex.RequestLock(ShowMenu);
		return true;
	}

	public virtual void ShowMenu()
	{
		this.ShowShopMenu();
	}

	public virtual void ShowChestMenu()
	{
		Game1.activeClickableMenu = new ItemGrabMenu(this.heldItems, reverseGrab: false, showReceivingMenu: true, InventoryMenu.highlightAllItems, GrabItemFromInventory, null, GrabItemFromChest, snapToBottom: false, canBeExitedWithKey: true, playRightClickSound: true, allowRightClick: true, showOrganizeButton: true, 1, this, -1, this)
		{
			behaviorBeforeCleanup = delegate
			{
				this.mutex.ReleaseLock();
				this.OnMenuClose();
			}
		};
		Game1.playSound("dwop");
	}

	public virtual void GrabItemFromInventory(Item item, Farmer who)
	{
		if (item.Stack == 0)
		{
			item.Stack = 1;
		}
		Item tmp = this.AddItem(item);
		if (tmp == null)
		{
			who.removeItemFromInventory(item);
		}
		else
		{
			tmp = who.addItemToInventory(tmp);
		}
		this.ClearNulls();
		int oldID = ((Game1.activeClickableMenu.currentlySnappedComponent != null) ? Game1.activeClickableMenu.currentlySnappedComponent.myID : (-1));
		this.ShowChestMenu();
		(Game1.activeClickableMenu as ItemGrabMenu).heldItem = tmp;
		if (oldID != -1)
		{
			Game1.activeClickableMenu.currentlySnappedComponent = Game1.activeClickableMenu.getComponentWithID(oldID);
			Game1.activeClickableMenu.snapCursorToCurrentSnappedComponent();
		}
	}

	public virtual bool HighlightItems(Item item)
	{
		return InventoryMenu.highlightAllItems(item);
	}

	public virtual void GrabItemFromChest(Item item, Farmer who)
	{
		if (who.couldInventoryAcceptThisItem(item))
		{
			this.heldItems.Remove(item);
			this.ClearNulls();
			this.ShowChestMenu();
		}
	}

	public virtual void ClearNulls()
	{
		this.heldItems.RemoveWhere((Item slot) => slot == null);
	}

	public virtual Item AddItem(Item item)
	{
		item.resetState();
		this.ClearNulls();
		for (int i = 0; i < this.heldItems.Count; i++)
		{
			if (this.heldItems[i] != null && this.heldItems[i].canStackWith(item))
			{
				int toRemove = item.Stack - this.heldItems[i].addToStack(item);
				if (item.ConsumeStack(toRemove) == null)
				{
					return null;
				}
			}
		}
		if (this.heldItems.Count < 36)
		{
			this.heldItems.Add(item);
			return null;
		}
		return item;
	}

	public virtual void ShowShopMenu()
	{
		List<Item> list = this.heldItems.ToList();
		list.Sort(SortItems);
		Dictionary<ISalable, ItemStockInformation> contents = new Dictionary<ISalable, ItemStockInformation>();
		foreach (Item item in list)
		{
			contents[item] = new ItemStockInformation(0, 1, null, null, LimitedStockMode.None);
		}
		Game1.activeClickableMenu = new ShopMenu(this.GetShopMenuContext(), contents, 0, null, onDresserItemWithdrawn, onDresserItemDeposited)
		{
			source = this,
			behaviorBeforeCleanup = delegate
			{
				this.mutex.ReleaseLock();
				this.OnMenuClose();
			}
		};
	}

	public virtual void OnMenuClose()
	{
	}

	public virtual string GetShopMenuContext()
	{
		return "Dresser";
	}

	/// <inheritdoc />
	public override bool canBeTrashed()
	{
		if (this.heldItems.Count > 0)
		{
			return false;
		}
		return base.canBeTrashed();
	}

	public override void DayUpdate()
	{
		base.DayUpdate();
		this.mutex.ReleaseLock();
	}

	/// <inheritdoc />
	protected override Item GetOneNew()
	{
		return new StorageFurniture(base.ItemId, base.tileLocation.Value);
	}

	public virtual int SortItems(Item a, Item b)
	{
		if (a.Category != b.Category)
		{
			return a.Category.CompareTo(b.Category);
		}
		if (a is Clothing clothingA && b is Clothing clothingB && clothingA.clothesType.Value != clothingB.clothesType.Value)
		{
			return clothingA.clothesType.Value.CompareTo(clothingB.clothesType.Value);
		}
		return a.ParentSheetIndex.CompareTo(b.ParentSheetIndex);
	}

	/// <summary>Handle an item being taken from the storage furniture.</summary>
	/// <inheritdoc cref="T:StardewValley.Menus.ShopMenu.OnPurchaseDelegate" />
	public virtual bool onDresserItemWithdrawn(ISalable salable, Farmer who, int countTaken, ItemStockInformation stock)
	{
		if (salable is Item item)
		{
			this.heldItems.Remove(item);
		}
		return false;
	}

	public override void updateWhenCurrentLocation(GameTime time)
	{
		GameLocation environment = this.Location;
		if (environment != null)
		{
			this.mutex.Update(environment);
		}
		base.updateWhenCurrentLocation(time);
	}

	public virtual bool onDresserItemDeposited(ISalable deposited_salable)
	{
		if (deposited_salable is Item depositedItem)
		{
			this.heldItems.Add(depositedItem);
			if (Game1.activeClickableMenu is ShopMenu)
			{
				Dictionary<ISalable, ItemStockInformation> contents = new Dictionary<ISalable, ItemStockInformation>();
				List<Item> list = this.heldItems.ToList();
				list.Sort(SortItems);
				foreach (Item item in list)
				{
					contents[item] = new ItemStockInformation(0, 1, null, null, LimitedStockMode.None);
				}
				(Game1.activeClickableMenu as ShopMenu).setItemPriceAndStock(contents);
				Game1.playSound("dwop");
				return true;
			}
		}
		return false;
	}

	/// <inheritdoc />
	public override bool ForEachItem(ForEachItemDelegate handler, GetForEachItemPathDelegate getPath)
	{
		if (base.ForEachItem(handler, getPath))
		{
			return ForEachItemHelper.ApplyToList(this.heldItems, handler, getPath);
		}
		return false;
	}
}
