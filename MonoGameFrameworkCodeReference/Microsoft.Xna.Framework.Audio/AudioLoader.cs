using System;
using System.IO;
using System.Runtime.InteropServices;
using MonoGame.OpenAL;

namespace Microsoft.Xna.Framework.Audio;

internal static class AudioLoader
{
	private struct ImaState
	{
		public int predictor;

		public int stepIndex;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	private struct MsAdpcmState
	{
		public int coeff1;

		public int coeff2;

		public int delta;

		public int sample1;

		public int sample2;
	}

	internal const int FormatPcm = 1;

	internal const int FormatMsAdpcm = 2;

	internal const int FormatIeee = 3;

	internal const int FormatIma4 = 17;

	private static int[] stepTable = new int[89]
	{
		7, 8, 9, 10, 11, 12, 13, 14, 16, 17,
		19, 21, 23, 25, 28, 31, 34, 37, 41, 45,
		50, 55, 60, 66, 73, 80, 88, 97, 107, 118,
		130, 143, 157, 173, 190, 209, 230, 253, 279, 307,
		337, 371, 408, 449, 494, 544, 598, 658, 724, 796,
		876, 963, 1060, 1166, 1282, 1411, 1552, 1707, 1878, 2066,
		2272, 2499, 2749, 3024, 3327, 3660, 4026, 4428, 4871, 5358,
		5894, 6484, 7132, 7845, 8630, 9493, 10442, 11487, 12635, 13899,
		15289, 16818, 18500, 20350, 22385, 24623, 27086, 29794, 32767
	};

	private static int[] indexTable = new int[16]
	{
		-1, -1, -1, -1, 2, 4, 6, 8, -1, -1,
		-1, -1, 2, 4, 6, 8
	};

	private static readonly int[] ADAPTATION_TABLE = new int[16]
	{
		230, 230, 230, 230, 307, 409, 512, 614, 768, 614,
		512, 409, 307, 230, 230, 230
	};

	private static readonly int[] ADAPTATION_COEFF1 = new int[7] { 256, 512, 0, 192, 240, 460, 392 };

	private static readonly int[] ADAPTATION_COEFF2 = new int[7] { 0, -256, 0, 64, 0, -208, -232 };

	public static ALFormat GetSoundFormat(int format, int channels, int bits)
	{
		switch (format)
		{
		case 1:
			switch (channels)
			{
			case 1:
				if (bits != 8)
				{
					return ALFormat.Mono16;
				}
				return ALFormat.Mono8;
			case 2:
				if (bits != 8)
				{
					return ALFormat.Stereo16;
				}
				return ALFormat.Stereo8;
			default:
				throw new NotSupportedException("The specified channel count is not supported.");
			}
		case 2:
			return channels switch
			{
				1 => ALFormat.MonoMSAdpcm, 
				2 => ALFormat.StereoMSAdpcm, 
				_ => throw new NotSupportedException("The specified channel count is not supported."), 
			};
		case 3:
			return channels switch
			{
				1 => ALFormat.MonoFloat32, 
				2 => ALFormat.StereoFloat32, 
				_ => throw new NotSupportedException("The specified channel count is not supported."), 
			};
		case 17:
			return channels switch
			{
				1 => ALFormat.MonoIma4, 
				2 => ALFormat.StereoIma4, 
				_ => throw new NotSupportedException("The specified channel count is not supported."), 
			};
		default:
			throw new NotSupportedException("The specified sound format (" + format + ") is not supported.");
		}
	}

	public static int SampleAlignment(ALFormat format, int blockAlignment)
	{
		return format switch
		{
			ALFormat.MonoIma4 => (blockAlignment - 4) / 4 * 8 + 1, 
			ALFormat.StereoIma4 => (blockAlignment / 2 - 4) / 4 * 8 + 1, 
			ALFormat.MonoMSAdpcm => (blockAlignment - 7) * 2 + 2, 
			ALFormat.StereoMSAdpcm => (blockAlignment / 2 - 7) * 2 + 2, 
			_ => 0, 
		};
	}

