using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Threading;
using RTAudioProcessing;

namespace Microsoft.Xna.Framework.Audio;

/// <summary>Represents a collection of wave files.</summary>
public class WaveBank : IDisposable
{
	private struct Segment
	{
		public int Offset;

		public int Length;
	}

	private struct WaveBankHeader
	{
		public int Version;

		public Segment[] Segments;
	}

	private struct WaveBankData
	{
		public int Flags;

		public int EntryCount;

		public string BankName;

		public int EntryMetaDataElementSize;

		public int EntryNameElementSize;

		public int Alignment;

		public int CompactFormat;

		public int BuildTime;
	}

	private struct StreamInfo
	{
		public int Format;

		public int FileOffset;

		public int FileLength;

		public int LoopStart;

		public int LoopLength;
	}

	private readonly SoundEffect[] _sounds;

	private readonly StreamInfo[] _streams;

	private readonly string _bankName;

	private readonly string _waveBankFileName;

	private readonly bool _streaming;

	private readonly int _offset;

	private readonly int _packetSize;

	private readonly int _version;

	private readonly int _playRegionOffset;

	private const int Flag_EntryNames = 65536;

	private const int Flag_Compact = 131072;

	private const int Flag_SyncDisabled = 262144;

	private const int Flag_SeekTables = 524288;

	private const int Flag_Mask = 983040;

	/// <summary>
	/// </summary>
	public bool IsInUse { get; private set; }

	/// <summary>
	/// </summary>
	public bool IsPrepared { get; private set; }

	/// <summary>
	/// Is true if the WaveBank has been disposed.
	/// </summary>
	public bool IsDisposed { get; private set; }

	/// <summary>
	/// This event is triggered when the WaveBank is disposed.
	/// </summary>
	public event EventHandler<EventArgs> Disposing;

	/// <param name="audioEngine">Instance of the AudioEngine to associate this wave bank with.</param>
	/// <param name="nonStreamingWaveBankFilename">Path to the .xwb file to load.</param>
	/// <remarks>This constructor immediately loads all wave data into memory at once.</remarks>
	public WaveBank(AudioEngine audioEngine, string nonStreamingWaveBankFilename)
		: this(audioEngine, nonStreamingWaveBankFilename, streaming: false, 0, 0)
	{
	}

