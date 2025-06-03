namespace Ionic.Zlib;

internal sealed class InflateManager
{
	private enum InflateManagerMode
	{
		METHOD,
		FLAG,
		DICT4,
		DICT3,
		DICT2,
		DICT1,
		DICT0,
		BLOCKS,
		CHECK4,
		CHECK3,
		CHECK2,
		CHECK1,
		DONE,
		BAD
	}

	private const int PRESET_DICT = 32;

	private const int Z_DEFLATED = 8;

	private InflateManagerMode mode;

	internal ZlibCodec _codec;

	internal int method;

	internal uint computedCheck;

	internal uint expectedCheck;

	internal int marker;

	private bool _handleRfc1950HeaderBytes = true;

	internal int wbits;

	internal InflateBlocks blocks;

	private static readonly byte[] mark = new byte[4] { 0, 0, 255, 255 };

	internal bool HandleRfc1950HeaderBytes
	{
		get
		{
			return this._handleRfc1950HeaderBytes;
		}
		set
		{
			this._handleRfc1950HeaderBytes = value;
		}
	}

	public InflateManager()
	{
	}

	public InflateManager(bool expectRfc1950HeaderBytes)
	{
		this._handleRfc1950HeaderBytes = expectRfc1950HeaderBytes;
	}

	internal int Reset()
	{
		this._codec.TotalBytesIn = (this._codec.TotalBytesOut = 0L);
		this._codec.Message = null;
		this.mode = ((!this.HandleRfc1950HeaderBytes) ? InflateManagerMode.BLOCKS : InflateManagerMode.METHOD);
		this.blocks.Reset();
		return 0;
	}

	internal int End()
	{
		if (this.blocks != null)
		{
			this.blocks.Free();
		}
		this.blocks = null;
		return 0;
	}

	internal int Initialize(ZlibCodec codec, int w)
	{
		this._codec = codec;
		this._codec.Message = null;
		this.blocks = null;
		if (w < 8 || w > 15)
		{
			this.End();
			throw new ZlibException("Bad window size.");
		}
		this.wbits = w;
		this.blocks = new InflateBlocks(codec, this.HandleRfc1950HeaderBytes ? this : null, 1 << w);
		this.Reset();
		return 0;
	}

