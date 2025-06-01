using System;

namespace Microsoft.Xna.Framework.Graphics.PackedVector;

/// <summary>
/// Packed vector type containing four 8-bit unsigned integer values, ranging from 0 to 255.
/// </summary>
public struct Byte4 : IPackedVector<uint>, IPackedVector, IEquatable<Byte4>
{
	private uint packedValue;

	/// <summary>
	/// Directly gets or sets the packed representation of the value.
	/// </summary>
	/// <value>The packed representation of the value.</value>
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

	/// <summary>
	/// Initializes a new instance of the Byte4 class.
	/// </summary>
	/// <param name="vector">A vector containing the initial values for the components of the Byte4 structure.</param>
	public Byte4(Vector4 vector)
	{
		this.packedValue = Byte4.Pack(ref vector);
	}

	/// <summary>
	/// Initializes a new instance of the Byte4 class.
	/// </summary>
	/// <param name="x">Initial value for the x component.</param>
	/// <param name="y">Initial value for the y component.</param>
	/// <param name="z">Initial value for the z component.</param>
	/// <param name="w">Initial value for the w component.</param>
	public Byte4(float x, float y, float z, float w)
	{
		Vector4 vector = new Vector4(x, y, z, w);
		this.packedValue = Byte4.Pack(ref vector);
	}

	/// <summary>
	/// Compares the current instance of a class to another instance to determine whether they are different.
	/// </summary>
	/// <param name="a">The object to the left of the equality operator.</param>
	/// <param name="b">The object to the right of the equality operator.</param>
	/// <returns>true if the objects are different; false otherwise.</returns>
	public static bool operator !=(Byte4 a, Byte4 b)
	{
		return a.PackedValue != b.PackedValue;
	}

	/// <summary>
	/// Compares the current instance of a class to another instance to determine whether they are the same.
	/// </summary>
	/// <param name="a">The object to the left of the equality operator.</param>
	/// <param name="b">The object to the right of the equality operator.</param>
	/// <returns>true if the objects are the same; false otherwise.</returns>
	public static bool operator ==(Byte4 a, Byte4 b)
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
		if (obj is Byte4)
		{
			return this == (Byte4)obj;
		}
		return false;
	}

	/// <summary>
	/// Returns a value that indicates whether the current instance is equal to a specified object.
	/// </summary>
	/// <param name="other">The object with which to make the comparison.</param>
	/// <returns>true if the current instance is equal to the specified object; false otherwise.</returns>
	public bool Equals(Byte4 other)
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
		return this.packedValue.ToString("x8");
	}

	/// <summary>
	/// Packs a vector into a uint.
	/// </summary>
	/// <param name="vector">The vector containing the values to pack.</param>
	/// <returns>The ulong containing the packed values.</returns>
	private static uint Pack(ref Vector4 vector)
	{
		uint num = (uint)Math.Round(MathHelper.Clamp(vector.X, 0f, 255f)) & 0xFF;
		uint byte3 = ((uint)Math.Round(MathHelper.Clamp(vector.Y, 0f, 255f)) & 0xFF) << 8;
		uint byte4 = ((uint)Math.Round(MathHelper.Clamp(vector.Z, 0f, 255f)) & 0xFF) << 16;
		uint byte5 = ((uint)Math.Round(MathHelper.Clamp(vector.W, 0f, 255f)) & 0xFF) << 24;
		return num | byte3 | byte4 | byte5;
	}

	/// <summary>
	/// Sets the packed representation from a Vector4.
	/// </summary>
	/// <param name="vector">The vector to create the packed representation from.</param>
	void IPackedVector.PackFromVector4(Vector4 vector)
	{
		this.packedValue = Byte4.Pack(ref vector);
	}

	/// <summary>
	/// Expands the packed representation into a Vector4.
	/// </summary>
	/// <returns>The expanded vector.</returns>
	public Vector4 ToVector4()
	{
		return new Vector4(this.packedValue & 0xFF, (this.packedValue >> 8) & 0xFF, (this.packedValue >> 16) & 0xFF, (this.packedValue >> 24) & 0xFF);
	}
}
