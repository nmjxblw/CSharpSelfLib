using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace StardewValley.Menus;

public class StorageContainer : MenuWithInventory
{
	public delegate bool behaviorOnItemChange(Item i, int position, Item old, StorageContainer container, bool onRemoval = false);

	public InventoryMenu ItemsToGrabMenu;

	private TemporaryAnimatedSprite poof;

	private behaviorOnItemChange itemChangeBehavior;

	public StorageContainer(IList<Item> inventory, int capacity, int rows = 3, behaviorOnItemChange itemChangeBehavior = null, InventoryMenu.highlightThisItem highlightMethod = null)
		: base(highlightMethod, okButton: true, trashCan: true)
	{
		this.itemChangeBehavior = itemChangeBehavior;
		int containerWidth = 64 * (capacity / rows);
		this.ItemsToGrabMenu = new InventoryMenu(Game1.uiViewport.Width / 2 - containerWidth / 2, base.yPositionOnScreen + 64, playerInventory: false, inventory, null, capacity, rows);
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
		}
		base.dropItemInvisibleButton.myID = -500;
		this.ItemsToGrabMenu.dropItemInvisibleButton.myID = -500;
		if (Game1.options.SnappyMenus)
		{
			this.populateClickableComponentList();
			this.setCurrentlySnappedComponentTo(53910);
			this.snapCursorToCurrentSnappedComponent();
		}
	}

	/// <inheritdoc />
	public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
	{
		base.gameWindowSizeChanged(oldBounds, newBounds);
		int containerWidth = 64 * (this.ItemsToGrabMenu.capacity / this.ItemsToGrabMenu.rows);
		this.ItemsToGrabMenu = new InventoryMenu(Game1.uiViewport.Width / 2 - containerWidth / 2, base.yPositionOnScreen + 64, playerInventory: false, this.ItemsToGrabMenu.actualInventory, null, this.ItemsToGrabMenu.capacity, this.ItemsToGrabMenu.rows);
	}

	/// <inheritdoc />
	public override void receiveLeftClick(int x, int y, bool playSound = true)
	{
		Item old = base.heldItem;
		int oldStack = old?.Stack ?? (-1);
		if (base.isWithinBounds(x, y))
		{
			base.receiveLeftClick(x, y, playSound: false);
			if (this.itemChangeBehavior == null && old == null && base.heldItem != null && Game1.oldKBState.IsKeyDown(Keys.LeftShift))
			{
				base.heldItem = this.ItemsToGrabMenu.tryToAddItem(base.heldItem, "Ship");
			}
		}
		bool sound = true;
		if (this.ItemsToGrabMenu.isWithinBounds(x, y))
		{
			base.heldItem = this.ItemsToGrabMenu.leftClick(x, y, base.heldItem, playSound: false);
			if ((base.heldItem != null && old == null) || (base.heldItem != null && old != null && !base.heldItem.Equals(old)))
			{
				if (this.itemChangeBehavior != null)
				{
					sound = this.itemChangeBehavior(base.heldItem, this.ItemsToGrabMenu.getInventoryPositionOfClick(x, y), old, this, onRemoval: true);
				}
				if (sound)
				{
					Game1.playSound("dwop");
				}
			}
			if ((base.heldItem == null && old != null) || (base.heldItem != null && old != null && !base.heldItem.Equals(old)))
			{
				Item tmp = base.heldItem;
				if (base.heldItem == null && this.ItemsToGrabMenu.getItemAt(x, y) != null && oldStack < this.ItemsToGrabMenu.getItemAt(x, y).Stack)
				{
					tmp = old.getOne();
					tmp.Stack = oldStack;
				}
				if (this.itemChangeBehavior != null)
				{
					sound = this.itemChangeBehavior(old, this.ItemsToGrabMenu.getInventoryPositionOfClick(x, y), tmp, this);
				}
				if (sound)
				{
					Game1.playSound("Ship");
				}
			}
			Item item = base.heldItem;
			if (item != null && item.IsRecipe)
			{
				base.heldItem.LearnRecipe();
				this.poof = new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 320, 64, 64), 50f, 8, 0, new Vector2(x - x % 64 + 16, y - y % 64 + 16), flicker: false, flipped: false);
				Game1.playSound("newRecipe");
				base.heldItem = null;
			}
			else if (Game1.oldKBState.IsKeyDown(Keys.LeftShift) && Game1.player.addItemToInventoryBool(base.heldItem))
			{
				base.heldItem = null;
				if (this.itemChangeBehavior != null)
				{
					sound = this.itemChangeBehavior(base.heldItem, this.ItemsToGrabMenu.getInventoryPositionOfClick(x, y), old, this, onRemoval: true);
				}
				if (sound)
				{
					Game1.playSound("coin");
				}
			}
		}
		if (base.okButton.containsPoint(x, y) && this.readyToClose())
		{
			Game1.playSound("bigDeSelect");
			Game1.exitActiveMenu();
		}
		if (base.trashCan.containsPoint(x, y) && base.heldItem != null && base.heldItem.canBeTrashed())
		{
			Utility.trashItem(base.heldItem);
			base.heldItem = null;
		}
	}

	/// <inheritdoc />
	public override void receiveRightClick(int x, int y, bool playSound = true)
	{
		int oldStack = ((base.heldItem != null) ? base.heldItem.Stack : 0);
		Item old = base.heldItem;
		if (base.isWithinBounds(x, y))
		{
			base.receiveRightClick(x, y);
			if (this.itemChangeBehavior == null && old == null && base.heldItem != null && Game1.oldKBState.IsKeyDown(Keys.LeftShift))
			{
				base.heldItem = this.ItemsToGrabMenu.tryToAddItem(base.heldItem, "Ship");
			}
		}
		if (this.ItemsToGrabMenu.isWithinBounds(x, y))
		{
			base.heldItem = this.ItemsToGrabMenu.rightClick(x, y, base.heldItem, playSound: false);
			if ((base.heldItem != null && old == null) || (base.heldItem != null && old != null && !base.heldItem.Equals(old)) || (base.heldItem != null && old != null && base.heldItem.Equals(old) && base.heldItem.Stack != oldStack))
			{
				this.itemChangeBehavior?.Invoke(base.heldItem, this.ItemsToGrabMenu.getInventoryPositionOfClick(x, y), old, this, onRemoval: true);
				Game1.playSound("dwop");
			}
			if ((base.heldItem == null && old != null) || (base.heldItem != null && old != null && !base.heldItem.Equals(old)))
			{
				this.itemChangeBehavior?.Invoke(old, this.ItemsToGrabMenu.getInventoryPositionOfClick(x, y), base.heldItem, this);
				Game1.playSound("Ship");
			}
			Item item = base.heldItem;
			if (item != null && item.IsRecipe)
			{
				base.heldItem.LearnRecipe();
				this.poof = new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 320, 64, 64), 50f, 8, 0, new Vector2(x - x % 64 + 16, y - y % 64 + 16), flicker: false, flipped: false);
				Game1.playSound("newRecipe");
				base.heldItem = null;
			}
			else if (Game1.oldKBState.IsKeyDown(Keys.LeftShift) && Game1.player.addItemToInventoryBool(base.heldItem))
			{
				base.heldItem = null;
				Game1.playSound("coin");
				this.itemChangeBehavior?.Invoke(base.heldItem, this.ItemsToGrabMenu.getInventoryPositionOfClick(x, y), old, this, onRemoval: true);
			}
		}
	}

	/// <inheritdoc />
	public override void update(GameTime time)
	{
		base.update(time);
		if (this.poof != null && this.poof.update(time))
		{
			this.poof = null;
		}
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
		this.poof?.draw(b, localPosition: true);
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
