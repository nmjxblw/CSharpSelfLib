using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.LookupAnything.Framework.Constants;
using Pathoschild.Stardew.LookupAnything.Framework.Models;
using StardewValley;

namespace Pathoschild.Stardew.LookupAnything.Framework.Fields;

internal class ItemGiftTastesField : GenericField
{
	public ItemGiftTastesField(string label, IDictionary<GiftTaste, GiftTasteModel[]> giftTastes, GiftTaste showTaste, bool showUnknown, bool highlightUnrevealed)
		: base(label, ItemGiftTastesField.GetText(giftTastes, showTaste, showUnknown, highlightUnrevealed))
	{
	}

	private static IEnumerable<IFormattedText> GetText(IDictionary<GiftTaste, GiftTasteModel[]> giftTastes, GiftTaste showTaste, bool showUnknown, bool highlightUnrevealed)
	{
		if (!giftTastes.TryGetValue(showTaste, out GiftTasteModel[] allEntries))
		{
			yield break;
		}
		GiftTasteModel[] visibleEntries = (from giftTasteModel in allEntries
			orderby ((Character)giftTasteModel.Villager).displayName
			where showUnknown || giftTasteModel.IsRevealed
			select giftTasteModel).ToArray();
		int unrevealed = ((!showUnknown) ? giftTastes[showTaste].Count((GiftTasteModel p) => !p.IsRevealed) : 0);
		if (visibleEntries.Any())
		{
			int i = 0;
			for (int last = visibleEntries.Length - 1; i <= last; i++)
			{
				GiftTasteModel entry = visibleEntries[i];
				string text = ((Character)entry.Villager).displayName + ((i != last) ? I18n.Generic_ListSeparator() : "");
				bool bold = highlightUnrevealed && !entry.IsRevealed;
				yield return new FormattedText(text, null, bold);
			}
			if (unrevealed > 0)
			{
				yield return new FormattedText(I18n.Item_UndiscoveredGiftTasteAppended(unrevealed), Color.Gray);
			}
		}
		else
		{
			yield return new FormattedText(I18n.Item_UndiscoveredGiftTaste(unrevealed), Color.Gray);
		}
	}
}
