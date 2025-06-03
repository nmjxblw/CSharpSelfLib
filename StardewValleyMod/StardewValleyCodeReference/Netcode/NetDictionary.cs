using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Netcode;

public abstract class NetDictionary<TKey, TValue, TField, TSerialDict, TSelf> : AbstractNetSerializable, IEquatable<TSelf>, IEnumerable<TSerialDict>, IEnumerable where TField : class, INetObject<INetSerializable>, new() where TSerialDict : IDictionary<TKey, TValue>, new() where TSelf : NetDictionary<TKey, TValue, TField, TSerialDict, TSelf>
{
	public class IncomingChange
	{
		public uint Tick;

		public bool Removal;

		public TKey Key;

		public TField Field;

		public NetVersion Reassigned;

		public IncomingChange(uint tick, bool removal, TKey key, TField field, NetVersion reassigned)
		{
			this.Tick = tick;
			this.Removal = removal;
			this.Key = key;
			this.Field = field;
			this.Reassigned = reassigned;
		}
	}

	public class OutgoingChange
	{
		public bool Removal;

		public TKey Key;

		public TField Field;

		public NetVersion Reassigned;

		public OutgoingChange(bool removal, TKey key, TField field, NetVersion reassigned)
		{
			this.Removal = removal;
			this.Key = key;
			this.Field = field;
			this.Reassigned = reassigned;
		}
	}

	public delegate void ContentsChangeEvent(TKey key, TValue value);

	public delegate void ConflictResolveEvent(TKey key, TField rejected, TField accepted);

	public delegate void ContentsUpdateEvent(TKey key, TValue old_target_value, TValue new_target_value);

	private delegate void ReadFunc(BinaryReader reader, NetVersion version);

	private delegate void WriteFunc<T>(BinaryWriter writer, T value);

	public struct PairsCollection : IEnumerable<KeyValuePair<TKey, TValue>>, IEnumerable
	{
		public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>, IEnumerator, IDisposable
		{
			private readonly NetDictionary<TKey, TValue, TField, TSerialDict, TSelf> _net;

			private Dictionary<TKey, TField>.Enumerator _enumerator;

			private KeyValuePair<TKey, TValue> _current;

			private bool _done;

			public KeyValuePair<TKey, TValue> Current => this._current;

			object IEnumerator.Current
			{
				get
				{
					if (this._done)
					{
						throw new InvalidOperationException();
					}
					return this._current;
				}
			}

			public Enumerator(NetDictionary<TKey, TValue, TField, TSerialDict, TSelf> net)
			{
				this._net = net;
				this._enumerator = this._net.dict.GetEnumerator();
				this._current = default(KeyValuePair<TKey, TValue>);
				this._done = false;
			}

			public bool MoveNext()
			{
				if (this._enumerator.MoveNext())
				{
					KeyValuePair<TKey, TField> pair = this._enumerator.Current;
					this._current = new KeyValuePair<TKey, TValue>(pair.Key, this._net.getFieldValue(pair.Value));
					return true;
				}
				this._done = true;
				this._current = default(KeyValuePair<TKey, TValue>);
				return false;
			}

			public void Dispose()
			{
			}

			void IEnumerator.Reset()
			{
				this._enumerator = this._net.dict.GetEnumerator();
				this._current = default(KeyValuePair<TKey, TValue>);
				this._done = false;
			}
		}

		private NetDictionary<TKey, TValue, TField, TSerialDict, TSelf> _net;

		public PairsCollection(NetDictionary<TKey, TValue, TField, TSerialDict, TSelf> net)
		{
			this._net = net;
		}

		public int Count()
		{
			return this._net.dict.Count;
		}

		public KeyValuePair<TKey, TValue> ElementAt(int index)
		{
			int count = 0;
			foreach (KeyValuePair<TKey, TValue> pair in this)
			{
				if (count == index)
				{
					return pair;
				}
				count++;
			}
			throw new ArgumentOutOfRangeException();
		}

		public Enumerator GetEnumerator()
		{
			return new Enumerator(this._net);
		}

		IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
		{
			return new Enumerator(this._net);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return new Enumerator(this._net);
		}
	}

	public struct ValuesCollection : IEnumerable<TValue>, IEnumerable
	{
		public struct Enumerator : IEnumerator<TValue>, IEnumerator, IDisposable
		{
			private readonly NetDictionary<TKey, TValue, TField, TSerialDict, TSelf> _net;

			private Dictionary<TKey, TField>.Enumerator _enumerator;

