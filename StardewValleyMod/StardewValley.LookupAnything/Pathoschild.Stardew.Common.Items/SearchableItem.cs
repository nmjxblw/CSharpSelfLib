using System;
using StardewValley;

namespace Pathoschild.Stardew.Common.Items;

internal class SearchableItem
{
	public string Type { get; }

	public Item Item { get; }

	public Func<Item> CreateItem { get; }

	public string Id { get; }

	public string QualifiedItemId { get; }

	public string Name => this.Item.Name;

	public string DisplayName => this.Item.DisplayName;

	public SearchableItem(string type, string id, Func<SearchableItem, Item> createItem)
	{
		SearchableItem arg = this;
		this.Type = type;
		this.Id = id;
		this.QualifiedItemId = this.Type + this.Id;
		this.CreateItem = () => createItem(arg);
		this.Item = createItem(this);
	}

	public bool NameContains(string substring)
	{
		if (this.Name.IndexOf(substring, StringComparison.OrdinalIgnoreCase) == -1)
		{
			return this.DisplayName.IndexOf(substring, StringComparison.OrdinalIgnoreCase) != -1;
		}
		return true;
	}

	public bool NameEquivalentTo(string name)
	{
		if (!this.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
		{
			return this.DisplayName.Equals(name, StringComparison.OrdinalIgnoreCase);
		}
		return true;
	}
}
