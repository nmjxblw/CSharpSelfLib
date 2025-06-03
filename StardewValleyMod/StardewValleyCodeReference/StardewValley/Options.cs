using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using StardewValley.SaveSerialization;
using StardewValley.SDKs.Steam;

namespace StardewValley;

public class Options
{
	public enum ItemStowingModes
	{
		Off,
		GamepadOnly,
		Both
	}

	public enum GamepadModes
	{
		Auto,
		ForceOn,
		ForceOff
	}

	public const float minZoom = 0.75f;

	public const float maxZoom = 2f;

	public const float minUIZoom = 0.75f;

	public const float maxUIZoom = 1.5f;

	public const int toggleAutoRun = 0;

	public const int musicVolume = 1;

	public const int soundVolume = 2;

	public const int toggleDialogueTypingSounds = 3;

	public const int toggleFullscreen = 4;

	public const int screenResolution = 6;

	public const int showPortraitsToggle = 7;

	public const int showMerchantPortraitsToggle = 8;

	public const int menuBG = 9;

	public const int toggleFootsteps = 10;

	public const int alwaysShowToolHitLocationToggle = 11;

	public const int hideToolHitLocationWhenInMotionToggle = 12;

	public const int windowMode = 13;

	public const int pauseWhenUnfocused = 14;

	public const int pinToolbar = 15;

	public const int toggleRumble = 16;

	public const int ambientOnly = 17;

	public const int zoom = 18;

	public const int zoomButtonsToggle = 19;

	public const int ambientVolume = 20;

	public const int footstepVolume = 21;

	public const int invertScrollDirectionToggle = 22;

	public const int snowTransparencyToggle = 23;

	public const int screenFlashToggle = 24;

	public const int toggleHardwareCursor = 26;

	public const int toggleShowPlacementTileGamepad = 27;

	public const int stowingModeSelect = 28;

	public const int toggleSnappyMenus = 29;

	public const int toggleIPConnections = 30;

	public const int serverMode = 31;

	public const int toggleFarmhandCreation = 32;

	public const int toggleShowAdvancedCraftingInformation = 34;

	public const int toggleMPReadyStatus = 35;

	public const int mapScreenshot = 36;

	public const int toggleVsync = 37;

	public const int gamepadModeSelect = 38;

	public const int uiScaleSlider = 39;

	public const int moveBuildingPermissions = 40;

	public const int slingshotModeSelect = 41;

	public const int biteChime = 42;

	public const int toggleMuteAnimalSounds = 43;

	public const int toggleUseChineseSmoothFont = 44;

	public const int dialogueFontToggle = 45;

	public const int toggleUseAlternateFont = 46;

	public const int input_actionButton = 7;

	public const int input_cancelButton = 9;

	public const int input_useToolButton = 10;

	public const int input_moveUpButton = 11;

	public const int input_moveRightButton = 12;

	public const int input_moveDownButton = 13;

	public const int input_moveLeftButton = 14;

	public const int input_menuButton = 15;

	public const int input_runButton = 16;

	public const int input_chatButton = 17;

	public const int input_journalButton = 18;

	public const int input_mapButton = 19;

	public const int input_slot1 = 20;

	public const int input_slot2 = 21;

	public const int input_slot3 = 22;

	public const int input_slot4 = 23;

	public const int input_slot5 = 24;

	public const int input_slot6 = 25;

	public const int input_slot7 = 26;

	public const int input_slot8 = 27;

	public const int input_slot9 = 28;

	public const int input_slot10 = 29;

	public const int input_slot11 = 30;

	public const int input_slot12 = 31;

	public const int input_toolbarSwap = 32;

	public const int input_emoteButton = 33;

	public const float defaultZoomLevel = 1f;

	public const int defaultLightingQuality = 8;

	public const float defaultSplitScreenZoomLevel = 1f;

	public bool autoRun;

	public bool dialogueTyping;

	public bool showPortraits;

	public bool showMerchantPortraits;

	public bool showMenuBackground;

	public bool playFootstepSounds;

	public bool alwaysShowToolHitLocation;

	public bool hideToolHitLocationWhenInMotion;

	public bool pauseWhenOutOfFocus;

	public bool pinToolbarToggle;

	public bool mouseControls;

	public bool gamepadControls;

	public bool rumble;

	public bool ambientOnlyToggle;

	public bool zoomButtons;

	public bool invertScrollDirection;

	public bool screenFlash;

	public bool showPlacementTileForGamepad;

	public bool snappyMenus;

	public bool showAdvancedCraftingInformation;

	public bool showMPEndOfNightReadyStatus;

	public bool muteAnimalSounds;

	public bool vsyncEnabled;

	public bool fullscreen;

	public bool windowedBorderlessFullscreen;

	public bool showClearBackgrounds;

	public bool useChineseSmoothFont;

	public bool useAlternateFont;

	[DontLoadDefaultSetting]
	public bool ipConnectionsEnabled;

	[DontLoadDefaultSetting]
	public bool enableServer;

	[DontLoadDefaultSetting]
	public bool enableFarmhandCreation;

	protected bool _hardwareCursor;

	public ItemStowingModes stowingMode;

	[DontLoadDefaultSetting]
	public GamepadModes gamepadMode;

	public bool useLegacySlingshotFiring;

	public float musicVolumeLevel;

	public float soundVolumeLevel;

	public float footstepVolumeLevel;

	public float ambientVolumeLevel;

	public float snowTransparency;

	public float dialogueFontScale = 1f;

	[XmlIgnore]
	public float baseZoomLevel = 1f;

	[DontLoadDefaultSetting]
	[XmlElement("zoomLevel")]
	public float singlePlayerBaseZoomLevel = 1f;

	[DontLoadDefaultSetting]
	public float localCoopBaseZoomLevel = 1f;

	[DontLoadDefaultSetting]
	[XmlElement("uiScale")]
	public float singlePlayerDesiredUIScale = -1f;

	[DontLoadDefaultSetting]
	public float localCoopDesiredUIScale = 1.5f;

	[XmlIgnore]
	public float baseUIScale = 1f;

	public int preferredResolutionX;

	public int preferredResolutionY;

	[DontLoadDefaultSetting]
	public ServerPrivacy serverPrivacy = ServerPrivacy.FriendsOnly;

	public InputButton[] actionButton = new InputButton[2]
	{
		new InputButton(Keys.X),
		new InputButton(mouseLeft: false)
	};

	public InputButton[] cancelButton = new InputButton[1]
	{
		new InputButton(Keys.V)
	};

	public InputButton[] useToolButton = new InputButton[2]
	{
		new InputButton(Keys.C),
		new InputButton(mouseLeft: true)
	};

	public InputButton[] moveUpButton = new InputButton[1]
	{
		new InputButton(Keys.W)
	};

	public InputButton[] moveRightButton = new InputButton[1]
	{
		new InputButton(Keys.D)
	};

	public InputButton[] moveDownButton = new InputButton[1]
	{
		new InputButton(Keys.S)
	};

	public InputButton[] moveLeftButton = new InputButton[1]
	{
		new InputButton(Keys.A)
	};

	public InputButton[] menuButton = new InputButton[2]
	{
		new InputButton(Keys.E),
		new InputButton(Keys.Escape)
	};

	public InputButton[] runButton = new InputButton[1]
	{
		new InputButton(Keys.LeftShift)
	};

	public InputButton[] tmpKeyToReplace = new InputButton[1]
	{
		new InputButton(Keys.None)
	};

