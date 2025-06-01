using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Xna.Framework;
using StardewValley.Extensions;
using StardewValley.Monsters;
using StardewValley.Projectiles;
using StardewValley.TokenizableStrings;

namespace StardewValley.Objects.Trinkets;

/// <summary>Implements the special behavior for a <see cref="T:StardewValley.Objects.Trinkets.Trinket" /> which shoots a damaging projectile at enemies.</summary>
public class MagicQuiverTrinketEffect : TrinketEffect
{
	/// <summary>The backing field for <see cref="M:StardewValley.Objects.Trinkets.MagicQuiverTrinketEffect.GetIgnoredLocations" />.</summary>
	public static HashSet<string> CachedIgnoreLocations;

	/// <summary>The backing field for <see cref="M:StardewValley.Objects.Trinkets.MagicQuiverTrinketEffect.GetIgnoredMonsterNames" />.</summary>
	public static HashSet<string> CachedIgnoreMonsters;

	/// <summary>The pixel range at which monsters can be targeted.</summary>
	public const int Range = 500;

	/// <summary>The number of milliseconds until the trinket next shoots a projectile.</summary>
	public float ProjectileTimer;

	/// <summary>The number of milliseconds between each projectile.</summary>
	public float ProjectileDelay = 1000f;

	/// <summary>The minimum damage that can be dealt to monsters.</summary>
	public int MinDamage = 10;

	/// <summary>The minimum damage that can be dealt to monsters.</summary>
	public int MaxDamage = 10;

	/// <inheritdoc />
	public MagicQuiverTrinketEffect(Trinket trinket)
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
		if (r.NextBool(0.04))
		{
			trinket.displayNameOverrideTemplate.Value = TokenStringBuilder.LocalizedText("Strings\\1_6_Strings:PerfectMagicQuiver");
			this.MinDamage = 30;
			this.MaxDamage = 35;
			this.ProjectileDelay = 900f;
		}
		else if (r.NextBool(0.1))
		{
			if (r.NextBool(0.5))
			{
				trinket.displayNameOverrideTemplate.Value = TokenStringBuilder.LocalizedText("Strings\\1_6_Strings:RapidMagicQuiver");
				this.MinDamage = r.Next(10, 15);
				this.MinDamage -= 2;
				this.MaxDamage = this.MinDamage + 5;
				this.ProjectileDelay = 600 + r.Next(11) * 10;
			}
			else
			{
				trinket.displayNameOverrideTemplate.Value = TokenStringBuilder.LocalizedText("Strings\\1_6_Strings:HeavyMagicQuiver");
				this.MinDamage = r.Next(25, 41);
				this.MinDamage -= 2;
				this.MaxDamage = this.MinDamage + 5;
				this.ProjectileDelay = 1500 + r.Next(6) * 100;
			}
		}
		else
		{
			this.MinDamage = r.Next(15, 31);
			this.MinDamage -= 2;
			this.MaxDamage = this.MinDamage + 5;
			this.ProjectileDelay = 1100 + r.Next(11) * 100;
		}
		trinket.descriptionSubstitutionTemplates.Clear();
		trinket.descriptionSubstitutionTemplates.Add(Math.Round((double)this.ProjectileDelay / 1000.0, 2).ToString(CultureInfo.InvariantCulture));
		trinket.descriptionSubstitutionTemplates.Add(this.MinDamage.ToString());
		trinket.descriptionSubstitutionTemplates.Add(this.MaxDamage.ToString());
		return true;
	}

	/// <inheritdoc />
	public override void Update(Farmer farmer, GameTime time, GameLocation location)
	{
		base.Update(farmer, time, location);
		if (!Game1.shouldTimePass())
		{
			return;
		}
		this.ProjectileTimer += (float)time.ElapsedGameTime.TotalMilliseconds;
		if (!(this.ProjectileTimer >= this.ProjectileDelay))
		{
			return;
		}
		this.ProjectileTimer = 0f;
		HashSet<string> ignoreLocations = this.GetIgnoredLocations();
		if (!ignoreLocations.Contains(location.NameOrUniqueName) && !ignoreLocations.Contains(location.Name))
		{
			HashSet<string> ignoreMonsterNames = this.GetIgnoredMonsterNames();
			Monster monster = Utility.findClosestMonsterWithinRange(location, farmer.getStandingPosition(), 500, ignoreUntargetables: true, (Monster m) => !ignoreMonsterNames.Contains(m.Name));
			if (monster != null)
			{
				Vector2 motion = Utility.getVelocityTowardPoint(farmer.getStandingPosition(), monster.getStandingPosition(), 2f);
				float projectileRotation = (float)Math.Atan2(motion.Y, motion.X) + (float)Math.PI / 2f;
				BasicProjectile p = new BasicProjectile(Game1.random.Next(this.MinDamage, this.MaxDamage + 1), 16, 0, 0, 0f, motion.X, motion.Y, farmer.getStandingPosition() - new Vector2(32f, 48f), null, null, null, explode: false, damagesMonsters: true, location, farmer);
				p.IgnoreLocationCollision = true;
				p.ignoreObjectCollisions.Value = true;
				p.acceleration.Value = motion;
				p.maxVelocity.Value = 24f;
				p.projectileID.Value = 14;
				p.startingRotation.Value = projectileRotation;
				p.alpha.Value = 0.001f;
				p.alphaChange.Value = 0.05f;
				p.light.Value = true;
				p.collisionSound.Value = "magic_arrow_hit";
				location.projectiles.Add(p);
				location.playSound("magic_arrow");
			}
		}
	}

	/// <summary>Get the locations which magic quivers should ignore.</summary>
	public HashSet<string> GetIgnoredLocations()
	{
		if (MagicQuiverTrinketEffect.CachedIgnoreLocations == null)
		{
			MagicQuiverTrinketEffect.CachedIgnoreLocations = new HashSet<string>(ArgUtility.SplitQuoteAware(base.Trinket.GetTrinketData()?.CustomFields?.GetValueOrDefault("IgnoreLocations"), '/'), StringComparer.OrdinalIgnoreCase);
		}
		return MagicQuiverTrinketEffect.CachedIgnoreLocations;
	}

	/// <summary>Get the monsters which magic quivers should ignore.</summary>
	public HashSet<string> GetIgnoredMonsterNames()
	{
		if (MagicQuiverTrinketEffect.CachedIgnoreMonsters == null)
		{
			MagicQuiverTrinketEffect.CachedIgnoreMonsters = new HashSet<string>(ArgUtility.SplitQuoteAware(base.Trinket.GetTrinketData()?.CustomFields?.GetValueOrDefault("IgnoreMonsters"), '/'), StringComparer.OrdinalIgnoreCase);
		}
		return MagicQuiverTrinketEffect.CachedIgnoreMonsters;
	}
}
