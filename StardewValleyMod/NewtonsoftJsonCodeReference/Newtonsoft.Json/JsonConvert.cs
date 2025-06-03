using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Numerics;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json;

/// <summary>
/// Provides methods for converting between .NET types and JSON types.
/// </summary>
/// <example>
///   <code lang="cs" source="..\Src\Newtonsoft.Json.Tests\Documentation\SerializationTests.cs" region="SerializeObject" title="Serializing and Deserializing JSON with JsonConvert" />
/// </example>
public static class JsonConvert
{
	/// <summary>
	/// Represents JavaScript's boolean value <c>true</c> as a string. This field is read-only.
	/// </summary>
	public static readonly string True = "true";

	/// <summary>
	/// Represents JavaScript's boolean value <c>false</c> as a string. This field is read-only.
	/// </summary>
	public static readonly string False = "false";

	/// <summary>
	/// Represents JavaScript's <c>null</c> as a string. This field is read-only.
	/// </summary>
	public static readonly string Null = "null";

	/// <summary>
	/// Represents JavaScript's <c>undefined</c> as a string. This field is read-only.
	/// </summary>
	public static readonly string Undefined = "undefined";

	/// <summary>
	/// Represents JavaScript's positive infinity as a string. This field is read-only.
	/// </summary>
	public static readonly string PositiveInfinity = "Infinity";

	/// <summary>
	/// Represents JavaScript's negative infinity as a string. This field is read-only.
	/// </summary>
	public static readonly string NegativeInfinity = "-Infinity";

	/// <summary>
	/// Represents JavaScript's <c>NaN</c> as a string. This field is read-only.
	/// </summary>
	public static readonly string NaN = "NaN";

	/// <summary>
	/// Gets or sets a function that creates default <see cref="T:Newtonsoft.Json.JsonSerializerSettings" />.
	/// Default settings are automatically used by serialization methods on <see cref="T:Newtonsoft.Json.JsonConvert" />,
	/// and <see cref="M:Newtonsoft.Json.Linq.JToken.ToObject``1" /> and <see cref="M:Newtonsoft.Json.Linq.JToken.FromObject(System.Object)" /> on <see cref="T:Newtonsoft.Json.Linq.JToken" />.
	/// To serialize without using any default settings create a <see cref="T:Newtonsoft.Json.JsonSerializer" /> with
	/// <see cref="M:Newtonsoft.Json.JsonSerializer.Create" />.
	/// </summary>
	public static Func<JsonSerializerSettings>? DefaultSettings { get; set; }

	/// <summary>
	/// Converts the <see cref="T:System.DateTime" /> to its JSON string representation.
	/// </summary>
	/// <param name="value">The value to convert.</param>
	/// <returns>A JSON string representation of the <see cref="T:System.DateTime" />.</returns>
	public static string ToString(DateTime value)
	{
		return JsonConvert.ToString(value, DateFormatHandling.IsoDateFormat, DateTimeZoneHandling.RoundtripKind);
	}

	/// <summary>
	/// Converts the <see cref="T:System.DateTime" /> to its JSON string representation using the <see cref="T:Newtonsoft.Json.DateFormatHandling" /> specified.
	/// </summary>
	/// <param name="value">The value to convert.</param>
	/// <param name="format">The format the date will be converted to.</param>
	/// <param name="timeZoneHandling">The time zone handling when the date is converted to a string.</param>
	/// <returns>A JSON string representation of the <see cref="T:System.DateTime" />.</returns>
	public static string ToString(DateTime value, DateFormatHandling format, DateTimeZoneHandling timeZoneHandling)
	{
		DateTime value2 = DateTimeUtils.EnsureDateTime(value, timeZoneHandling);
		using StringWriter stringWriter = StringUtils.CreateStringWriter(64);
		stringWriter.Write('"');
		DateTimeUtils.WriteDateTimeString(stringWriter, value2, format, null, CultureInfo.InvariantCulture);
		stringWriter.Write('"');
		return stringWriter.ToString();
	}

	/// <summary>
	/// Converts the <see cref="T:System.DateTimeOffset" /> to its JSON string representation.
	/// </summary>
	/// <param name="value">The value to convert.</param>
	/// <returns>A JSON string representation of the <see cref="T:System.DateTimeOffset" />.</returns>
	public static string ToString(DateTimeOffset value)
	{
		return JsonConvert.ToString(value, DateFormatHandling.IsoDateFormat);
	}

	/// <summary>
	/// Converts the <see cref="T:System.DateTimeOffset" /> to its JSON string representation using the <see cref="T:Newtonsoft.Json.DateFormatHandling" /> specified.
	/// </summary>
	/// <param name="value">The value to convert.</param>
	/// <param name="format">The format the date will be converted to.</param>
	/// <returns>A JSON string representation of the <see cref="T:System.DateTimeOffset" />.</returns>
	public static string ToString(DateTimeOffset value, DateFormatHandling format)
	{
		using StringWriter stringWriter = StringUtils.CreateStringWriter(64);
		stringWriter.Write('"');
		DateTimeUtils.WriteDateTimeOffsetString(stringWriter, value, format, null, CultureInfo.InvariantCulture);
		stringWriter.Write('"');
		return stringWriter.ToString();
	}

	/// <summary>
	/// Converts the <see cref="T:System.Boolean" /> to its JSON string representation.
	/// </summary>
	/// <param name="value">The value to convert.</param>
	/// <returns>A JSON string representation of the <see cref="T:System.Boolean" />.</returns>
	public static string ToString(bool value)
	{
		if (!value)
		{
			return JsonConvert.False;
		}
		return JsonConvert.True;
	}

