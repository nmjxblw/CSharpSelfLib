using System;
using Microsoft.Xna.Framework;
using StardewValley;

namespace Pathoschild.Stardew.LookupAnything.Framework.Lookups.Characters;

internal class FarmerTarget : GenericTarget<Farmer>
{
	public FarmerTarget(GameHelper gameHelper, Farmer value, Func<ISubject> getSubject)
		: base(gameHelper, SubjectType.Farmer, value, ((Character)value).Tile, getSubject)
	{
	}//IL_0005: Unknown result type (might be due to invalid IL or missing references)


	public override Rectangle GetSpritesheetArea()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		return ((AnimatedSprite)base.Value.FarmerSprite).SourceRect;
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
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		return ((Rectangle)(ref spriteArea)).Contains((int)position.X, (int)position.Y);
	}
}
