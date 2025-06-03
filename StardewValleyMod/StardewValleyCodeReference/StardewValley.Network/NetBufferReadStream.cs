using System;
using System.IO;
using Lidgren.Network;

namespace StardewValley.Network;

public class NetBufferReadStream : Stream
{
	private long offset;

	public NetBuffer Buffer;

	public override bool CanRead => true;

	public override bool CanSeek => true;

	public override bool CanWrite => false;

	public override long Length => (this.Buffer.LengthBits - this.offset) / 8;

	public override long Position
	{
		get
		{
			return (this.Buffer.Position - this.offset) / 8;
		}
		set
		{
			this.Buffer.Position = this.offset + value * 8;
		}
	}

	public NetBufferReadStream(NetBuffer buffer)
	{
		this.Buffer = buffer;
		this.offset = buffer.Position;
	}

	public override void Flush()
	{
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		this.Buffer.ReadBytes(buffer, offset, count);
		return count;
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		switch (origin)
		{
		case SeekOrigin.Begin:
			this.Position = offset;
			break;
		case SeekOrigin.Current:
			this.Position += offset;
			break;
		case SeekOrigin.End:
			this.Position = this.Length + offset;
			break;
		}
		return this.Position;
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
