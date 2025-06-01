using System;
using System.Collections.Generic;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Serialization;

/// <summary>
/// Resolves member mappings for a type, camel casing property names.
/// </summary>
public class CamelCasePropertyNamesContractResolver : DefaultContractResolver
{
	private static readonly object TypeContractCacheLock = new object();

	private static readonly DefaultJsonNameTable NameTable = new DefaultJsonNameTable();

	private static Dictionary<StructMultiKey<Type, Type>, JsonContract>? _contractCache;

	/// <summary>
	/// Initializes a new instance of the <see cref="T:Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver" /> class.
	/// </summary>
	public CamelCasePropertyNamesContractResolver()
	{
		base.NamingStrategy = new CamelCaseNamingStrategy
		{
			ProcessDictionaryKeys = true,
			OverrideSpecifiedNames = true
		};
	}

	/// <summary>
	/// Resolves the contract for a given type.
	/// </summary>
	/// <param name="type">The type to resolve a contract for.</param>
	/// <returns>The contract for a given type.</returns>
	public override JsonContract ResolveContract(Type type)
	{
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		StructMultiKey<Type, Type> key = new StructMultiKey<Type, Type>(base.GetType(), type);
		Dictionary<StructMultiKey<Type, Type>, JsonContract> contractCache = CamelCasePropertyNamesContractResolver._contractCache;
		if (contractCache == null || !contractCache.TryGetValue(key, out var value))
		{
			value = this.CreateContract(type);
			lock (CamelCasePropertyNamesContractResolver.TypeContractCacheLock)
			{
				contractCache = CamelCasePropertyNamesContractResolver._contractCache;
				Dictionary<StructMultiKey<Type, Type>, JsonContract> obj = ((contractCache != null) ? new Dictionary<StructMultiKey<Type, Type>, JsonContract>(contractCache) : new Dictionary<StructMultiKey<Type, Type>, JsonContract>());
				obj[key] = value;
				CamelCasePropertyNamesContractResolver._contractCache = obj;
			}
		}
		return value;
	}

	internal override DefaultJsonNameTable GetNameTable()
	{
		return CamelCasePropertyNamesContractResolver.NameTable;
	}
}
