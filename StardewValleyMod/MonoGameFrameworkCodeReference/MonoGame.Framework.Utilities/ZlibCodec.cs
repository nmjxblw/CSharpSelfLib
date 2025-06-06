using System;

namespace MonoGame.Framework.Utilities;

/// <summary>
/// Encoder and Decoder for ZLIB and DEFLATE (IETF RFC1950 and RFC1951).
/// </summary>
///
/// <remarks>
/// This class compresses and decompresses data according to the Deflate algorithm
/// and optionally, the ZLIB format, as documented in <see href="http://www.ietf.org/rfc/rfc1950.txt">RFC 1950 - ZLIB</see> and <see href="http://www.ietf.org/rfc/rfc1951.txt">RFC 1951 - DEFLATE</see>.
/// </remarks>
internal sealed class ZlibCodec
{
	/// <summary>
	/// The buffer from which data is taken.
	/// </summary>
	internal byte[] InputBuffer;

	/// <summary>
	/// An index into the InputBuffer array, indicating where to start reading. 
	/// </summary>
	internal int NextIn;

	/// <summary>
	/// The number of bytes available in the InputBuffer, starting at NextIn. 
	/// </summary>
	/// <remarks>
	/// Generally you should set this to InputBuffer.Length before the first Inflate() or Deflate() call. 
	/// The class will update this number as calls to Inflate/Deflate are made.
	/// </remarks>
	internal int AvailableBytesIn;

	/// <summary>
	/// Total number of bytes read so far, through all calls to Inflate()/Deflate().
	/// </summary>
	internal long TotalBytesIn;

	/// <summary>
	/// Buffer to store output data.
	/// </summary>
	internal byte[] OutputBuffer;

	/// <summary>
	/// An index into the OutputBuffer array, indicating where to start writing. 
	/// </summary>
	internal int NextOut;

	/// <summary>
	/// The number of bytes available in the OutputBuffer, starting at NextOut. 
	/// </summary>
	/// <remarks>
	/// Generally you should set this to OutputBuffer.Length before the first Inflate() or Deflate() call. 
	/// The class will update this number as calls to Inflate/Deflate are made.
	/// </remarks>
	internal int AvailableBytesOut;

	/// <summary>
	/// Total number of bytes written to the output so far, through all calls to Inflate()/Deflate().
	/// </summary>
	internal long TotalBytesOut;

	/// <summary>
	/// used for diagnostics, when something goes wrong!
	/// </summary>
	internal string Message;

	internal DeflateManager dstate;

	internal InflateManager istate;

	internal uint _Adler32;

	/// <summary>
	/// The compression level to use in this codec.  Useful only in compression mode.
	/// </summary>
	internal CompressionLevel CompressLevel = CompressionLevel.Default;

	/// <summary>
	/// The number of Window Bits to use.  
	/// </summary>
	/// <remarks>
	/// This gauges the size of the sliding window, and hence the 
	/// compression effectiveness as well as memory consumption. It's best to just leave this 
	/// setting alone if you don't know what it is.  The maximum value is 15 bits, which implies
	/// a 32k window.  
	/// </remarks>
	internal int WindowBits = 15;

	/// <summary>
	/// The compression strategy to use.
	/// </summary>
	/// <remarks>
	/// This is only effective in compression.  The theory offered by ZLIB is that different
	/// strategies could potentially produce significant differences in compression behavior
	/// for different data sets.  Unfortunately I don't have any good recommendations for how
	/// to set it differently.  When I tested changing the strategy I got minimally different
	/// compression performance. It's best to leave this property alone if you don't have a
	/// good feel for it.  Or, you may want to produce a test harness that runs through the
	/// different strategy options and evaluates them on different file types. If you do that,
	/// let me know your results.
	/// </remarks>
	internal CompressionStrategy Strategy;

	/// <summary>
	/// The Adler32 checksum on the data transferred through the codec so far. You probably don't need to look at this.
	/// </summary>
	internal int Adler32 => (int)this._Adler32;

