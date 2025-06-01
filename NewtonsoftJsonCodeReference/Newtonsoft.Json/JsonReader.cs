using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json;

/// <summary>
/// Represents a reader that provides fast, non-cached, forward-only access to serialized JSON data.
/// </summary>
public abstract class JsonReader : IAsyncDisposable, IDisposable
{
	/// <summary>
	/// Specifies the state of the reader.
	/// </summary>
	protected internal enum State
	{
		/// <summary>
		/// A <see cref="T:Newtonsoft.Json.JsonReader" /> read method has not been called.
		/// </summary>
		Start,
		/// <summary>
		/// The end of the file has been reached successfully.
		/// </summary>
		Complete,
		/// <summary>
		/// Reader is at a property.
		/// </summary>
		Property,
		/// <summary>
		/// Reader is at the start of an object.
		/// </summary>
		ObjectStart,
		/// <summary>
		/// Reader is in an object.
		/// </summary>
		Object,
		/// <summary>
		/// Reader is at the start of an array.
		/// </summary>
		ArrayStart,
		/// <summary>
		/// Reader is in an array.
		/// </summary>
		Array,
		/// <summary>
		/// The <see cref="M:Newtonsoft.Json.JsonReader.Close" /> method has been called.
		/// </summary>
		Closed,
		/// <summary>
		/// Reader has just read a value.
		/// </summary>
		PostValue,
		/// <summary>
		/// Reader is at the start of a constructor.
		/// </summary>
		ConstructorStart,
		/// <summary>
		/// Reader is in a constructor.
		/// </summary>
		Constructor,
		/// <summary>
		/// An error occurred that prevents the read operation from continuing.
		/// </summary>
		Error,
		/// <summary>
		/// The end of the file has been reached successfully.
		/// </summary>
		Finished
	}

	private JsonToken _tokenType;

	private object? _value;

	internal char _quoteChar;

	internal State _currentState;

	private JsonPosition _currentPosition;

	private CultureInfo? _culture;

	private DateTimeZoneHandling _dateTimeZoneHandling;

	private int? _maxDepth;

	private bool _hasExceededMaxDepth;

	internal DateParseHandling _dateParseHandling;

	internal FloatParseHandling _floatParseHandling;

	private string? _dateFormatString;

	private List<JsonPosition>? _stack;

	/// <summary>
	/// Gets the current reader state.
	/// </summary>
	/// <value>The current reader state.</value>
	protected State CurrentState => this._currentState;

	/// <summary>
	/// Gets or sets a value indicating whether the source should be closed when this reader is closed.
	/// </summary>
	/// <value>
	/// <c>true</c> to close the source when this reader is closed; otherwise <c>false</c>. The default is <c>true</c>.
	/// </value>
	public bool CloseInput { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether multiple pieces of JSON content can
	/// be read from a continuous stream without erroring.
	/// </summary>
	/// <value>
	/// <c>true</c> to support reading multiple pieces of JSON content; otherwise <c>false</c>.
	/// The default is <c>false</c>.
	/// </value>
	public bool SupportMultipleContent { get; set; }

	/// <summary>
	/// Gets the quotation mark character used to enclose the value of a string.
	/// </summary>
	public virtual char QuoteChar
	{
		get
		{
			return this._quoteChar;
		}
		protected internal set
		{
			this._quoteChar = value;
		}
	}

	/// <summary>
	/// Gets or sets how <see cref="T:System.DateTime" /> time zones are handled when reading JSON.
	/// </summary>
	public DateTimeZoneHandling DateTimeZoneHandling
	{
		get
		{
			return this._dateTimeZoneHandling;
		}
		set
		{
			if (value < DateTimeZoneHandling.Local || value > DateTimeZoneHandling.RoundtripKind)
			{
				throw new ArgumentOutOfRangeException("value");
			}
			this._dateTimeZoneHandling = value;
		}
	}

	/// <summary>
	/// Gets or sets how date formatted strings, e.g. "\/Date(1198908717056)\/" and "2012-03-21T05:40Z", are parsed when reading JSON.
	/// </summary>
	public DateParseHandling DateParseHandling
	{
		get
		{
			return this._dateParseHandling;
		}
		set
		{
			if (value < DateParseHandling.None || value > DateParseHandling.DateTimeOffset)
			{
				throw new ArgumentOutOfRangeException("value");
			}
			this._dateParseHandling = value;
		}
	}

	/// <summary>
	/// Gets or sets how floating point numbers, e.g. 1.0 and 9.9, are parsed when reading JSON text.
	/// </summary>
	public FloatParseHandling FloatParseHandling
	{
		get
		{
			return this._floatParseHandling;
		}
		set
		{
			if (value < FloatParseHandling.Double || value > FloatParseHandling.Decimal)
			{
				throw new ArgumentOutOfRangeException("value");
			}
			this._floatParseHandling = value;
		}
	}

	/// <summary>
	/// Gets or sets how custom date formatted strings are parsed when reading JSON.
	/// </summary>
	public string? DateFormatString
	{
		get
		{
			return this._dateFormatString;
		}
		set
		{
			this._dateFormatString = value;
		}
	}

	/// <summary>
	/// Gets or sets the maximum depth allowed when reading JSON. Reading past this depth will throw a <see cref="T:Newtonsoft.Json.JsonReaderException" />.
	/// A null value means there is no maximum. 
	/// The default value is <c>64</c>.
	/// </summary>
	public int? MaxDepth
	{
		get
		{
			return this._maxDepth;
		}
		set
		{
			if (value <= 0)
			{
				throw new ArgumentException("Value must be positive.", "value");
			}
			this._maxDepth = value;
		}
	}

	/// <summary>
	/// Gets the type of the current JSON token. 
	/// </summary>
	public virtual JsonToken TokenType => this._tokenType;

	/// <summary>
	/// Gets the text value of the current JSON token.
	/// </summary>
	public virtual object? Value => this._value;

	/// <summary>
	/// Gets the .NET type for the current JSON token.
	/// </summary>
	public virtual Type? ValueType => this._value?.GetType();

	/// <summary>
	/// Gets the depth of the current token in the JSON document.
	/// </summary>
	/// <value>The depth of the current token in the JSON document.</value>
	public virtual int Depth
	{
		get
		{
			int num = this._stack?.Count ?? 0;
			if (JsonTokenUtils.IsStartToken(this.TokenType) || this._currentPosition.Type == JsonContainerType.None)
			{
				return num;
			}
			return num + 1;
		}
	}

	/// <summary>
	/// Gets the path of the current JSON token. 
	/// </summary>
	public virtual string Path
	{
		get
		{
			if (this._currentPosition.Type == JsonContainerType.None)
			{
				return string.Empty;
			}
			JsonPosition? currentPosition = ((this._currentState != State.ArrayStart && this._currentState != State.ConstructorStart && this._currentState != State.ObjectStart) ? new JsonPosition?(this._currentPosition) : ((JsonPosition?)null));
			return JsonPosition.BuildPath(this._stack, currentPosition);
		}
	}

	/// <summary>
	/// Gets or sets the culture used when reading JSON. Defaults to <see cref="P:System.Globalization.CultureInfo.InvariantCulture" />.
	/// </summary>
	public CultureInfo Culture
	{
		get
		{
			return this._culture ?? CultureInfo.InvariantCulture;
		}
		set
		{
			this._culture = value;
		}
	}

	ValueTask IAsyncDisposable.DisposeAsync()
	{
		try
		{
			this.Dispose(disposing: true);
			return default(ValueTask);
		}
		catch (Exception exception)
		{
			return ValueTask.FromException(exception);
		}
	}

	/// <summary>
	/// Asynchronously reads the next JSON token from the source.
	/// </summary>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task`1" /> that represents the asynchronous read. The <see cref="P:System.Threading.Tasks.Task`1.Result" />
	/// property returns <c>true</c> if the next token was read successfully; <c>false</c> if there are no more tokens to read.</returns>
	/// <remarks>The default behaviour is to execute synchronously, returning an already-completed task. Derived
	/// classes can override this behaviour for true asynchronicity.</remarks>
	public virtual Task<bool> ReadAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		return cancellationToken.CancelIfRequestedAsync<bool>() ?? this.Read().ToAsync();
	}

