namespace Microsoft.Xna.Framework.Input;

/// <summary>
/// Represents a mouse state with cursor position and button press information.
/// </summary>
public struct MouseState
{
	private const byte LeftButtonFlag = 1;

	private const byte RightButtonFlag = 2;

	private const byte MiddleButtonFlag = 4;

	private const byte XButton1Flag = 8;

	private const byte XButton2Flag = 16;

	private int _x;

	private int _y;

	private int _scrollWheelValue;

	private int _horizontalScrollWheelValue;

	private byte _buttons;

	/// <summary>
	/// Gets horizontal position of the cursor in relation to the window.
	/// </summary>
	public int X
	{
		get
		{
			return this._x;
		}
		internal set
		{
			this._x = value;
		}
	}

	/// <summary>
	/// Gets vertical position of the cursor in relation to the window.
	/// </summary>
	public int Y
	{
		get
		{
			return this._y;
		}
		internal set
		{
			this._y = value;
		}
	}

	/// <summary>
	/// Gets cursor position.
	/// </summary>
	public Point Position => new Point(this._x, this._y);

	/// <summary>
	/// Gets state of the left mouse button.
	/// </summary>
	public ButtonState LeftButton
	{
		get
		{
			if ((this._buttons & 1) <= 0)
			{
				return ButtonState.Released;
			}
			return ButtonState.Pressed;
		}
		internal set
		{
			if (value == ButtonState.Pressed)
			{
				this._buttons |= 1;
			}
			else
			{
				this._buttons = (byte)(this._buttons & -2);
			}
		}
	}

	/// <summary>
	/// Gets state of the middle mouse button.
	/// </summary>
	public ButtonState MiddleButton
	{
		get
		{
			if ((this._buttons & 4) <= 0)
			{
				return ButtonState.Released;
			}
			return ButtonState.Pressed;
		}
		internal set
		{
			if (value == ButtonState.Pressed)
			{
				this._buttons |= 4;
			}
			else
			{
				this._buttons = (byte)(this._buttons & -5);
			}
		}
	}

	/// <summary>
	/// Gets state of the right mouse button.
	/// </summary>
	public ButtonState RightButton
	{
		get
		{
			if ((this._buttons & 2) <= 0)
			{
				return ButtonState.Released;
			}
			return ButtonState.Pressed;
		}
		internal set
		{
			if (value == ButtonState.Pressed)
			{
				this._buttons |= 2;
			}
			else
			{
				this._buttons = (byte)(this._buttons & -3);
			}
		}
	}

	/// <summary>
	/// Returns cumulative scroll wheel value since the game start.
	/// </summary>
	public int ScrollWheelValue
	{
		get
		{
			return this._scrollWheelValue;
		}
		internal set
		{
			this._scrollWheelValue = value;
		}
	}

	/// <summary>
	/// Returns the cumulative horizontal scroll wheel value since the game start
	/// </summary>
	public int HorizontalScrollWheelValue
	{
		get
		{
			return this._horizontalScrollWheelValue;
		}
		internal set
		{
			this._horizontalScrollWheelValue = value;
		}
	}

	/// <summary>
	/// Gets state of the XButton1.
	/// </summary>
	public ButtonState XButton1
	{
		get
		{
			if ((this._buttons & 8) <= 0)
			{
				return ButtonState.Released;
			}
			return ButtonState.Pressed;
		}
		internal set
		{
			if (value == ButtonState.Pressed)
			{
				this._buttons |= 8;
			}
			else
			{
				this._buttons = (byte)(this._buttons & -9);
			}
		}
	}

	/// <summary>
	/// Gets state of the XButton2.
	/// </summary>
	public ButtonState XButton2
	{
		get
		{
			if ((this._buttons & 0x10) <= 0)
			{
				return ButtonState.Released;
			}
			return ButtonState.Pressed;
		}
		internal set
		{
			if (value == ButtonState.Pressed)
			{
				this._buttons |= 16;
			}
			else
			{
				this._buttons = (byte)(this._buttons & -17);
			}
		}
	}

	/// <summary>
	/// Initializes a new instance of the MouseState.
	/// </summary>
	/// <param name="x">Horizontal position of the mouse in relation to the window.</param>
	/// <param name="y">Vertical position of the mouse in relation to the window.</param>
	/// <param name="scrollWheel">Mouse scroll wheel's value.</param>
	/// <param name="leftButton">Left mouse button's state.</param>
	/// <param name="middleButton">Middle mouse button's state.</param>
	/// <param name="rightButton">Right mouse button's state.</param>
	/// <param name="xButton1">XBUTTON1's state.</param>
	/// <param name="xButton2">XBUTTON2's state.</param>
	/// <remarks>Normally <see cref="M:Microsoft.Xna.Framework.Input.Mouse.GetState" /> should be used to get mouse current state. The constructor is provided for simulating mouse input.</remarks>
	public MouseState(int x, int y, int scrollWheel, ButtonState leftButton, ButtonState middleButton, ButtonState rightButton, ButtonState xButton1, ButtonState xButton2)
	{
		this._x = x;
		this._y = y;
		this._scrollWheelValue = scrollWheel;
		this._buttons = (byte)(((leftButton == ButtonState.Pressed) ? 1u : 0u) | (uint)((rightButton == ButtonState.Pressed) ? 2 : 0) | (uint)((middleButton == ButtonState.Pressed) ? 4 : 0) | (uint)((xButton1 == ButtonState.Pressed) ? 8 : 0) | (uint)((xButton2 == ButtonState.Pressed) ? 16 : 0));
		this._horizontalScrollWheelValue = 0;
	}