	/// <summary>
	/// Converts the <see cref="T:System.Char" /> to its JSON string representation.
	/// </summary>
	/// <param name="value">The value to convert.</param>
	/// <returns>A JSON string representation of the <see cref="T:System.Char" />.</returns>
	public static string ToString(char value)
	{
		return JsonConvert.ToString(char.ToString(value));
	}

	/// <summary>
	/// Converts the <see cref="T:System.Enum" /> to its JSON string representation.
	/// </summary>
	/// <param name="value">The value to convert.</param>
	/// <returns>A JSON string representation of the <see cref="T:System.Enum" />.</returns>
	public static string ToString(Enum value)
	{
		return value.ToString("D");
	}

	/// <summary>
	/// Converts the <see cref="T:System.Int32" /> to its JSON string representation.
	/// </summary>
	/// <param name="value">The value to convert.</param>
	/// <returns>A JSON string representation of the <see cref="T:System.Int32" />.</returns>
	public static string ToString(int value)
	{
		return value.ToString(null, CultureInfo.InvariantCulture);
	}

	/// <summary>
	/// Converts the <see cref="T:System.Int16" /> to its JSON string representation.
	/// </summary>
	/// <param name="value">The value to convert.</param>
	/// <returns>A JSON string representation of the <see cref="T:System.Int16" />.</returns>
	public static string ToString(short value)
	{
		return value.ToString(null, CultureInfo.InvariantCulture);
	}

	/// <summary>
	/// Converts the <see cref="T:System.UInt16" /> to its JSON string representation.
	/// </summary>
	/// <param name="value">The value to convert.</param>
	/// <returns>A JSON string representation of the <see cref="T:System.UInt16" />.</returns>
	[CLSCompliant(false)]
	public static string ToString(ushort value)
	{
		return value.ToString(null, CultureInfo.InvariantCulture);
	}

	/// <summary>
	/// Converts the <see cref="T:System.UInt32" /> to its JSON string representation.
	/// </summary>
	/// <param name="value">The value to convert.</param>
	/// <returns>A JSON string representation of the <see cref="T:System.UInt32" />.</returns>
	[CLSCompliant(false)]
	public static string ToString(uint value)
	{
		return value.ToString(null, CultureInfo.InvariantCulture);
	}

	/// <summary>
	/// Converts the <see cref="T:System.Int64" />  to its JSON string representation.
	/// </summary>
	/// <param name="value">The value to convert.</param>
	/// <returns>A JSON string representation of the <see cref="T:System.Int64" />.</returns>
	public static string ToString(long value)
	{
		return value.ToString(null, CultureInfo.InvariantCulture);
	}

	private static string ToStringInternal(BigInteger value)
	{
		return value.ToString(null, CultureInfo.InvariantCulture);
	}

	/// <summary>
	/// Converts the <see cref="T:System.UInt64" /> to its JSON string representation.
	/// </summary>
	/// <param name="value">The value to convert.</param>
	/// <returns>A JSON string representation of the <see cref="T:System.UInt64" />.</returns>
	[CLSCompliant(false)]
	public static string ToString(ulong value)
	{
		return value.ToString(null, CultureInfo.InvariantCulture);
	}

	/// <summary>
	/// Converts the <see cref="T:System.Single" /> to its JSON string representation.
	/// </summary>
	/// <param name="value">The value to convert.</param>
	/// <returns>A JSON string representation of the <see cref="T:System.Single" />.</returns>
	public static string ToString(float value)
	{
		return JsonConvert.EnsureDecimalPlace(value, value.ToString("R", CultureInfo.InvariantCulture));
	}

	internal static string ToString(float value, FloatFormatHandling floatFormatHandling, char quoteChar, bool nullable)
	{
		return JsonConvert.EnsureFloatFormat(value, JsonConvert.EnsureDecimalPlace(value, value.ToString("R", CultureInfo.InvariantCulture)), floatFormatHandling, quoteChar, nullable);
	}

	private static string EnsureFloatFormat(double value, string text, FloatFormatHandling floatFormatHandling, char quoteChar, bool nullable)
	{
		if (floatFormatHandling == FloatFormatHandling.Symbol || (!double.IsInfinity(value) && !double.IsNaN(value)))
		{
			return text;
		}
		if (floatFormatHandling == FloatFormatHandling.DefaultValue)
		{
			if (nullable)
			{
				return JsonConvert.Null;
			}
			return "0.0";
		}
		return quoteChar + text + quoteChar;
	}

	/// <summary>
	/// Converts the <see cref="T:System.Double" /> to its JSON string representation.
	/// </summary>
	/// <param name="value">The value to convert.</param>
	/// <returns>A JSON string representation of the <see cref="T:System.Double" />.</returns>
	public static string ToString(double value)
	{
		return JsonConvert.EnsureDecimalPlace(value, value.ToString("R", CultureInfo.InvariantCulture));
	}

	internal static string ToString(double value, FloatFormatHandling floatFormatHandling, char quoteChar, bool nullable)
	{
		return JsonConvert.EnsureFloatFormat(value, JsonConvert.EnsureDecimalPlace(value, value.ToString("R", CultureInfo.InvariantCulture)), floatFormatHandling, quoteChar, nullable);
	}

	private static string EnsureDecimalPlace(double value, string text)
	{
		if (double.IsNaN(value) || double.IsInfinity(value) || StringUtils.IndexOf(text, '.') != -1 || StringUtils.IndexOf(text, 'E') != -1 || StringUtils.IndexOf(text, 'e') != -1)
		{
			return text;
		}
		return text + ".0";
	}

