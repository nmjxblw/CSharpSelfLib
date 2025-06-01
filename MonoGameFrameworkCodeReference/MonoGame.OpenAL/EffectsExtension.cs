using System;
using System.Runtime.InteropServices;

namespace MonoGame.OpenAL;

[CLSCompliant(false)]
internal class EffectsExtension
{
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private delegate void alGenEffectsDelegate(int n, out uint effect);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private delegate void alDeleteEffectsDelegate(int n, ref int effect);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private delegate void alEffectfDelegate(uint effect, EfxEffectf param, float value);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private delegate void alEffectiDelegate(uint effect, EfxEffecti param, int value);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private delegate void alGenAuxiliaryEffectSlotsDelegate(int n, out uint effectslots);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private delegate void alDeleteAuxiliaryEffectSlotsDelegate(int n, ref int effectslots);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private delegate void alAuxiliaryEffectSlotiDelegate(uint slot, EfxEffecti type, uint effect);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private delegate void alAuxiliaryEffectSlotfDelegate(uint slot, EfxEffectSlotf param, float value);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private unsafe delegate void alGenFiltersDelegate(int n, [Out] uint* filters);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private delegate void alFilteriDelegate(uint fid, EfxFilteri param, int value);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private delegate void alFilterfDelegate(uint fid, EfxFilterf param, float value);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private unsafe delegate void alDeleteFiltersDelegate(int n, [In] uint* filters);

	private alGenEffectsDelegate alGenEffects;

	private alDeleteEffectsDelegate alDeleteEffects;

	private alEffectfDelegate alEffectf;

	private alEffectiDelegate alEffecti;

	private alGenAuxiliaryEffectSlotsDelegate alGenAuxiliaryEffectSlots;

	private alDeleteAuxiliaryEffectSlotsDelegate alDeleteAuxiliaryEffectSlots;

	private alAuxiliaryEffectSlotiDelegate alAuxiliaryEffectSloti;

	private alAuxiliaryEffectSlotfDelegate alAuxiliaryEffectSlotf;

	private alGenFiltersDelegate alGenFilters;

	private alFilteriDelegate alFilteri;

	private alFilterfDelegate alFilterf;

	private alDeleteFiltersDelegate alDeleteFilters;

	internal static IntPtr device;

	private static EffectsExtension _instance;

	internal static EffectsExtension Instance
	{
		get
		{
			if (EffectsExtension._instance == null)
			{
				EffectsExtension._instance = new EffectsExtension();
			}
			return EffectsExtension._instance;
		}
	}

	internal bool IsInitialized { get; private set; }

