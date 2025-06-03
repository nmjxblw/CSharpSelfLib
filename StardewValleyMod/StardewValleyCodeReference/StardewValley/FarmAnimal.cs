using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Buildings;
using StardewValley.Extensions;
using StardewValley.GameData;
using StardewValley.GameData.Buildings;
using StardewValley.GameData.FarmAnimals;
using StardewValley.Menus;
using StardewValley.Monsters;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.Pathfinding;
using StardewValley.TerrainFeatures;
using StardewValley.TokenizableStrings;
using StardewValley.Tools;
using xTile.Dimensions;

namespace StardewValley;

public class FarmAnimal : Character
{
	public const byte eatGrassBehavior = 0;

	public const short newHome = 0;

	public const short happy = 1;

	public const short neutral = 2;

	public const short unhappy = 3;

	public const short hungry = 4;

	public const short disturbedByDog = 5;

	public const short leftOutAtNight = 6;

	public const double chancePerUpdateToChangeDirection = 0.007;

	public const byte fullnessValueOfGrass = 60;

	public const int noWarpTimerTime = 3000;

	public new const double chanceForSound = 0.002;

	public const double chanceToGoOutside = 0.002;

	public const int uniqueDownFrame = 16;

	public const int uniqueRightFrame = 18;

	public const int uniqueUpFrame = 20;

	public const int uniqueLeftFrame = 22;

	public const int pushAccumulatorTimeTillPush = 60;

	public const int timePerUniqueFrame = 500;

	/// <summary>The texture name to load if the animal's actual sprite can't be loaded.</summary>
	public const string ErrorTextureName = "Animals\\Error";

	/// <summary>The pixel size of sprites in the <see cref="F:StardewValley.FarmAnimal.ErrorTextureName" />.</summary>
	public const int ErrorSpriteSize = 16;

	public NetBool isSwimming = new NetBool();

	[XmlIgnore]
	public Vector2 hopOffset = new Vector2(0f, 0f);

	[XmlElement("currentProduce")]
	public readonly NetString currentProduce = new NetString();

	[XmlElement("friendshipTowardFarmer")]
	public readonly NetInt friendshipTowardFarmer = new NetInt();

	[XmlElement("skinID")]
	public readonly NetString skinID = new NetString();

	[XmlIgnore]
	public int pushAccumulator;

	[XmlIgnore]
	public int uniqueFrameAccumulator = -1;

	[XmlElement("age")]
	public readonly NetInt age = new NetInt();

	[XmlElement("daysOwned")]
	public readonly NetInt daysOwned = new NetInt(-1);

	[XmlElement("health")]
	public readonly NetInt health = new NetInt();

	[XmlElement("produceQuality")]
	public readonly NetInt produceQuality = new NetInt();

	[XmlElement("daysSinceLastLay")]
	public readonly NetInt daysSinceLastLay = new NetInt();

	[XmlElement("happiness")]
	public readonly NetInt happiness = new NetInt();

	[XmlElement("fullness")]
	public readonly NetInt fullness = new NetInt();

	[XmlElement("wasAutoPet")]
	public readonly NetBool wasAutoPet = new NetBool();

	[XmlElement("wasPet")]
	public readonly NetBool wasPet = new NetBool();

	[XmlElement("allowReproduction")]
	public readonly NetBool allowReproduction = new NetBool(value: true);

	[XmlElement("type")]
	public readonly NetString type = new NetString();

	[XmlElement("buildingTypeILiveIn")]
	public readonly NetString buildingTypeILiveIn = new NetString();

	[XmlElement("myID")]
	public readonly NetLong myID = new NetLong();

	[XmlElement("ownerID")]
	public readonly NetLong ownerID = new NetLong();

	[XmlElement("parentId")]
	public readonly NetLong parentId = new NetLong(-1L);

	[XmlIgnore]
	private readonly NetLocationRef netHomeInterior = new NetLocationRef();

	[XmlElement("hasEatenAnimalCracker")]
	public readonly NetBool hasEatenAnimalCracker = new NetBool();

	[XmlIgnore]
	public int noWarpTimer;

	[XmlIgnore]
	public int hitGlowTimer;

	[XmlIgnore]
	public int pauseTimer;

	[XmlElement("moodMessage")]
	public readonly NetInt moodMessage = new NetInt();

	[XmlElement("isEating")]
	public readonly NetBool isEating = new NetBool();

	[XmlIgnore]
	private readonly NetEvent1Field<int, NetInt> doFarmerPushEvent = new NetEvent1Field<int, NetInt>();

	[XmlIgnore]
	private readonly NetEvent0 doBuildingPokeEvent = new NetEvent0();

	[XmlIgnore]
	private readonly NetEvent0 doDiveEvent = new NetEvent0();

	private string _displayHouse;

	private string _displayType;

	public static int NumPathfindingThisTick = 0;

	public static int MaxPathfindingPerTick = 1;

	[XmlIgnore]
	public int nextRipple;

	[XmlIgnore]
	public int nextFollowDirectionChange;

	protected FarmAnimal _followTarget;

	protected Point? _followTargetPosition;

	protected float _nextFollowTargetScan = 1f;

	[XmlIgnore]
	public int bobOffset;

	[XmlIgnore]
	protected Vector2 _swimmingVelocity = Vector2.Zero;

	[XmlIgnore]
	public static HashSet<Grass> reservedGrass = new HashSet<Grass>();

	[XmlIgnore]
	public Grass foundGrass;

	/// <summary>The building within which the animal is normally housed, if any.</summary>
	[XmlIgnore]
	public Building home
	{
		get
		{
			return this.netHomeInterior.Value?.ParentBuilding;
		}
		set
		{
			this.netHomeInterior.Value = value?.GetIndoors();
		}
	}

	[XmlIgnore]
	public GameLocation homeInterior
	{
		get
		{
			return this.netHomeInterior.Value;
		}
		set
		{
			this.netHomeInterior.Value = value;
		}
	}

	[XmlIgnore]
	public string displayHouse
	{
		get
		{
			if (this._displayHouse == null)
			{
				FarmAnimalData data = this.GetAnimalData();
				if (data != null)
				{
					this._displayHouse = (Game1.buildingData.TryGetValue(data.House, out var buildingData) ? TokenParser.ParseText(buildingData.Name) : data.House);
				}
				else
				{
					this._displayHouse = this.buildingTypeILiveIn.Value;
				}
			}
			return this._displayHouse;
		}
		set
		{
			this._displayHouse = value;
		}
	}

	[XmlIgnore]
	public string displayType
	{
		get
		{
			if (this._displayType == null)
			{
				this._displayType = TokenParser.ParseText(this.GetAnimalData()?.DisplayName);
			}
			return this._displayType;
		}
		set
		{
			this._displayType = value;
		}
	}

	public override string displayName
	{
		get
		{
			return base.Name;
		}
		set
		{
		}
	}

	/// <summary>Get whether the farm animal is currently inside their home building.</summary>
	[MemberNotNullWhen(true, "home")]
	public bool IsHome
	{
		[MemberNotNullWhen(true, "home")]
		get
		{
			return this.homeInterior?.animals.ContainsKey(this.myID.Value) ?? false;
		}
	}

	public FarmAnimal()
	{
	}

	protected override void initNetFields()
	{
		this.bobOffset = Game1.random.Next(0, 1000);
		base.initNetFields();
		base.NetFields.AddField(this.currentProduce, "currentProduce").AddField(this.friendshipTowardFarmer, "friendshipTowardFarmer").AddField(this.age, "age")
			.AddField(this.health, "health")
			.AddField(this.produceQuality, "produceQuality")
			.AddField(this.daysSinceLastLay, "daysSinceLastLay")
			.AddField(this.happiness, "happiness")
			.AddField(this.fullness, "fullness")
			.AddField(this.wasPet, "wasPet")
			.AddField(this.wasAutoPet, "wasAutoPet")
			.AddField(this.allowReproduction, "allowReproduction")
			.AddField(this.type, "type")
			.AddField(this.buildingTypeILiveIn, "buildingTypeILiveIn")
			.AddField(this.myID, "myID")
			.AddField(this.ownerID, "ownerID")
			.AddField(this.parentId, "parentId")
			.AddField(this.netHomeInterior.NetFields, "netHomeInterior.NetFields")
			.AddField(this.moodMessage, "moodMessage")
			.AddField(this.isEating, "isEating")
			.AddField(this.doFarmerPushEvent, "doFarmerPushEvent")
			.AddField(this.doBuildingPokeEvent, "doBuildingPokeEvent")
			.AddField(this.isSwimming, "isSwimming")
			.AddField(this.doDiveEvent.NetFields, "doDiveEvent.NetFields")
			.AddField(this.daysOwned, "daysOwned")
			.AddField(this.skinID, "skinID")
			.AddField(this.hasEatenAnimalCracker, "hasEatenAnimalCracker");
		base.position.Field.AxisAlignedMovement = true;
		this.doFarmerPushEvent.onEvent += doFarmerPush;
		this.doBuildingPokeEvent.onEvent += doBuildingPoke;
		this.doDiveEvent.onEvent += doDive;
		this.skinID.fieldChangeVisibleEvent += delegate
		{
			if (Game1.gameMode != 6)
			{
				this.ReloadTextureIfNeeded();
			}
		};
		this.isSwimming.fieldChangeVisibleEvent += delegate
		{
			if (this.isSwimming.Value)
			{
				base.position.Field.AxisAlignedMovement = false;
			}
			else
			{
				base.position.Field.AxisAlignedMovement = true;
			}
		};
		base.name.FilterStringEvent += Utility.FilterDirtyWords;
	}

	public FarmAnimal(string type, long id, long ownerID)
		: base(null, new Vector2(64 * Game1.random.Next(2, 9), 64 * Game1.random.Next(4, 8)), 2, type)
	{
		this.ownerID.Value = ownerID;
		this.health.Value = 3;
		this.myID.Value = id;
		if (type == "Dairy Cow")
		{
			type = "Brown Cow";
		}
		this.type.Value = type;
		base.Name = Dialogue.randomName();
		this.displayName = base.name.Value;
		this.happiness.Value = 255;
		this.fullness.Value = 255;
		this._nextFollowTargetScan = Utility.RandomFloat(1f, 3f);
		this.ReloadTextureIfNeeded(forceReload: true);
		FarmAnimalData data = this.GetAnimalData();
		if (data == null)
		{
			Game1.log.Warn("Constructed farm animal type '" + type + "' which has no entry in Data/FarmAnimals.");
		}
		this.buildingTypeILiveIn.Value = data?.House;
		if (data?.Skins == null)
		{
			return;
		}
		Random random = Utility.CreateRandom(id);
		float totalWeight = 1f;
		foreach (FarmAnimalSkin skin in data.Skins)
		{
			totalWeight += skin.Weight;
		}
		totalWeight = Utility.RandomFloat(0f, totalWeight, random);
		foreach (FarmAnimalSkin skin2 in data.Skins)
		{
			totalWeight -= skin2.Weight;
			if (totalWeight <= 0f)
			{
				this.skinID.Value = skin2.Id;
				break;
			}
		}
	}