	/// <summary>
	/// Create a ZlibCodec.
	/// </summary>
	/// <remarks>
	/// If you use this default constructor, you will later have to explicitly call 
	/// InitializeInflate() or InitializeDeflate() before using the ZlibCodec to compress 
	/// or decompress. 
	/// </remarks>
	internal ZlibCodec()
	{
	}

	/// <summary>
	/// Create a ZlibCodec that either compresses or decompresses.
	/// </summary>
	/// <param name="mode">
	/// Indicates whether the codec should compress (deflate) or decompress (inflate).
	/// </param>
	internal ZlibCodec(CompressionMode mode)
	{
		switch (mode)
		{
		case CompressionMode.Compress:
			if (this.InitializeDeflate() != 0)
			{
				throw new ZlibException("Cannot initialize for deflate.");
			}
			break;
		case CompressionMode.Decompress:
			if (this.InitializeInflate() != 0)
			{
				throw new ZlibException("Cannot initialize for inflate.");
			}
			break;
		default:
			throw new ZlibException("Invalid ZlibStreamFlavor.");
		}
	}

	/// <summary>
	/// Initialize the inflation state. 
	/// </summary>
	/// <remarks>
	/// It is not necessary to call this before using the ZlibCodec to inflate data; 
	/// It is implicitly called when you call the constructor.
	/// </remarks>
	/// <returns>Z_OK if everything goes well.</returns>
	internal int InitializeInflate()
	{
		return this.InitializeInflate(this.WindowBits);
	}

	/// <summary>
	/// Initialize the inflation state with an explicit flag to
	/// govern the handling of RFC1950 header bytes.
	/// </summary>
	///
	/// <remarks>
	/// By default, the ZLIB header defined in <see href="http://www.ietf.org/rfc/rfc1950.txt">RFC 1950</see> is expected.  If
	/// you want to read a zlib stream you should specify true for
	/// expectRfc1950Header.  If you have a deflate stream, you will want to specify
	/// false. It is only necessary to invoke this initializer explicitly if you
	/// want to specify false.
	/// </remarks>
	///
	/// <param name="expectRfc1950Header">whether to expect an RFC1950 header byte
	/// pair when reading the stream of data to be inflated.</param>
	///
	/// <returns>Z_OK if everything goes well.</returns>
	internal int InitializeInflate(bool expectRfc1950Header)
	{
		return this.InitializeInflate(this.WindowBits, expectRfc1950Header);
	}

	/// <summary>
	/// Initialize the ZlibCodec for inflation, with the specified number of window bits. 
	/// </summary>
	/// <param name="windowBits">The number of window bits to use. If you need to ask what that is, 
	/// then you shouldn't be calling this initializer.</param>
	/// <returns>Z_OK if all goes well.</returns>
	internal int InitializeInflate(int windowBits)
	{
		this.WindowBits = windowBits;
		return this.InitializeInflate(windowBits, expectRfc1950Header: true);
	}

	/// <summary>
	/// Initialize the inflation state with an explicit flag to govern the handling of
	/// RFC1950 header bytes. 
	/// </summary>
	///
	/// <remarks>
	/// If you want to read a zlib stream you should specify true for
	/// expectRfc1950Header. In this case, the library will expect to find a ZLIB
	/// header, as defined in <see href="http://www.ietf.org/rfc/rfc1950.txt">RFC
	/// 1950</see>, in the compressed stream.  If you will be reading a DEFLATE or
	/// GZIP stream, which does not have such a header, you will want to specify
	/// false.
	/// </remarks>
	///
	/// <param name="expectRfc1950Header">whether to expect an RFC1950 header byte pair when reading 
	/// the stream of data to be inflated.</param>
	/// <param name="windowBits">The number of window bits to use. If you need to ask what that is, 
	/// then you shouldn't be calling this initializer.</param>
	/// <returns>Z_OK if everything goes well.</returns>
	internal int InitializeInflate(int windowBits, bool expectRfc1950Header)
	{
		this.WindowBits = windowBits;
		if (this.dstate != null)
		{
			throw new ZlibException("You may not call InitializeInflate() after calling InitializeDeflate().");
		}
		this.istate = new InflateManager(expectRfc1950Header);
		return this.istate.Initialize(this, windowBits);
	}

