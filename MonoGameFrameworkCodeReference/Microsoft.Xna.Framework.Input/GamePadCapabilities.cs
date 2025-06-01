namespace Microsoft.Xna.Framework.Input;

/// <summary>
/// A stuct that represents the controller capabilities.
/// </summary>
public struct GamePadCapabilities
{
	/// <summary>
	/// Gets a value indicating if the controller is connected.
	/// </summary>
	/// <value><c>true</c> if it is connected; otherwise, <c>false</c>.</value>
	public bool IsConnected { get; internal set; }

	/// <summary>
	/// Gets the gamepad display name.
	///
	/// This property is not available in XNA.
	/// </summary>
	/// <value>String representing the display name of the gamepad.</value>
	public string DisplayName { get; internal set; }

	/// <summary>
	/// Gets the unique identifier of the gamepad.
	///
	/// This property is not available in XNA.
	/// </summary>
	/// <value>String representing the unique identifier of the gamepad.</value>
	public string Identifier { get; internal set; }

	/// <summary>
	/// Gets a value indicating whether the controller has the button A.
	/// </summary>
	/// <value><c>true</c> if it has the button A; otherwise, <c>false</c>.</value>
	public bool HasAButton { get; internal set; }

	/// <summary>
	/// Gets a value indicating whether the controller has the button Back.
	/// </summary>
	/// <value><c>true</c> if it has the button Back; otherwise, <c>false</c>.</value>
	public bool HasBackButton { get; internal set; }

	/// <summary>
	/// Gets a value indicating whether the controller has the button B.
	/// </summary>
	/// <value><c>true</c> if it has the button B; otherwise, <c>false</c>.</value>
	public bool HasBButton { get; internal set; }

	/// <summary>
	/// Gets a value indicating whether the controller has the directional pad down button.
	/// </summary>
	/// <value><c>true</c> if it has the directional pad down button; otherwise, <c>false</c>.</value>
	public bool HasDPadDownButton { get; internal set; }

	/// <summary>
	/// Gets a value indicating whether the controller has the directional pad left button.
	/// </summary>
	/// <value><c>true</c> if it has the directional pad left button; otherwise, <c>false</c>.</value>
	public bool HasDPadLeftButton { get; internal set; }

	/// <summary>
	/// Gets a value indicating whether the controller has the directional pad right button.
	/// </summary>
	/// <value><c>true</c> if it has the directional pad right button; otherwise, <c>false</c>.</value>
	public bool HasDPadRightButton { get; internal set; }

	/// <summary>
	/// Gets a value indicating whether the controller has the directional pad up button.
	/// </summary>
	/// <value><c>true</c> if it has the directional pad up button; otherwise, <c>false</c>.</value>
	public bool HasDPadUpButton { get; internal set; }

	/// <summary>
	/// Gets a value indicating whether the controller has the left shoulder button.
	/// </summary>
	/// <value><c>true</c> if it has the left shoulder button; otherwise, <c>false</c>.</value>
	public bool HasLeftShoulderButton { get; internal set; }

	/// <summary>
	/// Gets a value indicating whether the controller has the left stick button.
	/// </summary>
	/// <value><c>true</c> if it has the left stick button; otherwise, <c>false</c>.</value>
	public bool HasLeftStickButton { get; internal set; }

	/// <summary>
	/// Gets a value indicating whether the controller has the right shoulder button.
	/// </summary>
	/// <value><c>true</c> if it has the right shoulder button; otherwise, <c>false</c>.</value>
	public bool HasRightShoulderButton { get; internal set; }

	/// <summary>
	/// Gets a value indicating whether the controller has the right stick button.
	/// </summary>
	/// <value><c>true</c> if it has the right stick button; otherwise, <c>false</c>.</value>
	public bool HasRightStickButton { get; internal set; }

	/// <summary>
	/// Gets a value indicating whether the controller has the button Start.
	/// </summary>
	/// <value><c>true</c> if it has the button Start; otherwise, <c>false</c>.</value>
	public bool HasStartButton { get; internal set; }

	/// <summary>
	/// Gets a value indicating whether the controller has the button X.
	/// </summary>
	/// <value><c>true</c> if it has the button X; otherwise, <c>false</c>.</value>
	public bool HasXButton { get; internal set; }

	/// <summary>
	/// Gets a value indicating whether the controller has the button Y.
	/// </summary>
	/// <value><c>true</c> if it has the button Y; otherwise, <c>false</c>.</value>
	public bool HasYButton { get; internal set; }

	/// <summary>
	/// Gets a value indicating whether the controller has the guide button.
	/// </summary>
	/// <value><c>true</c> if it has the guide button; otherwise, <c>false</c>.</value>
	public bool HasBigButton { get; internal set; }

	/// <summary>
	/// Gets a value indicating whether the controller has X axis for the left stick (thumbstick) button.
	/// </summary>
	/// <value><c>true</c> if it has X axis for the left stick (thumbstick) button; otherwise, <c>false</c>.</value>
	public bool HasLeftXThumbStick { get; internal set; }

