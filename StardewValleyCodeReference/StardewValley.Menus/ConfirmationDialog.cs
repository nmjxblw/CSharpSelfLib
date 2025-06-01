using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.Extensions;

namespace StardewValley.Menus;

public class ConfirmationDialog : IClickableMenu
{
	public delegate void behavior(Farmer who);

	public const int region_okButton = 101;

	public const int region_cancelButton = 102;

	protected string message;

	public ClickableTextureComponent okButton;

	public ClickableTextureComponent cancelButton;

	protected behavior onConfirm;

	protected behavior onCancel;

	private bool active = true;

	private int delayBeforeCancellable;

	public ConfirmationDialog(string message, behavior onConfirm, behavior onCancel = null)
		: base(Game1.uiViewport.Width / 2 - (int)Game1.dialogueFont.MeasureString(message).X / 2 - IClickableMenu.borderWidth, Game1.uiViewport.Height / 2 - (int)Game1.dialogueFont.MeasureString(message).Y / 2, (int)Game1.dialogueFont.MeasureString(message).X + IClickableMenu.borderWidth * 2, (int)Game1.dialogueFont.MeasureString(message).Y + IClickableMenu.borderWidth * 2 + 160)
	{
		if (onCancel == null)
		{
			onCancel = closeDialog;
		}
		else
		{
			this.onCancel = onCancel;
		}
		this.onConfirm = onConfirm;
		Rectangle titleSafeArea = Game1.graphics.GraphicsDevice.Viewport.GetTitleSafeArea();
		message = Game1.parseText(message, Game1.dialogueFont, Math.Min(titleSafeArea.Width - 64, base.width));
		this.message = message;
		this.okButton = new ClickableTextureComponent("OK", new Rectangle(base.xPositionOnScreen + base.width - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder - 128 - 4, base.yPositionOnScreen + base.height - IClickableMenu.borderWidth - IClickableMenu.spaceToClearTopBorder + 21, 64, 64), null, null, Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46), 1f)
		{
			myID = 101,
			rightNeighborID = 102
		};
		this.cancelButton = new ClickableTextureComponent("OK", new Rectangle(base.xPositionOnScreen + base.width - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder - 64, base.yPositionOnScreen + base.height - IClickableMenu.borderWidth - IClickableMenu.spaceToClearTopBorder + 21, 64, 64), null, null, Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 47), 1f)
		{
			myID = 102,
			leftNeighborID = 101
		};
		if (Game1.options.SnappyMenus)
		{
			this.populateClickableComponentList();
			this.snapToDefaultClickableComponent();
			this.delayBeforeCancellable = 300;
		}
	}

	/// <inheritdoc />
	public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
	{
		base.gameWindowSizeChanged(oldBounds, newBounds);
		this.okButton.setPosition(base.xPositionOnScreen + base.width - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder - 128 - 4, base.yPositionOnScreen + base.height - IClickableMenu.borderWidth - IClickableMenu.spaceToClearTopBorder + 21);
		this.cancelButton.setPosition(base.xPositionOnScreen + base.width - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder - 64, base.yPositionOnScreen + base.height - IClickableMenu.borderWidth - IClickableMenu.spaceToClearTopBorder + 21);
	}

	public virtual void closeDialog(Farmer who)
	{
		if (Game1.activeClickableMenu is TitleMenu titleMenu)
		{
			titleMenu.backButtonPressed();
		}
		else
		{
			Game1.exitActiveMenu();
		}
	}

	public override void snapToDefaultClickableComponent()
	{
		base.currentlySnappedComponent = base.getComponentWithID(102);
		this.snapCursorToCurrentSnappedComponent();
	}

	public void confirm()
	{
		if (this.active)
		{
			this.active = false;
			this.onConfirm?.Invoke(Game1.player);
			Game1.playSound("smallSelect");
		}
	}

	public void cancel()
	{
		if (this.onCancel != null)
		{
			this.onCancel(Game1.player);
		}
		else
		{
			this.closeDialog(Game1.player);
		}
		Game1.playSound("bigDeSelect");
	}

	/// <inheritdoc />
	public override void receiveLeftClick(int x, int y, bool playSound = true)
	{
		if (this.active)
		{
			if (this.okButton.containsPoint(x, y))
			{
				this.confirm();
			}
			if (this.cancelButton.containsPoint(x, y) && this.delayBeforeCancellable <= 0)
			{
				this.cancel();
			}
		}
	}

	/// <inheritdoc />
	public override void receiveKeyPress(Keys key)
	{
		base.receiveKeyPress(key);
		if (this.active && Game1.activeClickableMenu == null && this.onCancel != null)
		{
			this.onCancel(Game1.player);
		}
		if (this.active)
		{
			switch (key)
			{
			case Keys.Y:
				this.confirm();
				break;
			case Keys.N:
				this.cancel();
				break;
			}
		}
	}

	public override void update(GameTime time)
	{
		base.update(time);
		if (this.delayBeforeCancellable > 0)
		{
			this.delayBeforeCancellable -= (int)time.ElapsedGameTime.TotalMilliseconds;
		}
	}

	/// <inheritdoc />
	public override void performHoverAction(int x, int y)
	{
		if (this.okButton.containsPoint(x, y))
		{
			this.okButton.scale = Math.Min(this.okButton.scale + 0.02f, this.okButton.baseScale + 0.2f);
		}
		else
		{
			this.okButton.scale = Math.Max(this.okButton.scale - 0.02f, this.okButton.baseScale);
		}
		if (this.cancelButton.containsPoint(x, y))
		{
			this.cancelButton.scale = ((this.cancelButton.baseScale == 1f) ? Math.Min(this.cancelButton.scale + 0.02f, this.cancelButton.baseScale + 0.2f) : Math.Min(this.cancelButton.scale + 0.1f, this.cancelButton.baseScale + 0.75f));
		}
		else
		{
			this.cancelButton.scale = ((this.cancelButton.baseScale == 1f) ? Math.Max(this.cancelButton.scale - 0.02f, this.cancelButton.baseScale) : Math.Max(this.cancelButton.scale - 0.1f, this.cancelButton.baseScale));
		}
	}

	/// <inheritdoc />
	public override void draw(SpriteBatch b)
	{
		if (this.active)
		{
			b.Draw(Game1.fadeToBlackRect, new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), Color.Black * 0.5f);
			Game1.drawDialogueBox(base.xPositionOnScreen, base.yPositionOnScreen, base.width, base.height, speaker: false, drawOnlyBox: true);
			b.DrawString(Game1.dialogueFont, this.message, new Vector2(base.xPositionOnScreen + IClickableMenu.borderWidth, base.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth / 2), Game1.textColor);
			this.okButton.draw(b);
			this.cancelButton.draw(b);
			base.drawMouse(b);
		}
	}
}
