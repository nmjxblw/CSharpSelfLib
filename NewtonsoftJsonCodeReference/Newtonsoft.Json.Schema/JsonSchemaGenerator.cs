using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Schema;

/// <summary>
/// <para>
/// Generates a <see cref="T:Newtonsoft.Json.Schema.JsonSchema" /> from a specified <see cref="T:System.Type" />.
/// </para>
/// <note type="caution">
/// JSON Schema validation has been moved to its own package. See <see href="https://www.newtonsoft.com/jsonschema">https://www.newtonsoft.com/jsonschema</see> for more details.
/// </note>
/// </summary>
[Obsolete("JSON Schema validation has been moved to its own package. See https://www.newtonsoft.com/jsonschema for more details.")]
public class JsonSchemaGenerator
{
	private class TypeSchema
	{
		public Type Type { get; }

		public JsonSchema Schema { get; }

		public TypeSchema(Type type, JsonSchema schema)
		{
			ValidationUtils.ArgumentNotNull(type, "type");
			ValidationUtils.ArgumentNotNull(schema, "schema");
			this.Type = type;
			this.Schema = schema;
		}
	}

	private IContractResolver _contractResolver;

	private JsonSchemaResolver _resolver;

	private readonly IList<TypeSchema> _stack = new List<TypeSchema>();

	private JsonSchema _currentSchema;

	/// <summary>
	/// Gets or sets how undefined schemas are handled by the serializer.
	/// </summary>
	public UndefinedSchemaIdHandling UndefinedSchemaIdHandling { get; set; }

	/// <summary>
	/// Gets or sets the contract resolver.
	/// </summary>
	/// <value>The contract resolver.</value>
	public IContractResolver ContractResolver
	{
		get
		{
			if (this._contractResolver == null)
			{
				return DefaultContractResolver.Instance;
			}
			return this._contractResolver;
		}
		set
		{
			this._contractResolver = value;
		}
	}

	private JsonSchema CurrentSchema => this._currentSchema;

	private void Push(TypeSchema typeSchema)
	{
		this._currentSchema = typeSchema.Schema;
		this._stack.Add(typeSchema);
		this._resolver.LoadedSchemas.Add(typeSchema.Schema);
	}

	private TypeSchema Pop()
	{
		TypeSchema result = this._stack[this._stack.Count - 1];
		this._stack.RemoveAt(this._stack.Count - 1);
		TypeSchema typeSchema = this._stack.LastOrDefault();
		if (typeSchema != null)
		{
			this._currentSchema = typeSchema.Schema;
			return result;
		}
		this._currentSchema = null;
		return result;
	}

	/// <summary>
	/// Generate a <see cref="T:Newtonsoft.Json.Schema.JsonSchema" /> from the specified type.
	/// </summary>
	/// <param name="type">The type to generate a <see cref="T:Newtonsoft.Json.Schema.JsonSchema" /> from.</param>
	/// <returns>A <see cref="T:Newtonsoft.Json.Schema.JsonSchema" /> generated from the specified type.</returns>
	public JsonSchema Generate(Type type)
	{
		return this.Generate(type, new JsonSchemaResolver(), rootSchemaNullable: false);
	}

	/// <summary>
	/// Generate a <see cref="T:Newtonsoft.Json.Schema.JsonSchema" /> from the specified type.
	/// </summary>
	/// <param name="type">The type to generate a <see cref="T:Newtonsoft.Json.Schema.JsonSchema" /> from.</param>
	/// <param name="resolver">The <see cref="T:Newtonsoft.Json.Schema.JsonSchemaResolver" /> used to resolve schema references.</param>
	/// <returns>A <see cref="T:Newtonsoft.Json.Schema.JsonSchema" /> generated from the specified type.</returns>
	public JsonSchema Generate(Type type, JsonSchemaResolver resolver)
	{
		return this.Generate(type, resolver, rootSchemaNullable: false);
	}

