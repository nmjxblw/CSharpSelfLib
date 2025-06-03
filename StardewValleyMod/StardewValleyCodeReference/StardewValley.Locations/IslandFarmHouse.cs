using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Extensions;
using StardewValley.Objects;
using xTile.Dimensions;
using xTile.Layers;

namespace StardewValley.Locations;

public class IslandFarmHouse : DecoratableLocation
{
	[XmlElement("fridge")]
	public readonly NetRef<Chest> fridge = new NetRef<Chest>(new Chest(playerChest: true));

	public Point fridgePosition;

	public NetBool visited = new NetBool(value: false)
	{
		InterpolationEnabled = false
	};

	private Color nightLightingColor = new Color(180, 180, 0);

	private Color rainLightingColor = new Color(90, 90, 0);

	public IslandFarmHouse()
	{
		this.fridge.Value.Location = this;
	}

	public IslandFarmHouse(string map, string name)
		: base(map, name)
	{
		this.fridge.Value.Location = this;
		base.furniture.Add(ItemRegistry.Create<Furniture>("(F)1798").SetPlacement(12, 8));
		base.furniture.Add(ItemRegistry.Create<Furniture>("(F)1614").SetPlacement(3, 1));
		base.furniture.Add(ItemRegistry.Create<Furniture>("(F)1614").SetPlacement(8, 1));
		base.furniture.Add(ItemRegistry.Create<Furniture>("(F)1614").SetPlacement(20, 1));
		base.furniture.Add(ItemRegistry.Create<Furniture>("(F)1614").SetPlacement(25, 1));
		base.furniture.Add(ItemRegistry.Create<Furniture>("(F)1294").SetPlacement(1, 4));
		base.furniture.Add(ItemRegistry.Create<Furniture>("(F)1294").SetPlacement(10, 4));
		base.furniture.Add(ItemRegistry.Create<Furniture>("(F)1294").SetPlacement(18, 4));
		base.furniture.Add(ItemRegistry.Create<Furniture>("(F)1294").SetPlacement(28, 4));
		base.furniture.Add(ItemRegistry.Create<Furniture>("(F)1742").SetPlacement(20, 4));
		Furniture f = ItemRegistry.Create<Furniture>("(F)1755").SetPlacement(14, 9);
		base.furniture.Add(f);
		this.ReadWallpaperAndFloorTileData();
		base.SetWallpaper("88", "UpperLeft");
		base.SetFloor("23", "UpperLeft");
		base.SetWallpaper("88", "UpperRight");
		base.SetFloor("48", "Kitchen");
		base.SetWallpaper("87", "Kitchen");
		base.SetFloor("52", "UpperRight");
		base.SetWallpaper("87", "BottomRight_Left");
		base.SetFloor("23", "BottomRight");
		base.SetWallpaper("87", "BottomRight_Right");
		this.fridgePosition = default(Point);
	}

	public override void TransferDataFromSavedLocation(GameLocation l)
	{
		IslandFarmHouse fromLocation = (IslandFarmHouse)l;
		this.fridge.Value = fromLocation.fridge.Value;
		this.visited.Value = fromLocation.visited.Value;
		base.TransferDataFromSavedLocation(l);
	}

	public override void UpdateWhenCurrentLocation(GameTime time)
	{
		base.UpdateWhenCurrentLocation(time);
		this.fridge.Value.updateWhenCurrentLocation(time);
	}

	public override List<Microsoft.Xna.Framework.Rectangle> getWalls()
	{
		return new List<Microsoft.Xna.Framework.Rectangle>
		{
			new Microsoft.Xna.Framework.Rectangle(1, 1, 10, 3),
			new Microsoft.Xna.Framework.Rectangle(18, 1, 11, 3),
			new Microsoft.Xna.Framework.Rectangle(12, 5, 5, 2),
			new Microsoft.Xna.Framework.Rectangle(17, 9, 2, 2),
			new Microsoft.Xna.Framework.Rectangle(21, 9, 8, 2)
		};
	}

	protected override void resetLocalState()
	{
		base.resetLocalState();
		if (!this.visited.Value)
		{
			this.visited.Value = true;
		}
		this.fridgePosition = this.GetFridgePositionFromMap() ?? Point.Zero;
	}

