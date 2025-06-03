using System;
using StardewValley;

namespace Pathoschild.Stardew.LookupAnything.Framework.ItemScanning;

public class FoundItem
{
	public object? Parent { get; }

	public Item Item { get; }

	public bool IsInInventory { get; }

	public FoundItem(Item item, object? parent, bool isInInventory)
	{
		this.Item = item;
		this.Parent = parent;
		this.IsInInventory = isInInventory;
	}

	public int GetCount()
	{
		int count = Math.Max(1, this.Item.Stack);
		if (this.Parent is Fence && this.Item is Torch && count == 93)
		{
			count = 1;
		}
		return count;
	}
}
