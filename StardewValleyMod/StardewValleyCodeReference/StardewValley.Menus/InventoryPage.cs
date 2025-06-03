using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Netcode;
using StardewValley.Characters;
using StardewValley.Objects;
using StardewValley.Objects.Trinkets;

namespace StardewValley.Menus;

public class InventoryPage : IClickableMenu
{
	public const int region_inventory = 100;

	public const int region_hat = 101;

	public const int region_ring1 = 102;

	public const int region_ring2 = 103;

	public const int region_boots = 104;

	public const int region_trashCan = 105;

	public const int region_organizeButton = 106;

	public const int region_accessory = 107;

	public const int region_shirt = 108;

	public const int region_pants = 109;

	public const int region_shoes = 110;

	public const int region_trinkets = 120;

	public InventoryMenu inventory;

	public string hoverText = "";

	public string hoverTitle = "";

	public int hoverAmount;

	public Item hoveredItem;

	public List<ClickableComponent> equipmentIcons = new List<ClickableComponent>();

	public ClickableComponent portrait;

	public ClickableTextureComponent trashCan;

	public ClickableTextureComponent organizeButton;

	private float trashCanLidRotation;

	public ClickableTextureComponent junimoNoteIcon;

	private int junimoNotePulser;

	protected Pet _pet;

	protected Horse _horse;

