using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.BellsAndWhistles;
using StardewValley.Constants;
using StardewValley.Enchantments;
using StardewValley.Extensions;
using StardewValley.GameData.WildTrees;
using StardewValley.Internal;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Locations;
using StardewValley.Tools;
using xTile.Dimensions;

namespace StardewValley.TerrainFeatures;

public class Tree : TerrainFeature
{
	/// <remarks>The backing field for <see cref="M:StardewValley.TerrainFeatures.Tree.GetWildTreeDataDictionary" />.</remarks>
	protected static Dictionary<string, WildTreeData> _WildTreeData;

	/// <summary>The backing field for <see cref="M:StardewValley.TerrainFeatures.Tree.GetWildTreeSeedLookup" />.</summary>
	protected static Dictionary<string, List<string>> _WildTreeSeedLookup;

	public const float chanceForDailySeed = 0.05f;

	public const float shakeRate = (float)Math.PI / 200f;

	public const float shakeDecayRate = 0.0030679617f;

	public const int minWoodDebrisForFallenTree = 12;

	public const int minWoodDebrisForStump = 5;

	public const int startingHealth = 10;

	public const int leafFallRate = 3;

	public const int stageForMossGrowth = 14;

	/// <summary>The oak tree type ID in <c>Data/WildTrees</c>.</summary>
	public const string bushyTree = "1";

	/// <summary>The maple tree type ID in <c>Data/WildTrees</c>.</summary>
	public const string leafyTree = "2";

	/// <summary>The pine tree type ID in <c>Data/WildTrees</c>.</summary>
	public const string pineTree = "3";

	public const string winterTree1 = "4";

	public const string winterTree2 = "5";

	/// <summary>The palm tree type ID (valley variant) in <c>Data/WildTrees</c>.</summary>
	public const string palmTree = "6";

	/// <summary>The mushroom tree type ID in <c>Data/WildTrees</c>.</summary>
	public const string mushroomTree = "7";

	/// <summary>The mahogany tree type ID in <c>Data/WildTrees</c>.</summary>
	public const string mahoganyTree = "8";

	/// <summary>The palm tree type ID (Ginger Island variant) in <c>Data/WildTrees</c>.</summary>
	public const string palmTree2 = "9";

	public const string greenRainTreeBushy = "10";

	public const string greenRainTreeLeafy = "11";

	public const string greenRainTreeFern = "12";

	public const string mysticTree = "13";

	public const int seedStage = 0;

	public const int sproutStage = 1;

	public const int saplingStage = 2;

	public const int bushStage = 3;

	public const int treeStage = 5;

	/// <summary>The texture for the displayed tree sprites.</summary>
	[XmlIgnore]
	public Lazy<Texture2D> texture;

	/// <summary>The current season for the location containing the tree.</summary>
	protected Season? localSeason;

	[XmlElement("growthStage")]
	public readonly NetInt growthStage = new NetInt();

	[XmlElement("treeType")]
	public readonly NetString treeType = new NetString();

	[XmlElement("health")]
	public readonly NetFloat health = new NetFloat();

	[XmlElement("flipped")]
	public readonly NetBool flipped = new NetBool();

	[XmlElement("stump")]
	public readonly NetBool stump = new NetBool();

	[XmlElement("tapped")]
	public readonly NetBool tapped = new NetBool();

	[XmlElement("hasSeed")]
	public readonly NetBool hasSeed = new NetBool();

	[XmlElement("hasMoss")]
	public readonly NetBool hasMoss = new NetBool();

	[XmlElement("isTemporaryGreenRainTree")]
	public readonly NetBool isTemporaryGreenRainTree = new NetBool();

	[XmlIgnore]
	public readonly NetBool wasShakenToday = new NetBool();

	[XmlElement("fertilized")]
	public readonly NetBool fertilized = new NetBool();

	[XmlIgnore]
	public readonly NetBool shakeLeft = new NetBool().Interpolated(interpolate: false, wait: false);

	[XmlIgnore]
	public readonly NetBool falling = new NetBool();

	[XmlIgnore]
	public readonly NetBool destroy = new NetBool();

	[XmlIgnore]
	public float shakeRotation;

	[XmlIgnore]
	public float maxShake;

	[XmlIgnore]
	public float alpha = 1f;

	private List<Leaf> leaves = new List<Leaf>();

	[XmlIgnore]
	public readonly NetLong lastPlayerToHit = new NetLong();

	[XmlIgnore]
	public float shakeTimer;

	[XmlElement("stopGrowingMoss")]
	public readonly NetBool stopGrowingMoss = new NetBool();

	public static Microsoft.Xna.Framework.Rectangle treeTopSourceRect = new Microsoft.Xna.Framework.Rectangle(0, 0, 48, 96);

	public static Microsoft.Xna.Framework.Rectangle stumpSourceRect = new Microsoft.Xna.Framework.Rectangle(32, 96, 16, 32);

	public static Microsoft.Xna.Framework.Rectangle shadowSourceRect = new Microsoft.Xna.Framework.Rectangle(663, 1011, 41, 30);

	/// <summary>The asset name for the texture loaded by <see cref="F:StardewValley.TerrainFeatures.Tree.texture" />, if applicable.</summary>
	[XmlIgnore]
	public string TextureName { get; private set; }

	public Tree()
		: base(needsTick: true)
	{
		this.resetTexture();
	}

	public Tree(string id, int growthStage, bool isGreenRainTemporaryTree = false)
		: this()
	{
		this.growthStage.Value = growthStage;
		this.isTemporaryGreenRainTree.Value = isGreenRainTemporaryTree;
		this.treeType.Value = id;
		if (this.treeType.Value == "4")
		{
			this.treeType.Value = "1";
		}
		if (this.treeType.Value == "5")
		{
			this.treeType.Value = "2";
		}
		this.flipped.Value = Game1.random.NextBool();
		this.health.Value = 10f;
	}

	public Tree(string id)
		: this()
	{
		this.treeType.Value = id;
		if (this.treeType.Value == "4")
		{
			this.treeType.Value = "1";
		}
		if (this.treeType.Value == "5")
		{
			this.treeType.Value = "2";
		}
		this.flipped.Value = Game1.random.NextBool();
		this.health.Value = 10f;
	}

