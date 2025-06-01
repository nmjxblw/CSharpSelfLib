using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json;

/// <summary>
/// <para>
/// Represents a reader that provides <see cref="T:Newtonsoft.Json.Schema.JsonSchema" /> validation.
/// </para>
/// <note type="caution">
/// JSON Schema validation has been moved to its own package. See <see href="https://www.newtonsoft.com/jsonschema">https://www.newtonsoft.com/jsonschema</see> for more details.
/// </note>
/// </summary>
[Obsolete("JSON Schema validation has been moved to its own package. See https://www.newtonsoft.com/jsonschema for more details.")]
public class JsonValidatingReader : JsonReader, IJsonLineInfo
{
	private class SchemaScope
	{
		private readonly JTokenType _tokenType;

		private readonly IList<JsonSchemaModel> _schemas;

		private readonly Dictionary<string, bool> _requiredProperties;

		public string CurrentPropertyName { get; set; }

		public int ArrayItemCount { get; set; }

		public bool IsUniqueArray { get; }

		public IList<JToken> UniqueArrayItems { get; }

		public JTokenWriter CurrentItemWriter { get; set; }

		public IList<JsonSchemaModel> Schemas => this._schemas;

		public Dictionary<string, bool> RequiredProperties => this._requiredProperties;

		public JTokenType TokenType => this._tokenType;

		public SchemaScope(JTokenType tokenType, IList<JsonSchemaModel> schemas)
		{
			this._tokenType = tokenType;
			this._schemas = schemas;
			this._requiredProperties = schemas.SelectMany(GetRequiredProperties).Distinct().ToDictionary((string p) => p, (string p) => false);
			if (tokenType == JTokenType.Array && schemas.Any((JsonSchemaModel s) => s.UniqueItems))
			{
				this.IsUniqueArray = true;
				this.UniqueArrayItems = new List<JToken>();
			}
		}

		private IEnumerable<string> GetRequiredProperties(JsonSchemaModel schema)
		{
			if (schema?.Properties == null)
			{
				return Enumerable.Empty<string>();
			}
			return from p in schema.Properties
				where p.Value.Required
				select p.Key;
		}
	}

	private readonly JsonReader _reader;

	private readonly Stack<SchemaScope> _stack;

	private JsonSchema _schema;

	private JsonSchemaModel _model;

	private SchemaScope _currentScope;

	private static readonly IList<JsonSchemaModel> EmptySchemaList = new List<JsonSchemaModel>();

	/// <summary>
	/// Gets the text value of the current JSON token.
	/// </summary>
	/// <value></value>
	public override object Value => this._reader.Value;

	/// <summary>
	/// Gets the depth of the current token in the JSON document.
	/// </summary>
	/// <value>The depth of the current token in the JSON document.</value>
	public override int Depth => this._reader.Depth;

	/// <summary>
	/// Gets the path of the current JSON token. 
	/// </summary>
	public override string Path => this._reader.Path;

	/// <summary>
	/// Gets the quotation mark character used to enclose the value of a string.
	/// </summary>
	/// <value></value>
	public override char QuoteChar
	{
		get
		{
			return this._reader.QuoteChar;
		}
		protected internal set
		{
		}
	}

	/// <summary>
	/// Gets the type of the current JSON token.
	/// </summary>
	/// <value></value>
	public override JsonToken TokenType => this._reader.TokenType;

	/// <summary>
	/// Gets the .NET type for the current JSON token.
	/// </summary>
	/// <value></value>
	public override Type ValueType => this._reader.ValueType;

	private IList<JsonSchemaModel> CurrentSchemas => this._currentScope.Schemas;

