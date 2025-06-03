using System;

namespace StardewValley.Characters;

/// <summary>Obsolete. This is only kept to preserve data from old save files. All dogs now use the <see cref="T:StardewValley.Characters.Pet" /> class instead.</summary>
[Obsolete("All dogs now use the Pet class.")]
public class Dog : Pet
{
	public Dog()
	{
		this.Sprite = new AnimatedSprite(this.getPetTextureName(), 0, 32, 32);
		base.HideShadow = true;
		base.Breather = false;
		base.willDestroyObjectsUnderfoot = false;
	}
}
