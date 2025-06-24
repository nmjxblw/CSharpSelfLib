using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using Netcode;
using StardewModdingAPI;
using StardewModdingAPI.Framework.Logging;
using StardewValley;
using StardewValley.Enchantments;
using StardewValley.GameData.Weapons;
using StardewValley.Tools;

namespace AdvancedMeleeFramework;

internal static class Utils
{
	private static ModEntry ctx;

	public const int CombatSkill = 4;

	internal static void Initialize(ModEntry context)
	{
		ctx = context;
	}

	public static void AddEnchantments(MeleeWeapon weapon)
	{
		AdvancedMeleeWeapon amw = GetAdvancedMeleeWeapon(weapon);
		if (amw == null || amw.enchantments.Count <= 0)
		{
			return;
		}
		((Tool)weapon).enchantments.Clear();
		foreach (AdvancedEnchantmentData enchtantData in amw.enchantments)
		{
			BaseWeaponEnchantment enchtant = GetEnchantmentFromDataType(enchtantData);
			if (enchtant == null)
			{
				((Mod)ctx).Monitor.Log("No suitable enchtantment found for type " + enchtantData.type, (LogLevel)4);
				((Mod)ctx).Monitor.Log("Currently registered advanced enchantments are: " + string.Join(", ", ctx.AdvancedEnchantmentCallbacks.Keys), (LogLevel)0);
			}
			else
			{
				((Tool)weapon).enchantments.Add((BaseEnchantment)(object)enchtant);
			}
		}
	}

	public static AdvancedMeleeWeapon? GetAdvancedMeleeWeapon(MeleeWeapon mw, Farmer? who = null)
	{
		AdvancedMeleeWeapon amw = null;
		int skillLevel = -1;
		if (ctx.AdvancedMeleeWeapons.ContainsKey(((Item)mw).ItemId))
		{
			foreach (AdvancedMeleeWeapon weapon in ctx.AdvancedMeleeWeapons[((Item)mw).ItemId])
			{
				if (who == null || (weapon.skillLevel <= who.getEffectiveSkillLevel(4) && weapon.skillLevel > skillLevel))
				{
					skillLevel = weapon.skillLevel;
					amw = weapon;
				}
			}
			if (amw != null)
			{
				return amw;
			}
		}
		if (ctx.AdvancedMeleeWeaponsByType.ContainsKey(((NetFieldBase<int, NetInt>)(object)mw.type).Value) && who != null)
		{
			skillLevel = -1;
			foreach (AdvancedMeleeWeapon weapon2 in ctx.AdvancedMeleeWeaponsByType[((NetFieldBase<int, NetInt>)(object)mw.type).Value])
			{
				if (weapon2.skillLevel <= who.getEffectiveSkillLevel(4) && weapon2.skillLevel > skillLevel)
				{
					skillLevel = weapon2.skillLevel;
					amw = weapon2;
				}
			}
		}
		return amw;
	}

	public static BaseWeaponEnchantment? GetEnchantmentFromDataType(AdvancedEnchantmentData data)
	{
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Expected O, but got Unknown
		//IL_01f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fe: Expected O, but got Unknown
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Expected O, but got Unknown
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Expected O, but got Unknown
		//IL_0222: Unknown result type (might be due to invalid IL or missing references)
		//IL_0228: Expected O, but got Unknown
		//IL_01d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d6: Expected O, but got Unknown
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Expected O, but got Unknown
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f6: Expected O, but got Unknown
		//IL_01e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e6: Expected O, but got Unknown
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Expected O, but got Unknown
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ee: Expected O, but got Unknown
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Expected O, but got Unknown
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Expected O, but got Unknown
		BaseWeaponEnchantment enchant = (BaseWeaponEnchantment)(data.type.ToLower() switch
		{
			"vampiric" => (object)new VampiricEnchantment(), 
			"haymaker" => (object)new HaymakerEnchantment(), 
			"bugkiller" => (object)new BugKillerEnchantment(), 
			"crusader" => (object)new CrusaderEnchantment(), 
			"artful" => (object)new ArtfulEnchantment(), 
			"magic" => (object)new MagicEnchantment(), 
			"jade" => (object)new JadeEnchantment(), 
			"aquamarine" => (object)new AquamarineEnchantment(), 
			"topaz" => (object)new TopazEnchantment(), 
			"amethyst" => (object)new AmethystEnchantment(), 
			"ruby" => (object)new RubyEnchantment(), 
			"emerald" => (object)new EmeraldEnchantment(), 
			_ => null, 
		});
		if (enchant != null)
		{
			return enchant;
		}
		if (!ctx.AdvancedEnchantmentCallbacks.ContainsKey(data.type))
		{
			return null;
		}
		enchant = new BaseWeaponEnchantment();
		((Mod)ctx).Helper.Reflection.GetField<string>((object)enchant, "_displayName", true).SetValue(data.name);
		return enchant;
	}

