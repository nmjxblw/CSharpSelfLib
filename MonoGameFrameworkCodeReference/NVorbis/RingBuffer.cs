using System;

namespace NVorbis;

internal class RingBuffer
{
	private float[] _buffer;

	private int _start;

	private int _end;

	private int _bufLen;

	internal int Channels;

	internal int Length
	{
		get
		{
			int temp = this._end - this._start;
			if (temp < 0)
			{
				temp += this._bufLen;
			}
			return temp;
		}
	}

	internal RingBuffer(int size)
	{
		this._buffer = new float[size];
		this._start = (this._end = 0);
		this._bufLen = size;
	}

	internal void EnsureSize(int size)
	{
		size += this.Channels;
		if (this._bufLen < size)
		{
			float[] temp = new float[size];
			Array.Copy(this._buffer, this._start, temp, 0, this._bufLen - this._start);
			if (this._end < this._start)
			{
				Array.Copy(this._buffer, 0, temp, this._bufLen - this._start, this._end);
			}
			int end = this.Length;
			this._start = 0;
			this._end = end;
			this._buffer = temp;
			this._bufLen = size;
		}
	}

	internal void CopyTo(float[] buffer, int index, int count)
	{
		if (index < 0 || index + count > buffer.Length)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		int start = this._start;
		this.RemoveItems(count);
		int len = (this._end - start + this._bufLen) % this._bufLen;
		if (count > len)
		{
			throw new ArgumentOutOfRangeException("count");
		}
		int cnt = Math.Min(count, this._bufLen - start);
		Buffer.BlockCopy(this._buffer, start * 4, buffer, index * 4, cnt * 4);
		if (cnt < count)
		{
			Buffer.BlockCopy(this._buffer, 0, buffer, (index + cnt) * 4, (count - cnt) * 4);
		}
	}

	internal void RemoveItems(int count)
	{
		int cnt = (count + this._start) % this._bufLen;
		if (this._end > this._start)
		{
			if (cnt > this._end || cnt < this._start)
			{
				throw new ArgumentOutOfRangeException();
			}
		}
		else if (cnt < this._start && cnt > this._end)
		{
			throw new ArgumentOutOfRangeException();
		}
		this._start = cnt;
	}

	internal void Clear()
	{
		this._start = (this._end = 0);
	}

	internal void Write(int channel, int index, int start, int switchPoint, int end, float[] pcm, float[] window)
	{
		int idx;
		for (idx = (index + start) * this.Channels + channel + this._start; idx >= this._bufLen; idx -= this._bufLen)
		{
		}
		if (idx < 0)
		{
			start -= index;
			idx = channel;
		}
		while (idx < this._bufLen && start < switchPoint)
		{
			this._buffer[idx] += pcm[start] * window[start];
			idx += this.Channels;
			start++;
		}
		if (idx >= this._bufLen)
		{
			idx -= this._bufLen;
			while (start < switchPoint)
			{
				this._buffer[idx] += pcm[start] * window[start];
				idx += this.Channels;
				start++;
			}
		}
		while (idx < this._bufLen && start < end)
		{
			this._buffer[idx] = pcm[start] * window[start];
			idx += this.Channels;
			start++;
		}
		if (idx >= this._bufLen)
		{
			idx -= this._bufLen;
			while (start < end)
			{
				this._buffer[idx] = pcm[start] * window[start];
				idx += this.Channels;
				start++;
			}
		}
		this._end = idx;
	}
}
