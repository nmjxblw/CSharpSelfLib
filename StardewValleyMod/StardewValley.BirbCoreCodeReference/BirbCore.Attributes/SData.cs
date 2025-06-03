using System;
using System.Reflection;
using BirbCore.Extensions;
using HarmonyLib;
using StardewModdingAPI;

namespace BirbCore.Attributes;

public class SData : ClassHandler
{
	public class SaveData(string key) : FieldHandler()
	{
		protected override void Handle(string name, Type fieldType, Func<object?, object?> getter, Action<object?, object?> setter, object? instance, IMod mod, object[]? args = null)
		{
			mod.Helper.Events.GameLoop.SaveLoaded += delegate
			{
				object arg = ((object)mod.Helper.Data).GetType().GetMethod("ReadSaveData")?.MakeGenericMethod(fieldType).Invoke(mod.Helper.Data, new object[1] { key });
				setter(instance, arg);
			};
			mod.Helper.Events.GameLoop.SaveCreated += delegate
			{
				object arg = AccessTools.CreateInstance(fieldType);
				setter(instance, arg);
			};
			mod.Helper.Events.GameLoop.DayEnding += delegate
			{
				object obj = getter(instance);
				mod.Helper.Data.WriteSaveData<object>(key, obj);
			};
		}
	}

	public class LocalData(string jsonFile) : FieldHandler()
	{
		protected override void Handle(string name, Type fieldType, Func<object?, object?> getter, Action<object?, object?> setter, object? instance, IMod mod, object[]? args = null)
		{
			object localData = ((object)mod.Helper.Data).GetType().GetMethod("ReadJsonFile")?.MakeGenericMethod(fieldType).Invoke(mod.Helper.Data, new object[1] { jsonFile });
			if (localData == null)
			{
				localData = AccessTools.CreateInstance(fieldType);
			}
			setter(instance, localData);
			mod.Helper.Events.GameLoop.DayEnding += delegate
			{
				object obj = getter(instance);
				mod.Helper.Data.WriteJsonFile<object>(jsonFile, obj);
			};
		}
	}

	public class GlobalData(string key) : FieldHandler()
	{
		protected override void Handle(string name, Type fieldType, Func<object?, object?> getter, Action<object?, object?> setter, object? instance, IMod mod, object[]? args = null)
		{
			object globalData = ((object)mod.Helper.Data).GetType().GetMethod("ReadGlobalData")?.MakeGenericMethod(fieldType).Invoke(mod.Helper.Data, new object[1] { key });
			if (globalData == null)
			{
				globalData = AccessTools.CreateInstance(fieldType);
			}
			setter(instance, globalData);
			mod.Helper.Events.GameLoop.DayEnding += delegate
			{
				object obj = getter(instance);
				mod.Helper.Data.WriteGlobalData<object>(key, obj);
			};
		}
	}

	public override void Handle(Type type, object? instance, IMod mod, object[]? args = null)
	{
		if (!((object)mod).GetType().TryGetMemberOfType(type, out MemberInfo modData))
		{
			Log.Error("Mod must define a data property");
			return;
		}
		modData.GetSetter()(mod, instance);
		base.Handle(type, instance, mod);
	}
}
