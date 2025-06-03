using System.Collections;
using System.Collections.Generic;
using Netcode;

namespace StardewValley.Network;

public class NetFarmerRef : INetObject<NetFields>, IEnumerable<long?>, IEnumerable
{
	public readonly NetBool defined = new NetBool();

	public readonly NetLong uid = new NetLong();

	public NetFields NetFields { get; } = new NetFields("NetFarmerRef");

	public long UID
	{
		get
		{
			if (!this.defined.Value)
			{
				return 0L;
			}
			return this.uid.Value;
		}
		set
		{
			this.uid.Value = value;
			this.defined.Value = true;
		}
	}

	public Farmer Value
	{
		get
		{
			if (!this.defined.Value)
			{
				return null;
			}
			return this.getFarmer(this.uid.Value);
		}
		set
		{
			this.defined.Value = value != null;
			this.uid.Value = value?.UniqueMultiplayerID ?? 0;
		}
	}

	public NetFarmerRef()
	{
		this.NetFields.SetOwner(this).AddField(this.defined, "defined").AddField(this.uid, "uid");
	}

	private Farmer getFarmer(long uid)
	{
		foreach (Farmer farmer in Game1.getAllFarmers())
		{
			if (farmer.UniqueMultiplayerID == uid)
			{
				return farmer;
			}
		}
		return null;
	}

	public NetFarmerRef Delayed(bool interpolationWait)
	{
		this.defined.Interpolated(interpolate: false, interpolationWait);
		this.uid.Interpolated(interpolate: false, interpolationWait);
		return this;
	}

	public void Set(NetFarmerRef other)
	{
		this.uid.Value = other.uid.Value;
		this.defined.Value = other.defined.Value;
	}

	public IEnumerator<long?> GetEnumerator()
	{
		yield return this.defined.Value ? new long?(this.uid.Value) : ((long?)null);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return this.GetEnumerator();
	}

	public void Add(long? value)
	{
		if (!value.HasValue)
		{
			this.defined.Value = false;
			this.uid.Value = 0L;
		}
		else
		{
			this.defined.Value = true;
			this.uid.Value = value.Value;
		}
	}
}
