using System;
using System.Collections.Generic;
using System.Threading;

namespace NVorbis;

internal class Mdct
{
	private const float M_PI = (float)Math.PI;

	private static Dictionary<int, Mdct> _setupCache = new Dictionary<int, Mdct>(2);

	private int _n;

	private int _n2;

	private int _n4;

	private int _n8;

	private int _ld;

	private float[] _A;

	private float[] _B;

	private float[] _C;

	private ushort[] _bitrev;

	private Dictionary<int, float[]> _threadLocalBuffers = new Dictionary<int, float[]>(1);

	public static void Reverse(float[] samples, int sampleCount)
	{
		Mdct.GetSetup(sampleCount).CalcReverse(samples);
	}

	private static Mdct GetSetup(int n)
	{
		lock (Mdct._setupCache)
		{
			if (!Mdct._setupCache.ContainsKey(n))
			{
				Mdct._setupCache[n] = new Mdct(n);
			}
			return Mdct._setupCache[n];
		}
	}

	private Mdct(int n)
	{
		this._n = n;
		this._n2 = n >> 1;
		this._n4 = this._n2 >> 1;
		this._n8 = this._n4 >> 1;
		this._ld = Utils.ilog(n) - 1;
		this._A = new float[this._n2];
		this._B = new float[this._n2];
		this._C = new float[this._n4];
		int k2;
		int k = (k2 = 0);
		while (k < this._n4)
		{
			this._A[k2] = (float)Math.Cos((float)(4 * k) * (float)Math.PI / (float)n);
			this._A[k2 + 1] = (float)(0.0 - Math.Sin((float)(4 * k) * (float)Math.PI / (float)n));
			this._B[k2] = (float)Math.Cos((float)(k2 + 1) * (float)Math.PI / (float)n / 2f) * 0.5f;
			this._B[k2 + 1] = (float)Math.Sin((float)(k2 + 1) * (float)Math.PI / (float)n / 2f) * 0.5f;
			k++;
			k2 += 2;
		}
		k = (k2 = 0);
		while (k < this._n8)
		{
			this._C[k2] = (float)Math.Cos((float)(2 * (k2 + 1)) * (float)Math.PI / (float)n);
			this._C[k2 + 1] = (float)(0.0 - Math.Sin((float)(2 * (k2 + 1)) * (float)Math.PI / (float)n));
			k++;
			k2 += 2;
		}
		this._bitrev = new ushort[this._n8];
		for (int i = 0; i < this._n8; i++)
		{
			this._bitrev[i] = (ushort)(Utils.BitReverse((uint)i, this._ld - 3) << 2);
		}
	}

	private float[] GetBuffer()
	{
		lock (this._threadLocalBuffers)
		{
			if (!this._threadLocalBuffers.TryGetValue(Thread.CurrentThread.ManagedThreadId, out var buf))
			{
				buf = (this._threadLocalBuffers[Thread.CurrentThread.ManagedThreadId] = new float[this._n2]);
			}
			return buf;
		}
	}

