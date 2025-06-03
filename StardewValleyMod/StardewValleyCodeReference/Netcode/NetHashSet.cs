using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Netcode;

public abstract class NetHashSet<TValue> : AbstractNetSerializable, IEquatable<NetHashSet<TValue>>, ISet<TValue>, ICollection<TValue>, IEnumerable<TValue>, IEnumerable
{
	public class IncomingChange
	{
		public uint Tick;

		public bool Removal;

		public TValue Value;

		public IncomingChange(uint tick, bool removal, TValue value)
		{
			this.Tick = tick;
			this.Removal = removal;
			this.Value = value;
		}
	}

	public class OutgoingChange
	{
		public bool Removal;

		public TValue Value;

		public OutgoingChange(bool removal, TValue value)
		{
			this.Removal = removal;
			this.Value = value;
		}
	}

	public delegate void ContentsChangeEvent(TValue value);

	public bool InterpolationWait = true;

	private readonly HashSet<TValue> Set = new HashSet<TValue>();

	private readonly List<IncomingChange> IncomingChanges = new List<IncomingChange>();

	private readonly List<OutgoingChange> OutgoingChanges = new List<OutgoingChange>();

	/// <inheritdoc />
	public int Count => this.Set.Count;

	/// <inheritdoc />
	public bool IsReadOnly => false;

	public event ContentsChangeEvent OnValueAdded;

	public event ContentsChangeEvent OnValueRemoved;

	public NetHashSet()
	{
	}

	public NetHashSet(IEnumerable<TValue> values)
		: this()
	{
		foreach (TValue value in values)
		{
			this.Add(value);
		}
	}

	public bool Add(TValue item)
	{
		if (!this.Set.Add(item))
		{
			return false;
		}
		this.OutgoingChanges.Add(new OutgoingChange(removal: false, item));
		base.MarkDirty();
		this.addedEvent(item);
		return true;
	}

	/// <inheritdoc />
	public void Clear()
	{
		TValue[] array = this.Set.ToArray();
		foreach (TValue entry in array)
		{
			this.Remove(entry);
		}
		this.OutgoingChanges.RemoveAll((OutgoingChange ch) => !ch.Removal);
	}

	/// <inheritdoc />
	public bool Contains(TValue item)
	{
		return this.Set.Contains(item);
	}

	/// <inheritdoc />
	public void CopyTo(TValue[] array, int arrayIndex)
	{
		this.Set.CopyTo(array, arrayIndex);
	}

	/// <inheritdoc />
	public bool Equals(NetHashSet<TValue> other)
	{
		return this.Set.Equals(other?.Set);
	}

	/// <inheritdoc />
	public void ExceptWith(IEnumerable<TValue> other)
	{
		this.Set.ExceptWith(other);
	}

	/// <inheritdoc />
	public IEnumerator<TValue> GetEnumerator()
	{
		return this.Set.GetEnumerator();
	}

	/// <inheritdoc />
	public void IntersectWith(IEnumerable<TValue> other)
	{
		this.Set.IntersectWith(other);
	}

	/// <inheritdoc />
	public bool IsProperSubsetOf(IEnumerable<TValue> other)
	{
		return this.Set.IsProperSubsetOf(other);
	}

	/// <inheritdoc />
	public bool IsProperSupersetOf(IEnumerable<TValue> other)
	{
		return this.Set.IsProperSupersetOf(other);
	}

	/// <inheritdoc />
	public bool IsSubsetOf(IEnumerable<TValue> other)
	{
		return this.Set.IsSubsetOf(other);
	}

	/// <inheritdoc />
	public bool IsSupersetOf(IEnumerable<TValue> other)
	{
		return this.Set.IsSupersetOf(other);
	}

	/// <inheritdoc />
	public bool Overlaps(IEnumerable<TValue> other)
	{
		return this.Set.Overlaps(other);
	}

	/// <inheritdoc />
	public bool Remove(TValue item)
	{
		if (!this.Set.Remove(item))
		{
			return false;
		}
		this.OutgoingChanges.Add(new OutgoingChange(removal: true, item));
		base.MarkDirty();
		this.removedEvent(item);
		return true;
	}

