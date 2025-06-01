using System;

namespace Microsoft.Xna.Framework.Graphics.PackedVector;

/// <summary>
/// Packed vector type containing unsigned normalized values ranging from 0 to 1.
/// The x , y and z components use 5 bits, and the w component uses 1 bit.
/// </summary>
public struct Bgra5551 : IPackedVector<ushort>, IPackedVector, IEquatable<Bgra5551>
{
	private ushort packedValue;

	/// <summary>
	/// Gets and sets the packed value.
	/// </summary>
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

	/// <summary>
	/// Creates a new instance of Bgra5551.
	/// </summary>
	/// <param name="x">The x component</param>
	/// <param name="y">The y component</param>
	/// <param name="z">The z component</param>
	/// <param name="w">The w component</param>
	public Bgra5551(float x, float y, float z, float w)
	{
		this.packedValue = Bgra5551.Pack(x, y, z, w);
	}

	/// <summary>
	/// Creates a new instance of Bgra5551.
	/// </summary>
	/// <param name="vector">
	/// Vector containing the components for the packed vector.
	/// </param>
	public Bgra5551(Vector4 vector)
	{
		this.packedValue = Bgra5551.Pack(vector.X, vector.Y, vector.Z, vector.W);
	}

	/// <summary>
	/// Gets the packed vector in Vector4 format.
	/// </summary>
	/// <returns>The packed vector in Vector4 format</returns>
	public Vector4 ToVector4()
	{
		return new Vector4((float)((this.packedValue >> 10) & 0x1F) / 31f, (float)((this.packedValue >> 5) & 0x1F) / 31f, (float)(this.packedValue & 0x1F) / 31f, (this.packedValue >> 15) & 1);
	}

	/// <summary>
	/// Sets the packed vector from a Vector4.
	/// </summary>
	/// <param name="vector">Vector containing the components.</param>
	void IPackedVector.PackFromVector4(Vector4 vector)
	{
		this.packedValue = Bgra5551.Pack(vector.X, vector.Y, vector.Z, vector.W);
	}

	/// <summary>
	/// Compares an object with the packed vector.
	/// </summary>
	/// <param name="obj">The object to compare.</param>
	/// <returns>True if the object is equal to the packed vector.</returns>
	public override bool Equals(object obj)
	{
		if (obj is Bgra5551)
		{
			return this.Equals((Bgra5551)obj);
		}
		return false;
	}

	/// <summary>
	/// Compares another Bgra5551 packed vector with the packed vector.
	/// </summary>
	/// <param name="other">The Bgra5551 packed vector to compare.</param>
	/// <returns>True if the packed vectors are equal.</returns>
	public bool Equals(Bgra5551 other)
	{
		return this.packedValue == other.packedValue;
	}

	/// <summary>
	/// Gets a string representation of the packed vector.
	/// </summary>
	/// <returns>A string representation of the packed vector.</returns>
	public override string ToString()
	{
		return this.ToVector4().ToString();
	}

	/// <summary>
	/// Gets a hash code of the packed vector.
	/// </summary>
	/// <returns>The hash code for the packed vector.</returns>
	public override int GetHashCode()
	{
		return this.packedValue.GetHashCode();
	}

	public static bool operator ==(Bgra5551 lhs, Bgra5551 rhs)
	{
		return lhs.packedValue == rhs.packedValue;
	}

	public static bool operator !=(Bgra5551 lhs, Bgra5551 rhs)
	{
		return lhs.packedValue != rhs.packedValue;
	}

	private static ushort Pack(float x, float y, float z, float w)
	{
		return (ushort)((((int)Math.Round(MathHelper.Clamp(x, 0f, 1f) * 31f) & 0x1F) << 10) | (((int)Math.Round(MathHelper.Clamp(y, 0f, 1f) * 31f) & 0x1F) << 5) | ((int)Math.Round(MathHelper.Clamp(z, 0f, 1f) * 31f) & 0x1F) | (((int)Math.Round(MathHelper.Clamp(w, 0f, 1f)) & 1) << 15));
	}
}
