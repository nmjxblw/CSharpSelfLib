using System.IO;

namespace Netcode;

public sealed class NetDouble : NetField<double, NetDouble>
{
	public NetDouble()
	{
	}

	public NetDouble(double value)
		: base(value)
	{
	}

	public override void Set(double newValue)
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

	protected override double interpolate(double startValue, double endValue, float factor)
	{
		return startValue + (endValue - startValue) * (double)factor;
	}

	protected override void ReadDelta(BinaryReader reader, NetVersion version)
	{
		double newValue = reader.ReadDouble();
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
