using System.IO;
using Microsoft.Xna.Framework;

namespace Netcode;

public sealed class NetRectangle : NetField<Rectangle, NetRectangle>
{
	public int X
	{
		get
		{
			return base.Value.X;
		}
		set
		{
			Rectangle rect = base.value;
			if (rect.X != value)
			{
				Rectangle newValue = new Rectangle(value, rect.Y, rect.Width, rect.Height);
				if (base.canShortcutSet())
				{
					base.value = newValue;
					return;
				}
				base.cleanSet(newValue);
				base.MarkDirty();
			}
		}
	}

	public int Y
	{
		get
		{
			return base.Value.Y;
		}
		set
		{
			Rectangle rect = base.value;
			if (rect.Y != value)
			{
				Rectangle newValue = new Rectangle(rect.X, value, rect.Width, rect.Height);
				if (base.canShortcutSet())
				{
					base.value = newValue;
					return;
				}
				base.cleanSet(newValue);
				base.MarkDirty();
			}
		}
	}

	public int Width
	{
		get
		{
			return base.Value.Width;
		}
		set
		{
			Rectangle rect = base.value;
			if (rect.Width != value)
			{
				Rectangle newValue = new Rectangle(rect.X, rect.Y, value, rect.Height);
				if (base.canShortcutSet())
				{
					base.value = newValue;
					return;
				}
				base.cleanSet(newValue);
				base.MarkDirty();
			}
		}
	}

	public int Height
	{
		get
		{
			return base.Value.Height;
		}
		set
		{
			Rectangle rect = base.value;
			if (rect.Height != value)
			{
				Rectangle newValue = new Rectangle(rect.X, rect.Y, rect.Width, value);
				if (base.canShortcutSet())
				{
					base.value = newValue;
					return;
				}
				base.cleanSet(newValue);
				base.MarkDirty();
			}
		}
	}

	public Point Center => base.value.Center;

	public int Top => base.value.Top;

	public int Bottom => base.value.Bottom;

	public int Left => base.value.Left;

	public int Right => base.value.Right;

	public NetRectangle()
	{
	}

	public NetRectangle(Rectangle value)
		: base(value)
	{
	}

	public void Set(int x, int y, int width, int height)
	{
		this.Set(new Rectangle(x, y, width, height));
	}

	public override void Set(Rectangle newValue)
	{
		if (base.canShortcutSet())
		{
			base.value = newValue;
		}
		else if (newValue != base.value)
		{
			base.cleanSet(newValue);
			base.MarkDirty();
		}
	}

	protected override void ReadDelta(BinaryReader reader, NetVersion version)
	{
		int newX = reader.ReadInt32();
		int newY = reader.ReadInt32();
		int newWidth = reader.ReadInt32();
		int newHeight = reader.ReadInt32();
		if (version.IsPriorityOver(base.ChangeVersion))
		{
			base.setInterpolationTarget(new Rectangle(newX, newY, newWidth, newHeight));
		}
	}

	protected override void WriteDelta(BinaryWriter writer)
	{
		writer.Write(base.value.X);
		writer.Write(base.value.Y);
		writer.Write(base.value.Width);
		writer.Write(base.value.Height);
	}
}
