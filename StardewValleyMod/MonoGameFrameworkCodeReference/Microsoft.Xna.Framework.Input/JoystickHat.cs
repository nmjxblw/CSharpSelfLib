namespace Microsoft.Xna.Framework.Input;

/// <summary>
/// Describes joystick hat state.
/// </summary>
public struct JoystickHat
{
	/// <summary>
	/// Gets if joysticks hat "down" is pressed.
	/// </summary>
	/// <value><see cref="F:Microsoft.Xna.Framework.Input.ButtonState.Pressed" /> if the button is pressed otherwise, <see cref="F:Microsoft.Xna.Framework.Input.ButtonState.Released" />.</value>
	public ButtonState Down { get; internal set; }

	/// <summary>
	/// Gets if joysticks hat "left" is pressed.
	/// </summary>
	/// <value><see cref="F:Microsoft.Xna.Framework.Input.ButtonState.Pressed" /> if the button is pressed otherwise, <see cref="F:Microsoft.Xna.Framework.Input.ButtonState.Released" />.</value>
	public ButtonState Left { get; internal set; }

	/// <summary>
	/// Gets if joysticks hat "right" is pressed.
	/// </summary>
	/// <value><see cref="F:Microsoft.Xna.Framework.Input.ButtonState.Pressed" /> if the button is pressed otherwise, <see cref="F:Microsoft.Xna.Framework.Input.ButtonState.Released" />.</value>
	public ButtonState Right { get; internal set; }

	/// <summary>
	/// Gets if joysticks hat "up" is pressed.
	/// </summary>
	/// <value><see cref="F:Microsoft.Xna.Framework.Input.ButtonState.Pressed" /> if the button is pressed otherwise, <see cref="F:Microsoft.Xna.Framework.Input.ButtonState.Released" />.</value>
	public ButtonState Up { get; internal set; }

	/// <summary>
	/// Determines whether a specified instance of <see cref="T:Microsoft.Xna.Framework.Input.JoystickHat" /> is equal
	/// to another specified <see cref="T:Microsoft.Xna.Framework.Input.JoystickHat" />.
	/// </summary>
	/// <param name="left">The first <see cref="T:Microsoft.Xna.Framework.Input.JoystickHat" /> to compare.</param>
	/// <param name="right">The second <see cref="T:Microsoft.Xna.Framework.Input.JoystickHat" /> to compare.</param>
	/// <returns><c>true</c> if <c>left</c> and <c>right</c> are equal; otherwise, <c>false</c>.</returns>
	public static bool operator ==(JoystickHat left, JoystickHat right)
	{
		if (left.Down == right.Down && left.Left == right.Left && left.Right == right.Right)
		{
			return left.Up == right.Up;
		}
		return false;
	}

	/// <summary>
	/// Determines whether a specified instance of <see cref="T:Microsoft.Xna.Framework.Input.JoystickHat" /> is not
	/// equal to another specified <see cref="T:Microsoft.Xna.Framework.Input.JoystickHat" />.
	/// </summary>
	/// <param name="left">The first <see cref="T:Microsoft.Xna.Framework.Input.JoystickHat" /> to compare.</param>
	/// <param name="right">The second <see cref="T:Microsoft.Xna.Framework.Input.JoystickHat" /> to compare.</param>
	/// <returns><c>true</c> if <c>left</c> and <c>right</c> are not equal; otherwise, <c>false</c>.</returns>
	public static bool operator !=(JoystickHat left, JoystickHat right)
	{
		return !(left == right);
	}

	/// <summary>
	/// Determines whether the specified <see cref="T:System.Object" /> is equal to the current <see cref="T:Microsoft.Xna.Framework.Input.JoystickHat" />.
	/// </summary>
	/// <param name="obj">The <see cref="T:System.Object" /> to compare with the current <see cref="T:Microsoft.Xna.Framework.Input.JoystickHat" />.</param>
	/// <returns><c>true</c> if the specified <see cref="T:System.Object" /> is equal to the current
	/// <see cref="T:Microsoft.Xna.Framework.Input.JoystickHat" />; otherwise, <c>false</c>.</returns>
	public override bool Equals(object obj)
	{
		if (obj is JoystickHat)
		{
			return this == (JoystickHat)obj;
		}
		return false;
	}

	/// <summary>
	/// Serves as a hash function for a <see cref="T:Microsoft.Xna.Framework.Input.JoystickHat" /> object.
	/// </summary>
	/// <returns>A hash code for this instance that is suitable for use in hashing algorithms and data structures such as a
	/// hash table.</returns>
	public override int GetHashCode()
	{
		int hash = 0;
		if (this.Left == ButtonState.Pressed)
		{
			hash |= 8;
		}
		if (this.Up == ButtonState.Pressed)
		{
			hash |= 4;
		}
		if (this.Right == ButtonState.Pressed)
		{
			hash |= 2;
		}
		if (this.Down == ButtonState.Pressed)
		{
			hash |= 1;
		}
		return hash;
	}

	/// <summary>
	/// Returns a <see cref="T:System.String" /> that represents the current <see cref="T:Microsoft.Xna.Framework.Input.JoystickHat" /> in a format of 0000 where each number represents a boolean value of each respecting object property: Left, Up, Right, Down.
	/// </summary>
	/// <returns>A <see cref="T:System.String" /> that represents the current <see cref="T:Microsoft.Xna.Framework.Input.JoystickHat" />.</returns>
	public override string ToString()
	{
		return ((int)this.Left).ToString() + (int)this.Up + (int)this.Right + (int)this.Down;
	}
}
