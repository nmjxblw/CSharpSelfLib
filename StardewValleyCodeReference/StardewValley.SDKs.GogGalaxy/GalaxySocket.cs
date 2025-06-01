using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Galaxy.Api;
using StardewValley.Network;
using StardewValley.SDKs.GogGalaxy.Internal;
using StardewValley.SDKs.GogGalaxy.Listeners;
using Steamworks;

namespace StardewValley.SDKs.GogGalaxy;

public class GalaxySocket
{
	public const long Timeout = 30000L;

	/// <summary>The key for the multiplayer protocol version in the Galaxy lobby data.</summary>
	public const string ProtocolVersionKey = "protocolVersion";

	/// <summary>The key for the host's display name in the Galaxy lobby data.</summary>
	public const string HostNameDataKey = "HostDisplayName";

	/// <summary>The key for the Steam host's ID in the Galaxy lobby data.</summary>
	public const string SteamHostIdDataKey = "SteamHostId";

	/// <summary>The key for the Steam lobby's ID in the Galaxy lobby data.</summary>
	public const string SteamLobbyIdDataKey = "SteamLobbyId";

	private const int SendMaxPacketSize = 1100;

	private const int ReceiveMaxPacketSize = 1300;

	private const long RecreateLobbyDelay = 20000L;

	private const long HeartbeatDelay = 8L;

	private const byte HeartbeatMessage = byte.MaxValue;

	public bool isRecreatedLobby;

	public bool isFirstRecreateAttempt;

	private GalaxyID selfId;

	private GalaxyID connectingLobbyID;

	private GalaxyID lobby;

	private GalaxyID lobbyOwner;

	private GalaxyLobbyEnteredListener galaxyLobbyEnterCallback;

	private GalaxyLobbyCreatedListener galaxyLobbyCreatedCallback;

	private GalaxyLobbyLeftListener galaxyLobbyLeftCallback;

	private GalaxyLobbyMemberStateListener galaxyLobbyMemberStateCallback;

	private string protocolVersion;

	private bool checkedProcotolVersion;

	private Dictionary<string, string> lobbyData = new Dictionary<string, string>();

	private ServerPrivacy privacy;

	private uint memberLimit;

	private long recreateTimer;

	private long heartbeatTimer;

	private Dictionary<ulong, GalaxyID> connections = new Dictionary<ulong, GalaxyID>();

	private HashSet<ulong> ghosts = new HashSet<ulong>();

	private Dictionary<ulong, MemoryStream> incompletePackets = new Dictionary<ulong, MemoryStream>();

	public int ConnectionCount => this.connections.Count;

	public IEnumerable<GalaxyID> Connections => this.connections.Values;

	public bool Connected => this.lobby != null;

	public GalaxyID LobbyOwner => this.lobbyOwner;

	public GalaxyID Lobby => this.lobby;

	public ulong? InviteDialogLobby => null;

	public GalaxySocket(string protocolVersion)
	{
		this.protocolVersion = protocolVersion;
		this.checkedProcotolVersion = false;
		this.lobbyData["protocolVersion"] = protocolVersion;
		this.selfId = GalaxyInstance.User().GetGalaxyID();
		this.galaxyLobbyEnterCallback = new GalaxyLobbyEnteredListener(onGalaxyLobbyEnter);
		this.galaxyLobbyCreatedCallback = new GalaxyLobbyCreatedListener(onGalaxyLobbyCreated);
		this.galaxyLobbyMemberStateCallback = new GalaxyLobbyMemberStateListener(onGalaxyMemberState);
		this.lobbyData["SteamHostId"] = SteamUser.GetSteamID().m_SteamID.ToString();
		this.lobbyData["HostDisplayName"] = SteamFriends.GetPersonaName();
	}

	public string GetInviteCode()
	{
		if (this.lobby == null)
		{
			return null;
		}
		return "S" + Base36.Encode(this.lobby.GetRealID());
	}

	private string getConnectionString()
	{
		if (this.lobby == null)
		{
			return "";
		}
		return "-connect-lobby-" + this.lobby.ToUint64();
	}

	private long getTimeNow()
	{
		return DateTime.UtcNow.Ticks / 10000;
	}

	public long GetPingWith(GalaxyID peer)
	{
		return GalaxyInstance.Networking().GetPingWith(peer);
	}

