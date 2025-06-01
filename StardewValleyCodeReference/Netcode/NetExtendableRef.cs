using System;
using System.IO;
using StardewValley.Util;

namespace Netcode;

public class NetExtendableRef<T, TSelf> : NetRefBase<T, TSelf> where T : class, INetObject<INetSerializable> where TSelf : NetExtendableRef<T, TSelf>
{
	public NetExtendableRef()
	{
		base.notifyOnTargetValueChange = true;
	}

	public NetExtendableRef(T value)
		: this()
	{
		base.cleanSet(value);
	}

	protected override void ForEachChild(Action<INetSerializable> childAction)
	{
		if (base.targetValue != null)
		{
			childAction(base.targetValue.NetFields);
		}
	}

	protected override void ReadValueFull(T value, BinaryReader reader, NetVersion version)
	{
		value.NetFields.ReadFull(reader, version);
	}

	protected override void ReadValueDelta(BinaryReader reader, NetVersion version)
	{
		base.targetValue.NetFields.Read(reader, version);
	}

	private void clearValueParent(T targetValue)
	{
		if (targetValue.NetFields.Parent == this)
		{
			targetValue.NetFields.Parent = null;
		}
	}

	private void setValueParent(T targetValue)
	{
		if (targetValue?.NetFields == null)
		{
			NetHelper.LogWarning((targetValue == null) ? ("Can't change net field parent for null target to '" + base.Name + ".") : $"Can't change net field parent for {targetValue.GetType().FullName} type's null {"NetFields"} to '{base.Name}'.");
			NetHelper.LogVerbose(new StackTraceHelper().ToString());
			return;
		}
		if (base.Parent != null || base.Root == this)
		{
			if (targetValue.NetFields.Parent != null && targetValue.NetFields.Parent != this)
			{
				NetHelper.LogWarning($"Changing net field parent for '{targetValue.NetFields.Name}' collection from '{targetValue.NetFields.Parent.Name}' to '{base.Name}'.");
				NetHelper.LogVerbose(new StackTraceHelper().ToString());
			}
			targetValue.NetFields.Parent = this;
		}
		targetValue.NetFields.MarkClean();
	}

	protected override void targetValueChanged(T oldValue, T newValue)
	{
		base.targetValueChanged(oldValue, newValue);
		if (oldValue != null)
		{
			this.clearValueParent(oldValue);
		}
		if (newValue != null)
		{
			this.setValueParent(newValue);
		}
	}

	protected override void WriteValueFull(BinaryWriter writer)
	{
		base.targetValue.NetFields.WriteFull(writer);
	}

	protected override void WriteValueDelta(BinaryWriter writer)
	{
		base.targetValue.NetFields.Write(writer);
	}
}
