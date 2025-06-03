using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Schema;

[Obsolete("JSON Schema validation has been moved to its own package. See https://www.newtonsoft.com/jsonschema for more details.")]
internal class JsonSchemaBuilder
{
	private readonly IList<JsonSchema> _stack;

	private readonly JsonSchemaResolver _resolver;

	private readonly IDictionary<string, JsonSchema> _documentSchemas;

	private JsonSchema _currentSchema;

	private JObject _rootSchema;

	private JsonSchema CurrentSchema => this._currentSchema;

	public JsonSchemaBuilder(JsonSchemaResolver resolver)
	{
		this._stack = new List<JsonSchema>();
		this._documentSchemas = new Dictionary<string, JsonSchema>();
		this._resolver = resolver;
	}

	private void Push(JsonSchema value)
	{
		this._currentSchema = value;
		this._stack.Add(value);
		this._resolver.LoadedSchemas.Add(value);
		this._documentSchemas.Add(value.Location, value);
	}

	private JsonSchema Pop()
	{
		JsonSchema currentSchema = this._currentSchema;
		this._stack.RemoveAt(this._stack.Count - 1);
		this._currentSchema = this._stack.LastOrDefault();
		return currentSchema;
	}

	internal JsonSchema Read(JsonReader reader)
	{
		JToken jToken = JToken.ReadFrom(reader);
		this._rootSchema = jToken as JObject;
		JsonSchema jsonSchema = this.BuildSchema(jToken);
		this.ResolveReferences(jsonSchema);
		return jsonSchema;
	}

	private string UnescapeReference(string reference)
	{
		return StringUtils.Replace(StringUtils.Replace(Uri.UnescapeDataString(reference), "~1", "/"), "~0", "~");
	}

	private JsonSchema ResolveReferences(JsonSchema schema)
	{
		if (schema.DeferredReference != null)
		{
			string text = schema.DeferredReference;
			bool flag = text.StartsWith("#", StringComparison.Ordinal);
			if (flag)
			{
				text = this.UnescapeReference(text);
			}
			JsonSchema jsonSchema = this._resolver.GetSchema(text);
			if (jsonSchema == null)
			{
				if (flag)
				{
					string[] array = schema.DeferredReference.TrimStart('#').Split(new char[1] { '/' }, StringSplitOptions.RemoveEmptyEntries);
					JToken jToken = this._rootSchema;
					string[] array2 = array;
					foreach (string reference in array2)
					{
						string text2 = this.UnescapeReference(reference);
						if (jToken.Type == JTokenType.Object)
						{
							jToken = jToken[text2];
						}
						else if (jToken.Type == JTokenType.Array || jToken.Type == JTokenType.Constructor)
						{
							jToken = ((!int.TryParse(text2, out var result) || result < 0 || result >= jToken.Count()) ? null : jToken[result]);
						}
						if (jToken == null)
						{
							break;
						}
					}
					if (jToken != null)
					{
						jsonSchema = this.BuildSchema(jToken);
					}
				}
				if (jsonSchema == null)
				{
					throw new JsonException("Could not resolve schema reference '{0}'.".FormatWith(CultureInfo.InvariantCulture, schema.DeferredReference));
				}
			}
			schema = jsonSchema;
		}
		if (schema.ReferencesResolved)
		{
			return schema;
		}
		schema.ReferencesResolved = true;
		if (schema.Extends != null)
		{
			for (int j = 0; j < schema.Extends.Count; j++)
			{
				schema.Extends[j] = this.ResolveReferences(schema.Extends[j]);
			}
		}
		if (schema.Items != null)
		{
			for (int k = 0; k < schema.Items.Count; k++)
			{
				schema.Items[k] = this.ResolveReferences(schema.Items[k]);
			}
		}
		if (schema.AdditionalItems != null)
		{
			schema.AdditionalItems = this.ResolveReferences(schema.AdditionalItems);
		}
		if (schema.PatternProperties != null)
		{
			foreach (KeyValuePair<string, JsonSchema> item in schema.PatternProperties.ToList())
			{
				schema.PatternProperties[item.Key] = this.ResolveReferences(item.Value);
			}
		}
		if (schema.Properties != null)
		{
			foreach (KeyValuePair<string, JsonSchema> item2 in schema.Properties.ToList())
			{
				schema.Properties[item2.Key] = this.ResolveReferences(item2.Value);
			}
		}
		if (schema.AdditionalProperties != null)
		{
			schema.AdditionalProperties = this.ResolveReferences(schema.AdditionalProperties);
		}
		return schema;
	}

