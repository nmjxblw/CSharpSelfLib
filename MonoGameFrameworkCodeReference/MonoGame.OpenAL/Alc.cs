using System;
using System.Runtime.InteropServices;
using MonoGame.Framework.Utilities;

namespace MonoGame.OpenAL;

internal class Alc
{
	[CLSCompliant(false)]
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate IntPtr d_alccreatecontext(IntPtr device, int[] attributes);

	[CLSCompliant(false)]
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate AlcError d_alcgeterror(IntPtr device);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void d_alcgetintegerv(IntPtr device, int param, int size, int[] values);

	[CLSCompliant(false)]
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate IntPtr d_alcgetcurrentcontext();

	[CLSCompliant(false)]
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void d_alcmakecontextcurrent(IntPtr context);

	[CLSCompliant(false)]
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void d_alcdestroycontext(IntPtr context);

	[CLSCompliant(false)]
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void d_alcclosedevice(IntPtr device);

	[CLSCompliant(false)]
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate IntPtr d_alcopendevice(string device);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate IntPtr d_alccaptureopendevice(string device, uint sampleRate, int format, int sampleSize);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate IntPtr d_alccapturestart(IntPtr device);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void d_alccapturesamples(IntPtr device, IntPtr buffer, int samples);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate IntPtr d_alccapturestop(IntPtr device);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate IntPtr d_alccaptureclosedevice(IntPtr device);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate bool d_alcisextensionpresent(IntPtr device, string extensionName);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate IntPtr d_alcgetstring(IntPtr device, int p);

	internal static d_alccreatecontext CreateContext = FuncLoader.LoadFunction<d_alccreatecontext>(AL.NativeLibrary, "alcCreateContext");

	internal static d_alcgeterror GetErrorForDevice = FuncLoader.LoadFunction<d_alcgeterror>(AL.NativeLibrary, "alcGetError");

	internal static d_alcgetintegerv alcGetIntegerv = FuncLoader.LoadFunction<d_alcgetintegerv>(AL.NativeLibrary, "alcGetIntegerv");

	internal static d_alcgetcurrentcontext GetCurrentContext = FuncLoader.LoadFunction<d_alcgetcurrentcontext>(AL.NativeLibrary, "alcGetCurrentContext");

	internal static d_alcmakecontextcurrent MakeContextCurrent = FuncLoader.LoadFunction<d_alcmakecontextcurrent>(AL.NativeLibrary, "alcMakeContextCurrent");

	internal static d_alcdestroycontext DestroyContext = FuncLoader.LoadFunction<d_alcdestroycontext>(AL.NativeLibrary, "alcDestroyContext");

	internal static d_alcclosedevice CloseDevice = FuncLoader.LoadFunction<d_alcclosedevice>(AL.NativeLibrary, "alcCloseDevice");

	internal static d_alcopendevice OpenDevice = FuncLoader.LoadFunction<d_alcopendevice>(AL.NativeLibrary, "alcOpenDevice");

	internal static d_alccaptureopendevice alcCaptureOpenDevice = FuncLoader.LoadFunction<d_alccaptureopendevice>(AL.NativeLibrary, "alcCaptureOpenDevice");

	internal static d_alccapturestart CaptureStart = FuncLoader.LoadFunction<d_alccapturestart>(AL.NativeLibrary, "alcCaptureStart");

	internal static d_alccapturesamples CaptureSamples = FuncLoader.LoadFunction<d_alccapturesamples>(AL.NativeLibrary, "alcCaptureSamples");

	internal static d_alccapturestop CaptureStop = FuncLoader.LoadFunction<d_alccapturestop>(AL.NativeLibrary, "alcCaptureStop");

	internal static d_alccaptureclosedevice CaptureCloseDevice = FuncLoader.LoadFunction<d_alccaptureclosedevice>(AL.NativeLibrary, "alcCaptureCloseDevice");

	internal static d_alcisextensionpresent IsExtensionPresent = FuncLoader.LoadFunction<d_alcisextensionpresent>(AL.NativeLibrary, "alcIsExtensionPresent");

	internal static d_alcgetstring alcGetString = FuncLoader.LoadFunction<d_alcgetstring>(AL.NativeLibrary, "alcGetString");

	internal static AlcError GetError()
	{
		return Alc.GetErrorForDevice(IntPtr.Zero);
	}

	internal static void GetInteger(IntPtr device, AlcGetInteger param, int size, int[] values)
	{
		Alc.alcGetIntegerv(device, (int)param, size, values);
	}

	internal static IntPtr CaptureOpenDevice(string device, uint sampleRate, ALFormat format, int sampleSize)
	{
		return Alc.alcCaptureOpenDevice(device, sampleRate, (int)format, sampleSize);
	}

	internal static string GetString(IntPtr device, int p)
	{
		return Marshal.PtrToStringAnsi(Alc.alcGetString(device, p));
	}

	internal static string GetString(IntPtr device, AlcGetString p)
	{
		return Alc.GetString(device, (int)p);
	}
}