	private static string EnsureDecimalPlace(string text)
	{
		if (StringUtils.IndexOf(text, '.') != -1)
		{
			return text;
		}
		return text + ".0";
	}

	/// <summary>
	/// Converts the <see cref="T:System.Byte" /> to its JSON string representation.
	/// </summary>
	/// <param name="value">The value to convert.</param>
	/// <returns>A JSON string representation of the <see cref="T:System.Byte" />.</returns>
	public static string ToString(byte value)
	{
		return value.ToString(null, CultureInfo.InvariantCulture);
	}

	/// <summary>
	/// Converts the <see cref="T:System.SByte" /> to its JSON string representation.
	/// </summary>
	/// <param name="value">The value to convert.</param>
	/// <returns>A JSON string representation of the <see cref="T:System.SByte" />.</returns>
	[CLSCompliant(false)]
	public static string ToString(sbyte value)
	{
		return value.ToString(null, CultureInfo.InvariantCulture);
	}

	/// <summary>
	/// Converts the <see cref="T:System.Decimal" /> to its JSON string representation.
	/// </summary>
	/// <param name="value">The value to convert.</param>
	/// <returns>A JSON string representation of the <see cref="T:System.Decimal" />.</returns>
	public static string ToString(decimal value)
	{
		return JsonConvert.EnsureDecimalPlace(value.ToString(null, CultureInfo.InvariantCulture));
	}

	/// <summary>
	/// Converts the <see cref="T:System.Guid" /> to its JSON string representation.
	/// </summary>
	/// <param name="value">The value to convert.</param>
	/// <returns>A JSON string representation of the <see cref="T:System.Guid" />.</returns>
	public static string ToString(Guid value)
	{
		return JsonConvert.ToString(value, '"');
	}

	internal static string ToString(Guid value, char quoteChar)
	{
		string text = value.ToString("D", CultureInfo.InvariantCulture);
		string text2 = quoteChar.ToString(CultureInfo.InvariantCulture);
		return text2 + text + text2;
	}

	/// <summary>
	/// Converts the <see cref="T:System.TimeSpan" /> to its JSON string representation.
	/// </summary>
	/// <param name="value">The value to convert.</param>
	/// <returns>A JSON string representation of the <see cref="T:System.TimeSpan" />.</returns>
	public static string ToString(TimeSpan value)
	{
		return JsonConvert.ToString(value, '"');
	}

	internal static string ToString(TimeSpan value, char quoteChar)
	{
		return JsonConvert.ToString(value.ToString(), quoteChar);
	}

	/// <summary>
	/// Converts the <see cref="T:System.Uri" /> to its JSON string representation.
	/// </summary>
	/// <param name="value">The value to convert.</param>
	/// <returns>A JSON string representation of the <see cref="T:System.Uri" />.</returns>
	public static string ToString(Uri? value)
	{
		if (value == null)
		{
			return JsonConvert.Null;
		}
		return JsonConvert.ToString(value, '"');
	}

	internal static string ToString(Uri value, char quoteChar)
	{
		return JsonConvert.ToString(value.OriginalString, quoteChar);
	}

	/// <summary>
	/// Converts the <see cref="T:System.String" /> to its JSON string representation.
	/// </summary>
	/// <param name="value">The value to convert.</param>
	/// <returns>A JSON string representation of the <see cref="T:System.String" />.</returns>
	public static string ToString(string? value)
	{
		return JsonConvert.ToString(value, '"');
	}

	/// <summary>
	/// Converts the <see cref="T:System.String" /> to its JSON string representation.
	/// </summary>
	/// <param name="value">The value to convert.</param>
	/// <param name="delimiter">The string delimiter character.</param>
	/// <returns>A JSON string representation of the <see cref="T:System.String" />.</returns>
	public static string ToString(string? value, char delimiter)
	{
		return JsonConvert.ToString(value, delimiter, StringEscapeHandling.Default);
	}

	/// <summary>
	/// Converts the <see cref="T:System.String" /> to its JSON string representation.
	/// </summary>
	/// <param name="value">The value to convert.</param>
	/// <param name="delimiter">The string delimiter character.</param>
	/// <param name="stringEscapeHandling">The string escape handling.</param>
	/// <returns>A JSON string representation of the <see cref="T:System.String" />.</returns>
	public static string ToString(string? value, char delimiter, StringEscapeHandling stringEscapeHandling)
	{
		if (delimiter != '"' && delimiter != '\'')
		{
			throw new ArgumentException("Delimiter must be a single or double quote.", "delimiter");
		}
		return JavaScriptUtils.ToEscapedJavaScriptString(value, delimiter, appendDelimiters: true, stringEscapeHandling);
	}

