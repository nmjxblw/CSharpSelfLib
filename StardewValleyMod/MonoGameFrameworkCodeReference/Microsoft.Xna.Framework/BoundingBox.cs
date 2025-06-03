using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Microsoft.Xna.Framework;

/// <summary>
/// Represents an axis-aligned bounding box (AABB) in 3D space.
/// </summary>
[DataContract]
[DebuggerDisplay("{DebugDisplayString,nq}")]
public struct BoundingBox : IEquatable<BoundingBox>
{
	/// <summary>
	///   The minimum extent of this <see cref="T:Microsoft.Xna.Framework.BoundingBox" />.
	/// </summary>
	[DataMember]
	public Vector3 Min;

	/// <summary>
	///   The maximum extent of this <see cref="T:Microsoft.Xna.Framework.BoundingBox" />.
	/// </summary>
	[DataMember]
	public Vector3 Max;

	/// <summary>
	///   The number of corners in a <see cref="T:Microsoft.Xna.Framework.BoundingBox" />. This is equal to 8.
	/// </summary>
	public const int CornerCount = 8;

	private static readonly Vector3 MaxVector3 = new Vector3(float.MaxValue);

	private static readonly Vector3 MinVector3 = new Vector3(float.MinValue);

	internal string DebugDisplayString => "Min( " + this.Min.DebugDisplayString + " )  \r\n" + "Max( " + this.Max.DebugDisplayString + " )";

	/// <summary>
	///   Create a <see cref="T:Microsoft.Xna.Framework.BoundingBox" />.
	/// </summary>
	/// <param name="min">The minimum extent of the <see cref="T:Microsoft.Xna.Framework.BoundingBox" />.</param>
	/// <param name="max">The maximum extent of the <see cref="T:Microsoft.Xna.Framework.BoundingBox" />.</param>
	public BoundingBox(Vector3 min, Vector3 max)
	{
		this.Min = min;
		this.Max = max;
	}

	/// <summary>
	///   Check if this <see cref="T:Microsoft.Xna.Framework.BoundingBox" /> contains another <see cref="T:Microsoft.Xna.Framework.BoundingBox" />.
	/// </summary>
	/// <param name="box">The <see cref="T:Microsoft.Xna.Framework.BoundingBox" /> to test for overlap.</param>
	/// <returns>
	///   A value indicating if this <see cref="T:Microsoft.Xna.Framework.BoundingBox" /> contains,
	///   intersects with or is disjoint with <paramref name="box" />.
	/// </returns>
	public ContainmentType Contains(BoundingBox box)
	{
		if (box.Max.X < this.Min.X || box.Min.X > this.Max.X || box.Max.Y < this.Min.Y || box.Min.Y > this.Max.Y || box.Max.Z < this.Min.Z || box.Min.Z > this.Max.Z)
		{
			return ContainmentType.Disjoint;
		}
		if (box.Min.X >= this.Min.X && box.Max.X <= this.Max.X && box.Min.Y >= this.Min.Y && box.Max.Y <= this.Max.Y && box.Min.Z >= this.Min.Z && box.Max.Z <= this.Max.Z)
		{
			return ContainmentType.Contains;
		}
		return ContainmentType.Intersects;
	}

	/// <summary>
	///   Check if this <see cref="T:Microsoft.Xna.Framework.BoundingBox" /> contains another <see cref="T:Microsoft.Xna.Framework.BoundingBox" />.
	/// </summary>
	/// <param name="box">The <see cref="T:Microsoft.Xna.Framework.BoundingBox" /> to test for overlap.</param>
	/// <param name="result">
	///   A value indicating if this <see cref="T:Microsoft.Xna.Framework.BoundingBox" /> contains,
	///   intersects with or is disjoint with <paramref name="box" />.
	/// </param>
	public void Contains(ref BoundingBox box, out ContainmentType result)
	{
		result = this.Contains(box);
	}

