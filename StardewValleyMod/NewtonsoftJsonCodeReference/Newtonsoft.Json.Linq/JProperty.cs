using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Linq;

/// <summary>
/// Represents a JSON property.
/// </summary>
public class JProperty : JContainer
{
	private class JPropertyList : IList<JToken>, ICollection<JToken>, IEnumerable<JToken>, IEnumerable
	{
		internal JToken? _token;

		public int Count
		{
			get
			{
				if (this._token == null)
				{
					return 0;
				}
				return 1;
			}
		}

		public bool IsReadOnly => false;

		public JToken this[int index]
		{
			get
			{
				if (index != 0)
				{
					throw new IndexOutOfRangeException();
				}
				return this._token;
			}
			set
			{
				if (index != 0)
				{
					throw new IndexOutOfRangeException();
				}
				this._token = value;
			}
		}

		public IEnumerator<JToken> GetEnumerator()
		{
			if (this._token != null)
			{
				yield return this._token;
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		public void Add(JToken item)
		{
			this._token = item;
		}

		public void Clear()
		{
			this._token = null;
		}

		public bool Contains(JToken item)
		{
			return this._token == item;
		}

		public void CopyTo(JToken[] array, int arrayIndex)
		{
			if (this._token != null)
			{
				array[arrayIndex] = this._token;
			}
		}

		public bool Remove(JToken item)
		{
			if (this._token == item)
			{
				this._token = null;
				return true;
			}
			return false;
		}

		public int IndexOf(JToken item)
		{
			if (this._token != item)
			{
				return -1;
			}
			return 0;
		}

		public void Insert(int index, JToken item)
		{
			if (index == 0)
			{
				this._token = item;
			}
		}

		public void RemoveAt(int index)
		{
			if (index == 0)
			{
				this._token = null;
			}
		}
	}

	private readonly JPropertyList _content = new JPropertyList();

	private readonly string _name;

	/// <summary>
	/// Gets the container's children tokens.
	/// </summary>
	/// <value>The container's children tokens.</value>
	protected override IList<JToken> ChildrenTokens => this._content;

	/// <summary>
	/// Gets the property name.
	/// </summary>
	/// <value>The property name.</value>
	public string Name
	{
		[DebuggerStepThrough]
		get
		{
			return this._name;
		}
	}

	/// <summary>
	/// Gets or sets the property value.
	/// </summary>
	/// <value>The property value.</value>
	public new JToken Value
	{
		[DebuggerStepThrough]
		get
		{
			return this._content._token;
		}
		set
		{
			base.CheckReentrancy();
			JToken item = value ?? JValue.CreateNull();
			if (this._content._token == null)
			{
				this.InsertItem(0, item, skipParentCheck: false, copyAnnotations: true);
			}
			else
			{
				this.SetItem(0, item);
			}
		}
	}

	/// <summary>
	/// Gets the node type for this <see cref="T:Newtonsoft.Json.Linq.JToken" />.
	/// </summary>
	/// <value>The type.</value>
	public override JTokenType Type
	{
		[DebuggerStepThrough]
		get
		{
			return JTokenType.Property;
		}
	}

	/// <summary>
	/// Writes this token to a <see cref="T:Newtonsoft.Json.JsonWriter" /> asynchronously.
	/// </summary>
	/// <param name="writer">A <see cref="T:Newtonsoft.Json.JsonWriter" /> into which this method will write.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
	/// <param name="converters">A collection of <see cref="T:Newtonsoft.Json.JsonConverter" /> which will be used when writing the token.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous write operation.</returns>
	public override Task WriteToAsync(JsonWriter writer, CancellationToken cancellationToken, params JsonConverter[] converters)
	{
		Task task = writer.WritePropertyNameAsync(this._name, cancellationToken);
		if (task.IsCompletedSuccessfully())
		{
			return this.WriteValueAsync(writer, cancellationToken, converters);
		}
		return this.WriteToAsync(task, writer, cancellationToken, converters);
	}

	private async Task WriteToAsync(Task task, JsonWriter writer, CancellationToken cancellationToken, params JsonConverter[] converters)
	{
		await task.ConfigureAwait(continueOnCapturedContext: false);
		await this.WriteValueAsync(writer, cancellationToken, converters).ConfigureAwait(continueOnCapturedContext: false);
	}

	private Task WriteValueAsync(JsonWriter writer, CancellationToken cancellationToken, JsonConverter[] converters)
	{
		JToken value = this.Value;
		if (value == null)
		{
			return writer.WriteNullAsync(cancellationToken);
		}
		return value.WriteToAsync(writer, cancellationToken, converters);
	}

	/// <summary>
	/// Asynchronously loads a <see cref="T:Newtonsoft.Json.Linq.JProperty" /> from a <see cref="T:Newtonsoft.Json.JsonReader" />.
	/// </summary>
	/// <param name="reader">A <see cref="T:Newtonsoft.Json.JsonReader" /> that will be read for the content of the <see cref="T:Newtonsoft.Json.Linq.JProperty" />.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task`1" /> representing the asynchronous creation. The <see cref="P:System.Threading.Tasks.Task`1.Result" />
	/// property returns a <see cref="T:Newtonsoft.Json.Linq.JProperty" /> that contains the JSON that was read from the specified <see cref="T:Newtonsoft.Json.JsonReader" />.</returns>
	public new static Task<JProperty> LoadAsync(JsonReader reader, CancellationToken cancellationToken = default(CancellationToken))
	{
		return JProperty.LoadAsync(reader, null, cancellationToken);
	}

	/// <summary>
	/// Asynchronously loads a <see cref="T:Newtonsoft.Json.Linq.JProperty" /> from a <see cref="T:Newtonsoft.Json.JsonReader" />.
	/// </summary>
	/// <param name="reader">A <see cref="T:Newtonsoft.Json.JsonReader" /> that will be read for the content of the <see cref="T:Newtonsoft.Json.Linq.JProperty" />.</param>
	/// <param name="settings">The <see cref="T:Newtonsoft.Json.Linq.JsonLoadSettings" /> used to load the JSON.
	/// If this is <c>null</c>, default load settings will be used.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task`1" /> representing the asynchronous creation. The <see cref="P:System.Threading.Tasks.Task`1.Result" />
	/// property returns a <see cref="T:Newtonsoft.Json.Linq.JProperty" /> that contains the JSON that was read from the specified <see cref="T:Newtonsoft.Json.JsonReader" />.</returns>
	public new static async Task<JProperty> LoadAsync(JsonReader reader, JsonLoadSettings? settings, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (reader.TokenType == JsonToken.None && !(await reader.ReadAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false)))
		{
			throw JsonReaderException.Create(reader, "Error reading JProperty from JsonReader.");
		}
		await reader.MoveToContentAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		if (reader.TokenType != JsonToken.PropertyName)
		{
			throw JsonReaderException.Create(reader, "Error reading JProperty from JsonReader. Current JsonReader item is not a property: {0}".FormatWith(CultureInfo.InvariantCulture, reader.TokenType));
		}
		JProperty p = new JProperty((string)reader.Value);
		p.SetLineInfo(reader as IJsonLineInfo, settings);
		await p.ReadTokenFromAsync(reader, settings, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		return p;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:Newtonsoft.Json.Linq.JProperty" /> class from another <see cref="T:Newtonsoft.Json.Linq.JProperty" /> object.
	/// </summary>
	/// <param name="other">A <see cref="T:Newtonsoft.Json.Linq.JProperty" /> object to copy from.</param>
	public JProperty(JProperty other)
		: base(other, null)
	{
		this._name = other.Name;
	}

	internal JProperty(JProperty other, JsonCloneSettings? settings)
		: base(other, settings)
	{
		this._name = other.Name;
	}

	internal override JToken GetItem(int index)
	{
		if (index != 0)
		{
			throw new ArgumentOutOfRangeException();
		}
		return this.Value;
	}

	internal override void SetItem(int index, JToken? item)
	{
		if (index != 0)
		{
			throw new ArgumentOutOfRangeException();
		}
		if (!JContainer.IsTokenUnchanged(this.Value, item))
		{
			((JObject)base.Parent)?.InternalPropertyChanging(this);
			base.SetItem(0, item);
			((JObject)base.Parent)?.InternalPropertyChanged(this);
		}
	}

	internal override bool RemoveItem(JToken? item)
	{
		throw new JsonException("Cannot add or remove items from {0}.".FormatWith(CultureInfo.InvariantCulture, typeof(JProperty)));
	}

	internal override void RemoveItemAt(int index)
	{
		throw new JsonException("Cannot add or remove items from {0}.".FormatWith(CultureInfo.InvariantCulture, typeof(JProperty)));
	}

	internal override int IndexOfItem(JToken? item)
	{
		if (item == null)
		{
			return -1;
		}
		return this._content.IndexOf(item);
	}

	internal override bool InsertItem(int index, JToken? item, bool skipParentCheck, bool copyAnnotations)
	{
		if (item != null && item.Type == JTokenType.Comment)
		{
			return false;
		}
		if (this.Value != null)
		{
			throw new JsonException("{0} cannot have multiple values.".FormatWith(CultureInfo.InvariantCulture, typeof(JProperty)));
		}
		return base.InsertItem(0, item, skipParentCheck: false, copyAnnotations);
	}

	internal override bool ContainsItem(JToken? item)
	{
		return this.Value == item;
	}

	internal override void MergeItem(object content, JsonMergeSettings? settings)
	{
		JToken jToken = (content as JProperty)?.Value;
		if (jToken != null && jToken.Type != JTokenType.Null)
		{
			this.Value = jToken;
		}
	}

	internal override void ClearItems()
	{
		throw new JsonException("Cannot add or remove items from {0}.".FormatWith(CultureInfo.InvariantCulture, typeof(JProperty)));
	}

	internal override bool DeepEquals(JToken node)
	{
		if (node is JProperty jProperty && this._name == jProperty.Name)
		{
			return base.ContentsEqual(jProperty);
		}
		return false;
	}

	internal override JToken CloneToken(JsonCloneSettings? settings)
	{
		return new JProperty(this, settings);
	}

	internal JProperty(string name)
	{
		ValidationUtils.ArgumentNotNull(name, "name");
		this._name = name;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:Newtonsoft.Json.Linq.JProperty" /> class.
	/// </summary>
	/// <param name="name">The property name.</param>
	/// <param name="content">The property content.</param>
	public JProperty(string name, params object[] content)
		: this(name, (object?)content)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:Newtonsoft.Json.Linq.JProperty" /> class.
	/// </summary>
	/// <param name="name">The property name.</param>
	/// <param name="content">The property content.</param>
	public JProperty(string name, object? content)
	{
		ValidationUtils.ArgumentNotNull(name, "name");
		this._name = name;
		this.Value = (base.IsMultiContent(content) ? new JArray(content) : JContainer.CreateFromContent(content));
	}

	/// <summary>
	/// Writes this token to a <see cref="T:Newtonsoft.Json.JsonWriter" />.
	/// </summary>
	/// <param name="writer">A <see cref="T:Newtonsoft.Json.JsonWriter" /> into which this method will write.</param>
	/// <param name="converters">A collection of <see cref="T:Newtonsoft.Json.JsonConverter" /> which will be used when writing the token.</param>
	public override void WriteTo(JsonWriter writer, params JsonConverter[] converters)
	{
		writer.WritePropertyName(this._name);
		JToken value = this.Value;
		if (value != null)
		{
			value.WriteTo(writer, converters);
		}
		else
		{
			writer.WriteNull();
		}
	}

	internal override int GetDeepHashCode()
	{
		return this._name.GetHashCode(StringComparison.Ordinal) ^ (this.Value?.GetDeepHashCode() ?? 0);
	}

	/// <summary>
	/// Loads a <see cref="T:Newtonsoft.Json.Linq.JProperty" /> from a <see cref="T:Newtonsoft.Json.JsonReader" />.
	/// </summary>
	/// <param name="reader">A <see cref="T:Newtonsoft.Json.JsonReader" /> that will be read for the content of the <see cref="T:Newtonsoft.Json.Linq.JProperty" />.</param>
	/// <returns>A <see cref="T:Newtonsoft.Json.Linq.JProperty" /> that contains the JSON that was read from the specified <see cref="T:Newtonsoft.Json.JsonReader" />.</returns>
	public new static JProperty Load(JsonReader reader)
	{
		return JProperty.Load(reader, null);
	}

	/// <summary>
	/// Loads a <see cref="T:Newtonsoft.Json.Linq.JProperty" /> from a <see cref="T:Newtonsoft.Json.JsonReader" />.
	/// </summary>
	/// <param name="reader">A <see cref="T:Newtonsoft.Json.JsonReader" /> that will be read for the content of the <see cref="T:Newtonsoft.Json.Linq.JProperty" />.</param>
	/// <param name="settings">The <see cref="T:Newtonsoft.Json.Linq.JsonLoadSettings" /> used to load the JSON.
	/// If this is <c>null</c>, default load settings will be used.</param>
	/// <returns>A <see cref="T:Newtonsoft.Json.Linq.JProperty" /> that contains the JSON that was read from the specified <see cref="T:Newtonsoft.Json.JsonReader" />.</returns>
	public new static JProperty Load(JsonReader reader, JsonLoadSettings? settings)
	{
		if (reader.TokenType == JsonToken.None && !reader.Read())
		{
			throw JsonReaderException.Create(reader, "Error reading JProperty from JsonReader.");
		}
		reader.MoveToContent();
		if (reader.TokenType != JsonToken.PropertyName)
		{
			throw JsonReaderException.Create(reader, "Error reading JProperty from JsonReader. Current JsonReader item is not a property: {0}".FormatWith(CultureInfo.InvariantCulture, reader.TokenType));
		}
		JProperty jProperty = new JProperty((string)reader.Value);
		jProperty.SetLineInfo(reader as IJsonLineInfo, settings);
		jProperty.ReadTokenFrom(reader, settings);
		return jProperty;
	}
}
