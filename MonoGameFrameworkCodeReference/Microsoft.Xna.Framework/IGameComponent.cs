namespace Microsoft.Xna.Framework;

/// <summary>
/// An interface for <see cref="T:Microsoft.Xna.Framework.GameComponent" />.
/// </summary>
public interface IGameComponent
{
	/// <summary>
	/// Initializes the component. Used to load non-graphical resources.
	/// </summary>
	void Initialize();
}
