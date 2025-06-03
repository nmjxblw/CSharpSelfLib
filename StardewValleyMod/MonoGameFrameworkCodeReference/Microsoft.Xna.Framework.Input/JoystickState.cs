using System.Linq;
using System.Text;

namespace Microsoft.Xna.Framework.Input;

/// <summary>
/// Describes current joystick state.
/// </summary>
public struct JoystickState
{
	/// <summary>
	/// Gets a value indicating whether the joystick is connected.
	/// </summary>
	/// <value><c>true</c> if the joystick is connected; otherwise, <c>false</c>.</value>
	public bool IsConnected { get; internal set; }

	/// <summary>
	/// Gets the joystick axis values.
	/// </summary>
	/// <value>An array list of ints that indicate axis values.</value>
	public int[] Axes { get; internal set; }

	/// <summary>
	/// Gets the joystick button values.
	/// </summary>
	/// <value>An array list of ButtonState that indicate button values.</value>
	public ButtonState[] Buttons { get; internal set; }

	/// <summary>
	/// Gets the joystick hat values.
	/// </summary>
	/// <value>An array list of <see cref="T:Microsoft.Xna.Framework.Input.JoystickHat" /> that indicate hat values.</value>
	public JoystickHat[] Hats { get; internal set; }

	/// <summary>
	/// Determines whether a specified instance of <see cref="T:Microsoft.Xna.Framework.Input.JoystickState" /> is
	/// equal to another specified <see cref="T:Microsoft.Xna.Framework.Input.JoystickState" />.
	/// </summary>
	/// <param name="left">The first <see cref="T:Microsoft.Xna.Framework.Input.JoystickState" /> to compare.</param>
	/// <param name="right">The second <see cref="T:Microsoft.Xna.Framework.Input.JoystickState" /> to compare.</param>
	/// <returns><c>true</c> if <c>left</c> and <c>right</c> are equal; otherwise, <c>false</c>.</returns>
	public static bool operator ==(JoystickState left, JoystickState right)
	{
		if (left.IsConnected == right.IsConnected && left.Axes.SequenceEqual(right.Axes) && left.Buttons.SequenceEqual(right.Buttons))
		{
			return left.Hats.SequenceEqual(right.Hats);
		}
		return false;
	}

	/// <summary>
	/// Determines whether a specified instance of <see cref="T:Microsoft.Xna.Framework.Input.JoystickState" /> is not
	/// equal to another specified <see cref="T:Microsoft.Xna.Framework.Input.JoystickState" />.
	/// </summary>
	/// <param name="left">The first <see cref="T:Microsoft.Xna.Framework.Input.JoystickState" /> to compare.</param>
	/// <param name="right">The second <see cref="T:Microsoft.Xna.Framework.Input.JoystickState" /> to compare.</param>
	/// <returns><c>true</c> if <c>left</c> and <c>right</c> are not equal; otherwise, <c>false</c>.</returns>
	public static bool operator !=(JoystickState left, JoystickState right)
	{
		return !(left == right);
	}

	/// <summary>
	/// Determines whether the specified <see cref="T:System.Object" /> is equal to the current <see cref="T:Microsoft.Xna.Framework.Input.JoystickState" />.
	/// </summary>
	/// <param name="obj">The <see cref="T:System.Object" /> to compare with the current <see cref="T:Microsoft.Xna.Framework.Input.JoystickState" />.</param>
	/// <returns><c>true</c> if the specified <see cref="T:System.Object" /> is equal to the current
	/// <see cref="T:Microsoft.Xna.Framework.Input.JoystickState" />; otherwise, <c>false</c>.</returns>
	public override bool Equals(object obj)
	{
		if (obj is JoystickState)
		{
			return this == (JoystickState)obj;
		}
		return false;
	}

	/// <summary>
	/// Serves as a hash function for a <see cref="T:Microsoft.Xna.Framework.Input.JoystickState" /> object.
	/// </summary>
	/// <returns>A hash code for this instance that is suitable for use in hashing algorithms and data structures such as a
	/// hash table.</returns>
	public override int GetHashCode()
	{
		int hash = 0;
		if (this.IsConnected)
		{
			int[] axes = this.Axes;
			foreach (int axis in axes)
			{
				hash = (hash * 397) ^ axis;
			}
			for (int j = 0; j < this.Buttons.Length; j++)
			{
				hash ^= (int)this.Buttons[j] << j % 32;
			}
			JoystickHat[] hats = this.Hats;
			for (int i = 0; i < hats.Length; i++)
			{
				JoystickHat hat = hats[i];
				hash = (hash * 397) ^ hat.GetHashCode();
			}
		}
		return hash;
	}

	/// <summary>
	/// Returns a <see cref="T:System.String" /> that represents the current <see cref="T:Microsoft.Xna.Framework.Input.JoystickState" />.
	/// </summary>
	/// <returns>A <see cref="T:System.String" /> that represents the current <see cref="T:Microsoft.Xna.Framework.Input.JoystickState" />.</returns>
	public override string ToString()
	{
		StringBuilder ret = new StringBuilder(52 + this.Axes.Length * 7 + this.Buttons.Length + this.Hats.Length * 5);
		ret.Append("[JoystickState: IsConnected=" + (this.IsConnected ? 1 : 0));
		if (this.IsConnected)
		{
			ret.Append(", Axes=");
			int[] axes = this.Axes;
			for (int i = 0; i < axes.Length; i++)
			{
				int axis = axes[i];
				ret.Append(((axis > 0) ? "+" : "") + axis.ToString("00000") + " ");
			}
			ret.Length--;
			ret.Append(", Buttons=");
			ButtonState[] buttons = this.Buttons;
			foreach (ButtonState button in buttons)
			{
				ret.Append((int)button);
			}
			ret.Append(", Hats=");
			JoystickHat[] hats = this.Hats;
			foreach (JoystickHat hat in hats)
			{
				JoystickHat joystickHat = hat;
				ret.Append(joystickHat.ToString() + " ");
			}
			ret.Length--;
		}
		ret.Append("]");
		return ret.ToString();
	}
}