	private LobbyType privacyToLobbyType(ServerPrivacy privacy)
	{
		return privacy switch
		{
			ServerPrivacy.InviteOnly => LobbyType.LOBBY_TYPE_PRIVATE, 
			ServerPrivacy.FriendsOnly => LobbyType.LOBBY_TYPE_FRIENDS_ONLY, 
			ServerPrivacy.Public => LobbyType.LOBBY_TYPE_PUBLIC, 
			_ => throw new ArgumentException($"Unknown server privacy type '{privacy}'"), 
		};
	}

	public void SetPrivacy(ServerPrivacy privacy)
	{
		this.privacy = privacy;
		this.updateLobbyPrivacy();
	}

	public void CreateLobby(ServerPrivacy privacy, uint memberLimit)
	{
		this.privacy = privacy;
		this.memberLimit = memberLimit;
		this.lobbyOwner = this.selfId;
		this.isRecreatedLobby = false;
		this.tryCreateLobby();
	}

	private void tryCreateLobby()
	{
		Game1.log.Verbose("Creating lobby...");
		if (this.galaxyLobbyLeftCallback != null)
		{
			this.galaxyLobbyLeftCallback.Dispose();
			this.galaxyLobbyLeftCallback = null;
		}
		this.galaxyLobbyLeftCallback = new GalaxyLobbyLeftListener(onGalaxyLobbyLeft);
		try
		{
			GalaxyInstance.Matchmaking().CreateLobby(this.privacyToLobbyType(this.privacy), this.memberLimit, joinable: true, LobbyTopologyType.LOBBY_TOPOLOGY_TYPE_STAR);
		}
		catch (Exception exception)
		{
			Game1.log.Error("Galaxy CreateLobby failed with an exception:", exception);
			this.OnLobbyCreateFailed();
		}
		this.recreateTimer = 0L;
	}

	public void JoinLobby(GalaxyID lobbyId, Action<string> onError)
	{
		try
		{
			this.connectingLobbyID = lobbyId;
			GalaxyInstance.Matchmaking().JoinLobby(this.connectingLobbyID);
		}
		catch (Exception ex)
		{
			Game1.log.Error("Error joining Galaxy lobby.", ex);
			string error_message = Game1.content.LoadString("Strings\\UI:CoopMenu_Failed");
			error_message = ((!ex.Message.EndsWith("already joined this lobby")) ? (error_message + " (" + ex.Message + ")") : (error_message + " (already connected)"));
			onError(error_message);
			this.Close();
		}
	}

	public void SetLobbyData(string key, string value)
	{
		this.lobbyData[key] = value;
		if (this.lobby != null)
		{
			GalaxyInstance.Matchmaking().SetLobbyData(this.lobby, key, value);
		}
	}

	private void updateLobbyPrivacy()
	{
		if (!(this.lobbyOwner != this.selfId) && this.lobby != null)
		{
			GalaxyInstance.Matchmaking().SetLobbyType(this.lobby, this.privacyToLobbyType(this.privacy));
		}
	}

	/// <summary>Logs a failure to create a lobby, and attempts to create a new lobby.</summary>
	private void OnLobbyCreateFailed()
	{
		if (Game1.chatBox != null && this.isFirstRecreateAttempt)
		{
			if (this.isRecreatedLobby)
			{
				Game1.chatBox.addInfoMessage(Game1.content.LoadString("Strings\\UI:Chat_LobbyCreateFail"));
			}
			else
			{
				Game1.chatBox.addInfoMessage(Game1.content.LoadString("Strings\\UI:Chat_LobbyCreateFail"));
			}
		}
		this.recreateTimer = this.getTimeNow() + 20000;
		this.isRecreatedLobby = true;
		this.isFirstRecreateAttempt = false;
	}

	private void onGalaxyLobbyCreated(GalaxyID lobbyID, LobbyCreateResult result)
	{
		if (result == LobbyCreateResult.LOBBY_CREATE_RESULT_ERROR)
		{
			Game1.log.Error("Failed to create Galaxy lobby.");
			this.OnLobbyCreateFailed();
		}
	}

