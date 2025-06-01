using System;

namespace Microsoft.Xna.Framework.Input.Touch;

/// <summary>
/// Represents data from a multi-touch gesture over a span of time.
/// </summary>
public struct GestureSample
{
	private GestureType _gestureType;

	private TimeSpan _timestamp;

	private Vector2 _position;

	private Vector2 _position2;

	private Vector2 _delta;

	private Vector2 _delta2;

	/// <summary>
	/// Gets the type of the gesture.
	/// </summary>
	public GestureType GestureType => this._gestureType;

	/// <summary>
	/// Gets the starting time for this multi-touch gesture sample.
	/// </summary>
	public TimeSpan Timestamp => this._timestamp;

	/// <summary>
	/// Gets the position of the first touch-point in the gesture sample.
	/// </summary>
	public Vector2 Position => this._position;

	/// <summary>
	/// Gets the position of the second touch-point in the gesture sample.
	/// </summary>
	public Vector2 Position2 => this._position2;

	/// <summary>
	/// Gets the delta information for the first touch-point in the gesture sample.
	/// </summary>
	public Vector2 Delta => this._delta;

	/// <summary>
	/// Gets the delta information for the second touch-point in the gesture sample.
	/// </summary>
	public Vector2 Delta2 => this._delta2;

	/// <summary>
	/// Initializes a new <see cref="T:Microsoft.Xna.Framework.Input.Touch.GestureSample" />.
	/// </summary>
	/// <param name="gestureType"><see cref="P:Microsoft.Xna.Framework.Input.Touch.GestureSample.GestureType" /></param>
	/// <param name="timestamp"></param>
	/// <param name="position"></param>
	/// <param name="position2"></param>
	/// <param name="delta"></param>
	/// <param name="delta2"></param>
	public GestureSample(GestureType gestureType, TimeSpan timestamp, Vector2 position, Vector2 position2, Vector2 delta, Vector2 delta2)
	{
		this._gestureType = gestureType;
		this._timestamp = timestamp;
		this._position = position;
		this._position2 = position2;
		this._delta = delta;
		this._delta2 = delta2;
	}
}