	private IList<JsonSchemaModel> CurrentMemberSchemas
	{
		get
		{
			if (this._currentScope == null)
			{
				return new List<JsonSchemaModel>(new JsonSchemaModel[1] { this._model });
			}
			if (this._currentScope.Schemas == null || this._currentScope.Schemas.Count == 0)
			{
				return JsonValidatingReader.EmptySchemaList;
			}
			switch (this._currentScope.TokenType)
			{
			case JTokenType.None:
				return this._currentScope.Schemas;
			case JTokenType.Object:
			{
				if (this._currentScope.CurrentPropertyName == null)
				{
					throw new JsonReaderException("CurrentPropertyName has not been set on scope.");
				}
				IList<JsonSchemaModel> list2 = new List<JsonSchemaModel>();
				{
					foreach (JsonSchemaModel currentSchema in this.CurrentSchemas)
					{
						if (currentSchema.Properties != null && currentSchema.Properties.TryGetValue(this._currentScope.CurrentPropertyName, out var value))
						{
							list2.Add(value);
						}
						if (currentSchema.PatternProperties != null)
						{
							foreach (KeyValuePair<string, JsonSchemaModel> patternProperty in currentSchema.PatternProperties)
							{
								if (Regex.IsMatch(this._currentScope.CurrentPropertyName, patternProperty.Key))
								{
									list2.Add(patternProperty.Value);
								}
							}
						}
						if (list2.Count == 0 && currentSchema.AllowAdditionalProperties && currentSchema.AdditionalProperties != null)
						{
							list2.Add(currentSchema.AdditionalProperties);
						}
					}
					return list2;
				}
			}
			case JTokenType.Array:
			{
				IList<JsonSchemaModel> list = new List<JsonSchemaModel>();
				{
					foreach (JsonSchemaModel currentSchema2 in this.CurrentSchemas)
					{
						if (!currentSchema2.PositionalItemsValidation)
						{
							if (currentSchema2.Items != null && currentSchema2.Items.Count > 0)
							{
								list.Add(currentSchema2.Items[0]);
							}
							continue;
						}
						if (currentSchema2.Items != null && currentSchema2.Items.Count > 0 && currentSchema2.Items.Count > this._currentScope.ArrayItemCount - 1)
						{
							list.Add(currentSchema2.Items[this._currentScope.ArrayItemCount - 1]);
						}
						if (currentSchema2.AllowAdditionalItems && currentSchema2.AdditionalItems != null)
						{
							list.Add(currentSchema2.AdditionalItems);
						}
					}
					return list;
				}
			}
			case JTokenType.Constructor:
				return JsonValidatingReader.EmptySchemaList;
			default:
				throw new ArgumentOutOfRangeException("TokenType", "Unexpected token type: {0}".FormatWith(CultureInfo.InvariantCulture, this._currentScope.TokenType));
			}
		}
	}

	/// <summary>
	/// Gets or sets the schema.
	/// </summary>
	/// <value>The schema.</value>
	public JsonSchema Schema
	{
		get
		{
			return this._schema;
		}
		set
		{
			if (this.TokenType != JsonToken.None)
			{
				throw new InvalidOperationException("Cannot change schema while validating JSON.");
			}
			this._schema = value;
			this._model = null;
		}
	}

	/// <summary>
	/// Gets the <see cref="T:Newtonsoft.Json.JsonReader" /> used to construct this <see cref="T:Newtonsoft.Json.JsonValidatingReader" />.
	/// </summary>
	/// <value>The <see cref="T:Newtonsoft.Json.JsonReader" /> specified in the constructor.</value>
	public JsonReader Reader => this._reader;

	int IJsonLineInfo.LineNumber
	{
		get
		{
			if (!(this._reader is IJsonLineInfo jsonLineInfo))
			{
				return 0;
			}
			return jsonLineInfo.LineNumber;
		}
	}

	int IJsonLineInfo.LinePosition
	{
		get
		{
			if (!(this._reader is IJsonLineInfo jsonLineInfo))
			{
				return 0;
			}
			return jsonLineInfo.LinePosition;
		}
	}

	/// <summary>
	/// Sets an event handler for receiving schema validation errors.
	/// </summary>
	public event ValidationEventHandler ValidationEventHandler;

	private void Push(SchemaScope scope)
	{
		this._stack.Push(scope);
		this._currentScope = scope;
	}

