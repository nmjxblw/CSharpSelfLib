using System.Linq;

namespace Pathoschild.Stardew.LookupAnything.Framework.Fields.Models;

internal record RecipeByTypeGroup(string Type, RecipeEntry[] Recipes, float[] ColumnWidths)
{
	public float TotalColumnWidth { get; } = ColumnWidths.Sum((float p) => p);
}
