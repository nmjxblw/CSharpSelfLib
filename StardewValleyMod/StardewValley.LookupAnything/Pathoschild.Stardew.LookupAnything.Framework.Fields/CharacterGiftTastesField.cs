using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.LookupAnything.Framework.Constants;
using Pathoschild.Stardew.LookupAnything.Framework.ItemScanning;
using Pathoschild.Stardew.LookupAnything.Framework.Models;
using StardewValley;

namespace Pathoschild.Stardew.LookupAnything.Framework.Fields;

internal class CharacterGiftTastesField : GenericField
{
	private record ItemRecord(Item Item, bool IsInventory, bool IsOwned, bool IsRevealed);

	public int TotalItems { get; }

	public CharacterGiftTastesField(string label, IDictionary<GiftTaste, GiftTasteModel[]> giftTastes, GiftTaste showTaste, bool showUnknown, bool highlightUnrevealed, bool onlyOwned, IDictionary<string, bool> ownedItemsCache)
		: base(label)
	{
		ItemRecord[] allItems = this.GetGiftTasteRecords(giftTastes, showTaste, ownedItemsCache);
		this.TotalItems = allItems.Length;
		base.Value = this.GetText(allItems, showUnknown, highlightUnrevealed, onlyOwned).ToArray();
		base.HasValue = base.Value.Length != 0;
	}

	public static IDictionary<string, bool> GetOwnedItemsCache(GameHelper gameHelper)
	{
		return (from entry in gameHelper.GetAllOwnedItems()
			group entry by entry.Item.QualifiedItemId).ToDictionary((IGrouping<string, FoundItem> group) => group.Key, (IGrouping<string, FoundItem> group) => group.Any((FoundItem p) => p.IsInInventory));
	}

	private ItemRecord[] GetGiftTasteRecords(IDictionary<GiftTaste, GiftTasteModel[]> giftTastes, GiftTaste showTaste, IDictionary<string, bool> ownedItemsCache)
	{
		if (!giftTastes.TryGetValue(showTaste, out GiftTasteModel[] entries))
		{
			return Array.Empty<ItemRecord>();
		}
		bool value;
		return (from entry in entries
			let item = entry.Item
			let ownership = ownedItemsCache.TryGetValue(item.QualifiedItemId, out value) ? new bool?(value) : ((bool?)null)
			let isOwned = ownership.HasValue
			let inInventory = ownership ?? false
			orderby inInventory descending, isOwned descending, item.DisplayName
			select new ItemRecord(item, inInventory, isOwned, entry.IsRevealed)).ToArray();
	}

	private IEnumerable<IFormattedText> GetText(ItemRecord[] items, bool showUnknown, bool highlightUnrevealed, bool onlyOwned)
	{
		if (!items.Any())
		{
			yield break;
		}
		int unrevealed = 0;
		int unowned = 0;
		int i = 0;
		for (int last = items.Length - 1; i <= last; i++)
		{
			ItemRecord entry = items[i];
			if (!showUnknown && !entry.IsRevealed)
			{
				unrevealed++;
				continue;
			}
			if (onlyOwned && !entry.IsOwned)
			{
				unowned++;
				continue;
			}
			string text = ((i != last) ? (entry.Item.DisplayName + I18n.Generic_ListSeparator()) : entry.Item.DisplayName);
			bool bold = highlightUnrevealed && !entry.IsRevealed;
			if (entry.IsInventory)
			{
				yield return new FormattedText(text, Color.Green, bold);
			}
			else if (entry.IsOwned)
			{
				yield return new FormattedText(text, Color.Black, bold);
			}
			else
			{
				yield return new FormattedText(text, Color.Gray, bold);
			}
		}
		if (unrevealed > 0)
		{
			yield return new FormattedText(I18n.Npc_UndiscoveredGiftTaste(unrevealed), Color.Gray);
		}
		if (unowned > 0)
		{
			yield return new FormattedText(I18n.Npc_UnownedGiftTaste(unowned), Color.Gray);
		}
	}
}
