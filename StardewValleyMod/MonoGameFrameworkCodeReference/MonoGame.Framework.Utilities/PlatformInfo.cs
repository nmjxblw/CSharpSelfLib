namespace MonoGame.Framework.Utilities;

/// <summary>
/// Utility class that returns information about the underlying platform
/// </summary>
public static class PlatformInfo
{
	/// <summary>
	/// Underlying game platform type
	/// </summary>
	public static MonoGamePlatform MonoGamePlatform => MonoGamePlatform.DesktopGL;

	/// <summary>
	/// Graphics backend
	/// </summary>
	public static GraphicsBackend GraphicsBackend => GraphicsBackend.OpenGL;
}
