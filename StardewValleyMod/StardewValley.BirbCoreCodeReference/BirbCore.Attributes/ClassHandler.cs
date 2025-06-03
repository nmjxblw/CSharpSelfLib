using System;
using System.Reflection;
using StardewModdingAPI;

namespace BirbCore.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public abstract class ClassHandler(int priority = 0) : Attribute()
{
	public int Priority = priority;

	public virtual void Handle(Type type, object? instance, IMod mod, object[]? args = null)
	{
		string className = ToString() ?? "";
		FieldInfo[] fields = type.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		foreach (FieldInfo fieldInfo in fields)
		{
			foreach (Attribute attribute in fieldInfo.GetCustomAttributes())
			{
				string attributeName = attribute.ToString() ?? "";
				if (attribute is FieldHandler handler && attributeName.StartsWith(className))
				{
					try
					{
						handler.Handle(fieldInfo, instance, mod, args);
					}
					catch (Exception value)
					{
						mod.Monitor.Log($"BirbCore failed to parse {handler.GetType().Name} field {fieldInfo.Name}: {value}", (LogLevel)4);
					}
				}
			}
		}
		PropertyInfo[] properties = type.GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		foreach (PropertyInfo propertyInfo in properties)
		{
			foreach (Attribute attribute2 in propertyInfo.GetCustomAttributes())
			{
				string attributeName2 = attribute2.ToString() ?? "";
				if (attribute2 is FieldHandler handler2 && attributeName2.StartsWith(className))
				{
					try
					{
						handler2.Handle(propertyInfo, instance, mod, args);
					}
					catch (Exception value2)
					{
						mod.Monitor.Log($"BirbCore failed to parse {handler2.GetType().Name} property {propertyInfo.Name}: {value2}", (LogLevel)4);
					}
				}
			}
		}
		MethodInfo[] methods = type.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		foreach (MethodInfo method in methods)
		{
			foreach (Attribute attribute3 in method.GetCustomAttributes())
			{
				string attributeName3 = attribute3.ToString() ?? "";
				if (attribute3 is MethodHandler handler3 && attributeName3.StartsWith(className))
				{
					try
					{
						handler3.Handle(method, instance, mod, args);
					}
					catch (Exception value3)
					{
						mod.Monitor.Log($"BirbCore failed to parse {handler3.GetType().Name} method {method.Name}: {value3}", (LogLevel)4);
					}
				}
			}
		}
	}
}
