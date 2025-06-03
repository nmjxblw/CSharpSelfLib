using Netcode;

namespace StardewValley.Network;

public abstract class NetPausableField<T, TField, TBaseField> : INetObject<NetFields> where TField : TBaseField, new() where TBaseField : NetFieldBase<T, TBaseField>, new()
{
	private bool paused;

	public readonly TField Field;

	private readonly NetEvent1Field<bool, NetBool> pauseEvent = new NetEvent1Field<bool, NetBool>();

	public T Value
	{
		get
		{
			return this.Get();
		}
		set
		{
			this.Set(value);
		}
	}

	public bool Paused
	{
		get
		{
			this.pauseEvent.Poll();
			return this.paused;
		}
		set
		{
			if (value != this.paused)
			{
				this.pauseEvent.Fire(value);
				this.pauseEvent.Poll();
			}
		}
	}

	public abstract NetFields NetFields { get; }

	public NetPausableField(TField field)
	{
		this.Field = field;
		this.initNetFields();
	}

	protected virtual void initNetFields()
	{
		this.NetFields.SetOwner(this).AddField(this.Field, "Field").AddField(this.pauseEvent, "pauseEvent");
		this.pauseEvent.onEvent += delegate(bool newPauseValue)
		{
			this.paused = newPauseValue;
		};
	}

	public NetPausableField()
		: this(new TField())
	{
	}

	public virtual T Get()
	{
		if (this.Paused)
		{
			this.Field.CancelInterpolation();
		}
		return this.Field.Get();
	}

	public void Set(T value)
	{
		this.Field.Set(value);
	}

	public bool IsPausePending()
	{
		return this.pauseEvent.HasPendingEvent((bool p) => p);
	}

	public bool IsInterpolating()
	{
		if (this.Field.IsInterpolating())
		{
			return !this.Paused;
		}
		return false;
	}
}
