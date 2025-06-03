using System;
using System.IO;
using System.Threading;

namespace NVorbis;

/// <summary>
/// A thread-safe, read-only, buffering stream wrapper.
/// </summary>
internal class BufferedReadStream : Stream
{
	private const int DEFAULT_INITIAL_SIZE = 32768;

	private const int DEFAULT_MAX_SIZE = 262144;

	private Stream _baseStream;

	private StreamReadBuffer _buffer;

	private long _readPosition;

	private object _localLock = new object();

	private Thread _owningThread;

	private int _lockCount;

	public bool CloseBaseStream { get; set; }

	public bool MinimalRead
	{
		get
		{
			return this._buffer.MinimalRead;
		}
		set
		{
			this._buffer.MinimalRead = value;
		}
	}

	public int MaxBufferSize
	{
		get
		{
			return this._buffer.MaxSize;
		}
		set
		{
			this.CheckLock();
			this._buffer.MaxSize = value;
		}
	}

	public long BufferBaseOffset => this._buffer.BaseOffset;

	public int BufferBytesFilled => this._buffer.BytesFilled;

	public override bool CanRead => true;

	public override bool CanSeek => true;

	public override bool CanWrite => false;

	public override long Length => this._baseStream.Length;

	public override long Position
	{
		get
		{
			return this._readPosition;
		}
		set
		{
			this.Seek(value, SeekOrigin.Begin);
		}
	}

	public BufferedReadStream(Stream baseStream)
		: this(baseStream, 32768, 262144, minimalRead: false)
	{
	}

	public BufferedReadStream(Stream baseStream, bool minimalRead)
		: this(baseStream, 32768, 262144, minimalRead)
	{
	}

	public BufferedReadStream(Stream baseStream, int initialSize, int maxSize)
		: this(baseStream, initialSize, maxSize, minimalRead: false)
	{
	}

	public BufferedReadStream(Stream baseStream, int initialSize, int maxBufferSize, bool minimalRead)
	{
		if (baseStream == null)
		{
			throw new ArgumentNullException("baseStream");
		}
		if (!baseStream.CanRead)
		{
			throw new ArgumentException("baseStream");
		}
		if (maxBufferSize < 1)
		{
			maxBufferSize = 1;
		}
		if (initialSize < 1)
		{
			initialSize = 1;
		}
		if (initialSize > maxBufferSize)
		{
			initialSize = maxBufferSize;
		}
		this._baseStream = baseStream;
		this._buffer = new StreamReadBuffer(baseStream, initialSize, maxBufferSize, minimalRead);
		this._buffer.MaxSize = maxBufferSize;
		this._buffer.MinimalRead = minimalRead;
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
		if (disposing)
		{
			if (this._buffer != null)
			{
				this._buffer.Dispose();
				this._buffer = null;
			}
			if (this.CloseBaseStream)
			{
				this._baseStream.Close();
			}
		}
	}

	public void TakeLock()
	{
		Monitor.Enter(this._localLock);
		if (++this._lockCount == 1)
		{
			this._owningThread = Thread.CurrentThread;
		}
	}

	private void CheckLock()
	{
		if (this._owningThread != Thread.CurrentThread)
		{
			throw new SynchronizationLockException();
		}
	}

	public void ReleaseLock()
	{
		this.CheckLock();
		if (--this._lockCount == 0)
		{
			this._owningThread = null;
		}
		Monitor.Exit(this._localLock);
	}

	public void Discard(int bytes)
	{
		this.CheckLock();
		this._buffer.DiscardThrough(this._buffer.BaseOffset + bytes);
	}

	public void DiscardThrough(long offset)
	{
		this.CheckLock();
		this._buffer.DiscardThrough(offset);
	}

	public override void Flush()
	{
	}

	public override int ReadByte()
	{
		this.CheckLock();
		int num = this._buffer.ReadByte(this.Position);
		if (num > -1)
		{
			this.Seek(1L, SeekOrigin.Current);
		}
		return num;
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		this.CheckLock();
		int cnt = this._buffer.Read(this.Position, buffer, offset, count);
		this.Seek(cnt, SeekOrigin.Current);
		return cnt;
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		this.CheckLock();
		switch (origin)
		{
		case SeekOrigin.Current:
			offset += this.Position;
			break;
		case SeekOrigin.End:
			offset += this._baseStream.Length;
			break;
		}
		if (!this._baseStream.CanSeek)
		{
			if (offset < this._buffer.BaseOffset)
			{
				throw new InvalidOperationException("Cannot seek to before the start of the buffer!");
			}
			if (offset >= this._buffer.BufferEndOffset)
			{
				throw new InvalidOperationException("Cannot seek to beyond the end of the buffer!  Discard some bytes.");
			}
		}
		return this._readPosition = offset;
	}

	public override void SetLength(long value)
	{
		throw new NotSupportedException();
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		throw new NotSupportedException();
	}
}
