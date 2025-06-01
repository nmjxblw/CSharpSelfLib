using System;

namespace Microsoft.Xna.Framework;

/// <summary>
/// Contains commonly used precalculated values and mathematical operations.
/// </summary>
public static class MathHelper
{
	/// <summary>
	/// Represents the mathematical constant e(2.71828175).
	/// </summary>
	public const float E = (float)Math.E;

	/// <summary>
	/// Represents the log base ten of e(0.4342945).
	/// </summary>
	public const float Log10E = 0.4342945f;

	/// <summary>
	/// Represents the log base two of e(1.442695).
	/// </summary>
	public const float Log2E = 1.442695f;

	/// <summary>
	/// Represents the value of pi(3.14159274).
	/// </summary>
	public const float Pi = (float)Math.PI;

	/// <summary>
	/// Represents the value of pi divided by two(1.57079637).
	/// </summary>
	public const float PiOver2 = (float)Math.PI / 2f;

	/// <summary>
	/// Represents the value of pi divided by four(0.7853982).
	/// </summary>
	public const float PiOver4 = (float)Math.PI / 4f;

	/// <summary>
	/// Represents the value of pi times two(6.28318548).
	/// </summary>
	public const float TwoPi = (float)Math.PI * 2f;

	/// <summary>
	/// Represents the value of pi times two(6.28318548).
	/// This is an alias of TwoPi.
	/// </summary>
	public const float Tau = (float)Math.PI * 2f;

	/// <summary>
	/// Returns the Cartesian coordinate for one axis of a point that is defined by a given triangle and two normalized barycentric (areal) coordinates.
	/// </summary>
	/// <param name="value1">The coordinate on one axis of vertex 1 of the defining triangle.</param>
	/// <param name="value2">The coordinate on the same axis of vertex 2 of the defining triangle.</param>
	/// <param name="value3">The coordinate on the same axis of vertex 3 of the defining triangle.</param>
	/// <param name="amount1">The normalized barycentric (areal) coordinate b2, equal to the weighting factor for vertex 2, the coordinate of which is specified in value2.</param>
	/// <param name="amount2">The normalized barycentric (areal) coordinate b3, equal to the weighting factor for vertex 3, the coordinate of which is specified in value3.</param>
	/// <returns>Cartesian coordinate of the specified point with respect to the axis being used.</returns>
	public static float Barycentric(float value1, float value2, float value3, float amount1, float amount2)
	{
		return value1 + (value2 - value1) * amount1 + (value3 - value1) * amount2;
	}

	/// <summary>
	/// Performs a Catmull-Rom interpolation using the specified positions.
	/// </summary>
	/// <param name="value1">The first position in the interpolation.</param>
	/// <param name="value2">The second position in the interpolation.</param>
	/// <param name="value3">The third position in the interpolation.</param>
	/// <param name="value4">The fourth position in the interpolation.</param>
	/// <param name="amount">Weighting factor.</param>
	/// <returns>A position that is the result of the Catmull-Rom interpolation.</returns>
	public static float CatmullRom(float value1, float value2, float value3, float value4, float amount)
	{
		double amountSquared = amount * amount;
		double amountCubed = amountSquared * (double)amount;
		return (float)(0.5 * (2.0 * (double)value2 + (double)((value3 - value1) * amount) + (2.0 * (double)value1 - 5.0 * (double)value2 + 4.0 * (double)value3 - (double)value4) * amountSquared + (3.0 * (double)value2 - (double)value1 - 3.0 * (double)value3 + (double)value4) * amountCubed));
	}

	/// <summary>
	/// Restricts a value to be within a specified range.
	/// </summary>
	/// <param name="value">The value to clamp.</param>
	/// <param name="min">The minimum value. If <c>value</c> is less than <c>min</c>, <c>min</c> will be returned.</param>
	/// <param name="max">The maximum value. If <c>value</c> is greater than <c>max</c>, <c>max</c> will be returned.</param>
	/// <returns>The clamped value.</returns>
	public static float Clamp(float value, float min, float max)
	{
		value = ((value > max) ? max : value);
		value = ((value < min) ? min : value);
		return value;
	}

