using System;
using System.Collections.Generic;
using Netcode;
using StardewValley;

namespace Pathoschild.Stardew.LookupAnything.Framework.Models;

internal class RecipeIngredientModel
{
	public RecipeType RecipeType { get; }

	public string? InputId { get; }

	public string[] InputContextTags { get; }

	public int Count { get; }

	public PreserveType? PreserveType { get; }

	public string? PreservedItemId { get; }

	public RecipeIngredientModel(RecipeType recipeType, string? inputId, int count, string[]? inputContextTags = null, PreserveType? preserveType = null, string? preservedItemId = null)
	{
		this.RecipeType = recipeType;
		this.InputId = inputId;
		this.InputContextTags = inputContextTags ?? Array.Empty<string>();
		this.Count = count;
		this.PreserveType = preserveType;
		this.PreservedItemId = preservedItemId;
	}

	public bool Matches(Item? item)
	{
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		if (item == null)
		{
			return false;
		}
		if ((this.InputId == null && this.InputContextTags.Length == 0) || (this.InputId != null && !(this.InputId == item.Category.ToString()) && !(this.InputId == item.ItemId) && !(this.InputId == item.QualifiedItemId) && (this.RecipeType != RecipeType.Crafting || !CraftingRecipe.ItemMatchesForCrafting(item, this.InputId))) || (this.InputContextTags.Length != 0 && !ItemContextTagManager.DoAllTagsMatch((IList<string>)this.InputContextTags, item.GetContextTags())))
		{
			return false;
		}
		if (this.PreservedItemId != null || this.PreserveType.HasValue)
		{
			Object obj = (Object)(object)((item is Object) ? item : null);
			if (obj == null)
			{
				return false;
			}
			if (this.PreservedItemId != null && this.PreservedItemId != ((NetFieldBase<string, NetString>)(object)obj.preservedParentSheetIndex).Value)
			{
				return false;
			}
			if (this.PreserveType.HasValue && this.PreserveType != ((NetFieldBase<PreserveType?, NetNullableEnum<PreserveType>>)(object)obj.preserve).Value)
			{
				return false;
			}
		}
		return true;
	}
}
