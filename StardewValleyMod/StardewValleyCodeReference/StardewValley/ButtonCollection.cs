using System;
using Microsoft.Xna.Framework.Input;

namespace StardewValley;

/// <summary>
/// An effiecent way to iterate over active button state.
/// </summary>
public struct ButtonCollection
{
	public struct ButtonEnumerator
	{
		private readonly Buttons _pressed;

		private int _current;

		public Buttons Current
		{
			get
			{
				if (this._current < 0 || this._current > 32)
				{
					throw new InvalidOperationException();
				}
				return (Buttons)(1 << this._current);
			}
		}

		public ButtonEnumerator(Buttons pressed)
		{
			this._pressed = pressed;
			this._current = -1;
		}

		public bool MoveNext()
		{
			if (this._pressed == (Buttons)0)
			{
				return false;
			}
			while (this._current < 31)
			{
				this._current++;
				if (((uint)this._pressed & (uint)(1 << this._current)) != 0)
				{
					return true;
				}
			}
			return false;
		}

		public void Reset()
		{
			this._current = -1;
		}
	}

	private readonly Buttons _pressed;

	private readonly int _count;

	/// <summary>
	/// The number of pressed buttons.
	/// </summary>
	public int Count => this._count;

	/// <summary>
	/// Constructs a collection of the new pressed buttons.
	/// </summary>
	public ButtonCollection(ref GamePadState padState, ref GamePadState oldPadState)
	{
		this._count = 0;
		this._pressed = (Buttons)0;
		if (padState.IsButtonDown(Buttons.A) && !oldPadState.IsButtonDown(Buttons.A))
		{
			this._pressed |= Buttons.A;
			this._count++;
		}
		if (padState.IsButtonDown(Buttons.B) && !oldPadState.IsButtonDown(Buttons.B))
		{
			this._pressed |= Buttons.B;
			this._count++;
		}
		if (padState.IsButtonDown(Buttons.X) && !oldPadState.IsButtonDown(Buttons.X))
		{
			this._pressed |= Buttons.X;
			this._count++;
		}
		if (padState.IsButtonDown(Buttons.Y) && !oldPadState.IsButtonDown(Buttons.Y))
		{
			this._pressed |= Buttons.Y;
			this._count++;
		}
		if (padState.IsButtonDown(Buttons.Start) && !oldPadState.IsButtonDown(Buttons.Start))
		{
			this._pressed |= Buttons.Start;
			this._count++;
		}
		if (padState.IsButtonDown(Buttons.Back) && !oldPadState.IsButtonDown(Buttons.Back))
		{
			this._pressed |= Buttons.Back;
			this._count++;
		}
		if (padState.IsButtonDown(Buttons.RightTrigger) && !oldPadState.IsButtonDown(Buttons.RightTrigger))
		{
			this._pressed |= Buttons.RightTrigger;
			this._count++;
		}
		if (padState.IsButtonDown(Buttons.LeftTrigger) && !oldPadState.IsButtonDown(Buttons.LeftTrigger))
		{
			this._pressed |= Buttons.LeftTrigger;
			this._count++;
		}
		if (padState.IsButtonDown(Buttons.RightShoulder) && !oldPadState.IsButtonDown(Buttons.RightShoulder))
		{
			this._pressed |= Buttons.RightShoulder;
			this._count++;
		}
		if (padState.IsButtonDown(Buttons.LeftShoulder) && !oldPadState.IsButtonDown(Buttons.LeftShoulder))
		{
			this._pressed |= Buttons.LeftShoulder;
			this._count++;
		}
		if (padState.IsButtonDown(Buttons.DPadUp) && !oldPadState.IsButtonDown(Buttons.DPadUp))
		{
			this._pressed |= Buttons.DPadUp;
			this._count++;
		}
		if (padState.IsButtonDown(Buttons.DPadRight) && !oldPadState.IsButtonDown(Buttons.DPadRight))
		{
			this._pressed |= Buttons.DPadRight;
			this._count++;
		}
		if (padState.IsButtonDown(Buttons.DPadDown) && !oldPadState.IsButtonDown(Buttons.DPadDown))
		{
			this._pressed |= Buttons.DPadDown;
			this._count++;
		}
		if (padState.IsButtonDown(Buttons.DPadLeft) && !oldPadState.IsButtonDown(Buttons.DPadLeft))
		{
			this._pressed |= Buttons.DPadLeft;
			this._count++;
		}
		if ((double)padState.ThumbSticks.Left.Y > 0.2 && (double)oldPadState.ThumbSticks.Left.Y <= 0.2 && Utility.thumbstickIsInDirection(0, padState))
		{
			this._pressed |= Buttons.LeftThumbstickUp;
			this._count++;
		}
		if ((double)padState.ThumbSticks.Left.X > 0.2 && (double)oldPadState.ThumbSticks.Left.X <= 0.2 && Utility.thumbstickIsInDirection(1, padState))
		{
			this._pressed |= Buttons.LeftThumbstickRight;
			this._count++;
		}
		if ((double)padState.ThumbSticks.Left.Y < -0.2 && (double)oldPadState.ThumbSticks.Left.Y >= -0.2 && Utility.thumbstickIsInDirection(2, padState))
		{
			this._pressed |= Buttons.LeftThumbstickDown;
			this._count++;
		}
		if ((double)padState.ThumbSticks.Left.X < -0.2 && (double)oldPadState.ThumbSticks.Left.X >= -0.2 && Utility.thumbstickIsInDirection(3, padState))
		{
			this._pressed |= Buttons.LeftThumbstickLeft;
			this._count++;
		}
	}

