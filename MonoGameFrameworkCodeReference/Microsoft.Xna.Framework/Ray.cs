using System;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Microsoft.Xna.Framework;

/// <summary>
/// Represents a ray with an origin and a direction in 3D space.
/// </summary>
[DataContract]
[DebuggerDisplay("{DebugDisplayString,nq}")]
public struct Ray : IEquatable<Ray>
{
	/// <summary>
	/// The direction of this <see cref="T:Microsoft.Xna.Framework.Ray" />.
	/// </summary>
	[DataMember]
	public Vector3 Direction;

	/// <summary>
	/// The origin of this <see cref="T:Microsoft.Xna.Framework.Ray" />.
	/// </summary>
	[DataMember]
	public Vector3 Position;

	internal string DebugDisplayString => "Pos( " + this.Position.DebugDisplayString + " )  \r\n" + "Dir( " + this.Direction.DebugDisplayString + " )";

	/// <summary>
	/// Create a <see cref="T:Microsoft.Xna.Framework.Ray" />.
	/// </summary>
	/// <param name="position">The origin of the <see cref="T:Microsoft.Xna.Framework.Ray" />.</param>
	/// <param name="direction">The direction of the <see cref="T:Microsoft.Xna.Framework.Ray" />.</param>
	public Ray(Vector3 position, Vector3 direction)
	{
		this.Position = position;
		this.Direction = direction;
	}

	/// <summary>
	/// Check if the specified <see cref="T:System.Object" /> is equal to this <see cref="T:Microsoft.Xna.Framework.Ray" />.
	/// </summary>
	/// <param name="obj">The <see cref="T:System.Object" /> to test for equality with this <see cref="T:Microsoft.Xna.Framework.Ray" />.</param>
	/// <returns>
	/// <code>true</code> if the specified <see cref="T:System.Object" /> is equal to this <see cref="T:Microsoft.Xna.Framework.Ray" />,
	/// <code>false</code> if it is not.
	/// </returns>
	public override bool Equals(object obj)
	{
		if (obj is Ray)
		{
			return this.Equals((Ray)obj);
		}
		return false;
	}

	/// <summary>
	/// Check if the specified <see cref="T:Microsoft.Xna.Framework.Ray" /> is equal to this <see cref="T:Microsoft.Xna.Framework.Ray" />.
	/// </summary>
	/// <param name="other">The <see cref="T:Microsoft.Xna.Framework.Ray" /> to test for equality with this <see cref="T:Microsoft.Xna.Framework.Ray" />.</param>
	/// <returns>
	/// <code>true</code> if the specified <see cref="T:Microsoft.Xna.Framework.Ray" /> is equal to this <see cref="T:Microsoft.Xna.Framework.Ray" />,
	/// <code>false</code> if it is not.
	/// </returns>
	public bool Equals(Ray other)
	{
		if (this.Position.Equals(other.Position))
		{
			return this.Direction.Equals(other.Direction);
		}
		return false;
	}

	/// <summary>
	/// Get a hash code for this <see cref="T:Microsoft.Xna.Framework.Ray" />.
	/// </summary>
	/// <returns>A hash code for this <see cref="T:Microsoft.Xna.Framework.Ray" />.</returns>
	public override int GetHashCode()
	{
		return this.Position.GetHashCode() ^ this.Direction.GetHashCode();
	}

