using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using StardewValley.Extensions;
using StardewValley.WorldMaps;

namespace StardewValley.Menus;

/// <summary>The in-game world map view.</summary>
public class MapPage : IClickableMenu
{
	/// <summary>The world map debug lines to draw.</summary>
	[Flags]
	public enum WorldMapDebugLineType
	{
		/// <summary>Don't show debug lines on the map.</summary>
		None = 0,
		/// <summary>Highlight map areas.</summary>
		Areas = 1,
		/// <summary>Highlight map position rectangles.</summary>
		Positions = 2,
		/// <summary>Highlight tooltip rectangles.</summary>
		Tooltips = 4,
		/// <summary>Highlight all types.</summary>
		All = -1
	}

	/// <summary>The world map debug lines to draw, if any.</summary>
	public static WorldMapDebugLineType EnableDebugLines;

	/// <summary>The map position containing the current player.</summary>
	public readonly MapAreaPositionWithContext? mapPosition;

	/// <summary>The map region containing the <see cref="F:StardewValley.Menus.MapPage.mapPosition" />.</summary>
	public readonly MapRegion mapRegion;

	/// <summary>The smaller sections of the map linked to one or more in-game locations. Each map area might be edited/swapped depending on the context, have its own tooltip(s), or have its own player marker positions.</summary>
	public readonly MapArea[] mapAreas;

	/// <summary>The translated scroll text to show at the bottom of the map, if any.</summary>
	public readonly string scrollText;

	/// <summary>The default component ID in <see cref="F:StardewValley.Menus.MapPage.points" /> to which to snap the controller cursor by default.</summary>
	public readonly int defaultComponentID;

	/// <summary>The pixel area on screen containing all the map areas being drawn.</summary>
	public Rectangle mapBounds;

	/// <summary>The tooltips to render, indexed by <see cref="P:StardewValley.WorldMaps.MapAreaTooltip.NamespacedId" />.</summary>
	public readonly Dictionary<string, ClickableComponent> points = new Dictionary<string, ClickableComponent>(StringComparer.OrdinalIgnoreCase);

	/// <summary>The tooltip text being drawn.</summary>
	public string hoverText = "";

	public MapPage(int x, int y, int width, int height)
		: base(x, y, width, height)
	{
		WorldMapManager.ReloadData();
		Point playerTile = this.GetNormalizedPlayerTile(Game1.player);
		this.mapPosition = WorldMapManager.GetPositionData(Game1.player.currentLocation, playerTile) ?? WorldMapManager.GetPositionData(Game1.getFarm(), Point.Zero);
		this.mapRegion = this.mapPosition?.Data.Region ?? WorldMapManager.GetMapRegions().First();
		this.mapAreas = this.mapRegion.GetAreas();
		this.scrollText = this.mapPosition?.Data.GetScrollText(playerTile);
		this.mapBounds = this.mapRegion.GetMapPixelBounds();
		int id = (this.defaultComponentID = 1000);
		MapArea[] array = this.mapAreas;
		for (int i = 0; i < array.Length; i++)
		{
			MapAreaTooltip[] tooltips = array[i].GetTooltips();
			foreach (MapAreaTooltip tooltip in tooltips)
			{
				Rectangle pixelArea = tooltip.GetPixelArea();
				pixelArea = new Rectangle(this.mapBounds.X + pixelArea.X, this.mapBounds.Y + pixelArea.Y, pixelArea.Width, pixelArea.Height);
				id++;
				ClickableComponent component = new ClickableComponent(pixelArea, tooltip.NamespacedId)
				{
					myID = id,
					label = tooltip.Text
				};
				this.points[tooltip.NamespacedId] = component;
				if (tooltip.NamespacedId == "Farm/Default")
				{
					this.defaultComponentID = id;
				}
			}
		}
		array = this.mapAreas;
		for (int i = 0; i < array.Length; i++)
		{
			MapAreaTooltip[] tooltips = array[i].GetTooltips();
			foreach (MapAreaTooltip tooltip2 in tooltips)
			{
				if (this.points.TryGetValue(tooltip2.NamespacedId, out var component2))
				{
					this.SetNeighborId(component2, "left", tooltip2.Data.LeftNeighbor);
					this.SetNeighborId(component2, "right", tooltip2.Data.RightNeighbor);
					this.SetNeighborId(component2, "up", tooltip2.Data.UpNeighbor);
					this.SetNeighborId(component2, "down", tooltip2.Data.DownNeighbor);
				}
			}
		}
	}

