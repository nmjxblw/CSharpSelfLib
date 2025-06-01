using System;
using System.Globalization;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Converters;

/// <summary>
/// Converts a <see cref="T:System.DateTime" /> to and from Unix epoch time
/// </summary>
public class UnixDateTimeConverter : DateTimeConverterBase
{
	internal static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

	/// <summary>
	/// Gets or sets a value indicating whether the dates before Unix epoch
	/// should converted to and from JSON.
	/// </summary>
	/// <value>
	/// <c>true</c> to allow converting dates before Unix epoch to and from JSON;
	/// <c>false</c> to throw an exception when a date being converted to or from JSON
	/// occurred before Unix epoch. The default value is <c>false</c>.
	/// </value>
	public bool AllowPreEpoch { get; set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="T:Newtonsoft.Json.Converters.UnixDateTimeConverter" /> class.
	/// </summary>
	public UnixDateTimeConverter()
		: this(allowPreEpoch: false)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:Newtonsoft.Json.Converters.UnixDateTimeConverter" /> class.
	/// </summary>
	/// <param name="allowPreEpoch">
	/// <c>true</c> to allow converting dates before Unix epoch to and from JSON;
	/// <c>false</c> to throw an exception when a date being converted to or from JSON
	/// occurred before Unix epoch. The default value is <c>false</c>.
	/// </param>
	public UnixDateTimeConverter(bool allowPreEpoch)
	{
		this.AllowPreEpoch = allowPreEpoch;
	}

	/// <summary>
	/// Writes the JSON representation of the object.
	/// </summary>
	/// <param name="writer">The <see cref="T:Newtonsoft.Json.JsonWriter" /> to write to.</param>
	/// <param name="value">The value.</param>
	/// <param name="serializer">The calling serializer.</param>
	public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
	{
		long num;
		if (value is DateTime dateTime)
		{
			num = (long)(dateTime.ToUniversalTime() - UnixDateTimeConverter.UnixEpoch).TotalSeconds;
		}
		else
		{
			if (!(value is DateTimeOffset dateTimeOffset))
			{
				throw new JsonSerializationException("Expected date object value.");
			}
			num = (long)(dateTimeOffset.ToUniversalTime() - UnixDateTimeConverter.UnixEpoch).TotalSeconds;
		}
		if (!this.AllowPreEpoch && num < 0)
		{
			throw new JsonSerializationException("Cannot convert date value that is before Unix epoch of 00:00:00 UTC on 1 January 1970.");
		}
		writer.WriteValue(num);
	}

	/// <summary>
	/// Reads the JSON representation of the object.
	/// </summary>
	/// <param name="reader">The <see cref="T:Newtonsoft.Json.JsonReader" /> to read from.</param>
	/// <param name="objectType">Type of the object.</param>
	/// <param name="existingValue">The existing property value of the JSON that is being converted.</param>
	/// <param name="serializer">The calling serializer.</param>
	/// <returns>The object value.</returns>
	public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
	{
		bool flag = ReflectionUtils.IsNullable(objectType);
		if (reader.TokenType == JsonToken.Null)
		{
			if (!flag)
			{
				throw JsonSerializationException.Create(reader, "Cannot convert null value to {0}.".FormatWith(CultureInfo.InvariantCulture, objectType));
			}
			return null;
		}
		long result;
		if (reader.TokenType == JsonToken.Integer)
		{
			result = (long)reader.Value;
		}
		else
		{
			if (reader.TokenType != JsonToken.String)
			{
				throw JsonSerializationException.Create(reader, "Unexpected token parsing date. Expected Integer or String, got {0}.".FormatWith(CultureInfo.InvariantCulture, reader.TokenType));
			}
			if (!long.TryParse((string)reader.Value, out result))
			{
				throw JsonSerializationException.Create(reader, "Cannot convert invalid value to {0}.".FormatWith(CultureInfo.InvariantCulture, objectType));
			}
		}
		if (this.AllowPreEpoch || result >= 0)
		{
			DateTime dateTime = UnixDateTimeConverter.UnixEpoch.AddSeconds(result);
			if ((flag ? Nullable.GetUnderlyingType(objectType) : objectType) == typeof(DateTimeOffset))
			{
				return new DateTimeOffset(dateTime, TimeSpan.Zero);
			}
			return dateTime;
		}
		throw JsonSerializationException.Create(reader, "Cannot convert value that is before Unix epoch of 00:00:00 UTC on 1 January 1970 to {0}.".FormatWith(CultureInfo.InvariantCulture, objectType));
	}
}
