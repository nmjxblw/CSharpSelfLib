using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Linq;

/// <summary>
/// Represents a JSON object.
/// </summary>
/// <example>
///   <code lang="cs" source="..\Src\Newtonsoft.Json.Tests\Documentation\LinqToJsonTests.cs" region="LinqToJsonCreateParse" title="Parsing a JSON Object from Text" />
/// </example>
public class JObject : JContainer, IDictionary<string, JToken?>, ICollection<KeyValuePair<string, JToken?>>, IEnumerable<KeyValuePair<string, JToken?>>, IEnumerable, INotifyPropertyChanged, ICustomTypeDescriptor, INotifyPropertyChanging
{
	private class JObjectDynamicProxy : DynamicProxy<JObject>
	{
		public override bool TryGetMember(JObject instance, GetMemberBinder binder, out object? result)
		{
			result = instance[binder.Name];
			return true;
		}

		public override bool TrySetMember(JObject instance, SetMemberBinder binder, object value)
		{
			JToken jToken = value as JToken;
			if (jToken == null)
			{
				jToken = new JValue(value);
			}
			instance[binder.Name] = jToken;
			return true;
		}

		public override IEnumerable<string> GetDynamicMemberNames(JObject instance)
		{
			return from p in instance.Properties()
				select p.Name;
		}
	}

	private readonly JPropertyKeyedCollection _properties = new JPropertyKeyedCollection();

	/// <summary>
	/// Gets the container's children tokens.
	/// </summary>
	/// <value>The container's children tokens.</value>
	protected override IList<JToken> ChildrenTokens => this._properties;

	/// <summary>
	/// Gets the node type for this <see cref="T:Newtonsoft.Json.Linq.JToken" />.
	/// </summary>
	/// <value>The type.</value>
	public override JTokenType Type => JTokenType.Object;

	/// <summary>
	/// Gets the <see cref="T:Newtonsoft.Json.Linq.JToken" /> with the specified key.
	/// </summary>
	/// <value>The <see cref="T:Newtonsoft.Json.Linq.JToken" /> with the specified key.</value>
	public override JToken? this[object key]
	{
		get
		{
			ValidationUtils.ArgumentNotNull(key, "key");
			if (!(key is string propertyName))
			{
				throw new ArgumentException("Accessed JObject values with invalid key value: {0}. Object property name expected.".FormatWith(CultureInfo.InvariantCulture, MiscellaneousUtils.ToString(key)));
			}
			return this[propertyName];
		}
		set
		{
			ValidationUtils.ArgumentNotNull(key, "key");
			if (!(key is string propertyName))
			{
				throw new ArgumentException("Set JObject values with invalid key value: {0}. Object property name expected.".FormatWith(CultureInfo.InvariantCulture, MiscellaneousUtils.ToString(key)));
			}
			this[propertyName] = value;
		}
	}

	/// <summary>
	/// Gets or sets the <see cref="T:Newtonsoft.Json.Linq.JToken" /> with the specified property name.
	/// </summary>
	/// <value></value>
	public JToken? this[string propertyName]
	{
		get
		{
			ValidationUtils.ArgumentNotNull(propertyName, "propertyName");
			return this.Property(propertyName, StringComparison.Ordinal)?.Value;
		}
		set
		{
			JProperty jProperty = this.Property(propertyName, StringComparison.Ordinal);
			if (jProperty != null)
			{
				jProperty.Value = value;
				return;
			}
			this.OnPropertyChanging(propertyName);
			this.Add(propertyName, value);
			this.OnPropertyChanged(propertyName);
		}
	}

	ICollection<string> IDictionary<string, JToken>.Keys => this._properties.Keys;

	ICollection<JToken?> IDictionary<string, JToken>.Values
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	bool ICollection<KeyValuePair<string, JToken>>.IsReadOnly => false;

	/// <summary>
	/// Occurs when a property value changes.
	/// </summary>
	public event PropertyChangedEventHandler? PropertyChanged;

