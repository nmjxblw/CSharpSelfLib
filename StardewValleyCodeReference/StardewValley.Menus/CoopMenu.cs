using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using StardewValley.GameData;
using StardewValley.Network;
using StardewValley.SDKs;

namespace StardewValley.Menus;

public class CoopMenu : LoadGameMenu
{
	public enum Tab
	{
		JOIN_TAB,
		HOST_TAB
	}

	protected abstract class CoopMenuSlot : MenuSlot
	{
		protected new CoopMenu menu;

		public CoopMenuSlot(CoopMenu menu)
			: base(menu)
		{
			this.menu = menu;
		}
	}

	protected abstract class LabeledSlot : CoopMenuSlot
	{
		private string message;

		public LabeledSlot(CoopMenu menu, string message)
			: base(menu)
		{
			this.message = message;
		}

		public abstract override void Activate();

		public override void Draw(SpriteBatch b, int i)
		{
			int strWidth = SpriteText.getWidthOfString(this.message);
			int strHeight = SpriteText.getHeightOfString(this.message);
			Rectangle bounds = base.menu.slotButtons[i].bounds;
			int x = bounds.X + (bounds.Width - strWidth) / 2;
			int y = bounds.Y + (bounds.Height - strHeight) / 2;
			SpriteText.drawString(b, this.message, x, y);
		}
	}

	protected class LanSlot : LabeledSlot
	{
		public LanSlot(CoopMenu menu)
			: base(menu, Game1.content.LoadString("Strings\\UI:CoopMenu_JoinLANGame"))
		{
		}

		public override void Activate()
		{
			base.menu.enterIPPressed();
		}
	}

	protected class InviteCodeSlot : LabeledSlot
	{
		public InviteCodeSlot(CoopMenu menu)
			: base(menu, Game1.content.LoadString("Strings\\UI:CoopMenu_EnterInviteCode"))
		{
		}

		public override void Activate()
		{
			base.menu.enterInviteCodePressed();
		}
	}

	protected class HostNewFarmSlot : LabeledSlot
	{
		private bool _multiplayer;

		public HostNewFarmSlot(CoopMenu menu, bool multiplayer)
			: base(menu, Game1.content.LoadString("Strings\\UI:CoopMenu_HostNewFarm"))
		{
			base.ActivateDelay = 2150;
			this._multiplayer = multiplayer;
		}

		public override void Activate()
		{
			Game1.resetPlayer();
			TitleMenu.subMenu = new CharacterCustomization(CharacterCustomization.Source.HostNewFarm, this._multiplayer);
			Game1.changeMusicTrack("CloudCountry");
		}
	}

	protected class TooManyFarmsSlot : LabeledSlot
	{
		public TooManyFarmsSlot(CoopMenu menu)
			: base(menu, Game1.content.LoadString("Strings\\UI:TooManyFarmsMenu_TooManyFarms"))
		{
		}

		public override void Activate()
		{
		}
	}

	protected class HostFileSlot : SaveFileSlot
	{
		protected new CoopMenu menu;

		private bool _multiplayer;

		public HostFileSlot(CoopMenu menu, bool multiplayer, Farmer farmer)
			: base(menu, farmer, null)
		{
			this.menu = menu;
			this._multiplayer = multiplayer;
		}

		public override void Activate()
		{
			Game1.multiplayerMode = (byte)(this._multiplayer ? 2 : 0);
			base.Activate();
		}

		protected override void drawSlotSaveNumber(SpriteBatch b, int i)
		{
		}

		protected override string slotName()
		{
			return Game1.content.LoadString("Strings\\UI:CoopMenu_HostFile", base.Farmer.Name, base.Farmer.farmName.Value);
		}

		protected override string slotSubName()
		{
			return base.Farmer.Name;
		}

		protected override Vector2 portraitOffset()
		{
			return base.portraitOffset() - new Vector2(32f, 0f);
		}
	}

	protected class FriendFarmData
	{
		public object Lobby;

		public string OwnerName;

		public string FarmName;

		public int FarmType;

		public ModFarmType ModFarmType;

		public WorldDate Date;

		public bool PreviouslyJoined;

