using System;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Input;

/// <summary> 
/// Allows interaction with joysticks. Unlike <see cref="T:Microsoft.Xna.Framework.Input.GamePad" /> the number of Buttons/Axes/DPads is not limited.
/// </summary>
public static class Joystick
{
	/// <summary>
	/// A default <see cref="T:Microsoft.Xna.Framework.Input.JoystickState" />.
	/// </summary>
	private static JoystickState _defaultJoystickState = new JoystickState
	{
		IsConnected = false,
		Axes = new int[0],
		Buttons = new ButtonState[0],
		Hats = new JoystickHat[0]
	};

	internal static Dictionary<int, IntPtr> Joysticks = new Dictionary<int, IntPtr>();

	private static int _lastConnectedIndex = -1;

	private const bool PlatformIsSupported = true;

	/// <summary>
	/// Gets a value indicating whether the current platform supports reading raw joystick data.
	/// </summary>
	/// <value><c>true</c> if the current platform supports reading raw joystick data; otherwise, <c>false</c>.</value>
	public static bool IsSupported => true;

	/// <summary>
	/// Gets a value indicating the last joystick index connected to the system. If this value is less than 0, no joysticks are connected.
	/// <para>The order joysticks are connected and disconnected determines their index.
	/// As such, this value may be larger than 0 even if only one joystick is connected.
	/// </para>
	/// </summary>
	public static int LastConnectedIndex => Joystick.PlatformLastConnectedIndex;

	private static int PlatformLastConnectedIndex => Joystick._lastConnectedIndex;

	/// <summary>
	/// Gets the capabilites of the joystick.
	/// </summary>
	/// <param name="index">Index of the joystick you want to access.</param>
	/// <returns>The capabilites of the joystick.</returns>
	public static JoystickCapabilities GetCapabilities(int index)
	{
		return Joystick.PlatformGetCapabilities(index);
	}

	/// <summary>
	/// Gets the current state of the joystick.
	/// </summary>
	/// <param name="index">Index of the joystick you want to access.</param>
	/// <returns>The state of the joystick.</returns>
	public static JoystickState GetState(int index)
	{
		return Joystick.PlatformGetState(index);
	}

	/// <summary>
	/// Gets the current state of the joystick by updating an existing <see cref="T:Microsoft.Xna.Framework.Input.JoystickState" />.
	/// </summary>
	/// <param name="joystickState">The <see cref="T:Microsoft.Xna.Framework.Input.JoystickState" /> to update.</param>
	/// <param name="index">Index of the joystick you want to access.</param>
	public static void GetState(ref JoystickState joystickState, int index)
	{
		Joystick.PlatformGetState(ref joystickState, index);
	}

	internal static void AddDevices()
	{
		int numJoysticks = Sdl.Joystick.NumJoysticks();
		for (int i = 0; i < numJoysticks; i++)
		{
			Joystick.AddDevice(i);
		}
	}

	internal static void AddDevice(int deviceId)
	{
		IntPtr jdevice = Sdl.Joystick.Open(deviceId);
		if (!Joystick.Joysticks.ContainsValue(jdevice))
		{
			int id;
			for (id = 0; Joystick.Joysticks.ContainsKey(id); id++)
			{
			}
			if (id != Joystick._lastConnectedIndex)
			{
				Joystick._lastConnectedIndex = id;
			}
			Joystick.Joysticks.Add(id, jdevice);
			if (Sdl.GameController.IsGameController(deviceId) == 1)
			{
				GamePad.AddDevice(deviceId);
			}
		}
	}

	internal static void RemoveDevice(int instanceid)
	{
		foreach (KeyValuePair<int, IntPtr> entry in Joystick.Joysticks)
		{
			if (Sdl.Joystick.InstanceID(entry.Value) == instanceid)
			{
				int key = entry.Key;
				Sdl.Joystick.Close(Joystick.Joysticks[entry.Key]);
				Joystick.Joysticks.Remove(entry.Key);
				if (key == Joystick._lastConnectedIndex)
				{
					Joystick.RecalculateLastConnectedIndex();
				}
				break;
			}
		}
	}

	internal static void CloseDevices()
	{
		GamePad.CloseDevices();
		foreach (KeyValuePair<int, IntPtr> entry in Joystick.Joysticks)
		{
			Sdl.Joystick.Close(entry.Value);
		}
		Joystick.Joysticks.Clear();
	}

	private static void RecalculateLastConnectedIndex()
	{
		Joystick._lastConnectedIndex = -1;
		foreach (KeyValuePair<int, IntPtr> entry in Joystick.Joysticks)
		{
			if (entry.Key > Joystick._lastConnectedIndex)
			{
				Joystick._lastConnectedIndex = entry.Key;
			}
		}
	}