	private void CalcReverse(float[] buffer)
	{
		float[] buf2 = this.GetBuffer();
		int d = this._n2 - 2;
		int AA = 0;
		int e = 0;
		for (int e_stop = this._n2; e != e_stop; e += 4)
		{
			buf2[d + 1] = buffer[e] * this._A[AA] - buffer[e + 2] * this._A[AA + 1];
			buf2[d] = buffer[e] * this._A[AA + 1] + buffer[e + 2] * this._A[AA];
			d -= 2;
			AA += 2;
		}
		e = this._n2 - 3;
		while (d >= 0)
		{
			buf2[d + 1] = (0f - buffer[e + 2]) * this._A[AA] - (0f - buffer[e]) * this._A[AA + 1];
			buf2[d] = (0f - buffer[e + 2]) * this._A[AA + 1] + (0f - buffer[e]) * this._A[AA];
			d -= 2;
			AA += 2;
			e -= 4;
		}
		float[] v = buf2;
		int AA2 = this._n2 - 8;
		int e2 = this._n4;
		int e3 = 0;
		int d2 = this._n4;
		int d3 = 0;
		while (AA2 >= 0)
		{
			float v41_21 = v[e2 + 1] - v[e3 + 1];
			float v40_20 = v[e2] - v[e3];
			buffer[d2 + 1] = v[e2 + 1] + v[e3 + 1];
			buffer[d2] = v[e2] + v[e3];
			buffer[d3 + 1] = v41_21 * this._A[AA2 + 4] - v40_20 * this._A[AA2 + 5];
			buffer[d3] = v40_20 * this._A[AA2 + 4] + v41_21 * this._A[AA2 + 5];
			v41_21 = v[e2 + 3] - v[e3 + 3];
			v40_20 = v[e2 + 2] - v[e3 + 2];
			buffer[d2 + 3] = v[e2 + 3] + v[e3 + 3];
			buffer[d2 + 2] = v[e2 + 2] + v[e3 + 2];
			buffer[d3 + 3] = v41_21 * this._A[AA2] - v40_20 * this._A[AA2 + 1];
			buffer[d3 + 2] = v40_20 * this._A[AA2] + v41_21 * this._A[AA2 + 1];
			AA2 -= 8;
			d2 += 4;
			d3 += 4;
			e2 += 4;
			e3 += 4;
		}
		int n = this._n >> 4;
		int num = this._n2 - 1;
		_ = this._n4;
		this.step3_iter0_loop(n, buffer, num - 0, -this._n8);
		this.step3_iter0_loop(this._n >> 4, buffer, this._n2 - 1 - this._n4, -this._n8);
		int lim = this._n >> 5;
		int num2 = this._n2 - 1;
		_ = this._n8;
		this.step3_inner_r_loop(lim, buffer, num2 - 0, -(this._n >> 4), 16);
		this.step3_inner_r_loop(this._n >> 5, buffer, this._n2 - 1 - this._n8, -(this._n >> 4), 16);
		this.step3_inner_r_loop(this._n >> 5, buffer, this._n2 - 1 - this._n8 * 2, -(this._n >> 4), 16);
		this.step3_inner_r_loop(this._n >> 5, buffer, this._n2 - 1 - this._n8 * 3, -(this._n >> 4), 16);
		int l;
		for (l = 2; l < this._ld - 3 >> 1; l++)
		{
			int k0 = this._n >> l + 2;
			int k0_2 = k0 >> 1;
			int lim2 = 1 << l + 1;
			for (int i = 0; i < lim2; i++)
			{
				this.step3_inner_r_loop(this._n >> l + 4, buffer, this._n2 - 1 - k0 * i, -k0_2, 1 << l + 3);
			}
		}
		for (; l < this._ld - 6; l++)
		{
			int k1 = this._n >> l + 2;
			int k2 = 1 << l + 3;
			int k0_3 = k1 >> 1;
			int num3 = this._n >> l + 6;
			int lim3 = 1 << l + 1;
			int i_off = this._n2 - 1;
			int A0 = 0;
			for (int r = num3; r > 0; r--)
			{
				this.step3_inner_s_loop(lim3, buffer, i_off, -k0_3, A0, k2, k1);
				A0 += k2 * 4;
				i_off -= 8;
			}
		}
		this.step3_inner_s_loop_ld654(this._n >> 5, buffer, this._n2 - 1, this._n);
		int bit = 0;
		int d4 = this._n4 - 4;
		int d5 = this._n2 - 4;
		while (d4 >= 0)
		{
			int k4 = this._bitrev[bit];
			v[d5 + 3] = buffer[k4];
			v[d5 + 2] = buffer[k4 + 1];
			v[d4 + 3] = buffer[k4 + 2];
			v[d4 + 2] = buffer[k4 + 3];
			k4 = this._bitrev[bit + 1];
			v[d5 + 1] = buffer[k4];
			v[d5] = buffer[k4 + 1];
			v[d4 + 1] = buffer[k4 + 2];
			v[d4] = buffer[k4 + 3];
			d4 -= 4;
			d5 -= 4;
			bit += 2;
		}
		int c = 0;
		int d6 = 0;
		int e4 = this._n2 - 4;
		while (d6 < e4)
		{
			float a02 = v[d6] - v[e4 + 2];
			float a11 = v[d6 + 1] + v[e4 + 3];
			float b0 = this._C[c + 1] * a02 + this._C[c] * a11;
			float b1 = this._C[c + 1] * a11 - this._C[c] * a02;
			float b2 = v[d6] + v[e4 + 2];
			float b3 = v[d6 + 1] - v[e4 + 3];
			v[d6] = b2 + b0;
			v[d6 + 1] = b3 + b1;
			v[e4 + 2] = b2 - b0;
			v[e4 + 3] = b1 - b3;
			a02 = v[d6 + 2] - v[e4];
			a11 = v[d6 + 3] + v[e4 + 1];
			b0 = this._C[c + 3] * a02 + this._C[c + 2] * a11;
			b1 = this._C[c + 3] * a11 - this._C[c + 2] * a02;
			b2 = v[d6 + 2] + v[e4];
			b3 = v[d6 + 3] - v[e4 + 1];
			v[d6 + 2] = b2 + b0;
			v[d6 + 3] = b3 + b1;
			v[e4] = b2 - b0;
			v[e4 + 1] = b1 - b3;
			c += 4;
			d6 += 4;
			e4 -= 4;
		}
		int b4 = this._n2 - 8;
		int e5 = this._n2 - 8;
		int d7 = 0;
		int d8 = this._n2 - 4;
		int d9 = this._n2;
		int d10 = this._n - 4;
		while (e5 >= 0)
		{
			float p3 = buf2[e5 + 6] * this._B[b4 + 7] - buf2[e5 + 7] * this._B[b4 + 6];
			float p4 = (0f - buf2[e5 + 6]) * this._B[b4 + 6] - buf2[e5 + 7] * this._B[b4 + 7];
			buffer[d7] = p3;
			buffer[d8 + 3] = 0f - p3;
			buffer[d9] = p4;
			buffer[d10 + 3] = p4;
			float p5 = buf2[e5 + 4] * this._B[b4 + 5] - buf2[e5 + 5] * this._B[b4 + 4];
			float p6 = (0f - buf2[e5 + 4]) * this._B[b4 + 4] - buf2[e5 + 5] * this._B[b4 + 5];
			buffer[d7 + 1] = p5;
			buffer[d8 + 2] = 0f - p5;
			buffer[d9 + 1] = p6;
			buffer[d10 + 2] = p6;
			p3 = buf2[e5 + 2] * this._B[b4 + 3] - buf2[e5 + 3] * this._B[b4 + 2];
			p4 = (0f - buf2[e5 + 2]) * this._B[b4 + 2] - buf2[e5 + 3] * this._B[b4 + 3];
			buffer[d7 + 2] = p3;
			buffer[d8 + 1] = 0f - p3;
			buffer[d9 + 2] = p4;
			buffer[d10 + 1] = p4;
			p5 = buf2[e5] * this._B[b4 + 1] - buf2[e5 + 1] * this._B[b4];
			p6 = (0f - buf2[e5]) * this._B[b4] - buf2[e5 + 1] * this._B[b4 + 1];
			buffer[d7 + 3] = p5;
			buffer[d8] = 0f - p5;
			buffer[d9 + 3] = p6;
			buffer[d10] = p6;
			b4 -= 8;
			e5 -= 8;
			d7 += 4;
			d9 += 4;
			d8 -= 4;
			d10 -= 4;
		}
	}

