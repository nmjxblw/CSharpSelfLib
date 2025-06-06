using System;

namespace MonoGame.Framework.Utilities;

internal sealed class InfTree
{
	private const int MANY = 1440;

	private const int Z_OK = 0;

	private const int Z_STREAM_END = 1;

	private const int Z_NEED_DICT = 2;

	private const int Z_ERRNO = -1;

	private const int Z_STREAM_ERROR = -2;

	private const int Z_DATA_ERROR = -3;

	private const int Z_MEM_ERROR = -4;

	private const int Z_BUF_ERROR = -5;

	private const int Z_VERSION_ERROR = -6;

	internal const int fixed_bl = 9;

	internal const int fixed_bd = 5;

	internal static readonly int[] fixed_tl = new int[1536]
	{
		96, 7, 256, 0, 8, 80, 0, 8, 16, 84,
		8, 115, 82, 7, 31, 0, 8, 112, 0, 8,
		48, 0, 9, 192, 80, 7, 10, 0, 8, 96,
		0, 8, 32, 0, 9, 160, 0, 8, 0, 0,
		8, 128, 0, 8, 64, 0, 9, 224, 80, 7,
		6, 0, 8, 88, 0, 8, 24, 0, 9, 144,
		83, 7, 59, 0, 8, 120, 0, 8, 56, 0,
		9, 208, 81, 7, 17, 0, 8, 104, 0, 8,
		40, 0, 9, 176, 0, 8, 8, 0, 8, 136,
		0, 8, 72, 0, 9, 240, 80, 7, 4, 0,
		8, 84, 0, 8, 20, 85, 8, 227, 83, 7,
		43, 0, 8, 116, 0, 8, 52, 0, 9, 200,
		81, 7, 13, 0, 8, 100, 0, 8, 36, 0,
		9, 168, 0, 8, 4, 0, 8, 132, 0, 8,
		68, 0, 9, 232, 80, 7, 8, 0, 8, 92,
		0, 8, 28, 0, 9, 152, 84, 7, 83, 0,
		8, 124, 0, 8, 60, 0, 9, 216, 82, 7,
		23, 0, 8, 108, 0, 8, 44, 0, 9, 184,
		0, 8, 12, 0, 8, 140, 0, 8, 76, 0,
		9, 248, 80, 7, 3, 0, 8, 82, 0, 8,
		18, 85, 8, 163, 83, 7, 35, 0, 8, 114,
		0, 8, 50, 0, 9, 196, 81, 7, 11, 0,
		8, 98, 0, 8, 34, 0, 9, 164, 0, 8,
		2, 0, 8, 130, 0, 8, 66, 0, 9, 228,
		80, 7, 7, 0, 8, 90, 0, 8, 26, 0,
		9, 148, 84, 7, 67, 0, 8, 122, 0, 8,
		58, 0, 9, 212, 82, 7, 19, 0, 8, 106,
		0, 8, 42, 0, 9, 180, 0, 8, 10, 0,
		8, 138, 0, 8, 74, 0, 9, 244, 80, 7,
		5, 0, 8, 86, 0, 8, 22, 192, 8, 0,
		83, 7, 51, 0, 8, 118, 0, 8, 54, 0,
		9, 204, 81, 7, 15, 0, 8, 102, 0, 8,
		38, 0, 9, 172, 0, 8, 6, 0, 8, 134,
		0, 8, 70, 0, 9, 236, 80, 7, 9, 0,
		8, 94, 0, 8, 30, 0, 9, 156, 84, 7,
		99, 0, 8, 126, 0, 8, 62, 0, 9, 220,
		82, 7, 27, 0, 8, 110, 0, 8, 46, 0,
		9, 188, 0, 8, 14, 0, 8, 142, 0, 8,
		78, 0, 9, 252, 96, 7, 256, 0, 8, 81,
		0, 8, 17, 85, 8, 131, 82, 7, 31, 0,
		8, 113, 0, 8, 49, 0, 9, 194, 80, 7,
		10, 0, 8, 97, 0, 8, 33, 0, 9, 162,
		0, 8, 1, 0, 8, 129, 0, 8, 65, 0,
		9, 226, 80, 7, 6, 0, 8, 89, 0, 8,
		25, 0, 9, 146, 83, 7, 59, 0, 8, 121,
		0, 8, 57, 0, 9, 210, 81, 7, 17, 0,
		8, 105, 0, 8, 41, 0, 9, 178, 0, 8,
		9, 0, 8, 137, 0, 8, 73, 0, 9, 242,
		80, 7, 4, 0, 8, 85, 0, 8, 21, 80,
		8, 258, 83, 7, 43, 0, 8, 117, 0, 8,
		53, 0, 9, 202, 81, 7, 13, 0, 8, 101,
		0, 8, 37, 0, 9, 170, 0, 8, 5, 0,
		8, 133, 0, 8, 69, 0, 9, 234, 80, 7,
		8, 0, 8, 93, 0, 8, 29, 0, 9, 154,
		84, 7, 83, 0, 8, 125, 0, 8, 61, 0,
		9, 218, 82, 7, 23, 0, 8, 109, 0, 8,
		45, 0, 9, 186, 0, 8, 13, 0, 8, 141,
		0, 8, 77, 0, 9, 250, 80, 7, 3, 0,
		8, 83, 0, 8, 19, 85, 8, 195, 83, 7,
		35, 0, 8, 115, 0, 8, 51, 0, 9, 198,
		81, 7, 11, 0, 8, 99, 0, 8, 35, 0,
		9, 166, 0, 8, 3, 0, 8, 131, 0, 8,
		67, 0, 9, 230, 80, 7, 7, 0, 8, 91,
		0, 8, 27, 0, 9, 150, 84, 7, 67, 0,
		8, 123, 0, 8, 59, 0, 9, 214, 82, 7,
		19, 0, 8, 107, 0, 8, 43, 0, 9, 182,
		0, 8, 11, 0, 8, 139, 0, 8, 75, 0,
		9, 246, 80, 7, 5, 0, 8, 87, 0, 8,
		23, 192, 8, 0, 83, 7, 51, 0, 8, 119,
		0, 8, 55, 0, 9, 206, 81, 7, 15, 0,
		8, 103, 0, 8, 39, 0, 9, 174, 0, 8,
		7, 0, 8, 135, 0, 8, 71, 0, 9, 238,
		80, 7, 9, 0, 8, 95, 0, 8, 31, 0,
		9, 158, 84, 7, 99, 0, 8, 127, 0, 8,
		63, 0, 9, 222, 82, 7, 27, 0, 8, 111,
		0, 8, 47, 0, 9, 190, 0, 8, 15, 0,
		8, 143, 0, 8, 79, 0, 9, 254, 96, 7,
		256, 0, 8, 80, 0, 8, 16, 84, 8, 115,
		82, 7, 31, 0, 8, 112, 0, 8, 48, 0,
		9, 193, 80, 7, 10, 0, 8, 96, 0, 8,
		32, 0, 9, 161, 0, 8, 0, 0, 8, 128,
		0, 8, 64, 0, 9, 225, 80, 7, 6, 0,
		8, 88, 0, 8, 24, 0, 9, 145, 83, 7,
		59, 0, 8, 120, 0, 8, 56, 0, 9, 209,
		81, 7, 17, 0, 8, 104, 0, 8, 40, 0,
		9, 177, 0, 8, 8, 0, 8, 136, 0, 8,
		72, 0, 9, 241, 80, 7, 4, 0, 8, 84,
		0, 8, 20, 85, 8, 227, 83, 7, 43, 0,
		8, 116, 0, 8, 52, 0, 9, 201, 81, 7,
		13, 0, 8, 100, 0, 8, 36, 0, 9, 169,
		0, 8, 4, 0, 8, 132, 0, 8, 68, 0,
		9, 233, 80, 7, 8, 0, 8, 92, 0, 8,
		28, 0, 9, 153, 84, 7, 83, 0, 8, 124,
		0, 8, 60, 0, 9, 217, 82, 7, 23, 0,
		8, 108, 0, 8, 44, 0, 9, 185, 0, 8,
		12, 0, 8, 140, 0, 8, 76, 0, 9, 249,
		80, 7, 3, 0, 8, 82, 0, 8, 18, 85,
		8, 163, 83, 7, 35, 0, 8, 114, 0, 8,
		50, 0, 9, 197, 81, 7, 11, 0, 8, 98,
		0, 8, 34, 0, 9, 165, 0, 8, 2, 0,
		8, 130, 0, 8, 66, 0, 9, 229, 80, 7,
		7, 0, 8, 90, 0, 8, 26, 0, 9, 149,
		84, 7, 67, 0, 8, 122, 0, 8, 58, 0,
		9, 213, 82, 7, 19, 0, 8, 106, 0, 8,
		42, 0, 9, 181, 0, 8, 10, 0, 8, 138,
		0, 8, 74, 0, 9, 245, 80, 7, 5, 0,
		8, 86, 0, 8, 22, 192, 8, 0, 83, 7,
		51, 0, 8, 118, 0, 8, 54, 0, 9, 205,
		81, 7, 15, 0, 8, 102, 0, 8, 38, 0,
		9, 173, 0, 8, 6, 0, 8, 134, 0, 8,
		70, 0, 9, 237, 80, 7, 9, 0, 8, 94,
		0, 8, 30, 0, 9, 157, 84, 7, 99, 0,
		8, 126, 0, 8, 62, 0, 9, 221, 82, 7,
		27, 0, 8, 110, 0, 8, 46, 0, 9, 189,
		0, 8, 14, 0, 8, 142, 0, 8, 78, 0,
		9, 253, 96, 7, 256, 0, 8, 81, 0, 8,
		17, 85, 8, 131, 82, 7, 31, 0, 8, 113,
		0, 8, 49, 0, 9, 195, 80, 7, 10, 0,
		8, 97, 0, 8, 33, 0, 9, 163, 0, 8,
		1, 0, 8, 129, 0, 8, 65, 0, 9, 227,
		80, 7, 6, 0, 8, 89, 0, 8, 25, 0,
		9, 147, 83, 7, 59, 0, 8, 121, 0, 8,
		57, 0, 9, 211, 81, 7, 17, 0, 8, 105,
		0, 8, 41, 0, 9, 179, 0, 8, 9, 0,
		8, 137, 0, 8, 73, 0, 9, 243, 80, 7,
		4, 0, 8, 85, 0, 8, 21, 80, 8, 258,
		83, 7, 43, 0, 8, 117, 0, 8, 53, 0,
		9, 203, 81, 7, 13, 0, 8, 101, 0, 8,
		37, 0, 9, 171, 0, 8, 5, 0, 8, 133,
		0, 8, 69, 0, 9, 235, 80, 7, 8, 0,
		8, 93, 0, 8, 29, 0, 9, 155, 84, 7,
		83, 0, 8, 125, 0, 8, 61, 0, 9, 219,
		82, 7, 23, 0, 8, 109, 0, 8, 45, 0,
		9, 187, 0, 8, 13, 0, 8, 141, 0, 8,
		77, 0, 9, 251, 80, 7, 3, 0, 8, 83,
		0, 8, 19, 85, 8, 195, 83, 7, 35, 0,
		8, 115, 0, 8, 51, 0, 9, 199, 81, 7,
		11, 0, 8, 99, 0, 8, 35, 0, 9, 167,
		0, 8, 3, 0, 8, 131, 0, 8, 67, 0,
		9, 231, 80, 7, 7, 0, 8, 91, 0, 8,
		27, 0, 9, 151, 84, 7, 67, 0, 8, 123,
		0, 8, 59, 0, 9, 215, 82, 7, 19, 0,
		8, 107, 0, 8, 43, 0, 9, 183, 0, 8,
		11, 0, 8, 139, 0, 8, 75, 0, 9, 247,
		80, 7, 5, 0, 8, 87, 0, 8, 23, 192,
		8, 0, 83, 7, 51, 0, 8, 119, 0, 8,
		55, 0, 9, 207, 81, 7, 15, 0, 8, 103,
		0, 8, 39, 0, 9, 175, 0, 8, 7, 0,
		8, 135, 0, 8, 71, 0, 9, 239, 80, 7,
		9, 0, 8, 95, 0, 8, 31, 0, 9, 159,
		84, 7, 99, 0, 8, 127, 0, 8, 63, 0,
		9, 223, 82, 7, 27, 0, 8, 111, 0, 8,
		47, 0, 9, 191, 0, 8, 15, 0, 8, 143,
		0, 8, 79, 0, 9, 255
	};

