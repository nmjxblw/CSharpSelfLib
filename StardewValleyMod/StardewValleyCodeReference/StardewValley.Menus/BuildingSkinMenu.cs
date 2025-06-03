using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using StardewValley.Buildings;
using StardewValley.GameData.Buildings;
using StardewValley.TokenizableStrings;

namespace StardewValley.Menus;

public class BuildingSkinMenu : IClickableMenu
{
	/// <summary>Metadata for a skin shown in the menu.</summary>
	public class SkinEntry
	{
		/// <summary>The index of the skin in the menu's list.</summary>
		public int Index;

		/// <summary>The skin ID in <c>Data/Buildings</c>.</summary>
		public readonly string Id;

		/// <summary>The translated display name.</summary>
		public readonly string DisplayName;

		/// <summary>The translated description.</summary>
		public readonly string Description;

		/// <summary>The skin data from <c>Data/Buildings</c>.</summary>
		public readonly BuildingSkin Data;

		/// <summary>Construct an instance.</summary>
		/// <param name="index">The index of the skin in the menu's list.</param>
		/// <param name="skin">The skin ID in <c>Data/Buildings</c>.</param>
		public SkinEntry(int index, BuildingSkin skin)
			: this(index, skin, TokenParser.ParseText(skin.Name), TokenParser.ParseText(skin.Description))
		{
		}

		/// <summary>Construct an instance.</summary>
		/// <param name="index">The index of the skin in the menu's list.</param>
		/// <param name="skin">The skin data from <c>Data/Buildings</c>.</param>
		/// <param name="displayName">The translated display name.</param>
		/// <param name="description">The translated description.</param>
		public SkinEntry(int index, BuildingSkin skin, string displayName, string description)
		{
			this.Index = index;
			this.Id = skin?.Id;
			this.Data = skin;
			this.DisplayName = displayName;
			this.Description = description;
		}
	}

	public const int region_okButton = 101;

	public const int region_nextSkin = 102;

	public const int region_prevSkin = 103;

	public static int WindowWidth = 576;

	public static int WindowHeight = 576;

	public Rectangle PreviewPane;

	public ClickableTextureComponent OkButton;

	/// <summary>The building whose skin to change.</summary>
	public Building Building;

	public ClickableTextureComponent NextSkinButton;

	public ClickableTextureComponent PreviousSkinButton;

	public string BuildingDisplayName;

	public string BuildingDescription;

	/// <summary>The building skins available in the menu.</summary>
	public List<SkinEntry> Skins = new List<SkinEntry>();

	/// <summary>The current building skin shown in the menu.</summary>
	public SkinEntry Skin;

	/// <summary>Construct an instance.</summary>
	/// <param name="targetBuilding">The building whose skin to change.</param>
	/// <param name="ignoreSeparateConstructionEntries">Whether to ignore skins with <see cref="F:StardewValley.GameData.Buildings.BuildingSkin.ShowAsSeparateConstructionEntry" /> set to true.</param>
	public BuildingSkinMenu(Building targetBuilding, bool ignoreSeparateConstructionEntries = false)
		: base(Game1.uiViewport.Width / 2 - BuildingSkinMenu.WindowWidth / 2, Game1.uiViewport.Height / 2 - BuildingSkinMenu.WindowHeight / 2, BuildingSkinMenu.WindowWidth, BuildingSkinMenu.WindowHeight)
	{
		Game1.player.Halt();
		this.Building = targetBuilding;
		BuildingData buildingData = targetBuilding.GetData();
		this.BuildingDisplayName = TokenParser.ParseText(buildingData.Name);
		this.BuildingDescription = TokenParser.ParseText(buildingData.Description);
		int index = 0;
		this.Skins.Add(new SkinEntry(index++, null, this.BuildingDisplayName, this.BuildingDescription));
		if (buildingData.Skins != null)
		{
			foreach (BuildingSkin skin in buildingData.Skins)
			{
				if (!(skin.Id != this.Building.skinId.Value) || ((!ignoreSeparateConstructionEntries || !skin.ShowAsSeparateConstructionEntry) && GameStateQuery.CheckConditions(skin.Condition, this.Building.GetParentLocation())))
				{
					this.Skins.Add(new SkinEntry(index++, skin));
				}
			}
		}
		this.RepositionElements();
		this.SetSkin(Math.Max(this.Skins.FindIndex((SkinEntry skinEntry) => skinEntry.Id == this.Building.skinId.Value), 0));
		this.populateClickableComponentList();
		if (Game1.options.SnappyMenus)
		{
			this.snapToDefaultClickableComponent();
		}
	}

	public override void snapToDefaultClickableComponent()
	{
		base.currentlySnappedComponent = base.getComponentWithID(101);
		this.snapCursorToCurrentSnappedComponent();
	}

	/// <inheritdoc />
	public override void receiveGamePadButton(Buttons button)
	{
		switch (button)
		{
		case Buttons.RightTrigger:
			Game1.playSound("shwip");
			this.SetSkin(this.Skin.Index + 1);
			break;
		case Buttons.LeftTrigger:
			Game1.playSound("shwip");
			this.SetSkin(this.Skin.Index - 1);
			break;
		}
		base.receiveGamePadButton(button);
	}

