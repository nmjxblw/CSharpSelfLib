using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.BellsAndWhistles;
using StardewValley.Tools;

namespace StardewValley.Menus;

public class ChooseFromIconsMenu : IClickableMenu
{
	private Rectangle iconBackRectangle;

	private Texture2D texture;

	private Point iconBackHighlightPosition;

	private Point iconFrontHighlightPositionOffset;

	private string which;

	public List<ClickableTextureComponent> icons = new List<ClickableTextureComponent>();

	public List<ClickableTextureComponent> iconFronts = new List<ClickableTextureComponent>();

	private int iconXOffset;

	private int maxTooltipHeight;

	private int maxTooltipWidth;

	private float destroyTimer = -1f;

	private List<TemporaryAnimatedSprite> temporarySprites = new List<TemporaryAnimatedSprite>();

	public Object sourceObject;

	private bool hasTooltips = true;

	private string title;

	private string hoverSound;

	private int titleStyle = 3;

	private int selected = -1;

	public ChooseFromIconsMenu(string which)
	{
		this.setUpIcons(which);
	}

	/// <inheritdoc />
	public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
	{
		base.gameWindowSizeChanged(oldBounds, newBounds);
		this.setUpIcons(this.which);
	}

	public void setUpIcons(string which)
	{
		int iconSpacing = 32;
		int iconOffsetXMargin = 12;
		int iconOffsetYMargin = 4;
		this.which = which;
		this.title = Game1.content.LoadString("Strings\\1_6_Strings:ChooseOne");
		this.hoverSound = "boulderCrack";
		this.icons.Clear();
		this.iconFronts.Clear();
		if (!(which == "dwarfStatue"))
		{
			if (which == "bobbers")
			{
				if (Game1.player.usingRandomizedBobber)
				{
					Game1.player.bobberStyle.Value = -2;
				}
				int available = Game1.player.fishCaught.Count() / 2;
				iconSpacing = 4;
				this.iconBackRectangle = new Rectangle(222, 317, 16, 16);
				this.iconBackHighlightPosition = new Point(256, 317);
				this.texture = Game1.mouseCursors_1_6;
				for (int i = 0; i < FishingRod.NUM_BOBBER_STYLES; i++)
				{
					bool num = i > available;
					Rectangle src = Game1.getSourceRectForStandardTileSheet(Game1.bobbersTexture, i, 16, 32);
					src.Height = 16;
					this.icons.Add(new ClickableTextureComponent(new Rectangle(0, 0, 64, 64), this.texture, this.iconBackRectangle, 4f, drawShadow: true)
					{
						name = (i.ToString() ?? "")
					});
					if (num)
					{
						this.iconFronts.Add(new ClickableTextureComponent(new Rectangle(0, 0, 16, 16), Game1.mouseCursors_1_6, new Rectangle(272, 317, 16, 16), 4f)
						{
							name = "ghosted"
						});
					}
					else
					{
						this.iconFronts.Add(new ClickableTextureComponent(new Rectangle(0, 0, 16, 16), Game1.bobbersTexture, src, 4f, drawShadow: true));
					}
				}
				this.icons.Add(new ClickableTextureComponent(new Rectangle(0, 0, 64, 64), null, new Rectangle(0, 0, 0, 0), 4f, drawShadow: true)
				{
					name = "-2"
				});
				this.iconFronts.Add(new ClickableTextureComponent(new Rectangle(0, 0, 10, 10), Game1.mouseCursors_1_6, new Rectangle(496, 28, 16, 16), 4f, drawShadow: true));
				this.selected = Game1.player.bobberStyle.Value;
				iconOffsetXMargin = 0;
				iconOffsetYMargin = 0;
				this.hasTooltips = false;
				this.title = Game1.content.LoadString("Strings\\1_6_Strings:ChooseBobber");
				this.titleStyle = 0;
				this.hoverSound = null;
			}
		}
		else
		{
			Game1.playSound("stone_button");
			this.iconBackRectangle = new Rectangle(127, 123, 21, 21);
			this.iconBackHighlightPosition = new Point(127, 144);
			this.iconFrontHighlightPositionOffset = new Point(0, 17);
			this.texture = Game1.mouseCursors_1_6;
			Random dwarf_random = Utility.CreateRandom(Game1.stats.DaysPlayed * 77, Game1.uniqueIDForThisGame);
			int icon1 = dwarf_random.Next(5);
			int icon2 = -1;
			do
			{
				icon2 = dwarf_random.Next(5);
			}
			while (icon2 == icon1);
			this.icons.Add(new ClickableTextureComponent(new Rectangle(0, 0, 84, 84), this.texture, this.iconBackRectangle, 4f, drawShadow: true)
			{
				name = (icon1.ToString() ?? ""),
				hoverText = Game1.content.LoadString("Strings\\1_6_Strings:DwarfStatue_" + icon1)
			});
			this.icons.Add(new ClickableTextureComponent(new Rectangle(0, 0, 84, 84), this.texture, this.iconBackRectangle, 4f, drawShadow: true)
			{
				name = (icon2.ToString() ?? ""),
				hoverText = Game1.content.LoadString("Strings\\1_6_Strings:DwarfStatue_" + icon2)
			});
			this.iconFronts.Add(new ClickableTextureComponent(new Rectangle(0, 0, 17, 17), this.texture, new Rectangle(148 + icon1 * 17, 123, 17, 17), 4f));
			this.iconFronts.Add(new ClickableTextureComponent(new Rectangle(0, 0, 17, 17), this.texture, new Rectangle(148 + icon2 * 17, 123, 17, 17), 4f));
		}
		int toolTipWidth = (this.hasTooltips ? 240 : 0);
		int iconWidth = Math.Max(this.iconBackRectangle.Width * 4, toolTipWidth) + iconSpacing;
		this.iconXOffset = iconWidth / 2 - this.iconBackRectangle.Width * 4 / 2 - 4;
		base.width = Math.Max(800, Game1.uiViewport.Width / 3);
		base.xPositionOnScreen = Game1.uiViewport.Width / 2 - base.width / 2;
		base.height = 100;
		this.maxTooltipHeight = 0;
		this.maxTooltipWidth = 0;
		if (this.hasTooltips)
		{
			foreach (ClickableTextureComponent i2 in this.icons)
			{
				i2.hoverText = Game1.parseText(i2.hoverText, Game1.smallFont, toolTipWidth - 32);
				this.maxTooltipHeight = Math.Max(this.maxTooltipHeight, (int)Game1.smallFont.MeasureString(i2.hoverText).Y);
				this.maxTooltipWidth = Math.Max(this.maxTooltipWidth, (int)Game1.smallFont.MeasureString(i2.hoverText).X);
			}
			this.maxTooltipHeight += 48;
			this.maxTooltipWidth += 48;
		}
		base.height += (this.icons.Count * iconWidth / base.width + 1) * (this.maxTooltipHeight + this.icons[0].bounds.Height + iconSpacing);
		int maxIconsPerRow = base.width / iconWidth;
		base.yPositionOnScreen = Game1.uiViewport.Height / 2 - base.height / 2;
		int y = base.yPositionOnScreen + 100;
		for (int j = 0; j < this.icons.Count; j += maxIconsPerRow)
		{
			int rowCount = Math.Min(this.icons.Count - j, maxIconsPerRow);
			int x = base.xPositionOnScreen + base.width / 2 - rowCount * iconWidth / 2;
			for (int k = 0; k < rowCount; k++)
			{
				int index = k + j;
				this.icons[index].bounds.X = x + k * iconWidth;
				this.icons[index].bounds.Y = y;
				this.icons[index].bounds.Width = iconWidth;
				this.icons[index].bounds.Height += this.maxTooltipHeight;
				this.iconFronts[index].bounds.X = this.icons[index].bounds.X + iconOffsetXMargin;
				this.iconFronts[index].bounds.Y = this.icons[index].bounds.Y + iconOffsetYMargin;
				this.icons[index].myID = index;
				this.icons[index].leftNeighborID = index - 1;
				this.icons[index].rightNeighborID = index + 1;
				this.icons[index].downNeighborID = index + rowCount;
				this.icons[index].upNeighborID = index - rowCount;
			}
			y += this.maxTooltipHeight + this.icons[0].bounds.Height + iconSpacing;
		}
		base.initialize(base.xPositionOnScreen, base.yPositionOnScreen, base.width, base.height, showUpperRightCloseButton: true);
		if (Game1.options.SnappyMenus)
		{
			this.populateClickableComponentList();
			base.currentlySnappedComponent = base.getComponentWithID(0);
			this.snapCursorToCurrentSnappedComponent();
		}
	}

