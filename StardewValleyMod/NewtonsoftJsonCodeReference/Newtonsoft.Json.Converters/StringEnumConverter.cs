using System;
using System.Globalization;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Converters;

/// <summary>
/// Converts an <see cref="T:System.Enum" /> to and from its name string value.
/// </summary>
public class StringEnumConverter : JsonConverter
{
	/// <summary>
	/// Gets or sets a value indicating whether the written enum text should be camel case.
	/// The default value is <c>false</c>.
	/// </summary>
	/// <value><c>true</c> if the written enum text will be camel case; otherwise, <c>false</c>.</value>
	[Obsolete("StringEnumConverter.CamelCaseText is obsolete. Set StringEnumConverter.NamingStrategy with CamelCaseNamingStrategy instead.")]
	public bool CamelCaseText
	{
		get
		{
			if (!(this.NamingStrategy is CamelCaseNamingStrategy))
			{
				return false;
			}
			return true;
		}
		set
		{
			if (value)
			{
				if (!(this.NamingStrategy is CamelCaseNamingStrategy))
				{
					this.NamingStrategy = new CamelCaseNamingStrategy();
				}
			}
			else if (this.NamingStrategy is CamelCaseNamingStrategy)
			{
				this.NamingStrategy = null;
			}
		}
	}