	/// <summary>
	/// Converts the <see cref="T:System.Object" /> to its JSON string representation.
	/// </summary>
	/// <param name="value">The value to convert.</param>
	/// <returns>A JSON string representation of the <see cref="T:System.Object" />.</returns>
	public static string ToString(object? value)
	{
		if (value == null)
		{
			return JsonConvert.Null;
		}
		return ConvertUtils.GetTypeCode(value.GetType()) switch
		{
			PrimitiveTypeCode.String => JsonConvert.ToString((string)value), 
			PrimitiveTypeCode.Char => JsonConvert.ToString((char)value), 
			PrimitiveTypeCode.Boolean => JsonConvert.ToString((bool)value), 
			PrimitiveTypeCode.SByte => JsonConvert.ToString((sbyte)value), 
			PrimitiveTypeCode.Int16 => JsonConvert.ToString((short)value), 
			PrimitiveTypeCode.UInt16 => JsonConvert.ToString((ushort)value), 
			PrimitiveTypeCode.Int32 => JsonConvert.ToString((int)value), 
			PrimitiveTypeCode.Byte => JsonConvert.ToString((byte)value), 
			PrimitiveTypeCode.UInt32 => JsonConvert.ToString((uint)value), 
			PrimitiveTypeCode.Int64 => JsonConvert.ToString((long)value), 
			PrimitiveTypeCode.UInt64 => JsonConvert.ToString((ulong)value), 
			PrimitiveTypeCode.Single => JsonConvert.ToString((float)value), 
			PrimitiveTypeCode.Double => JsonConvert.ToString((double)value), 
			PrimitiveTypeCode.DateTime => JsonConvert.ToString((DateTime)value), 
			PrimitiveTypeCode.Decimal => JsonConvert.ToString((decimal)value), 
			PrimitiveTypeCode.DBNull => JsonConvert.Null, 
			PrimitiveTypeCode.DateTimeOffset => JsonConvert.ToString((DateTimeOffset)value), 
			PrimitiveTypeCode.Guid => JsonConvert.ToString((Guid)value), 
			PrimitiveTypeCode.Uri => JsonConvert.ToString((Uri)value), 
			PrimitiveTypeCode.TimeSpan => JsonConvert.ToString((TimeSpan)value), 
			PrimitiveTypeCode.BigInteger => JsonConvert.ToStringInternal((BigInteger)value), 
			_ => throw new ArgumentException("Unsupported type: {0}. Use the JsonSerializer class to get the object's JSON representation.".FormatWith(CultureInfo.InvariantCulture, value.GetType())), 
		};
	}

	/// <summary>
	/// Serializes the specified object to a JSON string.
	/// </summary>
	/// <param name="value">The object to serialize.</param>
	/// <returns>A JSON string representation of the object.</returns>
	[DebuggerStepThrough]
	public static string SerializeObject(object? value)
	{
		return JsonConvert.SerializeObject(value, (Type?)null, (JsonSerializerSettings?)null);
	}

	/// <summary>
	/// Serializes the specified object to a JSON string using formatting.
	/// </summary>
	/// <param name="value">The object to serialize.</param>
	/// <param name="formatting">Indicates how the output should be formatted.</param>
	/// <returns>
	/// A JSON string representation of the object.
	/// </returns>
	[DebuggerStepThrough]
	public static string SerializeObject(object? value, Formatting formatting)
	{
		return JsonConvert.SerializeObject(value, formatting, (JsonSerializerSettings?)null);
	}

	/// <summary>
	/// Serializes the specified object to a JSON string using a collection of <see cref="T:Newtonsoft.Json.JsonConverter" />.
	/// </summary>
	/// <param name="value">The object to serialize.</param>
	/// <param name="converters">A collection of converters used while serializing.</param>
	/// <returns>A JSON string representation of the object.</returns>
	[DebuggerStepThrough]
	public static string SerializeObject(object? value, params JsonConverter[] converters)
	{
		JsonSerializerSettings settings = ((converters != null && converters.Length != 0) ? new JsonSerializerSettings
		{
			Converters = converters
		} : null);
		return JsonConvert.SerializeObject(value, null, settings);
	}

	/// <summary>
	/// Serializes the specified object to a JSON string using formatting and a collection of <see cref="T:Newtonsoft.Json.JsonConverter" />.
	/// </summary>
	/// <param name="value">The object to serialize.</param>
	/// <param name="formatting">Indicates how the output should be formatted.</param>
	/// <param name="converters">A collection of converters used while serializing.</param>
	/// <returns>A JSON string representation of the object.</returns>
	[DebuggerStepThrough]
	public static string SerializeObject(object? value, Formatting formatting, params JsonConverter[] converters)
	{
		JsonSerializerSettings settings = ((converters != null && converters.Length != 0) ? new JsonSerializerSettings
		{
			Converters = converters
		} : null);
		return JsonConvert.SerializeObject(value, null, formatting, settings);
	}

	/// <summary>
	/// Serializes the specified object to a JSON string using <see cref="T:Newtonsoft.Json.JsonSerializerSettings" />.
	/// </summary>
	/// <param name="value">The object to serialize.</param>
	/// <param name="settings">The <see cref="T:Newtonsoft.Json.JsonSerializerSettings" /> used to serialize the object.
	/// If this is <c>null</c>, default serialization settings will be used.</param>
	/// <returns>
	/// A JSON string representation of the object.
	/// </returns>
	[DebuggerStepThrough]
	public static string SerializeObject(object? value, JsonSerializerSettings? settings)
	{
		return JsonConvert.SerializeObject(value, null, settings);
	}

	/// <summary>
	/// Serializes the specified object to a JSON string using a type, formatting and <see cref="T:Newtonsoft.Json.JsonSerializerSettings" />.
	/// </summary>
	/// <param name="value">The object to serialize.</param>
	/// <param name="settings">The <see cref="T:Newtonsoft.Json.JsonSerializerSettings" /> used to serialize the object.
	/// If this is <c>null</c>, default serialization settings will be used.</param>
	/// <param name="type">
	/// The type of the value being serialized.
	/// This parameter is used when <see cref="P:Newtonsoft.Json.JsonSerializer.TypeNameHandling" /> is <see cref="F:Newtonsoft.Json.TypeNameHandling.Auto" /> to write out the type name if the type of the value does not match.
	/// Specifying the type is optional.
	/// </param>
	/// <returns>
	/// A JSON string representation of the object.
	/// </returns>
	[DebuggerStepThrough]
	public static string SerializeObject(object? value, Type? type, JsonSerializerSettings? settings)
	{
		JsonSerializer jsonSerializer = JsonSerializer.CreateDefault(settings);
		return JsonConvert.SerializeObjectInternal(value, type, jsonSerializer);
	}

