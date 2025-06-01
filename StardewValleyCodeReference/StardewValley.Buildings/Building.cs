using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using Netcode.Validation;
using StardewValley.Delegates;
using StardewValley.Extensions;
using StardewValley.GameData;
using StardewValley.GameData.Buildings;
using StardewValley.Internal;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Mods;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using StardewValley.TokenizableStrings;
using StardewValley.Util;
using xTile.Dimensions;

namespace StardewValley.Buildings;

[XmlInclude(typeof(Barn))]
[XmlInclude(typeof(Coop))]
[XmlInclude(typeof(FishPond))]
[XmlInclude(typeof(GreenhouseBuilding))]
[XmlInclude(typeof(JunimoHut))]
[XmlInclude(typeof(Mill))]
[XmlInclude(typeof(PetBowl))]
[XmlInclude(typeof(ShippingBin))]
[XmlInclude(typeof(Stable))]
[NotImplicitNetField]
public class Building : INetObject<NetFields>, IHaveModData
{
	/// <summary>A unique identifier for this specific building instance.</summary>
	[XmlElement("id")]
	public readonly NetGuid id = new NetGuid();

	[XmlIgnore]
	public Lazy<Texture2D> texture;

	[XmlIgnore]
	public Texture2D paintedTexture;

	public NetString skinId = new NetString();

	/// <summary>The indoor location created for this building, if any.</summary>
	/// <remarks>This is mutually exclusive with <see cref="F:StardewValley.Buildings.Building.nonInstancedIndoorsName" />. Most code should use <see cref="M:StardewValley.Buildings.Building.GetIndoors" /> instead, which handles both.</remarks>
	[XmlElement("indoors")]
	public readonly NetRef<GameLocation> indoors = new NetRef<GameLocation>();

	/// <summary>The unique ID of the separate location treated as the building interior (like <c>FarmHouse</c> for the farmhouse), if any.</summary>
	/// <remarks>This is mutually exclusive with <see cref="F:StardewValley.Buildings.Building.indoors" />. Most code should use <see cref="M:StardewValley.Buildings.Building.GetIndoors" /> instead, which handles both.</remarks>
	public readonly NetString nonInstancedIndoorsName = new NetString();

	[XmlElement("tileX")]
	public readonly NetInt tileX = new NetInt();

	[XmlElement("tileY")]
	public readonly NetInt tileY = new NetInt();

	[XmlElement("tilesWide")]
	public readonly NetInt tilesWide = new NetInt();

	[XmlElement("tilesHigh")]
	public readonly NetInt tilesHigh = new NetInt();

	[XmlElement("maxOccupants")]
	public readonly NetInt maxOccupants = new NetInt();

	[XmlElement("currentOccupants")]
	public readonly NetInt currentOccupants = new NetInt();

	[XmlElement("daysOfConstructionLeft")]
	public readonly NetInt daysOfConstructionLeft = new NetInt();

	[XmlElement("daysUntilUpgrade")]
	public readonly NetInt daysUntilUpgrade = new NetInt();

	[XmlElement("upgradeName")]
	public readonly NetString upgradeName = new NetString();

	[XmlElement("buildingType")]
	public readonly NetString buildingType = new NetString();

	[XmlElement("buildingPaintColor")]
	public NetRef<BuildingPaintColor> netBuildingPaintColor = new NetRef<BuildingPaintColor>();

	[XmlElement("hayCapacity")]
	public NetInt hayCapacity = new NetInt();

	public NetList<Chest, NetRef<Chest>> buildingChests = new NetList<Chest, NetRef<Chest>>();

	/// <summary>The unique name of the location which contains this building.</summary>
	[XmlIgnore]
	public NetString parentLocationName = new NetString();

	[XmlIgnore]
	public bool hasLoaded;

	[XmlIgnore]
	protected Dictionary<string, string> buildingMetadata = new Dictionary<string, string>();

	protected int lastHouseUpgradeLevel = -1;

	protected bool? hasChimney;

	protected Vector2 chimneyPosition = Vector2.Zero;

	protected int chimneyTimer = 500;

	[XmlElement("humanDoor")]
	public readonly NetPoint humanDoor = new NetPoint();

	[XmlElement("animalDoor")]
	public readonly NetPoint animalDoor = new NetPoint();

	/// <summary>A temporary color applied to the building sprite when it's highlighted in a menu.</summary>
	[XmlIgnore]
	public Color color = Color.White;

	[XmlElement("animalDoorOpen")]
	public readonly NetBool animalDoorOpen = new NetBool();

	[XmlElement("animalDoorOpenAmount")]
	public readonly NetFloat animalDoorOpenAmount = new NetFloat
	{
		InterpolationWait = false
	};

	[XmlElement("magical")]
	public readonly NetBool magical = new NetBool();

	/// <summary>Whether this building should fade into semi-transparency when the local player is behind it.</summary>
	[XmlElement("fadeWhenPlayerIsBehind")]
	public readonly NetBool fadeWhenPlayerIsBehind = new NetBool(value: true);

	[XmlElement("owner")]
	public readonly NetLong owner = new NetLong();

	[XmlElement("newConstructionTimer")]
	protected readonly NetInt newConstructionTimer = new NetInt();

	/// <summary>The building's opacity for the local player as a value between 0 (transparent) and 1 (opaque), accounting for <see cref="F:StardewValley.Buildings.Building.fadeWhenPlayerIsBehind" />.</summary>
	[XmlIgnore]
	public float alpha = 1f;

	[XmlIgnore]
	protected bool _isMoving;

	public static Microsoft.Xna.Framework.Rectangle leftShadow = new Microsoft.Xna.Framework.Rectangle(656, 394, 16, 16);

	public static Microsoft.Xna.Framework.Rectangle middleShadow = new Microsoft.Xna.Framework.Rectangle(672, 394, 16, 16);

	public static Microsoft.Xna.Framework.Rectangle rightShadow = new Microsoft.Xna.Framework.Rectangle(688, 394, 16, 16);

	/// <inheritdoc />
	[XmlIgnore]
	public ModDataDictionary modData { get; } = new ModDataDictionary();

	/// <inheritdoc />
	[XmlElement("modData")]
	public ModDataDictionary modDataForSerialization
	{
		get
		{
			return this.modData.GetForSerialization();
		}
		set
		{
			this.modData.SetFromSerialization(value);
		}
	}

	/// <summary>Get whether this is a farmhand cabin.</summary>
	/// <remarks>To check whether a farmhand has claimed it, use <see cref="M:StardewValley.Buildings.Building.GetIndoors" /> to get the <see cref="T:StardewValley.Locations.Cabin" /> or <see cref="T:StardewValley.Locations.FarmHouse" /> instance and call methods like <see cref="P:StardewValley.Locations.FarmHouse.HasOwner" />.</remarks>
	public bool isCabin => this.buildingType.Value == "Cabin";

	public bool isMoving
	{
		get
		{
			return this._isMoving;
		}
		set
		{
			if (this._isMoving != value)
			{
				this._isMoving = value;
				if (this._isMoving)
				{
					this.OnStartMove();
				}
				if (!this._isMoving)
				{
					this.OnEndMove();
				}
			}
		}
	}

	public NetFields NetFields { get; } = new NetFields("Building");

	/// <summary>Construct an instance.</summary>
	public Building()
	{
		this.id.Value = Guid.NewGuid();
		this.resetTexture();
		this.initNetFields();
	}

	/// <summary>Construct an instance.</summary>
	/// <param name="type">The building type ID in <see cref="F:StardewValley.Game1.buildingData" />.</param>
	/// <param name="tile">The top-left tile position of the building.</param>
	public Building(string type, Vector2 tile)
		: this()
	{
		this.tileX.Value = (int)tile.X;
		this.tileY.Value = (int)tile.Y;
		this.buildingType.Value = type;
		BuildingData data = this.ReloadBuildingData();
		this.daysOfConstructionLeft.Value = data?.BuildDays ?? 0;
	}

	/// <summary>Get whether the building has any skins that can be applied to it currently.</summary>
	/// <param name="ignoreSeparateConstructionEntries">Whether to ignore skins with <see cref="F:StardewValley.GameData.Buildings.BuildingSkin.ShowAsSeparateConstructionEntry" /> set to true.</param>
	public virtual bool CanBeReskinned(bool ignoreSeparateConstructionEntries = false)
	{
		BuildingData data = this.GetData();
		if (this.skinId.Value != null)
		{
			return true;
		}
		if (data?.Skins != null)
		{
			foreach (BuildingSkin skin in data.Skins)
			{
				if (!(skin.Id == this.skinId.Value) && (!ignoreSeparateConstructionEntries || !skin.ShowAsSeparateConstructionEntry) && GameStateQuery.CheckConditions(skin.Condition, this.GetParentLocation()))
				{
					return true;
				}
			}
		}
		return false;
	}

	/// <summary>Get whether animals within this building can get pregnant and produce offspring.</summary>
	public bool AllowsAnimalPregnancy()
	{
		return this.GetData()?.AllowAnimalPregnancy ?? false;
	}

	/// <summary>Get whether players can repaint this building.</summary>
	public virtual bool CanBePainted()
	{
		if (this is GreenhouseBuilding && !Game1.getFarm().greenhouseUnlocked.Value)
		{
			return false;
		}
		if ((this.isCabin || this.HasIndoorsName("Farmhouse")) && this.GetIndoors() is FarmHouse { upgradeLevel: <2 })
		{
			return false;
		}
		return this.GetPaintDataKey() != null;
	}

	/// <summary>Get the building's current skin, if applicable.</summary>
	public BuildingSkin GetSkin()
	{
		return Building.GetSkin(this.skinId.Value, this.GetData());
	}

	/// <summary>Get a building skin from data, if it exists.</summary>
	/// <param name="skinId">The building skin ID to find.</param>
	/// <param name="data">The building data to search.</param>
	/// <returns>Returns the matching building skin if found, else <c>null</c>.</returns>
	public static BuildingSkin GetSkin(string skinId, BuildingData data)
	{
		if (skinId != null && data?.Skins != null)
		{
			foreach (BuildingSkin skin in data.Skins)
			{
				if (skin.Id == skinId)
				{
					return skin;
				}
			}
		}
		return null;
	}

	/// <summary>Get the key in <c>Data/PaintData</c> for the building, if it has any.</summary>
	public virtual string GetPaintDataKey()
	{
		Dictionary<string, string> asset = DataLoader.PaintData(Game1.content);
		return this.GetPaintDataKey(asset);
	}

	/// <summary>Get the key in <c>Data/PaintData</c> for the building, if it has any.</summary>
	/// <param name="paintData">The loaded <c>Data/PaintData</c> asset.</param>
	public virtual string GetPaintDataKey(Dictionary<string, string> paintData)
	{
		if (this.skinId.Value != null && paintData.ContainsKey(this.skinId.Value))
		{
			return this.skinId.Value;
		}
		string value = this.buildingType.Value;
		string lookupName = ((value == "Farmhouse") ? "House" : ((!(value == "Cabin")) ? this.buildingType.Value : "Stone Cabin"));
		if (!paintData.ContainsKey(lookupName))
		{
			return null;
		}
		return lookupName;
	}

	public string GetMetadata(string key)
	{
		if (this.buildingMetadata == null)
		{
			this.buildingMetadata = new Dictionary<string, string>();
			BuildingData data = this.GetData();
			if (data != null)
			{
				foreach (KeyValuePair<string, string> kvp in data.Metadata)
				{
					this.buildingMetadata[kvp.Key] = kvp.Value;
				}
				BuildingSkin skin = Building.GetSkin(this.skinId.Value, data);
				if (skin != null)
				{
					foreach (KeyValuePair<string, string> kvp2 in skin.Metadata)
					{
						this.buildingMetadata[kvp2.Key] = kvp2.Value;
					}
				}
			}
		}
		if (!this.buildingMetadata.TryGetValue(key, out key))
		{
			return null;
		}
		return key;
	}

	/// <summary>Get the location which contains this building.</summary>
	public GameLocation GetParentLocation()
	{
		return Game1.getLocationFromName(this.parentLocationName.Value);
	}

	/// <summary>Get whether the building is in <see cref="P:StardewValley.Game1.currentLocation" />.</summary>
	public bool IsInCurrentLocation()
	{
		if (Game1.currentLocation != null)
		{
			return Game1.currentLocation.NameOrUniqueName == this.parentLocationName.Value;
		}
		return false;
	}

	public virtual bool hasCarpenterPermissions()
	{
		if (Game1.IsMasterGame)
		{
			return true;
		}
		if (this.owner.Value == Game1.player.UniqueMultiplayerID)
		{
			return true;
		}
		if (this.GetIndoors() is FarmHouse { IsOwnedByCurrentPlayer: not false })
		{
			return true;
		}
		return false;
	}