			private TValue _current;

			private bool _done;

			public TValue Current => this._current;

			object IEnumerator.Current
			{
				get
				{
					if (this._done)
					{
						throw new InvalidOperationException();
					}
					return this._current;
				}
			}

			public Enumerator(NetDictionary<TKey, TValue, TField, TSerialDict, TSelf> net)
			{
				this._net = net;
				this._enumerator = this._net.dict.GetEnumerator();
				this._current = default(TValue);
				this._done = false;
			}

			public bool MoveNext()
			{
				if (this._enumerator.MoveNext())
				{
					KeyValuePair<TKey, TField> pair = this._enumerator.Current;
					this._current = this._net.getFieldValue(pair.Value);
					return true;
				}
				this._done = true;
				this._current = default(TValue);
				return false;
			}

			public void Dispose()
			{
			}

			void IEnumerator.Reset()
			{
				this._enumerator = this._net.dict.GetEnumerator();
				this._current = default(TValue);
				this._done = false;
			}
		}

		private NetDictionary<TKey, TValue, TField, TSerialDict, TSelf> _net;

		public ValuesCollection(NetDictionary<TKey, TValue, TField, TSerialDict, TSelf> net)
		{
			this._net = net;
		}

		public Enumerator GetEnumerator()
		{
			return new Enumerator(this._net);
		}

		IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator()
		{
			return new Enumerator(this._net);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return new Enumerator(this._net);
		}
	}

	public struct KeysCollection : IEnumerable<TKey>, IEnumerable
	{
		public struct Enumerator : IEnumerator<TKey>, IEnumerator, IDisposable
		{
			private readonly Dictionary<TKey, TField> _dict;

			private Dictionary<TKey, TField>.Enumerator _enumerator;

			private TKey _current;

			private bool _done;

			public TKey Current => this._current;

			object IEnumerator.Current
			{
				get
				{
					if (this._done)
					{
						throw new InvalidOperationException();
					}
					return this._current;
				}
			}

			public Enumerator(Dictionary<TKey, TField> dict)
			{
				this._dict = dict;
				this._enumerator = this._dict.GetEnumerator();
				this._current = default(TKey);
				this._done = false;
			}

			public bool MoveNext()
			{
				if (this._enumerator.MoveNext())
				{
					this._current = this._enumerator.Current.Key;
					return true;
				}
				this._done = true;
				this._current = default(TKey);
				return false;
			}

			public void Dispose()
			{
			}

			void IEnumerator.Reset()
			{
				this._enumerator = this._dict.GetEnumerator();
				this._current = default(TKey);
				this._done = false;
			}
		}

		private Dictionary<TKey, TField> _dict;

		public KeysCollection(Dictionary<TKey, TField> dict)
		{
			this._dict = dict;
		}

		public bool Any()
		{
			return this._dict.Count > 0;
		}

		public TKey First()
		{
			using (Dictionary<TKey, TField>.Enumerator enumerator = this._dict.GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					return enumerator.Current.Key;
				}
			}
			return default(TKey);
		}

		public bool Contains(TKey key)
		{
			return this._dict.ContainsKey(key);
		}

		public Enumerator GetEnumerator()
		{
			return new Enumerator(this._dict);
		}

