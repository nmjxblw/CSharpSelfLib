using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Netcode;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.LookupAnything.Framework.Fields.Models;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.GameData.Buildings;
using StardewValley.Network;
using StardewValley.TokenizableStrings;

namespace Pathoschild.Stardew.LookupAnything.Framework.Models;

internal class RecipeModel
{
	private readonly Func<Item?, Item?>? Item;

	public string? Key { get; }

	public string? MachineId { get; }

	public RecipeType Type { get; }

	public string DisplayType { get; }

	public RecipeIngredientModel[] Ingredients { get; }

	public int GoldPrice { get; }

	public string? OutputQualifiedItemId { get; }

	public int MinOutput { get; }

	public int MaxOutput { get; }

	public decimal OutputChance { get; set; }

	public RecipeIngredientModel[] ExceptIngredients { get; }

	public Func<bool> IsKnown { get; }

	public RecipeItemEntry? SpecialOutput { get; }

	public int? Quality { get; }

	public string[] Conditions { get; }

	public RecipeModel(string? key, RecipeType type, string displayType, IEnumerable<RecipeIngredientModel> ingredients, int goldPrice, Func<Item?, Item?>? item, Func<bool> isKnown, string? machineId, IEnumerable<RecipeIngredientModel>? exceptIngredients = null, string? outputQualifiedItemId = null, int? minOutput = null, int? maxOutput = null, decimal? outputChance = null, int? quality = null, string[]? conditions = null)
	{
		if (!minOutput.HasValue && !maxOutput.HasValue)
		{
			minOutput = 1;
			maxOutput = 1;
		}
		else if (!minOutput.HasValue)
		{
			minOutput = maxOutput;
		}
		else if (!maxOutput.HasValue)
		{
			maxOutput = minOutput;
		}
		this.Key = key;
		this.Type = type;
		this.DisplayType = displayType;
		this.Ingredients = ingredients.ToArray();
		this.GoldPrice = goldPrice;
		this.MachineId = machineId;
		this.ExceptIngredients = exceptIngredients?.ToArray() ?? Array.Empty<RecipeIngredientModel>();
		this.Item = item;
		this.IsKnown = isKnown;
		this.OutputQualifiedItemId = outputQualifiedItemId;
		this.MinOutput = minOutput.Value;
		this.MaxOutput = maxOutput.Value;
		decimal outputChance2;
		if (outputChance.HasValue)
		{
			decimal valueOrDefault = outputChance.GetValueOrDefault();
			if (valueOrDefault > 0m && valueOrDefault < 100m)
			{
				outputChance2 = outputChance.Value;
				goto IL_0105;
			}
		}
		outputChance2 = 100m;
		goto IL_0105;
		IL_0105:
		this.OutputChance = outputChance2;
		this.Quality = quality;
		this.Conditions = conditions ?? Array.Empty<string>();
	}

	public RecipeModel(CraftingRecipe recipe, string? outputQualifiedItemId, RecipeIngredientModel[]? ingredients = null)
		: this(recipe.name, (!recipe.isCookingRecipe) ? RecipeType.Crafting : RecipeType.Cooking, recipe.isCookingRecipe ? I18n.RecipeType_Cooking() : I18n.RecipeType_Crafting(), ingredients ?? RecipeModel.ParseIngredients(recipe), 0, (Item? _) => recipe.createItem(), () => recipe.name != null && Game1.player.knowsRecipe(recipe.name), null, null, minOutput: recipe.numberProducedPerCraft, outputQualifiedItemId: RecipeModel.QualifyRecipeOutputId(recipe, outputQualifiedItemId) ?? outputQualifiedItemId)
	{
	}

