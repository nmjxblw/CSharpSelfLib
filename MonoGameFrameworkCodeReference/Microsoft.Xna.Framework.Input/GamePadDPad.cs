namespace Microsoft.Xna.Framework.Input;

public struct GamePadDPad
{
	/// <summary>
	/// Gets a value indicating wethever down is pressed on the directional pad.
	/// </summary>
	/// <value><see cref="F:Microsoft.Xna.Framework.Input.ButtonState.Pressed" /> if the down button is pressed; otherwise, <see cref="F:Microsoft.Xna.Framework.Input.ButtonState.Released" />.</value>
	public ButtonState Down { get; private set; }

	/// <summary>
	/// Gets a value indicating wethever left is pressed on the directional pad.
	/// </summary>
	/// <value><see cref="F:Microsoft.Xna.Framework.Input.ButtonState.Pressed" /> if the left button is pressed; otherwise, <see cref="F:Microsoft.Xna.Framework.Input.ButtonState.Released" />.</value>
	public ButtonState Left { get; private set; }

	/// <summary>
	/// Gets a value indicating wethever right is pressed on the directional pad.
	/// </summary>
	/// <value><see cref="F:Microsoft.Xna.Framework.Input.ButtonState.Pressed" /> if the right button is pressed; otherwise, <see cref="F:Microsoft.Xna.Framework.Input.ButtonState.Released" />.</value>
	public ButtonState Right { get; private set; }

	/// <summary>
	/// Gets a value indicating wethever up is pressed on the directional pad.
	/// </summary>
	/// <value><see cref="F:Microsoft.Xna.Framework.Input.ButtonState.Pressed" /> if the up button is pressed; otherwise, <see cref="F:Microsoft.Xna.Framework.Input.ButtonState.Released" />.</value>
	public ButtonState Up { get; private set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="T:Microsoft.Xna.Framework.Input.GamePadDPad" /> struct.
	/// </summary>
	/// <param name="upValue">Current state of directional pad up.</param>
	/// <param name="downValue">Current state of directional pad down.</param>
	/// <param name="leftValue">Current state of directional pad left.</param>
	/// <param name="rightValue">Current state of directional pad right.</param>
	public GamePadDPad(ButtonState upValue, ButtonState downValue, ButtonState leftValue, ButtonState rightValue)
	{
		this = default(GamePadDPad);
		this.Up = upValue;
		this.Down = downValue;
		this.Left = leftValue;
		this.Right = rightValue;
	}

	internal GamePadDPad(Buttons[] buttons)
	{
		this = default(GamePadDPad);
		foreach (Buttons button in buttons)
		{
			this.ConvertButtonToDirection(button);
		}
	}

	internal GamePadDPad(Buttons button)
	{
		this = default(GamePadDPad);
		this.ConvertButtonToDirection(button);
	}

	/// <summary>
	/// Determines whether two specified instances of <see cref="T:Microsoft.Xna.Framework.Input.GamePadDPad" /> are equal.
	/// </summary>
	/// <param name="left">The first object to compare.</param>
	/// <param name="right">The second object to compare.</param>
	/// <returns>true if <paramref name="left" /> and <paramref name="right" /> are equal; otherwise, false.</returns>
	public static bool operator ==(GamePadDPad left, GamePadDPad right)
	{
		if (left.Down == right.Down && left.Left == right.Left && left.Right == right.Right)
		{
			return left.Up == right.Up;
		}
		return false;
	}

	/// <summary>
	/// Determines whether two specified instances of <see cref="T:Microsoft.Xna.Framework.Input.GamePadDPad" /> are not equal.
	/// </summary>
	/// <param name="left">The first object to compare.</param>
	/// <param name="right">The second object to compare.</param>
	/// <returns>true if <paramref name="left" /> and <paramref name="right" /> are not equal; otherwise, false.</returns>
	public static bool operator !=(GamePadDPad left, GamePadDPad right)
	{
		return !(left == right);
	}

	/// <summary>
	/// Returns a value indicating whether this instance is equal to a specified object.
	/// </summary>
	/// <param name="obj">An object to compare to this instance.</param>
	/// <returns>true if <paramref name="obj" /> is a <see cref="T:Microsoft.Xna.Framework.Input.GamePadDPad" /> and has the same value as this instance; otherwise, false.</returns>
	public override bool Equals(object obj)
	{
		if (obj is GamePadDPad)
		{
			return this == (GamePadDPad)obj;
		}
		return false;
	}

	/// <summary>
	/// Serves as a hash function for a <see cref="T:Microsoft.Xna.Framework.Input.GamePadDPad" /> object.
	/// </summary>
	/// <returns>A hash code for this instance that is suitable for use in hashing algorithms and data structures such as a
	/// hash table.</returns>
	public override int GetHashCode()
	{
		return ((this.Down == ButtonState.Pressed) ? 1 : 0) + ((this.Left == ButtonState.Pressed) ? 2 : 0) + ((this.Right == ButtonState.Pressed) ? 4 : 0) + ((this.Up == ButtonState.Pressed) ? 8 : 0);
	}

	/// <summary>
	/// Returns a <see cref="T:System.String" /> that represents the current <see cref="T:Microsoft.Xna.Framework.Input.GamePadDPad" />
	/// in a format of 0000 where each number represents a boolean value of each respecting object property: Left, Up, Right, Down.
	/// </summary>
	/// <returns>A <see cref="T:System.String" /> that represents the current <see cref="T:Microsoft.Xna.Framework.Input.GamePadDPad" />.</returns>
	public override string ToString()
	{
		return ((int)this.Left).ToString() + (int)this.Up + (int)this.Right + (int)this.Down;
	}

	private void ConvertButtonToDirection(Buttons button)
	{
		if ((button & Buttons.DPadDown) == Buttons.DPadDown)
		{
			this.Down = ButtonState.Pressed;
		}
		if ((button & Buttons.DPadLeft) == Buttons.DPadLeft)
		{
			this.Left = ButtonState.Pressed;
		}
		if ((button & Buttons.DPadRight) == Buttons.DPadRight)
		{
			this.Right = ButtonState.Pressed;
		}
		if ((button & Buttons.DPadUp) == Buttons.DPadUp)
		{
			this.Up = ButtonState.Pressed;
		}
	}
}
