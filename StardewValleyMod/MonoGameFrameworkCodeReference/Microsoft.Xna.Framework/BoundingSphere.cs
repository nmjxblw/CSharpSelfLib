using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Microsoft.Xna.Framework;

/// <summary>
/// Describes a sphere in 3D-space for bounding operations.
/// </summary>
[DataContract]
[DebuggerDisplay("{DebugDisplayString,nq}")]
public struct BoundingSphere : IEquatable<BoundingSphere>
{
	/// <summary>
	/// The sphere center.
	/// </summary>
	[DataMember]
	public Vector3 Center;

	/// <summary>
	/// The sphere radius.
	/// </summary>
	[DataMember]
	public float Radius;

	internal string DebugDisplayString => "Center( " + this.Center.DebugDisplayString + " )  \r\n" + "Radius( " + this.Radius + " )";

	/// <summary>
	/// Constructs a bounding sphere with the specified center and radius.  
	/// </summary>
	/// <param name="center">The sphere center.</param>
	/// <param name="radius">The sphere radius.</param>
	public BoundingSphere(Vector3 center, float radius)
	{
		this.Center = center;
		this.Radius = radius;
	}

	/// <summary>
	/// Test if a bounding box is fully inside, outside, or just intersecting the sphere.
	/// </summary>
	/// <param name="box">The box for testing.</param>
	/// <returns>The containment type.</returns>
	public ContainmentType Contains(BoundingBox box)
	{
		bool inside = true;
		Vector3[] corners = box.GetCorners();
		foreach (Vector3 corner in corners)
		{
			if (this.Contains(corner) == ContainmentType.Disjoint)
			{
				inside = false;
				break;
			}
		}
		if (inside)
		{
			return ContainmentType.Contains;
		}
		double dmin = 0.0;
		if (this.Center.X < box.Min.X)
		{
			dmin += (double)((this.Center.X - box.Min.X) * (this.Center.X - box.Min.X));
		}
		else if (this.Center.X > box.Max.X)
		{
			dmin += (double)((this.Center.X - box.Max.X) * (this.Center.X - box.Max.X));
		}
		if (this.Center.Y < box.Min.Y)
		{
			dmin += (double)((this.Center.Y - box.Min.Y) * (this.Center.Y - box.Min.Y));
		}
		else if (this.Center.Y > box.Max.Y)
		{
			dmin += (double)((this.Center.Y - box.Max.Y) * (this.Center.Y - box.Max.Y));
		}
		if (this.Center.Z < box.Min.Z)
		{
			dmin += (double)((this.Center.Z - box.Min.Z) * (this.Center.Z - box.Min.Z));
		}
		else if (this.Center.Z > box.Max.Z)
		{
			dmin += (double)((this.Center.Z - box.Max.Z) * (this.Center.Z - box.Max.Z));
		}
		if (dmin <= (double)(this.Radius * this.Radius))
		{
			return ContainmentType.Intersects;
		}
		return ContainmentType.Disjoint;
	}

	/// <summary>
	/// Test if a bounding box is fully inside, outside, or just intersecting the sphere.
	/// </summary>
	/// <param name="box">The box for testing.</param>
	/// <param name="result">The containment type as an output parameter.</param>
	public void Contains(ref BoundingBox box, out ContainmentType result)
	{
		result = this.Contains(box);
	}

	/// <summary>
	/// Test if a frustum is fully inside, outside, or just intersecting the sphere.
	/// </summary>
	/// <param name="frustum">The frustum for testing.</param>
	/// <returns>The containment type.</returns>
	public ContainmentType Contains(BoundingFrustum frustum)
	{
		bool inside = true;
		Vector3[] corners = frustum.GetCorners();
		foreach (Vector3 corner in corners)
		{
			if (this.Contains(corner) == ContainmentType.Disjoint)
			{
				inside = false;
				break;
			}
		}
		if (inside)
		{
			return ContainmentType.Contains;
		}
		if (0.0 <= (double)(this.Radius * this.Radius))
		{
			return ContainmentType.Intersects;
		}
		return ContainmentType.Disjoint;
	}

