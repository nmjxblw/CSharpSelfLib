using System;

namespace StardewValley;

[InstanceStatics]
internal static class NoiseGenerator
{
	public static int Seed { get; set; }

	public static int Octaves { get; set; }

	public static double Amplitude { get; set; }

	public static double Persistence { get; set; }

	public static double Frequency { get; set; }

	static NoiseGenerator()
	{
		NoiseGenerator.Seed = new Random().Next(int.MaxValue);
		NoiseGenerator.Octaves = 8;
		NoiseGenerator.Amplitude = 1.0;
		NoiseGenerator.Frequency = 0.015;
		NoiseGenerator.Persistence = 0.65;
	}

	public static double Noise(int x, int y)
	{
		double total = 0.0;
		double freq = NoiseGenerator.Frequency;
		double amp = NoiseGenerator.Amplitude;
		for (int i = 0; i < NoiseGenerator.Octaves; i++)
		{
			total += NoiseGenerator.Smooth((double)x * freq, (double)y * freq) * amp;
			freq *= 2.0;
			amp *= NoiseGenerator.Persistence;
		}
		if (total < -2.4)
		{
			total = -2.4;
		}
		else if (total > 2.4)
		{
			total = 2.4;
		}
		return total / 2.4;
	}

	public static double NoiseGeneration(int x, int y)
	{
		int n = x + y * 57;
		n = (n << 13) ^ n;
		return 1.0 - (double)((n * (n * n * 15731 + 789221) + NoiseGenerator.Seed) & 0x7FFFFFFF) / 1073741824.0;
	}

	private static double Interpolate(double x, double y, double a)
	{
		double value = (1.0 - Math.Cos(a * Math.PI)) * 0.5;
		return x * (1.0 - value) + y * value;
	}

	private static double Smooth(double x, double y)
	{
		double x2 = NoiseGenerator.NoiseGeneration((int)x, (int)y);
		double n2 = NoiseGenerator.NoiseGeneration((int)x + 1, (int)y);
		double n3 = NoiseGenerator.NoiseGeneration((int)x, (int)y + 1);
		double n4 = NoiseGenerator.NoiseGeneration((int)x + 1, (int)y + 1);
		double x3 = NoiseGenerator.Interpolate(x2, n2, x - (double)(int)x);
		double i2 = NoiseGenerator.Interpolate(n3, n4, x - (double)(int)x);
		return NoiseGenerator.Interpolate(x3, i2, y - (double)(int)y);
	}
}
