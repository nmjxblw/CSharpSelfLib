using System;
using Netcode;

namespace StardewValley.Network;

public class NetNPCRef : INetObject<NetFields>
{
	private readonly NetGuid guid = new NetGuid();

	public NetFields NetFields { get; } = new NetFields("NetNPCRef");

	public NetNPCRef()
	{
		this.NetFields.SetOwner(this).AddField(this.guid, "guid");
	}

	public NPC Get(GameLocation location)
	{
		if (!(this.guid.Value != Guid.Empty) || location == null || !location.characters.TryGetValue(this.guid.Value, out var npc))
		{
			return null;
		}
		return npc;
	}

	public void Set(GameLocation location, NPC npc)
	{
		if (npc == null)
		{
			this.guid.Value = Guid.Empty;
			return;
		}
		Guid newGuid = location.characters.GuidOf(npc);
		if (newGuid == Guid.Empty)
		{
			throw new ArgumentException();
		}
		this.guid.Value = newGuid;
	}

	public void Clear()
	{
		this.guid.Value = Guid.Empty;
	}
}
