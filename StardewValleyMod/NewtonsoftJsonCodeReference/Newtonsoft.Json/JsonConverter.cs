using System;
using System.Globalization;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json;

/// <summary>
/// Converts an object to and from JSON.
/// </summary>
public abstract class JsonConverter
{
	/// <summary>
	/// Gets a value indicating whether this <see cref="T:Newtonsoft.Json.JsonConverter" /> can read JSON.
	/// </summary>
	/// <value><c>true</c> if this <see cref="T:Newtonsoft.Json.JsonConverter" /> can read JSON; otherwise, <c>false</c>.</value>
	public virtual bool CanRead => true;

	/// <summary>
	/// Gets a value indicating whether this <see cref="T:Newtonsoft.Json.JsonConverter" /> can write JSON.
	/// </summary>
	/// <value><c>true</c> if this <see cref="T:Newtonsoft.Json.JsonConverter" /> can write JSON; otherwise, <c>false</c>.</value>
	public virtual bool CanWrite => true;

	/// <summary>
	/// Writes the JSON representation of the object.
	/// </summary>
	/// <param name="writer">The <see cref="T:Newtonsoft.Json.JsonWriter" /> to write to.</param>
	/// <param name="value">The value.</param>
	/// <param name="serializer">The calling serializer.</param>
	public abstract void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer);

	/// <summary>
	/// Reads the JSON representation of the object.
	/// </summary>
	/// <param name="reader">The <see cref="T:Newtonsoft.Json.JsonReader" /> to read from.</param>
	/// <param name="objectType">Type of the object.</param>
	/// <param name="existingValue">The existing value of object being read.</param>
	/// <param name="serializer">The calling serializer.</param>
	/// <returns>The object value.</returns>
	public abstract object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer);

	/// <summary>
	/// Determines whether this instance can convert the specified object type.
	/// </summary>
	/// <param name="objectType">Type of the object.</param>
	/// <returns>
	/// 	<c>true</c> if this instance can convert the specified object type; otherwise, <c>false</c>.
	/// </returns>
	public abstract bool CanConvert(Type objectType);
}
/// <summary>
/// Converts an object to and from JSON.
/// </summary>
/// <typeparam name="T">The object type to convert.</typeparam>
public abstract class JsonConverter<T> : JsonConverter
{
	/// <summary>
	/// Writes the JSON representation of the object.
	/// </summary>
	/// <param name="writer">The <see cref="T:Newtonsoft.Json.JsonWriter" /> to write to.</param>
	/// <param name="value">The value.</param>
	/// <param name="serializer">The calling serializer.</param>
	public sealed override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
	{
		if (!((value != null) ? (value is T) : ReflectionUtils.IsNullable(typeof(T))))
		{
			throw new JsonSerializationException("Converter cannot write specified value to JSON. {0} is required.".FormatWith(CultureInfo.InvariantCulture, typeof(T)));
		}
		this.WriteJson(writer, (T)value, serializer);
	}

	/// <summary>
	/// Writes the JSON representation of the object.
	/// </summary>
	/// <param name="writer">The <see cref="T:Newtonsoft.Json.JsonWriter" /> to write to.</param>
	/// <param name="value">The value.</param>
	/// <param name="serializer">The calling serializer.</param>
	public abstract void WriteJson(JsonWriter writer, T? value, JsonSerializer serializer);

	/// <summary>
	/// Reads the JSON representation of the object.
	/// </summary>
	/// <param name="reader">The <see cref="T:Newtonsoft.Json.JsonReader" /> to read from.</param>
	/// <param name="objectType">Type of the object.</param>
	/// <param name="existingValue">The existing value of object being read.</param>
	/// <param name="serializer">The calling serializer.</param>
	/// <returns>The object value.</returns>
	public sealed override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
	{
		bool flag = existingValue == null;
		if (!flag && !(existingValue is T))
		{
			throw new JsonSerializationException("Converter cannot read JSON with the specified existing value. {0} is required.".FormatWith(CultureInfo.InvariantCulture, typeof(T)));
		}
		return this.ReadJson(reader, objectType, flag ? default(T) : ((T)existingValue), !flag, serializer);
	}

	/// <summary>
	/// Reads the JSON representation of the object.
	/// </summary>
	/// <param name="reader">The <see cref="T:Newtonsoft.Json.JsonReader" /> to read from.</param>
	/// <param name="objectType">Type of the object.</param>
	/// <param name="existingValue">The existing value of object being read. If there is no existing value then <c>null</c> will be used.</param>
	/// <param name="hasExistingValue">The existing value has a value.</param>
	/// <param name="serializer">The calling serializer.</param>
	/// <returns>The object value.</returns>
	public abstract T? ReadJson(JsonReader reader, Type objectType, T? existingValue, bool hasExistingValue, JsonSerializer serializer);

	/// <summary>
	/// Determines whether this instance can convert the specified object type.
	/// </summary>
	/// <param name="objectType">Type of the object.</param>
	/// <returns>
	/// 	<c>true</c> if this instance can convert the specified object type; otherwise, <c>false</c>.
	/// </returns>
	public sealed override bool CanConvert(Type objectType)
	{
		return typeof(T).IsAssignableFrom(objectType);
	}
}
