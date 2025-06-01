using System.Collections.Generic;

namespace MonoGame.Framework.Utilities;

internal class ByteBufferPool
{
	private readonly int _minBufferSize;

	private readonly int _maxBuffers;

	private readonly List<byte[]> _freeBuffers;

	public int FreeAmount => this._freeBuffers.Count;

	public ByteBufferPool(int minBufferSize = 0, int maxBuffers = int.MaxValue)
	{
		this._minBufferSize = minBufferSize;
		this._maxBuffers = maxBuffers;
		this._freeBuffers = new List<byte[]>();
	}

	/// <summary>
	/// Get a buffer that is at least as big as size.
	/// </summary>
	public byte[] Get(int size)
	{
		if (size < this._minBufferSize)
		{
			size = this._minBufferSize;
		}
		byte[] result;
		lock (this._freeBuffers)
		{
			int index = this.FirstLargerThan(size);
			if (index == -1)
			{
				if (this._freeBuffers.Count > 0)
				{
					this._freeBuffers.RemoveAt(0);
				}
				result = new byte[size];
			}
			else
			{
				result = this._freeBuffers[index];
				this._freeBuffers.RemoveAt(index);
			}
		}
		return result;
	}

	/// <summary>
	/// Return the given buffer to the pool.
	/// </summary>
	public void Return(byte[] buffer)
	{
		lock (this._freeBuffers)
		{
			if (this.FreeAmount < this._maxBuffers)
			{
				int index = this.FirstLargerThan(buffer.Length);
				if (index == -1)
				{
					this._freeBuffers.Add(buffer);
				}
				else
				{
					this._freeBuffers.Insert(index, buffer);
				}
			}
		}
	}

	private int FirstLargerThan(int size)
	{
		if (this._freeBuffers.Count == 0)
		{
			return -1;
		}
		int l = 0;
		int r = this._freeBuffers.Count - 1;
		while (l <= r)
		{
			int m = (l + r) / 2;
			byte[] buffer = this._freeBuffers[m];
			if (buffer.Length < size)
			{
				l = m + 1;
				continue;
			}
			if (buffer.Length > size)
			{
				r = m;
				if (l != r)
				{
					continue;
				}
				return l;
			}
			return m;
		}
		return -1;
	}
}
