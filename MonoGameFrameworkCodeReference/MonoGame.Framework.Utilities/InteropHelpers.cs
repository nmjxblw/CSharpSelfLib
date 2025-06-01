using System;
using System.Runtime.InteropServices;
using System.Text;

namespace MonoGame.Framework.Utilities;

internal static class InteropHelpers
{
	/// <summary>
	/// Convert a pointer to a Utf8 null-terminated string to a .NET System.String
	/// </summary>
	public unsafe static string Utf8ToString(IntPtr handle)
	{
		if (handle == IntPtr.Zero)
		{
			return string.Empty;
		}
		byte* ptr;
		for (ptr = (byte*)(void*)handle; *ptr != 0; ptr++)
		{
		}
		long len = ptr - (byte*)(void*)handle;
		if (len == 0L)
		{
			return string.Empty;
		}
		byte[] bytes = new byte[len];
		Marshal.Copy(handle, bytes, 0, bytes.Length);
		return Encoding.UTF8.GetString(bytes);
	}
}