	/// <summary>
	/// Gets a value indicating whether the controller has Y axis for the left stick (thumbstick) button.
	/// </summary>
	/// <value><c>true</c> if it has Y axis for the left stick (thumbstick) button; otherwise, <c>false</c>.</value>
	public bool HasLeftYThumbStick { get; internal set; }

	/// <summary>
	/// Gets a value indicating whether the controller has X axis for the right stick (thumbstick) button.
	/// </summary>
	/// <value><c>true</c> if it has X axis for the right stick (thumbstick) button; otherwise, <c>false</c>.</value>
	public bool HasRightXThumbStick { get; internal set; }

	/// <summary>
	/// Gets a value indicating whether the controller has Y axis for the right stick (thumbstick) button.
	/// </summary>
	/// <value><c>true</c> if it has Y axis for the right stick (thumbstick) button; otherwise, <c>false</c>.</value>
	public bool HasRightYThumbStick { get; internal set; }

	/// <summary>
	/// Gets a value indicating whether the controller has the left trigger button.
	/// </summary>
	/// <value><c>true</c> if it has the left trigger button; otherwise, <c>false</c>.</value>
	public bool HasLeftTrigger { get; internal set; }

	/// <summary>
	/// Gets a value indicating whether the controller has the right trigger button.
	/// </summary>
	/// <value><c>true</c> if it has the right trigger button; otherwise, <c>false</c>.</value>
	public bool HasRightTrigger { get; internal set; }

	/// <summary>
	/// Gets a value indicating whether the controller has the left vibration motor.
	/// </summary>
	/// <value><c>true</c> if it has the left vibration motor; otherwise, <c>false</c>.</value>
	public bool HasLeftVibrationMotor { get; internal set; }

	/// <summary>
	/// Gets a value indicating whether the controller has the right vibration motor.
	/// </summary>
	/// <value><c>true</c> if it has the right vibration motor; otherwise, <c>false</c>.</value>
	public bool HasRightVibrationMotor { get; internal set; }

	/// <summary>
	/// Gets a value indicating whether the controller has a microphone.
	/// </summary>
	/// <value><c>true</c> if it has a microphone; otherwise, <c>false</c>.</value>
	public bool HasVoiceSupport { get; internal set; }

	/// <summary>
	/// Gets the type of the controller.
	/// </summary>
	/// <value>A <see cref="P:Microsoft.Xna.Framework.Input.GamePadCapabilities.GamePadType" /> representing the controller type..</value>
	public GamePadType GamePadType { get; internal set; }

	/// <summary>
	/// Determines whether a specified instance of <see cref="T:Microsoft.Xna.Framework.Input.GamePadCapabilities" />
	/// is equal to another specified <see cref="T:Microsoft.Xna.Framework.Input.GamePadCapabilities" />.
	/// </summary>
	/// <param name="left">The first <see cref="T:Microsoft.Xna.Framework.Input.GamePadCapabilities" /> to compare.</param>
	/// <param name="right">The second <see cref="T:Microsoft.Xna.Framework.Input.GamePadCapabilities" /> to compare.</param>
	/// <returns><c>true</c> if <c>left</c> and <c>right</c> are equal; otherwise, <c>false</c>.</returns>
	public static bool operator ==(GamePadCapabilities left, GamePadCapabilities right)
	{
		return (byte)(1u & ((left.DisplayName == right.DisplayName) ? 1u : 0u) & ((left.Identifier == right.Identifier) ? 1u : 0u) & ((left.IsConnected == right.IsConnected) ? 1u : 0u) & ((left.HasAButton == right.HasAButton) ? 1u : 0u) & ((left.HasBackButton == right.HasBackButton) ? 1u : 0u) & ((left.HasBButton == right.HasBButton) ? 1u : 0u) & ((left.HasDPadDownButton == right.HasDPadDownButton) ? 1u : 0u) & ((left.HasDPadLeftButton == right.HasDPadLeftButton) ? 1u : 0u) & ((left.HasDPadRightButton == right.HasDPadRightButton) ? 1u : 0u) & ((left.HasDPadUpButton == right.HasDPadUpButton) ? 1u : 0u) & ((left.HasLeftShoulderButton == right.HasLeftShoulderButton) ? 1u : 0u) & ((left.HasLeftStickButton == right.HasLeftStickButton) ? 1u : 0u) & ((left.HasRightShoulderButton == right.HasRightShoulderButton) ? 1u : 0u) & ((left.HasRightStickButton == right.HasRightStickButton) ? 1u : 0u) & ((left.HasStartButton == right.HasStartButton) ? 1u : 0u) & ((left.HasXButton == right.HasXButton) ? 1u : 0u) & ((left.HasYButton == right.HasYButton) ? 1u : 0u) & ((left.HasBigButton == right.HasBigButton) ? 1u : 0u) & ((left.HasLeftXThumbStick == right.HasLeftXThumbStick) ? 1u : 0u) & ((left.HasLeftYThumbStick == right.HasLeftYThumbStick) ? 1u : 0u) & ((left.HasRightXThumbStick == right.HasRightXThumbStick) ? 1u : 0u) & ((left.HasRightYThumbStick == right.HasRightYThumbStick) ? 1u : 0u) & ((left.HasLeftTrigger == right.HasLeftTrigger) ? 1u : 0u) & ((left.HasRightTrigger == right.HasRightTrigger) ? 1u : 0u) & ((left.HasLeftVibrationMotor == right.HasLeftVibrationMotor) ? 1u : 0u) & ((left.HasRightVibrationMotor == right.HasRightVibrationMotor) ? 1u : 0u) & ((left.HasVoiceSupport == right.HasVoiceSupport) ? 1u : 0u) & ((left.GamePadType == right.GamePadType) ? 1u : 0u)) != 0;
	}