	internal EffectsExtension()
	{
		this.IsInitialized = false;
		if (Alc.IsExtensionPresent(EffectsExtension.device, "ALC_EXT_EFX"))
		{
			this.alGenEffects = (alGenEffectsDelegate)Marshal.GetDelegateForFunctionPointer(AL.alGetProcAddress("alGenEffects"), typeof(alGenEffectsDelegate));
			this.alDeleteEffects = (alDeleteEffectsDelegate)Marshal.GetDelegateForFunctionPointer(AL.alGetProcAddress("alDeleteEffects"), typeof(alDeleteEffectsDelegate));
			this.alEffectf = (alEffectfDelegate)Marshal.GetDelegateForFunctionPointer(AL.alGetProcAddress("alEffectf"), typeof(alEffectfDelegate));
			this.alEffecti = (alEffectiDelegate)Marshal.GetDelegateForFunctionPointer(AL.alGetProcAddress("alEffecti"), typeof(alEffectiDelegate));
			this.alGenAuxiliaryEffectSlots = (alGenAuxiliaryEffectSlotsDelegate)Marshal.GetDelegateForFunctionPointer(AL.alGetProcAddress("alGenAuxiliaryEffectSlots"), typeof(alGenAuxiliaryEffectSlotsDelegate));
			this.alDeleteAuxiliaryEffectSlots = (alDeleteAuxiliaryEffectSlotsDelegate)Marshal.GetDelegateForFunctionPointer(AL.alGetProcAddress("alDeleteAuxiliaryEffectSlots"), typeof(alDeleteAuxiliaryEffectSlotsDelegate));
			this.alAuxiliaryEffectSloti = (alAuxiliaryEffectSlotiDelegate)Marshal.GetDelegateForFunctionPointer(AL.alGetProcAddress("alAuxiliaryEffectSloti"), typeof(alAuxiliaryEffectSlotiDelegate));
			this.alAuxiliaryEffectSlotf = (alAuxiliaryEffectSlotfDelegate)Marshal.GetDelegateForFunctionPointer(AL.alGetProcAddress("alAuxiliaryEffectSlotf"), typeof(alAuxiliaryEffectSlotfDelegate));
			this.alGenFilters = (alGenFiltersDelegate)Marshal.GetDelegateForFunctionPointer(AL.alGetProcAddress("alGenFilters"), typeof(alGenFiltersDelegate));
			this.alFilteri = (alFilteriDelegate)Marshal.GetDelegateForFunctionPointer(AL.alGetProcAddress("alFilteri"), typeof(alFilteriDelegate));
			this.alFilterf = (alFilterfDelegate)Marshal.GetDelegateForFunctionPointer(AL.alGetProcAddress("alFilterf"), typeof(alFilterfDelegate));
			this.alDeleteFilters = (alDeleteFiltersDelegate)Marshal.GetDelegateForFunctionPointer(AL.alGetProcAddress("alDeleteFilters"), typeof(alDeleteFiltersDelegate));
			this.IsInitialized = true;
		}
	}

	internal void GenAuxiliaryEffectSlots(int count, out uint slot)
	{
		this.alGenAuxiliaryEffectSlots(count, out slot);
	}

	internal void GenEffect(out uint effect)
	{
		this.alGenEffects(1, out effect);
	}

	internal void DeleteAuxiliaryEffectSlot(int slot)
	{
		this.alDeleteAuxiliaryEffectSlots(1, ref slot);
	}

	internal void DeleteEffect(int effect)
	{
		this.alDeleteEffects(1, ref effect);
	}

	internal void BindEffectToAuxiliarySlot(uint slot, uint effect)
	{
		this.alAuxiliaryEffectSloti(slot, EfxEffecti.SlotEffect, effect);
	}

	internal void AuxiliaryEffectSlot(uint slot, EfxEffectSlotf param, float value)
	{
		this.alAuxiliaryEffectSlotf(slot, param, value);
	}

	internal void BindSourceToAuxiliarySlot(int SourceId, int slot, int slotnumber, int filter)
	{
		AL.alSource3i(SourceId, ALSourcei.EfxAuxilarySendFilter, slot, slotnumber, filter);
	}

	internal void Effect(uint effect, EfxEffectf param, float value)
	{
		this.alEffectf(effect, param, value);
	}

	internal void Effect(uint effect, EfxEffecti param, int value)
	{
		this.alEffecti(effect, param, value);
	}

	internal unsafe int GenFilter()
	{
		uint filter = 0u;
		this.alGenFilters(1, &filter);
		return (int)filter;
	}

	internal void Filter(int sourceId, EfxFilteri filter, int EfxFilterType)
	{
		this.alFilteri((uint)sourceId, filter, EfxFilterType);
	}

	internal void Filter(int sourceId, EfxFilterf filter, float EfxFilterType)
	{
		this.alFilterf((uint)sourceId, filter, EfxFilterType);
	}

	internal void BindFilterToSource(int sourceId, int filterId)
	{
		AL.Source(sourceId, ALSourcei.EfxDirectFilter, filterId);
	}

	internal unsafe void DeleteFilter(int filterId)
	{
		this.alDeleteFilters(1, (uint*)(&filterId));
	}
}