	/// <summary>
	///   Check if this <see cref="T:Microsoft.Xna.Framework.BoundingBox" /> contains a <see cref="T:Microsoft.Xna.Framework.BoundingFrustum" />.
	/// </summary>
	/// <param name="frustum">The <see cref="T:Microsoft.Xna.Framework.BoundingFrustum" /> to test for overlap.</param>
	/// <returns>
	///   A value indicating if this <see cref="T:Microsoft.Xna.Framework.BoundingBox" /> contains,
	///   intersects with or is disjoint with <paramref name="frustum" />.
	/// </returns>
	public ContainmentType Contains(BoundingFrustum frustum)
	{
		Vector3[] corners = frustum.GetCorners();
		ContainmentType contained;
		int i;
		for (i = 0; i < corners.Length; i++)
		{
			this.Contains(ref corners[i], out contained);
			if (contained == ContainmentType.Disjoint)
			{
				break;
			}
		}
		if (i == corners.Length)
		{
			return ContainmentType.Contains;
		}
		if (i != 0)
		{
			return ContainmentType.Intersects;
		}
		for (i++; i < corners.Length; i++)
		{
			this.Contains(ref corners[i], out contained);
			if (contained != ContainmentType.Contains)
			{
				return ContainmentType.Intersects;
			}
		}
		return ContainmentType.Contains;
	}

	/// <summary>
	///   Check if this <see cref="T:Microsoft.Xna.Framework.BoundingBox" /> contains a <see cref="T:Microsoft.Xna.Framework.BoundingSphere" />.
	/// </summary>
	/// <param name="sphere">The <see cref="T:Microsoft.Xna.Framework.BoundingSphere" /> to test for overlap.</param>
	/// <returns>
	///   A value indicating if this <see cref="T:Microsoft.Xna.Framework.BoundingBox" /> contains,
	///   intersects with or is disjoint with <paramref name="sphere" />.
	/// </returns>
	public ContainmentType Contains(BoundingSphere sphere)
	{
		if (sphere.Center.X - this.Min.X >= sphere.Radius && sphere.Center.Y - this.Min.Y >= sphere.Radius && sphere.Center.Z - this.Min.Z >= sphere.Radius && this.Max.X - sphere.Center.X >= sphere.Radius && this.Max.Y - sphere.Center.Y >= sphere.Radius && this.Max.Z - sphere.Center.Z >= sphere.Radius)
		{
			return ContainmentType.Contains;
		}
		double dmin = 0.0;
		double e = sphere.Center.X - this.Min.X;
		if (e < 0.0)
		{
			if (e < (double)(0f - sphere.Radius))
			{
				return ContainmentType.Disjoint;
			}
			dmin += e * e;
		}
		else
		{
			e = sphere.Center.X - this.Max.X;
			if (e > 0.0)
			{
				if (e > (double)sphere.Radius)
				{
					return ContainmentType.Disjoint;
				}
				dmin += e * e;
			}
		}
		e = sphere.Center.Y - this.Min.Y;
		if (e < 0.0)
		{
			if (e < (double)(0f - sphere.Radius))
			{
				return ContainmentType.Disjoint;
			}
			dmin += e * e;
		}
		else
		{
			e = sphere.Center.Y - this.Max.Y;
			if (e > 0.0)
			{
				if (e > (double)sphere.Radius)
				{
					return ContainmentType.Disjoint;
				}
				dmin += e * e;
			}
		}
		e = sphere.Center.Z - this.Min.Z;
		if (e < 0.0)
		{
			if (e < (double)(0f - sphere.Radius))
			{
				return ContainmentType.Disjoint;
			}
			dmin += e * e;
		}
		else
		{
			e = sphere.Center.Z - this.Max.Z;
			if (e > 0.0)
			{
				if (e > (double)sphere.Radius)
				{
					return ContainmentType.Disjoint;
				}
				dmin += e * e;
			}
		}
		if (dmin <= (double)(sphere.Radius * sphere.Radius))
		{
			return ContainmentType.Intersects;
		}
		return ContainmentType.Disjoint;
	}

	/// <summary>
	///   Check if this <see cref="T:Microsoft.Xna.Framework.BoundingBox" /> contains a <see cref="T:Microsoft.Xna.Framework.BoundingSphere" />.
	/// </summary>
	/// <param name="sphere">The <see cref="T:Microsoft.Xna.Framework.BoundingSphere" /> to test for overlap.</param>
	/// <param name="result">
	///   A value indicating if this <see cref="T:Microsoft.Xna.Framework.BoundingBox" /> contains,
	///   intersects with or is disjoint with <paramref name="sphere" />.
	/// </param>
	public void Contains(ref BoundingSphere sphere, out ContainmentType result)
	{
		result = this.Contains(sphere);
	}