	public override void populateClickableComponentList()
	{
		base.populateClickableComponentList();
		base.allClickableComponents.AddRange(this.points.Values);
	}

	/// <summary>Set a controller navigation ID for a tooltip component.</summary>
	/// <param name="component">The tooltip component whose neighbor ID to set.</param>
	/// <param name="direction">The direction to set.</param>
	/// <param name="neighborKeys">The tooltip neighbor keys to match. See remarks on <see cref="F:StardewValley.GameData.WorldMaps.WorldMapTooltipData.LeftNeighbor" /> for details on the format.</param>
	/// <returns>Returns whether the <paramref name="neighborKeys" /> matched an existing tooltip neighbor ID.</returns>
	public void SetNeighborId(ClickableComponent component, string direction, string neighborKeys)
	{
		if (string.IsNullOrWhiteSpace(neighborKeys))
		{
			return;
		}
		if (!this.TryGetNeighborId(neighborKeys, out var neighborId, out var foundIgnore))
		{
			if (!foundIgnore)
			{
				Game1.log.Warn($"World map tooltip '{component.name}' has {direction} neighbor keys '{neighborKeys}' which don't match a tooltip namespaced ID or alias.");
			}
			return;
		}
		switch (direction)
		{
		case "left":
			component.leftNeighborID = neighborId;
			break;
		case "right":
			component.rightNeighborID = neighborId;
			break;
		case "up":
			component.upNeighborID = neighborId;
			break;
		case "down":
			component.downNeighborID = neighborId;
			break;
		default:
			Game1.log.Warn("Can't set neighbor ID for unknown direction '" + direction + "'.");
			break;
		}
	}

	/// <summary>Get the controller navigation ID for a tooltip neighbor field value.</summary>
	/// <param name="keys">The tooltip neighbor keys to match. See remarks on <see cref="F:StardewValley.GameData.WorldMaps.WorldMapTooltipData.LeftNeighbor" /> for details on the format.</param>
	/// <param name="id">The matching controller navigation ID, if found.</param>
	/// <param name="foundIgnore">Whether the neighbor IDs contains <c>ignore</c>, which indicates it should be skipped silently if none match.</param>
	/// <param name="isAlias">Whether the <paramref name="keys" /> are from an alias in <see cref="F:StardewValley.GameData.WorldMaps.WorldMapRegionData.MapNeighborIdAliases" />.</param>
	/// <returns>Returns <c>true</c> if the neighbor ID was found, else <c>false</c>.</returns>
	public bool TryGetNeighborId(string keys, out int id, out bool foundIgnore, bool isAlias = false)
	{
		foundIgnore = false;
		if (!string.IsNullOrWhiteSpace(keys))
		{
			string[] array = keys.Split(',', StringSplitOptions.RemoveEmptyEntries);
			for (int i = 0; i < array.Length; i++)
			{
				string key = array[i].Trim();
				if (key.EqualsIgnoreCase("ignore"))
				{
					foundIgnore = true;
					continue;
				}
				if (this.points.TryGetValue(key, out var neighbor))
				{
					id = neighbor.myID;
					return true;
				}
				if (!isAlias && this.mapRegion.Data.MapNeighborIdAliases.TryGetValue(key, out var alias))
				{
					if (this.TryGetNeighborId(alias, out id, out var localFoundIgnore, isAlias: true))
					{
						foundIgnore |= localFoundIgnore;
						return true;
					}
					foundIgnore |= localFoundIgnore;
				}
			}
		}
		id = -1;
		return false;
	}