	/// <summary>
	/// Generate a <see cref="T:Newtonsoft.Json.Schema.JsonSchema" /> from the specified type.
	/// </summary>
	/// <param name="type">The type to generate a <see cref="T:Newtonsoft.Json.Schema.JsonSchema" /> from.</param>
	/// <param name="rootSchemaNullable">Specify whether the generated root <see cref="T:Newtonsoft.Json.Schema.JsonSchema" /> will be nullable.</param>
	/// <returns>A <see cref="T:Newtonsoft.Json.Schema.JsonSchema" /> generated from the specified type.</returns>
	public JsonSchema Generate(Type type, bool rootSchemaNullable)
	{
		return this.Generate(type, new JsonSchemaResolver(), rootSchemaNullable);
	}

	/// <summary>
	/// Generate a <see cref="T:Newtonsoft.Json.Schema.JsonSchema" /> from the specified type.
	/// </summary>
	/// <param name="type">The type to generate a <see cref="T:Newtonsoft.Json.Schema.JsonSchema" /> from.</param>
	/// <param name="resolver">The <see cref="T:Newtonsoft.Json.Schema.JsonSchemaResolver" /> used to resolve schema references.</param>
	/// <param name="rootSchemaNullable">Specify whether the generated root <see cref="T:Newtonsoft.Json.Schema.JsonSchema" /> will be nullable.</param>
	/// <returns>A <see cref="T:Newtonsoft.Json.Schema.JsonSchema" /> generated from the specified type.</returns>
	public JsonSchema Generate(Type type, JsonSchemaResolver resolver, bool rootSchemaNullable)
	{
		ValidationUtils.ArgumentNotNull(type, "type");
		ValidationUtils.ArgumentNotNull(resolver, "resolver");
		this._resolver = resolver;
		return this.GenerateInternal(type, (!rootSchemaNullable) ? Required.Always : Required.Default, required: false);
	}

	private string GetTitle(Type type)
	{
		JsonContainerAttribute cachedAttribute = JsonTypeReflector.GetCachedAttribute<JsonContainerAttribute>(type);
		if (!StringUtils.IsNullOrEmpty(cachedAttribute?.Title))
		{
			return cachedAttribute.Title;
		}
		return null;
	}

	private string GetDescription(Type type)
	{
		JsonContainerAttribute cachedAttribute = JsonTypeReflector.GetCachedAttribute<JsonContainerAttribute>(type);
		if (!StringUtils.IsNullOrEmpty(cachedAttribute?.Description))
		{
			return cachedAttribute.Description;
		}
		return ReflectionUtils.GetAttribute<DescriptionAttribute>(type)?.Description;
	}

	private string GetTypeId(Type type, bool explicitOnly)
	{
		JsonContainerAttribute cachedAttribute = JsonTypeReflector.GetCachedAttribute<JsonContainerAttribute>(type);
		if (!StringUtils.IsNullOrEmpty(cachedAttribute?.Id))
		{
			return cachedAttribute.Id;
		}
		if (explicitOnly)
		{
			return null;
		}
		return this.UndefinedSchemaIdHandling switch
		{
			UndefinedSchemaIdHandling.UseTypeName => type.FullName, 
			UndefinedSchemaIdHandling.UseAssemblyQualifiedName => type.AssemblyQualifiedName, 
			_ => null, 
		};
	}

