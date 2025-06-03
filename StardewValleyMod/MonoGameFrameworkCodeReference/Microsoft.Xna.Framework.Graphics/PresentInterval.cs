namespace Microsoft.Xna.Framework.Graphics;

/// <summary>
/// Defines how <see cref="M:Microsoft.Xna.Framework.Graphics.GraphicsDevice.Present" /> updates the game window.
/// </summary>
public enum PresentInterval
{
	/// <summary>
	/// Equivalent to <see cref="F:Microsoft.Xna.Framework.Graphics.PresentInterval.One" />.
	/// </summary>
	Default,
	/// <summary>
	/// The driver waits for the vertical retrace period, before updating window client area. Present operations are not affected more frequently than the screen refresh rate.
	/// </summary>
	One,
	/// <summary>
	/// The driver waits for the vertical retrace period, before updating window client area. Present operations are not affected more frequently than every second screen refresh. 
	/// </summary>
	Two,
	/// <summary>
	/// The driver updates the window client area immediately. Present operations might be affected immediately. There is no limit for framerate.
	/// </summary>
	Immediate
}
