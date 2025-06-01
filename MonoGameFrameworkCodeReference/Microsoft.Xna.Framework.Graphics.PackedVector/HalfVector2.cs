using System;

namespace Microsoft.Xna.Framework.Graphics.PackedVector;

public struct HalfVector2 : IPackedVector<uint>, IPackedVector, IEquatable<HalfVector2>
{
	private uint packedValue;

	[CLSCompliant(false)]
	public uint PackedValue
	{
		get
		{
			return this.packedValue;
		}
		set
		{
			this.packedValue = value;
		}
	}

	public HalfVector2(float x, float y)
	{
		this.packedValue = HalfVector2.PackHelper(x, y);
	}

	public HalfVector2(Vector2 vector)
	{
		this.packedValue = HalfVector2.PackHelper(vector.X, vector.Y);
	}

	void IPackedVector.PackFromVector4(Vector4 vector)
	{
		this.packedValue = HalfVector2.PackHelper(vector.X, vector.Y);
	}

	private static uint PackHelper(float vectorX, float vectorY)
	{
		ushort num = HalfTypeHelper.Convert(vectorX);
		uint num2 = (uint)(HalfTypeHelper.Convert(vectorY) << 16);
		return num | num2;
	}

	public Vector2 ToVector2()
	{
		Vector2 vector = default(Vector2);
		vector.X = HalfTypeHelper.Convert((ushort)this.packedValue);
		vector.Y = HalfTypeHelper.Convert((ushort)(this.packedValue >> 16));
		return vector;
	}

	/// <summary>
	/// Gets the packed vector in Vector4 format.
	/// </summary>
	/// <returns>The packed vector in Vector4 format</returns>
	public Vector4 ToVector4()
	{
		Vector2 vector = this.ToVector2();
		return new Vector4(vector.X, vector.Y, 0f, 1f);
	}

	public override string ToString()
	{
		return this.ToVector2().ToString();
	}

	public override int GetHashCode()
	{
		return this.packedValue.GetHashCode();
	}

	public override bool Equals(object obj)
	{
		if (obj is HalfVector2)
		{
			return this.Equals((HalfVector2)obj);
		}
		return false;
	}

	public bool Equals(HalfVector2 other)
	{
		return this.packedValue.Equals(other.packedValue);
	}

	public static bool operator ==(HalfVector2 a, HalfVector2 b)
	{
		return a.Equals(b);
	}

	public static bool operator !=(HalfVector2 a, HalfVector2 b)
	{
		return !a.Equals(b);
	}
}
