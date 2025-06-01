using System;

namespace MonoGame.Framework.Utilities.Deflate;

internal sealed class InflateBlocks
{
	private enum InflateBlockMode
	{
		TYPE,
		LENS,
		STORED,
		TABLE,
		BTREE,
		DTREE,
		CODES,
		DRY,
		DONE,
		BAD
	}

	private const int MANY = 1440;

	internal static readonly int[] border = new int[19]
	{
		16, 17, 18, 0, 8, 7, 9, 6, 10, 5,
		11, 4, 12, 3, 13, 2, 14, 1, 15
	};

	private InflateBlockMode mode;

	internal int left;

	internal int table;

	internal int index;

	internal int[] blens;

	internal int[] bb = new int[1];

	internal int[] tb = new int[1];

	internal InflateCodes codes = new InflateCodes();

	internal int last;

	internal ZlibCodec _codec;

	internal int bitk;

	internal int bitb;

	internal int[] hufts;

	internal byte[] window;

	internal int end;

	internal int readAt;

	internal int writeAt;

	internal object checkfn;

	internal uint check;

	internal InfTree inftree = new InfTree();

	internal InflateBlocks(ZlibCodec codec, object checkfn, int w)
	{
		this._codec = codec;
		this.hufts = new int[4320];
		this.window = new byte[w];
		this.end = w;
		this.checkfn = checkfn;
		this.mode = InflateBlockMode.TYPE;
		this.Reset();
	}

	internal uint Reset()
	{
		uint result = this.check;
		this.mode = InflateBlockMode.TYPE;
		this.bitk = 0;
		this.bitb = 0;
		this.readAt = (this.writeAt = 0);
		if (this.checkfn != null)
		{
			this._codec._Adler32 = (this.check = Adler.Adler32(0u, null, 0, 0));
		}
		return result;
	}

