using System;
using Microsoft.Xna.Framework.Audio;

namespace StardewValley;

/// <summary>The default sound bank implementation which defers to MonoGame audio.</summary>
public class SoundBankWrapper : ISoundBank, IDisposable
{
	/// <summary>The audio cue name used when a non-existent audio cue is requested to avoid a game crash.</summary>
	private string DefaultCueName = "shiny4";

	/// <summary>The underlying MonoGame sound bank.</summary>
	private SoundBank soundBank;

	/// <inheritdoc />
	public bool IsInUse => this.soundBank.IsInUse;

	/// <inheritdoc />
	public bool IsDisposed => this.soundBank.IsDisposed;

	/// <summary>Construct an instance.</summary>
	/// <param name="soundBank">The underlying MonoGame sound bank.</param>
	public SoundBankWrapper(SoundBank soundBank)
	{
		this.soundBank = soundBank;
	}

	/// <inheritdoc />
	public ICue GetCue(string name)
	{
		if (!this.Exists(name))
		{
			Game1.log.Error("Can't get audio ID '" + name + "' because it doesn't exist.");
			name = this.DefaultCueName;
		}
		return new CueWrapper(this.soundBank.GetCue(name));
	}

	/// <inheritdoc />
	public void PlayCue(string name)
	{
		if (!this.Exists(name))
		{
			Game1.log.Error("Can't play audio ID '" + name + "' because it doesn't exist.");
			name = this.DefaultCueName;
		}
		this.soundBank.PlayCue(name);
	}

	/// <inheritdoc />
	public void PlayCue(string name, AudioListener listener, AudioEmitter emitter)
	{
		this.soundBank.PlayCue(name, listener, emitter);
	}

	/// <inheritdoc />
	public void Dispose()
	{
		this.soundBank.Dispose();
	}

	/// <inheritdoc />
	public void AddCue(CueDefinition definition)
	{
		this.soundBank.AddCue(definition);
	}

	/// <inheritdoc />
	public bool Exists(string name)
	{
		return this.soundBank.Exists(name);
	}

	/// <inheritdoc />
	public CueDefinition GetCueDefinition(string name)
	{
		return this.soundBank.GetCueDefinition(name);
	}
}
