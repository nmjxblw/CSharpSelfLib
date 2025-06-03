using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.BellsAndWhistles;
using StardewValley.Extensions;
using StardewValley.Monsters;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using xTile;
using xTile.Dimensions;
using xTile.Layers;
using xTile.Tiles;

namespace StardewValley.Locations;

public class VolcanoDungeon : IslandLocation
{
	public enum TileNeighbors
	{
		N = 1,
		S = 2,
		E = 4,
		W = 8,
		NW = 0x10,
		NE = 0x20
	}

	private const int coalIndexPlaceholder = 1095382;

	private const string coalIndexPlaceholderString = "1095382";

	/// <summary>The main tile sheet ID for volcano dungeon tiles.</summary>
	public const string MainTileSheetId = "dungeon";

	public NetInt level = new NetInt();

	public NetEvent1Field<Point, NetPoint> coolLavaEvent = new NetEvent1Field<Point, NetPoint>();

	/// <summary>The volcano dungeon levels which are currently loaded and ready.</summary>
	/// <remarks>When removing a location from this list, code should call <see cref="M:StardewValley.Locations.VolcanoDungeon.OnRemoved" /> since it won't be called automatically.</remarks>
	public static List<VolcanoDungeon> activeLevels = new List<VolcanoDungeon>();

	public NetVector2Dictionary<bool, NetBool> cooledLavaTiles = new NetVector2Dictionary<bool, NetBool>();

	public Dictionary<Vector2, Point> localCooledLavaTiles = new Dictionary<Vector2, Point>();

	public HashSet<Point> dirtTiles = new HashSet<Point>();

	public NetInt generationSeed = new NetInt();

	public NetInt layoutIndex = new NetInt();

	public Random generationRandom;

	private LocalizedContentManager mapContent;

	[XmlIgnore]
	public int mapWidth;

	[XmlIgnore]
	public int mapHeight;

	public const int WALL_HEIGHT = 4;

	public Layer backLayer;

	public Layer buildingsLayer;

	public Layer frontLayer;

	public Layer alwaysFrontLayer;

	[XmlIgnore]
	public Point? startPosition;

	[XmlIgnore]
	public Point? endPosition;

	public const int LAYOUT_WIDTH = 64;

	public const int LAYOUT_HEIGHT = 64;

	[XmlIgnore]
	public Texture2D mapBaseTilesheet;

	public static List<Microsoft.Xna.Framework.Rectangle> setPieceAreas = new List<Microsoft.Xna.Framework.Rectangle>();

	protected static Dictionary<int, Point> _blobIndexLookup = null;

	protected static Dictionary<int, Point> _lavaBlobIndexLookup = null;

	protected bool generated;

	protected static Point shortcutOutPosition = new Point(29, 34);

	[XmlIgnore]
	protected NetBool shortcutOutUnlocked = new NetBool(value: false)
	{
		InterpolationWait = false
	};

	[XmlIgnore]
	protected NetBool bridgeUnlocked = new NetBool(value: false)
	{
		InterpolationWait = false
	};

	public Color[] pixelMap;

	public int[] heightMap;

	public Dictionary<int, List<Point>> possibleSwitchPositions = new Dictionary<int, List<Point>>();

	public Dictionary<int, List<Point>> possibleGatePositions = new Dictionary<int, List<Point>>();

	public NetList<DwarfGate, NetRef<DwarfGate>> dwarfGates = new NetList<DwarfGate, NetRef<DwarfGate>>();

	[XmlIgnore]
	protected bool _sawFlameSprite;

	private int lavaSoundsPlayedThisTick;

	private float steamTimer = 6000f;

	public VolcanoDungeon()
	{
		this.mapContent = Game1.game1.xTileContent.CreateTemporary();
		base.mapPath.Value = "Maps\\Mines\\VolcanoTemplate";
	}

	public VolcanoDungeon(int level)
		: this()
	{
		this.level.Value = level;
		base.name.Value = VolcanoDungeon.GetLevelName(level);
	}

