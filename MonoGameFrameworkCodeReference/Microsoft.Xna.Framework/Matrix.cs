using System;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Microsoft.Xna.Framework;

/// <summary>
/// Represents the right-handed 4x4 floating point matrix, which can store translation, scale and rotation information.
/// </summary>
[DataContract]
[DebuggerDisplay("{DebugDisplayString,nq}")]
public struct Matrix : IEquatable<Matrix>
{
	/// <summary>
	/// A first row and first column value.
	/// </summary>
	[DataMember]
	public float M11;

	/// <summary>
	/// A first row and second column value.
	/// </summary>
	[DataMember]
	public float M12;

	/// <summary>
	/// A first row and third column value.
	/// </summary>
	[DataMember]
	public float M13;

	/// <summary>
	/// A first row and fourth column value.
	/// </summary>
	[DataMember]
	public float M14;

	/// <summary>
	/// A second row and first column value.
	/// </summary>
	[DataMember]
	public float M21;

	/// <summary>
	/// A second row and second column value.
	/// </summary>
	[DataMember]
	public float M22;

	/// <summary>
	/// A second row and third column value.
	/// </summary>
	[DataMember]
	public float M23;

	/// <summary>
	/// A second row and fourth column value.
	/// </summary>
	[DataMember]
	public float M24;

	/// <summary>
	/// A third row and first column value.
	/// </summary>
	[DataMember]
	public float M31;

	/// <summary>
	/// A third row and second column value.
	/// </summary>
	[DataMember]
	public float M32;

	/// <summary>
	/// A third row and third column value.
	/// </summary>
	[DataMember]
	public float M33;

	/// <summary>
	/// A third row and fourth column value.
	/// </summary>
	[DataMember]
	public float M34;

	/// <summary>
	/// A fourth row and first column value.
	/// </summary>
	[DataMember]
	public float M41;

	/// <summary>
	/// A fourth row and second column value.
	/// </summary>
	[DataMember]
	public float M42;

	/// <summary>
	/// A fourth row and third column value.
	/// </summary>
	[DataMember]
	public float M43;

	/// <summary>
	/// A fourth row and fourth column value.
	/// </summary>
	[DataMember]
	public float M44;

	private static Matrix identity = new Matrix(1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f);

