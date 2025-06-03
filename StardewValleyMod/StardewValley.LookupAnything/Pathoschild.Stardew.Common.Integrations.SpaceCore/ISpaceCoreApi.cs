using StardewValley;

namespace Pathoschild.Stardew.Common.Integrations.SpaceCore;

public interface ISpaceCoreApi
{
	string[] GetCustomSkills();

	int GetExperienceForCustomSkill(Farmer farmer, string skill);

	string GetDisplayNameOfCustomSkill(string skill);
}
