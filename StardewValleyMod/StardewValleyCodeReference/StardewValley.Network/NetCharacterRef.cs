using System;
using Netcode;

namespace StardewValley.Network;

public class NetCharacterRef : INetObject<NetFields>
{
	private readonly NetNPCRef npc = new NetNPCRef();

	private readonly NetFarmerRef farmer = new NetFarmerRef();

	public NetFields NetFields { get; } = new NetFields("NetCharacterRef");

	public NetCharacterRef()
	{
		this.NetFields.SetOwner(this).AddField(this.npc.NetFields, "npc.NetFields").AddField(this.farmer.NetFields, "farmer.NetFields");
	}

	public Character Get(GameLocation location)
	{
		NPC npcValue = this.npc.Get(location);
		if (npcValue != null)
		{
			return npcValue;
		}
		return this.farmer.Value;
	}

	public void Set(GameLocation location, Character character)
	{
		if (!(character is NPC curNpc))
		{
			if (!(character is Farmer curFarmer))
			{
				throw new ArgumentException();
			}
			this.npc.Clear();
			this.farmer.Value = curFarmer;
		}
		else
		{
			this.npc.Set(location, curNpc);
			this.farmer.Value = null;
		}
	}

	public void Clear()
	{
		this.npc.Clear();
		this.farmer.Value = null;
	}
}