	/// <summary>
	/// Test if a frustum is fully inside, outside, or just intersecting the sphere.
	/// </summary>
	/// <param name="frustum">The frustum for testing.</param>
	/// <param name="result">The containment type as an output parameter.</param>
	public void Contains(ref BoundingFrustum frustum, out ContainmentType result)
	{
		result = this.Contains(frustum);
	}

	/// <summary>
	/// Test if a sphere is fully inside, outside, or just intersecting the sphere.
	/// </summary>
	/// <param name="sphere">The other sphere for testing.</param>
	/// <returns>The containment type.</returns>
	public ContainmentType Contains(BoundingSphere sphere)
	{
		this.Contains(ref sphere, out var result);
		return result;
	}

	/// <summary>
	/// Test if a sphere is fully inside, outside, or just intersecting the sphere.
	/// </summary>
	/// <param name="sphere">The other sphere for testing.</param>
	/// <param name="result">The containment type as an output parameter.</param>
	public void Contains(ref BoundingSphere sphere, out ContainmentType result)
	{
		Vector3.DistanceSquared(ref sphere.Center, ref this.Center, out var sqDistance);
		if (sqDistance > (sphere.Radius + this.Radius) * (sphere.Radius + this.Radius))
		{
			result = ContainmentType.Disjoint;
		}
		else if (sqDistance <= (this.Radius - sphere.Radius) * (this.Radius - sphere.Radius))
		{
			result = ContainmentType.Contains;
		}
		else
		{
			result = ContainmentType.Intersects;
		}
	}

	/// <summary>
	/// Test if a point is fully inside, outside, or just intersecting the sphere.
	/// </summary>
	/// <param name="point">The vector in 3D-space for testing.</param>
	/// <returns>The containment type.</returns>
	public ContainmentType Contains(Vector3 point)
	{
		this.Contains(ref point, out var result);
		return result;
	}

	/// <summary>
	/// Test if a point is fully inside, outside, or just intersecting the sphere.
	/// </summary>
	/// <param name="point">The vector in 3D-space for testing.</param>
	/// <param name="result">The containment type as an output parameter.</param>
	public void Contains(ref Vector3 point, out ContainmentType result)
	{
		float sqRadius = this.Radius * this.Radius;
		Vector3.DistanceSquared(ref point, ref this.Center, out var sqDistance);
		if (sqDistance > sqRadius)
		{
			result = ContainmentType.Disjoint;
		}
		else if (sqDistance < sqRadius)
		{
			result = ContainmentType.Contains;
		}
		else
		{
			result = ContainmentType.Intersects;
		}
	}

	/// <summary>
	/// Creates the smallest <see cref="T:Microsoft.Xna.Framework.BoundingSphere" /> that can contain a specified <see cref="T:Microsoft.Xna.Framework.BoundingBox" />.
	/// </summary>
	/// <param name="box">The box to create the sphere from.</param>
	/// <returns>The new <see cref="T:Microsoft.Xna.Framework.BoundingSphere" />.</returns>
	public static BoundingSphere CreateFromBoundingBox(BoundingBox box)
	{
		BoundingSphere.CreateFromBoundingBox(ref box, out var result);
		return result;
	}

	/// <summary>
	/// Creates the smallest <see cref="T:Microsoft.Xna.Framework.BoundingSphere" /> that can contain a specified <see cref="T:Microsoft.Xna.Framework.BoundingBox" />.
	/// </summary>
	/// <param name="box">The box to create the sphere from.</param>
	/// <param name="result">The new <see cref="T:Microsoft.Xna.Framework.BoundingSphere" /> as an output parameter.</param>
	public static void CreateFromBoundingBox(ref BoundingBox box, out BoundingSphere result)
	{
		Vector3 center = new Vector3((box.Min.X + box.Max.X) / 2f, (box.Min.Y + box.Max.Y) / 2f, (box.Min.Z + box.Max.Z) / 2f);
		float radius = Vector3.Distance(center, box.Max);
		result = new BoundingSphere(center, radius);
	}

