using Netcode;
using StardewValley.GameData.Buffs;

namespace StardewValley.Buffs;

/// <summary>The combined buff attribute values applied to a player.</summary>
public class BuffEffects : INetObject<NetFields>
{
	/// <summary>The attributes which are added to the player's stats.</summary>
	private readonly NetFloat[] AdditiveFields;

	/// <summary>The attributes which are multiplied by the player's stats.</summary>
	private readonly NetFloat[] MultiplicativeFields;

	/// <summary>The buff to the player's combat skill level.</summary>
	public readonly NetFloat CombatLevel = new NetFloat(0f);

	/// <summary>The buff to the player's farming skill level.</summary>
	public readonly NetFloat FarmingLevel = new NetFloat(0f);

	/// <summary>The buff to the player's fishing skill level.</summary>
	public readonly NetFloat FishingLevel = new NetFloat(0f);

	/// <summary>The buff to the player's mining skill level.</summary>
	public readonly NetFloat MiningLevel = new NetFloat(0f);

	/// <summary>The buff to the player's luck skill level.</summary>
	public readonly NetFloat LuckLevel = new NetFloat(0f);

	/// <summary>The buff to the player's foraging skill level.</summary>
	public readonly NetFloat ForagingLevel = new NetFloat(0f);

	/// <summary>The buff to the player's max stamina.</summary>
	public readonly NetFloat MaxStamina = new NetFloat(0f);

	/// <summary>The buff to the player's magnetic radius.</summary>
	public readonly NetFloat MagneticRadius = new NetFloat(0f);

	/// <summary>The buff to the player's walk speed.</summary>
	public readonly NetFloat Speed = new NetFloat(0f);

	/// <summary>The buff to the player's defense.</summary>
	public readonly NetFloat Defense = new NetFloat(0f);

	/// <summary>The buff to the player's attack power.</summary>
	public readonly NetFloat Attack = new NetFloat(0f);

	/// <summary>The combined multiplier applied to the player's attack power.</summary>
	public readonly NetFloat AttackMultiplier = new NetFloat(0f);

	/// <summary>The combined buff to the player's resistance to negative effects.</summary>
	public readonly NetFloat Immunity = new NetFloat(0f);

	/// <summary>The combined multiplier applied to monster knockback when hit by the player's weapon.</summary>
	public readonly NetFloat KnockbackMultiplier = new NetFloat(0f);

	/// <summary>The combined multiplier applied to the player's weapon swing speed.</summary>
	public readonly NetFloat WeaponSpeedMultiplier = new NetFloat(0f);

	/// <summary>The combined multiplier applied to the player's critical hit chance.</summary>
	public readonly NetFloat CriticalChanceMultiplier = new NetFloat(0f);

	/// <summary>The combined multiplier applied to the player's critical hit damage.</summary>
	public readonly NetFloat CriticalPowerMultiplier = new NetFloat(0f);

	/// <summary>The combined multiplier applied to the player's weapon accuracy.</summary>
	public readonly NetFloat WeaponPrecisionMultiplier = new NetFloat(0f);

	public NetFields NetFields { get; } = new NetFields("BuffEffects");

	/// <summary>Construct an instance.</summary>
	public BuffEffects()
	{
		this.AdditiveFields = new NetFloat[12]
		{
			this.CombatLevel, this.FarmingLevel, this.FishingLevel, this.MiningLevel, this.LuckLevel, this.ForagingLevel, this.MaxStamina, this.MagneticRadius, this.Speed, this.Defense,
			this.Attack, this.Immunity
		};
		this.MultiplicativeFields = new NetFloat[6] { this.AttackMultiplier, this.KnockbackMultiplier, this.WeaponSpeedMultiplier, this.CriticalChanceMultiplier, this.CriticalPowerMultiplier, this.WeaponPrecisionMultiplier };
		this.NetFields.SetOwner(this).AddField(this.CombatLevel, "CombatLevel").AddField(this.FarmingLevel, "FarmingLevel")
			.AddField(this.FishingLevel, "FishingLevel")
			.AddField(this.MiningLevel, "MiningLevel")
			.AddField(this.LuckLevel, "LuckLevel")
			.AddField(this.ForagingLevel, "ForagingLevel")
			.AddField(this.MaxStamina, "MaxStamina")
			.AddField(this.MagneticRadius, "MagneticRadius")
			.AddField(this.Speed, "Speed")
			.AddField(this.Defense, "Defense")
			.AddField(this.Attack, "Attack")
			.AddField(this.AttackMultiplier, "AttackMultiplier")
			.AddField(this.Immunity, "Immunity")
			.AddField(this.KnockbackMultiplier, "KnockbackMultiplier")
			.AddField(this.WeaponSpeedMultiplier, "WeaponSpeedMultiplier")
			.AddField(this.CriticalChanceMultiplier, "CriticalChanceMultiplier")
			.AddField(this.CriticalPowerMultiplier, "CriticalPowerMultiplier")
			.AddField(this.WeaponPrecisionMultiplier, "WeaponPrecisionMultiplier");
	}

