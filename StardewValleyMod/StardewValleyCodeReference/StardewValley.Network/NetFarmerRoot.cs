using Netcode;
using StardewValley.SaveSerialization;

namespace StardewValley.Network;

public class NetFarmerRoot : NetRoot<Farmer>
{
	public NetFarmerRoot()
	{
		base.Serializer = SaveSerializer.GetSerializer(typeof(Farmer));
	}

	public NetFarmerRoot(Farmer value)
		: base(value)
	{
		base.Serializer = SaveSerializer.GetSerializer(typeof(Farmer));
	}

	public override NetRoot<Farmer> Clone()
	{
		NetRoot<Farmer> result = base.Clone();
		if (Game1.serverHost != null && result.Value != null)
		{
			result.Value.teamRoot = Game1.serverHost.Value.teamRoot;
		}
		return result;
	}
}
