using Microsoft.Xna.Framework.Audio;

namespace Microsoft.Xna.Framework;

/// <summary>
/// Helper class for processing internal framework events.
/// </summary>
/// <remarks>
/// If you use <see cref="T:Microsoft.Xna.Framework.Game" /> class, <see cref="M:Microsoft.Xna.Framework.FrameworkDispatcher.Update" /> is called automatically.
/// Otherwise you must call it as part of your game loop.
/// </remarks>
public static class FrameworkDispatcher
{
	private static bool _initialized;

	/// <summary>
	/// Processes framework events.
	/// </summary>
	public static void Update()
	{
		if (!FrameworkDispatcher._initialized)
		{
			FrameworkDispatcher.Initialize();
		}
		FrameworkDispatcher.DoUpdate();
	}

	private static void DoUpdate()
	{
		DynamicSoundEffectInstanceManager.UpdatePlayingInstances();
		SoundEffectInstancePool.Update();
		Microphone.UpdateMicrophones();
	}

	private static void Initialize()
	{
		FrameworkDispatcher._initialized = true;
	}
}