	/// <summary>
	/// Creates the smallest <see cref="T:Microsoft.Xna.Framework.BoundingSphere" /> that can contain a specified <see cref="T:Microsoft.Xna.Framework.BoundingFrustum" />.
	/// </summary>
	/// <param name="frustum">The frustum to create the sphere from.</param>
	/// <returns>The new <see cref="T:Microsoft.Xna.Framework.BoundingSphere" />.</returns>
	public static BoundingSphere CreateFromFrustum(BoundingFrustum frustum)
	{
		return BoundingSphere.CreateFromPoints(frustum.GetCorners());
	}

	/// <summary>
	/// Creates the smallest <see cref="T:Microsoft.Xna.Framework.BoundingSphere" /> that can contain a specified list of points in 3D-space. 
	/// </summary>
	/// <param name="points">List of point to create the sphere from.</param>
	/// <returns>The new <see cref="T:Microsoft.Xna.Framework.BoundingSphere" />.</returns>
	public static BoundingSphere CreateFromPoints(IEnumerable<Vector3> points)
	{
		if (points == null)
		{
			throw new ArgumentNullException("points");
		}
		Vector3 minx = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
		Vector3 maxx = -minx;
		Vector3 miny = minx;
		Vector3 maxy = -minx;
		Vector3 minz = minx;
		Vector3 maxz = -minx;
		int numPoints = 0;
		foreach (Vector3 pt in points)
		{
			numPoints++;
			if (pt.X < minx.X)
			{
				minx = pt;
			}
			if (pt.X > maxx.X)
			{
				maxx = pt;
			}
			if (pt.Y < miny.Y)
			{
				miny = pt;
			}
			if (pt.Y > maxy.Y)
			{
				maxy = pt;
			}
			if (pt.Z < minz.Z)
			{
				minz = pt;
			}
			if (pt.Z > maxz.Z)
			{
				maxz = pt;
			}
		}
		if (numPoints == 0)
		{
			throw new ArgumentException("You should have at least one point in points.");
		}
		float sqDistX = Vector3.DistanceSquared(maxx, minx);
		float sqDistY = Vector3.DistanceSquared(maxy, miny);
		float sqDistZ = Vector3.DistanceSquared(maxz, minz);
		Vector3 min = minx;
		Vector3 max = maxx;
		if (sqDistY > sqDistX && sqDistY > sqDistZ)
		{
			max = maxy;
			min = miny;
		}
		if (sqDistZ > sqDistX && sqDistZ > sqDistY)
		{
			max = maxz;
			min = minz;
		}
		Vector3 center = (min + max) * 0.5f;
		float radius = Vector3.Distance(max, center);
		float sqRadius = radius * radius;
		foreach (Vector3 pt2 in points)
		{
			Vector3 diff = pt2 - center;
			float sqDist = diff.LengthSquared();
			if (sqDist > sqRadius)
			{
				float distance = (float)Math.Sqrt(sqDist);
				Vector3 direction = diff / distance;
				center = (center - radius * direction + pt2) / 2f;
				radius = Vector3.Distance(pt2, center);
				sqRadius = radius * radius;
			}
		}
		return new BoundingSphere(center, radius);
	}

	/// <summary>
	/// Creates the smallest <see cref="T:Microsoft.Xna.Framework.BoundingSphere" /> that can contain two spheres.
	/// </summary>
	/// <param name="original">First sphere.</param>
	/// <param name="additional">Second sphere.</param>
	/// <returns>The new <see cref="T:Microsoft.Xna.Framework.BoundingSphere" />.</returns>
	public static BoundingSphere CreateMerged(BoundingSphere original, BoundingSphere additional)
	{
		BoundingSphere.CreateMerged(ref original, ref additional, out var result);
		return result;
	}

