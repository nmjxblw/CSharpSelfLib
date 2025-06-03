using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Microsoft.Xna.Framework.Audio;

/// <summary>Represents a collection of Cues.</summary>
public class SoundBank : IDisposable
{
	private readonly AudioEngine _audioengine;

	private readonly string[] _waveBankNames;

	private readonly WaveBank[] _waveBanks;

	private readonly float[] defaultProbability = new float[1] { 1f };

	private readonly Dictionary<string, CueDefinition> _cues = new Dictionary<string, CueDefinition>();

	/// <summary>
	/// Is true if the SoundBank has any live Cues in use.
	/// </summary>
	public bool IsInUse { get; private set; }

	/// <summary>
	/// Is true if the SoundBank has been disposed.
	/// </summary>
	public bool IsDisposed { get; private set; }

	/// <summary>
	/// This event is triggered when the SoundBank is disposed.
	/// </summary>
	public event EventHandler<EventArgs> Disposing;

	/// <param name="audioEngine">AudioEngine that will be associated with this sound bank.</param>
	/// <param name="fileName">Path to a .xsb SoundBank file.</param>
	public SoundBank(AudioEngine audioEngine, string fileName)
	{
		if (audioEngine == null)
		{
			throw new ArgumentNullException("audioEngine");
		}
		if (string.IsNullOrEmpty(fileName))
		{
			throw new ArgumentNullException("fileName");
		}
		this._audioengine = audioEngine;
		using Stream stream = AudioEngine.OpenStream(fileName, useMemoryStream: true);
		using BinaryReader reader = new BinaryReader(stream);
		if (reader.ReadUInt32() != 1262634067)
		{
			throw new Exception("Bad soundbank format");
		}
		reader.ReadUInt16();
		reader.ReadUInt16();
		_ = 43;
		reader.ReadUInt16();
		reader.ReadUInt32();
		reader.ReadUInt32();
		reader.ReadByte();
		uint numSimpleCues = reader.ReadUInt16();
		uint numComplexCues = reader.ReadUInt16();
		reader.ReadUInt16();
		reader.ReadUInt16();
		uint numWaveBanks = reader.ReadByte();
		reader.ReadUInt16();
		uint cueNameTableLen = reader.ReadUInt16();
		reader.ReadUInt16();
		uint simpleCuesOffset = reader.ReadUInt32();
		uint complexCuesOffset = reader.ReadUInt32();
		uint cueNamesOffset = reader.ReadUInt32();
		reader.ReadUInt32();
		reader.ReadUInt32();
		reader.ReadUInt32();
		uint waveBankNameTableOffset = reader.ReadUInt32();
		reader.ReadUInt32();
		reader.ReadUInt32();
		reader.ReadUInt32();
		stream.Seek(waveBankNameTableOffset, SeekOrigin.Begin);
		this._waveBanks = new WaveBank[numWaveBanks];
		this._waveBankNames = new string[numWaveBanks];
		for (int i = 0; i < numWaveBanks; i++)
		{
			this._waveBankNames[i] = Encoding.UTF8.GetString(reader.ReadBytes(64), 0, 64).Replace("\0", "");
		}
		stream.Seek(cueNamesOffset, SeekOrigin.Begin);
		string[] cue_names = Encoding.UTF8.GetString(reader.ReadBytes((int)cueNameTableLen), 0, (int)cueNameTableLen).Split('\0');
		if (numSimpleCues != 0)
		{
			stream.Seek(simpleCuesOffset, SeekOrigin.Begin);
			for (int j = 0; j < numSimpleCues; j++)
			{
				CueDefinition cue = new CueDefinition();
				reader.ReadByte();
				uint soundOffset = reader.ReadUInt32();
				long oldPosition = stream.Position;
				stream.Seek(soundOffset, SeekOrigin.Begin);
				XactSoundBankSound sound = new XactSoundBankSound(audioEngine, this, reader);
				stream.Seek(oldPosition, SeekOrigin.Begin);
				cue.sounds.Clear();
				cue.sounds.Add(sound);
				cue.name = cue_names[j];
				this._cues[cue_names[j]] = cue;
			}
		}
		if (numComplexCues == 0)
		{
			return;
		}
		stream.Seek(complexCuesOffset, SeekOrigin.Begin);
		for (int k = 0; k < numComplexCues; k++)
		{
			CueDefinition cue2 = new CueDefinition();
			if (((reader.ReadByte() >> 2) & 1) != 0)
			{
				uint soundOffset2 = reader.ReadUInt32();
				reader.ReadUInt32();
				long oldPosition2 = stream.Position;
				stream.Seek(soundOffset2, SeekOrigin.Begin);
				XactSoundBankSound sound2 = new XactSoundBankSound(audioEngine, this, reader);
				stream.Seek(oldPosition2, SeekOrigin.Begin);
				cue2.sounds.Clear();
				cue2.sounds.Add(sound2);
			}
			else
			{
				uint variationTableOffset = reader.ReadUInt32();
				reader.ReadUInt32();
				long savepos = stream.Position;
				stream.Seek(variationTableOffset, SeekOrigin.Begin);
				uint numEntries = reader.ReadUInt16();
				ushort num = reader.ReadUInt16();
				reader.ReadByte();
				reader.ReadUInt16();
				reader.ReadByte();
				List<XactSoundBankSound> cue_sounds = new List<XactSoundBankSound>();
				_ = new float[numEntries];
				uint tableType = (uint)((num >>> 3) & 7);
				for (int l = 0; l < numEntries; l++)
				{
					switch (tableType)
					{
					case 0u:
					{
						int trackIndex2 = reader.ReadUInt16();
						int waveBankIndex2 = reader.ReadByte();
						reader.ReadByte();
						reader.ReadByte();
						cue_sounds.Add(new XactSoundBankSound(this, waveBankIndex2, trackIndex2));
						break;
					}
					case 1u:
					{
						uint soundOffset4 = reader.ReadUInt32();
						reader.ReadByte();
						reader.ReadByte();
						long oldPosition4 = stream.Position;
						stream.Seek(soundOffset4, SeekOrigin.Begin);
						cue_sounds.Add(new XactSoundBankSound(audioEngine, this, reader));
						stream.Seek(oldPosition4, SeekOrigin.Begin);
						break;
					}
					case 3u:
					{
						uint soundOffset3 = reader.ReadUInt32();
						reader.ReadSingle();
						reader.ReadSingle();
						reader.ReadUInt32();
						long oldPosition3 = stream.Position;
						stream.Seek(soundOffset3, SeekOrigin.Begin);
						cue_sounds.Add(new XactSoundBankSound(audioEngine, this, reader));
						stream.Seek(oldPosition3, SeekOrigin.Begin);
						break;
					}
					case 4u:
					{
						int trackIndex = reader.ReadUInt16();
						int waveBankIndex = reader.ReadByte();
						cue_sounds.Add(new XactSoundBankSound(this, waveBankIndex, trackIndex));
						break;
					}
					default:
						throw new NotSupportedException();
					}
				}
				stream.Seek(savepos, SeekOrigin.Begin);
				cue2.sounds = cue_sounds;
			}
			cue2.instanceLimit = reader.ReadByte();
			reader.ReadUInt16();
			reader.ReadUInt16();
			cue2.limitBehavior = (CueDefinition.LimitBehavior)(reader.ReadByte() >> 3);
			cue2.name = cue_names[numSimpleCues + k];
			this._cues[cue_names[numSimpleCues + k]] = cue2;
		}
	}

