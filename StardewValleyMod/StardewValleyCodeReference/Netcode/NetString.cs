using System.IO;

namespace Netcode;

public class NetString : NetField<string, NetString>
{
	public delegate string FilterString(string newValue);

	public int Length => base.Value.Length;

	public event FilterString FilterStringEvent;

	public NetString()
		: base((string)null)
	{
	}

	public NetString(string value)
		: base(value)
	{
	}

	public override void Set(string newValue)
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

	public bool Contains(string substr)
	{
		if (base.Value != null)
		{
			return base.Value.Contains(substr);
		}
		return false;
	}

	protected override void ReadDelta(BinaryReader reader, NetVersion version)
	{
		string newValue = null;
		if (reader.ReadBoolean())
		{
			newValue = reader.ReadString();
			if (this.FilterStringEvent != null)
			{
				newValue = this.FilterStringEvent(newValue);
			}
		}
		if (version.IsPriorityOver(base.ChangeVersion))
		{
			base.setInterpolationTarget(newValue);
		}
	}

	protected override void WriteDelta(BinaryWriter writer)
	{
		writer.Write(base.value != null);
		if (base.value != null)
		{
			writer.Write(base.value);
		}
	}
}
