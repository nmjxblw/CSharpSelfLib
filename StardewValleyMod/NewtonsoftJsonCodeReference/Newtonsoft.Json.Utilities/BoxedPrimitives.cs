using System;

namespace Newtonsoft.Json.Utilities;

internal static class BoxedPrimitives
{
	internal static readonly object BooleanTrue = true;

	internal static readonly object BooleanFalse = false;

	internal static readonly object Int32_M1 = -1;

	internal static readonly object Int32_0 = 0;

	internal static readonly object Int32_1 = 1;

	internal static readonly object Int32_2 = 2;

	internal static readonly object Int32_3 = 3;

	internal static readonly object Int32_4 = 4;

	internal static readonly object Int32_5 = 5;

	internal static readonly object Int32_6 = 6;

	internal static readonly object Int32_7 = 7;

	internal static readonly object Int32_8 = 8;

	internal static readonly object Int64_M1 = -1L;

	internal static readonly object Int64_0 = 0L;

	internal static readonly object Int64_1 = 1L;

	internal static readonly object Int64_2 = 2L;

	internal static readonly object Int64_3 = 3L;

	internal static readonly object Int64_4 = 4L;

	internal static readonly object Int64_5 = 5L;

	internal static readonly object Int64_6 = 6L;

	internal static readonly object Int64_7 = 7L;

	internal static readonly object Int64_8 = 8L;

	private static readonly object DecimalZero = 0m;

	private static readonly object DecimalZeroWithTrailingZero = 0.0m;

	internal static readonly object DoubleNaN = double.NaN;

	internal static readonly object DoublePositiveInfinity = double.PositiveInfinity;

	internal static readonly object DoubleNegativeInfinity = double.NegativeInfinity;

	internal static readonly object DoubleZero = 0.0;

	internal static readonly object DoubleNegativeZero = -0.0;

	internal static object Get(bool value)
	{
		if (!value)
		{
			return BoxedPrimitives.BooleanFalse;
		}
		return BoxedPrimitives.BooleanTrue;
	}

	internal static object Get(int value)
	{
		return value switch
		{
			-1 => BoxedPrimitives.Int32_M1, 
			0 => BoxedPrimitives.Int32_0, 
			1 => BoxedPrimitives.Int32_1, 
			2 => BoxedPrimitives.Int32_2, 
			3 => BoxedPrimitives.Int32_3, 
			4 => BoxedPrimitives.Int32_4, 
			5 => BoxedPrimitives.Int32_5, 
			6 => BoxedPrimitives.Int32_6, 
			7 => BoxedPrimitives.Int32_7, 
			8 => BoxedPrimitives.Int32_8, 
			_ => value, 
		};
	}

	internal static object Get(long value)
	{
		long num = value - -1;
		if ((ulong)num <= 9uL)
		{
			switch (num)
			{
			case 0L:
				return BoxedPrimitives.Int64_M1;
			case 1L:
				return BoxedPrimitives.Int64_0;
			case 2L:
				return BoxedPrimitives.Int64_1;
			case 3L:
				return BoxedPrimitives.Int64_2;
			case 4L:
				return BoxedPrimitives.Int64_3;
			case 5L:
				return BoxedPrimitives.Int64_4;
			case 6L:
				return BoxedPrimitives.Int64_5;
			case 7L:
				return BoxedPrimitives.Int64_6;
			case 8L:
				return BoxedPrimitives.Int64_7;
			case 9L:
				return BoxedPrimitives.Int64_8;
			}
		}
		return value;
	}

	internal static object Get(decimal value)
	{
		if (value == 0m)
		{
			Span<int> destination = stackalloc int[4];
			decimal.GetBits(value, destination);
			switch ((byte)(destination[3] >> 16))
			{
			case 0:
				return BoxedPrimitives.DecimalZero;
			case 1:
				return BoxedPrimitives.DecimalZeroWithTrailingZero;
			}
		}
		return value;
	}

	internal static object Get(double value)
	{
		if (value == 0.0)
		{
			if (!double.IsNegativeInfinity(1.0 / value))
			{
				return BoxedPrimitives.DoubleZero;
			}
			return BoxedPrimitives.DoubleNegativeZero;
		}
		if (double.IsInfinity(value))
		{
			if (!double.IsPositiveInfinity(value))
			{
				return BoxedPrimitives.DoubleNegativeInfinity;
			}
			return BoxedPrimitives.DoublePositiveInfinity;
		}
		if (double.IsNaN(value))
		{
			return BoxedPrimitives.DoubleNaN;
		}
		return value;
	}
}
