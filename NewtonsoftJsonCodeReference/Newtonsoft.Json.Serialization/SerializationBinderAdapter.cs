using System;
using System.Runtime.Serialization;

namespace Newtonsoft.Json.Serialization;

internal class SerializationBinderAdapter : ISerializationBinder
{
	public readonly SerializationBinder SerializationBinder;

	public SerializationBinderAdapter(SerializationBinder serializationBinder)
	{
		this.SerializationBinder = serializationBinder;
	}

	public Type BindToType(string? assemblyName, string typeName)
	{
		return this.SerializationBinder.BindToType(assemblyName, typeName);
	}

	public void BindToName(Type serializedType, out string? assemblyName, out string? typeName)
	{
		this.SerializationBinder.BindToName(serializedType, out assemblyName, out typeName);
	}
}