	/// <summary>
	/// Inflate the data in the InputBuffer, placing the result in the OutputBuffer.
	/// </summary>
	/// <remarks>
	/// You must have set InputBuffer and OutputBuffer, NextIn and NextOut, and AvailableBytesIn and 
	/// AvailableBytesOut  before calling this method.
	/// </remarks>
	/// <example>
	/// <code>
	/// private void InflateBuffer()
	/// {
	///     int bufferSize = 1024;
	///     byte[] buffer = new byte[bufferSize];
	///     ZlibCodec decompressor = new ZlibCodec();
	///
	///     Console.WriteLine("\n============================================");
	///     Console.WriteLine("Size of Buffer to Inflate: {0} bytes.", CompressedBytes.Length);
	///     MemoryStream ms = new MemoryStream(DecompressedBytes);
	///
	///     int rc = decompressor.InitializeInflate();
	///
	///     decompressor.InputBuffer = CompressedBytes;
	///     decompressor.NextIn = 0;
	///     decompressor.AvailableBytesIn = CompressedBytes.Length;
	///
	///     decompressor.OutputBuffer = buffer;
	///
	///     // pass 1: inflate 
	///     do
	///     {
	///         decompressor.NextOut = 0;
	///         decompressor.AvailableBytesOut = buffer.Length;
	///         rc = decompressor.Inflate(FlushType.None);
	///
	///         if (rc != ZlibConstants.Z_OK &amp;&amp; rc != ZlibConstants.Z_STREAM_END)
	///             throw new Exception("inflating: " + decompressor.Message);
	///
	///         ms.Write(decompressor.OutputBuffer, 0, buffer.Length - decompressor.AvailableBytesOut);
	///     }
	///     while (decompressor.AvailableBytesIn &gt; 0 || decompressor.AvailableBytesOut == 0);
	///
	///     // pass 2: finish and flush
	///     do
	///     {
	///         decompressor.NextOut = 0;
	///         decompressor.AvailableBytesOut = buffer.Length;
	///         rc = decompressor.Inflate(FlushType.Finish);
	///
	///         if (rc != ZlibConstants.Z_STREAM_END &amp;&amp; rc != ZlibConstants.Z_OK)
	///             throw new Exception("inflating: " + decompressor.Message);
	///
	///         if (buffer.Length - decompressor.AvailableBytesOut &gt; 0)
	///             ms.Write(buffer, 0, buffer.Length - decompressor.AvailableBytesOut);
	///     }
	///     while (decompressor.AvailableBytesIn &gt; 0 || decompressor.AvailableBytesOut == 0);
	///
	///     decompressor.EndInflate();
	/// }
	///
	/// </code>
	/// </example>
	/// <param name="flush">The flush to use when inflating.</param>
	/// <returns>Z_OK if everything goes well.</returns>
	internal int Inflate(FlushType flush)
	{
		if (this.istate == null)
		{
			throw new ZlibException("No Inflate State!");
		}
		return this.istate.Inflate(flush);
	}

	/// <summary>
	/// Ends an inflation session. 
	/// </summary>
	/// <remarks>
	/// Call this after successively calling Inflate().  This will cause all buffers to be flushed. 
	/// After calling this you cannot call Inflate() without a intervening call to one of the
	/// InitializeInflate() overloads.
	/// </remarks>
	/// <returns>Z_OK if everything goes well.</returns>
	internal int EndInflate()
	{
		if (this.istate == null)
		{
			throw new ZlibException("No Inflate State!");
		}
		int result = this.istate.End();
		this.istate = null;
		return result;
	}

	/// <summary>
	/// I don't know what this does!
	/// </summary>
	/// <returns>Z_OK if everything goes well.</returns>
	internal int SyncInflate()
	{
		if (this.istate == null)
		{
			throw new ZlibException("No Inflate State!");
		}
		return this.istate.Sync();
	}