	/// <summary>
	/// Restricts a value to be within a specified range.
	/// </summary>
	/// <param name="value">The value to clamp.</param>
	/// <param name="min">The minimum value. If <c>value</c> is less than <c>min</c>, <c>min</c> will be returned.</param>
	/// <param name="max">The maximum value. If <c>value</c> is greater than <c>max</c>, <c>max</c> will be returned.</param>
	/// <returns>The clamped value.</returns>
	public static int Clamp(int value, int min, int max)
	{
		value = ((value > max) ? max : value);
		value = ((value < min) ? min : value);
		return value;
	}

	/// <summary>
	/// Calculates the absolute value of the difference of two values.
	/// </summary>
	/// <param name="value1">Source value.</param>
	/// <param name="value2">Source value.</param>
	/// <returns>Distance between the two values.</returns>
	public static float Distance(float value1, float value2)
	{
		return Math.Abs(value1 - value2);
	}

	/// <summary>
	/// Performs a Hermite spline interpolation.
	/// </summary>
	/// <param name="value1">Source position.</param>
	/// <param name="tangent1">Source tangent.</param>
	/// <param name="value2">Source position.</param>
	/// <param name="tangent2">Source tangent.</param>
	/// <param name="amount">Weighting factor.</param>
	/// <returns>The result of the Hermite spline interpolation.</returns>
	public static float Hermite(float value1, float tangent1, float value2, float tangent2, float amount)
	{
		double v1 = value1;
		double v2 = value2;
		double t1 = tangent1;
		double t2 = tangent2;
		double s = amount;
		double sCubed = s * s * s;
		double sSquared = s * s;
		double result = ((amount == 0f) ? ((double)value1) : ((amount != 1f) ? ((2.0 * v1 - 2.0 * v2 + t2 + t1) * sCubed + (3.0 * v2 - 3.0 * v1 - 2.0 * t1 - t2) * sSquared + t1 * s + v1) : ((double)value2)));
		return (float)result;
	}

	/// <summary>
	/// Linearly interpolates between two values.
	/// </summary>
	/// <param name="value1">Source value.</param>
	/// <param name="value2">Destination value.</param>
	/// <param name="amount">Value between 0 and 1 indicating the weight of value2.</param>
	/// <returns>Interpolated value.</returns> 
	/// <remarks>This method performs the linear interpolation based on the following formula:
	/// <code>value1 + (value2 - value1) * amount</code>.
	/// Passing amount a value of 0 will cause value1 to be returned, a value of 1 will cause value2 to be returned.
	/// See <see cref="M:Microsoft.Xna.Framework.MathHelper.LerpPrecise(System.Single,System.Single,System.Single)" /> for a less efficient version with more precision around edge cases.
	/// </remarks>
	public static float Lerp(float value1, float value2, float amount)
	{
		return value1 + (value2 - value1) * amount;
	}

	/// <summary>
	/// Linearly interpolates between two values.
	/// This method is a less efficient, more precise version of <see cref="M:Microsoft.Xna.Framework.MathHelper.Lerp(System.Single,System.Single,System.Single)" />.
	/// See remarks for more info.
	/// </summary>
	/// <param name="value1">Source value.</param>
	/// <param name="value2">Destination value.</param>
	/// <param name="amount">Value between 0 and 1 indicating the weight of value2.</param>
	/// <returns>Interpolated value.</returns>
	/// <remarks>This method performs the linear interpolation based on the following formula:
	/// <code>((1 - amount) * value1) + (value2 * amount)</code>.
	/// Passing amount a value of 0 will cause value1 to be returned, a value of 1 will cause value2 to be returned.
	/// This method does not have the floating point precision issue that <see cref="M:Microsoft.Xna.Framework.MathHelper.Lerp(System.Single,System.Single,System.Single)" /> has.
	/// i.e. If there is a big gap between value1 and value2 in magnitude (e.g. value1=10000000000000000, value2=1),
	/// right at the edge of the interpolation range (amount=1), <see cref="M:Microsoft.Xna.Framework.MathHelper.Lerp(System.Single,System.Single,System.Single)" /> will return 0 (whereas it should return 1).
	/// This also holds for value1=10^17, value2=10; value1=10^18,value2=10^2... so on.
	/// For an in depth explanation of the issue, see below references:
	/// Relevant Wikipedia Article: https://en.wikipedia.org/wiki/Linear_interpolation#Programming_language_support
	/// Relevant StackOverflow Answer: http://stackoverflow.com/questions/4353525/floating-point-linear-interpolation#answer-23716956
	/// </remarks>
	public static float LerpPrecise(float value1, float value2, float amount)
	{
		return (1f - amount) * value1 + value2 * amount;
	}

