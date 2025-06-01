namespace StardewValley.Menus;

/// <summary>A UI element that provides information for screen readers.</summary>
/// <remarks>These values aren't displayed by the game; they're provided to allow for implementing screen reader mods.</remarks>
public interface IScreenReadable
{
	/// <summary>If set, the translated text which represents this component for a screen reader. This may be the displayed text (for a text component), or an equivalent representation (e.g. "exit" for an 'X' button).</summary>
	string ScreenReaderText { get; }

	/// <summary>If set, a translated tooltip-like description for this component which can be displayed by screen readers, in addition to the <see cref="P:StardewValley.Menus.IScreenReadable.ScreenReaderText" />.</summary>
	string ScreenReaderDescription { get; }

	/// <summary>Whether this is a purely visual component which should be ignored by screen readers.</summary>
	bool ScreenReaderIgnore { get; }
}
