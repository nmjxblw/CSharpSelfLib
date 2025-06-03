using System;
using System.Collections.Generic;
using System.Threading;

namespace Microsoft.Xna.Framework.Audio;

internal class OpenALSoundEffectInstanceManager : IDisposable
{
	internal static bool paused = false;

	internal static readonly object pauseMutex = new object();

	private static readonly object singletonMutex = new object();

	private static OpenALSoundEffectInstanceManager instance;

	private readonly Thread underlyingThread;

	private volatile bool running;

	private readonly List<WeakReference> _threadLocalInstances = new List<WeakReference>();

	internal static OpenALSoundEffectInstanceManager Instance
	{
		get
		{
			lock (OpenALSoundEffectInstanceManager.singletonMutex)
			{
				if (OpenALSoundEffectInstanceManager.instance == null)
				{
					throw new InvalidOperationException("No instance running");
				}
				return OpenALSoundEffectInstanceManager.instance;
			}
		}
		private set
		{
			lock (OpenALSoundEffectInstanceManager.singletonMutex)
			{
				OpenALSoundEffectInstanceManager.instance = value;
			}
		}
	}

	public OpenALSoundEffectInstanceManager()
	{
		lock (OpenALSoundEffectInstanceManager.singletonMutex)
		{
			if (OpenALSoundEffectInstanceManager.instance != null)
			{
				throw new InvalidOperationException("Already running");
			}
			this.running = true;
			OpenALSoundEffectInstanceManager.instance = this;
			this.underlyingThread = new Thread(Update)
			{
				Priority = ThreadPriority.Lowest,
				IsBackground = true
			};
			this.underlyingThread.Start();
		}
	}

	public void Update()
	{
		while (this.running)
		{
			Thread.Sleep(30);
			if (!this.running)
			{
				break;
			}
			lock (OpenALSoundEffectInstanceManager.pauseMutex)
			{
				if (OpenALSoundEffectInstanceManager.paused)
				{
					continue;
				}
				lock (SoundEffectInstancePool._locker)
				{
					this._threadLocalInstances.Clear();
					foreach (SoundEffectInstance instance in SoundEffectInstancePool._playingInstances)
					{
						this._threadLocalInstances.Add(new WeakReference(instance));
					}
				}
				SoundEffectInstance inst = null;
				for (int x = 0; x < this._threadLocalInstances.Count; x++)
				{
					inst = this._threadLocalInstances[x]?.Target as SoundEffectInstance;
					if (inst.IsDisposed || inst.State != SoundState.Playing || (inst._effect == null && !inst._isDynamic))
					{
						if (inst._isXAct)
						{
							continue;
						}
						lock (SoundEffectInstancePool._locker)
						{
							if (inst.IsDisposed)
							{
								goto IL_0117;
							}
							inst.Stop(immediate: true);
							if (!inst._isDynamic)
							{
								goto IL_0117;
							}
							goto end_IL_00f6;
							IL_0117:
							SoundEffectInstancePool.Add(inst);
							end_IL_00f6:;
						}
					}
					else
					{
						inst.UpdateQueue();
					}
				}
			}
		}
	}

	public void Dispose()
	{
		this.running = false;
	}
}