	private SchemaScope Pop()
	{
		SchemaScope result = this._stack.Pop();
		this._currentScope = ((this._stack.Count != 0) ? this._stack.Peek() : null);
		return result;
	}

	private void RaiseError(string message, JsonSchemaModel schema)
	{
		string message2 = (((IJsonLineInfo)this).HasLineInfo() ? (message + " Line {0}, position {1}.".FormatWith(CultureInfo.InvariantCulture, ((IJsonLineInfo)this).LineNumber, ((IJsonLineInfo)this).LinePosition)) : message);
		this.OnValidationEvent(new JsonSchemaException(message2, null, this.Path, ((IJsonLineInfo)this).LineNumber, ((IJsonLineInfo)this).LinePosition));
	}

	private void OnValidationEvent(JsonSchemaException exception)
	{
		ValidationEventHandler validationEventHandler = this.ValidationEventHandler;
		if (validationEventHandler != null)
		{
			validationEventHandler(this, new ValidationEventArgs(exception));
			return;
		}
		throw exception;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:Newtonsoft.Json.JsonValidatingReader" /> class that
	/// validates the content returned from the given <see cref="T:Newtonsoft.Json.JsonReader" />.
	/// </summary>
	/// <param name="reader">The <see cref="T:Newtonsoft.Json.JsonReader" /> to read from while validating.</param>
	public JsonValidatingReader(JsonReader reader)
	{
		ValidationUtils.ArgumentNotNull(reader, "reader");
		this._reader = reader;
		this._stack = new Stack<SchemaScope>();
	}

	/// <summary>
	/// Changes the reader's state to <see cref="F:Newtonsoft.Json.JsonReader.State.Closed" />.
	/// If <see cref="P:Newtonsoft.Json.JsonReader.CloseInput" /> is set to <c>true</c>, the underlying <see cref="T:Newtonsoft.Json.JsonReader" /> is also closed.
	/// </summary>
	public override void Close()
	{
		base.Close();
		if (base.CloseInput)
		{
			this._reader?.Close();
		}
	}

	private void ValidateNotDisallowed(JsonSchemaModel schema)
	{
		if (schema != null)
		{
			JsonSchemaType? currentNodeSchemaType = this.GetCurrentNodeSchemaType();
			if (currentNodeSchemaType.HasValue && JsonSchemaGenerator.HasFlag(schema.Disallow, currentNodeSchemaType.GetValueOrDefault()))
			{
				this.RaiseError("Type {0} is disallowed.".FormatWith(CultureInfo.InvariantCulture, currentNodeSchemaType), schema);
			}
		}
	}

	private JsonSchemaType? GetCurrentNodeSchemaType()
	{
		return this._reader.TokenType switch
		{
			JsonToken.StartObject => JsonSchemaType.Object, 
			JsonToken.StartArray => JsonSchemaType.Array, 
			JsonToken.Integer => JsonSchemaType.Integer, 
			JsonToken.Float => JsonSchemaType.Float, 
			JsonToken.String => JsonSchemaType.String, 
			JsonToken.Boolean => JsonSchemaType.Boolean, 
			JsonToken.Null => JsonSchemaType.Null, 
			_ => null, 
		};
	}

	/// <summary>
	/// Reads the next JSON token from the underlying <see cref="T:Newtonsoft.Json.JsonReader" /> as a <see cref="T:System.Nullable`1" /> of <see cref="T:System.Int32" />.
	/// </summary>
	/// <returns>A <see cref="T:System.Nullable`1" /> of <see cref="T:System.Int32" />.</returns>
	public override int? ReadAsInt32()
	{
		int? result = this._reader.ReadAsInt32();
		this.ValidateCurrentToken();
		return result;
	}

	/// <summary>
	/// Reads the next JSON token from the underlying <see cref="T:Newtonsoft.Json.JsonReader" /> as a <see cref="T:System.Byte" />[].
	/// </summary>
	/// <returns>
	/// A <see cref="T:System.Byte" />[] or <c>null</c> if the next JSON token is null.
	/// </returns>
	public override byte[] ReadAsBytes()
	{
		byte[]? result = this._reader.ReadAsBytes();
		this.ValidateCurrentToken();
		return result;
	}

	/// <summary>
	/// Reads the next JSON token from the underlying <see cref="T:Newtonsoft.Json.JsonReader" /> as a <see cref="T:System.Nullable`1" /> of <see cref="T:System.Decimal" />.
	/// </summary>
	/// <returns>A <see cref="T:System.Nullable`1" /> of <see cref="T:System.Decimal" />.</returns>
	public override decimal? ReadAsDecimal()
	{
		decimal? result = this._reader.ReadAsDecimal();
		this.ValidateCurrentToken();
		return result;
	}

	/// <summary>
	/// Reads the next JSON token from the underlying <see cref="T:Newtonsoft.Json.JsonReader" /> as a <see cref="T:System.Nullable`1" /> of <see cref="T:System.Double" />.
	/// </summary>
	/// <returns>A <see cref="T:System.Nullable`1" /> of <see cref="T:System.Double" />.</returns>
	public override double? ReadAsDouble()
	{
		double? result = this._reader.ReadAsDouble();
		this.ValidateCurrentToken();
		return result;
	}

	/// <summary>
	/// Reads the next JSON token from the underlying <see cref="T:Newtonsoft.Json.JsonReader" /> as a <see cref="T:System.Nullable`1" /> of <see cref="T:System.Boolean" />.
	/// </summary>
	/// <returns>A <see cref="T:System.Nullable`1" /> of <see cref="T:System.Boolean" />.</returns>
	public override bool? ReadAsBoolean()
	{
		bool? result = this._reader.ReadAsBoolean();
		this.ValidateCurrentToken();
		return result;
	}

	/// <summary>
	/// Reads the next JSON token from the underlying <see cref="T:Newtonsoft.Json.JsonReader" /> as a <see cref="T:System.String" />.
	/// </summary>
	/// <returns>A <see cref="T:System.String" />. This method will return <c>null</c> at the end of an array.</returns>
	public override string ReadAsString()
	{
		string? result = this._reader.ReadAsString();
		this.ValidateCurrentToken();
		return result;
	}

	/// <summary>
	/// Reads the next JSON token from the underlying <see cref="T:Newtonsoft.Json.JsonReader" /> as a <see cref="T:System.Nullable`1" /> of <see cref="T:System.DateTime" />.
	/// </summary>
	/// <returns>A <see cref="T:System.Nullable`1" /> of <see cref="T:System.DateTime" />. This method will return <c>null</c> at the end of an array.</returns>
	public override DateTime? ReadAsDateTime()
	{
		DateTime? result = this._reader.ReadAsDateTime();
		this.ValidateCurrentToken();
		return result;
	}

	/// <summary>
	/// Reads the next JSON token from the underlying <see cref="T:Newtonsoft.Json.JsonReader" /> as a <see cref="T:System.Nullable`1" /> of <see cref="T:System.DateTimeOffset" />.
	/// </summary>
	/// <returns>A <see cref="T:System.Nullable`1" /> of <see cref="T:System.DateTimeOffset" />.</returns>
	public override DateTimeOffset? ReadAsDateTimeOffset()
	{
		DateTimeOffset? result = this._reader.ReadAsDateTimeOffset();
		this.ValidateCurrentToken();
		return result;
	}

	/// <summary>
	/// Reads the next JSON token from the underlying <see cref="T:Newtonsoft.Json.JsonReader" />.
	/// </summary>
	/// <returns>
	/// <c>true</c> if the next token was read successfully; <c>false</c> if there are no more tokens to read.
	/// </returns>
	public override bool Read()
	{
		if (!this._reader.Read())
		{
			return false;
		}
		if (this._reader.TokenType == JsonToken.Comment)
		{
			return true;
		}
		this.ValidateCurrentToken();
		return true;
	}

	private void ValidateCurrentToken()
	{
		if (this._model == null)
		{
			JsonSchemaModelBuilder jsonSchemaModelBuilder = new JsonSchemaModelBuilder();
			this._model = jsonSchemaModelBuilder.Build(this._schema);
			if (!JsonTokenUtils.IsStartToken(this._reader.TokenType))
			{
				this.Push(new SchemaScope(JTokenType.None, this.CurrentMemberSchemas));
			}
		}
		switch (this._reader.TokenType)
		{
		case JsonToken.StartObject:
		{
			this.ProcessValue();
			IList<JsonSchemaModel> schemas2 = this.CurrentMemberSchemas.Where(ValidateObject).ToList();
			this.Push(new SchemaScope(JTokenType.Object, schemas2));
			this.WriteToken(this.CurrentSchemas);
			break;
		}
		case JsonToken.StartArray:
		{
			this.ProcessValue();
			IList<JsonSchemaModel> schemas = this.CurrentMemberSchemas.Where(ValidateArray).ToList();
			this.Push(new SchemaScope(JTokenType.Array, schemas));
			this.WriteToken(this.CurrentSchemas);
			break;
		}
		case JsonToken.StartConstructor:
			this.ProcessValue();
			this.Push(new SchemaScope(JTokenType.Constructor, null));
			this.WriteToken(this.CurrentSchemas);
			break;
		case JsonToken.PropertyName:
			this.WriteToken(this.CurrentSchemas);
			{
				foreach (JsonSchemaModel currentSchema in this.CurrentSchemas)
				{
					this.ValidatePropertyName(currentSchema);
				}
				break;
			}
		case JsonToken.Raw:
			this.ProcessValue();
			break;
		case JsonToken.Integer:
			this.ProcessValue();
			this.WriteToken(this.CurrentMemberSchemas);
			{
				foreach (JsonSchemaModel currentMemberSchema in this.CurrentMemberSchemas)
				{
					this.ValidateInteger(currentMemberSchema);
				}
				break;
			}
		case JsonToken.Float:
			this.ProcessValue();
			this.WriteToken(this.CurrentMemberSchemas);
			{
				foreach (JsonSchemaModel currentMemberSchema2 in this.CurrentMemberSchemas)
				{
					this.ValidateFloat(currentMemberSchema2);
				}
				break;
			}
		case JsonToken.String:
			this.ProcessValue();
			this.WriteToken(this.CurrentMemberSchemas);
			{
				foreach (JsonSchemaModel currentMemberSchema3 in this.CurrentMemberSchemas)
				{
					this.ValidateString(currentMemberSchema3);
				}
				break;
			}
		case JsonToken.Boolean:
			this.ProcessValue();
			this.WriteToken(this.CurrentMemberSchemas);
			{
				foreach (JsonSchemaModel currentMemberSchema4 in this.CurrentMemberSchemas)
				{
					this.ValidateBoolean(currentMemberSchema4);
				}
				break;
			}
		case JsonToken.Null:
			this.ProcessValue();
			this.WriteToken(this.CurrentMemberSchemas);
			{
				foreach (JsonSchemaModel currentMemberSchema5 in this.CurrentMemberSchemas)
				{
					this.ValidateNull(currentMemberSchema5);
				}
				break;
			}
		case JsonToken.EndObject:
			this.WriteToken(this.CurrentSchemas);
			foreach (JsonSchemaModel currentSchema2 in this.CurrentSchemas)
			{
				this.ValidateEndObject(currentSchema2);
			}
			this.Pop();
			break;
		case JsonToken.EndArray:
			this.WriteToken(this.CurrentSchemas);
			foreach (JsonSchemaModel currentSchema3 in this.CurrentSchemas)
			{
				this.ValidateEndArray(currentSchema3);
			}
			this.Pop();
			break;
		case JsonToken.EndConstructor:
			this.WriteToken(this.CurrentSchemas);
			this.Pop();
			break;
		case JsonToken.Undefined:
		case JsonToken.Date:
		case JsonToken.Bytes:
			this.WriteToken(this.CurrentMemberSchemas);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case JsonToken.None:
			break;
		}
	}

	private void WriteToken(IList<JsonSchemaModel> schemas)
	{
		foreach (SchemaScope item in this._stack)
		{
			bool flag = item.TokenType == JTokenType.Array && item.IsUniqueArray && item.ArrayItemCount > 0;
			if (!flag && !schemas.Any((JsonSchemaModel s) => s.Enum != null))
			{
				continue;
			}
			if (item.CurrentItemWriter == null)
			{
				if (JsonTokenUtils.IsEndToken(this._reader.TokenType))
				{
					continue;
				}
				item.CurrentItemWriter = new JTokenWriter();
			}
			item.CurrentItemWriter.WriteToken(this._reader, writeChildren: false);
			if (item.CurrentItemWriter.Top != 0 || this._reader.TokenType == JsonToken.PropertyName)
			{
				continue;
			}
			JToken token = item.CurrentItemWriter.Token;
			item.CurrentItemWriter = null;
			if (flag)
			{
				if (item.UniqueArrayItems.Contains(token, JToken.EqualityComparer))
				{
					this.RaiseError("Non-unique array item at index {0}.".FormatWith(CultureInfo.InvariantCulture, item.ArrayItemCount - 1), item.Schemas.First((JsonSchemaModel s) => s.UniqueItems));
				}
				item.UniqueArrayItems.Add(token);
			}
			else
			{
				if (!schemas.Any((JsonSchemaModel s) => s.Enum != null))
				{
					continue;
				}
				foreach (JsonSchemaModel schema in schemas)
				{
					if (schema.Enum != null && !schema.Enum.ContainsValue(token, JToken.EqualityComparer))
					{
						StringWriter stringWriter = new StringWriter(CultureInfo.InvariantCulture);
						token.WriteTo(new JsonTextWriter(stringWriter));
						this.RaiseError("Value {0} is not defined in enum.".FormatWith(CultureInfo.InvariantCulture, stringWriter.ToString()), schema);
					}
				}
			}
		}
	}

	private void ValidateEndObject(JsonSchemaModel schema)
	{
		if (schema == null)
		{
			return;
		}
		Dictionary<string, bool> requiredProperties = this._currentScope.RequiredProperties;
		if (requiredProperties != null && requiredProperties.Values.Any((bool v) => !v))
		{
			IEnumerable<string> values = from kv in requiredProperties
				where !kv.Value
				select kv.Key;
			this.RaiseError("Required properties are missing from object: {0}.".FormatWith(CultureInfo.InvariantCulture, string.Join(", ", values)), schema);
		}
	}

	private void ValidateEndArray(JsonSchemaModel schema)
	{
		if (schema != null)
		{
			int arrayItemCount = this._currentScope.ArrayItemCount;
			if (schema.MaximumItems.HasValue && arrayItemCount > schema.MaximumItems)
			{
				this.RaiseError("Array item count {0} exceeds maximum count of {1}.".FormatWith(CultureInfo.InvariantCulture, arrayItemCount, schema.MaximumItems), schema);
			}
			if (schema.MinimumItems.HasValue && arrayItemCount < schema.MinimumItems)
			{
				this.RaiseError("Array item count {0} is less than minimum count of {1}.".FormatWith(CultureInfo.InvariantCulture, arrayItemCount, schema.MinimumItems), schema);
			}
		}
	}

	private void ValidateNull(JsonSchemaModel schema)
	{
		if (schema != null && this.TestType(schema, JsonSchemaType.Null))
		{
			this.ValidateNotDisallowed(schema);
		}
	}

	private void ValidateBoolean(JsonSchemaModel schema)
	{
		if (schema != null && this.TestType(schema, JsonSchemaType.Boolean))
		{
			this.ValidateNotDisallowed(schema);
		}
	}

	private void ValidateString(JsonSchemaModel schema)
	{
		if (schema == null || !this.TestType(schema, JsonSchemaType.String))
		{
			return;
		}
		this.ValidateNotDisallowed(schema);
		string text = this._reader.Value.ToString();
		if (schema.MaximumLength.HasValue && text.Length > schema.MaximumLength)
		{
			this.RaiseError("String '{0}' exceeds maximum length of {1}.".FormatWith(CultureInfo.InvariantCulture, text, schema.MaximumLength), schema);
		}
		if (schema.MinimumLength.HasValue && text.Length < schema.MinimumLength)
		{
			this.RaiseError("String '{0}' is less than minimum length of {1}.".FormatWith(CultureInfo.InvariantCulture, text, schema.MinimumLength), schema);
		}
		if (schema.Patterns == null)
		{
			return;
		}
		foreach (string pattern in schema.Patterns)
		{
			if (!Regex.IsMatch(text, pattern))
			{
				this.RaiseError("String '{0}' does not match regex pattern '{1}'.".FormatWith(CultureInfo.InvariantCulture, text, pattern), schema);
			}
		}
	}

	private void ValidateInteger(JsonSchemaModel schema)
	{
		if (schema == null || !this.TestType(schema, JsonSchemaType.Integer))
		{
			return;
		}
		this.ValidateNotDisallowed(schema);
		object value = this._reader.Value;
		if (schema.Maximum.HasValue)
		{
			if (JValue.Compare(JTokenType.Integer, value, schema.Maximum) > 0)
			{
				this.RaiseError("Integer {0} exceeds maximum value of {1}.".FormatWith(CultureInfo.InvariantCulture, value, schema.Maximum), schema);
			}
			if (schema.ExclusiveMaximum && JValue.Compare(JTokenType.Integer, value, schema.Maximum) == 0)
			{
				this.RaiseError("Integer {0} equals maximum value of {1} and exclusive maximum is true.".FormatWith(CultureInfo.InvariantCulture, value, schema.Maximum), schema);
			}
		}
		if (schema.Minimum.HasValue)
		{
			if (JValue.Compare(JTokenType.Integer, value, schema.Minimum) < 0)
			{
				this.RaiseError("Integer {0} is less than minimum value of {1}.".FormatWith(CultureInfo.InvariantCulture, value, schema.Minimum), schema);
			}
			if (schema.ExclusiveMinimum && JValue.Compare(JTokenType.Integer, value, schema.Minimum) == 0)
			{
				this.RaiseError("Integer {0} equals minimum value of {1} and exclusive minimum is true.".FormatWith(CultureInfo.InvariantCulture, value, schema.Minimum), schema);
			}
		}
		if (schema.DivisibleBy.HasValue && ((!(value is BigInteger bigInteger)) ? (!JsonValidatingReader.IsZero((double)Convert.ToInt64(value, CultureInfo.InvariantCulture) % schema.DivisibleBy.GetValueOrDefault())) : (Math.Abs(schema.DivisibleBy.Value - Math.Truncate(schema.DivisibleBy.Value)).Equals(0.0) ? (bigInteger % new BigInteger(schema.DivisibleBy.Value) != 0L) : (bigInteger != 0L))))
		{
			this.RaiseError("Integer {0} is not evenly divisible by {1}.".FormatWith(CultureInfo.InvariantCulture, JsonConvert.ToString(value), schema.DivisibleBy), schema);
		}
	}

	private void ProcessValue()
	{
		if (this._currentScope == null || this._currentScope.TokenType != JTokenType.Array)
		{
			return;
		}
		this._currentScope.ArrayItemCount++;
		foreach (JsonSchemaModel currentSchema in this.CurrentSchemas)
		{
			if (currentSchema != null && currentSchema.PositionalItemsValidation && !currentSchema.AllowAdditionalItems && (currentSchema.Items == null || this._currentScope.ArrayItemCount - 1 >= currentSchema.Items.Count))
			{
				this.RaiseError("Index {0} has not been defined and the schema does not allow additional items.".FormatWith(CultureInfo.InvariantCulture, this._currentScope.ArrayItemCount), currentSchema);
			}
		}
	}

	private void ValidateFloat(JsonSchemaModel schema)
	{
		if (schema == null || !this.TestType(schema, JsonSchemaType.Float))
		{
			return;
		}
		this.ValidateNotDisallowed(schema);
		double num = Convert.ToDouble(this._reader.Value, CultureInfo.InvariantCulture);
		if (schema.Maximum.HasValue)
		{
			if (num > schema.Maximum)
			{
				this.RaiseError("Float {0} exceeds maximum value of {1}.".FormatWith(CultureInfo.InvariantCulture, JsonConvert.ToString(num), schema.Maximum), schema);
			}
			if (schema.ExclusiveMaximum && num == schema.Maximum)
			{
				this.RaiseError("Float {0} equals maximum value of {1} and exclusive maximum is true.".FormatWith(CultureInfo.InvariantCulture, JsonConvert.ToString(num), schema.Maximum), schema);
			}
		}
		if (schema.Minimum.HasValue)
		{
			if (num < schema.Minimum)
			{
				this.RaiseError("Float {0} is less than minimum value of {1}.".FormatWith(CultureInfo.InvariantCulture, JsonConvert.ToString(num), schema.Minimum), schema);
			}
			if (schema.ExclusiveMinimum && num == schema.Minimum)
			{
				this.RaiseError("Float {0} equals minimum value of {1} and exclusive minimum is true.".FormatWith(CultureInfo.InvariantCulture, JsonConvert.ToString(num), schema.Minimum), schema);
			}
		}
		if (schema.DivisibleBy.HasValue && !JsonValidatingReader.IsZero(JsonValidatingReader.FloatingPointRemainder(num, schema.DivisibleBy.GetValueOrDefault())))
		{
			this.RaiseError("Float {0} is not evenly divisible by {1}.".FormatWith(CultureInfo.InvariantCulture, JsonConvert.ToString(num), schema.DivisibleBy), schema);
		}
	}

	private static double FloatingPointRemainder(double dividend, double divisor)
	{
		return dividend - Math.Floor(dividend / divisor) * divisor;
	}

	private static bool IsZero(double value)
	{
		return Math.Abs(value) < 4.440892098500626E-15;
	}

	private void ValidatePropertyName(JsonSchemaModel schema)
	{
		if (schema != null)
		{
			string text = Convert.ToString(this._reader.Value, CultureInfo.InvariantCulture);
			if (this._currentScope.RequiredProperties.ContainsKey(text))
			{
				this._currentScope.RequiredProperties[text] = true;
			}
			if (!schema.AllowAdditionalProperties && !this.IsPropertyDefinied(schema, text))
			{
				this.RaiseError("Property '{0}' has not been defined and the schema does not allow additional properties.".FormatWith(CultureInfo.InvariantCulture, text), schema);
			}
			this._currentScope.CurrentPropertyName = text;
		}
	}

	private bool IsPropertyDefinied(JsonSchemaModel schema, string propertyName)
	{
		if (schema.Properties != null && schema.Properties.ContainsKey(propertyName))
		{
			return true;
		}
		if (schema.PatternProperties != null)
		{
			foreach (string key in schema.PatternProperties.Keys)
			{
				if (Regex.IsMatch(propertyName, key))
				{
					return true;
				}
			}
		}
		return false;
	}

	private bool ValidateArray(JsonSchemaModel schema)
	{
		if (schema == null)
		{
			return true;
		}
		return this.TestType(schema, JsonSchemaType.Array);
	}

	private bool ValidateObject(JsonSchemaModel schema)
	{
		if (schema == null)
		{
			return true;
		}
		return this.TestType(schema, JsonSchemaType.Object);
	}

	private bool TestType(JsonSchemaModel currentSchema, JsonSchemaType currentType)
	{
		if (!JsonSchemaGenerator.HasFlag(currentSchema.Type, currentType))
		{
			this.RaiseError("Invalid type. Expected {0} but got {1}.".FormatWith(CultureInfo.InvariantCulture, currentSchema.Type, currentType), currentSchema);
			return false;
		}
		return true;
	}

	bool IJsonLineInfo.HasLineInfo()
	{
		if (this._reader is IJsonLineInfo jsonLineInfo)
		{
			return jsonLineInfo.HasLineInfo();
		}
		return false;
	}
}
