using System;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Input;

/// <summary>
/// Allows getting keystrokes from keyboard.
/// </summary>
public static class Keyboard
{
	private static List<Keys> _keys;

	/// <summary>
	/// Returns the current keyboard state.
	/// </summary>
	/// <returns>Current keyboard state.</returns>
	public static KeyboardState GetState()
	{
		return Keyboard.PlatformGetState();
	}

	/// <summary>
	/// Returns the current keyboard state for a given player.
	/// </summary>
	/// <param name="playerIndex">Player index of the keyboard.</param>
	/// <returns>Current keyboard state.</returns>
	[Obsolete("Use GetState() instead. In future versions this method can be removed.")]
	public static KeyboardState GetState(PlayerIndex playerIndex)
	{
		return Keyboard.PlatformGetState();
	}

	private static KeyboardState PlatformGetState()
	{
		Sdl.Keyboard.Keymod modifiers = Sdl.Keyboard.GetModState();
		return new KeyboardState(Keyboard._keys, (modifiers & Sdl.Keyboard.Keymod.CapsLock) == Sdl.Keyboard.Keymod.CapsLock, (modifiers & Sdl.Keyboard.Keymod.NumLock) == Sdl.Keyboard.Keymod.NumLock);
	}

	internal static void SetKeys(List<Keys> keys)
	{
		Keyboard._keys = keys;
	}
}
