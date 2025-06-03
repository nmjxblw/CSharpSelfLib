using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace StardewValley.Menus;

public class TextEntryMenu : IClickableMenu
{
	public const int borderSpace = 4;

	public const int buttonSize = 16;

	public const int windowWidth = 168;

	public const int windowHeight = 88;

	public string[][] letterMaps = new string[3][]
	{
		new string[4] { "1234567890", "qwertyuiop", "asdfghjkl'", "zxcvbnm,.?" },
		new string[4] { "!@#$%^&*()", "QWERTYUIOP", "ASDFGHJKL\"", "ZXCVBNM,.?" },
		new string[4] { "&%#|~$£~/\\", "-+=<>:;'\"`", "()[]{}.^°ñ", "áéíóúü¡!¿?" }
	};

	public List<ClickableTextureComponent> keys = new List<ClickableTextureComponent>();

	public ClickableTextureComponent backspaceButton;

	public ClickableTextureComponent spaceButton;

	public ClickableTextureComponent okButton;

	public ClickableTextureComponent upperCaseButton;

	public ClickableTextureComponent symbolsButton;

	protected int _lettersPerRow;

	protected TextBox _target;

	public int _currentKeyboard;

	/// <inheritdoc />
	public override void receiveGamePadButton(Buttons button)
	{
		switch (button)
		{
		case Buttons.Y:
			this.OnSpaceBar();
			break;
		case Buttons.X:
			this.OnBackSpace();
			break;
		case Buttons.B:
			this.Close();
			break;
		case Buttons.Start:
			this.OnSubmit();
			break;
		default:
			base.receiveGamePadButton(button);
			break;
		}
	}

	/// <inheritdoc />
	public override void receiveKeyPress(Keys key)
	{
		if (key == Keys.Delete)
		{
			this.Close();
		}
		base.receiveKeyPress(key);
	}

	public TextEntryMenu(TextBox target)
		: base((int)Utility.getTopLeftPositionForCenteringOnScreen(672, 352).X, (int)Utility.getTopLeftPositionForCenteringOnScreen(672, 352).Y, 672, 352)
	{
		this._target = target;
		this._lettersPerRow = this.letterMaps[0][0].Length;
		for (int i = 0; i < this.letterMaps[0].Length; i++)
		{
			for (int j = 0; j < this._lettersPerRow; j++)
			{
				ClickableTextureComponent key_component = new ClickableTextureComponent(new Rectangle(0, 0, 1024, 1024), Game1.mouseCursors2, new Rectangle(32, 176, 16, 16), 4f)
				{
					myID = i * this._lettersPerRow + j,
					leftNeighborID = -99998,
					rightNeighborID = -99998,
					upNeighborID = -99998,
					downNeighborID = -99998
				};
				if (i == this.letterMaps[0].Length - 1)
				{
					if (j >= 2 && j <= this._lettersPerRow - 4)
					{
						key_component.downNeighborID = 99991;
						key_component.downNeighborImmutable = true;
					}
					if (j >= this._lettersPerRow - 3 && j <= this._lettersPerRow - 2)
					{
						key_component.downNeighborID = 99990;
						key_component.downNeighborImmutable = true;
					}
				}
				this.keys.Add(key_component);
			}
		}
		this.backspaceButton = new ClickableTextureComponent(new Rectangle(0, 0, 128, 64), Game1.mouseCursors2, new Rectangle(32, 144, 32, 16), 4f)
		{
			myID = 99990,
			leftNeighborID = -99998,
			rightNeighborID = -99998,
			upNeighborID = -99998,
			downNeighborID = -99998
		};
		this.spaceButton = new ClickableTextureComponent(new Rectangle(0, 0, 320, 64), Game1.mouseCursors2, new Rectangle(0, 160, 80, 16), 4f)
		{
			myID = 99991,
			leftNeighborID = -99998,
			rightNeighborID = -99998,
			upNeighborID = -99998,
			downNeighborID = -99998
		};
		this.okButton = new ClickableTextureComponent(new Rectangle(0, 0, 64, 64), Game1.mouseCursors2, new Rectangle(64, 144, 16, 16), 4f)
		{
			myID = 99992,
			leftNeighborID = -99998,
			rightNeighborID = -99998,
			upNeighborID = -99998,
			downNeighborID = -99998
		};
		this.upperCaseButton = new ClickableTextureComponent(new Rectangle(0, 0, 64, 64), Game1.mouseCursors2, new Rectangle(0, 144, 16, 16), 4f)
		{
			myID = 99993,
			leftNeighborID = -99998,
			rightNeighborID = -99998,
			upNeighborID = -99998,
			downNeighborID = -99998
		};
		this.symbolsButton = new ClickableTextureComponent(new Rectangle(0, 0, 64, 64), Game1.mouseCursors2, new Rectangle(16, 144, 16, 16), 4f)
		{
			myID = 99994,
			leftNeighborID = -99998,
			rightNeighborID = -99998,
			upNeighborID = -99998,
			downNeighborID = -99998
		};
		this.ShowKeyboard(0, play_sound: false);
		this.RepositionElements();
		this.populateClickableComponentList();
		if (Game1.options.SnappyMenus)
		{
			this.snapToDefaultClickableComponent();
		}
		Game1.playSound("bigSelect");
	}

