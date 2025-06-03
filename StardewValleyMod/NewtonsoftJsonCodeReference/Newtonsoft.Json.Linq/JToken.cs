using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq.JsonPath;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Linq;

/// <summary>
/// Represents an abstract JSON token.
/// </summary>
public abstract class JToken : IJEnumerable<JToken>, IEnumerable<JToken>, IEnumerable, IJsonLineInfo, ICloneable, IDynamicMetaObjectProvider
{
	private class LineInfoAnnotation
	{
		internal readonly int LineNumber;

		internal readonly int LinePosition;

		public LineInfoAnnotation(int lineNumber, int linePosition)
		{
			this.LineNumber = lineNumber;
			this.LinePosition = linePosition;
		}
	}

	private static JTokenEqualityComparer? _equalityComparer;

	private JContainer? _parent;

	private JToken? _previous;

	private JToken? _next;

	private object? _annotations;

	private static readonly JTokenType[] BooleanTypes = new JTokenType[6]
	{
		JTokenType.Integer,
		JTokenType.Float,
		JTokenType.String,
		JTokenType.Comment,
		JTokenType.Raw,
		JTokenType.Boolean
	};

	private static readonly JTokenType[] NumberTypes = new JTokenType[6]
	{
		JTokenType.Integer,
		JTokenType.Float,
		JTokenType.String,
		JTokenType.Comment,
		JTokenType.Raw,
		JTokenType.Boolean
	};

	private static readonly JTokenType[] BigIntegerTypes = new JTokenType[7]
	{
		JTokenType.Integer,
		JTokenType.Float,
		JTokenType.String,
		JTokenType.Comment,
		JTokenType.Raw,
		JTokenType.Boolean,
		JTokenType.Bytes
	};

	private static readonly JTokenType[] StringTypes = new JTokenType[11]
	{
		JTokenType.Date,
		JTokenType.Integer,
		JTokenType.Float,
		JTokenType.String,
		JTokenType.Comment,
		JTokenType.Raw,
		JTokenType.Boolean,
		JTokenType.Bytes,
		JTokenType.Guid,
		JTokenType.TimeSpan,
		JTokenType.Uri
	};

	private static readonly JTokenType[] GuidTypes = new JTokenType[5]
	{
		JTokenType.String,
		JTokenType.Comment,
		JTokenType.Raw,
		JTokenType.Guid,
		JTokenType.Bytes
	};

	private static readonly JTokenType[] TimeSpanTypes = new JTokenType[4]
	{
		JTokenType.String,
		JTokenType.Comment,
		JTokenType.Raw,
		JTokenType.TimeSpan
	};

	private static readonly JTokenType[] UriTypes = new JTokenType[4]
	{
		JTokenType.String,
		JTokenType.Comment,
		JTokenType.Raw,
		JTokenType.Uri
	};

	private static readonly JTokenType[] CharTypes = new JTokenType[5]
	{
		JTokenType.Integer,
		JTokenType.Float,
		JTokenType.String,
		JTokenType.Comment,
		JTokenType.Raw
	};

	private static readonly JTokenType[] DateTimeTypes = new JTokenType[4]
	{
		JTokenType.Date,
		JTokenType.String,
		JTokenType.Comment,
		JTokenType.Raw
	};

	private static readonly JTokenType[] BytesTypes = new JTokenType[5]
	{
		JTokenType.Bytes,
		JTokenType.String,
		JTokenType.Comment,
		JTokenType.Raw,
		JTokenType.Integer
	};

	/// <summary>
	/// Gets a comparer that can compare two tokens for value equality.
	/// </summary>
	/// <value>A <see cref="T:Newtonsoft.Json.Linq.JTokenEqualityComparer" /> that can compare two nodes for value equality.</value>
	public static JTokenEqualityComparer EqualityComparer
	{
		get
		{
			if (JToken._equalityComparer == null)
			{
				JToken._equalityComparer = new JTokenEqualityComparer();
			}
			return JToken._equalityComparer;
		}
	}

	/// <summary>
	/// Gets or sets the parent.
	/// </summary>
	/// <value>The parent.</value>
	public JContainer? Parent
	{
		[DebuggerStepThrough]
		get
		{
			return this._parent;
		}
		internal set
		{
			this._parent = value;
		}
	}

	/// <summary>
	/// Gets the root <see cref="T:Newtonsoft.Json.Linq.JToken" /> of this <see cref="T:Newtonsoft.Json.Linq.JToken" />.
	/// </summary>
	/// <value>The root <see cref="T:Newtonsoft.Json.Linq.JToken" /> of this <see cref="T:Newtonsoft.Json.Linq.JToken" />.</value>
	public JToken Root
	{
		get
		{
			JContainer parent = this.Parent;
			if (parent == null)
			{
				return this;
			}
			while (parent.Parent != null)
			{
				parent = parent.Parent;
			}
			return parent;
		}
	}

	/// <summary>
	/// Gets the node type for this <see cref="T:Newtonsoft.Json.Linq.JToken" />.
	/// </summary>
	/// <value>The type.</value>
	public abstract JTokenType Type { get; }

	/// <summary>
	/// Gets a value indicating whether this token has child tokens.
	/// </summary>
	/// <value>
	/// 	<c>true</c> if this token has child values; otherwise, <c>false</c>.
	/// </value>
	public abstract bool HasValues { get; }

	/// <summary>
	/// Gets the next sibling token of this node.
	/// </summary>
	/// <value>The <see cref="T:Newtonsoft.Json.Linq.JToken" /> that contains the next sibling token.</value>
	public JToken? Next
	{
		get
		{
			return this._next;
		}
		internal set
		{
			this._next = value;
		}
	}

	/// <summary>
	/// Gets the previous sibling token of this node.
	/// </summary>
	/// <value>The <see cref="T:Newtonsoft.Json.Linq.JToken" /> that contains the previous sibling token.</value>
	public JToken? Previous
	{
		get
		{
			return this._previous;
		}
		internal set
		{
			this._previous = value;
		}
	}

	/// <summary>
	/// Gets the path of the JSON token. 
	/// </summary>
	public string Path
	{
		get
		{
			if (this.Parent == null)
			{
				return string.Empty;
			}
			List<JsonPosition> list = new List<JsonPosition>();
			JToken jToken = null;
			for (JToken jToken2 = this; jToken2 != null; jToken2 = jToken2.Parent)
			{
				switch (jToken2.Type)
				{
				case JTokenType.Property:
				{
					JProperty jProperty = (JProperty)jToken2;
					list.Add(new JsonPosition(JsonContainerType.Object)
					{
						PropertyName = jProperty.Name
					});
					break;
				}
				case JTokenType.Array:
				case JTokenType.Constructor:
					if (jToken != null)
					{
						int position = ((IList<JToken>)jToken2).IndexOf(jToken);
						list.Add(new JsonPosition(JsonContainerType.Array)
						{
							Position = position
						});
					}
					break;
				}
				jToken = jToken2;
			}
			list.FastReverse();
			return JsonPosition.BuildPath(list, null);
		}
	}

	/// <summary>
	/// Gets the <see cref="T:Newtonsoft.Json.Linq.JToken" /> with the specified key.
	/// </summary>
	/// <value>The <see cref="T:Newtonsoft.Json.Linq.JToken" /> with the specified key.</value>
	public virtual JToken? this[object key]
	{
		get
		{
			throw new InvalidOperationException("Cannot access child value on {0}.".FormatWith(CultureInfo.InvariantCulture, base.GetType()));
		}
		set
		{
			throw new InvalidOperationException("Cannot set child value on {0}.".FormatWith(CultureInfo.InvariantCulture, base.GetType()));
		}
	}

	/// <summary>
	/// Get the first child token of this token.
	/// </summary>
	/// <value>A <see cref="T:Newtonsoft.Json.Linq.JToken" /> containing the first child token of the <see cref="T:Newtonsoft.Json.Linq.JToken" />.</value>
	public virtual JToken? First
	{
		get
		{
			throw new InvalidOperationException("Cannot access child value on {0}.".FormatWith(CultureInfo.InvariantCulture, base.GetType()));
		}
	}

	/// <summary>
	/// Get the last child token of this token.
	/// </summary>
	/// <value>A <see cref="T:Newtonsoft.Json.Linq.JToken" /> containing the last child token of the <see cref="T:Newtonsoft.Json.Linq.JToken" />.</value>
	public virtual JToken? Last
	{
		get
		{
			throw new InvalidOperationException("Cannot access child value on {0}.".FormatWith(CultureInfo.InvariantCulture, base.GetType()));
		}
	}

	IJEnumerable<JToken> IJEnumerable<JToken>.this[object key] => this[key];

	int IJsonLineInfo.LineNumber => this.Annotation<LineInfoAnnotation>()?.LineNumber ?? 0;

	int IJsonLineInfo.LinePosition => this.Annotation<LineInfoAnnotation>()?.LinePosition ?? 0;

	/// <summary>
	/// Writes this token to a <see cref="T:Newtonsoft.Json.JsonWriter" /> asynchronously.
	/// </summary>
	/// <param name="writer">A <see cref="T:Newtonsoft.Json.JsonWriter" /> into which this method will write.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
	/// <param name="converters">A collection of <see cref="T:Newtonsoft.Json.JsonConverter" /> which will be used when writing the token.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous write operation.</returns>
	public virtual Task WriteToAsync(JsonWriter writer, CancellationToken cancellationToken, params JsonConverter[] converters)
	{
		throw new NotImplementedException();
	}

	/// <summary>
	/// Writes this token to a <see cref="T:Newtonsoft.Json.JsonWriter" /> asynchronously.
	/// </summary>
	/// <param name="writer">A <see cref="T:Newtonsoft.Json.JsonWriter" /> into which this method will write.</param>
	/// <param name="converters">A collection of <see cref="T:Newtonsoft.Json.JsonConverter" /> which will be used when writing the token.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous write operation.</returns>
	public Task WriteToAsync(JsonWriter writer, params JsonConverter[] converters)
	{
		return this.WriteToAsync(writer, default(CancellationToken), converters);
	}

	/// <summary>
	/// Asynchronously creates a <see cref="T:Newtonsoft.Json.Linq.JToken" /> from a <see cref="T:Newtonsoft.Json.JsonReader" />.
	/// </summary>
	/// <param name="reader">An <see cref="T:Newtonsoft.Json.JsonReader" /> positioned at the token to read into this <see cref="T:Newtonsoft.Json.Linq.JToken" />.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>
	/// A <see cref="T:System.Threading.Tasks.Task`1" /> that represents the asynchronous creation. The
	/// <see cref="P:System.Threading.Tasks.Task`1.Result" /> property returns a <see cref="T:Newtonsoft.Json.Linq.JToken" /> that contains 
	/// the token and its descendant tokens
	/// that were read from the reader. The runtime type of the token is determined
	/// by the token type of the first token encountered in the reader.
	/// </returns>
	public static Task<JToken> ReadFromAsync(JsonReader reader, CancellationToken cancellationToken = default(CancellationToken))
	{
		return JToken.ReadFromAsync(reader, null, cancellationToken);
	}

