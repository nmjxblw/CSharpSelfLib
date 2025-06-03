using System.Linq;
using StardewValley;

namespace Pathoschild.Stardew.Common.Integrations.ProducerFrameworkMod;

internal class ProducerFrameworkRecipe
{
	public string? InputId { get; }

	public string MachineId { get; }

	public ProducerFrameworkIngredient[] Ingredients { get; }

	public string?[] ExceptIngredients { get; }

	public string OutputId { get; }

	public int MinOutput { get; }

	public int MaxOutput { get; }

	public double OutputChance { get; }

	public PreserveType? PreserveType { get; }

	public ProducerFrameworkRecipe(string? inputId, string machineId, ProducerFrameworkIngredient[] ingredients, string?[] exceptIngredients, string outputId, int minOutput, int maxOutput, double outputChance, PreserveType? preserveType)
	{
		this.InputId = inputId;
		this.MachineId = machineId;
		this.Ingredients = ingredients;
		this.ExceptIngredients = exceptIngredients;
		this.OutputId = outputId;
		this.MinOutput = minOutput;
		this.MaxOutput = maxOutput;
		this.OutputChance = outputChance;
		this.PreserveType = preserveType;
	}

	public bool HasContextTags()
	{
		if (this.InputId != null && !this.Ingredients.Any((ProducerFrameworkIngredient p) => p.InputId == null))
		{
			return this.ExceptIngredients.Any((string p) => p == null);
		}
		return true;
	}
}
