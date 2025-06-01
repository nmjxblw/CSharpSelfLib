using System.IO;

namespace Microsoft.Xna.Framework.Content;

internal class LzxDecoder
{
	private class BitBuffer
	{
		private uint buffer;

		private byte bitsleft;

		private Stream byteStream;

		public BitBuffer(Stream stream)
		{
			this.byteStream = stream;
			this.InitBitStream();
		}

		public void InitBitStream()
		{
			this.buffer = 0u;
			this.bitsleft = 0;
		}

		public void EnsureBits(byte bits)
		{
			while (this.bitsleft < bits)
			{
				int lo = (byte)this.byteStream.ReadByte();
				int hi = (byte)this.byteStream.ReadByte();
				this.buffer |= (uint)(((hi << 8) | lo) << 16 - this.bitsleft);
				this.bitsleft += 16;
			}
		}

		public uint PeekBits(byte bits)
		{
			return this.buffer >> 32 - bits;
		}

		public void RemoveBits(byte bits)
		{
			this.buffer <<= (int)bits;
			this.bitsleft -= bits;
		}

		public uint ReadBits(byte bits)
		{
			uint ret = 0u;
			if (bits > 0)
			{
				this.EnsureBits(bits);
				ret = this.PeekBits(bits);
				this.RemoveBits(bits);
			}
			return ret;
		}

		public uint GetBuffer()
		{
			return this.buffer;
		}

		public byte GetBitsLeft()
		{
			return this.bitsleft;
		}
	}

	private struct LzxState
	{
		public uint R0;

		public uint R1;

		public uint R2;

		public ushort main_elements;

		public int header_read;

		public LzxConstants.BLOCKTYPE block_type;

		public uint block_length;

		public uint block_remaining;

		public uint frames_read;

		public int intel_filesize;

		public int intel_curpos;

		public int intel_started;

		public ushort[] PRETREE_table;

		public byte[] PRETREE_len;

		public ushort[] MAINTREE_table;

		public byte[] MAINTREE_len;

		public ushort[] LENGTH_table;

		public byte[] LENGTH_len;

		public ushort[] ALIGNED_table;

		public byte[] ALIGNED_len;

		public uint actual_size;

		public byte[] window;

		public uint window_size;

		public uint window_posn;
	}

	public static uint[] position_base;

	public static byte[] extra_bits;

	private LzxState m_state;

	public LzxDecoder(int window)
	{
		uint wndsize = (uint)(1 << window);
		if (window < 15 || window > 21)
		{
			throw new UnsupportedWindowSizeRange();
		}
		this.m_state = default(LzxState);
		this.m_state.actual_size = 0u;
		this.m_state.window = new byte[wndsize];
		for (int i = 0; i < wndsize; i++)
		{
			this.m_state.window[i] = 220;
		}
		this.m_state.actual_size = wndsize;
		this.m_state.window_size = wndsize;
		this.m_state.window_posn = 0u;
		if (LzxDecoder.extra_bits == null)
		{
			LzxDecoder.extra_bits = new byte[52];
			int j = 0;
			int j2 = 0;
			for (; j <= 50; j += 2)
			{
				LzxDecoder.extra_bits[j] = (LzxDecoder.extra_bits[j + 1] = (byte)j2);
				if (j != 0 && j2 < 17)
				{
					j2++;
				}
			}
		}
		if (LzxDecoder.position_base == null)
		{
			LzxDecoder.position_base = new uint[51];
			int k = 0;
			int j3 = 0;
			for (; k <= 50; k++)
			{
				LzxDecoder.position_base[k] = (uint)j3;
				j3 += 1 << (int)LzxDecoder.extra_bits[k];
			}
		}
		int posn_slots = window switch
		{
			20 => 42, 
			21 => 50, 
			_ => window << 1, 
		};
		this.m_state.R0 = (this.m_state.R1 = (this.m_state.R2 = 1u));
		this.m_state.main_elements = (ushort)(256 + (posn_slots << 3));
		this.m_state.header_read = 0;
		this.m_state.frames_read = 0u;
		this.m_state.block_remaining = 0u;
		this.m_state.block_type = LzxConstants.BLOCKTYPE.INVALID;
		this.m_state.intel_curpos = 0;
		this.m_state.intel_started = 0;
		this.m_state.PRETREE_table = new ushort[104];
		this.m_state.PRETREE_len = new byte[84];
		this.m_state.MAINTREE_table = new ushort[5408];
		this.m_state.MAINTREE_len = new byte[720];
		this.m_state.LENGTH_table = new ushort[4596];
		this.m_state.LENGTH_len = new byte[314];
		this.m_state.ALIGNED_table = new ushort[144];
		this.m_state.ALIGNED_len = new byte[72];
		for (int l = 0; l < 656; l++)
		{
			this.m_state.MAINTREE_len[l] = 0;
		}
		for (int m = 0; m < 250; m++)
		{
			this.m_state.LENGTH_len[m] = 0;
		}
	}

