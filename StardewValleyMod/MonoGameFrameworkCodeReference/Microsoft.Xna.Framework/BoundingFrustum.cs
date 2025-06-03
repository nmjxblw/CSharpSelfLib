using System;
using System.Diagnostics;

namespace Microsoft.Xna.Framework;

/// <summary>
/// Defines a viewing frustum for intersection operations.
/// </summary>
[DebuggerDisplay("{DebugDisplayString,nq}")]
public class BoundingFrustum : IEquatable<BoundingFrustum>
{
	private Matrix _matrix;

	private readonly Vector3[] _corners = new Vector3[8];

	private readonly Plane[] _planes = new Plane[6];

	/// <summary>
	/// The number of planes in the frustum.
	/// </summary>
	public const int PlaneCount = 6;

	/// <summary>
	/// The number of corner points in the frustum.
	/// </summary>
	public const int CornerCount = 8;

	/// <summary>
	/// Gets or sets the <see cref="P:Microsoft.Xna.Framework.BoundingFrustum.Matrix" /> of the frustum.
	/// </summary>
	public Matrix Matrix
	{
		get
		{
			return this._matrix;
		}
		set
		{
			this._matrix = value;
			this.CreatePlanes();
			this.CreateCorners();
		}
	}

	/// <summary>
	/// Gets the near plane of the frustum.
	/// </summary>
	public Plane Near => this._planes[0];

	/// <summary>
	/// Gets the far plane of the frustum.
	/// </summary>
	public Plane Far => this._planes[1];

	/// <summary>
	/// Gets the left plane of the frustum.
	/// </summary>
	public Plane Left => this._planes[2];

	/// <summary>
	/// Gets the right plane of the frustum.
	/// </summary>
	public Plane Right => this._planes[3];

	/// <summary>
	/// Gets the top plane of the frustum.
	/// </summary>
	public Plane Top => this._planes[4];

	/// <summary>
	/// Gets the bottom plane of the frustum.
	/// </summary>
	public Plane Bottom => this._planes[5];

	internal string DebugDisplayString => "Near( " + this._planes[0].DebugDisplayString + " )  \r\n" + "Far( " + this._planes[1].DebugDisplayString + " )  \r\n" + "Left( " + this._planes[2].DebugDisplayString + " )  \r\n" + "Right( " + this._planes[3].DebugDisplayString + " )  \r\n" + "Top( " + this._planes[4].DebugDisplayString + " )  \r\n" + "Bottom( " + this._planes[5].DebugDisplayString + " )  ";

	/// <summary>
	/// Constructs the frustum by extracting the view planes from a matrix.
	/// </summary>
	/// <param name="value">Combined matrix which usually is (View * Projection).</param>
	public BoundingFrustum(Matrix value)
	{
		this._matrix = value;
		this.CreatePlanes();
		this.CreateCorners();
	}

	/// <summary>
	/// Compares whether two <see cref="T:Microsoft.Xna.Framework.BoundingFrustum" /> instances are equal.
	/// </summary>
	/// <param name="a"><see cref="T:Microsoft.Xna.Framework.BoundingFrustum" /> instance on the left of the equal sign.</param>
	/// <param name="b"><see cref="T:Microsoft.Xna.Framework.BoundingFrustum" /> instance on the right of the equal sign.</param>
	/// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
	public static bool operator ==(BoundingFrustum a, BoundingFrustum b)
	{
		if (object.Equals(a, null))
		{
			return object.Equals(b, null);
		}
		if (object.Equals(b, null))
		{
			return object.Equals(a, null);
		}
		return a._matrix == b._matrix;
	}

	/// <summary>
	/// Compares whether two <see cref="T:Microsoft.Xna.Framework.BoundingFrustum" /> instances are not equal.
	/// </summary>
	/// <param name="a"><see cref="T:Microsoft.Xna.Framework.BoundingFrustum" /> instance on the left of the not equal sign.</param>
	/// <param name="b"><see cref="T:Microsoft.Xna.Framework.BoundingFrustum" /> instance on the right of the not equal sign.</param>
	/// <returns><c>true</c> if the instances are not equal; <c>false</c> otherwise.</returns>
	public static bool operator !=(BoundingFrustum a, BoundingFrustum b)
	{
		return !(a == b);
	}

