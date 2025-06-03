using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Delegates;
using StardewValley.Inventories;
using StardewValley.Locations;
using StardewValley.SpecialOrders;

namespace StardewValley.Internal;

/// <summary>Iterates through every item in the game state and optionally edits, replaces, or removes instances.</summary>
/// <remarks>This is a low-level class. Most code should use a utility method like <see cref="M:StardewValley.Utility.ForEachItem(System.Func{StardewValley.Item,System.Boolean})" /> or <see cref="M:StardewValley.Utility.ForEachItemContext(StardewValley.Delegates.ForEachItemDelegate)" /> instead.</remarks>
public static class ForEachItemHelper
{
	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass4_0<TItem> where TItem : Item
	{
		public GetForEachItemPathDelegate getParentPath;

		public IList<TItem> list;

		public bool leaveNullSlotsOnRemoval;

		public Action<Item, Item, int> onChanged;
	}

	/// <summary>Perform an action for each item in the game world, including items within items (e.g. in a chest or on a table), hats placed on children, items in player inventories, etc.</summary>
	/// <param name="handler">The action to perform for each item.</param>
	/// <returns>Returns whether to continue iterating if needed (i.e. returns false if the last <paramref name="handler" /> call did).</returns>
	public static bool ForEachItemInWorld(ForEachItemDelegate handler)
	{
		bool canContinue = true;
		Utility.ForEachLocation((GameLocation location) => canContinue = ForEachItemHelper.ForEachItemInLocation(location, handler));
		if (!canContinue)
		{
			return false;
		}
		foreach (Farmer allFarmer in Game1.getAllFarmers())
		{
			Farmer farmer = allFarmer;
			int toolIndex = farmer.CurrentToolIndex;
			if (!ForEachItemHelper.ApplyToList(farmer.Items, handler, GetParentPath, leaveNullSlotsOnRemoval: true, OnChangedItemSlot) || !ForEachItemHelper.ApplyToField(farmer.shirtItem, handler, GetParentPath, OnChangedEquipment) || !ForEachItemHelper.ApplyToField(farmer.pantsItem, handler, GetParentPath, OnChangedEquipment) || !ForEachItemHelper.ApplyToField(farmer.boots, handler, GetParentPath, OnChangedEquipment) || !ForEachItemHelper.ApplyToField(farmer.hat, handler, GetParentPath, OnChangedEquipment) || !ForEachItemHelper.ApplyToField(farmer.leftRing, handler, GetParentPath, OnChangedEquipment) || !ForEachItemHelper.ApplyToField(farmer.rightRing, handler, GetParentPath, OnChangedEquipment) || !ForEachItemHelper.ApplyToItem(farmer.recoveredItem, handler, delegate
			{
				farmer.recoveredItem = null;
			}, delegate(Item newItem)
			{
				farmer.recoveredItem = ForEachItemHelper.PrepareForReplaceWith(farmer.recoveredItem, newItem);
			}, GetParentPath) || !ForEachItemHelper.ApplyToField(farmer.toolBeingUpgraded, handler, GetParentPath) || !ForEachItemHelper.ApplyToList(farmer.itemsLostLastDeath, handler, GetParentPath))
			{
				return false;
			}
			IList<object> GetParentPath()
			{
				return new List<object> { farmer };
			}
			void OnChangedEquipment(Item oldItem, Item newItem)
			{
				oldItem?.onUnequip(farmer);
				newItem?.onEquip(farmer);
			}
			void OnChangedItemSlot(Item oldItem, Item newItem, int index)
			{
				if (index == toolIndex)
				{
					(oldItem as Tool)?.onUnequip(farmer);
					(newItem as Tool)?.onEquip(farmer);
				}
			}
		}
		if (!ForEachItemHelper.ApplyToList(Game1.player.team.returnedDonations, handler, GetParentPathForTeam))
		{
			return false;
		}
		foreach (Inventory value in Game1.player.team.globalInventories.Values)
		{
			if (!ForEachItemHelper.ApplyToList(value, handler, GetParentPathForTeam))
			{
				return false;
			}
		}
		foreach (SpecialOrder order in Game1.player.team.specialOrders)
		{
			if (!ForEachItemHelper.ApplyToList(order.donatedItems, handler, () => ForEachItemHelper.CombinePath(GetParentPathForTeam, Game1.player.team.specialOrders, order)))
			{
				return false;
			}
		}
		return true;
		static IList<object> GetParentPathForTeam()
		{
			return new List<object> { Game1.player.team };
		}
	}

