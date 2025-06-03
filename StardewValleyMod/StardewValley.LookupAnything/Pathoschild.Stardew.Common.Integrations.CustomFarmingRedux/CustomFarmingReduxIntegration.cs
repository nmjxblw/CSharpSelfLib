using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;

namespace Pathoschild.Stardew.Common.Integrations.CustomFarmingRedux;

internal class CustomFarmingReduxIntegration : BaseIntegration<ICustomFarmingApi>
{
	public CustomFarmingReduxIntegration(IModRegistry modRegistry, IMonitor monitor)
		: base("Custom Farming Redux", "Platonymous.CustomFarming", "2.8.5", modRegistry, monitor)
	{
	}

	public SpriteInfo? GetSprite(Object obj)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		this.AssertLoaded();
		Tuple<Item, Texture2D, Rectangle, Color> data = base.ModApi.getRealItemAndTexture(obj);
		if (data == null)
		{
			return null;
		}
		return new SpriteInfo(data.Item2, data.Item3);
	}
}
