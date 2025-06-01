using System;

namespace Microsoft.Xna.Framework;

/// <summary>
/// Interface for updateable entities.
/// </summary>
public interface IUpdateable
{
	/// <summary>
	/// Indicates if <see cref="M:Microsoft.Xna.Framework.IUpdateable.Update(Microsoft.Xna.Framework.GameTime)" /> will be called.
	/// </summary>
	bool Enabled { get; }

	/// <summary>
	/// The update order of this <see cref="T:Microsoft.Xna.Framework.GameComponent" /> relative
	/// to other <see cref="T:Microsoft.Xna.Framework.GameComponent" /> instances.
	/// </summary>
	int UpdateOrder { get; }

	/// <summary>
	/// Raised when <see cref="P:Microsoft.Xna.Framework.IUpdateable.Enabled" /> changed.
	/// </summary>
	event EventHandler<EventArgs> EnabledChanged;

	/// <summary>
	/// Raised when <see cref="P:Microsoft.Xna.Framework.IUpdateable.UpdateOrder" /> changed.
	/// </summary>
	event EventHandler<EventArgs> UpdateOrderChanged;

	/// <summary>
	/// Called when this <see cref="T:Microsoft.Xna.Framework.IUpdateable" /> should update itself.
	/// </summary>
	/// <param name="gameTime">The elapsed time since the last call to <see cref="M:Microsoft.Xna.Framework.IUpdateable.Update(Microsoft.Xna.Framework.GameTime)" />.</param>
	void Update(GameTime gameTime);
}
