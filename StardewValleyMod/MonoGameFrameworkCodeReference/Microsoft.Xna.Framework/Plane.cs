using System;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Microsoft.Xna.Framework;

/// <summary>
/// A plane in 3d space, represented by its normal away from the origin and its distance from the origin, D.
/// </summary>
[DataContract]
[DebuggerDisplay("{DebugDisplayString,nq}")]
public struct Plane : IEquatable<Plane>
{
	/// <summary>
	/// The distance of the <see cref="T:Microsoft.Xna.Framework.Plane" /> to the origin.
	/// </summary>
	[DataMember]
	public float D;

	/// <summary>
	/// The normal of the <see cref="T:Microsoft.Xna.Framework.Plane" />.
	/// </summary>
	[DataMember]
	public Vector3 Normal;

	internal string DebugDisplayString => this.Normal.DebugDisplayString + "  " + this.D;

	/// <summary>
	/// Create a <see cref="T:Microsoft.Xna.Framework.Plane" /> with the first three components of the specified <see cref="T:Microsoft.Xna.Framework.Vector4" />
	/// as the normal and the last component as the distance to the origin.
	/// </summary>
	/// <param name="value">A vector holding the normal and distance to origin.</param>
	public Plane(Vector4 value)
		: this(new Vector3(value.X, value.Y, value.Z), value.W)
	{
	}

	/// <summary>
	/// Create a <see cref="T:Microsoft.Xna.Framework.Plane" /> with the specified normal and distance to the origin.
	/// </summary>
	/// <param name="normal">The normal of the plane.</param>
	/// <param name="d">The distance to the origin.</param>
	public Plane(Vector3 normal, float d)
	{
		this.Normal = normal;
		this.D = d;
	}

	/// <summary>
	/// Create the <see cref="T:Microsoft.Xna.Framework.Plane" /> that contains the three specified points.
	/// </summary>
	/// <param name="a">A point the created <see cref="T:Microsoft.Xna.Framework.Plane" /> should contain.</param>
	/// <param name="b">A point the created <see cref="T:Microsoft.Xna.Framework.Plane" /> should contain.</param>
	/// <param name="c">A point the created <see cref="T:Microsoft.Xna.Framework.Plane" /> should contain.</param>
	public Plane(Vector3 a, Vector3 b, Vector3 c)
	{
		Vector3 vector = b - a;
		Vector3 ac = c - a;
		Vector3 cross = Vector3.Cross(vector, ac);
		Vector3.Normalize(ref cross, out this.Normal);
		this.D = 0f - Vector3.Dot(this.Normal, a);
	}

	/// <summary>
	/// Create a <see cref="T:Microsoft.Xna.Framework.Plane" /> with the first three values as the X, Y and Z
	/// components of the normal and the last value as the distance to the origin.
	/// </summary>
	/// <param name="a">The X component of the normal.</param>
	/// <param name="b">The Y component of the normal.</param>
	/// <param name="c">The Z component of the normal.</param>
	/// <param name="d">The distance to the origin.</param>
	public Plane(float a, float b, float c, float d)
		: this(new Vector3(a, b, c), d)
	{
	}

	/// <summary>
	/// Create a <see cref="T:Microsoft.Xna.Framework.Plane" /> that contains the specified point and has the specified <see cref="F:Microsoft.Xna.Framework.Plane.Normal" /> vector.
	/// </summary>
	/// <param name="pointOnPlane">A point the created <see cref="T:Microsoft.Xna.Framework.Plane" /> should contain.</param>
	/// <param name="normal">The normal of the plane.</param>
	public Plane(Vector3 pointOnPlane, Vector3 normal)
	{
		this.Normal = normal;
		this.D = 0f - (pointOnPlane.X * normal.X + pointOnPlane.Y * normal.Y + pointOnPlane.Z * normal.Z);
	}

