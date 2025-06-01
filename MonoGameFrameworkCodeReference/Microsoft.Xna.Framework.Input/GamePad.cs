using System;
using System.Collections.Generic;
using System.IO;
using MonoGame.Framework.Utilities;

namespace Microsoft.Xna.Framework.Input;

/// <summary> 
/// Supports querying the game controllers and setting the vibration motors.
/// </summary>
public static class GamePad
{
	private class GamePadInfo
	{
		public IntPtr Device;

		public int PacketNumber;
	}

	private static readonly Dictionary<int, GamePadInfo> Gamepads = new Dictionary<int, GamePadInfo>();

	private static readonly Dictionary<int, int> _translationTable = new Dictionary<int, int>();

	/// <summary>
	/// The maximum number of game pads supported on this system.
	/// </summary>
	public static int MaximumGamePadCount => GamePad.PlatformGetMaxNumberOfGamePads();

	/// <summary>
	/// Returns the capabilites of the connected controller.
	/// </summary>
	/// <param name="playerIndex">Player index for the controller you want to query.</param>
	/// <returns>The capabilites of the controller.</returns>
	public static GamePadCapabilities GetCapabilities(PlayerIndex playerIndex)
	{
		return GamePad.GetCapabilities((int)playerIndex);
	}

	/// <summary>
	/// Returns the capabilites of the connected controller.
	/// </summary>
	/// <param name="index">Index for the controller you want to query.</param>
	/// <returns>The capabilites of the controller.</returns>
	public static GamePadCapabilities GetCapabilities(int index)
	{
		if (index < 0 || index >= GamePad.PlatformGetMaxNumberOfGamePads())
		{
			return default(GamePadCapabilities);
		}
		return GamePad.PlatformGetCapabilities(index);
	}

	/// <summary>
	/// Gets the current state of a game pad controller with an independent axes dead zone.
	/// </summary>
	/// <param name="playerIndex">Player index for the controller you want to query.</param>
	/// <returns>The state of the controller.</returns>
	public static GamePadState GetState(PlayerIndex playerIndex)
	{
		return GamePad.GetState((int)playerIndex, GamePadDeadZone.IndependentAxes);
	}

	/// <summary>
	/// Gets the current state of a game pad controller with an independent axes dead zone.
	/// </summary>
	/// <param name="index">Index for the controller you want to query.</param>
	/// <returns>The state of the controller.</returns>
	public static GamePadState GetState(int index)
	{
		return GamePad.GetState(index, GamePadDeadZone.IndependentAxes);
	}

	/// <summary>
	/// Gets the current state of a game pad controller, using a specified dead zone
	/// on analog stick positions.
	/// </summary>
	/// <param name="playerIndex">Player index for the controller you want to query.</param>
	/// <param name="deadZoneMode">Enumerated value that specifies what dead zone type to use.</param>
	/// <returns>The state of the controller.</returns>
	public static GamePadState GetState(PlayerIndex playerIndex, GamePadDeadZone deadZoneMode)
	{
		return GamePad.GetState((int)playerIndex, deadZoneMode);
	}

	/// <summary>
	/// Gets the current state of a game pad controller, using a specified dead zone
	/// on analog stick positions.
	/// </summary>
	/// <param name="index">Index for the controller you want to query.</param>
	/// <param name="deadZoneMode">Enumerated value that specifies what dead zone type to use.</param>
	/// <returns>The state of the controller.</returns>
	public static GamePadState GetState(int index, GamePadDeadZone deadZoneMode)
	{
		return GamePad.GetState(index, deadZoneMode, deadZoneMode);
	}

	/// <summary>
	/// Gets the current state of a game pad controller, using a specified dead zone
	/// on analog stick positions.
	/// </summary>
	/// <param name="playerIndex">Player index for the controller you want to query.</param>
	/// <param name="leftDeadZoneMode">Enumerated value that specifies what dead zone type to use for the left stick.</param>
	/// <param name="rightDeadZoneMode">Enumerated value that specifies what dead zone type to use for the right stick.</param>
	/// <returns>The state of the controller.</returns>
	public static GamePadState GetState(PlayerIndex playerIndex, GamePadDeadZone leftDeadZoneMode, GamePadDeadZone rightDeadZoneMode)
	{
		return GamePad.GetState((int)playerIndex, leftDeadZoneMode, rightDeadZoneMode);
	}

