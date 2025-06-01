using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Netcode;

namespace StardewValley.SpecialOrders.Objectives;

public class DeliverObjective : OrderObjective
{
	[XmlElement("acceptableContextTagSets")]
	public NetStringList acceptableContextTagSets = new NetStringList();

	[XmlElement("targetName")]
	public NetString targetName = new NetString();

	[XmlElement("message")]
	public NetString message = new NetString();

	public override void Load(SpecialOrder order, Dictionary<string, string> data)
	{
		if (data.TryGetValue("AcceptedContextTags", out var rawValue))
		{
			this.acceptableContextTagSets.Add(order.Parse(rawValue));
		}
		if (data.TryGetValue("TargetName", out rawValue))
		{
			this.targetName.Value = order.Parse(rawValue);
		}
		else
		{
			this.targetName.Value = base._order.requester.Value;
		}
		if (data.TryGetValue("Message", out rawValue))
		{
			this.message.Value = order.Parse(rawValue);
		}
		else
		{
			this.message.Value = "";
		}
	}

	public override void InitializeNetFields()
	{
		base.InitializeNetFields();
		base.NetFields.AddField(this.acceptableContextTagSets, "acceptableContextTagSets").AddField(this.targetName, "targetName").AddField(this.message, "message");
	}

	public override bool ShouldShowProgress()
	{
		return false;
	}

	protected override void _Register()
	{
		base._Register();
		SpecialOrder order = base._order;
		order.onItemDelivered = (Func<Farmer, NPC, Item, bool, int>)Delegate.Combine(order.onItemDelivered, new Func<Farmer, NPC, Item, bool, int>(OnItemDelivered));
	}

	protected override void _Unregister()
	{
		base._Unregister();
		SpecialOrder order = base._order;
		order.onItemDelivered = (Func<Farmer, NPC, Item, bool, int>)Delegate.Remove(order.onItemDelivered, new Func<Farmer, NPC, Item, bool, int>(OnItemDelivered));
	}

	public virtual int OnItemDelivered(Farmer farmer, NPC npc, Item item, bool probe)
	{
		if (this.IsComplete())
		{
			return 0;
		}
		if (npc.Name != this.targetName.Value)
		{
			return 0;
		}
		bool is_valid_delivery = true;
		foreach (string acceptableContextTagSet in this.acceptableContextTagSets)
		{
			is_valid_delivery = false;
			bool fail = false;
			string[] array = acceptableContextTagSet.Split(',');
			for (int i = 0; i < array.Length; i++)
			{
				if (!ItemContextTagManager.DoAnyTagsMatch(array[i].Split('/'), item.GetContextTags()))
				{
					fail = true;
					break;
				}
			}
			if (!fail)
			{
				is_valid_delivery = true;
				break;
			}
		}
		if (!is_valid_delivery)
		{
			return 0;
		}
		int required_amount = base.GetMaxCount() - base.GetCount();
		int donated_amount = Math.Min(item.Stack, required_amount);
		if (donated_amount < required_amount)
		{
			return 0;
		}
		if (!probe)
		{
			Item donated_item = item.getOne();
			donated_item.Stack = donated_amount;
			base._order.donatedItems.Add(donated_item);
			item.Stack -= donated_amount;
			this.IncrementCount(donated_amount);
			if (!string.IsNullOrEmpty(this.message.Value))
			{
				npc.CurrentDialogue.Push(new Dialogue(npc, null, this.message.Value));
				Game1.drawDialogue(npc);
			}
		}
		return donated_amount;
	}
}