		public string ProtocolVersion;
	}

	protected class FriendFarmSlot : CoopMenuSlot
	{
		public FriendFarmData Farm;

		public FriendFarmSlot(CoopMenu menu, FriendFarmData farm)
			: base(menu)
		{
			this.Farm = farm;
		}

		public bool MatchAddress(object Lobby)
		{
			return object.Equals(this.Farm.Lobby, Lobby);
		}

		public void Update(FriendFarmData newData)
		{
			this.Farm = newData;
		}

		public override void Activate()
		{
			base.menu.setMenu(new FarmhandMenu(Program.sdk.Networking.CreateClient(this.Farm.Lobby)));
		}

		protected virtual string slotName()
		{
			string messageKey = (this.Farm.PreviouslyJoined ? "Strings\\UI:CoopMenu_RevisitFriendFarm" : "Strings\\UI:CoopMenu_JoinFriendFarm");
			return Game1.content.LoadString(messageKey, this.Farm.FarmName);
		}

		protected virtual void drawSlotName(SpriteBatch b, int i)
		{
			SpriteText.drawString(b, this.slotName(), base.menu.slotButtons[i].bounds.X + 128 + 36, base.menu.slotButtons[i].bounds.Y + 36);
		}

		protected virtual void drawSlotDate(SpriteBatch b, int i)
		{
			Utility.drawTextWithShadow(b, this.Farm.Date.Localize(), Game1.dialogueFont, new Vector2(base.menu.slotButtons[i].bounds.X + 128 + 32, base.menu.slotButtons[i].bounds.Y + 64 + 40), Game1.textColor);
		}

		protected virtual void drawSlotFarm(SpriteBatch b, int i)
		{
			int drawn_farm_type = this.Farm.FarmType;
			if (drawn_farm_type == 7)
			{
				drawn_farm_type = 0;
			}
			Rectangle sourceRect = new Rectangle(22 * (drawn_farm_type % 5), 324 + 21 * (drawn_farm_type / 5), 22, 20);
			Texture2D texture = Game1.mouseCursors;
			Rectangle space = new Rectangle(base.menu.slotButtons[i].bounds.X, base.menu.slotButtons[i].bounds.Y, 160, base.menu.slotButtons[i].bounds.Height);
			Rectangle destRect = new Rectangle(space.X + (space.Width - sourceRect.Width * 4) / 2, space.Y + (space.Height - sourceRect.Height * 4) / 2, sourceRect.Width * 4, sourceRect.Height * 4);
			if (this.Farm.ModFarmType?.IconTexture != null)
			{
				texture = Game1.content.Load<Texture2D>(this.Farm.ModFarmType.IconTexture);
				b.Draw(texture, destRect, null, Color.White);
			}
			else
			{
				b.Draw(texture, destRect, sourceRect, Color.White);
			}
		}

		protected virtual void drawSlotOwnerName(SpriteBatch b, int i)
		{
			float scale = 1f;
			float x_pos_offset = 128f;
			float y_pos_offset = 44f;
			Utility.drawTextWithShadow(b, this.Farm.OwnerName, Game1.dialogueFont, new Vector2((float)(base.menu.slotButtons[i].bounds.X + base.menu.width) - x_pos_offset - Game1.dialogueFont.MeasureString(this.Farm.OwnerName).X * scale, (float)base.menu.slotButtons[i].bounds.Y + y_pos_offset), Game1.textColor, scale);
		}

		public override void Draw(SpriteBatch b, int i)
		{
			this.drawSlotName(b, i);
			this.drawSlotDate(b, i);
			this.drawSlotFarm(b, i);
			this.drawSlotOwnerName(b, i);
		}
	}

	public class LobbyUpdateCallback : LobbyUpdateListener
	{
		private Action<object> callback;

		public LobbyUpdateCallback(Action<object> callback)
		{
			this.callback = callback;
		}

		public void OnLobbyUpdate(object lobby)
		{
			this.callback?.Invoke(lobby);
		}
	}

	public const int region_refresh = 810;

	public const int region_joinTab = 811;

	public const int region_hostTab = 812;

	public const int region_tabs = 1000;

