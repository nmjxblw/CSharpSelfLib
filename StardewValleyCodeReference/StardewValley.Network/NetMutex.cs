using System;
using System.Linq;
using System.Xml.Serialization;
using Netcode;

namespace StardewValley.Network;

public class NetMutex : INetObject<NetFields>
{
	public const long NoOwner = -1L;

	private long prevOwner = -1L;

	private readonly NetLong owner = new NetLong(-1L)
	{
		InterpolationWait = false
	};

	private readonly NetEvent1Field<long, NetLong> lockRequest = new NetEvent1Field<long, NetLong>
	{
		InterpolationWait = false
	};

	private Action onLockAcquired;

	private Action onLockFailed;

	[XmlIgnore]
	public NetFields NetFields { get; } = new NetFields("NetMutex");

	public NetMutex()
	{
		this.NetFields.SetOwner(this).AddField(this.owner, "owner").AddField(this.lockRequest, "lockRequest");
		this.lockRequest.onEvent += delegate(long playerId)
		{
			if (Game1.IsMasterGame && (this.owner.Value == -1 || this.owner.Value == playerId))
			{
				this.owner.Value = playerId;
				this.owner.MarkDirty();
			}
		};
	}

	public void RequestLock(Action acquired = null, Action failed = null)
	{
		if (this.owner.Value == Game1.player.UniqueMultiplayerID)
		{
			acquired?.Invoke();
			return;
		}
		if (this.owner.Value != -1)
		{
			failed?.Invoke();
			return;
		}
		this.lockRequest.Fire(Game1.player.UniqueMultiplayerID);
		this.onLockAcquired = acquired;
		this.onLockFailed = failed;
	}

	public void ReleaseLock()
	{
		this.owner.Value = -1L;
		this.onLockFailed = null;
		this.onLockAcquired = null;
	}

	public bool IsLocked()
	{
		return this.owner.Value != -1;
	}

	public bool IsLockHeld()
	{
		return this.owner.Value == Game1.player.UniqueMultiplayerID;
	}

	public void Update(GameLocation location)
	{
		this.Update(location.farmers);
	}

	public void Update(FarmerCollection farmers)
	{
		this.lockRequest.Poll();
		if (this.owner.Value != this.prevOwner)
		{
			if (this.owner.Value == Game1.player.UniqueMultiplayerID && this.onLockAcquired != null)
			{
				this.onLockAcquired();
			}
			if (this.owner.Value != Game1.player.UniqueMultiplayerID && this.onLockFailed != null)
			{
				this.onLockFailed();
			}
			this.onLockAcquired = null;
			this.onLockFailed = null;
			this.prevOwner = this.owner.Value;
		}
		if (Game1.IsMasterGame && this.owner.Value != -1 && farmers.FirstOrDefault((Farmer f) => f.UniqueMultiplayerID == this.owner.Value && f.locationBeforeForcedEvent.Value == null) == null)
		{
			this.ReleaseLock();
		}
	}
}
