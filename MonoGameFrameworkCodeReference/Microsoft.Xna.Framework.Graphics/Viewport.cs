using System.Runtime.Serialization;

namespace Microsoft.Xna.Framework.Graphics;

/// <summary>
/// Describes the view bounds for render-target surface.
/// </summary>
[DataContract]
public struct Viewport
{
	private int x;

	private int y;

	private int width;

	private int height;

	private float minDepth;

	private float maxDepth;

	/// <summary>
	/// The height of the bounds in pixels.
	/// </summary>
	[DataMember]
	public int Height
	{
		get
		{
			return this.height;
		}
		set
		{
			this.height = value;
		}
	}

	/// <summary>
	/// The upper limit of depth of this viewport.
	/// </summary>
	[DataMember]
	public float MaxDepth
	{
		get
		{
			return this.maxDepth;
		}
		set
		{
			this.maxDepth = value;
		}
	}

	/// <summary>
	/// The lower limit of depth of this viewport.
	/// </summary>
	[DataMember]
	public float MinDepth
	{
		get
		{
			return this.minDepth;
		}
		set
		{
			this.minDepth = value;
		}
	}

	/// <summary>
	/// The width of the bounds in pixels.
	/// </summary>
	[DataMember]
	public int Width
	{
		get
		{
			return this.width;
		}
		set
		{
			this.width = value;
		}
	}

	/// <summary>
	/// The y coordinate of the beginning of this viewport.
	/// </summary>
	[DataMember]
	public int Y
	{
		get
		{
			return this.y;
		}
		set
		{
			this.y = value;
		}
	}

	/// <summary>
	/// The x coordinate of the beginning of this viewport.
	/// </summary>
	[DataMember]
	public int X
	{
		get
		{
			return this.x;
		}
		set
		{
			this.x = value;
		}
	}

	/// <summary>
	/// Gets the aspect ratio of this <see cref="T:Microsoft.Xna.Framework.Graphics.Viewport" />, which is width / height. 
	/// </summary>
	public float AspectRatio
	{
		get
		{
			if (this.height != 0 && this.width != 0)
			{
				return (float)this.width / (float)this.height;
			}
			return 0f;
		}
	}

	/// <summary>
	/// Gets or sets a boundary of this <see cref="T:Microsoft.Xna.Framework.Graphics.Viewport" />.
	/// </summary>
	public Rectangle Bounds
	{
		get
		{
			return new Rectangle(this.x, this.y, this.width, this.height);
		}
		set
		{
			this.x = value.X;
			this.y = value.Y;
			this.width = value.Width;
			this.height = value.Height;
		}
	}

	/// <summary>
	/// Returns the subset of the viewport that is guaranteed to be visible on a lower quality display.
	/// </summary>
	public Rectangle TitleSafeArea => GraphicsDevice.GetTitleSafeArea(this.x, this.y, this.width, this.height);

	/// <summary>
	/// Constructs a viewport from the given values. The <see cref="P:Microsoft.Xna.Framework.Graphics.Viewport.MinDepth" /> will be 0.0 and <see cref="P:Microsoft.Xna.Framework.Graphics.Viewport.MaxDepth" /> will be 1.0.
	/// </summary>
	/// <param name="x">The x coordinate of the upper-left corner of the view bounds in pixels.</param>
	/// <param name="y">The y coordinate of the upper-left corner of the view bounds in pixels.</param>
	/// <param name="width">The width of the view bounds in pixels.</param>
	/// <param name="height">The height of the view bounds in pixels.</param>
	public Viewport(int x, int y, int width, int height)
	{
		this.x = x;
		this.y = y;
		this.width = width;
		this.height = height;
		this.minDepth = 0f;
		this.maxDepth = 1f;
	}

	/// <summary>
	/// Constructs a viewport from the given values.
	/// </summary>
	/// <param name="x">The x coordinate of the upper-left corner of the view bounds in pixels.</param>
	/// <param name="y">The y coordinate of the upper-left corner of the view bounds in pixels.</param>
	/// <param name="width">The width of the view bounds in pixels.</param>
	/// <param name="height">The height of the view bounds in pixels.</param>
	/// <param name="minDepth">The lower limit of depth.</param>
	/// <param name="maxDepth">The upper limit of depth.</param>
	public Viewport(int x, int y, int width, int height, float minDepth, float maxDepth)
	{
		this.x = x;
		this.y = y;
		this.width = width;
		this.height = height;
		this.minDepth = minDepth;
		this.maxDepth = maxDepth;
	}

	/// <summary>
	/// Creates a new instance of <see cref="T:Microsoft.Xna.Framework.Graphics.Viewport" /> struct.
	/// </summary>
	/// <param name="bounds">A <see cref="T:Microsoft.Xna.Framework.Rectangle" /> that defines the location and size of the <see cref="T:Microsoft.Xna.Framework.Graphics.Viewport" /> in a render target.</param>
	public Viewport(Rectangle bounds)
		: this(bounds.X, bounds.Y, bounds.Width, bounds.Height)
	{
	}

