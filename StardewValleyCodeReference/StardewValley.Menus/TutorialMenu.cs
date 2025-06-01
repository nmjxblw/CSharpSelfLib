using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley.Menus;

public class TutorialMenu : IClickableMenu
{
	public const int constructionTab = 4;

	public const int friendshipTab = 5;

	public const int townTab = 6;

	public const int animalsTab = 7;

	private int currentTab = -1;

	private List<ClickableTextureComponent> topics = new List<ClickableTextureComponent>();

	private ClickableTextureComponent backButton;

	private ClickableTextureComponent okButton;

	private List<ClickableTextureComponent> icons = new List<ClickableTextureComponent>();

	public TutorialMenu()
		: base(Game1.uiViewport.Width / 2 - (600 + IClickableMenu.borderWidth * 2) / 2, Game1.uiViewport.Height / 2 - (600 + IClickableMenu.borderWidth * 2) / 2 - 192, 600 + IClickableMenu.borderWidth * 2, 600 + IClickableMenu.borderWidth * 2 + 192)
	{
		int xPos = base.xPositionOnScreen + 64 + 42 - 2;
		int yPos = base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder - 16;
		this.topics.Add(new ClickableTextureComponent("", new Rectangle(xPos, yPos, base.width, 64), Game1.content.LoadString("Strings\\StringsFromCSFiles:TutorialMenu.cs.11805"), "", Game1.content.Load<Texture2D>("LooseSprites\\TutorialImages\\FarmTut"), Rectangle.Empty, 1f));
		this.icons.Add(new ClickableTextureComponent(new Rectangle(xPos, yPos, 64, 64), Game1.objectSpriteSheet, Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 276), 1f));
		yPos += 68;
		this.topics.Add(new ClickableTextureComponent("", new Rectangle(xPos, yPos, base.width, 64), Game1.content.LoadString("Strings\\StringsFromCSFiles:TutorialMenu.cs.11807"), "", Game1.content.Load<Texture2D>("LooseSprites\\TutorialImages\\FarmTut"), Rectangle.Empty, 1f));
		this.icons.Add(new ClickableTextureComponent(new Rectangle(xPos, yPos, 64, 64), Game1.objectSpriteSheet, Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 142), 1f));
		yPos += 68;
		this.topics.Add(new ClickableTextureComponent("", new Rectangle(xPos, yPos, base.width, 64), Game1.content.LoadString("Strings\\StringsFromCSFiles:TutorialMenu.cs.11809"), "", Game1.content.Load<Texture2D>("LooseSprites\\TutorialImages\\FarmTut"), Rectangle.Empty, 1f));
		this.icons.Add(new ClickableTextureComponent(new Rectangle(xPos, yPos, 64, 64), Game1.objectSpriteSheet, Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 334), 1f));
		yPos += 68;
		this.topics.Add(new ClickableTextureComponent("", new Rectangle(xPos, yPos, base.width, 64), Game1.content.LoadString("Strings\\StringsFromCSFiles:TutorialMenu.cs.11811"), "", Game1.content.Load<Texture2D>("LooseSprites\\TutorialImages\\FarmTut"), Rectangle.Empty, 1f));
		this.icons.Add(new ClickableTextureComponent(new Rectangle(xPos, yPos, 64, 64), Game1.objectSpriteSheet, Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 308), 1f));
		yPos += 68;
		this.topics.Add(new ClickableTextureComponent("", new Rectangle(xPos, yPos, base.width, 64), Game1.content.LoadString("Strings\\StringsFromCSFiles:TutorialMenu.cs.11813"), "", Game1.content.Load<Texture2D>("LooseSprites\\TutorialImages\\FarmTut"), Rectangle.Empty, 1f));
		this.icons.Add(new ClickableTextureComponent(new Rectangle(xPos, yPos, 64, 64), Game1.objectSpriteSheet, Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 395), 1f));
		yPos += 68;
		this.topics.Add(new ClickableTextureComponent("", new Rectangle(xPos, yPos, base.width, 64), Game1.content.LoadString("Strings\\StringsFromCSFiles:TutorialMenu.cs.11815"), "", Game1.content.Load<Texture2D>("LooseSprites\\TutorialImages\\FarmTut"), Rectangle.Empty, 1f));
		this.icons.Add(new ClickableTextureComponent(new Rectangle(xPos, yPos, 64, 64), Game1.objectSpriteSheet, Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 458), 1f));
		yPos += 68;
		this.topics.Add(new ClickableTextureComponent("", new Rectangle(xPos, yPos, base.width, 64), Game1.content.LoadString("Strings\\StringsFromCSFiles:TutorialMenu.cs.11817"), "", Game1.content.Load<Texture2D>("LooseSprites\\TutorialImages\\FarmTut"), Rectangle.Empty, 1f));
		this.icons.Add(new ClickableTextureComponent(new Rectangle(xPos, yPos, 64, 64), Game1.objectSpriteSheet, Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 102), 1f));
		yPos += 68;
		this.topics.Add(new ClickableTextureComponent("", new Rectangle(xPos, yPos, base.width, 64), Game1.content.LoadString("Strings\\StringsFromCSFiles:TutorialMenu.cs.11819"), "", Game1.content.Load<Texture2D>("LooseSprites\\TutorialImages\\FarmTut"), Rectangle.Empty, 1f));
		this.icons.Add(new ClickableTextureComponent(new Rectangle(xPos, yPos, 64, 64), Game1.objectSpriteSheet, Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 403), 1f));
		yPos += 68;
		this.okButton = new ClickableTextureComponent("OK", new Rectangle(base.xPositionOnScreen + base.width - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder - 64, base.yPositionOnScreen + base.height - IClickableMenu.borderWidth - IClickableMenu.spaceToClearTopBorder + 16, 64, 64), null, null, Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46), 1f);
		this.backButton = new ClickableTextureComponent("Back", new Rectangle(base.xPositionOnScreen + base.width - IClickableMenu.borderWidth - IClickableMenu.spaceToClearSideBorder - 48, base.yPositionOnScreen + base.height - IClickableMenu.borderWidth - IClickableMenu.spaceToClearTopBorder + 16, 64, 64), null, null, Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44), 1f);
	}

	/// <inheritdoc />
	public override void receiveLeftClick(int x, int y, bool playSound = true)
	{
		if (this.currentTab == -1)
		{
			for (int i = 0; i < this.topics.Count; i++)
			{
				if (this.topics[i].containsPoint(x, y))
				{
					this.currentTab = i;
					Game1.playSound("smallSelect");
					break;
				}
			}
		}
		if (this.currentTab != -1 && this.backButton.containsPoint(x, y))
		{
			this.currentTab = -1;
			Game1.playSound("bigDeSelect");
		}
		else if (this.currentTab == -1 && this.okButton.containsPoint(x, y))
		{
			Game1.playSound("bigDeSelect");
			Game1.exitActiveMenu();
			if (Game1.currentLocation.currentEvent != null)
			{
				Game1.currentLocation.currentEvent.CurrentCommand++;
			}
		}
	}

	/// <inheritdoc />
	public override void performHoverAction(int x, int y)
	{
		foreach (ClickableTextureComponent c in this.topics)
		{
			if (c.containsPoint(x, y))
			{
				c.scale = 2f;
			}
			else
			{
				c.scale = 1f;
			}
		}
		if (this.okButton.containsPoint(x, y))
		{
			this.okButton.scale = Math.Min(this.okButton.scale + 0.02f, this.okButton.baseScale + 0.1f);
		}
		else
		{
			this.okButton.scale = Math.Max(this.okButton.scale - 0.02f, this.okButton.baseScale);
		}
		if (this.backButton.containsPoint(x, y))
		{
			this.backButton.scale = Math.Min(this.backButton.scale + 0.02f, this.backButton.baseScale + 0.1f);
		}
		else
		{
			this.backButton.scale = Math.Max(this.backButton.scale - 0.02f, this.backButton.baseScale);
		}
	}

	/// <inheritdoc />
	public override void draw(SpriteBatch b)
	{
		b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.4f);
		Game1.drawDialogueBox(base.xPositionOnScreen, base.yPositionOnScreen, base.width, base.height, speaker: false, drawOnlyBox: true);
		if (this.currentTab != -1)
		{
			this.backButton.draw(b);
			b.Draw(this.topics[this.currentTab].texture, new Vector2(base.xPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearSideBorder, base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder - 16), this.topics[this.currentTab].texture.Bounds, Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0.89f);
		}
		else
		{
			foreach (ClickableTextureComponent c in this.topics)
			{
				Color color = ((c.scale > 1f) ? Color.Blue : Game1.textColor);
				b.DrawString(Game1.smallFont, c.label, new Vector2(c.bounds.X + 64 + 16, c.bounds.Y + 21), color);
			}
			foreach (ClickableTextureComponent icon in this.icons)
			{
				icon.draw(b);
			}
			this.okButton.draw(b);
		}
		base.drawMouse(b);
	}
}