	/// <summary>
	/// Gets or sets the naming strategy used to resolve how enum text is written.
	/// </summary>
	/// <value>The naming strategy used to resolve how enum text is written.</value>
	public NamingStrategy? NamingStrategy { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether integer values are allowed when serializing and deserializing.
	/// The default value is <c>true</c>.
	/// </summary>
	/// <value><c>true</c> if integers are allowed when serializing and deserializing; otherwise, <c>false</c>.</value>
	public bool AllowIntegerValues { get; set; } = true;

	/// <summary>
	/// Initializes a new instance of the <see cref="T:Newtonsoft.Json.Converters.StringEnumConverter" /> class.
	/// </summary>
	public StringEnumConverter()
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:Newtonsoft.Json.Converters.StringEnumConverter" /> class.
	/// </summary>
	/// <param name="camelCaseText"><c>true</c> if the written enum text will be camel case; otherwise, <c>false</c>.</param>
	[Obsolete("StringEnumConverter(bool) is obsolete. Create a converter with StringEnumConverter(NamingStrategy, bool) instead.")]
	public StringEnumConverter(bool camelCaseText)
	{
		if (camelCaseText)
		{
			this.NamingStrategy = new CamelCaseNamingStrategy();
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:Newtonsoft.Json.Converters.StringEnumConverter" /> class.
	/// </summary>
	/// <param name="namingStrategy">The naming strategy used to resolve how enum text is written.</param>
	/// <param name="allowIntegerValues"><c>true</c> if integers are allowed when serializing and deserializing; otherwise, <c>false</c>.</param>
	public StringEnumConverter(NamingStrategy namingStrategy, bool allowIntegerValues = true)
	{
		this.NamingStrategy = namingStrategy;
		this.AllowIntegerValues = allowIntegerValues;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:Newtonsoft.Json.Converters.StringEnumConverter" /> class.
	/// </summary>
	/// <param name="namingStrategyType">The <see cref="T:System.Type" /> of the <see cref="T:Newtonsoft.Json.Serialization.NamingStrategy" /> used to write enum text.</param>
	public StringEnumConverter(Type namingStrategyType)
	{
		ValidationUtils.ArgumentNotNull(namingStrategyType, "namingStrategyType");
		this.NamingStrategy = JsonTypeReflector.CreateNamingStrategyInstance(namingStrategyType, null);
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:Newtonsoft.Json.Converters.StringEnumConverter" /> class.
	/// </summary>
	/// <param name="namingStrategyType">The <see cref="T:System.Type" /> of the <see cref="T:Newtonsoft.Json.Serialization.NamingStrategy" /> used to write enum text.</param>
	/// <param name="namingStrategyParameters">
	/// The parameter list to use when constructing the <see cref="T:Newtonsoft.Json.Serialization.NamingStrategy" /> described by <paramref name="namingStrategyType" />.
	/// If <c>null</c>, the default constructor is used.
	/// When non-<c>null</c>, there must be a constructor defined in the <see cref="T:Newtonsoft.Json.Serialization.NamingStrategy" /> that exactly matches the number,
	/// order, and type of these parameters.
	/// </param>
	public StringEnumConverter(Type namingStrategyType, object[] namingStrategyParameters)
	{
		ValidationUtils.ArgumentNotNull(namingStrategyType, "namingStrategyType");
		this.NamingStrategy = JsonTypeReflector.CreateNamingStrategyInstance(namingStrategyType, namingStrategyParameters);
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:Newtonsoft.Json.Converters.StringEnumConverter" /> class.
	/// </summary>
	/// <param name="namingStrategyType">The <see cref="T:System.Type" /> of the <see cref="T:Newtonsoft.Json.Serialization.NamingStrategy" /> used to write enum text.</param>
	/// <param name="namingStrategyParameters">
	/// The parameter list to use when constructing the <see cref="T:Newtonsoft.Json.Serialization.NamingStrategy" /> described by <paramref name="namingStrategyType" />.
	/// If <c>null</c>, the default constructor is used.
	/// When non-<c>null</c>, there must be a constructor defined in the <see cref="T:Newtonsoft.Json.Serialization.NamingStrategy" /> that exactly matches the number,
	/// order, and type of these parameters.
	/// </param>
	/// <param name="allowIntegerValues"><c>true</c> if integers are allowed when serializing and deserializing; otherwise, <c>false</c>.</param>
	public StringEnumConverter(Type namingStrategyType, object[] namingStrategyParameters, bool allowIntegerValues)
	{
		ValidationUtils.ArgumentNotNull(namingStrategyType, "namingStrategyType");
		this.NamingStrategy = JsonTypeReflector.CreateNamingStrategyInstance(namingStrategyType, namingStrategyParameters);
		this.AllowIntegerValues = allowIntegerValues;
	}

	/// <summary>
	/// Writes the JSON representation of the object.
	/// </summary>
	/// <param name="writer">The <see cref="T:Newtonsoft.Json.JsonWriter" /> to write to.</param>
	/// <param name="value">The value.</param>
	/// <param name="serializer">The calling serializer.</param>
	public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		Enum obj = (Enum)value;
		if (!EnumUtils.TryToString(obj.GetType(), value, this.NamingStrategy, out string name))
		{
			if (!this.AllowIntegerValues)
			{
				throw JsonSerializationException.Create(null, writer.ContainerPath, "Integer value {0} is not allowed.".FormatWith(CultureInfo.InvariantCulture, obj.ToString("D")), null);
			}
			writer.WriteValue(value);
		}
		else
		{
			writer.WriteValue(name);
		}
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
		if (reader.TokenType == JsonToken.Null)
		{
			if (!ReflectionUtils.IsNullableType(objectType))
			{
				throw JsonSerializationException.Create(reader, "Cannot convert null value to {0}.".FormatWith(CultureInfo.InvariantCulture, objectType));
			}
			return null;
		}
		bool flag = ReflectionUtils.IsNullableType(objectType);
		Type type = (flag ? Nullable.GetUnderlyingType(objectType) : objectType);
		try
		{
			if (reader.TokenType == JsonToken.String)
			{
				string value = reader.Value?.ToString();
				if (StringUtils.IsNullOrEmpty(value) && flag)
				{
					return null;
				}
				return EnumUtils.ParseEnum(type, this.NamingStrategy, value, !this.AllowIntegerValues);
			}
			if (reader.TokenType == JsonToken.Integer)
			{
				if (!this.AllowIntegerValues)
				{
					throw JsonSerializationException.Create(reader, "Integer value {0} is not allowed.".FormatWith(CultureInfo.InvariantCulture, reader.Value));
				}
				return ConvertUtils.ConvertOrCast(reader.Value, CultureInfo.InvariantCulture, type);
			}
		}
		catch (Exception ex)
		{
			throw JsonSerializationException.Create(reader, "Error converting value {0} to type '{1}'.".FormatWith(CultureInfo.InvariantCulture, MiscellaneousUtils.ToString(reader.Value), objectType), ex);
		}
		throw JsonSerializationException.Create(reader, "Unexpected token {0} when parsing enum.".FormatWith(CultureInfo.InvariantCulture, reader.TokenType));
	}

	/// <summary>
	/// Determines whether this instance can convert the specified object type.
	/// </summary>
	/// <param name="objectType">Type of the object.</param>
	/// <returns>
	/// <c>true</c> if this instance can convert the specified object type; otherwise, <c>false</c>.
	/// </returns>
	public override bool CanConvert(Type objectType)
	{
		return (ReflectionUtils.IsNullableType(objectType) ? Nullable.GetUnderlyingType(objectType) : objectType).IsEnum();
	}
}
