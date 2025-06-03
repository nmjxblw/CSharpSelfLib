using System;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace Pathoschild.Stardew.Common.Integrations.GenericModConfigMenu;

internal class GenericModConfigMenuIntegration<TConfig> : BaseIntegration<IGenericModConfigMenuApi> where TConfig : new()
{
	private readonly IManifest ConsumerManifest;

	private readonly Func<TConfig> GetConfig;

	private readonly Action Reset;

	private readonly Action SaveAndApply;

	public GenericModConfigMenuIntegration(IModRegistry modRegistry, IMonitor monitor, IManifest manifest, Func<TConfig> getConfig, Action reset, Action saveAndApply)
		: base("Generic Mod Config Menu", "spacechase0.GenericModConfigMenu", "1.9.6", modRegistry, monitor)
	{
		this.ConsumerManifest = manifest;
		this.GetConfig = getConfig;
		this.Reset = reset;
		this.SaveAndApply = saveAndApply;
	}

	public GenericModConfigMenuIntegration<TConfig> Register(bool titleScreenOnly = false)
	{
		this.AssertLoaded();
		base.ModApi.Register(this.ConsumerManifest, this.Reset, this.SaveAndApply, titleScreenOnly);
		return this;
	}

	public GenericModConfigMenuIntegration<TConfig> AddSectionTitle(Func<string> text, Func<string>? tooltip = null)
	{
		this.AssertLoaded();
		base.ModApi.AddSectionTitle(this.ConsumerManifest, text, tooltip);
		return this;
	}

	public GenericModConfigMenuIntegration<TConfig> AddParagraph(Func<string> text)
	{
		this.AssertLoaded();
		base.ModApi.AddParagraph(this.ConsumerManifest, text);
		return this;
	}

	public GenericModConfigMenuIntegration<TConfig> AddCheckbox(Func<string> name, Func<string> tooltip, Func<TConfig, bool> get, Action<TConfig, bool> set, bool enable = true)
	{
		this.AssertLoaded();
		if (enable)
		{
			base.ModApi.AddBoolOption(this.ConsumerManifest, () => get(this.GetConfig()), delegate(bool val)
			{
				set(this.GetConfig(), val);
			}, name, tooltip);
		}
		return this;
	}

	public GenericModConfigMenuIntegration<TConfig> AddDropdown(Func<string> name, Func<string> tooltip, Func<TConfig, string> get, Action<TConfig, string> set, string[] allowedValues, Func<string, string> formatAllowedValue, bool enable = true)
	{
		this.AssertLoaded();
		if (enable)
		{
			base.ModApi.AddTextOption(this.ConsumerManifest, () => get(this.GetConfig()), delegate(string val)
			{
				set(this.GetConfig(), val);
			}, name, tooltip, allowedValues, formatAllowedValue);
		}
		return this;
	}

	public GenericModConfigMenuIntegration<TConfig> AddTextbox(Func<string> name, Func<string> tooltip, Func<TConfig, string> get, Action<TConfig, string> set, bool enable = true)
	{
		this.AssertLoaded();
		if (enable)
		{
			base.ModApi.AddTextOption(this.ConsumerManifest, () => get(this.GetConfig()), delegate(string val)
			{
				set(this.GetConfig(), val);
			}, name, tooltip);
		}
		return this;
	}

	public GenericModConfigMenuIntegration<TConfig> AddNumberField(Func<string> name, Func<string> tooltip, Func<TConfig, int> get, Action<TConfig, int> set, int min, int max, bool enable = true)
	{
		this.AssertLoaded();
		if (enable)
		{
			base.ModApi.AddNumberOption(this.ConsumerManifest, () => get(this.GetConfig()), delegate(int val)
			{
				set(this.GetConfig(), val);
			}, name, tooltip, min, max);
		}
		return this;
	}

	public GenericModConfigMenuIntegration<TConfig> AddNumberField(Func<string> name, Func<string> tooltip, Func<TConfig, float> get, Action<TConfig, float> set, float min, float max, bool enable = true, float interval = 0.1f)
	{
		this.AssertLoaded();
		if (enable)
		{
			base.ModApi.AddNumberOption(this.ConsumerManifest, () => get(this.GetConfig()), delegate(float val)
			{
				set(this.GetConfig(), val);
			}, name, tooltip, min, max, interval);
		}
		return this;
	}

	public GenericModConfigMenuIntegration<TConfig> AddKeyBinding(Func<string> name, Func<string> tooltip, Func<TConfig, KeybindList> get, Action<TConfig, KeybindList> set, bool enable = true)
	{
		this.AssertLoaded();
		if (enable)
		{
			base.ModApi.AddKeybindList(this.ConsumerManifest, () => get(this.GetConfig()), delegate(KeybindList val)
			{
				set(this.GetConfig(), val);
			}, name, tooltip);
		}
		return this;
	}
}
internal static class GenericModConfigMenuIntegration
{
	public static void AddGenericModConfigMenu<TConfig>(this IMod mod, IGenericModConfigMenuIntegrationFor<TConfig> configMenu, Func<TConfig> get, Action<TConfig> set, Action? onSaved = null) where TConfig : class, new()
	{
		GenericModConfigMenuIntegration<TConfig> api = new GenericModConfigMenuIntegration<TConfig>(mod.Helper.ModRegistry, mod.Monitor, mod.ModManifest, get, Reset, SaveAndApply);
		if (api.IsLoaded)
		{
			configMenu.Register(api, mod.Monitor);
		}
		void Reset()
		{
			set(new TConfig());
			mod.Helper.WriteConfig<TConfig>(get());
		}
		void SaveAndApply()
		{
			mod.Helper.WriteConfig<TConfig>(get());
			onSaved?.Invoke();
		}
	}
}