	/// <summary>
	/// Load a WAV file from stream.
	/// </summary>
	/// <param name="stream">The stream positioned at the start of the WAV file.</param>
	/// <param name="format">Gets the OpenAL format enumeration value.</param>
	/// <param name="frequency">Gets the frequency or sample rate.</param>
	/// <param name="channels">Gets the number of channels.</param>
	/// <param name="blockAlignment">Gets the block alignment, important for compressed sounds.</param>
	/// <param name="bitsPerSample">Gets the number of bits per sample.</param>
	/// <param name="samplesPerBlock">Gets the number of samples per block.</param>
	/// <param name="sampleCount">Gets the total number of samples.</param>
	/// <returns>The byte buffer containing the waveform data or compressed blocks.</returns>
	public static byte[] Load(Stream stream, out ALFormat format, out int frequency, out int channels, out int blockAlignment, out int bitsPerSample, out int samplesPerBlock, out int sampleCount)
	{
		byte[] audioData = null;
		using BinaryReader reader = new BinaryReader(stream);
		reader.BaseStream.Seek(0L, SeekOrigin.Begin);
		return AudioLoader.LoadWave(reader, out format, out frequency, out channels, out blockAlignment, out bitsPerSample, out samplesPerBlock, out sampleCount);
	}

	private static byte[] LoadWave(BinaryReader reader, out ALFormat format, out int frequency, out int channels, out int blockAlignment, out int bitsPerSample, out int samplesPerBlock, out int sampleCount)
	{
		byte[] audioData = null;
		if (new string(reader.ReadChars(4)) != "RIFF")
		{
			throw new ArgumentException("Specified stream is not a wave file.");
		}
		reader.ReadInt32();
		if (new string(reader.ReadChars(4)) != "WAVE")
		{
			throw new ArgumentException("Specified stream is not a wave file.");
		}
		int audioFormat = 0;
		channels = 0;
		bitsPerSample = 0;
		format = ALFormat.Mono16;
		frequency = 0;
		blockAlignment = 0;
		samplesPerBlock = 0;
		sampleCount = 0;
		while (audioData == null)
		{
			string chunkType = new string(reader.ReadChars(4));
			int chunkSize = reader.ReadInt32();
			switch (chunkType)
			{
			case "fmt ":
			{
				audioFormat = reader.ReadInt16();
				channels = reader.ReadInt16();
				frequency = reader.ReadInt32();
				reader.ReadInt32();
				blockAlignment = reader.ReadInt16();
				bitsPerSample = reader.ReadInt16();
				if (chunkSize <= 16)
				{
					break;
				}
				int extraDataSize = reader.ReadInt16();
				if (audioFormat == 17)
				{
					samplesPerBlock = reader.ReadInt16();
					extraDataSize -= 2;
				}
				if (extraDataSize <= 0)
				{
					break;
				}
				if (reader.BaseStream.CanSeek)
				{
					reader.BaseStream.Seek(extraDataSize, SeekOrigin.Current);
					break;
				}
				for (int k = 0; k < extraDataSize; k++)
				{
					reader.ReadByte();
				}
				break;
			}
			case "fact":
			{
				if (audioFormat == 17)
				{
					sampleCount = reader.ReadInt32() * channels;
					chunkSize -= 4;
				}
				if (chunkSize <= 0)
				{
					break;
				}
				if (reader.BaseStream.CanSeek)
				{
					reader.BaseStream.Seek(chunkSize, SeekOrigin.Current);
					break;
				}
				for (int j = 0; j < chunkSize; j++)
				{
					reader.ReadByte();
				}
				break;
			}
			case "data":
				audioData = reader.ReadBytes(chunkSize);
				break;
			default:
			{
				if (reader.BaseStream.CanSeek)
				{
					reader.BaseStream.Seek(chunkSize, SeekOrigin.Current);
					break;
				}
				for (int i = 0; i < chunkSize; i++)
				{
					reader.ReadByte();
				}
				break;
			}
			}
		}
		format = AudioLoader.GetSoundFormat(audioFormat, channels, bitsPerSample);
		if (samplesPerBlock == 0)
		{
			samplesPerBlock = AudioLoader.SampleAlignment(format, blockAlignment);
		}
		if (sampleCount == 0)
		{
			switch (audioFormat)
			{
			case 2:
			case 17:
				sampleCount = audioData.Length / blockAlignment * samplesPerBlock + AudioLoader.SampleAlignment(format, audioData.Length % blockAlignment);
				break;
			case 1:
			case 3:
				sampleCount = audioData.Length / (channels * bitsPerSample / 8);
				break;
			default:
				throw new InvalidDataException("Unhandled WAV format " + format);
			}
		}
		return audioData;
	}