	protected List<MenuSlot> hostSlots = new List<MenuSlot>();

	public ClickableComponent refreshButton;

	public ClickableComponent joinTab;

	public ClickableComponent hostTab;

	private LobbyUpdateListener lobbyUpdateListener;

	public Tab currentTab;

	private bool smallScreenFormat;

	private bool isSetUp;

	private int updateCounter;

	private string Filter;

	private float _refreshDelay = -1f;

	public bool tooManyFarms;

	private readonly bool _splitScreen;

	public static string lastEnteredInviteCode;

	private StringBuilder _stringBuilder = new StringBuilder();

	public override List<MenuSlot> MenuSlots
	{
		get
		{
			if (this._splitScreen)
			{
				return this.hostSlots;
			}
			return this.currentTab switch
			{
				Tab.JOIN_TAB => base.menuSlots, 
				Tab.HOST_TAB => this.hostSlots, 
				_ => null, 
			};
		}
		set
		{
			if (this._splitScreen)
			{
				this.hostSlots = value;
				return;
			}
			switch (this.currentTab)
			{
			case Tab.JOIN_TAB:
				base.menuSlots = value;
				break;
			case Tab.HOST_TAB:
				this.hostSlots = value;
				break;
			}
		}
	}

	public CoopMenu(bool tooManyFarms, bool splitScreen = false, Tab initialTab = Tab.JOIN_TAB, string filter = null)
	{
		this.tooManyFarms = tooManyFarms;
		this.currentTab = initialTab;
		this.Filter = filter;
		this._splitScreen = splitScreen;
	}

	public override bool readyToClose()
	{
		if (this.isSetUp)
		{
			return base.readyToClose();
		}
		return true;
	}

	protected override bool hasDeleteButtons()
	{
		return false;
	}

	/// <inheritdoc />
	protected override void startListPopulation(string filter)
	{
	}

	protected virtual void connectionFinished()
	{
		string label = Game1.content.LoadString("Strings\\UI:CoopMenu_Refresh");
		int width = (int)Game1.dialogueFont.MeasureString(label).X + 64;
		Vector2 pos = new Vector2(base.backButton.bounds.Right - width, base.backButton.bounds.Y - 128);
		this.refreshButton = new ClickableComponent(new Rectangle((int)pos.X, (int)pos.Y, width, 96), "", label)
		{
			myID = 810,
			upNeighborID = -99998,
			leftNeighborID = -99998,
			rightNeighborID = -99998,
			downNeighborID = 81114
		};
		this._refreshDelay = 8f;
		this.smallScreenFormat = Game1.graphics.GraphicsDevice.Viewport.Height < 1080;
		label = Game1.content.LoadString("Strings\\UI:CoopMenu_Join");
		width = (int)Game1.dialogueFont.MeasureString(label).X + 64;
		pos = (this.smallScreenFormat ? new Vector2(base.xPositionOnScreen, base.yPositionOnScreen) : new Vector2(base.xPositionOnScreen + IClickableMenu.borderWidth, base.yPositionOnScreen - 96));
		this.joinTab = new ClickableComponent(new Rectangle((int)pos.X, (int)pos.Y, width, this.smallScreenFormat ? 72 : 64), "", label)
		{
			myID = 811,
			downNeighborID = -99998,
			rightNeighborID = 812,
			region = 1000
		};
		label = Game1.content.LoadString("Strings\\UI:CoopMenu_Host");
		width = (int)Game1.dialogueFont.MeasureString(label).X + 64;
		pos = (this.smallScreenFormat ? new Vector2(this.joinTab.bounds.Right + ((!this.smallScreenFormat) ? 4 : 0), base.yPositionOnScreen) : new Vector2(this.joinTab.bounds.Right + 4, base.yPositionOnScreen - 64));
		this.hostTab = new ClickableComponent(new Rectangle((int)pos.X, (int)pos.Y, width, this.smallScreenFormat ? 72 : 64), "", label)
		{
			myID = 812,
			downNeighborID = -99998,
			leftNeighborID = 811,
			rightNeighborID = 800,
			region = 1000
		};
		base.backButton.upNeighborID = 810;
		if (this.tooManyFarms)
		{
			this.hostSlots.Add(new TooManyFarmsSlot(this));
		}
		else
		{
			this.hostSlots.Add(new HostNewFarmSlot(this, !this._splitScreen));
		}
		if (this._splitScreen)
		{
			this.refreshButton.visible = false;
			this.joinTab.visible = false;
			this.hostTab.visible = false;
			base.backButton.upNeighborID = 0;
		}
		else
		{
			base.menuSlots.Add(new LanSlot(this));
			if (Program.sdk.Networking != null && Program.sdk.Networking.SupportsInviteCodes())
			{
				base.menuSlots.Add(new InviteCodeSlot(this));
			}
			this.SetTab(this.currentTab, playSound: false);
		}
		this.isSetUp = true;
		Game1.mouseCursor = 0;
		base.startListPopulation(this.Filter);
		this.populateClickableComponentList();
	}

