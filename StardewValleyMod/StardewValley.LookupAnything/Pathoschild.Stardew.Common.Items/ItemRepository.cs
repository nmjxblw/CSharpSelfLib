using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Content;
using StardewValley;
using StardewValley.GameData;
using StardewValley.GameData.FishPonds;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Objects;

namespace Pathoschild.Stardew.Common.Items;

internal class ItemRepository
{
	public IEnumerable<SearchableItem> GetAll(string? onlyType = null, bool includeVariants = true)
	{
		return from item in GetAllRaw()
			where item != null
			select item;
		IEnumerable<SearchableItem?> GetAllRaw()
		{
			foreach (IItemDataDefinition itemType in ItemRegistry.ItemTypes)
			{
				if (onlyType == null || !(itemType.Identifier != onlyType))
				{
					string identifier = itemType.Identifier;
					if (identifier == "(O)")
					{
						ObjectDataDefinition objectDataDefinition = (ObjectDataDefinition)ItemRegistry.GetTypeDefinition("(O)");
						foreach (string id in itemType.GetAllIds())
						{
							SearchableItem result = this.TryCreate(itemType.Identifier, id, (SearchableItem p) => ItemRegistry.Create(itemType.Identifier + p.Id, 1, 0, false));
							if (result?.Item is Ring)
							{
								yield return result;
							}
							else if (result?.QualifiedItemId == "(O)842")
							{
								foreach (SearchableItem secretNote in this.GetSecretNotes(itemType, isJournalScrap: true))
								{
									yield return secretNote;
								}
							}
							else if (result?.QualifiedItemId == "(O)79")
							{
								foreach (SearchableItem secretNote2 in this.GetSecretNotes(itemType, isJournalScrap: false))
								{
									yield return secretNote2;
								}
							}
							else
							{
								switch (result?.QualifiedItemId)
								{
								case "(O)340":
									yield return this.TryCreate(itemType.Identifier, result.Id, (SearchableItem _) => (Item)(object)objectDataDefinition.CreateFlavoredHoney((Object)null));
									break;
								default:
									if (result != null)
									{
										yield return result;
									}
									break;
								case "(O)DriedFruit":
								case "(O)DriedMushrooms":
								case "(O)SmokedFish":
								case "(O)SpecificBait":
									break;
								}
								if (includeVariants)
								{
									ItemRepository itemRepository = this;
									ObjectDataDefinition objectDataDefinition2 = objectDataDefinition;
									Item obj = result?.Item;
									foreach (SearchableItem flavoredObjectVariant in itemRepository.GetFlavoredObjectVariants(objectDataDefinition2, (Object?)(object)((obj is Object) ? obj : null), itemType))
									{
										yield return flavoredObjectVariant;
									}
								}
							}
						}
					}
					else
					{
						foreach (string id2 in itemType.GetAllIds())
						{
							yield return this.TryCreate(itemType.Identifier, id2, (SearchableItem p) => ItemRegistry.Create(itemType.Identifier + p.Id, 1, 0, false));
						}
					}
				}
			}
		}
	}

	private IEnumerable<SearchableItem?> GetSecretNotes(IItemDataDefinition itemType, bool isJournalScrap)
	{
		string baseId = (isJournalScrap ? "842" : "79");
		IEnumerable<int> ids = this.TryLoad(() => DataLoader.SecretNotes(Game1.content)).Keys.Where(isJournalScrap ? ((Func<int, bool>)((int num) => num >= GameLocation.JOURNAL_INDEX)) : ((Func<int, bool>)((int num) => num < GameLocation.JOURNAL_INDEX))).Select(isJournalScrap ? ((Func<int, int>)((int num) => num - GameLocation.JOURNAL_INDEX)) : ((Func<int, int>)((int result) => result)));
		foreach (int i in ids)
		{
			int id = i;
			yield return this.TryCreate(itemType.Identifier, $"{baseId}/{id}", delegate
			{
				Item val = ItemRegistry.Create(itemType.Identifier + baseId, 1, 0, false);
				val.Name = $"{val.Name} #{id}";
				return val;
			});
		}
	}

