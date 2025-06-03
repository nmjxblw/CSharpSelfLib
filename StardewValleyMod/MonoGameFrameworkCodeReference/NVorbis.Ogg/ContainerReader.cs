using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NVorbis.Ogg;

/// <summary>
/// Provides an <see cref="T:NVorbis.IContainerReader" /> implementation for basic Ogg files.
/// </summary>
public class ContainerReader : IContainerReader, IDisposable
{
	private class PageHeader
	{
		public int StreamSerial { get; set; }

		public PageFlags Flags { get; set; }

		public long GranulePosition { get; set; }

		public int SequenceNumber { get; set; }

		public long DataOffset { get; set; }

		public int[] PacketSizes { get; set; }

		public bool LastPacketContinues { get; set; }

		public bool IsResync { get; set; }
	}

	private Crc _crc = new Crc();

	private BufferedReadStream _stream;

	private Dictionary<int, PacketReader> _packetReaders;

	private List<int> _disposedStreamSerials;

	private long _nextPageOffset;

	private int _pageCount;

	private byte[] _readBuffer = new byte[65025];

	private long _containerBits;

	private long _wasteBits;

	/// <summary>
	/// Gets the list of stream serials found in the container so far.
	/// </summary>
	public int[] StreamSerials => this._packetReaders.Keys.ToArray();

	/// <summary>
	/// Gets the number of pages that have been read in the container.
	/// </summary>
	public int PagesRead => this._pageCount;

	/// <summary>
	/// Gets whether the container supports seeking.
	/// </summary>
	public bool CanSeek => this._stream.CanSeek;

	/// <summary>
	/// Gets the number of bits in the container that are not associated with a logical stream.
	/// </summary>
	public long WasteBits => this._wasteBits;

	/// <summary>
	/// Event raised when a new logical stream is found in the container.
	/// </summary>
	public event EventHandler<NewStreamEventArgs> NewStream;