	/// <summary>
	/// Initialize the ZlibCodec for deflation operation.
	/// </summary>
	/// <remarks>
	/// The codec will use the MAX window bits and the default level of compression.
	/// </remarks>
	/// <example>
	/// <code>
	///  int bufferSize = 40000;
	///  byte[] CompressedBytes = new byte[bufferSize];
	///  byte[] DecompressedBytes = new byte[bufferSize];
	///
	///  ZlibCodec compressor = new ZlibCodec();
	///
	///  compressor.InitializeDeflate(CompressionLevel.Default);
	///
	///  compressor.InputBuffer = System.Text.ASCIIEncoding.ASCII.GetBytes(TextToCompress);
	///  compressor.NextIn = 0;
	///  compressor.AvailableBytesIn = compressor.InputBuffer.Length;
	///
	///  compressor.OutputBuffer = CompressedBytes;
	///  compressor.NextOut = 0;
	///  compressor.AvailableBytesOut = CompressedBytes.Length;
	///
	///  while (compressor.TotalBytesIn != TextToCompress.Length &amp;&amp; compressor.TotalBytesOut &lt; bufferSize)
	///  {
	///    compressor.Deflate(FlushType.None);
	///  }
	///
	///  while (true)
	///  {
	///    int rc= compressor.Deflate(FlushType.Finish);
	///    if (rc == ZlibConstants.Z_STREAM_END) break;
	///  }
	///
	///  compressor.EndDeflate();
	///
	/// </code>
	/// </example>
	/// <returns>Z_OK if all goes well. You generally don't need to check the return code.</returns>
	internal int InitializeDeflate()
	{
		return this._InternalInitializeDeflate(wantRfc1950Header: true);
	}

	/// <summary>
	/// Initialize the ZlibCodec for deflation operation, using the specified CompressionLevel.
	/// </summary>
	/// <remarks>
	/// The codec will use the maximum window bits (15) and the specified
	/// CompressionLevel.  It will emit a ZLIB stream as it compresses.
	/// </remarks>
	/// <param name="level">The compression level for the codec.</param>
	/// <returns>Z_OK if all goes well.</returns>
	internal int InitializeDeflate(CompressionLevel level)
	{
		this.CompressLevel = level;
		return this._InternalInitializeDeflate(wantRfc1950Header: true);
	}

	/// <summary>
	/// Initialize the ZlibCodec for deflation operation, using the specified CompressionLevel, 
	/// and the explicit flag governing whether to emit an RFC1950 header byte pair.
	/// </summary>
	/// <remarks>
	/// The codec will use the maximum window bits (15) and the specified CompressionLevel.
	/// If you want to generate a zlib stream, you should specify true for
	/// wantRfc1950Header. In this case, the library will emit a ZLIB
	/// header, as defined in <see href="http://www.ietf.org/rfc/rfc1950.txt">RFC
	/// 1950</see>, in the compressed stream.  
	/// </remarks>
	/// <param name="level">The compression level for the codec.</param>
	/// <param name="wantRfc1950Header">whether to emit an initial RFC1950 byte pair in the compressed stream.</param>
	/// <returns>Z_OK if all goes well.</returns>
	internal int InitializeDeflate(CompressionLevel level, bool wantRfc1950Header)
	{
		this.CompressLevel = level;
		return this._InternalInitializeDeflate(wantRfc1950Header);
	}

	/// <summary>
	/// Initialize the ZlibCodec for deflation operation, using the specified CompressionLevel, 
	/// and the specified number of window bits. 
	/// </summary>
	/// <remarks>
	/// The codec will use the specified number of window bits and the specified CompressionLevel.
	/// </remarks>
	/// <param name="level">The compression level for the codec.</param>
	/// <param name="bits">the number of window bits to use.  If you don't know what this means, don't use this method.</param>
	/// <returns>Z_OK if all goes well.</returns>
	internal int InitializeDeflate(CompressionLevel level, int bits)
	{
		this.CompressLevel = level;
		this.WindowBits = bits;
		return this._InternalInitializeDeflate(wantRfc1950Header: true);
	}

