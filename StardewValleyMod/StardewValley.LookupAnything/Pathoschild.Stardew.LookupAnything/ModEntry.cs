using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Newtonsoft.Json.Linq;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.Common.Integrations.GenericModConfigMenu;
using Pathoschild.Stardew.Common.Integrations.IconicFramework;
using Pathoschild.Stardew.LookupAnything.Components;
using Pathoschild.Stardew.LookupAnything.Framework;
using Pathoschild.Stardew.LookupAnything.Framework.Lookups;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;

namespace Pathoschild.Stardew.LookupAnything;

internal class ModEntry : Mod
{
	private ModConfig Config;

	private Metadata? Metadata;

	private readonly string DatabaseFileName = "assets/data.json";

	private GameHelper? GameHelper;

	private TargetFactory? TargetFactory;

	private PerScreen<DebugInterface>? DebugInterface;

	private readonly PerScreen<Stack<IClickableMenu>> PreviousMenus = new PerScreen<Stack<IClickableMenu>>((Func<Stack<IClickableMenu>>)(() => new Stack<IClickableMenu>()));

	private ModConfigKeys Keys => this.Config.Controls;

	[MemberNotNullWhen(true, new string[] { "Metadata", "GameHelper", "TargetFactory", "DebugInterface" })]
	private bool IsDataValid
	{
		[MemberNotNullWhen(true, new string[] { "Metadata", "GameHelper", "TargetFactory", "DebugInterface" })]
		get;
		[MemberNotNullWhen(true, new string[] { "Metadata", "GameHelper", "TargetFactory", "DebugInterface" })]
		set;
	}

	public override void Entry(IModHelper helper)
	{
		CommonHelper.RemoveObsoleteFiles((IMod)(object)this, "LookupAnything.pdb");
		this.Config = this.LoadConfig();
		I18n.Init(helper.Translation);
		this.Metadata = this.LoadMetadata();
		this.IsDataValid = this.Metadata?.LooksValid() ?? false;
		if (!this.IsDataValid)
		{
			((Mod)this).Monitor.Log("The " + this.DatabaseFileName + " file seems to be missing or corrupt. Lookups will be disabled.", (LogLevel)4);
		}
		if (!helper.Translation.GetTranslations().Any())
		{
			((Mod)this).Monitor.Log("The translation files in this mod's i18n folder seem to be missing. The mod will still work, but you'll see 'missing translation' messages. Try reinstalling the mod to fix this.", (LogLevel)3);
		}
		helper.Events.GameLoop.GameLaunched += OnGameLaunched;
		helper.Events.GameLoop.DayStarted += OnDayStarted;
		helper.Events.Display.RenderedHud += OnRenderedHud;
		helper.Events.Display.MenuChanged += OnMenuChanged;
		helper.Events.Input.ButtonsChanged += OnButtonsChanged;
	}

	private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
	{
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		if (!this.IsDataValid)
		{
			return;
		}
		this.GameHelper = new GameHelper(this.Metadata, ((Mod)this).Monitor, ((Mod)this).Helper.ModRegistry, ((Mod)this).Helper.Reflection);
		this.TargetFactory = new TargetFactory(((Mod)this).Helper.Reflection, this.GameHelper, () => this.Config, () => this.Config.EnableTileLookups);
		this.DebugInterface = new PerScreen<DebugInterface>((Func<DebugInterface>)(() => new DebugInterface(this.GameHelper, this.TargetFactory, () => this.Config, ((Mod)this).Monitor)));
		((IMod)(object)this).AddGenericModConfigMenu(new GenericModConfigMenuIntegrationForLookupAnything(), () => this.Config, delegate(ModConfig config)
		{
			this.Config = config;
		});
		IconicFrameworkIntegration iconicFramework = new IconicFrameworkIntegration(((Mod)this).Helper.ModRegistry, ((Mod)this).Monitor);
		if (iconicFramework.IsLoaded)
		{
			iconicFramework.AddToolbarIcon("LooseSprites/Cursors", (Rectangle?)new Rectangle(330, 357, 7, 13), (Func<string>?)I18n.Icon_ToggleSearch_Name, (Func<string>?)I18n.Icon_ToggleSearch_Desc, (Action)delegate
			{
				this.ShowLookup(ignoreCursor: true);
			}, (Action?)TryToggleSearch);
		}
	}

	private void OnDayStarted(object? sender, DayStartedEventArgs e)
	{
		if (this.IsDataValid)
		{
			this.GameHelper.ResetCache(((Mod)this).Monitor);
		}
	}

