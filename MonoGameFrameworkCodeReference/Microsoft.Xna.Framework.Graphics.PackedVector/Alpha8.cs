using System;

namespace Microsoft.Xna.Framework.Graphics.PackedVector;

/// <summary>
/// Packed vector type containing a single 8 bit normalized W values that is ranging from 0 to 1.
/// </summary>
public struct Alpha8 : IPackedVector<byte>, IPackedVector, IEquatable<Alpha8>
{
	private byte packedValue;

	/// <summary>
	/// Gets and sets the packed value.
	/// </summary>
	[CLSCompliant(false)]
	public byte PackedValue
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
	/// Creates a new instance of Alpha8.
	/// </summary>
	/// <param name="alpha">The alpha component</param>
	public Alpha8(float alpha)
	{
		this.packedValue = Alpha8.Pack(alpha);
	}

	/// <summary>
	/// Gets the packed vector in float format.
	/// </summary>
	/// <returns>The packed vector in Vector3 format</returns>
	public float ToAlpha()
	{
		return (float)(int)this.packedValue / 255f;
	}

	/// <summary>
	/// Sets the packed vector from a Vector4.
	/// </summary>
	/// <param name="vector">Vector containing the components.</param>
	void IPackedVector.PackFromVector4(Vector4 vector)
	{
		this.packedValue = Alpha8.Pack(vector.W);
	}

	/// <summary>
	/// Gets the packed vector in Vector4 format.
	/// </summary>
	/// <returns>The packed vector in Vector4 format</returns>
	public Vector4 ToVector4()
	{
		return new Vector4(0f, 0f, 0f, (float)(int)this.packedValue / 255f);
	}

	/// <summary>
	/// Compares an object with the packed vector.
	/// </summary>
	/// <param name="obj">The object to compare.</param>
	/// <returns>True if the object is equal to the packed vector.</returns>
	public override bool Equals(object obj)
	{
		if (obj is Alpha8)
		{
			return this.Equals((Alpha8)obj);
		}
		return false;
	}

	/// <summary>
	/// Compares another Alpha8 packed vector with the packed vector.
	/// </summary>
	/// <param name="other">The Alpha8 packed vector to compare.</param>
	/// <returns>True if the packed vectors are equal.</returns>
	public bool Equals(Alpha8 other)
	{
		return this.packedValue == other.packedValue;
	}

	/// <summary>
	/// Gets a string representation of the packed vector.
	/// </summary>
	/// <returns>A string representation of the packed vector.</returns>
	public override string ToString()
	{
		return ((float)(int)this.packedValue / 255f).ToString();
	}

	/// <summary>
	/// Gets a hash code of the packed vector.
	/// </summary>
	/// <returns>The hash code for the packed vector.</returns>
	public override int GetHashCode()
	{
		return this.packedValue.GetHashCode();
	}

	public static bool operator ==(Alpha8 lhs, Alpha8 rhs)
	{
		return lhs.packedValue == rhs.packedValue;
	}

	public static bool operator !=(Alpha8 lhs, Alpha8 rhs)
	{
		return lhs.packedValue != rhs.packedValue;
	}

	private static byte Pack(float alpha)
	{
		return (byte)Math.Round(MathHelper.Clamp(alpha, 0f, 1f) * 255f);
	}
}
