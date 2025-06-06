namespace Microsoft.Xna.Framework.Graphics;

public class GraphicsDebug
{
	/// <summary>
	/// Attempt to dequeue a debugging message from the graphics subsystem.
	/// </summary>
	/// <remarks>
	/// When running on a graphics device with debugging enabled, this allows you to retrieve
	/// subsystem-specific (e.g. DirectX, OpenGL, etc.) debugging messages including information
	/// about improper usage of shaders and APIs.
	/// </remarks>
	/// <param name="message">The graphics debugging message if retrieved, null otherwise.</param>
	/// <returns>True if a graphics debugging message was retrieved, false otherwise.</returns>
	public bool TryDequeueMessage(out GraphicsDebugMessage message)
	{
		return this.PlatformTryDequeueMessage(out message);
	}

	private bool PlatformTryDequeueMessage(out GraphicsDebugMessage message)
	{
		message = null;
		return false;
	}
}