	/// <summary>
	/// Check if this <see cref="T:Microsoft.Xna.Framework.Ray" /> intersects the specified <see cref="T:Microsoft.Xna.Framework.BoundingBox" />.
	/// </summary>
	/// <param name="box">The <see cref="T:Microsoft.Xna.Framework.BoundingBox" /> to test for intersection.</param>
	/// <returns>
	/// The distance along the ray of the intersection or <code>null</code> if this
	/// <see cref="T:Microsoft.Xna.Framework.Ray" /> does not intersect the <see cref="T:Microsoft.Xna.Framework.BoundingBox" />.
	/// </returns>
	public float? Intersects(BoundingBox box)
	{
		float? tMin = null;
		float? tMax = null;
		if (Math.Abs(this.Direction.X) < 1E-06f)
		{
			if (this.Position.X < box.Min.X || this.Position.X > box.Max.X)
			{
				return null;
			}
		}
		else
		{
			tMin = (box.Min.X - this.Position.X) / this.Direction.X;
			tMax = (box.Max.X - this.Position.X) / this.Direction.X;
			if (tMin > tMax)
			{
				float? num = tMin;
				tMin = tMax;
				tMax = num;
			}
		}
		if (Math.Abs(this.Direction.Y) < 1E-06f)
		{
			if (this.Position.Y < box.Min.Y || this.Position.Y > box.Max.Y)
			{
				return null;
			}
		}
		else
		{
			float tMinY = (box.Min.Y - this.Position.Y) / this.Direction.Y;
			float tMaxY = (box.Max.Y - this.Position.Y) / this.Direction.Y;
			if (tMinY > tMaxY)
			{
				float num2 = tMinY;
				tMinY = tMaxY;
				tMaxY = num2;
			}
			if ((tMin.HasValue && tMin > tMaxY) || (tMax.HasValue && tMinY > tMax))
			{
				return null;
			}
			if (!tMin.HasValue || tMinY > tMin)
			{
				tMin = tMinY;
			}
			if (!tMax.HasValue || tMaxY < tMax)
			{
				tMax = tMaxY;
			}
		}
		if (Math.Abs(this.Direction.Z) < 1E-06f)
		{
			if (this.Position.Z < box.Min.Z || this.Position.Z > box.Max.Z)
			{
				return null;
			}
		}
		else
		{
			float tMinZ = (box.Min.Z - this.Position.Z) / this.Direction.Z;
			float tMaxZ = (box.Max.Z - this.Position.Z) / this.Direction.Z;
			if (tMinZ > tMaxZ)
			{
				float num3 = tMinZ;
				tMinZ = tMaxZ;
				tMaxZ = num3;
			}
			if ((tMin.HasValue && tMin > tMaxZ) || (tMax.HasValue && tMinZ > tMax))
			{
				return null;
			}
			if (!tMin.HasValue || tMinZ > tMin)
			{
				tMin = tMinZ;
			}
			if (!tMax.HasValue || tMaxZ < tMax)
			{
				tMax = tMaxZ;
			}
		}
		if (tMin.HasValue && tMin < 0f && tMax > 0f)
		{
			return 0f;
		}
		if (tMin < 0f)
		{
			return null;
		}
		return tMin;
	}

	/// <summary>
	/// Check if this <see cref="T:Microsoft.Xna.Framework.Ray" /> intersects the specified <see cref="T:Microsoft.Xna.Framework.BoundingBox" />.
	/// </summary>
	/// <param name="box">The <see cref="T:Microsoft.Xna.Framework.BoundingBox" /> to test for intersection.</param>
	/// <param name="result">
	/// The distance along the ray of the intersection or <code>null</code> if this
	/// <see cref="T:Microsoft.Xna.Framework.Ray" /> does not intersect the <see cref="T:Microsoft.Xna.Framework.BoundingBox" />.
	/// </param>
	public void Intersects(ref BoundingBox box, out float? result)
	{
		result = this.Intersects(box);
	}

	/// <summary>
	/// Check if this <see cref="T:Microsoft.Xna.Framework.Ray" /> intersects the specified <see cref="T:Microsoft.Xna.Framework.BoundingSphere" />.
	/// </summary>
	/// <param name="sphere">The <see cref="T:Microsoft.Xna.Framework.BoundingBox" /> to test for intersection.</param>
	/// <returns>
	/// The distance along the ray of the intersection or <code>null</code> if this
	/// <see cref="T:Microsoft.Xna.Framework.Ray" /> does not intersect the <see cref="T:Microsoft.Xna.Framework.BoundingSphere" />.
	/// </returns>
	public float? Intersects(BoundingSphere sphere)
	{
		this.Intersects(ref sphere, out var result);
		return result;
	}

	/// <summary>
	/// Check if this <see cref="T:Microsoft.Xna.Framework.Ray" /> intersects the specified <see cref="T:Microsoft.Xna.Framework.Plane" />.
	/// </summary>
	/// <param name="plane">The <see cref="T:Microsoft.Xna.Framework.Plane" /> to test for intersection.</param>
	/// <returns>
	/// The distance along the ray of the intersection or <code>null</code> if this
	/// <see cref="T:Microsoft.Xna.Framework.Ray" /> does not intersect the <see cref="T:Microsoft.Xna.Framework.Plane" />.
	/// </returns>
	public float? Intersects(Plane plane)
	{
		this.Intersects(ref plane, out var result);
		return result;
	}

