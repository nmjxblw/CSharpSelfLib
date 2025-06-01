using System;
using System.IO;

namespace MonoGame.Framework.Utilities;

internal class Lz4DecoderStream : Stream
{
	private enum DecodePhase
	{
		ReadToken,
		ReadExLiteralLength,
		CopyLiteral,
		ReadOffset,
		ReadExMatchLength,
		CopyMatch
	}

	private long inputLength;

	private Stream input;

	private const int DecBufLen = 65536;

	private const int DecBufMask = 65535;

	private const int InBufLen = 128;

	private byte[] decodeBuffer = new byte[65664];

	private int decodeBufferPos;

	private int inBufPos;

	private int inBufEnd;

	private DecodePhase phase;

	private int litLen;

	private int matLen;

	private int matDst;

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

	public Lz4DecoderStream()
	{
	}

	public Lz4DecoderStream(Stream input, long inputLength = long.MaxValue)
	{
		this.Reset(input, inputLength);
	}

	public void Reset(Stream input, long inputLength = long.MaxValue)
	{
		this.inputLength = inputLength;
		this.input = input;
		this.phase = DecodePhase.ReadToken;
		this.decodeBufferPos = 0;
		this.litLen = 0;
		this.matLen = 0;
		this.matDst = 0;
		this.inBufPos = 65536;
		this.inBufEnd = 65536;
	}