	/// <summary>
	/// Gets the current state of a game pad controller, using a specified dead zone
	/// on analog stick positions.
	/// </summary>
	/// <param name="index">Index for the controller you want to query.</param>
	/// <param name="leftDeadZoneMode">Enumerated value that specifies what dead zone type to use for the left stick.</param>
	/// <param name="rightDeadZoneMode">Enumerated value that specifies what dead zone type to use for the right stick.</param>
	/// <returns>The state of the controller.</returns>
	public static GamePadState GetState(int index, GamePadDeadZone leftDeadZoneMode, GamePadDeadZone rightDeadZoneMode)
	{
		if (index < 0 || index >= GamePad.PlatformGetMaxNumberOfGamePads())
		{
			return GamePadState.Default;
		}
		return GamePad.PlatformGetState(index, leftDeadZoneMode, rightDeadZoneMode);
	}

	/// <summary>
	/// Sets the vibration motor speeds on the controller device if supported.
	/// </summary>
	/// <param name="playerIndex">Player index that identifies the controller to set.</param>
	/// <param name="leftMotor">The speed of the left motor, between 0.0 and 1.0. This motor is a low-frequency motor.</param>
	/// <param name="rightMotor">The speed of the right motor, between 0.0 and 1.0. This motor is a high-frequency motor.</param>
	/// <returns>Returns true if the vibration motors were set.</returns>
	public static bool SetVibration(PlayerIndex playerIndex, float leftMotor, float rightMotor)
	{
		return GamePad.SetVibration((int)playerIndex, leftMotor, rightMotor, 0f, 0f);
	}

	/// <summary>
	/// Sets the vibration motor speeds on the controller device if supported.
	/// </summary>
	/// <param name="playerIndex">Player index that identifies the controller to set.</param>
	/// <param name="leftMotor">The speed of the left motor, between 0.0 and 1.0. This motor is a low-frequency motor.</param>
	/// <param name="rightMotor">The speed of the right motor, between 0.0 and 1.0. This motor is a high-frequency motor.</param>
	/// <param name="leftTrigger">(Xbox One controller only) The speed of the left trigger motor, between 0.0 and 1.0. This motor is a high-frequency motor.</param>
	/// <param name="rightTrigger">(Xbox One controller only) The speed of the right trigger motor, between 0.0 and 1.0. This motor is a high-frequency motor.</param>
	/// <returns>Returns true if the vibration motors were set.</returns>
	public static bool SetVibration(PlayerIndex playerIndex, float leftMotor, float rightMotor, float leftTrigger, float rightTrigger)
	{
		return GamePad.SetVibration((int)playerIndex, leftMotor, rightMotor, leftTrigger, rightTrigger);
	}

	/// <summary>
	/// Sets the vibration motor speeds on the controller device if supported.
	/// </summary>
	/// <param name="index">Index for the controller you want to query.</param>
	/// <param name="leftMotor">The speed of the left motor, between 0.0 and 1.0. This motor is a low-frequency motor.</param>
	/// <param name="rightMotor">The speed of the right motor, between 0.0 and 1.0. This motor is a high-frequency motor.</param>
	/// <returns>Returns true if the vibration motors were set.</returns>
	public static bool SetVibration(int index, float leftMotor, float rightMotor)
	{
		return GamePad.SetVibration(index, leftMotor, rightMotor, 0f, 0f);
	}

