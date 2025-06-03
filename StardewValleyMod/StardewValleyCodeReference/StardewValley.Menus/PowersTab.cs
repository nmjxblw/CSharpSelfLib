using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.GameData.Powers;
using StardewValley.TokenizableStrings;

namespace StardewValley.Menus;

public class PowersTab : IClickableMenu
{
	public const int region_forwardButton = 707;

	public const int region_backButton = 706;

	public const int distanceFromMenuBottomBeforeNewPage = 128;

	public int currentPage;

	public string descriptionText = "";

	public string hoverText = "";

	public ClickableTextureComponent backButton;

	public ClickableTextureComponent forwardButton;

	public List<List<ClickableTextureComponent>> powers;

	public PowersTab(int x, int y, int width, int height)
		: base(x, y, width, height)
	{
		this.backButton = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen + 48, base.yPositionOnScreen + height - 80, 48, 44), Game1.mouseCursors, new Rectangle(352, 495, 12, 11), 4f)
		{
			myID = 706,
			rightNeighborID = -7777
		};
		this.forwardButton = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen + width - 32 - 60, base.yPositionOnScreen + height - 80, 48, 44), Game1.mouseCursors, new Rectangle(365, 495, 12, 11), 4f)
		{
			myID = 707,
			leftNeighborID = -7777
		};
	}

	public override void snapToDefaultClickableComponent()
	{
		base.snapToDefaultClickableComponent();
		base.currentlySnappedComponent = base.getComponentWithID(0);
		this.snapCursorToCurrentSnappedComponent();
	}

	public override void populateClickableComponentList()
	{
		if (this.powers == null)
		{
			this.powers = new List<List<ClickableTextureComponent>>();
			Dictionary<string, PowersData> powersData = null;
			try
			{
				powersData = DataLoader.Powers(Game1.content);
			}
			catch (Exception)
			{
			}
			if (powersData != null)
			{
				int collectionWidth = 9;
				int widthUsed = 0;
				int baseX = base.xPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearSideBorder;
				int baseY = base.yPositionOnScreen + IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder - 16;
				foreach (KeyValuePair<string, PowersData> power in powersData)
				{
					int xPos = baseX + widthUsed % collectionWidth * 76;
					int yPos = baseY + widthUsed / collectionWidth * 76;
					bool unlocked = GameStateQuery.CheckConditions(power.Value.UnlockedCondition);
					string name = TokenParser.ParseText(power.Value.DisplayName) ?? power.Key;
					string description = TokenParser.ParseText(power.Value.Description) ?? "";
					Texture2D texture = Game1.content.Load<Texture2D>(power.Value.TexturePath);
					if (this.powers.Count == 0 || yPos > base.yPositionOnScreen + base.height - 128)
					{
						this.powers.Add(new List<ClickableTextureComponent>());
						widthUsed = 0;
						xPos = baseX;
						yPos = baseY;
					}
					List<ClickableTextureComponent> list = this.powers.Last();
					list.Add(new ClickableTextureComponent(power.Key, new Rectangle(xPos, yPos, 64, 64), name, description, texture, new Rectangle(power.Value.TexturePosition.X, power.Value.TexturePosition.Y, 16, 16), 4f, unlocked)
					{
						myID = list.Count,
						rightNeighborID = (((list.Count + 1) % collectionWidth == 0) ? (-1) : (list.Count + 1)),
						leftNeighborID = ((list.Count % collectionWidth == 0) ? (-1) : (list.Count - 1)),
						downNeighborID = ((yPos + 76 > base.yPositionOnScreen + base.height - 128) ? (-7777) : (list.Count + collectionWidth)),
						upNeighborID = ((list.Count < collectionWidth) ? 12346 : (list.Count - collectionWidth)),
						fullyImmutable = true,
						drawLabel = false
					});
					widthUsed++;
				}
			}
		}
		base.populateClickableComponentList();
	}

	/// <inheritdoc />
	public override void performHoverAction(int x, int y)
	{
		this.hoverText = "";
		this.descriptionText = "";
		base.performHoverAction(x, y);
		foreach (ClickableTextureComponent c in this.powers[this.currentPage])
		{
			if (c.containsPoint(x, y))
			{
				c.scale = Math.Min(c.scale + 0.02f, c.baseScale + 0.1f);
				this.hoverText = (c.drawShadow ? c.label : "???");
				this.descriptionText = Game1.parseText(c.hoverText, Game1.smallFont, Math.Max((int)Game1.dialogueFont.MeasureString(this.hoverText).X, 320));
			}
			else
			{
				c.scale = Math.Max(c.scale - 0.02f, c.baseScale);
			}
		}
		this.forwardButton.tryHover(x, y, 0.5f);
		this.backButton.tryHover(x, y, 0.5f);
	}

	/// <inheritdoc />
	public override void receiveLeftClick(int x, int y, bool playSound = true)
	{
		if (this.backButton.containsPoint(x, y) && this.currentPage > 0)
		{
			if (playSound)
			{
				Game1.playSound("shwip");
			}
			this.currentPage--;
		}
		else if (this.forwardButton.containsPoint(x, y) && this.currentPage < this.powers.Count - 1)
		{
			if (playSound)
			{
				Game1.playSound("shwip");
			}
			this.currentPage++;
		}
		else
		{
			base.receiveLeftClick(x, y, playSound);
		}
	}

	/// <inheritdoc />
	public override void draw(SpriteBatch b)
	{
		if (this.currentPage > 0)
		{
			this.backButton.draw(b);
		}
		if (this.currentPage < this.powers.Count - 1)
		{
			this.forwardButton.draw(b);
		}
		b.End();
		b.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp);
		foreach (ClickableTextureComponent item in this.powers[this.currentPage])
		{
			bool drawColor = item.drawShadow;
			item.draw(b, drawColor ? Color.White : (Color.Black * 0.2f), 0.86f);
		}
		b.End();
		b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
		if (!this.descriptionText.Equals("") && this.hoverText != "???")
		{
			IClickableMenu.drawHoverText(b, this.descriptionText, Game1.smallFont, 0, 0, -1, this.hoverText);
		}
		else if (!this.hoverText.Equals(""))
		{
			IClickableMenu.drawHoverText(b, this.hoverText, Game1.smallFont);
		}
	}
}
