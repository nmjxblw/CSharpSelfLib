using System;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework.Content;

public static class ContentReaderExtensions
{
	/// <summary>
	/// Gets the GraphicsDevice from the ContentManager.ServiceProvider.
	/// </summary>
	/// <returns>The <see cref="T:Microsoft.Xna.Framework.Graphics.GraphicsDevice" />.</returns>
	public static GraphicsDevice GetGraphicsDevice(this ContentReader contentReader)
	{
		return ((contentReader.ContentManager.ServiceProvider.GetService(typeof(IGraphicsDeviceService)) as IGraphicsDeviceService) ?? throw new InvalidOperationException("No Graphics Device Service")).GraphicsDevice;
	}
}
