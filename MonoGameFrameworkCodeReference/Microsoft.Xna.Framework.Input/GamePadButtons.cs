namespace Microsoft.Xna.Framework.Input;

/// <summary>
/// A struct that represents the current button states for the controller.
/// </summary>
public struct GamePadButtons
{
	internal readonly Buttons _buttons;

	/// <summary>
	/// Gets a value indicating if the button A is pressed.
	/// </summary>
	/// <value><see cref="F:Microsoft.Xna.Framework.Input.ButtonState.Pressed" /> if the button A is pressed; otherwise, <see cref="F:Microsoft.Xna.Framework.Input.ButtonState.Released" />.</value>
	public ButtonState A
	{
		get
		{
			if ((this._buttons & Buttons.A) != Buttons.A)
			{
				return ButtonState.Released;
			}
			return ButtonState.Pressed;
		}
	}

	/// <summary>
	/// Gets a value indicating if the button B is pressed.
	/// </summary>
	/// <value><see cref="F:Microsoft.Xna.Framework.Input.ButtonState.Pressed" /> if the button B is pressed; otherwise, <see cref="F:Microsoft.Xna.Framework.Input.ButtonState.Released" />.</value>
	public ButtonState B
	{
		get
		{
			if ((this._buttons & Buttons.B) != Buttons.B)
			{
				return ButtonState.Released;
			}
			return ButtonState.Pressed;
		}
	}

	/// <summary>
	/// Gets a value indicating if the button Back is pressed.
	/// </summary>
	/// <value><see cref="F:Microsoft.Xna.Framework.Input.ButtonState.Pressed" /> if the button Back is pressed; otherwise, <see cref="F:Microsoft.Xna.Framework.Input.ButtonState.Released" />.</value>
	public ButtonState Back
	{
		get
		{
			if ((this._buttons & Buttons.Back) != Buttons.Back)
			{
				return ButtonState.Released;
			}
			return ButtonState.Pressed;
		}
	}

	/// <summary>
	/// Gets a value indicating if the button X is pressed.
	/// </summary>
	/// <value><see cref="F:Microsoft.Xna.Framework.Input.ButtonState.Pressed" /> if the button X is pressed; otherwise, <see cref="F:Microsoft.Xna.Framework.Input.ButtonState.Released" />.</value>
	public ButtonState X
	{
		get
		{
			if ((this._buttons & Buttons.X) != Buttons.X)
			{
				return ButtonState.Released;
			}
			return ButtonState.Pressed;
		}
	}

	/// <summary>
	/// Gets a value indicating if the button Y is pressed.
	/// </summary>
	/// <value><see cref="F:Microsoft.Xna.Framework.Input.ButtonState.Pressed" /> if the button Y is pressed; otherwise, <see cref="F:Microsoft.Xna.Framework.Input.ButtonState.Released" />.</value>
	public ButtonState Y
	{
		get
		{
			if ((this._buttons & Buttons.Y) != Buttons.Y)
			{
				return ButtonState.Released;
			}
			return ButtonState.Pressed;
		}
	}

	/// <summary>
	/// Gets a value indicating if the button Start is pressed.
	/// </summary>
	/// <value><see cref="F:Microsoft.Xna.Framework.Input.ButtonState.Pressed" /> if the button Start is pressed; otherwise, <see cref="F:Microsoft.Xna.Framework.Input.ButtonState.Released" />.</value>
	public ButtonState Start
	{
		get
		{
			if ((this._buttons & Buttons.Start) != Buttons.Start)
			{
				return ButtonState.Released;
			}
			return ButtonState.Pressed;
		}
	}

	/// <summary>
	/// Gets a value indicating if the left shoulder button is pressed.
	/// </summary>
	/// <value><see cref="F:Microsoft.Xna.Framework.Input.ButtonState.Pressed" /> if the left shoulder button is pressed; otherwise, <see cref="F:Microsoft.Xna.Framework.Input.ButtonState.Released" />.</value>
	public ButtonState LeftShoulder
	{
		get
		{
			if ((this._buttons & Buttons.LeftShoulder) != Buttons.LeftShoulder)
			{
				return ButtonState.Released;
			}
			return ButtonState.Pressed;
		}
	}

	/// <summary>
	/// Gets a value indicating if the left stick button is pressed.
	/// </summary>
	/// <value><see cref="F:Microsoft.Xna.Framework.Input.ButtonState.Pressed" /> if the left stick button is pressed; otherwise, <see cref="F:Microsoft.Xna.Framework.Input.ButtonState.Released" />.</value>
	public ButtonState LeftStick
	{
		get
		{
			if ((this._buttons & Buttons.LeftStick) != Buttons.LeftStick)
			{
				return ButtonState.Released;
			}
			return ButtonState.Pressed;
		}
	}