		IEnumerator<TKey> IEnumerable<TKey>.GetEnumerator()
		{
			return new Enumerator(this._dict);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return new Enumerator(this._dict);
		}
	}

	public bool InterpolationWait = true;

	private Dictionary<TKey, TField> dict = new Dictionary<TKey, TField>();

	private Dictionary<TKey, NetVersion> dictReassigns = new Dictionary<TKey, NetVersion>();

	private List<OutgoingChange> outgoingChanges = new List<OutgoingChange>();

	private List<IncomingChange> incomingChanges = new List<IncomingChange>();

	/// <summary>The number of key/value pairs in the dictionary.</summary>
	public int Length => this.dict.Count;

	public bool IsReadOnly => false;

	public TValue this[TKey key]
	{
		get
		{
			return this.getFieldValue(this.dict[key]);
		}
		set
		{
			if (!this.dict.TryGetValue(key, out var field))
			{
				field = (this.dict[key] = new TField());
				this.dictReassigns[key] = base.GetLocalVersion();
				this.setFieldValue(field, key, value);
				this.added(key, field, this.dictReassigns[key]);
			}
			else
			{
				this.setFieldValue(field, key, value);
				this.addedEvent(key, field);
			}
		}
	}

	public KeysCollection Keys => new KeysCollection(this.dict);

	public ValuesCollection Values => new ValuesCollection(this);

	public PairsCollection Pairs => new PairsCollection(this);

	public Dictionary<TKey, TField> FieldDict => this.dict;

	public event ContentsChangeEvent OnValueAdded;

	public event ContentsChangeEvent OnValueRemoved;

	public event ContentsUpdateEvent OnValueTargetUpdated;

	public event ConflictResolveEvent OnConflictResolve;

	public bool Any()
	{
		return this.dict.Count > 0;
	}

	public NetDictionary()
	{
	}

	public NetDictionary(IEnumerable<KeyValuePair<TKey, TValue>> dict)
		: this()
	{
		this.CopyFrom(dict);
	}

	protected override bool tickImpl()
	{
		List<IncomingChange> triggeredChanges = null;
		foreach (IncomingChange ch in this.incomingChanges)
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
		if (triggeredChanges != null && triggeredChanges.Count > 0)
		{
			foreach (IncomingChange c in triggeredChanges)
			{
				this.incomingChanges.Remove(c);
			}
			foreach (IncomingChange ch2 in triggeredChanges)
			{
				if (ch2.Removal)
				{
					this.performIncomingRemove(ch2);
				}
				else
				{
					this.performIncomingAdd(ch2);
				}
			}
		}
		return this.incomingChanges.Count > 0;
	}

	protected abstract void setFieldValue(TField field, TKey key, TValue value);

	protected abstract TValue getFieldValue(TField field);

	protected abstract TValue getFieldTargetValue(TField field);

	protected TField createField(TKey key, TValue value)
	{
		TField field = new TField();
		this.setFieldValue(field, key, value);
		return field;
	}

	public void CopyFrom(IEnumerable<KeyValuePair<TKey, TValue>> dict)
	{
		foreach (KeyValuePair<TKey, TValue> pair in dict)
		{
			this[pair.Key] = pair.Value;
		}
	}

	public void Set(IEnumerable<KeyValuePair<TKey, TValue>> dict)
	{
		this.Clear();
		this.CopyFrom(dict);
	}

	public void MoveFrom(TSelf dict)
	{
		List<KeyValuePair<TKey, TValue>> pairs = new List<KeyValuePair<TKey, TValue>>(dict.Pairs);
		dict.Clear();
		this.Set(pairs);
	}

	public void SetEqualityComparer(IEqualityComparer<TKey> comparer)
	{
		this.dict = new Dictionary<TKey, TField>(this.dict, comparer);
		this.dictReassigns = new Dictionary<TKey, NetVersion>(this.dictReassigns, comparer);
	}

	private void setFieldParent(TField arg)
	{
		if (base.Parent != null)
		{
			arg.NetFields.Parent = this;
		}
	}

	private void added(TKey key, TField field, NetVersion reassign)
	{
		this.outgoingChanges.Add(new OutgoingChange(removal: false, key, field, reassign));
		this.setFieldParent(field);
		base.MarkDirty();
		this.addedEvent(key, field);
		foreach (IncomingChange change in this.incomingChanges)
		{
			if (!change.Removal && object.Equals(change.Key, key))
			{
				this.clearFieldParent(change.Field);
				if (this.OnConflictResolve != null)
				{
					this.OnConflictResolve(key, change.Field, field);
				}
			}
		}
		this.incomingChanges.RemoveAll((IncomingChange incomingChange) => object.Equals(key, incomingChange.Key));
	}

	private void addedEvent(TKey key, TField field)
	{
		if (this.OnValueAdded != null)
		{
			this.OnValueAdded(key, this.getFieldValue(field));
		}
	}

	private void updatedEvent(TKey key, TValue old_target_value, TValue new_target_value)
	{
		if (this.OnValueTargetUpdated != null)
		{
			this.OnValueTargetUpdated(key, old_target_value, new_target_value);
		}
	}

	private void clearFieldParent(TField arg)
	{
		if (arg.NetFields.Parent == this)
		{
			arg.NetFields.Parent = null;
		}
	}

	private void removed(TKey key, TField field, NetVersion reassign)
	{
		this.outgoingChanges.Add(new OutgoingChange(removal: true, key, field, reassign));
		this.clearFieldParent(field);
		base.MarkDirty();
		this.removedEvent(key, field);
	}

	private void removedEvent(TKey key, TField field)
	{
		if (this.OnValueRemoved != null)
		{
			this.OnValueRemoved(key, this.getFieldValue(field));
		}
	}

	/// <summary>Add an entry to the dictionary.</summary>
	/// <param name="key">The key of the element to add.</param>
	/// <param name="value">The value of the element to add.</param>
	/// <exception cref="T:System.ArgumentException">The key is already present in the dictionary.</exception>
	public void Add(TKey key, TValue value)
	{
		TField field = this.createField(key, value);
		this.Add(key, field);
	}

	/// <summary>Add an entry to the dictionary.</summary>
	/// <param name="key">The key of the element to add.</param>
	/// <param name="field">The net field to add.</param>
	/// <exception cref="T:System.ArgumentException">The key is already present in the dictionary.</exception>
	public void Add(TKey key, TField field)
	{
		this.dict.Add(key, field);
		this.dictReassigns.Add(key, base.GetLocalVersion());
		this.added(key, field, this.dictReassigns[key]);
	}

	/// <summary>Add an entry to the dictionary if the key isn't already present.</summary>
	/// <param name="key">The key of the element to add.</param>
	/// <param name="value">The value of the element to add.</param>
	/// <returns>Returns whether the value was successfully added.</returns>
	public bool TryAdd(TKey key, TValue value)
	{
		if (this.dict.ContainsKey(key))
		{
			return false;
		}
		TField field = this.createField(key, value);
		this.Add(key, field);
		return true;
	}

	public void Clear()
	{
		KeysCollection keys = this.Keys;
		while (keys.Any())
		{
			this.Remove(keys.First());
		}
		this.outgoingChanges.RemoveAll((OutgoingChange ch) => !ch.Removal);
	}

	public bool ContainsKey(TKey key)
	{
		return this.dict.ContainsKey(key);
	}

	public int Count()
	{
		return this.dict.Count;
	}

	public bool Remove(TKey key)
	{
		if (this.dict.TryGetValue(key, out var field))
		{
			NetVersion reassign = this.dictReassigns[key];
			this.dict.Remove(key);
			this.dictReassigns.Remove(key);
			this.removed(key, field, reassign);
			return true;
		}
		return false;
	}

	/// <summary>Remove all elements that match a condition.</summary>
	/// <param name="match">The predicate matching values to remove.</param>
	public int RemoveWhere(Func<KeyValuePair<TKey, TValue>, bool> match)
	{
		if (this.dict.Count == 0)
		{
			return 0;
		}
		int removed = 0;
		foreach (KeyValuePair<TKey, TValue> pair in this.Pairs)
		{
			if (match(pair))
			{
				this.Remove(pair.Key);
				removed++;
			}
		}
		return removed;
	}

	[Obsolete("Use RemoveWhere instead.")]
	public void Filter(Func<KeyValuePair<TKey, TValue>, bool> f)
	{
		this.RemoveWhere((KeyValuePair<TKey, TValue> pair) => !f(pair));
	}

	/// <summary>Try to get the value associated with a specified key.</summary>
	/// <param name="key">The key of the element to find.</param>
	/// <param name="value">The value that was found, or the default value if none was found.</param>
	/// <returns>Returns whether a value was found.</returns>
	public bool TryGetValue(TKey key, out TValue value)
	{
		if (this.dict.TryGetValue(key, out var field))
		{
			value = this.getFieldValue(field);
			return true;
		}
		value = default(TValue);
		return false;
	}

	/// <summary>Get the value associated with a specified key, or the default value if none was found.</summary>
	/// <param name="key">The key of the element to find.</param>
	/// <param name="defaultValue">The value to return if the element isn't found.</param>
	public TValue GetValueOrDefault(TKey key, TValue defaultValue = default(TValue))
	{
		if (!this.dict.TryGetValue(key, out var field))
		{
			return defaultValue;
		}
		return this.getFieldValue(field);
	}

	public bool Equals(TSelf other)
	{
		return object.Equals(this.dict, other.dict);
	}

	protected override void CleanImpl()
	{
		base.CleanImpl();
		this.outgoingChanges.Clear();
	}

	protected abstract TKey ReadKey(BinaryReader reader);

	protected abstract void WriteKey(BinaryWriter writer, TKey key);

	private void readMultiple(ReadFunc readFunc, BinaryReader reader, NetVersion version)
	{
		uint count = reader.Read7BitEncoded();
		for (uint i = 0u; i < count; i++)
		{
			readFunc(reader, version);
		}
	}

	private void writeMultiple<T>(WriteFunc<T> writeFunc, BinaryWriter writer, IEnumerable<T> values)
	{
		writer.Write7BitEncoded((uint)values.Count());
		foreach (T value in values)
		{
			writeFunc(writer, value);
		}
	}

	protected virtual TField ReadFieldFull(BinaryReader reader, NetVersion version)
	{
		TField val = new TField();
		val.NetFields.ReadFull(reader, version);
		return val;
	}

	protected virtual void WriteFieldFull(BinaryWriter writer, TField field)
	{
		field.NetFields.WriteFull(writer);
	}

	private void readAddition(BinaryReader reader, NetVersion version)
	{
		TKey key = this.ReadKey(reader);
		NetVersion reassign = default(NetVersion);
		reassign.Read(reader);
		TField field = this.ReadFieldFull(reader, version);
		this.setFieldParent(field);
		this.queueIncomingChange(removal: false, key, field, reassign);
	}

	protected virtual bool resolveConflict(TKey key, TField currentField, NetVersion currentReassign, TField incomingField, NetVersion incomingReassign)
	{
		if (incomingReassign.IsPriorityOver(currentReassign))
		{
			this.clearFieldParent(currentField);
			if (this.OnConflictResolve != null)
			{
				this.OnConflictResolve(key, currentField, incomingField);
			}
			return true;
		}
		this.clearFieldParent(incomingField);
		if (this.OnConflictResolve != null)
		{
			this.OnConflictResolve(key, incomingField, currentField);
		}
		return false;
	}

	private KeyValuePair<NetVersion, TField>? findConflict(TKey key)
	{
		foreach (IncomingChange change in this.incomingChanges.AsEnumerable().Reverse())
		{
			if (object.Equals(change.Key, key))
			{
				if (change.Removal)
				{
					return null;
				}
				return new KeyValuePair<NetVersion, TField>(change.Reassigned, change.Field);
			}
		}
		if (this.dict.TryGetValue(key, out var value))
		{
			return new KeyValuePair<NetVersion, TField>(this.dictReassigns[key], value);
		}
		return null;
	}

	private void queueIncomingChange(bool removal, TKey key, TField field, NetVersion fieldReassign)
	{
		if (!removal)
		{
			KeyValuePair<NetVersion, TField>? conflict = this.findConflict(key);
			if (conflict.HasValue && !this.resolveConflict(key, conflict.Value.Value, conflict.Value.Key, field, fieldReassign))
			{
				return;
			}
		}
		uint timestamp = base.GetLocalTick() + (uint)((this.InterpolationWait && base.Root != null) ? base.Root.Clock.InterpolationTicks : 0);
		this.incomingChanges.Add(new IncomingChange(timestamp, removal, key, field, fieldReassign));
		base.NeedsTick = true;
	}

	private void performIncomingAdd(IncomingChange add)
	{
		this.dict[add.Key] = add.Field;
		this.dictReassigns[add.Key] = add.Reassigned;
		this.addedEvent(add.Key, add.Field);
	}

	private void readRemoval(BinaryReader reader, NetVersion version)
	{
		TKey key = this.ReadKey(reader);
		NetVersion reassign = default(NetVersion);
		reassign.Read(reader);
		this.queueIncomingChange(removal: true, key, null, reassign);
	}

	private void readDictChange(BinaryReader reader, NetVersion version)
	{
		if (reader.ReadByte() != 0)
		{
			this.readRemoval(reader, version);
		}
		else
		{
			this.readAddition(reader, version);
		}
	}

	private void performIncomingRemove(IncomingChange remove)
	{
		if (this.dict.TryGetValue(remove.Key, out var field))
		{
			this.clearFieldParent(field);
			this.dict.Remove(remove.Key);
			this.dictReassigns.Remove(remove.Key);
			this.removedEvent(remove.Key, field);
		}
	}

	private void readUpdate(BinaryReader reader, NetVersion version)
	{
		TKey key = this.ReadKey(reader);
		NetVersion reassign = default(NetVersion);
		reassign.Read(reader);
		reader.ReadSkippable(delegate
		{
			int num = this.incomingChanges.FindLastIndex((IncomingChange ch) => !ch.Removal && object.Equals(ch.Key, key) && reassign.Equals(ch.Reassigned));
			TField value;
			if (num != -1)
			{
				TField field = this.incomingChanges[num].Field;
				if (this.OnValueTargetUpdated != null)
				{
					TValue fieldTargetValue = this.getFieldTargetValue(field);
					field.NetFields.Read(reader, version);
					this.updatedEvent(key, fieldTargetValue, this.getFieldTargetValue(field));
				}
				else
				{
					field.NetFields.Read(reader, version);
				}
			}
			else if (this.dict.TryGetValue(key, out value) && this.dictReassigns[key].Equals(reassign))
			{
				if (this.OnValueTargetUpdated != null)
				{
					TValue fieldTargetValue2 = this.getFieldTargetValue(value);
					value.NetFields.Read(reader, version);
					this.updatedEvent(key, fieldTargetValue2, this.getFieldTargetValue(value));
				}
				else
				{
					value.NetFields.Read(reader, version);
				}
			}
		});
	}

	public override void Read(BinaryReader reader, NetVersion version)
	{
		this.readMultiple(readDictChange, reader, version);
		this.readMultiple(readUpdate, reader, version);
	}

	public override void ReadFull(BinaryReader reader, NetVersion version)
	{
		this.dict.Clear();
		this.dictReassigns.Clear();
		this.outgoingChanges.Clear();
		this.incomingChanges.Clear();
		int count = reader.ReadInt32();
		for (int i = 0; i < count; i++)
		{
			TKey key = this.ReadKey(reader);
			NetVersion reassign = default(NetVersion);
			reassign.Read(reader);
			TField field = this.ReadFieldFull(reader, version);
			this.dict.Add(key, field);
			this.dictReassigns.Add(key, reassign);
			this.setFieldParent(field);
			this.addedEvent(key, field);
		}
	}

	private void writeAddition(BinaryWriter writer, OutgoingChange update)
	{
		this.WriteKey(writer, update.Key);
		update.Reassigned.Write(writer);
		this.WriteFieldFull(writer, update.Field);
	}

	private void writeRemoval(BinaryWriter writer, OutgoingChange update)
	{
		this.WriteKey(writer, update.Key);
		update.Reassigned.Write(writer);
	}

	private void writeDictChange(BinaryWriter writer, OutgoingChange ch)
	{
		if (ch.Removal)
		{
			writer.Write((byte)1);
			this.writeRemoval(writer, ch);
		}
		else
		{
			writer.Write((byte)0);
			this.writeAddition(writer, ch);
		}
	}

	private void writeUpdate(BinaryWriter writer, OutgoingChange update)
	{
		this.WriteKey(writer, update.Key);
		update.Reassigned.Write(writer);
		writer.WriteSkippable(delegate
		{
			update.Field.NetFields.Write(writer);
		});
	}

	private IEnumerable<OutgoingChange> updates()
	{
		foreach (KeyValuePair<TKey, TField> pair in this.dict)
		{
			if (pair.Value.NetFields.Dirty)
			{
				yield return new OutgoingChange(removal: false, pair.Key, pair.Value, this.dictReassigns[pair.Key]);
			}
		}
		foreach (OutgoingChange removal in this.outgoingChanges.Where((OutgoingChange ch) => ch.Removal))
		{
			if (removal.Field.NetFields.Dirty)
			{
				yield return removal;
			}
		}
	}

	public override void Write(BinaryWriter writer)
	{
		this.writeMultiple(writeDictChange, writer, this.outgoingChanges);
		this.writeMultiple(writeUpdate, writer, this.updates());
	}

	public override void WriteFull(BinaryWriter writer)
	{
		writer.Write(this.Length);
		foreach (TKey key in this.dict.Keys)
		{
			this.WriteKey(writer, key);
			this.dictReassigns[key].Write(writer);
			this.WriteFieldFull(writer, this.dict[key]);
		}
	}

	public IEnumerator<TSerialDict> GetEnumerator()
	{
		TSerialDict serial = new TSerialDict();
		foreach (KeyValuePair<TKey, TField> kvp in this.dict)
		{
			serial.Add(kvp.Key, this.getFieldValue(kvp.Value));
		}
		return new List<TSerialDict> { serial }.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return this.GetEnumerator();
	}

	protected override void ForEachChild(Action<INetSerializable> childAction)
	{
		foreach (IncomingChange ch in this.incomingChanges)
		{
			if (ch.Field != null)
			{
				childAction(ch.Field.NetFields);
			}
		}
		foreach (TField field in this.dict.Values)
		{
			childAction(field.NetFields);
		}
	}

	public void Add(TSerialDict dict)
	{
		this.Set(dict);
	}

	protected override void ValidateChildren()
	{
		if ((base.Parent != null || base.Root == this) && !base.NeedsTick)
		{
			this.ForEachChild(ValidateChild);
		}
	}
}