	public InputButton[] chatButton = new InputButton[2]
	{
		new InputButton(Keys.T),
		new InputButton(Keys.OemQuestion)
	};

	public InputButton[] mapButton = new InputButton[1]
	{
		new InputButton(Keys.M)
	};

	public InputButton[] journalButton = new InputButton[1]
	{
		new InputButton(Keys.F)
	};

	public InputButton[] inventorySlot1 = new InputButton[1]
	{
		new InputButton(Keys.D1)
	};

	public InputButton[] inventorySlot2 = new InputButton[1]
	{
		new InputButton(Keys.D2)
	};

	public InputButton[] inventorySlot3 = new InputButton[1]
	{
		new InputButton(Keys.D3)
	};

	public InputButton[] inventorySlot4 = new InputButton[1]
	{
		new InputButton(Keys.D4)
	};

	public InputButton[] inventorySlot5 = new InputButton[1]
	{
		new InputButton(Keys.D5)
	};

	public InputButton[] inventorySlot6 = new InputButton[1]
	{
		new InputButton(Keys.D6)
	};

	public InputButton[] inventorySlot7 = new InputButton[1]
	{
		new InputButton(Keys.D7)
	};

	public InputButton[] inventorySlot8 = new InputButton[1]
	{
		new InputButton(Keys.D8)
	};

	public InputButton[] inventorySlot9 = new InputButton[1]
	{
		new InputButton(Keys.D9)
	};

	public InputButton[] inventorySlot10 = new InputButton[1]
	{
		new InputButton(Keys.D0)
	};

	public InputButton[] inventorySlot11 = new InputButton[1]
	{
		new InputButton(Keys.OemMinus)
	};

	public InputButton[] inventorySlot12 = new InputButton[1]
	{
		new InputButton(Keys.OemPlus)
	};

	public InputButton[] toolbarSwap = new InputButton[1]
	{
		new InputButton(Keys.Tab)
	};

	public InputButton[] emoteButton = new InputButton[1]
	{
		new InputButton(Keys.Y)
	};

	[XmlIgnore]
	public bool optionsDirty;

	[XmlIgnore]
	private XmlSerializer defaultSettingsSerializer = SaveSerializer.GetSerializer(typeof(Options));

	private int appliedLightingQuality = -1;

	public bool hardwareCursor
	{
		get
		{
			if (LocalMultiplayer.IsLocalMultiplayer())
			{
				return false;
			}
			return this._hardwareCursor;
		}
		set
		{
			this._hardwareCursor = value;
		}
	}

	public int lightingQuality => 8;

	[XmlIgnore]
	public float zoomLevel
	{
		get
		{
			if (Game1.game1.takingMapScreenshot)
			{
				return this.baseZoomLevel;
			}
			return this.baseZoomLevel * Game1.game1.zoomModifier;
		}
	}

	[XmlIgnore]
	public float desiredBaseZoomLevel
	{
		get
		{
			if (LocalMultiplayer.IsLocalMultiplayer() || !Game1.game1.IsMainInstance)
			{
				return this.localCoopBaseZoomLevel;
			}
			return this.singlePlayerBaseZoomLevel;
		}
		set
		{
			if (LocalMultiplayer.IsLocalMultiplayer() || !Game1.game1.IsMainInstance)
			{
				this.localCoopBaseZoomLevel = value;
			}
			else
			{
				this.singlePlayerBaseZoomLevel = value;
			}
		}
	}

	[XmlIgnore]
	public float desiredUIScale
	{
		get
		{
			if (Game1.gameMode != 3)
			{
				return 1f;
			}
			if (LocalMultiplayer.IsLocalMultiplayer() || !Game1.game1.IsMainInstance)
			{
				return this.localCoopDesiredUIScale;
			}
			return this.singlePlayerDesiredUIScale;
		}
		set
		{
			if (Game1.gameMode == 3)
			{
				if (LocalMultiplayer.IsLocalMultiplayer() || !Game1.game1.IsMainInstance)
				{
					this.localCoopDesiredUIScale = value;
				}
				else
				{
					this.singlePlayerDesiredUIScale = value;
				}
			}
		}
	}

	[XmlIgnore]
	public float uiScale => this.baseUIScale * Game1.game1.zoomModifier;

	public bool allowStowing
	{
		get
		{
			switch (this.stowingMode)
			{
			case ItemStowingModes.Off:
				return false;
			case ItemStowingModes.GamepadOnly:
				if (this.gamepadControls)
				{
					if (Program.sdk is SteamHelper steamHelper && steamHelper.IsRunningOnSteamDeck() && Game1.input.GetMouseState().LeftButton == ButtonState.Pressed)
					{
						return false;
					}
					return true;
				}
				return false;
			default:
				return true;
			}
		}
	}

	public bool SnappyMenus
	{
		get
		{
			if (this.snappyMenus && this.gamepadControls && Game1.input.GetMouseState().LeftButton != ButtonState.Pressed)
			{
				return Game1.input.GetMouseState().RightButton != ButtonState.Pressed;
			}
			return false;
		}
	}

	public Options()
	{
		this.setToDefaults();
	}

	/// <summary>Get the absolute file path for the <c>default_options</c> file.</summary>
	public string GetFilePathForDefaultOptions()
	{
		return Path.Combine(Program.GetAppDataFolder(), "default_options");
	}

