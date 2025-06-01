using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using StardewValley.Extensions;
using StardewValley.Minigames;

namespace StardewValley.Menus;

public class TitleMenu : IClickableMenu, IDisposable
{
	public static bool SkipSplashScreens = false;

	public const int region_muteMusic = 81111;

	public const int region_windowedButton = 81112;

	public const int region_aboutButton = 81113;

	public const int region_backButton = 81114;

	public const int region_newButton = 81115;

	public const int region_loadButton = 81116;

	public const int region_coopButton = 81119;

	public const int region_exitButton = 81117;

	public const int region_languagesButton = 81118;

	public const int fadeFromWhiteDuration = 2000;

	public const int viewportFinalPosition = -1000;

	public const int logoSwipeDuration = 1000;

	public const int numberOfButtons = 4;

	public const int spaceBetweenButtons = 8;

	public const float bigCloudDX = 0.1f;

	public const float mediumCloudDX = 0.2f;

	public const float smallCloudDX = 0.3f;

	public const float bgmountainsParallaxSpeed = 0.66f;

	public const float mountainsParallaxSpeed = 1f;

	public const float foregroundJungleParallaxSpeed = 2f;

	public const float cloudsParallaxSpeed = 0.5f;

	public static int pixelZoom = 3;

	public const string titleButtonsTextureName = "Minigames\\TitleButtons";

	public LocalizedContentManager menuContent = Game1.content.CreateTemporary();

	public Texture2D cloudsTexture;

	public Texture2D titleButtonsTexture;

	public bool specialSurprised;

	public float specialSurprisedTimeStamp;

	private Texture2D amuzioTexture;

	private List<float> bigClouds = new List<float>();

	private List<float> smallClouds = new List<float>();

	private TemporaryAnimatedSpriteList tempSprites = new TemporaryAnimatedSpriteList();

	private TemporaryAnimatedSpriteList behindSignTempSprites = new TemporaryAnimatedSpriteList();

	public List<ClickableTextureComponent> buttons = new List<ClickableTextureComponent>();

	public ClickableTextureComponent backButton;

	public ClickableTextureComponent muteMusicButton;

	public ClickableTextureComponent aboutButton;

	public ClickableTextureComponent languageButton;

	public ClickableTextureComponent windowedButton;

	public ClickableComponent skipButton;

	protected bool _movedCursor;

	public TemporaryAnimatedSpriteList birds = new TemporaryAnimatedSpriteList();

	private Rectangle eRect;

	private Rectangle screwRect;

	private Rectangle cornerRect;

	private Rectangle r_hole_rect;

	private Rectangle r_hole_rect2;

	private List<Rectangle> leafRects;

	[InstancedStatic]
	internal static IClickableMenu _subMenu;

	public readonly StartupPreferences startupPreferences;

	public int globalXOffset;

	public float viewportY;

	public float viewportDY;

	public float logoSwipeTimer;

	public float globalCloudAlpha = 1f;

	public float cornerClickEndTimer;

	public float cornerClickParrotTimer;

	public float cornerClickSoundEffectTimer;

	private bool? hasRoomAnotherFarm = false;

	public int fadeFromWhiteTimer;

	public int pauseBeforeViewportRiseTimer;

	public int buttonsToShow;

	public int showButtonsTimer;

	public int logoFadeTimer;

	public int logoSurprisedTimer;

	public int clicksOnE;

	public int clicksOnLeaf;

	public int clicksOnScrew;

	public int cornerClicks;

	public int buttonsDX;

	public bool titleInPosition;

	public bool isTransitioningButtons;

	public bool shades;

	public bool cornerPhaseHolding;

	public bool showCornerClickEasterEgg;

	public bool transitioningCharacterCreationMenu;

	private int amuzioTimer;

	internal static int windowNumber = 3;

	public string startupMessage = "";

	public Color startupMessageColor = Color.DeepSkyBlue;

	public string debugSaveFileToTry;

	private int bCount;

	private string whichSubMenu = "";

	private int quitTimer;

	private bool transitioningFromLoadScreen;

	[NonInstancedStatic]
	public static int ticksUntilLanguageLoad = 1;

	private bool disposedValue;

	/// <summary>The sub-menu to show instead of the main title screen.</summary>
	/// <remarks>When returning to the main title screen from a submenu, call <see cref="M:StardewValley.Menus.TitleMenu.ReturnToMainTitleScreen" /> instead of setting it to null to allow cleanup.</remarks>
	public static IClickableMenu subMenu
	{
		get
		{
			return TitleMenu._subMenu;
		}
		set
		{
			if (TitleMenu._subMenu != null)
			{
				TitleMenu._subMenu.exitFunction = null;
				if (TitleMenu._subMenu is IDisposable disposable && !TitleMenu.subMenu.HasDependencies())
				{
					disposable.Dispose();
				}
			}
			TitleMenu._subMenu = value;
			if (TitleMenu._subMenu != null)
			{
				if (Game1.activeClickableMenu is TitleMenu titleMenu)
				{
					IClickableMenu clickableMenu = TitleMenu._subMenu;
					clickableMenu.exitFunction = (onExit)Delegate.Combine(clickableMenu.exitFunction, new onExit(titleMenu.CloseSubMenu));
				}
				if (Game1.options.snappyMenus && Game1.options.gamepadControls)
				{
					TitleMenu._subMenu.snapToDefaultClickableComponent();
				}
			}
		}
	}

	public bool HasActiveUser => true;

	/// <summary>An event raised when the player clicks the button to start after creating their new main character.</summary>
	public static event Action OnCreatedNewCharacter;

	/// <summary>Exit the current sub-menu and return to the main title screen.</summary>
	public static void ReturnToMainTitleScreen()
	{
		TitleMenu.subMenu = null;
		Game1.game1.ResetGameStateOnTitleScreen();
	}

	public void ForceSubmenu(IClickableMenu menu)
	{
		this.skipToTitleButtons();
		TitleMenu.subMenu = menu;
		this.moveFeatures(1920, 0);
		this.globalXOffset = 1920;
		this.buttonsToShow = 4;
		this.showButtonsTimer = 0;
		this.viewportDY = 0f;
		this.logoSwipeTimer = 0f;
		this.titleInPosition = true;
	}

	public TitleMenu()
		: base(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height)
	{
		LocalizedContentManager.OnLanguageChange += OnLanguageChange;
		this.cloudsTexture = this.menuContent.Load<Texture2D>("Minigames\\Clouds");
		this.titleButtonsTexture = this.menuContent.Load<Texture2D>("Minigames\\TitleButtons");
		if (Program.sdk.IsJapaneseRegionRelease)
		{
			this.amuzioTexture = this.menuContent.Load<Texture2D>("Minigames\\Amuzio");
		}
		this.viewportY = 0f;
		this.fadeFromWhiteTimer = 4000;
		this.logoFadeTimer = 5000;
		if (Program.sdk.IsJapaneseRegionRelease)
		{
			this.amuzioTimer = 4000;
		}
		this.bigClouds.Add(base.width * 3 / 4);
		this.shades = Game1.random.NextBool();
		this.smallClouds.Add(base.width - 1);
		this.smallClouds.Add(base.width - 1 + 230 * TitleMenu.pixelZoom);
		this.smallClouds.Add(base.width * 2 / 3);
		this.smallClouds.Add(base.width / 8);
		this.smallClouds.Add(base.width - 1 + 430 * TitleMenu.pixelZoom);
		this.smallClouds.Add(base.width * 3 / 4);
		this.smallClouds.Add(1f);
		this.smallClouds.Add(base.width / 2 + 150 * TitleMenu.pixelZoom);
		this.smallClouds.Add(base.width - 1 + 630 * TitleMenu.pixelZoom);
		this.smallClouds.Add(base.width - 1 + 130 * TitleMenu.pixelZoom);
		this.smallClouds.Add(base.width / 3 + 190 * TitleMenu.pixelZoom);
		this.smallClouds.Add(1 + 100 * TitleMenu.pixelZoom);
		this.smallClouds.Add(base.width / 2 + 830 * TitleMenu.pixelZoom);
		this.smallClouds.Add(base.width * 2 / 3 + 120 * TitleMenu.pixelZoom);
		this.smallClouds.Add(base.width * 3 / 4 + 170 * TitleMenu.pixelZoom);
		this.smallClouds.Add(base.width / 4 + 220 * TitleMenu.pixelZoom);
		for (int i = 0; i < this.smallClouds.Count; i++)
		{
			this.smallClouds[i] += Game1.random.Next(400);
		}
		this.birds.Add(new TemporaryAnimatedSprite("Minigames\\TitleButtons", new Rectangle(296, 227, 26, 21), new Vector2(base.width - 70 * TitleMenu.pixelZoom, base.height - 130 * TitleMenu.pixelZoom), flipped: false, 0f, Color.White)
		{
			scale = TitleMenu.pixelZoom,
			pingPong = true,
			animationLength = 4,
			interval = 100f,
			totalNumberOfLoops = 9999,
			local = true,
			motion = new Vector2(-1f, 0f),
			layerDepth = 0.25f
		});
		this.birds.Add(new TemporaryAnimatedSprite("Minigames\\TitleButtons", new Rectangle(296, 227, 26, 21), new Vector2(base.width - 40 * TitleMenu.pixelZoom, base.height - 120 * TitleMenu.pixelZoom), flipped: false, 0f, Color.White)
		{
			scale = TitleMenu.pixelZoom,
			pingPong = true,
			animationLength = 4,
			interval = 100f,
			totalNumberOfLoops = 9999,
			local = true,
			delayBeforeAnimationStart = 100,
			motion = new Vector2(-1f, 0f),
			layerDepth = 0.25f
		});
		this.setUpIcons();
		this.muteMusicButton = new ClickableTextureComponent(new Rectangle(16, 16, 36, 36), Game1.mouseCursors, new Rectangle(128, 384, 9, 9), 4f)
		{
			myID = 81111,
			downNeighborID = 81115,
			rightNeighborID = 81112
		};
		this.windowedButton = new ClickableTextureComponent(new Rectangle(Game1.uiViewport.Width - 36 - 16, 16, 36, 36), Game1.mouseCursors, new Rectangle((Game1.options != null && !Game1.options.isCurrentlyWindowed()) ? 155 : 146, 384, 9, 9), 4f)
		{
			myID = 81112,
			leftNeighborID = 81111,
			downNeighborID = 81113
		};
		this.startupPreferences = new StartupPreferences();
		this.startupPreferences.loadPreferences(async: false, applyLanguage: false);
		this.applyPreferences();
		switch (this.startupPreferences.timesPlayed)
		{
		case 3:
			this.startupMessage = this.menuContent.LoadString("Strings\\StringsFromCSFiles:TitleMenu.cs.11718");
			break;
		case 5:
			this.startupMessage = this.menuContent.LoadString("Strings\\StringsFromCSFiles:TitleMenu.cs.11720");
			break;
		case 7:
			this.startupMessage = this.menuContent.LoadString("Strings\\StringsFromCSFiles:TitleMenu.cs.11722");
			break;
		case 2:
			this.startupMessage = this.menuContent.LoadString("Strings\\StringsFromCSFiles:TitleMenu.cs.11717");
			break;
		case 4:
			this.startupMessage = this.menuContent.LoadString("Strings\\StringsFromCSFiles:TitleMenu.cs.11719");
			break;
		case 6:
			this.startupMessage = this.menuContent.LoadString("Strings\\StringsFromCSFiles:TitleMenu.cs.11721");
			break;
		case 8:
			this.startupMessage = this.menuContent.LoadString("Strings\\StringsFromCSFiles:TitleMenu.cs.11723");
			break;
		case 9:
			this.startupMessage = this.menuContent.LoadString("Strings\\StringsFromCSFiles:TitleMenu.cs.11724");
			break;
		case 10:
			this.startupMessage = this.menuContent.LoadString("Strings\\StringsFromCSFiles:TitleMenu.cs.11725");
			break;
		case 15:
			if (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.en)
			{
				string noun = Dialogue.getRandomNoun();
				string noun2 = Dialogue.getRandomNoun();
				this.startupMessage = this.menuContent.LoadString("Strings\\StringsFromCSFiles:TitleMenu.cs.11726") + Environment.NewLine + "The " + Dialogue.getRandomAdjective() + " " + noun + " " + Dialogue.getRandomVerb() + " " + Dialogue.getRandomPositional() + " the " + (noun.Equals(noun2) ? ("other " + noun2) : noun2);
			}
			else
			{
				int randSentence = Game1.random.Next(1, 15);
				this.startupMessage = this.menuContent.LoadString("Strings\\StringsFromCSFiles:TitleMenu.cs.11726") + this.menuContent.LoadString("Strings\\StringsFromCSFiles:RandomSentence." + randSentence);
			}
			break;
		case 20:
			this.startupMessage = "<";
			break;
		case 30:
			this.startupMessage = this.menuContent.LoadString("Strings\\StringsFromCSFiles:TitleMenu.cs.11731");
			break;
		case 100:
			this.startupMessage = this.menuContent.LoadString("Strings\\StringsFromCSFiles:TitleMenu.cs.11732");
			break;
		case 1000:
			this.startupMessage = this.menuContent.LoadString("Strings\\StringsFromCSFiles:TitleMenu.cs.11733");
			break;
		case 10000:
			this.startupMessage = this.menuContent.LoadString("Strings\\StringsFromCSFiles:TitleMenu.cs.11734");
			break;
		}
		this.startupPreferences.savePreferences(async: false);
		Game1.setRichPresence("menus");
		if (Game1.options.snappyMenus && Game1.options.gamepadControls)
		{
			this.populateClickableComponentList();
			this.snapToDefaultClickableComponent();
		}
		if (TitleMenu.SkipSplashScreens)
		{
			this.skipToTitleButtons();
		}
		else
		{
			TitleMenu.SkipSplashScreens = true;
		}
	}