	/// <summary>Remove all elements that match a condition.</summary>
	/// <param name="match">The predicate matching values to remove.</param>
	/// <returns>Returns the number of values removed from the set.</returns>
	public int RemoveWhere(Predicate<TValue> match)
	{
		int num = this.Set.RemoveWhere(delegate(TValue value)
		{
			if (match(value))
			{
				this.OutgoingChanges.Add(new OutgoingChange(removal: true, value));
				this.removedEvent(value);
				return true;
			}
			return false;
		});
		if (num > 0)
		{
			base.MarkDirty();
		}
		return num;
	}

	/// <inheritdoc />
	public bool SetEquals(IEnumerable<TValue> other)
	{
		return this.Set.SetEquals(other);
	}

	/// <inheritdoc />
	public void SymmetricExceptWith(IEnumerable<TValue> other)
	{
		this.Set.SymmetricExceptWith(other);
	}

	/// <inheritdoc />
	public void UnionWith(IEnumerable<TValue> other)
	{
		this.Set.UnionWith(other);
	}

	/// <inheritdoc />
	void ICollection<TValue>.Add(TValue item)
	{
		this.Add(item);
	}

	/// <inheritdoc />
	IEnumerator IEnumerable.GetEnumerator()
	{
		return this.Set.GetEnumerator();
	}

	protected override bool tickImpl()
	{
		List<IncomingChange> triggeredChanges = null;
		foreach (IncomingChange ch in this.IncomingChanges)
		{
			if (base.Root == null || base.GetLocalTick() >= ch.Tick)
			{
				if (triggeredChanges == null)
				{
					triggeredChanges = new List<IncomingChange>();
				}
				triggeredChanges.Add(ch);
				continue;
			}
			break;
		}
		if (triggeredChanges != null)
		{
			foreach (IncomingChange ch2 in triggeredChanges)
			{
				this.IncomingChanges.Remove(ch2);
			}
			foreach (IncomingChange ch3 in triggeredChanges)
			{
				if (ch3.Removal)
				{
					if (this.Set.Remove(ch3.Value))
					{
						this.removedEvent(ch3.Value);
					}
				}
				else if (this.Set.Add(ch3.Value))
				{
					this.addedEvent(ch3.Value);
				}
			}
		}
		return this.IncomingChanges.Count > 0;
	}

	private void removedEvent(TValue value)
	{
		this.OnValueRemoved?.Invoke(value);
	}

	private void addedEvent(TValue value)
	{
		this.OnValueAdded?.Invoke(value);
	}

	/// <inheritdoc />
	public override bool Equals(object obj)
	{
		if (obj is NetHashSet<TValue> other)
		{
			return this.Equals(other);
		}
		return false;
	}

	/// <inheritdoc />
	public override void Read(BinaryReader reader, NetVersion version)
	{
		uint tick = base.GetLocalTick() + (uint)((this.InterpolationWait && base.Root != null) ? base.Root.Clock.InterpolationTicks : 0);
		uint count = reader.Read7BitEncoded();
		for (uint i = 0u; i < count; i++)
		{
			bool removal = reader.ReadBoolean();
			TValue value = this.ReadValue(reader);
			this.IncomingChanges.Add(new IncomingChange(tick, removal, value));
			base.NeedsTick = true;
		}
	}

	/// <inheritdoc />
	public override void Write(BinaryWriter writer)
	{
		writer.Write7BitEncoded((uint)this.OutgoingChanges.Count);
		foreach (OutgoingChange ch in this.OutgoingChanges)
		{
			writer.Write(ch.Removal);
			this.WriteValue(writer, ch.Value);
		}
	}

	/// <inheritdoc />
	public override void ReadFull(BinaryReader reader, NetVersion version)
	{
		this.Set.Clear();
		int count = reader.ReadInt32();
		this.Set.EnsureCapacity(count);
		for (int i = 0; i < count; i++)
		{
			TValue value = this.ReadValue(reader);
			this.Set.Add(value);
			this.addedEvent(value);
		}
	}

	/// <inheritdoc />
	public override void WriteFull(BinaryWriter writer)
	{
		writer.Write(this.Set.Count);
		foreach (TValue value in this.Set)
		{
			this.WriteValue(writer, value);
		}
	}

	public override int GetHashCode()
	{
		return this.Set.GetHashCode();
	}

	public abstract TValue ReadValue(BinaryReader reader);

	public abstract void WriteValue(BinaryWriter writer, TValue value);

	protected override void CleanImpl()
	{
		base.CleanImpl();
		this.OutgoingChanges.Clear();
	}
}
