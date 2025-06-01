using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Extensions;
using StardewValley.GameData;
using StardewValley.Menus;
using StardewValley.Network;
using StardewValley.Objects;
using xTile;
using xTile.Dimensions;
using xTile.Layers;
using xTile.Tiles;

namespace StardewValley.Locations;

public class DecoratableLocation : GameLocation
{
	/// <summary>Obsolete.</summary>
	public readonly DecorationFacade wallPaper = new DecorationFacade();

	[XmlIgnore]
	public readonly NetStringList wallpaperIDs = new NetStringList();

	public readonly NetStringDictionary<string, NetString> appliedWallpaper = new NetStringDictionary<string, NetString>
	{
		InterpolationWait = false
	};

	[XmlIgnore]
	public readonly Dictionary<string, List<Vector3>> wallpaperTiles = new Dictionary<string, List<Vector3>>();

	/// <summary>Obsolete.</summary>
	public readonly DecorationFacade floor = new DecorationFacade();

	[XmlIgnore]
	public readonly NetStringList floorIDs = new NetStringList();

	public readonly NetStringDictionary<string, NetString> appliedFloor = new NetStringDictionary<string, NetString>
	{
		InterpolationWait = false
	};

	[XmlIgnore]
	public readonly Dictionary<string, List<Vector3>> floorTiles = new Dictionary<string, List<Vector3>>();

	protected Dictionary<string, TileSheet> _wallAndFloorTileSheets = new Dictionary<string, TileSheet>();

	protected Map _wallAndFloorTileSheetMap;

