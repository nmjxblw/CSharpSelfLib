using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.BellsAndWhistles;

namespace StardewValley.Menus;

public class ChooseFromListMenu : IClickableMenu
{
	public delegate void actionOnChoosingListOption(string s);

	public const int region_backButton = 101;

	public const int region_forwardButton = 102;

	public const int region_okButton = 103;

	public const int region_cancelButton = 104;

	public const int w = 640;

	public const int h = 192;

	public ClickableTextureComponent backButton;

	public ClickableTextureComponent forwardButton;

	public ClickableTextureComponent okButton;

	public ClickableTextureComponent cancelButton;

	private List<string> options = new List<string>();

	private int index;

	private actionOnChoosingListOption chooseAction;

	private bool isJukebox;

	public ChooseFromListMenu(List<string> options, actionOnChoosingListOption chooseAction, bool isJukebox = false, string default_selection = null)
		: base(Game1.uiViewport.Width / 2 - 320, Game1.uiViewport.Height - 64 - 192, 640, 192)
	{
		this.chooseAction = chooseAction;
		this.backButton = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen - 128 - 4, base.yPositionOnScreen + 85, 48, 44), Game1.mouseCursors, new Rectangle(352, 495, 12, 11), 4f)
		{
			myID = 101,
			rightNeighborID = 102
		};
		this.forwardButton = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen + 640 + 16 + 64, base.yPositionOnScreen + 85, 48, 44), Game1.mouseCursors, new Rectangle(365, 495, 12, 11), 4f)
		{
			myID = 102,
			leftNeighborID = 101,
			rightNeighborID = 103
		};
		this.okButton = new ClickableTextureComponent("OK", new Rectangle(base.xPositionOnScreen + base.width + 128 + 8, base.yPositionOnScreen + 192 - 128, 64, 64), null, null, Game1.mouseCursors, new Rectangle(175, 379, 16, 15), 4f)
		{
			myID = 103,
			leftNeighborID = 102,
			rightNeighborID = 104
		};
		this.cancelButton = new ClickableTextureComponent("OK", new Rectangle(base.xPositionOnScreen + base.width + 192 + 12, base.yPositionOnScreen + 192 - 128, 64, 64), null, null, Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 47), 1f)
		{
			myID = 104,
			leftNeighborID = 103
		};
		Game1.playSound("bigSelect");
		this.isJukebox = isJukebox;
		this.options = options;
		if (default_selection != null)
		{
			int default_index = options.IndexOf(default_selection);
			if (default_index >= 0)
			{
				this.index = default_index;
			}
		}
		if (Game1.options.SnappyMenus)
		{
			this.populateClickableComponentList();
			this.snapToDefaultClickableComponent();
		}
	}

	public override void snapToDefaultClickableComponent()
	{
		base.currentlySnappedComponent = base.getComponentWithID(103);
		this.snapCursorToCurrentSnappedComponent();
	}

	/// <inheritdoc />
	public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
	{
		base.gameWindowSizeChanged(oldBounds, newBounds);
		base.xPositionOnScreen = Game1.uiViewport.Width / 2 - 320;
		base.yPositionOnScreen = Game1.uiViewport.Height - 64 - 192;
		this.backButton = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen - 128 - 4, base.yPositionOnScreen + 85, 48, 44), Game1.mouseCursors, new Rectangle(352, 495, 12, 11), 4f);
		this.forwardButton = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen + 640 + 16 + 64, base.yPositionOnScreen + 85, 48, 44), Game1.mouseCursors, new Rectangle(365, 495, 12, 11), 4f);
		this.okButton = new ClickableTextureComponent("OK", new Rectangle(base.xPositionOnScreen + base.width + 128 + 8, base.yPositionOnScreen + 192 - 128, 64, 64), null, null, Game1.mouseCursors, new Rectangle(175, 379, 16, 15), 4f);
		this.cancelButton = new ClickableTextureComponent("OK", new Rectangle(base.xPositionOnScreen + base.width + 192 + 12, base.yPositionOnScreen + 192 - 128, 64, 64), null, null, Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 47), 1f);
	}

	public static void playSongAction(string s)
	{
		Game1.changeMusicTrack(s);
	}

	/// <inheritdoc />
	public override void performHoverAction(int x, int y)
	{
		base.performHoverAction(x, y);
		this.okButton.tryHover(x, y);
		this.cancelButton.tryHover(x, y);
		this.backButton.tryHover(x, y);
		this.forwardButton.tryHover(x, y);
	}

	/// <inheritdoc />
	public override void receiveLeftClick(int x, int y, bool playSound = true)
	{
		base.receiveLeftClick(x, y, playSound);
		if (this.okButton.containsPoint(x, y) && this.chooseAction != null)
		{
			this.chooseAction(this.options[this.index]);
			Game1.playSound("select");
		}
		if (this.cancelButton.containsPoint(x, y))
		{
			base.exitThisMenu();
		}
		if (this.backButton.containsPoint(x, y))
		{
			this.index--;
			if (this.index < 0)
			{
				this.index = this.options.Count - 1;
			}
			this.backButton.scale = this.backButton.baseScale - 1f;
			Game1.playSound("shwip");
		}
		if (this.forwardButton.containsPoint(x, y))
		{
			this.index++;
			this.index %= this.options.Count;
			Game1.playSound("shwip");
			this.forwardButton.scale = this.forwardButton.baseScale - 1f;
		}
	}

	/// <inheritdoc />
	public override void draw(SpriteBatch b)
	{
		base.draw(b);
		string maxWidthJukeboxString = "Summer (The Sun Can Bend An Orange Sky)";
		int stringWidth = (int)Game1.dialogueFont.MeasureString(this.isJukebox ? maxWidthJukeboxString : this.options[this.index]).X;
		IClickableMenu.drawTextureBox(b, base.xPositionOnScreen + base.width / 2 - stringWidth / 2 - 16, base.yPositionOnScreen + 64 - 4, stringWidth + 32, 80, Color.White);
		if (this.index < this.options.Count)
		{
			Utility.drawTextWithShadow(b, this.isJukebox ? Utility.getSongTitleFromCueName(this.options[this.index]) : this.options[this.index], Game1.dialogueFont, new Vector2((float)(base.xPositionOnScreen + base.width / 2) - Game1.dialogueFont.MeasureString(this.isJukebox ? Utility.getSongTitleFromCueName(this.options[this.index]) : this.options[this.index]).X / 2f, base.yPositionOnScreen + base.height / 2 - 16), Game1.textColor);
		}
		this.okButton.draw(b);
		this.cancelButton.draw(b);
		this.forwardButton.draw(b);
		this.backButton.draw(b);
		if (this.isJukebox)
		{
			SpriteText.drawStringWithScrollCenteredAt(b, Game1.content.LoadString("Strings\\UI:JukeboxMenu_Title"), base.xPositionOnScreen + base.width / 2, base.yPositionOnScreen - 32);
		}
		base.drawMouse(b);
	}
}
