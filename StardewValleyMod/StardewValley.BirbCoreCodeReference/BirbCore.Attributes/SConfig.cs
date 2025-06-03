using System;
using System.Reflection;
using BirbCore.APIs;
using BirbCore.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace BirbCore.Attributes;

public class SConfig(bool titleScreenOnly = false) : ClassHandler(1)
{
	public class Option : FieldHandler
	{
		private readonly string? _fieldId;

		private readonly float _min = float.MaxValue;

		private readonly float _max = float.MinValue;

		private readonly float _interval = float.MinValue;

		private readonly string[]? _allowedValues;

		public Option(string? fieldId = null)
		{
			_fieldId = fieldId;
		}

		public Option(int min, int max, int interval = 1, string? fieldId = null)
		{
			_fieldId = fieldId;
			_min = min;
			_max = max;
			_interval = interval;
		}

		public Option(float min, float max, float interval = 1f, string? fieldId = null)
		{
			_fieldId = fieldId;
			_min = min;
			_max = max;
			_interval = interval;
		}

		public Option(string[] allowedValues, string? fieldId = null)
		{
			_fieldId = fieldId;
			_allowedValues = allowedValues;
		}

		protected override void Handle(string name, Type fieldType, Func<object?, object?> getter, Action<object?, object?> setter, object? instance, IMod mod, object[]? args = null)
		{
			if (_api == null)
			{
				Log.Error("Attempting to use GMCM API before it is initialized");
				return;
			}
			if (fieldType == typeof(bool))
			{
				_api.AddBoolOption(mod.ModManifest, () => (bool)(getter(instance) ?? ((object)false)), delegate(bool value)
				{
					setter(instance, value);
				}, () => Translation.op_Implicit(mod.Helper.Translation.Get("config." + name).Default(name)), () => Translation.op_Implicit(mod.Helper.Translation.Get("config." + name + ".tooltip").UsePlaceholder(false)), _fieldId);
				return;
			}
			if (fieldType == typeof(int))
			{
				_api.AddNumberOption(mod.ModManifest, () => (int)(getter(instance) ?? ((object)0)), delegate(float value)
				{
					setter(instance, (int)value);
				}, () => Translation.op_Implicit(mod.Helper.Translation.Get("config." + name).Default(name)), () => Translation.op_Implicit(mod.Helper.Translation.Get("config." + name + ".tooltip").UsePlaceholder(false)), fieldId: _fieldId, min: (_min == float.MaxValue) ? ((float?)null) : new float?(_min), max: (_max == float.MinValue) ? ((float?)null) : new float?(_max), interval: (_interval == float.MinValue) ? ((float?)null) : new float?(_interval));
				return;
			}
			if (fieldType == typeof(float))
			{
				_api.AddNumberOption(mod.ModManifest, () => (float)(getter(instance) ?? ((object)0f)), delegate(float value)
				{
					setter(instance, value);
				}, () => Translation.op_Implicit(mod.Helper.Translation.Get("config." + name).Default(name)), () => Translation.op_Implicit(mod.Helper.Translation.Get("config." + name + ".tooltip").UsePlaceholder(false)), fieldId: _fieldId, min: (_min == float.MaxValue) ? ((float?)null) : new float?(_min), max: (_max == float.MinValue) ? ((float?)null) : new float?(_max), interval: (_interval == float.MinValue) ? ((float?)null) : new float?(_interval));
				return;
			}
			if (fieldType == typeof(string))
			{
				_api.AddTextOption(mod.ModManifest, () => (string)(getter(instance) ?? ""), delegate(string value)
				{
					setter(instance, value);
				}, () => Translation.op_Implicit(mod.Helper.Translation.Get("config." + name).Default(name)), () => Translation.op_Implicit(mod.Helper.Translation.Get("config." + name + ".tooltip").UsePlaceholder(false)), fieldId: _fieldId, allowedValues: _allowedValues);
				return;
			}
			if (fieldType == typeof(SButton))
			{
				_api.AddKeybind(mod.ModManifest, () => (SButton)(getter(instance) ?? ((object)(SButton)0)), delegate(SButton value)
				{
					//IL_000c: Unknown result type (might be due to invalid IL or missing references)
					setter(instance, value);
				}, () => Translation.op_Implicit(mod.Helper.Translation.Get("config." + name).Default(name)), () => Translation.op_Implicit(mod.Helper.Translation.Get("config." + name + ".tooltip").UsePlaceholder(false)), _fieldId);
				return;
			}
			if (fieldType == typeof(KeybindList))
			{
				_api.AddKeybindList(mod.ModManifest, () => (KeybindList)(getter(instance) ?? ((object)new KeybindList(Array.Empty<Keybind>()))), delegate(KeybindList value)
				{
					setter(instance, value);
				}, () => Translation.op_Implicit(mod.Helper.Translation.Get("config." + name).Default(name)), () => Translation.op_Implicit(mod.Helper.Translation.Get("config." + name + ".tooltip").UsePlaceholder(false)), _fieldId);
				return;
			}
			throw new Exception("Config had invalid property type " + name);
		}
	}

	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
	public class SectionTitle(string key) : FieldHandler()
	{
		protected override void Handle(string name, Type fieldType, Func<object?, object?> getter, Action<object?, object?> setter, object? instance, IMod mod, object[]? args = null)
		{
			if (_api == null)
			{
				Log.Error("Attempting to use GMCM API before it is initialized");
				return;
			}
			_api.AddSectionTitle(mod.ModManifest, () => Translation.op_Implicit(mod.Helper.Translation.Get("config." + key).Default(key)), () => Translation.op_Implicit(mod.Helper.Translation.Get("config." + key + ".tooltip").UsePlaceholder(false)));
		}
	}

	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
	public class Paragraph(string key) : FieldHandler()
	{
		protected override void Handle(string name, Type fieldType, Func<object?, object?> getter, Action<object?, object?> setter, object? instance, IMod mod, object[]? args = null)
		{
			if (_api == null)
			{
				Log.Error("Attempting to use GMCM API before it is initialized");
				return;
			}
			_api.AddParagraph(mod.ModManifest, () => Translation.op_Implicit(mod.Helper.Translation.Get("config." + key).Default(key)));
		}
	}

	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
	public class PageBlock(string pageId) : FieldHandler()
	{
		protected override void Handle(string name, Type fieldType, Func<object?, object?> getter, Action<object?, object?> setter, object? instance, IMod mod, object[]? args = null)
		{
			if (_api == null)
			{
				Log.Error("Attempting to use GMCM API before it is initialized");
				return;
			}
			_api.AddPage(mod.ModManifest, pageId, () => Translation.op_Implicit(mod.Helper.Translation.Get("config." + pageId).Default(pageId)));
		}
	}

	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
	public class PageLink(string pageId) : FieldHandler()
	{
		protected override void Handle(string name, Type fieldType, Func<object?, object?> getter, Action<object?, object?> setter, object? instance, IMod mod, object[]? args = null)
		{
			if (_api == null)
			{
				Log.Error("Attempting to use GMCM API before it is initialized");
				return;
			}
			_api.AddPageLink(mod.ModManifest, pageId, () => Translation.op_Implicit(mod.Helper.Translation.Get("config." + pageId).Default(pageId)), () => Translation.op_Implicit(mod.Helper.Translation.Get("config." + pageId + ".tooltip").UsePlaceholder(false)));
		}
	}

	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
	public class Image(string texture, int x = 0, int y = 0, int width = 0, int height = 0) : FieldHandler()
	{
		protected override void Handle(string name, Type fieldType, Func<object?, object?> getter, Action<object?, object?> setter, object? instance, IMod mod, object[]? args = null)
		{
			//IL_006e: Unknown result type (might be due to invalid IL or missing references)
			if (_api == null)
			{
				Log.Error("Attempting to use GMCM API before it is initialized");
				return;
			}
			_api.AddImage(mod.ModManifest, () => mod.Helper.GameContent.Load<Texture2D>(texture), (width != 0) ? new Rectangle?(new Rectangle(x, y, width, height)) : ((Rectangle?)null));
		}
	}

	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
	public class StartTitleOnlyBlock : FieldHandler
	{
		protected override void Handle(string name, Type fieldType, Func<object?, object?> getter, Action<object?, object?> setter, object? instance, IMod mod, object[]? args = null)
		{
			if (_api == null)
			{
				Log.Error("Attempting to use GMCM API before it is initialized");
			}
			else
			{
				_api.SetTitleScreenOnlyForNextOptions(mod.ModManifest, titleScreenOnly: true);
			}
		}
	}

	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
	public class EndTitleOnlyBlock : FieldHandler
	{
		protected override void Handle(string name, Type fieldType, Func<object?, object?> getter, Action<object?, object?> setter, object? instance, IMod mod, object[]? args = null)
		{
			if (_api == null)
			{
				Log.Error("Attempting to use GMCM API before it is initialized");
			}
			else
			{
				_api.SetTitleScreenOnlyForNextOptions(mod.ModManifest, titleScreenOnly: false);
			}
		}
	}