	/// <summary>Get the fridge position by scanning the map tiles for the sprite index.</summary>
	/// <remarks>This is relatively expensive. Most code should use the cached <see cref="F:StardewValley.Locations.IslandFarmHouse.fridgePosition" /> instead.</remarks>
	public Point? GetFridgePositionFromMap()
	{
		Layer layer = base.map.RequireLayer("Buildings");
		for (int y = 0; y < layer.LayerHeight; y++)
		{
			for (int x = 0; x < layer.LayerWidth; x++)
			{
				if (layer.GetTileIndexAt(x, y, "untitled tile sheet") == 258)
				{
					return new Point(x, y);
				}
			}
		}
		return null;
	}

	public override List<Microsoft.Xna.Framework.Rectangle> getFloors()
	{
		return new List<Microsoft.Xna.Framework.Rectangle>
		{
			new Microsoft.Xna.Framework.Rectangle(1, 3, 11, 12),
			new Microsoft.Xna.Framework.Rectangle(11, 7, 6, 9),
			new Microsoft.Xna.Framework.Rectangle(18, 3, 11, 6),
			new Microsoft.Xna.Framework.Rectangle(17, 11, 12, 6)
		};
	}

	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(this.fridge, "fridge").AddField(this.visited, "visited");
		this.visited.fieldChangeVisibleEvent += delegate
		{
			this.InitializeBeds();
		};
		this.fridge.fieldChangeEvent += delegate(NetRef<Chest> field, Chest oldValue, Chest newValue)
		{
			newValue.Location = this;
		};
	}

	public virtual void InitializeBeds()
	{
		if (!Game1.IsMasterGame || Game1.gameMode == 6 || !this.visited.Value)
		{
			return;
		}
		int player_count = 0;
		foreach (Farmer allFarmer in Game1.getAllFarmers())
		{
			_ = allFarmer;
			player_count++;
		}
		string bedId = "2176";
		base.furniture.Add(new BedFurniture(bedId, new Vector2(22f, 3f)));
		player_count--;
		if (player_count > 0)
		{
			base.furniture.Add(new BedFurniture(bedId, new Vector2(26f, 3f)));
			player_count--;
		}
		for (int i = 0; i < Math.Min(6, player_count); i++)
		{
			int x = 3;
			int y = 3;
			if (i % 2 == 0)
			{
				x += 4;
			}
			y += i / 2 * 4;
			base.furniture.Add(new BedFurniture(bedId, new Vector2(x, y)));
		}
	}

	protected override void _updateAmbientLighting()
	{
		if (Game1.isStartingToGetDarkOut(this) || base.lightLevel.Value > 0f)
		{
			int time = Game1.timeOfDay + Game1.gameTimeInterval / (Game1.realMilliSecondsPerGameMinute + base.ExtraMillisecondsPerInGameMinute);
			float lerp = 1f - Utility.Clamp((float)Utility.CalculateMinutesBetweenTimes(time, Game1.getTrulyDarkTime(this)) / 120f, 0f, 1f);
			Game1.ambientLight = new Color((byte)Utility.Lerp(Game1.isRaining ? this.rainLightingColor.R : 0, (int)this.nightLightingColor.R, lerp), (byte)Utility.Lerp(Game1.isRaining ? this.rainLightingColor.G : 0, (int)this.nightLightingColor.G, lerp), (byte)Utility.Lerp(0f, (int)this.nightLightingColor.B, lerp));
		}
		else
		{
			Game1.ambientLight = (Game1.isRaining ? this.rainLightingColor : Color.White);
		}
	}

	public override void drawAboveFrontLayer(SpriteBatch b)
	{
		base.drawAboveFrontLayer(b);
		if (this.fridge.Value.mutex.IsLocked())
		{
			b.Draw(Game1.mouseCursors2, Game1.GlobalToLocal(Game1.viewport, new Vector2(this.fridgePosition.X, this.fridgePosition.Y - 1) * 64f), new Microsoft.Xna.Framework.Rectangle(0, 192, 16, 32), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)((this.fridgePosition.Y + 1) * 64 + 1) / 10000f);
		}
	}

	public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
	{
		if (base.getTileIndexAt(tileLocation, "Buildings", "untitled tile sheet") == 258)
		{
			this.fridge.Value.fridge.Value = true;
			this.fridge.Value.checkForAction(who);
			return true;
		}
		return base.checkAction(tileLocation, viewport, who);
	}
}