	/// <inheritdoc />
	public override void receiveLeftClick(int x, int y, bool playSound = true)
	{
		if (this.OkButton.containsPoint(x, y))
		{
			base.exitThisMenu(playSound);
		}
		else if (this.PreviousSkinButton.containsPoint(x, y))
		{
			Game1.playSound("shwip");
			this.SetSkin(this.Skin.Index - 1);
		}
		else if (this.NextSkinButton.containsPoint(x, y))
		{
			this.SetSkin(this.Skin.Index + 1);
			Game1.playSound("shwip");
		}
		else
		{
			base.receiveLeftClick(x, y, playSound);
		}
	}

	public void SetSkin(int index)
	{
		if (this.Skins.Count == 0)
		{
			this.SetSkin(null);
			return;
		}
		index %= this.Skins.Count;
		if (index < 0)
		{
			index = this.Skins.Count + index;
		}
		this.SetSkin(this.Skins[index]);
	}

	public virtual void SetSkin(SkinEntry skin)
	{
		this.Skin = skin;
		if (this.Building.skinId.Value != skin.Id)
		{
			this.Building.skinId.Value = skin.Id;
			this.Building.netBuildingPaintColor.Value.Color1Default.Value = true;
			this.Building.netBuildingPaintColor.Value.Color2Default.Value = true;
			this.Building.netBuildingPaintColor.Value.Color3Default.Value = true;
			BuildingData buildingData = this.Building.GetData();
			if (buildingData != null && this.Building.daysOfConstructionLeft.Value == buildingData.BuildDays)
			{
				this.Building.daysOfConstructionLeft.Value = skin.Data?.BuildDays ?? buildingData.BuildDays;
			}
		}
	}

	/// <inheritdoc />
	public override void performHoverAction(int x, int y)
	{
		this.OkButton.tryHover(x, y);
		this.PreviousSkinButton.tryHover(x, y);
		this.NextSkinButton.tryHover(x, y);
	}

	public virtual void RepositionElements()
	{
		this.PreviewPane.Y = base.yPositionOnScreen + 48;
		this.PreviewPane.Width = 576;
		this.PreviewPane.Height = 576;
		this.PreviewPane.X = base.xPositionOnScreen + base.width / 2 - this.PreviewPane.Width / 2;
		Rectangle panelRectangle = this.PreviewPane;
		panelRectangle.Inflate(-16, -16);
		this.PreviousSkinButton = new ClickableTextureComponent(new Rectangle(panelRectangle.Left, panelRectangle.Center.Y - 32, 64, 64), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 44), 1f)
		{
			myID = 103,
			leftNeighborID = -99998,
			rightNeighborID = -99998,
			downNeighborID = 101,
			upNeighborID = -99998,
			fullyImmutable = true
		};
		this.NextSkinButton = new ClickableTextureComponent(new Rectangle(panelRectangle.Right - 64, panelRectangle.Center.Y - 32, 64, 64), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 33), 1f)
		{
			myID = 102,
			leftNeighborID = -99998,
			rightNeighborID = -99998,
			downNeighborID = 101,
			upNeighborID = -99998,
			fullyImmutable = true
		};
		panelRectangle.Y += 64;
		panelRectangle.Height = 0;
		panelRectangle.Y += 80;
		panelRectangle.Y += 64;
		this.OkButton = new ClickableTextureComponent(new Rectangle(this.PreviewPane.Right - 64 - 16, this.PreviewPane.Bottom - 64 - 16, 64, 64), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46), 1f)
		{
			myID = 101,
			upNeighborID = 102
		};
		if (this.Skins.Count == 0)
		{
			this.NextSkinButton.visible = false;
			this.PreviousSkinButton.visible = false;
		}
		this.populateClickableComponentList();
	}

	public virtual bool SaveColor()
	{
		return true;
	}

	public virtual void SetRegion(int newRegion)
	{
		this.RepositionElements();
	}

	/// <inheritdoc />
	public override void draw(SpriteBatch b)
	{
		if (!Game1.options.showClearBackgrounds)
		{
			b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
		}
		Game1.DrawBox(this.PreviewPane.X, this.PreviewPane.Y, this.PreviewPane.Width, this.PreviewPane.Height);
		Rectangle rectangle = this.PreviewPane;
		rectangle.Inflate(0, 0);
		b.End();
		b.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, null, Utility.ScissorEnabled);
		b.GraphicsDevice.ScissorRectangle = rectangle;
		Vector2 buildingDrawCenter = new Vector2(this.PreviewPane.X + this.PreviewPane.Width / 2, this.PreviewPane.Y + this.PreviewPane.Height / 2 - 16);
		Rectangle sourceRect = this.Building.getSourceRectForMenu() ?? this.Building.getSourceRect();
		this.Building?.drawInMenu(b, (int)buildingDrawCenter.X - (int)((float)this.Building.tilesWide.Value / 2f * 64f), (int)buildingDrawCenter.Y - sourceRect.Height * 4 / 2);
		b.End();
		b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
		SpriteText.drawStringWithScrollCenteredAt(b, Game1.content.LoadString("Strings\\Buildings:BuildingSkinMenu_ChooseAppearance", this.BuildingDisplayName), base.xPositionOnScreen + base.width / 2, this.PreviewPane.Top - 96);
		this.OkButton.draw(b);
		this.NextSkinButton.draw(b);
		this.PreviousSkinButton.draw(b);
		base.drawMouse(b);
	}
}
