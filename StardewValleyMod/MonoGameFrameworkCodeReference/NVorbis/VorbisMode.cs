using System;
using System.IO;

namespace NVorbis;

internal class VorbisMode
{
	private const float M_PI = (float)Math.PI;

	private const float M_PI2 = (float)Math.PI / 2f;

	private VorbisStreamDecoder _vorbis;

	private float[][] _windows;

	internal bool BlockFlag;

	internal int WindowType;

	internal int TransformType;

	internal VorbisMapping Mapping;

	internal int BlockSize;

	internal static VorbisMode Init(VorbisStreamDecoder vorbis, DataPacket packet)
	{
		VorbisMode mode = new VorbisMode(vorbis);
		mode.BlockFlag = packet.ReadBit();
		mode.WindowType = (int)packet.ReadBits(16);
		mode.TransformType = (int)packet.ReadBits(16);
		int mapping = (int)packet.ReadBits(8);
		if (mode.WindowType != 0 || mode.TransformType != 0 || mapping >= vorbis.Maps.Length)
		{
			throw new InvalidDataException();
		}
		mode.Mapping = vorbis.Maps[mapping];
		mode.BlockSize = (mode.BlockFlag ? vorbis.Block1Size : vorbis.Block0Size);
		if (mode.BlockFlag)
		{
			mode._windows = new float[4][];
			mode._windows[0] = new float[vorbis.Block1Size];
			mode._windows[1] = new float[vorbis.Block1Size];
			mode._windows[2] = new float[vorbis.Block1Size];
			mode._windows[3] = new float[vorbis.Block1Size];
		}
		else
		{
			mode._windows = new float[1][];
			mode._windows[0] = new float[vorbis.Block0Size];
		}
		mode.CalcWindows();
		return mode;
	}

	private VorbisMode(VorbisStreamDecoder vorbis)
	{
		this._vorbis = vorbis;
	}

	private void CalcWindows()
	{
		for (int idx = 0; idx < this._windows.Length; idx++)
		{
			float[] array = this._windows[idx];
			int left = (((idx & 1) == 0) ? this._vorbis.Block0Size : this._vorbis.Block1Size) / 2;
			int blockSize = this.BlockSize;
			int right = (((idx & 2) == 0) ? this._vorbis.Block0Size : this._vorbis.Block1Size) / 2;
			int leftbegin = blockSize / 4 - left / 2;
			int rightbegin = blockSize - blockSize / 4 - right / 2;
			for (int i = 0; i < left; i++)
			{
				float x = (float)Math.Sin(((double)i + 0.5) / (double)left * 1.5707963705062866);
				x *= x;
				array[leftbegin + i] = (float)Math.Sin(x * ((float)Math.PI / 2f));
			}
			for (int j = leftbegin + left; j < rightbegin; j++)
			{
				array[j] = 1f;
			}
			for (int k = 0; k < right; k++)
			{
				float x2 = (float)Math.Sin(((double)(right - k) - 0.5) / (double)right * 1.5707963705062866);
				x2 *= x2;
				array[rightbegin + k] = (float)Math.Sin(x2 * ((float)Math.PI / 2f));
			}
		}
	}

	internal float[] GetWindow(bool prev, bool next)
	{
		if (this.BlockFlag)
		{
			if (next)
			{
				if (prev)
				{
					return this._windows[3];
				}
				return this._windows[2];
			}
			if (prev)
			{
				return this._windows[1];
			}
		}
		return this._windows[0];
	}
}
