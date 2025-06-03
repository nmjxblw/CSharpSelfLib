using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Audio;

internal static class SoundEffectInstancePool
{
	internal static readonly List<SoundEffectInstance> _playingInstances;

	private static readonly List<SoundEffectInstance> _pooledInstances;

	internal static readonly object _locker;

	/// <summary>
	/// Gets a value indicating whether the platform has capacity for more sounds to be played at this time.
	/// </summary>
	/// <value><c>true</c> if more sounds can be played; otherwise, <c>false</c>.</value>
	internal static bool SoundsAvailable
	{
		get
		{
			lock (SoundEffectInstancePool._locker)
			{
				return SoundEffectInstancePool._playingInstances.Count < 256;
			}
		}
	}

	static SoundEffectInstancePool()
	{
		SoundEffectInstancePool._locker = new object();
		SoundEffectInstancePool._playingInstances = new List<SoundEffectInstance>(256);
		SoundEffectInstancePool._pooledInstances = new List<SoundEffectInstance>(256);
	}

	/// <summary>
	/// Add the specified instance to the pool if it is a pooled instance and removes it from the
	/// list of playing instances.
	/// </summary>
	/// <param name="inst">The SoundEffectInstance</param>
	internal static void Add(SoundEffectInstance inst)
	{
		lock (SoundEffectInstancePool._locker)
		{
			if (inst._isPooled)
			{
				SoundEffectInstancePool._pooledInstances.Add(inst);
				inst._effect = null;
			}
			SoundEffectInstancePool._playingInstances.Remove(inst);
		}
	}

	/// <summary>
	/// Adds the SoundEffectInstance to the list of playing instances.
	/// </summary>
	/// <param name="inst">The SoundEffectInstance to add to the playing list.</param>
	internal static void Remove(SoundEffectInstance inst)
	{
		lock (SoundEffectInstancePool._locker)
		{
			SoundEffectInstancePool._playingInstances.Add(inst);
		}
	}

	/// <summary>
	/// Returns a pooled SoundEffectInstance if one is available, or allocates a new
	/// SoundEffectInstance if the pool is empty.
	/// </summary>
	/// <returns>The SoundEffectInstance.</returns>
	internal static SoundEffectInstance GetInstance(bool forXAct)
	{
		lock (SoundEffectInstancePool._locker)
		{
			SoundEffectInstance inst = null;
			int count = SoundEffectInstancePool._pooledInstances.Count;
			if (count > 0)
			{
				inst = SoundEffectInstancePool._pooledInstances[count - 1];
				SoundEffectInstancePool._pooledInstances.RemoveAt(count - 1);
				inst._isPooled = true;
				inst._isXAct = forXAct;
				inst.Volume = 1f;
				inst.Pan = 0f;
				inst.Pitch = 0f;
				inst.LoopCount = 0u;
				inst.PlatformSetReverbMix(0f);
				inst.PlatformClearFilter();
			}
			else
			{
				inst = new SoundEffectInstance();
				inst._isPooled = true;
				inst._isXAct = forXAct;
			}
			return inst;
		}
	}

	/// <summary>
	/// Iterates the list of playing instances, returning them to the pool if they
	/// have stopped playing.
	/// </summary>
	internal static void Update()
	{
	}

	/// <summary>
	/// Iterates the list of playing instances, stop them and return them to the pool if they are instances of the given SoundEffect.
	/// </summary>
	/// <param name="effect">The SoundEffect</param>
	internal static void StopPooledInstances(SoundEffect effect)
	{
		lock (SoundEffectInstancePool._locker)
		{
			SoundEffectInstance inst = null;
			int x = 0;
			while (x < SoundEffectInstancePool._playingInstances.Count)
			{
				inst = SoundEffectInstancePool._playingInstances[x];
				if (inst._effect == effect)
				{
					inst.Stop(immediate: true);
					SoundEffectInstancePool.Add(inst);
				}
				else
				{
					x++;
				}
			}
		}
	}

	internal static void UpdateMasterVolume()
	{
		lock (SoundEffectInstancePool._locker)
		{
			foreach (SoundEffectInstance inst in SoundEffectInstancePool._playingInstances)
			{
				if (!inst._isXAct)
				{
					inst.Volume = inst.Volume;
				}
			}
		}
	}
}