	/// <summary>
	/// Projects a <see cref="T:Microsoft.Xna.Framework.Vector3" /> from model space into screen space.
	/// The source point is transformed from model space to world space by the world matrix,
	/// then from world space to view space by the view matrix, and
	/// finally from view space to screen space by the projection matrix.
	/// </summary>
	/// <param name="source">The <see cref="T:Microsoft.Xna.Framework.Vector3" /> to project.</param>
	/// <param name="projection">The projection <see cref="T:Microsoft.Xna.Framework.Matrix" />.</param>
	/// <param name="view">The view <see cref="T:Microsoft.Xna.Framework.Matrix" />.</param>
	/// <param name="world">The world <see cref="T:Microsoft.Xna.Framework.Matrix" />.</param>
	/// <returns></returns>
	public Vector3 Project(Vector3 source, Matrix projection, Matrix view, Matrix world)
	{
		Matrix matrix = Matrix.Multiply(Matrix.Multiply(world, view), projection);
		Vector3 vector = Vector3.Transform(source, matrix);
		float a = source.X * matrix.M14 + source.Y * matrix.M24 + source.Z * matrix.M34 + matrix.M44;
		if (!Viewport.WithinEpsilon(a, 1f))
		{
			vector.X /= a;
			vector.Y /= a;
			vector.Z /= a;
		}
		vector.X = (vector.X + 1f) * 0.5f * (float)this.width + (float)this.x;
		vector.Y = (0f - vector.Y + 1f) * 0.5f * (float)this.height + (float)this.y;
		vector.Z = vector.Z * (this.maxDepth - this.minDepth) + this.minDepth;
		return vector;
	}

	/// <summary>
	/// Unprojects a <see cref="T:Microsoft.Xna.Framework.Vector3" /> from screen space into model space.
	/// The source point is transformed from screen space to view space by the inverse of the projection matrix,
	/// then from view space to world space by the inverse of the view matrix, and
	/// finally from world space to model space by the inverse of the world matrix.
	/// Note source.Z must be less than or equal to MaxDepth.
	/// </summary>
	/// <param name="source">The <see cref="T:Microsoft.Xna.Framework.Vector3" /> to unproject.</param>
	/// <param name="projection">The projection <see cref="T:Microsoft.Xna.Framework.Matrix" />.</param>
	/// <param name="view">The view <see cref="T:Microsoft.Xna.Framework.Matrix" />.</param>
	/// <param name="world">The world <see cref="T:Microsoft.Xna.Framework.Matrix" />.</param>
	/// <returns></returns>
	public Vector3 Unproject(Vector3 source, Matrix projection, Matrix view, Matrix world)
	{
		Matrix matrix = Matrix.Invert(Matrix.Multiply(Matrix.Multiply(world, view), projection));
		source.X = (source.X - (float)this.x) / (float)this.width * 2f - 1f;
		source.Y = 0f - ((source.Y - (float)this.y) / (float)this.height * 2f - 1f);
		source.Z = (source.Z - this.minDepth) / (this.maxDepth - this.minDepth);
		Vector3 vector = Vector3.Transform(source, matrix);
		float a = source.X * matrix.M14 + source.Y * matrix.M24 + source.Z * matrix.M34 + matrix.M44;
		if (!Viewport.WithinEpsilon(a, 1f))
		{
			vector.X /= a;
			vector.Y /= a;
			vector.Z /= a;
		}
		return vector;
	}

	private static bool WithinEpsilon(float a, float b)
	{
		float num = a - b;
		if (-1E-45f <= num)
		{
			return num <= float.Epsilon;
		}
		return false;
	}

	/// <summary>
	/// Returns a <see cref="T:System.String" /> representation of this <see cref="T:Microsoft.Xna.Framework.Graphics.Viewport" /> in the format:
	/// {X:[<see cref="P:Microsoft.Xna.Framework.Graphics.Viewport.X" />] Y:[<see cref="P:Microsoft.Xna.Framework.Graphics.Viewport.Y" />] Width:[<see cref="P:Microsoft.Xna.Framework.Graphics.Viewport.Width" />] Height:[<see cref="P:Microsoft.Xna.Framework.Graphics.Viewport.Height" />] MinDepth:[<see cref="P:Microsoft.Xna.Framework.Graphics.Viewport.MinDepth" />] MaxDepth:[<see cref="P:Microsoft.Xna.Framework.Graphics.Viewport.MaxDepth" />]}
	/// </summary>
	/// <returns>A <see cref="T:System.String" /> representation of this <see cref="T:Microsoft.Xna.Framework.Graphics.Viewport" />.</returns>
	public override string ToString()
	{
		return "{X:" + this.x + " Y:" + this.y + " Width:" + this.width + " Height:" + this.height + " MinDepth:" + this.minDepth + " MaxDepth:" + this.maxDepth + "}";
	}
}
