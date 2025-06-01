using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Ionic.Crc;

namespace Ionic.Zlib;

internal class ZlibBaseStream : Stream
{
	internal enum StreamMode
	{
		Writer,
		Reader,
		Undefined
	}

	protected internal ZlibCodec _z;

	protected internal StreamMode _streamMode = StreamMode.Undefined;

	protected internal FlushType _flushMode;

	protected internal ZlibStreamFlavor _flavor;

	protected internal CompressionMode _compressionMode;

	protected internal CompressionLevel _level;

	protected internal bool _leaveOpen;

	protected internal byte[] _workingBuffer;

	protected internal int _bufferSize = 16384;

	protected internal byte[] _buf1 = new byte[1];

	protected internal Stream _stream;

	protected internal CompressionStrategy Strategy;

	private CRC32 crc;

	protected internal string _GzipFileName;

	protected internal string _GzipComment;

	protected internal DateTime _GzipMtime;

	protected internal int _gzipHeaderByteCount;

	private bool nomoreinput;

	internal int Crc32
	{
		get
		{
			if (this.crc == null)
			{
				return 0;
			}
			return this.crc.Crc32Result;
		}
	}

	protected internal bool _wantCompress => this._compressionMode == CompressionMode.Compress;

	private ZlibCodec z
	{
		get
		{
			if (this._z == null)
			{
				bool wantRfc1950Header = this._flavor == ZlibStreamFlavor.ZLIB;
				this._z = new ZlibCodec();
				if (this._compressionMode == CompressionMode.Decompress)
				{
					this._z.InitializeInflate(wantRfc1950Header);
				}
				else
				{
					this._z.Strategy = this.Strategy;
					this._z.InitializeDeflate(this._level, wantRfc1950Header);
				}
			}
			return this._z;
		}
	}

	private byte[] workingBuffer
	{
		get
		{
			if (this._workingBuffer == null)
			{
				this._workingBuffer = new byte[this._bufferSize];
			}
			return this._workingBuffer;
		}
	}

	public override bool CanRead => this._stream.CanRead;

	public override bool CanSeek => this._stream.CanSeek;

	public override bool CanWrite => this._stream.CanWrite;

	public override long Length => this._stream.Length;

	public override long Position
	{
		get
		{
			throw new NotImplementedException();
		}
		set
		{
			throw new NotImplementedException();
		}
	}

	public ZlibBaseStream(Stream stream, CompressionMode compressionMode, CompressionLevel level, ZlibStreamFlavor flavor, bool leaveOpen)
	{
		this._flushMode = FlushType.None;
		this._stream = stream;
		this._leaveOpen = leaveOpen;
		this._compressionMode = compressionMode;
		this._flavor = flavor;
		this._level = level;
		if (flavor == ZlibStreamFlavor.GZIP)
		{
			this.crc = new CRC32();
		}
	}

	public override void Write(byte[] buffer, int offset, int count)
	{
		if (this.crc != null)
		{
			this.crc.SlurpBlock(buffer, offset, count);
		}
		if (this._streamMode == StreamMode.Undefined)
		{
			this._streamMode = StreamMode.Writer;
		}
		else if (this._streamMode != StreamMode.Writer)
		{
			throw new ZlibException("Cannot Write after Reading.");
		}
		if (count == 0)
		{
			return;
		}
		this.z.InputBuffer = buffer;
		this._z.NextIn = offset;
		this._z.AvailableBytesIn = count;
		bool done = false;
		do
		{
			this._z.OutputBuffer = this.workingBuffer;
			this._z.NextOut = 0;
			this._z.AvailableBytesOut = this._workingBuffer.Length;
			int rc = (this._wantCompress ? this._z.Deflate(this._flushMode) : this._z.Inflate(this._flushMode));
			if (rc != 0 && rc != 1)
			{
				throw new ZlibException((this._wantCompress ? "de" : "in") + "flating: " + this._z.Message);
			}
			this._stream.Write(this._workingBuffer, 0, this._workingBuffer.Length - this._z.AvailableBytesOut);
			done = this._z.AvailableBytesIn == 0 && this._z.AvailableBytesOut != 0;
			if (this._flavor == ZlibStreamFlavor.GZIP && !this._wantCompress)
			{
				done = this._z.AvailableBytesIn == 8 && this._z.AvailableBytesOut != 0;
			}
		}
		while (!done);
	}