	internal int Process(int r)
	{
		int p = this._codec.NextIn;
		int n = this._codec.AvailableBytesIn;
		int b = this.bitb;
		int k = this.bitk;
		int q = this.writeAt;
		int m = ((q < this.readAt) ? (this.readAt - q - 1) : (this.end - q));
		while (true)
		{
			switch (this.mode)
			{
			case InflateBlockMode.TYPE:
			{
				for (; k < 3; k += 8)
				{
					if (n != 0)
					{
						r = 0;
						n--;
						b |= (this._codec.InputBuffer[p++] & 0xFF) << k;
						continue;
					}
					this.bitb = b;
					this.bitk = k;
					this._codec.AvailableBytesIn = n;
					this._codec.TotalBytesIn += p - this._codec.NextIn;
					this._codec.NextIn = p;
					this.writeAt = q;
					return this.Flush(r);
				}
				int t = b & 7;
				this.last = t & 1;
				switch ((uint)(t >>> 1))
				{
				case 0u:
					b >>= 3;
					k -= 3;
					t = k & 7;
					b >>= t;
					k -= t;
					this.mode = InflateBlockMode.LENS;
					break;
				case 1u:
				{
					int[] bl = new int[1];
					int[] bd = new int[1];
					int[][] tl = new int[1][];
					int[][] td = new int[1][];
					InfTree.inflate_trees_fixed(bl, bd, tl, td, this._codec);
					this.codes.Init(bl[0], bd[0], tl[0], 0, td[0], 0);
					b >>= 3;
					k -= 3;
					this.mode = InflateBlockMode.CODES;
					break;
				}
				case 2u:
					b >>= 3;
					k -= 3;
					this.mode = InflateBlockMode.TABLE;
					break;
				case 3u:
					b >>= 3;
					k -= 3;
					this.mode = InflateBlockMode.BAD;
					this._codec.Message = "invalid block type";
					r = -3;
					this.bitb = b;
					this.bitk = k;
					this._codec.AvailableBytesIn = n;
					this._codec.TotalBytesIn += p - this._codec.NextIn;
					this._codec.NextIn = p;
					this.writeAt = q;
					return this.Flush(r);
				}
				break;
			}
			case InflateBlockMode.LENS:
				for (; k < 32; k += 8)
				{
					if (n != 0)
					{
						r = 0;
						n--;
						b |= (this._codec.InputBuffer[p++] & 0xFF) << k;
						continue;
					}
					this.bitb = b;
					this.bitk = k;
					this._codec.AvailableBytesIn = n;
					this._codec.TotalBytesIn += p - this._codec.NextIn;
					this._codec.NextIn = p;
					this.writeAt = q;
					return this.Flush(r);
				}
				if (((~b >> 16) & 0xFFFF) != (b & 0xFFFF))
				{
					this.mode = InflateBlockMode.BAD;
					this._codec.Message = "invalid stored block lengths";
					r = -3;
					this.bitb = b;
					this.bitk = k;
					this._codec.AvailableBytesIn = n;
					this._codec.TotalBytesIn += p - this._codec.NextIn;
					this._codec.NextIn = p;
					this.writeAt = q;
					return this.Flush(r);
				}
				this.left = b & 0xFFFF;
				b = (k = 0);
				this.mode = ((this.left != 0) ? InflateBlockMode.STORED : ((this.last != 0) ? InflateBlockMode.DRY : InflateBlockMode.TYPE));
				break;
			case InflateBlockMode.STORED:
			{
				if (n == 0)
				{
					this.bitb = b;
					this.bitk = k;
					this._codec.AvailableBytesIn = n;
					this._codec.TotalBytesIn += p - this._codec.NextIn;
					this._codec.NextIn = p;
					this.writeAt = q;
					return this.Flush(r);
				}
				if (m == 0)
				{
					if (q == this.end && this.readAt != 0)
					{
						q = 0;
						m = ((q < this.readAt) ? (this.readAt - q - 1) : (this.end - q));
					}
					if (m == 0)
					{
						this.writeAt = q;
						r = this.Flush(r);
						q = this.writeAt;
						m = ((q < this.readAt) ? (this.readAt - q - 1) : (this.end - q));
						if (q == this.end && this.readAt != 0)
						{
							q = 0;
							m = ((q < this.readAt) ? (this.readAt - q - 1) : (this.end - q));
						}
						if (m == 0)
						{
							this.bitb = b;
							this.bitk = k;
							this._codec.AvailableBytesIn = n;
							this._codec.TotalBytesIn += p - this._codec.NextIn;
							this._codec.NextIn = p;
							this.writeAt = q;
							return this.Flush(r);
						}
					}
				}
				r = 0;
				int t = this.left;
				if (t > n)
				{
					t = n;
				}
				if (t > m)
				{
					t = m;
				}
				Array.Copy(this._codec.InputBuffer, p, this.window, q, t);
				p += t;
				n -= t;
				q += t;
				m -= t;
				if ((this.left -= t) == 0)
				{
					this.mode = ((this.last != 0) ? InflateBlockMode.DRY : InflateBlockMode.TYPE);
				}
				break;
			}
			case InflateBlockMode.TABLE:
			{
				for (; k < 14; k += 8)
				{
					if (n != 0)
					{
						r = 0;
						n--;
						b |= (this._codec.InputBuffer[p++] & 0xFF) << k;
						continue;
					}
					this.bitb = b;
					this.bitk = k;
					this._codec.AvailableBytesIn = n;
					this._codec.TotalBytesIn += p - this._codec.NextIn;
					this._codec.NextIn = p;
					this.writeAt = q;
					return this.Flush(r);
				}
				int t = (this.table = b & 0x3FFF);
				if ((t & 0x1F) > 29 || ((t >> 5) & 0x1F) > 29)
				{
					this.mode = InflateBlockMode.BAD;
					this._codec.Message = "too many length or distance symbols";
					r = -3;
					this.bitb = b;
					this.bitk = k;
					this._codec.AvailableBytesIn = n;
					this._codec.TotalBytesIn += p - this._codec.NextIn;
					this._codec.NextIn = p;
					this.writeAt = q;
					return this.Flush(r);
				}
				t = 258 + (t & 0x1F) + ((t >> 5) & 0x1F);
				if (this.blens == null || this.blens.Length < t)
				{
					this.blens = new int[t];
				}
				else
				{
					Array.Clear(this.blens, 0, t);
				}
				b >>= 14;
				k -= 14;
				this.index = 0;
				this.mode = InflateBlockMode.BTREE;
				goto case InflateBlockMode.BTREE;
			}
			case InflateBlockMode.BTREE:
			{
				while (this.index < 4 + (this.table >> 10))
				{
					for (; k < 3; k += 8)
					{
						if (n != 0)
						{
							r = 0;
							n--;
							b |= (this._codec.InputBuffer[p++] & 0xFF) << k;
							continue;
						}
						this.bitb = b;
						this.bitk = k;
						this._codec.AvailableBytesIn = n;
						this._codec.TotalBytesIn += p - this._codec.NextIn;
						this._codec.NextIn = p;
						this.writeAt = q;
						return this.Flush(r);
					}
					this.blens[InflateBlocks.border[this.index++]] = b & 7;
					b >>= 3;
					k -= 3;
				}
				while (this.index < 19)
				{
					this.blens[InflateBlocks.border[this.index++]] = 0;
				}
				this.bb[0] = 7;
				int t = this.inftree.inflate_trees_bits(this.blens, this.bb, this.tb, this.hufts, this._codec);
				if (t != 0)
				{
					r = t;
					if (r == -3)
					{
						this.blens = null;
						this.mode = InflateBlockMode.BAD;
					}
					this.bitb = b;
					this.bitk = k;
					this._codec.AvailableBytesIn = n;
					this._codec.TotalBytesIn += p - this._codec.NextIn;
					this._codec.NextIn = p;
					this.writeAt = q;
					return this.Flush(r);
				}
				this.index = 0;
				this.mode = InflateBlockMode.DTREE;
				goto case InflateBlockMode.DTREE;
			}
			case InflateBlockMode.DTREE:
			{
				int t;
				while (true)
				{
					t = this.table;
					if (this.index >= 258 + (t & 0x1F) + ((t >> 5) & 0x1F))
					{
						break;
					}
					for (t = this.bb[0]; k < t; k += 8)
					{
						if (n != 0)
						{
							r = 0;
							n--;
							b |= (this._codec.InputBuffer[p++] & 0xFF) << k;
							continue;
						}
						this.bitb = b;
						this.bitk = k;
						this._codec.AvailableBytesIn = n;
						this._codec.TotalBytesIn += p - this._codec.NextIn;
						this._codec.NextIn = p;
						this.writeAt = q;
						return this.Flush(r);
					}
					t = this.hufts[(this.tb[0] + (b & InternalInflateConstants.InflateMask[t])) * 3 + 1];
					int c = this.hufts[(this.tb[0] + (b & InternalInflateConstants.InflateMask[t])) * 3 + 2];
					if (c < 16)
					{
						b >>= t;
						k -= t;
						this.blens[this.index++] = c;
						continue;
					}
					int i = ((c == 18) ? 7 : (c - 14));
					int j = ((c == 18) ? 11 : 3);
					for (; k < t + i; k += 8)
					{
						if (n != 0)
						{
							r = 0;
							n--;
							b |= (this._codec.InputBuffer[p++] & 0xFF) << k;
							continue;
						}
						this.bitb = b;
						this.bitk = k;
						this._codec.AvailableBytesIn = n;
						this._codec.TotalBytesIn += p - this._codec.NextIn;
						this._codec.NextIn = p;
						this.writeAt = q;
						return this.Flush(r);
					}
					b >>= t;
					k -= t;
					j += b & InternalInflateConstants.InflateMask[i];
					b >>= i;
					k -= i;
					i = this.index;
					t = this.table;
					if (i + j > 258 + (t & 0x1F) + ((t >> 5) & 0x1F) || (c == 16 && i < 1))
					{
						this.blens = null;
						this.mode = InflateBlockMode.BAD;
						this._codec.Message = "invalid bit length repeat";
						r = -3;
						this.bitb = b;
						this.bitk = k;
						this._codec.AvailableBytesIn = n;
						this._codec.TotalBytesIn += p - this._codec.NextIn;
						this._codec.NextIn = p;
						this.writeAt = q;
						return this.Flush(r);
					}
					c = ((c == 16) ? this.blens[i - 1] : 0);
					do
					{
						this.blens[i++] = c;
					}
					while (--j != 0);
					this.index = i;
				}
				this.tb[0] = -1;
				int[] bl2 = new int[1] { 9 };
				int[] bd2 = new int[1] { 6 };
				int[] tl2 = new int[1];
				int[] td2 = new int[1];
				t = this.table;
				t = this.inftree.inflate_trees_dynamic(257 + (t & 0x1F), 1 + ((t >> 5) & 0x1F), this.blens, bl2, bd2, tl2, td2, this.hufts, this._codec);
				if (t != 0)
				{
					if (t == -3)
					{
						this.blens = null;
						this.mode = InflateBlockMode.BAD;
					}
					r = t;
					this.bitb = b;
					this.bitk = k;
					this._codec.AvailableBytesIn = n;
					this._codec.TotalBytesIn += p - this._codec.NextIn;
					this._codec.NextIn = p;
					this.writeAt = q;
					return this.Flush(r);
				}
				this.codes.Init(bl2[0], bd2[0], this.hufts, tl2[0], this.hufts, td2[0]);
				this.mode = InflateBlockMode.CODES;
				goto case InflateBlockMode.CODES;
			}
			case InflateBlockMode.CODES:
				this.bitb = b;
				this.bitk = k;
				this._codec.AvailableBytesIn = n;
				this._codec.TotalBytesIn += p - this._codec.NextIn;
				this._codec.NextIn = p;
				this.writeAt = q;
				r = this.codes.Process(this, r);
				if (r != 1)
				{
					return this.Flush(r);
				}
				r = 0;
				p = this._codec.NextIn;
				n = this._codec.AvailableBytesIn;
				b = this.bitb;
				k = this.bitk;
				q = this.writeAt;
				m = ((q < this.readAt) ? (this.readAt - q - 1) : (this.end - q));
				if (this.last == 0)
				{
					this.mode = InflateBlockMode.TYPE;
					break;
				}
				this.mode = InflateBlockMode.DRY;
				goto case InflateBlockMode.DRY;
			case InflateBlockMode.DRY:
				this.writeAt = q;
				r = this.Flush(r);
				q = this.writeAt;
				m = ((q < this.readAt) ? (this.readAt - q - 1) : (this.end - q));
				if (this.readAt != this.writeAt)
				{
					this.bitb = b;
					this.bitk = k;
					this._codec.AvailableBytesIn = n;
					this._codec.TotalBytesIn += p - this._codec.NextIn;
					this._codec.NextIn = p;
					this.writeAt = q;
					return this.Flush(r);
				}
				this.mode = InflateBlockMode.DONE;
				goto case InflateBlockMode.DONE;
			case InflateBlockMode.DONE:
				r = 1;
				this.bitb = b;
				this.bitk = k;
				this._codec.AvailableBytesIn = n;
				this._codec.TotalBytesIn += p - this._codec.NextIn;
				this._codec.NextIn = p;
				this.writeAt = q;
				return this.Flush(r);
			case InflateBlockMode.BAD:
				r = -3;
				this.bitb = b;
				this.bitk = k;
				this._codec.AvailableBytesIn = n;
				this._codec.TotalBytesIn += p - this._codec.NextIn;
				this._codec.NextIn = p;
				this.writeAt = q;
				return this.Flush(r);
			default:
				r = -2;
				this.bitb = b;
				this.bitk = k;
				this._codec.AvailableBytesIn = n;
				this._codec.TotalBytesIn += p - this._codec.NextIn;
				this._codec.NextIn = p;
				this.writeAt = q;
				return this.Flush(r);
			}
		}
	}

