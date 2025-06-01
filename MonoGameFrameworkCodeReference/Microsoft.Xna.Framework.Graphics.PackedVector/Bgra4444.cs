using System;

namespace Microsoft.Xna.Framework.Graphics.PackedVector;

/// <summary>
/// Packed vector type containing unsigned normalized values, ranging from 0 to 1, using 4 bits each for x, y, z, and w.
/// </summary>
public struct Bgra4444 : IPackedVector<ushort>, IPackedVector, IEquatable<Bgra4444>
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

	private static ushort Pack(float x, float y, float z, float w)
	{
		return (ushort)((((int)Math.Round(MathHelper.Clamp(w, 0f, 1f) * 15f) & 0xF) << 12) | (((int)Math.Round(MathHelper.Clamp(x, 0f, 1f) * 15f) & 0xF) << 8) | (((int)Math.Round(MathHelper.Clamp(y, 0f, 1f) * 15f) & 0xF) << 4) | ((int)Math.Round(MathHelper.Clamp(z, 0f, 1f) * 15f) & 0xF));
	}

	/// <summary>
	/// Creates a new instance of Bgra4444.
	/// </summary>
	/// <param name="x">The x component</param>
	/// <param name="y">The y component</param>
	/// <param name="z">The z component</param>
	/// <param name="w">The w component</param>
	public Bgra4444(float x, float y, float z, float w)
	{
		this._packedValue = Bgra4444.Pack(x, y, z, w);
	}

	/// <summary>
	/// Creates a new instance of Bgra4444.
	/// </summary>
	/// <param name="vector">Vector containing the components for the packed vector.</param>
	public Bgra4444(Vector4 vector)
	{
		this._packedValue = Bgra4444.Pack(vector.X, vector.Y, vector.Z, vector.W);
	}

	/// <summary>
	/// Gets the packed vector in Vector4 format.
	/// </summary>
	/// <returns>The packed vector in Vector4 format</returns>
	public Vector4 ToVector4()
	{
		return new Vector4((float)((this._packedValue >> 8) & 0xF) * (1f / 15f), (float)((this._packedValue >> 4) & 0xF) * (1f / 15f), (float)(this._packedValue & 0xF) * (1f / 15f), (float)((this._packedValue >> 12) & 0xF) * (1f / 15f));
	}

	/// <summary>
	/// Sets the packed vector from a Vector4.
	/// </summary>
	/// <param name="vector">Vector containing the components.</param>
	void IPackedVector.PackFromVector4(Vector4 vector)
	{
		this._packedValue = Bgra4444.Pack(vector.X, vector.Y, vector.Z, vector.W);
	}

	/// <summary>
	/// Compares an object with the packed vector.
	/// </summary>
	/// <param name="obj">The object to compare.</param>
	/// <returns>true if the object is equal to the packed vector.</returns>
	public override bool Equals(object obj)
	{
		if (obj != null && obj is Bgra4444)
		{
			return this == (Bgra4444)obj;
		}
		return false;
	}

	/// <summary>
	/// Compares another Bgra4444 packed vector with the packed vector.
	/// </summary>
	/// <param name="other">The Bgra4444 packed vector to compare.</param>
	/// <returns>true if the packed vectors are equal.</returns>
	public bool Equals(Bgra4444 other)
	{
		return this._packedValue == other._packedValue;
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
		return this._packedValue.GetHashCode();
	}

	public static bool operator ==(Bgra4444 lhs, Bgra4444 rhs)
	{
		return lhs._packedValue == rhs._packedValue;
	}

	public static bool operator !=(Bgra4444 lhs, Bgra4444 rhs)
	{
		return lhs._packedValue != rhs._packedValue;
	}
}
