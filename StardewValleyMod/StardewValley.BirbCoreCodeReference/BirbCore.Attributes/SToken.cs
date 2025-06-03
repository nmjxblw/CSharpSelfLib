using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using BirbCore.APIs;
using StardewModdingAPI;

namespace BirbCore.Attributes;

public class SToken : ClassHandler
{
	public class FieldToken : FieldHandler
	{
		private object? _instance;

		private Func<object?, object?>? _getter;

		protected override void Handle(string name, Type fieldType, Func<object?, object?> getter, Action<object?, object?> setter, object? instance, IMod mod, object[]? args = null)
		{
			if (_api == null)
			{
				Log.Error("Content Patcher is not enabled, so will skip parsing");
				return;
			}
			_instance = instance;
			_getter = getter;
			_api.RegisterToken(mod.ModManifest, name, GetValue);
		}

		private IEnumerable<string>? GetValue()
		{
			object value = _getter?.Invoke(_instance);
			if (value != null)
			{
				if (value is IEnumerable items)
				{
					foreach (object item in items)
					{
						yield return (string)item;
					}
				}
				else
				{
					yield return value.ToString() ?? "";
				}
			}
			else
			{
				yield return "";
			}
		}
	}

	public class Token : MethodHandler
	{
		public override void Handle(MethodInfo method, object? instance, IMod mod, object[]? args = null)
		{
			if (_api == null)
			{
				Log.Error("Content Patcher is not enabled, so will skip parsing");
				return;
			}
			_api.RegisterToken(mod.ModManifest, method.Name, () => (IEnumerable<string>)method.Invoke(instance, Array.Empty<object>()));
		}
	}

	public class AdvancedToken : ClassHandler
	{
		public override void Handle(Type type, object? instance, IMod mod, object[]? args = null)
		{
			instance = Activator.CreateInstance(type);
			if (instance == null)
			{
				Log.Error("Content Patcher advanced api requires an instance of token class. Provided token class may be static?");
				return;
			}
			base.Handle(type, instance, mod);
			if (_api == null)
			{
				Log.Error("Content Patcher is not enabled, so will skip parsing");
			}
			else
			{
				_api.RegisterToken(mod.ModManifest, type.Name, instance);
			}
		}
	}

	private static IContentPatcherApi? _api;

	public SToken()
		: base(2)
	{
	}

	public override void Handle(Type type, object? instance, IMod mod, object[]? args = null)
	{
		if (Priority < 1)
		{
			Log.Error("Tokens cannot be loaded with priority < 1");
			return;
		}
		_api = mod.Helper.ModRegistry.GetApi<IContentPatcherApi>("Pathoschild.ContentPatcher");
		if (_api == null)
		{
			Log.Error("Content Patcher is not enabled, so will skip parsing");
		}
		else
		{
			base.Handle(type, instance, mod);
		}
	}
}
