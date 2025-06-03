using System.IO;
using Microsoft.Xna.Framework;

namespace Netcode;

public sealed class NetColor : NetField<Color, NetColor>
{
	public byte R
	{
		get
		{
			return base.Value.R;
		}
		set
		{
			base.Value = new Color(value, this.G, this.B, this.A);
		}
	}

	public byte G
	{
		get
		{
			return base.Value.G;
		}
		set
		{
			base.Value = new Color(this.R, value, this.B, this.A);
		}
	}

	public byte B
	{
		get
		{
			return base.Value.B;
		}
		set
		{
			base.Value = new Color(this.R, this.G, value, this.A);
		}
	}

	public byte A
	{
		get
		{
			return base.Value.A;
		}
		set
		{
			base.Value = new Color(this.R, this.G, this.B, value);
		}
	}

	public NetColor()
	{
	}

	public NetColor(Color value)
		: base(value)
	{
	}

	public override void Set(Color newValue)
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

	public new bool Equals(NetColor other)
	{
		return base.value == other.value;
	}

	public bool Equals(Color other)
	{
		return base.value == other;
	}

	protected override void ReadDelta(BinaryReader reader, NetVersion version)
	{
		Color newValue = new Color
		{
			PackedValue = reader.ReadUInt32()
		};
		if (version.IsPriorityOver(base.ChangeVersion))
		{
			base.setInterpolationTarget(newValue);
		}
	}

	protected override void WriteDelta(BinaryWriter writer)
	{
		writer.Write(base.value.PackedValue);
	}
}