	public SoundEffect GetSoundEffect(int waveBankIndex, int trackIndex)
	{
		WaveBank waveBank = this._waveBanks[waveBankIndex];
		if (waveBank == null)
		{
			string name = this._waveBankNames[waveBankIndex];
			if (!this._audioengine.Wavebanks.TryGetValue(name, out waveBank))
			{
				throw new Exception("The wave bank '" + name + "' was not found!");
			}
			this._waveBanks[waveBankIndex] = waveBank;
		}
		return waveBank.GetSoundEffect(trackIndex);
	}

	public SoundEffectInstance GetSoundEffectInstance(int waveBankIndex, int trackIndex, out bool streaming)
	{
		WaveBank waveBank = this._waveBanks[waveBankIndex];
		if (waveBank == null)
		{
			string name = this._waveBankNames[waveBankIndex];
			if (!this._audioengine.Wavebanks.TryGetValue(name, out waveBank))
			{
				throw new Exception("The wave bank '" + name + "' was not found!");
			}
			this._waveBanks[waveBankIndex] = waveBank;
		}
		return waveBank.GetSoundEffectInstance(trackIndex, out streaming);
	}

	public bool Exists(string name)
	{
		if (string.IsNullOrEmpty(name))
		{
			throw new ArgumentNullException("name");
		}
		return this._cues.ContainsKey(name);
	}

