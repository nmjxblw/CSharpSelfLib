using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;

namespace StardewValley.Menus;

public class NamingMenu : IClickableMenu
{
	public delegate void doneNamingBehavior(string s);

	public const int region_okButton = 101;

	public const int region_doneNamingButton = 102;

	public const int region_randomButton = 103;

	public const int region_namingBox = 104;

	public ClickableTextureComponent doneNamingButton;

	public ClickableTextureComponent randomButton;

	public TextBox textBox;

	public ClickableComponent textBoxCC;

	public doneNamingBehavior doneNaming;

	public string title;

	public int minLength = 1;

	/// <summary>Whether to apply filtering to the text input.</summary>
	public bool FilterInput = true;

	public NamingMenu(doneNamingBehavior b, string title, string defaultName = null)
	{
		this.doneNaming = b;
		base.xPositionOnScreen = 0;
		base.yPositionOnScreen = 0;
		base.width = Game1.uiViewport.Width;
		base.height = Game1.uiViewport.Height;
		this.title = title;
		this.randomButton = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen + base.width + 51 + 64, Game1.uiViewport.Height / 2, 64, 64), Game1.mouseCursors, new Rectangle(381, 361, 10, 10), 4f);
		this.textBox = new TextBox(null, null, Game1.dialogueFont, Game1.textColor);
		this.textBox.X = Game1.uiViewport.Width / 2 - 192;
		this.textBox.Y = Game1.uiViewport.Height / 2;
		this.textBox.Width = 256;
		this.textBox.Height = 192;
		this.textBox.OnEnterPressed += textBoxEnter;
		Game1.keyboardDispatcher.Subscriber = this.textBox;
		this.textBox.Text = ((defaultName != null) ? defaultName : Dialogue.randomName());
		this.textBox.Selected = true;
		this.randomButton = new ClickableTextureComponent(new Rectangle(this.textBox.X + this.textBox.Width + 64 + 48 - 8, Game1.uiViewport.Height / 2 + 4, 64, 64), Game1.mouseCursors, new Rectangle(381, 361, 10, 10), 4f)
		{
			myID = 103,
			leftNeighborID = 102
		};
		this.doneNamingButton = new ClickableTextureComponent(new Rectangle(this.textBox.X + this.textBox.Width + 32 + 4, Game1.uiViewport.Height / 2 - 8, 64, 64), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46), 1f)
		{
			myID = 102,
			rightNeighborID = 103,
			leftNeighborID = 104
		};
		this.textBoxCC = new ClickableComponent(new Rectangle(this.textBox.X, this.textBox.Y, 192, 48), "")
		{
			myID = 104,
			rightNeighborID = 102
		};
		if (Game1.options.SnappyMenus)
		{
			this.populateClickableComponentList();
			this.snapToDefaultClickableComponent();
		}
	}

	public override void snapToDefaultClickableComponent()
	{
		base.currentlySnappedComponent = base.getComponentWithID(104);
		this.snapCursorToCurrentSnappedComponent();
	}

	public void textBoxEnter(TextBox sender)
	{
		if (sender.Text.Length >= this.minLength)
		{
			if (this.doneNaming != null)
			{
				string text = (this.FilterInput ? Utility.FilterDirtyWords(sender.Text) : sender.Text);
				this.doneNaming(text);
				this.textBox.Selected = false;
			}
			else
			{
				Game1.exitActiveMenu();
			}
		}
	}

	/// <inheritdoc />
	public override void receiveGamePadButton(Buttons button)
	{
		base.receiveGamePadButton(button);
		if (this.textBox.Selected)
		{
			switch (button)
			{
			case Buttons.DPadUp:
			case Buttons.DPadDown:
			case Buttons.DPadLeft:
			case Buttons.DPadRight:
			case Buttons.LeftThumbstickLeft:
			case Buttons.LeftThumbstickUp:
			case Buttons.LeftThumbstickDown:
			case Buttons.LeftThumbstickRight:
				this.textBox.Selected = false;
				break;
			}
		}
	}

	/// <inheritdoc />
	public override void receiveKeyPress(Keys key)
	{
		if (!this.textBox.Selected && !Game1.options.doesInputListContain(Game1.options.menuButton, key))
		{
			base.receiveKeyPress(key);
		}
	}

	/// <inheritdoc />
	public override void performHoverAction(int x, int y)
	{
		base.performHoverAction(x, y);
		if (this.doneNamingButton != null)
		{
			if (this.doneNamingButton.containsPoint(x, y))
			{
				this.doneNamingButton.scale = Math.Min(1.1f, this.doneNamingButton.scale + 0.05f);
			}
			else
			{
				this.doneNamingButton.scale = Math.Max(1f, this.doneNamingButton.scale - 0.05f);
			}
		}
		this.randomButton.tryHover(x, y, 0.5f);
	}

	/// <inheritdoc />
	public override void receiveLeftClick(int x, int y, bool playSound = true)
	{
		base.receiveLeftClick(x, y, playSound);
		this.textBox.Update();
		if (this.doneNamingButton.containsPoint(x, y))
		{
			this.textBoxEnter(this.textBox);
			Game1.playSound("smallSelect");
		}
		else if (this.randomButton.containsPoint(x, y))
		{
			this.textBox.Text = Dialogue.randomName();
			this.randomButton.scale = this.randomButton.baseScale;
			Game1.playSound("drumkit6");
		}
	}

	/// <inheritdoc />
	public override void draw(SpriteBatch b)
	{
		base.draw(b);
		if (!Game1.options.showClearBackgrounds)
		{
			b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
		}
		SpriteText.drawStringWithScrollCenteredAt(b, this.title, Game1.uiViewport.Width / 2, Game1.uiViewport.Height / 2 - 128, this.title);
		this.textBox.Draw(b);
		this.doneNamingButton.draw(b);
		this.randomButton.draw(b);
		base.drawMouse(b);
	}
}