	/// <summary>
	/// Asynchronously skips the children of the current token.
	/// </summary>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>The default behaviour is to execute synchronously, returning an already-completed task. Derived
	/// classes can override this behaviour for true asynchronicity.</remarks>
	public async Task SkipAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		if (this.TokenType == JsonToken.PropertyName)
		{
			await this.ReadAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		if (JsonTokenUtils.IsStartToken(this.TokenType))
		{
			int depth = this.Depth;
			while (await this.ReadAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false) && depth < this.Depth)
			{
			}
		}
	}

	internal async Task ReaderReadAndAssertAsync(CancellationToken cancellationToken)
	{
		if (!(await this.ReadAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false)))
		{
			throw this.CreateUnexpectedEndException();
		}
	}

	/// <summary>
	/// Asynchronously reads the next JSON token from the source as a <see cref="T:System.Nullable`1" /> of <see cref="T:System.Boolean" />.
	/// </summary>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task`1" /> that represents the asynchronous read. The <see cref="P:System.Threading.Tasks.Task`1.Result" />
	/// property returns the <see cref="T:System.Nullable`1" /> of <see cref="T:System.Boolean" />. This result will be <c>null</c> at the end of an array.</returns>
	/// <remarks>The default behaviour is to execute synchronously, returning an already-completed task. Derived
	/// classes can override this behaviour for true asynchronicity.</remarks>
	public virtual Task<bool?> ReadAsBooleanAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		return cancellationToken.CancelIfRequestedAsync<bool?>() ?? Task.FromResult(this.ReadAsBoolean());
	}

	/// <summary>
	/// Asynchronously reads the next JSON token from the source as a <see cref="T:System.Byte" />[].
	/// </summary>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task`1" /> that represents the asynchronous read. The <see cref="P:System.Threading.Tasks.Task`1.Result" />
	/// property returns the <see cref="T:System.Byte" />[]. This result will be <c>null</c> at the end of an array.</returns>
	/// <remarks>The default behaviour is to execute synchronously, returning an already-completed task. Derived
	/// classes can override this behaviour for true asynchronicity.</remarks>
	public virtual Task<byte[]?> ReadAsBytesAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		return cancellationToken.CancelIfRequestedAsync<byte[]>() ?? Task.FromResult(this.ReadAsBytes());
	}

	internal async Task<byte[]?> ReadArrayIntoByteArrayAsync(CancellationToken cancellationToken)
	{
		List<byte> buffer = new List<byte>();
		do
		{
			if (!(await this.ReadAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false)))
			{
				this.SetToken(JsonToken.None);
			}
		}
		while (!this.ReadArrayElementIntoByteArrayReportDone(buffer));
		byte[] array = buffer.ToArray();
		this.SetToken(JsonToken.Bytes, array, updateIndex: false);
		return array;
	}

	/// <summary>
	/// Asynchronously reads the next JSON token from the source as a <see cref="T:System.Nullable`1" /> of <see cref="T:System.DateTime" />.
	/// </summary>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task`1" /> that represents the asynchronous read. The <see cref="P:System.Threading.Tasks.Task`1.Result" />
	/// property returns the <see cref="T:System.Nullable`1" /> of <see cref="T:System.DateTime" />. This result will be <c>null</c> at the end of an array.</returns>
	/// <remarks>The default behaviour is to execute synchronously, returning an already-completed task. Derived
	/// classes can override this behaviour for true asynchronicity.</remarks>
	public virtual Task<DateTime?> ReadAsDateTimeAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		return cancellationToken.CancelIfRequestedAsync<DateTime?>() ?? Task.FromResult(this.ReadAsDateTime());
	}

	/// <summary>
	/// Asynchronously reads the next JSON token from the source as a <see cref="T:System.Nullable`1" /> of <see cref="T:System.DateTimeOffset" />.
	/// </summary>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task`1" /> that represents the asynchronous read. The <see cref="P:System.Threading.Tasks.Task`1.Result" />
	/// property returns the <see cref="T:System.Nullable`1" /> of <see cref="T:System.DateTimeOffset" />. This result will be <c>null</c> at the end of an array.</returns>
	/// <remarks>The default behaviour is to execute synchronously, returning an already-completed task. Derived
	/// classes can override this behaviour for true asynchronicity.</remarks>
	public virtual Task<DateTimeOffset?> ReadAsDateTimeOffsetAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		return cancellationToken.CancelIfRequestedAsync<DateTimeOffset?>() ?? Task.FromResult(this.ReadAsDateTimeOffset());
	}

	/// <summary>
	/// Asynchronously reads the next JSON token from the source as a <see cref="T:System.Nullable`1" /> of <see cref="T:System.Decimal" />.
	/// </summary>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task`1" /> that represents the asynchronous read. The <see cref="P:System.Threading.Tasks.Task`1.Result" />
	/// property returns the <see cref="T:System.Nullable`1" /> of <see cref="T:System.Decimal" />. This result will be <c>null</c> at the end of an array.</returns>
	/// <remarks>The default behaviour is to execute synchronously, returning an already-completed task. Derived
	/// classes can override this behaviour for true asynchronicity.</remarks>
	public virtual Task<decimal?> ReadAsDecimalAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		return cancellationToken.CancelIfRequestedAsync<decimal?>() ?? Task.FromResult(this.ReadAsDecimal());
	}

	/// <summary>
	/// Asynchronously reads the next JSON token from the source as a <see cref="T:System.Nullable`1" /> of <see cref="T:System.Double" />.
	/// </summary>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task`1" /> that represents the asynchronous read. The <see cref="P:System.Threading.Tasks.Task`1.Result" />
	/// property returns the <see cref="T:System.Nullable`1" /> of <see cref="T:System.Double" />. This result will be <c>null</c> at the end of an array.</returns>
	/// <remarks>The default behaviour is to execute synchronously, returning an already-completed task. Derived
	/// classes can override this behaviour for true asynchronicity.</remarks>
	public virtual Task<double?> ReadAsDoubleAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		return Task.FromResult(this.ReadAsDouble());
	}

	/// <summary>
	/// Asynchronously reads the next JSON token from the source as a <see cref="T:System.Nullable`1" /> of <see cref="T:System.Int32" />.
	/// </summary>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task`1" /> that represents the asynchronous read. The <see cref="P:System.Threading.Tasks.Task`1.Result" />
	/// property returns the <see cref="T:System.Nullable`1" /> of <see cref="T:System.Int32" />. This result will be <c>null</c> at the end of an array.</returns>
	/// <remarks>The default behaviour is to execute synchronously, returning an already-completed task. Derived
	/// classes can override this behaviour for true asynchronicity.</remarks>
	public virtual Task<int?> ReadAsInt32Async(CancellationToken cancellationToken = default(CancellationToken))
	{
		return cancellationToken.CancelIfRequestedAsync<int?>() ?? Task.FromResult(this.ReadAsInt32());
	}

	/// <summary>
	/// Asynchronously reads the next JSON token from the source as a <see cref="T:System.String" />.
	/// </summary>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task`1" /> that represents the asynchronous read. The <see cref="P:System.Threading.Tasks.Task`1.Result" />
	/// property returns the <see cref="T:System.String" />. This result will be <c>null</c> at the end of an array.</returns>
	/// <remarks>The default behaviour is to execute synchronously, returning an already-completed task. Derived
	/// classes can override this behaviour for true asynchronicity.</remarks>
	public virtual Task<string?> ReadAsStringAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		return cancellationToken.CancelIfRequestedAsync<string>() ?? Task.FromResult(this.ReadAsString());
	}

	internal async Task<bool> ReadAndMoveToContentAsync(CancellationToken cancellationToken)
	{
		bool flag = await this.ReadAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		if (flag)
		{
			flag = await this.MoveToContentAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		return flag;
	}

	internal Task<bool> MoveToContentAsync(CancellationToken cancellationToken)
	{
		JsonToken tokenType = this.TokenType;
		if (tokenType == JsonToken.None || tokenType == JsonToken.Comment)
		{
			return this.MoveToContentFromNonContentAsync(cancellationToken);
		}
		return AsyncUtils.True;
	}

	private async Task<bool> MoveToContentFromNonContentAsync(CancellationToken cancellationToken)
	{
		JsonToken tokenType;
		do
		{
			if (!(await this.ReadAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false)))
			{
				return false;
			}
			tokenType = this.TokenType;
		}
		while (tokenType == JsonToken.None || tokenType == JsonToken.Comment);
		return true;
	}

	internal JsonPosition GetPosition(int depth)
	{
		if (this._stack != null && depth < this._stack.Count)
		{
			return this._stack[depth];
		}
		return this._currentPosition;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:Newtonsoft.Json.JsonReader" /> class.
	/// </summary>
	protected JsonReader()
	{
		this._currentState = State.Start;
		this._dateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind;
		this._dateParseHandling = DateParseHandling.DateTime;
		this._floatParseHandling = FloatParseHandling.Double;
		this._maxDepth = 64;
		this.CloseInput = true;
	}

	private void Push(JsonContainerType value)
	{
		this.UpdateScopeWithFinishedValue();
		if (this._currentPosition.Type == JsonContainerType.None)
		{
			this._currentPosition = new JsonPosition(value);
			return;
		}
		if (this._stack == null)
		{
			this._stack = new List<JsonPosition>();
		}
		this._stack.Add(this._currentPosition);
		this._currentPosition = new JsonPosition(value);
		if (!this._maxDepth.HasValue || !(this.Depth + 1 > this._maxDepth) || this._hasExceededMaxDepth)
		{
			return;
		}
		this._hasExceededMaxDepth = true;
		throw JsonReaderException.Create(this, "The reader's MaxDepth of {0} has been exceeded.".FormatWith(CultureInfo.InvariantCulture, this._maxDepth));
	}

	private JsonContainerType Pop()
	{
		JsonPosition currentPosition;
		if (this._stack != null && this._stack.Count > 0)
		{
			currentPosition = this._currentPosition;
			this._currentPosition = this._stack[this._stack.Count - 1];
			this._stack.RemoveAt(this._stack.Count - 1);
		}
		else
		{
			currentPosition = this._currentPosition;
			this._currentPosition = default(JsonPosition);
		}
		if (this._maxDepth.HasValue && this.Depth <= this._maxDepth)
		{
			this._hasExceededMaxDepth = false;
		}
		return currentPosition.Type;
	}

	private JsonContainerType Peek()
	{
		return this._currentPosition.Type;
	}

	/// <summary>
	/// Reads the next JSON token from the source.
	/// </summary>
	/// <returns><c>true</c> if the next token was read successfully; <c>false</c> if there are no more tokens to read.</returns>
	public abstract bool Read();

	/// <summary>
	/// Reads the next JSON token from the source as a <see cref="T:System.Nullable`1" /> of <see cref="T:System.Int32" />.
	/// </summary>
	/// <returns>A <see cref="T:System.Nullable`1" /> of <see cref="T:System.Int32" />. This method will return <c>null</c> at the end of an array.</returns>
	public virtual int? ReadAsInt32()
	{
		JsonToken contentToken = this.GetContentToken();
		switch (contentToken)
		{
		case JsonToken.None:
		case JsonToken.Null:
		case JsonToken.EndArray:
			return null;
		case JsonToken.Integer:
		case JsonToken.Float:
		{
			object value = this.Value;
			if (value is int)
			{
				return (int)value;
			}
			int num;
			if (value is BigInteger bigInteger)
			{
				num = (int)bigInteger;
			}
			else
			{
				try
				{
					num = Convert.ToInt32(value, CultureInfo.InvariantCulture);
				}
				catch (Exception ex)
				{
					throw JsonReaderException.Create(this, "Could not convert to integer: {0}.".FormatWith(CultureInfo.InvariantCulture, value), ex);
				}
			}
			this.SetToken(JsonToken.Integer, num, updateIndex: false);
			return num;
		}
		case JsonToken.String:
		{
			string s = (string)this.Value;
			return this.ReadInt32String(s);
		}
		default:
			throw JsonReaderException.Create(this, "Error reading integer. Unexpected token: {0}.".FormatWith(CultureInfo.InvariantCulture, contentToken));
		}
	}

	internal int? ReadInt32String(string? s)
	{
		if (StringUtils.IsNullOrEmpty(s))
		{
			this.SetToken(JsonToken.Null, null, updateIndex: false);
			return null;
		}
		if (int.TryParse(s, NumberStyles.Integer, this.Culture, out var result))
		{
			this.SetToken(JsonToken.Integer, result, updateIndex: false);
			return result;
		}
		this.SetToken(JsonToken.String, s, updateIndex: false);
		throw JsonReaderException.Create(this, "Could not convert string to integer: {0}.".FormatWith(CultureInfo.InvariantCulture, s));
	}

	/// <summary>
	/// Reads the next JSON token from the source as a <see cref="T:System.String" />.
	/// </summary>
	/// <returns>A <see cref="T:System.String" />. This method will return <c>null</c> at the end of an array.</returns>
	public virtual string? ReadAsString()
	{
		JsonToken contentToken = this.GetContentToken();
		switch (contentToken)
		{
		case JsonToken.None:
		case JsonToken.Null:
		case JsonToken.EndArray:
			return null;
		case JsonToken.String:
			return (string)this.Value;
		default:
			if (JsonTokenUtils.IsPrimitiveToken(contentToken))
			{
				object value = this.Value;
				if (value != null)
				{
					string text = ((!(value is IFormattable formattable)) ? ((value is Uri uri) ? uri.OriginalString : value.ToString()) : formattable.ToString(null, this.Culture));
					this.SetToken(JsonToken.String, text, updateIndex: false);
					return text;
				}
			}
			throw JsonReaderException.Create(this, "Error reading string. Unexpected token: {0}.".FormatWith(CultureInfo.InvariantCulture, contentToken));
		}
	}

	/// <summary>
	/// Reads the next JSON token from the source as a <see cref="T:System.Byte" />[].
	/// </summary>
	/// <returns>A <see cref="T:System.Byte" />[] or <c>null</c> if the next JSON token is null. This method will return <c>null</c> at the end of an array.</returns>
	public virtual byte[]? ReadAsBytes()
	{
		JsonToken contentToken = this.GetContentToken();
		switch (contentToken)
		{
		case JsonToken.StartObject:
		{
			this.ReadIntoWrappedTypeObject();
			byte[] array2 = this.ReadAsBytes();
			this.ReaderReadAndAssert();
			if (this.TokenType != JsonToken.EndObject)
			{
				throw JsonReaderException.Create(this, "Error reading bytes. Unexpected token: {0}.".FormatWith(CultureInfo.InvariantCulture, this.TokenType));
			}
			this.SetToken(JsonToken.Bytes, array2, updateIndex: false);
			return array2;
		}
		case JsonToken.String:
		{
			string text = (string)this.Value;
			Guid g;
			byte[] array3 = ((text.Length == 0) ? CollectionUtils.ArrayEmpty<byte>() : ((!ConvertUtils.TryConvertGuid(text, out g)) ? Convert.FromBase64String(text) : g.ToByteArray()));
			this.SetToken(JsonToken.Bytes, array3, updateIndex: false);
			return array3;
		}
		case JsonToken.None:
		case JsonToken.Null:
		case JsonToken.EndArray:
			return null;
		case JsonToken.Bytes:
			if (this.Value is Guid guid)
			{
				byte[] array = guid.ToByteArray();
				this.SetToken(JsonToken.Bytes, array, updateIndex: false);
				return array;
			}
			return (byte[])this.Value;
		case JsonToken.StartArray:
			return this.ReadArrayIntoByteArray();
		default:
			throw JsonReaderException.Create(this, "Error reading bytes. Unexpected token: {0}.".FormatWith(CultureInfo.InvariantCulture, contentToken));
		}
	}

	internal byte[] ReadArrayIntoByteArray()
	{
		List<byte> list = new List<byte>();
		do
		{
			if (!this.Read())
			{
				this.SetToken(JsonToken.None);
			}
		}
		while (!this.ReadArrayElementIntoByteArrayReportDone(list));
		byte[] array = list.ToArray();
		this.SetToken(JsonToken.Bytes, array, updateIndex: false);
		return array;
	}

	private bool ReadArrayElementIntoByteArrayReportDone(List<byte> buffer)
	{
		switch (this.TokenType)
		{
		case JsonToken.None:
			throw JsonReaderException.Create(this, "Unexpected end when reading bytes.");
		case JsonToken.Integer:
			buffer.Add(Convert.ToByte(this.Value, CultureInfo.InvariantCulture));
			return false;
		case JsonToken.EndArray:
			return true;
		case JsonToken.Comment:
			return false;
		default:
			throw JsonReaderException.Create(this, "Unexpected token when reading bytes: {0}.".FormatWith(CultureInfo.InvariantCulture, this.TokenType));
		}
	}

	/// <summary>
	/// Reads the next JSON token from the source as a <see cref="T:System.Nullable`1" /> of <see cref="T:System.Double" />.
	/// </summary>
	/// <returns>A <see cref="T:System.Nullable`1" /> of <see cref="T:System.Double" />. This method will return <c>null</c> at the end of an array.</returns>
	public virtual double? ReadAsDouble()
	{
		JsonToken contentToken = this.GetContentToken();
		switch (contentToken)
		{
		case JsonToken.None:
		case JsonToken.Null:
		case JsonToken.EndArray:
			return null;
		case JsonToken.Integer:
		case JsonToken.Float:
		{
			object value = this.Value;
			if (value is double)
			{
				return (double)value;
			}
			double num = ((!(value is BigInteger bigInteger)) ? Convert.ToDouble(value, CultureInfo.InvariantCulture) : ((double)bigInteger));
			this.SetToken(JsonToken.Float, num, updateIndex: false);
			return num;
		}
		case JsonToken.String:
			return this.ReadDoubleString((string)this.Value);
		default:
			throw JsonReaderException.Create(this, "Error reading double. Unexpected token: {0}.".FormatWith(CultureInfo.InvariantCulture, contentToken));
		}
	}

	internal double? ReadDoubleString(string? s)
	{
		if (StringUtils.IsNullOrEmpty(s))
		{
			this.SetToken(JsonToken.Null, null, updateIndex: false);
			return null;
		}
		if (double.TryParse(s, NumberStyles.Float | NumberStyles.AllowThousands, this.Culture, out var result))
		{
			this.SetToken(JsonToken.Float, result, updateIndex: false);
			return result;
		}
		this.SetToken(JsonToken.String, s, updateIndex: false);
		throw JsonReaderException.Create(this, "Could not convert string to double: {0}.".FormatWith(CultureInfo.InvariantCulture, s));
	}

	/// <summary>
	/// Reads the next JSON token from the source as a <see cref="T:System.Nullable`1" /> of <see cref="T:System.Boolean" />.
	/// </summary>
	/// <returns>A <see cref="T:System.Nullable`1" /> of <see cref="T:System.Boolean" />. This method will return <c>null</c> at the end of an array.</returns>
	public virtual bool? ReadAsBoolean()
	{
		JsonToken contentToken = this.GetContentToken();
		switch (contentToken)
		{
		case JsonToken.None:
		case JsonToken.Null:
		case JsonToken.EndArray:
			return null;
		case JsonToken.Integer:
		case JsonToken.Float:
		{
			bool flag = ((!(this.Value is BigInteger bigInteger)) ? Convert.ToBoolean(this.Value, CultureInfo.InvariantCulture) : (bigInteger != 0L));
			this.SetToken(JsonToken.Boolean, flag, updateIndex: false);
			return flag;
		}
		case JsonToken.String:
			return this.ReadBooleanString((string)this.Value);
		case JsonToken.Boolean:
			return (bool)this.Value;
		default:
			throw JsonReaderException.Create(this, "Error reading boolean. Unexpected token: {0}.".FormatWith(CultureInfo.InvariantCulture, contentToken));
		}
	}

	internal bool? ReadBooleanString(string? s)
	{
		if (StringUtils.IsNullOrEmpty(s))
		{
			this.SetToken(JsonToken.Null, null, updateIndex: false);
			return null;
		}
		if (bool.TryParse(s, out var result))
		{
			this.SetToken(JsonToken.Boolean, result, updateIndex: false);
			return result;
		}
		this.SetToken(JsonToken.String, s, updateIndex: false);
		throw JsonReaderException.Create(this, "Could not convert string to boolean: {0}.".FormatWith(CultureInfo.InvariantCulture, s));
	}

	/// <summary>
	/// Reads the next JSON token from the source as a <see cref="T:System.Nullable`1" /> of <see cref="T:System.Decimal" />.
	/// </summary>
	/// <returns>A <see cref="T:System.Nullable`1" /> of <see cref="T:System.Decimal" />. This method will return <c>null</c> at the end of an array.</returns>
	public virtual decimal? ReadAsDecimal()
	{
		JsonToken contentToken = this.GetContentToken();
		switch (contentToken)
		{
		case JsonToken.None:
		case JsonToken.Null:
		case JsonToken.EndArray:
			return null;
		case JsonToken.Integer:
		case JsonToken.Float:
		{
			object value = this.Value;
			if (value is decimal)
			{
				return (decimal)value;
			}
			decimal num;
			if (value is BigInteger bigInteger)
			{
				num = (decimal)bigInteger;
			}
			else
			{
				try
				{
					num = Convert.ToDecimal(value, CultureInfo.InvariantCulture);
				}
				catch (Exception ex)
				{
					throw JsonReaderException.Create(this, "Could not convert to decimal: {0}.".FormatWith(CultureInfo.InvariantCulture, value), ex);
				}
			}
			this.SetToken(JsonToken.Float, num, updateIndex: false);
			return num;
		}
		case JsonToken.String:
			return this.ReadDecimalString((string)this.Value);
		default:
			throw JsonReaderException.Create(this, "Error reading decimal. Unexpected token: {0}.".FormatWith(CultureInfo.InvariantCulture, contentToken));
		}
	}

	internal decimal? ReadDecimalString(string? s)
	{
		if (StringUtils.IsNullOrEmpty(s))
		{
			this.SetToken(JsonToken.Null, null, updateIndex: false);
			return null;
		}
		if (decimal.TryParse(s, NumberStyles.Number, this.Culture, out var result))
		{
			this.SetToken(JsonToken.Float, result, updateIndex: false);
			return result;
		}
		if (ConvertUtils.DecimalTryParse(s.ToCharArray(), 0, s.Length, out result) == ParseResult.Success)
		{
			this.SetToken(JsonToken.Float, result, updateIndex: false);
			return result;
		}
		this.SetToken(JsonToken.String, s, updateIndex: false);
		throw JsonReaderException.Create(this, "Could not convert string to decimal: {0}.".FormatWith(CultureInfo.InvariantCulture, s));
	}

	/// <summary>
	/// Reads the next JSON token from the source as a <see cref="T:System.Nullable`1" /> of <see cref="T:System.DateTime" />.
	/// </summary>
	/// <returns>A <see cref="T:System.Nullable`1" /> of <see cref="T:System.DateTime" />. This method will return <c>null</c> at the end of an array.</returns>
	public virtual DateTime? ReadAsDateTime()
	{
		switch (this.GetContentToken())
		{
		case JsonToken.None:
		case JsonToken.Null:
		case JsonToken.EndArray:
			return null;
		case JsonToken.Date:
			if (this.Value is DateTimeOffset dateTimeOffset)
			{
				this.SetToken(JsonToken.Date, dateTimeOffset.DateTime, updateIndex: false);
			}
			return (DateTime)this.Value;
		case JsonToken.String:
			return this.ReadDateTimeString((string)this.Value);
		default:
			throw JsonReaderException.Create(this, "Error reading date. Unexpected token: {0}.".FormatWith(CultureInfo.InvariantCulture, this.TokenType));
		}
	}

	internal DateTime? ReadDateTimeString(string? s)
	{
		if (StringUtils.IsNullOrEmpty(s))
		{
			this.SetToken(JsonToken.Null, null, updateIndex: false);
			return null;
		}
		if (DateTimeUtils.TryParseDateTime(s, this.DateTimeZoneHandling, this._dateFormatString, this.Culture, out var dt))
		{
			dt = DateTimeUtils.EnsureDateTime(dt, this.DateTimeZoneHandling);
			this.SetToken(JsonToken.Date, dt, updateIndex: false);
			return dt;
		}
		if (DateTime.TryParse(s, this.Culture, DateTimeStyles.RoundtripKind, out dt))
		{
			dt = DateTimeUtils.EnsureDateTime(dt, this.DateTimeZoneHandling);
			this.SetToken(JsonToken.Date, dt, updateIndex: false);
			return dt;
		}
		throw JsonReaderException.Create(this, "Could not convert string to DateTime: {0}.".FormatWith(CultureInfo.InvariantCulture, s));
	}

	/// <summary>
	/// Reads the next JSON token from the source as a <see cref="T:System.Nullable`1" /> of <see cref="T:System.DateTimeOffset" />.
	/// </summary>
	/// <returns>A <see cref="T:System.Nullable`1" /> of <see cref="T:System.DateTimeOffset" />. This method will return <c>null</c> at the end of an array.</returns>
	public virtual DateTimeOffset? ReadAsDateTimeOffset()
	{
		JsonToken contentToken = this.GetContentToken();
		switch (contentToken)
		{
		case JsonToken.None:
		case JsonToken.Null:
		case JsonToken.EndArray:
			return null;
		case JsonToken.Date:
			if (this.Value is DateTime dateTime)
			{
				this.SetToken(JsonToken.Date, new DateTimeOffset(dateTime), updateIndex: false);
			}
			return (DateTimeOffset)this.Value;
		case JsonToken.String:
		{
			string s = (string)this.Value;
			return this.ReadDateTimeOffsetString(s);
		}
		default:
			throw JsonReaderException.Create(this, "Error reading date. Unexpected token: {0}.".FormatWith(CultureInfo.InvariantCulture, contentToken));
		}
	}

	internal DateTimeOffset? ReadDateTimeOffsetString(string? s)
	{
		if (StringUtils.IsNullOrEmpty(s))
		{
			this.SetToken(JsonToken.Null, null, updateIndex: false);
			return null;
		}
		if (DateTimeUtils.TryParseDateTimeOffset(s, this._dateFormatString, this.Culture, out var dt))
		{
			this.SetToken(JsonToken.Date, dt, updateIndex: false);
			return dt;
		}
		if (DateTimeOffset.TryParse(s, this.Culture, DateTimeStyles.RoundtripKind, out dt))
		{
			this.SetToken(JsonToken.Date, dt, updateIndex: false);
			return dt;
		}
		this.SetToken(JsonToken.String, s, updateIndex: false);
		throw JsonReaderException.Create(this, "Could not convert string to DateTimeOffset: {0}.".FormatWith(CultureInfo.InvariantCulture, s));
	}

	internal void ReaderReadAndAssert()
	{
		if (!this.Read())
		{
			throw this.CreateUnexpectedEndException();
		}
	}

	internal JsonReaderException CreateUnexpectedEndException()
	{
		return JsonReaderException.Create(this, "Unexpected end when reading JSON.");
	}

	internal void ReadIntoWrappedTypeObject()
	{
		this.ReaderReadAndAssert();
		if (this.Value != null && this.Value.ToString() == "$type")
		{
			this.ReaderReadAndAssert();
			if (this.Value != null && this.Value.ToString().StartsWith("System.Byte[]", StringComparison.Ordinal))
			{
				this.ReaderReadAndAssert();
				if (this.Value.ToString() == "$value")
				{
					return;
				}
			}
		}
		throw JsonReaderException.Create(this, "Error reading bytes. Unexpected token: {0}.".FormatWith(CultureInfo.InvariantCulture, JsonToken.StartObject));
	}

	/// <summary>
	/// Skips the children of the current token.
	/// </summary>
	public void Skip()
	{
		if (this.TokenType == JsonToken.PropertyName)
		{
			this.Read();
		}
		if (JsonTokenUtils.IsStartToken(this.TokenType))
		{
			int depth = this.Depth;
			while (this.Read() && depth < this.Depth)
			{
			}
		}
	}

	/// <summary>
	/// Sets the current token.
	/// </summary>
	/// <param name="newToken">The new token.</param>
	protected void SetToken(JsonToken newToken)
	{
		this.SetToken(newToken, null, updateIndex: true);
	}

	/// <summary>
	/// Sets the current token and value.
	/// </summary>
	/// <param name="newToken">The new token.</param>
	/// <param name="value">The value.</param>
	protected void SetToken(JsonToken newToken, object? value)
	{
		this.SetToken(newToken, value, updateIndex: true);
	}

	/// <summary>
	/// Sets the current token and value.
	/// </summary>
	/// <param name="newToken">The new token.</param>
	/// <param name="value">The value.</param>
	/// <param name="updateIndex">A flag indicating whether the position index inside an array should be updated.</param>
	protected void SetToken(JsonToken newToken, object? value, bool updateIndex)
	{
		this._tokenType = newToken;
		this._value = value;
		switch (newToken)
		{
		case JsonToken.StartObject:
			this._currentState = State.ObjectStart;
			this.Push(JsonContainerType.Object);
			break;
		case JsonToken.StartArray:
			this._currentState = State.ArrayStart;
			this.Push(JsonContainerType.Array);
			break;
		case JsonToken.StartConstructor:
			this._currentState = State.ConstructorStart;
			this.Push(JsonContainerType.Constructor);
			break;
		case JsonToken.EndObject:
			this.ValidateEnd(JsonToken.EndObject);
			break;
		case JsonToken.EndArray:
			this.ValidateEnd(JsonToken.EndArray);
			break;
		case JsonToken.EndConstructor:
			this.ValidateEnd(JsonToken.EndConstructor);
			break;
		case JsonToken.PropertyName:
			this._currentState = State.Property;
			this._currentPosition.PropertyName = (string)value;
			break;
		case JsonToken.Raw:
		case JsonToken.Integer:
		case JsonToken.Float:
		case JsonToken.String:
		case JsonToken.Boolean:
		case JsonToken.Null:
		case JsonToken.Undefined:
		case JsonToken.Date:
		case JsonToken.Bytes:
			this.SetPostValueState(updateIndex);
			break;
		case JsonToken.Comment:
			break;
		}
	}

	internal void SetPostValueState(bool updateIndex)
	{
		if (this.Peek() != JsonContainerType.None || this.SupportMultipleContent)
		{
			this._currentState = State.PostValue;
		}
		else
		{
			this.SetFinished();
		}
		if (updateIndex)
		{
			this.UpdateScopeWithFinishedValue();
		}
	}

	private void UpdateScopeWithFinishedValue()
	{
		if (this._currentPosition.HasIndex)
		{
			this._currentPosition.Position++;
		}
	}

	private void ValidateEnd(JsonToken endToken)
	{
		JsonContainerType jsonContainerType = this.Pop();
		if (this.GetTypeForCloseToken(endToken) != jsonContainerType)
		{
			throw JsonReaderException.Create(this, "JsonToken {0} is not valid for closing JsonType {1}.".FormatWith(CultureInfo.InvariantCulture, endToken, jsonContainerType));
		}
		if (this.Peek() != JsonContainerType.None || this.SupportMultipleContent)
		{
			this._currentState = State.PostValue;
		}
		else
		{
			this.SetFinished();
		}
	}

	/// <summary>
	/// Sets the state based on current token type.
	/// </summary>
	protected void SetStateBasedOnCurrent()
	{
		JsonContainerType jsonContainerType = this.Peek();
		switch (jsonContainerType)
		{
		case JsonContainerType.Object:
			this._currentState = State.Object;
			break;
		case JsonContainerType.Array:
			this._currentState = State.Array;
			break;
		case JsonContainerType.Constructor:
			this._currentState = State.Constructor;
			break;
		case JsonContainerType.None:
			this.SetFinished();
			break;
		default:
			throw JsonReaderException.Create(this, "While setting the reader state back to current object an unexpected JsonType was encountered: {0}".FormatWith(CultureInfo.InvariantCulture, jsonContainerType));
		}
	}

	private void SetFinished()
	{
		this._currentState = ((!this.SupportMultipleContent) ? State.Finished : State.Start);
	}

	private JsonContainerType GetTypeForCloseToken(JsonToken token)
	{
		return token switch
		{
			JsonToken.EndObject => JsonContainerType.Object, 
			JsonToken.EndArray => JsonContainerType.Array, 
			JsonToken.EndConstructor => JsonContainerType.Constructor, 
			_ => throw JsonReaderException.Create(this, "Not a valid close JsonToken: {0}".FormatWith(CultureInfo.InvariantCulture, token)), 
		};
	}

	void IDisposable.Dispose()
	{
		this.Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	/// <summary>
	/// Releases unmanaged and - optionally - managed resources.
	/// </summary>
	/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
	protected virtual void Dispose(bool disposing)
	{
		if (this._currentState != State.Closed && disposing)
		{
			this.Close();
		}
	}

	/// <summary>
	/// Changes the reader's state to <see cref="F:Newtonsoft.Json.JsonReader.State.Closed" />.
	/// If <see cref="P:Newtonsoft.Json.JsonReader.CloseInput" /> is set to <c>true</c>, the source is also closed.
	/// </summary>
	public virtual void Close()
	{
		this._currentState = State.Closed;
		this._tokenType = JsonToken.None;
		this._value = null;
	}

	internal void ReadAndAssert()
	{
		if (!this.Read())
		{
			throw JsonSerializationException.Create(this, "Unexpected end when reading JSON.");
		}
	}

	internal void ReadForTypeAndAssert(JsonContract? contract, bool hasConverter)
	{
		if (!this.ReadForType(contract, hasConverter))
		{
			throw JsonSerializationException.Create(this, "Unexpected end when reading JSON.");
		}
	}

	internal bool ReadForType(JsonContract? contract, bool hasConverter)
	{
		if (hasConverter)
		{
			return this.Read();
		}
		switch (contract?.InternalReadType ?? ReadType.Read)
		{
		case ReadType.Read:
			return this.ReadAndMoveToContent();
		case ReadType.ReadAsInt32:
			this.ReadAsInt32();
			break;
		case ReadType.ReadAsInt64:
		{
			bool result = this.ReadAndMoveToContent();
			if (this.TokenType == JsonToken.Undefined)
			{
				throw JsonReaderException.Create(this, "An undefined token is not a valid {0}.".FormatWith(CultureInfo.InvariantCulture, contract?.UnderlyingType ?? typeof(long)));
			}
			return result;
		}
		case ReadType.ReadAsDecimal:
			this.ReadAsDecimal();
			break;
		case ReadType.ReadAsDouble:
			this.ReadAsDouble();
			break;
		case ReadType.ReadAsBytes:
			this.ReadAsBytes();
			break;
		case ReadType.ReadAsBoolean:
			this.ReadAsBoolean();
			break;
		case ReadType.ReadAsString:
			this.ReadAsString();
			break;
		case ReadType.ReadAsDateTime:
			this.ReadAsDateTime();
			break;
		case ReadType.ReadAsDateTimeOffset:
			this.ReadAsDateTimeOffset();
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		return this.TokenType != JsonToken.None;
	}

	internal bool ReadAndMoveToContent()
	{
		if (this.Read())
		{
			return this.MoveToContent();
		}
		return false;
	}

	internal bool MoveToContent()
	{
		JsonToken tokenType = this.TokenType;
		while (tokenType == JsonToken.None || tokenType == JsonToken.Comment)
		{
			if (!this.Read())
			{
				return false;
			}
			tokenType = this.TokenType;
		}
		return true;
	}

	private JsonToken GetContentToken()
	{
		JsonToken tokenType;
		do
		{
			if (!this.Read())
			{
				this.SetToken(JsonToken.None);
				return JsonToken.None;
			}
			tokenType = this.TokenType;
		}
		while (tokenType == JsonToken.Comment);
		return tokenType;
	}
}