	/// <summary>
	///   Check if this <see cref="T:Microsoft.Xna.Framework.BoundingBox" /> contains a point.
	/// </summary>
	/// <param name="point">The <see cref="T:Microsoft.Xna.Framework.Vector3" /> to test.</param>
	/// <returns>
	///   <see cref="F:Microsoft.Xna.Framework.ContainmentType.Contains" /> if this <see cref="T:Microsoft.Xna.Framework.BoundingBox" /> contains
	///   <paramref name="point" /> or <see cref="F:Microsoft.Xna.Framework.ContainmentType.Disjoint" /> if it does not.
	/// </returns>
	public ContainmentType Contains(Vector3 point)
	{
		this.Contains(ref point, out var result);
		return result;
	}

	/// <summary>
	///   Check if this <see cref="T:Microsoft.Xna.Framework.BoundingBox" /> contains a point.
	/// </summary>
	/// <param name="point">The <see cref="T:Microsoft.Xna.Framework.Vector3" /> to test.</param>
	/// <param name="result">
	///   <see cref="F:Microsoft.Xna.Framework.ContainmentType.Contains" /> if this <see cref="T:Microsoft.Xna.Framework.BoundingBox" /> contains
	///   <paramref name="point" /> or <see cref="F:Microsoft.Xna.Framework.ContainmentType.Disjoint" /> if it does not.
	/// </param>
	public void Contains(ref Vector3 point, out ContainmentType result)
	{
		if (point.X < this.Min.X || point.X > this.Max.X || point.Y < this.Min.Y || point.Y > this.Max.Y || point.Z < this.Min.Z || point.Z > this.Max.Z)
		{
			result = ContainmentType.Disjoint;
		}
		else
		{
			result = ContainmentType.Contains;
		}
	}

	/// <summary>
	/// Create a bounding box from the given list of points.
	/// </summary>
	/// <param name="points">The array of Vector3 instances defining the point cloud to bound</param>
	/// <param name="index">The base index to start iterating from</param>
	/// <param name="count">The number of points to iterate</param>
	/// <returns>A bounding box that encapsulates the given point cloud.</returns>
	/// <exception cref="T:System.ArgumentException">Thrown if the given array is null or has no points.</exception>
	public static BoundingBox CreateFromPoints(Vector3[] points, int index = 0, int count = -1)
	{
		if (points == null || points.Length == 0)
		{
			throw new ArgumentException();
		}
		if (count == -1)
		{
			count = points.Length;
		}
		Vector3 minVec = BoundingBox.MaxVector3;
		Vector3 maxVec = BoundingBox.MinVector3;
		for (int i = index; i < count; i++)
		{
			minVec.X = ((minVec.X < points[i].X) ? minVec.X : points[i].X);
			minVec.Y = ((minVec.Y < points[i].Y) ? minVec.Y : points[i].Y);
			minVec.Z = ((minVec.Z < points[i].Z) ? minVec.Z : points[i].Z);
			maxVec.X = ((maxVec.X > points[i].X) ? maxVec.X : points[i].X);
			maxVec.Y = ((maxVec.Y > points[i].Y) ? maxVec.Y : points[i].Y);
			maxVec.Z = ((maxVec.Z > points[i].Z) ? maxVec.Z : points[i].Z);
		}
		return new BoundingBox(minVec, maxVec);
	}

	/// <summary>
	/// Create a bounding box from the given list of points.
	/// </summary>
	/// <param name="points">The list of Vector3 instances defining the point cloud to bound</param>
	/// <param name="index">The base index to start iterating from</param>
	/// <param name="count">The number of points to iterate</param>
	/// <returns>A bounding box that encapsulates the given point cloud.</returns>
	/// <exception cref="T:System.ArgumentException">Thrown if the given list is null or has no points.</exception>
	public static BoundingBox CreateFromPoints(List<Vector3> points, int index = 0, int count = -1)
	{
		if (points == null || points.Count == 0)
		{
			throw new ArgumentException();
		}
		if (count == -1)
		{
			count = points.Count;
		}
		Vector3 minVec = BoundingBox.MaxVector3;
		Vector3 maxVec = BoundingBox.MinVector3;
		for (int i = index; i < count; i++)
		{
			minVec.X = ((minVec.X < points[i].X) ? minVec.X : points[i].X);
			minVec.Y = ((minVec.Y < points[i].Y) ? minVec.Y : points[i].Y);
			minVec.Z = ((minVec.Z < points[i].Z) ? minVec.Z : points[i].Z);
			maxVec.X = ((maxVec.X > points[i].X) ? maxVec.X : points[i].X);
			maxVec.Y = ((maxVec.Y > points[i].Y) ? maxVec.Y : points[i].Y);
			maxVec.Z = ((maxVec.Z > points[i].Z) ? maxVec.Z : points[i].Z);
		}
		return new BoundingBox(minVec, maxVec);
	}

