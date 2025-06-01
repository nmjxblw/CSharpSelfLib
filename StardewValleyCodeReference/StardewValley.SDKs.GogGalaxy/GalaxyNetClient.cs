using System;
using System.IO;
using System.Linq;
using Galaxy.Api;
using StardewValley.Network;
using StardewValley.SDKs.GogGalaxy.Listeners;

namespace StardewValley.SDKs.GogGalaxy;

public class GalaxyNetClient : HookableClient
{
	public GalaxyID lobbyId;

	protected GalaxySocket client;

	private GalaxyID serverId;

	/// <summary>The custom display name for the host player, or null if no custom name was found.</summary>
	private string hostDisplayName;

	private GalaxySpecificUserDataListener galaxySpecificUserDataListener;

	private float lastPingMs;

	public GalaxyNetClient(GalaxyID lobbyId)
	{
		this.lobbyId = lobbyId;
		this.hostDisplayName = null;
	}

	~GalaxyNetClient()
	{
		this.galaxySpecificUserDataListener?.Dispose();
		this.galaxySpecificUserDataListener = null;
	}

	private void onProfileDataReady(GalaxyID userID)
	{
		if (!(userID != this.serverId))
		{
			this.hostDisplayName = null;
			try
			{
				this.hostDisplayName = GalaxyInstance.User().GetUserData("StardewDisplayName", userID);
			}
			catch (Exception)
			{
			}
			this.galaxySpecificUserDataListener?.Dispose();
			this.galaxySpecificUserDataListener = null;
		}
	}

	public override string getUserID()
	{
		return Convert.ToString(GalaxyInstance.User().GetGalaxyID().ToUint64());
	}

	protected override string getHostUserName()
	{
		if (!string.IsNullOrEmpty(this.hostDisplayName))
		{
			return this.hostDisplayName;
		}
		return GalaxyInstance.Friends().GetFriendPersonaName(this.serverId);
	}

	public override float GetPingToHost()
	{
		return this.lastPingMs;
	}

	protected override void connectImpl()
	{
		this.client = new GalaxySocket(Multiplayer.protocolVersion);
		GalaxyInstance.User().GetGalaxyID();
		this.client.JoinLobby(this.lobbyId, onReceiveError);
	}

	public override void disconnect(bool neatly = true)
	{
		if (this.client != null)
		{
			Game1.log.Verbose("Disconnecting from server " + this.lobbyId);
			this.client.Close();
			this.client = null;
			base.connectionMessage = null;
		}
	}

	protected override void receiveMessagesImpl()
	{
		if (this.client == null || !this.client.Connected)
		{
			return;
		}
		if (this.client.Connected && this.serverId == null)
		{
			Game1.log.Verbose("Connected to server " + this.lobbyId);
			this.serverId = this.client.LobbyOwner;
			if (GalaxyInstance.User().IsUserDataAvailable(this.serverId))
			{
				this.onProfileDataReady(this.serverId);
			}
			else
			{
				this.hostDisplayName = GalaxyNetHelper.TryGetHostSteamDisplayName(this.lobbyId);
				this.galaxySpecificUserDataListener = new GalaxySpecificUserDataListener(onProfileDataReady);
				GalaxyInstance.User().RequestUserData(this.serverId);
			}
		}
		this.client.Receive(onReceiveConnection, onReceiveMessage, onReceiveDisconnect, onReceiveError);
		if (this.client != null)
		{
			this.client.Heartbeat(Enumerable.Repeat(this.serverId, 1));
			this.lastPingMs = this.client.GetPingWith(this.serverId);
			if (this.lastPingMs > 30000f)
			{
				base.timedOut = true;
				base.pendingDisconnect = Multiplayer.DisconnectType.GalaxyTimeout;
				this.disconnect();
			}
		}
	}

	protected virtual void onReceiveConnection(GalaxyID peer)
	{
	}

	protected virtual void onReceiveMessage(GalaxyID peer, Stream messageStream)
	{
		if (peer != this.serverId)
		{
			return;
		}
		base.bandwidthLogger?.RecordBytesDown(messageStream.Length);
		IncomingMessage message = new IncomingMessage();
		try
		{
			using BinaryReader reader = new BinaryReader(messageStream);
			message.Read(reader);
			base.OnProcessingMessage(message, sendMessageImpl, delegate
			{
				this.processIncomingMessage(message);
			});
		}
		finally
		{
			if (message != null)
			{
				((IDisposable)message).Dispose();
			}
		}
	}

	protected virtual void onReceiveDisconnect(GalaxyID peer)
	{
		if (peer != this.serverId)
		{
			Game1.multiplayer.playerDisconnected((long)peer.ToUint64());
			return;
		}
		base.timedOut = true;
		base.pendingDisconnect = Multiplayer.DisconnectType.HostLeft;
	}

	protected virtual void onReceiveError(string message)
	{
		base.connectionMessage = message;
	}

	protected virtual void sendMessageImpl(OutgoingMessage message)
	{
		if (this.client == null || !this.client.Connected || this.serverId == null)
		{
			return;
		}
		if (base.bandwidthLogger != null)
		{
			using (MemoryStream stream = new MemoryStream())
			{
				using BinaryWriter writer = new BinaryWriter(stream);
				message.Write(writer);
				stream.Seek(0L, SeekOrigin.Begin);
				byte[] bytes = stream.ToArray();
				this.client.Send(this.serverId, bytes);
				base.bandwidthLogger.RecordBytesUp(bytes.Length);
				return;
			}
		}
		this.client.Send(this.serverId, message);
	}

	public override void sendMessage(OutgoingMessage message)
	{
		base.OnSendingMessage(message, sendMessageImpl, delegate
		{
			this.sendMessageImpl(message);
		});
	}
}