	/// <summary>
	/// Initializes a new instance of the MouseState.
	/// </summary>
	/// <param name="x">Horizontal position of the mouse in relation to the window.</param>
	/// <param name="y">Vertical position of the mouse in relation to the window.</param>
	/// <param name="scrollWheel">Mouse scroll wheel's value.</param>
	/// <param name="leftButton">Left mouse button's state.</param>
	/// <param name="middleButton">Middle mouse button's state.</param>
	/// <param name="rightButton">Right mouse button's state.</param>
	/// <param name="xButton1">XBUTTON1's state.</param>
	/// <param name="xButton2">XBUTTON2's state.</param>
	/// <param name="horizontalScrollWheel">Mouse horizontal scroll wheel's value.</param>
	/// <remarks>Normally <see cref="M:Microsoft.Xna.Framework.Input.Mouse.GetState" /> should be used to get mouse current state. The constructor is provided for simulating mouse input.</remarks>
	public MouseState(int x, int y, int scrollWheel, ButtonState leftButton, ButtonState middleButton, ButtonState rightButton, ButtonState xButton1, ButtonState xButton2, int horizontalScrollWheel)
	{
		this._x = x;
		this._y = y;
		this._scrollWheelValue = scrollWheel;
		this._buttons = (byte)(((leftButton == ButtonState.Pressed) ? 1u : 0u) | (uint)((rightButton == ButtonState.Pressed) ? 2 : 0) | (uint)((middleButton == ButtonState.Pressed) ? 4 : 0) | (uint)((xButton1 == ButtonState.Pressed) ? 8 : 0) | (uint)((xButton2 == ButtonState.Pressed) ? 16 : 0));
		this._horizontalScrollWheelValue = horizontalScrollWheel;
	}

	/// <summary>
	/// Compares whether two MouseState instances are equal.
	/// </summary>
	/// <param name="left">MouseState instance on the left of the equal sign.</param>
	/// <param name="right">MouseState instance  on the right of the equal sign.</param>
	/// <returns>true if the instances are equal; false otherwise.</returns>
	public static bool operator ==(MouseState left, MouseState right)
	{
		if (left._x == right._x && left._y == right._y && left._buttons == right._buttons && left._scrollWheelValue == right._scrollWheelValue)
		{
			return left._horizontalScrollWheelValue == right._horizontalScrollWheelValue;
		}
		return false;
	}

	/// <summary>
	/// Compares whether two MouseState instances are not equal.
	/// </summary>
	/// <param name="left">MouseState instance on the left of the equal sign.</param>
	/// <param name="right">MouseState instance  on the right of the equal sign.</param>
	/// <returns>true if the objects are not equal; false otherwise.</returns>
	public static bool operator !=(MouseState left, MouseState right)
	{
		return !(left == right);
	}

	/// <summary>
	/// Compares whether current instance is equal to specified object.
	/// </summary>
	/// <param name="obj">The MouseState to compare.</param>
	/// <returns></returns>
	public override bool Equals(object obj)
	{
		if (obj is MouseState)
		{
			return this == (MouseState)obj;
		}
		return false;
	}

	/// <summary>
	/// Gets the hash code for MouseState instance.
	/// </summary>
	/// <returns>Hash code of the object.</returns>
	public override int GetHashCode()
	{
		return (((((((this._x * 397) ^ this._y) * 397) ^ this._scrollWheelValue) * 397) ^ this._horizontalScrollWheelValue) * 397) ^ this._buttons;
	}

	/// <summary>
	/// Returns a string describing the mouse state.
	/// </summary>
	public override string ToString()
	{
		string buttons;
		if (this._buttons == 0)
		{
			buttons = "None";
		}
		else
		{
			buttons = string.Empty;
			if ((this._buttons & 1) == 1)
			{
				buttons = ((buttons.Length <= 0) ? (buttons + "Left") : (buttons + " Left"));
			}
			if ((this._buttons & 2) == 2)
			{
				buttons = ((buttons.Length <= 0) ? (buttons + "Right") : (buttons + " Right"));
			}
			if ((this._buttons & 4) == 4)
			{
				buttons = ((buttons.Length <= 0) ? (buttons + "Middle") : (buttons + " Middle"));
			}
			if ((this._buttons & 8) == 8)
			{
				buttons = ((buttons.Length <= 0) ? (buttons + "XButton1") : (buttons + " XButton1"));
			}
			if ((this._buttons & 0x10) == 16)
			{
				buttons = ((buttons.Length <= 0) ? (buttons + "XButton2") : (buttons + " XButton2"));
			}
		}
		return "[MouseState X=" + this._x + ", Y=" + this._y + ", Buttons=" + buttons + ", Wheel=" + this._scrollWheelValue + ", HWheel=" + this._horizontalScrollWheelValue + "]";
	}
}
