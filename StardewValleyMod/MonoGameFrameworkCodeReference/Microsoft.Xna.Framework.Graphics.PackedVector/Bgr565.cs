using System;

namespace Microsoft.Xna.Framework.Graphics.PackedVector;

/// <summary>
/// Packed vector type containing unsigned normalized values ranging from 0 to 1. The x and z components use 5 bits, and the y component uses 6 bits.
/// </summary>
public struct Bgr565 : IPackedVector<ushort>, IPackedVector, IEquatable<Bgr565>
{
	private ushort _packedValue;

	/// <summary>
	/// Gets and sets the packed value.
	/// </summary>
	[CLSCompliant(false)]
	public ushort PackedValue
	{
		get
		{
			return this._packedValue;
		}
		set
		{
			this._packedValue = value;
		}
	}

	private static ushort Pack(float x, float y, float z)
	{
		return (ushort)((((int)Math.Round(MathHelper.Clamp(x, 0f, 1f) * 31f) & 0x1F) << 11) | (((int)Math.Round(MathHelper.Clamp(y, 0f, 1f) * 63f) & 0x3F) << 5) | ((int)Math.Round(MathHelper.Clamp(z, 0f, 1f) * 31f) & 0x1F));
	}

	/// <summary>
	/// Creates a new instance of Bgr565.
	/// </summary>
	/// <param name="x">The x component</param>
	/// <param name="y">The y component</param>
	/// <param name="z">The z component</param>
	public Bgr565(float x, float y, float z)
	{
		this._packedValue = Bgr565.Pack(x, y, z);
	}

	/// <summary>
	/// Creates a new instance of Bgr565.
	/// </summary>
	/// <param name="vector">Vector containing the components for the packed vector.</param>
	public Bgr565(Vector3 vector)
	{
		this._packedValue = Bgr565.Pack(vector.X, vector.Y, vector.Z);
	}

	/// <summary>
	/// Gets the packed vector in Vector3 format.
	/// </summary>
	/// <returns>The packed vector in Vector3 format</returns>
	public Vector3 ToVector3()
	{
		return new Vector3((float)((this._packedValue >> 11) & 0x1F) * (1f / 31f), (float)((this._packedValue >> 5) & 0x3F) * (1f / 63f), (float)(this._packedValue & 0x1F) * (1f / 31f));
	}

	/// <summary>
	/// Sets the packed vector from a Vector4.
	/// </summary>
	/// <param name="vector">Vector containing the components.</param>
	void IPackedVector.PackFromVector4(Vector4 vector)
	{
		this._packedValue = (ushort)((((int)(vector.X * 31f) & 0x1F) << 11) | (((int)(vector.Y * 63f) & 0x3F) << 5) | ((int)(vector.Z * 31f) & 0x1F));
	}

	/// <summary>
	/// Gets the packed vector in Vector4 format.
	/// </summary>
	/// <returns>The packed vector in Vector4 format</returns>
	public Vector4 ToVector4()
	{
		return new Vector4(this.ToVector3(), 1f);
	}

	/// <summary>
	/// Compares an object with the packed vector.
	/// </summary>
	/// <param name="obj">The object to compare.</param>
	/// <returns>true if the object is equal to the packed vector.</returns>
	public override bool Equals(object obj)
	{
		if (obj != null && obj is Bgr565)
		{
			return this == (Bgr565)obj;
		}
		return false;
	}

	/// <summary>
	/// Compares another Bgr565 packed vector with the packed vector.
	/// </summary>
	/// <param name="other">The Bgr565 packed vector to compare.</param>
	/// <returns>true if the packed vectors are equal.</returns>
	public bool Equals(Bgr565 other)
	{
		return this._packedValue == other._packedValue;
	}

	/// <summary>
	/// Gets a string representation of the packed vector.
	/// </summary>
	/// <returns>A string representation of the packed vector.</returns>
	public override string ToString()
	{
		return this.ToVector3().ToString();
	}

	/// <summary>
	/// Gets a hash code of the packed vector.
	/// </summary>
	/// <returns>The hash code for the packed vector.</returns>
	public override int GetHashCode()
	{
		return this._packedValue.GetHashCode();
	}

	public static bool operator ==(Bgr565 lhs, Bgr565 rhs)
	{
		return lhs._packedValue == rhs._packedValue;
	}

	public static bool operator !=(Bgr565 lhs, Bgr565 rhs)
	{
		return lhs._packedValue != rhs._packedValue;
	}
}
