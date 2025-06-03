using System;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Microsoft.Xna.Framework;

/// <summary>
/// Describes a 2D-rectangle. 
/// </summary>
[DataContract]
[DebuggerDisplay("{DebugDisplayString,nq}")]
public struct Rectangle : IEquatable<Rectangle>
{
	private static Rectangle emptyRectangle;

	/// <summary>
	/// The x coordinate of the top-left corner of this <see cref="T:Microsoft.Xna.Framework.Rectangle" />.
	/// </summary>
	[DataMember]
	public int X;

	/// <summary>
	/// The y coordinate of the top-left corner of this <see cref="T:Microsoft.Xna.Framework.Rectangle" />.
	/// </summary>
	[DataMember]
	public int Y;

	/// <summary>
	/// The width of this <see cref="T:Microsoft.Xna.Framework.Rectangle" />.
	/// </summary>
	[DataMember]
	public int Width;

	/// <summary>
	/// The height of this <see cref="T:Microsoft.Xna.Framework.Rectangle" />.
	/// </summary>
	[DataMember]
	public int Height;

	/// <summary>
	/// Returns a <see cref="T:Microsoft.Xna.Framework.Rectangle" /> with X=0, Y=0, Width=0, Height=0.
	/// </summary>
	public static Rectangle Empty => Rectangle.emptyRectangle;

	/// <summary>
	/// Returns the x coordinate of the left edge of this <see cref="T:Microsoft.Xna.Framework.Rectangle" />.
	/// </summary>
	public int Left => this.X;

	/// <summary>
	/// Returns the x coordinate of the right edge of this <see cref="T:Microsoft.Xna.Framework.Rectangle" />.
	/// </summary>
	public int Right => this.X + this.Width;

	/// <summary>
	/// Returns the y coordinate of the top edge of this <see cref="T:Microsoft.Xna.Framework.Rectangle" />.
	/// </summary>
	public int Top => this.Y;

	/// <summary>
	/// Returns the y coordinate of the bottom edge of this <see cref="T:Microsoft.Xna.Framework.Rectangle" />.
	/// </summary>
	public int Bottom => this.Y + this.Height;

	/// <summary>
	/// Whether or not this <see cref="T:Microsoft.Xna.Framework.Rectangle" /> has a <see cref="F:Microsoft.Xna.Framework.Rectangle.Width" /> and
	/// <see cref="F:Microsoft.Xna.Framework.Rectangle.Height" /> of 0, and a <see cref="P:Microsoft.Xna.Framework.Rectangle.Location" /> of (0, 0).
	/// </summary>
	public bool IsEmpty
	{
		get
		{
			if (this.Width == 0 && this.Height == 0 && this.X == 0)
			{
				return this.Y == 0;
			}
			return false;
		}
	}

	/// <summary>
	/// The top-left coordinates of this <see cref="T:Microsoft.Xna.Framework.Rectangle" />.
	/// </summary>
	public Point Location
	{
		get
		{
			return new Point(this.X, this.Y);
		}
		set
		{
			this.X = value.X;
			this.Y = value.Y;
		}
	}

	/// <summary>
	/// The width-height coordinates of this <see cref="T:Microsoft.Xna.Framework.Rectangle" />.
	/// </summary>
	public Point Size
	{
		get
		{
			return new Point(this.Width, this.Height);
		}
		set
		{
			this.Width = value.X;
			this.Height = value.Y;
		}
	}

	/// <summary>
	/// A <see cref="T:Microsoft.Xna.Framework.Point" /> located in the center of this <see cref="T:Microsoft.Xna.Framework.Rectangle" />.
	/// </summary>
	/// <remarks>
	/// If <see cref="F:Microsoft.Xna.Framework.Rectangle.Width" /> or <see cref="F:Microsoft.Xna.Framework.Rectangle.Height" /> is an odd number,
	/// the center point will be rounded down.
	/// </remarks>
	public Point Center => new Point(this.X + this.Width / 2, this.Y + this.Height / 2);

