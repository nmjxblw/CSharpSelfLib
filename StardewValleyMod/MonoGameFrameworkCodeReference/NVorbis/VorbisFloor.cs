using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NVorbis;

internal abstract class VorbisFloor
{
	internal abstract class PacketData
	{
		internal int BlockSize;

		protected abstract bool HasEnergy { get; }

		internal bool ForceEnergy { get; set; }

		internal bool ForceNoEnergy { get; set; }

		internal bool ExecuteChannel => (this.ForceEnergy | this.HasEnergy) & !this.ForceNoEnergy;
	}

	private class Floor0 : VorbisFloor
	{
		private class PacketData0 : PacketData
		{
			internal float[] Coeff;

			internal float Amp;

			protected override bool HasEnergy => this.Amp > 0f;
		}

		private int _order;

		private int _rate;

		private int _bark_map_size;

		private int _ampBits;

		private int _ampOfs;

		private int _ampDiv;

		private VorbisCodebook[] _books;

		private int _bookBits;

		private Dictionary<int, float[]> _wMap;

		private Dictionary<int, int[]> _barkMaps;

		private PacketData0[] _reusablePacketData;

		internal Floor0(VorbisStreamDecoder vorbis)
			: base(vorbis)
		{
		}

		protected override void Init(DataPacket packet)
		{
			this._order = (int)packet.ReadBits(8);
			this._rate = (int)packet.ReadBits(16);
			this._bark_map_size = (int)packet.ReadBits(16);
			this._ampBits = (int)packet.ReadBits(6);
			this._ampOfs = (int)packet.ReadBits(8);
			this._books = new VorbisCodebook[(int)packet.ReadBits(4) + 1];
			if (this._order < 1 || this._rate < 1 || this._bark_map_size < 1 || this._books.Length == 0)
			{
				throw new InvalidDataException();
			}
			this._ampDiv = (1 << this._ampBits) - 1;
			for (int i = 0; i < this._books.Length; i++)
			{
				int num = (int)packet.ReadBits(8);
				if (num < 0 || num >= base._vorbis.Books.Length)
				{
					throw new InvalidDataException();
				}
				VorbisCodebook book = base._vorbis.Books[num];
				if (book.MapType == 0 || book.Dimensions < 1)
				{
					throw new InvalidDataException();
				}
				this._books[i] = book;
			}
			this._bookBits = Utils.ilog(this._books.Length);
			this._barkMaps = new Dictionary<int, int[]>();
			this._barkMaps[base._vorbis.Block0Size] = this.SynthesizeBarkCurve(base._vorbis.Block0Size / 2);
			this._barkMaps[base._vorbis.Block1Size] = this.SynthesizeBarkCurve(base._vorbis.Block1Size / 2);
			this._wMap = new Dictionary<int, float[]>();
			this._wMap[base._vorbis.Block0Size] = this.SynthesizeWDelMap(base._vorbis.Block0Size / 2);
			this._wMap[base._vorbis.Block1Size] = this.SynthesizeWDelMap(base._vorbis.Block1Size / 2);
			this._reusablePacketData = new PacketData0[base._vorbis._channels];
			for (int j = 0; j < this._reusablePacketData.Length; j++)
			{
				this._reusablePacketData[j] = new PacketData0
				{
					Coeff = new float[this._order + 1]
				};
			}
		}

		private int[] SynthesizeBarkCurve(int n)
		{
			float scale = (float)this._bark_map_size / Floor0.toBARK(this._rate / 2);
			int[] map = new int[n + 1];
			for (int i = 0; i < n - 1; i++)
			{
				map[i] = Math.Min(this._bark_map_size - 1, (int)Math.Floor(Floor0.toBARK((float)this._rate / 2f / (float)n * (float)i) * scale));
			}
			map[n] = -1;
			return map;
		}

		private static float toBARK(double lsp)
		{
			return (float)(13.1 * Math.Atan(0.00074 * lsp) + 2.24 * Math.Atan(1.85E-08 * lsp * lsp) + 0.0001 * lsp);
		}

