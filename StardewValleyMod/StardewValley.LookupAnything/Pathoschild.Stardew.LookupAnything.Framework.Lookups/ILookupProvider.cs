using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Menus;

namespace Pathoschild.Stardew.LookupAnything.Framework.Lookups;

internal interface ILookupProvider
{
	IEnumerable<ITarget> GetTargets(GameLocation location, Vector2 lookupTile);

	ISubject? GetSubject(IClickableMenu menu, int cursorX, int cursorY);

	ISubject? GetSubjectFor(object entity, GameLocation? location);

	IEnumerable<ISubject> GetSearchSubjects();
}
