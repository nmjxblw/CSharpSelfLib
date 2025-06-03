using System;
using System.IO;

namespace Netcode;

public abstract class NetFieldBase<T, TSelf> : AbstractNetSerializable, IEquatable<TSelf>, InterpolationCancellable where TSelf : NetFieldBase<T, TSelf>
{
	[Flags]
	protected enum NetFieldBaseBool : byte
	{
		None = 0,
		InterpolationEnabled = 1,
		ExtrapolationEnabled = 2,
		InterpolationWait = 4,
		notifyOnTargetValueChange = 8
	}

	protected NetFieldBaseBool _bools;

	protected uint interpolationStartTick;

	protected T value;

	protected T previousValue;

	protected T targetValue;

	public bool InterpolationEnabled
	{
		get
		{
			return (this._bools & NetFieldBaseBool.InterpolationEnabled) != 0;
		}
		set
		{
			if (value)
			{
				this._bools |= NetFieldBaseBool.InterpolationEnabled;
			}
			else
			{
				this._bools &= ~NetFieldBaseBool.InterpolationEnabled;
			}
		}
	}

	public bool ExtrapolationEnabled
	{
		get
		{
			return (this._bools & NetFieldBaseBool.ExtrapolationEnabled) != 0;
		}
		set
		{
			if (value)
			{
				this._bools |= NetFieldBaseBool.ExtrapolationEnabled;
			}
			else
			{
				this._bools &= ~NetFieldBaseBool.ExtrapolationEnabled;
			}
		}
	}

	public bool InterpolationWait
	{
		get
		{
			return (this._bools & NetFieldBaseBool.InterpolationWait) != 0;
		}
		set
		{
			if (value)
			{
				this._bools |= NetFieldBaseBool.InterpolationWait;
			}
			else
			{
				this._bools &= ~NetFieldBaseBool.InterpolationWait;
			}
		}
	}

	protected bool notifyOnTargetValueChange
	{
		get
		{
			return (this._bools & NetFieldBaseBool.notifyOnTargetValueChange) != 0;
		}
		set
		{
			if (value)
			{
				this._bools |= NetFieldBaseBool.notifyOnTargetValueChange;
			}
			else
			{
				this._bools &= ~NetFieldBaseBool.notifyOnTargetValueChange;
			}
		}
	}

	public T TargetValue => this.targetValue;

	public T Value
	{
		get
		{
			return this.value;
		}
		set
		{
			this.Set(value);
		}
	}

	/// <summary>An event raised when this field's value is set (either locally or remotely). Not triggered by changes due to interpolation. May be triggered before the change is visible on the field, if InterpolationTicks &gt; 0.</summary>
	public event FieldChange<TSelf, T> fieldChangeEvent;

	/// <summary>An event raised after this field's value is set and interpolated.</summary>
	public event FieldChange<TSelf, T> fieldChangeVisibleEvent;

	public NetFieldBase()
	{
		this.InterpolationWait = true;
		this.value = default(T);
		this.previousValue = default(T);
		this.targetValue = default(T);
	}

	public NetFieldBase(T value)
		: this()
	{
		this.cleanSet(value);
	}

	public TSelf Interpolated(bool interpolate, bool wait)
	{
		this.InterpolationEnabled = interpolate;
		this.InterpolationWait = wait;
		return (TSelf)this;
	}

	protected virtual int InterpolationTicks()
	{
		if (base.Root == null)
		{
			return 0;
		}
		return base.Root.Clock.InterpolationTicks;
	}

	protected float InterpolationFactor()
	{
		return (float)(base.Root.Clock.GetLocalTick() - this.interpolationStartTick) / (float)this.InterpolationTicks();
	}

	public bool IsInterpolating()
	{
		if (this.InterpolationEnabled)
		{
			return base.NeedsTick;
		}
		return false;
	}

	public bool IsChanging()
	{
		return base.NeedsTick;
	}