	public virtual void LoadDefaultOptions()
	{
		if (!Game1.game1.IsMainInstance)
		{
			return;
		}
		Options default_options = null;
		string filePath = this.GetFilePathForDefaultOptions();
		try
		{
			using FileStream stream = File.Open(filePath, FileMode.Open);
			default_options = this.defaultSettingsSerializer.DeserializeFast(stream) as Options;
		}
		catch (Exception)
		{
		}
		if (default_options == null)
		{
			return;
		}
		Type type = typeof(Options);
		FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public);
		foreach (FieldInfo field in fields)
		{
			if (field.GetCustomAttribute<DontLoadDefaultSetting>() == null && field.GetCustomAttribute<XmlIgnoreAttribute>() == null)
			{
				field.SetValue(this, field.GetValue(default_options));
			}
		}
		PropertyInfo[] properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
		foreach (PropertyInfo property_info in properties)
		{
			if (property_info.GetCustomAttribute<DontLoadDefaultSetting>() == null && property_info.GetCustomAttribute<XmlIgnoreAttribute>() == null && property_info.GetSetMethod() != null && property_info.GetGetMethod() != null)
			{
				property_info.SetValue(this, property_info.GetValue(default_options, null), null);
			}
		}
	}

	public virtual void SaveDefaultOptions()
	{
		this.optionsDirty = false;
		if (!Game1.game1.IsMainInstance)
		{
			return;
		}
		string filePath = this.GetFilePathForDefaultOptions();
		XmlWriterSettings settings = new XmlWriterSettings();
		try
		{
			using FileStream stream = File.Open(filePath, FileMode.Create);
			using XmlWriter writer = XmlWriter.Create(stream, settings);
			writer.WriteStartDocument();
			this.defaultSettingsSerializer.SerializeFast(writer, Game1.options);
			writer.WriteEndDocument();
			writer.Flush();
		}
		catch (Exception)
		{
		}
	}

	public void platformClampValues()
	{
	}

	public Keys getFirstKeyboardKeyFromInputButtonList(InputButton[] inputButton)
	{
		for (int i = 0; i < inputButton.Length; i++)
		{
			if (inputButton[i].key != Keys.None)
			{
				return inputButton[i].key;
			}
		}
		return Keys.None;
	}

	public void reApplySetOptions()
	{
		this.platformClampValues();
		if (this.lightingQuality != this.appliedLightingQuality)
		{
			Program.gamePtr.refreshWindowSettings();
			this.appliedLightingQuality = this.lightingQuality;
		}
		Program.gamePtr.IsMouseVisible = this.hardwareCursor;
	}

	public void setToDefaults()
	{
		this.playFootstepSounds = true;
		this.showMenuBackground = false;
		this.showClearBackgrounds = false;
		this.showMerchantPortraits = true;
		this.showPortraits = true;
		this.autoRun = true;
		this.alwaysShowToolHitLocation = false;
		this.hideToolHitLocationWhenInMotion = true;
		this.dialogueTyping = true;
		this.rumble = true;
		this.fullscreen = false;
		this.pinToolbarToggle = false;
		this.baseZoomLevel = 1f;
		this.localCoopBaseZoomLevel = 1f;
		if (Game1.options == this)
		{
			Game1.forceSnapOnNextViewportUpdate = true;
		}
		this.zoomButtons = false;
		this.pauseWhenOutOfFocus = true;
		this.screenFlash = true;
		this.snowTransparency = 1f;
		this.invertScrollDirection = false;
		this.ambientOnlyToggle = false;
		this.showAdvancedCraftingInformation = false;
		this.stowingMode = ItemStowingModes.Off;
		this.useLegacySlingshotFiring = false;
		this.gamepadMode = GamepadModes.Auto;
		this.windowedBorderlessFullscreen = true;
		this.showPlacementTileForGamepad = true;
		this.hardwareCursor = false;
		this.musicVolumeLevel = 0.75f;
		this.ambientVolumeLevel = 0.75f;
		this.footstepVolumeLevel = 0.9f;
		this.soundVolumeLevel = 1f;
		this.dialogueFontScale = 1f;
		DisplayMode displayMode = Game1.graphics.GraphicsDevice.Adapter.SupportedDisplayModes.Last();
		this.preferredResolutionX = displayMode.Width;
		this.preferredResolutionY = displayMode.Height;
		this.vsyncEnabled = true;
		GameRunner.instance.OnWindowSizeChange(null, null);
		this.snappyMenus = true;
		this.ipConnectionsEnabled = true;
		this.enableServer = true;
		this.serverPrivacy = ServerPrivacy.FriendsOnly;
		this.enableFarmhandCreation = true;
		this.showMPEndOfNightReadyStatus = false;
		this.muteAnimalSounds = false;
		this.useChineseSmoothFont = false;
		this.useAlternateFont = false;
	}

	public void setControlsToDefault()
	{
		this.actionButton = new InputButton[2]
		{
			new InputButton(Keys.X),
			new InputButton(mouseLeft: false)
		};
		this.cancelButton = new InputButton[1]
		{
			new InputButton(Keys.V)
		};
		this.useToolButton = new InputButton[2]
		{
			new InputButton(Keys.C),
			new InputButton(mouseLeft: true)
		};
		this.moveUpButton = new InputButton[1]
		{
			new InputButton(Keys.W)
		};
		this.moveRightButton = new InputButton[1]
		{
			new InputButton(Keys.D)
		};
		this.moveDownButton = new InputButton[1]
		{
			new InputButton(Keys.S)
		};
		this.moveLeftButton = new InputButton[1]
		{
			new InputButton(Keys.A)
		};
		this.menuButton = new InputButton[2]
		{
			new InputButton(Keys.E),
			new InputButton(Keys.Escape)
		};
		this.runButton = new InputButton[1]
		{
			new InputButton(Keys.LeftShift)
		};
		this.tmpKeyToReplace = new InputButton[1]
		{
			new InputButton(Keys.None)
		};
		this.chatButton = new InputButton[2]
		{
			new InputButton(Keys.T),
			new InputButton(Keys.OemQuestion)
		};
		this.mapButton = new InputButton[1]
		{
			new InputButton(Keys.M)
		};
		this.journalButton = new InputButton[1]
		{
			new InputButton(Keys.F)
		};
		this.inventorySlot1 = new InputButton[1]
		{
			new InputButton(Keys.D1)
		};
		this.inventorySlot2 = new InputButton[1]
		{
			new InputButton(Keys.D2)
		};
		this.inventorySlot3 = new InputButton[1]
		{
			new InputButton(Keys.D3)
		};
		this.inventorySlot4 = new InputButton[1]
		{
			new InputButton(Keys.D4)
		};
		this.inventorySlot5 = new InputButton[1]
		{
			new InputButton(Keys.D5)
		};
		this.inventorySlot6 = new InputButton[1]
		{
			new InputButton(Keys.D6)
		};
		this.inventorySlot7 = new InputButton[1]
		{
			new InputButton(Keys.D7)
		};
		this.inventorySlot8 = new InputButton[1]
		{
			new InputButton(Keys.D8)
		};
		this.inventorySlot9 = new InputButton[1]
		{
			new InputButton(Keys.D9)
		};
		this.inventorySlot10 = new InputButton[1]
		{
			new InputButton(Keys.D0)
		};
		this.inventorySlot11 = new InputButton[1]
		{
			new InputButton(Keys.OemMinus)
		};
		this.inventorySlot12 = new InputButton[1]
		{
			new InputButton(Keys.OemPlus)
		};
		this.emoteButton = new InputButton[1]
		{
			new InputButton(Keys.Y)
		};
		this.toolbarSwap = new InputButton[1]
		{
			new InputButton(Keys.Tab)
		};
	}

	public string getNameOfOptionFromIndex(int index)
	{
		return index switch
		{
			0 => Game1.content.LoadString("Strings\\StringsFromCSFiles:Options.cs.4556"), 
			1 => Game1.content.LoadString("Strings\\StringsFromCSFiles:Options.cs.4557"), 
			2 => Game1.content.LoadString("Strings\\StringsFromCSFiles:Options.cs.4558"), 
			3 => Game1.content.LoadString("Strings\\StringsFromCSFiles:Options.cs.4559"), 
			4 => Game1.content.LoadString("Strings\\StringsFromCSFiles:Options.cs.4560"), 
			5 => Game1.content.LoadString("Strings\\StringsFromCSFiles:Options.cs.4561"), 
			6 => Game1.content.LoadString("Strings\\StringsFromCSFiles:Options.cs.4562"), 
			_ => "", 
		};
	}

	public void changeCheckBoxOption(int which, bool value)
	{
		switch (which)
		{
		case 0:
			this.autoRun = value;
			Game1.player.setRunning(this.autoRun);
			break;
		case 3:
			this.dialogueTyping = value;
			break;
		case 7:
			this.showPortraits = value;
			break;
		case 8:
			this.showMerchantPortraits = value;
			break;
		case 9:
			this.showMenuBackground = value;
			break;
		case 10:
			this.playFootstepSounds = value;
			break;
		case 11:
			this.alwaysShowToolHitLocation = value;
			break;
		case 12:
			this.hideToolHitLocationWhenInMotion = value;
			break;
		case 14:
			this.pauseWhenOutOfFocus = value;
			break;
		case 15:
			this.pinToolbarToggle = value;
			break;
		case 16:
			this.rumble = value;
			break;
		case 17:
			this.ambientOnlyToggle = value;
			break;
		case 19:
			this.zoomButtons = value;
			break;
		case 22:
			this.invertScrollDirection = value;
			break;
		case 24:
			this.screenFlash = value;
			break;
		case 26:
			this.hardwareCursor = value;
			Program.gamePtr.IsMouseVisible = this.hardwareCursor;
			break;
		case 27:
			this.showPlacementTileForGamepad = value;
			break;
		case 37:
			this.vsyncEnabled = value;
			GameRunner.instance.OnWindowSizeChange(null, null);
			break;
		case 29:
			this.snappyMenus = value;
			break;
		case 30:
			this.ipConnectionsEnabled = value;
			break;
		case 32:
			this.enableFarmhandCreation = value;
			Game1.server?.updateLobbyData();
			break;
		case 34:
			this.showAdvancedCraftingInformation = value;
			break;
		case 35:
			this.showMPEndOfNightReadyStatus = value;
			break;
		case 43:
			this.muteAnimalSounds = value;
			break;
		case 44:
			this.useChineseSmoothFont = value;
			this.loadChineseFonts();
			break;
		case 46:
			this.useAlternateFont = value;
			break;
		}
		this.optionsDirty = true;
	}

	public void changeSliderOption(int which, int value)
	{
		switch (which)
		{
		case 45:
			this.dialogueFontScale = (float)value / 100f + 1f;
			break;
		case 1:
			this.musicVolumeLevel = (float)value / 100f;
			Game1.musicCategory.SetVolume(this.musicVolumeLevel);
			Game1.musicPlayerVolume = this.musicVolumeLevel;
			break;
		case 2:
			this.soundVolumeLevel = (float)value / 100f;
			Game1.soundCategory.SetVolume(this.soundVolumeLevel);
			break;
		case 20:
			this.ambientVolumeLevel = (float)value / 100f;
			Game1.ambientCategory.SetVolume(this.ambientVolumeLevel);
			Game1.ambientPlayerVolume = this.ambientVolumeLevel;
			break;
		case 21:
			this.footstepVolumeLevel = (float)value / 100f;
			Game1.footstepCategory.SetVolume(this.footstepVolumeLevel);
			break;
		case 23:
			this.snowTransparency = (float)value / 100f;
			break;
		case 39:
		{
			int zoomlvl2 = (int)(this.desiredUIScale * 100f);
			int newValue2 = (int)((float)value * 100f);
			if (newValue2 >= zoomlvl2 + 10 || newValue2 >= 100)
			{
				zoomlvl2 += 10;
				zoomlvl2 = Math.Min(100, zoomlvl2);
			}
			else if (newValue2 <= zoomlvl2 - 10 || newValue2 <= 50)
			{
				zoomlvl2 -= 10;
				zoomlvl2 = Math.Max(50, zoomlvl2);
			}
			this.desiredUIScale = (float)zoomlvl2 / 100f;
			break;
		}
		case 18:
		{
			int zoomlvl = (int)(this.desiredBaseZoomLevel * 100f);
			int oldZoom = zoomlvl;
			int newValue = (int)((float)value * 100f);
			if (newValue >= zoomlvl + 10 || newValue >= 100)
			{
				zoomlvl += 10;
				zoomlvl = Math.Min(100, zoomlvl);
			}
			else if (newValue <= zoomlvl - 10 || newValue <= 50)
			{
				zoomlvl -= 10;
				zoomlvl = Math.Max(50, zoomlvl);
			}
			if (zoomlvl != oldZoom)
			{
				this.desiredBaseZoomLevel = (float)zoomlvl / 100f;
				Game1.forceSnapOnNextViewportUpdate = true;
				Game1.showGlobalMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Options.cs.4563") + this.zoomLevel);
			}
			break;
		}
		}
		this.optionsDirty = true;
	}

	public void loadChineseFonts()
	{
		if (this.useChineseSmoothFont)
		{
			Game1.smallFont = Game1.content.Load<SpriteFont>("Fonts\\Chinese_round\\SmallFont");
			Game1.dialogueFont = Game1.content.Load<SpriteFont>("Fonts\\Chinese_round\\SpriteFont1");
			SpriteText.LoadFontData(LocalizedContentManager.LanguageCode.zh);
		}
		else
		{
			Game1.smallFont = Game1.content.Load<SpriteFont>("Fonts\\SmallFont");
			Game1.dialogueFont = Game1.content.Load<SpriteFont>("Fonts\\SpriteFont1");
			SpriteText.LoadFontData(LocalizedContentManager.LanguageCode.zh);
		}
	}

	public void setBackgroundMode(string setting)
	{
		switch (setting)
		{
		case "Standard":
			this.showMenuBackground = false;
			this.showClearBackgrounds = false;
			break;
		case "Graphical":
			this.showMenuBackground = true;
			break;
		case "None":
			this.showClearBackgrounds = true;
			this.showMenuBackground = false;
			break;
		}
	}

	public void setStowingMode(string setting)
	{
		switch (setting)
		{
		case "off":
			this.stowingMode = ItemStowingModes.Off;
			break;
		case "gamepad":
			this.stowingMode = ItemStowingModes.GamepadOnly;
			break;
		case "both":
			this.stowingMode = ItemStowingModes.Both;
			break;
		}
	}

	public void setSlingshotMode(string setting)
	{
		if (setting == "legacy")
		{
			this.useLegacySlingshotFiring = true;
		}
		else
		{
			this.useLegacySlingshotFiring = false;
		}
	}

	public void setBiteChime(string setting)
	{
		try
		{
			Game1.player.biteChime.Value = int.Parse(setting);
		}
		catch (Exception)
		{
			Game1.player.biteChime.Value = -1;
		}
	}

	public void setGamepadMode(string setting)
	{
		switch (setting)
		{
		case "auto":
			this.gamepadMode = GamepadModes.Auto;
			break;
		case "force_on":
			this.gamepadMode = GamepadModes.ForceOn;
			break;
		case "force_off":
			this.gamepadMode = GamepadModes.ForceOff;
			break;
		}
		try
		{
			StartupPreferences startupPreferences = new StartupPreferences();
			startupPreferences.loadPreferences(async: false, applyLanguage: false);
			startupPreferences.gamepadMode = this.gamepadMode;
			startupPreferences.savePreferences(async: false);
		}
		catch (Exception)
		{
		}
	}

	public void setMoveBuildingPermissions(string setting)
	{
		switch (setting)
		{
		case "off":
			Game1.player.team.farmhandsCanMoveBuildings.Value = FarmerTeam.RemoteBuildingPermissions.Off;
			break;
		case "on":
			Game1.player.team.farmhandsCanMoveBuildings.Value = FarmerTeam.RemoteBuildingPermissions.On;
			break;
		case "owned":
			Game1.player.team.farmhandsCanMoveBuildings.Value = FarmerTeam.RemoteBuildingPermissions.OwnedBuildings;
			break;
		}
	}

	public void setServerMode(string setting)
	{
		switch (setting)
		{
		case "offline":
			this.enableServer = false;
			Game1.multiplayer.Disconnect(Multiplayer.DisconnectType.ServerOfflineMode);
			return;
		case "friends":
			this.serverPrivacy = ServerPrivacy.FriendsOnly;
			break;
		case "invite":
			this.serverPrivacy = ServerPrivacy.InviteOnly;
			break;
		}
		if (Game1.server == null && Game1.client == null)
		{
			this.enableServer = true;
			Game1.multiplayer.StartServer();
		}
		else if (Game1.server != null)
		{
			this.enableServer = true;
			Game1.server.setPrivacy(this.serverPrivacy);
		}
	}

	public void setWindowedOption(string setting)
	{
		switch (setting)
		{
		case "Windowed":
			this.setWindowedOption(1);
			break;
		case "Fullscreen":
			this.setWindowedOption(2);
			break;
		case "Windowed Borderless":
			this.setWindowedOption(0);
			break;
		}
	}

	public void setWindowedOption(int setting)
	{
		this.windowedBorderlessFullscreen = this.isCurrentlyWindowedBorderless();
		this.fullscreen = !this.windowedBorderlessFullscreen && Game1.graphics.IsFullScreen;
		int whichMode = -1;
		switch (setting)
		{
		case 1:
			if (Game1.graphics.IsFullScreen && !this.windowedBorderlessFullscreen)
			{
				this.fullscreen = false;
				Game1.toggleNonBorderlessWindowedFullscreen();
				this.windowedBorderlessFullscreen = false;
			}
			else if (this.windowedBorderlessFullscreen)
			{
				this.fullscreen = false;
				this.windowedBorderlessFullscreen = false;
				Game1.toggleFullscreen();
			}
			whichMode = 1;
			break;
		case 2:
			if (this.windowedBorderlessFullscreen)
			{
				this.fullscreen = true;
				this.windowedBorderlessFullscreen = false;
				Game1.toggleFullscreen();
			}
			else if (!Game1.graphics.IsFullScreen)
			{
				this.fullscreen = true;
				this.windowedBorderlessFullscreen = false;
				Game1.toggleNonBorderlessWindowedFullscreen();
				this.hardwareCursor = false;
				Program.gamePtr.IsMouseVisible = false;
			}
			whichMode = 2;
			break;
		case 0:
			if (!this.windowedBorderlessFullscreen)
			{
				this.windowedBorderlessFullscreen = true;
				Game1.toggleFullscreen();
				this.fullscreen = false;
			}
			whichMode = 0;
			break;
		}
		try
		{
			StartupPreferences startupPreferences = new StartupPreferences();
			startupPreferences.loadPreferences(async: false, applyLanguage: false);
			startupPreferences.windowMode = whichMode;
			startupPreferences.fullscreenResolutionX = this.preferredResolutionX;
			startupPreferences.fullscreenResolutionY = this.preferredResolutionY;
			startupPreferences.displayIndex = GameRunner.instance.Window.GetDisplayIndex();
			startupPreferences.savePreferences(async: false);
		}
		catch (Exception)
		{
		}
	}

	public void changeDropDownOption(int which, string value)
	{
		switch (which)
		{
		case 9:
			this.setBackgroundMode(value);
			break;
		case 39:
		{
			int newZoom = Convert.ToInt32(value.Replace("%", ""));
			this.desiredUIScale = (float)newZoom / 100f;
			break;
		}
		case 18:
		{
			int newZoom2 = Convert.ToInt32(value.Replace("%", ""));
			this.desiredBaseZoomLevel = (float)newZoom2 / 100f;
			Game1.forceSnapOnNextViewportUpdate = true;
			if (Game1.debrisWeather != null)
			{
				Game1.randomizeDebrisWeatherPositions(Game1.debrisWeather);
			}
			Game1.randomizeRainPositions();
			break;
		}
		case 6:
		{
			string[] array = ArgUtility.SplitBySpace(value);
			int width = Convert.ToInt32(array[0]);
			int height = Convert.ToInt32(array[2]);
			this.preferredResolutionX = width;
			this.preferredResolutionY = height;
			Game1.graphics.PreferredBackBufferWidth = width;
			Game1.graphics.PreferredBackBufferHeight = height;
			if (!this.isCurrentlyWindowed())
			{
				try
				{
					StartupPreferences startupPreferences = new StartupPreferences();
					startupPreferences.loadPreferences(async: false, applyLanguage: false);
					startupPreferences.fullscreenResolutionX = this.preferredResolutionX;
					startupPreferences.fullscreenResolutionY = this.preferredResolutionY;
					startupPreferences.savePreferences(async: false);
				}
				catch (Exception)
				{
				}
			}
			Game1.graphics.ApplyChanges();
			GameRunner.instance.OnWindowSizeChange(null, null);
			break;
		}
		case 13:
			this.setWindowedOption(value);
			break;
		case 31:
			this.setServerMode(value);
			break;
		case 28:
			this.setStowingMode(value);
			break;
		case 38:
			this.setGamepadMode(value);
			break;
		case 40:
			this.setMoveBuildingPermissions(value);
			break;
		case 41:
			this.setSlingshotMode(value);
			break;
		case 42:
			this.setBiteChime(value);
			Game1.player.PlayFishBiteChime();
			break;
		}
		this.optionsDirty = true;
	}

	public bool isKeyInUse(Keys key)
	{
		foreach (InputButton allUsedInputButton in this.getAllUsedInputButtons())
		{
			if (allUsedInputButton.key == key)
			{
				return true;
			}
		}
		return false;
	}

	public List<InputButton> getAllUsedInputButtons()
	{
		List<InputButton> list = new List<InputButton>();
		list.AddRange(this.useToolButton);
		list.AddRange(this.actionButton);
		list.AddRange(this.moveUpButton);
		list.AddRange(this.moveRightButton);
		list.AddRange(this.moveDownButton);
		list.AddRange(this.moveLeftButton);
		list.AddRange(this.runButton);
		list.AddRange(this.menuButton);
		list.AddRange(this.journalButton);
		list.AddRange(this.mapButton);
		list.AddRange(this.chatButton);
		list.AddRange(this.inventorySlot1);
		list.AddRange(this.inventorySlot2);
		list.AddRange(this.inventorySlot3);
		list.AddRange(this.inventorySlot4);
		list.AddRange(this.inventorySlot5);
		list.AddRange(this.inventorySlot6);
		list.AddRange(this.inventorySlot7);
		list.AddRange(this.inventorySlot8);
		list.AddRange(this.inventorySlot9);
		list.AddRange(this.inventorySlot10);
		list.AddRange(this.inventorySlot11);
		list.AddRange(this.inventorySlot12);
		list.AddRange(this.toolbarSwap);
		list.AddRange(this.emoteButton);
		return list;
	}

	public void setCheckBoxToProperValue(OptionsCheckbox checkbox)
	{
		switch (checkbox.whichOption)
		{
		case 0:
			checkbox.isChecked = this.autoRun;
			break;
		case 3:
			checkbox.isChecked = this.dialogueTyping;
			break;
		case 4:
			this.fullscreen = Game1.graphics.IsFullScreen || this.windowedBorderlessFullscreen;
			checkbox.isChecked = this.fullscreen;
			break;
		case 5:
			checkbox.isChecked = this.windowedBorderlessFullscreen;
			checkbox.greyedOut = !this.fullscreen;
			break;
		case 7:
			checkbox.isChecked = this.showPortraits;
			break;
		case 8:
			checkbox.isChecked = this.showMerchantPortraits;
			break;
		case 9:
			checkbox.isChecked = this.showMenuBackground;
			break;
		case 10:
			checkbox.isChecked = this.playFootstepSounds;
			break;
		case 11:
			checkbox.isChecked = this.alwaysShowToolHitLocation;
			break;
		case 12:
			checkbox.isChecked = this.hideToolHitLocationWhenInMotion;
			break;
		case 14:
			checkbox.isChecked = this.pauseWhenOutOfFocus;
			break;
		case 15:
			checkbox.isChecked = this.pinToolbarToggle;
			break;
		case 16:
			checkbox.isChecked = this.rumble;
			checkbox.greyedOut = !this.gamepadControls;
			break;
		case 17:
			checkbox.isChecked = this.ambientOnlyToggle;
			break;
		case 19:
			checkbox.isChecked = this.zoomButtons;
			break;
		case 22:
			checkbox.isChecked = this.invertScrollDirection;
			break;
		case 24:
			checkbox.isChecked = this.screenFlash;
			break;
		case 26:
			checkbox.isChecked = this._hardwareCursor;
			checkbox.greyedOut = this.fullscreen;
			break;
		case 27:
			checkbox.isChecked = this.showPlacementTileForGamepad;
			checkbox.greyedOut = !this.gamepadControls;
			break;
		case 29:
			checkbox.isChecked = this.snappyMenus;
			break;
		case 30:
			checkbox.isChecked = this.ipConnectionsEnabled;
			break;
		case 32:
			checkbox.isChecked = this.enableFarmhandCreation;
			break;
		case 34:
			checkbox.isChecked = this.showAdvancedCraftingInformation;
			break;
		case 35:
			checkbox.isChecked = this.showMPEndOfNightReadyStatus;
			break;
		case 37:
			checkbox.isChecked = this.vsyncEnabled;
			break;
		case 43:
			checkbox.isChecked = this.muteAnimalSounds;
			break;
		case 44:
			checkbox.isChecked = this.useChineseSmoothFont;
			break;
		case 46:
			checkbox.isChecked = this.useAlternateFont;
			break;
		case 1:
		case 2:
		case 6:
		case 13:
		case 18:
		case 20:
		case 21:
		case 23:
		case 25:
		case 28:
		case 31:
		case 33:
		case 36:
		case 38:
		case 39:
		case 40:
		case 41:
		case 42:
		case 45:
			break;
		}
	}

	public void setPlusMinusToProperValue(OptionsPlusMinus plusMinus)
	{
		switch (plusMinus.whichOption)
		{
		case 39:
		{
			string currentZoom2 = Math.Round(this.desiredUIScale * 100f) + "%";
			for (int j = 0; j < plusMinus.options.Count; j++)
			{
				if (plusMinus.options[j].Equals(currentZoom2))
				{
					plusMinus.selected = j;
					break;
				}
			}
			break;
		}
		case 18:
		{
			string currentZoom = Math.Round(this.desiredBaseZoomLevel * 100f) + "%";
			for (int i = 0; i < plusMinus.options.Count; i++)
			{
				if (plusMinus.options[i].Equals(currentZoom))
				{
					plusMinus.selected = i;
					break;
				}
			}
			break;
		}
		}
	}

	public void setSliderToProperValue(OptionsSlider slider)
	{
		switch (slider.whichOption)
		{
		case 45:
			slider.value = (int)((this.dialogueFontScale - 1f) * 100f);
			break;
		case 1:
			slider.value = (int)(this.musicVolumeLevel * 100f);
			break;
		case 2:
			slider.value = (int)(this.soundVolumeLevel * 100f);
			break;
		case 20:
			slider.value = (int)(this.ambientVolumeLevel * 100f);
			break;
		case 21:
			slider.value = (int)(this.footstepVolumeLevel * 100f);
			break;
		case 23:
			slider.value = (int)(this.snowTransparency * 100f);
			break;
		case 18:
			slider.value = (int)(this.desiredBaseZoomLevel * 100f);
			break;
		case 39:
			slider.value = (int)(this.desiredUIScale * 100f);
			break;
		}
	}

	public bool doesInputListContain(InputButton[] list, Keys key)
	{
		for (int i = 0; i < list.Length; i++)
		{
			if (list[i].key == key)
			{
				return true;
			}
		}
		return false;
	}

	public void changeInputListenerValue(int whichListener, Keys key)
	{
		switch (whichListener)
		{
		case 7:
			this.actionButton[0] = new InputButton(key);
			break;
		case 17:
			this.chatButton[0] = new InputButton(key);
			break;
		case 15:
			this.menuButton[0] = new InputButton(key);
			break;
		case 13:
			this.moveDownButton[0] = new InputButton(key);
			break;
		case 14:
			this.moveLeftButton[0] = new InputButton(key);
			break;
		case 12:
			this.moveRightButton[0] = new InputButton(key);
			break;
		case 11:
			this.moveUpButton[0] = new InputButton(key);
			break;
		case 16:
			this.runButton[0] = new InputButton(key);
			break;
		case 10:
			this.useToolButton[0] = new InputButton(key);
			break;
		case 18:
			this.journalButton[0] = new InputButton(key);
			break;
		case 19:
			this.mapButton[0] = new InputButton(key);
			break;
		case 20:
			this.inventorySlot1[0] = new InputButton(key);
			break;
		case 21:
			this.inventorySlot2[0] = new InputButton(key);
			break;
		case 22:
			this.inventorySlot3[0] = new InputButton(key);
			break;
		case 23:
			this.inventorySlot4[0] = new InputButton(key);
			break;
		case 24:
			this.inventorySlot5[0] = new InputButton(key);
			break;
		case 25:
			this.inventorySlot6[0] = new InputButton(key);
			break;
		case 26:
			this.inventorySlot7[0] = new InputButton(key);
			break;
		case 27:
			this.inventorySlot8[0] = new InputButton(key);
			break;
		case 28:
			this.inventorySlot9[0] = new InputButton(key);
			break;
		case 29:
			this.inventorySlot10[0] = new InputButton(key);
			break;
		case 30:
			this.inventorySlot11[0] = new InputButton(key);
			break;
		case 31:
			this.inventorySlot12[0] = new InputButton(key);
			break;
		case 32:
			this.toolbarSwap[0] = new InputButton(key);
			break;
		case 33:
			this.emoteButton[0] = new InputButton(key);
			break;
		}
		this.optionsDirty = true;
	}

	public void setInputListenerToProperValue(OptionsInputListener inputListener)
	{
		inputListener.buttonNames.Clear();
		switch (inputListener.whichOption)
		{
		case 7:
		{
			InputButton[] array = this.actionButton;
			for (int i = 0; i < array.Length; i++)
			{
				InputButton b18 = array[i];
				inputListener.buttonNames.Add(b18.ToString());
			}
			break;
		}
		case 17:
		{
			InputButton[] array = this.chatButton;
			for (int i = 0; i < array.Length; i++)
			{
				InputButton b2 = array[i];
				inputListener.buttonNames.Add(b2.ToString());
			}
			break;
		}
		case 15:
		{
			InputButton[] array = this.menuButton;
			for (int i = 0; i < array.Length; i++)
			{
				InputButton b10 = array[i];
				inputListener.buttonNames.Add(b10.ToString());
			}
			break;
		}
		case 13:
		{
			InputButton[] array = this.moveDownButton;
			for (int i = 0; i < array.Length; i++)
			{
				InputButton b22 = array[i];
				inputListener.buttonNames.Add(b22.ToString());
			}
			break;
		}
		case 14:
		{
			InputButton[] array = this.moveLeftButton;
			for (int i = 0; i < array.Length; i++)
			{
				InputButton b14 = array[i];
				inputListener.buttonNames.Add(b14.ToString());
			}
			break;
		}
		case 12:
		{
			InputButton[] array = this.moveRightButton;
			for (int i = 0; i < array.Length; i++)
			{
				InputButton b6 = array[i];
				inputListener.buttonNames.Add(b6.ToString());
			}
			break;
		}
		case 11:
		{
			InputButton[] array = this.moveUpButton;
			for (int i = 0; i < array.Length; i++)
			{
				InputButton b24 = array[i];
				inputListener.buttonNames.Add(b24.ToString());
			}
			break;
		}
		case 16:
		{
			InputButton[] array = this.runButton;
			for (int i = 0; i < array.Length; i++)
			{
				InputButton b20 = array[i];
				inputListener.buttonNames.Add(b20.ToString());
			}
			break;
		}
		case 10:
		{
			InputButton[] array = this.useToolButton;
			for (int i = 0; i < array.Length; i++)
			{
				InputButton b16 = array[i];
				inputListener.buttonNames.Add(b16.ToString());
			}
			break;
		}
		case 32:
		{
			InputButton[] array = this.toolbarSwap;
			for (int i = 0; i < array.Length; i++)
			{
				InputButton b12 = array[i];
				inputListener.buttonNames.Add(b12.ToString());
			}
			break;
		}
		case 18:
		{
			InputButton[] array = this.journalButton;
			for (int i = 0; i < array.Length; i++)
			{
				InputButton b8 = array[i];
				inputListener.buttonNames.Add(b8.ToString());
			}
			break;
		}
		case 19:
		{
			InputButton[] array = this.mapButton;
			for (int i = 0; i < array.Length; i++)
			{
				InputButton b4 = array[i];
				inputListener.buttonNames.Add(b4.ToString());
			}
			break;
		}
		case 20:
		{
			InputButton[] array = this.inventorySlot1;
			for (int i = 0; i < array.Length; i++)
			{
				InputButton b25 = array[i];
				inputListener.buttonNames.Add(b25.ToString());
			}
			break;
		}
		case 21:
		{
			InputButton[] array = this.inventorySlot2;
			for (int i = 0; i < array.Length; i++)
			{
				InputButton b23 = array[i];
				inputListener.buttonNames.Add(b23.ToString());
			}
			break;
		}
		case 22:
		{
			InputButton[] array = this.inventorySlot3;
			for (int i = 0; i < array.Length; i++)
			{
				InputButton b21 = array[i];
				inputListener.buttonNames.Add(b21.ToString());
			}
			break;
		}
		case 23:
		{
			InputButton[] array = this.inventorySlot4;
			for (int i = 0; i < array.Length; i++)
			{
				InputButton b19 = array[i];
				inputListener.buttonNames.Add(b19.ToString());
			}
			break;
		}
		case 24:
		{
			InputButton[] array = this.inventorySlot5;
			for (int i = 0; i < array.Length; i++)
			{
				InputButton b17 = array[i];
				inputListener.buttonNames.Add(b17.ToString());
			}
			break;
		}
		case 25:
		{
			InputButton[] array = this.inventorySlot6;
			for (int i = 0; i < array.Length; i++)
			{
				InputButton b15 = array[i];
				inputListener.buttonNames.Add(b15.ToString());
			}
			break;
		}
		case 26:
		{
			InputButton[] array = this.inventorySlot7;
			for (int i = 0; i < array.Length; i++)
			{
				InputButton b13 = array[i];
				inputListener.buttonNames.Add(b13.ToString());
			}
			break;
		}
		case 27:
		{
			InputButton[] array = this.inventorySlot8;
			for (int i = 0; i < array.Length; i++)
			{
				InputButton b11 = array[i];
				inputListener.buttonNames.Add(b11.ToString());
			}
			break;
		}
		case 28:
		{
			InputButton[] array = this.inventorySlot9;
			for (int i = 0; i < array.Length; i++)
			{
				InputButton b9 = array[i];
				inputListener.buttonNames.Add(b9.ToString());
			}
			break;
		}
		case 29:
		{
			InputButton[] array = this.inventorySlot10;
			for (int i = 0; i < array.Length; i++)
			{
				InputButton b7 = array[i];
				inputListener.buttonNames.Add(b7.ToString());
			}
			break;
		}
		case 30:
		{
			InputButton[] array = this.inventorySlot11;
			for (int i = 0; i < array.Length; i++)
			{
				InputButton b5 = array[i];
				inputListener.buttonNames.Add(b5.ToString());
			}
			break;
		}
		case 31:
		{
			InputButton[] array = this.inventorySlot12;
			for (int i = 0; i < array.Length; i++)
			{
				InputButton b3 = array[i];
				inputListener.buttonNames.Add(b3.ToString());
			}
			break;
		}
		case 33:
		{
			InputButton[] array = this.emoteButton;
			for (int i = 0; i < array.Length; i++)
			{
				InputButton b = array[i];
				inputListener.buttonNames.Add(b.ToString());
			}
			break;
		}
		case 8:
		case 9:
			break;
		}
	}

	public void setDropDownToProperValue(OptionsDropDown dropDown)
	{
		switch (dropDown.whichOption)
		{
		case 9:
			dropDown.dropDownOptions.Add("Standard");
			dropDown.dropDownDisplayOptions.Add(Game1.content.LoadString("Strings\\1_6_Strings:options_menubg_0"));
			dropDown.dropDownOptions.Add("Graphical");
			dropDown.dropDownDisplayOptions.Add(Game1.content.LoadString("Strings\\1_6_Strings:options_menubg_1"));
			dropDown.dropDownOptions.Add("None");
			dropDown.dropDownDisplayOptions.Add(Game1.content.LoadString("Strings\\1_6_Strings:options_menubg_2"));
			if (this.showMenuBackground)
			{
				dropDown.selectedOption = 1;
			}
			else if (!this.showClearBackgrounds)
			{
				dropDown.selectedOption = 0;
			}
			else
			{
				dropDown.selectedOption = 2;
			}
			break;
		case 6:
		{
			try
			{
				StartupPreferences startupPreferences = new StartupPreferences();
				startupPreferences.loadPreferences(async: false, applyLanguage: false);
				if (startupPreferences.fullscreenResolutionX != 0)
				{
					this.preferredResolutionX = startupPreferences.fullscreenResolutionX;
					this.preferredResolutionY = startupPreferences.fullscreenResolutionY;
				}
			}
			catch (Exception)
			{
			}
			int i = 0;
			foreach (DisplayMode v in Game1.graphics.GraphicsDevice.Adapter.SupportedDisplayModes)
			{
				if (v.Width >= 1280)
				{
					dropDown.dropDownOptions.Add(v.Width + " x " + v.Height);
					dropDown.dropDownDisplayOptions.Add(v.Width + " x " + v.Height);
					if (v.Width == this.preferredResolutionX && v.Height == this.preferredResolutionY)
					{
						dropDown.selectedOption = i;
					}
					i++;
				}
			}
			dropDown.greyedOut = !this.fullscreen || this.windowedBorderlessFullscreen;
			break;
		}
		case 13:
			this.windowedBorderlessFullscreen = this.isCurrentlyWindowedBorderless();
			this.fullscreen = Game1.graphics.IsFullScreen && !this.windowedBorderlessFullscreen;
			dropDown.dropDownOptions.Add("Windowed");
			if (!this.windowedBorderlessFullscreen)
			{
				dropDown.dropDownOptions.Add("Fullscreen");
			}
			if (!this.fullscreen)
			{
				dropDown.dropDownOptions.Add("Windowed Borderless");
			}
			dropDown.dropDownDisplayOptions.Add(Game1.content.LoadString("Strings\\StringsFromCSFiles:Options.cs.4564"));
			if (!this.windowedBorderlessFullscreen)
			{
				dropDown.dropDownDisplayOptions.Add(Game1.content.LoadString("Strings\\StringsFromCSFiles:Options.cs.4560"));
			}
			if (!this.fullscreen)
			{
				dropDown.dropDownDisplayOptions.Add(Game1.content.LoadString("Strings\\StringsFromCSFiles:Options.cs.4561"));
			}
			if (Game1.graphics.IsFullScreen || this.windowedBorderlessFullscreen)
			{
				dropDown.selectedOption = 1;
			}
			else
			{
				dropDown.selectedOption = 0;
			}
			break;
		case 28:
			dropDown.dropDownOptions.Add("off");
			dropDown.dropDownDisplayOptions.Add(Game1.content.LoadString("Strings\\UI:Options_StowingMode_Off"));
			dropDown.dropDownOptions.Add("gamepad");
			dropDown.dropDownDisplayOptions.Add(Game1.content.LoadString("Strings\\UI:Options_StowingMode_GamepadOnly"));
			dropDown.dropDownOptions.Add("both");
			dropDown.dropDownDisplayOptions.Add(Game1.content.LoadString("Strings\\UI:Options_StowingMode_On"));
			switch (this.stowingMode)
			{
			case ItemStowingModes.Off:
				dropDown.selectedOption = 0;
				break;
			case ItemStowingModes.GamepadOnly:
				dropDown.selectedOption = 1;
				break;
			case ItemStowingModes.Both:
				dropDown.selectedOption = 2;
				break;
			}
			break;
		case 38:
			try
			{
				StartupPreferences startupPreferences2 = new StartupPreferences();
				startupPreferences2.loadPreferences(async: false, applyLanguage: false);
				this.gamepadMode = startupPreferences2.gamepadMode;
			}
			catch (Exception)
			{
			}
			dropDown.dropDownOptions.Add("auto");
			dropDown.dropDownDisplayOptions.Add(Game1.content.LoadString("Strings\\UI:Options_GamepadMode_Auto"));
			dropDown.dropDownOptions.Add("force_on");
			dropDown.dropDownDisplayOptions.Add(Game1.content.LoadString("Strings\\UI:Options_GamepadMode_ForceOn"));
			dropDown.dropDownOptions.Add("force_off");
			dropDown.dropDownDisplayOptions.Add(Game1.content.LoadString("Strings\\UI:Options_GamepadMode_ForceOff"));
			switch (this.gamepadMode)
			{
			case GamepadModes.Auto:
				dropDown.selectedOption = 0;
				break;
			case GamepadModes.ForceOn:
				dropDown.selectedOption = 1;
				break;
			case GamepadModes.ForceOff:
				dropDown.selectedOption = 2;
				break;
			}
			break;
		case 41:
			dropDown.dropDownOptions.Add("hold");
			dropDown.dropDownDisplayOptions.Add(Game1.content.LoadString("Strings\\UI:Options_SlingshotMode_Hold"));
			dropDown.dropDownOptions.Add("legacy");
			dropDown.dropDownDisplayOptions.Add(Game1.content.LoadString("Strings\\UI:Options_SlingshotMode_Pull"));
			if (this.useLegacySlingshotFiring)
			{
				dropDown.selectedOption = 1;
			}
			else
			{
				dropDown.selectedOption = 0;
			}
			break;
		case 42:
		{
			dropDown.dropDownOptions.Add("-1");
			dropDown.dropDownDisplayOptions.Add(Game1.content.LoadString("Strings\\StringsFromCSFiles:BiteChime_Default"));
			for (int j = 0; j <= 3; j++)
			{
				dropDown.dropDownOptions.Add(j.ToString());
				dropDown.dropDownDisplayOptions.Add((j + 1).ToString());
			}
			dropDown.selectedOption = Game1.player.biteChime.Value + 1;
			break;
		}
		case 31:
			dropDown.dropDownOptions.Add("offline");
			dropDown.dropDownDisplayOptions.Add(Game1.content.LoadString("Strings\\UI:GameMenu_ServerMode_Offline"));
			if (Program.sdk.Networking != null)
			{
				dropDown.dropDownOptions.Add("friends");
				dropDown.dropDownDisplayOptions.Add(Game1.content.LoadString("Strings\\UI:GameMenu_ServerMode_FriendsOnly"));
				dropDown.dropDownOptions.Add("invite");
				dropDown.dropDownDisplayOptions.Add(Game1.content.LoadString("Strings\\UI:GameMenu_ServerMode_InviteOnly"));
			}
			else
			{
				dropDown.dropDownOptions.Add("online");
				dropDown.dropDownDisplayOptions.Add(Game1.content.LoadString("Strings\\UI:GameMenu_ServerMode_Online"));
			}
			if (Game1.server == null)
			{
				dropDown.selectedOption = 0;
			}
			else if (Program.sdk.Networking != null)
			{
				switch (this.serverPrivacy)
				{
				case ServerPrivacy.FriendsOnly:
					dropDown.selectedOption = 1;
					break;
				case ServerPrivacy.InviteOnly:
					dropDown.selectedOption = 2;
					break;
				}
			}
			else
			{
				dropDown.selectedOption = 1;
			}
			Game1.log.Verbose("setDropDownToProperValue( serverMode, " + dropDown.dropDownOptions[dropDown.selectedOption] + " ) called.");
			break;
		case 40:
			dropDown.dropDownOptions.Add("on");
			dropDown.dropDownDisplayOptions.Add(Game1.content.LoadString("Strings\\UI:GameMenu_MoveBuildingPermissions_On"));
			dropDown.dropDownOptions.Add("owned");
			dropDown.dropDownDisplayOptions.Add(Game1.content.LoadString("Strings\\UI:GameMenu_MoveBuildingPermissions_Owned"));
			dropDown.dropDownOptions.Add("off");
			dropDown.dropDownDisplayOptions.Add(Game1.content.LoadString("Strings\\UI:GameMenu_MoveBuildingPermissions_Off"));
			switch (Game1.player.team.farmhandsCanMoveBuildings.Value)
			{
			case FarmerTeam.RemoteBuildingPermissions.On:
				dropDown.selectedOption = 0;
				break;
			case FarmerTeam.RemoteBuildingPermissions.OwnedBuildings:
				dropDown.selectedOption = 1;
				break;
			case FarmerTeam.RemoteBuildingPermissions.Off:
				dropDown.selectedOption = 2;
				break;
			}
			break;
		}
	}

	public bool isCurrentlyWindowedBorderless()
	{
		if (Game1.graphics.IsFullScreen)
		{
			return !Game1.graphics.HardwareModeSwitch;
		}
		return false;
	}

	public bool isCurrentlyFullscreen()
	{
		if (Game1.graphics.IsFullScreen)
		{
			return Game1.graphics.HardwareModeSwitch;
		}
		return false;
	}

	public bool isCurrentlyWindowed()
	{
		if (!this.isCurrentlyWindowedBorderless())
		{
			return !this.isCurrentlyFullscreen();
		}
		return false;
	}
}
