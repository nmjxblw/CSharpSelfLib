using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace MonoGame.Framework.Utilities;

internal static class ReflectionHelpers
{
	/// <summary>
	/// Generics handler for Marshal.SizeOf
	/// </summary>
	internal static class SizeOf<T>
	{
		private static int _sizeOf;

		static SizeOf()
		{
			SizeOf<T>._sizeOf = Marshal.SizeOf<T>();
		}

		public static int Get()
		{
			return SizeOf<T>._sizeOf;
		}
	}

	public static bool IsValueType(Type targetType)
	{
		if (targetType == null)
		{
			throw new NullReferenceException("Must supply the targetType parameter");
		}
		return targetType.IsValueType;
	}

	public static Type GetBaseType(Type targetType)
	{
		if (targetType == null)
		{
			throw new NullReferenceException("Must supply the targetType parameter");
		}
		return targetType.BaseType;
	}

	/// <summary>
	/// Returns the Assembly of a Type
	/// </summary>
	public static Assembly GetAssembly(Type targetType)
	{
		if (targetType == null)
		{
			throw new NullReferenceException("Must supply the targetType parameter");
		}
		return targetType.Assembly;
	}

	/// <summary>
	/// Returns true if the given type represents a non-object type that is not abstract.
	/// </summary>
	public static bool IsConcreteClass(Type t)
	{
		if (t == null)
		{
			throw new NullReferenceException("Must supply the t (type) parameter");
		}
		if (t == typeof(object))
		{
			return false;
		}
		if (t.IsClass && !t.IsAbstract)
		{
			return true;
		}
		return false;
	}

	public static MethodInfo GetMethodInfo(Type type, string methodName)
	{
		return type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
	}

	public static MethodInfo GetPropertyGetMethod(PropertyInfo property)
	{
		if (property == null)
		{
			throw new NullReferenceException("Must supply the property parameter");
		}
		return property.GetGetMethod();
	}

	public static MethodInfo GetPropertySetMethod(PropertyInfo property)
	{
		if (property == null)
		{
			throw new NullReferenceException("Must supply the property parameter");
		}
		return property.GetSetMethod();
	}

	public static T GetCustomAttribute<T>(MemberInfo member) where T : Attribute
	{
		if (member == null)
		{
			throw new NullReferenceException("Must supply the member parameter");
		}
		return Attribute.GetCustomAttribute(member, typeof(T)) as T;
	}

	/// <summary>
	/// Returns true if the get method of the given property exist and are public.
	/// Note that we allow a getter-only property to be serialized (and deserialized),
	/// *if* CanDeserializeIntoExistingObject is true for the property type.
	/// </summary>
	public static bool PropertyIsPublic(PropertyInfo property)
	{
		if (property == null)
		{
			throw new NullReferenceException("Must supply the property parameter");
		}
		MethodInfo getMethod = ReflectionHelpers.GetPropertyGetMethod(property);
		if (getMethod == null || !getMethod.IsPublic)
		{
			return false;
		}
		return true;
	}

	/// <summary>
	/// Returns true if the given type can be assigned the given value
	/// </summary>
	public static bool IsAssignableFrom(Type type, object value)
	{
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		return ReflectionHelpers.IsAssignableFromType(type, value.GetType());
	}

	/// <summary>
	/// Returns true if the given type can be assigned a value with the given object type
	/// </summary>
	public static bool IsAssignableFromType(Type type, Type objectType)
	{
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		if (objectType == null)
		{
			throw new ArgumentNullException("objectType");
		}
		if (type.IsAssignableFrom(objectType))
		{
			return true;
		}
		return false;
	}

	/// <summary>
	/// Fallback handler for Marshal.SizeOf(type)
	/// </summary>
	internal static int ManagedSizeOf(Type type)
	{
		return Marshal.SizeOf(type);
	}
}
