using System.Collections.Generic;
using StardewValley.GameData.Shops;

namespace StardewValley;

/// <summary>The info about an item in a shop.</summary>
public class ItemStockInformation
{
	/// <summary>The gold price to purchase the item from the shop, or <c>0</c> for no gold price.</summary>
	public int Price;

	/// <summary>The maximum number of sets of this item which can be purchased in one day, or <see cref="F:StardewValley.Menus.ShopMenu.infiniteStock" /> for unlimited.</summary>
	public int Stock;

	/// <summary>The qualified or unqualified item ID which must be traded to purchase this item, if applicable.</summary>
	public string TradeItem;

	/// <summary>The number of <see cref="F:StardewValley.ItemStockInformation.TradeItem" /> needed to purchase this item, if applicable.</summary>
	public int? TradeItemCount;

	/// <summary>How the <see cref="F:StardewValley.ItemStockInformation.Stock" /> limit is applied in multiplayer.</summary>
	public LimitedStockMode LimitedStockMode;

	/// <summary>A unique key to track this specific item's stock within the shop.</summary>
	public string SyncedKey;

	/// <summary>If set, the stack count will be synchronized with the given item's. This is very specialized and only used for objects whose available stock are tracked separately from the normal shop stock tracking.</summary>
	public ISalable ItemToSyncStack;

	/// <summary>Override how the item's stack number is drawn in the shop menu, or <c>null</c> for the default behavior.</summary>
	public StackDrawType? StackDrawType;

	/// <summary>The actions to perform when the item is purchased.</summary>
	public List<string> ActionsOnPurchase;

	/// <summary>Construct an instance.</summary>
	/// <param name="price">The gold price to purchase the item from the shop, or <c>0</c> for no gold price.</param>
	/// <param name="stock">The maximum number of sets of this item which can be purchased in one day, or <see cref="F:StardewValley.Menus.ShopMenu.infiniteStock" /> for unlimited.</param>
	/// <param name="tradeItem">The qualified or unqualified item ID which must be traded to purchase this item, if applicable.</param>
	/// <param name="tradeItemCount">The number of <paramref name="tradeItem" /> needed to purchase this item, if applicable.</param>
	/// <param name="stockMode">How the <paramref name="stock" /> limit is applied in multiplayer.</param>
	/// <param name="syncedKey">A unique key to track this specific item's stock within the shop.</param>
	/// <param name="itemToSyncStack">If set, the stack count will be synchronized with the given item's. This is very specialized and only used for objects whose available stock are tracked separately from the normal shop stock tracking.</param>
	/// <param name="stackDrawType">Override how the item's stack number is drawn in the shop menu, or <c>null</c> for the default behavior.</param>
	/// <param name="actionsOnPurchase">The actions to perform when the item is purchased.</param>
	public ItemStockInformation(int price, int stock, string tradeItem = null, int? tradeItemCount = null, LimitedStockMode stockMode = LimitedStockMode.Global, string syncedKey = null, ISalable itemToSyncStack = null, StackDrawType? stackDrawType = null, List<string> actionsOnPurchase = null)
	{
		this.Price = price;
		this.Stock = stock;
		this.TradeItem = tradeItem;
		this.TradeItemCount = tradeItemCount;
		this.LimitedStockMode = stockMode;
		this.SyncedKey = syncedKey;
		this.ItemToSyncStack = itemToSyncStack;
		this.StackDrawType = stackDrawType;
		this.ActionsOnPurchase = actionsOnPurchase;
	}
}