	/// <summary>
	/// Sets the vibration motor speeds on the controller device if supported.
	/// </summary>
	/// <param name="index">Index for the controller you want to query.</param>
	/// <param name="leftMotor">The speed of the left motor, between 0.0 and 1.0. This motor is a low-frequency motor.</param>
	/// <param name="rightMotor">The speed of the right motor, between 0.0 and 1.0. This motor is a high-frequency motor.</param>
	/// <param name="leftTrigger">(Xbox One controller only) The speed of the left trigger motor, between 0.0 and 1.0. This motor is a high-frequency motor.</param>
	/// <param name="rightTrigger">(Xbox One controller only) The speed of the right trigger motor, between 0.0 and 1.0. This motor is a high-frequency motor.</param>
	/// <returns>Returns true if the vibration motors were set.</returns>
	public static bool SetVibration(int index, float leftMotor, float rightMotor, float leftTrigger, float rightTrigger)
	{
		if (index < 0 || index >= GamePad.PlatformGetMaxNumberOfGamePads())
		{
			return false;
		}
		return GamePad.PlatformSetVibration(index, MathHelper.Clamp(leftMotor, 0f, 1f), MathHelper.Clamp(rightMotor, 0f, 1f), MathHelper.Clamp(leftTrigger, 0f, 1f), MathHelper.Clamp(rightTrigger, 0f, 1f));
	}

	/// <summary>
	/// Initialies the internal database of gamepad mappings for an SDL context
	/// </summary>
	public static void InitDatabase()
	{
		using Stream stream = ReflectionHelpers.GetAssembly(typeof(GamePad)).GetManifestResourceStream("gamecontrollerdb.txt");
		if (stream == null)
		{
			return;
		}
		using BinaryReader reader = new BinaryReader(stream);
		try
		{
			IntPtr src = Sdl.RwFromMem(reader.ReadBytes((int)stream.Length), (int)stream.Length);
			Sdl.GameController.AddMappingFromRw(src, 1);
		}
		catch
		{
		}
	}

	internal static void AddDevice(int deviceId)
	{
		GamePadInfo gamepad = new GamePadInfo();
		gamepad.Device = Sdl.GameController.Open(deviceId);
		int id;
		for (id = 0; GamePad.Gamepads.ContainsKey(id); id++)
		{
		}
		GamePad.Gamepads.Add(id, gamepad);
		GamePad.RefreshTranslationTable();
	}

	internal static void RemoveDevice(int instanceid)
	{
		foreach (KeyValuePair<int, GamePadInfo> entry in GamePad.Gamepads)
		{
			if (Sdl.Joystick.InstanceID(Sdl.GameController.GetJoystick(entry.Value.Device)) == instanceid)
			{
				GamePad.Gamepads.Remove(entry.Key);
				GamePad.DisposeDevice(entry.Value);
				break;
			}
		}
		GamePad.RefreshTranslationTable();
	}

	internal static void RefreshTranslationTable()
	{
		GamePad._translationTable.Clear();
		foreach (KeyValuePair<int, GamePadInfo> pair in GamePad.Gamepads)
		{
			GamePad._translationTable[Sdl.Joystick.InstanceID(Sdl.GameController.GetJoystick(pair.Value.Device))] = pair.Key;
		}
	}

	internal static void UpdatePacketInfo(int instanceid, uint packetNumber)
	{
		if (GamePad._translationTable.TryGetValue(instanceid, out var index))
		{
			GamePadInfo info = null;
			if (GamePad.Gamepads.TryGetValue(index, out info))
			{
				info.PacketNumber = (int)((packetNumber < int.MaxValue) ? packetNumber : (packetNumber - int.MaxValue));
			}
		}
	}

	private static void DisposeDevice(GamePadInfo info)
	{
		Sdl.GameController.Close(info.Device);
	}

	internal static void CloseDevices()
	{
		foreach (KeyValuePair<int, GamePadInfo> gamepad in GamePad.Gamepads)
		{
			GamePad.DisposeDevice(gamepad.Value);
		}
		GamePad.Gamepads.Clear();
	}

	private static int PlatformGetMaxNumberOfGamePads()
	{
		return 16;
	}

