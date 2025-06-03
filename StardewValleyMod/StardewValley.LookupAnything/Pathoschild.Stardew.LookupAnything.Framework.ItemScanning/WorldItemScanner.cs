using System;
using System.Collections.Generic;
using System.Linq;
using Netcode;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.Common.Utilities;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Locations;
using StardewValley.Objects;

namespace Pathoschild.Stardew.LookupAnything.Framework.ItemScanning;

public class WorldItemScanner
{
	private readonly IReflectionHelper Reflection;

	public WorldItemScanner(IReflectionHelper reflection)
	{
		this.Reflection = reflection;
	}

	public IEnumerable<FoundItem> GetAllOwnedItems()
	{
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		List<FoundItem> items = new List<FoundItem>();
		HashSet<Item> itemsSeen = new HashSet<Item>(new ObjectReferenceComparer<Item>());
		foreach (GameLocation location in CommonHelper.GetLocations())
		{
			foreach (Furniture furniture in location.furniture)
			{
				this.ScanAndTrack(items, itemsSeen, (Item?)(object)furniture, location, isInInventory: false, isRootInWorld: true);
			}
			FarmHouse house = (FarmHouse)(object)((location is FarmHouse) ? location : null);
			Chest val = ((house != null) ? ((NetFieldBase<Chest, NetRef<Chest>>)(object)house.fridge).Value : ((NetFieldBase<Chest, NetRef<Chest>>)(object)((IslandFarmHouse)(((location is IslandFarmHouse) ? location : null)?)).fridge).Value);
			Chest fridge = val;
			this.ScanAndTrack(items, itemsSeen, (Item?)(object)fridge, location, isInInventory: false, isRootInWorld: false, includeRoot: false);
			foreach (NPC npc in location.characters)
			{
				Hat hat = ((NetFieldBase<Hat, NetRef<Hat>>)(object)((Child)(((npc is Child) ? npc : null)?)).hat).Value ?? ((NetFieldBase<Hat, NetRef<Hat>>)(object)((Horse)(((npc is Horse) ? npc : null)?)).hat).Value;
				this.ScanAndTrack(items, itemsSeen, (Item?)(object)hat, npc);
			}
			foreach (Building building in location.buildings)
			{
				JunimoHut hut = (JunimoHut)(object)((building is JunimoHut) ? building : null);
				if (hut != null)
				{
					this.ScanAndTrack(items, itemsSeen, (Item?)(object)hut.GetOutputChest(), hut, isInInventory: false, isRootInWorld: false, includeRoot: false);
				}
				Enumerator<Chest, NetRef<Chest>> enumerator5 = building.buildingChests.GetEnumerator();
				try
				{
					while (enumerator5.MoveNext())
					{
						Chest chest = enumerator5.Current;
						this.ScanAndTrack(items, itemsSeen, (Item?)(object)chest, building, isInInventory: false, isRootInWorld: false, includeRoot: false);
					}
				}
				finally
				{
					((IDisposable)enumerator5/*cast due to .constrained prefix*/).Dispose();
				}
			}
			foreach (Object item in location.objects.Values)
			{
				if (item is Chest || !this.IsSpawnedWorldItem((Item)(object)item))
				{
					this.ScanAndTrack(items, itemsSeen, (Item?)(object)item, location, isInInventory: false, isRootInWorld: true);
				}
			}
		}
		this.ScanAndTrack(items, itemsSeen, (IEnumerable<Item>)Game1.player.Items, Game1.player, isInInventory: true);
		this.ScanAndTrack(items, (ISet<Item>)itemsSeen, (IEnumerable<Item>)new _003C_003Ez__ReadOnlyArray<Item>((Item[])(object)new Item[6]
		{
			(Item)((NetFieldBase<Clothing, NetRef<Clothing>>)(object)Game1.player.shirtItem).Value,
			(Item)((NetFieldBase<Clothing, NetRef<Clothing>>)(object)Game1.player.pantsItem).Value,
			(Item)((NetFieldBase<Boots, NetRef<Boots>>)(object)Game1.player.boots).Value,
			(Item)((NetFieldBase<Hat, NetRef<Hat>>)(object)Game1.player.hat).Value,
			(Item)((NetFieldBase<Ring, NetRef<Ring>>)(object)Game1.player.leftRing).Value,
			(Item)((NetFieldBase<Ring, NetRef<Ring>>)(object)Game1.player.rightRing).Value
		}), (object)Game1.player, isInInventory: true, isRootInWorld: false, includeRoots: true);
		Farm farm = Game1.getFarm();
		int hayCount = ((NetFieldBase<int, NetInt>)(object)((GameLocation)(farm?)).piecesOfHay).Value ?? 0;
		while (hayCount > 0)
		{
			Item hay = ItemRegistry.Create("(O)178", 1, 0, false);
			hay.Stack = Math.Min(hayCount, hay.maximumStackSize());
			hayCount -= hay.Stack;
			this.ScanAndTrack(items, itemsSeen, hay, farm);
		}
		return items;
	}

