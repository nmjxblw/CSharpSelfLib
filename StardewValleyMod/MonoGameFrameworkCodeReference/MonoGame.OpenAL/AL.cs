using System;
using System.Runtime.InteropServices;
using MonoGame.Framework.Utilities;

namespace MonoGame.OpenAL;

internal class AL
{
	[CLSCompliant(false)]
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void d_alenable(int cap);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void d_albufferdata(uint bid, int format, IntPtr data, int size, int freq);

	[CLSCompliant(false)]
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal unsafe delegate void d_aldeletebuffers(int n, int* buffers);

	[CLSCompliant(false)]
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void d_albufferi(int buffer, ALBufferi param, int value);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void d_algetbufferi(int bid, ALGetBufferi param, out int value);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void d_albufferiv(int bid, ALBufferi param, int[] values);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal unsafe delegate void d_algenbuffers(int count, int* buffers);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void d_algensources(int n, uint[] sources);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate ALError d_algeterror();

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate bool d_alisbuffer(uint buffer);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void d_alsourcepause(uint source);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void d_alsourceplay(uint source);

	[CLSCompliant(false)]
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate bool d_alissource(int source);

	[CLSCompliant(false)]
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void d_aldeletesources(int n, ref int sources);

	[CLSCompliant(false)]
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void d_alsourcestop(int sourceId);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void d_alsourcei(int sourceId, int i, int a);

	[CLSCompliant(false)]
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void d_alsource3i(int sourceId, ALSourcei i, int a, int b, int c);

	[CLSCompliant(false)]
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void d_alsourcef(int sourceId, ALSourcef i, float a);

	[CLSCompliant(false)]
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void d_alsource3f(int sourceId, ALSource3f i, float x, float y, float z);

	[CLSCompliant(false)]
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void d_algetsourcei(int sourceId, ALGetSourcei i, out int state);

	[CLSCompliant(false)]
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void d_algetlistener3f(ALListener3f param, out float value1, out float value2, out float value3);

	[CLSCompliant(false)]
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void d_aldistancemodel(ALDistanceModel model);

	[CLSCompliant(false)]
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void d_aldopplerfactor(float value);

	[CLSCompliant(false)]
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal unsafe delegate void d_alsourcequeuebuffers(int sourceId, int numEntries, int* buffers);

	[CLSCompliant(false)]
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal unsafe delegate void d_alsourceunqueuebuffers(int sourceId, int numEntries, int* salvaged);

	[CLSCompliant(false)]
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate void d_alsourceunqueuebuffers2(int sid, int numEntries, out int[] bids);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate int d_algetenumvalue(string enumName);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate bool d_alisextensionpresent(string extensionName);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	internal delegate IntPtr d_algetprocaddress(string functionName);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private delegate IntPtr d_algetstring(int p);

	public static IntPtr NativeLibrary = AL.GetNativeLibrary();

	internal static d_alenable Enable = FuncLoader.LoadFunction<d_alenable>(AL.NativeLibrary, "alEnable");

	internal static d_albufferdata alBufferData = FuncLoader.LoadFunction<d_albufferdata>(AL.NativeLibrary, "alBufferData");

	internal static d_aldeletebuffers alDeleteBuffers = FuncLoader.LoadFunction<d_aldeletebuffers>(AL.NativeLibrary, "alDeleteBuffers");

	internal static d_albufferi Bufferi = FuncLoader.LoadFunction<d_albufferi>(AL.NativeLibrary, "alBufferi");

	internal static d_algetbufferi GetBufferi = FuncLoader.LoadFunction<d_algetbufferi>(AL.NativeLibrary, "alGetBufferi");

	internal static d_albufferiv Bufferiv = FuncLoader.LoadFunction<d_albufferiv>(AL.NativeLibrary, "alBufferiv");

	internal static d_algenbuffers alGenBuffers = FuncLoader.LoadFunction<d_algenbuffers>(AL.NativeLibrary, "alGenBuffers");

	internal static d_algensources alGenSources = FuncLoader.LoadFunction<d_algensources>(AL.NativeLibrary, "alGenSources");

	internal static d_algeterror GetError = FuncLoader.LoadFunction<d_algeterror>(AL.NativeLibrary, "alGetError");

