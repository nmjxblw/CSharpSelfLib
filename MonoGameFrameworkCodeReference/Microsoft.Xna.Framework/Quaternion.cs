using System;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Microsoft.Xna.Framework;

/// <summary>
/// An efficient mathematical representation for three dimensional rotations.
/// </summary>
[DataContract]
[DebuggerDisplay("{DebugDisplayString,nq}")]
public struct Quaternion : IEquatable<Quaternion>
{
	private static readonly Quaternion _identity = new Quaternion(0f, 0f, 0f, 1f);

	/// <summary>
	/// The x coordinate of this <see cref="T:Microsoft.Xna.Framework.Quaternion" />.
	/// </summary>
	[DataMember]
	public float X;

	/// <summary>
	/// The y coordinate of this <see cref="T:Microsoft.Xna.Framework.Quaternion" />.
	/// </summary>
	[DataMember]
	public float Y;

	/// <summary>
	/// The z coordinate of this <see cref="T:Microsoft.Xna.Framework.Quaternion" />.
	/// </summary>
	[DataMember]
	public float Z;

	/// <summary>
	/// The rotation component of this <see cref="T:Microsoft.Xna.Framework.Quaternion" />.
	/// </summary>
	[DataMember]
	public float W;

	/// <summary>
	/// Returns a quaternion representing no rotation.
	/// </summary>
	public static Quaternion Identity => Quaternion._identity;

	internal string DebugDisplayString
	{
		get
		{
			if (this == Quaternion._identity)
			{
				return "Identity";
			}
			return this.X + " " + this.Y + " " + this.Z + " " + this.W;
		}
	}

	/// <summary>
	/// Constructs a quaternion with X, Y, Z and W from four values.
	/// </summary>
	/// <param name="x">The x coordinate in 3d-space.</param>
	/// <param name="y">The y coordinate in 3d-space.</param>
	/// <param name="z">The z coordinate in 3d-space.</param>
	/// <param name="w">The rotation component.</param>
	public Quaternion(float x, float y, float z, float w)
	{
		this.X = x;
		this.Y = y;
		this.Z = z;
		this.W = w;
	}

	/// <summary>
	/// Constructs a quaternion with X, Y, Z from <see cref="T:Microsoft.Xna.Framework.Vector3" /> and rotation component from a scalar.
	/// </summary>
	/// <param name="value">The x, y, z coordinates in 3d-space.</param>
	/// <param name="w">The rotation component.</param>
	public Quaternion(Vector3 value, float w)
	{
		this.X = value.X;
		this.Y = value.Y;
		this.Z = value.Z;
		this.W = w;
	}

