using System;
using System.Collections.Generic;

namespace AdvancedMeleeFramework;

public class AdvancedWeaponProjectile
{
	public int damage;

	public int parentSheetIndex;

	public int bouncesTillDestruct;

	public int tailLength = 1;

	public float rotationVelocity = 1f;

	public float xVelocity;

	public float yVelocity = -1f;

	public float startingPositionX;

	public float startingPositionY;

	public string collisionSound;

	public string bounceSound;

	public string firingSound;

	public bool explode;

	public bool damagesMonsters = true;

	[Obsolete("Replaced in 1.6 with shotItemId")]
	public bool spriteFromObjectSheet;

	public string shotItemId;

	public Dictionary<string, string> config = new Dictionary<string, string>();
}
