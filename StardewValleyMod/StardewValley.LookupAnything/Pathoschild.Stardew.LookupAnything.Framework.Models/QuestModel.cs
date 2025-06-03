using System;
using System.Collections.Generic;
using System.Linq;
using Netcode;
using StardewValley;
using StardewValley.Quests;
using StardewValley.SpecialOrders;
using StardewValley.SpecialOrders.Objectives;

namespace Pathoschild.Stardew.LookupAnything.Framework.Models;

internal class QuestModel
{
	private readonly Func<Item, bool> NeedsItemImpl;

	private static readonly DonateObjective DonateObjective = new DonateObjective();

	public string DisplayText { get; }

	public QuestModel(Quest quest)
	{
		QuestModel questModel = this;
		this.DisplayText = quest.GetName();
		this.NeedsItemImpl = (Item item) => questModel.NeedsItem(quest, item);
	}

	public QuestModel(SpecialOrder order)
	{
		QuestModel questModel = this;
		this.DisplayText = order.GetName();
		this.NeedsItemImpl = (Item item) => questModel.NeedsItem(order, item);
	}

	public bool NeedsItem(Object? obj)
	{
		if (obj != null)
		{
			return this.NeedsItemImpl((Item)(object)obj);
		}
		return false;
	}

	private bool NeedsItem(Quest quest, Item? item)
	{
		if (item == null)
		{
			return false;
		}
		CraftingQuest required = (CraftingQuest)(object)((quest is CraftingQuest) ? quest : null);
		if (required == null)
		{
			ItemDeliveryQuest required2 = (ItemDeliveryQuest)(object)((quest is ItemDeliveryQuest) ? quest : null);
			if (required2 == null)
			{
				ItemHarvestQuest required3 = (ItemHarvestQuest)(object)((quest is ItemHarvestQuest) ? quest : null);
				if (required3 == null)
				{
					LostItemQuest required4 = (LostItemQuest)(object)((quest is LostItemQuest) ? quest : null);
					if (required4 == null)
					{
						ResourceCollectionQuest required5 = (ResourceCollectionQuest)(object)((quest is ResourceCollectionQuest) ? quest : null);
						if (required5 == null)
						{
							SecretLostItemQuest required6 = (SecretLostItemQuest)(object)((quest is SecretLostItemQuest) ? quest : null);
							if (required6 != null)
							{
								return ((NetFieldBase<string, NetString>)(object)required6.ItemId).Value == item.QualifiedItemId;
							}
							return false;
						}
						return ((NetFieldBase<string, NetString>)(object)required5.ItemId).Value == item.QualifiedItemId;
					}
					return ((NetFieldBase<string, NetString>)(object)required4.ItemId).Value == item.QualifiedItemId;
				}
				return ((NetFieldBase<string, NetString>)(object)required3.ItemId).Value == item.QualifiedItemId;
			}
			return ((NetFieldBase<string, NetString>)(object)required2.ItemId).Value == item.QualifiedItemId;
		}
		return ((NetFieldBase<string, NetString>)(object)required.ItemId).Value == item.QualifiedItemId;
	}

	private bool NeedsItem(SpecialOrder order, Item? item)
	{
		return ((IEnumerable<OrderObjective>)order.objectives).Any(delegate(OrderObjective objective)
		{
			CollectObjective val = (CollectObjective)(object)((objective is CollectObjective) ? objective : null);
			if (val != null)
			{
				return this.IsMatch(item, val.acceptableContextTagSets);
			}
			DeliverObjective val2 = (DeliverObjective)(object)((objective is DeliverObjective) ? objective : null);
			if (val2 != null)
			{
				return this.IsMatch(item, val2.acceptableContextTagSets);
			}
			DonateObjective val3 = (DonateObjective)(object)((objective is DonateObjective) ? objective : null);
			if (val3 != null)
			{
				return val3.IsValidItem(item);
			}
			FishObjective val4 = (FishObjective)(object)((objective is FishObjective) ? objective : null);
			if (val4 != null)
			{
				return this.IsMatch(item, val4.acceptableContextTagSets);
			}
			GiftObjective val5 = (GiftObjective)(object)((objective is GiftObjective) ? objective : null);
			if (val5 != null)
			{
				return this.IsMatch(item, val5.acceptableContextTagSets);
			}
			ShipObjective val6 = (ShipObjective)(object)((objective is ShipObjective) ? objective : null);
			return val6 != null && this.IsMatch(item, val6.acceptableContextTagSets);
		});
	}

	private bool IsMatch(Item? item, NetStringList contextTags)
	{
		QuestModel.DonateObjective.acceptableContextTagSets = contextTags;
		return QuestModel.DonateObjective.IsValidItem(item);
	}
}
