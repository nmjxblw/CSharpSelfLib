using System.Collections.Generic;

namespace StardewValley.Network.NetReady.Internal;

/// <summary>A cancelable ready-check for the host player.</summary>
internal sealed class ServerReadyCheck : BaseReadyCheck
{
	/// <summary>The ready states for all farmers required by this ready check.</summary>
	private readonly Dictionary<long, ReadyState> ReadyStates = new Dictionary<long, ReadyState>();

	/// <summary>Whether we're currently attempting to lock all clients.</summary>
	private bool Locking;

	/// <summary>All farmers that should be included in this check.</summary>
	private readonly HashSet<long> RequiredFarmers = new HashSet<long>();

	/// <summary>Whether all farmers (including those that recently joined) should be included in this check.</summary>
	private bool IncludesAll => this.RequiredFarmers.Count == 0;

	/// <inheritdoc />
	public ServerReadyCheck(string id)
		: base(id)
	{
	}

	/// <inheritdoc />
	public override void SetRequiredFarmers(List<long> farmerIds)
	{
		this.RequireFarmers(farmerIds);
	}

	/// <inheritdoc />
	public override bool SetLocalReady(bool ready)
	{
		if (!base.SetLocalReady(ready))
		{
			return false;
		}
		if (!this.IsFarmerRequired(Game1.player.UniqueMultiplayerID))
		{
			base.State = ReadyState.NotReady;
			return false;
		}
		this.ReadyStates[Game1.player.UniqueMultiplayerID] = base.State;
		return true;
	}

	/// <inheritdoc />
	public override void Update()
	{
		if (base.IsReady)
		{
			return;
		}
		int ready = 0;
		int required = 0;
		int locked = 0;
		bool includeHost = this.IsFarmerRequired(Game1.player.UniqueMultiplayerID);
		foreach (Farmer farmer in Game1.getOnlineFarmers())
		{
			if (this.IsFarmerRequired(farmer.UniqueMultiplayerID) && !Game1.multiplayer.isDisconnecting(farmer))
			{
				if (!this.ReadyStates.TryGetValue(farmer.UniqueMultiplayerID, out var remoteState))
				{
					remoteState = ReadyState.NotReady;
					this.ReadyStates[farmer.UniqueMultiplayerID] = remoteState;
				}
				required++;
				switch (remoteState)
				{
				case ReadyState.Ready:
					ready++;
					break;
				case ReadyState.Locked:
					ready++;
					locked++;
					break;
				}
			}
		}
		if (ready != base.NumberReady || required != base.NumberRequired)
		{
			if (includeHost && Game1.IsDedicatedHost)
			{
				this.SendMessage(ReadyCheckMessageType.UpdateAmounts, ready - ((base.State == ReadyState.Ready) ? 1 : 0), required - 1);
			}
			else
			{
				this.SendMessage(ReadyCheckMessageType.UpdateAmounts, ready, required);
			}
			if (ready == required)
			{
				if (!this.Locking)
				{
					base.ActiveLockId++;
					this.Locking = true;
					if (includeHost && base.State == ReadyState.Ready)
					{
						Dictionary<long, ReadyState> readyStates = this.ReadyStates;
						long uniqueMultiplayerID = Game1.player.UniqueMultiplayerID;
						ReadyState value = (base.State = ReadyState.Locked);
						readyStates[uniqueMultiplayerID] = value;
						locked = 1;
					}
					this.SendMessage(ReadyCheckMessageType.Lock, base.ActiveLockId);
				}
			}
			else if (this.Locking)
			{
				this.Locking = false;
				if (base.State == ReadyState.Locked)
				{
					base.State = ReadyState.Ready;
				}
				foreach (long farmerId in this.ReadyStates.Keys)
				{
					if (this.ReadyStates[farmerId] == ReadyState.Locked && this.IsFarmerRequired(farmerId))
					{
						this.ReadyStates[farmerId] = ReadyState.Ready;
					}
				}
				locked = 0;
				this.SendMessage(ReadyCheckMessageType.Release, base.ActiveLockId);
			}
		}
		if (this.Locking && locked == required)
		{
			base.IsReady = true;
			this.SendMessage(ReadyCheckMessageType.Finish);
		}
		base.NumberReady = ready;
		base.NumberRequired = required;
	}

