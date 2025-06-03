using Pathoschild.Stardew.Common;

namespace Pathoschild.Stardew.LookupAnything.Framework.Fields.Models;

internal record RecipeItemEntry(SpriteInfo? Sprite, string DisplayText, int? Quality, bool IsGoldPrice, bool IsValid = true, object? Entity = null);
