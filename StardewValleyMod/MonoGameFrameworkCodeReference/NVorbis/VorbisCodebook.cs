using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NVorbis;

internal class VorbisCodebook
{
	internal int BookNum;

	internal int Dimensions;

	internal int Entries;

	private int[] Lengths;

	private float[] LookupTable;

	internal int MapType;

	private HuffmanListNode PrefixOverflowTree;

	private List<HuffmanListNode> PrefixList;

	private int PrefixBitLength;

	private int MaxBits;

	internal float this[int entry, int dim] => this.LookupTable[entry * this.Dimensions + dim];

	internal static VorbisCodebook Init(VorbisStreamDecoder vorbis, DataPacket packet, int number)
	{
		VorbisCodebook vorbisCodebook = new VorbisCodebook();
		vorbisCodebook.BookNum = number;
		vorbisCodebook.Init(packet);
		return vorbisCodebook;
	}

	private VorbisCodebook()
	{
	}

	internal void Init(DataPacket packet)
	{
		if (packet.ReadBits(24) != 5653314)
		{
			throw new InvalidDataException();
		}
		this.Dimensions = (int)packet.ReadBits(16);
		this.Entries = (int)packet.ReadBits(24);
		this.Lengths = new int[this.Entries];
		this.InitTree(packet);
		this.InitLookupTable(packet);
	}

	private void InitTree(DataPacket packet)
	{
		int total = 0;
		bool sparse;
		if (packet.ReadBit())
		{
			int len = (int)packet.ReadBits(5) + 1;
			int i = 0;
			while (i < this.Entries)
			{
				int cnt = (int)packet.ReadBits(Utils.ilog(this.Entries - i));
				while (--cnt >= 0)
				{
					this.Lengths[i++] = len;
				}
				len++;
			}
			total = 0;
			sparse = false;
		}
		else
		{
			sparse = packet.ReadBit();
			for (int j = 0; j < this.Entries; j++)
			{
				if (!sparse || packet.ReadBit())
				{
					this.Lengths[j] = (int)packet.ReadBits(5) + 1;
					total++;
				}
				else
				{
					this.Lengths[j] = -1;
				}
			}
		}
		if ((this.MaxBits = this.Lengths.Max()) > -1)
		{
			int sortedCount = 0;
			int[] codewordLengths = null;
			if (sparse && total >= this.Entries >> 2)
			{
				codewordLengths = new int[this.Entries];
				Array.Copy(this.Lengths, codewordLengths, this.Entries);
				sparse = false;
			}
			sortedCount = (sparse ? total : 0);
			int sortedEntries = sortedCount;
			int[] values = null;
			int[] codewords = null;
			if (!sparse)
			{
				codewords = new int[this.Entries];
			}
			else if (sortedEntries != 0)
			{
				codewordLengths = new int[sortedEntries];
				codewords = new int[sortedEntries];
				values = new int[sortedEntries];
			}
			if (!this.ComputeCodewords(sparse, sortedEntries, codewords, codewordLengths, this.Lengths, this.Entries, values))
			{
				throw new InvalidDataException();
			}
			this.PrefixList = Huffman.BuildPrefixedLinkedList(values ?? Enumerable.Range(0, codewords.Length).ToArray(), codewordLengths ?? this.Lengths, codewords, out this.PrefixBitLength, out this.PrefixOverflowTree);
		}
	}

	private bool ComputeCodewords(bool sparse, int sortedEntries, int[] codewords, int[] codewordLengths, int[] len, int n, int[] values)
	{
		int m = 0;
		uint[] available = new uint[32];
		int k;
		for (k = 0; k < n && len[k] <= 0; k++)
		{
		}
		if (k == n)
		{
			return true;
		}
		this.AddEntry(sparse, codewords, codewordLengths, 0u, k, m++, len[k], values);
		for (int i = 1; i <= len[k]; i++)
		{
			available[i] = (uint)(1 << 32 - i);
		}
		for (int i = k + 1; i < n; i++)
		{
			int z = len[i];
			if (z <= 0)
			{
				continue;
			}
			while (z > 0 && available[z] == 0)
			{
				z--;
			}
			if (z == 0)
			{
				return false;
			}
			uint res = available[z];
			available[z] = 0u;
			this.AddEntry(sparse, codewords, codewordLengths, Utils.BitReverse(res), i, m++, len[i], values);
			if (z != len[i])
			{
				for (int y = len[i]; y > z; y--)
				{
					available[y] = res + (uint)(1 << 32 - y);
				}
			}
		}
		return true;
	}