	/// <summary>
	/// Containment test between this <see cref="T:Microsoft.Xna.Framework.BoundingFrustum" /> and specified <see cref="T:Microsoft.Xna.Framework.BoundingBox" />.
	/// </summary>
	/// <param name="box">A <see cref="T:Microsoft.Xna.Framework.BoundingBox" /> for testing.</param>
	/// <returns>Result of testing for containment between this <see cref="T:Microsoft.Xna.Framework.BoundingFrustum" /> and specified <see cref="T:Microsoft.Xna.Framework.BoundingBox" />.</returns>
	public ContainmentType Contains(BoundingBox box)
	{
		ContainmentType result = ContainmentType.Disjoint;
		this.Contains(ref box, out result);
		return result;
	}

	/// <summary>
	/// Containment test between this <see cref="T:Microsoft.Xna.Framework.BoundingFrustum" /> and specified <see cref="T:Microsoft.Xna.Framework.BoundingBox" />.
	/// </summary>
	/// <param name="box">A <see cref="T:Microsoft.Xna.Framework.BoundingBox" /> for testing.</param>
	/// <param name="result">Result of testing for containment between this <see cref="T:Microsoft.Xna.Framework.BoundingFrustum" /> and specified <see cref="T:Microsoft.Xna.Framework.BoundingBox" /> as an output parameter.</param>
	public void Contains(ref BoundingBox box, out ContainmentType result)
	{
		bool intersects = false;
		for (int i = 0; i < 6; i++)
		{
			PlaneIntersectionType planeIntersectionType = PlaneIntersectionType.Front;
			box.Intersects(ref this._planes[i], out planeIntersectionType);
			switch (planeIntersectionType)
			{
			case PlaneIntersectionType.Front:
				result = ContainmentType.Disjoint;
				return;
			case PlaneIntersectionType.Intersecting:
				intersects = true;
				break;
			}
		}
		result = ((!intersects) ? ContainmentType.Contains : ContainmentType.Intersects);
	}

	/// <summary>
	/// Containment test between this <see cref="T:Microsoft.Xna.Framework.BoundingFrustum" /> and specified <see cref="T:Microsoft.Xna.Framework.BoundingFrustum" />.
	/// </summary>
	/// <param name="frustum">A <see cref="T:Microsoft.Xna.Framework.BoundingFrustum" /> for testing.</param>
	/// <returns>Result of testing for containment between this <see cref="T:Microsoft.Xna.Framework.BoundingFrustum" /> and specified <see cref="T:Microsoft.Xna.Framework.BoundingFrustum" />.</returns>
	public ContainmentType Contains(BoundingFrustum frustum)
	{
		if (this == frustum)
		{
			return ContainmentType.Contains;
		}
		bool intersects = false;
		for (int i = 0; i < 6; i++)
		{
			frustum.Intersects(ref this._planes[i], out var planeIntersectionType);
			switch (planeIntersectionType)
			{
			case PlaneIntersectionType.Front:
				return ContainmentType.Disjoint;
			case PlaneIntersectionType.Intersecting:
				intersects = true;
				break;
			}
		}
		if (!intersects)
		{
			return ContainmentType.Contains;
		}
		return ContainmentType.Intersects;
	}

	/// <summary>
	/// Containment test between this <see cref="T:Microsoft.Xna.Framework.BoundingFrustum" /> and specified <see cref="T:Microsoft.Xna.Framework.BoundingSphere" />.
	/// </summary>
	/// <param name="sphere">A <see cref="T:Microsoft.Xna.Framework.BoundingSphere" /> for testing.</param>
	/// <returns>Result of testing for containment between this <see cref="T:Microsoft.Xna.Framework.BoundingFrustum" /> and specified <see cref="T:Microsoft.Xna.Framework.BoundingSphere" />.</returns>
	public ContainmentType Contains(BoundingSphere sphere)
	{
		ContainmentType result = ContainmentType.Disjoint;
		this.Contains(ref sphere, out result);
		return result;
	}

