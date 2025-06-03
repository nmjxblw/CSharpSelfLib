using System;

namespace Pathoschild.Stardew.LookupAnything.Framework.Data;

[Flags]
internal enum ObjectContext
{
	World = 1,
	Inventory = 2,
	Any = 3
}