	private void step3_iter0_loop(int n, float[] e, int i_off, int k_off)
	{
		int ee0 = i_off;
		int ee2 = ee0 + k_off;
		int a = 0;
		for (int i = n >> 2; i > 0; i--)
		{
			float k00_20 = e[ee0] - e[ee2];
			float k01_21 = e[ee0 - 1] - e[ee2 - 1];
			e[ee0] += e[ee2];
			e[ee0 - 1] += e[ee2 - 1];
			e[ee2] = k00_20 * this._A[a] - k01_21 * this._A[a + 1];
			e[ee2 - 1] = k01_21 * this._A[a] + k00_20 * this._A[a + 1];
			a += 8;
			k00_20 = e[ee0 - 2] - e[ee2 - 2];
			k01_21 = e[ee0 - 3] - e[ee2 - 3];
			e[ee0 - 2] += e[ee2 - 2];
			e[ee0 - 3] += e[ee2 - 3];
			e[ee2 - 2] = k00_20 * this._A[a] - k01_21 * this._A[a + 1];
			e[ee2 - 3] = k01_21 * this._A[a] + k00_20 * this._A[a + 1];
			a += 8;
			k00_20 = e[ee0 - 4] - e[ee2 - 4];
			k01_21 = e[ee0 - 5] - e[ee2 - 5];
			e[ee0 - 4] += e[ee2 - 4];
			e[ee0 - 5] += e[ee2 - 5];
			e[ee2 - 4] = k00_20 * this._A[a] - k01_21 * this._A[a + 1];
			e[ee2 - 5] = k01_21 * this._A[a] + k00_20 * this._A[a + 1];
			a += 8;
			k00_20 = e[ee0 - 6] - e[ee2 - 6];
			k01_21 = e[ee0 - 7] - e[ee2 - 7];
			e[ee0 - 6] += e[ee2 - 6];
			e[ee0 - 7] += e[ee2 - 7];
			e[ee2 - 6] = k00_20 * this._A[a] - k01_21 * this._A[a + 1];
			e[ee2 - 7] = k01_21 * this._A[a] + k00_20 * this._A[a + 1];
			a += 8;
			ee0 -= 8;
			ee2 -= 8;
		}
	}

