using System;
using System.IO;
using System.Linq;

namespace NVorbis;

internal abstract class VorbisResidue
{
	private class Residue0 : VorbisResidue
	{
		private int _begin;

		private int _end;

		private int _partitionSize;

		private int _classifications;

		private int _maxStages;

		private VorbisCodebook[][] _books;

		private VorbisCodebook _classBook;

		private int[] _cascade;

		private int[] _entryCache;

		private int[][] _decodeMap;

		private int[][][] _partWordCache;

		internal Residue0(VorbisStreamDecoder vorbis)
			: base(vorbis)
		{
		}

		protected override void Init(DataPacket packet)
		{
			this._begin = (int)packet.ReadBits(24);
			this._end = (int)packet.ReadBits(24);
			this._partitionSize = (int)packet.ReadBits(24) + 1;
			this._classifications = (int)packet.ReadBits(6) + 1;
			this._classBook = base._vorbis.Books[(uint)packet.ReadBits(8)];
			this._cascade = new int[this._classifications];
			int acc = 0;
			for (int i = 0; i < this._classifications; i++)
			{
				int low_bits = (int)packet.ReadBits(3);
				if (packet.ReadBit())
				{
					this._cascade[i] = ((int)packet.ReadBits(5) << 3) | low_bits;
				}
				else
				{
					this._cascade[i] = low_bits;
				}
				acc += VorbisResidue.icount(this._cascade[i]);
			}
			int[] bookNums = new int[acc];
			for (int j = 0; j < acc; j++)
			{
				bookNums[j] = (int)packet.ReadBits(8);
				if (base._vorbis.Books[bookNums[j]].MapType == 0)
				{
					throw new InvalidDataException();
				}
			}
			int entries = this._classBook.Entries;
			int dim = this._classBook.Dimensions;
			int partvals = 1;
			while (dim > 0)
			{
				partvals *= this._classifications;
				if (partvals > entries)
				{
					throw new InvalidDataException();
				}
				dim--;
			}
			dim = this._classBook.Dimensions;
			this._books = new VorbisCodebook[this._classifications][];
			acc = 0;
			int maxstage = 0;
			for (int k = 0; k < this._classifications; k++)
			{
				int stages = Utils.ilog(this._cascade[k]);
				this._books[k] = new VorbisCodebook[stages];
				if (stages <= 0)
				{
					continue;
				}
				maxstage = Math.Max(maxstage, stages);
				for (int l = 0; l < stages; l++)
				{
					if ((this._cascade[k] & (1 << l)) > 0)
					{
						this._books[k][l] = base._vorbis.Books[bookNums[acc++]];
					}
				}
			}
			this._maxStages = maxstage;
			this._decodeMap = new int[partvals][];
			for (int m = 0; m < partvals; m++)
			{
				int val = m;
				int mult = partvals / this._classifications;
				this._decodeMap[m] = new int[this._classBook.Dimensions];
				for (int n = 0; n < this._classBook.Dimensions; n++)
				{
					int deco = val / mult;
					val -= deco * mult;
					mult /= this._classifications;
					this._decodeMap[m][n] = deco;
				}
			}
			this._entryCache = new int[this._partitionSize];
			this._partWordCache = new int[base._vorbis._channels][][];
			int maxPartWords = ((this._end - this._begin) / this._partitionSize + this._classBook.Dimensions - 1) / this._classBook.Dimensions;
			for (int ch = 0; ch < base._vorbis._channels; ch++)
			{
				this._partWordCache[ch] = new int[maxPartWords][];
			}
		}

		internal override float[][] Decode(DataPacket packet, bool[] doNotDecode, int channels, int blockSize)
		{
			float[][] residue = base.GetResidueBuffer(doNotDecode.Length);
			int n = ((this._end < blockSize / 2) ? this._end : (blockSize / 2)) - this._begin;
			if (n > 0 && doNotDecode.Contains(value: false))
			{
				int partVals = n / this._partitionSize;
				int partWords = (partVals + this._classBook.Dimensions - 1) / this._classBook.Dimensions;
				for (int j = 0; j < channels; j++)
				{
					Array.Clear(this._partWordCache[j], 0, partWords);
				}
				for (int s = 0; s < this._maxStages; s++)
				{
					int i = 0;
					int l = 0;
					while (i < partVals)
					{
						if (s == 0)
						{
							for (int k = 0; k < channels; k++)
							{
								int idx = this._classBook.DecodeScalar(packet);
								if (idx >= 0 && idx < this._decodeMap.Length)
								{
									this._partWordCache[k][l] = this._decodeMap[idx];
									continue;
								}
								i = partVals;
								s = this._maxStages;
								break;
							}
						}
						int k2 = 0;
						for (; i < partVals; i++)
						{
							if (k2 >= this._classBook.Dimensions)
							{
								break;
							}
							int offset = this._begin + i * this._partitionSize;
							for (int m = 0; m < channels; m++)
							{
								int idx2 = this._partWordCache[m][l][k2];
								if ((this._cascade[idx2] & (1 << s)) != 0)
								{
									VorbisCodebook book = this._books[idx2][s];
									if (book != null && this.WriteVectors(book, packet, residue, m, offset, this._partitionSize))
									{
										i = partVals;
										s = this._maxStages;
										break;
									}
								}
							}
							k2++;
						}
						l++;
					}
				}
			}
			return residue;
		}

