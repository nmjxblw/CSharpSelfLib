using System.Runtime.Serialization;

namespace Microsoft.Xna.Framework.Graphics;

[DataContract]
public class DisplayMode
{
	private SurfaceFormat format;

	private int height;

	private int width;

	public float AspectRatio => (float)this.width / (float)this.height;

	public SurfaceFormat Format => this.format;

	public int Height => this.height;

	public int Width => this.width;

	public Rectangle TitleSafeArea => GraphicsDevice.GetTitleSafeArea(0, 0, this.width, this.height);

	internal DisplayMode(int width, int height, SurfaceFormat format)
	{
		this.width = width;
		this.height = height;
		this.format = format;
	}

	public static bool operator !=(DisplayMode left, DisplayMode right)
	{
		return !(left == right);
	}

	public static bool operator ==(DisplayMode left, DisplayMode right)
	{
		if ((object)left == right)
		{
			return true;
		}
		if ((object)left == null || (object)right == null)
		{
			return false;
		}
		if (left.format == right.format && left.height == right.height)
		{
			return left.width == right.width;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is DisplayMode)
		{
			return this == (DisplayMode)obj;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return this.width.GetHashCode() ^ this.height.GetHashCode() ^ this.format.GetHashCode();
	}

	public override string ToString()
	{
		return "{Width:" + this.width + " Height:" + this.height + " Format:" + this.Format.ToString() + " AspectRatio:" + this.AspectRatio + "}";
	}
}
