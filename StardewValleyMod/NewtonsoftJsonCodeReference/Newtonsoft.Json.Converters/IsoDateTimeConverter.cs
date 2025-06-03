using System;
using System.Globalization;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Converters;

/// <summary>
/// Converts a <see cref="T:System.DateTime" /> to and from the ISO 8601 date format (e.g. <c>"2008-04-12T12:53Z"</c>).
/// </summary>
public class IsoDateTimeConverter : DateTimeConverterBase
{
	private const string DefaultDateTimeFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss.FFFFFFFK";

	private DateTimeStyles _dateTimeStyles = DateTimeStyles.RoundtripKind;

	private string? _dateTimeFormat;

	private CultureInfo? _culture;

	/// <summary>
	/// Gets or sets the date time styles used when converting a date to and from JSON.
	/// </summary>
	/// <value>The date time styles used when converting a date to and from JSON.</value>
	public DateTimeStyles DateTimeStyles
	{
		get
		{
			return this._dateTimeStyles;
		}
		set
		{
			this._dateTimeStyles = value;
		}
	}

	/// <summary>
	/// Gets or sets the date time format used when converting a date to and from JSON.
	/// </summary>
	/// <value>The date time format used when converting a date to and from JSON.</value>
	public string? DateTimeFormat
	{
		get
		{
			return this._dateTimeFormat ?? string.Empty;
		}
		set
		{
			this._dateTimeFormat = (StringUtils.IsNullOrEmpty(value) ? null : value);
		}
	}

	/// <summary>
	/// Gets or sets the culture used when converting a date to and from JSON.
	/// </summary>
	/// <value>The culture used when converting a date to and from JSON.</value>
	public CultureInfo Culture
	{
		get
		{
			return this._culture ?? CultureInfo.CurrentCulture;
		}
		set
		{
			this._culture = value;
		}
	}

	/// <summary>
	/// Writes the JSON representation of the object.
	/// </summary>
	/// <param name="writer">The <see cref="T:Newtonsoft.Json.JsonWriter" /> to write to.</param>
	/// <param name="value">The value.</param>
	/// <param name="serializer">The calling serializer.</param>
	public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
	{
		string value2;
		if (value is DateTime dateTime)
		{
			if ((this._dateTimeStyles & DateTimeStyles.AdjustToUniversal) == DateTimeStyles.AdjustToUniversal || (this._dateTimeStyles & DateTimeStyles.AssumeUniversal) == DateTimeStyles.AssumeUniversal)
			{
				dateTime = dateTime.ToUniversalTime();
			}
			value2 = dateTime.ToString(this._dateTimeFormat ?? "yyyy'-'MM'-'dd'T'HH':'mm':'ss.FFFFFFFK", this.Culture);
		}
		else
		{
			if (!(value is DateTimeOffset dateTimeOffset))
			{
				throw new JsonSerializationException("Unexpected value when converting date. Expected DateTime or DateTimeOffset, got {0}.".FormatWith(CultureInfo.InvariantCulture, ReflectionUtils.GetObjectType(value)));
			}
			if ((this._dateTimeStyles & DateTimeStyles.AdjustToUniversal) == DateTimeStyles.AdjustToUniversal || (this._dateTimeStyles & DateTimeStyles.AssumeUniversal) == DateTimeStyles.AssumeUniversal)
			{
				dateTimeOffset = dateTimeOffset.ToUniversalTime();
			}
			value2 = dateTimeOffset.ToString(this._dateTimeFormat ?? "yyyy'-'MM'-'dd'T'HH':'mm':'ss.FFFFFFFK", this.Culture);
		}
		writer.WriteValue(value2);
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
		bool flag = ReflectionUtils.IsNullableType(objectType);
		if (reader.TokenType == JsonToken.Null)
		{
			if (!flag)
			{
				throw JsonSerializationException.Create(reader, "Cannot convert null value to {0}.".FormatWith(CultureInfo.InvariantCulture, objectType));
			}
			return null;
		}
		Type type = (flag ? Nullable.GetUnderlyingType(objectType) : objectType);
		if (reader.TokenType == JsonToken.Date)
		{
			if (type == typeof(DateTimeOffset))
			{
				if (!(reader.Value is DateTimeOffset))
				{
					return new DateTimeOffset((DateTime)reader.Value);
				}
				return reader.Value;
			}
			if (reader.Value is DateTimeOffset dateTimeOffset)
			{
				return dateTimeOffset.DateTime;
			}
			return reader.Value;
		}
		if (reader.TokenType != JsonToken.String)
		{
			throw JsonSerializationException.Create(reader, "Unexpected token parsing date. Expected String, got {0}.".FormatWith(CultureInfo.InvariantCulture, reader.TokenType));
		}
		string text = reader.Value?.ToString();
		if (StringUtils.IsNullOrEmpty(text) && flag)
		{
			return null;
		}
		if (type == typeof(DateTimeOffset))
		{
			if (!StringUtils.IsNullOrEmpty(this._dateTimeFormat))
			{
				return DateTimeOffset.ParseExact(text, this._dateTimeFormat, this.Culture, this._dateTimeStyles);
			}
			return DateTimeOffset.Parse(text, this.Culture, this._dateTimeStyles);
		}
		if (!StringUtils.IsNullOrEmpty(this._dateTimeFormat))
		{
			return DateTime.ParseExact(text, this._dateTimeFormat, this.Culture, this._dateTimeStyles);
		}
		return DateTime.Parse(text, this.Culture, this._dateTimeStyles);
	}
}