	internal static readonly int[] fixed_td = new int[96]
	{
		80, 5, 1, 87, 5, 257, 83, 5, 17, 91,
		5, 4097, 81, 5, 5, 89, 5, 1025, 85, 5,
		65, 93, 5, 16385, 80, 5, 3, 88, 5, 513,
		84, 5, 33, 92, 5, 8193, 82, 5, 9, 90,
		5, 2049, 86, 5, 129, 192, 5, 24577, 80, 5,
		2, 87, 5, 385, 83, 5, 25, 91, 5, 6145,
		81, 5, 7, 89, 5, 1537, 85, 5, 97, 93,
		5, 24577, 80, 5, 4, 88, 5, 769, 84, 5,
		49, 92, 5, 12289, 82, 5, 13, 90, 5, 3073,
		86, 5, 193, 192, 5, 24577
	};

	internal static readonly int[] cplens = new int[31]
	{
		3, 4, 5, 6, 7, 8, 9, 10, 11, 13,
		15, 17, 19, 23, 27, 31, 35, 43, 51, 59,
		67, 83, 99, 115, 131, 163, 195, 227, 258, 0,
		0
	};

	internal static readonly int[] cplext = new int[31]
	{
		0, 0, 0, 0, 0, 0, 0, 0, 1, 1,
		1, 1, 2, 2, 2, 2, 3, 3, 3, 3,
		4, 4, 4, 4, 5, 5, 5, 5, 0, 112,
		112
	};

