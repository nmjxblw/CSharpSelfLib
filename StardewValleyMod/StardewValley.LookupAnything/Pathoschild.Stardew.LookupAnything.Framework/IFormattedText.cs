using Microsoft.Xna.Framework;

namespace Pathoschild.Stardew.LookupAnything.Framework;

internal interface IFormattedText
{
	Color? Color { get; }

	string? Text { get; }

	bool Bold { get; }
}