	private JsonSchema BuildSchema(JToken token)
	{
		if (!(token is JObject jObject))
		{
			throw JsonException.Create(token, token.Path, "Expected object while parsing schema object, got {0}.".FormatWith(CultureInfo.InvariantCulture, token.Type));
		}
		if (jObject.TryGetValue("$ref", out JToken value))
		{
			return new JsonSchema
			{
				DeferredReference = (string?)value
			};
		}
		string path = token.Path;
		path = StringUtils.Replace(path, ".", "/");
		path = StringUtils.Replace(path, "[", "/");
		path = StringUtils.Replace(path, "]", string.Empty);
		if (!StringUtils.IsNullOrEmpty(path))
		{
			path = "/" + path;
		}
		path = "#" + path;
		if (this._documentSchemas.TryGetValue(path, out var value2))
		{
			return value2;
		}
		this.Push(new JsonSchema
		{
			Location = path
		});
		this.ProcessSchemaProperties(jObject);
		return this.Pop();
	}

	private void ProcessSchemaProperties(JObject schemaObject)
	{
		foreach (KeyValuePair<string, JToken> item in schemaObject)
		{
			switch (item.Key)
			{
			case "type":
				this.CurrentSchema.Type = this.ProcessType(item.Value);
				break;
			case "id":
				this.CurrentSchema.Id = (string?)item.Value;
				break;
			case "title":
				this.CurrentSchema.Title = (string?)item.Value;
				break;
			case "description":
				this.CurrentSchema.Description = (string?)item.Value;
				break;
			case "properties":
				this.CurrentSchema.Properties = this.ProcessProperties(item.Value);
				break;
			case "items":
				this.ProcessItems(item.Value);
				break;
			case "additionalProperties":
				this.ProcessAdditionalProperties(item.Value);
				break;
			case "additionalItems":
				this.ProcessAdditionalItems(item.Value);
				break;
			case "patternProperties":
				this.CurrentSchema.PatternProperties = this.ProcessProperties(item.Value);
				break;
			case "required":
				this.CurrentSchema.Required = (bool)item.Value;
				break;
			case "requires":
				this.CurrentSchema.Requires = (string?)item.Value;
				break;
			case "minimum":
				this.CurrentSchema.Minimum = (double)item.Value;
				break;
			case "maximum":
				this.CurrentSchema.Maximum = (double)item.Value;
				break;
			case "exclusiveMinimum":
				this.CurrentSchema.ExclusiveMinimum = (bool)item.Value;
				break;
			case "exclusiveMaximum":
				this.CurrentSchema.ExclusiveMaximum = (bool)item.Value;
				break;
			case "maxLength":
				this.CurrentSchema.MaximumLength = (int)item.Value;
				break;
			case "minLength":
				this.CurrentSchema.MinimumLength = (int)item.Value;
				break;
			case "maxItems":
				this.CurrentSchema.MaximumItems = (int)item.Value;
				break;
			case "minItems":
				this.CurrentSchema.MinimumItems = (int)item.Value;
				break;
			case "divisibleBy":
				this.CurrentSchema.DivisibleBy = (double)item.Value;
				break;
			case "disallow":
				this.CurrentSchema.Disallow = this.ProcessType(item.Value);
				break;
			case "default":
				this.CurrentSchema.Default = item.Value.DeepClone();
				break;
			case "hidden":
				this.CurrentSchema.Hidden = (bool)item.Value;
				break;
			case "readonly":
				this.CurrentSchema.ReadOnly = (bool)item.Value;
				break;
			case "format":
				this.CurrentSchema.Format = (string?)item.Value;
				break;
			case "pattern":
				this.CurrentSchema.Pattern = (string?)item.Value;
				break;
			case "enum":
				this.ProcessEnum(item.Value);
				break;
			case "extends":
				this.ProcessExtends(item.Value);
				break;
			case "uniqueItems":
				this.CurrentSchema.UniqueItems = (bool)item.Value;
				break;
			}
		}
	}

	private void ProcessExtends(JToken token)
	{
		IList<JsonSchema> list = new List<JsonSchema>();
		if (token.Type == JTokenType.Array)
		{
			foreach (JToken item in (IEnumerable<JToken>)token)
			{
				list.Add(this.BuildSchema(item));
			}
		}
		else
		{
			JsonSchema jsonSchema = this.BuildSchema(token);
			if (jsonSchema != null)
			{
				list.Add(jsonSchema);
			}
		}
		if (list.Count > 0)
		{
			this.CurrentSchema.Extends = list;
		}
	}