	public static void LoadAdvancedMeleeWeapons()
	{
		//IL_0469: Unknown result type (might be due to invalid IL or missing references)
		//IL_046e: Unknown result type (might be due to invalid IL or missing references)
		//IL_060e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0613: Unknown result type (might be due to invalid IL or missing references)
		//IL_065b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0660: Unknown result type (might be due to invalid IL or missing references)
		//IL_06ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_06b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0701: Unknown result type (might be due to invalid IL or missing references)
		//IL_0706: Unknown result type (might be due to invalid IL or missing references)
		ctx.AdvancedMeleeWeapons.Clear();
		ctx.AdvancedMeleeWeaponsByType[1].Clear();
		ctx.AdvancedMeleeWeaponsByType[2].Clear();
		ctx.AdvancedMeleeWeaponsByType[3].Clear();
		Dictionary<string, WeaponData> weaponData = ((Mod)ctx).Helper.GameContent.Load<Dictionary<string, WeaponData>>("Data\\Weapons");
		VerboseLogStringHandler val2;
		bool flag = default(bool);
		foreach (IContentPack contentPack in ((Mod)ctx).Helper.ContentPacks.GetOwned())
		{
			((Mod)ctx).Monitor.Log($"Reading content pack: {contentPack.Manifest.Name} {contentPack.Manifest.Version}", (LogLevel)2);
			((Mod)ctx).Monitor.Log($"[{contentPack.Manifest.Name}] build for version {contentPack.Manifest.ContentPackFor.MinimumVersion}", (LogLevel)0);
			try
			{
				List<AdvancedMeleeWeapon> weapons = ReadContentPack(contentPack);
				if (weapons == null || weapons.Count <= 0)
				{
					continue;
				}
				Dictionary<string, object> config = ReadContentPackConfig(contentPack) ?? new Dictionary<string, object>();
				Type AdvancedMeleeWeaponType = typeof(AdvancedMeleeWeapon);
				Type MeleeActionFrameTyep = typeof(MeleeActionFrame);
				Type AdvancedWeaponProjectileType = typeof(AdvancedWeaponProjectile);
				foreach (AdvancedMeleeWeapon weapon in weapons)
				{
					WriteConfigFor(weapon, weapon.config, config);
					foreach (MeleeActionFrame frame in weapon.frames)
					{
						WriteConfigFor(frame, frame.config, config);
						foreach (AdvancedWeaponProjectile projectile in frame.projectiles)
						{
							WriteConfigFor(projectile, projectile.config, config);
						}
						if (frame.special != null)
						{
							WriteConfigFor(frame.special, frame.special.config, config);
						}
					}
					foreach (AdvancedEnchantmentData enchantment in weapon.enchantments)
					{
						WriteConfigFor(enchantment, enchantment.config, config);
						ctx.AdvancedEnchantments[enchantment.name] = enchantment;
					}
					if (config.Count > 0)
					{
						contentPack.WriteJsonFile<Dictionary<string, object>>("config.json", config);
					}
					if (weapon.type < 1 || weapon.type > 3)
					{
						((Mod)ctx).Monitor.Log($"Found unrecognized weapon type ({weapon.type}), trying to read from id ({weapon.id})", (LogLevel)0);
						string id;
						try
						{
							id = weaponData.First((KeyValuePair<string, WeaponData> x) => x.Key == weapon.id).Key;
							((Mod)ctx).Monitor.Log("Found weapon by item id", (LogLevel)0);
						}
						catch
						{
							try
							{
								id = weaponData.First((KeyValuePair<string, WeaponData> x) => x.Value.Name == weapon.id).Key;
								((Mod)ctx).Monitor.Log("Found weapon by name", (LogLevel)0);
								goto end_IL_03c0;
							}
							catch
							{
								id = ctx.JsonAssetsApi?.GetWeaponId(weapon.id);
								if (string.IsNullOrWhiteSpace(id))
								{
									((Mod)ctx).Monitor.Log("Could not read weapon id " + weapon.id, (LogLevel)4);
									goto end_IL_03f9;
								}
								IMonitor monitor = ((Mod)ctx).Monitor;
								IMonitor val = monitor;
								val2 = new VerboseLogStringHandler(51, 2, monitor, ref flag);
								if (flag)
								{
									((VerboseLogStringHandler)(ref val2)).AppendLiteral("Found weapon in json assets (given id: ");
									((VerboseLogStringHandler)(ref val2)).AppendFormatted<string>(weapon.id);
									((VerboseLogStringHandler)(ref val2)).AppendLiteral(" -> JA id: ");
									((VerboseLogStringHandler)(ref val2)).AppendFormatted<string>(id);
									((VerboseLogStringHandler)(ref val2)).AppendLiteral(")");
								}
								val.VerboseLog(ref val2);
								goto end_IL_03c0;
								end_IL_03f9:;
							}
							continue;
							end_IL_03c0:;
						}
						if (!ctx.AdvancedMeleeWeapons.ContainsKey(id))
						{
							ctx.AdvancedMeleeWeapons.Add(id, new List<AdvancedMeleeWeapon>());
						}
						ctx.AdvancedMeleeWeapons[id].Add(weapon);
					}
					else
					{
						ctx.AdvancedMeleeWeaponsByType[weapon.type].Add(weapon);
					}
				}
			}
			catch (Exception ex)
			{
				((Mod)ctx).Monitor.Log("Could not read content.json file for content pack " + contentPack.Manifest.Name, (LogLevel)4);
				((Mod)ctx).Monitor.Log($"{ex.GetType().Name} - {ex.Message}\n{ex.StackTrace}", (LogLevel)0);
			}
		}
		IMonitor monitor2 = ((Mod)ctx).Monitor;
		IMonitor val3 = monitor2;
		val2 = new VerboseLogStringHandler(30, 1, monitor2, ref flag);
		if (flag)
		{
			((VerboseLogStringHandler)(ref val2)).AppendLiteral("Total advanced melee weapons: ");
			((VerboseLogStringHandler)(ref val2)).AppendFormatted<int>(ctx.AdvancedMeleeWeapons.Count);
		}
		val3.VerboseLog(ref val2);
		IMonitor monitor3 = ((Mod)ctx).Monitor;
		IMonitor val4 = monitor3;
		val2 = new VerboseLogStringHandler(37, 1, monitor3, ref flag);
		if (flag)
		{
			((VerboseLogStringHandler)(ref val2)).AppendLiteral("Total advanced melee dagger attacks: ");
			((VerboseLogStringHandler)(ref val2)).AppendFormatted<int>(ctx.AdvancedMeleeWeaponsByType[1].Count);
		}
		val4.VerboseLog(ref val2);
		IMonitor monitor4 = ((Mod)ctx).Monitor;
		IMonitor val5 = monitor4;
		val2 = new VerboseLogStringHandler(35, 1, monitor4, ref flag);
		if (flag)
		{
			((VerboseLogStringHandler)(ref val2)).AppendLiteral("Total advanced melee club attacks: ");
			((VerboseLogStringHandler)(ref val2)).AppendFormatted<int>(ctx.AdvancedMeleeWeaponsByType[2].Count);
		}
		val5.VerboseLog(ref val2);
		IMonitor monitor5 = ((Mod)ctx).Monitor;
		IMonitor val6 = monitor5;
		val2 = new VerboseLogStringHandler(36, 1, monitor5, ref flag);
		if (flag)
		{
			((VerboseLogStringHandler)(ref val2)).AppendLiteral("Total advanced melee sword attacks: ");
			((VerboseLogStringHandler)(ref val2)).AppendFormatted<int>(ctx.AdvancedMeleeWeaponsByType[3].Count);
		}
		val6.VerboseLog(ref val2);
	}

