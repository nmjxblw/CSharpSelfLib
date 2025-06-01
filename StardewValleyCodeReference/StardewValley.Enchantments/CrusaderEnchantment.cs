using StardewValley.Monsters;

namespace StardewValley.Enchantments;

public class CrusaderEnchantment : BaseWeaponEnchantment
{
	/// <inheritdoc />
	public override void OnCalculateDamage(Monster monster, GameLocation location, Farmer who, bool fromBomb, ref int amount)
	{
		base.OnCalculateDamage(monster, location, who, fromBomb, ref amount);
		if (!fromBomb && (monster is Ghost || monster is Skeleton || monster is Mummy || monster is ShadowBrute || monster is ShadowShaman || monster is ShadowGirl || monster is ShadowGuy || monster is Shooter))
		{
			amount = (int)((float)amount * 1.5f);
		}
	}

	public override string GetName()
	{
		return "Crusader";
	}
}
