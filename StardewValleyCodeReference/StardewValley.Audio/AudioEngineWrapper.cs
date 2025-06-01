using System;
using Microsoft.Xna.Framework.Audio;

namespace StardewValley.Audio;

internal class AudioEngineWrapper : IAudioEngine, IDisposable
{
	private AudioEngine audioEngine;

	public AudioEngine Engine => this.audioEngine;

	public bool IsDisposed => this.audioEngine.IsDisposed;

	public AudioEngineWrapper(AudioEngine engine)
	{
		this.audioEngine = engine;
	}

	public void Dispose()
	{
		this.audioEngine.Dispose();
	}

	public IAudioCategory GetCategory(string name)
	{
		return new AudioCategoryWrapper(this.audioEngine.GetCategory(name));
	}

	public int GetCategoryIndex(string name)
	{
		return this.audioEngine.GetCategoryIndex(name);
	}

	public void Update()
	{
		this.audioEngine.Update();
	}
}
