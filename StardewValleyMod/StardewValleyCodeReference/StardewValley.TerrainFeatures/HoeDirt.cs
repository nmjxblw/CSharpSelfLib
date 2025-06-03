using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Characters;
using StardewValley.Enchantments;
using StardewValley.Extensions;
using StardewValley.GameData.Crops;
using StardewValley.Locations;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.Tools;

namespace StardewValley.TerrainFeatures;

public class HoeDirt : TerrainFeature
{
	private struct NeighborLoc
	{
		public readonly Vector2 Offset;

		public readonly byte Direction;

		public readonly byte InvDirection;

		public NeighborLoc(Vector2 a, byte b, byte c)
		{
			this.Offset = a;
			this.Direction = b;
			this.InvDirection = c;
		}
	}

	private struct Neighbor
	{
		public readonly HoeDirt feature;

		public readonly byte direction;

		public readonly byte invDirection;

		public Neighbor(HoeDirt a, byte b, byte c)
		{
			this.feature = a;
			this.direction = b;
			this.invDirection = c;
		}
	}

	public const float defaultShakeRate = (float)Math.PI / 80f;

	public const float maximumShake = (float)Math.PI / 8f;

	public const float shakeDecayRate = (float)Math.PI / 300f;

	public const byte N = 1;

	public const byte E = 2;

	public const byte S = 4;

	public const byte W = 8;

	public const byte Cardinals = 15;

	public static readonly Vector2 N_Offset = new Vector2(0f, -1f);

	public static readonly Vector2 E_Offset = new Vector2(1f, 0f);

	public static readonly Vector2 S_Offset = new Vector2(0f, 1f);

	public static readonly Vector2 W_Offset = new Vector2(-1f, 0f);

	public const float paddyGrowBonus = 0.25f;

	public const int dry = 0;

	public const int watered = 1;

	public const int invisible = 2;

	public const string fertilizerLowQualityID = "368";

	public const string fertilizerHighQualityID = "369";

	public const string waterRetentionSoilID = "370";

	public const string waterRetentionSoilQualityID = "371";

	public const string speedGroID = "465";

	public const string superSpeedGroID = "466";

	public const string hyperSpeedGroID = "918";

	public const string fertilizerDeluxeQualityID = "919";

	public const string waterRetentionSoilDeluxeID = "920";

	public const string fertilizerLowQualityQID = "(O)368";

	public const string fertilizerHighQualityQID = "(O)369";

	public const string waterRetentionSoilQID = "(O)370";

	public const string waterRetentionSoilQualityQID = "(O)371";

	public const string speedGroQID = "(O)465";

	public const string superSpeedGroQID = "(O)466";

	public const string hyperSpeedGroQID = "(O)918";

	public const string fertilizerDeluxeQualityQID = "(O)919";

	public const string waterRetentionSoilDeluxeQID = "(O)920";

	public static Texture2D lightTexture;

	public static Texture2D darkTexture;

	public static Texture2D snowTexture;

	private readonly NetRef<Crop> netCrop = new NetRef<Crop>();

	public static Dictionary<byte, int> drawGuide;

	[XmlElement("state")]
	public readonly NetInt state = new NetInt();

	/// <summary>The qualified or unqualified item ID of the fertilizer applied to this dirt, if any.</summary>
	/// <remarks>See also the helper methods like <see cref="M:StardewValley.TerrainFeatures.HoeDirt.HasFertilizer" />, <see cref="M:StardewValley.TerrainFeatures.HoeDirt.CanApplyFertilizer(System.String)" />, <see cref="M:StardewValley.TerrainFeatures.HoeDirt.GetFertilizerSpeedBoost" />, etc.</remarks>
	[XmlElement("fertilizer")]
	public readonly NetString fertilizer = new NetString();

	private bool shakeLeft;

	private float shakeRotation;

	private float maxShake;

	private float shakeRate;

	[XmlElement("c")]
	private readonly NetColor c = new NetColor(Color.White);

	private List<Action<GameLocation, Vector2>> queuedActions = new List<Action<GameLocation, Vector2>>();

	private byte neighborMask;

	private byte wateredNeighborMask;

	[XmlIgnore]
	public NetInt nearWaterForPaddy = new NetInt(-1);

	private byte drawSum;

	private int sourceRectPosition;

	private int wateredRectPosition;

	private Texture2D texture;

	private static readonly NeighborLoc[] _offsets = new NeighborLoc[4]
	{
		new NeighborLoc(HoeDirt.N_Offset, 1, 4),
		new NeighborLoc(HoeDirt.S_Offset, 4, 1),
		new NeighborLoc(HoeDirt.E_Offset, 2, 8),
		new NeighborLoc(HoeDirt.W_Offset, 8, 2)
	};

	private List<Neighbor> _neighbors = new List<Neighbor>();

	/// <inheritdoc />
	[XmlIgnore]
	public override GameLocation Location
	{
		get
		{
			return base.Location;
		}
		set
		{
			base.Location = value;
			if (this.netCrop.Value != null)
			{
				this.netCrop.Value.currentLocation = value;
			}
		}
	}

	/// <inheritdoc />
	public override Vector2 Tile
	{
		get
		{
			return base.Tile;
		}
		set
		{
			base.Tile = value;
			if (this.netCrop.Value != null)
			{
				this.netCrop.Value.tilePosition = value;
			}
		}
	}

	public Crop crop
	{
		get
		{
			return this.netCrop.Value;
		}
		set
		{
			this.netCrop.Value = value;
		}
	}

	/// <summary>The pot containing this dirt, if applicable.</summary>
	[XmlIgnore]
	public IndoorPot Pot { get; set; }