	/// <summary>Perform an action for each item within a location, including items within items (e.g. in a chest or on a table), hats placed on children, items in player inventories, etc.</summary>
	/// <param name="location">The location whose items to iterate.</param>
	/// <param name="handler">The action to perform for each item.</param>
	/// <returns>Returns whether to continue iterating if needed (i.e. returns false if the last <paramref name="handler" /> call did).</returns>
	public static bool ForEachItemInLocation(GameLocation location, ForEachItemDelegate handler)
	{
		if (location == null)
		{
			return true;
		}
		if (!ForEachItemHelper.ApplyToList(location.furniture, handler, GetLocationPath))
		{
			return false;
		}
		foreach (NPC character2 in location.characters)
		{
			NPC character = character2;
			if (!(character is Child child))
			{
				if (!(character is Horse horse))
				{
					if (character is Pet pet && !ForEachItemHelper.ApplyToField(pet.hat, handler, GetNpcPath))
					{
						return false;
					}
				}
				else if (!ForEachItemHelper.ApplyToField(horse.hat, handler, GetNpcPath))
				{
					return false;
				}
			}
			else if (!ForEachItemHelper.ApplyToField(child.hat, handler, GetNpcPath))
			{
				return false;
			}
			IList<object> GetNpcPath()
			{
				return ForEachItemHelper.CombinePath(GetLocationPath, character);
			}
		}
		foreach (Building building in location.buildings)
		{
			if (!building.ForEachItemContextExcludingInterior(handler, GetLocationPath))
			{
				return false;
			}
		}
		if ((!(location.GetFridge(onlyUnlocked: false)?.ForEachItem(handler, GetLocationPath))) ?? false)
		{
			return false;
		}
		if (location.objects.Length > 0)
		{
			foreach (Vector2 tile in location.objects.Keys)
			{
				Object obj = location.objects[tile];
				if (!ForEachItemHelper.ApplyToItem(obj, handler, delegate
				{
					location.objects.Remove(tile);
				}, delegate(Item newItem)
				{
					location.objects[tile] = ForEachItemHelper.PrepareForReplaceWith(obj, (Object)newItem);
				}, () => ForEachItemHelper.CombinePath(GetLocationPath, location.objects)))
				{
					return false;
				}
			}
		}
		for (int i = location.debris.Count - 1; i >= 0; i--)
		{
			Debris d = location.debris[i];
			if (d.item != null && !ForEachItemHelper.ApplyToItem(d.item, handler, Remove, ReplaceWith, () => ForEachItemHelper.CombinePath(GetLocationPath, location.debris)))
			{
				return false;
			}
			void Remove()
			{
				if (d.itemId.Value == null || ItemRegistry.HasItemId(d.item, d.itemId.Value))
				{
					location.debris.RemoveAt(i);
				}
				else
				{
					d.item = null;
				}
			}
			void ReplaceWith(Item newItem)
			{
				if (ItemRegistry.HasItemId(newItem, d.itemId.Value))
				{
					d.itemId.Value = newItem.QualifiedItemId;
				}
				d.item = ForEachItemHelper.PrepareForReplaceWith(d.item, newItem);
			}
		}
		ShopLocation shopLocation = location as ShopLocation;
		if (shopLocation != null)
		{
			if (!ForEachItemHelper.ApplyToList(shopLocation.itemsFromPlayerToSell, handler, () => ForEachItemHelper.CombinePath(GetLocationPath, shopLocation.itemsFromPlayerToSell)))
			{
				return false;
			}
			if (!ForEachItemHelper.ApplyToList(shopLocation.itemsToStartSellingTomorrow, handler, () => ForEachItemHelper.CombinePath(GetLocationPath, shopLocation.itemsToStartSellingTomorrow)))
			{
				return false;
			}
		}
		return true;
		IList<object> GetLocationPath()
		{
			return new List<object> { location };
		}
	}

	/// <summary>Apply a for-each-item callback to an item.</summary>
	/// <typeparam name="TItem">The item type.</typeparam>
	/// <param name="item">The item instance to iterate.</param>
	/// <param name="handler">The action to perform for each item.</param>
	/// <param name="remove">Delete this item instance.</param>
	/// <param name="replaceWith">Replace this item with a new instance.</param>
	/// <param name="getParentPath">Get the contextual path leading to this item (excluding the item itself).</param>
	/// <returns>Returns whether to continue iterating if needed.</returns>
	public static bool ApplyToItem<TItem>(TItem item, ForEachItemDelegate handler, Action remove, Action<Item> replaceWith, GetForEachItemPathDelegate getParentPath) where TItem : Item
	{
		if (item == null)
		{
			return true;
		}
		if (handler(new ForEachItemContext(item, Remove, ReplaceWith, getParentPath)))
		{
			return item?.ForEachItem(handler, () => ForEachItemHelper.CombinePath(getParentPath, item)) ?? true;
		}
		return false;
		void Remove()
		{
			remove();
			item = null;
		}
		void ReplaceWith(Item newItem)
		{
			if (newItem == null)
			{
				Remove();
			}
			else
			{
				item = ForEachItemHelper.PrepareForReplaceWith(item, (TItem)newItem);
				replaceWith(item);
			}
		}
	}