	public override void snapToDefaultClickableComponent()
	{
		base.currentlySnappedComponent = base.getComponentWithID(this.defaultComponentID);
		this.snapCursorToCurrentSnappedComponent();
	}

	/// <inheritdoc />
	public override void receiveLeftClick(int x, int y, bool playSound = true)
	{
		foreach (ClickableComponent c in this.points.Values)
		{
			if (!c.containsPoint(x, y))
			{
				continue;
			}
			string name = c.name;
			if (!(name == "Beach/LonelyStone"))
			{
				if (name == "Forest/SewerPipe")
				{
					Game1.playSound("shadowpeep");
				}
			}
			else
			{
				Game1.playSound("stoneCrack");
			}
			return;
		}
		if (Game1.activeClickableMenu is GameMenu gameMenu)
		{
			gameMenu.changeTab(gameMenu.lastOpenedNonMapTab);
		}
	}

	/// <inheritdoc />
	public override void performHoverAction(int x, int y)
	{
		this.hoverText = "";
		foreach (ClickableComponent c in this.points.Values)
		{
			if (c.containsPoint(x, y))
			{
				this.hoverText = c.label;
				break;
			}
		}
	}

	/// <inheritdoc />
	public override void draw(SpriteBatch b)
	{
		this.drawMap(b);
		this.drawMiniPortraits(b);
		this.drawScroll(b);
		this.drawTooltip(b);
	}

	/// <inheritdoc />
	public override void receiveKeyPress(Keys key)
	{
		if (Game1.options.doesInputListContain(Game1.options.mapButton, key) && this.readyToClose())
		{
			base.exitThisMenu();
		}
		base.receiveKeyPress(key);
	}

	public virtual void drawMiniPortraits(SpriteBatch b, float alpha = 1f)
	{
		Dictionary<Vector2, int> usedPositions = new Dictionary<Vector2, int>();
		foreach (Farmer player in Game1.getOnlineFarmers())
		{
			Point tile = this.GetNormalizedPlayerTile(player);
			MapAreaPositionWithContext? positionData = (player.IsLocalPlayer ? this.mapPosition : WorldMapManager.GetPositionData(player.currentLocation, tile));
			if (positionData.HasValue && !(positionData.Value.Data.Region.Id != this.mapRegion.Id))
			{
				Vector2 pos = positionData.Value.GetMapPixelPosition();
				pos = new Vector2(pos.X + (float)this.mapBounds.X - 32f, pos.Y + (float)this.mapBounds.Y - 32f);
				usedPositions.TryGetValue(pos, out var count);
				usedPositions[pos] = count + 1;
				if (count > 0)
				{
					pos += new Vector2(48 * (count % 2), 48 * (count / 2));
				}
				player.FarmerRenderer.drawMiniPortrat(b, pos, 0.00011f, 4f, 2, player, alpha);
			}
		}
	}

	public virtual void drawScroll(SpriteBatch b)
	{
		if (this.scrollText != null)
		{
			float scrollDrawY = base.yPositionOnScreen + base.height + 32 + 4;
			float scrollDrawBottom = scrollDrawY + 80f;
			if (scrollDrawBottom > (float)Game1.uiViewport.Height)
			{
				scrollDrawY -= scrollDrawBottom - (float)Game1.uiViewport.Height;
			}
			SpriteText.drawStringWithScrollCenteredAt(b, this.scrollText, base.xPositionOnScreen + base.width / 2, (int)scrollDrawY);
		}
	}