	internal static readonly int[] cpdist = new int[30]
	{
		1, 2, 3, 4, 5, 7, 9, 13, 17, 25,
		33, 49, 65, 97, 129, 193, 257, 385, 513, 769,
		1025, 1537, 2049, 3073, 4097, 6145, 8193, 12289, 16385, 24577
	};

	internal static readonly int[] cpdext = new int[30]
	{
		0, 0, 0, 0, 1, 1, 2, 2, 3, 3,
		4, 4, 5, 5, 6, 6, 7, 7, 8, 8,
		9, 9, 10, 10, 11, 11, 12, 12, 13, 13
	};

	internal const int BMAX = 15;

	internal int[] hn;

	internal int[] v;

	internal int[] c;

	internal int[] r;

	internal int[] u;

	internal int[] x;

	private int huft_build(int[] b, int bindex, int n, int s, int[] d, int[] e, int[] t, int[] m, int[] hp, int[] hn, int[] v)
	{
		int p = 0;
		int i = n;
		do
		{
			this.c[b[bindex + p]]++;
			p++;
			i--;
		}
		while (i != 0);
		if (this.c[0] == n)
		{
			t[0] = -1;
			m[0] = 0;
			return 0;
		}
		int l = m[0];
		int j;
		for (j = 1; j <= 15 && this.c[j] == 0; j++)
		{
		}
		int k = j;
		if (l < j)
		{
			l = j;
		}
		i = 15;
		while (i != 0 && this.c[i] == 0)
		{
			i--;
		}
		int g = i;
		if (l > i)
		{
			l = i;
		}
		m[0] = l;
		int y = 1 << j;
		while (j < i)
		{
			if ((y -= this.c[j]) < 0)
			{
				return -3;
			}
			j++;
			y <<= 1;
		}
		if ((y -= this.c[i]) < 0)
		{
			return -3;
		}
		this.c[i] += y;
		j = (this.x[1] = 0);
		p = 1;
		int xp = 2;
		while (--i != 0)
		{
			j = (this.x[xp] = j + this.c[p]);
			xp++;
			p++;
		}
		i = 0;
		p = 0;
		do
		{
			if ((j = b[bindex + p]) != 0)
			{
				v[this.x[j]++] = i;
			}
			p++;
		}
		while (++i < n);
		n = this.x[g];
		i = (this.x[0] = 0);
		p = 0;
		int h = -1;
		int w = -l;
		this.u[0] = 0;
		int q = 0;
		int z = 0;
		for (; k <= g; k++)
		{
			int a = this.c[k];
			while (a-- != 0)
			{
				int f;
				while (k > w + l)
				{
					h++;
					w += l;
					z = g - w;
					z = ((z > l) ? l : z);
					if ((f = 1 << (j = k - w)) > a + 1)
					{
						f -= a + 1;
						xp = k;
						if (j < z)
						{
							while (++j < z && (f <<= 1) > this.c[++xp])
							{
								f -= this.c[xp];
							}
						}
					}
					z = 1 << j;
					if (hn[0] + z > 1440)
					{
						return -3;
					}
					q = (this.u[h] = hn[0]);
					hn[0] += z;
					if (h != 0)
					{
						this.x[h] = i;
						this.r[0] = (sbyte)j;
						this.r[1] = (sbyte)l;
						j = SharedUtils.URShift(i, w - l);
						this.r[2] = q - this.u[h - 1] - j;
						Array.Copy(this.r, 0, hp, (this.u[h - 1] + j) * 3, 3);
					}
					else
					{
						t[0] = q;
					}
				}
				this.r[1] = (sbyte)(k - w);
				if (p >= n)
				{
					this.r[0] = 192;
				}
				else if (v[p] < s)
				{
					this.r[0] = (sbyte)((v[p] >= 256) ? 96 : 0);
					this.r[2] = v[p++];
				}
				else
				{
					this.r[0] = (sbyte)(e[v[p] - s] + 16 + 64);
					this.r[2] = d[v[p++] - s];
				}
				f = 1 << k - w;
				for (j = SharedUtils.URShift(i, w); j < z; j += f)
				{
					Array.Copy(this.r, 0, hp, (q + j) * 3, 3);
				}
				j = 1 << k - 1;
				while ((i & j) != 0)
				{
					i ^= j;
					j = SharedUtils.URShift(j, 1);
				}
				i ^= j;
				int mask = (1 << w) - 1;
				while ((i & mask) != this.x[h])
				{
					h--;
					w -= l;
					mask = (1 << w) - 1;
				}
			}
		}
		if (y == 0 || g == 1)
		{
			return 0;
		}
		return -5;
	}

