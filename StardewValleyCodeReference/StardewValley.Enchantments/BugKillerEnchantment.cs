using StardewValley.Monsters;

namespace StardewValley.Enchantments;

public class BugKillerEnchantment : BaseWeaponEnchantment
{
	/// <inheritdoc />
	public override void OnCalculateDamage(Monster monster, GameLocation location, Farmer who, bool fromBomb, ref int amount)
	{
		base.OnCalculateDamage(monster, location, who, fromBomb, ref amount);
		if (!fromBomb && (monster is Grub || monster is Fly || monster is Bug || monster is Leaper || monster is RockCrab))
		{
			amount = (int)((float)amount * 2f);
		}
	}

	public override string GetName()
	{
		return "Bug Killer";
	}
}
