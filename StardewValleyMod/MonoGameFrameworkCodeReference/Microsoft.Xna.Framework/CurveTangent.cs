namespace Microsoft.Xna.Framework;

/// <summary>
/// Defines the different tangent types to be calculated for <see cref="T:Microsoft.Xna.Framework.CurveKey" /> points in a <see cref="T:Microsoft.Xna.Framework.Curve" />.
/// </summary>
public enum CurveTangent
{
	/// <summary>
	/// The tangent which always has a value equal to zero. 
	/// </summary>
	Flat,
	/// <summary>
	/// The tangent which contains a difference between current tangent value and the tangent value from the previous <see cref="T:Microsoft.Xna.Framework.CurveKey" />. 
	/// </summary>
	Linear,
	/// <summary>
	/// The smoouth tangent which contains the inflection between <see cref="P:Microsoft.Xna.Framework.CurveKey.TangentIn" /> and <see cref="P:Microsoft.Xna.Framework.CurveKey.TangentOut" /> by taking into account the values of both neighbors of the <see cref="T:Microsoft.Xna.Framework.CurveKey" />.
	/// </summary>
	Smooth
}