	protected virtual void initNetFields()
	{
		this.NetFields.SetOwner(this).AddField(this.id, "id").AddField(this.indoors, "indoors")
			.AddField(this.nonInstancedIndoorsName, "nonInstancedIndoorsName")
			.AddField(this.tileX, "tileX")
			.AddField(this.tileY, "tileY")
			.AddField(this.tilesWide, "tilesWide")
			.AddField(this.tilesHigh, "tilesHigh")
			.AddField(this.maxOccupants, "maxOccupants")
			.AddField(this.currentOccupants, "currentOccupants")
			.AddField(this.daysOfConstructionLeft, "daysOfConstructionLeft")
			.AddField(this.daysUntilUpgrade, "daysUntilUpgrade")
			.AddField(this.buildingType, "buildingType")
			.AddField(this.humanDoor, "humanDoor")
			.AddField(this.animalDoor, "animalDoor")
			.AddField(this.magical, "magical")
			.AddField(this.fadeWhenPlayerIsBehind, "fadeWhenPlayerIsBehind")
			.AddField(this.animalDoorOpen, "animalDoorOpen")
			.AddField(this.owner, "owner")
			.AddField(this.newConstructionTimer, "newConstructionTimer")
			.AddField(this.netBuildingPaintColor, "netBuildingPaintColor")
			.AddField(this.buildingChests, "buildingChests")
			.AddField(this.animalDoorOpenAmount, "animalDoorOpenAmount")
			.AddField(this.hayCapacity, "hayCapacity")
			.AddField(this.parentLocationName, "parentLocationName")
			.AddField(this.upgradeName, "upgradeName")
			.AddField(this.skinId, "skinId")
			.AddField(this.modData, "modData");
		this.buildingType.fieldChangeVisibleEvent += delegate(NetString a, string b, string c)
		{
			this.hasChimney = null;
			bool forUpgrade = b != null && b != c;
			this.ReloadBuildingData(forUpgrade);
		};
		this.skinId.fieldChangeVisibleEvent += delegate
		{
			this.hasChimney = null;
			this.buildingMetadata = null;
			this.resetTexture();
		};
		this.buildingType.fieldChangeVisibleEvent += delegate
		{
			this.hasChimney = null;
			this.buildingMetadata = null;
			this.resetTexture();
		};
		this.indoors.fieldChangeVisibleEvent += delegate
		{
			this.UpdateIndoorParent();
		};
		this.parentLocationName.fieldChangeVisibleEvent += delegate
		{
			this.UpdateIndoorParent();
		};
		if (this.netBuildingPaintColor.Value == null)
		{
			this.netBuildingPaintColor.Value = new BuildingPaintColor();
		}
	}

	public virtual void UpdateIndoorParent()
	{
		GameLocation interior = this.GetIndoors();
		if (interior != null)
		{
			interior.ParentBuilding = this;
			interior.parentLocationName.Value = this.parentLocationName.Value;
		}
	}

	/// <summary>Get the building's data from <see cref="F:StardewValley.Game1.buildingData" />, if found.</summary>
	public virtual BuildingData GetData()
	{
		if (!Building.TryGetData(this.buildingType.Value, out var data))
		{
			return null;
		}
		return data;
	}

	/// <summary>Try to get a building's data from <see cref="F:StardewValley.Game1.buildingData" />.</summary>
	/// <param name="buildingType">The building type (i.e. the key in <see cref="F:StardewValley.Game1.buildingData" />).</param>
	/// <param name="data">The building data, if found.</param>
	/// <returns>Returns whether the building data was found.</returns>
	public static bool TryGetData(string buildingType, out BuildingData data)
	{
		if (buildingType == null)
		{
			data = null;
			return false;
		}
		return Game1.buildingData.TryGetValue(buildingType, out data);
	}

	/// <summary>Reload the building's data from <see cref="F:StardewValley.Game1.buildingData" /> and reapply it to the building's fields.</summary>
	/// <param name="forUpgrade">Whether the building is being upgraded.</param>
	/// <param name="forConstruction">Whether the building is being constructed.</param>
	/// <returns>Returns the loaded building data, if any.</returns>
	/// <remarks>See also <see cref="M:StardewValley.Buildings.Building.LoadFromBuildingData(StardewValley.GameData.Buildings.BuildingData,System.Boolean,System.Boolean)" />.</remarks>
	public virtual BuildingData ReloadBuildingData(bool forUpgrade = false, bool forConstruction = false)
	{
		BuildingData data = this.GetData();
		if (data != null)
		{
			this.LoadFromBuildingData(data, forUpgrade, forConstruction);
		}
		return data;
	}

	/// <summary>Reapply the loaded data to the building's fields.</summary>
	/// <param name="data">The building data to load.</param>
	/// <param name="forUpgrade">Whether the building is being upgraded.</param>
	/// <param name="forConstruction">Whether the building is being constructed.</param>
	/// <remarks>This doesn't reload the underlying data; see <see cref="M:StardewValley.Buildings.Building.ReloadBuildingData(System.Boolean,System.Boolean)" /> if you need to do that.</remarks>
	public virtual void LoadFromBuildingData(BuildingData data, bool forUpgrade = false, bool forConstruction = false)
	{
		if (data == null)
		{
			return;
		}
		this.tilesWide.Value = data.Size.X;
		this.tilesHigh.Value = data.Size.Y;
		this.humanDoor.X = data.HumanDoor.X;
		this.humanDoor.Y = data.HumanDoor.Y;
		this.animalDoor.Value = data.AnimalDoor.Location;
		if (data.MaxOccupants >= 0)
		{
			this.maxOccupants.Value = data.MaxOccupants;
		}
		this.hayCapacity.Value = data.HayCapacity;
		this.magical.Value = data.Builder == "Wizard";
		this.fadeWhenPlayerIsBehind.Value = data.FadeWhenBehind;
		foreach (KeyValuePair<string, string> pair in data.ModData)
		{
			this.modData[pair.Key] = pair.Value;
		}
		this.GetIndoors()?.InvalidateCachedMultiplayerMap(Game1.multiplayer.cachedMultiplayerMaps);
		if (!Game1.IsMasterGame)
		{
			return;
		}
		if (this.hasLoaded || forConstruction)
		{
			if (this.nonInstancedIndoorsName.Value == null)
			{
				string mapPath = data.IndoorMap;
				string mapType = typeof(GameLocation).ToString();
				if (data.IndoorMapType != null)
				{
					mapType = data.IndoorMapType;
				}
				if (mapPath != null)
				{
					mapPath = "Maps\\" + mapPath;
					if (this.indoors.Value == null)
					{
						this.indoors.Value = this.createIndoors(data, data.IndoorMap);
						this.InitializeIndoor(data, forConstruction, forUpgrade);
					}
					else if (this.indoors.Value.mapPath.Value == mapPath)
					{
						if (forUpgrade)
						{
							this.InitializeIndoor(data, forConstruction, forUpgrade: true);
						}
					}
					else
					{
						if (this.indoors.Value.GetType().ToString() != mapType)
						{
							this.load();
						}
						else
						{
							this.indoors.Value.mapPath.Value = mapPath;
							this.indoors.Value.updateMap();
						}
						this.updateInteriorWarps(this.indoors.Value);
						this.InitializeIndoor(data, forConstruction, forUpgrade);
					}
				}
			}
			else
			{
				this.updateInteriorWarps();
			}
		}
		if (!(this.hasLoaded || forConstruction))
		{
			return;
		}
		HashSet<string> validChests = new HashSet<string>();
		if (data.Chests != null)
		{
			foreach (BuildingChest buildingChest in data.Chests)
			{
				validChests.Add(buildingChest.Id);
			}
		}
		this.buildingChests.RemoveWhere((Chest chest) => !validChests.Contains(chest.Name));
		if (data.Chests == null)
		{
			return;
		}
		foreach (BuildingChest buildingChest2 in data.Chests)
		{
			if (this.GetBuildingChest(buildingChest2.Id) == null)
			{
				Chest newChest = new Chest(playerChest: true)
				{
					Name = buildingChest2.Id
				};
				this.buildingChests.Add(newChest);
			}
		}
	}

	/// <summary>Create a building instance from its type ID.</summary>
	/// <param name="typeId">The building type ID in <c>Data/Buildings</c>.</param>
	/// <param name="tile">The top-left tile position of the building.</param>
	public static Building CreateInstanceFromId(string typeId, Vector2 tile)
	{
		if (typeId != null && Game1.buildingData.TryGetValue(typeId, out var data))
		{
			Type type = ((data.BuildingType != null) ? Type.GetType(data.BuildingType) : null);
			if (type != null && type != typeof(Building))
			{
				try
				{
					return (Building)Activator.CreateInstance(type, typeId, tile);
				}
				catch (MissingMethodException)
				{
					try
					{
						Building obj = (Building)Activator.CreateInstance(type, tile);
						obj.buildingType.Value = typeId;
						return obj;
					}
					catch (Exception exception)
					{
						Game1.log.Error("Error trying to instantiate building for type '" + typeId + "'", exception);
					}
				}
			}
		}
		return new Building(typeId, tile);
	}

	public virtual void InitializeIndoor(BuildingData data, bool forConstruction, bool forUpgrade)
	{
		if (data == null)
		{
			return;
		}
		GameLocation interior = this.GetIndoors();
		if (interior == null)
		{
			return;
		}
		if (interior is AnimalHouse animalHouse && data.MaxOccupants > 0)
		{
			animalHouse.animalLimit.Value = data.MaxOccupants;
		}
		if (forUpgrade && data.IndoorItemMoves != null)
		{
			foreach (IndoorItemMove move in data.IndoorItemMoves)
			{
				for (int x = 0; x < move.Size.X; x++)
				{
					for (int y = 0; y < move.Size.Y; y++)
					{
						interior.moveContents(move.Source.X + x, move.Source.Y + y, move.Destination.X + x, move.Destination.Y + y, move.UnlessItemId);
					}
				}
			}
		}
		if (!(forConstruction || forUpgrade) || data.IndoorItems == null)
		{
			return;
		}
		foreach (IndoorItemAdd item in data.IndoorItems)
		{
			Vector2 tileVector = Utility.PointToVector2(item.Tile);
			Object newObj = ItemRegistry.Create(item.ItemId) as Object;
			Furniture newFurniture = newObj as Furniture;
			if (newObj == null)
			{
				continue;
			}
			if (item.ClearTile)
			{
				if (newFurniture != null)
				{
					int i = 0;
					for (int height = newFurniture.getTilesHigh(); i < height; i++)
					{
						int j = 0;
						for (int width = newFurniture.getTilesWide(); j < width; j++)
						{
							interior.cleanUpTileForMapOverride(new Point((int)tileVector.X + j, (int)tileVector.Y + i), item.ItemId);
						}
					}
				}
				else
				{
					interior.cleanUpTileForMapOverride(Utility.Vector2ToPoint(tileVector), item.ItemId);
				}
			}
			if (!interior.IsTileBlockedBy(tileVector, CollisionMask.Furniture | CollisionMask.Objects))
			{
				if (item.Indestructible)
				{
					newObj.fragility.Value = 2;
				}
				newObj.TileLocation = tileVector;
				if (newFurniture != null)
				{
					interior.furniture.Add(newFurniture);
				}
				else
				{
					interior.objects.Add(tileVector, newObj);
				}
			}
		}
	}

	public BuildingItemConversion GetItemConversionForItem(Item item, Chest chest)
	{
		if (item == null || chest == null)
		{
			return null;
		}
		BuildingData data = this.GetData();
		if (data?.ItemConversions != null)
		{
			foreach (BuildingItemConversion conversion in data.ItemConversions)
			{
				if (!(conversion.SourceChest == chest.Name))
				{
					continue;
				}
				bool fail = false;
				foreach (string requiredTag in conversion.RequiredTags)
				{
					if (!item.HasContextTag(requiredTag))
					{
						fail = true;
						break;
					}
				}
				if (!fail)
				{
					return conversion;
				}
			}
		}
		return null;
	}

	public bool IsValidObjectForChest(Item item, Chest chest)
	{
		return this.GetItemConversionForItem(item, chest) != null;
	}

	public bool PerformBuildingChestAction(string name, Farmer who)
	{
		Chest chest = this.GetBuildingChest(name);
		if (chest == null)
		{
			return false;
		}
		BuildingChest chestData = this.GetBuildingChestData(name);
		if (chestData == null)
		{
			return false;
		}
		switch (chestData.Type)
		{
		case BuildingChestType.Chest:
			((MenuWithInventory)(Game1.activeClickableMenu = new ItemGrabMenu(chest.Items, reverseGrab: false, showReceivingMenu: true, (Item item) => this.IsValidObjectForChest(item, chest), chest.grabItemFromInventory, null, chest.grabItemFromChest, snapToBottom: false, canBeExitedWithKey: true, playRightClickSound: true, allowRightClick: true, showOrganizeButton: true, 1, null, -1, this))).inventory.moveItemSound = chestData.Sound;
			return true;
		case BuildingChestType.Load:
			if (who?.ActiveObject != null)
			{
				if (!this.IsValidObjectForChest(who.ActiveObject, chest))
				{
					if (chestData.InvalidItemMessage != null && (chestData.InvalidItemMessageCondition == null || GameStateQuery.CheckConditions(chestData.InvalidItemMessageCondition, this.GetParentLocation(), who, who.ActiveObject, who.ActiveObject)))
					{
						Game1.showRedMessage(TokenParser.ParseText(chestData.InvalidItemMessage));
					}
					return false;
				}
				BuildingItemConversion conversion = this.GetItemConversionForItem(who.ActiveObject, chest);
				Utility.consolidateStacks(chest.Items);
				chest.clearNulls();
				int roomForItem = Utility.GetNumberOfItemThatCanBeAddedToThisInventoryList(who.ActiveObject, chest.Items, 36);
				if (who.ActiveObject.Stack > conversion.RequiredCount && roomForItem < conversion.RequiredCount)
				{
					Game1.showRedMessage(TokenParser.ParseText(chestData.ChestFullMessage));
					return false;
				}
				int acceptAmount = Math.Min(roomForItem, who.ActiveObject.Stack) / conversion.RequiredCount * conversion.RequiredCount;
				if (acceptAmount == 0)
				{
					if (chestData.InvalidCountMessage != null)
					{
						Game1.showRedMessage(TokenParser.ParseText(chestData.InvalidCountMessage));
					}
					return false;
				}
				Item one = who.ActiveObject.getOne();
				if (who.ActiveObject.ConsumeStack(acceptAmount) == null)
				{
					who.ActiveObject = null;
				}
				one.Stack = acceptAmount;
				Utility.addItemToThisInventoryList(one, chest.Items, 36);
				if (chestData.Sound != null)
				{
					Game1.playSound(chestData.Sound);
				}
			}
			return true;
		case BuildingChestType.Collect:
			Utility.CollectSingleItemOrShowChestMenu(chest);
			return true;
		default:
			return false;
		}
	}

