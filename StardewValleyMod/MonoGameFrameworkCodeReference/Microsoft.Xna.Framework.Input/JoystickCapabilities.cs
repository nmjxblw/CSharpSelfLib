namespace Microsoft.Xna.Framework.Input;

/// <summary>
/// Describes joystick capabilities.
/// </summary>
public struct JoystickCapabilities
{
	/// <summary>
	/// Gets a value indicating whether the joystick is connected.
	/// </summary>
	/// <value><c>true</c> if the joystick is connected; otherwise, <c>false</c>.</value>
	public bool IsConnected { get; internal set; }

	/// <summary>
	/// Gets the unique identifier of the joystick.
	/// </summary>
	/// <value>String representing the unique identifier of the joystick.</value>
	public string Identifier { get; internal set; }

	/// <summary>
	/// Gets the joystick's display name.
	/// </summary>
	/// <value>String representing the display name of the joystick.</value>
	public string DisplayName { get; internal set; }

	/// <summary>
	/// Gets a value indicating if the joystick is a gamepad.
	/// </summary>
	/// <value><c>true</c> if the joystick is a gamepad; otherwise, <c>false</c>.</value>
	public bool IsGamepad { get; internal set; }

	/// <summary>
	/// Gets the axis count.
	/// </summary>
	/// <value>The number of axes that the joystick possesses.</value>
	public int AxisCount { get; internal set; }

	/// <summary>
	/// Gets the button count.
	/// </summary>
	/// <value>The number of buttons that the joystick possesses.</value>
	public int ButtonCount { get; internal set; }

	/// <summary>
	/// Gets the hat count.
	/// </summary>
	/// <value>The number of hats/dpads that the joystick possesses.</value>
	public int HatCount { get; internal set; }

	/// <summary>
	/// Determines whether a specified instance of <see cref="T:Microsoft.Xna.Framework.Input.JoystickCapabilities" />
	/// is equal to another specified <see cref="T:Microsoft.Xna.Framework.Input.JoystickCapabilities" />.
	/// </summary>
	/// <param name="left">The first <see cref="T:Microsoft.Xna.Framework.Input.JoystickCapabilities" /> to compare.</param>
	/// <param name="right">The second <see cref="T:Microsoft.Xna.Framework.Input.JoystickCapabilities" /> to compare.</param>
	/// <returns><c>true</c> if <c>left</c> and <c>right</c> are equal; otherwise, <c>false</c>.</returns>
	public static bool operator ==(JoystickCapabilities left, JoystickCapabilities right)
	{
		if (left.IsConnected == right.IsConnected && left.Identifier == right.Identifier && left.IsGamepad == right.IsGamepad && left.AxisCount == right.AxisCount && left.ButtonCount == right.ButtonCount)
		{
			return left.HatCount == right.HatCount;
		}
		return false;
	}

	/// <summary>
	/// Determines whether a specified instance of <see cref="T:Microsoft.Xna.Framework.Input.JoystickCapabilities" />
	/// is not equal to another specified <see cref="T:Microsoft.Xna.Framework.Input.JoystickCapabilities" />.
	/// </summary>
	/// <param name="left">The first <see cref="T:Microsoft.Xna.Framework.Input.JoystickCapabilities" /> to compare.</param>
	/// <param name="right">The second <see cref="T:Microsoft.Xna.Framework.Input.JoystickCapabilities" /> to compare.</param>
	/// <returns><c>true</c> if <c>left</c> and <c>right</c> are not equal; otherwise, <c>false</c>.</returns>
	public static bool operator !=(JoystickCapabilities left, JoystickCapabilities right)
	{
		return !(left == right);
	}

	/// <summary>
	/// Determines whether the specified <see cref="T:System.Object" /> is equal to the current <see cref="T:Microsoft.Xna.Framework.Input.JoystickCapabilities" />.
	/// </summary>
	/// <param name="obj">The <see cref="T:System.Object" /> to compare with the current <see cref="T:Microsoft.Xna.Framework.Input.JoystickCapabilities" />.</param>
	/// <returns><c>true</c> if the specified <see cref="T:System.Object" /> is equal to the current
	/// <see cref="T:Microsoft.Xna.Framework.Input.JoystickCapabilities" />; otherwise, <c>false</c>.</returns>
	public override bool Equals(object obj)
	{
		if (obj is JoystickCapabilities)
		{
			return this == (JoystickCapabilities)obj;
		}
		return false;
	}

	/// <summary>
	/// Serves as a hash function for a <see cref="T:Microsoft.Xna.Framework.Input.JoystickCapabilities" /> object.
	/// </summary>
	/// <returns>A hash code for this instance that is suitable for use in hashing algorithms and data structures such as a
	/// hash table.</returns>
	public override int GetHashCode()
	{
		return this.Identifier.GetHashCode();
	}

	/// <summary>
	/// Returns a <see cref="T:System.String" /> that represents the current <see cref="T:Microsoft.Xna.Framework.Input.JoystickCapabilities" />.
	/// </summary>
	/// <returns>A <see cref="T:System.String" /> that represents the current <see cref="T:Microsoft.Xna.Framework.Input.JoystickCapabilities" />.</returns>
	public override string ToString()
	{
		return "[JoystickCapabilities: IsConnected=" + this.IsConnected + ", Identifier=" + this.Identifier + ", DisplayName=" + this.DisplayName + ", IsGamepad=" + this.IsGamepad + " , AxisCount=" + this.AxisCount + ", ButtonCount=" + this.ButtonCount + ", HatCount=" + this.HatCount + "]";
	}
}
