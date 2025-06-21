using StardewValley;
using StardewValley.ItemTypeDefinitions;

namespace Firearm.Framework.Patches;

public class WeaponDataDefinitionPatches
{
	internal static void CreateItem(ref Item __result, ParsedItemData data)
	{
		string itemId = data.ItemId;
		if (1 == 0)
		{
		}
		Item val = (Item)(object)((itemId == "Firearm_AK47") ? new Firearm("Firearm_AK47") : ((!(itemId == "Firearm_M16")) ? ((Firearm)(object)__result) : new Firearm("Firearm_M16")));
		if (1 == 0)
		{
		}
		__result = val;
	}
}
