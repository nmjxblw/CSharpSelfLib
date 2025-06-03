using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Netcode;

public class NetNullableEnum<T> : NetField<T?, NetNullableEnum<T>>, IEnumerable<string>, IEnumerable where T : struct, IConvertible
{
	private bool xmlInitialized;

	public NetNullableEnum()
		: base((T?)null)
	{
	}

	public NetNullableEnum(T value)
		: base((T?)value)
	{
	}

	public override void Set(T? newValue)
	{
		if (!EqualityComparer<T?>.Default.Equals(newValue, base.value))
		{
			base.cleanSet(newValue);
			base.MarkDirty();
		}
	}

	protected override void ReadDelta(BinaryReader reader, NetVersion version)
	{
		T? newValue = null;
		if (reader.ReadBoolean())
		{
			newValue = (T)Enum.ToObject(typeof(T), reader.ReadInt16());
		}
		if (version.IsPriorityOver(base.ChangeVersion))
		{
			base.setInterpolationTarget(newValue);
		}
	}

	protected override void WriteDelta(BinaryWriter writer)
	{
		if (!base.value.HasValue)
		{
			writer.Write(value: false);
			return;
		}
		writer.Write(value: true);
		writer.Write(Convert.ToInt16(base.value));
	}

	public new IEnumerator<string> GetEnumerator()
	{
		T? value = base.Get();
		if (!value.HasValue)
		{
			return Enumerable.Repeat<string>(null, 1).GetEnumerator();
		}
		return Enumerable.Repeat(Convert.ToString(value), 1).GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return this.GetEnumerator();
	}

	public void Add(string value)
	{
		if (this.xmlInitialized || base.Parent != null)
		{
			throw new InvalidOperationException(base.GetType().Name + " already has value " + this.ToString());
		}
		if (!string.IsNullOrEmpty(value))
		{
			base.cleanSet((T)Enum.Parse(typeof(T), value));
		}
		else
		{
			base.cleanSet(null);
		}
		this.xmlInitialized = true;
	}
}
