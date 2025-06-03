namespace Microsoft.Xna.Framework;

/// <summary>
/// Defines how <see cref="T:Microsoft.Xna.Framework.Game" /> should be runned.
/// </summary>
public enum GameRunBehavior
{
	/// <summary>
	/// The game loop will be runned asynchronous.
	/// </summary>
	Asynchronous,
	/// <summary>
	/// The game loop will be runned synchronous.
	/// </summary>
	Synchronous
}