	internal string DebugDisplayString => this.X + "  " + this.Y + "  " + this.Width + "  " + this.Height;

	/// <summary>
	/// Creates a new instance of <see cref="T:Microsoft.Xna.Framework.Rectangle" /> struct, with the specified
	/// position, width, and height.
	/// </summary>
	/// <param name="x">The x coordinate of the top-left corner of the created <see cref="T:Microsoft.Xna.Framework.Rectangle" />.</param>
	/// <param name="y">The y coordinate of the top-left corner of the created <see cref="T:Microsoft.Xna.Framework.Rectangle" />.</param>
	/// <param name="width">The width of the created <see cref="T:Microsoft.Xna.Framework.Rectangle" />.</param>
	/// <param name="height">The height of the created <see cref="T:Microsoft.Xna.Framework.Rectangle" />.</param>
	public Rectangle(int x, int y, int width, int height)
	{
		this.X = x;
		this.Y = y;
		this.Width = width;
		this.Height = height;
	}

	/// <summary>
	/// Creates a new instance of <see cref="T:Microsoft.Xna.Framework.Rectangle" /> struct, with the specified
	/// location and size.
	/// </summary>
	/// <param name="location">The x and y coordinates of the top-left corner of the created <see cref="T:Microsoft.Xna.Framework.Rectangle" />.</param>
	/// <param name="size">The width and height of the created <see cref="T:Microsoft.Xna.Framework.Rectangle" />.</param>
	public Rectangle(Point location, Point size)
	{
		this.X = location.X;
		this.Y = location.Y;
		this.Width = size.X;
		this.Height = size.Y;
	}

	/// <summary>
	/// Compares whether two <see cref="T:Microsoft.Xna.Framework.Rectangle" /> instances are equal.
	/// </summary>
	/// <param name="a"><see cref="T:Microsoft.Xna.Framework.Rectangle" /> instance on the left of the equal sign.</param>
	/// <param name="b"><see cref="T:Microsoft.Xna.Framework.Rectangle" /> instance on the right of the equal sign.</param>
	/// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
	public static bool operator ==(Rectangle a, Rectangle b)
	{
		if (a.X == b.X && a.Y == b.Y && a.Width == b.Width)
		{
			return a.Height == b.Height;
		}
		return false;
	}

	/// <summary>
	/// Compares whether two <see cref="T:Microsoft.Xna.Framework.Rectangle" /> instances are not equal.
	/// </summary>
	/// <param name="a"><see cref="T:Microsoft.Xna.Framework.Rectangle" /> instance on the left of the not equal sign.</param>
	/// <param name="b"><see cref="T:Microsoft.Xna.Framework.Rectangle" /> instance on the right of the not equal sign.</param>
	/// <returns><c>true</c> if the instances are not equal; <c>false</c> otherwise.</returns>
	public static bool operator !=(Rectangle a, Rectangle b)
	{
		return !(a == b);
	}

	/// <summary>
	/// Gets whether or not the provided coordinates lie within the bounds of this <see cref="T:Microsoft.Xna.Framework.Rectangle" />.
	/// </summary>
	/// <param name="x">The x coordinate of the point to check for containment.</param>
	/// <param name="y">The y coordinate of the point to check for containment.</param>
	/// <returns><c>true</c> if the provided coordinates lie inside this <see cref="T:Microsoft.Xna.Framework.Rectangle" />; <c>false</c> otherwise.</returns>
	public bool Contains(int x, int y)
	{
		if (this.X <= x && x < this.X + this.Width && this.Y <= y)
		{
			return y < this.Y + this.Height;
		}
		return false;
	}

	/// <summary>
	/// Gets whether or not the provided coordinates lie within the bounds of this <see cref="T:Microsoft.Xna.Framework.Rectangle" />.
	/// </summary>
	/// <param name="x">The x coordinate of the point to check for containment.</param>
	/// <param name="y">The y coordinate of the point to check for containment.</param>
	/// <returns><c>true</c> if the provided coordinates lie inside this <see cref="T:Microsoft.Xna.Framework.Rectangle" />; <c>false</c> otherwise.</returns>
	public bool Contains(float x, float y)
	{
		if ((float)this.X <= x && x < (float)(this.X + this.Width) && (float)this.Y <= y)
		{
			return y < (float)(this.Y + this.Height);
		}
		return false;
	}

