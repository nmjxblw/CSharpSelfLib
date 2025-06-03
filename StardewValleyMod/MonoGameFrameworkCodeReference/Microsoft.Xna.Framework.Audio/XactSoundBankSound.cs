using System.Collections.Generic;
using System.IO;

namespace Microsoft.Xna.Framework.Audio;

public class XactSoundBankSound
{
	public bool complexSound;

	public XactClip[] soundClips;

	public readonly int waveBankIndex;

	public readonly int trackIndex;

	public float volume = 1f;

	public float pitch;

	public uint categoryID;

	public SoundBank soundBank;

	public bool useReverb;

	public int[] rpcCurves;

	public XactSoundBankSound(SoundEffect[] sound_effects, int category_id, bool loop = false, bool use_reverb = false)
	{
		List<PlayWaveVariant> variants = new List<PlayWaveVariant>();
		foreach (SoundEffect sound_effect in sound_effects)
		{
			variants.Add(new PlayWaveVariant
			{
				overrideSoundEffect = sound_effect
			});
		}
		this.complexSound = true;
		this.soundClips = new XactClip[1];
		this.rpcCurves = new int[0];
		this.categoryID = (uint)category_id;
		this.useReverb = use_reverb;
		this.soundClips[0] = new XactClip(variants, loop, this.useReverb);
	}

	public XactSoundBankSound(List<PlayWaveVariant> variants, int category_id, bool loop = false, bool use_reverb = false)
	{
		this.complexSound = true;
		this.soundClips = new XactClip[1];
		this.rpcCurves = new int[0];
		this.categoryID = (uint)category_id;
		this.useReverb = use_reverb;
		this.soundClips[0] = new XactClip(variants, loop, this.useReverb);
	}

	public XactSoundBankSound(SoundBank soundBank, int waveBankIndex, int trackIndex)
	{
		this.complexSound = false;
		this.soundBank = soundBank;
		this.waveBankIndex = waveBankIndex;
		this.trackIndex = trackIndex;
		this.rpcCurves = new int[0];
	}

	public XactSoundBankSound(AudioEngine engine, SoundBank soundBank, BinaryReader soundReader)
	{
		this.soundBank = soundBank;
		byte flags = soundReader.ReadByte();
		this.complexSound = (flags & 1) != 0;
		bool num = (flags & 0xE) != 0;
		bool hasDSPs = (flags & 0x10) != 0;
		this.categoryID = soundReader.ReadUInt16();
		this.volume = XactHelpers.ParseVolumeFromDecibels(soundReader.ReadByte());
		this.pitch = (float)soundReader.ReadInt16() / 1200f;
		soundReader.ReadByte();
		soundReader.ReadUInt16();
		int numClips = 0;
		if (this.complexSound)
		{
			numClips = soundReader.ReadByte();
		}
		else
		{
			this.trackIndex = soundReader.ReadUInt16();
			this.waveBankIndex = soundReader.ReadByte();
		}
		if (!num)
		{
			this.rpcCurves = new int[0];
		}
		else
		{
			long current = soundReader.BaseStream.Position;
			ushort dataLength = soundReader.ReadUInt16();
			byte numPresets = soundReader.ReadByte();
			this.rpcCurves = new int[numPresets];
			for (int i = 0; i < numPresets; i++)
			{
				this.rpcCurves[i] = engine.GetRpcIndex(soundReader.ReadUInt32());
			}
			soundReader.BaseStream.Seek(current + dataLength, SeekOrigin.Begin);
		}
		if (!hasDSPs)
		{
			this.useReverb = false;
		}
		else
		{
			this.useReverb = true;
			soundReader.BaseStream.Seek(7L, SeekOrigin.Current);
		}
		if (this.complexSound)
		{
			this.soundClips = new XactClip[numClips];
			for (int j = 0; j < numClips; j++)
			{
				this.soundClips[j] = new XactClip(soundBank, soundReader, this.useReverb);
			}
		}
	}

	public SoundEffectInstance GetSimpleSoundInstance()
	{
		if (this.complexSound)
		{
			return null;
		}
		bool streaming;
		return this.soundBank.GetSoundEffectInstance(this.waveBankIndex, this.trackIndex, out streaming);
	}
}