	public virtual void drawMap(SpriteBatch b, bool drawBorders = true, float alpha = 1f)
	{
		if (drawBorders)
		{
			int boxY = this.mapBounds.Y - 96;
			Game1.drawDialogueBox(this.mapBounds.X - 32, boxY, (this.mapBounds.Width + 16) * 4, (this.mapBounds.Height + 32) * 4, speaker: false, drawOnlyBox: true);
		}
		float sortLayer = 0.86f;
		MapAreaTexture baseTexture = this.mapRegion.GetBaseTexture();
		if (baseTexture != null)
		{
			Rectangle destRect = baseTexture.GetOffsetMapPixelArea(this.mapBounds.X, this.mapBounds.Y);
			b.Draw(baseTexture.Texture, destRect, baseTexture.SourceRect, Color.White * alpha, 0f, Vector2.Zero, SpriteEffects.None, sortLayer);
			sortLayer += 0.001f;
		}
		MapArea[] array = this.mapAreas;
		for (int i = 0; i < array.Length; i++)
		{
			MapAreaTexture[] textures = array[i].GetTextures();
			foreach (MapAreaTexture overlay in textures)
			{
				Rectangle destRect2 = overlay.GetOffsetMapPixelArea(this.mapBounds.X, this.mapBounds.Y);
				b.Draw(overlay.Texture, destRect2, overlay.SourceRect, Color.White * alpha, 0f, Vector2.Zero, SpriteEffects.None, sortLayer);
				sortLayer += 0.001f;
			}
		}
		if (MapPage.EnableDebugLines == WorldMapDebugLineType.None)
		{
			return;
		}
		array = this.mapAreas;
		foreach (MapArea area in array)
		{
			if (MapPage.EnableDebugLines.HasFlag(WorldMapDebugLineType.Tooltips))
			{
				MapAreaTooltip[] tooltips = area.GetTooltips();
				for (int j = 0; j < tooltips.Length; j++)
				{
					Rectangle pixelArea = tooltips[j].GetPixelArea();
					pixelArea = new Rectangle(this.mapBounds.X + pixelArea.X, this.mapBounds.Y + pixelArea.Y, pixelArea.Width, pixelArea.Height);
					Utility.DrawSquare(b, pixelArea, 2, Color.Blue * alpha);
				}
			}
			if (MapPage.EnableDebugLines.HasFlag(WorldMapDebugLineType.Areas))
			{
				Rectangle pixelArea2 = area.Data.PixelArea;
				if (pixelArea2.Width > 0 || pixelArea2.Height > 0)
				{
					pixelArea2 = new Rectangle(this.mapBounds.X + pixelArea2.X * 4, this.mapBounds.Y + pixelArea2.Y * 4, pixelArea2.Width * 4, pixelArea2.Height * 4);
					Utility.DrawSquare(b, pixelArea2, 4, Color.Black * alpha);
				}
			}
			if (!MapPage.EnableDebugLines.HasFlag(WorldMapDebugLineType.Positions))
			{
				continue;
			}
			foreach (MapAreaPosition worldPosition in area.GetWorldPositions())
			{
				Rectangle pixelArea3 = worldPosition.GetPixelArea();
				pixelArea3 = new Rectangle(this.mapBounds.X + pixelArea3.X, this.mapBounds.Y + pixelArea3.Y, pixelArea3.Width, pixelArea3.Height);
				Utility.DrawSquare(b, pixelArea3, 2, Color.Red * alpha);
			}
		}
	}

	public virtual void drawTooltip(SpriteBatch b)
	{
		if (!string.IsNullOrEmpty(this.hoverText))
		{
			IClickableMenu.drawHoverText(b, this.hoverText, Game1.smallFont);
		}
	}

	/// <summary>Get the tile coordinate for a player, with negative values snapped to zero.</summary>
	/// <param name="player">The player instance.</param>
	public Point GetNormalizedPlayerTile(Farmer player)
	{
		Point tile = player.TilePoint;
		if (tile.X < 0 || tile.Y < 0)
		{
			tile = new Point(Math.Max(0, tile.X), Math.Max(0, tile.Y));
		}
		return tile;
	}
}
