using System;
using StardewValley;

namespace Pathoschild.Stardew.LookupAnything.Framework.Data;

internal record FishPondDropData : ItemDropData
{
	public Item SampleItem { get; }

	public int MinPopulation { get; }

	public int Precedence { get; }

	public FishPondDropData(int minPopulation, int precedence, Item sampleItem, int minDrop, int maxDrop, float probability, string? conditions)
		: base(sampleItem.QualifiedItemId, minDrop, maxDrop, probability, conditions)
	{
		this.SampleItem = sampleItem;
		this.MinPopulation = Math.Max(minPopulation, 1);
		this.Precedence = precedence;
	}
}
