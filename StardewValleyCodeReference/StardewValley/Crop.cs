using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Characters;
using StardewValley.Extensions;
using StardewValley.GameData.Crops;
using StardewValley.GameData.GiantCrops;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Locations;
using StardewValley.Mods;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;

namespace StardewValley;

public class Crop : INetObject<NetFields>, IHaveModData
{
	public const string mixedSeedsId = "770";

	public const string mixedSeedsQId = "(O)770";

	public const int seedPhase = 0;

	public const int rowOfWildSeeds = 23;

	public const int finalPhaseLength = 99999;

	public const int forageCrop_springOnion = 1;

	public const string forageCrop_springOnionID = "1";

	public const int forageCrop_ginger = 2;

	public const string forageCrop_gingerID = "2";

	/// <summary>The <see cref="F:StardewValley.Item.specialVariable" /> value which indicates the object was spawned by a farmed forage crop.</summary>
	public const int specialVariable_farmedForageCrop = 724519;

	/// <summary>The backing field for <see cref="P:StardewValley.Crop.currentLocation" />.</summary>
	private GameLocation currentLocationImpl;

	/// <summary>The number of days in each visual step of growth before the crop is harvestable. The last entry in this list is <see cref="F:StardewValley.Crop.finalPhaseLength" />.</summary>
	public readonly NetIntList phaseDays = new NetIntList();

	/// <summary>The index of this crop in the spritesheet texture (one crop per row).</summary>
	[XmlElement("rowInSpriteSheet")]
	public readonly NetInt rowInSpriteSheet = new NetInt();

	[XmlElement("phaseToShow")]
	public readonly NetInt phaseToShow = new NetInt(-1);

	[XmlElement("currentPhase")]
	public readonly NetInt currentPhase = new NetInt();

	/// <summary>The unqualified item ID produced when this crop is harvested.</summary>
	[XmlElement("indexOfHarvest")]
	public readonly NetString indexOfHarvest = new NetString();

	[XmlElement("dayOfCurrentPhase")]
	public readonly NetInt dayOfCurrentPhase = new NetInt();

	/// <summary>The seed ID, if this is a forage or wild seed crop.</summary>
	[XmlElement("whichForageCrop")]
	public readonly NetString whichForageCrop = new NetString();

	/// <summary>If set, the qualified object ID to spawn on the crop's tile when it's full-grown. The crop will be removed when the object is spawned.</summary>
	[XmlElement("overrideHarvestItemId")]
	public readonly NetString replaceWithObjectOnFullGrown = new NetString();

	/// <summary>The tint colors that can be applied to the crop sprite, if any.</summary>
	[XmlElement("tintColor")]
	public readonly NetColor tintColor = new NetColor();

	[XmlElement("flip")]
	public readonly NetBool flip = new NetBool();

	[XmlElement("fullGrown")]
	public readonly NetBool fullyGrown = new NetBool();

	/// <summary>Whether this is a raised crop on a trellis that can't be walked through.</summary>
	[XmlElement("raisedSeeds")]
	public readonly NetBool raisedSeeds = new NetBool();

	/// <summary>Whether to apply the <see cref="F:StardewValley.Crop.tintColor" />.</summary>
	[XmlElement("programColored")]
	public readonly NetBool programColored = new NetBool();

	[XmlElement("dead")]
	public readonly NetBool dead = new NetBool();

	[XmlElement("forageCrop")]
	public readonly NetBool forageCrop = new NetBool();

	/// <summary>The unqualified seed ID, if this is a regular crop.</summary>
	[XmlElement("seedIndex")]
	public readonly NetString netSeedIndex = new NetString();

	/// <summary>The asset name for the crop texture under the game's <c>Content</c> folder, or null to use <see cref="F:StardewValley.Game1.cropSpriteSheetName" />.</summary>
	[XmlElement("overrideTexturePath")]
	public readonly NetString overrideTexturePath = new NetString();

	protected Texture2D _drawnTexture;

	protected bool? _isErrorCrop;

	[XmlIgnore]
	public Vector2 drawPosition;

	[XmlIgnore]
	public Vector2 tilePosition;

	[XmlIgnore]
	public float layerDepth;

	[XmlIgnore]
	public float coloredLayerDepth;

	[XmlIgnore]
	public Rectangle sourceRect;

	[XmlIgnore]
	public Rectangle coloredSourceRect;

	private static Vector2 origin = new Vector2(8f, 24f);

	private static Vector2 smallestTileSizeOrigin = new Vector2(8f, 8f);

	/// <summary>The location containing the crop.</summary>
	[XmlIgnore]
	public GameLocation currentLocation
	{
		get
		{
			return this.currentLocationImpl;
		}
		set
		{
			if (value != this.currentLocationImpl)
			{
				this.currentLocationImpl = value;
				this.updateDrawMath(this.tilePosition);
			}
		}
	}

	/// <summary>The dirt which contains this crop.</summary>
	[XmlIgnore]
	public HoeDirt Dirt { get; set; }

	[XmlIgnore]
	public Texture2D DrawnCropTexture
	{
		get
		{
			if (this.dead.Value)
			{
				return Game1.cropSpriteSheet;
			}
			if (this._drawnTexture == null)
			{
				if (this.overrideTexturePath.Value == null)
				{
					this.overrideTexturePath.Value = this.GetData()?.GetCustomTextureName("TileSheets\\crops");
				}
				this._drawnTexture = null;
				if (this.overrideTexturePath.Value != null)
				{
					try
					{
						this._drawnTexture = Game1.content.Load<Texture2D>(this.overrideTexturePath.Value);
					}
					catch (Exception)
					{
						this._drawnTexture = null;
					}
				}
				if (this._drawnTexture == null)
				{
					this._drawnTexture = Game1.cropSpriteSheet;
				}
			}
			return this._drawnTexture;
		}
	}

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

