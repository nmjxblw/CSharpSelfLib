using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley.Menus;

public class AboutMenu : IClickableMenu
{
	public const int region_upArrow = 94444;

	public const int region_downArrow = 95555;

	public new const int height = 700;

	public ClickableComponent backButton;

	public ClickableTextureComponent upButton;

	public ClickableTextureComponent downButton;

	public List<ICreditsBlock> credits = new List<ICreditsBlock>();

	private int currentCreditsIndex;

	public AboutMenu()
	{
		base.width = 1280;
		base.height = 700;
		this.SetUpCredits();
		if (Game1.options.snappyMenus && Game1.options.gamepadControls)
		{
			this.populateClickableComponentList();
			this.snapToDefaultClickableComponent();
		}
	}

	public void SetUpCredits()
	{
		foreach (string line in Game1.temporaryContent.Load<List<string>>("Strings\\credits"))
		{
			if (line != null && line.Length >= 6 && line.StartsWith("[image"))
			{
				string[] split = ArgUtility.SplitBySpace(line);
				string path = split[1];
				int sourceX = Convert.ToInt32(split[2]);
				int sourceY = Convert.ToInt32(split[3]);
				int sourceWidth = Convert.ToInt32(split[4]);
				int sourceHeight = Convert.ToInt32(split[5]);
				int zoom = Convert.ToInt32(split[6]);
				int animationFrames = ((split.Length <= 7) ? 1 : Convert.ToInt32(split[7]));
				Texture2D tex = null;
				try
				{
					tex = Game1.temporaryContent.Load<Texture2D>(path);
				}
				catch (Exception)
				{
				}
				if (tex != null)
				{
					if (sourceWidth == -1)
					{
						sourceWidth = tex.Width;
						sourceHeight = tex.Height;
					}
					this.credits.Add(new ImageCreditsBlock(tex, new Rectangle(sourceX, sourceY, sourceWidth, sourceHeight), zoom, animationFrames));
				}
			}
			else if (line != null && line.Length >= 6 && line.StartsWith("[link"))
			{
				string[] array = ArgUtility.SplitBySpace(line, 3);
				string url = array[1];
				string text = array[2];
				this.credits.Add(new LinkCreditsBlock(text, url));
			}
			else
			{
				this.credits.Add(new TextCreditsBlock(line));
			}
		}
		Vector2 topLeft = Utility.getTopLeftPositionForCenteringOnScreen(base.width, base.height);
		base.xPositionOnScreen = (int)topLeft.X;
		base.yPositionOnScreen = (int)topLeft.Y;
		this.upButton = new ClickableTextureComponent(new Rectangle((int)topLeft.X + base.width - 80, (int)topLeft.Y + 64 + 16, 64, 64), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 12), 0.8f)
		{
			myID = 94444,
			downNeighborID = 95555,
			rightNeighborID = -99998,
			leftNeighborID = -99998
		};
		this.downButton = new ClickableTextureComponent(new Rectangle((int)topLeft.X + base.width - 80, (int)topLeft.Y + base.height - 32, 64, 64), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 11), 0.8f)
		{
			myID = 95555,
			upNeighborID = -99998,
			rightNeighborID = -99998,
			leftNeighborID = -99998
		};
		this.backButton = new ClickableComponent(new Rectangle(Game1.uiViewport.Width + -66 * TitleMenu.pixelZoom - 8 * TitleMenu.pixelZoom * 2, Game1.uiViewport.Height - 27 * TitleMenu.pixelZoom - 8 * TitleMenu.pixelZoom, 66 * TitleMenu.pixelZoom, 27 * TitleMenu.pixelZoom), "")
		{
			myID = 81114,
			leftNeighborID = -99998,
			rightNeighborID = -99998,
			upNeighborID = 95555
		};
	}

	public override void snapToDefaultClickableComponent()
	{
		base.currentlySnappedComponent = base.getComponentWithID(81114);
		this.snapCursorToCurrentSnappedComponent();
	}

	/// <inheritdoc />
	public override void receiveLeftClick(int x, int y, bool playSound = true)
	{
		base.receiveLeftClick(x, y, playSound);
		if (this.upButton.containsPoint(x, y))
		{
			if (this.currentCreditsIndex > 0)
			{
				this.currentCreditsIndex--;
				Game1.playSound("shiny4");
				this.upButton.scale = this.upButton.baseScale;
			}
		}
		else if (this.downButton.containsPoint(x, y))
		{
			if (this.currentCreditsIndex < this.credits.Count - 1)
			{
				this.currentCreditsIndex++;
				Game1.playSound("shiny4");
				this.downButton.scale = this.downButton.baseScale;
			}
		}
		else
		{
			if (!this.isWithinBounds(x, y))
			{
				return;
			}
			int yPos = base.yPositionOnScreen + 96;
			int oldYpos = yPos;
			int i = 0;
			while (yPos < base.yPositionOnScreen + base.height - 64 && this.credits.Count > this.currentCreditsIndex + i)
			{
				yPos += this.credits[this.currentCreditsIndex + i].getHeight(base.width - 64) + ((this.credits.Count <= this.currentCreditsIndex + i + 1 || !(this.credits[this.currentCreditsIndex + i + 1] is ImageCreditsBlock)) ? 8 : 0);
				if (y >= oldYpos && y < yPos)
				{
					this.credits[this.currentCreditsIndex + i].clicked();
					break;
				}
				i++;
				oldYpos = yPos;
			}
		}
	}

	/// <inheritdoc />
	public override void update(GameTime time)
	{
		base.update(time);
		this.upButton.visible = this.currentCreditsIndex > 0;
		this.downButton.visible = this.currentCreditsIndex < this.credits.Count - 1;
	}

	/// <inheritdoc />
	public override void receiveScrollWheelAction(int direction)
	{
		if (direction > 0 && this.currentCreditsIndex > 0)
		{
			this.currentCreditsIndex--;
			Game1.playSound("shiny4");
		}
		else if (direction < 0 && this.currentCreditsIndex < this.credits.Count - 1)
		{
			this.currentCreditsIndex++;
			Game1.playSound("shiny4");
		}
	}

	/// <inheritdoc />
	public override void performHoverAction(int x, int y)
	{
		base.performHoverAction(x, y);
		this.upButton.tryHover(x, y);
		this.downButton.tryHover(x, y);
		if (!this.isWithinBounds(x, y))
		{
			return;
		}
		int yPos = base.yPositionOnScreen + 96;
		int oldYpos = yPos;
		int i = 0;
		while (yPos < base.yPositionOnScreen + base.height - 64 && this.credits.Count > this.currentCreditsIndex + i)
		{
			yPos += this.credits[this.currentCreditsIndex + i].getHeight(base.width - 64) + ((this.credits.Count <= this.currentCreditsIndex + i + 1 || !(this.credits[this.currentCreditsIndex + i + 1] is ImageCreditsBlock)) ? 8 : 0);
			if (y >= oldYpos && y < yPos)
			{
				this.credits[this.currentCreditsIndex + i].hovered();
				break;
			}
			i++;
			oldYpos = yPos;
		}
	}

	/// <inheritdoc />
	public override void draw(SpriteBatch b)
	{
		Vector2 topLeft = Utility.getTopLeftPositionForCenteringOnScreen(base.width, base.height - 100);
		if (!Game1.options.showClearBackgrounds)
		{
			b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
		}
		IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(473, 36, 24, 24), (int)topLeft.X, (int)topLeft.Y, base.width, base.height, Color.White, 4f, drawShadow: false);
		int yPos = base.yPositionOnScreen + 96;
		int i = 0;
		while (yPos < base.yPositionOnScreen + base.height - 64 && this.credits.Count > this.currentCreditsIndex + i)
		{
			this.credits[this.currentCreditsIndex + i].draw(base.xPositionOnScreen + 32, yPos, base.width - 64, b);
			yPos += this.credits[this.currentCreditsIndex + i].getHeight(base.width - 64) + ((this.credits.Count <= this.currentCreditsIndex + i + 1 || !(this.credits[this.currentCreditsIndex + i + 1] is ImageCreditsBlock)) ? 8 : 0);
			i++;
		}
		if (this.currentCreditsIndex > 0)
		{
			this.upButton.draw(b);
		}
		if (this.currentCreditsIndex < this.credits.Count - 1)
		{
			this.downButton.draw(b);
		}
		string versionText = "v" + Game1.GetVersionString();
		float versionTextHeight = Game1.smallFont.MeasureString(versionText).Y;
		b.DrawString(Game1.smallFont, versionText, new Vector2(16f, (float)Game1.uiViewport.Height - versionTextHeight - 8f), Color.White);
		if (Game1.activeClickableMenu is TitleMenu titleMenu && !string.IsNullOrWhiteSpace(titleMenu.startupMessage))
		{
			string tipText = Game1.parseText(titleMenu.startupMessage, Game1.smallFont, 640);
			float tipHeight = Game1.smallFont.MeasureString(tipText).Y;
			b.DrawString(Game1.smallFont, tipText, new Vector2(8f, (float)Game1.uiViewport.Height - versionTextHeight - tipHeight - 4f), Color.White);
		}
		base.draw(b);
	}

	/// <inheritdoc />
	public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
	{
		base.gameWindowSizeChanged(oldBounds, newBounds);
		this.SetUpCredits();
		if (Game1.options.snappyMenus && Game1.options.gamepadControls)
		{
			int id = ((base.currentlySnappedComponent != null) ? base.currentlySnappedComponent.myID : 81114);
			this.populateClickableComponentList();
			base.currentlySnappedComponent = base.getComponentWithID(id);
			this.snapCursorToCurrentSnappedComponent();
		}
	}
}
