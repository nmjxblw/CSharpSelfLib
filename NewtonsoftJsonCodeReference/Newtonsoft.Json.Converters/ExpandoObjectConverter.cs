using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Converters;

/// <summary>
/// Converts an <see cref="T:System.Dynamic.ExpandoObject" /> to and from JSON.
/// </summary>
public class ExpandoObjectConverter : JsonConverter
{
	/// <summary>
	/// Gets a value indicating whether this <see cref="T:Newtonsoft.Json.JsonConverter" /> can write JSON.
	/// </summary>
	/// <value>
	/// 	<c>true</c> if this <see cref="T:Newtonsoft.Json.JsonConverter" /> can write JSON; otherwise, <c>false</c>.
	/// </value>
	public override bool CanWrite => false;

	/// <summary>
	/// Writes the JSON representation of the object.
	/// </summary>
	/// <param name="writer">The <see cref="T:Newtonsoft.Json.JsonWriter" /> to write to.</param>
	/// <param name="value">The value.</param>
	/// <param name="serializer">The calling serializer.</param>
	public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
	{
	}

	/// <summary>
	/// Reads the JSON representation of the object.
	/// </summary>
	/// <param name="reader">The <see cref="T:Newtonsoft.Json.JsonReader" /> to read from.</param>
	/// <param name="objectType">Type of the object.</param>
	/// <param name="existingValue">The existing value of object being read.</param>
	/// <param name="serializer">The calling serializer.</param>
	/// <returns>The object value.</returns>
	public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
	{
		return this.ReadValue(reader);
	}

	private object? ReadValue(JsonReader reader)
	{
		if (!reader.MoveToContent())
		{
			throw JsonSerializationException.Create(reader, "Unexpected end when reading ExpandoObject.");
		}
		switch (reader.TokenType)
		{
		case JsonToken.StartObject:
			return this.ReadObject(reader);
		case JsonToken.StartArray:
			return this.ReadList(reader);
		default:
			if (JsonTokenUtils.IsPrimitiveToken(reader.TokenType))
			{
				return reader.Value;
			}
			throw JsonSerializationException.Create(reader, "Unexpected token when converting ExpandoObject: {0}".FormatWith(CultureInfo.InvariantCulture, reader.TokenType));
		}
	}

	private object ReadList(JsonReader reader)
	{
		IList<object> list = new List<object>();
		while (reader.Read())
		{
			switch (reader.TokenType)
			{
			case JsonToken.EndArray:
				return list;
			case JsonToken.Comment:
				continue;
			}
			object item = this.ReadValue(reader);
			list.Add(item);
		}
		throw JsonSerializationException.Create(reader, "Unexpected end when reading ExpandoObject.");
	}

	private object ReadObject(JsonReader reader)
	{
		IDictionary<string, object> dictionary = new ExpandoObject();
		while (reader.Read())
		{
			switch (reader.TokenType)
			{
			case JsonToken.PropertyName:
			{
				string key = reader.Value.ToString();
				if (!reader.Read())
				{
					throw JsonSerializationException.Create(reader, "Unexpected end when reading ExpandoObject.");
				}
				object value = this.ReadValue(reader);
				dictionary[key] = value;
				break;
			}
			case JsonToken.EndObject:
				return dictionary;
			}
		}
		throw JsonSerializationException.Create(reader, "Unexpected end when reading ExpandoObject.");
	}

	/// <summary>
	/// Determines whether this instance can convert the specified object type.
	/// </summary>
	/// <param name="objectType">Type of the object.</param>
	/// <returns>
	/// 	<c>true</c> if this instance can convert the specified object type; otherwise, <c>false</c>.
	/// </returns>
	public override bool CanConvert(Type objectType)
	{
		return objectType == typeof(ExpandoObject);
	}
}
