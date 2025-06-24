using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewModdingAPI;
using StardewModdingAPI.Framework.Logging;
using StardewValley;
using StardewValley.Enchantments;
using StardewValley.Monsters;
using StardewValley.Network;
using StardewValley.Tools;

namespace AdvancedMeleeFramework;

internal static class AMFPatches
{
	private static ModEntry ctx;

	public static void Initialize(ModEntry context)
	{
		ctx = context;
		Harmony harmony = new Harmony(((Mod)ctx).ModManifest.UniqueID);
		harmony.Patch(AccessTools.Constructor(typeof(MeleeWeapon), new Type[1] { typeof(string) }), null, new HarmonyMethod(typeof(AMFPatches), "MeleeWeapon_Postfix"));
		harmony.Patch(AccessTools.Method(typeof(MeleeWeapon), "doAnimateSpecialMove"), new HarmonyMethod(typeof(AMFPatches), "MeleeWeapon_DoAnimateSpecialMove_Prefix"));
		harmony.Patch(AccessTools.Constructor(typeof(MeleeWeapon), Array.Empty<Type>()), null, new HarmonyMethod(typeof(AMFPatches), "MeleeWeapon_Postfix"));
		harmony.Patch(AccessTools.Method(typeof(MeleeWeapon), "drawInMenu", new Type[8]
		{
			typeof(SpriteBatch),
			typeof(Vector2),
			typeof(float),
			typeof(float),
			typeof(float),
			typeof(StackDrawType),
			typeof(Color),
			typeof(bool)
		}), new HarmonyMethod(typeof(AMFPatches), "MeleeWeapon_DrawInMenu_Prefix"), new HarmonyMethod(typeof(AMFPatches), "MeleeWeapon_DrawInMenu_Postfix"));
		harmony.Patch(AccessTools.Method(typeof(BaseEnchantment), "OnDealtDamage"), new HarmonyMethod(typeof(AMFPatches), "BaseEnchantment_OnDealtDamage_Prefix"));
		harmony.Patch(AccessTools.Method(typeof(BaseEnchantment), "OnMonsterSlay"), new HarmonyMethod(typeof(AMFPatches), "BaseEnchantment_OnMonsterSlay_Prefix"));
		harmony.Patch(AccessTools.Method(typeof(Farmer), "OnItemReceived"), new HarmonyMethod(typeof(AMFPatches), "Farmer_OnItemReceived_Prefix"));
	}

	private static void MeleeWeapon_Postfix(MeleeWeapon __instance)
	{
		Utils.AddEnchantments(__instance);
	}