	private bool alternativeTitleGraphic()
	{
		return LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.zh;
	}

	public void applyPreferences()
	{
		if (this.startupPreferences.playerLimit > 0)
		{
			Game1.multiplayer.playerLimit = this.startupPreferences.playerLimit;
		}
		if (this.startupPreferences.startMuted)
		{
			if (Utility.toggleMuteMusic())
			{
				this.muteMusicButton.sourceRect.X = 137;
			}
			else
			{
				this.muteMusicButton.sourceRect.X = 128;
			}
		}
		if (this.startupPreferences.skipWindowPreparation && TitleMenu.windowNumber == 3)
		{
			TitleMenu.windowNumber = -1;
		}
		if (this.startupPreferences.windowMode == 2 && this.startupPreferences.fullscreenResolutionX != 0 && this.startupPreferences.fullscreenResolutionY != 0)
		{
			Game1.options.preferredResolutionX = this.startupPreferences.fullscreenResolutionX;
			Game1.options.preferredResolutionY = this.startupPreferences.fullscreenResolutionY;
		}
		Game1.options.gamepadMode = this.startupPreferences.gamepadMode;
		Game1.game1.CheckGamepadMode();
		if (Game1.options.gamepadControls && Game1.options.snappyMenus)
		{
			this.populateClickableComponentList();
			this.snapToDefaultClickableComponent();
		}
	}

	private void OnLanguageChange(LocalizedContentManager.LanguageCode code)
	{
		this.titleButtonsTexture = this.menuContent.Load<Texture2D>("Minigames\\TitleButtons");
		this.setUpIcons();
		this.tempSprites.Clear();
		this.startupPreferences.OnLanguageChange(code);
	}

	public void skipToTitleButtons()
	{
		this.logoFadeTimer = 0;
		this.logoSwipeTimer = 0f;
		this.titleInPosition = false;
		this.pauseBeforeViewportRiseTimer = 0;
		this.fadeFromWhiteTimer = 0;
		this.viewportY = -999f;
		this.viewportDY = -0.01f;
		this.birds.Clear();
		this.logoSwipeTimer = 1f;
		this.amuzioTimer = 0;
		Game1.changeMusicTrack("MainTheme");
		if (Game1.options.SnappyMenus && Game1.options.gamepadControls)
		{
			this.snapToDefaultClickableComponent();
		}
	}

	public void setUpIcons()
	{
		this.buttons.Clear();
		int buttonWidth = 74;
		int mainButtonSetWidth = buttonWidth * 4 * TitleMenu.pixelZoom;
		mainButtonSetWidth += 24 * TitleMenu.pixelZoom;
		int curx = base.width / 2 - mainButtonSetWidth / 2;
		this.buttons.Add(new ClickableTextureComponent("New", new Rectangle(curx, base.height - 58 * TitleMenu.pixelZoom - 8 * TitleMenu.pixelZoom, buttonWidth * TitleMenu.pixelZoom, 58 * TitleMenu.pixelZoom), null, "", this.titleButtonsTexture, new Rectangle(0, 187, 74, 58), TitleMenu.pixelZoom)
		{
			myID = 81115,
			rightNeighborID = 81116,
			upNeighborID = 81111
		});
		curx += (buttonWidth + 8) * TitleMenu.pixelZoom;
		this.buttons.Add(new ClickableTextureComponent("Load", new Rectangle(curx, base.height - 58 * TitleMenu.pixelZoom - 8 * TitleMenu.pixelZoom, 74 * TitleMenu.pixelZoom, 58 * TitleMenu.pixelZoom), null, "", this.titleButtonsTexture, new Rectangle(74, 187, 74, 58), TitleMenu.pixelZoom)
		{
			myID = 81116,
			leftNeighborID = 81115,
			rightNeighborID = -7777,
			upNeighborID = 81111
		});
		curx += (buttonWidth + 8) * TitleMenu.pixelZoom;
		this.buttons.Add(new ClickableTextureComponent("Co-op", new Rectangle(curx, base.height - 58 * TitleMenu.pixelZoom - 8 * TitleMenu.pixelZoom, 74 * TitleMenu.pixelZoom, 58 * TitleMenu.pixelZoom), null, "", this.titleButtonsTexture, new Rectangle(148, 187, 74, 58), TitleMenu.pixelZoom)
		{
			myID = 81119,
			leftNeighborID = 81116,
			rightNeighborID = 81117
		});
		curx += (buttonWidth + 8) * TitleMenu.pixelZoom;
		this.buttons.Add(new ClickableTextureComponent("Exit", new Rectangle(curx, base.height - 58 * TitleMenu.pixelZoom - 8 * TitleMenu.pixelZoom, 74 * TitleMenu.pixelZoom, 58 * TitleMenu.pixelZoom), null, "", this.titleButtonsTexture, new Rectangle(222, 187, 74, 58), TitleMenu.pixelZoom)
		{
			myID = 81117,
			leftNeighborID = 81119,
			rightNeighborID = 81118,
			upNeighborID = 81111
		});
		int zoom = (this.ShouldShrinkLogo() ? 2 : TitleMenu.pixelZoom);
		this.eRect = new Rectangle(base.width / 2 - 200 * zoom + 251 * zoom, -300 * zoom - (int)(this.viewportY / 3f) * zoom + 26 * zoom, 42 * zoom, 68 * zoom);
		this.screwRect = new Rectangle(base.width / 2 + 150 * zoom, -300 * zoom - (int)(this.viewportY / 3f) * zoom + 80 * zoom, 5 * zoom, 5 * zoom);
		this.cornerRect = new Rectangle(base.width / 2 - 200 * zoom, -300 * zoom - (int)(this.viewportY / 3f) * zoom + 165 * zoom, 20 * zoom, 20 * zoom);
		this.r_hole_rect = new Rectangle(base.width / 2 - 21 * zoom, -300 * zoom - (int)(this.viewportY / 3f) * zoom + 39 * zoom, 10 * zoom, 11 * zoom);
		this.r_hole_rect2 = new Rectangle(base.width / 2 - 35 * zoom, -300 * zoom - (int)(this.viewportY / 3f) * zoom + 24 * zoom, 7 * zoom, 7 * zoom);
		this.populateLeafRects();
		this.backButton = new ClickableTextureComponent(this.menuContent.LoadString("Strings\\StringsFromCSFiles:TitleMenu.cs.11739"), new Rectangle(base.width + -66 * TitleMenu.pixelZoom - 8 * TitleMenu.pixelZoom * 2, base.height - 27 * TitleMenu.pixelZoom - 8 * TitleMenu.pixelZoom, 66 * TitleMenu.pixelZoom, 27 * TitleMenu.pixelZoom), null, "", this.titleButtonsTexture, new Rectangle(296, 252, 66, 27), TitleMenu.pixelZoom)
		{
			myID = 81114
		};
		this.aboutButton = new ClickableTextureComponent(this.menuContent.LoadString("Strings\\StringsFromCSFiles:TitleMenu.cs.11740"), new Rectangle(base.width + -22 * TitleMenu.pixelZoom - 8 * TitleMenu.pixelZoom * 2, base.height - 25 * TitleMenu.pixelZoom - 8 * TitleMenu.pixelZoom, 22 * TitleMenu.pixelZoom, 25 * TitleMenu.pixelZoom), null, "", this.titleButtonsTexture, new Rectangle(8, 458, 22, 25), TitleMenu.pixelZoom)
		{
			myID = 81113,
			upNeighborID = 81118,
			leftNeighborID = -7777
		};
		this.languageButton = new ClickableTextureComponent(this.menuContent.LoadString("Strings\\StringsFromCSFiles:TitleMenu.cs.11740"), new Rectangle(base.width + -22 * TitleMenu.pixelZoom - 8 * TitleMenu.pixelZoom * 2, base.height - 25 * TitleMenu.pixelZoom * 2 - 16 * TitleMenu.pixelZoom, 27 * TitleMenu.pixelZoom, 25 * TitleMenu.pixelZoom), null, "", this.titleButtonsTexture, new Rectangle(52, 458, 27, 25), TitleMenu.pixelZoom)
		{
			myID = 81118,
			downNeighborID = 81113,
			leftNeighborID = -7777,
			upNeighborID = 81112
		};
		this.skipButton = new ClickableComponent(new Rectangle(base.width / 2 - 87 * TitleMenu.pixelZoom, base.height / 2 - 34 * TitleMenu.pixelZoom, 83 * TitleMenu.pixelZoom, 67 * TitleMenu.pixelZoom), this.menuContent.LoadString("Strings\\StringsFromCSFiles:TitleMenu.cs.11741"));
		if (this.globalXOffset > base.width)
		{
			this.globalXOffset = base.width;
		}
		foreach (ClickableTextureComponent button in this.buttons)
		{
			button.bounds.X += this.globalXOffset;
		}
		if (Game1.options.gamepadControls && Game1.options.snappyMenus)
		{
			this.populateClickableComponentList();
			this.snapToDefaultClickableComponent();
		}
	}

	public override void snapToDefaultClickableComponent()
	{
		if (TitleMenu.subMenu != null)
		{
			TitleMenu.subMenu.snapToDefaultClickableComponent();
			return;
		}
		StartupPreferences obj = this.startupPreferences;
		base.currentlySnappedComponent = base.getComponentWithID((obj != null && obj.timesPlayed > 0) ? 81116 : 81115);
		this.snapCursorToCurrentSnappedComponent();
	}

	protected override void customSnapBehavior(int direction, int oldRegion, int oldID)
	{
		if (oldID == 81116 && direction == 1)
		{
			if (base.getComponentWithID(81119) != null)
			{
				this.setCurrentlySnappedComponentTo(81119);
				this.snapCursorToCurrentSnappedComponent();
			}
			else if (base.getComponentWithID(81117) != null)
			{
				this.setCurrentlySnappedComponentTo(81117);
				this.snapCursorToCurrentSnappedComponent();
			}
			else
			{
				this.setCurrentlySnappedComponentTo(81118);
				this.snapCursorToCurrentSnappedComponent();
			}
		}
		else if ((oldID == 81118 || oldID == 81113) && direction == 3)
		{
			if (base.getComponentWithID(81117) != null)
			{
				this.setCurrentlySnappedComponentTo(81117);
				this.snapCursorToCurrentSnappedComponent();
			}
			else
			{
				this.setCurrentlySnappedComponentTo(81116);
				this.snapCursorToCurrentSnappedComponent();
			}
		}
	}

	public void populateLeafRects()
	{
		int zoom = (this.ShouldShrinkLogo() ? 2 : TitleMenu.pixelZoom);
		this.leafRects = new List<Rectangle>
		{
			new Rectangle(base.width / 2 - 200 * zoom + 251 * zoom - 196 * zoom, -300 * zoom - (int)(this.viewportY / 3f) * zoom + 26 * zoom + 109 * zoom, 17 * zoom, 30 * zoom),
			new Rectangle(base.width / 2 - 200 * zoom + 251 * zoom + 91 * zoom, -300 * zoom - (int)(this.viewportY / 3f) * zoom + 26 * zoom - 26 * zoom, 17 * zoom, 31 * zoom),
			new Rectangle(base.width / 2 - 200 * zoom + 251 * zoom + 79 * zoom, -300 * zoom - (int)(this.viewportY / 3f) * zoom + 26 * zoom + 83 * zoom, 25 * zoom, 17 * zoom),
			new Rectangle(base.width / 2 - 200 * zoom + 251 * zoom - 213 * zoom, -300 * zoom - (int)(this.viewportY / 3f) * zoom + 26 * zoom - 24 * zoom, 14 * zoom, 23 * zoom),
			new Rectangle(base.width / 2 - 200 * zoom + 251 * zoom - 234 * zoom, -300 * zoom - (int)(this.viewportY / 3f) * zoom + 26 * zoom - 11 * zoom, 18 * zoom, 12 * zoom)
		};
	}

	/// <inheritdoc />
	public override void receiveRightClick(int x, int y, bool playSound = true)
	{
		if (this.ShouldAllowInteraction() && !this.transitioningCharacterCreationMenu)
		{
			TitleMenu.subMenu?.receiveRightClick(x, y);
		}
	}

