namespace Microsoft.Xna.Framework.Input;

/// <summary>
/// A struct that represents the current stick (thumbstick) states for the controller.
/// </summary>
public struct GamePadThumbSticks
{
	private const float leftThumbDeadZone = 0.24f;

	private const float rightThumbDeadZone = 0.265f;

	internal readonly Buttons _virtualButtons;

	private readonly Vector2 _left;

	private readonly Vector2 _right;

	/// <summary>
	/// Gets a value indicating the position of the left stick (thumbstick). 
	/// </summary>
	/// <value>A <see cref="T:Microsoft.Xna.Framework.Vector2" /> indicating the current position of the left stick (thumbstick).</value>
	public Vector2 Left => this._left;

	/// <summary>
	/// Gets a value indicating the position of the right stick (thumbstick). 
	/// </summary>
	/// <value>A <see cref="T:Microsoft.Xna.Framework.Vector2" /> indicating the current position of the right stick (thumbstick).</value>
	public Vector2 Right => this._right;

	public GamePadThumbSticks(Vector2 leftPosition, Vector2 rightPosition)
		: this(leftPosition, rightPosition, GamePadDeadZone.None, GamePadDeadZone.None)
	{
	}

	internal GamePadThumbSticks(Vector2 leftPosition, Vector2 rightPosition, GamePadDeadZone leftDeadZoneMode, GamePadDeadZone rightDeadZoneMode)
	{
		this = default(GamePadThumbSticks);
		this._left = this.ApplyDeadZone(leftDeadZoneMode, 0.24f, leftPosition);
		this._right = this.ApplyDeadZone(rightDeadZoneMode, 0.265f, rightPosition);
		this._virtualButtons = (Buttons)0;
		if (leftPosition.X < -0.24f)
		{
			this._virtualButtons |= Buttons.LeftThumbstickLeft;
		}
		else if (leftPosition.X > 0.24f)
		{
			this._virtualButtons |= Buttons.LeftThumbstickRight;
		}
		if (leftPosition.Y < -0.24f)
		{
			this._virtualButtons |= Buttons.LeftThumbstickDown;
		}
		else if (leftPosition.Y > 0.24f)
		{
			this._virtualButtons |= Buttons.LeftThumbstickUp;
		}
		if (rightPosition.X < -0.265f)
		{
			this._virtualButtons |= Buttons.RightThumbstickLeft;
		}
		else if (rightPosition.X > 0.265f)
		{
			this._virtualButtons |= Buttons.RightThumbstickRight;
		}
		if (rightPosition.Y < -0.265f)
		{
			this._virtualButtons |= Buttons.RightThumbstickDown;
		}
		else if (rightPosition.Y > 0.265f)
		{
			this._virtualButtons |= Buttons.RightThumbstickUp;
		}
	}

	private Vector2 ApplyDeadZone(GamePadDeadZone deadZoneMode, float deadZone, Vector2 thumbstickPosition)
	{
		switch (deadZoneMode)
		{
		case GamePadDeadZone.IndependentAxes:
			thumbstickPosition = this.ExcludeIndependentAxesDeadZone(thumbstickPosition, deadZone);
			break;
		case GamePadDeadZone.Circular:
			thumbstickPosition = this.ExcludeCircularDeadZone(thumbstickPosition, deadZone);
			break;
		}
		if (deadZoneMode == GamePadDeadZone.Circular)
		{
			if (thumbstickPosition.LengthSquared() > 1f)
			{
				thumbstickPosition.Normalize();
			}
		}
		else
		{
			thumbstickPosition = new Vector2(MathHelper.Clamp(thumbstickPosition.X, -1f, 1f), MathHelper.Clamp(thumbstickPosition.Y, -1f, 1f));
		}
		return thumbstickPosition;
	}

	private Vector2 ExcludeIndependentAxesDeadZone(Vector2 value, float deadZone)
	{
		return new Vector2(this.ExcludeAxisDeadZone(value.X, deadZone), this.ExcludeAxisDeadZone(value.Y, deadZone));
	}

	private float ExcludeAxisDeadZone(float value, float deadZone)
	{
		if (value < 0f - deadZone)
		{
			value += deadZone;
		}
		else
		{
			if (!(value > deadZone))
			{
				return 0f;
			}
			value -= deadZone;
		}
		return value / (1f - deadZone);
	}

	private Vector2 ExcludeCircularDeadZone(Vector2 value, float deadZone)
	{
		float originalLength = value.Length();
		if (originalLength <= deadZone)
		{
			return Vector2.Zero;
		}
		float newLength = (originalLength - deadZone) / (1f - deadZone);
		return value * (newLength / originalLength);
	}

	/// <summary>
	/// Determines whether two specified instances of <see cref="T:Microsoft.Xna.Framework.Input.GamePadThumbSticks" /> are equal.
	/// </summary>
	/// <param name="left">The first object to compare.</param>
	/// <param name="right">The second object to compare.</param>
	/// <returns>true if <paramref name="left" /> and <paramref name="right" /> are equal; otherwise, false.</returns>
	public static bool operator ==(GamePadThumbSticks left, GamePadThumbSticks right)
	{
		if (left.Left == right.Left)
		{
			return left.Right == right.Right;
		}
		return false;
	}

	/// <summary>
	/// Determines whether two specified instances of <see cref="T:Microsoft.Xna.Framework.Input.GamePadThumbSticks" /> are not equal.
	/// </summary>
	/// <param name="left">The first object to compare.</param>
	/// <param name="right">The second object to compare.</param>
	/// <returns>true if <paramref name="left" /> and <paramref name="right" /> are not equal; otherwise, false.</returns>
	public static bool operator !=(GamePadThumbSticks left, GamePadThumbSticks right)
	{
		return !(left == right);
	}

	/// <summary>
	/// Returns a value indicating whether this instance is equal to a specified object.
	/// </summary>
	/// <param name="obj">An object to compare to this instance.</param>
	/// <returns>true if <paramref name="obj" /> is a <see cref="T:Microsoft.Xna.Framework.Input.GamePadThumbSticks" /> and has the same value as this instance; otherwise, false.</returns>
	public override bool Equals(object obj)
	{
		if (obj is GamePadThumbSticks)
		{
			return this == (GamePadThumbSticks)obj;
		}
		return false;
	}

	/// <summary>
	/// Serves as a hash function for a <see cref="T:Microsoft.Xna.Framework.Input.GamePadThumbSticks" /> object.
	/// </summary>
	/// <returns>A hash code for this instance that is suitable for use in hashing algorithms and data structures such as a
	/// hash table.</returns>
	public override int GetHashCode()
	{
		return (this.Left.GetHashCode() * 397) ^ this.Right.GetHashCode();
	}

	/// <summary>
	/// Returns a <see cref="T:System.String" /> that represents the current <see cref="T:Microsoft.Xna.Framework.Input.GamePadThumbSticks" />.
	/// </summary>
	/// <returns>A <see cref="T:System.String" /> that represents the current <see cref="T:Microsoft.Xna.Framework.Input.GamePadThumbSticks" />.</returns>
	public override string ToString()
	{
		return "[GamePadThumbSticks: Left=" + this.Left.ToString() + ", Right=" + this.Right.ToString() + "]";
	}
}
