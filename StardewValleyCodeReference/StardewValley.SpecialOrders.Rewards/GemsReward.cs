using System.Collections.Generic;
using Netcode;

namespace StardewValley.SpecialOrders.Rewards;

public class GemsReward : OrderReward
{
	public NetInt amount = new NetInt(0);

	public override void InitializeNetFields()
	{
		base.InitializeNetFields();
		base.NetFields.AddField(this.amount, "amount");
	}

	public override void Load(SpecialOrder order, Dictionary<string, string> data)
	{
		this.amount.Value = int.Parse(order.Parse(data["Amount"]));
	}

	public override void Grant()
	{
		Game1.player.QiGems += this.amount.Value;
	}
}
