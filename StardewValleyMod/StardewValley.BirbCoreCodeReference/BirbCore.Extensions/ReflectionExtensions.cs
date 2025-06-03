using System;
using System.Reflection;

namespace BirbCore.Extensions;

public static class ReflectionExtensions
{
	public const BindingFlags ALL_DECLARED = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

	public static bool TryGetMemberOfType(this Type type, Type memberType, out MemberInfo memberInfo)
	{
		FieldInfo[] fields = type.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		foreach (FieldInfo fieldInfo in fields)
		{
			if (!(fieldInfo.FieldType != memberType))
			{
				memberInfo = fieldInfo;
				return true;
			}
		}
		PropertyInfo[] properties = type.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		foreach (PropertyInfo propertyInfo in properties)
		{
			if (!(propertyInfo.PropertyType != memberType))
			{
				memberInfo = propertyInfo;
				return true;
			}
		}
		memberInfo = typeof(int);
		return false;
	}

	public static bool TryGetMemberOfName(this Type type, string name, out MemberInfo memberInfo)
	{
		FieldInfo[] fields = type.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		foreach (FieldInfo fieldInfo in fields)
		{
			if (!(fieldInfo.Name != name))
			{
				memberInfo = fieldInfo;
				return true;
			}
		}
		PropertyInfo[] properties = type.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		foreach (PropertyInfo propertyInfo in properties)
		{
			if (!(propertyInfo.Name != name))
			{
				memberInfo = propertyInfo;
				return true;
			}
		}
		memberInfo = typeof(int);
		return false;
	}

	public static bool TryGetGetterOfName(this Type type, string name, out Func<object?, object?> getter)
	{
		if (!type.TryGetMemberOfName(name, out MemberInfo memberInfo))
		{
			getter = null;
			return false;
		}
		getter = memberInfo.GetGetter();
		return true;
	}

	public static bool TryGetSetterOfName(this Type type, string name, out Action<object?, object?> setter)
	{
		if (!type.TryGetMemberOfName(name, out MemberInfo memberInfo))
		{
			setter = null;
			return false;
		}
		setter = memberInfo.GetSetter();
		return true;
	}

	public static bool TryGetMemberWithCustomAttribute(this Type type, Type attributeType, out MemberInfo memberInfo)
	{
		FieldInfo[] fields = type.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		foreach (FieldInfo fieldInfo in fields)
		{
			foreach (Attribute attribute in fieldInfo.GetCustomAttributes())
			{
				if (!(attribute.GetType() != attributeType))
				{
					memberInfo = fieldInfo;
					return true;
				}
			}
		}
		PropertyInfo[] properties = type.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		foreach (PropertyInfo propertyInfo in properties)
		{
			foreach (Attribute attribute2 in propertyInfo.GetCustomAttributes())
			{
				if (!(attribute2.GetType() != attributeType))
				{
					memberInfo = propertyInfo;
					return true;
				}
			}
		}
		memberInfo = typeof(int);
		return false;
	}

	public static Type GetReflectedType(this MemberInfo member)
	{
		if (!(member is FieldInfo field))
		{
			if (member is PropertyInfo property)
			{
				return property.PropertyType;
			}
			return typeof(int);
		}
		return field.FieldType;
	}

	public static Func<object?, object?> GetGetter(this MemberInfo member)
	{
		if (!(member is FieldInfo field))
		{
			if (member is PropertyInfo property)
			{
				return property.GetValue;
			}
			return (object? a) => a;
		}
		return field.GetValue;
	}

	public static Action<object?, object?> GetSetter(this MemberInfo member)
	{
		if (!(member is FieldInfo field))
		{
			if (member is PropertyInfo property)
			{
				return property.SetValue;
			}
			return delegate
			{
			};
		}
		return field.SetValue;
	}

	public static T InitDelegate<T>(this MethodInfo method, object? instance = null) where T : Delegate
	{
		if (method.IsStatic)
		{
			return (T)Delegate.CreateDelegate(typeof(T), method);
		}
		return (T)Delegate.CreateDelegate(typeof(T), instance, method);
	}
}
