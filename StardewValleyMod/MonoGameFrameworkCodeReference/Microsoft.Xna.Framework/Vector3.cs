using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Text;
using Microsoft.Xna.Framework.Design;

namespace Microsoft.Xna.Framework;

/// <summary>
/// Describes a 3D-vector.
/// </summary>
[TypeConverter(typeof(Vector3TypeConverter))]
[DataContract]
[DebuggerDisplay("{DebugDisplayString,nq}")]
public struct Vector3 : IEquatable<Vector3>
{
	private static readonly Vector3 zero = new Vector3(0f, 0f, 0f);

	private static readonly Vector3 one = new Vector3(1f, 1f, 1f);

	private static readonly Vector3 unitX = new Vector3(1f, 0f, 0f);

	private static readonly Vector3 unitY = new Vector3(0f, 1f, 0f);

	private static readonly Vector3 unitZ = new Vector3(0f, 0f, 1f);

	private static readonly Vector3 up = new Vector3(0f, 1f, 0f);

	private static readonly Vector3 down = new Vector3(0f, -1f, 0f);

	private static readonly Vector3 right = new Vector3(1f, 0f, 0f);

	private static readonly Vector3 left = new Vector3(-1f, 0f, 0f);

	private static readonly Vector3 forward = new Vector3(0f, 0f, -1f);

	private static readonly Vector3 backward = new Vector3(0f, 0f, 1f);

	/// <summary>
	/// The x coordinate of this <see cref="T:Microsoft.Xna.Framework.Vector3" />.
	/// </summary>
	[DataMember]
	public float X;

	/// <summary>
	/// The y coordinate of this <see cref="T:Microsoft.Xna.Framework.Vector3" />.
	/// </summary>
	[DataMember]
	public float Y;

	/// <summary>
	/// The z coordinate of this <see cref="T:Microsoft.Xna.Framework.Vector3" />.
	/// </summary>
	[DataMember]
	public float Z;

	/// <summary>
	/// Returns a <see cref="T:Microsoft.Xna.Framework.Vector3" /> with components 0, 0, 0.
	/// </summary>
	public static Vector3 Zero => Vector3.zero;

	/// <summary>
	/// Returns a <see cref="T:Microsoft.Xna.Framework.Vector3" /> with components 1, 1, 1.
	/// </summary>
	public static Vector3 One => Vector3.one;

	/// <summary>
	/// Returns a <see cref="T:Microsoft.Xna.Framework.Vector3" /> with components 1, 0, 0.
	/// </summary>
	public static Vector3 UnitX => Vector3.unitX;

	/// <summary>
	/// Returns a <see cref="T:Microsoft.Xna.Framework.Vector3" /> with components 0, 1, 0.
	/// </summary>
	public static Vector3 UnitY => Vector3.unitY;

	/// <summary>
	/// Returns a <see cref="T:Microsoft.Xna.Framework.Vector3" /> with components 0, 0, 1.
	/// </summary>
	public static Vector3 UnitZ => Vector3.unitZ;

	/// <summary>
	/// Returns a <see cref="T:Microsoft.Xna.Framework.Vector3" /> with components 0, 1, 0.
	/// </summary>
	public static Vector3 Up => Vector3.up;

	/// <summary>
	/// Returns a <see cref="T:Microsoft.Xna.Framework.Vector3" /> with components 0, -1, 0.
	/// </summary>
	public static Vector3 Down => Vector3.down;

	/// <summary>
	/// Returns a <see cref="T:Microsoft.Xna.Framework.Vector3" /> with components 1, 0, 0.
	/// </summary>
	public static Vector3 Right => Vector3.right;

	/// <summary>
	/// Returns a <see cref="T:Microsoft.Xna.Framework.Vector3" /> with components -1, 0, 0.
	/// </summary>
	public static Vector3 Left => Vector3.left;

	/// <summary>
	/// Returns a <see cref="T:Microsoft.Xna.Framework.Vector3" /> with components 0, 0, -1.
	/// </summary>
	public static Vector3 Forward => Vector3.forward;

	/// <summary>
	/// Returns a <see cref="T:Microsoft.Xna.Framework.Vector3" /> with components 0, 0, 1.
	/// </summary>
	public static Vector3 Backward => Vector3.backward;

	internal string DebugDisplayString => this.X + "  " + this.Y + "  " + this.Z;

	/// <summary>
	/// Constructs a 3d vector with X, Y and Z from three values.
	/// </summary>
	/// <param name="x">The x coordinate in 3d-space.</param>
	/// <param name="y">The y coordinate in 3d-space.</param>
	/// <param name="z">The z coordinate in 3d-space.</param>
	public Vector3(float x, float y, float z)
	{
		this.X = x;
		this.Y = y;
		this.Z = z;
	}

	/// <summary>
	/// Constructs a 3d vector with X, Y and Z set to the same value.
	/// </summary>
	/// <param name="value">The x, y and z coordinates in 3d-space.</param>
	public Vector3(float value)
	{
		this.X = value;
		this.Y = value;
		this.Z = value;
	}

	/// <summary>
	/// Constructs a 3d vector with X, Y from <see cref="T:Microsoft.Xna.Framework.Vector2" /> and Z from a scalar.
	/// </summary>
	/// <param name="value">The x and y coordinates in 3d-space.</param>
	/// <param name="z">The z coordinate in 3d-space.</param>
	public Vector3(Vector2 value, float z)
	{
		this.X = value.X;
		this.Y = value.Y;
		this.Z = z;
	}

