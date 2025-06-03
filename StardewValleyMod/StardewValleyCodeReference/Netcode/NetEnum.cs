using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Netcode;

public class NetEnum<T> : NetFieldBase<T, NetEnum<T>>, IEnumerable<string>, IEnumerable where T : struct, IConvertible
{
	private bool xmlInitialized;

	public NetEnum()
	{
	}

	public NetEnum(T value)
		: base(value)
	{
	}

	public override void Set(T newValue)
	{
		if (!EqualityComparer<T>.Default.Equals(newValue, base.value))
		{
			base.cleanSet(newValue);
			base.MarkDirty();
		}
	}

	protected override void ReadDelta(BinaryReader reader, NetVersion version)
	{
		T newValue = (T)Enum.ToObject(typeof(T), reader.ReadInt16());
		if (version.IsPriorityOver(base.ChangeVersion))
		{
			base.setInterpolationTarget(newValue);
		}
	}

	protected override void WriteDelta(BinaryWriter writer)
	{
		writer.Write(Convert.ToInt16(base.value));
	}

	public IEnumerator<string> GetEnumerator()
	{
		return Enumerable.Repeat(Convert.ToString(base.Get()), 1).GetEnumerator();
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
		base.cleanSet((T)Enum.Parse(typeof(T), value));
		this.xmlInitialized = true;
	}
}
