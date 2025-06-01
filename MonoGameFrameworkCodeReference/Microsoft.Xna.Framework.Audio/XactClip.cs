using System;
using System.Collections.Generic;
using System.IO;

namespace Microsoft.Xna.Framework.Audio;

public class XactClip
{
	public float DefaultVolume;

	public ClipEvent[] clipEvents;

	public bool FilterEnabled;

	public FilterMode FilterMode;

	public float FilterQ;

	public ushort FilterFrequency;

	internal readonly bool UseReverb;

	public XactClip(List<PlayWaveVariant> variants, bool loop, bool use_reverb)
	{
		this.clipEvents = new ClipEvent[1];
		PlayWaveEvent play_wave_event = new PlayWaveEvent(this, variants, 0f, 0f, loop)
		{
			variationType = VariationType.Random
		};
		this.clipEvents[0] = play_wave_event;
		this.DefaultVolume = 1f;
		this.UseReverb = use_reverb;
	}

	public XactClip(SoundBank soundBank, BinaryReader clipReader, bool use_reverb)
	{
		this.UseReverb = use_reverb;
		float volume_db = XactHelpers.ParseDecibels(clipReader.ReadByte());
		this.DefaultVolume = XactHelpers.ParseVolumeFromDecibels(volume_db);
		uint clip_offset = clipReader.ReadUInt32();
		ushort filter_q_and_flags = clipReader.ReadUInt16();
		this.FilterEnabled = (filter_q_and_flags & 1) == 1;
		this.FilterMode = (FilterMode)((filter_q_and_flags >> 1) & 3);
		this.FilterQ = (float)(filter_q_and_flags >> 3) * 0.01f;
		this.FilterFrequency = clipReader.ReadUInt16();
		long old_position = clipReader.BaseStream.Position;
		clipReader.BaseStream.Seek(clip_offset, SeekOrigin.Begin);
		byte num_events = clipReader.ReadByte();
		this.clipEvents = new ClipEvent[num_events];
		for (int i = 0; i < num_events; i++)
		{
			uint num = clipReader.ReadUInt32();
			float randomOffset = (float)(int)clipReader.ReadUInt16() * 0.001f;
			uint eventId = num & 0x1F;
			float timeStamp = (float)((num >> 5) & 0xFFFF) * 0.001f;
			switch (eventId)
			{
			case 0u:
				throw new NotImplementedException("Stop event");
			case 1u:
			{
				clipReader.ReadByte();
				clipReader.ReadByte();
				int trackIndex = clipReader.ReadUInt16();
				int waveBankIndex = clipReader.ReadByte();
				byte loopCount2 = clipReader.ReadByte();
				_ = (float)(int)clipReader.ReadUInt16() / 100f;
				_ = (float)(int)clipReader.ReadUInt16() / 100f;
				this.clipEvents[i] = new PlayWaveEvent(this, timeStamp, randomOffset, soundBank, new int[1] { waveBankIndex }, new int[1] { trackIndex }, null, 0, VariationType.Ordered, null, null, null, loopCount2, newWaveOnLoop: false);
				break;
			}
			case 3u:
			{
				clipReader.ReadByte();
				clipReader.ReadByte();
				byte loopCount = clipReader.ReadByte();
				_ = (float)(int)clipReader.ReadUInt16() / 100f;
				_ = (float)(int)clipReader.ReadUInt16() / 100f;
				ushort numTracks = clipReader.ReadUInt16();
				byte num2 = clipReader.ReadByte();
				bool newWaveOnLoop = (num2 & 0x40) == 64;
				VariationType variationType = (VariationType)(num2 & 0xF);
				clipReader.ReadBytes(5);
				int[] waveBanks = new int[numTracks];
				int[] tracks = new int[numTracks];
				byte[] weights = new byte[numTracks];
				int totalWeights = 0;
				for (int j = 0; j < numTracks; j++)
				{
					tracks[j] = clipReader.ReadUInt16();
					waveBanks[j] = clipReader.ReadByte();
					byte minWeight = clipReader.ReadByte();
					byte maxWeight = clipReader.ReadByte();
					weights[j] = (byte)(maxWeight - minWeight);
					totalWeights += weights[j];
				}
				this.clipEvents[i] = new PlayWaveEvent(this, timeStamp, randomOffset, soundBank, waveBanks, tracks, weights, totalWeights, variationType, null, null, null, loopCount, newWaveOnLoop);
				break;
			}
			case 4u:
			{
				clipReader.ReadByte();
				clipReader.ReadByte();
				int trackIndex2 = clipReader.ReadUInt16();
				int waveBankIndex2 = clipReader.ReadByte();
				byte loopCount4 = clipReader.ReadByte();
				_ = (float)(int)clipReader.ReadUInt16() / 100f;
				_ = (float)(int)clipReader.ReadUInt16() / 100f;
				float minPitch2 = (float)clipReader.ReadInt16() / 1200f;
				float maxPitch2 = (float)clipReader.ReadInt16() / 1200f;
				float minVolume2 = XactHelpers.ParseVolumeFromDecibels(clipReader.ReadByte());
				float maxVolume2 = XactHelpers.ParseVolumeFromDecibels(clipReader.ReadByte());
				float minFrequency2 = clipReader.ReadSingle();
				float maxFrequency2 = clipReader.ReadSingle();
				float minQ2 = clipReader.ReadSingle();
				float maxQ2 = clipReader.ReadSingle();
				clipReader.ReadByte();
				byte num5 = clipReader.ReadByte();
				Vector2? pitchVar2 = null;
				if ((num5 & 0x10) == 16)
				{
					pitchVar2 = new Vector2(minPitch2, maxPitch2 - minPitch2);
				}
				Vector2? volumeVar2 = null;
				if ((num5 & 0x20) == 32)
				{
					volumeVar2 = new Vector2(minVolume2, maxVolume2 - minVolume2);
				}
				Vector4? filterVar2 = null;
				if ((num5 & 0x40) == 64)
				{
					filterVar2 = new Vector4(minFrequency2, maxFrequency2 - minFrequency2, minQ2, maxQ2 - minQ2);
				}
				this.clipEvents[i] = new PlayWaveEvent(this, timeStamp, randomOffset, soundBank, new int[1] { waveBankIndex2 }, new int[1] { trackIndex2 }, null, 0, VariationType.Ordered, volumeVar2, pitchVar2, filterVar2, loopCount4, newWaveOnLoop: false);
				break;
			}
			case 6u:
			{
				clipReader.ReadByte();
				clipReader.ReadByte();
				byte loopCount3 = clipReader.ReadByte();
				_ = (float)(int)clipReader.ReadUInt16() / 100f;
				_ = (float)(int)clipReader.ReadUInt16() / 100f;
				float minPitch = (float)clipReader.ReadInt16() / 1200f;
				float maxPitch = (float)clipReader.ReadInt16() / 1200f;
				float minVolume = XactHelpers.ParseVolumeFromDecibels(clipReader.ReadByte());
				float maxVolume = XactHelpers.ParseVolumeFromDecibels(clipReader.ReadByte());
				float minFrequency = clipReader.ReadSingle();
				float maxFrequency = clipReader.ReadSingle();
				float minQ = clipReader.ReadSingle();
				float maxQ = clipReader.ReadSingle();
				clipReader.ReadByte();
				byte num3 = clipReader.ReadByte();
				Vector2? pitchVar = null;
				if ((num3 & 0x10) == 16)
				{
					pitchVar = new Vector2(minPitch, maxPitch - minPitch);
				}
				Vector2? volumeVar = null;
				if ((num3 & 0x20) == 32)
				{
					volumeVar = new Vector2(minVolume, maxVolume - minVolume);
				}
				Vector4? filterVar = null;
				if ((num3 & 0x40) == 64)
				{
					filterVar = new Vector4(minFrequency, maxFrequency - minFrequency, minQ, maxQ - minQ);
				}
				ushort numTracks2 = clipReader.ReadUInt16();
				byte num4 = clipReader.ReadByte();
				bool newWaveOnLoop2 = (num4 & 0x40) == 64;
				VariationType variationType2 = (VariationType)(num4 & 0xF);
				clipReader.ReadBytes(5);
				int[] waveBanks2 = new int[numTracks2];
				int[] tracks2 = new int[numTracks2];
				byte[] weights2 = new byte[numTracks2];
				int totalWeights2 = 0;
				for (int k = 0; k < numTracks2; k++)
				{
					tracks2[k] = clipReader.ReadUInt16();
					waveBanks2[k] = clipReader.ReadByte();
					byte minWeight2 = clipReader.ReadByte();
					byte maxWeight2 = clipReader.ReadByte();
					weights2[k] = (byte)(maxWeight2 - minWeight2);
					totalWeights2 += weights2[k];
				}
				this.clipEvents[i] = new PlayWaveEvent(this, timeStamp, randomOffset, soundBank, waveBanks2, tracks2, weights2, totalWeights2, variationType2, volumeVar, pitchVar, filterVar, loopCount3, newWaveOnLoop2);
				break;
			}
			case 7u:
				throw new NotImplementedException("Pitch event");
			case 8u:
			{
				clipReader.ReadBytes(2);
				bool isAdd = (clipReader.ReadByte() & 1) == 1;
				float volume = XactHelpers.ParseVolumeFromDecibels(clipReader.ReadSingle() / 100f + (isAdd ? volume_db : 0f));
				clipReader.ReadBytes(9);
				this.clipEvents[i] = new VolumeEvent(this, timeStamp, randomOffset, volume);
				break;
			}
			case 17u:
				throw new NotImplementedException("Volume repeat event");
			case 9u:
				throw new NotImplementedException("Marker event");
			default:
				throw new NotSupportedException("Unknown event " + eventId);
			}
		}
		clipReader.BaseStream.Seek(old_position, SeekOrigin.Begin);
	}

	internal void Update(Cue cue, float old_time, float new_time)
	{
		for (int i = 0; i < this.clipEvents.Length; i++)
		{
			ClipEvent current_event = this.clipEvents[i];
			if (new_time >= current_event.TimeStamp && old_time < current_event.TimeStamp)
			{
				current_event.Fire(cue);
			}
		}
	}
}
