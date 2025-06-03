using System;
using Microsoft.Xna.Framework;
using StardewModdingAPI;

namespace Pathoschild.Stardew.Common.Integrations.IconicFramework;

internal class IconicFrameworkIntegration : BaseIntegration<IIconicFrameworkApi>
{
	public IconicFrameworkIntegration(IModRegistry modRegistry, IMonitor monitor)
		: base("IconicFramework", "furyx639.ToolbarIcons", "3.1.0-beta.1", modRegistry, monitor)
	{
	}

	public void AddToolbarIcon(string texturePath, Rectangle? sourceRect, Func<string>? getTitle, Func<string>? getDescription, Action onClick, Action? onRightClick = null)
	{
		this.AssertLoaded();
		base.ModApi.AddToolbarIcon(texturePath, sourceRect, getTitle, getDescription, onClick, onRightClick);
	}
}