	internal void Free()
	{
		this.Reset();
		this.window = null;
		this.hufts = null;
	}

	internal void SetDictionary(byte[] d, int start, int n)
	{
		Array.Copy(d, start, this.window, 0, n);
		this.readAt = (this.writeAt = n);
	}

	internal int SyncPoint()
	{
		return (this.mode == InflateBlockMode.LENS) ? 1 : 0;
	}

	internal int Flush(int r)
	{
		for (int pass = 0; pass < 2; pass++)
		{
			int nBytes = ((pass != 0) ? (this.writeAt - this.readAt) : (((this.readAt <= this.writeAt) ? this.writeAt : this.end) - this.readAt));
			if (nBytes == 0)
			{
				if (r == -5)
				{
					r = 0;
				}
				return r;
			}
			if (nBytes > this._codec.AvailableBytesOut)
			{
				nBytes = this._codec.AvailableBytesOut;
			}
			if (nBytes != 0 && r == -5)
			{
				r = 0;
			}
			this._codec.AvailableBytesOut -= nBytes;
			this._codec.TotalBytesOut += nBytes;
			if (this.checkfn != null)
			{
				this._codec._Adler32 = (this.check = Adler.Adler32(this.check, this.window, this.readAt, nBytes));
			}
			Array.Copy(this.window, this.readAt, this._codec.OutputBuffer, this._codec.NextOut, nBytes);
			this._codec.NextOut += nBytes;
			this.readAt += nBytes;
			if (this.readAt == this.end && pass == 0)
			{
				this.readAt = 0;
				if (this.writeAt == this.end)
				{
					this.writeAt = 0;
				}
			}
			else
			{
				pass++;
			}
		}
		return r;
	}
}
