using System;
using System.Runtime.Serialization;

namespace Microsoft.Xna.Framework;

/// <summary>
/// Key point on the <see cref="T:Microsoft.Xna.Framework.Curve" />.
/// </summary>
[DataContract]
public class CurveKey : IEquatable<CurveKey>, IComparable<CurveKey>
{
	private CurveContinuity _continuity;

	private readonly float _position;

	private float _tangentIn;

	private float _tangentOut;

	private float _value;

	/// <summary>
	/// Gets or sets the indicator whether the segment between this point and the next point on the curve is discrete or continuous.
	/// </summary>
	[DataMember]
	public CurveContinuity Continuity
	{
		get
		{
			return this._continuity;
		}
		set
		{
			this._continuity = value;
		}
	}

	/// <summary>
	/// Gets a position of the key on the curve.
	/// </summary>
	[DataMember]
	public float Position => this._position;

	/// <summary>
	/// Gets or sets a tangent when approaching this point from the previous point on the curve.
	/// </summary>
	[DataMember]
	public float TangentIn
	{
		get
		{
			return this._tangentIn;
		}
		set
		{
			this._tangentIn = value;
		}
	}

	/// <summary>
	/// Gets or sets a tangent when leaving this point to the next point on the curve.
	/// </summary>
	[DataMember]
	public float TangentOut
	{
		get
		{
			return this._tangentOut;
		}
		set
		{
			this._tangentOut = value;
		}
	}

	/// <summary>
	/// Gets a value of this point.
	/// </summary>
	[DataMember]
	public float Value
	{
		get
		{
			return this._value;
		}
		set
		{
			this._value = value;
		}
	}

	/// <summary>
	/// Creates a new instance of <see cref="T:Microsoft.Xna.Framework.CurveKey" /> class with position: 0 and value: 0.
	/// </summary>
	public CurveKey()
		: this(0f, 0f)
	{
	}

	/// <summary>
	/// Creates a new instance of <see cref="T:Microsoft.Xna.Framework.CurveKey" /> class.
	/// </summary>
	/// <param name="position">Position on the curve.</param>
	/// <param name="value">Value of the control point.</param>
	public CurveKey(float position, float value)
		: this(position, value, 0f, 0f, CurveContinuity.Smooth)
	{
	}

	/// <summary>
	/// Creates a new instance of <see cref="T:Microsoft.Xna.Framework.CurveKey" /> class.
	/// </summary>
	/// <param name="position">Position on the curve.</param>
	/// <param name="value">Value of the control point.</param>
	/// <param name="tangentIn">Tangent approaching point from the previous point on the curve.</param>
	/// <param name="tangentOut">Tangent leaving point toward next point on the curve.</param>
	public CurveKey(float position, float value, float tangentIn, float tangentOut)
		: this(position, value, tangentIn, tangentOut, CurveContinuity.Smooth)
	{
	}

	/// <summary>
	/// Creates a new instance of <see cref="T:Microsoft.Xna.Framework.CurveKey" /> class.
	/// </summary>
	/// <param name="position">Position on the curve.</param>
	/// <param name="value">Value of the control point.</param>
	/// <param name="tangentIn">Tangent approaching point from the previous point on the curve.</param>
	/// <param name="tangentOut">Tangent leaving point toward next point on the curve.</param>
	/// <param name="continuity">Indicates whether the curve is discrete or continuous.</param>
	public CurveKey(float position, float value, float tangentIn, float tangentOut, CurveContinuity continuity)
	{
		this._position = position;
		this._value = value;
		this._tangentIn = tangentIn;
		this._tangentOut = tangentOut;
		this._continuity = continuity;
	}

	/// <summary>
	///
	/// Compares whether two <see cref="T:Microsoft.Xna.Framework.CurveKey" /> instances are not equal.
	/// </summary>
	/// <param name="value1"><see cref="T:Microsoft.Xna.Framework.CurveKey" /> instance on the left of the not equal sign.</param>
	/// <param name="value2"><see cref="T:Microsoft.Xna.Framework.CurveKey" /> instance on the right of the not equal sign.</param>
	/// <returns><c>true</c> if the instances are not equal; <c>false</c> otherwise.</returns>	
	public static bool operator !=(CurveKey value1, CurveKey value2)
	{
		return !(value1 == value2);
	}

	/// <summary>
	/// Compares whether two <see cref="T:Microsoft.Xna.Framework.CurveKey" /> instances are equal.
	/// </summary>
	/// <param name="value1"><see cref="T:Microsoft.Xna.Framework.CurveKey" /> instance on the left of the equal sign.</param>
	/// <param name="value2"><see cref="T:Microsoft.Xna.Framework.CurveKey" /> instance on the right of the equal sign.</param>
	/// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
	public static bool operator ==(CurveKey value1, CurveKey value2)
	{
		if (object.Equals(value1, null))
		{
			return object.Equals(value2, null);
		}
		if (object.Equals(value2, null))
		{
			return object.Equals(value1, null);
		}
		if (value1._position == value2._position && value1._value == value2._value && value1._tangentIn == value2._tangentIn && value1._tangentOut == value2._tangentOut)
		{
			return value1._continuity == value2._continuity;
		}
		return false;
	}

	/// <summary>
	/// Creates a copy of this key.
	/// </summary>
	/// <returns>A copy of this key.</returns>
	public CurveKey Clone()
	{
		return new CurveKey(this._position, this._value, this._tangentIn, this._tangentOut, this._continuity);
	}

	public int CompareTo(CurveKey other)
	{
		return this._position.CompareTo(other._position);
	}

	public bool Equals(CurveKey other)
	{
		return this == other;
	}

	public override bool Equals(object obj)
	{
		if (obj as CurveKey != null)
		{
			return this.Equals((CurveKey)obj);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return this._position.GetHashCode() ^ this._value.GetHashCode() ^ this._tangentIn.GetHashCode() ^ this._tangentOut.GetHashCode() ^ this._continuity.GetHashCode();
	}
}
