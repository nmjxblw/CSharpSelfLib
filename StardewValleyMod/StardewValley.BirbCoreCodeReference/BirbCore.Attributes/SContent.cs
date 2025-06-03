using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json.Serialization;
using BirbCore.Extensions;
using HarmonyLib;
using StardewModdingAPI;

namespace BirbCore.Attributes;

public class SContent(string fileName = "content.json", bool isList = false, bool isDictionary = false) : ClassHandler()
{
	public class ModId : FieldHandler
	{
		protected override void Handle(string name, Type fieldType, Func<object, object?> getter, Action<object, object> setter, object? instance, IMod mod, object[]? args = null)
		{
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			if (instance == null)
			{
				Log.Error("Content instance might be static? Failing to add all content packs");
			}
			else if (args == null || args[0] == null)
			{
				Log.Error("Something went wrong in BirbCore Content Pack parsing");
			}
			else
			{
				setter(instance, ((IContentPack)args[0]).Manifest.UniqueID);
			}
		}
	}

	public class UniqueId : FieldHandler
	{
		protected override void Handle(string name, Type fieldType, Func<object, object?> getter, Action<object, object> setter, object? instance, IMod mod, object[]? args = null)
		{
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			if (instance == null)
			{
				Log.Error("Content instance might be static? Failing to add all content packs");
				return;
			}
			if (args == null || args[0] == null)
			{
				Log.Error("Something went wrong in BirbCore Content Pack parsing");
				return;
			}
			setter(instance, $"{((IContentPack)args[0]).Manifest.UniqueID}_{args[1]}");
		}
	}

	public class ContentId : FieldHandler
	{
		protected override void Handle(string name, Type fieldType, Func<object, object?> getter, Action<object, object> setter, object? instance, IMod mod, object[]? args = null)
		{
			if (instance == null)
			{
				Log.Error("Content instance might be static? Failing to add all content packs");
			}
			else if (args == null || args[1] == null)
			{
				Log.Error("Something went wrong in BirbCore Content Pack parsing");
			}
			else
			{
				setter(instance, args[1]);
			}
		}
	}

	public class ContentPack : FieldHandler
	{
		protected override void Handle(string name, Type fieldType, Func<object, object?> getter, Action<object, object> setter, object? instance, IMod mod, object[]? args = null)
		{
			if (instance == null)
			{
				Log.Error("Content instance might be static? Failing to add all content packs");
			}
			else if (args == null || args[0] == null)
			{
				Log.Error("Something went wrong in BirbCore Content Pack parsing");
			}
			else if (fieldType != typeof(IContentPack))
			{
				Log.Error("ContentPack attribute can only set value to field or property of type IContentPack");
			}
			else
			{
				setter(instance, args[0]);
			}
		}
	}

	public override void Handle(Type type, object? instance, IMod mod, object[]? args = null)
	{
		Type modEntryValueType = type;
		if (isList)
		{
			modEntryValueType = typeof(List<>).MakeGenericType(type);
		}
		else if (isDictionary)
		{
			modEntryValueType = typeof(Dictionary<, >).MakeGenericType(typeof(string), type);
		}
		Type modEntryType = typeof(Dictionary<, >).MakeGenericType(typeof(string), modEntryValueType);
		if (!((object)mod).GetType().TryGetMemberOfType(modEntryType, out MemberInfo modContent))
		{
			Log.Error("Mod must define a Content dictionary property");
			return;
		}
		IDictionary contentDictionary = (IDictionary)AccessTools.CreateInstance(modEntryType);
		if (contentDictionary == null)
		{
			Log.Error("contentDictionary was null.  Underlying type might be static? Cannot initialize.");
			return;
		}
		modContent.GetSetter()(mod, contentDictionary);
		if (type.TryGetMemberWithCustomAttribute(typeof(ContentId), out MemberInfo idMember) && idMember.GetCustomAttribute<JsonIgnoreAttribute>() != null)
		{
			idMember = null;
		}
		foreach (IContentPack contentPack in mod.Helper.ContentPacks.GetOwned())
		{
			object content = ((object)contentPack).GetType().GetMethod("ReadJsonFile")?.MakeGenericMethod(modEntryValueType).Invoke(contentPack, new object[1] { fileName });
			if (content == null)
			{
				Log.Error(fileName + " in content pack " + contentPack.Manifest.UniqueID + " was null");
				continue;
			}
			if (isList)
			{
				IList contentList = (IList)content;
				for (int i = 0; i < contentList.Count; i++)
				{
					string id = (string)(idMember?.GetGetter()(contentList[i]) ?? ((object)i));
					base.Handle(type, contentList[i], mod, new object[2] { contentPack, id });
				}
			}
			else if (isDictionary)
			{
				foreach (DictionaryEntry entry in (IDictionary)content)
				{
					string key = (string)entry.Key;
					object value = entry.Value;
					string id2 = (string)(idMember?.GetGetter()(value) ?? key);
					base.Handle(type, value, mod, new object[2] { contentPack, id2 });
				}
			}
			else
			{
				string id3 = (string)(idMember?.GetGetter()(content) ?? "");
				base.Handle(type, content, mod, new object[2] { contentPack, id3 });
			}
			string modId = contentPack.Manifest.UniqueID;
			contentDictionary.Add(modId, content);
		}
	}
}