	/// <summary>Whether to log troubleshooting warnings for wallpaper and flooring issues.</summary>
	public static bool LogTroubleshootingInfo;

	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(this.appliedWallpaper, "appliedWallpaper").AddField(this.appliedFloor, "appliedFloor").AddField(this.floorIDs, "floorIDs")
			.AddField(this.wallpaperIDs, "wallpaperIDs");
		this.appliedWallpaper.OnValueAdded += delegate(string key, string value)
		{
			this.UpdateWallpaper(key);
		};
		this.appliedWallpaper.OnConflictResolve += delegate(string key, NetString rejected, NetString accepted)
		{
			this.UpdateWallpaper(key);
		};
		this.appliedWallpaper.OnValueTargetUpdated += delegate(string key, string old_value, string new_value)
		{
			if (this.appliedWallpaper.FieldDict.TryGetValue(key, out var value))
			{
				value.CancelInterpolation();
			}
			this.UpdateWallpaper(key);
		};
		this.appliedFloor.OnValueAdded += delegate(string key, string value)
		{
			this.UpdateFloor(key);
		};
		this.appliedFloor.OnConflictResolve += delegate(string key, NetString rejected, NetString accepted)
		{
			this.UpdateFloor(key);
		};
		this.appliedFloor.OnValueTargetUpdated += delegate(string key, string old_value, string new_value)
		{
			if (this.appliedFloor.FieldDict.TryGetValue(key, out var value))
			{
				value.CancelInterpolation();
			}
			this.UpdateFloor(key);
		};
	}

	public DecoratableLocation()
	{
	}

	public DecoratableLocation(string mapPath, string name)
		: base(mapPath, name)
	{
	}

	public override void updateLayout()
	{
		base.updateLayout();
		if (Game1.IsMasterGame)
		{
			this.setWallpapers();
			this.setFloors();
		}
	}

	public virtual void ReadWallpaperAndFloorTileData()
	{
		this.updateMap();
		this.wallpaperTiles.Clear();
		this.floorTiles.Clear();
		this.wallpaperIDs.Clear();
		this.floorIDs.Clear();
		string defaultWallpaper = "0";
		string defaultFlooring = "0";
		if (this is FarmHouse { upgradeLevel: <3 })
		{
			Farm farm = Game1.getLocationFromName("Farm", isStructure: false) as Farm;
			defaultWallpaper = FarmHouse.GetStarterWallpaper(farm) ?? "0";
			defaultFlooring = FarmHouse.GetStarterFlooring(farm) ?? "0";
		}
		Dictionary<string, string> initial_values = new Dictionary<string, string>();
		if (base.TryGetMapProperty("WallIDs", out var wallProperty))
		{
			string[] array = wallProperty.Split(',');
			for (int i = 0; i < array.Length; i++)
			{
				string[] data_split = ArgUtility.SplitBySpace(array[i]);
				if (data_split.Length >= 1)
				{
					this.wallpaperIDs.Add(data_split[0]);
				}
				if (data_split.Length >= 2)
				{
					initial_values[data_split[0]] = data_split[1];
				}
			}
		}
		if (this.wallpaperIDs.Count == 0)
		{
			List<Microsoft.Xna.Framework.Rectangle> walls = this.getWalls();
			for (int j = 0; j < walls.Count; j++)
			{
				string id = "Wall_" + j;
				this.wallpaperIDs.Add(id);
				Microsoft.Xna.Framework.Rectangle rect = walls[j];
				if (!this.wallpaperTiles.ContainsKey(j.ToString()))
				{
					this.wallpaperTiles[id] = new List<Vector3>();
				}
				foreach (Point tile in rect.GetPoints())
				{
					this.wallpaperTiles[id].Add(new Vector3(tile.X, tile.Y, tile.Y - rect.Top));
				}
			}
		}
		else
		{
			for (int x = 0; x < base.map.Layers[0].LayerWidth; x++)
			{
				for (int y = 0; y < base.map.Layers[0].LayerHeight; y++)
				{
					string tile_property = this.doesTileHaveProperty(x, y, "WallID", "Back");
					if (tile_property == null)
					{
						continue;
					}
					if (!this.wallpaperIDs.Contains(tile_property))
					{
						this.wallpaperIDs.Add(tile_property);
					}
					if (this.appliedWallpaper.TryAdd(tile_property, defaultWallpaper) && initial_values.TryGetValue(tile_property, out var initial_value))
					{
						if (this.appliedWallpaper.TryGetValue(initial_value, out var newValue))
						{
							this.appliedWallpaper[tile_property] = newValue;
						}
						else if (this.GetWallpaperSource(initial_value).Value >= 0)
						{
							this.appliedWallpaper[tile_property] = initial_value;
						}
					}
					if (!this.wallpaperTiles.TryGetValue(tile_property, out var areas))
					{
						areas = (this.wallpaperTiles[tile_property] = new List<Vector3>());
					}
					areas.Add(new Vector3(x, y, 0f));
					if (this.IsFloorableOrWallpaperableTile(x, y + 1, "Back"))
					{
						areas.Add(new Vector3(x, y + 1, 1f));
					}
					if (this.IsFloorableOrWallpaperableTile(x, y + 2, "Buildings"))
					{
						areas.Add(new Vector3(x, y + 2, 2f));
					}
					else if (this.IsFloorableOrWallpaperableTile(x, y + 2, "Back") && !this.IsFloorableTile(x, y + 2, "Back"))
					{
						areas.Add(new Vector3(x, y + 2, 2f));
					}
				}
			}
		}
		initial_values.Clear();
		if (base.TryGetMapProperty("FloorIDs", out var floorProperty))
		{
			string[] array = floorProperty.Split(',');
			for (int i = 0; i < array.Length; i++)
			{
				string[] data_split2 = ArgUtility.SplitBySpace(array[i]);
				if (data_split2.Length >= 1)
				{
					this.floorIDs.Add(data_split2[0]);
				}
				if (data_split2.Length >= 2)
				{
					initial_values[data_split2[0]] = data_split2[1];
				}
			}
		}
		if (this.floorIDs.Count == 0)
		{
			List<Microsoft.Xna.Framework.Rectangle> floors = this.getFloors();
			for (int k = 0; k < floors.Count; k++)
			{
				string id2 = "Floor_" + k;
				this.floorIDs.Add(id2);
				Microsoft.Xna.Framework.Rectangle rect2 = floors[k];
				if (!this.floorTiles.ContainsKey(k.ToString()))
				{
					this.floorTiles[id2] = new List<Vector3>();
				}
				foreach (Point tile2 in rect2.GetPoints())
				{
					this.floorTiles[id2].Add(new Vector3(tile2.X, tile2.Y, 0f));
				}
			}
		}
		else
		{
			for (int l = 0; l < base.map.Layers[0].LayerWidth; l++)
			{
				for (int m = 0; m < base.map.Layers[0].LayerHeight; m++)
				{
					string tile_property2 = this.doesTileHaveProperty(l, m, "FloorID", "Back");
					if (tile_property2 == null)
					{
						continue;
					}
					if (!this.floorIDs.Contains(tile_property2))
					{
						this.floorIDs.Add(tile_property2);
					}
					if (this.appliedFloor.TryAdd(tile_property2, defaultFlooring) && initial_values.TryGetValue(tile_property2, out var initial_value2))
					{
						if (this.appliedFloor.TryGetValue(initial_value2, out var newValue2))
						{
							this.appliedFloor[tile_property2] = newValue2;
						}
						else if (this.GetFloorSource(initial_value2).Value >= 0)
						{
							this.appliedFloor[tile_property2] = initial_value2;
						}
					}
					if (!this.floorTiles.TryGetValue(tile_property2, out var areas2))
					{
						areas2 = (this.floorTiles[tile_property2] = new List<Vector3>());
					}
					areas2.Add(new Vector3(l, m, 0f));
				}
			}
		}
		this.setFloors();
		this.setWallpapers();
	}

	public virtual TileSheet GetWallAndFloorTilesheet(string id)
	{
		if (base.map != this._wallAndFloorTileSheetMap)
		{
			this._wallAndFloorTileSheets.Clear();
			this._wallAndFloorTileSheetMap = base.map;
		}
		if (this._wallAndFloorTileSheets.TryGetValue(id, out var wallAndFloorTilesheet))
		{
			return wallAndFloorTilesheet;
		}
		try
		{
			foreach (ModWallpaperOrFlooring entry in DataLoader.AdditionalWallpaperFlooring(Game1.content))
			{
				if (!(entry.Id != id))
				{
					Texture2D texture = Game1.content.Load<Texture2D>(entry.Texture);
					if (texture.Width != 256)
					{
						Game1.log.Warn($"The tilesheet for wallpaper/floor '{entry.Id}' is {texture.Width} pixels wide, but it must be exactly {256} pixels wide.");
					}
					TileSheet tilesheet = new TileSheet("x_WallsAndFloors_" + id, base.map, entry.Texture, new Size(texture.Width / 16, texture.Height / 16), new Size(16, 16));
					base.map.AddTileSheet(tilesheet);
					base.map.LoadTileSheets(Game1.mapDisplayDevice);
					this._wallAndFloorTileSheets[id] = tilesheet;
					return tilesheet;
				}
			}
			Game1.log.Error("The tilesheet for wallpaper/floor '" + id + "' could not be loaded: no such ID found in Data/AdditionalWallpaperFlooring.");
			this._wallAndFloorTileSheets[id] = null;
			return null;
		}
		catch (Exception exception)
		{
			Game1.log.Error("The tilesheet for wallpaper/floor '" + id + "' could not be loaded.", exception);
			this._wallAndFloorTileSheets[id] = null;
			return null;
		}
	}

	public virtual KeyValuePair<string, int> GetFloorSource(string pattern_id)
	{
		int pattern_index;
		if (pattern_id.Contains(':'))
		{
			string[] pattern_split = pattern_id.Split(':');
			TileSheet tilesheet = this.GetWallAndFloorTilesheet(pattern_split[0]);
			if (int.TryParse(pattern_split[1], out pattern_index) && tilesheet != null)
			{
				return new KeyValuePair<string, int>(tilesheet.Id, pattern_index);
			}
		}
		if (int.TryParse(pattern_id, out pattern_index))
		{
			return new KeyValuePair<string, int>("walls_and_floors", pattern_index);
		}
		return new KeyValuePair<string, int>(null, -1);
	}

	public virtual KeyValuePair<string, int> GetWallpaperSource(string pattern_id)
	{
		int pattern_index;
		if (pattern_id.Contains(':'))
		{
			string[] pattern_split = pattern_id.Split(':');
			TileSheet tilesheet = this.GetWallAndFloorTilesheet(pattern_split[0]);
			if (int.TryParse(pattern_split[1], out pattern_index) && tilesheet != null)
			{
				return new KeyValuePair<string, int>(tilesheet.Id, pattern_index);
			}
		}
		if (int.TryParse(pattern_id, out pattern_index))
		{
			return new KeyValuePair<string, int>("walls_and_floors", pattern_index);
		}
		return new KeyValuePair<string, int>(null, -1);
	}

	public virtual void UpdateFloor(string floorId)
	{
		this.updateMap();
		if (!this.appliedFloor.TryGetValue(floorId, out var patternId) || !this.floorTiles.TryGetValue(floorId, out var tiles))
		{
			return;
		}
		bool appliedAny = false;
		HashSet<string> errors = null;
		foreach (Vector3 item in tiles)
		{
			int x = (int)item.X;
			int y = (int)item.Y;
			KeyValuePair<string, int> source = this.GetFloorSource(patternId);
			if (source.Value < 0)
			{
				if (DecoratableLocation.LogTroubleshootingInfo)
				{
					errors = errors ?? new HashSet<string>();
					errors.Add("floor pattern '" + patternId + "' doesn't match any known floor set");
				}
				continue;
			}
			string tilesheetId = source.Key;
			int spriteIndex = source.Value;
			int tilesWide = base.map.RequireTileSheet(tilesheetId).SheetWidth;
			spriteIndex = spriteIndex * 2 + spriteIndex / (tilesWide / 2) * tilesWide;
			if (tilesheetId == "walls_and_floors")
			{
				spriteIndex += this.GetFirstFlooringTile();
			}
			if (!this.IsFloorableOrWallpaperableTile(x, y, "Back", out var reason))
			{
				if (DecoratableLocation.LogTroubleshootingInfo)
				{
					errors = errors ?? new HashSet<string>();
					errors.Add(reason);
				}
			}
			else
			{
				base.setMapTile(x, y, this.GetFlooringIndex(spriteIndex, x, y), "Back", tilesheetId);
				appliedAny = true;
			}
		}
		if (!appliedAny && errors != null && errors.Count > 0)
		{
			Game1.log.Warn($"Couldn't apply floors for area ID '{floorId}' ({string.Join("; ", errors)})");
		}
	}

	public virtual void UpdateWallpaper(string wallpaperId)
	{
		this.updateMap();
		if (!this.appliedWallpaper.TryGetValue(wallpaperId, out var patternId) || !this.wallpaperTiles.TryGetValue(wallpaperId, out var tiles))
		{
			return;
		}
		bool appliedAny = false;
		HashSet<string> errors = null;
		foreach (Vector3 item in tiles)
		{
			int x = (int)item.X;
			int y = (int)item.Y;
			int type = (int)item.Z;
			KeyValuePair<string, int> source = this.GetWallpaperSource(patternId);
			if (source.Value < 0)
			{
				if (DecoratableLocation.LogTroubleshootingInfo)
				{
					errors = errors ?? new HashSet<string>();
					errors.Add("wallpaper pattern '" + patternId + "' doesn't match any known wallpaper set");
				}
				continue;
			}
			string tileSheetId = source.Key;
			int spriteIndex = source.Value;
			TileSheet tilesheet = base.map.RequireTileSheet(tileSheetId);
			int tilesWide = tilesheet.SheetWidth;
			string reasonInvalid;
			string layer = ((type == 2 && this.IsFloorableOrWallpaperableTile(x, y, "Buildings", out reasonInvalid)) ? "Buildings" : "Back");
			if (!this.IsFloorableOrWallpaperableTile(x, y, layer, out var reason))
			{
				if (DecoratableLocation.LogTroubleshootingInfo)
				{
					errors = errors ?? new HashSet<string>();
					errors.Add(reason);
				}
			}
			else
			{
				base.setMapTile(x, y, spriteIndex / tilesWide * tilesWide * 3 + spriteIndex % tilesWide + type * tilesWide, layer, tilesheet.Id);
				appliedAny = true;
			}
		}
		if (!appliedAny && errors != null && errors.Count > 0)
		{
			Game1.log.Warn($"Couldn't apply wallpaper for area ID '{wallpaperId}' ({string.Join("; ", errors)})");
		}
	}

	public override void UpdateWhenCurrentLocation(GameTime time)
	{
		if (!base.wasUpdated)
		{
			base.UpdateWhenCurrentLocation(time);
		}
	}

	public override void MakeMapModifications(bool force = false)
	{
		base.MakeMapModifications(force);
		if (!(this is FarmHouse))
		{
			this.ReadWallpaperAndFloorTileData();
			this.setWallpapers();
			this.setFloors();
		}
		if (base.hasTileAt(Game1.player.TilePoint, "Buildings"))
		{
			Game1.player.position.Y += 64f;
		}
	}

	protected override void resetLocalState()
	{
		base.resetLocalState();
		if (Game1.player.mailReceived.Add("button_tut_1"))
		{
			Game1.onScreenMenus.Add(new ButtonTutorialMenu(0));
		}
	}

	public override bool CanFreePlaceFurniture()
	{
		return true;
	}

	public virtual bool isTileOnWall(int x, int y)
	{
		foreach (string id in this.wallpaperTiles.Keys)
		{
			foreach (Vector3 tile_data in this.wallpaperTiles[id])
			{
				if ((int)tile_data.X == x && (int)tile_data.Y == y)
				{
					return true;
				}
			}
		}
		return false;
	}

	public int GetWallTopY(int x, int y)
	{
		foreach (string id in this.wallpaperTiles.Keys)
		{
			foreach (Vector3 tile_data in this.wallpaperTiles[id])
			{
				if ((int)tile_data.X == x && (int)tile_data.Y == y)
				{
					return y - (int)tile_data.Z;
				}
			}
		}
		return -1;
	}

	public virtual void setFloors()
	{
		foreach (KeyValuePair<string, string> pair in this.appliedFloor.Pairs)
		{
			this.UpdateFloor(pair.Key);
		}
	}

	public virtual void setWallpapers()
	{
		foreach (KeyValuePair<string, string> pair in this.appliedWallpaper.Pairs)
		{
			this.UpdateWallpaper(pair.Key);
		}
	}

	public void SetFloor(string which, string which_room)
	{
		if (which_room == null)
		{
			foreach (string key in this.floorIDs)
			{
				this.appliedFloor[key] = which;
			}
			return;
		}
		this.appliedFloor[which_room] = which;
	}

	public void SetWallpaper(string which, string which_room)
	{
		if (which_room == null)
		{
			foreach (string key in this.wallpaperIDs)
			{
				this.appliedWallpaper[key] = which;
			}
			return;
		}
		this.appliedWallpaper[which_room] = which;
	}

	public void OverrideSpecificWallpaper(string which, string which_room, string wallpaperStyleToOverride)
	{
		if (which_room == null)
		{
			foreach (string key in this.wallpaperIDs)
			{
				if (this.appliedWallpaper.TryGetValue(key, out var prevStyle) && prevStyle == wallpaperStyleToOverride)
				{
					this.appliedWallpaper[key] = which;
				}
			}
			return;
		}
		if (this.appliedWallpaper[which_room] == wallpaperStyleToOverride)
		{
			this.appliedWallpaper[which_room] = which;
		}
	}

	public void OverrideSpecificFlooring(string which, string which_room, string flooringStyleToOverride)
	{
		if (which_room == null)
		{
			foreach (string key in this.floorIDs)
			{
				if (this.appliedFloor.TryGetValue(key, out var prevStyle) && prevStyle == flooringStyleToOverride)
				{
					this.appliedFloor[key] = which;
				}
			}
			return;
		}
		if (this.appliedFloor[which_room] == flooringStyleToOverride)
		{
			this.appliedFloor[which_room] = which;
		}
	}

	public string GetFloorID(int x, int y)
	{
		foreach (string id in this.floorTiles.Keys)
		{
			foreach (Vector3 tile_data in this.floorTiles[id])
			{
				if ((int)tile_data.X == x && (int)tile_data.Y == y)
				{
					return id;
				}
			}
		}
		return null;
	}

	public string GetWallpaperID(int x, int y)
	{
		foreach (string id in this.wallpaperTiles.Keys)
		{
			foreach (Vector3 tile_data in this.wallpaperTiles[id])
			{
				if ((int)tile_data.X == x && (int)tile_data.Y == y)
				{
					return id;
				}
			}
		}
		return null;
	}

	protected bool IsFloorableTile(int x, int y, string layer_name)
	{
		int tileIndex = base.getTileIndexAt(x, y, "Buildings", "untitled tile sheet");
		if (tileIndex >= 197 && tileIndex <= 199)
		{
			return false;
		}
		return this.IsFloorableOrWallpaperableTile(x, y, layer_name);
	}

	public bool IsWallAndFloorTilesheet(string tilesheet_id)
	{
		if (!(tilesheet_id == "walls_and_floors") && !tilesheet_id.Contains("walls_and_floors"))
		{
			return tilesheet_id.StartsWith("x_WallsAndFloors_");
		}
		return true;
	}

	protected bool IsFloorableOrWallpaperableTile(int x, int y, string layerName)
	{
		string reasonInvalid;
		return this.IsFloorableOrWallpaperableTile(x, y, layerName, out reasonInvalid);
	}

	protected bool IsFloorableOrWallpaperableTile(int x, int y, string layerName, out string reasonInvalid)
	{
		Layer layer = base.map.GetLayer(layerName);
		if (layer == null)
		{
			reasonInvalid = "layer '" + layerName + "' not found";
			return false;
		}
		if (x < 0 || x >= layer.LayerWidth || y < 0 || y >= layer.LayerHeight)
		{
			reasonInvalid = $"tile ({x}, {y}) is out of bounds for the layer";
			return false;
		}
		Tile tile = layer.Tiles[x, y];
		if (tile == null)
		{
			reasonInvalid = $"tile ({x}, {y}) not found";
			return false;
		}
		TileSheet tilesheet = tile.TileSheet;
		if (tilesheet == null)
		{
			reasonInvalid = $"tile ({x}, {y}) has unknown tilesheet";
			return false;
		}
		if (!this.IsWallAndFloorTilesheet(tilesheet.Id))
		{
			reasonInvalid = "tilesheet '" + tilesheet.Id + "' isn't a wall and floor tilesheet, expected tilesheet ID containing 'walls_and_floors' or starting with 'x_WallsAndFloors_'";
			return false;
		}
		reasonInvalid = null;
		return true;
	}

	public override void TransferDataFromSavedLocation(GameLocation l)
	{
		if (l is DecoratableLocation decoratable_location)
		{
			if (!decoratable_location.appliedWallpaper.Keys.Any() && !decoratable_location.appliedFloor.Keys.Any())
			{
				this.ReadWallpaperAndFloorTileData();
				for (int i = 0; i < decoratable_location.wallPaper.Count; i++)
				{
					try
					{
						string key = this.wallpaperIDs[i];
						string value = decoratable_location.wallPaper[i].ToString();
						this.appliedWallpaper[key] = value;
					}
					catch (Exception)
					{
					}
				}
				for (int j = 0; j < decoratable_location.floor.Count; j++)
				{
					try
					{
						string key2 = this.floorIDs[j];
						string value2 = decoratable_location.floor[j].ToString();
						this.appliedFloor[key2] = value2;
					}
					catch (Exception)
					{
					}
				}
			}
			else
			{
				foreach (string key3 in decoratable_location.appliedWallpaper.Keys)
				{
					this.appliedWallpaper[key3] = decoratable_location.appliedWallpaper[key3];
				}
				foreach (string key4 in decoratable_location.appliedFloor.Keys)
				{
					this.appliedFloor[key4] = decoratable_location.appliedFloor[key4];
				}
			}
		}
		this.setWallpapers();
		this.setFloors();
		base.TransferDataFromSavedLocation(l);
	}

	public Furniture getRandomFurniture(Random r)
	{
		return r.ChooseFrom(base.furniture);
	}

	public virtual string getFloorRoomIdAt(Point p)
	{
		foreach (string key in this.floorTiles.Keys)
		{
			foreach (Vector3 tile_data in this.floorTiles[key])
			{
				if ((int)tile_data.X == p.X && (int)tile_data.Y == p.Y)
				{
					return key;
				}
			}
		}
		return null;
	}

	public virtual int GetFirstFlooringTile()
	{
		return 336;
	}

	public virtual int GetFlooringIndex(int base_tile_sheet, int tile_x, int tile_y)
	{
		if (!base.hasTileAt(tile_x, tile_y, "Back"))
		{
			return 0;
		}
		string tilesheet_name = base.getTileSheetIDAt(tile_x, tile_y, "Back");
		TileSheet tilesheet = base.map.GetTileSheet(tilesheet_name);
		int tiles_wide = 16;
		if (tilesheet != null)
		{
			tiles_wide = tilesheet.SheetWidth;
		}
		int x_offset = tile_x % 2;
		int y_offset = tile_y % 2;
		return base_tile_sheet + x_offset + tiles_wide * y_offset;
	}

	public virtual List<Microsoft.Xna.Framework.Rectangle> getFloors()
	{
		return new List<Microsoft.Xna.Framework.Rectangle>();
	}
}
