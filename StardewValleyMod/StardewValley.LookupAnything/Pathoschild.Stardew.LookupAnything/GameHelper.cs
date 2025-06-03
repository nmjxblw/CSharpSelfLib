using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.Common.Integrations.BetterGameMenu;
using Pathoschild.Stardew.Common.Integrations.BushBloomMod;
using Pathoschild.Stardew.Common.Integrations.CustomBush;
using Pathoschild.Stardew.Common.Integrations.CustomFarmingRedux;
using Pathoschild.Stardew.Common.Integrations.ExtraMachineConfig;
using Pathoschild.Stardew.Common.Integrations.MultiFertilizer;
using Pathoschild.Stardew.Common.Integrations.ProducerFrameworkMod;
using Pathoschild.Stardew.Common.Integrations.SpaceCore;
using Pathoschild.Stardew.Common.Items;
using Pathoschild.Stardew.LookupAnything.Framework;
using Pathoschild.Stardew.LookupAnything.Framework.Constants;
using Pathoschild.Stardew.LookupAnything.Framework.Data;
using Pathoschild.Stardew.LookupAnything.Framework.ItemScanning;
using Pathoschild.Stardew.LookupAnything.Framework.Models;
using Pathoschild.Stardew.LookupAnything.Framework.Models.FishData;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Extensions;
using StardewValley.GameData.Crafting;
using StardewValley.GameData.Crops;
using StardewValley.GameData.FishPonds;
using StardewValley.GameData.Locations;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.Quests;
using StardewValley.SpecialOrders;
using xTile.Dimensions;

namespace Pathoschild.Stardew.LookupAnything;

internal class GameHelper
{
	private readonly CustomFarmingReduxIntegration CustomFarmingRedux;

	private readonly ProducerFrameworkModIntegration ProducerFrameworkMod;

	private readonly DataParser DataParser = new DataParser();

	private readonly WorldItemScanner WorldItemScanner;

	private readonly ItemRepository ItemRepository = new ItemRepository();

	private readonly IMonitor Monitor;

	private readonly IModRegistry ModRegistry;

	private Lazy<SearchableItem[]> Objects;

	private Lazy<RecipeModel[]> Recipes;

	public Metadata Metadata { get; }

	public BetterGameMenuIntegration BetterGameMenu { get; }

	public BushBloomModIntegration BushBloomMod { get; }

	public CustomBushIntegration CustomBush { get; }

	public ExtraMachineConfigIntegration ExtraMachineConfig { get; }

	public MultiFertilizerIntegration MultiFertilizer { get; }

	public SpaceCoreIntegration SpaceCore { get; }

	public GameHelper(Metadata metadata, IMonitor monitor, IModRegistry modRegistry, IReflectionHelper reflection)
	{
		this.Metadata = metadata;
		this.Monitor = monitor;
		this.ModRegistry = modRegistry;
		this.WorldItemScanner = new WorldItemScanner(reflection);
		this.BetterGameMenu = new BetterGameMenuIntegration(modRegistry, monitor);
		this.BushBloomMod = new BushBloomModIntegration(modRegistry, monitor);
		this.CustomBush = new CustomBushIntegration(modRegistry, monitor);
		this.CustomFarmingRedux = new CustomFarmingReduxIntegration(modRegistry, monitor);
		this.ExtraMachineConfig = new ExtraMachineConfigIntegration(modRegistry, monitor);
		this.MultiFertilizer = new MultiFertilizerIntegration(modRegistry, monitor);
		this.ProducerFrameworkMod = new ProducerFrameworkModIntegration(modRegistry, monitor);
		this.SpaceCore = new SpaceCoreIntegration(modRegistry, monitor);
		this.ResetCache(monitor);
	}

	[MemberNotNull(new string[] { "Objects", "Recipes" })]
	public void ResetCache(IMonitor monitor)
	{
		this.Objects = new Lazy<SearchableItem[]>(() => (from p in this.ItemRepository.GetAll("(O)")
			where !(p.Item is Ring)
			select p).ToArray());
		this.Recipes = new Lazy<RecipeModel[]>(() => this.GetAllRecipes(monitor).ToArray());
	}

	public string TranslateSeason(string season)
	{
		int number = Utility.getSeasonNumber(season);
		if (number == -1)
		{
			return season;
		}
		return Utility.getSeasonNameFromNumber(number);
	}