	private void step3_inner_r_loop(int lim, float[] e, int d0, int k_off, int k1)
	{
		int e2 = d0;
		int e3 = e2 + k_off;
		int a = 0;
		for (int i = lim >> 2; i > 0; i--)
		{
			float k00_20 = e[e2] - e[e3];
			float k01_21 = e[e2 - 1] - e[e3 - 1];
			e[e2] += e[e3];
			e[e2 - 1] += e[e3 - 1];
			e[e3] = k00_20 * this._A[a] - k01_21 * this._A[a + 1];
			e[e3 - 1] = k01_21 * this._A[a] + k00_20 * this._A[a + 1];
			a += k1;
			k00_20 = e[e2 - 2] - e[e3 - 2];
			k01_21 = e[e2 - 3] - e[e3 - 3];
			e[e2 - 2] += e[e3 - 2];
			e[e2 - 3] += e[e3 - 3];
			e[e3 - 2] = k00_20 * this._A[a] - k01_21 * this._A[a + 1];
			e[e3 - 3] = k01_21 * this._A[a] + k00_20 * this._A[a + 1];
			a += k1;
			k00_20 = e[e2 - 4] - e[e3 - 4];
			k01_21 = e[e2 - 5] - e[e3 - 5];
			e[e2 - 4] += e[e3 - 4];
			e[e2 - 5] += e[e3 - 5];
			e[e3 - 4] = k00_20 * this._A[a] - k01_21 * this._A[a + 1];
			e[e3 - 5] = k01_21 * this._A[a] + k00_20 * this._A[a + 1];
			a += k1;
			k00_20 = e[e2 - 6] - e[e3 - 6];
			k01_21 = e[e2 - 7] - e[e3 - 7];
			e[e2 - 6] += e[e3 - 6];
			e[e2 - 7] += e[e3 - 7];
			e[e3 - 6] = k00_20 * this._A[a] - k01_21 * this._A[a + 1];
			e[e3 - 7] = k01_21 * this._A[a] + k00_20 * this._A[a + 1];
			a += k1;
			e2 -= 8;
			e3 -= 8;
		}
	}

