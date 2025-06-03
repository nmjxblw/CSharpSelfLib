using System;

namespace Microsoft.Xna.Framework.Input.Touch;

public struct TouchLocation : IEquatable<TouchLocation>
{
	/// <summary>
	///             Attributes 
	/// </summary>
	private int _id;

	private Vector2 _position;

	private Vector2 _previousPosition;

	private TouchLocationState _state;

	private TouchLocationState _previousState;

	private float _pressure;

	private float _previousPressure;

	private Vector2 _velocity;

	private Vector2 _pressPosition;

	private TimeSpan _pressTimestamp;

	private TimeSpan _timestamp;

	/// <summary>
	/// True if this touch was pressed and released on the same frame.
	/// In this case we will keep it around for the user to get by GetState that frame.
	/// However if they do not call GetState that frame, this touch will be forgotten.
	/// </summary>
	internal bool SameFrameReleased;

	/// <summary>
	/// Helper for assigning an invalid touch location.
	/// </summary>
	internal static readonly TouchLocation Invalid;

	internal Vector2 PressPosition => this._pressPosition;

	internal TimeSpan PressTimestamp => this._pressTimestamp;

	internal TimeSpan Timestamp => this._timestamp;

	internal Vector2 Velocity => this._velocity;

	public int Id => this._id;

	public Vector2 Position => this._position;

	public float Pressure => this._pressure;

	public TouchLocationState State => this._state;

	public TouchLocation(int id, TouchLocationState state, Vector2 position)
		: this(id, state, position, TouchLocationState.Invalid, Vector2.Zero)
	{
	}

	public TouchLocation(int id, TouchLocationState state, Vector2 position, TouchLocationState previousState, Vector2 previousPosition)
		: this(id, state, position, previousState, previousPosition, TimeSpan.Zero)
	{
	}

	internal TouchLocation(int id, TouchLocationState state, Vector2 position, TimeSpan timestamp)
		: this(id, state, position, TouchLocationState.Invalid, Vector2.Zero, timestamp)
	{
	}

	internal TouchLocation(int id, TouchLocationState state, Vector2 position, TouchLocationState previousState, Vector2 previousPosition, TimeSpan timestamp)
	{
		this._id = id;
		this._state = state;
		this._position = position;
		this._pressure = 0f;
		this._previousState = previousState;
		this._previousPosition = previousPosition;
		this._previousPressure = 0f;
		this._timestamp = timestamp;
		this._velocity = Vector2.Zero;
		if (state == TouchLocationState.Pressed)
		{
			this._pressPosition = this._position;
			this._pressTimestamp = this._timestamp;
		}
		else
		{
			this._pressPosition = Vector2.Zero;
			this._pressTimestamp = TimeSpan.Zero;
		}
		this.SameFrameReleased = false;
	}

	/// <summary>
	/// Returns a copy of the touch with the state changed to moved.
	/// </summary>
	/// <returns>The new touch location.</returns>
	internal TouchLocation AsMovedState()
	{
		TouchLocation touch = this;
		touch._previousState = touch._state;
		touch._previousPosition = touch._position;
		touch._previousPressure = touch._pressure;
		touch._state = TouchLocationState.Moved;
		return touch;
	}

	/// <summary>
	/// Updates the touch location using the new event.
	/// </summary>
	/// <param name="touchEvent">The next event for this touch location.</param>
	internal bool UpdateState(TouchLocation touchEvent)
	{
		this._previousPosition = this._position;
		this._previousState = this._state;
		this._previousPressure = this._pressure;
		this._position = touchEvent._position;
		if (touchEvent.State == TouchLocationState.Released)
		{
			this._state = touchEvent._state;
		}
		this._pressure = touchEvent._pressure;
		Vector2 delta = this._position - this._previousPosition;
		TimeSpan elapsed = touchEvent.Timestamp - this._timestamp;
		if (elapsed > TimeSpan.Zero)
		{
			Vector2 velocity = delta / (float)elapsed.TotalSeconds;
			this._velocity += (velocity - this._velocity) * 0.45f;
		}
		if (this._previousState == TouchLocationState.Pressed && this._state == TouchLocationState.Released && elapsed == TimeSpan.Zero)
		{
			this.SameFrameReleased = true;
			this._state = TouchLocationState.Pressed;
		}
		this._timestamp = touchEvent.Timestamp;
		if (this._state == this._previousState)
		{
			return delta.LengthSquared() > 0.001f;
		}
		return true;
	}