	/// <summary>Get the animal's <see cref="P:StardewValley.Character.Sprite" /> value, loading it if needed.</summary>
	public AnimatedSprite GetOrLoadTexture()
	{
		AnimatedSprite loaded = this.Sprite;
		if (loaded == null)
		{
			this.ReloadTextureIfNeeded();
			loaded = this.Sprite;
		}
		return loaded;
	}

	/// <summary>Reload the texture if the asset name should change based on the current animal state and data.</summary>
	/// <param name="forceReload">Whether to reload the texture even if the texture path hasn't changed.</param>
	public void ReloadTextureIfNeeded(bool forceReload = false)
	{
		if (this.Sprite == null || forceReload)
		{
			FarmAnimalData data = this.GetAnimalData();
			string texturePath;
			int spriteWidth;
			int spriteHeight;
			if (data != null)
			{
				texturePath = this.GetTexturePath(data);
				spriteWidth = data.SpriteWidth;
				spriteHeight = data.SpriteHeight;
			}
			else
			{
				texturePath = "Animals\\Error";
				spriteWidth = 16;
				spriteHeight = 16;
			}
			if (!Game1.content.DoesAssetExist<Texture2D>(texturePath))
			{
				Game1.log.Warn($"Farm animal '{this.type.Value}' failed to load texture path '{texturePath}': asset doesn't exist. Defaulting to error texture.");
				texturePath = "Animals\\Error";
				spriteWidth = 16;
				spriteHeight = 16;
			}
			this.Sprite = new AnimatedSprite(texturePath, 0, spriteWidth, spriteHeight)
			{
				textureUsesFlippedRightForLeft = (data?.UseFlippedRightForLeft ?? false)
			};
			this.ValidateSpritesheetSize();
		}
		else
		{
			string texturePath2 = this.GetTexturePath();
			if (this.Sprite.textureName.Value != texturePath2)
			{
				this.Sprite.LoadTexture(texturePath2);
			}
		}
	}

	public string GetTexturePath()
	{
		return this.GetTexturePath(this.GetAnimalData());
	}

	public virtual string GetTexturePath(FarmAnimalData data)
	{
		string texturePath = "Animals\\" + this.type.Value;
		if (data != null)
		{
			FarmAnimalSkin skin = null;
			if (this.skinID.Value != null && data.Skins != null)
			{
				foreach (FarmAnimalSkin animalSkin in data.Skins)
				{
					if (this.skinID.Value == animalSkin.Id)
					{
						skin = animalSkin;
						break;
					}
				}
			}
			if (skin != null && skin.Texture != null)
			{
				texturePath = skin.Texture;
			}
			else if (data.Texture != null)
			{
				texturePath = data.Texture;
			}
			if (this.currentProduce.Value == null)
			{
				if (skin != null && skin.HarvestedTexture != null)
				{
					texturePath = skin.HarvestedTexture;
				}
				else if (data.HarvestedTexture != null)
				{
					texturePath = data.HarvestedTexture;
				}
			}
			if (this.isBaby())
			{
				if (skin != null && skin.BabyTexture != null)
				{
					texturePath = skin.BabyTexture;
				}
				else if (data.BabyTexture != null)
				{
					texturePath = data.BabyTexture;
				}
			}
		}
		return texturePath;
	}

	public static FarmAnimalData GetAnimalDataFromEgg(Item eggItem, GameLocation location)
	{
		if (!FarmAnimal.TryGetAnimalDataFromEgg(eggItem, location, out var _, out var data))
		{
			return null;
		}
		return data;
	}

	public static bool TryGetAnimalDataFromEgg(Item eggItem, GameLocation location, out string id, out FarmAnimalData data)
	{
		if (!eggItem.HasTypeObject())
		{
			id = null;
			data = null;
			return false;
		}
		List<string> validOccupantTypes = location?.ParentBuilding?.GetData()?.ValidOccupantTypes;
		foreach (KeyValuePair<string, FarmAnimalData> pair in Game1.farmAnimalData)
		{
			FarmAnimalData animalData = pair.Value;
			if (animalData.EggItemIds != null && animalData.EggItemIds.Count != 0 && (validOccupantTypes == null || validOccupantTypes.Contains(animalData.House)) && animalData.EggItemIds.Contains(eggItem.ItemId))
			{
				id = pair.Key;
				data = animalData;
				return true;
			}
		}
		id = null;
		data = null;
		return false;
	}

	public virtual FarmAnimalData GetAnimalData()
	{
		if (!Game1.farmAnimalData.TryGetValue(this.type.Value, out var animalData))
		{
			return null;
		}
		return animalData;
	}

	/// <summary>Get the translated display name for a farm animal from its data, if any.</summary>
	/// <param name="id">The animal type ID in <c>Data/FarmAnimals</c>.</param>
	/// <param name="forShop">Whether to get the shop name, if applicable.</param>
	public static string GetDisplayName(string id, bool forShop = false)
	{
		if (!Game1.farmAnimalData.TryGetValue(id, out var data))
		{
			return null;
		}
		return TokenParser.ParseText(forShop ? (data.ShopDisplayName ?? data.DisplayName) : data.DisplayName);
	}

	/// <summary>Get the translated shop description for a farm animal from its data, if any.</summary>
	/// <param name="id">The animal type ID in <c>Data/FarmAnimals</c>.</param>
	public static string GetShopDescription(string id)
	{
		if (!Game1.farmAnimalData.TryGetValue(id, out var data))
		{
			return null;
		}
		return TokenParser.ParseText(data.ShopDescription);
	}

	public string shortDisplayType()
	{
		switch (LocalizedContentManager.CurrentLanguageCode)
		{
		case LocalizedContentManager.LanguageCode.en:
			return ArgUtility.SplitBySpace(this.displayType).Last();
		case LocalizedContentManager.LanguageCode.ja:
			if (!this.displayType.Contains("トリ"))
			{
				if (!this.displayType.Contains("ウシ"))
				{
					if (!this.displayType.Contains("ブタ"))
					{
						return this.displayType;
					}
					return "ブタ";
				}
				return "ウシ";
			}
			return "トリ";
		case LocalizedContentManager.LanguageCode.ru:
			if (!this.displayType.ContainsIgnoreCase("курица"))
			{
				if (!this.displayType.ContainsIgnoreCase("корова"))
				{
					return this.displayType;
				}
				return "Корова";
			}
			return "Курица";
		case LocalizedContentManager.LanguageCode.zh:
			if (!this.displayType.Contains('鸡'))
			{
				if (!this.displayType.Contains('牛'))
				{
					if (!this.displayType.Contains('猪'))
					{
						return this.displayType;
					}
					return "猪";
				}
				return "牛";
			}
			return "鸡";
		case LocalizedContentManager.LanguageCode.pt:
		case LocalizedContentManager.LanguageCode.es:
			return ArgUtility.SplitBySpaceAndGet(this.displayType, 0);
		case LocalizedContentManager.LanguageCode.de:
			return ArgUtility.SplitBySpace(this.displayType).Last().Split('-')
				.Last();
		default:
			return this.displayType;
		}
	}

	public Microsoft.Xna.Framework.Rectangle GetHarvestBoundingBox()
	{
		Vector2 position = base.Position;
		return new Microsoft.Xna.Framework.Rectangle((int)(position.X + (float)(this.Sprite.getWidth() * 4 / 2) - 32f + 4f), (int)(position.Y + (float)(this.Sprite.getHeight() * 4) - 64f - 24f), 56, 72);
	}

	public Microsoft.Xna.Framework.Rectangle GetCursorPetBoundingBox()
	{
		Vector2 position = base.Position;
		FarmAnimalData animalData = this.GetAnimalData();
		if (animalData != null)
		{
			int width;
			int height;
			if (this.isBaby())
			{
				if (this.FacingDirection == 0 || this.FacingDirection == 2 || this.Sprite.currentFrame >= 12)
				{
					width = (int)(animalData.BabyUpDownPetHitboxTileSize.X * 64f);
					height = (int)(animalData.BabyUpDownPetHitboxTileSize.Y * 64f);
				}
				else
				{
					width = (int)(animalData.BabyLeftRightPetHitboxTileSize.X * 64f);
					height = (int)(animalData.BabyLeftRightPetHitboxTileSize.Y * 64f);
				}
			}
			else if (this.FacingDirection == 0 || this.FacingDirection == 2 || this.Sprite.currentFrame >= 12)
			{
				width = (int)(animalData.UpDownPetHitboxTileSize.X * 64f);
				height = (int)(animalData.UpDownPetHitboxTileSize.Y * 64f);
			}
			else
			{
				width = (int)(animalData.LeftRightPetHitboxTileSize.X * 64f);
				height = (int)(animalData.LeftRightPetHitboxTileSize.Y * 64f);
			}
			return new Microsoft.Xna.Framework.Rectangle((int)(base.Position.X + (float)(this.Sprite.getWidth() * 4 / 2) - (float)(width / 2)), (int)(base.Position.Y - 24f + (float)(this.Sprite.getHeight() * 4) - (float)height), width, height);
		}
		return new Microsoft.Xna.Framework.Rectangle((int)(position.X + (float)(this.Sprite.getWidth() * 4 / 2) - 32f + 4f), (int)(position.Y + (float)(this.Sprite.getHeight() * 4) - 64f - 24f), 56, 72);
	}

	public override Microsoft.Xna.Framework.Rectangle GetBoundingBox()
	{
		Vector2 position = base.Position;
		return new Microsoft.Xna.Framework.Rectangle((int)(position.X + (float)(this.Sprite.getWidth() * 4 / 2) - 32f + 8f), (int)(position.Y + (float)(this.Sprite.getHeight() * 4) - 64f + 8f), 48, 48);
	}

	public void reload(GameLocation homeInterior)
	{
		this.homeInterior = homeInterior;
		this.ReloadTextureIfNeeded();
	}

	public void reload(Building home)
	{
		this.reload(home?.GetIndoors());
	}

	public int GetDaysOwned()
	{
		if (this.daysOwned.Value < 0)
		{
			this.daysOwned.Value = this.age.Value;
		}
		return this.daysOwned.Value;
	}

