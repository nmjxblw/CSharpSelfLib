using System.Collections.Generic;

namespace AdvancedMeleeFramework;

public class AdvancedMeleeWeapon
{
	public string id = "none";

	public int type;

	public List<AdvancedEnchantmentData> enchantments = new List<AdvancedEnchantmentData>();

	public int skillLevel;

	public int cooldown = 1500;

	public List<MeleeActionFrame> frames = new List<MeleeActionFrame>();

	public Dictionary<string, string> config = new Dictionary<string, string>();
}