	public int Decompress(Stream inData, int inLen, Stream outData, int outLen)
	{
		BitBuffer bitbuf = new BitBuffer(inData);
		long startpos = inData.Position;
		long endpos = inData.Position + inLen;
		byte[] window = this.m_state.window;
		uint window_posn = this.m_state.window_posn;
		uint window_size = this.m_state.window_size;
		uint R0 = this.m_state.R0;
		uint R1 = this.m_state.R1;
		uint R2 = this.m_state.R2;
		int togo = outLen;
		bitbuf.InitBitStream();
		if (this.m_state.header_read == 0)
		{
			if (bitbuf.ReadBits(1) != 0)
			{
				uint i = bitbuf.ReadBits(16);
				uint j = bitbuf.ReadBits(16);
				this.m_state.intel_filesize = (int)((i << 16) | j);
			}
			this.m_state.header_read = 1;
		}
		while (togo > 0)
		{
			if (this.m_state.block_remaining == 0)
			{
				if (this.m_state.block_type == LzxConstants.BLOCKTYPE.UNCOMPRESSED)
				{
					if ((this.m_state.block_length & 1) == 1)
					{
						inData.ReadByte();
					}
					bitbuf.InitBitStream();
				}
				this.m_state.block_type = (LzxConstants.BLOCKTYPE)bitbuf.ReadBits(3);
				uint i = bitbuf.ReadBits(16);
				uint j = bitbuf.ReadBits(8);
				this.m_state.block_remaining = (this.m_state.block_length = (i << 8) | j);
				switch (this.m_state.block_type)
				{
				case LzxConstants.BLOCKTYPE.ALIGNED:
					i = 0u;
					j = 0u;
					for (; i < 8; i++)
					{
						j = bitbuf.ReadBits(3);
						this.m_state.ALIGNED_len[i] = (byte)j;
					}
					this.MakeDecodeTable(8u, 7u, this.m_state.ALIGNED_len, this.m_state.ALIGNED_table);
					goto case LzxConstants.BLOCKTYPE.VERBATIM;
				case LzxConstants.BLOCKTYPE.VERBATIM:
					this.ReadLengths(this.m_state.MAINTREE_len, 0u, 256u, bitbuf);
					this.ReadLengths(this.m_state.MAINTREE_len, 256u, this.m_state.main_elements, bitbuf);
					this.MakeDecodeTable(656u, 12u, this.m_state.MAINTREE_len, this.m_state.MAINTREE_table);
					if (this.m_state.MAINTREE_len[232] != 0)
					{
						this.m_state.intel_started = 1;
					}
					this.ReadLengths(this.m_state.LENGTH_len, 0u, 249u, bitbuf);
					this.MakeDecodeTable(250u, 12u, this.m_state.LENGTH_len, this.m_state.LENGTH_table);
					break;
				case LzxConstants.BLOCKTYPE.UNCOMPRESSED:
				{
					this.m_state.intel_started = 1;
					bitbuf.EnsureBits(16);
					if (bitbuf.GetBitsLeft() > 16)
					{
						inData.Seek(-2L, SeekOrigin.Current);
					}
					byte num = (byte)inData.ReadByte();
					byte ml = (byte)inData.ReadByte();
					byte mh = (byte)inData.ReadByte();
					byte hi = (byte)inData.ReadByte();
					R0 = (uint)(num | (ml << 8) | (mh << 16) | (hi << 24));
					byte num2 = (byte)inData.ReadByte();
					ml = (byte)inData.ReadByte();
					mh = (byte)inData.ReadByte();
					hi = (byte)inData.ReadByte();
					R1 = (uint)(num2 | (ml << 8) | (mh << 16) | (hi << 24));
					byte num3 = (byte)inData.ReadByte();
					ml = (byte)inData.ReadByte();
					mh = (byte)inData.ReadByte();
					hi = (byte)inData.ReadByte();
					R2 = (uint)(num3 | (ml << 8) | (mh << 16) | (hi << 24));
					break;
				}
				default:
					return -1;
				}
			}
			if (inData.Position > startpos + inLen && (inData.Position > startpos + inLen + 2 || bitbuf.GetBitsLeft() < 16))
			{
				return -1;
			}
			int this_run;
			while ((this_run = (int)this.m_state.block_remaining) > 0 && togo > 0)
			{
				if (this_run > togo)
				{
					this_run = togo;
				}
				togo -= this_run;
				this.m_state.block_remaining -= (uint)this_run;
				window_posn &= window_size - 1;
				if (window_posn + this_run > window_size)
				{
					return -1;
				}
				switch (this.m_state.block_type)
				{
				case LzxConstants.BLOCKTYPE.VERBATIM:
					while (this_run > 0)
					{
						int main_element = (int)this.ReadHuffSym(this.m_state.MAINTREE_table, this.m_state.MAINTREE_len, 656u, 12u, bitbuf);
						if (main_element < 256)
						{
							window[window_posn++] = (byte)main_element;
							this_run--;
							continue;
						}
						main_element -= 256;
						int match_length = main_element & 7;
						if (match_length == 7)
						{
							int length_footer = (int)this.ReadHuffSym(this.m_state.LENGTH_table, this.m_state.LENGTH_len, 250u, 12u, bitbuf);
							match_length += length_footer;
						}
						match_length += 2;
						int match_offset = main_element >> 3;
						if (match_offset > 2)
						{
							if (match_offset != 3)
							{
								int extra = LzxDecoder.extra_bits[match_offset];
								int verbatim_bits = (int)bitbuf.ReadBits((byte)extra);
								match_offset = (int)(LzxDecoder.position_base[match_offset] - 2) + verbatim_bits;
							}
							else
							{
								match_offset = 1;
							}
							R2 = R1;
							R1 = R0;
							R0 = (uint)match_offset;
						}
						else
						{
							switch (match_offset)
							{
							case 0:
								match_offset = (int)R0;
								break;
							case 1:
								match_offset = (int)R1;
								R1 = R0;
								R0 = (uint)match_offset;
								break;
							default:
								match_offset = (int)R2;
								R2 = R0;
								R0 = (uint)match_offset;
								break;
							}
						}
						int rundest = (int)window_posn;
						this_run -= match_length;
						int runsrc;
						if (window_posn >= match_offset)
						{
							runsrc = rundest - match_offset;
						}
						else
						{
							runsrc = rundest + ((int)window_size - match_offset);
							int copy_length = match_offset - (int)window_posn;
							if (copy_length < match_length)
							{
								match_length -= copy_length;
								window_posn += (uint)copy_length;
								while (copy_length-- > 0)
								{
									window[rundest++] = window[runsrc++];
								}
								runsrc = 0;
							}
						}
						window_posn += (uint)match_length;
						while (match_length-- > 0)
						{
							window[rundest++] = window[runsrc++];
						}
					}
					break;
				case LzxConstants.BLOCKTYPE.ALIGNED:
					while (this_run > 0)
					{
						int main_element = (int)this.ReadHuffSym(this.m_state.MAINTREE_table, this.m_state.MAINTREE_len, 656u, 12u, bitbuf);
						if (main_element < 256)
						{
							window[window_posn++] = (byte)main_element;
							this_run--;
							continue;
						}
						main_element -= 256;
						int match_length = main_element & 7;
						if (match_length == 7)
						{
							int length_footer = (int)this.ReadHuffSym(this.m_state.LENGTH_table, this.m_state.LENGTH_len, 250u, 12u, bitbuf);
							match_length += length_footer;
						}
						match_length += 2;
						int match_offset = main_element >> 3;
						if (match_offset > 2)
						{
							int extra = LzxDecoder.extra_bits[match_offset];
							match_offset = (int)(LzxDecoder.position_base[match_offset] - 2);
							if (extra > 3)
							{
								extra -= 3;
								int verbatim_bits = (int)bitbuf.ReadBits((byte)extra);
								match_offset += verbatim_bits << 3;
								int aligned_bits = (int)this.ReadHuffSym(this.m_state.ALIGNED_table, this.m_state.ALIGNED_len, 8u, 7u, bitbuf);
								match_offset += aligned_bits;
							}
							else if (extra == 3)
							{
								int aligned_bits = (int)this.ReadHuffSym(this.m_state.ALIGNED_table, this.m_state.ALIGNED_len, 8u, 7u, bitbuf);
								match_offset += aligned_bits;
							}
							else if (extra > 0)
							{
								int verbatim_bits = (int)bitbuf.ReadBits((byte)extra);
								match_offset += verbatim_bits;
							}
							else
							{
								match_offset = 1;
							}
							R2 = R1;
							R1 = R0;
							R0 = (uint)match_offset;
						}
						else
						{
							switch (match_offset)
							{
							case 0:
								match_offset = (int)R0;
								break;
							case 1:
								match_offset = (int)R1;
								R1 = R0;
								R0 = (uint)match_offset;
								break;
							default:
								match_offset = (int)R2;
								R2 = R0;
								R0 = (uint)match_offset;
								break;
							}
						}
						int rundest = (int)window_posn;
						this_run -= match_length;
						int runsrc;
						if (window_posn >= match_offset)
						{
							runsrc = rundest - match_offset;
						}
						else
						{
							runsrc = rundest + ((int)window_size - match_offset);
							int copy_length = match_offset - (int)window_posn;
							if (copy_length < match_length)
							{
								match_length -= copy_length;
								window_posn += (uint)copy_length;
								while (copy_length-- > 0)
								{
									window[rundest++] = window[runsrc++];
								}
								runsrc = 0;
							}
						}
						window_posn += (uint)match_length;
						while (match_length-- > 0)
						{
							window[rundest++] = window[runsrc++];
						}
					}
					break;
				case LzxConstants.BLOCKTYPE.UNCOMPRESSED:
				{
					if (inData.Position + this_run > endpos)
					{
						return -1;
					}
					byte[] temp_buffer = new byte[this_run];
					inData.Read(temp_buffer, 0, this_run);
					temp_buffer.CopyTo(window, (int)window_posn);
					window_posn += (uint)this_run;
					break;
				}
				default:
					return -1;
				}
			}
		}
		if (togo != 0)
		{
			return -1;
		}
		int start_window_pos = (int)window_posn;
		if (start_window_pos == 0)
		{
			start_window_pos = (int)window_size;
		}
		start_window_pos -= outLen;
		outData.Write(window, start_window_pos, outLen);
		this.m_state.window_posn = window_posn;
		this.m_state.R0 = R0;
		this.m_state.R1 = R1;
		this.m_state.R2 = R2;
		if (this.m_state.frames_read++ < 32768 && this.m_state.intel_filesize != 0)
		{
			if (outLen <= 6 || this.m_state.intel_started == 0)
			{
				this.m_state.intel_curpos += outLen;
			}
			else
			{
				int dataend = outLen - 10;
				uint curpos = (uint)this.m_state.intel_curpos;
				this.m_state.intel_curpos = (int)curpos + outLen;
				while (outData.Position < dataend)
				{
					if (outData.ReadByte() != 232)
					{
						curpos++;
					}
				}
			}
			return -1;
		}
		return 0;
	}

