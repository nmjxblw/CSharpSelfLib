using System;
using System.Reflection;
using Newtonsoft.Json.Serialization;

namespace Newtonsoft.Json.Utilities;

internal class LateBoundReflectionDelegateFactory : ReflectionDelegateFactory
{
	private static readonly LateBoundReflectionDelegateFactory _instance = new LateBoundReflectionDelegateFactory();

	internal static ReflectionDelegateFactory Instance => LateBoundReflectionDelegateFactory._instance;

	public override ObjectConstructor<object> CreateParameterizedConstructor(MethodBase method)
	{
		ValidationUtils.ArgumentNotNull(method, "method");
		ConstructorInfo c = method as ConstructorInfo;
		if ((object)c != null)
		{
			return (object?[] a) => c.Invoke(a);
		}
		return (object?[] a) => method.Invoke(null, a);
	}

	public override MethodCall<T, object?> CreateMethodCall<T>(MethodBase method)
	{
		ValidationUtils.ArgumentNotNull(method, "method");
		ConstructorInfo c = method as ConstructorInfo;
		if ((object)c != null)
		{
			return (T o, object?[] a) => c.Invoke(a);
		}
		return (T o, object?[] a) => method.Invoke(o, a);
	}

	public override Func<T> CreateDefaultConstructor<T>(Type type)
	{
		ValidationUtils.ArgumentNotNull(type, "type");
		if (type.IsValueType())
		{
			return () => (T)Activator.CreateInstance(type);
		}
		ConstructorInfo constructorInfo = ReflectionUtils.GetDefaultConstructor(type, nonPublic: true);
		if (constructorInfo == null)
		{
			throw new InvalidOperationException("Unable to find default constructor for " + type.FullName);
		}
		return () => (T)constructorInfo.Invoke(null);
	}

	public override Func<T, object?> CreateGet<T>(PropertyInfo propertyInfo)
	{
		ValidationUtils.ArgumentNotNull(propertyInfo, "propertyInfo");
		return (T o) => propertyInfo.GetValue(o, null);
	}

	public override Func<T, object?> CreateGet<T>(FieldInfo fieldInfo)
	{
		ValidationUtils.ArgumentNotNull(fieldInfo, "fieldInfo");
		return (T o) => fieldInfo.GetValue(o);
	}

	public override Action<T, object?> CreateSet<T>(FieldInfo fieldInfo)
	{
		ValidationUtils.ArgumentNotNull(fieldInfo, "fieldInfo");
		return delegate(T o, object? v)
		{
			fieldInfo.SetValue(o, v);
		};
	}

	public override Action<T, object?> CreateSet<T>(PropertyInfo propertyInfo)
	{
		ValidationUtils.ArgumentNotNull(propertyInfo, "propertyInfo");
		return delegate(T o, object? v)
		{
			propertyInfo.SetValue(o, v, null);
		};
	}
}