	private static IGenericModConfigMenuApi? _api;

	public override void Handle(Type type, object? instance, IMod mod, object[]? args = null)
	{
		if (Priority < 1)
		{
			Log.Error("Config cannot be loaded with priority < 1");
			return;
		}
		if (!((object)mod).GetType().TryGetMemberOfType(type, out MemberInfo configField))
		{
			Log.Error("Mod must define a Config property");
			return;
		}
		Func<object?, object?> getter = configField.GetGetter();
		Action<object, object> setter = configField.GetSetter();
		instance = ((object)mod.Helper).GetType().GetMethod("ReadConfig")?.MakeGenericMethod(type).Invoke(mod.Helper, Array.Empty<object>()) ?? instance;
		setter(mod, instance);
		_api = mod.Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
		if (_api == null)
		{
			Log.Info("Generic Mod Config Menu is not enabled, so will skip parsing");
			return;
		}
		_api.Register(mod.ModManifest, delegate
		{
			object obj = Activator.CreateInstance(type);
			object obj2 = getter(mod);
			PropertyInfo[] properties = type.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			foreach (PropertyInfo propertyInfo in properties)
			{
				propertyInfo.SetValue(obj2, propertyInfo.GetValue(obj));
			}
			FieldInfo[] fields = type.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
			foreach (FieldInfo fieldInfo in fields)
			{
				fieldInfo.SetValue(obj2, fieldInfo.GetValue(obj));
			}
		}, delegate
		{
			mod.Helper.WriteConfig<object>(getter(mod) ?? "");
		}, titleScreenOnly);
		base.Handle(type, instance, mod);
	}
}
