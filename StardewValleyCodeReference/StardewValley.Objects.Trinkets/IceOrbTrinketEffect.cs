using System;
using System.Globalization;
using Microsoft.Xna.Framework;
using StardewValley.Monsters;
using StardewValley.Projectiles;
using StardewValley.TokenizableStrings;

namespace StardewValley.Objects.Trinkets;

/// <summary>Implements the special behavior for a <see cref="T:StardewValley.Objects.Trinkets.Trinket" /> which shoots a freezing projectile at enemies.</summary>
public class IceOrbTrinketEffect : TrinketEffect
{
	/// <summary>The pixel range at which monsters can be targeted.</summary>
	public const int Range = 600;

	/// <summary>The number of milliseconds until the trinket next shoots a projectile.</summary>
	public float ProjectileTimer;

	/// <summary>The number of milliseconds between each projectile.</summary>
	public float ProjectileDelay = 4000f;

	/// <summary>The number of milliseconds for which a monster is frozen.</summary>
	public int FreezeTime = 4000;

	/// <inheritdoc />
	public IceOrbTrinketEffect(Trinket trinket)
		: base(trinket)
	{
	}

	/// <inheritdoc />
	public override void Apply(Farmer farmer)
	{
		this.ProjectileTimer = 0f;
		base.Apply(farmer);
	}

	/// <inheritdoc />
	public override bool GenerateRandomStats(Trinket trinket)
	{
		Random r = Utility.CreateRandom(trinket.generationSeed.Value);
		this.ProjectileDelay = r.Next(3000, 5001);
		this.FreezeTime = r.Next(2000, 4001);
		if (r.NextDouble() < 0.05)
		{
			trinket.displayNameOverrideTemplate.Value = TokenStringBuilder.LocalizedText("Strings\\1_6_Strings:PerfectIceRod");
			this.ProjectileDelay = 3000f;
			this.FreezeTime = 4000;
		}
		trinket.descriptionSubstitutionTemplates.Clear();
		trinket.descriptionSubstitutionTemplates.Add(Math.Round(this.ProjectileDelay / 1000f, 1).ToString(CultureInfo.InvariantCulture));
		trinket.descriptionSubstitutionTemplates.Add(Math.Round((float)this.FreezeTime / 1000f, 1).ToString(CultureInfo.InvariantCulture));
		return true;
	}

	/// <inheritdoc />
	public override void Update(Farmer farmer, GameTime time, GameLocation location)
	{
		if (!Game1.shouldTimePass())
		{
			return;
		}
		this.ProjectileTimer += (float)time.ElapsedGameTime.TotalMilliseconds;
		if (this.ProjectileTimer >= this.ProjectileDelay)
		{
			Monster monster = Utility.findClosestMonsterWithinRange(location, farmer.getStandingPosition(), 600);
			if (monster != null)
			{
				Vector2 motion = Utility.getVelocityTowardPoint(farmer.getStandingPosition(), monster.getStandingPosition(), 5f);
				DebuffingProjectile p = new DebuffingProjectile("frozen", 17, 0, 0, 0f, motion.X, motion.Y, farmer.getStandingPosition() - new Vector2(32f, 48f), location, farmer, hitsMonsters: true, playDefaultSoundOnFire: false);
				p.wavyMotion.Value = false;
				p.piercesLeft.Value = 99999;
				p.maxTravelDistance.Value = 3000;
				p.IgnoreLocationCollision = true;
				p.ignoreObjectCollisions.Value = true;
				p.maxVelocity.Value = 12f;
				p.projectileID.Value = 15;
				p.alpha.Value = 0.001f;
				p.alphaChange.Value = 0.05f;
				p.light.Value = true;
				p.debuffIntensity.Value = this.FreezeTime;
				p.boundingBoxWidth.Value = 32;
				location.projectiles.Add(p);
				location.playSound("fireball");
			}
			this.ProjectileTimer = 0f;
		}
		base.Update(farmer, time, location);
	}
}
