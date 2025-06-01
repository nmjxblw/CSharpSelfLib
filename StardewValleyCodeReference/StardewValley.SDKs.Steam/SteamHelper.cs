using System;
using Galaxy.Api;
using StardewValley.Menus;
using StardewValley.SDKs.GogGalaxy.Listeners;
using Steamworks;

namespace StardewValley.SDKs.Steam;

public class SteamHelper : SDKHelper
{
	private Callback<GameOverlayActivated_t> gameOverlayActivated;

	private CallResult<EncryptedAppTicketResponse_t> encryptedAppTicketResponse;

	private Callback<GamepadTextInputDismissed_t> gamepadTextInputDismissed;

	private GalaxyAuthListener galaxyAuthListener;

	private GalaxyOperationalStateChangeListener galaxyStateChangeListener;

	private GalaxySpecificUserDataListener galaxySpecificUserDataListener;

	public bool active;

	private SDKNetHelper networking;

	private TextBox _keyboardTextBox;

	protected bool _runningOnSteamDeck;

	public SDKNetHelper Networking => this.networking;

	public bool ConnectionFinished { get; private set; }

	public int ConnectionProgress { get; private set; }

	public bool GalaxyConnected { get; private set; }

	public string Name { get; } = "Steam";

	public bool HasOverlay => false;

	public bool IsJapaneseRegionRelease => false;

	public bool IsEnterButtonAssignmentFlipped => false;

	public void EarlyInitialize()
	{
	}

	public virtual bool IsRunningOnSteamDeck()
	{
		return this._runningOnSteamDeck;
	}

	public void Initialize()
	{
		try
		{
			this.active = SteamAPI.Init();
			Game1.log.Verbose("Steam logged on: " + SteamUser.BLoggedOn());
			if (this.active)
			{
				this._runningOnSteamDeck = SteamUtils.IsSteamRunningOnSteamDeck();
				Game1.log.Verbose("Initializing GalaxySDK");
				this.encryptedAppTicketResponse = CallResult<EncryptedAppTicketResponse_t>.Create(onEncryptedAppTicketResponse);
				Game1.log.Verbose("Requesting Steam app ticket");
				SteamAPICall_t handle = SteamUser.RequestEncryptedAppTicket(LegacyShims.EmptyArray<byte>(), 0);
				this.encryptedAppTicketResponse.Set(handle);
				this.ConnectionProgress++;
				SteamNetworkingUtils.InitRelayNetworkAccess();
			}
		}
		catch (Exception exception)
		{
			Game1.log.Error("Error connecting to Steam.", exception);
			this.active = false;
			this.ConnectionFinished = true;
		}
		if (this.active)
		{
			try
			{
				GalaxyInstance.Init(new InitParams("48767653913349277", "58be5c2e55d7f535cf8c4b6bbc09d185de90b152c8c42703cc13502465f0d04a", "."));
				this.galaxyAuthListener = new GalaxyAuthListener(onGalaxyAuthSuccess, onGalaxyAuthFailure, onGalaxyAuthLost);
				this.galaxyStateChangeListener = new GalaxyOperationalStateChangeListener(onGalaxyStateChange);
			}
			catch (Exception exception2)
			{
				Game1.log.Error("Error initializing the Galaxy API.", exception2);
			}
			this.gameOverlayActivated = Callback<GameOverlayActivated_t>.Create(onGameOverlayActivated);
			this.gamepadTextInputDismissed = Callback<GamepadTextInputDismissed_t>.Create(OnKeyboardDismissed);
		}
	}

	public void CancelKeyboard()
	{
		this._keyboardTextBox = null;
	}

	public void ShowKeyboard(TextBox text_box)
	{
		this._keyboardTextBox = text_box;
		SteamUtils.ShowGamepadTextInput(text_box.PasswordBox ? EGamepadTextInputMode.k_EGamepadTextInputModePassword : EGamepadTextInputMode.k_EGamepadTextInputModeNormal, EGamepadTextInputLineMode.k_EGamepadTextInputLineModeSingleLine, "", (text_box.textLimit < 0) ? 100u : ((uint)text_box.textLimit), text_box.Text);
	}

	public void OnKeyboardDismissed(GamepadTextInputDismissed_t callback)
	{
		if (this._keyboardTextBox == null)
		{
			return;
		}
		if (!callback.m_bSubmitted)
		{
			this._keyboardTextBox = null;
			return;
		}
		uint length = SteamUtils.GetEnteredGamepadTextLength();
		if (!SteamUtils.GetEnteredGamepadTextInput(out var entered_text, length))
		{
			this._keyboardTextBox = null;
			return;
		}
		this._keyboardTextBox.RecieveTextInput(entered_text);
		this._keyboardTextBox = null;
	}

