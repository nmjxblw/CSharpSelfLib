using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Netcode;

namespace StardewValley.Network;

public class NetMutexQueue<T> : INetObject<NetFields>
{
	private readonly NetLongDictionary<bool, NetBool> requests = new NetLongDictionary<bool, NetBool>
	{
		InterpolationWait = false
	};

	private readonly NetLong currentOwner = new NetLong
	{
		InterpolationWait = false
	};

	private readonly List<T> localJobs = new List<T>();

	[XmlIgnore]
	public Action<T> Processor = delegate
	{
	};

	[XmlIgnore]
	public NetFields NetFields { get; } = new NetFields("NetMutexQueue");

	public NetMutexQueue()
	{
		this.NetFields.SetOwner(this).AddField(this.requests, "requests").AddField(this.currentOwner, "currentOwner");
	}

	public void Add(T job)
	{
		this.localJobs.Add(job);
	}

	public bool Contains(T job)
	{
		return this.localJobs.Contains(job);
	}

	public void Clear()
	{
		this.localJobs.Clear();
	}

	public void Update(GameLocation location)
	{
		FarmerCollection farmers = location.farmers;
		if (farmers.Contains(Game1.player) && this.localJobs.Count > 0)
		{
			this.requests[Game1.player.UniqueMultiplayerID] = true;
		}
		else
		{
			this.requests.Remove(Game1.player.UniqueMultiplayerID);
		}
		if (Game1.IsMasterGame)
		{
			this.requests.RemoveWhere((KeyValuePair<long, bool> pair) => farmers.FirstOrDefault((Farmer f) => f.UniqueMultiplayerID == pair.Key) == null);
			if (!this.requests.ContainsKey(this.currentOwner.Value))
			{
				this.currentOwner.Value = -1L;
			}
		}
		if (this.currentOwner.Value == Game1.player.UniqueMultiplayerID)
		{
			foreach (T job in this.localJobs)
			{
				this.Processor(job);
			}
			this.localJobs.Clear();
			this.requests.Remove(Game1.player.UniqueMultiplayerID);
			this.currentOwner.Value = -1L;
		}
		if (Game1.IsMasterGame && this.currentOwner.Value == -1 && Utility.TryGetRandom(this.requests, out var ownerId, out var _))
		{
			this.currentOwner.Value = ownerId;
		}
	}
}
