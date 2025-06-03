using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace Pathoschild.Stardew.LookupAnything.Framework.Lookups.Characters;

internal class FarmAnimalTarget : GenericTarget<FarmAnimal>
{
	public FarmAnimalTarget(GameHelper gameHelper, FarmAnimal value, Vector2 tilePosition, Func<ISubject> getSubject)
		: base(gameHelper, SubjectType.FarmAnimal, value, tilePosition, getSubject)
	{
	}//IL_0004: Unknown result type (might be due to invalid IL or missing references)


	public override Rectangle GetSpritesheetArea()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		return ((Character)base.Value).Sprite.SourceRect;
	}

	public override Rectangle GetWorldArea()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		return base.GetSpriteArea(((Character)base.Value).GetBoundingBox(), this.GetSpritesheetArea());
	}

	public override bool SpriteIntersectsPixel(Vector2 tile, Vector2 position, Rectangle spriteArea)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		SpriteEffects spriteEffects = (SpriteEffects)(((Character)base.Value).flip ? 1 : 0);
		return base.SpriteIntersectsPixel(tile, position, spriteArea, ((Character)base.Value).Sprite.Texture, this.GetSpritesheetArea(), spriteEffects);
	}
}
