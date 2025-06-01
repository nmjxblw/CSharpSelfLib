using System;
using System.IO;
using System.Runtime.InteropServices;

namespace MonoGame.Framework.Utilities;

internal class FuncLoader
{
	private class Windows
	{
		[DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
		public static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

		[DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
		public static extern IntPtr LoadLibraryW(string lpszLib);
	}

	private class Linux
	{
		[DllImport("libdl.so.2")]
		public static extern IntPtr dlopen(string path, int flags);

		[DllImport("libdl.so.2")]
		public static extern IntPtr dlsym(IntPtr handle, string symbol);
	}

	private class OSX
	{
		[DllImport("/usr/lib/libSystem.dylib")]
		public static extern IntPtr dlopen(string path, int flags);

		[DllImport("/usr/lib/libSystem.dylib")]
		public static extern IntPtr dlsym(IntPtr handle, string symbol);
	}

	private const int RTLD_LAZY = 1;

	public static IntPtr LoadLibraryExt(string libname)
	{
		IntPtr ret = IntPtr.Zero;
		string assemblyLocation = Path.GetDirectoryName(typeof(FuncLoader).Assembly.Location) ?? "./";
		if (CurrentPlatform.OS == OS.MacOSX)
		{
			ret = FuncLoader.LoadLibrary(Path.Combine(assemblyLocation, libname));
			if (ret == IntPtr.Zero)
			{
				ret = FuncLoader.LoadLibrary(Path.Combine(assemblyLocation, "..", "Frameworks", libname));
			}
		}
		if (ret == IntPtr.Zero)
		{
			ret = FuncLoader.LoadLibrary(Path.Combine(assemblyLocation, "runtimes", CurrentPlatform.Rid, "native", libname));
		}
		if (ret == IntPtr.Zero)
		{
			ret = FuncLoader.LoadLibrary(Path.Combine(assemblyLocation, libname));
		}
		if (ret == IntPtr.Zero)
		{
			ret = FuncLoader.LoadLibrary(libname);
		}
		if (ret == IntPtr.Zero)
		{
			throw new Exception("Failed to load library: " + libname);
		}
		return ret;
	}

	public static IntPtr LoadLibrary(string libname)
	{
		if (CurrentPlatform.OS == OS.Windows)
		{
			return Windows.LoadLibraryW(libname);
		}
		if (CurrentPlatform.OS == OS.MacOSX)
		{
			return OSX.dlopen(libname, 1);
		}
		return Linux.dlopen(libname, 1);
	}

	public static T LoadFunction<T>(IntPtr library, string function, bool throwIfNotFound = false)
	{
		IntPtr ret = IntPtr.Zero;
		ret = ((CurrentPlatform.OS == OS.Windows) ? Windows.GetProcAddress(library, function) : ((CurrentPlatform.OS != OS.MacOSX) ? Linux.dlsym(library, function) : OSX.dlsym(library, function)));
		if (ret == IntPtr.Zero)
		{
			if (throwIfNotFound)
			{
				throw new EntryPointNotFoundException(function);
			}
			return default(T);
		}
		return Marshal.GetDelegateForFunctionPointer<T>(ret);
	}
}