	private void OnButtonsChanged(object? sender, ButtonsChangedEventArgs e)
	{
		if (!this.IsDataValid)
		{
			return;
		}
		((Mod)this).Monitor.InterceptErrors("handling your input", delegate
		{
			//IL_0150: Unknown result type (might be due to invalid IL or missing references)
			//IL_0156: Invalid comparison between Unknown and I4
			ModConfigKeys keys = this.Keys;
			if (keys.ToggleSearch.JustPressed())
			{
				this.TryToggleSearch();
			}
			else if (keys.ToggleLookup.JustPressed())
			{
				this.ToggleLookup();
			}
			else if (keys.ScrollUp.JustPressed())
			{
				(Game1.activeClickableMenu as IScrollableMenu)?.ScrollUp();
			}
			else if (keys.ScrollDown.JustPressed())
			{
				(Game1.activeClickableMenu as IScrollableMenu)?.ScrollDown();
			}
			else if (keys.PageUp.JustPressed())
			{
				(Game1.activeClickableMenu as IScrollableMenu)?.ScrollUp(Game1.activeClickableMenu.height);
			}
			else if (keys.PageDown.JustPressed())
			{
				(Game1.activeClickableMenu as IScrollableMenu)?.ScrollDown(Game1.activeClickableMenu.height);
			}
			else if (keys.ToggleDebug.JustPressed() && Context.IsPlayerFree)
			{
				this.DebugInterface.Value.Enabled = !this.DebugInterface.Value.Enabled;
			}
			if (this.Config.HideOnKeyUp && (int)keys.ToggleLookup.GetState() == 3)
			{
				this.HideLookup();
			}
		});
	}

	private void OnMenuChanged(object? sender, MenuChangedEventArgs e)
	{
		((Mod)this).Monitor.InterceptErrors("restoring the previous menu", delegate
		{
			bool flag = e.NewMenu == null;
			bool flag2 = flag;
			if (flag2)
			{
				IClickableMenu oldMenu = e.OldMenu;
				bool flag3 = ((oldMenu is LookupMenu || oldMenu is SearchMenu) ? true : false);
				flag2 = flag3;
			}
			if (flag2 && this.PreviousMenus.Value.Any())
			{
				Game1.activeClickableMenu = this.PreviousMenus.Value.Pop();
			}
		});
	}

	private void OnRenderedHud(object? sender, RenderedHudEventArgs e)
	{
		if (this.IsDataValid && this.DebugInterface.Value.Enabled)
		{
			this.DebugInterface.Value.Draw(Game1.spriteBatch);
		}
	}

	private void ToggleLookup()
	{
		if (Game1.activeClickableMenu is LookupMenu)
		{
			this.HideLookup();
		}
		else
		{
			this.ShowLookup();
		}
	}

	private void ShowLookup(bool ignoreCursor = false)
	{
		if (!this.IsDataValid)
		{
			return;
		}
		StringBuilder logMessage = new StringBuilder("Received a lookup request...");
		((Mod)this).Monitor.InterceptErrors("looking that up", delegate
		{
			try
			{
				ISubject subject = this.GetSubject(logMessage, ignoreCursor);
				if (subject == null)
				{
					((Mod)this).Monitor.Log($"{logMessage} no target found.", (LogLevel)0);
				}
				else
				{
					((Mod)this).Monitor.Log(logMessage.ToString(), (LogLevel)0);
					this.ShowLookupFor(subject);
				}
			}
			catch
			{
				((Mod)this).Monitor.Log($"{logMessage} an error occurred.", (LogLevel)0);
				throw;
			}
		});
	}

	internal void ShowLookupFor(ISubject subject)
	{
		((Mod)this).Monitor.InterceptErrors("looking that up", delegate
		{
			((Mod)this).Monitor.Log($"Showing {subject.GetType().Name}::{subject.Type}::{subject.Name}.", (LogLevel)0);
			this.PushMenu((IClickableMenu)(object)new LookupMenu(subject, ((Mod)this).Monitor, ((Mod)this).Helper.Reflection, this.Config.ScrollAmount, this.Config.ShowDataMiningFields, this.Config.ForceFullScreen, ShowLookupFor));
		});
	}

	private void HideLookup()
	{
		((Mod)this).Monitor.InterceptErrors("closing the menu", delegate
		{
			if (Game1.activeClickableMenu is LookupMenu lookupMenu)
			{
				lookupMenu.QueueExit();
			}
		});
	}

	private void TryToggleSearch()
	{
		if (Game1.activeClickableMenu is SearchMenu)
		{
			this.HideSearch();
		}
		else if (Context.IsWorldReady && !(Game1.activeClickableMenu is LookupMenu))
		{
			this.ShowSearch();
		}
	}

