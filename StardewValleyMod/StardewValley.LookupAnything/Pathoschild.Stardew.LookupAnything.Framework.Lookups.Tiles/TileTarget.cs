using System;
using Microsoft.Xna.Framework;

namespace Pathoschild.Stardew.LookupAnything.Framework.Lookups.Tiles;

internal class TileTarget : GenericTarget<Vector2>
{
	public TileTarget(GameHelper gameHelper, Vector2 position, Func<ISubject> getSubject)
		: base(gameHelper, SubjectType.Tile, position, position, getSubject)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		base.Precedence = 1000;
	}

	public override Rectangle GetSpritesheetArea()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		return Rectangle.Empty;
	}
}
