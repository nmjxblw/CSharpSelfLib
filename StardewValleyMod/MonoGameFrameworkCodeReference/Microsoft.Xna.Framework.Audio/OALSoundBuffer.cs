using System;
using MonoGame.OpenAL;

namespace Microsoft.Xna.Framework.Audio;

internal class OALSoundBuffer : IDisposable
{
	private int openALDataBuffer;

	private ALFormat openALFormat;

	private int dataSize;

	private bool _isDisposed;

	public int OpenALDataBuffer => this.openALDataBuffer;

	public double Duration { get; set; }

	public OALSoundBuffer()
	{
		AL.GenBuffers(1, out this.openALDataBuffer);
	}

	~OALSoundBuffer()
	{
		this.Dispose(disposing: false);
	}

	public void BindDataBuffer(byte[] dataBuffer, ALFormat format, int size, int sampleRate, int sampleAlignment = 0)
	{
		if ((format == ALFormat.MonoMSAdpcm || format == ALFormat.StereoMSAdpcm) && !OpenALSoundController.Instance.SupportsAdpcm)
		{
			throw new InvalidOperationException("MS-ADPCM is not supported by this OpenAL driver");
		}
		if ((format == ALFormat.MonoIma4 || format == ALFormat.StereoIma4) && !OpenALSoundController.Instance.SupportsIma4)
		{
			throw new InvalidOperationException("IMA/ADPCM is not supported by this OpenAL driver");
		}
		this.openALFormat = format;
		this.dataSize = size;
		int unpackedSize = 0;
		if (sampleAlignment > 0)
		{
			AL.Bufferi(this.openALDataBuffer, ALBufferi.UnpackBlockAlignmentSoft, sampleAlignment);
		}
		AL.BufferData(this.openALDataBuffer, this.openALFormat, dataBuffer, size, sampleRate);
		this.Duration = -1.0;
		AL.GetBuffer(this.openALDataBuffer, ALGetBufferi.Bits, out var bits);
		AL.GetBuffer(this.openALDataBuffer, ALGetBufferi.Channels, out var channels);
		AL.GetBuffer(this.openALDataBuffer, ALGetBufferi.Size, out unpackedSize);
		this.Duration = (float)(unpackedSize / (bits / 8 * channels)) / (float)sampleRate;
	}

	public void Dispose()
	{
		this.Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!this._isDisposed)
		{
			if (AL.IsBuffer(this.openALDataBuffer))
			{
				AL.DeleteBuffers(1, ref this.openALDataBuffer);
			}
			this._isDisposed = true;
		}
	}
}