	public override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(this.growthStage, "growthStage").AddField(this.treeType, "treeType").AddField(this.health, "health")
			.AddField(this.flipped, "flipped")
			.AddField(this.stump, "stump")
			.AddField(this.tapped, "tapped")
			.AddField(this.hasSeed, "hasSeed")
			.AddField(this.fertilized, "fertilized")
			.AddField(this.shakeLeft, "shakeLeft")
			.AddField(this.falling, "falling")
			.AddField(this.destroy, "destroy")
			.AddField(this.lastPlayerToHit, "lastPlayerToHit")
			.AddField(this.wasShakenToday, "wasShakenToday")
			.AddField(this.hasMoss, "hasMoss")
			.AddField(this.isTemporaryGreenRainTree, "isTemporaryGreenRainTree")
			.AddField(this.stopGrowingMoss, "stopGrowingMoss");
		this.treeType.fieldChangeVisibleEvent += delegate
		{
			this.CheckForNewTexture();
		};
	}

	/// <summary>Get the wild tree data from <c>Data/WildTrees</c>.</summary>
	/// <remarks>This is a specialized method; most code should use <see cref="M:StardewValley.TerrainFeatures.Tree.GetData" /> or <see cref="M:StardewValley.TerrainFeatures.Tree.TryGetData(System.String,StardewValley.GameData.WildTrees.WildTreeData@)" /> instead.</remarks>
	public static Dictionary<string, WildTreeData> GetWildTreeDataDictionary()
	{
		if (Tree._WildTreeData == null)
		{
			Tree._LoadWildTreeData();
		}
		return Tree._WildTreeData;
	}

	/// <summary>Get tree types indexed by their qualified and unqualified seed item IDs.</summary>
	public static Dictionary<string, List<string>> GetWildTreeSeedLookup()
	{
		if (Tree._WildTreeSeedLookup == null)
		{
			Tree._LoadWildTreeData();
		}
		return Tree._WildTreeSeedLookup;
	}

	/// <summary>Load the raw wild tree data from <c>Data/WildTrees</c>.</summary>
	/// <remarks>This generally shouldn't be called directly; most code should use <see cref="M:StardewValley.TerrainFeatures.Tree.GetWildTreeDataDictionary" /> or <see cref="M:StardewValley.TerrainFeatures.Tree.GetWildTreeSeedLookup" /> instead.</remarks>
	protected static void _LoadWildTreeData()
	{
		Tree._WildTreeData = DataLoader.WildTrees(Game1.content);
		Tree._WildTreeSeedLookup = new Dictionary<string, List<string>>();
		foreach (KeyValuePair<string, WildTreeData> pair in Tree._WildTreeData)
		{
			string treeId = pair.Key;
			WildTreeData treeData = pair.Value;
			if (!treeData.SeedPlantable || string.IsNullOrWhiteSpace(treeData.SeedItemId))
			{
				continue;
			}
			ItemMetadata seedData = ItemRegistry.ResolveMetadata(treeData.SeedItemId);
			if (seedData != null)
			{
				if (!Tree._WildTreeSeedLookup.TryGetValue(seedData.QualifiedItemId, out var itemIds))
				{
					itemIds = (Tree._WildTreeSeedLookup[seedData.QualifiedItemId] = new List<string>());
				}
				itemIds.Add(treeId);
				if (!Tree._WildTreeSeedLookup.TryGetValue(seedData.LocalItemId, out itemIds))
				{
					itemIds = (Tree._WildTreeSeedLookup[seedData.LocalItemId] = new List<string>());
				}
				itemIds.Add(treeId);
			}
		}
	}

	/// <summary>Get the next tree that will sprout when planting a seed item.</summary>
	/// <param name="itemId">The seed's qualified or unqualified item ID.</param>
	public static string ResolveTreeTypeFromSeed(string itemId)
	{
		ItemMetadata metadata = ItemRegistry.GetMetadata(itemId);
		if (metadata?.TypeIdentifier == "(O)" && Tree.GetWildTreeSeedLookup().TryGetValue(metadata.LocalItemId, out var possibles))
		{
			return Utility.CreateRandom(Game1.uniqueIDForThisGame, Game1.stats.Get("wildtreesplanted") + 1).ChooseFrom(possibles);
		}
		return null;
	}

	/// <summary>Reset the cached wild tree data, so it's reloaded on the next request.</summary>
	internal static void ClearCache()
	{
		Tree._WildTreeData = null;
		Tree._WildTreeSeedLookup = null;
	}

	/// <summary>Reload the tree texture based on <see cref="F:StardewValley.GameData.WildTrees.WildTreeData.Textures" /> if a different texture would be selected now.</summary>
	public void CheckForNewTexture()
	{
		if (this.texture.IsValueCreated)
		{
			string textureName = this.ChooseTexture();
			if (textureName != null && textureName != this.TextureName)
			{
				this.resetTexture();
			}
		}
	}

	/// <summary>Reset the tree texture, so it'll be reselected and reloaded next time it's accessed.</summary>
	public void resetTexture()
	{
		this.texture = new Lazy<Texture2D>(LoadTexture);
		Texture2D LoadTexture()
		{
			this.TextureName = this.ChooseTexture();
			if (this.TextureName == null)
			{
				return null;
			}
			return Game1.content.Load<Texture2D>(this.TextureName);
		}
	}

	/// <summary>Get the tree's data from <c>Data/WildTrees</c>, if found.</summary>
	public WildTreeData GetData()
	{
		if (!Tree.TryGetData(this.treeType.Value, out var data))
		{
			return null;
		}
		return data;
	}

	/// <summary>Try to get a tree's data from <c>Data/WildTrees</c>.</summary>
	/// <param name="id">The tree type ID (i.e. the key in <c>Data/WildTrees</c>).</param>
	/// <param name="data">The tree data, if found.</param>
	/// <returns>Returns whether the tree data was found.</returns>
	public static bool TryGetData(string id, out WildTreeData data)
	{
		if (id == null)
		{
			data = null;
			return false;
		}
		return Tree.GetWildTreeDataDictionary().TryGetValue(id, out data);
	}

	/// <summary>Choose an applicable texture from <see cref="F:StardewValley.GameData.WildTrees.WildTreeData.Textures" />.</summary>
	protected string ChooseTexture()
	{
		WildTreeData data = this.GetData();
		if (data != null && data.Textures?.Count > 0)
		{
			foreach (WildTreeTextureData entry in data.Textures)
			{
				if (this.Location != null && this.Location.IsGreenhouse && entry.Season.HasValue)
				{
					if (entry.Season == Season.Spring)
					{
						return entry.Texture;
					}
				}
				else if ((!entry.Season.HasValue || entry.Season == this.localSeason) && (entry.Condition == null || GameStateQuery.CheckConditions(entry.Condition, this.Location)))
				{
					return entry.Texture;
				}
			}
			return data.Textures[0].Texture;
		}
		return null;
	}

	public override Microsoft.Xna.Framework.Rectangle getBoundingBox()
	{
		Vector2 tileLocation = this.Tile;
		return new Microsoft.Xna.Framework.Rectangle((int)tileLocation.X * 64, (int)tileLocation.Y * 64, 64, 64);
	}

	public override Microsoft.Xna.Framework.Rectangle getRenderBounds()
	{
		Vector2 tileLocation = this.Tile;
		if (this.stump.Value || this.growthStage.Value < 5)
		{
			return new Microsoft.Xna.Framework.Rectangle((int)(tileLocation.X - 0f) * 64, (int)(tileLocation.Y - 1f) * 64, 64, 128);
		}
		return new Microsoft.Xna.Framework.Rectangle((int)(tileLocation.X - 1f) * 64, (int)(tileLocation.Y - 5f) * 64, 192, 448);
	}

	public override bool performUseAction(Vector2 tileLocation)
	{
		GameLocation location = this.Location;
		if (!this.tapped.Value)
		{
			if (this.maxShake == 0f && !this.stump.Value && this.growthStage.Value >= 3 && this.IsLeafy())
			{
				location.localSound("leafrustle");
			}
			this.shake(tileLocation, doEvenIfStillShaking: false);
		}
		if (Game1.player.ActiveObject != null && Game1.player.ActiveObject.canBePlacedHere(location, tileLocation))
		{
			return false;
		}
		return true;
	}

	private int extraWoodCalculator(Vector2 tileLocation)
	{
		Random random = Utility.CreateRandom(Game1.uniqueIDForThisGame, Game1.stats.DaysPlayed, (double)tileLocation.X * 7.0, (double)tileLocation.Y * 11.0);
		int extraWood = 0;
		if (random.NextDouble() < Game1.player.DailyLuck)
		{
			extraWood++;
		}
		if (random.NextDouble() < (double)Game1.player.ForagingLevel / 12.5)
		{
			extraWood++;
		}
		if (random.NextDouble() < (double)Game1.player.ForagingLevel / 12.5)
		{
			extraWood++;
		}
		if (random.NextDouble() < (double)Game1.player.LuckLevel / 25.0)
		{
			extraWood++;
		}
		if (this.treeType.Value == "3")
		{
			extraWood++;
		}
		return extraWood;
	}

	public override bool tickUpdate(GameTime time)
	{
		GameLocation location = this.Location;
		Season? season = this.localSeason;
		if (!season.HasValue)
		{
			this.setSeason();
			this.CheckForNewTexture();
		}
		if (this.shakeTimer > 0f)
		{
			this.shakeTimer -= time.ElapsedGameTime.Milliseconds;
		}
		if (this.destroy.Value)
		{
			return true;
		}
		this.alpha = Math.Min(1f, this.alpha + 0.05f);
		Vector2 tileLocation = this.Tile;
		if (this.growthStage.Value >= 5 && !this.falling.Value && !this.stump.Value && Game1.player.GetBoundingBox().Intersects(new Microsoft.Xna.Framework.Rectangle(64 * ((int)tileLocation.X - 1), 64 * ((int)tileLocation.Y - 5), 192, 288)))
		{
			this.alpha = Math.Max(0.4f, this.alpha - 0.09f);
		}
		if (!this.falling.Value)
		{
			if ((double)Math.Abs(this.shakeRotation) > Math.PI / 2.0 && this.leaves.Count <= 0 && this.health.Value <= 0f)
			{
				return true;
			}
			if (this.maxShake > 0f)
			{
				if (this.shakeLeft.Value)
				{
					this.shakeRotation -= ((this.growthStage.Value >= 5) ? 0.005235988f : ((float)Math.PI / 200f));
					if (this.shakeRotation <= 0f - this.maxShake)
					{
						this.shakeLeft.Value = false;
					}
				}
				else
				{
					this.shakeRotation += ((this.growthStage.Value >= 5) ? 0.005235988f : ((float)Math.PI / 200f));
					if (this.shakeRotation >= this.maxShake)
					{
						this.shakeLeft.Value = true;
					}
				}
			}
			if (this.maxShake > 0f)
			{
				this.maxShake = Math.Max(0f, this.maxShake - ((this.growthStage.Value >= 5) ? 0.0010226539f : 0.0030679617f));
			}
		}
		else
		{
			this.shakeRotation += (this.shakeLeft.Value ? (0f - this.maxShake * this.maxShake) : (this.maxShake * this.maxShake));
			this.maxShake += 0.0015339808f;
			WildTreeData data = this.GetData();
			if (data != null && Game1.random.NextDouble() < 0.01 && this.IsLeafy())
			{
				location.localSound("leafrustle");
			}
			if ((double)Math.Abs(this.shakeRotation) > Math.PI / 2.0)
			{
				this.falling.Value = false;
				this.maxShake = 0f;
				if (data != null)
				{
					location.localSound("treethud");
					if (this.IsLeafy())
					{
						int leavesToAdd = Game1.random.Next(90, 120);
						for (int i = 0; i < leavesToAdd; i++)
						{
							this.leaves.Add(new Leaf(new Vector2(Game1.random.Next((int)(tileLocation.X * 64f), (int)(tileLocation.X * 64f + 192f)) + (this.shakeLeft.Value ? (-320) : 256), tileLocation.Y * 64f - 64f), (float)Game1.random.Next(-10, 10) / 100f, Game1.random.Next(4), (float)Game1.random.Next(10, 40) / 10f));
						}
					}
					Random r;
					if (Game1.IsMultiplayer)
					{
						Game1.recentMultiplayerRandom = Utility.CreateRandom((double)tileLocation.X * 1000.0, tileLocation.Y);
						r = Game1.recentMultiplayerRandom;
					}
					else
					{
						r = Utility.CreateRandom(Game1.uniqueIDForThisGame, Game1.stats.DaysPlayed, (double)tileLocation.X * 7.0, (double)tileLocation.Y * 11.0);
					}
					Farmer lastHitBy = Game1.GetPlayer(this.lastPlayerToHit.Value) ?? Game1.MasterPlayer;
					if (data.DropWoodOnChop)
					{
						int numToDrop = (int)((lastHitBy.professions.Contains(12) ? 1.25 : 1.0) * (double)(12 + this.extraWoodCalculator(tileLocation)));
						if (lastHitBy.stats.Get("Book_Woodcutting") != 0 && r.NextDouble() < 0.05)
						{
							numToDrop *= 2;
						}
						Game1.createRadialDebris(location, 12, (int)tileLocation.X + (this.shakeLeft.Value ? (-4) : 4), (int)tileLocation.Y, numToDrop, resource: true);
						Game1.createRadialDebris(location, 12, (int)tileLocation.X + (this.shakeLeft.Value ? (-4) : 4), (int)tileLocation.Y, (int)((lastHitBy.professions.Contains(12) ? 1.25 : 1.0) * (double)(12 + this.extraWoodCalculator(tileLocation))), resource: false);
					}
					if (data.DropWoodOnChop)
					{
						Game1.createMultipleObjectDebris("(O)92", (int)tileLocation.X + (this.shakeLeft.Value ? (-4) : 4), (int)tileLocation.Y, 5, this.lastPlayerToHit.Value, location);
					}
					int numHardwood = 0;
					if (data.DropHardwoodOnLumberChop)
					{
						while (lastHitBy.professions.Contains(14) && r.NextBool())
						{
							numHardwood++;
						}
					}
					List<WildTreeChopItemData> chopItems = data.ChopItems;
					if (chopItems != null && chopItems.Count > 0)
					{
						bool addedAdditionalHardwood = false;
						foreach (WildTreeChopItemData drop in data.ChopItems)
						{
							Item item = this.TryGetDrop(drop, r, lastHitBy, "ChopItems", null, false);
							if (item != null)
							{
								if (drop.ItemId == "709")
								{
									numHardwood += item.Stack;
									addedAdditionalHardwood = true;
								}
								else
								{
									Game1.createMultipleItemDebris(item, new Vector2(tileLocation.X + (float)(this.shakeLeft.Value ? (-4) : 4), tileLocation.Y) * 64f, -2, location);
								}
							}
						}
						if (addedAdditionalHardwood && lastHitBy.professions.Contains(14))
						{
							numHardwood += (int)((float)numHardwood * 0.25f + 0.9f);
						}
					}
					if (numHardwood > 0)
					{
						Game1.createMultipleObjectDebris("(O)709", (int)tileLocation.X + (this.shakeLeft.Value ? (-4) : 4), (int)tileLocation.Y, numHardwood, this.lastPlayerToHit.Value, location);
					}
					float seedOnChopChance = data.SeedOnChopChance;
					if (lastHitBy.getEffectiveSkillLevel(2) >= 1 && data != null && data.SeedItemId != null && r.NextDouble() < (double)seedOnChopChance)
					{
						Game1.createMultipleObjectDebris(data.SeedItemId, (int)tileLocation.X + (this.shakeLeft.Value ? (-4) : 4), (int)tileLocation.Y, r.Next(1, 3), this.lastPlayerToHit.Value, location);
					}
				}
				if (this.health.Value == -100f)
				{
					return true;
				}
				if (this.health.Value <= 0f)
				{
					this.health.Value = -100f;
				}
			}
		}
		for (int i2 = this.leaves.Count - 1; i2 >= 0; i2--)
		{
			Leaf leaf = this.leaves[i2];
			leaf.position.Y -= leaf.yVelocity - 3f;
			leaf.yVelocity = Math.Max(0f, leaf.yVelocity - 0.01f);
			leaf.rotation += leaf.rotationRate;
			if (leaf.position.Y >= tileLocation.Y * 64f + 64f)
			{
				this.leaves.RemoveAt(i2);
			}
		}
		return false;
	}

	/// <summary>Get a dropped item if its fields match.</summary>
	/// <param name="drop">The drop data.</param>
	/// <param name="r">The RNG to use for random checks.</param>
	/// <param name="targetFarmer">The player interacting with the tree.</param>
	/// <param name="fieldName">The field name to show in error messages if the drop is invalid.</param>
	/// <param name="formatItemId">Format the selected item ID before it's resolved.</param>
	/// <param name="isStump">Whether the tree is a stump, or <c>null</c> to use <see cref="F:StardewValley.TerrainFeatures.Tree.stump" />.</param>
	/// <returns>Returns the produced item (if any), else <c>null</c>.</returns>
	public Item TryGetDrop(WildTreeItemData drop, Random r, Farmer targetFarmer, string fieldName, Func<string, string> formatItemId = null, bool? isStump = null)
	{
		if (!r.NextBool(drop.Chance))
		{
			return null;
		}
		if (drop.Season.HasValue && drop.Season != this.Location.GetSeason())
		{
			return null;
		}
		if (drop.Condition != null && !GameStateQuery.CheckConditions(drop.Condition, this.Location, targetFarmer, null, null, r))
		{
			return null;
		}
		if (drop is WildTreeChopItemData chopItemData && !chopItemData.IsValidForGrowthStage(this.growthStage.Value, isStump ?? this.stump.Value))
		{
			return null;
		}
		return ItemQueryResolver.TryResolveRandomItem(drop, new ItemQueryContext(this.Location, targetFarmer, r, $"wild tree '{this.treeType.Value}' > {fieldName} entry '{drop.Id}'"), avoidRepeat: false, null, formatItemId, null, delegate(string query, string error)
		{
			Game1.log.Error($"Wild tree '{this.treeType.Value}' failed parsing item query '{query}' for {fieldName} entry '{drop.Id}': {error}");
		});
	}

	public void shake(Vector2 tileLocation, bool doEvenIfStillShaking)
	{
		GameLocation location = this.Location;
		WildTreeData data = this.GetData();
		if ((this.maxShake == 0f || doEvenIfStillShaking) && this.growthStage.Value >= 3 && !this.stump.Value)
		{
			this.shakeLeft.Value = (float)Game1.player.StandingPixel.X > (tileLocation.X + 0.5f) * 64f || (Game1.player.Tile.X == tileLocation.X && Game1.random.NextBool());
			this.maxShake = (float)((this.growthStage.Value >= 5) ? (Math.PI / 128.0) : (Math.PI / 64.0));
			if (this.growthStage.Value >= 5)
			{
				if (this.IsLeafy())
				{
					if (Game1.random.NextDouble() < 0.66)
					{
						int numberOfLeaves = Game1.random.Next(1, 6);
						for (int i = 0; i < numberOfLeaves; i++)
						{
							this.leaves.Add(new Leaf(new Vector2(Game1.random.Next((int)(tileLocation.X * 64f - 64f), (int)(tileLocation.X * 64f + 128f)), Game1.random.Next((int)(tileLocation.Y * 64f - 256f), (int)(tileLocation.Y * 64f - 192f))), (float)Game1.random.Next(-10, 10) / 100f, Game1.random.Next(4), (float)Game1.random.Next(5) / 10f));
						}
					}
					if (Game1.random.NextDouble() < 0.01 && (this.localSeason == Season.Spring || this.localSeason == Season.Summer))
					{
						bool isIslandButterfly = this.Location.InIslandContext();
						while (Game1.random.NextDouble() < 0.8)
						{
							location.addCritter(new Butterfly(location, new Vector2(tileLocation.X + (float)Game1.random.Next(1, 3), tileLocation.Y - 2f + (float)Game1.random.Next(-1, 2)), isIslandButterfly));
						}
					}
				}
				if (this.hasSeed.Value && (Game1.IsMultiplayer || Game1.player.ForagingLevel >= 1))
				{
					bool dropDefaultSeed = true;
					if (data != null && data.SeedDropItems?.Count > 0)
					{
						foreach (WildTreeSeedDropItemData drop in data.SeedDropItems)
						{
							Item seed = this.TryGetDrop(drop, Game1.random, Game1.player, "SeedDropItems");
							if (seed != null)
							{
								if (Game1.player.professions.Contains(16) && seed.HasContextTag("forage_item"))
								{
									seed.Quality = 4;
								}
								Game1.createItemDebris(seed, new Vector2(tileLocation.X * 64f, (tileLocation.Y - 3f) * 64f), -1, location, Game1.player.StandingPixel.Y);
								if (!drop.ContinueOnDrop)
								{
									dropDefaultSeed = false;
									break;
								}
							}
						}
					}
					if (dropDefaultSeed && data != null)
					{
						Item seed2 = ItemRegistry.Create(data.SeedItemId);
						if (Game1.player.professions.Contains(16) && seed2.HasContextTag("forage_item"))
						{
							seed2.Quality = 4;
						}
						Game1.createItemDebris(seed2, new Vector2(tileLocation.X * 64f, (tileLocation.Y - 3f) * 64f), -1, location, Game1.player.StandingPixel.Y);
					}
					if (Utility.tryRollMysteryBox(0.03))
					{
						Game1.createItemDebris(ItemRegistry.Create((Game1.player.stats.Get(StatKeys.Mastery(2)) != 0) ? "(O)GoldenMysteryBox" : "(O)MysteryBox"), new Vector2(tileLocation.X, tileLocation.Y - 3f) * 64f, -1, location, Game1.player.StandingPixel.Y - 32);
					}
					Utility.trySpawnRareObject(Game1.player, new Vector2(tileLocation.X, tileLocation.Y - 3f) * 64f, this.Location, 2.0, 1.0, Game1.player.StandingPixel.Y - 32);
					if (Game1.random.NextBool() && Game1.player.team.SpecialOrderRuleActive("DROP_QI_BEANS"))
					{
						Game1.createObjectDebris("(O)890", (int)tileLocation.X, (int)tileLocation.Y - 3, ((int)tileLocation.Y + 1) * 64, 0, 1f, location);
					}
					this.hasSeed.Value = false;
				}
				if (this.wasShakenToday.Value)
				{
					return;
				}
				this.wasShakenToday.Value = true;
				if (data?.ShakeItems == null)
				{
					return;
				}
				{
					foreach (WildTreeItemData entry in data.ShakeItems)
					{
						Item item = this.TryGetDrop(entry, Game1.random, Game1.player, "ShakeItems");
						if (item != null)
						{
							Game1.createItemDebris(item, tileLocation * 64f, -2, this.Location);
						}
					}
					return;
				}
			}
			if (Game1.random.NextDouble() < 0.66)
			{
				int numberOfLeaves2 = Game1.random.Next(1, 3);
				for (int j = 0; j < numberOfLeaves2; j++)
				{
					this.leaves.Add(new Leaf(new Vector2(Game1.random.Next((int)(tileLocation.X * 64f), (int)(tileLocation.X * 64f + 48f)), tileLocation.Y * 64f - 32f), (float)Game1.random.Next(-10, 10) / 100f, Game1.random.Next(4), (float)Game1.random.Next(30) / 10f));
				}
			}
		}
		else if (this.stump.Value)
		{
			this.shakeTimer = 100f;
		}
	}

	/// <inheritdoc />
	public override bool isPassable(Character c = null)
	{
		if (!(this.health.Value <= -99f))
		{
			return this.growthStage.Value == 0;
		}
		return true;
	}

	/// <summary>Get the maximum size the tree can grow in its current position.</summary>
	/// <param name="ignoreSeason">Whether to assume the tree is in-season.</param>
	public virtual int GetMaxSizeHere(bool ignoreSeason = false)
	{
		GameLocation location = this.Location;
		Vector2 tile = this.Tile;
		if (this.GetData() == null)
		{
			return this.growthStage.Value;
		}
		if (location.IsNoSpawnTile(tile, "Tree") && !location.doesEitherTileOrTileIndexPropertyEqual((int)tile.X, (int)tile.Y, "CanPlantTrees", "Back", "T"))
		{
			return this.growthStage.Value;
		}
		if (!ignoreSeason && !this.IsInSeason())
		{
			return this.growthStage.Value;
		}
		if (this.growthStage.Value == 0 && location.objects.ContainsKey(tile))
		{
			return 0;
		}
		if (this.IsGrowthBlockedByNearbyTree())
		{
			return 4;
		}
		return 15;
	}

	/// <summary>Get whether this tree is in-season for its current location, so it can grow if applicable.</summary>
	public bool IsInSeason()
	{
		if (this.localSeason == Season.Winter && !this.fertilized.Value && !this.Location.SeedsIgnoreSeasonsHere())
		{
			return this.GetData()?.GrowsInWinter ?? false;
		}
		return true;
	}

	/// <summary>Get whether growth is blocked because it's too close to another fully-grown tree.</summary>
	public bool IsGrowthBlockedByNearbyTree()
	{
		GameLocation location = this.Location;
		Vector2 tile = this.Tile;
		Microsoft.Xna.Framework.Rectangle growthRect = new Microsoft.Xna.Framework.Rectangle((int)((tile.X - 1f) * 64f), (int)((tile.Y - 1f) * 64f), 192, 192);
		foreach (KeyValuePair<Vector2, TerrainFeature> other in location.terrainFeatures.Pairs)
		{
			if (other.Key != tile && other.Value is Tree otherTree && otherTree.growthStage.Value >= 5 && otherTree.getBoundingBox().Intersects(growthRect))
			{
				return true;
			}
		}
		return false;
	}

	public void onGreenRainDay(bool undo = false)
	{
		if (undo)
		{
			if (this.isTemporaryGreenRainTree.Value)
			{
				this.isTemporaryGreenRainTree.Value = false;
				if (this.treeType.Value == "10")
				{
					this.treeType.Value = "1";
				}
				else
				{
					this.treeType.Value = "2";
				}
				this.resetTexture();
			}
		}
		else
		{
			if (this.Location == null || !this.Location.IsOutdoors)
			{
				return;
			}
			if (this.growthStage.Value < 5)
			{
				if (this.growthStage.Value == 0 && (Game1.random.NextDouble() < 0.5 || this.Location == null || this.Location.objects.ContainsKey(this.Tile)))
				{
					return;
				}
				this.growthStage.Value = 4;
				for (int i = 0; i < 3; i++)
				{
					this.dayUpdate();
				}
			}
			bool? flag = this.GetData()?.GrowsMoss;
			if (flag.HasValue && flag == true && Game1.random.NextBool())
			{
				this.hasMoss.Value = true;
			}
			if ((this.treeType.Value == "1" || this.treeType.Value == "2") && this.growthStage.Value >= 5 && Game1.random.NextBool(0.75))
			{
				this.isTemporaryGreenRainTree.Value = true;
				if (this.treeType.Value == "1")
				{
					this.treeType.Value = "10";
				}
				else
				{
					this.treeType.Value = "11";
				}
				this.resetTexture();
			}
		}
	}

	public override void dayUpdate()
	{
		GameLocation environment = this.Location;
		if (!Game1.IsFall && !Game1.IsWinter)
		{
			GameLocation location = this.Location;
			if ((location == null || !location.IsGreenRainingHere()) && this.isTemporaryGreenRainTree.Value)
			{
				this.isTemporaryGreenRainTree.Value = false;
				if (this.treeType.Value == "10")
				{
					this.treeType.Value = "1";
				}
				else
				{
					this.treeType.Value = "2";
				}
				this.resetTexture();
			}
		}
		this.wasShakenToday.Value = false;
		this.setSeason();
		this.CheckForNewTexture();
		WildTreeData data = this.GetData();
		Vector2 tile = this.Tile;
		if (this.health.Value <= -100f)
		{
			this.destroy.Value = true;
		}
		if (this.tapped.Value)
		{
			Object tile_object = environment.getObjectAtTile((int)tile.X, (int)tile.Y);
			if (tile_object == null || !tile_object.IsTapper())
			{
				this.tapped.Value = false;
			}
			else if (tile_object.IsTapper() && tile_object.heldObject.Value == null)
			{
				this.UpdateTapperProduct(tile_object);
			}
		}
		if (this.GetMaxSizeHere() > this.growthStage.Value)
		{
			float chance = data?.GrowthChance ?? 0.2f;
			float fertilizedGrowthChance = data?.FertilizedGrowthChance ?? 1f;
			if (Game1.random.NextBool(chance) || (this.fertilized.Value && Game1.random.NextBool(fertilizedGrowthChance)))
			{
				this.growthStage.Value++;
			}
		}
		if (this.localSeason == Season.Winter && data != null && data.IsStumpDuringWinter && !this.Location.SeedsIgnoreSeasonsHere())
		{
			this.stump.Value = true;
		}
		else if (data != null && data.IsStumpDuringWinter && Game1.dayOfMonth <= 1 && Game1.IsSpring)
		{
			this.stump.Value = false;
			this.health.Value = 10f;
			this.shakeRotation = 0f;
		}
		if (this.growthStage.Value >= 5 && !this.stump.Value && environment is Farm && Game1.random.NextBool(data?.SeedSpreadChance ?? 0.15f))
		{
			int xCoord = Game1.random.Next(-3, 4) + (int)tile.X;
			int yCoord = Game1.random.Next(-3, 4) + (int)tile.Y;
			Vector2 location2 = new Vector2(xCoord, yCoord);
			if (!environment.IsNoSpawnTile(location2, "Tree") && environment.isTileLocationOpen(new Location(xCoord, yCoord)) && !environment.IsTileOccupiedBy(location2) && !environment.isWaterTile(xCoord, yCoord) && environment.isTileOnMap(location2))
			{
				environment.terrainFeatures.Add(location2, new Tree(this.treeType.Value, 0));
			}
		}
		if (this.isTemporaryGreenRainTree.Value && environment.IsGreenhouse && (this.localSeason == Season.Winter || this.localSeason == Season.Fall))
		{
			this.hasSeed.Value = false;
		}
		else
		{
			this.hasSeed.Value = data != null && data.SeedItemId != null && this.growthStage.Value >= 5 && Game1.random.NextBool(data.SeedOnShakeChance);
		}
		bool accelerateMoss = this.growthStage.Value >= 5 && !Game1.IsWinter && (this.treeType.Value == "10" || this.treeType.Value == "11") && !this.isTemporaryGreenRainTree.Value;
		if (this.growthStage.Value >= 5 && !Game1.IsWinter && !accelerateMoss)
		{
			for (int x = (int)tile.X - 2; (float)x <= tile.X + 2f; x++)
			{
				for (int y = (int)tile.Y - 2; (float)y <= tile.Y + 2f; y++)
				{
					Vector2 v = new Vector2(x, y);
					if (this.Location.terrainFeatures.GetValueOrDefault(v) is Tree tree && tree.growthStage.Value >= 5 && (tree.treeType.Value == "10" || tree.treeType.Value == "11") && !tree.isTemporaryGreenRainTree.Value && tree.hasMoss.Value)
					{
						accelerateMoss = true;
						break;
					}
				}
				if (accelerateMoss)
				{
					break;
				}
			}
		}
		float mossChance = (Game1.isRaining ? 0.2f : 0.1f);
		if (accelerateMoss && Game1.random.NextDouble() < 0.5)
		{
			this.growthStage.Value++;
		}
		if (Game1.IsSummer && !Game1.isGreenRain && !Game1.isRaining)
		{
			mossChance = 0.033f;
		}
		if (accelerateMoss && Game1.random.NextDouble() < 0.5)
		{
			mossChance += 0.1f;
		}
		if (this.stopGrowingMoss.Value)
		{
			this.hasMoss.Value = false;
			return;
		}
		if (!environment.IsGreenhouse && (this.localSeason == Season.Winter || this.stump.Value))
		{
			this.hasMoss.Value = false;
			return;
		}
		bool? flag = data?.GrowsMoss;
		if (flag.HasValue && flag == true && this.growthStage.Value >= 14 && !this.stump.Value && Game1.random.NextBool(mossChance))
		{
			this.hasMoss.Value = true;
		}
	}

	public override void performPlayerEntryAction()
	{
		base.performPlayerEntryAction();
		this.setSeason();
		this.CheckForNewTexture();
	}

	/// <inheritdoc />
	public override bool seasonUpdate(bool onLoad)
	{
		if (!onLoad && Game1.IsFall && Game1.random.NextDouble() < 0.05 && !this.tapped.Value && (this.treeType.Value == "1" || this.treeType.Value == "2") && this.growthStage.Value >= 5 && this.Location != null && !(this.Location is Town) && !this.Location.IsGreenhouse)
		{
			this.treeType.Value = ((this.treeType.Value == "1") ? "10" : "11");
			this.isTemporaryGreenRainTree.Value = true;
			this.resetTexture();
		}
		if (this.tapped.Value && this.Location != null)
		{
			Object tileObject = this.Location.getObjectAtTile((int)this.Tile.X, (int)this.Tile.Y);
			if (tileObject != null && tileObject.IsTapper())
			{
				this.UpdateTapperProduct(tileObject, null, onlyPerformRemovals: true);
			}
		}
		this.loadSprite();
		return false;
	}

	public override bool isActionable()
	{
		if (!this.tapped.Value)
		{
			return this.growthStage.Value >= 3;
		}
		return false;
	}

	public virtual bool IsLeafy()
	{
		WildTreeData data = this.GetData();
		if (data != null && data.IsLeafy)
		{
			if (data.IsLeafyInWinter || !this.Location.IsWinterHere())
			{
				if (!data.IsLeafyInFall)
				{
					return !this.Location.IsFallHere();
				}
				return true;
			}
			return false;
		}
		return false;
	}

	/// <summary>Get the color of the cosmetic wood chips when chopping the tree.</summary>
	public Color? GetChopDebrisColor()
	{
		return this.GetChopDebrisColor(this.GetData());
	}

	/// <summary>Get the color of the cosmetic wood chips when chopping the tree.</summary>
	/// <param name="data">The wild tree data to read.</param>
	public Color? GetChopDebrisColor(WildTreeData data)
	{
		string rawColor = data?.DebrisColor;
		if (rawColor == null)
		{
			return null;
		}
		if (!int.TryParse(rawColor, out var debrisType))
		{
			return Utility.StringToColor(rawColor);
		}
		return Debris.getColorForDebris(debrisType);
	}

	public override bool performToolAction(Tool t, int explosion, Vector2 tileLocation)
	{
		GameLocation location = this.Location ?? Game1.currentLocation;
		if (explosion > 0)
		{
			this.tapped.Value = false;
		}
		if (this.health.Value <= -99f)
		{
			return false;
		}
		if (this.growthStage.Value >= 5)
		{
			if (this.hasMoss.Value)
			{
				Item moss = Tree.CreateMossItem();
				if (t?.getLastFarmerToUse() != null)
				{
					t.getLastFarmerToUse().gainExperience(2, moss.Stack);
				}
				this.hasMoss.Value = false;
				Game1.createMultipleItemDebris(moss, new Vector2(tileLocation.X, tileLocation.Y - 1f) * 64f, -1, location, Game1.player.StandingPixel.Y - 32);
				Game1.stats.Increment("mossHarvested");
				this.shake(tileLocation, doEvenIfStillShaking: true);
				this.growthStage.Value = 12 - moss.Stack;
				Game1.playSound("moss_cut");
				for (int i = 0; i < 6; i++)
				{
					location.temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\debris", new Microsoft.Xna.Framework.Rectangle(Game1.random.Choose(16, 0), 96, 16, 16), new Vector2(tileLocation.X + (float)Game1.random.NextDouble() - 0.15f, tileLocation.Y - 1f + (float)Game1.random.NextDouble()) * 64f, flipped: false, 0.025f, Color.Green)
					{
						drawAboveAlwaysFront = true,
						motion = new Vector2((float)Game1.random.Next(-10, 11) / 10f, -4f),
						acceleration = new Vector2(0f, 0.3f + (float)Game1.random.Next(-10, 11) / 200f),
						animationLength = 1,
						interval = 1000f,
						sourceRectStartingPos = new Vector2(0f, 96f),
						alpha = 1f,
						layerDepth = 1f,
						scale = 4f
					});
				}
			}
			if (this.tapped.Value)
			{
				return false;
			}
			if (t is Axe)
			{
				location.playSound("axchop", tileLocation);
				this.lastPlayerToHit.Value = t.getLastFarmerToUse().UniqueMultiplayerID;
				location.debris.Add(new Debris(12, Game1.random.Next(1, 3), t.getLastFarmerToUse().GetToolLocation() + new Vector2(16f, 0f), t.getLastFarmerToUse().Position, 0, this.GetChopDebrisColor()));
				if (location is Town && tileLocation.X < 100f && !this.isTemporaryGreenRainTree.Value)
				{
					int pathsIndex = location.getTileIndexAt((int)tileLocation.X, (int)tileLocation.Y, "Paths");
					if (pathsIndex == 9 || pathsIndex == 10 || pathsIndex == 11)
					{
						this.shake(tileLocation, doEvenIfStillShaking: true);
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:TownTreeWarning"));
						return false;
					}
				}
				if (!this.stump.Value && t.getLastFarmerToUse() != null && location.HasUnlockedAreaSecretNotes(t.getLastFarmerToUse()) && Game1.random.NextDouble() < 0.005)
				{
					Object o = location.tryToCreateUnseenSecretNote(t.getLastFarmerToUse());
					if (o != null)
					{
						Game1.createItemDebris(o, new Vector2(tileLocation.X, tileLocation.Y - 3f) * 64f, -1, location, Game1.player.StandingPixel.Y - 32);
					}
				}
				else if (!this.stump.Value && t.getLastFarmerToUse() != null && Utility.tryRollMysteryBox(0.005))
				{
					Game1.createItemDebris(ItemRegistry.Create((t.getLastFarmerToUse().stats.Get(StatKeys.Mastery(2)) != 0) ? "(O)GoldenMysteryBox" : "(O)MysteryBox"), new Vector2(tileLocation.X, tileLocation.Y - 3f) * 64f, -1, location, Game1.player.StandingPixel.Y - 32);
				}
				else if (!this.stump.Value && t.getLastFarmerToUse() != null && t.getLastFarmerToUse().stats.Get("TreesChopped") > 20 && Game1.random.NextDouble() < 0.0003 + (t.getLastFarmerToUse().mailReceived.Contains("GotWoodcuttingBook") ? 0.0007 : ((double)t.getLastFarmerToUse().stats.Get("TreesChopped") * 1E-05)))
				{
					Game1.createItemDebris(ItemRegistry.Create("(O)Book_Woodcutting"), new Vector2(tileLocation.X, tileLocation.Y - 3f) * 64f, -1, location, Game1.player.StandingPixel.Y - 32);
					t.getLastFarmerToUse().mailReceived.Add("GotWoodcuttingBook");
				}
				else if (!this.stump.Value)
				{
					Utility.trySpawnRareObject(Game1.player, new Vector2(tileLocation.X, tileLocation.Y - 3f) * 64f, this.Location, 0.33, 1.0, Game1.player.StandingPixel.Y - 32);
				}
			}
			else if (explosion <= 0)
			{
				return false;
			}
			this.shake(tileLocation, doEvenIfStillShaking: true);
			float damage;
			if (explosion > 0)
			{
				damage = explosion;
				if (location is Town && tileLocation.X < 100f)
				{
					return false;
				}
			}
			else
			{
				if (t == null)
				{
					return false;
				}
				damage = t.upgradeLevel.Value switch
				{
					0 => 1f, 
					1 => 1.25f, 
					2 => 1.67f, 
					3 => 2.5f, 
					4 => 5f, 
					_ => t.upgradeLevel.Value + 1, 
				};
			}
			if (t is Axe && t.hasEnchantmentOfType<ShavingEnchantment>() && Game1.random.NextDouble() <= (double)(damage / 5f))
			{
				Debris d = this.treeType.Value switch
				{
					"12" => new Debris("(O)259", new Vector2(tileLocation.X * 64f + 32f, (tileLocation.Y - 0.5f) * 64f + 32f), Game1.player.getStandingPosition()), 
					"7" => new Debris("(O)420", new Vector2(tileLocation.X * 64f + 32f, (tileLocation.Y - 0.5f) * 64f + 32f), Game1.player.getStandingPosition()), 
					"8" => new Debris("(O)709", new Vector2(tileLocation.X * 64f + 32f, (tileLocation.Y - 0.5f) * 64f + 32f), Game1.player.getStandingPosition()), 
					_ => new Debris("388", new Vector2(tileLocation.X * 64f + 32f, (tileLocation.Y - 0.5f) * 64f + 32f), Game1.player.getStandingPosition()), 
				};
				d.Chunks[0].xVelocity.Value += (float)Game1.random.Next(-10, 11) / 10f;
				d.chunkFinalYLevel = (int)(tileLocation.Y * 64f + 64f);
				location.debris.Add(d);
			}
			this.health.Value -= damage;
			if (this.health.Value <= 0f && this.performTreeFall(t, explosion, tileLocation))
			{
				return true;
			}
		}
		else if (this.growthStage.Value >= 3)
		{
			if (t != null && t.Name.Contains("Ax"))
			{
				location.playSound("axchop", tileLocation);
				if (this.IsLeafy())
				{
					location.playSound("leafrustle");
				}
				location.debris.Add(new Debris(12, Game1.random.Next(t.upgradeLevel.Value * 2, t.upgradeLevel.Value * 4), t.getLastFarmerToUse().GetToolLocation() + new Vector2(16f, 0f), Utility.PointToVector2(t.getLastFarmerToUse().StandingPixel), 0));
			}
			else if (explosion <= 0)
			{
				return false;
			}
			this.shake(tileLocation, doEvenIfStillShaking: true);
			float damage2 = 1f;
			damage2 = ((explosion > 0) ? ((float)explosion) : (t.upgradeLevel.Value switch
			{
				0 => 2f, 
				1 => 2.5f, 
				2 => 3.34f, 
				3 => 5f, 
				4 => 10f, 
				_ => 10 + (t.upgradeLevel.Value - 4), 
			}));
			this.health.Value -= damage2;
			if (this.health.Value <= 0f)
			{
				this.performBushDestroy(tileLocation);
				return true;
			}
		}
		else if (this.growthStage.Value >= 1)
		{
			if (explosion > 0)
			{
				location.playSound("cut");
				return true;
			}
			if (t != null && t.Name.Contains("Axe"))
			{
				location.playSound("axchop", tileLocation);
				Game1.createRadialDebris(location, 12, (int)tileLocation.X, (int)tileLocation.Y, Game1.random.Next(10, 20), resource: false);
			}
			if (t is Axe || t is Pickaxe || t is Hoe || t is MeleeWeapon)
			{
				location.playSound("cut");
				this.performSproutDestroy(t, tileLocation);
				return true;
			}
		}
		else
		{
			if (explosion > 0)
			{
				return true;
			}
			if (t.Name.Contains("Axe") || t.Name.Contains("Pick") || t.Name.Contains("Hoe"))
			{
				location.playSound("woodyHit", tileLocation);
				location.playSound("axchop", tileLocation);
				this.performSeedDestroy(t, tileLocation);
				return true;
			}
		}
		return false;
	}

	public static Item CreateMossItem()
	{
		Random rand = Utility.CreateRandom(Game1.uniqueIDForThisGame, Game1.stats.Get("mossHarvested") * 50);
		return ItemRegistry.Create("(O)Moss", rand.Next(1, 3));
	}

	public bool fertilize()
	{
		GameLocation location = this.Location;
		if (this.growthStage.Value >= 5)
		{
			Game1.showRedMessageUsingLoadString("Strings\\StringsFromCSFiles:TreeFertilizer1");
			location.playSound("cancel");
			return false;
		}
		if (this.fertilized.Value)
		{
			Game1.showRedMessageUsingLoadString("Strings\\StringsFromCSFiles:TreeFertilizer2");
			location.playSound("cancel");
			return false;
		}
		this.fertilized.Value = true;
		location.playSound("dirtyHit");
		return true;
	}

	public bool instantDestroy(Vector2 tileLocation)
	{
		if (this.growthStage.Value >= 5)
		{
			return this.performTreeFall(null, 0, tileLocation);
		}
		if (this.growthStage.Value >= 3)
		{
			this.performBushDestroy(tileLocation);
			return true;
		}
		if (this.growthStage.Value >= 1)
		{
			this.performSproutDestroy(null, tileLocation);
			return true;
		}
		this.performSeedDestroy(null, tileLocation);
		return true;
	}

	protected void performSeedDestroy(Tool t, Vector2 tileLocation)
	{
		GameLocation location = this.Location;
		Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(17, tileLocation * 64f, Color.White));
		WildTreeData data = this.GetData();
		if (data != null && data.SeedItemId != null)
		{
			Farmer lastHitBy = Game1.GetPlayer(this.lastPlayerToHit.Value) ?? Game1.MasterPlayer;
			if (this.lastPlayerToHit.Value != 0L && lastHitBy.getEffectiveSkillLevel(2) >= 1)
			{
				Game1.createMultipleObjectDebris(data.SeedItemId, (int)tileLocation.X, (int)tileLocation.Y, 1, t.getLastFarmerToUse().UniqueMultiplayerID, location);
			}
			else if (Game1.player.getEffectiveSkillLevel(2) >= 1)
			{
				Game1.createMultipleObjectDebris(data.SeedItemId, (int)tileLocation.X, (int)tileLocation.Y, 1, t?.getLastFarmerToUse().UniqueMultiplayerID ?? Game1.player.UniqueMultiplayerID, location);
			}
		}
	}

	/// <summary>Update the attached tapper's held output.</summary>
	/// <param name="tapper">The attached tapper instance.</param>
	/// <param name="previousOutput">The previous item produced by the tapper, if any.</param>
	public void UpdateTapperProduct(Object tapper, Object previousOutput = null, bool onlyPerformRemovals = false)
	{
		if (tapper == null)
		{
			return;
		}
		WildTreeData data = this.GetData();
		if (data == null)
		{
			return;
		}
		float timeMultiplier = 1f;
		foreach (string contextTag in tapper.GetContextTags())
		{
			if (contextTag.StartsWithIgnoreCase("tapper_multiplier_") && float.TryParse(contextTag.Substring("tapper_multiplier_".Length), out var multiplier))
			{
				timeMultiplier = 1f / multiplier;
				break;
			}
		}
		Random random = Utility.CreateRandom(Game1.uniqueIDForThisGame, Game1.stats.DaysPlayed, 73137.0, (double)this.Tile.X * 9.0, (double)this.Tile.Y * 13.0);
		if (this.TryGetTapperOutput(data.TapItems, previousOutput?.ItemId, random, timeMultiplier, out var output, out var minutesUntilReady) && (!onlyPerformRemovals || output == null))
		{
			tapper.heldObject.Value = output;
			tapper.minutesUntilReady.Value = minutesUntilReady;
		}
	}

	/// <summary>Get a valid item that can be produced by the tree's current tapper.</summary>
	/// <param name="tapItems">The tap item data to choose from.</param>
	/// <param name="previousItemId">The previous item ID that was produced.</param>
	/// <param name="r">The RNG with which to randomize.</param>
	/// <param name="timeMultiplier">A multiplier to apply to the minutes until ready.</param>
	/// <param name="output">The possible tapper output.</param>
	/// <param name="minutesUntilReady">The number of minutes until the tapper would produce the output.</param>
	protected bool TryGetTapperOutput(List<WildTreeTapItemData> tapItems, string previousItemId, Random r, float timeMultiplier, out Object output, out int minutesUntilReady)
	{
		if (tapItems != null)
		{
			previousItemId = ((previousItemId != null) ? ItemRegistry.QualifyItemId(previousItemId) : null);
			foreach (WildTreeTapItemData tapData in tapItems)
			{
				if (!GameStateQuery.CheckConditions(tapData.Condition, this.Location))
				{
					continue;
				}
				if (tapData.PreviousItemId != null)
				{
					bool found = false;
					foreach (string expectedPrevId in tapData.PreviousItemId)
					{
						found = (string.IsNullOrEmpty(expectedPrevId) ? (previousItemId == null) : previousItemId.EqualsIgnoreCase(ItemRegistry.QualifyItemId(expectedPrevId)));
						if (found)
						{
							break;
						}
					}
					if (!found)
					{
						continue;
					}
				}
				if (tapData.Season.HasValue && tapData.Season != this.localSeason)
				{
					continue;
				}
				Farmer lastHitBy = Game1.GetPlayer(this.lastPlayerToHit.Value) ?? Game1.MasterPlayer;
				Item item = this.TryGetDrop(tapData, r, lastHitBy, "TapItems", (string id) => id.Replace("PREVIOUS_OUTPUT_ID", previousItemId));
				if (item != null)
				{
					if (item is Object obj)
					{
						int daysUntilReady = (int)Utility.ApplyQuantityModifiers(tapData.DaysUntilReady, tapData.DaysUntilReadyModifiers, tapData.DaysUntilReadyModifierMode, this.Location, Game1.player);
						output = obj;
						minutesUntilReady = Utility.CalculateMinutesUntilMorning(Game1.timeOfDay, (int)Math.Max(1.0, Math.Floor((float)daysUntilReady * timeMultiplier)));
						return true;
					}
					Game1.log.Warn($"Wild tree '{this.treeType.Value}' can't produce item '{item.ItemId}': must be an object-type item.");
				}
			}
			if (previousItemId != null)
			{
				return this.TryGetTapperOutput(tapItems, null, r, timeMultiplier, out output, out minutesUntilReady);
			}
		}
		output = null;
		minutesUntilReady = 0;
		return false;
	}

	protected void performSproutDestroy(Tool t, Vector2 tileLocation)
	{
		GameLocation location = this.Location;
		Game1.createRadialDebris(location, 12, (int)tileLocation.X, (int)tileLocation.Y, Game1.random.Next(10, 20), resource: false);
		if (t != null && t.Name.Contains("Axe") && Game1.recentMultiplayerRandom.NextDouble() < (double)((float)t.getLastFarmerToUse().ForagingLevel / 10f))
		{
			Game1.createDebris(12, (int)tileLocation.X, (int)tileLocation.Y, 1);
		}
		Game1.multiplayer.broadcastSprites(location, new TemporaryAnimatedSprite(17, tileLocation * 64f, Color.White));
	}

	protected void performBushDestroy(Vector2 tileLocation)
	{
		GameLocation location = this.Location;
		WildTreeData data = this.GetData();
		if (data == null)
		{
			return;
		}
		Farmer lastHitBy = Game1.GetPlayer(this.lastPlayerToHit.Value) ?? Game1.MasterPlayer;
		Game1.createRadialDebris(location, 12, (int)tileLocation.X, (int)tileLocation.Y, Game1.random.Next(20, 30), resource: false, -1, item: false, this.GetChopDebrisColor(data));
		if (data.DropWoodOnChop || data.DropHardwoodOnLumberChop)
		{
			Game1.createDebris(12, (int)tileLocation.X, (int)tileLocation.Y, (int)((lastHitBy.professions.Contains(12) ? 1.25 : 1.0) * 4.0), location);
		}
		List<WildTreeChopItemData> chopItems = data.ChopItems;
		if (chopItems == null || chopItems.Count <= 0)
		{
			return;
		}
		Random r;
		if (Game1.IsMultiplayer)
		{
			Game1.recentMultiplayerRandom = Utility.CreateRandom((double)tileLocation.X * 1000.0, tileLocation.Y);
			r = Game1.recentMultiplayerRandom;
		}
		else
		{
			r = Utility.CreateRandom(Game1.uniqueIDForThisGame, Game1.stats.DaysPlayed, (double)tileLocation.X * 7.0, (double)tileLocation.Y * 11.0);
		}
		foreach (WildTreeChopItemData drop in data.ChopItems)
		{
			Item item = this.TryGetDrop(drop, r, lastHitBy, "ChopItems");
			if (item != null)
			{
				Game1.createMultipleItemDebris(item, tileLocation * 64f, -2, location);
			}
		}
	}

	protected bool performTreeFall(Tool t, int explosion, Vector2 tileLocation)
	{
		GameLocation location = this.Location;
		WildTreeData data = this.GetData();
		this.Location.objects.Remove(this.Tile);
		this.tapped.Value = false;
		if (!this.stump.Value)
		{
			if (t != null || explosion > 0)
			{
				location.playSound("treecrack");
			}
			this.stump.Value = true;
			this.health.Value = 5f;
			this.falling.Value = true;
			if (t != null && t.getLastFarmerToUse().IsLocalPlayer)
			{
				t?.getLastFarmerToUse().gainExperience(2, 14);
				if (t?.getLastFarmerToUse() == null)
				{
					this.shakeLeft.Value = true;
				}
				else
				{
					this.shakeLeft.Value = (float)t.getLastFarmerToUse().StandingPixel.X > (tileLocation.X + 0.5f) * 64f;
				}
				t.getLastFarmerToUse().stats.Increment("TreesChopped", 1);
			}
		}
		else
		{
			if (t != null && this.health.Value != -100f && t.getLastFarmerToUse().IsLocalPlayer)
			{
				t?.getLastFarmerToUse().gainExperience(2, 2);
			}
			this.health.Value = -100f;
			if (data != null)
			{
				Game1.createRadialDebris(location, 12, (int)tileLocation.X, (int)tileLocation.Y, Game1.random.Next(30, 40), resource: false, -1, item: false, this.GetChopDebrisColor(data));
				Random r;
				if (Game1.IsMultiplayer)
				{
					Game1.recentMultiplayerRandom = Utility.CreateRandom((double)tileLocation.X * 2000.0, tileLocation.Y);
					r = Game1.recentMultiplayerRandom;
				}
				else
				{
					r = Utility.CreateRandom(Game1.uniqueIDForThisGame, Game1.stats.DaysPlayed, (double)tileLocation.X * 7.0, (double)tileLocation.Y * 11.0);
				}
				if (t?.getLastFarmerToUse() == null)
				{
					if (location.Equals(Game1.currentLocation))
					{
						Game1.createMultipleObjectDebris("(O)92", (int)tileLocation.X, (int)tileLocation.Y, 2, location);
					}
					else
					{
						for (int i = 0; i < 2; i++)
						{
							Game1.createItemDebris(ItemRegistry.Create("(O)92"), tileLocation * 64f, 2, location);
						}
					}
				}
				else
				{
					Farmer lastHitBy = Game1.GetPlayer(this.lastPlayerToHit.Value) ?? Game1.MasterPlayer;
					if (Game1.IsMultiplayer)
					{
						if (data.DropWoodOnChop)
						{
							Game1.createRadialDebris(location, 12, (int)tileLocation.X, (int)tileLocation.Y, (int)((lastHitBy.professions.Contains(12) ? 1.25 : 1.0) * 4.0), resource: true);
						}
						List<WildTreeChopItemData> chopItems = data.ChopItems;
						if (chopItems != null && chopItems.Count > 0)
						{
							foreach (WildTreeChopItemData drop in data.ChopItems)
							{
								Item item = this.TryGetDrop(drop, r, lastHitBy, "ChopItems");
								if (item != null)
								{
									if (item.QualifiedItemId == "(O)420" && tileLocation.X % 7f == 0f)
									{
										item = ItemRegistry.Create("(O)422", item.Stack, item.Quality);
									}
									Game1.createMultipleItemDebris(item, tileLocation * 64f, -2, location);
								}
							}
						}
					}
					else
					{
						if (data.DropWoodOnChop)
						{
							Game1.createRadialDebris(location, 12, (int)tileLocation.X, (int)tileLocation.Y, (int)((lastHitBy.professions.Contains(12) ? 1.25 : 1.0) * (double)(5 + this.extraWoodCalculator(tileLocation))), resource: true);
						}
						List<WildTreeChopItemData> chopItems2 = data.ChopItems;
						if (chopItems2 != null && chopItems2.Count > 0)
						{
							foreach (WildTreeChopItemData drop2 in data.ChopItems)
							{
								Item item2 = this.TryGetDrop(drop2, r, lastHitBy, "ChopItems");
								if (item2 != null)
								{
									if (item2.QualifiedItemId == "(O)420" && tileLocation.X % 7f == 0f)
									{
										item2 = ItemRegistry.Create("(O)422", item2.Stack, item2.Quality);
									}
									Game1.createMultipleItemDebris(item2, tileLocation * 64f, -2, location);
								}
							}
						}
					}
				}
				if (Game1.random.NextDouble() <= 0.25 && Game1.player.team.SpecialOrderRuleActive("DROP_QI_BEANS"))
				{
					Game1.createObjectDebris("(O)890", (int)tileLocation.X, (int)tileLocation.Y - 3, ((int)tileLocation.Y + 1) * 64, 0, 1f, location);
				}
				location.playSound("treethud");
			}
			if (!this.falling.Value)
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>Update the tree's season for the location it's planted in.</summary>
	protected void setSeason()
	{
		GameLocation location = this.Location;
		this.localSeason = ((!(location is Desert) && !(location is MineShaft)) ? Game1.GetSeasonForLocation(location) : Season.Spring);
	}

	public override void drawInMenu(SpriteBatch spriteBatch, Vector2 positionOnScreen, Vector2 tileLocation, float scale, float layerDepth)
	{
		layerDepth += positionOnScreen.X / 100000f;
		if (this.growthStage.Value < 5)
		{
			Microsoft.Xna.Framework.Rectangle sourceRect = this.growthStage.Value switch
			{
				0 => new Microsoft.Xna.Framework.Rectangle(32, 128, 16, 16), 
				1 => new Microsoft.Xna.Framework.Rectangle(0, 128, 16, 16), 
				2 => new Microsoft.Xna.Framework.Rectangle(16, 128, 16, 16), 
				_ => new Microsoft.Xna.Framework.Rectangle(0, 96, 16, 32), 
			};
			spriteBatch.Draw(this.texture.Value, positionOnScreen - new Vector2(0f, (float)sourceRect.Height * scale), sourceRect, Color.White, 0f, Vector2.Zero, scale, this.flipped.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth + (positionOnScreen.Y + (float)sourceRect.Height * scale) / 20000f);
			return;
		}
		if (!this.falling.Value)
		{
			spriteBatch.Draw(this.texture.Value, positionOnScreen + new Vector2(0f, -64f * scale), new Microsoft.Xna.Framework.Rectangle(32, 96, 16, 32), Color.White, 0f, Vector2.Zero, scale, this.flipped.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth + (positionOnScreen.Y + 448f * scale - 1f) / 20000f);
		}
		if (!this.stump.Value || this.falling.Value)
		{
			spriteBatch.Draw(this.texture.Value, positionOnScreen + new Vector2(-64f * scale, -320f * scale), new Microsoft.Xna.Framework.Rectangle(0, 0, 48, 96), Color.White, this.shakeRotation, Vector2.Zero, scale, this.flipped.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None, layerDepth + (positionOnScreen.Y + 448f * scale) / 20000f);
		}
	}

	public override void draw(SpriteBatch spriteBatch)
	{
		if (base.isTemporarilyInvisible)
		{
			return;
		}
		Vector2 tileLocation = this.Tile;
		float baseSortPosition = this.getBoundingBox().Bottom;
		if (this.texture.Value == null || !Tree.TryGetData(this.treeType.Value, out var data))
		{
			IItemDataDefinition itemType = ItemRegistry.RequireTypeDefinition("(O)");
			spriteBatch.Draw(itemType.GetErrorTexture(), Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f + ((this.shakeTimer > 0f) ? ((float)Math.Sin(Math.PI * 2.0 / (double)this.shakeTimer) * 3f) : 0f), tileLocation.Y * 64f)), itemType.GetErrorSourceRect(), Color.White * this.alpha, 0f, Vector2.Zero, 4f, this.flipped.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (baseSortPosition + 1f) / 10000f);
			return;
		}
		if (this.growthStage.Value < 5)
		{
			Microsoft.Xna.Framework.Rectangle sourceRect = this.growthStage.Value switch
			{
				0 => new Microsoft.Xna.Framework.Rectangle(32, 128, 16, 16), 
				1 => new Microsoft.Xna.Framework.Rectangle(0, 128, 16, 16), 
				2 => new Microsoft.Xna.Framework.Rectangle(16, 128, 16, 16), 
				_ => new Microsoft.Xna.Framework.Rectangle(0, 96, 16, 32), 
			};
			spriteBatch.Draw(this.texture.Value, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f + 32f, tileLocation.Y * 64f - (float)(sourceRect.Height * 4 - 64) + (float)((this.growthStage.Value >= 3) ? 128 : 64))), sourceRect, this.fertilized.Value ? Color.HotPink : Color.White, this.shakeRotation, new Vector2(8f, (this.growthStage.Value >= 3) ? 32 : 16), 4f, this.flipped.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (this.growthStage.Value == 0) ? 0.0001f : (baseSortPosition / 10000f));
		}
		else
		{
			if (!this.stump.Value || this.falling.Value)
			{
				if (this.IsLeafy())
				{
					spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f - 51f, tileLocation.Y * 64f - 16f)), Tree.shadowSourceRect, Color.White * ((float)Math.PI / 2f - Math.Abs(this.shakeRotation)), 0f, Vector2.Zero, 4f, this.flipped.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 1E-06f);
				}
				else
				{
					spriteBatch.Draw(Game1.mouseCursors_1_6, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f - 51f, tileLocation.Y * 64f - 16f)), new Microsoft.Xna.Framework.Rectangle(469, 298, 42, 31), Color.White * ((float)Math.PI / 2f - Math.Abs(this.shakeRotation)), 0f, Vector2.Zero, 4f, this.flipped.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 1E-06f);
				}
				Microsoft.Xna.Framework.Rectangle source_rect = Tree.treeTopSourceRect;
				if ((data.UseAlternateSpriteWhenSeedReady && this.hasSeed.Value) || (data.UseAlternateSpriteWhenNotShaken && !this.wasShakenToday.Value))
				{
					source_rect.X = 48;
				}
				else
				{
					source_rect.X = 0;
				}
				if (this.hasMoss.Value)
				{
					source_rect.X = 96;
				}
				spriteBatch.Draw(this.texture.Value, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f + 32f, tileLocation.Y * 64f + 64f)), source_rect, Color.White * this.alpha, this.shakeRotation, new Vector2(24f, 96f), 4f, this.flipped.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (baseSortPosition + 2f) / 10000f - tileLocation.X / 1000000f);
			}
			Microsoft.Xna.Framework.Rectangle stumpSource = Tree.stumpSourceRect;
			if (this.hasMoss.Value)
			{
				stumpSource.X += 96;
			}
			if (this.health.Value >= 1f || (!this.falling.Value && this.health.Value > -99f))
			{
				spriteBatch.Draw(this.texture.Value, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f + ((this.shakeTimer > 0f) ? ((float)Math.Sin(Math.PI * 2.0 / (double)this.shakeTimer) * 3f) : 0f), tileLocation.Y * 64f - 64f)), stumpSource, Color.White * this.alpha, 0f, Vector2.Zero, 4f, this.flipped.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None, baseSortPosition / 10000f);
			}
			if (this.stump.Value && this.health.Value < 4f && this.health.Value > -99f)
			{
				spriteBatch.Draw(this.texture.Value, Game1.GlobalToLocal(Game1.viewport, new Vector2(tileLocation.X * 64f + ((this.shakeTimer > 0f) ? ((float)Math.Sin(Math.PI * 2.0 / (double)this.shakeTimer) * 3f) : 0f), tileLocation.Y * 64f)), new Microsoft.Xna.Framework.Rectangle(Math.Min(2, (int)(3f - this.health.Value)) * 16, 144, 16, 16), Color.White * this.alpha, 0f, Vector2.Zero, 4f, this.flipped.Value ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (baseSortPosition + 1f) / 10000f);
			}
		}
		foreach (Leaf l in this.leaves)
		{
			spriteBatch.Draw(this.texture.Value, Game1.GlobalToLocal(Game1.viewport, l.position), new Microsoft.Xna.Framework.Rectangle(16 + l.type % 2 * 8, 112 + l.type / 2 * 8, 8, 8), Color.White, l.rotation, Vector2.Zero, 4f, SpriteEffects.None, baseSortPosition / 10000f + 0.01f);
		}
	}
}
