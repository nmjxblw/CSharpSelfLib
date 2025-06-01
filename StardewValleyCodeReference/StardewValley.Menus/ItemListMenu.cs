using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;

namespace StardewValley.Menus;

public class ItemListMenu : IClickableMenu
{
	public const int region_okbutton = 101;

	public const int region_forwardButton = 102;

	public const int region_backButton = 103;

	public int itemsPerCategoryPage = 8;

	public ClickableTextureComponent okButton;

	public ClickableTextureComponent forwardButton;

	public ClickableTextureComponent backButton;

	private List<Item> itemsToList;

	private string title;

	private int currentTab;

	private int totalValueOfItems;

	public ItemListMenu(string menuTitle, List<Item> itemList)
	{
		this.title = menuTitle;
		this.itemsToList = itemList;
		foreach (Item i in itemList)
		{
			this.totalValueOfItems += Utility.getSellToStorePriceOfItem(i);
		}
		this.itemsToList.Add(null);
		int centerX = Game1.uiViewport.Width / 2;
		int centerY = Game1.uiViewport.Height / 2;
		base.width = Math.Min(800, Game1.uiViewport.Width - 128);
		base.height = Math.Min(720, Game1.uiViewport.Height - 128);
		if (base.height <= 720)
		{
			this.itemsPerCategoryPage = 7;
		}
		base.xPositionOnScreen = centerX - base.width / 2;
		base.yPositionOnScreen = centerY - base.height / 2;
		Rectangle okRect = new Rectangle(centerX + base.width / 2 + 4, centerY + base.height / 2 - 96, 64, 64);
		this.okButton = new ClickableTextureComponent(Game1.content.LoadString("Strings\\StringsFromCSFiles:ShippingMenu.cs.11382"), okRect, null, Game1.content.LoadString("Strings\\StringsFromCSFiles:ShippingMenu.cs.11382"), Game1.mouseCursors, new Rectangle(128, 256, 64, 64), 1f)
		{
			myID = 101,
			leftNeighborID = -7777
		};
		if (Game1.options.gamepadControls)
		{
			Game1.setMousePositionRaw(okRect.Center.X, okRect.Center.Y);
		}
		this.backButton = new ClickableTextureComponent("", new Rectangle(base.xPositionOnScreen - 64, base.yPositionOnScreen + base.height - 64, 48, 44), null, "", Game1.mouseCursors, new Rectangle(352, 495, 12, 11), 4f)
		{
			myID = 103,
			rightNeighborID = -7777
		};
		this.forwardButton = new ClickableTextureComponent("", new Rectangle(base.xPositionOnScreen + base.width - 32 - 48, base.yPositionOnScreen + base.height - 64, 48, 44), null, "", Game1.mouseCursors, new Rectangle(365, 495, 12, 11), 4f)
		{
			myID = 102,
			leftNeighborID = 103,
			rightNeighborID = 101
		};
		if (Game1.options.SnappyMenus)
		{
			this.populateClickableComponentList();
			this.snapToDefaultClickableComponent();
		}
	}

	public override void snapToDefaultClickableComponent()
	{
		base.currentlySnappedComponent = base.getComponentWithID(101);
		this.snapCursorToCurrentSnappedComponent();
	}

	protected override void customSnapBehavior(int direction, int oldRegion, int oldID)
	{
		if (oldID == 103 && direction == 1)
		{
			if (this.showForwardButton())
			{
				base.currentlySnappedComponent = base.getComponentWithID(102);
				this.snapCursorToCurrentSnappedComponent();
			}
			else
			{
				this.snapToDefaultClickableComponent();
			}
		}
		else if (oldID == 101 && direction == 3)
		{
			if (this.showForwardButton())
			{
				base.currentlySnappedComponent = base.getComponentWithID(102);
				this.snapCursorToCurrentSnappedComponent();
			}
			else if (this.showBackButton())
			{
				base.currentlySnappedComponent = base.getComponentWithID(103);
				this.snapCursorToCurrentSnappedComponent();
			}
		}
	}

	/// <inheritdoc />
	public override void receiveGamePadButton(Buttons button)
	{
		base.receiveGamePadButton(button);
		switch (button)
		{
		case Buttons.LeftTrigger:
			if (this.showBackButton())
			{
				this.currentTab--;
				Game1.playSound("shwip");
			}
			break;
		case Buttons.RightTrigger:
			if (this.showForwardButton())
			{
				this.currentTab++;
				Game1.playSound("shwip");
			}
			break;
		case Buttons.B:
			base.exitThisMenu();
			break;
		}
	}

	/// <inheritdoc />
	public override void performHoverAction(int x, int y)
	{
		base.performHoverAction(x, y);
		this.okButton.tryHover(x, y);
		this.backButton.tryHover(x, y);
		this.forwardButton.tryHover(x, y);
	}

	/// <inheritdoc />
	public override void receiveLeftClick(int x, int y, bool playSound = true)
	{
		base.receiveLeftClick(x, y, playSound);
		if (this.okButton.containsPoint(x, y))
		{
			base.exitThisMenu();
		}
		if (this.backButton.containsPoint(x, y))
		{
			if (this.currentTab != 0)
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

	/// <inheritdoc />
	protected override void cleanupBeforeExit()
	{
		if (Game1.CurrentEvent != null)
		{
			Game1.CurrentEvent.CurrentCommand++;
		}
	}

	/// <inheritdoc />
	public override void draw(SpriteBatch b)
	{
		IClickableMenu.drawTextureBox(b, base.xPositionOnScreen, base.yPositionOnScreen, base.width, base.height, Color.White);
		SpriteText.drawStringHorizontallyCenteredAt(b, this.title, base.xPositionOnScreen + base.width / 2, base.yPositionOnScreen + 32 + 12);
		Vector2 position = new Vector2(base.xPositionOnScreen + 32, base.yPositionOnScreen + 96 + 4);
		for (int i = this.currentTab * this.itemsPerCategoryPage; i < this.currentTab * this.itemsPerCategoryPage + this.itemsPerCategoryPage; i++)
		{
			if (this.itemsToList.Count <= i)
			{
				continue;
			}
			if (this.itemsToList[i] == null)
			{
				if (this.totalValueOfItems > 0)
				{
					SpriteText.drawString(b, Game1.content.LoadString("Strings\\UI:ItemList_ItemsLostValue", this.totalValueOfItems), (int)position.X + 64 + 12, (int)position.Y + 12);
				}
			}
			else
			{
				this.itemsToList[i].drawInMenu(b, position, 1f, 1f, 1f, StackDrawType.Draw_OneInclusive);
				SpriteText.drawString(b, this.itemsToList[i].DisplayName, (int)position.X + 64 + 12, (int)position.Y + 12);
				position.Y += 68f;
			}
		}
		if (this.showBackButton())
		{
			this.backButton.draw(b);
		}
		if (this.showForwardButton())
		{
			this.forwardButton.draw(b);
		}
		this.okButton.draw(b);
		Game1.mouseCursorTransparency = 1f;
		base.drawMouse(b);
	}

	public bool showBackButton()
	{
		return this.currentTab > 0;
	}

	public bool showForwardButton()
	{
		return this.itemsToList.Count > this.itemsPerCategoryPage * (this.currentTab + 1);
	}
}