	/// <summary>
	/// Containment test between this <see cref="T:Microsoft.Xna.Framework.BoundingFrustum" /> and specified <see cref="T:Microsoft.Xna.Framework.BoundingSphere" />.
	/// </summary>
	/// <param name="sphere">A <see cref="T:Microsoft.Xna.Framework.BoundingSphere" /> for testing.</param>
	/// <param name="result">Result of testing for containment between this <see cref="T:Microsoft.Xna.Framework.BoundingFrustum" /> and specified <see cref="T:Microsoft.Xna.Framework.BoundingSphere" /> as an output parameter.</param>
	public void Contains(ref BoundingSphere sphere, out ContainmentType result)
	{
		bool intersects = false;
		for (int i = 0; i < 6; i++)
		{
			PlaneIntersectionType planeIntersectionType = PlaneIntersectionType.Front;
			sphere.Intersects(ref this._planes[i], out planeIntersectionType);
			switch (planeIntersectionType)
			{
			case PlaneIntersectionType.Front:
				result = ContainmentType.Disjoint;
				return;
			case PlaneIntersectionType.Intersecting:
				intersects = true;
				break;
			}
		}
		result = ((!intersects) ? ContainmentType.Contains : ContainmentType.Intersects);
	}

	/// <summary>
	/// Containment test between this <see cref="T:Microsoft.Xna.Framework.BoundingFrustum" /> and specified <see cref="T:Microsoft.Xna.Framework.Vector3" />.
	/// </summary>
	/// <param name="point">A <see cref="T:Microsoft.Xna.Framework.Vector3" /> for testing.</param>
	/// <returns>Result of testing for containment between this <see cref="T:Microsoft.Xna.Framework.BoundingFrustum" /> and specified <see cref="T:Microsoft.Xna.Framework.Vector3" />.</returns>
	public ContainmentType Contains(Vector3 point)
	{
		ContainmentType result = ContainmentType.Disjoint;
		this.Contains(ref point, out result);
		return result;
	}

	/// <summary>
	/// Containment test between this <see cref="T:Microsoft.Xna.Framework.BoundingFrustum" /> and specified <see cref="T:Microsoft.Xna.Framework.Vector3" />.
	/// </summary>
	/// <param name="point">A <see cref="T:Microsoft.Xna.Framework.Vector3" /> for testing.</param>
	/// <param name="result">Result of testing for containment between this <see cref="T:Microsoft.Xna.Framework.BoundingFrustum" /> and specified <see cref="T:Microsoft.Xna.Framework.Vector3" /> as an output parameter.</param>
	public void Contains(ref Vector3 point, out ContainmentType result)
	{
		for (int i = 0; i < 6; i++)
		{
			if (PlaneHelper.ClassifyPoint(ref point, ref this._planes[i]) > 0f)
			{
				result = ContainmentType.Disjoint;
				return;
			}
		}
		result = ContainmentType.Contains;
	}

	/// <summary>
	/// Compares whether current instance is equal to specified <see cref="T:Microsoft.Xna.Framework.BoundingFrustum" />.
	/// </summary>
	/// <param name="other">The <see cref="T:Microsoft.Xna.Framework.BoundingFrustum" /> to compare.</param>
	/// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
	public bool Equals(BoundingFrustum other)
	{
		return this == other;
	}

	/// <summary>
	/// Compares whether current instance is equal to specified <see cref="T:Microsoft.Xna.Framework.BoundingFrustum" />.
	/// </summary>
	/// <param name="obj">The <see cref="T:System.Object" /> to compare.</param>
	/// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
	public override bool Equals(object obj)
	{
		if (obj is BoundingFrustum)
		{
			return this == (BoundingFrustum)obj;
		}
		return false;
	}

	/// <summary>
	/// Returns a copy of internal corners array.
	/// </summary>
	/// <returns>The array of corners.</returns>
	public Vector3[] GetCorners()
	{
		return (Vector3[])this._corners.Clone();
	}

	/// <summary>
	/// Returns a copy of internal corners array.
	/// </summary>
	/// <param name="corners">The array which values will be replaced to corner values of this instance. It must have size of <see cref="F:Microsoft.Xna.Framework.BoundingFrustum.CornerCount" />.</param>
	public void GetCorners(Vector3[] corners)
	{
		if (corners == null)
		{
			throw new ArgumentNullException("corners");
		}
		if (corners.Length < 8)
		{
			throw new ArgumentOutOfRangeException("corners");
		}
		this._corners.CopyTo(corners, 0);
	}

