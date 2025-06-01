using System.IO;

namespace Netcode;

public sealed class NetByte : NetField<byte, NetByte>
{
	public NetByte()
	{
	}

	public NetByte(byte value)
		: base(value)
	{
	}

	public override void Set(byte newValue)
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
		byte newValue = reader.ReadByte();
		if (version.IsPriorityOver(base.ChangeVersion))
		{
			base.setInterpolationTarget(newValue);
		}
	}

	protected override void WriteDelta(BinaryWriter writer)
	{
		writer.Write(base.Value);
	}
}