	public NetFields NetFields { get; } = new NetFields("Crop");

	public Crop()
	{
		this.NetFields.SetOwner(this).AddField(this.phaseDays, "phaseDays").AddField(this.rowInSpriteSheet, "rowInSpriteSheet")
			.AddField(this.phaseToShow, "phaseToShow")
			.AddField(this.currentPhase, "currentPhase")
			.AddField(this.indexOfHarvest, "indexOfHarvest")
			.AddField(this.dayOfCurrentPhase, "dayOfCurrentPhase")
			.AddField(this.whichForageCrop, "whichForageCrop")
			.AddField(this.replaceWithObjectOnFullGrown, "replaceWithObjectOnFullGrown")
			.AddField(this.tintColor, "tintColor")
			.AddField(this.flip, "flip")
			.AddField(this.fullyGrown, "fullyGrown")
			.AddField(this.raisedSeeds, "raisedSeeds")
			.AddField(this.programColored, "programColored")
			.AddField(this.dead, "dead")
			.AddField(this.forageCrop, "forageCrop")
			.AddField(this.netSeedIndex, "netSeedIndex")
			.AddField(this.overrideTexturePath, "overrideTexturePath")
			.AddField(this.modData, "modData");
		this.dayOfCurrentPhase.fieldChangeVisibleEvent += delegate
		{
			this.updateDrawMath(this.tilePosition);
		};
		this.fullyGrown.fieldChangeVisibleEvent += delegate
		{
			this.updateDrawMath(this.tilePosition);
		};
		this.currentLocation = Game1.currentLocation;
	}

	public Crop(bool forageCrop, string which, int tileX, int tileY, GameLocation location)
		: this()
	{
		this.currentLocation = location;
		this.forageCrop.Value = forageCrop;
		this.whichForageCrop.Value = which;
		this.fullyGrown.Value = true;
		this.currentPhase.Value = 5;
		this.updateDrawMath(new Vector2(tileX, tileY));
	}

	public Crop(string seedId, int tileX, int tileY, GameLocation location)
		: this()
	{
		this.currentLocation = location;
		seedId = Crop.ResolveSeedId(seedId, location);
		if (Crop.TryGetData(seedId, out var data))
		{
			ParsedItemData harvestItemData = ItemRegistry.GetDataOrErrorItem(data.HarvestItemId);
			if (!harvestItemData.HasTypeObject())
			{
				Game1.log.Warn($"Crop seed {seedId} produces non-object item {harvestItemData.QualifiedItemId}, which isn't valid.");
			}
			this.phaseDays.AddRange(data.DaysInPhase);
			this.phaseDays.Add(99999);
			this.rowInSpriteSheet.Value = data.SpriteIndex;
			this.indexOfHarvest.Value = harvestItemData.ItemId;
			this.overrideTexturePath.Value = data.GetCustomTextureName("TileSheets\\crops");
			if (this.isWildSeedCrop())
			{
				this.whichForageCrop.Value = seedId;
				this.replaceWithObjectOnFullGrown.Value = this.getRandomWildCropForSeason(onlyDeterministic: true);
			}
			else
			{
				this.netSeedIndex.Value = seedId;
			}
			this.raisedSeeds.Value = data.IsRaised;
			List<string> tintColors = data.TintColors;
			if (tintColors != null && tintColors.Count > 0)
			{
				Color? color = Utility.StringToColor(Utility.CreateRandom((double)tileX * 1000.0, tileY, Game1.dayOfMonth).ChooseFrom(data.TintColors));
				if (color.HasValue)
				{
					this.tintColor.Value = color.Value;
					this.programColored.Value = true;
				}
			}
		}
		else
		{
			this.netSeedIndex.Value = seedId ?? "0";
			this.indexOfHarvest.Value = seedId ?? "0";
		}
		this.flip.Value = Game1.random.NextBool();
		this.updateDrawMath(new Vector2(tileX, tileY));
	}

	/// <summary>Choose a random seed from a bag of mixed seeds, if applicable.</summary>
	/// <param name="itemId">The unqualified item ID for the seed item.</param>
	/// <param name="location">The location for which to resolve the crop.</param>
	/// <returns>Returns the unqualified seed ID to use.</returns>
	public static string ResolveSeedId(string itemId, GameLocation location)
	{
		if (!(itemId == "MixedFlowerSeeds"))
		{
			if (itemId == "770")
			{
				string seedId = Crop.getRandomLowGradeCropForThisSeason(location.GetSeason());
				if (seedId == "473")
				{
					seedId = "472";
				}
				if (location is IslandLocation)
				{
					seedId = Game1.random.Next(4) switch
					{
						0 => "479", 
						1 => "833", 
						2 => "481", 
						_ => "478", 
					};
				}
				return seedId;
			}
			return itemId;
		}
		return Crop.getRandomFlowerSeedForThisSeason(location.GetSeason());
	}

	/// <summary>Get the crop's data from <see cref="F:StardewValley.Game1.cropData" />, if found.</summary>
	public CropData GetData()
	{
		if (!Crop.TryGetData(this.isWildSeedCrop() ? this.whichForageCrop.Value : this.netSeedIndex.Value, out var data))
		{
			return null;
		}
		return data;
	}

	/// <summary>Try to get a crop's data from <see cref="F:StardewValley.Game1.cropData" />.</summary>
	/// <param name="seedId">The unqualified item ID for the crop's seed (i.e. the key in <see cref="F:StardewValley.Game1.cropData" />).</param>
	/// <param name="data">The crop data, if found.</param>
	/// <returns>Returns whether the crop data was found.</returns>
	public static bool TryGetData(string seedId, out CropData data)
	{
		if (seedId == null)
		{
			data = null;
			return false;
		}
		return Game1.cropData.TryGetValue(seedId, out data);
	}

