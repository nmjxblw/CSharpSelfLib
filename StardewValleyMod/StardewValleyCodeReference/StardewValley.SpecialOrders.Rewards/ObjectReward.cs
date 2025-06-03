using System.Collections.Generic;
using Netcode;
using Netcode.Validation;

namespace StardewValley.SpecialOrders.Rewards;

public class ObjectReward : OrderReward
{
	public readonly NetString itemKey = new NetString("");

	public readonly NetInt amount = new NetInt(0);

	[NotNetField]
	private Object _objectInstance;

	/// <summary>The item stack to be drawn on the special orders board.</summary>
	public Object objectInstance
	{
		get
		{
			if (this._objectInstance == null && !string.IsNullOrEmpty(this.itemKey.Value) && this.amount.Value > 0)
			{
				this._objectInstance = new Object(this.itemKey.Value, this.amount.Value);
			}
			return this._objectInstance;
		}
	}

	public override void InitializeNetFields()
	{
		base.InitializeNetFields();
		base.NetFields.AddField(this.itemKey, "itemKey").AddField(this.amount, "amount");
	}

	public override void Load(SpecialOrder order, Dictionary<string, string> data)
	{
		this.itemKey.Value = order.Parse(data["Item"]);
		this.amount.Value = int.Parse(order.Parse(data["Amount"]));
		this._objectInstance = new Object(this.itemKey.Value, this.amount.Value);
	}

	public override void Grant()
	{
		Object i = new Object(this.itemKey.Value, this.amount.Value);
		Game1.player.addItemByMenuIfNecessary(i);
	}
}
