using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using Microsoft.Xna.Framework.Design;

namespace Microsoft.Xna.Framework;

/// <summary>
/// Describes a 2D-vector.
/// </summary>
[TypeConverter(typeof(Vector2TypeConverter))]
[DataContract]
[DebuggerDisplay("{DebugDisplayString,nq}")]
public struct Vector2 : IEquatable<Vector2>
{
	private static readonly Vector2 zeroVector = new Vector2(0f, 0f);

	private static readonly Vector2 unitVector = new Vector2(1f, 1f);

	private static readonly Vector2 unitXVector = new Vector2(1f, 0f);

	private static readonly Vector2 unitYVector = new Vector2(0f, 1f);

	/// <summary>
	/// The x coordinate of this <see cref="T:Microsoft.Xna.Framework.Vector2" />.
	/// </summary>
	[DataMember]
	public float X;

	/// <summary>
	/// The y coordinate of this <see cref="T:Microsoft.Xna.Framework.Vector2" />.
	/// </summary>
	[DataMember]
	public float Y;

	/// <summary>
	/// Returns a <see cref="T:Microsoft.Xna.Framework.Vector2" /> with components 0, 0.
	/// </summary>
	public static Vector2 Zero => Vector2.zeroVector;

	/// <summary>
	/// Returns a <see cref="T:Microsoft.Xna.Framework.Vector2" /> with components 1, 1.
	/// </summary>
	public static Vector2 One => Vector2.unitVector;

	/// <summary>
	/// Returns a <see cref="T:Microsoft.Xna.Framework.Vector2" /> with components 1, 0.
	/// </summary>
	public static Vector2 UnitX => Vector2.unitXVector;

	/// <summary>
	/// Returns a <see cref="T:Microsoft.Xna.Framework.Vector2" /> with components 0, 1.
	/// </summary>
	public static Vector2 UnitY => Vector2.unitYVector;

	internal string DebugDisplayString => this.X + "  " + this.Y;

	/// <summary>
	/// Constructs a 2d vector with X and Y from two values.
	/// </summary>
	/// <param name="x">The x coordinate in 2d-space.</param>
	/// <param name="y">The y coordinate in 2d-space.</param>
	public Vector2(float x, float y)
	{
		this.X = x;
		this.Y = y;
	}

	/// <summary>
	/// Constructs a 2d vector with X and Y set to the same value.
	/// </summary>
	/// <param name="value">The x and y coordinates in 2d-space.</param>
	public Vector2(float value)
	{
		this.X = value;
		this.Y = value;
	}

	/// <summary>
	/// Inverts values in the specified <see cref="T:Microsoft.Xna.Framework.Vector2" />.
	/// </summary>
	/// <param name="value">Source <see cref="T:Microsoft.Xna.Framework.Vector2" /> on the right of the sub sign.</param>
	/// <returns>Result of the inversion.</returns>
	public static Vector2 operator -(Vector2 value)
	{
		value.X = 0f - value.X;
		value.Y = 0f - value.Y;
		return value;
	}

	/// <summary>
	/// Adds two vectors.
	/// </summary>
	/// <param name="value1">Source <see cref="T:Microsoft.Xna.Framework.Vector2" /> on the left of the add sign.</param>
	/// <param name="value2">Source <see cref="T:Microsoft.Xna.Framework.Vector2" /> on the right of the add sign.</param>
	/// <returns>Sum of the vectors.</returns>
	public static Vector2 operator +(Vector2 value1, Vector2 value2)
	{
		value1.X += value2.X;
		value1.Y += value2.Y;
		return value1;
	}

	/// <summary>
	/// Subtracts a <see cref="T:Microsoft.Xna.Framework.Vector2" /> from a <see cref="T:Microsoft.Xna.Framework.Vector2" />.
	/// </summary>
	/// <param name="value1">Source <see cref="T:Microsoft.Xna.Framework.Vector2" /> on the left of the sub sign.</param>
	/// <param name="value2">Source <see cref="T:Microsoft.Xna.Framework.Vector2" /> on the right of the sub sign.</param>
	/// <returns>Result of the vector subtraction.</returns>
	public static Vector2 operator -(Vector2 value1, Vector2 value2)
	{
		value1.X -= value2.X;
		value1.Y -= value2.Y;
		return value1;
	}

	/// <summary>
	/// Multiplies the components of two vectors by each other.
	/// </summary>
	/// <param name="value1">Source <see cref="T:Microsoft.Xna.Framework.Vector2" /> on the left of the mul sign.</param>
	/// <param name="value2">Source <see cref="T:Microsoft.Xna.Framework.Vector2" /> on the right of the mul sign.</param>
	/// <returns>Result of the vector multiplication.</returns>
	public static Vector2 operator *(Vector2 value1, Vector2 value2)
	{
		value1.X *= value2.X;
		value1.Y *= value2.Y;
		return value1;
	}

	/// <summary>
	/// Multiplies the components of vector by a scalar.
	/// </summary>
	/// <param name="value">Source <see cref="T:Microsoft.Xna.Framework.Vector2" /> on the left of the mul sign.</param>
	/// <param name="scaleFactor">Scalar value on the right of the mul sign.</param>
	/// <returns>Result of the vector multiplication with a scalar.</returns>
	public static Vector2 operator *(Vector2 value, float scaleFactor)
	{
		value.X *= scaleFactor;
		value.Y *= scaleFactor;
		return value;
	}

	/// <summary>
	/// Multiplies the components of vector by a scalar.
	/// </summary>
	/// <param name="scaleFactor">Scalar value on the left of the mul sign.</param>
	/// <param name="value">Source <see cref="T:Microsoft.Xna.Framework.Vector2" /> on the right of the mul sign.</param>
	/// <returns>Result of the vector multiplication with a scalar.</returns>
	public static Vector2 operator *(float scaleFactor, Vector2 value)
	{
		value.X *= scaleFactor;
		value.Y *= scaleFactor;
		return value;
	}

	/// <summary>
	/// Divides the components of a <see cref="T:Microsoft.Xna.Framework.Vector2" /> by the components of another <see cref="T:Microsoft.Xna.Framework.Vector2" />.
	/// </summary>
	/// <param name="value1">Source <see cref="T:Microsoft.Xna.Framework.Vector2" /> on the left of the div sign.</param>
	/// <param name="value2">Divisor <see cref="T:Microsoft.Xna.Framework.Vector2" /> on the right of the div sign.</param>
	/// <returns>The result of dividing the vectors.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector2 operator /(Vector2 value1, Vector2 value2)
	{
		value1.X /= value2.X;
		value1.Y /= value2.Y;
		return value1;
	}

	/// <summary>
	/// Divides the components of a <see cref="T:Microsoft.Xna.Framework.Vector2" /> by a scalar.
	/// </summary>
	/// <param name="value1">Source <see cref="T:Microsoft.Xna.Framework.Vector2" /> on the left of the div sign.</param>
	/// <param name="divider">Divisor scalar on the right of the div sign.</param>
	/// <returns>The result of dividing a vector by a scalar.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Vector2 operator /(Vector2 value1, float divider)
	{
		float factor = 1f / divider;
		value1.X *= factor;
		value1.Y *= factor;
		return value1;
	}

