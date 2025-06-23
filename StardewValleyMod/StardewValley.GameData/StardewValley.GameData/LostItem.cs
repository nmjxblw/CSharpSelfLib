using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData;

/// <summary>An item which is otherwise unobtainable if lost, so it can appear in the crow's lost items shop.</summary>
public class LostItem
{
	/// <summary>A unique string ID for this entry in this list.</summary>
	public string Id;

	/// <summary>The qualified item ID to add to the shop.</summary>
	public string ItemId;

	/// <summary>The mail flag required to add this item.</summary>
	/// <remarks>The number added to the shop is the number of players which match this field minus the number of the item which exist in the world. If you specify multiple criteria fields, only one is applied in the order <see cref="F:StardewValley.GameData.LostItem.RequireMailReceived" /> and then <see cref="F:StardewValley.GameData.LostItem.RequireEventSeen" />.</remarks>
	[ContentSerializer(Optional = true)]
	public string RequireMailReceived;

	/// <summary>The event ID that must be seen to add this item.</summary>
	/// <remarks><inheritdoc cref="F:StardewValley.GameData.LostItem.RequireMailReceived" path="/remarks" /></remarks>
	[ContentSerializer(Optional = true)]
	public string RequireEventSeen;
}