	/// <summary>
	/// Performs vector addition on <paramref name="value1" /> and <paramref name="value2" />.
	/// </summary>
	/// <param name="value1">The first vector to add.</param>
	/// <param name="value2">The second vector to add.</param>
	/// <returns>The result of the vector addition.</returns>
	public static Vector3 Add(Vector3 value1, Vector3 value2)
	{
		value1.X += value2.X;
		value1.Y += value2.Y;
		value1.Z += value2.Z;
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
	public static void Add(ref Vector3 value1, ref Vector3 value2, out Vector3 result)
	{
		result.X = value1.X + value2.X;
		result.Y = value1.Y + value2.Y;
		result.Z = value1.Z + value2.Z;
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector3" /> that contains the cartesian coordinates of a vector specified in barycentric coordinates and relative to 3d-triangle.
	/// </summary>
	/// <param name="value1">The first vector of 3d-triangle.</param>
	/// <param name="value2">The second vector of 3d-triangle.</param>
	/// <param name="value3">The third vector of 3d-triangle.</param>
	/// <param name="amount1">Barycentric scalar <c>b2</c> which represents a weighting factor towards second vector of 3d-triangle.</param>
	/// <param name="amount2">Barycentric scalar <c>b3</c> which represents a weighting factor towards third vector of 3d-triangle.</param>
	/// <returns>The cartesian translation of barycentric coordinates.</returns>
	public static Vector3 Barycentric(Vector3 value1, Vector3 value2, Vector3 value3, float amount1, float amount2)
	{
		return new Vector3(MathHelper.Barycentric(value1.X, value2.X, value3.X, amount1, amount2), MathHelper.Barycentric(value1.Y, value2.Y, value3.Y, amount1, amount2), MathHelper.Barycentric(value1.Z, value2.Z, value3.Z, amount1, amount2));
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector3" /> that contains the cartesian coordinates of a vector specified in barycentric coordinates and relative to 3d-triangle.
	/// </summary>
	/// <param name="value1">The first vector of 3d-triangle.</param>
	/// <param name="value2">The second vector of 3d-triangle.</param>
	/// <param name="value3">The third vector of 3d-triangle.</param>
	/// <param name="amount1">Barycentric scalar <c>b2</c> which represents a weighting factor towards second vector of 3d-triangle.</param>
	/// <param name="amount2">Barycentric scalar <c>b3</c> which represents a weighting factor towards third vector of 3d-triangle.</param>
	/// <param name="result">The cartesian translation of barycentric coordinates as an output parameter.</param>
	public static void Barycentric(ref Vector3 value1, ref Vector3 value2, ref Vector3 value3, float amount1, float amount2, out Vector3 result)
	{
		result.X = MathHelper.Barycentric(value1.X, value2.X, value3.X, amount1, amount2);
		result.Y = MathHelper.Barycentric(value1.Y, value2.Y, value3.Y, amount1, amount2);
		result.Z = MathHelper.Barycentric(value1.Z, value2.Z, value3.Z, amount1, amount2);
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector3" /> that contains CatmullRom interpolation of the specified vectors.
	/// </summary>
	/// <param name="value1">The first vector in interpolation.</param>
	/// <param name="value2">The second vector in interpolation.</param>
	/// <param name="value3">The third vector in interpolation.</param>
	/// <param name="value4">The fourth vector in interpolation.</param>
	/// <param name="amount">Weighting factor.</param>
	/// <returns>The result of CatmullRom interpolation.</returns>
	public static Vector3 CatmullRom(Vector3 value1, Vector3 value2, Vector3 value3, Vector3 value4, float amount)
	{
		return new Vector3(MathHelper.CatmullRom(value1.X, value2.X, value3.X, value4.X, amount), MathHelper.CatmullRom(value1.Y, value2.Y, value3.Y, value4.Y, amount), MathHelper.CatmullRom(value1.Z, value2.Z, value3.Z, value4.Z, amount));
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector3" /> that contains CatmullRom interpolation of the specified vectors.
	/// </summary>
	/// <param name="value1">The first vector in interpolation.</param>
	/// <param name="value2">The second vector in interpolation.</param>
	/// <param name="value3">The third vector in interpolation.</param>
	/// <param name="value4">The fourth vector in interpolation.</param>
	/// <param name="amount">Weighting factor.</param>
	/// <param name="result">The result of CatmullRom interpolation as an output parameter.</param>
	public static void CatmullRom(ref Vector3 value1, ref Vector3 value2, ref Vector3 value3, ref Vector3 value4, float amount, out Vector3 result)
	{
		result.X = MathHelper.CatmullRom(value1.X, value2.X, value3.X, value4.X, amount);
		result.Y = MathHelper.CatmullRom(value1.Y, value2.Y, value3.Y, value4.Y, amount);
		result.Z = MathHelper.CatmullRom(value1.Z, value2.Z, value3.Z, value4.Z, amount);
	}

	/// <summary>
	/// Round the members of this <see cref="T:Microsoft.Xna.Framework.Vector3" /> towards positive infinity.
	/// </summary>
	public void Ceiling()
	{
		this.X = (float)Math.Ceiling(this.X);
		this.Y = (float)Math.Ceiling(this.Y);
		this.Z = (float)Math.Ceiling(this.Z);
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector3" /> that contains members from another vector rounded towards positive infinity.
	/// </summary>
	/// <param name="value">Source <see cref="T:Microsoft.Xna.Framework.Vector3" />.</param>
	/// <returns>The rounded <see cref="T:Microsoft.Xna.Framework.Vector3" />.</returns>
	public static Vector3 Ceiling(Vector3 value)
	{
		value.X = (float)Math.Ceiling(value.X);
		value.Y = (float)Math.Ceiling(value.Y);
		value.Z = (float)Math.Ceiling(value.Z);
		return value;
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector3" /> that contains members from another vector rounded towards positive infinity.
	/// </summary>
	/// <param name="value">Source <see cref="T:Microsoft.Xna.Framework.Vector3" />.</param>
	/// <param name="result">The rounded <see cref="T:Microsoft.Xna.Framework.Vector3" />.</param>
	public static void Ceiling(ref Vector3 value, out Vector3 result)
	{
		result.X = (float)Math.Ceiling(value.X);
		result.Y = (float)Math.Ceiling(value.Y);
		result.Z = (float)Math.Ceiling(value.Z);
	}

	/// <summary>
	/// Clamps the specified value within a range.
	/// </summary>
	/// <param name="value1">The value to clamp.</param>
	/// <param name="min">The min value.</param>
	/// <param name="max">The max value.</param>
	/// <returns>The clamped value.</returns>
	public static Vector3 Clamp(Vector3 value1, Vector3 min, Vector3 max)
	{
		return new Vector3(MathHelper.Clamp(value1.X, min.X, max.X), MathHelper.Clamp(value1.Y, min.Y, max.Y), MathHelper.Clamp(value1.Z, min.Z, max.Z));
	}

	/// <summary>
	/// Clamps the specified value within a range.
	/// </summary>
	/// <param name="value1">The value to clamp.</param>
	/// <param name="min">The min value.</param>
	/// <param name="max">The max value.</param>
	/// <param name="result">The clamped value as an output parameter.</param>
	public static void Clamp(ref Vector3 value1, ref Vector3 min, ref Vector3 max, out Vector3 result)
	{
		result.X = MathHelper.Clamp(value1.X, min.X, max.X);
		result.Y = MathHelper.Clamp(value1.Y, min.Y, max.Y);
		result.Z = MathHelper.Clamp(value1.Z, min.Z, max.Z);
	}

	/// <summary>
	/// Computes the cross product of two vectors.
	/// </summary>
	/// <param name="vector1">The first vector.</param>
	/// <param name="vector2">The second vector.</param>
	/// <returns>The cross product of two vectors.</returns>
	public static Vector3 Cross(Vector3 vector1, Vector3 vector2)
	{
		Vector3.Cross(ref vector1, ref vector2, out vector1);
		return vector1;
	}

	/// <summary>
	/// Computes the cross product of two vectors.
	/// </summary>
	/// <param name="vector1">The first vector.</param>
	/// <param name="vector2">The second vector.</param>
	/// <param name="result">The cross product of two vectors as an output parameter.</param>
	public static void Cross(ref Vector3 vector1, ref Vector3 vector2, out Vector3 result)
	{
		float x = vector1.Y * vector2.Z - vector2.Y * vector1.Z;
		float y = 0f - (vector1.X * vector2.Z - vector2.X * vector1.Z);
		float z = vector1.X * vector2.Y - vector2.X * vector1.Y;
		result.X = x;
		result.Y = y;
		result.Z = z;
	}

	/// <summary>
	/// Returns the distance between two vectors.
	/// </summary>
	/// <param name="value1">The first vector.</param>
	/// <param name="value2">The second vector.</param>
	/// <returns>The distance between two vectors.</returns>
	public static float Distance(Vector3 value1, Vector3 value2)
	{
		Vector3.DistanceSquared(ref value1, ref value2, out var result);
		return (float)Math.Sqrt(result);
	}

	/// <summary>
	/// Returns the distance between two vectors.
	/// </summary>
	/// <param name="value1">The first vector.</param>
	/// <param name="value2">The second vector.</param>
	/// <param name="result">The distance between two vectors as an output parameter.</param>
	public static void Distance(ref Vector3 value1, ref Vector3 value2, out float result)
	{
		Vector3.DistanceSquared(ref value1, ref value2, out result);
		result = (float)Math.Sqrt(result);
	}

	/// <summary>
	/// Returns the squared distance between two vectors.
	/// </summary>
	/// <param name="value1">The first vector.</param>
	/// <param name="value2">The second vector.</param>
	/// <returns>The squared distance between two vectors.</returns>
	public static float DistanceSquared(Vector3 value1, Vector3 value2)
	{
		return (value1.X - value2.X) * (value1.X - value2.X) + (value1.Y - value2.Y) * (value1.Y - value2.Y) + (value1.Z - value2.Z) * (value1.Z - value2.Z);
	}

	/// <summary>
	/// Returns the squared distance between two vectors.
	/// </summary>
	/// <param name="value1">The first vector.</param>
	/// <param name="value2">The second vector.</param>
	/// <param name="result">The squared distance between two vectors as an output parameter.</param>
	public static void DistanceSquared(ref Vector3 value1, ref Vector3 value2, out float result)
	{
		result = (value1.X - value2.X) * (value1.X - value2.X) + (value1.Y - value2.Y) * (value1.Y - value2.Y) + (value1.Z - value2.Z) * (value1.Z - value2.Z);
	}

	/// <summary>
	/// Divides the components of a <see cref="T:Microsoft.Xna.Framework.Vector3" /> by the components of another <see cref="T:Microsoft.Xna.Framework.Vector3" />.
	/// </summary>
	/// <param name="value1">Source <see cref="T:Microsoft.Xna.Framework.Vector3" />.</param>
	/// <param name="value2">Divisor <see cref="T:Microsoft.Xna.Framework.Vector3" />.</param>
	/// <returns>The result of dividing the vectors.</returns>
	public static Vector3 Divide(Vector3 value1, Vector3 value2)
	{
		value1.X /= value2.X;
		value1.Y /= value2.Y;
		value1.Z /= value2.Z;
		return value1;
	}

	/// <summary>
	/// Divides the components of a <see cref="T:Microsoft.Xna.Framework.Vector3" /> by a scalar.
	/// </summary>
	/// <param name="value1">Source <see cref="T:Microsoft.Xna.Framework.Vector3" />.</param>
	/// <param name="divider">Divisor scalar.</param>
	/// <returns>The result of dividing a vector by a scalar.</returns>
	public static Vector3 Divide(Vector3 value1, float divider)
	{
		float factor = 1f / divider;
		value1.X *= factor;
		value1.Y *= factor;
		value1.Z *= factor;
		return value1;
	}

	/// <summary>
	/// Divides the components of a <see cref="T:Microsoft.Xna.Framework.Vector3" /> by a scalar.
	/// </summary>
	/// <param name="value1">Source <see cref="T:Microsoft.Xna.Framework.Vector3" />.</param>
	/// <param name="divider">Divisor scalar.</param>
	/// <param name="result">The result of dividing a vector by a scalar as an output parameter.</param>
	public static void Divide(ref Vector3 value1, float divider, out Vector3 result)
	{
		float factor = 1f / divider;
		result.X = value1.X * factor;
		result.Y = value1.Y * factor;
		result.Z = value1.Z * factor;
	}

	/// <summary>
	/// Divides the components of a <see cref="T:Microsoft.Xna.Framework.Vector3" /> by the components of another <see cref="T:Microsoft.Xna.Framework.Vector3" />.
	/// </summary>
	/// <param name="value1">Source <see cref="T:Microsoft.Xna.Framework.Vector3" />.</param>
	/// <param name="value2">Divisor <see cref="T:Microsoft.Xna.Framework.Vector3" />.</param>
	/// <param name="result">The result of dividing the vectors as an output parameter.</param>
	public static void Divide(ref Vector3 value1, ref Vector3 value2, out Vector3 result)
	{
		result.X = value1.X / value2.X;
		result.Y = value1.Y / value2.Y;
		result.Z = value1.Z / value2.Z;
	}

	/// <summary>
	/// Returns a dot product of two vectors.
	/// </summary>
	/// <param name="value1">The first vector.</param>
	/// <param name="value2">The second vector.</param>
	/// <returns>The dot product of two vectors.</returns>
	public static float Dot(Vector3 value1, Vector3 value2)
	{
		return value1.X * value2.X + value1.Y * value2.Y + value1.Z * value2.Z;
	}

	/// <summary>
	/// Returns a dot product of two vectors.
	/// </summary>
	/// <param name="value1">The first vector.</param>
	/// <param name="value2">The second vector.</param>
	/// <param name="result">The dot product of two vectors as an output parameter.</param>
	public static void Dot(ref Vector3 value1, ref Vector3 value2, out float result)
	{
		result = value1.X * value2.X + value1.Y * value2.Y + value1.Z * value2.Z;
	}

	/// <summary>
	/// Compares whether current instance is equal to specified <see cref="T:System.Object" />.
	/// </summary>
	/// <param name="obj">The <see cref="T:System.Object" /> to compare.</param>
	/// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
	public override bool Equals(object obj)
	{
		if (!(obj is Vector3 other))
		{
			return false;
		}
		if (this.X == other.X && this.Y == other.Y)
		{
			return this.Z == other.Z;
		}
		return false;
	}

	/// <summary>
	/// Compares whether current instance is equal to specified <see cref="T:Microsoft.Xna.Framework.Vector3" />.
	/// </summary>
	/// <param name="other">The <see cref="T:Microsoft.Xna.Framework.Vector3" /> to compare.</param>
	/// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
	public bool Equals(Vector3 other)
	{
		if (this.X == other.X && this.Y == other.Y)
		{
			return this.Z == other.Z;
		}
		return false;
	}

	/// <summary>
	/// Round the members of this <see cref="T:Microsoft.Xna.Framework.Vector3" /> towards negative infinity.
	/// </summary>
	public void Floor()
	{
		this.X = (float)Math.Floor(this.X);
		this.Y = (float)Math.Floor(this.Y);
		this.Z = (float)Math.Floor(this.Z);
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector3" /> that contains members from another vector rounded towards negative infinity.
	/// </summary>
	/// <param name="value">Source <see cref="T:Microsoft.Xna.Framework.Vector3" />.</param>
	/// <returns>The rounded <see cref="T:Microsoft.Xna.Framework.Vector3" />.</returns>
	public static Vector3 Floor(Vector3 value)
	{
		value.X = (float)Math.Floor(value.X);
		value.Y = (float)Math.Floor(value.Y);
		value.Z = (float)Math.Floor(value.Z);
		return value;
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector3" /> that contains members from another vector rounded towards negative infinity.
	/// </summary>
	/// <param name="value">Source <see cref="T:Microsoft.Xna.Framework.Vector3" />.</param>
	/// <param name="result">The rounded <see cref="T:Microsoft.Xna.Framework.Vector3" />.</param>
	public static void Floor(ref Vector3 value, out Vector3 result)
	{
		result.X = (float)Math.Floor(value.X);
		result.Y = (float)Math.Floor(value.Y);
		result.Z = (float)Math.Floor(value.Z);
	}

	/// <summary>
	/// Gets the hash code of this <see cref="T:Microsoft.Xna.Framework.Vector3" />.
	/// </summary>
	/// <returns>Hash code of this <see cref="T:Microsoft.Xna.Framework.Vector3" />.</returns>
	public override int GetHashCode()
	{
		return (((this.X.GetHashCode() * 397) ^ this.Y.GetHashCode()) * 397) ^ this.Z.GetHashCode();
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector3" /> that contains hermite spline interpolation.
	/// </summary>
	/// <param name="value1">The first position vector.</param>
	/// <param name="tangent1">The first tangent vector.</param>
	/// <param name="value2">The second position vector.</param>
	/// <param name="tangent2">The second tangent vector.</param>
	/// <param name="amount">Weighting factor.</param>
	/// <returns>The hermite spline interpolation vector.</returns>
	public static Vector3 Hermite(Vector3 value1, Vector3 tangent1, Vector3 value2, Vector3 tangent2, float amount)
	{
		return new Vector3(MathHelper.Hermite(value1.X, tangent1.X, value2.X, tangent2.X, amount), MathHelper.Hermite(value1.Y, tangent1.Y, value2.Y, tangent2.Y, amount), MathHelper.Hermite(value1.Z, tangent1.Z, value2.Z, tangent2.Z, amount));
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector3" /> that contains hermite spline interpolation.
	/// </summary>
	/// <param name="value1">The first position vector.</param>
	/// <param name="tangent1">The first tangent vector.</param>
	/// <param name="value2">The second position vector.</param>
	/// <param name="tangent2">The second tangent vector.</param>
	/// <param name="amount">Weighting factor.</param>
	/// <param name="result">The hermite spline interpolation vector as an output parameter.</param>
	public static void Hermite(ref Vector3 value1, ref Vector3 tangent1, ref Vector3 value2, ref Vector3 tangent2, float amount, out Vector3 result)
	{
		result.X = MathHelper.Hermite(value1.X, tangent1.X, value2.X, tangent2.X, amount);
		result.Y = MathHelper.Hermite(value1.Y, tangent1.Y, value2.Y, tangent2.Y, amount);
		result.Z = MathHelper.Hermite(value1.Z, tangent1.Z, value2.Z, tangent2.Z, amount);
	}

	/// <summary>
	/// Returns the length of this <see cref="T:Microsoft.Xna.Framework.Vector3" />.
	/// </summary>
	/// <returns>The length of this <see cref="T:Microsoft.Xna.Framework.Vector3" />.</returns>
	public float Length()
	{
		return (float)Math.Sqrt(this.X * this.X + this.Y * this.Y + this.Z * this.Z);
	}

	/// <summary>
	/// Returns the squared length of this <see cref="T:Microsoft.Xna.Framework.Vector3" />.
	/// </summary>
	/// <returns>The squared length of this <see cref="T:Microsoft.Xna.Framework.Vector3" />.</returns>
	public float LengthSquared()
	{
		return this.X * this.X + this.Y * this.Y + this.Z * this.Z;
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector3" /> that contains linear interpolation of the specified vectors.
	/// </summary>
	/// <param name="value1">The first vector.</param>
	/// <param name="value2">The second vector.</param>
	/// <param name="amount">Weighting value(between 0.0 and 1.0).</param>
	/// <returns>The result of linear interpolation of the specified vectors.</returns>
	public static Vector3 Lerp(Vector3 value1, Vector3 value2, float amount)
	{
		return new Vector3(MathHelper.Lerp(value1.X, value2.X, amount), MathHelper.Lerp(value1.Y, value2.Y, amount), MathHelper.Lerp(value1.Z, value2.Z, amount));
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector3" /> that contains linear interpolation of the specified vectors.
	/// </summary>
	/// <param name="value1">The first vector.</param>
	/// <param name="value2">The second vector.</param>
	/// <param name="amount">Weighting value(between 0.0 and 1.0).</param>
	/// <param name="result">The result of linear interpolation of the specified vectors as an output parameter.</param>
	public static void Lerp(ref Vector3 value1, ref Vector3 value2, float amount, out Vector3 result)
	{
		result.X = MathHelper.Lerp(value1.X, value2.X, amount);
		result.Y = MathHelper.Lerp(value1.Y, value2.Y, amount);
		result.Z = MathHelper.Lerp(value1.Z, value2.Z, amount);
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector3" /> that contains linear interpolation of the specified vectors.
	/// Uses <see cref="M:Microsoft.Xna.Framework.MathHelper.LerpPrecise(System.Single,System.Single,System.Single)" /> on MathHelper for the interpolation.
	/// Less efficient but more precise compared to <see cref="M:Microsoft.Xna.Framework.Vector3.Lerp(Microsoft.Xna.Framework.Vector3,Microsoft.Xna.Framework.Vector3,System.Single)" />.
	/// See remarks section of <see cref="M:Microsoft.Xna.Framework.MathHelper.LerpPrecise(System.Single,System.Single,System.Single)" /> on MathHelper for more info.
	/// </summary>
	/// <param name="value1">The first vector.</param>
	/// <param name="value2">The second vector.</param>
	/// <param name="amount">Weighting value(between 0.0 and 1.0).</param>
	/// <returns>The result of linear interpolation of the specified vectors.</returns>
	public static Vector3 LerpPrecise(Vector3 value1, Vector3 value2, float amount)
	{
		return new Vector3(MathHelper.LerpPrecise(value1.X, value2.X, amount), MathHelper.LerpPrecise(value1.Y, value2.Y, amount), MathHelper.LerpPrecise(value1.Z, value2.Z, amount));
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector3" /> that contains linear interpolation of the specified vectors.
	/// Uses <see cref="M:Microsoft.Xna.Framework.MathHelper.LerpPrecise(System.Single,System.Single,System.Single)" /> on MathHelper for the interpolation.
	/// Less efficient but more precise compared to <see cref="M:Microsoft.Xna.Framework.Vector3.Lerp(Microsoft.Xna.Framework.Vector3@,Microsoft.Xna.Framework.Vector3@,System.Single,Microsoft.Xna.Framework.Vector3@)" />.
	/// See remarks section of <see cref="M:Microsoft.Xna.Framework.MathHelper.LerpPrecise(System.Single,System.Single,System.Single)" /> on MathHelper for more info.
	/// </summary>
	/// <param name="value1">The first vector.</param>
	/// <param name="value2">The second vector.</param>
	/// <param name="amount">Weighting value(between 0.0 and 1.0).</param>
	/// <param name="result">The result of linear interpolation of the specified vectors as an output parameter.</param>
	public static void LerpPrecise(ref Vector3 value1, ref Vector3 value2, float amount, out Vector3 result)
	{
		result.X = MathHelper.LerpPrecise(value1.X, value2.X, amount);
		result.Y = MathHelper.LerpPrecise(value1.Y, value2.Y, amount);
		result.Z = MathHelper.LerpPrecise(value1.Z, value2.Z, amount);
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector3" /> that contains a maximal values from the two vectors.
	/// </summary>
	/// <param name="value1">The first vector.</param>
	/// <param name="value2">The second vector.</param>
	/// <returns>The <see cref="T:Microsoft.Xna.Framework.Vector3" /> with maximal values from the two vectors.</returns>
	public static Vector3 Max(Vector3 value1, Vector3 value2)
	{
		return new Vector3(MathHelper.Max(value1.X, value2.X), MathHelper.Max(value1.Y, value2.Y), MathHelper.Max(value1.Z, value2.Z));
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector3" /> that contains a maximal values from the two vectors.
	/// </summary>
	/// <param name="value1">The first vector.</param>
	/// <param name="value2">The second vector.</param>
	/// <param name="result">The <see cref="T:Microsoft.Xna.Framework.Vector3" /> with maximal values from the two vectors as an output parameter.</param>
	public static void Max(ref Vector3 value1, ref Vector3 value2, out Vector3 result)
	{
		result.X = MathHelper.Max(value1.X, value2.X);
		result.Y = MathHelper.Max(value1.Y, value2.Y);
		result.Z = MathHelper.Max(value1.Z, value2.Z);
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector3" /> that contains a minimal values from the two vectors.
	/// </summary>
	/// <param name="value1">The first vector.</param>
	/// <param name="value2">The second vector.</param>
	/// <returns>The <see cref="T:Microsoft.Xna.Framework.Vector3" /> with minimal values from the two vectors.</returns>
	public static Vector3 Min(Vector3 value1, Vector3 value2)
	{
		return new Vector3(MathHelper.Min(value1.X, value2.X), MathHelper.Min(value1.Y, value2.Y), MathHelper.Min(value1.Z, value2.Z));
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector3" /> that contains a minimal values from the two vectors.
	/// </summary>
	/// <param name="value1">The first vector.</param>
	/// <param name="value2">The second vector.</param>
	/// <param name="result">The <see cref="T:Microsoft.Xna.Framework.Vector3" /> with minimal values from the two vectors as an output parameter.</param>
	public static void Min(ref Vector3 value1, ref Vector3 value2, out Vector3 result)
	{
		result.X = MathHelper.Min(value1.X, value2.X);
		result.Y = MathHelper.Min(value1.Y, value2.Y);
		result.Z = MathHelper.Min(value1.Z, value2.Z);
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector3" /> that contains a multiplication of two vectors.
	/// </summary>
	/// <param name="value1">Source <see cref="T:Microsoft.Xna.Framework.Vector3" />.</param>
	/// <param name="value2">Source <see cref="T:Microsoft.Xna.Framework.Vector3" />.</param>
	/// <returns>The result of the vector multiplication.</returns>
	public static Vector3 Multiply(Vector3 value1, Vector3 value2)
	{
		value1.X *= value2.X;
		value1.Y *= value2.Y;
		value1.Z *= value2.Z;
		return value1;
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector3" /> that contains a multiplication of <see cref="T:Microsoft.Xna.Framework.Vector3" /> and a scalar.
	/// </summary>
	/// <param name="value1">Source <see cref="T:Microsoft.Xna.Framework.Vector3" />.</param>
	/// <param name="scaleFactor">Scalar value.</param>
	/// <returns>The result of the vector multiplication with a scalar.</returns>
	public static Vector3 Multiply(Vector3 value1, float scaleFactor)
	{
		value1.X *= scaleFactor;
		value1.Y *= scaleFactor;
		value1.Z *= scaleFactor;
		return value1;
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector3" /> that contains a multiplication of <see cref="T:Microsoft.Xna.Framework.Vector3" /> and a scalar.
	/// </summary>
	/// <param name="value1">Source <see cref="T:Microsoft.Xna.Framework.Vector3" />.</param>
	/// <param name="scaleFactor">Scalar value.</param>
	/// <param name="result">The result of the multiplication with a scalar as an output parameter.</param>
	public static void Multiply(ref Vector3 value1, float scaleFactor, out Vector3 result)
	{
		result.X = value1.X * scaleFactor;
		result.Y = value1.Y * scaleFactor;
		result.Z = value1.Z * scaleFactor;
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector3" /> that contains a multiplication of two vectors.
	/// </summary>
	/// <param name="value1">Source <see cref="T:Microsoft.Xna.Framework.Vector3" />.</param>
	/// <param name="value2">Source <see cref="T:Microsoft.Xna.Framework.Vector3" />.</param>
	/// <param name="result">The result of the vector multiplication as an output parameter.</param>
	public static void Multiply(ref Vector3 value1, ref Vector3 value2, out Vector3 result)
	{
		result.X = value1.X * value2.X;
		result.Y = value1.Y * value2.Y;
		result.Z = value1.Z * value2.Z;
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector3" /> that contains the specified vector inversion.
	/// </summary>
	/// <param name="value">Source <see cref="T:Microsoft.Xna.Framework.Vector3" />.</param>
	/// <returns>The result of the vector inversion.</returns>
	public static Vector3 Negate(Vector3 value)
	{
		value = new Vector3(0f - value.X, 0f - value.Y, 0f - value.Z);
		return value;
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector3" /> that contains the specified vector inversion.
	/// </summary>
	/// <param name="value">Source <see cref="T:Microsoft.Xna.Framework.Vector3" />.</param>
	/// <param name="result">The result of the vector inversion as an output parameter.</param>
	public static void Negate(ref Vector3 value, out Vector3 result)
	{
		result.X = 0f - value.X;
		result.Y = 0f - value.Y;
		result.Z = 0f - value.Z;
	}

	/// <summary>
	/// Turns this <see cref="T:Microsoft.Xna.Framework.Vector3" /> to a unit vector with the same direction.
	/// </summary>
	public void Normalize()
	{
		float factor = (float)Math.Sqrt(this.X * this.X + this.Y * this.Y + this.Z * this.Z);
		factor = 1f / factor;
		this.X *= factor;
		this.Y *= factor;
		this.Z *= factor;
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector3" /> that contains a normalized values from another vector.
	/// </summary>
	/// <param name="value">Source <see cref="T:Microsoft.Xna.Framework.Vector3" />.</param>
	/// <returns>Unit vector.</returns>
	public static Vector3 Normalize(Vector3 value)
	{
		float factor = (float)Math.Sqrt(value.X * value.X + value.Y * value.Y + value.Z * value.Z);
		factor = 1f / factor;
		return new Vector3(value.X * factor, value.Y * factor, value.Z * factor);
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector3" /> that contains a normalized values from another vector.
	/// </summary>
	/// <param name="value">Source <see cref="T:Microsoft.Xna.Framework.Vector3" />.</param>
	/// <param name="result">Unit vector as an output parameter.</param>
	public static void Normalize(ref Vector3 value, out Vector3 result)
	{
		float factor = (float)Math.Sqrt(value.X * value.X + value.Y * value.Y + value.Z * value.Z);
		factor = 1f / factor;
		result.X = value.X * factor;
		result.Y = value.Y * factor;
		result.Z = value.Z * factor;
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector3" /> that contains reflect vector of the given vector and normal.
	/// </summary>
	/// <param name="vector">Source <see cref="T:Microsoft.Xna.Framework.Vector3" />.</param>
	/// <param name="normal">Reflection normal.</param>
	/// <returns>Reflected vector.</returns>
	public static Vector3 Reflect(Vector3 vector, Vector3 normal)
	{
		float dotProduct = vector.X * normal.X + vector.Y * normal.Y + vector.Z * normal.Z;
		Vector3 reflectedVector = default(Vector3);
		reflectedVector.X = vector.X - 2f * normal.X * dotProduct;
		reflectedVector.Y = vector.Y - 2f * normal.Y * dotProduct;
		reflectedVector.Z = vector.Z - 2f * normal.Z * dotProduct;
		return reflectedVector;
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector3" /> that contains reflect vector of the given vector and normal.
	/// </summary>
	/// <param name="vector">Source <see cref="T:Microsoft.Xna.Framework.Vector3" />.</param>
	/// <param name="normal">Reflection normal.</param>
	/// <param name="result">Reflected vector as an output parameter.</param>
	public static void Reflect(ref Vector3 vector, ref Vector3 normal, out Vector3 result)
	{
		float dotProduct = vector.X * normal.X + vector.Y * normal.Y + vector.Z * normal.Z;
		result.X = vector.X - 2f * normal.X * dotProduct;
		result.Y = vector.Y - 2f * normal.Y * dotProduct;
		result.Z = vector.Z - 2f * normal.Z * dotProduct;
	}

	/// <summary>
	/// Round the members of this <see cref="T:Microsoft.Xna.Framework.Vector3" /> towards the nearest integer value.
	/// </summary>
	public void Round()
	{
		this.X = (float)Math.Round(this.X);
		this.Y = (float)Math.Round(this.Y);
		this.Z = (float)Math.Round(this.Z);
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector3" /> that contains members from another vector rounded to the nearest integer value.
	/// </summary>
	/// <param name="value">Source <see cref="T:Microsoft.Xna.Framework.Vector3" />.</param>
	/// <returns>The rounded <see cref="T:Microsoft.Xna.Framework.Vector3" />.</returns>
	public static Vector3 Round(Vector3 value)
	{
		value.X = (float)Math.Round(value.X);
		value.Y = (float)Math.Round(value.Y);
		value.Z = (float)Math.Round(value.Z);
		return value;
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector3" /> that contains members from another vector rounded to the nearest integer value.
	/// </summary>
	/// <param name="value">Source <see cref="T:Microsoft.Xna.Framework.Vector3" />.</param>
	/// <param name="result">The rounded <see cref="T:Microsoft.Xna.Framework.Vector3" />.</param>
	public static void Round(ref Vector3 value, out Vector3 result)
	{
		result.X = (float)Math.Round(value.X);
		result.Y = (float)Math.Round(value.Y);
		result.Z = (float)Math.Round(value.Z);
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector3" /> that contains cubic interpolation of the specified vectors.
	/// </summary>
	/// <param name="value1">Source <see cref="T:Microsoft.Xna.Framework.Vector3" />.</param>
	/// <param name="value2">Source <see cref="T:Microsoft.Xna.Framework.Vector3" />.</param>
	/// <param name="amount">Weighting value.</param>
	/// <returns>Cubic interpolation of the specified vectors.</returns>
	public static Vector3 SmoothStep(Vector3 value1, Vector3 value2, float amount)
	{
		return new Vector3(MathHelper.SmoothStep(value1.X, value2.X, amount), MathHelper.SmoothStep(value1.Y, value2.Y, amount), MathHelper.SmoothStep(value1.Z, value2.Z, amount));
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector3" /> that contains cubic interpolation of the specified vectors.
	/// </summary>
	/// <param name="value1">Source <see cref="T:Microsoft.Xna.Framework.Vector3" />.</param>
	/// <param name="value2">Source <see cref="T:Microsoft.Xna.Framework.Vector3" />.</param>
	/// <param name="amount">Weighting value.</param>
	/// <param name="result">Cubic interpolation of the specified vectors as an output parameter.</param>
	public static void SmoothStep(ref Vector3 value1, ref Vector3 value2, float amount, out Vector3 result)
	{
		result.X = MathHelper.SmoothStep(value1.X, value2.X, amount);
		result.Y = MathHelper.SmoothStep(value1.Y, value2.Y, amount);
		result.Z = MathHelper.SmoothStep(value1.Z, value2.Z, amount);
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector3" /> that contains subtraction of on <see cref="T:Microsoft.Xna.Framework.Vector3" /> from a another.
	/// </summary>
	/// <param name="value1">Source <see cref="T:Microsoft.Xna.Framework.Vector3" />.</param>
	/// <param name="value2">Source <see cref="T:Microsoft.Xna.Framework.Vector3" />.</param>
	/// <returns>The result of the vector subtraction.</returns>
	public static Vector3 Subtract(Vector3 value1, Vector3 value2)
	{
		value1.X -= value2.X;
		value1.Y -= value2.Y;
		value1.Z -= value2.Z;
		return value1;
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector3" /> that contains subtraction of on <see cref="T:Microsoft.Xna.Framework.Vector3" /> from a another.
	/// </summary>
	/// <param name="value1">Source <see cref="T:Microsoft.Xna.Framework.Vector3" />.</param>
	/// <param name="value2">Source <see cref="T:Microsoft.Xna.Framework.Vector3" />.</param>
	/// <param name="result">The result of the vector subtraction as an output parameter.</param>
	public static void Subtract(ref Vector3 value1, ref Vector3 value2, out Vector3 result)
	{
		result.X = value1.X - value2.X;
		result.Y = value1.Y - value2.Y;
		result.Z = value1.Z - value2.Z;
	}

	/// <summary>
	/// Returns a <see cref="T:System.String" /> representation of this <see cref="T:Microsoft.Xna.Framework.Vector3" /> in the format:
	/// {X:[<see cref="F:Microsoft.Xna.Framework.Vector3.X" />] Y:[<see cref="F:Microsoft.Xna.Framework.Vector3.Y" />] Z:[<see cref="F:Microsoft.Xna.Framework.Vector3.Z" />]}
	/// </summary>
	/// <returns>A <see cref="T:System.String" /> representation of this <see cref="T:Microsoft.Xna.Framework.Vector3" />.</returns>
	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder(32);
		stringBuilder.Append("{X:");
		stringBuilder.Append(this.X);
		stringBuilder.Append(" Y:");
		stringBuilder.Append(this.Y);
		stringBuilder.Append(" Z:");
		stringBuilder.Append(this.Z);
		stringBuilder.Append("}");
		return stringBuilder.ToString();
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector3" /> that contains a transformation of 3d-vector by the specified <see cref="T:Microsoft.Xna.Framework.Matrix" />.
	/// </summary>
	/// <param name="position">Source <see cref="T:Microsoft.Xna.Framework.Vector3" />.</param>
	/// <param name="matrix">The transformation <see cref="T:Microsoft.Xna.Framework.Matrix" />.</param>
	/// <returns>Transformed <see cref="T:Microsoft.Xna.Framework.Vector3" />.</returns>
	public static Vector3 Transform(Vector3 position, Matrix matrix)
	{
		Vector3.Transform(ref position, ref matrix, out position);
		return position;
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector3" /> that contains a transformation of 3d-vector by the specified <see cref="T:Microsoft.Xna.Framework.Matrix" />.
	/// </summary>
	/// <param name="position">Source <see cref="T:Microsoft.Xna.Framework.Vector3" />.</param>
	/// <param name="matrix">The transformation <see cref="T:Microsoft.Xna.Framework.Matrix" />.</param>
	/// <param name="result">Transformed <see cref="T:Microsoft.Xna.Framework.Vector3" /> as an output parameter.</param>
	public static void Transform(ref Vector3 position, ref Matrix matrix, out Vector3 result)
	{
		float x = position.X * matrix.M11 + position.Y * matrix.M21 + position.Z * matrix.M31 + matrix.M41;
		float y = position.X * matrix.M12 + position.Y * matrix.M22 + position.Z * matrix.M32 + matrix.M42;
		float z = position.X * matrix.M13 + position.Y * matrix.M23 + position.Z * matrix.M33 + matrix.M43;
		result.X = x;
		result.Y = y;
		result.Z = z;
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector3" /> that contains a transformation of 3d-vector by the specified <see cref="T:Microsoft.Xna.Framework.Quaternion" />, representing the rotation.
	/// </summary>
	/// <param name="value">Source <see cref="T:Microsoft.Xna.Framework.Vector3" />.</param>
	/// <param name="rotation">The <see cref="T:Microsoft.Xna.Framework.Quaternion" /> which contains rotation transformation.</param>
	/// <returns>Transformed <see cref="T:Microsoft.Xna.Framework.Vector3" />.</returns>
	public static Vector3 Transform(Vector3 value, Quaternion rotation)
	{
		Vector3.Transform(ref value, ref rotation, out var result);
		return result;
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector3" /> that contains a transformation of 3d-vector by the specified <see cref="T:Microsoft.Xna.Framework.Quaternion" />, representing the rotation.
	/// </summary>
	/// <param name="value">Source <see cref="T:Microsoft.Xna.Framework.Vector3" />.</param>
	/// <param name="rotation">The <see cref="T:Microsoft.Xna.Framework.Quaternion" /> which contains rotation transformation.</param>
	/// <param name="result">Transformed <see cref="T:Microsoft.Xna.Framework.Vector3" /> as an output parameter.</param>
	public static void Transform(ref Vector3 value, ref Quaternion rotation, out Vector3 result)
	{
		float x = 2f * (rotation.Y * value.Z - rotation.Z * value.Y);
		float y = 2f * (rotation.Z * value.X - rotation.X * value.Z);
		float z = 2f * (rotation.X * value.Y - rotation.Y * value.X);
		result.X = value.X + x * rotation.W + (rotation.Y * z - rotation.Z * y);
		result.Y = value.Y + y * rotation.W + (rotation.Z * x - rotation.X * z);
		result.Z = value.Z + z * rotation.W + (rotation.X * y - rotation.Y * x);
	}

	/// <summary>
	/// Apply transformation on vectors within array of <see cref="T:Microsoft.Xna.Framework.Vector3" /> by the specified <see cref="T:Microsoft.Xna.Framework.Matrix" /> and places the results in an another array.
	/// </summary>
	/// <param name="sourceArray">Source array.</param>
	/// <param name="sourceIndex">The starting index of transformation in the source array.</param>
	/// <param name="matrix">The transformation <see cref="T:Microsoft.Xna.Framework.Matrix" />.</param>
	/// <param name="destinationArray">Destination array.</param>
	/// <param name="destinationIndex">The starting index in the destination array, where the first <see cref="T:Microsoft.Xna.Framework.Vector3" /> should be written.</param>
	/// <param name="length">The number of vectors to be transformed.</param>
	public static void Transform(Vector3[] sourceArray, int sourceIndex, ref Matrix matrix, Vector3[] destinationArray, int destinationIndex, int length)
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
			Vector3 position = sourceArray[sourceIndex + i];
			destinationArray[destinationIndex + i] = new Vector3(position.X * matrix.M11 + position.Y * matrix.M21 + position.Z * matrix.M31 + matrix.M41, position.X * matrix.M12 + position.Y * matrix.M22 + position.Z * matrix.M32 + matrix.M42, position.X * matrix.M13 + position.Y * matrix.M23 + position.Z * matrix.M33 + matrix.M43);
		}
	}

	/// <summary>
	/// Apply transformation on vectors within array of <see cref="T:Microsoft.Xna.Framework.Vector3" /> by the specified <see cref="T:Microsoft.Xna.Framework.Quaternion" /> and places the results in an another array.
	/// </summary>
	/// <param name="sourceArray">Source array.</param>
	/// <param name="sourceIndex">The starting index of transformation in the source array.</param>
	/// <param name="rotation">The <see cref="T:Microsoft.Xna.Framework.Quaternion" /> which contains rotation transformation.</param>
	/// <param name="destinationArray">Destination array.</param>
	/// <param name="destinationIndex">The starting index in the destination array, where the first <see cref="T:Microsoft.Xna.Framework.Vector3" /> should be written.</param>
	/// <param name="length">The number of vectors to be transformed.</param>
	public static void Transform(Vector3[] sourceArray, int sourceIndex, ref Quaternion rotation, Vector3[] destinationArray, int destinationIndex, int length)
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
			Vector3 position = sourceArray[sourceIndex + i];
			float x = 2f * (rotation.Y * position.Z - rotation.Z * position.Y);
			float y = 2f * (rotation.Z * position.X - rotation.X * position.Z);
			float z = 2f * (rotation.X * position.Y - rotation.Y * position.X);
			destinationArray[destinationIndex + i] = new Vector3(position.X + x * rotation.W + (rotation.Y * z - rotation.Z * y), position.Y + y * rotation.W + (rotation.Z * x - rotation.X * z), position.Z + z * rotation.W + (rotation.X * y - rotation.Y * x));
		}
	}

	/// <summary>
	/// Apply transformation on all vectors within array of <see cref="T:Microsoft.Xna.Framework.Vector3" /> by the specified <see cref="T:Microsoft.Xna.Framework.Matrix" /> and places the results in an another array.
	/// </summary>
	/// <param name="sourceArray">Source array.</param>
	/// <param name="matrix">The transformation <see cref="T:Microsoft.Xna.Framework.Matrix" />.</param>
	/// <param name="destinationArray">Destination array.</param>
	public static void Transform(Vector3[] sourceArray, ref Matrix matrix, Vector3[] destinationArray)
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
			Vector3 position = sourceArray[i];
			destinationArray[i] = new Vector3(position.X * matrix.M11 + position.Y * matrix.M21 + position.Z * matrix.M31 + matrix.M41, position.X * matrix.M12 + position.Y * matrix.M22 + position.Z * matrix.M32 + matrix.M42, position.X * matrix.M13 + position.Y * matrix.M23 + position.Z * matrix.M33 + matrix.M43);
		}
	}

	/// <summary>
	/// Apply transformation on all vectors within array of <see cref="T:Microsoft.Xna.Framework.Vector3" /> by the specified <see cref="T:Microsoft.Xna.Framework.Quaternion" /> and places the results in an another array.
	/// </summary>
	/// <param name="sourceArray">Source array.</param>
	/// <param name="rotation">The <see cref="T:Microsoft.Xna.Framework.Quaternion" /> which contains rotation transformation.</param>
	/// <param name="destinationArray">Destination array.</param>
	public static void Transform(Vector3[] sourceArray, ref Quaternion rotation, Vector3[] destinationArray)
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
			Vector3 position = sourceArray[i];
			float x = 2f * (rotation.Y * position.Z - rotation.Z * position.Y);
			float y = 2f * (rotation.Z * position.X - rotation.X * position.Z);
			float z = 2f * (rotation.X * position.Y - rotation.Y * position.X);
			destinationArray[i] = new Vector3(position.X + x * rotation.W + (rotation.Y * z - rotation.Z * y), position.Y + y * rotation.W + (rotation.Z * x - rotation.X * z), position.Z + z * rotation.W + (rotation.X * y - rotation.Y * x));
		}
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector3" /> that contains a transformation of the specified normal by the specified <see cref="T:Microsoft.Xna.Framework.Matrix" />.
	/// </summary>
	/// <param name="normal">Source <see cref="T:Microsoft.Xna.Framework.Vector3" /> which represents a normal vector.</param>
	/// <param name="matrix">The transformation <see cref="T:Microsoft.Xna.Framework.Matrix" />.</param>
	/// <returns>Transformed normal.</returns>
	public static Vector3 TransformNormal(Vector3 normal, Matrix matrix)
	{
		Vector3.TransformNormal(ref normal, ref matrix, out normal);
		return normal;
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Vector3" /> that contains a transformation of the specified normal by the specified <see cref="T:Microsoft.Xna.Framework.Matrix" />.
	/// </summary>
	/// <param name="normal">Source <see cref="T:Microsoft.Xna.Framework.Vector3" /> which represents a normal vector.</param>
	/// <param name="matrix">The transformation <see cref="T:Microsoft.Xna.Framework.Matrix" />.</param>
	/// <param name="result">Transformed normal as an output parameter.</param>
	public static void TransformNormal(ref Vector3 normal, ref Matrix matrix, out Vector3 result)
	{
		float x = normal.X * matrix.M11 + normal.Y * matrix.M21 + normal.Z * matrix.M31;
		float y = normal.X * matrix.M12 + normal.Y * matrix.M22 + normal.Z * matrix.M32;
		float z = normal.X * matrix.M13 + normal.Y * matrix.M23 + normal.Z * matrix.M33;
		result.X = x;
		result.Y = y;
		result.Z = z;
	}

	/// <summary>
	/// Apply transformation on normals within array of <see cref="T:Microsoft.Xna.Framework.Vector3" /> by the specified <see cref="T:Microsoft.Xna.Framework.Matrix" /> and places the results in an another array.
	/// </summary>
	/// <param name="sourceArray">Source array.</param>
	/// <param name="sourceIndex">The starting index of transformation in the source array.</param>
	/// <param name="matrix">The transformation <see cref="T:Microsoft.Xna.Framework.Matrix" />.</param>
	/// <param name="destinationArray">Destination array.</param>
	/// <param name="destinationIndex">The starting index in the destination array, where the first <see cref="T:Microsoft.Xna.Framework.Vector3" /> should be written.</param>
	/// <param name="length">The number of normals to be transformed.</param>
	public static void TransformNormal(Vector3[] sourceArray, int sourceIndex, ref Matrix matrix, Vector3[] destinationArray, int destinationIndex, int length)
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
			Vector3 normal = sourceArray[sourceIndex + x];
			destinationArray[destinationIndex + x] = new Vector3(normal.X * matrix.M11 + normal.Y * matrix.M21 + normal.Z * matrix.M31, normal.X * matrix.M12 + normal.Y * matrix.M22 + normal.Z * matrix.M32, normal.X * matrix.M13 + normal.Y * matrix.M23 + normal.Z * matrix.M33);
		}
	}

	/// <summary>
	/// Apply transformation on all normals within array of <see cref="T:Microsoft.Xna.Framework.Vector3" /> by the specified <see cref="T:Microsoft.Xna.Framework.Matrix" /> and places the results in an another array.
	/// </summary>
	/// <param name="sourceArray">Source array.</param>
	/// <param name="matrix">The transformation <see cref="T:Microsoft.Xna.Framework.Matrix" />.</param>
	/// <param name="destinationArray">Destination array.</param>
	public static void TransformNormal(Vector3[] sourceArray, ref Matrix matrix, Vector3[] destinationArray)
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
			Vector3 normal = sourceArray[i];
			destinationArray[i] = new Vector3(normal.X * matrix.M11 + normal.Y * matrix.M21 + normal.Z * matrix.M31, normal.X * matrix.M12 + normal.Y * matrix.M22 + normal.Z * matrix.M32, normal.X * matrix.M13 + normal.Y * matrix.M23 + normal.Z * matrix.M33);
		}
	}

	/// <summary>
	/// Deconstruction method for <see cref="T:Microsoft.Xna.Framework.Vector3" />.
	/// </summary>
	/// <param name="x"></param>
	/// <param name="y"></param>
	/// <param name="z"></param>
	public void Deconstruct(out float x, out float y, out float z)
	{
		x = this.X;
		y = this.Y;
		z = this.Z;
	}

	/// <summary>
	/// Compares whether two <see cref="T:Microsoft.Xna.Framework.Vector3" /> instances are equal.
	/// </summary>
	/// <param name="value1"><see cref="T:Microsoft.Xna.Framework.Vector3" /> instance on the left of the equal sign.</param>
	/// <param name="value2"><see cref="T:Microsoft.Xna.Framework.Vector3" /> instance on the right of the equal sign.</param>
	/// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
	public static bool operator ==(Vector3 value1, Vector3 value2)
	{
		if (value1.X == value2.X && value1.Y == value2.Y)
		{
			return value1.Z == value2.Z;
		}
		return false;
	}

	/// <summary>
	/// Compares whether two <see cref="T:Microsoft.Xna.Framework.Vector3" /> instances are not equal.
	/// </summary>
	/// <param name="value1"><see cref="T:Microsoft.Xna.Framework.Vector3" /> instance on the left of the not equal sign.</param>
	/// <param name="value2"><see cref="T:Microsoft.Xna.Framework.Vector3" /> instance on the right of the not equal sign.</param>
	/// <returns><c>true</c> if the instances are not equal; <c>false</c> otherwise.</returns>	
	public static bool operator !=(Vector3 value1, Vector3 value2)
	{
		return !(value1 == value2);
	}

	/// <summary>
	/// Adds two vectors.
	/// </summary>
	/// <param name="value1">Source <see cref="T:Microsoft.Xna.Framework.Vector3" /> on the left of the add sign.</param>
	/// <param name="value2">Source <see cref="T:Microsoft.Xna.Framework.Vector3" /> on the right of the add sign.</param>
	/// <returns>Sum of the vectors.</returns>
	public static Vector3 operator +(Vector3 value1, Vector3 value2)
	{
		value1.X += value2.X;
		value1.Y += value2.Y;
		value1.Z += value2.Z;
		return value1;
	}

	/// <summary>
	/// Inverts values in the specified <see cref="T:Microsoft.Xna.Framework.Vector3" />.
	/// </summary>
	/// <param name="value">Source <see cref="T:Microsoft.Xna.Framework.Vector3" /> on the right of the sub sign.</param>
	/// <returns>Result of the inversion.</returns>
	public static Vector3 operator -(Vector3 value)
	{
		value = new Vector3(0f - value.X, 0f - value.Y, 0f - value.Z);
		return value;
	}

	/// <summary>
	/// Subtracts a <see cref="T:Microsoft.Xna.Framework.Vector3" /> from a <see cref="T:Microsoft.Xna.Framework.Vector3" />.
	/// </summary>
	/// <param name="value1">Source <see cref="T:Microsoft.Xna.Framework.Vector3" /> on the left of the sub sign.</param>
	/// <param name="value2">Source <see cref="T:Microsoft.Xna.Framework.Vector3" /> on the right of the sub sign.</param>
	/// <returns>Result of the vector subtraction.</returns>
	public static Vector3 operator -(Vector3 value1, Vector3 value2)
	{
		value1.X -= value2.X;
		value1.Y -= value2.Y;
		value1.Z -= value2.Z;
		return value1;
	}

	/// <summary>
	/// Multiplies the components of two vectors by each other.
	/// </summary>
	/// <param name="value1">Source <see cref="T:Microsoft.Xna.Framework.Vector3" /> on the left of the mul sign.</param>
	/// <param name="value2">Source <see cref="T:Microsoft.Xna.Framework.Vector3" /> on the right of the mul sign.</param>
	/// <returns>Result of the vector multiplication.</returns>
	public static Vector3 operator *(Vector3 value1, Vector3 value2)
	{
		value1.X *= value2.X;
		value1.Y *= value2.Y;
		value1.Z *= value2.Z;
		return value1;
	}

	/// <summary>
	/// Multiplies the components of vector by a scalar.
	/// </summary>
	/// <param name="value">Source <see cref="T:Microsoft.Xna.Framework.Vector3" /> on the left of the mul sign.</param>
	/// <param name="scaleFactor">Scalar value on the right of the mul sign.</param>
	/// <returns>Result of the vector multiplication with a scalar.</returns>
	public static Vector3 operator *(Vector3 value, float scaleFactor)
	{
		value.X *= scaleFactor;
		value.Y *= scaleFactor;
		value.Z *= scaleFactor;
		return value;
	}

	/// <summary>
	/// Multiplies the components of vector by a scalar.
	/// </summary>
	/// <param name="scaleFactor">Scalar value on the left of the mul sign.</param>
	/// <param name="value">Source <see cref="T:Microsoft.Xna.Framework.Vector3" /> on the right of the mul sign.</param>
	/// <returns>Result of the vector multiplication with a scalar.</returns>
	public static Vector3 operator *(float scaleFactor, Vector3 value)
	{
		value.X *= scaleFactor;
		value.Y *= scaleFactor;
		value.Z *= scaleFactor;
		return value;
	}

	/// <summary>
	/// Divides the components of a <see cref="T:Microsoft.Xna.Framework.Vector3" /> by the components of another <see cref="T:Microsoft.Xna.Framework.Vector3" />.
	/// </summary>
	/// <param name="value1">Source <see cref="T:Microsoft.Xna.Framework.Vector3" /> on the left of the div sign.</param>
	/// <param name="value2">Divisor <see cref="T:Microsoft.Xna.Framework.Vector3" /> on the right of the div sign.</param>
	/// <returns>The result of dividing the vectors.</returns>
	public static Vector3 operator /(Vector3 value1, Vector3 value2)
	{
		value1.X /= value2.X;
		value1.Y /= value2.Y;
		value1.Z /= value2.Z;
		return value1;
	}

	/// <summary>
	/// Divides the components of a <see cref="T:Microsoft.Xna.Framework.Vector3" /> by a scalar.
	/// </summary>
	/// <param name="value1">Source <see cref="T:Microsoft.Xna.Framework.Vector3" /> on the left of the div sign.</param>
	/// <param name="divider">Divisor scalar on the right of the div sign.</param>
	/// <returns>The result of dividing a vector by a scalar.</returns>
	public static Vector3 operator /(Vector3 value1, float divider)
	{
		float factor = 1f / divider;
		value1.X *= factor;
		value1.Y *= factor;
		value1.Z *= factor;
		return value1;
	}
}