	private int MakeDecodeTable(uint nsyms, uint nbits, byte[] length, ushort[] table)
	{
		byte bit_num = 1;
		uint pos = 0u;
		uint table_mask = (uint)(1 << (int)nbits);
		uint bit_mask = table_mask >> 1;
		uint next_symbol = bit_mask;
		while (bit_num <= nbits)
		{
			for (ushort sym = 0; sym < nsyms; sym++)
			{
				if (length[sym] == bit_num)
				{
					uint leaf = pos;
					if ((pos += bit_mask) > table_mask)
					{
						return 1;
					}
					uint fill = bit_mask;
					while (fill-- != 0)
					{
						table[leaf++] = sym;
					}
				}
			}
			bit_mask >>= 1;
			bit_num++;
		}
		if (pos != table_mask)
		{
			for (ushort sym = (ushort)pos; sym < table_mask; sym++)
			{
				table[sym] = 0;
			}
			pos <<= 16;
			table_mask <<= 16;
			bit_mask = 32768u;
			while (bit_num <= 16)
			{
				for (ushort sym = 0; sym < nsyms; sym++)
				{
					if (length[sym] == bit_num)
					{
						uint leaf = pos >> 16;
						for (uint fill = 0u; fill < bit_num - nbits; fill++)
						{
							if (table[leaf] == 0)
							{
								table[next_symbol << 1] = 0;
								table[(next_symbol << 1) + 1] = 0;
								table[leaf] = (ushort)next_symbol++;
							}
							leaf = (uint)(table[leaf] << 1);
							if (((pos >> (int)(15 - fill)) & 1) == 1)
							{
								leaf++;
							}
						}
						table[leaf] = sym;
						if ((pos += bit_mask) > table_mask)
						{
							return 1;
						}
					}
				}
				bit_mask >>= 1;
				bit_num++;
			}
		}
		if (pos == table_mask)
		{
			return 0;
		}
		for (ushort sym = 0; sym < nsyms; sym++)
		{
			if (length[sym] != 0)
			{
				return 1;
			}
		}
		return 0;
	}