	private WaveBank(AudioEngine audioEngine, string waveBankFilename, bool streaming, int offset, int packetsize)
	{
		if (audioEngine == null)
		{
			throw new ArgumentNullException("audioEngine");
		}
		if (string.IsNullOrEmpty(waveBankFilename))
		{
			throw new ArgumentNullException("nonStreamingWaveBankFilename");
		}
		if (streaming)
		{
			if (offset != 0)
			{
				throw new ArgumentException("We only support a zero offset in streaming banks.", "offset");
			}
			if (packetsize < 2)
			{
				throw new ArgumentException("The packet size must be greater than 2.", "packetsize");
			}
			this._streaming = true;
			this._offset = offset;
			this._packetSize = packetsize;
		}
		WaveBankData wavebankdata = default(WaveBankData);
		wavebankdata.EntryNameElementSize = 0;
		wavebankdata.CompactFormat = 0;
		wavebankdata.Alignment = 0;
		wavebankdata.BuildTime = 0;
		int wavebank_offset = 0;
		this._waveBankFileName = waveBankFilename;
		BinaryReader reader = new BinaryReader(AudioEngine.OpenStream(waveBankFilename));
		reader.ReadBytes(4);
		WaveBankHeader wavebankheader = default(WaveBankHeader);
		this._version = (wavebankheader.Version = reader.ReadInt32());
		int last_segment = 4;
		if (wavebankheader.Version <= 3)
		{
			last_segment = 3;
		}
		if (wavebankheader.Version >= 42)
		{
			reader.ReadInt32();
		}
		wavebankheader.Segments = new Segment[5];
		for (int i = 0; i <= last_segment; i++)
		{
			wavebankheader.Segments[i].Offset = reader.ReadInt32();
			wavebankheader.Segments[i].Length = reader.ReadInt32();
		}
		reader.BaseStream.Seek(wavebankheader.Segments[0].Offset, SeekOrigin.Begin);
		wavebankdata.Flags = reader.ReadInt32();
		wavebankdata.EntryCount = reader.ReadInt32();
		if (wavebankheader.Version == 2 || wavebankheader.Version == 3)
		{
			wavebankdata.BankName = Encoding.UTF8.GetString(reader.ReadBytes(16), 0, 16).Replace("\0", "");
		}
		else
		{
			wavebankdata.BankName = Encoding.UTF8.GetString(reader.ReadBytes(64), 0, 64).Replace("\0", "");
		}
		this._bankName = wavebankdata.BankName;
		if (wavebankheader.Version == 1)
		{
			wavebankdata.EntryMetaDataElementSize = 20;
		}
		else
		{
			wavebankdata.EntryMetaDataElementSize = reader.ReadInt32();
			wavebankdata.EntryNameElementSize = reader.ReadInt32();
			wavebankdata.Alignment = reader.ReadInt32();
			wavebank_offset = wavebankheader.Segments[1].Offset;
		}
		if ((wavebankdata.Flags & 0x20000) != 0)
		{
			reader.ReadInt32();
		}
		this._playRegionOffset = wavebankheader.Segments[last_segment].Offset;
		if (this._playRegionOffset == 0)
		{
			this._playRegionOffset = wavebank_offset + wavebankdata.EntryCount * wavebankdata.EntryMetaDataElementSize;
		}
		int segidx_entry_name = 2;
		if (wavebankheader.Version >= 42)
		{
			segidx_entry_name = 3;
		}
		if (wavebankheader.Segments[segidx_entry_name].Offset != 0 && wavebankheader.Segments[segidx_entry_name].Length != 0)
		{
			if (wavebankdata.EntryNameElementSize == -1)
			{
				wavebankdata.EntryNameElementSize = 0;
			}
			(new byte[wavebankdata.EntryNameElementSize + 1])[wavebankdata.EntryNameElementSize] = 0;
		}
		this._sounds = new SoundEffect[wavebankdata.EntryCount];
		this._streams = new StreamInfo[wavebankdata.EntryCount];
		reader.BaseStream.Seek(wavebank_offset, SeekOrigin.Begin);
		if ((wavebankdata.Flags & 0x20000) != 0)
		{
			for (int j = 0; j < wavebankdata.EntryCount; j++)
			{
				int len = reader.ReadInt32();
				this._streams[j].Format = wavebankdata.CompactFormat;
				this._streams[j].FileOffset = (len & 0x1FFFFF) * wavebankdata.Alignment;
			}
			for (int k = 0; k < wavebankdata.EntryCount; k++)
			{
				int nextOffset = ((k != wavebankdata.EntryCount - 1) ? this._streams[k + 1].FileOffset : wavebankheader.Segments[last_segment].Length);
				this._streams[k].FileLength = nextOffset - this._streams[k].FileOffset;
			}
		}
		else
		{
			for (int l = 0; l < wavebankdata.EntryCount; l++)
			{
				StreamInfo info = default(StreamInfo);
				if (wavebankheader.Version == 1)
				{
					info.Format = reader.ReadInt32();
					info.FileOffset = reader.ReadInt32();
					info.FileLength = reader.ReadInt32();
					info.LoopStart = reader.ReadInt32();
					info.LoopLength = reader.ReadInt32();
				}
				else
				{
					reader.ReadInt32();
					if (wavebankdata.EntryMetaDataElementSize >= 8)
					{
						info.Format = reader.ReadInt32();
					}
					if (wavebankdata.EntryMetaDataElementSize >= 12)
					{
						info.FileOffset = reader.ReadInt32();
					}
					if (wavebankdata.EntryMetaDataElementSize >= 16)
					{
						info.FileLength = reader.ReadInt32();
					}
					if (wavebankdata.EntryMetaDataElementSize >= 20)
					{
						info.LoopStart = reader.ReadInt32();
					}
					if (wavebankdata.EntryMetaDataElementSize >= 24)
					{
						info.LoopLength = reader.ReadInt32();
					}
				}
				if (wavebankdata.EntryMetaDataElementSize < 24 && info.FileLength != 0)
				{
					info.FileLength = wavebankheader.Segments[last_segment].Length;
				}
				this._streams[l] = info;
			}
		}
		if (!this._streaming)
		{
			for (int m = 0; m < this._streams.Length; m++)
			{
				StreamInfo info2 = this._streams[m];
				reader.BaseStream.Seek(info2.FileOffset + this._playRegionOffset, SeekOrigin.Begin);
				byte[] audiodata = reader.ReadBytes(info2.FileLength);
				this.DecodeFormat(info2.Format, out var codec, out var channels, out var rate, out var alignment);
				this._sounds[m] = new SoundEffect(codec, audiodata, channels, rate, alignment, info2.LoopStart, info2.LoopLength);
			}
			this._streams = null;
		}
		audioEngine.Wavebanks[this._bankName] = this;
		this.IsPrepared = true;
	}

	private void DecodeFormat(int format, out MiniFormatTag codec, out int channels, out int rate, out int alignment, out int bits)
	{
		if (this._version == 1)
		{
			codec = (MiniFormatTag)(format & 1);
			channels = (format >> 1) & 7;
			rate = (format >> 5) & 0x3FFFF;
			alignment = (format >> 23) & 0xFF;
			bits = (format >> 31) & 1;
		}
		else
		{
			codec = (MiniFormatTag)(format & 3);
			channels = (format >> 2) & 7;
			rate = (format >> 5) & 0x3FFFF;
			alignment = (format >> 23) & 0xFF;
			bits = (format >> 31) & 1;
		}
	}

	private void DecodeFormat(int format, out MiniFormatTag codec, out int channels, out int rate, out int alignment)
	{
		this.DecodeFormat(format, out codec, out channels, out rate, out alignment, out var _);
	}

