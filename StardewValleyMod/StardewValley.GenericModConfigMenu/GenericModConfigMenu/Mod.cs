using System;
using System.Collections.Generic;
using GenericModConfigMenu.Framework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SpaceShared;
using SpaceShared.APIs;
using SpaceShared.UI;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Delegates;
using StardewValley.Menus;
using StardewValley.Triggers;
using xTile.Dimensions;

namespace GenericModConfigMenu;

internal class Mod : Mod
{
	public static Mod instance;

	private OwnModConfig Config;

	private RootElement? Ui;

	private Button ConfigButton;

	private int countdown = 5;

	internal readonly ModConfigManager ConfigManager = new ModConfigManager();

	private static HashSet<string> DidDeprecationWarningsFor = new HashSet<string>();

	private bool wasConfigMenu;

	public static IClickableMenu ActiveConfigMenu
	{
		get
		{
			if (Game1.activeClickableMenu is TitleMenu)
			{
				return TitleMenu.subMenu;
			}
			IClickableMenu menu = Game1.activeClickableMenu;
			if (menu == null)
			{
				return null;
			}
			while (menu.GetChildMenu() != null)
			{
				menu = menu.GetChildMenu();
			}
			if ((!(menu is ModConfigMenu) && !(menu is SpecificModConfigMenu)) || 1 == 0)
			{
				return null;
			}
			return menu;
		}
		set
		{
			if (Game1.activeClickableMenu is TitleMenu)
			{
				TitleMenu.subMenu = value;
			}
			else if (Game1.activeClickableMenu != null)
			{
				IClickableMenu menu = Game1.activeClickableMenu;
				while (menu.GetChildMenu() != null)
				{
					menu = menu.GetChildMenu();
				}
				if (value == null)
				{
					if (menu.GetParentMenu() != null)
					{
						menu.GetParentMenu().SetChildMenu((IClickableMenu)null);
					}
					else
					{
						Game1.activeClickableMenu = null;
					}
				}
				else
				{
					menu.SetChildMenu(value);
				}
			}
			else
			{
				Game1.activeClickableMenu = value;
			}
		}
	}

	public override void Entry(IModHelper helper)
	{
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Expected O, but got Unknown
		Mod.instance = this;
		I18n.Init(helper.Translation);
		Log.Monitor = ((Mod)this).Monitor;
		this.Config = helper.ReadConfig<OwnModConfig>();
		helper.Events.GameLoop.GameLaunched += OnGameLaunched;
		helper.Events.GameLoop.UpdateTicking += OnUpdateTicking;
		helper.Events.Display.WindowResized += OnWindowResized;
		helper.Events.Display.Rendered += OnRendered;
		helper.Events.Display.MenuChanged += OnMenuChanged;
		helper.Events.Input.MouseWheelScrolled += OnMouseWheelScrolled;
		helper.Events.Input.ButtonPressed += OnButtonPressed;
		helper.Events.Input.ButtonsChanged += OnButtonChanged;
		helper.Events.Content.AssetRequested += delegate(object? _, AssetRequestedEventArgs e)
		{
			AssetManager.Apply(e);
		};
		TriggerActionManager.RegisterAction("spacechase0.GenericModConfigMenu_OpenModConfig", (TriggerActionDelegate)delegate(string[] args, TriggerActionContext ctx, out string error)
		{
			if (args.Length < 2)
			{
				error = "Not enough arguments";
				return false;
			}
			if (!((Mod)this).Helper.ModRegistry.IsLoaded(args[1]))
			{
				error = "Mod " + args[1] + " not loaded.";
				return false;
			}
			IManifest manifest = ((Mod)this).Helper.ModRegistry.Get(args[1]).Manifest;
			if (this.ConfigManager.Get(manifest, assert: false) == null)
			{
				error = "Mod " + args[1] + " not registered with GMCM.";
				return false;
			}
			this.OpenModMenuNew(manifest, null, null);
			error = null;
			return true;
		});
	}