	/// <summary>
	/// Constructs a collection of held buttons.
	/// </summary>
	/// <param name="padState"></param>
	public ButtonCollection(ref GamePadState padState)
	{
		this._count = 0;
		this._pressed = (Buttons)0;
		if (padState.IsButtonDown(Buttons.A))
		{
			this._pressed |= Buttons.A;
			this._count++;
		}
		if (padState.IsButtonDown(Buttons.B))
		{
			this._pressed |= Buttons.B;
			this._count++;
		}
		if (padState.IsButtonDown(Buttons.X))
		{
			this._pressed |= Buttons.X;
			this._count++;
		}
		if (padState.IsButtonDown(Buttons.Y))
		{
			this._pressed |= Buttons.Y;
			this._count++;
		}
		if (padState.IsButtonDown(Buttons.Start))
		{
			this._pressed |= Buttons.Start;
			this._count++;
		}
		if (padState.IsButtonDown(Buttons.Back))
		{
			this._pressed |= Buttons.Back;
			this._count++;
		}
		if (padState.IsButtonDown(Buttons.RightTrigger))
		{
			this._pressed |= Buttons.RightTrigger;
			this._count++;
		}
		if (padState.IsButtonDown(Buttons.LeftTrigger))
		{
			this._pressed |= Buttons.LeftTrigger;
			this._count++;
		}
		if (padState.IsButtonDown(Buttons.RightShoulder))
		{
			this._pressed |= Buttons.RightShoulder;
			this._count++;
		}
		if (padState.IsButtonDown(Buttons.LeftShoulder))
		{
			this._pressed |= Buttons.LeftShoulder;
			this._count++;
		}
		if (padState.IsButtonDown(Buttons.DPadUp))
		{
			this._pressed |= Buttons.DPadUp;
			this._count++;
		}
		if (padState.IsButtonDown(Buttons.DPadRight))
		{
			this._pressed |= Buttons.DPadRight;
			this._count++;
		}
		if (padState.IsButtonDown(Buttons.DPadDown))
		{
			this._pressed |= Buttons.DPadDown;
			this._count++;
		}
		if (padState.IsButtonDown(Buttons.DPadLeft))
		{
			this._pressed |= Buttons.DPadLeft;
			this._count++;
		}
		if (padState.IsButtonDown(Buttons.LeftThumbstickUp))
		{
			this._pressed |= Buttons.LeftThumbstickUp;
			this._count++;
		}
		if (padState.IsButtonDown(Buttons.LeftThumbstickRight))
		{
			this._pressed |= Buttons.LeftThumbstickRight;
			this._count++;
		}
		if (padState.IsButtonDown(Buttons.LeftThumbstickDown))
		{
			this._pressed |= Buttons.LeftThumbstickDown;
			this._count++;
		}
		if (padState.IsButtonDown(Buttons.LeftThumbstickLeft))
		{
			this._pressed |= Buttons.LeftThumbstickLeft;
			this._count++;
		}
	}

	public ButtonEnumerator GetEnumerator()
	{
		return new ButtonEnumerator(this._pressed);
	}
}
