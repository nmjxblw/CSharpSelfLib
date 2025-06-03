using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using Pathoschild.Stardew.LookupAnything.Framework.Constants;
using Pathoschild.Stardew.LookupAnything.Framework.DebugFields;
using Pathoschild.Stardew.LookupAnything.Framework.Fields;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.GameData.FruitTrees;
using StardewValley.TerrainFeatures;
using StardewValley.TokenizableStrings;

namespace Pathoschild.Stardew.LookupAnything.Framework.Lookups.TerrainFeatures;

internal class FruitTreeSubject : BaseSubject
{
	private readonly FruitTree Target;

	private readonly Vector2 Tile;

	public FruitTreeSubject(GameHelper gameHelper, FruitTree tree, Vector2 tile)
		: base(gameHelper, I18n.FruitTree_Name(FruitTreeSubject.GetDisplayName(tree)), null, I18n.Type_FruitTree())
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		this.Target = tree;
		this.Tile = tile;
	}

	public override IEnumerable<ICustomField> GetData()
	{
		FruitTree tree = this.Target;
		bool isMature = ((NetFieldBase<int, NetInt>)(object)tree.daysUntilMature).Value <= 0;
		bool isDead = ((NetFieldBase<bool, NetBool>)(object)tree.stump).Value;
		bool isStruckByLightning = ((NetFieldBase<int, NetInt>)(object)tree.struckByLightningCountdown).Value > 0;
		IModInfo fromMod = base.GameHelper.TryGetModFromStringId(((NetFieldBase<string, NetString>)(object)tree.treeId).Value);
		if (fromMod != null)
		{
			yield return new GenericField(I18n.AddedByMod(), I18n.AddedByMod_Summary(fromMod.Manifest.Name));
		}
		if (isMature && !isDead)
		{
			SDate nextFruit = SDate.Now().AddDays(1);
			string label = I18n.FruitTree_NextFruit();
			if (isStruckByLightning)
			{
				yield return new GenericField(label, I18n.FruitTree_NextFruit_StruckByLightning(((NetFieldBase<int, NetInt>)(object)tree.struckByLightningCountdown).Value));
			}
			else if (!this.IsInSeason(tree, nextFruit.Season))
			{
				yield return new GenericField(label, I18n.FruitTree_NextFruit_OutOfSeason());
			}
			else if (tree.fruit.Count >= 3)
			{
				yield return new GenericField(label, I18n.FruitTree_NextFruit_MaxFruit());
			}
			else
			{
				yield return new GenericField(label, I18n.Generic_Tomorrow());
			}
		}
		if (!isMature)
		{
			SDate dayOfMaturity = SDate.Now().AddDays(((NetFieldBase<int, NetInt>)(object)tree.daysUntilMature).Value);
			string grownOnDateText = I18n.FruitTree_Growth_Summary(base.Stringify(dayOfMaturity));
			yield return new GenericField(I18n.FruitTree_NextFruit(), I18n.FruitTree_NextFruit_TooYoung());
			yield return new GenericField(I18n.FruitTree_Growth(), grownOnDateText + " (" + base.GetRelativeDateStr(dayOfMaturity) + ")");
			if (FruitTree.IsGrowthBlocked(this.Tile, ((TerrainFeature)tree).Location))
			{
				yield return new GenericField(I18n.FruitTree_Complaints(), I18n.FruitTree_Complaints_AdjacentObjects());
			}
		}
		else
		{
			ItemQuality currentQuality = this.GetCurrentQuality(tree, base.Constants.FruitTreeQualityGrowthTime);
			if (currentQuality == ItemQuality.Iridium)
			{
				yield return new GenericField(I18n.FruitTree_Quality(), I18n.FruitTree_Quality_Now(I18n.For(currentQuality)));
			}
			else
			{
				string[] summary = this.GetQualitySchedule(tree, currentQuality, base.Constants.FruitTreeQualityGrowthTime).Select(delegate(KeyValuePair<ItemQuality, int> entry)
				{
					ItemQuality key = entry.Key;
					int value = entry.Value;
					SDate val = SDate.Now().AddDays(value);
					int num = val.Year - Game1.year;
					if (value <= 0)
					{
						return "-" + I18n.FruitTree_Quality_Now(I18n.For(key));
					}
					string text = ((num == 1) ? ("-" + I18n.FruitTree_Quality_OnDateNextYear(I18n.For(key), base.Stringify(val))) : ("-" + I18n.FruitTree_Quality_OnDate(I18n.For(key), base.Stringify(val))));
					return text + " (" + base.GetRelativeDateStr(value) + ")";
				}).ToArray();
				yield return new GenericField(I18n.FruitTree_Quality(), string.Join(Environment.NewLine, summary));
			}
		}
		FruitTreeData data = tree.GetData();
		IEnumerable<string> seasons = ((data == null) ? null : data.Seasons?.Select(I18n.GetSeasonName));
		if (seasons != null)
		{
			yield return new GenericField(I18n.FruitTree_Season(), I18n.FruitTree_Season_Summary(I18n.List(seasons)));
		}
		yield return new GenericField(I18n.InternalId(), ((NetFieldBase<string, NetString>)(object)tree.treeId).Value);
	}

	public override IEnumerable<IDebugField> GetDebugFields()
	{
		FruitTree target = this.Target;
		yield return new GenericDebugField("mature in", $"{target.daysUntilMature} days", null, pinned: true);
		yield return new GenericDebugField("growth stage", ((NetFieldBase<int, NetInt>)(object)target.growthStage).Value, null, pinned: true);
		yield return new GenericDebugField("health", ((NetFieldBase<float, NetFloat>)(object)target.health).Value, null, pinned: true);
		foreach (IDebugField item in base.GetDebugFieldsFrom(target))
		{
			yield return item;
		}
	}

	public override bool DrawPortrait(SpriteBatch spriteBatch, Vector2 position, Vector2 size)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		((TerrainFeature)this.Target).drawInMenu(spriteBatch, position, Vector2.Zero, 1f, 1f);
		return true;
	}

	private static string GetDisplayName(FruitTree tree)
	{
		FruitTreeData data = tree.GetData();
		string displayName = TokenParser.ParseText((data != null) ? data.DisplayName : null, (Random)null, (TokenParserDelegate)null, (Farmer)null);
		if (!string.IsNullOrWhiteSpace((data != null) ? data.DisplayName : null))
		{
			return displayName;
		}
		return "???";
	}

	private ItemQuality GetCurrentQuality(FruitTree tree, int daysPerQuality)
	{
		int maturityLevel = Math.Max(0, Math.Min(3, -((NetFieldBase<int, NetInt>)(object)tree.daysUntilMature).Value / daysPerQuality));
		return maturityLevel switch
		{
			0 => ItemQuality.Normal, 
			1 => ItemQuality.Silver, 
			2 => ItemQuality.Gold, 
			3 => ItemQuality.Iridium, 
			_ => throw new NotSupportedException($"Unexpected quality level {maturityLevel}."), 
		};
	}

	private IEnumerable<KeyValuePair<ItemQuality, int>> GetQualitySchedule(FruitTree tree, ItemQuality currentQuality, int daysPerQuality)
	{
		if (((NetFieldBase<int, NetInt>)(object)tree.daysUntilMature).Value > 0)
		{
			yield break;
		}
		yield return new KeyValuePair<ItemQuality, int>(currentQuality, 0);
		int dayOffset = daysPerQuality - Math.Abs(((NetFieldBase<int, NetInt>)(object)tree.daysUntilMature).Value % daysPerQuality);
		ItemQuality[] array = new ItemQuality[3]
		{
			ItemQuality.Silver,
			ItemQuality.Gold,
			ItemQuality.Iridium
		};
		foreach (ItemQuality futureQuality in array)
		{
			if (currentQuality < futureQuality)
			{
				yield return new KeyValuePair<ItemQuality, int>(futureQuality, dayOffset);
				dayOffset += daysPerQuality;
			}
		}
	}

	private bool IsInSeason(FruitTree tree, Season season)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		if (((TerrainFeature)tree).Location.SeedsIgnoreSeasonsHere())
		{
			return true;
		}
		FruitTreeData data = tree.GetData();
		List<Season> growSeasons = ((data != null) ? data.Seasons : null);
		if (growSeasons != null)
		{
			foreach (Season growSeason in growSeasons)
			{
				if (season == growSeason)
				{
					return true;
				}
			}
		}
		return false;
	}
}
