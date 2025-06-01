using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Netcode;
using StardewValley.SaveSerialization;

namespace StardewValley.Inventories;

/// <summary>A managed list of items.</summary>
[XmlRoot("items")]
public class Inventory : INetObject<NetFields>, IXmlSerializable, IInventory, IList<Item>, ICollection<Item>, IEnumerable<Item>, IEnumerable
{
	/// <summary>The underlying list of items.</summary>
	private readonly NetObjectList<Item> Items = new NetObjectList<Item>();

	/// <summary>The <see cref="F:StardewValley.Inventories.Inventory.Items" /> indexed by their qualified item ID.</summary>
	private InventoryIndex ItemsById;

	/// <summary>The backing field for <see cref="M:StardewValley.Inventories.Inventory.CountItemStacks" />.</summary>
	private int? CachedItemStackCount;

	/// <inheritdoc />
	public NetFields NetFields { get; } = new NetFields("Inventory");

	/// <summary>The number of items in the inventory, including <c>null</c> slots.</summary>
	public int Count => this.Items.Count;

	/// <inheritdoc />
	public bool IsReadOnly => this.Items.IsReadOnly;

	/// <inheritdoc />
	public Item this[int index]
	{
		get
		{
			return this.Items[index];
		}
		set
		{
			this.Items[index] = value;
		}
	}

	/// <inheritdoc />
	public bool IsLocalPlayerInventory { get; set; }

	/// <inheritdoc />
	public long LastTickSlotChanged { get; private set; }

	/// <summary>An event raised when an item stack is added or removed.</summary>
	public event OnSlotChangedDelegate OnSlotChanged;

	/// <summary>An event raised when the inventory is cleared or replaced.</summary>
	public event OnInventoryReplacedDelegate OnInventoryReplaced;

	/// <summary>Construct an instance.</summary>
	public Inventory()
	{
		this.NetFields.SetOwner(this).AddField(this.Items, "this.Items");
		this.Items.OnElementChanged += HandleElementChanged;
		this.Items.OnArrayReplaced += HandleArrayReplaced;
	}

	/// <inheritdoc />
	public bool HasAny()
	{
		return this.GetItemsById().CountKeys() > 0;
	}

	/// <inheritdoc />
	public bool HasEmptySlots()
	{
		return this.Count > this.CountItemStacks();
	}

	/// <inheritdoc />
	public int CountItemStacks()
	{
		int? cachedItemStackCount = this.CachedItemStackCount;
		if (!cachedItemStackCount.HasValue)
		{
			int? num = (this.CachedItemStackCount = this.GetItemsById().CountItems());
			return num.Value;
		}
		return cachedItemStackCount.GetValueOrDefault();
	}

	/// <inheritdoc />
	public void OverwriteWith(IList<Item> list)
	{
		if (this != list && this.Items != list)
		{
			this.ClearIndex();
			this.Items.CopyFrom(list);
		}
	}

	/// <inheritdoc />
	public IList<Item> GetRange(int index, int count)
	{
		return this.Items.GetRange(index, count);
	}

	/// <inheritdoc />
	public void AddRange(ICollection<Item> collection)
	{
		this.Items.AddRange(collection);
	}

	/// <inheritdoc />
	public void RemoveRange(int index, int count)
	{
		this.Items.RemoveRange(index, count);
	}

	/// <inheritdoc />
	public void RemoveEmptySlots()
	{
		if (!this.HasEmptySlots())
		{
			return;
		}
		for (int i = this.Count - 1; i >= 0; i--)
		{
			if (this[i] == null)
			{
				this.RemoveAt(i);
			}
		}
	}

	/// <inheritdoc />
	public bool ContainsId(string itemId)
	{
		itemId = ItemRegistry.QualifyItemId(itemId);
		if (itemId == null)
		{
			return false;
		}
		return this.GetItemsById().Contains(itemId);
	}

