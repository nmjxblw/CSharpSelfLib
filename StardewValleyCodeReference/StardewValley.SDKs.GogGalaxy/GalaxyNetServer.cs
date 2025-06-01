using System;
using System.Collections.Generic;
using System.IO;
using Galaxy.Api;
using StardewValley.Network;
using StardewValley.SDKs.GogGalaxy.Listeners;

namespace StardewValley.SDKs.GogGalaxy;

public class GalaxyNetServer : HookableServer
{
	private GalaxyID host;

	protected GalaxySocket server;

	private GalaxySpecificUserDataListener galaxySpecificUserDataListener;

	protected Bimap<long, ulong> peers = new Bimap<long, ulong>();

	/// <summary>A mapping of raw GalaxyIDs to custom display names.</summary>
	protected Dictionary<ulong, string> displayNames = new Dictionary<ulong, string>();

	public override int connectionsCount
	{
		get
		{
			if (this.server == null)
			{
				return 0;
			}
			return this.server.ConnectionCount;
		}
	}

	public GalaxyNetServer(IGameServer gameServer)
		: base(gameServer)
	{
	}

	public override string getUserId(long farmerId)
	{
		if (!this.peers.ContainsLeft(farmerId))
		{
			return null;
		}
		return this.peers[farmerId].ToString();
	}

	public override bool hasUserId(string userId)
	{
		foreach (ulong rightValue in this.peers.RightValues)
		{
			if (rightValue.ToString().Equals(userId))
			{
				return true;
			}
		}
		return false;
	}

	public override bool isConnectionActive(string connection_id)
	{
		foreach (GalaxyID connection in this.server.Connections)
		{
			if (this.getConnectionId(connection) == connection_id && connection.IsValid())
			{
				return true;
			}
		}
		return false;
	}

	public override string getUserName(long farmerId)
	{
		if (!this.peers.ContainsLeft(farmerId))
		{
			return null;
		}
		ulong peerId = this.peers[farmerId];
		if (this.displayNames.TryGetValue(peerId, out var displayName))
		{
			return displayName;
		}
		GalaxyID user = new GalaxyID(peerId);
		return GalaxyInstance.Friends().GetFriendPersonaName(user);
	}

	public override float getPingToClient(long farmerId)
	{
		if (!this.peers.ContainsLeft(farmerId))
		{
			return -1f;
		}
		GalaxyID user = new GalaxyID(this.peers[farmerId]);
		return this.server.GetPingWith(user);
	}

	public override void setPrivacy(ServerPrivacy privacy)
	{
		this.server.SetPrivacy(privacy);
	}

	public override bool connected()
	{
		return this.server.Connected;
	}

	public override string getInviteCode()
	{
		return this.server.GetInviteCode();
	}

	public override void initialize()
	{
		Game1.log.Verbose("Starting Galaxy server");
		this.host = GalaxyInstance.User().GetGalaxyID();
		this.galaxySpecificUserDataListener = new GalaxySpecificUserDataListener(onProfileDataReady);
		this.server = new GalaxySocket(Multiplayer.protocolVersion);
		this.server.CreateLobby(Game1.options.serverPrivacy, (uint)(Game1.multiplayer.playerLimit * 2));
	}

	public override void stopServer()
	{
		Game1.log.Verbose("Stopping Galaxy server");
		this.server.Close();
		this.galaxySpecificUserDataListener?.Dispose();
		this.galaxySpecificUserDataListener = null;
	}

	private void onProfileDataReady(GalaxyID userID)
	{
		if (!(userID == this.host) && !this.displayNames.ContainsKey(userID.ToUint64()))
		{
			string displayName = null;
			try
			{
				displayName = GalaxyInstance.User().GetUserData("StardewDisplayName", userID);
			}
			catch (Exception)
			{
			}
			if (!string.IsNullOrEmpty(displayName))
			{
				this.displayNames[userID.ToUint64()] = displayName;
				Game1.log.Verbose($"{userID} ({displayName}) connected");
			}
			else
			{
				Game1.log.Verbose(userID?.ToString() + " connected");
			}
			this.onConnect(this.getConnectionId(userID));
			base.gameServer.sendAvailableFarmhands(this.createUserID(userID), this.getConnectionId(userID), delegate(OutgoingMessage msg)
			{
				this.sendMessage(userID, msg);
			});
		}
	}