	/// <summary>Construct an instance.</summary>
	/// <param name="data">The initial attributes to copy from raw object data.</param>
	public BuffEffects(BuffAttributesData data)
		: this()
	{
		this.Add(data);
	}

	/// <summary>Add another buff's effects to the stats.</summary>
	/// <param name="other">The buff effects to add.</param>
	public void Add(BuffEffects other)
	{
		if (other != null)
		{
			for (int i = 0; i < this.AdditiveFields.Length; i++)
			{
				this.AdditiveFields[i].Value += other.AdditiveFields[i].Value;
			}
			for (int j = 0; j < this.MultiplicativeFields.Length; j++)
			{
				this.MultiplicativeFields[j].Value += other.MultiplicativeFields[j].Value;
			}
		}
	}

	/// <summary>Add buff effect data to the stats.</summary>
	/// <param name="data">The buff effect data to add.</param>
	public void Add(BuffAttributesData data)
	{
		if (data != null)
		{
			this.CombatLevel.Value = data.CombatLevel;
			this.FarmingLevel.Value = data.FarmingLevel;
			this.FishingLevel.Value = data.FishingLevel;
			this.MiningLevel.Value = data.MiningLevel;
			this.LuckLevel.Value = data.LuckLevel;
			this.ForagingLevel.Value = data.ForagingLevel;
			this.MaxStamina.Value = data.MaxStamina;
			this.MagneticRadius.Value = data.MagneticRadius;
			this.Speed.Value = data.Speed;
			this.Defense.Value = data.Defense;
			this.Attack.Value = data.Attack;
			this.AttackMultiplier.Value = data.AttackMultiplier;
			this.Immunity.Value = data.Immunity;
			this.KnockbackMultiplier.Value = data.KnockbackMultiplier;
			this.WeaponSpeedMultiplier.Value = data.WeaponSpeedMultiplier;
			this.CriticalChanceMultiplier.Value = data.CriticalChanceMultiplier;
			this.CriticalPowerMultiplier.Value = data.CriticalPowerMultiplier;
			this.WeaponPrecisionMultiplier.Value = data.WeaponPrecisionMultiplier;
		}
	}

	/// <summary>Get whether any stat has a non-zero value.</summary>
	public bool HasAnyValue()
	{
		NetFloat[] additiveFields = this.AdditiveFields;
		for (int i = 0; i < additiveFields.Length; i++)
		{
			if (additiveFields[i].Value != 0f)
			{
				return true;
			}
		}
		additiveFields = this.MultiplicativeFields;
		for (int i = 0; i < additiveFields.Length; i++)
		{
			if (additiveFields[i].Value != 0f)
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>Remove all effects from the stats.</summary>
	public void Clear()
	{
		NetFloat[] additiveFields = this.AdditiveFields;
		for (int i = 0; i < additiveFields.Length; i++)
		{
			additiveFields[i].Value = 0f;
		}
		additiveFields = this.MultiplicativeFields;
		for (int i = 0; i < additiveFields.Length; i++)
		{
			additiveFields[i].Value = 0f;
		}
	}

	/// <summary>Get the main effects in the pre-1.6 <c>Data/ObjectInformation</c> format.</summary>
	/// <remarks>This is a specialized method and shouldn't be used by most code.</remarks>
	public string[] ToLegacyAttributeFormat()
	{
		return new string[13]
		{
			((int)this.FarmingLevel.Value).ToString(),
			((int)this.FishingLevel.Value).ToString(),
			((int)this.MiningLevel.Value).ToString(),
			"0",
			((int)this.LuckLevel.Value).ToString(),
			((int)this.ForagingLevel.Value).ToString(),
			"0",
			((int)this.MaxStamina.Value).ToString(),
			((int)this.MagneticRadius.Value).ToString(),
			this.Speed.Value.ToString(),
			((int)this.Defense.Value).ToString(),
			((int)this.Attack.Value).ToString(),
			""
		};
	}
}
