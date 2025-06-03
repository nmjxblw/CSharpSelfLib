using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Locations;

namespace StardewValley.Menus;

public class MenuWithInventory : IClickableMenu
{
	public const int region_okButton = 4857;

	public const int region_trashCan = 5948;

	private Item _heldItem;

	public string descriptionText = "";

	public string hoverText = "";

	public string descriptionTitle = "";

	public InventoryMenu inventory;

	public Item hoveredItem;

	public int wiggleWordsTimer;

	public int hoverAmount;

	public ClickableTextureComponent okButton;

	public ClickableTextureComponent trashCan;

	public float trashCanLidRotation;

	public ClickableComponent dropItemInvisibleButton;

	/// <summary>What to do with the <see cref="P:StardewValley.Menus.MenuWithInventory.heldItem" /> if the menu is closed before it can be put down.</summary>
	public ItemExitBehavior HeldItemExitBehavior;

	/// <summary>Whether to allow exiting the menu while the player has a held item on their cursor. The <see cref="F:StardewValley.Menus.MenuWithInventory.HeldItemExitBehavior" /> will be applied.</summary>
	public bool AllowExitWithHeldItem;

	public Item heldItem
	{
		get
		{
			return this._heldItem;
		}
		set
		{
			value?.onDetachedFromParent();
			this._heldItem = value;
		}
	}