	/// <summary>
	/// Creates the smallest <see cref="T:Microsoft.Xna.Framework.BoundingSphere" /> that can contain two spheres.
	/// </summary>
	/// <param name="original">First sphere.</param>
	/// <param name="additional">Second sphere.</param>
	/// <param name="result">The new <see cref="T:Microsoft.Xna.Framework.BoundingSphere" /> as an output parameter.</param>
	public static void CreateMerged(ref BoundingSphere original, ref BoundingSphere additional, out BoundingSphere result)
	{
		Vector3 ocenterToaCenter = Vector3.Subtract(additional.Center, original.Center);
		float distance = ocenterToaCenter.Length();
		if (distance <= original.Radius + additional.Radius)
		{
			if (distance <= original.Radius - additional.Radius)
			{
				result = original;
				return;
			}
			if (distance <= additional.Radius - original.Radius)
			{
				result = additional;
				return;
			}
		}
		float leftRadius = Math.Max(original.Radius - distance, additional.Radius);
		float Rightradius = Math.Max(original.Radius + distance, additional.Radius);
		ocenterToaCenter += (leftRadius - Rightradius) / (2f * ocenterToaCenter.Length()) * ocenterToaCenter;
		result = default(BoundingSphere);
		result.Center = original.Center + ocenterToaCenter;
		result.Radius = (leftRadius + Rightradius) / 2f;
	}

	/// <summary>
	/// Compares whether current instance is equal to specified <see cref="T:Microsoft.Xna.Framework.BoundingSphere" />.
	/// </summary>
	/// <param name="other">The <see cref="T:Microsoft.Xna.Framework.BoundingSphere" /> to compare.</param>
	/// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
	public bool Equals(BoundingSphere other)
	{
		if (this.Center == other.Center)
		{
			return this.Radius == other.Radius;
		}
		return false;
	}

	/// <summary>
	/// Compares whether current instance is equal to specified <see cref="T:System.Object" />.
	/// </summary>
	/// <param name="obj">The <see cref="T:System.Object" /> to compare.</param>
	/// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
	public override bool Equals(object obj)
	{
		if (obj is BoundingSphere)
		{
			return this.Equals((BoundingSphere)obj);
		}
		return false;
	}

	/// <summary>
	/// Gets the hash code of this <see cref="T:Microsoft.Xna.Framework.BoundingSphere" />.
	/// </summary>
	/// <returns>Hash code of this <see cref="T:Microsoft.Xna.Framework.BoundingSphere" />.</returns>
	public override int GetHashCode()
	{
		return this.Center.GetHashCode() + this.Radius.GetHashCode();
	}

	/// <summary>
	/// Gets whether or not a specified <see cref="T:Microsoft.Xna.Framework.BoundingBox" /> intersects with this sphere.
	/// </summary>
	/// <param name="box">The box for testing.</param>
	/// <returns><c>true</c> if <see cref="T:Microsoft.Xna.Framework.BoundingBox" /> intersects with this sphere; <c>false</c> otherwise.</returns>
	public bool Intersects(BoundingBox box)
	{
		return box.Intersects(this);
	}

	/// <summary>
	/// Gets whether or not a specified <see cref="T:Microsoft.Xna.Framework.BoundingBox" /> intersects with this sphere.
	/// </summary>
	/// <param name="box">The box for testing.</param>
	/// <param name="result"><c>true</c> if <see cref="T:Microsoft.Xna.Framework.BoundingBox" /> intersects with this sphere; <c>false</c> otherwise. As an output parameter.</param>
	public void Intersects(ref BoundingBox box, out bool result)
	{
		box.Intersects(ref this, out result);
	}

