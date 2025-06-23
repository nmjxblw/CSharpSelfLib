using Microsoft.Xna.Framework.Content;

namespace StardewValley.GameData.Buffs;

/// <summary>As part of <see cref="T:StardewValley.GameData.Buffs.BuffData" /> or <see cref="T:StardewValley.GameData.Objects.ObjectBuffData" />, the attribute values to add to the player's stats.</summary>
public class BuffAttributesData
{
	/// <summary>The buff to the player's combat skill level.</summary>
	[ContentSerializer(Optional = true)]
	public float CombatLevel;

	/// <summary>The buff to the player's farming skill level.</summary>
	[ContentSerializer(Optional = true)]
	public float FarmingLevel;

	/// <summary>The buff to the player's fishing skill level.</summary>
	[ContentSerializer(Optional = true)]
	public float FishingLevel;

	/// <summary>The buff to the player's mining skill level.</summary>
	[ContentSerializer(Optional = true)]
	public float MiningLevel;

	/// <summary>The buff to the player's luck skill level.</summary>
	[ContentSerializer(Optional = true)]
	public float LuckLevel;

	/// <summary>The buff to the player's foraging skill level.</summary>
	[ContentSerializer(Optional = true)]
	public float ForagingLevel;

	/// <summary>The buff to the player's max stamina.</summary>
	[ContentSerializer(Optional = true)]
	public float MaxStamina;

	/// <summary>The buff to the player's magnetic radius.</summary>
	[ContentSerializer(Optional = true)]
	public float MagneticRadius;

	/// <summary>The buff to the player's walk speed.</summary>
	[ContentSerializer(Optional = true)]
	public float Speed;

	/// <summary>The buff to the player's defense.</summary>
	[ContentSerializer(Optional = true)]
	public float Defense;

	/// <summary>The buff to the player's attack power.</summary>
	[ContentSerializer(Optional = true)]
	public float Attack;

	/// <summary>The combined multiplier applied to the player's attack power.</summary>
	[ContentSerializer(Optional = true)]
	public float AttackMultiplier;

	/// <summary>The combined buff to the player's resistance to negative effects.</summary>
	[ContentSerializer(Optional = true)]
	public float Immunity;

	/// <summary>The combined multiplier applied to monster knockback when hit by the player's weapon.</summary>
	[ContentSerializer(Optional = true)]
	public float KnockbackMultiplier;

	/// <summary>The combined multiplier applied to the player's weapon swing speed.</summary>
	[ContentSerializer(Optional = true)]
	public float WeaponSpeedMultiplier;

	/// <summary>The combined multiplier applied to the player's critical hit chance.</summary>
	[ContentSerializer(Optional = true)]
	public float CriticalChanceMultiplier;

	/// <summary>The combined multiplier applied to the player's critical hit damage.</summary>
	[ContentSerializer(Optional = true)]
	public float CriticalPowerMultiplier;

	/// <summary>The combined multiplier applied to the player's weapon accuracy.</summary>
	[ContentSerializer(Optional = true)]
	public float WeaponPrecisionMultiplier;
}