	public BuildingChest GetBuildingChestData(string name)
	{
		return Building.GetBuildingChestData(this.GetData(), name);
	}

	public static BuildingChest GetBuildingChestData(BuildingData data, string name)
	{
		if (data == null)
		{
			return null;
		}
		foreach (BuildingChest buildingChestData in data.Chests)
		{
			if (buildingChestData.Id == name)
			{
				return buildingChestData;
			}
		}
		return null;
	}

	public Chest GetBuildingChest(string name)
	{
		foreach (Chest buildingChest in this.buildingChests)
		{
			if (buildingChest.Name == name)
			{
				return buildingChest;
			}
		}
		return null;
	}

	public virtual string textureName()
	{
		BuildingData data = this.GetData();
		return Building.GetSkin(this.skinId.Value, data)?.Texture ?? data?.Texture ?? ("Buildings\\" + this.buildingType.Value);
	}

	public virtual void resetTexture()
	{
		this.texture = new Lazy<Texture2D>(delegate
		{
			if (this.paintedTexture != null)
			{
				this.paintedTexture.Dispose();
				this.paintedTexture = null;
			}
			string text = this.textureName();
			Texture2D texture2D;
			try
			{
				texture2D = Game1.content.Load<Texture2D>(text);
			}
			catch
			{
				return Game1.content.Load<Texture2D>("Buildings\\Error");
			}
			this.paintedTexture = BuildingPainter.Apply(texture2D, text + "_PaintMask", this.netBuildingPaintColor.Value);
			if (this.paintedTexture != null)
			{
				texture2D = this.paintedTexture;
			}
			return texture2D;
		});
	}

	public int getTileSheetIndexForStructurePlacementTile(int x, int y)
	{
		if (x == this.humanDoor.X && y == this.humanDoor.Y)
		{
			return 2;
		}
		if (x == this.animalDoor.X && y == this.animalDoor.Y)
		{
			return 4;
		}
		return 0;
	}

	public virtual void performTenMinuteAction(int timeElapsed)
	{
	}

	public virtual void resetLocalState()
	{
		this.alpha = 1f;
		this.color = Color.White;
		this.isMoving = false;
	}

	public virtual bool CanLeftClick(int x, int y)
	{
		Microsoft.Xna.Framework.Rectangle r = new Microsoft.Xna.Framework.Rectangle(x, y, 1, 1);
		return this.intersects(r);
	}

	public virtual bool leftClicked()
	{
		return false;
	}

	public virtual void ToggleAnimalDoor(Farmer who)
	{
		BuildingData data = this.GetData();
		string sound = ((!this.animalDoorOpen.Value) ? data?.AnimalDoorCloseSound : data?.AnimalDoorOpenSound);
		if (sound != null)
		{
			who.currentLocation.playSound(sound);
		}
		this.animalDoorOpen.Value = !this.animalDoorOpen.Value;
	}

	public virtual bool OnUseHumanDoor(Farmer who)
	{
		return true;
	}

	public virtual bool doAction(Vector2 tileLocation, Farmer who)
	{
		if (who.isRidingHorse())
		{
			return false;
		}
		if (who.IsLocalPlayer && this.occupiesTile(tileLocation) && this.daysOfConstructionLeft.Value > 0)
		{
			Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Buildings:UnderConstruction"));
		}
		else
		{
			if (who.ActiveObject != null && who.ActiveObject.IsFloorPathItem() && who.currentLocation != null && !who.currentLocation.terrainFeatures.ContainsKey(tileLocation))
			{
				return false;
			}
			GameLocation interior = this.GetIndoors();
			if (who.IsLocalPlayer && tileLocation.X == (float)(this.humanDoor.X + this.tileX.Value) && tileLocation.Y == (float)(this.humanDoor.Y + this.tileY.Value) && interior != null)
			{
				if (who.mount != null)
				{
					Game1.showRedMessage(Game1.content.LoadString("Strings\\Buildings:DismountBeforeEntering"));
					return false;
				}
				if (who.team.demolishLock.IsLocked())
				{
					Game1.showRedMessage(Game1.content.LoadString("Strings\\Buildings:CantEnter"));
					return false;
				}
				if (this.OnUseHumanDoor(who))
				{
					who.currentLocation.playSound("doorClose", tileLocation);
					bool isStructure = this.indoors.Value != null;
					Game1.warpFarmer(interior.NameOrUniqueName, interior.warps[0].X, interior.warps[0].Y - 1, Game1.player.FacingDirection, isStructure);
				}
				return true;
			}
			BuildingData data = this.GetData();
			if (data != null)
			{
				Microsoft.Xna.Framework.Rectangle door = this.getRectForAnimalDoor(data);
				door.Width /= 64;
				door.Height /= 64;
				door.X /= 64;
				door.Y /= 64;
				if (this.daysOfConstructionLeft.Value <= 0 && door != Microsoft.Xna.Framework.Rectangle.Empty && door.Contains(Utility.Vector2ToPoint(tileLocation)) && Game1.didPlayerJustRightClick(ignoreNonMouseHeldInput: true))
				{
					this.ToggleAnimalDoor(who);
					return true;
				}
				if (who.IsLocalPlayer && this.occupiesTile(tileLocation, applyTilePropertyRadius: true) && !this.isTilePassable(tileLocation))
				{
					string tileAction = data.GetActionAtTile((int)tileLocation.X - this.tileX.Value, (int)tileLocation.Y - this.tileY.Value);
					if (tileAction != null)
					{
						tileAction = TokenParser.ParseText(tileAction);
						if (who.currentLocation.performAction(tileAction, who, new Location((int)tileLocation.X, (int)tileLocation.Y)))
						{
							return true;
						}
					}
				}
			}
			else if (who.IsLocalPlayer)
			{
				if (!this.isTilePassable(tileLocation) && Building.TryPerformObeliskWarp(this.buildingType.Value, who))
				{
					return true;
				}
				if (who.ActiveObject != null && !this.isTilePassable(tileLocation))
				{
					return this.performActiveObjectDropInAction(who, probe: false);
				}
			}
		}
		return false;
	}

	public static bool TryPerformObeliskWarp(string buildingType, Farmer who)
	{
		switch (buildingType)
		{
		case "Desert Obelisk":
			Building.PerformObeliskWarp("Desert", 35, 43, force_dismount: true, who);
			return true;
		case "Water Obelisk":
			Building.PerformObeliskWarp("Beach", 20, 4, force_dismount: false, who);
			return true;
		case "Earth Obelisk":
			Building.PerformObeliskWarp("Mountain", 31, 20, force_dismount: false, who);
			return true;
		case "Island Obelisk":
			Building.PerformObeliskWarp("IslandSouth", 11, 11, force_dismount: false, who);
			return true;
		default:
			return false;
		}
	}