	public override void receiveMessages()
	{
		if (this.server == null)
		{
			return;
		}
		this.server.Receive(onReceiveConnection, onReceiveMessage, onReceiveDisconnect, onReceiveError);
		this.server.Heartbeat(this.server.LobbyMembers());
		foreach (GalaxyID client in this.server.Connections)
		{
			if (this.server.GetPingWith(client) > 30000)
			{
				this.server.Kick(client);
			}
		}
		base.bandwidthLogger?.Update();
	}

	public override void kick(long disconnectee)
	{
		base.kick(disconnectee);
		if (this.peers.ContainsLeft(disconnectee))
		{
			GalaxyID user = new GalaxyID(this.peers[disconnectee]);
			this.server.Kick(user);
			this.sendMessage(user, new OutgoingMessage(23, Game1.player));
		}
	}

	public string getConnectionId(GalaxyID peer)
	{
		return "GN_" + Convert.ToString(peer.ToUint64());
	}

	private string createUserID(GalaxyID peer)
	{
		return Convert.ToString(peer.ToUint64());
	}

	protected virtual void onReceiveConnection(GalaxyID peer)
	{
		if (!base.gameServer.isUserBanned(peer.ToString()))
		{
			if (GalaxyInstance.User().IsUserDataAvailable(peer))
			{
				this.onProfileDataReady(peer);
			}
			else
			{
				GalaxyInstance.User().RequestUserData(peer);
			}
		}
	}

	protected virtual void onReceiveMessage(GalaxyID peer, Stream messageStream)
	{
		base.bandwidthLogger?.RecordBytesDown(messageStream.Length);
		IncomingMessage message = new IncomingMessage();
		try
		{
			using BinaryReader reader = new BinaryReader(messageStream);
			message.Read(reader);
			base.OnProcessingMessage(message, delegate(OutgoingMessage outgoing)
			{
				this.sendMessage(peer, outgoing);
			}, delegate
			{
				if (this.peers.ContainsLeft(message.FarmerID) && this.peers[message.FarmerID] == peer.ToUint64())
				{
					base.gameServer.processIncomingMessage(message);
				}
				else if (message.MessageType == 2)
				{
					NetFarmerRoot farmer = Game1.multiplayer.readFarmer(message.Reader);
					GalaxyID capturedPeer = new GalaxyID(peer.ToUint64());
					base.gameServer.checkFarmhandRequest(this.createUserID(peer), this.getConnectionId(peer), farmer, delegate(OutgoingMessage msg)
					{
						this.sendMessage(capturedPeer, msg);
					}, delegate
					{
						this.peers[farmer.Value.UniqueMultiplayerID] = capturedPeer.ToUint64();
					});
				}
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

	public virtual void onReceiveDisconnect(GalaxyID peer)
	{
		Game1.log.Verbose(peer?.ToString() + " disconnected");
		this.onDisconnect(this.getConnectionId(peer));
		if (this.peers.ContainsRight(peer.ToUint64()))
		{
			this.playerDisconnected(this.peers[peer.ToUint64()]);
		}
		this.displayNames.Remove(peer.ToUint64());
	}

	protected virtual void onReceiveError(string messageKey)
	{
		Game1.log.Error("Server error: " + Game1.content.LoadString(messageKey));
	}

	public override void playerDisconnected(long disconnectee)
	{
		base.playerDisconnected(disconnectee);
		this.peers.RemoveLeft(disconnectee);
	}

	public override void sendMessage(long peerId, OutgoingMessage message)
	{
		if (this.peers.ContainsLeft(peerId))
		{
			this.sendMessage(new GalaxyID(this.peers[peerId]), message);
		}
	}

	protected virtual void sendMessage(GalaxyID peer, OutgoingMessage message)
	{
		if (base.bandwidthLogger != null)
		{
			using (MemoryStream stream = new MemoryStream())
			{
				using BinaryWriter writer = new BinaryWriter(stream);
				message.Write(writer);
				stream.Seek(0L, SeekOrigin.Begin);
				byte[] bytes = stream.ToArray();
				this.server.Send(peer, bytes);
				base.bandwidthLogger.RecordBytesUp(bytes.Length);
				return;
			}
		}
		this.server.Send(peer, message);
	}

	public override void setLobbyData(string key, string value)
	{
		this.server.SetLobbyData(key, value);
	}
}