	/// <summary>
	/// Get the dot product of a <see cref="T:Microsoft.Xna.Framework.Vector4" /> with this <see cref="T:Microsoft.Xna.Framework.Plane" />.
	/// </summary>
	/// <param name="value">The <see cref="T:Microsoft.Xna.Framework.Vector4" /> to calculate the dot product with.</param>
	/// <returns>The dot product of the specified <see cref="T:Microsoft.Xna.Framework.Vector4" /> and this <see cref="T:Microsoft.Xna.Framework.Plane" />.</returns>
	public float Dot(Vector4 value)
	{
		return this.Normal.X * value.X + this.Normal.Y * value.Y + this.Normal.Z * value.Z + this.D * value.W;
	}

	/// <summary>
	/// Get the dot product of a <see cref="T:Microsoft.Xna.Framework.Vector4" /> with this <see cref="T:Microsoft.Xna.Framework.Plane" />.
	/// </summary>
	/// <param name="value">The <see cref="T:Microsoft.Xna.Framework.Vector4" /> to calculate the dot product with.</param>
	/// <param name="result">
	/// The dot product of the specified <see cref="T:Microsoft.Xna.Framework.Vector4" /> and this <see cref="T:Microsoft.Xna.Framework.Plane" />.
	/// </param>
	public void Dot(ref Vector4 value, out float result)
	{
		result = this.Normal.X * value.X + this.Normal.Y * value.Y + this.Normal.Z * value.Z + this.D * value.W;
	}

	/// <summary>
	/// Get the dot product of a <see cref="T:Microsoft.Xna.Framework.Vector3" /> with
	/// the <see cref="F:Microsoft.Xna.Framework.Plane.Normal" /> vector of this <see cref="T:Microsoft.Xna.Framework.Plane" />
	/// plus the <see cref="F:Microsoft.Xna.Framework.Plane.D" /> value of this <see cref="T:Microsoft.Xna.Framework.Plane" />.
	/// </summary>
	/// <param name="value">The <see cref="T:Microsoft.Xna.Framework.Vector3" /> to calculate the dot product with.</param>
	/// <returns>
	/// The dot product of the specified <see cref="T:Microsoft.Xna.Framework.Vector3" /> and the normal of this <see cref="T:Microsoft.Xna.Framework.Plane" />
	/// plus the <see cref="F:Microsoft.Xna.Framework.Plane.D" /> value of this <see cref="T:Microsoft.Xna.Framework.Plane" />.
	/// </returns>
	public float DotCoordinate(Vector3 value)
	{
		return this.Normal.X * value.X + this.Normal.Y * value.Y + this.Normal.Z * value.Z + this.D;
	}

	/// <summary>
	/// Get the dot product of a <see cref="T:Microsoft.Xna.Framework.Vector3" /> with
	/// the <see cref="F:Microsoft.Xna.Framework.Plane.Normal" /> vector of this <see cref="T:Microsoft.Xna.Framework.Plane" />
	/// plus the <see cref="F:Microsoft.Xna.Framework.Plane.D" /> value of this <see cref="T:Microsoft.Xna.Framework.Plane" />.
	/// </summary>
	/// <param name="value">The <see cref="T:Microsoft.Xna.Framework.Vector3" /> to calculate the dot product with.</param>
	/// <param name="result">
	/// The dot product of the specified <see cref="T:Microsoft.Xna.Framework.Vector3" /> and the normal of this <see cref="T:Microsoft.Xna.Framework.Plane" />
	/// plus the <see cref="F:Microsoft.Xna.Framework.Plane.D" /> value of this <see cref="T:Microsoft.Xna.Framework.Plane" />.
	/// </param>
	public void DotCoordinate(ref Vector3 value, out float result)
	{
		result = this.Normal.X * value.X + this.Normal.Y * value.Y + this.Normal.Z * value.Z + this.D;
	}

	/// <summary>
	/// Get the dot product of a <see cref="T:Microsoft.Xna.Framework.Vector3" /> with
	/// the <see cref="F:Microsoft.Xna.Framework.Plane.Normal" /> vector of this <see cref="T:Microsoft.Xna.Framework.Plane" />.
	/// </summary>
	/// <param name="value">The <see cref="T:Microsoft.Xna.Framework.Vector3" /> to calculate the dot product with.</param>
	/// <returns>
	/// The dot product of the specified <see cref="T:Microsoft.Xna.Framework.Vector3" /> and the normal of this <see cref="T:Microsoft.Xna.Framework.Plane" />.
	/// </returns>
	public float DotNormal(Vector3 value)
	{
		return this.Normal.X * value.X + this.Normal.Y * value.Y + this.Normal.Z * value.Z;
	}

