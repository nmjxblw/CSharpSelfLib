using System;
using System.Xml.Serialization;
using Netcode;

namespace StardewValley.Network;

public class NetWitnessedLock : INetObject<NetFields>
{
	private readonly NetBool requested = new NetBool().Interpolated(interpolate: false, wait: false);

	private readonly NetFarmerCollection witnesses = new NetFarmerCollection();

	private Action acquired;

	[XmlIgnore]
	public NetFields NetFields { get; } = new NetFields("NetWitnessedLock");

	public NetWitnessedLock()
	{
		this.NetFields.SetOwner(this).AddField(this.requested, "requested").AddField(this.witnesses.NetFields, "witnesses.NetFields");
	}

	public void RequestLock(Action acquired, Action failed)
	{
		if (!Game1.IsMasterGame)
		{
			throw new InvalidOperationException();
		}
		if (acquired == null)
		{
			throw new ArgumentException();
		}
		if (this.requested.Value)
		{
			failed();
			return;
		}
		this.requested.Value = true;
		this.acquired = acquired;
	}

	public bool IsLocked()
	{
		return this.requested.Value;
	}

	public void Update()
	{
		this.witnesses.RetainOnlinePlayers();
		if (!this.requested.Value)
		{
			return;
		}
		if (!this.witnesses.Contains(Game1.player))
		{
			this.witnesses.Add(Game1.player);
		}
		if (!Game1.IsMasterGame)
		{
			return;
		}
		foreach (Farmer f in Game1.otherFarmers.Values)
		{
			if (!this.witnesses.Contains(f))
			{
				return;
			}
		}
		this.acquired();
		this.acquired = null;
		this.requested.Value = false;
		this.witnesses.Clear();
	}
}
