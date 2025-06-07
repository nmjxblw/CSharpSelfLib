using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;

namespace GiantCropRing;

public class ModEntry : Mod
{
	private ModConfig config;

	private Texture2D giantRingTexture;

	private int numberOfTimeTicksWearingOneRing;

	private int numberOfTimeTicksWearingTwoRings;

	private int totalNumberOfSeenTimeTicks;

	public override void Entry(IModHelper helper)
	{
		helper.Events.GameLoop.DayEnding += OnDayEnding;
		helper.Events.GameLoop.TimeChanged += OnTimeChanged;
		helper.Events.Display.MenuChanged += OnMenuChanged;
		config = helper.ReadConfig<ModConfig>();
		giantRingTexture = ((Mod)this).Helper.Content.Load<Texture2D>("assets/ring.png", (ContentSource)1);
		GiantRing.texture = giantRingTexture;
		GiantRing.price = config.cropRingPrice / 2;
	}

	private void OnTimeChanged(object sender, TimeChangedEventArgs e)
	{
		bool left = ((NetFieldBase<Ring, NetRef<Ring>>)(object)Game1.player.leftRing).Value is GiantRing;
		bool right = ((NetFieldBase<Ring, NetRef<Ring>>)(object)Game1.player.rightRing).Value is GiantRing;
		if (left && right)
		{
			numberOfTimeTicksWearingOneRing++;
			numberOfTimeTicksWearingTwoRings++;
		}
		else if (left || right)
		{
			numberOfTimeTicksWearingOneRing++;
		}
		totalNumberOfSeenTimeTicks++;
	}