	/// <summary>
	/// Get the dot product of a <see cref="T:Microsoft.Xna.Framework.Vector3" /> with
	/// the <see cref="F:Microsoft.Xna.Framework.Plane.Normal" /> vector of this <see cref="T:Microsoft.Xna.Framework.Plane" />.
	/// </summary>
	/// <param name="value">The <see cref="T:Microsoft.Xna.Framework.Vector3" /> to calculate the dot product with.</param>
	/// <param name="result">
	/// The dot product of the specified <see cref="T:Microsoft.Xna.Framework.Vector3" /> and the normal of this <see cref="T:Microsoft.Xna.Framework.Plane" />.
	/// </param>
	public void DotNormal(ref Vector3 value, out float result)
	{
		result = this.Normal.X * value.X + this.Normal.Y * value.Y + this.Normal.Z * value.Z;
	}

	/// <summary>
	/// Transforms a normalized plane by a matrix.
	/// </summary>
	/// <param name="plane">The normalized plane to transform.</param>
	/// <param name="matrix">The transformation matrix.</param>
	/// <returns>The transformed plane.</returns>
	public static Plane Transform(Plane plane, Matrix matrix)
	{
		Plane.Transform(ref plane, ref matrix, out var result);
		return result;
	}

	/// <summary>
	/// Transforms a normalized plane by a matrix.
	/// </summary>
	/// <param name="plane">The normalized plane to transform.</param>
	/// <param name="matrix">The transformation matrix.</param>
	/// <param name="result">The transformed plane.</param>
	public static void Transform(ref Plane plane, ref Matrix matrix, out Plane result)
	{
		Matrix.Invert(ref matrix, out var transformedMatrix);
		Matrix.Transpose(ref transformedMatrix, out transformedMatrix);
		Vector4 vector = new Vector4(plane.Normal, plane.D);
		Vector4.Transform(ref vector, ref transformedMatrix, out var transformedVector);
		result = new Plane(transformedVector);
	}

	/// <summary>
	/// Transforms a normalized plane by a quaternion rotation.
	/// </summary>
	/// <param name="plane">The normalized plane to transform.</param>
	/// <param name="rotation">The quaternion rotation.</param>
	/// <returns>The transformed plane.</returns>
	public static Plane Transform(Plane plane, Quaternion rotation)
	{
		Plane.Transform(ref plane, ref rotation, out var result);
		return result;
	}

	/// <summary>
	/// Transforms a normalized plane by a quaternion rotation.
	/// </summary>
	/// <param name="plane">The normalized plane to transform.</param>
	/// <param name="rotation">The quaternion rotation.</param>
	/// <param name="result">The transformed plane.</param>
	public static void Transform(ref Plane plane, ref Quaternion rotation, out Plane result)
	{
		Vector3.Transform(ref plane.Normal, ref rotation, out result.Normal);
		result.D = plane.D;
	}

	/// <summary>
	/// Normalize the normal vector of this plane.
	/// </summary>
	public void Normalize()
	{
		float length = this.Normal.Length();
		float factor = 1f / length;
		Vector3.Multiply(ref this.Normal, factor, out this.Normal);
		this.D *= factor;
	}

	/// <summary>
	/// Get a normalized version of the specified plane.
	/// </summary>
	/// <param name="value">The <see cref="T:Microsoft.Xna.Framework.Plane" /> to normalize.</param>
	/// <returns>A normalized version of the specified <see cref="T:Microsoft.Xna.Framework.Plane" />.</returns>
	public static Plane Normalize(Plane value)
	{
		Plane.Normalize(ref value, out var ret);
		return ret;
	}

