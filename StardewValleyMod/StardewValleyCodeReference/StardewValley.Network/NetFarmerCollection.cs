using System;
using System.Collections;
using System.Collections.Generic;
using Netcode;

namespace StardewValley.Network;

public class NetFarmerCollection : INetObject<NetFields>, ICollection<Farmer>, IEnumerable<Farmer>, IEnumerable
{
	public delegate void FarmerEvent(Farmer f);

	private List<Farmer> farmers = new List<Farmer>();

	private NetLongDictionary<bool, NetBool> uids = new NetLongDictionary<bool, NetBool>();

	public NetFields NetFields { get; } = new NetFields("NetFarmerCollection");

	public int Count => this.farmers.Count;

	public bool IsReadOnly => false;

	public event FarmerEvent FarmerAdded;

	public event FarmerEvent FarmerRemoved;

	public NetFarmerCollection()
	{
		this.NetFields.SetOwner(this).AddField(this.uids, "uids");
		this.uids.OnValueAdded += delegate(long uid, bool _)
		{
			Farmer farmer = this.getFarmer(uid);
			if (farmer != null && !this.farmers.Contains(farmer))
			{
				this.farmers.Add(farmer);
				this.FarmerAdded?.Invoke(farmer);
			}
		};
		this.uids.OnValueRemoved += delegate(long uid, bool _)
		{
			Farmer farmer = this.getFarmer(uid);
			if (farmer != null)
			{
				this.farmers.Remove(farmer);
				this.FarmerRemoved?.Invoke(farmer);
			}
		};
	}

	private static bool playerIsOnline(long uid)
	{
		if (Game1.player.UniqueMultiplayerID != uid && (!(Game1.serverHost != null) || Game1.serverHost.Value.UniqueMultiplayerID != uid))
		{
			if (Game1.otherFarmers.ContainsKey(uid))
			{
				return !Game1.multiplayer.isDisconnecting(uid);
			}
			return false;
		}
		return true;
	}

	public bool RetainOnlinePlayers()
	{
		int origCount = this.uids.Length;
		if (origCount == 0)
		{
			return false;
		}
		this.uids.RemoveWhere((KeyValuePair<long, bool> pair) => !NetFarmerCollection.playerIsOnline(pair.Key));
		this.farmers.Clear();
		foreach (long uid in this.uids.Keys)
		{
			Farmer f = this.getFarmer(uid);
			if (f != null)
			{
				this.farmers.Add(f);
			}
		}
		return this.uids.Length < origCount;
	}

	private Farmer getFarmer(long uid)
	{
		foreach (Farmer farmer in Game1.getOnlineFarmers())
		{
			if (farmer.UniqueMultiplayerID == uid)
			{
				return farmer;
			}
		}
		return null;
	}

	public void Add(Farmer item)
	{
		this.farmers.Add(item);
		this.uids.TryAdd(item.UniqueMultiplayerID, value: true);
	}

	public void Clear()
	{
		this.farmers.Clear();
		this.uids.Clear();
	}

	public bool Contains(Farmer item)
	{
		return this.farmers.Contains(item);
	}

	public void CopyTo(Farmer[] array, int arrayIndex)
	{
		if (array == null)
		{
			throw new ArgumentNullException();
		}
		if (arrayIndex < 0)
		{
			throw new ArgumentOutOfRangeException();
		}
		if (this.Count - arrayIndex > array.Length)
		{
			throw new ArgumentException();
		}
		foreach (Farmer value in this)
		{
			array[arrayIndex++] = value;
		}
	}

	public bool Remove(Farmer item)
	{
		this.uids.Remove(item.UniqueMultiplayerID);
		return this.farmers.Remove(item);
	}

	public IEnumerator<Farmer> GetEnumerator()
	{
		return this.farmers.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return this.GetEnumerator();
	}
}