	public override bool readyToClose()
	{
		return false;
	}

	/// <inheritdoc />
	public override bool overrideSnappyMenuCursorMovementBan()
	{
		return !this.titleInPosition;
	}

	/// <inheritdoc />
	public override void leftClickHeld(int x, int y)
	{
		if (!this.transitioningCharacterCreationMenu)
		{
			base.leftClickHeld(x, y);
			if (TitleMenu.subMenu != null)
			{
				TitleMenu.subMenu.leftClickHeld(x, y);
			}
		}
	}

	/// <inheritdoc />
	public override void releaseLeftClick(int x, int y)
	{
		if (!this.transitioningCharacterCreationMenu && !this.transitioningCharacterCreationMenu)
		{
			base.releaseLeftClick(x, y);
			TitleMenu.subMenu?.releaseLeftClick(x, y);
		}
	}

	/// <inheritdoc />
	public override void receiveKeyPress(Keys key)
	{
		if (this.transitioningCharacterCreationMenu)
		{
			return;
		}
		switch (key)
		{
		case Keys.N:
			if (!Program.releaseBuild && Game1.oldKBState.IsKeyDown(Keys.RightShift) && Game1.oldKBState.IsKeyDown(Keys.LeftControl))
			{
				Season season = Season.Spring;
				if (Game1.oldKBState.IsKeyDown(Keys.D1))
				{
					Game1.whichFarm = 1;
				}
				else if (Game1.oldKBState.IsKeyDown(Keys.D2))
				{
					Game1.whichFarm = 2;
				}
				else if (Game1.oldKBState.IsKeyDown(Keys.D3))
				{
					Game1.whichFarm = 3;
				}
				else if (Game1.oldKBState.IsKeyDown(Keys.D4))
				{
					Game1.whichFarm = 4;
				}
				else if (Game1.oldKBState.IsKeyDown(Keys.D5))
				{
					Game1.whichFarm = 5;
				}
				else if (Game1.oldKBState.IsKeyDown(Keys.D6))
				{
					Game1.whichFarm = 6;
				}
				if (Game1.oldKBState.IsKeyDown(Keys.C))
				{
					Game1.whichFarm = Game1.random.Next(6);
					Game1.season = (Season)Game1.random.Next(4);
				}
				Game1.game1.loadForNewGame();
				Game1.saveOnNewDay = false;
				Game1.player.eventsSeen.Add("60367");
				Game1.player.currentLocation = Utility.getHomeOfFarmer(Game1.player);
				Game1.player.Position = new Vector2(9f, 9f) * 64f;
				Game1.player.isInBed.Value = true;
				Game1.player.farmName.Value = "Test";
				if (Game1.oldKBState.IsKeyDown(Keys.C))
				{
					Game1.season = season;
					Game1.setGraphicsForSeason(onLoad: true);
				}
				Game1.player.mailReceived.Add("button_tut_1");
				Game1.player.mailReceived.Add("button_tut_2");
				Game1.NewDay(0f);
				Game1.exitActiveMenu();
				Game1.setGameMode(3);
				return;
			}
			break;
		case Keys.Escape:
		case Keys.B:
			if (this.logoFadeTimer > 0)
			{
				this.bCount++;
				if (key == Keys.Escape)
				{
					this.bCount += 3;
				}
				if (this.bCount >= 3)
				{
					Game1.playSound("bigDeSelect");
					this.logoFadeTimer = 0;
					this.fadeFromWhiteTimer = 0;
					Game1.delayedActions.Clear();
					Game1.morningSongPlayAction = null;
					this.pauseBeforeViewportRiseTimer = 0;
					this.fadeFromWhiteTimer = 0;
					this.viewportY = -999f;
					this.viewportDY = -0.01f;
					this.birds.Clear();
					this.logoSwipeTimer = 1f;
					this.amuzioTimer = 0;
					Game1.changeMusicTrack("MainTheme");
				}
			}
			break;
		}
		if (!Game1.options.doesInputListContain(Game1.options.menuButton, key) && this.ShouldAllowInteraction())
		{
			TitleMenu.subMenu?.receiveKeyPress(key);
			if (Game1.options.snappyMenus && Game1.options.gamepadControls && TitleMenu.subMenu == null)
			{
				base.receiveKeyPress(key);
			}
		}
	}

	/// <inheritdoc />
	public override void receiveGamePadButton(Buttons button)
	{
		base.receiveGamePadButton(button);
		TitleMenu.subMenu?.receiveGamePadButton(button);
		if (button != Buttons.B || !this.titleInPosition || this.logoFadeTimer > 0 || this.fadeFromWhiteTimer > 0)
		{
			return;
		}
		IClickableMenu clickableMenu = TitleMenu.subMenu;
		if (!(clickableMenu is LoadGameMenu loadGameMenu))
		{
			if (clickableMenu is CharacterCustomization customizationMenu)
			{
				if (!customizationMenu.showingCoopHelp)
				{
					this.backButtonPressed();
				}
			}
			else
			{
				this.backButtonPressed();
			}
		}
		else if (!loadGameMenu.deleteConfirmationScreen)
		{
			this.backButtonPressed();
		}
	}

	/// <inheritdoc />
	public override void gamePadButtonHeld(Buttons b)
	{
		if (!Game1.lastCursorMotionWasMouse)
		{
			this._movedCursor = true;
		}
		TitleMenu.subMenu?.gamePadButtonHeld(b);
	}

	public void backButtonPressed()
	{
		if (TitleMenu.subMenu == null || !TitleMenu.subMenu.readyToClose())
		{
			return;
		}
		Game1.playSound("bigDeSelect");
		this.buttonsDX = -1;
		if (TitleMenu.subMenu is AboutMenu)
		{
			TitleMenu.ReturnToMainTitleScreen();
			this.buttonsDX = 0;
			if (Game1.options.SnappyMenus)
			{
				this.setCurrentlySnappedComponentTo(81113);
				this.snapCursorToCurrentSnappedComponent();
			}
		}
		else if (TitleMenu.subMenu is TitleTextInputMenu { context: "join_menu" } || TitleMenu.subMenu is FarmhandMenu)
		{
			this.buttonsDX = 0;
			((CoopMenu)(TitleMenu.subMenu = new CoopMenu(tooManyFarms: false))).SetTab(CoopMenu.Tab.JOIN_TAB, playSound: false);
			if (Game1.options.SnappyMenus)
			{
				TitleMenu.subMenu.snapToDefaultClickableComponent();
			}
		}
		else if (TitleMenu.subMenu is CharacterCustomization { source: CharacterCustomization.Source.HostNewFarm })
		{
			this.buttonsDX = 0;
			((CoopMenu)(TitleMenu.subMenu = new CoopMenu(tooManyFarms: false))).SetTab(CoopMenu.Tab.HOST_TAB, playSound: false);
			Game1.changeMusicTrack("title_night");
			if (Game1.options.SnappyMenus)
			{
				TitleMenu.subMenu.snapToDefaultClickableComponent();
			}
		}
		else
		{
			this.isTransitioningButtons = true;
			if (TitleMenu.subMenu is LoadGameMenu)
			{
				this.transitioningFromLoadScreen = true;
			}
			TitleMenu.ReturnToMainTitleScreen();
			Game1.changeMusicTrack("spring_day_ambient");
		}
	}

	private void UpdateHasRoomAnotherFarm()
	{
		lock (this)
		{
			this.hasRoomAnotherFarm = null;
		}
		Game1.GetHasRoomAnotherFarmAsync(delegate(bool yes)
		{
			lock (this)
			{
				this.hasRoomAnotherFarm = yes;
			}
		});
	}

	protected void CloseSubMenu()
	{
		if (!TitleMenu.subMenu.readyToClose())
		{
			return;
		}
		this.buttonsDX = -1;
		if (TitleMenu.subMenu is AboutMenu || TitleMenu.subMenu is LanguageSelectionMenu)
		{
			TitleMenu.subMenu = null;
			this.buttonsDX = 0;
			return;
		}
		this.isTransitioningButtons = true;
		if (TitleMenu.subMenu is LoadGameMenu)
		{
			this.transitioningFromLoadScreen = true;
		}
		TitleMenu.subMenu = null;
		Game1.changeMusicTrack("spring_day_ambient");
	}

