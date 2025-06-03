using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace Pathoschild.Stardew.LookupAnything.Framework.Lookups;

internal abstract class BaseLookupProvider : ILookupProvider
{
	protected readonly IReflectionHelper Reflection;

	protected readonly GameHelper GameHelper;

	public virtual IEnumerable<ITarget> GetTargets(GameLocation location, Vector2 lookupTile)
	{
		yield break;
	}

	public virtual ISubject? GetSubject(IClickableMenu menu, int cursorX, int cursorY)
	{
		return null;
	}

	public virtual ISubject? GetSubjectFor(object entity, GameLocation? location)
	{
		return null;
	}

	public virtual IEnumerable<ISubject> GetSearchSubjects()
	{
		yield break;
	}

	protected BaseLookupProvider(IReflectionHelper reflection, GameHelper gameHelper)
	{
		this.Reflection = reflection;
		this.GameHelper = gameHelper;
	}
}
