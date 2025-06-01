using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json;

/// <summary>
/// Represents a writer that provides a fast, non-cached, forward-only way of generating JSON data.
/// </summary>
public abstract class JsonWriter : IAsyncDisposable, IDisposable
{
	internal enum State
	{
		Start,
		Property,
		ObjectStart,
		Object,
		ArrayStart,
		Array,
		ConstructorStart,
		Constructor,
		Closed,
		Error
	}

	private static readonly State[][] StateArray;

	internal static readonly State[][] StateArrayTemplate;

	private List<JsonPosition>? _stack;

	private JsonPosition _currentPosition;

	private State _currentState;

	private Formatting _formatting;

	private DateFormatHandling _dateFormatHandling;

	private DateTimeZoneHandling _dateTimeZoneHandling;

	private StringEscapeHandling _stringEscapeHandling;

	private FloatFormatHandling _floatFormatHandling;

	private string? _dateFormatString;

	private CultureInfo? _culture;

	/// <summary>
	/// Gets or sets a value indicating whether the destination should be closed when this writer is closed.
	/// </summary>
	/// <value>
	/// <c>true</c> to close the destination when this writer is closed; otherwise <c>false</c>. The default is <c>true</c>.
	/// </value>
	public bool CloseOutput { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether the JSON should be auto-completed when this writer is closed.
	/// </summary>
	/// <value>
	/// <c>true</c> to auto-complete the JSON when this writer is closed; otherwise <c>false</c>. The default is <c>true</c>.
	/// </value>
	public bool AutoCompleteOnClose { get; set; }

	/// <summary>
	/// Gets the top.
	/// </summary>
	/// <value>The top.</value>
	protected internal int Top
	{
		get
		{
			int num = this._stack?.Count ?? 0;
			if (this.Peek() != JsonContainerType.None)
			{
				num++;
			}
			return num;
		}
	}

	/// <summary>
	/// Gets the state of the writer.
	/// </summary>
	public WriteState WriteState
	{
		get
		{
			switch (this._currentState)
			{
			case State.Error:
				return WriteState.Error;
			case State.Closed:
				return WriteState.Closed;
			case State.ObjectStart:
			case State.Object:
				return WriteState.Object;
			case State.ArrayStart:
			case State.Array:
				return WriteState.Array;
			case State.ConstructorStart:
			case State.Constructor:
				return WriteState.Constructor;
			case State.Property:
				return WriteState.Property;
			case State.Start:
				return WriteState.Start;
			default:
				throw JsonWriterException.Create(this, "Invalid state: " + this._currentState, null);
			}
		}
	}

	internal string ContainerPath
	{
		get
		{
			if (this._currentPosition.Type == JsonContainerType.None || this._stack == null)
			{
				return string.Empty;
			}
			return JsonPosition.BuildPath(this._stack, null);
		}
	}

	/// <summary>
	/// Gets the path of the writer. 
	/// </summary>
	public string Path
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
	/// Gets or sets a value indicating how JSON text output should be formatted.
	/// </summary>
	public Formatting Formatting
	{
		get
		{
			return this._formatting;
		}
		set
		{
			if (value < Formatting.None || value > Formatting.Indented)
			{
				throw new ArgumentOutOfRangeException("value");
			}
			this._formatting = value;
		}
	}

	/// <summary>
	/// Gets or sets how dates are written to JSON text.
	/// </summary>
	public DateFormatHandling DateFormatHandling
	{
		get
		{
			return this._dateFormatHandling;
		}
		set
		{
			if (value < DateFormatHandling.IsoDateFormat || value > DateFormatHandling.MicrosoftDateFormat)
			{
				throw new ArgumentOutOfRangeException("value");
			}
			this._dateFormatHandling = value;
		}
	}

	/// <summary>
	/// Gets or sets how <see cref="T:System.DateTime" /> time zones are handled when writing JSON text.
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
	/// Gets or sets how strings are escaped when writing JSON text.
	/// </summary>
	public StringEscapeHandling StringEscapeHandling
	{
		get
		{
			return this._stringEscapeHandling;
		}
		set
		{
			if (value < StringEscapeHandling.Default || value > StringEscapeHandling.EscapeHtml)
			{
				throw new ArgumentOutOfRangeException("value");
			}
			this._stringEscapeHandling = value;
			this.OnStringEscapeHandlingChanged();
		}
	}

	/// <summary>
	/// Gets or sets how special floating point numbers, e.g. <see cref="F:System.Double.NaN" />,
	/// <see cref="F:System.Double.PositiveInfinity" /> and <see cref="F:System.Double.NegativeInfinity" />,
	/// are written to JSON text.
	/// </summary>
	public FloatFormatHandling FloatFormatHandling
	{
		get
		{
			return this._floatFormatHandling;
		}
		set
		{
			if (value < FloatFormatHandling.String || value > FloatFormatHandling.DefaultValue)
			{
				throw new ArgumentOutOfRangeException("value");
			}
			this._floatFormatHandling = value;
		}
	}

	/// <summary>
	/// Gets or sets how <see cref="T:System.DateTime" /> and <see cref="T:System.DateTimeOffset" /> values are formatted when writing JSON text.
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
	/// Gets or sets the culture used when writing JSON. Defaults to <see cref="P:System.Globalization.CultureInfo.InvariantCulture" />.
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

	async ValueTask IAsyncDisposable.DisposeAsync()
	{
		if (this._currentState != State.Closed)
		{
			await this.CloseAsync().ConfigureAwait(continueOnCapturedContext: false);
		}
	}

	internal Task AutoCompleteAsync(JsonToken tokenBeingWritten, CancellationToken cancellationToken)
	{
		State currentState = this._currentState;
		State state = JsonWriter.StateArray[(int)tokenBeingWritten][(int)currentState];
		if (state == State.Error)
		{
			throw JsonWriterException.Create(this, "Token {0} in state {1} would result in an invalid JSON object.".FormatWith(CultureInfo.InvariantCulture, tokenBeingWritten.ToString(), currentState.ToString()), null);
		}
		this._currentState = state;
		if (this._formatting == Formatting.Indented)
		{
			switch (currentState)
			{
			case State.Property:
				return this.WriteIndentSpaceAsync(cancellationToken);
			case State.ArrayStart:
			case State.ConstructorStart:
				return this.WriteIndentAsync(cancellationToken);
			case State.Array:
			case State.Constructor:
				if (tokenBeingWritten != JsonToken.Comment)
				{
					return this.AutoCompleteAsync(cancellationToken);
				}
				return this.WriteIndentAsync(cancellationToken);
			case State.Object:
				switch (tokenBeingWritten)
				{
				case JsonToken.PropertyName:
					return this.AutoCompleteAsync(cancellationToken);
				default:
					return this.WriteValueDelimiterAsync(cancellationToken);
				case JsonToken.Comment:
					break;
				}
				break;
			default:
				if (tokenBeingWritten == JsonToken.PropertyName)
				{
					return this.WriteIndentAsync(cancellationToken);
				}
				break;
			case State.Start:
				break;
			}
		}
		else if (tokenBeingWritten != JsonToken.Comment)
		{
			switch (currentState)
			{
			case State.Object:
			case State.Array:
			case State.Constructor:
				return this.WriteValueDelimiterAsync(cancellationToken);
			}
		}
		return AsyncUtils.CompletedTask;
	}

	private async Task AutoCompleteAsync(CancellationToken cancellationToken)
	{
		await this.WriteValueDelimiterAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		await this.WriteIndentAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	/// <summary>
	/// Asynchronously closes this writer.
	/// If <see cref="P:Newtonsoft.Json.JsonWriter.CloseOutput" /> is set to <c>true</c>, the destination is also closed.
	/// </summary>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>The default behaviour is to execute synchronously, returning an already-completed task. Derived
	/// classes can override this behaviour for true asynchronicity.</remarks>
	public virtual Task CloseAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		if (cancellationToken.IsCancellationRequested)
		{
			return cancellationToken.FromCanceled();
		}
		this.Close();
		return AsyncUtils.CompletedTask;
	}

	/// <summary>
	/// Asynchronously flushes whatever is in the buffer to the destination and also flushes the destination.
	/// </summary>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>The default behaviour is to execute synchronously, returning an already-completed task. Derived
	/// classes can override this behaviour for true asynchronicity.</remarks>
	public virtual Task FlushAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		if (cancellationToken.IsCancellationRequested)
		{
			return cancellationToken.FromCanceled();
		}
		this.Flush();
		return AsyncUtils.CompletedTask;
	}

	/// <summary>
	/// Asynchronously writes the specified end token.
	/// </summary>
	/// <param name="token">The end token to write.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>The default behaviour is to execute synchronously, returning an already-completed task. Derived
	/// classes can override this behaviour for true asynchronicity.</remarks>
	protected virtual Task WriteEndAsync(JsonToken token, CancellationToken cancellationToken)
	{
		if (cancellationToken.IsCancellationRequested)
		{
			return cancellationToken.FromCanceled();
		}
		this.WriteEnd(token);
		return AsyncUtils.CompletedTask;
	}

	/// <summary>
	/// Asynchronously writes indent characters.
	/// </summary>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>The default behaviour is to execute synchronously, returning an already-completed task. Derived
	/// classes can override this behaviour for true asynchronicity.</remarks>
	protected virtual Task WriteIndentAsync(CancellationToken cancellationToken)
	{
		if (cancellationToken.IsCancellationRequested)
		{
			return cancellationToken.FromCanceled();
		}
		this.WriteIndent();
		return AsyncUtils.CompletedTask;
	}

	/// <summary>
	/// Asynchronously writes the JSON value delimiter.
	/// </summary>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>The default behaviour is to execute synchronously, returning an already-completed task. Derived
	/// classes can override this behaviour for true asynchronicity.</remarks>
	protected virtual Task WriteValueDelimiterAsync(CancellationToken cancellationToken)
	{
		if (cancellationToken.IsCancellationRequested)
		{
			return cancellationToken.FromCanceled();
		}
		this.WriteValueDelimiter();
		return AsyncUtils.CompletedTask;
	}

	/// <summary>
	/// Asynchronously writes an indent space.
	/// </summary>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>The default behaviour is to execute synchronously, returning an already-completed task. Derived
	/// classes can override this behaviour for true asynchronicity.</remarks>
	protected virtual Task WriteIndentSpaceAsync(CancellationToken cancellationToken)
	{
		if (cancellationToken.IsCancellationRequested)
		{
			return cancellationToken.FromCanceled();
		}
		this.WriteIndentSpace();
		return AsyncUtils.CompletedTask;
	}

	/// <summary>
	/// Asynchronously writes raw JSON without changing the writer's state.
	/// </summary>
	/// <param name="json">The raw JSON to write.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>The default behaviour is to execute synchronously, returning an already-completed task. Derived
	/// classes can override this behaviour for true asynchronicity.</remarks>
	public virtual Task WriteRawAsync(string? json, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (cancellationToken.IsCancellationRequested)
		{
			return cancellationToken.FromCanceled();
		}
		this.WriteRaw(json);
		return AsyncUtils.CompletedTask;
	}

