using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NVorbis.Ogg;

namespace NVorbis;

public class VorbisReader : IDisposable
{
	private int _streamIdx;

	private IContainerReader _containerReader;

	private List<VorbisStreamDecoder> _decoders;

	private List<int> _serials;

	private VorbisStreamDecoder ActiveDecoder
	{
		get
		{
			if (this._decoders == null)
			{
				throw new ObjectDisposedException("VorbisReader");
			}
			return this._decoders[this._streamIdx];
		}
	}

	/// <summary>
	/// Gets the number of channels in the current selected Vorbis stream
	/// </summary>
	public int Channels => this.ActiveDecoder._channels;

	/// <summary>
	/// Gets the sample rate of the current selected Vorbis stream
	/// </summary>
	public int SampleRate => this.ActiveDecoder._sampleRate;

	/// <summary>
	/// Gets the encoder's upper bitrate of the current selected Vorbis stream
	/// </summary>
	public int UpperBitrate => this.ActiveDecoder._upperBitrate;

	/// <summary>
	/// Gets the encoder's nominal bitrate of the current selected Vorbis stream
	/// </summary>
	public int NominalBitrate => this.ActiveDecoder._nominalBitrate;

	/// <summary>
	/// Gets the encoder's lower bitrate of the current selected Vorbis stream
	/// </summary>
	public int LowerBitrate => this.ActiveDecoder._lowerBitrate;

	/// <summary>
	/// Gets the encoder's vendor string for the current selected Vorbis stream
	/// </summary>
	public string Vendor => this.ActiveDecoder._vendor;

	/// <summary>
	/// Gets the comments in the current selected Vorbis stream
	/// </summary>
	public string[] Comments => this.ActiveDecoder._comments;

	/// <summary>
	/// Gets whether the previous short sample count was due to a parameter change in the stream.
	/// </summary>
	public bool IsParameterChange => this.ActiveDecoder.IsParameterChange;

	/// <summary>
	/// Gets the number of bits read that are related to framing and transport alone
	/// </summary>
	public long ContainerOverheadBits => this.ActiveDecoder.ContainerBits;

	/// <summary>
	/// Gets or sets whether to automatically apply clipping to samples returned by <see cref="M:NVorbis.VorbisReader.ReadSamples(System.Single[],System.Int32,System.Int32)" />.
	/// </summary>
	public bool ClipSamples { get; set; }

	/// <summary>
	/// Gets stats from each decoder stream available
	/// </summary>
	public IVorbisStreamStatus[] Stats => this._decoders.Select((VorbisStreamDecoder d) => d).Cast<IVorbisStreamStatus>().ToArray();

	/// <summary>
	/// Gets the currently-selected stream's index
	/// </summary>
	public int StreamIndex => this._streamIdx;

	/// <summary>
	/// Returns the number of logical streams found so far in the physical container
	/// </summary>
	public int StreamCount => this._decoders.Count;

	/// <summary>
	/// Gets or Sets the current timestamp of the decoder.  Is the timestamp before the next sample to be decoded
	/// </summary>
	public TimeSpan DecodedTime
	{
		get
		{
			return TimeSpan.FromSeconds((double)this.ActiveDecoder.CurrentPosition / (double)this.SampleRate);
		}
		set
		{
			this.ActiveDecoder.SeekTo((long)(value.TotalSeconds * (double)this.SampleRate));
		}
	}

	/// <summary>
	/// Gets or Sets the current position of the next sample to be decoded.
	/// </summary>
	public long DecodedPosition
	{
		get
		{
			return this.ActiveDecoder.CurrentPosition;
		}
		set
		{
			this.ActiveDecoder.SeekTo(value);
		}
	}

	/// <summary>
	/// Gets the total length of the current logical stream
	/// </summary>
	public TimeSpan TotalTime
	{
		get
		{
			VorbisStreamDecoder decoder = this.ActiveDecoder;
			if (decoder.CanSeek)
			{
				return TimeSpan.FromSeconds((double)decoder.GetLastGranulePos() / (double)decoder._sampleRate);
			}
			return TimeSpan.MaxValue;
		}
	}

	public long TotalSamples
	{
		get
		{
			VorbisStreamDecoder decoder = this.ActiveDecoder;
			if (decoder.CanSeek)
			{
				return decoder.GetLastGranulePos();
			}
			return long.MaxValue;
		}
	}

	private VorbisReader()
	{
		this.ClipSamples = true;
		this._decoders = new List<VorbisStreamDecoder>();
		this._serials = new List<int>();
	}

