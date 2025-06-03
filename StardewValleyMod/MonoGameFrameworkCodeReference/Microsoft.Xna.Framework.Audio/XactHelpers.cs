using System;

namespace Microsoft.Xna.Framework.Audio;

internal static class XactHelpers
{
	internal static readonly Random Random = new Random();

	public static float ParseDecibels(byte decibles)
	{
		return (float)(-163.7385212334047 / (1.0 + Math.Pow((double)(int)decibles / 80.1748600297963, 0.432254984608615)) + 67.7385212334047);
	}

	public static float ParseVolumeFromDecibels(byte decibles)
	{
		return XactHelpers.ParseVolumeFromDecibels((float)(-163.7385212334047 / (1.0 + Math.Pow((double)(int)decibles / 80.1748600297963, 0.432254984608615)) + 67.7385212334047));
	}

	public static float ParseVolumeFromDecibels(float decibles)
	{
		return (float)Math.Pow(10.0, (double)decibles / 20.0);
	}
}
