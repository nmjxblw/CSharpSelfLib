using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Lidgren.Network;

namespace StardewValley.Network;

public class LidgrenServer : HookableServer
{
	public const int defaultPort = 24642;

	public NetServer server;

	private HashSet<NetConnection> introductionsSent = new HashSet<NetConnection>();

	protected Bimap<long, NetConnection> peers = new Bimap<long, NetConnection>();

	public override int connectionsCount
	{
		get
		{
			if (this.server == null)
			{
				return 0;
			}
			return this.server.ConnectionsCount;
		}
	}

	public LidgrenServer(IGameServer gameServer)
		: base(gameServer)
	{
	}

	public override bool isConnectionActive(string connectionID)
	{
		foreach (NetConnection connection in this.server.Connections)
		{
			if (this.getConnectionId(connection) == connectionID && connection.Status == NetConnectionStatus.Connected)
			{
				return true;
			}
		}
		return false;
	}

	public override string getUserId(long farmerId)
	{
		if (!this.peers.ContainsLeft(farmerId))
		{
			return null;
		}
		return this.peers[farmerId].RemoteEndPoint.Address.ToString();
	}

	public override bool hasUserId(string userId)
	{
		foreach (NetConnection rightValue in this.peers.RightValues)
		{
			if (rightValue.RemoteEndPoint.Address.ToString().Equals(userId))
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
		return this.peers[farmerId].RemoteEndPoint.Address.ToString();
	}

	public override float getPingToClient(long farmerId)
	{
		if (!this.peers.ContainsLeft(farmerId))
		{
			return -1f;
		}
		return this.peers[farmerId].AverageRoundtripTime / 2f * 1000f;
	}

	public override void setPrivacy(ServerPrivacy privacy)
	{
	}

	public override bool canAcceptIPConnections()
	{
		return true;
	}

	public override bool connected()
	{
		return this.server != null;
	}

	public override void initialize()
	{
		Game1.log.Verbose("Starting LAN server");
		NetPeerConfiguration config = new NetPeerConfiguration("StardewValley");
		config.EnableMessageType(NetIncomingMessageType.DiscoveryRequest);
		config.EnableMessageType(NetIncomingMessageType.ConnectionApproval);
		config.Port = 24642;
		config.ConnectionTimeout = 30f;
		config.PingInterval = 5f;
		config.MaximumConnections = Game1.multiplayer.playerLimit * 2;
		config.MaximumTransmissionUnit = 1200;
		this.server = new NetServer(config);
		this.server.Start();
	}

	public override void stopServer()
	{
		Game1.log.Verbose("Stopping LAN server");
		this.server.Shutdown("Server shutting down...");
		this.server.FlushSendQueue();
		this.introductionsSent.Clear();
		this.peers.Clear();
	}

	public static bool IsLocal(string host_name_or_address)
	{
		if (string.IsNullOrEmpty(host_name_or_address))
		{
			return false;
		}
		try
		{
			IPAddress[] hostAddresses = Dns.GetHostAddresses(host_name_or_address);
			IPAddress[] local_ips = Dns.GetHostAddresses(Dns.GetHostName());
			return hostAddresses.Any((IPAddress host_ip) => IPAddress.IsLoopback(host_ip) || local_ips.Contains(host_ip));
		}
		catch
		{
			return false;
		}
	}

	public override void receiveMessages()
	{
		NetIncomingMessage inc;
		while ((inc = this.server.ReadMessage()) != null)
		{
			base.bandwidthLogger?.RecordBytesDown(inc.LengthBytes);
			switch (inc.MessageType)
			{
			case NetIncomingMessageType.DiscoveryRequest:
				if ((Game1.options.ipConnectionsEnabled || base.gameServer.IsLocalMultiplayerInitiatedServer()) && (!base.gameServer.IsLocalMultiplayerInitiatedServer() || LidgrenServer.IsLocal(inc.SenderEndPoint.Address.ToString())) && !base.gameServer.isUserBanned(inc.SenderEndPoint.Address.ToString()))
				{
					this.sendVersionInfo(inc);
				}
				break;
			case NetIncomingMessageType.ConnectionApproval:
				if (Game1.options.ipConnectionsEnabled || base.gameServer.IsLocalMultiplayerInitiatedServer())
				{
					inc.SenderConnection.Approve();
				}
				else
				{
					inc.SenderConnection.Deny();
				}
				break;
			case NetIncomingMessageType.Data:
				this.parseDataMessageFromClient(inc);
				break;
			case NetIncomingMessageType.DebugMessage:
			case NetIncomingMessageType.WarningMessage:
			case NetIncomingMessageType.ErrorMessage:
			{
				string message = inc.ReadString();
				Game1.log.Verbose(inc.MessageType.ToString() + ": " + message);
				Game1.debugOutput = message;
				break;
			}
			case NetIncomingMessageType.StatusChanged:
				this.statusChanged(inc);
				break;
			default:
				Game1.debugOutput = inc.ToString();
				break;
			}
			this.server.Recycle(inc);
		}
		foreach (NetConnection conn in this.server.Connections)
		{
			if (conn.Status == NetConnectionStatus.Connected && !this.introductionsSent.Contains(conn))
			{
				if (!base.gameServer.whenGameAvailable(delegate
				{
					base.gameServer.sendAvailableFarmhands("", this.getConnectionId(conn), delegate(OutgoingMessage msg)
					{
						this.sendMessage(conn, msg);
					});
				}, () => Game1.gameMode != 6))
				{
					Game1.log.Verbose("Postponing introduction message");
					this.sendMessage(conn, new OutgoingMessage(11, Game1.player, "Strings\\UI:Client_WaitForHostLoad"));
				}
				this.introductionsSent.Add(conn);
			}
		}
		base.bandwidthLogger?.Update();
	}

	private void sendVersionInfo(NetIncomingMessage message)
	{
		NetOutgoingMessage response = this.server.CreateMessage();
		response.Write(Multiplayer.protocolVersion);
		response.Write("StardewValley");
		this.server.SendDiscoveryResponse(response, message.SenderEndPoint);
		base.bandwidthLogger?.RecordBytesUp(response.LengthBytes);
	}

	private void statusChanged(NetIncomingMessage message)
	{
		switch ((NetConnectionStatus)message.ReadByte())
		{
		case NetConnectionStatus.Connected:
			this.onConnect(this.getConnectionId(message.SenderConnection));
			break;
		case NetConnectionStatus.Disconnecting:
		case NetConnectionStatus.Disconnected:
			this.onDisconnect(this.getConnectionId(message.SenderConnection));
			if (this.peers.ContainsRight(message.SenderConnection))
			{
				this.playerDisconnected(this.peers[message.SenderConnection]);
			}
			break;
		}
	}

	public override void kick(long disconnectee)
	{
		base.kick(disconnectee);
		if (this.peers.ContainsLeft(disconnectee))
		{
			this.peers[disconnectee].Disconnect(Multiplayer.kicked);
			this.server.FlushSendQueue();
			this.playerDisconnected(disconnectee);
		}
	}

	public override void playerDisconnected(long disconnectee)
	{
		base.playerDisconnected(disconnectee);
		this.introductionsSent.Remove(this.peers[disconnectee]);
		this.peers.RemoveLeft(disconnectee);
	}

	protected virtual void parseDataMessageFromClient(NetIncomingMessage dataMsg)
	{
		NetConnection peer = dataMsg.SenderConnection;
		IncomingMessage message = new IncomingMessage();
		try
		{
			using NetBufferReadStream stream = new NetBufferReadStream(dataMsg);
			while (dataMsg.LengthBits - dataMsg.Position >= 8)
			{
				LidgrenMessageUtils.ReadStreamToMessage(stream, message);
				base.OnProcessingMessage(message, delegate(OutgoingMessage outgoing)
				{
					this.sendMessage(peer, outgoing);
				}, delegate
				{
					if (this.peers.ContainsLeft(message.FarmerID) && this.peers[message.FarmerID] == peer)
					{
						base.gameServer.processIncomingMessage(message);
					}
					else if (message.MessageType == 2)
					{
						NetFarmerRoot farmer = Game1.multiplayer.readFarmer(message.Reader);
						base.gameServer.checkFarmhandRequest("", this.getConnectionId(dataMsg.SenderConnection), farmer, delegate(OutgoingMessage msg)
						{
							this.sendMessage(peer, msg);
						}, delegate
						{
							this.peers[farmer.Value.UniqueMultiplayerID] = peer;
						});
					}
				});
			}
		}
		finally
		{
			if (message != null)
			{
				((IDisposable)message).Dispose();
			}
		}
	}

	public string getConnectionId(NetConnection connection)
	{
		return "L_" + connection.RemoteUniqueIdentifier;
	}

	public override void sendMessage(long peerId, OutgoingMessage message)
	{
		if (this.peers.ContainsLeft(peerId))
		{
			this.sendMessage(this.peers[peerId], message);
		}
	}

	protected virtual void sendMessage(NetConnection connection, OutgoingMessage message)
	{
		NetOutgoingMessage msg = this.server.CreateMessage();
		LidgrenMessageUtils.WriteMessage(message, msg);
		this.server.SendMessage(msg, connection, NetDeliveryMethod.ReliableOrdered);
		base.bandwidthLogger?.RecordBytesUp(msg.LengthBytes);
	}

	public override void setLobbyData(string key, string value)
	{
	}
}
