using System;
using Microsoft.Xna.Framework;
using StardewValley.Companions;
using StardewValley.Extensions;
using StardewValley.Monsters;

namespace StardewValley.Objects.Trinkets;

/// <summary>Implements the special behavior for a <see cref="T:StardewValley.Objects.Trinkets.Trinket" /> which summons a fairy which heals the player.</summary>
public class FairyBoxTrinketEffect : TrinketEffect
{
	/// <summary>The number of milliseconds until the fairy next heals the player.</summary>
	public float HealTimer;

	/// <summary>The number of milliseconds between each heal.</summary>
	public float HealDelay = 4000f;

	/// <summary>The power rating applied to the heal amount.</summary>
	public float Power = 0.25f;

	/// <summary>The amount of damage taken by the player since the last heal.</summary>
	public int DamageSinceLastHeal;

	/// <inheritdoc />
	public FairyBoxTrinketEffect(Trinket trinket)
		: base(trinket)
	{
	}

	/// <inheritdoc />
	public override bool GenerateRandomStats(Trinket trinket)
	{
		Random r = Utility.CreateRandom(trinket.generationSeed.Value);
		int level = 1;
		if (r.NextBool(0.45))
		{
			level = 2;
		}
		else if (r.NextBool(0.25))
		{
			level = 3;
		}
		else if (r.NextBool(0.125))
		{
			level = 4;
		}
		else if (r.NextBool(0.0675))
		{
			level = 5;
		}
		this.HealDelay = 5000 - level * 300;
		this.Power = 0.7f + (float)level * 0.1f;
		trinket.descriptionSubstitutionTemplates.Clear();
		trinket.descriptionSubstitutionTemplates.Add(level.ToString());
		return true;
	}

	/// <inheritdoc />
	public override void OnDamageMonster(Farmer farmer, Monster monster, int damageAmount, bool isBomb, bool isCriticalHit)
	{
		this.DamageSinceLastHeal += damageAmount;
		base.OnDamageMonster(farmer, monster, damageAmount, isBomb, isCriticalHit);
	}

	/// <inheritdoc />
	public override void OnReceiveDamage(Farmer farmer, int damageAmount)
	{
		this.DamageSinceLastHeal += damageAmount;
		base.OnReceiveDamage(farmer, damageAmount);
	}

	/// <inheritdoc />
	public override void Update(Farmer farmer, GameTime time, GameLocation location)
	{
		this.HealTimer += (float)time.ElapsedGameTime.TotalMilliseconds;
		if (this.HealTimer >= this.HealDelay)
		{
			if (farmer.health < farmer.maxHealth && this.DamageSinceLastHeal >= 0)
			{
				int healAmount = (int)Math.Min(Math.Pow(this.DamageSinceLastHeal, 0.33000001311302185), (float)farmer.maxHealth / 10f);
				healAmount = (int)((float)healAmount * this.Power);
				healAmount += Game1.random.Next((int)((float)(-healAmount) * 0.25f), (int)((float)healAmount * 0.25f) + 1);
				if (healAmount > 0)
				{
					farmer.health = Math.Min(farmer.maxHealth, farmer.health + healAmount);
					location.debris.Add(new Debris(healAmount, farmer.getStandingPosition(), Color.Lime, 1f, farmer));
					Game1.playSound("fairy_heal");
					this.DamageSinceLastHeal = 0;
				}
			}
			this.HealTimer = 0f;
		}
		base.Update(farmer, time, location);
	}

	/// <inheritdoc />
	public override void Apply(Farmer farmer)
	{
		this.HealTimer = 0f;
		this.DamageSinceLastHeal = 0;
		base.Companion = new FlyingCompanion(0);
		if (Game1.gameMode == 3)
		{
			farmer.AddCompanion(base.Companion);
		}
		base.Apply(farmer);
	}

	/// <inheritdoc />
	public override void Unapply(Farmer farmer)
	{
		farmer.RemoveCompanion(base.Companion);
	}
}
