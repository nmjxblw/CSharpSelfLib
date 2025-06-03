using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.LookupAnything.Framework.DebugFields;
using Pathoschild.Stardew.LookupAnything.Framework.Fields;

namespace Pathoschild.Stardew.LookupAnything.Framework.Lookups;

internal interface ISubject
{
	string Name { get; }

	string? Description { get; }

	string? Type { get; }

	IEnumerable<ICustomField> GetData();

	IEnumerable<IDebugField> GetDebugFields();

	bool DrawPortrait(SpriteBatch spriteBatch, Vector2 position, Vector2 size);
}
