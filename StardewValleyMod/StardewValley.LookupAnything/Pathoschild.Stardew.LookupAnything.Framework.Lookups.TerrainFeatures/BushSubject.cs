using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using Pathoschild.Stardew.Common.Integrations.CustomBush;
using Pathoschild.Stardew.Common.Utilities;
using Pathoschild.Stardew.LookupAnything.Framework.Data;
using Pathoschild.Stardew.LookupAnything.Framework.DebugFields;
using Pathoschild.Stardew.LookupAnything.Framework.Fields;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.GameData;
using StardewValley.TerrainFeatures;
using StardewValley.TokenizableStrings;

namespace Pathoschild.Stardew.LookupAnything.Framework.Lookups.TerrainFeatures;

internal class BushSubject : BaseSubject
{
	private readonly Bush Target;

	private readonly ISubjectRegistry Codex;

	public BushSubject(GameHelper gameHelper, ISubjectRegistry codex, Bush bush)
		: base(gameHelper)
	{
		this.Target = bush;
		this.Codex = codex;
		if (this.TryGetCustomBush(bush, out ICustomBush customBush))
		{
			base.Initialize(TokenParser.ParseText(customBush.DisplayName, (Random)null, (TokenParserDelegate)null, (Farmer)null), TokenParser.ParseText(customBush.Description, (Random)null, (TokenParserDelegate)null, (Farmer)null), I18n.Type_Bush());
		}
		else if (this.IsBerryBush(bush))
		{
			base.Initialize(I18n.Bush_Name_Berry(), I18n.Bush_Description_Berry(), I18n.Type_Bush());
		}
		else if (this.IsTeaBush(bush))
		{
			base.Initialize(I18n.Bush_Name_Tea(), I18n.Bush_Description_Tea(), I18n.Type_Bush());
		}
		else
		{
			base.Initialize(I18n.Bush_Name_Plain(), I18n.Bush_Description_Plain(), I18n.Type_Bush());
		}
	}

	public override IEnumerable<ICustomField> GetData()
	{
		Bush bush = this.Target;
		bool isBerryBush = this.IsBerryBush(bush);
		bool isTeaBush = this.IsTeaBush(bush);
		SDate today = SDate.Now();
		if (isBerryBush && this.TryGetBushBloomSchedules(bush, out (string, WorldDate, WorldDate)[] bushBloomSchedule))
		{
			List<Item> itemList = new List<Item>();
			Dictionary<Item, string> displayText = new Dictionary<Item, string>(new ObjectReferenceComparer<Item>());
			foreach (var entry in from p in bushBloomSchedule
				orderby p.StartDay.TotalDays, p.EndDay.TotalDays
				select p)
			{
				SDate lastDay = SDate.From(entry.EndDay);
				SDate firstDay = SDate.From(entry.StartDay);
				Item item = ItemRegistry.Create(entry.UnqualifiedItemId, 1, 0, false);
				itemList.Add(item);
				if (firstDay < today)
				{
					firstDay = today;
				}
				if (!(lastDay < today))
				{
					displayText[item] = ((firstDay == lastDay) ? (item.DisplayName + ": " + base.Stringify(firstDay)) : $"{item.DisplayName}: {base.Stringify(firstDay)} - {base.Stringify(lastDay)}");
				}
			}
			yield return new ItemIconListField(base.GameHelper, I18n.Bush_NextHarvest(), itemList, showStackSize: false, (Item key) => displayText.GetValueOrDefault(key));
			yield break;
		}
		if (isBerryBush || isTeaBush)
		{
			SDate nextHarvest = this.GetNextHarvestDate(bush);
			string nextHarvestStr = ((nextHarvest == today) ? I18n.Generic_Now() : (base.Stringify(nextHarvest) + " (" + base.GetRelativeDateStr(nextHarvest) + ")"));
			if (this.TryGetCustomBushDrops(bush, out IList<ItemDropData> drops))
			{
				yield return new ItemDropListField(base.GameHelper, this.Codex, I18n.Bush_NextHarvest(), drops, sort: true, fadeNonGuaranteed: false, crossOutNonGuaranteed: false, null, nextHarvestStr);
			}
			else
			{
				string harvestSchedule = (isTeaBush ? I18n.Bush_Schedule_Tea() : I18n.Bush_Schedule_Berry());
				yield return new GenericField(I18n.Bush_NextHarvest(), nextHarvestStr + Environment.NewLine + harvestSchedule);
			}
		}
		if (isTeaBush)
		{
			SDate datePlanted = this.GetDatePlanted(bush);
			int daysOld = SDate.Now().DaysSinceStart - datePlanted.DaysSinceStart;
			SDate dateGrown = this.GetDateFullyGrown(bush);
			yield return new GenericField(I18n.Bush_DatePlanted(), base.Stringify(datePlanted) + " (" + base.GetRelativeDateStr(-daysOld) + ")");
			if (dateGrown > today)
			{
				string grownOnDateText = I18n.Bush_Growth_Summary(base.Stringify(dateGrown));
				yield return new GenericField(I18n.Bush_Growth(), grownOnDateText + " (" + base.GetRelativeDateStr(dateGrown) + ")");
			}
		}
	}