	protected override bool tickImpl()
	{
		if (base.Root != null && this.InterpolationTicks() > 0)
		{
			float factor = this.InterpolationFactor();
			bool shouldExtrapolate = this.ExtrapolationEnabled && base.ChangeVersion[0] == base.Root.Clock.netVersion[0];
			if ((factor < 1f && this.InterpolationEnabled) || (shouldExtrapolate && factor < 3f))
			{
				this.value = this.interpolate(this.previousValue, this.targetValue, factor);
				return true;
			}
			if (factor < 1f && this.InterpolationWait)
			{
				this.value = this.previousValue;
				return true;
			}
		}
		T oldValue = this.previousValue;
		this.CancelInterpolation();
		if (this.fieldChangeVisibleEvent != null)
		{
			this.fieldChangeVisibleEvent((TSelf)this, oldValue, this.value);
		}
		return false;
	}

	public void CancelInterpolation()
	{
		if (base.NeedsTick)
		{
			this.value = this.targetValue;
			this.previousValue = default(T);
			base.NeedsTick = false;
		}
	}

	public T Get()
	{
		return this.value;
	}

	protected virtual T interpolate(T startValue, T endValue, float factor)
	{
		return startValue;
	}

	public abstract void Set(T newValue);

	protected bool canShortcutSet()
	{
		if (this.Dirty && this.fieldChangeEvent == null)
		{
			return this.fieldChangeVisibleEvent == null;
		}
		return false;
	}

	protected virtual void targetValueChanged(T oldValue, T newValue)
	{
	}

	protected void cleanSet(T newValue)
	{
		T oldValue = this.value;
		T oldTargetValue = this.targetValue;
		this.targetValue = newValue;
		this.value = newValue;
		this.previousValue = default(T);
		base.NeedsTick = false;
		if (this.notifyOnTargetValueChange)
		{
			this.targetValueChanged(oldTargetValue, newValue);
		}
		if (this.fieldChangeEvent != null)
		{
			this.fieldChangeEvent((TSelf)this, oldValue, newValue);
		}
		if (this.fieldChangeVisibleEvent != null)
		{
			this.fieldChangeVisibleEvent((TSelf)this, oldValue, newValue);
		}
	}

	protected virtual bool setUpInterpolation(T oldValue, T newValue)
	{
		return true;
	}

	protected void setInterpolationTarget(T newValue)
	{
		T oldValue = this.value;
		if (!this.InterpolationWait || base.Root == null || !this.setUpInterpolation(oldValue, newValue))
		{
			this.cleanSet(newValue);
			return;
		}
		T oldTargetValue = this.targetValue;
		this.previousValue = oldValue;
		base.NeedsTick = true;
		this.targetValue = newValue;
		this.interpolationStartTick = base.Root.Clock.GetLocalTick();
		if (this.notifyOnTargetValueChange)
		{
			this.targetValueChanged(oldTargetValue, newValue);
		}
		if (this.fieldChangeEvent != null)
		{
			this.fieldChangeEvent((TSelf)this, oldValue, newValue);
		}
	}

	protected abstract void ReadDelta(BinaryReader reader, NetVersion version);

	protected abstract void WriteDelta(BinaryWriter writer);

	public override void ReadFull(BinaryReader reader, NetVersion version)
	{
		this.ReadDelta(reader, version);
		this.CancelInterpolation();
		base.ChangeVersion.Merge(version);
	}

	public override void WriteFull(BinaryWriter writer)
	{
		this.WriteDelta(writer);
	}

	public override void Read(BinaryReader reader, NetVersion version)
	{
		this.ReadDelta(reader, version);
		base.ChangeVersion.Merge(version);
	}

	public override void Write(BinaryWriter writer)
	{
		this.WriteDelta(writer);
	}

	public override string ToString()
	{
		if (this.value != null)
		{
			return this.value.ToString();
		}
		return "null";
	}

	public override bool Equals(object obj)
	{
		if (!(obj is TSelf otherField) || !this.Equals(otherField))
		{
			return object.Equals(this.Value, obj);
		}
		return true;
	}

	public bool Equals(TSelf other)
	{
		return object.Equals(this.Value, other.Value);
	}

	public static bool operator ==(NetFieldBase<T, TSelf> self, TSelf other)
	{
		if ((object)self != other)
		{
			return object.Equals(self, other);
		}
		return true;
	}

	public static bool operator !=(NetFieldBase<T, TSelf> self, TSelf other)
	{
		if ((object)self != other)
		{
			return !object.Equals(self, other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return ((this.value != null) ? this.value.GetHashCode() : 0) ^ -858436897;
	}
}
