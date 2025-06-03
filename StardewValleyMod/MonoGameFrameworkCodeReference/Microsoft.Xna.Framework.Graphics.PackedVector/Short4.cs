using System;

namespace Microsoft.Xna.Framework.Graphics.PackedVector;

/// <summary>
/// Packed vector type containing four 16-bit signed integer values.
/// </summary>
public struct Short4 : IPackedVector<ulong>, IPackedVector, IEquatable<Short4>
{
	private ulong packedValue;

	/// <summary>
	/// Directly gets or sets the packed representation of the value.
	/// </summary>
	/// <value>The packed representation of the value.</value>
	[CLSCompliant(false)]
	public ulong PackedValue
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
	/// Initializes a new instance of the Short4 class.
	/// </summary>
	/// <param name="vector">A vector containing the initial values for the components of the Short4 structure.</param>
	public Short4(Vector4 vector)
	{
		this.packedValue = Short4.Pack(ref vector);
	}

	/// <summary>
	/// Initializes a new instance of the Short4 class.
	/// </summary>
	/// <param name="x">Initial value for the x component.</param>
	/// <param name="y">Initial value for the y component.</param>
	/// <param name="z">Initial value for the z component.</param>
	/// <param name="w">Initial value for the w component.</param>
	public Short4(float x, float y, float z, float w)
	{
		Vector4 vector = new Vector4(x, y, z, w);
		this.packedValue = Short4.Pack(ref vector);
	}

	/// <summary>
	/// Compares the current instance of a class to another instance to determine whether they are different.
	/// </summary>
	/// <param name="a">The object to the left of the equality operator.</param>
	/// <param name="b">The object to the right of the equality operator.</param>
	/// <returns>true if the objects are different; false otherwise.</returns>
	public static bool operator !=(Short4 a, Short4 b)
	{
		return a.PackedValue != b.PackedValue;
	}

	/// <summary>
	/// Compares the current instance of a class to another instance to determine whether they are the same.
	/// </summary>
	/// <param name="a">The object to the left of the equality operator.</param>
	/// <param name="b">The object to the right of the equality operator.</param>
	/// <returns>true if the objects are the same; false otherwise.</returns>
	public static bool operator ==(Short4 a, Short4 b)
	{
		return a.PackedValue == b.PackedValue;
	}

	/// <summary>
	/// Returns a value that indicates whether the current instance is equal to a specified object.
	/// </summary>
	/// <param name="obj">The object with which to make the comparison.</param>
	/// <returns>true if the current instance is equal to the specified object; false otherwise.</returns>
	public override bool Equals(object obj)
	{
		if (obj is Short4)
		{
			return this == (Short4)obj;
		}
		return false;
	}

	/// <summary>
	/// Returns a value that indicates whether the current instance is equal to a specified object.
	/// </summary>
	/// <param name="other">The object with which to make the comparison.</param>
	/// <returns>true if the current instance is equal to the specified object; false otherwise.</returns>
	public bool Equals(Short4 other)
	{
		return this == other;
	}

	/// <summary>
	/// Gets the hash code for the current instance.
	/// </summary>
	/// <returns>Hash code for the instance.</returns>
	public override int GetHashCode()
	{
		return this.packedValue.GetHashCode();
	}

	/// <summary>
	/// Returns a string representation of the current instance.
	/// </summary>
	/// <returns>String that represents the object.</returns>
	public override string ToString()
	{
		return this.packedValue.ToString("x16");
	}

	/// <summary>
	/// Packs a vector into a ulong.
	/// </summary>
	/// <param name="vector">The vector containing the values to pack.</param>
	/// <returns>The ulong containing the packed values.</returns>
	private static ulong Pack(ref Vector4 vector)
	{
		long num = (long)(int)Math.Round(MathHelper.Clamp(vector.X, -32768f, 32767f)) & 0xFFFFL;
		ulong word3 = ((ulong)(int)Math.Round(MathHelper.Clamp(vector.Y, -32768f, 32767f)) & 0xFFFFuL) << 16;
		ulong word4 = ((ulong)(int)Math.Round(MathHelper.Clamp(vector.Z, -32768f, 32767f)) & 0xFFFFuL) << 32;
		ulong word5 = ((ulong)(int)Math.Round(MathHelper.Clamp(vector.W, -32768f, 32767f)) & 0xFFFFuL) << 48;
		return (ulong)num | word3 | word4 | word5;
	}

	/// <summary>
	/// Sets the packed representation from a Vector4.
	/// </summary>
	/// <param name="vector">The vector to create the packed representation from.</param>
	void IPackedVector.PackFromVector4(Vector4 vector)
	{
		this.packedValue = Short4.Pack(ref vector);
	}

	/// <summary>
	/// Expands the packed representation into a Vector4.
	/// </summary>
	/// <returns>The expanded vector.</returns>
	public Vector4 ToVector4()
	{
		return new Vector4((short)(this.packedValue & 0xFFFF), (short)((this.packedValue >> 16) & 0xFFFF), (short)((this.packedValue >> 32) & 0xFFFF), (short)((this.packedValue >> 48) & 0xFFFF));
	}
}