	private static GamePadCapabilities PlatformGetCapabilities(int index)
	{
		if (!GamePad.Gamepads.ContainsKey(index))
		{
			return default(GamePadCapabilities);
		}
		IntPtr gamecontroller = GamePad.Gamepads[index].Device;
		GamePadCapabilities caps = new GamePadCapabilities
		{
			IsConnected = true,
			DisplayName = Sdl.GameController.GetName(gamecontroller),
			Identifier = Sdl.Joystick.GetGUID(Sdl.GameController.GetJoystick(gamecontroller)).ToString()
		};
		bool hasLeftVibrationMotor = (caps.HasRightVibrationMotor = Sdl.GameController.HasRumble(gamecontroller) != 0);
		caps.HasLeftVibrationMotor = hasLeftVibrationMotor;
		caps.GamePadType = GamePadType.GamePad;
		caps.HasAButton = Sdl.GameController.HasButton(gamecontroller, Sdl.GameController.Button.A);
		caps.HasBButton = Sdl.GameController.HasButton(gamecontroller, Sdl.GameController.Button.B);
		caps.HasXButton = Sdl.GameController.HasButton(gamecontroller, Sdl.GameController.Button.X);
		caps.HasYButton = Sdl.GameController.HasButton(gamecontroller, Sdl.GameController.Button.Y);
		caps.HasBackButton = Sdl.GameController.HasButton(gamecontroller, Sdl.GameController.Button.Back);
		caps.HasBigButton = Sdl.GameController.HasButton(gamecontroller, Sdl.GameController.Button.Guide);
		caps.HasStartButton = Sdl.GameController.HasButton(gamecontroller, Sdl.GameController.Button.Start);
		caps.HasDPadLeftButton = Sdl.GameController.HasButton(gamecontroller, Sdl.GameController.Button.DpadLeft);
		caps.HasDPadDownButton = Sdl.GameController.HasButton(gamecontroller, Sdl.GameController.Button.DpadDown);
		caps.HasDPadRightButton = Sdl.GameController.HasButton(gamecontroller, Sdl.GameController.Button.DpadRight);
		caps.HasDPadUpButton = Sdl.GameController.HasButton(gamecontroller, Sdl.GameController.Button.DpadUp);
		caps.HasLeftShoulderButton = Sdl.GameController.HasButton(gamecontroller, Sdl.GameController.Button.LeftShoulder);
		caps.HasLeftTrigger = Sdl.GameController.HasAxis(gamecontroller, Sdl.GameController.Axis.TriggerLeft);
		caps.HasRightShoulderButton = Sdl.GameController.HasButton(gamecontroller, Sdl.GameController.Button.RightShoulder);
		caps.HasRightTrigger = Sdl.GameController.HasAxis(gamecontroller, Sdl.GameController.Axis.TriggerRight);
		caps.HasLeftStickButton = Sdl.GameController.HasButton(gamecontroller, Sdl.GameController.Button.LeftStick);
		caps.HasRightStickButton = Sdl.GameController.HasButton(gamecontroller, Sdl.GameController.Button.RightStick);
		caps.HasLeftXThumbStick = Sdl.GameController.HasAxis(gamecontroller, Sdl.GameController.Axis.LeftX);
		caps.HasLeftYThumbStick = Sdl.GameController.HasAxis(gamecontroller, Sdl.GameController.Axis.LeftY);
		caps.HasRightXThumbStick = Sdl.GameController.HasAxis(gamecontroller, Sdl.GameController.Axis.RightX);
		caps.HasRightYThumbStick = Sdl.GameController.HasAxis(gamecontroller, Sdl.GameController.Axis.RightY);
		return caps;
	}

	private static float GetFromSdlAxis(int axis)
	{
		if (axis < 0)
		{
			return (float)axis / 32768f;
		}
		return (float)axis / 32767f;
	}

