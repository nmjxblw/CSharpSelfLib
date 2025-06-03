using Microsoft.Xna.Framework.Input;

namespace Microsoft.Xna.Framework;

/// <summary>
/// This class is used in the <see cref="E:Microsoft.Xna.Framework.GameWindow.TextInput" /> event as <see cref="T:System.EventArgs" />.
/// </summary>
public struct TextInputEventArgs
{
	/// <summary>
	/// The character for the key that was pressed.
	/// </summary>
	public readonly char Character;

	/// <summary>
	/// The pressed key.
	/// </summary>
	public readonly Keys Key;

	public TextInputEventArgs(char character, Keys key = Keys.None)
	{
		this.Character = character;
		this.Key = key;
	}
}