	/// <inheritdoc />
	public override void receiveLeftClick(int x, int y, bool playSound = true)
	{
		if (this.HasActiveUser && this.muteMusicButton.containsPoint(x, y))
		{
			this.startupPreferences.startMuted = Utility.toggleMuteMusic();
			if (this.muteMusicButton.sourceRect.X == 128)
			{
				this.muteMusicButton.sourceRect.X = 137;
			}
			else
			{
				this.muteMusicButton.sourceRect.X = 128;
			}
			Game1.playSound("drumkit6");
			this.startupPreferences.savePreferences(async: false);
			return;
		}
		if (this.HasActiveUser && this.windowedButton.containsPoint(x, y))
		{
			if (!Game1.options.isCurrentlyWindowed())
			{
				Game1.options.setWindowedOption("Windowed");
				this.windowedButton.sourceRect.X = 146;
				this.startupPreferences.windowMode = 1;
			}
			else
			{
				Game1.options.setWindowedOption("Windowed Borderless");
				this.windowedButton.sourceRect.X = 155;
				this.startupPreferences.windowMode = 0;
			}
			this.startupPreferences.savePreferences(async: false);
			Game1.playSound("drumkit6");
			return;
		}
		if (this.logoFadeTimer > 0 && this.skipButton != null && this.skipButton.containsPoint(x, y))
		{
			if (this.logoSurprisedTimer <= 0)
			{
				int pitch = 1200;
				this.logoSurprisedTimer = 1500;
				string soundtoPlay = "fishSlap";
				Game1.changeMusicTrack("none");
				switch (Game1.random.Next(2))
				{
				case 0:
					soundtoPlay = "Duck";
					pitch = 0;
					break;
				case 1:
					soundtoPlay = "fishSlap";
					break;
				}
				if (Game1.random.NextDouble() < 0.02)
				{
					this.specialSurprised = true;
					Game1.playSound("moss_cut");
					this.fadeFromWhiteTimer = 3000;
				}
				else
				{
					Game1.playSound(soundtoPlay, pitch);
				}
			}
			else if (this.logoSurprisedTimer > 1)
			{
				this.logoSurprisedTimer = Math.Max(1, this.logoSurprisedTimer - 500);
			}
		}
		if (this.amuzioTimer > 500)
		{
			this.amuzioTimer = 500;
		}
		if (this.logoFadeTimer > 0 || this.fadeFromWhiteTimer > 0 || this.transitioningCharacterCreationMenu)
		{
			return;
		}
		if (TitleMenu.subMenu != null)
		{
			bool should_ignore_back_button_press = false;
			if (Game1.options.SnappyMenus && TitleMenu.subMenu.currentlySnappedComponent != null && TitleMenu.subMenu.currentlySnappedComponent.myID != 81114)
			{
				should_ignore_back_button_press = true;
			}
			bool handled_submenu_close = false;
			if (TitleMenu.subMenu.readyToClose() && this.backButton != null && this.backButton.containsPoint(x, y) && !should_ignore_back_button_press)
			{
				this.backButtonPressed();
				handled_submenu_close = true;
			}
			else if (!this.isTransitioningButtons)
			{
				TitleMenu.subMenu.receiveLeftClick(x, y);
			}
			if (handled_submenu_close || TitleMenu.subMenu == null || !TitleMenu.subMenu.readyToClose() || (!(TitleMenu.subMenu is TooManyFarmsMenu) && (this.backButton == null || !this.backButton.containsPoint(x, y))) || should_ignore_back_button_press)
			{
				return;
			}
			Game1.playSound("bigDeSelect");
			this.buttonsDX = -1;
			if (TitleMenu.subMenu is AboutMenu || TitleMenu.subMenu is LanguageSelectionMenu)
			{
				TitleMenu.ReturnToMainTitleScreen();
				this.buttonsDX = 0;
				return;
			}
			this.isTransitioningButtons = true;
			if (TitleMenu.subMenu is LoadGameMenu)
			{
				this.transitioningFromLoadScreen = true;
			}
			TitleMenu.ReturnToMainTitleScreen();
			Game1.changeMusicTrack("spring_day_ambient");
			return;
		}
		if (this.logoFadeTimer <= 0 && !this.titleInPosition && this.logoSwipeTimer == 0f)
		{
			this.pauseBeforeViewportRiseTimer = 0;
			this.fadeFromWhiteTimer = 0;
			this.viewportY = -999f;
			this.viewportDY = -0.01f;
			this.birds.Clear();
			this.logoSwipeTimer = 1f;
			return;
		}
		if (!this.alternativeTitleGraphic())
		{
			if (this.clicksOnLeaf >= 10 && Game1.random.NextDouble() < 0.001)
			{
				Game1.playSound("junimoMeep1");
			}
			if (this.titleInPosition && this.eRect.Contains(x, y) && this.clicksOnE < 10)
			{
				this.clicksOnE++;
				Game1.playSound("woodyStep");
				if (this.clicksOnE == 10)
				{
					int zoom = (this.ShouldShrinkLogo() ? 2 : TitleMenu.pixelZoom);
					Game1.playSound("openChest");
					this.tempSprites.Add(new TemporaryAnimatedSprite("Minigames\\TitleButtons", new Rectangle(0, 491, 42, 68), new Vector2(base.width / 2 - 200 * zoom + 251 * zoom, -300 * zoom - (int)(this.viewportY / 3f) * zoom + 26 * zoom), flipped: false, 0f, Color.White)
					{
						scale = zoom,
						animationLength = 9,
						interval = 200f,
						local = true,
						holdLastFrame = true
					});
				}
			}
			else if (this.titleInPosition)
			{
				bool clicked = false;
				foreach (Rectangle leafRect in this.leafRects)
				{
					if (leafRect.Contains(x, y))
					{
						clicked = true;
						break;
					}
				}
				if (this.screwRect.Contains(x, y) && this.clicksOnScrew < 10)
				{
					Game1.playSound("cowboy_monsterhit");
					this.clicksOnScrew++;
					if (this.clicksOnScrew == 10)
					{
						this.showButterflies();
					}
				}
				if (Game1.content.GetCurrentLanguage() != LocalizedContentManager.LanguageCode.zh)
				{
					if (this.cornerPhaseHolding && (this.r_hole_rect.Contains(x, y) || this.r_hole_rect2.Contains(x, y)) && this.cornerClicks < 999)
					{
						Game1.playSound("coin");
						this.cornerClickEndTimer = 1000f;
						this.cornerClickSoundEffectTimer = 400f;
						this.cornerClicks = 9999;
						this.showCornerClickEasterEgg = true;
					}
					else if (this.cornerRect.Contains(x, y) && !this.cornerPhaseHolding)
					{
						int zoom2 = (this.ShouldShrinkLogo() ? 2 : TitleMenu.pixelZoom);
						this.cornerClicks++;
						if (this.cornerClicks > 5)
						{
							if (!this.cornerPhaseHolding)
							{
								Game1.playSound("coin");
								this.cornerClicks = 0;
								this.cornerPhaseHolding = true;
							}
						}
						else
						{
							Game1.playSound("hammer");
							for (int i = 0; i < 3; i++)
							{
								this.tempSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(280 + Game1.random.Choose(8, 0), 1954, 8, 8), 1000f, 1, 99, new Vector2(base.width / 2 - 190 * zoom2, -300 * zoom2 - (int)(this.viewportY / 3f) * zoom2 + 175 * zoom2), flicker: false, flipped: false, 1f, 0f, Color.White, TitleMenu.pixelZoom, 0f, 0f, (float)Game1.random.Next(-10, 11) / 100f)
								{
									motion = new Vector2(Game1.random.Next(-4, 5), -8f + (float)Game1.random.Next(-10, 1) / 100f),
									acceleration = new Vector2(0f, 0.3f),
									local = true,
									delayBeforeAnimationStart = i * 15
								});
							}
						}
					}
				}
				if (clicked)
				{
					this.clicksOnLeaf++;
					if (this.clicksOnLeaf == 10)
					{
						int zoom3 = (this.ShouldShrinkLogo() ? 2 : TitleMenu.pixelZoom);
						Game1.playSound("discoverMineral");
						this.tempSprites.Add(new TemporaryAnimatedSprite("Minigames\\TitleButtons", new Rectangle(264, 464, 16, 16), new Vector2(base.width / 2 - 200 * zoom3 + 80 * zoom3, -300 * zoom3 - (int)(this.viewportY / 3f) * zoom3 + 10 * zoom3 + 2), flipped: false, 0f, Color.White)
						{
							scale = zoom3,
							animationLength = 8,
							interval = 80f,
							totalNumberOfLoops = 999999,
							local = true,
							holdLastFrame = false,
							delayBeforeAnimationStart = 200
						});
						this.tempSprites.Add(new TemporaryAnimatedSprite("Minigames\\TitleButtons", new Rectangle(136, 448, 16, 16), new Vector2(base.width / 2 - 200 * zoom3 + 80 * zoom3, -300 * zoom3 - (int)(this.viewportY / 3f) * zoom3 + 10 * zoom3), flipped: false, 0f, Color.White)
						{
							scale = zoom3,
							animationLength = 8,
							interval = 50f,
							local = true,
							holdLastFrame = false
						});
						this.tempSprites.Add(new TemporaryAnimatedSprite("Minigames\\TitleButtons", new Rectangle(200, 464, 16, 16), new Vector2(base.width / 2 - 200 * zoom3 + 178 * zoom3, -300 * zoom3 - (int)(this.viewportY / 3f) * zoom3 + 141 * zoom3 + 2), flipped: false, 0f, Color.White)
						{
							scale = zoom3,
							animationLength = 4,
							interval = 150f,
							totalNumberOfLoops = 999999,
							local = true,
							holdLastFrame = false,
							delayBeforeAnimationStart = 400
						});
						this.tempSprites.Add(new TemporaryAnimatedSprite("Minigames\\TitleButtons", new Rectangle(136, 448, 16, 16), new Vector2(base.width / 2 - 200 * zoom3 + 178 * zoom3, -300 * zoom3 - (int)(this.viewportY / 3f) * zoom3 + 141 * zoom3), flipped: false, 0f, Color.White)
						{
							scale = zoom3,
							animationLength = 8,
							interval = 50f,
							local = true,
							holdLastFrame = false,
							delayBeforeAnimationStart = 200
						});
						this.tempSprites.Add(new TemporaryAnimatedSprite("Minigames\\TitleButtons", new Rectangle(136, 464, 16, 16), new Vector2(base.width / 2 - 200 * zoom3 + 294 * zoom3, -300 * zoom3 - (int)(this.viewportY / 3f) * zoom3 + 89 * zoom3 + 2), flipped: false, 0f, Color.White)
						{
							scale = zoom3,
							animationLength = 4,
							interval = 150f,
							totalNumberOfLoops = 999999,
							local = true,
							holdLastFrame = false,
							delayBeforeAnimationStart = 600
						});
						this.tempSprites.Add(new TemporaryAnimatedSprite("Minigames\\TitleButtons", new Rectangle(136, 448, 16, 16), new Vector2(base.width / 2 - 200 * zoom3 + 294 * zoom3, -300 * zoom3 - (int)(this.viewportY / 3f) * zoom3 + 89 * zoom3), flipped: false, 0f, Color.White)
						{
							scale = zoom3,
							animationLength = 8,
							interval = 50f,
							local = true,
							holdLastFrame = false,
							delayBeforeAnimationStart = 400
						});
					}
					else
					{
						Game1.playSound("leafrustle");
						int zoom4 = (this.ShouldShrinkLogo() ? 2 : TitleMenu.pixelZoom);
						for (int j = 0; j < 2; j++)
						{
							this.tempSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(355, 1199 + Game1.random.Next(-1, 2) * 16, 16, 16), new Vector2(x + Game1.random.Next(-8, 9), y + Game1.random.Next(-8, 9)), Game1.random.NextBool(), 0f, Color.White)
							{
								scale = zoom4,
								animationLength = 11,
								interval = 50 + Game1.random.Next(50),
								totalNumberOfLoops = 999,
								motion = new Vector2((float)Game1.random.Next(-100, 101) / 100f, 1f + (float)Game1.random.Next(-100, 100) / 500f),
								xPeriodic = Game1.random.NextBool(),
								xPeriodicLoopTime = Game1.random.Next(6000, 16000),
								xPeriodicRange = Game1.random.Next(64, 192),
								alphaFade = 0.001f,
								local = true,
								holdLastFrame = false,
								delayBeforeAnimationStart = j * 20
							});
						}
					}
				}
			}
		}
		if (!this.ShouldAllowInteraction() || !this.HasActiveUser || (TitleMenu.subMenu != null && !TitleMenu.subMenu.readyToClose()) || this.isTransitioningButtons)
		{
			return;
		}
		for (int k = 0; k < this.buttons.Count; k++)
		{
			ClickableTextureComponent c = this.buttons[k];
			if (c.containsPoint(x, y))
			{
				this.performButtonAction(c.name);
			}
		}
		if (this.aboutButton.containsPoint(x, y))
		{
			TitleMenu.subMenu = new AboutMenu();
			Game1.playSound("newArtifact");
		}
		if (this.languageButton.visible && this.languageButton.containsPoint(x, y))
		{
			TitleMenu.subMenu = new LanguageSelectionMenu();
			Game1.playSound("newArtifact");
		}
	}

	public void performButtonAction(string which)
	{
		this.whichSubMenu = which;
		switch (which)
		{
		case "New":
			this.buttonsDX = 1;
			this.isTransitioningButtons = true;
			Game1.playSound("select");
			foreach (TemporaryAnimatedSprite tempSprite in this.tempSprites)
			{
				tempSprite.pingPong = false;
			}
			this.UpdateHasRoomAnotherFarm();
			break;
		case "Co-op":
			this.buttonsDX = 1;
			this.isTransitioningButtons = true;
			Game1.playSound("select");
			this.UpdateHasRoomAnotherFarm();
			break;
		case "Load":
		case "Invite":
			this.buttonsDX = 1;
			this.isTransitioningButtons = true;
			Game1.playSound("select");
			break;
		case "Exit":
			Game1.playSound("bigDeSelect");
			Game1.changeMusicTrack("none");
			this.quitTimer = 500;
			break;
		}
	}

	private void addRightLeafGust()
	{
		if (!this.isTransitioningButtons && this.tempSprites.Count <= 0 && !this.alternativeTitleGraphic())
		{
			int zoom = (this.ShouldShrinkLogo() ? 2 : TitleMenu.pixelZoom);
			this.tempSprites.Add(new TemporaryAnimatedSprite("Minigames\\TitleButtons", new Rectangle(296, 187, 27, 21), new Vector2(base.width / 2 - 200 * zoom + 327 * zoom, (float)(-300 * zoom) - this.viewportY / 3f * (float)zoom + (float)(107 * zoom)), flipped: false, 0f, Color.White)
			{
				scale = zoom,
				pingPong = true,
				animationLength = 3,
				interval = 100f,
				totalNumberOfLoops = 3,
				local = true
			});
		}
	}

	public bool ShouldShrinkLogo()
	{
		return base.height <= 850;
	}

	private void addLeftLeafGust()
	{
		if (!this.isTransitioningButtons && this.tempSprites.Count <= 0 && !this.alternativeTitleGraphic())
		{
			int zoom = (this.ShouldShrinkLogo() ? 2 : TitleMenu.pixelZoom);
			this.tempSprites.Add(new TemporaryAnimatedSprite("Minigames\\TitleButtons", new Rectangle(296, 208, 22, 18), new Vector2(base.width / 2 - 200 * zoom + 16 * zoom, (float)(-300 * zoom) - this.viewportY / 3f * (float)zoom + (float)(16 * zoom)), flipped: false, 0f, Color.White)
			{
				scale = zoom,
				pingPong = true,
				animationLength = 3,
				interval = 100f,
				totalNumberOfLoops = 3,
				local = true
			});
		}
	}

	public void createdNewCharacter(bool skipIntro)
	{
		TitleMenu.OnCreatedNewCharacter?.Invoke();
		Game1.playSound("smallSelect");
		TitleMenu.subMenu = null;
		this.transitioningCharacterCreationMenu = true;
		if (skipIntro)
		{
			Game1.game1.loadForNewGame();
			Game1.saveOnNewDay = true;
			Game1.player.eventsSeen.Add("60367");
			Game1.player.currentLocation = Utility.getHomeOfFarmer(Game1.player);
			Game1.player.Position = new Vector2(9f, 9f) * 64f;
			Game1.player.isInBed.Value = true;
			Game1.NewDay(0f);
			Game1.exitActiveMenu();
			Game1.setGameMode(3);
		}
	}

	/// <inheritdoc />
	public override void update(GameTime time)
	{
		if (Game1.game1.IsMainInstance)
		{
			if (TitleMenu.ticksUntilLanguageLoad > 0)
			{
				TitleMenu.ticksUntilLanguageLoad--;
			}
			else if (TitleMenu.ticksUntilLanguageLoad == 0)
			{
				TitleMenu.ticksUntilLanguageLoad--;
				this.startupPreferences.loadPreferences(async: false, applyLanguage: true);
			}
		}
		if (TitleMenu.windowNumber > 0)
		{
			if (this.startupPreferences.displayIndex >= 0 && !GameRunner.instance.Window.CenterOnDisplay(this.startupPreferences.displayIndex))
			{
				Game1.log.Error("Error: Couldn't find display with index " + this.startupPreferences.displayIndex + ". Reverting to windowed mode on display 0.");
				this.startupPreferences.windowMode = 1;
			}
			Game1.options.setWindowedOption(this.startupPreferences.windowMode);
			TitleMenu.windowNumber = 0;
		}
		if (!Game1.options.isCurrentlyWindowed())
		{
			Vector2 corner_position = new Vector2(Game1.viewport.Width - 36 - 16, 16f);
			corner_position.X = Math.Min(GameRunner.instance.Window.GetDisplayBounds(GameRunner.instance.Window.GetDisplayIndex()).Right - GameRunner.instance.Window.ClientBounds.Left, Game1.viewport.Width) - 36 - 16;
			this.windowedButton.setPosition(corner_position);
		}
		base.update(time);
		TitleMenu.subMenu?.update(time);
		if (this.transitioningCharacterCreationMenu)
		{
			this.globalCloudAlpha -= (float)time.ElapsedGameTime.Milliseconds * 0.001f;
			if (this.globalCloudAlpha <= 0f)
			{
				this.transitioningCharacterCreationMenu = false;
				this.globalCloudAlpha = 0f;
				TitleMenu.subMenu = null;
				Game1.currentMinigame = new GrandpaStory();
				Game1.exitActiveMenu();
				Game1.setGameMode(3);
			}
		}
		if (this.quitTimer > 0)
		{
			this.quitTimer -= time.ElapsedGameTime.Milliseconds;
			if (this.quitTimer <= 0)
			{
				Game1.quit = true;
				Game1.exitActiveMenu();
			}
		}
		if (this.amuzioTimer > 0)
		{
			this.amuzioTimer -= time.ElapsedGameTime.Milliseconds;
		}
		else if (this.logoFadeTimer > 0)
		{
			if (this.logoSurprisedTimer > 0)
			{
				this.logoSurprisedTimer -= time.ElapsedGameTime.Milliseconds;
				if (this.logoSurprisedTimer <= 0)
				{
					this.logoFadeTimer = 1;
				}
			}
			else
			{
				int old = this.logoFadeTimer;
				this.logoFadeTimer -= time.ElapsedGameTime.Milliseconds;
				if (this.logoFadeTimer < 4000 && old >= 4000)
				{
					Game1.playSound("mouseClick");
				}
				if (this.logoFadeTimer < 2500 && old >= 2500)
				{
					Game1.playSound("mouseClick");
				}
				if (this.logoFadeTimer < 2000 && old >= 2000)
				{
					Game1.playSound("mouseClick");
				}
				if (this.logoFadeTimer <= 0)
				{
					Game1.changeMusicTrack("MainTheme");
				}
			}
		}
		else if (this.fadeFromWhiteTimer > 0)
		{
			this.fadeFromWhiteTimer -= time.ElapsedGameTime.Milliseconds;
			if (this.fadeFromWhiteTimer <= 0)
			{
				this.pauseBeforeViewportRiseTimer = 3500;
			}
		}
		else if (this.pauseBeforeViewportRiseTimer > 0)
		{
			this.pauseBeforeViewportRiseTimer -= time.ElapsedGameTime.Milliseconds;
			if (this.pauseBeforeViewportRiseTimer <= 0)
			{
				this.viewportDY = -0.05f;
			}
		}
		this.viewportY += this.viewportDY;
		if (this.viewportDY < 0f)
		{
			this.viewportDY -= 0.006f;
		}
		if (this.viewportY <= -1000f)
		{
			if (this.viewportDY != 0f)
			{
				this.logoSwipeTimer = 1000f;
				this.showButtonsTimer = 200;
			}
			this.viewportDY = 0f;
		}
		if (this.logoSwipeTimer > 0f)
		{
			this.logoSwipeTimer -= time.ElapsedGameTime.Milliseconds;
			if (this.logoSwipeTimer <= 0f)
			{
				this.addLeftLeafGust();
				this.addRightLeafGust();
				this.titleInPosition = true;
				int zoom = (this.ShouldShrinkLogo() ? 2 : TitleMenu.pixelZoom);
				this.eRect = new Rectangle(base.width / 2 - 200 * zoom + 251 * zoom, -300 * zoom - (int)(this.viewportY / 3f) * zoom + 26 * zoom, 42 * zoom, 68 * zoom);
				this.screwRect = new Rectangle(base.width / 2 + 150 * zoom, -300 * zoom - (int)(this.viewportY / 3f) * zoom + 80 * zoom, 5 * zoom, 5 * zoom);
				this.cornerRect = new Rectangle(base.width / 2 - 200 * zoom, -300 * zoom - (int)(this.viewportY / 3f) * zoom + 165 * zoom, 20 * zoom, 20 * zoom);
				this.r_hole_rect = new Rectangle(base.width / 2 - 21 * zoom, -300 * zoom - (int)(this.viewportY / 3f) * zoom + 39 * zoom, 10 * zoom, 11 * zoom);
				this.r_hole_rect2 = new Rectangle(base.width / 2 - 35 * zoom, -300 * zoom - (int)(this.viewportY / 3f) * zoom + 24 * zoom, 7 * zoom, 7 * zoom);
				this.populateLeafRects();
			}
		}
		if (this.showButtonsTimer > 0 && this.HasActiveUser && TitleMenu.subMenu == null)
		{
			this.showButtonsTimer -= time.ElapsedGameTime.Milliseconds;
			if (this.showButtonsTimer <= 0)
			{
				if (this.buttonsToShow < 4)
				{
					this.buttonsToShow++;
					Game1.playSound("Cowboy_gunshot");
					this.showButtonsTimer = 200;
				}
				else if (Game1.options.gamepadControls && Game1.options.snappyMenus)
				{
					this.populateClickableComponentList();
					this.snapToDefaultClickableComponent();
				}
			}
		}
		if (this.titleInPosition && !this.isTransitioningButtons && this.globalXOffset == 0 && Game1.random.NextDouble() < 0.005)
		{
			if (Game1.random.NextBool())
			{
				this.addLeftLeafGust();
			}
			else
			{
				this.addRightLeafGust();
			}
		}
		if (this.titleInPosition)
		{
			if (this.isTransitioningButtons)
			{
				int dx = this.buttonsDX * (int)time.ElapsedGameTime.TotalMilliseconds;
				int offsetx = this.globalXOffset + dx;
				int over = offsetx - base.width;
				if (over > 0)
				{
					offsetx -= over;
					dx -= over;
				}
				this.globalXOffset = offsetx;
				this.moveFeatures(dx, 0);
				if (this.buttonsDX > 0 && this.globalXOffset >= base.width)
				{
					if (TitleMenu.subMenu != null)
					{
						if (TitleMenu.subMenu.readyToClose())
						{
							this.isTransitioningButtons = false;
							this.buttonsDX = 0;
						}
					}
					else
					{
						switch (this.whichSubMenu)
						{
						case "Load":
							TitleMenu.subMenu = new LoadGameMenu();
							Game1.changeMusicTrack("title_night");
							this.buttonsDX = 0;
							this.isTransitioningButtons = false;
							break;
						case "Co-op":
							if (!this.hasRoomAnotherFarm.HasValue)
							{
								break;
							}
							this.buttonsDX = 0;
							this.isTransitioningButtons = false;
							if (true)
							{
								TitleMenu.subMenu = new CoopMenu(!this.hasRoomAnotherFarm.Value);
								Game1.changeMusicTrack("title_night");
								break;
							}
							Game1.playSound("bigDeSelect");
							if (Game1.options.SnappyMenus)
							{
								this.setCurrentlySnappedComponentTo(81119);
								this.snapCursorToCurrentSnappedComponent();
							}
							break;
						case "Invite":
							TitleMenu.subMenu = new FarmhandMenu();
							Game1.changeMusicTrack("title_night");
							this.buttonsDX = 0;
							this.isTransitioningButtons = false;
							break;
						case "New":
							if (!this.hasRoomAnotherFarm.HasValue)
							{
								break;
							}
							if (!this.hasRoomAnotherFarm.Value)
							{
								TitleMenu.subMenu = new TooManyFarmsMenu();
								Game1.playSound("newArtifact");
								this.buttonsDX = 0;
								this.isTransitioningButtons = false;
								break;
							}
							Game1.resetPlayer();
							TitleMenu.subMenu = new CharacterCustomization(CharacterCustomization.Source.NewGame);
							if (this.startupPreferences.timesPlayed > 1 && !this.startupPreferences.sawAdvancedCharacterCreationIndicator)
							{
								if (TitleMenu.subMenu is CharacterCustomization custom)
								{
									custom.showAdvancedCharacterCreationHighlight();
								}
								this.startupPreferences.sawAdvancedCharacterCreationIndicator = true;
								this.startupPreferences.savePreferences(async: false);
							}
							Game1.playSound("select");
							Game1.changeMusicTrack("CloudCountry");
							Game1.player.favoriteThing.Value = "";
							this.buttonsDX = 0;
							this.isTransitioningButtons = false;
							break;
						}
					}
					if (!this.isTransitioningButtons)
					{
						this.whichSubMenu = "";
					}
				}
				else if (this.buttonsDX < 0 && this.globalXOffset <= 0)
				{
					this.globalXOffset = 0;
					this.isTransitioningButtons = false;
					this.buttonsDX = 0;
					this.setUpIcons();
					this.whichSubMenu = "";
					this.transitioningFromLoadScreen = false;
				}
			}
			if (this.cornerClickEndTimer > 0f)
			{
				this.cornerClickEndTimer -= (float)Game1.currentGameTime.ElapsedGameTime.TotalMilliseconds;
				if (this.cornerClickEndTimer <= 0f)
				{
					this.cornerClickParrotTimer = 400f;
				}
			}
			if (this.cornerClickSoundEffectTimer > 0f)
			{
				this.cornerClickSoundEffectTimer -= (float)Game1.currentGameTime.ElapsedGameTime.TotalMilliseconds;
				if (this.cornerClickSoundEffectTimer <= 0f)
				{
					Game1.playSound("goldenWalnut");
				}
			}
			if (this.cornerClickParrotTimer > 0f)
			{
				this.cornerClickParrotTimer -= (float)Game1.currentGameTime.ElapsedGameTime.TotalMilliseconds;
				if (this.cornerClickParrotTimer <= 0f)
				{
					int zoom2 = (this.ShouldShrinkLogo() ? 2 : TitleMenu.pixelZoom);
					this.behindSignTempSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\parrots", new Rectangle(120, 0, 24, 24), 100f, 3, 999, new Vector2(this.globalXOffset + base.width / 2 - 200 * zoom2, (float)(-300 * zoom2) - this.viewportY / 3f * (float)zoom2 + (float)(100 * zoom2)), flicker: false, flipped: false, 0.2f, 0f, Color.White, zoom2, 0.01f, 0f, 0f, local: true)
					{
						pingPong = true,
						motion = new Vector2(-6f, -1f),
						acceleration = new Vector2(0.02f, 0.02f)
					});
					this.behindSignTempSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\parrots", new Rectangle(120, 48, 24, 24), 95f, 3, 999, new Vector2(this.globalXOffset + base.width / 2 - 200 * zoom2, (float)(-300 * zoom2) - this.viewportY / 3f * (float)zoom2 + (float)(120 * zoom2)), flicker: false, flipped: false, 0.2f, 0f, Color.White, zoom2, 0.01f, 0f, 0f, local: true)
					{
						pingPong = true,
						motion = new Vector2(-6f, -1f),
						acceleration = new Vector2(0.02f, 0.02f),
						delayBeforeAnimationStart = 300,
						startSound = "leafrustle"
					});
					this.behindSignTempSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\parrots", new Rectangle(120, 24, 24, 24), 100f, 3, 999, new Vector2(this.globalXOffset + base.width / 2 - 200 * zoom2, (float)(-300 * zoom2) - this.viewportY / 3f * (float)zoom2 + (float)(100 * zoom2)), flicker: false, flipped: false, 0.2f, 0f, Color.White, zoom2, 0.01f, 0f, 0f, local: true)
					{
						pingPong = true,
						motion = new Vector2(-6f, -1f),
						acceleration = new Vector2(0.02f, 0.02f),
						delayBeforeAnimationStart = 600,
						startSound = "parrot_squawk"
					});
					this.behindSignTempSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\parrots", new Rectangle(120, 72, 24, 24), 95f, 3, 999, new Vector2(this.globalXOffset + base.width / 2 - 200 * zoom2, (float)(-300 * zoom2) - this.viewportY / 3f * (float)zoom2 + (float)(120 * zoom2)), flicker: false, flipped: false, 0.2f, 0f, Color.White, zoom2, 0.01f, 0f, 0f, local: true)
					{
						pingPong = true,
						motion = new Vector2(-6f, -1f),
						acceleration = new Vector2(0.02f, 0.02f),
						delayBeforeAnimationStart = 1300,
						startSound = "leafrustle"
					});
					this.behindSignTempSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\parrots", new Rectangle(120, 0, 24, 24), 100f, 3, 999, new Vector2(this.globalXOffset + base.width / 2 + 200 * zoom2 - 24 * zoom2, (float)(-300 * zoom2) - this.viewportY / 3f * (float)zoom2 + (float)(100 * zoom2)), flicker: false, flipped: true, 0.2f, 0f, Color.White, zoom2, 0.01f, 0f, 0f, local: true)
					{
						pingPong = true,
						motion = new Vector2(6f, -1f),
						acceleration = new Vector2(-0.02f, -0.02f),
						delayBeforeAnimationStart = 600
					});
					this.behindSignTempSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\parrots", new Rectangle(120, 48, 24, 24), 95f, 3, 999, new Vector2(this.globalXOffset + base.width / 2 + 200 * zoom2 - 24 * zoom2, (float)(-300 * zoom2) - this.viewportY / 3f * (float)zoom2 + (float)(120 * zoom2)), flicker: false, flipped: true, 0.2f, 0f, Color.White, zoom2, 0.01f, 0f, 0f, local: true)
					{
						pingPong = true,
						motion = new Vector2(6f, -1f),
						acceleration = new Vector2(-0.02f, -0.02f),
						delayBeforeAnimationStart = 900,
						startSound = "leafrustle"
					});
					this.behindSignTempSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\parrots", new Rectangle(120, 24, 24, 24), 100f, 3, 999, new Vector2(this.globalXOffset + base.width / 2 + 200 * zoom2 - 24 * zoom2, (float)(-300 * zoom2) - this.viewportY / 3f * (float)zoom2 + (float)(100 * zoom2)), flicker: false, flipped: true, 0.2f, 0f, Color.White, zoom2, 0.01f, 0f, 0f, local: true)
					{
						pingPong = true,
						motion = new Vector2(6f, -1f),
						acceleration = new Vector2(-0.02f, -0.02f),
						delayBeforeAnimationStart = 1200
					});
					this.behindSignTempSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\parrots", new Rectangle(120, 72, 24, 24), 95f, 3, 999, new Vector2(this.globalXOffset + base.width / 2 + 200 * zoom2 - 24 * zoom2, (float)(-300 * zoom2) - this.viewportY / 3f * (float)zoom2 + (float)(120 * zoom2)), flicker: false, flipped: true, 0.2f, 0f, Color.White, zoom2, 0.01f, 0f, 0f, local: true)
					{
						pingPong = true,
						motion = new Vector2(6f, -1f),
						acceleration = new Vector2(-0.02f, -0.02f),
						delayBeforeAnimationStart = 1500
					});
					for (int i = 0; i < 14; i++)
					{
						this.tempSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(355, 1199, 16, 16), new Vector2(this.globalXOffset + base.width / 2 - 220 * zoom2, (float)(-300 * zoom2) - this.viewportY / 3f * (float)zoom2 + (float)(60 * zoom2) + (float)(Game1.random.Next(100) * zoom2)), Game1.random.NextBool(), 0f, new Color(180, 180, 240))
						{
							scale = zoom2,
							animationLength = 11,
							interval = 50 + Game1.random.Next(50),
							totalNumberOfLoops = 999,
							motion = new Vector2((float)Game1.random.Next(-100, 101) / 100f, 1f + (float)Game1.random.Next(-100, 100) / 500f),
							xPeriodic = Game1.random.NextBool(),
							xPeriodicLoopTime = Game1.random.Next(6000, 16000),
							xPeriodicRange = Game1.random.Next(64, 192),
							alphaFade = 0.001f,
							local = true,
							holdLastFrame = false,
							delayBeforeAnimationStart = 100 + i * 20
						});
					}
					for (int j = 0; j < 14; j++)
					{
						this.tempSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(355, 1199, 16, 16), new Vector2(this.globalXOffset + base.width / 2 + 220 * zoom2, (float)(-300 * zoom2) - this.viewportY / 3f * (float)zoom2 + (float)(60 * zoom2) + (float)(Game1.random.Next(100) * zoom2)), Game1.random.NextBool(), 0f, new Color(180, 180, 240))
						{
							scale = zoom2,
							animationLength = 11,
							interval = 50 + Game1.random.Next(50),
							totalNumberOfLoops = 999,
							motion = new Vector2((float)Game1.random.Next(-100, 101) / 100f, 1f + (float)Game1.random.Next(-100, 100) / 500f),
							xPeriodic = Game1.random.NextBool(),
							xPeriodicLoopTime = Game1.random.Next(6000, 16000),
							xPeriodicRange = Game1.random.Next(64, 192),
							alphaFade = 0.001f,
							local = true,
							holdLastFrame = false,
							delayBeforeAnimationStart = 900 + j * 20
						});
					}
				}
			}
		}
		for (int i2 = this.bigClouds.Count - 1; i2 >= 0; i2--)
		{
			this.bigClouds[i2] -= 0.1f;
			this.bigClouds[i2] += this.buttonsDX * time.ElapsedGameTime.Milliseconds / 2;
			if (this.bigClouds[i2] < (float)(-512 * TitleMenu.pixelZoom))
			{
				this.bigClouds[i2] = base.width;
			}
		}
		for (int i3 = this.smallClouds.Count - 1; i3 >= 0; i3--)
		{
			this.smallClouds[i3] -= 0.3f;
			this.smallClouds[i3] += this.buttonsDX * time.ElapsedGameTime.Milliseconds / 2;
			if (this.smallClouds[i3] < (float)(-149 * TitleMenu.pixelZoom))
			{
				this.smallClouds[i3] = base.width;
			}
		}
		this.tempSprites.RemoveWhere((TemporaryAnimatedSprite sprite) => sprite.update(time));
		this.behindSignTempSprites.RemoveWhere((TemporaryAnimatedSprite sprite) => sprite.update(time));
		this.birds.RemoveWhere(delegate(TemporaryAnimatedSprite bird)
		{
			bird.position.Y -= this.viewportDY * 2f;
			return bird.update(time);
		});
	}

	private void moveFeatures(int dx, int dy)
	{
		foreach (TemporaryAnimatedSprite tempSprite in this.tempSprites)
		{
			tempSprite.position.X += dx;
			tempSprite.position.Y += dy;
		}
		foreach (TemporaryAnimatedSprite behindSignTempSprite in this.behindSignTempSprites)
		{
			behindSignTempSprite.position.X += dx;
			behindSignTempSprite.position.Y += dy;
		}
		foreach (ClickableTextureComponent button in this.buttons)
		{
			button.bounds.X += dx;
			button.bounds.Y += dy;
		}
	}

	/// <inheritdoc />
	public override void receiveScrollWheelAction(int direction)
	{
		if (this.ShouldAllowInteraction())
		{
			base.receiveScrollWheelAction(direction);
			TitleMenu.subMenu?.receiveScrollWheelAction(direction);
		}
	}

	/// <inheritdoc />
	public override void performHoverAction(int x, int y)
	{
		if (!this.ShouldAllowInteraction())
		{
			x = int.MinValue;
			y = int.MinValue;
		}
		base.performHoverAction(x, y);
		this.muteMusicButton.tryHover(x, y);
		if (TitleMenu.subMenu != null)
		{
			TitleMenu.subMenu.performHoverAction(x, y);
			if (this.backButton == null || !TitleMenu.subMenu.readyToClose())
			{
				return;
			}
			if (this.backButton.containsPoint(x, y))
			{
				if (this.backButton.sourceRect.Y == 252)
				{
					Game1.playSound("Cowboy_Footstep");
				}
				this.backButton.sourceRect.Y = 279;
			}
			else
			{
				this.backButton.sourceRect.Y = 252;
			}
			this.backButton.tryHover(x, y, 0.25f);
		}
		else
		{
			if (!this.titleInPosition || !this.HasActiveUser)
			{
				return;
			}
			foreach (ClickableTextureComponent c in this.buttons)
			{
				if (c.containsPoint(x, y))
				{
					if (c.sourceRect.Y == 187)
					{
						Game1.playSound("Cowboy_Footstep");
					}
					c.sourceRect.Y = 245;
				}
				else
				{
					c.sourceRect.Y = 187;
				}
				c.tryHover(x, y, 0.25f);
			}
			this.aboutButton.tryHover(x, y, 0.25f);
			if (this.aboutButton.containsPoint(x, y))
			{
				if (this.aboutButton.sourceRect.X == 8)
				{
					Game1.playSound("Cowboy_Footstep");
				}
				this.aboutButton.sourceRect.X = 30;
			}
			else
			{
				this.aboutButton.sourceRect.X = 8;
			}
			if (!this.languageButton.visible)
			{
				return;
			}
			this.languageButton.tryHover(x, y, 0.25f);
			if (this.languageButton.containsPoint(x, y))
			{
				if (this.languageButton.sourceRect.X == 52)
				{
					Game1.playSound("Cowboy_Footstep");
				}
				this.languageButton.sourceRect.X = 79;
			}
			else
			{
				this.languageButton.sourceRect.X = 52;
			}
		}
	}

	/// <inheritdoc />
	public override void draw(SpriteBatch b)
	{
		bool shouldDrawMenu = TitleMenu.subMenu == null || TitleMenu.subMenu is AboutMenu || TitleMenu.subMenu is LanguageSelectionMenu;
		b.Draw(Game1.staminaRect, new Rectangle(0, 0, base.width, base.height), new Color(64, 136, 248));
		b.Draw(Game1.mouseCursors, new Rectangle(0, (int)((float)(-300 * TitleMenu.pixelZoom) - this.viewportY * 0.66f), base.width, 300 * TitleMenu.pixelZoom + base.height - 120 * TitleMenu.pixelZoom), new Rectangle(703, 1912, 1, 264), Color.White);
		if (!this.whichSubMenu.Equals("Load"))
		{
			for (int x = -10; x < base.width; x += 638)
			{
				b.Draw(Game1.mouseCursors, new Vector2(x * TitleMenu.pixelZoom, (float)(-360 * TitleMenu.pixelZoom) - this.viewportY * 0.66f), new Rectangle(0, 1453, 638, 195), Color.White * (1f - (float)this.globalXOffset / 1200f), 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.8f);
			}
		}
		foreach (float f in this.bigClouds)
		{
			b.Draw(this.cloudsTexture, new Vector2(f, (float)(base.height - 250 * TitleMenu.pixelZoom) - this.viewportY * 0.5f), new Rectangle(0, 0, 512, 337), Color.White * this.globalCloudAlpha, 0f, Vector2.Zero, TitleMenu.pixelZoom, SpriteEffects.None, 0.01f);
		}
		b.Draw(Game1.mouseCursors, new Vector2(-30 * TitleMenu.pixelZoom, (float)(base.height - 158 * TitleMenu.pixelZoom) - this.viewportY * 0.66f), new Rectangle(0, 886, 639, 148), Color.White, 0f, Vector2.Zero, TitleMenu.pixelZoom, SpriteEffects.None, 0.08f);
		b.Draw(Game1.mouseCursors, new Vector2(-30 * TitleMenu.pixelZoom + 639 * TitleMenu.pixelZoom, (float)(base.height - 158 * TitleMenu.pixelZoom) - this.viewportY * 0.66f), new Rectangle(0, 886, 640, 148), Color.White, 0f, Vector2.Zero, TitleMenu.pixelZoom, SpriteEffects.None, 0.08f);
		for (int i = 0; i < this.smallClouds.Count; i++)
		{
			b.Draw(this.cloudsTexture, new Vector2(this.smallClouds[i], (float)(base.height - 300 * TitleMenu.pixelZoom - i * 12 * TitleMenu.pixelZoom) - this.viewportY * 0.5f), (i % 3 == 0) ? new Rectangle(152, 447, 123, 55) : ((i % 3 == 1) ? new Rectangle(0, 471, 149, 66) : new Rectangle(410, 467, 63, 37)), Color.White * this.globalCloudAlpha, 0f, Vector2.Zero, TitleMenu.pixelZoom, SpriteEffects.None, 0.01f);
		}
		b.Draw(Game1.mouseCursors, new Vector2(0f, (float)(base.height - 148 * TitleMenu.pixelZoom) - this.viewportY * 1f), new Rectangle(0, 737, 639, 148), Color.White, 0f, Vector2.Zero, TitleMenu.pixelZoom, SpriteEffects.None, 0.1f);
		b.Draw(Game1.mouseCursors, new Vector2(639 * TitleMenu.pixelZoom, (float)(base.height - 148 * TitleMenu.pixelZoom) - this.viewportY * 1f), new Rectangle(0, 737, 640, 148), Color.White, 0f, Vector2.Zero, TitleMenu.pixelZoom, SpriteEffects.None, 0.1f);
		foreach (TemporaryAnimatedSprite bird in this.birds)
		{
			bird.draw(b);
		}
		b.Draw(this.cloudsTexture, new Vector2(0f, (float)(base.height - 142 * TitleMenu.pixelZoom) - this.viewportY * 2f), new Rectangle(0, 554, 165, 142), Color.White, 0f, Vector2.Zero, TitleMenu.pixelZoom, SpriteEffects.None, 0.2f);
		b.Draw(this.cloudsTexture, new Vector2(base.width - 122 * TitleMenu.pixelZoom, (float)(base.height - 153 * TitleMenu.pixelZoom) - this.viewportY * 2f), new Rectangle(390, 543, 122, 153), Color.White, 0f, Vector2.Zero, TitleMenu.pixelZoom, SpriteEffects.None, 0.2f);
		int zoom = (this.ShouldShrinkLogo() ? 2 : TitleMenu.pixelZoom);
		if (!this.whichSubMenu.Equals("Load") && !this.whichSubMenu.Equals("Co-op") && !(TitleMenu.subMenu is LoadGameMenu))
		{
			CharacterCustomization obj = TitleMenu.subMenu as CharacterCustomization;
			if ((obj == null || obj.source != CharacterCustomization.Source.HostNewFarm) && !this.transitioningFromLoadScreen)
			{
				goto IL_073a;
			}
		}
		Texture2D texture = Game1.mouseCursors;
		Rectangle dstRect = new Rectangle(0, 0, base.width, base.height);
		Rectangle srcRect = new Rectangle(702, 1912, 1, 264);
		b.Draw(texture, dstRect, srcRect, Color.White * ((float)this.globalXOffset / 1200f));
		SpriteEffects effect = SpriteEffects.None;
		for (int y = 0; y < base.height; y += 195)
		{
			for (int j = 0; j < base.width; j += 638)
			{
				b.Draw(Game1.mouseCursors, new Vector2(j, y) * 4f, new Rectangle(0, 1453, 638, 195), Color.White * ((float)this.globalXOffset / 1200f), 0f, Vector2.Zero, 4f, effect, 0.8f);
			}
			effect = ((effect == SpriteEffects.None) ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
		}
		goto IL_073a;
		IL_073a:
		if (shouldDrawMenu)
		{
			foreach (TemporaryAnimatedSprite behindSignTempSprite in this.behindSignTempSprites)
			{
				behindSignTempSprite.draw(b);
			}
			if (this.showCornerClickEasterEgg && Game1.content.GetCurrentLanguage() != LocalizedContentManager.LanguageCode.zh)
			{
				float movementPercent = 1f - Math.Min(1f, 1f - this.cornerClickEndTimer / 700f);
				float yOffset = (float)(40 * zoom) * movementPercent;
				Vector2 baseVect = new Vector2(this.globalXOffset + base.width / 2 - 200 * zoom, (float)(-300 * zoom) - this.viewportY / 3f * (float)zoom);
				b.Draw(Game1.mouseCursors2, baseVect + new Vector2(80 * zoom, (float)(-10 * zoom) + yOffset), new Rectangle(224, 148, 32, 21), Color.White, 0f, Vector2.Zero, zoom, SpriteEffects.None, 0.01f);
				b.Draw(Game1.mouseCursors2, baseVect + new Vector2(120 * zoom, (float)(-15 * zoom) + yOffset), new Rectangle(224, 148, 32, 21), Color.White, 0f, Vector2.Zero, zoom, SpriteEffects.None, 0.01f);
				b.Draw(Game1.mouseCursors, baseVect + new Vector2(160 * zoom, (float)(-25 * zoom) + yOffset), new Rectangle(646, 895, 55, 48), Color.White, 0f, Vector2.Zero, zoom, SpriteEffects.None, 0.01f);
				b.Draw(Game1.mouseCursors2, baseVect + new Vector2(220 * zoom, (float)(-15 * zoom) + yOffset), new Rectangle(224, 148, 32, 21), Color.White, 0f, Vector2.Zero, zoom, SpriteEffects.None, 0.01f);
				b.Draw(Game1.mouseCursors2, baseVect + new Vector2(260 * zoom, (float)(-5 * zoom) + yOffset), new Rectangle(224, 148, 32, 21), Color.White, 0f, Vector2.Zero, zoom, SpriteEffects.None, 0.01f);
				float xOffset = (float)(40 * zoom) * movementPercent;
				b.Draw(Game1.mouseCursors2, baseVect + new Vector2((float)(-10 * zoom) + xOffset, 70 * zoom), new Rectangle(224, 148, 32, 21), Color.White, -(float)Math.PI / 2f, Vector2.Zero, zoom, SpriteEffects.None, 0.01f);
				b.Draw(Game1.mouseCursors2, baseVect + new Vector2((float)(-5 * zoom) + xOffset, 100 * zoom), new Rectangle(224, 148, 32, 21), Color.White, -(float)Math.PI / 2f, Vector2.Zero, zoom, SpriteEffects.None, 0.01f);
				b.Draw(Game1.mouseCursors2, baseVect + new Vector2((float)(-12 * zoom) + xOffset, 130 * zoom), new Rectangle(224, 148, 32, 21), Color.White, -(float)Math.PI / 2f, Vector2.Zero, zoom, SpriteEffects.None, 0.01f);
				b.Draw(Game1.mouseCursors2, baseVect + new Vector2((float)(-10 * zoom) + xOffset, 160 * zoom), new Rectangle(224, 148, 32, 21), Color.White, -(float)Math.PI / 2f, Vector2.Zero, zoom, SpriteEffects.None, 0.01f);
				xOffset = (float)(-40 * zoom) * movementPercent;
				b.Draw(Game1.mouseCursors2, baseVect + new Vector2((float)(410 * zoom) + xOffset, 40 * zoom), new Rectangle(224, 148, 32, 21), Color.White, (float)Math.PI / 2f, Vector2.Zero, zoom, SpriteEffects.None, 0.01f);
				b.Draw(Game1.mouseCursors2, baseVect + new Vector2((float)(415 * zoom) + xOffset, 70 * zoom), new Rectangle(224, 148, 32, 21), Color.White, (float)Math.PI / 2f, Vector2.Zero, zoom, SpriteEffects.None, 0.01f);
				b.Draw(Game1.mouseCursors2, baseVect + new Vector2((float)(405 * zoom) + xOffset, 100 * zoom), new Rectangle(224, 148, 32, 21), Color.White, (float)Math.PI / 2f, Vector2.Zero, zoom, SpriteEffects.None, 0.01f);
				b.Draw(Game1.mouseCursors2, baseVect + new Vector2((float)(410 * zoom) + xOffset, 130 * zoom), new Rectangle(224, 148, 32, 21), Color.White, (float)Math.PI / 2f, Vector2.Zero, zoom, SpriteEffects.None, 0.01f);
			}
			b.Draw(this.titleButtonsTexture, new Vector2(this.globalXOffset + base.width / 2 - 200 * zoom, (float)(-300 * zoom) - this.viewportY / 3f * (float)zoom), new Rectangle(0, 0, 400, 187), Color.White, 0f, Vector2.Zero, zoom, SpriteEffects.None, 0.2f);
			if (this.logoSwipeTimer > 0f)
			{
				b.Draw(this.titleButtonsTexture, new Vector2(this.globalXOffset + base.width / 2, (float)(-300 * zoom) - this.viewportY / 3f * (float)zoom + (float)(93 * zoom)), new Rectangle(0, 0, 400, 187), Color.White, 0f, new Vector2(200f, 93f), (float)zoom + (0.5f - Math.Abs(this.logoSwipeTimer / 1000f - 0.5f)) * 0.1f, SpriteEffects.None, 0.2f);
			}
			if (this.cornerPhaseHolding && this.cornerClicks > 999 && Game1.content.GetCurrentLanguage() != LocalizedContentManager.LanguageCode.zh)
			{
				b.Draw(Game1.mouseCursors2, new Vector2(this.globalXOffset + this.r_hole_rect.X + zoom, this.r_hole_rect.Y - 2), new Rectangle(131, 196, 9, 10), Color.White, 0f, Vector2.Zero, zoom, SpriteEffects.None, 0.24f);
			}
		}
		if (shouldDrawMenu)
		{
			bool greyButtons = TitleMenu.subMenu is AboutMenu || TitleMenu.subMenu is LanguageSelectionMenu;
			for (int k = 0; k < this.buttonsToShow; k++)
			{
				if (this.buttons.Count > k)
				{
					this.buttons[k].draw(b, (TitleMenu.subMenu == null || !greyButtons) ? Color.White : (Color.LightGray * 0.8f), 1f);
				}
			}
			if (TitleMenu.subMenu == null)
			{
				foreach (TemporaryAnimatedSprite tempSprite in this.tempSprites)
				{
					tempSprite.draw(b);
				}
			}
		}
		if (TitleMenu.subMenu != null && !this.isTransitioningButtons)
		{
			if (this.backButton != null && TitleMenu.subMenu.readyToClose())
			{
				this.backButton.draw(b);
			}
			TitleMenu.subMenu.draw(b);
			if (this.backButton != null && !(TitleMenu.subMenu is CharacterCustomization) && TitleMenu.subMenu.readyToClose())
			{
				this.backButton.draw(b);
			}
		}
		else if (TitleMenu.subMenu == null && this.isTransitioningButtons && (this.whichSubMenu.Equals("Load") || this.whichSubMenu.Equals("New")))
		{
			int x2 = 84;
			int y2 = Game1.uiViewport.Height - 64;
			int w = 0;
			int h = 64;
			Utility.makeSafe(ref x2, ref y2, w, h);
			SpriteText.drawStringWithScrollBackground(b, Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3689"), x2, y2);
		}
		else if (TitleMenu.subMenu == null && !this.isTransitioningButtons && this.titleInPosition && !this.transitioningCharacterCreationMenu && this.HasActiveUser && shouldDrawMenu)
		{
			this.aboutButton.draw(b);
			this.languageButton.draw(b);
		}
		if (this.amuzioTimer > 0)
		{
			b.Draw(Game1.staminaRect, new Rectangle(0, 0, base.width, base.height), Color.White);
			Vector2 pos = new Vector2(base.width / 2 - this.amuzioTexture.Width / 2 * 4, base.height / 2 - this.amuzioTexture.Height / 2 * 4);
			pos.X = MathHelper.Lerp(pos.X, -this.amuzioTexture.Width * 4, (float)Math.Max(0, this.amuzioTimer - 3750) / 250f);
			b.Draw(this.amuzioTexture, pos, null, Color.White * Math.Min(1f, (float)this.amuzioTimer / 500f), 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.2f);
		}
		else if (this.logoFadeTimer > 0 || this.fadeFromWhiteTimer > 0)
		{
			b.Draw(Game1.staminaRect, new Rectangle(0, 0, base.width, base.height), Color.White * ((float)this.fadeFromWhiteTimer / 2000f));
			if (!this.specialSurprised)
			{
				b.Draw(this.titleButtonsTexture, new Vector2(base.width / 2, base.height / 2 - 30 * TitleMenu.pixelZoom), new Rectangle(171 + ((this.logoFadeTimer / 100 % 2 == 0 && this.logoSurprisedTimer <= 0) ? 111 : 0), 311, 111, 60), Color.White * ((this.logoFadeTimer < 500) ? ((float)this.logoFadeTimer / 500f) : ((this.logoFadeTimer > 4500) ? (1f - (float)(this.logoFadeTimer - 4500) / 500f) : 1f)), 0f, Vector2.Zero, TitleMenu.pixelZoom, SpriteEffects.None, 0.2f);
			}
			if (this.logoSurprisedTimer <= 0)
			{
				b.Draw(this.titleButtonsTexture, new Vector2(base.width / 2 - 87 * TitleMenu.pixelZoom, base.height / 2 - 34 * TitleMenu.pixelZoom), new Rectangle((this.logoFadeTimer / 100 % 2 == 0) ? 85 : 0, 306 + (this.shades ? 69 : 0), 85, 69), Color.White * ((this.logoFadeTimer < 500) ? ((float)this.logoFadeTimer / 500f) : ((this.logoFadeTimer > 4500) ? (1f - (float)(this.logoFadeTimer - 4500) / 500f) : 1f)), 0f, Vector2.Zero, TitleMenu.pixelZoom, SpriteEffects.None, 0.2f);
			}
			if (this.specialSurprised)
			{
				if (this.logoFadeTimer > 0)
				{
					b.Draw(Game1.staminaRect, new Rectangle(0, 0, base.width, base.height), new Color(221, 255, 198));
				}
				b.Draw(Game1.staminaRect, new Rectangle(0, 0, base.width, base.height), new Color(221, 255, 198) * ((float)this.fadeFromWhiteTimer / 2000f));
				int time = (int)Game1.currentGameTime.TotalGameTime.TotalMilliseconds;
				for (int l = 64; l < base.width + 1000; l += 192)
				{
					for (int m = -1000; m < base.height; m += 192)
					{
						b.Draw(Game1.mouseCursors, new Vector2(l, m) + new Vector2((float)(-time) / 20f, (float)time / 20f), new Rectangle(355 + (time + l * 77 + m * 77) / 12 % 110 / 11 * 16, 1200, 16, 16), Color.White * 0.66f * ((float)(this.fadeFromWhiteTimer - (2000 - this.fadeFromWhiteTimer)) / 2000f), 0f, Vector2.Zero, TitleMenu.pixelZoom, SpriteEffects.None, 0.18f);
					}
				}
				b.Draw(this.titleButtonsTexture, new Vector2(base.width / 2, base.height / 2 - 30 * TitleMenu.pixelZoom), new Rectangle(171 + ((time / 200 % 2 == 0) ? 111 : 0), 563, 111, 60), Color.White * ((float)(this.fadeFromWhiteTimer - (2000 - this.fadeFromWhiteTimer)) / 2000f), 0f, Vector2.Zero, TitleMenu.pixelZoom, SpriteEffects.None, 0.2f);
				this.specialSurprisedTimeStamp += (float)Game1.currentGameTime.ElapsedGameTime.TotalSeconds;
				Utility.drawWithShadow(b, this.titleButtonsTexture, new Vector2(base.width / 2 - 87 * TitleMenu.pixelZoom, base.height / 2 - 34 * TitleMenu.pixelZoom), new Rectangle((time / 200 % 2 == 0) ? 85 : 0, 559, 85, 69), Color.White * ((float)(this.fadeFromWhiteTimer - (2000 - this.fadeFromWhiteTimer)) / 2000f), 0f, Vector2.Zero, TitleMenu.pixelZoom, flipped: false, 0.2f, -4, -4, 0f);
			}
			else if (this.logoSurprisedTimer > 0)
			{
				b.Draw(this.titleButtonsTexture, new Vector2(base.width / 2 - 87 * TitleMenu.pixelZoom, base.height / 2 - 34 * TitleMenu.pixelZoom), new Rectangle((this.logoSurprisedTimer > 800 || this.logoSurprisedTimer < 400) ? 176 : 260, 375, 85, 69), Color.White * ((this.logoSurprisedTimer < 200) ? ((float)this.logoSurprisedTimer / 200f) : 1f), 0f, Vector2.Zero, TitleMenu.pixelZoom, SpriteEffects.None, 0.22f);
			}
			if (this.startupMessage.Length > 0 && this.logoFadeTimer > 0)
			{
				b.DrawString(Game1.smallFont, Game1.parseText(this.startupMessage, Game1.smallFont, 640), new Vector2(8f, (float)Game1.uiViewport.Height - Game1.smallFont.MeasureString(Game1.parseText(this.startupMessage, Game1.smallFont, 640)).Y - 4f), this.startupMessageColor * ((this.logoFadeTimer < 500) ? ((float)this.logoFadeTimer / 500f) : ((this.logoFadeTimer > 4500) ? (1f - (float)(this.logoFadeTimer - 4500) / 500f) : 1f)));
			}
		}
		if (this.quitTimer > 0)
		{
			b.Draw(Game1.staminaRect, new Rectangle(0, 0, base.width, base.height), Color.Black * (1f - (float)this.quitTimer / 500f));
		}
		if (this.HasActiveUser)
		{
			this.muteMusicButton.draw(b);
			this.windowedButton.draw(b);
		}
		if (this.ShouldDrawCursor())
		{
			int whichCursor = -1;
			if (TitleMenu.subMenu is LoadGameMenu)
			{
				whichCursor = ((TitleMenu.subMenu as LoadGameMenu).IsDoingTask() ? 1 : (-1));
			}
			base.drawMouse(b, ignore_transparency: false, whichCursor);
			if (this.cornerPhaseHolding && this.cornerClicks < 100)
			{
				b.Draw(Game1.mouseCursors2, new Vector2(Game1.getMouseX() + 32 + 4, Game1.getMouseY() + 32 + 4), new Rectangle(131, 196, 9, 10), Color.White, 0f, Vector2.Zero, zoom, SpriteEffects.None, 0.9999f);
			}
		}
	}

	protected bool ShouldAllowInteraction()
	{
		if (this.quitTimer > 0)
		{
			return false;
		}
		if (this.isTransitioningButtons)
		{
			return false;
		}
		if (this.showButtonsTimer > 0 && this.HasActiveUser && TitleMenu.subMenu == null)
		{
			return false;
		}
		if (TitleMenu.subMenu != null)
		{
			if (TitleMenu.subMenu is LoadGameMenu loadGameMenu && loadGameMenu.IsDoingTask())
			{
				return false;
			}
		}
		else if (!this.titleInPosition)
		{
			return false;
		}
		return true;
	}

	protected bool ShouldDrawCursor()
	{
		if (!Game1.options.gamepadControls || !Game1.options.snappyMenus)
		{
			return true;
		}
		if (this.pauseBeforeViewportRiseTimer > 0)
		{
			return false;
		}
		if (this.logoSwipeTimer > 0f)
		{
			return false;
		}
		if (this.logoFadeTimer > 0)
		{
			if (this._movedCursor)
			{
				return true;
			}
			return false;
		}
		if (this.fadeFromWhiteTimer > 0)
		{
			return false;
		}
		if (!this.titleInPosition)
		{
			return false;
		}
		if (this.viewportDY != 0f)
		{
			return false;
		}
		if (TitleMenu._subMenu is TooManyFarmsMenu)
		{
			return false;
		}
		if (!this.ShouldAllowInteraction())
		{
			return false;
		}
		return true;
	}

	/// <inheritdoc />
	public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
	{
		if (this.globalXOffset >= base.width)
		{
			this.globalXOffset = Game1.uiViewport.Width;
		}
		base.width = Game1.uiViewport.Width;
		base.height = Game1.uiViewport.Height;
		this.setUpIcons();
		TitleMenu.subMenu?.gameWindowSizeChanged(oldBounds, newBounds);
		this.backButton = new ClickableTextureComponent(this.menuContent.LoadString("Strings\\StringsFromCSFiles:TitleMenu.cs.11739"), new Rectangle(base.width + -66 * TitleMenu.pixelZoom - 8 * TitleMenu.pixelZoom * 2, base.height - 27 * TitleMenu.pixelZoom - 8 * TitleMenu.pixelZoom, 66 * TitleMenu.pixelZoom, 27 * TitleMenu.pixelZoom), null, "", this.titleButtonsTexture, new Rectangle(296, 252, 66, 27), TitleMenu.pixelZoom)
		{
			myID = 81114
		};
		this.tempSprites.Clear();
		if (this.birds.Count > 0 && !this.titleInPosition)
		{
			for (int i = 0; i < this.birds.Count; i++)
			{
				this.birds[i].position = ((i % 2 == 0) ? new Vector2(base.width - 70 * TitleMenu.pixelZoom, base.height - 120 * TitleMenu.pixelZoom) : new Vector2(base.width - 40 * TitleMenu.pixelZoom, base.height - 110 * TitleMenu.pixelZoom));
			}
		}
		this.windowedButton = new ClickableTextureComponent(new Rectangle(Game1.viewport.Width - 36 - 16, 16, 36, 36), Game1.mouseCursors, new Rectangle((Game1.options != null && !Game1.options.isCurrentlyWindowed()) ? 155 : 146, 384, 9, 9), 4f)
		{
			myID = 81112,
			leftNeighborID = 81111,
			downNeighborID = 81113
		};
		if (Game1.options.SnappyMenus)
		{
			int id = ((base.currentlySnappedComponent != null) ? base.currentlySnappedComponent.myID : 81115);
			this.populateClickableComponentList();
			base.currentlySnappedComponent = base.getComponentWithID(id);
			if (TitleMenu._subMenu != null)
			{
				TitleMenu._subMenu.snapCursorToCurrentSnappedComponent();
			}
			else
			{
				this.snapCursorToCurrentSnappedComponent();
			}
		}
	}

	private void showButterflies()
	{
		Game1.playSound("yoba");
		int zoom = (this.ShouldShrinkLogo() ? 2 : TitleMenu.pixelZoom);
		this.tempSprites.Add(new TemporaryAnimatedSprite("TileSheets\\critters", new Rectangle(128, 96, 16, 16), new Vector2(base.width / 2 - 240 * zoom, -300 * zoom - (int)(this.viewportY / 3f) * zoom + 86 * zoom), flipped: false, 0f, Color.White)
		{
			scale = zoom,
			animationLength = 4,
			totalNumberOfLoops = 999999,
			pingPong = true,
			interval = 75f,
			local = true,
			yPeriodic = true,
			yPeriodicLoopTime = 3200f,
			yPeriodicRange = 16f,
			xPeriodic = true,
			xPeriodicLoopTime = 5000f,
			xPeriodicRange = 21f,
			alpha = 0.001f,
			alphaFade = -0.03f
		});
		TemporaryAnimatedSpriteList l = Utility.sparkleWithinArea(new Rectangle(base.width / 2 - 240 * zoom - 8 * zoom, -300 * zoom - (int)(this.viewportY / 3f) * zoom + 86 * zoom - 8 * zoom, 80, 64), 2, Color.White * 0.75f);
		foreach (TemporaryAnimatedSprite item in l)
		{
			item.local = true;
			item.scale = (float)zoom / 4f;
		}
		this.tempSprites.AddRange(l);
		this.tempSprites.Add(new TemporaryAnimatedSprite("TileSheets\\critters", new Rectangle(192, 96, 16, 16), new Vector2(base.width / 2 + 220 * zoom, -300 * zoom - (int)(this.viewportY / 3f) * zoom + 15 * zoom), flipped: false, 0f, Color.White)
		{
			scale = zoom,
			animationLength = 4,
			totalNumberOfLoops = 999999,
			pingPong = true,
			delayBeforeAnimationStart = 10,
			interval = 70f,
			local = true,
			yPeriodic = true,
			yPeriodicLoopTime = 2800f,
			yPeriodicRange = 12f,
			xPeriodic = true,
			xPeriodicLoopTime = 4000f,
			xPeriodicRange = 16f,
			alpha = 0.001f,
			alphaFade = -0.03f
		});
		l = Utility.sparkleWithinArea(new Rectangle(base.width / 2 + 220 * zoom - 8 * zoom, -300 * zoom - (int)(this.viewportY / 3f) * zoom + 15 * zoom - 8 * zoom, 80, 64), 2, Color.White * 0.75f);
		foreach (TemporaryAnimatedSprite item2 in l)
		{
			item2.local = true;
			item2.scale = (float)zoom / 4f;
		}
		this.tempSprites.AddRange(l);
		this.tempSprites.Add(new TemporaryAnimatedSprite("TileSheets\\critters", new Rectangle(256, 96, 16, 16), new Vector2(base.width / 2 - 250 * zoom, -300 * zoom - (int)(this.viewportY / 3f) * zoom + 35 * zoom), flipped: false, 0f, Color.White)
		{
			scale = zoom,
			animationLength = 4,
			totalNumberOfLoops = 999999,
			pingPong = true,
			delayBeforeAnimationStart = 20,
			interval = 65f,
			local = true,
			yPeriodic = true,
			yPeriodicLoopTime = 3500f,
			yPeriodicRange = 16f,
			xPeriodic = true,
			xPeriodicLoopTime = 3000f,
			xPeriodicRange = 10f,
			alpha = 0.001f,
			alphaFade = -0.03f
		});
		l = Utility.sparkleWithinArea(new Rectangle(base.width / 2 - 250 * zoom - 8 * zoom, -300 * zoom - (int)(this.viewportY / 3f) * zoom + 35 * zoom - 8 * zoom, 80, 64), 2, Color.White * 0.75f);
		foreach (TemporaryAnimatedSprite item3 in l)
		{
			item3.local = true;
			item3.scale = (float)zoom / 4f;
		}
		this.tempSprites.AddRange(l);
		this.tempSprites.Add(new TemporaryAnimatedSprite("TileSheets\\critters", new Rectangle(256, 112, 16, 16), new Vector2(base.width / 2 + 250 * zoom, -300 * zoom - (int)(this.viewportY / 3f) * zoom + 60 * zoom), flipped: false, 0f, Color.White)
		{
			scale = zoom,
			animationLength = 4,
			totalNumberOfLoops = 999999,
			yPeriodic = true,
			yPeriodicLoopTime = 3000f,
			yPeriodicRange = 16f,
			pingPong = true,
			delayBeforeAnimationStart = 30,
			interval = 85f,
			local = true,
			xPeriodic = true,
			xPeriodicLoopTime = 5000f,
			xPeriodicRange = 16f,
			alpha = 0.001f,
			alphaFade = -0.03f
		});
		l = Utility.sparkleWithinArea(new Rectangle(base.width / 2 + 250 * zoom - 8 * zoom, -300 * zoom - (int)(this.viewportY / 3f) * zoom + 60 * zoom - 8 * zoom, 80, 64), 2, Color.White * 0.75f);
		foreach (TemporaryAnimatedSprite item4 in l)
		{
			item4.local = true;
			item4.scale = (float)zoom / 4f;
		}
		this.tempSprites.AddRange(l);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (this.disposedValue)
		{
			return;
		}
		if (disposing)
		{
			this.tempSprites?.Clear();
			if (this.menuContent != null)
			{
				this.menuContent.Dispose();
				this.menuContent = null;
			}
			LocalizedContentManager.OnLanguageChange -= OnLanguageChange;
			TitleMenu.subMenu = null;
		}
		this.disposedValue = true;
	}

	~TitleMenu()
	{
		this.Dispose(disposing: false);
	}

	public void Dispose()
	{
		this.Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}
