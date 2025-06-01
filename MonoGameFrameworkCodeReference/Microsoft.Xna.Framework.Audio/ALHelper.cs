using System;
using System.Diagnostics;
using MonoGame.OpenAL;

namespace Microsoft.Xna.Framework.Audio;

internal static class ALHelper
{
	[Conditional("DEBUG")]
	[DebuggerHidden]
	internal static void CheckError(string message = "", params object[] args)
	{
		ALError error;
		if ((error = AL.GetError()) != ALError.NoError)
		{
			if (args != null && args.Length != 0)
			{
				message = string.Format(message, args);
			}
			throw new InvalidOperationException(message + " (Reason: " + AL.GetErrorString(error) + ")");
		}
	}

	public static bool IsStereoFormat(ALFormat format)
	{
		if (format != ALFormat.Stereo8 && format != ALFormat.Stereo16 && format != ALFormat.StereoFloat32 && format != ALFormat.StereoIma4)
		{
			return format == ALFormat.StereoMSAdpcm;
		}
		return true;
	}
}
