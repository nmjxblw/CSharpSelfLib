using System;

namespace Microsoft.Xna.Framework.Graphics.PackedVector;

public struct NormalizedShort2 : IPackedVector<uint>, IPackedVector, IEquatable<NormalizedShort2>
{
	private uint short2Packed;

	[CLSCompliant(false)]
	public uint PackedValue
	{
		get
		{
			return this.short2Packed;
		}
		set
		{
			this.short2Packed = value;
		}
	}

	public NormalizedShort2(Vector2 vector)
	{
		this.short2Packed = NormalizedShort2.PackInTwo(vector.X, vector.Y);
	}

	public NormalizedShort2(float x, float y)
	{
		this.short2Packed = NormalizedShort2.PackInTwo(x, y);
	}

	public static bool operator !=(NormalizedShort2 a, NormalizedShort2 b)
	{
		return !a.Equals(b);
	}

	public static bool operator ==(NormalizedShort2 a, NormalizedShort2 b)
	{
		return a.Equals(b);
	}

	public override bool Equals(object obj)
	{
		if (obj is NormalizedShort2)
		{
			return this.Equals((NormalizedShort2)obj);
		}
		return false;
	}

	public bool Equals(NormalizedShort2 other)
	{
		return this.short2Packed.Equals(other.short2Packed);
	}

	public override int GetHashCode()
	{
		return this.short2Packed.GetHashCode();
	}

	public override string ToString()
	{
		return this.short2Packed.ToString("X");
	}

	public Vector2 ToVector2()
	{
		return new Vector2
		{
			X = (float)(short)(this.short2Packed & 0xFFFF) / 32767f,
			Y = (float)(short)(this.short2Packed >> 16) / 32767f
		};
	}

	private static uint PackInTwo(float vectorX, float vectorY)
	{
		int num = (int)MathHelper.Clamp((float)Math.Round(vectorX * 32767f), -32767f, 32767f) & 0xFFFF;
		uint word1 = (uint)(((int)MathHelper.Clamp((float)Math.Round(vectorY * 32767f), -32767f, 32767f) & 0xFFFF) << 16);
		return (uint)num | word1;
	}

	void IPackedVector.PackFromVector4(Vector4 vector)
	{
		this.short2Packed = NormalizedShort2.PackInTwo(vector.X, vector.Y);
	}

	/// <summary>
	/// Gets the packed vector in Vector4 format.
	/// </summary>
	/// <returns>The packed vector in Vector4 format</returns>
	public Vector4 ToVector4()
	{
		Vector4 v4 = new Vector4(0f, 0f, 0f, 1f);
		v4.X = (float)(short)(this.short2Packed & 0xFFFF) / 32767f;
		v4.Y = (float)(short)((this.short2Packed >> 16) & 0xFFFF) / 32767f;
		return v4;
	}
}
