using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.LookupAnything.Framework.Data;
using StardewValley;

namespace Pathoschild.Stardew.LookupAnything.Framework.Fields.Models;

internal record FishPondDrop : FishPondDropData
{
	public SpriteInfo? Sprite { get; }

	public bool IsUnlocked { get; }

	public FishPondDrop(FishPondDropData data, Item sampleItem, SpriteInfo? sprite, bool isUnlocked)
		: base(data.MinPopulation, data.Precedence, sampleItem, data.MinDrop, data.MaxDrop, data.Probability, data.Conditions)
	{
		this.Sprite = sprite;
		this.IsUnlocked = isUnlocked;
	}
}