	/// <summary>A Galaxy lobby listener that logs member state changes (entering the lobby, leaving the lobby, etc.)</summary>
	private void onGalaxyMemberState(GalaxyID lobbyID, GalaxyID memberID, LobbyMemberStateChange memberStateChange)
	{
		switch (memberStateChange)
		{
		case LobbyMemberStateChange.LOBBY_MEMBER_STATE_CHANGED_ENTERED:
			Game1.log.Verbose($"{memberID} connected to lobby {lobbyID}");
			break;
		case LobbyMemberStateChange.LOBBY_MEMBER_STATE_CHANGED_LEFT:
			Game1.log.Verbose($"{memberID} left lobby {lobbyID}");
			break;
		case LobbyMemberStateChange.LOBBY_MEMBER_STATE_CHANGED_DISCONNECTED:
			Game1.log.Verbose($"{memberID} disconnected from lobby {lobbyID} without leaving");
			break;
		case LobbyMemberStateChange.LOBBY_MEMBER_STATE_CHANGED_KICKED:
			Game1.log.Verbose($"{memberID} was kicked from lobby {lobbyID}");
			break;
		case LobbyMemberStateChange.LOBBY_MEMBER_STATE_CHANGED_BANNED:
			Game1.log.Verbose($"{memberID} was banned from lobby {lobbyID}");
			break;
		}
	}

	private void onGalaxyLobbyLeft(GalaxyID lobbyID, ILobbyLeftListener.LobbyLeaveReason leaveReason)
	{
		if (leaveReason != ILobbyLeftListener.LobbyLeaveReason.LOBBY_LEAVE_REASON_USER_LEFT)
		{
			Program.WriteLog(Program.LogType.Disconnect, "Forcibly left Galaxy lobby at " + DateTime.Now.ToLongTimeString() + " - " + leaveReason, append: true);
		}
		if (Game1.chatBox != null)
		{
			string lobby_lost_reason = leaveReason switch
			{
				ILobbyLeftListener.LobbyLeaveReason.LOBBY_LEAVE_REASON_CONNECTION_LOST => Game1.content.LoadString("Strings\\UI:Chat_LobbyLost_ConnectionLost"), 
				ILobbyLeftListener.LobbyLeaveReason.LOBBY_LEAVE_REASON_LOBBY_CLOSED => Game1.content.LoadString("Strings\\UI:Chat_LobbyLost_LobbyClosed"), 
				ILobbyLeftListener.LobbyLeaveReason.LOBBY_LEAVE_REASON_USER_LEFT => Game1.content.LoadString("Strings\\UI:Chat_LobbyLost_UserLeft"), 
				_ => "", 
			};
			Game1.chatBox.addInfoMessage(Game1.content.LoadString("Strings\\UI:Chat_LobbyLost", lobby_lost_reason).Trim());
		}
		Game1.log.Verbose("Left lobby " + lobbyID.ToUint64() + " - leaveReason: " + leaveReason);
		this.lobby = null;
		this.recreateTimer = this.getTimeNow() + 20000;
		this.isRecreatedLobby = true;
		this.isFirstRecreateAttempt = true;
	}

	private void onGalaxyLobbyEnter(GalaxyID lobbyID, LobbyEnterResult result)
	{
		this.connectingLobbyID = null;
		if (result != LobbyEnterResult.LOBBY_ENTER_RESULT_SUCCESS)
		{
			return;
		}
		Game1.log.Verbose("Lobby entered: " + lobbyID.ToUint64());
		this.lobby = lobbyID;
		this.lobbyOwner = GalaxyInstance.Matchmaking().GetLobbyOwner(lobbyID);
		if (Game1.chatBox != null)
		{
			string invite_code_string = "";
			if (Program.sdk.Networking != null && Program.sdk.Networking.SupportsInviteCodes())
			{
				invite_code_string = Game1.content.LoadString("Strings\\UI:Chat_LobbyJoined_InviteCode", this.GetInviteCode());
			}
			if (this.isRecreatedLobby)
			{
				Game1.chatBox.addInfoMessage(Game1.content.LoadString("Strings\\UI:Chat_LobbyRecreated", invite_code_string).Trim());
			}
			else
			{
				Game1.chatBox.addInfoMessage(Game1.content.LoadString("Strings\\UI:Chat_LobbyJoined", invite_code_string).Trim());
			}
		}
		if (!(this.lobbyOwner == this.selfId))
		{
			return;
		}
		foreach (KeyValuePair<string, string> pair in this.lobbyData)
		{
			GalaxyInstance.Matchmaking().SetLobbyData(this.lobby, pair.Key, pair.Value);
		}
		this.updateLobbyPrivacy();
	}

