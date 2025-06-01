using System;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Input.Touch;

public class TouchPanelState
{
	/// <summary>
	/// The reserved touchId for all mouse touch points.
	/// </summary>
	private const int MouseTouchId = 1;

	/// <summary>
	/// The current touch state.
	/// </summary>
	private readonly List<TouchLocation> _touchState = new List<TouchLocation>();

	/// <summary>
	/// The current gesture state.
	/// </summary>
	private readonly List<TouchLocation> _gestureState = new List<TouchLocation>();

	/// <summary>
	/// The positional scale to apply to touch input.
	/// </summary>
	private Vector2 _touchScale = Vector2.One;

	/// <summary>
	/// The current size of the display.
	/// </summary>
	private Point _displaySize = Point.Zero;

	/// <summary>
	/// The next touch location identifier.
	/// The value 1 is reserved for the mouse touch point.
	/// </summary>
	private int _nextTouchId = 2;

	/// <summary>
	/// The mapping between platform specific touch ids
	/// and the touch ids we assign to touch locations.
	/// </summary>
	private readonly Dictionary<int, int> _touchIds = new Dictionary<int, int>();

	internal readonly Queue<GestureSample> GestureList = new Queue<GestureSample>();

	private TouchPanelCapabilities Capabilities;

	internal readonly GameWindow Window;

	/// <summary>
	/// Maximum distance a touch location can wiggle and 
	/// not be considered to have moved.
	/// </summary>
	internal const float TapJitterTolerance = 35f;

	internal static readonly TimeSpan TimeRequiredForHold = TimeSpan.FromMilliseconds(1024.0);

	/// <summary>
	/// The pinch touch locations.
	/// </summary>
	private readonly TouchLocation[] _pinchTouch = new TouchLocation[2];

	/// <summary>
	/// If true the pinch touch locations are valid and
	/// a pinch gesture has begun.
	/// </summary>
	private bool _pinchGestureStarted;

	/// <summary>
	/// Used to disable emitting of tap gestures.
	/// </summary>
	private bool _tapDisabled;

	/// <summary>
	/// Used to disable emitting of hold gestures.
	/// </summary>
	private bool _holdDisabled;

	private TouchLocation _lastTap;

	private GestureType _dragGestureStarted;

	/// <summary>
	/// The current timestamp that we use for setting the timestamp of new TouchLocations
	/// </summary>
	internal static TimeSpan CurrentTimestamp { get; set; }

	/// <summary>
	/// The window handle of the touch panel. Purely for Xna compatibility.
	/// </summary>
	public IntPtr WindowHandle { get; set; }

	/// <summary>
	/// Gets or sets the display height of the touch panel.
	/// </summary>
	public int DisplayHeight
	{
		get
		{
			return this._displaySize.Y;
		}
		set
		{
			this._displaySize.Y = value;
			this.UpdateTouchScale();
		}
	}

	/// <summary>
	/// Gets or sets the display orientation of the touch panel.
	/// </summary>
	public DisplayOrientation DisplayOrientation { get; set; }

	/// <summary>
	/// Gets or sets the display width of the touch panel.
	/// </summary>
	public int DisplayWidth
	{
		get
		{
			return this._displaySize.X;
		}
		set
		{
			this._displaySize.X = value;
			this.UpdateTouchScale();
		}
	}

	/// <summary>
	/// Gets or sets enabled gestures.
	/// </summary>
	public GestureType EnabledGestures { get; set; }

	public bool EnableMouseTouchPoint { get; set; }

	public bool EnableMouseGestures { get; set; }

	/// <summary>
	/// Returns true if a touch gesture is available.
	/// </summary>
	public bool IsGestureAvailable
	{
		get
		{
			this.UpdateGestures(stateChanged: false);
			return this.GestureList.Count > 0;
		}
	}

	internal TouchPanelState(GameWindow window)
	{
		this.Window = window;
	}

	/// <summary>
	/// Returns capabilities of touch panel device.
	/// </summary>
	/// <returns><see cref="T:Microsoft.Xna.Framework.Input.Touch.TouchPanelCapabilities" /></returns>
	public TouchPanelCapabilities GetCapabilities()
	{
		this.Capabilities.Initialize();
		return this.Capabilities;
	}

	/// <summary>
	/// Age all the touches, so any that were Pressed become Moved, and any that were Released are removed
	/// </summary>
	private void AgeTouches(List<TouchLocation> state)
	{
		for (int i = state.Count - 1; i >= 0; i--)
		{
			TouchLocation touch = state[i];
			switch (touch.State)
			{
			case TouchLocationState.Released:
				state.RemoveAt(i);
				break;
			case TouchLocationState.Moved:
			case TouchLocationState.Pressed:
				touch.AgeState();
				state[i] = touch;
				break;
			}
		}
	}

