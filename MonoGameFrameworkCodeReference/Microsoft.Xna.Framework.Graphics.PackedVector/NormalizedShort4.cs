using System;

namespace Microsoft.Xna.Framework.Graphics.PackedVector;

public struct NormalizedShort4 : IPackedVector<ulong>, IPackedVector, IEquatable<NormalizedShort4>
{
	private ulong short4Packed;

	[CLSCompliant(false)]
	public ulong PackedValue
	{
		get
		{
			return this.short4Packed;
		}
		set
		{
			this.short4Packed = value;
		}
	}

	public NormalizedShort4(Vector4 vector)
	{
		this.short4Packed = NormalizedShort4.PackInFour(vector.X, vector.Y, vector.Z, vector.W);
	}

	public NormalizedShort4(float x, float y, float z, float w)
	{
		this.short4Packed = NormalizedShort4.PackInFour(x, y, z, w);
	}

	public static bool operator !=(NormalizedShort4 a, NormalizedShort4 b)
	{
		return !a.Equals(b);
	}

	public static bool operator ==(NormalizedShort4 a, NormalizedShort4 b)
	{
		return a.Equals(b);
	}

	public override bool Equals(object obj)
	{
		if (obj is NormalizedShort4)
		{
			return this.Equals((NormalizedShort4)obj);
		}
		return false;
	}

	public bool Equals(NormalizedShort4 other)
	{
		return this.short4Packed.Equals(other.short4Packed);
	}

	public override int GetHashCode()
	{
		return this.short4Packed.GetHashCode();
	}

	public override string ToString()
	{
		return this.short4Packed.ToString("X");
	}

	private static ulong PackInFour(float vectorX, float vectorY, float vectorZ, float vectorW)
	{
		long num = (long)(int)Math.Round(MathHelper.Clamp(vectorX * 32767f, -32767f, 32767f)) & 0xFFFFL;
		ulong word3 = ((ulong)(int)Math.Round(MathHelper.Clamp(vectorY * 32767f, -32767f, 32767f)) & 0xFFFFuL) << 16;
		ulong word4 = ((ulong)(int)Math.Round(MathHelper.Clamp(vectorZ * 32767f, -32767f, 32767f)) & 0xFFFFuL) << 32;
		ulong word5 = ((ulong)(int)Math.Round(MathHelper.Clamp(vectorW * 32767f, -32767f, 32767f)) & 0xFFFFuL) << 48;
		return (ulong)num | word3 | word4 | word5;
	}

	void IPackedVector.PackFromVector4(Vector4 vector)
	{
		this.short4Packed = NormalizedShort4.PackInFour(vector.X, vector.Y, vector.Z, vector.W);
	}

	public Vector4 ToVector4()
	{
		return new Vector4
		{
			X = (float)(short)(this.short4Packed & 0xFFFF) / 32767f,
			Y = (float)(short)((this.short4Packed >> 16) & 0xFFFF) / 32767f,
			Z = (float)(short)((this.short4Packed >> 32) & 0xFFFF) / 32767f,
			W = (float)(short)((this.short4Packed >> 48) & 0xFFFF) / 32767f
		};
	}
}