	internal int inflate_trees_bits(int[] c, int[] bb, int[] tb, int[] hp, ZlibCodec z)
	{
		this.initWorkArea(19);
		this.hn[0] = 0;
		int result = this.huft_build(c, 0, 19, 19, null, null, tb, bb, hp, this.hn, this.v);
		if (result == -3)
		{
			z.Message = "oversubscribed dynamic bit lengths tree";
		}
		else if (result == -5 || bb[0] == 0)
		{
			z.Message = "incomplete dynamic bit lengths tree";
			result = -3;
		}
		return result;
	}

	internal int inflate_trees_dynamic(int nl, int nd, int[] c, int[] bl, int[] bd, int[] tl, int[] td, int[] hp, ZlibCodec z)
	{
		this.initWorkArea(288);
		this.hn[0] = 0;
		int result = this.huft_build(c, 0, nl, 257, InfTree.cplens, InfTree.cplext, tl, bl, hp, this.hn, this.v);
		if (result != 0 || bl[0] == 0)
		{
			switch (result)
			{
			case -3:
				z.Message = "oversubscribed literal/length tree";
				break;
			default:
				z.Message = "incomplete literal/length tree";
				result = -3;
				break;
			case -4:
				break;
			}
			return result;
		}
		this.initWorkArea(288);
		result = this.huft_build(c, nl, nd, 0, InfTree.cpdist, InfTree.cpdext, td, bd, hp, this.hn, this.v);
		if (result != 0 || (bd[0] == 0 && nl > 257))
		{
			switch (result)
			{
			case -3:
				z.Message = "oversubscribed distance tree";
				break;
			case -5:
				z.Message = "incomplete distance tree";
				result = -3;
				break;
			default:
				z.Message = "empty distance tree with lengths";
				result = -3;
				break;
			case -4:
				break;
			}
			return result;
		}
		return 0;
	}

	internal static int inflate_trees_fixed(int[] bl, int[] bd, int[][] tl, int[][] td, ZlibCodec z)
	{
		bl[0] = 9;
		bd[0] = 5;
		tl[0] = InfTree.fixed_tl;
		td[0] = InfTree.fixed_td;
		return 0;
	}

	private void initWorkArea(int vsize)
	{
		if (this.hn == null)
		{
			this.hn = new int[1];
			this.v = new int[vsize];
			this.c = new int[16];
			this.r = new int[3];
			this.u = new int[15];
			this.x = new int[16];
			return;
		}
		if (this.v.Length < vsize)
		{
			this.v = new int[vsize];
		}
		Array.Clear(this.v, 0, vsize);
		Array.Clear(this.c, 0, 16);
		this.r[0] = 0;
		this.r[1] = 0;
		this.r[2] = 0;
		Array.Clear(this.u, 0, 15);
		Array.Clear(this.x, 0, 16);
	}
}