	/// <summary>
	///   Create the enclosing <see cref="T:Microsoft.Xna.Framework.BoundingBox" /> from the given list of points.
	/// </summary>
	/// <param name="points">The list of <see cref="T:Microsoft.Xna.Framework.Vector3" /> instances defining the point cloud to bound.</param>
	/// <returns>A <see cref="T:Microsoft.Xna.Framework.BoundingBox" /> that encloses the given point cloud.</returns>
	/// <exception cref="T:System.ArgumentException">Thrown if the given list has no points.</exception>
	public static BoundingBox CreateFromPoints(IEnumerable<Vector3> points)
	{
		if (points == null)
		{
			throw new ArgumentNullException();
		}
		bool empty = true;
		Vector3 minVec = BoundingBox.MaxVector3;
		Vector3 maxVec = BoundingBox.MinVector3;
		foreach (Vector3 ptVector in points)
		{
			minVec.X = ((minVec.X < ptVector.X) ? minVec.X : ptVector.X);
			minVec.Y = ((minVec.Y < ptVector.Y) ? minVec.Y : ptVector.Y);
			minVec.Z = ((minVec.Z < ptVector.Z) ? minVec.Z : ptVector.Z);
			maxVec.X = ((maxVec.X > ptVector.X) ? maxVec.X : ptVector.X);
			maxVec.Y = ((maxVec.Y > ptVector.Y) ? maxVec.Y : ptVector.Y);
			maxVec.Z = ((maxVec.Z > ptVector.Z) ? maxVec.Z : ptVector.Z);
			empty = false;
		}
		if (empty)
		{
			throw new ArgumentException();
		}
		return new BoundingBox(minVec, maxVec);
	}

	/// <summary>
	///   Create the enclosing <see cref="T:Microsoft.Xna.Framework.BoundingBox" /> of a <see cref="T:Microsoft.Xna.Framework.BoundingSphere" />.
	/// </summary>
	/// <param name="sphere">The <see cref="T:Microsoft.Xna.Framework.BoundingSphere" /> to enclose.</param>
	/// <returns>A <see cref="T:Microsoft.Xna.Framework.BoundingBox" /> enclosing <paramref name="sphere" />.</returns>
	public static BoundingBox CreateFromSphere(BoundingSphere sphere)
	{
		BoundingBox.CreateFromSphere(ref sphere, out var result);
		return result;
	}

	/// <summary>
	///   Create the enclosing <see cref="T:Microsoft.Xna.Framework.BoundingBox" /> of a <see cref="T:Microsoft.Xna.Framework.BoundingSphere" />.
	/// </summary>
	/// <param name="sphere">The <see cref="T:Microsoft.Xna.Framework.BoundingSphere" /> to enclose.</param>
	/// <param name="result">A <see cref="T:Microsoft.Xna.Framework.BoundingBox" /> enclosing <paramref name="sphere" />.</param>
	public static void CreateFromSphere(ref BoundingSphere sphere, out BoundingBox result)
	{
		Vector3 corner = new Vector3(sphere.Radius);
		result.Min = sphere.Center - corner;
		result.Max = sphere.Center + corner;
	}

	/// <summary>
	///   Create the <see cref="T:Microsoft.Xna.Framework.BoundingBox" /> enclosing two other <see cref="T:Microsoft.Xna.Framework.BoundingBox" /> instances.
	/// </summary>
	/// <param name="original">A <see cref="T:Microsoft.Xna.Framework.BoundingBox" /> to enclose.</param>
	/// <param name="additional">A <see cref="T:Microsoft.Xna.Framework.BoundingBox" /> to enclose.</param>
	/// <returns>
	///   The <see cref="T:Microsoft.Xna.Framework.BoundingBox" /> enclosing <paramref name="original" /> and <paramref name="additional" />.
	/// </returns>
	public static BoundingBox CreateMerged(BoundingBox original, BoundingBox additional)
	{
		BoundingBox.CreateMerged(ref original, ref additional, out var result);
		return result;
	}

