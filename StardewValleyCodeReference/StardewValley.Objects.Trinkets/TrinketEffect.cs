using System;
using Microsoft.Xna.Framework;
using StardewValley.Companions;
using StardewValley.Monsters;
using StardewValley.TokenizableStrings;

namespace StardewValley.Objects.Trinkets;

/// <summary>Implements the special behavior for a <see cref="T:StardewValley.Objects.Trinkets.Trinket" /> based on its <see cref="F:StardewValley.GameData.TrinketData.TrinketEffectClass" /> data field.</summary>
public class TrinketEffect
{
	/// <summary>The trinket this effect is linked to.</summary>
	public Trinket Trinket;

	/// <summary>Used for very basic trinkets that only have 1 variable stat.</summary>
	public int GeneralStat;

	/// <summary>The companion linked to this effect, if applicable.</summary>
	public Companion Companion;

	/// <summary>Construct an instance.</summary>
	/// <param name="trinket">The trinket  this effect is linked to.</param>
	public TrinketEffect(Trinket trinket)
	{
		this.Trinket = trinket;
	}

	/// <summary>Handle the player performing a use action on the trinket.</summary>
	/// <param name="farmer">The player using the trinket.</param>
	public virtual void OnUse(Farmer farmer)
	{
	}

	/// <summary>Handle the trinket being equipped.</summary>
	/// <param name="farmer">The player equipping the trinket.</param>
	public virtual void Apply(Farmer farmer)
	{
		if (this.Trinket.ItemId == "ParrotEgg")
		{
			this.Companion = new FlyingCompanion(1);
			if (Game1.gameMode == 3)
			{
				farmer.AddCompanion(this.Companion);
			}
		}
	}

	/// <summary>Handle the trinket being unequipped.</summary>
	/// <param name="farmer">The player unequipping the trinket.</param>
	public virtual void Unapply(Farmer farmer)
	{
		farmer.RemoveCompanion(this.Companion);
	}

	/// <summary>Handle the player having taken a step.</summary>
	/// <param name="farmer">The player with the trinket equipped.</param>
	public virtual void OnFootstep(Farmer farmer)
	{
	}

	/// <summary>Handle the player having received damage.</summary>
	/// <param name="farmer">The player with the trinket equipped.</param>
	/// <param name="damageAmount">The amount of damage that was taken.</param>
	public virtual void OnReceiveDamage(Farmer farmer, int damageAmount)
	{
	}

	/// <summary>Handle the player dealing damage to a monster.</summary>
	/// <param name="farmer">The player with the trinket equipped.</param>
	/// <param name="monster">The monster which was damaged.</param>
	/// <param name="damageAmount">The amount of damage that was dealt.</param>
	/// <param name="isBomb">Whether the damage is from a bomb.</param>
	/// <param name="isCriticalHit">Whether the attack which caused the damage was a critical hit.</param>
	public virtual void OnDamageMonster(Farmer farmer, Monster monster, int damageAmount, bool isBomb, bool isCriticalHit)
	{
		if (this.Trinket.ItemId == "ParrotEgg" && monster != null && monster.Health <= 0)
		{
			double chance = (double)(this.GeneralStat + 1) * 0.1;
			while (Game1.random.NextDouble() <= chance)
			{
				monster.objectsToDrop.Add("GoldCoin");
			}
		}
	}

	/// <summary>Re-roll the trinket stats if applicable.</summary>
	/// <param name="trinket">The trinket whose stats to re-roll.</param>
	/// <remarks>Returns whether the trinket stats were re-rolled (regardless of whether they changed).</remarks>
	public virtual bool GenerateRandomStats(Trinket trinket)
	{
		Random r = Utility.CreateRandom(trinket.generationSeed.Value);
		string itemId = trinket.ItemId;
		if (!(itemId == "IridiumSpur"))
		{
			if (itemId == "ParrotEgg")
			{
				int maxLevel = Math.Min(4, (int)(1 + Game1.player.totalMoneyEarned / 750000));
				int wasLevel = this.GeneralStat;
				this.GeneralStat = r.Next(0, maxLevel);
				trinket.descriptionSubstitutionTemplates.Clear();
				trinket.descriptionSubstitutionTemplates.Add((this.GeneralStat + 1).ToString());
				trinket.descriptionSubstitutionTemplates.Add(TokenStringBuilder.LocalizedText("Strings\\1_6_Strings:ParrotEgg_Chance_" + this.GeneralStat));
				if (maxLevel <= 1)
				{
					return this.GeneralStat != wasLevel;
				}
				return true;
			}
			return false;
		}
		this.GeneralStat = r.Next(5, 11);
		trinket.descriptionSubstitutionTemplates.Clear();
		trinket.descriptionSubstitutionTemplates.Add(this.GeneralStat.ToString());
		return true;
	}

	/// <summary>Update the trinket effects.</summary>
	/// <param name="farmer">The player with the trinket equipped.</param>
	/// <param name="time">The elapsed game time.</param>
	/// <param name="location">The player's current location.</param>
	public virtual void Update(Farmer farmer, GameTime time, GameLocation location)
	{
	}
}