	private void ShowSearch()
	{
		if (this.IsDataValid)
		{
			this.PushMenu((IClickableMenu)(object)new SearchMenu(this.TargetFactory.GetSearchSubjects(), ShowLookupFor, ((Mod)this).Monitor, this.Config.ScrollAmount));
		}
	}

	private void HideSearch()
	{
		if (Game1.activeClickableMenu is SearchMenu)
		{
			Game1.playSound("bigDeSelect", (int?)null);
			Game1.activeClickableMenu = null;
		}
	}

	private ModConfig LoadConfig()
	{
		try
		{
			if (File.Exists(Path.Combine(((Mod)this).Helper.DirectoryPath, "config.json")))
			{
				JObject model = ((Mod)this).Helper.ReadConfig<JObject>();
				JObject controls = model.Value<JObject>("Controls");
				string toggleLookup = controls?.Value<string>("ToggleLookup");
				string toggleLookupInFrontOfPlayer = controls?.Value<string>("ToggleLookupInFrontOfPlayer");
				if (!string.IsNullOrWhiteSpace(toggleLookupInFrontOfPlayer))
				{
					controls.Remove("ToggleLookupInFrontOfPlayer");
					controls["ToggleLookup"] = string.Join(", ", (from p in (toggleLookup ?? "").Split(',').Concat(toggleLookupInFrontOfPlayer.Split(','))
						select p.Trim() into p
						where p != ""
						select p).Distinct());
					((Mod)this).Helper.WriteConfig<JObject>(model);
				}
			}
		}
		catch (Exception ex)
		{
			((Mod)this).Monitor.Log("Couldn't migrate legacy settings in config.json; they'll be removed instead.", (LogLevel)3);
			((Mod)this).Monitor.Log(ex.ToString(), (LogLevel)0);
		}
		return ((Mod)this).Helper.ReadConfig<ModConfig>();
	}

	private ISubject? GetSubject(StringBuilder logMessage, bool ignoreCursor = false)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		if (!this.IsDataValid)
		{
			return null;
		}
		Vector2 cursorPos = this.GameHelper.GetScreenCoordinatesFromCursor();
		if (!Game1.uiMode)
		{
			cursorPos = Utility.ModifyCoordinatesForUIScale(cursorPos);
		}
		bool hasCursor = !ignoreCursor && (int)Constants.TargetPlatform != 0 && Game1.wasMouseVisibleThisFrame;
		if (Game1.activeClickableMenu != null)
		{
			StringBuilder stringBuilder = logMessage;
			StringBuilder stringBuilder2 = stringBuilder;
			StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(30, 1, stringBuilder);
			handler.AppendLiteral(" searching the open '");
			handler.AppendFormatted(((object)Game1.activeClickableMenu).GetType().Name);
			handler.AppendLiteral("' menu...");
			stringBuilder2.Append(ref handler);
			return this.TargetFactory.GetSubjectFrom(Game1.activeClickableMenu, cursorPos);
		}
		if (hasCursor)
		{
			foreach (IClickableMenu menu in Game1.onScreenMenus)
			{
				if (menu.isWithinBounds((int)cursorPos.X, (int)cursorPos.Y))
				{
					StringBuilder stringBuilder = logMessage;
					StringBuilder stringBuilder3 = stringBuilder;
					StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(35, 1, stringBuilder);
					handler.AppendLiteral(" searching the on-screen '");
					handler.AppendFormatted(((object)menu).GetType().Name);
					handler.AppendLiteral("' menu...");
					stringBuilder3.Append(ref handler);
					return this.TargetFactory.GetSubjectFrom(menu, cursorPos);
				}
			}
		}
		logMessage.Append(" searching the world...");
		return this.TargetFactory.GetSubjectFrom(Game1.player, Game1.currentLocation, hasCursor);
	}

	private void PushMenu(IClickableMenu menu)
	{
		if (this.ShouldRestoreMenu(Game1.activeClickableMenu))
		{
			this.PreviousMenus.Value.Push(Game1.activeClickableMenu);
			((Mod)this).Helper.Reflection.GetField<IClickableMenu>(typeof(Game1), "_activeClickableMenu", true).SetValue(menu);
		}
		else
		{
			Game1.activeClickableMenu = menu;
		}
	}

	private Metadata? LoadMetadata()
	{
		Metadata metadata = null;
		((Mod)this).Monitor.InterceptErrors("loading metadata", delegate
		{
			metadata = ((Mod)this).Helper.Data.ReadJsonFile<Metadata>(this.DatabaseFileName);
		});
		return metadata;
	}

	private bool ShouldRestoreMenu(IClickableMenu? menu)
	{
		if (menu == null)
		{
			return false;
		}
		if (this.Config.HideOnKeyUp && menu is LookupMenu)
		{
			return false;
		}
		return true;
	}
}