	internal unsafe static byte[] Convert24To16(byte[] data, int offset, int count)
	{
		if (offset + count > data.Length || count % 3 != 0)
		{
			throw new ArgumentException("Invalid 24-bit PCM data received");
		}
		int sampleCount = count / 3;
		byte[] outData = new byte[sampleCount * 2];
		fixed (byte* src = &data[offset])
		{
			fixed (byte* dst = &outData[0])
			{
				int srcIndex = 0;
				int dstIndex = 0;
				for (int i = 0; i < sampleCount; i++)
				{
					dst[dstIndex] = src[srcIndex + 1];
					dst[dstIndex + 1] = src[srcIndex + 2];
					dstIndex += 2;
					srcIndex += 3;
				}
			}
		}
		return outData;
	}

	internal unsafe static byte[] ConvertFloatTo16(byte[] data, int offset, int count)
	{
		if (offset + count > data.Length || count % 4 != 0)
		{
			throw new ArgumentException("Invalid 32-bit float PCM data received");
		}
		int sampleCount = count / 4;
		byte[] outData = new byte[sampleCount * 2];
		fixed (byte* ptr = &data[offset])
		{
			float* f = (float*)ptr;
			fixed (byte* ptr2 = &outData[0])
			{
				byte* d = ptr2;
				for (int i = 0; i < sampleCount; i++)
				{
					short s = (short)(*f * 32767f);
					*(d++) = (byte)(s & 0xFF);
					*(d++) = (byte)(s >> 8);
					f++;
				}
			}
		}
		return outData;
	}

	private static int AdpcmImaWavExpandNibble(ref ImaState channel, int nibble)
	{
		int diff = AudioLoader.stepTable[channel.stepIndex] >> 3;
		if ((nibble & 4) != 0)
		{
			diff += AudioLoader.stepTable[channel.stepIndex];
		}
		if ((nibble & 2) != 0)
		{
			diff += AudioLoader.stepTable[channel.stepIndex] >> 1;
		}
		if ((nibble & 1) != 0)
		{
			diff += AudioLoader.stepTable[channel.stepIndex] >> 2;
		}
		if ((nibble & 8) != 0)
		{
			channel.predictor -= diff;
		}
		else
		{
			channel.predictor += diff;
		}
		if (channel.predictor < -32768)
		{
			channel.predictor = -32768;
		}
		else if (channel.predictor > 32767)
		{
			channel.predictor = 32767;
		}
		channel.stepIndex += AudioLoader.indexTable[nibble];
		if (channel.stepIndex < 0)
		{
			channel.stepIndex = 0;
		}
		else if (channel.stepIndex > 88)
		{
			channel.stepIndex = 88;
		}
		return channel.predictor;
	}