	/// <summary>
	/// Asynchronously creates a <see cref="T:Newtonsoft.Json.Linq.JToken" /> from a <see cref="T:Newtonsoft.Json.JsonReader" />.
	/// </summary>
	/// <param name="reader">An <see cref="T:Newtonsoft.Json.JsonReader" /> positioned at the token to read into this <see cref="T:Newtonsoft.Json.Linq.JToken" />.</param>
	/// <param name="settings">The <see cref="T:Newtonsoft.Json.Linq.JsonLoadSettings" /> used to load the JSON.
	/// If this is <c>null</c>, default load settings will be used.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>
	/// A <see cref="T:System.Threading.Tasks.Task`1" /> that represents the asynchronous creation. The
	/// <see cref="P:System.Threading.Tasks.Task`1.Result" /> property returns a <see cref="T:Newtonsoft.Json.Linq.JToken" /> that contains 
	/// the token and its descendant tokens
	/// that were read from the reader. The runtime type of the token is determined
	/// by the token type of the first token encountered in the reader.
	/// </returns>
	public static async Task<JToken> ReadFromAsync(JsonReader reader, JsonLoadSettings? settings, CancellationToken cancellationToken = default(CancellationToken))
	{
		ValidationUtils.ArgumentNotNull(reader, "reader");
		if (reader.TokenType == JsonToken.None && !(await ((settings != null && settings.CommentHandling == CommentHandling.Ignore) ? reader.ReadAndMoveToContentAsync(cancellationToken) : reader.ReadAsync(cancellationToken)).ConfigureAwait(continueOnCapturedContext: false)))
		{
			throw JsonReaderException.Create(reader, "Error reading JToken from JsonReader.");
		}
		IJsonLineInfo lineInfo = reader as IJsonLineInfo;
		switch (reader.TokenType)
		{
		case JsonToken.StartObject:
			return await JObject.LoadAsync(reader, settings, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		case JsonToken.StartArray:
			return await JArray.LoadAsync(reader, settings, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		case JsonToken.StartConstructor:
			return await JConstructor.LoadAsync(reader, settings, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		case JsonToken.PropertyName:
			return await JProperty.LoadAsync(reader, settings, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		case JsonToken.Integer:
		case JsonToken.Float:
		case JsonToken.String:
		case JsonToken.Boolean:
		case JsonToken.Date:
		case JsonToken.Bytes:
		{
			JValue jValue4 = new JValue(reader.Value);
			jValue4.SetLineInfo(lineInfo, settings);
			return jValue4;
		}
		case JsonToken.Comment:
		{
			JValue jValue3 = JValue.CreateComment(reader.Value?.ToString());
			jValue3.SetLineInfo(lineInfo, settings);
			return jValue3;
		}
		case JsonToken.Null:
		{
			JValue jValue2 = JValue.CreateNull();
			jValue2.SetLineInfo(lineInfo, settings);
			return jValue2;
		}
		case JsonToken.Undefined:
		{
			JValue jValue = JValue.CreateUndefined();
			jValue.SetLineInfo(lineInfo, settings);
			return jValue;
		}
		default:
			throw JsonReaderException.Create(reader, "Error reading JToken from JsonReader. Unexpected token: {0}".FormatWith(CultureInfo.InvariantCulture, reader.TokenType));
		}
	}

	/// <summary>
	/// Asynchronously creates a <see cref="T:Newtonsoft.Json.Linq.JToken" /> from a <see cref="T:Newtonsoft.Json.JsonReader" />.
	/// </summary>
	/// <param name="reader">A <see cref="T:Newtonsoft.Json.JsonReader" /> positioned at the token to read into this <see cref="T:Newtonsoft.Json.Linq.JToken" />.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>
	/// A <see cref="T:System.Threading.Tasks.Task`1" /> that represents the asynchronous creation. The <see cref="P:System.Threading.Tasks.Task`1.Result" />
	/// property returns a <see cref="T:Newtonsoft.Json.Linq.JToken" /> that contains the token and its descendant tokens
	/// that were read from the reader. The runtime type of the token is determined
	/// by the token type of the first token encountered in the reader.
	/// </returns>
	public static Task<JToken> LoadAsync(JsonReader reader, CancellationToken cancellationToken = default(CancellationToken))
	{
		return JToken.LoadAsync(reader, null, cancellationToken);
	}

	/// <summary>
	/// Asynchronously creates a <see cref="T:Newtonsoft.Json.Linq.JToken" /> from a <see cref="T:Newtonsoft.Json.JsonReader" />.
	/// </summary>
	/// <param name="reader">A <see cref="T:Newtonsoft.Json.JsonReader" /> positioned at the token to read into this <see cref="T:Newtonsoft.Json.Linq.JToken" />.</param>
	/// <param name="settings">The <see cref="T:Newtonsoft.Json.Linq.JsonLoadSettings" /> used to load the JSON.
	/// If this is <c>null</c>, default load settings will be used.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>
	/// A <see cref="T:System.Threading.Tasks.Task`1" /> that represents the asynchronous creation. The <see cref="P:System.Threading.Tasks.Task`1.Result" />
	/// property returns a <see cref="T:Newtonsoft.Json.Linq.JToken" /> that contains the token and its descendant tokens
	/// that were read from the reader. The runtime type of the token is determined
	/// by the token type of the first token encountered in the reader.
	/// </returns>
	public static Task<JToken> LoadAsync(JsonReader reader, JsonLoadSettings? settings, CancellationToken cancellationToken = default(CancellationToken))
	{
		return JToken.ReadFromAsync(reader, settings, cancellationToken);
	}

	internal abstract JToken CloneToken(JsonCloneSettings? settings);

	internal abstract bool DeepEquals(JToken node);

	/// <summary>
	/// Compares the values of two tokens, including the values of all descendant tokens.
	/// </summary>
	/// <param name="t1">The first <see cref="T:Newtonsoft.Json.Linq.JToken" /> to compare.</param>
	/// <param name="t2">The second <see cref="T:Newtonsoft.Json.Linq.JToken" /> to compare.</param>
	/// <returns><c>true</c> if the tokens are equal; otherwise <c>false</c>.</returns>
	public static bool DeepEquals(JToken? t1, JToken? t2)
	{
		if (t1 != t2)
		{
			if (t1 != null && t2 != null)
			{
				return t1.DeepEquals(t2);
			}
			return false;
		}
		return true;
	}

	internal JToken()
	{
	}

	/// <summary>
	/// Adds the specified content immediately after this token.
	/// </summary>
	/// <param name="content">A content object that contains simple content or a collection of content objects to be added after this token.</param>
	public void AddAfterSelf(object? content)
	{
		if (this._parent == null)
		{
			throw new InvalidOperationException("The parent is missing.");
		}
		int num = this._parent.IndexOfItem(this);
		this._parent.TryAddInternal(num + 1, content, skipParentCheck: false, copyAnnotations: true);
	}

	/// <summary>
	/// Adds the specified content immediately before this token.
	/// </summary>
	/// <param name="content">A content object that contains simple content or a collection of content objects to be added before this token.</param>
	public void AddBeforeSelf(object? content)
	{
		if (this._parent == null)
		{
			throw new InvalidOperationException("The parent is missing.");
		}
		int index = this._parent.IndexOfItem(this);
		this._parent.TryAddInternal(index, content, skipParentCheck: false, copyAnnotations: true);
	}

	/// <summary>
	/// Returns a collection of the ancestor tokens of this token.
	/// </summary>
	/// <returns>A collection of the ancestor tokens of this token.</returns>
	public IEnumerable<JToken> Ancestors()
	{
		return this.GetAncestors(self: false);
	}

	/// <summary>
	/// Returns a collection of tokens that contain this token, and the ancestors of this token.
	/// </summary>
	/// <returns>A collection of tokens that contain this token, and the ancestors of this token.</returns>
	public IEnumerable<JToken> AncestorsAndSelf()
	{
		return this.GetAncestors(self: true);
	}

	internal IEnumerable<JToken> GetAncestors(bool self)
	{
		for (JToken current = (self ? this : this.Parent); current != null; current = current.Parent)
		{
			yield return current;
		}
	}

	/// <summary>
	/// Returns a collection of the sibling tokens after this token, in document order.
	/// </summary>
	/// <returns>A collection of the sibling tokens after this tokens, in document order.</returns>
	public IEnumerable<JToken> AfterSelf()
	{
		if (this.Parent != null)
		{
			for (JToken o = this.Next; o != null; o = o.Next)
			{
				yield return o;
			}
		}
	}

	/// <summary>
	/// Returns a collection of the sibling tokens before this token, in document order.
	/// </summary>
	/// <returns>A collection of the sibling tokens before this token, in document order.</returns>
	public IEnumerable<JToken> BeforeSelf()
	{
		if (this.Parent != null)
		{
			JToken o = this.Parent.First;
			while (o != this && o != null)
			{
				yield return o;
				o = o.Next;
			}
		}
	}

	/// <summary>
	/// Gets the <see cref="T:Newtonsoft.Json.Linq.JToken" /> with the specified key converted to the specified type.
	/// </summary>
	/// <typeparam name="T">The type to convert the token to.</typeparam>
	/// <param name="key">The token key.</param>
	/// <returns>The converted token value.</returns>
	public virtual T? Value<T>(object key)
	{
		JToken jToken = this[key];
		if (jToken != null)
		{
			return jToken.Convert<JToken, T>();
		}
		return default(T);
	}

	/// <summary>
	/// Returns a collection of the child tokens of this token, in document order.
	/// </summary>
	/// <returns>An <see cref="T:System.Collections.Generic.IEnumerable`1" /> of <see cref="T:Newtonsoft.Json.Linq.JToken" /> containing the child tokens of this <see cref="T:Newtonsoft.Json.Linq.JToken" />, in document order.</returns>
	public virtual JEnumerable<JToken> Children()
	{
		return JEnumerable<JToken>.Empty;
	}

	/// <summary>
	/// Returns a collection of the child tokens of this token, in document order, filtered by the specified type.
	/// </summary>
	/// <typeparam name="T">The type to filter the child tokens on.</typeparam>
	/// <returns>A <see cref="T:Newtonsoft.Json.Linq.JEnumerable`1" /> containing the child tokens of this <see cref="T:Newtonsoft.Json.Linq.JToken" />, in document order.</returns>
	public JEnumerable<T> Children<T>() where T : JToken
	{
		return new JEnumerable<T>(this.Children().OfType<T>());
	}

	/// <summary>
	/// Returns a collection of the child values of this token, in document order.
	/// </summary>
	/// <typeparam name="T">The type to convert the values to.</typeparam>
	/// <returns>A <see cref="T:System.Collections.Generic.IEnumerable`1" /> containing the child values of this <see cref="T:Newtonsoft.Json.Linq.JToken" />, in document order.</returns>
	public virtual IEnumerable<T?> Values<T>()
	{
		throw new InvalidOperationException("Cannot access child value on {0}.".FormatWith(CultureInfo.InvariantCulture, base.GetType()));
	}

	/// <summary>
	/// Removes this token from its parent.
	/// </summary>
	public void Remove()
	{
		if (this._parent == null)
		{
			throw new InvalidOperationException("The parent is missing.");
		}
		this._parent.RemoveItem(this);
	}

	/// <summary>
	/// Replaces this token with the specified token.
	/// </summary>
	/// <param name="value">The value.</param>
	public void Replace(JToken value)
	{
		if (this._parent == null)
		{
			throw new InvalidOperationException("The parent is missing.");
		}
		this._parent.ReplaceItem(this, value);
	}

	/// <summary>
	/// Writes this token to a <see cref="T:Newtonsoft.Json.JsonWriter" />.
	/// </summary>
	/// <param name="writer">A <see cref="T:Newtonsoft.Json.JsonWriter" /> into which this method will write.</param>
	/// <param name="converters">A collection of <see cref="T:Newtonsoft.Json.JsonConverter" /> which will be used when writing the token.</param>
	public abstract void WriteTo(JsonWriter writer, params JsonConverter[] converters);

	/// <summary>
	/// Returns the indented JSON for this token.
	/// </summary>
	/// <remarks>
	/// <c>ToString()</c> returns a non-JSON string value for tokens with a type of <see cref="F:Newtonsoft.Json.Linq.JTokenType.String" />.
	/// If you want the JSON for all token types then you should use <see cref="M:Newtonsoft.Json.Linq.JToken.WriteTo(Newtonsoft.Json.JsonWriter,Newtonsoft.Json.JsonConverter[])" />.
	/// </remarks>
	/// <returns>
	/// The indented JSON for this token.
	/// </returns>
	public override string ToString()
	{
		return this.ToString(Formatting.Indented);
	}

	/// <summary>
	/// Returns the JSON for this token using the given formatting and converters.
	/// </summary>
	/// <param name="formatting">Indicates how the output should be formatted.</param>
	/// <param name="converters">A collection of <see cref="T:Newtonsoft.Json.JsonConverter" />s which will be used when writing the token.</param>
	/// <returns>The JSON for this token using the given formatting and converters.</returns>
	public string ToString(Formatting formatting, params JsonConverter[] converters)
	{
		using StringWriter stringWriter = new StringWriter(CultureInfo.InvariantCulture);
		JsonTextWriter jsonTextWriter = new JsonTextWriter(stringWriter);
		jsonTextWriter.Formatting = formatting;
		this.WriteTo(jsonTextWriter, converters);
		return stringWriter.ToString();
	}

	private static JValue? EnsureValue(JToken value)
	{
		if (value == null)
		{
			throw new ArgumentNullException("value");
		}
		if (value is JProperty jProperty)
		{
			value = jProperty.Value;
		}
		return value as JValue;
	}

	private static string GetType(JToken token)
	{
		ValidationUtils.ArgumentNotNull(token, "token");
		if (token is JProperty jProperty)
		{
			token = jProperty.Value;
		}
		return token.Type.ToString();
	}

	private static bool ValidateToken(JToken o, JTokenType[] validTypes, bool nullable)
	{
		if (Array.IndexOf(validTypes, o.Type) == -1)
		{
			if (nullable)
			{
				if (o.Type != JTokenType.Null)
				{
					return o.Type == JTokenType.Undefined;
				}
				return true;
			}
			return false;
		}
		return true;
	}

	/// <summary>
	/// Performs an explicit conversion from <see cref="T:Newtonsoft.Json.Linq.JToken" /> to <see cref="T:System.Boolean" />.
	/// </summary>
	/// <param name="value">The value.</param>
	/// <returns>The result of the conversion.</returns>
	public static explicit operator bool(JToken value)
	{
		JValue jValue = JToken.EnsureValue(value);
		if (jValue == null || !JToken.ValidateToken(jValue, JToken.BooleanTypes, nullable: false))
		{
			throw new ArgumentException("Can not convert {0} to Boolean.".FormatWith(CultureInfo.InvariantCulture, JToken.GetType(value)));
		}
		if (jValue.Value is BigInteger bigInteger)
		{
			return Convert.ToBoolean((int)bigInteger);
		}
		return Convert.ToBoolean(jValue.Value, CultureInfo.InvariantCulture);
	}

	/// <summary>
	/// Performs an explicit conversion from <see cref="T:Newtonsoft.Json.Linq.JToken" /> to <see cref="T:System.DateTimeOffset" />.
	/// </summary>
	/// <param name="value">The value.</param>
	/// <returns>The result of the conversion.</returns>
	public static explicit operator DateTimeOffset(JToken value)
	{
		JValue jValue = JToken.EnsureValue(value);
		if (jValue == null || !JToken.ValidateToken(jValue, JToken.DateTimeTypes, nullable: false))
		{
			throw new ArgumentException("Can not convert {0} to DateTimeOffset.".FormatWith(CultureInfo.InvariantCulture, JToken.GetType(value)));
		}
		object value2 = jValue.Value;
		if (value2 is DateTimeOffset)
		{
			return (DateTimeOffset)value2;
		}
		if (jValue.Value is string input)
		{
			return DateTimeOffset.Parse(input, CultureInfo.InvariantCulture);
		}
		return new DateTimeOffset(Convert.ToDateTime(jValue.Value, CultureInfo.InvariantCulture));
	}

	/// <summary>
	/// Performs an explicit conversion from <see cref="T:Newtonsoft.Json.Linq.JToken" /> to <see cref="T:System.Nullable`1" /> of <see cref="T:System.Boolean" />.
	/// </summary>
	/// <param name="value">The value.</param>
	/// <returns>The result of the conversion.</returns>
	public static explicit operator bool?(JToken? value)
	{
		if (value == null)
		{
			return null;
		}
		JValue jValue = JToken.EnsureValue(value);
		if (jValue == null || !JToken.ValidateToken(jValue, JToken.BooleanTypes, nullable: true))
		{
			throw new ArgumentException("Can not convert {0} to Boolean.".FormatWith(CultureInfo.InvariantCulture, JToken.GetType(value)));
		}
		if (jValue.Value is BigInteger bigInteger)
		{
			return Convert.ToBoolean((int)bigInteger);
		}
		if (jValue.Value == null)
		{
			return null;
		}
		return Convert.ToBoolean(jValue.Value, CultureInfo.InvariantCulture);
	}

	/// <summary>
	/// Performs an explicit conversion from <see cref="T:Newtonsoft.Json.Linq.JToken" /> to <see cref="T:System.Nullable`1" /> of <see cref="T:System.Int64" />.
	/// </summary>
	/// <param name="value">The value.</param>
	/// <returns>The result of the conversion.</returns>
	public static explicit operator long(JToken value)
	{
		JValue jValue = JToken.EnsureValue(value);
		if (jValue == null || !JToken.ValidateToken(jValue, JToken.NumberTypes, nullable: false))
		{
			throw new ArgumentException("Can not convert {0} to Int64.".FormatWith(CultureInfo.InvariantCulture, JToken.GetType(value)));
		}
		if (jValue.Value is BigInteger bigInteger)
		{
			return (long)bigInteger;
		}
		return Convert.ToInt64(jValue.Value, CultureInfo.InvariantCulture);
	}

	/// <summary>
	/// Performs an explicit conversion from <see cref="T:Newtonsoft.Json.Linq.JToken" /> to <see cref="T:System.Nullable`1" /> of <see cref="T:System.DateTime" />.
	/// </summary>
	/// <param name="value">The value.</param>
	/// <returns>The result of the conversion.</returns>
	public static explicit operator DateTime?(JToken? value)
	{
		if (value == null)
		{
			return null;
		}
		JValue jValue = JToken.EnsureValue(value);
		if (jValue == null || !JToken.ValidateToken(jValue, JToken.DateTimeTypes, nullable: true))
		{
			throw new ArgumentException("Can not convert {0} to DateTime.".FormatWith(CultureInfo.InvariantCulture, JToken.GetType(value)));
		}
		if (jValue.Value is DateTimeOffset dateTimeOffset)
		{
			return dateTimeOffset.DateTime;
		}
		if (jValue.Value == null)
		{
			return null;
		}
		return Convert.ToDateTime(jValue.Value, CultureInfo.InvariantCulture);
	}

	/// <summary>
	/// Performs an explicit conversion from <see cref="T:Newtonsoft.Json.Linq.JToken" /> to <see cref="T:System.Nullable`1" /> of <see cref="T:System.DateTimeOffset" />.
	/// </summary>
	/// <param name="value">The value.</param>
	/// <returns>The result of the conversion.</returns>
	public static explicit operator DateTimeOffset?(JToken? value)
	{
		if (value == null)
		{
			return null;
		}
		JValue jValue = JToken.EnsureValue(value);
		if (jValue == null || !JToken.ValidateToken(jValue, JToken.DateTimeTypes, nullable: true))
		{
			throw new ArgumentException("Can not convert {0} to DateTimeOffset.".FormatWith(CultureInfo.InvariantCulture, JToken.GetType(value)));
		}
		if (jValue.Value == null)
		{
			return null;
		}
		object value2 = jValue.Value;
		if (value2 is DateTimeOffset)
		{
			return (DateTimeOffset)value2;
		}
		if (jValue.Value is string input)
		{
			return DateTimeOffset.Parse(input, CultureInfo.InvariantCulture);
		}
		return new DateTimeOffset(Convert.ToDateTime(jValue.Value, CultureInfo.InvariantCulture));
	}

	/// <summary>
	/// Performs an explicit conversion from <see cref="T:Newtonsoft.Json.Linq.JToken" /> to <see cref="T:System.Nullable`1" /> of <see cref="T:System.Decimal" />.
	/// </summary>
	/// <param name="value">The value.</param>
	/// <returns>The result of the conversion.</returns>
	public static explicit operator decimal?(JToken? value)
	{
		if (value == null)
		{
			return null;
		}
		JValue jValue = JToken.EnsureValue(value);
		if (jValue == null || !JToken.ValidateToken(jValue, JToken.NumberTypes, nullable: true))
		{
			throw new ArgumentException("Can not convert {0} to Decimal.".FormatWith(CultureInfo.InvariantCulture, JToken.GetType(value)));
		}
		if (jValue.Value is BigInteger bigInteger)
		{
			return (decimal)bigInteger;
		}
		if (jValue.Value == null)
		{
			return null;
		}
		return Convert.ToDecimal(jValue.Value, CultureInfo.InvariantCulture);
	}

	/// <summary>
	/// Performs an explicit conversion from <see cref="T:Newtonsoft.Json.Linq.JToken" /> to <see cref="T:System.Nullable`1" /> of <see cref="T:System.Double" />.
	/// </summary>
	/// <param name="value">The value.</param>
	/// <returns>The result of the conversion.</returns>
	public static explicit operator double?(JToken? value)
	{
		if (value == null)
		{
			return null;
		}
		JValue jValue = JToken.EnsureValue(value);
		if (jValue == null || !JToken.ValidateToken(jValue, JToken.NumberTypes, nullable: true))
		{
			throw new ArgumentException("Can not convert {0} to Double.".FormatWith(CultureInfo.InvariantCulture, JToken.GetType(value)));
		}
		if (jValue.Value is BigInteger bigInteger)
		{
			return (double)bigInteger;
		}
		if (jValue.Value == null)
		{
			return null;
		}
		return Convert.ToDouble(jValue.Value, CultureInfo.InvariantCulture);
	}

	/// <summary>
	/// Performs an explicit conversion from <see cref="T:Newtonsoft.Json.Linq.JToken" /> to <see cref="T:System.Nullable`1" /> of <see cref="T:System.Char" />.
	/// </summary>
	/// <param name="value">The value.</param>
	/// <returns>The result of the conversion.</returns>
	public static explicit operator char?(JToken? value)
	{
		if (value == null)
		{
			return null;
		}
		JValue jValue = JToken.EnsureValue(value);
		if (jValue == null || !JToken.ValidateToken(jValue, JToken.CharTypes, nullable: true))
		{
			throw new ArgumentException("Can not convert {0} to Char.".FormatWith(CultureInfo.InvariantCulture, JToken.GetType(value)));
		}
		if (jValue.Value is BigInteger bigInteger)
		{
			return (char)(ushort)bigInteger;
		}
		if (jValue.Value == null)
		{
			return null;
		}
		return Convert.ToChar(jValue.Value, CultureInfo.InvariantCulture);
	}

	/// <summary>
	/// Performs an explicit conversion from <see cref="T:Newtonsoft.Json.Linq.JToken" /> to <see cref="T:System.Int32" />.
	/// </summary>
	/// <param name="value">The value.</param>
	/// <returns>The result of the conversion.</returns>
	public static explicit operator int(JToken value)
	{
		JValue jValue = JToken.EnsureValue(value);
		if (jValue == null || !JToken.ValidateToken(jValue, JToken.NumberTypes, nullable: false))
		{
			throw new ArgumentException("Can not convert {0} to Int32.".FormatWith(CultureInfo.InvariantCulture, JToken.GetType(value)));
		}
		if (jValue.Value is BigInteger bigInteger)
		{
			return (int)bigInteger;
		}
		return Convert.ToInt32(jValue.Value, CultureInfo.InvariantCulture);
	}

	/// <summary>
	/// Performs an explicit conversion from <see cref="T:Newtonsoft.Json.Linq.JToken" /> to <see cref="T:System.Int16" />.
	/// </summary>
	/// <param name="value">The value.</param>
	/// <returns>The result of the conversion.</returns>
	public static explicit operator short(JToken value)
	{
		JValue jValue = JToken.EnsureValue(value);
		if (jValue == null || !JToken.ValidateToken(jValue, JToken.NumberTypes, nullable: false))
		{
			throw new ArgumentException("Can not convert {0} to Int16.".FormatWith(CultureInfo.InvariantCulture, JToken.GetType(value)));
		}
		if (jValue.Value is BigInteger bigInteger)
		{
			return (short)bigInteger;
		}
		return Convert.ToInt16(jValue.Value, CultureInfo.InvariantCulture);
	}

	/// <summary>
	/// Performs an explicit conversion from <see cref="T:Newtonsoft.Json.Linq.JToken" /> to <see cref="T:System.UInt16" />.
	/// </summary>
	/// <param name="value">The value.</param>
	/// <returns>The result of the conversion.</returns>
	[CLSCompliant(false)]
	public static explicit operator ushort(JToken value)
	{
		JValue jValue = JToken.EnsureValue(value);
		if (jValue == null || !JToken.ValidateToken(jValue, JToken.NumberTypes, nullable: false))
		{
			throw new ArgumentException("Can not convert {0} to UInt16.".FormatWith(CultureInfo.InvariantCulture, JToken.GetType(value)));
		}
		if (jValue.Value is BigInteger bigInteger)
		{
			return (ushort)bigInteger;
		}
		return Convert.ToUInt16(jValue.Value, CultureInfo.InvariantCulture);
	}

	/// <summary>
	/// Performs an explicit conversion from <see cref="T:Newtonsoft.Json.Linq.JToken" /> to <see cref="T:System.Char" />.
	/// </summary>
	/// <param name="value">The value.</param>
	/// <returns>The result of the conversion.</returns>
	[CLSCompliant(false)]
	public static explicit operator char(JToken value)
	{
		JValue jValue = JToken.EnsureValue(value);
		if (jValue == null || !JToken.ValidateToken(jValue, JToken.CharTypes, nullable: false))
		{
			throw new ArgumentException("Can not convert {0} to Char.".FormatWith(CultureInfo.InvariantCulture, JToken.GetType(value)));
		}
		if (jValue.Value is BigInteger bigInteger)
		{
			return (char)(ushort)bigInteger;
		}
		return Convert.ToChar(jValue.Value, CultureInfo.InvariantCulture);
	}

	/// <summary>
	/// Performs an explicit conversion from <see cref="T:Newtonsoft.Json.Linq.JToken" /> to <see cref="T:System.Byte" />.
	/// </summary>
	/// <param name="value">The value.</param>
	/// <returns>The result of the conversion.</returns>
	public static explicit operator byte(JToken value)
	{
		JValue jValue = JToken.EnsureValue(value);
		if (jValue == null || !JToken.ValidateToken(jValue, JToken.NumberTypes, nullable: false))
		{
			throw new ArgumentException("Can not convert {0} to Byte.".FormatWith(CultureInfo.InvariantCulture, JToken.GetType(value)));
		}
		if (jValue.Value is BigInteger bigInteger)
		{
			return (byte)bigInteger;
		}
		return Convert.ToByte(jValue.Value, CultureInfo.InvariantCulture);
	}

	/// <summary>
	/// Performs an explicit conversion from <see cref="T:Newtonsoft.Json.Linq.JToken" /> to <see cref="T:System.SByte" />.
	/// </summary>
	/// <param name="value">The value.</param>
	/// <returns>The result of the conversion.</returns>
	[CLSCompliant(false)]
	public static explicit operator sbyte(JToken value)
	{
		JValue jValue = JToken.EnsureValue(value);
		if (jValue == null || !JToken.ValidateToken(jValue, JToken.NumberTypes, nullable: false))
		{
			throw new ArgumentException("Can not convert {0} to SByte.".FormatWith(CultureInfo.InvariantCulture, JToken.GetType(value)));
		}
		if (jValue.Value is BigInteger bigInteger)
		{
			return (sbyte)bigInteger;
		}
		return Convert.ToSByte(jValue.Value, CultureInfo.InvariantCulture);
	}

	/// <summary>
	/// Performs an explicit conversion from <see cref="T:Newtonsoft.Json.Linq.JToken" /> to <see cref="T:System.Nullable`1" /> of <see cref="T:System.Int32" /> .
	/// </summary>
	/// <param name="value">The value.</param>
	/// <returns>The result of the conversion.</returns>
	public static explicit operator int?(JToken? value)
	{
		if (value == null)
		{
			return null;
		}
		JValue jValue = JToken.EnsureValue(value);
		if (jValue == null || !JToken.ValidateToken(jValue, JToken.NumberTypes, nullable: true))
		{
			throw new ArgumentException("Can not convert {0} to Int32.".FormatWith(CultureInfo.InvariantCulture, JToken.GetType(value)));
		}
		if (jValue.Value is BigInteger bigInteger)
		{
			return (int)bigInteger;
		}
		if (jValue.Value == null)
		{
			return null;
		}
		return Convert.ToInt32(jValue.Value, CultureInfo.InvariantCulture);
	}

	/// <summary>
	/// Performs an explicit conversion from <see cref="T:Newtonsoft.Json.Linq.JToken" /> to <see cref="T:System.Nullable`1" /> of <see cref="T:System.Int16" />.
	/// </summary>
	/// <param name="value">The value.</param>
	/// <returns>The result of the conversion.</returns>
	public static explicit operator short?(JToken? value)
	{
		if (value == null)
		{
			return null;
		}
		JValue jValue = JToken.EnsureValue(value);
		if (jValue == null || !JToken.ValidateToken(jValue, JToken.NumberTypes, nullable: true))
		{
			throw new ArgumentException("Can not convert {0} to Int16.".FormatWith(CultureInfo.InvariantCulture, JToken.GetType(value)));
		}
		if (jValue.Value is BigInteger bigInteger)
		{
			return (short)bigInteger;
		}
		if (jValue.Value == null)
		{
			return null;
		}
		return Convert.ToInt16(jValue.Value, CultureInfo.InvariantCulture);
	}

	/// <summary>
	/// Performs an explicit conversion from <see cref="T:Newtonsoft.Json.Linq.JToken" /> to <see cref="T:System.Nullable`1" /> of <see cref="T:System.UInt16" />.
	/// </summary>
	/// <param name="value">The value.</param>
	/// <returns>The result of the conversion.</returns>
	[CLSCompliant(false)]
	public static explicit operator ushort?(JToken? value)
	{
		if (value == null)
		{
			return null;
		}
		JValue jValue = JToken.EnsureValue(value);
		if (jValue == null || !JToken.ValidateToken(jValue, JToken.NumberTypes, nullable: true))
		{
			throw new ArgumentException("Can not convert {0} to UInt16.".FormatWith(CultureInfo.InvariantCulture, JToken.GetType(value)));
		}
		if (jValue.Value is BigInteger bigInteger)
		{
			return (ushort)bigInteger;
		}
		if (jValue.Value == null)
		{
			return null;
		}
		return Convert.ToUInt16(jValue.Value, CultureInfo.InvariantCulture);
	}

	/// <summary>
	/// Performs an explicit conversion from <see cref="T:Newtonsoft.Json.Linq.JToken" /> to <see cref="T:System.Nullable`1" /> of <see cref="T:System.Byte" />.
	/// </summary>
	/// <param name="value">The value.</param>
	/// <returns>The result of the conversion.</returns>
	public static explicit operator byte?(JToken? value)
	{
		if (value == null)
		{
			return null;
		}
		JValue jValue = JToken.EnsureValue(value);
		if (jValue == null || !JToken.ValidateToken(jValue, JToken.NumberTypes, nullable: true))
		{
			throw new ArgumentException("Can not convert {0} to Byte.".FormatWith(CultureInfo.InvariantCulture, JToken.GetType(value)));
		}
		if (jValue.Value is BigInteger bigInteger)
		{
			return (byte)bigInteger;
		}
		if (jValue.Value == null)
		{
			return null;
		}
		return Convert.ToByte(jValue.Value, CultureInfo.InvariantCulture);
	}

	/// <summary>
	/// Performs an explicit conversion from <see cref="T:Newtonsoft.Json.Linq.JToken" /> to <see cref="T:System.Nullable`1" /> of <see cref="T:System.SByte" />.
	/// </summary>
	/// <param name="value">The value.</param>
	/// <returns>The result of the conversion.</returns>
	[CLSCompliant(false)]
	public static explicit operator sbyte?(JToken? value)
	{
		if (value == null)
		{
			return null;
		}
		JValue jValue = JToken.EnsureValue(value);
		if (jValue == null || !JToken.ValidateToken(jValue, JToken.NumberTypes, nullable: true))
		{
			throw new ArgumentException("Can not convert {0} to SByte.".FormatWith(CultureInfo.InvariantCulture, JToken.GetType(value)));
		}
		if (jValue.Value is BigInteger bigInteger)
		{
			return (sbyte)bigInteger;
		}
		if (jValue.Value == null)
		{
			return null;
		}
		return Convert.ToSByte(jValue.Value, CultureInfo.InvariantCulture);
	}

	/// <summary>
	/// Performs an explicit conversion from <see cref="T:Newtonsoft.Json.Linq.JToken" /> to <see cref="T:System.Nullable`1" /> of <see cref="T:System.DateTime" />.
	/// </summary>
	/// <param name="value">The value.</param>
	/// <returns>The result of the conversion.</returns>
	public static explicit operator DateTime(JToken value)
	{
		JValue jValue = JToken.EnsureValue(value);
		if (jValue == null || !JToken.ValidateToken(jValue, JToken.DateTimeTypes, nullable: false))
		{
			throw new ArgumentException("Can not convert {0} to DateTime.".FormatWith(CultureInfo.InvariantCulture, JToken.GetType(value)));
		}
		if (jValue.Value is DateTimeOffset dateTimeOffset)
		{
			return dateTimeOffset.DateTime;
		}
		return Convert.ToDateTime(jValue.Value, CultureInfo.InvariantCulture);
	}

	/// <summary>
	/// Performs an explicit conversion from <see cref="T:Newtonsoft.Json.Linq.JToken" /> to <see cref="T:System.Nullable`1" /> of <see cref="T:System.Int64" />.
	/// </summary>
	/// <param name="value">The value.</param>
	/// <returns>The result of the conversion.</returns>
	public static explicit operator long?(JToken? value)
	{
		if (value == null)
		{
			return null;
		}
		JValue jValue = JToken.EnsureValue(value);
		if (jValue == null || !JToken.ValidateToken(jValue, JToken.NumberTypes, nullable: true))
		{
			throw new ArgumentException("Can not convert {0} to Int64.".FormatWith(CultureInfo.InvariantCulture, JToken.GetType(value)));
		}
		if (jValue.Value is BigInteger bigInteger)
		{
			return (long)bigInteger;
		}
		if (jValue.Value == null)
		{
			return null;
		}
		return Convert.ToInt64(jValue.Value, CultureInfo.InvariantCulture);
	}

	/// <summary>
	/// Performs an explicit conversion from <see cref="T:Newtonsoft.Json.Linq.JToken" /> to <see cref="T:System.Nullable`1" /> of <see cref="T:System.Single" />.
	/// </summary>
	/// <param name="value">The value.</param>
	/// <returns>The result of the conversion.</returns>
	public static explicit operator float?(JToken? value)
	{
		if (value == null)
		{
			return null;
		}
		JValue jValue = JToken.EnsureValue(value);
		if (jValue == null || !JToken.ValidateToken(jValue, JToken.NumberTypes, nullable: true))
		{
			throw new ArgumentException("Can not convert {0} to Single.".FormatWith(CultureInfo.InvariantCulture, JToken.GetType(value)));
		}
		if (jValue.Value is BigInteger bigInteger)
		{
			return (float)bigInteger;
		}
		if (jValue.Value == null)
		{
			return null;
		}
		return Convert.ToSingle(jValue.Value, CultureInfo.InvariantCulture);
	}

	/// <summary>
	/// Performs an explicit conversion from <see cref="T:Newtonsoft.Json.Linq.JToken" /> to <see cref="T:System.Decimal" />.
	/// </summary>
	/// <param name="value">The value.</param>
	/// <returns>The result of the conversion.</returns>
	public static explicit operator decimal(JToken value)
	{
		JValue jValue = JToken.EnsureValue(value);
		if (jValue == null || !JToken.ValidateToken(jValue, JToken.NumberTypes, nullable: false))
		{
			throw new ArgumentException("Can not convert {0} to Decimal.".FormatWith(CultureInfo.InvariantCulture, JToken.GetType(value)));
		}
		if (jValue.Value is BigInteger bigInteger)
		{
			return (decimal)bigInteger;
		}
		return Convert.ToDecimal(jValue.Value, CultureInfo.InvariantCulture);
	}

	/// <summary>
	/// Performs an explicit conversion from <see cref="T:Newtonsoft.Json.Linq.JToken" /> to <see cref="T:System.Nullable`1" /> of <see cref="T:System.UInt32" />.
	/// </summary>
	/// <param name="value">The value.</param>
	/// <returns>The result of the conversion.</returns>
	[CLSCompliant(false)]
	public static explicit operator uint?(JToken? value)
	{
		if (value == null)
		{
			return null;
		}
		JValue jValue = JToken.EnsureValue(value);
		if (jValue == null || !JToken.ValidateToken(jValue, JToken.NumberTypes, nullable: true))
		{
			throw new ArgumentException("Can not convert {0} to UInt32.".FormatWith(CultureInfo.InvariantCulture, JToken.GetType(value)));
		}
		if (jValue.Value is BigInteger bigInteger)
		{
			return (uint)bigInteger;
		}
		if (jValue.Value == null)
		{
			return null;
		}
		return Convert.ToUInt32(jValue.Value, CultureInfo.InvariantCulture);
	}

	/// <summary>
	/// Performs an explicit conversion from <see cref="T:Newtonsoft.Json.Linq.JToken" /> to <see cref="T:System.Nullable`1" /> of <see cref="T:System.UInt64" />.
	/// </summary>
	/// <param name="value">The value.</param>
	/// <returns>The result of the conversion.</returns>
	[CLSCompliant(false)]
	public static explicit operator ulong?(JToken? value)
	{
		if (value == null)
		{
			return null;
		}
		JValue jValue = JToken.EnsureValue(value);
		if (jValue == null || !JToken.ValidateToken(jValue, JToken.NumberTypes, nullable: true))
		{
			throw new ArgumentException("Can not convert {0} to UInt64.".FormatWith(CultureInfo.InvariantCulture, JToken.GetType(value)));
		}
		if (jValue.Value is BigInteger bigInteger)
		{
			return (ulong)bigInteger;
		}
		if (jValue.Value == null)
		{
			return null;
		}
		return Convert.ToUInt64(jValue.Value, CultureInfo.InvariantCulture);
	}

	/// <summary>
	/// Performs an explicit conversion from <see cref="T:Newtonsoft.Json.Linq.JToken" /> to <see cref="T:System.Double" />.
	/// </summary>
	/// <param name="value">The value.</param>
	/// <returns>The result of the conversion.</returns>
	public static explicit operator double(JToken value)
	{
		JValue jValue = JToken.EnsureValue(value);
		if (jValue == null || !JToken.ValidateToken(jValue, JToken.NumberTypes, nullable: false))
		{
			throw new ArgumentException("Can not convert {0} to Double.".FormatWith(CultureInfo.InvariantCulture, JToken.GetType(value)));
		}
		if (jValue.Value is BigInteger bigInteger)
		{
			return (double)bigInteger;
		}
		return Convert.ToDouble(jValue.Value, CultureInfo.InvariantCulture);
	}

	/// <summary>
	/// Performs an explicit conversion from <see cref="T:Newtonsoft.Json.Linq.JToken" /> to <see cref="T:System.Single" />.
	/// </summary>
	/// <param name="value">The value.</param>
	/// <returns>The result of the conversion.</returns>
	public static explicit operator float(JToken value)
	{
		JValue jValue = JToken.EnsureValue(value);
		if (jValue == null || !JToken.ValidateToken(jValue, JToken.NumberTypes, nullable: false))
		{
			throw new ArgumentException("Can not convert {0} to Single.".FormatWith(CultureInfo.InvariantCulture, JToken.GetType(value)));
		}
		if (jValue.Value is BigInteger bigInteger)
		{
			return (float)bigInteger;
		}
		return Convert.ToSingle(jValue.Value, CultureInfo.InvariantCulture);
	}

	/// <summary>
	/// Performs an explicit conversion from <see cref="T:Newtonsoft.Json.Linq.JToken" /> to <see cref="T:System.String" />.
	/// </summary>
	/// <param name="value">The value.</param>
	/// <returns>The result of the conversion.</returns>
	public static explicit operator string?(JToken? value)
	{
		if (value == null)
		{
			return null;
		}
		JValue jValue = JToken.EnsureValue(value);
		if (jValue == null || !JToken.ValidateToken(jValue, JToken.StringTypes, nullable: true))
		{
			throw new ArgumentException("Can not convert {0} to String.".FormatWith(CultureInfo.InvariantCulture, JToken.GetType(value)));
		}
		if (jValue.Value == null)
		{
			return null;
		}
		if (jValue.Value is byte[] inArray)
		{
			return Convert.ToBase64String(inArray);
		}
		if (jValue.Value is BigInteger bigInteger)
		{
			return bigInteger.ToString(CultureInfo.InvariantCulture);
		}
		return Convert.ToString(jValue.Value, CultureInfo.InvariantCulture);
	}

	/// <summary>
	/// Performs an explicit conversion from <see cref="T:Newtonsoft.Json.Linq.JToken" /> to <see cref="T:System.UInt32" />.
	/// </summary>
	/// <param name="value">The value.</param>
	/// <returns>The result of the conversion.</returns>
	[CLSCompliant(false)]
	public static explicit operator uint(JToken value)
	{
		JValue jValue = JToken.EnsureValue(value);
		if (jValue == null || !JToken.ValidateToken(jValue, JToken.NumberTypes, nullable: false))
		{
			throw new ArgumentException("Can not convert {0} to UInt32.".FormatWith(CultureInfo.InvariantCulture, JToken.GetType(value)));
		}
		if (jValue.Value is BigInteger bigInteger)
		{
			return (uint)bigInteger;
		}
		return Convert.ToUInt32(jValue.Value, CultureInfo.InvariantCulture);
	}

	/// <summary>
	/// Performs an explicit conversion from <see cref="T:Newtonsoft.Json.Linq.JToken" /> to <see cref="T:System.UInt64" />.
	/// </summary>
	/// <param name="value">The value.</param>
	/// <returns>The result of the conversion.</returns>
	[CLSCompliant(false)]
	public static explicit operator ulong(JToken value)
	{
		JValue jValue = JToken.EnsureValue(value);
		if (jValue == null || !JToken.ValidateToken(jValue, JToken.NumberTypes, nullable: false))
		{
			throw new ArgumentException("Can not convert {0} to UInt64.".FormatWith(CultureInfo.InvariantCulture, JToken.GetType(value)));
		}
		if (jValue.Value is BigInteger bigInteger)
		{
			return (ulong)bigInteger;
		}
		return Convert.ToUInt64(jValue.Value, CultureInfo.InvariantCulture);
	}

	/// <summary>
	/// Performs an explicit conversion from <see cref="T:Newtonsoft.Json.Linq.JToken" /> to <see cref="T:System.Byte" />[].
	/// </summary>
	/// <param name="value">The value.</param>
	/// <returns>The result of the conversion.</returns>
	public static explicit operator byte[]?(JToken? value)
	{
		if (value == null)
		{
			return null;
		}
		JValue jValue = JToken.EnsureValue(value);
		if (jValue == null || !JToken.ValidateToken(jValue, JToken.BytesTypes, nullable: false))
		{
			throw new ArgumentException("Can not convert {0} to byte array.".FormatWith(CultureInfo.InvariantCulture, JToken.GetType(value)));
		}
		if (jValue.Value is string)
		{
			return Convert.FromBase64String(Convert.ToString(jValue.Value, CultureInfo.InvariantCulture));
		}
		if (jValue.Value is BigInteger bigInteger)
		{
			return bigInteger.ToByteArray();
		}
		if (jValue.Value is byte[] result)
		{
			return result;
		}
		throw new ArgumentException("Can not convert {0} to byte array.".FormatWith(CultureInfo.InvariantCulture, JToken.GetType(value)));
	}

	/// <summary>
	/// Performs an explicit conversion from <see cref="T:Newtonsoft.Json.Linq.JToken" /> to <see cref="T:System.Guid" />.
	/// </summary>
	/// <param name="value">The value.</param>
	/// <returns>The result of the conversion.</returns>
	public static explicit operator Guid(JToken value)
	{
		JValue jValue = JToken.EnsureValue(value);
		if (jValue == null || !JToken.ValidateToken(jValue, JToken.GuidTypes, nullable: false))
		{
			throw new ArgumentException("Can not convert {0} to Guid.".FormatWith(CultureInfo.InvariantCulture, JToken.GetType(value)));
		}
		if (jValue.Value is byte[] b)
		{
			return new Guid(b);
		}
		object value2 = jValue.Value;
		if (value2 is Guid)
		{
			return (Guid)value2;
		}
		return new Guid(Convert.ToString(jValue.Value, CultureInfo.InvariantCulture));
	}

	/// <summary>
	/// Performs an explicit conversion from <see cref="T:Newtonsoft.Json.Linq.JToken" /> to <see cref="T:System.Nullable`1" /> of <see cref="T:System.Guid" /> .
	/// </summary>
	/// <param name="value">The value.</param>
	/// <returns>The result of the conversion.</returns>
	public static explicit operator Guid?(JToken? value)
	{
		if (value == null)
		{
			return null;
		}
		JValue jValue = JToken.EnsureValue(value);
		if (jValue == null || !JToken.ValidateToken(jValue, JToken.GuidTypes, nullable: true))
		{
			throw new ArgumentException("Can not convert {0} to Guid.".FormatWith(CultureInfo.InvariantCulture, JToken.GetType(value)));
		}
		if (jValue.Value == null)
		{
			return null;
		}
		if (jValue.Value is byte[] b)
		{
			return new Guid(b);
		}
		return (jValue.Value is Guid guid) ? guid : new Guid(Convert.ToString(jValue.Value, CultureInfo.InvariantCulture));
	}

	/// <summary>
	/// Performs an explicit conversion from <see cref="T:Newtonsoft.Json.Linq.JToken" /> to <see cref="T:System.TimeSpan" />.
	/// </summary>
	/// <param name="value">The value.</param>
	/// <returns>The result of the conversion.</returns>
	public static explicit operator TimeSpan(JToken value)
	{
		JValue jValue = JToken.EnsureValue(value);
		if (jValue == null || !JToken.ValidateToken(jValue, JToken.TimeSpanTypes, nullable: false))
		{
			throw new ArgumentException("Can not convert {0} to TimeSpan.".FormatWith(CultureInfo.InvariantCulture, JToken.GetType(value)));
		}
		object value2 = jValue.Value;
		if (value2 is TimeSpan)
		{
			return (TimeSpan)value2;
		}
		return ConvertUtils.ParseTimeSpan(Convert.ToString(jValue.Value, CultureInfo.InvariantCulture));
	}

	/// <summary>
	/// Performs an explicit conversion from <see cref="T:Newtonsoft.Json.Linq.JToken" /> to <see cref="T:System.Nullable`1" /> of <see cref="T:System.TimeSpan" />.
	/// </summary>
	/// <param name="value">The value.</param>
	/// <returns>The result of the conversion.</returns>
	public static explicit operator TimeSpan?(JToken? value)
	{
		if (value == null)
		{
			return null;
		}
		JValue jValue = JToken.EnsureValue(value);
		if (jValue == null || !JToken.ValidateToken(jValue, JToken.TimeSpanTypes, nullable: true))
		{
			throw new ArgumentException("Can not convert {0} to TimeSpan.".FormatWith(CultureInfo.InvariantCulture, JToken.GetType(value)));
		}
		if (jValue.Value == null)
		{
			return null;
		}
		return (jValue.Value is TimeSpan timeSpan) ? timeSpan : ConvertUtils.ParseTimeSpan(Convert.ToString(jValue.Value, CultureInfo.InvariantCulture));
	}

	/// <summary>
	/// Performs an explicit conversion from <see cref="T:Newtonsoft.Json.Linq.JToken" /> to <see cref="T:System.Uri" />.
	/// </summary>
	/// <param name="value">The value.</param>
	/// <returns>The result of the conversion.</returns>
	public static explicit operator Uri?(JToken? value)
	{
		if (value == null)
		{
			return null;
		}
		JValue jValue = JToken.EnsureValue(value);
		if (jValue == null || !JToken.ValidateToken(jValue, JToken.UriTypes, nullable: true))
		{
			throw new ArgumentException("Can not convert {0} to Uri.".FormatWith(CultureInfo.InvariantCulture, JToken.GetType(value)));
		}
		if (jValue.Value == null)
		{
			return null;
		}
		if (!(jValue.Value is Uri result))
		{
			return new Uri(Convert.ToString(jValue.Value, CultureInfo.InvariantCulture));
		}
		return result;
	}

	private static BigInteger ToBigInteger(JToken value)
	{
		JValue jValue = JToken.EnsureValue(value);
		if (jValue == null || !JToken.ValidateToken(jValue, JToken.BigIntegerTypes, nullable: false))
		{
			throw new ArgumentException("Can not convert {0} to BigInteger.".FormatWith(CultureInfo.InvariantCulture, JToken.GetType(value)));
		}
		return ConvertUtils.ToBigInteger(jValue.Value);
	}

	private static BigInteger? ToBigIntegerNullable(JToken value)
	{
		JValue jValue = JToken.EnsureValue(value);
		if (jValue == null || !JToken.ValidateToken(jValue, JToken.BigIntegerTypes, nullable: true))
		{
			throw new ArgumentException("Can not convert {0} to BigInteger.".FormatWith(CultureInfo.InvariantCulture, JToken.GetType(value)));
		}
		if (jValue.Value == null)
		{
			return null;
		}
		return ConvertUtils.ToBigInteger(jValue.Value);
	}

	/// <summary>
	/// Performs an implicit conversion from <see cref="T:System.Boolean" /> to <see cref="T:Newtonsoft.Json.Linq.JToken" />.
	/// </summary>
	/// <param name="value">The value to create a <see cref="T:Newtonsoft.Json.Linq.JValue" /> from.</param>
	/// <returns>The <see cref="T:Newtonsoft.Json.Linq.JValue" /> initialized with the specified value.</returns>
	public static implicit operator JToken(bool value)
	{
		return new JValue(value);
	}

	/// <summary>
	/// Performs an implicit conversion from <see cref="T:System.DateTimeOffset" /> to <see cref="T:Newtonsoft.Json.Linq.JToken" />.
	/// </summary>
	/// <param name="value">The value to create a <see cref="T:Newtonsoft.Json.Linq.JValue" /> from.</param>
	/// <returns>The <see cref="T:Newtonsoft.Json.Linq.JValue" /> initialized with the specified value.</returns>
	public static implicit operator JToken(DateTimeOffset value)
	{
		return new JValue(value);
	}

	/// <summary>
	/// Performs an implicit conversion from <see cref="T:System.Byte" /> to <see cref="T:Newtonsoft.Json.Linq.JToken" />.
	/// </summary>
	/// <param name="value">The value to create a <see cref="T:Newtonsoft.Json.Linq.JValue" /> from.</param>
	/// <returns>The <see cref="T:Newtonsoft.Json.Linq.JValue" /> initialized with the specified value.</returns>
	public static implicit operator JToken(byte value)
	{
		return new JValue(value);
	}

	/// <summary>
	/// Performs an implicit conversion from <see cref="T:System.Nullable`1" /> of <see cref="T:System.Byte" /> to <see cref="T:Newtonsoft.Json.Linq.JToken" />.
	/// </summary>
	/// <param name="value">The value to create a <see cref="T:Newtonsoft.Json.Linq.JValue" /> from.</param>
	/// <returns>The <see cref="T:Newtonsoft.Json.Linq.JValue" /> initialized with the specified value.</returns>
	public static implicit operator JToken(byte? value)
	{
		return new JValue(value);
	}

	/// <summary>
	/// Performs an implicit conversion from <see cref="T:System.SByte" /> to <see cref="T:Newtonsoft.Json.Linq.JToken" />.
	/// </summary>
	/// <param name="value">The value to create a <see cref="T:Newtonsoft.Json.Linq.JValue" /> from.</param>
	/// <returns>The <see cref="T:Newtonsoft.Json.Linq.JValue" /> initialized with the specified value.</returns>
	[CLSCompliant(false)]
	public static implicit operator JToken(sbyte value)
	{
		return new JValue(value);
	}

	/// <summary>
	/// Performs an implicit conversion from <see cref="T:System.Nullable`1" /> of <see cref="T:System.SByte" /> to <see cref="T:Newtonsoft.Json.Linq.JToken" />.
	/// </summary>
	/// <param name="value">The value to create a <see cref="T:Newtonsoft.Json.Linq.JValue" /> from.</param>
	/// <returns>The <see cref="T:Newtonsoft.Json.Linq.JValue" /> initialized with the specified value.</returns>
	[CLSCompliant(false)]
	public static implicit operator JToken(sbyte? value)
	{
		return new JValue(value);
	}

	/// <summary>
	/// Performs an implicit conversion from <see cref="T:System.Nullable`1" /> of <see cref="T:System.Boolean" /> to <see cref="T:Newtonsoft.Json.Linq.JToken" />.
	/// </summary>
	/// <param name="value">The value to create a <see cref="T:Newtonsoft.Json.Linq.JValue" /> from.</param>
	/// <returns>The <see cref="T:Newtonsoft.Json.Linq.JValue" /> initialized with the specified value.</returns>
	public static implicit operator JToken(bool? value)
	{
		return new JValue(value);
	}

	/// <summary>
	/// Performs an implicit conversion from <see cref="T:System.Nullable`1" /> of <see cref="T:System.Int64" /> to <see cref="T:Newtonsoft.Json.Linq.JToken" />.
	/// </summary>
	/// <param name="value">The value to create a <see cref="T:Newtonsoft.Json.Linq.JValue" /> from.</param>
	/// <returns>The <see cref="T:Newtonsoft.Json.Linq.JValue" /> initialized with the specified value.</returns>
	public static implicit operator JToken(long value)
	{
		return new JValue(value);
	}

	/// <summary>
	/// Performs an implicit conversion from <see cref="T:System.Nullable`1" /> of <see cref="T:System.DateTime" /> to <see cref="T:Newtonsoft.Json.Linq.JToken" />.
	/// </summary>
	/// <param name="value">The value to create a <see cref="T:Newtonsoft.Json.Linq.JValue" /> from.</param>
	/// <returns>The <see cref="T:Newtonsoft.Json.Linq.JValue" /> initialized with the specified value.</returns>
	public static implicit operator JToken(DateTime? value)
	{
		return new JValue(value);
	}

	/// <summary>
	/// Performs an implicit conversion from <see cref="T:System.Nullable`1" /> of <see cref="T:System.DateTimeOffset" /> to <see cref="T:Newtonsoft.Json.Linq.JToken" />.
	/// </summary>
	/// <param name="value">The value to create a <see cref="T:Newtonsoft.Json.Linq.JValue" /> from.</param>
	/// <returns>The <see cref="T:Newtonsoft.Json.Linq.JValue" /> initialized with the specified value.</returns>
	public static implicit operator JToken(DateTimeOffset? value)
	{
		return new JValue(value);
	}

	/// <summary>
	/// Performs an implicit conversion from <see cref="T:System.Nullable`1" /> of <see cref="T:System.Decimal" /> to <see cref="T:Newtonsoft.Json.Linq.JToken" />.
	/// </summary>
	/// <param name="value">The value to create a <see cref="T:Newtonsoft.Json.Linq.JValue" /> from.</param>
	/// <returns>The <see cref="T:Newtonsoft.Json.Linq.JValue" /> initialized with the specified value.</returns>
	public static implicit operator JToken(decimal? value)
	{
		return new JValue(value);
	}

	/// <summary>
	/// Performs an implicit conversion from <see cref="T:System.Nullable`1" /> of <see cref="T:System.Double" /> to <see cref="T:Newtonsoft.Json.Linq.JToken" />.
	/// </summary>
	/// <param name="value">The value to create a <see cref="T:Newtonsoft.Json.Linq.JValue" /> from.</param>
	/// <returns>The <see cref="T:Newtonsoft.Json.Linq.JValue" /> initialized with the specified value.</returns>
	public static implicit operator JToken(double? value)
	{
		return new JValue(value);
	}

	/// <summary>
	/// Performs an implicit conversion from <see cref="T:System.Int16" /> to <see cref="T:Newtonsoft.Json.Linq.JToken" />.
	/// </summary>
	/// <param name="value">The value to create a <see cref="T:Newtonsoft.Json.Linq.JValue" /> from.</param>
	/// <returns>The <see cref="T:Newtonsoft.Json.Linq.JValue" /> initialized with the specified value.</returns>
	[CLSCompliant(false)]
	public static implicit operator JToken(short value)
	{
		return new JValue(value);
	}

	/// <summary>
	/// Performs an implicit conversion from <see cref="T:System.UInt16" /> to <see cref="T:Newtonsoft.Json.Linq.JToken" />.
	/// </summary>
	/// <param name="value">The value to create a <see cref="T:Newtonsoft.Json.Linq.JValue" /> from.</param>
	/// <returns>The <see cref="T:Newtonsoft.Json.Linq.JValue" /> initialized with the specified value.</returns>
	[CLSCompliant(false)]
	public static implicit operator JToken(ushort value)
	{
		return new JValue(value);
	}

	/// <summary>
	/// Performs an implicit conversion from <see cref="T:System.Int32" /> to <see cref="T:Newtonsoft.Json.Linq.JToken" />.
	/// </summary>
	/// <param name="value">The value to create a <see cref="T:Newtonsoft.Json.Linq.JValue" /> from.</param>
	/// <returns>The <see cref="T:Newtonsoft.Json.Linq.JValue" /> initialized with the specified value.</returns>
	public static implicit operator JToken(int value)
	{
		return new JValue(value);
	}

	/// <summary>
	/// Performs an implicit conversion from <see cref="T:System.Nullable`1" /> of <see cref="T:System.Int32" /> to <see cref="T:Newtonsoft.Json.Linq.JToken" />.
	/// </summary>
	/// <param name="value">The value to create a <see cref="T:Newtonsoft.Json.Linq.JValue" /> from.</param>
	/// <returns>The <see cref="T:Newtonsoft.Json.Linq.JValue" /> initialized with the specified value.</returns>
	public static implicit operator JToken(int? value)
	{
		return new JValue(value);
	}

	/// <summary>
	/// Performs an implicit conversion from <see cref="T:System.DateTime" /> to <see cref="T:Newtonsoft.Json.Linq.JToken" />.
	/// </summary>
	/// <param name="value">The value to create a <see cref="T:Newtonsoft.Json.Linq.JValue" /> from.</param>
	/// <returns>The <see cref="T:Newtonsoft.Json.Linq.JValue" /> initialized with the specified value.</returns>
	public static implicit operator JToken(DateTime value)
	{
		return new JValue(value);
	}

	/// <summary>
	/// Performs an implicit conversion from <see cref="T:System.Nullable`1" /> of <see cref="T:System.Int64" /> to <see cref="T:Newtonsoft.Json.Linq.JToken" />.
	/// </summary>
	/// <param name="value">The value to create a <see cref="T:Newtonsoft.Json.Linq.JValue" /> from.</param>
	/// <returns>The <see cref="T:Newtonsoft.Json.Linq.JValue" /> initialized with the specified value.</returns>
	public static implicit operator JToken(long? value)
	{
		return new JValue(value);
	}

	/// <summary>
	/// Performs an implicit conversion from <see cref="T:System.Nullable`1" /> of <see cref="T:System.Single" /> to <see cref="T:Newtonsoft.Json.Linq.JToken" />.
	/// </summary>
	/// <param name="value">The value to create a <see cref="T:Newtonsoft.Json.Linq.JValue" /> from.</param>
	/// <returns>The <see cref="T:Newtonsoft.Json.Linq.JValue" /> initialized with the specified value.</returns>
	public static implicit operator JToken(float? value)
	{
		return new JValue(value);
	}

	/// <summary>
	/// Performs an implicit conversion from <see cref="T:System.Decimal" /> to <see cref="T:Newtonsoft.Json.Linq.JToken" />.
	/// </summary>
	/// <param name="value">The value to create a <see cref="T:Newtonsoft.Json.Linq.JValue" /> from.</param>
	/// <returns>The <see cref="T:Newtonsoft.Json.Linq.JValue" /> initialized with the specified value.</returns>
	public static implicit operator JToken(decimal value)
	{
		return new JValue(value);
	}

	/// <summary>
	/// Performs an implicit conversion from <see cref="T:System.Nullable`1" /> of <see cref="T:System.Int16" /> to <see cref="T:Newtonsoft.Json.Linq.JToken" />.
	/// </summary>
	/// <param name="value">The value to create a <see cref="T:Newtonsoft.Json.Linq.JValue" /> from.</param>
	/// <returns>The <see cref="T:Newtonsoft.Json.Linq.JValue" /> initialized with the specified value.</returns>
	[CLSCompliant(false)]
	public static implicit operator JToken(short? value)
	{
		return new JValue(value);
	}

	/// <summary>
	/// Performs an implicit conversion from <see cref="T:System.Nullable`1" /> of <see cref="T:System.UInt16" /> to <see cref="T:Newtonsoft.Json.Linq.JToken" />.
	/// </summary>
	/// <param name="value">The value to create a <see cref="T:Newtonsoft.Json.Linq.JValue" /> from.</param>
	/// <returns>The <see cref="T:Newtonsoft.Json.Linq.JValue" /> initialized with the specified value.</returns>
	[CLSCompliant(false)]
	public static implicit operator JToken(ushort? value)
	{
		return new JValue(value);
	}

	/// <summary>
	/// Performs an implicit conversion from <see cref="T:System.Nullable`1" /> of <see cref="T:System.UInt32" /> to <see cref="T:Newtonsoft.Json.Linq.JToken" />.
	/// </summary>
	/// <param name="value">The value to create a <see cref="T:Newtonsoft.Json.Linq.JValue" /> from.</param>
	/// <returns>The <see cref="T:Newtonsoft.Json.Linq.JValue" /> initialized with the specified value.</returns>
	[CLSCompliant(false)]
	public static implicit operator JToken(uint? value)
	{
		return new JValue(value);
	}

	/// <summary>
	/// Performs an implicit conversion from <see cref="T:System.Nullable`1" /> of <see cref="T:System.UInt64" /> to <see cref="T:Newtonsoft.Json.Linq.JToken" />.
	/// </summary>
	/// <param name="value">The value to create a <see cref="T:Newtonsoft.Json.Linq.JValue" /> from.</param>
	/// <returns>The <see cref="T:Newtonsoft.Json.Linq.JValue" /> initialized with the specified value.</returns>
	[CLSCompliant(false)]
	public static implicit operator JToken(ulong? value)
	{
		return new JValue(value);
	}

	/// <summary>
	/// Performs an implicit conversion from <see cref="T:System.Double" /> to <see cref="T:Newtonsoft.Json.Linq.JToken" />.
	/// </summary>
	/// <param name="value">The value to create a <see cref="T:Newtonsoft.Json.Linq.JValue" /> from.</param>
	/// <returns>The <see cref="T:Newtonsoft.Json.Linq.JValue" /> initialized with the specified value.</returns>
	public static implicit operator JToken(double value)
	{
		return new JValue(value);
	}

	/// <summary>
	/// Performs an implicit conversion from <see cref="T:System.Single" /> to <see cref="T:Newtonsoft.Json.Linq.JToken" />.
	/// </summary>
	/// <param name="value">The value to create a <see cref="T:Newtonsoft.Json.Linq.JValue" /> from.</param>
	/// <returns>The <see cref="T:Newtonsoft.Json.Linq.JValue" /> initialized with the specified value.</returns>
	public static implicit operator JToken(float value)
	{
		return new JValue(value);
	}

	/// <summary>
	/// Performs an implicit conversion from <see cref="T:System.String" /> to <see cref="T:Newtonsoft.Json.Linq.JToken" />.
	/// </summary>
	/// <param name="value">The value to create a <see cref="T:Newtonsoft.Json.Linq.JValue" /> from.</param>
	/// <returns>The <see cref="T:Newtonsoft.Json.Linq.JValue" /> initialized with the specified value.</returns>
	public static implicit operator JToken(string? value)
	{
		return new JValue(value);
	}

	/// <summary>
	/// Performs an implicit conversion from <see cref="T:System.UInt32" /> to <see cref="T:Newtonsoft.Json.Linq.JToken" />.
	/// </summary>
	/// <param name="value">The value to create a <see cref="T:Newtonsoft.Json.Linq.JValue" /> from.</param>
	/// <returns>The <see cref="T:Newtonsoft.Json.Linq.JValue" /> initialized with the specified value.</returns>
	[CLSCompliant(false)]
	public static implicit operator JToken(uint value)
	{
		return new JValue(value);
	}

	/// <summary>
	/// Performs an implicit conversion from <see cref="T:System.UInt64" /> to <see cref="T:Newtonsoft.Json.Linq.JToken" />.
	/// </summary>
	/// <param name="value">The value to create a <see cref="T:Newtonsoft.Json.Linq.JValue" /> from.</param>
	/// <returns>The <see cref="T:Newtonsoft.Json.Linq.JValue" /> initialized with the specified value.</returns>
	[CLSCompliant(false)]
	public static implicit operator JToken(ulong value)
	{
		return new JValue(value);
	}

	/// <summary>
	/// Performs an implicit conversion from <see cref="T:System.Byte" />[] to <see cref="T:Newtonsoft.Json.Linq.JToken" />.
	/// </summary>
	/// <param name="value">The value to create a <see cref="T:Newtonsoft.Json.Linq.JValue" /> from.</param>
	/// <returns>The <see cref="T:Newtonsoft.Json.Linq.JValue" /> initialized with the specified value.</returns>
	public static implicit operator JToken(byte[] value)
	{
		return new JValue(value);
	}

	/// <summary>
	/// Performs an implicit conversion from <see cref="T:System.Uri" /> to <see cref="T:Newtonsoft.Json.Linq.JToken" />.
	/// </summary>
	/// <param name="value">The value to create a <see cref="T:Newtonsoft.Json.Linq.JValue" /> from.</param>
	/// <returns>The <see cref="T:Newtonsoft.Json.Linq.JValue" /> initialized with the specified value.</returns>
	public static implicit operator JToken(Uri? value)
	{
		return new JValue(value);
	}

	/// <summary>
	/// Performs an implicit conversion from <see cref="T:System.TimeSpan" /> to <see cref="T:Newtonsoft.Json.Linq.JToken" />.
	/// </summary>
	/// <param name="value">The value to create a <see cref="T:Newtonsoft.Json.Linq.JValue" /> from.</param>
	/// <returns>The <see cref="T:Newtonsoft.Json.Linq.JValue" /> initialized with the specified value.</returns>
	public static implicit operator JToken(TimeSpan value)
	{
		return new JValue(value);
	}

	/// <summary>
	/// Performs an implicit conversion from <see cref="T:System.Nullable`1" /> of <see cref="T:System.TimeSpan" /> to <see cref="T:Newtonsoft.Json.Linq.JToken" />.
	/// </summary>
	/// <param name="value">The value to create a <see cref="T:Newtonsoft.Json.Linq.JValue" /> from.</param>
	/// <returns>The <see cref="T:Newtonsoft.Json.Linq.JValue" /> initialized with the specified value.</returns>
	public static implicit operator JToken(TimeSpan? value)
	{
		return new JValue(value);
	}

	/// <summary>
	/// Performs an implicit conversion from <see cref="T:System.Guid" /> to <see cref="T:Newtonsoft.Json.Linq.JToken" />.
	/// </summary>
	/// <param name="value">The value to create a <see cref="T:Newtonsoft.Json.Linq.JValue" /> from.</param>
	/// <returns>The <see cref="T:Newtonsoft.Json.Linq.JValue" /> initialized with the specified value.</returns>
	public static implicit operator JToken(Guid value)
	{
		return new JValue(value);
	}

	/// <summary>
	/// Performs an implicit conversion from <see cref="T:System.Nullable`1" /> of <see cref="T:System.Guid" /> to <see cref="T:Newtonsoft.Json.Linq.JToken" />.
	/// </summary>
	/// <param name="value">The value to create a <see cref="T:Newtonsoft.Json.Linq.JValue" /> from.</param>
	/// <returns>The <see cref="T:Newtonsoft.Json.Linq.JValue" /> initialized with the specified value.</returns>
	public static implicit operator JToken(Guid? value)
	{
		return new JValue(value);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return ((IEnumerable<JToken>)this).GetEnumerator();
	}

	IEnumerator<JToken> IEnumerable<JToken>.GetEnumerator()
	{
		return this.Children().GetEnumerator();
	}

	internal abstract int GetDeepHashCode();

	/// <summary>
	/// Creates a <see cref="T:Newtonsoft.Json.JsonReader" /> for this token.
	/// </summary>
	/// <returns>A <see cref="T:Newtonsoft.Json.JsonReader" /> that can be used to read this token and its descendants.</returns>
	public JsonReader CreateReader()
	{
		return new JTokenReader(this);
	}

	internal static JToken FromObjectInternal(object o, JsonSerializer jsonSerializer)
	{
		ValidationUtils.ArgumentNotNull(o, "o");
		ValidationUtils.ArgumentNotNull(jsonSerializer, "jsonSerializer");
		using JTokenWriter jTokenWriter = new JTokenWriter();
		jsonSerializer.Serialize(jTokenWriter, o);
		return jTokenWriter.Token;
	}

	/// <summary>
	/// Creates a <see cref="T:Newtonsoft.Json.Linq.JToken" /> from an object.
	/// </summary>
	/// <param name="o">The object that will be used to create <see cref="T:Newtonsoft.Json.Linq.JToken" />.</param>
	/// <returns>A <see cref="T:Newtonsoft.Json.Linq.JToken" /> with the value of the specified object.</returns>
	public static JToken FromObject(object o)
	{
		return JToken.FromObjectInternal(o, JsonSerializer.CreateDefault());
	}

	/// <summary>
	/// Creates a <see cref="T:Newtonsoft.Json.Linq.JToken" /> from an object using the specified <see cref="T:Newtonsoft.Json.JsonSerializer" />.
	/// </summary>
	/// <param name="o">The object that will be used to create <see cref="T:Newtonsoft.Json.Linq.JToken" />.</param>
	/// <param name="jsonSerializer">The <see cref="T:Newtonsoft.Json.JsonSerializer" /> that will be used when reading the object.</param>
	/// <returns>A <see cref="T:Newtonsoft.Json.Linq.JToken" /> with the value of the specified object.</returns>
	public static JToken FromObject(object o, JsonSerializer jsonSerializer)
	{
		return JToken.FromObjectInternal(o, jsonSerializer);
	}

	/// <summary>
	/// Creates an instance of the specified .NET type from the <see cref="T:Newtonsoft.Json.Linq.JToken" />.
	/// </summary>
	/// <typeparam name="T">The object type that the token will be deserialized to.</typeparam>
	/// <returns>The new object created from the JSON value.</returns>
	public T? ToObject<T>()
	{
		return (T)this.ToObject(typeof(T));
	}

	/// <summary>
	/// Creates an instance of the specified .NET type from the <see cref="T:Newtonsoft.Json.Linq.JToken" />.
	/// </summary>
	/// <param name="objectType">The object type that the token will be deserialized to.</param>
	/// <returns>The new object created from the JSON value.</returns>
	public object? ToObject(Type objectType)
	{
		if (JsonConvert.DefaultSettings == null)
		{
			bool isEnum;
			PrimitiveTypeCode typeCode = ConvertUtils.GetTypeCode(objectType, out isEnum);
			if (isEnum)
			{
				if (this.Type == JTokenType.String)
				{
					try
					{
						return this.ToObject(objectType, JsonSerializer.CreateDefault());
					}
					catch (Exception innerException)
					{
						Type type = (objectType.IsEnum() ? objectType : Nullable.GetUnderlyingType(objectType));
						throw new ArgumentException("Could not convert '{0}' to {1}.".FormatWith(CultureInfo.InvariantCulture, (string?)this, type.Name), innerException);
					}
				}
				if (this.Type == JTokenType.Integer)
				{
					return Enum.ToObject(objectType.IsEnum() ? objectType : Nullable.GetUnderlyingType(objectType), ((JValue)this).Value);
				}
			}
			switch (typeCode)
			{
			case PrimitiveTypeCode.BooleanNullable:
				return (bool?)this;
			case PrimitiveTypeCode.Boolean:
				return (bool)this;
			case PrimitiveTypeCode.CharNullable:
				return (char?)this;
			case PrimitiveTypeCode.Char:
				return (char)this;
			case PrimitiveTypeCode.SByte:
				return (sbyte)this;
			case PrimitiveTypeCode.SByteNullable:
				return (sbyte?)this;
			case PrimitiveTypeCode.ByteNullable:
				return (byte?)this;
			case PrimitiveTypeCode.Byte:
				return (byte)this;
			case PrimitiveTypeCode.Int16Nullable:
				return (short?)this;
			case PrimitiveTypeCode.Int16:
				return (short)this;
			case PrimitiveTypeCode.UInt16Nullable:
				return (ushort?)this;
			case PrimitiveTypeCode.UInt16:
				return (ushort)this;
			case PrimitiveTypeCode.Int32Nullable:
				return (int?)this;
			case PrimitiveTypeCode.Int32:
				return (int)this;
			case PrimitiveTypeCode.UInt32Nullable:
				return (uint?)this;
			case PrimitiveTypeCode.UInt32:
				return (uint)this;
			case PrimitiveTypeCode.Int64Nullable:
				return (long?)this;
			case PrimitiveTypeCode.Int64:
				return (long)this;
			case PrimitiveTypeCode.UInt64Nullable:
				return (ulong?)this;
			case PrimitiveTypeCode.UInt64:
				return (ulong)this;
			case PrimitiveTypeCode.SingleNullable:
				return (float?)this;
			case PrimitiveTypeCode.Single:
				return (float)this;
			case PrimitiveTypeCode.DoubleNullable:
				return (double?)this;
			case PrimitiveTypeCode.Double:
				return (double)this;
			case PrimitiveTypeCode.DecimalNullable:
				return (decimal?)this;
			case PrimitiveTypeCode.Decimal:
				return (decimal)this;
			case PrimitiveTypeCode.DateTimeNullable:
				return (DateTime?)this;
			case PrimitiveTypeCode.DateTime:
				return (DateTime)this;
			case PrimitiveTypeCode.DateTimeOffsetNullable:
				return (DateTimeOffset?)this;
			case PrimitiveTypeCode.DateTimeOffset:
				return (DateTimeOffset)this;
			case PrimitiveTypeCode.String:
				return (string?)this;
			case PrimitiveTypeCode.GuidNullable:
				return (Guid?)this;
			case PrimitiveTypeCode.Guid:
				return (Guid)this;
			case PrimitiveTypeCode.Uri:
				return (Uri?)this;
			case PrimitiveTypeCode.TimeSpanNullable:
				return (TimeSpan?)this;
			case PrimitiveTypeCode.TimeSpan:
				return (TimeSpan)this;
			case PrimitiveTypeCode.BigIntegerNullable:
				return JToken.ToBigIntegerNullable(this);
			case PrimitiveTypeCode.BigInteger:
				return JToken.ToBigInteger(this);
			}
		}
		return this.ToObject(objectType, JsonSerializer.CreateDefault());
	}

	/// <summary>
	/// Creates an instance of the specified .NET type from the <see cref="T:Newtonsoft.Json.Linq.JToken" /> using the specified <see cref="T:Newtonsoft.Json.JsonSerializer" />.
	/// </summary>
	/// <typeparam name="T">The object type that the token will be deserialized to.</typeparam>
	/// <param name="jsonSerializer">The <see cref="T:Newtonsoft.Json.JsonSerializer" /> that will be used when creating the object.</param>
	/// <returns>The new object created from the JSON value.</returns>
	public T? ToObject<T>(JsonSerializer jsonSerializer)
	{
		return (T)this.ToObject(typeof(T), jsonSerializer);
	}

	/// <summary>
	/// Creates an instance of the specified .NET type from the <see cref="T:Newtonsoft.Json.Linq.JToken" /> using the specified <see cref="T:Newtonsoft.Json.JsonSerializer" />.
	/// </summary>
	/// <param name="objectType">The object type that the token will be deserialized to.</param>
	/// <param name="jsonSerializer">The <see cref="T:Newtonsoft.Json.JsonSerializer" /> that will be used when creating the object.</param>
	/// <returns>The new object created from the JSON value.</returns>
	public object? ToObject(Type? objectType, JsonSerializer jsonSerializer)
	{
		ValidationUtils.ArgumentNotNull(jsonSerializer, "jsonSerializer");
		using JTokenReader reader = new JTokenReader(this);
		if (jsonSerializer is JsonSerializerProxy jsonSerializerProxy)
		{
			jsonSerializerProxy._serializer.SetupReader(reader, out CultureInfo _, out DateTimeZoneHandling? _, out DateParseHandling? _, out FloatParseHandling? _, out int? _, out string _);
		}
		return jsonSerializer.Deserialize(reader, objectType);
	}

	/// <summary>
	/// Creates a <see cref="T:Newtonsoft.Json.Linq.JToken" /> from a <see cref="T:Newtonsoft.Json.JsonReader" />.
	/// </summary>
	/// <param name="reader">A <see cref="T:Newtonsoft.Json.JsonReader" /> positioned at the token to read into this <see cref="T:Newtonsoft.Json.Linq.JToken" />.</param>
	/// <returns>
	/// A <see cref="T:Newtonsoft.Json.Linq.JToken" /> that contains the token and its descendant tokens
	/// that were read from the reader. The runtime type of the token is determined
	/// by the token type of the first token encountered in the reader.
	/// </returns>
	public static JToken ReadFrom(JsonReader reader)
	{
		return JToken.ReadFrom(reader, null);
	}

	/// <summary>
	/// Creates a <see cref="T:Newtonsoft.Json.Linq.JToken" /> from a <see cref="T:Newtonsoft.Json.JsonReader" />.
	/// </summary>
	/// <param name="reader">An <see cref="T:Newtonsoft.Json.JsonReader" /> positioned at the token to read into this <see cref="T:Newtonsoft.Json.Linq.JToken" />.</param>
	/// <param name="settings">The <see cref="T:Newtonsoft.Json.Linq.JsonLoadSettings" /> used to load the JSON.
	/// If this is <c>null</c>, default load settings will be used.</param>
	/// <returns>
	/// A <see cref="T:Newtonsoft.Json.Linq.JToken" /> that contains the token and its descendant tokens
	/// that were read from the reader. The runtime type of the token is determined
	/// by the token type of the first token encountered in the reader.
	/// </returns>
	public static JToken ReadFrom(JsonReader reader, JsonLoadSettings? settings)
	{
		ValidationUtils.ArgumentNotNull(reader, "reader");
		if (!((reader.TokenType == JsonToken.None) ? ((settings != null && settings.CommentHandling == CommentHandling.Ignore) ? reader.ReadAndMoveToContent() : reader.Read()) : (reader.TokenType != JsonToken.Comment || settings == null || settings.CommentHandling != CommentHandling.Ignore || reader.ReadAndMoveToContent())))
		{
			throw JsonReaderException.Create(reader, "Error reading JToken from JsonReader.");
		}
		IJsonLineInfo lineInfo = reader as IJsonLineInfo;
		switch (reader.TokenType)
		{
		case JsonToken.StartObject:
			return JObject.Load(reader, settings);
		case JsonToken.StartArray:
			return JArray.Load(reader, settings);
		case JsonToken.StartConstructor:
			return JConstructor.Load(reader, settings);
		case JsonToken.PropertyName:
			return JProperty.Load(reader, settings);
		case JsonToken.Integer:
		case JsonToken.Float:
		case JsonToken.String:
		case JsonToken.Boolean:
		case JsonToken.Date:
		case JsonToken.Bytes:
		{
			JValue jValue4 = new JValue(reader.Value);
			jValue4.SetLineInfo(lineInfo, settings);
			return jValue4;
		}
		case JsonToken.Comment:
		{
			JValue jValue3 = JValue.CreateComment(reader.Value.ToString());
			jValue3.SetLineInfo(lineInfo, settings);
			return jValue3;
		}
		case JsonToken.Null:
		{
			JValue jValue2 = JValue.CreateNull();
			jValue2.SetLineInfo(lineInfo, settings);
			return jValue2;
		}
		case JsonToken.Undefined:
		{
			JValue jValue = JValue.CreateUndefined();
			jValue.SetLineInfo(lineInfo, settings);
			return jValue;
		}
		default:
			throw JsonReaderException.Create(reader, "Error reading JToken from JsonReader. Unexpected token: {0}".FormatWith(CultureInfo.InvariantCulture, reader.TokenType));
		}
	}

	/// <summary>
	/// Load a <see cref="T:Newtonsoft.Json.Linq.JToken" /> from a string that contains JSON.
	/// </summary>
	/// <param name="json">A <see cref="T:System.String" /> that contains JSON.</param>
	/// <returns>A <see cref="T:Newtonsoft.Json.Linq.JToken" /> populated from the string that contains JSON.</returns>
	public static JToken Parse(string json)
	{
		return JToken.Parse(json, null);
	}

	/// <summary>
	/// Load a <see cref="T:Newtonsoft.Json.Linq.JToken" /> from a string that contains JSON.
	/// </summary>
	/// <param name="json">A <see cref="T:System.String" /> that contains JSON.</param>
	/// <param name="settings">The <see cref="T:Newtonsoft.Json.Linq.JsonLoadSettings" /> used to load the JSON.
	/// If this is <c>null</c>, default load settings will be used.</param>
	/// <returns>A <see cref="T:Newtonsoft.Json.Linq.JToken" /> populated from the string that contains JSON.</returns>
	public static JToken Parse(string json, JsonLoadSettings? settings)
	{
		using JsonReader jsonReader = new JsonTextReader(new StringReader(json));
		JToken result = JToken.Load(jsonReader, settings);
		while (jsonReader.Read())
		{
		}
		return result;
	}

	/// <summary>
	/// Creates a <see cref="T:Newtonsoft.Json.Linq.JToken" /> from a <see cref="T:Newtonsoft.Json.JsonReader" />.
	/// </summary>
	/// <param name="reader">A <see cref="T:Newtonsoft.Json.JsonReader" /> positioned at the token to read into this <see cref="T:Newtonsoft.Json.Linq.JToken" />.</param>
	/// <param name="settings">The <see cref="T:Newtonsoft.Json.Linq.JsonLoadSettings" /> used to load the JSON.
	/// If this is <c>null</c>, default load settings will be used.</param>
	/// <returns>
	/// A <see cref="T:Newtonsoft.Json.Linq.JToken" /> that contains the token and its descendant tokens
	/// that were read from the reader. The runtime type of the token is determined
	/// by the token type of the first token encountered in the reader.
	/// </returns>
	public static JToken Load(JsonReader reader, JsonLoadSettings? settings)
	{
		return JToken.ReadFrom(reader, settings);
	}

	/// <summary>
	/// Creates a <see cref="T:Newtonsoft.Json.Linq.JToken" /> from a <see cref="T:Newtonsoft.Json.JsonReader" />.
	/// </summary>
	/// <param name="reader">A <see cref="T:Newtonsoft.Json.JsonReader" /> positioned at the token to read into this <see cref="T:Newtonsoft.Json.Linq.JToken" />.</param>
	/// <returns>
	/// A <see cref="T:Newtonsoft.Json.Linq.JToken" /> that contains the token and its descendant tokens
	/// that were read from the reader. The runtime type of the token is determined
	/// by the token type of the first token encountered in the reader.
	/// </returns>
	public static JToken Load(JsonReader reader)
	{
		return JToken.Load(reader, null);
	}

	internal void SetLineInfo(IJsonLineInfo? lineInfo, JsonLoadSettings? settings)
	{
		if ((settings == null || settings.LineInfoHandling == LineInfoHandling.Load) && lineInfo != null && lineInfo.HasLineInfo())
		{
			this.SetLineInfo(lineInfo.LineNumber, lineInfo.LinePosition);
		}
	}

	internal void SetLineInfo(int lineNumber, int linePosition)
	{
		this.AddAnnotation(new LineInfoAnnotation(lineNumber, linePosition));
	}

	bool IJsonLineInfo.HasLineInfo()
	{
		return this.Annotation<LineInfoAnnotation>() != null;
	}

	/// <summary>
	/// Selects a <see cref="T:Newtonsoft.Json.Linq.JToken" /> using a JSONPath expression. Selects the token that matches the object path.
	/// </summary>
	/// <param name="path">
	/// A <see cref="T:System.String" /> that contains a JSONPath expression.
	/// </param>
	/// <returns>A <see cref="T:Newtonsoft.Json.Linq.JToken" />, or <c>null</c>.</returns>
	public JToken? SelectToken(string path)
	{
		return this.SelectToken(path, null);
	}

	/// <summary>
	/// Selects a <see cref="T:Newtonsoft.Json.Linq.JToken" /> using a JSONPath expression. Selects the token that matches the object path.
	/// </summary>
	/// <param name="path">
	/// A <see cref="T:System.String" /> that contains a JSONPath expression.
	/// </param>
	/// <param name="errorWhenNoMatch">A flag to indicate whether an error should be thrown if no tokens are found when evaluating part of the expression.</param>
	/// <returns>A <see cref="T:Newtonsoft.Json.Linq.JToken" />.</returns>
	public JToken? SelectToken(string path, bool errorWhenNoMatch)
	{
		JsonSelectSettings settings = (errorWhenNoMatch ? new JsonSelectSettings
		{
			ErrorWhenNoMatch = true
		} : null);
		return this.SelectToken(path, settings);
	}

	/// <summary>
	/// Selects a <see cref="T:Newtonsoft.Json.Linq.JToken" /> using a JSONPath expression. Selects the token that matches the object path.
	/// </summary>
	/// <param name="path">
	/// A <see cref="T:System.String" /> that contains a JSONPath expression.
	/// </param>
	/// <param name="settings">The <see cref="T:Newtonsoft.Json.Linq.JsonSelectSettings" /> used to select tokens.</param>
	/// <returns>A <see cref="T:Newtonsoft.Json.Linq.JToken" />.</returns>
	public JToken? SelectToken(string path, JsonSelectSettings? settings)
	{
		JPath jPath = new JPath(path);
		JToken jToken = null;
		foreach (JToken item in jPath.Evaluate(this, this, settings))
		{
			if (jToken != null)
			{
				throw new JsonException("Path returned multiple tokens.");
			}
			jToken = item;
		}
		return jToken;
	}

	/// <summary>
	/// Selects a collection of elements using a JSONPath expression.
	/// </summary>
	/// <param name="path">
	/// A <see cref="T:System.String" /> that contains a JSONPath expression.
	/// </param>
	/// <returns>An <see cref="T:System.Collections.Generic.IEnumerable`1" /> of <see cref="T:Newtonsoft.Json.Linq.JToken" /> that contains the selected elements.</returns>
	public IEnumerable<JToken> SelectTokens(string path)
	{
		return this.SelectTokens(path, null);
	}

	/// <summary>
	/// Selects a collection of elements using a JSONPath expression.
	/// </summary>
	/// <param name="path">
	/// A <see cref="T:System.String" /> that contains a JSONPath expression.
	/// </param>
	/// <param name="errorWhenNoMatch">A flag to indicate whether an error should be thrown if no tokens are found when evaluating part of the expression.</param>
	/// <returns>An <see cref="T:System.Collections.Generic.IEnumerable`1" /> of <see cref="T:Newtonsoft.Json.Linq.JToken" /> that contains the selected elements.</returns>
	public IEnumerable<JToken> SelectTokens(string path, bool errorWhenNoMatch)
	{
		JsonSelectSettings settings = (errorWhenNoMatch ? new JsonSelectSettings
		{
			ErrorWhenNoMatch = true
		} : null);
		return this.SelectTokens(path, settings);
	}

	/// <summary>
	/// Selects a collection of elements using a JSONPath expression.
	/// </summary>
	/// <param name="path">
	/// A <see cref="T:System.String" /> that contains a JSONPath expression.
	/// </param>
	/// <param name="settings">The <see cref="T:Newtonsoft.Json.Linq.JsonSelectSettings" /> used to select tokens.</param>
	/// <returns>An <see cref="T:System.Collections.Generic.IEnumerable`1" /> of <see cref="T:Newtonsoft.Json.Linq.JToken" /> that contains the selected elements.</returns>
	public IEnumerable<JToken> SelectTokens(string path, JsonSelectSettings? settings)
	{
		return new JPath(path).Evaluate(this, this, settings);
	}

	/// <summary>
	/// Returns the <see cref="T:System.Dynamic.DynamicMetaObject" /> responsible for binding operations performed on this object.
	/// </summary>
	/// <param name="parameter">The expression tree representation of the runtime value.</param>
	/// <returns>
	/// The <see cref="T:System.Dynamic.DynamicMetaObject" /> to bind this object.
	/// </returns>
	protected virtual DynamicMetaObject GetMetaObject(Expression parameter)
	{
		return new DynamicProxyMetaObject<JToken>(parameter, this, new DynamicProxy<JToken>());
	}

	/// <summary>
	/// Returns the <see cref="T:System.Dynamic.DynamicMetaObject" /> responsible for binding operations performed on this object.
	/// </summary>
	/// <param name="parameter">The expression tree representation of the runtime value.</param>
	/// <returns>
	/// The <see cref="T:System.Dynamic.DynamicMetaObject" /> to bind this object.
	/// </returns>
	DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter)
	{
		return this.GetMetaObject(parameter);
	}

	object ICloneable.Clone()
	{
		return this.DeepClone();
	}

	/// <summary>
	/// Creates a new instance of the <see cref="T:Newtonsoft.Json.Linq.JToken" />. All child tokens are recursively cloned.
	/// </summary>
	/// <returns>A new instance of the <see cref="T:Newtonsoft.Json.Linq.JToken" />.</returns>
	public JToken DeepClone()
	{
		return this.CloneToken(null);
	}

	/// <summary>
	/// Creates a new instance of the <see cref="T:Newtonsoft.Json.Linq.JToken" />. All child tokens are recursively cloned.
	/// </summary>
	/// <param name="settings">A <see cref="T:Newtonsoft.Json.Linq.JsonCloneSettings" /> object to configure cloning settings.</param>
	/// <returns>A new instance of the <see cref="T:Newtonsoft.Json.Linq.JToken" />.</returns>
	public JToken DeepClone(JsonCloneSettings settings)
	{
		return this.CloneToken(settings);
	}

	/// <summary>
	/// Adds an object to the annotation list of this <see cref="T:Newtonsoft.Json.Linq.JToken" />.
	/// </summary>
	/// <param name="annotation">The annotation to add.</param>
	public void AddAnnotation(object annotation)
	{
		if (annotation == null)
		{
			throw new ArgumentNullException("annotation");
		}
		if (this._annotations == null)
		{
			this._annotations = ((!(annotation is object[])) ? annotation : new object[1] { annotation });
			return;
		}
		object[] array = this._annotations as object[];
		if (array == null)
		{
			this._annotations = new object[2] { this._annotations, annotation };
			return;
		}
		int i;
		for (i = 0; i < array.Length && array[i] != null; i++)
		{
		}
		if (i == array.Length)
		{
			Array.Resize(ref array, i * 2);
			this._annotations = array;
		}
		array[i] = annotation;
	}

	/// <summary>
	/// Get the first annotation object of the specified type from this <see cref="T:Newtonsoft.Json.Linq.JToken" />.
	/// </summary>
	/// <typeparam name="T">The type of the annotation to retrieve.</typeparam>
	/// <returns>The first annotation object that matches the specified type, or <c>null</c> if no annotation is of the specified type.</returns>
	public T? Annotation<T>() where T : class
	{
		if (this._annotations != null)
		{
			if (!(this._annotations is object[] array))
			{
				return this._annotations as T;
			}
			foreach (object obj in array)
			{
				if (obj == null)
				{
					break;
				}
				if (obj is T result)
				{
					return result;
				}
			}
		}
		return null;
	}

	/// <summary>
	/// Gets the first annotation object of the specified type from this <see cref="T:Newtonsoft.Json.Linq.JToken" />.
	/// </summary>
	/// <param name="type">The <see cref="P:Newtonsoft.Json.Linq.JToken.Type" /> of the annotation to retrieve.</param>
	/// <returns>The first annotation object that matches the specified type, or <c>null</c> if no annotation is of the specified type.</returns>
	public object? Annotation(Type type)
	{
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		if (this._annotations != null)
		{
			if (!(this._annotations is object[] array))
			{
				if (type.IsInstanceOfType(this._annotations))
				{
					return this._annotations;
				}
			}
			else
			{
				foreach (object obj in array)
				{
					if (obj == null)
					{
						break;
					}
					if (type.IsInstanceOfType(obj))
					{
						return obj;
					}
				}
			}
		}
		return null;
	}

	/// <summary>
	/// Gets a collection of annotations of the specified type for this <see cref="T:Newtonsoft.Json.Linq.JToken" />.
	/// </summary>
	/// <typeparam name="T">The type of the annotations to retrieve.</typeparam>
	/// <returns>An <see cref="T:System.Collections.Generic.IEnumerable`1" /> that contains the annotations for this <see cref="T:Newtonsoft.Json.Linq.JToken" />.</returns>
	public IEnumerable<T> Annotations<T>() where T : class
	{
		if (this._annotations == null)
		{
			yield break;
		}
		object annotations = this._annotations;
		object[] annotations2 = annotations as object[];
		if (annotations2 != null)
		{
			foreach (object obj in annotations2)
			{
				if (obj != null)
				{
					if (obj is T val)
					{
						yield return val;
					}
					continue;
				}
				break;
			}
		}
		else if (this._annotations is T val2)
		{
			yield return val2;
		}
	}

	/// <summary>
	/// Gets a collection of annotations of the specified type for this <see cref="T:Newtonsoft.Json.Linq.JToken" />.
	/// </summary>
	/// <param name="type">The <see cref="P:Newtonsoft.Json.Linq.JToken.Type" /> of the annotations to retrieve.</param>
	/// <returns>An <see cref="T:System.Collections.Generic.IEnumerable`1" /> of <see cref="T:System.Object" /> that contains the annotations that match the specified type for this <see cref="T:Newtonsoft.Json.Linq.JToken" />.</returns>
	public IEnumerable<object> Annotations(Type type)
	{
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		if (this._annotations == null)
		{
			yield break;
		}
		object annotations = this._annotations;
		object[] annotations2 = annotations as object[];
		if (annotations2 != null)
		{
			foreach (object obj in annotations2)
			{
				if (obj != null)
				{
					if (type.IsInstanceOfType(obj))
					{
						yield return obj;
					}
					continue;
				}
				break;
			}
		}
		else if (type.IsInstanceOfType(this._annotations))
		{
			yield return this._annotations;
		}
	}

	/// <summary>
	/// Removes the annotations of the specified type from this <see cref="T:Newtonsoft.Json.Linq.JToken" />.
	/// </summary>
	/// <typeparam name="T">The type of annotations to remove.</typeparam>
	public void RemoveAnnotations<T>() where T : class
	{
		if (this._annotations == null)
		{
			return;
		}
		if (!(this._annotations is object[] array))
		{
			if (this._annotations is T)
			{
				this._annotations = null;
			}
			return;
		}
		int i = 0;
		int num = 0;
		for (; i < array.Length; i++)
		{
			object obj = array[i];
			if (obj == null)
			{
				break;
			}
			if (!(obj is T))
			{
				array[num++] = obj;
			}
		}
		if (num != 0)
		{
			while (num < i)
			{
				array[num++] = null;
			}
		}
		else
		{
			this._annotations = null;
		}
	}

	/// <summary>
	/// Removes the annotations of the specified type from this <see cref="T:Newtonsoft.Json.Linq.JToken" />.
	/// </summary>
	/// <param name="type">The <see cref="P:Newtonsoft.Json.Linq.JToken.Type" /> of annotations to remove.</param>
	public void RemoveAnnotations(Type type)
	{
		if (type == null)
		{
			throw new ArgumentNullException("type");
		}
		if (this._annotations == null)
		{
			return;
		}
		if (!(this._annotations is object[] array))
		{
			if (type.IsInstanceOfType(this._annotations))
			{
				this._annotations = null;
			}
			return;
		}
		int i = 0;
		int num = 0;
		for (; i < array.Length; i++)
		{
			object obj = array[i];
			if (obj == null)
			{
				break;
			}
			if (!type.IsInstanceOfType(obj))
			{
				array[num++] = obj;
			}
		}
		if (num != 0)
		{
			while (num < i)
			{
				array[num++] = null;
			}
		}
		else
		{
			this._annotations = null;
		}
	}

	internal void CopyAnnotations(JToken target, JToken source)
	{
		if (source._annotations is object[] source2)
		{
			target._annotations = source2.ToArray();
		}
		else
		{
			target._annotations = source._annotations;
		}
	}
}
