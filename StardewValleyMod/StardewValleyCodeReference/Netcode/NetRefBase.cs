using System;
using System.IO;
using System.Xml.Serialization;
using StardewValley.SaveSerialization;

namespace Netcode;

public abstract class NetRefBase<T, TSelf> : NetField<T, TSelf> where T : class where TSelf : NetRefBase<T, TSelf>
{
	private enum RefDeltaType : byte
	{
		ChildDelta,
		Reassigned
	}

	public delegate void ConflictResolveEvent(T rejected, T accepted);

	public XmlSerializer Serializer;

	private RefDeltaType deltaType;

	protected NetVersion reassigned;

	public event ConflictResolveEvent OnConflictResolve;

	public NetRefBase()
	{
	}

	public NetRefBase(T value)
		: this()
	{
		base.cleanSet(value);
	}

	protected override void SetParent(INetSerializable parent)
	{
		if (parent == null || parent.Root != base.Root)
		{
			this.reassigned.Clear();
		}
		base.SetParent(parent);
	}

	protected override void CleanImpl()
	{
		base.CleanImpl();
		this.deltaType = RefDeltaType.ChildDelta;
	}

	public void MarkReassigned()
	{
		this.deltaType = RefDeltaType.Reassigned;
		if (base.Root != null)
		{
			this.reassigned.Set(base.Root.Clock.netVersion);
		}
		base.MarkDirty();
	}

	public override void Set(T newValue)
	{
		if (newValue != base.Value)
		{
			this.deltaType = RefDeltaType.Reassigned;
			if (base.Root != null)
			{
				this.reassigned.Set(base.Root.Clock.netVersion);
			}
			base.cleanSet(newValue);
			base.MarkDirty();
		}
	}

	private T createType(Type type)
	{
		if ((object)type == null)
		{
			return null;
		}
		if (!typeof(T).IsAssignableFrom(type))
		{
			throw new InvalidCastException($"Net ref field '{base.Name}' received invalid type '{type.FullName}', which can't be converted to expected type '{typeof(T).FullName}'.");
		}
		return (T)Activator.CreateInstance(type);
	}

	protected T ReadType(BinaryReader reader)
	{
		return this.createType(reader.ReadType());
	}

	protected void WriteType(BinaryWriter writer)
	{
		writer.WriteTypeOf(base.targetValue);
	}

	private void serialize(BinaryWriter writer, XmlSerializer serializer = null)
	{
		using MemoryStream stream = new MemoryStream();
		(serializer ?? this.Serializer).SerializeFast(stream, base.targetValue);
		stream.Seek(0L, SeekOrigin.Begin);
		writer.Write((int)stream.Length);
		writer.Write(stream.ToArray());
	}

	private T deserialize(BinaryReader reader, XmlSerializer serializer = null)
	{
		int length = reader.ReadInt32();
		using MemoryStream stream = new MemoryStream(reader.ReadBytes(length));
		return (T)(serializer ?? this.Serializer).DeserializeFast(stream);
	}

	protected abstract void ReadValueFull(T value, BinaryReader reader, NetVersion version);

	protected abstract void ReadValueDelta(BinaryReader reader, NetVersion version);

	protected abstract void WriteValueFull(BinaryWriter writer);

	protected abstract void WriteValueDelta(BinaryWriter writer);

	private void writeBaseValue(BinaryWriter writer)
	{
		if (this.Serializer != null)
		{
			this.serialize(writer);
		}
		else
		{
			this.WriteType(writer);
		}
	}

	private T readBaseValue(BinaryReader reader, NetVersion version)
	{
		if (this.Serializer != null)
		{
			return this.deserialize(reader);
		}
		return this.ReadType(reader);
	}

	protected override void ReadDelta(BinaryReader reader, NetVersion version)
	{
		if (reader.ReadByte() == 1)
		{
			reader.ReadSkippable(delegate
			{
				NetVersion other = default(NetVersion);
				other.Read(reader);
				T val = this.readBaseValue(reader, version);
				if (val != null)
				{
					this.ReadValueFull(val, reader, version);
				}
				if (other.IsIndependent(this.reassigned))
				{
					if (!other.IsPriorityOver(this.reassigned))
					{
						if (this.OnConflictResolve != null)
						{
							this.OnConflictResolve(val, base.targetValue);
						}
						return;
					}
					if (this.OnConflictResolve != null)
					{
						this.OnConflictResolve(base.targetValue, val);
					}
				}
				else if (!other.IsPriorityOver(this.reassigned))
				{
					return;
				}
				this.reassigned.Set(other);
				base.setInterpolationTarget(val);
			});
			return;
		}
		reader.ReadSkippable(delegate
		{
			if (version.IsPrecededBy(this.reassigned) && base.targetValue != null)
			{
				this.ReadValueDelta(reader, version);
			}
		});
	}

	protected override void WriteDelta(BinaryWriter writer)
	{
		writer.Push((base.targetValue != null) ? base.targetValue.GetType().Name : "null");
		writer.Write((byte)this.deltaType);
		if (this.deltaType == RefDeltaType.Reassigned)
		{
			writer.WriteSkippable(delegate
			{
				this.reassigned.Write(writer);
				this.writeBaseValue(writer);
				if (base.targetValue != null)
				{
					this.WriteValueFull(writer);
				}
			});
		}
		else
		{
			writer.WriteSkippable(delegate
			{
				if (base.targetValue != null)
				{
					this.WriteValueDelta(writer);
				}
			});
		}
		this.deltaType = RefDeltaType.ChildDelta;
		writer.Pop();
	}

	public override void ReadFull(BinaryReader reader, NetVersion version)
	{
		this.reassigned.Read(reader);
		T remoteValue = this.readBaseValue(reader, version);
		if (remoteValue != null)
		{
			this.ReadValueFull(remoteValue, reader, version);
		}
		base.cleanSet(remoteValue);
		base.ChangeVersion.Merge(version);
	}

	public override void WriteFull(BinaryWriter writer)
	{
		writer.Push((base.targetValue != null) ? base.targetValue.GetType().Name : "null");
		this.reassigned.Write(writer);
		this.writeBaseValue(writer);
		if (base.targetValue != null)
		{
			this.WriteValueFull(writer);
		}
		writer.Pop();
	}
}
