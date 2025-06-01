using System;
using System.IO;

namespace Netcode;

/// <summary>Stores an integer value.</summary>
/// <remarks><see cref="T:Netcode.NetInt" /> and <see cref="T:Netcode.NetIntDelta" /> are closely related, but resolve simultaneous changes differently. Whereas NetInt sends absolute values over the network, NetIntDelta sends the change relative to the previous value, so that a simultaneous increase of 1 on two peers results in an overall increase by 2.</remarks>
public sealed class NetIntDelta : NetField<int, NetIntDelta>
{
	private int networkValue;

	public int DirtyThreshold;

	public int? Minimum;

	public int? Maximum;

	public NetIntDelta()
	{
		base.Interpolated(interpolate: false, wait: false);
	}

	public NetIntDelta(int value)
		: base(value)
	{
		base.Interpolated(interpolate: false, wait: false);
	}

	private int fixRange(int value)
	{
		if (this.Minimum.HasValue)
		{
			value = Math.Max(this.Minimum.Value, value);
		}
		if (this.Maximum.HasValue)
		{
			value = Math.Min(this.Maximum.Value, value);
		}
		return value;
	}

	public override void Set(int newValue)
	{
		newValue = this.fixRange(newValue);
		if (newValue != base.value)
		{
			base.cleanSet(newValue);
			if (Math.Abs(newValue - this.networkValue) > this.DirtyThreshold)
			{
				base.MarkDirty();
			}
		}
	}

	protected override int interpolate(int startValue, int endValue, float factor)
	{
		return startValue + (int)((float)(endValue - startValue) * factor);
	}

	protected override void ReadDelta(BinaryReader reader, NetVersion version)
	{
		int delta = reader.ReadInt32();
		this.networkValue = this.fixRange(this.networkValue + delta);
		base.setInterpolationTarget(this.fixRange(base.targetValue + delta));
	}

	protected override void WriteDelta(BinaryWriter writer)
	{
		writer.Write(base.targetValue - this.networkValue);
		this.networkValue = base.targetValue;
	}

	public override void ReadFull(BinaryReader reader, NetVersion version)
	{
		int fullValue = reader.ReadInt32();
		base.cleanSet(fullValue);
		this.networkValue = fullValue;
		base.ChangeVersion.Merge(version);
	}

	public override void WriteFull(BinaryWriter writer)
	{
		writer.Write(base.targetValue);
		this.networkValue = base.targetValue;
	}
}