	private void finish()
	{
		if (this._z == null)
		{
			return;
		}
		if (this._streamMode == StreamMode.Writer)
		{
			bool done = false;
			do
			{
				this._z.OutputBuffer = this.workingBuffer;
				this._z.NextOut = 0;
				this._z.AvailableBytesOut = this._workingBuffer.Length;
				int rc = (this._wantCompress ? this._z.Deflate(FlushType.Finish) : this._z.Inflate(FlushType.Finish));
				if (rc != 1 && rc != 0)
				{
					string verb = (this._wantCompress ? "de" : "in") + "flating";
					if (this._z.Message == null)
					{
						throw new ZlibException($"{verb}: (rc = {rc})");
					}
					throw new ZlibException(verb + ": " + this._z.Message);
				}
				if (this._workingBuffer.Length - this._z.AvailableBytesOut > 0)
				{
					this._stream.Write(this._workingBuffer, 0, this._workingBuffer.Length - this._z.AvailableBytesOut);
				}
				done = this._z.AvailableBytesIn == 0 && this._z.AvailableBytesOut != 0;
				if (this._flavor == ZlibStreamFlavor.GZIP && !this._wantCompress)
				{
					done = this._z.AvailableBytesIn == 8 && this._z.AvailableBytesOut != 0;
				}
			}
			while (!done);
			this.Flush();
			if (this._flavor == ZlibStreamFlavor.GZIP)
			{
				if (!this._wantCompress)
				{
					throw new ZlibException("Writing with decompression is not supported.");
				}
				int c1 = this.crc.Crc32Result;
				this._stream.Write(BitConverter.GetBytes(c1), 0, 4);
				int c2 = (int)(this.crc.TotalBytesRead & 0xFFFFFFFFu);
				this._stream.Write(BitConverter.GetBytes(c2), 0, 4);
			}
		}
		else
		{
			if (this._streamMode != StreamMode.Reader || this._flavor != ZlibStreamFlavor.GZIP)
			{
				return;
			}
			if (this._wantCompress)
			{
				throw new ZlibException("Reading with compression is not supported.");
			}
			if (this._z.TotalBytesOut == 0L)
			{
				return;
			}
			byte[] trailer = new byte[8];
			if (this._z.AvailableBytesIn < 8)
			{
				Array.Copy(this._z.InputBuffer, this._z.NextIn, trailer, 0, this._z.AvailableBytesIn);
				int bytesNeeded = 8 - this._z.AvailableBytesIn;
				int bytesRead = this._stream.Read(trailer, this._z.AvailableBytesIn, bytesNeeded);
				if (bytesNeeded != bytesRead)
				{
					throw new ZlibException($"Missing or incomplete GZIP trailer. Expected 8 bytes, got {this._z.AvailableBytesIn + bytesRead}.");
				}
			}
			else
			{
				Array.Copy(this._z.InputBuffer, this._z.NextIn, trailer, 0, trailer.Length);
			}
			int crc32_expected = BitConverter.ToInt32(trailer, 0);
			int crc32_actual = this.crc.Crc32Result;
			int isize_expected = BitConverter.ToInt32(trailer, 4);
			int isize_actual = (int)(this._z.TotalBytesOut & 0xFFFFFFFFu);
			if (crc32_actual != crc32_expected)
			{
				throw new ZlibException($"Bad CRC32 in GZIP trailer. (actual({crc32_actual:X8})!=expected({crc32_expected:X8}))");
			}
			if (isize_actual != isize_expected)
			{
				throw new ZlibException($"Bad size in GZIP trailer. (actual({isize_actual})!=expected({isize_expected}))");
			}
		}
	}

	private void end()
	{
		if (this.z != null)
		{
			if (this._wantCompress)
			{
				this._z.EndDeflate();
			}
			else
			{
				this._z.EndInflate();
			}
			this._z = null;
		}
	}

	public override void Close()
	{
		if (this._stream == null)
		{
			return;
		}
		try
		{
			this.finish();
		}
		finally
		{
			this.end();
			if (!this._leaveOpen)
			{
				this._stream.Close();
			}
			this._stream = null;
		}
	}

	public override void Flush()
	{
		this._stream.Flush();
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		throw new NotImplementedException();
	}

	public override void SetLength(long value)
	{
		this._stream.SetLength(value);
	}

	private string ReadZeroTerminatedString()
	{
		List<byte> list = new List<byte>();
		bool done = false;
		do
		{
			if (this._stream.Read(this._buf1, 0, 1) != 1)
			{
				throw new ZlibException("Unexpected EOF reading GZIP header.");
			}
			if (this._buf1[0] == 0)
			{
				done = true;
			}
			else
			{
				list.Add(this._buf1[0]);
			}
		}
		while (!done);
		byte[] a = list.ToArray();
		return GZipStream.iso8859dash1.GetString(a, 0, a.Length);
	}