	public CueDefinition GetCueDefinition(string name)
	{
		if (string.IsNullOrEmpty(name))
		{
			throw new ArgumentNullException("name");
		}
		return this.RequireCueDefinition(name);
	}

	/// <summary>
	/// Returns a pooled Cue object.
	/// </summary>
	/// <param name="name">Friendly name of the cue to get.</param>
	/// <returns>a unique Cue object from a pool.</returns>
	/// <remarks>
	/// <para>Cue instances are unique, even when sharing the same name. This allows multiple instances to simultaneously play.</para>
	/// </remarks>
	public Cue GetCue(string name)
	{
		if (string.IsNullOrEmpty(name))
		{
			throw new ArgumentNullException("name");
		}
		CueDefinition cue_definition = this.RequireCueDefinition(name);
		this.IsInUse = true;
		Cue cue = new Cue(this._audioengine, cue_definition);
		cue.Prepare();
		return cue;
	}

	/// <summary>
	/// Plays a cue.
	/// </summary>
	/// <param name="name">Name of the cue to play.</param>
	public void PlayCue(string name)
	{
		if (string.IsNullOrEmpty(name))
		{
			throw new ArgumentNullException("name");
		}
		CueDefinition cue_definition = this.RequireCueDefinition(name);
		this.IsInUse = true;
		Cue cue = new Cue(this._audioengine, cue_definition);
		cue.Prepare();
		cue.Play();
	}

	/// <summary>
	/// Plays a cue with static 3D positional information.
	/// </summary>
	/// <remarks>
	/// Commonly used for short lived effects.  To dynamically change the 3D 
	/// positional information on a cue over time use <see cref="M:Microsoft.Xna.Framework.Audio.SoundBank.GetCue(System.String)" /> and <see cref="M:Microsoft.Xna.Framework.Audio.Cue.Apply3D(Microsoft.Xna.Framework.Audio.AudioListener,Microsoft.Xna.Framework.Audio.AudioEmitter)" />.</remarks>
	/// <param name="name">The name of the cue to play.</param>
	/// <param name="listener">The listener state.</param>
	/// <param name="emitter">The cue emitter state.</param>
	public void PlayCue(string name, AudioListener listener, AudioEmitter emitter)
	{
		if (string.IsNullOrEmpty(name))
		{
			throw new ArgumentNullException("name");
		}
		CueDefinition cue_definition = this.RequireCueDefinition(name);
		this.IsInUse = true;
		Cue cue = new Cue(this._audioengine, cue_definition);
		cue.Prepare();
		cue.Apply3D(listener, emitter);
		cue.Play();
	}

	/// <summary>
	/// Disposes the SoundBank.
	/// </summary>
	public void Dispose()
	{
		this.Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	~SoundBank()
	{
		this.Dispose(disposing: false);
	}

	private void Dispose(bool disposing)
	{
		if (!this.IsDisposed)
		{
			this.IsDisposed = true;
			if (disposing)
			{
				this.IsInUse = false;
				EventHelpers.Raise(this, this.Disposing, EventArgs.Empty);
			}
		}
	}

	public void AddCue(CueDefinition cue_definition)
	{
		this._cues[cue_definition.name] = cue_definition;
	}

	/// <summary>Get a cue definition if it exists, else throw an exception.</summary>
	/// <param name="name">The cue name to load.</param>
	/// <exception cref="T:System.ArgumentException">There's no cue definition matching <paramref name="name" />.</exception>
	private CueDefinition RequireCueDefinition(string name)
	{
		if (!this._cues.TryGetValue(name, out var cueDefinition))
		{
			throw new ArgumentException("There's no audio cue with ID '" + name + "'.", "name");
		}
		return cueDefinition;
	}
}
