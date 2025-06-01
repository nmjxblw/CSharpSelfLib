using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace NVorbis.Ogg;

[DebuggerTypeProxy(typeof(DebugView))]
internal class PacketReader : IPacketProvider, IDisposable
{
	private class DebugView
	{
		private PacketReader _reader;

		private Packet _last;

		private Packet _first;

		private Packet[] _packetList = new Packet[0];

		public ContainerReader Container => this._reader._container;

		public int StreamSerial => this._reader._streamSerial;

		public bool EndOfStreamFound => this._reader._eosFound;

		public int CurrentPacketIndex
		{
			get
			{
				if (this._reader._current == null)
				{
					return -1;
				}
				return Array.IndexOf(this.Packets, this._reader._current);
			}
		}

		public Packet[] Packets
		{
			get
			{
				if (this._reader._last == this._last && this._reader._first == this._first)
				{
					return this._packetList;
				}
				this._last = this._reader._last;
				this._first = this._reader._first;
				List<Packet> packets = new List<Packet>();
				for (Packet node = this._first; node != null; node = node.Next)
				{
					packets.Add(node);
				}
				this._packetList = packets.ToArray();
				return this._packetList;
			}
		}

		public DebugView(PacketReader reader)
		{
			if (reader == null)
			{
				throw new ArgumentNullException("reader");
			}
			this._reader = reader;
		}
	}

	private ContainerReader _container;

	private int _streamSerial;

	private bool _eosFound;

	private Packet _first;

	private Packet _current;

	private Packet _last;

	private object _packetLock = new object();

	internal bool HasEndOfStream => this._eosFound;

	public int StreamSerial => this._streamSerial;

	public long ContainerBits { get; set; }

	public bool CanSeek => this._container.CanSeek;

	public event EventHandler<ParameterChangeEventArgs> ParameterChange;

	internal PacketReader(ContainerReader container, int streamSerial)
	{
		this._container = container;
		this._streamSerial = streamSerial;
	}

	public void Dispose()
	{
		this._eosFound = true;
		if (this._container != null)
		{
			this._container.DisposePacketReader(this);
		}
		this._container = null;
		this._current = null;
		if (this._first != null)
		{
			Packet node = this._first;
			this._first = null;
			while (node.Next != null)
			{
				Packet temp = node.Next;
				node.Next = null;
				node = temp;
				node.Prev = null;
			}
			node = null;
		}
		this._last = null;
	}

	internal void AddPacket(Packet packet)
	{
		lock (this._packetLock)
		{
			if (this._eosFound)
			{
				return;
			}
			if (packet.IsResync)
			{
				packet.IsContinuation = false;
				if (this._last != null)
				{
					this._last.IsContinued = false;
				}
			}
			if (packet.IsContinuation)
			{
				if (this._last == null)
				{
					throw new InvalidDataException();
				}
				if (!this._last.IsContinued)
				{
					throw new InvalidDataException();
				}
				this._last.MergeWith(packet);
				this._last.IsContinued = packet.IsContinued;
			}
			else
			{
				if (packet == null)
				{
					throw new ArgumentException("Wrong packet datatype", "packet");
				}
				if (this._first == null)
				{
					this._first = packet;
					this._last = packet;
				}
				else
				{
					Packet packet2 = (packet.Prev = this._last);
					Packet last2 = (packet2.Next = packet);
					this._last = last2;
				}
			}
			if (packet.IsEndOfStream)
			{
				this.SetEndOfStream();
			}
		}
	}

	internal void SetEndOfStream()
	{
		lock (this._packetLock)
		{
			this._eosFound = true;
			if (this._last.IsContinued)
			{
				this._last = this._last.Prev;
				this._last.Next.Prev = null;
				this._last.Next = null;
			}
		}
	}

	public DataPacket GetNextPacket()
	{
		return this._current = this.PeekNextPacketInternal();
	}

	public DataPacket PeekNextPacket()
	{
		return this.PeekNextPacketInternal();
	}

	private Packet PeekNextPacketInternal()
	{
		Packet curPacket;
		if (this._current == null)
		{
			curPacket = this._first;
		}
		else
		{
			while (true)
			{
				lock (this._packetLock)
				{
					curPacket = this._current.Next;
					if ((curPacket != null && !curPacket.IsContinued) || this._eosFound)
					{
						break;
					}
					goto IL_004f;
				}
				IL_004f:
				this._container.GatherNextPage(this._streamSerial);
			}
		}
		if (curPacket != null)
		{
			if (curPacket.IsContinued)
			{
				throw new InvalidDataException("Packet is incomplete!");
			}
			curPacket.Reset();
		}
		return curPacket;
	}

	internal void ReadAllPages()
	{
		if (!this.CanSeek)
		{
			throw new InvalidOperationException();
		}
		while (!this._eosFound)
		{
			this._container.GatherNextPage(this._streamSerial);
		}
	}

	internal DataPacket GetLastPacket()
	{
		this.ReadAllPages();
		return this._last;
	}