	/// <summary>
	/// Gets whether or not the provided <see cref="T:Microsoft.Xna.Framework.Point" /> lies within the bounds of this <see cref="T:Microsoft.Xna.Framework.Rectangle" />.
	/// </summary>
	/// <param name="value">The coordinates to check for inclusion in this <see cref="T:Microsoft.Xna.Framework.Rectangle" />.</param>
	/// <returns><c>true</c> if the provided <see cref="T:Microsoft.Xna.Framework.Point" /> lies inside this <see cref="T:Microsoft.Xna.Framework.Rectangle" />; <c>false</c> otherwise.</returns>
	public bool Contains(Point value)
	{
		if (this.X <= value.X && value.X < this.X + this.Width && this.Y <= value.Y)
		{
			return value.Y < this.Y + this.Height;
		}
		return false;
	}

	/// <summary>
	/// Gets whether or not the provided <see cref="T:Microsoft.Xna.Framework.Point" /> lies within the bounds of this <see cref="T:Microsoft.Xna.Framework.Rectangle" />.
	/// </summary>
	/// <param name="value">The coordinates to check for inclusion in this <see cref="T:Microsoft.Xna.Framework.Rectangle" />.</param>
	/// <param name="result"><c>true</c> if the provided <see cref="T:Microsoft.Xna.Framework.Point" /> lies inside this <see cref="T:Microsoft.Xna.Framework.Rectangle" />; <c>false</c> otherwise. As an output parameter.</param>
	public void Contains(ref Point value, out bool result)
	{
		result = this.X <= value.X && value.X < this.X + this.Width && this.Y <= value.Y && value.Y < this.Y + this.Height;
	}

	/// <summary>
	/// Gets whether or not the provided <see cref="T:Microsoft.Xna.Framework.Vector2" /> lies within the bounds of this <see cref="T:Microsoft.Xna.Framework.Rectangle" />.
	/// </summary>
	/// <param name="value">The coordinates to check for inclusion in this <see cref="T:Microsoft.Xna.Framework.Rectangle" />.</param>
	/// <returns><c>true</c> if the provided <see cref="T:Microsoft.Xna.Framework.Vector2" /> lies inside this <see cref="T:Microsoft.Xna.Framework.Rectangle" />; <c>false</c> otherwise.</returns>
	public bool Contains(Vector2 value)
	{
		if ((float)this.X <= value.X && value.X < (float)(this.X + this.Width) && (float)this.Y <= value.Y)
		{
			return value.Y < (float)(this.Y + this.Height);
		}
		return false;
	}

	/// <summary>
	/// Gets whether or not the provided <see cref="T:Microsoft.Xna.Framework.Vector2" /> lies within the bounds of this <see cref="T:Microsoft.Xna.Framework.Rectangle" />.
	/// </summary>
	/// <param name="value">The coordinates to check for inclusion in this <see cref="T:Microsoft.Xna.Framework.Rectangle" />.</param>
	/// <param name="result"><c>true</c> if the provided <see cref="T:Microsoft.Xna.Framework.Vector2" /> lies inside this <see cref="T:Microsoft.Xna.Framework.Rectangle" />; <c>false</c> otherwise. As an output parameter.</param>
	public void Contains(ref Vector2 value, out bool result)
	{
		result = (float)this.X <= value.X && value.X < (float)(this.X + this.Width) && (float)this.Y <= value.Y && value.Y < (float)(this.Y + this.Height);
	}

