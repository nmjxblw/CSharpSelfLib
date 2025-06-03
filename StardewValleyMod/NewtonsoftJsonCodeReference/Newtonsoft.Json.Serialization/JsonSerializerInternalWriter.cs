using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Security;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Serialization;

internal class JsonSerializerInternalWriter : JsonSerializerInternalBase
{
	private Type? _rootType;

	private int _rootLevel;

	private readonly List<object> _serializeStack = new List<object>();

	public JsonSerializerInternalWriter(JsonSerializer serializer)
		: base(serializer)
	{
	}

	public void Serialize(JsonWriter jsonWriter, object? value, Type? objectType)
	{
		if (jsonWriter == null)
		{
			throw new ArgumentNullException("jsonWriter");
		}
		this._rootType = objectType;
		this._rootLevel = this._serializeStack.Count + 1;
		JsonContract contractSafe = this.GetContractSafe(value);
		try
		{
			if (this.ShouldWriteReference(value, null, contractSafe, null, null))
			{
				this.WriteReference(jsonWriter, value);
			}
			else
			{
				this.SerializeValue(jsonWriter, value, contractSafe, null, null, null);
			}
		}
		catch (Exception ex)
		{
			if (base.IsErrorHandled(null, contractSafe, null, null, jsonWriter.Path, ex))
			{
				this.HandleError(jsonWriter, 0);
				return;
			}
			base.ClearErrorContext();
			throw;
		}
		finally
		{
			this._rootType = null;
		}
	}

	private JsonSerializerProxy GetInternalSerializer()
	{
		if (base.InternalSerializer == null)
		{
			base.InternalSerializer = new JsonSerializerProxy(this);
		}
		return base.InternalSerializer;
	}

	private JsonContract? GetContractSafe(object? value)
	{
		if (value == null)
		{
			return null;
		}
		return this.GetContract(value);
	}

	private JsonContract GetContract(object value)
	{
		return base.Serializer._contractResolver.ResolveContract(value.GetType());
	}

	private void SerializePrimitive(JsonWriter writer, object value, JsonPrimitiveContract contract, JsonProperty? member, JsonContainerContract? containerContract, JsonProperty? containerProperty)
	{
		if (contract.TypeCode == PrimitiveTypeCode.Bytes && this.ShouldWriteType(TypeNameHandling.Objects, contract, member, containerContract, containerProperty))
		{
			writer.WriteStartObject();
			this.WriteTypeProperty(writer, contract.CreatedType);
			writer.WritePropertyName("$value", escape: false);
			JsonWriter.WriteValue(writer, contract.TypeCode, value);
			writer.WriteEndObject();
		}
		else
		{
			JsonWriter.WriteValue(writer, contract.TypeCode, value);
		}
	}

