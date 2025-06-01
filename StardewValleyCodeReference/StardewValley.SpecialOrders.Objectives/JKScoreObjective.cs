using System;

namespace StardewValley.SpecialOrders.Objectives;

public class JKScoreObjective : OrderObjective
{
	protected override void _Register()
	{
		base._Register();
		SpecialOrder order = base._order;
		order.onJKScoreAchieved = (Action<Farmer, int>)Delegate.Combine(order.onJKScoreAchieved, new Action<Farmer, int>(OnNewValue));
	}

	protected override void _Unregister()
	{
		base._Unregister();
		SpecialOrder order = base._order;
		order.onJKScoreAchieved = (Action<Farmer, int>)Delegate.Remove(order.onJKScoreAchieved, new Action<Farmer, int>(OnNewValue));
	}

	public virtual void OnNewValue(Farmer who, int new_value)
	{
		this.SetCount(Math.Min(Math.Max(new_value, base.currentCount.Value), base.GetMaxCount()));
	}
}