	/// <inheritdoc />
	public override void receiveGamePadButton(Buttons button)
	{
		base.receiveGamePadButton(button);
		if (!this.isSetUp || base.IsDoingTask())
		{
			return;
		}
		switch (button)
		{
		case Buttons.LeftTrigger:
		{
			ClickableComponent clickableComponent2 = this.joinTab;
			if (clickableComponent2 != null && clickableComponent2.visible)
			{
				this.SetTab(Tab.JOIN_TAB);
				this.setCurrentlySnappedComponentTo(this.joinTab.myID);
				this.snapCursorToCurrentSnappedComponent();
			}
			break;
		}
		case Buttons.RightTrigger:
		{
			ClickableComponent clickableComponent = this.hostTab;
			if (clickableComponent != null && clickableComponent.visible)
			{
				this.SetTab(Tab.HOST_TAB);
				this.setCurrentlySnappedComponentTo(this.hostTab.myID);
				this.snapCursorToCurrentSnappedComponent();
			}
			break;
		}
		}
	}

	public override void UpdateButtons()
	{
		base.UpdateButtons();
		if (this._splitScreen)
		{
			return;
		}
		foreach (ClickableComponent c in base.slotButtons)
		{
			if (c.myID == 0)
			{
				if (base.currentItemIndex == 0)
				{
					c.upNeighborID = 811;
				}
				else
				{
					c.upNeighborID = -7777;
				}
			}
		}
	}

	/// <inheritdoc />
	public override void update(GameTime time)
	{
		float elapsed = (float)time.ElapsedGameTime.TotalSeconds;
		this.updateCounter++;
		if (!this.isSetUp)
		{
			if (this._splitScreen)
			{
				if (this.updateCounter > 1)
				{
					this.connectionFinished();
				}
			}
			else if (Program.sdk.ConnectionFinished)
			{
				this.connectionFinished();
			}
			else
			{
				Game1.mouseCursor = 1;
			}
		}
		else
		{
			if (this.refreshButton != null && this.refreshButton.visible && this._refreshDelay > 0f)
			{
				this._refreshDelay -= elapsed;
			}
			base.update(time);
		}
	}

	/// <inheritdoc />
	public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
	{
		base.gameWindowSizeChanged(oldBounds, newBounds);
		if (this.joinTab != null && this.hostTab != null && base.backButton != null && this.refreshButton != null)
		{
			this.smallScreenFormat = Game1.graphics.GraphicsDevice.Viewport.Height < 1080;
			string label = Game1.content.LoadString("Strings\\UI:CoopMenu_Join");
			Vector2 pos = (this.smallScreenFormat ? new Vector2(base.xPositionOnScreen, base.yPositionOnScreen) : new Vector2(base.xPositionOnScreen + IClickableMenu.borderWidth, base.yPositionOnScreen - 96));
			this.joinTab.bounds.X = (int)pos.X;
			this.joinTab.bounds.Y = (int)pos.Y;
			label = Game1.content.LoadString("Strings\\UI:CoopMenu_Host");
			pos = (this.smallScreenFormat ? new Vector2(this.joinTab.bounds.Right + ((!this.smallScreenFormat) ? 4 : 0), base.yPositionOnScreen) : new Vector2(this.joinTab.bounds.Right + 4, base.yPositionOnScreen - 64));
			this.hostTab.bounds.X = (int)pos.X;
			this.hostTab.bounds.Y = (int)pos.Y;
			label = Game1.content.LoadString("Strings\\UI:CoopMenu_Refresh");
			int width = (int)Game1.dialogueFont.MeasureString(label).X + 64;
			pos = new Vector2(base.backButton.bounds.Right - width, base.backButton.bounds.Y - 128);
			this.refreshButton.bounds.X = (int)pos.X;
			this.refreshButton.bounds.Y = (int)pos.Y;
		}
	}

