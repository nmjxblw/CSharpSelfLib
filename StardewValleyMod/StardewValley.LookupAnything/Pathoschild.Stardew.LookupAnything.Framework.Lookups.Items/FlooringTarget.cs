using System;
using Microsoft.Xna.Framework;
using StardewValley.TerrainFeatures;

namespace Pathoschild.Stardew.LookupAnything.Framework.Lookups.Items;

internal class FlooringTarget : GenericTarget<Flooring>
{
	public FlooringTarget(GameHelper gameHelper, Flooring value, Vector2 tilePosition, Func<ISubject> getSubject)
		: base(gameHelper, SubjectType.Object, value, tilePosition, getSubject)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		base.Precedence = 999;
	}

	public override Rectangle GetSpritesheetArea()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		return Rectangle.Empty;
	}
}
