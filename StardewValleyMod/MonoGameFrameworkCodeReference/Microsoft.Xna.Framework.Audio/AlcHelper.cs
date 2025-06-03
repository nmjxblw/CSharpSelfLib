using System;
using System.Diagnostics;
using MonoGame.OpenAL;

namespace Microsoft.Xna.Framework.Audio;

internal static class AlcHelper
{
	[Conditional("DEBUG")]
	[DebuggerHidden]
	internal static void CheckError(string message = "", params object[] args)
	{
		AlcError error;
		if ((error = Alc.GetError()) != AlcError.NoError)
		{
			if (args != null && args.Length != 0)
			{
				message = string.Format(message, args);
			}
			throw new InvalidOperationException(message + " (Reason: " + error.ToString() + ")");
		}
	}
}