	/// <summary>
	/// Gets the hash code of this <see cref="T:Microsoft.Xna.Framework.BoundingFrustum" />.
	/// </summary>
	/// <returns>Hash code of this <see cref="T:Microsoft.Xna.Framework.BoundingFrustum" />.</returns>
	public override int GetHashCode()
	{
		return this._matrix.GetHashCode();
	}

	/// <summary>
	/// Gets whether or not a specified <see cref="T:Microsoft.Xna.Framework.BoundingBox" /> intersects with this <see cref="T:Microsoft.Xna.Framework.BoundingFrustum" />.
	/// </summary>
	/// <param name="box">A <see cref="T:Microsoft.Xna.Framework.BoundingBox" /> for intersection test.</param>
	/// <returns><c>true</c> if specified <see cref="T:Microsoft.Xna.Framework.BoundingBox" /> intersects with this <see cref="T:Microsoft.Xna.Framework.BoundingFrustum" />; <c>false</c> otherwise.</returns>
	public bool Intersects(BoundingBox box)
	{
		bool result = false;
		this.Intersects(ref box, out result);
		return result;
	}

	/// <summary>
	/// Gets whether or not a specified <see cref="T:Microsoft.Xna.Framework.BoundingBox" /> intersects with this <see cref="T:Microsoft.Xna.Framework.BoundingFrustum" />.
	/// </summary>
	/// <param name="box">A <see cref="T:Microsoft.Xna.Framework.BoundingBox" /> for intersection test.</param>
	/// <param name="result"><c>true</c> if specified <see cref="T:Microsoft.Xna.Framework.BoundingBox" /> intersects with this <see cref="T:Microsoft.Xna.Framework.BoundingFrustum" />; <c>false</c> otherwise as an output parameter.</param>
	public void Intersects(ref BoundingBox box, out bool result)
	{
		ContainmentType containment = ContainmentType.Disjoint;
		this.Contains(ref box, out containment);
		result = containment != ContainmentType.Disjoint;
	}

	/// <summary>
	/// Gets whether or not a specified <see cref="T:Microsoft.Xna.Framework.BoundingFrustum" /> intersects with this <see cref="T:Microsoft.Xna.Framework.BoundingFrustum" />.
	/// </summary>
	/// <param name="frustum">An other <see cref="T:Microsoft.Xna.Framework.BoundingFrustum" /> for intersection test.</param>
	/// <returns><c>true</c> if other <see cref="T:Microsoft.Xna.Framework.BoundingFrustum" /> intersects with this <see cref="T:Microsoft.Xna.Framework.BoundingFrustum" />; <c>false</c> otherwise.</returns>
	public bool Intersects(BoundingFrustum frustum)
	{
		return this.Contains(frustum) != ContainmentType.Disjoint;
	}

	/// <summary>
	/// Gets whether or not a specified <see cref="T:Microsoft.Xna.Framework.BoundingSphere" /> intersects with this <see cref="T:Microsoft.Xna.Framework.BoundingFrustum" />.
	/// </summary>
	/// <param name="sphere">A <see cref="T:Microsoft.Xna.Framework.BoundingSphere" /> for intersection test.</param>
	/// <returns><c>true</c> if specified <see cref="T:Microsoft.Xna.Framework.BoundingSphere" /> intersects with this <see cref="T:Microsoft.Xna.Framework.BoundingFrustum" />; <c>false</c> otherwise.</returns>
	public bool Intersects(BoundingSphere sphere)
	{
		bool result = false;
		this.Intersects(ref sphere, out result);
		return result;
	}

	/// <summary>
	/// Gets whether or not a specified <see cref="T:Microsoft.Xna.Framework.BoundingSphere" /> intersects with this <see cref="T:Microsoft.Xna.Framework.BoundingFrustum" />.
	/// </summary>
	/// <param name="sphere">A <see cref="T:Microsoft.Xna.Framework.BoundingSphere" /> for intersection test.</param>
	/// <param name="result"><c>true</c> if specified <see cref="T:Microsoft.Xna.Framework.BoundingSphere" /> intersects with this <see cref="T:Microsoft.Xna.Framework.BoundingFrustum" />; <c>false</c> otherwise as an output parameter.</param>
	public void Intersects(ref BoundingSphere sphere, out bool result)
	{
		ContainmentType containment = ContainmentType.Disjoint;
		this.Contains(ref sphere, out containment);
		result = containment != ContainmentType.Disjoint;
	}

