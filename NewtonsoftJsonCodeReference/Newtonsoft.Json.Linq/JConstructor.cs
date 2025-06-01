using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Linq;

/// <summary>
/// Represents a JSON constructor.
/// </summary>
public class JConstructor : JContainer
{
	private string? _name;

	private readonly List<JToken> _values = new List<JToken>();

	/// <summary>
	/// Gets the container's children tokens.
	/// </summary>
	/// <value>The container's children tokens.</value>
	protected override IList<JToken> ChildrenTokens => this._values;

	/// <summary>
	/// Gets or sets the name of this constructor.
	/// </summary>
	/// <value>The constructor name.</value>
	public string? Name
	{
		get
		{
			return this._name;
		}
		set
		{
			this._name = value;
		}
	}

	/// <summary>
	/// Gets the node type for this <see cref="T:Newtonsoft.Json.Linq.JToken" />.
	/// </summary>
	/// <value>The type.</value>
	public override JTokenType Type => JTokenType.Constructor;

	/// <summary>
	/// Gets the <see cref="T:Newtonsoft.Json.Linq.JToken" /> with the specified key.
	/// </summary>
	/// <value>The <see cref="T:Newtonsoft.Json.Linq.JToken" /> with the specified key.</value>
	public override JToken? this[object key]
	{
		get
		{
			ValidationUtils.ArgumentNotNull(key, "key");
			if (!(key is int index))
			{
				throw new ArgumentException("Accessed JConstructor values with invalid key value: {0}. Argument position index expected.".FormatWith(CultureInfo.InvariantCulture, MiscellaneousUtils.ToString(key)));
			}
			return this.GetItem(index);
		}
		set
		{
			ValidationUtils.ArgumentNotNull(key, "key");
			if (!(key is int index))
			{
				throw new ArgumentException("Set JConstructor values with invalid key value: {0}. Argument position index expected.".FormatWith(CultureInfo.InvariantCulture, MiscellaneousUtils.ToString(key)));
			}
			this.SetItem(index, value);
		}
	}

