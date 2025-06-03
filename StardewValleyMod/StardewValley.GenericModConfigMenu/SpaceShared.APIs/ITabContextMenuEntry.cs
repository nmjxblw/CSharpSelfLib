using System;

namespace SpaceShared.APIs;

public interface ITabContextMenuEntry
{
	string Label { get; }

	Action? OnSelect { get; }

	IBetterGameMenuApi.DrawDelegate? Icon { get; }
}