	/// <summary>
	/// Returns the greater of two values.
	/// </summary>
	/// <param name="value1">Source value.</param>
	/// <param name="value2">Source value.</param>
	/// <returns>The greater value.</returns>
	public static float Max(float value1, float value2)
	{
		if (!(value1 > value2))
		{
			return value2;
		}
		return value1;
	}

	/// <summary>
	/// Returns the greater of two values.
	/// </summary>
	/// <param name="value1">Source value.</param>
	/// <param name="value2">Source value.</param>
	/// <returns>The greater value.</returns>
	public static int Max(int value1, int value2)
	{
		if (value1 <= value2)
		{
			return value2;
		}
		return value1;
	}

	/// <summary>
	/// Returns the lesser of two values.
	/// </summary>
	/// <param name="value1">Source value.</param>
	/// <param name="value2">Source value.</param>
	/// <returns>The lesser value.</returns>
	public static float Min(float value1, float value2)
	{
		if (!(value1 < value2))
		{
			return value2;
		}
		return value1;
	}

	/// <summary>
	/// Returns the lesser of two values.
	/// </summary>
	/// <param name="value1">Source value.</param>
	/// <param name="value2">Source value.</param>
	/// <returns>The lesser value.</returns>
	public static int Min(int value1, int value2)
	{
		if (value1 >= value2)
		{
			return value2;
		}
		return value1;
	}

	/// <summary>
	/// Interpolates between two values using a cubic equation.
	/// </summary>
	/// <param name="value1">Source value.</param>
	/// <param name="value2">Source value.</param>
	/// <param name="amount">Weighting value.</param>
	/// <returns>Interpolated value.</returns>
	public static float SmoothStep(float value1, float value2, float amount)
	{
		float result = MathHelper.Clamp(amount, 0f, 1f);
		return MathHelper.Hermite(value1, 0f, value2, 0f, result);
	}

	/// <summary>
	/// Converts radians to degrees.
	/// </summary>
	/// <param name="radians">The angle in radians.</param>
	/// <returns>The angle in degrees.</returns>
	/// <remarks>
	/// This method uses double precission internally,
	/// though it returns single float
	/// Factor = 180 / pi
	/// </remarks>
	public static float ToDegrees(float radians)
	{
		return (float)((double)radians * (180.0 / Math.PI));
	}

	/// <summary>
	/// Converts degrees to radians.
	/// </summary>
	/// <param name="degrees">The angle in degrees.</param>
	/// <returns>The angle in radians.</returns>
	/// <remarks>
	/// This method uses double precission internally,
	/// though it returns single float
	/// Factor = pi / 180
	/// </remarks>
	public static float ToRadians(float degrees)
	{
		return (float)((double)degrees * (Math.PI / 180.0));
	}

	/// <summary>
	/// Reduces a given angle to a value between π and -π.
	/// </summary>
	/// <param name="angle">The angle to reduce, in radians.</param>
	/// <returns>The new angle, in radians.</returns>
	public static float WrapAngle(float angle)
	{
		if (angle > -(float)Math.PI && angle <= (float)Math.PI)
		{
			return angle;
		}
		angle %= (float)Math.PI * 2f;
		if (angle <= -(float)Math.PI)
		{
			return angle + (float)Math.PI * 2f;
		}
		if (angle > (float)Math.PI)
		{
			return angle - (float)Math.PI * 2f;
		}
		return angle;
	}

	/// <summary>
	/// Determines if value is powered by two.
	/// </summary>
	/// <param name="value">A value.</param>
	/// <returns><c>true</c> if <c>value</c> is powered by two; otherwise <c>false</c>.</returns>
	public static bool IsPowerOfTwo(int value)
	{
		if (value > 0)
		{
			return (value & (value - 1)) == 0;
		}
		return false;
	}
}
