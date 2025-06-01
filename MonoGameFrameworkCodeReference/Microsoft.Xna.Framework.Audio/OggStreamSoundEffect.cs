using System;
using System.Collections.Concurrent;
using System.Threading;
using NVorbis;

namespace Microsoft.Xna.Framework.Audio;

public class OggStreamSoundEffect : SoundEffect
{
	private const int BufferSize = 16384;

	private const int BytesPerSample = 2;

	private const int BufferSamples = 8192;

	private string OggFileName;

	private long TotalSamplesPerChannel;

	private int SampleRate;

	private AudioChannels Channels;

	public OggStreamSoundEffect(string oggFileName)
	{
		this.OggFileName = oggFileName;
		using VorbisReader reader = new VorbisReader(this.OggFileName);
		this.TotalSamplesPerChannel = reader.TotalSamples;
		this.SampleRate = reader.SampleRate;
		this.Channels = ((reader.Channels != 2) ? AudioChannels.Mono : AudioChannels.Stereo);
	}

	public override SoundEffectInstance GetPooledInstance(bool forXAct)
	{
		DynamicSoundEffectInstance sound = new DynamicSoundEffectInstance(this.SampleRate, this.Channels);
		sound._isXAct = forXAct;
		ConcurrentQueue<byte[]> queue = new ConcurrentQueue<byte[]>();
		AutoResetEvent signal = new AutoResetEvent(initialState: false);
		AutoResetEvent stop = new AutoResetEvent(initialState: false);
		sound.BufferNeeded += delegate
		{
			byte[] result = null;
			try
			{
				do
				{
					int num = 0;
					while (queue.Count > 0)
					{
						if (queue.TryDequeue(out result))
						{
							sound.SubmitBuffer(result);
							num++;
						}
					}
					signal.Set();
					if (num > 0)
					{
						return;
					}
				}
				while (!stop.WaitOne(0));
				sound.Stop();
			}
			catch (Exception)
			{
			}
		};
		Thread thread = new Thread((ThreadStart)delegate
		{
			int num = (int)(this.TotalSamplesPerChannel * (long)this.Channels * 2 % 16384);
			int num2 = 0;
			float[] array = new float[8192];
			short[] array2 = new short[8192];
			int num3 = 0;
			byte[][] array3 = new byte[4][]
			{
				new byte[16384],
				new byte[16384],
				new byte[16384],
				new byte[16384]
			};
			byte[] array4 = new byte[num];
			VorbisReader vorbisReader = new VorbisReader(this.OggFileName);
			do
			{
				vorbisReader.DecodedPosition = 0L;
				while (!sound.IsDisposed)
				{
					while (queue.Count < 3 && vorbisReader.DecodedPosition < this.TotalSamplesPerChannel)
					{
						byte[] array5 = array4;
						int num4 = Math.Min(8192, (int)((this.TotalSamplesPerChannel - vorbisReader.DecodedPosition) * (long)this.Channels));
						if (num4 == 8192)
						{
							array5 = array3[num3];
							num3 = (num3 + 1) % 4;
						}
						num4 = vorbisReader.ReadSamples(array, 0, num4);
						OggStream.CastBuffer(array, array2, num4);
						Buffer.BlockCopy(array2, 0, array5, 0, num4 * 2);
						queue.Enqueue(array5);
						if (vorbisReader.DecodedPosition >= this.TotalSamplesPerChannel)
						{
							goto end_IL_0167;
						}
					}
					signal.WaitOne(1000);
					continue;
					end_IL_0167:
					break;
				}
			}
			while (!sound.IsDisposed && (sound.LoopCount >= 255 || num2++ < sound.LoopCount));
			vorbisReader.Dispose();
			stop.Set();
		});
		thread.Priority = ThreadPriority.Highest;
		thread.Start();
		return sound;
	}
}