	/// <param name="audioEngine">Instance of the AudioEngine to associate this wave bank with.</param>
	/// <param name="streamingWaveBankFilename">Path to the .xwb to stream from.</param>
	/// <param name="offset">DVD sector-aligned offset within the wave bank data file.</param>
	/// <param name="packetsize">Stream packet size, in sectors, to use for each stream. The minimum value is 2.</param>
	/// <remarks>
	/// <para>This constructor streams wave data as needed.</para>
	/// <para>Note that packetsize is in sectors, which is 2048 bytes.</para>
	/// <para>AudioEngine.Update() must be called at least once before using data from a streaming wave bank.</para>
	/// </remarks>
	public WaveBank(AudioEngine audioEngine, string streamingWaveBankFilename, int offset, short packetsize)
		: this(audioEngine, streamingWaveBankFilename, streaming: true, offset, packetsize)
	{
	}

	public SoundEffect GetSoundEffect(int trackIndex)
	{
		return this._sounds[trackIndex];
	}

	public SoundEffectInstance GetSoundEffectInstance(int trackIndex, out bool streaming)
	{
		if (this._streaming)
		{
			streaming = true;
			StreamInfo stream = this._streams[trackIndex];
			return this.PlatformCreateStream(stream);
		}
		streaming = false;
		return this._sounds[trackIndex].GetPooledInstance(forXAct: true);
	}

	/// <summary>
	/// Disposes the WaveBank.
	/// </summary>
	public void Dispose()
	{
		this.Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	~WaveBank()
	{
		this.Dispose(disposing: false);
	}

	private void Dispose(bool disposing)
	{
		if (this.IsDisposed)
		{
			return;
		}
		this.IsDisposed = true;
		if (disposing)
		{
			SoundEffect[] sounds = this._sounds;
			for (int i = 0; i < sounds.Length; i++)
			{
				sounds[i].Dispose();
			}
			this.IsPrepared = false;
			this.IsInUse = false;
			EventHelpers.Raise(this, this.Disposing, EventArgs.Empty);
		}
	}

	private unsafe SoundEffectInstance PlatformCreateStream(StreamInfo info)
	{
		this.DecodeFormat(info.Format, out var codec, out var channels, out var rate, out var alignment, out var bits);
		int bufferSize = 16384;
		int pcmAlignment = alignment;
		bool msadpcm = codec == MiniFormatTag.Adpcm;
		bool stereo = channels == 2;
		if (msadpcm)
		{
			alignment = (alignment + 22) * channels;
			pcmAlignment = (channels + 1) * 2;
		}
		DynamicSoundEffectInstance sound = new DynamicSoundEffectInstance(msadpcm: false, pcmAlignment, rate, (channels != 2) ? AudioChannels.Mono : AudioChannels.Stereo);
		sound._isXAct = true;
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
			int num = 0;
			int num2 = this._playRegionOffset + info.FileOffset;
			int num3 = 0;
			byte[][] array = new byte[4][]
			{
				new byte[bufferSize],
				new byte[bufferSize],
				new byte[bufferSize],
				new byte[bufferSize]
			};
			byte[] array2 = new byte[info.FileLength];
			using (Stream stream = TitleContainer.OpenStream(this._waveBankFileName))
			{
				if (stream.CanSeek)
				{
					stream.Seek(num2, SeekOrigin.Begin);
				}
				else
				{
					byte[] array3 = new byte[32768];
					int count;
					for (int i = 0; i != num2; i += stream.Read(array3, 0, count))
					{
						count = Math.Min(array3.Length, num2 - i);
					}
				}
				stream.Read(array2, 0, array2.Length);
			}
			RtapFormat format = RtapFormat.Mono8;
			if (msadpcm)
			{
				format = (stereo ? RtapFormat.StereoMSAdpcm : RtapFormat.MonoMSAdpcm);
			}
			else if (bits == 16)
			{
				format = ((!stereo) ? RtapFormat.Mono16 : RtapFormat.Stereo16);
			}
			else if (stereo)
			{
				format = RtapFormat.Stereo8;
			}
			RtapSpring rtapSpring = new RtapSpring(array2, format, rate, alignment);
			RtapRiver rtapRiver = new RtapRiver();
			rtapRiver.SetSpring(rtapSpring);
			array2 = null;
			int length = rtapSpring.Length;
			byte[] array4 = new byte[length % bufferSize];
			do
			{
				int num4 = 0;
				while (!sound.IsDisposed)
				{
					while (queue.Count < 3 && num4 < length)
					{
						byte[] array5 = array4;
						int num5 = Math.Min(bufferSize, length - num4);
						if (num5 == bufferSize)
						{
							array5 = array[num3];
							num3 = (num3 + 1) % 4;
						}
						fixed (byte* ptr = array5)
						{
							rtapRiver.ReadInto((IntPtr)ptr, num4, num5);
						}
						num4 += num5;
						queue.Enqueue(array5);
						if (num4 >= length)
						{
							goto end_IL_0210;
						}
					}
					signal.WaitOne(100);
					continue;
					end_IL_0210:
					break;
				}
			}
			while (!sound.IsDisposed && (sound.LoopCount >= 255 || num++ < sound.LoopCount));
			rtapRiver.Dispose();
			rtapSpring.Dispose();
			stop.Set();
		});
		thread.Priority = ThreadPriority.Highest;
		thread.Start();
		return sound;
	}
}
