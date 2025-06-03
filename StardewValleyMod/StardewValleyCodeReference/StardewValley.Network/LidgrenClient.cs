using System;
using Lidgren.Network;

namespace StardewValley.Network;

public class LidgrenClient : HookableClient
{
	public string address;

	public NetClient client;

	private bool serverDiscovered;

	private int maxRetryAttempts;

	private int retryMs = 10000;

	private double lastAttemptMs;

	private int retryAttempts;

	private float lastLatencyMs;

	public LidgrenClient(string address)
	{
		this.address = address;
	}

	public override string getUserID()
	{
		return "";
	}

	public override float GetPingToHost()
	{
		return this.lastLatencyMs / 2f;
	}

	protected override string getHostUserName()
	{
		return this.client?.ServerConnection?.RemoteEndPoint?.Address?.ToString() ?? "";
	}

	protected override void connectImpl()
	{
		NetPeerConfiguration config = new NetPeerConfiguration("StardewValley");
		config.EnableMessageType(NetIncomingMessageType.DiscoveryResponse);
		config.EnableMessageType(NetIncomingMessageType.ConnectionLatencyUpdated);
		config.ConnectionTimeout = 30f;
		config.PingInterval = 5f;
		config.MaximumTransmissionUnit = 1200;
		this.client = new NetClient(config);
		this.client.Start();
		this.attemptConnection();
	}

	private void attemptConnection()
	{
		int port = 24642;
		if (this.address.Contains(':'))
		{
			string[] split = this.address.Split(':');
			this.address = split[0];
			try
			{
				port = Convert.ToInt32(split[1]);
			}
			catch (Exception)
			{
				port = 24642;
			}
		}
		this.client.DiscoverKnownPeer(this.address, port);
		this.lastAttemptMs = DateTime.UtcNow.TimeOfDay.TotalMilliseconds;
	}

	public override void disconnect(bool neatly = true)
	{
		if (this.client == null)
		{
			return;
		}
		if (this.client.ConnectionStatus != NetConnectionStatus.Disconnected && this.client.ConnectionStatus != NetConnectionStatus.Disconnecting)
		{
			if (neatly)
			{
				this.sendMessage(new OutgoingMessage(19, Game1.player));
			}
			this.client.FlushSendQueue();
			this.client.Disconnect("");
			this.client.FlushSendQueue();
		}
		base.connectionMessage = null;
	}

	protected virtual bool validateProtocol(string version)
	{
		return version == Multiplayer.protocolVersion;
	}

	protected override void receiveMessagesImpl()
	{
		if (this.client != null && !this.serverDiscovered && DateTime.UtcNow.TimeOfDay.TotalMilliseconds >= this.lastAttemptMs + (double)this.retryMs && this.retryAttempts < this.maxRetryAttempts)
		{
			this.attemptConnection();
			this.retryAttempts++;
		}
		NetIncomingMessage inc;
		while ((inc = this.client.ReadMessage()) != null)
		{
			switch (inc.MessageType)
			{
			case NetIncomingMessageType.ConnectionLatencyUpdated:
				this.readLatency(inc);
				break;
			case NetIncomingMessageType.DiscoveryResponse:
				if (!this.serverDiscovered)
				{
					Game1.log.Verbose("Found server at " + inc.SenderEndPoint);
					string protocolVersion = inc.ReadString();
					if (this.validateProtocol(protocolVersion))
					{
						base.serverName = inc.ReadString();
						this.receiveHandshake(inc);
						this.serverDiscovered = true;
						break;
					}
					Game1.log.Warn($"Failed to connect. The server's protocol ({protocolVersion}) does not match our own ({Multiplayer.protocolVersion}).");
					base.connectionMessage = Game1.content.LoadString("Strings\\UI:CoopMenu_FailedProtocolVersion");
					this.client.Disconnect("");
				}
				break;
			case NetIncomingMessageType.Data:
				this.parseDataMessageFromServer(inc);
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
			}
		}
	}

	private void readLatency(NetIncomingMessage msg)
	{
		this.lastLatencyMs = msg.ReadFloat() * 1000f;
	}

	private void receiveHandshake(NetIncomingMessage msg)
	{
		this.client.Connect(msg.SenderEndPoint.Address.ToString(), msg.SenderEndPoint.Port);
	}

	private void statusChanged(NetIncomingMessage message)
	{
		NetConnectionStatus status = (NetConnectionStatus)message.ReadByte();
		if (status == NetConnectionStatus.Disconnected || status == NetConnectionStatus.Disconnecting)
		{
			string byeMessage = message.ReadString();
			this.clientRemotelyDisconnected(status, byeMessage);
		}
	}

	private void clientRemotelyDisconnected(NetConnectionStatus status, string message)
	{
		base.timedOut = true;
		if (status == NetConnectionStatus.Disconnected)
		{
			if (message == Multiplayer.kicked)
			{
				base.pendingDisconnect = Multiplayer.DisconnectType.Kicked;
			}
			else
			{
				base.pendingDisconnect = Multiplayer.DisconnectType.LidgrenTimeout;
			}
		}
		else
		{
			base.pendingDisconnect = Multiplayer.DisconnectType.LidgrenDisconnect_Unknown;
		}
	}

	protected virtual void sendMessageImpl(OutgoingMessage message)
	{
		NetOutgoingMessage sendMsg = this.client.CreateMessage();
		LidgrenMessageUtils.WriteMessage(message, sendMsg);
		this.client.SendMessage(sendMsg, NetDeliveryMethod.ReliableOrdered);
		base.bandwidthLogger?.RecordBytesUp(sendMsg.LengthBytes);
	}

	public override void sendMessage(OutgoingMessage message)
	{
		base.OnSendingMessage(message, sendMessageImpl, delegate
		{
			this.sendMessageImpl(message);
		});
	}

	private void parseDataMessageFromServer(NetIncomingMessage dataMsg)
	{
		base.bandwidthLogger?.RecordBytesDown(dataMsg.LengthBytes);
		IncomingMessage message = new IncomingMessage();
		try
		{
			using NetBufferReadStream stream = new NetBufferReadStream(dataMsg);
			while (dataMsg.LengthBits - dataMsg.Position >= 8)
			{
				LidgrenMessageUtils.ReadStreamToMessage(stream, message);
				base.OnProcessingMessage(message, sendMessageImpl, delegate
				{
					this.processIncomingMessage(message);
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
}
