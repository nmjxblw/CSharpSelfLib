using System;

namespace Microsoft.Xna.Framework.Graphics.PackedVector;

public struct HalfSingle : IPackedVector<ushort>, IPackedVector, IEquatable<HalfSingle>
{
	private ushort packedValue;

	[CLSCompliant(false)]
	public ushort PackedValue
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

	public HalfSingle(float single)
	{
		this.packedValue = HalfTypeHelper.Convert(single);
	}

	public float ToSingle()
	{
		return HalfTypeHelper.Convert(this.packedValue);
	}

	void IPackedVector.PackFromVector4(Vector4 vector)
	{
		this.packedValue = HalfTypeHelper.Convert(vector.X);
	}

	/// <summary>
	/// Gets the packed vector in Vector4 format.
	/// </summary>
	/// <returns>The packed vector in Vector4 format</returns>
	public Vector4 ToVector4()
	{
		return new Vector4(this.ToSingle(), 0f, 0f, 1f);
	}

	public override bool Equals(object obj)
	{
		if (obj != null && obj.GetType() == base.GetType())
		{
			return this == (HalfSingle)obj;
		}
		return false;
	}

	public bool Equals(HalfSingle other)
	{
		return this.packedValue == other.packedValue;
	}

	public override string ToString()
	{
		return this.ToSingle().ToString();
	}

	public override int GetHashCode()
	{
		return this.packedValue.GetHashCode();
	}

	public static bool operator ==(HalfSingle lhs, HalfSingle rhs)
	{
		return lhs.packedValue == rhs.packedValue;
	}

	public static bool operator !=(HalfSingle lhs, HalfSingle rhs)
	{
		return lhs.packedValue != rhs.packedValue;
	}
}