	/// <summary>
	/// Compares whether two <see cref="T:Microsoft.Xna.Framework.Vector2" /> instances are equal.
	/// </summary>
	/// <param name="value1"><see cref="T:Microsoft.Xna.Framework.Vector2" /> instance on the left of the equal sign.</param>
	/// <param name="value2"><see cref="T:Microsoft.Xna.Framework.Vector2" /> instance on the right of the equal sign.</param>
	/// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
	public static bool operator ==(Vector2 value1, Vector2 value2)
	{
		if (value1.X == value2.X)
		{
			return value1.Y == value2.Y;
		}
		return false;
	}

	/// <summary>
	/// Compares whether two <see cref="T:Microsoft.Xna.Framework.Vector2" /> instances are not equal.
	/// </summary>
	/// <param name="value1"><see cref="T:Microsoft.Xna.Framework.Vector2" /> instance on the left of the not equal sign.</param>
	/// <param name="value2"><see cref="T:Microsoft.Xna.Framework.Vector2" /> instance on the right of the not equal sign.</param>
	/// <returns><c>true</c> if the instances are not equal; <c>false</c> otherwise.</returns>	
	public static bool operator !=(Vector2 value1, Vector2 value2)
	{
		if (value1.X == value2.X)
		{
			return value1.Y != value2.Y;
		}
		return true;
	}

	/// <summary>
	/// Performs vector addition on <paramref name="value1" /> and <paramref name="value2" />.
	/// </summary>
	/// <param name="value1">The first vector to add.</param>
	/// <param name="value2">The second vector to add.</param>
	/// <returns>The result of the vector addition.</returns>
	public static Vector2 Add(Vector2 value1, Vector2 value2)
	{
		value1.X += value2.X;
		value1.Y += value2.Y;
		return value1;
	}

