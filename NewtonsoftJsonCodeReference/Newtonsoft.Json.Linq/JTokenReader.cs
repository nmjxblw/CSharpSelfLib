using System;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Linq;

/// <summary>
/// Represents a reader that provides fast, non-cached, forward-only access to serialized JSON data.
/// </summary>
public class JTokenReader : JsonReader, IJsonLineInfo
{
	private readonly JToken _root;

	private string? _initialPath;

	private JToken? _parent;

	private JToken? _current;

	/// <summary>
	/// Gets the <see cref="T:Newtonsoft.Json.Linq.JToken" /> at the reader's current position.
	/// </summary>
	public JToken? CurrentToken => this._current;

	int IJsonLineInfo.LineNumber
	{
		get
		{
			if (base.CurrentState == State.Start)
			{
				return 0;
			}
			return ((IJsonLineInfo)this._current)?.LineNumber ?? 0;
		}
	}

	int IJsonLineInfo.LinePosition
	{
		get
		{
			if (base.CurrentState == State.Start)
			{
				return 0;
			}
			return ((IJsonLineInfo)this._current)?.LinePosition ?? 0;
		}
	}

	/// <summary>
	/// Gets the path of the current JSON token. 
	/// </summary>
	public override string Path
	{
		get
		{
			string text = base.Path;
			if (this._initialPath == null)
			{
				this._initialPath = this._root.Path;
			}
			if (!StringUtils.IsNullOrEmpty(this._initialPath))
			{
				if (StringUtils.IsNullOrEmpty(text))
				{
					return this._initialPath;
				}
				text = ((!text.StartsWith('[')) ? (this._initialPath + "." + text) : (this._initialPath + text));
			}
			return text;
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:Newtonsoft.Json.Linq.JTokenReader" /> class.
	/// </summary>
	/// <param name="token">The token to read from.</param>
	public JTokenReader(JToken token)
	{
		ValidationUtils.ArgumentNotNull(token, "token");
		this._root = token;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:Newtonsoft.Json.Linq.JTokenReader" /> class.
	/// </summary>
	/// <param name="token">The token to read from.</param>
	/// <param name="initialPath">The initial path of the token. It is prepended to the returned <see cref="P:Newtonsoft.Json.Linq.JTokenReader.Path" />.</param>
	public JTokenReader(JToken token, string initialPath)
		: this(token)
	{
		this._initialPath = initialPath;
	}

	/// <summary>
	/// Reads the next JSON token from the underlying <see cref="T:Newtonsoft.Json.Linq.JToken" />.
	/// </summary>
	/// <returns>
	/// <c>true</c> if the next token was read successfully; <c>false</c> if there are no more tokens to read.
	/// </returns>
	public override bool Read()
	{
		if (base.CurrentState != State.Start)
		{
			if (this._current == null)
			{
				return false;
			}
			if (this._current is JContainer jContainer && this._parent != jContainer)
			{
				return this.ReadInto(jContainer);
			}
			return this.ReadOver(this._current);
		}
		if (this._current == this._root)
		{
			return false;
		}
		this._current = this._root;
		this.SetToken(this._current);
		return true;
	}

	private bool ReadOver(JToken t)
	{
		if (t == this._root)
		{
			return this.ReadToEnd();
		}
		JToken next = t.Next;
		if (next == null || next == t || t == t.Parent.Last)
		{
			if (t.Parent == null)
			{
				return this.ReadToEnd();
			}
			return this.SetEnd(t.Parent);
		}
		this._current = next;
		this.SetToken(this._current);
		return true;
	}

	private bool ReadToEnd()
	{
		this._current = null;
		base.SetToken(JsonToken.None);
		return false;
	}

	private JsonToken? GetEndToken(JContainer c)
	{
		return c.Type switch
		{
			JTokenType.Object => JsonToken.EndObject, 
			JTokenType.Array => JsonToken.EndArray, 
			JTokenType.Constructor => JsonToken.EndConstructor, 
			JTokenType.Property => null, 
			_ => throw MiscellaneousUtils.CreateArgumentOutOfRangeException("Type", c.Type, "Unexpected JContainer type."), 
		};
	}

	private bool ReadInto(JContainer c)
	{
		JToken first = c.First;
		if (first == null)
		{
			return this.SetEnd(c);
		}
		this.SetToken(first);
		this._current = first;
		this._parent = c;
		return true;
	}

	private bool SetEnd(JContainer c)
	{
		JsonToken? endToken = this.GetEndToken(c);
		if (endToken.HasValue)
		{
			base.SetToken(endToken.GetValueOrDefault());
			this._current = c;
			this._parent = c;
			return true;
		}
		return this.ReadOver(c);
	}

	private void SetToken(JToken token)
	{
		switch (token.Type)
		{
		case JTokenType.Object:
			base.SetToken(JsonToken.StartObject);
			break;
		case JTokenType.Array:
			base.SetToken(JsonToken.StartArray);
			break;
		case JTokenType.Constructor:
			base.SetToken(JsonToken.StartConstructor, ((JConstructor)token).Name);
			break;
		case JTokenType.Property:
			base.SetToken(JsonToken.PropertyName, ((JProperty)token).Name);
			break;
		case JTokenType.Comment:
			base.SetToken(JsonToken.Comment, ((JValue)token).Value);
			break;
		case JTokenType.Integer:
			base.SetToken(JsonToken.Integer, ((JValue)token).Value);
			break;
		case JTokenType.Float:
			base.SetToken(JsonToken.Float, ((JValue)token).Value);
			break;
		case JTokenType.String:
			base.SetToken(JsonToken.String, ((JValue)token).Value);
			break;
		case JTokenType.Boolean:
			base.SetToken(JsonToken.Boolean, ((JValue)token).Value);
			break;
		case JTokenType.Null:
			base.SetToken(JsonToken.Null, ((JValue)token).Value);
			break;
		case JTokenType.Undefined:
			base.SetToken(JsonToken.Undefined, ((JValue)token).Value);
			break;
		case JTokenType.Date:
		{
			object obj = ((JValue)token).Value;
			if (obj is DateTime value2)
			{
				obj = DateTimeUtils.EnsureDateTime(value2, base.DateTimeZoneHandling);
			}
			base.SetToken(JsonToken.Date, obj);
			break;
		}
		case JTokenType.Raw:
			base.SetToken(JsonToken.Raw, ((JValue)token).Value);
			break;
		case JTokenType.Bytes:
			base.SetToken(JsonToken.Bytes, ((JValue)token).Value);
			break;
		case JTokenType.Guid:
			base.SetToken(JsonToken.String, this.SafeToString(((JValue)token).Value));
			break;
		case JTokenType.Uri:
		{
			object value = ((JValue)token).Value;
			base.SetToken(JsonToken.String, (value is Uri uri) ? uri.OriginalString : this.SafeToString(value));
			break;
		}
		case JTokenType.TimeSpan:
			base.SetToken(JsonToken.String, this.SafeToString(((JValue)token).Value));
			break;
		default:
			throw MiscellaneousUtils.CreateArgumentOutOfRangeException("Type", token.Type, "Unexpected JTokenType.");
		}
	}

	private string? SafeToString(object? value)
	{
		return value?.ToString();
	}

	bool IJsonLineInfo.HasLineInfo()
	{
		if (base.CurrentState == State.Start)
		{
			return false;
		}
		return ((IJsonLineInfo)this._current)?.HasLineInfo() ?? false;
	}
}
