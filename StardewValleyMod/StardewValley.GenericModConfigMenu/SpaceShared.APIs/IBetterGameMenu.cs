using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using StardewValley.Menus;

namespace SpaceShared.APIs;

public interface IBetterGameMenu
{
	IClickableMenu Menu { get; }

	bool Invisible { get; set; }

	IReadOnlyList<string> VisibleTabs { get; }

	string CurrentTab { get; }

	IClickableMenu? CurrentPage { get; }

	bool CurrentTabHasErrored { get; }

	bool TryGetSource(string target, [NotNullWhen(true)] out string? source);

	bool TryGetPage(string target, [NotNullWhen(true)] out IClickableMenu? page, bool forceCreation = false);

	bool TryChangeTab(string target, bool playSound = true);

	void UpdateTabs(string? target = null);
}