	internal static d_alisbuffer alIsBuffer = FuncLoader.LoadFunction<d_alisbuffer>(AL.NativeLibrary, "alIsBuffer");

	internal static d_alsourcepause alSourcePause = FuncLoader.LoadFunction<d_alsourcepause>(AL.NativeLibrary, "alSourcePause");

	internal static d_alsourceplay alSourcePlay = FuncLoader.LoadFunction<d_alsourceplay>(AL.NativeLibrary, "alSourcePlay");

	internal static d_alissource IsSource = FuncLoader.LoadFunction<d_alissource>(AL.NativeLibrary, "alIsSource");

	internal static d_aldeletesources alDeleteSources = FuncLoader.LoadFunction<d_aldeletesources>(AL.NativeLibrary, "alDeleteSources");

	internal static d_alsourcestop SourceStop = FuncLoader.LoadFunction<d_alsourcestop>(AL.NativeLibrary, "alSourceStop");

	internal static d_alsourcei alSourcei = FuncLoader.LoadFunction<d_alsourcei>(AL.NativeLibrary, "alSourcei");

	internal static d_alsource3i alSource3i = FuncLoader.LoadFunction<d_alsource3i>(AL.NativeLibrary, "alSource3i");

	internal static d_alsourcef alSourcef = FuncLoader.LoadFunction<d_alsourcef>(AL.NativeLibrary, "alSourcef");

	internal static d_alsource3f alSource3f = FuncLoader.LoadFunction<d_alsource3f>(AL.NativeLibrary, "alSource3f");

	internal static d_algetsourcei GetSource = FuncLoader.LoadFunction<d_algetsourcei>(AL.NativeLibrary, "alGetSourcei");

	internal static d_algetlistener3f GetListener = FuncLoader.LoadFunction<d_algetlistener3f>(AL.NativeLibrary, "alGetListener3f");

	internal static d_aldistancemodel DistanceModel = FuncLoader.LoadFunction<d_aldistancemodel>(AL.NativeLibrary, "alDistanceModel");

	internal static d_aldopplerfactor DopplerFactor = FuncLoader.LoadFunction<d_aldopplerfactor>(AL.NativeLibrary, "alDopplerFactor");

	internal static d_alsourcequeuebuffers alSourceQueueBuffers = FuncLoader.LoadFunction<d_alsourcequeuebuffers>(AL.NativeLibrary, "alSourceQueueBuffers");

	internal static d_alsourceunqueuebuffers alSourceUnqueueBuffers = FuncLoader.LoadFunction<d_alsourceunqueuebuffers>(AL.NativeLibrary, "alSourceUnqueueBuffers");

	internal static d_alsourceunqueuebuffers2 alSourceUnqueueBuffers2 = FuncLoader.LoadFunction<d_alsourceunqueuebuffers2>(AL.NativeLibrary, "alSourceUnqueueBuffers");

	internal static d_algetenumvalue alGetEnumValue = FuncLoader.LoadFunction<d_algetenumvalue>(AL.NativeLibrary, "alGetEnumValue");

	internal static d_alisextensionpresent IsExtensionPresent = FuncLoader.LoadFunction<d_alisextensionpresent>(AL.NativeLibrary, "alIsExtensionPresent");

	internal static d_algetprocaddress alGetProcAddress = FuncLoader.LoadFunction<d_algetprocaddress>(AL.NativeLibrary, "alGetProcAddress");

	private static d_algetstring alGetString = FuncLoader.LoadFunction<d_algetstring>(AL.NativeLibrary, "alGetString");

	private static IntPtr GetNativeLibrary()
	{
		if (CurrentPlatform.OS == OS.Windows)
		{
			return FuncLoader.LoadLibraryExt("soft_oal.dll");
		}
		if (CurrentPlatform.OS == OS.Linux)
		{
			return FuncLoader.LoadLibraryExt("libopenal.so.1");
		}
		if (CurrentPlatform.OS == OS.MacOSX)
		{
			return FuncLoader.LoadLibraryExt("libopenal.1.dylib");
		}
		return FuncLoader.LoadLibraryExt("openal");
	}

