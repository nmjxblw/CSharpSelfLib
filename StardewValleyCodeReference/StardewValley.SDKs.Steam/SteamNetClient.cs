using System;
using Galaxy.Api;
using StardewValley.Network;
using StardewValley.SDKs.GogGalaxy.Listeners;
using StardewValley.SDKs.Steam.Internal;
using Steamworks;

namespace StardewValley.SDKs.Steam;

internal sealed class SteamNetClient : HookableClient
{
	/// <summary>The max number of messages we can receive in a single frame.</summary>
	private const int ClientBufferSize = 256;

	/// <summary>The callback used to check the result of entering a Steam lobby.</summary>
	private CallResult<LobbyEnter_t> SteamLobbyEnterCallResult;

	/// <summary>The callback used to handle changes in the connection state (connecting, connected, disconnected, etc.).</summary>
	private readonly Callback<SteamNetConnectionStatusChangedCallback_t> SteamNetConnectionStatusChangedCallback;

	/// <summary>The callback used to check the result of retrieving Galaxy lobby data.</summary>
	private GalaxyLobbyDataRetrieveListener GalaxyLobbyDataRetrieveCallback;

	/// <summary>The pointers to received messages.</summary>
	private readonly IntPtr[] Messages = new IntPtr[256];

	/// <summary>The Galaxy lobby ID. If this is valid, we will fetch <see cref="F:StardewValley.SDKs.Steam.SteamNetClient.SteamLobby" /> by querying the <see cref="F:StardewValley.SDKs.GogGalaxy.GalaxySocket.SteamLobbyIdDataKey" /> lobby data.</summary>
	private GalaxyID GalaxyLobby;

	/// <summary>The Steam lobby ID. If this is valid, we will fetch <see cref="F:StardewValley.SDKs.Steam.SteamNetClient.HostId" /> by querying the lobby owner.</summary>
	private CSteamID SteamLobby;

	/// <summary>The Steam host ID that the client will connect to.</summary>
	private CSteamID HostId;

	/// <summary>The Steam display name of the hosting player.</summary>
	private string CachedHostName;

	/// <summary>The Steam Networking Socket connection between the client and server.</summary>
	private HSteamNetConnection Connection = HSteamNetConnection.Invalid;

	/// <summary>Constructs an instance that resolves the host from a Galaxy lobby.</summary>
	/// <param name="galaxyLobby">The Galaxy lobby that we will be querying for the Steam host ID.</param>
	public SteamNetClient(GalaxyID galaxyLobby)
	{
		this.SteamNetConnectionStatusChangedCallback = Callback<SteamNetConnectionStatusChangedCallback_t>.Create(OnSteamNetConnectionStatusChanged);
		this.GalaxyLobby = galaxyLobby;
	}

	/// <summary>Constructs an instance that resolves the host from a Steam lobby.</summary>
	/// <param name="steamLobby">The Steam lobby that we will be querying for the Steam host ID.</param>
	public SteamNetClient(CSteamID steamLobby)
	{
		this.SteamNetConnectionStatusChangedCallback = Callback<SteamNetConnectionStatusChangedCallback_t>.Create(OnSteamNetConnectionStatusChanged);
		this.GalaxyLobby = null;
		this.SteamLobby = steamLobby;
	}

	/// <summary>Cleans up the instance and its callbacks.</summary>
	~SteamNetClient()
	{
		this.CleanupLobbyDataRetrieve();
		this.SteamNetConnectionStatusChangedCallback.Unregister();
	}

	/// <summary>Handles disconnecting from the server, and cleans up the connection.</summary>
	/// <param name="connection">The connection to clean up.</param>
	private void OnDisconnected(HSteamNetConnection connection)
	{
		if (!(connection == HSteamNetConnection.Invalid) && !(connection != this.Connection))
		{
			Game1.log.Verbose($"Client disconnected from server {this.HostId.m_SteamID}");
			base.timedOut = true;
			base.pendingDisconnect = Multiplayer.DisconnectType.HostLeft;
			SteamSocketUtils.CloseConnection(this.Connection);
			this.Connection = HSteamNetConnection.Invalid;
		}
	}