	public InventoryPage(int x, int y, int width, int height)
		: base(x, y, width, height)
	{
		this.inventory = new InventoryMenu(base.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth, base.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth, playerInventory: true);
		bool num = Game1.player.stats.Get("trinketSlots") != 0;
		int trinkets_or_trash = (num ? 120 : 105);
		this.equipmentIcons.Add(new ClickableComponent(new Rectangle(base.xPositionOnScreen + 48, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 4 + 256 - 12, 64, 64), "Left Ring")
		{
			myID = 102,
			downNeighborID = 103,
			upNeighborID = Game1.player.MaxItems - 12,
			rightNeighborID = 101,
			fullyImmutable = false
		});
		this.equipmentIcons.Add(new ClickableComponent(new Rectangle(base.xPositionOnScreen + 48, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 4 + 320 - 12, 64, 64), "Right Ring")
		{
			myID = 103,
			upNeighborID = 102,
			downNeighborID = 104,
			rightNeighborID = 108,
			fullyImmutable = true
		});
		this.equipmentIcons.Add(new ClickableComponent(new Rectangle(base.xPositionOnScreen + 48, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 4 + 384 - 12, 64, 64), "Boots")
		{
			myID = 104,
			upNeighborID = 103,
			rightNeighborID = 109,
			fullyImmutable = true
		});
		this.portrait = new ClickableComponent(new Rectangle(base.xPositionOnScreen + 192 - 8 - 64 + 32, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 256 - 8 + 64, 64, 96), "32");
		this.trashCan = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen + width / 3 + 576 + 32, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 192 + 64, 64, 104), Game1.mouseCursors, new Rectangle(564 + Game1.player.trashCanLevel * 18, 102, 18, 26), 4f)
		{
			myID = 105,
			upNeighborID = 106,
			leftNeighborID = 101
		};
		this.organizeButton = new ClickableTextureComponent("", new Rectangle(base.xPositionOnScreen + width, base.yPositionOnScreen + height / 3 - 64 + 8, 64, 64), "", Game1.content.LoadString("Strings\\UI:ItemGrab_Organize"), Game1.mouseCursors, new Rectangle(162, 440, 16, 16), 4f)
		{
			myID = 106,
			downNeighborID = 105,
			leftNeighborID = 11,
			upNeighborID = 898
		};
		this.equipmentIcons.Add(new ClickableComponent(new Rectangle(base.xPositionOnScreen + 48 + 208, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 4 + 256 - 12, 64, 64), "Hat")
		{
			myID = 101,
			leftNeighborID = 102,
			downNeighborID = 108,
			upNeighborID = Game1.player.MaxItems - 9,
			rightNeighborID = trinkets_or_trash,
			fullyImmutable = false
		});
		this.equipmentIcons.Add(new ClickableComponent(new Rectangle(base.xPositionOnScreen + 48 + 208, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 4 + 320 - 12, 64, 64), "Shirt")
		{
			myID = 108,
			upNeighborID = 101,
			downNeighborID = 109,
			rightNeighborID = trinkets_or_trash,
			leftNeighborID = 103,
			fullyImmutable = true
		});
		this.equipmentIcons.Add(new ClickableComponent(new Rectangle(base.xPositionOnScreen + 48 + 208, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 4 + 384 - 12, 64, 64), "Pants")
		{
			myID = 109,
			upNeighborID = 108,
			rightNeighborID = trinkets_or_trash,
			leftNeighborID = 104,
			fullyImmutable = true
		});
		if (num)
		{
			Farmer.MaximumTrinkets = 1;
			for (int i = 0; i < Farmer.MaximumTrinkets; i++)
			{
				ClickableComponent trinket_slot = new ClickableComponent(new Rectangle(base.xPositionOnScreen + 48 + 280, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 4 + (4 + i) * 64 - 12, 64, 64), "Trinket")
				{
					myID = 120 + i,
					upNeighborID = Game1.player.MaxItems - 8,
					rightNeighborID = 105,
					leftNeighborID = -99998,
					fullyImmutable = true
				};
				if (i < Farmer.MaximumTrinkets - 1)
				{
					trinket_slot.downNeighborID = -99998;
				}
				this.equipmentIcons.Add(trinket_slot);
			}
		}
		if (InventoryPage.ShouldShowJunimoNoteIcon())
		{
			this.junimoNoteIcon = new ClickableTextureComponent("", new Rectangle(base.xPositionOnScreen + width, base.yPositionOnScreen + 96, 64, 64), "", Game1.content.LoadString("Strings\\UI:GameMenu_JunimoNote_Hover"), Game1.mouseCursors, new Rectangle(331, 374, 15, 14), 4f)
			{
				myID = 898,
				leftNeighborID = 11,
				downNeighborID = 106
			};
		}
		this._pet = Game1.GetCharacterOfType<Pet>();
		this._horse = Game1.getCharacterFromName<Horse>(Game1.player.horseName.Value, mustBeVillager: false);
		if (this._horse == null && Game1.player.isRidingHorse() && Game1.player.mount.Name.Equals(Game1.player.horseName.Value))
		{
			this._horse = Game1.player.mount;
		}
	}

	public static bool ShouldShowJunimoNoteIcon()
	{
		if (Game1.player.hasOrWillReceiveMail("canReadJunimoText") && !Game1.player.hasOrWillReceiveMail("JojaMember"))
		{
			if (Game1.MasterPlayer.hasCompletedCommunityCenter())
			{
				if (Game1.player.hasOrWillReceiveMail("hasSeenAbandonedJunimoNote"))
				{
					return !Game1.MasterPlayer.hasOrWillReceiveMail("ccMovieTheater");
				}
				return false;
			}
			return true;
		}
		return false;
	}

	protected virtual bool checkHeldItem(Func<Item, bool> f = null)
	{
		return f?.Invoke(Game1.player.CursorSlotItem) ?? (Game1.player.CursorSlotItem != null);
	}

	protected virtual Item takeHeldItem()
	{
		Item cursorSlotItem = Game1.player.CursorSlotItem;
		Game1.player.CursorSlotItem = null;
		return cursorSlotItem;
	}

	protected virtual void setHeldItem(Item item)
	{
		item?.onDetachedFromParent();
		Game1.player.CursorSlotItem = item;
	}

	/// <inheritdoc />
	public override void receiveKeyPress(Keys key)
	{
		base.receiveKeyPress(key);
		if (Game1.isAnyGamePadButtonBeingPressed() && Game1.options.doesInputListContain(Game1.options.menuButton, key) && this.checkHeldItem())
		{
			Game1.setMousePosition(this.trashCan.bounds.Center);
		}
		if (key == Keys.Delete && this.checkHeldItem((Item i) => i?.canBeTrashed() ?? false))
		{
			Utility.trashItem(this.takeHeldItem());
		}
		if (Game1.options.doesInputListContain(Game1.options.inventorySlot1, key))
		{
			Game1.player.CurrentToolIndex = 0;
			Game1.playSound("toolSwap");
		}
		else if (Game1.options.doesInputListContain(Game1.options.inventorySlot2, key))
		{
			Game1.player.CurrentToolIndex = 1;
			Game1.playSound("toolSwap");
		}
		else if (Game1.options.doesInputListContain(Game1.options.inventorySlot3, key))
		{
			Game1.player.CurrentToolIndex = 2;
			Game1.playSound("toolSwap");
		}
		else if (Game1.options.doesInputListContain(Game1.options.inventorySlot4, key))
		{
			Game1.player.CurrentToolIndex = 3;
			Game1.playSound("toolSwap");
		}
		else if (Game1.options.doesInputListContain(Game1.options.inventorySlot5, key))
		{
			Game1.player.CurrentToolIndex = 4;
			Game1.playSound("toolSwap");
		}
		else if (Game1.options.doesInputListContain(Game1.options.inventorySlot6, key))
		{
			Game1.player.CurrentToolIndex = 5;
			Game1.playSound("toolSwap");
		}
		else if (Game1.options.doesInputListContain(Game1.options.inventorySlot7, key))
		{
			Game1.player.CurrentToolIndex = 6;
			Game1.playSound("toolSwap");
		}
		else if (Game1.options.doesInputListContain(Game1.options.inventorySlot8, key))
		{
			Game1.player.CurrentToolIndex = 7;
			Game1.playSound("toolSwap");
		}
		else if (Game1.options.doesInputListContain(Game1.options.inventorySlot9, key))
		{
			Game1.player.CurrentToolIndex = 8;
			Game1.playSound("toolSwap");
		}
		else if (Game1.options.doesInputListContain(Game1.options.inventorySlot10, key))
		{
			Game1.player.CurrentToolIndex = 9;
			Game1.playSound("toolSwap");
		}
		else if (Game1.options.doesInputListContain(Game1.options.inventorySlot11, key))
		{
			Game1.player.CurrentToolIndex = 10;
			Game1.playSound("toolSwap");
		}
		else if (Game1.options.doesInputListContain(Game1.options.inventorySlot12, key))
		{
			Game1.player.CurrentToolIndex = 11;
			Game1.playSound("toolSwap");
		}
	}

	public override void setUpForGamePadMode()
	{
		base.setUpForGamePadMode();
		this.inventory?.setUpForGamePadMode();
	}

	/// <inheritdoc />
	public override void receiveLeftClick(int x, int y, bool playSound = true)
	{
		foreach (ClickableComponent c in this.equipmentIcons)
		{
			if (!c.containsPoint(x, y))
			{
				continue;
			}
			Item newItem = Utility.PerformSpecialItemPlaceReplacement(Game1.player.CursorSlotItem);
			bool heldItemWasNull = newItem == null;
			switch (c.name)
			{
			case "Hat":
				if (newItem == null || newItem is Hat)
				{
					Item oldItem = Utility.PerformSpecialItemGrabReplacement(Game1.player.Equip((Hat)newItem, Game1.player.hat));
					this.setHeldItem(oldItem);
					if (Game1.player.hat.Value != null)
					{
						Game1.playSound("grassyStep");
					}
					else if (this.checkHeldItem())
					{
						Game1.playSound("dwop");
					}
				}
				break;
			case "Left Ring":
			case "Right Ring":
				if (newItem == null || newItem is Ring)
				{
					NetRef<Ring> ringField = ((c.name == "Left Ring") ? Game1.player.leftRing : Game1.player.rightRing);
					Item oldItem5 = Utility.PerformSpecialItemGrabReplacement(Game1.player.Equip((Ring)newItem, ringField));
					this.setHeldItem(oldItem5);
					if (Game1.player.leftRing.Value != null)
					{
						Game1.playSound("crit");
					}
					else if (this.checkHeldItem())
					{
						Game1.playSound("dwop");
					}
				}
				break;
			case "Boots":
				if (newItem == null || newItem is Boots)
				{
					Item oldItem2 = Utility.PerformSpecialItemGrabReplacement(Game1.player.Equip((Boots)newItem, Game1.player.boots));
					this.setHeldItem(oldItem2);
					if (Game1.player.boots.Value != null)
					{
						Game1.playSound("sandyStep");
						DelayedAction.playSoundAfterDelay("sandyStep", 150);
					}
					else if (this.checkHeldItem())
					{
						Game1.playSound("dwop");
					}
				}
				break;
			case "Shirt":
			{
				if (newItem != null)
				{
					Clothing obj2 = newItem as Clothing;
					if (obj2 == null || obj2.clothesType.Value != Clothing.ClothesType.SHIRT)
					{
						break;
					}
				}
				Item oldItem4 = Utility.PerformSpecialItemGrabReplacement(Game1.player.Equip((Clothing)newItem, Game1.player.shirtItem));
				this.setHeldItem(oldItem4);
				if (Game1.player.shirtItem.Value != null)
				{
					Game1.playSound("sandyStep");
				}
				else if (this.checkHeldItem())
				{
					Game1.playSound("dwop");
				}
				break;
			}
			case "Pants":
			{
				if (newItem != null)
				{
					Clothing obj = newItem as Clothing;
					if (obj == null || obj.clothesType.Value != Clothing.ClothesType.PANTS)
					{
						break;
					}
				}
				Item oldItem3 = Utility.PerformSpecialItemGrabReplacement(Game1.player.Equip((Clothing)newItem, Game1.player.pantsItem));
				this.setHeldItem(oldItem3);
				if (Game1.player.pantsItem.Value != null)
				{
					Game1.playSound("sandyStep");
				}
				else if (this.checkHeldItem())
				{
					Game1.playSound("dwop");
				}
				break;
			}
			case "Trinket":
				if (Game1.player.stats.Get("trinketSlots") != 0 && this.checkHeldItem((Item item) => item == null || item is Trinket))
				{
					int trinket_index = c.myID - 120;
					Trinket new_item = (Trinket)this.takeHeldItem();
					Trinket old_item = null;
					if (Game1.player.trinketItems.Count > trinket_index)
					{
						old_item = Game1.player.trinketItems[trinket_index];
					}
					old_item = (Trinket)Utility.PerformSpecialItemGrabReplacement(old_item);
					this.setHeldItem(old_item);
					while (Game1.player.trinketItems.Count <= trinket_index)
					{
						Game1.player.trinketItems.Add(null);
					}
					Game1.player.trinketItems[trinket_index] = new_item;
					if (Game1.player.trinketItems[trinket_index] != null)
					{
						Game1.playSound("clank");
					}
					else if (this.checkHeldItem())
					{
						Game1.playSound("dwop");
					}
				}
				break;
			}
			if (!heldItemWasNull || !this.checkHeldItem() || !Game1.oldKBState.IsKeyDown(Keys.LeftShift))
			{
				continue;
			}
			int i;
			for (i = 0; i < Game1.player.Items.Count; i++)
			{
				if (Game1.player.Items[i] == null || this.checkHeldItem((Item item) => Game1.player.Items[i].canStackWith(item)))
				{
					if (Game1.player.CurrentToolIndex == i && this.checkHeldItem())
					{
						Game1.player.CursorSlotItem.actionWhenBeingHeld(Game1.player);
					}
					this.setHeldItem(Utility.addItemToInventory(this.takeHeldItem(), i, this.inventory.actualInventory));
					if (Game1.player.CurrentToolIndex == i && this.checkHeldItem())
					{
						Game1.player.CursorSlotItem.actionWhenStopBeingHeld(Game1.player);
					}
					Game1.playSound("stoneStep");
					return;
				}
			}
		}
		this.setHeldItem(this.inventory.leftClick(x, y, this.takeHeldItem(), !Game1.oldKBState.IsKeyDown(Keys.LeftShift)));
		if (this.checkHeldItem((Item item) => item?.QualifiedItemId == "(O)434"))
		{
			Game1.playSound("smallSelect");
			Game1.player.eatObject(this.takeHeldItem() as Object, overrideFullness: true);
			Game1.exitActiveMenu();
		}
		else if (this.checkHeldItem() && Game1.oldKBState.IsKeyDown(Keys.LeftShift))
		{
			if (this.checkHeldItem((Item item) => item is Ring))
			{
				if (Game1.player.leftRing.Value == null)
				{
					Game1.player.Equip(this.takeHeldItem() as Ring, Game1.player.leftRing);
					Game1.playSound("crit");
					return;
				}
				if (Game1.player.rightRing.Value == null)
				{
					Game1.player.Equip(this.takeHeldItem() as Ring, Game1.player.rightRing);
					Game1.playSound("crit");
					return;
				}
			}
			else if (this.checkHeldItem((Item item) => item is Hat))
			{
				if (Game1.player.hat.Value == null)
				{
					Game1.player.Equip(this.takeHeldItem() as Hat, Game1.player.hat);
					Game1.playSound("grassyStep");
					return;
				}
			}
			else if (this.checkHeldItem((Item item) => item is Boots))
			{
				if (Game1.player.boots.Value == null)
				{
					Game1.player.Equip(this.takeHeldItem() as Boots, Game1.player.boots);
					Game1.playSound("sandyStep");
					DelayedAction.playSoundAfterDelay("sandyStep", 150);
					return;
				}
			}
			else if (this.checkHeldItem((Item item) => item is Clothing clothing && clothing.clothesType.Value == Clothing.ClothesType.SHIRT))
			{
				if (Game1.player.shirtItem.Value == null)
				{
					Game1.player.Equip(this.takeHeldItem() as Clothing, Game1.player.shirtItem);
					Game1.playSound("sandyStep");
					DelayedAction.playSoundAfterDelay("sandyStep", 150);
					return;
				}
			}
			else if (this.checkHeldItem((Item item) => item is Clothing clothing && clothing.clothesType.Value == Clothing.ClothesType.PANTS))
			{
				if (Game1.player.pantsItem.Value == null)
				{
					Game1.player.Equip(this.takeHeldItem() as Clothing, Game1.player.pantsItem);
					Game1.playSound("sandyStep");
					DelayedAction.playSoundAfterDelay("sandyStep", 150);
					return;
				}
			}
			else if (this.checkHeldItem((Item item) => item is Trinket) && Game1.player.stats.Get("trinketSlots") != 0)
			{
				bool success = false;
				for (int i2 = 0; i2 < Game1.player.trinketItems.Count; i2++)
				{
					if (Game1.player.trinketItems[i2] == null)
					{
						Game1.player.trinketItems[i2] = this.takeHeldItem() as Trinket;
						success = true;
						break;
					}
				}
				if (Game1.player.trinketItems.Count < Farmer.MaximumTrinkets)
				{
					Game1.player.trinketItems.Add(this.takeHeldItem() as Trinket);
					success = true;
				}
				if (success)
				{
					Game1.playSound("clank");
					return;
				}
			}
			if (this.inventory.getInventoryPositionOfClick(x, y) >= 12)
			{
				int i3;
				for (i3 = 0; i3 < 12; i3++)
				{
					if (Game1.player.Items[i3] == null || this.checkHeldItem((Item item) => Game1.player.Items[i3].canStackWith(item)))
					{
						if (Game1.player.CurrentToolIndex == i3 && this.checkHeldItem())
						{
							Game1.player.CursorSlotItem.actionWhenBeingHeld(Game1.player);
						}
						this.setHeldItem(Utility.addItemToInventory(this.takeHeldItem(), i3, this.inventory.actualInventory));
						if (this.checkHeldItem())
						{
							Game1.player.CursorSlotItem.actionWhenStopBeingHeld(Game1.player);
						}
						Game1.playSound("stoneStep");
						return;
					}
				}
			}
			else if (this.inventory.getInventoryPositionOfClick(x, y) < 12)
			{
				int i4;
				for (i4 = 12; i4 < Game1.player.Items.Count; i4++)
				{
					if (Game1.player.Items[i4] == null || this.checkHeldItem((Item item) => Game1.player.Items[i4].canStackWith(item)))
					{
						if (Game1.player.CurrentToolIndex == i4 && this.checkHeldItem())
						{
							Game1.player.CursorSlotItem.actionWhenBeingHeld(Game1.player);
						}
						this.setHeldItem(Utility.addItemToInventory(this.takeHeldItem(), i4, this.inventory.actualInventory));
						if (this.checkHeldItem())
						{
							Game1.player.CursorSlotItem.actionWhenStopBeingHeld(Game1.player);
						}
						Game1.playSound("stoneStep");
						return;
					}
				}
			}
		}
		if (this.portrait.containsPoint(x, y))
		{
			this.portrait.name = (this.portrait.name.Equals("32") ? "8" : "32");
		}
		if (this.trashCan.containsPoint(x, y) && this.checkHeldItem((Item item) => item?.canBeTrashed() ?? false))
		{
			Utility.trashItem(this.takeHeldItem());
			if (Game1.options.SnappyMenus)
			{
				this.snapCursorToCurrentSnappedComponent();
			}
		}
		else if (!this.isWithinBounds(x, y) && this.checkHeldItem((Item item) => item?.canBeTrashed() ?? false))
		{
			Game1.playSound("throwDownITem");
			Game1.createItemDebris(this.takeHeldItem(), Game1.player.getStandingPosition(), Game1.player.FacingDirection).DroppedByPlayerID.Value = Game1.player.UniqueMultiplayerID;
		}
		if (this.organizeButton != null && this.organizeButton.containsPoint(x, y))
		{
			ItemGrabMenu.organizeItemsInList(Game1.player.Items);
			Game1.playSound("Ship");
		}
		if (this.junimoNoteIcon != null && this.junimoNoteIcon.containsPoint(x, y) && this.readyToClose())
		{
			Game1.activeClickableMenu = new JunimoNoteMenu(fromGameMenu: true)
			{
				gameMenuTabToReturnTo = GameMenu.inventoryTab
			};
		}
	}

	/// <inheritdoc />
	public override void receiveGamePadButton(Buttons button)
	{
		if (button == Buttons.Back && this.organizeButton != null)
		{
			ItemGrabMenu.organizeItemsInList(Game1.player.Items);
			Game1.playSound("Ship");
		}
	}

	/// <inheritdoc />
	public override void receiveRightClick(int x, int y, bool playSound = true)
	{
		this.setHeldItem(this.inventory.rightClick(x, y, this.takeHeldItem()));
	}

	/// <inheritdoc />
	public override void performHoverAction(int x, int y)
	{
		this.hoverAmount = -1;
		this.hoveredItem = this.inventory.hover(x, y, Game1.player.CursorSlotItem);
		this.hoverText = this.inventory.hoverText;
		this.hoverTitle = this.inventory.hoverTitle;
		foreach (ClickableComponent c in this.equipmentIcons)
		{
			if (c.containsPoint(x, y))
			{
				switch (c.name)
				{
				case "Hat":
					if (Game1.player.hat.Value != null)
					{
						this.hoveredItem = Game1.player.hat.Value;
						this.hoverText = Game1.player.hat.Value.getDescription();
						this.hoverTitle = Game1.player.hat.Value.DisplayName;
					}
					break;
				case "Right Ring":
					if (Game1.player.rightRing.Value != null)
					{
						this.hoveredItem = Game1.player.rightRing.Value;
						this.hoverText = Game1.player.rightRing.Value.getDescription();
						this.hoverTitle = Game1.player.rightRing.Value.DisplayName;
					}
					break;
				case "Left Ring":
					if (Game1.player.leftRing.Value != null)
					{
						this.hoveredItem = Game1.player.leftRing.Value;
						this.hoverText = Game1.player.leftRing.Value.getDescription();
						this.hoverTitle = Game1.player.leftRing.Value.DisplayName;
					}
					break;
				case "Boots":
					if (Game1.player.boots.Value != null)
					{
						this.hoveredItem = Game1.player.boots.Value;
						this.hoverText = Game1.player.boots.Value.getDescription();
						this.hoverTitle = Game1.player.boots.Value.DisplayName;
					}
					break;
				case "Shirt":
					if (Game1.player.shirtItem.Value != null)
					{
						this.hoveredItem = Game1.player.shirtItem.Value;
						this.hoverText = Game1.player.shirtItem.Value.getDescription();
						this.hoverTitle = Game1.player.shirtItem.Value.DisplayName;
					}
					break;
				case "Pants":
					if (Game1.player.pantsItem.Value != null)
					{
						this.hoveredItem = Game1.player.pantsItem.Value;
						this.hoverText = Game1.player.pantsItem.Value.getDescription();
						this.hoverTitle = Game1.player.pantsItem.Value.DisplayName;
					}
					break;
				case "Trinket":
					if (Game1.player.trinketItems.Count == 1 && Game1.player.trinketItems[0] != null)
					{
						this.hoveredItem = Game1.player.trinketItems[0];
						this.hoverText = Game1.player.trinketItems[0].getDescription();
						this.hoverTitle = Game1.player.trinketItems[0].DisplayName;
					}
					break;
				}
				c.scale = Math.Min(c.scale + 0.05f, 1.1f);
			}
			c.scale = Math.Max(1f, c.scale - 0.025f);
		}
		if (this.portrait.containsPoint(x, y))
		{
			this.portrait.scale += 0.2f;
			this.hoverText = Game1.content.LoadString("Strings\\UI:Inventory_PortraitHover_Level", Game1.player.Level) + Environment.NewLine + Game1.player.getTitle();
		}
		else
		{
			this.portrait.scale = 0f;
		}
		if (this.trashCan.containsPoint(x, y))
		{
			if (this.trashCanLidRotation <= 0f)
			{
				Game1.playSound("trashcanlid");
			}
			this.trashCanLidRotation = Math.Min(this.trashCanLidRotation + (float)Math.PI / 48f, (float)Math.PI / 2f);
			if (this.checkHeldItem() && Utility.getTrashReclamationPrice(Game1.player.CursorSlotItem, Game1.player) > 0)
			{
				this.hoverText = Game1.content.LoadString("Strings\\UI:TrashCanSale");
				this.hoverAmount = Utility.getTrashReclamationPrice(Game1.player.CursorSlotItem, Game1.player);
			}
		}
		else if (this.trashCanLidRotation != 0f)
		{
			this.trashCanLidRotation = Math.Max(this.trashCanLidRotation - (float)Math.PI / 24f, 0f);
			if (this.trashCanLidRotation == 0f)
			{
				Game1.playSound("thudStep");
			}
		}
		if (this.organizeButton != null)
		{
			this.organizeButton.tryHover(x, y);
			if (this.organizeButton.containsPoint(x, y))
			{
				this.hoverText = this.organizeButton.hoverText;
			}
		}
		if (this.junimoNoteIcon != null)
		{
			this.junimoNoteIcon.tryHover(x, y);
			if (this.junimoNoteIcon.containsPoint(x, y))
			{
				this.hoverText = this.junimoNoteIcon.hoverText;
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
	}

	public override void snapToDefaultClickableComponent()
	{
		base.currentlySnappedComponent = base.getComponentWithID(0);
		this.snapCursorToCurrentSnappedComponent();
	}

	public override bool readyToClose()
	{
		return !this.checkHeldItem();
	}

	/// <inheritdoc />
	public override void draw(SpriteBatch b)
	{
		base.drawHorizontalPartition(b, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 192);
		this.inventory.draw(b);
		foreach (ClickableComponent c in this.equipmentIcons)
		{
			switch (c.name)
			{
			case "Hat":
				if (Game1.player.hat.Value != null)
				{
					b.Draw(Game1.menuTexture, c.bounds, Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 10), Color.White);
					Game1.player.hat.Value.drawInMenu(b, new Vector2(c.bounds.X, c.bounds.Y), c.scale, 1f, 0.866f, StackDrawType.Hide);
				}
				else
				{
					b.Draw(Game1.menuTexture, c.bounds, Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 42), Color.White);
				}
				break;
			case "Right Ring":
				if (Game1.player.rightRing.Value != null)
				{
					b.Draw(Game1.menuTexture, c.bounds, Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 10), Color.White);
					Game1.player.rightRing.Value.drawInMenu(b, new Vector2(c.bounds.X, c.bounds.Y), c.scale);
				}
				else
				{
					b.Draw(Game1.menuTexture, c.bounds, Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 41), Color.White);
				}
				break;
			case "Left Ring":
				if (Game1.player.leftRing.Value != null)
				{
					b.Draw(Game1.menuTexture, c.bounds, Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 10), Color.White);
					Game1.player.leftRing.Value.drawInMenu(b, new Vector2(c.bounds.X, c.bounds.Y), c.scale);
				}
				else
				{
					b.Draw(Game1.menuTexture, c.bounds, Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 41), Color.White);
				}
				break;
			case "Boots":
				if (Game1.player.boots.Value != null)
				{
					b.Draw(Game1.menuTexture, c.bounds, Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 10), Color.White);
					Game1.player.boots.Value.drawInMenu(b, new Vector2(c.bounds.X, c.bounds.Y), c.scale);
				}
				else
				{
					b.Draw(Game1.menuTexture, c.bounds, Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 40), Color.White);
				}
				break;
			case "Shirt":
				if (Game1.player.shirtItem.Value != null)
				{
					b.Draw(Game1.menuTexture, c.bounds, Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 10), Color.White);
					Game1.player.shirtItem.Value.drawInMenu(b, new Vector2(c.bounds.X, c.bounds.Y), c.scale);
				}
				else
				{
					b.Draw(Game1.menuTexture, c.bounds, Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 69), Color.White);
				}
				break;
			case "Pants":
				if (Game1.player.pantsItem.Value != null)
				{
					b.Draw(Game1.menuTexture, c.bounds, Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 10), Color.White);
					Game1.player.pantsItem.Value.drawInMenu(b, new Vector2(c.bounds.X, c.bounds.Y), c.scale);
				}
				else
				{
					b.Draw(Game1.menuTexture, c.bounds, Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 68), Color.White);
				}
				break;
			case "Trinket":
			{
				int trinket_index = c.myID - 120;
				if (Game1.player.trinketItems.Count > trinket_index && Game1.player.trinketItems[trinket_index] != null)
				{
					b.Draw(Game1.menuTexture, c.bounds, Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 10), Color.White);
					Game1.player.trinketItems[trinket_index].drawInMenu(b, new Vector2(c.bounds.X, c.bounds.Y), c.scale);
				}
				else
				{
					b.Draw(Game1.menuTexture, c.bounds, Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, 70), Color.White);
				}
				break;
			}
			}
		}
		b.Draw((Game1.timeOfDay >= 1900) ? Game1.nightbg : Game1.daybg, new Vector2(base.xPositionOnScreen + 192 - 64 - 8, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 256 - 8), Color.White);
		FarmerRenderer.isDrawingForUI = true;
		Game1.player.FarmerRenderer.draw(b, new FarmerSprite.AnimationFrame(0, Game1.player.bathingClothes.Value ? 108 : 0, secondaryArm: false, flip: false), Game1.player.bathingClothes.Value ? 108 : 0, new Rectangle(0, Game1.player.bathingClothes.Value ? 576 : 0, 16, 32), new Vector2(base.xPositionOnScreen + 192 - 8 - 32, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 320 - 32 - 8), Vector2.Zero, 0.8f, 2, Color.White, 0f, 1f, Game1.player);
		if (Game1.timeOfDay >= 1900)
		{
			Game1.player.FarmerRenderer.draw(b, new FarmerSprite.AnimationFrame(0, Game1.player.bathingClothes.Value ? 108 : 0, secondaryArm: false, flip: false), Game1.player.bathingClothes.Value ? 108 : 0, new Rectangle(0, Game1.player.bathingClothes.Value ? 576 : 0, 16, 32), new Vector2(base.xPositionOnScreen + 192 - 8 - 32, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 320 - 32 - 8), Vector2.Zero, 0.8f, 2, Color.DarkBlue * 0.3f, 0f, 1f, Game1.player);
		}
		FarmerRenderer.isDrawingForUI = false;
		Utility.drawTextWithShadow(b, Game1.player.Name, Game1.dialogueFont, new Vector2((float)(base.xPositionOnScreen + 192 - 8) - Game1.dialogueFont.MeasureString(Game1.player.Name).X / 2f, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 448 + 8), Game1.textColor);
		float offset = 32f;
		string farmName = Game1.content.LoadString("Strings\\UI:Inventory_FarmName", Game1.player.farmName);
		Utility.drawTextWithShadow(b, farmName, Game1.dialogueFont, new Vector2((float)base.xPositionOnScreen + offset + 512f + 32f - Game1.dialogueFont.MeasureString(farmName).X / 2f, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 256 + 4), Game1.textColor);
		string currentFunds = Game1.content.LoadString("Strings\\UI:Inventory_CurrentFunds" + (Game1.player.useSeparateWallets ? "_Separate" : ""), Utility.getNumberWithCommas(Game1.player.Money));
		Utility.drawTextWithShadow(b, currentFunds, Game1.dialogueFont, new Vector2((float)base.xPositionOnScreen + offset + 512f + 32f - Game1.dialogueFont.MeasureString(currentFunds).X / 2f, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 320 + 4), Game1.textColor);
		string totalEarnings = Game1.content.LoadString("Strings\\UI:Inventory_TotalEarnings" + (Game1.player.useSeparateWallets ? "_Separate" : ""), Utility.getNumberWithCommas((int)Game1.player.totalMoneyEarned));
		Utility.drawTextWithShadow(b, totalEarnings, Game1.dialogueFont, new Vector2((float)base.xPositionOnScreen + offset + 512f + 32f - Game1.dialogueFont.MeasureString(totalEarnings).X / 2f, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 384), Game1.textColor);
		Utility.drawTextWithShadow(b, Utility.getDateString(), Game1.dialogueFont, new Vector2((float)base.xPositionOnScreen + offset + 512f + 32f - Game1.dialogueFont.MeasureString(Utility.getDateString()).X / 2f, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 448), Game1.textColor * 0.8f);
		this.organizeButton?.draw(b);
		this.trashCan.draw(b);
		b.Draw(Game1.mouseCursors, new Vector2(this.trashCan.bounds.X + 60, this.trashCan.bounds.Y + 40), new Rectangle(564 + Game1.player.trashCanLevel * 18, 129, 18, 10), Color.White, this.trashCanLidRotation, new Vector2(16f, 10f), 4f, SpriteEffects.None, 0.86f);
		if (this.checkHeldItem())
		{
			Game1.player.CursorSlotItem.drawInMenu(b, new Vector2(Game1.getOldMouseX() + 16, Game1.getOldMouseY() + 16), 1f);
		}
		if (!string.IsNullOrEmpty(this.hoverText))
		{
			if (this.hoverAmount > 0)
			{
				IClickableMenu.drawToolTip(b, this.hoverText, this.hoverTitle, null, heldItem: true, -1, 0, null, -1, null, this.hoverAmount);
			}
			else
			{
				IClickableMenu.drawToolTip(b, this.hoverText, this.hoverTitle, this.hoveredItem, this.checkHeldItem());
			}
		}
		this.junimoNoteIcon?.draw(b);
	}

	public override void emergencyShutDown()
	{
		base.emergencyShutDown();
		this.setHeldItem(Game1.player.addItemToInventory(this.takeHeldItem()));
		if (this.checkHeldItem())
		{
			Game1.playSound("throwDownITem");
			Game1.createItemDebris(this.takeHeldItem(), Game1.player.getStandingPosition(), Game1.player.FacingDirection);
		}
	}
}
