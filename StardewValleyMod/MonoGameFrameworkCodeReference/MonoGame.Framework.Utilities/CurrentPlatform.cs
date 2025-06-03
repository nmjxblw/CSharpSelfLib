using System;
using System.Runtime.InteropServices;

namespace MonoGame.Framework.Utilities;

internal static class CurrentPlatform
{
	private static bool _init;

	private static OS _os;

	public static OS OS
	{
		get
		{
			CurrentPlatform.Init();
			return CurrentPlatform._os;
		}
	}

	public static string Rid
	{
		get
		{
			if (CurrentPlatform.OS == OS.Windows && Environment.Is64BitProcess)
			{
				return "win-x64";
			}
			if (CurrentPlatform.OS == OS.Windows && !Environment.Is64BitProcess)
			{
				return "win-x86";
			}
			if (CurrentPlatform.OS == OS.Linux)
			{
				return "linux-x64";
			}
			if (CurrentPlatform.OS == OS.MacOSX)
			{
				return "osx";
			}
			return "unknown";
		}
	}

	[DllImport("libc")]
	private static extern int uname(IntPtr buf);

	private static void Init()
	{
		if (CurrentPlatform._init)
		{
			return;
		}
		switch (Environment.OSVersion.Platform)
		{
		case PlatformID.Win32S:
		case PlatformID.Win32Windows:
		case PlatformID.Win32NT:
		case PlatformID.WinCE:
			CurrentPlatform._os = OS.Windows;
			break;
		case PlatformID.MacOSX:
			CurrentPlatform._os = OS.MacOSX;
			break;
		case PlatformID.Unix:
		{
			CurrentPlatform._os = OS.MacOSX;
			IntPtr buf = IntPtr.Zero;
			try
			{
				buf = Marshal.AllocHGlobal(8192);
				if (CurrentPlatform.uname(buf) == 0 && Marshal.PtrToStringAnsi(buf) == "Linux")
				{
					CurrentPlatform._os = OS.Linux;
				}
			}
			catch
			{
			}
			finally
			{
				if (buf != IntPtr.Zero)
				{
					Marshal.FreeHGlobal(buf);
				}
			}
			break;
		}
		default:
			CurrentPlatform._os = OS.Unknown;
			break;
		}
		CurrentPlatform._init = true;
	}
}