	private IEnumerable<SearchableItem?> GetFlavoredObjectVariants(ObjectDataDefinition objectDataDefinition, Object? item, IItemDataDefinition itemType)
	{
		if (item == null)
		{
			yield break;
		}
		string id = ((Item)item).ItemId;
		switch (((Item)item).Category)
		{
		case -4:
			yield return this.TryCreate(itemType.Identifier, "SmokedFish/" + id, (SearchableItem _) => (Item)(object)objectDataDefinition.CreateFlavoredSmokedFish(item));
			yield return this.TryCreate(itemType.Identifier, "SpecificBait/" + id, (SearchableItem _) => (Item)(object)objectDataDefinition.CreateFlavoredBait(item));
			break;
		case -79:
			yield return this.TryCreate(itemType.Identifier, "348/" + id, (SearchableItem _) => (Item)(object)objectDataDefinition.CreateFlavoredWine(item));
			yield return this.TryCreate(itemType.Identifier, "344/" + id, (SearchableItem _) => (Item)(object)objectDataDefinition.CreateFlavoredJelly(item));
			if (((Item)item).QualifiedItemId != "(O)398")
			{
				yield return this.TryCreate(itemType.Identifier, "398/" + id, (SearchableItem _) => (Item)(object)objectDataDefinition.CreateFlavoredDriedFruit(item));
			}
			break;
		case -81:
			yield return this.TryCreate(itemType.Identifier, "342/" + id, (SearchableItem _) => (Item)(object)objectDataDefinition.CreateFlavoredPickle(item));
			break;
		case -75:
			yield return this.TryCreate(itemType.Identifier, "350/" + id, (SearchableItem _) => (Item)(object)objectDataDefinition.CreateFlavoredJuice(item));
			yield return this.TryCreate(itemType.Identifier, "342/" + id, (SearchableItem _) => (Item)(object)objectDataDefinition.CreateFlavoredPickle(item));
			break;
		case -80:
			yield return this.TryCreate(itemType.Identifier, "340/" + id, (SearchableItem _) => (Item)(object)objectDataDefinition.CreateFlavoredHoney(item));
			break;
		case -23:
		{
			if (!(((Item)item).QualifiedItemId == "(O)812"))
			{
				break;
			}
			this.GetRoeContextTagLookups(out HashSet<string> simpleTags, out List<List<string>> complexTags);
			Object input = default(Object);
			foreach (string key in Game1.objectData.Keys)
			{
				ref Object reference = ref input;
				Item obj = this.TryCreate(itemType.Identifier, key, (SearchableItem p) => (Item)new Object(p.Id, 1, false, -1, 0))?.Item;
				reference = (Object)(object)((obj is Object) ? obj : null);
				if (input == null)
				{
					continue;
				}
				HashSet<string> inputTags = ((Item)input).GetContextTags();
				if (!inputTags.Any() || (!inputTags.Any((string tag) => simpleTags.Contains(tag)) && !complexTags.Any((List<string> set) => set.All((string tag) => ((Item)input).HasContextTag(tag)))))
				{
					continue;
				}
				SearchableItem roe = this.TryCreate(itemType.Identifier, "812/" + ((Item)input).ItemId, (SearchableItem _) => (Item)(object)objectDataDefinition.CreateFlavoredRoe(input));
				yield return roe;
				Item val = roe?.Item;
				Object roeObj = (Object)(object)((val is Object) ? val : null);
				if (roeObj != null && ((Item)input).QualifiedItemId != "(O)698")
				{
					yield return this.TryCreate(itemType.Identifier, "447/" + ((Item)input).ItemId, (SearchableItem _) => (Item)(object)objectDataDefinition.CreateFlavoredAgedRoe(roeObj));
				}
			}
			break;
		}
		}
		bool flag = ((Item)item).HasContextTag("preserves_pickle");
		bool flag2 = flag;
		if (flag2)
		{
			int category = ((Item)item).Category;
			bool flag3 = ((category == -81 || category == -75) ? true : false);
			flag2 = !flag3;
		}
		if (flag2)
		{
			yield return this.TryCreate(itemType.Identifier, "342/" + id, (SearchableItem _) => (Item)(object)objectDataDefinition.CreateFlavoredPickle(item));
		}
		if (((Item)item).HasContextTag("edible_mushroom"))
		{
			yield return this.TryCreate(itemType.Identifier, "DriedMushrooms/" + id, (SearchableItem _) => (Item)(object)objectDataDefinition.CreateFlavoredDriedMushroom(item));
		}
	}

	private void GetRoeContextTagLookups(out HashSet<string> simpleTags, out List<List<string>> complexTags)
	{
		simpleTags = new HashSet<string>();
		complexTags = new List<List<string>>();
		foreach (FishPondData data in this.TryLoad(() => DataLoader.FishPondData(Game1.content)))
		{
			if (!data.ProducedItems.All(delegate(FishPondReward p)
			{
				string itemId = ((GenericSpawnItemData)p).ItemId;
				bool flag = ((itemId == "812" || itemId == "(O)812") ? true : false);
				return !flag;
			}))
			{
				if (data.RequiredTags.Count == 1 && !data.RequiredTags[0].StartsWith("!"))
				{
					simpleTags.Add(data.RequiredTags[0]);
				}
				else
				{
					complexTags.Add(data.RequiredTags);
				}
			}
		}
	}

	private TAsset TryLoad<TAsset>(Func<TAsset> load) where TAsset : new()
	{
		try
		{
			return load();
		}
		catch (ContentLoadException)
		{
			return new TAsset();
		}
	}

	private SearchableItem? TryCreate(string type, string key, Func<SearchableItem, Item> createItem)
	{
		try
		{
			SearchableItem item = new SearchableItem(type, key, createItem);
			item.Item.getDescription();
			string name = item.Item.Name;
			if ((name == null || name == "Error Item") ? true : false)
			{
				return null;
			}
			return item;
		}
		catch
		{
			return null;
		}
	}
}