	internal static byte[] ConvertIma4ToPcm(byte[] buffer, int offset, int count, int channels, int blockAlignment)
	{
		ImaState channel0 = default(ImaState);
		ImaState channel1 = default(ImaState);
		int sampleCountFullBlock = (blockAlignment / channels - 4) / 4 * 8 + 1;
		int sampleCountLastBlock = 0;
		if (count % blockAlignment > 0)
		{
			sampleCountLastBlock = (count % blockAlignment / channels - 4) / 4 * 8 + 1;
		}
		byte[] samples = new byte[(count / blockAlignment * sampleCountFullBlock + sampleCountLastBlock) * 2 * channels];
		int sampleOffset = 0;
		while (count > 0)
		{
			int blockSize = blockAlignment;
			if (count < blockSize)
			{
				blockSize = count;
			}
			count -= blockAlignment;
			channel0.predictor = buffer[offset++];
			channel0.predictor |= buffer[offset++] << 8;
			if ((channel0.predictor & 0x8000) != 0)
			{
				channel0.predictor -= 65536;
			}
			channel0.stepIndex = buffer[offset++];
			if (channel0.stepIndex > 88)
			{
				channel0.stepIndex = 88;
			}
			offset++;
			int index = sampleOffset * 2;
			samples[index] = (byte)channel0.predictor;
			samples[index + 1] = (byte)(channel0.predictor >> 8);
			sampleOffset++;
			if (channels == 2)
			{
				channel1.predictor = buffer[offset++];
				channel1.predictor |= buffer[offset++] << 8;
				if ((channel1.predictor & 0x8000) != 0)
				{
					channel1.predictor -= 65536;
				}
				channel1.stepIndex = buffer[offset++];
				if (channel1.stepIndex > 88)
				{
					channel1.stepIndex = 88;
				}
				offset++;
				index = sampleOffset * 2;
				samples[index] = (byte)channel1.predictor;
				samples[index + 1] = (byte)(channel1.predictor >> 8);
				sampleOffset++;
			}
			if (channels == 2)
			{
				for (int nibbles = 2 * (blockSize - 8); nibbles > 0; nibbles -= 16)
				{
					for (int i = 0; i < 4; i++)
					{
						index = (sampleOffset + i * 4) * 2;
						int sample = AudioLoader.AdpcmImaWavExpandNibble(ref channel0, buffer[offset + i] & 0xF);
						samples[index] = (byte)sample;
						samples[index + 1] = (byte)(sample >> 8);
						index = (sampleOffset + i * 4 + 2) * 2;
						sample = AudioLoader.AdpcmImaWavExpandNibble(ref channel0, buffer[offset + i] >> 4);
						samples[index] = (byte)sample;
						samples[index + 1] = (byte)(sample >> 8);
					}
					offset += 4;
					for (int j = 0; j < 4; j++)
					{
						index = (sampleOffset + j * 4 + 1) * 2;
						int sample2 = AudioLoader.AdpcmImaWavExpandNibble(ref channel1, buffer[offset + j] & 0xF);
						samples[index] = (byte)sample2;
						samples[index + 1] = (byte)(sample2 >> 8);
						index = (sampleOffset + j * 4 + 3) * 2;
						sample2 = AudioLoader.AdpcmImaWavExpandNibble(ref channel1, buffer[offset + j] >> 4);
						samples[index] = (byte)sample2;
						samples[index + 1] = (byte)(sample2 >> 8);
					}
					offset += 4;
					sampleOffset += 16;
				}
			}
			else
			{
				for (int nibbles2 = 2 * (blockSize - 4); nibbles2 > 0; nibbles2 -= 2)
				{
					index = sampleOffset * 2;
					int b = buffer[offset];
					int sample3 = AudioLoader.AdpcmImaWavExpandNibble(ref channel0, b & 0xF);
					samples[index] = (byte)sample3;
					samples[index + 1] = (byte)(sample3 >> 8);
					index += 2;
					sample3 = AudioLoader.AdpcmImaWavExpandNibble(ref channel0, b >> 4);
					samples[index] = (byte)sample3;
					samples[index + 1] = (byte)(sample3 >> 8);
					sampleOffset += 2;
					offset++;
				}
			}
		}
		return samples;
	}