	/// <summary>Handles changes in the <see cref="F:StardewValley.SDKs.Steam.SteamNetClient.Connection" /> status.</summary>
	/// <param name="evt">The information about the connection and its new status.</param>
	private void OnSteamNetConnectionStatusChanged(SteamNetConnectionStatusChangedCallback_t evt)
	{
		if (!(evt.m_hConn != this.Connection))
		{
			switch (evt.m_info.m_eState)
			{
			case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connecting:
				Game1.log.Verbose($"Client connecting to server {this.HostId.m_SteamID}");
				break;
			case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_Connected:
				Game1.log.Verbose($"Client connected to server {this.HostId.m_SteamID}");
				break;
			case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_ClosedByPeer:
			case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_ProblemDetectedLocally:
				this.OnDisconnected(evt.m_hConn);
				break;
			case ESteamNetworkingConnectionState.k_ESteamNetworkingConnectionState_FindingRoute:
				break;
			}
		}
	}

	public override string getUserID()
	{
		return Program.sdk.Networking.GetUserID();
	}

	protected override string getHostUserName()
	{
		if (!this.HostId.IsValid())
		{
			return "???";
		}
		string userName = SteamFriends.GetFriendPersonaName(this.HostId);
		if (string.IsNullOrWhiteSpace(userName) || userName == "[unknown]")
		{
			userName = this.CachedHostName;
		}
		this.CachedHostName = userName;
		return userName;
	}

	/// <summary>Connects to the obtained host ID.</summary>
	private void ConnectToHost()
	{
		Game1.log.Verbose($"Found Steam host {this.HostId.m_SteamID}");
		SteamNetworkingIdentity identity = default(SteamNetworkingIdentity);
		identity.SetSteamID(this.HostId);
		SteamNetworkingConfigValue_t[] options = SteamSocketUtils.GetNetworkingOptions();
		this.Connection = SteamNetworkingSockets.ConnectP2P(ref identity, 0, options.Length, options);
	}

	/// <summary>Attempts to fetch the host data and connect from the Steam lobby.</summary>
	/// <param name="evt">The data for the lobby enter event.</param>
	/// <param name="ioFailure">Whether joining the lobby failed due to an I/O error.</param>
	/// <param name="errorTranslationKey">The translation key for the UI error message, if applicable.</param>
	/// <returns>Returns an error indicating why connection failed, if applicable.</returns>
	private string TryConnectSteam(LobbyEnter_t evt, bool ioFailure, out string errorTranslationKey)
	{
		this.SteamLobby.Clear();
		if (ioFailure)
		{
			errorTranslationKey = "Strings\\UI:CoopMenu_Failed";
			return "IO Failure";
		}
		if (evt.m_EChatRoomEnterResponse != 1)
		{
			errorTranslationKey = "Strings\\UI:CoopMenu_Failed";
			return $"Failed to join: {evt.m_EChatRoomEnterResponse}";
		}
		this.SteamLobby = new CSteamID(evt.m_ulSteamIDLobby);
		string protocolVersion = SteamMatchmaking.GetLobbyData(this.SteamLobby, "protocolVersion");
		if (protocolVersion != Multiplayer.protocolVersion)
		{
			errorTranslationKey = "Strings\\UI:CoopMenu_FailedProtocolVersion";
			if (!(protocolVersion == ""))
			{
				return $"Protocol ({protocolVersion}) does not match our own ({Multiplayer.protocolVersion})";
			}
			return "Missing protocol version data";
		}
		if (!SteamMatchmaking.GetLobbyGameServer(this.SteamLobby, out var _, out var _, out var hostId))
		{
			errorTranslationKey = "Strings\\UI:CoopMenu_Failed";
			return "Missing game server data";
		}
		if (!hostId.IsValid())
		{
			errorTranslationKey = "Strings\\UI:CoopMenu_Failed";
			return "Invalid host ID";
		}
		this.CachedHostName = SteamFriends.GetFriendPersonaName(this.HostId);
		SteamFriends.RequestUserInformation(hostId, bRequireNameOnly: true);
		this.HostId = hostId;
		this.ConnectToHost();
		errorTranslationKey = null;
		return null;
	}

	/// <summary>Handles the result of joining a Steam lobby.</summary>
	/// <param name="evt">The data for the Lobby enter event.</param>
	/// <param name="ioFailure">Whether joining the lobby failed due to an I/O error.</param>
	private void OnLobbyEnter(LobbyEnter_t evt, bool ioFailure)
	{
		if (evt.m_ulSteamIDLobby == this.SteamLobby.m_SteamID)
		{
			string errorTranslationKey;
			string errorMsg = this.TryConnectSteam(evt, ioFailure, out errorTranslationKey);
			if (errorMsg != null)
			{
				base.connectionMessage = Game1.content.LoadString(errorTranslationKey);
				Game1.log.Verbose($"Error joining via Steam lobby {evt.m_ulSteamIDLobby} ({errorMsg})");
			}
			this.SteamLobbyEnterCallResult = null;
		}
	}