	/// <summary>
	/// Serializes the specified object to a JSON string using formatting and <see cref="T:Newtonsoft.Json.JsonSerializerSettings" />.
	/// </summary>
	/// <param name="value">The object to serialize.</param>
	/// <param name="formatting">Indicates how the output should be formatted.</param>
	/// <param name="settings">The <see cref="T:Newtonsoft.Json.JsonSerializerSettings" /> used to serialize the object.
	/// If this is <c>null</c>, default serialization settings will be used.</param>
	/// <returns>
	/// A JSON string representation of the object.
	/// </returns>
	[DebuggerStepThrough]
	public static string SerializeObject(object? value, Formatting formatting, JsonSerializerSettings? settings)
	{
		return JsonConvert.SerializeObject(value, null, formatting, settings);
	}

	/// <summary>
	/// Serializes the specified object to a JSON string using a type, formatting and <see cref="T:Newtonsoft.Json.JsonSerializerSettings" />.
	/// </summary>
	/// <param name="value">The object to serialize.</param>
	/// <param name="formatting">Indicates how the output should be formatted.</param>
	/// <param name="settings">The <see cref="T:Newtonsoft.Json.JsonSerializerSettings" /> used to serialize the object.
	/// If this is <c>null</c>, default serialization settings will be used.</param>
	/// <param name="type">
	/// The type of the value being serialized.
	/// This parameter is used when <see cref="P:Newtonsoft.Json.JsonSerializer.TypeNameHandling" /> is <see cref="F:Newtonsoft.Json.TypeNameHandling.Auto" /> to write out the type name if the type of the value does not match.
	/// Specifying the type is optional.
	/// </param>
	/// <returns>
	/// A JSON string representation of the object.
	/// </returns>
	[DebuggerStepThrough]
	public static string SerializeObject(object? value, Type? type, Formatting formatting, JsonSerializerSettings? settings)
	{
		JsonSerializer jsonSerializer = JsonSerializer.CreateDefault(settings);
		jsonSerializer.Formatting = formatting;
		return JsonConvert.SerializeObjectInternal(value, type, jsonSerializer);
	}

	private static string SerializeObjectInternal(object? value, Type? type, JsonSerializer jsonSerializer)
	{
		StringWriter stringWriter = new StringWriter(new StringBuilder(256), CultureInfo.InvariantCulture);
		using (JsonTextWriter jsonTextWriter = new JsonTextWriter(stringWriter))
		{
			jsonTextWriter.Formatting = jsonSerializer.Formatting;
			jsonSerializer.Serialize(jsonTextWriter, value, type);
		}
		return stringWriter.ToString();
	}

	/// <summary>
	/// Deserializes the JSON to a .NET object.
	/// </summary>
	/// <param name="value">The JSON to deserialize.</param>
	/// <returns>The deserialized object from the JSON string.</returns>
	[DebuggerStepThrough]
	public static object? DeserializeObject(string value)
	{
		return JsonConvert.DeserializeObject(value, (Type?)null, (JsonSerializerSettings?)null);
	}

	/// <summary>
	/// Deserializes the JSON to a .NET object using <see cref="T:Newtonsoft.Json.JsonSerializerSettings" />.
	/// </summary>
	/// <param name="value">The JSON to deserialize.</param>
	/// <param name="settings">
	/// The <see cref="T:Newtonsoft.Json.JsonSerializerSettings" /> used to deserialize the object.
	/// If this is <c>null</c>, default serialization settings will be used.
	/// </param>
	/// <returns>The deserialized object from the JSON string.</returns>
	[DebuggerStepThrough]
	public static object? DeserializeObject(string value, JsonSerializerSettings settings)
	{
		return JsonConvert.DeserializeObject(value, null, settings);
	}

	/// <summary>
	/// Deserializes the JSON to the specified .NET type.
	/// </summary>
	/// <param name="value">The JSON to deserialize.</param>
	/// <param name="type">The <see cref="T:System.Type" /> of object being deserialized.</param>
	/// <returns>The deserialized object from the JSON string.</returns>
	[DebuggerStepThrough]
	public static object? DeserializeObject(string value, Type type)
	{
		return JsonConvert.DeserializeObject(value, type, (JsonSerializerSettings?)null);
	}

	/// <summary>
	/// Deserializes the JSON to the specified .NET type.
	/// </summary>
	/// <typeparam name="T">The type of the object to deserialize to.</typeparam>
	/// <param name="value">The JSON to deserialize.</param>
	/// <returns>The deserialized object from the JSON string.</returns>
	[DebuggerStepThrough]
	public static T? DeserializeObject<T>(string value)
	{
		return JsonConvert.DeserializeObject<T>(value, (JsonSerializerSettings?)null);
	}

	/// <summary>
	/// Deserializes the JSON to the given anonymous type.
	/// </summary>
	/// <typeparam name="T">
	/// The anonymous type to deserialize to. This can't be specified
	/// traditionally and must be inferred from the anonymous type passed
	/// as a parameter.
	/// </typeparam>
	/// <param name="value">The JSON to deserialize.</param>
	/// <param name="anonymousTypeObject">The anonymous type object.</param>
	/// <returns>The deserialized anonymous type from the JSON string.</returns>
	[DebuggerStepThrough]
	public static T? DeserializeAnonymousType<T>(string value, T anonymousTypeObject)
	{
		return JsonConvert.DeserializeObject<T>(value);
	}