	internal static void BufferData(int bid, ALFormat format, byte[] data, int size, int freq)
	{
		GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
		AL.alBufferData((uint)bid, (int)format, handle.AddrOfPinnedObject(), size, freq);
		handle.Free();
	}

	internal static void BufferData(int bid, ALFormat format, short[] data, int size, int freq)
	{
		GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
		AL.alBufferData((uint)bid, (int)format, handle.AddrOfPinnedObject(), size, freq);
		handle.Free();
	}

	internal static void DeleteBuffers(int[] buffers)
	{
		AL.DeleteBuffers(buffers.Length, ref buffers[0]);
	}

	internal unsafe static void DeleteBuffers(int n, ref int buffers)
	{
		fixed (int* pbuffers = &buffers)
		{
			AL.alDeleteBuffers(n, pbuffers);
		}
	}

	internal static void GetBuffer(int bid, ALGetBufferi param, out int value)
	{
		AL.GetBufferi(bid, param, out value);
	}

	internal unsafe static void GenBuffers(int count, out int[] buffers)
	{
		buffers = new int[count];
		fixed (int* ptr = &buffers[0])
		{
			AL.alGenBuffers(count, ptr);
		}
	}

	internal static void GenBuffers(int count, out int buffer)
	{
		AL.GenBuffers(count, out int[] ret);
		buffer = ret[0];
	}

	internal static int[] GenBuffers(int count)
	{
		AL.GenBuffers(count, out int[] ret);
		return ret;
	}

	internal static void GenSources(int[] sources)
	{
		uint[] temp = new uint[sources.Length];
		AL.alGenSources(temp.Length, temp);
		for (int i = 0; i < temp.Length; i++)
		{
			sources[i] = (int)temp[i];
		}
	}

	internal static bool IsBuffer(int buffer)
	{
		return AL.alIsBuffer((uint)buffer);
	}

	internal static void SourcePause(int source)
	{
		AL.alSourcePause((uint)source);
	}

	internal static void SourcePlay(int source)
	{
		AL.alSourcePlay((uint)source);
	}

	internal static string GetErrorString(ALError errorCode)
	{
		return errorCode.ToString();
	}

	internal static void DeleteSource(int source)
	{
		AL.alDeleteSources(1, ref source);
	}

	internal static void Source(int sourceId, ALSourcei i, int a)
	{
		AL.alSourcei(sourceId, (int)i, a);
	}

	internal static void Source(int sourceId, ALSourceb i, bool a)
	{
		AL.alSourcei(sourceId, (int)i, a ? 1 : 0);
	}

	internal static void Source(int sourceId, ALSource3f i, float x, float y, float z)
	{
		AL.alSource3f(sourceId, i, x, y, z);
	}

	internal static void Source(int sourceId, ALSourcef i, float dist)
	{
		AL.alSourcef(sourceId, i, dist);
	}

	internal static ALSourceState GetSourceState(int sourceId)
	{
		AL.GetSource(sourceId, ALGetSourcei.SourceState, out var state);
		return (ALSourceState)state;
	}

	[CLSCompliant(false)]
	internal unsafe static void SourceQueueBuffers(int sourceId, int numEntries, int[] buffers)
	{
		fixed (int* ptr = &buffers[0])
		{
			AL.alSourceQueueBuffers(sourceId, numEntries, ptr);
		}
	}

	internal unsafe static void SourceQueueBuffer(int sourceId, int buffer)
	{
		AL.alSourceQueueBuffers(sourceId, 1, &buffer);
	}

	internal unsafe static int[] SourceUnqueueBuffers(int sourceId, int numEntries)
	{
		if (numEntries <= 0)
		{
			throw new ArgumentOutOfRangeException("numEntries", "Must be greater than zero.");
		}
		int[] array = new int[numEntries];
		fixed (int* ptr = &array[0])
		{
			AL.alSourceUnqueueBuffers(sourceId, numEntries, ptr);
		}
		return array;
	}

	internal static void SourceUnqueueBuffers(int sid, int numENtries, out int[] bids)
	{
		AL.alSourceUnqueueBuffers2(sid, numENtries, out bids);
	}

	internal static string GetString(int p)
	{
		return Marshal.PtrToStringAnsi(AL.alGetString(p));
	}

	internal static string Get(ALGetString p)
	{
		return AL.GetString((int)p);
	}
}
