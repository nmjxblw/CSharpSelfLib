using System;

namespace Microsoft.Xna.Framework;

/// <summary>
/// Interface for drawable entities.
/// </summary>
public interface IDrawable
{
	/// <summary>
	/// The draw order of this <see cref="T:Microsoft.Xna.Framework.IDrawable" /> relative
	/// to other <see cref="T:Microsoft.Xna.Framework.IDrawable" /> instances.
	/// </summary>
	int DrawOrder { get; }

	/// <summary>
	/// Indicates if <see cref="M:Microsoft.Xna.Framework.IDrawable.Draw(Microsoft.Xna.Framework.GameTime)" /> will be called.
	/// </summary>
	bool Visible { get; }

	/// <summary>
	/// Raised when <see cref="P:Microsoft.Xna.Framework.IDrawable.DrawOrder" /> changed.
	/// </summary>
	event EventHandler<EventArgs> DrawOrderChanged;

	/// <summary>
	/// Raised when <see cref="P:Microsoft.Xna.Framework.IDrawable.Visible" /> changed.
	/// </summary>
	event EventHandler<EventArgs> VisibleChanged;

	/// <summary>
	/// Called when this <see cref="T:Microsoft.Xna.Framework.IDrawable" /> should draw itself.
	/// </summary>
	/// <param name="gameTime">The elapsed time since the last call to <see cref="M:Microsoft.Xna.Framework.IDrawable.Draw(Microsoft.Xna.Framework.GameTime)" />.</param>
	void Draw(GameTime gameTime);
}
