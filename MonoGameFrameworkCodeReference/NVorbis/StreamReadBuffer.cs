using System;
using System.Collections.Generic;
using System.IO;

namespace NVorbis;

internal class StreamReadBuffer : IDisposable
{
	private class StreamWrapper
	{
		internal Stream Source;

		internal object LockObject = new object();

		internal long EofOffset = long.MaxValue;

		internal int RefCount = 1;
	}

	private class SavedBuffer
	{
		public byte[] Buffer;

		public long BaseOffset;

		public int End;

		public int DiscardCount;

		public long VersionSaved;
	}

	private static Dictionary<Stream, StreamWrapper> _lockObjects = new Dictionary<Stream, StreamWrapper>();

	private StreamWrapper _wrapper;

	private int _maxSize;

	private byte[] _data;

	private long _baseOffset;

	private int _end;

	private int _discardCount;

	private bool _minimalRead;

	private long _versionCounter;

	private List<SavedBuffer> _savedBuffers;

	/// <summary>
	/// Gets or Sets whether to limit reads to the smallest size possible.
	/// </summary>
	public bool MinimalRead
	{
		get
		{
			return this._minimalRead;
		}
		set
		{
			this._minimalRead = value;
		}
	}

	/// <summary>
	/// Gets or Sets the maximum size of the buffer.  This is not a hard limit.
	/// </summary>
	public int MaxSize
	{
		get
		{
			return this._maxSize;
		}
		set
		{
			if (value < 1)
			{
				throw new ArgumentOutOfRangeException("Must be greater than zero.");
			}
			int newMaxSize = 1 << (int)Math.Ceiling(Math.Log(value, 2.0));
			if (newMaxSize < this._end)
			{
				if (newMaxSize < this._end - this._discardCount)
				{
					throw new ArgumentOutOfRangeException("Must be greater than or equal to the number of bytes currently buffered.");
				}
				this.CommitDiscard();
				byte[] newBuf = new byte[newMaxSize];
				Buffer.BlockCopy(this._data, 0, newBuf, 0, this._end);
				this._data = newBuf;
			}
			this._maxSize = newMaxSize;
		}
	}

	/// <summary>
	/// Gets the offset of the start of the buffered data.  Reads to offsets before this are likely to require a seek.
	/// </summary>
	public long BaseOffset => this._baseOffset + this._discardCount;

	/// <summary>
	/// Gets the number of bytes currently buffered.
	/// </summary>
	public int BytesFilled => this._end - this._discardCount;

	/// <summary>
	/// Gets the number of bytes the buffer can hold.
	/// </summary>
	public int Length => this._data.Length;

	internal long BufferEndOffset
	{
		get
		{
			if (this._end - this._discardCount > 0)
			{
				return this._baseOffset + this._discardCount + this._maxSize;
			}
			return this._wrapper.Source.Length;
		}
	}

	internal StreamReadBuffer(Stream source, int initialSize, int maxSize, bool minimalRead)
	{
		StreamWrapper wrapper;
		lock (StreamReadBuffer._lockObjects)
		{
			if (!StreamReadBuffer._lockObjects.TryGetValue(source, out wrapper))
			{
				StreamReadBuffer._lockObjects.Add(source, new StreamWrapper
				{
					Source = source
				});
				wrapper = StreamReadBuffer._lockObjects[source];
				if (source.CanSeek)
				{
					wrapper.EofOffset = source.Length;
				}
			}
			else
			{
				wrapper.RefCount++;
			}
		}
		initialSize = 2 << (int)Math.Log(initialSize - 1, 2.0);
		maxSize = 1 << (int)Math.Log(maxSize, 2.0);
		this._wrapper = wrapper;
		this._data = new byte[initialSize];
		this._maxSize = maxSize;
		this._minimalRead = minimalRead;
		this._savedBuffers = new List<SavedBuffer>();
	}

	public void Dispose()
	{
		lock (StreamReadBuffer._lockObjects)
		{
			if (--this._wrapper.RefCount == 0)
			{
				StreamReadBuffer._lockObjects.Remove(this._wrapper.Source);
			}
		}
	}

