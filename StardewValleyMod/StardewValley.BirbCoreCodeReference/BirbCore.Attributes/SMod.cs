using System;
using StardewModdingAPI;

namespace BirbCore.Attributes;

public class SMod : ClassHandler
{
	public class Api(string uniqueId, bool isRequired = true) : FieldHandler()
	{
		public bool IsRequired = isRequired;

		protected override void Handle(string name, Type fieldType, Func<object?, object?> getter, Action<object?, object?> setter, object? instance, IMod mod, object[]? args = null)
		{
			object api = ((object)mod.Helper.ModRegistry).GetType().GetMethod("GetApi", 1, new Type[1] { typeof(string) })?.MakeGenericMethod(fieldType).Invoke(mod.Helper.ModRegistry, new object[1] { uniqueId });
			if (api == null && isRequired)
			{
				Log.Error("[" + name + "] Can't access required API");
			}
			setter(instance, api);
		}
	}

	public class Instance : FieldHandler
	{
		protected override void Handle(string name, Type fieldType, Func<object?, object?> getter, Action<object?, object?> setter, object? instance, IMod mod, object[]? args = null)
		{
			setter(instance, mod);
		}
	}

	public SMod()
		: base(1)
	{
	}

	public override void Handle(Type type, object? instance, IMod mod, object[]? args = null)
	{
		if (Priority < 1)
		{
			Log.Error("ModEntry cannot be loaded with priority < 1");
		}
		else
		{
			base.Handle(type, mod, mod, args);
		}
	}
}