	private void ReadLengths(byte[] lens, uint first, uint last, BitBuffer bitbuf)
	{
		uint x;
		for (x = 0u; x < 20; x++)
		{
			uint y = bitbuf.ReadBits(4);
			this.m_state.PRETREE_len[x] = (byte)y;
		}
		this.MakeDecodeTable(20u, 6u, this.m_state.PRETREE_len, this.m_state.PRETREE_table);
		x = first;
		while (x < last)
		{
			int z = (int)this.ReadHuffSym(this.m_state.PRETREE_table, this.m_state.PRETREE_len, 20u, 6u, bitbuf);
			switch (z)
			{
			case 17:
			{
				uint y = bitbuf.ReadBits(4);
				y += 4;
				while (y-- != 0)
				{
					lens[x++] = 0;
				}
				break;
			}
			case 18:
			{
				uint y = bitbuf.ReadBits(5);
				y += 20;
				while (y-- != 0)
				{
					lens[x++] = 0;
				}
				break;
			}
			case 19:
			{
				uint y = bitbuf.ReadBits(1);
				y += 4;
				z = (int)this.ReadHuffSym(this.m_state.PRETREE_table, this.m_state.PRETREE_len, 20u, 6u, bitbuf);
				z = lens[x] - z;
				if (z < 0)
				{
					z += 17;
				}
				while (y-- != 0)
				{
					lens[x++] = (byte)z;
				}
				break;
			}
			default:
				z = lens[x] - z;
				if (z < 0)
				{
					z += 17;
				}
				lens[x++] = (byte)z;
				break;
			}
		}
	}

	private uint ReadHuffSym(ushort[] table, byte[] lengths, uint nsyms, uint nbits, BitBuffer bitbuf)
	{
		bitbuf.EnsureBits(16);
		uint i;
		uint j;
		if ((i = table[bitbuf.PeekBits((byte)nbits)]) >= nsyms)
		{
			j = (uint)(1 << (int)(32 - nbits));
			do
			{
				j >>= 1;
				i <<= 1;
				i |= (((bitbuf.GetBuffer() & j) != 0) ? 1u : 0u);
				if (j == 0)
				{
					return 0u;
				}
			}
			while ((i = table[i]) >= nsyms);
		}
		j = lengths[i];
		bitbuf.RemoveBits((byte)j);
		return i;
	}
}