	public IEnumerable<GalaxyID> LobbyMembers()
	{
		if (this.lobby == null)
		{
			yield break;
		}
		uint lobby_members_count;
		try
		{
			lobby_members_count = GalaxyInstance.Matchmaking().GetNumLobbyMembers(this.lobby);
		}
		catch
		{
			yield break;
		}
		uint i = 0u;
		while (i < lobby_members_count)
		{
			GalaxyID lobbyMember = GalaxyInstance.Matchmaking().GetLobbyMemberByIndex(this.lobby, i);
			if (!(lobbyMember == this.selfId) && !this.ghosts.Contains(lobbyMember.ToUint64()))
			{
				yield return lobbyMember;
			}
			uint num = i + 1;
			i = num;
		}
	}

	private void close(GalaxyID peer)
	{
		this.connections.Remove(peer.ToUint64());
		this.incompletePackets.Remove(peer.ToUint64());
	}

	public void Kick(GalaxyID user)
	{
		this.ghosts.Add(user.ToUint64());
	}

	public void Close()
	{
		if (this.connectingLobbyID != null)
		{
			GalaxyInstance.Matchmaking().LeaveLobby(this.connectingLobbyID);
			this.connectingLobbyID = null;
		}
		if (this.lobby != null)
		{
			while (this.ConnectionCount > 0)
			{
				this.close(this.Connections.First());
			}
			GalaxyInstance.Matchmaking().LeaveLobby(this.lobby);
			this.lobby = null;
		}
		this.updateLobbyPrivacy();
		try
		{
			this.galaxyLobbyEnterCallback.Dispose();
		}
		catch (Exception)
		{
		}
		try
		{
			this.galaxyLobbyCreatedCallback.Dispose();
		}
		catch (Exception)
		{
		}
		try
		{
			this.galaxyLobbyMemberStateCallback.Dispose();
		}
		catch (Exception)
		{
		}
		this.galaxyLobbyLeftCallback?.Dispose();
	}

	/// <summary>Decompress a message if necessary and pass the result to <paramref name="onMessage" />.</summary>
	/// <param name="peer">The Galaxy ID of the peer who sent this message to us.</param>
	/// <param name="stream">A memory stream containing the message data.</param>
	/// <param name="onMessage">A callback to handle the message the processed data.</param>
	private void PreprocessMessage(GalaxyID peer, MemoryStream stream, Action<GalaxyID, Stream> onMessage)
	{
		if (Program.netCompression.TryDecompressStream(stream, out var decompressed))
		{
			stream = new MemoryStream(decompressed);
		}
		onMessage(peer, stream);
	}

	public void Receive(Action<GalaxyID> onConnection, Action<GalaxyID, Stream> onMessage, Action<GalaxyID> onDisconnect, Action<string> onError)
	{
		long timeNow = this.getTimeNow();
		if (this.lobby == null)
		{
			if (this.lobbyOwner == this.selfId && this.recreateTimer > 0 && this.recreateTimer <= timeNow)
			{
				this.recreateTimer = 0L;
				this.tryCreateLobby();
			}
			this.DisconnectPeers(onDisconnect);
			return;
		}
		if (!this.checkedProcotolVersion)
		{
			try
			{
				string lobbyVersion = GalaxyInstance.Matchmaking().GetLobbyData(this.lobby, "protocolVersion");
				if (lobbyVersion != "")
				{
					this.checkedProcotolVersion = true;
					if (lobbyVersion != this.protocolVersion)
					{
						onError(Game1.content.LoadString("Strings\\UI:CoopMenu_FailedProtocolVersion"));
						this.Close();
						return;
					}
				}
			}
			catch (Exception)
			{
			}
		}
		IEnumerable<GalaxyID> cachedLobbyMembers = this.LobbyMembers();
		foreach (GalaxyID lobbyMember in cachedLobbyMembers)
		{
			if (!this.connections.ContainsKey(lobbyMember.ToUint64()) && !this.ghosts.Contains(lobbyMember.ToUint64()))
			{
				this.connections.Add(lobbyMember.ToUint64(), lobbyMember);
				onConnection(lobbyMember);
			}
		}
		this.ghosts.IntersectWith(cachedLobbyMembers.Select((GalaxyID peer) => peer.ToUint64()));
		byte[] buffer = new byte[1300];
		uint packetSize = 1300u;
		GalaxyID sender = new GalaxyID();
		while (GalaxyInstance.Networking().ReadP2PPacket(buffer, (uint)buffer.Length, ref packetSize, ref sender))
		{
			if (!this.connections.ContainsKey(sender.ToUint64()) || buffer[0] == byte.MaxValue)
			{
				continue;
			}
			bool incomplete = buffer[0] == 1;
			MemoryStream messageData = new MemoryStream();
			messageData.Write(buffer, 4, (int)(packetSize - 4));
			if (this.incompletePackets.TryGetValue(sender.ToUint64(), out var packet))
			{
				messageData.Position = 0L;
				messageData.CopyTo(packet);
				if (!incomplete)
				{
					messageData = packet;
					this.incompletePackets.Remove(sender.ToUint64());
					messageData.Position = 0L;
					this.PreprocessMessage(sender, messageData, onMessage);
				}
			}
			else if (incomplete)
			{
				messageData.Position = messageData.Length;
				this.incompletePackets[sender.ToUint64()] = messageData;
			}
			else
			{
				messageData.Position = 0L;
				this.PreprocessMessage(sender, messageData, onMessage);
			}
		}
		this.DisconnectPeers(onDisconnect);
	}

