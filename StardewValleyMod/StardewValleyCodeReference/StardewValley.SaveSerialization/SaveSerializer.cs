using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using StardewValley.Quests;
using StardewValley.SaveMigrations;

namespace StardewValley.SaveSerialization;

public static class SaveSerializer
{
	private static readonly Dictionary<Type, XmlSerializer> _serializerLookup = new Dictionary<Type, XmlSerializer>();

	public static XmlSerializer GetSerializer(Type type)
	{
		if (!SaveSerializer._serializerLookup.TryGetValue(type, out var obj))
		{
			if (type == typeof(SaveGame))
			{
				return SaveGame.serializer;
			}
			if (type == typeof(Farmer))
			{
				return SaveGame.farmerSerializer;
			}
			if (type == typeof(GameLocation))
			{
				return SaveGame.locationSerializer;
			}
			if (type == typeof(DescriptionElement))
			{
				return SaveGame.descriptionElementSerializer;
			}
			if (type == typeof(SaveMigrator_1_6.LegacyDescriptionElement))
			{
				return SaveGame.legacyDescriptionElementSerializer;
			}
			obj = new XmlSerializer(type);
			SaveSerializer._serializerLookup.Add(type, obj);
		}
		return obj;
	}

	public static void SerializeFast(this XmlSerializer serializer, Stream stream, object obj)
	{
		serializer.Serialize(stream, obj);
	}

	public static void Serialize<T>(XmlWriter xmlWriter, T obj)
	{
		SaveSerializer.GetSerializer(typeof(T)).SerializeFast(xmlWriter, obj);
	}

	public static void SerializeFast(this XmlSerializer serializer, XmlWriter xmlWriter, object obj)
	{
		serializer.Serialize(xmlWriter, obj);
	}

	public static T Deserialize<T>(Stream stream)
	{
		return (T)SaveSerializer.GetSerializer(typeof(T)).DeserializeFast(stream);
	}

	public static T Deserialize<T>(XmlReader reader)
	{
		return (T)SaveSerializer.GetSerializer(typeof(T)).DeserializeFast(reader);
	}

	public static object DeserializeFast(this XmlSerializer serializer, Stream stream)
	{
		return serializer.Deserialize(stream);
	}

	public static object DeserializeFast(this XmlSerializer serializer, XmlReader reader)
	{
		return serializer.Deserialize(reader);
	}
}