	private static List<AdvancedMeleeWeapon>? ReadContentPack(IContentPack contentPack)
	{
		try
		{
			return contentPack.ReadJsonFile<AdvancedMeleeWeaponData>("content.json").weapons;
		}
		catch
		{
			try
			{
				return contentPack.ReadJsonFile<List<AdvancedMeleeWeapon>>("content.json");
			}
			catch (Exception ex)
			{
				((Mod)ctx).Monitor.Log("Could not read content.json file for content pack", (LogLevel)4);
				((Mod)ctx).Monitor.Log($"{ex.GetType().Name} - {ex.Message}\n{ex.StackTrace}", (LogLevel)0);
				return null;
			}
		}
	}

	private static Dictionary<string, object>? ReadContentPackConfig(IContentPack contentPack)
	{
		try
		{
			return contentPack.ReadJsonFile<Dictionary<string, object>>("config.json");
		}
		catch
		{
			try
			{
				WeaponPackConfigData legacyConfig = contentPack.ReadJsonFile<WeaponPackConfigData>("config.json");
				if (legacyConfig == null)
				{
					((Mod)ctx).Monitor.Log("Could not read config.json file for content pack", (LogLevel)4);
					return null;
				}
				contentPack.WriteJsonFile<Dictionary<string, object>>("config.json", legacyConfig.variables);
				((Mod)ctx).Monitor.Log("Found legacy config format for content pack, the config has automatically migrated to the new format", (LogLevel)3);
				return legacyConfig.variables;
			}
			catch (Exception ex)
			{
				((Mod)ctx).Monitor.Log("Could not read config.json file for content pack", (LogLevel)4);
				((Mod)ctx).Monitor.Log($"{ex.GetType().Name} - {ex.Message}\n{ex.StackTrace}", (LogLevel)0);
				return null;
			}
		}
	}