	public virtual void DisconnectPeers(Action<GalaxyID> onDisconnect)
	{
		List<GalaxyID> disconnectedPeers = new List<GalaxyID>();
		HashSet<GalaxyID> onlinePeers = new HashSet<GalaxyID>();
		foreach (GalaxyID lobbyMember in this.LobbyMembers())
		{
			onlinePeers.Add(lobbyMember);
		}
		foreach (GalaxyID peer in this.connections.Values)
		{
			if (this.lobby == null || !onlinePeers.Contains(peer))
			{
				disconnectedPeers.Add(peer);
			}
		}
		foreach (GalaxyID peer2 in disconnectedPeers)
		{
			onDisconnect(peer2);
			this.close(peer2);
		}
	}

	public void Heartbeat(IEnumerable<GalaxyID> peers)
	{
		long timeNow = this.getTimeNow();
		if (this.heartbeatTimer > timeNow)
		{
			return;
		}
		this.heartbeatTimer = timeNow + 8;
		byte[] heartbeatPacket = new byte[1] { 255 };
		foreach (GalaxyID peer in peers)
		{
			GalaxyInstance.Networking().SendP2PPacket(peer, heartbeatPacket, (uint)heartbeatPacket.Length, P2PSendType.P2P_SEND_RELIABLE_IMMEDIATE);
		}
	}

	public void Send(GalaxyID peer, byte[] data)
	{
		if (!this.connections.ContainsKey(peer.ToUint64()))
		{
			return;
		}
		data = Program.netCompression.CompressAbove(data);
		if (data.Length <= 1100)
		{
			byte[] packet = new byte[data.Length + 4];
			data.CopyTo(packet, 4);
			GalaxyInstance.Networking().SendP2PPacket(peer, packet, (uint)packet.Length, P2PSendType.P2P_SEND_RELIABLE);
			return;
		}
		int chunkSize = 1096;
		int messageOffset = 0;
		byte[] packet2 = new byte[1100];
		packet2[0] = 1;
		while (messageOffset < data.Length)
		{
			int thisChunkSize = chunkSize;
			if (messageOffset + chunkSize >= data.Length)
			{
				packet2[0] = 0;
				thisChunkSize = data.Length - messageOffset;
			}
			Buffer.BlockCopy(data, messageOffset, packet2, 4, thisChunkSize);
			messageOffset += thisChunkSize;
			GalaxyInstance.Networking().SendP2PPacket(peer, packet2, (uint)(thisChunkSize + 4), P2PSendType.P2P_SEND_RELIABLE);
		}
	}

	public void Send(GalaxyID peer, OutgoingMessage message)
	{
		using MemoryStream stream = new MemoryStream();
		using BinaryWriter writer = new BinaryWriter(stream);
		message.Write(writer);
		stream.Seek(0L, SeekOrigin.Begin);
		this.Send(peer, stream.ToArray());
	}
}
