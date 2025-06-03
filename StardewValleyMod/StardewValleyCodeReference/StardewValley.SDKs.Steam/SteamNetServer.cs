using System;
using System.Collections.Generic;
using StardewValley.Network;
using StardewValley.SDKs.GogGalaxy;
using StardewValley.SDKs.Steam.Internal;
using Steamworks;

namespace StardewValley.SDKs.Steam;

internal sealed class SteamNetServer : HookableServer
{
	/// <summary>The max number of messages we can receive in a single frame.</summary>
	private const int ServerBufferSize = 256;

	/// <summary>The bit mask to check if a player entered the lobby.</summary>
	private const int FlagsLobbyEntered = 1;

	/// <summary>The bit mask to check if a player left the lobby for any reason.</summary>
	private const int FlagsLobbyLeft = 30;

	/// <summary>The callback used to check the result of creating a Steam lobby.</summary>
	private CallResult<LobbyCreated_t> LobbyCreatedCallResult;

	/// <summary>The callback used to handle changes in the connection state (connecting, connected, disconnected, etc).</summary>
	private Callback<SteamNetConnectionStatusChangedCallback_t> SteamNetConnectionStatusChangedCallback;

	/// <summary>The callback used to handle changes to chat room members (joined the lobby, left the lobby, etc).</summary>
	private Callback<LobbyChatUpdate_t> LobbyChatUpdateCallback;

	/// <summary>A local copy of the lobby data, in case the Steam lobby is not ready.</summary>
	private Dictionary<string, string> LobbyData;

	/// <summary>The connection data by Steam Networking Socket.</summary>
	private Dictionary<HSteamNetConnection, ConnectionData> ConnectionDataMap;

	/// <summary>The connection data by farmer ID.</summary>
	private Dictionary<long, ConnectionData> FarmerConnectionMap;

	/// <summary>The cached display names of Steam lobby members.</summary>
	private Dictionary<CSteamID, string> CachedDisplayNames;

	/// <summary>The connections that changed poll groups during a call to <see cref="M:StardewValley.SDKs.Steam.SteamNetServer.PollJoiningMessages" />.</summary>
	private HashSet<HSteamNetConnection> RecentlyJoined;

	/// <summary>The pointers to received messages.</summary>
	private readonly IntPtr[] Messages = new IntPtr[256];

	/// <summary>The Steam ID of the game server's lobby.</summary>
	private CSteamID Lobby;

	/// <summary>The Steam socket used to handle incoming connections.</summary>
	private HSteamListenSocket ListenSocket = HSteamListenSocket.Invalid;

	/// <summary>The poll group used for connections that have not selected a farmhand.</summary>
	private HSteamNetPollGroup JoiningGroup = HSteamNetPollGroup.Invalid;

	/// <summary>The poll group used for connections currently playing as a farmhand.</summary>
	private HSteamNetPollGroup FarmhandGroup = HSteamNetPollGroup.Invalid;

	/// <summary>The privacy setting for the server's lobby.</summary>
	private ServerPrivacy Privacy;

	public override int connectionsCount => this.ConnectionDataMap?.Count ?? 0;

	/// <summary>Creates an instance of the <see cref="T:StardewValley.SDKs.Steam.SteamNetServer" />.</summary>
	public SteamNetServer(IGameServer gameServer)
		: base(gameServer)
	{
	}

	/// <summary>Applies the privacy setting from <see cref="F:StardewValley.SDKs.Steam.SteamNetServer.Privacy" /> to the game server's lobby.</summary>
	private void UpdateLobbyPrivacy()
	{
		if (this.Lobby.IsValid())
		{
			ServerPrivacy privacy = this.Privacy;
			SteamMatchmaking.SetLobbyType(this.Lobby, privacy switch
			{
				ServerPrivacy.FriendsOnly => ELobbyType.k_ELobbyTypeFriendsOnly, 
				ServerPrivacy.Public => ELobbyType.k_ELobbyTypePublic, 
				_ => ELobbyType.k_ELobbyTypePrivate, 
			});
		}
	}