	public RecipeModel(Building building, RecipeIngredientModel[] ingredients, int goldPrice)
		: this(TokenParser.ParseText(building.GetData()?.Name, (Random)null, (TokenParserDelegate)null, (Farmer)null) ?? ((NetFieldBase<string, NetString>)(object)building.buildingType).Value, RecipeType.BuildingBlueprint, I18n.Building_Construction(), ingredients, goldPrice, (Item? _) => (Item?)null, () => true, ((NetFieldBase<string, NetString>)(object)building.buildingType).Value)
	{
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		this.SpecialOutput = new RecipeItemEntry(new SpriteInfo(building.texture.Value, (Rectangle)(((_003F?)building.getSourceRectForMenu()) ?? building.getSourceRect())), TokenParser.ParseText(building.GetData()?.Name, (Random)null, (TokenParserDelegate)null, (Farmer)null) ?? ((NetFieldBase<string, NetString>)(object)building.buildingType).Value, null, IsGoldPrice: false, IsValid: true, building);
	}

	public RecipeModel(RecipeModel other)
		: this(other.Key, other.Type, other.DisplayType, other.Ingredients, other.GoldPrice, other.Item, other.IsKnown, exceptIngredients: other.ExceptIngredients, outputQualifiedItemId: other.OutputQualifiedItemId, minOutput: other.MinOutput, machineId: other.MachineId, maxOutput: null, outputChance: null, quality: null, conditions: other.Conditions)
	{
	}

	public static RecipeIngredientModel[] ParseIngredients(CraftingRecipe recipe)
	{
		RecipeType type = ((!recipe.isCookingRecipe) ? RecipeType.Crafting : RecipeType.Cooking);
		return recipe.recipeList.Select((KeyValuePair<string, int> p) => new RecipeIngredientModel(type, p.Key, p.Value)).ToArray();
	}

	public static RecipeIngredientModel[] ParseIngredients(BuildingData? building)
	{
		int? num = building?.BuildMaterials?.Count;
		if (!num.HasValue || num.GetValueOrDefault() <= 0)
		{
			return Array.Empty<RecipeIngredientModel>();
		}
		return building.BuildMaterials.Select((BuildingMaterial ingredient) => new RecipeIngredientModel(RecipeType.BuildingBlueprint, ingredient.ItemId, ingredient.Amount)).ToArray();
	}

	public bool IsForMachine(Building building)
	{
		if (this.MachineId != null)
		{
			return this.MachineId == ((NetFieldBase<string, NetString>)(object)building.buildingType).Value;
		}
		return false;
	}

	public bool IsForMachine(Item machine)
	{
		if (this.MachineId != null)
		{
			return this.MachineId == machine.QualifiedItemId;
		}
		return false;
	}

	public Item? TryCreateItem(Item? ingredient)
	{
		if (this.Item == null)
		{
			return null;
		}
		try
		{
			return this.Item(ingredient);
		}
		catch
		{
			return null;
		}
	}

	public int GetTimesCrafted(Farmer player)
	{
		switch (this.Type)
		{
		case RecipeType.Cooking:
		{
			string localId = ItemRegistry.GetData(this.OutputQualifiedItemId)?.ItemId;
			if (localId != null)
			{
				int timesCooked = default(int);
				if (!((NetDictionary<string, int, NetInt, SerializableDictionary<string, int>, NetStringDictionary<int, NetInt>>)(object)player.recipesCooked).TryGetValue(localId, ref timesCooked))
				{
					return 0;
				}
				return timesCooked;
			}
			return 0;
		}
		case RecipeType.Crafting:
		{
			int timesCrafted = default(int);
			if (!((NetDictionary<string, int, NetInt, SerializableDictionary<string, int>, NetStringDictionary<int, NetInt>>)(object)player.craftingRecipes).TryGetValue(this.Key, ref timesCrafted))
			{
				return 0;
			}
			return timesCrafted;
		}
		default:
			return -1;
		}
	}

	public static string? QualifyRecipeOutputId(CraftingRecipe recipe, string? itemId)
	{
		if (!recipe.bigCraftable)
		{
			return ItemRegistry.QualifyItemId(itemId);
		}
		return ItemRegistry.ManuallyQualifyItemId(itemId, "(BC)", false);
	}
}