	private void onSetGalaxyProfileName(GalaxyID userID)
	{
		try
		{
			if (userID != GalaxyInstance.User().GetGalaxyID())
			{
				return;
			}
		}
		catch (Exception)
		{
			return;
		}
		Game1.log.Verbose("Successfully set GOG Galaxy profile name.");
		this.galaxySpecificUserDataListener?.Dispose();
		this.galaxySpecificUserDataListener = null;
	}

	private void onGalaxyStateChange(uint operationalState)
	{
		if (this.networking != null)
		{
			return;
		}
		if ((operationalState & 1) != 0)
		{
			Game1.log.Verbose("Galaxy signed in");
			this.ConnectionProgress++;
		}
		if ((operationalState & 2) == 0)
		{
			return;
		}
		Game1.log.Verbose("Galaxy logged on");
		this.networking = new SteamNetHelper();
		this.ConnectionProgress++;
		this.ConnectionFinished = true;
		this.GalaxyConnected = true;
		try
		{
			this.galaxySpecificUserDataListener = new GalaxySpecificUserDataListener(onSetGalaxyProfileName);
			GalaxyInstance.User().SetUserData("StardewDisplayName", SteamFriends.GetPersonaName());
		}
		catch (Exception exception)
		{
			Game1.log.Error("Failed to set GOG Galaxy profile name.", exception);
		}
	}

	private void onGalaxyAuthSuccess()
	{
		Game1.log.Verbose("Galaxy auth success");
		this.ConnectionProgress++;
	}

	private void onGalaxyAuthFailure(IAuthListener.FailureReason reason)
	{
		if (this.networking == null)
		{
			this.networking = new SteamNetHelper();
		}
		Game1.log.Error("Galaxy auth failure: " + reason);
		this.ConnectionFinished = true;
		this.GalaxyConnected = false;
	}

	private void onGalaxyAuthLost()
	{
		if (this.networking == null)
		{
			this.networking = new SteamNetHelper();
		}
		Game1.log.Error("Galaxy auth lost");
		this.ConnectionFinished = true;
		this.GalaxyConnected = false;
	}

	private void onEncryptedAppTicketResponse(EncryptedAppTicketResponse_t response, bool ioFailure)
	{
		if (response.m_eResult == EResult.k_EResultOK)
		{
			byte[] ticket = new byte[1024];
			SteamUser.GetEncryptedAppTicket(ticket, 1024, out var ticketSize);
			this.ConnectionProgress++;
			Game1.log.Verbose("Signing into GalaxySDK");
			try
			{
				GalaxyInstance.User().SignInSteam(ticket, ticketSize, SteamFriends.GetPersonaName());
				return;
			}
			catch (Exception exception)
			{
				Game1.log.Error("Galaxy SignInSteam failed with an exception:", exception);
				return;
			}
		}
		Game1.log.Error("Failed to retrieve encrypted app ticket: " + response.m_eResult.ToString() + ", " + ioFailure);
		this.ConnectionFinished = true;
	}

	private void onGameOverlayActivated(GameOverlayActivated_t pCallback)
	{
		if (this.active)
		{
			if (pCallback.m_bActive != 0)
			{
				Game1.paused = !Game1.IsMultiplayer;
			}
			else
			{
				Game1.paused = false;
			}
		}
	}

	/// <inheritdoc />
	public bool RetroactiveAchievementsAllowed()
	{
		return true;
	}

	public void GetAchievement(string achieve)
	{
		if (this.active && SteamAPI.IsSteamRunning())
		{
			if (achieve.Equals("0"))
			{
				achieve = "a0";
			}
			try
			{
				SteamUserStats.SetAchievement(achieve);
				SteamUserStats.StoreStats();
			}
			catch (Exception)
			{
			}
		}
	}

	public void ResetAchievements()
	{
		if (this.active && SteamAPI.IsSteamRunning())
		{
			try
			{
				SteamUserStats.ResetAllStats(bAchievementsToo: true);
			}
			catch (Exception)
			{
			}
		}
	}

	public void Update()
	{
		if (this.active)
		{
			SteamAPI.RunCallbacks();
			try
			{
				GalaxyInstance.ProcessData();
			}
			catch (Exception)
			{
			}
		}
		Game1.game1.IsMouseVisible = Game1.paused || Game1.options.hardwareCursor;
	}

	public void Shutdown()
	{
		SteamAPI.Shutdown();
	}

	public void DebugInfo()
	{
		if (SteamAPI.IsSteamRunning())
		{
			Game1.debugOutput = (SteamUser.BLoggedOn() ? "steam is running, user logged on" : "steam is running");
			return;
		}
		Game1.debugOutput = "steam is not running";
		SteamAPI.Init();
	}

	public string FilterDirtyWords(string words)
	{
		return words;
	}
}
