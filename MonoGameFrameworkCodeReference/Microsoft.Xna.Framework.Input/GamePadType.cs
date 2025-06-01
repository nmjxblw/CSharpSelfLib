namespace Microsoft.Xna.Framework.Input;

/// <summary>
/// Defines a type of gamepad.
/// </summary>
public enum GamePadType
{
	/// <summary>
	/// Unknown.
	/// </summary>
	Unknown = 0,
	/// <summary>
	/// GamePad is the XBOX controller.
	/// </summary>
	GamePad = 1,
	/// <summary>
	/// GamePad is a wheel.
	/// </summary>
	Wheel = 2,
	/// <summary>
	/// GamePad is an arcade stick.
	/// </summary>
	ArcadeStick = 3,
	/// <summary>
	/// GamePad is a flight stick.
	/// </summary>
	FlightStick = 4,
	/// <summary>
	/// GamePad is a dance pad.
	/// </summary>
	DancePad = 5,
	/// <summary>
	/// GamePad is a guitar.
	/// </summary>
	Guitar = 6,
	/// <summary>
	/// GamePad is an alternate guitar.
	/// </summary>
	AlternateGuitar = 7,
	/// <summary>
	/// GamePad is a drum kit.
	/// </summary>
	DrumKit = 8,
	/// <summary>
	/// GamePad is a big button pad.
	/// </summary>
	BigButtonPad = 768
}
