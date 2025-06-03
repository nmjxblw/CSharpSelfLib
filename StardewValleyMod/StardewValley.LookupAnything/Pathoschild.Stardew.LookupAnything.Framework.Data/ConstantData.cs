using System;
using System.Collections.Generic;
using Pathoschild.Stardew.LookupAnything.Framework.Constants;

namespace Pathoschild.Stardew.LookupAnything.Framework.Data;

internal class ConstantData
{
	public int AnimalFriendshipPointsPerLevel { get; set; }

	public int AnimalFriendshipMaxPoints { get; set; }

	public int AnimalMaxHappiness { get; set; }

	public int FruitTreeQualityGrowthTime { get; set; }

	public Dictionary<string, bool> ForceSocialVillagers { get; set; } = new Dictionary<string, bool>();

	public int DatingHearts { get; set; }

	public int SpouseMaxFriendship { get; set; }

	public int SpouseFriendshipForStardrop { get; set; }

	public int PlayerMaxSkillPoints { get; set; }

	public int[] PlayerSkillPointsPerLevel { get; set; } = Array.Empty<int>();

	public int DaysInSeason { get; set; }

	public float FenceDecayRate { get; set; }

	public Dictionary<ItemQuality, int> CaskAgeSchedule { get; set; } = new Dictionary<ItemQuality, int>();

	public string[] ItemsWithIridiumQuality { get; set; } = Array.Empty<string>();

	public int MonocultureCount { get; set; }

	public int PolycultureCount { get; set; }
}
