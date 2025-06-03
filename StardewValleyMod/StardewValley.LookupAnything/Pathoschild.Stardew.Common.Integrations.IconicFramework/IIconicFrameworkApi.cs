using System;
using Microsoft.Xna.Framework;

namespace Pathoschild.Stardew.Common.Integrations.IconicFramework;

public interface IIconicFrameworkApi
{
	void AddToolbarIcon(string texturePath, Rectangle? sourceRect, Func<string>? getTitle, Func<string>? getDescription, Action onClick, Action? onRightClick = null);
}
