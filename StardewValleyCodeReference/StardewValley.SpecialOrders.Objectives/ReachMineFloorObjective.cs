using System;
using System.Collections.Generic;
using Netcode;
using StardewValley.Extensions;

namespace StardewValley.SpecialOrders.Objectives;

public class ReachMineFloorObjective : OrderObjective
{
	public NetBool skullCave = new NetBool(value: false);

	public override void InitializeNetFields()
	{
		base.InitializeNetFields();
		base.NetFields.AddField(this.skullCave, "skullCave");
	}

	public override void Load(SpecialOrder order, Dictionary<string, string> data)
	{
		base.Load(order, data);
		if (data.TryGetValue("SkullCave", out var rawValue) && rawValue.EqualsIgnoreCase("true"))
		{
			this.skullCave.Value = true;
		}
	}

	protected override void _Register()
	{
		base._Register();
		SpecialOrder order = base._order;
		order.onMineFloorReached = (Action<Farmer, int>)Delegate.Combine(order.onMineFloorReached, new Action<Farmer, int>(OnNewValue));
	}

	protected override void _Unregister()
	{
		base._Unregister();
		SpecialOrder order = base._order;
		order.onMineFloorReached = (Action<Farmer, int>)Delegate.Remove(order.onMineFloorReached, new Action<Farmer, int>(OnNewValue));
	}

	public virtual void OnNewValue(Farmer who, int new_value)
	{
		if (this.skullCave.Value)
		{
			new_value -= 120;
		}
		else if (new_value > 120)
		{
			return;
		}
		if (new_value > 0)
		{
			this.SetCount(Math.Min(Math.Max(new_value, base.currentCount.Value), base.GetMaxCount()));
		}
	}
}