	/// <summary>
	/// Creates a new instance with the specified file.
	/// </summary>
	/// <param name="path">The full path to the file.</param>
	public ContainerReader(string path)
		: this(File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read), closeOnDispose: true)
	{
	}

	/// <summary>
	/// Creates a new instance with the specified stream.  Optionally sets to close the stream when disposed.
	/// </summary>
	/// <param name="stream">The stream to read.</param>
	/// <param name="closeOnDispose"><c>True</c> to close the stream when <see cref="M:NVorbis.Ogg.ContainerReader.Dispose" /> is called, otherwise <c>False</c>.</param>
	public ContainerReader(Stream stream, bool closeOnDispose)
	{
		this._packetReaders = new Dictionary<int, PacketReader>();
		this._disposedStreamSerials = new List<int>();
		this._stream = (stream as BufferedReadStream) ?? new BufferedReadStream(stream)
		{
			CloseBaseStream = closeOnDispose
		};
	}

	/// <summary>
	/// Initializes the container and finds the first stream.
	/// </summary>
	/// <returns><c>True</c> if a valid logical stream is found, otherwise <c>False</c>.</returns>
	public bool Init()
	{
		this._stream.TakeLock();
		try
		{
			return this.GatherNextPage() != -1;
		}
		finally
		{
			this._stream.ReleaseLock();
		}
	}

	/// <summary>
	/// Disposes this instance.
	/// </summary>
	public void Dispose()
	{
		int[] streamSerials = this.StreamSerials;
		foreach (int streamSerial in streamSerials)
		{
			this._packetReaders[streamSerial].Dispose();
		}
		this._nextPageOffset = 0L;
		this._containerBits = 0L;
		this._wasteBits = 0L;
		this._stream.Dispose();
	}

	/// <summary>
	/// Gets the <see cref="T:NVorbis.IPacketProvider" /> instance for the specified stream serial.
	/// </summary>
	/// <param name="streamSerial">The stream serial to look for.</param>
	/// <returns>An <see cref="T:NVorbis.IPacketProvider" /> instance.</returns>
	/// <exception cref="T:System.ArgumentOutOfRangeException">The specified stream serial was not found.</exception>
	public IPacketProvider GetStream(int streamSerial)
	{
		if (!this._packetReaders.TryGetValue(streamSerial, out var provider))
		{
			throw new ArgumentOutOfRangeException("streamSerial");
		}
		return provider;
	}

	/// <summary>
	/// Finds the next new stream in the container.
	/// </summary>
	/// <returns><c>True</c> if a new stream was found, otherwise <c>False</c>.</returns>
	/// <exception cref="T:System.InvalidOperationException"><see cref="P:NVorbis.Ogg.ContainerReader.CanSeek" /> is <c>False</c>.</exception>
	public bool FindNextStream()
	{
		if (!this.CanSeek)
		{
			throw new InvalidOperationException();
		}
		int cnt = this._packetReaders.Count;
		while (this._packetReaders.Count == cnt)
		{
			this._stream.TakeLock();
			try
			{
				if (this.GatherNextPage() == -1)
				{
					break;
				}
			}
			finally
			{
				this._stream.ReleaseLock();
			}
		}
		return cnt > this._packetReaders.Count;
	}

	/// <summary>
	/// Retrieves the total number of pages in the container.
	/// </summary>
	/// <returns>The total number of pages.</returns>
	/// <exception cref="T:System.InvalidOperationException"><see cref="P:NVorbis.Ogg.ContainerReader.CanSeek" /> is <c>False</c>.</exception>
	public int GetTotalPageCount()
	{
		if (!this.CanSeek)
		{
			throw new InvalidOperationException();
		}
		while (true)
		{
			this._stream.TakeLock();
			try
			{
				if (this.GatherNextPage() == -1)
				{
					break;
				}
			}
			finally
			{
				this._stream.ReleaseLock();
			}
		}
		return this._pageCount;
	}

	private PageHeader ReadPageHeader(long position)
	{
		this._stream.Seek(position, SeekOrigin.Begin);
		if (this._stream.Read(this._readBuffer, 0, 27) != 27)
		{
			return null;
		}
		if (this._readBuffer[0] != 79 || this._readBuffer[1] != 103 || this._readBuffer[2] != 103 || this._readBuffer[3] != 83)
		{
			return null;
		}
		if (this._readBuffer[4] != 0)
		{
			return null;
		}
		PageHeader hdr = new PageHeader();
		hdr.Flags = (PageFlags)this._readBuffer[5];
		hdr.GranulePosition = BitConverter.ToInt64(this._readBuffer, 6);
		hdr.StreamSerial = BitConverter.ToInt32(this._readBuffer, 14);
		hdr.SequenceNumber = BitConverter.ToInt32(this._readBuffer, 18);
		uint crc = BitConverter.ToUInt32(this._readBuffer, 22);
		this._crc.Reset();
		for (int i = 0; i < 22; i++)
		{
			this._crc.Update(this._readBuffer[i]);
		}
		this._crc.Update(0);
		this._crc.Update(0);
		this._crc.Update(0);
		this._crc.Update(0);
		this._crc.Update(this._readBuffer[26]);
		int segCnt = this._readBuffer[26];
		if (this._stream.Read(this._readBuffer, 0, segCnt) != segCnt)
		{
			return null;
		}
		List<int> packetSizes = new List<int>(segCnt);
		int size = 0;
		int idx = 0;
		for (int j = 0; j < segCnt; j++)
		{
			byte temp = this._readBuffer[j];
			this._crc.Update(temp);
			if (idx == packetSizes.Count)
			{
				packetSizes.Add(0);
			}
			packetSizes[idx] += temp;
			if (temp < byte.MaxValue)
			{
				idx++;
				hdr.LastPacketContinues = false;
			}
			else
			{
				hdr.LastPacketContinues = true;
			}
			size += temp;
		}
		hdr.PacketSizes = packetSizes.ToArray();
		hdr.DataOffset = position + 27 + segCnt;
		if (this._stream.Read(this._readBuffer, 0, size) != size)
		{
			return null;
		}
		for (int k = 0; k < size; k++)
		{
			this._crc.Update(this._readBuffer[k]);
		}
		if (this._crc.Test(crc))
		{
			this._containerBits += 8 * (27 + segCnt);
			this._pageCount++;
			return hdr;
		}
		return null;
	}

	private PageHeader FindNextPageHeader()
	{
		long startPos = this._nextPageOffset;
		bool isResync = false;
		PageHeader hdr;
		while ((hdr = this.ReadPageHeader(startPos)) == null)
		{
			isResync = true;
			this._wasteBits += 8L;
			startPos = (this._stream.Position = startPos + 1);
			int cnt = 0;
			do
			{
				switch (this._stream.ReadByte())
				{
				case 79:
					if (this._stream.ReadByte() == 103 && this._stream.ReadByte() == 103 && this._stream.ReadByte() == 83)
					{
						startPos += cnt;
						goto end_IL_0032;
					}
					this._stream.Seek(-3L, SeekOrigin.Current);
					break;
				case -1:
					return null;
				}
				this._wasteBits += 8L;
				continue;
				end_IL_0032:
				break;
			}
			while (++cnt < 65536);
			if (cnt == 65536)
			{
				return null;
			}
		}
		hdr.IsResync = isResync;
		this._nextPageOffset = hdr.DataOffset;
		for (int i = 0; i < hdr.PacketSizes.Length; i++)
		{
			this._nextPageOffset += hdr.PacketSizes[i];
		}
		return hdr;
	}

	private bool AddPage(PageHeader hdr)
	{
		if (!this._packetReaders.TryGetValue(hdr.StreamSerial, out var packetReader))
		{
			packetReader = new PacketReader(this, hdr.StreamSerial);
		}
		packetReader.ContainerBits += this._containerBits;
		this._containerBits = 0L;
		bool isContinued = hdr.PacketSizes.Length == 1 && hdr.LastPacketContinues;
		bool isContinuation = (hdr.Flags & PageFlags.ContinuesPacket) == PageFlags.ContinuesPacket;
		bool isEOS = false;
		bool isResync = hdr.IsResync;
		long dataOffset = hdr.DataOffset;
		int cnt = hdr.PacketSizes.Length;
		int[] packetSizes = hdr.PacketSizes;
		foreach (int size in packetSizes)
		{
			Packet packet = new Packet(this, dataOffset, size)
			{
				PageGranulePosition = hdr.GranulePosition,
				IsEndOfStream = isEOS,
				PageSequenceNumber = hdr.SequenceNumber,
				IsContinued = isContinued,
				IsContinuation = isContinuation,
				IsResync = isResync
			};
			packetReader.AddPacket(packet);
			dataOffset += size;
			isContinuation = false;
			isResync = false;
			if (--cnt == 1)
			{
				isContinued = hdr.LastPacketContinues;
				isEOS = (hdr.Flags & PageFlags.EndOfStream) == PageFlags.EndOfStream;
			}
		}
		if (!this._packetReaders.ContainsKey(hdr.StreamSerial))
		{
			this._packetReaders.Add(hdr.StreamSerial, packetReader);
			return true;
		}
		return false;
	}

	private int GatherNextPage()
	{
		PageHeader hdr;
		while (true)
		{
			hdr = this.FindNextPageHeader();
			if (hdr == null)
			{
				return -1;
			}
			if (!this._disposedStreamSerials.Contains(hdr.StreamSerial))
			{
				if (!this.AddPage(hdr))
				{
					break;
				}
				EventHandler<NewStreamEventArgs> callback = this.NewStream;
				if (callback == null)
				{
					break;
				}
				NewStreamEventArgs ea = new NewStreamEventArgs(this._packetReaders[hdr.StreamSerial]);
				callback(this, ea);
				if (!ea.IgnoreStream)
				{
					break;
				}
				this._packetReaders[hdr.StreamSerial].Dispose();
			}
		}
		return hdr.StreamSerial;
	}

	internal void DisposePacketReader(PacketReader packetReader)
	{
		this._disposedStreamSerials.Add(packetReader.StreamSerial);
		this._packetReaders.Remove(packetReader.StreamSerial);
	}

	internal int PacketReadByte(long offset)
	{
		this._stream.TakeLock();
		try
		{
			this._stream.Position = offset;
			return this._stream.ReadByte();
		}
		finally
		{
			this._stream.ReleaseLock();
		}
	}

	internal void PacketDiscardThrough(long offset)
	{
		this._stream.TakeLock();
		try
		{
			this._stream.DiscardThrough(offset);
		}
		finally
		{
			this._stream.ReleaseLock();
		}
	}

	internal void GatherNextPage(int streamSerial)
	{
		if (!this._packetReaders.ContainsKey(streamSerial))
		{
			throw new ArgumentOutOfRangeException("streamSerial");
		}
		int nextSerial;
		do
		{
			this._stream.TakeLock();
			try
			{
				if (this._packetReaders[streamSerial].HasEndOfStream)
				{
					break;
				}
				nextSerial = this.GatherNextPage();
				if (nextSerial != -1)
				{
					continue;
				}
				foreach (KeyValuePair<int, PacketReader> reader in this._packetReaders)
				{
					if (!reader.Value.HasEndOfStream)
					{
						reader.Value.SetEndOfStream();
					}
				}
				break;
			}
			finally
			{
				this._stream.ReleaseLock();
			}
		}
		while (nextSerial != streamSerial);
	}
}