		private float[] SynthesizeWDelMap(int n)
		{
			float wdel = (float)(Math.PI / (double)this._bark_map_size);
			float[] map = new float[n];
			for (int i = 0; i < n; i++)
			{
				map[i] = 2f * (float)Math.Cos(wdel * (float)i);
			}
			return map;
		}

		internal override PacketData UnpackPacket(DataPacket packet, int blockSize, int channel)
		{
			PacketData0 data = this._reusablePacketData[channel];
			data.BlockSize = blockSize;
			data.ForceEnergy = false;
			data.ForceNoEnergy = false;
			data.Amp = packet.ReadBits(this._ampBits);
			if (data.Amp > 0f)
			{
				Array.Clear(data.Coeff, 0, data.Coeff.Length);
				data.Amp = data.Amp / (float)this._ampDiv * (float)this._ampOfs;
				uint bookNum = (uint)packet.ReadBits(this._bookBits);
				if (bookNum >= this._books.Length)
				{
					data.Amp = 0f;
					return data;
				}
				VorbisCodebook book = this._books[bookNum];
				int i = 0;
				while (i < this._order)
				{
					int entry = book.DecodeScalar(packet);
					if (entry == -1)
					{
						data.Amp = 0f;
						return data;
					}
					int j = 0;
					for (; i < this._order; i++)
					{
						if (j >= book.Dimensions)
						{
							break;
						}
						data.Coeff[i] = book[entry, j];
						j++;
					}
				}
				float last = 0f;
				int j2 = 0;
				while (j2 < this._order)
				{
					int k = 0;
					while (j2 < this._order && k < book.Dimensions)
					{
						data.Coeff[j2] += last;
						j2++;
						k++;
					}
					last = data.Coeff[j2 - 1];
				}
			}
			return data;
		}

		internal override void Apply(PacketData packetData, float[] residue)
		{
			if (!(packetData is PacketData0 data))
			{
				throw new ArgumentException("Incorrect packet data!");
			}
			int n = data.BlockSize / 2;
			if (data.Amp > 0f)
			{
				int[] barkMap = this._barkMaps[data.BlockSize];
				float[] wMap = this._wMap[data.BlockSize];
				int i = 0;
				for (i = 0; i < this._order; i++)
				{
					data.Coeff[i] = 2f * (float)Math.Cos(data.Coeff[i]);
				}
				i = 0;
				while (i < n)
				{
					int k = barkMap[i];
					float p = 0.5f;
					float q = 0.5f;
					float w = wMap[k];
					int j;
					for (j = 1; j < this._order; j += 2)
					{
						q *= w - data.Coeff[j - 1];
						p *= w - data.Coeff[j];
					}
					if (j == this._order)
					{
						q *= w - data.Coeff[j - 1];
						p *= p * (4f - w * w);
						q *= q;
					}
					else
					{
						p *= p * (2f - w);
						q *= q * (2f + w);
					}
					q = data.Amp / (float)Math.Sqrt(p + q) - (float)this._ampOfs;
					q = (float)Math.Exp(q * 0.11512925f);
					residue[i] *= q;
					while (barkMap[++i] == k)
					{
						residue[i] *= q;
					}
				}
			}
			else
			{
				Array.Clear(residue, 0, n);
			}
		}
	}

	private class Floor1 : VorbisFloor
	{
		private class PacketData1 : PacketData
		{
			public int[] Posts = new int[64];

			public int PostCount;

			protected override bool HasEnergy => this.PostCount > 0;
		}

		private int[] _partitionClass;

		private int[] _classDimensions;

		private int[] _classSubclasses;

		private int[] _xList;

		private int[] _classMasterBookIndex;

		private int[] _hNeigh;

		private int[] _lNeigh;

		private int[] _sortIdx;

		private int _multiplier;

		private int _range;

		private int _yBits;

		private VorbisCodebook[] _classMasterbooks;

		private VorbisCodebook[][] _subclassBooks;

		private int[][] _subclassBookIndex;

		private static int[] _rangeLookup = new int[4] { 256, 128, 86, 64 };

		private static int[] _yBitsLookup = new int[4] { 8, 7, 7, 6 };

		private PacketData1[] _reusablePacketData;

