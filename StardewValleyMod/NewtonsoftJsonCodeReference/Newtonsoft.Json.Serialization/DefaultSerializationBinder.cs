using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Runtime.Serialization;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Serialization;

/// <summary>
/// The default serialization binder used when resolving and loading classes from type names.
/// </summary>
public class DefaultSerializationBinder : SerializationBinder, ISerializationBinder
{
	internal static readonly DefaultSerializationBinder Instance = new DefaultSerializationBinder();

	private readonly ThreadSafeStore<StructMultiKey<string?, string>, Type> _typeCache;

	/// <summary>
	/// Initializes a new instance of the <see cref="T:Newtonsoft.Json.Serialization.DefaultSerializationBinder" /> class.
	/// </summary>
	public DefaultSerializationBinder()
	{
		this._typeCache = new ThreadSafeStore<StructMultiKey<string, string>, Type>(GetTypeFromTypeNameKey);
	}

	private Type GetTypeFromTypeNameKey(StructMultiKey<string?, string> typeNameKey)
	{
		string value = typeNameKey.Value1;
		string value2 = typeNameKey.Value2;
		if (value != null)
		{
			Assembly assembly = Assembly.LoadWithPartialName(value);
			if (assembly == null)
			{
				Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
				foreach (Assembly assembly2 in assemblies)
				{
					if (assembly2.FullName == value || assembly2.GetName().Name == value)
					{
						assembly = assembly2;
						break;
					}
				}
			}
			if (assembly == null)
			{
				throw new JsonSerializationException("Could not load assembly '{0}'.".FormatWith(CultureInfo.InvariantCulture, value));
			}
			Type type = assembly.GetType(value2);
			if (type == null)
			{
				if (StringUtils.IndexOf(value2, '`') >= 0)
				{
					try
					{
						type = this.GetGenericTypeFromTypeName(value2, assembly);
					}
					catch (Exception innerException)
					{
						throw new JsonSerializationException("Could not find type '{0}' in assembly '{1}'.".FormatWith(CultureInfo.InvariantCulture, value2, assembly.FullName), innerException);
					}
				}
				if (type == null)
				{
					throw new JsonSerializationException("Could not find type '{0}' in assembly '{1}'.".FormatWith(CultureInfo.InvariantCulture, value2, assembly.FullName));
				}
			}
			return type;
		}
		return Type.GetType(value2);
	}

	private Type? GetGenericTypeFromTypeName(string typeName, Assembly assembly)
	{
		Type result = null;
		int num = StringUtils.IndexOf(typeName, '[');
		if (num >= 0)
		{
			string name = typeName.Substring(0, num);
			Type type = assembly.GetType(name);
			if (type != null)
			{
				List<Type> list = new List<Type>();
				int num2 = 0;
				int num3 = 0;
				int num4 = typeName.Length - 1;
				for (int i = num + 1; i < num4; i++)
				{
					switch (typeName[i])
					{
					case '[':
						if (num2 == 0)
						{
							num3 = i + 1;
						}
						num2++;
						break;
					case ']':
						num2--;
						if (num2 == 0)
						{
							StructMultiKey<string, string> typeNameKey = ReflectionUtils.SplitFullyQualifiedTypeName(typeName.Substring(num3, i - num3));
							list.Add(this.GetTypeByName(typeNameKey));
						}
						break;
					}
				}
				result = type.MakeGenericType(list.ToArray());
			}
		}
		return result;
	}

	private Type GetTypeByName(StructMultiKey<string?, string> typeNameKey)
	{
		return this._typeCache.Get(typeNameKey);
	}

	/// <summary>
	/// When overridden in a derived class, controls the binding of a serialized object to a type.
	/// </summary>
	/// <param name="assemblyName">Specifies the <see cref="T:System.Reflection.Assembly" /> name of the serialized object.</param>
	/// <param name="typeName">Specifies the <see cref="T:System.Type" /> name of the serialized object.</param>
	/// <returns>
	/// The type of the object the formatter creates a new instance of.
	/// </returns>
	public override Type BindToType(string? assemblyName, string typeName)
	{
		return this.GetTypeByName(new StructMultiKey<string, string>(assemblyName, typeName));
	}

	/// <summary>
	/// When overridden in a derived class, controls the binding of a serialized object to a type.
	/// </summary>
	/// <param name="serializedType">The type of the object the formatter creates a new instance of.</param>
	/// <param name="assemblyName">Specifies the <see cref="T:System.Reflection.Assembly" /> name of the serialized object.</param>
	/// <param name="typeName">Specifies the <see cref="T:System.Type" /> name of the serialized object.</param>
	public override void BindToName(Type serializedType, out string? assemblyName, out string? typeName)
	{
		assemblyName = serializedType.Assembly.FullName;
		typeName = serializedType.FullName;
	}
}