	/// <summary>Converts a <see cref="T:StardewValley.SDKs.Steam.Internal.ConnectionData" /> to a connection string to be used in the Stardew API.</summary>
	/// <param name="connection">The connection data to convert to a unique connection string.</param>
	/// <returns>Returns a string that uniquely corresponds to the <see cref="T:StardewValley.SDKs.Steam.Internal.ConnectionData" />.</returns>
	private string ConnectionDataToId(ConnectionData connection)
	{
		return $"SN_{connection.SteamId.m_SteamID}_{connection.Connection.m_HSteamNetConnection}";
	}

	/// <summary>Gets the internal <see cref="T:StardewValley.SDKs.Steam.Internal.ConnectionData" /> for a corresponding connection string.</summary>
	/// <param name="connectionId">The unique connection string to fetch <see cref="T:StardewValley.SDKs.Steam.Internal.ConnectionData" /> for.</param>
	/// <returns>Returns the <see cref="T:StardewValley.SDKs.Steam.Internal.ConnectionData" /> bookkeeping object that corresponds to the <paramref name="connectionId" /> string, or <c>null</c> if not found.</returns>
	private ConnectionData IdToConnectionData(string connectionId)
	{
		if (connectionId.Length <= 3 || !connectionId.StartsWith("SN_"))
		{
			return null;
		}
		string steamConnectionString = connectionId.Substring(3);
		int underscoreIdx = steamConnectionString.IndexOf('_');
		if (underscoreIdx < 0)
		{
			return null;
		}
		ulong rawSteamId = default(CSteamID).m_SteamID;
		uint connectionRaw = HSteamNetConnection.Invalid.m_HSteamNetConnection;
		try
		{
			rawSteamId = Convert.ToUInt64(steamConnectionString.Substring(0, underscoreIdx));
			connectionRaw = Convert.ToUInt32(steamConnectionString.Substring(underscoreIdx + 1));
		}
		catch (Exception)
		{
		}
		if (!new CSteamID(rawSteamId).IsValid())
		{
			return null;
		}
		HSteamNetConnection connection = HSteamNetConnection.Invalid;
		connection.m_HSteamNetConnection = connectionRaw;
		if (!this.ConnectionDataMap.TryGetValue(connection, out var connectionData))
		{
			return null;
		}
		if (connectionData.SteamId.m_SteamID != rawSteamId)
		{
			return null;
		}
		return connectionData;
	}

	public override bool isConnectionActive(string connectionId)
	{
		return this.IdToConnectionData(connectionId) != null;
	}

	public override string getUserId(long farmerId)
	{
		if (!this.FarmerConnectionMap.TryGetValue(farmerId, out var connectionData))
		{
			return null;
		}
		return connectionData.SteamId.m_SteamID.ToString();
	}

	public override bool hasUserId(string userId)
	{
		CSteamID steamId = default(CSteamID);
		try
		{
			steamId = new CSteamID(Convert.ToUInt64(userId));
		}
		catch (Exception)
		{
		}
		if (!steamId.IsValid())
		{
			return false;
		}
		foreach (KeyValuePair<HSteamNetConnection, ConnectionData> item in this.ConnectionDataMap)
		{
			if (item.Value.SteamId.m_SteamID == steamId.m_SteamID)
			{
				return true;
			}
		}
		return false;
	}

	public override string getUserName(long farmerId)
	{
		if (!this.FarmerConnectionMap.TryGetValue(farmerId, out var connectionData))
		{
			return "";
		}
		string userName = SteamFriends.GetFriendPersonaName(connectionData.SteamId);
		if (string.IsNullOrWhiteSpace(userName) || userName == "[unknown]")
		{
			userName = connectionData.DisplayName;
		}
		connectionData.DisplayName = userName;
		return userName;
	}

	public override void setPrivacy(ServerPrivacy privacy)
	{
		this.Privacy = privacy;
		this.UpdateLobbyPrivacy();
	}

