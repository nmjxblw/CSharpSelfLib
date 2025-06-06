using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using StardewValley.Network;

namespace StardewValley.Menus;

public class FarmhandMenu : LoadGameMenu
{
	public class FarmhandSlot : SaveFileSlot
	{
		protected new FarmhandMenu menu;

		protected bool _belongsToAnotherPlayer;

		public bool BelongsToAnotherPlayer()
		{
			if (Game1.game1 != null && !Game1.game1.IsMainInstance)
			{
				return false;
			}
			return this._belongsToAnotherPlayer;
		}

		public FarmhandSlot(FarmhandMenu menu, Farmer farmer)
			: base(menu, farmer, null)
		{
			this.menu = menu;
			if (Program.sdk.Networking != null)
			{
				string local_user_id = Program.sdk.Networking.GetUserID();
				if (local_user_id != "" && farmer != null && farmer.userID.Value != "" && local_user_id != farmer.userID.Value)
				{
					this._belongsToAnotherPlayer = true;
				}
			}
		}

		public override void Activate()
		{
			if (this.menu.client != null)
			{
				Game1.game1.loadForNewGame();
				Game1.player = base.Farmer;
				this.menu.client.availableFarmhands = null;
				this.menu.client.sendPlayerIntroduction();
				this.menu.approvingFarmhand = true;
				this.menu.menuSlots.Clear();
				Game1.gameMode = 6;
			}
		}

		public override float getSlotAlpha()
		{
			if (this.BelongsToAnotherPlayer())
			{
				return 0.5f;
			}
			return base.getSlotAlpha();
		}

		protected override void drawSlotName(SpriteBatch b, int i)
		{
			if (base.Farmer.isCustomized.Value)
			{
				base.drawSlotName(b, i);
				return;
			}
			string slotName = Game1.content.LoadString("Strings\\UI:CoopMenu_NewFarmhand");
			SpriteText.drawString(b, slotName, this.menu.slotButtons[i].bounds.X + 128 + 36, this.menu.slotButtons[i].bounds.Y + 36);
		}

		protected override void drawSlotShadow(SpriteBatch b, int i)
		{
			if (base.Farmer.isCustomized.Value)
			{
				base.drawSlotShadow(b, i);
			}
		}

		protected override void drawSlotFarmer(SpriteBatch b, int i)
		{
			if (base.Farmer.isCustomized.Value)
			{
				base.drawSlotFarmer(b, i);
			}
		}

		protected override void drawSlotTimer(SpriteBatch b, int i)
		{
			if (base.Farmer.isCustomized.Value)
			{
				base.drawSlotTimer(b, i);
			}
		}

		protected override void drawSlotMoney(SpriteBatch b, int i)
		{
		}
	}

	public bool gettingFarmhands;

	public bool approvingFarmhand;

	public Client client;

	public FarmhandMenu()
		: this(null)
	{
	}

	public FarmhandMenu(Client client)
	{
		if (client == null && Program.sdk.Networking != null)
		{
			client = Program.sdk.Networking.GetRequestedClient();
		}
		this.client = client;
		if (client != null)
		{
			this.gettingFarmhands = true;
		}
	}

	public override bool readyToClose()
	{
		return !base.loading;
	}

	protected override bool hasDeleteButtons()
	{
		return false;
	}

	/// <inheritdoc />
	protected override void startListPopulation(string filter)
	{
	}

	public override void UpdateButtons()
	{
		base.UpdateButtons();
		if (LocalMultiplayer.IsLocalMultiplayer() && !Game1.game1.IsMainInstance && base.backButton != null)
		{
			base.backButton.visible = false;
		}
	}

	protected override bool checkListPopulation()
	{
		if (this.client != null && (this.gettingFarmhands || this.approvingFarmhand) && (this.client.availableFarmhands != null || this.client.connectionMessage != null))
		{
			base.timerToLoad = 0;
			base.selected = -1;
			base.loading = false;
			this.gettingFarmhands = false;
			if (base.menuSlots == null)
			{
				base.menuSlots = new List<MenuSlot>();
			}
			else
			{
				base.menuSlots.Clear();
			}
			if (this.client.availableFarmhands == null)
			{
				this.approvingFarmhand = true;
			}
			else
			{
				this.approvingFarmhand = false;
				base.menuSlots.AddRange(this.client.availableFarmhands.Select((Farmer farmer) => new FarmhandSlot(this, farmer)));
			}
			if (Game1.activeClickableMenu is TitleMenu)
			{
				Game1.gameMode = 0;
			}
			else if (!Game1.game1.IsMainInstance)
			{
				Game1.gameMode = 0;
			}
			this.UpdateButtons();
			if (Game1.options.SnappyMenus)
			{
				this.populateClickableComponentList();
				this.snapToDefaultClickableComponent();
			}
		}
		return false;
	}