	internal int Inflate(FlushType flush)
	{
		if (this._codec.InputBuffer == null)
		{
			throw new ZlibException("InputBuffer is null. ");
		}
		int f = 0;
		int r = -5;
		while (true)
		{
			switch (this.mode)
			{
			case InflateManagerMode.METHOD:
				if (this._codec.AvailableBytesIn == 0)
				{
					return r;
				}
				r = f;
				this._codec.AvailableBytesIn--;
				this._codec.TotalBytesIn++;
				if (((this.method = this._codec.InputBuffer[this._codec.NextIn++]) & 0xF) != 8)
				{
					this.mode = InflateManagerMode.BAD;
					this._codec.Message = $"unknown compression method (0x{this.method:X2})";
					this.marker = 5;
				}
				else if ((this.method >> 4) + 8 > this.wbits)
				{
					this.mode = InflateManagerMode.BAD;
					this._codec.Message = $"invalid window size ({(this.method >> 4) + 8})";
					this.marker = 5;
				}
				else
				{
					this.mode = InflateManagerMode.FLAG;
				}
				break;
			case InflateManagerMode.FLAG:
			{
				if (this._codec.AvailableBytesIn == 0)
				{
					return r;
				}
				r = f;
				this._codec.AvailableBytesIn--;
				this._codec.TotalBytesIn++;
				int b = this._codec.InputBuffer[this._codec.NextIn++] & 0xFF;
				if (((this.method << 8) + b) % 31 != 0)
				{
					this.mode = InflateManagerMode.BAD;
					this._codec.Message = "incorrect header check";
					this.marker = 5;
				}
				else
				{
					this.mode = (((b & 0x20) == 0) ? InflateManagerMode.BLOCKS : InflateManagerMode.DICT4);
				}
				break;
			}
			case InflateManagerMode.DICT4:
				if (this._codec.AvailableBytesIn == 0)
				{
					return r;
				}
				r = f;
				this._codec.AvailableBytesIn--;
				this._codec.TotalBytesIn++;
				this.expectedCheck = (uint)((this._codec.InputBuffer[this._codec.NextIn++] << 24) & 0xFF000000u);
				this.mode = InflateManagerMode.DICT3;
				break;
			case InflateManagerMode.DICT3:
				if (this._codec.AvailableBytesIn == 0)
				{
					return r;
				}
				r = f;
				this._codec.AvailableBytesIn--;
				this._codec.TotalBytesIn++;
				this.expectedCheck += (uint)((this._codec.InputBuffer[this._codec.NextIn++] << 16) & 0xFF0000);
				this.mode = InflateManagerMode.DICT2;
				break;
			case InflateManagerMode.DICT2:
				if (this._codec.AvailableBytesIn == 0)
				{
					return r;
				}
				r = f;
				this._codec.AvailableBytesIn--;
				this._codec.TotalBytesIn++;
				this.expectedCheck += (uint)((this._codec.InputBuffer[this._codec.NextIn++] << 8) & 0xFF00);
				this.mode = InflateManagerMode.DICT1;
				break;
			case InflateManagerMode.DICT1:
				if (this._codec.AvailableBytesIn == 0)
				{
					return r;
				}
				r = f;
				this._codec.AvailableBytesIn--;
				this._codec.TotalBytesIn++;
				this.expectedCheck += (uint)(this._codec.InputBuffer[this._codec.NextIn++] & 0xFF);
				this._codec._Adler32 = this.expectedCheck;
				this.mode = InflateManagerMode.DICT0;
				return 2;
			case InflateManagerMode.DICT0:
				this.mode = InflateManagerMode.BAD;
				this._codec.Message = "need dictionary";
				this.marker = 0;
				return -2;
			case InflateManagerMode.BLOCKS:
				r = this.blocks.Process(r);
				switch (r)
				{
				case -3:
					this.mode = InflateManagerMode.BAD;
					this.marker = 0;
					goto end_IL_0025;
				case 0:
					r = f;
					break;
				}
				if (r != 1)
				{
					return r;
				}
				r = f;
				this.computedCheck = this.blocks.Reset();
				if (!this.HandleRfc1950HeaderBytes)
				{
					this.mode = InflateManagerMode.DONE;
					return 1;
				}
				this.mode = InflateManagerMode.CHECK4;
				break;
			case InflateManagerMode.CHECK4:
				if (this._codec.AvailableBytesIn == 0)
				{
					return r;
				}
				r = f;
				this._codec.AvailableBytesIn--;
				this._codec.TotalBytesIn++;
				this.expectedCheck = (uint)((this._codec.InputBuffer[this._codec.NextIn++] << 24) & 0xFF000000u);
				this.mode = InflateManagerMode.CHECK3;
				break;
			case InflateManagerMode.CHECK3:
				if (this._codec.AvailableBytesIn == 0)
				{
					return r;
				}
				r = f;
				this._codec.AvailableBytesIn--;
				this._codec.TotalBytesIn++;
				this.expectedCheck += (uint)((this._codec.InputBuffer[this._codec.NextIn++] << 16) & 0xFF0000);
				this.mode = InflateManagerMode.CHECK2;
				break;
			case InflateManagerMode.CHECK2:
				if (this._codec.AvailableBytesIn == 0)
				{
					return r;
				}
				r = f;
				this._codec.AvailableBytesIn--;
				this._codec.TotalBytesIn++;
				this.expectedCheck += (uint)((this._codec.InputBuffer[this._codec.NextIn++] << 8) & 0xFF00);
				this.mode = InflateManagerMode.CHECK1;
				break;
			case InflateManagerMode.CHECK1:
				if (this._codec.AvailableBytesIn == 0)
				{
					return r;
				}
				r = f;
				this._codec.AvailableBytesIn--;
				this._codec.TotalBytesIn++;
				this.expectedCheck += (uint)(this._codec.InputBuffer[this._codec.NextIn++] & 0xFF);
				if (this.computedCheck != this.expectedCheck)
				{
					this.mode = InflateManagerMode.BAD;
					this._codec.Message = "incorrect data check";
					this.marker = 5;
					break;
				}
				this.mode = InflateManagerMode.DONE;
				return 1;
			case InflateManagerMode.DONE:
				return 1;
			case InflateManagerMode.BAD:
				throw new ZlibException($"Bad state ({this._codec.Message})");
			default:
				{
					throw new ZlibException("Stream error.");
				}
				end_IL_0025:
				break;
			}
		}
	}

	internal int SetDictionary(byte[] dictionary)
	{
		int index = 0;
		int length = dictionary.Length;
		if (this.mode != InflateManagerMode.DICT0)
		{
			throw new ZlibException("Stream error.");
		}
		if (Adler.Adler32(1u, dictionary, 0, dictionary.Length) != this._codec._Adler32)
		{
			return -3;
		}
		this._codec._Adler32 = Adler.Adler32(0u, null, 0, 0);
		if (length >= 1 << this.wbits)
		{
			length = (1 << this.wbits) - 1;
			index = dictionary.Length - length;
		}
		this.blocks.SetDictionary(dictionary, index, length);
		this.mode = InflateManagerMode.BLOCKS;
		return 0;
	}

	internal int Sync()
	{
		if (this.mode != InflateManagerMode.BAD)
		{
			this.mode = InflateManagerMode.BAD;
			this.marker = 0;
		}
		int n;
		if ((n = this._codec.AvailableBytesIn) == 0)
		{
			return -5;
		}
		int p = this._codec.NextIn;
		int m = this.marker;
		while (n != 0 && m < 4)
		{
			m = ((this._codec.InputBuffer[p] != InflateManager.mark[m]) ? ((this._codec.InputBuffer[p] == 0) ? (4 - m) : 0) : (m + 1));
			p++;
			n--;
		}
		this._codec.TotalBytesIn += p - this._codec.NextIn;
		this._codec.NextIn = p;
		this._codec.AvailableBytesIn = n;
		this.marker = m;
		if (m != 4)
		{
			return -3;
		}
		long r = this._codec.TotalBytesIn;
		long w = this._codec.TotalBytesOut;
		this.Reset();
		this._codec.TotalBytesIn = r;
		this._codec.TotalBytesOut = w;
		this.mode = InflateManagerMode.BLOCKS;
		return 0;
	}

	internal int SyncPoint(ZlibCodec z)
	{
		return this.blocks.SyncPoint();
	}
}
