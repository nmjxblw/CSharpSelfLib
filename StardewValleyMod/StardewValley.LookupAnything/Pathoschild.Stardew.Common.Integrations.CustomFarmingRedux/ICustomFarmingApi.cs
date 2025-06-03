using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace Pathoschild.Stardew.Common.Integrations.CustomFarmingRedux;

public interface ICustomFarmingApi
{
	Tuple<Item, Texture2D, Rectangle, Color>? getRealItemAndTexture(Object dummy);
}
