using System;
using System.Collections.Generic;
using StardewValley;
using StardewValley.Monsters;
using StardewValley.Tools;

namespace AdvancedMeleeFramework;

public interface IAdvancedMeleeFrameworkApi
{
	bool RegisterEnchantment(string typeId, Action<Farmer, MeleeWeapon, Monster?, Dictionary<string, string>> callback);

	bool UnRegisterEnchantment(string typeId);

	bool RegisterSpecialEffect(string name, Action<Farmer, MeleeWeapon, Dictionary<string, string>> callback);

	bool UnRegisterSpecialEffect(string name);
}
