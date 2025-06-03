using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI.Events;
using StardewValley.Menus;

namespace SpaceShared.APIs;

public interface IBetterGameMenuApi
{
	public delegate void DrawDelegate(SpriteBatch batch, Rectangle bounds);

	public delegate void TabContextMenuDelegate(ITabContextMenuEvent evt);

	public delegate void PageCreatedDelegate(IPageCreatedEvent evt);

	void RegisterImplementation(string id, int priority, Func<IClickableMenu, IClickableMenu> getPageInstance, Func<DrawDelegate?>? getDecoration = null, Func<bool>? getTabVisible = null, Func<bool>? getMenuInvisible = null, Func<int, int>? getWidth = null, Func<int, int>? getHeight = null, Func<(IClickableMenu Menu, IClickableMenu OldPage), IClickableMenu?>? onResize = null, Action<IClickableMenu>? onClose = null);

	IBetterGameMenu? AsMenu(IClickableMenu menu);

	void OnTabContextMenu(TabContextMenuDelegate handler, EventPriority priority = (EventPriority)0);

	void OffTabContextMenu(TabContextMenuDelegate handler);

	void OnPageCreated(PageCreatedDelegate handler, EventPriority priority = (EventPriority)0);

	void OffPageCreated(PageCreatedDelegate handler);
}