	/// <inheritdoc />
	public override void update(GameTime time)
	{
		base.update(time);
		if (this.destroyTimer > 0f)
		{
			this.destroyTimer -= (float)time.ElapsedGameTime.TotalMilliseconds;
			if (this.destroyTimer <= 0f)
			{
				this.flairOnDestroy();
				Game1.activeClickableMenu = null;
			}
		}
		this.temporarySprites.RemoveAll((TemporaryAnimatedSprite sprite) => sprite.update(time));
	}

	/// <inheritdoc />
	public override void performHoverAction(int x, int y)
	{
		base.performHoverAction(x, y);
		for (int i = 0; i < this.icons.Count; i++)
		{
			ClickableTextureComponent c = this.icons[i];
			this.iconFronts[i].sourceRect = this.iconFronts[i].startingSourceRect;
			if (c.containsPoint(x, y) && this.destroyTimer == -1f)
			{
				if (c.sourceRect == c.startingSourceRect && this.hoverSound != null)
				{
					Game1.playSound(this.hoverSound);
				}
				c.sourceRect.Location = this.iconBackHighlightPosition;
				this.iconFronts[i].sourceRect.Location = new Point(this.iconFronts[i].sourceRect.Location.X + this.iconFrontHighlightPositionOffset.X, this.iconFronts[i].sourceRect.Location.Y + this.iconFrontHighlightPositionOffset.Y);
			}
			else
			{
				c.sourceRect = this.iconBackRectangle;
			}
		}
	}

