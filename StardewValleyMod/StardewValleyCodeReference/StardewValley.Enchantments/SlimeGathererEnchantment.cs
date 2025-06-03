using System;
using StardewValley.Monsters;

namespace StardewValley.Enchantments;

public class SlimeGathererEnchantment : BaseWeaponEnchantment
{
	public override bool IsSecondaryEnchantment()
	{
		return true;
	}

	public override bool IsForge()
	{
		return false;
	}

	/// <inheritdoc />
	public override void OnMonsterSlay(Monster monster, GameLocation location, Farmer who, bool slainByBomb)
	{
		base.OnMonsterSlay(monster, location, who, slainByBomb);
		if (!slainByBomb && (monster is GreenSlime || monster is BigSlime))
		{
			int toDrop = 1 + Game1.random.Next((int)Math.Ceiling(Math.Sqrt(monster.MaxHealth) / 3.0));
			Game1.createMultipleItemDebris(ItemRegistry.Create("(O)766", toDrop), monster.getStandingPosition(), -1);
		}
	}

	public override int GetMaximumLevel()
	{
		return 5;
	}

	public override string GetName()
	{
		return Game1.content.LoadString("Strings\\1_6_Strings:SlimeGathererEnchantment");
	}
}