	/// <inheritdoc />
	public override void receiveGamePadButton(Buttons button)
	{
		if (button == Buttons.B && this.readyToClose())
		{
			base.exitThisMenu();
		}
		base.receiveGamePadButton(button);
	}

	/// <inheritdoc />
	public override void receiveLeftClick(int x, int y, bool playSound = true)
	{
		for (int i = 0; i < base.slotButtons.Count; i++)
		{
			if (base.slotButtons[i].containsPoint(x, y) && i < this.MenuSlots.Count && this.MenuSlots[base.currentItemIndex + i] is FarmhandSlot slot && slot.BelongsToAnotherPlayer())
			{
				Game1.playSound("cancel");
				return;
			}
		}
		base.receiveLeftClick(x, y, playSound);
	}

	/// <inheritdoc />
	public override void performHoverAction(int x, int y)
	{
		base.performHoverAction(x, y);
		if (!(base.hoverText == ""))
		{
			return;
		}
		for (int i = 0; i < base.slotButtons.Count; i++)
		{
			if (base.currentItemIndex + i < this.MenuSlots.Count && base.slotButtons[i].containsPoint(x, y) && this.MenuSlots[base.currentItemIndex + i] is FarmhandSlot farmhandSlot && farmhandSlot.BelongsToAnotherPlayer())
			{
				base.hoverText = Game1.content.LoadString("Strings\\UI:Farmhand_Locked");
			}
		}
	}

	public override bool IsAutomaticSnapValid(int direction, ClickableComponent a, ClickableComponent b)
	{
		if (b != null && (b.myID == 800 || b.myID == 801) && base.menuSlots.Count <= 4)
		{
			return false;
		}
		return base.IsAutomaticSnapValid(direction, a, b);
	}

	/// <inheritdoc />
	public override void update(GameTime time)
	{
		if (this.client != null)
		{
			if (!this.client.connectionStarted && base.drawn)
			{
				this.client.connect();
			}
			if (this.client.connectionStarted)
			{
				this.client.receiveMessages();
			}
			if (this.client.readyToPlay)
			{
				Game1.gameMode = 3;
				this.loadClientOptions();
				if (Game1.activeClickableMenu is FarmhandMenu || (Game1.activeClickableMenu is TitleMenu && TitleMenu.subMenu is FarmhandMenu))
				{
					Game1.exitActiveMenu();
				}
			}
			else if (this.client.timedOut)
			{
				if (this.approvingFarmhand)
				{
					Game1.multiplayer.clientRemotelyDisconnected(Multiplayer.IsTimeout(this.client.pendingDisconnect) ? Multiplayer.DisconnectType.Timeout_FarmhandSelection : this.client.pendingDisconnect);
				}
				else
				{
					base.menuSlots.RemoveAll((MenuSlot slot) => slot is FarmhandSlot);
				}
			}
		}
		base.update(time);
	}

	private void loadClientOptions()
	{
		if (LocalMultiplayer.IsLocalMultiplayer())
		{
			Game1.currentSong = Game1.soundBank.GetCue("spring_day_ambient");
			LoadOptions();
		}
		else
		{
			Task task = new Task(LoadOptions);
			Game1.hooks.StartTask(task, "ClientOptions_Load");
		}
		static void LoadOptions()
		{
			StartupPreferences preferences = new StartupPreferences();
			preferences.loadPreferences(async: false, applyLanguage: false);
			if (Game1.game1.IsMainInstance)
			{
				Game1.options = preferences.clientOptions;
			}
			else
			{
				Game1.options = new Options();
			}
			Game1.initializeVolumeLevels();
		}
	}

	protected override string getStatusText()
	{
		if (this.client == null)
		{
			return Game1.content.LoadString("Strings\\UI:CoopMenu_NoInvites");
		}
		if (this.client.timedOut)
		{
			return Game1.content.LoadString("Strings\\UI:CoopMenu_Failed");
		}
		if (this.client.connectionMessage != null)
		{
			return this.client.connectionMessage;
		}
		if (this.gettingFarmhands || this.approvingFarmhand)
		{
			return Game1.content.LoadString("Strings\\UI:CoopMenu_Connecting");
		}
		if (base.menuSlots.Count == 0)
		{
			return Game1.content.LoadString("Strings\\UI:CoopMenu_NoSlots");
		}
		return null;
	}

	protected override void Dispose(bool disposing)
	{
		if (this.client != null && disposing && Game1.client != this.client)
		{
			Multiplayer.LogDisconnect(Multiplayer.IsTimeout(this.client.pendingDisconnect) ? Multiplayer.DisconnectType.Timeout_FarmhandSelection : Multiplayer.DisconnectType.ExitedToMainMenu_FromFarmhandSelect);
			this.client.disconnect();
			if (!Game1.game1.IsMainInstance)
			{
				GameRunner.instance.RemoveGameInstance(Game1.game1);
			}
		}
		base.Dispose(disposing);
	}
}