	protected override void saveFileScanComplete()
	{
		if (!this._splitScreen && Program.sdk.Networking != null)
		{
			this.lobbyUpdateListener = new LobbyUpdateCallback(onLobbyUpdate);
			Program.sdk.Networking.AddLobbyUpdateListener(this.lobbyUpdateListener);
			Program.sdk.Networking.RequestFriendLobbyData();
		}
	}

	protected virtual FriendFarmData readLobbyFarmData(object lobby)
	{
		FriendFarmData farm = new FriendFarmData
		{
			Lobby = lobby,
			Date = new WorldDate()
		};
		farm.OwnerName = Program.sdk.Networking.GetLobbyOwnerName(lobby);
		farm.FarmName = Program.sdk.Networking.GetLobbyData(lobby, "farmName");
		string farmType = Program.sdk.Networking.GetLobbyData(lobby, "farmType");
		string mod_farm_type = Program.sdk.Networking.GetLobbyData(lobby, "modFarmType");
		string lobbyData = Program.sdk.Networking.GetLobbyData(lobby, "date");
		int farmType_i = Convert.ToInt32(farmType);
		int farmDate_day = Convert.ToInt32(lobbyData);
		farm.FarmType = farmType_i;
		farm.ModFarmType = null;
		if (!string.IsNullOrEmpty(mod_farm_type))
		{
			List<ModFarmType> farm_types = DataLoader.AdditionalFarms(Game1.content);
			if (farm_types != null)
			{
				foreach (ModFarmType farm_type in farm_types)
				{
					if (farm_type.Id == mod_farm_type)
					{
						farm.ModFarmType = farm_type;
						break;
					}
				}
			}
		}
		farm.Date.TotalDays = farmDate_day;
		farm.ProtocolVersion = Program.sdk.Networking.GetLobbyData(lobby, "protocolVersion");
		farm.FarmName = Program.sdk.FilterDirtyWords(farm.FarmName);
		farm.OwnerName = Program.sdk.FilterDirtyWords(farm.OwnerName);
		return farm;
	}

	protected virtual bool checkFriendFarmCompatibility(FriendFarmData farm)
	{
		if (farm.FarmType < 0 || farm.FarmType > 7)
		{
			return false;
		}
		if (farm.ProtocolVersion != Multiplayer.protocolVersion)
		{
			return false;
		}
		return true;
	}

	protected virtual void onLobbyUpdate(object lobby)
	{
		try
		{
			string protocolVersion = Program.sdk.Networking.GetLobbyData(lobby, "protocolVersion");
			if (protocolVersion != Multiplayer.protocolVersion)
			{
				return;
			}
			Game1.log.Verbose("Receiving friend lobby data...\nOwner: " + Program.sdk.Networking.GetLobbyOwnerName(lobby) + "\nfarmName = " + Program.sdk.Networking.GetLobbyData(lobby, "farmName") + "\nfarmType = " + Program.sdk.Networking.GetLobbyData(lobby, "farmType") + "\ndate = " + Program.sdk.Networking.GetLobbyData(lobby, "date") + "\nprotocolVersion = " + protocolVersion + "\nfarmhands = " + Program.sdk.Networking.GetLobbyData(lobby, "farmhands") + "\nnewFarmhands = " + Program.sdk.Networking.GetLobbyData(lobby, "newFarmhands"));
			FriendFarmData farm = this.readLobbyFarmData(lobby);
			if (!this.checkFriendFarmCompatibility(farm) || (farm.FarmType == 7 && farm.ModFarmType == null))
			{
				return;
			}
			string selfID = Program.sdk.Networking.GetUserID();
			string farmhands = Program.sdk.Networking.GetLobbyData(lobby, "farmhands");
			bool newFarmhands = Convert.ToBoolean(Program.sdk.Networking.GetLobbyData(lobby, "newFarmhands"));
			if (farmhands == "" && !newFarmhands)
			{
				return;
			}
			string[] farmUsers = farmhands.Split(',');
			if (!farmUsers.Contains(selfID) && !newFarmhands)
			{
				return;
			}
			farm.PreviouslyJoined = farmUsers.Contains(selfID);
			if (base.menuSlots == null)
			{
				return;
			}
			foreach (MenuSlot menuSlot in base.menuSlots)
			{
				if (menuSlot is FriendFarmSlot farmSlot && farmSlot.MatchAddress(lobby))
				{
					farmSlot.Update(farm);
					return;
				}
			}
			base.menuSlots.Add(new FriendFarmSlot(this, farm));
			this.UpdateButtons();
			this.populateClickableComponentList();
		}
		catch (FormatException)
		{
		}
		catch (OverflowException)
		{
		}
	}