	/// <summary>
	/// Gets type of intersection between specified <see cref="T:Microsoft.Xna.Framework.Plane" /> and this <see cref="T:Microsoft.Xna.Framework.BoundingFrustum" />.
	/// </summary>
	/// <param name="plane">A <see cref="T:Microsoft.Xna.Framework.Plane" /> for intersection test.</param>
	/// <returns>A plane intersection type.</returns>
	public PlaneIntersectionType Intersects(Plane plane)
	{
		this.Intersects(ref plane, out var result);
		return result;
	}

	/// <summary>
	/// Gets type of intersection between specified <see cref="T:Microsoft.Xna.Framework.Plane" /> and this <see cref="T:Microsoft.Xna.Framework.BoundingFrustum" />.
	/// </summary>
	/// <param name="plane">A <see cref="T:Microsoft.Xna.Framework.Plane" /> for intersection test.</param>
	/// <param name="result">A plane intersection type as an output parameter.</param>
	public void Intersects(ref Plane plane, out PlaneIntersectionType result)
	{
		result = plane.Intersects(ref this._corners[0]);
		for (int i = 1; i < this._corners.Length; i++)
		{
			if (plane.Intersects(ref this._corners[i]) != result)
			{
				result = PlaneIntersectionType.Intersecting;
			}
		}
	}

	/// <summary>
	/// Gets the distance of intersection of <see cref="T:Microsoft.Xna.Framework.Ray" /> and this <see cref="T:Microsoft.Xna.Framework.BoundingFrustum" /> or null if no intersection happens.
	/// </summary>
	/// <param name="ray">A <see cref="T:Microsoft.Xna.Framework.Ray" /> for intersection test.</param>
	/// <returns>Distance at which ray intersects with this <see cref="T:Microsoft.Xna.Framework.BoundingFrustum" /> or null if no intersection happens.</returns>
	public float? Intersects(Ray ray)
	{
		this.Intersects(ref ray, out var result);
		return result;
	}