	public override bool readyToClose()
	{
		return false;
	}

	public void ShowKeyboard(int index, bool play_sound = true)
	{
		this._currentKeyboard = index;
		int key_index = 0;
		string[] array = this.letterMaps[index];
		foreach (string key_map in array)
		{
			foreach (char key_character in key_map)
			{
				this.keys[key_index].name = key_character.ToString() ?? "";
				key_index++;
			}
		}
		this.upperCaseButton.sourceRect = new Rectangle(0, 144, 16, 16);
		this.symbolsButton.sourceRect = new Rectangle(16, 144, 16, 16);
		switch (this._currentKeyboard)
		{
		case 1:
			this.upperCaseButton.sourceRect = new Rectangle(0, 176, 16, 16);
			break;
		case 2:
			this.symbolsButton.sourceRect = new Rectangle(16, 176, 16, 16);
			break;
		}
		if (play_sound)
		{
			Game1.playSound("button1");
		}
	}

	public override void snapToDefaultClickableComponent()
	{
		base.currentlySnappedComponent = base.getComponentWithID(this._lettersPerRow);
		this.snapCursorToCurrentSnappedComponent();
	}

	public void RepositionElements()
	{
		base.xPositionOnScreen = (int)Utility.getTopLeftPositionForCenteringOnScreen(672, 352).X;
		base.yPositionOnScreen = (int)Utility.getTopLeftPositionForCenteringOnScreen(672, 256).Y;
		for (int y = 0; y < this.keys.Count / this._lettersPerRow; y++)
		{
			for (int x = 0; x < this._lettersPerRow; x++)
			{
				this.keys[x + y * this._lettersPerRow].bounds = new Rectangle(base.xPositionOnScreen + 16 + x * 16 * 4, base.yPositionOnScreen + 16 + y * 16 * 4, 64, 64);
			}
		}
		this.upperCaseButton.bounds = new Rectangle(base.xPositionOnScreen + 16, base.yPositionOnScreen + 16 + 256, this.upperCaseButton.bounds.Width, this.upperCaseButton.bounds.Height);
		this.symbolsButton.bounds = new Rectangle(base.xPositionOnScreen + 16 + 64, base.yPositionOnScreen + 16 + 256, this.symbolsButton.bounds.Width, this.symbolsButton.bounds.Height);
		this.backspaceButton.bounds = new Rectangle(base.xPositionOnScreen + 16 + 448, base.yPositionOnScreen + 16 + 256, this.backspaceButton.bounds.Width, this.backspaceButton.bounds.Height);
		this.spaceButton.bounds = new Rectangle(base.xPositionOnScreen + 16 + 128, base.yPositionOnScreen + 16 + 256, this.spaceButton.bounds.Width, this.spaceButton.bounds.Height);
		this.okButton.bounds = new Rectangle(base.xPositionOnScreen + 16 + 576, base.yPositionOnScreen + 16 + 256, this.okButton.bounds.Width, this.okButton.bounds.Height);
	}

	/// <inheritdoc />
	public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
	{
		base.gameWindowSizeChanged(oldBounds, newBounds);
		this.RepositionElements();
	}