	private void SerializeValue(JsonWriter writer, object? value, JsonContract? valueContract, JsonProperty? member, JsonContainerContract? containerContract, JsonProperty? containerProperty)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		JsonConverter jsonConverter = member?.Converter ?? containerProperty?.ItemConverter ?? containerContract?.ItemConverter ?? valueContract.Converter ?? base.Serializer.GetMatchingConverter(valueContract.UnderlyingType) ?? valueContract.InternalConverter;
		if (jsonConverter != null && jsonConverter.CanWrite)
		{
			this.SerializeConvertable(writer, jsonConverter, value, valueContract, containerContract, containerProperty);
			return;
		}
		switch (valueContract.ContractType)
		{
		case JsonContractType.Object:
			this.SerializeObject(writer, value, (JsonObjectContract)valueContract, member, containerContract, containerProperty);
			break;
		case JsonContractType.Array:
		{
			JsonArrayContract jsonArrayContract = (JsonArrayContract)valueContract;
			if (!jsonArrayContract.IsMultidimensionalArray)
			{
				this.SerializeList(writer, (IEnumerable)value, jsonArrayContract, member, containerContract, containerProperty);
			}
			else
			{
				this.SerializeMultidimensionalArray(writer, (Array)value, jsonArrayContract, member, containerContract, containerProperty);
			}
			break;
		}
		case JsonContractType.Primitive:
			this.SerializePrimitive(writer, value, (JsonPrimitiveContract)valueContract, member, containerContract, containerProperty);
			break;
		case JsonContractType.String:
			this.SerializeString(writer, value, (JsonStringContract)valueContract);
			break;
		case JsonContractType.Dictionary:
		{
			JsonDictionaryContract jsonDictionaryContract = (JsonDictionaryContract)valueContract;
			IDictionary values;
			if (!(value is IDictionary dictionary))
			{
				IDictionary dictionary2 = jsonDictionaryContract.CreateWrapper(value);
				values = dictionary2;
			}
			else
			{
				values = dictionary;
			}
			this.SerializeDictionary(writer, values, jsonDictionaryContract, member, containerContract, containerProperty);
			break;
		}
		case JsonContractType.Dynamic:
			this.SerializeDynamic(writer, (IDynamicMetaObjectProvider)value, (JsonDynamicContract)valueContract, member, containerContract, containerProperty);
			break;
		case JsonContractType.Serializable:
			this.SerializeISerializable(writer, (ISerializable)value, (JsonISerializableContract)valueContract, member, containerContract, containerProperty);
			break;
		case JsonContractType.Linq:
			((JToken)value).WriteTo(writer, base.Serializer.Converters.ToArray());
			break;
		}
	}

	private bool? ResolveIsReference(JsonContract contract, JsonProperty? property, JsonContainerContract? collectionContract, JsonProperty? containerProperty)
	{
		bool? result = null;
		if (property != null)
		{
			result = property.IsReference;
		}
		if (!result.HasValue && containerProperty != null)
		{
			result = containerProperty.ItemIsReference;
		}
		if (!result.HasValue && collectionContract != null)
		{
			result = collectionContract.ItemIsReference;
		}
		if (!result.HasValue)
		{
			result = contract.IsReference;
		}
		return result;
	}

	private bool ShouldWriteReference(object? value, JsonProperty? property, JsonContract? valueContract, JsonContainerContract? collectionContract, JsonProperty? containerProperty)
	{
		if (value == null)
		{
			return false;
		}
		if (valueContract.ContractType == JsonContractType.Primitive || valueContract.ContractType == JsonContractType.String)
		{
			return false;
		}
		bool? flag = this.ResolveIsReference(valueContract, property, collectionContract, containerProperty);
		if (!flag.HasValue)
		{
			flag = ((valueContract.ContractType != JsonContractType.Array) ? new bool?(this.HasFlag(base.Serializer._preserveReferencesHandling, PreserveReferencesHandling.Objects)) : new bool?(this.HasFlag(base.Serializer._preserveReferencesHandling, PreserveReferencesHandling.Arrays)));
		}
		if (flag != true)
		{
			return false;
		}
		return base.Serializer.GetReferenceResolver().IsReferenced(this, value);
	}

	private bool ShouldWriteProperty(object? memberValue, JsonObjectContract? containerContract, JsonProperty property)
	{
		if (memberValue == null && base.ResolvedNullValueHandling(containerContract, property) == NullValueHandling.Ignore)
		{
			return false;
		}
		if (this.HasFlag(property.DefaultValueHandling.GetValueOrDefault(base.Serializer._defaultValueHandling), DefaultValueHandling.Ignore) && MiscellaneousUtils.ValueEquals(memberValue, property.GetResolvedDefaultValue()))
		{
			return false;
		}
		return true;
	}

	private bool CheckForCircularReference(JsonWriter writer, object? value, JsonProperty? property, JsonContract? contract, JsonContainerContract? containerContract, JsonProperty? containerProperty)
	{
		if (value == null)
		{
			return true;
		}
		if (contract.ContractType == JsonContractType.Primitive || contract.ContractType == JsonContractType.String)
		{
			return true;
		}
		ReferenceLoopHandling? referenceLoopHandling = null;
		if (property != null)
		{
			referenceLoopHandling = property.ReferenceLoopHandling;
		}
		if (!referenceLoopHandling.HasValue && containerProperty != null)
		{
			referenceLoopHandling = containerProperty.ItemReferenceLoopHandling;
		}
		if (!referenceLoopHandling.HasValue && containerContract != null)
		{
			referenceLoopHandling = containerContract.ItemReferenceLoopHandling;
		}
		if ((base.Serializer._equalityComparer != null) ? this._serializeStack.Contains(value, base.Serializer._equalityComparer) : this._serializeStack.Contains(value))
		{
			string text = "Self referencing loop detected";
			if (property != null)
			{
				text += " for property '{0}'".FormatWith(CultureInfo.InvariantCulture, property.PropertyName);
			}
			text += " with type '{0}'.".FormatWith(CultureInfo.InvariantCulture, value.GetType());
			switch (referenceLoopHandling.GetValueOrDefault(base.Serializer._referenceLoopHandling))
			{
			case ReferenceLoopHandling.Error:
				throw JsonSerializationException.Create(null, writer.ContainerPath, text, null);
			case ReferenceLoopHandling.Ignore:
				if (base.TraceWriter != null && base.TraceWriter.LevelFilter >= TraceLevel.Verbose)
				{
					base.TraceWriter.Trace(TraceLevel.Verbose, JsonPosition.FormatMessage(null, writer.Path, text + ". Skipping serializing self referenced value."), null);
				}
				return false;
			case ReferenceLoopHandling.Serialize:
				if (base.TraceWriter != null && base.TraceWriter.LevelFilter >= TraceLevel.Verbose)
				{
					base.TraceWriter.Trace(TraceLevel.Verbose, JsonPosition.FormatMessage(null, writer.Path, text + ". Serializing self referenced value."), null);
				}
				return true;
			}
		}
		return true;
	}

	private void WriteReference(JsonWriter writer, object value)
	{
		string reference = this.GetReference(writer, value);
		if (base.TraceWriter != null && base.TraceWriter.LevelFilter >= TraceLevel.Info)
		{
			base.TraceWriter.Trace(TraceLevel.Info, JsonPosition.FormatMessage(null, writer.Path, "Writing object reference to Id '{0}' for {1}.".FormatWith(CultureInfo.InvariantCulture, reference, value.GetType())), null);
		}
		writer.WriteStartObject();
		writer.WritePropertyName("$ref", escape: false);
		writer.WriteValue(reference);
		writer.WriteEndObject();
	}

	private string GetReference(JsonWriter writer, object value)
	{
		try
		{
			return base.Serializer.GetReferenceResolver().GetReference(this, value);
		}
		catch (Exception ex)
		{
			throw JsonSerializationException.Create(null, writer.ContainerPath, "Error writing object reference for '{0}'.".FormatWith(CultureInfo.InvariantCulture, value.GetType()), ex);
		}
	}

	internal static bool TryConvertToString(object value, Type type, [NotNullWhen(true)] out string? s)
	{
		if (value is DateOnly dateOnly)
		{
			s = dateOnly.ToString("yyyy'-'MM'-'dd", CultureInfo.InvariantCulture);
			return true;
		}
		if (value is TimeOnly timeOnly)
		{
			s = timeOnly.ToString("HH':'mm':'ss.FFFFFFF", CultureInfo.InvariantCulture);
			return true;
		}
		if (JsonTypeReflector.CanTypeDescriptorConvertString(type, out TypeConverter typeConverter))
		{
			s = typeConverter.ConvertToInvariantString(value);
			return true;
		}
		if (value is Type type2)
		{
			s = type2.AssemblyQualifiedName;
			return true;
		}
		s = null;
		return false;
	}

	private void SerializeString(JsonWriter writer, object value, JsonStringContract contract)
	{
		this.OnSerializing(writer, contract, value);
		JsonSerializerInternalWriter.TryConvertToString(value, contract.UnderlyingType, out string s);
		writer.WriteValue(s);
		this.OnSerialized(writer, contract, value);
	}

	private void OnSerializing(JsonWriter writer, JsonContract contract, object value)
	{
		if (base.TraceWriter != null && base.TraceWriter.LevelFilter >= TraceLevel.Info)
		{
			base.TraceWriter.Trace(TraceLevel.Info, JsonPosition.FormatMessage(null, writer.Path, "Started serializing {0}".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType)), null);
		}
		contract.InvokeOnSerializing(value, base.Serializer._context);
	}

	private void OnSerialized(JsonWriter writer, JsonContract contract, object value)
	{
		if (base.TraceWriter != null && base.TraceWriter.LevelFilter >= TraceLevel.Info)
		{
			base.TraceWriter.Trace(TraceLevel.Info, JsonPosition.FormatMessage(null, writer.Path, "Finished serializing {0}".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType)), null);
		}
		contract.InvokeOnSerialized(value, base.Serializer._context);
	}

	private void SerializeObject(JsonWriter writer, object value, JsonObjectContract contract, JsonProperty? member, JsonContainerContract? collectionContract, JsonProperty? containerProperty)
	{
		this.OnSerializing(writer, contract, value);
		this._serializeStack.Add(value);
		this.WriteObjectStart(writer, value, contract, member, collectionContract, containerProperty);
		int top = writer.Top;
		for (int i = 0; i < contract.Properties.Count; i++)
		{
			JsonProperty jsonProperty = contract.Properties[i];
			try
			{
				if (this.CalculatePropertyValues(writer, value, contract, member, jsonProperty, out JsonContract memberContract, out object memberValue))
				{
					jsonProperty.WritePropertyName(writer);
					this.SerializeValue(writer, memberValue, memberContract, jsonProperty, contract, member);
				}
			}
			catch (Exception ex)
			{
				if (base.IsErrorHandled(value, contract, jsonProperty.PropertyName, null, writer.ContainerPath, ex))
				{
					this.HandleError(writer, top);
					continue;
				}
				throw;
			}
		}
		IEnumerable<KeyValuePair<object, object>> enumerable = contract.ExtensionDataGetter?.Invoke(value);
		if (enumerable != null)
		{
			foreach (KeyValuePair<object, object> item in enumerable)
			{
				JsonContract contract2 = this.GetContract(item.Key);
				JsonContract contractSafe = this.GetContractSafe(item.Value);
				string propertyName = this.GetPropertyName(writer, item.Key, contract2, out var _);
				propertyName = ((contract.ExtensionDataNameResolver != null) ? contract.ExtensionDataNameResolver(propertyName) : propertyName);
				if (this.ShouldWriteReference(item.Value, null, contractSafe, contract, member))
				{
					writer.WritePropertyName(propertyName);
					this.WriteReference(writer, item.Value);
				}
				else if (this.CheckForCircularReference(writer, item.Value, null, contractSafe, contract, member))
				{
					writer.WritePropertyName(propertyName);
					this.SerializeValue(writer, item.Value, contractSafe, null, contract, member);
				}
			}
		}
		writer.WriteEndObject();
		this._serializeStack.RemoveAt(this._serializeStack.Count - 1);
		this.OnSerialized(writer, contract, value);
	}

	private bool CalculatePropertyValues(JsonWriter writer, object value, JsonContainerContract contract, JsonProperty? member, JsonProperty property, [NotNullWhen(true)] out JsonContract? memberContract, out object? memberValue)
	{
		if (!property.Ignored && property.Readable && this.ShouldSerialize(writer, property, value) && this.IsSpecified(writer, property, value))
		{
			if (property.PropertyContract == null)
			{
				property.PropertyContract = base.Serializer._contractResolver.ResolveContract(property.PropertyType);
			}
			memberValue = property.ValueProvider.GetValue(value);
			memberContract = (property.PropertyContract.IsSealed ? property.PropertyContract : this.GetContractSafe(memberValue));
			if (this.ShouldWriteProperty(memberValue, contract as JsonObjectContract, property))
			{
				if (this.ShouldWriteReference(memberValue, property, memberContract, contract, member))
				{
					property.WritePropertyName(writer);
					this.WriteReference(writer, memberValue);
					return false;
				}
				if (!this.CheckForCircularReference(writer, memberValue, property, memberContract, contract, member))
				{
					return false;
				}
				if (memberValue == null)
				{
					JsonObjectContract jsonObjectContract = contract as JsonObjectContract;
					switch (property._required ?? (jsonObjectContract?.ItemRequired).GetValueOrDefault())
					{
					case Required.Always:
						throw JsonSerializationException.Create(null, writer.ContainerPath, "Cannot write a null value for property '{0}'. Property requires a value.".FormatWith(CultureInfo.InvariantCulture, property.PropertyName), null);
					case Required.DisallowNull:
						throw JsonSerializationException.Create(null, writer.ContainerPath, "Cannot write a null value for property '{0}'. Property requires a non-null value.".FormatWith(CultureInfo.InvariantCulture, property.PropertyName), null);
					}
				}
				return true;
			}
		}
		memberContract = null;
		memberValue = null;
		return false;
	}

	private void WriteObjectStart(JsonWriter writer, object value, JsonContract contract, JsonProperty? member, JsonContainerContract? collectionContract, JsonProperty? containerProperty)
	{
		writer.WriteStartObject();
		if ((this.ResolveIsReference(contract, member, collectionContract, containerProperty) ?? this.HasFlag(base.Serializer._preserveReferencesHandling, PreserveReferencesHandling.Objects)) && (member == null || member.Writable || this.HasCreatorParameter(collectionContract, member)))
		{
			this.WriteReferenceIdProperty(writer, contract.UnderlyingType, value);
		}
		if (this.ShouldWriteType(TypeNameHandling.Objects, contract, member, collectionContract, containerProperty))
		{
			this.WriteTypeProperty(writer, contract.UnderlyingType);
		}
	}

	private bool HasCreatorParameter(JsonContainerContract? contract, JsonProperty property)
	{
		if (!(contract is JsonObjectContract jsonObjectContract))
		{
			return false;
		}
		return jsonObjectContract.CreatorParameters.Contains(property.PropertyName);
	}

	private void WriteReferenceIdProperty(JsonWriter writer, Type type, object value)
	{
		string reference = this.GetReference(writer, value);
		if (base.TraceWriter != null && base.TraceWriter.LevelFilter >= TraceLevel.Verbose)
		{
			base.TraceWriter.Trace(TraceLevel.Verbose, JsonPosition.FormatMessage(null, writer.Path, "Writing object reference Id '{0}' for {1}.".FormatWith(CultureInfo.InvariantCulture, reference, type)), null);
		}
		writer.WritePropertyName("$id", escape: false);
		writer.WriteValue(reference);
	}

	private void WriteTypeProperty(JsonWriter writer, Type type)
	{
		string typeName = ReflectionUtils.GetTypeName(type, base.Serializer._typeNameAssemblyFormatHandling, base.Serializer._serializationBinder);
		if (base.TraceWriter != null && base.TraceWriter.LevelFilter >= TraceLevel.Verbose)
		{
			base.TraceWriter.Trace(TraceLevel.Verbose, JsonPosition.FormatMessage(null, writer.Path, "Writing type name '{0}' for {1}.".FormatWith(CultureInfo.InvariantCulture, typeName, type)), null);
		}
		writer.WritePropertyName("$type", escape: false);
		writer.WriteValue(typeName);
	}

	private bool HasFlag(DefaultValueHandling value, DefaultValueHandling flag)
	{
		return (value & flag) == flag;
	}

	private bool HasFlag(PreserveReferencesHandling value, PreserveReferencesHandling flag)
	{
		return (value & flag) == flag;
	}

	private bool HasFlag(TypeNameHandling value, TypeNameHandling flag)
	{
		return (value & flag) == flag;
	}

	private void SerializeConvertable(JsonWriter writer, JsonConverter converter, object value, JsonContract contract, JsonContainerContract? collectionContract, JsonProperty? containerProperty)
	{
		if (this.ShouldWriteReference(value, null, contract, collectionContract, containerProperty))
		{
			this.WriteReference(writer, value);
		}
		else if (this.CheckForCircularReference(writer, value, null, contract, collectionContract, containerProperty))
		{
			this._serializeStack.Add(value);
			if (base.TraceWriter != null && base.TraceWriter.LevelFilter >= TraceLevel.Info)
			{
				base.TraceWriter.Trace(TraceLevel.Info, JsonPosition.FormatMessage(null, writer.Path, "Started serializing {0} with converter {1}.".FormatWith(CultureInfo.InvariantCulture, value.GetType(), converter.GetType())), null);
			}
			converter.WriteJson(writer, value, this.GetInternalSerializer());
			if (base.TraceWriter != null && base.TraceWriter.LevelFilter >= TraceLevel.Info)
			{
				base.TraceWriter.Trace(TraceLevel.Info, JsonPosition.FormatMessage(null, writer.Path, "Finished serializing {0} with converter {1}.".FormatWith(CultureInfo.InvariantCulture, value.GetType(), converter.GetType())), null);
			}
			this._serializeStack.RemoveAt(this._serializeStack.Count - 1);
		}
	}

	private void SerializeList(JsonWriter writer, IEnumerable values, JsonArrayContract contract, JsonProperty? member, JsonContainerContract? collectionContract, JsonProperty? containerProperty)
	{
		object obj = ((values is IWrappedCollection wrappedCollection) ? wrappedCollection.UnderlyingCollection : values);
		this.OnSerializing(writer, contract, obj);
		this._serializeStack.Add(obj);
		bool flag = this.WriteStartArray(writer, obj, contract, member, collectionContract, containerProperty);
		writer.WriteStartArray();
		int top = writer.Top;
		int num = 0;
		foreach (object value in values)
		{
			try
			{
				JsonContract jsonContract = contract.FinalItemContract ?? this.GetContractSafe(value);
				if (this.ShouldWriteReference(value, null, jsonContract, contract, member))
				{
					this.WriteReference(writer, value);
				}
				else if (this.CheckForCircularReference(writer, value, null, jsonContract, contract, member))
				{
					this.SerializeValue(writer, value, jsonContract, null, contract, member);
				}
			}
			catch (Exception ex)
			{
				if (base.IsErrorHandled(obj, contract, num, null, writer.ContainerPath, ex))
				{
					this.HandleError(writer, top);
					continue;
				}
				throw;
			}
			finally
			{
				num++;
			}
		}
		writer.WriteEndArray();
		if (flag)
		{
			writer.WriteEndObject();
		}
		this._serializeStack.RemoveAt(this._serializeStack.Count - 1);
		this.OnSerialized(writer, contract, obj);
	}

	private void SerializeMultidimensionalArray(JsonWriter writer, Array values, JsonArrayContract contract, JsonProperty? member, JsonContainerContract? collectionContract, JsonProperty? containerProperty)
	{
		this.OnSerializing(writer, contract, values);
		this._serializeStack.Add(values);
		bool num = this.WriteStartArray(writer, values, contract, member, collectionContract, containerProperty);
		this.SerializeMultidimensionalArray(writer, values, contract, member, writer.Top, CollectionUtils.ArrayEmpty<int>());
		if (num)
		{
			writer.WriteEndObject();
		}
		this._serializeStack.RemoveAt(this._serializeStack.Count - 1);
		this.OnSerialized(writer, contract, values);
	}

	private void SerializeMultidimensionalArray(JsonWriter writer, Array values, JsonArrayContract contract, JsonProperty? member, int initialDepth, int[] indices)
	{
		int num = indices.Length;
		int[] array = new int[num + 1];
		for (int i = 0; i < num; i++)
		{
			array[i] = indices[i];
		}
		writer.WriteStartArray();
		for (int j = values.GetLowerBound(num); j <= values.GetUpperBound(num); j++)
		{
			array[num] = j;
			if (array.Length == values.Rank)
			{
				object value = values.GetValue(array);
				try
				{
					JsonContract jsonContract = contract.FinalItemContract ?? this.GetContractSafe(value);
					if (this.ShouldWriteReference(value, null, jsonContract, contract, member))
					{
						this.WriteReference(writer, value);
					}
					else if (this.CheckForCircularReference(writer, value, null, jsonContract, contract, member))
					{
						this.SerializeValue(writer, value, jsonContract, null, contract, member);
					}
				}
				catch (Exception ex)
				{
					if (base.IsErrorHandled(values, contract, j, null, writer.ContainerPath, ex))
					{
						this.HandleError(writer, initialDepth + 1);
						continue;
					}
					throw;
				}
			}
			else
			{
				this.SerializeMultidimensionalArray(writer, values, contract, member, initialDepth + 1, array);
			}
		}
		writer.WriteEndArray();
	}

	private bool WriteStartArray(JsonWriter writer, object values, JsonArrayContract contract, JsonProperty? member, JsonContainerContract? containerContract, JsonProperty? containerProperty)
	{
		bool flag = (this.ResolveIsReference(contract, member, containerContract, containerProperty) ?? this.HasFlag(base.Serializer._preserveReferencesHandling, PreserveReferencesHandling.Arrays)) && (member == null || member.Writable || this.HasCreatorParameter(containerContract, member));
		bool flag2 = this.ShouldWriteType(TypeNameHandling.Arrays, contract, member, containerContract, containerProperty);
		bool num = flag || flag2;
		if (num)
		{
			writer.WriteStartObject();
			if (flag)
			{
				this.WriteReferenceIdProperty(writer, contract.UnderlyingType, values);
			}
			if (flag2)
			{
				this.WriteTypeProperty(writer, values.GetType());
			}
			writer.WritePropertyName("$values", escape: false);
		}
		if (contract.ItemContract == null)
		{
			contract.ItemContract = base.Serializer._contractResolver.ResolveContract(contract.CollectionItemType ?? typeof(object));
		}
		return num;
	}

	[SecuritySafeCritical]
	private void SerializeISerializable(JsonWriter writer, ISerializable value, JsonISerializableContract contract, JsonProperty? member, JsonContainerContract? collectionContract, JsonProperty? containerProperty)
	{
		if (!JsonTypeReflector.FullyTrusted)
		{
			string format = "Type '{0}' implements ISerializable but cannot be serialized using the ISerializable interface because the current application is not fully trusted and ISerializable can expose secure data." + Environment.NewLine + "To fix this error either change the environment to be fully trusted, change the application to not deserialize the type, add JsonObjectAttribute to the type or change the JsonSerializer setting ContractResolver to use a new DefaultContractResolver with IgnoreSerializableInterface set to true." + Environment.NewLine;
			format = format.FormatWith(CultureInfo.InvariantCulture, value.GetType());
			throw JsonSerializationException.Create(null, writer.ContainerPath, format, null);
		}
		this.OnSerializing(writer, contract, value);
		this._serializeStack.Add(value);
		this.WriteObjectStart(writer, value, contract, member, collectionContract, containerProperty);
		SerializationInfo serializationInfo = new SerializationInfo(contract.UnderlyingType, new FormatterConverter());
		value.GetObjectData(serializationInfo, base.Serializer._context);
		SerializationInfoEnumerator enumerator = serializationInfo.GetEnumerator();
		while (enumerator.MoveNext())
		{
			SerializationEntry current = enumerator.Current;
			JsonContract contractSafe = this.GetContractSafe(current.Value);
			if (this.ShouldWriteReference(current.Value, null, contractSafe, contract, member))
			{
				writer.WritePropertyName(current.Name);
				this.WriteReference(writer, current.Value);
			}
			else if (this.CheckForCircularReference(writer, current.Value, null, contractSafe, contract, member))
			{
				writer.WritePropertyName(current.Name);
				this.SerializeValue(writer, current.Value, contractSafe, null, contract, member);
			}
		}
		writer.WriteEndObject();
		this._serializeStack.RemoveAt(this._serializeStack.Count - 1);
		this.OnSerialized(writer, contract, value);
	}

	private void SerializeDynamic(JsonWriter writer, IDynamicMetaObjectProvider value, JsonDynamicContract contract, JsonProperty? member, JsonContainerContract? collectionContract, JsonProperty? containerProperty)
	{
		this.OnSerializing(writer, contract, value);
		this._serializeStack.Add(value);
		this.WriteObjectStart(writer, value, contract, member, collectionContract, containerProperty);
		int top = writer.Top;
		for (int i = 0; i < contract.Properties.Count; i++)
		{
			JsonProperty jsonProperty = contract.Properties[i];
			if (!jsonProperty.HasMemberAttribute)
			{
				continue;
			}
			try
			{
				if (this.CalculatePropertyValues(writer, value, contract, member, jsonProperty, out JsonContract memberContract, out object memberValue))
				{
					jsonProperty.WritePropertyName(writer);
					this.SerializeValue(writer, memberValue, memberContract, jsonProperty, contract, member);
				}
			}
			catch (Exception ex)
			{
				if (base.IsErrorHandled(value, contract, jsonProperty.PropertyName, null, writer.ContainerPath, ex))
				{
					this.HandleError(writer, top);
					continue;
				}
				throw;
			}
		}
		foreach (string dynamicMemberName in value.GetDynamicMemberNames())
		{
			if (!contract.TryGetMember(value, dynamicMemberName, out object value2))
			{
				continue;
			}
			try
			{
				JsonContract contractSafe = this.GetContractSafe(value2);
				if (this.ShouldWriteDynamicProperty(value2) && this.CheckForCircularReference(writer, value2, null, contractSafe, contract, member))
				{
					string name = ((contract.PropertyNameResolver != null) ? contract.PropertyNameResolver(dynamicMemberName) : dynamicMemberName);
					writer.WritePropertyName(name);
					this.SerializeValue(writer, value2, contractSafe, null, contract, member);
				}
			}
			catch (Exception ex2)
			{
				if (base.IsErrorHandled(value, contract, dynamicMemberName, null, writer.ContainerPath, ex2))
				{
					this.HandleError(writer, top);
					continue;
				}
				throw;
			}
		}
		writer.WriteEndObject();
		this._serializeStack.RemoveAt(this._serializeStack.Count - 1);
		this.OnSerialized(writer, contract, value);
	}

	private bool ShouldWriteDynamicProperty(object? memberValue)
	{
		if (base.Serializer._nullValueHandling == NullValueHandling.Ignore && memberValue == null)
		{
			return false;
		}
		if (this.HasFlag(base.Serializer._defaultValueHandling, DefaultValueHandling.Ignore) && (memberValue == null || MiscellaneousUtils.ValueEquals(memberValue, ReflectionUtils.GetDefaultValue(memberValue.GetType()))))
		{
			return false;
		}
		return true;
	}

	private bool ShouldWriteType(TypeNameHandling typeNameHandlingFlag, JsonContract contract, JsonProperty? member, JsonContainerContract? containerContract, JsonProperty? containerProperty)
	{
		TypeNameHandling value = member?.TypeNameHandling ?? containerProperty?.ItemTypeNameHandling ?? containerContract?.ItemTypeNameHandling ?? base.Serializer._typeNameHandling;
		if (this.HasFlag(value, typeNameHandlingFlag))
		{
			return true;
		}
		if (this.HasFlag(value, TypeNameHandling.Auto))
		{
			if (member != null)
			{
				if (contract.NonNullableUnderlyingType != member.PropertyContract.CreatedType)
				{
					return true;
				}
			}
			else if (containerContract != null)
			{
				if (containerContract.ItemContract == null || contract.NonNullableUnderlyingType != containerContract.ItemContract.CreatedType)
				{
					return true;
				}
			}
			else if (this._rootType != null && this._serializeStack.Count == this._rootLevel)
			{
				JsonContract jsonContract = base.Serializer._contractResolver.ResolveContract(this._rootType);
				if (contract.NonNullableUnderlyingType != jsonContract.CreatedType)
				{
					return true;
				}
			}
		}
		return false;
	}

	private void SerializeDictionary(JsonWriter writer, IDictionary values, JsonDictionaryContract contract, JsonProperty? member, JsonContainerContract? collectionContract, JsonProperty? containerProperty)
	{
		object obj = ((values is IWrappedDictionary wrappedDictionary) ? wrappedDictionary.UnderlyingDictionary : values);
		this.OnSerializing(writer, contract, obj);
		this._serializeStack.Add(obj);
		this.WriteObjectStart(writer, obj, contract, member, collectionContract, containerProperty);
		if (contract.ItemContract == null)
		{
			contract.ItemContract = base.Serializer._contractResolver.ResolveContract(contract.DictionaryValueType ?? typeof(object));
		}
		if (contract.KeyContract == null)
		{
			contract.KeyContract = base.Serializer._contractResolver.ResolveContract(contract.DictionaryKeyType ?? typeof(object));
		}
		int top = writer.Top;
		foreach (DictionaryEntry value2 in values)
		{
			string propertyName = this.GetPropertyName(writer, value2.Key, contract.KeyContract, out var escape);
			propertyName = ((contract.DictionaryKeyResolver != null) ? contract.DictionaryKeyResolver(propertyName) : propertyName);
			try
			{
				object value = value2.Value;
				JsonContract jsonContract = contract.FinalItemContract ?? this.GetContractSafe(value);
				if (this.ShouldWriteReference(value, null, jsonContract, contract, member))
				{
					writer.WritePropertyName(propertyName, escape);
					this.WriteReference(writer, value);
				}
				else if (this.CheckForCircularReference(writer, value, null, jsonContract, contract, member))
				{
					writer.WritePropertyName(propertyName, escape);
					this.SerializeValue(writer, value, jsonContract, null, contract, member);
				}
			}
			catch (Exception ex)
			{
				if (base.IsErrorHandled(obj, contract, propertyName, null, writer.ContainerPath, ex))
				{
					this.HandleError(writer, top);
					continue;
				}
				throw;
			}
		}
		writer.WriteEndObject();
		this._serializeStack.RemoveAt(this._serializeStack.Count - 1);
		this.OnSerialized(writer, contract, obj);
	}

	private string GetPropertyName(JsonWriter writer, object name, JsonContract contract, out bool escape)
	{
		if (contract.ContractType == JsonContractType.Primitive)
		{
			JsonPrimitiveContract jsonPrimitiveContract = (JsonPrimitiveContract)contract;
			switch (jsonPrimitiveContract.TypeCode)
			{
			case PrimitiveTypeCode.DateTime:
			case PrimitiveTypeCode.DateTimeNullable:
			{
				DateTime value = DateTimeUtils.EnsureDateTime((DateTime)name, writer.DateTimeZoneHandling);
				escape = false;
				StringWriter stringWriter2 = new StringWriter(CultureInfo.InvariantCulture);
				DateTimeUtils.WriteDateTimeString(stringWriter2, value, writer.DateFormatHandling, writer.DateFormatString, writer.Culture);
				return stringWriter2.ToString();
			}
			case PrimitiveTypeCode.DateTimeOffset:
			case PrimitiveTypeCode.DateTimeOffsetNullable:
			{
				escape = false;
				StringWriter stringWriter = new StringWriter(CultureInfo.InvariantCulture);
				DateTimeUtils.WriteDateTimeOffsetString(stringWriter, (DateTimeOffset)name, writer.DateFormatHandling, writer.DateFormatString, writer.Culture);
				return stringWriter.ToString();
			}
			case PrimitiveTypeCode.Double:
			case PrimitiveTypeCode.DoubleNullable:
			{
				double num = (double)name;
				escape = false;
				return num.ToString("R", CultureInfo.InvariantCulture);
			}
			case PrimitiveTypeCode.Single:
			case PrimitiveTypeCode.SingleNullable:
			{
				float num2 = (float)name;
				escape = false;
				return num2.ToString("R", CultureInfo.InvariantCulture);
			}
			default:
			{
				escape = true;
				if (jsonPrimitiveContract.IsEnum && EnumUtils.TryToString(jsonPrimitiveContract.NonNullableUnderlyingType, name, null, out string name2))
				{
					return name2;
				}
				return Convert.ToString(name, CultureInfo.InvariantCulture);
			}
			}
		}
		if (JsonSerializerInternalWriter.TryConvertToString(name, name.GetType(), out string s))
		{
			escape = true;
			return s;
		}
		escape = true;
		return name.ToString();
	}

	private void HandleError(JsonWriter writer, int initialDepth)
	{
		base.ClearErrorContext();
		if (writer.WriteState == WriteState.Property)
		{
			writer.WriteNull();
		}
		while (writer.Top > initialDepth)
		{
			writer.WriteEnd();
		}
	}

	private bool ShouldSerialize(JsonWriter writer, JsonProperty property, object target)
	{
		if (property.ShouldSerialize == null)
		{
			return true;
		}
		bool flag = property.ShouldSerialize(target);
		if (base.TraceWriter != null && base.TraceWriter.LevelFilter >= TraceLevel.Verbose)
		{
			base.TraceWriter.Trace(TraceLevel.Verbose, JsonPosition.FormatMessage(null, writer.Path, "ShouldSerialize result for property '{0}' on {1}: {2}".FormatWith(CultureInfo.InvariantCulture, property.PropertyName, property.DeclaringType, flag)), null);
		}
		return flag;
	}

	private bool IsSpecified(JsonWriter writer, JsonProperty property, object target)
	{
		if (property.GetIsSpecified == null)
		{
			return true;
		}
		bool flag = property.GetIsSpecified(target);
		if (base.TraceWriter != null && base.TraceWriter.LevelFilter >= TraceLevel.Verbose)
		{
			base.TraceWriter.Trace(TraceLevel.Verbose, JsonPosition.FormatMessage(null, writer.Path, "IsSpecified result for property '{0}' on {1}: {2}".FormatWith(CultureInfo.InvariantCulture, property.PropertyName, property.DeclaringType, flag)), null);
		}
		return flag;
	}
}
