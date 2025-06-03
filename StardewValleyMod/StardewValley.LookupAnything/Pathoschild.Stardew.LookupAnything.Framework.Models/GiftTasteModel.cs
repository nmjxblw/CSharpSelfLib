using Pathoschild.Stardew.LookupAnything.Framework.Constants;
using StardewValley;

namespace Pathoschild.Stardew.LookupAnything.Framework.Models;

internal record GiftTasteModel(NPC Villager, Item Item, GiftTaste Taste)
{
	public bool IsRevealed => Game1.player.hasGiftTasteBeenRevealed(this.Villager, this.Item.ItemId);
}
