using System;
using System.Collections.Concurrent;

namespace Force.DeepCloner.Helpers;

internal static class DeepClonerCache
{
	private static readonly ConcurrentDictionary<Type, object> _typeCache = new ConcurrentDictionary<Type, object>();

	private static readonly ConcurrentDictionary<Type, object> _typeCacheDeepTo = new ConcurrentDictionary<Type, object>();

	private static readonly ConcurrentDictionary<Type, object> _typeCacheShallowTo = new ConcurrentDictionary<Type, object>();

	private static readonly ConcurrentDictionary<Type, object> _structAsObjectCache = new ConcurrentDictionary<Type, object>();

	private static readonly ConcurrentDictionary<Tuple<Type, Type>, object> _typeConvertCache = new ConcurrentDictionary<Tuple<Type, Type>, object>();

	public static object GetOrAddClass<T>(Type type, Func<Type, T> adder)
	{
		if (DeepClonerCache._typeCache.TryGetValue(type, out var value))
		{
			return value;
		}
		lock (type)
		{
			return DeepClonerCache._typeCache.GetOrAdd(type, (Type t) => adder(t));
		}
	}

	public static object GetOrAddDeepClassTo<T>(Type type, Func<Type, T> adder)
	{
		if (DeepClonerCache._typeCacheDeepTo.TryGetValue(type, out var value))
		{
			return value;
		}
		lock (type)
		{
			return DeepClonerCache._typeCacheDeepTo.GetOrAdd(type, (Type t) => adder(t));
		}
	}

	public static object GetOrAddShallowClassTo<T>(Type type, Func<Type, T> adder)
	{
		if (DeepClonerCache._typeCacheShallowTo.TryGetValue(type, out var value))
		{
			return value;
		}
		lock (type)
		{
			return DeepClonerCache._typeCacheShallowTo.GetOrAdd(type, (Type t) => adder(t));
		}
	}

	public static object GetOrAddStructAsObject<T>(Type type, Func<Type, T> adder)
	{
		if (DeepClonerCache._structAsObjectCache.TryGetValue(type, out var value))
		{
			return value;
		}
		lock (type)
		{
			return DeepClonerCache._structAsObjectCache.GetOrAdd(type, (Type t) => adder(t));
		}
	}

	public static T GetOrAddConvertor<T>(Type from, Type to, Func<Type, Type, T> adder)
	{
		return (T)DeepClonerCache._typeConvertCache.GetOrAdd(new Tuple<Type, Type>(from, to), (Tuple<Type, Type> tuple) => adder(tuple.Item1, tuple.Item2));
	}

	/// <summary>
	/// This method can be used when we switch between safe / unsafe variants (for testing)
	/// </summary>
	public static void ClearCache()
	{
		DeepClonerCache._typeCache.Clear();
		DeepClonerCache._typeCacheDeepTo.Clear();
		DeepClonerCache._typeCacheShallowTo.Clear();
		DeepClonerCache._structAsObjectCache.Clear();
		DeepClonerCache._typeConvertCache.Clear();
	}
}
