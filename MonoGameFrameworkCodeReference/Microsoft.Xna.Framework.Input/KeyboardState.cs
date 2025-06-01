using System;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Input;

/// <summary>
/// Holds the state of keystrokes by a keyboard.
/// </summary>
public struct KeyboardState
{
	private const byte CapsLockModifier = 1;

	private const byte NumLockModifier = 2;

	private static Keys[] empty = new Keys[0];

	private uint _keys0;

	private uint _keys1;

	private uint _keys2;

	private uint _keys3;

	private uint _keys4;

	private uint _keys5;

	private uint _keys6;

	private uint _keys7;

	private byte _modifiers;

	/// <summary>
	/// Gets the current state of the Caps Lock key.
	/// </summary>
	public bool CapsLock => (this._modifiers & 1) > 0;

	/// <summary>
	/// Gets the current state of the Num Lock key.
	/// </summary>
	public bool NumLock => (this._modifiers & 2) > 0;

	/// <summary>
	/// Returns the state of a specified key.
	/// </summary>
	/// <param name="key">The key to query.</param>
	/// <returns>The state of the key.</returns>
	public KeyState this[Keys key]
	{
		get
		{
			if (!this.InternalGetKey(key))
			{
				return KeyState.Up;
			}
			return KeyState.Down;
		}
	}

	private bool InternalGetKey(Keys key)
	{
		uint mask = (uint)(1 << (int)(key & (Keys)31));
		return ((uint)(((int)key >> 5) switch
		{
			0 => (int)this._keys0, 
			1 => (int)this._keys1, 
			2 => (int)this._keys2, 
			3 => (int)this._keys3, 
			4 => (int)this._keys4, 
			5 => (int)this._keys5, 
			6 => (int)this._keys6, 
			7 => (int)this._keys7, 
			_ => 0, 
		}) & mask) != 0;
	}

	internal void InternalSetKey(Keys key)
	{
		uint mask = (uint)(1 << (int)(key & (Keys)31));
		switch ((int)key >> 5)
		{
		case 0:
			this._keys0 |= mask;
			break;
		case 1:
			this._keys1 |= mask;
			break;
		case 2:
			this._keys2 |= mask;
			break;
		case 3:
			this._keys3 |= mask;
			break;
		case 4:
			this._keys4 |= mask;
			break;
		case 5:
			this._keys5 |= mask;
			break;
		case 6:
			this._keys6 |= mask;
			break;
		case 7:
			this._keys7 |= mask;
			break;
		}
	}

	internal void InternalClearKey(Keys key)
	{
		uint mask = (uint)(1 << (int)(key & (Keys)31));
		switch ((int)key >> 5)
		{
		case 0:
			this._keys0 &= ~mask;
			break;
		case 1:
			this._keys1 &= ~mask;
			break;
		case 2:
			this._keys2 &= ~mask;
			break;
		case 3:
			this._keys3 &= ~mask;
			break;
		case 4:
			this._keys4 &= ~mask;
			break;
		case 5:
			this._keys5 &= ~mask;
			break;
		case 6:
			this._keys6 &= ~mask;
			break;
		case 7:
			this._keys7 &= ~mask;
			break;
		}
	}

	internal void InternalClearAllKeys()
	{
		this._keys0 = 0u;
		this._keys1 = 0u;
		this._keys2 = 0u;
		this._keys3 = 0u;
		this._keys4 = 0u;
		this._keys5 = 0u;
		this._keys6 = 0u;
		this._keys7 = 0u;
	}

