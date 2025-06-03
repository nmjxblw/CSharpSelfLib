using System;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Audio;

/// <summary>
/// Handles the buffer events of all DynamicSoundEffectInstance instances.
/// </summary>
internal static class DynamicSoundEffectInstanceManager
{
	private static readonly List<WeakReference> _playingInstances;

	static DynamicSoundEffectInstanceManager()
	{
		DynamicSoundEffectInstanceManager._playingInstances = new List<WeakReference>();
	}

	public static void AddInstance(SoundEffectInstance instance)
	{
	}

	public static void RemoveInstance(SoundEffectInstance instance)
	{
	}

	/// <summary>
	/// Updates buffer queues of the currently playing instances.
	/// </summary>
	/// <remarks>
	/// XNA posts <see cref="E:Microsoft.Xna.Framework.Audio.DynamicSoundEffectInstance.BufferNeeded" /> events always on the main thread.
	/// </remarks>
	public static void UpdatePlayingInstances()
	{
	}
}
