using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.TerrainFeatures;

namespace Pathoschild.Stardew.LookupAnything.Framework.Lookups.TerrainFeatures;

internal class BushTarget : GenericTarget<Bush>
{
	public BushTarget(GameHelper gameHelper, Bush value, Func<ISubject> getSubject)
		: base(gameHelper, SubjectType.Bush, value, ((TerrainFeature)value).Tile, getSubject)
	{
	}//IL_0006: Unknown result type (might be due to invalid IL or missing references)


	public override Rectangle GetSpritesheetArea()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		Bush bush = base.Value;
		return ((NetFieldBase<Rectangle, NetRectangle>)(object)bush.sourceRect).Value;
	}

	public override Rectangle GetWorldArea()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		return base.GetSpriteArea(((TerrainFeature)base.Value).getBoundingBox(), this.GetSpritesheetArea());
	}

	public override bool SpriteIntersectsPixel(Vector2 tile, Vector2 position, Rectangle spriteArea)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		SpriteEffects spriteEffects = (SpriteEffects)(((NetFieldBase<bool, NetBool>)(object)base.Value.flipped).Value ? 1 : 0);
		return base.SpriteIntersectsPixel(tile, position, spriteArea, Bush.texture.Value, this.GetSpritesheetArea(), spriteEffects);
	}
}