	public override bool IsAutomaticSnapValid(int direction, ClickableComponent a, ClickableComponent b)
	{
		if (a.region == 1000 && (direction == 2 || direction == 0) && b.region == 1000)
		{
			return false;
		}
		if (a.myID == 810 && direction == 0 && b.region != 900)
		{
			return false;
		}
		if (a.myID == 810 && direction == 1 && b.myID == 81114)
		{
			return false;
		}
		return base.IsAutomaticSnapValid(direction, a, b);
	}

	protected override void addSaveFiles(List<Farmer> files)
	{
		this.hostSlots.AddRange(files.Where((Farmer file) => file.slotCanHost).Select((Func<Farmer, MenuSlot>)((Farmer file) => new HostFileSlot(this, !this._splitScreen, file))));
		this.UpdateButtons();
	}

	protected virtual void setMenu(IClickableMenu menu)
	{
		if (Game1.activeClickableMenu is TitleMenu)
		{
			TitleMenu.subMenu = menu;
		}
		else
		{
			Game1.activeClickableMenu = menu;
		}
	}

	private void enterIPPressed()
	{
		string last_entered_ip = "";
		try
		{
			StartupPreferences startupPreferences = new StartupPreferences();
			startupPreferences.loadPreferences(async: false, applyLanguage: false);
			last_entered_ip = startupPreferences.lastEnteredIP;
		}
		catch (Exception)
		{
		}
		string title = Game1.content.LoadString("Strings\\UI:CoopMenu_EnterIP");
		this.setMenu(new TitleTextInputMenu(title, delegate(string address)
		{
			try
			{
				StartupPreferences startupPreferences2 = new StartupPreferences();
				startupPreferences2.loadPreferences(async: false, applyLanguage: false);
				startupPreferences2.lastEnteredIP = address;
				startupPreferences2.savePreferences(async: false);
			}
			catch (Exception)
			{
			}
			if (address == "")
			{
				address = "localhost";
			}
			this.setMenu(new FarmhandMenu(Game1.multiplayer.InitClient(new LidgrenClient(address))));
		}, last_entered_ip, "join_menu", filterInput: false));
	}

	private void enterInviteCodePressed()
	{
		if (Program.sdk.Networking == null || !Program.sdk.Networking.SupportsInviteCodes())
		{
			return;
		}
		string title = Game1.content.LoadString("Strings\\UI:CoopMenu_EnterInviteCode");
		this.setMenu(new TitleTextInputMenu(title, delegate(string code)
		{
			CoopMenu.lastEnteredInviteCode = code;
			object lobbyFromInviteCode = Program.sdk.Networking.GetLobbyFromInviteCode(code);
			if (lobbyFromInviteCode != null)
			{
				Client client = Program.sdk.Networking.CreateClient(lobbyFromInviteCode);
				this.setMenu(new FarmhandMenu(client));
			}
		}, CoopMenu.lastEnteredInviteCode, "join_menu", filterInput: false));
	}