	/// <summary>Starts the client connection process via Steam lobby.</summary>
	private void ConnectImplSteam()
	{
		Game1.log.Verbose($"Resolving Steam host via Steam lobby {this.SteamLobby.m_SteamID}");
		this.SteamLobbyEnterCallResult = CallResult<LobbyEnter_t>.Create(OnLobbyEnter);
		SteamAPICall_t steamApiCall = SteamMatchmaking.JoinLobby(this.SteamLobby);
		this.SteamLobbyEnterCallResult.Set(steamApiCall);
	}

	/// <summary>Handles common cleanup tasks for <see cref="M:StardewValley.SDKs.Steam.SteamNetClient.OnLobbyDataRetrieveSuccess(Galaxy.Api.GalaxyID)" /> and <see cref="M:StardewValley.SDKs.Steam.SteamNetClient.OnLobbyDataRetrieveFailure(Galaxy.Api.GalaxyID,Galaxy.Api.ILobbyDataRetrieveListener.FailureReason)" />.</summary>
	private void CleanupLobbyDataRetrieve()
	{
		this.GalaxyLobbyDataRetrieveCallback?.Dispose();
		this.GalaxyLobbyDataRetrieveCallback = null;
	}

	/// <summary>Attempts to fetch the host data and connect from the Galaxy lobby.</summary>
	/// <param name="lobbyId">The Galaxy ID of the lobby to fetch host data from.</param>
	/// <param name="errorTranslationKey">The translation key for the UI error message, if applicable.</param>
	/// <returns>Returns an error indicating why connection failed, if applicable.</returns>
	private string TryConnectGalaxy(GalaxyID lobbyId, out string errorTranslationKey)
	{
		string steamLobbyIdString;
		try
		{
			steamLobbyIdString = GalaxyInstance.Matchmaking().GetLobbyData(lobbyId, "SteamLobbyId");
		}
		catch (Exception)
		{
			errorTranslationKey = "Strings\\UI:CoopMenu_Failed";
			return "Failed to get Steam lobby ID";
		}
		if (string.IsNullOrEmpty(steamLobbyIdString))
		{
			errorTranslationKey = "Strings\\UI:CoopMenu_Failed";
			return "Missing Steam lobby ID";
		}
		string protocolVersion;
		try
		{
			protocolVersion = GalaxyInstance.Matchmaking().GetLobbyData(lobbyId, "protocolVersion");
		}
		catch (Exception)
		{
			errorTranslationKey = "Strings\\UI:CoopMenu_FailedProtocolVersion";
			return "Failed to get protocol version";
		}
		if (protocolVersion != Multiplayer.protocolVersion)
		{
			errorTranslationKey = "Strings\\UI:CoopMenu_FailedProtocolVersion";
			if (!string.IsNullOrEmpty(protocolVersion))
			{
				return $"Protocol ({protocolVersion}) does not match our own ({Multiplayer.protocolVersion})";
			}
			return "Missing protocol version data";
		}
		CSteamID steamLobbyId = default(CSteamID);
		try
		{
			steamLobbyId = new CSteamID(Convert.ToUInt64(steamLobbyIdString));
		}
		catch (Exception)
		{
		}
		if (!steamLobbyId.IsValid())
		{
			errorTranslationKey = "Strings\\UI:CoopMenu_Failed";
			return "Invalid lobby ID";
		}
		this.SteamLobby = steamLobbyId;
		this.GalaxyLobby = null;
		errorTranslationKey = null;
		this.ConnectImplSteam();
		return null;
	}

	/// <summary>Handles a successful retrieval of data from the Galaxy lobby.</summary>
	/// <param name="lobbyId">The Galaxy ID of the lobby we retrieved data from.</param>
	private void OnLobbyDataRetrieveSuccess(GalaxyID lobbyId)
	{
		if (lobbyId != null && lobbyId != this.GalaxyLobby)
		{
			return;
		}
		string errorTranslationKey;
		string errorMsg = this.TryConnectGalaxy(lobbyId, out errorTranslationKey);
		if (errorMsg != null)
		{
			base.connectionMessage = Game1.content.LoadString(errorTranslationKey);
			Game1.log.Verbose($"Error joining via Galaxy lobby {lobbyId} ({errorMsg})");
		}
		else
		{
			try
			{
				GalaxyInstance.Matchmaking().LeaveLobby(lobbyId);
			}
			catch (Exception)
			{
			}
		}
		this.CleanupLobbyDataRetrieve();
	}

