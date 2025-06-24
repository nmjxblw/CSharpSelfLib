using System;
using System.Collections.Generic;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Monsters;
using StardewValley.Tools;

namespace AdvancedMeleeFramework;

public class AdvancedMeleeFrameworkApi : IAdvancedMeleeFrameworkApi
{
	private ModEntry ctx;

	private IModInfo mod;

	public AdvancedMeleeFrameworkApi(IModInfo accessor, ModEntry context)
	{
		ctx = context;
		mod = accessor;
	}

	public bool RegisterEnchantment(string typeId, Action<Farmer, MeleeWeapon, Monster?, Dictionary<string, string>> callback)
	{
		if (ctx.AdvancedEnchantmentCallbacks.ContainsKey(typeId))
		{
			((Mod)ctx).Monitor.Log(mod.Manifest.Name + " tried to add enchantment " + typeId + " but it already exists", (LogLevel)0);
			return false;
		}
		ctx.AdvancedEnchantmentCallbacks.Add(typeId, callback);
		((Mod)ctx).Monitor.Log(mod.Manifest.Name + " added enchantment " + typeId, (LogLevel)0);
		return true;
	}

	public bool RegisterSpecialEffect(string name, Action<Farmer, MeleeWeapon, Dictionary<string, string>> callback)
	{
		if (ctx.SpecialEffectCallbacks.ContainsKey(name))
		{
			((Mod)ctx).Monitor.Log(mod.Manifest.Name + " tried to add special effect " + name + " but it already exists", (LogLevel)0);
			return false;
		}
		ctx.SpecialEffectCallbacks.Add(name, callback);
		((Mod)ctx).Monitor.Log(mod.Manifest.Name + " added special effect " + name, (LogLevel)0);
		return true;
	}

	public bool UnRegisterEnchantment(string typeId)
	{
		if (ctx.AdvancedEnchantmentCallbacks.Remove(typeId))
		{
			((Mod)ctx).Monitor.Log(mod.Manifest.Name + " removed enchantment " + typeId, (LogLevel)0);
			return true;
		}
		((Mod)ctx).Monitor.Log(mod.Manifest.Name + " tried to remove enchantment " + typeId + " but it didn't exist", (LogLevel)0);
		return false;
	}

	public bool UnRegisterSpecialEffect(string name)
	{
		if (ctx.SpecialEffectCallbacks.Remove(name))
		{
			((Mod)ctx).Monitor.Log(mod.Manifest.Name + " removed special effect " + name, (LogLevel)0);
			return true;
		}
		((Mod)ctx).Monitor.Log(mod.Manifest.Name + " tried to remove special effect " + name + " but it didn't exist", (LogLevel)0);
		return false;
	}
}