	/// <summary>Get whether this crop is in season for the given location.</summary>
	/// <param name="location">The location to check.</param>
	public bool IsInSeason(GameLocation location)
	{
		if (location.SeedsIgnoreSeasonsHere())
		{
			return true;
		}
		return this.GetData()?.Seasons?.Contains(location.GetSeason()) ?? false;
	}

	/// <summary>Get whether a crop is in season for the given location.</summary>
	/// <param name="location">The location to check.</param>
	/// <param name="seedId">The unqualified item ID for the crop's seed.</param>
	public static bool IsInSeason(GameLocation location, string seedId)
	{
		if (location.SeedsIgnoreSeasonsHere())
		{
			return true;
		}
		if (Crop.TryGetData(seedId, out var data))
		{
			return data.Seasons?.Contains(location.GetSeason()) ?? false;
		}
		return false;
	}

	/// <summary>Get the method by which the crop can be harvested.</summary>
	public HarvestMethod GetHarvestMethod()
	{
		return this.GetData()?.HarvestMethod ?? HarvestMethod.Grab;
	}

	/// <summary>Get whether this crop regrows after it's harvested.</summary>
	public bool RegrowsAfterHarvest()
	{
		CropData data = this.GetData();
		if (data == null)
		{
			return false;
		}
		return data.RegrowDays > 0;
	}

	public virtual bool IsErrorCrop()
	{
		if (this.forageCrop.Value)
		{
			return false;
		}
		if (!this._isErrorCrop.HasValue)
		{
			this._isErrorCrop = this.GetData() == null;
		}
		return this._isErrorCrop.Value;
	}

	public virtual void ResetPhaseDays()
	{
		CropData data = this.GetData();
		if (data != null)
		{
			this.phaseDays.Clear();
			this.phaseDays.AddRange(data.DaysInPhase);
			this.phaseDays.Add(99999);
		}
	}

	public static string getRandomLowGradeCropForThisSeason(Season season)
	{
		if (season == Season.Winter)
		{
			season = Game1.random.Choose(Season.Spring, Season.Summer, Season.Fall);
		}
		return season switch
		{
			Season.Spring => Game1.random.Next(472, 476).ToString(), 
			Season.Summer => Game1.random.Next(4) switch
			{
				0 => "487", 
				1 => "483", 
				2 => "482", 
				_ => "484", 
			}, 
			Season.Fall => Game1.random.Next(487, 491).ToString(), 
			_ => null, 
		};
	}

	public static string getRandomFlowerSeedForThisSeason(Season season)
	{
		if (season == Season.Winter)
		{
			season = Game1.random.Choose(Season.Spring, Season.Summer, Season.Fall);
		}
		return season switch
		{
			Season.Spring => Game1.random.Choose("427", "429"), 
			Season.Summer => Game1.random.Choose("455", "453", "431"), 
			Season.Fall => Game1.random.Choose("431", "425"), 
			_ => "-1", 
		};
	}

	public virtual void growCompletely()
	{
		this.currentPhase.Value = this.phaseDays.Count - 1;
		this.dayOfCurrentPhase.Value = 0;
		if (this.RegrowsAfterHarvest())
		{
			this.fullyGrown.Value = true;
		}
		this.updateDrawMath(this.tilePosition);
	}