	/// <summary>
	/// Constructs a quaternion from <see cref="T:Microsoft.Xna.Framework.Vector4" />.
	/// </summary>
	/// <param name="value">The x, y, z coordinates in 3d-space and the rotation component.</param>
	public Quaternion(Vector4 value)
	{
		this.X = value.X;
		this.Y = value.Y;
		this.Z = value.Z;
		this.W = value.W;
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Quaternion" /> that contains the sum of two quaternions.
	/// </summary>
	/// <param name="quaternion1">Source <see cref="T:Microsoft.Xna.Framework.Quaternion" />.</param>
	/// <param name="quaternion2">Source <see cref="T:Microsoft.Xna.Framework.Quaternion" />.</param>
	/// <returns>The result of the quaternion addition.</returns>
	public static Quaternion Add(Quaternion quaternion1, Quaternion quaternion2)
	{
		Quaternion quaternion3 = default(Quaternion);
		quaternion3.X = quaternion1.X + quaternion2.X;
		quaternion3.Y = quaternion1.Y + quaternion2.Y;
		quaternion3.Z = quaternion1.Z + quaternion2.Z;
		quaternion3.W = quaternion1.W + quaternion2.W;
		return quaternion3;
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Quaternion" /> that contains the sum of two quaternions.
	/// </summary>
	/// <param name="quaternion1">Source <see cref="T:Microsoft.Xna.Framework.Quaternion" />.</param>
	/// <param name="quaternion2">Source <see cref="T:Microsoft.Xna.Framework.Quaternion" />.</param>
	/// <param name="result">The result of the quaternion addition as an output parameter.</param>
	public static void Add(ref Quaternion quaternion1, ref Quaternion quaternion2, out Quaternion result)
	{
		result.X = quaternion1.X + quaternion2.X;
		result.Y = quaternion1.Y + quaternion2.Y;
		result.Z = quaternion1.Z + quaternion2.Z;
		result.W = quaternion1.W + quaternion2.W;
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Quaternion" /> that contains concatenation between two quaternion.
	/// </summary>
	/// <param name="value1">The first <see cref="T:Microsoft.Xna.Framework.Quaternion" /> to concatenate.</param>
	/// <param name="value2">The second <see cref="T:Microsoft.Xna.Framework.Quaternion" /> to concatenate.</param>
	/// <returns>The result of rotation of <paramref name="value1" /> followed by <paramref name="value2" /> rotation.</returns>
	public static Quaternion Concatenate(Quaternion value1, Quaternion value2)
	{
		float x1 = value1.X;
		float y1 = value1.Y;
		float z1 = value1.Z;
		float w1 = value1.W;
		float x2 = value2.X;
		float y2 = value2.Y;
		float z2 = value2.Z;
		float w2 = value2.W;
		Quaternion quaternion = default(Quaternion);
		quaternion.X = x2 * w1 + x1 * w2 + (y2 * z1 - z2 * y1);
		quaternion.Y = y2 * w1 + y1 * w2 + (z2 * x1 - x2 * z1);
		quaternion.Z = z2 * w1 + z1 * w2 + (x2 * y1 - y2 * x1);
		quaternion.W = w2 * w1 - (x2 * x1 + y2 * y1 + z2 * z1);
		return quaternion;
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Quaternion" /> that contains concatenation between two quaternion.
	/// </summary>
	/// <param name="value1">The first <see cref="T:Microsoft.Xna.Framework.Quaternion" /> to concatenate.</param>
	/// <param name="value2">The second <see cref="T:Microsoft.Xna.Framework.Quaternion" /> to concatenate.</param>
	/// <param name="result">The result of rotation of <paramref name="value1" /> followed by <paramref name="value2" /> rotation as an output parameter.</param>
	public static void Concatenate(ref Quaternion value1, ref Quaternion value2, out Quaternion result)
	{
		float x1 = value1.X;
		float y1 = value1.Y;
		float z1 = value1.Z;
		float w1 = value1.W;
		float x2 = value2.X;
		float y2 = value2.Y;
		float z2 = value2.Z;
		float w2 = value2.W;
		result.X = x2 * w1 + x1 * w2 + (y2 * z1 - z2 * y1);
		result.Y = y2 * w1 + y1 * w2 + (z2 * x1 - x2 * z1);
		result.Z = z2 * w1 + z1 * w2 + (x2 * y1 - y2 * x1);
		result.W = w2 * w1 - (x2 * x1 + y2 * y1 + z2 * z1);
	}

	/// <summary>
	/// Transforms this quaternion into its conjugated version.
	/// </summary>
	public void Conjugate()
	{
		this.X = 0f - this.X;
		this.Y = 0f - this.Y;
		this.Z = 0f - this.Z;
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Quaternion" /> that contains conjugated version of the specified quaternion.
	/// </summary>
	/// <param name="value">The quaternion which values will be used to create the conjugated version.</param>
	/// <returns>The conjugate version of the specified quaternion.</returns>
	public static Quaternion Conjugate(Quaternion value)
	{
		return new Quaternion(0f - value.X, 0f - value.Y, 0f - value.Z, value.W);
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Quaternion" /> that contains conjugated version of the specified quaternion.
	/// </summary>
	/// <param name="value">The quaternion which values will be used to create the conjugated version.</param>
	/// <param name="result">The conjugated version of the specified quaternion as an output parameter.</param>
	public static void Conjugate(ref Quaternion value, out Quaternion result)
	{
		result.X = 0f - value.X;
		result.Y = 0f - value.Y;
		result.Z = 0f - value.Z;
		result.W = value.W;
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Quaternion" /> from the specified axis and angle.
	/// </summary>
	/// <param name="axis">The axis of rotation.</param>
	/// <param name="angle">The angle in radians.</param>
	/// <returns>The new quaternion builded from axis and angle.</returns>
	public static Quaternion CreateFromAxisAngle(Vector3 axis, float angle)
	{
		float num = angle * 0.5f;
		float sin = (float)Math.Sin(num);
		float cos = (float)Math.Cos(num);
		return new Quaternion(axis.X * sin, axis.Y * sin, axis.Z * sin, cos);
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Quaternion" /> from the specified axis and angle.
	/// </summary>
	/// <param name="axis">The axis of rotation.</param>
	/// <param name="angle">The angle in radians.</param>
	/// <param name="result">The new quaternion builded from axis and angle as an output parameter.</param>
	public static void CreateFromAxisAngle(ref Vector3 axis, float angle, out Quaternion result)
	{
		float num = angle * 0.5f;
		float sin = (float)Math.Sin(num);
		float cos = (float)Math.Cos(num);
		result.X = axis.X * sin;
		result.Y = axis.Y * sin;
		result.Z = axis.Z * sin;
		result.W = cos;
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Quaternion" /> from the specified <see cref="T:Microsoft.Xna.Framework.Matrix" />.
	/// </summary>
	/// <param name="matrix">The rotation matrix.</param>
	/// <returns>A quaternion composed from the rotation part of the matrix.</returns>
	public static Quaternion CreateFromRotationMatrix(Matrix matrix)
	{
		float scale = matrix.M11 + matrix.M22 + matrix.M33;
		Quaternion quaternion = default(Quaternion);
		float sqrt;
		if (scale > 0f)
		{
			sqrt = (float)Math.Sqrt(scale + 1f);
			quaternion.W = sqrt * 0.5f;
			sqrt = 0.5f / sqrt;
			quaternion.X = (matrix.M23 - matrix.M32) * sqrt;
			quaternion.Y = (matrix.M31 - matrix.M13) * sqrt;
			quaternion.Z = (matrix.M12 - matrix.M21) * sqrt;
			return quaternion;
		}
		float half;
		if (matrix.M11 >= matrix.M22 && matrix.M11 >= matrix.M33)
		{
			sqrt = (float)Math.Sqrt(1f + matrix.M11 - matrix.M22 - matrix.M33);
			half = 0.5f / sqrt;
			quaternion.X = 0.5f * sqrt;
			quaternion.Y = (matrix.M12 + matrix.M21) * half;
			quaternion.Z = (matrix.M13 + matrix.M31) * half;
			quaternion.W = (matrix.M23 - matrix.M32) * half;
			return quaternion;
		}
		if (matrix.M22 > matrix.M33)
		{
			sqrt = (float)Math.Sqrt(1f + matrix.M22 - matrix.M11 - matrix.M33);
			half = 0.5f / sqrt;
			quaternion.X = (matrix.M21 + matrix.M12) * half;
			quaternion.Y = 0.5f * sqrt;
			quaternion.Z = (matrix.M32 + matrix.M23) * half;
			quaternion.W = (matrix.M31 - matrix.M13) * half;
			return quaternion;
		}
		sqrt = (float)Math.Sqrt(1f + matrix.M33 - matrix.M11 - matrix.M22);
		half = 0.5f / sqrt;
		quaternion.X = (matrix.M31 + matrix.M13) * half;
		quaternion.Y = (matrix.M32 + matrix.M23) * half;
		quaternion.Z = 0.5f * sqrt;
		quaternion.W = (matrix.M12 - matrix.M21) * half;
		return quaternion;
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Quaternion" /> from the specified <see cref="T:Microsoft.Xna.Framework.Matrix" />.
	/// </summary>
	/// <param name="matrix">The rotation matrix.</param>
	/// <param name="result">A quaternion composed from the rotation part of the matrix as an output parameter.</param>
	public static void CreateFromRotationMatrix(ref Matrix matrix, out Quaternion result)
	{
		float scale = matrix.M11 + matrix.M22 + matrix.M33;
		if (scale > 0f)
		{
			float sqrt = (float)Math.Sqrt(scale + 1f);
			result.W = sqrt * 0.5f;
			sqrt = 0.5f / sqrt;
			result.X = (matrix.M23 - matrix.M32) * sqrt;
			result.Y = (matrix.M31 - matrix.M13) * sqrt;
			result.Z = (matrix.M12 - matrix.M21) * sqrt;
		}
		else if (matrix.M11 >= matrix.M22 && matrix.M11 >= matrix.M33)
		{
			float sqrt = (float)Math.Sqrt(1f + matrix.M11 - matrix.M22 - matrix.M33);
			float half = 0.5f / sqrt;
			result.X = 0.5f * sqrt;
			result.Y = (matrix.M12 + matrix.M21) * half;
			result.Z = (matrix.M13 + matrix.M31) * half;
			result.W = (matrix.M23 - matrix.M32) * half;
		}
		else if (matrix.M22 > matrix.M33)
		{
			float sqrt = (float)Math.Sqrt(1f + matrix.M22 - matrix.M11 - matrix.M33);
			float half = 0.5f / sqrt;
			result.X = (matrix.M21 + matrix.M12) * half;
			result.Y = 0.5f * sqrt;
			result.Z = (matrix.M32 + matrix.M23) * half;
			result.W = (matrix.M31 - matrix.M13) * half;
		}
		else
		{
			float sqrt = (float)Math.Sqrt(1f + matrix.M33 - matrix.M11 - matrix.M22);
			float half = 0.5f / sqrt;
			result.X = (matrix.M31 + matrix.M13) * half;
			result.Y = (matrix.M32 + matrix.M23) * half;
			result.Z = 0.5f * sqrt;
			result.W = (matrix.M12 - matrix.M21) * half;
		}
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Quaternion" /> from the specified yaw, pitch and roll angles.
	/// </summary>
	/// <param name="yaw">Yaw around the y axis in radians.</param>
	/// <param name="pitch">Pitch around the x axis in radians.</param>
	/// <param name="roll">Roll around the z axis in radians.</param>
	/// <returns>A new quaternion from the concatenated yaw, pitch, and roll angles.</returns>
	public static Quaternion CreateFromYawPitchRoll(float yaw, float pitch, float roll)
	{
		float num = roll * 0.5f;
		float halfPitch = pitch * 0.5f;
		float halfYaw = yaw * 0.5f;
		float sinRoll = (float)Math.Sin(num);
		float cosRoll = (float)Math.Cos(num);
		float sinPitch = (float)Math.Sin(halfPitch);
		float cosPitch = (float)Math.Cos(halfPitch);
		float sinYaw = (float)Math.Sin(halfYaw);
		float cosYaw = (float)Math.Cos(halfYaw);
		return new Quaternion(cosYaw * sinPitch * cosRoll + sinYaw * cosPitch * sinRoll, sinYaw * cosPitch * cosRoll - cosYaw * sinPitch * sinRoll, cosYaw * cosPitch * sinRoll - sinYaw * sinPitch * cosRoll, cosYaw * cosPitch * cosRoll + sinYaw * sinPitch * sinRoll);
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Quaternion" /> from the specified yaw, pitch and roll angles.
	/// </summary>
	/// <param name="yaw">Yaw around the y axis in radians.</param>
	/// <param name="pitch">Pitch around the x axis in radians.</param>
	/// <param name="roll">Roll around the z axis in radians.</param>
	/// <param name="result">A new quaternion from the concatenated yaw, pitch, and roll angles as an output parameter.</param>
	public static void CreateFromYawPitchRoll(float yaw, float pitch, float roll, out Quaternion result)
	{
		float num = roll * 0.5f;
		float halfPitch = pitch * 0.5f;
		float halfYaw = yaw * 0.5f;
		float sinRoll = (float)Math.Sin(num);
		float cosRoll = (float)Math.Cos(num);
		float sinPitch = (float)Math.Sin(halfPitch);
		float cosPitch = (float)Math.Cos(halfPitch);
		float sinYaw = (float)Math.Sin(halfYaw);
		float cosYaw = (float)Math.Cos(halfYaw);
		result.X = cosYaw * sinPitch * cosRoll + sinYaw * cosPitch * sinRoll;
		result.Y = sinYaw * cosPitch * cosRoll - cosYaw * sinPitch * sinRoll;
		result.Z = cosYaw * cosPitch * sinRoll - sinYaw * sinPitch * cosRoll;
		result.W = cosYaw * cosPitch * cosRoll + sinYaw * sinPitch * sinRoll;
	}

	/// <summary>
	/// Divides a <see cref="T:Microsoft.Xna.Framework.Quaternion" /> by the other <see cref="T:Microsoft.Xna.Framework.Quaternion" />.
	/// </summary>
	/// <param name="quaternion1">Source <see cref="T:Microsoft.Xna.Framework.Quaternion" />.</param>
	/// <param name="quaternion2">Divisor <see cref="T:Microsoft.Xna.Framework.Quaternion" />.</param>
	/// <returns>The result of dividing the quaternions.</returns>
	public static Quaternion Divide(Quaternion quaternion1, Quaternion quaternion2)
	{
		float x = quaternion1.X;
		float y = quaternion1.Y;
		float z = quaternion1.Z;
		float w = quaternion1.W;
		float num14 = quaternion2.X * quaternion2.X + quaternion2.Y * quaternion2.Y + quaternion2.Z * quaternion2.Z + quaternion2.W * quaternion2.W;
		float num15 = 1f / num14;
		float num16 = (0f - quaternion2.X) * num15;
		float num17 = (0f - quaternion2.Y) * num15;
		float num18 = (0f - quaternion2.Z) * num15;
		float num19 = quaternion2.W * num15;
		float num20 = y * num18 - z * num17;
		float num21 = z * num16 - x * num18;
		float num22 = x * num17 - y * num16;
		float num23 = x * num16 + y * num17 + z * num18;
		Quaternion quaternion3 = default(Quaternion);
		quaternion3.X = x * num19 + num16 * w + num20;
		quaternion3.Y = y * num19 + num17 * w + num21;
		quaternion3.Z = z * num19 + num18 * w + num22;
		quaternion3.W = w * num19 - num23;
		return quaternion3;
	}

	/// <summary>
	/// Divides a <see cref="T:Microsoft.Xna.Framework.Quaternion" /> by the other <see cref="T:Microsoft.Xna.Framework.Quaternion" />.
	/// </summary>
	/// <param name="quaternion1">Source <see cref="T:Microsoft.Xna.Framework.Quaternion" />.</param>
	/// <param name="quaternion2">Divisor <see cref="T:Microsoft.Xna.Framework.Quaternion" />.</param>
	/// <param name="result">The result of dividing the quaternions as an output parameter.</param>
	public static void Divide(ref Quaternion quaternion1, ref Quaternion quaternion2, out Quaternion result)
	{
		float x = quaternion1.X;
		float y = quaternion1.Y;
		float z = quaternion1.Z;
		float w = quaternion1.W;
		float num14 = quaternion2.X * quaternion2.X + quaternion2.Y * quaternion2.Y + quaternion2.Z * quaternion2.Z + quaternion2.W * quaternion2.W;
		float num15 = 1f / num14;
		float num16 = (0f - quaternion2.X) * num15;
		float num17 = (0f - quaternion2.Y) * num15;
		float num18 = (0f - quaternion2.Z) * num15;
		float num19 = quaternion2.W * num15;
		float num20 = y * num18 - z * num17;
		float num21 = z * num16 - x * num18;
		float num22 = x * num17 - y * num16;
		float num23 = x * num16 + y * num17 + z * num18;
		result.X = x * num19 + num16 * w + num20;
		result.Y = y * num19 + num17 * w + num21;
		result.Z = z * num19 + num18 * w + num22;
		result.W = w * num19 - num23;
	}

	/// <summary>
	/// Returns a dot product of two quaternions.
	/// </summary>
	/// <param name="quaternion1">The first quaternion.</param>
	/// <param name="quaternion2">The second quaternion.</param>
	/// <returns>The dot product of two quaternions.</returns>
	public static float Dot(Quaternion quaternion1, Quaternion quaternion2)
	{
		return quaternion1.X * quaternion2.X + quaternion1.Y * quaternion2.Y + quaternion1.Z * quaternion2.Z + quaternion1.W * quaternion2.W;
	}

	/// <summary>
	/// Returns a dot product of two quaternions.
	/// </summary>
	/// <param name="quaternion1">The first quaternion.</param>
	/// <param name="quaternion2">The second quaternion.</param>
	/// <param name="result">The dot product of two quaternions as an output parameter.</param>
	public static void Dot(ref Quaternion quaternion1, ref Quaternion quaternion2, out float result)
	{
		result = quaternion1.X * quaternion2.X + quaternion1.Y * quaternion2.Y + quaternion1.Z * quaternion2.Z + quaternion1.W * quaternion2.W;
	}

	/// <summary>
	/// Compares whether current instance is equal to specified <see cref="T:System.Object" />.
	/// </summary>
	/// <param name="obj">The <see cref="T:System.Object" /> to compare.</param>
	/// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
	public override bool Equals(object obj)
	{
		if (obj is Quaternion)
		{
			return this.Equals((Quaternion)obj);
		}
		return false;
	}

	/// <summary>
	/// Compares whether current instance is equal to specified <see cref="T:Microsoft.Xna.Framework.Quaternion" />.
	/// </summary>
	/// <param name="other">The <see cref="T:Microsoft.Xna.Framework.Quaternion" /> to compare.</param>
	/// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
	public bool Equals(Quaternion other)
	{
		if (this.X == other.X && this.Y == other.Y && this.Z == other.Z)
		{
			return this.W == other.W;
		}
		return false;
	}

	/// <summary>
	/// Gets the hash code of this <see cref="T:Microsoft.Xna.Framework.Quaternion" />.
	/// </summary>
	/// <returns>Hash code of this <see cref="T:Microsoft.Xna.Framework.Quaternion" />.</returns>
	public override int GetHashCode()
	{
		return this.X.GetHashCode() + this.Y.GetHashCode() + this.Z.GetHashCode() + this.W.GetHashCode();
	}

	/// <summary>
	/// Returns the inverse quaternion which represents the opposite rotation.
	/// </summary>
	/// <param name="quaternion">Source <see cref="T:Microsoft.Xna.Framework.Quaternion" />.</param>
	/// <returns>The inverse quaternion.</returns>
	public static Quaternion Inverse(Quaternion quaternion)
	{
		float num2 = quaternion.X * quaternion.X + quaternion.Y * quaternion.Y + quaternion.Z * quaternion.Z + quaternion.W * quaternion.W;
		float num3 = 1f / num2;
		Quaternion quaternion2 = default(Quaternion);
		quaternion2.X = (0f - quaternion.X) * num3;
		quaternion2.Y = (0f - quaternion.Y) * num3;
		quaternion2.Z = (0f - quaternion.Z) * num3;
		quaternion2.W = quaternion.W * num3;
		return quaternion2;
	}

	/// <summary>
	/// Returns the inverse quaternion which represents the opposite rotation.
	/// </summary>
	/// <param name="quaternion">Source <see cref="T:Microsoft.Xna.Framework.Quaternion" />.</param>
	/// <param name="result">The inverse quaternion as an output parameter.</param>
	public static void Inverse(ref Quaternion quaternion, out Quaternion result)
	{
		float num2 = quaternion.X * quaternion.X + quaternion.Y * quaternion.Y + quaternion.Z * quaternion.Z + quaternion.W * quaternion.W;
		float num3 = 1f / num2;
		result.X = (0f - quaternion.X) * num3;
		result.Y = (0f - quaternion.Y) * num3;
		result.Z = (0f - quaternion.Z) * num3;
		result.W = quaternion.W * num3;
	}

	/// <summary>
	/// Returns the magnitude of the quaternion components.
	/// </summary>
	/// <returns>The magnitude of the quaternion components.</returns>
	public float Length()
	{
		return (float)Math.Sqrt(this.X * this.X + this.Y * this.Y + this.Z * this.Z + this.W * this.W);
	}

	/// <summary>
	/// Returns the squared magnitude of the quaternion components.
	/// </summary>
	/// <returns>The squared magnitude of the quaternion components.</returns>
	public float LengthSquared()
	{
		return this.X * this.X + this.Y * this.Y + this.Z * this.Z + this.W * this.W;
	}

	/// <summary>
	/// Performs a linear blend between two quaternions.
	/// </summary>
	/// <param name="quaternion1">Source <see cref="T:Microsoft.Xna.Framework.Quaternion" />.</param>
	/// <param name="quaternion2">Source <see cref="T:Microsoft.Xna.Framework.Quaternion" />.</param>
	/// <param name="amount">The blend amount where 0 returns <paramref name="quaternion1" /> and 1 <paramref name="quaternion2" />.</param>
	/// <returns>The result of linear blending between two quaternions.</returns>
	public static Quaternion Lerp(Quaternion quaternion1, Quaternion quaternion2, float amount)
	{
		float num2 = 1f - amount;
		Quaternion quaternion3 = default(Quaternion);
		if (quaternion1.X * quaternion2.X + quaternion1.Y * quaternion2.Y + quaternion1.Z * quaternion2.Z + quaternion1.W * quaternion2.W >= 0f)
		{
			quaternion3.X = num2 * quaternion1.X + amount * quaternion2.X;
			quaternion3.Y = num2 * quaternion1.Y + amount * quaternion2.Y;
			quaternion3.Z = num2 * quaternion1.Z + amount * quaternion2.Z;
			quaternion3.W = num2 * quaternion1.W + amount * quaternion2.W;
		}
		else
		{
			quaternion3.X = num2 * quaternion1.X - amount * quaternion2.X;
			quaternion3.Y = num2 * quaternion1.Y - amount * quaternion2.Y;
			quaternion3.Z = num2 * quaternion1.Z - amount * quaternion2.Z;
			quaternion3.W = num2 * quaternion1.W - amount * quaternion2.W;
		}
		float num4 = quaternion3.X * quaternion3.X + quaternion3.Y * quaternion3.Y + quaternion3.Z * quaternion3.Z + quaternion3.W * quaternion3.W;
		float num5 = 1f / (float)Math.Sqrt(num4);
		quaternion3.X *= num5;
		quaternion3.Y *= num5;
		quaternion3.Z *= num5;
		quaternion3.W *= num5;
		return quaternion3;
	}

	/// <summary>
	/// Performs a linear blend between two quaternions.
	/// </summary>
	/// <param name="quaternion1">Source <see cref="T:Microsoft.Xna.Framework.Quaternion" />.</param>
	/// <param name="quaternion2">Source <see cref="T:Microsoft.Xna.Framework.Quaternion" />.</param>
	/// <param name="amount">The blend amount where 0 returns <paramref name="quaternion1" /> and 1 <paramref name="quaternion2" />.</param>
	/// <param name="result">The result of linear blending between two quaternions as an output parameter.</param>
	public static void Lerp(ref Quaternion quaternion1, ref Quaternion quaternion2, float amount, out Quaternion result)
	{
		float num2 = 1f - amount;
		if (quaternion1.X * quaternion2.X + quaternion1.Y * quaternion2.Y + quaternion1.Z * quaternion2.Z + quaternion1.W * quaternion2.W >= 0f)
		{
			result.X = num2 * quaternion1.X + amount * quaternion2.X;
			result.Y = num2 * quaternion1.Y + amount * quaternion2.Y;
			result.Z = num2 * quaternion1.Z + amount * quaternion2.Z;
			result.W = num2 * quaternion1.W + amount * quaternion2.W;
		}
		else
		{
			result.X = num2 * quaternion1.X - amount * quaternion2.X;
			result.Y = num2 * quaternion1.Y - amount * quaternion2.Y;
			result.Z = num2 * quaternion1.Z - amount * quaternion2.Z;
			result.W = num2 * quaternion1.W - amount * quaternion2.W;
		}
		float num4 = result.X * result.X + result.Y * result.Y + result.Z * result.Z + result.W * result.W;
		float num5 = 1f / (float)Math.Sqrt(num4);
		result.X *= num5;
		result.Y *= num5;
		result.Z *= num5;
		result.W *= num5;
	}

	/// <summary>
	/// Performs a spherical linear blend between two quaternions.
	/// </summary>
	/// <param name="quaternion1">Source <see cref="T:Microsoft.Xna.Framework.Quaternion" />.</param>
	/// <param name="quaternion2">Source <see cref="T:Microsoft.Xna.Framework.Quaternion" />.</param>
	/// <param name="amount">The blend amount where 0 returns <paramref name="quaternion1" /> and 1 <paramref name="quaternion2" />.</param>
	/// <returns>The result of spherical linear blending between two quaternions.</returns>
	public static Quaternion Slerp(Quaternion quaternion1, Quaternion quaternion2, float amount)
	{
		float num4 = quaternion1.X * quaternion2.X + quaternion1.Y * quaternion2.Y + quaternion1.Z * quaternion2.Z + quaternion1.W * quaternion2.W;
		bool flag = false;
		if (num4 < 0f)
		{
			flag = true;
			num4 = 0f - num4;
		}
		float num5;
		float num6;
		if (num4 > 0.999999f)
		{
			num5 = 1f - amount;
			num6 = (flag ? (0f - amount) : amount);
		}
		else
		{
			float num7 = (float)Math.Acos(num4);
			float num8 = (float)(1.0 / Math.Sin(num7));
			num5 = (float)Math.Sin((1f - amount) * num7) * num8;
			num6 = (flag ? ((float)(0.0 - Math.Sin(amount * num7)) * num8) : ((float)Math.Sin(amount * num7) * num8));
		}
		Quaternion quaternion3 = default(Quaternion);
		quaternion3.X = num5 * quaternion1.X + num6 * quaternion2.X;
		quaternion3.Y = num5 * quaternion1.Y + num6 * quaternion2.Y;
		quaternion3.Z = num5 * quaternion1.Z + num6 * quaternion2.Z;
		quaternion3.W = num5 * quaternion1.W + num6 * quaternion2.W;
		return quaternion3;
	}

	/// <summary>
	/// Performs a spherical linear blend between two quaternions.
	/// </summary>
	/// <param name="quaternion1">Source <see cref="T:Microsoft.Xna.Framework.Quaternion" />.</param>
	/// <param name="quaternion2">Source <see cref="T:Microsoft.Xna.Framework.Quaternion" />.</param>
	/// <param name="amount">The blend amount where 0 returns <paramref name="quaternion1" /> and 1 <paramref name="quaternion2" />.</param>
	/// <param name="result">The result of spherical linear blending between two quaternions as an output parameter.</param>
	public static void Slerp(ref Quaternion quaternion1, ref Quaternion quaternion2, float amount, out Quaternion result)
	{
		float num4 = quaternion1.X * quaternion2.X + quaternion1.Y * quaternion2.Y + quaternion1.Z * quaternion2.Z + quaternion1.W * quaternion2.W;
		bool flag = false;
		if (num4 < 0f)
		{
			flag = true;
			num4 = 0f - num4;
		}
		float num5;
		float num6;
		if (num4 > 0.999999f)
		{
			num5 = 1f - amount;
			num6 = (flag ? (0f - amount) : amount);
		}
		else
		{
			float num7 = (float)Math.Acos(num4);
			float num8 = (float)(1.0 / Math.Sin(num7));
			num5 = (float)Math.Sin((1f - amount) * num7) * num8;
			num6 = (flag ? ((float)(0.0 - Math.Sin(amount * num7)) * num8) : ((float)Math.Sin(amount * num7) * num8));
		}
		result.X = num5 * quaternion1.X + num6 * quaternion2.X;
		result.Y = num5 * quaternion1.Y + num6 * quaternion2.Y;
		result.Z = num5 * quaternion1.Z + num6 * quaternion2.Z;
		result.W = num5 * quaternion1.W + num6 * quaternion2.W;
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Quaternion" /> that contains subtraction of one <see cref="T:Microsoft.Xna.Framework.Quaternion" /> from another.
	/// </summary>
	/// <param name="quaternion1">Source <see cref="T:Microsoft.Xna.Framework.Quaternion" />.</param>
	/// <param name="quaternion2">Source <see cref="T:Microsoft.Xna.Framework.Quaternion" />.</param>
	/// <returns>The result of the quaternion subtraction.</returns>
	public static Quaternion Subtract(Quaternion quaternion1, Quaternion quaternion2)
	{
		Quaternion quaternion3 = default(Quaternion);
		quaternion3.X = quaternion1.X - quaternion2.X;
		quaternion3.Y = quaternion1.Y - quaternion2.Y;
		quaternion3.Z = quaternion1.Z - quaternion2.Z;
		quaternion3.W = quaternion1.W - quaternion2.W;
		return quaternion3;
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Quaternion" /> that contains subtraction of one <see cref="T:Microsoft.Xna.Framework.Quaternion" /> from another.
	/// </summary>
	/// <param name="quaternion1">Source <see cref="T:Microsoft.Xna.Framework.Quaternion" />.</param>
	/// <param name="quaternion2">Source <see cref="T:Microsoft.Xna.Framework.Quaternion" />.</param>
	/// <param name="result">The result of the quaternion subtraction as an output parameter.</param>
	public static void Subtract(ref Quaternion quaternion1, ref Quaternion quaternion2, out Quaternion result)
	{
		result.X = quaternion1.X - quaternion2.X;
		result.Y = quaternion1.Y - quaternion2.Y;
		result.Z = quaternion1.Z - quaternion2.Z;
		result.W = quaternion1.W - quaternion2.W;
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Quaternion" /> that contains a multiplication of two quaternions.
	/// </summary>
	/// <param name="quaternion1">Source <see cref="T:Microsoft.Xna.Framework.Quaternion" />.</param>
	/// <param name="quaternion2">Source <see cref="T:Microsoft.Xna.Framework.Quaternion" />.</param>
	/// <returns>The result of the quaternion multiplication.</returns>
	public static Quaternion Multiply(Quaternion quaternion1, Quaternion quaternion2)
	{
		float x = quaternion1.X;
		float y = quaternion1.Y;
		float z = quaternion1.Z;
		float w = quaternion1.W;
		float num4 = quaternion2.X;
		float num5 = quaternion2.Y;
		float num6 = quaternion2.Z;
		float num7 = quaternion2.W;
		float num12 = y * num6 - z * num5;
		float num13 = z * num4 - x * num6;
		float num14 = x * num5 - y * num4;
		float num15 = x * num4 + y * num5 + z * num6;
		Quaternion quaternion3 = default(Quaternion);
		quaternion3.X = x * num7 + num4 * w + num12;
		quaternion3.Y = y * num7 + num5 * w + num13;
		quaternion3.Z = z * num7 + num6 * w + num14;
		quaternion3.W = w * num7 - num15;
		return quaternion3;
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Quaternion" /> that contains a multiplication of <see cref="T:Microsoft.Xna.Framework.Quaternion" /> and a scalar.
	/// </summary>
	/// <param name="quaternion1">Source <see cref="T:Microsoft.Xna.Framework.Quaternion" />.</param>
	/// <param name="scaleFactor">Scalar value.</param>
	/// <returns>The result of the quaternion multiplication with a scalar.</returns>
	public static Quaternion Multiply(Quaternion quaternion1, float scaleFactor)
	{
		Quaternion quaternion2 = default(Quaternion);
		quaternion2.X = quaternion1.X * scaleFactor;
		quaternion2.Y = quaternion1.Y * scaleFactor;
		quaternion2.Z = quaternion1.Z * scaleFactor;
		quaternion2.W = quaternion1.W * scaleFactor;
		return quaternion2;
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Quaternion" /> that contains a multiplication of <see cref="T:Microsoft.Xna.Framework.Quaternion" /> and a scalar.
	/// </summary>
	/// <param name="quaternion1">Source <see cref="T:Microsoft.Xna.Framework.Quaternion" />.</param>
	/// <param name="scaleFactor">Scalar value.</param>
	/// <param name="result">The result of the quaternion multiplication with a scalar as an output parameter.</param>
	public static void Multiply(ref Quaternion quaternion1, float scaleFactor, out Quaternion result)
	{
		result.X = quaternion1.X * scaleFactor;
		result.Y = quaternion1.Y * scaleFactor;
		result.Z = quaternion1.Z * scaleFactor;
		result.W = quaternion1.W * scaleFactor;
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Quaternion" /> that contains a multiplication of two quaternions.
	/// </summary>
	/// <param name="quaternion1">Source <see cref="T:Microsoft.Xna.Framework.Quaternion" />.</param>
	/// <param name="quaternion2">Source <see cref="T:Microsoft.Xna.Framework.Quaternion" />.</param>
	/// <param name="result">The result of the quaternion multiplication as an output parameter.</param>
	public static void Multiply(ref Quaternion quaternion1, ref Quaternion quaternion2, out Quaternion result)
	{
		float x = quaternion1.X;
		float y = quaternion1.Y;
		float z = quaternion1.Z;
		float w = quaternion1.W;
		float num4 = quaternion2.X;
		float num5 = quaternion2.Y;
		float num6 = quaternion2.Z;
		float num7 = quaternion2.W;
		float num12 = y * num6 - z * num5;
		float num13 = z * num4 - x * num6;
		float num14 = x * num5 - y * num4;
		float num15 = x * num4 + y * num5 + z * num6;
		result.X = x * num7 + num4 * w + num12;
		result.Y = y * num7 + num5 * w + num13;
		result.Z = z * num7 + num6 * w + num14;
		result.W = w * num7 - num15;
	}

	/// <summary>
	/// Flips the sign of the all the quaternion components.
	/// </summary>
	/// <param name="quaternion">Source <see cref="T:Microsoft.Xna.Framework.Quaternion" />.</param>
	/// <returns>The result of the quaternion negation.</returns>
	public static Quaternion Negate(Quaternion quaternion)
	{
		return new Quaternion(0f - quaternion.X, 0f - quaternion.Y, 0f - quaternion.Z, 0f - quaternion.W);
	}

	/// <summary>
	/// Flips the sign of the all the quaternion components.
	/// </summary>
	/// <param name="quaternion">Source <see cref="T:Microsoft.Xna.Framework.Quaternion" />.</param>
	/// <param name="result">The result of the quaternion negation as an output parameter.</param>
	public static void Negate(ref Quaternion quaternion, out Quaternion result)
	{
		result.X = 0f - quaternion.X;
		result.Y = 0f - quaternion.Y;
		result.Z = 0f - quaternion.Z;
		result.W = 0f - quaternion.W;
	}

	/// <summary>
	/// Scales the quaternion magnitude to unit length.
	/// </summary>
	public void Normalize()
	{
		float num = 1f / (float)Math.Sqrt(this.X * this.X + this.Y * this.Y + this.Z * this.Z + this.W * this.W);
		this.X *= num;
		this.Y *= num;
		this.Z *= num;
		this.W *= num;
	}

	/// <summary>
	/// Scales the quaternion magnitude to unit length.
	/// </summary>
	/// <param name="quaternion">Source <see cref="T:Microsoft.Xna.Framework.Quaternion" />.</param>
	/// <returns>The unit length quaternion.</returns>
	public static Quaternion Normalize(Quaternion quaternion)
	{
		float num = 1f / (float)Math.Sqrt(quaternion.X * quaternion.X + quaternion.Y * quaternion.Y + quaternion.Z * quaternion.Z + quaternion.W * quaternion.W);
		Quaternion result = default(Quaternion);
		result.X = quaternion.X * num;
		result.Y = quaternion.Y * num;
		result.Z = quaternion.Z * num;
		result.W = quaternion.W * num;
		return result;
	}

	/// <summary>
	/// Scales the quaternion magnitude to unit length.
	/// </summary>
	/// <param name="quaternion">Source <see cref="T:Microsoft.Xna.Framework.Quaternion" />.</param>
	/// <param name="result">The unit length quaternion an output parameter.</param>
	public static void Normalize(ref Quaternion quaternion, out Quaternion result)
	{
		float num = 1f / (float)Math.Sqrt(quaternion.X * quaternion.X + quaternion.Y * quaternion.Y + quaternion.Z * quaternion.Z + quaternion.W * quaternion.W);
		result.X = quaternion.X * num;
		result.Y = quaternion.Y * num;
		result.Z = quaternion.Z * num;
		result.W = quaternion.W * num;
	}

	/// <summary>
	/// Returns a <see cref="T:System.String" /> representation of this <see cref="T:Microsoft.Xna.Framework.Quaternion" /> in the format:
	/// {X:[<see cref="F:Microsoft.Xna.Framework.Quaternion.X" />] Y:[<see cref="F:Microsoft.Xna.Framework.Quaternion.Y" />] Z:[<see cref="F:Microsoft.Xna.Framework.Quaternion.Z" />] W:[<see cref="F:Microsoft.Xna.Framework.Quaternion.W" />]}
	/// </summary>
	/// <returns>A <see cref="T:System.String" /> representation of this <see cref="T:Microsoft.Xna.Framework.Quaternion" />.</returns>
	public override string ToString()
	{
		return "{X:" + this.X + " Y:" + this.Y + " Z:" + this.Z + " W:" + this.W + "}";
	}

	/// <summary>
	/// Gets a <see cref="T:Microsoft.Xna.Framework.Vector4" /> representation for this object.
	/// </summary>
	/// <returns>A <see cref="T:Microsoft.Xna.Framework.Vector4" /> representation for this object.</returns>
	public Vector4 ToVector4()
	{
		return new Vector4(this.X, this.Y, this.Z, this.W);
	}

	public void Deconstruct(out float x, out float y, out float z, out float w)
	{
		x = this.X;
		y = this.Y;
		z = this.Z;
		w = this.W;
	}

	/// <summary>
	/// Adds two quaternions.
	/// </summary>
	/// <param name="quaternion1">Source <see cref="T:Microsoft.Xna.Framework.Quaternion" /> on the left of the add sign.</param>
	/// <param name="quaternion2">Source <see cref="T:Microsoft.Xna.Framework.Quaternion" /> on the right of the add sign.</param>
	/// <returns>Sum of the vectors.</returns>
	public static Quaternion operator +(Quaternion quaternion1, Quaternion quaternion2)
	{
		Quaternion quaternion3 = default(Quaternion);
		quaternion3.X = quaternion1.X + quaternion2.X;
		quaternion3.Y = quaternion1.Y + quaternion2.Y;
		quaternion3.Z = quaternion1.Z + quaternion2.Z;
		quaternion3.W = quaternion1.W + quaternion2.W;
		return quaternion3;
	}

	/// <summary>
	/// Divides a <see cref="T:Microsoft.Xna.Framework.Quaternion" /> by the other <see cref="T:Microsoft.Xna.Framework.Quaternion" />.
	/// </summary>
	/// <param name="quaternion1">Source <see cref="T:Microsoft.Xna.Framework.Quaternion" /> on the left of the div sign.</param>
	/// <param name="quaternion2">Divisor <see cref="T:Microsoft.Xna.Framework.Quaternion" /> on the right of the div sign.</param>
	/// <returns>The result of dividing the quaternions.</returns>
	public static Quaternion operator /(Quaternion quaternion1, Quaternion quaternion2)
	{
		float x = quaternion1.X;
		float y = quaternion1.Y;
		float z = quaternion1.Z;
		float w = quaternion1.W;
		float num14 = quaternion2.X * quaternion2.X + quaternion2.Y * quaternion2.Y + quaternion2.Z * quaternion2.Z + quaternion2.W * quaternion2.W;
		float num15 = 1f / num14;
		float num16 = (0f - quaternion2.X) * num15;
		float num17 = (0f - quaternion2.Y) * num15;
		float num18 = (0f - quaternion2.Z) * num15;
		float num19 = quaternion2.W * num15;
		float num20 = y * num18 - z * num17;
		float num21 = z * num16 - x * num18;
		float num22 = x * num17 - y * num16;
		float num23 = x * num16 + y * num17 + z * num18;
		Quaternion quaternion3 = default(Quaternion);
		quaternion3.X = x * num19 + num16 * w + num20;
		quaternion3.Y = y * num19 + num17 * w + num21;
		quaternion3.Z = z * num19 + num18 * w + num22;
		quaternion3.W = w * num19 - num23;
		return quaternion3;
	}

	/// <summary>
	/// Compares whether two <see cref="T:Microsoft.Xna.Framework.Quaternion" /> instances are equal.
	/// </summary>
	/// <param name="quaternion1"><see cref="T:Microsoft.Xna.Framework.Quaternion" /> instance on the left of the equal sign.</param>
	/// <param name="quaternion2"><see cref="T:Microsoft.Xna.Framework.Quaternion" /> instance on the right of the equal sign.</param>
	/// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
	public static bool operator ==(Quaternion quaternion1, Quaternion quaternion2)
	{
		if (quaternion1.X == quaternion2.X && quaternion1.Y == quaternion2.Y && quaternion1.Z == quaternion2.Z)
		{
			return quaternion1.W == quaternion2.W;
		}
		return false;
	}

	/// <summary>
	/// Compares whether two <see cref="T:Microsoft.Xna.Framework.Quaternion" /> instances are not equal.
	/// </summary>
	/// <param name="quaternion1"><see cref="T:Microsoft.Xna.Framework.Quaternion" /> instance on the left of the not equal sign.</param>
	/// <param name="quaternion2"><see cref="T:Microsoft.Xna.Framework.Quaternion" /> instance on the right of the not equal sign.</param>
	/// <returns><c>true</c> if the instances are not equal; <c>false</c> otherwise.</returns>
	public static bool operator !=(Quaternion quaternion1, Quaternion quaternion2)
	{
		if (quaternion1.X == quaternion2.X && quaternion1.Y == quaternion2.Y && quaternion1.Z == quaternion2.Z)
		{
			return quaternion1.W != quaternion2.W;
		}
		return true;
	}

	/// <summary>
	/// Multiplies two quaternions.
	/// </summary>
	/// <param name="quaternion1">Source <see cref="T:Microsoft.Xna.Framework.Quaternion" /> on the left of the mul sign.</param>
	/// <param name="quaternion2">Source <see cref="T:Microsoft.Xna.Framework.Quaternion" /> on the right of the mul sign.</param>
	/// <returns>Result of the quaternions multiplication.</returns>
	public static Quaternion operator *(Quaternion quaternion1, Quaternion quaternion2)
	{
		float x = quaternion1.X;
		float y = quaternion1.Y;
		float z = quaternion1.Z;
		float w = quaternion1.W;
		float num4 = quaternion2.X;
		float num5 = quaternion2.Y;
		float num6 = quaternion2.Z;
		float num7 = quaternion2.W;
		float num12 = y * num6 - z * num5;
		float num13 = z * num4 - x * num6;
		float num14 = x * num5 - y * num4;
		float num15 = x * num4 + y * num5 + z * num6;
		Quaternion quaternion3 = default(Quaternion);
		quaternion3.X = x * num7 + num4 * w + num12;
		quaternion3.Y = y * num7 + num5 * w + num13;
		quaternion3.Z = z * num7 + num6 * w + num14;
		quaternion3.W = w * num7 - num15;
		return quaternion3;
	}

	/// <summary>
	/// Multiplies the components of quaternion by a scalar.
	/// </summary>
	/// <param name="quaternion1">Source <see cref="T:Microsoft.Xna.Framework.Vector3" /> on the left of the mul sign.</param>
	/// <param name="scaleFactor">Scalar value on the right of the mul sign.</param>
	/// <returns>Result of the quaternion multiplication with a scalar.</returns>
	public static Quaternion operator *(Quaternion quaternion1, float scaleFactor)
	{
		Quaternion quaternion2 = default(Quaternion);
		quaternion2.X = quaternion1.X * scaleFactor;
		quaternion2.Y = quaternion1.Y * scaleFactor;
		quaternion2.Z = quaternion1.Z * scaleFactor;
		quaternion2.W = quaternion1.W * scaleFactor;
		return quaternion2;
	}

	/// <summary>
	/// Subtracts a <see cref="T:Microsoft.Xna.Framework.Quaternion" /> from a <see cref="T:Microsoft.Xna.Framework.Quaternion" />.
	/// </summary>
	/// <param name="quaternion1">Source <see cref="T:Microsoft.Xna.Framework.Vector3" /> on the left of the sub sign.</param>
	/// <param name="quaternion2">Source <see cref="T:Microsoft.Xna.Framework.Vector3" /> on the right of the sub sign.</param>
	/// <returns>Result of the quaternion subtraction.</returns>
	public static Quaternion operator -(Quaternion quaternion1, Quaternion quaternion2)
	{
		Quaternion quaternion3 = default(Quaternion);
		quaternion3.X = quaternion1.X - quaternion2.X;
		quaternion3.Y = quaternion1.Y - quaternion2.Y;
		quaternion3.Z = quaternion1.Z - quaternion2.Z;
		quaternion3.W = quaternion1.W - quaternion2.W;
		return quaternion3;
	}

	/// <summary>
	/// Flips the sign of the all the quaternion components.
	/// </summary>
	/// <param name="quaternion">Source <see cref="T:Microsoft.Xna.Framework.Quaternion" /> on the right of the sub sign.</param>
	/// <returns>The result of the quaternion negation.</returns>
	public static Quaternion operator -(Quaternion quaternion)
	{
		Quaternion quaternion2 = default(Quaternion);
		quaternion2.X = 0f - quaternion.X;
		quaternion2.Y = 0f - quaternion.Y;
		quaternion2.Z = 0f - quaternion.Z;
		quaternion2.W = 0f - quaternion.W;
		return quaternion2;
	}
}
