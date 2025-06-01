using System;
using System.Data;
using Newtonsoft.Json.Serialization;

namespace Newtonsoft.Json.Converters;

/// <summary>
/// Converts a <see cref="T:System.Data.DataSet" /> to and from JSON.
/// </summary>
public class DataSetConverter : JsonConverter
{
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
		DataSet obj = (DataSet)value;
		DefaultContractResolver defaultContractResolver = serializer.ContractResolver as DefaultContractResolver;
		DataTableConverter dataTableConverter = new DataTableConverter();
		writer.WriteStartObject();
		foreach (DataTable table in obj.Tables)
		{
			writer.WritePropertyName((defaultContractResolver != null) ? defaultContractResolver.GetResolvedPropertyName(table.TableName) : table.TableName);
			dataTableConverter.WriteJson(writer, table, serializer);
		}
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
		if (reader.TokenType == JsonToken.Null)
		{
			return null;
		}
		DataSet dataSet = ((objectType == typeof(DataSet)) ? new DataSet() : ((DataSet)Activator.CreateInstance(objectType)));
		DataTableConverter dataTableConverter = new DataTableConverter();
		reader.ReadAndAssert();
		while (reader.TokenType == JsonToken.PropertyName)
		{
			DataTable dataTable = dataSet.Tables[(string)reader.Value];
			bool num = dataTable != null;
			dataTable = (DataTable)dataTableConverter.ReadJson(reader, typeof(DataTable), dataTable, serializer);
			if (!num)
			{
				dataSet.Tables.Add(dataTable);
			}
			reader.ReadAndAssert();
		}
		return dataSet;
	}

	/// <summary>
	/// Determines whether this instance can convert the specified value type.
	/// </summary>
	/// <param name="valueType">Type of the value.</param>
	/// <returns>
	/// 	<c>true</c> if this instance can convert the specified value type; otherwise, <c>false</c>.
	/// </returns>
	public override bool CanConvert(Type valueType)
	{
		return typeof(DataSet).IsAssignableFrom(valueType);
	}
}
