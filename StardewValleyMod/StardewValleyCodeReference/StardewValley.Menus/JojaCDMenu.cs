using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Locations;

namespace StardewValley.Menus;

public class JojaCDMenu : IClickableMenu
{
	public new const int width = 1280;

	public new const int height = 576;

	public const int buttonWidth = 147;

	public const int buttonHeight = 30;

	private Texture2D noteTexture;

	public List<ClickableComponent> checkboxes = new List<ClickableComponent>();

	private string hoverText;

	private bool boughtSomething;

	private int exitTimer = -1;

	public JojaCDMenu(Texture2D noteTexture)
		: base(Game1.uiViewport.Width / 2 - 640, Game1.uiViewport.Height / 2 - 288, 1280, 576, showUpperRightCloseButton: true)
	{
		Game1.player.forceCanMove();
		this.noteTexture = noteTexture;
		int x = base.xPositionOnScreen + 4;
		int y = base.yPositionOnScreen + 208;
		for (int i = 0; i < 5; i++)
		{
			this.checkboxes.Add(new ClickableComponent(new Rectangle(x, y, 588, 120), i.ToString() ?? "")
			{
				myID = i,
				rightNeighborID = ((i % 2 != 0 || i == 4) ? (-1) : (i + 1)),
				leftNeighborID = ((i % 2 == 0) ? (-1) : (i - 1)),
				downNeighborID = i + 2,
				upNeighborID = i - 2
			});
			x += 592;
			if (x > base.xPositionOnScreen + 1184)
			{
				x = base.xPositionOnScreen + 4;
				y += 120;
			}
		}
		if (Utility.doesAnyFarmerHaveOrWillReceiveMail("ccVault"))
		{
			this.checkboxes[0].name = "complete";
		}
		if (Utility.doesAnyFarmerHaveOrWillReceiveMail("ccBoilerRoom"))
		{
			this.checkboxes[1].name = "complete";
		}
		if (Utility.doesAnyFarmerHaveOrWillReceiveMail("ccCraftsRoom"))
		{
			this.checkboxes[2].name = "complete";
		}
		if (Utility.doesAnyFarmerHaveOrWillReceiveMail("ccPantry"))
		{
			this.checkboxes[3].name = "complete";
		}
		if (Utility.doesAnyFarmerHaveOrWillReceiveMail("ccFishTank"))
		{
			this.checkboxes[4].name = "complete";
		}
		base.exitFunction = onExitFunction;
		if (Game1.options.SnappyMenus)
		{
			this.populateClickableComponentList();
			this.snapToDefaultClickableComponent();
			Game1.mouseCursorTransparency = 1f;
		}
	}

	public override void snapToDefaultClickableComponent()
	{
		base.currentlySnappedComponent = base.getComponentWithID(0);
		this.snapCursorToCurrentSnappedComponent();
	}

	private void onExitFunction()
	{
		if (this.boughtSomething)
		{
			JojaMart.Morris.setNewDialogue("Data\\ExtraDialogue:Morris_JojaCDConfirm");
			Game1.drawDialogue(JojaMart.Morris);
		}
	}

	/// <inheritdoc />
	public override void receiveLeftClick(int x, int y, bool playSound = true)
	{
		if (this.exitTimer >= 0)
		{
			return;
		}
		base.receiveLeftClick(x, y);
		foreach (ClickableComponent b in this.checkboxes)
		{
			if (!b.containsPoint(x, y) || b.name.Equals("complete"))
			{
				continue;
			}
			int buttonNumber = Convert.ToInt32(b.name);
			int price = this.getPriceFromButtonNumber(buttonNumber);
			if (Game1.player.Money >= price)
			{
				Game1.player.Money -= price;
				Game1.playSound("reward");
				b.name = "complete";
				this.boughtSomething = true;
				switch (buttonNumber)
				{
				case 0:
					Game1.addMailForTomorrow("jojaVault", noLetter: true, sendToEveryone: true);
					Game1.addMailForTomorrow("ccVault", noLetter: true, sendToEveryone: true);
					break;
				case 1:
					Game1.addMailForTomorrow("jojaBoilerRoom", noLetter: true, sendToEveryone: true);
					Game1.addMailForTomorrow("ccBoilerRoom", noLetter: true, sendToEveryone: true);
					break;
				case 2:
					Game1.addMailForTomorrow("jojaCraftsRoom", noLetter: true, sendToEveryone: true);
					Game1.addMailForTomorrow("ccCraftsRoom", noLetter: true, sendToEveryone: true);
					break;
				case 3:
					Game1.addMailForTomorrow("jojaPantry", noLetter: true, sendToEveryone: true);
					Game1.addMailForTomorrow("ccPantry", noLetter: true, sendToEveryone: true);
					break;
				case 4:
					Game1.addMailForTomorrow("jojaFishTank", noLetter: true, sendToEveryone: true);
					Game1.addMailForTomorrow("ccFishTank", noLetter: true, sendToEveryone: true);
					break;
				}
				this.exitTimer = 1000;
			}
			else
			{
				Game1.dayTimeMoneyBox.moneyShakeTimer = 1000;
			}
		}
	}

