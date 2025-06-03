using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Netcode;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Extensions;
using StardewValley.GameData.Buildings;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Objects;

namespace Pathoschild.Stardew.Common;

public static class MachineDataHelper
{
	public static void GetBuildingChestNames(BuildingData? data, ISet<string> inputChests, ISet<string> outputChests)
	{
		int? num = data?.ItemConversions?.Count;
		if (!num.HasValue || num.GetValueOrDefault() <= 0)
		{
			return;
		}
		foreach (BuildingItemConversion rule in data.ItemConversions)
		{
			if (rule?.SourceChest != null)
			{
				inputChests.Add(rule.SourceChest);
			}
			if (rule?.DestinationChest != null)
			{
				outputChests.Add(rule.DestinationChest);
			}
		}
	}

	public static bool TryGetBuildingChestNames(BuildingData? data, out ISet<string> inputChests, out ISet<string> outputChests)
	{
		inputChests = new HashSet<string>();
		outputChests = new HashSet<string>();
		MachineDataHelper.GetBuildingChestNames(data, inputChests, outputChests);
		if (inputChests.Count <= 0)
		{
			return outputChests.Count > 0;
		}
		return true;
	}

	public static IEnumerable<Chest> GetBuildingChests(Building building, ISet<string> chestNames)
	{
		Enumerator<Chest, NetRef<Chest>> enumerator = building.buildingChests.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				Chest chest = enumerator.Current;
				if (chestNames.Contains(((Item)chest).Name))
				{
					yield return chest;
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to .constrained prefix*/).Dispose();
		}
	}

	public static bool TryGetUniqueItemFromContextTag(string contextTag, [NotNullWhen(true)] out ParsedItemData? data)
	{
		if (contextTag.StartsWith("id_"))
		{
			string rawIdentifier = contextTag.Substring(3, contextTag.Length - 3);
			string qualifiedId = null;
			if (rawIdentifier.StartsWith('('))
			{
				qualifiedId = rawIdentifier;
			}
			else
			{
				string[] parts = rawIdentifier.Split('_', 2);
				foreach (IItemDataDefinition type in ItemRegistry.ItemTypes)
				{
					if (string.Equals(parts[0], type.StandardDescriptor, StringComparison.InvariantCultureIgnoreCase))
					{
						qualifiedId = type.Identifier + parts[1];
						break;
					}
				}
			}
			data = ItemRegistry.GetData(qualifiedId);
			return data != null;
		}
		data = null;
		return false;
	}

	public static bool TryGetUniqueItemFromGameStateQuery(string query, [NotNullWhen(true)] out ParsedItemData? data)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		data = null;
		ParsedGameStateQuery[] array = GameStateQuery.Parse(query);
		foreach (ParsedGameStateQuery condition in array)
		{
			if (condition.Error != null)
			{
				continue;
			}
			string queryName = ArgUtility.Get(condition.Query, 0, (string)null, true);
			ParsedItemData itemData;
			if (StringExtensions.EqualsIgnoreCase(queryName, "ITEM_ID"))
			{
				if (condition.Query.Length == 3 && StringExtensions.EqualsIgnoreCase(condition.Query[1], "Input"))
				{
					string itemId = ItemRegistry.QualifyItemId(condition.Query[2]);
					if (data == null)
					{
						data = ItemRegistry.GetData(itemId);
					}
					else if (data.QualifiedItemId != itemId)
					{
						data = null;
						return false;
					}
				}
			}
			else if (StringExtensions.EqualsIgnoreCase(queryName, "ITEM_CONTEXT_TAG") && condition.Query.Length == 3 && StringExtensions.EqualsIgnoreCase(condition.Query[1], "Input") && MachineDataHelper.TryGetUniqueItemFromContextTag(condition.Query[2], out itemData))
			{
				if (data == null)
				{
					data = itemData;
				}
				else if (data.QualifiedItemId != itemData.QualifiedItemId)
				{
					data = null;
					return false;
				}
			}
		}
		return data != null;
	}
}
