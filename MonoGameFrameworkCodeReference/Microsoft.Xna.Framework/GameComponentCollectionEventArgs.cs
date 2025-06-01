using System;

namespace Microsoft.Xna.Framework;

/// <summary>
/// Arguments for the <see cref="E:Microsoft.Xna.Framework.GameComponentCollection.ComponentAdded" /> and
/// <see cref="E:Microsoft.Xna.Framework.GameComponentCollection.ComponentRemoved" /> events.
/// </summary>
public class GameComponentCollectionEventArgs : EventArgs
{
	private IGameComponent _gameComponent;

	/// <summary>
	/// The <see cref="T:Microsoft.Xna.Framework.IGameComponent" /> that the event notifies about.
	/// </summary>
	public IGameComponent GameComponent => this._gameComponent;

	/// <summary>
	/// Create a <see cref="T:Microsoft.Xna.Framework.GameComponentCollectionEventArgs" /> instance.
	/// </summary>
	/// <param name="gameComponent">The <see cref="T:Microsoft.Xna.Framework.IGameComponent" /> that the event notifies about.</param>
	public GameComponentCollectionEventArgs(IGameComponent gameComponent)
	{
		this._gameComponent = gameComponent;
	}
}
