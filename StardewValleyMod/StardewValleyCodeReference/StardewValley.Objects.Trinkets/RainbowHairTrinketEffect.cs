namespace StardewValley.Objects.Trinkets;

/// <summary>Implements the special behavior for a <see cref="T:StardewValley.Objects.Trinkets.Trinket" /> which makes the player's hair prismatic.</summary>
public class RainbowHairTrinketEffect : TrinketEffect
{
	/// <inheritdoc />
	public RainbowHairTrinketEffect(Trinket trinket)
		: base(trinket)
	{
	}

	/// <inheritdoc />
	public override void Apply(Farmer farmer)
	{
		farmer.prismaticHair.Value = true;
	}

	/// <inheritdoc />
	public override void Unapply(Farmer farmer)
	{
		farmer.prismaticHair.Value = false;
	}
}