	/// <inheritdoc />
	public bool ContainsId(string itemId, int minimum)
	{
		itemId = ItemRegistry.QualifyItemId(itemId);
		if (itemId == null)
		{
			return false;
		}
		if (this.GetItemsById().TryGet(itemId, out var items))
		{
			if (minimum <= 1)
			{
				return true;
			}
			int count = 0;
			foreach (Item item in items)
			{
				if (item.QualifiedItemId == itemId)
				{
					count += item.Stack;
				}
				if (count >= minimum)
				{
					return true;
				}
			}
		}
		return false;
	}

	/// <inheritdoc />
	public int CountId(string itemId)
	{
		itemId = ItemRegistry.QualifyItemId(itemId);
		if (itemId == null)
		{
			return 0;
		}
		if (this.GetItemsById().TryGet(itemId, out var items))
		{
			int count = 0;
			{
				foreach (Item item in items)
				{
					if (item.QualifiedItemId == itemId)
					{
						count += item.Stack;
					}
				}
				return count;
			}
		}
		return 0;
	}

	/// <inheritdoc />
	public IEnumerable<Item> GetById(string itemId)
	{
		itemId = ItemRegistry.QualifyItemId(itemId);
		if (itemId == null || !this.GetItemsById().TryGet(itemId, out var items))
		{
			return LegacyShims.EmptyArray<Item>();
		}
		return items;
	}

