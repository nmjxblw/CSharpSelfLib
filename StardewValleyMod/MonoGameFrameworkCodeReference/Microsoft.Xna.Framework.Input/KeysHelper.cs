using System;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Input;

internal static class KeysHelper
{
	private static HashSet<int> _map;

	static KeysHelper()
	{
		KeysHelper._map = new HashSet<int>();
		Keys[] array = (Keys[])Enum.GetValues(typeof(Keys));
		foreach (Keys key in array)
		{
			KeysHelper._map.Add((int)key);
		}
	}

	/// <summary>
	/// Checks if specified value is valid Key.
	/// </summary>
	/// <param name="value">Keys base value</param>
	/// <returns>Returns true if value is valid Key, false otherwise</returns>
	public static bool IsKey(int value)
	{
		return KeysHelper._map.Contains(value);
	}
}
