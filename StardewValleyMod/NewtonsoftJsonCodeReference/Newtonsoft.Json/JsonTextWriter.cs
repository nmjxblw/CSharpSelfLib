using System;
using System.Globalization;
using System.IO;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json;

/// <summary>
/// Represents a writer that provides a fast, non-cached, forward-only way of generating JSON data.
/// </summary>
public class JsonTextWriter : JsonWriter
{
	private readonly bool _safeAsync;

	private const int IndentCharBufferSize = 12;

	private readonly TextWriter _writer;

	private Base64Encoder? _base64Encoder;

	private char _indentChar;

	private int _indentation;

	private char _quoteChar;

	private bool _quoteName;

	private bool[]? _charEscapeFlags;

	private char[]? _writeBuffer;

	private IArrayPool<char>? _arrayPool;

	private char[]? _indentChars;

	private Base64Encoder Base64Encoder
	{
		get
		{
			if (this._base64Encoder == null)
			{
				this._base64Encoder = new Base64Encoder(this._writer);
			}
			return this._base64Encoder;
		}
	}

	/// <summary>
	/// Gets or sets the writer's character array pool.
	/// </summary>
	public IArrayPool<char>? ArrayPool
	{
		get
		{
			return this._arrayPool;
		}
		set
		{
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			this._arrayPool = value;
		}
	}

	/// <summary>
	/// Gets or sets how many <see cref="P:Newtonsoft.Json.JsonTextWriter.IndentChar" />s to write for each level in the hierarchy when <see cref="P:Newtonsoft.Json.JsonWriter.Formatting" /> is set to <see cref="F:Newtonsoft.Json.Formatting.Indented" />.
	/// </summary>
	public int Indentation
	{
		get
		{
			return this._indentation;
		}
		set
		{
			if (value < 0)
			{
				throw new ArgumentException("Indentation value must be greater than 0.");
			}
			this._indentation = value;
		}
	}

	/// <summary>
	/// Gets or sets which character to use to quote attribute values.
	/// </summary>
	public char QuoteChar
	{
		get
		{
			return this._quoteChar;
		}
		set
		{
			if (value != '"' && value != '\'')
			{
				throw new ArgumentException("Invalid JavaScript string quote character. Valid quote characters are ' and \".");
			}
			this._quoteChar = value;
			this.UpdateCharEscapeFlags();
		}
	}

	/// <summary>
	/// Gets or sets which character to use for indenting when <see cref="P:Newtonsoft.Json.JsonWriter.Formatting" /> is set to <see cref="F:Newtonsoft.Json.Formatting.Indented" />.
	/// </summary>
	public char IndentChar
	{
		get
		{
			return this._indentChar;
		}
		set
		{
			if (value != this._indentChar)
			{
				this._indentChar = value;
				this._indentChars = null;
			}
		}
	}

	/// <summary>
	/// Gets or sets a value indicating whether object names will be surrounded with quotes.
	/// </summary>
	public bool QuoteName
	{
		get
		{
			return this._quoteName;
		}
		set
		{
			this._quoteName = value;
		}
	}

