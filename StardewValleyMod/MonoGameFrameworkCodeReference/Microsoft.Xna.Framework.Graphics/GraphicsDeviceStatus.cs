namespace Microsoft.Xna.Framework.Graphics;

/// <summary>
/// Describes the status of the <see cref="T:Microsoft.Xna.Framework.Graphics.GraphicsDevice" />.
/// </summary>
public enum GraphicsDeviceStatus
{
	/// <summary>
	/// The device is normal.
	/// </summary>
	Normal,
	/// <summary>
	/// The device has been lost.
	/// </summary>
	Lost,
	/// <summary>
	/// The device has not been reset.
	/// </summary>
	NotReset
}
