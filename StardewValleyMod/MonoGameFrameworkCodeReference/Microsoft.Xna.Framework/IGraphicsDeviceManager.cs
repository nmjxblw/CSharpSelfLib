namespace Microsoft.Xna.Framework;

/// <summary>
/// Used by the platform code to control the graphics device.
/// </summary>
public interface IGraphicsDeviceManager
{
	/// <summary>
	/// Called at the start of rendering a frame.
	/// </summary>
	/// <returns>Returns true if the frame should be rendered.</returns>
	bool BeginDraw();

	/// <summary>
	/// Called to create the graphics device.
	/// </summary>
	/// <remarks>Does nothing if the graphics device is already created.</remarks>
	void CreateDevice();

	/// <summary>
	/// Called after rendering to present the frame to the screen.
	/// </summary>
	void EndDraw();
}