	/// <summary>
	/// Gets whether or not the provided <see cref="T:Microsoft.Xna.Framework.Rectangle" /> lies within the bounds of this <see cref="T:Microsoft.Xna.Framework.Rectangle" />.
	/// </summary>
	/// <param name="value">The <see cref="T:Microsoft.Xna.Framework.Rectangle" /> to check for inclusion in this <see cref="T:Microsoft.Xna.Framework.Rectangle" />.</param>
	/// <returns><c>true</c> if the provided <see cref="T:Microsoft.Xna.Framework.Rectangle" />'s bounds lie entirely inside this <see cref="T:Microsoft.Xna.Framework.Rectangle" />; <c>false</c> otherwise.</returns>
	public bool Contains(Rectangle value)
	{
		if (this.X <= value.X && value.X + value.Width <= this.X + this.Width && this.Y <= value.Y)
		{
			return value.Y + value.Height <= this.Y + this.Height;
		}
		return false;
	}

	/// <summary>
	/// Gets whether or not the provided <see cref="T:Microsoft.Xna.Framework.Rectangle" /> lies within the bounds of this <see cref="T:Microsoft.Xna.Framework.Rectangle" />.
	/// </summary>
	/// <param name="value">The <see cref="T:Microsoft.Xna.Framework.Rectangle" /> to check for inclusion in this <see cref="T:Microsoft.Xna.Framework.Rectangle" />.</param>
	/// <param name="result"><c>true</c> if the provided <see cref="T:Microsoft.Xna.Framework.Rectangle" />'s bounds lie entirely inside this <see cref="T:Microsoft.Xna.Framework.Rectangle" />; <c>false</c> otherwise. As an output parameter.</param>
	public void Contains(ref Rectangle value, out bool result)
	{
		result = this.X <= value.X && value.X + value.Width <= this.X + this.Width && this.Y <= value.Y && value.Y + value.Height <= this.Y + this.Height;
	}

	/// <summary>
	/// Compares whether current instance is equal to specified <see cref="T:System.Object" />.
	/// </summary>
	/// <param name="obj">The <see cref="T:System.Object" /> to compare.</param>
	/// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
	public override bool Equals(object obj)
	{
		if (obj is Rectangle)
		{
			return this == (Rectangle)obj;
		}
		return false;
	}

	/// <summary>
	/// Compares whether current instance is equal to specified <see cref="T:Microsoft.Xna.Framework.Rectangle" />.
	/// </summary>
	/// <param name="other">The <see cref="T:Microsoft.Xna.Framework.Rectangle" /> to compare.</param>
	/// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
	public bool Equals(Rectangle other)
	{
		return this == other;
	}

	/// <summary>
	/// Gets the hash code of this <see cref="T:Microsoft.Xna.Framework.Rectangle" />.
	/// </summary>
	/// <returns>Hash code of this <see cref="T:Microsoft.Xna.Framework.Rectangle" />.</returns>
	public override int GetHashCode()
	{
		return (((17 * 23 + this.X.GetHashCode()) * 23 + this.Y.GetHashCode()) * 23 + this.Width.GetHashCode()) * 23 + this.Height.GetHashCode();
	}

	/// <summary>
	/// Adjusts the edges of this <see cref="T:Microsoft.Xna.Framework.Rectangle" /> by specified horizontal and vertical amounts. 
	/// </summary>
	/// <param name="horizontalAmount">Value to adjust the left and right edges.</param>
	/// <param name="verticalAmount">Value to adjust the top and bottom edges.</param>
	public void Inflate(int horizontalAmount, int verticalAmount)
	{
		this.X -= horizontalAmount;
		this.Y -= verticalAmount;
		this.Width += horizontalAmount * 2;
		this.Height += verticalAmount * 2;
	}

	/// <summary>
	/// Adjusts the edges of this <see cref="T:Microsoft.Xna.Framework.Rectangle" /> by specified horizontal and vertical amounts. 
	/// </summary>
	/// <param name="horizontalAmount">Value to adjust the left and right edges.</param>
	/// <param name="verticalAmount">Value to adjust the top and bottom edges.</param>
	public void Inflate(float horizontalAmount, float verticalAmount)
	{
		this.X -= (int)horizontalAmount;
		this.Y -= (int)verticalAmount;
		this.Width += (int)horizontalAmount * 2;
		this.Height += (int)verticalAmount * 2;
	}

