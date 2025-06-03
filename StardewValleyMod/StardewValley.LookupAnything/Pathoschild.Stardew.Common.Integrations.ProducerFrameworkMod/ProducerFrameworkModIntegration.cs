using System;
using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI;
using StardewValley;

namespace Pathoschild.Stardew.Common.Integrations.ProducerFrameworkMod;

internal class ProducerFrameworkModIntegration : BaseIntegration<IProducerFrameworkModApi>
{
	private bool LoggedInvalidRecipeError;

	public ProducerFrameworkModIntegration(IModRegistry modRegistry, IMonitor monitor)
		: base("Producer Framework Mod", "Digus.ProducerFrameworkMod", "1.9.3", modRegistry, monitor)
	{
	}

	public IEnumerable<ProducerFrameworkRecipe> GetRecipes()
	{
		this.AssertLoaded();
		return this.ReadRecipes(base.ModApi.GetRecipes());
	}

	public IEnumerable<ProducerFrameworkRecipe> GetRecipes(Object machine)
	{
		this.AssertLoaded();
		return this.ReadRecipes(base.ModApi.GetRecipes(machine));
	}

	private IEnumerable<ProducerFrameworkRecipe> ReadRecipes(IEnumerable<IDictionary<string, object?>> raw)
	{
		return raw.Select(ReadRecipe).WhereNotNull();
	}

	private ProducerFrameworkRecipe? ReadRecipe(IDictionary<string, object?> raw)
	{
		try
		{
			string inputId = (string)raw["InputKey"];
			string machineId = (string)raw["MachineID"];
			ProducerFrameworkIngredient[] ingredients = ((List<Dictionary<string, object>>)raw["Ingredients"]).Select(ReadIngredient).ToArray();
			string[] exceptIngredients = (from p in ((List<Dictionary<string, object>>)raw["ExceptIngredients"]).Select(ReadIngredient)
				select p.InputId).ToArray();
			string outputId = (string)raw["Output"];
			int minOutput = (int)raw["MinOutput"];
			int maxOutput = (int)raw["MaxOutput"];
			PreserveType? preserveType = (PreserveType?)raw["PreserveType"];
			double outputChance = (double)raw["OutputChance"];
			PreserveType? preserveType2 = preserveType;
			return new ProducerFrameworkRecipe(inputId, machineId, ingredients, exceptIngredients, outputId, minOutput, maxOutput, outputChance, preserveType2);
		}
		catch (Exception ex)
		{
			if (!this.LoggedInvalidRecipeError)
			{
				this.LoggedInvalidRecipeError = true;
				base.Monitor.Log("Failed to load some recipes from Producer Framework Mod. Some custom machines may not appear in lookups.", (LogLevel)3);
				base.Monitor.Log(ex.ToString(), (LogLevel)0);
			}
			return null;
		}
	}

	private ProducerFrameworkIngredient ReadIngredient(IDictionary<string, object?> raw)
	{
		string id = (string)raw["ID"];
		object rawCount;
		int count = ((!raw.TryGetValue("Count", out rawCount) || rawCount == null) ? 1 : ((int)rawCount));
		return new ProducerFrameworkIngredient
		{
			InputId = id,
			Count = count
		};
	}
}