	/// <summary>
	/// Get a normalized version of the specified plane.
	/// </summary>
	/// <param name="value">The <see cref="T:Microsoft.Xna.Framework.Plane" /> to normalize.</param>
	/// <param name="result">A normalized version of the specified <see cref="T:Microsoft.Xna.Framework.Plane" />.</param>
	public static void Normalize(ref Plane value, out Plane result)
	{
		float length = value.Normal.Length();
		float factor = 1f / length;
		Vector3.Multiply(ref value.Normal, factor, out result.Normal);
		result.D = value.D * factor;
	}

	/// <summary>
	/// Check if two planes are not equal.
	/// </summary>
	/// <param name="plane1">A <see cref="T:Microsoft.Xna.Framework.Plane" /> to check for inequality.</param>
	/// <param name="plane2">A <see cref="T:Microsoft.Xna.Framework.Plane" /> to check for inequality.</param>
	/// <returns><code>true</code> if the two planes are not equal, <code>false</code> if they are.</returns>
	public static bool operator !=(Plane plane1, Plane plane2)
	{
		return !plane1.Equals(plane2);
	}

	/// <summary>
	/// Check if two planes are equal.
	/// </summary>
	/// <param name="plane1">A <see cref="T:Microsoft.Xna.Framework.Plane" /> to check for equality.</param>
	/// <param name="plane2">A <see cref="T:Microsoft.Xna.Framework.Plane" /> to check for equality.</param>
	/// <returns><code>true</code> if the two planes are equal, <code>false</code> if they are not.</returns>
	public static bool operator ==(Plane plane1, Plane plane2)
	{
		return plane1.Equals(plane2);
	}

	/// <summary>
	/// Check if this <see cref="T:Microsoft.Xna.Framework.Plane" /> is equal to another <see cref="T:Microsoft.Xna.Framework.Plane" />.
	/// </summary>
	/// <param name="other">An <see cref="T:System.Object" /> to check for equality with this <see cref="T:Microsoft.Xna.Framework.Plane" />.</param>
	/// <returns>
	/// <code>true</code> if the specified <see cref="T:System.Object" /> is equal to this <see cref="T:Microsoft.Xna.Framework.Plane" />,
	/// <code>false</code> if it is not.
	/// </returns>
	public override bool Equals(object other)
	{
		if (!(other is Plane))
		{
			return false;
		}
		return this.Equals((Plane)other);
	}

	/// <summary>
	/// Check if this <see cref="T:Microsoft.Xna.Framework.Plane" /> is equal to another <see cref="T:Microsoft.Xna.Framework.Plane" />.
	/// </summary>
	/// <param name="other">A <see cref="T:Microsoft.Xna.Framework.Plane" /> to check for equality with this <see cref="T:Microsoft.Xna.Framework.Plane" />.</param>
	/// <returns>
	/// <code>true</code> if the specified <see cref="T:Microsoft.Xna.Framework.Plane" /> is equal to this one,
	/// <code>false</code> if it is not.
	/// </returns>
	public bool Equals(Plane other)
	{
		if (this.Normal == other.Normal)
		{
			return this.D == other.D;
		}
		return false;
	}

	/// <summary>
	/// Get a hash code for this <see cref="T:Microsoft.Xna.Framework.Plane" />.
	/// </summary>
	/// <returns>A hash code for this <see cref="T:Microsoft.Xna.Framework.Plane" />.</returns>
	public override int GetHashCode()
	{
		return this.Normal.GetHashCode() ^ this.D.GetHashCode();
	}

	/// <summary>
	/// Check if this <see cref="T:Microsoft.Xna.Framework.Plane" /> intersects a <see cref="T:Microsoft.Xna.Framework.BoundingBox" />.
	/// </summary>
	/// <param name="box">The <see cref="T:Microsoft.Xna.Framework.BoundingBox" /> to test for intersection.</param>
	/// <returns>
	/// The type of intersection of this <see cref="T:Microsoft.Xna.Framework.Plane" /> with the specified <see cref="T:Microsoft.Xna.Framework.BoundingBox" />.
	/// </returns>
	public PlaneIntersectionType Intersects(BoundingBox box)
	{
		return box.Intersects(this);
	}