	public HoeDirt()
		: base(needsTick: true)
	{
		this.loadSprite();
		if (HoeDirt.drawGuide == null)
		{
			HoeDirt.populateDrawGuide();
		}
		this.initialize(Game1.currentLocation);
	}

	public HoeDirt(int startingState, GameLocation location = null)
		: this()
	{
		this.state.Value = startingState;
		this.Location = location ?? Game1.currentLocation;
		if (location != null)
		{
			this.initialize(location);
		}
	}

	public HoeDirt(int startingState, Crop crop)
		: this()
	{
		this.state.Value = startingState;
		this.crop = crop;
	}

	public override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(this.netCrop, "netCrop").AddField(this.state, "state").AddField(this.fertilizer, "fertilizer")
			.AddField(this.c, "c")
			.AddField(this.nearWaterForPaddy, "nearWaterForPaddy");
		this.state.fieldChangeVisibleEvent += delegate
		{
			this.OnAdded(this.Location, this.Tile);
		};
		this.netCrop.fieldChangeVisibleEvent += delegate
		{
			this.nearWaterForPaddy.Value = -1;
			this.updateNeighbors();
			if (this.netCrop.Value != null)
			{
				this.netCrop.Value.Dirt = this;
				this.netCrop.Value.currentLocation = this.Location;
				this.netCrop.Value.updateDrawMath(this.Tile);
			}
		};
		this.nearWaterForPaddy.Interpolated(interpolate: false, wait: false);
		this.netCrop.Interpolated(interpolate: false, wait: false);
		this.netCrop.OnConflictResolve += delegate(Crop rejected, Crop accepted)
		{
			if (Game1.IsMasterGame && rejected != null && rejected.netSeedIndex.Value != null)
			{
				this.queuedActions.Add(delegate(GameLocation gLocation, Vector2 tileLocation)
				{
					Vector2 vector = tileLocation * 64f;
					gLocation.debris.Add(new Debris(rejected.netSeedIndex.Value, vector, vector));
				});
				base.NeedsUpdate = true;
			}
		};
	}

	private void initialize(GameLocation location)
	{
		if (location == null)
		{
			location = Game1.currentLocation;
		}
		if (location == null)
		{
			return;
		}
		if (location is MineShaft mine)
		{
			int mineArea = mine.getMineArea();
			if (mine.GetAdditionalDifficulty() > 0)
			{
				if (mineArea == 0 || mineArea == 10)
				{
					this.c.Value = new Color(80, 100, 140) * 0.5f;
				}
			}
			else if (mineArea == 80)
			{
				this.c.Value = Color.MediumPurple * 0.4f;
			}
		}
		else if (location.GetSeason() == Season.Fall && location.IsOutdoors && !(location is Beach))
		{
			this.c.Value = new Color(250, 210, 240);
		}
		else if (location is VolcanoDungeon)
		{
			this.c.Value = Color.MediumPurple * 0.7f;
		}
	}

	public float getShakeRotation()
	{
		return this.shakeRotation;
	}

	public float getMaxShake()
	{
		return this.maxShake;
	}

	public override Rectangle getBoundingBox()
	{
		Vector2 tileLocation = this.Tile;
		return new Rectangle((int)(tileLocation.X * 64f), (int)(tileLocation.Y * 64f), 64, 64);
	}

	public override void doCollisionAction(Rectangle positionOfCollider, int speedOfCollision, Vector2 tileLocation, Character who)
	{
		if (this.crop != null && this.crop.currentPhase.Value != 0 && speedOfCollision > 0 && this.maxShake == 0f && positionOfCollider.Intersects(this.getBoundingBox()) && Utility.isOnScreen(Utility.Vector2ToPoint(tileLocation), 64, this.Location))
		{
			if (!(who is FarmAnimal))
			{
				Grass.PlayGrassSound();
			}
			this.shake((float)Math.PI / 8f / Math.Min(1f, 5f / (float)speedOfCollision) - ((speedOfCollision > 2) ? ((float)this.crop.currentPhase.Value * (float)Math.PI / 64f) : 0f), (float)Math.PI / 80f / Math.Min(1f, 5f / (float)speedOfCollision), (float)positionOfCollider.Center.X > tileLocation.X * 64f + 32f);
		}
		if (this.crop != null && this.crop.currentPhase.Value != 0 && who is Farmer { running: not false } player)
		{
			if (player.stats.Get("Book_Grass") != 0)
			{
				player.temporarySpeedBuff = -0.33f;
			}
			else
			{
				player.temporarySpeedBuff = -1f;
			}
		}
	}

	public void shake(float shake, float rate, bool left)
	{
		if (this.crop != null)
		{
			this.maxShake = shake * (this.crop.raisedSeeds.Value ? 0.6f : 1.5f);
			this.shakeRate = rate * 0.5f;
			this.shakeRotation = 0f;
			this.shakeLeft = left;
		}
		base.NeedsUpdate = true;
	}

	/// <summary>Whether this dirt contains a crop which needs water to grow further. To check whether it is watered, see <see cref="M:StardewValley.TerrainFeatures.HoeDirt.isWatered" />.</summary>
	public bool needsWatering()
	{
		if (this.crop != null && (!this.readyForHarvest() || this.crop.RegrowsAfterHarvest()))
		{
			return this.crop.GetData()?.NeedsWatering ?? true;
		}
		return false;
	}

	/// <summary>Whether this dirt is watered.</summary>
	/// <remarks>See also <see cref="M:StardewValley.TerrainFeatures.HoeDirt.needsWatering" />.</remarks>
	public bool isWatered()
	{
		return this.state.Value == 1;
	}

	public static void populateDrawGuide()
	{
		HoeDirt.drawGuide = new Dictionary<byte, int>
		{
			[0] = 0,
			[8] = 15,
			[2] = 13,
			[1] = 12,
			[4] = 4,
			[9] = 11,
			[3] = 9,
			[5] = 8,
			[6] = 1,
			[12] = 3,
			[10] = 14,
			[7] = 5,
			[15] = 6,
			[13] = 7,
			[11] = 10,
			[14] = 2
		};
	}

	public override void loadSprite()
	{
		if (HoeDirt.lightTexture == null)
		{
			try
			{
				HoeDirt.lightTexture = Game1.content.Load<Texture2D>("TerrainFeatures\\hoeDirt");
			}
			catch (Exception)
			{
			}
		}
		if (HoeDirt.darkTexture == null)
		{
			try
			{
				HoeDirt.darkTexture = Game1.content.Load<Texture2D>("TerrainFeatures\\hoeDirtDark");
			}
			catch (Exception)
			{
			}
		}
		if (HoeDirt.snowTexture == null)
		{
			try
			{
				HoeDirt.snowTexture = Game1.content.Load<Texture2D>("TerrainFeatures\\hoeDirtSnow");
			}
			catch (Exception)
			{
			}
		}
		this.nearWaterForPaddy.Value = -1;
		this.crop?.updateDrawMath(this.Tile);
	}

	/// <inheritdoc />
	public override bool isPassable(Character c = null)
	{
		if (this.crop != null && this.crop.raisedSeeds.Value)
		{
			return c is JunimoHarvester;
		}
		return true;
	}

	public bool readyForHarvest()
	{
		if (this.crop != null && (!this.crop.fullyGrown.Value || this.crop.dayOfCurrentPhase.Value <= 0) && this.crop.currentPhase.Value >= this.crop.phaseDays.Count - 1 && !this.crop.dead.Value)
		{
			if (this.crop.forageCrop.Value)
			{
				return this.crop.whichForageCrop.Value != "2";
			}
			return true;
		}
		return false;
	}

	public override bool performUseAction(Vector2 tileLocation)
	{
		if (this.crop != null)
		{
			bool harvestable = this.crop.currentPhase.Value >= this.crop.phaseDays.Count - 1 && (!this.crop.fullyGrown.Value || this.crop.dayOfCurrentPhase.Value <= 0);
			HarvestMethod harvestMethod = this.crop.GetHarvestMethod();
			if (Game1.player.CurrentTool != null && Game1.player.CurrentTool.isScythe() && Game1.player.CurrentTool.ItemId == "66")
			{
				harvestMethod = HarvestMethod.Scythe;
			}
			switch (harvestMethod)
			{
			case HarvestMethod.Grab:
				if (this.crop.harvest((int)tileLocation.X, (int)tileLocation.Y, this))
				{
					GameLocation location = this.Location;
					if (location is IslandLocation && Game1.random.NextDouble() < 0.05)
					{
						Game1.player.team.RequestLimitedNutDrops("IslandFarming", location, (int)tileLocation.X * 64, (int)tileLocation.Y * 64, 5);
					}
					this.destroyCrop(showAnimation: false);
					return true;
				}
				break;
			case HarvestMethod.Scythe:
			{
				if (!this.readyForHarvest())
				{
					break;
				}
				Tool currentTool = Game1.player.CurrentTool;
				if (currentTool != null && currentTool.isScythe())
				{
					Game1.player.CanMove = false;
					Game1.player.UsingTool = true;
					Game1.player.canReleaseTool = true;
					Game1.player.Halt();
					try
					{
						Game1.player.CurrentTool.beginUsing(Game1.currentLocation, (int)Game1.player.lastClick.X, (int)Game1.player.lastClick.Y, Game1.player);
					}
					catch (Exception)
					{
					}
					((MeleeWeapon)Game1.player.CurrentTool).setFarmerAnimating(Game1.player);
				}
				else if (Game1.didPlayerJustClickAtAll(ignoreNonMouseHeldInput: true))
				{
					Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:HoeDirt.cs.13915"));
				}
				break;
			}
			}
			return harvestable;
		}
		return false;
	}

	public bool plant(string itemId, Farmer who, bool isFertilizer)
	{
		GameLocation location = this.Location;
		if (isFertilizer)
		{
			if (!this.CanApplyFertilizer(itemId))
			{
				return false;
			}
			this.fertilizer.Value = ItemRegistry.QualifyItemId(itemId) ?? itemId;
			this.applySpeedIncreases(who);
			location.playSound("dirtyHit");
			return true;
		}
		Season season = location.GetSeason();
		Point tilePos = Utility.Vector2ToPoint(this.Tile);
		itemId = Crop.ResolveSeedId(itemId, location);
		if (!Crop.TryGetData(itemId, out var cropData) || cropData.Seasons.Count == 0)
		{
			return false;
		}
		Object obj;
		bool isGardenPot = location.objects.TryGetValue(this.Tile, out obj) && obj is IndoorPot;
		bool isIndoorPot = isGardenPot && !location.IsOutdoors;
		if (!who.currentLocation.CheckItemPlantRules(itemId, isGardenPot, isIndoorPot || (location.GetData()?.CanPlantHere ?? location.IsFarm), out var deniedMessage))
		{
			if (Game1.didPlayerJustClickAtAll(ignoreNonMouseHeldInput: true))
			{
				if (deniedMessage == null && location.NameOrUniqueName != "Farm")
				{
					Farm farm = Game1.getFarm();
					if (farm.CheckItemPlantRules(itemId, isGardenPot, farm.GetData()?.CanPlantHere ?? true, out var _))
					{
						deniedMessage = Game1.content.LoadString("Strings\\StringsFromCSFiles:HoeDirt.cs.13919");
					}
				}
				if (deniedMessage == null)
				{
					deniedMessage = Game1.content.LoadString("Strings\\StringsFromCSFiles:HoeDirt.cs.13925");
				}
				Game1.showRedMessage(deniedMessage);
			}
			return false;
		}
		if (!isIndoorPot && !who.currentLocation.CanPlantSeedsHere(itemId, tilePos.X, tilePos.Y, isGardenPot, out deniedMessage))
		{
			if (Game1.didPlayerJustClickAtAll(ignoreNonMouseHeldInput: true))
			{
				if (deniedMessage == null)
				{
					deniedMessage = Game1.content.LoadString("Strings\\StringsFromCSFiles:HoeDirt.cs.13925");
				}
				Game1.showRedMessage(deniedMessage);
			}
			return false;
		}
		if (isIndoorPot || location.SeedsIgnoreSeasonsHere() || !((!(cropData.Seasons?.Contains(season))) ?? true))
		{
			this.crop = new Crop(itemId, tilePos.X, tilePos.Y, this.Location);
			if (this.crop.raisedSeeds.Value)
			{
				location.playSound("stoneStep");
			}
			location.playSound("dirtyHit");
			Game1.stats.SeedsSown++;
			this.applySpeedIncreases(who);
			this.nearWaterForPaddy.Value = -1;
			if (this.hasPaddyCrop() && this.paddyWaterCheck())
			{
				this.state.Value = 1;
				this.updateNeighbors();
			}
			return true;
		}
		if (Game1.didPlayerJustClickAtAll(ignoreNonMouseHeldInput: true))
		{
			string errorKey = (((!(cropData.Seasons?.Contains(season))) ?? false) ? "Strings\\StringsFromCSFiles:HoeDirt.cs.13924" : "Strings\\StringsFromCSFiles:HoeDirt.cs.13925");
			Game1.showRedMessage(Game1.content.LoadString(errorKey));
		}
		return false;
	}

	public void applySpeedIncreases(Farmer who)
	{
		if (this.crop == null)
		{
			return;
		}
		bool paddy_bonus = this.Location != null && this.paddyWaterCheck();
		float fertilizerSpeedBoost = this.GetFertilizerSpeedBoost();
		if (!(fertilizerSpeedBoost != 0f || who.professions.Contains(5) || paddy_bonus))
		{
			return;
		}
		this.crop.ResetPhaseDays();
		int totalDaysOfCropGrowth = 0;
		for (int i = 0; i < this.crop.phaseDays.Count - 1; i++)
		{
			totalDaysOfCropGrowth += this.crop.phaseDays[i];
		}
		float speedIncrease = fertilizerSpeedBoost;
		if (paddy_bonus)
		{
			speedIncrease += 0.25f;
		}
		if (who.professions.Contains(5))
		{
			speedIncrease += 0.1f;
		}
		int daysToRemove = (int)Math.Ceiling((float)totalDaysOfCropGrowth * speedIncrease);
		int tries = 0;
		while (daysToRemove > 0 && tries < 3)
		{
			for (int j = 0; j < this.crop.phaseDays.Count; j++)
			{
				if ((j > 0 || this.crop.phaseDays[j] > 1) && this.crop.phaseDays[j] != 99999 && this.crop.phaseDays[j] > 0)
				{
					this.crop.phaseDays[j]--;
					daysToRemove--;
				}
				if (daysToRemove <= 0)
				{
					break;
				}
			}
			tries++;
		}
	}

	public void destroyCrop(bool showAnimation)
	{
		GameLocation location = this.Location;
		if (this.crop != null && showAnimation && location != null)
		{
			Vector2 tileLocation = this.Tile;
			if (this.crop.currentPhase.Value < 1 && !this.crop.dead.Value)
			{
				Game1.multiplayer.broadcastSprites(Game1.player.currentLocation, new TemporaryAnimatedSprite(12, tileLocation * 64f, Color.White));
				location.playSound("dirtyHit", tileLocation);
			}
			else
			{
				Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(50, tileLocation * 64f, this.crop.dead.Value ? new Color(207, 193, 43) : Color.ForestGreen));
			}
		}
		this.crop = null;
		this.nearWaterForPaddy.Value = -1;
		if (location != null)
		{
			this.updateNeighbors();
		}
	}

	public override bool performToolAction(Tool t, int damage, Vector2 tileLocation)
	{
		GameLocation location = this.Location;
		if (t != null)
		{
			if (!(t is Hoe))
			{
				if (!(t is Pickaxe))
				{
					if (t is WateringCan)
					{
						if (this.crop == null || !this.crop.forageCrop.Value || this.crop.whichForageCrop.Value != "2")
						{
							this.state.Value = 1;
						}
						goto IL_0379;
					}
				}
				else if (this.crop == null)
				{
					return true;
				}
				if (t.isScythe())
				{
					Crop obj = this.crop;
					if ((obj != null && obj.GetHarvestMethod() == HarvestMethod.Scythe) || (this.crop != null && t.ItemId == "66"))
					{
						if (this.crop.indexOfHarvest.Value == "771" && t.hasEnchantmentOfType<HaymakerEnchantment>())
						{
							for (int i = 0; i < 2; i++)
							{
								Game1.createItemDebris(ItemRegistry.Create("(O)771"), new Vector2(tileLocation.X * 64f + 32f, tileLocation.Y * 64f + 32f), -1);
							}
						}
						if (this.crop.harvest((int)tileLocation.X, (int)tileLocation.Y, this, null, isForcedScytheHarvest: true))
						{
							if (location is IslandLocation && Game1.random.NextDouble() < 0.05)
							{
								Game1.player.team.RequestLimitedNutDrops("IslandFarming", location, (int)tileLocation.X * 64, (int)tileLocation.Y * 64, 5);
							}
							this.destroyCrop(showAnimation: true);
						}
					}
					if (this.crop != null && this.crop.dead.Value)
					{
						this.destroyCrop(showAnimation: true);
					}
					if (this.crop == null && t.ItemId == "66" && location.objects.TryGetValue(tileLocation, out var tileObj) && tileObj.isForage())
					{
						Farmer player = t.getLastFarmerToUse() ?? Game1.player;
						tileObj.Quality = location.GetHarvestSpawnedObjectQuality(player, tileObj.isForage(), tileObj.TileLocation);
						Vector2 spawnPosition = new Vector2(tileLocation.X * 64f + 32f, tileLocation.Y * 64f + 32f);
						Game1.createItemDebris(tileObj, spawnPosition, -1);
						location.OnHarvestedForage(player, tileObj);
						location.objects.Remove(tileLocation);
						if (player.professions.Contains(13) && Game1.random.NextDouble() < 0.2)
						{
							Object extraDrop = (Object)tileObj.getOne();
							extraDrop.Quality = location.GetHarvestSpawnedObjectQuality(player, extraDrop.isForage(), extraDrop.TileLocation);
							Game1.createItemDebris(extraDrop, spawnPosition, -1);
							location.OnHarvestedForage(player, extraDrop);
						}
					}
				}
				else if (t.isHeavyHitter() && !(t is MeleeWeapon) && this.crop != null)
				{
					this.destroyCrop(showAnimation: true);
				}
			}
			else if (this.crop != null && this.crop.hitWithHoe((int)tileLocation.X, (int)tileLocation.Y, location, this))
			{
				if (this.crop.forageCrop.Value && this.crop.whichForageCrop.Value == "2" && t.getLastFarmerToUse() != null)
				{
					t.getLastFarmerToUse().gainExperience(2, 7);
				}
				this.destroyCrop(showAnimation: true);
			}
			goto IL_0379;
		}
		if (damage > 0 && this.crop != null)
		{
			if (damage == 50)
			{
				this.crop.Kill();
			}
			else
			{
				this.destroyCrop(showAnimation: true);
			}
		}
		goto IL_03cd;
		IL_0379:
		this.shake((float)Math.PI / 32f, (float)Math.PI / 40f, tileLocation.X * 64f < Game1.player.Position.X);
		goto IL_03cd;
		IL_03cd:
		return false;
	}

	public bool canPlantThisSeedHere(string itemId, bool isFertilizer = false)
	{
		if (isFertilizer)
		{
			return this.CanApplyFertilizer(itemId);
		}
		if (this.crop == null)
		{
			Season season = this.Location.GetSeason();
			itemId = Crop.ResolveSeedId(itemId, this.Location);
			if (Crop.TryGetData(itemId, out var cropData))
			{
				if (cropData.Seasons.Count == 0)
				{
					return false;
				}
				if (!Game1.currentLocation.IsOutdoors || Game1.currentLocation.SeedsIgnoreSeasonsHere() || cropData.Seasons.Contains(season))
				{
					if (cropData.IsRaised && Utility.doesRectangleIntersectTile(Game1.player.GetBoundingBox(), (int)this.Tile.X, (int)this.Tile.Y))
					{
						return false;
					}
					return true;
				}
				switch (itemId)
				{
				case "309":
				case "310":
				case "311":
					return true;
				}
				if (Game1.didPlayerJustClickAtAll() && !Game1.doesHUDMessageExist(Game1.content.LoadString("Strings\\StringsFromCSFiles:HoeDirt.cs.13924")))
				{
					Game1.playSound("cancel");
					Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:HoeDirt.cs.13924"));
				}
			}
		}
		return false;
	}

	public override void performPlayerEntryAction()
	{
		base.performPlayerEntryAction();
		this.crop?.updateDrawMath(this.Tile);
	}

	public override bool tickUpdate(GameTime time)
	{
		foreach (Action<GameLocation, Vector2> queuedAction in this.queuedActions)
		{
			queuedAction(this.Location, this.Tile);
		}
		this.queuedActions.Clear();
		if (this.maxShake > 0f)
		{
			if (this.shakeLeft)
			{
				this.shakeRotation -= this.shakeRate;
				if (Math.Abs(this.shakeRotation) >= this.maxShake)
				{
					this.shakeLeft = false;
				}
			}
			else
			{
				this.shakeRotation += this.shakeRate;
				if (this.shakeRotation >= this.maxShake)
				{
					this.shakeLeft = true;
					this.shakeRotation -= this.shakeRate;
				}
			}
			this.maxShake = Math.Max(0f, this.maxShake - (float)Math.PI / 300f);
		}
		else
		{
			this.shakeRotation /= 2f;
			if (this.shakeRotation <= 0.01f)
			{
				base.NeedsUpdate = false;
				this.shakeRotation = 0f;
			}
		}
		if (this.state.Value == 2)
		{
			return this.crop == null;
		}
		return false;
	}

	/// <summary>Get whether this dirt contains a crop which should be planted near water.</summary>
	public bool hasPaddyCrop()
	{
		if (this.crop != null)
		{
			return this.crop.isPaddyCrop();
		}
		return false;
	}

	/// <summary>Get whether this is a paddy crop planted near water, so it should be watered automatically.</summary>
	/// <param name="forceUpdate">Whether to recheck the surrounding map area instead of using the cached value.</param>
	public bool paddyWaterCheck(bool forceUpdate = false)
	{
		if (!forceUpdate && this.nearWaterForPaddy.Value >= 0)
		{
			return this.nearWaterForPaddy.Value == 1;
		}
		if (!this.hasPaddyCrop())
		{
			this.nearWaterForPaddy.Value = 0;
			return false;
		}
		Vector2 tile_location = this.Tile;
		if (this.Location.getObjectAtTile((int)tile_location.X, (int)tile_location.Y) is IndoorPot)
		{
			this.nearWaterForPaddy.Value = 0;
			return false;
		}
		int range = 3;
		for (int x_offset = -range; x_offset <= range; x_offset++)
		{
			for (int y_offset = -range; y_offset <= range; y_offset++)
			{
				if (this.Location.isWaterTile((int)(tile_location.X + (float)x_offset), (int)(tile_location.Y + (float)y_offset)))
				{
					this.nearWaterForPaddy.Value = 1;
					return true;
				}
			}
		}
		this.nearWaterForPaddy.Value = 0;
		return false;
	}

	public override void dayUpdate()
	{
		GameLocation environment = this.Location;
		int num;
		if (this.hasPaddyCrop())
		{
			num = (this.paddyWaterCheck(forceUpdate: true) ? 1 : 0);
			if (num != 0 && this.state.Value == 0)
			{
				this.state.Value = 1;
			}
		}
		else
		{
			num = 0;
		}
		if (this.crop != null)
		{
			this.crop.newDay(this.state.Value);
			if (environment.isOutdoors.Value && environment.GetSeason() == Season.Winter && this.crop != null && !this.crop.isWildSeedCrop() && !this.crop.IsInSeason(environment))
			{
				this.destroyCrop(showAnimation: false);
			}
		}
		if (num == 0 && !Game1.random.NextBool(this.GetFertilizerWaterRetentionChance()))
		{
			this.state.Value = 0;
		}
		if (environment.IsGreenhouse)
		{
			this.c.Value = Color.White;
		}
	}

	/// <inheritdoc />
	public override bool seasonUpdate(bool onLoad)
	{
		GameLocation location = this.Location;
		if (!onLoad && !location.SeedsIgnoreSeasonsHere() && (this.crop == null || this.crop.dead.Value || !this.crop.IsInSeason(location)))
		{
			this.fertilizer.Value = null;
		}
		if (location.GetSeason() == Season.Fall && !location.IsGreenhouse)
		{
			this.c.Value = new Color(250, 210, 240);
		}
		else
		{
			this.c.Value = Color.White;
		}
		this.texture = null;
		return false;
	}

	public override void drawInMenu(SpriteBatch spriteBatch, Vector2 positionOnScreen, Vector2 tileLocation, float scale, float layerDepth)
	{
		byte drawSum = 0;
		Vector2 surroundingLocations = tileLocation;
		surroundingLocations.X += 1f;
		Farm farm = Game1.getFarm();
		if (farm.terrainFeatures.TryGetValue(surroundingLocations, out var rightFeature) && rightFeature is HoeDirt)
		{
			drawSum += 2;
		}
		surroundingLocations.X -= 2f;
		if (farm.terrainFeatures.TryGetValue(surroundingLocations, out var leftFeature) && leftFeature is HoeDirt)
		{
			drawSum += 8;
		}
		surroundingLocations.X += 1f;
		surroundingLocations.Y += 1f;
		if (Game1.currentLocation.terrainFeatures.TryGetValue(surroundingLocations, out var downFeature) && downFeature is HoeDirt)
		{
			drawSum += 4;
		}
		surroundingLocations.Y -= 2f;
		if (farm.terrainFeatures.TryGetValue(surroundingLocations, out var upFeature) && upFeature is HoeDirt)
		{
			drawSum++;
		}
		int sourceRectPosition = HoeDirt.drawGuide[drawSum];
		spriteBatch.Draw(HoeDirt.lightTexture, positionOnScreen, new Rectangle(sourceRectPosition % 4 * 64, sourceRectPosition / 4 * 64, 64, 64), Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, layerDepth + positionOnScreen.Y / 20000f);
		this.crop?.drawInMenu(spriteBatch, positionOnScreen + new Vector2(64f * scale, 64f * scale), Color.White, 0f, scale, layerDepth + (positionOnScreen.Y + 64f * scale) / 20000f);
	}

	public override void draw(SpriteBatch spriteBatch)
	{
		this.DrawOptimized(spriteBatch, spriteBatch, spriteBatch);
	}

	public void DrawOptimized(SpriteBatch dirt_batch, SpriteBatch fert_batch, SpriteBatch crop_batch)
	{
		int state = this.state.Value;
		Vector2 tileLocation = this.Tile;
		if (state != 2 && (dirt_batch != null || fert_batch != null))
		{
			if (dirt_batch != null && this.texture == null)
			{
				this.texture = ((Game1.currentLocation.Name.Equals("Mountain") || Game1.currentLocation.Name.Equals("Mine") || (Game1.currentLocation is MineShaft mine && mine.shouldShowDarkHoeDirt()) || Game1.currentLocation is VolcanoDungeon) ? HoeDirt.darkTexture : HoeDirt.lightTexture);
				if ((Game1.currentLocation.GetSeason() == Season.Winter && !Game1.currentLocation.SeedsIgnoreSeasonsHere() && !(Game1.currentLocation is MineShaft)) || (Game1.currentLocation is MineShaft shaft && shaft.shouldUseSnowTextureHoeDirt()))
				{
					this.texture = HoeDirt.snowTexture;
				}
			}
			Vector2 drawPos = Game1.GlobalToLocal(Game1.viewport, tileLocation * 64f);
			if (dirt_batch != null)
			{
				dirt_batch.Draw(this.texture, drawPos, new Rectangle(this.sourceRectPosition % 4 * 16, this.sourceRectPosition / 4 * 16, 16, 16), this.c.Value, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1E-08f);
				if (state == 1)
				{
					dirt_batch.Draw(this.texture, drawPos, new Rectangle(this.wateredRectPosition % 4 * 16 + (this.paddyWaterCheck() ? 128 : 64), this.wateredRectPosition / 4 * 16, 16, 16), this.c.Value, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1.2E-08f);
				}
			}
			if (fert_batch != null && this.HasFertilizer())
			{
				fert_batch.Draw(Game1.mouseCursors, drawPos, this.GetFertilizerSourceRect(), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1.9E-08f);
			}
		}
		if (this.crop != null && crop_batch != null)
		{
			this.crop.draw(crop_batch, tileLocation, (state == 1 && this.crop.currentPhase.Value == 0 && this.crop.shouldDrawDarkWhenWatered()) ? (new Color(180, 100, 200) * 1f) : Color.White, this.shakeRotation);
		}
	}

	/// <summary>Get whether the dirt has any fertilizer applied.</summary>
	public virtual bool HasFertilizer()
	{
		if (this.fertilizer.Value != null)
		{
			return this.fertilizer.Value != "0";
		}
		return false;
	}

	/// <summary>Get whether a player can apply the given fertilizer to this dirt.</summary>
	/// <param name="fertilizerId">The fertilizer item ID.</param>
	public virtual bool CanApplyFertilizer(string fertilizerId)
	{
		return this.CheckApplyFertilizerRules(fertilizerId) == HoeDirtFertilizerApplyStatus.Okay;
	}

	/// <summary>Get a status which indicates whether fertilizer can be applied to this dirt, and the reason it can't if applicable.</summary>
	/// <param name="fertilizerId">The fertilizer item ID.</param>
	public virtual HoeDirtFertilizerApplyStatus CheckApplyFertilizerRules(string fertilizerId)
	{
		if (this.HasFertilizer())
		{
			fertilizerId = ItemRegistry.QualifyItemId(fertilizerId);
			if (!(fertilizerId == ItemRegistry.QualifyItemId(this.fertilizer.Value)))
			{
				return HoeDirtFertilizerApplyStatus.HasAnotherFertilizer;
			}
			return HoeDirtFertilizerApplyStatus.HasThisFertilizer;
		}
		if (this.crop != null && this.crop.currentPhase.Value != 0 && (fertilizerId == "(O)368" || fertilizerId == "(O)369"))
		{
			return HoeDirtFertilizerApplyStatus.CropAlreadySprouted;
		}
		return HoeDirtFertilizerApplyStatus.Okay;
	}

	/// <summary>Get the crop growth speed boost from fertilizers applied to this dirt.</summary>
	public virtual float GetFertilizerSpeedBoost()
	{
		switch (this.fertilizer.Value)
		{
		case "465":
		case "(O)465":
			return 0.1f;
		case "466":
		case "(O)466":
			return 0.25f;
		case "918":
		case "(O)918":
			return 0.33f;
		default:
			return 0f;
		}
	}

	/// <summary>Get the water retention chance from fertilizers applied to this dirt, as a value between 0 (no change) and 1 (100% chance of staying watered).</summary>
	public virtual float GetFertilizerWaterRetentionChance()
	{
		switch (this.fertilizer.Value)
		{
		case "370":
		case "(O)370":
			return 0.33f;
		case "371":
		case "(O)371":
			return 0.66f;
		case "920":
		case "(O)920":
			return 1f;
		default:
			return 0f;
		}
	}

	/// <summary>Get the quality boost level from fertilizers applied to this dirt, which influences the chance of producing a higher-quality crop.</summary>
	/// <remarks>See <see cref="M:StardewValley.Crop.harvest(System.Int32,System.Int32,StardewValley.TerrainFeatures.HoeDirt,StardewValley.Characters.JunimoHarvester,System.Boolean)" /> for the quality boost logic.</remarks>
	public virtual int GetFertilizerQualityBoostLevel()
	{
		switch (this.fertilizer.Value)
		{
		case "368":
		case "(O)368":
			return 1;
		case "369":
		case "(O)369":
			return 2;
		case "919":
		case "(O)919":
			return 3;
		default:
			return 0;
		}
	}

	/// <summary>Get the pixel area within the dirt spritesheet to draw for any fertilizer applied to this dirt.</summary>
	public virtual Rectangle GetFertilizerSourceRect()
	{
		string value = this.fertilizer.Value;
		if (value != null)
		{
			int length = value.Length;
			if (length != 3)
			{
				if (length == 6)
				{
					switch (value[5])
					{
					case '9':
						break;
					case '0':
						goto IL_015c;
					case '1':
						goto IL_0178;
					case '5':
						goto IL_0187;
					case '6':
						goto IL_0196;
					case '8':
						goto IL_01a5;
					default:
						goto IL_01d4;
					}
					if (value == "(O)369")
					{
						goto IL_01b4;
					}
					if (value == "(O)919")
					{
						goto IL_01d0;
					}
				}
			}
			else
			{
				switch (value[2])
				{
				case '9':
					break;
				case '0':
					goto IL_00c7;
				case '1':
					goto IL_00ec;
				case '5':
					goto IL_0101;
				case '6':
					goto IL_0116;
				case '8':
					goto IL_012b;
				default:
					goto IL_01d4;
				}
				if (value == "369")
				{
					goto IL_01b4;
				}
				if (value == "919")
				{
					goto IL_01d0;
				}
			}
		}
		goto IL_01d4;
		IL_0196:
		if (value == "(O)466")
		{
			goto IL_01c8;
		}
		goto IL_01d4;
		IL_01a5:
		if (value == "(O)918")
		{
			goto IL_01cc;
		}
		goto IL_01d4;
		IL_00c7:
		if (value == "370")
		{
			goto IL_01b8;
		}
		if (value == "920")
		{
			goto IL_01c0;
		}
		goto IL_01d4;
		IL_01d6:
		int fertilizerIndex;
		return new Rectangle(173 + fertilizerIndex / 3 * 16, 462 + fertilizerIndex % 3 * 16, 16, 16);
		IL_00ec:
		if (value == "371")
		{
			goto IL_01bc;
		}
		goto IL_01d4;
		IL_0101:
		if (value == "465")
		{
			goto IL_01c4;
		}
		goto IL_01d4;
		IL_0116:
		if (value == "466")
		{
			goto IL_01c8;
		}
		goto IL_01d4;
		IL_012b:
		if (value == "918")
		{
			goto IL_01cc;
		}
		goto IL_01d4;
		IL_01d0:
		fertilizerIndex = 2;
		goto IL_01d6;
		IL_01d4:
		fertilizerIndex = 0;
		goto IL_01d6;
		IL_01c8:
		fertilizerIndex = 7;
		goto IL_01d6;
		IL_01cc:
		fertilizerIndex = 8;
		goto IL_01d6;
		IL_015c:
		if (value == "(O)370")
		{
			goto IL_01b8;
		}
		if (value == "(O)920")
		{
			goto IL_01c0;
		}
		goto IL_01d4;
		IL_01b8:
		fertilizerIndex = 3;
		goto IL_01d6;
		IL_01b4:
		fertilizerIndex = 1;
		goto IL_01d6;
		IL_01c0:
		fertilizerIndex = 5;
		goto IL_01d6;
		IL_0178:
		if (value == "(O)371")
		{
			goto IL_01bc;
		}
		goto IL_01d4;
		IL_01bc:
		fertilizerIndex = 4;
		goto IL_01d6;
		IL_0187:
		if (value == "(O)465")
		{
			goto IL_01c4;
		}
		goto IL_01d4;
		IL_01c4:
		fertilizerIndex = 6;
		goto IL_01d6;
	}

	private List<Neighbor> gatherNeighbors()
	{
		List<Neighbor> results = this._neighbors;
		results.Clear();
		if (this.Pot == null)
		{
			GameLocation location = this.Location;
			Vector2 tilePos = this.Tile;
			NetVector2Dictionary<TerrainFeature, NetRef<TerrainFeature>> terrainFeatures = location.terrainFeatures;
			NeighborLoc[] offsets = HoeDirt._offsets;
			for (int i = 0; i < offsets.Length; i++)
			{
				NeighborLoc item = offsets[i];
				Vector2 tile = tilePos + item.Offset;
				if (terrainFeatures.TryGetValue(tile, out var feature) && feature is HoeDirt dirt && dirt.state.Value != 2)
				{
					Neighbor n = new Neighbor(dirt, item.Direction, item.InvDirection);
					results.Add(n);
				}
			}
		}
		return results;
	}

	public void updateNeighbors()
	{
		if (this.Location == null)
		{
			return;
		}
		List<Neighbor> list = this.gatherNeighbors();
		this.neighborMask = 0;
		this.wateredNeighborMask = 0;
		foreach (Neighbor n in list)
		{
			this.neighborMask |= n.direction;
			if (this.state.Value != 2)
			{
				n.feature.OnNeighborAdded(n.invDirection, this.state.Value);
			}
			if (this.isWatered() && n.feature.isWatered())
			{
				if (n.feature.paddyWaterCheck() == this.paddyWaterCheck())
				{
					this.wateredNeighborMask |= n.direction;
					n.feature.wateredNeighborMask |= n.invDirection;
				}
				else
				{
					n.feature.wateredNeighborMask = (byte)(n.feature.wateredNeighborMask & ~n.invDirection);
				}
			}
			n.feature.UpdateDrawSums();
		}
		this.UpdateDrawSums();
	}

	public void OnAdded(GameLocation loc, Vector2 tilePos)
	{
		this.Location = loc;
		this.Tile = tilePos;
		this.updateNeighbors();
	}

	public void OnRemoved()
	{
		if (this.Location == null)
		{
			return;
		}
		List<Neighbor> list = this.gatherNeighbors();
		this.neighborMask = 0;
		this.wateredNeighborMask = 0;
		foreach (Neighbor n in list)
		{
			n.feature.OnNeighborRemoved(n.invDirection);
			if (this.isWatered())
			{
				n.feature.wateredNeighborMask = (byte)(n.feature.wateredNeighborMask & ~n.invDirection);
			}
			n.feature.UpdateDrawSums();
		}
		this.UpdateDrawSums();
	}

	public virtual void UpdateDrawSums()
	{
		this.drawSum = (byte)(this.neighborMask & 0xF);
		this.sourceRectPosition = HoeDirt.drawGuide[this.drawSum];
		this.wateredRectPosition = HoeDirt.drawGuide[this.wateredNeighborMask];
	}

	/// <summary>Called when a neighbor is added or changed.</summary>
	/// <param name="direction">The direction from this dirt to the one which changed.</param>
	/// <param name="neighborState">The water state for the neighbor which changed.</param>
	public void OnNeighborAdded(byte direction, int neighborState)
	{
		this.neighborMask |= direction;
		if (neighborState == 1)
		{
			this.wateredNeighborMask |= direction;
		}
		else
		{
			this.wateredNeighborMask = (byte)(this.wateredNeighborMask & ~direction);
		}
	}

	/// <summary>Called when a neighbor is removed.</summary>
	/// <param name="direction">The direction from this dirt to the one which was removed.</param>
	public void OnNeighborRemoved(byte direction)
	{
		this.neighborMask = (byte)(this.neighborMask & ~direction);
		this.wateredNeighborMask = (byte)(this.wateredNeighborMask & ~direction);
	}
}