	public virtual bool hitWithHoe(int xTile, int yTile, GameLocation location, HoeDirt dirt)
	{
		if (this.forageCrop.Value && this.whichForageCrop.Value == "2")
		{
			dirt.state.Value = (location.IsRainingHere() ? 1 : 0);
			Object harvestedItem = ItemRegistry.Create<Object>("(O)829");
			Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(12, new Vector2(xTile * 64, yTile * 64), Color.White, 8, Game1.random.NextBool(), 50f));
			location.playSound("dirtyHit");
			Game1.createItemDebris(harvestedItem.getOne(), new Vector2(xTile * 64 + 32, yTile * 64 + 32), -1);
			return true;
		}
		return false;
	}

	public virtual bool harvest(int xTile, int yTile, HoeDirt soil, JunimoHarvester junimoHarvester = null, bool isForcedScytheHarvest = false)
	{
		if (this.dead.Value)
		{
			if (junimoHarvester != null)
			{
				return true;
			}
			return false;
		}
		bool success = false;
		if (this.forageCrop.Value)
		{
			Object o = null;
			int experience = 3;
			Random r = Utility.CreateDaySaveRandom(xTile * 1000, yTile * 2000);
			string value = this.whichForageCrop.Value;
			if (!(value == "1"))
			{
				if (value == "2")
				{
					soil.shake((float)Math.PI / 48f, (float)Math.PI / 40f, (float)(xTile * 64) < Game1.player.Position.X);
					return false;
				}
			}
			else
			{
				o = ItemRegistry.Create<Object>("(O)399");
			}
			if (Game1.player.professions.Contains(16))
			{
				o.Quality = 4;
			}
			else if (r.NextDouble() < (double)((float)Game1.player.ForagingLevel / 30f))
			{
				o.Quality = 2;
			}
			else if (r.NextDouble() < (double)((float)Game1.player.ForagingLevel / 15f))
			{
				o.Quality = 1;
			}
			Game1.stats.ItemsForaged += (uint)o.Stack;
			if (junimoHarvester != null)
			{
				junimoHarvester.tryToAddItemToHut(o);
				return true;
			}
			if (isForcedScytheHarvest)
			{
				Vector2 initialTile = new Vector2(xTile, yTile);
				Game1.createItemDebris(o, new Vector2(initialTile.X * 64f + 32f, initialTile.Y * 64f + 32f), -1);
				Game1.player.gainExperience(2, experience);
				Game1.player.currentLocation.playSound("moss_cut");
				return true;
			}
			if (Game1.player.addItemToInventoryBool(o))
			{
				Vector2 initialTile2 = new Vector2(xTile, yTile);
				Game1.player.animateOnce(279 + Game1.player.FacingDirection);
				Game1.player.canMove = false;
				Game1.player.currentLocation.playSound("harvest");
				DelayedAction.playSoundAfterDelay("coin", 260);
				if (!this.RegrowsAfterHarvest())
				{
					Game1.multiplayer.broadcastSprites(Game1.currentLocation, new TemporaryAnimatedSprite(17, new Vector2(initialTile2.X * 64f, initialTile2.Y * 64f), Color.White, 7, r.NextBool(), 125f));
					Game1.multiplayer.broadcastSprites(Game1.currentLocation, new TemporaryAnimatedSprite(14, new Vector2(initialTile2.X * 64f, initialTile2.Y * 64f), Color.White, 7, r.NextBool(), 50f));
				}
				Game1.player.gainExperience(2, experience);
				return true;
			}
			Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Crop.cs.588"));
		}
		else if (this.currentPhase.Value >= this.phaseDays.Count - 1 && (!this.fullyGrown.Value || this.dayOfCurrentPhase.Value <= 0))
		{
			if (string.IsNullOrWhiteSpace(this.indexOfHarvest.Value))
			{
				return true;
			}
			CropData data = this.GetData();
			Random r2 = Utility.CreateRandom((double)xTile * 7.0, (double)yTile * 11.0, Game1.stats.DaysPlayed, Game1.uniqueIDForThisGame);
			int fertilizerQualityLevel = soil.GetFertilizerQualityBoostLevel();
			double chanceForGoldQuality = 0.2 * ((double)Game1.player.FarmingLevel / 10.0) + 0.2 * (double)fertilizerQualityLevel * (((double)Game1.player.FarmingLevel + 2.0) / 12.0) + 0.01;
			double chanceForSilverQuality = Math.Min(0.75, chanceForGoldQuality * 2.0);
			int cropQuality = 0;
			if (fertilizerQualityLevel >= 3 && r2.NextDouble() < chanceForGoldQuality / 2.0)
			{
				cropQuality = 4;
			}
			else if (r2.NextDouble() < chanceForGoldQuality)
			{
				cropQuality = 2;
			}
			else if (r2.NextDouble() < chanceForSilverQuality || fertilizerQualityLevel >= 3)
			{
				cropQuality = 1;
			}
			cropQuality = MathHelper.Clamp(cropQuality, data?.HarvestMinQuality ?? 0, data?.HarvestMaxQuality ?? cropQuality);
			int numToHarvest = 1;
			if (data != null)
			{
				int minStack = data.HarvestMinStack;
				int maxStack = Math.Max(minStack, data.HarvestMaxStack);
				if (data.HarvestMaxIncreasePerFarmingLevel > 0f)
				{
					maxStack += (int)((float)Game1.player.FarmingLevel * data.HarvestMaxIncreasePerFarmingLevel);
				}
				if (minStack > 1 || maxStack > 1)
				{
					numToHarvest = r2.Next(minStack, maxStack + 1);
				}
			}
			if (data != null && data.ExtraHarvestChance > 0.0)
			{
				while (r2.NextDouble() < Math.Min(0.9, data.ExtraHarvestChance))
				{
					numToHarvest++;
				}
			}
			Item harvestedItem = (this.programColored.Value ? new ColoredObject(this.indexOfHarvest.Value, 1, this.tintColor.Value)
			{
				Quality = cropQuality
			} : ItemRegistry.Create(this.indexOfHarvest.Value, 1, cropQuality));
			HarvestMethod harvestMethod = data?.HarvestMethod ?? HarvestMethod.Grab;
			if (harvestMethod == HarvestMethod.Scythe || isForcedScytheHarvest)
			{
				if (junimoHarvester != null)
				{
					DelayedAction.playSoundAfterDelay("daggerswipe", 150, junimoHarvester.currentLocation);
					if (Utility.isOnScreen(junimoHarvester.TilePoint, 64, junimoHarvester.currentLocation))
					{
						junimoHarvester.currentLocation.playSound("harvest");
						DelayedAction.playSoundAfterDelay("coin", 260, junimoHarvester.currentLocation);
					}
					junimoHarvester.tryToAddItemToHut(harvestedItem.getOne());
				}
				else
				{
					Game1.createItemDebris(harvestedItem.getOne(), new Vector2(xTile * 64 + 32, yTile * 64 + 32), -1);
				}
				success = true;
			}
			else if (junimoHarvester != null || (harvestedItem != null && Game1.player.addItemToInventoryBool(harvestedItem.getOne())))
			{
				Vector2 initialTile3 = new Vector2(xTile, yTile);
				if (junimoHarvester == null)
				{
					Game1.player.animateOnce(279 + Game1.player.FacingDirection);
					Game1.player.canMove = false;
				}
				else
				{
					junimoHarvester.tryToAddItemToHut(harvestedItem.getOne());
				}
				if (r2.NextDouble() < Game1.player.team.AverageLuckLevel() / 1500.0 + Game1.player.team.AverageDailyLuck() / 1200.0 + 9.999999747378752E-05)
				{
					numToHarvest *= 2;
					if (junimoHarvester == null)
					{
						Game1.player.currentLocation.playSound("dwoop");
					}
					else if (Utility.isOnScreen(junimoHarvester.TilePoint, 64, junimoHarvester.currentLocation))
					{
						junimoHarvester.currentLocation.playSound("dwoop");
					}
				}
				else if (harvestMethod == HarvestMethod.Grab)
				{
					if (junimoHarvester == null)
					{
						Game1.player.currentLocation.playSound("harvest");
					}
					else if (Utility.isOnScreen(junimoHarvester.TilePoint, 64, junimoHarvester.currentLocation))
					{
						junimoHarvester.currentLocation.playSound("harvest");
					}
					if (junimoHarvester == null)
					{
						DelayedAction.playSoundAfterDelay("coin", 260, Game1.player.currentLocation);
					}
					else if (Utility.isOnScreen(junimoHarvester.TilePoint, 64, junimoHarvester.currentLocation))
					{
						DelayedAction.playSoundAfterDelay("coin", 260, junimoHarvester.currentLocation);
					}
					if (!this.RegrowsAfterHarvest() && (junimoHarvester == null || junimoHarvester.currentLocation.Equals(Game1.currentLocation)))
					{
						Game1.multiplayer.broadcastSprites(Game1.currentLocation, new TemporaryAnimatedSprite(17, new Vector2(initialTile3.X * 64f, initialTile3.Y * 64f), Color.White, 7, Game1.random.NextBool(), 125f));
						Game1.multiplayer.broadcastSprites(Game1.currentLocation, new TemporaryAnimatedSprite(14, new Vector2(initialTile3.X * 64f, initialTile3.Y * 64f), Color.White, 7, Game1.random.NextBool(), 50f));
					}
				}
				success = true;
			}
			else
			{
				Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Crop.cs.588"));
			}
			if (success)
			{
				if (this.indexOfHarvest.Value == "421")
				{
					this.indexOfHarvest.Value = "431";
					numToHarvest = r2.Next(1, 4);
				}
				harvestedItem = (this.programColored.Value ? new ColoredObject(this.indexOfHarvest.Value, 1, this.tintColor.Value) : ItemRegistry.Create(this.indexOfHarvest.Value));
				int price = 0;
				if (harvestedItem is Object obj)
				{
					price = obj.Price;
				}
				float experience2 = (float)(16.0 * Math.Log(0.018 * (double)price + 1.0, Math.E));
				if (junimoHarvester == null)
				{
					Game1.player.gainExperience(0, (int)Math.Round(experience2));
				}
				for (int i = 0; i < numToHarvest - 1; i++)
				{
					if (junimoHarvester == null)
					{
						Game1.createItemDebris(harvestedItem.getOne(), new Vector2(xTile * 64 + 32, yTile * 64 + 32), -1);
					}
					else
					{
						junimoHarvester.tryToAddItemToHut(harvestedItem.getOne());
					}
				}
				string value = this.indexOfHarvest.Value;
				if (!(value == "262"))
				{
					if (value == "771")
					{
						soil?.Location?.playSound("cut");
						if (r2.NextDouble() < 0.1)
						{
							Item mixedSeeds = ItemRegistry.Create("(O)770");
							if (junimoHarvester == null)
							{
								Game1.createItemDebris(mixedSeeds.getOne(), new Vector2(xTile * 64 + 32, yTile * 64 + 32), -1);
							}
							else
							{
								junimoHarvester.tryToAddItemToHut(mixedSeeds.getOne());
							}
						}
					}
				}
				else if (r2.NextDouble() < 0.4)
				{
					Item hay_item = ItemRegistry.Create("(O)178");
					if (junimoHarvester == null)
					{
						Game1.createItemDebris(hay_item.getOne(), new Vector2(xTile * 64 + 32, yTile * 64 + 32), -1);
					}
					else
					{
						junimoHarvester.tryToAddItemToHut(hay_item.getOne());
					}
				}
				int regrowDays = data?.RegrowDays ?? (-1);
				if (regrowDays <= 0)
				{
					return true;
				}
				this.fullyGrown.Value = true;
				if (this.dayOfCurrentPhase.Value == regrowDays)
				{
					this.updateDrawMath(this.tilePosition);
				}
				this.dayOfCurrentPhase.Value = regrowDays;
			}
		}
		return false;
	}

	/// <summary>Get a random qualified object ID to harvest from wild seeds.</summary>
	/// <param name="onlyDeterministic">Only return a value if it can be accurately predicted ahead of time (i.e. the harvest doesn't depend on the date that it's harvested).</param>
	/// <remarks>This uses the season associated with the crop (e.g. spring for Spring Seeds) or the current location's season.</remarks>
	public string getRandomWildCropForSeason(bool onlyDeterministic = false)
	{
		switch (this.whichForageCrop.Value)
		{
		case "495":
			return this.getRandomWildCropForSeason(Season.Spring);
		case "496":
			return this.getRandomWildCropForSeason(Season.Summer);
		case "497":
			return this.getRandomWildCropForSeason(Season.Fall);
		case "498":
			return this.getRandomWildCropForSeason(Season.Winter);
		default:
			if (onlyDeterministic && !this.currentLocation.SeedsIgnoreSeasonsHere())
			{
				return null;
			}
			return this.getRandomWildCropForSeason(this.currentLocation.GetSeason());
		}
	}

	/// <summary>Get a random qualified object ID to harvest from wild seeds.</summary>
	/// <param name="season">The season for which to choose a produce.</param>
	public string getRandomWildCropForSeason(Season season)
	{
		return season switch
		{
			Season.Spring => Game1.random.Choose("(O)16", "(O)18", "(O)20", "(O)22"), 
			Season.Summer => Game1.random.Choose("(O)396", "(O)398", "(O)402"), 
			Season.Fall => Game1.random.Choose("(O)404", "(O)406", "(O)408", "(O)410"), 
			Season.Winter => Game1.random.Choose("(O)412", "(O)414", "(O)416", "(O)418"), 
			_ => "(O)22", 
		};
	}

	public virtual Rectangle getSourceRect(int number)
	{
		if (this.dead.Value)
		{
			return new Rectangle(192 + number % 4 * 16, 384, 16, 32);
		}
		int effectiveRow = this.rowInSpriteSheet.Value;
		Season localSeason = Game1.GetSeasonForLocation(this.currentLocation);
		if (this.indexOfHarvest.Value == "771")
		{
			switch (localSeason)
			{
			case Season.Fall:
				effectiveRow = this.rowInSpriteSheet.Value + 1;
				break;
			case Season.Winter:
				effectiveRow = this.rowInSpriteSheet.Value + 2;
				break;
			}
		}
		return new Rectangle(Math.Min(240, ((!this.fullyGrown.Value) ? (((this.phaseToShow.Value != -1) ? this.phaseToShow.Value : this.currentPhase.Value) + ((((this.phaseToShow.Value != -1) ? this.phaseToShow.Value : this.currentPhase.Value) == 0 && number % 2 == 0) ? (-1) : 0) + 1) : ((this.dayOfCurrentPhase.Value <= 0) ? 6 : 7)) * 16 + ((effectiveRow % 2 != 0) ? 128 : 0)), effectiveRow / 2 * 16 * 2, 16, 32);
	}

	/// <summary>Get the giant crops which can grow from this crop, if any.</summary>
	/// <param name="giantCrops">The giant crops which can grow from this crop.</param>
	/// <returns>Returns whether <paramref name="giantCrops" /> is non-empty.</returns>
	public bool TryGetGiantCrops(out IReadOnlyList<KeyValuePair<string, GiantCropData>> giantCrops)
	{
		giantCrops = GiantCrop.GetGiantCropsFor("(O)" + this.indexOfHarvest.Value);
		return giantCrops.Count > 0;
	}

	public void Kill()
	{
		this.dead.Value = true;
		this.raisedSeeds.Value = false;
	}

	public virtual void newDay(int state)
	{
		GameLocation environment = this.currentLocation;
		Vector2 tileVector = this.tilePosition;
		Utility.Vector2ToPoint(tileVector);
		if (environment.isOutdoors.Value && (this.dead.Value || !this.IsInSeason(environment)))
		{
			this.Kill();
			return;
		}
		if (state != 1)
		{
			CropData data = this.GetData();
			if (data == null || data.NeedsWatering)
			{
				goto IL_01f4;
			}
		}
		if (!this.fullyGrown.Value)
		{
			this.dayOfCurrentPhase.Value = Math.Min(this.dayOfCurrentPhase.Value + 1, (this.phaseDays.Count > 0) ? this.phaseDays[Math.Min(this.phaseDays.Count - 1, this.currentPhase.Value)] : 0);
		}
		else
		{
			this.dayOfCurrentPhase.Value--;
		}
		if (this.dayOfCurrentPhase.Value >= ((this.phaseDays.Count > 0) ? this.phaseDays[Math.Min(this.phaseDays.Count - 1, this.currentPhase.Value)] : 0) && this.currentPhase.Value < this.phaseDays.Count - 1)
		{
			this.currentPhase.Value++;
			this.dayOfCurrentPhase.Value = 0;
		}
		while (this.currentPhase.Value < this.phaseDays.Count - 1 && this.phaseDays.Count > 0 && this.phaseDays[this.currentPhase.Value] <= 0)
		{
			this.currentPhase.Value++;
		}
		if (this.isWildSeedCrop() && this.phaseToShow.Value == -1 && this.currentPhase.Value > 0)
		{
			this.phaseToShow.Value = Game1.random.Next(1, 7);
		}
		this.TryGrowGiantCrop();
		goto IL_01f4;
		IL_01f4:
		if ((!this.fullyGrown.Value || this.dayOfCurrentPhase.Value <= 0) && this.currentPhase.Value >= this.phaseDays.Count - 1)
		{
			if (this.replaceWithObjectOnFullGrown.Value != null || this.isWildSeedCrop())
			{
				if (environment.objects.TryGetValue(tileVector, out var obj))
				{
					if (obj is IndoorPot pot)
					{
						pot.heldObject.Value = ItemRegistry.Create<Object>(this.replaceWithObjectOnFullGrown.Value ?? this.getRandomWildCropForSeason());
						pot.hoeDirt.Value.crop = null;
					}
					else
					{
						environment.objects.Remove(tileVector);
					}
				}
				if (!environment.objects.ContainsKey(tileVector))
				{
					Object spawned = ItemRegistry.Create<Object>(this.replaceWithObjectOnFullGrown.Value ?? this.getRandomWildCropForSeason());
					spawned.IsSpawnedObject = true;
					spawned.CanBeGrabbed = true;
					spawned.SpecialVariable = 724519;
					environment.objects.Add(tileVector, spawned);
				}
				if (environment.terrainFeatures.TryGetValue(tileVector, out var terrainFeature) && terrainFeature is HoeDirt dirt)
				{
					dirt.crop = null;
				}
			}
			if (this.indexOfHarvest.Value != null && this.indexOfHarvest.Value != null && this.indexOfHarvest.Value.Length > 0 && environment.IsFarm)
			{
				foreach (Farmer allFarmer in Game1.getAllFarmers())
				{
					allFarmer.autoGenerateActiveDialogueEvent("cropMatured_" + this.indexOfHarvest.Value);
				}
			}
		}
		if (this.fullyGrown.Value && this.indexOfHarvest.Value != null && this.indexOfHarvest.Value != null && this.indexOfHarvest.Value == "595")
		{
			Game1.getFarm().hasMatureFairyRoseTonight = true;
		}
		this.updateDrawMath(tileVector);
	}

	/// <summary>Try to replace the grid of crops with this one at its top-left corner with a giant crop, if valid and the probability check passes.</summary>
	/// <param name="checkPreconditions">Whether to check that the location allows giant crops and the crop is fully grown. Setting this to false won't affect other conditions like having a grid of crops or the per-giant-crop conditions and chance.</param>
	/// <param name="random">The RNG to use for random checks, or <c>null</c> for the default seed logic.</param>
	public virtual bool TryGrowGiantCrop(bool checkPreconditions = true, Random random = null)
	{
		GameLocation environment = this.currentLocation;
		Vector2 tile = this.tilePosition;
		if (checkPreconditions)
		{
			if (!(environment is Farm) && !environment.HasMapPropertyWithValue("AllowGiantCrops"))
			{
				return false;
			}
			if (this.currentPhase.Value != this.phaseDays.Count - 1)
			{
				return false;
			}
		}
		if (!this.TryGetGiantCrops(out var possibleGiantCrops))
		{
			return false;
		}
		foreach (KeyValuePair<string, GiantCropData> pair in possibleGiantCrops)
		{
			string giantCropId = pair.Key;
			GiantCropData giantCrop = pair.Value;
			if ((giantCrop.Chance < 1f && !(random ?? Utility.CreateDaySaveRandom(tile.X, tile.Y, Game1.hash.GetDeterministicHashCode(giantCropId))).NextBool(giantCrop.Chance)) || !GameStateQuery.CheckConditions(giantCrop.Condition, environment))
			{
				continue;
			}
			bool valid = true;
			for (int y = (int)tile.Y; (float)y < tile.Y + (float)giantCrop.TileSize.Y; y++)
			{
				for (int x = (int)tile.X; (float)x < tile.X + (float)giantCrop.TileSize.X; x++)
				{
					if (!(environment.terrainFeatures.GetValueOrDefault(new Vector2(x, y)) is HoeDirt dirt) || !(dirt.crop?.indexOfHarvest.Value == this.indexOfHarvest.Value))
					{
						valid = false;
						break;
					}
				}
				if (!valid)
				{
					break;
				}
			}
			if (!valid)
			{
				continue;
			}
			for (int i = (int)tile.Y; (float)i < tile.Y + (float)giantCrop.TileSize.Y; i++)
			{
				for (int j = (int)tile.X; (float)j < tile.X + (float)giantCrop.TileSize.X; j++)
				{
					Vector2 v = new Vector2(j, i);
					((HoeDirt)environment.terrainFeatures[v]).crop = null;
				}
			}
			environment.resourceClumps.Add(new GiantCrop(giantCropId, tile));
			return true;
		}
		return false;
	}

	public virtual bool isPaddyCrop()
	{
		return this.GetData()?.IsPaddyCrop ?? false;
	}

	public virtual bool shouldDrawDarkWhenWatered()
	{
		if (this.isPaddyCrop())
		{
			return false;
		}
		return !this.raisedSeeds.Value;
	}

	/// <summary>Get whether this is a vanilla wild seed crop.</summary>
	public virtual bool isWildSeedCrop()
	{
		if (this.overrideTexturePath.Value == null || this.overrideTexturePath.Value == Game1.cropSpriteSheet.Name)
		{
			return this.rowInSpriteSheet.Value == 23;
		}
		return false;
	}

	public virtual void updateDrawMath(Vector2 tileLocation)
	{
		if (tileLocation.Equals(Vector2.Zero))
		{
			return;
		}
		if (this.forageCrop.Value)
		{
			if (!int.TryParse(this.whichForageCrop.Value, out var which_forage_crop))
			{
				which_forage_crop = 1;
			}
			this.drawPosition = new Vector2(tileLocation.X * 64f + ((tileLocation.X * 11f + tileLocation.Y * 7f) % 10f - 5f) + 32f, tileLocation.Y * 64f + ((tileLocation.Y * 11f + tileLocation.X * 7f) % 10f - 5f) + 32f);
			this.layerDepth = (tileLocation.Y * 64f + 32f + ((tileLocation.Y * 11f + tileLocation.X * 7f) % 10f - 5f)) / 10000f;
			this.sourceRect = new Rectangle((int)(tileLocation.X * 51f + tileLocation.Y * 77f) % 3 * 16, 128 + which_forage_crop * 16, 16, 16);
		}
		else
		{
			this.drawPosition = new Vector2(tileLocation.X * 64f + ((!this.shouldDrawDarkWhenWatered() || this.currentPhase.Value >= this.phaseDays.Count - 1) ? 0f : ((tileLocation.X * 11f + tileLocation.Y * 7f) % 10f - 5f)) + 32f, tileLocation.Y * 64f + ((this.raisedSeeds.Value || this.currentPhase.Value >= this.phaseDays.Count - 1) ? 0f : ((tileLocation.Y * 11f + tileLocation.X * 7f) % 10f - 5f)) + 32f);
			this.layerDepth = (tileLocation.Y * 64f + 32f + ((!this.shouldDrawDarkWhenWatered() || this.currentPhase.Value >= this.phaseDays.Count - 1) ? 0f : ((tileLocation.Y * 11f + tileLocation.X * 7f) % 10f - 5f))) / 10000f / ((this.currentPhase.Value == 0 && this.shouldDrawDarkWhenWatered()) ? 2f : 1f);
			this.sourceRect = this.getSourceRect((int)tileLocation.X * 7 + (int)tileLocation.Y * 11);
			this.coloredSourceRect = new Rectangle(((!this.fullyGrown.Value) ? (this.currentPhase.Value + 1 + 1) : ((this.dayOfCurrentPhase.Value <= 0) ? 6 : 7)) * 16 + ((this.rowInSpriteSheet.Value % 2 != 0) ? 128 : 0), this.rowInSpriteSheet.Value / 2 * 16 * 2, 16, 32);
			this.coloredLayerDepth = (tileLocation.Y * 64f + 32f + ((tileLocation.Y * 11f + tileLocation.X * 7f) % 10f - 5f)) / 10000f / (float)((this.currentPhase.Value != 0 || !this.shouldDrawDarkWhenWatered()) ? 1 : 2);
		}
		this.tilePosition = tileLocation;
	}

	public virtual void draw(SpriteBatch b, Vector2 tileLocation, Color toTint, float rotation)
	{
		Vector2 position = Game1.GlobalToLocal(Game1.viewport, this.drawPosition);
		if (this.forageCrop.Value)
		{
			if (this.whichForageCrop.Value == "2")
			{
				b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f + ((tileLocation.X * 11f + tileLocation.Y * 7f) % 10f - 5f) + 32f, tileLocation.Y * 64f + ((tileLocation.Y * 11f + tileLocation.X * 7f) % 10f - 5f) + 64f)), new Rectangle(128 + (int)((Game1.currentGameTime.TotalGameTime.TotalMilliseconds + (double)(tileLocation.X * 111f + tileLocation.Y * 77f)) % 800.0 / 200.0) * 16, 128, 16, 16), Color.White, rotation, new Vector2(8f, 16f), 4f, SpriteEffects.None, (tileLocation.Y * 64f + 32f + ((tileLocation.Y * 11f + tileLocation.X * 7f) % 10f - 5f)) / 10000f);
			}
			else
			{
				b.Draw(Game1.mouseCursors, position, this.sourceRect, Color.White, 0f, Crop.smallestTileSizeOrigin, 4f, SpriteEffects.None, this.layerDepth);
			}
			return;
		}
		if (this.IsErrorCrop())
		{
			ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem("(O)" + this.indexOfHarvest.Value);
			b.Draw(itemData.GetTexture(), position, itemData.GetSourceRect(), toTint, rotation, new Vector2(8f, 8f), 4f, SpriteEffects.None, this.layerDepth);
			return;
		}
		SpriteEffects effect = (this.flip.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
		b.Draw(this.DrawnCropTexture, position, this.sourceRect, toTint, rotation, Crop.origin, 4f, effect, this.layerDepth);
		Color tintColor = this.tintColor.Value;
		if (!tintColor.Equals(Color.White) && this.currentPhase.Value == this.phaseDays.Count - 1 && !this.dead.Value)
		{
			b.Draw(this.DrawnCropTexture, position, this.coloredSourceRect, tintColor, rotation, Crop.origin, 4f, effect, this.coloredLayerDepth);
		}
	}

	public virtual void drawInMenu(SpriteBatch b, Vector2 screenPosition, Color toTint, float rotation, float scale, float layerDepth)
	{
		if (this.IsErrorCrop())
		{
			ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem("(O)" + this.indexOfHarvest.Value);
			b.Draw(itemData.GetTexture(), screenPosition, itemData.GetSourceRect(), toTint, rotation, new Vector2(32f, 32f), scale, this.flip.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth);
		}
		else
		{
			b.Draw(this.DrawnCropTexture, screenPosition, this.getSourceRect(0), toTint, rotation, new Vector2(32f, 96f), scale, this.flip.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth);
		}
	}

	public virtual void drawWithOffset(SpriteBatch b, Vector2 tileLocation, Color toTint, float rotation, Vector2 offset)
	{
		if (this.IsErrorCrop())
		{
			ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem("(O)" + this.indexOfHarvest.Value);
			b.Draw(itemData.GetTexture(), Game1.GlobalToLocal(Game1.viewport, offset + new Vector2(tileLocation.X * 64f, tileLocation.Y * 64f)), itemData.GetSourceRect(), toTint, rotation, new Vector2(8f, 8f), 4f, this.flip.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (tileLocation.Y + 0.66f) * 64f / 10000f + tileLocation.X * 1E-05f);
			return;
		}
		if (this.forageCrop.Value)
		{
			b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, offset + new Vector2(tileLocation.X * 64f, tileLocation.Y * 64f)), this.sourceRect, Color.White, 0f, new Vector2(8f, 8f), 4f, SpriteEffects.None, (tileLocation.Y + 0.66f) * 64f / 10000f + tileLocation.X * 1E-05f);
			return;
		}
		b.Draw(this.DrawnCropTexture, Game1.GlobalToLocal(Game1.viewport, offset + new Vector2(tileLocation.X * 64f, tileLocation.Y * 64f)), this.sourceRect, toTint, rotation, new Vector2(8f, 24f), 4f, this.flip.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (tileLocation.Y + 0.66f) * 64f / 10000f + tileLocation.X * 1E-05f);
		if (!this.tintColor.Equals(Color.White) && this.currentPhase.Value == this.phaseDays.Count - 1 && !this.dead.Value)
		{
			b.Draw(this.DrawnCropTexture, Game1.GlobalToLocal(Game1.viewport, offset + new Vector2(tileLocation.X * 64f, tileLocation.Y * 64f)), this.coloredSourceRect, this.tintColor.Value, rotation, new Vector2(8f, 24f), 4f, this.flip.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (tileLocation.Y + 0.67f) * 64f / 10000f + tileLocation.X * 1E-05f);
		}
	}
}
