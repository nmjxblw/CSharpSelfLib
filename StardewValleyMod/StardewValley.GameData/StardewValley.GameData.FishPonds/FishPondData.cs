using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.FishPonds;

/// <summary>The fish data for a Fish Pond building.</summary>
public class FishPondData
{
	/// <summary>A unique identifier for the entry. The ID should only contain alphanumeric/underscore/dot characters. For custom fish pond entries, this should be prefixed with your mod ID like <c>Example.ModId_Fish.</c></summary>
	public string Id;

	/// <summary>The context tags for the fish item to configure. If this lists multiple context tags, an item must match all of them. If an item matches multiple entries, the first entry which matches is used.</summary>
	public List<string> RequiredTags;

	/// <summary>The order in which this entry should be checked, where 0 is the default value used by most entries. Entries with the same precedence are checked in the order listed.</summary>
	[ContentSerializer(Optional = true)]
	public int Precedence;

	/// <summary>The maximum number of fish which can be added to this pond.</summary>
	/// <remarks>This cannot exceed the global maximum of 10.</remarks>
	[ContentSerializer(Optional = true)]
	public int MaxPopulation = -1;

	/// <summary>The number of days needed to raise the population by one if there's enough room in the fish pond, or <c>-1</c> to choose a number automatically based on the fish value.</summary>
	[ContentSerializer(Optional = true)]
	public int SpawnTime = -1;

	/// <summary>The minimum daily chance that this fish pond checks for output on a given day, as a value between 0 (never) and 1 (always).</summary>
	/// <remarks>The actual probability is lerped between <see cref="F:StardewValley.GameData.FishPonds.FishPondData.BaseMinProduceChance" /> and <see cref="F:StardewValley.GameData.FishPonds.FishPondData.BaseMaxProduceChance" /> based on the fish pond's population. If the min chance is 95+%, it's treated as the actual probability without lerping. If this check passes, output is only produced if one of the <see cref="F:StardewValley.GameData.FishPonds.FishPondData.ProducedItems" /> passes its checks too.</remarks>
	[ContentSerializer(Optional = true)]
	public float BaseMinProduceChance = 0.15f;

	/// <summary>The maximum daily chance that this fish pond checks for output on a given day, as a value between 0 (never) and 1 (always).</summary>
	/// <inheritdoc cref="F:StardewValley.GameData.FishPonds.FishPondData.BaseMinProduceChance" path="/remarks" />
	[ContentSerializer(Optional = true)]
	public float BaseMaxProduceChance = 0.95f;

	/// <summary>The custom water color to set, if applicable.</summary>
	[ContentSerializer(Optional = true)]
	public List<FishPondWaterColor> WaterColor;

	/// <summary>The items that can be produced by the fish pond. When a fish pond is ready to produce output, it will check each entry in the list and take the first one that matches. If no entry matches, no output is produced.</summary>
	public List<FishPondReward> ProducedItems;

	/// <summary>The rules which determine when the fish pond population can grow, and the quests that must be completed to do so.</summary>
	[ContentSerializer(Optional = true)]
	public Dictionary<int, List<string>> PopulationGates;

	/// <summary>Custom fields ignored by the base game, for use by mods.</summary>
	[ContentSerializer(Optional = true)]
	public Dictionary<string, string> CustomFields;
}
