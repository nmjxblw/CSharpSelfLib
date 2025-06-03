using System;
using System.IO;
using Microsoft.Xna.Framework;

namespace Netcode;

public sealed class NetVector2 : NetField<Vector2, NetVector2>
{
	public bool AxisAlignedMovement;

	public float ExtrapolationSpeed;

	public float MinDeltaForDirectionChange = 8f;

	public float MaxInterpolationDistance = 320f;

	private bool interpolateXFirst;

	private bool isExtrapolating;

	private bool isFixingExtrapolation;

	public float X
	{
		get
		{
			return base.Value.X;
		}
		set
		{
			Vector2 vector = base.value;
			if (vector.X != value)
			{
				Vector2 newValue = new Vector2(value, vector.Y);
				if (base.canShortcutSet())
				{
					base.value = newValue;
					return;
				}
				base.cleanSet(newValue);
				base.MarkDirty();
			}
		}
	}

	public float Y
	{
		get
		{
			return base.Value.Y;
		}
		set
		{
			Vector2 vector = base.value;
			if (vector.Y != value)
			{
				Vector2 newValue = new Vector2(vector.X, value);
				if (base.canShortcutSet())
				{
					base.value = newValue;
					return;
				}
				base.cleanSet(newValue);
				base.MarkDirty();
			}
		}
	}

	public NetVector2()
	{
	}

	public NetVector2(Vector2 value)
		: base(value)
	{
	}

	public void Set(float x, float y)
	{
		this.Set(new Vector2(x, y));
	}

	public override void Set(Vector2 newValue)
	{
		if (base.canShortcutSet())
		{
			base.value = newValue;
		}
		else if (newValue != base.value)
		{
			base.cleanSet(newValue);
			base.MarkDirty();
		}
	}

	public Vector2 InterpolationDelta()
	{
		if (base.NeedsTick)
		{
			return base.targetValue - base.previousValue;
		}
		return Vector2.Zero;
	}

	protected override bool setUpInterpolation(Vector2 oldValue, Vector2 newValue)
	{
		if ((newValue - oldValue).LengthSquared() >= this.MaxInterpolationDistance * this.MaxInterpolationDistance)
		{
			return false;
		}
		if (this.AxisAlignedMovement)
		{
			if (base.NeedsTick)
			{
				Vector2 delta = base.targetValue - base.previousValue;
				Vector2 absDelta = new Vector2(Math.Abs(delta.X), Math.Abs(delta.Y));
				if (this.interpolateXFirst)
				{
					this.interpolateXFirst = base.InterpolationFactor() * (absDelta.X + absDelta.Y) < absDelta.X;
				}
				else
				{
					this.interpolateXFirst = base.InterpolationFactor() * (absDelta.X + absDelta.Y) > absDelta.Y;
				}
			}
			else
			{
				Vector2 delta2 = newValue - oldValue;
				Vector2 absDelta2 = new Vector2(Math.Abs(delta2.X), Math.Abs(delta2.Y));
				this.interpolateXFirst = absDelta2.X < absDelta2.Y;
			}
		}
		return true;
	}

	public Vector2 CurrentInterpolationDirection()
	{
		if (this.AxisAlignedMovement)
		{
			float factor = base.InterpolationFactor();
			Vector2 delta = this.InterpolationDelta();
			float traveledLength = (Math.Abs(delta.X) + Math.Abs(delta.Y)) * factor;
			if (Math.Abs(delta.X) < this.MinDeltaForDirectionChange && Math.Abs(delta.Y) < this.MinDeltaForDirectionChange)
			{
				return Vector2.Zero;
			}
			if (Math.Abs(delta.X) < this.MinDeltaForDirectionChange)
			{
				return new Vector2(0f, Math.Sign(delta.Y));
			}
			if (Math.Abs(delta.Y) < this.MinDeltaForDirectionChange)
			{
				return new Vector2(Math.Sign(delta.X), 0f);
			}
			if (this.interpolateXFirst)
			{
				if (traveledLength > Math.Abs(delta.X))
				{
					return new Vector2(0f, Math.Sign(delta.Y));
				}
				return new Vector2(Math.Sign(delta.X), 0f);
			}
			if (traveledLength > Math.Abs(delta.Y))
			{
				return new Vector2(Math.Sign(delta.X), 0f);
			}
			return new Vector2(0f, Math.Sign(delta.Y));
		}
		Vector2 delta2 = this.InterpolationDelta();
		delta2.Normalize();
		return delta2;
	}

	public float CurrentInterpolationSpeed()
	{
		float distance = this.InterpolationDelta().Length();
		if (this.InterpolationTicks() == 0)
		{
			return distance;
		}
		if (base.InterpolationFactor() > 1f)
		{
			return this.ExtrapolationSpeed;
		}
		return distance / (float)this.InterpolationTicks();
	}

	protected override Vector2 interpolate(Vector2 startValue, Vector2 endValue, float factor)
	{
		if (this.AxisAlignedMovement && factor <= 1f && !this.isFixingExtrapolation)
		{
			this.isExtrapolating = false;
			Vector2 delta = this.InterpolationDelta();
			Vector2 absDelta = new Vector2(Math.Abs(delta.X), Math.Abs(delta.Y));
			float traveledLength = (absDelta.X + absDelta.Y) * factor;
			float x;
			float y;
			if (this.interpolateXFirst)
			{
				if (traveledLength > absDelta.X)
				{
					x = endValue.X;
					y = startValue.Y + (traveledLength - absDelta.X) * (float)Math.Sign(delta.Y);
				}
				else
				{
					x = startValue.X + traveledLength * (float)Math.Sign(delta.X);
					y = startValue.Y;
				}
			}
			else if (traveledLength > absDelta.Y)
			{
				y = endValue.Y;
				x = startValue.X + (traveledLength - absDelta.Y) * (float)Math.Sign(delta.X);
			}
			else
			{
				y = startValue.Y + traveledLength * (float)Math.Sign(delta.Y);
				x = startValue.X;
			}
			return new Vector2(x, y);
		}
		if (factor > 1f)
		{
			this.isExtrapolating = true;
			uint extrapolationTicks = base.Root.Clock.GetLocalTick() - base.interpolationStartTick - (uint)this.InterpolationTicks();
			Vector2 direction = endValue - startValue;
			if (direction.LengthSquared() > this.ExtrapolationSpeed * this.ExtrapolationSpeed)
			{
				direction.Normalize();
				return endValue + direction * extrapolationTicks * this.ExtrapolationSpeed;
			}
		}
		this.isExtrapolating = false;
		return startValue + (endValue - startValue) * factor;
	}

	protected override void ReadDelta(BinaryReader reader, NetVersion version)
	{
		float newX = reader.ReadSingle();
		float newY = reader.ReadSingle();
		if (version.IsPriorityOver(base.ChangeVersion))
		{
			this.isFixingExtrapolation = this.isExtrapolating;
			base.setInterpolationTarget(new Vector2(newX, newY));
			this.isExtrapolating = false;
		}
	}

	protected override void WriteDelta(BinaryWriter writer)
	{
		writer.Write(base.Value.X);
		writer.Write(base.Value.Y);
	}
}