	/// <summary>
	/// Occurs when a property value is changing.
	/// </summary>
	public event PropertyChangingEventHandler? PropertyChanging;

	/// <summary>
	/// Writes this token to a <see cref="T:Newtonsoft.Json.JsonWriter" /> asynchronously.
	/// </summary>
	/// <param name="writer">A <see cref="T:Newtonsoft.Json.JsonWriter" /> into which this method will write.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
	/// <param name="converters">A collection of <see cref="T:Newtonsoft.Json.JsonConverter" /> which will be used when writing the token.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous write operation.</returns>
	public override Task WriteToAsync(JsonWriter writer, CancellationToken cancellationToken, params JsonConverter[] converters)
	{
		Task task = writer.WriteStartObjectAsync(cancellationToken);
		if (!task.IsCompletedSuccessfully())
		{
			return AwaitProperties(task, 0, writer, cancellationToken, converters);
		}
		for (int i = 0; i < this._properties.Count; i++)
		{
			task = this._properties[i].WriteToAsync(writer, cancellationToken, converters);
			if (!task.IsCompletedSuccessfully())
			{
				return AwaitProperties(task, i + 1, writer, cancellationToken, converters);
			}
		}
		return writer.WriteEndObjectAsync(cancellationToken);
		async Task AwaitProperties(Task task2, int num, JsonWriter Writer, CancellationToken CancellationToken, JsonConverter[] Converters)
		{
			await task2.ConfigureAwait(continueOnCapturedContext: false);
			while (num < this._properties.Count)
			{
				await this._properties[num].WriteToAsync(Writer, CancellationToken, Converters).ConfigureAwait(continueOnCapturedContext: false);
				num++;
			}
			await Writer.WriteEndObjectAsync(CancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
	}

	/// <summary>
	/// Asynchronously loads a <see cref="T:Newtonsoft.Json.Linq.JObject" /> from a <see cref="T:Newtonsoft.Json.JsonReader" />.
	/// </summary>
	/// <param name="reader">A <see cref="T:Newtonsoft.Json.JsonReader" /> that will be read for the content of the <see cref="T:Newtonsoft.Json.Linq.JObject" />.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>
	/// A <see cref="T:System.Threading.Tasks.Task`1" /> that represents the asynchronous load. The <see cref="P:System.Threading.Tasks.Task`1.Result" />
	/// property returns a <see cref="T:Newtonsoft.Json.Linq.JObject" /> that contains the JSON that was read from the specified <see cref="T:Newtonsoft.Json.JsonReader" />.</returns>
	public new static Task<JObject> LoadAsync(JsonReader reader, CancellationToken cancellationToken = default(CancellationToken))
	{
		return JObject.LoadAsync(reader, null, cancellationToken);
	}

	/// <summary>
	/// Asynchronously loads a <see cref="T:Newtonsoft.Json.Linq.JObject" /> from a <see cref="T:Newtonsoft.Json.JsonReader" />.
	/// </summary>
	/// <param name="reader">A <see cref="T:Newtonsoft.Json.JsonReader" /> that will be read for the content of the <see cref="T:Newtonsoft.Json.Linq.JObject" />.</param>
	/// <param name="settings">The <see cref="T:Newtonsoft.Json.Linq.JsonLoadSettings" /> used to load the JSON.
	/// If this is <c>null</c>, default load settings will be used.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>
	/// A <see cref="T:System.Threading.Tasks.Task`1" /> that represents the asynchronous load. The <see cref="P:System.Threading.Tasks.Task`1.Result" />
	/// property returns a <see cref="T:Newtonsoft.Json.Linq.JObject" /> that contains the JSON that was read from the specified <see cref="T:Newtonsoft.Json.JsonReader" />.</returns>
	public new static async Task<JObject> LoadAsync(JsonReader reader, JsonLoadSettings? settings, CancellationToken cancellationToken = default(CancellationToken))
	{
		ValidationUtils.ArgumentNotNull(reader, "reader");
		if (reader.TokenType == JsonToken.None && !(await reader.ReadAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false)))
		{
			throw JsonReaderException.Create(reader, "Error reading JObject from JsonReader.");
		}
		await reader.MoveToContentAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		if (reader.TokenType != JsonToken.StartObject)
		{
			throw JsonReaderException.Create(reader, "Error reading JObject from JsonReader. Current JsonReader item is not an object: {0}".FormatWith(CultureInfo.InvariantCulture, reader.TokenType));
		}
		JObject o = new JObject();
		o.SetLineInfo(reader as IJsonLineInfo, settings);
		await o.ReadTokenFromAsync(reader, settings, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		return o;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:Newtonsoft.Json.Linq.JObject" /> class.
	/// </summary>
	public JObject()
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:Newtonsoft.Json.Linq.JObject" /> class from another <see cref="T:Newtonsoft.Json.Linq.JObject" /> object.
	/// </summary>
	/// <param name="other">A <see cref="T:Newtonsoft.Json.Linq.JObject" /> object to copy from.</param>
	public JObject(JObject other)
		: base(other, null)
	{
	}

	internal JObject(JObject other, JsonCloneSettings? settings)
		: base(other, settings)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:Newtonsoft.Json.Linq.JObject" /> class with the specified content.
	/// </summary>
	/// <param name="content">The contents of the object.</param>
	public JObject(params object[] content)
		: this((object)content)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:Newtonsoft.Json.Linq.JObject" /> class with the specified content.
	/// </summary>
	/// <param name="content">The contents of the object.</param>
	public JObject(object content)
	{
		this.Add(content);
	}

	internal override bool DeepEquals(JToken node)
	{
		if (!(node is JObject jObject))
		{
			return false;
		}
		return this._properties.Compare(jObject._properties);
	}

	internal override int IndexOfItem(JToken? item)
	{
		if (item == null)
		{
			return -1;
		}
		return this._properties.IndexOfReference(item);
	}

	internal override bool InsertItem(int index, JToken? item, bool skipParentCheck, bool copyAnnotations)
	{
		if (item != null && item.Type == JTokenType.Comment)
		{
			return false;
		}
		return base.InsertItem(index, item, skipParentCheck, copyAnnotations);
	}

	internal override void ValidateToken(JToken o, JToken? existing)
	{
		ValidationUtils.ArgumentNotNull(o, "o");
		if (o.Type != JTokenType.Property)
		{
			throw new ArgumentException("Can not add {0} to {1}.".FormatWith(CultureInfo.InvariantCulture, o.GetType(), base.GetType()));
		}
		JProperty jProperty = (JProperty)o;
		if (existing != null)
		{
			JProperty jProperty2 = (JProperty)existing;
			if (jProperty.Name == jProperty2.Name)
			{
				return;
			}
		}
		if (this._properties.TryGetValue(jProperty.Name, out existing))
		{
			throw new ArgumentException("Can not add property {0} to {1}. Property with the same name already exists on object.".FormatWith(CultureInfo.InvariantCulture, jProperty.Name, base.GetType()));
		}
	}

	internal override void MergeItem(object content, JsonMergeSettings? settings)
	{
		if (!(content is JObject jObject))
		{
			return;
		}
		foreach (KeyValuePair<string, JToken> item in jObject)
		{
			JProperty jProperty = this.Property(item.Key, settings?.PropertyNameComparison ?? StringComparison.Ordinal);
			if (jProperty == null)
			{
				this.Add(item.Key, item.Value);
			}
			else
			{
				if (item.Value == null)
				{
					continue;
				}
				if (!(jProperty.Value is JContainer jContainer) || jContainer.Type != item.Value.Type)
				{
					if (!JObject.IsNull(item.Value) || (settings != null && settings.MergeNullValueHandling == MergeNullValueHandling.Merge))
					{
						jProperty.Value = item.Value;
					}
				}
				else
				{
					jContainer.Merge(item.Value, settings);
				}
			}
		}
	}

	private static bool IsNull(JToken token)
	{
		if (token.Type == JTokenType.Null)
		{
			return true;
		}
		if (token is JValue { Value: null })
		{
			return true;
		}
		return false;
	}

	internal void InternalPropertyChanged(JProperty childProperty)
	{
		this.OnPropertyChanged(childProperty.Name);
		if (base._listChanged != null)
		{
			this.OnListChanged(new ListChangedEventArgs(ListChangedType.ItemChanged, this.IndexOfItem(childProperty)));
		}
		if (base._collectionChanged != null)
		{
			this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, childProperty, childProperty, this.IndexOfItem(childProperty)));
		}
	}

	internal void InternalPropertyChanging(JProperty childProperty)
	{
		this.OnPropertyChanging(childProperty.Name);
	}

	internal override JToken CloneToken(JsonCloneSettings? settings)
	{
		return new JObject(this, settings);
	}

	/// <summary>
	/// Gets an <see cref="T:System.Collections.Generic.IEnumerable`1" /> of <see cref="T:Newtonsoft.Json.Linq.JProperty" /> of this object's properties.
	/// </summary>
	/// <returns>An <see cref="T:System.Collections.Generic.IEnumerable`1" /> of <see cref="T:Newtonsoft.Json.Linq.JProperty" /> of this object's properties.</returns>
	public IEnumerable<JProperty> Properties()
	{
		return this._properties.Cast<JProperty>();
	}

	/// <summary>
	/// Gets a <see cref="T:Newtonsoft.Json.Linq.JProperty" /> with the specified name.
	/// </summary>
	/// <param name="name">The property name.</param>
	/// <returns>A <see cref="T:Newtonsoft.Json.Linq.JProperty" /> with the specified name or <c>null</c>.</returns>
	public JProperty? Property(string name)
	{
		return this.Property(name, StringComparison.Ordinal);
	}

	/// <summary>
	/// Gets the <see cref="T:Newtonsoft.Json.Linq.JProperty" /> with the specified name.
	/// The exact name will be searched for first and if no matching property is found then
	/// the <see cref="T:System.StringComparison" /> will be used to match a property.
	/// </summary>
	/// <param name="name">The property name.</param>
	/// <param name="comparison">One of the enumeration values that specifies how the strings will be compared.</param>
	/// <returns>A <see cref="T:Newtonsoft.Json.Linq.JProperty" /> matched with the specified name or <c>null</c>.</returns>
	public JProperty? Property(string name, StringComparison comparison)
	{
		if (name == null)
		{
			return null;
		}
		if (this._properties.TryGetValue(name, out JToken value))
		{
			return (JProperty)value;
		}
		if (comparison != StringComparison.Ordinal)
		{
			for (int i = 0; i < this._properties.Count; i++)
			{
				JProperty jProperty = (JProperty)this._properties[i];
				if (string.Equals(jProperty.Name, name, comparison))
				{
					return jProperty;
				}
			}
		}
		return null;
	}

	/// <summary>
	/// Gets a <see cref="T:Newtonsoft.Json.Linq.JEnumerable`1" /> of <see cref="T:Newtonsoft.Json.Linq.JToken" /> of this object's property values.
	/// </summary>
	/// <returns>A <see cref="T:Newtonsoft.Json.Linq.JEnumerable`1" /> of <see cref="T:Newtonsoft.Json.Linq.JToken" /> of this object's property values.</returns>
	public JEnumerable<JToken> PropertyValues()
	{
		return new JEnumerable<JToken>(from p in this.Properties()
			select p.Value);
	}

	/// <summary>
	/// Loads a <see cref="T:Newtonsoft.Json.Linq.JObject" /> from a <see cref="T:Newtonsoft.Json.JsonReader" />.
	/// </summary>
	/// <param name="reader">A <see cref="T:Newtonsoft.Json.JsonReader" /> that will be read for the content of the <see cref="T:Newtonsoft.Json.Linq.JObject" />.</param>
	/// <returns>A <see cref="T:Newtonsoft.Json.Linq.JObject" /> that contains the JSON that was read from the specified <see cref="T:Newtonsoft.Json.JsonReader" />.</returns>
	/// <exception cref="T:Newtonsoft.Json.JsonReaderException">
	///     <paramref name="reader" /> is not valid JSON.
	/// </exception>
	public new static JObject Load(JsonReader reader)
	{
		return JObject.Load(reader, null);
	}

	/// <summary>
	/// Loads a <see cref="T:Newtonsoft.Json.Linq.JObject" /> from a <see cref="T:Newtonsoft.Json.JsonReader" />.
	/// </summary>
	/// <param name="reader">A <see cref="T:Newtonsoft.Json.JsonReader" /> that will be read for the content of the <see cref="T:Newtonsoft.Json.Linq.JObject" />.</param>
	/// <param name="settings">The <see cref="T:Newtonsoft.Json.Linq.JsonLoadSettings" /> used to load the JSON.
	/// If this is <c>null</c>, default load settings will be used.</param>
	/// <returns>A <see cref="T:Newtonsoft.Json.Linq.JObject" /> that contains the JSON that was read from the specified <see cref="T:Newtonsoft.Json.JsonReader" />.</returns>
	/// <exception cref="T:Newtonsoft.Json.JsonReaderException">
	///     <paramref name="reader" /> is not valid JSON.
	/// </exception>
	public new static JObject Load(JsonReader reader, JsonLoadSettings? settings)
	{
		ValidationUtils.ArgumentNotNull(reader, "reader");
		if (reader.TokenType == JsonToken.None && !reader.Read())
		{
			throw JsonReaderException.Create(reader, "Error reading JObject from JsonReader.");
		}
		reader.MoveToContent();
		if (reader.TokenType != JsonToken.StartObject)
		{
			throw JsonReaderException.Create(reader, "Error reading JObject from JsonReader. Current JsonReader item is not an object: {0}".FormatWith(CultureInfo.InvariantCulture, reader.TokenType));
		}
		JObject jObject = new JObject();
		jObject.SetLineInfo(reader as IJsonLineInfo, settings);
		jObject.ReadTokenFrom(reader, settings);
		return jObject;
	}

	/// <summary>
	/// Load a <see cref="T:Newtonsoft.Json.Linq.JObject" /> from a string that contains JSON.
	/// </summary>
	/// <param name="json">A <see cref="T:System.String" /> that contains JSON.</param>
	/// <returns>A <see cref="T:Newtonsoft.Json.Linq.JObject" /> populated from the string that contains JSON.</returns>
	/// <exception cref="T:Newtonsoft.Json.JsonReaderException">
	///     <paramref name="json" /> is not valid JSON.
	/// </exception>
	/// <example>
	///   <code lang="cs" source="..\Src\Newtonsoft.Json.Tests\Documentation\LinqToJsonTests.cs" region="LinqToJsonCreateParse" title="Parsing a JSON Object from Text" />
	/// </example>
	public new static JObject Parse(string json)
	{
		return JObject.Parse(json, null);
	}

	/// <summary>
	/// Load a <see cref="T:Newtonsoft.Json.Linq.JObject" /> from a string that contains JSON.
	/// </summary>
	/// <param name="json">A <see cref="T:System.String" /> that contains JSON.</param>
	/// <param name="settings">The <see cref="T:Newtonsoft.Json.Linq.JsonLoadSettings" /> used to load the JSON.
	/// If this is <c>null</c>, default load settings will be used.</param>
	/// <returns>A <see cref="T:Newtonsoft.Json.Linq.JObject" /> populated from the string that contains JSON.</returns>
	/// <exception cref="T:Newtonsoft.Json.JsonReaderException">
	///     <paramref name="json" /> is not valid JSON.
	/// </exception>
	/// <example>
	///   <code lang="cs" source="..\Src\Newtonsoft.Json.Tests\Documentation\LinqToJsonTests.cs" region="LinqToJsonCreateParse" title="Parsing a JSON Object from Text" />
	/// </example>
	public new static JObject Parse(string json, JsonLoadSettings? settings)
	{
		using JsonReader jsonReader = new JsonTextReader(new StringReader(json));
		JObject result = JObject.Load(jsonReader, settings);
		while (jsonReader.Read())
		{
		}
		return result;
	}

	/// <summary>
	/// Creates a <see cref="T:Newtonsoft.Json.Linq.JObject" /> from an object.
	/// </summary>
	/// <param name="o">The object that will be used to create <see cref="T:Newtonsoft.Json.Linq.JObject" />.</param>
	/// <returns>A <see cref="T:Newtonsoft.Json.Linq.JObject" /> with the values of the specified object.</returns>
	public new static JObject FromObject(object o)
	{
		return JObject.FromObject(o, JsonSerializer.CreateDefault());
	}

	/// <summary>
	/// Creates a <see cref="T:Newtonsoft.Json.Linq.JObject" /> from an object.
	/// </summary>
	/// <param name="o">The object that will be used to create <see cref="T:Newtonsoft.Json.Linq.JObject" />.</param>
	/// <param name="jsonSerializer">The <see cref="T:Newtonsoft.Json.JsonSerializer" /> that will be used to read the object.</param>
	/// <returns>A <see cref="T:Newtonsoft.Json.Linq.JObject" /> with the values of the specified object.</returns>
	public new static JObject FromObject(object o, JsonSerializer jsonSerializer)
	{
		JToken jToken = JToken.FromObjectInternal(o, jsonSerializer);
		if (jToken.Type != JTokenType.Object)
		{
			throw new ArgumentException("Object serialized to {0}. JObject instance expected.".FormatWith(CultureInfo.InvariantCulture, jToken.Type));
		}
		return (JObject)jToken;
	}

	/// <summary>
	/// Writes this token to a <see cref="T:Newtonsoft.Json.JsonWriter" />.
	/// </summary>
	/// <param name="writer">A <see cref="T:Newtonsoft.Json.JsonWriter" /> into which this method will write.</param>
	/// <param name="converters">A collection of <see cref="T:Newtonsoft.Json.JsonConverter" /> which will be used when writing the token.</param>
	public override void WriteTo(JsonWriter writer, params JsonConverter[] converters)
	{
		writer.WriteStartObject();
		for (int i = 0; i < this._properties.Count; i++)
		{
			this._properties[i].WriteTo(writer, converters);
		}
		writer.WriteEndObject();
	}

	/// <summary>
	/// Gets the <see cref="T:Newtonsoft.Json.Linq.JToken" /> with the specified property name.
	/// </summary>
	/// <param name="propertyName">Name of the property.</param>
	/// <returns>The <see cref="T:Newtonsoft.Json.Linq.JToken" /> with the specified property name.</returns>
	public JToken? GetValue(string? propertyName)
	{
		return this.GetValue(propertyName, StringComparison.Ordinal);
	}

	/// <summary>
	/// Gets the <see cref="T:Newtonsoft.Json.Linq.JToken" /> with the specified property name.
	/// The exact property name will be searched for first and if no matching property is found then
	/// the <see cref="T:System.StringComparison" /> will be used to match a property.
	/// </summary>
	/// <param name="propertyName">Name of the property.</param>
	/// <param name="comparison">One of the enumeration values that specifies how the strings will be compared.</param>
	/// <returns>The <see cref="T:Newtonsoft.Json.Linq.JToken" /> with the specified property name.</returns>
	public JToken? GetValue(string? propertyName, StringComparison comparison)
	{
		if (propertyName == null)
		{
			return null;
		}
		return this.Property(propertyName, comparison)?.Value;
	}

	/// <summary>
	/// Tries to get the <see cref="T:Newtonsoft.Json.Linq.JToken" /> with the specified property name.
	/// The exact property name will be searched for first and if no matching property is found then
	/// the <see cref="T:System.StringComparison" /> will be used to match a property.
	/// </summary>
	/// <param name="propertyName">Name of the property.</param>
	/// <param name="value">The value.</param>
	/// <param name="comparison">One of the enumeration values that specifies how the strings will be compared.</param>
	/// <returns><c>true</c> if a value was successfully retrieved; otherwise, <c>false</c>.</returns>
	public bool TryGetValue(string propertyName, StringComparison comparison, [NotNullWhen(true)] out JToken? value)
	{
		value = this.GetValue(propertyName, comparison);
		return value != null;
	}

	/// <summary>
	/// Adds the specified property name.
	/// </summary>
	/// <param name="propertyName">Name of the property.</param>
	/// <param name="value">The value.</param>
	public void Add(string propertyName, JToken? value)
	{
		this.Add(new JProperty(propertyName, value));
	}

	/// <summary>
	/// Determines whether the JSON object has the specified property name.
	/// </summary>
	/// <param name="propertyName">Name of the property.</param>
	/// <returns><c>true</c> if the JSON object has the specified property name; otherwise, <c>false</c>.</returns>
	public bool ContainsKey(string propertyName)
	{
		ValidationUtils.ArgumentNotNull(propertyName, "propertyName");
		return this._properties.Contains(propertyName);
	}

	/// <summary>
	/// Removes the property with the specified name.
	/// </summary>
	/// <param name="propertyName">Name of the property.</param>
	/// <returns><c>true</c> if item was successfully removed; otherwise, <c>false</c>.</returns>
	public bool Remove(string propertyName)
	{
		JProperty jProperty = this.Property(propertyName, StringComparison.Ordinal);
		if (jProperty == null)
		{
			return false;
		}
		jProperty.Remove();
		return true;
	}

	/// <summary>
	/// Tries to get the <see cref="T:Newtonsoft.Json.Linq.JToken" /> with the specified property name.
	/// </summary>
	/// <param name="propertyName">Name of the property.</param>
	/// <param name="value">The value.</param>
	/// <returns><c>true</c> if a value was successfully retrieved; otherwise, <c>false</c>.</returns>
	public bool TryGetValue(string propertyName, [NotNullWhen(true)] out JToken? value)
	{
		JProperty jProperty = this.Property(propertyName, StringComparison.Ordinal);
		if (jProperty == null)
		{
			value = null;
			return false;
		}
		value = jProperty.Value;
		return true;
	}

	void ICollection<KeyValuePair<string, JToken>>.Add(KeyValuePair<string, JToken?> item)
	{
		this.Add(new JProperty(item.Key, item.Value));
	}

	void ICollection<KeyValuePair<string, JToken>>.Clear()
	{
		base.RemoveAll();
	}

	bool ICollection<KeyValuePair<string, JToken>>.Contains(KeyValuePair<string, JToken?> item)
	{
		JProperty jProperty = this.Property(item.Key, StringComparison.Ordinal);
		if (jProperty == null)
		{
			return false;
		}
		return jProperty.Value == item.Value;
	}

	void ICollection<KeyValuePair<string, JToken>>.CopyTo(KeyValuePair<string, JToken?>[] array, int arrayIndex)
	{
		if (array == null)
		{
			throw new ArgumentNullException("array");
		}
		if (arrayIndex < 0)
		{
			throw new ArgumentOutOfRangeException("arrayIndex", "arrayIndex is less than 0.");
		}
		if (arrayIndex >= array.Length && arrayIndex != 0)
		{
			throw new ArgumentException("arrayIndex is equal to or greater than the length of array.");
		}
		if (base.Count > array.Length - arrayIndex)
		{
			throw new ArgumentException("The number of elements in the source JObject is greater than the available space from arrayIndex to the end of the destination array.");
		}
		int num = 0;
		foreach (JProperty property in this._properties)
		{
			array[arrayIndex + num] = new KeyValuePair<string, JToken>(property.Name, property.Value);
			num++;
		}
	}

	bool ICollection<KeyValuePair<string, JToken>>.Remove(KeyValuePair<string, JToken?> item)
	{
		if (!((ICollection<KeyValuePair<string, JToken>>)this).Contains(item))
		{
			return false;
		}
		((IDictionary<string, JToken>)this).Remove(item.Key);
		return true;
	}

	internal override int GetDeepHashCode()
	{
		return base.ContentsHashCode();
	}

	/// <summary>
	/// Returns an enumerator that can be used to iterate through the collection.
	/// </summary>
	/// <returns>
	/// A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
	/// </returns>
	public IEnumerator<KeyValuePair<string, JToken?>> GetEnumerator()
	{
		foreach (JProperty property in this._properties)
		{
			yield return new KeyValuePair<string, JToken>(property.Name, property.Value);
		}
	}

	/// <summary>
	/// Raises the <see cref="E:Newtonsoft.Json.Linq.JObject.PropertyChanged" /> event with the provided arguments.
	/// </summary>
	/// <param name="propertyName">Name of the property.</param>
	protected virtual void OnPropertyChanged(string propertyName)
	{
		this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	/// <summary>
	/// Raises the <see cref="E:Newtonsoft.Json.Linq.JObject.PropertyChanging" /> event with the provided arguments.
	/// </summary>
	/// <param name="propertyName">Name of the property.</param>
	protected virtual void OnPropertyChanging(string propertyName)
	{
		this.PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(propertyName));
	}

	PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
	{
		return ((ICustomTypeDescriptor)this).GetProperties((Attribute[]?)null);
	}

	PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[]? attributes)
	{
		PropertyDescriptor[] array = new PropertyDescriptor[base.Count];
		int num = 0;
		foreach (KeyValuePair<string, JToken> item in this)
		{
			array[num] = new JPropertyDescriptor(item.Key);
			num++;
		}
		return new PropertyDescriptorCollection(array);
	}