	private static JoystickCapabilities PlatformGetCapabilities(int index)
	{
		IntPtr joystickPtr = IntPtr.Zero;
		if (!Joystick.Joysticks.TryGetValue(index, out joystickPtr))
		{
			return new JoystickCapabilities
			{
				IsConnected = false,
				DisplayName = string.Empty,
				Identifier = "",
				IsGamepad = false,
				AxisCount = 0,
				ButtonCount = 0,
				HatCount = 0
			};
		}
		IntPtr jdevice = joystickPtr;
		return new JoystickCapabilities
		{
			IsConnected = true,
			DisplayName = Sdl.Joystick.GetJoystickName(jdevice),
			Identifier = Sdl.Joystick.GetGUID(jdevice).ToString(),
			IsGamepad = (Sdl.GameController.IsGameController(index) == 1),
			AxisCount = Sdl.Joystick.NumAxes(jdevice),
			ButtonCount = Sdl.Joystick.NumButtons(jdevice),
			HatCount = Sdl.Joystick.NumHats(jdevice)
		};
	}

	private static JoystickState PlatformGetState(int index)
	{
		IntPtr joystickPtr = IntPtr.Zero;
		if (!Joystick.Joysticks.TryGetValue(index, out joystickPtr))
		{
			return Joystick._defaultJoystickState;
		}
		JoystickCapabilities jcap = Joystick.PlatformGetCapabilities(index);
		IntPtr jdevice = joystickPtr;
		int[] axes = new int[jcap.AxisCount];
		for (int i = 0; i < axes.Length; i++)
		{
			axes[i] = Sdl.Joystick.GetAxis(jdevice, i);
		}
		ButtonState[] buttons = new ButtonState[jcap.ButtonCount];
		for (int j = 0; j < buttons.Length; j++)
		{
			buttons[j] = ((Sdl.Joystick.GetButton(jdevice, j) != 0) ? ButtonState.Pressed : ButtonState.Released);
		}
		JoystickHat[] hats = new JoystickHat[jcap.HatCount];
		for (int k = 0; k < hats.Length; k++)
		{
			Sdl.Joystick.Hat hatstate = Sdl.Joystick.GetHat(jdevice, k);
			hats[k] = new JoystickHat
			{
				Up = (((hatstate & Sdl.Joystick.Hat.Up) != Sdl.Joystick.Hat.Centered) ? ButtonState.Pressed : ButtonState.Released),
				Down = (((hatstate & Sdl.Joystick.Hat.Down) != Sdl.Joystick.Hat.Centered) ? ButtonState.Pressed : ButtonState.Released),
				Left = (((hatstate & Sdl.Joystick.Hat.Left) != Sdl.Joystick.Hat.Centered) ? ButtonState.Pressed : ButtonState.Released),
				Right = (((hatstate & Sdl.Joystick.Hat.Right) != Sdl.Joystick.Hat.Centered) ? ButtonState.Pressed : ButtonState.Released)
			};
		}
		return new JoystickState
		{
			IsConnected = true,
			Axes = axes,
			Buttons = buttons,
			Hats = hats
		};
	}

	private static void PlatformGetState(ref JoystickState joystickState, int index)
	{
		IntPtr joystickPtr = IntPtr.Zero;
		if (!Joystick.Joysticks.TryGetValue(index, out joystickPtr))
		{
			joystickState.IsConnected = false;
			return;
		}
		JoystickCapabilities jcap = Joystick.PlatformGetCapabilities(index);
		IntPtr jdevice = joystickPtr;
		if (joystickState.Axes.Length < jcap.AxisCount)
		{
			joystickState.Axes = new int[jcap.AxisCount];
		}
		if (joystickState.Buttons.Length < jcap.ButtonCount)
		{
			joystickState.Buttons = new ButtonState[jcap.ButtonCount];
		}
		if (joystickState.Hats.Length < jcap.HatCount)
		{
			joystickState.Hats = new JoystickHat[jcap.HatCount];
		}
		for (int i = 0; i < jcap.AxisCount; i++)
		{
			joystickState.Axes[i] = Sdl.Joystick.GetAxis(jdevice, i);
		}
		for (int j = 0; j < jcap.ButtonCount; j++)
		{
			joystickState.Buttons[j] = ((Sdl.Joystick.GetButton(jdevice, j) != 0) ? ButtonState.Pressed : ButtonState.Released);
		}
		for (int k = 0; k < jcap.HatCount; k++)
		{
			Sdl.Joystick.Hat hatstate = Sdl.Joystick.GetHat(jdevice, k);
			joystickState.Hats[k] = new JoystickHat
			{
				Up = (((hatstate & Sdl.Joystick.Hat.Up) != Sdl.Joystick.Hat.Centered) ? ButtonState.Pressed : ButtonState.Released),
				Down = (((hatstate & Sdl.Joystick.Hat.Down) != Sdl.Joystick.Hat.Centered) ? ButtonState.Pressed : ButtonState.Released),
				Left = (((hatstate & Sdl.Joystick.Hat.Left) != Sdl.Joystick.Hat.Centered) ? ButtonState.Pressed : ButtonState.Released),
				Right = (((hatstate & Sdl.Joystick.Hat.Right) != Sdl.Joystick.Hat.Centered) ? ButtonState.Pressed : ButtonState.Released)
			};
		}
		joystickState.IsConnected = true;
	}
}