	/// <inheritdoc />
	public override void performHoverAction(int x, int y)
	{
		base.performHoverAction(x, y);
		foreach (ClickableTextureComponent key in this.keys)
		{
			key.tryHover(x, y);
		}
		this.spaceButton.tryHover(x, y);
		this.backspaceButton.tryHover(x, y);
		this.okButton.tryHover(x, y);
		this.symbolsButton.tryHover(x, y);
		this.upperCaseButton.tryHover(x, y);
	}

	/// <inheritdoc />
	public override void receiveLeftClick(int x, int y, bool playSound = true)
	{
		foreach (ClickableTextureComponent component in this.keys)
		{
			if (component.containsPoint(x, y))
			{
				this.OnLetter(component.name);
			}
		}
		if (this.okButton.containsPoint(x, y))
		{
			this.OnSubmit();
			return;
		}
		if (this.spaceButton.containsPoint(x, y))
		{
			this.OnSpaceBar();
		}
		if (this.upperCaseButton.containsPoint(x, y))
		{
			if (this._currentKeyboard != 1)
			{
				this.ShowKeyboard(1);
			}
			else
			{
				this.ShowKeyboard(0);
			}
		}
		if (this.symbolsButton.containsPoint(x, y))
		{
			if (this._currentKeyboard != 2)
			{
				this.ShowKeyboard(2);
			}
			else
			{
				this.ShowKeyboard(0);
			}
		}
		if (this.backspaceButton.containsPoint(x, y))
		{
			this.OnBackSpace();
		}
	}

	public void OnSubmit()
	{
		this._target.RecieveCommandInput('\r');
		this.Close();
	}

	public void OnSpaceBar()
	{
		this._target.RecieveTextInput(' ');
	}

	public void OnBackSpace()
	{
		this._target.RecieveCommandInput('\b');
	}

	public void OnLetter(string letter)
	{
		if (letter.Length > 0)
		{
			this._target.RecieveTextInput(letter[0]);
		}
	}

	public void Close()
	{
		Game1.playSound("bigDeSelect");
		Game1.closeTextEntry();
	}

	/// <inheritdoc />
	public override void draw(SpriteBatch b)
	{
		if (!Game1.options.showClearBackgrounds)
		{
			b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.8f);
		}
		Game1.DrawBox(base.xPositionOnScreen, base.yPositionOnScreen, base.width, base.height);
		foreach (ClickableTextureComponent key in this.keys)
		{
			key.draw(b);
			Vector2 size = Game1.dialogueFont.MeasureString(key.name);
			b.DrawString(Game1.dialogueFont, key.name, Utility.snapDrawPosition(new Vector2((float)key.bounds.Center.X - size.X / 2f, (float)key.bounds.Center.Y - size.Y / 2f)), Color.Black);
		}
		this.backspaceButton.draw(b);
		this.okButton.draw(b);
		this.spaceButton.draw(b);
		this.symbolsButton.draw(b);
		this.upperCaseButton.draw(b);
		if (this._target != null)
		{
			int x = this._target.X;
			int y = this._target.Y;
			this._target.X = (int)Utility.getTopLeftPositionForCenteringOnScreen(this._target.Width, this._target.Height * 4).X;
			this._target.Y = base.yPositionOnScreen - 96;
			this._target.Draw(b);
			this._target.X = x;
			this._target.Y = y;
		}
		base.draw(b);
		base.drawMouse(b, ignore_transparency: true);
	}

	/// <inheritdoc />
	public override void update(GameTime time)
	{
		if (this._target == null || !this._target.Selected)
		{
			this.Close();
		}
		if (Game1.input.GetGamePadState().IsButtonDown(Buttons.LeftStick) && !Game1.oldPadState.IsButtonDown(Buttons.LeftStick))
		{
			if (this._currentKeyboard != 1)
			{
				this.ShowKeyboard(1);
			}
			else
			{
				this.ShowKeyboard(0);
			}
		}
		if (Game1.input.GetGamePadState().IsButtonDown(Buttons.RightStick) && !Game1.oldPadState.IsButtonDown(Buttons.RightStick))
		{
			if (this._currentKeyboard != 2)
			{
				this.ShowKeyboard(2);
			}
			else
			{
				this.ShowKeyboard(0);
			}
		}
	}
}