	private bool tabClick(int x, int y)
	{
		if (this.joinTab.visible && this.joinTab.containsPoint(x, y))
		{
			this.SetTab(Tab.JOIN_TAB);
			return true;
		}
		if (this.hostTab.visible && this.hostTab.containsPoint(x, y))
		{
			this.SetTab(Tab.HOST_TAB);
			return true;
		}
		return false;
	}

	public virtual void SetTab(Tab newTab, bool playSound = true)
	{
		if (this.currentTab == newTab)
		{
			return;
		}
		this.currentTab = newTab;
		if (!this.smallScreenFormat && this.isSetUp)
		{
			if (this.currentTab == Tab.HOST_TAB)
			{
				this.hostTab.bounds.Y = base.yPositionOnScreen - 96;
				this.joinTab.bounds.Y = base.yPositionOnScreen - 64;
			}
			else
			{
				this.hostTab.bounds.Y = base.yPositionOnScreen - 64;
				this.joinTab.bounds.Y = base.yPositionOnScreen - 96;
			}
		}
		if (playSound)
		{
			Game1.playSound("smallSelect");
		}
		if (this.isSetUp)
		{
			this.UpdateButtons();
		}
		base.currentItemIndex = 0;
	}

	/// <inheritdoc />
	public override void receiveLeftClick(int x, int y, bool playSound = true)
	{
		if (!this.isSetUp)
		{
			return;
		}
		if (this.refreshButton.visible && this.refreshButton.containsPoint(x, y))
		{
			if (this._refreshDelay < 0f)
			{
				Game1.playSound("bigDeSelect");
				this.setMenu(new CoopMenu(this.tooManyFarms, this._splitScreen));
			}
		}
		else if (!this.smallScreenFormat || !this.tabClick(x, y))
		{
			base.receiveLeftClick(x, y, playSound);
			if (!this.smallScreenFormat && !base.loading)
			{
				this.tabClick(x, y);
			}
		}
	}

	/// <inheritdoc />
	public override void performHoverAction(int x, int y)
	{
		if (this.isSetUp)
		{
			if (this.refreshButton.visible && this.refreshButton.containsPoint(x, y))
			{
				this.refreshButton.scale = 1f;
			}
			else
			{
				this.refreshButton.scale = 0f;
			}
			if (this.smallScreenFormat && (this.hostTab.containsPoint(x, y) || this.joinTab.containsPoint(x, y)))
			{
				base.performHoverAction(-100, -100);
			}
			else
			{
				base.performHoverAction(x, y);
			}
		}
	}

	protected override string getStatusText()
	{
		return null;
	}

