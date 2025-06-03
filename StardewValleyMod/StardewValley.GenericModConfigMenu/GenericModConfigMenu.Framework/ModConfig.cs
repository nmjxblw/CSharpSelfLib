using System;
using System.Collections.Generic;
using GenericModConfigMenu.Framework.ModOption;
using StardewModdingAPI;

namespace GenericModConfigMenu.Framework;

internal class ModConfig
{
	private ModConfigPage ActiveRegisteringPage;

	public string ModName => this.ModManifest.Name;

	public IManifest ModManifest { get; }

	public Action Reset { get; }

	public Action Save { get; }

	public bool DefaultTitleScreenOnly { get; set; }

	public bool AnyEditableInGame { get; set; }

	public Dictionary<string, ModConfigPage> Pages { get; } = new Dictionary<string, ModConfigPage>();

	public ModConfigPage ActiveDisplayPage { get; set; }

	public List<Action<string, object>> ChangeHandlers { get; } = new List<Action<string, object>>();

	public ModConfig(IManifest manifest, Action reset, Action save, bool defaultTitleScreenOnly)
	{
		this.ModManifest = manifest;
		this.Reset = reset;
		this.Save = save;
		this.DefaultTitleScreenOnly = defaultTitleScreenOnly;
		this.SetActiveRegisteringPage("", null);
	}

	public void SetActiveRegisteringPage(string pageId, Func<string> pageTitle)
	{
		if (this.Pages.TryGetValue(pageId, out var page))
		{
			this.ActiveRegisteringPage = page;
		}
		else
		{
			this.Pages[pageId] = (this.ActiveRegisteringPage = new ModConfigPage(pageId, pageTitle));
		}
	}

	public void AddOption(BaseModOption option)
	{
		this.ActiveRegisteringPage.Options.Add(option);
		if (!this.DefaultTitleScreenOnly)
		{
			this.AnyEditableInGame = true;
		}
	}

	public IEnumerable<BaseModOption> GetAllOptions()
	{
		foreach (ModConfigPage page in this.Pages.Values)
		{
			foreach (BaseModOption option in page.Options)
			{
				yield return option;
			}
		}
	}
}
