using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley.Menus;

public class QuestContainerMenu : MenuWithInventory
{
	public enum ChangeType
	{
		None,
		Place,
		Grab
	}

	public InventoryMenu ItemsToGrabMenu;

	public Func<Item, int> stackCapacityCheck;

	public Action onItemChanged;

	public Action onConfirm;

	public QuestContainerMenu(IList<Item> inventory, int rows = 3, InventoryMenu.highlightThisItem highlight_method = null, Func<Item, int> stack_capacity_check = null, Action on_item_changed = null, Action on_confirm = null)
		: base(highlight_method, okButton: true)
	{
		this.onItemChanged = (Action)Delegate.Combine(this.onItemChanged, on_item_changed);
		this.onConfirm = (Action)Delegate.Combine(this.onConfirm, on_confirm);
		int capacity = inventory.Count;
		int containerWidth = 64 * (capacity / rows);
		this.ItemsToGrabMenu = new InventoryMenu(Game1.uiViewport.Width / 2 - containerWidth / 2, base.yPositionOnScreen + 64, playerInventory: false, inventory, null, capacity, rows);
		this.stackCapacityCheck = stack_capacity_check;
		for (int i = 0; i < this.ItemsToGrabMenu.actualInventory.Count; i++)
		{
			if (i >= this.ItemsToGrabMenu.actualInventory.Count - this.ItemsToGrabMenu.capacity / this.ItemsToGrabMenu.rows)
			{
				this.ItemsToGrabMenu.inventory[i].downNeighborID = i + 53910;
			}
		}
		for (int j = 0; j < base.inventory.inventory.Count; j++)
		{
			base.inventory.inventory[j].myID = j + 53910;
			if (base.inventory.inventory[j].downNeighborID != -1)
			{
				base.inventory.inventory[j].downNeighborID += 53910;
			}
			if (base.inventory.inventory[j].rightNeighborID != -1)
			{
				base.inventory.inventory[j].rightNeighborID += 53910;
			}
			if (base.inventory.inventory[j].leftNeighborID != -1)
			{
				base.inventory.inventory[j].leftNeighborID += 53910;
			}
			if (base.inventory.inventory[j].upNeighborID != -1)
			{
				base.inventory.inventory[j].upNeighborID += 53910;
			}
			if (j < 12)
			{
				base.inventory.inventory[j].upNeighborID = this.ItemsToGrabMenu.actualInventory.Count - this.ItemsToGrabMenu.capacity / this.ItemsToGrabMenu.rows;
			}
			foreach (ClickableComponent item in base.inventory.GetBorder(InventoryMenu.BorderSide.Right))
			{
				item.rightNeighborID = base.okButton.myID;
			}
		}
		base.dropItemInvisibleButton.myID = -500;
		this.ItemsToGrabMenu.dropItemInvisibleButton.myID = -500;
		this.populateClickableComponentList();
		if (Game1.options.SnappyMenus)
		{
			this.setCurrentlySnappedComponentTo(53910);
			this.snapCursorToCurrentSnappedComponent();
		}
	}

	public virtual int GetDonatableAmount(Item item)
	{
		if (item == null)
		{
			return 0;
		}
		int stack_capacity = item.Stack;
		if (this.stackCapacityCheck != null)
		{
			stack_capacity = Math.Min(stack_capacity, this.stackCapacityCheck(item));
		}
		return stack_capacity;
	}

	public virtual Item TryToGrab(Item item, int amount)
	{
		int grabbed_amount = Math.Min(amount, item.Stack);
		if (grabbed_amount == 0)
		{
			return item;
		}
		Item taken_stack = item.getOne();
		taken_stack.Stack = grabbed_amount;
		item.Stack -= grabbed_amount;
		InventoryMenu.highlightThisItem highlight_method = base.inventory.highlightMethod;
		base.inventory.highlightMethod = InventoryMenu.highlightAllItems;
		Item leftover_items = base.inventory.tryToAddItem(taken_stack);
		base.inventory.highlightMethod = highlight_method;
		if (leftover_items != null)
		{
			item.Stack += leftover_items.Stack;
		}
		this.onItemChanged?.Invoke();
		if (item.Stack <= 0)
		{
			return null;
		}
		return item;
	}