	/// <summary>
	/// Gets whether or not the other <see cref="T:Microsoft.Xna.Framework.BoundingSphere" /> intersects with this sphere.
	/// </summary>
	/// <param name="sphere">The other sphere for testing.</param>
	/// <returns><c>true</c> if other <see cref="T:Microsoft.Xna.Framework.BoundingSphere" /> intersects with this sphere; <c>false</c> otherwise.</returns>
	public bool Intersects(BoundingSphere sphere)
	{
		this.Intersects(ref sphere, out var result);
		return result;
	}

	/// <summary>
	/// Gets whether or not the other <see cref="T:Microsoft.Xna.Framework.BoundingSphere" /> intersects with this sphere.
	/// </summary>
	/// <param name="sphere">The other sphere for testing.</param>
	/// <param name="result"><c>true</c> if other <see cref="T:Microsoft.Xna.Framework.BoundingSphere" /> intersects with this sphere; <c>false</c> otherwise. As an output parameter.</param>
	public void Intersects(ref BoundingSphere sphere, out bool result)
	{
		Vector3.DistanceSquared(ref sphere.Center, ref this.Center, out var sqDistance);
		if (sqDistance > (sphere.Radius + this.Radius) * (sphere.Radius + this.Radius))
		{
			result = false;
		}
		else
		{
			result = true;
		}
	}

	/// <summary>
	/// Gets whether or not a specified <see cref="T:Microsoft.Xna.Framework.Plane" /> intersects with this sphere.
	/// </summary>
	/// <param name="plane">The plane for testing.</param>
	/// <returns>Type of intersection.</returns>
	public PlaneIntersectionType Intersects(Plane plane)
	{
		PlaneIntersectionType result = PlaneIntersectionType.Front;
		this.Intersects(ref plane, out result);
		return result;
	}

	/// <summary>
	/// Gets whether or not a specified <see cref="T:Microsoft.Xna.Framework.Plane" /> intersects with this sphere.
	/// </summary>
	/// <param name="plane">The plane for testing.</param>
	/// <param name="result">Type of intersection as an output parameter.</param>
	public void Intersects(ref Plane plane, out PlaneIntersectionType result)
	{
		float distance = 0f;
		Vector3.Dot(ref plane.Normal, ref this.Center, out distance);
		distance += plane.D;
		if (distance > this.Radius)
		{
			result = PlaneIntersectionType.Front;
		}
		else if (distance < 0f - this.Radius)
		{
			result = PlaneIntersectionType.Back;
		}
		else
		{
			result = PlaneIntersectionType.Intersecting;
		}
	}

	/// <summary>
	/// Gets whether or not a specified <see cref="T:Microsoft.Xna.Framework.Ray" /> intersects with this sphere.
	/// </summary>
	/// <param name="ray">The ray for testing.</param>
	/// <returns>Distance of ray intersection or <c>null</c> if there is no intersection.</returns>
	public float? Intersects(Ray ray)
	{
		return ray.Intersects(this);
	}

	/// <summary>
	/// Gets whether or not a specified <see cref="T:Microsoft.Xna.Framework.Ray" /> intersects with this sphere.
	/// </summary>
	/// <param name="ray">The ray for testing.</param>
	/// <param name="result">Distance of ray intersection or <c>null</c> if there is no intersection as an output parameter.</param>
	public void Intersects(ref Ray ray, out float? result)
	{
		ray.Intersects(ref this, out result);
	}