	public override bool connected()
	{
		if (this.Lobby.IsValid() && this.Lobby.IsLobby() && this.ListenSocket != HSteamListenSocket.Invalid && this.JoiningGroup != HSteamNetPollGroup.Invalid)
		{
			return this.FarmhandGroup != HSteamNetPollGroup.Invalid;
		}
		return false;
	}

	/// <summary>Handles new incoming connections, and rejects users that are banned.</summary>
	/// <param name="evt">The data about the incoming client connection.</param>
	/// <param name="steamId">The Steam ID of the connecting client.</param>
	private void OnConnecting(SteamNetConnectionStatusChangedCallback_t evt, CSteamID steamId)
	{
		Game1.log.Verbose($"{steamId.m_SteamID} connecting to server");
		if (base.gameServer.isUserBanned(steamId.m_SteamID.ToString()))
		{
			Game1.log.Verbose($"{steamId.m_SteamID} is banned");
			this.ShutdownConnection(evt.m_hConn);
		}
		else
		{
			SteamFriends.RequestUserInformation(steamId, bRequireNameOnly: true);
			SteamNetworkingSockets.AcceptConnection(evt.m_hConn);
		}
	}

	/// <summary>Handles newly connected clients, and creates internal bookkeeping structures for the connection.</summary>
	/// <param name="evt">A structure containing data about the newly connected client.</param>
	/// <param name="steamId">The Steam ID of the connected client.</param>
	private void OnConnected(SteamNetConnectionStatusChangedCallback_t evt, CSteamID steamId)
	{
		Game1.log.Verbose($"{steamId.m_SteamID} connected to server");
		string displayName = this.CachedDisplayNames.GetValueOrDefault(steamId);
		ConnectionData connectionData = new ConnectionData(evt.m_hConn, steamId, displayName);
		this.ConnectionDataMap[evt.m_hConn] = connectionData;
		SteamNetworkingSockets.SetConnectionPollGroup(evt.m_hConn, this.JoiningGroup);
		string connectionId = this.ConnectionDataToId(connectionData);
		this.onConnect(connectionId);
		base.gameServer.sendAvailableFarmhands("", connectionId, delegate(OutgoingMessage outgoing)
		{
			this.SendMessageToConnection(evt.m_hConn, outgoing);
		});
	}

	/// <summary>Handles client disconnects, and cleans up all bookkeeping data about the connection.</summary>
	/// <param name="evt">The data about the disconnected client.</param>
	/// <param name="steamId">The Steam ID of the disconnected client.</param>
	private void OnDisconnected(SteamNetConnectionStatusChangedCallback_t evt, CSteamID steamId)
	{
		if (!steamId.IsValid())
		{
			return;
		}
		Game1.log.Verbose($"{steamId.m_SteamID} disconnected from server");
		if (!this.ConnectionDataMap.TryGetValue(evt.m_hConn, out var connectionData))
		{
			SteamSocketUtils.CloseConnection(evt.m_hConn);
			return;
		}
		this.onDisconnect(this.ConnectionDataToId(connectionData));
		if (connectionData.Online)
		{
			this.playerDisconnected(connectionData.FarmerId);
		}
		this.ConnectionDataMap.Remove(evt.m_hConn);
		SteamSocketUtils.CloseConnection(evt.m_hConn);
	}

	/// <summary>Handles clients disconnected via <see cref="M:Steamworks.SteamNetworkingSockets.CloseConnection(Steamworks.HSteamNetConnection,System.Int32,System.String,System.Boolean)" />, and cleans up all bookkeeping data about the connection.</summary>
	/// <param name="connection">The connection to clean up.</param>
	private void OnDisconnected(HSteamNetConnection connection)
	{
		SteamNetConnectionStatusChangedCallback_t fakeStatusChange = new SteamNetConnectionStatusChangedCallback_t
		{
			m_hConn = connection,
			m_eOldState = ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connected
		};
		SteamNetworkingSockets.GetConnectionInfo(connection, out fakeStatusChange.m_info);
		this.OnDisconnected(fakeStatusChange, fakeStatusChange.m_info.m_identityRemote.GetSteamID());
	}

