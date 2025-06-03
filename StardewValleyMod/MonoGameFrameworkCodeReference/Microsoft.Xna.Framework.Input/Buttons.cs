using System;

namespace Microsoft.Xna.Framework.Input;

/// <summary>
/// Defines the buttons on gamepad.
/// </summary>
[Flags]
public enum Buttons
{
	/// <summary>
	/// Directional pad up.
	/// </summary>
	DPadUp = 1,
	/// <summary>
	/// Directional pad down.
	/// </summary>
	DPadDown = 2,
	/// <summary>
	/// Directional pad left.
	/// </summary>
	DPadLeft = 4,
	/// <summary>
	/// Directional pad right.
	/// </summary>
	DPadRight = 8,
	/// <summary>
	/// START button.
	/// </summary>
	Start = 0x10,
	/// <summary>
	/// BACK button.
	/// </summary>
	Back = 0x20,
	/// <summary>
	/// Left stick button (pressing the left stick).
	/// </summary>
	LeftStick = 0x40,
	/// <summary>
	/// Right stick button (pressing the right stick).
	/// </summary>
	RightStick = 0x80,
	/// <summary>
	/// Left bumper (shoulder) button.
	/// </summary>
	LeftShoulder = 0x100,
	/// <summary>
	/// Right bumper (shoulder) button.
	/// </summary>
	RightShoulder = 0x200,
	/// <summary>
	/// Big button.
	/// </summary>    
	BigButton = 0x800,
	/// <summary>
	/// A button.
	/// </summary>
	A = 0x1000,
	/// <summary>
	/// B button.
	/// </summary>
	B = 0x2000,
	/// <summary>
	/// X button.
	/// </summary>
	X = 0x4000,
	/// <summary>
	/// Y button.
	/// </summary>
	Y = 0x8000,
	/// <summary>
	/// Left stick is towards the left.
	/// </summary>
	LeftThumbstickLeft = 0x200000,
	/// <summary>
	/// Right trigger.
	/// </summary>
	RightTrigger = 0x400000,
	/// <summary>
	/// Left trigger.
	/// </summary>
	LeftTrigger = 0x800000,
	/// <summary>
	/// Right stick is towards up.
	/// </summary>   
	RightThumbstickUp = 0x1000000,
	/// <summary>
	/// Right stick is towards down.
	/// </summary>   
	RightThumbstickDown = 0x2000000,
	/// <summary>
	/// Right stick is towards the right.
	/// </summary>
	RightThumbstickRight = 0x4000000,
	/// <summary>
	/// Right stick is towards the left.
	/// </summary>
	RightThumbstickLeft = 0x8000000,
	/// <summary>
	/// Left stick is towards up.
	/// </summary>  
	LeftThumbstickUp = 0x10000000,
	/// <summary>
	/// Left stick is towards down.
	/// </summary>  
	LeftThumbstickDown = 0x20000000,
	/// <summary>
	/// Left stick is towards the right.
	/// </summary>
	LeftThumbstickRight = 0x40000000
}