	public int GetTotalPageCount()
	{
		this.ReadAllPages();
		int cnt = 0;
		int lastPageSeqNo = 0;
		for (Packet packet = this._first; packet != null; packet = packet.Next)
		{
			if (packet.PageSequenceNumber != lastPageSeqNo)
			{
				cnt++;
				lastPageSeqNo = packet.PageSequenceNumber;
			}
		}
		return cnt;
	}

	public DataPacket GetPacket(int packetIndex)
	{
		if (!this.CanSeek)
		{
			throw new InvalidOperationException();
		}
		if (packetIndex < 0)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		if (this._first == null)
		{
			throw new InvalidOperationException("Packet reader has no packets!");
		}
		Packet packet = this._first;
		while (--packetIndex >= 0)
		{
			while (packet.Next == null)
			{
				if (this._eosFound)
				{
					throw new ArgumentOutOfRangeException("index");
				}
				this._container.GatherNextPage(this._streamSerial);
			}
			packet = packet.Next;
		}
		packet.Reset();
		return packet;
	}

	private Packet GetLastPacketInPage(Packet packet)
	{
		if (packet != null)
		{
			int pageSeqNumber = packet.PageSequenceNumber;
			while (packet.Next != null && packet.Next.PageSequenceNumber == pageSeqNumber)
			{
				packet = packet.Next;
			}
			if (packet != null && packet.IsContinued)
			{
				packet = packet.Prev;
			}
		}
		return packet;
	}

	private Packet FindPacketInPage(Packet pagePacket, long targetGranulePos, Func<DataPacket, DataPacket, int> packetGranuleCountCallback)
	{
		Packet lastPacketInPage = this.GetLastPacketInPage(pagePacket);
		if (lastPacketInPage == null)
		{
			return null;
		}
		Packet packet = lastPacketInPage;
		do
		{
			if (!packet.GranuleCount.HasValue)
			{
				if (packet == lastPacketInPage)
				{
					packet.GranulePosition = packet.PageGranulePosition;
				}
				else
				{
					packet.GranulePosition = packet.Next.GranulePosition - packet.Next.GranuleCount.Value;
				}
				if (packet == this._last && this._eosFound && packet.Prev.PageSequenceNumber < packet.PageSequenceNumber)
				{
					packet.GranuleCount = (int)(packet.GranulePosition - packet.Prev.PageGranulePosition);
				}
				else if (packet.Prev != null)
				{
					packet.Prev.Reset();
					packet.Reset();
					packet.GranuleCount = packetGranuleCountCallback(packet, packet.Prev);
				}
				else
				{
					if (packet.GranulePosition > packet.Next.GranulePosition - packet.Next.GranuleCount)
					{
						throw new InvalidOperationException("First data packet size mismatch");
					}
					packet.GranuleCount = (int)packet.GranulePosition;
				}
			}
			if (targetGranulePos <= packet.GranulePosition && targetGranulePos > packet.GranulePosition - packet.GranuleCount)
			{
				if (packet.Prev != null && !packet.Prev.GranuleCount.HasValue)
				{
					packet.Prev.GranulePosition = packet.GranulePosition - packet.GranuleCount.Value;
				}
				return packet;
			}
			packet = packet.Prev;
		}
		while (packet != null && packet.PageSequenceNumber == lastPacketInPage.PageSequenceNumber);
		if (packet != null && packet.PageGranulePosition < targetGranulePos)
		{
			packet.GranulePosition = packet.PageGranulePosition;
			return packet.Next;
		}
		return null;
	}

	public DataPacket FindPacket(long granulePos, Func<DataPacket, DataPacket, int> packetGranuleCountCallback)
	{
		if (granulePos < 0)
		{
			throw new ArgumentOutOfRangeException("granulePos");
		}
		Packet foundPacket = null;
		Packet packet = this._current ?? this._first;
		if (granulePos > packet.PageGranulePosition)
		{
			while (granulePos > packet.PageGranulePosition)
			{
				if ((packet.Next == null || packet.IsContinued) && !this._eosFound)
				{
					this._container.GatherNextPage(this._streamSerial);
					if (this._eosFound)
					{
						packet = null;
						break;
					}
				}
				packet = packet.Next;
			}
			return this.FindPacketInPage(packet, granulePos, packetGranuleCountCallback);
		}
		while (packet.Prev != null && (granulePos <= packet.Prev.PageGranulePosition || packet.Prev.PageGranulePosition == -1))
		{
			packet = packet.Prev;
		}
		return this.FindPacketInPage(packet, granulePos, packetGranuleCountCallback);
	}

	public void SeekToPacket(DataPacket packet, int preRoll)
	{
		if (preRoll < 0)
		{
			throw new ArgumentOutOfRangeException("preRoll");
		}
		if (packet == null)
		{
			throw new ArgumentNullException("granulePos");
		}
		Packet op = packet as Packet;
		if (op == null)
		{
			throw new ArgumentException("Incorrect packet type!", "packet");
		}
		while (--preRoll >= 0)
		{
			op = op.Prev;
			if (op == null)
			{
				throw new ArgumentOutOfRangeException("preRoll");
			}
		}
		this._current = op.Prev;
	}

	public long GetGranuleCount()
	{
		return this.GetLastPacket().PageGranulePosition;
	}
}