	/// <summary>
	///   Create the <see cref="T:Microsoft.Xna.Framework.BoundingBox" /> enclosing two other <see cref="T:Microsoft.Xna.Framework.BoundingBox" /> instances.
	/// </summary>
	/// <param name="original">A <see cref="T:Microsoft.Xna.Framework.BoundingBox" /> to enclose.</param>
	/// <param name="additional">A <see cref="T:Microsoft.Xna.Framework.BoundingBox" /> to enclose.</param>
	/// <param name="result">
	///   The <see cref="T:Microsoft.Xna.Framework.BoundingBox" /> enclosing <paramref name="original" /> and <paramref name="additional" />.
	/// </param>
	public static void CreateMerged(ref BoundingBox original, ref BoundingBox additional, out BoundingBox result)
	{
		result.Min.X = Math.Min(original.Min.X, additional.Min.X);
		result.Min.Y = Math.Min(original.Min.Y, additional.Min.Y);
		result.Min.Z = Math.Min(original.Min.Z, additional.Min.Z);
		result.Max.X = Math.Max(original.Max.X, additional.Max.X);
		result.Max.Y = Math.Max(original.Max.Y, additional.Max.Y);
		result.Max.Z = Math.Max(original.Max.Z, additional.Max.Z);
	}

	/// <summary>
	///   Check if two <see cref="T:Microsoft.Xna.Framework.BoundingBox" /> instances are equal.
	/// </summary>
	/// <param name="other">The <see cref="T:Microsoft.Xna.Framework.BoundingBox" /> to compare with this <see cref="T:Microsoft.Xna.Framework.BoundingBox" />.</param>
	/// <returns>
	///   <code>true</code> if <see cref="!:other" /> is equal to this <see cref="T:Microsoft.Xna.Framework.BoundingBox" />,
	///   <code>false</code> if it is not.
	/// </returns>
	public bool Equals(BoundingBox other)
	{
		if (this.Min == other.Min)
		{
			return this.Max == other.Max;
		}
		return false;
	}

	/// <summary>
	///   Check if two <see cref="T:Microsoft.Xna.Framework.BoundingBox" /> instances are equal.
	/// </summary>
	/// <param name="obj">The <see cref="T:System.Object" /> to compare with this <see cref="T:Microsoft.Xna.Framework.BoundingBox" />.</param>
	/// <returns>
	///   <code>true</code> if <see cref="!:obj" /> is equal to this <see cref="T:Microsoft.Xna.Framework.BoundingBox" />,
	///   <code>false</code> if it is not.
	/// </returns>
	public override bool Equals(object obj)
	{
		if (obj is BoundingBox)
		{
			return this.Equals((BoundingBox)obj);
		}
		return false;
	}

	/// <summary>
	///   Get an array of <see cref="T:Microsoft.Xna.Framework.Vector3" /> containing the corners of this <see cref="T:Microsoft.Xna.Framework.BoundingBox" />.
	/// </summary>
	/// <returns>An array of <see cref="T:Microsoft.Xna.Framework.Vector3" /> containing the corners of this <see cref="T:Microsoft.Xna.Framework.BoundingBox" />.</returns>
	public Vector3[] GetCorners()
	{
		return new Vector3[8]
		{
			new Vector3(this.Min.X, this.Max.Y, this.Max.Z),
			new Vector3(this.Max.X, this.Max.Y, this.Max.Z),
			new Vector3(this.Max.X, this.Min.Y, this.Max.Z),
			new Vector3(this.Min.X, this.Min.Y, this.Max.Z),
			new Vector3(this.Min.X, this.Max.Y, this.Min.Z),
			new Vector3(this.Max.X, this.Max.Y, this.Min.Z),
			new Vector3(this.Max.X, this.Min.Y, this.Min.Z),
			new Vector3(this.Min.X, this.Min.Y, this.Min.Z)
		};
	}