	/// <summary>
	/// Apply the given new touch to the state. If it is a Pressed it will be added as a new touch, otherwise we update the existing touch it matches
	/// </summary>
	private void ApplyTouch(List<TouchLocation> state, TouchLocation touch)
	{
		if (touch.State == TouchLocationState.Pressed)
		{
			state.Add(touch);
			return;
		}
		for (int i = 0; i < state.Count; i++)
		{
			TouchLocation existingTouch = state[i];
			if (existingTouch.Id == touch.Id)
			{
				if (existingTouch.State == TouchLocationState.Pressed && touch.State == TouchLocationState.Released && existingTouch.PressTimestamp != touch.Timestamp)
				{
					state.RemoveAt(i);
					break;
				}
				existingTouch.UpdateState(touch);
				state[i] = existingTouch;
				break;
			}
		}
	}

	public TouchCollection GetState()
	{
		for (int i = this._touchState.Count - 1; i >= 0; i--)
		{
			TouchLocation touch = this._touchState[i];
			if (touch.SameFrameReleased && touch.Timestamp < TouchPanelState.CurrentTimestamp && touch.State == TouchLocationState.Pressed)
			{
				this._touchState.RemoveAt(i);
			}
		}
		TouchCollection result = ((this._touchState.Count > 0) ? new TouchCollection(this._touchState.ToArray()) : TouchCollection.Empty);
		this.AgeTouches(this._touchState);
		return result;
	}

	internal void AddEvent(int id, TouchLocationState state, Vector2 position)
	{
		this.AddEvent(id, state, position, isMouse: false);
	}

	internal void AddEvent(int id, TouchLocationState state, Vector2 position, bool isMouse)
	{
		if (state == TouchLocationState.Pressed)
		{
			if (isMouse)
			{
				this._touchIds[id] = 1;
			}
			else
			{
				this._touchIds[id] = this._nextTouchId++;
			}
		}
		if (!this._touchIds.TryGetValue(id, out var touchId))
		{
			return;
		}
		if (!isMouse || this.EnableMouseTouchPoint || this.EnableMouseGestures)
		{
			TouchLocation evt = new TouchLocation(touchId, state, position * this._touchScale, TouchPanelState.CurrentTimestamp);
			if (!isMouse || this.EnableMouseTouchPoint)
			{
				this.ApplyTouch(this._touchState, evt);
			}
			if ((this.EnabledGestures != GestureType.None || this._gestureState.Count > 0) && (!isMouse || this.EnableMouseGestures))
			{
				this.ApplyTouch(this._gestureState, evt);
				if (this.EnabledGestures != GestureType.None)
				{
					this.UpdateGestures(stateChanged: true);
				}
				this.AgeTouches(this._gestureState);
			}
		}
		if (state == TouchLocationState.Released)
		{
			this._touchIds.Remove(id);
		}
	}

	private void UpdateTouchScale()
	{
		Vector2 windowSize = new Vector2(this.Window.ClientBounds.Width, this.Window.ClientBounds.Height);
		this._touchScale = new Vector2((float)this._displaySize.X / windowSize.X, (float)this._displaySize.Y / windowSize.Y);
	}

	/// <summary>
	/// This will release all touch locations.  It should only be 
	/// called on platforms where touch state is reset all at once.
	/// </summary>
	internal void ReleaseAllTouches()
	{
		int mostToRemove = Math.Max(this._touchState.Count, this._gestureState.Count);
		if (mostToRemove > 0)
		{
			List<TouchLocation> temp = new List<TouchLocation>(mostToRemove);
			temp.AddRange(this._touchState);
			foreach (TouchLocation touch in temp)
			{
				if (touch.State != TouchLocationState.Released)
				{
					this.ApplyTouch(this._touchState, new TouchLocation(touch.Id, TouchLocationState.Released, touch.Position, TouchPanelState.CurrentTimestamp));
				}
			}
			temp.Clear();
			temp.AddRange(this._gestureState);
			foreach (TouchLocation touch2 in temp)
			{
				if (touch2.State != TouchLocationState.Released)
				{
					this.ApplyTouch(this._gestureState, new TouchLocation(touch2.Id, TouchLocationState.Released, touch2.Position, TouchPanelState.CurrentTimestamp));
				}
			}
		}
		this._touchIds.Clear();
	}

