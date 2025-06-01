namespace MonoGame.Framework.Utilities;

/// <summary>
/// Type of the underlying graphics backend.
/// </summary>
public enum GraphicsBackend
{
	/// <summary>
	/// Represents the Microsoft DirectX graphics backend.
	/// </summary>
	DirectX,
	/// <summary>
	/// Represents the OpenGL graphics backend.
	/// </summary>
	OpenGL,
	/// <summary>
	/// Represents the Vulkan graphics backend.
	/// </summary>
	Vulkan,
	/// <summary>
	/// Represents the Apple Metal graphics backend.
	/// </summary>
	Metal
}