	/// <summary>
	/// Deserializes the JSON to the given anonymous type using <see cref="T:Newtonsoft.Json.JsonSerializerSettings" />.
	/// </summary>
	/// <typeparam name="T">
	/// The anonymous type to deserialize to. This can't be specified
	/// traditionally and must be inferred from the anonymous type passed
	/// as a parameter.
	/// </typeparam>
	/// <param name="value">The JSON to deserialize.</param>
	/// <param name="anonymousTypeObject">The anonymous type object.</param>
	/// <param name="settings">
	/// The <see cref="T:Newtonsoft.Json.JsonSerializerSettings" /> used to deserialize the object.
	/// If this is <c>null</c>, default serialization settings will be used.
	/// </param>
	/// <returns>The deserialized anonymous type from the JSON string.</returns>
	[DebuggerStepThrough]
	public static T? DeserializeAnonymousType<T>(string value, T anonymousTypeObject, JsonSerializerSettings settings)
	{
		return JsonConvert.DeserializeObject<T>(value, settings);
	}

	/// <summary>
	/// Deserializes the JSON to the specified .NET type using a collection of <see cref="T:Newtonsoft.Json.JsonConverter" />.
	/// </summary>
	/// <typeparam name="T">The type of the object to deserialize to.</typeparam>
	/// <param name="value">The JSON to deserialize.</param>
	/// <param name="converters">Converters to use while deserializing.</param>
	/// <returns>The deserialized object from the JSON string.</returns>
	[DebuggerStepThrough]
	public static T? DeserializeObject<T>(string value, params JsonConverter[] converters)
	{
		return (T)JsonConvert.DeserializeObject(value, typeof(T), converters);
	}

	/// <summary>
	/// Deserializes the JSON to the specified .NET type using <see cref="T:Newtonsoft.Json.JsonSerializerSettings" />.
	/// </summary>
	/// <typeparam name="T">The type of the object to deserialize to.</typeparam>
	/// <param name="value">The object to deserialize.</param>
	/// <param name="settings">
	/// The <see cref="T:Newtonsoft.Json.JsonSerializerSettings" /> used to deserialize the object.
	/// If this is <c>null</c>, default serialization settings will be used.
	/// </param>
	/// <returns>The deserialized object from the JSON string.</returns>
	[DebuggerStepThrough]
	public static T? DeserializeObject<T>(string value, JsonSerializerSettings? settings)
	{
		return (T)JsonConvert.DeserializeObject(value, typeof(T), settings);
	}

	/// <summary>
	/// Deserializes the JSON to the specified .NET type using a collection of <see cref="T:Newtonsoft.Json.JsonConverter" />.
	/// </summary>
	/// <param name="value">The JSON to deserialize.</param>
	/// <param name="type">The type of the object to deserialize.</param>
	/// <param name="converters">Converters to use while deserializing.</param>
	/// <returns>The deserialized object from the JSON string.</returns>
	[DebuggerStepThrough]
	public static object? DeserializeObject(string value, Type type, params JsonConverter[] converters)
	{
		JsonSerializerSettings settings = ((converters != null && converters.Length != 0) ? new JsonSerializerSettings
		{
			Converters = converters
		} : null);
		return JsonConvert.DeserializeObject(value, type, settings);
	}

	/// <summary>
	/// Deserializes the JSON to the specified .NET type using <see cref="T:Newtonsoft.Json.JsonSerializerSettings" />.
	/// </summary>
	/// <param name="value">The JSON to deserialize.</param>
	/// <param name="type">The type of the object to deserialize to.</param>
	/// <param name="settings">
	/// The <see cref="T:Newtonsoft.Json.JsonSerializerSettings" /> used to deserialize the object.
	/// If this is <c>null</c>, default serialization settings will be used.
	/// </param>
	/// <returns>The deserialized object from the JSON string.</returns>
	public static object? DeserializeObject(string value, Type? type, JsonSerializerSettings? settings)
	{
		ValidationUtils.ArgumentNotNull(value, "value");
		JsonSerializer jsonSerializer = JsonSerializer.CreateDefault(settings);
		if (!jsonSerializer.IsCheckAdditionalContentSet())
		{
			jsonSerializer.CheckAdditionalContent = true;
		}
		using JsonTextReader reader = new JsonTextReader(new StringReader(value));
		return jsonSerializer.Deserialize(reader, type);
	}

	/// <summary>
	/// Populates the object with values from the JSON string.
	/// </summary>
	/// <param name="value">The JSON to populate values from.</param>
	/// <param name="target">The target object to populate values onto.</param>
	[DebuggerStepThrough]
	public static void PopulateObject(string value, object target)
	{
		JsonConvert.PopulateObject(value, target, null);
	}

	/// <summary>
	/// Populates the object with values from the JSON string using <see cref="T:Newtonsoft.Json.JsonSerializerSettings" />.
	/// </summary>
	/// <param name="value">The JSON to populate values from.</param>
	/// <param name="target">The target object to populate values onto.</param>
	/// <param name="settings">
	/// The <see cref="T:Newtonsoft.Json.JsonSerializerSettings" /> used to deserialize the object.
	/// If this is <c>null</c>, default serialization settings will be used.
	/// </param>
	public static void PopulateObject(string value, object target, JsonSerializerSettings? settings)
	{
		JsonSerializer jsonSerializer = JsonSerializer.CreateDefault(settings);
		using JsonReader jsonReader = new JsonTextReader(new StringReader(value));
		jsonSerializer.Populate(jsonReader, target);
		if (settings == null || !settings.CheckAdditionalContent)
		{
			return;
		}
		while (jsonReader.Read())
		{
			if (jsonReader.TokenType != JsonToken.Comment)
			{
				throw JsonSerializationException.Create(jsonReader, "Additional text found in JSON string after finishing deserializing object.");
			}
		}
	}