	/// <summary>
	/// Reads the number of bytes specified into the buffer given, starting with the offset indicated.
	/// </summary>
	/// <param name="offset">The offset into the stream to start reading.</param>
	/// <param name="buffer">The buffer to read to.</param>
	/// <param name="index">The index into the buffer to start writing to.</param>
	/// <param name="count">The number of bytes to read.</param>
	/// <returns>The number of bytes read.</returns>
	public int Read(long offset, byte[] buffer, int index, int count)
	{
		if (offset < 0)
		{
			throw new ArgumentOutOfRangeException("offset");
		}
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer");
		}
		if (index < 0 || index + count > buffer.Length)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count");
		}
		if (offset >= this._wrapper.EofOffset)
		{
			return 0;
		}
		int startIdx = this.EnsureAvailable(offset, ref count, isRecursion: false);
		Buffer.BlockCopy(this._data, startIdx, buffer, index, count);
		return count;
	}

	internal int ReadByte(long offset)
	{
		if (offset < 0)
		{
			throw new ArgumentOutOfRangeException("offset");
		}
		if (offset >= this._wrapper.EofOffset)
		{
			return -1;
		}
		int count = 1;
		int startIdx = this.EnsureAvailable(offset, ref count, isRecursion: false);
		if (count == 1)
		{
			return this._data[startIdx];
		}
		return -1;
	}

	private int EnsureAvailable(long offset, ref int count, bool isRecursion)
	{
		if (offset >= this._baseOffset && offset + count < this._baseOffset + this._end)
		{
			return (int)(offset - this._baseOffset);
		}
		if (count > this._maxSize)
		{
			throw new InvalidOperationException("Not enough room in the buffer!  Increase the maximum size and try again.");
		}
		this._versionCounter++;
		if (!isRecursion)
		{
			for (int i = 0; i < this._savedBuffers.Count; i++)
			{
				long tempS = this._savedBuffers[i].BaseOffset - offset;
				if ((tempS < 0 && this._savedBuffers[i].End + tempS > 0) || (tempS > 0 && count - tempS > 0))
				{
					this.SwapBuffers(this._savedBuffers[i]);
					return this.EnsureAvailable(offset, ref count, isRecursion: true);
				}
			}
		}
		while (this._savedBuffers.Count > 0 && this._savedBuffers[0].VersionSaved + 25 < this._versionCounter)
		{
			this._savedBuffers[0].Buffer = null;
			this._savedBuffers.RemoveAt(0);
		}
		if (offset < this._baseOffset && !this._wrapper.Source.CanSeek)
		{
			throw new InvalidOperationException("Cannot seek before buffer on forward-only streams!");
		}
		this.CalcBuffer(offset, count, out var readStart, out var readEnd);
		count = this.FillBuffer(offset, count, readStart, readEnd);
		return (int)(offset - this._baseOffset);
	}

	private void SaveBuffer()
	{
		this._savedBuffers.Add(new SavedBuffer
		{
			Buffer = this._data,
			BaseOffset = this._baseOffset,
			End = this._end,
			DiscardCount = this._discardCount,
			VersionSaved = this._versionCounter
		});
		this._data = null;
		this._end = 0;
		this._discardCount = 0;
	}

	private void CreateNewBuffer(long offset, int count)
	{
		this.SaveBuffer();
		this._data = new byte[Math.Min(2 << (int)Math.Log(count - 1, 2.0), this._maxSize)];
		this._baseOffset = offset;
	}

	private void SwapBuffers(SavedBuffer savedBuffer)
	{
		this._savedBuffers.Remove(savedBuffer);
		this.SaveBuffer();
		this._data = savedBuffer.Buffer;
		this._baseOffset = savedBuffer.BaseOffset;
		this._end = savedBuffer.End;
		this._discardCount = savedBuffer.DiscardCount;
	}

	private void CalcBuffer(long offset, int count, out int readStart, out int readEnd)
	{
		readStart = 0;
		readEnd = 0;
		if (offset < this._baseOffset)
		{
			if (offset + this._maxSize <= this._baseOffset)
			{
				if (this._baseOffset - (offset + this._maxSize) > this._maxSize)
				{
					this.CreateNewBuffer(offset, count);
				}
				else
				{
					this.EnsureBufferSize(count, copyContents: false, 0);
				}
				this._baseOffset = offset;
				readEnd = count;
			}
			else
			{
				readEnd = (int)(offset - this._baseOffset);
				this.EnsureBufferSize(Math.Min((int)(offset + this._maxSize - this._baseOffset), this._end) - readEnd, copyContents: true, readEnd);
				readEnd = (int)(offset - this._baseOffset) - readEnd;
			}
		}
		else if (offset >= this._baseOffset + this._maxSize)
		{
			if (offset - (this._baseOffset + this._maxSize) > this._maxSize)
			{
				this.CreateNewBuffer(offset, count);
			}
			else
			{
				this.EnsureBufferSize(count, copyContents: false, 0);
			}
			this._baseOffset = offset;
			readEnd = count;
		}
		else
		{
			readEnd = (int)(offset + count - this._baseOffset);
			int ofs = Math.Max(readEnd - this._maxSize, 0);
			this.EnsureBufferSize(readEnd - ofs, copyContents: true, ofs);
			readStart = this._end;
			readEnd = (int)(offset + count - this._baseOffset);
		}
	}

	private void EnsureBufferSize(int reqSize, bool copyContents, int copyOffset)
	{
		byte[] newBuf = this._data;
		if (reqSize > this._data.Length)
		{
			if (reqSize > this._maxSize)
			{
				if (!this._wrapper.Source.CanSeek && reqSize - this._discardCount > this._maxSize)
				{
					throw new InvalidOperationException("Not enough room in the buffer!  Increase the maximum size and try again.");
				}
				int ofs = reqSize - this._maxSize;
				copyOffset += ofs;
				reqSize = this._maxSize;
			}
			else
			{
				int size;
				for (size = this._data.Length; size < reqSize; size *= 2)
				{
				}
				reqSize = size;
			}
			if (reqSize > this._data.Length)
			{
				newBuf = new byte[reqSize];
			}
		}
		if (copyContents)
		{
			if ((copyOffset > 0 && copyOffset < this._end) || (copyOffset == 0 && newBuf != this._data))
			{
				Buffer.BlockCopy(this._data, copyOffset, newBuf, 0, this._end - copyOffset);
				if ((this._discardCount -= copyOffset) < 0)
				{
					this._discardCount = 0;
				}
			}
			else if (copyOffset < 0 && -copyOffset < this._end)
			{
				if (newBuf != this._data || this._end <= -copyOffset)
				{
					Buffer.BlockCopy(this._data, 0, newBuf, -copyOffset, Math.Max(this._end, Math.Min(this._end, this._data.Length + copyOffset)));
				}
				else
				{
					this._end = copyOffset;
				}
				this._discardCount = 0;
			}
			else
			{
				this._end = copyOffset;
				this._discardCount = 0;
			}
			this._baseOffset += copyOffset;
			this._end -= copyOffset;
			if (this._end > newBuf.Length)
			{
				this._end = newBuf.Length;
			}
		}
		else
		{
			this._discardCount = 0;
			this._end = 0;
		}
		this._data = newBuf;
	}

	private int FillBuffer(long offset, int count, int readStart, int readEnd)
	{
		long readOffset = this._baseOffset + readStart;
		int readCount = readEnd - readStart;
		lock (this._wrapper.LockObject)
		{
			readCount = this.PrepareStreamForRead(readCount, readOffset);
			this.ReadStream(readStart, readCount, readOffset);
			if (this._end < readStart + readCount)
			{
				count = Math.Max(0, (int)(this._baseOffset + this._end - offset));
			}
			else if (!this._minimalRead && this._end < this._data.Length)
			{
				readCount = this._data.Length - this._end;
				readCount = this.PrepareStreamForRead(readCount, this._baseOffset + this._end);
				this._end += this._wrapper.Source.Read(this._data, this._end, readCount);
			}
		}
		return count;
	}

	private int PrepareStreamForRead(int readCount, long readOffset)
	{
		if (readCount > 0 && this._wrapper.Source.Position != readOffset)
		{
			if (readOffset < this._wrapper.EofOffset)
			{
				if (this._wrapper.Source.CanSeek)
				{
					this._wrapper.Source.Position = readOffset;
				}
				else
				{
					long seekCount = readOffset - this._wrapper.Source.Position;
					if (seekCount < 0)
					{
						readCount = 0;
					}
					else
					{
						while (--seekCount >= 0)
						{
							if (this._wrapper.Source.ReadByte() == -1)
							{
								this._wrapper.EofOffset = this._wrapper.Source.Position;
								readCount = 0;
								break;
							}
						}
					}
				}
			}
			else
			{
				readCount = 0;
			}
		}
		return readCount;
	}

	private void ReadStream(int readStart, int readCount, long readOffset)
	{
		while (readCount > 0 && readOffset < this._wrapper.EofOffset)
		{
			int temp = this._wrapper.Source.Read(this._data, readStart, readCount);
			if (temp == 0)
			{
				break;
			}
			readStart += temp;
			readOffset += temp;
			readCount -= temp;
		}
		if (readStart > this._end)
		{
			this._end = readStart;
		}
	}

	/// <summary>
	/// Tells the buffer that it no longer needs to maintain any bytes before the indicated offset.
	/// </summary>
	/// <param name="offset">The offset to discard through.</param>
	public void DiscardThrough(long offset)
	{
		int count = (int)(offset - this._baseOffset);
		this._discardCount = Math.Max(count, this._discardCount);
		if (this._discardCount >= this._data.Length)
		{
			this.CommitDiscard();
		}
	}

	private void CommitDiscard()
	{
		if (this._discardCount >= this._data.Length || this._discardCount >= this._end)
		{
			this._baseOffset += this._discardCount;
			this._end = 0;
		}
		else
		{
			Buffer.BlockCopy(this._data, this._discardCount, this._data, 0, this._end - this._discardCount);
			this._baseOffset += this._discardCount;
			this._end -= this._discardCount;
		}
		this._discardCount = 0;
	}
}
