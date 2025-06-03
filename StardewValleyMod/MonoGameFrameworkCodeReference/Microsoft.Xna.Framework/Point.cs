using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace Microsoft.Xna.Framework;

/// <summary>
/// Describes a 2D-point.
/// </summary>
[DataContract]
[DebuggerDisplay("{DebugDisplayString,nq}")]
public struct Point : IEquatable<Point>
{
	private static readonly Point zeroPoint;

	/// <summary>
	/// The x coordinate of this <see cref="T:Microsoft.Xna.Framework.Point" />.
	/// </summary>
	[DataMember]
	public int X;

	/// <summary>
	/// The y coordinate of this <see cref="T:Microsoft.Xna.Framework.Point" />.
	/// </summary>
	[DataMember]
	public int Y;

	/// <summary>
	/// Returns a <see cref="T:Microsoft.Xna.Framework.Point" /> with coordinates 0, 0.
	/// </summary>
	public static Point Zero => Point.zeroPoint;

	internal string DebugDisplayString => this.X + "  " + this.Y;

	/// <summary>
	/// Constructs a point with X and Y from two values.
	/// </summary>
	/// <param name="x">The x coordinate in 2d-space.</param>
	/// <param name="y">The y coordinate in 2d-space.</param>
	public Point(int x, int y)
	{
		this.X = x;
		this.Y = y;
	}

	/// <summary>
	/// Constructs a point with X and Y set to the same value.
	/// </summary>
	/// <param name="value">The x and y coordinates in 2d-space.</param>
	public Point(int value)
	{
		this.X = value;
		this.Y = value;
	}

	/// <summary>
	/// Adds two points.
	/// </summary>
	/// <param name="value1">Source <see cref="T:Microsoft.Xna.Framework.Point" /> on the left of the add sign.</param>
	/// <param name="value2">Source <see cref="T:Microsoft.Xna.Framework.Point" /> on the right of the add sign.</param>
	/// <returns>Sum of the points.</returns>
	public static Point operator +(Point value1, Point value2)
	{
		return new Point(value1.X + value2.X, value1.Y + value2.Y);
	}

	/// <summary>
	/// Subtracts a <see cref="T:Microsoft.Xna.Framework.Point" /> from a <see cref="T:Microsoft.Xna.Framework.Point" />.
	/// </summary>
	/// <param name="value1">Source <see cref="T:Microsoft.Xna.Framework.Point" /> on the left of the sub sign.</param>
	/// <param name="value2">Source <see cref="T:Microsoft.Xna.Framework.Point" /> on the right of the sub sign.</param>
	/// <returns>Result of the subtraction.</returns>
	public static Point operator -(Point value1, Point value2)
	{
		return new Point(value1.X - value2.X, value1.Y - value2.Y);
	}

	/// <summary>
	/// Multiplies the components of two points by each other.
	/// </summary>
	/// <param name="value1">Source <see cref="T:Microsoft.Xna.Framework.Point" /> on the left of the mul sign.</param>
	/// <param name="value2">Source <see cref="T:Microsoft.Xna.Framework.Point" /> on the right of the mul sign.</param>
	/// <returns>Result of the multiplication.</returns>
	public static Point operator *(Point value1, Point value2)
	{
		return new Point(value1.X * value2.X, value1.Y * value2.Y);
	}

	/// <summary>
	/// Divides the components of a <see cref="T:Microsoft.Xna.Framework.Point" /> by the components of another <see cref="T:Microsoft.Xna.Framework.Point" />.
	/// </summary>
	/// <param name="source">Source <see cref="T:Microsoft.Xna.Framework.Point" /> on the left of the div sign.</param>
	/// <param name="divisor">Divisor <see cref="T:Microsoft.Xna.Framework.Point" /> on the right of the div sign.</param>
	/// <returns>The result of dividing the points.</returns>
	public static Point operator /(Point source, Point divisor)
	{
		return new Point(source.X / divisor.X, source.Y / divisor.Y);
	}

	/// <summary>
	/// Compares whether two <see cref="T:Microsoft.Xna.Framework.Point" /> instances are equal.
	/// </summary>
	/// <param name="a"><see cref="T:Microsoft.Xna.Framework.Point" /> instance on the left of the equal sign.</param>
	/// <param name="b"><see cref="T:Microsoft.Xna.Framework.Point" /> instance on the right of the equal sign.</param>
	/// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
	public static bool operator ==(Point a, Point b)
	{
		return a.Equals(b);
	}

	/// <summary>
	/// Compares whether two <see cref="T:Microsoft.Xna.Framework.Point" /> instances are not equal.
	/// </summary>
	/// <param name="a"><see cref="T:Microsoft.Xna.Framework.Point" /> instance on the left of the not equal sign.</param>
	/// <param name="b"><see cref="T:Microsoft.Xna.Framework.Point" /> instance on the right of the not equal sign.</param>
	/// <returns><c>true</c> if the instances are not equal; <c>false</c> otherwise.</returns>	
	public static bool operator !=(Point a, Point b)
	{
		return !a.Equals(b);
	}

	/// <summary>
	/// Compares whether current instance is equal to specified <see cref="T:System.Object" />.
	/// </summary>
	/// <param name="obj">The <see cref="T:System.Object" /> to compare.</param>
	/// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
	public override bool Equals(object obj)
	{
		if (obj is Point)
		{
			return this.Equals((Point)obj);
		}
		return false;
	}

	/// <summary>
	/// Compares whether current instance is equal to specified <see cref="T:Microsoft.Xna.Framework.Point" />.
	/// </summary>
	/// <param name="other">The <see cref="T:Microsoft.Xna.Framework.Point" /> to compare.</param>
	/// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
	public bool Equals(Point other)
	{
		if (this.X == other.X)
		{
			return this.Y == other.Y;
		}
		return false;
	}

	/// <summary>
	/// Gets the hash code of this <see cref="T:Microsoft.Xna.Framework.Point" />.
	/// </summary>
	/// <returns>Hash code of this <see cref="T:Microsoft.Xna.Framework.Point" />.</returns>
	public override int GetHashCode()
	{
		return (17 * 23 + this.X.GetHashCode()) * 23 + this.Y.GetHashCode();
	}

	/// <summary>
	/// Returns a <see cref="T:System.String" /> representation of this <see cref="T:Microsoft.Xna.Framework.Point" /> in the format:
	/// {X:[<see cref="F:Microsoft.Xna.Framework.Point.X" />] Y:[<see cref="F:Microsoft.Xna.Framework.Point.Y" />]}
	/// </summary>
	/// <returns><see cref="T:System.String" /> representation of this <see cref="T:Microsoft.Xna.Framework.Point" />.</returns>
	public override string ToString()
	{
		return "{X:" + this.X + " Y:" + this.Y + "}";
	}

	/// <summary>
	/// Gets a <see cref="T:Microsoft.Xna.Framework.Vector2" /> representation for this object.
	/// </summary>
	/// <returns>A <see cref="T:Microsoft.Xna.Framework.Vector2" /> representation for this object.</returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Vector2 ToVector2()
	{
		return new Vector2(this.X, this.Y);
	}

	/// <summary>
	/// Deconstruction method for <see cref="T:Microsoft.Xna.Framework.Point" />.
	/// </summary>
	/// <param name="x"></param>
	/// <param name="y"></param>
	public void Deconstruct(out int x, out int y)
	{
		x = this.X;
		y = this.Y;
	}
}