	private void OnMenuChanged(object sender, MenuChangedEventArgs e)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Expected O, but got Unknown
		if (Game1.activeClickableMenu is ShopMenu)
		{
			ShopMenu shop = (ShopMenu)Game1.activeClickableMenu;
			if (shop.portraitPerson != null && ((Character)shop.portraitPerson).Name == "Pierre")
			{
				GiantRing ring = new GiantRing();
				shop.itemPriceAndStock.Add((ISalable)(object)ring, new int[2] { config.cropRingPrice, 2147483647 });
				shop.forSale.Add((ISalable)(object)ring);
			}
		}
	}

	private void OnDayEnding(object sender, DayEndingEventArgs e)
	{
		double chance = 0.0;
		totalNumberOfSeenTimeTicks = Math.Max(1, totalNumberOfSeenTimeTicks);
		numberOfTimeTicksWearingOneRing = Math.Max(1, numberOfTimeTicksWearingOneRing);
		numberOfTimeTicksWearingTwoRings = Math.Max(1, numberOfTimeTicksWearingTwoRings);
		if ((double)numberOfTimeTicksWearingOneRing / ((double)totalNumberOfSeenTimeTicks * 1.0) >= config.percentOfDayNeededToWearRingToTriggerEffect)
		{
			chance = config.cropChancePercentWithRing;
		}
		if (config.shouldWearingBothRingsDoublePercentage && (double)numberOfTimeTicksWearingTwoRings / ((double)totalNumberOfSeenTimeTicks * 1.0) >= config.percentOfDayNeededToWearRingToTriggerEffect)
		{
			chance = 2.0 * config.cropChancePercentWithRing;
		}
		if (chance > 0.0)
		{
			MaybeChangeCrops(chance, (GameLocation)(object)Game1.getFarm());
		}
		numberOfTimeTicksWearingOneRing = 0;
		numberOfTimeTicksWearingTwoRings = 0;
		totalNumberOfSeenTimeTicks = 0;
	}

	private void MaybeChangeCrops(double chance, GameLocation environment)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_0237: Unknown result type (might be due to invalid IL or missing references)
		//IL_023c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0246: Expected O, but got Unknown
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		Vector2 key = default(Vector2);
		Vector2 index3 = default(Vector2);
		foreach (Tuple<Vector2, Crop> tup in GetValidCrops())
		{
			int xTile = (int)tup.Item1.X;
			int yTile = (int)tup.Item1.Y;
			Crop crop = tup.Item2;
			double rand = new Random((int)Game1.uniqueIDForThisGame + (int)Game1.stats.DaysPlayed + xTile * 2000 + yTile).NextDouble();
			bool okCrop = true;
			if (((NetFieldBase<int, NetInt>)(object)crop.currentPhase).Value != ((NetList<int, NetInt>)(object)crop.phaseDays).Count - 1 || (((NetFieldBase<int, NetInt>)(object)crop.indexOfHarvest).Value != 276 && ((NetFieldBase<int, NetInt>)(object)crop.indexOfHarvest).Value != 190 && ((NetFieldBase<int, NetInt>)(object)crop.indexOfHarvest).Value != 254) || !(rand < chance))
			{
				continue;
			}
			for (int index1 = xTile - 1; index1 <= xTile + 1; index1++)
			{
				for (int i = yTile - 1; i <= yTile + 1; i++)
				{
					((Vector2)(ref key))._002Ector((float)index1, (float)i);
					if (!((NetDictionary<Vector2, TerrainFeature, NetRef<TerrainFeature>, SerializableDictionary<Vector2, TerrainFeature>, NetVector2Dictionary<TerrainFeature, NetRef<TerrainFeature>>>)(object)environment.terrainFeatures).ContainsKey(key) || !(((NetDictionary<Vector2, TerrainFeature, NetRef<TerrainFeature>, SerializableDictionary<Vector2, TerrainFeature>, NetVector2Dictionary<TerrainFeature, NetRef<TerrainFeature>>>)(object)environment.terrainFeatures)[key] is HoeDirt) || ((HoeDirt)/*isinst with value type is only supported in some contexts*/).crop == null || (NetFieldBase<int, NetInt>)(object)((HoeDirt)/*isinst with value type is only supported in some contexts*/).crop.indexOfHarvest != crop.indexOfHarvest)
					{
						okCrop = false;
						break;
					}
				}
				if (!okCrop)
				{
					break;
				}
			}
			if (!okCrop)
			{
				continue;
			}
			for (int j = xTile - 1; j <= xTile + 1; j++)
			{
				for (int k = yTile - 1; k <= yTile + 1; k++)
				{
					((Vector2)(ref index3))._002Ector((float)j, (float)k);
					TerrainFeature obj = ((NetDictionary<Vector2, TerrainFeature, NetRef<TerrainFeature>, SerializableDictionary<Vector2, TerrainFeature>, NetVector2Dictionary<TerrainFeature, NetRef<TerrainFeature>>>)(object)environment.terrainFeatures)[index3];
					((HoeDirt)((obj is HoeDirt) ? obj : null)).crop = null;
				}
			}
			((Farm)((environment is Farm) ? environment : null)).resourceClumps.Add((ResourceClump)new GiantCrop(((NetFieldBase<int, NetInt>)(object)crop.indexOfHarvest).Value, new Vector2((float)(xTile - 1), (float)(yTile - 1))));
		}
	}

	private List<Tuple<Vector2, Crop>> GetValidCrops()
	{
		return Game1.locations.Where((GameLocation gl) => gl is Farm).SelectMany((GameLocation gl) => from c in ((IEnumerable<KeyValuePair<Vector2, TerrainFeature>>)(object)((NetDictionary<Vector2, TerrainFeature, NetRef<TerrainFeature>, SerializableDictionary<Vector2, TerrainFeature>, NetVector2Dictionary<TerrainFeature, NetRef<TerrainFeature>>>)(object)((gl is Farm) ? gl : null).terrainFeatures).Pairs).Where(delegate(KeyValuePair<Vector2, TerrainFeature> tf)
			{
				TerrainFeature value = tf.Value;
				HoeDirt val = (HoeDirt)(object)((value is HoeDirt) ? value : null);
				return val != null && val.crop != null && ((NetFieldBase<int, NetInt>)(object)val.state).Value == 1;
			}).Select(delegate(KeyValuePair<Vector2, TerrainFeature> hd)
			{
				//IL_0002: Unknown result type (might be due to invalid IL or missing references)
				Vector2 key = hd.Key;
				TerrainFeature value = hd.Value;
				return new Tuple<Vector2, Crop>(key, ((HoeDirt)((value is HoeDirt) ? value : null)).crop);
			})
			where !((NetFieldBase<bool, NetBool>)(object)c.Item2.dead).Value && ((NetList<string, NetString>)(object)c.Item2.seasonsToGrowIn).Contains(Game1.currentSeason)
			select c).ToList();
	}
}
