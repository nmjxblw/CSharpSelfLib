using System;

namespace Microsoft.Xna.Framework.Graphics;

/// <summary>
/// Provider of a <see cref="T:Microsoft.Xna.Framework.Graphics.GraphicsDevice" />.
/// </summary>
public interface IGraphicsDeviceService
{
	/// <summary>
	/// The provided <see cref="T:Microsoft.Xna.Framework.Graphics.GraphicsDevice" />.
	/// </summary>
	GraphicsDevice GraphicsDevice { get; }

	/// <summary>
	/// Raised when a new <see cref="T:Microsoft.Xna.Framework.Graphics.GraphicsDevice" /> has been created.
	/// </summary>
	event EventHandler<EventArgs> DeviceCreated;

	/// <summary>
	/// Raised when the <see cref="P:Microsoft.Xna.Framework.Graphics.IGraphicsDeviceService.GraphicsDevice" /> is disposed.
	/// </summary>
	event EventHandler<EventArgs> DeviceDisposing;

	/// <summary>
	/// Raised when the <see cref="P:Microsoft.Xna.Framework.Graphics.IGraphicsDeviceService.GraphicsDevice" /> has reset.
	/// </summary>
	/// <seealso cref="M:Microsoft.Xna.Framework.Graphics.GraphicsDevice.Reset" />
	event EventHandler<EventArgs> DeviceReset;

	/// <summary>
	/// Raised before the <see cref="P:Microsoft.Xna.Framework.Graphics.IGraphicsDeviceService.GraphicsDevice" /> is resetting.
	/// </summary>
	event EventHandler<EventArgs> DeviceResetting;
}