	/// <summary>
	/// Serializes the <see cref="T:System.Xml.XmlNode" /> to a JSON string.
	/// </summary>
	/// <param name="node">The node to serialize.</param>
	/// <returns>A JSON string of the <see cref="T:System.Xml.XmlNode" />.</returns>
	public static string SerializeXmlNode(XmlNode? node)
	{
		return JsonConvert.SerializeXmlNode(node, Formatting.None);
	}

	/// <summary>
	/// Serializes the <see cref="T:System.Xml.XmlNode" /> to a JSON string using formatting.
	/// </summary>
	/// <param name="node">The node to serialize.</param>
	/// <param name="formatting">Indicates how the output should be formatted.</param>
	/// <returns>A JSON string of the <see cref="T:System.Xml.XmlNode" />.</returns>
	public static string SerializeXmlNode(XmlNode? node, Formatting formatting)
	{
		XmlNodeConverter xmlNodeConverter = new XmlNodeConverter();
		return JsonConvert.SerializeObject(node, formatting, xmlNodeConverter);
	}

	/// <summary>
	/// Serializes the <see cref="T:System.Xml.XmlNode" /> to a JSON string using formatting and omits the root object if <paramref name="omitRootObject" /> is <c>true</c>.
	/// </summary>
	/// <param name="node">The node to serialize.</param>
	/// <param name="formatting">Indicates how the output should be formatted.</param>
	/// <param name="omitRootObject">Omits writing the root object.</param>
	/// <returns>A JSON string of the <see cref="T:System.Xml.XmlNode" />.</returns>
	public static string SerializeXmlNode(XmlNode? node, Formatting formatting, bool omitRootObject)
	{
		XmlNodeConverter xmlNodeConverter = new XmlNodeConverter
		{
			OmitRootObject = omitRootObject
		};
		return JsonConvert.SerializeObject(node, formatting, xmlNodeConverter);
	}

	/// <summary>
	/// Deserializes the <see cref="T:System.Xml.XmlNode" /> from a JSON string.
	/// </summary>
	/// <param name="value">The JSON string.</param>
	/// <returns>The deserialized <see cref="T:System.Xml.XmlNode" />.</returns>
	public static XmlDocument? DeserializeXmlNode(string value)
	{
		return JsonConvert.DeserializeXmlNode(value, null);
	}

	/// <summary>
	/// Deserializes the <see cref="T:System.Xml.XmlNode" /> from a JSON string nested in a root element specified by <paramref name="deserializeRootElementName" />.
	/// </summary>
	/// <param name="value">The JSON string.</param>
	/// <param name="deserializeRootElementName">The name of the root element to append when deserializing.</param>
	/// <returns>The deserialized <see cref="T:System.Xml.XmlNode" />.</returns>
	public static XmlDocument? DeserializeXmlNode(string value, string? deserializeRootElementName)
	{
		return JsonConvert.DeserializeXmlNode(value, deserializeRootElementName, writeArrayAttribute: false);
	}

	/// <summary>
	/// Deserializes the <see cref="T:System.Xml.XmlNode" /> from a JSON string nested in a root element specified by <paramref name="deserializeRootElementName" />
	/// and writes a Json.NET array attribute for collections.
	/// </summary>
	/// <param name="value">The JSON string.</param>
	/// <param name="deserializeRootElementName">The name of the root element to append when deserializing.</param>
	/// <param name="writeArrayAttribute">
	/// A value to indicate whether to write the Json.NET array attribute.
	/// This attribute helps preserve arrays when converting the written XML back to JSON.
	/// </param>
	/// <returns>The deserialized <see cref="T:System.Xml.XmlNode" />.</returns>
	public static XmlDocument? DeserializeXmlNode(string value, string? deserializeRootElementName, bool writeArrayAttribute)
	{
		return JsonConvert.DeserializeXmlNode(value, deserializeRootElementName, writeArrayAttribute, encodeSpecialCharacters: false);
	}

	/// <summary>
	/// Deserializes the <see cref="T:System.Xml.XmlNode" /> from a JSON string nested in a root element specified by <paramref name="deserializeRootElementName" />,
	/// writes a Json.NET array attribute for collections, and encodes special characters.
	/// </summary>
	/// <param name="value">The JSON string.</param>
	/// <param name="deserializeRootElementName">The name of the root element to append when deserializing.</param>
	/// <param name="writeArrayAttribute">
	/// A value to indicate whether to write the Json.NET array attribute.
	/// This attribute helps preserve arrays when converting the written XML back to JSON.
	/// </param>
	/// <param name="encodeSpecialCharacters">
	/// A value to indicate whether to encode special characters when converting JSON to XML.
	/// If <c>true</c>, special characters like ':', '@', '?', '#' and '$' in JSON property names aren't used to specify
	/// XML namespaces, attributes or processing directives. Instead special characters are encoded and written
	/// as part of the XML element name.
	/// </param>
	/// <returns>The deserialized <see cref="T:System.Xml.XmlNode" />.</returns>
	public static XmlDocument? DeserializeXmlNode(string value, string? deserializeRootElementName, bool writeArrayAttribute, bool encodeSpecialCharacters)
	{
		XmlNodeConverter xmlNodeConverter = new XmlNodeConverter();
		xmlNodeConverter.DeserializeRootElementName = deserializeRootElementName;
		xmlNodeConverter.WriteArrayAttribute = writeArrayAttribute;
		xmlNodeConverter.EncodeSpecialCharacters = encodeSpecialCharacters;
		return (XmlDocument)JsonConvert.DeserializeObject(value, typeof(XmlDocument), xmlNodeConverter);
	}

