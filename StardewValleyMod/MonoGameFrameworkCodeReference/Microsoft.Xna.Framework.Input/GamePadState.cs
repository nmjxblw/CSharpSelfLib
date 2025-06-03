namespace Microsoft.Xna.Framework.Input;

/// <summary>
/// Represents specific information about the state of the controller,
/// including the current state of buttons and sticks.
///
/// This is implemented as a partial struct to allow for individual platforms
/// to offer additional data without separate state queries to GamePad.
/// </summary>
public struct GamePadState
{
	/// <summary>
	/// The default initialized gamepad state.
	/// </summary>
	public static readonly GamePadState Default;

	/// <summary>
	/// Gets a value indicating if the controller is connected.
	/// </summary>
	/// <value><c>true</c> if it is connected; otherwise, <c>false</c>.</value>
	public bool IsConnected { get; internal set; }

	/// <summary>
	/// Gets the packet number associated with this state.
	/// </summary>
	/// <value>The packet number.</value>
	public int PacketNumber { get; internal set; }

	/// <summary>
	/// Gets a structure that identifies what buttons on the controller are pressed.
	/// </summary>
	/// <value>The buttons structure.</value>
	public GamePadButtons Buttons { get; internal set; }

	/// <summary>
	/// Gets a structure that identifies what directions of the directional pad on the controller are pressed.
	/// </summary>
	/// <value>The directional pad structure.</value>
	public GamePadDPad DPad { get; internal set; }

	/// <summary>
	/// Gets a structure that indicates the position of the controller sticks (thumbsticks).
	/// </summary>
	/// <value>The thumbsticks position.</value>
	public GamePadThumbSticks ThumbSticks { get; internal set; }

