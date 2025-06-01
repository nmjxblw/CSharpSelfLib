using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace StardewValley.Menus;

public class NumberSelectionMenu : IClickableMenu
{
	public delegate void behaviorOnNumberSelect(int number, int price, Farmer who);

	public const int region_leftButton = 101;

	public const int region_rightButton = 102;

	public const int region_okButton = 103;

	public const int region_cancelButton = 104;

	private string message;

	protected int price;

	protected int minValue;

	protected int maxValue;

	protected int currentValue;

	protected int priceShake;

	protected int heldTimer;

	private behaviorOnNumberSelect behaviorFunction;

	protected TextBox numberSelectedBox;

	public ClickableTextureComponent leftButton;

	public ClickableTextureComponent rightButton;

	public ClickableTextureComponent okButton;

	public ClickableTextureComponent cancelButton;

	protected virtual Vector2 centerPosition => new Vector2(Game1.uiViewport.Width / 2, Game1.uiViewport.Height / 2);

	public NumberSelectionMenu(string message, behaviorOnNumberSelect behaviorOnSelection, int price = -1, int minValue = 0, int maxValue = 99, int defaultNumber = 0)
	{
		Vector2 vector = Game1.dialogueFont.MeasureString(message);
		int menuWidth = Math.Max((int)vector.X, 600) + IClickableMenu.borderWidth * 2;
		int menuHeight = (int)vector.Y + IClickableMenu.borderWidth * 2 + 160;
		int menuX = (int)this.centerPosition.X - menuWidth / 2;
		int menuY = (int)this.centerPosition.Y - menuHeight / 2;
		base.initialize(menuX, menuY, menuWidth, menuHeight);
		this.message = message;
		this.price = price;
		this.minValue = minValue;
		this.maxValue = maxValue;
		this.currentValue = defaultNumber;
		this.behaviorFunction = behaviorOnSelection;
		this.numberSelectedBox = new TextBox(Game1.content.Load<Texture2D>("LooseSprites\\textBox"), null, Game1.smallFont, Game1.textColor)
		{
			X = base.xPositionOnScreen + IClickableMenu.borderWidth + 56,
			Y = base.yPositionOnScreen + IClickableMenu.borderWidth + base.height / 2,
			Text = (this.currentValue.ToString() ?? ""),
			numbersOnly = true,
			textLimit = (maxValue.ToString() ?? "").Length
		};
		this.numberSelectedBox.SelectMe();
		this.leftButton = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen + IClickableMenu.borderWidth, base.yPositionOnScreen + IClickableMenu.borderWidth + base.height / 2, 48, 44), Game1.mouseCursors, new Rectangle(352, 495, 12, 11), 4f)
		{
			myID = 101,
			rightNeighborID = 102,
			upNeighborID = -99998
		};
		this.rightButton = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen + IClickableMenu.borderWidth + 64 + this.numberSelectedBox.Width, base.yPositionOnScreen + IClickableMenu.borderWidth + base.height / 2, 48, 44), Game1.mouseCursors, new Rectangle(365, 495, 12, 11), 4f)
		{
			myID = 102,
			leftNeighborID = 101,
			rightNeighborID = 103,
			upNeighborID = -99998
		};
		this.okButton = new ClickableTextureComponent("OK", new Rectangle(base.xPositionOnScreen + base.width - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder - 128, base.yPositionOnScreen + base.height - IClickableMenu.borderWidth - IClickableMenu.spaceToClearTopBorder + 21, 64, 64), null, null, Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46), 1f)
		{
			myID = 103,
			leftNeighborID = 102,
			rightNeighborID = 104,
			upNeighborID = -99998
		};
		this.cancelButton = new ClickableTextureComponent("OK", new Rectangle(base.xPositionOnScreen + base.width - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder - 64, base.yPositionOnScreen + base.height - IClickableMenu.borderWidth - IClickableMenu.spaceToClearTopBorder + 21, 64, 64), null, null, Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 47), 1f)
		{
			myID = 104,
			leftNeighborID = 103,
			upNeighborID = -99998
		};
		if (Game1.options.SnappyMenus)
		{
			this.populateClickableComponentList();
			this.snapToDefaultClickableComponent();
		}
	}

	public override void snapToDefaultClickableComponent()
	{
		base.currentlySnappedComponent = base.getComponentWithID(102);
		this.snapCursorToCurrentSnappedComponent();
	}

	/// <inheritdoc />
	public override void gamePadButtonHeld(Buttons b)
	{
		base.gamePadButtonHeld(b);
		if (b != Buttons.A || base.currentlySnappedComponent == null)
		{
			return;
		}
		this.heldTimer += Game1.currentGameTime.ElapsedGameTime.Milliseconds;
		if (this.heldTimer <= 300)
		{
			return;
		}
		int step_size = (int)Math.Pow(10.0, (this.heldTimer - 300) / 3000);
		switch (base.currentlySnappedComponent.myID)
		{
		case 102:
		{
			int tempNumber2 = this.currentValue + step_size;
			int max_affordable = int.MaxValue;
			if (this.price != -1 && this.price != 0)
			{
				max_affordable = Game1.player.Money / this.price;
			}
			tempNumber2 = Math.Min(tempNumber2, Math.Min(this.maxValue, max_affordable));
			if (tempNumber2 != this.currentValue)
			{
				this.rightButton.scale = this.rightButton.baseScale;
				this.currentValue = tempNumber2;
				this.numberSelectedBox.Text = this.currentValue.ToString() ?? "";
			}
			break;
		}
		case 101:
		{
			int tempNumber = this.currentValue - step_size;
			tempNumber = Math.Max(tempNumber, this.minValue);
			if (tempNumber != this.currentValue)
			{
				this.leftButton.scale = this.leftButton.baseScale;
				this.currentValue = tempNumber;
				this.numberSelectedBox.Text = this.currentValue.ToString() ?? "";
			}
			break;
		}
		}
	}

	/// <inheritdoc />
	public override void receiveLeftClick(int x, int y, bool playSound = true)
	{
		if (this.leftButton.containsPoint(x, y))
		{
			int tempNumber = this.currentValue - 1;
			if (tempNumber >= this.minValue)
			{
				this.leftButton.scale = this.leftButton.baseScale;
				this.currentValue = tempNumber;
				this.numberSelectedBox.Text = this.currentValue.ToString() ?? "";
				Game1.playSound("smallSelect");
			}
		}
		if (this.rightButton.containsPoint(x, y))
		{
			int tempNumber2 = this.currentValue + 1;
			if (tempNumber2 <= this.maxValue && (this.price == -1 || tempNumber2 * this.price <= Game1.player.Money))
			{
				this.rightButton.scale = this.rightButton.baseScale;
				this.currentValue = tempNumber2;
				this.numberSelectedBox.Text = this.currentValue.ToString() ?? "";
				Game1.playSound("smallSelect");
			}
		}
		if (this.okButton.containsPoint(x, y))
		{
			if (this.currentValue > this.maxValue || this.currentValue < this.minValue)
			{
				this.currentValue = Math.Max(this.minValue, Math.Min(this.maxValue, this.currentValue));
				this.numberSelectedBox.Text = this.currentValue.ToString() ?? "";
			}
			else
			{
				this.behaviorFunction(this.currentValue, this.price, Game1.player);
			}
			Game1.playSound("smallSelect");
		}
		if (this.cancelButton.containsPoint(x, y))
		{
			Game1.exitActiveMenu();
			Game1.playSound("bigDeSelect");
			Game1.player.canMove = true;
		}
		this.numberSelectedBox.Update();
	}

	/// <inheritdoc />
	public override void receiveKeyPress(Keys key)
	{
		base.receiveKeyPress(key);
		if (key == Keys.Enter)
		{
			this.receiveLeftClick(this.okButton.bounds.Center.X, this.okButton.bounds.Center.Y);
		}
	}

	/// <inheritdoc />
	public override void update(GameTime time)
	{
		base.update(time);
		this.currentValue = 0;
		if (this.numberSelectedBox.Text != null)
		{
			int.TryParse(this.numberSelectedBox.Text, out this.currentValue);
		}
		if (this.priceShake > 0)
		{
			this.priceShake -= time.ElapsedGameTime.Milliseconds;
		}
		if (Game1.options.SnappyMenus && !Game1.oldPadState.IsButtonDown(Buttons.A))
		{
			this.heldTimer = 0;
		}
	}

	/// <inheritdoc />
	public override void performHoverAction(int x, int y)
	{
		if (this.okButton.containsPoint(x, y) && (this.price == -1 || this.currentValue > this.minValue))
		{
			this.okButton.scale = Math.Min(this.okButton.scale + 0.02f, this.okButton.baseScale + 0.2f);
		}
		else
		{
			this.okButton.scale = Math.Max(this.okButton.scale - 0.02f, this.okButton.baseScale);
		}
		if (this.cancelButton.containsPoint(x, y))
		{
			this.cancelButton.scale = Math.Min(this.cancelButton.scale + 0.02f, this.cancelButton.baseScale + 0.2f);
		}
		else
		{
			this.cancelButton.scale = Math.Max(this.cancelButton.scale - 0.02f, this.cancelButton.baseScale);
		}
		if (this.leftButton.containsPoint(x, y))
		{
			this.leftButton.scale = Math.Min(this.leftButton.scale + 0.02f, this.leftButton.baseScale + 0.2f);
		}
		else
		{
			this.leftButton.scale = Math.Max(this.leftButton.scale - 0.02f, this.leftButton.baseScale);
		}
		if (this.rightButton.containsPoint(x, y))
		{
			this.rightButton.scale = Math.Min(this.rightButton.scale + 0.02f, this.rightButton.baseScale + 0.2f);
		}
		else
		{
			this.rightButton.scale = Math.Max(this.rightButton.scale - 0.02f, this.rightButton.baseScale);
		}
	}

	/// <inheritdoc />
	public override void draw(SpriteBatch b)
	{
		b.Draw(Game1.fadeToBlackRect, new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), Color.Black * 0.5f);
		Game1.drawDialogueBox(base.xPositionOnScreen, base.yPositionOnScreen, base.width, base.height, speaker: false, drawOnlyBox: true);
		b.DrawString(Game1.dialogueFont, this.message, new Vector2(base.xPositionOnScreen + IClickableMenu.borderWidth, base.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth / 2), Game1.textColor);
		this.okButton.draw(b);
		this.cancelButton.draw(b);
		this.leftButton.draw(b);
		this.rightButton.draw(b);
		if (this.price != -1)
		{
			b.DrawString(Game1.dialogueFont, Game1.content.LoadString("Strings\\StringsFromCSFiles:LoadGameMenu.cs.11020", this.price * this.currentValue), new Vector2(this.rightButton.bounds.Right + 32 + ((this.priceShake > 0) ? Game1.random.Next(-1, 2) : 0), this.rightButton.bounds.Y + ((this.priceShake > 0) ? Game1.random.Next(-1, 2) : 0)), (this.currentValue * this.price > Game1.player.Money) ? Color.Red : Game1.textColor);
		}
		this.numberSelectedBox.Draw(b);
		base.drawMouse(b);
	}

	/// <inheritdoc />
	public override void receiveRightClick(int x, int y, bool playSound = true)
	{
	}
}