	private void AddEntry(bool sparse, int[] codewords, int[] codewordLengths, uint huffCode, int symbol, int count, int len, int[] values)
	{
		if (sparse)
		{
			codewords[count] = (int)huffCode;
			codewordLengths[count] = len;
			values[count] = symbol;
		}
		else
		{
			codewords[symbol] = (int)huffCode;
		}
	}

	private void InitLookupTable(DataPacket packet)
	{
		this.MapType = (int)packet.ReadBits(4);
		if (this.MapType == 0)
		{
			return;
		}
		float minValue = Utils.ConvertFromVorbisFloat32(packet.ReadUInt32());
		float deltaValue = Utils.ConvertFromVorbisFloat32(packet.ReadUInt32());
		int valueBits = (int)packet.ReadBits(4) + 1;
		bool sequence_p = packet.ReadBit();
		int lookupValueCount = this.Entries * this.Dimensions;
		float[] lookupTable = new float[lookupValueCount];
		if (this.MapType == 1)
		{
			lookupValueCount = this.lookup1_values();
		}
		uint[] multiplicands = new uint[lookupValueCount];
		for (int i = 0; i < lookupValueCount; i++)
		{
			multiplicands[i] = (uint)packet.ReadBits(valueBits);
		}
		if (this.MapType == 1)
		{
			for (int idx = 0; idx < this.Entries; idx++)
			{
				double last = 0.0;
				int idxDiv = 1;
				for (int j = 0; j < this.Dimensions; j++)
				{
					int moff = idx / idxDiv % lookupValueCount;
					double value = (double)((float)multiplicands[moff] * deltaValue + minValue) + last;
					lookupTable[idx * this.Dimensions + j] = (float)value;
					if (sequence_p)
					{
						last = value;
					}
					idxDiv *= lookupValueCount;
				}
			}
		}
		else
		{
			for (int k = 0; k < this.Entries; k++)
			{
				double last2 = 0.0;
				int moff2 = k * this.Dimensions;
				for (int l = 0; l < this.Dimensions; l++)
				{
					double value2 = (double)((float)multiplicands[moff2] * deltaValue + minValue) + last2;
					lookupTable[k * this.Dimensions + l] = (float)value2;
					if (sequence_p)
					{
						last2 = value2;
					}
					moff2++;
				}
			}
		}
		this.LookupTable = lookupTable;
	}

	private int lookup1_values()
	{
		int r = (int)Math.Floor(Math.Exp(Math.Log(this.Entries) / (double)this.Dimensions));
		if (Math.Floor(Math.Pow(r + 1, this.Dimensions)) <= (double)this.Entries)
		{
			r++;
		}
		return r;
	}

	internal int DecodeScalar(DataPacket packet)
	{
		int bits = (int)packet.TryPeekBits(this.PrefixBitLength, out var bitCnt);
		if (bitCnt == 0)
		{
			return -1;
		}
		HuffmanListNode node = this.PrefixList[bits];
		if (node != null)
		{
			packet.SkipBits(node.Length);
			return node.Value;
		}
		bits = (int)packet.TryPeekBits(this.MaxBits, out bitCnt);
		node = this.PrefixOverflowTree;
		do
		{
			if (node.Bits == (bits & node.Mask))
			{
				packet.SkipBits(node.Length);
				return node.Value;
			}
		}
		while ((node = node.Next) != null);
		return -1;
	}
}