	/// <summary>
	///   Fill the first 8 places of an array of <see cref="T:Microsoft.Xna.Framework.Vector3" />
	///   with the corners of this <see cref="T:Microsoft.Xna.Framework.BoundingBox" />.
	/// </summary>
	/// <param name="corners">The array to fill.</param>
	/// <exception cref="T:System.ArgumentNullException">If <paramref name="corners" /> is <code>null</code>.</exception>
	/// <exception cref="T:System.ArgumentOutOfRangeException">
	///   If <paramref name="corners" /> has a length of less than 8.
	/// </exception>
	public void GetCorners(Vector3[] corners)
	{
		if (corners == null)
		{
			throw new ArgumentNullException("corners");
		}
		if (corners.Length < 8)
		{
			throw new ArgumentOutOfRangeException("corners", "Not Enought Corners");
		}
		corners[0].X = this.Min.X;
		corners[0].Y = this.Max.Y;
		corners[0].Z = this.Max.Z;
		corners[1].X = this.Max.X;
		corners[1].Y = this.Max.Y;
		corners[1].Z = this.Max.Z;
		corners[2].X = this.Max.X;
		corners[2].Y = this.Min.Y;
		corners[2].Z = this.Max.Z;
		corners[3].X = this.Min.X;
		corners[3].Y = this.Min.Y;
		corners[3].Z = this.Max.Z;
		corners[4].X = this.Min.X;
		corners[4].Y = this.Max.Y;
		corners[4].Z = this.Min.Z;
		corners[5].X = this.Max.X;
		corners[5].Y = this.Max.Y;
		corners[5].Z = this.Min.Z;
		corners[6].X = this.Max.X;
		corners[6].Y = this.Min.Y;
		corners[6].Z = this.Min.Z;
		corners[7].X = this.Min.X;
		corners[7].Y = this.Min.Y;
		corners[7].Z = this.Min.Z;
	}

	/// <summary>
	///   Get the hash code for this <see cref="T:Microsoft.Xna.Framework.BoundingBox" />.
	/// </summary>
	/// <returns>A hash code for this <see cref="T:Microsoft.Xna.Framework.BoundingBox" />.</returns>
	public override int GetHashCode()
	{
		return this.Min.GetHashCode() + this.Max.GetHashCode();
	}

	/// <summary>
	///   Check if this <see cref="T:Microsoft.Xna.Framework.BoundingBox" /> intersects another <see cref="T:Microsoft.Xna.Framework.BoundingBox" />.
	/// </summary>
	/// <param name="box">The <see cref="T:Microsoft.Xna.Framework.BoundingBox" /> to test for intersection.</param>
	/// <returns>
	///   <code>true</code> if this <see cref="T:Microsoft.Xna.Framework.BoundingBox" /> intersects <paramref name="box" />,
	///   <code>false</code> if it does not.
	/// </returns>
	public bool Intersects(BoundingBox box)
	{
		this.Intersects(ref box, out var result);
		return result;
	}

	/// <summary>
	///   Check if this <see cref="T:Microsoft.Xna.Framework.BoundingBox" /> intersects another <see cref="T:Microsoft.Xna.Framework.BoundingBox" />.
	/// </summary>
	/// <param name="box">The <see cref="T:Microsoft.Xna.Framework.BoundingBox" /> to test for intersection.</param>
	/// <param name="result">
	///   <code>true</code> if this <see cref="T:Microsoft.Xna.Framework.BoundingBox" /> intersects <paramref name="box" />,
	///   <code>false</code> if it does not.
	/// </param>
	public void Intersects(ref BoundingBox box, out bool result)
	{
		if (this.Max.X >= box.Min.X && this.Min.X <= box.Max.X)
		{
			if (this.Max.Y < box.Min.Y || this.Min.Y > box.Max.Y)
			{
				result = false;
			}
			else
			{
				result = this.Max.Z >= box.Min.Z && this.Min.Z <= box.Max.Z;
			}
		}
		else
		{
			result = false;
		}
	}

	/// <summary>
	///   Check if this <see cref="T:Microsoft.Xna.Framework.BoundingBox" /> intersects a <see cref="T:Microsoft.Xna.Framework.BoundingFrustum" />.
	/// </summary>
	/// <param name="frustum">The <see cref="T:Microsoft.Xna.Framework.BoundingFrustum" /> to test for intersection.</param>
	/// <returns>
	///   <code>true</code> if this <see cref="T:Microsoft.Xna.Framework.BoundingBox" /> intersects <paramref name="frustum" />,
	///   <code>false</code> if it does not.
	/// </returns>
	public bool Intersects(BoundingFrustum frustum)
	{
		return frustum.Intersects(this);
	}

