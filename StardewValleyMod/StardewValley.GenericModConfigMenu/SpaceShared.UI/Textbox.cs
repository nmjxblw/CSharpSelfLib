using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;

namespace SpaceShared.UI;

internal class Textbox : Element, IKeyboardSubscriber
{
	private readonly Texture2D Tex;

	private readonly SpriteFont Font;

	private bool SelectedImpl;

	public virtual string String { get; set; }

	public bool Selected
	{
		get
		{
			return this.SelectedImpl;
		}
		set
		{
			if (this.SelectedImpl != value)
			{
				this.SelectedImpl = value;
				if (this.SelectedImpl)
				{
					Game1.keyboardDispatcher.Subscriber = (IKeyboardSubscriber)(object)this;
				}
				else if ((object)Game1.keyboardDispatcher.Subscriber == this)
				{
					Game1.keyboardDispatcher.Subscriber = null;
				}
			}
		}
	}

	public Action<Element> Callback { get; set; }

	public override int Width => 192;

	public override int Height => 48;

	public Textbox()
	{
		this.Tex = Game1.content.Load<Texture2D>("LooseSprites\\textBox");
		this.Font = Game1.smallFont;
	}

	public override void Update(bool isOffScreen = false)
	{
		base.Update(isOffScreen);
		if (base.ClickGestured && this.Callback != null)
		{
			this.Selected = base.Hover;
		}
	}

	public override void Draw(SpriteBatch b)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		if (!base.IsHidden())
		{
			b.Draw(this.Tex, base.Position, Color.White);
			string text = this.String;
			Vector2 vector2 = this.Font.MeasureString(text);
			while (vector2.X > 192f)
			{
				text = text.Substring(1);
				vector2 = this.Font.MeasureString(text);
			}
			if (DateTime.UtcNow.Millisecond % 1000 >= 500 && this.Selected)
			{
				b.Draw(Game1.staminaRect, new Rectangle((int)base.Position.X + 16 + (int)vector2.X + 2, (int)base.Position.Y + 8, 4, 32), Game1.textColor);
			}
			b.DrawString(this.Font, text, base.Position + new Vector2(16f, 12f), Game1.textColor);
		}
	}

	public void RecieveTextInput(char inputChar)
	{
		this.ReceiveInput(inputChar.ToString());
		switch (inputChar)
		{
		case '"':
			break;
		case '$':
			Game1.playSound("money", (int?)null);
			break;
		case '*':
			Game1.playSound("hammer", (int?)null);
			break;
		case '+':
			Game1.playSound("slimeHit", (int?)null);
			break;
		case '<':
			Game1.playSound("crystal", (int?)null);
			break;
		case '=':
			Game1.playSound("coin", (int?)null);
			break;
		default:
			Game1.playSound("cowboy_monsterhit", (int?)null);
			break;
		}
	}

	public void RecieveTextInput(string text)
	{
		this.ReceiveInput(text);
	}

	public void RecieveCommandInput(char command)
	{
		if (command == '\b' && this.String.Length > 0)
		{
			Game1.playSound("tinyWhip", (int?)null);
			this.String = this.String.Substring(0, this.String.Length - 1);
			this.Callback?.Invoke(this);
		}
	}

	public void RecieveSpecialInput(Keys key)
	{
	}

	protected virtual void ReceiveInput(string str)
	{
		this.String += str;
		this.Callback?.Invoke(this);
	}
}
