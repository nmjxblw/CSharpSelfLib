using System;
using System.Globalization;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Linq;

/// <summary>
/// Represents a writer that provides a fast, non-cached, forward-only way of generating JSON data.
/// </summary>
public class JTokenWriter : JsonWriter
{
	private JContainer? _token;

	private JContainer? _parent;

	private JValue? _value;

	private JToken? _current;

	/// <summary>
	/// Gets the <see cref="T:Newtonsoft.Json.Linq.JToken" /> at the writer's current position.
	/// </summary>
	public JToken? CurrentToken => this._current;

	/// <summary>
	/// Gets the token being written.
	/// </summary>
	/// <value>The token being written.</value>
	public JToken? Token
	{
		get
		{
			if (this._token != null)
			{
				return this._token;
			}
			return this._value;
		}
	}

	internal override Task WriteTokenAsync(JsonReader reader, bool writeChildren, bool writeDateConstructorAsDate, bool writeComments, CancellationToken cancellationToken)
	{
		if (reader is JTokenReader)
		{
			this.WriteToken(reader, writeChildren, writeDateConstructorAsDate, writeComments);
			return AsyncUtils.CompletedTask;
		}
		return base.WriteTokenSyncReadingAsync(reader, cancellationToken);
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:Newtonsoft.Json.Linq.JTokenWriter" /> class writing to the given <see cref="T:Newtonsoft.Json.Linq.JContainer" />.
	/// </summary>
	/// <param name="container">The container being written to.</param>
	public JTokenWriter(JContainer container)
	{
		ValidationUtils.ArgumentNotNull(container, "container");
		this._token = container;
		this._parent = container;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:Newtonsoft.Json.Linq.JTokenWriter" /> class.
	/// </summary>
	public JTokenWriter()
	{
	}

	/// <summary>
	/// Flushes whatever is in the buffer to the underlying <see cref="T:Newtonsoft.Json.Linq.JContainer" />.
	/// </summary>
	public override void Flush()
	{
	}

	/// <summary>
	/// Closes this writer.
	/// If <see cref="P:Newtonsoft.Json.JsonWriter.AutoCompleteOnClose" /> is set to <c>true</c>, the JSON is auto-completed.
	/// </summary>
	/// <remarks>
	/// Setting <see cref="P:Newtonsoft.Json.JsonWriter.CloseOutput" /> to <c>true</c> has no additional effect, since the underlying <see cref="T:Newtonsoft.Json.Linq.JContainer" /> is a type that cannot be closed.
	/// </remarks>
	public override void Close()
	{
		base.Close();
	}

	/// <summary>
	/// Writes the beginning of a JSON object.
	/// </summary>
	public override void WriteStartObject()
	{
		base.WriteStartObject();
		this.AddParent(new JObject());
	}

	private void AddParent(JContainer container)
	{
		if (this._parent == null)
		{
			this._token = container;
		}
		else
		{
			this._parent.AddAndSkipParentCheck(container);
		}
		this._parent = container;
		this._current = container;
	}

	private void RemoveParent()
	{
		this._current = this._parent;
		this._parent = this._parent.Parent;
		if (this._parent != null && this._parent.Type == JTokenType.Property)
		{
			this._parent = this._parent.Parent;
		}
	}

	/// <summary>
	/// Writes the beginning of a JSON array.
	/// </summary>
	public override void WriteStartArray()
	{
		base.WriteStartArray();
		this.AddParent(new JArray());
	}

	/// <summary>
	/// Writes the start of a constructor with the given name.
	/// </summary>
	/// <param name="name">The name of the constructor.</param>
	public override void WriteStartConstructor(string name)
	{
		base.WriteStartConstructor(name);
		this.AddParent(new JConstructor(name));
	}

	/// <summary>
	/// Writes the end.
	/// </summary>
	/// <param name="token">The token.</param>
	protected override void WriteEnd(JsonToken token)
	{
		this.RemoveParent();
	}

	/// <summary>
	/// Writes the property name of a name/value pair on a JSON object.
	/// </summary>
	/// <param name="name">The name of the property.</param>
	public override void WritePropertyName(string name)
	{
		(this._parent as JObject)?.Remove(name);
		this.AddParent(new JProperty(name));
		base.WritePropertyName(name);
	}

	private void AddRawValue(object? value, JTokenType type, JsonToken token)
	{
		this.AddJValue(new JValue(value, type), token);
	}

	internal void AddJValue(JValue? value, JsonToken token)
	{
		if (this._parent != null)
		{
			if (this._parent.TryAdd(value))
			{
				this._current = this._parent.Last;
				if (this._parent.Type == JTokenType.Property)
				{
					this._parent = this._parent.Parent;
				}
			}
		}
		else
		{
			this._value = value ?? JValue.CreateNull();
			this._current = this._value;
		}
	}

	/// <summary>
	/// Writes a <see cref="T:System.Object" /> value.
	/// An error will be raised if the value cannot be written as a single JSON token.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Object" /> value to write.</param>
	public override void WriteValue(object? value)
	{
		if (value is BigInteger)
		{
			base.InternalWriteValue(JsonToken.Integer);
			this.AddRawValue(value, JTokenType.Integer, JsonToken.Integer);
		}
		else
		{
			base.WriteValue(value);
		}
	}

	/// <summary>
	/// Writes a null value.
	/// </summary>
	public override void WriteNull()
	{
		base.WriteNull();
		this.AddJValue(JValue.CreateNull(), JsonToken.Null);
	}

	/// <summary>
	/// Writes an undefined value.
	/// </summary>
	public override void WriteUndefined()
	{
		base.WriteUndefined();
		this.AddJValue(JValue.CreateUndefined(), JsonToken.Undefined);
	}

	/// <summary>
	/// Writes raw JSON.
	/// </summary>
	/// <param name="json">The raw JSON to write.</param>
	public override void WriteRaw(string? json)
	{
		base.WriteRaw(json);
		this.AddJValue(new JRaw(json), JsonToken.Raw);
	}

	/// <summary>
	/// Writes a comment <c>/*...*/</c> containing the specified text.
	/// </summary>
	/// <param name="text">Text to place inside the comment.</param>
	public override void WriteComment(string? text)
	{
		base.WriteComment(text);
		this.AddJValue(JValue.CreateComment(text), JsonToken.Comment);
	}

	/// <summary>
	/// Writes a <see cref="T:System.String" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.String" /> value to write.</param>
	public override void WriteValue(string? value)
	{
		if (value == null)
		{
			this.WriteNull();
			return;
		}
		base.WriteValue(value);
		this.AddJValue(new JValue(value), JsonToken.String);
	}

	/// <summary>
	/// Writes a <see cref="T:System.Int32" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Int32" /> value to write.</param>
	public override void WriteValue(int value)
	{
		base.WriteValue(value);
		this.AddRawValue(value, JTokenType.Integer, JsonToken.Integer);
	}

	/// <summary>
	/// Writes a <see cref="T:System.UInt32" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.UInt32" /> value to write.</param>
	[CLSCompliant(false)]
	public override void WriteValue(uint value)
	{
		base.WriteValue(value);
		this.AddRawValue(value, JTokenType.Integer, JsonToken.Integer);
	}

	/// <summary>
	/// Writes a <see cref="T:System.Int64" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Int64" /> value to write.</param>
	public override void WriteValue(long value)
	{
		base.WriteValue(value);
		this.AddJValue(new JValue(value), JsonToken.Integer);
	}

	/// <summary>
	/// Writes a <see cref="T:System.UInt64" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.UInt64" /> value to write.</param>
	[CLSCompliant(false)]
	public override void WriteValue(ulong value)
	{
		base.WriteValue(value);
		this.AddJValue(new JValue(value), JsonToken.Integer);
	}

	/// <summary>
	/// Writes a <see cref="T:System.Single" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Single" /> value to write.</param>
	public override void WriteValue(float value)
	{
		base.WriteValue(value);
		this.AddJValue(new JValue(value), JsonToken.Float);
	}

	/// <summary>
	/// Writes a <see cref="T:System.Double" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Double" /> value to write.</param>
	public override void WriteValue(double value)
	{
		base.WriteValue(value);
		this.AddJValue(new JValue(value), JsonToken.Float);
	}

	/// <summary>
	/// Writes a <see cref="T:System.Boolean" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Boolean" /> value to write.</param>
	public override void WriteValue(bool value)
	{
		base.WriteValue(value);
		this.AddJValue(new JValue(value), JsonToken.Boolean);
	}

	/// <summary>
	/// Writes a <see cref="T:System.Int16" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Int16" /> value to write.</param>
	public override void WriteValue(short value)
	{
		base.WriteValue(value);
		this.AddRawValue(value, JTokenType.Integer, JsonToken.Integer);
	}

	/// <summary>
	/// Writes a <see cref="T:System.UInt16" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.UInt16" /> value to write.</param>
	[CLSCompliant(false)]
	public override void WriteValue(ushort value)
	{
		base.WriteValue(value);
		this.AddRawValue(value, JTokenType.Integer, JsonToken.Integer);
	}

	/// <summary>
	/// Writes a <see cref="T:System.Char" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Char" /> value to write.</param>
	public override void WriteValue(char value)
	{
		base.WriteValue(value);
		string value2 = value.ToString(CultureInfo.InvariantCulture);
		this.AddJValue(new JValue(value2), JsonToken.String);
	}

	/// <summary>
	/// Writes a <see cref="T:System.Byte" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Byte" /> value to write.</param>
	public override void WriteValue(byte value)
	{
		base.WriteValue(value);
		this.AddRawValue(value, JTokenType.Integer, JsonToken.Integer);
	}

	/// <summary>
	/// Writes a <see cref="T:System.SByte" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.SByte" /> value to write.</param>
	[CLSCompliant(false)]
	public override void WriteValue(sbyte value)
	{
		base.WriteValue(value);
		this.AddRawValue(value, JTokenType.Integer, JsonToken.Integer);
	}

	/// <summary>
	/// Writes a <see cref="T:System.Decimal" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Decimal" /> value to write.</param>
	public override void WriteValue(decimal value)
	{
		base.WriteValue(value);
		this.AddJValue(new JValue(value), JsonToken.Float);
	}

	/// <summary>
	/// Writes a <see cref="T:System.DateTime" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.DateTime" /> value to write.</param>
	public override void WriteValue(DateTime value)
	{
		base.WriteValue(value);
		value = DateTimeUtils.EnsureDateTime(value, base.DateTimeZoneHandling);
		this.AddJValue(new JValue(value), JsonToken.Date);
	}

	/// <summary>
	/// Writes a <see cref="T:System.DateTimeOffset" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.DateTimeOffset" /> value to write.</param>
	public override void WriteValue(DateTimeOffset value)
	{
		base.WriteValue(value);
		this.AddJValue(new JValue(value), JsonToken.Date);
	}

	/// <summary>
	/// Writes a <see cref="T:System.Byte" />[] value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Byte" />[] value to write.</param>
	public override void WriteValue(byte[]? value)
	{
		base.WriteValue(value);
		this.AddJValue(new JValue(value, JTokenType.Bytes), JsonToken.Bytes);
	}

	/// <summary>
	/// Writes a <see cref="T:System.TimeSpan" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.TimeSpan" /> value to write.</param>
	public override void WriteValue(TimeSpan value)
	{
		base.WriteValue(value);
		this.AddJValue(new JValue(value), JsonToken.String);
	}

	/// <summary>
	/// Writes a <see cref="T:System.Guid" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Guid" /> value to write.</param>
	public override void WriteValue(Guid value)
	{
		base.WriteValue(value);
		this.AddJValue(new JValue(value), JsonToken.String);
	}

	/// <summary>
	/// Writes a <see cref="T:System.Uri" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Uri" /> value to write.</param>
	public override void WriteValue(Uri? value)
	{
		base.WriteValue(value);
		this.AddJValue(new JValue(value), JsonToken.String);
	}

	internal override void WriteToken(JsonReader reader, bool writeChildren, bool writeDateConstructorAsDate, bool writeComments)
	{
		JTokenReader jTokenReader = reader as JTokenReader;
		if (jTokenReader != null && writeChildren && writeDateConstructorAsDate && writeComments)
		{
			if (jTokenReader.TokenType == JsonToken.None && !jTokenReader.Read())
			{
				return;
			}
			JToken jToken = jTokenReader.CurrentToken.CloneToken(null);
			if (this._parent != null)
			{
				this._parent.Add(jToken);
				this._current = this._parent.Last;
				if (this._parent.Type == JTokenType.Property)
				{
					this._parent = this._parent.Parent;
					base.InternalWriteValue(JsonToken.Null);
				}
			}
			else
			{
				this._current = jToken;
				if (this._token == null && this._value == null)
				{
					this._token = jToken as JContainer;
					this._value = jToken as JValue;
				}
			}
			jTokenReader.Skip();
		}
		else
		{
			base.WriteToken(reader, writeChildren, writeDateConstructorAsDate, writeComments);
		}
	}
}
