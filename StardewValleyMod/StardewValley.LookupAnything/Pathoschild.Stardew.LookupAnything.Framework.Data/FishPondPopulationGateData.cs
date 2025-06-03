namespace Pathoschild.Stardew.LookupAnything.Framework.Data;

internal record FishPondPopulationGateData(int RequiredPopulation, FishPondPopulationGateQuestItemData[] RequiredItems)
{
	public int NewPopulation => this.RequiredPopulation + 1;
}
