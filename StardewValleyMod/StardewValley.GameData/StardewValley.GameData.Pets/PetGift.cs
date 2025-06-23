using System;
using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.Pets;

/// <summary>The item spawn info for a pet gift.</summary>
public class PetGift : GenericSpawnItemDataWithCondition
{
	/// <summary>The friendship level that this pet must be at before it can give this gift. Defaults to 1000 (max friendship)</summary>
	[ContentSerializer(Optional = true)]
	public int MinimumFriendshipThreshold { get; set; } = 1000;

	/// <summary>The item's weight when randomly choosing a item, relative to other items in the list (e.g. 2 is twice as likely as 1).</summary>
	[ContentSerializer(Optional = true)]
	public float Weight { get; set; } = 1f;

	/// <summary>Obsolete.</summary>
	[Obsolete("Use ItemId instead.")]
	[ContentSerializerIgnore]
	public string QualifiedItemID
	{
		get
		{
			return null;
		}
		set
		{
			base.ItemId = value ?? base.ItemId;
		}
	}

	/// <summary>Obsolete.</summary>
	[Obsolete("Use MinStack instead.")]
	[ContentSerializerIgnore]
	public int? Stack
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
}