	/// <summary>
	/// Serializes the <see cref="T:System.Xml.Linq.XNode" /> to a JSON string.
	/// </summary>
	/// <param name="node">The node to convert to JSON.</param>
	/// <returns>A JSON string of the <see cref="T:System.Xml.Linq.XNode" />.</returns>
	public static string SerializeXNode(XObject? node)
	{
		return JsonConvert.SerializeXNode(node, Formatting.None);
	}

	/// <summary>
	/// Serializes the <see cref="T:System.Xml.Linq.XNode" /> to a JSON string using formatting.
	/// </summary>
	/// <param name="node">The node to convert to JSON.</param>
	/// <param name="formatting">Indicates how the output should be formatted.</param>
	/// <returns>A JSON string of the <see cref="T:System.Xml.Linq.XNode" />.</returns>
	public static string SerializeXNode(XObject? node, Formatting formatting)
	{
		return JsonConvert.SerializeXNode(node, formatting, omitRootObject: false);
	}

	/// <summary>
	/// Serializes the <see cref="T:System.Xml.Linq.XNode" /> to a JSON string using formatting and omits the root object if <paramref name="omitRootObject" /> is <c>true</c>.
	/// </summary>
	/// <param name="node">The node to serialize.</param>
	/// <param name="formatting">Indicates how the output should be formatted.</param>
	/// <param name="omitRootObject">Omits writing the root object.</param>
	/// <returns>A JSON string of the <see cref="T:System.Xml.Linq.XNode" />.</returns>
	public static string SerializeXNode(XObject? node, Formatting formatting, bool omitRootObject)
	{
		XmlNodeConverter xmlNodeConverter = new XmlNodeConverter
		{
			OmitRootObject = omitRootObject
		};
		return JsonConvert.SerializeObject(node, formatting, xmlNodeConverter);
	}

	/// <summary>
	/// Deserializes the <see cref="T:System.Xml.Linq.XNode" /> from a JSON string.
	/// </summary>
	/// <param name="value">The JSON string.</param>
	/// <returns>The deserialized <see cref="T:System.Xml.Linq.XNode" />.</returns>
	public static XDocument? DeserializeXNode(string value)
	{
		return JsonConvert.DeserializeXNode(value, null);
	}

	/// <summary>
	/// Deserializes the <see cref="T:System.Xml.Linq.XNode" /> from a JSON string nested in a root element specified by <paramref name="deserializeRootElementName" />.
	/// </summary>
	/// <param name="value">The JSON string.</param>
	/// <param name="deserializeRootElementName">The name of the root element to append when deserializing.</param>
	/// <returns>The deserialized <see cref="T:System.Xml.Linq.XNode" />.</returns>
	public static XDocument? DeserializeXNode(string value, string? deserializeRootElementName)
	{
		return JsonConvert.DeserializeXNode(value, deserializeRootElementName, writeArrayAttribute: false);
	}

	/// <summary>
	/// Deserializes the <see cref="T:System.Xml.Linq.XNode" /> from a JSON string nested in a root element specified by <paramref name="deserializeRootElementName" />
	/// and writes a Json.NET array attribute for collections.
	/// </summary>
	/// <param name="value">The JSON string.</param>
	/// <param name="deserializeRootElementName">The name of the root element to append when deserializing.</param>
	/// <param name="writeArrayAttribute">
	/// A value to indicate whether to write the Json.NET array attribute.
	/// This attribute helps preserve arrays when converting the written XML back to JSON.
	/// </param>
	/// <returns>The deserialized <see cref="T:System.Xml.Linq.XNode" />.</returns>
	public static XDocument? DeserializeXNode(string value, string? deserializeRootElementName, bool writeArrayAttribute)
	{
		return JsonConvert.DeserializeXNode(value, deserializeRootElementName, writeArrayAttribute, encodeSpecialCharacters: false);
	}

	/// <summary>
	/// Deserializes the <see cref="T:System.Xml.Linq.XNode" /> from a JSON string nested in a root element specified by <paramref name="deserializeRootElementName" />,
	/// writes a Json.NET array attribute for collections, and encodes special characters.
	/// </summary>
	/// <param name="value">The JSON string.</param>
	/// <param name="deserializeRootElementName">The name of the root element to append when deserializing.</param>
	/// <param name="writeArrayAttribute">
	/// A value to indicate whether to write the Json.NET array attribute.
	/// This attribute helps preserve arrays when converting the written XML back to JSON.
	/// </param>
	/// <param name="encodeSpecialCharacters">
	/// A value to indicate whether to encode special characters when converting JSON to XML.
	/// If <c>true</c>, special characters like ':', '@', '?', '#' and '$' in JSON property names aren't used to specify
	/// XML namespaces, attributes or processing directives. Instead special characters are encoded and written
	/// as part of the XML element name.
	/// </param>
	/// <returns>The deserialized <see cref="T:System.Xml.Linq.XNode" />.</returns>
	public static XDocument? DeserializeXNode(string value, string? deserializeRootElementName, bool writeArrayAttribute, bool encodeSpecialCharacters)
	{
		XmlNodeConverter xmlNodeConverter = new XmlNodeConverter();
		xmlNodeConverter.DeserializeRootElementName = deserializeRootElementName;
		xmlNodeConverter.WriteArrayAttribute = writeArrayAttribute;
		xmlNodeConverter.EncodeSpecialCharacters = encodeSpecialCharacters;
		return (XDocument)JsonConvert.DeserializeObject(value, typeof(XDocument), xmlNodeConverter);
	}
}
