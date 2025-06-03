using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using xTile.Dimensions;

namespace Pathoschild.Stardew.LookupAnything.Components;

internal class BaseMenu : IClickableMenu
{
	protected static bool UseSafeDimensions { get; set; }

	protected Point GetViewportSize()
	{
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		Point viewport = default(Point);
		((Point)(ref viewport))._002Ector(((Rectangle)(ref Game1.uiViewport)).Width, ((Rectangle)(ref Game1.uiViewport)).Height);
		if (BaseMenu.UseSafeDimensions)
		{
			Viewport viewport2 = Game1.graphics.GraphicsDevice.Viewport;
			if (((Viewport)(ref viewport2)).Width < viewport.X)
			{
				int x = viewport.X;
				viewport2 = Game1.graphics.GraphicsDevice.Viewport;
				int num = Math.Min(x, ((Viewport)(ref viewport2)).Width);
				int y = viewport.Y;
				viewport2 = Game1.graphics.GraphicsDevice.Viewport;
				((Point)(ref viewport))._002Ector(num, Math.Min(y, ((Viewport)(ref viewport2)).Height));
			}
		}
		return viewport;
	}
}