	public MenuWithInventory(InventoryMenu.highlightThisItem highlighterMethod = null, bool okButton = false, bool trashCan = false, int inventoryXOffset = 0, int inventoryYOffset = 0, int menuOffsetHack = 0, ItemExitBehavior heldItemExitBehavior = ItemExitBehavior.ReturnToPlayer, bool allowExitWithHeldItem = false)
		: base(Game1.uiViewport.Width / 2 - (800 + IClickableMenu.borderWidth * 2) / 2, Game1.uiViewport.Height / 2 - (600 + IClickableMenu.borderWidth * 2) / 2 + menuOffsetHack, 800 + IClickableMenu.borderWidth * 2, 600 + IClickableMenu.borderWidth * 2)
	{
		if (base.yPositionOnScreen < IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder)
		{
			base.yPositionOnScreen = IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder;
		}
		if (base.xPositionOnScreen < 0)
		{
			base.xPositionOnScreen = 0;
		}
		int yPositionForInventory = base.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth + 192 - 16 + inventoryYOffset;
		this.inventory = new InventoryMenu(base.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth / 2 + inventoryXOffset, yPositionForInventory, playerInventory: false, null, highlighterMethod);
		this.HeldItemExitBehavior = heldItemExitBehavior;
		this.AllowExitWithHeldItem = allowExitWithHeldItem;
		if (okButton)
		{
			this.okButton = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen + base.width + 4, base.yPositionOnScreen + base.height - 192 - IClickableMenu.borderWidth, 64, 64), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46), 1f)
			{
				myID = 4857,
				upNeighborID = 5948,
				leftNeighborID = 12
			};
		}
		if (trashCan)
		{
			this.trashCan = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen + base.width + 4, base.yPositionOnScreen + base.height - 192 - 32 - IClickableMenu.borderWidth - 104, 64, 104), Game1.mouseCursors, new Rectangle(564 + Game1.player.trashCanLevel * 18, 102, 18, 26), 4f)
			{
				myID = 5948,
				downNeighborID = 4857,
				leftNeighborID = 12,
				upNeighborID = 106
			};
		}
		this.dropItemInvisibleButton = new ClickableComponent(new Rectangle(base.xPositionOnScreen - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder - 128, yPositionForInventory - 12, 64, 64), "")
		{
			myID = 107,
			rightNeighborID = 0
		};
	}

	public void movePosition(int dx, int dy)
	{
		base.xPositionOnScreen += dx;
		base.yPositionOnScreen += dy;
		this.inventory.movePosition(dx, dy);
		if (this.okButton != null)
		{
			this.okButton.bounds.X += dx;
			this.okButton.bounds.Y += dy;
		}
		if (this.trashCan != null)
		{
			this.trashCan.bounds.X += dx;
			this.trashCan.bounds.Y += dy;
		}
		if (this.dropItemInvisibleButton != null)
		{
			this.dropItemInvisibleButton.bounds.X += dx;
			this.dropItemInvisibleButton.bounds.Y += dy;
		}
	}

	public override bool readyToClose()
	{
		if (!this.AllowExitWithHeldItem)
		{
			return this.heldItem == null;
		}
		return true;
	}

	/// <inheritdoc />
	protected override void cleanupBeforeExit()
	{
		this.RescueHeldItemOnExit();
		base.cleanupBeforeExit();
	}

	public override void emergencyShutDown()
	{
		this.RescueHeldItemOnExit();
		base.emergencyShutDown();
	}

	/// <summary>Rescue the <see cref="P:StardewValley.Menus.MenuWithInventory.heldItem" /> if the menu is exiting.</summary>
	protected void RescueHeldItemOnExit()
	{
		if (this.heldItem != null)
		{
			switch (this.HeldItemExitBehavior)
			{
			case ItemExitBehavior.ReturnToPlayer:
				this.heldItem = Game1.player.addItemToInventory(this.heldItem);
				break;
			case ItemExitBehavior.ReturnToMenu:
				this.heldItem = this.inventory.tryToAddItem(this.heldItem);
				break;
			case ItemExitBehavior.Discard:
				this.heldItem = null;
				break;
			}
			this.DropHeldItem();
		}
	}

	public virtual void DropHeldItem()
	{
		if (this.heldItem != null)
		{
			Game1.playSound("throwDownITem");
			int drop_direction = Game1.player.FacingDirection;
			if (this is ItemGrabMenu grabMenu && grabMenu.context is LibraryMuseum)
			{
				drop_direction = 2;
			}
			Game1.createItemDebris(this.heldItem, Game1.player.getStandingPosition(), drop_direction);
			this.inventory.onAddItem?.Invoke(this.heldItem, Game1.player);
			this.heldItem = null;
		}
	}

	/// <inheritdoc />
	public override void receiveLeftClick(int x, int y, bool playSound = true)
	{
		this.heldItem = this.inventory.leftClick(x, y, this.heldItem, playSound);
		if (!this.isWithinBounds(x, y) && this.readyToClose() && this.trashCan != null)
		{
			this.trashCan.containsPoint(x, y);
		}
		if (this.okButton != null && this.okButton.containsPoint(x, y) && this.readyToClose())
		{
			base.exitThisMenu();
			Event currentEvent = Game1.currentLocation.currentEvent;
			if (currentEvent != null && currentEvent.CurrentCommand > 0)
			{
				Game1.currentLocation.currentEvent.CurrentCommand++;
			}
			Game1.playSound("bigDeSelect");
		}
		if (this.trashCan != null && this.trashCan.containsPoint(x, y) && this.heldItem != null && this.heldItem.canBeTrashed())
		{
			Utility.trashItem(this.heldItem);
			this.heldItem = null;
		}
	}

	/// <inheritdoc />
	public override void receiveRightClick(int x, int y, bool playSound = true)
	{
		this.heldItem = this.inventory.rightClick(x, y, this.heldItem, playSound);
	}

	public void receiveRightClickOnlyToolAttachments(int x, int y)
	{
		this.heldItem = this.inventory.rightClick(x, y, this.heldItem, playSound: true, onlyCheckToolAttachments: true);
	}

	/// <inheritdoc />
	public override void performHoverAction(int x, int y)
	{
		this.descriptionText = "";
		this.descriptionTitle = "";
		this.hoveredItem = this.inventory.hover(x, y, this.heldItem);
		this.hoverText = this.inventory.hoverText;
		this.hoverAmount = 0;
		if (this.okButton != null)
		{
			if (this.okButton.containsPoint(x, y))
			{
				this.okButton.scale = Math.Min(1.1f, this.okButton.scale + 0.05f);
			}
			else
			{
				this.okButton.scale = Math.Max(1f, this.okButton.scale - 0.05f);
			}
		}
		if (this.trashCan == null)
		{
			return;
		}
		if (this.trashCan.containsPoint(x, y))
		{
			if (this.trashCanLidRotation <= 0f)
			{
				Game1.playSound("trashcanlid");
			}
			this.trashCanLidRotation = Math.Min(this.trashCanLidRotation + (float)Math.PI / 48f, (float)Math.PI / 2f);
			if (this.heldItem != null && Utility.getTrashReclamationPrice(this.heldItem, Game1.player) > 0)
			{
				this.hoverText = Game1.content.LoadString("Strings\\UI:TrashCanSale");
				this.hoverAmount = Utility.getTrashReclamationPrice(this.heldItem, Game1.player);
			}
		}
		else
		{
			this.trashCanLidRotation = Math.Max(this.trashCanLidRotation - (float)Math.PI / 48f, 0f);
		}
	}

	/// <inheritdoc />
	public override void update(GameTime time)
	{
		if (this.wiggleWordsTimer > 0)
		{
			this.wiggleWordsTimer -= time.ElapsedGameTime.Milliseconds;
		}
	}

	public virtual void draw(SpriteBatch b, bool drawUpperPortion = true, bool drawDescriptionArea = true, int red = -1, int green = -1, int blue = -1)
	{
		if (this.trashCan != null)
		{
			this.trashCan.draw(b);
			b.Draw(Game1.mouseCursors, new Vector2(this.trashCan.bounds.X + 60, this.trashCan.bounds.Y + 40), new Rectangle(564 + Game1.player.trashCanLevel * 18, 129, 18, 10), Color.White, this.trashCanLidRotation, new Vector2(16f, 10f), 4f, SpriteEffects.None, 0.86f);
		}
		if (drawUpperPortion)
		{
			Game1.drawDialogueBox(base.xPositionOnScreen, base.yPositionOnScreen, base.width, base.height, speaker: false, drawOnlyBox: true, null, objectDialogueWithPortrait: false, ignoreTitleSafe: false, red, green, blue);
			base.drawHorizontalPartition(b, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 256, small: false, red, green, blue);
			if (drawDescriptionArea)
			{
				base.drawVerticalUpperIntersectingPartition(b, base.xPositionOnScreen + 576, 328, red, green, blue);
				if (!this.descriptionText.Equals(""))
				{
					int xPosition = base.xPositionOnScreen + 576 + 42 + ((this.wiggleWordsTimer > 0) ? Game1.random.Next(-2, 3) : 0);
					int yPosition = base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder - 32 + ((this.wiggleWordsTimer > 0) ? Game1.random.Next(-2, 3) : 0);
					int max_height = 320;
					float scale = 0f;
					string parsed_text;
					do
					{
						scale = ((scale != 0f) ? (scale - 0.1f) : 1f);
						parsed_text = Game1.parseText(this.descriptionText, Game1.smallFont, (int)(224f / scale));
					}
					while (Game1.smallFont.MeasureString(parsed_text).Y > (float)max_height / scale && scale > 0.5f);
					if (red == -1)
					{
						Utility.drawTextWithShadow(b, parsed_text, Game1.smallFont, new Vector2(xPosition, yPosition), Game1.textColor * 0.75f, scale);
					}
					else
					{
						Utility.drawTextWithColoredShadow(b, parsed_text, Game1.smallFont, new Vector2(xPosition, yPosition), Game1.textColor * 0.75f, Color.Black * 0.2f, scale);
					}
				}
			}
		}
		else
		{
			Game1.drawDialogueBox(base.xPositionOnScreen - IClickableMenu.borderWidth / 2, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 64, base.width, base.height - (IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder + 192), speaker: false, drawOnlyBox: true);
		}
		this.okButton?.draw(b);
		this.inventory.draw(b, red, green, blue);
	}

	/// <inheritdoc />
	public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
	{
		base.gameWindowSizeChanged(oldBounds, newBounds);
		if (base.yPositionOnScreen < IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder)
		{
			base.yPositionOnScreen = IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder;
		}
		if (base.xPositionOnScreen < 0)
		{
			base.xPositionOnScreen = 0;
		}
		int yPositionForInventory = base.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth + 192 - 16;
		string move_item_sound = this.inventory.moveItemSound;
		this.inventory = new InventoryMenu(base.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth / 2, yPositionForInventory, playerInventory: false, null, this.inventory.highlightMethod);
		this.inventory.moveItemSound = move_item_sound;
		if (this.okButton != null)
		{
			this.okButton = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen + base.width + 4, base.yPositionOnScreen + base.height - 192 - IClickableMenu.borderWidth, 64, 64), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46), 1f);
		}
		if (this.trashCan != null)
		{
			this.trashCan = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen + base.width + 4, base.yPositionOnScreen + base.height - 192 - 32 - IClickableMenu.borderWidth - 104, 64, 104), Game1.mouseCursors, new Rectangle(669, 261, 16, 26), 4f);
		}
	}

	/// <inheritdoc />
	/// <remarks>This method is not implemented and will throw an exception.</remarks>
	public override void draw(SpriteBatch b)
	{
		throw new NotImplementedException();
	}
}
