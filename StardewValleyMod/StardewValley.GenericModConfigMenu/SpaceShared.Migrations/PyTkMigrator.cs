using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Objects;

namespace SpaceShared.Migrations;

internal class PyTkMigrator
{
	public static void MigrateBuildings(SaveGame loaded, Dictionary<string, Func<Building, IDictionary<string, string>, Building>> getReplacements)
	{
		foreach (GameLocation location in loaded.locations.OfType<GameLocation>())
		{
			for (int i = 0; i < location.buildings.Count; i++)
			{
				Building building = location.buildings[i];
				Chest buildingChest = building.GetBuildingChest("Input");
				if (PyTkMigrator.TryParseSerializedString((buildingChest != null) ? ((Item)buildingChest).Name : null, out var actualType, out var customData) && getReplacements.TryGetValue(actualType, out var getReplacement))
				{
					Building replacement = getReplacement(building, customData);
					location.buildings[i] = replacement;
				}
			}
		}
	}

	public static void MigrateItems(string type, Func<IDictionary<string, string>, Item> getReplacement)
	{
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		foreach (Farmer player in Game1.getAllFarmers())
		{
			PyTkMigrator.TryMigrate((IList<Item>)player.Items, type, getReplacement);
		}
		foreach (GameLocation location in CommonHelper.GetLocations())
		{
			KeyValuePair<Vector2, Object>[] array = location.Objects.Pairs.ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				KeyValuePair<Vector2, Object> keyValuePair = array[i];
				var (key, obj) = (KeyValuePair<Vector2, Object>)(ref keyValuePair);
				if (PyTkMigrator.TryMigrate((Item)(object)obj, type, getReplacement, out var replaceWith))
				{
					Object replaceWithObj = (Object)(object)((replaceWith is Object) ? replaceWith : null);
					if (replaceWithObj != null)
					{
						location.Objects[key] = replaceWithObj;
					}
				}
			}
			foreach (StorageFurniture furniture in ((IEnumerable)location.furniture).OfType<StorageFurniture>())
			{
				PyTkMigrator.TryMigrate((IList<Item>)furniture.heldItems, type, getReplacement);
			}
		}
	}

	private static void TryMigrate(IList<Item> items, string type, Func<IDictionary<string, string>, Item> getReplacement)
	{
		for (int i = 0; i < items.Count; i++)
		{
			if (PyTkMigrator.TryMigrate(items[i], type, getReplacement, out var newItem))
			{
				items[i] = newItem;
			}
		}
	}

	private static bool TryMigrate(Item item, string type, Func<IDictionary<string, string>, Item> getReplacement, out Item replaceWith)
	{
		if (PyTkMigrator.TryParseSerializedString((item != null) ? item.Name : null, out var actualType, out var customData) && actualType == type)
		{
			replaceWith = getReplacement(customData);
			return true;
		}
		Chest chest = (Chest)(object)((item is Chest) ? item : null);
		if (chest != null)
		{
			PyTkMigrator.TryMigrate((IList<Item>)chest.Items, type, getReplacement);
		}
		replaceWith = null;
		return false;
	}

	private static bool TryParseSerializedString(string serialized, out string type, out IDictionary<string, string> customData)
	{
		if (serialized == null || !serialized.StartsWith("PyTK|Item|"))
		{
			type = null;
			customData = null;
			return false;
		}
		string[] fields = serialized.Split('|');
		type = fields[2];
		customData = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		foreach (string field in fields.Skip(3))
		{
			string[] parts = field.Split('=', 2);
			customData[parts[0]] = parts.GetOrDefault(1);
		}
		return true;
	}
}