		private bool[] _stepFlags = new bool[64];

		private int[] _finalY = new int[64];

		private static readonly float[] inverse_dB_table = new float[256]
		{
			1.0649863E-07f, 1.1341951E-07f, 1.2079015E-07f, 1.2863978E-07f, 1.369995E-07f, 1.459025E-07f, 1.5538409E-07f, 1.6548181E-07f, 1.7623574E-07f, 1.8768856E-07f,
			1.998856E-07f, 2.128753E-07f, 2.2670913E-07f, 2.4144197E-07f, 2.5713223E-07f, 2.7384212E-07f, 2.9163792E-07f, 3.1059022E-07f, 3.307741E-07f, 3.5226967E-07f,
			3.7516213E-07f, 3.995423E-07f, 4.255068E-07f, 4.5315863E-07f, 4.8260745E-07f, 5.1397E-07f, 5.4737063E-07f, 5.829419E-07f, 6.208247E-07f, 6.611694E-07f,
			7.041359E-07f, 7.4989464E-07f, 7.98627E-07f, 8.505263E-07f, 9.057983E-07f, 9.646621E-07f, 1.0273513E-06f, 1.0941144E-06f, 1.1652161E-06f, 1.2409384E-06f,
			1.3215816E-06f, 1.4074654E-06f, 1.4989305E-06f, 1.5963394E-06f, 1.7000785E-06f, 1.8105592E-06f, 1.9282195E-06f, 2.053526E-06f, 2.1869757E-06f, 2.3290977E-06f,
			2.4804558E-06f, 2.6416496E-06f, 2.813319E-06f, 2.9961443E-06f, 3.1908505E-06f, 3.39821E-06f, 3.619045E-06f, 3.8542307E-06f, 4.1047006E-06f, 4.371447E-06f,
			4.6555283E-06f, 4.958071E-06f, 5.280274E-06f, 5.623416E-06f, 5.988857E-06f, 6.3780467E-06f, 6.7925284E-06f, 7.2339453E-06f, 7.704048E-06f, 8.2047E-06f,
			8.737888E-06f, 9.305725E-06f, 9.910464E-06f, 1.0554501E-05f, 1.1240392E-05f, 1.1970856E-05f, 1.2748789E-05f, 1.3577278E-05f, 1.4459606E-05f, 1.5399271E-05f,
			1.6400005E-05f, 1.7465769E-05f, 1.8600793E-05f, 1.9809577E-05f, 2.1096914E-05f, 2.2467912E-05f, 2.3928002E-05f, 2.5482977E-05f, 2.7139005E-05f, 2.890265E-05f,
			3.078091E-05f, 3.2781227E-05f, 3.4911533E-05f, 3.718028E-05f, 3.9596467E-05f, 4.2169668E-05f, 4.491009E-05f, 4.7828602E-05f, 5.0936775E-05f, 5.424693E-05f,
			5.7772202E-05f, 6.152657E-05f, 6.552491E-05f, 6.9783084E-05f, 7.4317984E-05f, 7.914758E-05f, 8.429104E-05f, 8.976875E-05f, 9.560242E-05f, 0.00010181521f,
			0.00010843174f, 0.00011547824f, 0.00012298267f, 0.00013097477f, 0.00013948625f, 0.00014855085f, 0.00015820454f, 0.00016848555f, 0.00017943469f, 0.00019109536f,
			0.00020351382f, 0.0002167393f, 0.00023082423f, 0.00024582449f, 0.00026179955f, 0.00027881275f, 0.00029693157f, 0.00031622787f, 0.00033677815f, 0.00035866388f,
			0.00038197188f, 0.00040679457f, 0.00043323037f, 0.0004613841f, 0.0004913675f, 0.00052329927f, 0.0005573062f, 0.0005935231f, 0.0006320936f, 0.0006731706f,
			0.000716917f, 0.0007635063f, 0.00081312325f, 0.00086596457f, 0.00092223985f, 0.0009821722f, 0.0010459992f, 0.0011139743f, 0.0011863665f, 0.0012634633f,
			0.0013455702f, 0.0014330129f, 0.0015261382f, 0.0016253153f, 0.0017309374f, 0.0018434235f, 0.0019632196f, 0.0020908006f, 0.0022266726f, 0.0023713743f,
			0.0025254795f, 0.0026895993f, 0.0028643848f, 0.0030505287f, 0.003248769f, 0.0034598925f, 0.0036847359f, 0.0039241905f, 0.0041792067f, 0.004450795f,
			0.004740033f, 0.005048067f, 0.0053761187f, 0.005725489f, 0.0060975635f, 0.0064938175f, 0.0069158226f, 0.0073652514f, 0.007843887f, 0.008353627f,
			0.008896492f, 0.009474637f, 0.010090352f, 0.01074608f, 0.011444421f, 0.012188144f, 0.012980198f, 0.013823725f, 0.014722068f, 0.015678791f,
			0.016697686f, 0.017782796f, 0.018938422f, 0.020169148f, 0.021479854f, 0.022875736f, 0.02436233f, 0.025945531f, 0.027631618f, 0.029427277f,
			0.031339627f, 0.03337625f, 0.035545226f, 0.037855156f, 0.0403152f, 0.042935107f, 0.045725275f, 0.048696756f, 0.05186135f, 0.05523159f,
			0.05882085f, 0.062643364f, 0.06671428f, 0.07104975f, 0.075666964f, 0.08058423f, 0.08582105f, 0.09139818f, 0.097337745f, 0.1036633f,
			0.11039993f, 0.11757434f, 0.12521498f, 0.13335215f, 0.14201812f, 0.15124726f, 0.16107617f, 0.1715438f, 0.18269168f, 0.19456401f,
			0.20720787f, 0.22067343f, 0.23501402f, 0.25028655f, 0.26655158f, 0.28387362f, 0.3023213f, 0.32196787f, 0.34289113f, 0.36517414f,
			0.3889052f, 0.41417846f, 0.44109413f, 0.4697589f, 0.50028646f, 0.53279793f, 0.5674221f, 0.6042964f, 0.64356697f, 0.6853896f,
			0.72993004f, 0.777365f, 0.8278826f, 0.88168305f, 0.9389798f, 1f
		};