	/// <summary>
	///   Check if this <see cref="T:Microsoft.Xna.Framework.BoundingBox" /> intersects a <see cref="T:Microsoft.Xna.Framework.BoundingFrustum" />.
	/// </summary>
	/// <param name="sphere">The <see cref="T:Microsoft.Xna.Framework.BoundingFrustum" /> to test for intersection.</param>
	/// <returns>
	///   <code>true</code> if this <see cref="T:Microsoft.Xna.Framework.BoundingBox" /> intersects <paramref name="sphere" />,
	///   <code>false</code> if it does not.
	/// </returns>
	public bool Intersects(BoundingSphere sphere)
	{
		this.Intersects(ref sphere, out var result);
		return result;
	}

	/// <summary>
	///   Check if this <see cref="T:Microsoft.Xna.Framework.BoundingBox" /> intersects a <see cref="T:Microsoft.Xna.Framework.BoundingFrustum" />.
	/// </summary>
	/// <param name="sphere">The <see cref="T:Microsoft.Xna.Framework.BoundingFrustum" /> to test for intersection.</param>
	/// <param name="result">
	///   <code>true</code> if this <see cref="T:Microsoft.Xna.Framework.BoundingBox" /> intersects <paramref name="sphere" />,
	///   <code>false</code> if it does not.
	/// </param>
	public void Intersects(ref BoundingSphere sphere, out bool result)
	{
		float squareDistance = 0f;
		Vector3 point = sphere.Center;
		if (point.X < this.Min.X)
		{
			squareDistance += (this.Min.X - point.X) * (this.Min.X - point.X);
		}
		if (point.X > this.Max.X)
		{
			squareDistance += (point.X - this.Max.X) * (point.X - this.Max.X);
		}
		if (point.Y < this.Min.Y)
		{
			squareDistance += (this.Min.Y - point.Y) * (this.Min.Y - point.Y);
		}
		if (point.Y > this.Max.Y)
		{
			squareDistance += (point.Y - this.Max.Y) * (point.Y - this.Max.Y);
		}
		if (point.Z < this.Min.Z)
		{
			squareDistance += (this.Min.Z - point.Z) * (this.Min.Z - point.Z);
		}
		if (point.Z > this.Max.Z)
		{
			squareDistance += (point.Z - this.Max.Z) * (point.Z - this.Max.Z);
		}
		result = squareDistance <= sphere.Radius * sphere.Radius;
	}

	/// <summary>
	///   Check if this <see cref="T:Microsoft.Xna.Framework.BoundingBox" /> intersects a <see cref="T:Microsoft.Xna.Framework.Plane" />.
	/// </summary>
	/// <param name="plane">The <see cref="T:Microsoft.Xna.Framework.Plane" /> to test for intersection.</param>
	/// <returns>
	///   <code>true</code> if this <see cref="T:Microsoft.Xna.Framework.BoundingBox" /> intersects <paramref name="plane" />,
	///   <code>false</code> if it does not.
	/// </returns>
	public PlaneIntersectionType Intersects(Plane plane)
	{
		this.Intersects(ref plane, out var result);
		return result;
	}

	/// <summary>
	///   Check if this <see cref="T:Microsoft.Xna.Framework.BoundingBox" /> intersects a <see cref="T:Microsoft.Xna.Framework.Plane" />.
	/// </summary>
	/// <param name="plane">The <see cref="T:Microsoft.Xna.Framework.Plane" /> to test for intersection.</param>
	/// <param name="result">
	///   <code>true</code> if this <see cref="T:Microsoft.Xna.Framework.BoundingBox" /> intersects <paramref name="plane" />,
	///   <code>false</code> if it does not.
	/// </param>
	public void Intersects(ref Plane plane, out PlaneIntersectionType result)
	{
		Vector3 positiveVertex = default(Vector3);
		Vector3 negativeVertex = default(Vector3);
		if (plane.Normal.X >= 0f)
		{
			positiveVertex.X = this.Max.X;
			negativeVertex.X = this.Min.X;
		}
		else
		{
			positiveVertex.X = this.Min.X;
			negativeVertex.X = this.Max.X;
		}
		if (plane.Normal.Y >= 0f)
		{
			positiveVertex.Y = this.Max.Y;
			negativeVertex.Y = this.Min.Y;
		}
		else
		{
			positiveVertex.Y = this.Min.Y;
			negativeVertex.Y = this.Max.Y;
		}
		if (plane.Normal.Z >= 0f)
		{
			positiveVertex.Z = this.Max.Z;
			negativeVertex.Z = this.Min.Z;
		}
		else
		{
			positiveVertex.Z = this.Min.Z;
			negativeVertex.Z = this.Max.Z;
		}
		if (plane.Normal.X * negativeVertex.X + plane.Normal.Y * negativeVertex.Y + plane.Normal.Z * negativeVertex.Z + plane.D > 0f)
		{
			result = PlaneIntersectionType.Front;
		}
		else if (plane.Normal.X * positiveVertex.X + plane.Normal.Y * positiveVertex.Y + plane.Normal.Z * positiveVertex.Z + plane.D < 0f)
		{
			result = PlaneIntersectionType.Back;
		}
		else
		{
			result = PlaneIntersectionType.Intersecting;
		}
	}