	private void step3_inner_s_loop(int n, float[] e, int i_off, int k_off, int a, int a_off, int k0)
	{
		float A0 = this._A[a];
		float A1 = this._A[a + 1];
		float A2 = this._A[a + a_off];
		float A3 = this._A[a + a_off + 1];
		float A4 = this._A[a + a_off * 2];
		float A5 = this._A[a + a_off * 2 + 1];
		float A6 = this._A[a + a_off * 3];
		float A7 = this._A[a + a_off * 3 + 1];
		int ee0 = i_off;
		int ee2 = ee0 + k_off;
		for (int i = n; i > 0; i--)
		{
			float k1 = e[ee0] - e[ee2];
			float k11 = e[ee0 - 1] - e[ee2 - 1];
			e[ee0] += e[ee2];
			e[ee0 - 1] += e[ee2 - 1];
			e[ee2] = k1 * A0 - k11 * A1;
			e[ee2 - 1] = k11 * A0 + k1 * A1;
			k1 = e[ee0 - 2] - e[ee2 - 2];
			k11 = e[ee0 - 3] - e[ee2 - 3];
			e[ee0 - 2] += e[ee2 - 2];
			e[ee0 - 3] += e[ee2 - 3];
			e[ee2 - 2] = k1 * A2 - k11 * A3;
			e[ee2 - 3] = k11 * A2 + k1 * A3;
			k1 = e[ee0 - 4] - e[ee2 - 4];
			k11 = e[ee0 - 5] - e[ee2 - 5];
			e[ee0 - 4] += e[ee2 - 4];
			e[ee0 - 5] += e[ee2 - 5];
			e[ee2 - 4] = k1 * A4 - k11 * A5;
			e[ee2 - 5] = k11 * A4 + k1 * A5;
			k1 = e[ee0 - 6] - e[ee2 - 6];
			k11 = e[ee0 - 7] - e[ee2 - 7];
			e[ee0 - 6] += e[ee2 - 6];
			e[ee0 - 7] += e[ee2 - 7];
			e[ee2 - 6] = k1 * A6 - k11 * A7;
			e[ee2 - 7] = k11 * A6 + k1 * A7;
			ee0 -= k0;
			ee2 -= k0;
		}
	}

	private void step3_inner_s_loop_ld654(int n, float[] e, int i_off, int base_n)
	{
		int a_off = base_n >> 3;
		float A2 = this._A[a_off];
		int z = i_off;
		int @base = z - 16 * n;
		while (z > @base)
		{
			float k00 = e[z] - e[z - 8];
			float k11 = e[z - 1] - e[z - 9];
			e[z] += e[z - 8];
			e[z - 1] += e[z - 9];
			e[z - 8] = k00;
			e[z - 9] = k11;
			k00 = e[z - 2] - e[z - 10];
			k11 = e[z - 3] - e[z - 11];
			e[z - 2] += e[z - 10];
			e[z - 3] += e[z - 11];
			e[z - 10] = (k00 + k11) * A2;
			e[z - 11] = (k11 - k00) * A2;
			k00 = e[z - 12] - e[z - 4];
			k11 = e[z - 5] - e[z - 13];
			e[z - 4] += e[z - 12];
			e[z - 5] += e[z - 13];
			e[z - 12] = k11;
			e[z - 13] = k00;
			k00 = e[z - 14] - e[z - 6];
			k11 = e[z - 7] - e[z - 15];
			e[z - 6] += e[z - 14];
			e[z - 7] += e[z - 15];
			e[z - 14] = (k00 + k11) * A2;
			e[z - 15] = (k00 - k11) * A2;
			this.iter_54(e, z);
			this.iter_54(e, z - 8);
			z -= 16;
		}
	}

	private void iter_54(float[] e, int z)
	{
		float k00 = e[z] - e[z - 4];
		float y0 = e[z] + e[z - 4];
		float y2 = e[z - 2] + e[z - 6];
		float k22 = e[z - 2] - e[z - 6];
		e[z] = y0 + y2;
		e[z - 2] = y0 - y2;
		float k33 = e[z - 3] - e[z - 7];
		e[z - 4] = k00 + k33;
		e[z - 6] = k00 - k33;
		float k34 = e[z - 1] - e[z - 5];
		float y3 = e[z - 1] + e[z - 5];
		float y4 = e[z - 3] + e[z - 7];
		e[z - 1] = y3 + y4;
		e[z - 3] = y3 - y4;
		e[z - 5] = k34 - k22;
		e[z - 7] = k34 + k22;
	}
}