	/// <inheritdoc />
	public int Reduce(Item item, int count, bool reduceRemainderFromInventory = false)
	{
		int index = -1;
		if (this.IsLocalPlayerInventory)
		{
			index = Game1.player.CurrentToolIndex;
			if (index < 0 || index >= this.Count || this.Items[index] != item)
			{
				index = -1;
			}
		}
		if (index < 0)
		{
			index = this.IndexOf(item);
		}
		int remaining = count;
		if (index > -1)
		{
			remaining -= item.Stack;
			this.Items[index] = item.ConsumeStack(count);
		}
		else
		{
			Game1.log.Warn($"Can't deduct item with ID '{item.QualifiedItemId}' from {(this.IsLocalPlayerInventory ? "the player's" : "this")} inventory because it's not in that inventory.");
		}
		if (reduceRemainderFromInventory && remaining > 0)
		{
			remaining -= this.ReduceId(item.QualifiedItemId, remaining);
		}
		if (remaining > 0)
		{
			return count - remaining;
		}
		return count;
	}

	/// <inheritdoc />
	public int ReduceId(string itemId, int count)
	{
		itemId = ItemRegistry.QualifyItemId(itemId);
		if (itemId == null)
		{
			return 0;
		}
		InventoryIndex itemsById = this.GetItemsById();
		if (itemsById.TryGetMutable(itemId, out var items))
		{
			bool anyStacksRemoved = false;
			int remaining = count;
			for (int i = 0; i < items.Count; i++)
			{
				if (remaining <= 0)
				{
					break;
				}
				Item item = items[i];
				int toRemove = Math.Min(remaining, item.Stack);
				items[i] = item.ConsumeStack(toRemove);
				if (items[i] == null)
				{
					anyStacksRemoved = true;
					items.RemoveAt(i);
					item.SetTempData("__Inventory_ReduceId_Remove", "");
					i--;
				}
				remaining -= toRemove;
			}
			if (items.Count == 0)
			{
				itemsById.RemoveKey(itemId);
			}
			if (anyStacksRemoved)
			{
				for (int i2 = this.Items.Count - 1; i2 >= 0; i2--)
				{
					if (this.Items[i2]?.tempData?.Remove("__Inventory_ReduceId_Remove") ?? false)
					{
						this.Items[i2] = null;
					}
				}
			}
			return count - remaining;
		}
		return 0;
	}

	/// <inheritdoc />
	public bool RemoveButKeepEmptySlot(Item item)
	{
		if (item == null)
		{
			return false;
		}
		int index = this.Items.IndexOf(item);
		if (index == -1)
		{
			return false;
		}
		this.Items[index] = null;
		return true;
	}

	/// <inheritdoc />
	public IEnumerator<Item> GetEnumerator()
	{
		return this.Items.GetEnumerator();
	}

	/// <inheritdoc />
	IEnumerator IEnumerable.GetEnumerator()
	{
		return this.Items.GetEnumerator();
	}

	/// <inheritdoc />
	public void Add(Item item)
	{
		this.Items.Add(item);
	}

	/// <inheritdoc />
	public void Clear()
	{
		this.ClearIndex();
		this.Items.Clear();
	}

	/// <inheritdoc />
	public bool Contains(Item item)
	{
		if (item == null)
		{
			return false;
		}
		if (this.GetItemsById().TryGetMutable(item.QualifiedItemId, out var list))
		{
			return list.Contains(item);
		}
		return false;
	}

	/// <inheritdoc />
	public void CopyTo(Item[] array, int arrayIndex)
	{
		this.Items.CopyTo(array, arrayIndex);
	}

	/// <inheritdoc />
	public bool Remove(Item item)
	{
		if (item == null)
		{
			return false;
		}
		return this.Items.Remove(item);
	}

	/// <inheritdoc />
	public int IndexOf(Item item)
	{
		return this.Items.IndexOf(item);
	}

	/// <inheritdoc />
	public void Insert(int index, Item item)
	{
		this.Items.Insert(index, item);
	}

	/// <inheritdoc />
	public void RemoveAt(int index)
	{
		this.Items.RemoveAt(index);
	}

	/// <inheritdoc />
	public XmlSchema GetSchema()
	{
		return null;
	}

	/// <inheritdoc />
	public void ReadXml(XmlReader reader)
	{
		bool isEmptyElement = reader.IsEmptyElement;
		reader.Read();
		if (!isEmptyElement)
		{
			while (reader.NodeType != XmlNodeType.EndElement)
			{
				Item item = SaveSerializer.Deserialize<Item>(reader);
				this.Items.Add(item);
				reader.MoveToContent();
			}
			reader.ReadEndElement();
		}
	}

	/// <inheritdoc />
	public void WriteXml(XmlWriter writer)
	{
		foreach (Item item in this.Items)
		{
			SaveSerializer.Serialize(writer, item);
		}
	}

	/// <summary>Get an index of items by ID.</summary>
	private InventoryIndex GetItemsById()
	{
		return this.ItemsById ?? (this.ItemsById = InventoryIndex.ById(this.Items));
	}

	/// <summary>Handle the <see cref="F:StardewValley.Inventories.Inventory.Items" /> data getting replaced.</summary>
	/// <param name="list">The item list.</param>
	/// <param name="before">The previous item list.</param>
	/// <param name="after">The new item list.</param>
	private void HandleArrayReplaced(NetList<Item, NetRef<Item>> list, IList<Item> before, IList<Item> after)
	{
		if (before.Count != 0 || after.Count != 0)
		{
			this.ClearIndex();
			this.CachedItemStackCount = null;
			this.LastTickSlotChanged = DateTime.UtcNow.Ticks;
			this.OnInventoryReplaced?.Invoke(this, before, after);
		}
	}

	/// <summary>Handle a slot in the <see cref="F:StardewValley.Inventories.Inventory.Items" /> data changing.</summary>
	/// <param name="list">The item list.</param>
	/// <param name="index">The item slot's index within the inventory.</param>
	/// <param name="before">The previous item value (which may be <c>null</c> when adding a stack).</param>
	/// <param name="after">The new item value (which may be <c>null</c> when removing a stack).</param>
	private void HandleElementChanged(NetList<Item, NetRef<Item>> list, int index, Item before, Item after)
	{
		if (before != after)
		{
			this.ItemsById?.Remove(before);
			this.ItemsById?.Add(after);
			this.CachedItemStackCount = null;
			this.LastTickSlotChanged = DateTime.UtcNow.Ticks;
			this.OnSlotChanged?.Invoke(this, index, before, after);
		}
	}

	/// <summary>Clear the item index, so it'll be rebuilt next time it's needed.</summary>
	private void ClearIndex()
	{
		this.ItemsById = null;
	}
}
