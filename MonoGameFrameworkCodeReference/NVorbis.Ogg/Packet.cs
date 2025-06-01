using System;

namespace NVorbis.Ogg;

internal class Packet : DataPacket
{
	private long _offset;

	private int _length;

	private int _curOfs;

	private Packet _mergedPacket;

	private Packet _next;

	private Packet _prev;

	private ContainerReader _containerReader;

	internal Packet Next
	{
		get
		{
			return this._next;
		}
		set
		{
			this._next = value;
		}
	}

	internal Packet Prev
	{
		get
		{
			return this._prev;
		}
		set
		{
			this._prev = value;
		}
	}

	internal bool IsContinued
	{
		get
		{
			return base.GetFlag(PacketFlags.User1);
		}
		set
		{
			base.SetFlag(PacketFlags.User1, value);
		}
	}

	internal bool IsContinuation
	{
		get
		{
			return base.GetFlag(PacketFlags.User2);
		}
		set
		{
			base.SetFlag(PacketFlags.User2, value);
		}
	}

	internal Packet(ContainerReader containerReader, long streamOffset, int length)
		: base(length)
	{
		this._containerReader = containerReader;
		this._offset = streamOffset;
		this._length = length;
		this._curOfs = 0;
	}

	internal void MergeWith(DataPacket continuation)
	{
		if (!(continuation is Packet op))
		{
			throw new ArgumentException("Incorrect packet type!");
		}
		base.Length += continuation.Length;
		if (this._mergedPacket == null)
		{
			this._mergedPacket = op;
		}
		else
		{
			this._mergedPacket.MergeWith(continuation);
		}
		base.PageGranulePosition = continuation.PageGranulePosition;
		base.PageSequenceNumber = continuation.PageSequenceNumber;
	}

	internal void Reset()
	{
		this._curOfs = 0;
		base.ResetBitReader();
		if (this._mergedPacket != null)
		{
			this._mergedPacket.Reset();
		}
	}

	protected override int ReadNextByte()
	{
		if (this._curOfs == this._length)
		{
			if (this._mergedPacket == null)
			{
				return -1;
			}
			return this._mergedPacket.ReadNextByte();
		}
		int num = this._containerReader.PacketReadByte(this._offset + this._curOfs);
		if (num != -1)
		{
			this._curOfs++;
		}
		return num;
	}

	public override void Done()
	{
		if (this._mergedPacket != null)
		{
			this._mergedPacket.Done();
		}
		else
		{
			this._containerReader.PacketDiscardThrough(this._offset + this._length);
		}
	}
}