	/// <summary>
	/// Returns the next available gesture on touch panel device.
	/// </summary>
	/// <returns><see cref="T:Microsoft.Xna.Framework.Input.Touch.GestureSample" /></returns>
	public GestureSample ReadGesture()
	{
		return this.GestureList.Dequeue();
	}

	private bool GestureIsEnabled(GestureType gestureType)
	{
		return (this.EnabledGestures & gestureType) != 0;
	}

	private void UpdateGestures(bool stateChanged)
	{
		int heldLocations = 0;
		foreach (TouchLocation item in this._gestureState)
		{
			heldLocations += ((item.State != TouchLocationState.Released) ? 1 : 0);
		}
		if (heldLocations > 1)
		{
			this._tapDisabled = true;
			this._holdDisabled = true;
		}
		foreach (TouchLocation touch in this._gestureState)
		{
			switch (touch.State)
			{
			case TouchLocationState.Moved:
			case TouchLocationState.Pressed:
			{
				if (touch.State == TouchLocationState.Pressed && this.ProcessDoubleTap(touch))
				{
					break;
				}
				if (this.GestureIsEnabled(GestureType.Pinch) && heldLocations > 1)
				{
					if (this._pinchTouch[0].State == TouchLocationState.Invalid || this._pinchTouch[0].Id == touch.Id)
					{
						this._pinchTouch[0] = touch;
					}
					else if (this._pinchTouch[1].State == TouchLocationState.Invalid || this._pinchTouch[1].Id == touch.Id)
					{
						this._pinchTouch[1] = touch;
					}
					break;
				}
				float dist = Vector2.Distance(touch.Position, touch.PressPosition);
				if (this._dragGestureStarted == GestureType.None && dist < 35f)
				{
					this.ProcessHold(touch);
				}
				else if (stateChanged)
				{
					this.ProcessDrag(touch);
				}
				break;
			}
			case TouchLocationState.Released:
				if (!stateChanged)
				{
					break;
				}
				if (this._pinchGestureStarted && (touch.Id == this._pinchTouch[0].Id || touch.Id == this._pinchTouch[1].Id))
				{
					if (this.GestureIsEnabled(GestureType.PinchComplete))
					{
						this.GestureList.Enqueue(new GestureSample(GestureType.PinchComplete, touch.Timestamp, Vector2.Zero, Vector2.Zero, Vector2.Zero, Vector2.Zero));
					}
					this._pinchGestureStarted = false;
					this._pinchTouch[0] = TouchLocation.Invalid;
					this._pinchTouch[1] = TouchLocation.Invalid;
				}
				else
				{
					if (heldLocations != 0)
					{
						break;
					}
					if (Vector2.Distance(touch.Position, touch.PressPosition) > 35f && touch.Velocity.Length() > 100f && this.GestureIsEnabled(GestureType.Flick))
					{
						this.GestureList.Enqueue(new GestureSample(GestureType.Flick, touch.Timestamp, Vector2.Zero, Vector2.Zero, touch.Velocity, Vector2.Zero));
					}
					if (this._dragGestureStarted != GestureType.None)
					{
						if (this.GestureIsEnabled(GestureType.DragComplete))
						{
							this.GestureList.Enqueue(new GestureSample(GestureType.DragComplete, touch.Timestamp, Vector2.Zero, Vector2.Zero, Vector2.Zero, Vector2.Zero));
						}
						this._dragGestureStarted = GestureType.None;
					}
					else
					{
						this.ProcessTap(touch);
					}
				}
				break;
			}
		}
		if (stateChanged)
		{
			if (this.GestureIsEnabled(GestureType.Pinch) && this._pinchTouch[0].State != TouchLocationState.Invalid && this._pinchTouch[1].State != TouchLocationState.Invalid)
			{
				this.ProcessPinch(this._pinchTouch);
			}
			else
			{
				this._pinchGestureStarted = false;
				this._pinchTouch[0] = TouchLocation.Invalid;
				this._pinchTouch[1] = TouchLocation.Invalid;
			}
			if (heldLocations == 0)
			{
				this._tapDisabled = false;
				this._holdDisabled = false;
				this._dragGestureStarted = GestureType.None;
			}
		}
	}

	private void ProcessHold(TouchLocation touch)
	{
		if (this.GestureIsEnabled(GestureType.Hold) && !this._holdDisabled && !(TouchPanelState.CurrentTimestamp - touch.PressTimestamp < TouchPanelState.TimeRequiredForHold))
		{
			this._holdDisabled = true;
			this.GestureList.Enqueue(new GestureSample(GestureType.Hold, touch.Timestamp, touch.Position, Vector2.Zero, Vector2.Zero, Vector2.Zero));
		}
	}

