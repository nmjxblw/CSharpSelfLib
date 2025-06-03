using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace StardewValley.Menus;

public class TitleTextInputMenu : NamingMenu
{
	public ClickableTextureComponent pasteButton;

	public const int region_pasteButton = 105;

	public string context = "";

	public TitleTextInputMenu(string title, doneNamingBehavior b, string default_text = "", string context = "", bool filterInput = true)
		: base(b, title, "")
	{
		base.FilterInput = filterInput;
		this.context = context;
		base.textBox.limitWidth = false;
		base.textBox.Width = 512;
		base.textBox.X -= 128;
		base.randomButton.visible = false;
		this.pasteButton = new ClickableTextureComponent(new Rectangle(base.textBox.X + base.textBox.Width + 32 + 4 + 64, Game1.viewport.Height / 2 - 8, 64, 64), Game1.mouseCursors, new Rectangle(274, 284, 16, 16), 4f)
		{
			myID = 105,
			leftNeighborID = 102
		};
		this.pasteButton.visible = true;
		base.doneNamingButton.rightNeighborID = 105;
		base.doneNamingButton.bounds.X += 128;
		base.minLength = 0;
		if (Game1.options.SnappyMenus)
		{
			this.populateClickableComponentList();
			this.snapToDefaultClickableComponent();
		}
		base.textBox.Text = default_text;
	}

	/// <inheritdoc />
	public override void performHoverAction(int x, int y)
	{
		this.pasteButton?.tryHover(x, y);
		base.performHoverAction(x, y);
	}

	/// <inheritdoc />
	public override void receiveLeftClick(int x, int y, bool playSound = true)
	{
		if (this.pasteButton != null && this.pasteButton.containsPoint(x, y))
		{
			string pasted_text = "";
			if (DesktopClipboard.GetText(ref pasted_text))
			{
				Game1.playSound("drumkit6");
				base.textBox.Text = pasted_text;
			}
			else
			{
				Game1.playSound("cancel");
			}
		}
		base.receiveLeftClick(x, y, playSound);
	}

	/// <inheritdoc />
	public override void update(GameTime time)
	{
		GamePadState pad = Game1.input.GetGamePadState();
		KeyboardState keyboard = Game1.GetKeyboardState();
		if (Game1.IsPressEvent(ref pad, Buttons.B) || Game1.IsPressEvent(ref keyboard, Keys.Escape))
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
		base.update(time);
	}

	/// <inheritdoc />
	public override void draw(SpriteBatch b)
	{
		base.draw(b);
		this.pasteButton?.draw(b);
	}
}