	/// <summary>
	/// Gets a structure that identifies the position of triggers on the controller.
	/// </summary>
	/// <value>Positions of the triggers.</value>
	public GamePadTriggers Triggers { get; internal set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="T:Microsoft.Xna.Framework.Input.GamePadState" /> struct
	/// using the specified GamePadThumbSticks, GamePadTriggers, GamePadButtons, and GamePadDPad.
	/// </summary>
	/// <param name="thumbSticks">Initial thumbstick state.</param>
	/// <param name="triggers">Initial trigger state..</param>
	/// <param name="buttons">Initial button state.</param>
	/// <param name="dPad">Initial directional pad state.</param>
	public GamePadState(GamePadThumbSticks thumbSticks, GamePadTriggers triggers, GamePadButtons buttons, GamePadDPad dPad)
	{
		this = default(GamePadState);
		this.ThumbSticks = thumbSticks;
		this.Triggers = triggers;
		this.Buttons = buttons;
		this.DPad = dPad;
		this.IsConnected = true;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:Microsoft.Xna.Framework.Input.GamePadState" /> struct
	/// using the specified stick, trigger, and button values.
	/// </summary>
	/// <param name="leftThumbStick">Left stick value. Each axis is clamped between −1.0 and 1.0.</param>
	/// <param name="rightThumbStick">Right stick value. Each axis is clamped between −1.0 and 1.0.</param>
	/// <param name="leftTrigger">Left trigger value. This value is clamped between 0.0 and 1.0.</param>
	/// <param name="rightTrigger">Right trigger value. This value is clamped between 0.0 and 1.0.</param>
	/// <param name="button">Button(s) to initialize as pressed.</param>
	public GamePadState(Vector2 leftThumbStick, Vector2 rightThumbStick, float leftTrigger, float rightTrigger, Buttons button)
		: this(new GamePadThumbSticks(leftThumbStick, rightThumbStick), new GamePadTriggers(leftTrigger, rightTrigger), new GamePadButtons(button), new GamePadDPad(button))
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:Microsoft.Xna.Framework.Input.GamePadState" /> struct
	/// using the specified stick, trigger, and button values.
	/// </summary>
	/// <param name="leftThumbStick">Left stick value. Each axis is clamped between −1.0 and 1.0.</param>
	/// <param name="rightThumbStick">Right stick value. Each axis is clamped between −1.0 and 1.0.</param>
	/// <param name="leftTrigger">Left trigger value. This value is clamped between 0.0 and 1.0.</param>
	/// <param name="rightTrigger">Right trigger value. This value is clamped between 0.0 and 1.0.</param>
	/// <param name="buttons"> Array of Buttons to initialize as pressed.</param>
	public GamePadState(Vector2 leftThumbStick, Vector2 rightThumbStick, float leftTrigger, float rightTrigger, Buttons[] buttons)
		: this(new GamePadThumbSticks(leftThumbStick, rightThumbStick), new GamePadTriggers(leftTrigger, rightTrigger), new GamePadButtons(buttons), new GamePadDPad(buttons))
	{
	}

	/// <summary>
	/// Gets the button mask along with 'virtual buttons' like LeftThumbstickLeft.
	/// </summary>
	private Buttons GetVirtualButtons()
	{
		Buttons result = this.Buttons._buttons;
		result |= this.ThumbSticks._virtualButtons;
		if (this.DPad.Down == ButtonState.Pressed)
		{
			result |= Microsoft.Xna.Framework.Input.Buttons.DPadDown;
		}
		if (this.DPad.Up == ButtonState.Pressed)
		{
			result |= Microsoft.Xna.Framework.Input.Buttons.DPadUp;
		}
		if (this.DPad.Left == ButtonState.Pressed)
		{
			result |= Microsoft.Xna.Framework.Input.Buttons.DPadLeft;
		}
		if (this.DPad.Right == ButtonState.Pressed)
		{
			result |= Microsoft.Xna.Framework.Input.Buttons.DPadRight;
		}
		return result;
	}

	/// <summary>
	/// Determines whether specified input device buttons are pressed in this GamePadState.
	/// </summary>
	/// <returns><c>true</c>, if button was pressed, <c>false</c> otherwise.</returns>
	/// <param name="button">Buttons to query. Specify a single button, or combine multiple buttons using a bitwise OR operation.</param>
	public bool IsButtonDown(Buttons button)
	{
		return (this.GetVirtualButtons() & button) == button;
	}

	/// <summary>
	/// Determines whether specified input device buttons are released (not pressed) in this GamePadState.
	/// </summary>
	/// <returns><c>true</c>, if button was released (not pressed), <c>false</c> otherwise.</returns>
	/// <param name="button">Buttons to query. Specify a single button, or combine multiple buttons using a bitwise OR operation.</param>
	public bool IsButtonUp(Buttons button)
	{
		return (this.GetVirtualButtons() & button) != button;
	}

	/// <summary>
	/// Determines whether a specified instance of <see cref="T:Microsoft.Xna.Framework.Input.GamePadState" /> is equal
	/// to another specified <see cref="T:Microsoft.Xna.Framework.Input.GamePadState" />.
	/// </summary>
	/// <param name="left">The first <see cref="T:Microsoft.Xna.Framework.Input.GamePadState" /> to compare.</param>
	/// <param name="right">The second <see cref="T:Microsoft.Xna.Framework.Input.GamePadState" /> to compare.</param>
	/// <returns><c>true</c> if <c>left</c> and <c>right</c> are equal; otherwise, <c>false</c>.</returns>
	public static bool operator ==(GamePadState left, GamePadState right)
	{
		if (left.IsConnected == right.IsConnected && left.PacketNumber == right.PacketNumber && left.Buttons == right.Buttons && left.DPad == right.DPad && left.ThumbSticks == right.ThumbSticks)
		{
			return left.Triggers == right.Triggers;
		}
		return false;
	}

	/// <summary>
	/// Determines whether a specified instance of <see cref="T:Microsoft.Xna.Framework.Input.GamePadState" /> is not
	/// equal to another specified <see cref="T:Microsoft.Xna.Framework.Input.GamePadState" />.
	/// </summary>
	/// <param name="left">The first <see cref="T:Microsoft.Xna.Framework.Input.GamePadState" /> to compare.</param>
	/// <param name="right">The second <see cref="T:Microsoft.Xna.Framework.Input.GamePadState" /> to compare.</param>
	/// <returns><c>true</c> if <c>left</c> and <c>right</c> are not equal; otherwise, <c>false</c>.</returns>
	public static bool operator !=(GamePadState left, GamePadState right)
	{
		return !(left == right);
	}

	/// <summary>
	/// Determines whether the specified <see cref="T:System.Object" /> is equal to the current <see cref="T:Microsoft.Xna.Framework.Input.GamePadState" />.
	/// </summary>
	/// <param name="obj">The <see cref="T:System.Object" /> to compare with the current <see cref="T:Microsoft.Xna.Framework.Input.GamePadState" />.</param>
	/// <returns><c>true</c> if the specified <see cref="T:System.Object" /> is equal to the current
	/// <see cref="T:Microsoft.Xna.Framework.Input.GamePadState" />; otherwise, <c>false</c>.</returns>
	public override bool Equals(object obj)
	{
		if (obj is GamePadState)
		{
			return this == (GamePadState)obj;
		}
		return false;
	}

	/// <summary>
	/// Serves as a hash function for a <see cref="T:Microsoft.Xna.Framework.Input.GamePadState" /> object.
	/// </summary>
	/// <returns>A hash code for this instance that is suitable for use in hashing algorithms and data structures such as a
	/// hash table.</returns>
	public override int GetHashCode()
	{
		return (((((((this.PacketNumber * 397) ^ this.Buttons.GetHashCode()) * 397) ^ this.DPad.GetHashCode()) * 397) ^ this.ThumbSticks.GetHashCode()) * 397) ^ this.Triggers.GetHashCode();
	}

	/// <summary>
	/// Returns a <see cref="T:System.String" /> that represents the current <see cref="T:Microsoft.Xna.Framework.Input.GamePadState" />.
	/// </summary>
	/// <returns>A <see cref="T:System.String" /> that represents the current <see cref="T:Microsoft.Xna.Framework.Input.GamePadState" />.</returns>
	public override string ToString()
	{
		if (!this.IsConnected)
		{
			return "[GamePadState: IsConnected = 0]";
		}
		return "[GamePadState: IsConnected=" + (this.IsConnected ? "1" : "0") + ", PacketNumber=" + this.PacketNumber.ToString("00000") + ", Buttons=" + this.Buttons.ToString() + ", DPad=" + this.DPad.ToString() + ", ThumbSticks=" + this.ThumbSticks.ToString() + ", Triggers=" + this.Triggers.ToString() + "]";
	}
}