	/// <inheritdoc />
	public override void update(GameTime time)
	{
		base.update(time);
		if (this.exitTimer >= 0)
		{
			this.exitTimer -= time.ElapsedGameTime.Milliseconds;
			if (this.exitTimer <= 0)
			{
				base.exitThisMenu();
			}
		}
		Game1.mouseCursorTransparency = 1f;
	}

	public int getPriceFromButtonNumber(int buttonNumber)
	{
		return buttonNumber switch
		{
			0 => 40000, 
			1 => 15000, 
			2 => 25000, 
			3 => 35000, 
			4 => 20000, 
			_ => -1, 
		};
	}

	public string getDescriptionFromButtonNumber(int buttonNumber)
	{
		return Game1.content.LoadString("Strings\\UI:JojaCDMenu_Hover" + buttonNumber);
	}

	/// <inheritdoc />
	public override void performHoverAction(int x, int y)
	{
		base.performHoverAction(x, y);
		this.hoverText = "";
		foreach (ClickableComponent b in this.checkboxes)
		{
			if (b.containsPoint(x, y))
			{
				this.hoverText = (b.name.Equals("complete") ? "" : Game1.parseText(this.getDescriptionFromButtonNumber(Convert.ToInt32(b.name)), Game1.dialogueFont, 384));
			}
		}
	}

	/// <inheritdoc />
	public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
	{
		base.gameWindowSizeChanged(oldBounds, newBounds);
		base.xPositionOnScreen = Game1.uiViewport.Width / 2 - 640;
		base.yPositionOnScreen = Game1.uiViewport.Height / 2 - 288;
		int x = base.xPositionOnScreen + 4;
		int y = base.yPositionOnScreen + 208;
		this.checkboxes.Clear();
		for (int i = 0; i < 5; i++)
		{
			this.checkboxes.Add(new ClickableComponent(new Rectangle(x, y, 588, 120), i.ToString() ?? ""));
			x += 592;
			if (x > base.xPositionOnScreen + 1184)
			{
				x = base.xPositionOnScreen + 4;
				y += 120;
			}
		}
	}

	/// <inheritdoc />
	public override void draw(SpriteBatch b)
	{
		if (!Game1.options.showClearBackgrounds)
		{
			b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
		}
		b.Draw(this.noteTexture, Utility.getTopLeftPositionForCenteringOnScreen(1280, 576), new Rectangle(0, 0, 320, 144), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.79f);
		base.draw(b);
		foreach (ClickableComponent c in this.checkboxes)
		{
			if (c.name.Equals("complete"))
			{
				b.Draw(this.noteTexture, new Vector2(c.bounds.Left + 16, c.bounds.Y + 16), new Rectangle(0, 144, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.8f);
			}
		}
		Game1.dayTimeMoneyBox.drawMoneyBox(b, Game1.uiViewport.Width - 300 - IClickableMenu.spaceToClearSideBorder * 2, 4);
		Game1.mouseCursorTransparency = 1f;
		base.drawMouse(b);
		if (!string.IsNullOrEmpty(this.hoverText))
		{
			IClickableMenu.drawHoverText(b, this.hoverText, Game1.dialogueFont);
		}
	}
}