	/// <summary>Handles all changes in client connection status, and invokes the corresponding handler.</summary>
	/// <param name="evt">A structure containing data about the client whose connection status changed.</param>
	private void OnSteamNetConnectionStatusChanged(SteamNetConnectionStatusChangedCallback_t evt)
	{
		switch (evt.m_info.m_eState)
		{
		case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connecting:
			this.OnConnecting(evt, evt.m_info.m_identityRemote.GetSteamID());
			break;
		case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connected:
			this.OnConnected(evt, evt.m_info.m_identityRemote.GetSteamID());
			break;
		case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_ClosedByPeer:
		case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_ProblemDetectedLocally:
			this.OnDisconnected(evt, evt.m_info.m_identityRemote.GetSteamID());
			break;
		case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_FindingRoute:
			break;
		}
	}

	/// <summary>Handles all changes in lobby member status.</summary>
	/// <param name="evt">A structure containing data about the changes to a lobby member.</param>
	private void OnLobbyChatUpdate(LobbyChatUpdate_t evt)
	{
		if (evt.m_ulSteamIDLobby == this.Lobby.m_SteamID)
		{
			CSteamID memberId = new CSteamID(evt.m_ulSteamIDUserChanged);
			if ((evt.m_rgfChatMemberStateChange & 1) != 0)
			{
				this.CachedDisplayNames[memberId] = SteamFriends.GetFriendPersonaName(memberId);
			}
			else if ((evt.m_rgfChatMemberStateChange & 0x1E) != 0)
			{
				this.CachedDisplayNames.Remove(memberId);
			}
		}
	}

	/// <summary>Handles the result of Steam lobby creation.</summary>
	/// <param name="evt">The data for the Lobby creation event.</param>
	/// <param name="ioFailure">Whether creating the lobby failed due to an I/O error.</param>
	/// <returns>Returns an error indicating why creation failed, if applicable.</returns>
	private string OnLobbyCreatedHelper(LobbyCreated_t evt, bool ioFailure)
	{
		if (ioFailure)
		{
			return "IO Failure";
		}
		switch (evt.m_eResult)
		{
		case EResult.k_EResultOK:
			this.Lobby = new CSteamID(evt.m_ulSteamIDLobby);
			return null;
		case EResult.k_EResultTimeout:
			return "Steam timed out";
		case EResult.k_EResultLimitExceeded:
			return "Too many Steam lobbies created";
		case EResult.k_EResultAccessDenied:
			return "Steam denied access";
		case EResult.k_EResultNoConnection:
			return "No connection to Steam";
		default:
			return "Unknown Steam failure";
		}
	}

	/// <summary>Handles the result of Steam lobby creation.</summary>
	/// <param name="evt">The data for the Lobby creation event.</param>
	/// <param name="ioFailure">Whether creating the lobby failed due to an I/O error.</param>
	private void OnLobbyCreated(LobbyCreated_t evt, bool ioFailure)
	{
		string lobbyError = this.OnLobbyCreatedHelper(evt, ioFailure);
		if (lobbyError == null)
		{
			SteamNetworkingConfigValue_t[] options = SteamSocketUtils.GetNetworkingOptions();
			this.ListenSocket = SteamNetworkingSockets.CreateListenSocketP2P(0, options.Length, options);
			this.JoiningGroup = SteamNetworkingSockets.CreatePollGroup();
			this.FarmhandGroup = SteamNetworkingSockets.CreatePollGroup();
			SteamMatchmaking.SetLobbyGameServer(this.Lobby, 0u, 0, SteamUser.GetSteamID());
			foreach (KeyValuePair<string, string> data in this.LobbyData)
			{
				SteamMatchmaking.SetLobbyData(this.Lobby, data.Key, data.Value);
			}
			SteamMatchmaking.SetLobbyJoinable(this.Lobby, bLobbyJoinable: true);
			this.UpdateLobbyPrivacy();
			Game1.log.Verbose($"Steam server successfully created lobby {this.Lobby.m_SteamID}");
			if (!(base.gameServer is StardewValley.Network.GameServer gameServerImpl))
			{
				return;
			}
			{
				foreach (Server server in gameServerImpl.servers)
				{
					if (server is GalaxyNetServer galaxyServer)
					{
						galaxyServer.setLobbyData("SteamLobbyId", this.Lobby.m_SteamID.ToString());
						Game1.log.Verbose("Updated Galaxy server with Steam lobby info");
						break;
					}
				}
				return;
			}
		}
		Game1.log.Verbose("Server failed to create lobby (" + lobbyError + ")");
	}