		internal Floor1(VorbisStreamDecoder vorbis)
			: base(vorbis)
		{
		}

		protected override void Init(DataPacket packet)
		{
			this._partitionClass = new int[(uint)packet.ReadBits(5)];
			for (int i = 0; i < this._partitionClass.Length; i++)
			{
				this._partitionClass[i] = (int)packet.ReadBits(4);
			}
			int maximum_class = this._partitionClass.Max();
			this._classDimensions = new int[maximum_class + 1];
			this._classSubclasses = new int[maximum_class + 1];
			this._classMasterbooks = new VorbisCodebook[maximum_class + 1];
			this._classMasterBookIndex = new int[maximum_class + 1];
			this._subclassBooks = new VorbisCodebook[maximum_class + 1][];
			this._subclassBookIndex = new int[maximum_class + 1][];
			for (int j = 0; j <= maximum_class; j++)
			{
				this._classDimensions[j] = (int)packet.ReadBits(3) + 1;
				this._classSubclasses[j] = (int)packet.ReadBits(2);
				if (this._classSubclasses[j] > 0)
				{
					this._classMasterBookIndex[j] = (int)packet.ReadBits(8);
					this._classMasterbooks[j] = base._vorbis.Books[this._classMasterBookIndex[j]];
				}
				this._subclassBooks[j] = new VorbisCodebook[1 << this._classSubclasses[j]];
				this._subclassBookIndex[j] = new int[this._subclassBooks[j].Length];
				for (int k = 0; k < this._subclassBooks[j].Length; k++)
				{
					int bookNum = (int)packet.ReadBits(8) - 1;
					if (bookNum >= 0)
					{
						this._subclassBooks[j][k] = base._vorbis.Books[bookNum];
					}
					this._subclassBookIndex[j][k] = bookNum;
				}
			}
			this._multiplier = (int)packet.ReadBits(2);
			this._range = Floor1._rangeLookup[this._multiplier];
			this._yBits = Floor1._yBitsLookup[this._multiplier];
			this._multiplier++;
			int rangeBits = (int)packet.ReadBits(4);
			List<int> xList = new List<int>();
			xList.Add(0);
			xList.Add(1 << rangeBits);
			for (int l = 0; l < this._partitionClass.Length; l++)
			{
				int classNum = this._partitionClass[l];
				for (int m = 0; m < this._classDimensions[classNum]; m++)
				{
					xList.Add((int)packet.ReadBits(rangeBits));
				}
			}
			this._xList = xList.ToArray();
			this._lNeigh = new int[xList.Count];
			this._hNeigh = new int[xList.Count];
			this._sortIdx = new int[xList.Count];
			this._sortIdx[0] = 0;
			this._sortIdx[1] = 1;
			for (int n = 2; n < this._lNeigh.Length; n++)
			{
				this._lNeigh[n] = 0;
				this._hNeigh[n] = 1;
				this._sortIdx[n] = n;
				for (int num = 2; num < n; num++)
				{
					int temp = this._xList[num];
					if (temp < this._xList[n])
					{
						if (temp > this._xList[this._lNeigh[n]])
						{
							this._lNeigh[n] = num;
						}
					}
					else if (temp < this._xList[this._hNeigh[n]])
					{
						this._hNeigh[n] = num;
					}
				}
			}
			for (int num2 = 0; num2 < this._sortIdx.Length - 1; num2++)
			{
				for (int num3 = num2 + 1; num3 < this._sortIdx.Length; num3++)
				{
					if (this._xList[num2] == this._xList[num3])
					{
						throw new InvalidDataException();
					}
					if (this._xList[this._sortIdx[num2]] > this._xList[this._sortIdx[num3]])
					{
						int temp2 = this._sortIdx[num2];
						this._sortIdx[num2] = this._sortIdx[num3];
						this._sortIdx[num3] = temp2;
					}
				}
			}
			this._reusablePacketData = new PacketData1[base._vorbis._channels];
			for (int num4 = 0; num4 < this._reusablePacketData.Length; num4++)
			{
				this._reusablePacketData[num4] = new PacketData1();
			}
		}