	private bool IsSpawnedWorldItem(Item item)
	{
		Object obj = (Object)(object)((item is Object) ? item : null);
		if (obj != null)
		{
			if (!obj.IsSpawnedObject && !obj.isForage())
			{
				return ((Item)obj).Category == -999;
			}
			return true;
		}
		return false;
	}

	private void ScanAndTrack(List<FoundItem> tracked, ISet<Item> itemsSeen, Item? root, object? parent, bool isInInventory = false, bool isRootInWorld = false, bool includeRoot = true)
	{
		foreach (FoundItem found in this.Scan(itemsSeen, root, parent, isInInventory, isRootInWorld, includeRoot))
		{
			tracked.Add(found);
		}
	}

	private void ScanAndTrack(List<FoundItem> tracked, ISet<Item> itemsSeen, IEnumerable<Item> roots, object parent, bool isInInventory = false, bool isRootInWorld = false, bool includeRoots = true)
	{
		foreach (FoundItem found in roots.SelectMany((Item root) => this.Scan(itemsSeen, root, parent, isInInventory, isRootInWorld, includeRoots)))
		{
			tracked.Add(found);
		}
	}

	private IEnumerable<FoundItem> Scan(ISet<Item> itemsSeen, Item? root, object? parent, bool isInInventory, bool isRootInWorld, bool includeRoot = true)
	{
		if (root == null || !itemsSeen.Add(root))
		{
			yield break;
		}
		yield return new FoundItem(root, parent, isInInventory);
		foreach (FoundItem item in this.GetDirectContents(root, isRootInWorld).SelectMany((Item p) => this.Scan(itemsSeen, p, root, isInInventory, isRootInWorld: false)))
		{
			yield return item;
		}
	}

	private IEnumerable<Item> GetDirectContents(Item? root, bool isRootInWorld)
	{
		if (root == null)
		{
			yield break;
		}
		Object obj = (Object)(object)((root is Object) ? root : null);
		if (obj != null)
		{
			if (obj.MinutesUntilReady <= 0 || obj is Cask)
			{
				yield return (Item)(object)((NetFieldBase<Object, NetRef<Object>>)(object)obj.heldObject).Value;
			}
		}
		else if (this.IsCustomItemClass(root))
		{
			Item heldItem = this.Reflection.GetField<Item>((object)root, "heldObject", false)?.GetValue() ?? this.Reflection.GetProperty<Item>((object)root, "heldObject", false)?.GetValue();
			if (heldItem != null)
			{
				yield return heldItem;
			}
		}
		StorageFurniture dresser = (StorageFurniture)(object)((root is StorageFurniture) ? root : null);
		if (dresser == null)
		{
			Chest chest = (Chest)(object)((root is Chest) ? root : null);
			if (chest == null)
			{
				Tool tool = (Tool)(object)((root is Tool) ? root : null);
				if (tool == null)
				{
					yield break;
				}
				foreach (Object item in (NetArray<Object, NetRef<Object>>)(object)tool.attachments)
				{
					yield return (Item)(object)item;
				}
			}
			else
			{
				if (isRootInWorld && !((NetFieldBase<bool, NetBool>)(object)chest.playerChest).Value)
				{
					yield break;
				}
				foreach (Item item2 in (IEnumerable<Item>)chest.GetItemsForPlayer(Game1.player.UniqueMultiplayerID))
				{
					yield return item2;
				}
			}
			yield break;
		}
		Enumerator<Item, NetRef<Item>> enumerator3 = ((NetList<Item, NetRef<Item>>)(object)dresser.heldItems).GetEnumerator();
		try
		{
			while (enumerator3.MoveNext())
			{
				yield return enumerator3.Current;
			}
		}
		finally
		{
			((IDisposable)enumerator3/*cast due to .constrained prefix*/).Dispose();
		}
	}

	private bool IsCustomItemClass(Item item)
	{
		string itemNamespace = ((object)item).GetType().Namespace ?? "";
		if (itemNamespace != "StardewValley")
		{
			return !itemNamespace.StartsWith("StardewValley.");
		}
		return false;
	}
}