	/// <inheritdoc />
	public override void receiveLeftClick(int x, int y, bool playSound = true)
	{
		if (this.destroyTimer >= 0f)
		{
			return;
		}
		base.receiveLeftClick(x, y, playSound);
		for (int i = 0; i < this.icons.Count; i++)
		{
			ClickableTextureComponent c = this.icons[i];
			if (!c.containsPoint(x, y))
			{
				continue;
			}
			bool ghosted = this.iconFronts[i].name.Contains("ghosted");
			string text = this.which;
			if (!(text == "dwarfStatue"))
			{
				if (text == "bobbers")
				{
					if (ghosted)
					{
						Game1.playSound("smallSelect");
						break;
					}
					int selection = Convert.ToInt32(c.name);
					if (Game1.player.bobberStyle.Value != selection)
					{
						Game1.playSound("button1");
						this.hoverSound = null;
						Game1.player.bobberStyle.Value = Convert.ToInt32(c.name);
						this.selected = Game1.player.bobberStyle.Value;
						if (this.selected == -2)
						{
							Game1.player.usingRandomizedBobber = true;
						}
						else
						{
							Game1.player.usingRandomizedBobber = false;
						}
					}
				}
			}
			else
			{
				Game1.playSound("button_tap");
				DelayedAction.playSoundAfterDelay("button_tap", 70);
				DelayedAction.playSoundAfterDelay("discoverMineral", 750);
				for (int j = 0; j < 16; j++)
				{
					this.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors_1_6", new Rectangle(98 + Game1.random.Next(3) * 4, 161, 4, 4), Utility.getRandomPositionInThisRectangle(c.bounds, Game1.random), flipped: false, 0f, Color.White)
					{
						local = true,
						scale = 4f,
						interval = 9999f,
						motion = new Vector2((float)Game1.random.Next(-15, 16) / 10f, -7f + (float)Game1.random.Next(-10, 11) / 10f),
						acceleration = new Vector2(0f, 0.5f)
					});
				}
				this.destroyTimer = 800f;
			}
			this.doIconAction(c.name);
		}
	}

	private void doIconAction(string iconName)
	{
		if (this.which == "dwarfStatue" && !Game1.player.hasBuffWithNameContainingString("dwarfStatue"))
		{
			Game1.player.applyBuff(this.which + "_" + iconName);
		}
	}

	private void flairOnDestroy()
	{
		if (this.which == "dwarfStatue")
		{
			this.sourceObject.shakeTimer = 500;
			if (this.sourceObject.Location != null)
			{
				Utility.addSprinklesToLocation(this.sourceObject.Location, (int)this.sourceObject.TileLocation.X, (int)this.sourceObject.TileLocation.Y, 3, 4, 800, 40, Color.White);
			}
		}
	}

	/// <inheritdoc />
	public override void draw(SpriteBatch b)
	{
		b.Draw(Game1.fadeToBlackRect, new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), Color.Black * 0.7f);
		base.draw(b);
		SpriteText.drawStringWithScrollCenteredAt(b, this.title, base.xPositionOnScreen + base.width / 2, base.yPositionOnScreen + 20, "", 1f, (this.titleStyle == 3) ? Color.LightGray : Game1.textColor, this.titleStyle);
		for (int i = 0; i < this.icons.Count; i++)
		{
			if (this.selected == i || (this.selected == -2 && i == this.icons.Count - 1))
			{
				if (this.selected == i)
				{
					Rectangle rect = this.icons[i].bounds;
					rect.Inflate(2, 4);
					rect.X += this.iconXOffset - 2;
					b.Draw(Game1.staminaRect, rect, Color.Red);
					if (this.icons[i].sourceRect.Width > 0)
					{
						this.icons[i].sourceRect.X = this.iconBackHighlightPosition.X;
						this.icons[i].sourceRect.Y = this.iconBackHighlightPosition.Y;
					}
				}
				else
				{
					b.Draw(Game1.mouseCursors_1_6, this.icons[i].getVector2(), new Rectangle(480, 28, 16, 16), Color.Red, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
				}
			}
			this.icons[i].draw(b, Color.White, 0f, 0, this.iconXOffset);
			this.iconFronts[i].draw(b, this.iconFronts[i].name.Equals("ghosted_fade") ? (Color.Black * 0.4f) : Color.White, 0.87f, 0, this.iconXOffset);
			IClickableMenu.drawHoverText(b, this.icons[i].hoverText, Game1.smallFont, 0, 0, -1, null, -1, null, null, 0, null, -1, this.icons[i].bounds.X + 4, this.icons[i].bounds.Y + this.icons[i].bounds.Height - this.maxTooltipHeight + 4, 1f, null, null, Game1.mouseCursors_1_6, (this.icons[i].sourceRect != this.icons[i].startingSourceRect) ? new Rectangle(111, 145, 15, 15) : new Rectangle(96, 145, 15, 15), Color.White, new Color(26, 26, 43), 4f, this.maxTooltipWidth, this.maxTooltipHeight);
		}
		foreach (TemporaryAnimatedSprite temporarySprite in this.temporarySprites)
		{
			temporarySprite.draw(b);
		}
		base.drawMouse(b);
	}
}