	/// <summary>
	/// Writes this token to a <see cref="T:Newtonsoft.Json.JsonWriter" /> asynchronously.
	/// </summary>
	/// <param name="writer">A <see cref="T:Newtonsoft.Json.JsonWriter" /> into which this method will write.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
	/// <param name="converters">A collection of <see cref="T:Newtonsoft.Json.JsonConverter" /> which will be used when writing the token.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous write operation.</returns>
	public override async Task WriteToAsync(JsonWriter writer, CancellationToken cancellationToken, params JsonConverter[] converters)
	{
		await writer.WriteStartConstructorAsync(this._name ?? string.Empty, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		for (int i = 0; i < this._values.Count; i++)
		{
			await this._values[i].WriteToAsync(writer, cancellationToken, converters).ConfigureAwait(continueOnCapturedContext: false);
		}
		await writer.WriteEndConstructorAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	/// <summary>
	/// Asynchronously loads a <see cref="T:Newtonsoft.Json.Linq.JConstructor" /> from a <see cref="T:Newtonsoft.Json.JsonReader" />.
	/// </summary>
	/// <param name="reader">A <see cref="T:Newtonsoft.Json.JsonReader" /> that will be read for the content of the <see cref="T:Newtonsoft.Json.Linq.JConstructor" />.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>
	/// A <see cref="T:System.Threading.Tasks.Task`1" /> that represents the asynchronous load. The <see cref="P:System.Threading.Tasks.Task`1.Result" />
	/// property returns a <see cref="T:Newtonsoft.Json.Linq.JConstructor" /> that contains the JSON that was read from the specified <see cref="T:Newtonsoft.Json.JsonReader" />.</returns>
	public new static Task<JConstructor> LoadAsync(JsonReader reader, CancellationToken cancellationToken = default(CancellationToken))
	{
		return JConstructor.LoadAsync(reader, null, cancellationToken);
	}

	/// <summary>
	/// Asynchronously loads a <see cref="T:Newtonsoft.Json.Linq.JConstructor" /> from a <see cref="T:Newtonsoft.Json.JsonReader" />.
	/// </summary>
	/// <param name="reader">A <see cref="T:Newtonsoft.Json.JsonReader" /> that will be read for the content of the <see cref="T:Newtonsoft.Json.Linq.JConstructor" />.</param>
	/// <param name="settings">The <see cref="T:Newtonsoft.Json.Linq.JsonLoadSettings" /> used to load the JSON.
	/// If this is <c>null</c>, default load settings will be used.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>
	/// A <see cref="T:System.Threading.Tasks.Task`1" /> that represents the asynchronous load. The <see cref="P:System.Threading.Tasks.Task`1.Result" />
	/// property returns a <see cref="T:Newtonsoft.Json.Linq.JConstructor" /> that contains the JSON that was read from the specified <see cref="T:Newtonsoft.Json.JsonReader" />.</returns>
	public new static async Task<JConstructor> LoadAsync(JsonReader reader, JsonLoadSettings? settings, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (reader.TokenType == JsonToken.None && !(await reader.ReadAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false)))
		{
			throw JsonReaderException.Create(reader, "Error reading JConstructor from JsonReader.");
		}
		await reader.MoveToContentAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		if (reader.TokenType != JsonToken.StartConstructor)
		{
			throw JsonReaderException.Create(reader, "Error reading JConstructor from JsonReader. Current JsonReader item is not a constructor: {0}".FormatWith(CultureInfo.InvariantCulture, reader.TokenType));
		}
		JConstructor c = new JConstructor((string)reader.Value);
		c.SetLineInfo(reader as IJsonLineInfo, settings);
		await c.ReadTokenFromAsync(reader, settings, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		return c;
	}

	internal override int IndexOfItem(JToken? item)
	{
		if (item == null)
		{
			return -1;
		}
		return this._values.IndexOfReference(item);
	}

	internal override void MergeItem(object content, JsonMergeSettings? settings)
	{
		if (content is JConstructor jConstructor)
		{
			if (jConstructor.Name != null)
			{
				this.Name = jConstructor.Name;
			}
			JContainer.MergeEnumerableContent(this, jConstructor, settings);
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:Newtonsoft.Json.Linq.JConstructor" /> class.
	/// </summary>
	public JConstructor()
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:Newtonsoft.Json.Linq.JConstructor" /> class from another <see cref="T:Newtonsoft.Json.Linq.JConstructor" /> object.
	/// </summary>
	/// <param name="other">A <see cref="T:Newtonsoft.Json.Linq.JConstructor" /> object to copy from.</param>
	public JConstructor(JConstructor other)
		: base(other, null)
	{
		this._name = other.Name;
	}

	internal JConstructor(JConstructor other, JsonCloneSettings? settings)
		: base(other, settings)
	{
		this._name = other.Name;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:Newtonsoft.Json.Linq.JConstructor" /> class with the specified name and content.
	/// </summary>
	/// <param name="name">The constructor name.</param>
	/// <param name="content">The contents of the constructor.</param>
	public JConstructor(string name, params object[] content)
		: this(name, (object)content)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:Newtonsoft.Json.Linq.JConstructor" /> class with the specified name and content.
	/// </summary>
	/// <param name="name">The constructor name.</param>
	/// <param name="content">The contents of the constructor.</param>
	public JConstructor(string name, object content)
		: this(name)
	{
		this.Add(content);
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:Newtonsoft.Json.Linq.JConstructor" /> class with the specified name.
	/// </summary>
	/// <param name="name">The constructor name.</param>
	public JConstructor(string name)
	{
		if (name == null)
		{
			throw new ArgumentNullException("name");
		}
		if (name.Length == 0)
		{
			throw new ArgumentException("Constructor name cannot be empty.", "name");
		}
		this._name = name;
	}

	internal override bool DeepEquals(JToken node)
	{
		if (node is JConstructor jConstructor && this._name == jConstructor.Name)
		{
			return base.ContentsEqual(jConstructor);
		}
		return false;
	}

	internal override JToken CloneToken(JsonCloneSettings? settings = null)
	{
		return new JConstructor(this, settings);
	}

	/// <summary>
	/// Writes this token to a <see cref="T:Newtonsoft.Json.JsonWriter" />.
	/// </summary>
	/// <param name="writer">A <see cref="T:Newtonsoft.Json.JsonWriter" /> into which this method will write.</param>
	/// <param name="converters">A collection of <see cref="T:Newtonsoft.Json.JsonConverter" /> which will be used when writing the token.</param>
	public override void WriteTo(JsonWriter writer, params JsonConverter[] converters)
	{
		writer.WriteStartConstructor(this._name);
		int count = this._values.Count;
		for (int i = 0; i < count; i++)
		{
			this._values[i].WriteTo(writer, converters);
		}
		writer.WriteEndConstructor();
	}

	internal override int GetDeepHashCode()
	{
		return (this._name?.GetHashCode(StringComparison.Ordinal) ?? 0) ^ base.ContentsHashCode();
	}

	/// <summary>
	/// Loads a <see cref="T:Newtonsoft.Json.Linq.JConstructor" /> from a <see cref="T:Newtonsoft.Json.JsonReader" />.
	/// </summary>
	/// <param name="reader">A <see cref="T:Newtonsoft.Json.JsonReader" /> that will be read for the content of the <see cref="T:Newtonsoft.Json.Linq.JConstructor" />.</param>
	/// <returns>A <see cref="T:Newtonsoft.Json.Linq.JConstructor" /> that contains the JSON that was read from the specified <see cref="T:Newtonsoft.Json.JsonReader" />.</returns>
	public new static JConstructor Load(JsonReader reader)
	{
		return JConstructor.Load(reader, null);
	}

	/// <summary>
	/// Loads a <see cref="T:Newtonsoft.Json.Linq.JConstructor" /> from a <see cref="T:Newtonsoft.Json.JsonReader" />.
	/// </summary>
	/// <param name="reader">A <see cref="T:Newtonsoft.Json.JsonReader" /> that will be read for the content of the <see cref="T:Newtonsoft.Json.Linq.JConstructor" />.</param>
	/// <param name="settings">The <see cref="T:Newtonsoft.Json.Linq.JsonLoadSettings" /> used to load the JSON.
	/// If this is <c>null</c>, default load settings will be used.</param>
	/// <returns>A <see cref="T:Newtonsoft.Json.Linq.JConstructor" /> that contains the JSON that was read from the specified <see cref="T:Newtonsoft.Json.JsonReader" />.</returns>
	public new static JConstructor Load(JsonReader reader, JsonLoadSettings? settings)
	{
		if (reader.TokenType == JsonToken.None && !reader.Read())
		{
			throw JsonReaderException.Create(reader, "Error reading JConstructor from JsonReader.");
		}
		reader.MoveToContent();
		if (reader.TokenType != JsonToken.StartConstructor)
		{
			throw JsonReaderException.Create(reader, "Error reading JConstructor from JsonReader. Current JsonReader item is not a constructor: {0}".FormatWith(CultureInfo.InvariantCulture, reader.TokenType));
		}
		JConstructor jConstructor = new JConstructor((string)reader.Value);
		jConstructor.SetLineInfo(reader as IJsonLineInfo, settings);
		jConstructor.ReadTokenFrom(reader, settings);
		return jConstructor;
	}
}