	internal KeyboardState(List<Keys> keys, bool capsLock = false, bool numLock = false)
	{
		this = default(KeyboardState);
		this._keys0 = 0u;
		this._keys1 = 0u;
		this._keys2 = 0u;
		this._keys3 = 0u;
		this._keys4 = 0u;
		this._keys5 = 0u;
		this._keys6 = 0u;
		this._keys7 = 0u;
		this._modifiers = (byte)(0u | (capsLock ? 1u : 0u) | (uint)(numLock ? 2 : 0));
		if (keys == null)
		{
			return;
		}
		foreach (Keys k in keys)
		{
			this.InternalSetKey(k);
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:Microsoft.Xna.Framework.Input.KeyboardState" /> class.
	/// </summary>
	/// <param name="keys">List of keys to be flagged as pressed on initialization.</param>
	/// <param name="capsLock">Caps Lock state.</param>
	/// <param name="numLock">Num Lock state.</param>
	public KeyboardState(Keys[] keys, bool capsLock = false, bool numLock = false)
	{
		this = default(KeyboardState);
		this._keys0 = 0u;
		this._keys1 = 0u;
		this._keys2 = 0u;
		this._keys3 = 0u;
		this._keys4 = 0u;
		this._keys5 = 0u;
		this._keys6 = 0u;
		this._keys7 = 0u;
		this._modifiers = (byte)(0u | (capsLock ? 1u : 0u) | (uint)(numLock ? 2 : 0));
		if (keys != null)
		{
			foreach (Keys k in keys)
			{
				this.InternalSetKey(k);
			}
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:Microsoft.Xna.Framework.Input.KeyboardState" /> class.
	/// </summary>
	/// <param name="keys">List of keys to be flagged as pressed on initialization.</param>
	public KeyboardState(params Keys[] keys)
	{
		this = default(KeyboardState);
		this._keys0 = 0u;
		this._keys1 = 0u;
		this._keys2 = 0u;
		this._keys3 = 0u;
		this._keys4 = 0u;
		this._keys5 = 0u;
		this._keys6 = 0u;
		this._keys7 = 0u;
		this._modifiers = 0;
		if (keys != null)
		{
			foreach (Keys k in keys)
			{
				this.InternalSetKey(k);
			}
		}
	}

	/// <summary>
	/// Gets whether given key is currently being pressed.
	/// </summary>
	/// <param name="key">The key to query.</param>
	/// <returns>true if the key is pressed; false otherwise.</returns>
	public bool IsKeyDown(Keys key)
	{
		return this.InternalGetKey(key);
	}

	/// <summary>
	/// Gets whether given key is currently being not pressed.
	/// </summary>
	/// <param name="key">The key to query.</param>
	/// <returns>true if the key is not pressed; false otherwise.</returns>
	public bool IsKeyUp(Keys key)
	{
		return !this.InternalGetKey(key);
	}

	/// <summary>
	/// Returns the number of pressed keys in this <see cref="T:Microsoft.Xna.Framework.Input.KeyboardState" />.
	/// </summary>
	/// <returns>An integer representing the number of keys currently pressed in this <see cref="T:Microsoft.Xna.Framework.Input.KeyboardState" />.</returns>
	public int GetPressedKeyCount()
	{
		return (int)(KeyboardState.CountBits(this._keys0) + KeyboardState.CountBits(this._keys1) + KeyboardState.CountBits(this._keys2) + KeyboardState.CountBits(this._keys3) + KeyboardState.CountBits(this._keys4) + KeyboardState.CountBits(this._keys5) + KeyboardState.CountBits(this._keys6) + KeyboardState.CountBits(this._keys7));
	}

	private static uint CountBits(uint v)
	{
		v -= (v >> 1) & 0x55555555;
		v = (v & 0x33333333) + ((v >> 2) & 0x33333333);
		return ((v + (v >> 4)) & 0xF0F0F0F) * 16843009 >> 24;
	}

	private static int AddKeysToArray(uint keys, int offset, Keys[] pressedKeys, int index)
	{
		for (int i = 0; i < 32; i++)
		{
			if ((keys & (1 << i)) != 0L)
			{
				pressedKeys[index++] = (Keys)(offset + i);
			}
		}
		return index;
	}

	/// <summary>
	/// Returns an array of values holding keys that are currently being pressed.
	/// </summary>
	/// <returns>The keys that are currently being pressed.</returns>
	public Keys[] GetPressedKeys()
	{
		uint count = KeyboardState.CountBits(this._keys0) + KeyboardState.CountBits(this._keys1) + KeyboardState.CountBits(this._keys2) + KeyboardState.CountBits(this._keys3) + KeyboardState.CountBits(this._keys4) + KeyboardState.CountBits(this._keys5) + KeyboardState.CountBits(this._keys6) + KeyboardState.CountBits(this._keys7);
		if (count == 0)
		{
			return KeyboardState.empty;
		}
		Keys[] keys = new Keys[count];
		int index = 0;
		if (this._keys0 != 0)
		{
			index = KeyboardState.AddKeysToArray(this._keys0, 0, keys, index);
		}
		if (this._keys1 != 0)
		{
			index = KeyboardState.AddKeysToArray(this._keys1, 32, keys, index);
		}
		if (this._keys2 != 0)
		{
			index = KeyboardState.AddKeysToArray(this._keys2, 64, keys, index);
		}
		if (this._keys3 != 0)
		{
			index = KeyboardState.AddKeysToArray(this._keys3, 96, keys, index);
		}
		if (this._keys4 != 0)
		{
			index = KeyboardState.AddKeysToArray(this._keys4, 128, keys, index);
		}
		if (this._keys5 != 0)
		{
			index = KeyboardState.AddKeysToArray(this._keys5, 160, keys, index);
		}
		if (this._keys6 != 0)
		{
			index = KeyboardState.AddKeysToArray(this._keys6, 192, keys, index);
		}
		if (this._keys7 != 0)
		{
			index = KeyboardState.AddKeysToArray(this._keys7, 224, keys, index);
		}
		return keys;
	}

	/// <summary>
	/// Fills an array of values holding keys that are currently being pressed.
	/// </summary>
	/// <param name="keys">The keys array to fill.
	/// This array is not cleared, and it must be equal to or larger than the number of keys pressed.</param>
	public void GetPressedKeys(Keys[] keys)
	{
		if (keys == null)
		{
			throw new ArgumentNullException("keys");
		}
		if (KeyboardState.CountBits(this._keys0) + KeyboardState.CountBits(this._keys1) + KeyboardState.CountBits(this._keys2) + KeyboardState.CountBits(this._keys3) + KeyboardState.CountBits(this._keys4) + KeyboardState.CountBits(this._keys5) + KeyboardState.CountBits(this._keys6) + KeyboardState.CountBits(this._keys7) > keys.Length)
		{
			throw new ArgumentOutOfRangeException("keys", "The supplied array cannot fit the number of pressed keys. Call GetPressedKeyCount() to get the number of pressed keys.");
		}
		int index = 0;
		if (this._keys0 != 0 && index < keys.Length)
		{
			index = KeyboardState.AddKeysToArray(this._keys0, 0, keys, index);
		}
		if (this._keys1 != 0 && index < keys.Length)
		{
			index = KeyboardState.AddKeysToArray(this._keys1, 32, keys, index);
		}
		if (this._keys2 != 0 && index < keys.Length)
		{
			index = KeyboardState.AddKeysToArray(this._keys2, 64, keys, index);
		}
		if (this._keys3 != 0 && index < keys.Length)
		{
			index = KeyboardState.AddKeysToArray(this._keys3, 96, keys, index);
		}
		if (this._keys4 != 0 && index < keys.Length)
		{
			index = KeyboardState.AddKeysToArray(this._keys4, 128, keys, index);
		}
		if (this._keys5 != 0 && index < keys.Length)
		{
			index = KeyboardState.AddKeysToArray(this._keys5, 160, keys, index);
		}
		if (this._keys6 != 0 && index < keys.Length)
		{
			index = KeyboardState.AddKeysToArray(this._keys6, 192, keys, index);
		}
		if (this._keys7 != 0 && index < keys.Length)
		{
			index = KeyboardState.AddKeysToArray(this._keys7, 224, keys, index);
		}
	}

	/// <summary>
	/// Gets the hash code for <see cref="T:Microsoft.Xna.Framework.Input.KeyboardState" /> instance.
	/// </summary>
	/// <returns>Hash code of the object.</returns>
	public override int GetHashCode()
	{
		return (int)(this._keys0 ^ this._keys1 ^ this._keys2 ^ this._keys3 ^ this._keys4 ^ this._keys5 ^ this._keys6 ^ this._keys7);
	}

	/// <summary>
	/// Compares whether two <see cref="T:Microsoft.Xna.Framework.Input.KeyboardState" /> instances are equal.
	/// </summary>
	/// <param name="a"><see cref="T:Microsoft.Xna.Framework.Input.KeyboardState" /> instance to the left of the equality operator.</param>
	/// <param name="b"><see cref="T:Microsoft.Xna.Framework.Input.KeyboardState" /> instance to the right of the equality operator.</param>
	/// <returns>true if the instances are equal; false otherwise.</returns>
	public static bool operator ==(KeyboardState a, KeyboardState b)
	{
		if (a._keys0 == b._keys0 && a._keys1 == b._keys1 && a._keys2 == b._keys2 && a._keys3 == b._keys3 && a._keys4 == b._keys4 && a._keys5 == b._keys5 && a._keys6 == b._keys6)
		{
			return a._keys7 == b._keys7;
		}
		return false;
	}

	/// <summary>
	/// Compares whether two <see cref="T:Microsoft.Xna.Framework.Input.KeyboardState" /> instances are not equal.
	/// </summary>
	/// <param name="a"><see cref="T:Microsoft.Xna.Framework.Input.KeyboardState" /> instance to the left of the inequality operator.</param>
	/// <param name="b"><see cref="T:Microsoft.Xna.Framework.Input.KeyboardState" /> instance to the right of the inequality operator.</param>
	/// <returns>true if the instances are different; false otherwise.</returns>
	public static bool operator !=(KeyboardState a, KeyboardState b)
	{
		return !(a == b);
	}

	/// <summary>
	/// Compares whether current instance is equal to specified object.
	/// </summary>
	/// <param name="obj">The <see cref="T:Microsoft.Xna.Framework.Input.KeyboardState" /> to compare.</param>
	/// <returns>true if the provided <see cref="T:Microsoft.Xna.Framework.Input.KeyboardState" /> instance is same with current; false otherwise.</returns>
	public override bool Equals(object obj)
	{
		if (obj is KeyboardState)
		{
			return this == (KeyboardState)obj;
		}
		return false;
	}
}
