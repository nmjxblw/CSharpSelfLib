using System.Collections.Generic;
using System.Linq;
using Pathoschild.Stardew.LookupAnything.Framework.Data;
using Pathoschild.Stardew.LookupAnything.Framework.Models.FishData;
using StardewValley;

namespace Pathoschild.Stardew.LookupAnything.Framework;

internal record Metadata(ConstantData Constants, ItemData[] Items, CharacterData[] Characters, ShopData[] Shops, Dictionary<string, FishSpawnData> CustomFishSpawnRules, HashSet<string> IgnoreFishingLocations, PuzzleSolutionsData PuzzleSolutions)
{
	public bool LooksValid()
	{
		return new object[7] { this.Constants, this.Items, this.Characters, this.Shops, this.CustomFishSpawnRules, this.IgnoreFishingLocations, this.PuzzleSolutions }.All((object p) => p != null);
	}

	public ItemData? GetObject(Item item, ObjectContext context)
	{
		return this.Items.FirstOrDefault((ItemData p) => p.QualifiedId.Contains(item.QualifiedItemId) && p.Context.HasFlag(context));
	}

	public CharacterData? GetCharacter(NPC character, SubjectType type)
	{
		object obj = this.Characters?.FirstOrDefault((CharacterData p) => p.ID == $"{type}::{((Character)character).Name}");
		if (obj == null)
		{
			CharacterData[] characters = this.Characters;
			if (characters == null)
			{
				return null;
			}
			obj = characters.FirstOrDefault((CharacterData p) => p.ID == type.ToString());
		}
		return (CharacterData?)obj;
	}
}
