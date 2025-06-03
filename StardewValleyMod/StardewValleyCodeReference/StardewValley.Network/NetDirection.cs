using System;
using System.IO;
using Microsoft.Xna.Framework;
using Netcode;

namespace StardewValley.Network;

public sealed class NetDirection : NetField<int, NetDirection>
{
	public NetPosition Position;

	public NetDirection()
	{
		base.InterpolationEnabled = true;
		base.InterpolationWait = true;
	}

	public NetDirection(int value)
		: base(value)
	{
		base.InterpolationEnabled = true;
		base.InterpolationWait = true;
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

	protected override bool setUpInterpolation(int oldValue, int newValue)
	{
		return true;
	}

	public int getInterpolatedDirection()
	{
		if (this.Position != null && this.Position.IsInterpolating() && !this.Position.IsPausePending())
		{
			Vector2 dir = this.Position.CurrentInterpolationDirection();
			if (Math.Abs(dir.X) > Math.Abs(dir.Y))
			{
				if (dir.X < 0f)
				{
					return 3;
				}
				return 1;
			}
			if (Math.Abs(dir.Y) > Math.Abs(dir.X))
			{
				if (dir.Y < 0f)
				{
					return 0;
				}
				return 2;
			}
		}
		return base.value;
	}

	protected override int interpolate(int startValue, int endValue, float factor)
	{
		if (this.Position != null && this.Position.IsInterpolating() && !this.Position.IsPausePending())
		{
			Vector2 dir = this.Position.CurrentInterpolationDirection();
			if (Math.Abs(dir.X) > Math.Abs(dir.Y))
			{
				if (dir.X < 0f)
				{
					return 3;
				}
				return 1;
			}
			if (Math.Abs(dir.Y) > Math.Abs(dir.X))
			{
				if (dir.Y < 0f)
				{
					return 0;
				}
				return 2;
			}
		}
		return startValue;
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