	private void drawTabs(SpriteBatch b)
	{
		if (!this._splitScreen && this.isSetUp)
		{
			Color selectColor = (this.smallScreenFormat ? Color.Orange : new Color(255, 255, 150));
			Color hoverColor = Color.Yellow;
			Color selectShadow = (this.smallScreenFormat ? Color.DarkOrange : Game1.textShadowDarkerColor);
			Color hoverShadow = Color.DarkGoldenrod;
			if (this.joinTab.visible)
			{
				bool colorSelect = this.currentTab == Tab.JOIN_TAB;
				bool colorHover = this.currentTab != Tab.JOIN_TAB && this.joinTab.containsPoint(Game1.getMouseX(), Game1.getMouseY());
				IClickableMenu.drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60), this.joinTab.bounds.X, this.joinTab.bounds.Y, this.joinTab.bounds.Width, this.joinTab.bounds.Height + ((!this.smallScreenFormat) ? 64 : 0), colorSelect ? selectColor : (colorHover ? hoverColor : Color.White), 1f, drawShadow: false);
				Utility.drawTextWithColoredShadow(b, this.joinTab.label, Game1.dialogueFont, new Vector2(this.joinTab.bounds.Center.X, this.joinTab.bounds.Y + 40) - Game1.dialogueFont.MeasureString(this.joinTab.label) / 2f, Game1.textColor, colorHover ? hoverShadow : (colorSelect ? selectShadow : Game1.textShadowDarkerColor), 1.01f);
			}
			if (this.hostTab.visible)
			{
				bool colorSelect2 = this.currentTab == Tab.HOST_TAB;
				bool colorHover2 = this.currentTab != Tab.HOST_TAB && this.hostTab.containsPoint(Game1.getMouseX(), Game1.getMouseY());
				IClickableMenu.drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60), this.hostTab.bounds.X, this.hostTab.bounds.Y, this.hostTab.bounds.Width, this.hostTab.bounds.Height + ((!this.smallScreenFormat) ? 64 : 0), colorSelect2 ? selectColor : (colorHover2 ? hoverColor : Color.White), 1f, drawShadow: false);
				Utility.drawTextWithColoredShadow(b, this.hostTab.label, Game1.dialogueFont, new Vector2(this.hostTab.bounds.Center.X, this.hostTab.bounds.Y + 40) - Game1.dialogueFont.MeasureString(this.hostTab.label) / 2f, Game1.textColor, colorHover2 ? hoverShadow : (colorSelect2 ? selectShadow : Game1.textShadowDarkerColor), 1.01f);
			}
		}
	}

	public override void snapToDefaultClickableComponent()
	{
		base.snapToDefaultClickableComponent();
		if (base.currentlySnappedComponent == null)
		{
			if (!this._splitScreen)
			{
				base.currentlySnappedComponent = base.getComponentWithID(811);
			}
			this.snapCursorToCurrentSnappedComponent();
		}
	}

	protected override void drawBefore(SpriteBatch b)
	{
		base.drawBefore(b);
		if (this.isSetUp && !this.smallScreenFormat)
		{
			this.drawTabs(b);
		}
	}

	protected override void drawExtra(SpriteBatch b)
	{
		base.drawExtra(b);
		if (!this.isSetUp)
		{
			return;
		}
		if (this.refreshButton.visible)
		{
			Color color = ((this.refreshButton.scale > 0f) ? Color.Wheat : Color.White);
			if (this._refreshDelay > 0f)
			{
				color = Color.Gray;
			}
			IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(432, 439, 9, 9), this.refreshButton.bounds.X, this.refreshButton.bounds.Y, this.refreshButton.bounds.Width, this.refreshButton.bounds.Height, color, 4f);
			Utility.drawTextWithShadow(b, this.refreshButton.label, Game1.dialogueFont, new Vector2(this.refreshButton.bounds.Center.X, this.refreshButton.bounds.Center.Y + 4) - Game1.dialogueFont.MeasureString(this.refreshButton.label) / 2f, Game1.textColor, 1f, -1f, -1, -1, 0f);
		}
		if (this.smallScreenFormat)
		{
			this.drawTabs(b);
		}
	}

	protected override void drawStatusText(SpriteBatch b)
	{
		if (this._splitScreen)
		{
			return;
		}
		if (this.getStatusText() != null)
		{
			base.drawStatusText(b);
		}
		else if (!this.isSetUp)
		{
			int maxEllipsis = 1 + Program.sdk.ConnectionProgress;
			int ellipsisCount = this.updateCounter / 5 % maxEllipsis;
			string basicText = Game1.content.LoadString("Strings\\UI:CoopMenu_ConnectingOnlineServices");
			this._stringBuilder.Clear();
			this._stringBuilder.Append(basicText);
			for (int i = 0; i < ellipsisCount; i++)
			{
				this._stringBuilder.Append(".");
			}
			string currentText = this._stringBuilder.ToString();
			for (int j = ellipsisCount; j < maxEllipsis; j++)
			{
				this._stringBuilder.Append(".");
			}
			int maxWidth = SpriteText.getWidthOfString(this._stringBuilder.ToString());
			SpriteText.drawString(b, currentText, Game1.graphics.GraphicsDevice.Viewport.Bounds.Center.X - maxWidth / 2, Game1.graphics.GraphicsDevice.Viewport.Bounds.Center.Y);
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (!this._splitScreen)
		{
			if (this.lobbyUpdateListener != null && Program.sdk.Networking != null)
			{
				Program.sdk.Networking.RemoveLobbyUpdateListener(this.lobbyUpdateListener);
			}
			this.lobbyUpdateListener = null;
		}
		base.Dispose(disposing);
	}
}
