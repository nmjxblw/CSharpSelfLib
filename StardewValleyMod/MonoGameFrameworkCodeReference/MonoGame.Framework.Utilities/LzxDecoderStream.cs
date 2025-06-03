using System;
using System.IO;
using Microsoft.Xna.Framework.Content;

namespace MonoGame.Framework.Utilities;

internal class LzxDecoderStream : Stream
{
	private LzxDecoder dec;

	private MemoryStream decompressedStream;

	public override bool CanRead => true;

	public override bool CanSeek => false;

	public override bool CanWrite => false;

	public override long Length
	{
		get
		{
			throw new NotSupportedException();
		}
	}

	public override long Position
	{
		get
		{
			throw new NotSupportedException();
		}
		set
		{
			throw new NotSupportedException();
		}
	}

	public LzxDecoderStream(Stream input, int decompressedSize, int compressedSize)
	{
		this.dec = new LzxDecoder(16);
		this.Decompress(input, decompressedSize, compressedSize);
	}

	private void Decompress(Stream stream, int decompressedSize, int compressedSize)
	{
		this.decompressedStream = new MemoryStream(decompressedSize);
		long startPos = stream.Position;
		long pos = startPos;
		while (pos - startPos < compressedSize)
		{
			int num = stream.ReadByte();
			int lo = stream.ReadByte();
			int block_size = (num << 8) | lo;
			int frame_size = 32768;
			if (num == 255)
			{
				int num2 = lo;
				lo = (byte)stream.ReadByte();
				frame_size = (num2 << 8) | lo;
				byte num3 = (byte)stream.ReadByte();
				lo = (byte)stream.ReadByte();
				block_size = (num3 << 8) | lo;
				pos += 5;
			}
			else
			{
				pos += 2;
			}
			if (block_size == 0 || frame_size == 0)
			{
				break;
			}
			this.dec.Decompress(stream, block_size, this.decompressedStream, frame_size);
			pos += block_size;
			stream.Seek(pos, SeekOrigin.Begin);
		}
		if (this.decompressedStream.Position != decompressedSize)
		{
			throw new ContentLoadException("Decompression failed.");
		}
		this.decompressedStream.Seek(0L, SeekOrigin.Begin);
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
		if (disposing)
		{
			this.decompressedStream.Dispose();
		}
		this.dec = null;
		this.decompressedStream = null;
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		return this.decompressedStream.Read(buffer, offset, count);
	}

	public override void Flush()
	{
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		throw new NotImplementedException();
	}

	public override void SetLength(long value)
	{
		throw new NotImplementedException();
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		throw new NotImplementedException();
	}
}