	private JsonSchema GenerateInternal(Type type, Required valueRequired, bool required)
	{
		ValidationUtils.ArgumentNotNull(type, "type");
		string typeId = this.GetTypeId(type, explicitOnly: false);
		string typeId2 = this.GetTypeId(type, explicitOnly: true);
		if (!StringUtils.IsNullOrEmpty(typeId))
		{
			JsonSchema schema = this._resolver.GetSchema(typeId);
			if (schema != null)
			{
				if (valueRequired != Required.Always && !JsonSchemaGenerator.HasFlag(schema.Type, JsonSchemaType.Null))
				{
					schema.Type |= JsonSchemaType.Null;
				}
				if (required && schema.Required != true)
				{
					schema.Required = true;
				}
				return schema;
			}
		}
		if (this._stack.Any((TypeSchema tc) => tc.Type == type))
		{
			throw new JsonException("Unresolved circular reference for type '{0}'. Explicitly define an Id for the type using a JsonObject/JsonArray attribute or automatically generate a type Id using the UndefinedSchemaIdHandling property.".FormatWith(CultureInfo.InvariantCulture, type));
		}
		JsonContract jsonContract = this.ContractResolver.ResolveContract(type);
		JsonConverter? obj = jsonContract.Converter ?? jsonContract.InternalConverter;
		this.Push(new TypeSchema(type, new JsonSchema()));
		if (typeId2 != null)
		{
			this.CurrentSchema.Id = typeId2;
		}
		if (required)
		{
			this.CurrentSchema.Required = true;
		}
		this.CurrentSchema.Title = this.GetTitle(type);
		this.CurrentSchema.Description = this.GetDescription(type);
		if (obj != null)
		{
			this.CurrentSchema.Type = JsonSchemaType.Any;
		}
		else
		{
			switch (jsonContract.ContractType)
			{
			case JsonContractType.Object:
				this.CurrentSchema.Type = this.AddNullType(JsonSchemaType.Object, valueRequired);
				this.CurrentSchema.Id = this.GetTypeId(type, explicitOnly: false);
				this.GenerateObjectSchema(type, (JsonObjectContract)jsonContract);
				break;
			case JsonContractType.Array:
			{
				this.CurrentSchema.Type = this.AddNullType(JsonSchemaType.Array, valueRequired);
				this.CurrentSchema.Id = this.GetTypeId(type, explicitOnly: false);
				bool flag = JsonTypeReflector.GetCachedAttribute<JsonArrayAttribute>(type)?.AllowNullItems ?? true;
				Type collectionItemType = ReflectionUtils.GetCollectionItemType(type);
				if (collectionItemType != null)
				{
					this.CurrentSchema.Items = new List<JsonSchema>();
					this.CurrentSchema.Items.Add(this.GenerateInternal(collectionItemType, (!flag) ? Required.Always : Required.Default, required: false));
				}
				break;
			}
			case JsonContractType.Primitive:
				this.CurrentSchema.Type = this.GetJsonSchemaType(type, valueRequired);
				if (this.CurrentSchema.Type == JsonSchemaType.Integer && type.IsEnum() && !type.IsDefined(typeof(FlagsAttribute), inherit: true))
				{
					this.CurrentSchema.Enum = new List<JToken>();
					EnumInfo enumValuesAndNames = EnumUtils.GetEnumValuesAndNames(type);
					for (int num = 0; num < enumValuesAndNames.Names.Length; num++)
					{
						ulong value = enumValuesAndNames.Values[num];
						JToken item = JToken.FromObject(Enum.ToObject(type, value));
						this.CurrentSchema.Enum.Add(item);
					}
				}
				break;
			case JsonContractType.String:
			{
				JsonSchemaType value2 = ((!ReflectionUtils.IsNullable(jsonContract.UnderlyingType)) ? JsonSchemaType.String : this.AddNullType(JsonSchemaType.String, valueRequired));
				this.CurrentSchema.Type = value2;
				break;
			}
			case JsonContractType.Dictionary:
			{
				this.CurrentSchema.Type = this.AddNullType(JsonSchemaType.Object, valueRequired);
				ReflectionUtils.GetDictionaryKeyValueTypes(type, out Type keyType, out Type valueType);
				if (keyType != null && this.ContractResolver.ResolveContract(keyType).ContractType == JsonContractType.Primitive)
				{
					this.CurrentSchema.AdditionalProperties = this.GenerateInternal(valueType, Required.Default, required: false);
				}
				break;
			}
			case JsonContractType.Serializable:
				this.CurrentSchema.Type = this.AddNullType(JsonSchemaType.Object, valueRequired);
				this.CurrentSchema.Id = this.GetTypeId(type, explicitOnly: false);
				this.GenerateISerializableContract(type, (JsonISerializableContract)jsonContract);
				break;
			case JsonContractType.Dynamic:
			case JsonContractType.Linq:
				this.CurrentSchema.Type = JsonSchemaType.Any;
				break;
			default:
				throw new JsonException("Unexpected contract type: {0}".FormatWith(CultureInfo.InvariantCulture, jsonContract));
			}
		}
		return this.Pop().Schema;
	}