	protected override void Dispose(bool disposing)
	{
		this.input = null;
		base.Dispose(disposing);
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer");
		}
		if (offset < 0 || count < 0 || buffer.Length - count < offset)
		{
			throw new ArgumentOutOfRangeException();
		}
		if (this.input == null)
		{
			throw new InvalidOperationException();
		}
		int nToRead = count;
		byte[] decBuf = this.decodeBuffer;
		int nRead;
		switch (this.phase)
		{
		default:
		{
			int tok;
			if (this.inBufPos < this.inBufEnd)
			{
				tok = decBuf[this.inBufPos++];
			}
			else
			{
				tok = this.ReadByteCore();
				if (tok == -1)
				{
					break;
				}
			}
			this.litLen = tok >> 4;
			this.matLen = (tok & 0xF) + 4;
			int num = this.litLen;
			if (num != 0)
			{
				if (num == 15)
				{
					this.phase = DecodePhase.ReadExLiteralLength;
					goto case DecodePhase.ReadExLiteralLength;
				}
				this.phase = DecodePhase.CopyLiteral;
				goto case DecodePhase.CopyLiteral;
			}
			this.phase = DecodePhase.ReadOffset;
			goto case DecodePhase.ReadOffset;
		}
		case DecodePhase.ReadExLiteralLength:
			while (true)
			{
				int exLitLen;
				if (this.inBufPos < this.inBufEnd)
				{
					exLitLen = decBuf[this.inBufPos++];
				}
				else
				{
					exLitLen = this.ReadByteCore();
					if (exLitLen == -1)
					{
						break;
					}
				}
				this.litLen += exLitLen;
				if (exLitLen == 255)
				{
					continue;
				}
				goto IL_012e;
			}
			break;
		case DecodePhase.CopyLiteral:
			do
			{
				int nReadLit = ((this.litLen < nToRead) ? this.litLen : nToRead);
				if (nReadLit == 0)
				{
					break;
				}
				if (this.inBufPos + nReadLit <= this.inBufEnd)
				{
					int ofs = offset;
					int c = nReadLit;
					while (c-- != 0)
					{
						buffer[ofs++] = decBuf[this.inBufPos++];
					}
					nRead = nReadLit;
				}
				else
				{
					nRead = this.ReadCore(buffer, offset, nReadLit);
					if (nRead == 0)
					{
						goto end_IL_0045;
					}
				}
				offset += nRead;
				nToRead -= nRead;
				this.litLen -= nRead;
			}
			while (this.litLen != 0);
			if (nToRead == 0)
			{
				break;
			}
			this.phase = DecodePhase.ReadOffset;
			goto case DecodePhase.ReadOffset;
		case DecodePhase.ReadOffset:
			if (this.inBufPos + 1 < this.inBufEnd)
			{
				this.matDst = (decBuf[this.inBufPos + 1] << 8) | decBuf[this.inBufPos];
				this.inBufPos += 2;
			}
			else
			{
				this.matDst = this.ReadOffsetCore();
				if (this.matDst == -1)
				{
					break;
				}
			}
			if (this.matLen == 19)
			{
				this.phase = DecodePhase.ReadExMatchLength;
				goto case DecodePhase.ReadExMatchLength;
			}
			this.phase = DecodePhase.CopyMatch;
			goto case DecodePhase.CopyMatch;
		case DecodePhase.ReadExMatchLength:
			while (true)
			{
				int exMatLen;
				if (this.inBufPos < this.inBufEnd)
				{
					exMatLen = decBuf[this.inBufPos++];
				}
				else
				{
					exMatLen = this.ReadByteCore();
					if (exMatLen == -1)
					{
						break;
					}
				}
				this.matLen += exMatLen;
				if (exMatLen == 255)
				{
					continue;
				}
				goto IL_0293;
			}
			break;
		case DecodePhase.CopyMatch:
			{
				int nCpyMat = ((this.matLen < nToRead) ? this.matLen : nToRead);
				if (nCpyMat != 0)
				{
					nRead = count - nToRead;
					int bufDst = this.matDst - nRead;
					if (bufDst > 0)
					{
						int bufSrc = this.decodeBufferPos - bufDst;
						if (bufSrc < 0)
						{
							bufSrc += 65536;
						}
						int c2 = ((bufDst < nCpyMat) ? bufDst : nCpyMat);
						while (c2-- != 0)
						{
							buffer[offset++] = decBuf[bufSrc++ & 0xFFFF];
						}
					}
					else
					{
						bufDst = 0;
					}
					int sOfs = offset - this.matDst;
					for (int i = bufDst; i < nCpyMat; i++)
					{
						buffer[offset++] = buffer[sOfs++];
					}
					nToRead -= nCpyMat;
					this.matLen -= nCpyMat;
				}
				if (nToRead == 0)
				{
					break;
				}
				this.phase = DecodePhase.ReadToken;
				goto default;
			}
			IL_0293:
			this.phase = DecodePhase.CopyMatch;
			goto case DecodePhase.CopyMatch;
			IL_012e:
			this.phase = DecodePhase.CopyLiteral;
			goto case DecodePhase.CopyLiteral;
			end_IL_0045:
			break;
		}
		nRead = count - nToRead;
		int nToBuf = ((nRead < 65536) ? nRead : 65536);
		int repPos = offset - nToBuf;
		if (nToBuf == 65536)
		{
			Buffer.BlockCopy(buffer, repPos, decBuf, 0, 65536);
			this.decodeBufferPos = 0;
		}
		else
		{
			int decPos = this.decodeBufferPos;
			while (nToBuf-- != 0)
			{
				decBuf[decPos++ & 0xFFFF] = buffer[repPos++];
			}
			this.decodeBufferPos = decPos & 0xFFFF;
		}
		return nRead;
	}

	private int ReadByteCore()
	{
		byte[] buf = this.decodeBuffer;
		if (this.inBufPos == this.inBufEnd)
		{
			int nRead = this.input.Read(buf, 65536, (int)((128 < this.inputLength) ? 128 : this.inputLength));
			if (nRead == 0)
			{
				return -1;
			}
			this.inputLength -= nRead;
			this.inBufPos = 65536;
			this.inBufEnd = 65536 + nRead;
		}
		return buf[this.inBufPos++];
	}

	private int ReadOffsetCore()
	{
		byte[] buf = this.decodeBuffer;
		if (this.inBufPos == this.inBufEnd)
		{
			int nRead = this.input.Read(buf, 65536, (int)((128 < this.inputLength) ? 128 : this.inputLength));
			if (nRead == 0)
			{
				return -1;
			}
			this.inputLength -= nRead;
			this.inBufPos = 65536;
			this.inBufEnd = 65536 + nRead;
		}
		if (this.inBufEnd - this.inBufPos == 1)
		{
			buf[65536] = buf[this.inBufPos];
			int nRead2 = this.input.Read(buf, 65537, (int)((127 < this.inputLength) ? 127 : this.inputLength));
			if (nRead2 == 0)
			{
				this.inBufPos = 65536;
				this.inBufEnd = 65537;
				return -1;
			}
			this.inputLength -= nRead2;
			this.inBufPos = 65536;
			this.inBufEnd = 65536 + nRead2 + 1;
		}
		int result = (buf[this.inBufPos + 1] << 8) | buf[this.inBufPos];
		this.inBufPos += 2;
		return result;
	}

	private int ReadCore(byte[] buffer, int offset, int count)
	{
		int nToRead = count;
		byte[] buf = this.decodeBuffer;
		int inBufLen = this.inBufEnd - this.inBufPos;
		int fromBuf = ((nToRead < inBufLen) ? nToRead : inBufLen);
		if (fromBuf != 0)
		{
			int bufPos = this.inBufPos;
			int c = fromBuf;
			while (c-- != 0)
			{
				buffer[offset++] = buf[bufPos++];
			}
			this.inBufPos = bufPos;
			nToRead -= fromBuf;
		}
		if (nToRead != 0)
		{
			int nRead;
			if (nToRead >= 128)
			{
				nRead = this.input.Read(buffer, offset, (int)((nToRead < this.inputLength) ? nToRead : this.inputLength));
				nToRead -= nRead;
			}
			else
			{
				nRead = this.input.Read(buf, 65536, (int)((128 < this.inputLength) ? 128 : this.inputLength));
				this.inBufPos = 65536;
				this.inBufEnd = 65536 + nRead;
				fromBuf = ((nToRead < nRead) ? nToRead : nRead);
				int bufPos2 = this.inBufPos;
				int c2 = fromBuf;
				while (c2-- != 0)
				{
					buffer[offset++] = buf[bufPos2++];
				}
				this.inBufPos = bufPos2;
				nToRead -= fromBuf;
			}
			this.inputLength -= nRead;
		}
		return count - nToRead;
	}

	public override void Flush()
	{
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		throw new NotSupportedException();
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