	/// <summary>
	/// Performs vector addition on <paramref name="value1" /> and
	/// <paramref name="value2" />, storing the result of the
	/// addition in <paramref name="result" />.
	/// </summary>
	/// <param name="value1">The first vector to add.</param>
	/// <param name="value2">The second vector to add.</param>
	/// <param name="result">The result of the vector addition.</param>
	public static void Add(ref Vector2 value1, ref Vector2 value2, out Vector2 result)
	{
		result.X = value1.X + value2.X;
		result.Y = value1.Y + value2.Y;
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector2" /> that contains the cartesian coordinates of a vector specified in barycentric coordinates and relative to 2d-triangle.
	/// </summary>
	/// <param name="value1">The first vector of 2d-triangle.</param>
	/// <param name="value2">The second vector of 2d-triangle.</param>
	/// <param name="value3">The third vector of 2d-triangle.</param>
	/// <param name="amount1">Barycentric scalar <c>b2</c> which represents a weighting factor towards second vector of 2d-triangle.</param>
	/// <param name="amount2">Barycentric scalar <c>b3</c> which represents a weighting factor towards third vector of 2d-triangle.</param>
	/// <returns>The cartesian translation of barycentric coordinates.</returns>
	public static Vector2 Barycentric(Vector2 value1, Vector2 value2, Vector2 value3, float amount1, float amount2)
	{
		return new Vector2(MathHelper.Barycentric(value1.X, value2.X, value3.X, amount1, amount2), MathHelper.Barycentric(value1.Y, value2.Y, value3.Y, amount1, amount2));
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector2" /> that contains the cartesian coordinates of a vector specified in barycentric coordinates and relative to 2d-triangle.
	/// </summary>
	/// <param name="value1">The first vector of 2d-triangle.</param>
	/// <param name="value2">The second vector of 2d-triangle.</param>
	/// <param name="value3">The third vector of 2d-triangle.</param>
	/// <param name="amount1">Barycentric scalar <c>b2</c> which represents a weighting factor towards second vector of 2d-triangle.</param>
	/// <param name="amount2">Barycentric scalar <c>b3</c> which represents a weighting factor towards third vector of 2d-triangle.</param>
	/// <param name="result">The cartesian translation of barycentric coordinates as an output parameter.</param>
	public static void Barycentric(ref Vector2 value1, ref Vector2 value2, ref Vector2 value3, float amount1, float amount2, out Vector2 result)
	{
		result.X = MathHelper.Barycentric(value1.X, value2.X, value3.X, amount1, amount2);
		result.Y = MathHelper.Barycentric(value1.Y, value2.Y, value3.Y, amount1, amount2);
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector2" /> that contains CatmullRom interpolation of the specified vectors.
	/// </summary>
	/// <param name="value1">The first vector in interpolation.</param>
	/// <param name="value2">The second vector in interpolation.</param>
	/// <param name="value3">The third vector in interpolation.</param>
	/// <param name="value4">The fourth vector in interpolation.</param>
	/// <param name="amount">Weighting factor.</param>
	/// <returns>The result of CatmullRom interpolation.</returns>
	public static Vector2 CatmullRom(Vector2 value1, Vector2 value2, Vector2 value3, Vector2 value4, float amount)
	{
		return new Vector2(MathHelper.CatmullRom(value1.X, value2.X, value3.X, value4.X, amount), MathHelper.CatmullRom(value1.Y, value2.Y, value3.Y, value4.Y, amount));
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector2" /> that contains CatmullRom interpolation of the specified vectors.
	/// </summary>
	/// <param name="value1">The first vector in interpolation.</param>
	/// <param name="value2">The second vector in interpolation.</param>
	/// <param name="value3">The third vector in interpolation.</param>
	/// <param name="value4">The fourth vector in interpolation.</param>
	/// <param name="amount">Weighting factor.</param>
	/// <param name="result">The result of CatmullRom interpolation as an output parameter.</param>
	public static void CatmullRom(ref Vector2 value1, ref Vector2 value2, ref Vector2 value3, ref Vector2 value4, float amount, out Vector2 result)
	{
		result.X = MathHelper.CatmullRom(value1.X, value2.X, value3.X, value4.X, amount);
		result.Y = MathHelper.CatmullRom(value1.Y, value2.Y, value3.Y, value4.Y, amount);
	}

	/// <summary>
	/// Round the members of this <see cref="T:Microsoft.Xna.Framework.Vector2" /> towards positive infinity.
	/// </summary>
	public void Ceiling()
	{
		this.X = (float)Math.Ceiling(this.X);
		this.Y = (float)Math.Ceiling(this.Y);
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector2" /> that contains members from another vector rounded towards positive infinity.
	/// </summary>
	/// <param name="value">Source <see cref="T:Microsoft.Xna.Framework.Vector2" />.</param>
	/// <returns>The rounded <see cref="T:Microsoft.Xna.Framework.Vector2" />.</returns>
	public static Vector2 Ceiling(Vector2 value)
	{
		value.X = (float)Math.Ceiling(value.X);
		value.Y = (float)Math.Ceiling(value.Y);
		return value;
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector2" /> that contains members from another vector rounded towards positive infinity.
	/// </summary>
	/// <param name="value">Source <see cref="T:Microsoft.Xna.Framework.Vector2" />.</param>
	/// <param name="result">The rounded <see cref="T:Microsoft.Xna.Framework.Vector2" />.</param>
	public static void Ceiling(ref Vector2 value, out Vector2 result)
	{
		result.X = (float)Math.Ceiling(value.X);
		result.Y = (float)Math.Ceiling(value.Y);
	}

	/// <summary>
	/// Clamps the specified value within a range.
	/// </summary>
	/// <param name="value1">The value to clamp.</param>
	/// <param name="min">The min value.</param>
	/// <param name="max">The max value.</param>
	/// <returns>The clamped value.</returns>
	public static Vector2 Clamp(Vector2 value1, Vector2 min, Vector2 max)
	{
		return new Vector2(MathHelper.Clamp(value1.X, min.X, max.X), MathHelper.Clamp(value1.Y, min.Y, max.Y));
	}

	/// <summary>
	/// Clamps the specified value within a range.
	/// </summary>
	/// <param name="value1">The value to clamp.</param>
	/// <param name="min">The min value.</param>
	/// <param name="max">The max value.</param>
	/// <param name="result">The clamped value as an output parameter.</param>
	public static void Clamp(ref Vector2 value1, ref Vector2 min, ref Vector2 max, out Vector2 result)
	{
		result.X = MathHelper.Clamp(value1.X, min.X, max.X);
		result.Y = MathHelper.Clamp(value1.Y, min.Y, max.Y);
	}

	/// <summary>
	/// Returns the distance between two vectors.
	/// </summary>
	/// <param name="value1">The first vector.</param>
	/// <param name="value2">The second vector.</param>
	/// <returns>The distance between two vectors.</returns>
	public static float Distance(Vector2 value1, Vector2 value2)
	{
		float num = value1.X - value2.X;
		float v2 = value1.Y - value2.Y;
		return (float)Math.Sqrt(num * num + v2 * v2);
	}

	/// <summary>
	/// Returns the distance between two vectors.
	/// </summary>
	/// <param name="value1">The first vector.</param>
	/// <param name="value2">The second vector.</param>
	/// <param name="result">The distance between two vectors as an output parameter.</param>
	public static void Distance(ref Vector2 value1, ref Vector2 value2, out float result)
	{
		float v1 = value1.X - value2.X;
		float v2 = value1.Y - value2.Y;
		result = (float)Math.Sqrt(v1 * v1 + v2 * v2);
	}

	/// <summary>
	/// Returns the squared distance between two vectors.
	/// </summary>
	/// <param name="value1">The first vector.</param>
	/// <param name="value2">The second vector.</param>
	/// <returns>The squared distance between two vectors.</returns>
	public static float DistanceSquared(Vector2 value1, Vector2 value2)
	{
		float num = value1.X - value2.X;
		float v2 = value1.Y - value2.Y;
		return num * num + v2 * v2;
	}

	/// <summary>
	/// Returns the squared distance between two vectors.
	/// </summary>
	/// <param name="value1">The first vector.</param>
	/// <param name="value2">The second vector.</param>
	/// <param name="result">The squared distance between two vectors as an output parameter.</param>
	public static void DistanceSquared(ref Vector2 value1, ref Vector2 value2, out float result)
	{
		float v1 = value1.X - value2.X;
		float v2 = value1.Y - value2.Y;
		result = v1 * v1 + v2 * v2;
	}

	/// <summary>
	/// Divides the components of a <see cref="T:Microsoft.Xna.Framework.Vector2" /> by the components of another <see cref="T:Microsoft.Xna.Framework.Vector2" />.
	/// </summary>
	/// <param name="value1">Source <see cref="T:Microsoft.Xna.Framework.Vector2" />.</param>
	/// <param name="value2">Divisor <see cref="T:Microsoft.Xna.Framework.Vector2" />.</param>
	/// <returns>The result of dividing the vectors.</returns>
	public static Vector2 Divide(Vector2 value1, Vector2 value2)
	{
		value1.X /= value2.X;
		value1.Y /= value2.Y;
		return value1;
	}

	/// <summary>
	/// Divides the components of a <see cref="T:Microsoft.Xna.Framework.Vector2" /> by the components of another <see cref="T:Microsoft.Xna.Framework.Vector2" />.
	/// </summary>
	/// <param name="value1">Source <see cref="T:Microsoft.Xna.Framework.Vector2" />.</param>
	/// <param name="value2">Divisor <see cref="T:Microsoft.Xna.Framework.Vector2" />.</param>
	/// <param name="result">The result of dividing the vectors as an output parameter.</param>
	public static void Divide(ref Vector2 value1, ref Vector2 value2, out Vector2 result)
	{
		result.X = value1.X / value2.X;
		result.Y = value1.Y / value2.Y;
	}

	/// <summary>
	/// Divides the components of a <see cref="T:Microsoft.Xna.Framework.Vector2" /> by a scalar.
	/// </summary>
	/// <param name="value1">Source <see cref="T:Microsoft.Xna.Framework.Vector2" />.</param>
	/// <param name="divider">Divisor scalar.</param>
	/// <returns>The result of dividing a vector by a scalar.</returns>
	public static Vector2 Divide(Vector2 value1, float divider)
	{
		float factor = 1f / divider;
		value1.X *= factor;
		value1.Y *= factor;
		return value1;
	}

	/// <summary>
	/// Divides the components of a <see cref="T:Microsoft.Xna.Framework.Vector2" /> by a scalar.
	/// </summary>
	/// <param name="value1">Source <see cref="T:Microsoft.Xna.Framework.Vector2" />.</param>
	/// <param name="divider">Divisor scalar.</param>
	/// <param name="result">The result of dividing a vector by a scalar as an output parameter.</param>
	public static void Divide(ref Vector2 value1, float divider, out Vector2 result)
	{
		float factor = 1f / divider;
		result.X = value1.X * factor;
		result.Y = value1.Y * factor;
	}

	/// <summary>
	/// Returns a dot product of two vectors.
	/// </summary>
	/// <param name="value1">The first vector.</param>
	/// <param name="value2">The second vector.</param>
	/// <returns>The dot product of two vectors.</returns>
	public static float Dot(Vector2 value1, Vector2 value2)
	{
		return value1.X * value2.X + value1.Y * value2.Y;
	}

	/// <summary>
	/// Returns a dot product of two vectors.
	/// </summary>
	/// <param name="value1">The first vector.</param>
	/// <param name="value2">The second vector.</param>
	/// <param name="result">The dot product of two vectors as an output parameter.</param>
	public static void Dot(ref Vector2 value1, ref Vector2 value2, out float result)
	{
		result = value1.X * value2.X + value1.Y * value2.Y;
	}

	/// <summary>
	/// Compares whether current instance is equal to specified <see cref="T:System.Object" />.
	/// </summary>
	/// <param name="obj">The <see cref="T:System.Object" /> to compare.</param>
	/// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
	public override bool Equals(object obj)
	{
		if (obj is Vector2)
		{
			return this.Equals((Vector2)obj);
		}
		return false;
	}

	/// <summary>
	/// Compares whether current instance is equal to specified <see cref="T:Microsoft.Xna.Framework.Vector2" />.
	/// </summary>
	/// <param name="other">The <see cref="T:Microsoft.Xna.Framework.Vector2" /> to compare.</param>
	/// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
	public bool Equals(Vector2 other)
	{
		if (this.X == other.X)
		{
			return this.Y == other.Y;
		}
		return false;
	}

	/// <summary>
	/// Round the members of this <see cref="T:Microsoft.Xna.Framework.Vector2" /> towards negative infinity.
	/// </summary>
	public void Floor()
	{
		this.X = (float)Math.Floor(this.X);
		this.Y = (float)Math.Floor(this.Y);
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector2" /> that contains members from another vector rounded towards negative infinity.
	/// </summary>
	/// <param name="value">Source <see cref="T:Microsoft.Xna.Framework.Vector2" />.</param>
	/// <returns>The rounded <see cref="T:Microsoft.Xna.Framework.Vector2" />.</returns>
	public static Vector2 Floor(Vector2 value)
	{
		value.X = (float)Math.Floor(value.X);
		value.Y = (float)Math.Floor(value.Y);
		return value;
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector2" /> that contains members from another vector rounded towards negative infinity.
	/// </summary>
	/// <param name="value">Source <see cref="T:Microsoft.Xna.Framework.Vector2" />.</param>
	/// <param name="result">The rounded <see cref="T:Microsoft.Xna.Framework.Vector2" />.</param>
	public static void Floor(ref Vector2 value, out Vector2 result)
	{
		result.X = (float)Math.Floor(value.X);
		result.Y = (float)Math.Floor(value.Y);
	}

	/// <summary>
	/// Gets the hash code of this <see cref="T:Microsoft.Xna.Framework.Vector2" />.
	/// </summary>
	/// <returns>Hash code of this <see cref="T:Microsoft.Xna.Framework.Vector2" />.</returns>
	public override int GetHashCode()
	{
		return (this.X.GetHashCode() * 397) ^ this.Y.GetHashCode();
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector2" /> that contains hermite spline interpolation.
	/// </summary>
	/// <param name="value1">The first position vector.</param>
	/// <param name="tangent1">The first tangent vector.</param>
	/// <param name="value2">The second position vector.</param>
	/// <param name="tangent2">The second tangent vector.</param>
	/// <param name="amount">Weighting factor.</param>
	/// <returns>The hermite spline interpolation vector.</returns>
	public static Vector2 Hermite(Vector2 value1, Vector2 tangent1, Vector2 value2, Vector2 tangent2, float amount)
	{
		return new Vector2(MathHelper.Hermite(value1.X, tangent1.X, value2.X, tangent2.X, amount), MathHelper.Hermite(value1.Y, tangent1.Y, value2.Y, tangent2.Y, amount));
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector2" /> that contains hermite spline interpolation.
	/// </summary>
	/// <param name="value1">The first position vector.</param>
	/// <param name="tangent1">The first tangent vector.</param>
	/// <param name="value2">The second position vector.</param>
	/// <param name="tangent2">The second tangent vector.</param>
	/// <param name="amount">Weighting factor.</param>
	/// <param name="result">The hermite spline interpolation vector as an output parameter.</param>
	public static void Hermite(ref Vector2 value1, ref Vector2 tangent1, ref Vector2 value2, ref Vector2 tangent2, float amount, out Vector2 result)
	{
		result.X = MathHelper.Hermite(value1.X, tangent1.X, value2.X, tangent2.X, amount);
		result.Y = MathHelper.Hermite(value1.Y, tangent1.Y, value2.Y, tangent2.Y, amount);
	}

	/// <summary>
	/// Returns the length of this <see cref="T:Microsoft.Xna.Framework.Vector2" />.
	/// </summary>
	/// <returns>The length of this <see cref="T:Microsoft.Xna.Framework.Vector2" />.</returns>
	public float Length()
	{
		return (float)Math.Sqrt(this.X * this.X + this.Y * this.Y);
	}

	/// <summary>
	/// Returns the squared length of this <see cref="T:Microsoft.Xna.Framework.Vector2" />.
	/// </summary>
	/// <returns>The squared length of this <see cref="T:Microsoft.Xna.Framework.Vector2" />.</returns>
	public float LengthSquared()
	{
		return this.X * this.X + this.Y * this.Y;
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector2" /> that contains linear interpolation of the specified vectors.
	/// </summary>
	/// <param name="value1">The first vector.</param>
	/// <param name="value2">The second vector.</param>
	/// <param name="amount">Weighting value(between 0.0 and 1.0).</param>
	/// <returns>The result of linear interpolation of the specified vectors.</returns>
	public static Vector2 Lerp(Vector2 value1, Vector2 value2, float amount)
	{
		return new Vector2(MathHelper.Lerp(value1.X, value2.X, amount), MathHelper.Lerp(value1.Y, value2.Y, amount));
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector2" /> that contains linear interpolation of the specified vectors.
	/// </summary>
	/// <param name="value1">The first vector.</param>
	/// <param name="value2">The second vector.</param>
	/// <param name="amount">Weighting value(between 0.0 and 1.0).</param>
	/// <param name="result">The result of linear interpolation of the specified vectors as an output parameter.</param>
	public static void Lerp(ref Vector2 value1, ref Vector2 value2, float amount, out Vector2 result)
	{
		result.X = MathHelper.Lerp(value1.X, value2.X, amount);
		result.Y = MathHelper.Lerp(value1.Y, value2.Y, amount);
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector2" /> that contains linear interpolation of the specified vectors.
	/// Uses <see cref="M:Microsoft.Xna.Framework.MathHelper.LerpPrecise(System.Single,System.Single,System.Single)" /> on MathHelper for the interpolation.
	/// Less efficient but more precise compared to <see cref="M:Microsoft.Xna.Framework.Vector2.Lerp(Microsoft.Xna.Framework.Vector2,Microsoft.Xna.Framework.Vector2,System.Single)" />.
	/// See remarks section of <see cref="M:Microsoft.Xna.Framework.MathHelper.LerpPrecise(System.Single,System.Single,System.Single)" /> on MathHelper for more info.
	/// </summary>
	/// <param name="value1">The first vector.</param>
	/// <param name="value2">The second vector.</param>
	/// <param name="amount">Weighting value(between 0.0 and 1.0).</param>
	/// <returns>The result of linear interpolation of the specified vectors.</returns>
	public static Vector2 LerpPrecise(Vector2 value1, Vector2 value2, float amount)
	{
		return new Vector2(MathHelper.LerpPrecise(value1.X, value2.X, amount), MathHelper.LerpPrecise(value1.Y, value2.Y, amount));
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector2" /> that contains linear interpolation of the specified vectors.
	/// Uses <see cref="M:Microsoft.Xna.Framework.MathHelper.LerpPrecise(System.Single,System.Single,System.Single)" /> on MathHelper for the interpolation.
	/// Less efficient but more precise compared to <see cref="M:Microsoft.Xna.Framework.Vector2.Lerp(Microsoft.Xna.Framework.Vector2@,Microsoft.Xna.Framework.Vector2@,System.Single,Microsoft.Xna.Framework.Vector2@)" />.
	/// See remarks section of <see cref="M:Microsoft.Xna.Framework.MathHelper.LerpPrecise(System.Single,System.Single,System.Single)" /> on MathHelper for more info.
	/// </summary>
	/// <param name="value1">The first vector.</param>
	/// <param name="value2">The second vector.</param>
	/// <param name="amount">Weighting value(between 0.0 and 1.0).</param>
	/// <param name="result">The result of linear interpolation of the specified vectors as an output parameter.</param>
	public static void LerpPrecise(ref Vector2 value1, ref Vector2 value2, float amount, out Vector2 result)
	{
		result.X = MathHelper.LerpPrecise(value1.X, value2.X, amount);
		result.Y = MathHelper.LerpPrecise(value1.Y, value2.Y, amount);
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector2" /> that contains a maximal values from the two vectors.
	/// </summary>
	/// <param name="value1">The first vector.</param>
	/// <param name="value2">The second vector.</param>
	/// <returns>The <see cref="T:Microsoft.Xna.Framework.Vector2" /> with maximal values from the two vectors.</returns>
	public static Vector2 Max(Vector2 value1, Vector2 value2)
	{
		return new Vector2((value1.X > value2.X) ? value1.X : value2.X, (value1.Y > value2.Y) ? value1.Y : value2.Y);
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector2" /> that contains a maximal values from the two vectors.
	/// </summary>
	/// <param name="value1">The first vector.</param>
	/// <param name="value2">The second vector.</param>
	/// <param name="result">The <see cref="T:Microsoft.Xna.Framework.Vector2" /> with maximal values from the two vectors as an output parameter.</param>
	public static void Max(ref Vector2 value1, ref Vector2 value2, out Vector2 result)
	{
		result.X = ((value1.X > value2.X) ? value1.X : value2.X);
		result.Y = ((value1.Y > value2.Y) ? value1.Y : value2.Y);
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector2" /> that contains a minimal values from the two vectors.
	/// </summary>
	/// <param name="value1">The first vector.</param>
	/// <param name="value2">The second vector.</param>
	/// <returns>The <see cref="T:Microsoft.Xna.Framework.Vector2" /> with minimal values from the two vectors.</returns>
	public static Vector2 Min(Vector2 value1, Vector2 value2)
	{
		return new Vector2((value1.X < value2.X) ? value1.X : value2.X, (value1.Y < value2.Y) ? value1.Y : value2.Y);
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector2" /> that contains a minimal values from the two vectors.
	/// </summary>
	/// <param name="value1">The first vector.</param>
	/// <param name="value2">The second vector.</param>
	/// <param name="result">The <see cref="T:Microsoft.Xna.Framework.Vector2" /> with minimal values from the two vectors as an output parameter.</param>
	public static void Min(ref Vector2 value1, ref Vector2 value2, out Vector2 result)
	{
		result.X = ((value1.X < value2.X) ? value1.X : value2.X);
		result.Y = ((value1.Y < value2.Y) ? value1.Y : value2.Y);
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector2" /> that contains a multiplication of two vectors.
	/// </summary>
	/// <param name="value1">Source <see cref="T:Microsoft.Xna.Framework.Vector2" />.</param>
	/// <param name="value2">Source <see cref="T:Microsoft.Xna.Framework.Vector2" />.</param>
	/// <returns>The result of the vector multiplication.</returns>
	public static Vector2 Multiply(Vector2 value1, Vector2 value2)
	{
		value1.X *= value2.X;
		value1.Y *= value2.Y;
		return value1;
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector2" /> that contains a multiplication of two vectors.
	/// </summary>
	/// <param name="value1">Source <see cref="T:Microsoft.Xna.Framework.Vector2" />.</param>
	/// <param name="value2">Source <see cref="T:Microsoft.Xna.Framework.Vector2" />.</param>
	/// <param name="result">The result of the vector multiplication as an output parameter.</param>
	public static void Multiply(ref Vector2 value1, ref Vector2 value2, out Vector2 result)
	{
		result.X = value1.X * value2.X;
		result.Y = value1.Y * value2.Y;
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector2" /> that contains a multiplication of <see cref="T:Microsoft.Xna.Framework.Vector2" /> and a scalar.
	/// </summary>
	/// <param name="value1">Source <see cref="T:Microsoft.Xna.Framework.Vector2" />.</param>
	/// <param name="scaleFactor">Scalar value.</param>
	/// <returns>The result of the vector multiplication with a scalar.</returns>
	public static Vector2 Multiply(Vector2 value1, float scaleFactor)
	{
		value1.X *= scaleFactor;
		value1.Y *= scaleFactor;
		return value1;
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector2" /> that contains a multiplication of <see cref="T:Microsoft.Xna.Framework.Vector2" /> and a scalar.
	/// </summary>
	/// <param name="value1">Source <see cref="T:Microsoft.Xna.Framework.Vector2" />.</param>
	/// <param name="scaleFactor">Scalar value.</param>
	/// <param name="result">The result of the multiplication with a scalar as an output parameter.</param>
	public static void Multiply(ref Vector2 value1, float scaleFactor, out Vector2 result)
	{
		result.X = value1.X * scaleFactor;
		result.Y = value1.Y * scaleFactor;
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector2" /> that contains the specified vector inversion.
	/// </summary>
	/// <param name="value">Source <see cref="T:Microsoft.Xna.Framework.Vector2" />.</param>
	/// <returns>The result of the vector inversion.</returns>
	public static Vector2 Negate(Vector2 value)
	{
		value.X = 0f - value.X;
		value.Y = 0f - value.Y;
		return value;
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector2" /> that contains the specified vector inversion.
	/// </summary>
	/// <param name="value">Source <see cref="T:Microsoft.Xna.Framework.Vector2" />.</param>
	/// <param name="result">The result of the vector inversion as an output parameter.</param>
	public static void Negate(ref Vector2 value, out Vector2 result)
	{
		result.X = 0f - value.X;
		result.Y = 0f - value.Y;
	}

	/// <summary>
	/// Turns this <see cref="T:Microsoft.Xna.Framework.Vector2" /> to a unit vector with the same direction.
	/// </summary>
	public void Normalize()
	{
		float val = 1f / (float)Math.Sqrt(this.X * this.X + this.Y * this.Y);
		this.X *= val;
		this.Y *= val;
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector2" /> that contains a normalized values from another vector.
	/// </summary>
	/// <param name="value">Source <see cref="T:Microsoft.Xna.Framework.Vector2" />.</param>
	/// <returns>Unit vector.</returns>
	public static Vector2 Normalize(Vector2 value)
	{
		float val = 1f / (float)Math.Sqrt(value.X * value.X + value.Y * value.Y);
		value.X *= val;
		value.Y *= val;
		return value;
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector2" /> that contains a normalized values from another vector.
	/// </summary>
	/// <param name="value">Source <see cref="T:Microsoft.Xna.Framework.Vector2" />.</param>
	/// <param name="result">Unit vector as an output parameter.</param>
	public static void Normalize(ref Vector2 value, out Vector2 result)
	{
		float val = 1f / (float)Math.Sqrt(value.X * value.X + value.Y * value.Y);
		result.X = value.X * val;
		result.Y = value.Y * val;
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector2" /> that contains reflect vector of the given vector and normal.
	/// </summary>
	/// <param name="vector">Source <see cref="T:Microsoft.Xna.Framework.Vector2" />.</param>
	/// <param name="normal">Reflection normal.</param>
	/// <returns>Reflected vector.</returns>
	public static Vector2 Reflect(Vector2 vector, Vector2 normal)
	{
		float val = 2f * (vector.X * normal.X + vector.Y * normal.Y);
		Vector2 result = default(Vector2);
		result.X = vector.X - normal.X * val;
		result.Y = vector.Y - normal.Y * val;
		return result;
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector2" /> that contains reflect vector of the given vector and normal.
	/// </summary>
	/// <param name="vector">Source <see cref="T:Microsoft.Xna.Framework.Vector2" />.</param>
	/// <param name="normal">Reflection normal.</param>
	/// <param name="result">Reflected vector as an output parameter.</param>
	public static void Reflect(ref Vector2 vector, ref Vector2 normal, out Vector2 result)
	{
		float val = 2f * (vector.X * normal.X + vector.Y * normal.Y);
		result.X = vector.X - normal.X * val;
		result.Y = vector.Y - normal.Y * val;
	}

	/// <summary>
	/// Round the members of this <see cref="T:Microsoft.Xna.Framework.Vector2" /> to the nearest integer value.
	/// </summary>
	public void Round()
	{
		this.X = (float)Math.Round(this.X);
		this.Y = (float)Math.Round(this.Y);
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector2" /> that contains members from another vector rounded to the nearest integer value.
	/// </summary>
	/// <param name="value">Source <see cref="T:Microsoft.Xna.Framework.Vector2" />.</param>
	/// <returns>The rounded <see cref="T:Microsoft.Xna.Framework.Vector2" />.</returns>
	public static Vector2 Round(Vector2 value)
	{
		value.X = (float)Math.Round(value.X);
		value.Y = (float)Math.Round(value.Y);
		return value;
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector2" /> that contains members from another vector rounded to the nearest integer value.
	/// </summary>
	/// <param name="value">Source <see cref="T:Microsoft.Xna.Framework.Vector2" />.</param>
	/// <param name="result">The rounded <see cref="T:Microsoft.Xna.Framework.Vector2" />.</param>
	public static void Round(ref Vector2 value, out Vector2 result)
	{
		result.X = (float)Math.Round(value.X);
		result.Y = (float)Math.Round(value.Y);
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector2" /> that contains cubic interpolation of the specified vectors.
	/// </summary>
	/// <param name="value1">Source <see cref="T:Microsoft.Xna.Framework.Vector2" />.</param>
	/// <param name="value2">Source <see cref="T:Microsoft.Xna.Framework.Vector2" />.</param>
	/// <param name="amount">Weighting value.</param>
	/// <returns>Cubic interpolation of the specified vectors.</returns>
	public static Vector2 SmoothStep(Vector2 value1, Vector2 value2, float amount)
	{
		return new Vector2(MathHelper.SmoothStep(value1.X, value2.X, amount), MathHelper.SmoothStep(value1.Y, value2.Y, amount));
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector2" /> that contains cubic interpolation of the specified vectors.
	/// </summary>
	/// <param name="value1">Source <see cref="T:Microsoft.Xna.Framework.Vector2" />.</param>
	/// <param name="value2">Source <see cref="T:Microsoft.Xna.Framework.Vector2" />.</param>
	/// <param name="amount">Weighting value.</param>
	/// <param name="result">Cubic interpolation of the specified vectors as an output parameter.</param>
	public static void SmoothStep(ref Vector2 value1, ref Vector2 value2, float amount, out Vector2 result)
	{
		result.X = MathHelper.SmoothStep(value1.X, value2.X, amount);
		result.Y = MathHelper.SmoothStep(value1.Y, value2.Y, amount);
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector2" /> that contains subtraction of on <see cref="T:Microsoft.Xna.Framework.Vector2" /> from a another.
	/// </summary>
	/// <param name="value1">Source <see cref="T:Microsoft.Xna.Framework.Vector2" />.</param>
	/// <param name="value2">Source <see cref="T:Microsoft.Xna.Framework.Vector2" />.</param>
	/// <returns>The result of the vector subtraction.</returns>
	public static Vector2 Subtract(Vector2 value1, Vector2 value2)
	{
		value1.X -= value2.X;
		value1.Y -= value2.Y;
		return value1;
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector2" /> that contains subtraction of on <see cref="T:Microsoft.Xna.Framework.Vector2" /> from a another.
	/// </summary>
	/// <param name="value1">Source <see cref="T:Microsoft.Xna.Framework.Vector2" />.</param>
	/// <param name="value2">Source <see cref="T:Microsoft.Xna.Framework.Vector2" />.</param>
	/// <param name="result">The result of the vector subtraction as an output parameter.</param>
	public static void Subtract(ref Vector2 value1, ref Vector2 value2, out Vector2 result)
	{
		result.X = value1.X - value2.X;
		result.Y = value1.Y - value2.Y;
	}

	/// <summary>
	/// Returns a <see cref="T:System.String" /> representation of this <see cref="T:Microsoft.Xna.Framework.Vector2" /> in the format:
	/// {X:[<see cref="F:Microsoft.Xna.Framework.Vector2.X" />] Y:[<see cref="F:Microsoft.Xna.Framework.Vector2.Y" />]}
	/// </summary>
	/// <returns>A <see cref="T:System.String" /> representation of this <see cref="T:Microsoft.Xna.Framework.Vector2" />.</returns>
	public override string ToString()
	{
		return "{X:" + this.X + " Y:" + this.Y + "}";
	}

	/// <summary>
	/// Gets a <see cref="T:Microsoft.Xna.Framework.Point" /> representation for this object.
	/// </summary>
	/// <returns>A <see cref="T:Microsoft.Xna.Framework.Point" /> representation for this object.</returns>
	public Point ToPoint()
	{
		return new Point((int)this.X, (int)this.Y);
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector2" /> that contains a transformation of 2d-vector by the specified <see cref="T:Microsoft.Xna.Framework.Matrix" />.
	/// </summary>
	/// <param name="position">Source <see cref="T:Microsoft.Xna.Framework.Vector2" />.</param>
	/// <param name="matrix">The transformation <see cref="T:Microsoft.Xna.Framework.Matrix" />.</param>
	/// <returns>Transformed <see cref="T:Microsoft.Xna.Framework.Vector2" />.</returns>
	public static Vector2 Transform(Vector2 position, Matrix matrix)
	{
		return new Vector2(position.X * matrix.M11 + position.Y * matrix.M21 + matrix.M41, position.X * matrix.M12 + position.Y * matrix.M22 + matrix.M42);
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector2" /> that contains a transformation of 2d-vector by the specified <see cref="T:Microsoft.Xna.Framework.Matrix" />.
	/// </summary>
	/// <param name="position">Source <see cref="T:Microsoft.Xna.Framework.Vector2" />.</param>
	/// <param name="matrix">The transformation <see cref="T:Microsoft.Xna.Framework.Matrix" />.</param>
	/// <param name="result">Transformed <see cref="T:Microsoft.Xna.Framework.Vector2" /> as an output parameter.</param>
	public static void Transform(ref Vector2 position, ref Matrix matrix, out Vector2 result)
	{
		float x = position.X * matrix.M11 + position.Y * matrix.M21 + matrix.M41;
		float y = position.X * matrix.M12 + position.Y * matrix.M22 + matrix.M42;
		result.X = x;
		result.Y = y;
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector2" /> that contains a transformation of 2d-vector by the specified <see cref="T:Microsoft.Xna.Framework.Quaternion" />, representing the rotation.
	/// </summary>
	/// <param name="value">Source <see cref="T:Microsoft.Xna.Framework.Vector2" />.</param>
	/// <param name="rotation">The <see cref="T:Microsoft.Xna.Framework.Quaternion" /> which contains rotation transformation.</param>
	/// <returns>Transformed <see cref="T:Microsoft.Xna.Framework.Vector2" />.</returns>
	public static Vector2 Transform(Vector2 value, Quaternion rotation)
	{
		Vector2.Transform(ref value, ref rotation, out value);
		return value;
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector2" /> that contains a transformation of 2d-vector by the specified <see cref="T:Microsoft.Xna.Framework.Quaternion" />, representing the rotation.
	/// </summary>
	/// <param name="value">Source <see cref="T:Microsoft.Xna.Framework.Vector2" />.</param>
	/// <param name="rotation">The <see cref="T:Microsoft.Xna.Framework.Quaternion" /> which contains rotation transformation.</param>
	/// <param name="result">Transformed <see cref="T:Microsoft.Xna.Framework.Vector2" /> as an output parameter.</param>
	public static void Transform(ref Vector2 value, ref Quaternion rotation, out Vector2 result)
	{
		Vector3 vector = new Vector3(rotation.X + rotation.X, rotation.Y + rotation.Y, rotation.Z + rotation.Z);
		Vector3 rot2 = new Vector3(rotation.X, rotation.X, rotation.W);
		Vector3 rot3 = new Vector3(1f, rotation.Y, rotation.Z);
		Vector3 rot4 = vector * rot2;
		Vector3 rot5 = vector * rot3;
		Vector2 v = new Vector2
		{
			X = (float)((double)value.X * (1.0 - (double)rot5.Y - (double)rot5.Z) + (double)value.Y * ((double)rot4.Y - (double)rot4.Z)),
			Y = (float)((double)value.X * ((double)rot4.Y + (double)rot4.Z) + (double)value.Y * (1.0 - (double)rot4.X - (double)rot5.Z))
		};
		result.X = v.X;
		result.Y = v.Y;
	}

	/// <summary>
	/// Apply transformation on vectors within array of <see cref="T:Microsoft.Xna.Framework.Vector2" /> by the specified <see cref="T:Microsoft.Xna.Framework.Matrix" /> and places the results in an another array.
	/// </summary>
	/// <param name="sourceArray">Source array.</param>
	/// <param name="sourceIndex">The starting index of transformation in the source array.</param>
	/// <param name="matrix">The transformation <see cref="T:Microsoft.Xna.Framework.Matrix" />.</param>
	/// <param name="destinationArray">Destination array.</param>
	/// <param name="destinationIndex">The starting index in the destination array, where the first <see cref="T:Microsoft.Xna.Framework.Vector2" /> should be written.</param>
	/// <param name="length">The number of vectors to be transformed.</param>
	public static void Transform(Vector2[] sourceArray, int sourceIndex, ref Matrix matrix, Vector2[] destinationArray, int destinationIndex, int length)
	{
		if (sourceArray == null)
		{
			throw new ArgumentNullException("sourceArray");
		}
		if (destinationArray == null)
		{
			throw new ArgumentNullException("destinationArray");
		}
		if (sourceArray.Length < sourceIndex + length)
		{
			throw new ArgumentException("Source array length is lesser than sourceIndex + length");
		}
		if (destinationArray.Length < destinationIndex + length)
		{
			throw new ArgumentException("Destination array length is lesser than destinationIndex + length");
		}
		for (int x = 0; x < length; x++)
		{
			Vector2 position = sourceArray[sourceIndex + x];
			Vector2 destination = destinationArray[destinationIndex + x];
			destination.X = position.X * matrix.M11 + position.Y * matrix.M21 + matrix.M41;
			destination.Y = position.X * matrix.M12 + position.Y * matrix.M22 + matrix.M42;
			destinationArray[destinationIndex + x] = destination;
		}
	}

	/// <summary>
	/// Apply transformation on vectors within array of <see cref="T:Microsoft.Xna.Framework.Vector2" /> by the specified <see cref="T:Microsoft.Xna.Framework.Quaternion" /> and places the results in an another array.
	/// </summary>
	/// <param name="sourceArray">Source array.</param>
	/// <param name="sourceIndex">The starting index of transformation in the source array.</param>
	/// <param name="rotation">The <see cref="T:Microsoft.Xna.Framework.Quaternion" /> which contains rotation transformation.</param>
	/// <param name="destinationArray">Destination array.</param>
	/// <param name="destinationIndex">The starting index in the destination array, where the first <see cref="T:Microsoft.Xna.Framework.Vector2" /> should be written.</param>
	/// <param name="length">The number of vectors to be transformed.</param>
	public static void Transform(Vector2[] sourceArray, int sourceIndex, ref Quaternion rotation, Vector2[] destinationArray, int destinationIndex, int length)
	{
		if (sourceArray == null)
		{
			throw new ArgumentNullException("sourceArray");
		}
		if (destinationArray == null)
		{
			throw new ArgumentNullException("destinationArray");
		}
		if (sourceArray.Length < sourceIndex + length)
		{
			throw new ArgumentException("Source array length is lesser than sourceIndex + length");
		}
		if (destinationArray.Length < destinationIndex + length)
		{
			throw new ArgumentException("Destination array length is lesser than destinationIndex + length");
		}
		for (int x = 0; x < length; x++)
		{
			Vector2 position = sourceArray[sourceIndex + x];
			Vector2 destination = destinationArray[destinationIndex + x];
			Vector2.Transform(ref position, ref rotation, out var v);
			destination.X = v.X;
			destination.Y = v.Y;
			destinationArray[destinationIndex + x] = destination;
		}
	}

	/// <summary>
	/// Apply transformation on all vectors within array of <see cref="T:Microsoft.Xna.Framework.Vector2" /> by the specified <see cref="T:Microsoft.Xna.Framework.Matrix" /> and places the results in an another array.
	/// </summary>
	/// <param name="sourceArray">Source array.</param>
	/// <param name="matrix">The transformation <see cref="T:Microsoft.Xna.Framework.Matrix" />.</param>
	/// <param name="destinationArray">Destination array.</param>
	public static void Transform(Vector2[] sourceArray, ref Matrix matrix, Vector2[] destinationArray)
	{
		Vector2.Transform(sourceArray, 0, ref matrix, destinationArray, 0, sourceArray.Length);
	}

	/// <summary>
	/// Apply transformation on all vectors within array of <see cref="T:Microsoft.Xna.Framework.Vector2" /> by the specified <see cref="T:Microsoft.Xna.Framework.Quaternion" /> and places the results in an another array.
	/// </summary>
	/// <param name="sourceArray">Source array.</param>
	/// <param name="rotation">The <see cref="T:Microsoft.Xna.Framework.Quaternion" /> which contains rotation transformation.</param>
	/// <param name="destinationArray">Destination array.</param>
	public static void Transform(Vector2[] sourceArray, ref Quaternion rotation, Vector2[] destinationArray)
	{
		Vector2.Transform(sourceArray, 0, ref rotation, destinationArray, 0, sourceArray.Length);
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector2" /> that contains a transformation of the specified normal by the specified <see cref="T:Microsoft.Xna.Framework.Matrix" />.
	/// </summary>
	/// <param name="normal">Source <see cref="T:Microsoft.Xna.Framework.Vector2" /> which represents a normal vector.</param>
	/// <param name="matrix">The transformation <see cref="T:Microsoft.Xna.Framework.Matrix" />.</param>
	/// <returns>Transformed normal.</returns>
	public static Vector2 TransformNormal(Vector2 normal, Matrix matrix)
	{
		return new Vector2(normal.X * matrix.M11 + normal.Y * matrix.M21, normal.X * matrix.M12 + normal.Y * matrix.M22);
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector2" /> that contains a transformation of the specified normal by the specified <see cref="T:Microsoft.Xna.Framework.Matrix" />.
	/// </summary>
	/// <param name="normal">Source <see cref="T:Microsoft.Xna.Framework.Vector2" /> which represents a normal vector.</param>
	/// <param name="matrix">The transformation <see cref="T:Microsoft.Xna.Framework.Matrix" />.</param>
	/// <param name="result">Transformed normal as an output parameter.</param>
	public static void TransformNormal(ref Vector2 normal, ref Matrix matrix, out Vector2 result)
	{
		float x = normal.X * matrix.M11 + normal.Y * matrix.M21;
		float y = normal.X * matrix.M12 + normal.Y * matrix.M22;
		result.X = x;
		result.Y = y;
	}

	/// <summary>
	/// Apply transformation on normals within array of <see cref="T:Microsoft.Xna.Framework.Vector2" /> by the specified <see cref="T:Microsoft.Xna.Framework.Matrix" /> and places the results in an another array.
	/// </summary>
	/// <param name="sourceArray">Source array.</param>
	/// <param name="sourceIndex">The starting index of transformation in the source array.</param>
	/// <param name="matrix">The transformation <see cref="T:Microsoft.Xna.Framework.Matrix" />.</param>
	/// <param name="destinationArray">Destination array.</param>
	/// <param name="destinationIndex">The starting index in the destination array, where the first <see cref="T:Microsoft.Xna.Framework.Vector2" /> should be written.</param>
	/// <param name="length">The number of normals to be transformed.</param>
	public static void TransformNormal(Vector2[] sourceArray, int sourceIndex, ref Matrix matrix, Vector2[] destinationArray, int destinationIndex, int length)
	{
		if (sourceArray == null)
		{
			throw new ArgumentNullException("sourceArray");
		}
		if (destinationArray == null)
		{
			throw new ArgumentNullException("destinationArray");
		}
		if (sourceArray.Length < sourceIndex + length)
		{
			throw new ArgumentException("Source array length is lesser than sourceIndex + length");
		}
		if (destinationArray.Length < destinationIndex + length)
		{
			throw new ArgumentException("Destination array length is lesser than destinationIndex + length");
		}
		for (int i = 0; i < length; i++)
		{
			Vector2 normal = sourceArray[sourceIndex + i];
			destinationArray[destinationIndex + i] = new Vector2(normal.X * matrix.M11 + normal.Y * matrix.M21, normal.X * matrix.M12 + normal.Y * matrix.M22);
		}
	}

	/// <summary>
	/// Apply transformation on all normals within array of <see cref="T:Microsoft.Xna.Framework.Vector2" /> by the specified <see cref="T:Microsoft.Xna.Framework.Matrix" /> and places the results in an another array.
	/// </summary>
	/// <param name="sourceArray">Source array.</param>
	/// <param name="matrix">The transformation <see cref="T:Microsoft.Xna.Framework.Matrix" />.</param>
	/// <param name="destinationArray">Destination array.</param>
	public static void TransformNormal(Vector2[] sourceArray, ref Matrix matrix, Vector2[] destinationArray)
	{
		if (sourceArray == null)
		{
			throw new ArgumentNullException("sourceArray");
		}
		if (destinationArray == null)
		{
			throw new ArgumentNullException("destinationArray");
		}
		if (destinationArray.Length < sourceArray.Length)
		{
			throw new ArgumentException("Destination array length is lesser than source array length");
		}
		for (int i = 0; i < sourceArray.Length; i++)
		{
			Vector2 normal = sourceArray[i];
			destinationArray[i] = new Vector2(normal.X * matrix.M11 + normal.Y * matrix.M21, normal.X * matrix.M12 + normal.Y * matrix.M22);
		}
	}

	/// <summary>
	/// Deconstruction method for <see cref="T:Microsoft.Xna.Framework.Vector2" />.
	/// </summary>
	/// <param name="x"></param>
	/// <param name="y"></param>
	public void Deconstruct(out float x, out float y)
	{
		x = this.X;
		y = this.Y;
	}
}
