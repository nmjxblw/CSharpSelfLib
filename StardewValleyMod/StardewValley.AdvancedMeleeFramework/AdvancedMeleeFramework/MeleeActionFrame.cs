using System;
using System.Collections.Generic;

namespace AdvancedMeleeFramework;

public class MeleeActionFrame
{
	public int frameTicks;

	public bool? invincible;

	public SpecialEffect special;

	[Obsolete("Never used in the code, will be removed in next version")]
	public WeaponFarmerAnimation animation;

	public WeaponAction action;

	public string sound;

	public List<AdvancedWeaponProjectile> projectiles = new List<AdvancedWeaponProjectile>();

	public int relativeFacingDirection;

	public float trajectoryX;

	public float trajectoryY;

	public Dictionary<string, string> config = new Dictionary<string, string>();
}