	/// <summary>
	/// Check if this <see cref="T:Microsoft.Xna.Framework.Ray" /> intersects the specified <see cref="T:Microsoft.Xna.Framework.Plane" />.
	/// </summary>
	/// <param name="plane">The <see cref="T:Microsoft.Xna.Framework.Plane" /> to test for intersection.</param>
	/// <param name="result">
	/// The distance along the ray of the intersection or <code>null</code> if this
	/// <see cref="T:Microsoft.Xna.Framework.Ray" /> does not intersect the <see cref="T:Microsoft.Xna.Framework.Plane" />.
	/// </param>
	public void Intersects(ref Plane plane, out float? result)
	{
		float den = Vector3.Dot(this.Direction, plane.Normal);
		if (Math.Abs(den) < 1E-05f)
		{
			result = null;
			return;
		}
		result = (0f - plane.D - Vector3.Dot(plane.Normal, this.Position)) / den;
		if (result < 0f)
		{
			if (result < -1E-05f)
			{
				result = null;
			}
			else
			{
				result = 0f;
			}
		}
	}

	/// <summary>
	/// Check if this <see cref="T:Microsoft.Xna.Framework.Ray" /> intersects the specified <see cref="T:Microsoft.Xna.Framework.BoundingSphere" />.
	/// </summary>
	/// <param name="sphere">The <see cref="T:Microsoft.Xna.Framework.BoundingBox" /> to test for intersection.</param>
	/// <param name="result">
	/// The distance along the ray of the intersection or <code>null</code> if this
	/// <see cref="T:Microsoft.Xna.Framework.Ray" /> does not intersect the <see cref="T:Microsoft.Xna.Framework.BoundingSphere" />.
	/// </param>
	public void Intersects(ref BoundingSphere sphere, out float? result)
	{
		Vector3 difference = sphere.Center - this.Position;
		float differenceLengthSquared = difference.LengthSquared();
		float sphereRadiusSquared = sphere.Radius * sphere.Radius;
		if (differenceLengthSquared < sphereRadiusSquared)
		{
			result = 0f;
			return;
		}
		Vector3.Dot(ref this.Direction, ref difference, out var distanceAlongRay);
		if (distanceAlongRay < 0f)
		{
			result = null;
			return;
		}
		float dist = sphereRadiusSquared + distanceAlongRay * distanceAlongRay - differenceLengthSquared;
		result = ((dist < 0f) ? ((float?)null) : new float?(distanceAlongRay - (float)Math.Sqrt(dist)));
	}

	/// <summary>
	/// Check if two rays are not equal.
	/// </summary>
	/// <param name="a">A ray to check for inequality.</param>
	/// <param name="b">A ray to check for inequality.</param>
	/// <returns><code>true</code> if the two rays are not equal, <code>false</code> if they are.</returns>
	public static bool operator !=(Ray a, Ray b)
	{
		return !a.Equals(b);
	}

	/// <summary>
	/// Check if two rays are equal.
	/// </summary>
	/// <param name="a">A ray to check for equality.</param>
	/// <param name="b">A ray to check for equality.</param>
	/// <returns><code>true</code> if the two rays are equals, <code>false</code> if they are not.</returns>
	public static bool operator ==(Ray a, Ray b)
	{
		return a.Equals(b);
	}

	/// <summary>
	/// Get a <see cref="T:System.String" /> representation of this <see cref="T:Microsoft.Xna.Framework.Ray" />.
	/// </summary>
	/// <returns>A <see cref="T:System.String" /> representation of this <see cref="T:Microsoft.Xna.Framework.Ray" />.</returns>
	public override string ToString()
	{
		return "{{Position:" + this.Position.ToString() + " Direction:" + this.Direction.ToString() + "}}";
	}

	/// <summary>
	/// Deconstruction method for <see cref="T:Microsoft.Xna.Framework.Ray" />.
	/// </summary>
	/// <param name="position">Receives the start position of the ray.</param>
	/// <param name="direction">Receives the direction of the ray.</param>
	public void Deconstruct(out Vector3 position, out Vector3 direction)
	{
		position = this.Position;
		direction = this.Direction;
	}
}