	/// <summary>
	/// Asynchronously writes the end of the current JSON object or array.
	/// </summary>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>The default behaviour is to execute synchronously, returning an already-completed task. Derived
	/// classes can override this behaviour for true asynchronicity.</remarks>
	public virtual Task WriteEndAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		if (cancellationToken.IsCancellationRequested)
		{
			return cancellationToken.FromCanceled();
		}
		this.WriteEnd();
		return AsyncUtils.CompletedTask;
	}

	internal Task WriteEndInternalAsync(CancellationToken cancellationToken)
	{
		JsonContainerType jsonContainerType = this.Peek();
		switch (jsonContainerType)
		{
		case JsonContainerType.Object:
			return this.WriteEndObjectAsync(cancellationToken);
		case JsonContainerType.Array:
			return this.WriteEndArrayAsync(cancellationToken);
		case JsonContainerType.Constructor:
			return this.WriteEndConstructorAsync(cancellationToken);
		default:
			if (cancellationToken.IsCancellationRequested)
			{
				return cancellationToken.FromCanceled();
			}
			throw JsonWriterException.Create(this, "Unexpected type when writing end: " + jsonContainerType, null);
		}
	}

	internal Task InternalWriteEndAsync(JsonContainerType type, CancellationToken cancellationToken)
	{
		if (cancellationToken.IsCancellationRequested)
		{
			return cancellationToken.FromCanceled();
		}
		int levelsToComplete = this.CalculateLevelsToComplete(type);
		while (levelsToComplete-- > 0)
		{
			JsonToken closeTokenForType = this.GetCloseTokenForType(this.Pop());
			Task task;
			if (this._currentState == State.Property)
			{
				task = this.WriteNullAsync(cancellationToken);
				if (!task.IsCompletedSuccessfully())
				{
					return AwaitProperty(task, levelsToComplete, closeTokenForType, cancellationToken);
				}
			}
			if (this._formatting == Formatting.Indented && this._currentState != State.ObjectStart && this._currentState != State.ArrayStart)
			{
				task = this.WriteIndentAsync(cancellationToken);
				if (!task.IsCompletedSuccessfully())
				{
					return AwaitIndent(task, levelsToComplete, closeTokenForType, cancellationToken);
				}
			}
			task = this.WriteEndAsync(closeTokenForType, cancellationToken);
			if (!task.IsCompletedSuccessfully())
			{
				return AwaitEnd(task, levelsToComplete, cancellationToken);
			}
			this.UpdateCurrentState();
		}
		return AsyncUtils.CompletedTask;
		async Task AwaitEnd(Task task2, int LevelsToComplete, CancellationToken CancellationToken)
		{
			await task2.ConfigureAwait(continueOnCapturedContext: false);
			this.UpdateCurrentState();
			await AwaitRemaining(LevelsToComplete, CancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		async Task AwaitIndent(Task task2, int LevelsToComplete, JsonToken token, CancellationToken CancellationToken)
		{
			await task2.ConfigureAwait(continueOnCapturedContext: false);
			await this.WriteEndAsync(token, CancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			this.UpdateCurrentState();
			await AwaitRemaining(LevelsToComplete, CancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		async Task AwaitProperty(Task task2, int LevelsToComplete, JsonToken token, CancellationToken CancellationToken)
		{
			await task2.ConfigureAwait(continueOnCapturedContext: false);
			if (this._formatting == Formatting.Indented && this._currentState != State.ObjectStart && this._currentState != State.ArrayStart)
			{
				await this.WriteIndentAsync(CancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			}
			await this.WriteEndAsync(token, CancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			this.UpdateCurrentState();
			await AwaitRemaining(LevelsToComplete, CancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		async Task AwaitRemaining(int LevelsToComplete, CancellationToken CancellationToken)
		{
			while (LevelsToComplete-- > 0)
			{
				JsonToken token = this.GetCloseTokenForType(this.Pop());
				if (this._currentState == State.Property)
				{
					await this.WriteNullAsync(CancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				}
				if (this._formatting == Formatting.Indented && this._currentState != State.ObjectStart && this._currentState != State.ArrayStart)
				{
					await this.WriteIndentAsync(CancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				}
				await this.WriteEndAsync(token, CancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				this.UpdateCurrentState();
			}
		}
	}

	/// <summary>
	/// Asynchronously writes the end of an array.
	/// </summary>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>The default behaviour is to execute synchronously, returning an already-completed task. Derived
	/// classes can override this behaviour for true asynchronicity.</remarks>
	public virtual Task WriteEndArrayAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		if (cancellationToken.IsCancellationRequested)
		{
			return cancellationToken.FromCanceled();
		}
		this.WriteEndArray();
		return AsyncUtils.CompletedTask;
	}

	/// <summary>
	/// Asynchronously writes the end of a constructor.
	/// </summary>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>The default behaviour is to execute synchronously, returning an already-completed task. Derived
	/// classes can override this behaviour for true asynchronicity.</remarks>
	public virtual Task WriteEndConstructorAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		if (cancellationToken.IsCancellationRequested)
		{
			return cancellationToken.FromCanceled();
		}
		this.WriteEndConstructor();
		return AsyncUtils.CompletedTask;
	}

	/// <summary>
	/// Asynchronously writes the end of a JSON object.
	/// </summary>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>The default behaviour is to execute synchronously, returning an already-completed task. Derived
	/// classes can override this behaviour for true asynchronicity.</remarks>
	public virtual Task WriteEndObjectAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		if (cancellationToken.IsCancellationRequested)
		{
			return cancellationToken.FromCanceled();
		}
		this.WriteEndObject();
		return AsyncUtils.CompletedTask;
	}

	/// <summary>
	/// Asynchronously writes a null value.
	/// </summary>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>The default behaviour is to execute synchronously, returning an already-completed task. Derived
	/// classes can override this behaviour for true asynchronicity.</remarks>
	public virtual Task WriteNullAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		if (cancellationToken.IsCancellationRequested)
		{
			return cancellationToken.FromCanceled();
		}
		this.WriteNull();
		return AsyncUtils.CompletedTask;
	}

	/// <summary>
	/// Asynchronously writes the property name of a name/value pair of a JSON object.
	/// </summary>
	/// <param name="name">The name of the property.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>The default behaviour is to execute synchronously, returning an already-completed task. Derived
	/// classes can override this behaviour for true asynchronicity.</remarks>
	public virtual Task WritePropertyNameAsync(string name, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (cancellationToken.IsCancellationRequested)
		{
			return cancellationToken.FromCanceled();
		}
		this.WritePropertyName(name);
		return AsyncUtils.CompletedTask;
	}

	/// <summary>
	/// Asynchronously writes the property name of a name/value pair of a JSON object.
	/// </summary>
	/// <param name="name">The name of the property.</param>
	/// <param name="escape">A flag to indicate whether the text should be escaped when it is written as a JSON property name.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>The default behaviour is to execute synchronously, returning an already-completed task. Derived
	/// classes can override this behaviour for true asynchronicity.</remarks>
	public virtual Task WritePropertyNameAsync(string name, bool escape, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (cancellationToken.IsCancellationRequested)
		{
			return cancellationToken.FromCanceled();
		}
		this.WritePropertyName(name, escape);
		return AsyncUtils.CompletedTask;
	}

	internal Task InternalWritePropertyNameAsync(string name, CancellationToken cancellationToken)
	{
		if (cancellationToken.IsCancellationRequested)
		{
			return cancellationToken.FromCanceled();
		}
		this._currentPosition.PropertyName = name;
		return this.AutoCompleteAsync(JsonToken.PropertyName, cancellationToken);
	}

	/// <summary>
	/// Asynchronously writes the beginning of a JSON array.
	/// </summary>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>The default behaviour is to execute synchronously, returning an already-completed task. Derived
	/// classes can override this behaviour for true asynchronicity.</remarks>
	public virtual Task WriteStartArrayAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		if (cancellationToken.IsCancellationRequested)
		{
			return cancellationToken.FromCanceled();
		}
		this.WriteStartArray();
		return AsyncUtils.CompletedTask;
	}

	internal async Task InternalWriteStartAsync(JsonToken token, JsonContainerType container, CancellationToken cancellationToken)
	{
		this.UpdateScopeWithFinishedValue();
		await this.AutoCompleteAsync(token, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		this.Push(container);
	}

	/// <summary>
	/// Asynchronously writes a comment <c>/*...*/</c> containing the specified text.
	/// </summary>
	/// <param name="text">Text to place inside the comment.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>The default behaviour is to execute synchronously, returning an already-completed task. Derived
	/// classes can override this behaviour for true asynchronicity.</remarks>
	public virtual Task WriteCommentAsync(string? text, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (cancellationToken.IsCancellationRequested)
		{
			return cancellationToken.FromCanceled();
		}
		this.WriteComment(text);
		return AsyncUtils.CompletedTask;
	}

	internal Task InternalWriteCommentAsync(CancellationToken cancellationToken)
	{
		return this.AutoCompleteAsync(JsonToken.Comment, cancellationToken);
	}

	/// <summary>
	/// Asynchronously writes raw JSON where a value is expected and updates the writer's state.
	/// </summary>
	/// <param name="json">The raw JSON to write.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>The default behaviour is to execute synchronously, returning an already-completed task. Derived
	/// classes can override this behaviour for true asynchronicity.</remarks>
	public virtual Task WriteRawValueAsync(string? json, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (cancellationToken.IsCancellationRequested)
		{
			return cancellationToken.FromCanceled();
		}
		this.WriteRawValue(json);
		return AsyncUtils.CompletedTask;
	}

	/// <summary>
	/// Asynchronously writes the start of a constructor with the given name.
	/// </summary>
	/// <param name="name">The name of the constructor.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>The default behaviour is to execute synchronously, returning an already-completed task. Derived
	/// classes can override this behaviour for true asynchronicity.</remarks>
	public virtual Task WriteStartConstructorAsync(string name, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (cancellationToken.IsCancellationRequested)
		{
			return cancellationToken.FromCanceled();
		}
		this.WriteStartConstructor(name);
		return AsyncUtils.CompletedTask;
	}

	/// <summary>
	/// Asynchronously writes the beginning of a JSON object.
	/// </summary>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>The default behaviour is to execute synchronously, returning an already-completed task. Derived
	/// classes can override this behaviour for true asynchronicity.</remarks>
	public virtual Task WriteStartObjectAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		if (cancellationToken.IsCancellationRequested)
		{
			return cancellationToken.FromCanceled();
		}
		this.WriteStartObject();
		return AsyncUtils.CompletedTask;
	}

	/// <summary>
	/// Asynchronously writes the current <see cref="T:Newtonsoft.Json.JsonReader" /> token.
	/// </summary>
	/// <param name="reader">The <see cref="T:Newtonsoft.Json.JsonReader" /> to read the token from.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>The default behaviour is to execute synchronously, returning an already-completed task. Derived
	/// classes can override this behaviour for true asynchronicity.</remarks>
	public Task WriteTokenAsync(JsonReader reader, CancellationToken cancellationToken = default(CancellationToken))
	{
		return this.WriteTokenAsync(reader, writeChildren: true, cancellationToken);
	}

	/// <summary>
	/// Asynchronously writes the current <see cref="T:Newtonsoft.Json.JsonReader" /> token.
	/// </summary>
	/// <param name="reader">The <see cref="T:Newtonsoft.Json.JsonReader" /> to read the token from.</param>
	/// <param name="writeChildren">A flag indicating whether the current token's children should be written.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>The default behaviour is to execute synchronously, returning an already-completed task. Derived
	/// classes can override this behaviour for true asynchronicity.</remarks>
	public Task WriteTokenAsync(JsonReader reader, bool writeChildren, CancellationToken cancellationToken = default(CancellationToken))
	{
		ValidationUtils.ArgumentNotNull(reader, "reader");
		return this.WriteTokenAsync(reader, writeChildren, writeDateConstructorAsDate: true, writeComments: true, cancellationToken);
	}

	/// <summary>
	/// Asynchronously writes the <see cref="T:Newtonsoft.Json.JsonToken" /> token and its value.
	/// </summary>
	/// <param name="token">The <see cref="T:Newtonsoft.Json.JsonToken" /> to write.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>The default behaviour is to execute synchronously, returning an already-completed task. Derived
	/// classes can override this behaviour for true asynchronicity.</remarks>
	public Task WriteTokenAsync(JsonToken token, CancellationToken cancellationToken = default(CancellationToken))
	{
		return this.WriteTokenAsync(token, null, cancellationToken);
	}

	/// <summary>
	/// Asynchronously writes the <see cref="T:Newtonsoft.Json.JsonToken" /> token and its value.
	/// </summary>
	/// <param name="token">The <see cref="T:Newtonsoft.Json.JsonToken" /> to write.</param>
	/// <param name="value">
	/// The value to write.
	/// A value is only required for tokens that have an associated value, e.g. the <see cref="T:System.String" /> property name for <see cref="F:Newtonsoft.Json.JsonToken.PropertyName" />.
	/// <c>null</c> can be passed to the method for tokens that don't have a value, e.g. <see cref="F:Newtonsoft.Json.JsonToken.StartObject" />.
	/// </param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>The default behaviour is to execute synchronously, returning an already-completed task. Derived
	/// classes can override this behaviour for true asynchronicity.</remarks>
	public Task WriteTokenAsync(JsonToken token, object? value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (cancellationToken.IsCancellationRequested)
		{
			return cancellationToken.FromCanceled();
		}
		switch (token)
		{
		case JsonToken.None:
			return AsyncUtils.CompletedTask;
		case JsonToken.StartObject:
			return this.WriteStartObjectAsync(cancellationToken);
		case JsonToken.StartArray:
			return this.WriteStartArrayAsync(cancellationToken);
		case JsonToken.StartConstructor:
			ValidationUtils.ArgumentNotNull(value, "value");
			return this.WriteStartConstructorAsync(value.ToString(), cancellationToken);
		case JsonToken.PropertyName:
			ValidationUtils.ArgumentNotNull(value, "value");
			return this.WritePropertyNameAsync(value.ToString(), cancellationToken);
		case JsonToken.Comment:
			return this.WriteCommentAsync(value?.ToString(), cancellationToken);
		case JsonToken.Integer:
			ValidationUtils.ArgumentNotNull(value, "value");
			if (!(value is BigInteger bigInteger))
			{
				return this.WriteValueAsync(Convert.ToInt64(value, CultureInfo.InvariantCulture), cancellationToken);
			}
			return this.WriteValueAsync(bigInteger, cancellationToken);
		case JsonToken.Float:
			ValidationUtils.ArgumentNotNull(value, "value");
			if (value is decimal value4)
			{
				return this.WriteValueAsync(value4, cancellationToken);
			}
			if (value is double value5)
			{
				return this.WriteValueAsync(value5, cancellationToken);
			}
			if (value is float value6)
			{
				return this.WriteValueAsync(value6, cancellationToken);
			}
			return this.WriteValueAsync(Convert.ToDouble(value, CultureInfo.InvariantCulture), cancellationToken);
		case JsonToken.String:
			ValidationUtils.ArgumentNotNull(value, "value");
			return this.WriteValueAsync(value.ToString(), cancellationToken);
		case JsonToken.Boolean:
			ValidationUtils.ArgumentNotNull(value, "value");
			return this.WriteValueAsync(Convert.ToBoolean(value, CultureInfo.InvariantCulture), cancellationToken);
		case JsonToken.Null:
			return this.WriteNullAsync(cancellationToken);
		case JsonToken.Undefined:
			return this.WriteUndefinedAsync(cancellationToken);
		case JsonToken.EndObject:
			return this.WriteEndObjectAsync(cancellationToken);
		case JsonToken.EndArray:
			return this.WriteEndArrayAsync(cancellationToken);
		case JsonToken.EndConstructor:
			return this.WriteEndConstructorAsync(cancellationToken);
		case JsonToken.Date:
			ValidationUtils.ArgumentNotNull(value, "value");
			if (value is DateTimeOffset value3)
			{
				return this.WriteValueAsync(value3, cancellationToken);
			}
			return this.WriteValueAsync(Convert.ToDateTime(value, CultureInfo.InvariantCulture), cancellationToken);
		case JsonToken.Raw:
			return this.WriteRawValueAsync(value?.ToString(), cancellationToken);
		case JsonToken.Bytes:
			ValidationUtils.ArgumentNotNull(value, "value");
			if (value is Guid value2)
			{
				return this.WriteValueAsync(value2, cancellationToken);
			}
			return this.WriteValueAsync((byte[])value, cancellationToken);
		default:
			throw MiscellaneousUtils.CreateArgumentOutOfRangeException("token", token, "Unexpected token type.");
		}
	}

	internal virtual async Task WriteTokenAsync(JsonReader reader, bool writeChildren, bool writeDateConstructorAsDate, bool writeComments, CancellationToken cancellationToken)
	{
		int initialDepth = this.CalculateWriteTokenInitialDepth(reader);
		bool flag;
		do
		{
			if (writeDateConstructorAsDate && reader.TokenType == JsonToken.StartConstructor && string.Equals(reader.Value?.ToString(), "Date", StringComparison.Ordinal))
			{
				await this.WriteConstructorDateAsync(reader, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			}
			else if (writeComments || reader.TokenType != JsonToken.Comment)
			{
				await this.WriteTokenAsync(reader.TokenType, reader.Value, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			}
			flag = initialDepth - 1 < reader.Depth - (JsonTokenUtils.IsEndToken(reader.TokenType) ? 1 : 0) && writeChildren;
			if (flag)
			{
				flag = await reader.ReadAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			}
		}
		while (flag);
		if (this.IsWriteTokenIncomplete(reader, writeChildren, initialDepth))
		{
			throw JsonWriterException.Create(this, "Unexpected end when reading token.", null);
		}
	}

	internal async Task WriteTokenSyncReadingAsync(JsonReader reader, CancellationToken cancellationToken)
	{
		int initialDepth = this.CalculateWriteTokenInitialDepth(reader);
		bool flag;
		do
		{
			if (reader.TokenType == JsonToken.StartConstructor && string.Equals(reader.Value?.ToString(), "Date", StringComparison.Ordinal))
			{
				this.WriteConstructorDate(reader);
			}
			else
			{
				this.WriteToken(reader.TokenType, reader.Value);
			}
			flag = initialDepth - 1 < reader.Depth - (JsonTokenUtils.IsEndToken(reader.TokenType) ? 1 : 0);
			if (flag)
			{
				flag = await reader.ReadAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			}
		}
		while (flag);
		if (initialDepth < this.CalculateWriteTokenFinalDepth(reader))
		{
			throw JsonWriterException.Create(this, "Unexpected end when reading token.", null);
		}
	}

	private async Task WriteConstructorDateAsync(JsonReader reader, CancellationToken cancellationToken)
	{
		if (!(await reader.ReadAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false)))
		{
			throw JsonWriterException.Create(this, "Unexpected end when reading date constructor.", null);
		}
		if (reader.TokenType != JsonToken.Integer)
		{
			throw JsonWriterException.Create(this, "Unexpected token when reading date constructor. Expected Integer, got " + reader.TokenType, null);
		}
		DateTime date = DateTimeUtils.ConvertJavaScriptTicksToDateTime((long)reader.Value);
		if (!(await reader.ReadAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false)))
		{
			throw JsonWriterException.Create(this, "Unexpected end when reading date constructor.", null);
		}
		if (reader.TokenType != JsonToken.EndConstructor)
		{
			throw JsonWriterException.Create(this, "Unexpected token when reading date constructor. Expected EndConstructor, got " + reader.TokenType, null);
		}
		await this.WriteValueAsync(date, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	/// <summary>
	/// Asynchronously writes a <see cref="T:System.Nullable`1" /> of <see cref="T:System.Boolean" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Nullable`1" /> of <see cref="T:System.Boolean" /> value to write.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>The default behaviour is to execute synchronously, returning an already-completed task. Derived
	/// classes can override this behaviour for true asynchronicity.</remarks>
	public virtual Task WriteValueAsync(bool value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (cancellationToken.IsCancellationRequested)
		{
			return cancellationToken.FromCanceled();
		}
		this.WriteValue(value);
		return AsyncUtils.CompletedTask;
	}

	/// <summary>
	/// Asynchronously writes a <see cref="T:System.Boolean" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Boolean" /> value to write.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>The default behaviour is to execute synchronously, returning an already-completed task. Derived
	/// classes can override this behaviour for true asynchronicity.</remarks>
	public virtual Task WriteValueAsync(bool? value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (cancellationToken.IsCancellationRequested)
		{
			return cancellationToken.FromCanceled();
		}
		this.WriteValue(value);
		return AsyncUtils.CompletedTask;
	}

	/// <summary>
	/// Asynchronously writes a <see cref="T:System.Byte" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Byte" /> value to write.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>The default behaviour is to execute synchronously, returning an already-completed task. Derived
	/// classes can override this behaviour for true asynchronicity.</remarks>
	public virtual Task WriteValueAsync(byte value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (cancellationToken.IsCancellationRequested)
		{
			return cancellationToken.FromCanceled();
		}
		this.WriteValue(value);
		return AsyncUtils.CompletedTask;
	}

	/// <summary>
	/// Asynchronously writes a <see cref="T:System.Nullable`1" /> of <see cref="T:System.Byte" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Nullable`1" /> of <see cref="T:System.Byte" /> value to write.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>The default behaviour is to execute synchronously, returning an already-completed task. Derived
	/// classes can override this behaviour for true asynchronicity.</remarks>
	public virtual Task WriteValueAsync(byte? value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (cancellationToken.IsCancellationRequested)
		{
			return cancellationToken.FromCanceled();
		}
		this.WriteValue(value);
		return AsyncUtils.CompletedTask;
	}

	/// <summary>
	/// Asynchronously writes a <see cref="T:System.Byte" />[] value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Byte" />[] value to write.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>The default behaviour is to execute synchronously, returning an already-completed task. Derived
	/// classes can override this behaviour for true asynchronicity.</remarks>
	public virtual Task WriteValueAsync(byte[]? value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (cancellationToken.IsCancellationRequested)
		{
			return cancellationToken.FromCanceled();
		}
		this.WriteValue(value);
		return AsyncUtils.CompletedTask;
	}

	/// <summary>
	/// Asynchronously writes a <see cref="T:System.Char" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Char" /> value to write.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>The default behaviour is to execute synchronously, returning an already-completed task. Derived
	/// classes can override this behaviour for true asynchronicity.</remarks>
	public virtual Task WriteValueAsync(char value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (cancellationToken.IsCancellationRequested)
		{
			return cancellationToken.FromCanceled();
		}
		this.WriteValue(value);
		return AsyncUtils.CompletedTask;
	}

	/// <summary>
	/// Asynchronously writes a <see cref="T:System.Nullable`1" /> of <see cref="T:System.Char" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Nullable`1" /> of <see cref="T:System.Char" /> value to write.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>The default behaviour is to execute synchronously, returning an already-completed task. Derived
	/// classes can override this behaviour for true asynchronicity.</remarks>
	public virtual Task WriteValueAsync(char? value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (cancellationToken.IsCancellationRequested)
		{
			return cancellationToken.FromCanceled();
		}
		this.WriteValue(value);
		return AsyncUtils.CompletedTask;
	}

	/// <summary>
	/// Asynchronously writes a <see cref="T:System.DateTime" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.DateTime" /> value to write.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>The default behaviour is to execute synchronously, returning an already-completed task. Derived
	/// classes can override this behaviour for true asynchronicity.</remarks>
	public virtual Task WriteValueAsync(DateTime value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (cancellationToken.IsCancellationRequested)
		{
			return cancellationToken.FromCanceled();
		}
		this.WriteValue(value);
		return AsyncUtils.CompletedTask;
	}

	/// <summary>
	/// Asynchronously writes a <see cref="T:System.Nullable`1" /> of <see cref="T:System.DateTime" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Nullable`1" /> of <see cref="T:System.DateTime" /> value to write.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>The default behaviour is to execute synchronously, returning an already-completed task. Derived
	/// classes can override this behaviour for true asynchronicity.</remarks>
	public virtual Task WriteValueAsync(DateTime? value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (cancellationToken.IsCancellationRequested)
		{
			return cancellationToken.FromCanceled();
		}
		this.WriteValue(value);
		return AsyncUtils.CompletedTask;
	}

	/// <summary>
	/// Asynchronously writes a <see cref="T:System.DateTimeOffset" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.DateTimeOffset" /> value to write.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>The default behaviour is to execute synchronously, returning an already-completed task. Derived
	/// classes can override this behaviour for true asynchronicity.</remarks>
	public virtual Task WriteValueAsync(DateTimeOffset value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (cancellationToken.IsCancellationRequested)
		{
			return cancellationToken.FromCanceled();
		}
		this.WriteValue(value);
		return AsyncUtils.CompletedTask;
	}

	/// <summary>
	/// Asynchronously writes a <see cref="T:System.Nullable`1" /> of <see cref="T:System.DateTimeOffset" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Nullable`1" /> of <see cref="T:System.DateTimeOffset" /> value to write.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>The default behaviour is to execute synchronously, returning an already-completed task. Derived
	/// classes can override this behaviour for true asynchronicity.</remarks>
	public virtual Task WriteValueAsync(DateTimeOffset? value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (cancellationToken.IsCancellationRequested)
		{
			return cancellationToken.FromCanceled();
		}
		this.WriteValue(value);
		return AsyncUtils.CompletedTask;
	}

	/// <summary>
	/// Asynchronously writes a <see cref="T:System.Decimal" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Decimal" /> value to write.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>The default behaviour is to execute synchronously, returning an already-completed task. Derived
	/// classes can override this behaviour for true asynchronicity.</remarks>
	public virtual Task WriteValueAsync(decimal value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (cancellationToken.IsCancellationRequested)
		{
			return cancellationToken.FromCanceled();
		}
		this.WriteValue(value);
		return AsyncUtils.CompletedTask;
	}

	/// <summary>
	/// Asynchronously writes a <see cref="T:System.Nullable`1" /> of <see cref="T:System.Decimal" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Nullable`1" /> of <see cref="T:System.Decimal" /> value to write.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>The default behaviour is to execute synchronously, returning an already-completed task. Derived
	/// classes can override this behaviour for true asynchronicity.</remarks>
	public virtual Task WriteValueAsync(decimal? value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (cancellationToken.IsCancellationRequested)
		{
			return cancellationToken.FromCanceled();
		}
		this.WriteValue(value);
		return AsyncUtils.CompletedTask;
	}

	/// <summary>
	/// Asynchronously writes a <see cref="T:System.Double" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Double" /> value to write.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>The default behaviour is to execute synchronously, returning an already-completed task. Derived
	/// classes can override this behaviour for true asynchronicity.</remarks>
	public virtual Task WriteValueAsync(double value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (cancellationToken.IsCancellationRequested)
		{
			return cancellationToken.FromCanceled();
		}
		this.WriteValue(value);
		return AsyncUtils.CompletedTask;
	}

	/// <summary>
	/// Asynchronously writes a <see cref="T:System.Nullable`1" /> of <see cref="T:System.Double" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Nullable`1" /> of <see cref="T:System.Double" /> value to write.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>The default behaviour is to execute synchronously, returning an already-completed task. Derived
	/// classes can override this behaviour for true asynchronicity.</remarks>
	public virtual Task WriteValueAsync(double? value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (cancellationToken.IsCancellationRequested)
		{
			return cancellationToken.FromCanceled();
		}
		this.WriteValue(value);
		return AsyncUtils.CompletedTask;
	}

	/// <summary>
	/// Asynchronously writes a <see cref="T:System.Single" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Single" /> value to write.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>The default behaviour is to execute synchronously, returning an already-completed task. Derived
	/// classes can override this behaviour for true asynchronicity.</remarks>
	public virtual Task WriteValueAsync(float value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (cancellationToken.IsCancellationRequested)
		{
			return cancellationToken.FromCanceled();
		}
		this.WriteValue(value);
		return AsyncUtils.CompletedTask;
	}

	/// <summary>
	/// Asynchronously writes a <see cref="T:System.Nullable`1" /> of <see cref="T:System.Single" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Nullable`1" /> of <see cref="T:System.Single" /> value to write.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>The default behaviour is to execute synchronously, returning an already-completed task. Derived
	/// classes can override this behaviour for true asynchronicity.</remarks>
	public virtual Task WriteValueAsync(float? value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (cancellationToken.IsCancellationRequested)
		{
			return cancellationToken.FromCanceled();
		}
		this.WriteValue(value);
		return AsyncUtils.CompletedTask;
	}

	/// <summary>
	/// Asynchronously writes a <see cref="T:System.Guid" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Guid" /> value to write.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>The default behaviour is to execute synchronously, returning an already-completed task. Derived
	/// classes can override this behaviour for true asynchronicity.</remarks>
	public virtual Task WriteValueAsync(Guid value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (cancellationToken.IsCancellationRequested)
		{
			return cancellationToken.FromCanceled();
		}
		this.WriteValue(value);
		return AsyncUtils.CompletedTask;
	}

	/// <summary>
	/// Asynchronously writes a <see cref="T:System.Nullable`1" /> of <see cref="T:System.Guid" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Nullable`1" /> of <see cref="T:System.Guid" /> value to write.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>The default behaviour is to execute synchronously, returning an already-completed task. Derived
	/// classes can override this behaviour for true asynchronicity.</remarks>
	public virtual Task WriteValueAsync(Guid? value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (cancellationToken.IsCancellationRequested)
		{
			return cancellationToken.FromCanceled();
		}
		this.WriteValue(value);
		return AsyncUtils.CompletedTask;
	}

	/// <summary>
	/// Asynchronously writes a <see cref="T:System.Int32" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Int32" /> value to write.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>The default behaviour is to execute synchronously, returning an already-completed task. Derived
	/// classes can override this behaviour for true asynchronicity.</remarks>
	public virtual Task WriteValueAsync(int value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (cancellationToken.IsCancellationRequested)
		{
			return cancellationToken.FromCanceled();
		}
		this.WriteValue(value);
		return AsyncUtils.CompletedTask;
	}

	/// <summary>
	/// Asynchronously writes a <see cref="T:System.Nullable`1" /> of <see cref="T:System.Int32" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Nullable`1" /> of <see cref="T:System.Int32" /> value to write.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>The default behaviour is to execute synchronously, returning an already-completed task. Derived
	/// classes can override this behaviour for true asynchronicity.</remarks>
	public virtual Task WriteValueAsync(int? value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (cancellationToken.IsCancellationRequested)
		{
			return cancellationToken.FromCanceled();
		}
		this.WriteValue(value);
		return AsyncUtils.CompletedTask;
	}

	/// <summary>
	/// Asynchronously writes a <see cref="T:System.Int64" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Int64" /> value to write.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>The default behaviour is to execute synchronously, returning an already-completed task. Derived
	/// classes can override this behaviour for true asynchronicity.</remarks>
	public virtual Task WriteValueAsync(long value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (cancellationToken.IsCancellationRequested)
		{
			return cancellationToken.FromCanceled();
		}
		this.WriteValue(value);
		return AsyncUtils.CompletedTask;
	}

	/// <summary>
	/// Asynchronously writes a <see cref="T:System.Nullable`1" /> of <see cref="T:System.Int64" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Nullable`1" /> of <see cref="T:System.Int64" /> value to write.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>The default behaviour is to execute synchronously, returning an already-completed task. Derived
	/// classes can override this behaviour for true asynchronicity.</remarks>
	public virtual Task WriteValueAsync(long? value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (cancellationToken.IsCancellationRequested)
		{
			return cancellationToken.FromCanceled();
		}
		this.WriteValue(value);
		return AsyncUtils.CompletedTask;
	}

	/// <summary>
	/// Asynchronously writes a <see cref="T:System.Object" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Object" /> value to write.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>The default behaviour is to execute synchronously, returning an already-completed task. Derived
	/// classes can override this behaviour for true asynchronicity.</remarks>
	public virtual Task WriteValueAsync(object? value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (cancellationToken.IsCancellationRequested)
		{
			return cancellationToken.FromCanceled();
		}
		this.WriteValue(value);
		return AsyncUtils.CompletedTask;
	}

	/// <summary>
	/// Asynchronously writes a <see cref="T:System.SByte" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.SByte" /> value to write.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>The default behaviour is to execute synchronously, returning an already-completed task. Derived
	/// classes can override this behaviour for true asynchronicity.</remarks>
	[CLSCompliant(false)]
	public virtual Task WriteValueAsync(sbyte value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (cancellationToken.IsCancellationRequested)
		{
			return cancellationToken.FromCanceled();
		}
		this.WriteValue(value);
		return AsyncUtils.CompletedTask;
	}

	/// <summary>
	/// Asynchronously writes a <see cref="T:System.Nullable`1" /> of <see cref="T:System.SByte" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Nullable`1" /> of <see cref="T:System.SByte" /> value to write.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>The default behaviour is to execute synchronously, returning an already-completed task. Derived
	/// classes can override this behaviour for true asynchronicity.</remarks>
	[CLSCompliant(false)]
	public virtual Task WriteValueAsync(sbyte? value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (cancellationToken.IsCancellationRequested)
		{
			return cancellationToken.FromCanceled();
		}
		this.WriteValue(value);
		return AsyncUtils.CompletedTask;
	}

	/// <summary>
	/// Asynchronously writes a <see cref="T:System.Int16" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Int16" /> value to write.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>The default behaviour is to execute synchronously, returning an already-completed task. Derived
	/// classes can override this behaviour for true asynchronicity.</remarks>
	public virtual Task WriteValueAsync(short value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (cancellationToken.IsCancellationRequested)
		{
			return cancellationToken.FromCanceled();
		}
		this.WriteValue(value);
		return AsyncUtils.CompletedTask;
	}

	/// <summary>
	/// Asynchronously writes a <see cref="T:System.Nullable`1" /> of <see cref="T:System.Int16" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Nullable`1" /> of <see cref="T:System.Int16" /> value to write.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>The default behaviour is to execute synchronously, returning an already-completed task. Derived
	/// classes can override this behaviour for true asynchronicity.</remarks>
	public virtual Task WriteValueAsync(short? value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (cancellationToken.IsCancellationRequested)
		{
			return cancellationToken.FromCanceled();
		}
		this.WriteValue(value);
		return AsyncUtils.CompletedTask;
	}

	/// <summary>
	/// Asynchronously writes a <see cref="T:System.String" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.String" /> value to write.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>The default behaviour is to execute synchronously, returning an already-completed task. Derived
	/// classes can override this behaviour for true asynchronicity.</remarks>
	public virtual Task WriteValueAsync(string? value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (cancellationToken.IsCancellationRequested)
		{
			return cancellationToken.FromCanceled();
		}
		this.WriteValue(value);
		return AsyncUtils.CompletedTask;
	}

	/// <summary>
	/// Asynchronously writes a <see cref="T:System.TimeSpan" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.TimeSpan" /> value to write.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>The default behaviour is to execute synchronously, returning an already-completed task. Derived
	/// classes can override this behaviour for true asynchronicity.</remarks>
	public virtual Task WriteValueAsync(TimeSpan value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (cancellationToken.IsCancellationRequested)
		{
			return cancellationToken.FromCanceled();
		}
		this.WriteValue(value);
		return AsyncUtils.CompletedTask;
	}

	/// <summary>
	/// Asynchronously writes a <see cref="T:System.Nullable`1" /> of <see cref="T:System.TimeSpan" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Nullable`1" /> of <see cref="T:System.TimeSpan" /> value to write.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>The default behaviour is to execute synchronously, returning an already-completed task. Derived
	/// classes can override this behaviour for true asynchronicity.</remarks>
	public virtual Task WriteValueAsync(TimeSpan? value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (cancellationToken.IsCancellationRequested)
		{
			return cancellationToken.FromCanceled();
		}
		this.WriteValue(value);
		return AsyncUtils.CompletedTask;
	}

	/// <summary>
	/// Asynchronously writes a <see cref="T:System.UInt32" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.UInt32" /> value to write.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>The default behaviour is to execute synchronously, returning an already-completed task. Derived
	/// classes can override this behaviour for true asynchronicity.</remarks>
	[CLSCompliant(false)]
	public virtual Task WriteValueAsync(uint value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (cancellationToken.IsCancellationRequested)
		{
			return cancellationToken.FromCanceled();
		}
		this.WriteValue(value);
		return AsyncUtils.CompletedTask;
	}

	/// <summary>
	/// Asynchronously writes a <see cref="T:System.Nullable`1" /> of <see cref="T:System.UInt32" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Nullable`1" /> of <see cref="T:System.UInt32" /> value to write.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>The default behaviour is to execute synchronously, returning an already-completed task. Derived
	/// classes can override this behaviour for true asynchronicity.</remarks>
	[CLSCompliant(false)]
	public virtual Task WriteValueAsync(uint? value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (cancellationToken.IsCancellationRequested)
		{
			return cancellationToken.FromCanceled();
		}
		this.WriteValue(value);
		return AsyncUtils.CompletedTask;
	}

	/// <summary>
	/// Asynchronously writes a <see cref="T:System.UInt64" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.UInt64" /> value to write.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>The default behaviour is to execute synchronously, returning an already-completed task. Derived
	/// classes can override this behaviour for true asynchronicity.</remarks>
	[CLSCompliant(false)]
	public virtual Task WriteValueAsync(ulong value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (cancellationToken.IsCancellationRequested)
		{
			return cancellationToken.FromCanceled();
		}
		this.WriteValue(value);
		return AsyncUtils.CompletedTask;
	}

	/// <summary>
	/// Asynchronously writes a <see cref="T:System.Nullable`1" /> of <see cref="T:System.UInt64" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Nullable`1" /> of <see cref="T:System.UInt64" /> value to write.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>The default behaviour is to execute synchronously, returning an already-completed task. Derived
	/// classes can override this behaviour for true asynchronicity.</remarks>
	[CLSCompliant(false)]
	public virtual Task WriteValueAsync(ulong? value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (cancellationToken.IsCancellationRequested)
		{
			return cancellationToken.FromCanceled();
		}
		this.WriteValue(value);
		return AsyncUtils.CompletedTask;
	}

	/// <summary>
	/// Asynchronously writes a <see cref="T:System.Uri" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Uri" /> value to write.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>The default behaviour is to execute synchronously, returning an already-completed task. Derived
	/// classes can override this behaviour for true asynchronicity.</remarks>
	public virtual Task WriteValueAsync(Uri? value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (cancellationToken.IsCancellationRequested)
		{
			return cancellationToken.FromCanceled();
		}
		this.WriteValue(value);
		return AsyncUtils.CompletedTask;
	}

	/// <summary>
	/// Asynchronously writes a <see cref="T:System.UInt16" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.UInt16" /> value to write.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>The default behaviour is to execute synchronously, returning an already-completed task. Derived
	/// classes can override this behaviour for true asynchronicity.</remarks>
	[CLSCompliant(false)]
	public virtual Task WriteValueAsync(ushort value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (cancellationToken.IsCancellationRequested)
		{
			return cancellationToken.FromCanceled();
		}
		this.WriteValue(value);
		return AsyncUtils.CompletedTask;
	}

	/// <summary>
	/// Asynchronously writes a <see cref="T:System.Nullable`1" /> of <see cref="T:System.UInt16" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Nullable`1" /> of <see cref="T:System.UInt16" /> value to write.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>The default behaviour is to execute synchronously, returning an already-completed task. Derived
	/// classes can override this behaviour for true asynchronicity.</remarks>
	[CLSCompliant(false)]
	public virtual Task WriteValueAsync(ushort? value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (cancellationToken.IsCancellationRequested)
		{
			return cancellationToken.FromCanceled();
		}
		this.WriteValue(value);
		return AsyncUtils.CompletedTask;
	}

	/// <summary>
	/// Asynchronously writes an undefined value.
	/// </summary>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>The default behaviour is to execute synchronously, returning an already-completed task. Derived
	/// classes can override this behaviour for true asynchronicity.</remarks>
	public virtual Task WriteUndefinedAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		if (cancellationToken.IsCancellationRequested)
		{
			return cancellationToken.FromCanceled();
		}
		this.WriteUndefined();
		return AsyncUtils.CompletedTask;
	}

	/// <summary>
	/// Asynchronously writes the given white space.
	/// </summary>
	/// <param name="ws">The string of white space characters.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>The default behaviour is to execute synchronously, returning an already-completed task. Derived
	/// classes can override this behaviour for true asynchronicity.</remarks>
	public virtual Task WriteWhitespaceAsync(string ws, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (cancellationToken.IsCancellationRequested)
		{
			return cancellationToken.FromCanceled();
		}
		this.WriteWhitespace(ws);
		return AsyncUtils.CompletedTask;
	}

	internal Task InternalWriteValueAsync(JsonToken token, CancellationToken cancellationToken)
	{
		if (cancellationToken.IsCancellationRequested)
		{
			return cancellationToken.FromCanceled();
		}
		this.UpdateScopeWithFinishedValue();
		return this.AutoCompleteAsync(token, cancellationToken);
	}

	/// <summary>
	/// Asynchronously ets the state of the <see cref="T:Newtonsoft.Json.JsonWriter" />.
	/// </summary>
	/// <param name="token">The <see cref="T:Newtonsoft.Json.JsonToken" /> being written.</param>
	/// <param name="value">The value being written.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>The default behaviour is to execute synchronously, returning an already-completed task. Derived
	/// classes can override this behaviour for true asynchronicity.</remarks>
	protected Task SetWriteStateAsync(JsonToken token, object value, CancellationToken cancellationToken)
	{
		if (cancellationToken.IsCancellationRequested)
		{
			return cancellationToken.FromCanceled();
		}
		switch (token)
		{
		case JsonToken.StartObject:
			return this.InternalWriteStartAsync(token, JsonContainerType.Object, cancellationToken);
		case JsonToken.StartArray:
			return this.InternalWriteStartAsync(token, JsonContainerType.Array, cancellationToken);
		case JsonToken.StartConstructor:
			return this.InternalWriteStartAsync(token, JsonContainerType.Constructor, cancellationToken);
		case JsonToken.PropertyName:
			if (!(value is string name))
			{
				throw new ArgumentException("A name is required when setting property name state.", "value");
			}
			return this.InternalWritePropertyNameAsync(name, cancellationToken);
		case JsonToken.Comment:
			return this.InternalWriteCommentAsync(cancellationToken);
		case JsonToken.Raw:
			return AsyncUtils.CompletedTask;
		case JsonToken.Integer:
		case JsonToken.Float:
		case JsonToken.String:
		case JsonToken.Boolean:
		case JsonToken.Null:
		case JsonToken.Undefined:
		case JsonToken.Date:
		case JsonToken.Bytes:
			return this.InternalWriteValueAsync(token, cancellationToken);
		case JsonToken.EndObject:
			return this.InternalWriteEndAsync(JsonContainerType.Object, cancellationToken);
		case JsonToken.EndArray:
			return this.InternalWriteEndAsync(JsonContainerType.Array, cancellationToken);
		case JsonToken.EndConstructor:
			return this.InternalWriteEndAsync(JsonContainerType.Constructor, cancellationToken);
		default:
			throw new ArgumentOutOfRangeException("token");
		}
	}

	internal static Task WriteValueAsync(JsonWriter writer, PrimitiveTypeCode typeCode, object value, CancellationToken cancellationToken)
	{
		while (true)
		{
			switch (typeCode)
			{
			case PrimitiveTypeCode.Char:
				return writer.WriteValueAsync((char)value, cancellationToken);
			case PrimitiveTypeCode.CharNullable:
				return writer.WriteValueAsync((value == null) ? ((char?)null) : new char?((char)value), cancellationToken);
			case PrimitiveTypeCode.Boolean:
				return writer.WriteValueAsync((bool)value, cancellationToken);
			case PrimitiveTypeCode.BooleanNullable:
				return writer.WriteValueAsync((value == null) ? ((bool?)null) : new bool?((bool)value), cancellationToken);
			case PrimitiveTypeCode.SByte:
				return writer.WriteValueAsync((sbyte)value, cancellationToken);
			case PrimitiveTypeCode.SByteNullable:
				return writer.WriteValueAsync((value == null) ? ((sbyte?)null) : new sbyte?((sbyte)value), cancellationToken);
			case PrimitiveTypeCode.Int16:
				return writer.WriteValueAsync((short)value, cancellationToken);
			case PrimitiveTypeCode.Int16Nullable:
				return writer.WriteValueAsync((value == null) ? ((short?)null) : new short?((short)value), cancellationToken);
			case PrimitiveTypeCode.UInt16:
				return writer.WriteValueAsync((ushort)value, cancellationToken);
			case PrimitiveTypeCode.UInt16Nullable:
				return writer.WriteValueAsync((value == null) ? ((ushort?)null) : new ushort?((ushort)value), cancellationToken);
			case PrimitiveTypeCode.Int32:
				return writer.WriteValueAsync((int)value, cancellationToken);
			case PrimitiveTypeCode.Int32Nullable:
				return writer.WriteValueAsync((value == null) ? ((int?)null) : new int?((int)value), cancellationToken);
			case PrimitiveTypeCode.Byte:
				return writer.WriteValueAsync((byte)value, cancellationToken);
			case PrimitiveTypeCode.ByteNullable:
				return writer.WriteValueAsync((value == null) ? ((byte?)null) : new byte?((byte)value), cancellationToken);
			case PrimitiveTypeCode.UInt32:
				return writer.WriteValueAsync((uint)value, cancellationToken);
			case PrimitiveTypeCode.UInt32Nullable:
				return writer.WriteValueAsync((value == null) ? ((uint?)null) : new uint?((uint)value), cancellationToken);
			case PrimitiveTypeCode.Int64:
				return writer.WriteValueAsync((long)value, cancellationToken);
			case PrimitiveTypeCode.Int64Nullable:
				return writer.WriteValueAsync((value == null) ? ((long?)null) : new long?((long)value), cancellationToken);
			case PrimitiveTypeCode.UInt64:
				return writer.WriteValueAsync((ulong)value, cancellationToken);
			case PrimitiveTypeCode.UInt64Nullable:
				return writer.WriteValueAsync((value == null) ? ((ulong?)null) : new ulong?((ulong)value), cancellationToken);
			case PrimitiveTypeCode.Single:
				return writer.WriteValueAsync((float)value, cancellationToken);
			case PrimitiveTypeCode.SingleNullable:
				return writer.WriteValueAsync((value == null) ? ((float?)null) : new float?((float)value), cancellationToken);
			case PrimitiveTypeCode.Double:
				return writer.WriteValueAsync((double)value, cancellationToken);
			case PrimitiveTypeCode.DoubleNullable:
				return writer.WriteValueAsync((value == null) ? ((double?)null) : new double?((double)value), cancellationToken);
			case PrimitiveTypeCode.DateTime:
				return writer.WriteValueAsync((DateTime)value, cancellationToken);
			case PrimitiveTypeCode.DateTimeNullable:
				return writer.WriteValueAsync((value == null) ? ((DateTime?)null) : new DateTime?((DateTime)value), cancellationToken);
			case PrimitiveTypeCode.DateTimeOffset:
				return writer.WriteValueAsync((DateTimeOffset)value, cancellationToken);
			case PrimitiveTypeCode.DateTimeOffsetNullable:
				return writer.WriteValueAsync((value == null) ? ((DateTimeOffset?)null) : new DateTimeOffset?((DateTimeOffset)value), cancellationToken);
			case PrimitiveTypeCode.Decimal:
				return writer.WriteValueAsync((decimal)value, cancellationToken);
			case PrimitiveTypeCode.DecimalNullable:
				return writer.WriteValueAsync((value == null) ? ((decimal?)null) : new decimal?((decimal)value), cancellationToken);
			case PrimitiveTypeCode.Guid:
				return writer.WriteValueAsync((Guid)value, cancellationToken);
			case PrimitiveTypeCode.GuidNullable:
				return writer.WriteValueAsync((value == null) ? ((Guid?)null) : new Guid?((Guid)value), cancellationToken);
			case PrimitiveTypeCode.TimeSpan:
				return writer.WriteValueAsync((TimeSpan)value, cancellationToken);
			case PrimitiveTypeCode.TimeSpanNullable:
				return writer.WriteValueAsync((value == null) ? ((TimeSpan?)null) : new TimeSpan?((TimeSpan)value), cancellationToken);
			case PrimitiveTypeCode.BigInteger:
				return writer.WriteValueAsync((BigInteger)value, cancellationToken);
			case PrimitiveTypeCode.BigIntegerNullable:
				return writer.WriteValueAsync((value == null) ? ((BigInteger?)null) : new BigInteger?((BigInteger)value), cancellationToken);
			case PrimitiveTypeCode.Uri:
				return writer.WriteValueAsync((Uri)value, cancellationToken);
			case PrimitiveTypeCode.String:
				return writer.WriteValueAsync((string)value, cancellationToken);
			case PrimitiveTypeCode.Bytes:
				return writer.WriteValueAsync((byte[])value, cancellationToken);
			case PrimitiveTypeCode.DBNull:
				return writer.WriteNullAsync(cancellationToken);
			}
			if (value is IConvertible convertible)
			{
				JsonWriter.ResolveConvertibleValue(convertible, out typeCode, out value);
				continue;
			}
			if (value == null)
			{
				return writer.WriteNullAsync(cancellationToken);
			}
			throw JsonWriter.CreateUnsupportedTypeException(writer, value);
		}
	}

	internal static State[][] BuildStateArray()
	{
		List<State[]> list = JsonWriter.StateArrayTemplate.ToList();
		State[] item = JsonWriter.StateArrayTemplate[0];
		State[] item2 = JsonWriter.StateArrayTemplate[7];
		ulong[] values = EnumUtils.GetEnumValuesAndNames(typeof(JsonToken)).Values;
		foreach (ulong num in values)
		{
			if (list.Count <= (int)num)
			{
				JsonToken jsonToken = (JsonToken)num;
				if ((uint)(jsonToken - 7) <= 5u || (uint)(jsonToken - 16) <= 1u)
				{
					list.Add(item2);
				}
				else
				{
					list.Add(item);
				}
			}
		}
		return list.ToArray();
	}

	static JsonWriter()
	{
		JsonWriter.StateArrayTemplate = new State[8][]
		{
			new State[10]
			{
				State.Error,
				State.Error,
				State.Error,
				State.Error,
				State.Error,
				State.Error,
				State.Error,
				State.Error,
				State.Error,
				State.Error
			},
			new State[10]
			{
				State.ObjectStart,
				State.ObjectStart,
				State.Error,
				State.Error,
				State.ObjectStart,
				State.ObjectStart,
				State.ObjectStart,
				State.ObjectStart,
				State.Error,
				State.Error
			},
			new State[10]
			{
				State.ArrayStart,
				State.ArrayStart,
				State.Error,
				State.Error,
				State.ArrayStart,
				State.ArrayStart,
				State.ArrayStart,
				State.ArrayStart,
				State.Error,
				State.Error
			},
			new State[10]
			{
				State.ConstructorStart,
				State.ConstructorStart,
				State.Error,
				State.Error,
				State.ConstructorStart,
				State.ConstructorStart,
				State.ConstructorStart,
				State.ConstructorStart,
				State.Error,
				State.Error
			},
			new State[10]
			{
				State.Property,
				State.Error,
				State.Property,
				State.Property,
				State.Error,
				State.Error,
				State.Error,
				State.Error,
				State.Error,
				State.Error
			},
			new State[10]
			{
				State.Start,
				State.Property,
				State.ObjectStart,
				State.Object,
				State.ArrayStart,
				State.Array,
				State.Constructor,
				State.Constructor,
				State.Error,
				State.Error
			},
			new State[10]
			{
				State.Start,
				State.Property,
				State.ObjectStart,
				State.Object,
				State.ArrayStart,
				State.Array,
				State.Constructor,
				State.Constructor,
				State.Error,
				State.Error
			},
			new State[10]
			{
				State.Start,
				State.Object,
				State.Error,
				State.Error,
				State.Array,
				State.Array,
				State.Constructor,
				State.Constructor,
				State.Error,
				State.Error
			}
		};
		JsonWriter.StateArray = JsonWriter.BuildStateArray();
	}

	internal virtual void OnStringEscapeHandlingChanged()
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:Newtonsoft.Json.JsonWriter" /> class.
	/// </summary>
	protected JsonWriter()
	{
		this._currentState = State.Start;
		this._formatting = Formatting.None;
		this._dateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind;
		this.CloseOutput = true;
		this.AutoCompleteOnClose = true;
	}

	internal void UpdateScopeWithFinishedValue()
	{
		if (this._currentPosition.HasIndex)
		{
			this._currentPosition.Position++;
		}
	}

	private void Push(JsonContainerType value)
	{
		if (this._currentPosition.Type != JsonContainerType.None)
		{
			if (this._stack == null)
			{
				this._stack = new List<JsonPosition>();
			}
			this._stack.Add(this._currentPosition);
		}
		this._currentPosition = new JsonPosition(value);
	}

	private JsonContainerType Pop()
	{
		JsonPosition currentPosition = this._currentPosition;
		if (this._stack != null && this._stack.Count > 0)
		{
			this._currentPosition = this._stack[this._stack.Count - 1];
			this._stack.RemoveAt(this._stack.Count - 1);
		}
		else
		{
			this._currentPosition = default(JsonPosition);
		}
		return currentPosition.Type;
	}

	private JsonContainerType Peek()
	{
		return this._currentPosition.Type;
	}

	/// <summary>
	/// Flushes whatever is in the buffer to the destination and also flushes the destination.
	/// </summary>
	public abstract void Flush();

	/// <summary>
	/// Closes this writer.
	/// If <see cref="P:Newtonsoft.Json.JsonWriter.CloseOutput" /> is set to <c>true</c>, the destination is also closed.
	/// If <see cref="P:Newtonsoft.Json.JsonWriter.AutoCompleteOnClose" /> is set to <c>true</c>, the JSON is auto-completed.
	/// </summary>
	public virtual void Close()
	{
		if (this.AutoCompleteOnClose)
		{
			this.AutoCompleteAll();
		}
	}

	/// <summary>
	/// Writes the beginning of a JSON object.
	/// </summary>
	public virtual void WriteStartObject()
	{
		this.InternalWriteStart(JsonToken.StartObject, JsonContainerType.Object);
	}

	/// <summary>
	/// Writes the end of a JSON object.
	/// </summary>
	public virtual void WriteEndObject()
	{
		this.InternalWriteEnd(JsonContainerType.Object);
	}

	/// <summary>
	/// Writes the beginning of a JSON array.
	/// </summary>
	public virtual void WriteStartArray()
	{
		this.InternalWriteStart(JsonToken.StartArray, JsonContainerType.Array);
	}

	/// <summary>
	/// Writes the end of an array.
	/// </summary>
	public virtual void WriteEndArray()
	{
		this.InternalWriteEnd(JsonContainerType.Array);
	}

	/// <summary>
	/// Writes the start of a constructor with the given name.
	/// </summary>
	/// <param name="name">The name of the constructor.</param>
	public virtual void WriteStartConstructor(string name)
	{
		this.InternalWriteStart(JsonToken.StartConstructor, JsonContainerType.Constructor);
	}

	/// <summary>
	/// Writes the end constructor.
	/// </summary>
	public virtual void WriteEndConstructor()
	{
		this.InternalWriteEnd(JsonContainerType.Constructor);
	}

	/// <summary>
	/// Writes the property name of a name/value pair of a JSON object.
	/// </summary>
	/// <param name="name">The name of the property.</param>
	public virtual void WritePropertyName(string name)
	{
		this.InternalWritePropertyName(name);
	}

	/// <summary>
	/// Writes the property name of a name/value pair of a JSON object.
	/// </summary>
	/// <param name="name">The name of the property.</param>
	/// <param name="escape">A flag to indicate whether the text should be escaped when it is written as a JSON property name.</param>
	public virtual void WritePropertyName(string name, bool escape)
	{
		this.WritePropertyName(name);
	}

	/// <summary>
	/// Writes the end of the current JSON object or array.
	/// </summary>
	public virtual void WriteEnd()
	{
		this.WriteEnd(this.Peek());
	}

	/// <summary>
	/// Writes the current <see cref="T:Newtonsoft.Json.JsonReader" /> token and its children.
	/// </summary>
	/// <param name="reader">The <see cref="T:Newtonsoft.Json.JsonReader" /> to read the token from.</param>
	public void WriteToken(JsonReader reader)
	{
		this.WriteToken(reader, writeChildren: true);
	}

	/// <summary>
	/// Writes the current <see cref="T:Newtonsoft.Json.JsonReader" /> token.
	/// </summary>
	/// <param name="reader">The <see cref="T:Newtonsoft.Json.JsonReader" /> to read the token from.</param>
	/// <param name="writeChildren">A flag indicating whether the current token's children should be written.</param>
	public void WriteToken(JsonReader reader, bool writeChildren)
	{
		ValidationUtils.ArgumentNotNull(reader, "reader");
		this.WriteToken(reader, writeChildren, writeDateConstructorAsDate: true, writeComments: true);
	}

	/// <summary>
	/// Writes the <see cref="T:Newtonsoft.Json.JsonToken" /> token and its value.
	/// </summary>
	/// <param name="token">The <see cref="T:Newtonsoft.Json.JsonToken" /> to write.</param>
	/// <param name="value">
	/// The value to write.
	/// A value is only required for tokens that have an associated value, e.g. the <see cref="T:System.String" /> property name for <see cref="F:Newtonsoft.Json.JsonToken.PropertyName" />.
	/// <c>null</c> can be passed to the method for tokens that don't have a value, e.g. <see cref="F:Newtonsoft.Json.JsonToken.StartObject" />.
	/// </param>
	public void WriteToken(JsonToken token, object? value)
	{
		switch (token)
		{
		case JsonToken.StartObject:
			this.WriteStartObject();
			break;
		case JsonToken.StartArray:
			this.WriteStartArray();
			break;
		case JsonToken.StartConstructor:
			ValidationUtils.ArgumentNotNull(value, "value");
			this.WriteStartConstructor(value.ToString());
			break;
		case JsonToken.PropertyName:
			ValidationUtils.ArgumentNotNull(value, "value");
			this.WritePropertyName(value.ToString());
			break;
		case JsonToken.Comment:
			this.WriteComment(value?.ToString());
			break;
		case JsonToken.Integer:
			ValidationUtils.ArgumentNotNull(value, "value");
			if (value is BigInteger bigInteger)
			{
				this.WriteValue(bigInteger);
			}
			else
			{
				this.WriteValue(Convert.ToInt64(value, CultureInfo.InvariantCulture));
			}
			break;
		case JsonToken.Float:
			ValidationUtils.ArgumentNotNull(value, "value");
			if (value is decimal value3)
			{
				this.WriteValue(value3);
			}
			else if (value is double value4)
			{
				this.WriteValue(value4);
			}
			else if (value is float value5)
			{
				this.WriteValue(value5);
			}
			else
			{
				this.WriteValue(Convert.ToDouble(value, CultureInfo.InvariantCulture));
			}
			break;
		case JsonToken.String:
			this.WriteValue(value?.ToString());
			break;
		case JsonToken.Boolean:
			ValidationUtils.ArgumentNotNull(value, "value");
			this.WriteValue(Convert.ToBoolean(value, CultureInfo.InvariantCulture));
			break;
		case JsonToken.Null:
			this.WriteNull();
			break;
		case JsonToken.Undefined:
			this.WriteUndefined();
			break;
		case JsonToken.EndObject:
			this.WriteEndObject();
			break;
		case JsonToken.EndArray:
			this.WriteEndArray();
			break;
		case JsonToken.EndConstructor:
			this.WriteEndConstructor();
			break;
		case JsonToken.Date:
			ValidationUtils.ArgumentNotNull(value, "value");
			if (value is DateTimeOffset value6)
			{
				this.WriteValue(value6);
			}
			else
			{
				this.WriteValue(Convert.ToDateTime(value, CultureInfo.InvariantCulture));
			}
			break;
		case JsonToken.Raw:
			this.WriteRawValue(value?.ToString());
			break;
		case JsonToken.Bytes:
			ValidationUtils.ArgumentNotNull(value, "value");
			if (value is Guid value2)
			{
				this.WriteValue(value2);
			}
			else
			{
				this.WriteValue((byte[])value);
			}
			break;
		default:
			throw MiscellaneousUtils.CreateArgumentOutOfRangeException("token", token, "Unexpected token type.");
		case JsonToken.None:
			break;
		}
	}

	/// <summary>
	/// Writes the <see cref="T:Newtonsoft.Json.JsonToken" /> token.
	/// </summary>
	/// <param name="token">The <see cref="T:Newtonsoft.Json.JsonToken" /> to write.</param>
	public void WriteToken(JsonToken token)
	{
		this.WriteToken(token, null);
	}

	internal virtual void WriteToken(JsonReader reader, bool writeChildren, bool writeDateConstructorAsDate, bool writeComments)
	{
		int num = this.CalculateWriteTokenInitialDepth(reader);
		do
		{
			if (writeDateConstructorAsDate && reader.TokenType == JsonToken.StartConstructor && string.Equals(reader.Value?.ToString(), "Date", StringComparison.Ordinal))
			{
				this.WriteConstructorDate(reader);
			}
			else if (writeComments || reader.TokenType != JsonToken.Comment)
			{
				this.WriteToken(reader.TokenType, reader.Value);
			}
		}
		while (num - 1 < reader.Depth - (JsonTokenUtils.IsEndToken(reader.TokenType) ? 1 : 0) && writeChildren && reader.Read());
		if (this.IsWriteTokenIncomplete(reader, writeChildren, num))
		{
			throw JsonWriterException.Create(this, "Unexpected end when reading token.", null);
		}
	}

	private bool IsWriteTokenIncomplete(JsonReader reader, bool writeChildren, int initialDepth)
	{
		int num = this.CalculateWriteTokenFinalDepth(reader);
		if (initialDepth >= num)
		{
			if (writeChildren && initialDepth == num)
			{
				return JsonTokenUtils.IsStartToken(reader.TokenType);
			}
			return false;
		}
		return true;
	}

	private int CalculateWriteTokenInitialDepth(JsonReader reader)
	{
		JsonToken tokenType = reader.TokenType;
		if (tokenType == JsonToken.None)
		{
			return -1;
		}
		if (!JsonTokenUtils.IsStartToken(tokenType))
		{
			return reader.Depth + 1;
		}
		return reader.Depth;
	}

	private int CalculateWriteTokenFinalDepth(JsonReader reader)
	{
		JsonToken tokenType = reader.TokenType;
		if (tokenType == JsonToken.None)
		{
			return -1;
		}
		if (!JsonTokenUtils.IsEndToken(tokenType))
		{
			return reader.Depth;
		}
		return reader.Depth - 1;
	}

	private void WriteConstructorDate(JsonReader reader)
	{
		if (!JavaScriptUtils.TryGetDateFromConstructorJson(reader, out DateTime dateTime, out string errorMessage))
		{
			throw JsonWriterException.Create(this, errorMessage, null);
		}
		this.WriteValue(dateTime);
	}

	private void WriteEnd(JsonContainerType type)
	{
		switch (type)
		{
		case JsonContainerType.Object:
			this.WriteEndObject();
			break;
		case JsonContainerType.Array:
			this.WriteEndArray();
			break;
		case JsonContainerType.Constructor:
			this.WriteEndConstructor();
			break;
		default:
			throw JsonWriterException.Create(this, "Unexpected type when writing end: " + type, null);
		}
	}

	private void AutoCompleteAll()
	{
		while (this.Top > 0)
		{
			this.WriteEnd();
		}
	}

	private JsonToken GetCloseTokenForType(JsonContainerType type)
	{
		return type switch
		{
			JsonContainerType.Object => JsonToken.EndObject, 
			JsonContainerType.Array => JsonToken.EndArray, 
			JsonContainerType.Constructor => JsonToken.EndConstructor, 
			_ => throw JsonWriterException.Create(this, "No close token for type: " + type, null), 
		};
	}

	private void AutoCompleteClose(JsonContainerType type)
	{
		int num = this.CalculateLevelsToComplete(type);
		for (int i = 0; i < num; i++)
		{
			JsonToken closeTokenForType = this.GetCloseTokenForType(this.Pop());
			if (this._currentState == State.Property)
			{
				this.WriteNull();
			}
			if (this._formatting == Formatting.Indented && this._currentState != State.ObjectStart && this._currentState != State.ArrayStart)
			{
				this.WriteIndent();
			}
			this.WriteEnd(closeTokenForType);
			this.UpdateCurrentState();
		}
	}

	private int CalculateLevelsToComplete(JsonContainerType type)
	{
		int num = 0;
		if (this._currentPosition.Type == type)
		{
			num = 1;
		}
		else
		{
			int num2 = this.Top - 2;
			for (int num3 = num2; num3 >= 0; num3--)
			{
				int index = num2 - num3;
				if (this._stack[index].Type == type)
				{
					num = num3 + 2;
					break;
				}
			}
		}
		if (num == 0)
		{
			throw JsonWriterException.Create(this, "No token to close.", null);
		}
		return num;
	}

	private void UpdateCurrentState()
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
			this._currentState = State.Array;
			break;
		case JsonContainerType.None:
			this._currentState = State.Start;
			break;
		default:
			throw JsonWriterException.Create(this, "Unknown JsonType: " + jsonContainerType, null);
		}
	}

	/// <summary>
	/// Writes the specified end token.
	/// </summary>
	/// <param name="token">The end token to write.</param>
	protected virtual void WriteEnd(JsonToken token)
	{
	}

	/// <summary>
	/// Writes indent characters.
	/// </summary>
	protected virtual void WriteIndent()
	{
	}

	/// <summary>
	/// Writes the JSON value delimiter.
	/// </summary>
	protected virtual void WriteValueDelimiter()
	{
	}

	/// <summary>
	/// Writes an indent space.
	/// </summary>
	protected virtual void WriteIndentSpace()
	{
	}

	internal void AutoComplete(JsonToken tokenBeingWritten)
	{
		State state = JsonWriter.StateArray[(int)tokenBeingWritten][(int)this._currentState];
		if (state == State.Error)
		{
			throw JsonWriterException.Create(this, "Token {0} in state {1} would result in an invalid JSON object.".FormatWith(CultureInfo.InvariantCulture, tokenBeingWritten.ToString(), this._currentState.ToString()), null);
		}
		if ((this._currentState == State.Object || this._currentState == State.Array || this._currentState == State.Constructor) && tokenBeingWritten != JsonToken.Comment)
		{
			this.WriteValueDelimiter();
		}
		if (this._formatting == Formatting.Indented)
		{
			if (this._currentState == State.Property)
			{
				this.WriteIndentSpace();
			}
			if (this._currentState == State.Array || this._currentState == State.ArrayStart || this._currentState == State.Constructor || this._currentState == State.ConstructorStart || (tokenBeingWritten == JsonToken.PropertyName && this._currentState != State.Start))
			{
				this.WriteIndent();
			}
		}
		this._currentState = state;
	}

	/// <summary>
	/// Writes a null value.
	/// </summary>
	public virtual void WriteNull()
	{
		this.InternalWriteValue(JsonToken.Null);
	}

	/// <summary>
	/// Writes an undefined value.
	/// </summary>
	public virtual void WriteUndefined()
	{
		this.InternalWriteValue(JsonToken.Undefined);
	}

	/// <summary>
	/// Writes raw JSON without changing the writer's state.
	/// </summary>
	/// <param name="json">The raw JSON to write.</param>
	public virtual void WriteRaw(string? json)
	{
		this.InternalWriteRaw();
	}

	/// <summary>
	/// Writes raw JSON where a value is expected and updates the writer's state.
	/// </summary>
	/// <param name="json">The raw JSON to write.</param>
	public virtual void WriteRawValue(string? json)
	{
		this.UpdateScopeWithFinishedValue();
		this.AutoComplete(JsonToken.Undefined);
		this.WriteRaw(json);
	}

	/// <summary>
	/// Writes a <see cref="T:System.String" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.String" /> value to write.</param>
	public virtual void WriteValue(string? value)
	{
		this.InternalWriteValue(JsonToken.String);
	}

	/// <summary>
	/// Writes a <see cref="T:System.Int32" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Int32" /> value to write.</param>
	public virtual void WriteValue(int value)
	{
		this.InternalWriteValue(JsonToken.Integer);
	}

	/// <summary>
	/// Writes a <see cref="T:System.UInt32" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.UInt32" /> value to write.</param>
	[CLSCompliant(false)]
	public virtual void WriteValue(uint value)
	{
		this.InternalWriteValue(JsonToken.Integer);
	}

	/// <summary>
	/// Writes a <see cref="T:System.Int64" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Int64" /> value to write.</param>
	public virtual void WriteValue(long value)
	{
		this.InternalWriteValue(JsonToken.Integer);
	}

	/// <summary>
	/// Writes a <see cref="T:System.UInt64" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.UInt64" /> value to write.</param>
	[CLSCompliant(false)]
	public virtual void WriteValue(ulong value)
	{
		this.InternalWriteValue(JsonToken.Integer);
	}

	/// <summary>
	/// Writes a <see cref="T:System.Single" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Single" /> value to write.</param>
	public virtual void WriteValue(float value)
	{
		this.InternalWriteValue(JsonToken.Float);
	}

	/// <summary>
	/// Writes a <see cref="T:System.Double" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Double" /> value to write.</param>
	public virtual void WriteValue(double value)
	{
		this.InternalWriteValue(JsonToken.Float);
	}

	/// <summary>
	/// Writes a <see cref="T:System.Boolean" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Boolean" /> value to write.</param>
	public virtual void WriteValue(bool value)
	{
		this.InternalWriteValue(JsonToken.Boolean);
	}

	/// <summary>
	/// Writes a <see cref="T:System.Int16" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Int16" /> value to write.</param>
	public virtual void WriteValue(short value)
	{
		this.InternalWriteValue(JsonToken.Integer);
	}

	/// <summary>
	/// Writes a <see cref="T:System.UInt16" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.UInt16" /> value to write.</param>
	[CLSCompliant(false)]
	public virtual void WriteValue(ushort value)
	{
		this.InternalWriteValue(JsonToken.Integer);
	}

	/// <summary>
	/// Writes a <see cref="T:System.Char" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Char" /> value to write.</param>
	public virtual void WriteValue(char value)
	{
		this.InternalWriteValue(JsonToken.String);
	}

	/// <summary>
	/// Writes a <see cref="T:System.Byte" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Byte" /> value to write.</param>
	public virtual void WriteValue(byte value)
	{
		this.InternalWriteValue(JsonToken.Integer);
	}

	/// <summary>
	/// Writes a <see cref="T:System.SByte" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.SByte" /> value to write.</param>
	[CLSCompliant(false)]
	public virtual void WriteValue(sbyte value)
	{
		this.InternalWriteValue(JsonToken.Integer);
	}

	/// <summary>
	/// Writes a <see cref="T:System.Decimal" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Decimal" /> value to write.</param>
	public virtual void WriteValue(decimal value)
	{
		this.InternalWriteValue(JsonToken.Float);
	}

	/// <summary>
	/// Writes a <see cref="T:System.DateTime" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.DateTime" /> value to write.</param>
	public virtual void WriteValue(DateTime value)
	{
		this.InternalWriteValue(JsonToken.Date);
	}

	/// <summary>
	/// Writes a <see cref="T:System.DateTimeOffset" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.DateTimeOffset" /> value to write.</param>
	public virtual void WriteValue(DateTimeOffset value)
	{
		this.InternalWriteValue(JsonToken.Date);
	}

	/// <summary>
	/// Writes a <see cref="T:System.Guid" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Guid" /> value to write.</param>
	public virtual void WriteValue(Guid value)
	{
		this.InternalWriteValue(JsonToken.String);
	}

	/// <summary>
	/// Writes a <see cref="T:System.TimeSpan" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.TimeSpan" /> value to write.</param>
	public virtual void WriteValue(TimeSpan value)
	{
		this.InternalWriteValue(JsonToken.String);
	}

	/// <summary>
	/// Writes a <see cref="T:System.Nullable`1" /> of <see cref="T:System.Int32" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Nullable`1" /> of <see cref="T:System.Int32" /> value to write.</param>
	public virtual void WriteValue(int? value)
	{
		if (!value.HasValue)
		{
			this.WriteNull();
		}
		else
		{
			this.WriteValue(value.GetValueOrDefault());
		}
	}

	/// <summary>
	/// Writes a <see cref="T:System.Nullable`1" /> of <see cref="T:System.UInt32" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Nullable`1" /> of <see cref="T:System.UInt32" /> value to write.</param>
	[CLSCompliant(false)]
	public virtual void WriteValue(uint? value)
	{
		if (!value.HasValue)
		{
			this.WriteNull();
		}
		else
		{
			this.WriteValue(value.GetValueOrDefault());
		}
	}

	/// <summary>
	/// Writes a <see cref="T:System.Nullable`1" /> of <see cref="T:System.Int64" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Nullable`1" /> of <see cref="T:System.Int64" /> value to write.</param>
	public virtual void WriteValue(long? value)
	{
		if (!value.HasValue)
		{
			this.WriteNull();
		}
		else
		{
			this.WriteValue(value.GetValueOrDefault());
		}
	}

	/// <summary>
	/// Writes a <see cref="T:System.Nullable`1" /> of <see cref="T:System.UInt64" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Nullable`1" /> of <see cref="T:System.UInt64" /> value to write.</param>
	[CLSCompliant(false)]
	public virtual void WriteValue(ulong? value)
	{
		if (!value.HasValue)
		{
			this.WriteNull();
		}
		else
		{
			this.WriteValue(value.GetValueOrDefault());
		}
	}

	/// <summary>
	/// Writes a <see cref="T:System.Nullable`1" /> of <see cref="T:System.Single" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Nullable`1" /> of <see cref="T:System.Single" /> value to write.</param>
	public virtual void WriteValue(float? value)
	{
		if (!value.HasValue)
		{
			this.WriteNull();
		}
		else
		{
			this.WriteValue(value.GetValueOrDefault());
		}
	}

	/// <summary>
	/// Writes a <see cref="T:System.Nullable`1" /> of <see cref="T:System.Double" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Nullable`1" /> of <see cref="T:System.Double" /> value to write.</param>
	public virtual void WriteValue(double? value)
	{
		if (!value.HasValue)
		{
			this.WriteNull();
		}
		else
		{
			this.WriteValue(value.GetValueOrDefault());
		}
	}

	/// <summary>
	/// Writes a <see cref="T:System.Nullable`1" /> of <see cref="T:System.Boolean" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Nullable`1" /> of <see cref="T:System.Boolean" /> value to write.</param>
	public virtual void WriteValue(bool? value)
	{
		if (!value.HasValue)
		{
			this.WriteNull();
		}
		else
		{
			this.WriteValue(value == true);
		}
	}

	/// <summary>
	/// Writes a <see cref="T:System.Nullable`1" /> of <see cref="T:System.Int16" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Nullable`1" /> of <see cref="T:System.Int16" /> value to write.</param>
	public virtual void WriteValue(short? value)
	{
		if (!value.HasValue)
		{
			this.WriteNull();
		}
		else
		{
			this.WriteValue(value.GetValueOrDefault());
		}
	}

	/// <summary>
	/// Writes a <see cref="T:System.Nullable`1" /> of <see cref="T:System.UInt16" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Nullable`1" /> of <see cref="T:System.UInt16" /> value to write.</param>
	[CLSCompliant(false)]
	public virtual void WriteValue(ushort? value)
	{
		if (!value.HasValue)
		{
			this.WriteNull();
		}
		else
		{
			this.WriteValue(value.GetValueOrDefault());
		}
	}

	/// <summary>
	/// Writes a <see cref="T:System.Nullable`1" /> of <see cref="T:System.Char" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Nullable`1" /> of <see cref="T:System.Char" /> value to write.</param>
	public virtual void WriteValue(char? value)
	{
		if (!value.HasValue)
		{
			this.WriteNull();
		}
		else
		{
			this.WriteValue(value.GetValueOrDefault());
		}
	}

	/// <summary>
	/// Writes a <see cref="T:System.Nullable`1" /> of <see cref="T:System.Byte" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Nullable`1" /> of <see cref="T:System.Byte" /> value to write.</param>
	public virtual void WriteValue(byte? value)
	{
		if (!value.HasValue)
		{
			this.WriteNull();
		}
		else
		{
			this.WriteValue(value.GetValueOrDefault());
		}
	}

	/// <summary>
	/// Writes a <see cref="T:System.Nullable`1" /> of <see cref="T:System.SByte" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Nullable`1" /> of <see cref="T:System.SByte" /> value to write.</param>
	[CLSCompliant(false)]
	public virtual void WriteValue(sbyte? value)
	{
		if (!value.HasValue)
		{
			this.WriteNull();
		}
		else
		{
			this.WriteValue(value.GetValueOrDefault());
		}
	}

	/// <summary>
	/// Writes a <see cref="T:System.Nullable`1" /> of <see cref="T:System.Decimal" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Nullable`1" /> of <see cref="T:System.Decimal" /> value to write.</param>
	public virtual void WriteValue(decimal? value)
	{
		if (!value.HasValue)
		{
			this.WriteNull();
		}
		else
		{
			this.WriteValue(value.GetValueOrDefault());
		}
	}

	/// <summary>
	/// Writes a <see cref="T:System.Nullable`1" /> of <see cref="T:System.DateTime" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Nullable`1" /> of <see cref="T:System.DateTime" /> value to write.</param>
	public virtual void WriteValue(DateTime? value)
	{
		if (!value.HasValue)
		{
			this.WriteNull();
		}
		else
		{
			this.WriteValue(value.GetValueOrDefault());
		}
	}

	/// <summary>
	/// Writes a <see cref="T:System.Nullable`1" /> of <see cref="T:System.DateTimeOffset" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Nullable`1" /> of <see cref="T:System.DateTimeOffset" /> value to write.</param>
	public virtual void WriteValue(DateTimeOffset? value)
	{
		if (!value.HasValue)
		{
			this.WriteNull();
		}
		else
		{
			this.WriteValue(value.GetValueOrDefault());
		}
	}

	/// <summary>
	/// Writes a <see cref="T:System.Nullable`1" /> of <see cref="T:System.Guid" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Nullable`1" /> of <see cref="T:System.Guid" /> value to write.</param>
	public virtual void WriteValue(Guid? value)
	{
		if (!value.HasValue)
		{
			this.WriteNull();
		}
		else
		{
			this.WriteValue(value.GetValueOrDefault());
		}
	}

	/// <summary>
	/// Writes a <see cref="T:System.Nullable`1" /> of <see cref="T:System.TimeSpan" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Nullable`1" /> of <see cref="T:System.TimeSpan" /> value to write.</param>
	public virtual void WriteValue(TimeSpan? value)
	{
		if (!value.HasValue)
		{
			this.WriteNull();
		}
		else
		{
			this.WriteValue(value.GetValueOrDefault());
		}
	}

	/// <summary>
	/// Writes a <see cref="T:System.Byte" />[] value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Byte" />[] value to write.</param>
	public virtual void WriteValue(byte[]? value)
	{
		if (value == null)
		{
			this.WriteNull();
		}
		else
		{
			this.InternalWriteValue(JsonToken.Bytes);
		}
	}

	/// <summary>
	/// Writes a <see cref="T:System.Uri" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Uri" /> value to write.</param>
	public virtual void WriteValue(Uri? value)
	{
		if (value == null)
		{
			this.WriteNull();
		}
		else
		{
			this.InternalWriteValue(JsonToken.String);
		}
	}

	/// <summary>
	/// Writes a <see cref="T:System.Object" /> value.
	/// An error will raised if the value cannot be written as a single JSON token.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Object" /> value to write.</param>
	public virtual void WriteValue(object? value)
	{
		if (value == null)
		{
			this.WriteNull();
			return;
		}
		if (value is BigInteger)
		{
			throw JsonWriter.CreateUnsupportedTypeException(this, value);
		}
		JsonWriter.WriteValue(this, ConvertUtils.GetTypeCode(value.GetType()), value);
	}

	/// <summary>
	/// Writes a comment <c>/*...*/</c> containing the specified text.
	/// </summary>
	/// <param name="text">Text to place inside the comment.</param>
	public virtual void WriteComment(string? text)
	{
		this.InternalWriteComment();
	}

	/// <summary>
	/// Writes the given white space.
	/// </summary>
	/// <param name="ws">The string of white space characters.</param>
	public virtual void WriteWhitespace(string ws)
	{
		this.InternalWriteWhitespace(ws);
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

	internal static void WriteValue(JsonWriter writer, PrimitiveTypeCode typeCode, object value)
	{
		while (true)
		{
			switch (typeCode)
			{
			case PrimitiveTypeCode.Char:
				writer.WriteValue((char)value);
				return;
			case PrimitiveTypeCode.CharNullable:
				writer.WriteValue((value == null) ? ((char?)null) : new char?((char)value));
				return;
			case PrimitiveTypeCode.Boolean:
				writer.WriteValue((bool)value);
				return;
			case PrimitiveTypeCode.BooleanNullable:
				writer.WriteValue((value == null) ? ((bool?)null) : new bool?((bool)value));
				return;
			case PrimitiveTypeCode.SByte:
				writer.WriteValue((sbyte)value);
				return;
			case PrimitiveTypeCode.SByteNullable:
				writer.WriteValue((value == null) ? ((sbyte?)null) : new sbyte?((sbyte)value));
				return;
			case PrimitiveTypeCode.Int16:
				writer.WriteValue((short)value);
				return;
			case PrimitiveTypeCode.Int16Nullable:
				writer.WriteValue((value == null) ? ((short?)null) : new short?((short)value));
				return;
			case PrimitiveTypeCode.UInt16:
				writer.WriteValue((ushort)value);
				return;
			case PrimitiveTypeCode.UInt16Nullable:
				writer.WriteValue((value == null) ? ((ushort?)null) : new ushort?((ushort)value));
				return;
			case PrimitiveTypeCode.Int32:
				writer.WriteValue((int)value);
				return;
			case PrimitiveTypeCode.Int32Nullable:
				writer.WriteValue((value == null) ? ((int?)null) : new int?((int)value));
				return;
			case PrimitiveTypeCode.Byte:
				writer.WriteValue((byte)value);
				return;
			case PrimitiveTypeCode.ByteNullable:
				writer.WriteValue((value == null) ? ((byte?)null) : new byte?((byte)value));
				return;
			case PrimitiveTypeCode.UInt32:
				writer.WriteValue((uint)value);
				return;
			case PrimitiveTypeCode.UInt32Nullable:
				writer.WriteValue((value == null) ? ((uint?)null) : new uint?((uint)value));
				return;
			case PrimitiveTypeCode.Int64:
				writer.WriteValue((long)value);
				return;
			case PrimitiveTypeCode.Int64Nullable:
				writer.WriteValue((value == null) ? ((long?)null) : new long?((long)value));
				return;
			case PrimitiveTypeCode.UInt64:
				writer.WriteValue((ulong)value);
				return;
			case PrimitiveTypeCode.UInt64Nullable:
				writer.WriteValue((value == null) ? ((ulong?)null) : new ulong?((ulong)value));
				return;
			case PrimitiveTypeCode.Single:
				writer.WriteValue((float)value);
				return;
			case PrimitiveTypeCode.SingleNullable:
				writer.WriteValue((value == null) ? ((float?)null) : new float?((float)value));
				return;
			case PrimitiveTypeCode.Double:
				writer.WriteValue((double)value);
				return;
			case PrimitiveTypeCode.DoubleNullable:
				writer.WriteValue((value == null) ? ((double?)null) : new double?((double)value));
				return;
			case PrimitiveTypeCode.DateTime:
				writer.WriteValue((DateTime)value);
				return;
			case PrimitiveTypeCode.DateTimeNullable:
				writer.WriteValue((value == null) ? ((DateTime?)null) : new DateTime?((DateTime)value));
				return;
			case PrimitiveTypeCode.DateTimeOffset:
				writer.WriteValue((DateTimeOffset)value);
				return;
			case PrimitiveTypeCode.DateTimeOffsetNullable:
				writer.WriteValue((value == null) ? ((DateTimeOffset?)null) : new DateTimeOffset?((DateTimeOffset)value));
				return;
			case PrimitiveTypeCode.Decimal:
				writer.WriteValue((decimal)value);
				return;
			case PrimitiveTypeCode.DecimalNullable:
				writer.WriteValue((value == null) ? ((decimal?)null) : new decimal?((decimal)value));
				return;
			case PrimitiveTypeCode.Guid:
				writer.WriteValue((Guid)value);
				return;
			case PrimitiveTypeCode.GuidNullable:
				writer.WriteValue((value == null) ? ((Guid?)null) : new Guid?((Guid)value));
				return;
			case PrimitiveTypeCode.TimeSpan:
				writer.WriteValue((TimeSpan)value);
				return;
			case PrimitiveTypeCode.TimeSpanNullable:
				writer.WriteValue((value == null) ? ((TimeSpan?)null) : new TimeSpan?((TimeSpan)value));
				return;
			case PrimitiveTypeCode.BigInteger:
				writer.WriteValue((BigInteger)value);
				return;
			case PrimitiveTypeCode.BigIntegerNullable:
				writer.WriteValue((value == null) ? ((BigInteger?)null) : new BigInteger?((BigInteger)value));
				return;
			case PrimitiveTypeCode.Uri:
				writer.WriteValue((Uri)value);
				return;
			case PrimitiveTypeCode.String:
				writer.WriteValue((string)value);
				return;
			case PrimitiveTypeCode.Bytes:
				writer.WriteValue((byte[])value);
				return;
			case PrimitiveTypeCode.DBNull:
				writer.WriteNull();
				return;
			}
			if (value is IConvertible convertible)
			{
				JsonWriter.ResolveConvertibleValue(convertible, out typeCode, out value);
				continue;
			}
			if (value == null)
			{
				writer.WriteNull();
				return;
			}
			throw JsonWriter.CreateUnsupportedTypeException(writer, value);
		}
	}

	private static void ResolveConvertibleValue(IConvertible convertible, out PrimitiveTypeCode typeCode, out object value)
	{
		TypeInformation typeInformation = ConvertUtils.GetTypeInformation(convertible);
		typeCode = ((typeInformation.TypeCode == PrimitiveTypeCode.Object) ? PrimitiveTypeCode.String : typeInformation.TypeCode);
		Type conversionType = ((typeInformation.TypeCode == PrimitiveTypeCode.Object) ? typeof(string) : typeInformation.Type);
		value = convertible.ToType(conversionType, CultureInfo.InvariantCulture);
	}

	private static JsonWriterException CreateUnsupportedTypeException(JsonWriter writer, object value)
	{
		return JsonWriterException.Create(writer, "Unsupported type: {0}. Use the JsonSerializer class to get the object's JSON representation.".FormatWith(CultureInfo.InvariantCulture, value.GetType()), null);
	}

	/// <summary>
	/// Sets the state of the <see cref="T:Newtonsoft.Json.JsonWriter" />.
	/// </summary>
	/// <param name="token">The <see cref="T:Newtonsoft.Json.JsonToken" /> being written.</param>
	/// <param name="value">The value being written.</param>
	protected void SetWriteState(JsonToken token, object value)
	{
		switch (token)
		{
		case JsonToken.StartObject:
			this.InternalWriteStart(token, JsonContainerType.Object);
			break;
		case JsonToken.StartArray:
			this.InternalWriteStart(token, JsonContainerType.Array);
			break;
		case JsonToken.StartConstructor:
			this.InternalWriteStart(token, JsonContainerType.Constructor);
			break;
		case JsonToken.PropertyName:
			if (!(value is string name))
			{
				throw new ArgumentException("A name is required when setting property name state.", "value");
			}
			this.InternalWritePropertyName(name);
			break;
		case JsonToken.Comment:
			this.InternalWriteComment();
			break;
		case JsonToken.Raw:
			this.InternalWriteRaw();
			break;
		case JsonToken.Integer:
		case JsonToken.Float:
		case JsonToken.String:
		case JsonToken.Boolean:
		case JsonToken.Null:
		case JsonToken.Undefined:
		case JsonToken.Date:
		case JsonToken.Bytes:
			this.InternalWriteValue(token);
			break;
		case JsonToken.EndObject:
			this.InternalWriteEnd(JsonContainerType.Object);
			break;
		case JsonToken.EndArray:
			this.InternalWriteEnd(JsonContainerType.Array);
			break;
		case JsonToken.EndConstructor:
			this.InternalWriteEnd(JsonContainerType.Constructor);
			break;
		default:
			throw new ArgumentOutOfRangeException("token");
		}
	}

	internal void InternalWriteEnd(JsonContainerType container)
	{
		this.AutoCompleteClose(container);
	}

	internal void InternalWritePropertyName(string name)
	{
		this._currentPosition.PropertyName = name;
		this.AutoComplete(JsonToken.PropertyName);
	}

	internal void InternalWriteRaw()
	{
	}

	internal void InternalWriteStart(JsonToken token, JsonContainerType container)
	{
		this.UpdateScopeWithFinishedValue();
		this.AutoComplete(token);
		this.Push(container);
	}

	internal void InternalWriteValue(JsonToken token)
	{
		this.UpdateScopeWithFinishedValue();
		this.AutoComplete(token);
	}

	internal void InternalWriteWhitespace(string ws)
	{
		if (ws != null && !StringUtils.IsWhiteSpace(ws))
		{
			throw JsonWriterException.Create(this, "Only white space characters should be used.", null);
		}
	}

	internal void InternalWriteComment()
	{
		this.AutoComplete(JsonToken.Comment);
	}
}
