using System;
using Microsoft.Xna.Framework;
using StardewValley.Monsters;

namespace StardewValley.Enchantments;

/// <summary>Siphons health from monsters sometimes.</summary>
public class VampiricEnchantment : BaseWeaponEnchantment
{
	/// <inheritdoc />
	public override void OnMonsterSlay(Monster monster, GameLocation location, Farmer who, bool slainByBomb)
	{
		base.OnMonsterSlay(monster, location, who, slainByBomb);
		if (!slainByBomb && Game1.random.NextDouble() < 0.09000000357627869)
		{
			int amount = Math.Max(1, (int)((float)(monster.MaxHealth + Game1.random.Next(-monster.MaxHealth / 10, monster.MaxHealth / 15 + 1)) * 0.1f));
			who.health = Math.Min(who.maxHealth, who.health + amount);
			location.debris.Add(new Debris(amount, who.getStandingPosition(), Color.Lime, 1f, who));
			Game1.playSound("healSound");
		}
	}

	public override string GetName()
	{
		return "Vampiric";
	}
}