		internal override PacketData UnpackPacket(DataPacket packet, int blockSize, int channel)
		{
			PacketData1 data = this._reusablePacketData[channel];
			data.BlockSize = blockSize;
			data.ForceEnergy = false;
			data.ForceNoEnergy = false;
			data.PostCount = 0;
			Array.Clear(data.Posts, 0, 64);
			if (packet.ReadBit())
			{
				int postCount = 2;
				data.Posts[0] = (int)packet.ReadBits(this._yBits);
				data.Posts[1] = (int)packet.ReadBits(this._yBits);
				for (int i = 0; i < this._partitionClass.Length; i++)
				{
					int clsNum = this._partitionClass[i];
					int cdim = this._classDimensions[clsNum];
					int cbits = this._classSubclasses[clsNum];
					int csub = (1 << cbits) - 1;
					uint cval = 0u;
					if (cbits > 0 && (cval = (uint)this._classMasterbooks[clsNum].DecodeScalar(packet)) == uint.MaxValue)
					{
						postCount = 0;
						break;
					}
					for (int j = 0; j < cdim; j++)
					{
						VorbisCodebook book = this._subclassBooks[clsNum][cval & csub];
						cval >>= cbits;
						if (book != null && (data.Posts[postCount] = book.DecodeScalar(packet)) == -1)
						{
							postCount = 0;
							i = this._partitionClass.Length;
							break;
						}
						postCount++;
					}
				}
				data.PostCount = postCount;
			}
			return data;
		}

		internal override void Apply(PacketData packetData, float[] residue)
		{
			if (!(packetData is PacketData1 data))
			{
				throw new ArgumentException("Incorrect packet data!", "packetData");
			}
			int n = data.BlockSize / 2;
			if (data.PostCount > 0)
			{
				bool[] stepFlags = this.UnwrapPosts(data);
				int lx = 0;
				int ly = data.Posts[0] * this._multiplier;
				for (int i = 1; i < data.PostCount; i++)
				{
					int idx = this._sortIdx[i];
					if (stepFlags[idx])
					{
						int hx = this._xList[idx];
						int hy = data.Posts[idx] * this._multiplier;
						if (lx < n)
						{
							this.RenderLineMulti(lx, ly, Math.Min(hx, n), hy, residue);
						}
						lx = hx;
						ly = hy;
					}
					if (lx >= n)
					{
						break;
					}
				}
				if (lx < n)
				{
					this.RenderLineMulti(lx, ly, n, ly, residue);
				}
			}
			else
			{
				Array.Clear(residue, 0, n);
			}
		}