	public override object GetApi(IModInfo mod)
	{
		return new Api(mod.Manifest, this.ConfigManager, delegate(IManifest mod2)
		{
			this.OpenModMenu(mod2, null, null);
		}, delegate(IManifest mod2)
		{
			this.OpenModMenuNew(mod2, null, null);
		}, delegate(string s)
		{
			this.LogDeprecated(mod.Manifest.UniqueID, s);
		});
	}

	private void LogDeprecated(string modid, string str)
	{
		if (!Mod.DidDeprecationWarningsFor.Contains(modid))
		{
			Mod.DidDeprecationWarningsFor.Add(modid);
			Log.Info(str);
		}
	}

	private void OpenListMenuNew(int? scrollRow = null)
	{
		Mod.ActiveConfigMenu = (IClickableMenu)(object)new ModConfigMenu(this.Config.ScrollSpeed, delegate(IManifest mod, int curScrollRow)
		{
			this.OpenModMenuNew(mod, null, curScrollRow);
		}, delegate(int currScrollRow)
		{
			this.OpenKeybindsMenuNew(currScrollRow);
		}, this.ConfigManager, ((Mod)this).Helper.GameContent.Load<Texture2D>(AssetManager.KeyboardButton), scrollRow);
	}

	private void OpenListMenu(int? scrollRow = null)
	{
		ModConfigMenu newMenu = new ModConfigMenu(this.Config.ScrollSpeed, delegate(IManifest mod, int curScrollRow)
		{
			this.OpenModMenuNew(mod, null, curScrollRow);
		}, delegate(int currScrollRow)
		{
			this.OpenKeybindsMenuNew(currScrollRow);
		}, this.ConfigManager, ((Mod)this).Helper.GameContent.Load<Texture2D>(AssetManager.KeyboardButton), scrollRow);
		if (Game1.activeClickableMenu is TitleMenu)
		{
			TitleMenu.subMenu = (IClickableMenu)(object)newMenu;
		}
		else
		{
			Game1.activeClickableMenu = (IClickableMenu)(object)newMenu;
		}
	}

	private void OpenKeybindsMenuNew(int listScrollRow)
	{
		Mod.ActiveConfigMenu = (IClickableMenu)(object)new SpecificModConfigMenu(this.ConfigManager, this.Config.ScrollSpeed, delegate
		{
			if (Game1.activeClickableMenu is TitleMenu)
			{
				this.OpenListMenuNew(listScrollRow);
			}
			else
			{
				Mod.ActiveConfigMenu = null;
			}
		});
	}

	private void OpenKeybindsMenu(int listScrollRow)
	{
		SpecificModConfigMenu newMenu = new SpecificModConfigMenu(this.ConfigManager, this.Config.ScrollSpeed, delegate
		{
			this.OpenListMenuNew(listScrollRow);
		});
		if (Game1.activeClickableMenu is TitleMenu)
		{
			TitleMenu.subMenu = (IClickableMenu)(object)newMenu;
		}
		else
		{
			Game1.activeClickableMenu = (IClickableMenu)(object)newMenu;
		}
	}

	private void OpenModMenuNew(IManifest mod, string page, int? listScrollRow)
	{
		ModConfig config = this.ConfigManager.Get(mod, assert: true);
		Mod.ActiveConfigMenu = (IClickableMenu)(object)new SpecificModConfigMenu(config, this.Config.ScrollSpeed, page, delegate(string newPage)
		{
			if (!(Game1.activeClickableMenu is TitleMenu))
			{
				Mod.ActiveConfigMenu = null;
			}
			this.OpenModMenuNew(mod, newPage, listScrollRow);
		}, delegate
		{
			if (Game1.activeClickableMenu is TitleMenu)
			{
				this.OpenListMenuNew(listScrollRow);
			}
			else
			{
				Mod.ActiveConfigMenu = null;
			}
		});
	}