	/// <summary>
	/// Returns a <see cref="T:System.String" /> representation of this <see cref="T:Microsoft.Xna.Framework.BoundingSphere" /> in the format:
	/// {Center:[<see cref="F:Microsoft.Xna.Framework.BoundingSphere.Center" />] Radius:[<see cref="F:Microsoft.Xna.Framework.BoundingSphere.Radius" />]}
	/// </summary>
	/// <returns>A <see cref="T:System.String" /> representation of this <see cref="T:Microsoft.Xna.Framework.BoundingSphere" />.</returns>
	public override string ToString()
	{
		string[] obj = new string[5] { "{Center:", null, null, null, null };
		Vector3 center = this.Center;
		obj[1] = center.ToString();
		obj[2] = " Radius:";
		obj[3] = this.Radius.ToString();
		obj[4] = "}";
		return string.Concat(obj);
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.BoundingSphere" /> that contains a transformation of translation and scale from this sphere by the specified <see cref="T:Microsoft.Xna.Framework.Matrix" />.
	/// </summary>
	/// <param name="matrix">The transformation <see cref="T:Microsoft.Xna.Framework.Matrix" />.</param>
	/// <returns>Transformed <see cref="T:Microsoft.Xna.Framework.BoundingSphere" />.</returns>
	public BoundingSphere Transform(Matrix matrix)
	{
		return new BoundingSphere
		{
			Center = Vector3.Transform(this.Center, matrix),
			Radius = this.Radius * (float)Math.Sqrt(Math.Max(matrix.M11 * matrix.M11 + matrix.M12 * matrix.M12 + matrix.M13 * matrix.M13, Math.Max(matrix.M21 * matrix.M21 + matrix.M22 * matrix.M22 + matrix.M23 * matrix.M23, matrix.M31 * matrix.M31 + matrix.M32 * matrix.M32 + matrix.M33 * matrix.M33)))
		};
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.BoundingSphere" /> that contains a transformation of translation and scale from this sphere by the specified <see cref="T:Microsoft.Xna.Framework.Matrix" />.
	/// </summary>
	/// <param name="matrix">The transformation <see cref="T:Microsoft.Xna.Framework.Matrix" />.</param>
	/// <param name="result">Transformed <see cref="T:Microsoft.Xna.Framework.BoundingSphere" /> as an output parameter.</param>
	public void Transform(ref Matrix matrix, out BoundingSphere result)
	{
		result.Center = Vector3.Transform(this.Center, matrix);
		result.Radius = this.Radius * (float)Math.Sqrt(Math.Max(matrix.M11 * matrix.M11 + matrix.M12 * matrix.M12 + matrix.M13 * matrix.M13, Math.Max(matrix.M21 * matrix.M21 + matrix.M22 * matrix.M22 + matrix.M23 * matrix.M23, matrix.M31 * matrix.M31 + matrix.M32 * matrix.M32 + matrix.M33 * matrix.M33)));
	}

	/// <summary>
	/// Deconstruction method for <see cref="T:Microsoft.Xna.Framework.BoundingSphere" />.
	/// </summary>
	/// <param name="center"></param>
	/// <param name="radius"></param>
	public void Deconstruct(out Vector3 center, out float radius)
	{
		center = this.Center;
		radius = this.Radius;
	}

	/// <summary>
	/// Compares whether two <see cref="T:Microsoft.Xna.Framework.BoundingSphere" /> instances are equal.
	/// </summary>
	/// <param name="a"><see cref="T:Microsoft.Xna.Framework.BoundingSphere" /> instance on the left of the equal sign.</param>
	/// <param name="b"><see cref="T:Microsoft.Xna.Framework.BoundingSphere" /> instance on the right of the equal sign.</param>
	/// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
	public static bool operator ==(BoundingSphere a, BoundingSphere b)
	{
		return a.Equals(b);
	}

	/// <summary>
	/// Compares whether two <see cref="T:Microsoft.Xna.Framework.BoundingSphere" /> instances are not equal.
	/// </summary>
	/// <param name="a"><see cref="T:Microsoft.Xna.Framework.BoundingSphere" /> instance on the left of the not equal sign.</param>
	/// <param name="b"><see cref="T:Microsoft.Xna.Framework.BoundingSphere" /> instance on the right of the not equal sign.</param>
	/// <returns><c>true</c> if the instances are not equal; <c>false</c> otherwise.</returns>
	public static bool operator !=(BoundingSphere a, BoundingSphere b)
	{
		return !a.Equals(b);
	}
}
