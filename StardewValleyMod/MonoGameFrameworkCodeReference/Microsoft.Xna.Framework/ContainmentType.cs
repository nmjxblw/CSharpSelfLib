namespace Microsoft.Xna.Framework;

/// <summary>
/// Defines how the bounding volumes intersects or contain one another.
/// </summary>
public enum ContainmentType
{
	/// <summary>
	/// Indicates that there is no overlap between two bounding volumes.
	/// </summary>
	Disjoint,
	/// <summary>
	/// Indicates that one bounding volume completely contains another volume.
	/// </summary>
	Contains,
	/// <summary>
	/// Indicates that bounding volumes partially overlap one another.
	/// </summary>
	Intersects
}