	public override IEnumerable<IDebugField> GetDebugFields()
	{
		Bush target = this.Target;
		yield return new GenericDebugField("health", target.health, null, pinned: true);
		yield return new GenericDebugField("is town bush", base.Stringify(((NetFieldBase<bool, NetBool>)(object)target.townBush).Value), null, pinned: true);
		yield return new GenericDebugField("is in bloom", base.Stringify(target.inBloom()), null, pinned: true);
		foreach (IDebugField item in base.GetDebugFieldsFrom(target))
		{
			yield return item;
		}
	}

	public override bool DrawPortrait(SpriteBatch spriteBatch, Vector2 position, Vector2 size)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		Bush bush = this.Target;
		Rectangle sourceArea = ((NetFieldBase<Rectangle, NetRectangle>)(object)bush.sourceRect).Value;
		Point spriteSize = default(Point);
		((Point)(ref spriteSize))._002Ector(sourceArea.Width * 4, sourceArea.Height * 4);
		SpriteEffects spriteEffects = (SpriteEffects)(((NetFieldBase<bool, NetBool>)(object)bush.flipped).Value ? 1 : 0);
		float scale = Math.Min(size.X / (float)spriteSize.X, size.Y / (float)spriteSize.Y);
		Point targetSize = default(Point);
		((Point)(ref targetSize))._002Ector((int)((float)spriteSize.X * scale), (int)((float)spriteSize.Y * scale));
		Vector2 offset = new Vector2(size.X - (float)targetSize.X, size.Y - (float)targetSize.Y) / 2f;
		ICustomBush customBush;
		Texture2D texture = ((!this.TryGetCustomBush(bush, out customBush)) ? Bush.texture.Value : (bush.IsSheltered() ? Game1.content.Load<Texture2D>(customBush.IndoorTexture) : Game1.content.Load<Texture2D>(customBush.Texture)));
		spriteBatch.Draw(texture, new Rectangle((int)(position.X + offset.X), (int)(position.Y + offset.Y), targetSize.X, targetSize.Y), (Rectangle?)sourceArea, Color.White, 0f, Vector2.Zero, spriteEffects, 0f);
		return true;
	}

	private bool IsBerryBush(Bush bush)
	{
		if (((NetFieldBase<int, NetInt>)(object)bush.size).Value == 1 && !((NetFieldBase<bool, NetBool>)(object)bush.townBush).Value)
		{
			return !((TerrainFeature)bush).Location.InIslandContext();
		}
		return false;
	}

	private bool IsTeaBush(Bush bush)
	{
		return ((NetFieldBase<int, NetInt>)(object)bush.size).Value == 3;
	}

	private bool TryGetCustomBush(Bush bush, [NotNullWhen(true)] out ICustomBush? customBush)
	{
		customBush = null;
		if (base.GameHelper.CustomBush.IsLoaded)
		{
			return base.GameHelper.CustomBush.ModApi.TryGetCustomBush(bush, out customBush);
		}
		return false;
	}

	private bool TryGetCustomBushDrops(Bush bush, [NotNullWhen(true)] out IList<ItemDropData>? drops)
	{
		CustomBushIntegration customBush = base.GameHelper.CustomBush;
		if (customBush.IsLoaded && customBush.ModApi.TryGetCustomBush(bush, out ICustomBush _, out string id) && customBush.ModApi.TryGetDrops(id, out IList<ICustomBushDrop> rawDrops))
		{
			drops = new List<ItemDropData>(rawDrops.Count);
			foreach (ICustomBushDrop drop in rawDrops)
			{
				drops.Add(new ItemDropData(((ISpawnItemData)drop).ItemId, ((ISpawnItemData)drop).MinStack, ((ISpawnItemData)drop).MaxStack, drop.Chance, drop.Condition));
			}
			return true;
		}
		drops = null;
		return false;
	}

	private bool TryGetBushBloomSchedules(Bush bush, [NotNullWhen(true)] out (string UnqualifiedItemId, WorldDate StartDay, WorldDate EndDay)[]? schedule)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		if (base.GameHelper.BushBloomMod.IsLoaded && base.GameHelper.BushBloomMod.ModApi.IsReady())
		{
			SDate today = SDate.Now();
			schedule = base.GameHelper.BushBloomMod.ModApi.GetActiveSchedules(((object)today.Season/*cast due to .constrained prefix*/).ToString(), today.Day, today.Year, ((TerrainFeature)bush).Location, ((TerrainFeature)bush).Tile);
			return true;
		}
		schedule = null;
		return false;
	}

	private SDate GetDatePlanted(Bush bush)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Expected O, but got Unknown
		SDate date = new SDate(1, (Season)0, 1);
		if (this.IsTeaBush(bush) && ((NetFieldBase<int, NetInt>)(object)bush.datePlanted).Value > 0)
		{
			date = date.AddDays(((NetFieldBase<int, NetInt>)(object)bush.datePlanted).Value);
		}
		return date;
	}

	private SDate GetDateFullyGrown(Bush bush)
	{
		SDate date = this.GetDatePlanted(bush);
		if (this.TryGetCustomBush(bush, out ICustomBush customBush))
		{
			date = date.AddDays(customBush.AgeToProduce);
		}
		else if (this.IsTeaBush(bush))
		{
			date = date.AddDays(20);
		}
		return date;
	}

	private int GetDayToBeginProducing(Bush bush)
	{
		if (this.TryGetCustomBush(bush, out ICustomBush customBush))
		{
			return customBush.DayToBeginProducing;
		}
		if (this.IsTeaBush(bush))
		{
			return 22;
		}
		return -1;
	}

	private List<Season> GetProducingSeasons(Bush bush)
	{
		if (this.TryGetCustomBush(bush, out ICustomBush customBush))
		{
			return customBush.Seasons;
		}
		if (this.IsTeaBush(bush))
		{
			return new List<Season>(3)
			{
				(Season)0,
				(Season)1,
				(Season)2
			};
		}
		return new List<Season>(2)
		{
			(Season)0,
			(Season)2
		};
	}

	private SDate GetNextHarvestDate(Bush bush)
	{
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Expected O, but got Unknown
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Expected O, but got Unknown
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Expected O, but got Unknown
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Expected O, but got Unknown
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Expected O, but got Unknown
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Expected O, but got Unknown
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Expected O, but got Unknown
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		SDate today = SDate.Now();
		SDate tomorrow = today.AddDays(1);
		if (((NetFieldBase<int, NetInt>)(object)bush.tileSheetOffset).Value == 1)
		{
			return today;
		}
		int dayToBegin = this.GetDayToBeginProducing(bush);
		if (dayToBegin >= 0)
		{
			SDate readyDate = this.GetDateFullyGrown(bush);
			if (readyDate < tomorrow)
			{
				readyDate = tomorrow;
			}
			if (!bush.IsSheltered())
			{
				List<Season> producingSeasons = this.GetProducingSeasons(bush);
				SDate seasonDate = new SDate(Math.Max(1, dayToBegin), readyDate.Season, readyDate.Year);
				while (!producingSeasons.Contains(seasonDate.Season))
				{
					seasonDate = seasonDate.AddDays(28);
				}
				if (readyDate < seasonDate)
				{
					return seasonDate;
				}
			}
			if (readyDate.Day < dayToBegin)
			{
				readyDate = new SDate(dayToBegin, readyDate.Season, readyDate.Year);
			}
			return readyDate;
		}
		SDate springStart = new SDate(15, (Season)0);
		SDate springEnd = new SDate(18, (Season)0);
		SDate fallStart = new SDate(8, (Season)2);
		SDate fallEnd = new SDate(11, (Season)2);
		if (tomorrow < springStart)
		{
			return springStart;
		}
		if (tomorrow > springEnd && tomorrow < fallStart)
		{
			return fallStart;
		}
		if (tomorrow > fallEnd)
		{
			return new SDate(springStart.Day, springStart.Season, springStart.Year + 1);
		}
		return tomorrow;
	}
}