		private bool[] UnwrapPosts(PacketData1 data)
		{
			Array.Clear(this._stepFlags, 2, 62);
			this._stepFlags[0] = true;
			this._stepFlags[1] = true;
			Array.Clear(this._finalY, 2, 62);
			this._finalY[0] = data.Posts[0];
			this._finalY[1] = data.Posts[1];
			for (int i = 2; i < data.PostCount; i++)
			{
				int lowOfs = this._lNeigh[i];
				int highOfs = this._hNeigh[i];
				int predicted = this.RenderPoint(this._xList[lowOfs], this._finalY[lowOfs], this._xList[highOfs], this._finalY[highOfs], this._xList[i]);
				int val = data.Posts[i];
				int highroom = this._range - predicted;
				int lowroom = predicted;
				int room = ((highroom >= lowroom) ? (lowroom * 2) : (highroom * 2));
				if (val != 0)
				{
					this._stepFlags[lowOfs] = true;
					this._stepFlags[highOfs] = true;
					this._stepFlags[i] = true;
					if (val >= room)
					{
						if (highroom > lowroom)
						{
							this._finalY[i] = val - lowroom + predicted;
						}
						else
						{
							this._finalY[i] = predicted - val + highroom - 1;
						}
					}
					else if (val % 2 == 1)
					{
						this._finalY[i] = predicted - (val + 1) / 2;
					}
					else
					{
						this._finalY[i] = predicted + val / 2;
					}
				}
				else
				{
					this._stepFlags[i] = false;
					this._finalY[i] = predicted;
				}
			}
			for (int j = 0; j < data.PostCount; j++)
			{
				data.Posts[j] = this._finalY[j];
			}
			return this._stepFlags;
		}

		private int RenderPoint(int x0, int y0, int x1, int y1, int X)
		{
			int num = y1 - y0;
			int adx = x1 - x0;
			int off = Math.Abs(num) * (X - x0) / adx;
			if (num < 0)
			{
				return y0 - off;
			}
			return y0 + off;
		}

		private void RenderLineMulti(int x0, int y0, int x1, int y1, float[] v)
		{
			int dy = y1 - y0;
			int adx = x1 - x0;
			int ady = Math.Abs(dy);
			int sy = 1 - ((dy >> 31) & 1) * 2;
			int b = dy / adx;
			int x2 = x0;
			int y2 = y0;
			int err = -adx;
			v[x0] *= Floor1.inverse_dB_table[y0];
			ady -= Math.Abs(b) * adx;
			while (++x2 < x1)
			{
				y2 += b;
				err += ady;
				if (err >= 0)
				{
					err -= adx;
					y2 += sy;
				}
				v[x2] *= Floor1.inverse_dB_table[y2];
			}
		}
	}

	private VorbisStreamDecoder _vorbis;

	internal static VorbisFloor Init(VorbisStreamDecoder vorbis, DataPacket packet)
	{
		int type = (int)packet.ReadBits(16);
		VorbisFloor floor = null;
		switch (type)
		{
		case 0:
			floor = new Floor0(vorbis);
			break;
		case 1:
			floor = new Floor1(vorbis);
			break;
		}
		if (floor == null)
		{
			throw new InvalidDataException();
		}
		floor.Init(packet);
		return floor;
	}

	protected VorbisFloor(VorbisStreamDecoder vorbis)
	{
		this._vorbis = vorbis;
	}

	protected abstract void Init(DataPacket packet);

	internal abstract PacketData UnpackPacket(DataPacket packet, int blockSize, int channel);

	internal abstract void Apply(PacketData packetData, float[] residue);
}