	public static void PerformObeliskWarp(string destination, int warp_x, int warp_y, bool force_dismount, Farmer who)
	{
		if (force_dismount && who.isRidingHorse() && who.mount != null)
		{
			who.mount.checkAction(who, who.currentLocation);
			return;
		}
		for (int i = 0; i < 12; i++)
		{
			who.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(354, Game1.random.Next(25, 75), 6, 1, new Vector2(Game1.random.Next((int)who.Position.X - 256, (int)who.Position.X + 192), Game1.random.Next((int)who.Position.Y - 256, (int)who.Position.Y + 192)), flicker: false, Game1.random.NextBool()));
		}
		who.currentLocation.playSound("wand");
		Game1.displayFarmer = false;
		Game1.player.temporarilyInvincible = true;
		Game1.player.temporaryInvincibilityTimer = -2000;
		Game1.player.freezePause = 1000;
		Game1.flashAlpha = 1f;
		Microsoft.Xna.Framework.Rectangle playerBounds = who.GetBoundingBox();
		DelayedAction.fadeAfterDelay(delegate
		{
			Building.obeliskWarpForReal(destination, warp_x, warp_y, who);
		}, 1000);
		new Microsoft.Xna.Framework.Rectangle(playerBounds.X, playerBounds.Y, 64, 64).Inflate(192, 192);
		int j = 0;
		Point playerTile = who.TilePoint;
		for (int x = playerTile.X + 8; x >= playerTile.X - 8; x--)
		{
			who.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(6, new Vector2(x, playerTile.Y) * 64f, Color.White, 8, flipped: false, 50f)
			{
				layerDepth = 1f,
				delayBeforeAnimationStart = j * 25,
				motion = new Vector2(-0.25f, 0f)
			});
			j++;
		}
	}

	private static void obeliskWarpForReal(string destination, int warp_x, int warp_y, Farmer who)
	{
		Game1.warpFarmer(destination, warp_x, warp_y, flip: false);
		Game1.fadeToBlackAlpha = 0.99f;
		Game1.screenGlow = false;
		Game1.player.temporarilyInvincible = false;
		Game1.player.temporaryInvincibilityTimer = 0;
		Game1.displayFarmer = true;
	}

	public virtual bool isActionableTile(int xTile, int yTile, Farmer who)
	{
		BuildingData data = this.GetData();
		if (data != null)
		{
			Vector2 tileLocation = new Vector2(xTile, yTile);
			if (this.occupiesTile(tileLocation, applyTilePropertyRadius: true) && !this.isTilePassable(tileLocation) && data.GetActionAtTile(xTile - this.tileX.Value, yTile - this.tileY.Value) != null)
			{
				return true;
			}
		}
		if (this.humanDoor.X >= 0 && xTile == this.tileX.Value + this.humanDoor.X && yTile == this.tileY.Value + this.humanDoor.Y)
		{
			return true;
		}
		Microsoft.Xna.Framework.Rectangle door = this.getRectForAnimalDoor(data);
		door.Width /= 64;
		door.Height /= 64;
		door.X /= 64;
		door.Y /= 64;
		if (door != Microsoft.Xna.Framework.Rectangle.Empty)
		{
			return door.Contains(new Point(xTile, yTile));
		}
		return false;
	}

	/// <summary>Handle the building being moved within its location by any player.</summary>
	public virtual void performActionOnBuildingPlacement()
	{
		GameLocation location = this.GetParentLocation();
		if (location == null)
		{
			return;
		}
		for (int y = 0; y < this.tilesHigh.Value; y++)
		{
			for (int x = 0; x < this.tilesWide.Value; x++)
			{
				Vector2 currentGlobalTilePosition = new Vector2(this.tileX.Value + x, this.tileY.Value + y);
				if (!(location.terrainFeatures.GetValueOrDefault(currentGlobalTilePosition) is Flooring) || !(this.GetData()?.AllowsFlooringUnderneath ?? false))
				{
					location.terrainFeatures.Remove(currentGlobalTilePosition);
				}
			}
		}
		foreach (BuildingPlacementTile additionalPlacementTile in this.GetAdditionalPlacementTiles())
		{
			bool onlyNeedsToBePassable = additionalPlacementTile.OnlyNeedsToBePassable;
			foreach (Point areaTile in additionalPlacementTile.TileArea.GetPoints())
			{
				Vector2 currentGlobalTilePosition2 = new Vector2(this.tileX.Value + areaTile.X, this.tileY.Value + areaTile.Y);
				if ((!onlyNeedsToBePassable || (location.terrainFeatures.TryGetValue(currentGlobalTilePosition2, out var feature) && !feature.isPassable())) && (!(location.terrainFeatures.GetValueOrDefault(currentGlobalTilePosition2) is Flooring) || !(this.GetData()?.AllowsFlooringUnderneath ?? false)))
				{
					location.terrainFeatures.Remove(currentGlobalTilePosition2);
				}
			}
		}
	}

	/// <summary>Handle the building being constructed.</summary>
	/// <param name="location">The location containing the building.</param>
	/// <param name="who">The player that constructed the building.</param>
	public virtual void performActionOnConstruction(GameLocation location, Farmer who)
	{
		BuildingData data = this.GetData();
		this.LoadFromBuildingData(data, forUpgrade: false, forConstruction: true);
		Vector2 buildingCenter = new Vector2((float)this.tileX.Value + (float)this.tilesWide.Value * 0.5f, (float)this.tileY.Value + (float)this.tilesHigh.Value * 0.5f);
		location.localSound("axchop", buildingCenter);
		this.newConstructionTimer.Value = ((this.magical.Value || this.daysOfConstructionLeft.Value <= 0) ? 2000 : 1000);
		if (data?.AddMailOnBuild != null)
		{
			foreach (string item in data.AddMailOnBuild)
			{
				Game1.addMail(item, noLetter: false, sendToEveryone: true);
			}
		}
		if (!this.magical.Value)
		{
			location.localSound("axchop", buildingCenter);
			for (int x = this.tileX.Value; x < this.tileX.Value + this.tilesWide.Value; x++)
			{
				for (int y = this.tileY.Value; y < this.tileY.Value + this.tilesHigh.Value; y++)
				{
					for (int i = 0; i < 5; i++)
					{
						location.temporarySprites.Add(new TemporaryAnimatedSprite(Game1.random.Choose(46, 12), new Vector2(x, y) * 64f + new Vector2(Game1.random.Next(-16, 32), Game1.random.Next(-16, 32)), Color.White, 10, Game1.random.NextBool())
						{
							delayBeforeAnimationStart = Math.Max(0, Game1.random.Next(-200, 400)),
							motion = new Vector2(0f, -1f),
							interval = Game1.random.Next(50, 80)
						});
					}
					location.temporarySprites.Add(new TemporaryAnimatedSprite(14, new Vector2(x, y) * 64f + new Vector2(Game1.random.Next(-16, 32), Game1.random.Next(-16, 32)), Color.White, 10, Game1.random.NextBool()));
				}
			}
			for (int j = 0; j < 8; j++)
			{
				DelayedAction.playSoundAfterDelay("dirtyHit", 250 + j * 150, location, buildingCenter, -1, local: true);
			}
		}
		else
		{
			for (int k = 0; k < 8; k++)
			{
				DelayedAction.playSoundAfterDelay("dirtyHit", 100 + k * 210, location, buildingCenter, -1, local: true);
			}
			if (Game1.player == who)
			{
				Game1.flashAlpha = 2f;
			}
			location.localSound("wand", buildingCenter);
			Microsoft.Xna.Framework.Rectangle mainSourceRect = this.getSourceRect();
			Microsoft.Xna.Framework.Rectangle sourceRectForMenu = this.getSourceRectForMenu() ?? mainSourceRect;
			int l = 0;
			for (int bottomEdge = mainSourceRect.Height / 16 * 2; l <= bottomEdge; l++)
			{
				int m = 0;
				for (int rightEdge = sourceRectForMenu.Width / 16 * 2; m < rightEdge; m++)
				{
					location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(666, 1851, 8, 8), 40f, 4, 2, new Vector2(this.tileX.Value, this.tileY.Value) * 64f + new Vector2(m * 64 / 2, l * 64 / 2 - mainSourceRect.Height * 4 + this.tilesHigh.Value * 64) + new Vector2(Game1.random.Next(-32, 32), Game1.random.Next(-32, 32)), flicker: false, flipped: false)
					{
						layerDepth = (float)((this.tileY.Value + this.tilesHigh.Value) * 64) / 10000f + (float)m / 10000f,
						pingPong = true,
						delayBeforeAnimationStart = (mainSourceRect.Height / 16 * 2 - l) * 100,
						scale = 4f,
						alphaFade = 0.01f,
						color = Color.AliceBlue
					});
					location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(666, 1851, 8, 8), 40f, 4, 2, new Vector2(this.tileX.Value, this.tileY.Value) * 64f + new Vector2(m * 64 / 2, l * 64 / 2 - mainSourceRect.Height * 4 + this.tilesHigh.Value * 64) + new Vector2(Game1.random.Next(-32, 32), Game1.random.Next(-32, 32)), flicker: false, flipped: false)
					{
						layerDepth = (float)((this.tileY.Value + this.tilesHigh.Value) * 64) / 10000f + (float)m / 10000f + 0.0001f,
						pingPong = true,
						delayBeforeAnimationStart = (mainSourceRect.Height / 16 * 2 - l) * 100,
						scale = 4f,
						alphaFade = 0.01f,
						color = Color.AliceBlue
					});
				}
			}
		}
		if (this.GetIndoors() is Cabin { HasOwner: false } cabin)
		{
			cabin.CreateFarmhand();
			if (Game1.IsMasterGame)
			{
				this.hasLoaded = true;
			}
		}
	}

	/// <summary>Handle the building being demolished.</summary>
	/// <param name="location">The location which previously contained the building.</param>
	public virtual void performActionOnDemolition(GameLocation location)
	{
		if (this.GetIndoors() is Cabin cabin)
		{
			cabin.DeleteFarmhand();
		}
		if (this.indoors.Value != null)
		{
			Game1.multiplayer.broadcastRemoveLocationFromLookup(this.indoors.Value);
			this.indoors.Value.OnRemoved();
			this.indoors.Value = null;
		}
	}

	/// <summary>Perform an action for each item within the building instance, excluding those in the interior location.</summary>
	/// <param name="action">The action to perform for each item.  This should return true (continue iterating) or false (stop).</param>
	/// <returns>Returns whether to continue iterating.</returns>
	/// <remarks>For items in the interior location, use <see cref="M:StardewValley.Utility.ForEachItemIn(StardewValley.GameLocation,System.Func{StardewValley.Item,System.Boolean})" /> instead. See also <see cref="M:StardewValley.Buildings.Building.ForEachItemContextExcludingInterior(StardewValley.Delegates.ForEachItemDelegate,StardewValley.Delegates.GetForEachItemPathDelegate)" /> for more advanced scenarios like replacing items.</remarks>
	public virtual bool ForEachItemExcludingInterior(Func<Item, bool> action)
	{
		return this.ForEachItemContextExcludingInterior(Handle, GetParentPath);
		IList<object> GetParentPath()
		{
			return new List<object> { this.GetParentLocation() };
		}
		bool Handle(in ForEachItemContext context)
		{
			return action(context.Item);
		}
	}

	/// <summary>Perform an action for each item within the building instance, excluding those in the interior location.</summary>
	/// <param name="handler">The action to perform for each item.</param>
	/// <param name="getParentPath">The contextual path leading to this building (excluding the building itself), or <c>null</c> to treat this as the root.</param>
	/// <returns>Returns whether to continue iterating.</returns>
	/// <remarks>For items in the interior location, use <see cref="M:StardewValley.Utility.ForEachItemIn(StardewValley.GameLocation,System.Func{StardewValley.Item,System.Boolean})" /> instead. See also <see cref="M:StardewValley.Buildings.Building.ForEachItemExcludingInterior(System.Func{StardewValley.Item,System.Boolean})" /> if you only need to iterate items.</remarks>
	public virtual bool ForEachItemContextExcludingInterior(ForEachItemDelegate handler, GetForEachItemPathDelegate getParentPath)
	{
		foreach (Chest buildingChest in this.buildingChests)
		{
			Chest chest = buildingChest;
			if (!chest.ForEachItem(handler, GetPath))
			{
				return false;
			}
			IList<object> GetPath()
			{
				return ForEachItemHelper.CombinePath(getParentPath, this, this.buildingChests, chest);
			}
		}
		return true;
	}

	public virtual void BeforeDemolish()
	{
		List<Item> quest_items = new List<Item>();
		this.ForEachItemExcludingInterior(delegate(Item item)
		{
			CollectQuestItem(item);
			return true;
		});
		if (this.indoors.Value != null)
		{
			Utility.ForEachItemIn(this.indoors.Value, delegate(Item item)
			{
				CollectQuestItem(item);
				return true;
			});
			if (this.indoors.Value is Cabin cabin)
			{
				Cellar cellar = cabin.GetCellar();
				if (cellar != null)
				{
					Utility.ForEachItemIn(cellar, delegate(Item item)
					{
						CollectQuestItem(item);
						return true;
					});
				}
			}
		}
		if (quest_items.Count > 0)
		{
			Game1.showGlobalMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:NewLostAndFoundItems"));
			for (int i = 0; i < quest_items.Count; i++)
			{
				Game1.player.team.returnedDonations.Add(quest_items[i]);
			}
		}
		void CollectQuestItem(Item item)
		{
			if (item is Object obj && obj.questItem.Value)
			{
				Item clone = obj.getOne();
				clone.Stack = obj.Stack;
				quest_items.Add(clone);
			}
		}
	}

	public virtual void performActionOnUpgrade(GameLocation location)
	{
		if (location is Farm farm)
		{
			farm.UnsetFarmhouseValues();
		}
	}

	public virtual string isThereAnythingtoPreventConstruction(GameLocation location, Vector2 tile_location)
	{
		return null;
	}

	public virtual bool performActiveObjectDropInAction(Farmer who, bool probe)
	{
		return false;
	}

	public virtual void performToolAction(Tool t, int tileX, int tileY)
	{
	}

	public virtual void updateWhenFarmNotCurrentLocation(GameTime time)
	{
		if (this.indoors.Value != null && Game1.currentLocation != this.indoors.Value)
		{
			this.indoors.Value.netAudio.Update();
		}
		this.netBuildingPaintColor.Value?.Poll(resetTexture);
		if (this.newConstructionTimer.Value > 0)
		{
			this.newConstructionTimer.Value -= time.ElapsedGameTime.Milliseconds;
			if (this.newConstructionTimer.Value <= 0 && this.magical.Value)
			{
				this.daysOfConstructionLeft.Value = 0;
			}
		}
		if (!Game1.IsMasterGame)
		{
			return;
		}
		BuildingData data = this.GetData();
		if (data == null)
		{
			return;
		}
		if (this.animalDoorOpen.Value)
		{
			if (this.animalDoorOpenAmount.Value < 1f)
			{
				this.animalDoorOpenAmount.Value = ((data.AnimalDoorOpenDuration > 0f) ? Utility.MoveTowards(this.animalDoorOpenAmount.Value, 1f, (float)time.ElapsedGameTime.TotalSeconds / data.AnimalDoorOpenDuration) : 1f);
			}
		}
		else if (this.animalDoorOpenAmount.Value > 0f)
		{
			this.animalDoorOpenAmount.Value = ((data.AnimalDoorCloseDuration > 0f) ? Utility.MoveTowards(this.animalDoorOpenAmount.Value, 0f, (float)time.ElapsedGameTime.TotalSeconds / data.AnimalDoorCloseDuration) : 0f);
		}
	}

	public virtual void Update(GameTime time)
	{
		if (!this.hasLoaded && Game1.IsMasterGame && Game1.hasLoadedGame)
		{
			this.ReloadBuildingData(forUpgrade: false, forConstruction: true);
			this.load();
		}
		this.UpdateTransparency();
		if (this.isUnderConstruction())
		{
			return;
		}
		if (!this.hasChimney.HasValue)
		{
			string chimneyString = this.GetMetadata("ChimneyPosition");
			if (chimneyString != null)
			{
				this.hasChimney = true;
				string[] split = ArgUtility.SplitBySpace(chimneyString);
				this.chimneyPosition.X = int.Parse(split[0]);
				this.chimneyPosition.Y = int.Parse(split[1]);
			}
			else
			{
				this.hasChimney = false;
			}
		}
		GameLocation interior = this.GetIndoors();
		if (interior is FarmHouse { upgradeLevel: var upgradeLevel } && this.lastHouseUpgradeLevel != upgradeLevel)
		{
			this.lastHouseUpgradeLevel = upgradeLevel;
			string chimneyString2 = null;
			for (int i = 1; i <= this.lastHouseUpgradeLevel; i++)
			{
				string currentChimneyString = this.GetMetadata("ChimneyPosition" + (i + 1));
				if (currentChimneyString != null)
				{
					chimneyString2 = currentChimneyString;
				}
			}
			if (chimneyString2 != null)
			{
				this.hasChimney = true;
				string[] split2 = ArgUtility.SplitBySpace(chimneyString2);
				this.chimneyPosition.X = int.Parse(split2[0]);
				this.chimneyPosition.Y = int.Parse(split2[1]);
			}
		}
		if (this.hasChimney != true || interior == null)
		{
			return;
		}
		this.chimneyTimer -= time.ElapsedGameTime.Milliseconds;
		if (this.chimneyTimer <= 0)
		{
			if (interior.hasActiveFireplace())
			{
				GameLocation parentLocation = this.GetParentLocation();
				Microsoft.Xna.Framework.Rectangle mainSourceRect = this.getSourceRect();
				Vector2 cornerPosition = new Vector2(this.tileX.Value * 64, this.tileY.Value * 64 + this.tilesHigh.Value * 64 - mainSourceRect.Height * 4);
				BuildingData data = this.GetData();
				Vector2 cornerOffset = ((data != null) ? (data.DrawOffset * 4f) : Vector2.Zero);
				TemporaryAnimatedSprite sprite = TemporaryAnimatedSprite.GetTemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(372, 1956, 10, 10), new Vector2(cornerPosition.X + cornerOffset.X, cornerPosition.Y + cornerOffset.Y) + this.chimneyPosition * 4f + new Vector2(-8f, -12f), flipped: false, 0.002f, Color.Gray);
				sprite.alpha = 0.75f;
				sprite.motion = new Vector2(0f, -0.5f);
				sprite.acceleration = new Vector2(0.002f, 0f);
				sprite.interval = 99999f;
				sprite.layerDepth = 1f;
				sprite.scale = 2f;
				sprite.scaleChange = 0.02f;
				sprite.rotationChange = (float)Game1.random.Next(-5, 6) * (float)Math.PI / 256f;
				parentLocation.temporarySprites.Add(sprite);
			}
			this.chimneyTimer = 500;
		}
	}

	/// <summary>Update the building transparency on tick for the local player's position.</summary>
	public virtual void UpdateTransparency()
	{
		if (this.fadeWhenPlayerIsBehind.Value)
		{
			Microsoft.Xna.Framework.Rectangle sourceRect = this.getSourceRectForMenu() ?? this.getSourceRect();
			Microsoft.Xna.Framework.Rectangle boundingBox = new Microsoft.Xna.Framework.Rectangle(this.tileX.Value * 64, (this.tileY.Value + (-(sourceRect.Height / 16) + this.tilesHigh.Value)) * 64, this.tilesWide.Value * 64, (sourceRect.Height / 16 - this.tilesHigh.Value) * 64 + 32);
			if (Game1.player.GetBoundingBox().Intersects(boundingBox))
			{
				if (this.alpha > 0.4f)
				{
					this.alpha = Math.Max(0.4f, this.alpha - 0.04f);
				}
				return;
			}
		}
		if (this.alpha < 1f)
		{
			this.alpha = Math.Min(1f, this.alpha + 0.05f);
		}
	}

	public virtual void showUpgradeAnimation(GameLocation location)
	{
		this.color = Color.White;
		location.temporarySprites.Add(new TemporaryAnimatedSprite(46, this.getUpgradeSignLocation() + new Vector2(Game1.random.Next(-16, 16), Game1.random.Next(-16, 16)), Color.Beige, 10, Game1.random.NextBool(), 75f)
		{
			motion = new Vector2(0f, -0.5f),
			acceleration = new Vector2(-0.02f, 0.01f),
			delayBeforeAnimationStart = Game1.random.Next(100),
			layerDepth = 0.89f
		});
		location.temporarySprites.Add(new TemporaryAnimatedSprite(46, this.getUpgradeSignLocation() + new Vector2(Game1.random.Next(-16, 16), Game1.random.Next(-16, 16)), Color.Beige, 10, Game1.random.NextBool(), 75f)
		{
			motion = new Vector2(0f, -0.5f),
			acceleration = new Vector2(-0.02f, 0.01f),
			delayBeforeAnimationStart = Game1.random.Next(40),
			layerDepth = 0.89f
		});
	}

	public virtual Vector2 getUpgradeSignLocation()
	{
		BuildingData data = this.GetData();
		Vector2 signOffset = data?.UpgradeSignTile ?? new Vector2(0.5f, 0f);
		float signHeight = data?.UpgradeSignHeight ?? 8f;
		return new Vector2(((float)this.tileX.Value + signOffset.X) * 64f, ((float)this.tileY.Value + signOffset.Y) * 64f - signHeight * 4f);
	}

	public virtual void showDestroyedAnimation(GameLocation location)
	{
		for (int x = this.tileX.Value; x < this.tileX.Value + this.tilesWide.Value; x++)
		{
			for (int y = this.tileY.Value; y < this.tileY.Value + this.tilesHigh.Value; y++)
			{
				location.temporarySprites.Add(new TemporaryAnimatedSprite(362, Game1.random.Next(30, 90), 6, 1, new Vector2(x * 64, y * 64) + new Vector2(Game1.random.Next(-16, 16), Game1.random.Next(-16, 16)), flicker: false, Game1.random.NextBool())
				{
					delayBeforeAnimationStart = Game1.random.Next(300)
				});
				location.temporarySprites.Add(new TemporaryAnimatedSprite(362, Game1.random.Next(30, 90), 6, 1, new Vector2(x * 64, y * 64) + new Vector2(Game1.random.Next(-16, 16), Game1.random.Next(-16, 16)), flicker: false, Game1.random.NextBool())
				{
					delayBeforeAnimationStart = 250 + Game1.random.Next(300)
				});
				location.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(276, 1985, 12, 11), new Vector2(x, y) * 64f + new Vector2(32f, -32f) + new Vector2(Game1.random.Next(-32, 32), Game1.random.Next(-16, 16)), flipped: false, 0f, Color.White)
				{
					interval = 30f,
					totalNumberOfLoops = 99999,
					animationLength = 4,
					scale = 4f,
					alphaFade = 0.01f
				});
			}
		}
	}

	/// <summary>Instantly finish constructing or upgrading the building, if applicable.</summary>
	public void FinishConstruction(bool onGameStart = false)
	{
		bool changed = false;
		if (this.daysOfConstructionLeft.Value > 0)
		{
			Game1.player.team.constructedBuildings.Add(this.buildingType.Value);
			if (this.buildingType.Value == "Slime Hutch")
			{
				Game1.player.mailReceived.Add("slimeHutchBuilt");
			}
			this.daysOfConstructionLeft.Value = 0;
			changed = true;
		}
		if (this.daysUntilUpgrade.Value > 0)
		{
			string nextUpgrade = this.upgradeName.Value ?? "Well";
			Game1.player.team.constructedBuildings.Add(nextUpgrade);
			this.buildingType.Value = nextUpgrade;
			this.ReloadBuildingData(forUpgrade: true);
			this.daysUntilUpgrade.Value = 0;
			this.OnUpgraded();
			changed = true;
		}
		if (changed)
		{
			Game1.netWorldState.Value.UpdateUnderConstruction();
			this.resetTexture();
		}
		if (onGameStart)
		{
			return;
		}
		foreach (Farmer allFarmer in Game1.getAllFarmers())
		{
			allFarmer.autoGenerateActiveDialogueEvent("structureBuilt_" + this.buildingType.Value);
		}
	}

	public virtual void dayUpdate(int dayOfMonth)
	{
		if (this.daysOfConstructionLeft.Value > 0 && !Utility.isFestivalDay(dayOfMonth, Game1.season) && (!Game1.isGreenRain || Game1.year > 1))
		{
			if (this.daysOfConstructionLeft.Value == 1)
			{
				this.FinishConstruction();
			}
			else
			{
				this.daysOfConstructionLeft.Value--;
			}
			return;
		}
		if (this.daysUntilUpgrade.Value > 0 && !Utility.isFestivalDay(dayOfMonth, Game1.season) && (!Game1.isGreenRain || Game1.year > 1))
		{
			if (this.daysUntilUpgrade.Value == 1)
			{
				this.FinishConstruction();
			}
			else
			{
				this.daysUntilUpgrade.Value--;
			}
		}
		GameLocation interior = this.GetIndoors();
		if (interior is AnimalHouse animalHouse)
		{
			this.currentOccupants.Value = animalHouse.animals.Length;
		}
		if (this.GetIndoorsType() == IndoorsType.Instanced)
		{
			interior?.DayUpdate(dayOfMonth);
		}
		BuildingData data = this.GetData();
		if (data == null || !(data.ItemConversions?.Count > 0))
		{
			return;
		}
		ItemQueryContext itemQueryContext = new ItemQueryContext(this.GetParentLocation(), null, null, "building '" + this.buildingType.Value + "' > item conversion rules");
		foreach (BuildingItemConversion conversion in data.ItemConversions)
		{
			this.CheckItemConversionRule(conversion, itemQueryContext);
		}
	}

	public virtual void CheckItemConversionRule(BuildingItemConversion conversion, ItemQueryContext itemQueryContext)
	{
		int convertAmount = 0;
		int currentCount = 0;
		Chest sourceChest = this.GetBuildingChest(conversion.SourceChest);
		Chest destinationChest = this.GetBuildingChest(conversion.DestinationChest);
		if (sourceChest == null)
		{
			return;
		}
		foreach (Item item in sourceChest.Items)
		{
			if (item == null)
			{
				continue;
			}
			bool fail = false;
			foreach (string requiredTag in conversion.RequiredTags)
			{
				if (!item.HasContextTag(requiredTag))
				{
					fail = true;
					break;
				}
			}
			if (fail)
			{
				continue;
			}
			currentCount += item.Stack;
			if (currentCount >= conversion.RequiredCount)
			{
				int conversions = currentCount / conversion.RequiredCount;
				if (conversion.MaxDailyConversions >= 0)
				{
					conversions = Math.Min(conversions, conversion.MaxDailyConversions - convertAmount);
				}
				convertAmount += conversions;
				currentCount -= conversions * conversion.RequiredCount;
			}
			if (conversion.MaxDailyConversions >= 0 && convertAmount >= conversion.MaxDailyConversions)
			{
				break;
			}
		}
		if (convertAmount == 0)
		{
			return;
		}
		int totalConversions = 0;
		for (int j = 0; j < convertAmount; j++)
		{
			bool conversionCreatedItem = false;
			for (int i = 0; i < conversion.ProducedItems.Count; i++)
			{
				GenericSpawnItemDataWithCondition producedItem = conversion.ProducedItems[i];
				if (GameStateQuery.CheckConditions(producedItem.Condition, this.GetParentLocation()))
				{
					Item item2 = ItemQueryResolver.TryResolveRandomItem(producedItem, itemQueryContext);
					int producedCount = item2.Stack;
					Item item3 = destinationChest.addItem(item2);
					if (item3 == null || item3.Stack != producedCount)
					{
						conversionCreatedItem = true;
					}
				}
			}
			if (conversionCreatedItem)
			{
				totalConversions++;
			}
		}
		if (totalConversions <= 0)
		{
			return;
		}
		int requiredAmount = totalConversions * conversion.RequiredCount;
		for (int k = 0; k < sourceChest.Items.Count; k++)
		{
			Item item4 = sourceChest.Items[k];
			if (item4 == null)
			{
				continue;
			}
			bool fail2 = false;
			foreach (string requiredTag2 in conversion.RequiredTags)
			{
				if (!item4.HasContextTag(requiredTag2))
				{
					fail2 = true;
					break;
				}
			}
			if (!fail2)
			{
				int consumedAmount = Math.Min(requiredAmount, item4.Stack);
				sourceChest.Items[k] = item4.ConsumeStack(consumedAmount);
				requiredAmount -= consumedAmount;
				if (requiredAmount <= 0)
				{
					break;
				}
			}
		}
	}

	public virtual void OnUpgraded()
	{
		this.GetIndoors()?.OnParentBuildingUpgraded(this);
		BuildingData data = this.GetData();
		if (data?.AddMailOnBuild == null)
		{
			return;
		}
		foreach (string item in data.AddMailOnBuild)
		{
			Game1.addMail(item, noLetter: false, sendToEveryone: true);
		}
	}

	public virtual Microsoft.Xna.Framework.Rectangle getSourceRect()
	{
		BuildingData data = this.GetData();
		if (data != null)
		{
			Microsoft.Xna.Framework.Rectangle rect = data.SourceRect;
			if (rect == Microsoft.Xna.Framework.Rectangle.Empty)
			{
				return this.texture.Value.Bounds;
			}
			GameLocation interior = this.GetIndoors();
			if (interior is FarmHouse farmhouse)
			{
				if (interior is Cabin)
				{
					rect.X += rect.Width * Math.Min(farmhouse.upgradeLevel, 2);
				}
				else
				{
					rect.Y += rect.Height * Math.Min(farmhouse.upgradeLevel, 2);
				}
			}
			rect = this.ApplySourceRectOffsets(rect);
			if (this.buildingType.Value == "Greenhouse" && this.GetParentLocation() is Farm farm && !farm.greenhouseUnlocked.Value)
			{
				rect.Y -= rect.Height;
			}
			return rect;
		}
		if (this.isCabin)
		{
			return new Microsoft.Xna.Framework.Rectangle(((this.GetIndoors() is Cabin cabin) ? Math.Min(cabin.upgradeLevel, 2) : 0) * 80, 0, 80, 112);
		}
		return this.texture.Value.Bounds;
	}

	public virtual Microsoft.Xna.Framework.Rectangle ApplySourceRectOffsets(Microsoft.Xna.Framework.Rectangle source)
	{
		BuildingData data = this.GetData();
		if (data != null && data.SeasonOffset != Point.Zero)
		{
			int seasonOffset = Game1.GetSeasonIndexForLocation(this.GetParentLocation());
			source.X += data.SeasonOffset.X * seasonOffset;
			source.Y += data.SeasonOffset.Y * seasonOffset;
		}
		return source;
	}

	public virtual Microsoft.Xna.Framework.Rectangle? getSourceRectForMenu()
	{
		return null;
	}

	public virtual void updateInteriorWarps(GameLocation interior = null)
	{
		interior = interior ?? this.GetIndoors();
		if (interior == null)
		{
			return;
		}
		GameLocation parentLocation = this.GetParentLocation();
		foreach (Warp warp in interior.warps)
		{
			if (warp.TargetName == "Farm" || (parentLocation != null && warp.TargetName == parentLocation.NameOrUniqueName))
			{
				warp.TargetName = parentLocation?.NameOrUniqueName ?? warp.TargetName;
				warp.TargetX = this.humanDoor.X + this.tileX.Value;
				warp.TargetY = this.humanDoor.Y + this.tileY.Value + 1;
			}
		}
	}

	/// <summary>Get whether the building has an interior location.</summary>
	public bool HasIndoors()
	{
		if (this.indoors.Value == null)
		{
			return this.nonInstancedIndoorsName.Value != null;
		}
		return true;
	}

	/// <summary>Get whether the building has an interior location with the given unique name.</summary>
	/// <param name="name">The name to check.</param>
	public bool HasIndoorsName(string name)
	{
		if (name != null)
		{
			return this.GetIndoorsName().EqualsIgnoreCase(name);
		}
		return false;
	}

	/// <summary>Get the unique name of the location within this building, if it's linked to an instanced or non-instanced interior.</summary>
	public string GetIndoorsName()
	{
		return this.indoors.Value?.NameOrUniqueName ?? this.nonInstancedIndoorsName.Value;
	}

	/// <summary>Get the type of indoors location this building has.</summary>
	public IndoorsType GetIndoorsType()
	{
		if (this.indoors.Value != null)
		{
			return IndoorsType.Instanced;
		}
		if (this.nonInstancedIndoorsName.Value != null)
		{
			return IndoorsType.Global;
		}
		return IndoorsType.None;
	}

	/// <summary>Get the location within this building, if it's linked to an instanced or non-instanced interior.</summary>
	public GameLocation GetIndoors()
	{
		if (this.indoors.Value != null)
		{
			return this.indoors.Value;
		}
		if (this.nonInstancedIndoorsName.Value != null)
		{
			return Game1.getLocationFromName(this.nonInstancedIndoorsName.Value);
		}
		return null;
	}

	protected virtual GameLocation createIndoors(BuildingData data, string nameOfIndoorsWithoutUnique)
	{
		GameLocation localIndoors = null;
		if (data != null && !string.IsNullOrEmpty(data.IndoorMap))
		{
			Type locationType = typeof(GameLocation);
			if (data.IndoorMapType != null)
			{
				Exception exception = null;
				try
				{
					locationType = Type.GetType(data.IndoorMapType);
				}
				catch (Exception ex)
				{
					exception = ex;
				}
				if ((object)locationType == null || exception != null)
				{
					Game1.log.Error($"Error constructing interior type '{data.IndoorMapType}' for building '{this.buildingType.Value}'" + ((exception != null) ? "." : ": that type doesn't exist."));
					locationType = typeof(GameLocation);
				}
			}
			string mapAssetName = "Maps\\" + data.IndoorMap;
			try
			{
				localIndoors = (GameLocation)Activator.CreateInstance(locationType, mapAssetName, this.buildingType.Value);
			}
			catch (Exception)
			{
				try
				{
					localIndoors = (GameLocation)Activator.CreateInstance(locationType, mapAssetName);
				}
				catch (Exception exception2)
				{
					Game1.log.Error($"Error trying to instantiate indoors for '{this.buildingType}'", exception2);
					localIndoors = new GameLocation("Maps\\" + nameOfIndoorsWithoutUnique, this.buildingType.Value);
				}
			}
		}
		if (localIndoors != null)
		{
			localIndoors.uniqueName.Value = nameOfIndoorsWithoutUnique + GuidHelper.NewGuid();
			localIndoors.IsFarm = true;
			localIndoors.isStructure.Value = true;
			localIndoors.ParentBuilding = this;
			this.updateInteriorWarps(localIndoors);
		}
		return localIndoors;
	}

	public virtual Point getPointForHumanDoor()
	{
		return new Point(this.tileX.Value + this.humanDoor.Value.X, this.tileY.Value + this.humanDoor.Value.Y);
	}

	public virtual Microsoft.Xna.Framework.Rectangle getRectForHumanDoor()
	{
		return new Microsoft.Xna.Framework.Rectangle(this.getPointForHumanDoor().X * 64, this.getPointForHumanDoor().Y * 64, 64, 64);
	}

	public Microsoft.Xna.Framework.Rectangle getRectForAnimalDoor()
	{
		return this.getRectForAnimalDoor(this.GetData());
	}

	public virtual Microsoft.Xna.Framework.Rectangle getRectForAnimalDoor(BuildingData data)
	{
		if (data != null)
		{
			Microsoft.Xna.Framework.Rectangle rect = data.AnimalDoor;
			return new Microsoft.Xna.Framework.Rectangle((rect.X + this.tileX.Value) * 64, (rect.Y + this.tileY.Value) * 64, rect.Width * 64, rect.Height * 64);
		}
		return new Microsoft.Xna.Framework.Rectangle((this.animalDoor.X + this.tileX.Value) * 64, (this.tileY.Value + this.animalDoor.Y) * 64, 64, 64);
	}

	public virtual void load()
	{
		if (!Game1.IsMasterGame)
		{
			return;
		}
		BuildingData data = this.GetData();
		if (!this.hasLoaded)
		{
			this.hasLoaded = true;
			if (data != null)
			{
				if (data.NonInstancedIndoorLocation == null && this.nonInstancedIndoorsName.Value != null)
				{
					GameLocation interior = this.GetIndoors();
					if (interior != null)
					{
						interior.parentLocationName.Value = null;
					}
					this.nonInstancedIndoorsName.Value = null;
				}
				else if (data.NonInstancedIndoorLocation != null)
				{
					bool nonInstancedLocationAlreadyUsed = false;
					Utility.ForEachBuilding(delegate(Building building)
					{
						if (building.HasIndoorsName(data.NonInstancedIndoorLocation))
						{
							nonInstancedLocationAlreadyUsed = true;
							return false;
						}
						return true;
					});
					if (!nonInstancedLocationAlreadyUsed)
					{
						this.nonInstancedIndoorsName.Value = Game1.RequireLocation(data.NonInstancedIndoorLocation).NameOrUniqueName;
					}
				}
			}
			this.LoadFromBuildingData(data);
		}
		if (this.nonInstancedIndoorsName.Value != null)
		{
			this.UpdateIndoorParent();
		}
		else
		{
			string nameOfIndoorsWithoutUnique = data?.IndoorMap ?? this.indoors.Value?.Name;
			GameLocation indoorInstance = this.createIndoors(data, nameOfIndoorsWithoutUnique);
			if (indoorInstance != null && this.indoors.Value != null)
			{
				indoorInstance.characters.Set(this.indoors.Value.characters);
				indoorInstance.netObjects.MoveFrom(this.indoors.Value.netObjects);
				indoorInstance.terrainFeatures.MoveFrom(this.indoors.Value.terrainFeatures);
				indoorInstance.IsFarm = true;
				indoorInstance.IsOutdoors = false;
				indoorInstance.isStructure.Value = true;
				indoorInstance.miniJukeboxCount.Set(this.indoors.Value.miniJukeboxCount.Value);
				indoorInstance.miniJukeboxTrack.Set(this.indoors.Value.miniJukeboxTrack.Value);
				indoorInstance.uniqueName.Value = this.indoors.Value.uniqueName.Value ?? (nameOfIndoorsWithoutUnique + (this.tileX.Value * 2000 + this.tileY.Value));
				indoorInstance.numberOfSpawnedObjectsOnMap = this.indoors.Value.numberOfSpawnedObjectsOnMap;
				indoorInstance.animals.MoveFrom(this.indoors.Value.animals);
				if (this.indoors.Value is AnimalHouse house && indoorInstance is AnimalHouse houseInstance)
				{
					houseInstance.animalsThatLiveHere.Set(house.animalsThatLiveHere);
				}
				foreach (KeyValuePair<long, FarmAnimal> pair in indoorInstance.animals.Pairs)
				{
					pair.Value.reload(indoorInstance);
				}
				indoorInstance.furniture.Set(this.indoors.Value.furniture);
				foreach (Furniture item in indoorInstance.furniture)
				{
					item.updateDrawPosition();
				}
				if (this.indoors.Value is Cabin cabin && indoorInstance is Cabin cabinInstance)
				{
					cabinInstance.fridge.Value = cabin.fridge.Value;
					cabinInstance.farmhandReference.Value = cabin.farmhandReference.Value;
				}
				indoorInstance.TransferDataFromSavedLocation(this.indoors.Value);
				this.indoors.Value = indoorInstance;
			}
			this.updateInteriorWarps();
			if (this.indoors.Value != null)
			{
				for (int i = this.indoors.Value.characters.Count - 1; i >= 0; i--)
				{
					SaveGame.initializeCharacter(this.indoors.Value.characters[i], this.indoors.Value);
				}
				foreach (TerrainFeature value in this.indoors.Value.terrainFeatures.Values)
				{
					value.loadSprite();
				}
				foreach (KeyValuePair<Vector2, Object> v in this.indoors.Value.objects.Pairs)
				{
					v.Value.initializeLightSource(v.Key);
					v.Value.reloadSprite();
				}
			}
		}
		if (data != null)
		{
			this.humanDoor.X = data.HumanDoor.X;
			this.humanDoor.Y = data.HumanDoor.Y;
		}
	}

	/// <summary>Get the extra tiles to treat as part of the building when placing it through a construction menu, if any. For example, the farmhouse uses this to make sure the stairs are clear.</summary>
	public IEnumerable<BuildingPlacementTile> GetAdditionalPlacementTiles()
	{
		IEnumerable<BuildingPlacementTile> enumerable = this.GetData()?.AdditionalPlacementTiles;
		return enumerable ?? LegacyShims.EmptyArray<BuildingPlacementTile>();
	}

	public bool isUnderConstruction(bool ignoreUpgrades = true)
	{
		if (!ignoreUpgrades && this.daysUntilUpgrade.Value > 0)
		{
			return true;
		}
		return this.daysOfConstructionLeft.Value > 0;
	}

	/// <summary>Get whether the building's bounds covers a given tile coordinate.</summary>
	/// <param name="tile">The tile position to check.</param>
	/// <param name="applyTilePropertyRadius">Whether to check the extra tiles around the building itself for which it may add tile properties.</param>
	public bool occupiesTile(Vector2 tile, bool applyTilePropertyRadius = false)
	{
		return this.occupiesTile((int)tile.X, (int)tile.Y, applyTilePropertyRadius);
	}

	/// <summary>Get whether the building's bounds covers a given tile coordinate.</summary>
	/// <param name="x">The X tile position to check.</param>
	/// <param name="y">The Y tile position to check</param>
	/// <param name="applyTilePropertyRadius">Whether to check the extra tiles around the building itself for which it may add tile properties.</param>
	public virtual bool occupiesTile(int x, int y, bool applyTilePropertyRadius = false)
	{
		int additionalRadius = (applyTilePropertyRadius ? this.GetAdditionalTilePropertyRadius() : 0);
		int leftX = this.tileX.Value;
		int topY = this.tileY.Value;
		int width = this.tilesWide.Value;
		int height = this.tilesHigh.Value;
		if (x >= leftX - additionalRadius && x < leftX + width + additionalRadius && y >= topY - additionalRadius)
		{
			return y < topY + height + additionalRadius;
		}
		return false;
	}

	public virtual bool isTilePassable(Vector2 tile)
	{
		bool occupied = this.occupiesTile(tile);
		if (occupied && this.isUnderConstruction())
		{
			return false;
		}
		BuildingData data = this.GetData();
		if (data != null && this.occupiesTile(tile, applyTilePropertyRadius: true))
		{
			return data.IsTilePassable((int)tile.X - this.tileX.Value, (int)tile.Y - this.tileY.Value);
		}
		return !occupied;
	}

	public virtual bool isTileOccupiedForPlacement(Vector2 tile, Object to_place)
	{
		if (!this.isTilePassable(tile))
		{
			return true;
		}
		return false;
	}

	/// <summary>If this building is fishable, get the color of the water at the given tile position.</summary>
	/// <param name="tile">The tile position.</param>
	/// <returns>Returns the water color to use, or <c>null</c> to use the location's default water color.</returns>
	public virtual Color? GetWaterColor(Vector2 tile)
	{
		return null;
	}

	public virtual bool isTileFishable(Vector2 tile)
	{
		return false;
	}

	/// <summary>Whether watering cans can be refilled from any tile covered by this building.</summary>
	/// <remarks>If this is false, watering cans may still be refillable based on tile data (e.g. the <c>WaterSource</c> back tile property).</remarks>
	public virtual bool CanRefillWateringCan()
	{
		return false;
	}

	/// <summary>Create a pixel rectangle for the building's ground footprint within its location.</summary>
	public Microsoft.Xna.Framework.Rectangle GetBoundingBox()
	{
		return new Microsoft.Xna.Framework.Rectangle(this.tileX.Value * 64, this.tileY.Value * 64, this.tilesWide.Value * 64, this.tilesHigh.Value * 64);
	}

	public virtual bool intersects(Microsoft.Xna.Framework.Rectangle boundingBox)
	{
		Microsoft.Xna.Framework.Rectangle buildingRect = this.GetBoundingBox();
		int additionalRadius = this.GetAdditionalTilePropertyRadius();
		if (additionalRadius > 0)
		{
			buildingRect.Inflate(additionalRadius * 64, additionalRadius * 64);
		}
		if (buildingRect.Intersects(boundingBox))
		{
			int y = boundingBox.Top / 64;
			for (int maxY = boundingBox.Bottom / 64; y <= maxY; y++)
			{
				int x = boundingBox.Left / 64;
				for (int maxX = boundingBox.Right / 64; x <= maxX; x++)
				{
					if (!this.isTilePassable(new Vector2(x, y)))
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public virtual void drawInMenu(SpriteBatch b, int x, int y)
	{
		BuildingData data = this.GetData();
		if (data != null)
		{
			x += (int)(data.DrawOffset.X * 4f);
			y += (int)(data.DrawOffset.Y * 4f);
		}
		float baseSortY = this.tilesHigh.Value * 64;
		float sortY = baseSortY;
		if (data != null)
		{
			sortY -= data.SortTileOffset * 64f;
		}
		sortY /= 10000f;
		if (this.ShouldDrawShadow(data))
		{
			this.drawShadow(b, x, y);
		}
		Microsoft.Xna.Framework.Rectangle mainSourceRect = this.getSourceRect();
		b.Draw(this.texture.Value, new Vector2(x, y), mainSourceRect, this.color, 0f, new Vector2(0f, 0f), 4f, SpriteEffects.None, sortY);
		if (data?.DrawLayers == null)
		{
			return;
		}
		foreach (BuildingDrawLayer drawLayer in data.DrawLayers)
		{
			if (drawLayer.OnlyDrawIfChestHasContents == null)
			{
				sortY = baseSortY - drawLayer.SortTileOffset * 64f;
				sortY += 1f;
				if (drawLayer.DrawInBackground)
				{
					sortY = 0f;
				}
				sortY /= 10000f;
				Microsoft.Xna.Framework.Rectangle sourceRect = drawLayer.GetSourceRect((int)Game1.currentGameTime.TotalGameTime.TotalMilliseconds);
				sourceRect = this.ApplySourceRectOffsets(sourceRect);
				Texture2D layerTexture = this.texture.Value;
				if (drawLayer.Texture != null)
				{
					layerTexture = Game1.content.Load<Texture2D>(drawLayer.Texture);
				}
				b.Draw(layerTexture, new Vector2(x, y) + drawLayer.DrawPosition * 4f, sourceRect, Color.White, 0f, new Vector2(0f, 0f), 4f, SpriteEffects.None, sortY);
			}
		}
	}

	public virtual void drawBackground(SpriteBatch b)
	{
		if (this.isMoving || this.daysOfConstructionLeft.Value > 0 || this.newConstructionTimer.Value > 0)
		{
			return;
		}
		BuildingData data = this.GetData();
		if (data?.DrawLayers == null)
		{
			return;
		}
		Vector2 drawOrigin = new Vector2(0f, this.getSourceRect().Height);
		Vector2 drawPosition = new Vector2(this.tileX.Value * 64, this.tileY.Value * 64 + this.tilesHigh.Value * 64);
		foreach (BuildingDrawLayer drawLayer in data.DrawLayers)
		{
			if (!drawLayer.DrawInBackground)
			{
				continue;
			}
			if (drawLayer.OnlyDrawIfChestHasContents != null)
			{
				Chest chest = this.GetBuildingChest(drawLayer.OnlyDrawIfChestHasContents);
				if (chest == null || chest.isEmpty())
				{
					continue;
				}
			}
			Microsoft.Xna.Framework.Rectangle sourceRect = drawLayer.GetSourceRect((int)Game1.currentGameTime.TotalGameTime.TotalMilliseconds);
			sourceRect = this.ApplySourceRectOffsets(sourceRect);
			Vector2 drawOffset = Vector2.Zero;
			if (drawLayer.AnimalDoorOffset != Point.Zero)
			{
				drawOffset = new Vector2((float)drawLayer.AnimalDoorOffset.X * this.animalDoorOpenAmount.Value, (float)drawLayer.AnimalDoorOffset.Y * this.animalDoorOpenAmount.Value);
			}
			Texture2D layerTexture = this.texture.Value;
			if (drawLayer.Texture != null)
			{
				layerTexture = Game1.content.Load<Texture2D>(drawLayer.Texture);
			}
			b.Draw(layerTexture, Game1.GlobalToLocal(Game1.viewport, drawPosition + (drawOffset - drawOrigin + drawLayer.DrawPosition) * 4f), sourceRect, this.color * this.alpha, 0f, new Vector2(0f, 0f), 4f, SpriteEffects.None, 0f);
		}
	}

	public virtual void draw(SpriteBatch b)
	{
		if (this.isMoving)
		{
			return;
		}
		if (this.daysOfConstructionLeft.Value > 0 || this.newConstructionTimer.Value > 0)
		{
			this.drawInConstruction(b);
			return;
		}
		BuildingData data = this.GetData();
		if (this.ShouldDrawShadow(data))
		{
			this.drawShadow(b);
		}
		float baseSortY = (this.tileY.Value + this.tilesHigh.Value) * 64;
		float sortY = baseSortY;
		if (data != null)
		{
			sortY -= data.SortTileOffset * 64f;
		}
		sortY /= 10000f;
		Vector2 drawPosition = new Vector2(this.tileX.Value * 64, this.tileY.Value * 64 + this.tilesHigh.Value * 64);
		Vector2 drawOffset = Vector2.Zero;
		if (data != null)
		{
			drawOffset = data.DrawOffset * 4f;
		}
		Microsoft.Xna.Framework.Rectangle mainSourceRect = this.getSourceRect();
		Vector2 drawOrigin = new Vector2(0f, mainSourceRect.Height);
		b.Draw(this.texture.Value, Game1.GlobalToLocal(Game1.viewport, drawPosition + drawOffset), mainSourceRect, this.color * this.alpha, 0f, drawOrigin, 4f, SpriteEffects.None, sortY);
		if (this.magical.Value && this.buildingType.Value.Equals("Gold Clock"))
		{
			if (Game1.netWorldState.Value.goldenClocksTurnedOff.Value)
			{
				b.Draw(Game1.mouseCursors_1_6, Game1.GlobalToLocal(Game1.viewport, new Vector2(this.tileX.Value * 64 + 68, this.tileY.Value * 64 - 56)), new Microsoft.Xna.Framework.Rectangle(498, 368, 13, 9), Color.White * this.alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)((this.tileY.Value + this.tilesHigh.Value) * 64) / 10000f + 0.0001f);
			}
			else
			{
				b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(this.tileX.Value * 64 + 92, this.tileY.Value * 64 - 40)), Town.hourHandSource, Color.White * this.alpha, (float)(Math.PI * 2.0 * (double)((float)(Game1.timeOfDay % 1200) / 1200f) + (double)((float)Game1.gameTimeInterval / (float)Game1.realMilliSecondsPerGameTenMinutes / 23f)), new Vector2(2.5f, 8f), 3f, SpriteEffects.None, (float)((this.tileY.Value + this.tilesHigh.Value) * 64) / 10000f + 0.0001f);
				b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(this.tileX.Value * 64 + 92, this.tileY.Value * 64 - 40)), Town.minuteHandSource, Color.White * this.alpha, (float)(Math.PI * 2.0 * (double)((float)(Game1.timeOfDay % 1000 % 100 % 60) / 60f) + (double)((float)Game1.gameTimeInterval / (float)Game1.realMilliSecondsPerGameTenMinutes * 1.02f)), new Vector2(2.5f, 12f), 3f, SpriteEffects.None, (float)((this.tileY.Value + this.tilesHigh.Value) * 64) / 10000f + 0.00011f);
				b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(this.tileX.Value * 64 + 92, this.tileY.Value * 64 - 40)), Town.clockNub, Color.White * this.alpha, 0f, new Vector2(2f, 2f), 4f, SpriteEffects.None, (float)((this.tileY.Value + this.tilesHigh.Value) * 64) / 10000f + 0.00012f);
			}
		}
		if (data != null)
		{
			foreach (Chest chest in this.buildingChests)
			{
				BuildingChest chestData = Building.GetBuildingChestData(data, chest.Name);
				if (chestData.DisplayTile.X != -1f && chestData.DisplayTile.Y != -1f && chest.Items.Count > 0 && chest.Items[0] != null)
				{
					sortY = ((float)this.tileY.Value + chestData.DisplayTile.Y + 1f) * 64f;
					sortY += 1f;
					float yOffset = 4f * (float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 250.0), 2) - chestData.DisplayHeight * 64f;
					float drawX = ((float)this.tileX.Value + chestData.DisplayTile.X) * 64f;
					float drawY = ((float)this.tileY.Value + chestData.DisplayTile.Y - 1f) * 64f;
					b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(drawX, drawY + yOffset)), new Microsoft.Xna.Framework.Rectangle(141, 465, 20, 24), Color.White * 0.75f, 0f, Vector2.Zero, 4f, SpriteEffects.None, sortY / 10000f);
					ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(chest.Items[0].QualifiedItemId);
					b.Draw(itemData.GetTexture(), Game1.GlobalToLocal(Game1.viewport, new Vector2(drawX + 32f + 4f, drawY + 32f + yOffset)), itemData.GetSourceRect(), Color.White * 0.75f, 0f, new Vector2(8f, 8f), 4f, SpriteEffects.None, (sortY + 1f) / 10000f);
				}
			}
			if (data.DrawLayers != null)
			{
				foreach (BuildingDrawLayer drawLayer in data.DrawLayers)
				{
					if (drawLayer.DrawInBackground)
					{
						continue;
					}
					if (drawLayer.OnlyDrawIfChestHasContents != null)
					{
						Chest chest2 = this.GetBuildingChest(drawLayer.OnlyDrawIfChestHasContents);
						if (chest2 == null || chest2.isEmpty())
						{
							continue;
						}
					}
					sortY = baseSortY - drawLayer.SortTileOffset * 64f;
					sortY += 1f;
					sortY /= 10000f;
					Microsoft.Xna.Framework.Rectangle sourceRect = drawLayer.GetSourceRect((int)Game1.currentGameTime.TotalGameTime.TotalMilliseconds);
					sourceRect = this.ApplySourceRectOffsets(sourceRect);
					drawOffset = Vector2.Zero;
					if (drawLayer.AnimalDoorOffset != Point.Zero)
					{
						drawOffset = new Vector2((float)drawLayer.AnimalDoorOffset.X * this.animalDoorOpenAmount.Value, (float)drawLayer.AnimalDoorOffset.Y * this.animalDoorOpenAmount.Value);
					}
					Texture2D layerTexture = this.texture.Value;
					if (drawLayer.Texture != null)
					{
						layerTexture = Game1.content.Load<Texture2D>(drawLayer.Texture);
					}
					b.Draw(layerTexture, Game1.GlobalToLocal(Game1.viewport, drawPosition + (drawOffset - drawOrigin + drawLayer.DrawPosition) * 4f), sourceRect, this.color * this.alpha, 0f, new Vector2(0f, 0f), 4f, SpriteEffects.None, sortY);
				}
			}
		}
		if (this.daysUntilUpgrade.Value <= 0)
		{
			return;
		}
		if (data != null)
		{
			if (data.UpgradeSignTile.X >= 0f)
			{
				sortY = ((float)this.tileY.Value + data.UpgradeSignTile.Y + 1f) * 64f;
				sortY += 2f;
				sortY /= 10000f;
				b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, this.getUpgradeSignLocation()), new Microsoft.Xna.Framework.Rectangle(367, 309, 16, 15), Color.White * this.alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, sortY);
			}
		}
		else if (this.GetIndoors() is Shed)
		{
			b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, this.getUpgradeSignLocation()), new Microsoft.Xna.Framework.Rectangle(367, 309, 16, 15), Color.White * this.alpha, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)((this.tileY.Value + this.tilesHigh.Value) * 64) / 10000f + 0.0001f);
		}
	}

	public bool ShouldDrawShadow(BuildingData data)
	{
		return data?.DrawShadow ?? true;
	}

	public virtual void drawShadow(SpriteBatch b, int localX = -1, int localY = -1)
	{
		Microsoft.Xna.Framework.Rectangle sourceRectForMenu = this.getSourceRectForMenu() ?? this.getSourceRect();
		Vector2 basePosition = ((localX == -1) ? Game1.GlobalToLocal(new Vector2(this.tileX.Value * 64, (this.tileY.Value + this.tilesHigh.Value) * 64)) : new Vector2(localX, localY + sourceRectForMenu.Height * 4));
		b.Draw(Game1.mouseCursors, basePosition, Building.leftShadow, Color.White * ((localX == -1) ? this.alpha : 1f), 0f, Vector2.Zero, 4f, SpriteEffects.None, 1E-05f);
		for (int x = 1; x < this.tilesWide.Value - 1; x++)
		{
			b.Draw(Game1.mouseCursors, basePosition + new Vector2(x * 64, 0f), Building.middleShadow, Color.White * ((localX == -1) ? this.alpha : 1f), 0f, Vector2.Zero, 4f, SpriteEffects.None, 1E-05f);
		}
		b.Draw(Game1.mouseCursors, basePosition + new Vector2((this.tilesWide.Value - 1) * 64, 0f), Building.rightShadow, Color.White * ((localX == -1) ? this.alpha : 1f), 0f, Vector2.Zero, 4f, SpriteEffects.None, 1E-05f);
	}

	public virtual void OnStartMove()
	{
	}

	public virtual void OnEndMove()
	{
		Game1.player.team.SendBuildingMovedEvent(this.GetParentLocation(), this);
	}

	public Point getPorchStandingSpot()
	{
		if (this.isCabin)
		{
			return new Point(this.tileX.Value + 1, this.tileY.Value + this.tilesHigh.Value - 1);
		}
		return new Point(0, 0);
	}

	public virtual bool doesTileHaveProperty(int tile_x, int tile_y, string property_name, string layer_name, ref string property_value)
	{
		BuildingData data = this.GetData();
		if (data != null && this.daysOfConstructionLeft.Value <= 0 && data.HasPropertyAtTile(tile_x - this.tileX.Value, tile_y - this.tileY.Value, property_name, layer_name, ref property_value))
		{
			return true;
		}
		if (property_name == "NoSpawn" && layer_name == "Back" && this.occupiesTile(tile_x, tile_y))
		{
			property_value = "All";
			return true;
		}
		return false;
	}

	public Point getMailboxPosition()
	{
		if (this.isCabin)
		{
			return new Point(this.tileX.Value + this.tilesWide.Value - 1, this.tileY.Value + this.tilesHigh.Value - 1);
		}
		return new Point(68, 16);
	}

	/// <summary>Get the number of extra tiles around the building for which it may add tile properties, but without hiding tile properties from the underlying ground that aren't overwritten by the building data.</summary>
	public virtual int GetAdditionalTilePropertyRadius()
	{
		return this.GetData()?.AdditionalTilePropertyRadius ?? 0;
	}

	public void removeOverlappingBushes(GameLocation location)
	{
		for (int x = this.tileX.Value; x < this.tileX.Value + this.tilesWide.Value; x++)
		{
			for (int y = this.tileY.Value; y < this.tileY.Value + this.tilesHigh.Value; y++)
			{
				if (location.isTerrainFeatureAt(x, y))
				{
					LargeTerrainFeature large_feature = location.getLargeTerrainFeatureAt(x, y);
					if (large_feature is Bush)
					{
						location.largeTerrainFeatures.Remove(large_feature);
					}
				}
			}
		}
	}

	public virtual void drawInConstruction(SpriteBatch b)
	{
		int drawPercentage = Math.Min(16, Math.Max(0, (int)(16f - (float)this.newConstructionTimer.Value / 1000f * 16f)));
		float drawPercentageReal = (float)(2000 - this.newConstructionTimer.Value) / 2000f;
		if (this.magical.Value || this.daysOfConstructionLeft.Value <= 0)
		{
			BuildingData data = this.GetData();
			if (this.ShouldDrawShadow(data))
			{
				this.drawShadow(b);
			}
			Microsoft.Xna.Framework.Rectangle mainSourceRect = this.getSourceRect();
			Microsoft.Xna.Framework.Rectangle sourceRectForMenu = this.getSourceRectForMenu() ?? mainSourceRect;
			int yPos = (int)((float)(mainSourceRect.Height * 4) * (1f - drawPercentageReal));
			float baseSortY = (this.tileY.Value + this.tilesHigh.Value) * 64;
			float sortY = baseSortY;
			if (data != null)
			{
				sortY -= data.SortTileOffset * 64f;
			}
			sortY /= 10000f;
			Vector2 drawPosition = new Vector2(this.tileX.Value * 64, this.tileY.Value * 64 + this.tilesHigh.Value * 64);
			Vector2 drawOffset = Vector2.Zero;
			if (data != null)
			{
				drawOffset = data.DrawOffset * 4f;
			}
			Vector2 offset = new Vector2(0f, yPos + 4 - yPos % 4);
			Vector2 drawOrigin = new Vector2(0f, mainSourceRect.Height);
			b.Draw(this.texture.Value, Game1.GlobalToLocal(Game1.viewport, drawPosition + offset + drawOffset), new Microsoft.Xna.Framework.Rectangle(mainSourceRect.Left, mainSourceRect.Bottom - (int)(drawPercentageReal * (float)mainSourceRect.Height), sourceRectForMenu.Width, (int)((float)mainSourceRect.Height * drawPercentageReal)), this.color * this.alpha, 0f, new Vector2(0f, mainSourceRect.Height), 4f, SpriteEffects.None, sortY);
			if (data?.DrawLayers != null)
			{
				foreach (BuildingDrawLayer drawLayer in data.DrawLayers)
				{
					if (drawLayer.OnlyDrawIfChestHasContents != null)
					{
						continue;
					}
					sortY = baseSortY - drawLayer.SortTileOffset * 64f;
					sortY += 1f;
					sortY /= 10000f;
					Microsoft.Xna.Framework.Rectangle sourceRect = drawLayer.GetSourceRect((int)Game1.currentGameTime.TotalGameTime.TotalMilliseconds);
					sourceRect = this.ApplySourceRectOffsets(sourceRect);
					float cutoffPixels = (float)(yPos / 4) - drawLayer.DrawPosition.Y;
					drawOffset = Vector2.Zero;
					if (!(cutoffPixels > (float)sourceRect.Height))
					{
						if (cutoffPixels > 0f)
						{
							drawOffset.Y += cutoffPixels;
							sourceRect.Y += (int)cutoffPixels;
							sourceRect.Height -= (int)cutoffPixels;
						}
						Texture2D layerTexture = this.texture.Value;
						if (drawLayer.Texture != null)
						{
							layerTexture = Game1.content.Load<Texture2D>(drawLayer.Texture);
						}
						b.Draw(layerTexture, Game1.GlobalToLocal(Game1.viewport, drawPosition + (drawOffset - drawOrigin + drawLayer.DrawPosition) * 4f), sourceRect, this.color * this.alpha, 0f, new Vector2(0f, 0f), 4f, SpriteEffects.None, sortY);
					}
				}
			}
			if (this.magical.Value)
			{
				for (int i = 0; i < this.tilesWide.Value * 4; i++)
				{
					b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(this.tileX.Value * 64 + i * 16, (float)(this.tileY.Value * 64 - mainSourceRect.Height * 4 + this.tilesHigh.Value * 64) + (float)(mainSourceRect.Height * 4) * (1f - drawPercentageReal))) + new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2) - ((i % 2 == 0) ? 32 : 8)), new Microsoft.Xna.Framework.Rectangle(536 + (this.newConstructionTimer.Value + i * 4) % 56 / 8 * 8, 1945, 8, 8), (i % 2 == 1) ? (Color.Pink * this.alpha) : (Color.LightPink * this.alpha), 0f, new Vector2(0f, 0f), 4f + (float)Game1.random.Next(100) / 100f, SpriteEffects.None, (float)((this.tileY.Value + this.tilesHigh.Value) * 64) / 10000f + 0.0001f);
					if (i % 2 == 0)
					{
						b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(this.tileX.Value * 64 + i * 16, (float)(this.tileY.Value * 64 - mainSourceRect.Height * 4 + this.tilesHigh.Value * 64) + (float)(mainSourceRect.Height * 4) * (1f - drawPercentageReal))) + new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2) + ((i % 2 == 0) ? 32 : 8)), new Microsoft.Xna.Framework.Rectangle(536 + (this.newConstructionTimer.Value + i * 4) % 56 / 8 * 8, 1945, 8, 8), Color.White * this.alpha, 0f, new Vector2(0f, 0f), 4f + (float)Game1.random.Next(100) / 100f, SpriteEffects.None, (float)((this.tileY.Value + this.tilesHigh.Value) * 64) / 10000f + 0.0001f);
					}
				}
				return;
			}
			for (int j = 0; j < this.tilesWide.Value * 4; j++)
			{
				b.Draw(Game1.animations, Game1.GlobalToLocal(Game1.viewport, new Vector2(this.tileX.Value * 64 - 16 + j * 16, (float)(this.tileY.Value * 64 - mainSourceRect.Height * 4 + this.tilesHigh.Value * 64) + (float)(mainSourceRect.Height * 4) * (1f - drawPercentageReal))) + new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2) - ((j % 2 == 0) ? 32 : 8)), new Microsoft.Xna.Framework.Rectangle((this.newConstructionTimer.Value + j * 20) % 304 / 38 * 64, 768, 64, 64), Color.White * this.alpha * ((float)this.newConstructionTimer.Value / 500f), 0f, new Vector2(0f, 0f), 1f, SpriteEffects.None, (float)((this.tileY.Value + this.tilesHigh.Value) * 64) / 10000f + 0.0001f);
				if (j % 2 == 0)
				{
					b.Draw(Game1.animations, Game1.GlobalToLocal(Game1.viewport, new Vector2(this.tileX.Value * 64 - 16 + j * 16, (float)(this.tileY.Value * 64 - mainSourceRect.Height * 4 + this.tilesHigh.Value * 64) + (float)(mainSourceRect.Height * 4) * (1f - drawPercentageReal))) + new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2) - ((j % 2 == 0) ? 32 : 8)), new Microsoft.Xna.Framework.Rectangle((this.newConstructionTimer.Value + j * 20) % 400 / 50 * 64, 2944, 64, 64), Color.White * this.alpha * ((float)this.newConstructionTimer.Value / 500f), 0f, new Vector2(0f, 0f), 1f, SpriteEffects.None, (float)((this.tileY.Value + this.tilesHigh.Value) * 64) / 10000f + 0.0001f);
				}
			}
			return;
		}
		bool drawFloor = this.daysOfConstructionLeft.Value == 1;
		for (int x = this.tileX.Value; x < this.tileX.Value + this.tilesWide.Value; x++)
		{
			for (int y = this.tileY.Value; y < this.tileY.Value + this.tilesHigh.Value; y++)
			{
				if (x == this.tileX.Value + this.tilesWide.Value / 2 && y == this.tileY.Value + this.tilesHigh.Value - 1)
				{
					if (drawFloor)
					{
						b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(x, y) * 64f) + new Vector2(0f, 64 - drawPercentage * 4 + 16 - 4), new Microsoft.Xna.Framework.Rectangle(367, 277, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1E-05f);
					}
					b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(x, y) * 64f) + new Vector2(0f, 64 - drawPercentage * 4) + ((this.newConstructionTimer.Value > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero), new Microsoft.Xna.Framework.Rectangle(367, 309, 16, drawPercentage), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(y * 64 + 64 - 1) / 10000f);
				}
				else if (x == this.tileX.Value && y == this.tileY.Value)
				{
					if (drawFloor)
					{
						b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(x, y) * 64f) + new Vector2(0f, 64 - drawPercentage * 4 + 16), new Microsoft.Xna.Framework.Rectangle(351, 261, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1E-05f);
					}
					b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(x, y) * 64f) + new Vector2(0f, 64 - drawPercentage * 4) + ((this.newConstructionTimer.Value > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero), new Microsoft.Xna.Framework.Rectangle(351, 293, 16, drawPercentage), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(y * 64 + 64 - 1) / 10000f);
				}
				else if (x == this.tileX.Value + this.tilesWide.Value - 1 && y == this.tileY.Value)
				{
					if (drawFloor)
					{
						b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(x, y) * 64f) + new Vector2(0f, 64 - drawPercentage * 4 + 16), new Microsoft.Xna.Framework.Rectangle(383, 261, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1E-05f);
					}
					b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(x, y) * 64f) + new Vector2(0f, 64 - drawPercentage * 4) + ((this.newConstructionTimer.Value > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero), new Microsoft.Xna.Framework.Rectangle(383, 293, 16, drawPercentage), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(y * 64 + 64 - 1) / 10000f);
				}
				else if (x == this.tileX.Value + this.tilesWide.Value - 1 && y == this.tileY.Value + this.tilesHigh.Value - 1)
				{
					if (drawFloor)
					{
						b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(x, y) * 64f) + new Vector2(0f, 64 - drawPercentage * 4 + 16), new Microsoft.Xna.Framework.Rectangle(383, 277, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1E-05f);
					}
					b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(x, y) * 64f) + new Vector2(0f, 64 - drawPercentage * 4) + ((this.newConstructionTimer.Value > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero), new Microsoft.Xna.Framework.Rectangle(383, 325, 16, drawPercentage), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(y * 64) / 10000f);
				}
				else if (x == this.tileX.Value && y == this.tileY.Value + this.tilesHigh.Value - 1)
				{
					if (drawFloor)
					{
						b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(x, y) * 64f) + new Vector2(0f, 64 - drawPercentage * 4 + 16), new Microsoft.Xna.Framework.Rectangle(351, 277, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1E-05f);
					}
					b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(x, y) * 64f) + new Vector2(0f, 64 - drawPercentage * 4) + ((this.newConstructionTimer.Value > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero), new Microsoft.Xna.Framework.Rectangle(351, 325, 16, drawPercentage), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(y * 64) / 10000f);
				}
				else if (x == this.tileX.Value + this.tilesWide.Value - 1)
				{
					if (drawFloor)
					{
						b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(x, y) * 64f) + new Vector2(0f, 64 - drawPercentage * 4 + 16), new Microsoft.Xna.Framework.Rectangle(383, 261, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1E-05f);
					}
					b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(x, y) * 64f) + new Vector2(0f, 64 - drawPercentage * 4) + ((this.newConstructionTimer.Value > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero), new Microsoft.Xna.Framework.Rectangle(383, 309, 16, drawPercentage), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(y * 64) / 10000f);
				}
				else if (y == this.tileY.Value + this.tilesHigh.Value - 1)
				{
					if (drawFloor)
					{
						b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(x, y) * 64f) + new Vector2(0f, 64 - drawPercentage * 4 + 16), new Microsoft.Xna.Framework.Rectangle(367, 277, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1E-05f);
					}
					b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(x, y) * 64f) + new Vector2(0f, 64 - drawPercentage * 4) + ((this.newConstructionTimer.Value > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero), new Microsoft.Xna.Framework.Rectangle(367, 325, 16, drawPercentage), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(y * 64) / 10000f);
				}
				else if (x == this.tileX.Value)
				{
					if (drawFloor)
					{
						b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(x, y) * 64f) + new Vector2(0f, 64 - drawPercentage * 4 + 16), new Microsoft.Xna.Framework.Rectangle(351, 261, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1E-05f);
					}
					b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(x, y) * 64f) + new Vector2(0f, 64 - drawPercentage * 4) + ((this.newConstructionTimer.Value > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero), new Microsoft.Xna.Framework.Rectangle(351, 309, 16, drawPercentage), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(y * 64) / 10000f);
				}
				else if (y == this.tileY.Value)
				{
					if (drawFloor)
					{
						b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(x, y) * 64f) + new Vector2(0f, 64 - drawPercentage * 4 + 16), new Microsoft.Xna.Framework.Rectangle(367, 261, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1E-05f);
					}
					b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(x, y) * 64f) + new Vector2(0f, 64 - drawPercentage * 4) + ((this.newConstructionTimer.Value > 0) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero), new Microsoft.Xna.Framework.Rectangle(367, 293, 16, drawPercentage), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(y * 64 + 64 - 1) / 10000f);
				}
				else if (drawFloor)
				{
					b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(x, y) * 64f) + new Vector2(0f, 64 - drawPercentage * 4 + 16), new Microsoft.Xna.Framework.Rectangle(367, 261, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1E-05f);
				}
			}
		}
	}
}