		protected virtual bool WriteVectors(VorbisCodebook codebook, DataPacket packet, float[][] residue, int channel, int offset, int partitionSize)
		{
			float[] res = residue[channel];
			int step = partitionSize / codebook.Dimensions;
			for (int i = 0; i < step; i++)
			{
				if ((this._entryCache[i] = codebook.DecodeScalar(packet)) == -1)
				{
					return true;
				}
			}
			for (int j = 0; j < codebook.Dimensions; j++)
			{
				int j2 = 0;
				while (j2 < step)
				{
					res[offset] += codebook[this._entryCache[j2], j];
					j2++;
					offset++;
				}
			}
			return false;
		}
	}

	private class Residue1 : Residue0
	{
		internal Residue1(VorbisStreamDecoder vorbis)
			: base(vorbis)
		{
		}

		protected override bool WriteVectors(VorbisCodebook codebook, DataPacket packet, float[][] residue, int channel, int offset, int partitionSize)
		{
			float[] res = residue[channel];
			int i = 0;
			while (i < partitionSize)
			{
				int entry = codebook.DecodeScalar(packet);
				if (entry == -1)
				{
					return true;
				}
				for (int j = 0; j < codebook.Dimensions; j++)
				{
					res[offset + i] += codebook[entry, j];
					i++;
				}
			}
			return false;
		}
	}

	private class Residue2 : Residue0
	{
		private int _channels;

		internal Residue2(VorbisStreamDecoder vorbis)
			: base(vorbis)
		{
		}

		internal override float[][] Decode(DataPacket packet, bool[] doNotDecode, int channels, int blockSize)
		{
			this._channels = channels;
			return base.Decode(packet, doNotDecode, 1, blockSize * channels);
		}

		protected override bool WriteVectors(VorbisCodebook codebook, DataPacket packet, float[][] residue, int channel, int offset, int partitionSize)
		{
			int chPtr = 0;
			offset /= this._channels;
			int c = 0;
			while (c < partitionSize)
			{
				int entry = codebook.DecodeScalar(packet);
				if (entry == -1)
				{
					return true;
				}
				int d = 0;
				while (d < codebook.Dimensions)
				{
					residue[chPtr][offset] += codebook[entry, d];
					if (++chPtr == this._channels)
					{
						chPtr = 0;
						offset++;
					}
					d++;
					c++;
				}
			}
			return false;
		}
	}

	private VorbisStreamDecoder _vorbis;

	private float[][] _residue;

	internal static VorbisResidue Init(VorbisStreamDecoder vorbis, DataPacket packet)
	{
		int type = (int)packet.ReadBits(16);
		VorbisResidue residue = null;
		switch (type)
		{
		case 0:
			residue = new Residue0(vorbis);
			break;
		case 1:
			residue = new Residue1(vorbis);
			break;
		case 2:
			residue = new Residue2(vorbis);
			break;
		}
		if (residue == null)
		{
			throw new InvalidDataException();
		}
		residue.Init(packet);
		return residue;
	}

	private static int icount(int v)
	{
		int ret = 0;
		while (v != 0)
		{
			ret += v & 1;
			v >>= 1;
		}
		return ret;
	}

	protected VorbisResidue(VorbisStreamDecoder vorbis)
	{
		this._vorbis = vorbis;
		this._residue = new float[this._vorbis._channels][];
		for (int i = 0; i < this._vorbis._channels; i++)
		{
			this._residue[i] = new float[this._vorbis.Block1Size];
		}
	}

	protected float[][] GetResidueBuffer(int channels)
	{
		float[][] temp = this._residue;
		if (channels < this._vorbis._channels)
		{
			temp = new float[channels][];
			Array.Copy(this._residue, temp, channels);
		}
		for (int i = 0; i < channels; i++)
		{
			Array.Clear(temp[i], 0, temp[i].Length);
		}
		return temp;
	}

	internal abstract float[][] Decode(DataPacket packet, bool[] doNotDecode, int channels, int blockSize);

	protected abstract void Init(DataPacket packet);
}