	/// <summary>
	/// Determines whether a specified instance of <see cref="T:Microsoft.Xna.Framework.Input.GamePadCapabilities" />
	/// is not equal to another specified <see cref="T:Microsoft.Xna.Framework.Input.GamePadCapabilities" />.
	/// </summary>
	/// <param name="left">The first <see cref="T:Microsoft.Xna.Framework.Input.GamePadCapabilities" /> to compare.</param>
	/// <param name="right">The second <see cref="T:Microsoft.Xna.Framework.Input.GamePadCapabilities" /> to compare.</param>
	/// <returns><c>true</c> if <c>left</c> and <c>right</c> are not equal; otherwise, <c>false</c>.</returns>
	public static bool operator !=(GamePadCapabilities left, GamePadCapabilities right)
	{
		return !(left == right);
	}

	/// <summary>
	/// Determines whether the specified <see cref="T:System.Object" /> is equal to the current <see cref="T:Microsoft.Xna.Framework.Input.GamePadCapabilities" />.
	/// </summary>
	/// <param name="obj">The <see cref="T:System.Object" /> to compare with the current <see cref="T:Microsoft.Xna.Framework.Input.GamePadCapabilities" />.</param>
	/// <returns><c>true</c> if the specified <see cref="T:System.Object" /> is equal to the current
	/// <see cref="T:Microsoft.Xna.Framework.Input.GamePadCapabilities" />; otherwise, <c>false</c>.</returns>
	public override bool Equals(object obj)
	{
		if (obj is GamePadCapabilities)
		{
			return this == (GamePadCapabilities)obj;
		}
		return false;
	}

	/// <summary>
	/// Serves as a hash function for a <see cref="T:Microsoft.Xna.Framework.Input.GamePadCapabilities" /> object.
	/// </summary>
	/// <returns>A hash code for this instance that is suitable for use in hashing algorithms and data structures such as a
	/// hash table.</returns>
	public override int GetHashCode()
	{
		return this.Identifier.GetHashCode();
	}

	/// <summary>
	/// Returns a <see cref="T:System.String" /> that represents the current <see cref="T:Microsoft.Xna.Framework.Input.GamePadCapabilities" />.
	/// </summary>
	/// <returns>A <see cref="T:System.String" /> that represents the current <see cref="T:Microsoft.Xna.Framework.Input.GamePadCapabilities" />.</returns>
	public override string ToString()
	{
		return "[GamePadCapabilities: IsConnected=" + this.IsConnected + ", DisplayName=" + this.DisplayName + ", Identifier=" + this.Identifier + ", HasAButton=" + this.HasAButton + ", HasBackButton=" + this.HasBackButton + ", HasBButton=" + this.HasBButton + ", HasDPadDownButton=" + this.HasDPadDownButton + ", HasDPadLeftButton=" + this.HasDPadLeftButton + ", HasDPadRightButton=" + this.HasDPadRightButton + ", HasDPadUpButton=" + this.HasDPadUpButton + ", HasLeftShoulderButton=" + this.HasLeftShoulderButton + ", HasLeftStickButton=" + this.HasLeftStickButton + ", HasRightShoulderButton=" + this.HasRightShoulderButton + ", HasRightStickButton=" + this.HasRightStickButton + ", HasStartButton=" + this.HasStartButton + ", HasXButton=" + this.HasXButton + ", HasYButton=" + this.HasYButton + ", HasBigButton=" + this.HasBigButton + ", HasLeftXThumbStick=" + this.HasLeftXThumbStick + ", HasLeftYThumbStick=" + this.HasLeftYThumbStick + ", HasRightXThumbStick=" + this.HasRightXThumbStick + ", HasRightYThumbStick=" + this.HasRightYThumbStick + ", HasLeftTrigger=" + this.HasLeftTrigger + ", HasRightTrigger=" + this.HasRightTrigger + ", HasLeftVibrationMotor=" + this.HasLeftVibrationMotor + ", HasRightVibrationMotor=" + this.HasRightVibrationMotor + ", HasVoiceSupport=" + this.HasVoiceSupport + ", GamePadType=" + this.GamePadType.ToString() + "]";
	}
}