	AttributeCollection ICustomTypeDescriptor.GetAttributes()
	{
		return AttributeCollection.Empty;
	}

	string? ICustomTypeDescriptor.GetClassName()
	{
		return null;
	}

	string? ICustomTypeDescriptor.GetComponentName()
	{
		return null;
	}

	TypeConverter ICustomTypeDescriptor.GetConverter()
	{
		return new TypeConverter();
	}

	EventDescriptor? ICustomTypeDescriptor.GetDefaultEvent()
	{
		return null;
	}

	PropertyDescriptor? ICustomTypeDescriptor.GetDefaultProperty()
	{
		return null;
	}

	object? ICustomTypeDescriptor.GetEditor(Type editorBaseType)
	{
		return null;
	}

	EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[]? attributes)
	{
		return EventDescriptorCollection.Empty;
	}

	EventDescriptorCollection ICustomTypeDescriptor.GetEvents()
	{
		return EventDescriptorCollection.Empty;
	}

	object? ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor? pd)
	{
		if (pd is JPropertyDescriptor)
		{
			return this;
		}
		return null;
	}

	/// <summary>
	/// Returns the <see cref="T:System.Dynamic.DynamicMetaObject" /> responsible for binding operations performed on this object.
	/// </summary>
	/// <param name="parameter">The expression tree representation of the runtime value.</param>
	/// <returns>
	/// The <see cref="T:System.Dynamic.DynamicMetaObject" /> to bind this object.
	/// </returns>
	protected override DynamicMetaObject GetMetaObject(Expression parameter)
	{
		return new DynamicProxyMetaObject<JObject>(parameter, this, new JObjectDynamicProxy());
	}
}