	/// <summary>Handles a failure to retrieve data from the Galaxy lobby.</summary>
	/// <param name="lobbyId">The Galaxy ID of the lobby we failed to retrieve data from.</param>
	/// <param name="failureReason">The reason why we failed to retrieve data from the lobby.</param>
	private void OnLobbyDataRetrieveFailure(GalaxyID lobbyId, ILobbyDataRetrieveListener.FailureReason failureReason)
	{
		if (!(lobbyId != null) || !(lobbyId != this.GalaxyLobby))
		{
			base.connectionMessage = Game1.content.LoadString("Strings\\UI:CoopMenu_Failed");
			Game1.log.Verbose($"Steam client failed to get data from Galaxy lobby {lobbyId}");
			this.CleanupLobbyDataRetrieve();
		}
	}

	/// <summary>Starts the client connection process via Galaxy lobby.</summary>
	private void ConnectImplGalaxy()
	{
		Game1.log.Verbose($"Resolving Steam lobby via Galaxy lobby {this.GalaxyLobby}");
		this.GalaxyLobbyDataRetrieveCallback = new GalaxyLobbyDataRetrieveListener(OnLobbyDataRetrieveSuccess, OnLobbyDataRetrieveFailure);
		try
		{
			GalaxyInstance.Matchmaking().RequestLobbyData(this.GalaxyLobby, this.GalaxyLobbyDataRetrieveCallback);
		}
		catch (Exception exception)
		{
			base.connectionMessage = Game1.content.LoadString("Strings\\UI:CoopMenu_Failed");
			Game1.log.Error("Steam client Galaxy RequestLobbyData failed with an exception:", exception);
			this.CleanupLobbyDataRetrieve();
		}
	}

	protected override void connectImpl()
	{
		if (this.GalaxyLobby == null)
		{
			this.ConnectImplSteam();
		}
		else
		{
			this.ConnectImplGalaxy();
		}
	}

	public override void disconnect(bool neatly = true)
	{
		if (this.SteamLobby.IsValid())
		{
			SteamMatchmaking.LeaveLobby(this.SteamLobby);
			this.SteamLobby.Clear();
		}
		Game1.log.Verbose($"Client disconnecting from server {this.HostId.m_SteamID}");
		base.connectionMessage = null;
		this.ShutdownConnection();
	}

	protected override void receiveMessagesImpl()
	{
		if (this.Connection == HSteamNetConnection.Invalid)
		{
			return;
		}
		int messageCount = SteamNetworkingSockets.ReceiveMessagesOnConnection(this.Connection, this.Messages, 256);
		for (int messageIndex = 0; messageIndex < messageCount; messageIndex++)
		{
			IncomingMessage message = new IncomingMessage();
			SteamSocketUtils.ProcessSteamMessage(this.Messages[messageIndex], message, out var _, base.bandwidthLogger);
			base.OnProcessingMessage(message, SendMessageImpl, delegate
			{
				this.processIncomingMessage(message);
			});
		}
		SteamNetworkingSockets.FlushMessagesOnConnection(this.Connection);
	}

	public override void sendMessage(OutgoingMessage message)
	{
		base.OnSendingMessage(message, SendMessageImpl, delegate
		{
			this.SendMessageImpl(message);
		});
	}

	public override float GetPingToHost()
	{
		if (this.Connection == HSteamNetConnection.Invalid)
		{
			return -1f;
		}
		SteamNetworkingSockets.GetQuickConnectionStatus(this.Connection, out var status);
		return status.m_nPing;
	}

	/// <summary>Send a message to the server.</summary>
	/// <param name="message">The message to send.</param>
	private void SendMessageImpl(OutgoingMessage message)
	{
		if (!(this.Connection == HSteamNetConnection.Invalid))
		{
			SteamSocketUtils.SendMessage(this.Connection, message, base.bandwidthLogger, OnDisconnected);
		}
	}

	/// <summary>Closes the client connection and cleans up the bookkeeping data.</summary>
	/// <remarks>
	/// In most cases, this should be used instead of calling <see cref="M:StardewValley.SDKs.Steam.Internal.SteamSocketUtils.CloseConnection(Steamworks.HSteamNetConnection,System.Action{Steamworks.HSteamNetConnection})" /> directly,
	/// otherwise the <see cref="M:StardewValley.SDKs.Steam.SteamNetClient.OnDisconnected(Steamworks.HSteamNetConnection)" /> handler will not get called. However, the <see cref="M:StardewValley.SDKs.Steam.SteamNetClient.OnDisconnected(Steamworks.HSteamNetConnection)" />
	/// handler itself should not use this method.
	/// </remarks>
	private void ShutdownConnection()
	{
		SteamSocketUtils.CloseConnection(this.Connection, OnDisconnected);
	}
}
