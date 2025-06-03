using System;
using System.Runtime.InteropServices;

namespace MonoGame.OpenAL;

internal class XRamExtension
{
	internal enum XRamStorage
	{
		Automatic,
		Hardware,
		Accessible
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private delegate bool SetBufferModeDelegate(int n, ref int buffers, int value);

	private int RamSize;

	private int RamFree;

	private int StorageAuto;

	private int StorageHardware;

	private int StorageAccessible;

	private SetBufferModeDelegate setBufferMode;

	internal bool IsInitialized { get; private set; }

	internal XRamExtension()
	{
		this.IsInitialized = false;
		if (!AL.IsExtensionPresent("EAX-RAM"))
		{
			return;
		}
		this.RamSize = AL.alGetEnumValue("AL_EAX_RAM_SIZE");
		this.RamFree = AL.alGetEnumValue("AL_EAX_RAM_FREE");
		this.StorageAuto = AL.alGetEnumValue("AL_STORAGE_AUTOMATIC");
		this.StorageHardware = AL.alGetEnumValue("AL_STORAGE_HARDWARE");
		this.StorageAccessible = AL.alGetEnumValue("AL_STORAGE_ACCESSIBLE");
		if (this.RamSize != 0 && this.RamFree != 0 && this.StorageAuto != 0 && this.StorageHardware != 0 && this.StorageAccessible != 0)
		{
			try
			{
				this.setBufferMode = (SetBufferModeDelegate)Marshal.GetDelegateForFunctionPointer(AL.alGetProcAddress("EAXSetBufferMode"), typeof(SetBufferModeDelegate));
			}
			catch (Exception)
			{
				return;
			}
			this.IsInitialized = true;
		}
	}

	internal bool SetBufferMode(int i, ref int id, XRamStorage storage)
	{
		return storage switch
		{
			XRamStorage.Accessible => this.setBufferMode(i, ref id, this.StorageAccessible), 
			XRamStorage.Hardware => this.setBufferMode(i, ref id, this.StorageHardware), 
			_ => this.setBufferMode(i, ref id, this.StorageAuto), 
		};
	}
}
