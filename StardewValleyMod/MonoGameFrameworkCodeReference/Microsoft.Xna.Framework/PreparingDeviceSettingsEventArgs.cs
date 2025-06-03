using System;

namespace Microsoft.Xna.Framework;

/// <summary>
/// The arguments to the <see cref="E:Microsoft.Xna.Framework.GraphicsDeviceManager.PreparingDeviceSettings" /> event.
/// </summary>
public class PreparingDeviceSettingsEventArgs : EventArgs
{
	/// <summary>
	/// The default settings that will be used in device creation.
	/// </summary>
	public GraphicsDeviceInformation GraphicsDeviceInformation { get; private set; }

	/// <summary>
	/// Create a new instance of the event.
	/// </summary>
	/// <param name="graphicsDeviceInformation">The default settings to be used in device creation.</param>
	public PreparingDeviceSettingsEventArgs(GraphicsDeviceInformation graphicsDeviceInformation)
	{
		this.GraphicsDeviceInformation = graphicsDeviceInformation;
	}
}