	/// <summary>
	/// Get or set the matrix element at the given index, indexed in row major order.
	/// </summary>
	/// <param name="index">The linearized, zero-based index of the matrix element.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	/// If the index is less than <code>0</code> or larger than <code>15</code>.
	/// </exception>
	public float this[int index]
	{
		get
		{
			return index switch
			{
				0 => this.M11, 
				1 => this.M12, 
				2 => this.M13, 
				3 => this.M14, 
				4 => this.M21, 
				5 => this.M22, 
				6 => this.M23, 
				7 => this.M24, 
				8 => this.M31, 
				9 => this.M32, 
				10 => this.M33, 
				11 => this.M34, 
				12 => this.M41, 
				13 => this.M42, 
				14 => this.M43, 
				15 => this.M44, 
				_ => throw new ArgumentOutOfRangeException(), 
			};
		}
		set
		{
			switch (index)
			{
			case 0:
				this.M11 = value;
				break;
			case 1:
				this.M12 = value;
				break;
			case 2:
				this.M13 = value;
				break;
			case 3:
				this.M14 = value;
				break;
			case 4:
				this.M21 = value;
				break;
			case 5:
				this.M22 = value;
				break;
			case 6:
				this.M23 = value;
				break;
			case 7:
				this.M24 = value;
				break;
			case 8:
				this.M31 = value;
				break;
			case 9:
				this.M32 = value;
				break;
			case 10:
				this.M33 = value;
				break;
			case 11:
				this.M34 = value;
				break;
			case 12:
				this.M41 = value;
				break;
			case 13:
				this.M42 = value;
				break;
			case 14:
				this.M43 = value;
				break;
			case 15:
				this.M44 = value;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}
	}

	/// <summary>
	/// Get or set the value at the specified row and column (indices are zero-based).
	/// </summary>
	/// <param name="row">The row of the element.</param>
	/// <param name="column">The column of the element.</param>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	/// If the row or column is less than <code>0</code> or larger than <code>3</code>.
	/// </exception>
	public float this[int row, int column]
	{
		get
		{
			return this[row * 4 + column];
		}
		set
		{
			this[row * 4 + column] = value;
		}
	}

	/// <summary>
	/// The backward vector formed from the third row M31, M32, M33 elements.
	/// </summary>
	public Vector3 Backward
	{
		get
		{
			return new Vector3(this.M31, this.M32, this.M33);
		}
		set
		{
			this.M31 = value.X;
			this.M32 = value.Y;
			this.M33 = value.Z;
		}
	}

	/// <summary>
	/// The down vector formed from the second row -M21, -M22, -M23 elements.
	/// </summary>
	public Vector3 Down
	{
		get
		{
			return new Vector3(0f - this.M21, 0f - this.M22, 0f - this.M23);
		}
		set
		{
			this.M21 = 0f - value.X;
			this.M22 = 0f - value.Y;
			this.M23 = 0f - value.Z;
		}
	}

	/// <summary>
	/// The forward vector formed from the third row -M31, -M32, -M33 elements.
	/// </summary>
	public Vector3 Forward
	{
		get
		{
			return new Vector3(0f - this.M31, 0f - this.M32, 0f - this.M33);
		}
		set
		{
			this.M31 = 0f - value.X;
			this.M32 = 0f - value.Y;
			this.M33 = 0f - value.Z;
		}
	}

	/// <summary>
	/// Returns the identity matrix.
	/// </summary>
	public static Matrix Identity => Matrix.identity;

	/// <summary>
	/// The left vector formed from the first row -M11, -M12, -M13 elements.
	/// </summary>
	public Vector3 Left
	{
		get
		{
			return new Vector3(0f - this.M11, 0f - this.M12, 0f - this.M13);
		}
		set
		{
			this.M11 = 0f - value.X;
			this.M12 = 0f - value.Y;
			this.M13 = 0f - value.Z;
		}
	}

	/// <summary>
	/// The right vector formed from the first row M11, M12, M13 elements.
	/// </summary>
	public Vector3 Right
	{
		get
		{
			return new Vector3(this.M11, this.M12, this.M13);
		}
		set
		{
			this.M11 = value.X;
			this.M12 = value.Y;
			this.M13 = value.Z;
		}
	}

	/// <summary>
	/// Position stored in this matrix.
	/// </summary>
	public Vector3 Translation
	{
		get
		{
			return new Vector3(this.M41, this.M42, this.M43);
		}
		set
		{
			this.M41 = value.X;
			this.M42 = value.Y;
			this.M43 = value.Z;
		}
	}

	/// <summary>
	/// The upper vector formed from the second row M21, M22, M23 elements.
	/// </summary>
	public Vector3 Up
	{
		get
		{
			return new Vector3(this.M21, this.M22, this.M23);
		}
		set
		{
			this.M21 = value.X;
			this.M22 = value.Y;
			this.M23 = value.Z;
		}
	}

	internal string DebugDisplayString
	{
		get
		{
			if (this == Matrix.Identity)
			{
				return "Identity";
			}
			return "( " + this.M11 + "  " + this.M12 + "  " + this.M13 + "  " + this.M14 + " )  \r\n" + "( " + this.M21 + "  " + this.M22 + "  " + this.M23 + "  " + this.M24 + " )  \r\n" + "( " + this.M31 + "  " + this.M32 + "  " + this.M33 + "  " + this.M34 + " )  \r\n" + "( " + this.M41 + "  " + this.M42 + "  " + this.M43 + "  " + this.M44 + " )";
		}
	}

	/// <summary>
	/// Constructs a matrix.
	/// </summary>
	/// <param name="m11">A first row and first column value.</param>
	/// <param name="m12">A first row and second column value.</param>
	/// <param name="m13">A first row and third column value.</param>
	/// <param name="m14">A first row and fourth column value.</param>
	/// <param name="m21">A second row and first column value.</param>
	/// <param name="m22">A second row and second column value.</param>
	/// <param name="m23">A second row and third column value.</param>
	/// <param name="m24">A second row and fourth column value.</param>
	/// <param name="m31">A third row and first column value.</param>
	/// <param name="m32">A third row and second column value.</param>
	/// <param name="m33">A third row and third column value.</param>
	/// <param name="m34">A third row and fourth column value.</param>
	/// <param name="m41">A fourth row and first column value.</param>
	/// <param name="m42">A fourth row and second column value.</param>
	/// <param name="m43">A fourth row and third column value.</param>
	/// <param name="m44">A fourth row and fourth column value.</param>
	public Matrix(float m11, float m12, float m13, float m14, float m21, float m22, float m23, float m24, float m31, float m32, float m33, float m34, float m41, float m42, float m43, float m44)
	{
		this.M11 = m11;
		this.M12 = m12;
		this.M13 = m13;
		this.M14 = m14;
		this.M21 = m21;
		this.M22 = m22;
		this.M23 = m23;
		this.M24 = m24;
		this.M31 = m31;
		this.M32 = m32;
		this.M33 = m33;
		this.M34 = m34;
		this.M41 = m41;
		this.M42 = m42;
		this.M43 = m43;
		this.M44 = m44;
	}

	/// <summary>
	/// Constructs a matrix.
	/// </summary>
	/// <param name="row1">A first row of the created matrix.</param>
	/// <param name="row2">A second row of the created matrix.</param>
	/// <param name="row3">A third row of the created matrix.</param>
	/// <param name="row4">A fourth row of the created matrix.</param>
	public Matrix(Vector4 row1, Vector4 row2, Vector4 row3, Vector4 row4)
	{
		this.M11 = row1.X;
		this.M12 = row1.Y;
		this.M13 = row1.Z;
		this.M14 = row1.W;
		this.M21 = row2.X;
		this.M22 = row2.Y;
		this.M23 = row2.Z;
		this.M24 = row2.W;
		this.M31 = row3.X;
		this.M32 = row3.Y;
		this.M33 = row3.Z;
		this.M34 = row3.W;
		this.M41 = row4.X;
		this.M42 = row4.Y;
		this.M43 = row4.Z;
		this.M44 = row4.W;
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Matrix" /> which contains sum of two matrixes.
	/// </summary>
	/// <param name="matrix1">The first matrix to add.</param>
	/// <param name="matrix2">The second matrix to add.</param>
	/// <returns>The result of the matrix addition.</returns>
	public static Matrix Add(Matrix matrix1, Matrix matrix2)
	{
		matrix1.M11 += matrix2.M11;
		matrix1.M12 += matrix2.M12;
		matrix1.M13 += matrix2.M13;
		matrix1.M14 += matrix2.M14;
		matrix1.M21 += matrix2.M21;
		matrix1.M22 += matrix2.M22;
		matrix1.M23 += matrix2.M23;
		matrix1.M24 += matrix2.M24;
		matrix1.M31 += matrix2.M31;
		matrix1.M32 += matrix2.M32;
		matrix1.M33 += matrix2.M33;
		matrix1.M34 += matrix2.M34;
		matrix1.M41 += matrix2.M41;
		matrix1.M42 += matrix2.M42;
		matrix1.M43 += matrix2.M43;
		matrix1.M44 += matrix2.M44;
		return matrix1;
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Matrix" /> which contains sum of two matrixes.
	/// </summary>
	/// <param name="matrix1">The first matrix to add.</param>
	/// <param name="matrix2">The second matrix to add.</param>
	/// <param name="result">The result of the matrix addition as an output parameter.</param>
	public static void Add(ref Matrix matrix1, ref Matrix matrix2, out Matrix result)
	{
		result.M11 = matrix1.M11 + matrix2.M11;
		result.M12 = matrix1.M12 + matrix2.M12;
		result.M13 = matrix1.M13 + matrix2.M13;
		result.M14 = matrix1.M14 + matrix2.M14;
		result.M21 = matrix1.M21 + matrix2.M21;
		result.M22 = matrix1.M22 + matrix2.M22;
		result.M23 = matrix1.M23 + matrix2.M23;
		result.M24 = matrix1.M24 + matrix2.M24;
		result.M31 = matrix1.M31 + matrix2.M31;
		result.M32 = matrix1.M32 + matrix2.M32;
		result.M33 = matrix1.M33 + matrix2.M33;
		result.M34 = matrix1.M34 + matrix2.M34;
		result.M41 = matrix1.M41 + matrix2.M41;
		result.M42 = matrix1.M42 + matrix2.M42;
		result.M43 = matrix1.M43 + matrix2.M43;
		result.M44 = matrix1.M44 + matrix2.M44;
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Matrix" /> for spherical billboarding that rotates around specified object position.
	/// </summary>
	/// <param name="objectPosition">Position of billboard object. It will rotate around that vector.</param>
	/// <param name="cameraPosition">The camera position.</param>
	/// <param name="cameraUpVector">The camera up vector.</param>
	/// <param name="cameraForwardVector">Optional camera forward vector.</param>
	/// <returns>The <see cref="T:Microsoft.Xna.Framework.Matrix" /> for spherical billboarding.</returns>
	public static Matrix CreateBillboard(Vector3 objectPosition, Vector3 cameraPosition, Vector3 cameraUpVector, Vector3? cameraForwardVector)
	{
		Matrix.CreateBillboard(ref objectPosition, ref cameraPosition, ref cameraUpVector, cameraForwardVector, out var result);
		return result;
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Matrix" /> for spherical billboarding that rotates around specified object position.
	/// </summary>
	/// <param name="objectPosition">Position of billboard object. It will rotate around that vector.</param>
	/// <param name="cameraPosition">The camera position.</param>
	/// <param name="cameraUpVector">The camera up vector.</param>
	/// <param name="cameraForwardVector">Optional camera forward vector.</param>
	/// <param name="result">The <see cref="T:Microsoft.Xna.Framework.Matrix" /> for spherical billboarding as an output parameter.</param>
	public static void CreateBillboard(ref Vector3 objectPosition, ref Vector3 cameraPosition, ref Vector3 cameraUpVector, Vector3? cameraForwardVector, out Matrix result)
	{
		Vector3 vector = default(Vector3);
		vector.X = objectPosition.X - cameraPosition.X;
		vector.Y = objectPosition.Y - cameraPosition.Y;
		vector.Z = objectPosition.Z - cameraPosition.Z;
		float num = vector.LengthSquared();
		if (num < 0.0001f)
		{
			vector = (cameraForwardVector.HasValue ? (-cameraForwardVector.Value) : Vector3.Forward);
		}
		else
		{
			Vector3.Multiply(ref vector, 1f / (float)Math.Sqrt(num), out vector);
		}
		Vector3.Cross(ref cameraUpVector, ref vector, out var vector3);
		vector3.Normalize();
		Vector3.Cross(ref vector, ref vector3, out var vector4);
		result.M11 = vector3.X;
		result.M12 = vector3.Y;
		result.M13 = vector3.Z;
		result.M14 = 0f;
		result.M21 = vector4.X;
		result.M22 = vector4.Y;
		result.M23 = vector4.Z;
		result.M24 = 0f;
		result.M31 = vector.X;
		result.M32 = vector.Y;
		result.M33 = vector.Z;
		result.M34 = 0f;
		result.M41 = objectPosition.X;
		result.M42 = objectPosition.Y;
		result.M43 = objectPosition.Z;
		result.M44 = 1f;
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Matrix" /> for cylindrical billboarding that rotates around specified axis.
	/// </summary>
	/// <param name="objectPosition">Object position the billboard will rotate around.</param>
	/// <param name="cameraPosition">Camera position.</param>
	/// <param name="rotateAxis">Axis of billboard for rotation.</param>
	/// <param name="cameraForwardVector">Optional camera forward vector.</param>
	/// <param name="objectForwardVector">Optional object forward vector.</param>
	/// <returns>The <see cref="T:Microsoft.Xna.Framework.Matrix" /> for cylindrical billboarding.</returns>
	public static Matrix CreateConstrainedBillboard(Vector3 objectPosition, Vector3 cameraPosition, Vector3 rotateAxis, Vector3? cameraForwardVector, Vector3? objectForwardVector)
	{
		Matrix.CreateConstrainedBillboard(ref objectPosition, ref cameraPosition, ref rotateAxis, cameraForwardVector, objectForwardVector, out var result);
		return result;
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Matrix" /> for cylindrical billboarding that rotates around specified axis.
	/// </summary>
	/// <param name="objectPosition">Object position the billboard will rotate around.</param>
	/// <param name="cameraPosition">Camera position.</param>
	/// <param name="rotateAxis">Axis of billboard for rotation.</param>
	/// <param name="cameraForwardVector">Optional camera forward vector.</param>
	/// <param name="objectForwardVector">Optional object forward vector.</param>
	/// <param name="result">The <see cref="T:Microsoft.Xna.Framework.Matrix" /> for cylindrical billboarding as an output parameter.</param>
	public static void CreateConstrainedBillboard(ref Vector3 objectPosition, ref Vector3 cameraPosition, ref Vector3 rotateAxis, Vector3? cameraForwardVector, Vector3? objectForwardVector, out Matrix result)
	{
		Vector3 vector2 = default(Vector3);
		vector2.X = objectPosition.X - cameraPosition.X;
		vector2.Y = objectPosition.Y - cameraPosition.Y;
		vector2.Z = objectPosition.Z - cameraPosition.Z;
		float num2 = vector2.LengthSquared();
		if (num2 < 0.0001f)
		{
			vector2 = (cameraForwardVector.HasValue ? (-cameraForwardVector.Value) : Vector3.Forward);
		}
		else
		{
			Vector3.Multiply(ref vector2, 1f / (float)Math.Sqrt(num2), out vector2);
		}
		Vector3 vector4 = rotateAxis;
		Vector3.Dot(ref rotateAxis, ref vector2, out var num3);
		Vector3 vector5;
		Vector3 vector6;
		if (Math.Abs(num3) > 0.9982547f)
		{
			if (objectForwardVector.HasValue)
			{
				vector5 = objectForwardVector.Value;
				Vector3.Dot(ref rotateAxis, ref vector5, out num3);
				if (Math.Abs(num3) > 0.9982547f)
				{
					num3 = rotateAxis.X * Vector3.Forward.X + rotateAxis.Y * Vector3.Forward.Y + rotateAxis.Z * Vector3.Forward.Z;
					vector5 = ((Math.Abs(num3) > 0.9982547f) ? Vector3.Right : Vector3.Forward);
				}
			}
			else
			{
				num3 = rotateAxis.X * Vector3.Forward.X + rotateAxis.Y * Vector3.Forward.Y + rotateAxis.Z * Vector3.Forward.Z;
				vector5 = ((Math.Abs(num3) > 0.9982547f) ? Vector3.Right : Vector3.Forward);
			}
			Vector3.Cross(ref rotateAxis, ref vector5, out vector6);
			vector6.Normalize();
			Vector3.Cross(ref vector6, ref rotateAxis, out vector5);
			vector5.Normalize();
		}
		else
		{
			Vector3.Cross(ref rotateAxis, ref vector2, out vector6);
			vector6.Normalize();
			Vector3.Cross(ref vector6, ref vector4, out vector5);
			vector5.Normalize();
		}
		result.M11 = vector6.X;
		result.M12 = vector6.Y;
		result.M13 = vector6.Z;
		result.M14 = 0f;
		result.M21 = vector4.X;
		result.M22 = vector4.Y;
		result.M23 = vector4.Z;
		result.M24 = 0f;
		result.M31 = vector5.X;
		result.M32 = vector5.Y;
		result.M33 = vector5.Z;
		result.M34 = 0f;
		result.M41 = objectPosition.X;
		result.M42 = objectPosition.Y;
		result.M43 = objectPosition.Z;
		result.M44 = 1f;
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Matrix" /> which contains the rotation moment around specified axis.
	/// </summary>
	/// <param name="axis">The axis of rotation.</param>
	/// <param name="angle">The angle of rotation in radians.</param>
	/// <returns>The rotation <see cref="T:Microsoft.Xna.Framework.Matrix" />.</returns>
	public static Matrix CreateFromAxisAngle(Vector3 axis, float angle)
	{
		Matrix.CreateFromAxisAngle(ref axis, angle, out var result);
		return result;
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Matrix" /> which contains the rotation moment around specified axis.
	/// </summary>
	/// <param name="axis">The axis of rotation.</param>
	/// <param name="angle">The angle of rotation in radians.</param>
	/// <param name="result">The rotation <see cref="T:Microsoft.Xna.Framework.Matrix" /> as an output parameter.</param>
	public static void CreateFromAxisAngle(ref Vector3 axis, float angle, out Matrix result)
	{
		float x = axis.X;
		float y = axis.Y;
		float z = axis.Z;
		float num2 = (float)Math.Sin(angle);
		float num3 = (float)Math.Cos(angle);
		float num11 = x * x;
		float num12 = y * y;
		float num13 = z * z;
		float num14 = x * y;
		float num15 = x * z;
		float num16 = y * z;
		result.M11 = num11 + num3 * (1f - num11);
		result.M12 = num14 - num3 * num14 + num2 * z;
		result.M13 = num15 - num3 * num15 - num2 * y;
		result.M14 = 0f;
		result.M21 = num14 - num3 * num14 - num2 * z;
		result.M22 = num12 + num3 * (1f - num12);
		result.M23 = num16 - num3 * num16 + num2 * x;
		result.M24 = 0f;
		result.M31 = num15 - num3 * num15 + num2 * y;
		result.M32 = num16 - num3 * num16 - num2 * x;
		result.M33 = num13 + num3 * (1f - num13);
		result.M34 = 0f;
		result.M41 = 0f;
		result.M42 = 0f;
		result.M43 = 0f;
		result.M44 = 1f;
	}

	/// <summary>
	/// Creates a new rotation <see cref="T:Microsoft.Xna.Framework.Matrix" /> from a <see cref="T:Microsoft.Xna.Framework.Quaternion" />.
	/// </summary>
	/// <param name="quaternion"><see cref="T:Microsoft.Xna.Framework.Quaternion" /> of rotation moment.</param>
	/// <returns>The rotation <see cref="T:Microsoft.Xna.Framework.Matrix" />.</returns>
	public static Matrix CreateFromQuaternion(Quaternion quaternion)
	{
		Matrix.CreateFromQuaternion(ref quaternion, out var result);
		return result;
	}

	/// <summary>
	/// Creates a new rotation <see cref="T:Microsoft.Xna.Framework.Matrix" /> from a <see cref="T:Microsoft.Xna.Framework.Quaternion" />.
	/// </summary>
	/// <param name="quaternion"><see cref="T:Microsoft.Xna.Framework.Quaternion" /> of rotation moment.</param>
	/// <param name="result">The rotation <see cref="T:Microsoft.Xna.Framework.Matrix" /> as an output parameter.</param>
	public static void CreateFromQuaternion(ref Quaternion quaternion, out Matrix result)
	{
		float num9 = quaternion.X * quaternion.X;
		float num10 = quaternion.Y * quaternion.Y;
		float num11 = quaternion.Z * quaternion.Z;
		float num12 = quaternion.X * quaternion.Y;
		float num13 = quaternion.Z * quaternion.W;
		float num14 = quaternion.Z * quaternion.X;
		float num15 = quaternion.Y * quaternion.W;
		float num16 = quaternion.Y * quaternion.Z;
		float num17 = quaternion.X * quaternion.W;
		result.M11 = 1f - 2f * (num10 + num11);
		result.M12 = 2f * (num12 + num13);
		result.M13 = 2f * (num14 - num15);
		result.M14 = 0f;
		result.M21 = 2f * (num12 - num13);
		result.M22 = 1f - 2f * (num11 + num9);
		result.M23 = 2f * (num16 + num17);
		result.M24 = 0f;
		result.M31 = 2f * (num14 + num15);
		result.M32 = 2f * (num16 - num17);
		result.M33 = 1f - 2f * (num10 + num9);
		result.M34 = 0f;
		result.M41 = 0f;
		result.M42 = 0f;
		result.M43 = 0f;
		result.M44 = 1f;
	}

	/// <summary>
	/// Creates a new rotation <see cref="T:Microsoft.Xna.Framework.Matrix" /> from the specified yaw, pitch and roll values.
	/// </summary>
	/// <param name="yaw">The yaw rotation value in radians.</param>
	/// <param name="pitch">The pitch rotation value in radians.</param>
	/// <param name="roll">The roll rotation value in radians.</param>
	/// <returns>The rotation <see cref="T:Microsoft.Xna.Framework.Matrix" />.</returns>
	/// <remarks>For more information about yaw, pitch and roll visit http://en.wikipedia.org/wiki/Euler_angles.
	/// </remarks>
	public static Matrix CreateFromYawPitchRoll(float yaw, float pitch, float roll)
	{
		Matrix.CreateFromYawPitchRoll(yaw, pitch, roll, out var matrix);
		return matrix;
	}

	/// <summary>
	/// Creates a new rotation <see cref="T:Microsoft.Xna.Framework.Matrix" /> from the specified yaw, pitch and roll values.
	/// </summary>
	/// <param name="yaw">The yaw rotation value in radians.</param>
	/// <param name="pitch">The pitch rotation value in radians.</param>
	/// <param name="roll">The roll rotation value in radians.</param>
	/// <param name="result">The rotation <see cref="T:Microsoft.Xna.Framework.Matrix" /> as an output parameter.</param>
	/// <remarks>For more information about yaw, pitch and roll visit http://en.wikipedia.org/wiki/Euler_angles.
	/// </remarks>
	public static void CreateFromYawPitchRoll(float yaw, float pitch, float roll, out Matrix result)
	{
		Quaternion.CreateFromYawPitchRoll(yaw, pitch, roll, out var quaternion);
		Matrix.CreateFromQuaternion(ref quaternion, out result);
	}

	/// <summary>
	/// Creates a new viewing <see cref="T:Microsoft.Xna.Framework.Matrix" />.
	/// </summary>
	/// <param name="cameraPosition">Position of the camera.</param>
	/// <param name="cameraTarget">Lookup vector of the camera.</param>
	/// <param name="cameraUpVector">The direction of the upper edge of the camera.</param>
	/// <returns>The viewing <see cref="T:Microsoft.Xna.Framework.Matrix" />.</returns>
	public static Matrix CreateLookAt(Vector3 cameraPosition, Vector3 cameraTarget, Vector3 cameraUpVector)
	{
		Matrix.CreateLookAt(ref cameraPosition, ref cameraTarget, ref cameraUpVector, out var matrix);
		return matrix;
	}

	/// <summary>
	/// Creates a new viewing <see cref="T:Microsoft.Xna.Framework.Matrix" />.
	/// </summary>
	/// <param name="cameraPosition">Position of the camera.</param>
	/// <param name="cameraTarget">Lookup vector of the camera.</param>
	/// <param name="cameraUpVector">The direction of the upper edge of the camera.</param>
	/// <param name="result">The viewing <see cref="T:Microsoft.Xna.Framework.Matrix" /> as an output parameter.</param>
	public static void CreateLookAt(ref Vector3 cameraPosition, ref Vector3 cameraTarget, ref Vector3 cameraUpVector, out Matrix result)
	{
		Vector3 vector = Vector3.Normalize(cameraPosition - cameraTarget);
		Vector3 vector2 = Vector3.Normalize(Vector3.Cross(cameraUpVector, vector));
		Vector3 vector3 = Vector3.Cross(vector, vector2);
		result.M11 = vector2.X;
		result.M12 = vector3.X;
		result.M13 = vector.X;
		result.M14 = 0f;
		result.M21 = vector2.Y;
		result.M22 = vector3.Y;
		result.M23 = vector.Y;
		result.M24 = 0f;
		result.M31 = vector2.Z;
		result.M32 = vector3.Z;
		result.M33 = vector.Z;
		result.M34 = 0f;
		result.M41 = 0f - Vector3.Dot(vector2, cameraPosition);
		result.M42 = 0f - Vector3.Dot(vector3, cameraPosition);
		result.M43 = 0f - Vector3.Dot(vector, cameraPosition);
		result.M44 = 1f;
	}

	/// <summary>
	/// Creates a new projection <see cref="T:Microsoft.Xna.Framework.Matrix" /> for orthographic view.
	/// </summary>
	/// <param name="width">Width of the viewing volume.</param>
	/// <param name="height">Height of the viewing volume.</param>
	/// <param name="zNearPlane">Depth of the near plane.</param>
	/// <param name="zFarPlane">Depth of the far plane.</param>
	/// <returns>The new projection <see cref="T:Microsoft.Xna.Framework.Matrix" /> for orthographic view.</returns>
	public static Matrix CreateOrthographic(float width, float height, float zNearPlane, float zFarPlane)
	{
		Matrix.CreateOrthographic(width, height, zNearPlane, zFarPlane, out var matrix);
		return matrix;
	}

	/// <summary>
	/// Creates a new projection <see cref="T:Microsoft.Xna.Framework.Matrix" /> for orthographic view.
	/// </summary>
	/// <param name="width">Width of the viewing volume.</param>
	/// <param name="height">Height of the viewing volume.</param>
	/// <param name="zNearPlane">Depth of the near plane.</param>
	/// <param name="zFarPlane">Depth of the far plane.</param>
	/// <param name="result">The new projection <see cref="T:Microsoft.Xna.Framework.Matrix" /> for orthographic view as an output parameter.</param>
	public static void CreateOrthographic(float width, float height, float zNearPlane, float zFarPlane, out Matrix result)
	{
		result.M11 = 2f / width;
		result.M12 = (result.M13 = (result.M14 = 0f));
		result.M22 = 2f / height;
		result.M21 = (result.M23 = (result.M24 = 0f));
		result.M33 = 1f / (zNearPlane - zFarPlane);
		result.M31 = (result.M32 = (result.M34 = 0f));
		result.M41 = (result.M42 = 0f);
		result.M43 = zNearPlane / (zNearPlane - zFarPlane);
		result.M44 = 1f;
	}

	/// <summary>
	/// Creates a new projection <see cref="T:Microsoft.Xna.Framework.Matrix" /> for customized orthographic view.
	/// </summary>
	/// <param name="left">Lower x-value at the near plane.</param>
	/// <param name="right">Upper x-value at the near plane.</param>
	/// <param name="bottom">Lower y-coordinate at the near plane.</param>
	/// <param name="top">Upper y-value at the near plane.</param>
	/// <param name="zNearPlane">Depth of the near plane.</param>
	/// <param name="zFarPlane">Depth of the far plane.</param>
	/// <returns>The new projection <see cref="T:Microsoft.Xna.Framework.Matrix" /> for customized orthographic view.</returns>
	public static Matrix CreateOrthographicOffCenter(float left, float right, float bottom, float top, float zNearPlane, float zFarPlane)
	{
		Matrix.CreateOrthographicOffCenter(left, right, bottom, top, zNearPlane, zFarPlane, out var matrix);
		return matrix;
	}

	/// <summary>
	/// Creates a new projection <see cref="T:Microsoft.Xna.Framework.Matrix" /> for customized orthographic view.
	/// </summary>
	/// <param name="viewingVolume">The viewing volume.</param>
	/// <param name="zNearPlane">Depth of the near plane.</param>
	/// <param name="zFarPlane">Depth of the far plane.</param>
	/// <returns>The new projection <see cref="T:Microsoft.Xna.Framework.Matrix" /> for customized orthographic view.</returns>
	public static Matrix CreateOrthographicOffCenter(Rectangle viewingVolume, float zNearPlane, float zFarPlane)
	{
		Matrix.CreateOrthographicOffCenter(viewingVolume.Left, viewingVolume.Right, viewingVolume.Bottom, viewingVolume.Top, zNearPlane, zFarPlane, out var matrix);
		return matrix;
	}

	/// <summary>
	/// Creates a new projection <see cref="T:Microsoft.Xna.Framework.Matrix" /> for customized orthographic view.
	/// </summary>
	/// <param name="left">Lower x-value at the near plane.</param>
	/// <param name="right">Upper x-value at the near plane.</param>
	/// <param name="bottom">Lower y-coordinate at the near plane.</param>
	/// <param name="top">Upper y-value at the near plane.</param>
	/// <param name="zNearPlane">Depth of the near plane.</param>
	/// <param name="zFarPlane">Depth of the far plane.</param>
	/// <param name="result">The new projection <see cref="T:Microsoft.Xna.Framework.Matrix" /> for customized orthographic view as an output parameter.</param>
	public static void CreateOrthographicOffCenter(float left, float right, float bottom, float top, float zNearPlane, float zFarPlane, out Matrix result)
	{
		result.M11 = (float)(2.0 / ((double)right - (double)left));
		result.M12 = 0f;
		result.M13 = 0f;
		result.M14 = 0f;
		result.M21 = 0f;
		result.M22 = (float)(2.0 / ((double)top - (double)bottom));
		result.M23 = 0f;
		result.M24 = 0f;
		result.M31 = 0f;
		result.M32 = 0f;
		result.M33 = (float)(1.0 / ((double)zNearPlane - (double)zFarPlane));
		result.M34 = 0f;
		result.M41 = (float)(((double)left + (double)right) / ((double)left - (double)right));
		result.M42 = (float)(((double)top + (double)bottom) / ((double)bottom - (double)top));
		result.M43 = (float)((double)zNearPlane / ((double)zNearPlane - (double)zFarPlane));
		result.M44 = 1f;
	}

	/// <summary>
	/// Creates a new projection <see cref="T:Microsoft.Xna.Framework.Matrix" /> for perspective view.
	/// </summary>
	/// <param name="width">Width of the viewing volume.</param>
	/// <param name="height">Height of the viewing volume.</param>
	/// <param name="nearPlaneDistance">Distance to the near plane.</param>
	/// <param name="farPlaneDistance">Distance to the far plane.</param>
	/// <returns>The new projection <see cref="T:Microsoft.Xna.Framework.Matrix" /> for perspective view.</returns>
	public static Matrix CreatePerspective(float width, float height, float nearPlaneDistance, float farPlaneDistance)
	{
		Matrix.CreatePerspective(width, height, nearPlaneDistance, farPlaneDistance, out var matrix);
		return matrix;
	}

	/// <summary>
	/// Creates a new projection <see cref="T:Microsoft.Xna.Framework.Matrix" /> for perspective view.
	/// </summary>
	/// <param name="width">Width of the viewing volume.</param>
	/// <param name="height">Height of the viewing volume.</param>
	/// <param name="nearPlaneDistance">Distance to the near plane.</param>
	/// <param name="farPlaneDistance">Distance to the far plane.</param>
	/// <param name="result">The new projection <see cref="T:Microsoft.Xna.Framework.Matrix" /> for perspective view as an output parameter.</param>
	public static void CreatePerspective(float width, float height, float nearPlaneDistance, float farPlaneDistance, out Matrix result)
	{
		if (nearPlaneDistance <= 0f)
		{
			throw new ArgumentException("nearPlaneDistance <= 0");
		}
		if (farPlaneDistance <= 0f)
		{
			throw new ArgumentException("farPlaneDistance <= 0");
		}
		if (nearPlaneDistance >= farPlaneDistance)
		{
			throw new ArgumentException("nearPlaneDistance >= farPlaneDistance");
		}
		result.M11 = 2f * nearPlaneDistance / width;
		result.M12 = (result.M13 = (result.M14 = 0f));
		result.M22 = 2f * nearPlaneDistance / height;
		result.M21 = (result.M23 = (result.M24 = 0f));
		result.M33 = farPlaneDistance / (nearPlaneDistance - farPlaneDistance);
		result.M31 = (result.M32 = 0f);
		result.M34 = -1f;
		result.M41 = (result.M42 = (result.M44 = 0f));
		result.M43 = nearPlaneDistance * farPlaneDistance / (nearPlaneDistance - farPlaneDistance);
	}

	/// <summary>
	/// Creates a new projection <see cref="T:Microsoft.Xna.Framework.Matrix" /> for perspective view with field of view.
	/// </summary>
	/// <param name="fieldOfView">Field of view in the y direction in radians.</param>
	/// <param name="aspectRatio">Width divided by height of the viewing volume.</param>
	/// <param name="nearPlaneDistance">Distance to the near plane.</param>
	/// <param name="farPlaneDistance">Distance to the far plane.</param>
	/// <returns>The new projection <see cref="T:Microsoft.Xna.Framework.Matrix" /> for perspective view with FOV.</returns>
	public static Matrix CreatePerspectiveFieldOfView(float fieldOfView, float aspectRatio, float nearPlaneDistance, float farPlaneDistance)
	{
		Matrix.CreatePerspectiveFieldOfView(fieldOfView, aspectRatio, nearPlaneDistance, farPlaneDistance, out var result);
		return result;
	}

	/// <summary>
	/// Creates a new projection <see cref="T:Microsoft.Xna.Framework.Matrix" /> for perspective view with field of view.
	/// </summary>
	/// <param name="fieldOfView">Field of view in the y direction in radians.</param>
	/// <param name="aspectRatio">Width divided by height of the viewing volume.</param>
	/// <param name="nearPlaneDistance">Distance of the near plane.</param>
	/// <param name="farPlaneDistance">Distance of the far plane.</param>
	/// <param name="result">The new projection <see cref="T:Microsoft.Xna.Framework.Matrix" /> for perspective view with FOV as an output parameter.</param>
	public static void CreatePerspectiveFieldOfView(float fieldOfView, float aspectRatio, float nearPlaneDistance, float farPlaneDistance, out Matrix result)
	{
		if (fieldOfView <= 0f || fieldOfView >= 3.141593f)
		{
			throw new ArgumentException("fieldOfView <= 0 or >= PI");
		}
		if (nearPlaneDistance <= 0f)
		{
			throw new ArgumentException("nearPlaneDistance <= 0");
		}
		if (farPlaneDistance <= 0f)
		{
			throw new ArgumentException("farPlaneDistance <= 0");
		}
		if (nearPlaneDistance >= farPlaneDistance)
		{
			throw new ArgumentException("nearPlaneDistance >= farPlaneDistance");
		}
		float num = 1f / (float)Math.Tan(fieldOfView * 0.5f);
		float num9 = num / aspectRatio;
		result.M11 = num9;
		result.M12 = (result.M13 = (result.M14 = 0f));
		result.M22 = num;
		result.M21 = (result.M23 = (result.M24 = 0f));
		result.M31 = (result.M32 = 0f);
		result.M33 = farPlaneDistance / (nearPlaneDistance - farPlaneDistance);
		result.M34 = -1f;
		result.M41 = (result.M42 = (result.M44 = 0f));
		result.M43 = nearPlaneDistance * farPlaneDistance / (nearPlaneDistance - farPlaneDistance);
	}

	/// <summary>
	/// Creates a new projection <see cref="T:Microsoft.Xna.Framework.Matrix" /> for customized perspective view.
	/// </summary>
	/// <param name="left">Lower x-value at the near plane.</param>
	/// <param name="right">Upper x-value at the near plane.</param>
	/// <param name="bottom">Lower y-coordinate at the near plane.</param>
	/// <param name="top">Upper y-value at the near plane.</param>
	/// <param name="nearPlaneDistance">Distance to the near plane.</param>
	/// <param name="farPlaneDistance">Distance to the far plane.</param>
	/// <returns>The new <see cref="T:Microsoft.Xna.Framework.Matrix" /> for customized perspective view.</returns>
	public static Matrix CreatePerspectiveOffCenter(float left, float right, float bottom, float top, float nearPlaneDistance, float farPlaneDistance)
	{
		Matrix.CreatePerspectiveOffCenter(left, right, bottom, top, nearPlaneDistance, farPlaneDistance, out var result);
		return result;
	}

	/// <summary>
	/// Creates a new projection <see cref="T:Microsoft.Xna.Framework.Matrix" /> for customized perspective view.
	/// </summary>
	/// <param name="viewingVolume">The viewing volume.</param>
	/// <param name="nearPlaneDistance">Distance to the near plane.</param>
	/// <param name="farPlaneDistance">Distance to the far plane.</param>
	/// <returns>The new <see cref="T:Microsoft.Xna.Framework.Matrix" /> for customized perspective view.</returns>
	public static Matrix CreatePerspectiveOffCenter(Rectangle viewingVolume, float nearPlaneDistance, float farPlaneDistance)
	{
		Matrix.CreatePerspectiveOffCenter(viewingVolume.Left, viewingVolume.Right, viewingVolume.Bottom, viewingVolume.Top, nearPlaneDistance, farPlaneDistance, out var result);
		return result;
	}

	/// <summary>
	/// Creates a new projection <see cref="T:Microsoft.Xna.Framework.Matrix" /> for customized perspective view.
	/// </summary>
	/// <param name="left">Lower x-value at the near plane.</param>
	/// <param name="right">Upper x-value at the near plane.</param>
	/// <param name="bottom">Lower y-coordinate at the near plane.</param>
	/// <param name="top">Upper y-value at the near plane.</param>
	/// <param name="nearPlaneDistance">Distance to the near plane.</param>
	/// <param name="farPlaneDistance">Distance to the far plane.</param>
	/// <param name="result">The new <see cref="T:Microsoft.Xna.Framework.Matrix" /> for customized perspective view as an output parameter.</param>
	public static void CreatePerspectiveOffCenter(float left, float right, float bottom, float top, float nearPlaneDistance, float farPlaneDistance, out Matrix result)
	{
		if (nearPlaneDistance <= 0f)
		{
			throw new ArgumentException("nearPlaneDistance <= 0");
		}
		if (farPlaneDistance <= 0f)
		{
			throw new ArgumentException("farPlaneDistance <= 0");
		}
		if (nearPlaneDistance >= farPlaneDistance)
		{
			throw new ArgumentException("nearPlaneDistance >= farPlaneDistance");
		}
		result.M11 = 2f * nearPlaneDistance / (right - left);
		result.M12 = (result.M13 = (result.M14 = 0f));
		result.M22 = 2f * nearPlaneDistance / (top - bottom);
		result.M21 = (result.M23 = (result.M24 = 0f));
		result.M31 = (left + right) / (right - left);
		result.M32 = (top + bottom) / (top - bottom);
		result.M33 = farPlaneDistance / (nearPlaneDistance - farPlaneDistance);
		result.M34 = -1f;
		result.M43 = nearPlaneDistance * farPlaneDistance / (nearPlaneDistance - farPlaneDistance);
		result.M41 = (result.M42 = (result.M44 = 0f));
	}

	/// <summary>
	/// Creates a new rotation <see cref="T:Microsoft.Xna.Framework.Matrix" /> around X axis.
	/// </summary>
	/// <param name="radians">Angle in radians.</param>
	/// <returns>The rotation <see cref="T:Microsoft.Xna.Framework.Matrix" /> around X axis.</returns>
	public static Matrix CreateRotationX(float radians)
	{
		Matrix.CreateRotationX(radians, out var result);
		return result;
	}

	/// <summary>
	/// Creates a new rotation <see cref="T:Microsoft.Xna.Framework.Matrix" /> around X axis.
	/// </summary>
	/// <param name="radians">Angle in radians.</param>
	/// <param name="result">The rotation <see cref="T:Microsoft.Xna.Framework.Matrix" /> around X axis as an output parameter.</param>
	public static void CreateRotationX(float radians, out Matrix result)
	{
		result = Matrix.Identity;
		float val1 = (float)Math.Cos(radians);
		float val2 = (float)Math.Sin(radians);
		result.M22 = val1;
		result.M23 = val2;
		result.M32 = 0f - val2;
		result.M33 = val1;
	}

	/// <summary>
	/// Creates a new rotation <see cref="T:Microsoft.Xna.Framework.Matrix" /> around Y axis.
	/// </summary>
	/// <param name="radians">Angle in radians.</param>
	/// <returns>The rotation <see cref="T:Microsoft.Xna.Framework.Matrix" /> around Y axis.</returns>
	public static Matrix CreateRotationY(float radians)
	{
		Matrix.CreateRotationY(radians, out var result);
		return result;
	}

	/// <summary>
	/// Creates a new rotation <see cref="T:Microsoft.Xna.Framework.Matrix" /> around Y axis.
	/// </summary>
	/// <param name="radians">Angle in radians.</param>
	/// <param name="result">The rotation <see cref="T:Microsoft.Xna.Framework.Matrix" /> around Y axis as an output parameter.</param>
	public static void CreateRotationY(float radians, out Matrix result)
	{
		result = Matrix.Identity;
		float val1 = (float)Math.Cos(radians);
		float val2 = (float)Math.Sin(radians);
		result.M11 = val1;
		result.M13 = 0f - val2;
		result.M31 = val2;
		result.M33 = val1;
	}

	/// <summary>
	/// Creates a new rotation <see cref="T:Microsoft.Xna.Framework.Matrix" /> around Z axis.
	/// </summary>
	/// <param name="radians">Angle in radians.</param>
	/// <returns>The rotation <see cref="T:Microsoft.Xna.Framework.Matrix" /> around Z axis.</returns>
	public static Matrix CreateRotationZ(float radians)
	{
		Matrix.CreateRotationZ(radians, out var result);
		return result;
	}

	/// <summary>
	/// Creates a new rotation <see cref="T:Microsoft.Xna.Framework.Matrix" /> around Z axis.
	/// </summary>
	/// <param name="radians">Angle in radians.</param>
	/// <param name="result">The rotation <see cref="T:Microsoft.Xna.Framework.Matrix" /> around Z axis as an output parameter.</param>
	public static void CreateRotationZ(float radians, out Matrix result)
	{
		result = Matrix.Identity;
		float val1 = (float)Math.Cos(radians);
		float val2 = (float)Math.Sin(radians);
		result.M11 = val1;
		result.M12 = val2;
		result.M21 = 0f - val2;
		result.M22 = val1;
	}

	/// <summary>
	/// Creates a new scaling <see cref="T:Microsoft.Xna.Framework.Matrix" />.
	/// </summary>
	/// <param name="scale">Scale value for all three axises.</param>
	/// <returns>The scaling <see cref="T:Microsoft.Xna.Framework.Matrix" />.</returns>
	public static Matrix CreateScale(float scale)
	{
		Matrix.CreateScale(scale, scale, scale, out var result);
		return result;
	}

	/// <summary>
	/// Creates a new scaling <see cref="T:Microsoft.Xna.Framework.Matrix" />.
	/// </summary>
	/// <param name="scale">Scale value for all three axises.</param>
	/// <param name="result">The scaling <see cref="T:Microsoft.Xna.Framework.Matrix" /> as an output parameter.</param>
	public static void CreateScale(float scale, out Matrix result)
	{
		Matrix.CreateScale(scale, scale, scale, out result);
	}

	/// <summary>
	/// Creates a new scaling <see cref="T:Microsoft.Xna.Framework.Matrix" />.
	/// </summary>
	/// <param name="xScale">Scale value for X axis.</param>
	/// <param name="yScale">Scale value for Y axis.</param>
	/// <param name="zScale">Scale value for Z axis.</param>
	/// <returns>The scaling <see cref="T:Microsoft.Xna.Framework.Matrix" />.</returns>
	public static Matrix CreateScale(float xScale, float yScale, float zScale)
	{
		Matrix.CreateScale(xScale, yScale, zScale, out var result);
		return result;
	}

	/// <summary>
	/// Creates a new scaling <see cref="T:Microsoft.Xna.Framework.Matrix" />.
	/// </summary>
	/// <param name="xScale">Scale value for X axis.</param>
	/// <param name="yScale">Scale value for Y axis.</param>
	/// <param name="zScale">Scale value for Z axis.</param>
	/// <param name="result">The scaling <see cref="T:Microsoft.Xna.Framework.Matrix" /> as an output parameter.</param>
	public static void CreateScale(float xScale, float yScale, float zScale, out Matrix result)
	{
		result.M11 = xScale;
		result.M12 = 0f;
		result.M13 = 0f;
		result.M14 = 0f;
		result.M21 = 0f;
		result.M22 = yScale;
		result.M23 = 0f;
		result.M24 = 0f;
		result.M31 = 0f;
		result.M32 = 0f;
		result.M33 = zScale;
		result.M34 = 0f;
		result.M41 = 0f;
		result.M42 = 0f;
		result.M43 = 0f;
		result.M44 = 1f;
	}

	/// <summary>
	/// Creates a new scaling <see cref="T:Microsoft.Xna.Framework.Matrix" />.
	/// </summary>
	/// <param name="scales"><see cref="T:Microsoft.Xna.Framework.Vector3" /> representing x,y and z scale values.</param>
	/// <returns>The scaling <see cref="T:Microsoft.Xna.Framework.Matrix" />.</returns>
	public static Matrix CreateScale(Vector3 scales)
	{
		Matrix.CreateScale(ref scales, out var result);
		return result;
	}

	/// <summary>
	/// Creates a new scaling <see cref="T:Microsoft.Xna.Framework.Matrix" />.
	/// </summary>
	/// <param name="scales"><see cref="T:Microsoft.Xna.Framework.Vector3" /> representing x,y and z scale values.</param>
	/// <param name="result">The scaling <see cref="T:Microsoft.Xna.Framework.Matrix" /> as an output parameter.</param>
	public static void CreateScale(ref Vector3 scales, out Matrix result)
	{
		result.M11 = scales.X;
		result.M12 = 0f;
		result.M13 = 0f;
		result.M14 = 0f;
		result.M21 = 0f;
		result.M22 = scales.Y;
		result.M23 = 0f;
		result.M24 = 0f;
		result.M31 = 0f;
		result.M32 = 0f;
		result.M33 = scales.Z;
		result.M34 = 0f;
		result.M41 = 0f;
		result.M42 = 0f;
		result.M43 = 0f;
		result.M44 = 1f;
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Matrix" /> that flattens geometry into a specified <see cref="T:Microsoft.Xna.Framework.Plane" /> as if casting a shadow from a specified light source. 
	/// </summary>
	/// <param name="lightDirection">A vector specifying the direction from which the light that will cast the shadow is coming.</param>
	/// <param name="plane">The plane onto which the new matrix should flatten geometry so as to cast a shadow.</param>
	/// <returns>A <see cref="T:Microsoft.Xna.Framework.Matrix" /> that can be used to flatten geometry onto the specified plane from the specified direction. </returns>
	public static Matrix CreateShadow(Vector3 lightDirection, Plane plane)
	{
		Matrix.CreateShadow(ref lightDirection, ref plane, out var result);
		return result;
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Matrix" /> that flattens geometry into a specified <see cref="T:Microsoft.Xna.Framework.Plane" /> as if casting a shadow from a specified light source. 
	/// </summary>
	/// <param name="lightDirection">A vector specifying the direction from which the light that will cast the shadow is coming.</param>
	/// <param name="plane">The plane onto which the new matrix should flatten geometry so as to cast a shadow.</param>
	/// <param name="result">A <see cref="T:Microsoft.Xna.Framework.Matrix" /> that can be used to flatten geometry onto the specified plane from the specified direction as an output parameter.</param>
	public static void CreateShadow(ref Vector3 lightDirection, ref Plane plane, out Matrix result)
	{
		float dot = plane.Normal.X * lightDirection.X + plane.Normal.Y * lightDirection.Y + plane.Normal.Z * lightDirection.Z;
		float x = 0f - plane.Normal.X;
		float y = 0f - plane.Normal.Y;
		float z = 0f - plane.Normal.Z;
		float d = 0f - plane.D;
		result.M11 = x * lightDirection.X + dot;
		result.M12 = x * lightDirection.Y;
		result.M13 = x * lightDirection.Z;
		result.M14 = 0f;
		result.M21 = y * lightDirection.X;
		result.M22 = y * lightDirection.Y + dot;
		result.M23 = y * lightDirection.Z;
		result.M24 = 0f;
		result.M31 = z * lightDirection.X;
		result.M32 = z * lightDirection.Y;
		result.M33 = z * lightDirection.Z + dot;
		result.M34 = 0f;
		result.M41 = d * lightDirection.X;
		result.M42 = d * lightDirection.Y;
		result.M43 = d * lightDirection.Z;
		result.M44 = dot;
	}

	/// <summary>
	/// Creates a new translation <see cref="T:Microsoft.Xna.Framework.Matrix" />.
	/// </summary>
	/// <param name="xPosition">X coordinate of translation.</param>
	/// <param name="yPosition">Y coordinate of translation.</param>
	/// <param name="zPosition">Z coordinate of translation.</param>
	/// <returns>The translation <see cref="T:Microsoft.Xna.Framework.Matrix" />.</returns>
	public static Matrix CreateTranslation(float xPosition, float yPosition, float zPosition)
	{
		Matrix.CreateTranslation(xPosition, yPosition, zPosition, out var result);
		return result;
	}

	/// <summary>
	/// Creates a new translation <see cref="T:Microsoft.Xna.Framework.Matrix" />.
	/// </summary>
	/// <param name="position">X,Y and Z coordinates of translation.</param>
	/// <param name="result">The translation <see cref="T:Microsoft.Xna.Framework.Matrix" /> as an output parameter.</param>
	public static void CreateTranslation(ref Vector3 position, out Matrix result)
	{
		result.M11 = 1f;
		result.M12 = 0f;
		result.M13 = 0f;
		result.M14 = 0f;
		result.M21 = 0f;
		result.M22 = 1f;
		result.M23 = 0f;
		result.M24 = 0f;
		result.M31 = 0f;
		result.M32 = 0f;
		result.M33 = 1f;
		result.M34 = 0f;
		result.M41 = position.X;
		result.M42 = position.Y;
		result.M43 = position.Z;
		result.M44 = 1f;
	}

	/// <summary>
	/// Creates a new translation <see cref="T:Microsoft.Xna.Framework.Matrix" />.
	/// </summary>
	/// <param name="position">X,Y and Z coordinates of translation.</param>
	/// <returns>The translation <see cref="T:Microsoft.Xna.Framework.Matrix" />.</returns>
	public static Matrix CreateTranslation(Vector3 position)
	{
		Matrix.CreateTranslation(ref position, out var result);
		return result;
	}

	/// <summary>
	/// Creates a new translation <see cref="T:Microsoft.Xna.Framework.Matrix" />.
	/// </summary>
	/// <param name="xPosition">X coordinate of translation.</param>
	/// <param name="yPosition">Y coordinate of translation.</param>
	/// <param name="zPosition">Z coordinate of translation.</param>
	/// <param name="result">The translation <see cref="T:Microsoft.Xna.Framework.Matrix" /> as an output parameter.</param>
	public static void CreateTranslation(float xPosition, float yPosition, float zPosition, out Matrix result)
	{
		result.M11 = 1f;
		result.M12 = 0f;
		result.M13 = 0f;
		result.M14 = 0f;
		result.M21 = 0f;
		result.M22 = 1f;
		result.M23 = 0f;
		result.M24 = 0f;
		result.M31 = 0f;
		result.M32 = 0f;
		result.M33 = 1f;
		result.M34 = 0f;
		result.M41 = xPosition;
		result.M42 = yPosition;
		result.M43 = zPosition;
		result.M44 = 1f;
	}

	/// <summary>
	/// Creates a new reflection <see cref="T:Microsoft.Xna.Framework.Matrix" />.
	/// </summary>
	/// <param name="value">The plane that used for reflection calculation.</param>
	/// <returns>The reflection <see cref="T:Microsoft.Xna.Framework.Matrix" />.</returns>
	public static Matrix CreateReflection(Plane value)
	{
		Matrix.CreateReflection(ref value, out var result);
		return result;
	}

	/// <summary>
	/// Creates a new reflection <see cref="T:Microsoft.Xna.Framework.Matrix" />.
	/// </summary>
	/// <param name="value">The plane that used for reflection calculation.</param>
	/// <param name="result">The reflection <see cref="T:Microsoft.Xna.Framework.Matrix" /> as an output parameter.</param>
	public static void CreateReflection(ref Plane value, out Matrix result)
	{
		Plane.Normalize(ref value, out var plane);
		float x = plane.Normal.X;
		float y = plane.Normal.Y;
		float z = plane.Normal.Z;
		float num3 = -2f * x;
		float num4 = -2f * y;
		float num5 = -2f * z;
		result.M11 = num3 * x + 1f;
		result.M12 = num4 * x;
		result.M13 = num5 * x;
		result.M14 = 0f;
		result.M21 = num3 * y;
		result.M22 = num4 * y + 1f;
		result.M23 = num5 * y;
		result.M24 = 0f;
		result.M31 = num3 * z;
		result.M32 = num4 * z;
		result.M33 = num5 * z + 1f;
		result.M34 = 0f;
		result.M41 = num3 * plane.D;
		result.M42 = num4 * plane.D;
		result.M43 = num5 * plane.D;
		result.M44 = 1f;
	}

	/// <summary>
	/// Creates a new world <see cref="T:Microsoft.Xna.Framework.Matrix" />.
	/// </summary>
	/// <param name="position">The position vector.</param>
	/// <param name="forward">The forward direction vector.</param>
	/// <param name="up">The upward direction vector. Usually <see cref="P:Microsoft.Xna.Framework.Vector3.Up" />.</param>
	/// <returns>The world <see cref="T:Microsoft.Xna.Framework.Matrix" />.</returns>
	public static Matrix CreateWorld(Vector3 position, Vector3 forward, Vector3 up)
	{
		Matrix.CreateWorld(ref position, ref forward, ref up, out var ret);
		return ret;
	}

	/// <summary>
	/// Creates a new world <see cref="T:Microsoft.Xna.Framework.Matrix" />.
	/// </summary>
	/// <param name="position">The position vector.</param>
	/// <param name="forward">The forward direction vector.</param>
	/// <param name="up">The upward direction vector. Usually <see cref="P:Microsoft.Xna.Framework.Vector3.Up" />.</param>
	/// <param name="result">The world <see cref="T:Microsoft.Xna.Framework.Matrix" /> as an output parameter.</param>
	public static void CreateWorld(ref Vector3 position, ref Vector3 forward, ref Vector3 up, out Matrix result)
	{
		Vector3.Normalize(ref forward, out var z);
		Vector3.Cross(ref forward, ref up, out var x);
		Vector3.Cross(ref x, ref forward, out var y);
		x.Normalize();
		y.Normalize();
		result = default(Matrix);
		result.Right = x;
		result.Up = y;
		result.Forward = z;
		result.Translation = position;
		result.M44 = 1f;
	}

	/// <summary>
	/// Decomposes this matrix to translation, rotation and scale elements. Returns <c>true</c> if matrix can be decomposed; <c>false</c> otherwise.
	/// </summary>
	/// <param name="scale">Scale vector as an output parameter.</param>
	/// <param name="rotation">Rotation quaternion as an output parameter.</param>
	/// <param name="translation">Translation vector as an output parameter.</param>
	/// <returns><c>true</c> if matrix can be decomposed; <c>false</c> otherwise.</returns>
	public bool Decompose(out Vector3 scale, out Quaternion rotation, out Vector3 translation)
	{
		translation.X = this.M41;
		translation.Y = this.M42;
		translation.Z = this.M43;
		float xs = ((Math.Sign(this.M11 * this.M12 * this.M13 * this.M14) >= 0) ? 1 : (-1));
		float ys = ((Math.Sign(this.M21 * this.M22 * this.M23 * this.M24) >= 0) ? 1 : (-1));
		float zs = ((Math.Sign(this.M31 * this.M32 * this.M33 * this.M34) >= 0) ? 1 : (-1));
		scale.X = xs * (float)Math.Sqrt(this.M11 * this.M11 + this.M12 * this.M12 + this.M13 * this.M13);
		scale.Y = ys * (float)Math.Sqrt(this.M21 * this.M21 + this.M22 * this.M22 + this.M23 * this.M23);
		scale.Z = zs * (float)Math.Sqrt(this.M31 * this.M31 + this.M32 * this.M32 + this.M33 * this.M33);
		if ((double)scale.X == 0.0 || (double)scale.Y == 0.0 || (double)scale.Z == 0.0)
		{
			rotation = Quaternion.Identity;
			return false;
		}
		Matrix m1 = new Matrix(this.M11 / scale.X, this.M12 / scale.X, this.M13 / scale.X, 0f, this.M21 / scale.Y, this.M22 / scale.Y, this.M23 / scale.Y, 0f, this.M31 / scale.Z, this.M32 / scale.Z, this.M33 / scale.Z, 0f, 0f, 0f, 0f, 1f);
		rotation = Quaternion.CreateFromRotationMatrix(m1);
		return true;
	}

	/// <summary>
	/// Returns a determinant of this <see cref="T:Microsoft.Xna.Framework.Matrix" />.
	/// </summary>
	/// <returns>Determinant of this <see cref="T:Microsoft.Xna.Framework.Matrix" /></returns>
	/// <remarks>See more about determinant here - http://en.wikipedia.org/wiki/Determinant.
	/// </remarks>
	public float Determinant()
	{
		float m = this.M11;
		float num21 = this.M12;
		float num22 = this.M13;
		float num23 = this.M14;
		float num24 = this.M21;
		float num25 = this.M22;
		float num26 = this.M23;
		float num27 = this.M24;
		float m2 = this.M31;
		float num28 = this.M32;
		float num29 = this.M33;
		float num30 = this.M34;
		float num31 = this.M41;
		float num32 = this.M42;
		float num33 = this.M43;
		float num34 = this.M44;
		float num35 = num29 * num34 - num30 * num33;
		float num36 = num28 * num34 - num30 * num32;
		float num37 = num28 * num33 - num29 * num32;
		float num38 = m2 * num34 - num30 * num31;
		float num39 = m2 * num33 - num29 * num31;
		float num40 = m2 * num32 - num28 * num31;
		return m * (num25 * num35 - num26 * num36 + num27 * num37) - num21 * (num24 * num35 - num26 * num38 + num27 * num39) + num22 * (num24 * num36 - num25 * num38 + num27 * num40) - num23 * (num24 * num37 - num25 * num39 + num26 * num40);
	}

	/// <summary>
	/// Divides the elements of a <see cref="T:Microsoft.Xna.Framework.Matrix" /> by the elements of another matrix.
	/// </summary>
	/// <param name="matrix1">Source <see cref="T:Microsoft.Xna.Framework.Matrix" />.</param>
	/// <param name="matrix2">Divisor <see cref="T:Microsoft.Xna.Framework.Matrix" />.</param>
	/// <returns>The result of dividing the matrix.</returns>
	public static Matrix Divide(Matrix matrix1, Matrix matrix2)
	{
		matrix1.M11 /= matrix2.M11;
		matrix1.M12 /= matrix2.M12;
		matrix1.M13 /= matrix2.M13;
		matrix1.M14 /= matrix2.M14;
		matrix1.M21 /= matrix2.M21;
		matrix1.M22 /= matrix2.M22;
		matrix1.M23 /= matrix2.M23;
		matrix1.M24 /= matrix2.M24;
		matrix1.M31 /= matrix2.M31;
		matrix1.M32 /= matrix2.M32;
		matrix1.M33 /= matrix2.M33;
		matrix1.M34 /= matrix2.M34;
		matrix1.M41 /= matrix2.M41;
		matrix1.M42 /= matrix2.M42;
		matrix1.M43 /= matrix2.M43;
		matrix1.M44 /= matrix2.M44;
		return matrix1;
	}

	/// <summary>
	/// Divides the elements of a <see cref="T:Microsoft.Xna.Framework.Matrix" /> by the elements of another matrix.
	/// </summary>
	/// <param name="matrix1">Source <see cref="T:Microsoft.Xna.Framework.Matrix" />.</param>
	/// <param name="matrix2">Divisor <see cref="T:Microsoft.Xna.Framework.Matrix" />.</param>
	/// <param name="result">The result of dividing the matrix as an output parameter.</param>
	public static void Divide(ref Matrix matrix1, ref Matrix matrix2, out Matrix result)
	{
		result.M11 = matrix1.M11 / matrix2.M11;
		result.M12 = matrix1.M12 / matrix2.M12;
		result.M13 = matrix1.M13 / matrix2.M13;
		result.M14 = matrix1.M14 / matrix2.M14;
		result.M21 = matrix1.M21 / matrix2.M21;
		result.M22 = matrix1.M22 / matrix2.M22;
		result.M23 = matrix1.M23 / matrix2.M23;
		result.M24 = matrix1.M24 / matrix2.M24;
		result.M31 = matrix1.M31 / matrix2.M31;
		result.M32 = matrix1.M32 / matrix2.M32;
		result.M33 = matrix1.M33 / matrix2.M33;
		result.M34 = matrix1.M34 / matrix2.M34;
		result.M41 = matrix1.M41 / matrix2.M41;
		result.M42 = matrix1.M42 / matrix2.M42;
		result.M43 = matrix1.M43 / matrix2.M43;
		result.M44 = matrix1.M44 / matrix2.M44;
	}

	/// <summary>
	/// Divides the elements of a <see cref="T:Microsoft.Xna.Framework.Matrix" /> by a scalar.
	/// </summary>
	/// <param name="matrix1">Source <see cref="T:Microsoft.Xna.Framework.Matrix" />.</param>
	/// <param name="divider">Divisor scalar.</param>
	/// <returns>The result of dividing a matrix by a scalar.</returns>
	public static Matrix Divide(Matrix matrix1, float divider)
	{
		float num = 1f / divider;
		matrix1.M11 *= num;
		matrix1.M12 *= num;
		matrix1.M13 *= num;
		matrix1.M14 *= num;
		matrix1.M21 *= num;
		matrix1.M22 *= num;
		matrix1.M23 *= num;
		matrix1.M24 *= num;
		matrix1.M31 *= num;
		matrix1.M32 *= num;
		matrix1.M33 *= num;
		matrix1.M34 *= num;
		matrix1.M41 *= num;
		matrix1.M42 *= num;
		matrix1.M43 *= num;
		matrix1.M44 *= num;
		return matrix1;
	}

	/// <summary>
	/// Divides the elements of a <see cref="T:Microsoft.Xna.Framework.Matrix" /> by a scalar.
	/// </summary>
	/// <param name="matrix1">Source <see cref="T:Microsoft.Xna.Framework.Matrix" />.</param>
	/// <param name="divider">Divisor scalar.</param>
	/// <param name="result">The result of dividing a matrix by a scalar as an output parameter.</param>
	public static void Divide(ref Matrix matrix1, float divider, out Matrix result)
	{
		float num = 1f / divider;
		result.M11 = matrix1.M11 * num;
		result.M12 = matrix1.M12 * num;
		result.M13 = matrix1.M13 * num;
		result.M14 = matrix1.M14 * num;
		result.M21 = matrix1.M21 * num;
		result.M22 = matrix1.M22 * num;
		result.M23 = matrix1.M23 * num;
		result.M24 = matrix1.M24 * num;
		result.M31 = matrix1.M31 * num;
		result.M32 = matrix1.M32 * num;
		result.M33 = matrix1.M33 * num;
		result.M34 = matrix1.M34 * num;
		result.M41 = matrix1.M41 * num;
		result.M42 = matrix1.M42 * num;
		result.M43 = matrix1.M43 * num;
		result.M44 = matrix1.M44 * num;
	}

	/// <summary>
	/// Compares whether current instance is equal to specified <see cref="T:Microsoft.Xna.Framework.Matrix" /> without any tolerance.
	/// </summary>
	/// <param name="other">The <see cref="T:Microsoft.Xna.Framework.Matrix" /> to compare.</param>
	/// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
	public bool Equals(Matrix other)
	{
		if (this.M11 == other.M11 && this.M22 == other.M22 && this.M33 == other.M33 && this.M44 == other.M44 && this.M12 == other.M12 && this.M13 == other.M13 && this.M14 == other.M14 && this.M21 == other.M21 && this.M23 == other.M23 && this.M24 == other.M24 && this.M31 == other.M31 && this.M32 == other.M32 && this.M34 == other.M34 && this.M41 == other.M41 && this.M42 == other.M42)
		{
			return this.M43 == other.M43;
		}
		return false;
	}

	/// <summary>
	/// Compares whether current instance is equal to specified <see cref="T:System.Object" /> without any tolerance.
	/// </summary>
	/// <param name="obj">The <see cref="T:System.Object" /> to compare.</param>
	/// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
	public override bool Equals(object obj)
	{
		bool flag = false;
		if (obj is Matrix)
		{
			flag = this.Equals((Matrix)obj);
		}
		return flag;
	}

	/// <summary>
	/// Gets the hash code of this <see cref="T:Microsoft.Xna.Framework.Matrix" />.
	/// </summary>
	/// <returns>Hash code of this <see cref="T:Microsoft.Xna.Framework.Matrix" />.</returns>
	public override int GetHashCode()
	{
		return this.M11.GetHashCode() + this.M12.GetHashCode() + this.M13.GetHashCode() + this.M14.GetHashCode() + this.M21.GetHashCode() + this.M22.GetHashCode() + this.M23.GetHashCode() + this.M24.GetHashCode() + this.M31.GetHashCode() + this.M32.GetHashCode() + this.M33.GetHashCode() + this.M34.GetHashCode() + this.M41.GetHashCode() + this.M42.GetHashCode() + this.M43.GetHashCode() + this.M44.GetHashCode();
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Matrix" /> which contains inversion of the specified matrix. 
	/// </summary>
	/// <param name="matrix">Source <see cref="T:Microsoft.Xna.Framework.Matrix" />.</param>
	/// <returns>The inverted matrix.</returns>
	public static Matrix Invert(Matrix matrix)
	{
		Matrix.Invert(ref matrix, out var result);
		return result;
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Matrix" /> which contains inversion of the specified matrix. 
	/// </summary>
	/// <param name="matrix">Source <see cref="T:Microsoft.Xna.Framework.Matrix" />.</param>
	/// <param name="result">The inverted matrix as output parameter.</param>
	public static void Invert(ref Matrix matrix, out Matrix result)
	{
		float num1 = matrix.M11;
		float num2 = matrix.M12;
		float num3 = matrix.M13;
		float num4 = matrix.M14;
		float m = matrix.M21;
		float num6 = matrix.M22;
		float num7 = matrix.M23;
		float num8 = matrix.M24;
		float num9 = matrix.M31;
		float num10 = matrix.M32;
		float num11 = matrix.M33;
		float num12 = matrix.M34;
		float num13 = matrix.M41;
		float num14 = matrix.M42;
		float num15 = matrix.M43;
		float num16 = matrix.M44;
		float num17 = (float)((double)num11 * (double)num16 - (double)num12 * (double)num15);
		float num18 = (float)((double)num10 * (double)num16 - (double)num12 * (double)num14);
		float num19 = (float)((double)num10 * (double)num15 - (double)num11 * (double)num14);
		float num20 = (float)((double)num9 * (double)num16 - (double)num12 * (double)num13);
		float num21 = (float)((double)num9 * (double)num15 - (double)num11 * (double)num13);
		float num22 = (float)((double)num9 * (double)num14 - (double)num10 * (double)num13);
		float num23 = (float)((double)num6 * (double)num17 - (double)num7 * (double)num18 + (double)num8 * (double)num19);
		float num24 = (float)(0.0 - ((double)m * (double)num17 - (double)num7 * (double)num20 + (double)num8 * (double)num21));
		float num25 = (float)((double)m * (double)num18 - (double)num6 * (double)num20 + (double)num8 * (double)num22);
		float num26 = (float)(0.0 - ((double)m * (double)num19 - (double)num6 * (double)num21 + (double)num7 * (double)num22));
		float num27 = (float)(1.0 / ((double)num1 * (double)num23 + (double)num2 * (double)num24 + (double)num3 * (double)num25 + (double)num4 * (double)num26));
		result.M11 = num23 * num27;
		result.M21 = num24 * num27;
		result.M31 = num25 * num27;
		result.M41 = num26 * num27;
		result.M12 = (float)(0.0 - ((double)num2 * (double)num17 - (double)num3 * (double)num18 + (double)num4 * (double)num19)) * num27;
		result.M22 = (float)((double)num1 * (double)num17 - (double)num3 * (double)num20 + (double)num4 * (double)num21) * num27;
		result.M32 = (float)(0.0 - ((double)num1 * (double)num18 - (double)num2 * (double)num20 + (double)num4 * (double)num22)) * num27;
		result.M42 = (float)((double)num1 * (double)num19 - (double)num2 * (double)num21 + (double)num3 * (double)num22) * num27;
		float num28 = (float)((double)num7 * (double)num16 - (double)num8 * (double)num15);
		float num29 = (float)((double)num6 * (double)num16 - (double)num8 * (double)num14);
		float num30 = (float)((double)num6 * (double)num15 - (double)num7 * (double)num14);
		float num31 = (float)((double)m * (double)num16 - (double)num8 * (double)num13);
		float num32 = (float)((double)m * (double)num15 - (double)num7 * (double)num13);
		float num33 = (float)((double)m * (double)num14 - (double)num6 * (double)num13);
		result.M13 = (float)((double)num2 * (double)num28 - (double)num3 * (double)num29 + (double)num4 * (double)num30) * num27;
		result.M23 = (float)(0.0 - ((double)num1 * (double)num28 - (double)num3 * (double)num31 + (double)num4 * (double)num32)) * num27;
		result.M33 = (float)((double)num1 * (double)num29 - (double)num2 * (double)num31 + (double)num4 * (double)num33) * num27;
		result.M43 = (float)(0.0 - ((double)num1 * (double)num30 - (double)num2 * (double)num32 + (double)num3 * (double)num33)) * num27;
		float num34 = (float)((double)num7 * (double)num12 - (double)num8 * (double)num11);
		float num35 = (float)((double)num6 * (double)num12 - (double)num8 * (double)num10);
		float num36 = (float)((double)num6 * (double)num11 - (double)num7 * (double)num10);
		float num37 = (float)((double)m * (double)num12 - (double)num8 * (double)num9);
		float num38 = (float)((double)m * (double)num11 - (double)num7 * (double)num9);
		float num39 = (float)((double)m * (double)num10 - (double)num6 * (double)num9);
		result.M14 = (float)(0.0 - ((double)num2 * (double)num34 - (double)num3 * (double)num35 + (double)num4 * (double)num36)) * num27;
		result.M24 = (float)((double)num1 * (double)num34 - (double)num3 * (double)num37 + (double)num4 * (double)num38) * num27;
		result.M34 = (float)(0.0 - ((double)num1 * (double)num35 - (double)num2 * (double)num37 + (double)num4 * (double)num39)) * num27;
		result.M44 = (float)((double)num1 * (double)num36 - (double)num2 * (double)num38 + (double)num3 * (double)num39) * num27;
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Matrix" /> that contains linear interpolation of the values in specified matrixes.
	/// </summary>
	/// <param name="matrix1">The first <see cref="T:Microsoft.Xna.Framework.Matrix" />.</param>
	/// <param name="matrix2">The second <see cref="T:Microsoft.Xna.Framework.Vector2" />.</param>
	/// <param name="amount">Weighting value(between 0.0 and 1.0).</param>
	/// <returns>&gt;The result of linear interpolation of the specified matrixes.</returns>
	public static Matrix Lerp(Matrix matrix1, Matrix matrix2, float amount)
	{
		matrix1.M11 += (matrix2.M11 - matrix1.M11) * amount;
		matrix1.M12 += (matrix2.M12 - matrix1.M12) * amount;
		matrix1.M13 += (matrix2.M13 - matrix1.M13) * amount;
		matrix1.M14 += (matrix2.M14 - matrix1.M14) * amount;
		matrix1.M21 += (matrix2.M21 - matrix1.M21) * amount;
		matrix1.M22 += (matrix2.M22 - matrix1.M22) * amount;
		matrix1.M23 += (matrix2.M23 - matrix1.M23) * amount;
		matrix1.M24 += (matrix2.M24 - matrix1.M24) * amount;
		matrix1.M31 += (matrix2.M31 - matrix1.M31) * amount;
		matrix1.M32 += (matrix2.M32 - matrix1.M32) * amount;
		matrix1.M33 += (matrix2.M33 - matrix1.M33) * amount;
		matrix1.M34 += (matrix2.M34 - matrix1.M34) * amount;
		matrix1.M41 += (matrix2.M41 - matrix1.M41) * amount;
		matrix1.M42 += (matrix2.M42 - matrix1.M42) * amount;
		matrix1.M43 += (matrix2.M43 - matrix1.M43) * amount;
		matrix1.M44 += (matrix2.M44 - matrix1.M44) * amount;
		return matrix1;
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Matrix" /> that contains linear interpolation of the values in specified matrixes.
	/// </summary>
	/// <param name="matrix1">The first <see cref="T:Microsoft.Xna.Framework.Matrix" />.</param>
	/// <param name="matrix2">The second <see cref="T:Microsoft.Xna.Framework.Vector2" />.</param>
	/// <param name="amount">Weighting value(between 0.0 and 1.0).</param>
	/// <param name="result">The result of linear interpolation of the specified matrixes as an output parameter.</param>
	public static void Lerp(ref Matrix matrix1, ref Matrix matrix2, float amount, out Matrix result)
	{
		result.M11 = matrix1.M11 + (matrix2.M11 - matrix1.M11) * amount;
		result.M12 = matrix1.M12 + (matrix2.M12 - matrix1.M12) * amount;
		result.M13 = matrix1.M13 + (matrix2.M13 - matrix1.M13) * amount;
		result.M14 = matrix1.M14 + (matrix2.M14 - matrix1.M14) * amount;
		result.M21 = matrix1.M21 + (matrix2.M21 - matrix1.M21) * amount;
		result.M22 = matrix1.M22 + (matrix2.M22 - matrix1.M22) * amount;
		result.M23 = matrix1.M23 + (matrix2.M23 - matrix1.M23) * amount;
		result.M24 = matrix1.M24 + (matrix2.M24 - matrix1.M24) * amount;
		result.M31 = matrix1.M31 + (matrix2.M31 - matrix1.M31) * amount;
		result.M32 = matrix1.M32 + (matrix2.M32 - matrix1.M32) * amount;
		result.M33 = matrix1.M33 + (matrix2.M33 - matrix1.M33) * amount;
		result.M34 = matrix1.M34 + (matrix2.M34 - matrix1.M34) * amount;
		result.M41 = matrix1.M41 + (matrix2.M41 - matrix1.M41) * amount;
		result.M42 = matrix1.M42 + (matrix2.M42 - matrix1.M42) * amount;
		result.M43 = matrix1.M43 + (matrix2.M43 - matrix1.M43) * amount;
		result.M44 = matrix1.M44 + (matrix2.M44 - matrix1.M44) * amount;
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Matrix" /> that contains a multiplication of two matrix.
	/// </summary>
	/// <param name="matrix1">Source <see cref="T:Microsoft.Xna.Framework.Matrix" />.</param>
	/// <param name="matrix2">Source <see cref="T:Microsoft.Xna.Framework.Matrix" />.</param>
	/// <returns>Result of the matrix multiplication.</returns>
	public static Matrix Multiply(Matrix matrix1, Matrix matrix2)
	{
		float m11 = matrix1.M11 * matrix2.M11 + matrix1.M12 * matrix2.M21 + matrix1.M13 * matrix2.M31 + matrix1.M14 * matrix2.M41;
		float m12 = matrix1.M11 * matrix2.M12 + matrix1.M12 * matrix2.M22 + matrix1.M13 * matrix2.M32 + matrix1.M14 * matrix2.M42;
		float m13 = matrix1.M11 * matrix2.M13 + matrix1.M12 * matrix2.M23 + matrix1.M13 * matrix2.M33 + matrix1.M14 * matrix2.M43;
		float m14 = matrix1.M11 * matrix2.M14 + matrix1.M12 * matrix2.M24 + matrix1.M13 * matrix2.M34 + matrix1.M14 * matrix2.M44;
		float m21 = matrix1.M21 * matrix2.M11 + matrix1.M22 * matrix2.M21 + matrix1.M23 * matrix2.M31 + matrix1.M24 * matrix2.M41;
		float m22 = matrix1.M21 * matrix2.M12 + matrix1.M22 * matrix2.M22 + matrix1.M23 * matrix2.M32 + matrix1.M24 * matrix2.M42;
		float m23 = matrix1.M21 * matrix2.M13 + matrix1.M22 * matrix2.M23 + matrix1.M23 * matrix2.M33 + matrix1.M24 * matrix2.M43;
		float m24 = matrix1.M21 * matrix2.M14 + matrix1.M22 * matrix2.M24 + matrix1.M23 * matrix2.M34 + matrix1.M24 * matrix2.M44;
		float m31 = matrix1.M31 * matrix2.M11 + matrix1.M32 * matrix2.M21 + matrix1.M33 * matrix2.M31 + matrix1.M34 * matrix2.M41;
		float m32 = matrix1.M31 * matrix2.M12 + matrix1.M32 * matrix2.M22 + matrix1.M33 * matrix2.M32 + matrix1.M34 * matrix2.M42;
		float m33 = matrix1.M31 * matrix2.M13 + matrix1.M32 * matrix2.M23 + matrix1.M33 * matrix2.M33 + matrix1.M34 * matrix2.M43;
		float m34 = matrix1.M31 * matrix2.M14 + matrix1.M32 * matrix2.M24 + matrix1.M33 * matrix2.M34 + matrix1.M34 * matrix2.M44;
		float m41 = matrix1.M41 * matrix2.M11 + matrix1.M42 * matrix2.M21 + matrix1.M43 * matrix2.M31 + matrix1.M44 * matrix2.M41;
		float m42 = matrix1.M41 * matrix2.M12 + matrix1.M42 * matrix2.M22 + matrix1.M43 * matrix2.M32 + matrix1.M44 * matrix2.M42;
		float m43 = matrix1.M41 * matrix2.M13 + matrix1.M42 * matrix2.M23 + matrix1.M43 * matrix2.M33 + matrix1.M44 * matrix2.M43;
		float m44 = matrix1.M41 * matrix2.M14 + matrix1.M42 * matrix2.M24 + matrix1.M43 * matrix2.M34 + matrix1.M44 * matrix2.M44;
		matrix1.M11 = m11;
		matrix1.M12 = m12;
		matrix1.M13 = m13;
		matrix1.M14 = m14;
		matrix1.M21 = m21;
		matrix1.M22 = m22;
		matrix1.M23 = m23;
		matrix1.M24 = m24;
		matrix1.M31 = m31;
		matrix1.M32 = m32;
		matrix1.M33 = m33;
		matrix1.M34 = m34;
		matrix1.M41 = m41;
		matrix1.M42 = m42;
		matrix1.M43 = m43;
		matrix1.M44 = m44;
		return matrix1;
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Matrix" /> that contains a multiplication of two matrix.
	/// </summary>
	/// <param name="matrix1">Source <see cref="T:Microsoft.Xna.Framework.Matrix" />.</param>
	/// <param name="matrix2">Source <see cref="T:Microsoft.Xna.Framework.Matrix" />.</param>
	/// <param name="result">Result of the matrix multiplication as an output parameter.</param>
	public static void Multiply(ref Matrix matrix1, ref Matrix matrix2, out Matrix result)
	{
		float m11 = matrix1.M11 * matrix2.M11 + matrix1.M12 * matrix2.M21 + matrix1.M13 * matrix2.M31 + matrix1.M14 * matrix2.M41;
		float m12 = matrix1.M11 * matrix2.M12 + matrix1.M12 * matrix2.M22 + matrix1.M13 * matrix2.M32 + matrix1.M14 * matrix2.M42;
		float m13 = matrix1.M11 * matrix2.M13 + matrix1.M12 * matrix2.M23 + matrix1.M13 * matrix2.M33 + matrix1.M14 * matrix2.M43;
		float m14 = matrix1.M11 * matrix2.M14 + matrix1.M12 * matrix2.M24 + matrix1.M13 * matrix2.M34 + matrix1.M14 * matrix2.M44;
		float m21 = matrix1.M21 * matrix2.M11 + matrix1.M22 * matrix2.M21 + matrix1.M23 * matrix2.M31 + matrix1.M24 * matrix2.M41;
		float m22 = matrix1.M21 * matrix2.M12 + matrix1.M22 * matrix2.M22 + matrix1.M23 * matrix2.M32 + matrix1.M24 * matrix2.M42;
		float m23 = matrix1.M21 * matrix2.M13 + matrix1.M22 * matrix2.M23 + matrix1.M23 * matrix2.M33 + matrix1.M24 * matrix2.M43;
		float m24 = matrix1.M21 * matrix2.M14 + matrix1.M22 * matrix2.M24 + matrix1.M23 * matrix2.M34 + matrix1.M24 * matrix2.M44;
		float m31 = matrix1.M31 * matrix2.M11 + matrix1.M32 * matrix2.M21 + matrix1.M33 * matrix2.M31 + matrix1.M34 * matrix2.M41;
		float m32 = matrix1.M31 * matrix2.M12 + matrix1.M32 * matrix2.M22 + matrix1.M33 * matrix2.M32 + matrix1.M34 * matrix2.M42;
		float m33 = matrix1.M31 * matrix2.M13 + matrix1.M32 * matrix2.M23 + matrix1.M33 * matrix2.M33 + matrix1.M34 * matrix2.M43;
		float m34 = matrix1.M31 * matrix2.M14 + matrix1.M32 * matrix2.M24 + matrix1.M33 * matrix2.M34 + matrix1.M34 * matrix2.M44;
		float m41 = matrix1.M41 * matrix2.M11 + matrix1.M42 * matrix2.M21 + matrix1.M43 * matrix2.M31 + matrix1.M44 * matrix2.M41;
		float m42 = matrix1.M41 * matrix2.M12 + matrix1.M42 * matrix2.M22 + matrix1.M43 * matrix2.M32 + matrix1.M44 * matrix2.M42;
		float m43 = matrix1.M41 * matrix2.M13 + matrix1.M42 * matrix2.M23 + matrix1.M43 * matrix2.M33 + matrix1.M44 * matrix2.M43;
		float m44 = matrix1.M41 * matrix2.M14 + matrix1.M42 * matrix2.M24 + matrix1.M43 * matrix2.M34 + matrix1.M44 * matrix2.M44;
		result.M11 = m11;
		result.M12 = m12;
		result.M13 = m13;
		result.M14 = m14;
		result.M21 = m21;
		result.M22 = m22;
		result.M23 = m23;
		result.M24 = m24;
		result.M31 = m31;
		result.M32 = m32;
		result.M33 = m33;
		result.M34 = m34;
		result.M41 = m41;
		result.M42 = m42;
		result.M43 = m43;
		result.M44 = m44;
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Matrix" /> that contains a multiplication of <see cref="T:Microsoft.Xna.Framework.Matrix" /> and a scalar.
	/// </summary>
	/// <param name="matrix1">Source <see cref="T:Microsoft.Xna.Framework.Matrix" />.</param>
	/// <param name="scaleFactor">Scalar value.</param>
	/// <returns>Result of the matrix multiplication with a scalar.</returns>
	public static Matrix Multiply(Matrix matrix1, float scaleFactor)
	{
		matrix1.M11 *= scaleFactor;
		matrix1.M12 *= scaleFactor;
		matrix1.M13 *= scaleFactor;
		matrix1.M14 *= scaleFactor;
		matrix1.M21 *= scaleFactor;
		matrix1.M22 *= scaleFactor;
		matrix1.M23 *= scaleFactor;
		matrix1.M24 *= scaleFactor;
		matrix1.M31 *= scaleFactor;
		matrix1.M32 *= scaleFactor;
		matrix1.M33 *= scaleFactor;
		matrix1.M34 *= scaleFactor;
		matrix1.M41 *= scaleFactor;
		matrix1.M42 *= scaleFactor;
		matrix1.M43 *= scaleFactor;
		matrix1.M44 *= scaleFactor;
		return matrix1;
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Matrix" /> that contains a multiplication of <see cref="T:Microsoft.Xna.Framework.Matrix" /> and a scalar.
	/// </summary>
	/// <param name="matrix1">Source <see cref="T:Microsoft.Xna.Framework.Matrix" />.</param>
	/// <param name="scaleFactor">Scalar value.</param>
	/// <param name="result">Result of the matrix multiplication with a scalar as an output parameter.</param>
	public static void Multiply(ref Matrix matrix1, float scaleFactor, out Matrix result)
	{
		result.M11 = matrix1.M11 * scaleFactor;
		result.M12 = matrix1.M12 * scaleFactor;
		result.M13 = matrix1.M13 * scaleFactor;
		result.M14 = matrix1.M14 * scaleFactor;
		result.M21 = matrix1.M21 * scaleFactor;
		result.M22 = matrix1.M22 * scaleFactor;
		result.M23 = matrix1.M23 * scaleFactor;
		result.M24 = matrix1.M24 * scaleFactor;
		result.M31 = matrix1.M31 * scaleFactor;
		result.M32 = matrix1.M32 * scaleFactor;
		result.M33 = matrix1.M33 * scaleFactor;
		result.M34 = matrix1.M34 * scaleFactor;
		result.M41 = matrix1.M41 * scaleFactor;
		result.M42 = matrix1.M42 * scaleFactor;
		result.M43 = matrix1.M43 * scaleFactor;
		result.M44 = matrix1.M44 * scaleFactor;
	}

	/// <summary>
	/// Copy the values of specified <see cref="T:Microsoft.Xna.Framework.Matrix" /> to the float array.
	/// </summary>
	/// <param name="matrix">The source <see cref="T:Microsoft.Xna.Framework.Matrix" />.</param>
	/// <returns>The array which matrix values will be stored.</returns>
	/// <remarks>
	/// Required for OpenGL 2.0 projection matrix stuff.
	/// </remarks>
	public static float[] ToFloatArray(Matrix matrix)
	{
		return new float[16]
		{
			matrix.M11, matrix.M12, matrix.M13, matrix.M14, matrix.M21, matrix.M22, matrix.M23, matrix.M24, matrix.M31, matrix.M32,
			matrix.M33, matrix.M34, matrix.M41, matrix.M42, matrix.M43, matrix.M44
		};
	}

	/// <summary>
	/// Returns a matrix with the all values negated.
	/// </summary>
	/// <param name="matrix">Source <see cref="T:Microsoft.Xna.Framework.Matrix" />.</param>
	/// <returns>Result of the matrix negation.</returns>
	public static Matrix Negate(Matrix matrix)
	{
		matrix.M11 = 0f - matrix.M11;
		matrix.M12 = 0f - matrix.M12;
		matrix.M13 = 0f - matrix.M13;
		matrix.M14 = 0f - matrix.M14;
		matrix.M21 = 0f - matrix.M21;
		matrix.M22 = 0f - matrix.M22;
		matrix.M23 = 0f - matrix.M23;
		matrix.M24 = 0f - matrix.M24;
		matrix.M31 = 0f - matrix.M31;
		matrix.M32 = 0f - matrix.M32;
		matrix.M33 = 0f - matrix.M33;
		matrix.M34 = 0f - matrix.M34;
		matrix.M41 = 0f - matrix.M41;
		matrix.M42 = 0f - matrix.M42;
		matrix.M43 = 0f - matrix.M43;
		matrix.M44 = 0f - matrix.M44;
		return matrix;
	}

	/// <summary>
	/// Returns a matrix with the all values negated.
	/// </summary>
	/// <param name="matrix">Source <see cref="T:Microsoft.Xna.Framework.Matrix" />.</param>
	/// <param name="result">Result of the matrix negation as an output parameter.</param>
	public static void Negate(ref Matrix matrix, out Matrix result)
	{
		result.M11 = 0f - matrix.M11;
		result.M12 = 0f - matrix.M12;
		result.M13 = 0f - matrix.M13;
		result.M14 = 0f - matrix.M14;
		result.M21 = 0f - matrix.M21;
		result.M22 = 0f - matrix.M22;
		result.M23 = 0f - matrix.M23;
		result.M24 = 0f - matrix.M24;
		result.M31 = 0f - matrix.M31;
		result.M32 = 0f - matrix.M32;
		result.M33 = 0f - matrix.M33;
		result.M34 = 0f - matrix.M34;
		result.M41 = 0f - matrix.M41;
		result.M42 = 0f - matrix.M42;
		result.M43 = 0f - matrix.M43;
		result.M44 = 0f - matrix.M44;
	}

	/// <summary>
	/// Adds two matrixes.
	/// </summary>
	/// <param name="matrix1">Source <see cref="T:Microsoft.Xna.Framework.Matrix" /> on the left of the add sign.</param>
	/// <param name="matrix2">Source <see cref="T:Microsoft.Xna.Framework.Matrix" /> on the right of the add sign.</param>
	/// <returns>Sum of the matrixes.</returns>
	public static Matrix operator +(Matrix matrix1, Matrix matrix2)
	{
		matrix1.M11 += matrix2.M11;
		matrix1.M12 += matrix2.M12;
		matrix1.M13 += matrix2.M13;
		matrix1.M14 += matrix2.M14;
		matrix1.M21 += matrix2.M21;
		matrix1.M22 += matrix2.M22;
		matrix1.M23 += matrix2.M23;
		matrix1.M24 += matrix2.M24;
		matrix1.M31 += matrix2.M31;
		matrix1.M32 += matrix2.M32;
		matrix1.M33 += matrix2.M33;
		matrix1.M34 += matrix2.M34;
		matrix1.M41 += matrix2.M41;
		matrix1.M42 += matrix2.M42;
		matrix1.M43 += matrix2.M43;
		matrix1.M44 += matrix2.M44;
		return matrix1;
	}

	/// <summary>
	/// Divides the elements of a <see cref="T:Microsoft.Xna.Framework.Matrix" /> by the elements of another <see cref="T:Microsoft.Xna.Framework.Matrix" />.
	/// </summary>
	/// <param name="matrix1">Source <see cref="T:Microsoft.Xna.Framework.Matrix" /> on the left of the div sign.</param>
	/// <param name="matrix2">Divisor <see cref="T:Microsoft.Xna.Framework.Matrix" /> on the right of the div sign.</param>
	/// <returns>The result of dividing the matrixes.</returns>
	public static Matrix operator /(Matrix matrix1, Matrix matrix2)
	{
		matrix1.M11 /= matrix2.M11;
		matrix1.M12 /= matrix2.M12;
		matrix1.M13 /= matrix2.M13;
		matrix1.M14 /= matrix2.M14;
		matrix1.M21 /= matrix2.M21;
		matrix1.M22 /= matrix2.M22;
		matrix1.M23 /= matrix2.M23;
		matrix1.M24 /= matrix2.M24;
		matrix1.M31 /= matrix2.M31;
		matrix1.M32 /= matrix2.M32;
		matrix1.M33 /= matrix2.M33;
		matrix1.M34 /= matrix2.M34;
		matrix1.M41 /= matrix2.M41;
		matrix1.M42 /= matrix2.M42;
		matrix1.M43 /= matrix2.M43;
		matrix1.M44 /= matrix2.M44;
		return matrix1;
	}

	/// <summary>
	/// Divides the elements of a <see cref="T:Microsoft.Xna.Framework.Matrix" /> by a scalar.
	/// </summary>
	/// <param name="matrix">Source <see cref="T:Microsoft.Xna.Framework.Matrix" /> on the left of the div sign.</param>
	/// <param name="divider">Divisor scalar on the right of the div sign.</param>
	/// <returns>The result of dividing a matrix by a scalar.</returns>
	public static Matrix operator /(Matrix matrix, float divider)
	{
		float num = 1f / divider;
		matrix.M11 *= num;
		matrix.M12 *= num;
		matrix.M13 *= num;
		matrix.M14 *= num;
		matrix.M21 *= num;
		matrix.M22 *= num;
		matrix.M23 *= num;
		matrix.M24 *= num;
		matrix.M31 *= num;
		matrix.M32 *= num;
		matrix.M33 *= num;
		matrix.M34 *= num;
		matrix.M41 *= num;
		matrix.M42 *= num;
		matrix.M43 *= num;
		matrix.M44 *= num;
		return matrix;
	}

	/// <summary>
	/// Compares whether two <see cref="T:Microsoft.Xna.Framework.Matrix" /> instances are equal without any tolerance.
	/// </summary>
	/// <param name="matrix1">Source <see cref="T:Microsoft.Xna.Framework.Matrix" /> on the left of the equal sign.</param>
	/// <param name="matrix2">Source <see cref="T:Microsoft.Xna.Framework.Matrix" /> on the right of the equal sign.</param>
	/// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
	public static bool operator ==(Matrix matrix1, Matrix matrix2)
	{
		if (matrix1.M11 == matrix2.M11 && matrix1.M12 == matrix2.M12 && matrix1.M13 == matrix2.M13 && matrix1.M14 == matrix2.M14 && matrix1.M21 == matrix2.M21 && matrix1.M22 == matrix2.M22 && matrix1.M23 == matrix2.M23 && matrix1.M24 == matrix2.M24 && matrix1.M31 == matrix2.M31 && matrix1.M32 == matrix2.M32 && matrix1.M33 == matrix2.M33 && matrix1.M34 == matrix2.M34 && matrix1.M41 == matrix2.M41 && matrix1.M42 == matrix2.M42 && matrix1.M43 == matrix2.M43)
		{
			return matrix1.M44 == matrix2.M44;
		}
		return false;
	}

	/// <summary>
	/// Compares whether two <see cref="T:Microsoft.Xna.Framework.Matrix" /> instances are not equal without any tolerance.
	/// </summary>
	/// <param name="matrix1">Source <see cref="T:Microsoft.Xna.Framework.Matrix" /> on the left of the not equal sign.</param>
	/// <param name="matrix2">Source <see cref="T:Microsoft.Xna.Framework.Matrix" /> on the right of the not equal sign.</param>
	/// <returns><c>true</c> if the instances are not equal; <c>false</c> otherwise.</returns>
	public static bool operator !=(Matrix matrix1, Matrix matrix2)
	{
		if (matrix1.M11 == matrix2.M11 && matrix1.M12 == matrix2.M12 && matrix1.M13 == matrix2.M13 && matrix1.M14 == matrix2.M14 && matrix1.M21 == matrix2.M21 && matrix1.M22 == matrix2.M22 && matrix1.M23 == matrix2.M23 && matrix1.M24 == matrix2.M24 && matrix1.M31 == matrix2.M31 && matrix1.M32 == matrix2.M32 && matrix1.M33 == matrix2.M33 && matrix1.M34 == matrix2.M34 && matrix1.M41 == matrix2.M41 && matrix1.M42 == matrix2.M42 && matrix1.M43 == matrix2.M43)
		{
			return matrix1.M44 != matrix2.M44;
		}
		return true;
	}

	/// <summary>
	/// Multiplies two matrixes.
	/// </summary>
	/// <param name="matrix1">Source <see cref="T:Microsoft.Xna.Framework.Matrix" /> on the left of the mul sign.</param>
	/// <param name="matrix2">Source <see cref="T:Microsoft.Xna.Framework.Matrix" /> on the right of the mul sign.</param>
	/// <returns>Result of the matrix multiplication.</returns>
	/// <remarks>
	/// Using matrix multiplication algorithm - see http://en.wikipedia.org/wiki/Matrix_multiplication.
	/// </remarks>
	public static Matrix operator *(Matrix matrix1, Matrix matrix2)
	{
		float m11 = matrix1.M11 * matrix2.M11 + matrix1.M12 * matrix2.M21 + matrix1.M13 * matrix2.M31 + matrix1.M14 * matrix2.M41;
		float m12 = matrix1.M11 * matrix2.M12 + matrix1.M12 * matrix2.M22 + matrix1.M13 * matrix2.M32 + matrix1.M14 * matrix2.M42;
		float m13 = matrix1.M11 * matrix2.M13 + matrix1.M12 * matrix2.M23 + matrix1.M13 * matrix2.M33 + matrix1.M14 * matrix2.M43;
		float m14 = matrix1.M11 * matrix2.M14 + matrix1.M12 * matrix2.M24 + matrix1.M13 * matrix2.M34 + matrix1.M14 * matrix2.M44;
		float m21 = matrix1.M21 * matrix2.M11 + matrix1.M22 * matrix2.M21 + matrix1.M23 * matrix2.M31 + matrix1.M24 * matrix2.M41;
		float m22 = matrix1.M21 * matrix2.M12 + matrix1.M22 * matrix2.M22 + matrix1.M23 * matrix2.M32 + matrix1.M24 * matrix2.M42;
		float m23 = matrix1.M21 * matrix2.M13 + matrix1.M22 * matrix2.M23 + matrix1.M23 * matrix2.M33 + matrix1.M24 * matrix2.M43;
		float m24 = matrix1.M21 * matrix2.M14 + matrix1.M22 * matrix2.M24 + matrix1.M23 * matrix2.M34 + matrix1.M24 * matrix2.M44;
		float m31 = matrix1.M31 * matrix2.M11 + matrix1.M32 * matrix2.M21 + matrix1.M33 * matrix2.M31 + matrix1.M34 * matrix2.M41;
		float m32 = matrix1.M31 * matrix2.M12 + matrix1.M32 * matrix2.M22 + matrix1.M33 * matrix2.M32 + matrix1.M34 * matrix2.M42;
		float m33 = matrix1.M31 * matrix2.M13 + matrix1.M32 * matrix2.M23 + matrix1.M33 * matrix2.M33 + matrix1.M34 * matrix2.M43;
		float m34 = matrix1.M31 * matrix2.M14 + matrix1.M32 * matrix2.M24 + matrix1.M33 * matrix2.M34 + matrix1.M34 * matrix2.M44;
		float m41 = matrix1.M41 * matrix2.M11 + matrix1.M42 * matrix2.M21 + matrix1.M43 * matrix2.M31 + matrix1.M44 * matrix2.M41;
		float m42 = matrix1.M41 * matrix2.M12 + matrix1.M42 * matrix2.M22 + matrix1.M43 * matrix2.M32 + matrix1.M44 * matrix2.M42;
		float m43 = matrix1.M41 * matrix2.M13 + matrix1.M42 * matrix2.M23 + matrix1.M43 * matrix2.M33 + matrix1.M44 * matrix2.M43;
		float m44 = matrix1.M41 * matrix2.M14 + matrix1.M42 * matrix2.M24 + matrix1.M43 * matrix2.M34 + matrix1.M44 * matrix2.M44;
		matrix1.M11 = m11;
		matrix1.M12 = m12;
		matrix1.M13 = m13;
		matrix1.M14 = m14;
		matrix1.M21 = m21;
		matrix1.M22 = m22;
		matrix1.M23 = m23;
		matrix1.M24 = m24;
		matrix1.M31 = m31;
		matrix1.M32 = m32;
		matrix1.M33 = m33;
		matrix1.M34 = m34;
		matrix1.M41 = m41;
		matrix1.M42 = m42;
		matrix1.M43 = m43;
		matrix1.M44 = m44;
		return matrix1;
	}

	/// <summary>
	/// Multiplies the elements of matrix by a scalar.
	/// </summary>
	/// <param name="matrix">Source <see cref="T:Microsoft.Xna.Framework.Matrix" /> on the left of the mul sign.</param>
	/// <param name="scaleFactor">Scalar value on the right of the mul sign.</param>
	/// <returns>Result of the matrix multiplication with a scalar.</returns>
	public static Matrix operator *(Matrix matrix, float scaleFactor)
	{
		matrix.M11 *= scaleFactor;
		matrix.M12 *= scaleFactor;
		matrix.M13 *= scaleFactor;
		matrix.M14 *= scaleFactor;
		matrix.M21 *= scaleFactor;
		matrix.M22 *= scaleFactor;
		matrix.M23 *= scaleFactor;
		matrix.M24 *= scaleFactor;
		matrix.M31 *= scaleFactor;
		matrix.M32 *= scaleFactor;
		matrix.M33 *= scaleFactor;
		matrix.M34 *= scaleFactor;
		matrix.M41 *= scaleFactor;
		matrix.M42 *= scaleFactor;
		matrix.M43 *= scaleFactor;
		matrix.M44 *= scaleFactor;
		return matrix;
	}

	/// <summary>
	/// Subtracts the values of one <see cref="T:Microsoft.Xna.Framework.Matrix" /> from another <see cref="T:Microsoft.Xna.Framework.Matrix" />.
	/// </summary>
	/// <param name="matrix1">Source <see cref="T:Microsoft.Xna.Framework.Matrix" /> on the left of the sub sign.</param>
	/// <param name="matrix2">Source <see cref="T:Microsoft.Xna.Framework.Matrix" /> on the right of the sub sign.</param>
	/// <returns>Result of the matrix subtraction.</returns>
	public static Matrix operator -(Matrix matrix1, Matrix matrix2)
	{
		matrix1.M11 -= matrix2.M11;
		matrix1.M12 -= matrix2.M12;
		matrix1.M13 -= matrix2.M13;
		matrix1.M14 -= matrix2.M14;
		matrix1.M21 -= matrix2.M21;
		matrix1.M22 -= matrix2.M22;
		matrix1.M23 -= matrix2.M23;
		matrix1.M24 -= matrix2.M24;
		matrix1.M31 -= matrix2.M31;
		matrix1.M32 -= matrix2.M32;
		matrix1.M33 -= matrix2.M33;
		matrix1.M34 -= matrix2.M34;
		matrix1.M41 -= matrix2.M41;
		matrix1.M42 -= matrix2.M42;
		matrix1.M43 -= matrix2.M43;
		matrix1.M44 -= matrix2.M44;
		return matrix1;
	}

	/// <summary>
	/// Inverts values in the specified <see cref="T:Microsoft.Xna.Framework.Matrix" />.
	/// </summary>
	/// <param name="matrix">Source <see cref="T:Microsoft.Xna.Framework.Matrix" /> on the right of the sub sign.</param>
	/// <returns>Result of the inversion.</returns>
	public static Matrix operator -(Matrix matrix)
	{
		matrix.M11 = 0f - matrix.M11;
		matrix.M12 = 0f - matrix.M12;
		matrix.M13 = 0f - matrix.M13;
		matrix.M14 = 0f - matrix.M14;
		matrix.M21 = 0f - matrix.M21;
		matrix.M22 = 0f - matrix.M22;
		matrix.M23 = 0f - matrix.M23;
		matrix.M24 = 0f - matrix.M24;
		matrix.M31 = 0f - matrix.M31;
		matrix.M32 = 0f - matrix.M32;
		matrix.M33 = 0f - matrix.M33;
		matrix.M34 = 0f - matrix.M34;
		matrix.M41 = 0f - matrix.M41;
		matrix.M42 = 0f - matrix.M42;
		matrix.M43 = 0f - matrix.M43;
		matrix.M44 = 0f - matrix.M44;
		return matrix;
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Matrix" /> that contains subtraction of one matrix from another.
	/// </summary>
	/// <param name="matrix1">The first <see cref="T:Microsoft.Xna.Framework.Matrix" />.</param>
	/// <param name="matrix2">The second <see cref="T:Microsoft.Xna.Framework.Matrix" />.</param>
	/// <returns>The result of the matrix subtraction.</returns>
	public static Matrix Subtract(Matrix matrix1, Matrix matrix2)
	{
		matrix1.M11 -= matrix2.M11;
		matrix1.M12 -= matrix2.M12;
		matrix1.M13 -= matrix2.M13;
		matrix1.M14 -= matrix2.M14;
		matrix1.M21 -= matrix2.M21;
		matrix1.M22 -= matrix2.M22;
		matrix1.M23 -= matrix2.M23;
		matrix1.M24 -= matrix2.M24;
		matrix1.M31 -= matrix2.M31;
		matrix1.M32 -= matrix2.M32;
		matrix1.M33 -= matrix2.M33;
		matrix1.M34 -= matrix2.M34;
		matrix1.M41 -= matrix2.M41;
		matrix1.M42 -= matrix2.M42;
		matrix1.M43 -= matrix2.M43;
		matrix1.M44 -= matrix2.M44;
		return matrix1;
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Matrix" /> that contains subtraction of one matrix from another.
	/// </summary>
	/// <param name="matrix1">The first <see cref="T:Microsoft.Xna.Framework.Matrix" />.</param>
	/// <param name="matrix2">The second <see cref="T:Microsoft.Xna.Framework.Matrix" />.</param>
	/// <param name="result">The result of the matrix subtraction as an output parameter.</param>
	public static void Subtract(ref Matrix matrix1, ref Matrix matrix2, out Matrix result)
	{
		result.M11 = matrix1.M11 - matrix2.M11;
		result.M12 = matrix1.M12 - matrix2.M12;
		result.M13 = matrix1.M13 - matrix2.M13;
		result.M14 = matrix1.M14 - matrix2.M14;
		result.M21 = matrix1.M21 - matrix2.M21;
		result.M22 = matrix1.M22 - matrix2.M22;
		result.M23 = matrix1.M23 - matrix2.M23;
		result.M24 = matrix1.M24 - matrix2.M24;
		result.M31 = matrix1.M31 - matrix2.M31;
		result.M32 = matrix1.M32 - matrix2.M32;
		result.M33 = matrix1.M33 - matrix2.M33;
		result.M34 = matrix1.M34 - matrix2.M34;
		result.M41 = matrix1.M41 - matrix2.M41;
		result.M42 = matrix1.M42 - matrix2.M42;
		result.M43 = matrix1.M43 - matrix2.M43;
		result.M44 = matrix1.M44 - matrix2.M44;
	}

	/// <summary>
	/// Returns a <see cref="T:System.String" /> representation of this <see cref="T:Microsoft.Xna.Framework.Matrix" /> in the format:
	/// {M11:[<see cref="F:Microsoft.Xna.Framework.Matrix.M11" />] M12:[<see cref="F:Microsoft.Xna.Framework.Matrix.M12" />] M13:[<see cref="F:Microsoft.Xna.Framework.Matrix.M13" />] M14:[<see cref="F:Microsoft.Xna.Framework.Matrix.M14" />]}
	/// {M21:[<see cref="F:Microsoft.Xna.Framework.Matrix.M21" />] M12:[<see cref="F:Microsoft.Xna.Framework.Matrix.M22" />] M13:[<see cref="F:Microsoft.Xna.Framework.Matrix.M23" />] M14:[<see cref="F:Microsoft.Xna.Framework.Matrix.M24" />]}
	/// {M31:[<see cref="F:Microsoft.Xna.Framework.Matrix.M31" />] M32:[<see cref="F:Microsoft.Xna.Framework.Matrix.M32" />] M33:[<see cref="F:Microsoft.Xna.Framework.Matrix.M33" />] M34:[<see cref="F:Microsoft.Xna.Framework.Matrix.M34" />]}
	/// {M41:[<see cref="F:Microsoft.Xna.Framework.Matrix.M41" />] M42:[<see cref="F:Microsoft.Xna.Framework.Matrix.M42" />] M43:[<see cref="F:Microsoft.Xna.Framework.Matrix.M43" />] M44:[<see cref="F:Microsoft.Xna.Framework.Matrix.M44" />]}
	/// </summary>
	/// <returns>A <see cref="T:System.String" /> representation of this <see cref="T:Microsoft.Xna.Framework.Matrix" />.</returns>
	public override string ToString()
	{
		return "{M11:" + this.M11 + " M12:" + this.M12 + " M13:" + this.M13 + " M14:" + this.M14 + "} {M21:" + this.M21 + " M22:" + this.M22 + " M23:" + this.M23 + " M24:" + this.M24 + "} {M31:" + this.M31 + " M32:" + this.M32 + " M33:" + this.M33 + " M34:" + this.M34 + "} {M41:" + this.M41 + " M42:" + this.M42 + " M43:" + this.M43 + " M44:" + this.M44 + "}";
	}

	/// <summary>
	/// Swap the matrix rows and columns.
	/// </summary>
	/// <param name="matrix">The matrix for transposing operation.</param>
	/// <returns>The new <see cref="T:Microsoft.Xna.Framework.Matrix" /> which contains the transposing result.</returns>
	public static Matrix Transpose(Matrix matrix)
	{
		Matrix.Transpose(ref matrix, out var ret);
		return ret;
	}

	/// <summary>
	/// Swap the matrix rows and columns.
	/// </summary>
	/// <param name="matrix">The matrix for transposing operation.</param>
	/// <param name="result">The new <see cref="T:Microsoft.Xna.Framework.Matrix" /> which contains the transposing result as an output parameter.</param>
	public static void Transpose(ref Matrix matrix, out Matrix result)
	{
		Matrix ret = default(Matrix);
		ret.M11 = matrix.M11;
		ret.M12 = matrix.M21;
		ret.M13 = matrix.M31;
		ret.M14 = matrix.M41;
		ret.M21 = matrix.M12;
		ret.M22 = matrix.M22;
		ret.M23 = matrix.M32;
		ret.M24 = matrix.M42;
		ret.M31 = matrix.M13;
		ret.M32 = matrix.M23;
		ret.M33 = matrix.M33;
		ret.M34 = matrix.M43;
		ret.M41 = matrix.M14;
		ret.M42 = matrix.M24;
		ret.M43 = matrix.M34;
		ret.M44 = matrix.M44;
		result = ret;
	}

	/// <summary>
	/// Helper method for using the Laplace expansion theorem using two rows expansions to calculate major and 
	/// minor determinants of a 4x4 matrix. This method is used for inverting a matrix.
	/// </summary>
	private static void FindDeterminants(ref Matrix matrix, out float major, out float minor1, out float minor2, out float minor3, out float minor4, out float minor5, out float minor6, out float minor7, out float minor8, out float minor9, out float minor10, out float minor11, out float minor12)
	{
		double det1 = (double)matrix.M11 * (double)matrix.M22 - (double)matrix.M12 * (double)matrix.M21;
		double det2 = (double)matrix.M11 * (double)matrix.M23 - (double)matrix.M13 * (double)matrix.M21;
		double det3 = (double)matrix.M11 * (double)matrix.M24 - (double)matrix.M14 * (double)matrix.M21;
		double det4 = (double)matrix.M12 * (double)matrix.M23 - (double)matrix.M13 * (double)matrix.M22;
		double det5 = (double)matrix.M12 * (double)matrix.M24 - (double)matrix.M14 * (double)matrix.M22;
		double det6 = (double)matrix.M13 * (double)matrix.M24 - (double)matrix.M14 * (double)matrix.M23;
		double det7 = (double)matrix.M31 * (double)matrix.M42 - (double)matrix.M32 * (double)matrix.M41;
		double det8 = (double)matrix.M31 * (double)matrix.M43 - (double)matrix.M33 * (double)matrix.M41;
		double det9 = (double)matrix.M31 * (double)matrix.M44 - (double)matrix.M34 * (double)matrix.M41;
		double det10 = (double)matrix.M32 * (double)matrix.M43 - (double)matrix.M33 * (double)matrix.M42;
		double det11 = (double)matrix.M32 * (double)matrix.M44 - (double)matrix.M34 * (double)matrix.M42;
		double det12 = (double)matrix.M33 * (double)matrix.M44 - (double)matrix.M34 * (double)matrix.M43;
		major = (float)(det1 * det12 - det2 * det11 + det3 * det10 + det4 * det9 - det5 * det8 + det6 * det7);
		minor1 = (float)det1;
		minor2 = (float)det2;
		minor3 = (float)det3;
		minor4 = (float)det4;
		minor5 = (float)det5;
		minor6 = (float)det6;
		minor7 = (float)det7;
		minor8 = (float)det8;
		minor9 = (float)det9;
		minor10 = (float)det10;
		minor11 = (float)det11;
		minor12 = (float)det12;
	}
}
