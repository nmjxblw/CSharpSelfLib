using StardewValley.Menus;

namespace SpaceShared.APIs;

public interface IPageCreatedEvent
{
	IClickableMenu Menu { get; }

	string Tab { get; }

	string Source { get; }

	IClickableMenu Page { get; }

	IClickableMenu? OldPage { get; }
}
