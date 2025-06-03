using System;
using System.Linq;

namespace Pathoschild.Stardew.LookupAnything.Framework.Fields.Models;

internal class RecipeEntry
{
	private readonly Lazy<string> UniqueKeyImpl;

	public string? Name { get; }

	public string Type { get; }

	public bool IsKnown { get; }

	public RecipeItemEntry[] Inputs { get; }

	public RecipeItemEntry Output { get; }

	public string? Conditions { get; }

	public string UniqueKey => this.UniqueKeyImpl.Value;

	public bool IsValid { get; }

	public RecipeEntry(string? name, string type, bool isKnown, RecipeItemEntry[] inputs, RecipeItemEntry output, string? conditions)
	{
		this.Name = name;
		this.Type = type;
		this.IsKnown = isKnown;
		this.Inputs = inputs;
		this.Output = output;
		this.Conditions = conditions;
		this.UniqueKeyImpl = new Lazy<string>(() => RecipeEntry.GetUniqueKey(name, inputs, output));
		this.IsValid = output.IsValid && inputs.All((RecipeItemEntry input) => input.IsValid);
	}

	private static string GetUniqueKey(string? name, RecipeItemEntry[] inputs, RecipeItemEntry output)
	{
		IOrderedEnumerable<string> inputNames = from item in inputs
			select item.DisplayText into item
			orderby item
			select item;
		return string.Join(", ", inputNames.Concat(new _003C_003Ez__ReadOnlyArray<string>(new string[2] { output.DisplayText, name })));
	}
}