	private static GamePadState PlatformGetState(int index, GamePadDeadZone leftDeadZoneMode, GamePadDeadZone rightDeadZoneMode)
	{
		if (!GamePad.Gamepads.ContainsKey(index))
		{
			return GamePadState.Default;
		}
		GamePadInfo gamepadInfo = GamePad.Gamepads[index];
		IntPtr gdevice = gamepadInfo.Device;
		GamePadThumbSticks thumbSticks = new GamePadThumbSticks(new Vector2(GamePad.GetFromSdlAxis(Sdl.GameController.GetAxis(gdevice, Sdl.GameController.Axis.LeftX)), GamePad.GetFromSdlAxis(Sdl.GameController.GetAxis(gdevice, Sdl.GameController.Axis.LeftY)) * -1f), new Vector2(GamePad.GetFromSdlAxis(Sdl.GameController.GetAxis(gdevice, Sdl.GameController.Axis.RightX)), GamePad.GetFromSdlAxis(Sdl.GameController.GetAxis(gdevice, Sdl.GameController.Axis.RightY)) * -1f), leftDeadZoneMode, rightDeadZoneMode);
		GamePadTriggers triggers = new GamePadTriggers(GamePad.GetFromSdlAxis(Sdl.GameController.GetAxis(gdevice, Sdl.GameController.Axis.TriggerLeft)), GamePad.GetFromSdlAxis(Sdl.GameController.GetAxis(gdevice, Sdl.GameController.Axis.TriggerRight)));
		GamePadButtons buttons = new GamePadButtons((Buttons)(((Sdl.GameController.GetButton(gdevice, Sdl.GameController.Button.A) == 1) ? 4096 : 0) | ((Sdl.GameController.GetButton(gdevice, Sdl.GameController.Button.B) == 1) ? 8192 : 0) | ((Sdl.GameController.GetButton(gdevice, Sdl.GameController.Button.Back) == 1) ? 32 : 0) | ((Sdl.GameController.GetButton(gdevice, Sdl.GameController.Button.Guide) == 1) ? 2048 : 0) | ((Sdl.GameController.GetButton(gdevice, Sdl.GameController.Button.LeftShoulder) == 1) ? 256 : 0) | ((Sdl.GameController.GetButton(gdevice, Sdl.GameController.Button.RightShoulder) == 1) ? 512 : 0) | ((Sdl.GameController.GetButton(gdevice, Sdl.GameController.Button.LeftStick) == 1) ? 64 : 0) | ((Sdl.GameController.GetButton(gdevice, Sdl.GameController.Button.RightStick) == 1) ? 128 : 0) | ((Sdl.GameController.GetButton(gdevice, Sdl.GameController.Button.Start) == 1) ? 16 : 0) | ((Sdl.GameController.GetButton(gdevice, Sdl.GameController.Button.X) == 1) ? 16384 : 0) | ((Sdl.GameController.GetButton(gdevice, Sdl.GameController.Button.Y) == 1) ? 32768 : 0) | ((triggers.Left > 0f) ? 8388608 : 0) | ((triggers.Right > 0f) ? 4194304 : 0)));
		GamePadDPad dPad = new GamePadDPad((Sdl.GameController.GetButton(gdevice, Sdl.GameController.Button.DpadUp) == 1) ? ButtonState.Pressed : ButtonState.Released, (Sdl.GameController.GetButton(gdevice, Sdl.GameController.Button.DpadDown) == 1) ? ButtonState.Pressed : ButtonState.Released, (Sdl.GameController.GetButton(gdevice, Sdl.GameController.Button.DpadLeft) == 1) ? ButtonState.Pressed : ButtonState.Released, (Sdl.GameController.GetButton(gdevice, Sdl.GameController.Button.DpadRight) == 1) ? ButtonState.Pressed : ButtonState.Released);
		GamePadState ret = new GamePadState(thumbSticks, triggers, buttons, dPad);
		ret.PacketNumber = gamepadInfo.PacketNumber;
		return ret;
	}

	private static bool PlatformSetVibration(int index, float leftMotor, float rightMotor, float leftTrigger, float rightTrigger)
	{
		if (!GamePad.Gamepads.ContainsKey(index))
		{
			return false;
		}
		GamePadInfo gamepad = GamePad.Gamepads[index];
		if (Sdl.GameController.Rumble(gamepad.Device, (ushort)(65535f * leftMotor), (ushort)(65535f * rightMotor), uint.MaxValue) == 0)
		{
			return Sdl.GameController.RumbleTriggers(gamepad.Device, (ushort)(65535f * leftTrigger), (ushort)(65535f * rightTrigger), uint.MaxValue) == 0;
		}
		return false;
	}
}
