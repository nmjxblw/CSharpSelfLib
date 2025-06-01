namespace Microsoft.Xna.Framework.Input.Touch;

/// <summary>
/// Allows retrieval of capabilities information from touch panel device.
/// </summary>
public struct TouchPanelCapabilities
{
	private bool hasPressure;

	private bool isConnected;

	private int maximumTouchCount;

	private bool initialized;

	public bool HasPressure => this.hasPressure;

	/// <summary>
	/// Returns true if a device is available for use.
	/// </summary>
	public bool IsConnected => this.isConnected;

	/// <summary>
	/// Returns the maximum number of touch locations tracked by the touch panel device.
	/// </summary>
	public int MaximumTouchCount => this.maximumTouchCount;

	internal void Initialize()
	{
		if (!this.initialized)
		{
			this.initialized = true;
			this.hasPressure = false;
			this.isConnected = false;
		}
	}
}