	public void pet(Farmer who, bool is_auto_pet = false)
	{
		if (!is_auto_pet)
		{
			if (who.FarmerSprite.PauseForSingleAnimation)
			{
				return;
			}
			who.Halt();
			who.faceGeneralDirection(base.Position, 0, opposite: false, useTileCalculations: false);
			if (Game1.timeOfDay >= 1900 && !this.isMoving())
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\FarmAnimals:TryingToSleep", this.displayName));
				return;
			}
			this.Halt();
			this.Sprite.StopAnimation();
			this.uniqueFrameAccumulator = -1;
			switch (Game1.player.FacingDirection)
			{
			case 0:
				this.Sprite.currentFrame = 0;
				break;
			case 1:
				this.Sprite.currentFrame = 12;
				break;
			case 2:
				this.Sprite.currentFrame = 8;
				break;
			case 3:
				this.Sprite.currentFrame = 4;
				break;
			}
			if (!this.hasEatenAnimalCracker.Value && who.ActiveObject?.QualifiedItemId == "(O)GoldenAnimalCracker")
			{
				if ((!(this.GetAnimalData()?.CanEatGoldenCrackers)) ?? false)
				{
					Game1.playSound("cancel");
					base.doEmote(8);
					return;
				}
				this.hasEatenAnimalCracker.Value = true;
				Game1.playSound("give_gift");
				base.doEmote(56);
				Game1.player.reduceActiveItemByOne();
				return;
			}
		}
		else if (this.wasAutoPet.Value)
		{
			return;
		}
		if (!this.wasPet.Value)
		{
			if (!is_auto_pet)
			{
				this.wasPet.Value = true;
			}
			int auto_pet_reduction = 7;
			if (this.wasAutoPet.Value)
			{
				this.friendshipTowardFarmer.Value = Math.Min(1000, this.friendshipTowardFarmer.Value + auto_pet_reduction);
			}
			else if (is_auto_pet)
			{
				this.friendshipTowardFarmer.Value = Math.Min(1000, this.friendshipTowardFarmer.Value + (15 - auto_pet_reduction));
			}
			else
			{
				this.friendshipTowardFarmer.Value = Math.Min(1000, this.friendshipTowardFarmer.Value + 15);
			}
			if (is_auto_pet)
			{
				this.wasAutoPet.Value = true;
			}
			FarmAnimalData data = this.GetAnimalData();
			int happinessDrain = data?.HappinessDrain ?? 0;
			if (!is_auto_pet)
			{
				if (data != null && data.ProfessionForHappinessBoost >= 0 && who.professions.Contains(data.ProfessionForHappinessBoost))
				{
					this.friendshipTowardFarmer.Value = Math.Min(1000, this.friendshipTowardFarmer.Value + 15);
					this.happiness.Value = (byte)Math.Min(255, this.happiness.Value + Math.Max(5, 30 + happinessDrain));
				}
				int emote_index = 20;
				if (this.wasAutoPet.Value)
				{
					emote_index = 32;
				}
				base.doEmote((this.moodMessage.Value == 4) ? 12 : emote_index);
			}
			this.happiness.Value = (byte)Math.Min(255, this.happiness.Value + Math.Max(5, 30 + happinessDrain));
			if (!is_auto_pet)
			{
				this.makeSound();
				who.gainExperience(0, 5);
			}
		}
		else if (!is_auto_pet && who.ActiveObject?.QualifiedItemId != "(O)178")
		{
			Game1.activeClickableMenu = new AnimalQueryMenu(this);
		}
	}

	public void farmerPushing()
	{
		this.pushAccumulator++;
		if (this.pushAccumulator > 60)
		{
			this.doFarmerPushEvent.Fire(Game1.player.FacingDirection);
			Microsoft.Xna.Framework.Rectangle bounds = this.GetBoundingBox();
			bounds = Utility.ExpandRectangle(bounds, Utility.GetOppositeFacingDirection(Game1.player.FacingDirection), 6);
			Game1.player.TemporaryPassableTiles.Add(bounds);
			this.pushAccumulator = 0;
		}
	}

	public virtual void doDive()
	{
		base.yJumpVelocity = 8f;
		base.yJumpOffset = 1;
	}

	public void doFarmerPush(int direction)
	{
		if (Game1.IsMasterGame)
		{
			switch (direction)
			{
			case 0:
				this.Halt();
				break;
			case 1:
				this.Halt();
				break;
			case 2:
				this.Halt();
				break;
			case 3:
				this.Halt();
				break;
			}
		}
	}

	public void Poke()
	{
		this.doBuildingPokeEvent.Fire();
	}

	public void doBuildingPoke()
	{
		if (Game1.IsMasterGame)
		{
			this.FacingDirection = Game1.random.Next(4);
			base.setMovingInFacingDirection();
		}
	}

	public void setRandomPosition(GameLocation location)
	{
		this.StopAllActions();
		if (!location.TryGetMapPropertyAs("ProduceArea", out Microsoft.Xna.Framework.Rectangle produceArea, required: true))
		{
			return;
		}
		base.Position = new Vector2(Game1.random.Next(produceArea.X, produceArea.Right) * 64, Game1.random.Next(produceArea.Y, produceArea.Bottom) * 64);
		int tries = 0;
		while (base.Position.Equals(Vector2.Zero) || location.Objects.ContainsKey(base.Position) || location.isCollidingPosition(this.GetBoundingBox(), Game1.viewport, isFarmer: false, 0, glider: false, this))
		{
			base.Position = new Vector2(Game1.random.Next(produceArea.X, produceArea.Right), Game1.random.Next(produceArea.Y, produceArea.Bottom)) * 64f;
			tries++;
			if (tries > 64)
			{
				break;
			}
		}
		this.SleepIfNecessary();
	}

	public virtual void StopAllActions()
	{
		this.foundGrass = null;
		base.controller = null;
		this.isSwimming.Value = false;
		this.hopOffset = Vector2.Zero;
		this._followTarget = null;
		this._followTargetPosition = null;
		this.Halt();
		this.Sprite.StopAnimation();
		this.Sprite.UpdateSourceRect();
	}

	public virtual void HandleStatsOnProduceCollected(Item item, uint amount = 1u)
	{
		this.HandleStats(this.GetAnimalData()?.StatToIncrementOnProduce, item, amount);
	}

	public virtual void HandleStats(List<StatIncrement> stats, Item item, uint amount = 1u)
	{
		if (stats == null)
		{
			return;
		}
		foreach (StatIncrement stat in stats)
		{
			if (stat.RequiredItemId == null || ItemRegistry.HasItemId(item, stat.RequiredItemId))
			{
				List<string> requiredTags = stat.RequiredTags;
				if (requiredTags == null || requiredTags.Count <= 0 || ItemContextTagManager.DoAllTagsMatch(stat.RequiredTags, item.GetContextTags()))
				{
					Game1.stats.Increment(stat.StatName, amount);
				}
			}
		}
	}

	public string GetProduceID(Random r, bool deluxe = false)
	{
		FarmAnimalData data = this.GetAnimalData();
		if (data == null)
		{
			return null;
		}
		List<FarmAnimalProduce> produceList = new List<FarmAnimalProduce>();
		if (deluxe)
		{
			if (data.DeluxeProduceItemIds != null)
			{
				produceList.AddRange(data.DeluxeProduceItemIds);
			}
		}
		else if (data.ProduceItemIds != null)
		{
			produceList.AddRange(data.ProduceItemIds);
		}
		produceList.RemoveAll((FarmAnimalProduce produce) => (produce.MinimumFriendship > 0 && this.friendshipTowardFarmer.Value < produce.MinimumFriendship) || (produce.Condition != null && !GameStateQuery.CheckConditions(produce.Condition, base.currentLocation, null, null, null, r)));
		return r.ChooseFrom(produceList)?.ItemId;
	}

	/// <summary>Update the animal state when setting up the new day, before the game saves overnight.</summary>
	/// <param name="environment">The location containing the animal.</param>
	/// <remarks>See also <see cref="M:StardewValley.FarmAnimal.OnDayStarted" />, which happens after saving when the day has started.</remarks>
	public void dayUpdate(GameLocation environment)
	{
		if (this.daysOwned.Value < 0)
		{
			this.daysOwned.Value = this.age.Value;
		}
		FarmAnimalData data = this.GetAnimalData();
		int happinessDrain = this.GetAnimalData()?.HappinessDrain ?? 0;
		int produceSpeedBonus = ((data != null && data.FriendshipForFasterProduce >= 0 && this.friendshipTowardFarmer.Value >= data.FriendshipForFasterProduce) ? 1 : 0);
		this.StopAllActions();
		this.health.Value = 3;
		bool wasLeftOutLastNight = false;
		GameLocation insideHome = this.homeInterior;
		if (insideHome != null && !this.IsHome)
		{
			if (this.home.animalDoorOpen.Value)
			{
				environment.animals.Remove(this.myID.Value);
				insideHome.animals.TryAdd(this.myID.Value, this);
				if (Game1.timeOfDay > 1800 && base.controller == null)
				{
					this.happiness.Value /= 2;
				}
				this.setRandomPosition(insideHome);
				return;
			}
			this.moodMessage.Value = 6;
			wasLeftOutLastNight = true;
			this.happiness.Value /= 2;
		}
		else if (insideHome != null && this.IsHome && !this.home.animalDoorOpen.Value)
		{
			this.happiness.Value = (byte)Math.Min(255, this.happiness.Value + happinessDrain * 2);
		}
		this.daysSinceLastLay.Value++;
		if (!this.wasPet.Value && !this.wasAutoPet.Value)
		{
			this.friendshipTowardFarmer.Value = Math.Max(0, this.friendshipTowardFarmer.Value - (10 - this.friendshipTowardFarmer.Value / 200));
			this.happiness.Value = (byte)Math.Max(0, this.happiness.Value - 50);
		}
		this.wasPet.Value = false;
		this.wasAutoPet.Value = false;
		this.daysOwned.Value++;
		if (this.fullness.Value < 200 && environment is AnimalHouse)
		{
			KeyValuePair<Vector2, Object>[] array = environment.objects.Pairs.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				KeyValuePair<Vector2, Object> pair = array[i];
				if (pair.Value.QualifiedItemId == "(O)178")
				{
					environment.objects.Remove(pair.Key);
					this.fullness.Value = 255;
					break;
				}
			}
		}
		Random r = Utility.CreateRandom((double)this.myID.Value / 2.0, Game1.stats.DaysPlayed);
		if (this.fullness.Value > 200 || r.NextDouble() < (double)(this.fullness.Value - 30) / 170.0)
		{
			if (this.age.Value == ((data != null) ? new int?(data.DaysToMature - 1) : ((int?)null)))
			{
				this.growFully(r);
			}
			else
			{
				this.age.Value++;
			}
			this.happiness.Value = (byte)Math.Min(255, this.happiness.Value + happinessDrain * 2);
		}
		if (this.fullness.Value < 200)
		{
			this.happiness.Value = (byte)Math.Max(0, this.happiness.Value - 100);
			this.friendshipTowardFarmer.Value = Math.Max(0, this.friendshipTowardFarmer.Value - 20);
		}
		Farmer owner = Game1.GetPlayer(this.ownerID.Value) ?? Game1.MasterPlayer;
		if (data != null && data.ProfessionForFasterProduce >= 0 && owner.professions.Contains(data.ProfessionForFasterProduce))
		{
			produceSpeedBonus++;
		}
		bool produceToday = this.daysSinceLastLay.Value >= ((data != null) ? new int?(data.DaysToProduce - produceSpeedBonus) : ((int?)null)) && r.NextDouble() < (double)this.fullness.Value / 200.0 && r.NextDouble() < (double)this.happiness.Value / 70.0;
		string whichProduce;
		if (!produceToday || this.isBaby())
		{
			whichProduce = null;
		}
		else
		{
			whichProduce = this.GetProduceID(r);
			if (r.NextDouble() < (double)this.happiness.Value / 150.0)
			{
				float happinessModifier = ((this.happiness.Value > 200) ? ((float)this.happiness.Value * 1.5f) : ((float)((this.happiness.Value <= 100) ? (this.happiness.Value - 100) : 0)));
				string deluxeProduce = this.GetProduceID(r, deluxe: true);
				if (data != null && data.DeluxeProduceCareDivisor >= 0f && deluxeProduce != null && this.friendshipTowardFarmer.Value >= data.DeluxeProduceMinimumFriendship && r.NextDouble() < (double)(((float)this.friendshipTowardFarmer.Value + happinessModifier) / data.DeluxeProduceCareDivisor) + Game1.player.team.AverageDailyLuck() * (double)data.DeluxeProduceLuckMultiplier)
				{
					whichProduce = deluxeProduce;
				}
				this.daysSinceLastLay.Value = 0;
				double chanceForQuality = (float)this.friendshipTowardFarmer.Value / 1000f - (1f - (float)this.happiness.Value / 225f);
				if (data != null && data.ProfessionForQualityBoost >= 0 && owner.professions.Contains(data.ProfessionForQualityBoost))
				{
					chanceForQuality += 0.33;
				}
				if (chanceForQuality >= 0.95 && r.NextDouble() < chanceForQuality / 2.0)
				{
					this.produceQuality.Value = 4;
				}
				else if (r.NextDouble() < chanceForQuality / 2.0)
				{
					this.produceQuality.Value = 2;
				}
				else if (r.NextDouble() < chanceForQuality)
				{
					this.produceQuality.Value = 1;
				}
				else
				{
					this.produceQuality.Value = 0;
				}
			}
		}
		if ((data == null || data.HarvestType != FarmAnimalHarvestType.DropOvernight) && produceToday)
		{
			this.currentProduce.Value = whichProduce;
			whichProduce = null;
		}
		if (whichProduce != null && this.home != null)
		{
			bool spawn_object = true;
			Object producedObject = ItemRegistry.Create<Object>("(O)" + whichProduce);
			producedObject.CanBeSetDown = false;
			producedObject.Quality = this.produceQuality.Value;
			if (this.hasEatenAnimalCracker.Value)
			{
				producedObject.Stack = 2;
			}
			this.HandleStats(data?.StatToIncrementOnProduce, producedObject, (uint)producedObject.Stack);
			foreach (Object location_object in insideHome.objects.Values)
			{
				if (location_object.QualifiedItemId == "(BC)165" && location_object.heldObject.Value is Chest chest && chest.addItem(producedObject) == null)
				{
					location_object.showNextIndex.Value = true;
					spawn_object = false;
					break;
				}
			}
			if (spawn_object)
			{
				producedObject.Stack = 1;
				Utility.spawnObjectAround(base.Tile, producedObject, environment);
				if (this.hasEatenAnimalCracker.Value)
				{
					Object o = (Object)producedObject.getOne();
					Utility.spawnObjectAround(base.Tile, o, environment);
				}
			}
		}
		if (!wasLeftOutLastNight)
		{
			if (this.fullness.Value < 30)
			{
				this.moodMessage.Value = 4;
			}
			else if (this.happiness.Value < 30)
			{
				this.moodMessage.Value = 3;
			}
			else if (this.happiness.Value < 200)
			{
				this.moodMessage.Value = 2;
			}
			else
			{
				this.moodMessage.Value = 1;
			}
		}
		this.fullness.Value = 0;
		if (Utility.isFestivalDay())
		{
			this.fullness.Value = 250;
		}
		this.reload(this.homeInterior);
	}

	/// <summary>Handle the new day starting after the player saves, loads, or connects.</summary>
	/// <remarks>See also <see cref="M:StardewValley.FarmAnimal.dayUpdate(StardewValley.GameLocation)" />, which happens while setting up the day before saving.</remarks>
	public void OnDayStarted()
	{
		FarmAnimalData animalData = this.GetAnimalData();
		if (animalData != null && animalData.GrassEatAmount < 1)
		{
			this.fullness.Value = 255;
		}
	}

	public int getSellPrice()
	{
		int num = this.GetAnimalData()?.SellPrice ?? 0;
		double adjustedFriendship = (double)this.friendshipTowardFarmer.Value / 1000.0 + 0.3;
		return (int)((double)num * adjustedFriendship);
	}

	public bool isMale()
	{
		return this.GetAnimalData()?.Gender switch
		{
			FarmAnimalGender.Female => false, 
			FarmAnimalGender.Male => true, 
			_ => this.myID.Value % 2 == 0, 
		};
	}

	public string getMoodMessage()
	{
		string gender = (this.isMale() ? "Male" : "Female");
		switch (this.moodMessage.Value)
		{
		case 0:
			if (this.parentId.Value != -1)
			{
				return Game1.content.LoadString("Strings\\FarmAnimals:MoodMessage_NewHome_Baby_" + gender, this.displayName);
			}
			return Game1.content.LoadString("Strings\\FarmAnimals:MoodMessage_NewHome_Adult_" + gender + "_" + (Game1.dayOfMonth % 2 + 1), this.displayName);
		case 6:
			return Game1.content.LoadString("Strings\\FarmAnimals:MoodMessage_LeftOutsideAtNight_" + gender, this.displayName);
		case 5:
			return Game1.content.LoadString("Strings\\FarmAnimals:MoodMessage_DisturbedByDog_" + gender, this.displayName);
		case 4:
			return Game1.content.LoadString("Strings\\FarmAnimals:MoodMessage_" + (((Game1.dayOfMonth + this.myID.Value) % 2 == 0L) ? "Hungry1" : "Hungry2"), this.displayName);
		default:
			if (this.happiness.Value < 30)
			{
				this.moodMessage.Value = 3;
			}
			else if (this.happiness.Value < 200)
			{
				this.moodMessage.Value = 2;
			}
			else
			{
				this.moodMessage.Value = 1;
			}
			return this.moodMessage.Value switch
			{
				3 => Game1.content.LoadString("Strings\\FarmAnimals:MoodMessage_Sad", this.displayName), 
				2 => Game1.content.LoadString("Strings\\FarmAnimals:MoodMessage_Fine", this.displayName), 
				1 => Game1.content.LoadString("Strings\\FarmAnimals:MoodMessage_Happy", this.displayName), 
				_ => "", 
			};
		}
	}

	/// <summary>Get whether this farm animal is fully grown.</summary>
	/// <remarks>See also <see cref="M:StardewValley.FarmAnimal.isBaby" />.</remarks>
	public bool isAdult()
	{
		int? adultAge = this.GetAnimalData()?.DaysToMature;
		if (adultAge.HasValue)
		{
			return this.age.Value >= adultAge;
		}
		return true;
	}

	/// <summary>Get whether this farm animal is a baby.</summary>
	/// <remarks>See also <see cref="M:StardewValley.FarmAnimal.isAdult" />.</remarks>
	public bool isBaby()
	{
		return this.age.Value < this.GetAnimalData()?.DaysToMature;
	}

	/// <summary>Get whether this farm animal's produce can be collected using a given tool.</summary>
	/// <param name="tool">The tool to check.</param>
	public bool CanGetProduceWithTool(Tool tool)
	{
		if (tool != null && tool.Name != null)
		{
			return this.GetAnimalData().HarvestTool == tool.Name;
		}
		return false;
	}

	/// <summary>Get the way in which the animal's produce is output.</summary>
	public FarmAnimalHarvestType? GetHarvestType()
	{
		return this.GetAnimalData()?.HarvestType;
	}

	/// <summary>Get whether this farm animal can live in a building.</summary>
	/// <param name="building">The building to check.</param>
	/// <remarks>This doesn't check whether there's room for it in the building; see <see cref="M:StardewValley.AnimalHouse.isFull" /> on <see cref="M:StardewValley.Buildings.Building.GetIndoors" /> for that.</remarks>
	public bool CanLiveIn(Building building)
	{
		BuildingData buildingData = building?.GetData();
		if (buildingData?.ValidOccupantTypes != null && buildingData.ValidOccupantTypes.Contains(this.buildingTypeILiveIn.Value) && !building.isUnderConstruction())
		{
			return building.GetIndoors() is AnimalHouse;
		}
		return false;
	}

	public void warpHome()
	{
		GameLocation insideHome = this.homeInterior;
		if (insideHome != null && insideHome != base.currentLocation)
		{
			if (insideHome.animals.TryAdd(this.myID.Value, this))
			{
				this.setRandomPosition(insideHome);
				this.home.currentOccupants.Value++;
			}
			base.currentLocation?.animals.Remove(this.myID.Value);
			base.controller = null;
			this.isSwimming.Value = false;
			this.hopOffset = Vector2.Zero;
			this._followTarget = null;
			this._followTargetPosition = null;
		}
	}

	/// <summary>If the animal is a baby, instantly age it to adult.</summary>
	/// <param name="random">The RNG with which to select its produce, if applicable.</param>
	public void growFully(Random random = null)
	{
		FarmAnimalData data = this.GetAnimalData();
		if (this.age.Value <= data?.DaysToMature)
		{
			this.age.Value = data.DaysToMature;
			if (data.ProduceOnMature)
			{
				this.currentProduce.Value = this.GetProduceID(random ?? Game1.random);
			}
			this.daysSinceLastLay.Value = 99;
			this.ReloadTextureIfNeeded();
		}
	}

	public override void draw(SpriteBatch b)
	{
		Vector2 offset = new Vector2(0f, base.yJumpOffset);
		Microsoft.Xna.Framework.Rectangle boundingBox = this.GetBoundingBox();
		FarmAnimalData data = this.GetAnimalData();
		bool isActuallySwimming = this.IsActuallySwimming();
		bool baby = this.isBaby();
		FarmAnimalShadowData shadow = data?.GetShadow(baby, isActuallySwimming);
		if (shadow == null || shadow.Visible)
		{
			int shadowOffsetX = (shadow?.Offset?.X).GetValueOrDefault();
			int shadowOffsetY = (shadow?.Offset?.Y).GetValueOrDefault();
			if (isActuallySwimming)
			{
				float shadowScale = shadow?.Scale ?? (baby ? 2.5f : 3.5f);
				Vector2 shadowPos = new Vector2(base.Position.X + (float)shadowOffsetX, base.Position.Y - 24f + (float)shadowOffsetY);
				this.Sprite.drawShadow(b, Game1.GlobalToLocal(Game1.viewport, shadowPos), shadowScale, 0.5f);
				int bobAmount = (int)((Math.Sin(Game1.currentGameTime.TotalGameTime.TotalSeconds * 4.0 + (double)this.bobOffset) + 0.5) * 3.0);
				offset.Y += bobAmount;
			}
			else
			{
				float shadowScale2 = shadow?.Scale ?? (baby ? 3f : 4f);
				Vector2 shadowPos2 = new Vector2(base.Position.X + (float)shadowOffsetX, base.Position.Y - 24f + (float)shadowOffsetY);
				this.Sprite.drawShadow(b, Game1.GlobalToLocal(Game1.viewport, shadowPos2), shadowScale2);
			}
		}
		offset.Y += base.yJumpOffset;
		float layer_depth = ((float)(boundingBox.Center.Y + 4) + base.Position.X / 20000f) / 10000f;
		this.Sprite.draw(b, Utility.snapDrawPosition(Game1.GlobalToLocal(Game1.viewport, base.Position - new Vector2(0f, 24f) + offset)), layer_depth, 0, 0, (this.hitGlowTimer > 0) ? Color.Red : Color.White, this.FacingDirection == 3, 4f);
		if (base.isEmoting)
		{
			int emoteOffsetX = this.Sprite.SpriteWidth / 2 * 4 - 32 + (data?.EmoteOffset.X ?? 0);
			int emoteOffsetY = -64 + (data?.EmoteOffset.Y ?? 0);
			Vector2 emotePosition = Game1.GlobalToLocal(Game1.viewport, new Vector2(base.Position.X + offset.X + (float)emoteOffsetX, base.Position.Y + offset.Y + (float)emoteOffsetY));
			b.Draw(Game1.emoteSpriteSheet, emotePosition, new Microsoft.Xna.Framework.Rectangle(base.CurrentEmoteIndex * 16 % Game1.emoteSpriteSheet.Width, base.CurrentEmoteIndex * 16 / Game1.emoteSpriteSheet.Width * 16, 16, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (float)boundingBox.Bottom / 10000f);
		}
	}

	public virtual void updateWhenNotCurrentLocation(Building currentBuilding, GameTime time, GameLocation environment)
	{
		this.doFarmerPushEvent.Poll();
		this.doBuildingPokeEvent.Poll();
		this.doDiveEvent.Poll();
		AnimatedSprite sprite = this.GetOrLoadTexture();
		if (!Game1.shouldTimePass())
		{
			return;
		}
		this.update(time, environment, this.myID.Value, move: false);
		if (!Game1.IsMasterGame)
		{
			return;
		}
		if (this.hopOffset != Vector2.Zero)
		{
			this.HandleHop();
			return;
		}
		if (currentBuilding != null && Game1.random.NextBool(0.002) && currentBuilding.animalDoorOpen.Value && Game1.timeOfDay < 1630 && !environment.IsRainingHere() && !environment.IsWinterHere() && !environment.farmers.Any())
		{
			GameLocation buildingLocation = currentBuilding.GetParentLocation();
			Microsoft.Xna.Framework.Rectangle doorArea = currentBuilding.getRectForAnimalDoor();
			doorArea.Inflate(-2, -2);
			if (buildingLocation.isCollidingPosition(doorArea, Game1.viewport, isFarmer: false, 0, glider: false, this, pathfinding: false) || buildingLocation.isCollidingPosition(new Microsoft.Xna.Framework.Rectangle(doorArea.X, doorArea.Y + 64, doorArea.Width, doorArea.Height), Game1.viewport, isFarmer: false, 0, glider: false, this, pathfinding: false))
			{
				return;
			}
			buildingLocation.animals.Remove(this.myID.Value);
			currentBuilding.GetIndoors().animals.Remove(this.myID.Value);
			buildingLocation.animals.TryAdd(this.myID.Value, this);
			this.faceDirection(2);
			this.SetMovingDown(b: true);
			base.Position = new Vector2(doorArea.X, doorArea.Y - (sprite.getHeight() * 4 - this.GetBoundingBox().Height) + 32);
			if (FarmAnimal.NumPathfindingThisTick < FarmAnimal.MaxPathfindingPerTick)
			{
				FarmAnimal.NumPathfindingThisTick++;
				base.controller = new PathFindController(this, buildingLocation, grassEndPointFunction, Game1.random.Next(4), behaviorAfterFindingGrassPatch, 200, Point.Zero);
			}
			if (base.controller?.pathToEndPoint == null || base.controller.pathToEndPoint.Count < 3)
			{
				this.SetMovingDown(b: true);
				base.controller = null;
			}
			else
			{
				this.faceDirection(2);
				base.Position = new Vector2(base.controller.pathToEndPoint.Peek().X * 64, base.controller.pathToEndPoint.Peek().Y * 64 - (sprite.getHeight() * 4 - this.GetBoundingBox().Height) + 16);
				if (sprite.SpriteWidth * 4 > 64)
				{
					base.position.X -= 32f;
				}
			}
			this.noWarpTimer = 3000;
			currentBuilding.currentOccupants.Value--;
			if (Utility.isOnScreen(base.TilePoint, 192, buildingLocation))
			{
				buildingLocation.localSound("sandyStep");
			}
			environment.isTileOccupiedByFarmer(base.Tile)?.TemporaryPassableTiles.Add(this.GetBoundingBox());
		}
		this.UpdateRandomMovements();
		this.behaviors(time, environment);
	}

	public static void behaviorAfterFindingGrassPatch(Character c, GameLocation environment)
	{
		if (environment.terrainFeatures.TryGetValue(c.Tile, out var feature) && feature is Grass grass)
		{
			FarmAnimal.reservedGrass.Remove(grass);
		}
		if (((FarmAnimal)c).fullness.Value < 255)
		{
			((FarmAnimal)c).eatGrass(environment);
		}
	}

	public static bool grassEndPointFunction(PathNode currentPoint, Point endPoint, GameLocation location, Character c)
	{
		Vector2 tileLocation = new Vector2(currentPoint.x, currentPoint.y);
		if (location.terrainFeatures.TryGetValue(tileLocation, out var t) && t is Grass grass)
		{
			if (FarmAnimal.reservedGrass.Contains(t))
			{
				return false;
			}
			FarmAnimal.reservedGrass.Add(grass);
			if (c is FarmAnimal animal)
			{
				animal.foundGrass = grass;
			}
			return true;
		}
		return false;
	}

	public virtual void updatePerTenMinutes(int timeOfDay, GameLocation environment)
	{
		if (timeOfDay >= 1800)
		{
			int happinessDrain = this.GetAnimalData()?.HappinessDrain ?? 0;
			int change = 0;
			if (environment.IsOutdoors)
			{
				change = ((timeOfDay > 1900 || environment.IsRainingHere() || environment.IsWinterHere()) ? (-happinessDrain) : happinessDrain);
			}
			else if (this.happiness.Value > 150 && environment.IsWinterHere())
			{
				change = ((environment.numberOfObjectsWithName("Heater") > 0) ? happinessDrain : (-happinessDrain));
			}
			if (change != 0)
			{
				this.happiness.Value = (byte)MathHelper.Clamp(this.happiness.Value + change, 0, 255);
			}
		}
		environment.isTileOccupiedByFarmer(base.Tile)?.TemporaryPassableTiles.Add(this.GetBoundingBox());
	}

	public void eatGrass(GameLocation environment)
	{
		if (environment.terrainFeatures.TryGetValue(base.Tile, out var feature) && feature is Grass grass)
		{
			FarmAnimal.reservedGrass.Remove(grass);
			if (this.foundGrass != null)
			{
				FarmAnimal.reservedGrass.Remove(this.foundGrass);
			}
			this.foundGrass = null;
			this.Eat(environment);
		}
	}

	public virtual void Eat(GameLocation location)
	{
		Vector2 tile = base.Tile;
		this.isEating.Value = true;
		int grassType = 1;
		if (location.terrainFeatures.TryGetValue(tile, out var terrainFeature) && terrainFeature is Grass grass)
		{
			grassType = grass.grassType.Value;
			int grassEatAmount = this.GetAnimalData()?.GrassEatAmount ?? 2;
			if (grass.reduceBy(grassEatAmount, location.Equals(Game1.currentLocation)))
			{
				location.terrainFeatures.Remove(tile);
			}
		}
		this.Sprite.loop = false;
		this.fullness.Value = 255;
		if (this.moodMessage.Value != 5 && this.moodMessage.Value != 6 && !location.IsRainingHere())
		{
			this.happiness.Value = 255;
			this.friendshipTowardFarmer.Value = Math.Min(1000, this.friendshipTowardFarmer.Value + ((grassType == 7) ? 16 : 8));
		}
	}

	public virtual bool behaviors(GameTime time, GameLocation location)
	{
		if (!Game1.IsMasterGame)
		{
			return false;
		}
		Building home = this.home;
		if (home == null)
		{
			return false;
		}
		if (this.isBaby() && this.CanFollowAdult())
		{
			this._nextFollowTargetScan -= (float)time.ElapsedGameTime.TotalSeconds;
			if (this._nextFollowTargetScan < 0f)
			{
				this._nextFollowTargetScan = Utility.RandomFloat(1f, 3f);
				if (base.controller != null || !location.IsOutdoors)
				{
					this._followTarget = null;
					this._followTargetPosition = null;
				}
				else
				{
					if (this._followTarget != null)
					{
						if (!FarmAnimal.GetFollowRange(this._followTarget).Contains(this._followTargetPosition.Value))
						{
							this.GetNewFollowPosition();
						}
						return false;
					}
					if (location.IsOutdoors)
					{
						foreach (FarmAnimal animal in location.animals.Values)
						{
							if (!animal.isBaby() && animal.type.Value == this.type.Value && FarmAnimal.GetFollowRange(animal, 4).Contains(base.StandingPixel))
							{
								this._followTarget = animal;
								this.GetNewFollowPosition();
								return false;
							}
						}
					}
				}
			}
		}
		if (this.isEating.Value)
		{
			if (home != null && home.getRectForAnimalDoor().Intersects(this.GetBoundingBox()))
			{
				FarmAnimal.behaviorAfterFindingGrassPatch(this, location);
				this.isEating.Value = false;
				this.Halt();
				return false;
			}
			FarmAnimalData animalData = this.GetAnimalData();
			int eatFrame = 16;
			if (!this.Sprite.textureUsesFlippedRightForLeft)
			{
				eatFrame += 4;
			}
			if (animalData?.UseDoubleUniqueAnimationFrames ?? false)
			{
				eatFrame += 4;
			}
			if (this.Sprite.Animate(time, eatFrame, 4, 100f))
			{
				this.isEating.Value = false;
				this.Sprite.loop = true;
				this.Sprite.currentFrame = 0;
				this.faceDirection(2);
			}
			return true;
		}
		if (base.controller != null)
		{
			return true;
		}
		if (!this.isSwimming.Value && location.IsOutdoors && this.fullness.Value < 195 && Game1.random.NextDouble() < 0.002 && FarmAnimal.NumPathfindingThisTick < FarmAnimal.MaxPathfindingPerTick)
		{
			FarmAnimal.NumPathfindingThisTick++;
			base.controller = new PathFindController(this, location, grassEndPointFunction, -1, behaviorAfterFindingGrassPatch, 200, Point.Zero);
			this._followTarget = null;
			this._followTargetPosition = null;
		}
		if (Game1.timeOfDay >= 1700 && location.IsOutdoors && base.controller == null && Game1.random.NextDouble() < 0.002 && home.animalDoorOpen.Value)
		{
			if (!location.farmers.Any())
			{
				GameLocation insideHome = home.GetIndoors();
				location.animals.Remove(this.myID.Value);
				insideHome.animals.TryAdd(this.myID.Value, this);
				this.setRandomPosition(insideHome);
				this.faceDirection(Game1.random.Next(4));
				base.controller = null;
				return true;
			}
			if (FarmAnimal.NumPathfindingThisTick < FarmAnimal.MaxPathfindingPerTick)
			{
				FarmAnimal.NumPathfindingThisTick++;
				base.controller = new PathFindController(this, location, PathFindController.isAtEndPoint, 0, null, 200, new Point(home.tileX.Value + home.animalDoor.X, home.tileY.Value + home.animalDoor.Y));
				this._followTarget = null;
				this._followTargetPosition = null;
			}
		}
		if (location.IsOutdoors && !location.IsRainingHere() && !location.IsWinterHere() && this.currentProduce.Value != null && this.isAdult() && this.GetHarvestType() == FarmAnimalHarvestType.DigUp && Game1.random.NextDouble() < 0.0002)
		{
			Object produce = ItemRegistry.Create<Object>(this.currentProduce.Value);
			Microsoft.Xna.Framework.Rectangle rect = this.GetBoundingBox();
			for (int i = 0; i < 4; i++)
			{
				Vector2 v = Utility.getCornersOfThisRectangle(ref rect, i);
				Vector2 vec = new Vector2((int)(v.X / 64f), (int)(v.Y / 64f));
				if (location.terrainFeatures.ContainsKey(vec) || location.objects.ContainsKey(vec))
				{
					return false;
				}
			}
			if (Game1.player.currentLocation.Equals(location))
			{
				DelayedAction.playSoundAfterDelay("dirtyHit", 450);
				DelayedAction.playSoundAfterDelay("dirtyHit", 900);
				DelayedAction.playSoundAfterDelay("dirtyHit", 1350);
			}
			if (location.Equals(Game1.currentLocation))
			{
				switch (this.FacingDirection)
				{
				case 2:
					this.Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
					{
						new FarmerSprite.AnimationFrame(1, 250),
						new FarmerSprite.AnimationFrame(3, 250),
						new FarmerSprite.AnimationFrame(1, 250),
						new FarmerSprite.AnimationFrame(3, 250),
						new FarmerSprite.AnimationFrame(1, 250),
						new FarmerSprite.AnimationFrame(3, 250, secondaryArm: false, flip: false, delegate
						{
							this.DigUpProduce(location, produce);
						})
					});
					break;
				case 1:
					this.Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
					{
						new FarmerSprite.AnimationFrame(5, 250),
						new FarmerSprite.AnimationFrame(7, 250),
						new FarmerSprite.AnimationFrame(5, 250),
						new FarmerSprite.AnimationFrame(7, 250),
						new FarmerSprite.AnimationFrame(5, 250),
						new FarmerSprite.AnimationFrame(7, 250, secondaryArm: false, flip: false, delegate
						{
							this.DigUpProduce(location, produce);
						})
					});
					break;
				case 0:
					this.Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
					{
						new FarmerSprite.AnimationFrame(9, 250),
						new FarmerSprite.AnimationFrame(11, 250),
						new FarmerSprite.AnimationFrame(9, 250),
						new FarmerSprite.AnimationFrame(11, 250),
						new FarmerSprite.AnimationFrame(9, 250),
						new FarmerSprite.AnimationFrame(11, 250, secondaryArm: false, flip: false, delegate
						{
							this.DigUpProduce(location, produce);
						})
					});
					break;
				case 3:
					this.Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
					{
						new FarmerSprite.AnimationFrame(5, 250, secondaryArm: false, flip: true),
						new FarmerSprite.AnimationFrame(7, 250, secondaryArm: false, flip: true),
						new FarmerSprite.AnimationFrame(5, 250, secondaryArm: false, flip: true),
						new FarmerSprite.AnimationFrame(7, 250, secondaryArm: false, flip: true),
						new FarmerSprite.AnimationFrame(5, 250, secondaryArm: false, flip: true),
						new FarmerSprite.AnimationFrame(7, 250, secondaryArm: false, flip: true, delegate
						{
							this.DigUpProduce(location, produce);
						})
					});
					break;
				}
				this.Sprite.loop = false;
			}
			else
			{
				this.DigUpProduce(location, produce);
			}
		}
		return false;
	}

	public virtual void DigUpProduce(GameLocation location, Object produce)
	{
		Random r = Utility.CreateRandom((double)this.myID.Value / 2.0, Game1.stats.DaysPlayed, Game1.timeOfDay);
		bool success = false;
		if (produce.QualifiedItemId == "(O)430" && r.NextDouble() < 0.002)
		{
			RockCrab crab = new RockCrab(base.Tile, "Truffle Crab");
			Vector2 v = Utility.recursiveFindOpenTileForCharacter(crab, location, base.Tile, 50, allowOffMap: false);
			if (v != Vector2.Zero)
			{
				crab.setTileLocation(v);
				location.addCharacter(crab);
				success = true;
			}
		}
		if (!success && Utility.spawnObjectAround(Utility.getTranslatedVector2(base.Tile, this.FacingDirection, 1f), produce, base.currentLocation) && produce.QualifiedItemId == "(O)430")
		{
			Game1.stats.TrufflesFound++;
		}
		if (!r.NextBool((double)this.friendshipTowardFarmer.Value / 1500.0))
		{
			this.currentProduce.Value = null;
		}
	}

	public static Microsoft.Xna.Framework.Rectangle GetFollowRange(FarmAnimal animal, int distance = 2)
	{
		Point standingPixel = animal.StandingPixel;
		return new Microsoft.Xna.Framework.Rectangle(standingPixel.X - distance * 64, standingPixel.Y - distance * 64, distance * 64 * 2, 64 * distance * 2);
	}

	public virtual void GetNewFollowPosition()
	{
		if (this._followTarget == null)
		{
			this._followTargetPosition = null;
		}
		else if (this._followTarget.isMoving() && this._followTarget.IsActuallySwimming())
		{
			this._followTargetPosition = Utility.Vector2ToPoint(Utility.getRandomPositionInThisRectangle(FarmAnimal.GetFollowRange(this._followTarget, 1), Game1.random));
		}
		else
		{
			this._followTargetPosition = Utility.Vector2ToPoint(Utility.getRandomPositionInThisRectangle(FarmAnimal.GetFollowRange(this._followTarget), Game1.random));
		}
	}

	public void hitWithWeapon(MeleeWeapon t)
	{
	}

	public void makeSound()
	{
		if (base.currentLocation == Game1.currentLocation && !Game1.options.muteAnimalSounds)
		{
			string soundToPlay = this.GetSoundId();
			if (soundToPlay != null)
			{
				Game1.playSound(soundToPlay, 1200 + Game1.random.Next(-200, 201));
			}
		}
	}

	/// <summary>Get the sound ID produced by the animal (e.g. when pet).</summary>
	public string GetSoundId()
	{
		FarmAnimalData data = this.GetAnimalData();
		if (!this.isBaby() || data == null || data.BabySound == null)
		{
			return data?.Sound;
		}
		return data.BabySound;
	}

	public virtual bool CanHavePregnancy()
	{
		return this.GetAnimalData()?.CanGetPregnant ?? false;
	}

	public virtual bool SleepIfNecessary()
	{
		if (Game1.timeOfDay >= 2000)
		{
			this.isSwimming.Value = false;
			this.hopOffset = Vector2.Zero;
			this._followTarget = null;
			this._followTargetPosition = null;
			if (this.isMoving())
			{
				this.Halt();
			}
			FarmAnimalData data = this.GetAnimalData();
			this.Sprite.currentFrame = data?.SleepFrame ?? 12;
			this.FacingDirection = 2;
			this.Sprite.UpdateSourceRect();
			return true;
		}
		return false;
	}

	public override bool isMoving()
	{
		if (this._swimmingVelocity != Vector2.Zero)
		{
			return true;
		}
		if (!this.IsActuallySwimming() && this.uniqueFrameAccumulator != -1)
		{
			return false;
		}
		return base.isMoving();
	}

	public virtual bool updateWhenCurrentLocation(GameTime time, GameLocation location)
	{
		if (!Game1.shouldTimePass())
		{
			return false;
		}
		if (this.health.Value <= 0)
		{
			return true;
		}
		AnimatedSprite sprite = this.GetOrLoadTexture();
		this.doBuildingPokeEvent.Poll();
		this.doDiveEvent.Poll();
		if (this.IsActuallySwimming())
		{
			int time_multiplier = 1;
			if (this.isMoving())
			{
				time_multiplier = 4;
			}
			this.nextRipple -= (int)time.ElapsedGameTime.TotalMilliseconds * time_multiplier;
			if (this.nextRipple <= 0)
			{
				this.nextRipple = 2000;
				float scale = 1f;
				if (this.isBaby())
				{
					scale = 0.65f;
				}
				Point standingPixel = base.StandingPixel;
				float x_offset = base.Position.X - (float)standingPixel.X;
				TemporaryAnimatedSprite ripple = new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(0, 0, 64, 64), this.isMoving() ? 75f : 150f, 8, 0, new Vector2((float)standingPixel.X + x_offset * scale, (float)standingPixel.Y - 32f * scale), flicker: false, Game1.random.NextBool(), 0.01f, 0.01f, Color.White * 0.75f, scale, 0f, 0f, 0f);
				Vector2 offset = Utility.PointToVector2(Utility.getTranslatedPoint(default(Point), this.FacingDirection, -1));
				ripple.motion = offset * 0.25f;
				location.TemporarySprites.Add(ripple);
			}
		}
		if (this.hitGlowTimer > 0)
		{
			this.hitGlowTimer -= time.ElapsedGameTime.Milliseconds;
		}
		if (sprite.CurrentAnimation != null)
		{
			if (sprite.animateOnce(time))
			{
				sprite.CurrentAnimation = null;
			}
			return false;
		}
		this.update(time, location, this.myID.Value, move: false);
		if (this.hopOffset != Vector2.Zero)
		{
			sprite.UpdateSourceRect();
			this.HandleHop();
			return false;
		}
		if (Game1.IsMasterGame && this.behaviors(time, location))
		{
			return false;
		}
		if (sprite.CurrentAnimation != null)
		{
			return false;
		}
		PathFindController pathFindController = base.controller;
		if (pathFindController != null && pathFindController.timerSinceLastCheckPoint > 10000)
		{
			base.controller = null;
			this.Halt();
		}
		if (Game1.IsMasterGame)
		{
			if (!this.IsHome && this.noWarpTimer <= 0)
			{
				GameLocation insideHome = this.homeInterior;
				if (insideHome != null)
				{
					Microsoft.Xna.Framework.Rectangle bounds = this.GetBoundingBox();
					if (this.home.getRectForAnimalDoor().Contains(bounds.Center.X, bounds.Top))
					{
						if (Utility.isOnScreen(base.TilePoint, 192, location))
						{
							location.localSound("dwoop");
						}
						location.animals.Remove(this.myID.Value);
						insideHome.animals[this.myID.Value] = this;
						this.setRandomPosition(insideHome);
						this.faceDirection(Game1.random.Next(4));
						base.controller = null;
						return true;
					}
				}
			}
			this.noWarpTimer = Math.Max(0, this.noWarpTimer - time.ElapsedGameTime.Milliseconds);
		}
		if (this.pauseTimer > 0)
		{
			this.pauseTimer -= time.ElapsedGameTime.Milliseconds;
		}
		if (this.SleepIfNecessary())
		{
			if (!base.isEmoting && Game1.random.NextDouble() < 0.002)
			{
				base.doEmote(24);
			}
		}
		else if (this.pauseTimer <= 0 && Game1.random.NextDouble() < 0.001 && this.isAdult() && Game1.gameMode == 3 && Utility.isOnScreen(base.Position, 192))
		{
			this.makeSound();
		}
		if (Game1.IsMasterGame)
		{
			this.UpdateRandomMovements();
			if (this.uniqueFrameAccumulator != -1 && this._followTarget != null && !FarmAnimal.GetFollowRange(this._followTarget, 1).Contains(base.StandingPixel))
			{
				this.uniqueFrameAccumulator = -1;
			}
			if (this.uniqueFrameAccumulator != -1)
			{
				this.uniqueFrameAccumulator += time.ElapsedGameTime.Milliseconds;
				if (this.uniqueFrameAccumulator > 500)
				{
					if (this.GetAnimalData()?.UseDoubleUniqueAnimationFrames ?? false)
					{
						sprite.currentFrame = sprite.currentFrame + 1 - sprite.currentFrame % 2 * 2;
					}
					else if (sprite.currentFrame > 12)
					{
						sprite.currentFrame = (sprite.currentFrame - 13) * 4;
					}
					else
					{
						switch (this.FacingDirection)
						{
						case 0:
							sprite.currentFrame = 15;
							break;
						case 1:
							sprite.currentFrame = 14;
							break;
						case 2:
							sprite.currentFrame = 13;
							break;
						case 3:
							sprite.currentFrame = 14;
							break;
						}
					}
					this.uniqueFrameAccumulator = 0;
					if (Game1.random.NextDouble() < 0.4)
					{
						this.uniqueFrameAccumulator = -1;
					}
				}
				if (this.IsActuallySwimming())
				{
					this.MovePosition(time, Game1.viewport, location);
				}
			}
			else
			{
				this.MovePosition(time, Game1.viewport, location);
			}
		}
		if (this.IsActuallySwimming())
		{
			FarmAnimalData data = this.GetAnimalData();
			sprite.UpdateSourceRect();
			Microsoft.Xna.Framework.Rectangle source_rect = sprite.SourceRect;
			source_rect.Offset(data?.SwimOffset ?? new Point(0, 112));
			sprite.SourceRect = source_rect;
		}
		return false;
	}

	public virtual void UpdateRandomMovements()
	{
		if (!Game1.IsMasterGame || Game1.timeOfDay >= 2000 || this.pauseTimer > 0)
		{
			return;
		}
		if (this.fullness.Value < 255 && this.IsActuallySwimming() && Game1.random.NextDouble() < 0.002 && !this.isEating.Value)
		{
			this.Eat(base.currentLocation);
		}
		if (Game1.random.NextDouble() < 0.007 && this.uniqueFrameAccumulator == -1)
		{
			int newDirection = Game1.random.Next(5);
			if (newDirection != (this.FacingDirection + 2) % 4 || this.IsActuallySwimming())
			{
				if (newDirection < 4)
				{
					int oldDirection = this.FacingDirection;
					this.faceDirection(newDirection);
					if (!base.currentLocation.isOutdoors.Value && base.currentLocation.isCollidingPosition(this.nextPosition(newDirection), Game1.viewport, this))
					{
						this.faceDirection(oldDirection);
						return;
					}
				}
				switch (newDirection)
				{
				case 0:
					this.SetMovingUp(b: true);
					break;
				case 1:
					this.SetMovingRight(b: true);
					break;
				case 2:
					this.SetMovingDown(b: true);
					break;
				case 3:
					this.SetMovingLeft(b: true);
					break;
				default:
					this.Halt();
					this.Sprite.StopAnimation();
					break;
				}
			}
			else if (this.noWarpTimer <= 0)
			{
				this.Halt();
				this.Sprite.StopAnimation();
			}
		}
		if (!this.isMoving() || !(Game1.random.NextDouble() < 0.014) || this.uniqueFrameAccumulator != -1)
		{
			return;
		}
		this.Halt();
		this.Sprite.StopAnimation();
		if (Game1.random.NextDouble() < 0.75)
		{
			FarmAnimalData data = this.GetAnimalData();
			this.uniqueFrameAccumulator = 0;
			if (data?.UseDoubleUniqueAnimationFrames ?? false)
			{
				switch (this.FacingDirection)
				{
				case 0:
					this.Sprite.currentFrame = 20;
					break;
				case 1:
					this.Sprite.currentFrame = 18;
					break;
				case 2:
					this.Sprite.currentFrame = 16;
					break;
				case 3:
					this.Sprite.currentFrame = 22;
					break;
				}
			}
			else
			{
				switch (this.FacingDirection)
				{
				case 0:
					this.Sprite.currentFrame = 15;
					break;
				case 1:
					this.Sprite.currentFrame = 14;
					break;
				case 2:
					this.Sprite.currentFrame = 13;
					break;
				case 3:
					this.Sprite.currentFrame = ((data?.UseFlippedRightForLeft ?? false) ? 14 : 12);
					break;
				}
			}
			this.uniqueFrameAccumulator = 0;
		}
		this.Sprite.UpdateSourceRect();
	}

	public virtual bool CanSwim()
	{
		return this.GetAnimalData()?.CanSwim ?? false;
	}

	public virtual bool CanFollowAdult()
	{
		if (this.isBaby())
		{
			return this.GetAnimalData()?.BabiesFollowAdults ?? false;
		}
		return false;
	}

	public override bool shouldCollideWithBuildingLayer(GameLocation location)
	{
		return true;
	}

	public virtual void HandleHop()
	{
		int hop_speed = 4;
		if (this.hopOffset != Vector2.Zero)
		{
			if (this.hopOffset.X != 0f)
			{
				int move_amount = (int)Math.Min(hop_speed, Math.Abs(this.hopOffset.X));
				base.Position += new Vector2(move_amount * Math.Sign(this.hopOffset.X), 0f);
				this.hopOffset.X = Utility.MoveTowards(this.hopOffset.X, 0f, move_amount);
			}
			if (this.hopOffset.Y != 0f)
			{
				int move_amount2 = (int)Math.Min(hop_speed, Math.Abs(this.hopOffset.Y));
				base.Position += new Vector2(0f, move_amount2 * Math.Sign(this.hopOffset.Y));
				this.hopOffset.Y = Utility.MoveTowards(this.hopOffset.Y, 0f, move_amount2);
			}
			if (this.hopOffset == Vector2.Zero && this.isSwimming.Value)
			{
				this.Splash();
				this._swimmingVelocity = Utility.getTranslatedVector2(Vector2.Zero, this.FacingDirection, base.speed);
				base.Position = new Vector2((int)Math.Round(base.Position.X), (int)Math.Round(base.Position.Y));
			}
		}
	}

	public override void MovePosition(GameTime time, xTile.Dimensions.Rectangle viewport, GameLocation currentLocation)
	{
		if (this.pauseTimer > 0 || Game1.IsClient)
		{
			return;
		}
		Location next_tile = base.nextPositionTile();
		if (!currentLocation.isTileOnMap(new Vector2(next_tile.X, next_tile.Y)))
		{
			this.FacingDirection = Utility.GetOppositeFacingDirection(this.FacingDirection);
			base.moveUp = base.facingDirection.Value == 0;
			base.moveLeft = base.facingDirection.Value == 3;
			base.moveDown = base.facingDirection.Value == 2;
			base.moveRight = base.facingDirection.Value == 1;
			this._followTarget = null;
			this._followTargetPosition = null;
			this._swimmingVelocity = Vector2.Zero;
			return;
		}
		if (this._followTarget != null && (this._followTarget.currentLocation != currentLocation || this._followTarget.health.Value <= 0))
		{
			this._followTarget = null;
			this._followTargetPosition = null;
		}
		if (this._followTargetPosition.HasValue)
		{
			Point standingPixel = base.StandingPixel;
			Point targetPosition = this._followTargetPosition.Value;
			Point offset = new Point(standingPixel.X - targetPosition.X, standingPixel.Y - targetPosition.Y);
			if (Math.Abs(offset.X) <= 64 || Math.Abs(offset.Y) <= 64)
			{
				base.moveDown = false;
				base.moveUp = false;
				base.moveLeft = false;
				base.moveRight = false;
				this.GetNewFollowPosition();
			}
			else if (this.nextFollowDirectionChange >= 0)
			{
				this.nextFollowDirectionChange -= (int)time.ElapsedGameTime.TotalMilliseconds;
			}
			else
			{
				if (this.IsActuallySwimming())
				{
					this.nextFollowDirectionChange = 100;
				}
				else
				{
					this.nextFollowDirectionChange = 500;
				}
				base.moveDown = false;
				base.moveUp = false;
				base.moveLeft = false;
				base.moveRight = false;
				if (Math.Abs(standingPixel.X - this._followTargetPosition.Value.X) < Math.Abs(standingPixel.Y - this._followTargetPosition.Value.Y))
				{
					if (standingPixel.Y > this._followTargetPosition.Value.Y)
					{
						base.moveUp = true;
					}
					else if (standingPixel.Y < this._followTargetPosition.Value.Y)
					{
						base.moveDown = true;
					}
				}
				else if (standingPixel.X < this._followTargetPosition.Value.X)
				{
					base.moveRight = true;
				}
				else if (standingPixel.X > this._followTargetPosition.Value.X)
				{
					base.moveLeft = true;
				}
			}
		}
		if (this.IsActuallySwimming())
		{
			Vector2 desired_movement = default(Vector2);
			if (!this.isEating.Value)
			{
				if (base.moveUp)
				{
					desired_movement.Y = -base.speed;
				}
				else if (base.moveDown)
				{
					desired_movement.Y = base.speed;
				}
				if (base.moveLeft)
				{
					desired_movement.X = -base.speed;
				}
				else if (base.moveRight)
				{
					desired_movement.X = base.speed;
				}
			}
			this._swimmingVelocity = new Vector2(Utility.MoveTowards(this._swimmingVelocity.X, desired_movement.X, 0.025f), Utility.MoveTowards(this._swimmingVelocity.Y, desired_movement.Y, 0.025f));
			Vector2 old_position = base.Position;
			base.Position += this._swimmingVelocity;
			Microsoft.Xna.Framework.Rectangle next_bounds = this.GetBoundingBox();
			base.Position = old_position;
			int moving_direction = -1;
			if (!currentLocation.isCollidingPosition(next_bounds, Game1.viewport, isFarmer: false, 0, glider: false, this, pathfinding: false))
			{
				base.Position += this._swimmingVelocity;
				if (Math.Abs(this._swimmingVelocity.X) > Math.Abs(this._swimmingVelocity.Y))
				{
					if (this._swimmingVelocity.X < 0f)
					{
						moving_direction = 3;
					}
					else if (this._swimmingVelocity.X > 0f)
					{
						moving_direction = 1;
					}
				}
				else if (this._swimmingVelocity.Y < 0f)
				{
					moving_direction = 0;
				}
				else if (this._swimmingVelocity.Y > 0f)
				{
					moving_direction = 2;
				}
				switch (moving_direction)
				{
				case 0:
					this.Sprite.AnimateUp(time);
					this.faceDirection(0);
					break;
				case 3:
					this.Sprite.AnimateRight(time);
					this.FacingDirection = 3;
					break;
				case 1:
					this.Sprite.AnimateRight(time);
					this.faceDirection(1);
					break;
				case 2:
					this.Sprite.AnimateDown(time);
					this.faceDirection(2);
					break;
				}
			}
			else if (!this.HandleCollision(next_bounds))
			{
				this.Halt();
				this.Sprite.StopAnimation();
				this._swimmingVelocity *= -1f;
			}
		}
		else if (base.moveUp)
		{
			if (!currentLocation.isCollidingPosition(this.nextPosition(0), Game1.viewport, isFarmer: false, 0, glider: false, this, pathfinding: false))
			{
				base.position.Y -= base.speed;
				this.Sprite.AnimateUp(time);
			}
			else if (!this.HandleCollision(this.nextPosition(0)))
			{
				this.Halt();
				this.Sprite.StopAnimation();
				if (Game1.random.NextDouble() < 0.6 || this.IsActuallySwimming())
				{
					this.SetMovingDown(b: true);
				}
			}
			this.faceDirection(0);
		}
		else if (base.moveRight)
		{
			if (!currentLocation.isCollidingPosition(this.nextPosition(1), Game1.viewport, isFarmer: false, 0, glider: false, this))
			{
				base.position.X += base.speed;
				this.Sprite.AnimateRight(time);
			}
			else if (!this.HandleCollision(this.nextPosition(1)))
			{
				this.Halt();
				this.Sprite.StopAnimation();
				if (Game1.random.NextDouble() < 0.6 || this.IsActuallySwimming())
				{
					this.SetMovingLeft(b: true);
				}
			}
			this.faceDirection(1);
		}
		else if (base.moveDown)
		{
			if (!currentLocation.isCollidingPosition(this.nextPosition(2), Game1.viewport, isFarmer: false, 0, glider: false, this))
			{
				base.position.Y += base.speed;
				this.Sprite.AnimateDown(time);
			}
			else if (!this.HandleCollision(this.nextPosition(2)))
			{
				this.Halt();
				this.Sprite.StopAnimation();
				if (Game1.random.NextDouble() < 0.6 || this.IsActuallySwimming())
				{
					this.SetMovingUp(b: true);
				}
			}
			this.faceDirection(2);
		}
		else
		{
			if (!base.moveLeft)
			{
				return;
			}
			if (!currentLocation.isCollidingPosition(this.nextPosition(3), Game1.viewport, isFarmer: false, 0, glider: false, this))
			{
				base.position.X -= base.speed;
				this.Sprite.AnimateRight(time);
			}
			else if (!this.HandleCollision(this.nextPosition(3)))
			{
				this.Halt();
				this.Sprite.StopAnimation();
				if (Game1.random.NextDouble() < 0.6 || this.IsActuallySwimming())
				{
					this.SetMovingRight(b: true);
				}
			}
			this.FacingDirection = 3;
		}
	}

	public virtual bool HandleCollision(Microsoft.Xna.Framework.Rectangle next_position)
	{
		if (this._followTarget != null)
		{
			this._followTarget = null;
			this._followTargetPosition = null;
		}
		if (base.currentLocation.IsOutdoors && this.CanSwim() && (this.isSwimming.Value || base.controller == null) && this.wasPet.Value && this.hopOffset == Vector2.Zero)
		{
			base.Position = new Vector2((int)Math.Round(base.Position.X), (int)Math.Round(base.Position.Y));
			Microsoft.Xna.Framework.Rectangle current_position = this.GetBoundingBox();
			Vector2 offset = Utility.getTranslatedVector2(Vector2.Zero, this.FacingDirection, 1f);
			if (offset != Vector2.Zero)
			{
				Point hop_over_tile = base.TilePoint;
				hop_over_tile.X += (int)offset.X;
				hop_over_tile.Y += (int)offset.Y;
				offset *= 128f;
				Microsoft.Xna.Framework.Rectangle hop_destination = current_position;
				hop_destination.Offset(Utility.Vector2ToPoint(offset));
				Point hop_tile = new Point(hop_destination.X / 64, hop_destination.Y / 64);
				if (base.currentLocation.isWaterTile(hop_over_tile.X, hop_over_tile.Y) && base.currentLocation.doesTileHaveProperty(hop_over_tile.X, hop_over_tile.Y, "Passable", "Buildings") == null && !base.currentLocation.isCollidingPosition(hop_destination, Game1.viewport, isFarmer: false, 0, glider: false, this) && base.currentLocation.isOpenWater(hop_tile.X, hop_tile.Y) != this.isSwimming.Value)
				{
					this.isSwimming.Value = !this.isSwimming.Value;
					if (!this.isSwimming.Value)
					{
						this.Splash();
					}
					this.hopOffset = offset;
					this.pauseTimer = 0;
					this.doDiveEvent.Fire();
				}
				return true;
			}
		}
		return false;
	}

	public virtual bool IsActuallySwimming()
	{
		if (this.isSwimming.Value)
		{
			return this.hopOffset == Vector2.Zero;
		}
		return false;
	}

	public virtual void Splash()
	{
		if (Utility.isOnScreen(base.TilePoint, 192, base.currentLocation))
		{
			base.currentLocation.playSound("dropItemInWater");
		}
		Game1.multiplayer.broadcastSprites(base.currentLocation, new TemporaryAnimatedSprite(28, 100f, 2, 1, base.getStandingPosition() + new Vector2(-0.5f, -0.5f) * 64f, flicker: false, flipped: false)
		{
			delayBeforeAnimationStart = 0,
			layerDepth = (float)base.StandingPixel.Y / 10000f
		});
	}

	public override void animateInFacingDirection(GameTime time)
	{
		if (this.FacingDirection == 3)
		{
			this.Sprite.AnimateRight(time);
		}
		else
		{
			base.animateInFacingDirection(time);
		}
	}

	/// <summary>Log warnings if the farm animal's sprite is incorrectly sized, which would otherwise lead to hard-to-diagnose issues like animals freezing.</summary>
	private void ValidateSpritesheetSize()
	{
		int expectedRows = 5 + ((!this.Sprite.textureUsesFlippedRightForLeft) ? 1 : 0) + ((this.GetAnimalData()?.UseDoubleUniqueAnimationFrames ?? false) ? 1 : 0);
		if (this.Sprite.Texture.Height < expectedRows * this.Sprite.SpriteHeight)
		{
			Game1.log.Warn($"Farm animal '{this.type.Value}' has sprite height {this.Sprite.Texture.Height}px, but expected at least {expectedRows * this.Sprite.SpriteHeight}px based on its data. This may cause issues like frozen animations.");
		}
		if (this.Sprite.Texture.Width != 4 * this.Sprite.SpriteWidth)
		{
			Game1.log.Warn($"Farm animal '{this.type.Value}' has sprite width {this.Sprite.Texture.Width}px, but it should be exactly {4 * this.Sprite.SpriteWidth}px.");
		}
	}
}
