using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace StardewValley.Menus;

public class OptionsPage : IClickableMenu
{
	public const int itemsPerPage = 7;

	private string hoverText = "";

	public List<ClickableComponent> optionSlots = new List<ClickableComponent>();

	public int currentItemIndex;

	private ClickableTextureComponent upArrow;

	private ClickableTextureComponent downArrow;

	private ClickableTextureComponent scrollBar;

	private bool scrolling;

	public List<OptionsElement> options = new List<OptionsElement>();

	private Rectangle scrollBarRunner;

	internal static int _lastSelectedIndex;

	internal static int _lastCurrentItemIndex;

	public int lastRebindTick = -1;

	private int optionsSlotHeld = -1;

	public OptionsPage(int x, int y, int width, int height)
		: base(x, y, width, height)
	{
		this.upArrow = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen + width + 16, base.yPositionOnScreen + 64, 44, 48), Game1.mouseCursors, new Rectangle(421, 459, 11, 12), 4f);
		this.downArrow = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen + width + 16, base.yPositionOnScreen + height - 64, 44, 48), Game1.mouseCursors, new Rectangle(421, 472, 11, 12), 4f);
		this.scrollBar = new ClickableTextureComponent(new Rectangle(this.upArrow.bounds.X + 12, this.upArrow.bounds.Y + this.upArrow.bounds.Height + 4, 24, 40), Game1.mouseCursors, new Rectangle(435, 463, 6, 10), 4f);
		this.scrollBarRunner = new Rectangle(this.scrollBar.bounds.X, this.upArrow.bounds.Y + this.upArrow.bounds.Height + 4, this.scrollBar.bounds.Width, height - 128 - this.upArrow.bounds.Height - 8);
		for (int i = 0; i < 7; i++)
		{
			this.optionSlots.Add(new ClickableComponent(new Rectangle(base.xPositionOnScreen + 16, base.yPositionOnScreen + 80 + 4 + i * ((height - 128) / 7) + 16, width - 32, (height - 128) / 7 + 4), i.ToString() ?? "")
			{
				myID = i,
				downNeighborID = ((i < 6) ? (i + 1) : (-7777)),
				upNeighborID = ((i > 0) ? (i - 1) : (-7777)),
				fullyImmutable = true
			});
		}
		this.options.Add(new OptionsElement(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11233")));
		this.options.Add(new OptionsCheckbox(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11234"), 0));
		this.options.Add(new OptionsCheckbox(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11235"), 7));
		this.options.Add(new OptionsCheckbox(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11236"), 8));
		this.options.Add(new OptionsCheckbox(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11237"), 11));
		this.options.Add(new OptionsCheckbox(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11238"), 12));
		if (Game1.game1.IsMainInstance)
		{
			this.options.Add(new OptionsDropDown(Game1.content.LoadString("Strings\\UI:Options_GamepadMode"), 38));
		}
		this.options.Add(new OptionsDropDown(Game1.content.LoadString("Strings\\UI:Options_StowingMode"), 28));
		this.options.Add(new OptionsDropDown(Game1.content.LoadString("Strings\\UI:Options_SlingshotMode"), 41));
		this.options.Add(new OptionsCheckbox(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11239"), 27));
		this.options.Add(new OptionsCheckbox(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11240"), 14));
		this.options.Add(new OptionsCheckbox(Game1.content.LoadString("Strings\\UI:Options_GamepadStyleMenus"), 29));
		this.options.Add(new OptionsCheckbox(Game1.content.LoadString("Strings\\UI:Options_ShowAdvancedCraftingInformation"), 34));
		bool showLocalCoopOptions = Game1.game1.IsMainInstance && Game1.game1.IsLocalCoopJoinable();
		if (Game1.multiplayerMode == 2 || showLocalCoopOptions)
		{
			this.options.Add(new OptionsElement(Game1.content.LoadString("Strings\\UI:OptionsPage_MultiplayerSection")));
		}
		if (Game1.multiplayerMode == 2 && Game1.server != null && !Game1.server.IsLocalMultiplayerInitiatedServer())
		{
			this.options.Add(new OptionsDropDown(Game1.content.LoadString("Strings\\UI:GameMenu_ServerMode"), 31));
			this.options.Add(new OptionsCheckbox(Game1.content.LoadString("Strings\\UI:OptionsPage_IPConnections"), 30));
			this.options.Add(new OptionsCheckbox(Game1.content.LoadString("Strings\\UI:OptionsPage_FarmhandCreation"), 32));
		}
		if (Game1.multiplayerMode == 2 && Game1.server != null)
		{
			this.options.Add(new OptionsDropDown(Game1.content.LoadString("Strings\\UI:GameMenu_MoveBuildingPermissions"), 40));
		}
		if (Game1.multiplayerMode == 2 && Game1.server != null && !Game1.server.IsLocalMultiplayerInitiatedServer() && Program.sdk.Networking != null)
		{
			this.options.Add(new OptionsButton(Game1.content.LoadString("Strings\\UI:GameMenu_ServerInvite"), offerInvite));
			if (Program.sdk.Networking.SupportsInviteCodes())
			{
				this.options.Add(new OptionsButton(Game1.content.LoadString("Strings\\UI:OptionsPage_ShowInviteCode"), showInviteCode));
			}
		}
		if (showLocalCoopOptions)
		{
			this.options.Add(new OptionsButton(Game1.content.LoadString("Strings\\UI:StartLocalMulti"), delegate
			{
				base.exitThisMenu(playSound: false);
				Game1.game1.ShowLocalCoopJoinMenu();
			}));
		}
		if (Game1.IsMultiplayer)
		{
			this.options.Add(new OptionsCheckbox(Game1.content.LoadString("Strings\\UI:OptionsPage_ShowReadyStatus"), 35));
		}
		this.options.Add(new OptionsElement(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11241")));
		if (Game1.game1.IsMainInstance)
		{
			this.options.Add(new OptionsSlider(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11242"), 1));
			this.options.Add(new OptionsSlider(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11243"), 2));
			this.options.Add(new OptionsSlider(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11244"), 20));
			this.options.Add(new OptionsSlider(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11245"), 21));
		}
		this.options.Add(new OptionsDropDown(Game1.content.LoadString("Strings\\StringsFromCSFiles:BiteChime"), 42));
		this.options.Add(new OptionsCheckbox(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11246"), 3));
		this.options.Add(new OptionsCheckbox(Game1.content.LoadString("Strings\\StringsFromCSFiles:Options_ToggleAnimalSounds"), 43));
		this.options.Add(new OptionsElement(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11247")));
		if (!Game1.conventionMode && Game1.game1.IsMainInstance)
		{
			this.options.Add(new OptionsDropDown(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11248"), 13));
			this.options.Add(new OptionsDropDown(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11251"), 6));
		}
		this.options.Add(new OptionsDropDown(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11252"), 9));
		if (Game1.game1.IsMainInstance)
		{
			this.options.Add(new OptionsCheckbox(Game1.content.LoadString("Strings\\UI:Options_Vsync"), 37));
		}
		List<string> zoom_options = new List<string>();
		for (int zoom = 75; zoom <= 150; zoom += 5)
		{
			zoom_options.Add(zoom + "%");
		}
		this.options.Add(new OptionsPlusMinus(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage_UIScale"), 39, zoom_options, zoom_options));
		zoom_options = new List<string>();
		for (int zoom2 = 75; zoom2 <= 200; zoom2 += 5)
		{
			zoom_options.Add(zoom2 + "%");
		}
		this.options.Add(new OptionsCheckbox(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11253"), 15));
		this.options.Add(new OptionsPlusMinus(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11254"), 18, zoom_options, zoom_options));
		this.options.Add(new OptionsCheckbox(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11266"), 19));
		this.options.Add(new OptionsSlider(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11271"), 23));
		this.options.Add(new OptionsCheckbox(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11272"), 24));
		if (!LocalMultiplayer.IsLocalMultiplayer())
		{
			this.options.Add(new OptionsCheckbox(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11273"), 26));
		}
		if (Game1.content.GetCurrentLanguage() == LocalizedContentManager.LanguageCode.zh)
		{
			this.options.Add(new OptionsCheckbox("使用平滑字体", 44));
			this.options.Add(new OptionsSlider("对话字体大小", 45));
		}
		else if (Game1.content.GetCurrentLanguage() == LocalizedContentManager.LanguageCode.ru)
		{
			this.options.Add(new OptionsCheckbox("Использовать альтернативный шрифт", 46));
		}
		this.options.Add(new OptionsElement(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11274")));
		this.options.Add(new OptionsCheckbox(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11275"), 16));
		this.options.Add(new OptionsCheckbox(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11276"), 22));
		if (Game1.game1.IsMainInstance)
		{
			this.options.Add(new OptionsInputListener(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11277"), -1, this.optionSlots[0].bounds.Width));
			this.options.Add(new OptionsInputListener(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11278"), 7, this.optionSlots[0].bounds.Width));
			this.options.Add(new OptionsInputListener(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11279"), 10, this.optionSlots[0].bounds.Width));
			this.options.Add(new OptionsInputListener(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11280"), 15, this.optionSlots[0].bounds.Width));
			this.options.Add(new OptionsInputListener(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11281"), 18, this.optionSlots[0].bounds.Width));
			this.options.Add(new OptionsInputListener(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11282"), 19, this.optionSlots[0].bounds.Width));
			this.options.Add(new OptionsInputListener(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11283"), 11, this.optionSlots[0].bounds.Width));
			this.options.Add(new OptionsInputListener(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11284"), 14, this.optionSlots[0].bounds.Width));
			this.options.Add(new OptionsInputListener(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11285"), 13, this.optionSlots[0].bounds.Width));
			this.options.Add(new OptionsInputListener(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11286"), 12, this.optionSlots[0].bounds.Width));
			this.options.Add(new OptionsInputListener(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11287"), 17, this.optionSlots[0].bounds.Width));
			this.options.Add(new OptionsInputListener(Game1.content.LoadString("Strings\\UI:Input_EmoteButton"), 33, this.optionSlots[0].bounds.Width));
			this.options.Add(new OptionsInputListener(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11288"), 16, this.optionSlots[0].bounds.Width));
			this.options.Add(new OptionsInputListener(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.toolbarSwap"), 32, this.optionSlots[0].bounds.Width));
			this.options.Add(new OptionsInputListener(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11289"), 20, this.optionSlots[0].bounds.Width));
			this.options.Add(new OptionsInputListener(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11290"), 21, this.optionSlots[0].bounds.Width));
			this.options.Add(new OptionsInputListener(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11291"), 22, this.optionSlots[0].bounds.Width));
			this.options.Add(new OptionsInputListener(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11292"), 23, this.optionSlots[0].bounds.Width));
			this.options.Add(new OptionsInputListener(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11293"), 24, this.optionSlots[0].bounds.Width));
			this.options.Add(new OptionsInputListener(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11294"), 25, this.optionSlots[0].bounds.Width));
			this.options.Add(new OptionsInputListener(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11295"), 26, this.optionSlots[0].bounds.Width));
			this.options.Add(new OptionsInputListener(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11296"), 27, this.optionSlots[0].bounds.Width));
			this.options.Add(new OptionsInputListener(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11297"), 28, this.optionSlots[0].bounds.Width));
			this.options.Add(new OptionsInputListener(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11298"), 29, this.optionSlots[0].bounds.Width));
			this.options.Add(new OptionsInputListener(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11299"), 30, this.optionSlots[0].bounds.Width));
			this.options.Add(new OptionsInputListener(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11300"), 31, this.optionSlots[0].bounds.Width));
		}
		if (!Game1.game1.CanTakeScreenshots())
		{
			return;
		}
		this.options.Add(new OptionsElement(Game1.content.LoadString("Strings\\UI:OptionsPage_ScreenshotHeader")));
		int index = this.options.Count;
		if (!Game1.game1.CanZoomScreenshots())
		{
			OptionsButton btn = new OptionsButton(Game1.content.LoadString("Strings\\UI:OptionsPage_ScreenshotHeader").Replace(":", ""), TakeScreenshot);
			if (Game1.game1.ScreenshotBusy)
			{
				btn.greyedOut = true;
			}
			this.options.Add(btn);
		}
		else
		{
			this.options.Add(new OptionsPlusMinusButton(Game1.content.LoadString("Strings\\StringsFromCSFiles:OptionsPage.cs.11254"), 36, new List<string> { "25%", "50%", "75%", "100%" }, new List<string> { "25%", "50%", "75%", "100%" }, Game1.mouseCursors2, new Rectangle(72, 31, 18, 16), delegate(string selection)
			{
				Game1.flashAlpha = 1f;
				selection = selection.Substring(0, selection.Length - 1);
				if (!int.TryParse(selection, out var result))
				{
					result = 25;
				}
				string text = Game1.game1.takeMapScreenshot((float)result / 100f, null, null);
				if (text != null)
				{
					Game1.addHUDMessage(new HUDMessage(text, 6));
				}
				Game1.playSound("cameraNoise");
			}));
		}
		if (Game1.game1.CanBrowseScreenshots())
		{
			this.options.Add(new OptionsButton(Game1.content.LoadString("Strings\\UI:OptionsPage_OpenFolder"), Game1.game1.BrowseScreenshots));
		}
		void TakeScreenshot()
		{
			OptionsElement e = this.options[index];
			Game1.flashAlpha = 1f;
			e.greyedOut = true;
			string screenshot = Game1.game1.takeMapScreenshot(null, null, OnDone);
			if (screenshot != null)
			{
				Game1.addHUDMessage(new HUDMessage(screenshot, 6));
			}
			Game1.playSound("cameraNoise");
			void OnDone()
			{
				e.greyedOut = false;
			}
		}
	}

	public override bool readyToClose()
	{
		if (this.lastRebindTick == Game1.ticks)
		{
			return false;
		}
		return base.readyToClose();
	}

	private void waitForServerConnection(Action onConnection)
	{
		IClickableMenu thisMenu;
		if (Game1.server != null)
		{
			if (Game1.server.connected())
			{
				onConnection();
				return;
			}
			thisMenu = Game1.activeClickableMenu;
			Game1.activeClickableMenu = new ServerConnectionDialog(OnConfirm, OnClose);
		}
		void OnClose(Farmer who)
		{
			Game1.activeClickableMenu = thisMenu;
			thisMenu.snapCursorToCurrentSnappedComponent();
		}
		void OnConfirm(Farmer who)
		{
			OnClose(who);
			onConnection();
		}
	}

	private void offerInvite()
	{
		this.waitForServerConnection(Game1.server.offerInvite);
	}

	private void showInviteCode()
	{
		IClickableMenu thisMenu = Game1.activeClickableMenu;
		this.waitForServerConnection(delegate
		{
			Game1.activeClickableMenu = new InviteCodeDialog(Game1.server.getInviteCode(), OnClose);
		});
		void OnClose(Farmer who)
		{
			Game1.activeClickableMenu = thisMenu;
			thisMenu.snapCursorToCurrentSnappedComponent();
		}
	}

	public override void snapToDefaultClickableComponent()
	{
		base.snapToDefaultClickableComponent();
		base.currentlySnappedComponent = base.getComponentWithID(1);
		this.snapCursorToCurrentSnappedComponent();
	}

	public override void applyMovementKey(int direction)
	{
		if (!this.IsDropdownActive())
		{
			base.applyMovementKey(direction);
		}
	}

	protected override void customSnapBehavior(int direction, int oldRegion, int oldID)
	{
		base.customSnapBehavior(direction, oldRegion, oldID);
		if (oldID == 6 && direction == 2 && this.currentItemIndex < Math.Max(0, this.options.Count - 7))
		{
			this.downArrowPressed();
			Game1.playSound("shiny4");
		}
		else
		{
			if (oldID != 0 || direction != 0)
			{
				return;
			}
			if (this.currentItemIndex > 0)
			{
				this.upArrowPressed();
				Game1.playSound("shiny4");
				return;
			}
			base.currentlySnappedComponent = base.getComponentWithID(12348);
			if (base.currentlySnappedComponent != null)
			{
				base.currentlySnappedComponent.downNeighborID = 0;
			}
			this.snapCursorToCurrentSnappedComponent();
		}
	}

	private void setScrollBarToCurrentIndex()
	{
		if (this.options.Count > 0)
		{
			this.scrollBar.bounds.Y = this.scrollBarRunner.Height / Math.Max(1, this.options.Count - 7 + 1) * this.currentItemIndex + this.upArrow.bounds.Bottom + 4;
			if (this.scrollBar.bounds.Y > this.downArrow.bounds.Y - this.scrollBar.bounds.Height - 4)
			{
				this.scrollBar.bounds.Y = this.downArrow.bounds.Y - this.scrollBar.bounds.Height - 4;
			}
		}
	}

	public override void snapCursorToCurrentSnappedComponent()
	{
		if (base.currentlySnappedComponent != null && base.currentlySnappedComponent.myID < this.options.Count)
		{
			OptionsElement optionsElement = this.options[base.currentlySnappedComponent.myID + this.currentItemIndex];
			if (!(optionsElement is OptionsDropDown dropdown))
			{
				if (!(optionsElement is OptionsPlusMinusButton))
				{
					if (optionsElement is OptionsInputListener)
					{
						Game1.setMousePosition(base.currentlySnappedComponent.bounds.Right - 48, base.currentlySnappedComponent.bounds.Center.Y - 12);
					}
					else
					{
						Game1.setMousePosition(base.currentlySnappedComponent.bounds.Left + 48, base.currentlySnappedComponent.bounds.Center.Y - 12);
					}
				}
				else
				{
					Game1.setMousePosition(base.currentlySnappedComponent.bounds.Left + 64, base.currentlySnappedComponent.bounds.Center.Y + 4);
				}
			}
			else
			{
				Game1.setMousePosition(base.currentlySnappedComponent.bounds.Left + dropdown.bounds.Right - 32, base.currentlySnappedComponent.bounds.Center.Y - 4);
			}
		}
		else if (base.currentlySnappedComponent != null)
		{
			base.snapCursorToCurrentSnappedComponent();
		}
	}

	/// <inheritdoc />
	public override void leftClickHeld(int x, int y)
	{
		if (GameMenu.forcePreventClose)
		{
			return;
		}
		base.leftClickHeld(x, y);
		if (this.scrolling)
		{
			int y2 = this.scrollBar.bounds.Y;
			this.scrollBar.bounds.Y = Math.Min(base.yPositionOnScreen + base.height - 64 - 12 - this.scrollBar.bounds.Height, Math.Max(y, base.yPositionOnScreen + this.upArrow.bounds.Height + 20));
			float percentage = (float)(y - this.scrollBarRunner.Y) / (float)this.scrollBarRunner.Height;
			this.currentItemIndex = Math.Min(this.options.Count - 7, Math.Max(0, (int)((float)this.options.Count * percentage)));
			this.setScrollBarToCurrentIndex();
			if (y2 != this.scrollBar.bounds.Y)
			{
				Game1.playSound("shiny4");
			}
		}
		else if (this.optionsSlotHeld != -1 && this.optionsSlotHeld + this.currentItemIndex < this.options.Count)
		{
			this.options[this.currentItemIndex + this.optionsSlotHeld].leftClickHeld(x - this.optionSlots[this.optionsSlotHeld].bounds.X, y - this.optionSlots[this.optionsSlotHeld].bounds.Y);
		}
	}

	public override void setCurrentlySnappedComponentTo(int id)
	{
		base.currentlySnappedComponent = base.getComponentWithID(id);
		this.snapCursorToCurrentSnappedComponent();
	}

	/// <inheritdoc />
	public override void receiveKeyPress(Keys key)
	{
		if ((this.optionsSlotHeld != -1 && this.optionsSlotHeld + this.currentItemIndex < this.options.Count) || (Game1.options.snappyMenus && Game1.options.gamepadControls))
		{
			if (base.currentlySnappedComponent != null && Game1.options.snappyMenus && Game1.options.gamepadControls && this.options.Count > this.currentItemIndex + base.currentlySnappedComponent.myID && this.currentItemIndex + base.currentlySnappedComponent.myID >= 0)
			{
				this.options[this.currentItemIndex + base.currentlySnappedComponent.myID].receiveKeyPress(key);
			}
			else if (this.options.Count > this.currentItemIndex + this.optionsSlotHeld && this.currentItemIndex + this.optionsSlotHeld >= 0)
			{
				this.options[this.currentItemIndex + this.optionsSlotHeld].receiveKeyPress(key);
			}
		}
		base.receiveKeyPress(key);
	}

	/// <inheritdoc />
	public override void receiveScrollWheelAction(int direction)
	{
		if (!GameMenu.forcePreventClose && !this.IsDropdownActive())
		{
			base.receiveScrollWheelAction(direction);
			if (direction > 0 && this.currentItemIndex > 0)
			{
				this.upArrowPressed();
				Game1.playSound("shiny4");
			}
			else if (direction < 0 && this.currentItemIndex < Math.Max(0, this.options.Count - 7))
			{
				this.downArrowPressed();
				Game1.playSound("shiny4");
			}
			if (Game1.options.SnappyMenus)
			{
				this.snapCursorToCurrentSnappedComponent();
			}
		}
	}

	/// <inheritdoc />
	public override void releaseLeftClick(int x, int y)
	{
		if (!GameMenu.forcePreventClose)
		{
			base.releaseLeftClick(x, y);
			if (this.optionsSlotHeld != -1 && this.optionsSlotHeld + this.currentItemIndex < this.options.Count)
			{
				this.options[this.currentItemIndex + this.optionsSlotHeld].leftClickReleased(x - this.optionSlots[this.optionsSlotHeld].bounds.X, y - this.optionSlots[this.optionsSlotHeld].bounds.Y);
			}
			this.optionsSlotHeld = -1;
			this.scrolling = false;
		}
	}

	public bool IsDropdownActive()
	{
		if (this.optionsSlotHeld != -1 && this.optionsSlotHeld + this.currentItemIndex < this.options.Count && this.options[this.currentItemIndex + this.optionsSlotHeld] is OptionsDropDown)
		{
			return true;
		}
		return false;
	}

	private void downArrowPressed()
	{
		if (!this.IsDropdownActive())
		{
			this.UnsubscribeFromSelectedTextbox();
			this.downArrow.scale = this.downArrow.baseScale;
			this.currentItemIndex++;
			this.setScrollBarToCurrentIndex();
		}
	}

	public virtual void UnsubscribeFromSelectedTextbox()
	{
		if (Game1.keyboardDispatcher.Subscriber == null)
		{
			return;
		}
		foreach (OptionsElement option in this.options)
		{
			if (option is OptionsTextEntry entry && Game1.keyboardDispatcher.Subscriber == entry.textBox)
			{
				Game1.keyboardDispatcher.Subscriber = null;
				break;
			}
		}
	}

	/// <summary>Restore the page state when it's recreated for a window resize.</summary>
	/// <param name="oldPage">The previous page instance before it was recreated.</param>
	public void postWindowSizeChange(IClickableMenu oldPage)
	{
		if (oldPage is OptionsPage oldCollectionsPage)
		{
			int lastSelectedIndex = oldCollectionsPage.getCurrentlySnappedComponent()?.myID ?? (-1);
			int lastCurrentItemIndex = oldCollectionsPage.currentItemIndex;
			if (Game1.options.SnappyMenus)
			{
				Game1.activeClickableMenu.setCurrentlySnappedComponentTo(lastSelectedIndex);
			}
			this.currentItemIndex = lastCurrentItemIndex;
			this.setScrollBarToCurrentIndex();
		}
	}

	private void upArrowPressed()
	{
		if (!this.IsDropdownActive())
		{
			this.UnsubscribeFromSelectedTextbox();
			this.upArrow.scale = this.upArrow.baseScale;
			this.currentItemIndex--;
			this.setScrollBarToCurrentIndex();
		}
	}

	/// <inheritdoc />
	public override void receiveLeftClick(int x, int y, bool playSound = true)
	{
		if (GameMenu.forcePreventClose)
		{
			return;
		}
		if (this.downArrow.containsPoint(x, y) && this.currentItemIndex < Math.Max(0, this.options.Count - 7))
		{
			this.downArrowPressed();
			Game1.playSound("shwip");
		}
		else if (this.upArrow.containsPoint(x, y) && this.currentItemIndex > 0)
		{
			this.upArrowPressed();
			Game1.playSound("shwip");
		}
		else if (this.scrollBar.containsPoint(x, y))
		{
			this.scrolling = true;
		}
		else if (!this.downArrow.containsPoint(x, y) && x > base.xPositionOnScreen + base.width && x < base.xPositionOnScreen + base.width + 128 && y > base.yPositionOnScreen && y < base.yPositionOnScreen + base.height)
		{
			this.scrolling = true;
			this.leftClickHeld(x, y);
			this.releaseLeftClick(x, y);
		}
		this.currentItemIndex = Math.Max(0, Math.Min(this.options.Count - 7, this.currentItemIndex));
		this.UnsubscribeFromSelectedTextbox();
		for (int i = 0; i < this.optionSlots.Count; i++)
		{
			if (this.optionSlots[i].bounds.Contains(x, y) && this.currentItemIndex + i < this.options.Count && this.options[this.currentItemIndex + i].bounds.Contains(x - this.optionSlots[i].bounds.X, y - this.optionSlots[i].bounds.Y))
			{
				this.options[this.currentItemIndex + i].receiveLeftClick(x - this.optionSlots[i].bounds.X, y - this.optionSlots[i].bounds.Y);
				this.optionsSlotHeld = i;
				break;
			}
		}
	}

	public override void performHoverAction(int x, int y)
	{
		for (int i = 0; i < this.optionSlots.Count; i++)
		{
			if (this.currentItemIndex >= 0 && this.currentItemIndex + i < this.options.Count && this.options[this.currentItemIndex + i].bounds.Contains(x - this.optionSlots[i].bounds.X, y - this.optionSlots[i].bounds.Y))
			{
				Game1.SetFreeCursorDrag();
				break;
			}
		}
		if (this.scrollBarRunner.Contains(x, y))
		{
			Game1.SetFreeCursorDrag();
		}
		if (!GameMenu.forcePreventClose)
		{
			this.hoverText = "";
			this.upArrow.tryHover(x, y);
			this.downArrow.tryHover(x, y);
			this.scrollBar.tryHover(x, y);
		}
	}

	/// <inheritdoc />
	public override void draw(SpriteBatch b)
	{
		b.End();
		b.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp);
		for (int i = 0; i < this.optionSlots.Count; i++)
		{
			if (this.currentItemIndex >= 0 && this.currentItemIndex + i < this.options.Count)
			{
				this.options[this.currentItemIndex + i].draw(b, this.optionSlots[i].bounds.X, this.optionSlots[i].bounds.Y, this);
			}
		}
		b.End();
		b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);
		if (!GameMenu.forcePreventClose)
		{
			this.upArrow.draw(b);
			this.downArrow.draw(b);
			if (this.options.Count > 7)
			{
				IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 383, 6, 6), this.scrollBarRunner.X, this.scrollBarRunner.Y, this.scrollBarRunner.Width, this.scrollBarRunner.Height, Color.White, 4f, drawShadow: false);
				this.scrollBar.draw(b);
			}
		}
		if (!this.hoverText.Equals(""))
		{
			IClickableMenu.drawHoverText(b, this.hoverText, Game1.smallFont);
		}
	}
}