	public VorbisReader(string fileName)
		: this(File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read), closeStreamOnDispose: true)
	{
	}

	public VorbisReader(Stream stream, bool closeStreamOnDispose)
		: this()
	{
		BufferedReadStream bufferedStream = new BufferedReadStream(stream)
		{
			CloseBaseStream = closeStreamOnDispose
		};
		ContainerReader oggContainer = new ContainerReader(bufferedStream, closeStreamOnDispose);
		if (!this.LoadContainer(oggContainer))
		{
			bufferedStream.Close();
			throw new InvalidDataException("Could not determine container type!");
		}
		this._containerReader = oggContainer;
		if (this._decoders.Count == 0)
		{
			throw new InvalidDataException("No Vorbis data found!");
		}
	}

	public VorbisReader(IContainerReader containerReader)
		: this()
	{
		if (!this.LoadContainer(containerReader))
		{
			throw new InvalidDataException("Container did not initialize!");
		}
		this._containerReader = containerReader;
		if (this._decoders.Count == 0)
		{
			throw new InvalidDataException("No Vorbis data found!");
		}
	}

	public VorbisReader(IPacketProvider packetProvider)
		: this()
	{
		NewStreamEventArgs ea = new NewStreamEventArgs(packetProvider);
		this.NewStream(this, ea);
		if (ea.IgnoreStream)
		{
			throw new InvalidDataException("No Vorbis data found!");
		}
	}

	private bool LoadContainer(IContainerReader containerReader)
	{
		containerReader.NewStream += NewStream;
		if (!containerReader.Init())
		{
			containerReader.NewStream -= NewStream;
			return false;
		}
		return true;
	}

	private void NewStream(object sender, NewStreamEventArgs ea)
	{
		IPacketProvider packetProvider = ea.PacketProvider;
		VorbisStreamDecoder decoder = new VorbisStreamDecoder(packetProvider);
		if (decoder.TryInit())
		{
			this._decoders.Add(decoder);
			this._serials.Add(packetProvider.StreamSerial);
		}
		else
		{
			ea.IgnoreStream = true;
		}
	}

	public void Dispose()
	{
		if (this._decoders != null)
		{
			foreach (VorbisStreamDecoder decoder in this._decoders)
			{
				decoder.Dispose();
			}
			this._decoders.Clear();
			this._decoders = null;
		}
		if (this._containerReader != null)
		{
			this._containerReader.NewStream -= NewStream;
			this._containerReader.Dispose();
			this._containerReader = null;
		}
	}

	/// <summary>
	/// Reads decoded samples from the current logical stream
	/// </summary>
	/// <param name="buffer">The buffer to write the samples to</param>
	/// <param name="offset">The offset into the buffer to write the samples to</param>
	/// <param name="count">The number of samples to write</param>
	/// <returns>The number of samples written</returns>
	public int ReadSamples(float[] buffer, int offset, int count)
	{
		if (offset < 0)
		{
			throw new ArgumentOutOfRangeException("offset");
		}
		if (count < 0 || offset + count > buffer.Length)
		{
			throw new ArgumentOutOfRangeException("count");
		}
		count = this.ActiveDecoder.ReadSamples(buffer, offset, count);
		if (this.ClipSamples)
		{
			VorbisStreamDecoder decoder = this._decoders[this._streamIdx];
			int i = 0;
			while (i < count)
			{
				buffer[offset] = Utils.ClipValue(buffer[offset], ref decoder._clipped);
				i++;
				offset++;
			}
		}
		return count;
	}

	/// <summary>
	/// Clears the parameter change flag so further samples can be requested.
	/// </summary>
	public void ClearParameterChange()
	{
		this.ActiveDecoder.IsParameterChange = false;
	}

	/// <summary>
	/// Searches for the next stream in a concatenated file
	/// </summary>
	/// <returns><c>True</c> if a new stream was found, otherwise <c>false</c>.</returns>
	public bool FindNextStream()
	{
		if (this._containerReader == null)
		{
			return false;
		}
		return this._containerReader.FindNextStream();
	}

	/// <summary>
	/// Switches to an alternate logical stream.
	/// </summary>
	/// <param name="index">The logical stream index to switch to</param>
	/// <returns><c>True</c> if the properties of the logical stream differ from those of the one previously being decoded. Otherwise, <c>False</c>.</returns>
	public bool SwitchStreams(int index)
	{
		if (index < 0 || index >= this.StreamCount)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		if (this._decoders == null)
		{
			throw new ObjectDisposedException("VorbisReader");
		}
		if (this._streamIdx == index)
		{
			return false;
		}
		VorbisStreamDecoder curDecoder = this._decoders[this._streamIdx];
		this._streamIdx = index;
		VorbisStreamDecoder newDecoder = this._decoders[this._streamIdx];
		if (curDecoder._channels == newDecoder._channels)
		{
			return curDecoder._sampleRate != newDecoder._sampleRate;
		}
		return true;
	}
}