	private bool ProcessDoubleTap(TouchLocation touch)
	{
		if (!this.GestureIsEnabled(GestureType.DoubleTap) || this._tapDisabled || this._lastTap.State == TouchLocationState.Invalid)
		{
			return false;
		}
		if (Vector2.Distance(touch.Position, this._lastTap.Position) > 35f)
		{
			return false;
		}
		if ((touch.Timestamp - this._lastTap.Timestamp).TotalMilliseconds > 300.0)
		{
			return false;
		}
		this.GestureList.Enqueue(new GestureSample(GestureType.DoubleTap, touch.Timestamp, touch.Position, Vector2.Zero, Vector2.Zero, Vector2.Zero));
		this._tapDisabled = true;
		return true;
	}

	private void ProcessTap(TouchLocation touch)
	{
		if (!this._tapDisabled && !(Vector2.Distance(touch.PressPosition, touch.Position) > 35f) && !(TouchPanelState.CurrentTimestamp - touch.PressTimestamp > TouchPanelState.TimeRequiredForHold))
		{
			this._lastTap = touch;
			if (this.GestureIsEnabled(GestureType.Tap))
			{
				GestureSample tap = new GestureSample(GestureType.Tap, touch.Timestamp, touch.Position, Vector2.Zero, Vector2.Zero, Vector2.Zero);
				this.GestureList.Enqueue(tap);
			}
		}
	}

	private void ProcessDrag(TouchLocation touch)
	{
		bool dragH = this.GestureIsEnabled(GestureType.HorizontalDrag);
		bool dragV = this.GestureIsEnabled(GestureType.VerticalDrag);
		bool dragF = this.GestureIsEnabled(GestureType.FreeDrag);
		if ((!dragH && !dragV && !dragF) || touch.State != TouchLocationState.Moved || !touch.TryGetPreviousLocation(out var prevTouch))
		{
			return;
		}
		Vector2 delta = touch.Position - prevTouch.Position;
		if (this._dragGestureStarted != GestureType.FreeDrag)
		{
			bool isHorizontalDelta = Math.Abs(delta.X) > Math.Abs(delta.Y * 2f);
			bool isVerticalDelta = Math.Abs(delta.Y) > Math.Abs(delta.X * 2f);
			bool classify = this._dragGestureStarted == GestureType.None;
			if (dragH && ((classify && isHorizontalDelta) || this._dragGestureStarted == GestureType.HorizontalDrag))
			{
				delta.Y = 0f;
				this._dragGestureStarted = GestureType.HorizontalDrag;
			}
			else if (dragV && ((classify && isVerticalDelta) || this._dragGestureStarted == GestureType.VerticalDrag))
			{
				delta.X = 0f;
				this._dragGestureStarted = GestureType.VerticalDrag;
			}
			else if (dragF && classify)
			{
				this._dragGestureStarted = GestureType.FreeDrag;
			}
			else
			{
				this._dragGestureStarted = GestureType.DragComplete;
			}
		}
		if (this._dragGestureStarted != GestureType.None && this._dragGestureStarted != GestureType.DragComplete)
		{
			this._tapDisabled = true;
			this._holdDisabled = true;
			this.GestureList.Enqueue(new GestureSample(this._dragGestureStarted, touch.Timestamp, touch.Position, Vector2.Zero, delta, Vector2.Zero));
		}
	}

	private void ProcessPinch(TouchLocation[] touches)
	{
		if (!touches[0].TryGetPreviousLocation(out var prevPos0))
		{
			prevPos0 = touches[0];
		}
		if (!touches[1].TryGetPreviousLocation(out var prevPos1))
		{
			prevPos1 = touches[1];
		}
		Vector2 delta0 = touches[0].Position - prevPos0.Position;
		Vector2 delta1 = touches[1].Position - prevPos1.Position;
		TimeSpan timestamp = ((touches[0].Timestamp > touches[1].Timestamp) ? touches[0].Timestamp : touches[1].Timestamp);
		if (this._dragGestureStarted != GestureType.None)
		{
			if (this.GestureIsEnabled(GestureType.DragComplete))
			{
				this.GestureList.Enqueue(new GestureSample(GestureType.DragComplete, timestamp, Vector2.Zero, Vector2.Zero, Vector2.Zero, Vector2.Zero));
			}
			this._dragGestureStarted = GestureType.None;
		}
		this.GestureList.Enqueue(new GestureSample(GestureType.Pinch, timestamp, touches[0].Position, touches[1].Position, delta0, delta1));
		this._pinchGestureStarted = true;
		this._tapDisabled = true;
		this._holdDisabled = true;
	}
}