	/// <summary>Apply a for-each-item callback to an item.</summary>
	/// <typeparam name="TItem">The item type.</typeparam>
	/// <param name="field">The field instance to iterate.</param>
	/// <param name="handler">The action to perform for each item.</param>
	/// <param name="getParentPath">Get the contextual path leading to this field, excluding the field itself.</param>
	/// <param name="onChanged">A callback to invoke when the assigned value changes, which receives the old and new items.</param>
	/// <returns>Returns whether to continue iterating if needed.</returns>
	public static bool ApplyToField<TItem>(NetRef<TItem> field, ForEachItemDelegate handler, GetForEachItemPathDelegate getParentPath, Action<Item, Item> onChanged = null) where TItem : Item
	{
		Item oldValue = field.Value;
		return ForEachItemHelper.ApplyToItem(field.Value, handler, Remove, ReplaceWith, GetPath);
		IList<object> GetPath()
		{
			return ForEachItemHelper.CombinePath(getParentPath, field);
		}
		void Remove()
		{
			field.Value = null;
			onChanged?.Invoke(oldValue, null);
		}
		void ReplaceWith(Item newItem)
		{
			field.Value = ForEachItemHelper.PrepareForReplaceWith(field.Value, (TItem)newItem);
			onChanged?.Invoke(oldValue, newItem);
		}
	}

	/// <summary>Apply a for-each-item callback to an item.</summary>
	/// <typeparam name="TItem">The item type.</typeparam>
	/// <param name="list">The list of items to iterate.</param>
	/// <param name="handler">The action to perform for each item.</param>
	/// <param name="getParentPath">Get the contextual path leading to this list, excluding the list itself.</param>
	/// <param name="leaveNullSlotsOnRemoval">Whether to leave a null entry in the list when an item is removed. If <c>false</c>, the index is removed from the list instead.</param>
	/// <param name="onChanged">A callback to invoke when the assigned value changes, which receives the old and new items.</param>
	/// <returns>Returns whether to continue iterating if needed.</returns>
	public static bool ApplyToList<TItem>(IList<TItem> list, ForEachItemDelegate handler, GetForEachItemPathDelegate getParentPath, bool leaveNullSlotsOnRemoval = false, Action<Item, Item, int> onChanged = null) where TItem : Item
	{
		for (int i = list.Count - 1; i >= 0; i--)
		{
			Item oldValue = list[i];
			if (!ForEachItemHelper.ApplyToItem(list[i], handler, Remove, ReplaceWith, GetPath))
			{
				return false;
			}
			void Remove()
			{
				if (leaveNullSlotsOnRemoval)
				{
					list[i] = null;
				}
				else
				{
					list.RemoveAt(i);
				}
				onChanged?.Invoke(oldValue, null, i);
			}
			void ReplaceWith(Item newItem)
			{
				list[i] = ForEachItemHelper.PrepareForReplaceWith(list[i], (TItem)newItem);
				onChanged?.Invoke(oldValue, newItem, i);
			}
		}
		return true;
		IList<object> GetPath()
		{
			return ForEachItemHelper.CombinePath(((_003C_003Ec__DisplayClass4_0<TItem>)this).getParentPath, ((_003C_003Ec__DisplayClass4_0<TItem>)this).list);
		}
	}

	/// <summary>Combine the result of a <see cref="T:StardewValley.Delegates.GetForEachItemPathDelegate" /> parent path with child paths into a single path.</summary>
	/// <param name="parentPath">The parent path, or <c>null</c> to start the root at the first <paramref name="pathValues" /> value.</param>
	/// <param name="pathValues">The path segments to append.</param>
	/// <returns></returns>
	public static IList<object> CombinePath(GetForEachItemPathDelegate parentPath, params object[] pathValues)
	{
		IList<object> combined = parentPath?.Invoke() ?? new List<object>();
		foreach (object pathValue in pathValues)
		{
			combined.Add(pathValue);
		}
		return combined;
	}

	/// <summary>Prepare a new item instance as a replacement for an existing item.</summary>
	/// <param name="previousItem">The existing item that's being replaced.</param>
	/// <param name="newItem">The new item that will replace <paramref name="previousItem" />.</param>
	/// <returns>Returns the <paramref name="newItem" /> for convenience.</returns>
	private static TItem PrepareForReplaceWith<TItem>(TItem previousItem, TItem newItem) where TItem : Item
	{
		Object previousObj = previousItem as Object;
		Object newObj = newItem as Object;
		if (previousObj != null && newObj != null)
		{
			newObj.TileLocation = previousObj.TileLocation;
		}
		return newItem;
	}
}
