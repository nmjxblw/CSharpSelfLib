using Microsoft.Xna.Framework;
using Netcode;

namespace StardewValley.Network;

public sealed class NetPosition : NetPausableField<Vector2, NetVector2, NetVector2>
{
	private const float SmoothingFudge = 0.8f;

	private const ushort DefaultDeltaAggregateTicks = 0;

	public bool ExtrapolationEnabled;

	public readonly NetBool moving = new NetBool().Interpolated(interpolate: false, wait: false);

	public override NetFields NetFields { get; } = new NetFields("NetPosition");

	public float X
	{
		get
		{
			return this.Get().X;
		}
		set
		{
			base.Set(new Vector2(value, this.Y));
		}
	}

	public float Y
	{
		get
		{
			return this.Get().Y;
		}
		set
		{
			base.Set(new Vector2(this.X, value));
		}
	}

	/// <summary>An event raised when this field's value is set (either locally or remotely). Not triggered by changes due to interpolation. May be triggered before the change is visible on the field, if InterpolationTicks &gt; 0.</summary>
	public event FieldChange<NetPosition, Vector2> fieldChangeEvent;

	/// <summary>An event raised after this field's value is set and interpolated.</summary>
	public event FieldChange<NetPosition, Vector2> fieldChangeVisibleEvent;

	public NetPosition()
		: base(new NetVector2().Interpolated(interpolate: true, wait: true))
	{
	}

	public NetPosition(NetVector2 field)
		: base(field)
	{
	}

	protected override void initNetFields()
	{
		base.initNetFields();
		this.NetFields.AddField(this.moving, "moving");
		this.NetFields.DeltaAggregateTicks = 0;
		base.Field.fieldChangeEvent += delegate(NetVector2 f, Vector2 oldValue, Vector2 newValue)
		{
			if (this.IsMaster())
			{
				this.moving.Value = true;
			}
			this.fieldChangeEvent?.Invoke(this, oldValue, newValue);
		};
		base.Field.fieldChangeVisibleEvent += delegate(NetVector2 field, Vector2 oldValue, Vector2 newValue)
		{
			this.fieldChangeVisibleEvent?.Invoke(this, oldValue, newValue);
		};
		this.moving.fieldChangeEvent += delegate(NetBool f, bool oldValue, bool newValue)
		{
			if (!this.IsMaster())
			{
				base.Field.ExtrapolationEnabled = newValue && this.ExtrapolationEnabled;
			}
		};
	}

	protected bool IsMaster()
	{
		INetRoot root = this.NetFields.Root;
		if (root == null)
		{
			return false;
		}
		return root.Clock.LocalId == 0;
	}

	public override Vector2 Get()
	{
		if (Game1.HostPaused)
		{
			base.Field.CancelInterpolation();
		}
		return base.Get();
	}

	public Vector2 CurrentInterpolationDirection()
	{
		if (base.Paused)
		{
			return Vector2.Zero;
		}
		return base.Field.CurrentInterpolationDirection();
	}

	public float CurrentInterpolationSpeed()
	{
		if (base.Paused)
		{
			return 0f;
		}
		return base.Field.CurrentInterpolationSpeed();
	}

	public void UpdateExtrapolation(float extrapolationSpeed)
	{
		this.NetFields.DeltaAggregateTicks = (ushort)((this.NetFields.Root != null) ? ((ushort)((float)this.NetFields.Root.Clock.InterpolationTicks * 0.8f)) : 0);
		this.ExtrapolationEnabled = true;
		base.Field.ExtrapolationSpeed = extrapolationSpeed;
		if (this.IsMaster())
		{
			this.moving.Value = false;
		}
	}
}