	public override bool Equals(object obj)
	{
		if (obj is TouchLocation)
		{
			return this.Equals((TouchLocation)obj);
		}
		return false;
	}

	public bool Equals(TouchLocation other)
	{
		if (this._id.Equals(other._id) && this._position.Equals(other._position))
		{
			return this._previousPosition.Equals(other._previousPosition);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return this._id;
	}

	public override string ToString()
	{
		string[] obj = new string[14]
		{
			"Touch id:",
			this._id.ToString(),
			" state:",
			this._state.ToString(),
			" position:",
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null,
			null
		};
		Vector2 position = this._position;
		obj[5] = position.ToString();
		obj[6] = " pressure:";
		obj[7] = this._pressure.ToString();
		obj[8] = " prevState:";
		obj[9] = this._previousState.ToString();
		obj[10] = " prevPosition:";
		position = this._previousPosition;
		obj[11] = position.ToString();
		obj[12] = " previousPressure:";
		obj[13] = this._previousPressure.ToString();
		return string.Concat(obj);
	}

	public bool TryGetPreviousLocation(out TouchLocation aPreviousLocation)
	{
		if (this._previousState == TouchLocationState.Invalid)
		{
			aPreviousLocation._id = -1;
			aPreviousLocation._state = TouchLocationState.Invalid;
			aPreviousLocation._position = Vector2.Zero;
			aPreviousLocation._previousState = TouchLocationState.Invalid;
			aPreviousLocation._previousPosition = Vector2.Zero;
			aPreviousLocation._pressure = 0f;
			aPreviousLocation._previousPressure = 0f;
			aPreviousLocation._timestamp = TimeSpan.Zero;
			aPreviousLocation._pressPosition = Vector2.Zero;
			aPreviousLocation._pressTimestamp = TimeSpan.Zero;
			aPreviousLocation._velocity = Vector2.Zero;
			aPreviousLocation.SameFrameReleased = false;
			return false;
		}
		aPreviousLocation._id = this._id;
		aPreviousLocation._state = this._previousState;
		aPreviousLocation._position = this._previousPosition;
		aPreviousLocation._previousState = TouchLocationState.Invalid;
		aPreviousLocation._previousPosition = Vector2.Zero;
		aPreviousLocation._pressure = this._previousPressure;
		aPreviousLocation._previousPressure = 0f;
		aPreviousLocation._timestamp = this._timestamp;
		aPreviousLocation._pressPosition = this._pressPosition;
		aPreviousLocation._pressTimestamp = this._pressTimestamp;
		aPreviousLocation._velocity = this._velocity;
		aPreviousLocation.SameFrameReleased = this.SameFrameReleased;
		return true;
	}

	public static bool operator !=(TouchLocation value1, TouchLocation value2)
	{
		if (value1._id == value2._id && value1._state == value2._state && !(value1._position != value2._position) && value1._previousState == value2._previousState)
		{
			return value1._previousPosition != value2._previousPosition;
		}
		return true;
	}

	public static bool operator ==(TouchLocation value1, TouchLocation value2)
	{
		if (value1._id == value2._id && value1._state == value2._state && value1._position == value2._position && value1._previousState == value2._previousState)
		{
			return value1._previousPosition == value2._previousPosition;
		}
		return false;
	}

	internal void AgeState()
	{
		if (this._state == TouchLocationState.Moved)
		{
			this._previousState = this._state;
			this._previousPosition = this._position;
			this._previousPressure = this._pressure;
			return;
		}
		this._previousState = this._state;
		this._previousPosition = this._position;
		this._previousPressure = this._pressure;
		if (this.SameFrameReleased)
		{
			this._state = TouchLocationState.Released;
		}
		else
		{
			this._state = TouchLocationState.Moved;
		}
	}
}