	/// <summary>
	/// Check if this <see cref="T:Microsoft.Xna.Framework.Plane" /> intersects a <see cref="T:Microsoft.Xna.Framework.BoundingBox" />.
	/// </summary>
	/// <param name="box">The <see cref="T:Microsoft.Xna.Framework.BoundingBox" /> to test for intersection.</param>
	/// <param name="result">
	/// The type of intersection of this <see cref="T:Microsoft.Xna.Framework.Plane" /> with the specified <see cref="T:Microsoft.Xna.Framework.BoundingBox" />.
	/// </param>
	public void Intersects(ref BoundingBox box, out PlaneIntersectionType result)
	{
		box.Intersects(ref this, out result);
	}

	/// <summary>
	/// Check if this <see cref="T:Microsoft.Xna.Framework.Plane" /> intersects a <see cref="T:Microsoft.Xna.Framework.BoundingFrustum" />.
	/// </summary>
	/// <param name="frustum">The <see cref="T:Microsoft.Xna.Framework.BoundingFrustum" /> to test for intersection.</param>
	/// <returns>
	/// The type of intersection of this <see cref="T:Microsoft.Xna.Framework.Plane" /> with the specified <see cref="T:Microsoft.Xna.Framework.BoundingFrustum" />.
	/// </returns>
	public PlaneIntersectionType Intersects(BoundingFrustum frustum)
	{
		return frustum.Intersects(this);
	}

	/// <summary>
	/// Check if this <see cref="T:Microsoft.Xna.Framework.Plane" /> intersects a <see cref="T:Microsoft.Xna.Framework.BoundingSphere" />.
	/// </summary>
	/// <param name="sphere">The <see cref="T:Microsoft.Xna.Framework.BoundingSphere" /> to test for intersection.</param>
	/// <returns>
	/// The type of intersection of this <see cref="T:Microsoft.Xna.Framework.Plane" /> with the specified <see cref="T:Microsoft.Xna.Framework.BoundingSphere" />.
	/// </returns>
	public PlaneIntersectionType Intersects(BoundingSphere sphere)
	{
		return sphere.Intersects(this);
	}

	/// <summary>
	/// Check if this <see cref="T:Microsoft.Xna.Framework.Plane" /> intersects a <see cref="T:Microsoft.Xna.Framework.BoundingSphere" />.
	/// </summary>
	/// <param name="sphere">The <see cref="T:Microsoft.Xna.Framework.BoundingSphere" /> to test for intersection.</param>
	/// <param name="result">
	/// The type of intersection of this <see cref="T:Microsoft.Xna.Framework.Plane" /> with the specified <see cref="T:Microsoft.Xna.Framework.BoundingSphere" />.
	/// </param>
	public void Intersects(ref BoundingSphere sphere, out PlaneIntersectionType result)
	{
		sphere.Intersects(ref this, out result);
	}

	internal PlaneIntersectionType Intersects(ref Vector3 point)
	{
		this.DotCoordinate(ref point, out var distance);
		if (distance > 0f)
		{
			return PlaneIntersectionType.Front;
		}
		if (distance < 0f)
		{
			return PlaneIntersectionType.Back;
		}
		return PlaneIntersectionType.Intersecting;
	}

	/// <summary>
	/// Get a <see cref="T:System.String" /> representation of this <see cref="T:Microsoft.Xna.Framework.Plane" />.
	/// </summary>
	/// <returns>A <see cref="T:System.String" /> representation of this <see cref="T:Microsoft.Xna.Framework.Plane" />.</returns>
	public override string ToString()
	{
		string[] obj = new string[5] { "{Normal:", null, null, null, null };
		Vector3 normal = this.Normal;
		obj[1] = normal.ToString();
		obj[2] = " D:";
		obj[3] = this.D.ToString();
		obj[4] = "}";
		return string.Concat(obj);
	}

	/// <summary>
	/// Deconstruction method for <see cref="T:Microsoft.Xna.Framework.Plane" />.
	/// </summary>
	/// <param name="normal"></param>
	/// <param name="d"></param>
	public void Deconstruct(out Vector3 normal, out float d)
	{
		normal = this.Normal;
		d = this.D;
	}
}