	private JsonSchemaType AddNullType(JsonSchemaType type, Required valueRequired)
	{
		if (valueRequired != Required.Always)
		{
			return type | JsonSchemaType.Null;
		}
		return type;
	}

	private bool HasFlag(DefaultValueHandling value, DefaultValueHandling flag)
	{
		return (value & flag) == flag;
	}

	private void GenerateObjectSchema(Type type, JsonObjectContract contract)
	{
		this.CurrentSchema.Properties = new Dictionary<string, JsonSchema>();
		foreach (JsonProperty property in contract.Properties)
		{
			if (!property.Ignored)
			{
				bool flag = property.NullValueHandling == NullValueHandling.Ignore || this.HasFlag(property.DefaultValueHandling.GetValueOrDefault(), DefaultValueHandling.Ignore) || property.ShouldSerialize != null || property.GetIsSpecified != null;
				JsonSchema jsonSchema = this.GenerateInternal(property.PropertyType, property.Required, !flag);
				if (property.DefaultValue != null)
				{
					jsonSchema.Default = JToken.FromObject(property.DefaultValue);
				}
				this.CurrentSchema.Properties.Add(property.PropertyName, jsonSchema);
			}
		}
		if (type.IsSealed())
		{
			this.CurrentSchema.AllowAdditionalProperties = false;
		}
	}

	private void GenerateISerializableContract(Type type, JsonISerializableContract contract)
	{
		this.CurrentSchema.AllowAdditionalProperties = true;
	}

	internal static bool HasFlag(JsonSchemaType? value, JsonSchemaType flag)
	{
		if (!value.HasValue)
		{
			return true;
		}
		if ((value & flag) == flag)
		{
			return true;
		}
		if (flag == JsonSchemaType.Integer && ((uint?)value & 2u) == 2)
		{
			return true;
		}
		return false;
	}

	private JsonSchemaType GetJsonSchemaType(Type type, Required valueRequired)
	{
		JsonSchemaType jsonSchemaType = JsonSchemaType.None;
		if (valueRequired != Required.Always && ReflectionUtils.IsNullable(type))
		{
			jsonSchemaType = JsonSchemaType.Null;
			if (ReflectionUtils.IsNullableType(type))
			{
				type = Nullable.GetUnderlyingType(type);
			}
		}
		PrimitiveTypeCode typeCode = ConvertUtils.GetTypeCode(type);
		switch (typeCode)
		{
		case PrimitiveTypeCode.Empty:
		case PrimitiveTypeCode.Object:
			return jsonSchemaType | JsonSchemaType.String;
		case PrimitiveTypeCode.DBNull:
			return jsonSchemaType | JsonSchemaType.Null;
		case PrimitiveTypeCode.Boolean:
			return jsonSchemaType | JsonSchemaType.Boolean;
		case PrimitiveTypeCode.Char:
			return jsonSchemaType | JsonSchemaType.String;
		case PrimitiveTypeCode.SByte:
		case PrimitiveTypeCode.Int16:
		case PrimitiveTypeCode.UInt16:
		case PrimitiveTypeCode.Int32:
		case PrimitiveTypeCode.Byte:
		case PrimitiveTypeCode.UInt32:
		case PrimitiveTypeCode.Int64:
		case PrimitiveTypeCode.UInt64:
		case PrimitiveTypeCode.BigInteger:
			return jsonSchemaType | JsonSchemaType.Integer;
		case PrimitiveTypeCode.Single:
		case PrimitiveTypeCode.Double:
		case PrimitiveTypeCode.Decimal:
			return jsonSchemaType | JsonSchemaType.Float;
		case PrimitiveTypeCode.DateTime:
		case PrimitiveTypeCode.DateTimeOffset:
			return jsonSchemaType | JsonSchemaType.String;
		case PrimitiveTypeCode.Guid:
		case PrimitiveTypeCode.TimeSpan:
		case PrimitiveTypeCode.Uri:
		case PrimitiveTypeCode.String:
		case PrimitiveTypeCode.Bytes:
			return jsonSchemaType | JsonSchemaType.String;
		default:
			throw new JsonException("Unexpected type code '{0}' for type '{1}'.".FormatWith(CultureInfo.InvariantCulture, typeCode, type));
		}
	}
}