	public override bool BlocksDamageLOS(int x, int y)
	{
		if (this.cooledLavaTiles.ContainsKey(new Vector2(x, y)))
		{
			return false;
		}
		return base.BlocksDamageLOS(x, y);
	}

	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(this.level, "level").AddField(this.coolLavaEvent, "coolLavaEvent").AddField(this.cooledLavaTiles.NetFields, "cooledLavaTiles.NetFields")
			.AddField(this.generationSeed, "generationSeed")
			.AddField(this.layoutIndex, "layoutIndex")
			.AddField(this.dwarfGates, "dwarfGates")
			.AddField(this.shortcutOutUnlocked, "shortcutOutUnlocked")
			.AddField(this.bridgeUnlocked, "bridgeUnlocked");
		this.coolLavaEvent.onEvent += OnCoolLavaEvent;
		this.bridgeUnlocked.fieldChangeEvent += delegate(NetBool f, bool oldValue, bool newValue)
		{
			if (newValue && base.mapPath.Value != null)
			{
				this.UpdateBridge();
			}
		};
		this.shortcutOutUnlocked.fieldChangeEvent += delegate(NetBool f, bool oldValue, bool newValue)
		{
			if (newValue && base.mapPath.Value != null)
			{
				this.UpdateShortcutOut();
			}
		};
	}

	protected override LocalizedContentManager getMapLoader()
	{
		return this.mapContent;
	}

	public override bool CanPlaceThisFurnitureHere(Furniture furniture)
	{
		return false;
	}

	public virtual void OnCoolLavaEvent(Point point)
	{
		this.CoolLava(point.X, point.Y);
		this.UpdateLavaNeighbor(point.X, point.Y);
		this.UpdateLavaNeighbor(point.X - 1, point.Y);
		this.UpdateLavaNeighbor(point.X + 1, point.Y);
		this.UpdateLavaNeighbor(point.X, point.Y - 1);
		this.UpdateLavaNeighbor(point.X, point.Y + 1);
	}

	public virtual void CoolLava(int x, int y, bool playSound = true)
	{
		if (Game1.currentLocation == this)
		{
			for (int i = 0; i < 5; i++)
			{
				base.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(372, 1956, 10, 10), new Vector2(x, (float)y - 0.5f) * 64f + new Vector2(Game1.random.Next(64), Game1.random.Next(64)), flipped: false, 0.007f, Color.White)
				{
					alpha = 0.75f,
					motion = new Vector2(0f, -1f),
					acceleration = new Vector2(0.002f, 0f),
					interval = 99999f,
					layerDepth = 1f,
					scale = 4f,
					scaleChange = 0.02f,
					rotationChange = (float)Game1.random.Next(-5, 6) * (float)Math.PI / 256f,
					delayBeforeAnimationStart = i * 35
				});
			}
			if (playSound && this.lavaSoundsPlayedThisTick < 3)
			{
				DelayedAction.playSoundAfterDelay("steam", this.lavaSoundsPlayedThisTick * 300);
				this.lavaSoundsPlayedThisTick++;
			}
		}
		this.cooledLavaTiles.TryAdd(new Vector2(x, y), value: true);
	}

	public virtual void UpdateLavaNeighbor(int x, int y)
	{
		if (this.IsCooledLava(x, y))
		{
			base.setTileProperty(x, y, "Buildings", "Passable", "T");
			int neighbors = 0;
			if (this.IsCooledLava(x, y - 1))
			{
				neighbors++;
			}
			if (this.IsCooledLava(x, y + 1))
			{
				neighbors += 2;
			}
			if (this.IsCooledLava(x - 1, y))
			{
				neighbors += 8;
			}
			if (this.IsCooledLava(x + 1, y))
			{
				neighbors += 4;
			}
			if (this.GetBlobLookup().TryGetValue(neighbors, out var offset))
			{
				this.localCooledLavaTiles[new Vector2(x, y)] = offset;
			}
		}
	}

	public virtual bool IsCooledLava(int x, int y)
	{
		if (x < 0 || x >= this.mapWidth)
		{
			return false;
		}
		if (y < 0 || y >= this.mapHeight)
		{
			return false;
		}
		return this.cooledLavaTiles.ContainsKey(new Vector2(x, y));
	}

	public override bool answerDialogueAction(string questionAndAnswer, string[] questionParams)
	{
		if (questionAndAnswer == null)
		{
			return false;
		}
		if (questionAndAnswer == "LeaveVolcano_Yes")
		{
			this.UseVolcanoShortcut();
			return true;
		}
		return base.answerDialogueAction(questionAndAnswer, questionParams);
	}

	public void UseVolcanoShortcut()
	{
		DelayedAction.playSoundAfterDelay("fallDown", 200);
		DelayedAction.playSoundAfterDelay("clubSmash", 900);
		Game1.player.CanMove = false;
		Game1.player.jump();
		Game1.warpFarmer("IslandNorth", 56, 17, 1);
	}

	public virtual void GenerateContents(bool use_level_level_as_layout = false)
	{
		this.generated = true;
		if (Game1.IsMasterGame)
		{
			this.generationSeed.Value = Utility.CreateRandomSeed(Game1.stats.DaysPlayed * (this.level.Value + 1), this.level.Value * 5152, Game1.uniqueIDForThisGame / 2);
			switch (this.level.Value)
			{
			case 0:
				this.layoutIndex.Value = 0;
				this.bridgeUnlocked.Value = Game1.MasterPlayer.hasOrWillReceiveMail("Island_VolcanoBridge");
				base.parrotUpgradePerches.Clear();
				base.parrotUpgradePerches.Add(new ParrotUpgradePerch(this, new Point(27, 39), new Microsoft.Xna.Framework.Rectangle(28, 34, 5, 4), 5, delegate
				{
					Game1.addMailForTomorrow("Island_VolcanoBridge", noLetter: true, sendToEveryone: true);
					this.bridgeUnlocked.Value = true;
				}, () => this.bridgeUnlocked.Value, "VolcanoBridge", "reachedCaldera, Island_Turtle"));
				break;
			case 5:
				this.layoutIndex.Value = 31;
				base.waterColor.Value = Color.DeepSkyBlue * 0.6f;
				this.shortcutOutUnlocked.Value = Game1.MasterPlayer.hasOrWillReceiveMail("Island_VolcanoShortcutOut");
				base.parrotUpgradePerches.Clear();
				base.parrotUpgradePerches.Add(new ParrotUpgradePerch(this, new Point(VolcanoDungeon.shortcutOutPosition.X, VolcanoDungeon.shortcutOutPosition.Y), new Microsoft.Xna.Framework.Rectangle(VolcanoDungeon.shortcutOutPosition.X - 1, VolcanoDungeon.shortcutOutPosition.Y - 1, 3, 3), 5, delegate
				{
					Game1.addMailForTomorrow("Island_VolcanoShortcutOut", noLetter: true, sendToEveryone: true);
					this.shortcutOutUnlocked.Value = true;
				}, () => this.shortcutOutUnlocked.Value, "VolcanoShortcutOut", "Island_Turtle"));
				break;
			case 9:
				this.layoutIndex.Value = 30;
				break;
			default:
			{
				List<int> valid_layouts = new List<int>();
				for (int i = 1; i < this.GetMaxRoomLayouts(); i++)
				{
					valid_layouts.Add(i);
				}
				Random layout_random = Utility.CreateRandom(this.generationSeed.Value);
				float luckMultiplier = 1f + (float)Game1.player.team.AverageLuckLevel() * 0.035f + (float)Game1.player.team.AverageDailyLuck() / 2f;
				if (this.level.Value > 1 && layout_random.NextDouble() < 0.5 * (double)luckMultiplier)
				{
					bool foundSpecialLevel = false;
					for (int j = 0; j < VolcanoDungeon.activeLevels.Count; j++)
					{
						if (VolcanoDungeon.activeLevels[j].layoutIndex.Value >= 32)
						{
							foundSpecialLevel = true;
							break;
						}
					}
					if (!foundSpecialLevel)
					{
						for (int k = 32; k < 38; k++)
						{
							valid_layouts.Add(k);
						}
					}
				}
				if (this.level.Value > 0 && Game1.MasterPlayer.hasOrWillReceiveMail("volcanoShortcutUnlocked") && layout_random.NextDouble() < 0.75)
				{
					for (int l = 38; l < 58; l++)
					{
						valid_layouts.Add(l);
					}
				}
				for (int m = 0; m < VolcanoDungeon.activeLevels.Count; m++)
				{
					if (VolcanoDungeon.activeLevels[m].level.Value == this.level.Value - 1)
					{
						valid_layouts.Remove(VolcanoDungeon.activeLevels[m].layoutIndex.Value);
						break;
					}
				}
				this.layoutIndex.Value = layout_random.ChooseFrom(valid_layouts);
				break;
			}
			}
		}
		this.GenerateLevel(use_level_level_as_layout);
		if (this.level.Value != 5)
		{
			return;
		}
		base.ApplyMapOverride("Mines\\Volcano_Well", (Microsoft.Xna.Framework.Rectangle?)null, (Microsoft.Xna.Framework.Rectangle?)new Microsoft.Xna.Framework.Rectangle(25, 29, 6, 4));
		for (int x = 27; x < 31; x++)
		{
			for (int y = 29; y < 33; y++)
			{
				base.waterTiles[x, y] = true;
			}
		}
		base.ApplyMapOverride("Mines\\Volcano_DwarfShop", (Microsoft.Xna.Framework.Rectangle?)null, (Microsoft.Xna.Framework.Rectangle?)new Microsoft.Xna.Framework.Rectangle(34, 29, 5, 4));
		base.setMapTile(36, 30, 77, "Buildings", "dungeon", "asedf");
		base.setMapTile(36, 29, 61, "Front", "dungeon");
		base.setMapTile(35, 31, 78, "Back", "dungeon");
		base.setMapTile(36, 31, 79, "Back", "dungeon");
		base.setMapTile(37, 31, 62, "Back", "dungeon");
		if (Game1.IsMasterGame)
		{
			base.objects.Add(new Vector2(34f, 29f), BreakableContainer.GetBarrelForVolcanoDungeon(new Vector2(34f, 29f)));
			base.objects.Add(new Vector2(26f, 32f), BreakableContainer.GetBarrelForVolcanoDungeon(new Vector2(26f, 32f)));
			base.objects.Add(new Vector2(38f, 33f), BreakableContainer.GetBarrelForVolcanoDungeon(new Vector2(38f, 33f)));
		}
	}

	public bool isMushroomLevel()
	{
		if (this.layoutIndex.Value >= 32)
		{
			return this.layoutIndex.Value <= 34;
		}
		return false;
	}

	public bool isMonsterLevel()
	{
		if (this.layoutIndex.Value >= 35)
		{
			return this.layoutIndex.Value <= 37;
		}
		return false;
	}

	/// <inheritdoc />
	public override void checkForMusic(GameTime time)
	{
		if (Game1.getMusicTrackName() == "none" || Game1.isMusicContextActiveButNotPlaying())
		{
			Game1.changeMusicTrack("Volcano_Ambient");
		}
		base.checkForMusic(time);
	}

	public virtual void UpdateShortcutOut()
	{
		if (this == Game1.currentLocation)
		{
			if (this.shortcutOutUnlocked.Value)
			{
				base.setMapTile(VolcanoDungeon.shortcutOutPosition.X, VolcanoDungeon.shortcutOutPosition.Y, 367, "Buildings", "dungeon");
				base.removeTile(VolcanoDungeon.shortcutOutPosition.X, VolcanoDungeon.shortcutOutPosition.Y - 1, "Front");
			}
			else
			{
				base.setMapTile(VolcanoDungeon.shortcutOutPosition.X, VolcanoDungeon.shortcutOutPosition.Y, 399, "Buildings", "dungeon");
				base.setMapTile(VolcanoDungeon.shortcutOutPosition.X, VolcanoDungeon.shortcutOutPosition.Y - 1, 383, "Front", "dungeon");
			}
		}
	}

	public virtual void UpdateBridge()
	{
		if (this != Game1.currentLocation)
		{
			return;
		}
		if (Game1.MasterPlayer.hasOrWillReceiveMail("reachedCaldera"))
		{
			base.setMapTile(27, 39, 399, "Buildings", "dungeon");
			base.setMapTile(27, 38, 383, "Front", "dungeon");
		}
		if (!this.bridgeUnlocked.Value)
		{
			return;
		}
		for (int x = 28; x <= 32; x++)
		{
			for (int y = 34; y <= 37; y++)
			{
				base.setMapTile(x, y, x switch
				{
					28 => y switch
					{
						34 => 189, 
						37 => 221, 
						_ => 205, 
					}, 
					32 => y switch
					{
						34 => 191, 
						37 => 223, 
						_ => 207, 
					}, 
					_ => y switch
					{
						34 => 190, 
						37 => 222, 
						_ => 206, 
					}, 
				}, "Buildings", "dungeon").Properties["Passable"] = "T";
				base.removeTileProperty(x, y, "Back", "Water");
				NPC n = base.isCharacterAtTile(new Vector2(x, y));
				if (n is Monster)
				{
					base.characters.Remove(n);
				}
				if (base.waterTiles != null && x != 28 && x != 32)
				{
					base.waterTiles[x, y] = false;
				}
				this.cooledLavaTiles.Remove(new Vector2(x, y));
			}
		}
	}

	protected override void resetLocalState()
	{
		if (!this.generated)
		{
			this.GenerateContents();
			this.generated = true;
		}
		foreach (Vector2 position in this.cooledLavaTiles.Keys)
		{
			this.UpdateLavaNeighbor((int)position.X, (int)position.Y);
		}
		if (this.level.Value == 0)
		{
			this.UpdateBridge();
		}
		if (this.level.Value == 5)
		{
			this.UpdateShortcutOut();
		}
		base.resetLocalState();
		Game1.ambientLight = Color.White;
		int player_tile_y = (int)(Game1.player.Position.Y / 64f);
		if (this.level.Value == 0 && Game1.player.previousLocationName == "Caldera")
		{
			Game1.player.Position = new Vector2(44f, 50f) * 64f;
		}
		else if (player_tile_y == 0 && this.endPosition.HasValue)
		{
			if (this.endPosition.HasValue)
			{
				Game1.player.Position = new Vector2(this.endPosition.Value.X, this.endPosition.Value.Y) * 64f;
			}
		}
		else if (player_tile_y == 1 && this.startPosition.HasValue)
		{
			Game1.player.Position = new Vector2(this.startPosition.Value.X, this.startPosition.Value.Y) * 64f;
		}
		TileSheet mainTileSheet = base.map.RequireTileSheet(0, "dungeon");
		this.mapBaseTilesheet = Game1.temporaryContent.Load<Texture2D>(mainTileSheet.ImageSource);
		foreach (DwarfGate dwarfGate in this.dwarfGates)
		{
			dwarfGate.ResetLocalState();
		}
		if (this.level.Value == 5)
		{
			AmbientLocationSounds.addSound(new Vector2(29f, 31f), 0);
		}
		if (this.level.Value != 0)
		{
			return;
		}
		if (Game1.player.hasOrWillReceiveMail("Saw_Flame_Sprite_Volcano"))
		{
			this._sawFlameSprite = true;
		}
		if (!this._sawFlameSprite)
		{
			base.temporarySprites.Add(new TemporaryAnimatedSprite("Characters\\Monsters\\Magma Sprite", new Microsoft.Xna.Framework.Rectangle(0, 32, 16, 16), new Vector2(30f, 38f) * 64f, flipped: false, 0f, Color.White)
			{
				id = 999,
				scale = 4f,
				totalNumberOfLoops = 99999,
				interval = 70f,
				lightId = "VolcanoDungeon_FlameSpirit",
				lightRadius = 1f,
				animationLength = 7,
				layerDepth = 1f,
				yPeriodic = true,
				yPeriodicRange = 12f,
				yPeriodicLoopTime = 1000f,
				xPeriodic = true,
				xPeriodicRange = 16f,
				xPeriodicLoopTime = 1800f
			});
			base.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\shadow", new Microsoft.Xna.Framework.Rectangle(0, 0, 12, 7), new Vector2(30.2f, 39.4f) * 64f, flipped: false, 0f, Color.White)
			{
				id = 998,
				scale = 4f,
				totalNumberOfLoops = 99999,
				interval = 1000f,
				animationLength = 1,
				layerDepth = 0.001f,
				yPeriodic = true,
				yPeriodicRange = 1f,
				yPeriodicLoopTime = 1000f,
				xPeriodic = true,
				xPeriodicRange = 16f,
				xPeriodicLoopTime = 1800f
			});
		}
		base.ApplyMapOverride("Mines\\Volcano_Well", (Microsoft.Xna.Framework.Rectangle?)null, (Microsoft.Xna.Framework.Rectangle?)new Microsoft.Xna.Framework.Rectangle(22, 43, 6, 4));
		for (int x = 24; x < 28; x++)
		{
			for (int y = 43; y < 47; y++)
			{
				base.waterTiles[x, y] = true;
			}
		}
	}

	public override string GetLocationSpecificMusic()
	{
		if (this.level.Value == 5)
		{
			return "Volcano_Ambient";
		}
		if (Game1.getMusicTrackName() == "VolcanoMines")
		{
			return "VolcanoMines";
		}
		if (this.level.Value == 1 || ((Game1.random.NextDouble() < 0.25 || this.level.Value == 6) && (Game1.getMusicTrackName() == "none" || Game1.isMusicContextActiveButNotPlaying() || Game1.getMusicTrackName().EndsWith("_Ambient"))))
		{
			return "VolcanoMines";
		}
		return "Volcano_Ambient";
	}

	protected override void resetSharedState()
	{
		base.resetSharedState();
		if (this.level.Value != 5)
		{
			base.waterColor.Value = Color.White;
		}
	}

	public override bool CanRefillWateringCanOnTile(int tileX, int tileY)
	{
		if (this.level.Value == 5 && new Microsoft.Xna.Framework.Rectangle(27, 29, 4, 4).Contains(tileX, tileY))
		{
			return true;
		}
		if (this.level.Value == 0 && tileX > 23 && tileX < 28 && tileY > 42 && tileY < 47)
		{
			return true;
		}
		return false;
	}

	public virtual void GenerateLevel(bool use_level_level_as_layout = false)
	{
		this.generationRandom = Utility.CreateRandom(this.generationSeed.Value);
		this.generationRandom.Next();
		base.mapPath.Value = "Maps\\Mines\\VolcanoTemplate";
		this.updateMap();
		base.loadedMapPath = base.mapPath.Value;
		Texture2D layout_texture = Game1.temporaryContent.Load<Texture2D>("VolcanoLayouts\\Layouts");
		this.mapWidth = 64;
		this.mapHeight = 64;
		base.waterTiles = new WaterTiles(this.mapWidth, this.mapHeight);
		for (int i = 0; i < base.map.Layers.Count; i++)
		{
			Layer template_layer = base.map.Layers[i];
			base.map.RemoveLayer(template_layer);
			base.map.InsertLayer(new Layer(template_layer.Id, base.map, new Size(this.mapWidth, this.mapHeight), template_layer.TileSize), i);
		}
		this.backLayer = base.map.RequireLayer("Back");
		this.buildingsLayer = base.map.RequireLayer("Buildings");
		this.frontLayer = base.map.RequireLayer("Front");
		this.alwaysFrontLayer = base.map.RequireLayer("AlwaysFront");
		TileSheet tileSheet = base.map.RequireTileSheet(0, "dungeon");
		tileSheet.TileIndexProperties[1].Add("Type", "Stone");
		tileSheet.TileIndexProperties[2].Add("Type", "Stone");
		tileSheet.TileIndexProperties[3].Add("Type", "Stone");
		tileSheet.TileIndexProperties[17].Add("Type", "Stone");
		tileSheet.TileIndexProperties[18].Add("Type", "Stone");
		tileSheet.TileIndexProperties[19].Add("Type", "Stone");
		tileSheet.TileIndexProperties[528].Add("Type", "Stone");
		tileSheet.TileIndexProperties[544].Add("Type", "Stone");
		tileSheet.TileIndexProperties[560].Add("Type", "Stone");
		tileSheet.TileIndexProperties[545].Add("Type", "Stone");
		tileSheet.TileIndexProperties[561].Add("Type", "Stone");
		tileSheet.TileIndexProperties[564].Add("Type", "Stone");
		tileSheet.TileIndexProperties[565].Add("Type", "Stone");
		tileSheet.TileIndexProperties[555].Add("Type", "Stone");
		tileSheet.TileIndexProperties[571].Add("Type", "Stone");
		this.pixelMap = new Color[this.mapWidth * this.mapHeight];
		this.heightMap = new int[this.mapWidth * this.mapHeight];
		int columns = layout_texture.Width / 64;
		int value = this.layoutIndex.Value;
		int layout_offset_x = value % columns * 64;
		int layout_offset_y = value / columns * 64;
		bool flip_x = this.generationRandom.Next(2) == 1;
		if (this.layoutIndex.Value == 0 || this.layoutIndex.Value == 31)
		{
			flip_x = false;
		}
		this.ApplyPixels("VolcanoLayouts\\Layouts", layout_offset_x, layout_offset_y, this.mapWidth, this.mapHeight, 0, 0, flip_x);
		for (int x = 0; x < this.mapWidth; x++)
		{
			for (int y = 0; y < this.mapHeight; y++)
			{
				this.PlaceGroundTile(x, y);
			}
		}
		this.ApplyToColor(new Color(0, 255, 0), delegate(int num, int num2)
		{
			if (!this.startPosition.HasValue)
			{
				this.startPosition = new Point(num, num2);
			}
			if (this.level.Value == 0)
			{
				base.warps.Add(new Warp(num, num2 + 2, "IslandNorth", 40, 24, flipFarmer: false));
			}
			else
			{
				base.warps.Add(new Warp(num, num2 + 2, VolcanoDungeon.GetLevelName(this.level.Value - 1), num - this.startPosition.Value.X, 0, flipFarmer: false));
			}
		});
		this.ApplyToColor(new Color(255, 0, 0), delegate(int num, int num2)
		{
			if (!this.endPosition.HasValue)
			{
				this.endPosition = new Point(num, num2);
			}
			if (this.level.Value == 9)
			{
				base.warps.Add(new Warp(num, num2 - 2, "Caldera", 21, 39, flipFarmer: false));
			}
			else
			{
				base.warps.Add(new Warp(num, num2 - 2, VolcanoDungeon.GetLevelName(this.level.Value + 1), num - this.endPosition.Value.X, 1, flipFarmer: false));
			}
		});
		VolcanoDungeon.setPieceAreas.Clear();
		Color set_piece_color = new Color(255, 255, 0);
		this.ApplyToColor(set_piece_color, delegate(int num, int num2)
		{
			int j;
			for (j = 0; j < 32 && !(this.GetPixel(num + j, num2, Color.Black) != set_piece_color) && !(this.GetPixel(num, num2 + j, Color.Black) != set_piece_color); j++)
			{
			}
			VolcanoDungeon.setPieceAreas.Add(new Microsoft.Xna.Framework.Rectangle(num, num2, j, j));
			for (int k = 0; k < j; k++)
			{
				for (int l = 0; l < j; l++)
				{
					this.SetPixelMap(num + k, num2 + l, Color.White);
				}
			}
		});
		this.possibleSwitchPositions = new Dictionary<int, List<Point>>();
		this.possibleGatePositions = new Dictionary<int, List<Point>>();
		this.ApplyToColor(new Color(128, 128, 128), delegate(int x2, int y2)
		{
			this.AddPossibleSwitchLocation(0, x2, y2);
		});
		this.ApplySetPieces();
		this.GenerateWalls(Color.Black, 0, 4, 4, 4, start_in_wall: true, delegate(int x2, int y2)
		{
			this.SetPixelMap(x2, y2, Color.Chartreuse);
		}, use_corner_hack: true);
		this.GenerateWalls(Color.Chartreuse, 0, 13, 1);
		this.ApplyToColor(Color.Blue, delegate(int num, int num2)
		{
			base.waterTiles[num, num2] = true;
			this.SetTile(this.backLayer, num, num2, 4);
			base.setTileProperty(num, num2, "Back", "Water", "T");
			if (this.generationRandom.NextDouble() < 0.1)
			{
				base.sharedLights.AddLight(new LightSource($"VolcanoDungeon_{this.level.Value}_Lava_{num}_{num2}", 4, new Vector2(num, num2) * 64f, 2f, new Color(0, 50, 50), LightSource.LightContext.None, 0L, base.NameOrUniqueName));
			}
		});
		this.GenerateBlobs(Color.Blue, 0, 16, fill_center: true, is_lava_pool: true);
		if (this.startPosition.HasValue)
		{
			this.CreateEntrance(this.startPosition.Value);
		}
		if (this.endPosition.HasValue)
		{
			this.CreateExit(this.endPosition);
		}
		if (this.level.Value != 0)
		{
			this.GenerateDirtTiles();
		}
		if ((this.level.Value == 9 || this.generationRandom.NextDouble() < (this.isMonsterLevel() ? 1.0 : 0.2)) && this.possibleSwitchPositions.TryGetValue(0, out var endSwitchPositions) && endSwitchPositions.Count > 0)
		{
			this.AddPossibleGateLocation(0, this.endPosition.Value.X, this.endPosition.Value.Y);
		}
		foreach (int index in this.possibleGatePositions.Keys)
		{
			if (this.possibleGatePositions[index].Count > 0 && this.possibleSwitchPositions.TryGetValue(index, out var dwarfSwitchPositions) && dwarfSwitchPositions.Count > 0)
			{
				Point gate_point = this.generationRandom.ChooseFrom(this.possibleGatePositions[index]);
				this.CreateDwarfGate(index, gate_point);
			}
		}
		if (this.level.Value == 0)
		{
			this.CreateExit(new Point(40, 48), draw_stairs: false);
			base.removeTile(40, 46, "Buildings");
			base.removeTile(40, 45, "Buildings");
			base.removeTile(40, 44, "Buildings");
			base.setMapTile(40, 45, 266, "AlwaysFront", "dungeon");
			base.setMapTile(40, 44, 76, "AlwaysFront", "dungeon");
			base.setMapTile(39, 44, 76, "AlwaysFront", "dungeon");
			base.setMapTile(41, 44, 76, "AlwaysFront", "dungeon");
			base.removeTile(40, 43, "Front");
			base.setMapTile(40, 43, 70, "AlwaysFront", "dungeon");
			base.removeTile(39, 43, "Front");
			base.setMapTile(39, 43, 69, "AlwaysFront", "dungeon");
			base.removeTile(41, 43, "Front");
			base.setMapTile(41, 43, 69, "AlwaysFront", "dungeon");
			base.setMapTile(39, 45, 265, "AlwaysFront", "dungeon");
			base.setMapTile(41, 45, 267, "AlwaysFront", "dungeon");
			base.setMapTile(40, 45, 60, "Back", "dungeon");
			base.setMapTile(40, 46, 60, "Back", "dungeon");
			base.setMapTile(40, 47, 60, "Back", "dungeon");
			base.setMapTile(40, 48, 555, "Back", "dungeon");
			this.AddPossibleSwitchLocation(-1, 40, 51);
			this.CreateDwarfGate(-1, new Point(40, 48));
			base.setMapTile(34, 30, 90, "Buildings", "dungeon");
			base.setMapTile(34, 29, 148, "Buildings", "dungeon");
			base.setMapTile(34, 31, 180, "Buildings", "dungeon");
			base.setMapTile(34, 32, 196, "Buildings", "dungeon");
			this.CoolLava(34, 34, playSound: false);
			if (Game1.MasterPlayer.hasOrWillReceiveMail("volcanoShortcutUnlocked"))
			{
				foreach (DwarfGate gate in this.dwarfGates)
				{
					if (gate.gateIndex.Value != -1)
					{
						continue;
					}
					gate.opened.Value = true;
					gate.triggeredOpen = true;
					foreach (Point point in gate.switches.Keys)
					{
						gate.switches[point] = true;
					}
				}
			}
			this.CreateExit(new Point(44, 50));
			base.warps.Add(new Warp(44, 48, "Caldera", 11, 36, flipFarmer: false));
			this.CreateEntrance(new Point(6, 48));
			base.warps.Add(new Warp(6, 50, "IslandNorth", 12, 31, flipFarmer: false));
		}
		if (Game1.IsMasterGame)
		{
			this.GenerateEntities();
		}
		this.pixelMap = null;
		this.SortLayers();
	}

	public virtual void GenerateDirtTiles()
	{
		if (this.level.Value == 5)
		{
			return;
		}
		for (int i = 0; i < 8; i++)
		{
			int center_x = this.generationRandom.Next(0, 64);
			int center_y = this.generationRandom.Next(0, 64);
			int travel_distance = this.generationRandom.Next(2, 8);
			int radius = this.generationRandom.Next(1, 3);
			int direction_x = ((this.generationRandom.Next(2) != 0) ? 1 : (-1));
			int direction_y = ((this.generationRandom.Next(2) != 0) ? 1 : (-1));
			bool x_oriented = this.generationRandom.Next(2) == 0;
			for (int j = 0; j < travel_distance; j++)
			{
				for (int x = center_x - radius; x <= center_x + radius; x++)
				{
					for (int y = center_y - radius; y <= center_y + radius; y++)
					{
						if (!(this.GetPixel(x, y, Color.Black) != Color.White))
						{
							this.dirtTiles.Add(new Point(x, y));
						}
					}
				}
				if (x_oriented)
				{
					direction_y += ((this.generationRandom.Next(2) != 0) ? 1 : (-1));
				}
				else
				{
					direction_x += ((this.generationRandom.Next(2) != 0) ? 1 : (-1));
				}
				center_x += direction_x;
				center_y += direction_y;
				radius += ((this.generationRandom.Next(2) != 0) ? 1 : (-1));
				if (radius < 1)
				{
					radius = 1;
				}
				if (radius > 4)
				{
					radius = 4;
				}
			}
		}
		for (int k = 0; k < 2; k++)
		{
			this.ErodeInvalidDirtTiles();
		}
		HashSet<Point> visited_neighbors = new HashSet<Point>();
		Point[] neighboring_tiles = new Point[8]
		{
			new Point(-1, -1),
			new Point(0, -1),
			new Point(1, -1),
			new Point(-1, 0),
			new Point(1, 0),
			new Point(-1, 1),
			new Point(0, 1),
			new Point(1, 1)
		};
		foreach (Point point in this.dirtTiles)
		{
			this.SetTile(this.backLayer, point.X, point.Y, VolcanoDungeon.GetTileIndex(9, 1));
			if (this.generationRandom.NextDouble() < 0.015)
			{
				base.characters.Add(new Duggy(Utility.PointToVector2(point) * 64f, magmaDuggy: true));
			}
			Point[] array = neighboring_tiles;
			for (int l = 0; l < array.Length; l++)
			{
				Point offset = array[l];
				Point neighbor = new Point(point.X + offset.X, point.Y + offset.Y);
				if (!this.dirtTiles.Contains(neighbor) && !visited_neighbors.Contains(neighbor))
				{
					visited_neighbors.Add(neighbor);
					Point? neighbor_tile_offset = this.GetDirtNeighborTile(neighbor.X, neighbor.Y);
					if (neighbor_tile_offset.HasValue)
					{
						this.SetTile(this.backLayer, neighbor.X, neighbor.Y, VolcanoDungeon.GetTileIndex(8 + neighbor_tile_offset.Value.X, neighbor_tile_offset.Value.Y));
					}
				}
			}
		}
	}

	public virtual void CreateEntrance(Point? position)
	{
		for (int x = -1; x <= 1; x++)
		{
			for (int y = 0; y <= 3; y++)
			{
				if (base.isTileOnMap(new Vector2(position.Value.X + x, position.Value.Y + y)))
				{
					base.removeTile(position.Value.X + x, position.Value.Y + y, "Back");
					base.removeTile(position.Value.X + x, position.Value.Y + y, "Buildings");
					base.removeTile(position.Value.X + x, position.Value.Y + y, "Front");
				}
			}
		}
		if (base.hasTileAt(position.Value.X - 1, position.Value.Y - 1, "Front"))
		{
			this.SetTile(this.frontLayer, position.Value.X - 1, position.Value.Y - 1, VolcanoDungeon.GetTileIndex(13, 16));
		}
		base.removeTile(position.Value.X, position.Value.Y - 1, "Front");
		this.SetTile(this.buildingsLayer, position.Value.X - 1, position.Value.Y, VolcanoDungeon.GetTileIndex(13, 17));
		this.SetTile(this.buildingsLayer, position.Value.X - 1, position.Value.Y + 1, VolcanoDungeon.GetTileIndex(13, 18));
		this.SetTile(this.buildingsLayer, position.Value.X - 1, position.Value.Y + 2, VolcanoDungeon.GetTileIndex(13, 19));
		if (base.hasTileAt(position.Value.X + 1, position.Value.Y - 1, "Front"))
		{
			this.SetTile(this.frontLayer, position.Value.X + 1, position.Value.Y - 1, VolcanoDungeon.GetTileIndex(15, 16));
		}
		this.SetTile(this.buildingsLayer, position.Value.X + 1, position.Value.Y, VolcanoDungeon.GetTileIndex(15, 17));
		this.SetTile(this.buildingsLayer, position.Value.X + 1, position.Value.Y + 1, VolcanoDungeon.GetTileIndex(15, 18));
		this.SetTile(this.buildingsLayer, position.Value.X + 1, position.Value.Y + 2, VolcanoDungeon.GetTileIndex(15, 19));
		this.SetTile(this.backLayer, position.Value.X, position.Value.Y, VolcanoDungeon.GetTileIndex(14, 17));
		this.SetTile(this.backLayer, position.Value.X, position.Value.Y + 1, VolcanoDungeon.GetTileIndex(14, 18));
		this.SetTile(this.frontLayer, position.Value.X, position.Value.Y + 2, VolcanoDungeon.GetTileIndex(14, 19));
		this.SetTile(this.buildingsLayer, position.Value.X - 1, position.Value.Y + 3, VolcanoDungeon.GetTileIndex(12, 4));
		this.SetTile(this.buildingsLayer, position.Value.X, position.Value.Y + 3, VolcanoDungeon.GetTileIndex(12, 4));
		this.SetTile(this.buildingsLayer, position.Value.X + 1, position.Value.Y + 3, VolcanoDungeon.GetTileIndex(12, 4));
	}

	private void CreateExit(Point? position, bool draw_stairs = true)
	{
		for (int x = -1; x <= 1; x++)
		{
			for (int y = -4; y <= 0; y++)
			{
				if (base.isTileOnMap(new Vector2(position.Value.X + x, position.Value.Y + y)))
				{
					if (draw_stairs)
					{
						base.removeTile(position.Value.X + x, position.Value.Y + y, "Back");
					}
					base.removeTile(position.Value.X + x, position.Value.Y + y, "Buildings");
					base.removeTile(position.Value.X + x, position.Value.Y + y, "Front");
				}
			}
		}
		this.SetTile(this.buildingsLayer, position.Value.X - 1, position.Value.Y, VolcanoDungeon.GetTileIndex(9, 19));
		this.SetTile(this.buildingsLayer, position.Value.X - 1, position.Value.Y - 1, VolcanoDungeon.GetTileIndex(9, 18));
		this.SetTile(this.buildingsLayer, position.Value.X - 1, position.Value.Y - 2, VolcanoDungeon.GetTileIndex(9, 17));
		this.SetTile(this.buildingsLayer, position.Value.X - 1, position.Value.Y - 3, VolcanoDungeon.GetTileIndex(9, 16));
		this.SetTile(this.alwaysFrontLayer, position.Value.X - 1, position.Value.Y - 4, VolcanoDungeon.GetTileIndex(12, 4));
		this.SetTile(this.alwaysFrontLayer, position.Value.X, position.Value.Y - 4, VolcanoDungeon.GetTileIndex(12, 4));
		this.SetTile(this.alwaysFrontLayer, position.Value.X + 1, position.Value.Y - 4, VolcanoDungeon.GetTileIndex(12, 4));
		this.SetTile(this.buildingsLayer, position.Value.X, position.Value.Y - 3, VolcanoDungeon.GetTileIndex(10, 16));
		this.SetTile(this.buildingsLayer, position.Value.X + 1, position.Value.Y, VolcanoDungeon.GetTileIndex(11, 19));
		this.SetTile(this.buildingsLayer, position.Value.X + 1, position.Value.Y - 1, VolcanoDungeon.GetTileIndex(11, 18));
		this.SetTile(this.buildingsLayer, position.Value.X + 1, position.Value.Y - 2, VolcanoDungeon.GetTileIndex(11, 17));
		this.SetTile(this.buildingsLayer, position.Value.X + 1, position.Value.Y - 3, VolcanoDungeon.GetTileIndex(11, 16));
		if (draw_stairs)
		{
			this.SetTile(this.backLayer, position.Value.X, position.Value.Y, VolcanoDungeon.GetTileIndex(12, 19));
			this.SetTile(this.backLayer, position.Value.X, position.Value.Y - 1, VolcanoDungeon.GetTileIndex(12, 18));
			this.SetTile(this.backLayer, position.Value.X, position.Value.Y - 2, VolcanoDungeon.GetTileIndex(12, 17));
			this.SetTile(this.backLayer, position.Value.X, position.Value.Y - 3, VolcanoDungeon.GetTileIndex(12, 16));
		}
		this.SetTile(this.buildingsLayer, position.Value.X - 1, position.Value.Y - 4, VolcanoDungeon.GetTileIndex(12, 4));
		this.SetTile(this.buildingsLayer, position.Value.X, position.Value.Y - 4, VolcanoDungeon.GetTileIndex(12, 4));
		this.SetTile(this.buildingsLayer, position.Value.X + 1, position.Value.Y - 4, VolcanoDungeon.GetTileIndex(12, 4));
	}

	public virtual void ErodeInvalidDirtTiles()
	{
		Point[] neighboring_tiles = new Point[8]
		{
			new Point(-1, -1),
			new Point(0, -1),
			new Point(1, -1),
			new Point(-1, 0),
			new Point(1, 0),
			new Point(-1, 1),
			new Point(0, 1),
			new Point(1, 1)
		};
		Dictionary<Point, bool> visited_tiles = new Dictionary<Point, bool>();
		List<Point> dirt_to_remove = new List<Point>();
		foreach (Point dirt_tile in this.dirtTiles)
		{
			bool fail = false;
			foreach (Microsoft.Xna.Framework.Rectangle setPieceArea in VolcanoDungeon.setPieceAreas)
			{
				if (setPieceArea.Contains(dirt_tile))
				{
					fail = true;
					break;
				}
			}
			if (!fail && base.hasTileAt(dirt_tile, "Buildings"))
			{
				fail = true;
			}
			if (!fail)
			{
				Point[] array = neighboring_tiles;
				for (int i = 0; i < array.Length; i++)
				{
					Point offset = array[i];
					Point neighbor = new Point(dirt_tile.X + offset.X, dirt_tile.Y + offset.Y);
					if (visited_tiles.TryGetValue(neighbor, out var prevSucceeded))
					{
						if (!prevSucceeded)
						{
							fail = true;
							break;
						}
					}
					else if (!this.dirtTiles.Contains(neighbor))
					{
						if (!this.GetDirtNeighborTile(neighbor.X, neighbor.Y).HasValue)
						{
							fail = true;
						}
						visited_tiles[neighbor] = !fail;
						if (fail)
						{
							break;
						}
					}
				}
			}
			if (fail)
			{
				dirt_to_remove.Add(dirt_tile);
			}
		}
		foreach (Point remove in dirt_to_remove)
		{
			this.dirtTiles.Remove(remove);
		}
	}

	public override void monsterDrop(Monster monster, int x, int y, Farmer who)
	{
		base.monsterDrop(monster, x, y, who);
		if (Game1.random.NextDouble() < 0.05)
		{
			Game1.player.team.RequestLimitedNutDrops("VolcanoMonsterDrop", this, x, y, 5);
		}
	}

	public Point? GetDirtNeighborTile(int tile_x, int tile_y)
	{
		if (this.GetPixel(tile_x, tile_y, Color.Black) != Color.White)
		{
			return null;
		}
		if (base.hasTileAt(new Point(tile_x, tile_y), "Buildings"))
		{
			return null;
		}
		if (this.dirtTiles.Contains(new Point(tile_x, tile_y - 1)) && this.dirtTiles.Contains(new Point(tile_x, tile_y + 1)))
		{
			return null;
		}
		if (this.dirtTiles.Contains(new Point(tile_x - 1, tile_y)) && this.dirtTiles.Contains(new Point(tile_x + 1, tile_y)))
		{
			return null;
		}
		if (this.dirtTiles.Contains(new Point(tile_x - 1, tile_y)) && !this.dirtTiles.Contains(new Point(tile_x + 1, tile_y)))
		{
			if (this.dirtTiles.Contains(new Point(tile_x, tile_y - 1)))
			{
				return new Point(3, 3);
			}
			if (this.dirtTiles.Contains(new Point(tile_x, tile_y + 1)))
			{
				return new Point(3, 1);
			}
			return new Point(2, 1);
		}
		if (this.dirtTiles.Contains(new Point(tile_x + 1, tile_y)) && !this.dirtTiles.Contains(new Point(tile_x - 1, tile_y)))
		{
			if (this.dirtTiles.Contains(new Point(tile_x, tile_y - 1)))
			{
				return new Point(3, 2);
			}
			if (this.dirtTiles.Contains(new Point(tile_x, tile_y + 1)))
			{
				return new Point(3, 0);
			}
			return new Point(0, 1);
		}
		if (this.dirtTiles.Contains(new Point(tile_x, tile_y - 1)) && !this.dirtTiles.Contains(new Point(tile_x, tile_y + 1)))
		{
			return new Point(1, 2);
		}
		if (this.dirtTiles.Contains(new Point(tile_x, tile_y + 1)) && !this.dirtTiles.Contains(new Point(tile_x, tile_y - 1)))
		{
			return new Point(1, 0);
		}
		if (this.dirtTiles.Contains(new Point(tile_x - 1, tile_y - 1)))
		{
			return new Point(2, 2);
		}
		if (this.dirtTiles.Contains(new Point(tile_x + 1, tile_y - 1)))
		{
			return new Point(0, 2);
		}
		if (this.dirtTiles.Contains(new Point(tile_x - 1, tile_y + 1)))
		{
			return new Point(0, 2);
		}
		if (this.dirtTiles.Contains(new Point(tile_x + 1, tile_y + 1)))
		{
			return new Point(2, 2);
		}
		return null;
	}

	public virtual void CreateDwarfGate(int gate_index, Point tile_position)
	{
		this.SetTile(this.backLayer, tile_position.X, tile_position.Y + 1, VolcanoDungeon.GetTileIndex(3, 34));
		this.SetTile(this.buildingsLayer, tile_position.X - 1, tile_position.Y + 1, VolcanoDungeon.GetTileIndex(2, 34));
		this.SetTile(this.buildingsLayer, tile_position.X + 1, tile_position.Y + 1, VolcanoDungeon.GetTileIndex(4, 34));
		this.SetTile(this.buildingsLayer, tile_position.X - 1, tile_position.Y, VolcanoDungeon.GetTileIndex(2, 33));
		this.SetTile(this.buildingsLayer, tile_position.X + 1, tile_position.Y, VolcanoDungeon.GetTileIndex(4, 33));
		this.SetTile(this.frontLayer, tile_position.X - 1, tile_position.Y - 1, VolcanoDungeon.GetTileIndex(2, 32));
		this.SetTile(this.frontLayer, tile_position.X + 1, tile_position.Y - 1, VolcanoDungeon.GetTileIndex(4, 32));
		this.SetTile(this.alwaysFrontLayer, tile_position.X - 1, tile_position.Y - 1, VolcanoDungeon.GetTileIndex(2, 32));
		this.SetTile(this.alwaysFrontLayer, tile_position.X, tile_position.Y - 1, VolcanoDungeon.GetTileIndex(3, 32));
		this.SetTile(this.alwaysFrontLayer, tile_position.X + 1, tile_position.Y - 1, VolcanoDungeon.GetTileIndex(4, 32));
		if (gate_index == 0)
		{
			this.SetTile(this.alwaysFrontLayer, tile_position.X - 1, tile_position.Y - 2, VolcanoDungeon.GetTileIndex(0, 32));
			this.SetTile(this.alwaysFrontLayer, tile_position.X + 1, tile_position.Y - 2, VolcanoDungeon.GetTileIndex(0, 32));
		}
		else
		{
			this.SetTile(this.alwaysFrontLayer, tile_position.X - 1, tile_position.Y - 2, VolcanoDungeon.GetTileIndex(9, 25));
			this.SetTile(this.alwaysFrontLayer, tile_position.X + 1, tile_position.Y - 2, VolcanoDungeon.GetTileIndex(10, 25));
		}
		int seed = this.generationRandom.Next();
		if (Game1.IsMasterGame)
		{
			DwarfGate gate = new DwarfGate(this, gate_index, tile_position.X, tile_position.Y, seed);
			this.dwarfGates.Add(gate);
		}
	}

	public virtual void AddPossibleSwitchLocation(int switch_index, int x, int y)
	{
		if (!this.possibleSwitchPositions.TryGetValue(switch_index, out var positions))
		{
			positions = (this.possibleSwitchPositions[switch_index] = new List<Point>());
		}
		positions.Add(new Point(x, y));
	}

	public virtual void AddPossibleGateLocation(int gate_index, int x, int y)
	{
		if (!this.possibleGatePositions.TryGetValue(gate_index, out var positions))
		{
			positions = (this.possibleGatePositions[gate_index] = new List<Point>());
		}
		positions.Add(new Point(x, y));
	}

	private void adjustLevelChances(ref double stoneChance, ref double monsterChance, ref double itemChance, ref double gemStoneChance)
	{
		if (this.level.Value == 0 || this.level.Value == 5)
		{
			monsterChance = 0.0;
			itemChance = 0.0;
			gemStoneChance = 0.0;
			stoneChance = 0.0;
		}
		if (this.isMushroomLevel())
		{
			monsterChance = 0.025;
			itemChance *= 35.0;
			stoneChance = 0.0;
		}
		else if (this.isMonsterLevel())
		{
			stoneChance = 0.0;
			itemChance = 0.0;
			monsterChance *= 2.0;
		}
		bool has_avoid_monsters_buff = false;
		bool has_spawn_monsters_buff = false;
		foreach (Farmer onlineFarmer in Game1.getOnlineFarmers())
		{
			if (onlineFarmer.hasBuff("23"))
			{
				has_avoid_monsters_buff = true;
			}
			if (onlineFarmer.hasBuff("24"))
			{
				has_spawn_monsters_buff = true;
			}
			if (has_spawn_monsters_buff && has_avoid_monsters_buff)
			{
				break;
			}
		}
		if (has_spawn_monsters_buff)
		{
			monsterChance *= 2.0;
		}
		gemStoneChance /= 2.0;
	}

	public bool isTileClearForMineObjects(Vector2 v, bool ignoreRuins = false)
	{
		if ((Math.Abs((float)this.startPosition.Value.X - v.X) <= 2f && Math.Abs((float)this.startPosition.Value.Y - v.Y) <= 2f) || (Math.Abs((float)this.endPosition.Value.X - v.X) <= 2f && Math.Abs((float)this.endPosition.Value.Y - v.Y) <= 2f))
		{
			return false;
		}
		if (this.GetPixel((int)v.X, (int)v.Y, Color.Black) == new Color(128, 128, 128))
		{
			return false;
		}
		if (!this.CanItemBePlacedHere(v, itemIsPassable: false, CollisionMask.All, CollisionMask.None))
		{
			return false;
		}
		string s = this.doesTileHaveProperty((int)v.X, (int)v.Y, "Type", "Back");
		if (s == null || !s.Equals("Stone"))
		{
			return false;
		}
		if (!this.isTileOnClearAndSolidGround(v))
		{
			return false;
		}
		if (base.objects.ContainsKey(v))
		{
			return false;
		}
		if (ignoreRuins)
		{
			int tileIndex = base.getTileIndexAt((int)v.X, (int)v.Y, "Back", "dungeon");
			if (tileIndex == -1 || tileIndex >= 384)
			{
				return false;
			}
		}
		return true;
	}

	public bool isTileOnClearAndSolidGround(Vector2 v)
	{
		if (base.map.RequireLayer("Back").Tiles[(int)v.X, (int)v.Y] == null)
		{
			return false;
		}
		if (base.map.RequireLayer("Front").Tiles[(int)v.X, (int)v.Y] != null || base.map.RequireLayer("Buildings").Tiles[(int)v.X, (int)v.Y] != null)
		{
			return false;
		}
		return true;
	}

	public virtual void GenerateEntities()
	{
		List<Point> spawn_points = new List<Point>();
		this.ApplyToColor(new Color(0, 255, 255), delegate(int x2, int y2)
		{
			spawn_points.Add(new Point(x2, y2));
		});
		List<Point> spiker_spawn_points = new List<Point>();
		this.ApplyToColor(new Color(0, 128, 255), delegate(int x2, int y2)
		{
			spiker_spawn_points.Add(new Point(x2, y2));
		});
		double stoneChance = (double)this.generationRandom.Next(11, 18) / 150.0;
		double monsterChance = 0.0008 + (double)this.generationRandom.Next(70) / 10000.0;
		double itemChance = 0.001;
		double gemStoneChance = 0.003;
		this.adjustLevelChances(ref stoneChance, ref monsterChance, ref itemChance, ref gemStoneChance);
		if (this.level.Value > 0 && this.level.Value != 5 && (this.generationRandom.NextBool() || this.isMushroomLevel()))
		{
			int numBarrels = this.generationRandom.Next(5) + (int)(Game1.player.team.AverageDailyLuck(Game1.currentLocation) * 20.0);
			if (this.isMushroomLevel())
			{
				numBarrels += 50;
			}
			for (int i = 0; i < numBarrels; i++)
			{
				Point p;
				Point motion;
				if (this.generationRandom.NextDouble() < 0.33)
				{
					p = new Point(this.generationRandom.Next(base.map.RequireLayer("Back").LayerWidth), 0);
					motion = new Point(0, 1);
				}
				else if (this.generationRandom.NextBool())
				{
					p = new Point(0, this.generationRandom.Next(base.map.RequireLayer("Back").LayerHeight));
					motion = new Point(1, 0);
				}
				else
				{
					p = new Point(base.map.RequireLayer("Back").LayerWidth - 1, this.generationRandom.Next(base.map.RequireLayer("Back").LayerHeight));
					motion = new Point(-1, 0);
				}
				while (base.isTileOnMap(p.X, p.Y))
				{
					p.X += motion.X;
					p.Y += motion.Y;
					if (this.isTileClearForMineObjects(new Vector2(p.X, p.Y)))
					{
						Vector2 objectPos = new Vector2(p.X, p.Y);
						if (this.isMushroomLevel())
						{
							base.terrainFeatures.Add(objectPos, new CosmeticPlant(6 + this.generationRandom.Next(3)));
						}
						else
						{
							base.objects.Add(objectPos, BreakableContainer.GetBarrelForVolcanoDungeon(objectPos));
						}
						break;
					}
				}
			}
		}
		if (this.level.Value != 5)
		{
			for (int x = 0; x < base.map.Layers[0].LayerWidth; x++)
			{
				for (int y = 0; y < base.map.Layers[0].LayerHeight; y++)
				{
					Vector2 objectPos2 = new Vector2(x, y);
					if ((Math.Abs((float)this.startPosition.Value.X - objectPos2.X) <= 5f && Math.Abs((float)this.startPosition.Value.Y - objectPos2.Y) <= 5f) || (Math.Abs((float)this.endPosition.Value.X - objectPos2.X) <= 5f && Math.Abs((float)this.endPosition.Value.Y - objectPos2.Y) <= 5f))
					{
						continue;
					}
					if (this.CanItemBePlacedHere(objectPos2) && this.generationRandom.NextDouble() < monsterChance)
					{
						if (base.getTileIndexAt((int)objectPos2.X, (int)objectPos2.Y, "Back", "dungeon") == 25)
						{
							if (!this.isMushroomLevel())
							{
								base.characters.Add(new Duggy(objectPos2 * 64f, magmaDuggy: true));
							}
						}
						else if (this.isMushroomLevel())
						{
							base.characters.Add(new RockCrab(objectPos2 * 64f, "False Magma Cap"));
						}
						else
						{
							base.characters.Add(new Bat(objectPos2 * 64f, (this.level.Value > 5 && this.generationRandom.NextBool()) ? (-556) : (-555)));
						}
					}
					else
					{
						if (!this.isTileClearForMineObjects(objectPos2, ignoreRuins: true))
						{
							continue;
						}
						double chance = stoneChance;
						if (chance > 0.0)
						{
							foreach (Vector2 v in Utility.getAdjacentTileLocations(objectPos2))
							{
								if (base.objects.ContainsKey(v))
								{
									chance += 0.1;
								}
							}
						}
						int stoneIndex = this.chooseStoneTypeIndexOnly(objectPos2);
						bool basicStone = stoneIndex >= 845 && stoneIndex <= 847;
						if (chance > 0.0 && (!basicStone || this.generationRandom.NextDouble() < chance))
						{
							Object stone = this.createStone(stoneIndex, objectPos2);
							if (stone != null)
							{
								base.Objects.Add(objectPos2, stone);
							}
						}
						else if (this.generationRandom.NextDouble() < itemChance)
						{
							base.Objects.Add(objectPos2, new Object("851", 1)
							{
								IsSpawnedObject = true,
								CanBeGrabbed = true
							});
						}
					}
				}
			}
			while (stoneChance != 0.0 && this.generationRandom.NextDouble() < 0.2)
			{
				this.tryToAddOreClumps();
			}
		}
		for (int i2 = 0; i2 < 7; i2++)
		{
			if (spawn_points.Count == 0)
			{
				break;
			}
			int index = this.generationRandom.Next(0, spawn_points.Count);
			Point spawn_point = spawn_points[index];
			if (this.CanItemBePlacedHere(new Vector2(spawn_point.X, spawn_point.Y)))
			{
				Monster monster = null;
				if (this.generationRandom.NextDouble() <= 0.25)
				{
					for (int j = 0; j < 20; j++)
					{
						Point point = spawn_point;
						point.X += this.generationRandom.Next(-10, 11);
						point.Y += this.generationRandom.Next(-10, 11);
						bool fail = false;
						for (int check_x = -1; check_x <= 1; check_x++)
						{
							for (int check_y = -1; check_y <= 1; check_y++)
							{
								if (!LavaLurk.IsLavaTile(this, point.X + check_x, point.Y + check_y))
								{
									fail = true;
									break;
								}
							}
						}
						if (!fail)
						{
							monster = new LavaLurk(Utility.PointToVector2(point) * 64f);
							break;
						}
					}
				}
				if (monster == null && this.generationRandom.NextDouble() <= 0.20000000298023224)
				{
					monster = new HotHead(Utility.PointToVector2(spawn_point) * 64f);
				}
				if (monster == null)
				{
					GreenSlime greenSlime = new GreenSlime(Utility.PointToVector2(spawn_point) * 64f, 0);
					greenSlime.makeTigerSlime();
					monster = greenSlime;
				}
				if (monster != null)
				{
					base.characters.Add(monster);
				}
			}
			spawn_points.RemoveAt(index);
		}
		foreach (Point p2 in spiker_spawn_points)
		{
			if (this.CanSpawnCharacterHere(new Vector2(p2.X, p2.Y)))
			{
				int direction = 1;
				switch (base.getTileIndexAt(p2, "Back", "dungeon"))
				{
				case 537:
				case 538:
					direction = 2;
					break;
				case 552:
				case 569:
					direction = 3;
					break;
				case 553:
				case 570:
					direction = 0;
					break;
				}
				base.characters.Add(new Spiker(new Vector2(p2.X, p2.Y) * 64f, direction));
			}
		}
	}

	private Object createStone(int stone, Vector2 tile)
	{
		string whichStone = this.chooseStoneTypeIndexOnly(tile).ToString() ?? "";
		int stoneHealth = 1;
		switch (whichStone)
		{
		case "1095382":
			whichStone = (Game1.random.NextBool() ? "VolcanoCoalNode0" : "VolcanoCoalNode1");
			stoneHealth = 10;
			break;
		case "845":
		case "846":
		case "847":
			stoneHealth = 6;
			break;
		case "843":
		case "844":
			stoneHealth = 12;
			break;
		case "765":
			stoneHealth = 16;
			break;
		case "764":
			whichStone = "VolcanoGoldNode";
			stoneHealth = 8;
			break;
		case "290":
			stoneHealth = 8;
			break;
		case "751":
			stoneHealth = 8;
			break;
		case "819":
			stoneHealth = 8;
			break;
		}
		return new Object(whichStone, 1)
		{
			MinutesUntilReady = stoneHealth
		};
	}

	private int chooseStoneTypeIndexOnly(Vector2 tile)
	{
		int whichStone = this.generationRandom.Next(845, 848);
		float levelMod = 1f + (float)this.level.Value / 7f;
		float masterMultiplier = 0.8f;
		float luckMultiplier = 1f + (float)Game1.player.team.AverageLuckLevel() * 0.035f + (float)Game1.player.team.AverageDailyLuck() / 2f;
		double chance = 0.008 * (double)levelMod * (double)masterMultiplier * (double)luckMultiplier;
		foreach (Vector2 v in Utility.getAdjacentTileLocations(tile))
		{
			if (base.objects.TryGetValue(v, out var obj) && (obj.QualifiedItemId == "(O)843" || obj.QualifiedItemId == "(O)844"))
			{
				chance += 0.15;
			}
		}
		if (this.generationRandom.NextDouble() < chance)
		{
			whichStone = this.generationRandom.Next(843, 845);
		}
		else
		{
			chance = 0.0025 * (double)levelMod * (double)masterMultiplier * (double)luckMultiplier;
			foreach (Vector2 v2 in Utility.getAdjacentTileLocations(tile))
			{
				if (base.objects.TryGetValue(v2, out var obj2) && obj2.QualifiedItemId == "(O)765")
				{
					chance += 0.1;
				}
			}
			if (this.generationRandom.NextDouble() < chance)
			{
				whichStone = 765;
			}
			else
			{
				chance = 0.01 * (double)levelMod * (double)masterMultiplier;
				foreach (Vector2 v3 in Utility.getAdjacentTileLocations(tile))
				{
					if (base.objects.TryGetValue(v3, out var obj3) && obj3.QualifiedItemId == "(O)VolcanoGoldNode")
					{
						chance += 0.2;
					}
				}
				if (this.generationRandom.NextDouble() < chance)
				{
					whichStone = 764;
				}
				else
				{
					chance = 0.012 * (double)levelMod * (double)masterMultiplier;
					foreach (Vector2 v4 in Utility.getAdjacentTileLocations(tile))
					{
						if (base.objects.TryGetValue(v4, out var obj4) && obj4.QualifiedItemId.StartsWith("(O)VolcanoCoalNode"))
						{
							chance += 0.2;
						}
					}
					if (this.generationRandom.NextDouble() < chance)
					{
						whichStone = 1095382;
					}
					else
					{
						chance = 0.015 * (double)levelMod * (double)masterMultiplier;
						foreach (Vector2 v5 in Utility.getAdjacentTileLocations(tile))
						{
							if (base.objects.TryGetValue(v5, out var obj5) && obj5.QualifiedItemId == "(O)850")
							{
								chance += 0.25;
							}
						}
						if (this.generationRandom.NextDouble() < chance)
						{
							whichStone = 850;
						}
						else
						{
							chance = 0.018 * (double)levelMod * (double)masterMultiplier;
							foreach (Vector2 v6 in Utility.getAdjacentTileLocations(tile))
							{
								if (base.objects.TryGetValue(v6, out var obj6) && obj6.QualifiedItemId == "(O)849")
								{
									chance += 0.25;
								}
							}
							if (this.generationRandom.NextDouble() < chance)
							{
								whichStone = 849;
							}
						}
					}
				}
			}
		}
		if (this.generationRandom.NextDouble() < 0.0005)
		{
			whichStone = 819;
		}
		if (this.generationRandom.NextDouble() < 0.0007)
		{
			whichStone = 44;
		}
		if (this.level.Value > 2 && this.generationRandom.NextDouble() < 0.0002)
		{
			whichStone = 46;
		}
		return whichStone;
	}

	public void tryToAddOreClumps()
	{
		if (!(this.generationRandom.NextDouble() < 0.55 + Game1.player.team.AverageDailyLuck(Game1.currentLocation)))
		{
			return;
		}
		Vector2 endPoint = base.getRandomTile();
		for (int tries = 0; tries < 1 || this.generationRandom.NextDouble() < 0.25 + Game1.player.team.AverageDailyLuck(Game1.currentLocation); tries++)
		{
			if (this.CanItemBePlacedHere(endPoint, itemIsPassable: false, CollisionMask.All, CollisionMask.None) && this.isTileOnClearAndSolidGround(endPoint) && this.doesTileHaveProperty((int)endPoint.X, (int)endPoint.Y, "Diggable", "Back") == null)
			{
				Utility.recursiveObjectPlacement(new Object(this.generationRandom.Next(843, 845).ToString(), 1)
				{
					MinutesUntilReady = 12
				}, (int)endPoint.X, (int)endPoint.Y, 0.949999988079071, 0.30000001192092896, this, "Dirt", 0, 0.05000000074505806);
			}
			endPoint = base.getRandomTile();
		}
	}

	public virtual void ApplySetPieces()
	{
		for (int i = 0; i < VolcanoDungeon.setPieceAreas.Count; i++)
		{
			Microsoft.Xna.Framework.Rectangle rectangle = VolcanoDungeon.setPieceAreas[i];
			int size = 3;
			if (rectangle.Width >= 32)
			{
				size = 32;
			}
			else if (rectangle.Width >= 16)
			{
				size = 16;
			}
			else if (rectangle.Width >= 8)
			{
				size = 8;
			}
			else if (rectangle.Width >= 4)
			{
				size = 4;
			}
			Map override_map = Game1.game1.xTileContent.Load<Map>("Maps\\Mines\\Volcano_SetPieces_" + size);
			int cols = override_map.Layers[0].LayerWidth / size;
			int rows = override_map.Layers[0].LayerHeight / size;
			int selected_col = this.generationRandom.Next(0, cols);
			int selected_row = this.generationRandom.Next(0, rows);
			base.ApplyMapOverride(override_map, "area_" + i, new Microsoft.Xna.Framework.Rectangle(selected_col * size, selected_row * size, size, size), rectangle);
			Layer paths_layer = override_map.GetLayer("Paths");
			if (paths_layer == null)
			{
				continue;
			}
			for (int x = 0; x < size; x++)
			{
				for (int y = 0; y <= size; y++)
				{
					int source_x = selected_col * size + x;
					int source_y = selected_row * size + y;
					int dest_x = rectangle.Left + x;
					int dest_y = rectangle.Top + y;
					if (!paths_layer.IsValidTileLocation(source_x, source_y))
					{
						continue;
					}
					Tile tile = paths_layer.Tiles[source_x, source_y];
					int path_index = tile?.TileIndex ?? (-1);
					if (path_index >= VolcanoDungeon.GetTileIndex(10, 14) && path_index <= VolcanoDungeon.GetTileIndex(15, 14))
					{
						int index = path_index - VolcanoDungeon.GetTileIndex(10, 14);
						if (index > 0)
						{
							index += i * 10;
						}
						double chance = 1.0;
						if (tile.Properties.TryGetValue("Chance", out var property) && !double.TryParse(property, out chance))
						{
							chance = 1.0;
						}
						if (this.generationRandom.NextDouble() < chance)
						{
							this.AddPossibleGateLocation(index, dest_x, dest_y);
						}
					}
					else if (path_index >= VolcanoDungeon.GetTileIndex(10, 15) && path_index <= VolcanoDungeon.GetTileIndex(15, 15))
					{
						int index2 = path_index - VolcanoDungeon.GetTileIndex(10, 15);
						if (index2 > 0)
						{
							index2 += i * 10;
						}
						this.AddPossibleSwitchLocation(index2, dest_x, dest_y);
					}
					else if (path_index == VolcanoDungeon.GetTileIndex(10, 20))
					{
						this.SetPixelMap(dest_x, dest_y, new Color(0, 255, 255));
					}
					else if (path_index == VolcanoDungeon.GetTileIndex(11, 20))
					{
						this.SetPixelMap(dest_x, dest_y, new Color(0, 0, 255));
					}
					else if (path_index == VolcanoDungeon.GetTileIndex(12, 20))
					{
						this.SpawnChest(dest_x, dest_y);
					}
					else if (path_index == VolcanoDungeon.GetTileIndex(13, 20))
					{
						this.SetPixelMap(dest_x, dest_y, new Color(0, 0, 0));
					}
					else if (path_index == VolcanoDungeon.GetTileIndex(14, 20) && this.generationRandom.NextBool())
					{
						if (Game1.IsMasterGame)
						{
							base.objects.Add(new Vector2(dest_x, dest_y), BreakableContainer.GetBarrelForVolcanoDungeon(new Vector2(dest_x, dest_y)));
						}
					}
					else if (path_index == VolcanoDungeon.GetTileIndex(15, 20) && this.generationRandom.NextBool())
					{
						if (Game1.IsMasterGame)
						{
							Vector2 objTile = new Vector2(dest_x, dest_y);
							base.objects.Add(objTile, new Object("852", 1)
							{
								IsSpawnedObject = true,
								CanBeGrabbed = true
							});
						}
					}
					else if (path_index == VolcanoDungeon.GetTileIndex(10, 21))
					{
						this.SetPixelMap(dest_x, dest_y, new Color(0, 128, 255));
					}
				}
			}
		}
	}

	public virtual void SpawnChest(int tile_x, int tile_y)
	{
		Random chest_random = Utility.CreateRandom(this.generationRandom.Next());
		float extraRare_luckboost = (float)Game1.player.team.AverageLuckLevel() * 0.035f + (float)Game1.player.team.AverageDailyLuck() / 2f;
		if (Game1.IsMasterGame)
		{
			Vector2 position = new Vector2(tile_x, tile_y);
			Chest chest = new Chest(playerChest: false, position);
			chest.dropContents.Value = true;
			chest.synchronized.Value = true;
			chest.type.Value = "interactive";
			if (chest_random.NextDouble() < (double)((this.level.Value == 9) ? (0.5f + extraRare_luckboost) : (0.1f + extraRare_luckboost)))
			{
				chest.SetBigCraftableSpriteIndex(227);
				this.PopulateChest(chest.Items, chest_random, 1);
			}
			else
			{
				chest.SetBigCraftableSpriteIndex(223);
				this.PopulateChest(chest.Items, chest_random, 0);
			}
			base.setObject(position, chest);
		}
	}

	protected override bool breakStone(string stoneId, int x, int y, Farmer who, Random r)
	{
		if (who != null)
		{
			switch (stoneId)
			{
			case "845":
			case "846":
			case "847":
				if (Game1.random.NextDouble() < 0.005)
				{
					Game1.createObjectDebris("(O)827", x, y, who.UniqueMultiplayerID, this);
				}
				break;
			}
		}
		if (who != null && r.NextDouble() < 0.03)
		{
			Game1.player.team.RequestLimitedNutDrops("VolcanoMining", this, x * 64, y * 64, 5);
		}
		return base.breakStone(stoneId, x, y, who, r);
	}

	public virtual void PopulateChest(IList<Item> items, Random chest_random, int chest_type)
	{
		switch (chest_type)
		{
		case 0:
		{
			int random_count2 = 7;
			int random2 = chest_random.Next(random_count2);
			if (!Game1.netWorldState.Value.GoldenCoconutCracked)
			{
				while (random2 == 1)
				{
					random2 = chest_random.Next(random_count2);
				}
			}
			if (Game1.random.NextBool() && Game1.player.team.SpecialOrderRuleActive("DROP_QI_BEANS"))
			{
				int num2 = chest_random.Next(2, 6);
				for (int l = 0; l < num2; l++)
				{
					items.Add(ItemRegistry.Create("(O)890"));
				}
			}
			switch (random2)
			{
			case 0:
			{
				for (int num3 = 0; num3 < 3; num3++)
				{
					items.Add(ItemRegistry.Create("(O)848"));
				}
				break;
			}
			case 1:
				items.Add(ItemRegistry.Create("(O)791"));
				break;
			case 2:
			{
				for (int n = 0; n < 8; n++)
				{
					items.Add(ItemRegistry.Create("(O)831"));
				}
				break;
			}
			case 3:
			{
				for (int m = 0; m < 5; m++)
				{
					items.Add(ItemRegistry.Create("(O)833"));
				}
				break;
			}
			case 4:
				items.Add(new Ring("861"));
				break;
			case 5:
				items.Add(new Ring("862"));
				break;
			default:
				items.Add(MeleeWeapon.attemptAddRandomInnateEnchantment(ItemRegistry.Create("(W)" + chest_random.Next(54, 57)), chest_random));
				break;
			}
			break;
		}
		case 1:
		{
			int random_count = 9;
			int random = chest_random.Next(random_count);
			if (!Game1.netWorldState.Value.GoldenCoconutCracked)
			{
				while (random == 3)
				{
					random = chest_random.Next(random_count);
				}
			}
			if (Game1.random.NextDouble() <= 1.0 && Game1.player.team.SpecialOrderRuleActive("DROP_QI_BEANS"))
			{
				int num = chest_random.Next(4, 6);
				for (int i = 0; i < num; i++)
				{
					items.Add(ItemRegistry.Create("(O)890"));
				}
			}
			switch (random)
			{
			case 0:
			{
				for (int k = 0; k < 10; k++)
				{
					items.Add(ItemRegistry.Create("(O)848"));
				}
				break;
			}
			case 1:
				items.Add(ItemRegistry.Create("(B)854"));
				break;
			case 2:
				items.Add(ItemRegistry.Create("(B)855"));
				break;
			case 3:
			{
				for (int j = 0; j < 3; j++)
				{
					items.Add(ItemRegistry.Create<Object>("(O)791"));
				}
				break;
			}
			case 4:
				items.Add(new Ring("863"));
				break;
			case 5:
				items.Add(new Ring("860"));
				break;
			case 6:
				items.Add(MeleeWeapon.attemptAddRandomInnateEnchantment(ItemRegistry.Create("(W)" + chest_random.Next(57, 60)), chest_random));
				break;
			case 7:
				items.Add(ItemRegistry.Create("(H)76"));
				break;
			default:
				items.Add(ItemRegistry.Create("(O)289"));
				break;
			}
			break;
		}
		}
	}

	public virtual void ApplyToColor(Color match, Action<int, int> action)
	{
		for (int x = 0; x < this.mapWidth; x++)
		{
			for (int y = 0; y < this.mapHeight; y++)
			{
				if (this.GetPixel(x, y, match) == match)
				{
					action?.Invoke(x, y);
				}
			}
		}
	}

	public override bool sinkDebris(Debris debris, Vector2 chunkTile, Vector2 chunkPosition)
	{
		if (this.cooledLavaTiles.ContainsKey(chunkTile))
		{
			return false;
		}
		return base.sinkDebris(debris, chunkTile, chunkPosition);
	}

	public override bool performToolAction(Tool t, int tileX, int tileY)
	{
		if (this.level.Value != 5 && t is WateringCan && base.isTileOnMap(new Vector2(tileX, tileY)) && base.waterTiles[tileX, tileY] && !this.cooledLavaTiles.ContainsKey(new Vector2(tileX, tileY)))
		{
			this.coolLavaEvent.Fire(new Point(tileX, tileY));
		}
		return base.performToolAction(t, tileX, tileY);
	}

	public virtual void GenerateBlobs(Color match, int tile_x, int tile_y, bool fill_center = true, bool is_lava_pool = false)
	{
		for (int x = 0; x < this.mapWidth; x++)
		{
			for (int y = 0; y < this.mapHeight; y++)
			{
				if (!(this.GetPixel(x, y, match) == match))
				{
					continue;
				}
				int value = this.GetNeighborValue(x, y, match, is_lava_pool);
				if (fill_center || value != 15)
				{
					Dictionary<int, Point> blob_lookup = this.GetBlobLookup();
					if (is_lava_pool)
					{
						blob_lookup = this.GetLavaBlobLookup();
					}
					if (blob_lookup.TryGetValue(value, out var offset))
					{
						this.SetTile(this.buildingsLayer, x, y, VolcanoDungeon.GetTileIndex(tile_x + offset.X, tile_y + offset.Y));
					}
				}
			}
		}
	}

	public Dictionary<int, Point> GetBlobLookup()
	{
		if (VolcanoDungeon._blobIndexLookup == null)
		{
			VolcanoDungeon._blobIndexLookup = new Dictionary<int, Point>();
			VolcanoDungeon._blobIndexLookup[0] = new Point(0, 0);
			VolcanoDungeon._blobIndexLookup[6] = new Point(1, 0);
			VolcanoDungeon._blobIndexLookup[14] = new Point(2, 0);
			VolcanoDungeon._blobIndexLookup[10] = new Point(3, 0);
			VolcanoDungeon._blobIndexLookup[7] = new Point(1, 1);
			VolcanoDungeon._blobIndexLookup[11] = new Point(3, 1);
			VolcanoDungeon._blobIndexLookup[5] = new Point(1, 2);
			VolcanoDungeon._blobIndexLookup[13] = new Point(2, 2);
			VolcanoDungeon._blobIndexLookup[9] = new Point(3, 2);
			VolcanoDungeon._blobIndexLookup[2] = new Point(0, 1);
			VolcanoDungeon._blobIndexLookup[3] = new Point(0, 2);
			VolcanoDungeon._blobIndexLookup[1] = new Point(0, 3);
			VolcanoDungeon._blobIndexLookup[4] = new Point(1, 3);
			VolcanoDungeon._blobIndexLookup[12] = new Point(2, 3);
			VolcanoDungeon._blobIndexLookup[8] = new Point(3, 3);
			VolcanoDungeon._blobIndexLookup[15] = new Point(2, 1);
		}
		return VolcanoDungeon._blobIndexLookup;
	}

	public override bool isCollidingPosition(Microsoft.Xna.Framework.Rectangle position, xTile.Dimensions.Rectangle viewport, bool isFarmer, int damagesFarmer, bool glider, Character character, bool pathfinding, bool projectile = false, bool ignoreCharacterRequirement = false, bool skipCollisionEffects = false)
	{
		if (isFarmer && !glider && (position.Left < 0 || position.Right > base.map.DisplayWidth || position.Top < 0 || position.Bottom > base.map.DisplayHeight))
		{
			return true;
		}
		return base.isCollidingPosition(position, viewport, isFarmer, damagesFarmer, glider, character, pathfinding, projectile, ignoreCharacterRequirement);
	}

	public Dictionary<int, Point> GetLavaBlobLookup()
	{
		if (VolcanoDungeon._lavaBlobIndexLookup == null)
		{
			VolcanoDungeon._lavaBlobIndexLookup = new Dictionary<int, Point>(this.GetBlobLookup());
			VolcanoDungeon._lavaBlobIndexLookup[63] = new Point(2, 1);
			VolcanoDungeon._lavaBlobIndexLookup[47] = new Point(4, 3);
			VolcanoDungeon._lavaBlobIndexLookup[31] = new Point(4, 2);
			VolcanoDungeon._lavaBlobIndexLookup[15] = new Point(4, 1);
		}
		return VolcanoDungeon._lavaBlobIndexLookup;
	}

	public virtual void GenerateWalls(Color match, int source_x, int source_y, int wall_height = 4, int random_wall_variants = 1, bool start_in_wall = false, Action<int, int> on_insufficient_wall_height = null, bool use_corner_hack = false)
	{
		this.heightMap = new int[this.mapWidth * this.mapHeight];
		for (int i = 0; i < this.heightMap.Length; i++)
		{
			this.heightMap[i] = -1;
		}
		for (int pass = 0; pass < 2; pass++)
		{
			for (int x = 0; x < this.mapWidth; x++)
			{
				int last_y = -1;
				int clearance = 0;
				if (start_in_wall)
				{
					clearance = wall_height;
				}
				for (int current_y = 0; current_y <= this.mapHeight; current_y++)
				{
					if (this.GetPixel(x, current_y, match) != match || current_y >= this.mapHeight)
					{
						int current_height = 0;
						int wall_variant_index = 0;
						if (random_wall_variants > 1 && this.generationRandom.NextBool())
						{
							wall_variant_index = this.generationRandom.Next(1, random_wall_variants);
						}
						if (current_y >= this.mapHeight)
						{
							current_height = wall_height;
							clearance = wall_height;
						}
						for (int curr_y = current_y - 1; curr_y > last_y; curr_y--)
						{
							if (clearance < wall_height)
							{
								if (on_insufficient_wall_height != null)
								{
									on_insufficient_wall_height(x, curr_y);
								}
								else
								{
									this.SetPixelMap(x, curr_y, Color.White);
									this.PlaceSingleWall(x, curr_y);
								}
								current_height--;
							}
							else
							{
								switch (pass)
								{
								case 0:
									if (this.GetPixelClearance(x - 1, curr_y, wall_height, match) < wall_height && this.GetPixelClearance(x + 1, curr_y, wall_height, match) < wall_height)
									{
										if (on_insufficient_wall_height != null)
										{
											on_insufficient_wall_height(x, curr_y);
										}
										else
										{
											this.SetPixelMap(x, curr_y, Color.White);
											this.PlaceSingleWall(x, curr_y);
										}
										current_height--;
									}
									break;
								case 1:
									this.heightMap[x + curr_y * this.mapWidth] = current_height + 1;
									if (current_height < wall_height || wall_height == 0)
									{
										if (wall_height > 0)
										{
											this.SetTile(this.buildingsLayer, x, curr_y, VolcanoDungeon.GetTileIndex(source_x + random_wall_variants + wall_variant_index, source_y + 1 + random_wall_variants + wall_height - current_height - 1));
										}
									}
									else
									{
										this.SetTile(this.buildingsLayer, x, curr_y, VolcanoDungeon.GetTileIndex(source_x + random_wall_variants * 3, source_y));
									}
									break;
								}
							}
							if (current_height < wall_height)
							{
								current_height++;
							}
						}
						last_y = current_y;
						clearance = 0;
					}
					else
					{
						clearance++;
					}
				}
			}
		}
		List<Point> corner_tiles = new List<Point>();
		for (int y = 0; y < this.mapHeight; y++)
		{
			for (int j = 0; j < this.mapWidth; j++)
			{
				int height = this.GetHeight(j, y, wall_height);
				int left_height = this.GetHeight(j - 1, y, wall_height);
				int right_height = this.GetHeight(j + 1, y, wall_height);
				int top_height = this.GetHeight(j, y - 1, wall_height);
				int index = this.generationRandom.Next(0, random_wall_variants);
				if (right_height < height)
				{
					if (right_height == wall_height)
					{
						if (use_corner_hack)
						{
							corner_tiles.Add(new Point(j, y));
							this.SetTile(this.buildingsLayer, j, y, VolcanoDungeon.GetTileIndex(source_x + random_wall_variants * 3, source_y));
						}
						else
						{
							this.SetTile(this.buildingsLayer, j, y, VolcanoDungeon.GetTileIndex(source_x + random_wall_variants * 3, source_y + 1));
						}
					}
					else
					{
						Layer target_layer = this.buildingsLayer;
						if (right_height >= 0)
						{
							this.SetTile(this.buildingsLayer, j, y, VolcanoDungeon.GetTileIndex(source_x + random_wall_variants, source_y + 1 + random_wall_variants + wall_height - right_height));
							target_layer = this.frontLayer;
						}
						if (height > wall_height)
						{
							this.SetTile(target_layer, j, y, VolcanoDungeon.GetTileIndex(source_x + random_wall_variants * 3 - 1, source_y + 1 + index));
						}
						else
						{
							this.SetTile(target_layer, j, y, VolcanoDungeon.GetTileIndex(source_x + random_wall_variants * 2 + index, source_y + 1 + random_wall_variants * 2 + 1 - height - 1));
						}
						if (wall_height > 0 && y + 1 < this.mapHeight && right_height == -1 && this.GetHeight(j + 1, y + 1, wall_height) >= 0 && this.GetHeight(j, y + 1, wall_height) >= 0)
						{
							if (use_corner_hack)
							{
								corner_tiles.Add(new Point(j, y));
								this.SetTile(this.buildingsLayer, j, y, VolcanoDungeon.GetTileIndex(source_x + random_wall_variants * 3, source_y));
							}
							else
							{
								this.SetTile(this.frontLayer, j, y, VolcanoDungeon.GetTileIndex(source_x + random_wall_variants * 3, source_y + 2));
							}
						}
					}
				}
				else if (left_height < height)
				{
					if (left_height == wall_height)
					{
						if (use_corner_hack)
						{
							corner_tiles.Add(new Point(j, y));
							this.SetTile(this.buildingsLayer, j, y, VolcanoDungeon.GetTileIndex(source_x + random_wall_variants * 3, source_y));
						}
						else
						{
							this.SetTile(this.buildingsLayer, j, y, VolcanoDungeon.GetTileIndex(source_x + random_wall_variants * 3 + 1, source_y + 1));
						}
					}
					else
					{
						Layer target_layer2 = this.buildingsLayer;
						if (left_height >= 0)
						{
							this.SetTile(this.buildingsLayer, j, y, VolcanoDungeon.GetTileIndex(source_x + random_wall_variants, source_y + 1 + random_wall_variants + wall_height - left_height));
							target_layer2 = this.frontLayer;
						}
						if (height > wall_height)
						{
							this.SetTile(target_layer2, j, y, VolcanoDungeon.GetTileIndex(source_x, source_y + 1 + index));
						}
						else
						{
							this.SetTile(target_layer2, j, y, VolcanoDungeon.GetTileIndex(source_x + index, source_y + 1 + random_wall_variants * 2 + 1 - height - 1));
						}
						if (wall_height > 0 && y + 1 < this.mapHeight && left_height == -1 && this.GetHeight(j - 1, y + 1, wall_height) >= 0 && this.GetHeight(j, y + 1, wall_height) >= 0)
						{
							if (use_corner_hack)
							{
								corner_tiles.Add(new Point(j, y));
								this.SetTile(this.buildingsLayer, j, y, VolcanoDungeon.GetTileIndex(source_x + random_wall_variants * 3, source_y));
							}
							else
							{
								this.SetTile(this.frontLayer, j, y, VolcanoDungeon.GetTileIndex(source_x + random_wall_variants * 3 + 1, source_y + 2));
							}
						}
					}
				}
				if (height < 0 || top_height != -1)
				{
					continue;
				}
				if (wall_height > 0)
				{
					if (right_height == -1)
					{
						this.SetTile(this.frontLayer, j, y - 1, VolcanoDungeon.GetTileIndex(source_x + random_wall_variants * 2 + index, source_y));
					}
					else if (left_height == -1)
					{
						this.SetTile(this.frontLayer, j, y - 1, VolcanoDungeon.GetTileIndex(source_x + index, source_y));
					}
					else
					{
						this.SetTile(this.frontLayer, j, y - 1, VolcanoDungeon.GetTileIndex(source_x + random_wall_variants + index, source_y));
					}
				}
				else if (right_height == -1)
				{
					this.SetTile(this.buildingsLayer, j, y, VolcanoDungeon.GetTileIndex(source_x + random_wall_variants * 2 + index, source_y));
				}
				else if (left_height == -1)
				{
					this.SetTile(this.buildingsLayer, j, y, VolcanoDungeon.GetTileIndex(source_x + index, source_y));
				}
				else
				{
					this.SetTile(this.buildingsLayer, j, y, VolcanoDungeon.GetTileIndex(source_x + random_wall_variants + index, source_y));
				}
			}
		}
		if (use_corner_hack)
		{
			foreach (Point corner_tile in corner_tiles)
			{
				if (this.GetHeight(corner_tile.X - 1, corner_tile.Y, wall_height) == -1)
				{
					this.SetTile(this.frontLayer, corner_tile.X, corner_tile.Y, VolcanoDungeon.GetTileIndex(source_x + random_wall_variants * 3 + 1, source_y + 2));
				}
				else if (this.GetHeight(corner_tile.X + 1, corner_tile.Y, wall_height) == -1)
				{
					this.SetTile(this.frontLayer, corner_tile.X, corner_tile.Y, VolcanoDungeon.GetTileIndex(source_x + random_wall_variants * 3, source_y + 2));
				}
				if (this.GetHeight(corner_tile.X - 1, corner_tile.Y, wall_height) == wall_height)
				{
					this.SetTile(this.alwaysFrontLayer, corner_tile.X, corner_tile.Y, VolcanoDungeon.GetTileIndex(source_x + random_wall_variants * 3 + 1, source_y + 1));
				}
				else if (this.GetHeight(corner_tile.X + 1, corner_tile.Y, wall_height) == wall_height)
				{
					this.SetTile(this.alwaysFrontLayer, corner_tile.X, corner_tile.Y, VolcanoDungeon.GetTileIndex(source_x + random_wall_variants * 3, source_y + 1));
				}
			}
		}
		this.heightMap = null;
	}

	public int GetPixelClearance(int x, int y, int wall_height, Color match)
	{
		int current_height = 0;
		if (this.GetPixel(x, y, Color.White) == match)
		{
			current_height++;
			for (int i = 1; i < wall_height; i++)
			{
				if (current_height >= wall_height)
				{
					break;
				}
				if (y + i >= this.mapHeight)
				{
					return wall_height;
				}
				if (!(this.GetPixel(x, y + i, Color.White) == match))
				{
					break;
				}
				current_height++;
			}
			for (int j = 1; j < wall_height; j++)
			{
				if (current_height >= wall_height)
				{
					break;
				}
				if (y - j < 0)
				{
					return wall_height;
				}
				if (!(this.GetPixel(x, y - j, Color.White) == match))
				{
					break;
				}
				current_height++;
			}
			return current_height;
		}
		return 0;
	}

	public override void UpdateWhenCurrentLocation(GameTime time)
	{
		base.UpdateWhenCurrentLocation(time);
		this.coolLavaEvent.Poll();
		this.lavaSoundsPlayedThisTick = 0;
		if (this.level.Value == 0 && Game1.currentLocation == this)
		{
			this.steamTimer -= (float)time.ElapsedGameTime.TotalMilliseconds;
			if (this.steamTimer < 0f)
			{
				this.steamTimer = 5000f;
				Game1.playSound("cavedrip");
				base.temporarySprites.Add(new TemporaryAnimatedSprite(null, new Microsoft.Xna.Framework.Rectangle(0, 0, 1, 1), new Vector2(34.5f, 30.75f) * 64f, flipped: false, 0f, Color.White)
				{
					texture = Game1.staminaRect,
					color = new Color(100, 150, 255),
					alpha = 0.75f,
					motion = new Vector2(0f, 1f),
					acceleration = new Vector2(0f, 0.1f),
					interval = 99999f,
					layerDepth = 1f,
					scale = 8f,
					id = 89898,
					yStopCoordinate = 2208,
					reachedStopCoordinate = delegate
					{
						base.removeTemporarySpritesWithID(89898);
						Game1.playSound("steam");
						for (int i = 0; i < 4; i++)
						{
							base.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(372, 1956, 10, 10), new Vector2(33.75f, 33.5f) * 64f + new Vector2(Game1.random.Next(64), Game1.random.Next(64)), flipped: false, 0.007f, Color.White)
							{
								alpha = 0.75f,
								motion = new Vector2(0f, -1f),
								acceleration = new Vector2(0.002f, 0f),
								interval = 99999f,
								layerDepth = 1f,
								scale = 4f,
								scaleChange = 0.02f,
								rotationChange = (float)Game1.random.Next(-5, 6) * (float)Math.PI / 256f
							});
						}
					}
				});
			}
		}
		foreach (DwarfGate dwarfGate in this.dwarfGates)
		{
			dwarfGate.UpdateWhenCurrentLocation(time, this);
		}
		if (!this._sawFlameSprite && Utility.isThereAFarmerWithinDistance(new Vector2(30f, 38f), 3, this) != null)
		{
			Game1.addMailForTomorrow("Saw_Flame_Sprite_Volcano", noLetter: true);
			TemporaryAnimatedSprite v = base.getTemporarySpriteByID(999);
			if (v != null)
			{
				v.yPeriodic = false;
				v.xPeriodic = false;
				v.sourceRect.Y = 0;
				v.sourceRectStartingPos.Y = 0f;
				v.motion = new Vector2(0f, -4f);
				v.acceleration = new Vector2(0f, -0.04f);
			}
			base.localSound("magma_sprite_spot");
			v = base.getTemporarySpriteByID(998);
			if (v != null)
			{
				v.yPeriodic = false;
				v.xPeriodic = false;
				v.motion = new Vector2(0f, -4f);
				v.acceleration = new Vector2(0f, -0.04f);
			}
			this._sawFlameSprite = true;
		}
	}

	public virtual void PlaceGroundTile(int x, int y)
	{
		if (this.generationRandom.NextDouble() < 0.30000001192092896)
		{
			this.SetTile(this.backLayer, x, y, VolcanoDungeon.GetTileIndex(1 + this.generationRandom.Next(0, 3), this.generationRandom.Next(0, 2)));
		}
		else
		{
			this.SetTile(this.backLayer, x, y, VolcanoDungeon.GetTileIndex(1, 0));
		}
	}

	public override void drawFloorDecorations(SpriteBatch b)
	{
		base.drawFloorDecorations(b);
		for (int y = Game1.viewport.Y / 64 - 1; y < (Game1.viewport.Y + Game1.viewport.Height) / 64 + 1; y++)
		{
			for (int x = Game1.viewport.X / 64 - 1; x < (Game1.viewport.X + Game1.viewport.Width) / 64 + 1; x++)
			{
				Vector2 tile = new Vector2(x, y);
				if (this.localCooledLavaTiles.TryGetValue(tile, out var point))
				{
					point.X += 5;
					point.Y += 16;
					b.Draw(this.mapBaseTilesheet, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, y * 64)), new Microsoft.Xna.Framework.Rectangle(point.X * 16, point.Y * 16, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.55f);
				}
			}
		}
	}

	public override void drawWaterTile(SpriteBatch b, int x, int y)
	{
		if (this.level.Value == 5)
		{
			base.drawWaterTile(b, x, y);
			return;
		}
		if (this.level.Value == 0 && x > 23 && x < 28 && y > 42 && y < 47)
		{
			base.drawWaterTile(b, x, y, Color.DeepSkyBlue * 0.8f);
			return;
		}
		bool num = y == base.map.Layers[0].LayerHeight - 1 || !base.waterTiles[x, y + 1];
		bool topY = y == 0 || !base.waterTiles[x, y - 1];
		int water_tile_upper_left_x = 0;
		int water_tile_upper_left_y = 320;
		b.Draw(this.mapBaseTilesheet, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, y * 64 - (int)((!topY) ? base.waterPosition : 0f))), new Microsoft.Xna.Framework.Rectangle(water_tile_upper_left_x + base.waterAnimationIndex * 16, water_tile_upper_left_y + (((x + y) % 2 != 0) ? ((!base.waterTileFlip) ? 32 : 0) : (base.waterTileFlip ? 32 : 0)) + (topY ? ((int)base.waterPosition / 4) : 0), 16, 16 + (topY ? ((int)(0f - base.waterPosition) / 4) : 0)), base.waterColor.Value, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.56f);
		if (num)
		{
			b.Draw(this.mapBaseTilesheet, Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, (y + 1) * 64 - (int)base.waterPosition)), new Microsoft.Xna.Framework.Rectangle(water_tile_upper_left_x + base.waterAnimationIndex * 16, water_tile_upper_left_y + (((x + (y + 1)) % 2 != 0) ? ((!base.waterTileFlip) ? 32 : 0) : (base.waterTileFlip ? 32 : 0)), 16, 16 - (int)(16f - base.waterPosition / 4f) - 1), base.waterColor.Value, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.56f);
		}
	}

	public override void draw(SpriteBatch b)
	{
		base.draw(b);
		foreach (DwarfGate dwarfGate in this.dwarfGates)
		{
			dwarfGate.Draw(b);
		}
	}

	public override void drawAboveAlwaysFrontLayer(SpriteBatch b)
	{
		base.drawAboveAlwaysFrontLayer(b);
		if (!Game1.game1.takingMapScreenshot && this.level.Value > 0)
		{
			Color col = SpriteText.color_Red;
			string txt = this.level.Value.ToString() ?? "";
			Microsoft.Xna.Framework.Rectangle tsarea = Game1.game1.GraphicsDevice.Viewport.GetTitleSafeArea();
			SpriteText.drawString(b, txt, tsarea.Left + 16, tsarea.Top + 16, 999999, -1, 999999, 1f, 1f, junimoText: false, 2, "", col);
		}
	}

	public override void performTenMinuteUpdate(int timeOfDay)
	{
		base.performTenMinuteUpdate(timeOfDay);
		if (!(Game1.random.NextDouble() < 0.1) || this.level.Value <= 0 || this.level.Value == 5)
		{
			return;
		}
		int numsprites = 0;
		foreach (NPC character in base.characters)
		{
			if (character is Bat)
			{
				numsprites++;
			}
		}
		if (numsprites < base.farmers.Count * 4)
		{
			this.spawnFlyingMonsterOffScreen();
		}
	}

	public void spawnFlyingMonsterOffScreen()
	{
		Vector2 spawnLocation = Vector2.Zero;
		switch (Game1.random.Next(4))
		{
		case 0:
			spawnLocation.X = Game1.random.Next(base.map.Layers[0].LayerWidth);
			break;
		case 3:
			spawnLocation.Y = Game1.random.Next(base.map.Layers[0].LayerHeight);
			break;
		case 1:
			spawnLocation.X = base.map.Layers[0].LayerWidth - 1;
			spawnLocation.Y = Game1.random.Next(base.map.Layers[0].LayerHeight);
			break;
		case 2:
			spawnLocation.Y = base.map.Layers[0].LayerHeight - 1;
			spawnLocation.X = Game1.random.Next(base.map.Layers[0].LayerWidth);
			break;
		}
		base.playSound("magma_sprite_spot");
		base.characters.Add(new Bat(spawnLocation, (this.level.Value > 5 && Game1.random.NextBool()) ? (-556) : (-555))
		{
			focusedOnFarmers = true
		});
	}

	public virtual void PlaceSingleWall(int x, int y)
	{
		int index = this.generationRandom.Next(0, 4);
		this.SetTile(this.frontLayer, x, y - 1, VolcanoDungeon.GetTileIndex(index, 2));
		this.SetTile(this.buildingsLayer, x, y, VolcanoDungeon.GetTileIndex(index, 3));
	}

	public virtual void ApplyPixels(string layout_texture_name, int source_x = 0, int source_y = 0, int width = 64, int height = 64, int x_offset = 0, int y_offset = 0, bool flip_x = false)
	{
		Texture2D texture2D = Game1.temporaryContent.Load<Texture2D>(layout_texture_name);
		Color[] pixels = new Color[width * height];
		texture2D.GetData(0, new Microsoft.Xna.Framework.Rectangle(source_x, source_y, width, height), pixels, 0, width * height);
		for (int base_x = 0; base_x < width; base_x++)
		{
			int x = base_x + x_offset;
			if (flip_x)
			{
				x = x_offset + width - 1 - base_x;
			}
			if (x < 0 || x >= this.mapWidth)
			{
				continue;
			}
			for (int base_y = 0; base_y < height; base_y++)
			{
				int y = base_y + y_offset;
				if (y >= 0 && y < this.mapHeight)
				{
					Color pixel_color = this.GetPixelColor(width, height, pixels, base_x, base_y);
					this.SetPixelMap(x, y, pixel_color);
				}
			}
		}
	}

	public int GetHeight(int x, int y, int max_height)
	{
		if (x < 0 || x >= this.mapWidth || y < 0 || y >= this.mapHeight)
		{
			return max_height + 1;
		}
		return this.heightMap[x + y * this.mapWidth];
	}

	public Color GetPixel(int x, int y, Color out_of_bounds_color)
	{
		if (x < 0 || x >= this.mapWidth || y < 0 || y >= this.mapHeight)
		{
			return out_of_bounds_color;
		}
		return this.pixelMap[x + y * this.mapWidth];
	}

	public void SetPixelMap(int x, int y, Color color)
	{
		if (x >= 0 && x < this.mapWidth && y >= 0 && y < this.mapHeight)
		{
			this.pixelMap[x + y * this.mapWidth] = color;
		}
	}

	public int GetNeighborValue(int x, int y, Color matched_color, bool is_lava_pool = false)
	{
		int neighbor_value = 0;
		if (this.GetPixel(x, y - 1, matched_color) == matched_color)
		{
			neighbor_value++;
		}
		if (this.GetPixel(x, y + 1, matched_color) == matched_color)
		{
			neighbor_value += 2;
		}
		if (this.GetPixel(x + 1, y, matched_color) == matched_color)
		{
			neighbor_value += 4;
		}
		if (this.GetPixel(x - 1, y, matched_color) == matched_color)
		{
			neighbor_value += 8;
		}
		if (is_lava_pool && neighbor_value == 15)
		{
			if (this.GetPixel(x - 1, y - 1, matched_color) == matched_color)
			{
				neighbor_value += 16;
			}
			if (this.GetPixel(x + 1, y - 1, matched_color) == matched_color)
			{
				neighbor_value += 32;
			}
		}
		return neighbor_value;
	}

	public Color GetPixelColor(int width, int height, Color[] pixels, int x, int y)
	{
		if (x < 0 || x >= width)
		{
			return Color.Black;
		}
		if (y < 0 || y >= height)
		{
			return Color.Black;
		}
		int index = x + y * width;
		return pixels[index];
	}

	public static int GetTileIndex(int x, int y)
	{
		return x + y * 16;
	}

	public void SetTile(Layer layer, int x, int y, int index)
	{
		if (x >= 0 && x < layer.LayerWidth && y >= 0 && y < layer.LayerHeight)
		{
			Location location = new Location(x, y);
			TileSheet mainTileSheet = base.map.RequireTileSheet(0, "dungeon");
			layer.Tiles[location] = new StaticTile(layer, mainTileSheet, BlendMode.Alpha, index);
		}
	}

	public int GetMaxRoomLayouts()
	{
		return 30;
	}

	public static VolcanoDungeon GetLevel(string name, bool use_level_level_as_layout = false)
	{
		foreach (VolcanoDungeon level in VolcanoDungeon.activeLevels)
		{
			if (level.Name.Equals(name))
			{
				return level;
			}
		}
		if (!VolcanoDungeon.IsGeneratedLevel(name, out var newLevelNumber))
		{
			Game1.log.Warn("Failed parsing Volcano Dungeon level from location name '" + name + "', defaulting to level 0.");
			newLevelNumber = 0;
		}
		VolcanoDungeon new_level = new VolcanoDungeon(newLevelNumber);
		VolcanoDungeon.activeLevels.Add(new_level);
		if (Game1.IsMasterGame)
		{
			new_level.GenerateContents(use_level_level_as_layout);
		}
		else
		{
			new_level.reloadMap();
		}
		return new_level;
	}

	/// <summary>Get the location name for a generated Volcano Dungeon level.</summary>
	/// <param name="level">The dungeon level.</param>
	public static string GetLevelName(int level)
	{
		return "VolcanoDungeon" + level;
	}

	/// <param name="locationName">The location name to check.</param>
	public static bool IsGeneratedLevel(string locationName)
	{
		int num;
		return VolcanoDungeon.IsGeneratedLevel(locationName, out num);
	}

	/// <summary>Get whether a location name is a generated Volcano Dungeon level.</summary>
	/// <param name="locationName">The location name to check.</param>
	/// <param name="level">The parsed dungeon level, if applicable.</param>
	public static bool IsGeneratedLevel(string locationName, out int level)
	{
		if (locationName == null || !locationName.StartsWithIgnoreCase("VolcanoDungeon"))
		{
			level = 0;
			return false;
		}
		return int.TryParse(locationName.Substring("VolcanoDungeon".Length), out level);
	}

	public static void UpdateLevels(GameTime time)
	{
		foreach (VolcanoDungeon level in VolcanoDungeon.activeLevels)
		{
			if (level.farmers.Count > 0)
			{
				level.UpdateWhenCurrentLocation(time);
			}
			level.updateEvenIfFarmerIsntHere(time);
		}
	}

	public static void UpdateLevels10Minutes(int timeOfDay)
	{
		if (Game1.IsClient)
		{
			return;
		}
		foreach (VolcanoDungeon level in VolcanoDungeon.activeLevels)
		{
			if (level.farmers.Count > 0)
			{
				level.performTenMinuteUpdate(timeOfDay);
			}
		}
	}

	public static void ClearAllLevels()
	{
		VolcanoDungeon.activeLevels.RemoveAll(delegate(VolcanoDungeon level)
		{
			level.OnRemoved();
			return true;
		});
	}

	/// <inheritdoc />
	public override void OnRemoved()
	{
		base.OnRemoved();
		if (Game1.IsMasterGame)
		{
			base.debris.RemoveWhere((Debris d) => d.isEssentialItem() && d.collect(Game1.player));
		}
		this.mapContent.Dispose();
	}

	public static void ForEach(Action<VolcanoDungeon> action)
	{
		foreach (VolcanoDungeon level in VolcanoDungeon.activeLevels)
		{
			action(level);
		}
	}

	/// <inheritdoc />
	public override bool ShouldExcludeFromNpcPathfinding()
	{
		return true;
	}

	public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
	{
		switch (base.getTileIndexAt(tileLocation.X, tileLocation.Y, "Buildings", "dungeon"))
		{
		case 367:
			base.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:Volcano_ShortcutOut"), base.createYesNoResponses(), "LeaveVolcano");
			return true;
		case 77:
			if (Game1.player.canUnderstandDwarves)
			{
				Utility.TryOpenShopMenu("VolcanoShop", null, playOpenSound: true);
			}
			else
			{
				Game1.player.doEmote(8);
			}
			return true;
		default:
			return base.checkAction(tileLocation, viewport, who);
		}
	}

	/// <inheritdoc />
	public override void performTouchAction(string[] action, Vector2 playerStandingPosition)
	{
		if (this.IgnoreTouchActions())
		{
			return;
		}
		if (ArgUtility.Get(action, 0) == "DwarfSwitch")
		{
			Point tile_point = new Point((int)playerStandingPosition.X, (int)playerStandingPosition.Y);
			{
				foreach (DwarfGate gate in this.dwarfGates)
				{
					if (gate.switches.TryGetValue(tile_point, out var wasPressed) && !wasPressed)
					{
						gate.pressEvent.Fire(tile_point);
					}
				}
				return;
			}
		}
		base.performTouchAction(action, playerStandingPosition);
	}
}