	private void OpenModMenu(IManifest mod, string page, int? listScrollRow)
	{
		ModConfig config = this.ConfigManager.Get(mod, assert: true);
		SpecificModConfigMenu newMenu = new SpecificModConfigMenu(config, this.Config.ScrollSpeed, page, delegate(string newPage)
		{
			this.OpenModMenuNew(mod, newPage, listScrollRow);
		}, delegate
		{
			this.OpenListMenuNew(listScrollRow);
		});
		if (Game1.activeClickableMenu is TitleMenu)
		{
			TitleMenu.subMenu = (IClickableMenu)(object)newMenu;
		}
		else
		{
			Game1.activeClickableMenu = (IClickableMenu)(object)newMenu;
		}
	}

	private void SetupTitleMenuButton()
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Expected O, but got Unknown
		if (this.Ui == null)
		{
			this.Ui = new RootElement();
			Texture2D tex = ((Mod)this).Helper.GameContent.Load<Texture2D>(AssetManager.ConfigButton);
			this.ConfigButton = new Button(tex)
			{
				LocalPosition = new Vector2(36f, (float)(((Rectangle)(ref Game1.viewport)).Height - 100)),
				Callback = delegate
				{
					Game1.playSound("newArtifact", (int?)null);
					this.OpenListMenuNew();
				}
			};
			this.Ui.AddChild(this.ConfigButton);
		}
		IClickableMenu activeClickableMenu = Game1.activeClickableMenu;
		TitleMenu tm = (TitleMenu)(object)((activeClickableMenu is TitleMenu) ? activeClickableMenu : null);
		if (tm != null && ((IClickableMenu)tm).allClickableComponents?.Find((ClickableComponent cc) => cc != null && cc.myID == 509800) == null)
		{
			Texture2D tex2 = ((Mod)this).Helper.GameContent.Load<Texture2D>(AssetManager.ConfigButton);
			ClickableComponent button = new ClickableComponent(new Rectangle(0, ((Rectangle)(ref Game1.viewport)).Height - 100, tex2.Width / 2, tex2.Height / 2), "GMCM")
			{
				myID = 509800,
				rightNeighborID = ((ClickableComponent)tm.buttons[0]).myID
			};
			((IClickableMenu)tm).allClickableComponents?.Add(button);
			((ClickableComponent)tm.buttons[0]).leftNeighborID = 509800;
		}
	}

	private bool IsTitleMenuInteractable()
	{
		IClickableMenu activeClickableMenu = Game1.activeClickableMenu;
		TitleMenu titleMenu = (TitleMenu)(object)((activeClickableMenu is TitleMenu) ? activeClickableMenu : null);
		if (titleMenu == null || TitleMenu.subMenu != null)
		{
			return false;
		}
		IReflectedMethod method = ((Mod)this).Helper.Reflection.GetMethod((object)titleMenu, "ShouldAllowInteraction", false);
		if (method != null)
		{
			return method.Invoke<bool>(Array.Empty<object>());
		}
		return ((Mod)this).Helper.Reflection.GetField<bool>((object)titleMenu, "titleInPosition", true).GetValue();
	}

	private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
	{
		((Mod)this).Helper.Events.GameLoop.UpdateTicking += FiveTicksAfterGameLaunched;
		Api configMenu = new Api(((Mod)this).ModManifest, this.ConfigManager, delegate(IManifest mod)
		{
			this.OpenModMenu(mod, null, null);
		}, delegate(IManifest mod)
		{
			this.OpenModMenuNew(mod, null, null);
		}, delegate(string s)
		{
			this.LogDeprecated(((Mod)this).ModManifest.UniqueID, s);
		});
		configMenu.Register(((Mod)this).ModManifest, delegate
		{
			this.Config = new OwnModConfig();
		}, delegate
		{
			((Mod)this).Helper.WriteConfig<OwnModConfig>(this.Config);
		}, titleScreenOnly: false);
		configMenu.AddNumberOption(((Mod)this).ModManifest, () => this.Config.ScrollSpeed, delegate(int value)
		{
			this.Config.ScrollSpeed = value;
		}, I18n.Options_ScrollSpeed_Name, I18n.Options_ScrollSpeed_Desc, 1, 500);
		configMenu.AddKeybindList(((Mod)this).ModManifest, () => this.Config.OpenMenuKey, delegate(KeybindList value)
		{
			this.Config.OpenMenuKey = value;
		}, I18n.Options_OpenMenuKey_Name, I18n.Options_OpenMenuKey_Desc);
		IBetterGameMenuApi BetterGameMenu = ((Mod)this).Helper.ModRegistry.GetApi<IBetterGameMenuApi>("leclair.bettergamemenu");
		BetterGameMenu?.OnTabContextMenu(delegate(ITabContextMenuEvent evt)
		{
			if (evt.Tab == "Options")
			{
				evt.Entries.Add(evt.CreateEntry(I18n.Button_ModOptions(), delegate
				{
					this.OpenListMenuNew();
				}));
			}
		}, (EventPriority)0);
		BetterGameMenu?.OnPageCreated(delegate(IPageCreatedEvent evt)
		{
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Expected O, but got Unknown
			if (evt.Tab == "Options")
			{
				IClickableMenu page = evt.Page;
				((OptionsPage)(((page is OptionsPage) ? page : null)?)).options.Add((OptionsElement)new OptionsButton(I18n.Button_ModOptions(), (Action)delegate
				{
					this.OpenListMenuNew();
				}));
			}
		}, (EventPriority)0);
	}

	private void FiveTicksAfterGameLaunched(object sender, UpdateTickingEventArgs e)
	{
		if (this.countdown-- < 0)
		{
			this.SetupTitleMenuButton();
			((Mod)this).Helper.Events.GameLoop.UpdateTicking -= FiveTicksAfterGameLaunched;
		}
	}

	private void OnUpdateTicking(object sender, UpdateTickingEventArgs e)
	{
		if (this.IsTitleMenuInteractable())
		{
			this.SetupTitleMenuButton();
			this.Ui?.Update();
		}
		if (this.wasConfigMenu && TitleMenu.subMenu == null)
		{
			IReflectedField<bool> f = ((Mod)this).Helper.Reflection.GetField<bool>((object)Game1.activeClickableMenu, "titleInPosition", true);
			if (!f.GetValue())
			{
				f.SetValue(true);
			}
		}
		this.wasConfigMenu = TitleMenu.subMenu is ModConfigMenu || TitleMenu.subMenu is SpecificModConfigMenu;
	}

	private void OnWindowResized(object sender, WindowResizedEventArgs e)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		if (this.ConfigButton != null)
		{
			this.ConfigButton.LocalPosition = new Vector2(this.ConfigButton.Position.X, (float)(((Rectangle)(ref Game1.viewport)).Height - 100));
		}
	}

	private void OnRendered(object sender, RenderedEventArgs e)
	{
		if (this.IsTitleMenuInteractable())
		{
			this.Ui?.Draw(e.SpriteBatch);
		}
	}

	private void OnMenuChanged(object sender, MenuChangedEventArgs e)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Expected O, but got Unknown
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Expected O, but got Unknown
		IClickableMenu newMenu = e.NewMenu;
		GameMenu menu = (GameMenu)(object)((newMenu is GameMenu) ? newMenu : null);
		if (menu != null)
		{
			OptionsPage page = (OptionsPage)menu.pages[GameMenu.optionsTab];
			page.options.Add((OptionsElement)new OptionsButton(I18n.Button_ModOptions(), (Action)delegate
			{
				this.OpenListMenuNew();
			}));
		}
	}

	private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		Keys key = default(Keys);
		if (Context.IsPlayerFree && this.Config.OpenMenuKey.JustPressed())
		{
			this.OpenListMenuNew();
		}
		else if (Mod.ActiveConfigMenu is SpecificModConfigMenu menu && SButtonExtensions.TryGetKeyboard(e.Button, ref key))
		{
			((IClickableMenu)menu).receiveKeyPress(key);
		}
	}

	private void OnButtonChanged(object sender, ButtonsChangedEventArgs e)
	{
		if (Mod.ActiveConfigMenu is SpecificModConfigMenu menu)
		{
			menu.OnButtonsChanged(e);
		}
	}

	private void OnMouseWheelScrolled(object sender, MouseWheelScrolledEventArgs e)
	{
		Dropdown.ActiveDropdown?.ReceiveScrollWheelAction(e.Delta);
	}
}
