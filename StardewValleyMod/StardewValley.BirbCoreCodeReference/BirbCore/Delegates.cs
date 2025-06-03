using System;
using System.Collections.Generic;
using BirbCore.Attributes;
using Netcode;
using StardewValley;
using StardewValley.Internal;

namespace BirbCore;

[SDelegate]
internal class Delegates
{
	[SDelegate.ResolveItemQuery]
	public static IList<ItemQueryResult> CustomFlavoredItem(string key, string arguments, ItemQueryContext _, bool _1, HashSet<string> _2, Action<string, string> logError)
	{
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Expected O, but got Unknown
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Expected O, but got Unknown
		string[] splitArgs = Helpers.SplitArguments(arguments);
		string flavoredItemId = default(string);
		string error = default(string);
		string ingredientItemId = default(string);
		string ingredientPreservedItemId = default(string);
		if (!ArgUtility.TryGet(splitArgs, 0, ref flavoredItemId, ref error, true, "string flavoredItemId") || !ArgUtility.TryGet(splitArgs, 1, ref ingredientItemId, ref error, true, "string ingredientItemId") || !ArgUtility.TryGetOptional(splitArgs, 2, ref ingredientPreservedItemId, ref error, (string)null, true, "string ingredientPreservedItemId"))
		{
			return Helpers.ErrorResult(key, arguments, logError, error);
		}
		Item obj = ItemRegistry.Create(ingredientPreservedItemId ?? ingredientItemId, 1, 0, false);
		Object ingredient = (Object)(object)((obj is Object) ? obj : null);
		Item obj2 = ItemRegistry.Create(flavoredItemId, 1, 0, false);
		Object flavored = (Object)(object)((obj2 is Object) ? obj2 : null);
		if (flavored == null)
		{
			return null;
		}
		if (ingredient != null)
		{
			((Item)flavored).Name = ((Item)flavored).Name + ((Item)ingredient).Name;
			((NetFieldBase<string, NetString>)(object)flavored.preservedParentSheetIndex).Value = ((Item)ingredient).ItemId;
			return (IList<ItemQueryResult>)(object)new ItemQueryResult[1]
			{
				new ItemQueryResult((ISalable)(object)flavored)
			};
		}
		return (IList<ItemQueryResult>)(object)new ItemQueryResult[1]
		{
			new ItemQueryResult((ISalable)(object)flavored)
		};
	}
}
