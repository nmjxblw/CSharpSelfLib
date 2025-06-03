using System;
using System.Collections.Generic;
using StardewValley.Menus;

namespace SpaceShared.APIs;

public interface ITabContextMenuEvent
{
	IClickableMenu Menu { get; }

	bool IsCurrentTab { get; }

	string Tab { get; }

	IClickableMenu? Page { get; }

	IList<ITabContextMenuEntry> Entries { get; }

	ITabContextMenuEntry CreateEntry(string label, Action? onSelect, IBetterGameMenuApi.DrawDelegate? icon = null);
}
