using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.LookupAnything.Framework.Lookups;

namespace Pathoschild.Stardew.LookupAnything.Framework.Fields;

internal interface ICustomField
{
	string Label { get; }

	IFormattedText[]? Value { get; }

	bool HasValue { get; }

	bool MayHaveLinks { get; }

	LinkField? ExpandLink { get; }

	Vector2? DrawValue(SpriteBatch spriteBatch, SpriteFont font, Vector2 position, float wrapWidth);

	bool TryGetLinkAt(int x, int y, [NotNullWhen(true)] out ISubject? subject);
}