	public bool TryGetDate(int day, string season, out SDate date)
	{
		return this.TryGetDate(day, season, Game1.year, out date);
	}

	public bool TryGetDate(int day, string season, int year, out SDate date)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected O, but got Unknown
		try
		{
			date = new SDate(day, season, year);
			return true;
		}
		catch
		{
			date = SDate.Now();
			return false;
		}
	}

	public int GetShipped(string itemID)
	{
		if (!((NetDictionary<string, int, NetInt, SerializableDictionary<string, int>, NetStringDictionary<int, NetInt>>)(object)Game1.player.basicShipped).ContainsKey(itemID))
		{
			return 0;
		}
		return ((NetDictionary<string, int, NetInt, SerializableDictionary<string, int>, NetStringDictionary<int, NetInt>>)(object)Game1.player.basicShipped)[itemID];
	}

	public IEnumerable<KeyValuePair<string, bool>> GetFullShipmentAchievementItems()
	{
		return from entry in this.Objects.Value
			let obj = (Object)entry.Item
			where obj.Type != "Arch" && obj.Type != "Fish" && obj.Type != "Mineral" && obj.Type != "Cooking" && Object.isPotentialBasicShipped(((Item)obj).ItemId, ((Item)obj).Category, obj.Type)
			select new KeyValuePair<string, bool>(((Item)obj).QualifiedItemId, ((NetDictionary<string, int, NetInt, SerializableDictionary<string, int>, NetStringDictionary<int, NetInt>>)(object)Game1.player.basicShipped).ContainsKey(((Item)obj).ItemId));
	}

	public static CropData? GetCropDataByHarvestItem(string itemId)
	{
		foreach (CropData crop in Game1.cropData.Values)
		{
			if (crop.HarvestItemId == itemId)
			{
				return crop;
			}
		}
		return null;
	}

	public IEnumerable<FoundItem> GetAllOwnedItems()
	{
		return this.WorldItemScanner.GetAllOwnedItems();
	}

	public IEnumerable<NPC> GetAllCharacters()
	{
		return Utility.getAllCharacters().Distinct();
	}

	public int CountOwnedItems(Item item, bool flavorSpecific)
	{
		return (from found in this.GetAllOwnedItems()
			let foundItem = found.Item
			where this.AreEquivalent(foundItem, item, flavorSpecific)
			let canStack = foundItem.canStackWith((ISalable)(object)foundItem)
			select (!canStack) ? 1 : found.GetCount()).Sum();
	}

	public bool IsSocialVillager(NPC npc)
	{
		if (!((Character)npc).IsVillager)
		{
			return false;
		}
		if (this.Metadata.Constants.ForceSocialVillagers.TryGetValue(((Character)npc).Name, out var social))
		{
			return social;
		}
		if (!((NetDictionary<string, Friendship, NetRef<Friendship>, SerializableDictionary<string, Friendship>, NetStringDictionary<Friendship, NetRef<Friendship>>>)(object)Game1.player.friendshipData).ContainsKey(((Character)npc).Name))
		{
			return npc.CanSocialize;
		}
		return true;
	}

	public IEnumerable<GiftTasteModel> GetGiftTastes(Item item)
	{
		if (!item.canBeGivenAsGift())
		{
			yield break;
		}
		foreach (NPC npc in this.GetAllCharacters())
		{
			if (this.IsSocialVillager(npc))
			{
				GiftTaste? taste = this.GetGiftTaste(npc, item);
				if (taste.HasValue)
				{
					yield return new GiftTasteModel(npc, item, taste.Value);
				}
			}
		}
	}

	public IEnumerable<GiftTasteModel> GetGiftTastes(NPC npc)
	{
		if (!this.IsSocialVillager(npc))
		{
			return Array.Empty<GiftTasteModel>();
		}
		return from entry in this.ItemRepository.GetAll("(O)", includeVariants: false)
			where !(entry.Item is Ring)
			let item = entry.CreateItem()
			let taste = this.GetGiftTaste(npc, item)
			where taste.HasValue
			select new GiftTasteModel(npc, item, taste.Value);
	}

	public IEnumerable<KeyValuePair<NPC, GiftTaste?>> GetMovieTastes()
	{
		foreach (NPC npc in this.GetAllCharacters())
		{
			if (this.IsSocialVillager(npc))
			{
				string rawTaste = MovieTheater.GetResponseForMovie(npc);
				switch (rawTaste)
				{
				case "love":
				case "like":
				case "dislike":
					yield return new KeyValuePair<NPC, GiftTaste?>(npc, Enum.Parse<GiftTaste>(rawTaste, ignoreCase: true));
					break;
				case "reject":
					yield return new KeyValuePair<NPC, GiftTaste?>(npc, null);
					break;
				}
			}
		}
	}

	public IEnumerable<FishPondPopulationGateData> GetFishPondPopulationGates(FishPondData data)
	{
		return this.DataParser.GetFishPondPopulationGates(data);
	}

	public IEnumerable<FishPondDropData> GetFishPondDrops(FishPondData data)
	{
		return this.DataParser.GetFishPondDrops(data);
	}

	public FishSpawnData GetFishSpawnRules(ParsedItemData fish)
	{
		return this.DataParser.GetFishSpawnRules(fish, this.Metadata);
	}

	public IEnumerable<FishSpawnData> GetFishSpawnRules(GameLocation location, Vector2 tile, string fishAreaId)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return this.DataParser.GetFishSpawnRules(location, tile, fishAreaId, this.Metadata);
	}

	public FriendshipModel GetFriendshipForVillager(Farmer player, NPC npc, Friendship friendship)
	{
		return this.DataParser.GetFriendshipForVillager(player, npc, friendship, this.Metadata);
	}

	public FriendshipModel GetFriendshipForPet(Farmer player, Pet pet)
	{
		return this.DataParser.GetFriendshipForPet(player, pet);
	}

	public FriendshipModel GetFriendshipForAnimal(Farmer player, FarmAnimal animal)
	{
		return this.DataParser.GetFriendshipForAnimal(player, animal, this.Metadata);
	}

	public string GetLocationDisplayName(FishSpawnLocationData fishSpawnData)
	{
		return this.DataParser.GetLocationDisplayName(fishSpawnData);
	}

	public string GetLocationDisplayName(GameLocation location, string? fishAreaId)
	{
		return this.DataParser.GetLocationDisplayName(location.Name, location.GetData(), fishAreaId);
	}

	public string GetLocationDisplayName(string id, LocationData? data)
	{
		return this.DataParser.GetLocationDisplayName(id, data);
	}

	public IEnumerable<MonsterData> GetMonsterData()
	{
		return this.DataParser.GetMonsters();
	}

	public IEnumerable<BundleModel> GetBundleData()
	{
		return this.DataParser.GetBundles(this.Monitor);
	}

	public IEnumerable<RecipeModel> GetRecipes()
	{
		return this.Recipes.Value;
	}

	public IEnumerable<RecipeModel> GetRecipesForIngredient(Item item)
	{
		if (item.TypeDefinitionId != "(O)")
		{
			return Array.Empty<RecipeModel>();
		}
		List<RecipeModel> recipes = (from recipe in this.GetRecipes()
			where recipe.Ingredients.Any((RecipeIngredientModel p) => p.Matches(item)) && !recipe.ExceptIngredients.Any((RecipeIngredientModel p) => p.Matches(item))
			select recipe).ToList();
		recipes.RemoveAll(delegate(RecipeModel recipe)
		{
			if (recipe.Type != RecipeType.MachineInput)
			{
				return false;
			}
			int result;
			return int.TryParse(recipe.Ingredients.FirstOrDefault()?.InputId, out result) && result < 0 && recipes.Any((RecipeModel other) => other.Ingredients.FirstOrDefault()?.InputId == item.QualifiedItemId && other.DisplayType == recipe.DisplayType);
		});
		return recipes;
	}

	public IEnumerable<RecipeModel> GetRecipesForOutput(Item item)
	{
		return from recipe in this.GetRecipes()
			where this.AreEquivalent(item, recipe.TryCreateItem(item), flavorSpecific: false)
			select recipe;
	}

	public IEnumerable<RecipeModel> GetRecipesForMachine(Object? machine)
	{
		if (machine == null)
		{
			return Array.Empty<RecipeModel>();
		}
		return (from recipe in this.GetRecipes()
			where recipe.IsForMachine((Item)(object)machine)
			select recipe).ToList();
	}

	public IEnumerable<RecipeModel> GetRecipesForBuilding(Building? building)
	{
		if (building == null)
		{
			return Array.Empty<RecipeModel>();
		}
		return (from recipe in this.GetRecipes()
			where recipe.IsForMachine(building)
			select recipe).ToList();
	}

	public IEnumerable<QuestModel> GetQuestsWhichNeedItem(Object item)
	{
		IEnumerable<QuestModel> quests = ((IEnumerable<Quest>)Game1.player.questLog).Select((Quest quest2) => new QuestModel(quest2)).Concat(((IEnumerable<SpecialOrder>)Game1.player.team.specialOrders).Select((SpecialOrder order) => new QuestModel(order)));
		foreach (QuestModel quest in quests)
		{
			if (!string.IsNullOrWhiteSpace(quest.DisplayText) && quest.NeedsItem(item))
			{
				yield return quest;
			}
		}
	}

	public IEnumerable<Object> GetObjectsByCategory(int category)
	{
		foreach (SearchableItem entry in this.Objects.Value.Where((SearchableItem obj) => obj.Item.Category == category))
		{
			yield return (Object)entry.CreateItem();
		}
	}

	public bool CanHaveQuality(Item item)
	{
		if (new string[9] { "Artifact", "Trash", "Crafting", "Seed", "Decor", "Resource", "Fertilizer", "Bait", "Fishing Tackle" }.Contains(item.getCategoryName()))
		{
			return false;
		}
		string[] source = new string[3] { "Crafting", "asdf", "Quest" };
		Item obj = ((item is Object) ? item : null);
		if (source.Contains((obj != null) ? ((Object)obj).Type : null))
		{
			return false;
		}
		return true;
	}

	public IModInfo? TryGetModFromStringId(string? id)
	{
		return CommonHelper.TryGetModFromStringId(this.ModRegistry, id);
	}

	public Vector2 GetScreenCoordinatesFromCursor()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		return new Vector2((float)Game1.getMouseX(), (float)Game1.getMouseY());
	}

	public Vector2 GetScreenCoordinatesFromAbsolute(Vector2 coordinates)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		return coordinates - new Vector2((float)((Rectangle)(ref Game1.uiViewport)).X, (float)((Rectangle)(ref Game1.uiViewport)).Y);
	}

	public Rectangle GetScreenCoordinatesFromTile(Vector2 tile)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		Vector2 position = this.GetScreenCoordinatesFromAbsolute(tile * new Vector2(64f));
		return new Rectangle((int)position.X, (int)position.Y, 64, 64);
	}

	public bool CouldSpriteOccludeTile(Vector2 spriteTile, Vector2 occludeTile, Vector2? spriteSize = null)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		Vector2 valueOrDefault = spriteSize.GetValueOrDefault();
		if (!spriteSize.HasValue)
		{
			valueOrDefault = Constant.MaxTargetSpriteSize;
			spriteSize = valueOrDefault;
		}
		if (spriteTile.Y >= occludeTile.Y && Math.Abs(spriteTile.X - occludeTile.X) <= spriteSize.Value.X)
		{
			return Math.Abs(spriteTile.Y - occludeTile.Y) <= spriteSize.Value.Y;
		}
		return false;
	}

	public Vector2 GetSpriteSheetCoordinates(Vector2 worldPosition, Rectangle worldRectangle, Rectangle spriteRectangle, SpriteEffects spriteEffects = (SpriteEffects)0)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		float x = (worldPosition.X - (float)worldRectangle.X) / 4f;
		float y = (worldPosition.Y - (float)worldRectangle.Y) / 4f;
		if (((Enum)spriteEffects).HasFlag((Enum)(object)(SpriteEffects)1))
		{
			x = (float)spriteRectangle.Width - x;
		}
		if (((Enum)spriteEffects).HasFlag((Enum)(object)(SpriteEffects)2))
		{
			y = (float)spriteRectangle.Height - y;
		}
		x += (float)spriteRectangle.X;
		y += (float)spriteRectangle.Y;
		return new Vector2(x, y);
	}

	public TPixel GetSpriteSheetPixel<TPixel>(Texture2D spriteSheet, Vector2 position) where TPixel : struct
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		int x = (int)position.X;
		int y = (int)position.Y;
		int spriteIndex = y * spriteSheet.Width + x;
		TPixel[] pixels = new TPixel[spriteSheet.Width * spriteSheet.Height];
		spriteSheet.GetData<TPixel>(pixels);
		return pixels[spriteIndex];
	}

	public SpriteInfo? GetSprite(Item? item, bool onlyCustom = false)
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		Object obj = (Object)(object)((item is Object) ? item : null);
		if (obj != null && this.CustomFarmingRedux.IsLoaded)
		{
			SpriteInfo data = this.CustomFarmingRedux.GetSprite(obj);
			if (data != null)
			{
				return data;
			}
		}
		if (!onlyCustom && item != null)
		{
			ParsedItemData data2 = ItemRegistry.GetDataOrErrorItem(item.QualifiedItemId);
			return new SpriteInfo(data2.GetTexture(), data2.GetSourceRect(0, (int?)null));
		}
		return null;
	}

	public Vector2 DrawHoverBox(SpriteBatch spriteBatch, string label, Vector2 position, float wrapWidth)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return CommonHelper.DrawHoverBox(spriteBatch, label, in position, wrapWidth);
	}

	public void ShowErrorMessage(string message)
	{
		CommonHelper.ShowErrorMessage(message);
	}

	public IClickableMenu? GetGameMenuPage(IClickableMenu menu)
	{
		GameMenu gameMenu = (GameMenu)(object)((menu is GameMenu) ? menu : null);
		if (gameMenu != null)
		{
			return gameMenu.GetCurrentPage();
		}
		return this.BetterGameMenu.GetCurrentPage(menu);
	}

	private bool AreEquivalent(Item? a, Item? b, bool flavorSpecific)
	{
		if (a != null && b != null && a.QualifiedItemId == b.QualifiedItemId && ((NetFieldBase<bool, NetBool>)(object)((Chest)(((a is Chest) ? a : null)?)).fridge).Value == ((NetFieldBase<bool, NetBool>)(object)((Chest)(((b is Chest) ? b : null)?)).fridge).Value)
		{
			if (flavorSpecific)
			{
				Item? obj = ((a is Object) ? a : null);
				string obj2 = ((obj != null) ? ((Object)obj).GetPreservedItemId() : null);
				Item? obj3 = ((b is Object) ? b : null);
				return obj2 == ((obj3 != null) ? ((Object)obj3).GetPreservedItemId() : null);
			}
			return true;
		}
		return false;
	}

	private RecipeModel[] GetAllRecipes(IMonitor monitor)
	{
		List<RecipeModel> recipes = this.DataParser.GetRecipes(this.Metadata, monitor, this.ExtraMachineConfig).ToList();
		if (this.ProducerFrameworkMod.IsLoaded)
		{
			List<RecipeModel> customRecipes = new List<RecipeModel>();
			foreach (ProducerFrameworkRecipe recipe in this.ProducerFrameworkMod.GetRecipes())
			{
				if (recipe.InputId != null)
				{
					recipes.RemoveAll((RecipeModel other) => other.Type == RecipeType.MachineInput && other.MachineId == recipe.MachineId && other.Ingredients.Length != 0 && other.Ingredients[0].InputId == recipe.InputId);
				}
				Object machine = ItemRegistry.Create<Object>(recipe.MachineId, 1, 0, true);
				if (machine == null || !ItemExtensions.HasTypeBigCraftable((IHaveItemTypeId)(object)machine))
				{
					continue;
				}
				customRecipes.Add(new RecipeModel(null, RecipeType.MachineInput, ((Item)machine).DisplayName, recipe.Ingredients.Select((ProducerFrameworkIngredient p) => new RecipeIngredientModel(RecipeType.MachineInput, p.InputId, p.Count)), 0, delegate(Item? ingredient)
				{
					Object val = ItemRegistry.Create<Object>(recipe.OutputId, 1, 0, false);
					if (ingredient != null)
					{
						_ = ingredient.ParentSheetIndex;
						((NetFieldBase<string, NetString>)(object)val.preservedParentSheetIndex).Value = ingredient.ItemId;
						((NetFieldBase<PreserveType?, NetNullableEnum<PreserveType>>)(object)val.preserve).Value = recipe.PreserveType;
					}
					return (Item?)(object)val;
				}, () => true, exceptIngredients: recipe.ExceptIngredients.Select((string id) => new RecipeIngredientModel(RecipeType.MachineInput, id, 1)), outputQualifiedItemId: recipe.OutputId, minOutput: recipe.MinOutput, maxOutput: recipe.MaxOutput, outputChance: (decimal)recipe.OutputChance, machineId: ItemRegistry.ManuallyQualifyItemId(recipe.MachineId, "(BC)", false)));
			}
			recipes.AddRange(customRecipes);
		}
		recipes.AddRange(this.GetAllTailorRecipes());
		return recipes.ToArray();
	}

	private IEnumerable<RecipeModel> GetAllTailorRecipes()
	{
		Item[] objectList = this.Objects.Value.Select((SearchableItem p) => p.Item).ToArray();
		Dictionary<string, Item[]> contextLookupCache = (from item in objectList
			from tag in item.GetContextTags()
			select new { item, tag } into @group
			group @group by @group.tag).ToDictionary(group => group.Key, group => group.Select(p => p.item).ToArray());
		HashSet<string> seenPermutation = new HashSet<string>();
		TailoringMenu tailor = new TailoringMenu();
		foreach (TailorItemRecipe recipe in tailor._tailoringRecipes)
		{
			Item[] clothItems = GetObjectsWithTags(recipe.FirstItemTags);
			Item[] spoolItems = GetObjectsWithTags(recipe.SecondItemTags);
			string[] outputItemIds = ((recipe.CraftedItemIds?.Any() ?? false) ? recipe.CraftedItemIds.ToArray() : ((recipe.CraftedItemIdFeminine == null || (int)((Character)Game1.player).Gender != 1) ? new string[1] { recipe.CraftedItemId } : new string[1] { recipe.CraftedItemIdFeminine }));
			string[] array = outputItemIds;
			foreach (string outputId in array)
			{
				if (int.TryParse(outputId, out var categoryId) && categoryId < 0)
				{
					continue;
				}
				Item[] array2 = clothItems;
				foreach (Item clothItem in array2)
				{
					Item[] array3 = spoolItems;
					foreach (Item spoolItem in array3)
					{
						if (seenPermutation.Add(clothItem.QualifiedItemId + "|" + spoolItem.QualifiedItemId))
						{
							Item output;
							try
							{
								output = this.GetTailoredItem(outputId, tailor, spoolItem);
							}
							catch (Exception value)
							{
								this.Monitor.LogOnce($"Failed to get output #{outputId} for tailoring recipe [{string.Join(", ", recipe.FirstItemTags ?? new List<string>())}] + [{string.Join(", ", recipe.SecondItemTags ?? new List<string>())}]. Technical details:\n{value}", (LogLevel)3);
								continue;
							}
							yield return new RecipeModel(null, RecipeType.TailorInput, I18n.RecipeType_Tailoring(), new _003C_003Ez__ReadOnlyArray<RecipeIngredientModel>(new RecipeIngredientModel[2]
							{
								new RecipeIngredientModel(RecipeType.TailorInput, clothItem.QualifiedItemId, 1),
								new RecipeIngredientModel(RecipeType.TailorInput, spoolItem.QualifiedItemId, 1)
							}), 0, (Item? _) => output.getOne(), () => Game1.player.HasTailoredThisItem(output), null, null, ItemRegistry.QualifyItemId(recipe.CraftedItemId));
						}
					}
				}
			}
		}
		Item[] GetObjectsWithTags(List<string>? contextTags)
		{
			if (contextTags == null)
			{
				return Array.Empty<Item>();
			}
			if (contextTags.Count == 1 && !contextTags[0].StartsWith("!"))
			{
				if (!contextLookupCache.TryGetValue(contextTags[0], out var items))
				{
					return Array.Empty<Item>();
				}
				return items;
			}
			string cacheKey = string.Join("|", contextTags.OrderBy((string p) => p));
			if (!contextLookupCache.TryGetValue(cacheKey, out var items2))
			{
				items2 = (contextLookupCache[cacheKey] = objectList.Where((Item entry) => ((IEnumerable<string>)contextTags).All((Func<string, bool>)entry.HasContextTag)).ToArray());
			}
			return items2;
		}
	}

	private Item GetTailoredItem(string craftedItemId, TailoringMenu tailor, Item spoolItem)
	{
		Item obj = ItemRegistry.Create(craftedItemId, 1, 0, false);
		Clothing clothing = (Clothing)(object)((obj is Clothing) ? obj : null);
		if (clothing != null)
		{
			tailor.DyeItems(clothing, spoolItem, 1f);
		}
		return obj;
	}

	private GiftTaste? GetGiftTaste(NPC npc, Item item)
	{
		try
		{
			return (GiftTaste)npc.getGiftTasteForThisItem(item);
		}
		catch
		{
			return null;
		}
	}
}
