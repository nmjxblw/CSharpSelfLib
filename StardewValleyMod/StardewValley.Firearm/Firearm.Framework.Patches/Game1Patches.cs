using StardewValley;

namespace Firearm.Framework.Patches;

public class Game1Patches
{
	internal static bool DrawTool(Farmer f)
	{
		if (!(f.CurrentTool is Firearm))
		{
			return true;
		}
		f.CurrentTool.draw(Game1.spriteBatch);
		return false;
	}
}
