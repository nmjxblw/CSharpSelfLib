using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace StardewValley;

public class InputState
{
	protected Point _simulatedMousePosition = Point.Zero;

	protected List<Keys> _ignoredKeys = new List<Keys>();

	protected List<Keys> _pressedKeys = new List<Keys>();

	protected KeyboardState? _keyState;

	protected int _lastKeyStateTick = -1;

	protected KeyboardState _currentKeyboardState;

	protected MouseState _currentMouseState;

	protected GamePadState _currentGamepadState;

	public virtual void UpdateStates()
	{
		this._currentKeyboardState = Keyboard.GetState();
		this._currentMouseState = Mouse.GetState();
		if (Game1.playerOneIndex >= PlayerIndex.One)
		{
			this._currentGamepadState = GamePad.GetState(Game1.playerOneIndex);
		}
		else
		{
			this._currentGamepadState = default(GamePadState);
		}
	}

	public virtual void Update()
	{
	}

	public virtual void IgnoreKeys(Keys[] keys)
	{
		if (keys.Length != 0)
		{
			this._ignoredKeys.AddRange(keys);
		}
	}

	public virtual KeyboardState GetKeyboardState()
	{
		if (!Game1.game1.IsMainInstance || !Game1.game1.HasKeyboardFocus())
		{
			return default(KeyboardState);
		}
		if (this._lastKeyStateTick != Game1.ticks || !this._keyState.HasValue)
		{
			if (this._ignoredKeys.Count == 0)
			{
				this._keyState = this._currentKeyboardState;
			}
			else
			{
				this._pressedKeys.Clear();
				this._pressedKeys.AddRange(this._currentKeyboardState.GetPressedKeys());
				this._ignoredKeys.RemoveAll((Keys key) => !this._pressedKeys.Contains(key));
				this._pressedKeys.RemoveAll((Keys key) => this._ignoredKeys.Contains(key));
				this._keyState = new KeyboardState(this._pressedKeys.ToArray());
			}
			this._lastKeyStateTick = Game1.ticks;
		}
		return this._keyState.Value;
	}

	public virtual GamePadState GetGamePadState()
	{
		if (Game1.options.gamepadMode == Options.GamepadModes.ForceOff || Game1.playerOneIndex == (PlayerIndex)(-1))
		{
			return default(GamePadState);
		}
		return this._currentGamepadState;
	}

	public virtual MouseState GetMouseState()
	{
		if (!Game1.game1.IsMainInstance)
		{
			return new MouseState(this._simulatedMousePosition.X, this._simulatedMousePosition.Y, 0, ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released);
		}
		return this._currentMouseState;
	}

	public virtual void SetMousePosition(int x, int y)
	{
		if (!Game1.game1.IsMainInstance)
		{
			this._simulatedMousePosition.X = x;
			this._simulatedMousePosition.Y = y;
		}
		else
		{
			Mouse.SetPosition(x, y);
			this._currentMouseState = new MouseState(x, y, this._currentMouseState.ScrollWheelValue, this._currentMouseState.LeftButton, this._currentMouseState.MiddleButton, this._currentMouseState.RightButton, this._currentMouseState.XButton1, this._currentMouseState.XButton2);
		}
	}
}
