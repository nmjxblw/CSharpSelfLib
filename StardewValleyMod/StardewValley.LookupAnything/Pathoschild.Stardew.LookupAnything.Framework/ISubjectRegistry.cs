using Pathoschild.Stardew.LookupAnything.Framework.Lookups;
using StardewValley;

namespace Pathoschild.Stardew.LookupAnything.Framework;

internal interface ISubjectRegistry
{
	ISubject? GetByEntity(object entity, GameLocation? location);
}
