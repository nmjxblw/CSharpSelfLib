using System;
using System.IO;

namespace Netcode;

public sealed class NetGuid : NetField<Guid, NetGuid>
{
	public NetGuid()
	{
	}

	public NetGuid(Guid value)
		: base(value)
	{
	}

	public override void Set(Guid newValue)
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
		Guid newValue = reader.ReadGuid();
		if (version.IsPriorityOver(base.ChangeVersion))
		{
			base.setInterpolationTarget(newValue);
		}
	}

	protected override void WriteDelta(BinaryWriter writer)
	{
		writer.WriteGuid(base.value);
	}
}