	/// <inheritdoc />
	public override void ProcessMessage(ReadyCheckMessageType messageType, IncomingMessage message)
	{
		switch (messageType)
		{
		case ReadyCheckMessageType.Ready:
			this.ProcessReady(message);
			return;
		case ReadyCheckMessageType.Cancel:
			this.ProcessCancel(message);
			return;
		case ReadyCheckMessageType.AcceptLock:
			this.ProcessAcceptLock(message);
			return;
		case ReadyCheckMessageType.RejectLock:
			this.ProcessRejectLock(message);
			return;
		case ReadyCheckMessageType.RequireFarmers:
			this.ProcessRequireFarmers(message);
			return;
		}
		Game1.log.Warn($"{"ServerReadyCheck"} '{base.Id}' received invalid message type '{messageType}'.");
	}

	/// <inheritdoc />
	protected override void SendMessage(ReadyCheckMessageType messageType, params object[] data)
	{
		if (Game1.server == null)
		{
			return;
		}
		foreach (Farmer farmer in Game1.otherFarmers.Values)
		{
			Game1.server.sendMessage(farmer.UniqueMultiplayerID, base.CreateSyncMessage(messageType, data));
		}
	}

	/// <summary>Handle a request to mark a farmer's state as ready.</summary>
	/// <param name="message">The incoming <see cref="F:StardewValley.Network.NetReady.Internal.ReadyCheckMessageType.Ready" /> message.</param>
	private void ProcessReady(IncomingMessage message)
	{
		if (!this.Locking)
		{
			this.ReadyStates[message.FarmerID] = ReadyState.Ready;
		}
	}

	/// <summary>Handle a request to mark a farmer as non-ready.</summary>
	/// <param name="message">The incoming <see cref="F:StardewValley.Network.NetReady.Internal.ReadyCheckMessageType.Cancel" /> message.</param>
	private void ProcessCancel(IncomingMessage message)
	{
		if (!this.Locking)
		{
			this.ReadyStates[message.FarmerID] = ReadyState.NotReady;
		}
	}

	/// <summary>Handle a request to mark a farmer as locked.</summary>
	/// <param name="message">The incoming <see cref="F:StardewValley.Network.NetReady.Internal.ReadyCheckMessageType.AcceptLock" /> message.</param>
	private void ProcessAcceptLock(IncomingMessage message)
	{
		if (message.Reader.ReadInt32() == base.ActiveLockId)
		{
			this.ReadyStates[message.FarmerID] = ReadyState.Locked;
		}
	}

	/// <summary>Handle a request to mark a farmer as not ready to lock.</summary>
	/// <param name="message">The incoming <see cref="F:StardewValley.Network.NetReady.Internal.ReadyCheckMessageType.RejectLock" /> message.</param>
	private void ProcessRejectLock(IncomingMessage message)
	{
		if (message.Reader.ReadInt32() == base.ActiveLockId)
		{
			this.ReadyStates[message.FarmerID] = ReadyState.NotReady;
		}
	}

	/// <summary>Handle a request to set the required farmers for this check.</summary>
	/// <param name="message">The incoming <see cref="F:StardewValley.Network.NetReady.Internal.ReadyCheckMessageType.RequireFarmers" /> message.</param>
	private void ProcessRequireFarmers(IncomingMessage message)
	{
		int count = message.Reader.ReadInt32();
		HashSet<long> farmerIds = new HashSet<long>();
		for (int i = 0; i < count; i++)
		{
			farmerIds.Add(message.Reader.ReadInt64());
		}
		this.RequireFarmers(farmerIds);
	}

	/// <summary>Update the required farmers in <see cref="F:StardewValley.Network.NetReady.Internal.ServerReadyCheck.ReadyStates" /> to be the set of <paramref name="farmerIds" />.</summary>
	/// <param name="farmerIds">The list of farmer multiplayer IDs that should be required for this check.</param>
	private void RequireFarmers(ICollection<long> farmerIds)
	{
		this.RequiredFarmers.Clear();
		if (farmerIds == null)
		{
			return;
		}
		foreach (long farmerId in farmerIds)
		{
			this.RequiredFarmers.Add(farmerId);
		}
	}

	/// <summary>Checks if a farmer is required for this ready check to pass.</summary>
	/// <param name="uid">The unique multiplayer ID of the farmer to check.</param>
	private bool IsFarmerRequired(long uid)
	{
		if (!this.IncludesAll)
		{
			return this.RequiredFarmers.Contains(uid);
		}
		return true;
	}
}