	/// <summary>
	/// Initialize the ZlibCodec for deflation operation, using the specified
	/// CompressionLevel, the specified number of window bits, and the explicit flag
	/// governing whether to emit an RFC1950 header byte pair.
	/// </summary>
	///
	/// <param name="level">The compression level for the codec.</param>
	/// <param name="wantRfc1950Header">whether to emit an initial RFC1950 byte pair in the compressed stream.</param>
	/// <param name="bits">the number of window bits to use.  If you don't know what this means, don't use this method.</param>
	/// <returns>Z_OK if all goes well.</returns>
	internal int InitializeDeflate(CompressionLevel level, int bits, bool wantRfc1950Header)
	{
		this.CompressLevel = level;
		this.WindowBits = bits;
		return this._InternalInitializeDeflate(wantRfc1950Header);
	}

	private int _InternalInitializeDeflate(bool wantRfc1950Header)
	{
		if (this.istate != null)
		{
			throw new ZlibException("You may not call InitializeDeflate() after calling InitializeInflate().");
		}
		this.dstate = new DeflateManager();
		this.dstate.WantRfc1950HeaderBytes = wantRfc1950Header;
		return this.dstate.Initialize(this, this.CompressLevel, this.WindowBits, this.Strategy);
	}

	/// <summary>
	/// Deflate one batch of data.
	/// </summary>
	/// <remarks>
	/// You must have set InputBuffer and OutputBuffer before calling this method.
	/// </remarks>
	/// <example>
	/// <code>
	/// private void DeflateBuffer(CompressionLevel level)
	/// {
	///     int bufferSize = 1024;
	///     byte[] buffer = new byte[bufferSize];
	///     ZlibCodec compressor = new ZlibCodec();
	///
	///     Console.WriteLine("\n============================================");
	///     Console.WriteLine("Size of Buffer to Deflate: {0} bytes.", UncompressedBytes.Length);
	///     MemoryStream ms = new MemoryStream();
	///
	///     int rc = compressor.InitializeDeflate(level);
	///
	///     compressor.InputBuffer = UncompressedBytes;
	///     compressor.NextIn = 0;
	///     compressor.AvailableBytesIn = UncompressedBytes.Length;
	///
	///     compressor.OutputBuffer = buffer;
	///
	///     // pass 1: deflate 
	///     do
	///     {
	///         compressor.NextOut = 0;
	///         compressor.AvailableBytesOut = buffer.Length;
	///         rc = compressor.Deflate(FlushType.None);
	///
	///         if (rc != ZlibConstants.Z_OK &amp;&amp; rc != ZlibConstants.Z_STREAM_END)
	///             throw new Exception("deflating: " + compressor.Message);
	///
	///         ms.Write(compressor.OutputBuffer, 0, buffer.Length - compressor.AvailableBytesOut);
	///     }
	///     while (compressor.AvailableBytesIn &gt; 0 || compressor.AvailableBytesOut == 0);
	///
	///     // pass 2: finish and flush
	///     do
	///     {
	///         compressor.NextOut = 0;
	///         compressor.AvailableBytesOut = buffer.Length;
	///         rc = compressor.Deflate(FlushType.Finish);
	///
	///         if (rc != ZlibConstants.Z_STREAM_END &amp;&amp; rc != ZlibConstants.Z_OK)
	///             throw new Exception("deflating: " + compressor.Message);
	///
	///         if (buffer.Length - compressor.AvailableBytesOut &gt; 0)
	///             ms.Write(buffer, 0, buffer.Length - compressor.AvailableBytesOut);
	///     }
	///     while (compressor.AvailableBytesIn &gt; 0 || compressor.AvailableBytesOut == 0);
	///
	///     compressor.EndDeflate();
	///
	///     ms.Seek(0, SeekOrigin.Begin);
	///     CompressedBytes = new byte[compressor.TotalBytesOut];
	///     ms.Read(CompressedBytes, 0, CompressedBytes.Length);
	/// }
	/// </code>
	/// </example>
	/// <param name="flush">whether to flush all data as you deflate. Generally you will want to 
	/// use Z_NO_FLUSH here, in a series of calls to Deflate(), and then call EndDeflate() to 
	/// flush everything. 
	/// </param>
	/// <returns>Z_OK if all goes well.</returns>
	internal int Deflate(FlushType flush)
	{
		if (this.dstate == null)
		{
			throw new ZlibException("No Deflate State!");
		}
		return this.dstate.Deflate(flush);
	}

