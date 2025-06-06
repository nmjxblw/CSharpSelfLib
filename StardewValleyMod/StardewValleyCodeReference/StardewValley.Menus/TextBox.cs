using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.SDKs.Steam;

namespace StardewValley.Menus;

public class TextBox : IKeyboardSubscriber
{
	protected Texture2D _textBoxTexture;

	protected Texture2D _caretTexture;

	protected SpriteFont _font;

	protected Color _textColor;

	public bool numbersOnly;

	public int textLimit = -1;

	public bool limitWidth = true;

	private string _text = "";

	private bool _showKeyboard;

	private bool _selected;

	public SpriteFont Font => this._font;

	public Color TextColor => this._textColor;

	public int X { get; set; }

	public int Y { get; set; }

	public int Width { get; set; }

	public int Height { get; set; }

	public bool PasswordBox { get; set; }

	public string Text
	{
		get
		{
			return this._text;
		}
		set
		{
			this._text = value;
			if (this._text == null)
			{
				this._text = "";
			}
			if (this._text != "")
			{
				this._text = Utility.FilterDirtyWordsIfStrictPlatform(this._text);
				if (this.limitWidth && this._font.MeasureString(this._text).X > (float)(this.Width - 21))
				{
					this.Text = this._text.Substring(0, this._text.Length - 1);
				}
			}
		}
	}

	/// <summary>
	/// Displayed as the title for virtual keyboards.
	/// </summary>
	public string TitleText { get; set; }

	public bool Selected
	{
		get
		{
			return this._selected;
		}
		set
		{
			if (this._selected == value)
			{
				return;
			}
			this._selected = value;
			if (this._selected)
			{
				Game1.keyboardDispatcher.Subscriber = this;
				this._showKeyboard = true;
				return;
			}
			this._showKeyboard = false;
			if (Program.sdk is SteamHelper { active: not false } steamHelper)
			{
				steamHelper.CancelKeyboard();
			}
			if (Game1.keyboardDispatcher.Subscriber == this)
			{
				Game1.keyboardDispatcher.Subscriber = null;
			}
		}
	}

	public event TextBoxEvent OnEnterPressed;

	public event TextBoxEvent OnTabPressed;

	public event TextBoxEvent OnBackspacePressed;

	public TextBox(Texture2D textBoxTexture, Texture2D caretTexture, SpriteFont font, Color textColor)
	{
		this._textBoxTexture = textBoxTexture;
		if (textBoxTexture != null)
		{
			this.Width = textBoxTexture.Width;
			this.Height = textBoxTexture.Height;
		}
		this._caretTexture = caretTexture;
		this._font = font;
		this._textColor = textColor;
	}

	public void SelectMe()
	{
		this.Selected = true;
	}

	public void Update()
	{
		Point mousePoint = new Point(Game1.getMouseX(), Game1.getMouseY());
		if (new Rectangle(this.X, this.Y, this.Width, this.Height).Contains(mousePoint))
		{
			this.Selected = true;
		}
		else
		{
			this.Selected = false;
		}
		if (this._showKeyboard)
		{
			if (Game1.options.gamepadControls && !Game1.lastCursorMotionWasMouse)
			{
				Game1.showTextEntry(this);
			}
			this._showKeyboard = false;
		}
	}

	public virtual void Draw(SpriteBatch spriteBatch, bool drawShadow = true)
	{
		bool caretVisible = Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 1000.0 >= 500.0;
		string toDraw = this.Text;
		if (this.PasswordBox)
		{
			toDraw = "";
			for (int i = 0; i < this.Text.Length; i++)
			{
				toDraw += "•";
			}
		}
		if (this._textBoxTexture != null)
		{
			spriteBatch.Draw(this._textBoxTexture, new Rectangle(this.X, this.Y, 16, this.Height), new Rectangle(0, 0, 16, this.Height), Color.White);
			spriteBatch.Draw(this._textBoxTexture, new Rectangle(this.X + 16, this.Y, this.Width - 32, this.Height), new Rectangle(16, 0, 4, this.Height), Color.White);
			spriteBatch.Draw(this._textBoxTexture, new Rectangle(this.X + this.Width - 16, this.Y, 16, this.Height), new Rectangle(this._textBoxTexture.Bounds.Width - 16, 0, 16, this.Height), Color.White);
		}
		else
		{
			Game1.drawDialogueBox(this.X - 32, this.Y - 112 + 10, this.Width + 80, this.Height, speaker: false, drawOnlyBox: true);
		}
		Vector2 size = this._font.MeasureString(toDraw);
		while (size.X > (float)this.Width)
		{
			toDraw = toDraw.Substring(1);
			size = this._font.MeasureString(toDraw);
		}
		if (caretVisible && this.Selected)
		{
			spriteBatch.Draw(Game1.staminaRect, new Rectangle(this.X + 16 + (int)size.X + 2, this.Y + 8, 4, 32), this._textColor);
		}
		if (drawShadow)
		{
			Utility.drawTextWithShadow(spriteBatch, toDraw, this._font, new Vector2(this.X + 16, this.Y + ((this._textBoxTexture != null) ? 12 : 8)), this._textColor);
		}
		else
		{
			spriteBatch.DrawString(this._font, toDraw, new Vector2(this.X + 16, this.Y + ((this._textBoxTexture != null) ? 12 : 8)), this._textColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.99f);
		}
	}

	public virtual void RecieveTextInput(char inputChar)
	{
		if (!this.Selected || (this.numbersOnly && !char.IsDigit(inputChar)) || (this.textLimit != -1 && this.Text.Length >= this.textLimit))
		{
			return;
		}
		if (Game1.gameMode != 3)
		{
			switch (inputChar)
			{
			case '+':
				Game1.playSound("slimeHit");
				break;
			case '*':
				Game1.playSound("hammer");
				break;
			case '=':
				Game1.playSound("coin");
				break;
			case '<':
				Game1.playSound("crystal", 0);
				break;
			case '$':
				Game1.playSound("money");
				break;
			case '"':
				return;
			default:
				Game1.playSound("cowboy_monsterhit");
				break;
			}
		}
		this.Text += inputChar;
	}

	public virtual void RecieveTextInput(string text)
	{
		int dummy = -1;
		if (this.Selected && (!this.numbersOnly || int.TryParse(text, out dummy)) && (this.textLimit == -1 || this.Text.Length < this.textLimit))
		{
			this.Text += text;
		}
	}

	public virtual void RecieveCommandInput(char command)
	{
		if (!this.Selected)
		{
			return;
		}
		switch (command)
		{
		case '\b':
			if (this.Text.Length <= 0)
			{
				break;
			}
			if (this.OnBackspacePressed != null)
			{
				this.OnBackspacePressed(this);
				break;
			}
			this.Text = this.Text.Substring(0, this.Text.Length - 1);
			if (Game1.gameMode != 3)
			{
				Game1.playSound("tinyWhip");
			}
			break;
		case '\r':
			this.OnEnterPressed?.Invoke(this);
			break;
		case '\t':
			this.OnTabPressed?.Invoke(this);
			break;
		}
	}

	public void RecieveSpecialInput(Keys key)
	{
	}

	public void Hover(int x, int y)
	{
		if (x > this.X && x < this.X + this.Width && y > this.Y && y < this.Y + this.Height)
		{
			Game1.SetFreeCursorDrag();
		}
	}
}
