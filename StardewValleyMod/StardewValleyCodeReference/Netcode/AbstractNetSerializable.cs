using System;
using System.IO;

namespace Netcode;

public abstract class AbstractNetSerializable : INetSerializable, INetObject<INetSerializable>
{
	private uint dirtyTick = uint.MaxValue;

	private uint minNextDirtyTime;

	protected NetVersion ChangeVersion;

	public ushort DeltaAggregateTicks;

	private bool needsTick;

	private bool childNeedsTick;

	private INetSerializable parent;

	public uint DirtyTick
	{
		get
		{
			return this.dirtyTick;
		}
		set
		{
			if (value < this.dirtyTick)
			{
				this.SetDirtySooner(value);
			}
			else if (value > this.dirtyTick)
			{
				this.SetDirtyLater(value);
			}
		}
	}

	public virtual bool Dirty => this.dirtyTick != uint.MaxValue;

	public bool NeedsTick
	{
		get
		{
			return this.needsTick;
		}
		set
		{
			if (value != this.needsTick)
			{
				this.needsTick = value;
				if (value && this.Parent != null)
				{
					this.Parent.ChildNeedsTick = true;
				}
			}
		}
	}

	public bool ChildNeedsTick
	{
		get
		{
			return this.childNeedsTick;
		}
		set
		{
			if (value != this.childNeedsTick)
			{
				this.childNeedsTick = value;
				if (value && this.Parent != null)
				{
					this.Parent.ChildNeedsTick = true;
				}
			}
		}
	}

	/// <inheritdoc />
	public string Name { get; set; }

	public INetRoot Root { get; protected set; }

	public INetSerializable Parent
	{
		get
		{
			return this.parent;
		}
		set
		{
			this.SetParent(value);
		}
	}

	public INetSerializable NetFields => this;

	/// <summary>
	/// Use this when you want to always use the update from the other end, even if
	/// it is "older" (such as us updating a position every frame, but we receive
	/// a better position from the host from a couple frames ago)
	/// </summary>
	public void ResetNewestReceivedChangeVersion()
	{
		this.ChangeVersion.Clear();
	}

	protected void SetDirtySooner(uint tick)
	{
		tick = Math.Max(tick, this.minNextDirtyTime);
		if (this.dirtyTick > tick)
		{
			this.dirtyTick = tick;
			if (this.Parent != null)
			{
				this.Parent.DirtyTick = Math.Min(this.Parent.DirtyTick, tick);
			}
			if (this.Root != null)
			{
				this.minNextDirtyTime = this.Root.Clock.GetLocalTick() + this.DeltaAggregateTicks;
				this.ChangeVersion.Set(this.Root.Clock.netVersion);
			}
			else
			{
				this.minNextDirtyTime = 0u;
				this.ChangeVersion.Clear();
			}
		}
	}

	protected void SetDirtyLater(uint tick)
	{
		if (this.dirtyTick < tick)
		{
			this.dirtyTick = tick;
			this.ForEachChild(delegate(INetSerializable child)
			{
				child.DirtyTick = Math.Max(child.DirtyTick, tick);
			});
			if (tick == uint.MaxValue)
			{
				this.CleanImpl();
			}
		}
	}

	protected virtual void CleanImpl()
	{
		if (this.Root == null)
		{
			this.minNextDirtyTime = 0u;
		}
		else
		{
			this.minNextDirtyTime = this.Root.Clock.GetLocalTick() + this.DeltaAggregateTicks;
		}
	}

	public void MarkDirty()
	{
		if (this.Root == null)
		{
			this.SetDirtySooner(0u);
		}
		else
		{
			this.SetDirtySooner(this.Root.Clock.GetLocalTick());
		}
	}

	public void MarkClean()
	{
		this.SetDirtyLater(uint.MaxValue);
	}

	protected virtual bool tickImpl()
	{
		return false;
	}

	public bool Tick()
	{
		if (this.needsTick)
		{
			this.needsTick = this.tickImpl();
		}
		if (this.childNeedsTick)
		{
			this.childNeedsTick = false;
			this.ForEachChild(delegate(INetSerializable child)
			{
				if (child.NeedsTick || child.ChildNeedsTick)
				{
					this.childNeedsTick |= child.Tick();
				}
			});
		}
		return this.childNeedsTick | this.needsTick;
	}

	public abstract void Read(BinaryReader reader, NetVersion version);

	public abstract void Write(BinaryWriter writer);

	public abstract void ReadFull(BinaryReader reader, NetVersion version);

	public abstract void WriteFull(BinaryWriter writer);

	protected uint GetLocalTick()
	{
		if (this.Root != null)
		{
			return this.Root.Clock.GetLocalTick();
		}
		return 0u;
	}

	protected NetVersion GetLocalVersion()
	{
		NetVersion version = default(NetVersion);
		if (this.Root != null)
		{
			version.Set(this.Root.Clock.netVersion);
		}
		return version;
	}

	protected virtual void SetParent(INetSerializable parent)
	{
		this.parent = parent;
		if (parent != null)
		{
			this.Root = parent.Root;
			this.SetChildParents();
		}
		else
		{
			this.ClearChildParents();
		}
		this.MarkClean();
		this.ChangeVersion.Clear();
		this.minNextDirtyTime = 0u;
	}

	protected virtual void SetChildParents()
	{
		this.ForEachChild(delegate(INetSerializable child)
		{
			child.Parent = this;
		});
	}

	protected virtual void ClearChildParents()
	{
		this.ForEachChild(delegate(INetSerializable child)
		{
			if (child.Parent == this)
			{
				child.Parent = null;
			}
		});
	}

	protected virtual void ValidateChild(INetSerializable child)
	{
		if (child == null)
		{
			throw new InvalidOperationException("Net field '" + this.Name + "' incorrectly contains a null field.");
		}
		if ((this.Parent != null || this.Root == this) && child.Parent != this)
		{
			throw new InvalidOperationException($"Net field '{this.Name}' has child '{child.Name}' which is already linked to parent '{child.Parent?.Name ?? "<null>"}'.");
		}
	}

	protected virtual void ValidateChildren()
	{
		if (this.Parent != null || this.Root == this)
		{
			this.ForEachChild(ValidateChild);
		}
	}

	protected virtual void ForEachChild(Action<INetSerializable> childAction)
	{
	}
}