	/// <summary>
	/// Gets the distance of intersection of <see cref="T:Microsoft.Xna.Framework.Ray" /> and this <see cref="T:Microsoft.Xna.Framework.BoundingFrustum" /> or null if no intersection happens.
	/// </summary>
	/// <param name="ray">A <see cref="T:Microsoft.Xna.Framework.Ray" /> for intersection test.</param>
	/// <param name="result">Distance at which ray intersects with this <see cref="T:Microsoft.Xna.Framework.BoundingFrustum" /> or null if no intersection happens as an output parameter.</param>
	public void Intersects(ref Ray ray, out float? result)
	{
		this.Contains(ref ray.Position, out var ctype);
		switch (ctype)
		{
		case ContainmentType.Disjoint:
			result = null;
			break;
		case ContainmentType.Contains:
			result = 0f;
			break;
		case ContainmentType.Intersects:
			throw new NotImplementedException();
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	/// <summary>
	/// Returns a <see cref="T:System.String" /> representation of this <see cref="T:Microsoft.Xna.Framework.BoundingFrustum" /> in the format:
	/// {Near:[nearPlane] Far:[farPlane] Left:[leftPlane] Right:[rightPlane] Top:[topPlane] Bottom:[bottomPlane]}
	/// </summary>
	/// <returns><see cref="T:System.String" /> representation of this <see cref="T:Microsoft.Xna.Framework.BoundingFrustum" />.</returns>
	public override string ToString()
	{
		string[] obj = new string[13]
		{
			"{Near: ", null, null, null, null, null, null, null, null, null,
			null, null, null
		};
		Plane plane = this._planes[0];
		obj[1] = plane.ToString();
		obj[2] = " Far:";
		plane = this._planes[1];
		obj[3] = plane.ToString();
		obj[4] = " Left:";
		plane = this._planes[2];
		obj[5] = plane.ToString();
		obj[6] = " Right:";
		plane = this._planes[3];
		obj[7] = plane.ToString();
		obj[8] = " Top:";
		plane = this._planes[4];
		obj[9] = plane.ToString();
		obj[10] = " Bottom:";
		plane = this._planes[5];
		obj[11] = plane.ToString();
		obj[12] = "}";
		return string.Concat(obj);
	}

	private void CreateCorners()
	{
		BoundingFrustum.IntersectionPoint(ref this._planes[0], ref this._planes[2], ref this._planes[4], out this._corners[0]);
		BoundingFrustum.IntersectionPoint(ref this._planes[0], ref this._planes[3], ref this._planes[4], out this._corners[1]);
		BoundingFrustum.IntersectionPoint(ref this._planes[0], ref this._planes[3], ref this._planes[5], out this._corners[2]);
		BoundingFrustum.IntersectionPoint(ref this._planes[0], ref this._planes[2], ref this._planes[5], out this._corners[3]);
		BoundingFrustum.IntersectionPoint(ref this._planes[1], ref this._planes[2], ref this._planes[4], out this._corners[4]);
		BoundingFrustum.IntersectionPoint(ref this._planes[1], ref this._planes[3], ref this._planes[4], out this._corners[5]);
		BoundingFrustum.IntersectionPoint(ref this._planes[1], ref this._planes[3], ref this._planes[5], out this._corners[6]);
		BoundingFrustum.IntersectionPoint(ref this._planes[1], ref this._planes[2], ref this._planes[5], out this._corners[7]);
	}

	private void CreatePlanes()
	{
		this._planes[0] = new Plane(0f - this._matrix.M13, 0f - this._matrix.M23, 0f - this._matrix.M33, 0f - this._matrix.M43);
		this._planes[1] = new Plane(this._matrix.M13 - this._matrix.M14, this._matrix.M23 - this._matrix.M24, this._matrix.M33 - this._matrix.M34, this._matrix.M43 - this._matrix.M44);
		this._planes[2] = new Plane(0f - this._matrix.M14 - this._matrix.M11, 0f - this._matrix.M24 - this._matrix.M21, 0f - this._matrix.M34 - this._matrix.M31, 0f - this._matrix.M44 - this._matrix.M41);
		this._planes[3] = new Plane(this._matrix.M11 - this._matrix.M14, this._matrix.M21 - this._matrix.M24, this._matrix.M31 - this._matrix.M34, this._matrix.M41 - this._matrix.M44);
		this._planes[4] = new Plane(this._matrix.M12 - this._matrix.M14, this._matrix.M22 - this._matrix.M24, this._matrix.M32 - this._matrix.M34, this._matrix.M42 - this._matrix.M44);
		this._planes[5] = new Plane(0f - this._matrix.M14 - this._matrix.M12, 0f - this._matrix.M24 - this._matrix.M22, 0f - this._matrix.M34 - this._matrix.M32, 0f - this._matrix.M44 - this._matrix.M42);
		this.NormalizePlane(ref this._planes[0]);
		this.NormalizePlane(ref this._planes[1]);
		this.NormalizePlane(ref this._planes[2]);
		this.NormalizePlane(ref this._planes[3]);
		this.NormalizePlane(ref this._planes[4]);
		this.NormalizePlane(ref this._planes[5]);
	}

	private static void IntersectionPoint(ref Plane a, ref Plane b, ref Plane c, out Vector3 result)
	{
		Vector3.Cross(ref b.Normal, ref c.Normal, out var cross);
		Vector3.Dot(ref a.Normal, ref cross, out var f);
		f *= -1f;
		Vector3.Cross(ref b.Normal, ref c.Normal, out cross);
		Vector3.Multiply(ref cross, a.D, out var v1);
		Vector3.Cross(ref c.Normal, ref a.Normal, out cross);
		Vector3.Multiply(ref cross, b.D, out var v2);
		Vector3.Cross(ref a.Normal, ref b.Normal, out cross);
		Vector3.Multiply(ref cross, c.D, out var v3);
		result.X = (v1.X + v2.X + v3.X) / f;
		result.Y = (v1.Y + v2.Y + v3.Y) / f;
		result.Z = (v1.Z + v2.Z + v3.Z) / f;
	}

	private void NormalizePlane(ref Plane p)
	{
		float factor = 1f / p.Normal.Length();
		p.Normal.X *= factor;
		p.Normal.Y *= factor;
		p.Normal.Z *= factor;
		p.D *= factor;
	}
}