	private static int AdpcmMsExpandNibbleOrig(ref MsAdpcmState channel, int nibble)
	{
		int nibbleSign = nibble - (((nibble & 8) != 0) ? 16 : 0);
		int predictor = (channel.sample1 * channel.coeff1 + channel.sample2 * channel.coeff2) / 256 + nibbleSign * channel.delta;
		if (predictor < -32768)
		{
			predictor = -32768;
		}
		else if (predictor > 32767)
		{
			predictor = 32767;
		}
		channel.sample2 = channel.sample1;
		channel.sample1 = predictor;
		channel.delta = AudioLoader.ADAPTATION_TABLE[nibble] * channel.delta / 256;
		if (channel.delta < 16)
		{
			channel.delta = 16;
		}
		return predictor;
	}

	internal static byte[] ConvertMsAdpcmToPcm(byte[] buffer, int offset, int count, int channels, int blockAlignment)
	{
		MsAdpcmState channel0 = default(MsAdpcmState);
		MsAdpcmState channel1 = default(MsAdpcmState);
		int sampleCountFullBlock = (blockAlignment / channels - 7) * 2 + 2;
		int sampleCountLastBlock = 0;
		if (count % blockAlignment > 0)
		{
			sampleCountLastBlock = (count % blockAlignment / channels - 7) * 2 + 2;
		}
		byte[] samples = new byte[(count / blockAlignment * sampleCountFullBlock + sampleCountLastBlock) * 2 * channels];
		int sampleOffset = 0;
		if (channels == 2)
		{
			while (count > 0)
			{
				int blockSize = blockAlignment;
				if (count < blockSize)
				{
					blockSize = count;
				}
				count -= blockAlignment;
				if ((blockSize / channels - 7) * 2 + 2 < 2)
				{
					break;
				}
				int offsetStart = offset;
				int blockPredictor = buffer[offset];
				offset++;
				if (blockPredictor > 6)
				{
					blockPredictor = 6;
				}
				channel0.coeff1 = AudioLoader.ADAPTATION_COEFF1[blockPredictor];
				channel0.coeff2 = AudioLoader.ADAPTATION_COEFF2[blockPredictor];
				blockPredictor = buffer[offset];
				offset++;
				if (blockPredictor > 6)
				{
					blockPredictor = 6;
				}
				channel1.coeff1 = AudioLoader.ADAPTATION_COEFF1[blockPredictor];
				channel1.coeff2 = AudioLoader.ADAPTATION_COEFF2[blockPredictor];
				channel0.delta = buffer[offset];
				channel0.delta |= buffer[offset + 1] << 8;
				if ((channel0.delta & 0x8000) != 0)
				{
					channel0.delta -= 65536;
				}
				offset += 2;
				channel1.delta = buffer[offset];
				channel1.delta |= buffer[offset + 1] << 8;
				if ((channel1.delta & 0x8000) != 0)
				{
					channel1.delta -= 65536;
				}
				offset += 2;
				channel0.sample1 = buffer[offset];
				channel0.sample1 |= buffer[offset + 1] << 8;
				if ((channel0.sample1 & 0x8000) != 0)
				{
					channel0.sample1 -= 65536;
				}
				offset += 2;
				channel1.sample1 = buffer[offset];
				channel1.sample1 |= buffer[offset + 1] << 8;
				if ((channel1.sample1 & 0x8000) != 0)
				{
					channel1.sample1 -= 65536;
				}
				offset += 2;
				channel0.sample2 = buffer[offset];
				channel0.sample2 |= buffer[offset + 1] << 8;
				if ((channel0.sample2 & 0x8000) != 0)
				{
					channel0.sample2 -= 65536;
				}
				offset += 2;
				channel1.sample2 = buffer[offset];
				channel1.sample2 |= buffer[offset + 1] << 8;
				if ((channel1.sample2 & 0x8000) != 0)
				{
					channel1.sample2 -= 65536;
				}
				offset += 2;
				samples[sampleOffset] = (byte)channel0.sample2;
				samples[sampleOffset + 1] = (byte)(channel0.sample2 >> 8);
				samples[sampleOffset + 2] = (byte)channel1.sample2;
				samples[sampleOffset + 3] = (byte)(channel1.sample2 >> 8);
				samples[sampleOffset + 4] = (byte)channel0.sample1;
				samples[sampleOffset + 5] = (byte)(channel0.sample1 >> 8);
				samples[sampleOffset + 6] = (byte)channel1.sample1;
				samples[sampleOffset + 7] = (byte)(channel1.sample1 >> 8);
				sampleOffset += 8;
				blockSize -= offset - offsetStart;
				for (int i = 0; i < blockSize; i++)
				{
					int nibbles = buffer[offset];
					int sample = AudioLoader.AdpcmMsExpandNibbleOrig(ref channel0, nibbles >> 4);
					samples[sampleOffset] = (byte)sample;
					samples[sampleOffset + 1] = (byte)(sample >> 8);
					sample = AudioLoader.AdpcmMsExpandNibbleOrig(ref channel1, nibbles & 0xF);
					samples[sampleOffset + 2] = (byte)sample;
					samples[sampleOffset + 3] = (byte)(sample >> 8);
					sampleOffset += 4;
					offset++;
				}
			}
		}
		else
		{
			while (count > 0)
			{
				int blockSize2 = blockAlignment;
				if (count < blockSize2)
				{
					blockSize2 = count;
				}
				count -= blockAlignment;
				if ((blockSize2 / channels - 7) * 2 + 2 < 2)
				{
					break;
				}
				int offsetStart2 = offset;
				int blockPredictor = buffer[offset];
				offset++;
				if (blockPredictor > 6)
				{
					blockPredictor = 6;
				}
				channel0.coeff1 = AudioLoader.ADAPTATION_COEFF1[blockPredictor];
				channel0.coeff2 = AudioLoader.ADAPTATION_COEFF2[blockPredictor];
				channel0.delta = buffer[offset];
				channel0.delta |= buffer[offset + 1] << 8;
				if ((channel0.delta & 0x8000) != 0)
				{
					channel0.delta -= 65536;
				}
				offset += 2;
				channel0.sample1 = buffer[offset];
				channel0.sample1 |= buffer[offset + 1] << 8;
				if ((channel0.sample1 & 0x8000) != 0)
				{
					channel0.sample1 -= 65536;
				}
				offset += 2;
				channel0.sample2 = buffer[offset];
				channel0.sample2 |= buffer[offset + 1] << 8;
				if ((channel0.sample2 & 0x8000) != 0)
				{
					channel0.sample2 -= 65536;
				}
				offset += 2;
				samples[sampleOffset] = (byte)channel0.sample2;
				samples[sampleOffset + 1] = (byte)(channel0.sample2 >> 8);
				samples[sampleOffset + 2] = (byte)channel0.sample1;
				samples[sampleOffset + 3] = (byte)(channel0.sample1 >> 8);
				sampleOffset += 4;
				blockSize2 -= offset - offsetStart2;
				for (int j = 0; j < blockSize2; j++)
				{
					int nibbles2 = buffer[offset];
					int sample2 = AudioLoader.AdpcmMsExpandNibbleOrig(ref channel0, nibbles2 >> 4);
					samples[sampleOffset] = (byte)sample2;
					samples[sampleOffset + 1] = (byte)(sample2 >> 8);
					sample2 = AudioLoader.AdpcmMsExpandNibbleOrig(ref channel0, nibbles2 & 0xF);
					samples[sampleOffset + 2] = (byte)sample2;
					samples[sampleOffset + 3] = (byte)(sample2 >> 8);
					sampleOffset += 4;
					offset++;
				}
			}
		}
		return samples;
	}
}
