using System;
using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.FishPonds;

/// <summary>As part of <see cref="T:StardewValley.GameData.FishPonds.FishPondData" />, an item that can be produced by the fish pond.</summary>
public class FishPondReward : GenericSpawnItemDataWithCondition
{
	/// <summary>The minimum population needed before this output becomes available.</summary>
	[ContentSerializer(Optional = true)]
	public int RequiredPopulation;

	/// <summary>The percentage chance that this output is selected, as a value between 0 (never) and 1 (always). If multiple items pass, only the first one will be produced.</summary>
	[ContentSerializer(Optional = true)]
	public float Chance = 1f;

	/// <summary>The order in which this entry should be checked, where 0 is the default value used by most entries. Entries with the same precedence are checked in the order listed.</summary>
	[ContentSerializer(Optional = true)]
	public int Precedence;

	/// <summary>Obsolete; use <see cref="P:StardewValley.GameData.GenericSpawnItemData.MinStack" /> instead.</summary>
	[Obsolete("Use MinStack instead.")]
	[ContentSerializerIgnore]
	public int? MinQuantity
	{
		get
		{
			return null;
		}
		set
		{
			base.MinStack = value ?? base.MinStack;
		}
	}

	/// <summary>Obsolete; use <see cref="P:StardewValley.GameData.GenericSpawnItemData.MaxStack" /> instead.</summary>
	[Obsolete("Use MaxStack instead.")]
	[ContentSerializerIgnore]
	public int? MaxQuantity
	{
		get
		{
			return null;
		}
		set
		{
			base.MaxStack = value ?? base.MaxStack;
		}
	}
}