	/// <summary>
	///   Check if this <see cref="T:Microsoft.Xna.Framework.BoundingBox" /> intersects a <see cref="T:Microsoft.Xna.Framework.Ray" />.
	/// </summary>
	/// <param name="ray">The <see cref="T:Microsoft.Xna.Framework.Ray" /> to test for intersection.</param>
	/// <returns>
	///   The distance along the <see cref="T:Microsoft.Xna.Framework.Ray" /> to the intersection point or
	///   <code>null</code> if the <see cref="T:Microsoft.Xna.Framework.Ray" /> does not intesect this <see cref="T:Microsoft.Xna.Framework.BoundingBox" />.
	/// </returns>
	public float? Intersects(Ray ray)
	{
		return ray.Intersects(this);
	}

	/// <summary>
	///   Check if this <see cref="T:Microsoft.Xna.Framework.BoundingBox" /> intersects a <see cref="T:Microsoft.Xna.Framework.Ray" />.
	/// </summary>
	/// <param name="ray">The <see cref="T:Microsoft.Xna.Framework.Ray" /> to test for intersection.</param>
	/// <param name="result">
	///   The distance along the <see cref="T:Microsoft.Xna.Framework.Ray" /> to the intersection point or
	///   <code>null</code> if the <see cref="T:Microsoft.Xna.Framework.Ray" /> does not intesect this <see cref="T:Microsoft.Xna.Framework.BoundingBox" />.
	/// </param>
	public void Intersects(ref Ray ray, out float? result)
	{
		result = this.Intersects(ray);
	}

	/// <summary>
	///   Check if two <see cref="T:Microsoft.Xna.Framework.BoundingBox" /> instances are equal.
	/// </summary>
	/// <param name="a">A <see cref="T:Microsoft.Xna.Framework.BoundingBox" /> to compare the other.</param>
	/// <param name="b">A <see cref="T:Microsoft.Xna.Framework.BoundingBox" /> to compare the other.</param>
	/// <returns>
	///   <code>true</code> if <see cref="!:a" /> is equal to this <see cref="!:b" />,
	///   <code>false</code> if it is not.
	/// </returns>
	public static bool operator ==(BoundingBox a, BoundingBox b)
	{
		return a.Equals(b);
	}

	/// <summary>
	///   Check if two <see cref="T:Microsoft.Xna.Framework.BoundingBox" /> instances are not equal.
	/// </summary>
	/// <param name="a">A <see cref="T:Microsoft.Xna.Framework.BoundingBox" /> to compare the other.</param>
	/// <param name="b">A <see cref="T:Microsoft.Xna.Framework.BoundingBox" /> to compare the other.</param>
	/// <returns>
	///   <code>true</code> if <see cref="!:a" /> is not equal to this <see cref="!:b" />,
	///   <code>false</code> if it is.
	/// </returns>
	public static bool operator !=(BoundingBox a, BoundingBox b)
	{
		return !a.Equals(b);
	}

	/// <summary>
	/// Get a <see cref="T:System.String" /> representation of this <see cref="T:Microsoft.Xna.Framework.BoundingBox" />.
	/// </summary>
	/// <returns>A <see cref="T:System.String" /> representation of this <see cref="T:Microsoft.Xna.Framework.BoundingBox" />.</returns>
	public override string ToString()
	{
		return "{{Min:" + this.Min.ToString() + " Max:" + this.Max.ToString() + "}}";
	}

	/// <summary>
	/// Deconstruction method for <see cref="T:Microsoft.Xna.Framework.BoundingBox" />.
	/// </summary>
	/// <param name="min"></param>
	/// <param name="max"></param>
	public void Deconstruct(out Vector3 min, out Vector3 max)
	{
		min = this.Min;
		max = this.Max;
	}
}
