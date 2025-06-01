using System.IO;

namespace Microsoft.Xna.Framework.Audio;

internal static class AudioUtil
{
	/// <summary>
	/// Takes WAV data and appends a header to it.
	/// </summary>
	internal static byte[] FormatWavData(byte[] buffer, int sampleRate, int channels)
	{
		short bitsPerSample = 16;
		using MemoryStream mStream = new MemoryStream(44 + buffer.Length);
		using BinaryWriter writer = new BinaryWriter(mStream);
		writer.Write("RIFF".ToCharArray());
		writer.Write(36 + buffer.Length);
		writer.Write("WAVE".ToCharArray());
		writer.Write("fmt ".ToCharArray());
		writer.Write(16);
		writer.Write((short)1);
		writer.Write((short)channels);
		writer.Write(sampleRate);
		short blockAlign = (short)(bitsPerSample / 8 * channels);
		writer.Write(sampleRate * blockAlign);
		writer.Write(blockAlign);
		writer.Write(bitsPerSample);
		writer.Write("data".ToCharArray());
		writer.Write(buffer.Length);
		writer.Write(buffer);
		return mStream.ToArray();
	}
}