	private void ProcessEnum(JToken token)
	{
		if (token.Type != JTokenType.Array)
		{
			throw JsonException.Create(token, token.Path, "Expected Array token while parsing enum values, got {0}.".FormatWith(CultureInfo.InvariantCulture, token.Type));
		}
		this.CurrentSchema.Enum = new List<JToken>();
		foreach (JToken item in (IEnumerable<JToken>)token)
		{
			this.CurrentSchema.Enum.Add(item.DeepClone());
		}
	}

	private void ProcessAdditionalProperties(JToken token)
	{
		if (token.Type == JTokenType.Boolean)
		{
			this.CurrentSchema.AllowAdditionalProperties = (bool)token;
		}
		else
		{
			this.CurrentSchema.AdditionalProperties = this.BuildSchema(token);
		}
	}

	private void ProcessAdditionalItems(JToken token)
	{
		if (token.Type == JTokenType.Boolean)
		{
			this.CurrentSchema.AllowAdditionalItems = (bool)token;
		}
		else
		{
			this.CurrentSchema.AdditionalItems = this.BuildSchema(token);
		}
	}

	private IDictionary<string, JsonSchema> ProcessProperties(JToken token)
	{
		IDictionary<string, JsonSchema> dictionary = new Dictionary<string, JsonSchema>();
		if (token.Type != JTokenType.Object)
		{
			throw JsonException.Create(token, token.Path, "Expected Object token while parsing schema properties, got {0}.".FormatWith(CultureInfo.InvariantCulture, token.Type));
		}
		foreach (JProperty item in (IEnumerable<JToken>)token)
		{
			if (dictionary.ContainsKey(item.Name))
			{
				throw new JsonException("Property {0} has already been defined in schema.".FormatWith(CultureInfo.InvariantCulture, item.Name));
			}
			dictionary.Add(item.Name, this.BuildSchema(item.Value));
		}
		return dictionary;
	}

	private void ProcessItems(JToken token)
	{
		this.CurrentSchema.Items = new List<JsonSchema>();
		switch (token.Type)
		{
		case JTokenType.Object:
			this.CurrentSchema.Items.Add(this.BuildSchema(token));
			this.CurrentSchema.PositionalItemsValidation = false;
			break;
		case JTokenType.Array:
			this.CurrentSchema.PositionalItemsValidation = true;
			{
				foreach (JToken item in (IEnumerable<JToken>)token)
				{
					this.CurrentSchema.Items.Add(this.BuildSchema(item));
				}
				break;
			}
		default:
			throw JsonException.Create(token, token.Path, "Expected array or JSON schema object, got {0}.".FormatWith(CultureInfo.InvariantCulture, token.Type));
		}
	}

	private JsonSchemaType? ProcessType(JToken token)
	{
		switch (token.Type)
		{
		case JTokenType.Array:
		{
			JsonSchemaType? jsonSchemaType = JsonSchemaType.None;
			{
				foreach (JToken item in (IEnumerable<JToken>)token)
				{
					if (item.Type != JTokenType.String)
					{
						throw JsonException.Create(item, item.Path, "Expected JSON schema type string token, got {0}.".FormatWith(CultureInfo.InvariantCulture, token.Type));
					}
					jsonSchemaType |= JsonSchemaBuilder.MapType((string?)item);
				}
				return jsonSchemaType;
			}
		}
		case JTokenType.String:
			return JsonSchemaBuilder.MapType((string?)token);
		default:
			throw JsonException.Create(token, token.Path, "Expected array or JSON schema type string token, got {0}.".FormatWith(CultureInfo.InvariantCulture, token.Type));
		}
	}

	internal static JsonSchemaType MapType(string type)
	{
		if (!JsonSchemaConstants.JsonSchemaTypeMapping.TryGetValue(type, out var value))
		{
			throw new JsonException("Invalid JSON schema type: {0}".FormatWith(CultureInfo.InvariantCulture, type));
		}
		return value;
	}

	internal static string MapType(JsonSchemaType type)
	{
		return JsonSchemaConstants.JsonSchemaTypeMapping.Single((KeyValuePair<string, JsonSchemaType> kv) => kv.Value == type).Key;
	}
}