	private int _ReadAndValidateGzipHeader()
	{
		int totalBytesRead = 0;
		byte[] header = new byte[10];
		int n = this._stream.Read(header, 0, header.Length);
		switch (n)
		{
		case 0:
			return 0;
		default:
			throw new ZlibException("Not a valid GZIP stream.");
		case 10:
		{
			if (header[0] != 31 || header[1] != 139 || header[2] != 8)
			{
				throw new ZlibException("Bad GZIP header.");
			}
			int timet = BitConverter.ToInt32(header, 4);
			this._GzipMtime = GZipStream._unixEpoch.AddSeconds(timet);
			totalBytesRead += n;
			if ((header[3] & 4) == 4)
			{
				n = this._stream.Read(header, 0, 2);
				totalBytesRead += n;
				short extraLength = (short)(header[0] + header[1] * 256);
				byte[] extra = new byte[extraLength];
				n = this._stream.Read(extra, 0, extra.Length);
				if (n != extraLength)
				{
					throw new ZlibException("Unexpected end-of-file reading GZIP header.");
				}
				totalBytesRead += n;
			}
			if ((header[3] & 8) == 8)
			{
				this._GzipFileName = this.ReadZeroTerminatedString();
			}
			if ((header[3] & 0x10) == 16)
			{
				this._GzipComment = this.ReadZeroTerminatedString();
			}
			if ((header[3] & 2) == 2)
			{
				this.Read(this._buf1, 0, 1);
			}
			return totalBytesRead;
		}
		}
	}

	public override int Read(byte[] buffer, int offset, int count)
	{
		if (this._streamMode == StreamMode.Undefined)
		{
			if (!this._stream.CanRead)
			{
				throw new ZlibException("The stream is not readable.");
			}
			this._streamMode = StreamMode.Reader;
			this.z.AvailableBytesIn = 0;
			if (this._flavor == ZlibStreamFlavor.GZIP)
			{
				this._gzipHeaderByteCount = this._ReadAndValidateGzipHeader();
				if (this._gzipHeaderByteCount == 0)
				{
					return 0;
				}
			}
		}
		if (this._streamMode != StreamMode.Reader)
		{
			throw new ZlibException("Cannot Read after Writing.");
		}
		if (count == 0)
		{
			return 0;
		}
		if (this.nomoreinput && this._wantCompress)
		{
			return 0;
		}
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer");
		}
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count");
		}
		if (offset < buffer.GetLowerBound(0))
		{
			throw new ArgumentOutOfRangeException("offset");
		}
		if (offset + count > buffer.GetLength(0))
		{
			throw new ArgumentOutOfRangeException("count");
		}
		int rc = 0;
		this._z.OutputBuffer = buffer;
		this._z.NextOut = offset;
		this._z.AvailableBytesOut = count;
		this._z.InputBuffer = this.workingBuffer;
		do
		{
			if (this._z.AvailableBytesIn == 0 && !this.nomoreinput)
			{
				this._z.NextIn = 0;
				this._z.AvailableBytesIn = this._stream.Read(this._workingBuffer, 0, this._workingBuffer.Length);
				if (this._z.AvailableBytesIn == 0)
				{
					this.nomoreinput = true;
				}
			}
			rc = (this._wantCompress ? this._z.Deflate(this._flushMode) : this._z.Inflate(this._flushMode));
			if (this.nomoreinput && rc == -5)
			{
				return 0;
			}
			if (rc != 0 && rc != 1)
			{
				throw new ZlibException(string.Format("{0}flating:  rc={1}  msg={2}", this._wantCompress ? "de" : "in", rc, this._z.Message));
			}
		}
		while (((!this.nomoreinput && rc != 1) || this._z.AvailableBytesOut != count) && this._z.AvailableBytesOut > 0 && !this.nomoreinput && rc == 0);
		if (this._z.AvailableBytesOut > 0)
		{
			if (rc == 0)
			{
				_ = this._z.AvailableBytesIn;
			}
			if (this.nomoreinput && this._wantCompress)
			{
				rc = this._z.Deflate(FlushType.Finish);
				if (rc != 0 && rc != 1)
				{
					throw new ZlibException($"Deflating:  rc={rc}  msg={this._z.Message}");
				}
			}
		}
		rc = count - this._z.AvailableBytesOut;
		if (this.crc != null)
		{
			this.crc.SlurpBlock(buffer, offset, rc);
		}
		return rc;
	}

	public static void CompressString(string s, Stream compressor)
	{
		byte[] uncompressed = Encoding.UTF8.GetBytes(s);
		using (compressor)
		{
			compressor.Write(uncompressed, 0, uncompressed.Length);
		}
	}

	public static void CompressBuffer(byte[] b, Stream compressor)
	{
		using (compressor)
		{
			compressor.Write(b, 0, b.Length);
		}
	}

	public static string UncompressString(byte[] compressed, Stream decompressor)
	{
		byte[] working = new byte[1024];
		Encoding encoding = Encoding.UTF8;
		using MemoryStream output = new MemoryStream();
		using (decompressor)
		{
			int n;
			while ((n = decompressor.Read(working, 0, working.Length)) != 0)
			{
				output.Write(working, 0, n);
			}
		}
		output.Seek(0L, SeekOrigin.Begin);
		return new StreamReader(output, encoding).ReadToEnd();
	}

	public static byte[] UncompressBuffer(byte[] compressed, Stream decompressor)
	{
		byte[] working = new byte[1024];
		using MemoryStream output = new MemoryStream();
		using (decompressor)
		{
			int n;
			while ((n = decompressor.Read(working, 0, working.Length)) != 0)
			{
				output.Write(working, 0, n);
			}
		}
		return output.ToArray();
	}
}