	/// <summary>
	/// End a deflation session.
	/// </summary>
	/// <remarks>
	/// Call this after making a series of one or more calls to Deflate(). All buffers are flushed.
	/// </remarks>
	/// <returns>Z_OK if all goes well.</returns>
	internal int EndDeflate()
	{
		if (this.dstate == null)
		{
			throw new ZlibException("No Deflate State!");
		}
		this.dstate = null;
		return 0;
	}

	/// <summary>
	/// Reset a codec for another deflation session.
	/// </summary>
	/// <remarks>
	/// Call this to reset the deflation state.  For example if a thread is deflating
	/// non-consecutive blocks, you can call Reset() after the Deflate(Sync) of the first
	/// block and before the next Deflate(None) of the second block.
	/// </remarks>
	/// <returns>Z_OK if all goes well.</returns>
	internal void ResetDeflate()
	{
		if (this.dstate == null)
		{
			throw new ZlibException("No Deflate State!");
		}
		this.dstate.Reset();
	}

	/// <summary>
	/// Set the CompressionStrategy and CompressionLevel for a deflation session.
	/// </summary>
	/// <param name="level">the level of compression to use.</param>
	/// <param name="strategy">the strategy to use for compression.</param>
	/// <returns>Z_OK if all goes well.</returns>
	internal int SetDeflateParams(CompressionLevel level, CompressionStrategy strategy)
	{
		if (this.dstate == null)
		{
			throw new ZlibException("No Deflate State!");
		}
		return this.dstate.SetParams(level, strategy);
	}

	/// <summary>
	/// Set the dictionary to be used for either Inflation or Deflation.
	/// </summary>
	/// <param name="dictionary">The dictionary bytes to use.</param>
	/// <returns>Z_OK if all goes well.</returns>
	internal int SetDictionary(byte[] dictionary)
	{
		if (this.istate != null)
		{
			return this.istate.SetDictionary(dictionary);
		}
		if (this.dstate != null)
		{
			return this.dstate.SetDictionary(dictionary);
		}
		throw new ZlibException("No Inflate or Deflate state!");
	}

	internal void flush_pending()
	{
		int len = this.dstate.pendingCount;
		if (len > this.AvailableBytesOut)
		{
			len = this.AvailableBytesOut;
		}
		if (len != 0)
		{
			if (this.dstate.pending.Length <= this.dstate.nextPending || this.OutputBuffer.Length <= this.NextOut || this.dstate.pending.Length < this.dstate.nextPending + len || this.OutputBuffer.Length < this.NextOut + len)
			{
				throw new ZlibException($"Invalid State. (pending.Length={this.dstate.pending.Length}, pendingCount={this.dstate.pendingCount})");
			}
			Array.Copy(this.dstate.pending, this.dstate.nextPending, this.OutputBuffer, this.NextOut, len);
			this.NextOut += len;
			this.dstate.nextPending += len;
			this.TotalBytesOut += len;
			this.AvailableBytesOut -= len;
			this.dstate.pendingCount -= len;
			if (this.dstate.pendingCount == 0)
			{
				this.dstate.nextPending = 0;
			}
		}
	}

	internal int read_buf(byte[] buf, int start, int size)
	{
		int len = this.AvailableBytesIn;
		if (len > size)
		{
			len = size;
		}
		if (len == 0)
		{
			return 0;
		}
		this.AvailableBytesIn -= len;
		if (this.dstate.WantRfc1950HeaderBytes)
		{
			this._Adler32 = Adler.Adler32(this._Adler32, this.InputBuffer, this.NextIn, len);
		}
		Array.Copy(this.InputBuffer, this.NextIn, buf, start, len);
		this.NextIn += len;
		this.TotalBytesIn += len;
		return len;
	}
}
