using System.Collections.Generic;
using Netcode;

namespace StardewValley.SpecialOrders.Rewards;

public class MoneyReward : OrderReward
{
	public NetInt amount = new NetInt(0);

	public NetFloat multiplier = new NetFloat(1f);

	public override void InitializeNetFields()
	{
		base.InitializeNetFields();
		base.NetFields.AddField(this.amount, "amount").AddField(this.multiplier, "multiplier");
	}

	public virtual int GetRewardMoneyAmount()
	{
		return (int)((float)this.amount.Value * this.multiplier.Value);
	}

	public override void Load(SpecialOrder order, Dictionary<string, string> data)
	{
		this.amount.Value = int.Parse(order.Parse(data["Amount"]));
		if (data.TryGetValue("Multiplier", out var rawValue))
		{
			this.multiplier.Value = float.Parse(order.Parse(rawValue));
		}
	}
}