	/// <summary>
	/// Gets a value indicating if the right shoulder button is pressed.
	/// </summary>
	/// <value><see cref="F:Microsoft.Xna.Framework.Input.ButtonState.Pressed" /> if the right shoulder button is pressed; otherwise, <see cref="F:Microsoft.Xna.Framework.Input.ButtonState.Released" />.</value>
	public ButtonState RightShoulder
	{
		get
		{
			if ((this._buttons & Buttons.RightShoulder) != Buttons.RightShoulder)
			{
				return ButtonState.Released;
			}
			return ButtonState.Pressed;
		}
	}

	/// <summary>
	/// Gets a value indicating if the right stick button is pressed.
	/// </summary>
	/// <value><see cref="F:Microsoft.Xna.Framework.Input.ButtonState.Pressed" /> if the right stick button is pressed; otherwise, <see cref="F:Microsoft.Xna.Framework.Input.ButtonState.Released" />.</value>
	public ButtonState RightStick
	{
		get
		{
			if ((this._buttons & Buttons.RightStick) != Buttons.RightStick)
			{
				return ButtonState.Released;
			}
			return ButtonState.Pressed;
		}
	}

	/// <summary>
	/// Gets a value indicating if the guide button is pressed.
	/// </summary>
	/// <value><see cref="F:Microsoft.Xna.Framework.Input.ButtonState.Pressed" /> if the guide button is pressed; otherwise, <see cref="F:Microsoft.Xna.Framework.Input.ButtonState.Released" />.</value>
	public ButtonState BigButton
	{
		get
		{
			if ((this._buttons & Buttons.BigButton) != Buttons.BigButton)
			{
				return ButtonState.Released;
			}
			return ButtonState.Pressed;
		}
	}

	public GamePadButtons(Buttons buttons)
	{
		this._buttons = buttons;
	}

	internal GamePadButtons(params Buttons[] buttons)
	{
		this = default(GamePadButtons);
		foreach (Buttons b in buttons)
		{
			this._buttons |= b;
		}
	}

	/// <summary>
	/// Determines whether two specified instances of <see cref="T:Microsoft.Xna.Framework.Input.GamePadButtons" /> are equal.
	/// </summary>
	/// <param name="left">The first object to compare.</param>
	/// <param name="right">The second object to compare.</param>
	/// <returns>true if <paramref name="left" /> and <paramref name="right" /> are equal; otherwise, false.</returns>
	public static bool operator ==(GamePadButtons left, GamePadButtons right)
	{
		return left._buttons == right._buttons;
	}

	/// <summary>
	/// Determines whether two specified instances of <see cref="T:Microsoft.Xna.Framework.Input.GamePadButtons" /> are not equal.
	/// </summary>
	/// <param name="left">The first object to compare.</param>
	/// <param name="right">The second object to compare.</param>
	/// <returns>true if <paramref name="left" /> and <paramref name="right" /> are not equal; otherwise, false.</returns>
	public static bool operator !=(GamePadButtons left, GamePadButtons right)
	{
		return !(left == right);
	}

	/// <summary>
	/// Returns a value indicating whether this instance is equal to a specified object.
	/// </summary>
	/// <param name="obj">An object to compare to this instance.</param>
	/// <returns>true if <paramref name="obj" /> is a <see cref="T:Microsoft.Xna.Framework.Input.GamePadButtons" /> and has the same value as this instance; otherwise, false.</returns>
	public override bool Equals(object obj)
	{
		if (obj is GamePadButtons)
		{
			return this == (GamePadButtons)obj;
		}
		return false;
	}

	/// <summary>
	/// Serves as a hash function for a <see cref="T:Microsoft.Xna.Framework.Input.GamePadButtons" /> object.
	/// </summary>
	/// <returns>A hash code for this instance that is suitable for use in hashing algorithms and data structures such as a
	/// hash table.</returns>
	public override int GetHashCode()
	{
		return (int)this._buttons;
	}

	/// <summary>
	/// Returns a <see cref="T:System.String" /> that represents the current <see cref="T:Microsoft.Xna.Framework.Input.GamePadButtons" />.
	/// </summary>
	/// <returns>A <see cref="T:System.String" /> that represents the current <see cref="T:Microsoft.Xna.Framework.Input.GamePadButtons" />.</returns>
	public override string ToString()
	{
		return "[GamePadButtons: A=" + (int)this.A + ", B=" + (int)this.B + ", Back=" + (int)this.Back + ", X=" + (int)this.X + ", Y=" + (int)this.Y + ", Start=" + (int)this.Start + ", LeftShoulder=" + (int)this.LeftShoulder + ", LeftStick=" + (int)this.LeftStick + ", RightShoulder=" + (int)this.RightShoulder + ", RightStick=" + (int)this.RightStick + ", BigButton=" + (int)this.BigButton + "]";
	}
}
