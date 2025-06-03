using System.IO;

namespace Netcode;

/// <summary>Stores an integer value.</summary>
/// <inheritdoc cref="T:Netcode.NetIntDelta" path="/remarks" />
public sealed class NetInt : NetField<int, NetInt>
{
	public NetInt()
	{
	}

	public NetInt(int value)
		: base(value)
	{
	}

	public override void Set(int newValue)
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

	public new bool Equals(NetInt other)
	{
		return base.value == other.value;
	}

	public bool Equals(int other)
	{
		return base.value == other;
	}

	protected override int interpolate(int startValue, int endValue, float factor)
	{
		return startValue + (int)((float)(endValue - startValue) * factor);
	}

	protected override void ReadDelta(BinaryReader reader, NetVersion version)
	{
		int newValue = reader.ReadInt32();
		if (version.IsPriorityOver(base.ChangeVersion))
		{
			base.setInterpolationTarget(newValue);
		}
	}

	protected override void WriteDelta(BinaryWriter writer)
	{
		writer.Write(base.value);
	}
}