	private static void WriteConfigFor<T>(T value, Dictionary<string, string> configuredFields, Dictionary<string, object> config)
	{
		try
		{
			Type type = typeof(T);
			foreach (KeyValuePair<string, string> field in configuredFields)
			{
				FieldInfo fi = type.GetField(field.Key);
				if ((object)fi == null)
				{
					FieldInfo paramF = type.GetField("parameters");
					if ((object)paramF != null && paramF.GetValue(value) is Dictionary<string, string> param)
					{
						WriteConfigForParameter(param, field.Key, type, config);
					}
					else
					{
						((Mod)ctx).Monitor.Log("Field " + field.Key + " could not be found for type " + type.Name, (LogLevel)4);
					}
				}
				else if (config.ContainsKey(field.Key))
				{
					if (config[field.Key] is long)
					{
						fi.SetValue(value, Convert.ToInt32(config[field.Key].ToString()));
					}
					else
					{
						fi.SetValue(value, config[field.Key]);
					}
				}
				else
				{
					config.Add(field.Key, fi.GetValue(value));
				}
			}
		}
		catch (Exception ex)
		{
			((Mod)ctx).Monitor.Log("An error occured when assigning configurable fields", (LogLevel)4);
			((Mod)ctx).Monitor.Log($"{ex.GetType().Name} - {ex.Message}\n{ex.StackTrace}", (LogLevel)0);
		}
	}

	private static void WriteConfigForParameter(Dictionary<string, string> parameters, string key, Type type, Dictionary<string, object> config)
	{
		try
		{
			if (!parameters.ContainsKey(key))
			{
				((Mod)ctx).Monitor.Log("Field " + key + " could not be found for parameters of type " + type.Name, (LogLevel)4);
			}
			else if (config.ContainsKey(key))
			{
				if (config[key] is long)
				{
					parameters[key] = Convert.ToInt32(config[key].ToString()).ToString();
				}
				else
				{
					parameters[key] = config[key].ToString();
				}
			}
			else
			{
				config.Add(key, parameters[key]);
			}
		}
		catch (Exception ex)
		{
			((Mod)ctx).Monitor.Log("An error occured when assigning configurable fields for parameters", (LogLevel)4);
			((Mod)ctx).Monitor.Log($"{ex.GetType().Name} - {ex.Message}\n{ex.StackTrace}", (LogLevel)0);
		}
	}

	public static Vector2 TranslateVector(Vector2 vector, int facingDirection)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		switch (facingDirection)
		{
		case 0:
			return new Vector2(0f - vector.X, 0f - vector.Y);
		case 1:
			return new Vector2(vector.Y, 0f - vector.X);
		case 2:
			return vector;
		case 3:
			return new Vector2(0f - vector.Y, vector.X);
		default:
		{
			global::_003CPrivateImplementationDetails_003E.ThrowSwitchExpressionException(facingDirection);
			Vector2 result = default(Vector2);
			return result;
		}
		}
	}
}
