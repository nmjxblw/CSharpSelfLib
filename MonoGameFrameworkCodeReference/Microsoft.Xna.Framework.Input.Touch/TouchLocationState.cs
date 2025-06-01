namespace Microsoft.Xna.Framework.Input.Touch;

/// <summary>
/// Holds the possible state information for a touch location..
/// </summary>
public enum TouchLocationState
{
	/// <summary>
	/// This touch location position is invalid.
	/// </summary>
	/// <remarks>Typically, you will encounter this state when a new touch location attempts to get the previous state of itself.</remarks>
	Invalid,
	/// <summary>
	/// This touch location position was updated or pressed at the same position.
	/// </summary>
	Moved,
	/// <summary>
	/// This touch location position is new. 
	/// </summary>
	Pressed,
	/// <summary>
	/// This touch location position was released. 
	/// </summary>
	Released
}
