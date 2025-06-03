using System;
using System.Collections.Generic;
using System.IO;

namespace Netcode;

public struct NetVersion : IEquatable<NetVersion>
{
	private List<uint> _vector;

	private List<uint> vector
	{
		get
		{
			if (this._vector == null)
			{
				this._vector = new List<uint>();
			}
			return this._vector;
		}
	}

	public uint this[int peerId]
	{
		get
		{
			if (peerId >= this.vector.Count)
			{
				return 0u;
			}
			return this.vector[peerId];
		}
		set
		{
			while (this.vector.Count <= peerId)
			{
				this.vector.Add(0u);
			}
			this.vector[peerId] = value;
		}
	}

	public NetVersion(NetVersion other)
	{
		this._vector = new List<uint>();
		this.Set(other);
	}

	public int Size()
	{
		return this.vector.Count;
	}

	public void Set(NetVersion other)
	{
		for (int i = 0; i < Math.Max(this.Size(), other.Size()); i++)
		{
			this[i] = other[i];
		}
	}

	public void Merge(NetVersion other)
	{
		for (int i = 0; i < Math.Max(this.Size(), other.Size()); i++)
		{
			this[i] = Math.Max(this[i], other[i]);
		}
	}

	public bool IsPriorityOver(NetVersion other)
	{
		for (int i = 0; i < Math.Max(this.Size(), other.Size()); i++)
		{
			if (this[i] > other[i])
			{
				return true;
			}
			if (this[i] < other[i])
			{
				return false;
			}
		}
		return true;
	}

	public bool IsSimultaneousWith(NetVersion other)
	{
		return this.isOrdered(other, (uint a, uint b) => a == b);
	}

	public bool IsPrecededBy(NetVersion other)
	{
		return this.isOrdered(other, (uint a, uint b) => a >= b);
	}

	public bool IsFollowedBy(NetVersion other)
	{
		return this.isOrdered(other, (uint a, uint b) => a < b);
	}

	public bool IsIndependent(NetVersion other)
	{
		if (!this.IsSimultaneousWith(other) && !this.IsPrecededBy(other))
		{
			return !this.IsFollowedBy(other);
		}
		return false;
	}

	private bool isOrdered(NetVersion other, Func<uint, uint, bool> comparison)
	{
		for (int i = 0; i < Math.Max(this.Size(), other.Size()); i++)
		{
			if (!comparison(this[i], other[i]))
			{
				return false;
			}
		}
		return true;
	}

	public override string ToString()
	{
		if (this.Size() == 0)
		{
			return "v0";
		}
		return "v" + string.Join(",", this.vector);
	}

	public bool Equals(NetVersion other)
	{
		for (int i = 0; i < Math.Max(this.Size(), other.Size()); i++)
		{
			if (this[i] != other[i])
			{
				return false;
			}
		}
		return true;
	}

	public override int GetHashCode()
	{
		return this.vector.GetHashCode() ^ -583558975;
	}

	public void Write(BinaryWriter writer)
	{
		writer.Write((byte)this.Size());
		for (int i = 0; i < this.Size(); i++)
		{
			writer.Write(this[i]);
		}
	}

	public void Read(BinaryReader reader)
	{
		int size = reader.ReadByte();
		while (this.vector.Count > size)
		{
			this.vector.RemoveAt(size);
		}
		while (this.vector.Count < size)
		{
			this.vector.Add(0u);
		}
		for (int i = 0; i < size; i++)
		{
			this[i] = reader.ReadUInt32();
		}
		for (int j = size; j < this.Size(); j++)
		{
			this[j] = 0u;
		}
	}

	public void Clear()
	{
		for (int i = 0; i < this.Size(); i++)
		{
			this[i] = 0u;
		}
	}
}