	private static bool MeleeWeapon_DoAnimateSpecialMove_Prefix(MeleeWeapon __instance, Farmer ___lastUser)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		IMonitor monitor = ((Mod)ctx).Monitor;
		IMonitor val = monitor;
		bool flag = default(bool);
		VerboseLogStringHandler val2 = new VerboseLogStringHandler(22, 2, monitor, ref flag);
		if (flag)
		{
			((VerboseLogStringHandler)(ref val2)).AppendLiteral("Special move for ");
			((VerboseLogStringHandler)(ref val2)).AppendFormatted<string>(((Item)__instance).Name);
			((VerboseLogStringHandler)(ref val2)).AppendLiteral(", id ");
			((VerboseLogStringHandler)(ref val2)).AppendFormatted<string>(((Item)__instance).ItemId);
		}
		val.VerboseLog(ref val2);
		if (ctx.Config.RequireModKey && !((Mod)ctx).Helper.Input.IsDown(ctx.Config.ModKey))
		{
			return true;
		}
		ctx.AdvancedWeaponAnimating = Utils.GetAdvancedMeleeWeapon(__instance, ___lastUser);
		if (ctx.WeaponAnimationFrame > -1 || ctx.AdvancedWeaponAnimating == null || !ctx.AdvancedWeaponAnimating.frames.Any())
		{
			return true;
		}
		if (___lastUser == null || (object)___lastUser.CurrentTool != __instance)
		{
			return false;
		}
		IMonitor monitor2 = ((Mod)ctx).Monitor;
		IMonitor val3 = monitor2;
		val2 = new VerboseLogStringHandler(10, 1, monitor2, ref flag);
		if (flag)
		{
			((VerboseLogStringHandler)(ref val2)).AppendLiteral("Animating ");
			((VerboseLogStringHandler)(ref val2)).AppendFormatted<string>(((Item)__instance).Name);
		}
		val3.VerboseLog(ref val2);
		if (___lastUser.isEmoteAnimating)
		{
			___lastUser.EndEmoteAnimation();
		}
		ctx.WeaponStartFacingDirection = ((Character)___lastUser).FacingDirection;
		ctx.WeaponAnimationFrame = 0;
		ctx.WeaponAnimating = __instance;
		return false;
	}

	private static void MeleeWeapon_DrawInMenu_Prefix(MeleeWeapon __instance, ref int __state)
	{
		__state = 0;
		switch (((NetFieldBase<int, NetInt>)(object)__instance.type).Value)
		{
		case 0:
		case 3:
			if (MeleeWeapon.defenseCooldown > 1500)
			{
				__state = MeleeWeapon.defenseCooldown;
				MeleeWeapon.defenseCooldown = 1500;
			}
			break;
		case 1:
			if (MeleeWeapon.daggerCooldown > 3000)
			{
				__state = MeleeWeapon.daggerCooldown;
				MeleeWeapon.daggerCooldown = 3000;
			}
			break;
		case 2:
			if (MeleeWeapon.clubCooldown > 6000)
			{
				__state = MeleeWeapon.clubCooldown;
				MeleeWeapon.clubCooldown = 6000;
			}
			break;
		}
	}

	private static void MeleeWeapon_DrawInMenu_Postfix(MeleeWeapon __instance, int __state)
	{
		if (__state != 0)
		{
			switch (((NetFieldBase<int, NetInt>)(object)__instance.type).Value)
			{
			case 0:
			case 3:
				MeleeWeapon.defenseCooldown = __state;
				break;
			case 1:
				MeleeWeapon.daggerCooldown = __state;
				break;
			case 2:
				MeleeWeapon.clubCooldown = __state;
				break;
			}
		}
	}

	private static bool BaseEnchantment_OnDealtDamage_Prefix(BaseEnchantment __instance, string ____displayName, Monster monster, GameLocation location, Farmer who, int amount)
	{
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		if (__instance is BaseWeaponEnchantment && who != null)
		{
			Tool currentTool = who.CurrentTool;
			MeleeWeapon mw = (MeleeWeapon)(object)((currentTool is MeleeWeapon) ? currentTool : null);
			if (mw != null && !string.IsNullOrEmpty(____displayName) && ctx.AdvancedEnchantments.TryGetValue(____displayName, out var enchantment) && (!ctx.EnchantmentTriggers.TryGetValue(who.UniqueMultiplayerID + ____displayName, out var triggerTicks) || triggerTicks != Game1.ticks))
			{
				Dictionary<string, string> parameters = enchantment.parameters;
				if (parameters == null || !parameters.TryGetValue("trigger", out var trigger))
				{
					return true;
				}
				if (trigger == "damage" || (trigger == "crit" && amount > ((NetFieldBase<int, NetInt>)(object)mw.maxDamage).Value && !Environment.StackTrace.Contains("OnCalculateDamage")))
				{
					IMonitor monitor = ((Mod)ctx).Monitor;
					IMonitor val = monitor;
					bool flag = default(bool);
					VerboseLogStringHandler val2 = new VerboseLogStringHandler(62, 5, monitor, ref flag);
					if (flag)
					{
						((VerboseLogStringHandler)(ref val2)).AppendLiteral("Triggered enchantment ");
						((VerboseLogStringHandler)(ref val2)).AppendFormatted<string>(enchantment.name);
						((VerboseLogStringHandler)(ref val2)).AppendLiteral(" on ");
						((VerboseLogStringHandler)(ref val2)).AppendFormatted<string>(trigger);
						((VerboseLogStringHandler)(ref val2)).AppendLiteral(". ");
						((VerboseLogStringHandler)(ref val2)).AppendFormatted<string>(((Item)mw).Name);
						((VerboseLogStringHandler)(ref val2)).AppendLiteral(" did ");
						((VerboseLogStringHandler)(ref val2)).AppendFormatted<int>(amount);
						((VerboseLogStringHandler)(ref val2)).AppendLiteral(" damage and has ");
						((VerboseLogStringHandler)(ref val2)).AppendFormatted<int>(((Tool)mw).enchantments.Count);
						((VerboseLogStringHandler)(ref val2)).AppendLiteral(" enchantments");
					}
					val.VerboseLog(ref val2);
					ctx.EnchantmentTriggers[who.UniqueMultiplayerID + ____displayName] = Game1.ticks;
					if (!ctx.AdvancedEnchantmentCallbacks.TryGetValue(enchantment.type, out Action<Farmer, MeleeWeapon, Monster, Dictionary<string, string>> callback))
					{
						((Mod)ctx).Monitor.Log("Triggered enchantment " + enchantment.type + " could not be found", (LogLevel)4);
						return false;
					}
					Dictionary<string, string> parameters2 = new Dictionary<string, string>(enchantment.parameters) { 
					{
						"amount",
						amount.ToString()
					} };
					callback?.Invoke(who, mw, monster, parameters2);
				}
				return false;
			}
		}
		return true;
	}

	private static bool BaseEnchantment_OnMonsterSlay_Prefix(BaseEnchantment __instance, string ____displayName, Monster monster, GameLocation location, Farmer who)
	{
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		if (__instance is BaseWeaponEnchantment && who != null)
		{
			Tool currentTool = who.CurrentTool;
			MeleeWeapon mw = (MeleeWeapon)(object)((currentTool is MeleeWeapon) ? currentTool : null);
			if (mw != null && !string.IsNullOrEmpty(____displayName) && ctx.AdvancedEnchantments.TryGetValue(____displayName, out var enchantment) && (!ctx.EnchantmentTriggers.TryGetValue(who.UniqueMultiplayerID + ____displayName, out var triggerTicks) || triggerTicks != Game1.ticks))
			{
				Dictionary<string, string> parameters = enchantment.parameters;
				if (parameters == null || !parameters.TryGetValue("trigger", out var trigger))
				{
					return true;
				}
				if (trigger == "slay")
				{
					IMonitor monitor = ((Mod)ctx).Monitor;
					IMonitor val = monitor;
					bool flag = default(bool);
					VerboseLogStringHandler val2 = new VerboseLogStringHandler(66, 5, monitor, ref flag);
					if (flag)
					{
						((VerboseLogStringHandler)(ref val2)).AppendLiteral("Triggered enchantment ");
						((VerboseLogStringHandler)(ref val2)).AppendFormatted<string>(enchantment.name);
						((VerboseLogStringHandler)(ref val2)).AppendLiteral(" on ");
						((VerboseLogStringHandler)(ref val2)).AppendFormatted<string>(trigger);
						((VerboseLogStringHandler)(ref val2)).AppendLiteral(". Slayed ");
						((VerboseLogStringHandler)(ref val2)).AppendFormatted<string>(((Character)monster).Name);
						((VerboseLogStringHandler)(ref val2)).AppendLiteral(" with ");
						((VerboseLogStringHandler)(ref val2)).AppendFormatted<string>(((Item)mw).Name);
						((VerboseLogStringHandler)(ref val2)).AppendLiteral(", which has ");
						((VerboseLogStringHandler)(ref val2)).AppendFormatted<int>(((Tool)mw).enchantments.Count);
						((VerboseLogStringHandler)(ref val2)).AppendLiteral(" enchantments");
					}
					val.VerboseLog(ref val2);
					ctx.EnchantmentTriggers[who.UniqueMultiplayerID + ____displayName] = Game1.ticks;
					if (!ctx.AdvancedEnchantmentCallbacks.TryGetValue(enchantment.type, out Action<Farmer, MeleeWeapon, Monster, Dictionary<string, string>> callback))
					{
						((Mod)ctx).Monitor.Log("Triggered enchantment " + enchantment.type + " could not be found", (LogLevel)4);
						return false;
					}
					Dictionary<string, string> parameters2 = new Dictionary<string, string>(enchantment.parameters) { 
					{
						"amount",
						monster.MaxHealth.ToString()
					} };
					callback?.Invoke(who, mw, monster, parameters2);
				}
				return false;
			}
		}
		return true;
	}

	private static bool Farmer_OnItemReceived_Prefix(Farmer __instance, Item item)
	{
		string amountStr = default(string);
		if (!__instance.IsLocalPlayer || ((item != null) ? item.QualifiedItemId : null) != "(O)GoldCoin" || !((NetDictionary<string, string, NetString, SerializableDictionary<string, string>, NetStringDictionary<string, NetString>>)(object)item.modData).TryGetValue(((Mod)ctx).ModManifest.UniqueID + "/moneyAmount", ref amountStr))
		{
			return true;
		}
		Game1.playSound("moneyDial", (int?)null);
		int amount = int.Parse(amountStr);
		__instance.Money += amount;
		__instance.removeItemFromInventory(item);
		Game1.dayTimeMoneyBox.gotGoldCoin(amount);
		return false;
	}
}
