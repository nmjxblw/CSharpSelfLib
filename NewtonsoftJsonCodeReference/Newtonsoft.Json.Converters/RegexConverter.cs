using System;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Converters;

/// <summary>
/// Converts a <see cref="T:System.Text.RegularExpressions.Regex" /> to and from JSON and BSON.
/// </summary>
public class RegexConverter : JsonConverter
{
	private const string PatternName = "Pattern";

	private const string OptionsName = "Options";

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
		Regex regex = (Regex)value;
		if (writer is BsonWriter writer2)
		{
			this.WriteBson(writer2, regex);
		}
		else
		{
			this.WriteJson(writer, regex, serializer);
		}
	}

	private bool HasFlag(RegexOptions options, RegexOptions flag)
	{
		return (options & flag) == flag;
	}

	private void WriteBson(BsonWriter writer, Regex regex)
	{
		string text = null;
		if (this.HasFlag(regex.Options, RegexOptions.IgnoreCase))
		{
			text += "i";
		}
		if (this.HasFlag(regex.Options, RegexOptions.Multiline))
		{
			text += "m";
		}
		if (this.HasFlag(regex.Options, RegexOptions.Singleline))
		{
			text += "s";
		}
		text += "u";
		if (this.HasFlag(regex.Options, RegexOptions.ExplicitCapture))
		{
			text += "x";
		}
		writer.WriteRegex(regex.ToString(), text);
	}

	private void WriteJson(JsonWriter writer, Regex regex, JsonSerializer serializer)
	{
		DefaultContractResolver defaultContractResolver = serializer.ContractResolver as DefaultContractResolver;
		writer.WriteStartObject();
		writer.WritePropertyName((defaultContractResolver != null) ? defaultContractResolver.GetResolvedPropertyName("Pattern") : "Pattern");
		writer.WriteValue(regex.ToString());
		writer.WritePropertyName((defaultContractResolver != null) ? defaultContractResolver.GetResolvedPropertyName("Options") : "Options");
		serializer.Serialize(writer, regex.Options);
		writer.WriteEndObject();
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
		return reader.TokenType switch
		{
			JsonToken.StartObject => this.ReadRegexObject(reader, serializer), 
			JsonToken.String => this.ReadRegexString(reader), 
			JsonToken.Null => null, 
			_ => throw JsonSerializationException.Create(reader, "Unexpected token when reading Regex."), 
		};
	}

	private object ReadRegexString(JsonReader reader)
	{
		string text = (string)reader.Value;
		if (text.Length > 0 && text[0] == '/')
		{
			int num = text.LastIndexOf('/');
			if (num > 0)
			{
				string pattern = text.Substring(1, num - 1);
				RegexOptions regexOptions = MiscellaneousUtils.GetRegexOptions(text.Substring(num + 1));
				return new Regex(pattern, regexOptions);
			}
		}
		throw JsonSerializationException.Create(reader, "Regex pattern must be enclosed by slashes.");
	}

	private Regex ReadRegexObject(JsonReader reader, JsonSerializer serializer)
	{
		string text = null;
		RegexOptions? regexOptions = null;
		while (reader.Read())
		{
			switch (reader.TokenType)
			{
			case JsonToken.PropertyName:
			{
				string a = reader.Value.ToString();
				if (!reader.Read())
				{
					throw JsonSerializationException.Create(reader, "Unexpected end when reading Regex.");
				}
				if (string.Equals(a, "Pattern", StringComparison.OrdinalIgnoreCase))
				{
					text = (string)reader.Value;
				}
				else if (string.Equals(a, "Options", StringComparison.OrdinalIgnoreCase))
				{
					regexOptions = serializer.Deserialize<RegexOptions>(reader);
				}
				else
				{
					reader.Skip();
				}
				break;
			}
			case JsonToken.EndObject:
				if (text == null)
				{
					throw JsonSerializationException.Create(reader, "Error deserializing Regex. No pattern found.");
				}
				return new Regex(text, regexOptions.GetValueOrDefault());
			}
		}
		throw JsonSerializationException.Create(reader, "Unexpected end when reading Regex.");
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
		if (objectType.Name == "Regex")
		{
			return this.IsRegex(objectType);
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private bool IsRegex(Type objectType)
	{
		return objectType == typeof(Regex);
	}
}
