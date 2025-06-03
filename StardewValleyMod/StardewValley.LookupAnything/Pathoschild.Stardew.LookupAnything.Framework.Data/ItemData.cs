using System.Collections.Generic;

namespace Pathoschild.Stardew.LookupAnything.Framework.Data;

internal record ItemData
{
	public ObjectContext Context { get; set; } = ObjectContext.Any;

	public HashSet<string> QualifiedId { get; set; } = new HashSet<string>();

	public string? NameKey { get; set; }

	public string? DescriptionKey { get; set; }

	public string? TypeKey { get; set; }

	public bool? ShowInventoryFields { get; set; }
}