	public override void initialize()
	{
		Game1.log.Verbose("Starting Steam server");
		this.LobbyCreatedCallResult = CallResult<LobbyCreated_t>.Create(OnLobbyCreated);
		this.SteamNetConnectionStatusChangedCallback = Callback<SteamNetConnectionStatusChangedCallback_t>.Create(OnSteamNetConnectionStatusChanged);
		this.LobbyChatUpdateCallback = Callback<LobbyChatUpdate_t>.Create(OnLobbyChatUpdate);
		this.LobbyData = new Dictionary<string, string>();
		this.ConnectionDataMap = new Dictionary<HSteamNetConnection, ConnectionData>();
		this.FarmerConnectionMap = new Dictionary<long, ConnectionData>();
		this.CachedDisplayNames = new Dictionary<CSteamID, string>();
		this.RecentlyJoined = new HashSet<HSteamNetConnection>();
		this.LobbyData["protocolVersion"] = Multiplayer.protocolVersion;
		this.Lobby.Clear();
		this.ListenSocket = HSteamListenSocket.Invalid;
		this.JoiningGroup = HSteamNetPollGroup.Invalid;
		this.FarmhandGroup = HSteamNetPollGroup.Invalid;
		this.Privacy = Game1.options.serverPrivacy;
		SteamAPICall_t steamApiCall = SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePrivate, Game1.multiplayer.playerLimit * 2);
		this.LobbyCreatedCallResult.Set(steamApiCall);
	}

	public override void stopServer()
	{
		Game1.log.Verbose("Stopping Steam server");
		foreach (KeyValuePair<HSteamNetConnection, ConnectionData> item in this.ConnectionDataMap)
		{
			this.ShutdownConnection(item.Value.Connection);
		}
		if (this.Lobby.IsValid())
		{
			SteamMatchmaking.LeaveLobby(this.Lobby);
		}
		if (this.ListenSocket != HSteamListenSocket.Invalid)
		{
			SteamNetworkingSockets.CloseListenSocket(this.ListenSocket);
			this.ListenSocket = HSteamListenSocket.Invalid;
		}
		if (this.JoiningGroup != HSteamNetPollGroup.Invalid)
		{
			SteamNetworkingSockets.DestroyPollGroup(this.JoiningGroup);
			this.JoiningGroup = HSteamNetPollGroup.Invalid;
		}
		if (this.FarmhandGroup != HSteamNetPollGroup.Invalid)
		{
			SteamNetworkingSockets.DestroyPollGroup(this.FarmhandGroup);
			this.FarmhandGroup = HSteamNetPollGroup.Invalid;
		}
		this.SteamNetConnectionStatusChangedCallback?.Unregister();
		this.LobbyChatUpdateCallback?.Unregister();
	}

	/// <summary>Handles an incoming <see cref="F:StardewValley.Multiplayer.playerIntroduction" /> message.</summary>
	/// <param name="message">The incoming <see cref="F:StardewValley.Multiplayer.playerIntroduction" /> message containing information about the requested farmhand.</param>
	/// <param name="connectionData">The connection data for the player who sent the <paramref name="message" />.</param>
	private void HandleFarmhandRequest(IncomingMessage message, ConnectionData connectionData)
	{
		NetFarmerRoot farmer = Game1.multiplayer.readFarmer(message.Reader);
		long farmerId = farmer.Value.UniqueMultiplayerID;
		Game1.log.Verbose($"Server received farmhand request from {connectionData.SteamId.m_SteamID} for {farmerId}");
		base.gameServer.checkFarmhandRequest("", this.ConnectionDataToId(connectionData), farmer, delegate(OutgoingMessage outgoing)
		{
			this.SendMessageToConnection(connectionData.Connection, outgoing);
		}, delegate
		{
			Game1.log.Verbose($"Server accepted {connectionData.SteamId.m_SteamID} as farmhand {farmerId}");
			SteamNetworkingSockets.SetConnectionUserData(connectionData.Connection, farmerId);
			SteamNetworkingSockets.SetConnectionPollGroup(connectionData.Connection, this.FarmhandGroup);
			this.RecentlyJoined.Add(connectionData.Connection);
			connectionData.FarmerId = farmerId;
			connectionData.Online = true;
			this.FarmerConnectionMap[farmerId] = connectionData;
		});
	}

	/// <summary>Receives messages from the <see cref="F:StardewValley.SDKs.Steam.SteamNetServer.JoiningGroup" /> poll group, where all clients without farmhands should be.</summary>
	private void PollJoiningMessages()
	{
		this.RecentlyJoined.Clear();
		int messageCount = SteamNetworkingSockets.ReceiveMessagesOnPollGroup(this.JoiningGroup, this.Messages, 256);
		for (int messageIndex = 0; messageIndex < messageCount; messageIndex++)
		{
			IncomingMessage message = new IncomingMessage();
			SteamSocketUtils.ProcessSteamMessage(this.Messages[messageIndex], message, out var messageConnection, base.bandwidthLogger);
			if (!this.ConnectionDataMap.TryGetValue(messageConnection, out var connectionData))
			{
				Game1.log.Warn("Tried to process multiplayer message from an invalid connection.");
				this.ShutdownConnection(messageConnection);
				continue;
			}
			bool isRecentlyJoined = this.RecentlyJoined.Contains(messageConnection);
			if (connectionData.Online && !isRecentlyJoined)
			{
				Game1.log.Warn($"Online farmhand {connectionData.FarmerId} is in the wrong poll group. Closing their connection.");
				this.ShutdownConnection(messageConnection);
				continue;
			}
			base.OnProcessingMessage(message, delegate(OutgoingMessage outgoing)
			{
				this.SendMessageToConnection(messageConnection, outgoing);
			}, delegate
			{
				if (isRecentlyJoined)
				{
					base.gameServer.processIncomingMessage(message);
				}
				else if (message.MessageType == 2)
				{
					this.HandleFarmhandRequest(message, connectionData);
				}
			});
		}
	}

	/// <summary>Receives messages from the <see cref="F:StardewValley.SDKs.Steam.SteamNetServer.FarmhandGroup" /> poll group, where all clients with actively playing farmhands should be.</summary>
	private void PollFarmhandMessages()
	{
		int messageCount = SteamNetworkingSockets.ReceiveMessagesOnPollGroup(this.FarmhandGroup, this.Messages, 256);
		for (int messageIndex = 0; messageIndex < messageCount; messageIndex++)
		{
			IncomingMessage message = new IncomingMessage();
			SteamSocketUtils.ProcessSteamMessage(this.Messages[messageIndex], message, out var messageConnection, base.bandwidthLogger);
			if (message.MessageType == 2)
			{
				Game1.log.Warn("Received farmhand request in the wrong poll group. Closing their connection.");
				this.ShutdownConnection(messageConnection);
				continue;
			}
			if (!this.ConnectionDataMap.TryGetValue(messageConnection, out var connectionData))
			{
				Game1.log.Warn("Tried to process multiplayer message from an invalid connection.");
				this.ShutdownConnection(messageConnection);
				continue;
			}
			if (!connectionData.Online)
			{
				Game1.log.Warn("A non-farmhand connection is in the wrong poll group. Closing their connection.");
				this.ShutdownConnection(messageConnection);
				continue;
			}
			base.OnProcessingMessage(message, delegate(OutgoingMessage outgoing)
			{
				this.SendMessageToConnection(messageConnection, outgoing);
			}, delegate
			{
				base.gameServer.processIncomingMessage(message);
			});
		}
	}

	public override void receiveMessages()
	{
		if (!this.connected())
		{
			return;
		}
		this.PollJoiningMessages();
		this.PollFarmhandMessages();
		foreach (KeyValuePair<HSteamNetConnection, ConnectionData> item in this.ConnectionDataMap)
		{
			SteamNetworkingSockets.FlushMessagesOnConnection(item.Value.Connection);
		}
	}

	private void SendMessageToConnection(HSteamNetConnection connection, OutgoingMessage message)
	{
		SteamSocketUtils.SendMessage(connection, message, base.bandwidthLogger, OnDisconnected);
	}

	public override void sendMessage(long peerId, OutgoingMessage message)
	{
		if (this.connected() && this.FarmerConnectionMap.TryGetValue(peerId, out var connectionData) && !(connectionData.Connection == HSteamNetConnection.Invalid))
		{
			this.SendMessageToConnection(connectionData.Connection, message);
		}
	}

	public override void setLobbyData(string key, string value)
	{
		if (this.LobbyData != null)
		{
			this.LobbyData[key] = value;
			if (this.Lobby.IsValid())
			{
				SteamMatchmaking.SetLobbyData(this.Lobby, key, value);
			}
		}
	}

	public override void kick(long disconnectee)
	{
		base.kick(disconnectee);
		this.sendMessage(disconnectee, new OutgoingMessage(23, Game1.player));
		if (this.FarmerConnectionMap.TryGetValue(disconnectee, out var connectionData))
		{
			this.ShutdownConnection(connectionData.Connection);
		}
	}

	public override void playerDisconnected(long disconnectee)
	{
		if (this.FarmerConnectionMap.TryGetValue(disconnectee, out var _))
		{
			base.playerDisconnected(disconnectee);
			this.FarmerConnectionMap.Remove(disconnectee);
		}
	}

	public override float getPingToClient(long farmerId)
	{
		if (!this.FarmerConnectionMap.TryGetValue(farmerId, out var connectionData))
		{
			return -1f;
		}
		SteamNetworkingSockets.GetQuickConnectionStatus(connectionData.Connection, out var status);
		return status.m_nPing;
	}

	public override bool canOfferInvite()
	{
		return this.connected();
	}

	public override void offerInvite()
	{
		if (this.connected())
		{
			Program.sdk.Networking.ShowInviteDialog(this.Lobby);
		}
	}

	/// <summary>Closes a connection and cleans up the corresponding bookkeeping data.</summary>
	/// <param name="connection">The connection to close and clean up.</param>
	/// <remarks>
	/// In most cases, this should be used instead of calling <see cref="M:StardewValley.SDKs.Steam.Internal.SteamSocketUtils.CloseConnection(Steamworks.HSteamNetConnection,System.Action{Steamworks.HSteamNetConnection})" /> directly,
	/// otherwise the <see cref="M:StardewValley.SDKs.Steam.SteamNetServer.OnDisconnected(Steamworks.SteamNetConnectionStatusChangedCallback_t,Steamworks.CSteamID)" /> handler will not get called. However, the  <see cref="M:StardewValley.SDKs.Steam.SteamNetServer.OnDisconnected(Steamworks.SteamNetConnectionStatusChangedCallback_t,Steamworks.CSteamID)" />
	/// handler itself should not use this method.
	/// </remarks>
	private void ShutdownConnection(HSteamNetConnection connection)
	{
		SteamSocketUtils.CloseConnection(connection, OnDisconnected);
	}
}
