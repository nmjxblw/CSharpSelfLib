using System.Collections.Generic;
using System.Xml.Serialization;
using Netcode;

namespace StardewValley.SpecialOrders.Rewards;

public class ResetEventReward : OrderReward
{
	[XmlArrayItem("int")]
	public NetStringList resetEvents = new NetStringList();

	public override void InitializeNetFields()
	{
		base.InitializeNetFields();
		base.NetFields.AddField(this.resetEvents, "resetEvents");
	}

	public override void Load(SpecialOrder order, Dictionary<string, string> data)
	{
		string raw = order.Parse(data["ResetEvents"]);
		this.resetEvents.AddRange(ArgUtility.SplitBySpace(raw));
	}

	public override void Grant()
	{
		foreach (string event_index in this.resetEvents)
		{
			Game1.player.eventsSeen.Remove(event_index);
		}
	}
}