	/// <summary>
	/// Gets whether or not the other <see cref="T:Microsoft.Xna.Framework.Rectangle" /> intersects with this rectangle.
	/// </summary>
	/// <param name="value">The other rectangle for testing.</param>
	/// <returns><c>true</c> if other <see cref="T:Microsoft.Xna.Framework.Rectangle" /> intersects with this rectangle; <c>false</c> otherwise.</returns>
	public bool Intersects(Rectangle value)
	{
		if (value.Left < this.Right && this.Left < value.Right && value.Top < this.Bottom)
		{
			return this.Top < value.Bottom;
		}
		return false;
	}

	/// <summary>
	/// Gets whether or not the other <see cref="T:Microsoft.Xna.Framework.Rectangle" /> intersects with this rectangle.
	/// </summary>
	/// <param name="value">The other rectangle for testing.</param>
	/// <param name="result"><c>true</c> if other <see cref="T:Microsoft.Xna.Framework.Rectangle" /> intersects with this rectangle; <c>false</c> otherwise. As an output parameter.</param>
	public void Intersects(ref Rectangle value, out bool result)
	{
		result = value.Left < this.Right && this.Left < value.Right && value.Top < this.Bottom && this.Top < value.Bottom;
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Rectangle" /> that contains overlapping region of two other rectangles.
	/// </summary>
	/// <param name="value1">The first <see cref="T:Microsoft.Xna.Framework.Rectangle" />.</param>
	/// <param name="value2">The second <see cref="T:Microsoft.Xna.Framework.Rectangle" />.</param>
	/// <returns>Overlapping region of the two rectangles.</returns>
	public static Rectangle Intersect(Rectangle value1, Rectangle value2)
	{
		Rectangle.Intersect(ref value1, ref value2, out var rectangle);
		return rectangle;
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Rectangle" /> that contains overlapping region of two other rectangles.
	/// </summary>
	/// <param name="value1">The first <see cref="T:Microsoft.Xna.Framework.Rectangle" />.</param>
	/// <param name="value2">The second <see cref="T:Microsoft.Xna.Framework.Rectangle" />.</param>
	/// <param name="result">Overlapping region of the two rectangles as an output parameter.</param>
	public static void Intersect(ref Rectangle value1, ref Rectangle value2, out Rectangle result)
	{
		if (value1.Intersects(value2))
		{
			int right_side = Math.Min(value1.X + value1.Width, value2.X + value2.Width);
			int left_side = Math.Max(value1.X, value2.X);
			int top_side = Math.Max(value1.Y, value2.Y);
			int bottom_side = Math.Min(value1.Y + value1.Height, value2.Y + value2.Height);
			result = new Rectangle(left_side, top_side, right_side - left_side, bottom_side - top_side);
		}
		else
		{
			result = new Rectangle(0, 0, 0, 0);
		}
	}

	/// <summary>
	/// Changes the <see cref="P:Microsoft.Xna.Framework.Rectangle.Location" /> of this <see cref="T:Microsoft.Xna.Framework.Rectangle" />.
	/// </summary>
	/// <param name="offsetX">The x coordinate to add to this <see cref="T:Microsoft.Xna.Framework.Rectangle" />.</param>
	/// <param name="offsetY">The y coordinate to add to this <see cref="T:Microsoft.Xna.Framework.Rectangle" />.</param>
	public void Offset(int offsetX, int offsetY)
	{
		this.X += offsetX;
		this.Y += offsetY;
	}

	/// <summary>
	/// Changes the <see cref="P:Microsoft.Xna.Framework.Rectangle.Location" /> of this <see cref="T:Microsoft.Xna.Framework.Rectangle" />.
	/// </summary>
	/// <param name="offsetX">The x coordinate to add to this <see cref="T:Microsoft.Xna.Framework.Rectangle" />.</param>
	/// <param name="offsetY">The y coordinate to add to this <see cref="T:Microsoft.Xna.Framework.Rectangle" />.</param>
	public void Offset(float offsetX, float offsetY)
	{
		this.X += (int)offsetX;
		this.Y += (int)offsetY;
	}

	/// <summary>
	/// Changes the <see cref="P:Microsoft.Xna.Framework.Rectangle.Location" /> of this <see cref="T:Microsoft.Xna.Framework.Rectangle" />.
	/// </summary>
	/// <param name="amount">The x and y components to add to this <see cref="T:Microsoft.Xna.Framework.Rectangle" />.</param>
	public void Offset(Point amount)
	{
		this.X += amount.X;
		this.Y += amount.Y;
	}

	/// <summary>
	/// Changes the <see cref="P:Microsoft.Xna.Framework.Rectangle.Location" /> of this <see cref="T:Microsoft.Xna.Framework.Rectangle" />.
	/// </summary>
	/// <param name="amount">The x and y components to add to this <see cref="T:Microsoft.Xna.Framework.Rectangle" />.</param>
	public void Offset(Vector2 amount)
	{
		this.X += (int)amount.X;
		this.Y += (int)amount.Y;
	}

	/// <summary>
	/// Returns a <see cref="T:System.String" /> representation of this <see cref="T:Microsoft.Xna.Framework.Rectangle" /> in the format:
	/// {X:[<see cref="F:Microsoft.Xna.Framework.Rectangle.X" />] Y:[<see cref="F:Microsoft.Xna.Framework.Rectangle.Y" />] Width:[<see cref="F:Microsoft.Xna.Framework.Rectangle.Width" />] Height:[<see cref="F:Microsoft.Xna.Framework.Rectangle.Height" />]}
	/// </summary>
	/// <returns><see cref="T:System.String" /> representation of this <see cref="T:Microsoft.Xna.Framework.Rectangle" />.</returns>
	public override string ToString()
	{
		return "{X:" + this.X + " Y:" + this.Y + " Width:" + this.Width + " Height:" + this.Height + "}";
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Rectangle" /> that completely contains two other rectangles.
	/// </summary>
	/// <param name="value1">The first <see cref="T:Microsoft.Xna.Framework.Rectangle" />.</param>
	/// <param name="value2">The second <see cref="T:Microsoft.Xna.Framework.Rectangle" />.</param>
	/// <returns>The union of the two rectangles.</returns>
	public static Rectangle Union(Rectangle value1, Rectangle value2)
	{
		int x = Math.Min(value1.X, value2.X);
		int y = Math.Min(value1.Y, value2.Y);
		return new Rectangle(x, y, Math.Max(value1.Right, value2.Right) - x, Math.Max(value1.Bottom, value2.Bottom) - y);
	}

	/// <summary>
	/// Creates a new <see cref="T:Microsoft.Xna.Framework.Rectangle" /> that completely contains two other rectangles.
	/// </summary>
	/// <param name="value1">The first <see cref="T:Microsoft.Xna.Framework.Rectangle" />.</param>
	/// <param name="value2">The second <see cref="T:Microsoft.Xna.Framework.Rectangle" />.</param>
	/// <param name="result">The union of the two rectangles as an output parameter.</param>
	public static void Union(ref Rectangle value1, ref Rectangle value2, out Rectangle result)
	{
		result.X = Math.Min(value1.X, value2.X);
		result.Y = Math.Min(value1.Y, value2.Y);
		result.Width = Math.Max(value1.Right, value2.Right) - result.X;
		result.Height = Math.Max(value1.Bottom, value2.Bottom) - result.Y;
	}

	/// <summary>
	/// Deconstruction method for <see cref="T:Microsoft.Xna.Framework.Rectangle" />.
	/// </summary>
	/// <param name="x"></param>
	/// <param name="y"></param>
	/// <param name="width"></param>
	/// <param name="height"></param>
	public void Deconstruct(out int x, out int y, out int width, out int height)
	{
		x = this.X;
		y = this.Y;
		width = this.Width;
		height = this.Height;
	}
}
