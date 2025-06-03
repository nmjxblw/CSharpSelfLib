using System;
using System.Collections.Generic;
using System.Linq;
using Netcode;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.GameData.Crops;

namespace Pathoschild.Stardew.Common.DataParsers;

internal class CropDataParser
{
	public Crop? Crop { get; }

	public CropData? CropData { get; }

	public Season[] Seasons { get; }

	public int HarvestablePhase { get; }

	public int DaysToFirstHarvest { get; }

	public int DaysToSubsequentHarvest { get; }

	public bool HasMultipleHarvests { get; }

	public bool CanHarvestNow { get; }

	public CropDataParser(Crop? crop, bool isPlanted)
	{
		this.Crop = crop;
		this.CropData = ((crop != null) ? crop.GetData() : null);
		CropData data = this.CropData;
		if (data != null)
		{
			this.Seasons = data.Seasons.ToArray();
			this.HasMultipleHarvests = crop.RegrowsAfterHarvest();
			this.HarvestablePhase = ((NetList<int, NetInt>)(object)crop.phaseDays).Count - 1;
			this.CanHarvestNow = ((NetFieldBase<int, NetInt>)(object)crop.currentPhase).Value >= this.HarvestablePhase && (!((NetFieldBase<bool, NetBool>)(object)crop.fullyGrown).Value || ((NetFieldBase<int, NetInt>)(object)crop.dayOfCurrentPhase).Value <= 0);
			this.DaysToFirstHarvest = ((IEnumerable<int>)crop.phaseDays).Take(((NetList<int, NetInt>)(object)crop.phaseDays).Count - 1).Sum();
			this.DaysToSubsequentHarvest = data.RegrowDays;
			if (!isPlanted && ((NetHashSet<int>)(object)Game1.player.professions).Contains(5))
			{
				this.DaysToFirstHarvest = (int)((double)this.DaysToFirstHarvest * 0.9);
			}
		}
		else
		{
			this.Seasons = Array.Empty<Season>();
		}
	}

	public SDate GetNextHarvest()
	{
		Crop crop = this.Crop;
		if (crop == null)
		{
			throw new InvalidOperationException("Can't get the harvest date because there's no crop.");
		}
		CropData data = this.CropData;
		if (data == null)
		{
			throw new InvalidOperationException("Can't get the harvest date because the crop has no data.");
		}
		if (this.CanHarvestNow)
		{
			return SDate.Now();
		}
		if (!((NetFieldBase<bool, NetBool>)(object)crop.fullyGrown).Value)
		{
			int daysUntilLastPhase = this.DaysToFirstHarvest - ((NetFieldBase<int, NetInt>)(object)crop.dayOfCurrentPhase).Value - ((IEnumerable<int>)crop.phaseDays).Take(((NetFieldBase<int, NetInt>)(object)crop.currentPhase).Value).Sum();
			return SDate.Now().AddDays(daysUntilLastPhase);
		}
		if (((NetFieldBase<int, NetInt>)(object)crop.dayOfCurrentPhase).Value >= data.RegrowDays)
		{
			return SDate.Now().AddDays(data.RegrowDays);
		}
		return SDate.Now().AddDays(((NetFieldBase<int, NetInt>)(object)crop.dayOfCurrentPhase).Value);
	}

	public Item GetSampleDrop()
	{
		if (this.Crop == null)
		{
			throw new InvalidOperationException("Can't get a sample drop because there's no crop.");
		}
		return ItemRegistry.Create(((NetFieldBase<string, NetString>)(object)this.Crop.indexOfHarvest).Value, 1, 0, false);
	}
}