	/// <summary>
	/// Asynchronously flushes whatever is in the buffer to the destination and also flushes the destination.
	/// </summary>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
	/// execute synchronously, returning an already-completed task.</remarks>
	public override Task FlushAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!this._safeAsync)
		{
			return base.FlushAsync(cancellationToken);
		}
		return this.DoFlushAsync(cancellationToken);
	}

	internal Task DoFlushAsync(CancellationToken cancellationToken)
	{
		return cancellationToken.CancelIfRequestedAsync() ?? this._writer.FlushAsync();
	}

	/// <summary>
	/// Asynchronously writes the JSON value delimiter.
	/// </summary>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
	/// execute synchronously, returning an already-completed task.</remarks>
	protected override Task WriteValueDelimiterAsync(CancellationToken cancellationToken)
	{
		if (!this._safeAsync)
		{
			return base.WriteValueDelimiterAsync(cancellationToken);
		}
		return this.DoWriteValueDelimiterAsync(cancellationToken);
	}

	internal Task DoWriteValueDelimiterAsync(CancellationToken cancellationToken)
	{
		return this._writer.WriteAsync(',', cancellationToken);
	}

	/// <summary>
	/// Asynchronously writes the specified end token.
	/// </summary>
	/// <param name="token">The end token to write.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
	/// execute synchronously, returning an already-completed task.</remarks>
	protected override Task WriteEndAsync(JsonToken token, CancellationToken cancellationToken)
	{
		if (!this._safeAsync)
		{
			return base.WriteEndAsync(token, cancellationToken);
		}
		return this.DoWriteEndAsync(token, cancellationToken);
	}

	internal Task DoWriteEndAsync(JsonToken token, CancellationToken cancellationToken)
	{
		return token switch
		{
			JsonToken.EndObject => this._writer.WriteAsync('}', cancellationToken), 
			JsonToken.EndArray => this._writer.WriteAsync(']', cancellationToken), 
			JsonToken.EndConstructor => this._writer.WriteAsync(')', cancellationToken), 
			_ => throw JsonWriterException.Create(this, "Invalid JsonToken: " + token, null), 
		};
	}

	/// <summary>
	/// Asynchronously closes this writer.
	/// If <see cref="P:Newtonsoft.Json.JsonWriter.CloseOutput" /> is set to <c>true</c>, the destination is also closed.
	/// </summary>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
	/// execute synchronously, returning an already-completed task.</remarks>
	public override Task CloseAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!this._safeAsync)
		{
			return base.CloseAsync(cancellationToken);
		}
		return this.DoCloseAsync(cancellationToken);
	}

	internal async Task DoCloseAsync(CancellationToken cancellationToken)
	{
		if (base.Top == 0)
		{
			cancellationToken.ThrowIfCancellationRequested();
		}
		while (base.Top > 0)
		{
			await this.WriteEndAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		await this.CloseBufferAndWriterAsync().ConfigureAwait(continueOnCapturedContext: false);
	}

	private async Task CloseBufferAndWriterAsync()
	{
		if (this._writeBuffer != null)
		{
			BufferUtils.ReturnBuffer(this._arrayPool, this._writeBuffer);
			this._writeBuffer = null;
		}
		if (base.CloseOutput && this._writer != null)
		{
			await this._writer.DisposeAsync().ConfigureAwait(continueOnCapturedContext: false);
		}
	}

	/// <summary>
	/// Asynchronously writes the end of the current JSON object or array.
	/// </summary>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
	/// execute synchronously, returning an already-completed task.</remarks>
	public override Task WriteEndAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!this._safeAsync)
		{
			return base.WriteEndAsync(cancellationToken);
		}
		return base.WriteEndInternalAsync(cancellationToken);
	}

	/// <summary>
	/// Asynchronously writes indent characters.
	/// </summary>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
	/// execute synchronously, returning an already-completed task.</remarks>
	protected override Task WriteIndentAsync(CancellationToken cancellationToken)
	{
		if (!this._safeAsync)
		{
			return base.WriteIndentAsync(cancellationToken);
		}
		return this.DoWriteIndentAsync(cancellationToken);
	}

	internal Task DoWriteIndentAsync(CancellationToken cancellationToken)
	{
		int num = base.Top * this._indentation;
		int num2 = this.SetIndentChars();
		if (num <= 12)
		{
			return this._writer.WriteAsync(this._indentChars, 0, num2 + num, cancellationToken);
		}
		return this.WriteIndentAsync(num, num2, cancellationToken);
	}

	private async Task WriteIndentAsync(int currentIndentCount, int newLineLen, CancellationToken cancellationToken)
	{
		await this._writer.WriteAsync(this._indentChars, 0, newLineLen + Math.Min(currentIndentCount, 12), cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		while (true)
		{
			int num;
			currentIndentCount = (num = currentIndentCount - 12);
			if (num <= 0)
			{
				break;
			}
			await this._writer.WriteAsync(this._indentChars, newLineLen, Math.Min(currentIndentCount, 12), cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
	}

	private Task WriteValueInternalAsync(JsonToken token, string value, CancellationToken cancellationToken)
	{
		Task task = base.InternalWriteValueAsync(token, cancellationToken);
		if (task.IsCompletedSuccessfully())
		{
			return this._writer.WriteAsync(value, cancellationToken);
		}
		return this.WriteValueInternalAsync(task, value, cancellationToken);
	}

	private async Task WriteValueInternalAsync(Task task, string value, CancellationToken cancellationToken)
	{
		await task.ConfigureAwait(continueOnCapturedContext: false);
		await this._writer.WriteAsync(value, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	/// <summary>
	/// Asynchronously writes an indent space.
	/// </summary>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
	/// execute synchronously, returning an already-completed task.</remarks>
	protected override Task WriteIndentSpaceAsync(CancellationToken cancellationToken)
	{
		if (!this._safeAsync)
		{
			return base.WriteIndentSpaceAsync(cancellationToken);
		}
		return this.DoWriteIndentSpaceAsync(cancellationToken);
	}

	internal Task DoWriteIndentSpaceAsync(CancellationToken cancellationToken)
	{
		return this._writer.WriteAsync(' ', cancellationToken);
	}

	/// <summary>
	/// Asynchronously writes raw JSON without changing the writer's state.
	/// </summary>
	/// <param name="json">The raw JSON to write.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
	/// execute synchronously, returning an already-completed task.</remarks>
	public override Task WriteRawAsync(string? json, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!this._safeAsync)
		{
			return base.WriteRawAsync(json, cancellationToken);
		}
		return this.DoWriteRawAsync(json, cancellationToken);
	}

	internal Task DoWriteRawAsync(string? json, CancellationToken cancellationToken)
	{
		return this._writer.WriteAsync(json, cancellationToken);
	}

	/// <summary>
	/// Asynchronously writes a null value.
	/// </summary>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
	/// execute synchronously, returning an already-completed task.</remarks>
	public override Task WriteNullAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!this._safeAsync)
		{
			return base.WriteNullAsync(cancellationToken);
		}
		return this.DoWriteNullAsync(cancellationToken);
	}

	internal Task DoWriteNullAsync(CancellationToken cancellationToken)
	{
		return this.WriteValueInternalAsync(JsonToken.Null, JsonConvert.Null, cancellationToken);
	}

	private Task WriteDigitsAsync(ulong uvalue, bool negative, CancellationToken cancellationToken)
	{
		if (uvalue <= 9 && !negative)
		{
			return this._writer.WriteAsync((char)(48 + uvalue), cancellationToken);
		}
		int count = this.WriteNumberToBuffer(uvalue, negative);
		return this._writer.WriteAsync(this._writeBuffer, 0, count, cancellationToken);
	}

	private Task WriteIntegerValueAsync(ulong uvalue, bool negative, CancellationToken cancellationToken)
	{
		Task task = base.InternalWriteValueAsync(JsonToken.Integer, cancellationToken);
		if (task.IsCompletedSuccessfully())
		{
			return this.WriteDigitsAsync(uvalue, negative, cancellationToken);
		}
		return this.WriteIntegerValueAsync(task, uvalue, negative, cancellationToken);
	}

	private async Task WriteIntegerValueAsync(Task task, ulong uvalue, bool negative, CancellationToken cancellationToken)
	{
		await task.ConfigureAwait(continueOnCapturedContext: false);
		await this.WriteDigitsAsync(uvalue, negative, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	internal Task WriteIntegerValueAsync(long value, CancellationToken cancellationToken)
	{
		bool flag = value < 0;
		if (flag)
		{
			value = -value;
		}
		return this.WriteIntegerValueAsync((ulong)value, flag, cancellationToken);
	}

	internal Task WriteIntegerValueAsync(ulong uvalue, CancellationToken cancellationToken)
	{
		return this.WriteIntegerValueAsync(uvalue, negative: false, cancellationToken);
	}

	private Task WriteEscapedStringAsync(string value, bool quote, CancellationToken cancellationToken)
	{
		return JavaScriptUtils.WriteEscapedJavaScriptStringAsync(this._writer, value, this._quoteChar, quote, this._charEscapeFlags, base.StringEscapeHandling, this, this._writeBuffer, cancellationToken);
	}

	/// <summary>
	/// Asynchronously writes the property name of a name/value pair of a JSON object.
	/// </summary>
	/// <param name="name">The name of the property.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
	/// execute synchronously, returning an already-completed task.</remarks>
	public override Task WritePropertyNameAsync(string name, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!this._safeAsync)
		{
			return base.WritePropertyNameAsync(name, cancellationToken);
		}
		return this.DoWritePropertyNameAsync(name, cancellationToken);
	}

	internal Task DoWritePropertyNameAsync(string name, CancellationToken cancellationToken)
	{
		Task task = base.InternalWritePropertyNameAsync(name, cancellationToken);
		if (!task.IsCompletedSuccessfully())
		{
			return this.DoWritePropertyNameAsync(task, name, cancellationToken);
		}
		task = this.WriteEscapedStringAsync(name, this._quoteName, cancellationToken);
		if (task.IsCompletedSuccessfully())
		{
			return this._writer.WriteAsync(':', cancellationToken);
		}
		return JavaScriptUtils.WriteCharAsync(task, this._writer, ':', cancellationToken);
	}

	private async Task DoWritePropertyNameAsync(Task task, string name, CancellationToken cancellationToken)
	{
		await task.ConfigureAwait(continueOnCapturedContext: false);
		await this.WriteEscapedStringAsync(name, this._quoteName, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		await this._writer.WriteAsync(':').ConfigureAwait(continueOnCapturedContext: false);
	}

	/// <summary>
	/// Asynchronously writes the property name of a name/value pair of a JSON object.
	/// </summary>
	/// <param name="name">The name of the property.</param>
	/// <param name="escape">A flag to indicate whether the text should be escaped when it is written as a JSON property name.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
	/// execute synchronously, returning an already-completed task.</remarks>
	public override Task WritePropertyNameAsync(string name, bool escape, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!this._safeAsync)
		{
			return base.WritePropertyNameAsync(name, escape, cancellationToken);
		}
		return this.DoWritePropertyNameAsync(name, escape, cancellationToken);
	}

	internal async Task DoWritePropertyNameAsync(string name, bool escape, CancellationToken cancellationToken)
	{
		await base.InternalWritePropertyNameAsync(name, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		if (escape)
		{
			await this.WriteEscapedStringAsync(name, this._quoteName, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		else
		{
			if (this._quoteName)
			{
				await this._writer.WriteAsync(this._quoteChar).ConfigureAwait(continueOnCapturedContext: false);
			}
			await this._writer.WriteAsync(name, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			if (this._quoteName)
			{
				await this._writer.WriteAsync(this._quoteChar).ConfigureAwait(continueOnCapturedContext: false);
			}
		}
		await this._writer.WriteAsync(':').ConfigureAwait(continueOnCapturedContext: false);
	}

	/// <summary>
	/// Asynchronously writes the beginning of a JSON array.
	/// </summary>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
	/// execute synchronously, returning an already-completed task.</remarks>
	public override Task WriteStartArrayAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!this._safeAsync)
		{
			return base.WriteStartArrayAsync(cancellationToken);
		}
		return this.DoWriteStartArrayAsync(cancellationToken);
	}

	internal Task DoWriteStartArrayAsync(CancellationToken cancellationToken)
	{
		Task task = base.InternalWriteStartAsync(JsonToken.StartArray, JsonContainerType.Array, cancellationToken);
		if (task.IsCompletedSuccessfully())
		{
			return this._writer.WriteAsync('[', cancellationToken);
		}
		return this.DoWriteStartArrayAsync(task, cancellationToken);
	}

	internal async Task DoWriteStartArrayAsync(Task task, CancellationToken cancellationToken)
	{
		await task.ConfigureAwait(continueOnCapturedContext: false);
		await this._writer.WriteAsync('[', cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	/// <summary>
	/// Asynchronously writes the beginning of a JSON object.
	/// </summary>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
	/// execute synchronously, returning an already-completed task.</remarks>
	public override Task WriteStartObjectAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!this._safeAsync)
		{
			return base.WriteStartObjectAsync(cancellationToken);
		}
		return this.DoWriteStartObjectAsync(cancellationToken);
	}

	internal Task DoWriteStartObjectAsync(CancellationToken cancellationToken)
	{
		Task task = base.InternalWriteStartAsync(JsonToken.StartObject, JsonContainerType.Object, cancellationToken);
		if (task.IsCompletedSuccessfully())
		{
			return this._writer.WriteAsync('{', cancellationToken);
		}
		return this.DoWriteStartObjectAsync(task, cancellationToken);
	}

	internal async Task DoWriteStartObjectAsync(Task task, CancellationToken cancellationToken)
	{
		await task.ConfigureAwait(continueOnCapturedContext: false);
		await this._writer.WriteAsync('{', cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	/// <summary>
	/// Asynchronously writes the start of a constructor with the given name.
	/// </summary>
	/// <param name="name">The name of the constructor.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
	/// execute synchronously, returning an already-completed task.</remarks>
	public override Task WriteStartConstructorAsync(string name, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!this._safeAsync)
		{
			return base.WriteStartConstructorAsync(name, cancellationToken);
		}
		return this.DoWriteStartConstructorAsync(name, cancellationToken);
	}

	internal async Task DoWriteStartConstructorAsync(string name, CancellationToken cancellationToken)
	{
		await base.InternalWriteStartAsync(JsonToken.StartConstructor, JsonContainerType.Constructor, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		await this._writer.WriteAsync("new ", cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		await this._writer.WriteAsync(name, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		await this._writer.WriteAsync('(').ConfigureAwait(continueOnCapturedContext: false);
	}

	/// <summary>
	/// Asynchronously writes an undefined value.
	/// </summary>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
	/// execute synchronously, returning an already-completed task.</remarks>
	public override Task WriteUndefinedAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!this._safeAsync)
		{
			return base.WriteUndefinedAsync(cancellationToken);
		}
		return this.DoWriteUndefinedAsync(cancellationToken);
	}

	internal Task DoWriteUndefinedAsync(CancellationToken cancellationToken)
	{
		Task task = base.InternalWriteValueAsync(JsonToken.Undefined, cancellationToken);
		if (task.IsCompletedSuccessfully())
		{
			return this._writer.WriteAsync(JsonConvert.Undefined, cancellationToken);
		}
		return this.DoWriteUndefinedAsync(task, cancellationToken);
	}

	private async Task DoWriteUndefinedAsync(Task task, CancellationToken cancellationToken)
	{
		await task.ConfigureAwait(continueOnCapturedContext: false);
		await this._writer.WriteAsync(JsonConvert.Undefined, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	/// <summary>
	/// Asynchronously writes the given white space.
	/// </summary>
	/// <param name="ws">The string of white space characters.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
	/// execute synchronously, returning an already-completed task.</remarks>
	public override Task WriteWhitespaceAsync(string ws, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!this._safeAsync)
		{
			return base.WriteWhitespaceAsync(ws, cancellationToken);
		}
		return this.DoWriteWhitespaceAsync(ws, cancellationToken);
	}

	internal Task DoWriteWhitespaceAsync(string ws, CancellationToken cancellationToken)
	{
		base.InternalWriteWhitespace(ws);
		return this._writer.WriteAsync(ws, cancellationToken);
	}

	/// <summary>
	/// Asynchronously writes a <see cref="T:System.Nullable`1" /> of <see cref="T:System.Boolean" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Nullable`1" /> of <see cref="T:System.Boolean" /> value to write.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
	/// execute synchronously, returning an already-completed task.</remarks>
	public override Task WriteValueAsync(bool value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!this._safeAsync)
		{
			return base.WriteValueAsync(value, cancellationToken);
		}
		return this.DoWriteValueAsync(value, cancellationToken);
	}

	internal Task DoWriteValueAsync(bool value, CancellationToken cancellationToken)
	{
		return this.WriteValueInternalAsync(JsonToken.Boolean, JsonConvert.ToString(value), cancellationToken);
	}

	/// <summary>
	/// Asynchronously writes a <see cref="T:System.Boolean" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Boolean" /> value to write.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
	/// execute synchronously, returning an already-completed task.</remarks>
	public override Task WriteValueAsync(bool? value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!this._safeAsync)
		{
			return base.WriteValueAsync(value, cancellationToken);
		}
		return this.DoWriteValueAsync(value, cancellationToken);
	}

	internal Task DoWriteValueAsync(bool? value, CancellationToken cancellationToken)
	{
		if (value.HasValue)
		{
			return this.DoWriteValueAsync(value == true, cancellationToken);
		}
		return this.DoWriteNullAsync(cancellationToken);
	}

	/// <summary>
	/// Asynchronously writes a <see cref="T:System.Byte" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Byte" /> value to write.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
	/// execute synchronously, returning an already-completed task.</remarks>
	public override Task WriteValueAsync(byte value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!this._safeAsync)
		{
			return base.WriteValueAsync(value, cancellationToken);
		}
		return this.WriteIntegerValueAsync(value, cancellationToken);
	}

	/// <summary>
	/// Asynchronously writes a <see cref="T:System.Nullable`1" /> of <see cref="T:System.Byte" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Nullable`1" /> of <see cref="T:System.Byte" /> value to write.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
	/// execute synchronously, returning an already-completed task.</remarks>
	public override Task WriteValueAsync(byte? value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!this._safeAsync)
		{
			return base.WriteValueAsync(value, cancellationToken);
		}
		return this.DoWriteValueAsync(value, cancellationToken);
	}

	internal Task DoWriteValueAsync(byte? value, CancellationToken cancellationToken)
	{
		if (value.HasValue)
		{
			return this.WriteIntegerValueAsync(value.GetValueOrDefault(), cancellationToken);
		}
		return this.DoWriteNullAsync(cancellationToken);
	}

	/// <summary>
	/// Asynchronously writes a <see cref="T:System.Byte" />[] value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Byte" />[] value to write.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
	/// execute synchronously, returning an already-completed task.</remarks>
	public override Task WriteValueAsync(byte[]? value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!this._safeAsync)
		{
			return base.WriteValueAsync(value, cancellationToken);
		}
		if (value != null)
		{
			return this.WriteValueNonNullAsync(value, cancellationToken);
		}
		return this.WriteNullAsync(cancellationToken);
	}

	internal async Task WriteValueNonNullAsync(byte[] value, CancellationToken cancellationToken)
	{
		await base.InternalWriteValueAsync(JsonToken.Bytes, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		await this._writer.WriteAsync(this._quoteChar).ConfigureAwait(continueOnCapturedContext: false);
		await this.Base64Encoder.EncodeAsync(value, 0, value.Length, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		await this.Base64Encoder.FlushAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		await this._writer.WriteAsync(this._quoteChar).ConfigureAwait(continueOnCapturedContext: false);
	}

	/// <summary>
	/// Asynchronously writes a <see cref="T:System.Char" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Char" /> value to write.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
	/// execute synchronously, returning an already-completed task.</remarks>
	public override Task WriteValueAsync(char value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!this._safeAsync)
		{
			return base.WriteValueAsync(value, cancellationToken);
		}
		return this.DoWriteValueAsync(value, cancellationToken);
	}

	internal Task DoWriteValueAsync(char value, CancellationToken cancellationToken)
	{
		return this.WriteValueInternalAsync(JsonToken.String, JsonConvert.ToString(value), cancellationToken);
	}

	/// <summary>
	/// Asynchronously writes a <see cref="T:System.Nullable`1" /> of <see cref="T:System.Char" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Nullable`1" /> of <see cref="T:System.Char" /> value to write.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
	/// execute synchronously, returning an already-completed task.</remarks>
	public override Task WriteValueAsync(char? value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!this._safeAsync)
		{
			return base.WriteValueAsync(value, cancellationToken);
		}
		return this.DoWriteValueAsync(value, cancellationToken);
	}

	internal Task DoWriteValueAsync(char? value, CancellationToken cancellationToken)
	{
		if (value.HasValue)
		{
			return this.DoWriteValueAsync(value.GetValueOrDefault(), cancellationToken);
		}
		return this.DoWriteNullAsync(cancellationToken);
	}

	/// <summary>
	/// Asynchronously writes a <see cref="T:System.DateTime" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.DateTime" /> value to write.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
	/// execute synchronously, returning an already-completed task.</remarks>
	public override Task WriteValueAsync(DateTime value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!this._safeAsync)
		{
			return base.WriteValueAsync(value, cancellationToken);
		}
		return this.DoWriteValueAsync(value, cancellationToken);
	}

	internal async Task DoWriteValueAsync(DateTime value, CancellationToken cancellationToken)
	{
		await base.InternalWriteValueAsync(JsonToken.Date, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		value = DateTimeUtils.EnsureDateTime(value, base.DateTimeZoneHandling);
		if (StringUtils.IsNullOrEmpty(base.DateFormatString))
		{
			int count = this.WriteValueToBuffer(value);
			await this._writer.WriteAsync(this._writeBuffer, 0, count, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		else
		{
			await this._writer.WriteAsync(this._quoteChar).ConfigureAwait(continueOnCapturedContext: false);
			await this._writer.WriteAsync(value.ToString(base.DateFormatString, base.Culture), cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			await this._writer.WriteAsync(this._quoteChar).ConfigureAwait(continueOnCapturedContext: false);
		}
	}

	/// <summary>
	/// Asynchronously writes a <see cref="T:System.Nullable`1" /> of <see cref="T:System.DateTime" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Nullable`1" /> of <see cref="T:System.DateTime" /> value to write.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
	/// execute synchronously, returning an already-completed task.</remarks>
	public override Task WriteValueAsync(DateTime? value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!this._safeAsync)
		{
			return base.WriteValueAsync(value, cancellationToken);
		}
		return this.DoWriteValueAsync(value, cancellationToken);
	}

	internal Task DoWriteValueAsync(DateTime? value, CancellationToken cancellationToken)
	{
		if (value.HasValue)
		{
			return this.DoWriteValueAsync(value.GetValueOrDefault(), cancellationToken);
		}
		return this.DoWriteNullAsync(cancellationToken);
	}

	/// <summary>
	/// Asynchronously writes a <see cref="T:System.DateTimeOffset" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.DateTimeOffset" /> value to write.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
	/// execute synchronously, returning an already-completed task.</remarks>
	public override Task WriteValueAsync(DateTimeOffset value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!this._safeAsync)
		{
			return base.WriteValueAsync(value, cancellationToken);
		}
		return this.DoWriteValueAsync(value, cancellationToken);
	}

	internal async Task DoWriteValueAsync(DateTimeOffset value, CancellationToken cancellationToken)
	{
		await base.InternalWriteValueAsync(JsonToken.Date, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		if (StringUtils.IsNullOrEmpty(base.DateFormatString))
		{
			int count = this.WriteValueToBuffer(value);
			await this._writer.WriteAsync(this._writeBuffer, 0, count, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		else
		{
			await this._writer.WriteAsync(this._quoteChar).ConfigureAwait(continueOnCapturedContext: false);
			await this._writer.WriteAsync(value.ToString(base.DateFormatString, base.Culture), cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			await this._writer.WriteAsync(this._quoteChar).ConfigureAwait(continueOnCapturedContext: false);
		}
	}

	/// <summary>
	/// Asynchronously writes a <see cref="T:System.Nullable`1" /> of <see cref="T:System.DateTimeOffset" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Nullable`1" /> of <see cref="T:System.DateTimeOffset" /> value to write.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
	/// execute synchronously, returning an already-completed task.</remarks>
	public override Task WriteValueAsync(DateTimeOffset? value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!this._safeAsync)
		{
			return base.WriteValueAsync(value, cancellationToken);
		}
		return this.DoWriteValueAsync(value, cancellationToken);
	}

	internal Task DoWriteValueAsync(DateTimeOffset? value, CancellationToken cancellationToken)
	{
		if (value.HasValue)
		{
			return this.DoWriteValueAsync(value.GetValueOrDefault(), cancellationToken);
		}
		return this.DoWriteNullAsync(cancellationToken);
	}

	/// <summary>
	/// Asynchronously writes a <see cref="T:System.Decimal" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Decimal" /> value to write.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
	/// execute synchronously, returning an already-completed task.</remarks>
	public override Task WriteValueAsync(decimal value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!this._safeAsync)
		{
			return base.WriteValueAsync(value, cancellationToken);
		}
		return this.DoWriteValueAsync(value, cancellationToken);
	}

	internal Task DoWriteValueAsync(decimal value, CancellationToken cancellationToken)
	{
		return this.WriteValueInternalAsync(JsonToken.Float, JsonConvert.ToString(value), cancellationToken);
	}

	/// <summary>
	/// Asynchronously writes a <see cref="T:System.Nullable`1" /> of <see cref="T:System.Decimal" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Nullable`1" /> of <see cref="T:System.Decimal" /> value to write.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
	/// execute synchronously, returning an already-completed task.</remarks>
	public override Task WriteValueAsync(decimal? value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!this._safeAsync)
		{
			return base.WriteValueAsync(value, cancellationToken);
		}
		return this.DoWriteValueAsync(value, cancellationToken);
	}

	internal Task DoWriteValueAsync(decimal? value, CancellationToken cancellationToken)
	{
		if (value.HasValue)
		{
			return this.DoWriteValueAsync(value.GetValueOrDefault(), cancellationToken);
		}
		return this.DoWriteNullAsync(cancellationToken);
	}

	/// <summary>
	/// Asynchronously writes a <see cref="T:System.Double" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Double" /> value to write.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
	/// execute synchronously, returning an already-completed task.</remarks>
	public override Task WriteValueAsync(double value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!this._safeAsync)
		{
			return base.WriteValueAsync(value, cancellationToken);
		}
		return this.WriteValueAsync(value, nullable: false, cancellationToken);
	}

	internal Task WriteValueAsync(double value, bool nullable, CancellationToken cancellationToken)
	{
		return this.WriteValueInternalAsync(JsonToken.Float, JsonConvert.ToString(value, base.FloatFormatHandling, this.QuoteChar, nullable), cancellationToken);
	}

	/// <summary>
	/// Asynchronously writes a <see cref="T:System.Nullable`1" /> of <see cref="T:System.Double" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Nullable`1" /> of <see cref="T:System.Double" /> value to write.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
	/// execute synchronously, returning an already-completed task.</remarks>
	public override Task WriteValueAsync(double? value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!this._safeAsync)
		{
			return base.WriteValueAsync(value, cancellationToken);
		}
		if (!value.HasValue)
		{
			return this.WriteNullAsync(cancellationToken);
		}
		return this.WriteValueAsync(value.GetValueOrDefault(), nullable: true, cancellationToken);
	}

	/// <summary>
	/// Asynchronously writes a <see cref="T:System.Single" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Single" /> value to write.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
	/// execute synchronously, returning an already-completed task.</remarks>
	public override Task WriteValueAsync(float value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!this._safeAsync)
		{
			return base.WriteValueAsync(value, cancellationToken);
		}
		return this.WriteValueAsync(value, nullable: false, cancellationToken);
	}

	internal Task WriteValueAsync(float value, bool nullable, CancellationToken cancellationToken)
	{
		return this.WriteValueInternalAsync(JsonToken.Float, JsonConvert.ToString(value, base.FloatFormatHandling, this.QuoteChar, nullable), cancellationToken);
	}

	/// <summary>
	/// Asynchronously writes a <see cref="T:System.Nullable`1" /> of <see cref="T:System.Single" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Nullable`1" /> of <see cref="T:System.Single" /> value to write.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
	/// execute synchronously, returning an already-completed task.</remarks>
	public override Task WriteValueAsync(float? value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!this._safeAsync)
		{
			return base.WriteValueAsync(value, cancellationToken);
		}
		if (!value.HasValue)
		{
			return this.WriteNullAsync(cancellationToken);
		}
		return this.WriteValueAsync(value.GetValueOrDefault(), nullable: true, cancellationToken);
	}

	/// <summary>
	/// Asynchronously writes a <see cref="T:System.Guid" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Guid" /> value to write.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
	/// execute synchronously, returning an already-completed task.</remarks>
	public override Task WriteValueAsync(Guid value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!this._safeAsync)
		{
			return base.WriteValueAsync(value, cancellationToken);
		}
		return this.DoWriteValueAsync(value, cancellationToken);
	}

	internal async Task DoWriteValueAsync(Guid value, CancellationToken cancellationToken)
	{
		await base.InternalWriteValueAsync(JsonToken.String, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		await this._writer.WriteAsync(this._quoteChar).ConfigureAwait(continueOnCapturedContext: false);
		await this._writer.WriteAsync(value.ToString("D", CultureInfo.InvariantCulture), cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		await this._writer.WriteAsync(this._quoteChar).ConfigureAwait(continueOnCapturedContext: false);
	}

	/// <summary>
	/// Asynchronously writes a <see cref="T:System.Nullable`1" /> of <see cref="T:System.Guid" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Nullable`1" /> of <see cref="T:System.Guid" /> value to write.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
	/// execute synchronously, returning an already-completed task.</remarks>
	public override Task WriteValueAsync(Guid? value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!this._safeAsync)
		{
			return base.WriteValueAsync(value, cancellationToken);
		}
		return this.DoWriteValueAsync(value, cancellationToken);
	}

	internal Task DoWriteValueAsync(Guid? value, CancellationToken cancellationToken)
	{
		if (value.HasValue)
		{
			return this.DoWriteValueAsync(value.GetValueOrDefault(), cancellationToken);
		}
		return this.DoWriteNullAsync(cancellationToken);
	}

	/// <summary>
	/// Asynchronously writes a <see cref="T:System.Int32" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Int32" /> value to write.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
	/// execute synchronously, returning an already-completed task.</remarks>
	public override Task WriteValueAsync(int value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!this._safeAsync)
		{
			return base.WriteValueAsync(value, cancellationToken);
		}
		return this.WriteIntegerValueAsync(value, cancellationToken);
	}

	/// <summary>
	/// Asynchronously writes a <see cref="T:System.Nullable`1" /> of <see cref="T:System.Int32" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Nullable`1" /> of <see cref="T:System.Int32" /> value to write.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
	/// execute synchronously, returning an already-completed task.</remarks>
	public override Task WriteValueAsync(int? value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!this._safeAsync)
		{
			return base.WriteValueAsync(value, cancellationToken);
		}
		return this.DoWriteValueAsync(value, cancellationToken);
	}

	internal Task DoWriteValueAsync(int? value, CancellationToken cancellationToken)
	{
		if (value.HasValue)
		{
			return this.WriteIntegerValueAsync(value.GetValueOrDefault(), cancellationToken);
		}
		return this.DoWriteNullAsync(cancellationToken);
	}

	/// <summary>
	/// Asynchronously writes a <see cref="T:System.Int64" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Int64" /> value to write.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
	/// execute synchronously, returning an already-completed task.</remarks>
	public override Task WriteValueAsync(long value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!this._safeAsync)
		{
			return base.WriteValueAsync(value, cancellationToken);
		}
		return this.WriteIntegerValueAsync(value, cancellationToken);
	}

	/// <summary>
	/// Asynchronously writes a <see cref="T:System.Nullable`1" /> of <see cref="T:System.Int64" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Nullable`1" /> of <see cref="T:System.Int64" /> value to write.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
	/// execute synchronously, returning an already-completed task.</remarks>
	public override Task WriteValueAsync(long? value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!this._safeAsync)
		{
			return base.WriteValueAsync(value, cancellationToken);
		}
		return this.DoWriteValueAsync(value, cancellationToken);
	}

	internal Task DoWriteValueAsync(long? value, CancellationToken cancellationToken)
	{
		if (value.HasValue)
		{
			return this.WriteIntegerValueAsync(value.GetValueOrDefault(), cancellationToken);
		}
		return this.DoWriteNullAsync(cancellationToken);
	}

	internal Task WriteValueAsync(BigInteger value, CancellationToken cancellationToken)
	{
		return this.WriteValueInternalAsync(JsonToken.Integer, value.ToString(CultureInfo.InvariantCulture), cancellationToken);
	}

	/// <summary>
	/// Asynchronously writes a <see cref="T:System.Object" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Object" /> value to write.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
	/// execute synchronously, returning an already-completed task.</remarks>
	public override Task WriteValueAsync(object? value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (this._safeAsync)
		{
			if (value == null)
			{
				return this.WriteNullAsync(cancellationToken);
			}
			if (value is BigInteger value2)
			{
				return this.WriteValueAsync(value2, cancellationToken);
			}
			return JsonWriter.WriteValueAsync(this, ConvertUtils.GetTypeCode(value.GetType()), value, cancellationToken);
		}
		return base.WriteValueAsync(value, cancellationToken);
	}

	/// <summary>
	/// Asynchronously writes a <see cref="T:System.SByte" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.SByte" /> value to write.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
	/// execute synchronously, returning an already-completed task.</remarks>
	[CLSCompliant(false)]
	public override Task WriteValueAsync(sbyte value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!this._safeAsync)
		{
			return base.WriteValueAsync(value, cancellationToken);
		}
		return this.WriteIntegerValueAsync(value, cancellationToken);
	}

	/// <summary>
	/// Asynchronously writes a <see cref="T:System.Nullable`1" /> of <see cref="T:System.SByte" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Nullable`1" /> of <see cref="T:System.SByte" /> value to write.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
	/// execute synchronously, returning an already-completed task.</remarks>
	[CLSCompliant(false)]
	public override Task WriteValueAsync(sbyte? value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!this._safeAsync)
		{
			return base.WriteValueAsync(value, cancellationToken);
		}
		return this.DoWriteValueAsync(value, cancellationToken);
	}

	internal Task DoWriteValueAsync(sbyte? value, CancellationToken cancellationToken)
	{
		if (value.HasValue)
		{
			return this.WriteIntegerValueAsync(value.GetValueOrDefault(), cancellationToken);
		}
		return this.DoWriteNullAsync(cancellationToken);
	}

	/// <summary>
	/// Asynchronously writes a <see cref="T:System.Int16" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Int16" /> value to write.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
	/// execute synchronously, returning an already-completed task.</remarks>
	public override Task WriteValueAsync(short value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!this._safeAsync)
		{
			return base.WriteValueAsync(value, cancellationToken);
		}
		return this.WriteIntegerValueAsync(value, cancellationToken);
	}

	/// <summary>
	/// Asynchronously writes a <see cref="T:System.Nullable`1" /> of <see cref="T:System.Int16" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Nullable`1" /> of <see cref="T:System.Int16" /> value to write.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
	/// execute synchronously, returning an already-completed task.</remarks>
	public override Task WriteValueAsync(short? value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!this._safeAsync)
		{
			return base.WriteValueAsync(value, cancellationToken);
		}
		return this.DoWriteValueAsync(value, cancellationToken);
	}

	internal Task DoWriteValueAsync(short? value, CancellationToken cancellationToken)
	{
		if (value.HasValue)
		{
			return this.WriteIntegerValueAsync(value.GetValueOrDefault(), cancellationToken);
		}
		return this.DoWriteNullAsync(cancellationToken);
	}

	/// <summary>
	/// Asynchronously writes a <see cref="T:System.String" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.String" /> value to write.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
	/// execute synchronously, returning an already-completed task.</remarks>
	public override Task WriteValueAsync(string? value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!this._safeAsync)
		{
			return base.WriteValueAsync(value, cancellationToken);
		}
		return this.DoWriteValueAsync(value, cancellationToken);
	}

	internal Task DoWriteValueAsync(string? value, CancellationToken cancellationToken)
	{
		Task task = base.InternalWriteValueAsync(JsonToken.String, cancellationToken);
		if (task.IsCompletedSuccessfully())
		{
			if (value != null)
			{
				return this.WriteEscapedStringAsync(value, quote: true, cancellationToken);
			}
			return this._writer.WriteAsync(JsonConvert.Null, cancellationToken);
		}
		return this.DoWriteValueAsync(task, value, cancellationToken);
	}

	private async Task DoWriteValueAsync(Task task, string? value, CancellationToken cancellationToken)
	{
		await task.ConfigureAwait(continueOnCapturedContext: false);
		await ((value == null) ? this._writer.WriteAsync(JsonConvert.Null, cancellationToken) : this.WriteEscapedStringAsync(value, quote: true, cancellationToken)).ConfigureAwait(continueOnCapturedContext: false);
	}

	/// <summary>
	/// Asynchronously writes a <see cref="T:System.TimeSpan" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.TimeSpan" /> value to write.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
	/// execute synchronously, returning an already-completed task.</remarks>
	public override Task WriteValueAsync(TimeSpan value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!this._safeAsync)
		{
			return base.WriteValueAsync(value, cancellationToken);
		}
		return this.DoWriteValueAsync(value, cancellationToken);
	}

	internal async Task DoWriteValueAsync(TimeSpan value, CancellationToken cancellationToken)
	{
		await base.InternalWriteValueAsync(JsonToken.String, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		await this._writer.WriteAsync(this._quoteChar, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		await this._writer.WriteAsync(value.ToString(null, CultureInfo.InvariantCulture), cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		await this._writer.WriteAsync(this._quoteChar, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	/// <summary>
	/// Asynchronously writes a <see cref="T:System.Nullable`1" /> of <see cref="T:System.TimeSpan" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Nullable`1" /> of <see cref="T:System.TimeSpan" /> value to write.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
	/// execute synchronously, returning an already-completed task.</remarks>
	public override Task WriteValueAsync(TimeSpan? value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!this._safeAsync)
		{
			return base.WriteValueAsync(value, cancellationToken);
		}
		return this.DoWriteValueAsync(value, cancellationToken);
	}

	internal Task DoWriteValueAsync(TimeSpan? value, CancellationToken cancellationToken)
	{
		if (value.HasValue)
		{
			return this.DoWriteValueAsync(value.GetValueOrDefault(), cancellationToken);
		}
		return this.DoWriteNullAsync(cancellationToken);
	}

	/// <summary>
	/// Asynchronously writes a <see cref="T:System.UInt32" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.UInt32" /> value to write.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
	/// execute synchronously, returning an already-completed task.</remarks>
	[CLSCompliant(false)]
	public override Task WriteValueAsync(uint value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!this._safeAsync)
		{
			return base.WriteValueAsync(value, cancellationToken);
		}
		return this.WriteIntegerValueAsync(value, cancellationToken);
	}

	/// <summary>
	/// Asynchronously writes a <see cref="T:System.Nullable`1" /> of <see cref="T:System.UInt32" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Nullable`1" /> of <see cref="T:System.UInt32" /> value to write.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
	/// execute synchronously, returning an already-completed task.</remarks>
	[CLSCompliant(false)]
	public override Task WriteValueAsync(uint? value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!this._safeAsync)
		{
			return base.WriteValueAsync(value, cancellationToken);
		}
		return this.DoWriteValueAsync(value, cancellationToken);
	}

	internal Task DoWriteValueAsync(uint? value, CancellationToken cancellationToken)
	{
		if (value.HasValue)
		{
			return this.WriteIntegerValueAsync(value.GetValueOrDefault(), cancellationToken);
		}
		return this.DoWriteNullAsync(cancellationToken);
	}

	/// <summary>
	/// Asynchronously writes a <see cref="T:System.UInt64" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.UInt64" /> value to write.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
	/// execute synchronously, returning an already-completed task.</remarks>
	[CLSCompliant(false)]
	public override Task WriteValueAsync(ulong value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!this._safeAsync)
		{
			return base.WriteValueAsync(value, cancellationToken);
		}
		return this.WriteIntegerValueAsync(value, cancellationToken);
	}

	/// <summary>
	/// Asynchronously writes a <see cref="T:System.Nullable`1" /> of <see cref="T:System.UInt64" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Nullable`1" /> of <see cref="T:System.UInt64" /> value to write.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
	/// execute synchronously, returning an already-completed task.</remarks>
	[CLSCompliant(false)]
	public override Task WriteValueAsync(ulong? value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!this._safeAsync)
		{
			return base.WriteValueAsync(value, cancellationToken);
		}
		return this.DoWriteValueAsync(value, cancellationToken);
	}

	internal Task DoWriteValueAsync(ulong? value, CancellationToken cancellationToken)
	{
		if (value.HasValue)
		{
			return this.WriteIntegerValueAsync(value.GetValueOrDefault(), cancellationToken);
		}
		return this.DoWriteNullAsync(cancellationToken);
	}

	/// <summary>
	/// Asynchronously writes a <see cref="T:System.Uri" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Uri" /> value to write.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
	/// execute synchronously, returning an already-completed task.</remarks>
	public override Task WriteValueAsync(Uri? value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!this._safeAsync)
		{
			return base.WriteValueAsync(value, cancellationToken);
		}
		if (!(value == null))
		{
			return this.WriteValueNotNullAsync(value, cancellationToken);
		}
		return this.WriteNullAsync(cancellationToken);
	}

	internal Task WriteValueNotNullAsync(Uri value, CancellationToken cancellationToken)
	{
		Task task = base.InternalWriteValueAsync(JsonToken.String, cancellationToken);
		if (task.IsCompletedSuccessfully())
		{
			return this.WriteEscapedStringAsync(value.OriginalString, quote: true, cancellationToken);
		}
		return this.WriteValueNotNullAsync(task, value, cancellationToken);
	}

	internal async Task WriteValueNotNullAsync(Task task, Uri value, CancellationToken cancellationToken)
	{
		await task.ConfigureAwait(continueOnCapturedContext: false);
		await this.WriteEscapedStringAsync(value.OriginalString, quote: true, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	/// <summary>
	/// Asynchronously writes a <see cref="T:System.UInt16" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.UInt16" /> value to write.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
	/// execute synchronously, returning an already-completed task.</remarks>
	[CLSCompliant(false)]
	public override Task WriteValueAsync(ushort value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!this._safeAsync)
		{
			return base.WriteValueAsync(value, cancellationToken);
		}
		return this.WriteIntegerValueAsync(value, cancellationToken);
	}

	/// <summary>
	/// Asynchronously writes a <see cref="T:System.Nullable`1" /> of <see cref="T:System.UInt16" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Nullable`1" /> of <see cref="T:System.UInt16" /> value to write.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
	/// execute synchronously, returning an already-completed task.</remarks>
	[CLSCompliant(false)]
	public override Task WriteValueAsync(ushort? value, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!this._safeAsync)
		{
			return base.WriteValueAsync(value, cancellationToken);
		}
		return this.DoWriteValueAsync(value, cancellationToken);
	}

	internal Task DoWriteValueAsync(ushort? value, CancellationToken cancellationToken)
	{
		if (value.HasValue)
		{
			return this.WriteIntegerValueAsync(value.GetValueOrDefault(), cancellationToken);
		}
		return this.DoWriteNullAsync(cancellationToken);
	}

	/// <summary>
	/// Asynchronously writes a comment <c>/*...*/</c> containing the specified text.
	/// </summary>
	/// <param name="text">Text to place inside the comment.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
	/// execute synchronously, returning an already-completed task.</remarks>
	public override Task WriteCommentAsync(string? text, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!this._safeAsync)
		{
			return base.WriteCommentAsync(text, cancellationToken);
		}
		return this.DoWriteCommentAsync(text, cancellationToken);
	}

	internal async Task DoWriteCommentAsync(string? text, CancellationToken cancellationToken)
	{
		await base.InternalWriteCommentAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		await this._writer.WriteAsync("/*", cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		await this._writer.WriteAsync(text ?? string.Empty, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		await this._writer.WriteAsync("*/", cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	/// <summary>
	/// Asynchronously writes the end of an array.
	/// </summary>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
	/// execute synchronously, returning an already-completed task.</remarks>
	public override Task WriteEndArrayAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!this._safeAsync)
		{
			return base.WriteEndArrayAsync(cancellationToken);
		}
		return base.InternalWriteEndAsync(JsonContainerType.Array, cancellationToken);
	}

	/// <summary>
	/// Asynchronously writes the end of a constructor.
	/// </summary>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
	/// execute synchronously, returning an already-completed task.</remarks>
	public override Task WriteEndConstructorAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!this._safeAsync)
		{
			return base.WriteEndConstructorAsync(cancellationToken);
		}
		return base.InternalWriteEndAsync(JsonContainerType.Constructor, cancellationToken);
	}

	/// <summary>
	/// Asynchronously writes the end of a JSON object.
	/// </summary>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
	/// execute synchronously, returning an already-completed task.</remarks>
	public override Task WriteEndObjectAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!this._safeAsync)
		{
			return base.WriteEndObjectAsync(cancellationToken);
		}
		return base.InternalWriteEndAsync(JsonContainerType.Object, cancellationToken);
	}

	/// <summary>
	/// Asynchronously writes raw JSON where a value is expected and updates the writer's state.
	/// </summary>
	/// <param name="json">The raw JSON to write.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task" /> that represents the asynchronous operation.</returns>
	/// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
	/// execute synchronously, returning an already-completed task.</remarks>
	public override Task WriteRawValueAsync(string? json, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!this._safeAsync)
		{
			return base.WriteRawValueAsync(json, cancellationToken);
		}
		return this.DoWriteRawValueAsync(json, cancellationToken);
	}

	internal Task DoWriteRawValueAsync(string? json, CancellationToken cancellationToken)
	{
		base.UpdateScopeWithFinishedValue();
		Task task = base.AutoCompleteAsync(JsonToken.Undefined, cancellationToken);
		if (task.IsCompletedSuccessfully())
		{
			return this.WriteRawAsync(json, cancellationToken);
		}
		return this.DoWriteRawValueAsync(task, json, cancellationToken);
	}

	private async Task DoWriteRawValueAsync(Task task, string? json, CancellationToken cancellationToken)
	{
		await task.ConfigureAwait(continueOnCapturedContext: false);
		await this.WriteRawAsync(json, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	internal char[] EnsureWriteBuffer(int length, int copyTo)
	{
		if (length < 35)
		{
			length = 35;
		}
		char[] writeBuffer = this._writeBuffer;
		if (writeBuffer == null)
		{
			return this._writeBuffer = BufferUtils.RentBuffer(this._arrayPool, length);
		}
		if (writeBuffer.Length >= length)
		{
			return writeBuffer;
		}
		char[] array = BufferUtils.RentBuffer(this._arrayPool, length);
		if (copyTo != 0)
		{
			Array.Copy(writeBuffer, array, copyTo);
		}
		BufferUtils.ReturnBuffer(this._arrayPool, writeBuffer);
		this._writeBuffer = array;
		return array;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:Newtonsoft.Json.JsonTextWriter" /> class using the specified <see cref="T:System.IO.TextWriter" />.
	/// </summary>
	/// <param name="textWriter">The <see cref="T:System.IO.TextWriter" /> to write to.</param>
	public JsonTextWriter(TextWriter textWriter)
	{
		if (textWriter == null)
		{
			throw new ArgumentNullException("textWriter");
		}
		this._writer = textWriter;
		this._quoteChar = '"';
		this._quoteName = true;
		this._indentChar = ' ';
		this._indentation = 2;
		this.UpdateCharEscapeFlags();
		this._safeAsync = base.GetType() == typeof(JsonTextWriter);
	}

	/// <summary>
	/// Flushes whatever is in the buffer to the underlying <see cref="T:System.IO.TextWriter" /> and also flushes the underlying <see cref="T:System.IO.TextWriter" />.
	/// </summary>
	public override void Flush()
	{
		this._writer.Flush();
	}

	/// <summary>
	/// Closes this writer.
	/// If <see cref="P:Newtonsoft.Json.JsonWriter.CloseOutput" /> is set to <c>true</c>, the underlying <see cref="T:System.IO.TextWriter" /> is also closed.
	/// If <see cref="P:Newtonsoft.Json.JsonWriter.AutoCompleteOnClose" /> is set to <c>true</c>, the JSON is auto-completed.
	/// </summary>
	public override void Close()
	{
		base.Close();
		this.CloseBufferAndWriter();
	}

	private void CloseBufferAndWriter()
	{
		if (this._writeBuffer != null)
		{
			BufferUtils.ReturnBuffer(this._arrayPool, this._writeBuffer);
			this._writeBuffer = null;
		}
		if (base.CloseOutput)
		{
			this._writer?.Close();
		}
	}

	/// <summary>
	/// Writes the beginning of a JSON object.
	/// </summary>
	public override void WriteStartObject()
	{
		base.InternalWriteStart(JsonToken.StartObject, JsonContainerType.Object);
		this._writer.Write('{');
	}

	/// <summary>
	/// Writes the beginning of a JSON array.
	/// </summary>
	public override void WriteStartArray()
	{
		base.InternalWriteStart(JsonToken.StartArray, JsonContainerType.Array);
		this._writer.Write('[');
	}

	/// <summary>
	/// Writes the start of a constructor with the given name.
	/// </summary>
	/// <param name="name">The name of the constructor.</param>
	public override void WriteStartConstructor(string name)
	{
		base.InternalWriteStart(JsonToken.StartConstructor, JsonContainerType.Constructor);
		this._writer.Write("new ");
		this._writer.Write(name);
		this._writer.Write('(');
	}

	/// <summary>
	/// Writes the specified end token.
	/// </summary>
	/// <param name="token">The end token to write.</param>
	protected override void WriteEnd(JsonToken token)
	{
		switch (token)
		{
		case JsonToken.EndObject:
			this._writer.Write('}');
			break;
		case JsonToken.EndArray:
			this._writer.Write(']');
			break;
		case JsonToken.EndConstructor:
			this._writer.Write(')');
			break;
		default:
			throw JsonWriterException.Create(this, "Invalid JsonToken: " + token, null);
		}
	}

	/// <summary>
	/// Writes the property name of a name/value pair on a JSON object.
	/// </summary>
	/// <param name="name">The name of the property.</param>
	public override void WritePropertyName(string name)
	{
		base.InternalWritePropertyName(name);
		this.WriteEscapedString(name, this._quoteName);
		this._writer.Write(':');
	}

	/// <summary>
	/// Writes the property name of a name/value pair on a JSON object.
	/// </summary>
	/// <param name="name">The name of the property.</param>
	/// <param name="escape">A flag to indicate whether the text should be escaped when it is written as a JSON property name.</param>
	public override void WritePropertyName(string name, bool escape)
	{
		base.InternalWritePropertyName(name);
		if (escape)
		{
			this.WriteEscapedString(name, this._quoteName);
		}
		else
		{
			if (this._quoteName)
			{
				this._writer.Write(this._quoteChar);
			}
			this._writer.Write(name);
			if (this._quoteName)
			{
				this._writer.Write(this._quoteChar);
			}
		}
		this._writer.Write(':');
	}

	internal override void OnStringEscapeHandlingChanged()
	{
		this.UpdateCharEscapeFlags();
	}

	private void UpdateCharEscapeFlags()
	{
		this._charEscapeFlags = JavaScriptUtils.GetCharEscapeFlags(base.StringEscapeHandling, this._quoteChar);
	}

	/// <summary>
	/// Writes indent characters.
	/// </summary>
	protected override void WriteIndent()
	{
		int num = base.Top * this._indentation;
		int num2 = this.SetIndentChars();
		this._writer.Write(this._indentChars, 0, num2 + Math.Min(num, 12));
		while ((num -= 12) > 0)
		{
			this._writer.Write(this._indentChars, num2, Math.Min(num, 12));
		}
	}

	private int SetIndentChars()
	{
		string newLine = this._writer.NewLine;
		int length = newLine.Length;
		bool flag = this._indentChars != null && this._indentChars.Length == 12 + length;
		if (flag)
		{
			for (int i = 0; i != length; i++)
			{
				if (newLine[i] != this._indentChars[i])
				{
					flag = false;
					break;
				}
			}
		}
		if (!flag)
		{
			this._indentChars = (newLine + new string(this._indentChar, 12)).ToCharArray();
		}
		return length;
	}

	/// <summary>
	/// Writes the JSON value delimiter.
	/// </summary>
	protected override void WriteValueDelimiter()
	{
		this._writer.Write(',');
	}

	/// <summary>
	/// Writes an indent space.
	/// </summary>
	protected override void WriteIndentSpace()
	{
		this._writer.Write(' ');
	}

	private void WriteValueInternal(string value, JsonToken token)
	{
		this._writer.Write(value);
	}

	/// <summary>
	/// Writes a <see cref="T:System.Object" /> value.
	/// An error will raised if the value cannot be written as a single JSON token.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Object" /> value to write.</param>
	public override void WriteValue(object? value)
	{
		if (value is BigInteger bigInteger)
		{
			base.InternalWriteValue(JsonToken.Integer);
			this.WriteValueInternal(bigInteger.ToString(CultureInfo.InvariantCulture), JsonToken.String);
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
		base.InternalWriteValue(JsonToken.Null);
		this.WriteValueInternal(JsonConvert.Null, JsonToken.Null);
	}

	/// <summary>
	/// Writes an undefined value.
	/// </summary>
	public override void WriteUndefined()
	{
		base.InternalWriteValue(JsonToken.Undefined);
		this.WriteValueInternal(JsonConvert.Undefined, JsonToken.Undefined);
	}

	/// <summary>
	/// Writes raw JSON.
	/// </summary>
	/// <param name="json">The raw JSON to write.</param>
	public override void WriteRaw(string? json)
	{
		base.InternalWriteRaw();
		this._writer.Write(json);
	}

	/// <summary>
	/// Writes a <see cref="T:System.String" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.String" /> value to write.</param>
	public override void WriteValue(string? value)
	{
		base.InternalWriteValue(JsonToken.String);
		if (value == null)
		{
			this.WriteValueInternal(JsonConvert.Null, JsonToken.Null);
		}
		else
		{
			this.WriteEscapedString(value, quote: true);
		}
	}

	private void WriteEscapedString(string value, bool quote)
	{
		this.EnsureWriteBuffer();
		JavaScriptUtils.WriteEscapedJavaScriptString(this._writer, value, this._quoteChar, quote, this._charEscapeFlags, base.StringEscapeHandling, this._arrayPool, ref this._writeBuffer);
	}

	/// <summary>
	/// Writes a <see cref="T:System.Int32" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Int32" /> value to write.</param>
	public override void WriteValue(int value)
	{
		base.InternalWriteValue(JsonToken.Integer);
		this.WriteIntegerValue(value);
	}

	/// <summary>
	/// Writes a <see cref="T:System.UInt32" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.UInt32" /> value to write.</param>
	[CLSCompliant(false)]
	public override void WriteValue(uint value)
	{
		base.InternalWriteValue(JsonToken.Integer);
		this.WriteIntegerValue(value);
	}

	/// <summary>
	/// Writes a <see cref="T:System.Int64" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Int64" /> value to write.</param>
	public override void WriteValue(long value)
	{
		base.InternalWriteValue(JsonToken.Integer);
		this.WriteIntegerValue(value);
	}

	/// <summary>
	/// Writes a <see cref="T:System.UInt64" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.UInt64" /> value to write.</param>
	[CLSCompliant(false)]
	public override void WriteValue(ulong value)
	{
		base.InternalWriteValue(JsonToken.Integer);
		this.WriteIntegerValue(value, negative: false);
	}

	/// <summary>
	/// Writes a <see cref="T:System.Single" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Single" /> value to write.</param>
	public override void WriteValue(float value)
	{
		base.InternalWriteValue(JsonToken.Float);
		this.WriteValueInternal(JsonConvert.ToString(value, base.FloatFormatHandling, this.QuoteChar, nullable: false), JsonToken.Float);
	}

	/// <summary>
	/// Writes a <see cref="T:System.Nullable`1" /> of <see cref="T:System.Single" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Nullable`1" /> of <see cref="T:System.Single" /> value to write.</param>
	public override void WriteValue(float? value)
	{
		if (!value.HasValue)
		{
			this.WriteNull();
			return;
		}
		base.InternalWriteValue(JsonToken.Float);
		this.WriteValueInternal(JsonConvert.ToString(value.GetValueOrDefault(), base.FloatFormatHandling, this.QuoteChar, nullable: true), JsonToken.Float);
	}

	/// <summary>
	/// Writes a <see cref="T:System.Double" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Double" /> value to write.</param>
	public override void WriteValue(double value)
	{
		base.InternalWriteValue(JsonToken.Float);
		this.WriteValueInternal(JsonConvert.ToString(value, base.FloatFormatHandling, this.QuoteChar, nullable: false), JsonToken.Float);
	}

	/// <summary>
	/// Writes a <see cref="T:System.Nullable`1" /> of <see cref="T:System.Double" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Nullable`1" /> of <see cref="T:System.Double" /> value to write.</param>
	public override void WriteValue(double? value)
	{
		if (!value.HasValue)
		{
			this.WriteNull();
			return;
		}
		base.InternalWriteValue(JsonToken.Float);
		this.WriteValueInternal(JsonConvert.ToString(value.GetValueOrDefault(), base.FloatFormatHandling, this.QuoteChar, nullable: true), JsonToken.Float);
	}

	/// <summary>
	/// Writes a <see cref="T:System.Boolean" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Boolean" /> value to write.</param>
	public override void WriteValue(bool value)
	{
		base.InternalWriteValue(JsonToken.Boolean);
		this.WriteValueInternal(JsonConvert.ToString(value), JsonToken.Boolean);
	}

	/// <summary>
	/// Writes a <see cref="T:System.Int16" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Int16" /> value to write.</param>
	public override void WriteValue(short value)
	{
		base.InternalWriteValue(JsonToken.Integer);
		this.WriteIntegerValue(value);
	}

	/// <summary>
	/// Writes a <see cref="T:System.UInt16" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.UInt16" /> value to write.</param>
	[CLSCompliant(false)]
	public override void WriteValue(ushort value)
	{
		base.InternalWriteValue(JsonToken.Integer);
		this.WriteIntegerValue(value);
	}

	/// <summary>
	/// Writes a <see cref="T:System.Char" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Char" /> value to write.</param>
	public override void WriteValue(char value)
	{
		base.InternalWriteValue(JsonToken.String);
		this.WriteValueInternal(JsonConvert.ToString(value), JsonToken.String);
	}

	/// <summary>
	/// Writes a <see cref="T:System.Byte" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Byte" /> value to write.</param>
	public override void WriteValue(byte value)
	{
		base.InternalWriteValue(JsonToken.Integer);
		this.WriteIntegerValue(value);
	}

	/// <summary>
	/// Writes a <see cref="T:System.SByte" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.SByte" /> value to write.</param>
	[CLSCompliant(false)]
	public override void WriteValue(sbyte value)
	{
		base.InternalWriteValue(JsonToken.Integer);
		this.WriteIntegerValue(value);
	}

	/// <summary>
	/// Writes a <see cref="T:System.Decimal" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Decimal" /> value to write.</param>
	public override void WriteValue(decimal value)
	{
		base.InternalWriteValue(JsonToken.Float);
		this.WriteValueInternal(JsonConvert.ToString(value), JsonToken.Float);
	}

	/// <summary>
	/// Writes a <see cref="T:System.DateTime" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.DateTime" /> value to write.</param>
	public override void WriteValue(DateTime value)
	{
		base.InternalWriteValue(JsonToken.Date);
		value = DateTimeUtils.EnsureDateTime(value, base.DateTimeZoneHandling);
		if (StringUtils.IsNullOrEmpty(base.DateFormatString))
		{
			int count = this.WriteValueToBuffer(value);
			this._writer.Write(this._writeBuffer, 0, count);
		}
		else
		{
			this._writer.Write(this._quoteChar);
			this._writer.Write(value.ToString(base.DateFormatString, base.Culture));
			this._writer.Write(this._quoteChar);
		}
	}

	private int WriteValueToBuffer(DateTime value)
	{
		this.EnsureWriteBuffer();
		int start = 0;
		this._writeBuffer[start++] = this._quoteChar;
		start = DateTimeUtils.WriteDateTimeString(this._writeBuffer, start, value, null, value.Kind, base.DateFormatHandling);
		this._writeBuffer[start++] = this._quoteChar;
		return start;
	}

	/// <summary>
	/// Writes a <see cref="T:System.Byte" />[] value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Byte" />[] value to write.</param>
	public override void WriteValue(byte[]? value)
	{
		if (value == null)
		{
			this.WriteNull();
			return;
		}
		base.InternalWriteValue(JsonToken.Bytes);
		this._writer.Write(this._quoteChar);
		this.Base64Encoder.Encode(value, 0, value.Length);
		this.Base64Encoder.Flush();
		this._writer.Write(this._quoteChar);
	}

	/// <summary>
	/// Writes a <see cref="T:System.DateTimeOffset" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.DateTimeOffset" /> value to write.</param>
	public override void WriteValue(DateTimeOffset value)
	{
		base.InternalWriteValue(JsonToken.Date);
		if (StringUtils.IsNullOrEmpty(base.DateFormatString))
		{
			int count = this.WriteValueToBuffer(value);
			this._writer.Write(this._writeBuffer, 0, count);
		}
		else
		{
			this._writer.Write(this._quoteChar);
			this._writer.Write(value.ToString(base.DateFormatString, base.Culture));
			this._writer.Write(this._quoteChar);
		}
	}

	private int WriteValueToBuffer(DateTimeOffset value)
	{
		this.EnsureWriteBuffer();
		int start = 0;
		this._writeBuffer[start++] = this._quoteChar;
		start = DateTimeUtils.WriteDateTimeString(this._writeBuffer, start, (base.DateFormatHandling == DateFormatHandling.IsoDateFormat) ? value.DateTime : value.UtcDateTime, value.Offset, DateTimeKind.Local, base.DateFormatHandling);
		this._writeBuffer[start++] = this._quoteChar;
		return start;
	}

	/// <summary>
	/// Writes a <see cref="T:System.Guid" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Guid" /> value to write.</param>
	public override void WriteValue(Guid value)
	{
		base.InternalWriteValue(JsonToken.String);
		string value2 = value.ToString("D", CultureInfo.InvariantCulture);
		this._writer.Write(this._quoteChar);
		this._writer.Write(value2);
		this._writer.Write(this._quoteChar);
	}

	/// <summary>
	/// Writes a <see cref="T:System.TimeSpan" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.TimeSpan" /> value to write.</param>
	public override void WriteValue(TimeSpan value)
	{
		base.InternalWriteValue(JsonToken.String);
		string value2 = value.ToString(null, CultureInfo.InvariantCulture);
		this._writer.Write(this._quoteChar);
		this._writer.Write(value2);
		this._writer.Write(this._quoteChar);
	}

	/// <summary>
	/// Writes a <see cref="T:System.Uri" /> value.
	/// </summary>
	/// <param name="value">The <see cref="T:System.Uri" /> value to write.</param>
	public override void WriteValue(Uri? value)
	{
		if (value == null)
		{
			this.WriteNull();
			return;
		}
		base.InternalWriteValue(JsonToken.String);
		this.WriteEscapedString(value.OriginalString, quote: true);
	}

	/// <summary>
	/// Writes a comment <c>/*...*/</c> containing the specified text. 
	/// </summary>
	/// <param name="text">Text to place inside the comment.</param>
	public override void WriteComment(string? text)
	{
		base.InternalWriteComment();
		this._writer.Write("/*");
		this._writer.Write(text);
		this._writer.Write("*/");
	}

	/// <summary>
	/// Writes the given white space.
	/// </summary>
	/// <param name="ws">The string of white space characters.</param>
	public override void WriteWhitespace(string ws)
	{
		base.InternalWriteWhitespace(ws);
		this._writer.Write(ws);
	}

	private void EnsureWriteBuffer()
	{
		if (this._writeBuffer == null)
		{
			this._writeBuffer = BufferUtils.RentBuffer(this._arrayPool, 35);
		}
	}

	private void WriteIntegerValue(long value)
	{
		if (value >= 0 && value <= 9)
		{
			this._writer.Write((char)(48 + value));
			return;
		}
		bool flag = value < 0;
		this.WriteIntegerValue((ulong)(flag ? (-value) : value), flag);
	}

	private void WriteIntegerValue(ulong value, bool negative)
	{
		if (!negative && value <= 9)
		{
			this._writer.Write((char)(48 + value));
			return;
		}
		int count = this.WriteNumberToBuffer(value, negative);
		this._writer.Write(this._writeBuffer, 0, count);
	}

	private int WriteNumberToBuffer(ulong value, bool negative)
	{
		if (value <= uint.MaxValue)
		{
			return this.WriteNumberToBuffer((uint)value, negative);
		}
		this.EnsureWriteBuffer();
		int num = MathUtils.IntLength(value);
		if (negative)
		{
			num++;
			this._writeBuffer[0] = '-';
		}
		int num2 = num;
		do
		{
			ulong num3 = value / 10;
			ulong num4 = value - num3 * 10;
			this._writeBuffer[--num2] = (char)(48 + num4);
			value = num3;
		}
		while (value != 0L);
		return num;
	}

	private void WriteIntegerValue(int value)
	{
		if (value >= 0 && value <= 9)
		{
			this._writer.Write((char)(48 + value));
			return;
		}
		bool flag = value < 0;
		this.WriteIntegerValue((uint)(flag ? (-value) : value), flag);
	}

	private void WriteIntegerValue(uint value, bool negative)
	{
		if (!negative && value <= 9)
		{
			this._writer.Write((char)(48 + value));
			return;
		}
		int count = this.WriteNumberToBuffer(value, negative);
		this._writer.Write(this._writeBuffer, 0, count);
	}

	private int WriteNumberToBuffer(uint value, bool negative)
	{
		this.EnsureWriteBuffer();
		int num = MathUtils.IntLength(value);
		if (negative)
		{
			num++;
			this._writeBuffer[0] = '-';
		}
		int num2 = num;
		do
		{
			uint num3 = value / 10;
			uint num4 = value - num3 * 10;
			this._writeBuffer[--num2] = (char)(48 + num4);
			value = num3;
		}
		while (value != 0);
		return num;
	}
}
