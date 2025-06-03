using System;
using System.Linq;
using System.Reflection;

namespace Microsoft.Xna.Framework.Content;

internal static class ContentExtensions
{
	public static ConstructorInfo GetDefaultConstructor(this Type type)
	{
		BindingFlags attrs = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
		return type.GetConstructor(attrs, null, new Type[0], null);
	}

	public static PropertyInfo[] GetAllProperties(this Type type)
	{
		return type.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).ToList().FindAll((PropertyInfo p) => p.GetGetMethod(nonPublic: true) != null && p.GetGetMethod(nonPublic: true) == p.GetGetMethod(nonPublic: true).GetBaseDefinition())
			.ToArray();
	}

	public static FieldInfo[] GetAllFields(this Type type)
	{
		BindingFlags attrs = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
		return type.GetFields(attrs);
	}

	public static bool IsClass(this Type type)
	{
		return type.IsClass;
	}
}