	public virtual Item TryToPlace(Item item, int amount)
	{
		int stack_capacity = Math.Min(amount, this.GetDonatableAmount(item));
		if (stack_capacity == 0)
		{
			return item;
		}
		Item donation_stack = item.getOne();
		donation_stack.Stack = stack_capacity;
		item.Stack -= stack_capacity;
		Item leftover_items = this.ItemsToGrabMenu.tryToAddItem(donation_stack, "Ship");
		if (leftover_items != null)
		{
			item.Stack += leftover_items.Stack;
		}
		this.onItemChanged?.Invoke();
		if (item.Stack <= 0)
		{
			return null;
		}
		return item;
	}

	/// <inheritdoc />
	public override void receiveLeftClick(int x, int y, bool playSound = true)
	{
		if (base.isWithinBounds(x, y))
		{
			Item clicked_item = base.inventory.getItemAt(x, y);
			if (clicked_item != null)
			{
				int clicked_index = base.inventory.getInventoryPositionOfClick(x, y);
				base.inventory.actualInventory[clicked_index] = this.TryToPlace(clicked_item, clicked_item.Stack);
			}
		}
		if (this.ItemsToGrabMenu.isWithinBounds(x, y))
		{
			Item clicked_item2 = this.ItemsToGrabMenu.getItemAt(x, y);
			if (clicked_item2 != null)
			{
				int clicked_index2 = this.ItemsToGrabMenu.getInventoryPositionOfClick(x, y);
				this.ItemsToGrabMenu.actualInventory[clicked_index2] = this.TryToGrab(clicked_item2, clicked_item2.Stack);
			}
		}
		if (base.okButton.containsPoint(x, y) && this.readyToClose())
		{
			base.exitThisMenu();
		}
	}

	/// <inheritdoc />
	public override void receiveRightClick(int x, int y, bool playSound = true)
	{
		if (base.isWithinBounds(x, y))
		{
			Item clicked_item = base.inventory.getItemAt(x, y);
			if (clicked_item != null)
			{
				int clicked_index = base.inventory.getInventoryPositionOfClick(x, y);
				base.inventory.actualInventory[clicked_index] = this.TryToPlace(clicked_item, 1);
			}
		}
		if (this.ItemsToGrabMenu.isWithinBounds(x, y))
		{
			Item clicked_item2 = this.ItemsToGrabMenu.getItemAt(x, y);
			if (clicked_item2 != null)
			{
				int clicked_index2 = this.ItemsToGrabMenu.getInventoryPositionOfClick(x, y);
				this.ItemsToGrabMenu.actualInventory[clicked_index2] = this.TryToGrab(clicked_item2, 1);
			}
		}
	}

	/// <inheritdoc />
	protected override void cleanupBeforeExit()
	{
		this.onConfirm?.Invoke();
		base.cleanupBeforeExit();
	}

	/// <inheritdoc />
	public override void performHoverAction(int x, int y)
	{
		base.performHoverAction(x, y);
		this.ItemsToGrabMenu.hover(x, y, base.heldItem);
	}

	/// <inheritdoc />
	public override void draw(SpriteBatch b)
	{
		b.Draw(Game1.fadeToBlackRect, new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), Color.Black * 0.5f);
		base.draw(b, drawUpperPortion: false, drawDescriptionArea: false);
		Game1.drawDialogueBox(this.ItemsToGrabMenu.xPositionOnScreen - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder, this.ItemsToGrabMenu.yPositionOnScreen - IClickableMenu.borderWidth - IClickableMenu.spaceToClearTopBorder, this.ItemsToGrabMenu.width + IClickableMenu.borderWidth * 2 + IClickableMenu.spaceToClearSideBorder * 2, this.ItemsToGrabMenu.height + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth * 2, speaker: false, drawOnlyBox: true);
		this.ItemsToGrabMenu.draw(b);
		if (!base.hoverText.Equals(""))
		{
			IClickableMenu.drawHoverText(b, base.hoverText, Game1.smallFont);
		}
		base.heldItem?.drawInMenu(b, new Vector2(Game1.getOldMouseX() + 16, Game1.getOldMouseY() + 16), 1f);
		base.drawMouse(b);
		string text = this.ItemsToGrabMenu.descriptionTitle;
		if (text != null && text.Length > 1)
		{
			IClickableMenu.drawHoverText(b, this.ItemsToGrabMenu.descriptionTitle, Game1.smallFont, 32 + ((base.heldItem != null) ? 16 : (-21)), 32 + ((base.heldItem != null) ? 16 : (-21)));
		}
	}
}
