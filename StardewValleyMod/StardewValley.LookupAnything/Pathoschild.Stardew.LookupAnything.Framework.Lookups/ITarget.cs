using System;
using Microsoft.Xna.Framework;

namespace Pathoschild.Stardew.LookupAnything.Framework.Lookups;

internal interface ITarget
{
	SubjectType Type { get; }

	Vector2 Tile { get; }

	Func<ISubject?> GetSubject { get; }

	int Precedence { get; }

	Rectangle GetSpritesheetArea();

	Rectangle GetWorldArea();

	bool SpriteIntersectsPixel(Vector2 tile, Vector2 position, Rectangle spriteArea);
}
