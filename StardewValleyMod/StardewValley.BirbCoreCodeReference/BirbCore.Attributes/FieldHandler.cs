using System;
using System.Reflection;
using StardewModdingAPI;

namespace BirbCore.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public abstract class FieldHandler : Attribute
{
	public void Handle(FieldInfo fieldInfo, object? instance, IMod mod, object[]? args = null)
	{
		Handle(fieldInfo.Name, fieldInfo.FieldType, fieldInfo.GetValue, fieldInfo.SetValue, instance, mod, args);
	}

	public void Handle(PropertyInfo propertyInfo, object? instance, IMod mod, object[]? args = null)
	{
		Handle(propertyInfo.Name, propertyInfo.PropertyType, propertyInfo.GetValue, propertyInfo.SetValue, instance, mod, args);
	}

	protected abstract void Handle(string name, Type fieldType, Func<object?, object?> getter, Action<object?, object?> setter, object? instance, IMod mod, object[]? args = null);
}
