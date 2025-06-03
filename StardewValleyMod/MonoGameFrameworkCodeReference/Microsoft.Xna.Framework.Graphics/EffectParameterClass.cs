namespace Microsoft.Xna.Framework.Graphics;

/// <summary>
/// Defines classes for effect parameters and shader constants.
/// </summary>
public enum EffectParameterClass
{
	/// <summary>
	/// Scalar class type.
	/// </summary>
	Scalar,
	/// <summary>
	/// Vector class type.
	/// </summary>
	Vector,
	/// <summary>
	/// Matrix class type.
	/// </summary>
	Matrix,
	/// <summary>
	/// Class type for textures, shaders or strings. 
	/// </summary>
	Object,
	/// <summary>
	/// Structure class type.
	/// </summary>
	Struct
}
