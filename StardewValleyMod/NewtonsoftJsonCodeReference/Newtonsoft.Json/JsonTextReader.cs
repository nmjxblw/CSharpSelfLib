using System;
using System.Globalization;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json;

/// <summary>
/// Represents a reader that provides fast, non-cached, forward-only access to JSON text data.
/// </summary>
public class JsonTextReader : JsonReader, IJsonLineInfo
{
	private readonly bool _safeAsync;

	private const char UnicodeReplacementChar = '\ufffd';

	private const int MaximumJavascriptIntegerCharacterLength = 380;

	private const int LargeBufferLength = 1073741823;

	private readonly TextReader _reader;

	private char[]? _chars;

	private int _charsUsed;

	private int _charPos;

	private int _lineStartPos;

	private int _lineNumber;

	private bool _isEndOfFile;

	private StringBuffer _stringBuffer;

	private StringReference _stringReference;

	private IArrayPool<char>? _arrayPool;

	/// <summary>
	/// Gets or sets the reader's property name table.
	/// </summary>
	public JsonNameTable? PropertyNameTable { get; set; }

	/// <summary>
	/// Gets or sets the reader's character buffer pool.
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
	/// Gets the current line number.
	/// </summary>
	/// <value>
	/// The current line number or 0 if no line information is available (for example, <see cref="M:Newtonsoft.Json.JsonTextReader.HasLineInfo" /> returns <c>false</c>).
	/// </value>
	public int LineNumber
	{
		get
		{
			if (base.CurrentState == State.Start && this.LinePosition == 0 && this.TokenType != JsonToken.Comment)
			{
				return 0;
			}
			return this._lineNumber;
		}
	}

	/// <summary>
	/// Gets the current line position.
	/// </summary>
	/// <value>
	/// The current line position or 0 if no line information is available (for example, <see cref="M:Newtonsoft.Json.JsonTextReader.HasLineInfo" /> returns <c>false</c>).
	/// </value>
	public int LinePosition => this._charPos - this._lineStartPos;

	/// <summary>
	/// Asynchronously reads the next JSON token from the source.
	/// </summary>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task`1" /> that represents the asynchronous read. The <see cref="P:System.Threading.Tasks.Task`1.Result" />
	/// property returns <c>true</c> if the next token was read successfully; <c>false</c> if there are no more tokens to read.</returns>
	/// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
	/// execute synchronously, returning an already-completed task.</remarks>
	public override Task<bool> ReadAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!this._safeAsync)
		{
			return base.ReadAsync(cancellationToken);
		}
		return this.DoReadAsync(cancellationToken);
	}

	internal Task<bool> DoReadAsync(CancellationToken cancellationToken)
	{
		this.EnsureBuffer();
		Task<bool> task;
		do
		{
			switch (base._currentState)
			{
			case State.Start:
			case State.Property:
			case State.ArrayStart:
			case State.Array:
			case State.ConstructorStart:
			case State.Constructor:
				return this.ParseValueAsync(cancellationToken);
			case State.ObjectStart:
			case State.Object:
				return this.ParseObjectAsync(cancellationToken);
			case State.PostValue:
				task = this.ParsePostValueAsync(ignoreComments: false, cancellationToken);
				if (!task.IsCompletedSuccessfully())
				{
					return this.DoReadAsync(task, cancellationToken);
				}
				break;
			case State.Finished:
				return this.ReadFromFinishedAsync(cancellationToken);
			default:
				throw JsonReaderException.Create(this, "Unexpected state: {0}.".FormatWith(CultureInfo.InvariantCulture, base.CurrentState));
			}
		}
		while (!task.Result);
		return AsyncUtils.True;
	}

	private async Task<bool> DoReadAsync(Task<bool> task, CancellationToken cancellationToken)
	{
		if (await task.ConfigureAwait(continueOnCapturedContext: false))
		{
			return true;
		}
		return await this.DoReadAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
	}

	private async Task<bool> ParsePostValueAsync(bool ignoreComments, CancellationToken cancellationToken)
	{
		while (true)
		{
			char c = this._chars[this._charPos];
			switch (c)
			{
			case '\0':
				if (this._charsUsed == this._charPos)
				{
					if (await this.ReadDataAsync(append: false, cancellationToken).ConfigureAwait(continueOnCapturedContext: false) == 0)
					{
						base._currentState = State.Finished;
						return false;
					}
				}
				else
				{
					this._charPos++;
				}
				continue;
			case '}':
				this._charPos++;
				base.SetToken(JsonToken.EndObject);
				return true;
			case ']':
				this._charPos++;
				base.SetToken(JsonToken.EndArray);
				return true;
			case ')':
				this._charPos++;
				base.SetToken(JsonToken.EndConstructor);
				return true;
			case '/':
				await this.ParseCommentAsync(!ignoreComments, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				if (!ignoreComments)
				{
					return true;
				}
				continue;
			case ',':
				this._charPos++;
				base.SetStateBasedOnCurrent();
				return false;
			case '\t':
			case ' ':
				this._charPos++;
				continue;
			case '\r':
				await this.ProcessCarriageReturnAsync(append: false, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				continue;
			case '\n':
				this.ProcessLineFeed();
				continue;
			}
			if (char.IsWhiteSpace(c))
			{
				this._charPos++;
				continue;
			}
			if (base.SupportMultipleContent && this.Depth == 0)
			{
				base.SetStateBasedOnCurrent();
				return false;
			}
			throw JsonReaderException.Create(this, "After parsing a value an unexpected character was encountered: {0}.".FormatWith(CultureInfo.InvariantCulture, c));
		}
	}

	private async Task<bool> ReadFromFinishedAsync(CancellationToken cancellationToken)
	{
		if (await this.EnsureCharsAsync(0, append: false, cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
		{
			await this.EatWhitespaceAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			if (this._isEndOfFile)
			{
				base.SetToken(JsonToken.None);
				return false;
			}
			if (this._chars[this._charPos] == '/')
			{
				await this.ParseCommentAsync(setToken: true, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				return true;
			}
			throw JsonReaderException.Create(this, "Additional text encountered after finished reading JSON content: {0}.".FormatWith(CultureInfo.InvariantCulture, this._chars[this._charPos]));
		}
		base.SetToken(JsonToken.None);
		return false;
	}

	private Task<int> ReadDataAsync(bool append, CancellationToken cancellationToken)
	{
		return this.ReadDataAsync(append, 0, cancellationToken);
	}

	private async Task<int> ReadDataAsync(bool append, int charsRequired, CancellationToken cancellationToken)
	{
		if (this._isEndOfFile)
		{
			return 0;
		}
		this.PrepareBufferForReadData(append, charsRequired);
		int num = await this._reader.ReadAsync(this._chars, this._charsUsed, this._chars.Length - this._charsUsed - 1, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		this._charsUsed += num;
		if (num == 0)
		{
			this._isEndOfFile = true;
		}
		this._chars[this._charsUsed] = '\0';
		return num;
	}

	private async Task<bool> ParseValueAsync(CancellationToken cancellationToken)
	{
		while (true)
		{
			char c = this._chars[this._charPos];
			switch (c)
			{
			case '\0':
				if (this._charsUsed == this._charPos)
				{
					if (await this.ReadDataAsync(append: false, cancellationToken).ConfigureAwait(continueOnCapturedContext: false) == 0)
					{
						return false;
					}
				}
				else
				{
					this._charPos++;
				}
				break;
			case '"':
			case '\'':
				await this.ParseStringAsync(c, ReadType.Read, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				return true;
			case 't':
				await this.ParseTrueAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				return true;
			case 'f':
				await this.ParseFalseAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				return true;
			case 'n':
				if (await this.EnsureCharsAsync(1, append: true, cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
				{
					switch (this._chars[this._charPos + 1])
					{
					case 'u':
						await this.ParseNullAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
						break;
					case 'e':
						await this.ParseConstructorAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
						break;
					default:
						throw this.CreateUnexpectedCharacterException(this._chars[this._charPos]);
					}
					return true;
				}
				this._charPos++;
				throw base.CreateUnexpectedEndException();
			case 'N':
				await this.ParseNumberNaNAsync(ReadType.Read, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				return true;
			case 'I':
				await this.ParseNumberPositiveInfinityAsync(ReadType.Read, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				return true;
			case '-':
				if (!(await this.EnsureCharsAsync(1, append: true, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)) || this._chars[this._charPos + 1] != 'I')
				{
					await this.ParseNumberAsync(ReadType.Read, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				}
				else
				{
					await this.ParseNumberNegativeInfinityAsync(ReadType.Read, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				}
				return true;
			case '/':
				await this.ParseCommentAsync(setToken: true, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				return true;
			case 'u':
				await this.ParseUndefinedAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				return true;
			case '{':
				this._charPos++;
				base.SetToken(JsonToken.StartObject);
				return true;
			case '[':
				this._charPos++;
				base.SetToken(JsonToken.StartArray);
				return true;
			case ']':
				this._charPos++;
				base.SetToken(JsonToken.EndArray);
				return true;
			case ',':
				base.SetToken(JsonToken.Undefined);
				return true;
			case ')':
				this._charPos++;
				base.SetToken(JsonToken.EndConstructor);
				return true;
			case '\r':
				await this.ProcessCarriageReturnAsync(append: false, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				break;
			case '\n':
				this.ProcessLineFeed();
				break;
			case '\t':
			case ' ':
				this._charPos++;
				break;
			default:
				if (char.IsWhiteSpace(c))
				{
					this._charPos++;
					break;
				}
				if (char.IsNumber(c) || c == '-' || c == '.')
				{
					await this.ParseNumberAsync(ReadType.Read, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
					return true;
				}
				throw this.CreateUnexpectedCharacterException(c);
			}
		}
	}

	private async Task ReadStringIntoBufferAsync(char quote, CancellationToken cancellationToken)
	{
		int charPos = this._charPos;
		int initialPosition = this._charPos;
		int lastWritePosition = this._charPos;
		this._stringBuffer.Position = 0;
		while (true)
		{
			switch (this._chars[charPos++])
			{
			case '\0':
				if (this._charsUsed == charPos - 1)
				{
					charPos--;
					if (await this.ReadDataAsync(append: true, cancellationToken).ConfigureAwait(continueOnCapturedContext: false) == 0)
					{
						this._charPos = charPos;
						throw JsonReaderException.Create(this, "Unterminated string. Expected delimiter: {0}.".FormatWith(CultureInfo.InvariantCulture, quote));
					}
				}
				break;
			case '\\':
			{
				this._charPos = charPos;
				if (!(await this.EnsureCharsAsync(0, append: true, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)))
				{
					throw JsonReaderException.Create(this, "Unterminated string. Expected delimiter: {0}.".FormatWith(CultureInfo.InvariantCulture, quote));
				}
				int escapeStartPos = charPos - 1;
				char c = this._chars[charPos];
				charPos++;
				char writeChar;
				switch (c)
				{
				case 'b':
					writeChar = '\b';
					break;
				case 't':
					writeChar = '\t';
					break;
				case 'n':
					writeChar = '\n';
					break;
				case 'f':
					writeChar = '\f';
					break;
				case 'r':
					writeChar = '\r';
					break;
				case '\\':
					writeChar = '\\';
					break;
				case '"':
				case '\'':
				case '/':
					writeChar = c;
					break;
				case 'u':
					this._charPos = charPos;
					writeChar = await this.ParseUnicodeAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
					if (StringUtils.IsLowSurrogate(writeChar))
					{
						writeChar = '\ufffd';
					}
					else if (StringUtils.IsHighSurrogate(writeChar))
					{
						bool anotherHighSurrogate;
						do
						{
							anotherHighSurrogate = false;
							if (await this.EnsureCharsAsync(2, append: true, cancellationToken).ConfigureAwait(continueOnCapturedContext: false) && this._chars[this._charPos] == '\\' && this._chars[this._charPos + 1] == 'u')
							{
								char highSurrogate = writeChar;
								this._charPos += 2;
								writeChar = await this.ParseUnicodeAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
								if (!StringUtils.IsLowSurrogate(writeChar))
								{
									if (StringUtils.IsHighSurrogate(writeChar))
									{
										highSurrogate = '\ufffd';
										anotherHighSurrogate = true;
									}
									else
									{
										highSurrogate = '\ufffd';
									}
								}
								this.EnsureBufferNotEmpty();
								this.WriteCharToBuffer(highSurrogate, lastWritePosition, escapeStartPos);
								lastWritePosition = this._charPos;
							}
							else
							{
								writeChar = '\ufffd';
							}
						}
						while (anotherHighSurrogate);
					}
					charPos = this._charPos;
					break;
				default:
					this._charPos = charPos;
					throw JsonReaderException.Create(this, "Bad JSON escape sequence: {0}.".FormatWith(CultureInfo.InvariantCulture, "\\" + c));
				}
				this.EnsureBufferNotEmpty();
				this.WriteCharToBuffer(writeChar, lastWritePosition, escapeStartPos);
				lastWritePosition = charPos;
				break;
			}
			case '\r':
				this._charPos = charPos - 1;
				await this.ProcessCarriageReturnAsync(append: true, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				charPos = this._charPos;
				break;
			case '\n':
				this._charPos = charPos - 1;
				this.ProcessLineFeed();
				charPos = this._charPos;
				break;
			case '"':
			case '\'':
				if (this._chars[charPos - 1] == quote)
				{
					this.FinishReadStringIntoBuffer(charPos - 1, initialPosition, lastWritePosition);
					return;
				}
				break;
			}
		}
	}

	private Task ProcessCarriageReturnAsync(bool append, CancellationToken cancellationToken)
	{
		this._charPos++;
		Task<bool> task = this.EnsureCharsAsync(1, append, cancellationToken);
		if (task.IsCompletedSuccessfully())
		{
			this.SetNewLine(task.Result);
			return AsyncUtils.CompletedTask;
		}
		return this.ProcessCarriageReturnAsync(task);
	}

	private async Task ProcessCarriageReturnAsync(Task<bool> task)
	{
		this.SetNewLine(await task.ConfigureAwait(continueOnCapturedContext: false));
	}

	private async Task<char> ParseUnicodeAsync(CancellationToken cancellationToken)
	{
		return this.ConvertUnicode(await this.EnsureCharsAsync(4, append: true, cancellationToken).ConfigureAwait(continueOnCapturedContext: false));
	}

	private Task<bool> EnsureCharsAsync(int relativePosition, bool append, CancellationToken cancellationToken)
	{
		if (this._charPos + relativePosition < this._charsUsed)
		{
			return AsyncUtils.True;
		}
		if (this._isEndOfFile)
		{
			return AsyncUtils.False;
		}
		return this.ReadCharsAsync(relativePosition, append, cancellationToken);
	}

	private async Task<bool> ReadCharsAsync(int relativePosition, bool append, CancellationToken cancellationToken)
	{
		int charsRequired = this._charPos + relativePosition - this._charsUsed + 1;
		do
		{
			int num = await this.ReadDataAsync(append, charsRequired, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			if (num == 0)
			{
				return false;
			}
			charsRequired -= num;
		}
		while (charsRequired > 0);
		return true;
	}

	private async Task<bool> ParseObjectAsync(CancellationToken cancellationToken)
	{
		while (true)
		{
			char c = this._chars[this._charPos];
			switch (c)
			{
			case '\0':
				if (this._charsUsed == this._charPos)
				{
					if (await this.ReadDataAsync(append: false, cancellationToken).ConfigureAwait(continueOnCapturedContext: false) == 0)
					{
						return false;
					}
				}
				else
				{
					this._charPos++;
				}
				break;
			case '}':
				base.SetToken(JsonToken.EndObject);
				this._charPos++;
				return true;
			case '/':
				await this.ParseCommentAsync(setToken: true, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				return true;
			case '\r':
				await this.ProcessCarriageReturnAsync(append: false, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				break;
			case '\n':
				this.ProcessLineFeed();
				break;
			case '\t':
			case ' ':
				this._charPos++;
				break;
			default:
				if (char.IsWhiteSpace(c))
				{
					this._charPos++;
					break;
				}
				return await this.ParsePropertyAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			}
		}
	}

	private async Task ParseCommentAsync(bool setToken, CancellationToken cancellationToken)
	{
		this._charPos++;
		if (!(await this.EnsureCharsAsync(1, append: false, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)))
		{
			throw JsonReaderException.Create(this, "Unexpected end while parsing comment.");
		}
		bool singlelineComment;
		if (this._chars[this._charPos] == '*')
		{
			singlelineComment = false;
		}
		else
		{
			if (this._chars[this._charPos] != '/')
			{
				throw JsonReaderException.Create(this, "Error parsing comment. Expected: *, got {0}.".FormatWith(CultureInfo.InvariantCulture, this._chars[this._charPos]));
			}
			singlelineComment = true;
		}
		this._charPos++;
		int initialPosition = this._charPos;
		while (true)
		{
			switch (this._chars[this._charPos])
			{
			case '\0':
				if (this._charsUsed == this._charPos)
				{
					if (await this.ReadDataAsync(append: true, cancellationToken).ConfigureAwait(continueOnCapturedContext: false) == 0)
					{
						if (!singlelineComment)
						{
							throw JsonReaderException.Create(this, "Unexpected end while parsing comment.");
						}
						this.EndComment(setToken, initialPosition, this._charPos);
						return;
					}
				}
				else
				{
					this._charPos++;
				}
				break;
			case '*':
				this._charPos++;
				if (!singlelineComment && await this.EnsureCharsAsync(0, append: true, cancellationToken).ConfigureAwait(continueOnCapturedContext: false) && this._chars[this._charPos] == '/')
				{
					this.EndComment(setToken, initialPosition, this._charPos - 1);
					this._charPos++;
					return;
				}
				break;
			case '\r':
				if (singlelineComment)
				{
					this.EndComment(setToken, initialPosition, this._charPos);
					return;
				}
				await this.ProcessCarriageReturnAsync(append: true, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				break;
			case '\n':
				if (singlelineComment)
				{
					this.EndComment(setToken, initialPosition, this._charPos);
					return;
				}
				this.ProcessLineFeed();
				break;
			default:
				this._charPos++;
				break;
			}
		}
	}

	private async Task EatWhitespaceAsync(CancellationToken cancellationToken)
	{
		while (true)
		{
			char c = this._chars[this._charPos];
			switch (c)
			{
			case '\0':
				if (this._charsUsed == this._charPos)
				{
					if (await this.ReadDataAsync(append: false, cancellationToken).ConfigureAwait(continueOnCapturedContext: false) == 0)
					{
						return;
					}
				}
				else
				{
					this._charPos++;
				}
				break;
			case '\r':
				await this.ProcessCarriageReturnAsync(append: false, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				break;
			case '\n':
				this.ProcessLineFeed();
				break;
			default:
				if (!char.IsWhiteSpace(c))
				{
					return;
				}
				goto case ' ';
			case ' ':
				this._charPos++;
				break;
			}
		}
	}

	private async Task ParseStringAsync(char quote, ReadType readType, CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		this._charPos++;
		this.ShiftBufferIfNeeded();
		await this.ReadStringIntoBufferAsync(quote, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		this.ParseReadString(quote, readType);
	}

	private async Task<bool> MatchValueAsync(string value, CancellationToken cancellationToken)
	{
		return this.MatchValue(await this.EnsureCharsAsync(value.Length - 1, append: true, cancellationToken).ConfigureAwait(continueOnCapturedContext: false), value);
	}

	private async Task<bool> MatchValueWithTrailingSeparatorAsync(string value, CancellationToken cancellationToken)
	{
		if (!(await this.MatchValueAsync(value, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)))
		{
			return false;
		}
		if (!(await this.EnsureCharsAsync(0, append: false, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)))
		{
			return true;
		}
		return this.IsSeparator(this._chars[this._charPos]) || this._chars[this._charPos] == '\0';
	}

	private async Task MatchAndSetAsync(string value, JsonToken newToken, object? tokenValue, CancellationToken cancellationToken)
	{
		if (await this.MatchValueWithTrailingSeparatorAsync(value, cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
		{
			base.SetToken(newToken, tokenValue);
			return;
		}
		throw JsonReaderException.Create(this, "Error parsing " + newToken.ToString().ToLowerInvariant() + " value.");
	}

	private Task ParseTrueAsync(CancellationToken cancellationToken)
	{
		return this.MatchAndSetAsync(JsonConvert.True, JsonToken.Boolean, true, cancellationToken);
	}

	private Task ParseFalseAsync(CancellationToken cancellationToken)
	{
		return this.MatchAndSetAsync(JsonConvert.False, JsonToken.Boolean, false, cancellationToken);
	}

	private Task ParseNullAsync(CancellationToken cancellationToken)
	{
		return this.MatchAndSetAsync(JsonConvert.Null, JsonToken.Null, null, cancellationToken);
	}

	private async Task ParseConstructorAsync(CancellationToken cancellationToken)
	{
		if (await this.MatchValueWithTrailingSeparatorAsync("new", cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
		{
			await this.EatWhitespaceAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			int initialPosition = this._charPos;
			int endPosition;
			while (true)
			{
				char c = this._chars[this._charPos];
				if (c == '\0')
				{
					if (this._charsUsed == this._charPos)
					{
						if (await this.ReadDataAsync(append: true, cancellationToken).ConfigureAwait(continueOnCapturedContext: false) == 0)
						{
							throw JsonReaderException.Create(this, "Unexpected end while parsing constructor.");
						}
						continue;
					}
					endPosition = this._charPos;
					this._charPos++;
					break;
				}
				if (char.IsLetterOrDigit(c))
				{
					this._charPos++;
					continue;
				}
				switch (c)
				{
				case '\r':
					endPosition = this._charPos;
					await this.ProcessCarriageReturnAsync(append: true, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
					break;
				case '\n':
					endPosition = this._charPos;
					this.ProcessLineFeed();
					break;
				default:
					if (char.IsWhiteSpace(c))
					{
						endPosition = this._charPos;
						this._charPos++;
						break;
					}
					if (c == '(')
					{
						endPosition = this._charPos;
						break;
					}
					throw JsonReaderException.Create(this, "Unexpected character while parsing constructor: {0}.".FormatWith(CultureInfo.InvariantCulture, c));
				}
				break;
			}
			this._stringReference = new StringReference(this._chars, initialPosition, endPosition - initialPosition);
			string constructorName = this._stringReference.ToString();
			await this.EatWhitespaceAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			if (this._chars[this._charPos] != '(')
			{
				throw JsonReaderException.Create(this, "Unexpected character while parsing constructor: {0}.".FormatWith(CultureInfo.InvariantCulture, this._chars[this._charPos]));
			}
			this._charPos++;
			this.ClearRecentString();
			base.SetToken(JsonToken.StartConstructor, constructorName);
			return;
		}
		throw JsonReaderException.Create(this, "Unexpected content while parsing JSON.");
	}

	private async Task<object> ParseNumberNaNAsync(ReadType readType, CancellationToken cancellationToken)
	{
		return this.ParseNumberNaN(readType, await this.MatchValueWithTrailingSeparatorAsync(JsonConvert.NaN, cancellationToken).ConfigureAwait(continueOnCapturedContext: false));
	}

	private async Task<object> ParseNumberPositiveInfinityAsync(ReadType readType, CancellationToken cancellationToken)
	{
		return this.ParseNumberPositiveInfinity(readType, await this.MatchValueWithTrailingSeparatorAsync(JsonConvert.PositiveInfinity, cancellationToken).ConfigureAwait(continueOnCapturedContext: false));
	}

	private async Task<object> ParseNumberNegativeInfinityAsync(ReadType readType, CancellationToken cancellationToken)
	{
		return this.ParseNumberNegativeInfinity(readType, await this.MatchValueWithTrailingSeparatorAsync(JsonConvert.NegativeInfinity, cancellationToken).ConfigureAwait(continueOnCapturedContext: false));
	}

	private async Task ParseNumberAsync(ReadType readType, CancellationToken cancellationToken)
	{
		this.ShiftBufferIfNeeded();
		char firstChar = this._chars[this._charPos];
		int initialPosition = this._charPos;
		await this.ReadNumberIntoBufferAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		this.ParseReadNumber(readType, firstChar, initialPosition);
	}

	private Task ParseUndefinedAsync(CancellationToken cancellationToken)
	{
		return this.MatchAndSetAsync(JsonConvert.Undefined, JsonToken.Undefined, null, cancellationToken);
	}

	private async Task<bool> ParsePropertyAsync(CancellationToken cancellationToken)
	{
		char c = this._chars[this._charPos];
		char quoteChar;
		if (c == '"' || c == '\'')
		{
			this._charPos++;
			quoteChar = c;
			this.ShiftBufferIfNeeded();
			await this.ReadStringIntoBufferAsync(quoteChar, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		else
		{
			if (!this.ValidIdentifierChar(c))
			{
				throw JsonReaderException.Create(this, "Invalid property identifier character: {0}.".FormatWith(CultureInfo.InvariantCulture, this._chars[this._charPos]));
			}
			quoteChar = '\0';
			this.ShiftBufferIfNeeded();
			await this.ParseUnquotedPropertyAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		string propertyName = ((this.PropertyNameTable == null) ? this._stringReference.ToString() : (this.PropertyNameTable.Get(this._stringReference.Chars, this._stringReference.StartIndex, this._stringReference.Length) ?? this._stringReference.ToString()));
		await this.EatWhitespaceAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		if (this._chars[this._charPos] != ':')
		{
			throw JsonReaderException.Create(this, "Invalid character after parsing property name. Expected ':' but got: {0}.".FormatWith(CultureInfo.InvariantCulture, this._chars[this._charPos]));
		}
		this._charPos++;
		base.SetToken(JsonToken.PropertyName, propertyName);
		base._quoteChar = quoteChar;
		this.ClearRecentString();
		return true;
	}

	private async Task ReadNumberIntoBufferAsync(CancellationToken cancellationToken)
	{
		int charPos = this._charPos;
		while (true)
		{
			char c = this._chars[charPos];
			if (c == '\0')
			{
				this._charPos = charPos;
				if (this._charsUsed != charPos || await this.ReadDataAsync(append: true, cancellationToken).ConfigureAwait(continueOnCapturedContext: false) == 0)
				{
					break;
				}
			}
			else
			{
				if (this.ReadNumberCharIntoBuffer(c, charPos))
				{
					break;
				}
				charPos++;
			}
		}
	}

	private async Task ParseUnquotedPropertyAsync(CancellationToken cancellationToken)
	{
		int initialPosition = this._charPos;
		while (true)
		{
			char c = this._chars[this._charPos];
			if (c == '\0')
			{
				if (this._charsUsed != this._charPos)
				{
					this._stringReference = new StringReference(this._chars, initialPosition, this._charPos - initialPosition);
					break;
				}
				if (await this.ReadDataAsync(append: true, cancellationToken).ConfigureAwait(continueOnCapturedContext: false) == 0)
				{
					throw JsonReaderException.Create(this, "Unexpected end while parsing unquoted property name.");
				}
			}
			else if (this.ReadUnquotedPropertyReportIfDone(c, initialPosition))
			{
				break;
			}
		}
	}

	private async Task<bool> ReadNullCharAsync(CancellationToken cancellationToken)
	{
		if (this._charsUsed == this._charPos)
		{
			if (await this.ReadDataAsync(append: false, cancellationToken).ConfigureAwait(continueOnCapturedContext: false) == 0)
			{
				this._isEndOfFile = true;
				return true;
			}
		}
		else
		{
			this._charPos++;
		}
		return false;
	}

	private async Task HandleNullAsync(CancellationToken cancellationToken)
	{
		if (await this.EnsureCharsAsync(1, append: true, cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
		{
			if (this._chars[this._charPos + 1] == 'u')
			{
				await this.ParseNullAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				return;
			}
			this._charPos += 2;
			throw this.CreateUnexpectedCharacterException(this._chars[this._charPos - 1]);
		}
		this._charPos = this._charsUsed;
		throw base.CreateUnexpectedEndException();
	}

	private async Task ReadFinishedAsync(CancellationToken cancellationToken)
	{
		if (await this.EnsureCharsAsync(0, append: false, cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
		{
			await this.EatWhitespaceAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			if (this._isEndOfFile)
			{
				base.SetToken(JsonToken.None);
				return;
			}
			if (this._chars[this._charPos] != '/')
			{
				throw JsonReaderException.Create(this, "Additional text encountered after finished reading JSON content: {0}.".FormatWith(CultureInfo.InvariantCulture, this._chars[this._charPos]));
			}
			await this.ParseCommentAsync(setToken: false, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		base.SetToken(JsonToken.None);
	}

	private async Task<object?> ReadStringValueAsync(ReadType readType, CancellationToken cancellationToken)
	{
		this.EnsureBuffer();
		switch (base._currentState)
		{
		case State.PostValue:
			if (await this.ParsePostValueAsync(ignoreComments: true, cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
			{
				return null;
			}
			goto case State.Start;
		case State.Start:
		case State.Property:
		case State.ArrayStart:
		case State.Array:
		case State.ConstructorStart:
		case State.Constructor:
			while (true)
			{
				char c = this._chars[this._charPos];
				switch (c)
				{
				case '\0':
					if (await this.ReadNullCharAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
					{
						base.SetToken(JsonToken.None, null, updateIndex: false);
						return null;
					}
					break;
				case '"':
				case '\'':
					await this.ParseStringAsync(c, readType, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
					return this.FinishReadQuotedStringValue(readType);
				case '-':
					if (await this.EnsureCharsAsync(1, append: true, cancellationToken).ConfigureAwait(continueOnCapturedContext: false) && this._chars[this._charPos + 1] == 'I')
					{
						return this.ParseNumberNegativeInfinity(readType);
					}
					await this.ParseNumberAsync(readType, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
					return this.Value;
				case '.':
				case '0':
				case '1':
				case '2':
				case '3':
				case '4':
				case '5':
				case '6':
				case '7':
				case '8':
				case '9':
					if (readType != ReadType.ReadAsString)
					{
						this._charPos++;
						throw this.CreateUnexpectedCharacterException(c);
					}
					await this.ParseNumberAsync(ReadType.ReadAsString, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
					return this.Value;
				case 'f':
				case 't':
				{
					if (readType != ReadType.ReadAsString)
					{
						this._charPos++;
						throw this.CreateUnexpectedCharacterException(c);
					}
					string expected = ((c == 't') ? JsonConvert.True : JsonConvert.False);
					if (!(await this.MatchValueWithTrailingSeparatorAsync(expected, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)))
					{
						throw this.CreateUnexpectedCharacterException(this._chars[this._charPos]);
					}
					base.SetToken(JsonToken.String, expected);
					return expected;
				}
				case 'I':
					return await this.ParseNumberPositiveInfinityAsync(readType, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				case 'N':
					return await this.ParseNumberNaNAsync(readType, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				case 'n':
					await this.HandleNullAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
					return null;
				case '/':
					await this.ParseCommentAsync(setToken: false, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
					break;
				case ',':
					this.ProcessValueComma();
					break;
				case ']':
					this._charPos++;
					if (base._currentState == State.Array || base._currentState == State.ArrayStart || base._currentState == State.PostValue)
					{
						base.SetToken(JsonToken.EndArray);
						return null;
					}
					throw this.CreateUnexpectedCharacterException(c);
				case '\r':
					await this.ProcessCarriageReturnAsync(append: false, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
					break;
				case '\n':
					this.ProcessLineFeed();
					break;
				case '\t':
				case ' ':
					this._charPos++;
					break;
				default:
					this._charPos++;
					if (!char.IsWhiteSpace(c))
					{
						throw this.CreateUnexpectedCharacterException(c);
					}
					break;
				}
			}
		case State.Finished:
			await this.ReadFinishedAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			return null;
		default:
			throw JsonReaderException.Create(this, "Unexpected state: {0}.".FormatWith(CultureInfo.InvariantCulture, base.CurrentState));
		}
	}

	private async Task<object?> ReadNumberValueAsync(ReadType readType, CancellationToken cancellationToken)
	{
		this.EnsureBuffer();
		switch (base._currentState)
		{
		case State.PostValue:
			if (await this.ParsePostValueAsync(ignoreComments: true, cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
			{
				return null;
			}
			goto case State.Start;
		case State.Start:
		case State.Property:
		case State.ArrayStart:
		case State.Array:
		case State.ConstructorStart:
		case State.Constructor:
			while (true)
			{
				char c = this._chars[this._charPos];
				switch (c)
				{
				case '\0':
					if (await this.ReadNullCharAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
					{
						base.SetToken(JsonToken.None, null, updateIndex: false);
						return null;
					}
					break;
				case '"':
				case '\'':
					await this.ParseStringAsync(c, readType, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
					return this.FinishReadQuotedNumber(readType);
				case 'n':
					await this.HandleNullAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
					return null;
				case 'N':
					return await this.ParseNumberNaNAsync(readType, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				case 'I':
					return await this.ParseNumberPositiveInfinityAsync(readType, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				case '-':
					if (await this.EnsureCharsAsync(1, append: true, cancellationToken).ConfigureAwait(continueOnCapturedContext: false) && this._chars[this._charPos + 1] == 'I')
					{
						return await this.ParseNumberNegativeInfinityAsync(readType, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
					}
					await this.ParseNumberAsync(readType, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
					return this.Value;
				case '.':
				case '0':
				case '1':
				case '2':
				case '3':
				case '4':
				case '5':
				case '6':
				case '7':
				case '8':
				case '9':
					await this.ParseNumberAsync(readType, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
					return this.Value;
				case '/':
					await this.ParseCommentAsync(setToken: false, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
					break;
				case ',':
					this.ProcessValueComma();
					break;
				case ']':
					this._charPos++;
					if (base._currentState == State.Array || base._currentState == State.ArrayStart || base._currentState == State.PostValue)
					{
						base.SetToken(JsonToken.EndArray);
						return null;
					}
					throw this.CreateUnexpectedCharacterException(c);
				case '\r':
					await this.ProcessCarriageReturnAsync(append: false, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
					break;
				case '\n':
					this.ProcessLineFeed();
					break;
				case '\t':
				case ' ':
					this._charPos++;
					break;
				default:
					this._charPos++;
					if (!char.IsWhiteSpace(c))
					{
						throw this.CreateUnexpectedCharacterException(c);
					}
					break;
				}
			}
		case State.Finished:
			await this.ReadFinishedAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			return null;
		default:
			throw JsonReaderException.Create(this, "Unexpected state: {0}.".FormatWith(CultureInfo.InvariantCulture, base.CurrentState));
		}
	}

	/// <summary>
	/// Asynchronously reads the next JSON token from the source as a <see cref="T:System.Nullable`1" /> of <see cref="T:System.Boolean" />.
	/// </summary>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task`1" /> that represents the asynchronous read. The <see cref="P:System.Threading.Tasks.Task`1.Result" />
	/// property returns the <see cref="T:System.Nullable`1" /> of <see cref="T:System.Boolean" />. This result will be <c>null</c> at the end of an array.</returns>
	/// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
	/// execute synchronously, returning an already-completed task.</remarks>
	public override Task<bool?> ReadAsBooleanAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!this._safeAsync)
		{
			return base.ReadAsBooleanAsync(cancellationToken);
		}
		return this.DoReadAsBooleanAsync(cancellationToken);
	}

	internal async Task<bool?> DoReadAsBooleanAsync(CancellationToken cancellationToken)
	{
		this.EnsureBuffer();
		switch (base._currentState)
		{
		case State.PostValue:
			if (await this.ParsePostValueAsync(ignoreComments: true, cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
			{
				return null;
			}
			goto case State.Start;
		case State.Start:
		case State.Property:
		case State.ArrayStart:
		case State.Array:
		case State.ConstructorStart:
		case State.Constructor:
			while (true)
			{
				char c = this._chars[this._charPos];
				switch (c)
				{
				case '\0':
					if (await this.ReadNullCharAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
					{
						base.SetToken(JsonToken.None, null, updateIndex: false);
						return null;
					}
					break;
				case '"':
				case '\'':
					await this.ParseStringAsync(c, ReadType.Read, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
					return base.ReadBooleanString(this._stringReference.ToString());
				case 'n':
					await this.HandleNullAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
					return null;
				case '-':
				case '.':
				case '0':
				case '1':
				case '2':
				case '3':
				case '4':
				case '5':
				case '6':
				case '7':
				case '8':
				case '9':
				{
					await this.ParseNumberAsync(ReadType.Read, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
					bool flag = ((!(this.Value is BigInteger i)) ? Convert.ToBoolean(this.Value, CultureInfo.InvariantCulture) : (i != 0L));
					base.SetToken(JsonToken.Boolean, flag, updateIndex: false);
					return flag;
				}
				case 'f':
				case 't':
				{
					bool isTrue = c == 't';
					if (!(await this.MatchValueWithTrailingSeparatorAsync(isTrue ? JsonConvert.True : JsonConvert.False, cancellationToken).ConfigureAwait(continueOnCapturedContext: false)))
					{
						throw this.CreateUnexpectedCharacterException(this._chars[this._charPos]);
					}
					base.SetToken(JsonToken.Boolean, isTrue);
					return isTrue;
				}
				case '/':
					await this.ParseCommentAsync(setToken: false, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
					break;
				case ',':
					this.ProcessValueComma();
					break;
				case ']':
					this._charPos++;
					if (base._currentState == State.Array || base._currentState == State.ArrayStart || base._currentState == State.PostValue)
					{
						base.SetToken(JsonToken.EndArray);
						return null;
					}
					throw this.CreateUnexpectedCharacterException(c);
				case '\r':
					await this.ProcessCarriageReturnAsync(append: false, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
					break;
				case '\n':
					this.ProcessLineFeed();
					break;
				case '\t':
				case ' ':
					this._charPos++;
					break;
				default:
					this._charPos++;
					if (!char.IsWhiteSpace(c))
					{
						throw this.CreateUnexpectedCharacterException(c);
					}
					break;
				}
			}
		case State.Finished:
			await this.ReadFinishedAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			return null;
		default:
			throw JsonReaderException.Create(this, "Unexpected state: {0}.".FormatWith(CultureInfo.InvariantCulture, base.CurrentState));
		}
	}

	/// <summary>
	/// Asynchronously reads the next JSON token from the source as a <see cref="T:System.Byte" />[].
	/// </summary>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task`1" /> that represents the asynchronous read. The <see cref="P:System.Threading.Tasks.Task`1.Result" />
	/// property returns the <see cref="T:System.Byte" />[]. This result will be <c>null</c> at the end of an array.</returns>
	/// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
	/// execute synchronously, returning an already-completed task.</remarks>
	public override Task<byte[]?> ReadAsBytesAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!this._safeAsync)
		{
			return base.ReadAsBytesAsync(cancellationToken);
		}
		return this.DoReadAsBytesAsync(cancellationToken);
	}

	internal async Task<byte[]?> DoReadAsBytesAsync(CancellationToken cancellationToken)
	{
		this.EnsureBuffer();
		bool isWrapped = false;
		switch (base._currentState)
		{
		case State.PostValue:
			if (await this.ParsePostValueAsync(ignoreComments: true, cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
			{
				return null;
			}
			goto case State.Start;
		case State.Start:
		case State.Property:
		case State.ArrayStart:
		case State.Array:
		case State.ConstructorStart:
		case State.Constructor:
			while (true)
			{
				char c = this._chars[this._charPos];
				switch (c)
				{
				case '\0':
					if (await this.ReadNullCharAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false))
					{
						base.SetToken(JsonToken.None, null, updateIndex: false);
						return null;
					}
					break;
				case '"':
				case '\'':
				{
					await this.ParseStringAsync(c, ReadType.ReadAsBytes, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
					byte[] data = (byte[])this.Value;
					if (isWrapped)
					{
						await base.ReaderReadAndAssertAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
						if (this.TokenType != JsonToken.EndObject)
						{
							throw JsonReaderException.Create(this, "Error reading bytes. Unexpected token: {0}.".FormatWith(CultureInfo.InvariantCulture, this.TokenType));
						}
						base.SetToken(JsonToken.Bytes, data, updateIndex: false);
					}
					return data;
				}
				case '{':
					this._charPos++;
					base.SetToken(JsonToken.StartObject);
					await this.ReadIntoWrappedTypeObjectAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
					isWrapped = true;
					break;
				case '[':
					this._charPos++;
					base.SetToken(JsonToken.StartArray);
					return await base.ReadArrayIntoByteArrayAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				case 'n':
					await this.HandleNullAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
					return null;
				case '/':
					await this.ParseCommentAsync(setToken: false, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
					break;
				case ',':
					this.ProcessValueComma();
					break;
				case ']':
					this._charPos++;
					if (base._currentState == State.Array || base._currentState == State.ArrayStart || base._currentState == State.PostValue)
					{
						base.SetToken(JsonToken.EndArray);
						return null;
					}
					throw this.CreateUnexpectedCharacterException(c);
				case '\r':
					await this.ProcessCarriageReturnAsync(append: false, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
					break;
				case '\n':
					this.ProcessLineFeed();
					break;
				case '\t':
				case ' ':
					this._charPos++;
					break;
				default:
					this._charPos++;
					if (!char.IsWhiteSpace(c))
					{
						throw this.CreateUnexpectedCharacterException(c);
					}
					break;
				}
			}
		case State.Finished:
			await this.ReadFinishedAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			return null;
		default:
			throw JsonReaderException.Create(this, "Unexpected state: {0}.".FormatWith(CultureInfo.InvariantCulture, base.CurrentState));
		}
	}

	private async Task ReadIntoWrappedTypeObjectAsync(CancellationToken cancellationToken)
	{
		await base.ReaderReadAndAssertAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		if (this.Value != null && this.Value.ToString() == "$type")
		{
			await base.ReaderReadAndAssertAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			if (this.Value != null && this.Value.ToString().StartsWith("System.Byte[]", StringComparison.Ordinal))
			{
				await base.ReaderReadAndAssertAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
				if (this.Value.ToString() == "$value")
				{
					return;
				}
			}
		}
		throw JsonReaderException.Create(this, "Error reading bytes. Unexpected token: {0}.".FormatWith(CultureInfo.InvariantCulture, JsonToken.StartObject));
	}

	/// <summary>
	/// Asynchronously reads the next JSON token from the source as a <see cref="T:System.Nullable`1" /> of <see cref="T:System.DateTime" />.
	/// </summary>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task`1" /> that represents the asynchronous read. The <see cref="P:System.Threading.Tasks.Task`1.Result" />
	/// property returns the <see cref="T:System.Nullable`1" /> of <see cref="T:System.DateTime" />. This result will be <c>null</c> at the end of an array.</returns>
	/// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
	/// execute synchronously, returning an already-completed task.</remarks>
	public override Task<DateTime?> ReadAsDateTimeAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!this._safeAsync)
		{
			return base.ReadAsDateTimeAsync(cancellationToken);
		}
		return this.DoReadAsDateTimeAsync(cancellationToken);
	}

	internal async Task<DateTime?> DoReadAsDateTimeAsync(CancellationToken cancellationToken)
	{
		return (DateTime?)(await this.ReadStringValueAsync(ReadType.ReadAsDateTime, cancellationToken).ConfigureAwait(continueOnCapturedContext: false));
	}

	/// <summary>
	/// Asynchronously reads the next JSON token from the source as a <see cref="T:System.Nullable`1" /> of <see cref="T:System.DateTimeOffset" />.
	/// </summary>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task`1" /> that represents the asynchronous read. The <see cref="P:System.Threading.Tasks.Task`1.Result" />
	/// property returns the <see cref="T:System.Nullable`1" /> of <see cref="T:System.DateTimeOffset" />. This result will be <c>null</c> at the end of an array.</returns>
	/// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
	/// execute synchronously, returning an already-completed task.</remarks>
	public override Task<DateTimeOffset?> ReadAsDateTimeOffsetAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!this._safeAsync)
		{
			return base.ReadAsDateTimeOffsetAsync(cancellationToken);
		}
		return this.DoReadAsDateTimeOffsetAsync(cancellationToken);
	}

	internal async Task<DateTimeOffset?> DoReadAsDateTimeOffsetAsync(CancellationToken cancellationToken)
	{
		return (DateTimeOffset?)(await this.ReadStringValueAsync(ReadType.ReadAsDateTimeOffset, cancellationToken).ConfigureAwait(continueOnCapturedContext: false));
	}

	/// <summary>
	/// Asynchronously reads the next JSON token from the source as a <see cref="T:System.Nullable`1" /> of <see cref="T:System.Decimal" />.
	/// </summary>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task`1" /> that represents the asynchronous read. The <see cref="P:System.Threading.Tasks.Task`1.Result" />
	/// property returns the <see cref="T:System.Nullable`1" /> of <see cref="T:System.Decimal" />. This result will be <c>null</c> at the end of an array.</returns>
	/// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
	/// execute synchronously, returning an already-completed task.</remarks>
	public override Task<decimal?> ReadAsDecimalAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!this._safeAsync)
		{
			return base.ReadAsDecimalAsync(cancellationToken);
		}
		return this.DoReadAsDecimalAsync(cancellationToken);
	}

	internal async Task<decimal?> DoReadAsDecimalAsync(CancellationToken cancellationToken)
	{
		return (decimal?)(await this.ReadNumberValueAsync(ReadType.ReadAsDecimal, cancellationToken).ConfigureAwait(continueOnCapturedContext: false));
	}

	/// <summary>
	/// Asynchronously reads the next JSON token from the source as a <see cref="T:System.Nullable`1" /> of <see cref="T:System.Double" />.
	/// </summary>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task`1" /> that represents the asynchronous read. The <see cref="P:System.Threading.Tasks.Task`1.Result" />
	/// property returns the <see cref="T:System.Nullable`1" /> of <see cref="T:System.Double" />. This result will be <c>null</c> at the end of an array.</returns>
	/// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
	/// execute synchronously, returning an already-completed task.</remarks>
	public override Task<double?> ReadAsDoubleAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!this._safeAsync)
		{
			return base.ReadAsDoubleAsync(cancellationToken);
		}
		return this.DoReadAsDoubleAsync(cancellationToken);
	}

	internal async Task<double?> DoReadAsDoubleAsync(CancellationToken cancellationToken)
	{
		return (double?)(await this.ReadNumberValueAsync(ReadType.ReadAsDouble, cancellationToken).ConfigureAwait(continueOnCapturedContext: false));
	}

	/// <summary>
	/// Asynchronously reads the next JSON token from the source as a <see cref="T:System.Nullable`1" /> of <see cref="T:System.Int32" />.
	/// </summary>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task`1" /> that represents the asynchronous read. The <see cref="P:System.Threading.Tasks.Task`1.Result" />
	/// property returns the <see cref="T:System.Nullable`1" /> of <see cref="T:System.Int32" />. This result will be <c>null</c> at the end of an array.</returns>
	/// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
	/// execute synchronously, returning an already-completed task.</remarks>
	public override Task<int?> ReadAsInt32Async(CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!this._safeAsync)
		{
			return base.ReadAsInt32Async(cancellationToken);
		}
		return this.DoReadAsInt32Async(cancellationToken);
	}

	internal async Task<int?> DoReadAsInt32Async(CancellationToken cancellationToken)
	{
		return (int?)(await this.ReadNumberValueAsync(ReadType.ReadAsInt32, cancellationToken).ConfigureAwait(continueOnCapturedContext: false));
	}

	/// <summary>
	/// Asynchronously reads the next JSON token from the source as a <see cref="T:System.String" />.
	/// </summary>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="P:System.Threading.CancellationToken.None" />.</param>
	/// <returns>A <see cref="T:System.Threading.Tasks.Task`1" /> that represents the asynchronous read. The <see cref="P:System.Threading.Tasks.Task`1.Result" />
	/// property returns the <see cref="T:System.String" />. This result will be <c>null</c> at the end of an array.</returns>
	/// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
	/// execute synchronously, returning an already-completed task.</remarks>
	public override Task<string?> ReadAsStringAsync(CancellationToken cancellationToken = default(CancellationToken))
	{
		if (!this._safeAsync)
		{
			return base.ReadAsStringAsync(cancellationToken);
		}
		return this.DoReadAsStringAsync(cancellationToken);
	}

	internal async Task<string?> DoReadAsStringAsync(CancellationToken cancellationToken)
	{
		return (string)(await this.ReadStringValueAsync(ReadType.ReadAsString, cancellationToken).ConfigureAwait(continueOnCapturedContext: false));
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:Newtonsoft.Json.JsonTextReader" /> class with the specified <see cref="T:System.IO.TextReader" />.
	/// </summary>
	/// <param name="reader">The <see cref="T:System.IO.TextReader" /> containing the JSON data to read.</param>
	public JsonTextReader(TextReader reader)
	{
		if (reader == null)
		{
			throw new ArgumentNullException("reader");
		}
		this._reader = reader;
		this._lineNumber = 1;
		this._safeAsync = base.GetType() == typeof(JsonTextReader);
	}

	private void EnsureBufferNotEmpty()
	{
		if (this._stringBuffer.IsEmpty)
		{
			this._stringBuffer = new StringBuffer(this._arrayPool, 1024);
		}
	}

	private void SetNewLine(bool hasNextChar)
	{
		if (hasNextChar && this._chars[this._charPos] == '\n')
		{
			this._charPos++;
		}
		this.OnNewLine(this._charPos);
	}

	private void OnNewLine(int pos)
	{
		this._lineNumber++;
		this._lineStartPos = pos;
	}

	private void ParseString(char quote, ReadType readType)
	{
		this._charPos++;
		this.ShiftBufferIfNeeded();
		this.ReadStringIntoBuffer(quote);
		this.ParseReadString(quote, readType);
	}

	private void ParseReadString(char quote, ReadType readType)
	{
		base.SetPostValueState(updateIndex: true);
		switch (readType)
		{
		case ReadType.ReadAsBytes:
		{
			Guid g;
			byte[] value2 = ((this._stringReference.Length == 0) ? CollectionUtils.ArrayEmpty<byte>() : ((this._stringReference.Length != 36 || !ConvertUtils.TryConvertGuid(this._stringReference.ToString(), out g)) ? Convert.FromBase64CharArray(this._stringReference.Chars, this._stringReference.StartIndex, this._stringReference.Length) : g.ToByteArray()));
			base.SetToken(JsonToken.Bytes, value2, updateIndex: false);
			return;
		}
		case ReadType.ReadAsString:
		{
			string value = this._stringReference.ToString();
			base.SetToken(JsonToken.String, value, updateIndex: false);
			base._quoteChar = quote;
			return;
		}
		case ReadType.ReadAsInt32:
		case ReadType.ReadAsDecimal:
		case ReadType.ReadAsBoolean:
			return;
		}
		if (base._dateParseHandling != DateParseHandling.None)
		{
			DateTimeOffset dt2;
			if (readType switch
			{
				ReadType.ReadAsDateTime => 1, 
				ReadType.ReadAsDateTimeOffset => 2, 
				_ => (int)base._dateParseHandling, 
			} == 1)
			{
				if (DateTimeUtils.TryParseDateTime(this._stringReference, base.DateTimeZoneHandling, base.DateFormatString, base.Culture, out var dt))
				{
					base.SetToken(JsonToken.Date, dt, updateIndex: false);
					return;
				}
			}
			else if (DateTimeUtils.TryParseDateTimeOffset(this._stringReference, base.DateFormatString, base.Culture, out dt2))
			{
				base.SetToken(JsonToken.Date, dt2, updateIndex: false);
				return;
			}
		}
		base.SetToken(JsonToken.String, this._stringReference.ToString(), updateIndex: false);
		base._quoteChar = quote;
	}

	private static void BlockCopyChars(char[] src, int srcOffset, char[] dst, int dstOffset, int count)
	{
		Buffer.BlockCopy(src, srcOffset * 2, dst, dstOffset * 2, count * 2);
	}

	private void ShiftBufferIfNeeded()
	{
		int num = this._chars.Length;
		if ((double)(num - this._charPos) <= (double)num * 0.1 || num >= 1073741823)
		{
			int num2 = this._charsUsed - this._charPos;
			if (num2 > 0)
			{
				JsonTextReader.BlockCopyChars(this._chars, this._charPos, this._chars, 0, num2);
			}
			this._lineStartPos -= this._charPos;
			this._charPos = 0;
			this._charsUsed = num2;
			this._chars[this._charsUsed] = '\0';
		}
	}

	private int ReadData(bool append)
	{
		return this.ReadData(append, 0);
	}

	private void PrepareBufferForReadData(bool append, int charsRequired)
	{
		if (this._charsUsed + charsRequired < this._chars.Length - 1)
		{
			return;
		}
		if (append)
		{
			int num = this._chars.Length * 2;
			int minSize = Math.Max((num < 0) ? int.MaxValue : num, this._charsUsed + charsRequired + 1);
			char[] array = BufferUtils.RentBuffer(this._arrayPool, minSize);
			JsonTextReader.BlockCopyChars(this._chars, 0, array, 0, this._chars.Length);
			BufferUtils.ReturnBuffer(this._arrayPool, this._chars);
			this._chars = array;
			return;
		}
		int num2 = this._charsUsed - this._charPos;
		if (num2 + charsRequired + 1 >= this._chars.Length)
		{
			char[] array2 = BufferUtils.RentBuffer(this._arrayPool, num2 + charsRequired + 1);
			if (num2 > 0)
			{
				JsonTextReader.BlockCopyChars(this._chars, this._charPos, array2, 0, num2);
			}
			BufferUtils.ReturnBuffer(this._arrayPool, this._chars);
			this._chars = array2;
		}
		else if (num2 > 0)
		{
			JsonTextReader.BlockCopyChars(this._chars, this._charPos, this._chars, 0, num2);
		}
		this._lineStartPos -= this._charPos;
		this._charPos = 0;
		this._charsUsed = num2;
	}

	private int ReadData(bool append, int charsRequired)
	{
		if (this._isEndOfFile)
		{
			return 0;
		}
		this.PrepareBufferForReadData(append, charsRequired);
		int count = this._chars.Length - this._charsUsed - 1;
		int num = this._reader.Read(this._chars, this._charsUsed, count);
		this._charsUsed += num;
		if (num == 0)
		{
			this._isEndOfFile = true;
		}
		this._chars[this._charsUsed] = '\0';
		return num;
	}

	private bool EnsureChars(int relativePosition, bool append)
	{
		if (this._charPos + relativePosition >= this._charsUsed)
		{
			return this.ReadChars(relativePosition, append);
		}
		return true;
	}

	private bool ReadChars(int relativePosition, bool append)
	{
		if (this._isEndOfFile)
		{
			return false;
		}
		int num = this._charPos + relativePosition - this._charsUsed + 1;
		int num2 = 0;
		do
		{
			int num3 = this.ReadData(append, num - num2);
			if (num3 == 0)
			{
				break;
			}
			num2 += num3;
		}
		while (num2 < num);
		if (num2 < num)
		{
			return false;
		}
		return true;
	}

	/// <summary>
	/// Reads the next JSON token from the underlying <see cref="T:System.IO.TextReader" />.
	/// </summary>
	/// <returns>
	/// <c>true</c> if the next token was read successfully; <c>false</c> if there are no more tokens to read.
	/// </returns>
	public override bool Read()
	{
		this.EnsureBuffer();
		do
		{
			switch (base._currentState)
			{
			case State.Start:
			case State.Property:
			case State.ArrayStart:
			case State.Array:
			case State.ConstructorStart:
			case State.Constructor:
				return this.ParseValue();
			case State.ObjectStart:
			case State.Object:
				return this.ParseObject();
			case State.PostValue:
				break;
			case State.Finished:
				if (this.EnsureChars(0, append: false))
				{
					this.EatWhitespace();
					if (this._isEndOfFile)
					{
						base.SetToken(JsonToken.None);
						return false;
					}
					if (this._chars[this._charPos] == '/')
					{
						this.ParseComment(setToken: true);
						return true;
					}
					throw JsonReaderException.Create(this, "Additional text encountered after finished reading JSON content: {0}.".FormatWith(CultureInfo.InvariantCulture, this._chars[this._charPos]));
				}
				base.SetToken(JsonToken.None);
				return false;
			default:
				throw JsonReaderException.Create(this, "Unexpected state: {0}.".FormatWith(CultureInfo.InvariantCulture, base.CurrentState));
			}
		}
		while (!this.ParsePostValue(ignoreComments: false));
		return true;
	}

	/// <summary>
	/// Reads the next JSON token from the underlying <see cref="T:System.IO.TextReader" /> as a <see cref="T:System.Nullable`1" /> of <see cref="T:System.Int32" />.
	/// </summary>
	/// <returns>A <see cref="T:System.Nullable`1" /> of <see cref="T:System.Int32" />. This method will return <c>null</c> at the end of an array.</returns>
	public override int? ReadAsInt32()
	{
		return (int?)this.ReadNumberValue(ReadType.ReadAsInt32);
	}

	/// <summary>
	/// Reads the next JSON token from the underlying <see cref="T:System.IO.TextReader" /> as a <see cref="T:System.Nullable`1" /> of <see cref="T:System.DateTime" />.
	/// </summary>
	/// <returns>A <see cref="T:System.Nullable`1" /> of <see cref="T:System.DateTime" />. This method will return <c>null</c> at the end of an array.</returns>
	public override DateTime? ReadAsDateTime()
	{
		return (DateTime?)this.ReadStringValue(ReadType.ReadAsDateTime);
	}

	/// <summary>
	/// Reads the next JSON token from the underlying <see cref="T:System.IO.TextReader" /> as a <see cref="T:System.String" />.
	/// </summary>
	/// <returns>A <see cref="T:System.String" />. This method will return <c>null</c> at the end of an array.</returns>
	public override string? ReadAsString()
	{
		return (string)this.ReadStringValue(ReadType.ReadAsString);
	}

	/// <summary>
	/// Reads the next JSON token from the underlying <see cref="T:System.IO.TextReader" /> as a <see cref="T:System.Byte" />[].
	/// </summary>
	/// <returns>A <see cref="T:System.Byte" />[] or <c>null</c> if the next JSON token is null. This method will return <c>null</c> at the end of an array.</returns>
	public override byte[]? ReadAsBytes()
	{
		this.EnsureBuffer();
		bool flag = false;
		switch (base._currentState)
		{
		case State.PostValue:
			if (this.ParsePostValue(ignoreComments: true))
			{
				return null;
			}
			goto case State.Start;
		case State.Start:
		case State.Property:
		case State.ArrayStart:
		case State.Array:
		case State.ConstructorStart:
		case State.Constructor:
			while (true)
			{
				char c = this._chars[this._charPos];
				switch (c)
				{
				case '\0':
					if (this.ReadNullChar())
					{
						base.SetToken(JsonToken.None, null, updateIndex: false);
						return null;
					}
					break;
				case '"':
				case '\'':
				{
					this.ParseString(c, ReadType.ReadAsBytes);
					byte[] array = (byte[])this.Value;
					if (flag)
					{
						base.ReaderReadAndAssert();
						if (this.TokenType != JsonToken.EndObject)
						{
							throw JsonReaderException.Create(this, "Error reading bytes. Unexpected token: {0}.".FormatWith(CultureInfo.InvariantCulture, this.TokenType));
						}
						base.SetToken(JsonToken.Bytes, array, updateIndex: false);
					}
					return array;
				}
				case '{':
					this._charPos++;
					base.SetToken(JsonToken.StartObject);
					base.ReadIntoWrappedTypeObject();
					flag = true;
					break;
				case '[':
					this._charPos++;
					base.SetToken(JsonToken.StartArray);
					return base.ReadArrayIntoByteArray();
				case 'n':
					this.HandleNull();
					return null;
				case '/':
					this.ParseComment(setToken: false);
					break;
				case ',':
					this.ProcessValueComma();
					break;
				case ']':
					this._charPos++;
					if (base._currentState == State.Array || base._currentState == State.ArrayStart || base._currentState == State.PostValue)
					{
						base.SetToken(JsonToken.EndArray);
						return null;
					}
					throw this.CreateUnexpectedCharacterException(c);
				case '\r':
					this.ProcessCarriageReturn(append: false);
					break;
				case '\n':
					this.ProcessLineFeed();
					break;
				case '\t':
				case ' ':
					this._charPos++;
					break;
				default:
					this._charPos++;
					if (!char.IsWhiteSpace(c))
					{
						throw this.CreateUnexpectedCharacterException(c);
					}
					break;
				}
			}
		case State.Finished:
			this.ReadFinished();
			return null;
		default:
			throw JsonReaderException.Create(this, "Unexpected state: {0}.".FormatWith(CultureInfo.InvariantCulture, base.CurrentState));
		}
	}

	private object? ReadStringValue(ReadType readType)
	{
		this.EnsureBuffer();
		switch (base._currentState)
		{
		case State.PostValue:
			if (this.ParsePostValue(ignoreComments: true))
			{
				return null;
			}
			goto case State.Start;
		case State.Start:
		case State.Property:
		case State.ArrayStart:
		case State.Array:
		case State.ConstructorStart:
		case State.Constructor:
			while (true)
			{
				char c = this._chars[this._charPos];
				switch (c)
				{
				case '\0':
					if (this.ReadNullChar())
					{
						base.SetToken(JsonToken.None, null, updateIndex: false);
						return null;
					}
					break;
				case '"':
				case '\'':
					this.ParseString(c, readType);
					return this.FinishReadQuotedStringValue(readType);
				case '-':
					if (this.EnsureChars(1, append: true) && this._chars[this._charPos + 1] == 'I')
					{
						return this.ParseNumberNegativeInfinity(readType);
					}
					this.ParseNumber(readType);
					return this.Value;
				case '.':
				case '0':
				case '1':
				case '2':
				case '3':
				case '4':
				case '5':
				case '6':
				case '7':
				case '8':
				case '9':
					if (readType != ReadType.ReadAsString)
					{
						this._charPos++;
						throw this.CreateUnexpectedCharacterException(c);
					}
					this.ParseNumber(ReadType.ReadAsString);
					return this.Value;
				case 'f':
				case 't':
				{
					if (readType != ReadType.ReadAsString)
					{
						this._charPos++;
						throw this.CreateUnexpectedCharacterException(c);
					}
					string text = ((c == 't') ? JsonConvert.True : JsonConvert.False);
					if (!this.MatchValueWithTrailingSeparator(text))
					{
						throw this.CreateUnexpectedCharacterException(this._chars[this._charPos]);
					}
					base.SetToken(JsonToken.String, text);
					return text;
				}
				case 'I':
					return this.ParseNumberPositiveInfinity(readType);
				case 'N':
					return this.ParseNumberNaN(readType);
				case 'n':
					this.HandleNull();
					return null;
				case '/':
					this.ParseComment(setToken: false);
					break;
				case ',':
					this.ProcessValueComma();
					break;
				case ']':
					this._charPos++;
					if (base._currentState == State.Array || base._currentState == State.ArrayStart || base._currentState == State.PostValue)
					{
						base.SetToken(JsonToken.EndArray);
						return null;
					}
					throw this.CreateUnexpectedCharacterException(c);
				case '\r':
					this.ProcessCarriageReturn(append: false);
					break;
				case '\n':
					this.ProcessLineFeed();
					break;
				case '\t':
				case ' ':
					this._charPos++;
					break;
				default:
					this._charPos++;
					if (!char.IsWhiteSpace(c))
					{
						throw this.CreateUnexpectedCharacterException(c);
					}
					break;
				}
			}
		case State.Finished:
			this.ReadFinished();
			return null;
		default:
			throw JsonReaderException.Create(this, "Unexpected state: {0}.".FormatWith(CultureInfo.InvariantCulture, base.CurrentState));
		}
	}

	private object? FinishReadQuotedStringValue(ReadType readType)
	{
		switch (readType)
		{
		case ReadType.ReadAsBytes:
		case ReadType.ReadAsString:
			return this.Value;
		case ReadType.ReadAsDateTime:
			if (this.Value is DateTime dateTime)
			{
				return dateTime;
			}
			return base.ReadDateTimeString((string)this.Value);
		case ReadType.ReadAsDateTimeOffset:
			if (this.Value is DateTimeOffset dateTimeOffset)
			{
				return dateTimeOffset;
			}
			return base.ReadDateTimeOffsetString((string)this.Value);
		default:
			throw new ArgumentOutOfRangeException("readType");
		}
	}

	private JsonReaderException CreateUnexpectedCharacterException(char c)
	{
		return JsonReaderException.Create(this, "Unexpected character encountered while parsing value: {0}.".FormatWith(CultureInfo.InvariantCulture, c));
	}

	/// <summary>
	/// Reads the next JSON token from the underlying <see cref="T:System.IO.TextReader" /> as a <see cref="T:System.Nullable`1" /> of <see cref="T:System.Boolean" />.
	/// </summary>
	/// <returns>A <see cref="T:System.Nullable`1" /> of <see cref="T:System.Boolean" />. This method will return <c>null</c> at the end of an array.</returns>
	public override bool? ReadAsBoolean()
	{
		this.EnsureBuffer();
		switch (base._currentState)
		{
		case State.PostValue:
			if (this.ParsePostValue(ignoreComments: true))
			{
				return null;
			}
			goto case State.Start;
		case State.Start:
		case State.Property:
		case State.ArrayStart:
		case State.Array:
		case State.ConstructorStart:
		case State.Constructor:
			while (true)
			{
				char c = this._chars[this._charPos];
				switch (c)
				{
				case '\0':
					if (this.ReadNullChar())
					{
						base.SetToken(JsonToken.None, null, updateIndex: false);
						return null;
					}
					break;
				case '"':
				case '\'':
					this.ParseString(c, ReadType.Read);
					return base.ReadBooleanString(this._stringReference.ToString());
				case 'n':
					this.HandleNull();
					return null;
				case '-':
				case '.':
				case '0':
				case '1':
				case '2':
				case '3':
				case '4':
				case '5':
				case '6':
				case '7':
				case '8':
				case '9':
				{
					this.ParseNumber(ReadType.Read);
					bool flag2 = ((!(this.Value is BigInteger bigInteger)) ? Convert.ToBoolean(this.Value, CultureInfo.InvariantCulture) : (bigInteger != 0L));
					base.SetToken(JsonToken.Boolean, flag2, updateIndex: false);
					return flag2;
				}
				case 'f':
				case 't':
				{
					bool flag = c == 't';
					string value = (flag ? JsonConvert.True : JsonConvert.False);
					if (!this.MatchValueWithTrailingSeparator(value))
					{
						throw this.CreateUnexpectedCharacterException(this._chars[this._charPos]);
					}
					base.SetToken(JsonToken.Boolean, BoxedPrimitives.Get(flag));
					return flag;
				}
				case '/':
					this.ParseComment(setToken: false);
					break;
				case ',':
					this.ProcessValueComma();
					break;
				case ']':
					this._charPos++;
					if (base._currentState == State.Array || base._currentState == State.ArrayStart || base._currentState == State.PostValue)
					{
						base.SetToken(JsonToken.EndArray);
						return null;
					}
					throw this.CreateUnexpectedCharacterException(c);
				case '\r':
					this.ProcessCarriageReturn(append: false);
					break;
				case '\n':
					this.ProcessLineFeed();
					break;
				case '\t':
				case ' ':
					this._charPos++;
					break;
				default:
					this._charPos++;
					if (!char.IsWhiteSpace(c))
					{
						throw this.CreateUnexpectedCharacterException(c);
					}
					break;
				}
			}
		case State.Finished:
			this.ReadFinished();
			return null;
		default:
			throw JsonReaderException.Create(this, "Unexpected state: {0}.".FormatWith(CultureInfo.InvariantCulture, base.CurrentState));
		}
	}

	private void ProcessValueComma()
	{
		this._charPos++;
		if (base._currentState != State.PostValue)
		{
			base.SetToken(JsonToken.Undefined);
			JsonReaderException ex = this.CreateUnexpectedCharacterException(',');
			this._charPos--;
			throw ex;
		}
		base.SetStateBasedOnCurrent();
	}

	private object? ReadNumberValue(ReadType readType)
	{
		this.EnsureBuffer();
		switch (base._currentState)
		{
		case State.PostValue:
			if (this.ParsePostValue(ignoreComments: true))
			{
				return null;
			}
			goto case State.Start;
		case State.Start:
		case State.Property:
		case State.ArrayStart:
		case State.Array:
		case State.ConstructorStart:
		case State.Constructor:
			while (true)
			{
				char c = this._chars[this._charPos];
				switch (c)
				{
				case '\0':
					if (this.ReadNullChar())
					{
						base.SetToken(JsonToken.None, null, updateIndex: false);
						return null;
					}
					break;
				case '"':
				case '\'':
					this.ParseString(c, readType);
					return this.FinishReadQuotedNumber(readType);
				case 'n':
					this.HandleNull();
					return null;
				case 'N':
					return this.ParseNumberNaN(readType);
				case 'I':
					return this.ParseNumberPositiveInfinity(readType);
				case '-':
					if (this.EnsureChars(1, append: true) && this._chars[this._charPos + 1] == 'I')
					{
						return this.ParseNumberNegativeInfinity(readType);
					}
					this.ParseNumber(readType);
					return this.Value;
				case '.':
				case '0':
				case '1':
				case '2':
				case '3':
				case '4':
				case '5':
				case '6':
				case '7':
				case '8':
				case '9':
					this.ParseNumber(readType);
					return this.Value;
				case '/':
					this.ParseComment(setToken: false);
					break;
				case ',':
					this.ProcessValueComma();
					break;
				case ']':
					this._charPos++;
					if (base._currentState == State.Array || base._currentState == State.ArrayStart || base._currentState == State.PostValue)
					{
						base.SetToken(JsonToken.EndArray);
						return null;
					}
					throw this.CreateUnexpectedCharacterException(c);
				case '\r':
					this.ProcessCarriageReturn(append: false);
					break;
				case '\n':
					this.ProcessLineFeed();
					break;
				case '\t':
				case ' ':
					this._charPos++;
					break;
				default:
					this._charPos++;
					if (!char.IsWhiteSpace(c))
					{
						throw this.CreateUnexpectedCharacterException(c);
					}
					break;
				}
			}
		case State.Finished:
			this.ReadFinished();
			return null;
		default:
			throw JsonReaderException.Create(this, "Unexpected state: {0}.".FormatWith(CultureInfo.InvariantCulture, base.CurrentState));
		}
	}

	private object? FinishReadQuotedNumber(ReadType readType)
	{
		return readType switch
		{
			ReadType.ReadAsInt32 => base.ReadInt32String(this._stringReference.ToString()), 
			ReadType.ReadAsDecimal => base.ReadDecimalString(this._stringReference.ToString()), 
			ReadType.ReadAsDouble => base.ReadDoubleString(this._stringReference.ToString()), 
			_ => throw new ArgumentOutOfRangeException("readType"), 
		};
	}

	/// <summary>
	/// Reads the next JSON token from the underlying <see cref="T:System.IO.TextReader" /> as a <see cref="T:System.Nullable`1" /> of <see cref="T:System.DateTimeOffset" />.
	/// </summary>
	/// <returns>A <see cref="T:System.Nullable`1" /> of <see cref="T:System.DateTimeOffset" />. This method will return <c>null</c> at the end of an array.</returns>
	public override DateTimeOffset? ReadAsDateTimeOffset()
	{
		return (DateTimeOffset?)this.ReadStringValue(ReadType.ReadAsDateTimeOffset);
	}

	/// <summary>
	/// Reads the next JSON token from the underlying <see cref="T:System.IO.TextReader" /> as a <see cref="T:System.Nullable`1" /> of <see cref="T:System.Decimal" />.
	/// </summary>
	/// <returns>A <see cref="T:System.Nullable`1" /> of <see cref="T:System.Decimal" />. This method will return <c>null</c> at the end of an array.</returns>
	public override decimal? ReadAsDecimal()
	{
		return (decimal?)this.ReadNumberValue(ReadType.ReadAsDecimal);
	}

	/// <summary>
	/// Reads the next JSON token from the underlying <see cref="T:System.IO.TextReader" /> as a <see cref="T:System.Nullable`1" /> of <see cref="T:System.Double" />.
	/// </summary>
	/// <returns>A <see cref="T:System.Nullable`1" /> of <see cref="T:System.Double" />. This method will return <c>null</c> at the end of an array.</returns>
	public override double? ReadAsDouble()
	{
		return (double?)this.ReadNumberValue(ReadType.ReadAsDouble);
	}

	private void HandleNull()
	{
		if (this.EnsureChars(1, append: true))
		{
			if (this._chars[this._charPos + 1] == 'u')
			{
				this.ParseNull();
				return;
			}
			this._charPos += 2;
			throw this.CreateUnexpectedCharacterException(this._chars[this._charPos - 1]);
		}
		this._charPos = this._charsUsed;
		throw base.CreateUnexpectedEndException();
	}

	private void ReadFinished()
	{
		if (this.EnsureChars(0, append: false))
		{
			this.EatWhitespace();
			if (this._isEndOfFile)
			{
				return;
			}
			if (this._chars[this._charPos] != '/')
			{
				throw JsonReaderException.Create(this, "Additional text encountered after finished reading JSON content: {0}.".FormatWith(CultureInfo.InvariantCulture, this._chars[this._charPos]));
			}
			this.ParseComment(setToken: false);
		}
		base.SetToken(JsonToken.None);
	}

	private bool ReadNullChar()
	{
		if (this._charsUsed == this._charPos)
		{
			if (this.ReadData(append: false) == 0)
			{
				this._isEndOfFile = true;
				return true;
			}
		}
		else
		{
			this._charPos++;
		}
		return false;
	}

	private void EnsureBuffer()
	{
		if (this._chars == null)
		{
			this._chars = BufferUtils.RentBuffer(this._arrayPool, 1024);
			this._chars[0] = '\0';
		}
	}

	private void ReadStringIntoBuffer(char quote)
	{
		int num = this._charPos;
		int charPos = this._charPos;
		int lastWritePosition = this._charPos;
		this._stringBuffer.Position = 0;
		while (true)
		{
			switch (this._chars[num++])
			{
			case '\0':
				if (this._charsUsed == num - 1)
				{
					num--;
					if (this.ReadData(append: true) == 0)
					{
						this._charPos = num;
						throw JsonReaderException.Create(this, "Unterminated string. Expected delimiter: {0}.".FormatWith(CultureInfo.InvariantCulture, quote));
					}
				}
				break;
			case '\\':
			{
				this._charPos = num;
				if (!this.EnsureChars(0, append: true))
				{
					throw JsonReaderException.Create(this, "Unterminated string. Expected delimiter: {0}.".FormatWith(CultureInfo.InvariantCulture, quote));
				}
				int writeToPosition = num - 1;
				char c = this._chars[num];
				num++;
				char c2;
				switch (c)
				{
				case 'b':
					c2 = '\b';
					break;
				case 't':
					c2 = '\t';
					break;
				case 'n':
					c2 = '\n';
					break;
				case 'f':
					c2 = '\f';
					break;
				case 'r':
					c2 = '\r';
					break;
				case '\\':
					c2 = '\\';
					break;
				case '"':
				case '\'':
				case '/':
					c2 = c;
					break;
				case 'u':
					this._charPos = num;
					c2 = this.ParseUnicode();
					if (StringUtils.IsLowSurrogate(c2))
					{
						c2 = '\ufffd';
					}
					else if (StringUtils.IsHighSurrogate(c2))
					{
						bool flag;
						do
						{
							flag = false;
							if (this.EnsureChars(2, append: true) && this._chars[this._charPos] == '\\' && this._chars[this._charPos + 1] == 'u')
							{
								char writeChar = c2;
								this._charPos += 2;
								c2 = this.ParseUnicode();
								if (!StringUtils.IsLowSurrogate(c2))
								{
									if (StringUtils.IsHighSurrogate(c2))
									{
										writeChar = '\ufffd';
										flag = true;
									}
									else
									{
										writeChar = '\ufffd';
									}
								}
								this.EnsureBufferNotEmpty();
								this.WriteCharToBuffer(writeChar, lastWritePosition, writeToPosition);
								lastWritePosition = this._charPos;
							}
							else
							{
								c2 = '\ufffd';
							}
						}
						while (flag);
					}
					num = this._charPos;
					break;
				default:
					this._charPos = num;
					throw JsonReaderException.Create(this, "Bad JSON escape sequence: {0}.".FormatWith(CultureInfo.InvariantCulture, "\\" + c));
				}
				this.EnsureBufferNotEmpty();
				this.WriteCharToBuffer(c2, lastWritePosition, writeToPosition);
				lastWritePosition = num;
				break;
			}
			case '\r':
				this._charPos = num - 1;
				this.ProcessCarriageReturn(append: true);
				num = this._charPos;
				break;
			case '\n':
				this._charPos = num - 1;
				this.ProcessLineFeed();
				num = this._charPos;
				break;
			case '"':
			case '\'':
				if (this._chars[num - 1] == quote)
				{
					this.FinishReadStringIntoBuffer(num - 1, charPos, lastWritePosition);
					return;
				}
				break;
			}
		}
	}

	private void FinishReadStringIntoBuffer(int charPos, int initialPosition, int lastWritePosition)
	{
		if (initialPosition == lastWritePosition)
		{
			this._stringReference = new StringReference(this._chars, initialPosition, charPos - initialPosition);
		}
		else
		{
			this.EnsureBufferNotEmpty();
			if (charPos > lastWritePosition)
			{
				this._stringBuffer.Append(this._arrayPool, this._chars, lastWritePosition, charPos - lastWritePosition);
			}
			this._stringReference = new StringReference(this._stringBuffer.InternalBuffer, 0, this._stringBuffer.Position);
		}
		this._charPos = charPos + 1;
	}

	private void WriteCharToBuffer(char writeChar, int lastWritePosition, int writeToPosition)
	{
		if (writeToPosition > lastWritePosition)
		{
			this._stringBuffer.Append(this._arrayPool, this._chars, lastWritePosition, writeToPosition - lastWritePosition);
		}
		this._stringBuffer.Append(this._arrayPool, writeChar);
	}

	private char ConvertUnicode(bool enoughChars)
	{
		if (enoughChars)
		{
			if (ConvertUtils.TryHexTextToInt(this._chars, this._charPos, this._charPos + 4, out var value))
			{
				char result = Convert.ToChar(value);
				this._charPos += 4;
				return result;
			}
			throw JsonReaderException.Create(this, "Invalid Unicode escape sequence: \\u{0}.".FormatWith(CultureInfo.InvariantCulture, new string(this._chars, this._charPos, 4)));
		}
		throw JsonReaderException.Create(this, "Unexpected end while parsing Unicode escape sequence.");
	}

	private char ParseUnicode()
	{
		return this.ConvertUnicode(this.EnsureChars(4, append: true));
	}

	private void ReadNumberIntoBuffer()
	{
		int num = this._charPos;
		while (true)
		{
			char c = this._chars[num];
			if (c == '\0')
			{
				this._charPos = num;
				if (this._charsUsed != num || this.ReadData(append: true) == 0)
				{
					break;
				}
			}
			else
			{
				if (this.ReadNumberCharIntoBuffer(c, num))
				{
					break;
				}
				num++;
			}
		}
	}

	private bool ReadNumberCharIntoBuffer(char currentChar, int charPos)
	{
		switch (currentChar)
		{
		case '+':
		case '-':
		case '.':
		case '0':
		case '1':
		case '2':
		case '3':
		case '4':
		case '5':
		case '6':
		case '7':
		case '8':
		case '9':
		case 'A':
		case 'B':
		case 'C':
		case 'D':
		case 'E':
		case 'F':
		case 'X':
		case 'a':
		case 'b':
		case 'c':
		case 'd':
		case 'e':
		case 'f':
		case 'x':
			return false;
		default:
			this._charPos = charPos;
			if (char.IsWhiteSpace(currentChar) || currentChar == ',' || currentChar == '}' || currentChar == ']' || currentChar == ')' || currentChar == '/')
			{
				return true;
			}
			throw JsonReaderException.Create(this, "Unexpected character encountered while parsing number: {0}.".FormatWith(CultureInfo.InvariantCulture, currentChar));
		}
	}

	private void ClearRecentString()
	{
		this._stringBuffer.Position = 0;
		this._stringReference = default(StringReference);
	}

	private bool ParsePostValue(bool ignoreComments)
	{
		while (true)
		{
			char c = this._chars[this._charPos];
			switch (c)
			{
			case '\0':
				if (this._charsUsed == this._charPos)
				{
					if (this.ReadData(append: false) == 0)
					{
						base._currentState = State.Finished;
						return false;
					}
				}
				else
				{
					this._charPos++;
				}
				continue;
			case '}':
				this._charPos++;
				base.SetToken(JsonToken.EndObject);
				return true;
			case ']':
				this._charPos++;
				base.SetToken(JsonToken.EndArray);
				return true;
			case ')':
				this._charPos++;
				base.SetToken(JsonToken.EndConstructor);
				return true;
			case '/':
				this.ParseComment(!ignoreComments);
				if (!ignoreComments)
				{
					return true;
				}
				continue;
			case ',':
				this._charPos++;
				base.SetStateBasedOnCurrent();
				return false;
			case '\t':
			case ' ':
				this._charPos++;
				continue;
			case '\r':
				this.ProcessCarriageReturn(append: false);
				continue;
			case '\n':
				this.ProcessLineFeed();
				continue;
			}
			if (char.IsWhiteSpace(c))
			{
				this._charPos++;
				continue;
			}
			if (base.SupportMultipleContent && this.Depth == 0)
			{
				base.SetStateBasedOnCurrent();
				return false;
			}
			throw JsonReaderException.Create(this, "After parsing a value an unexpected character was encountered: {0}.".FormatWith(CultureInfo.InvariantCulture, c));
		}
	}

	private bool ParseObject()
	{
		while (true)
		{
			char c = this._chars[this._charPos];
			switch (c)
			{
			case '\0':
				if (this._charsUsed == this._charPos)
				{
					if (this.ReadData(append: false) == 0)
					{
						return false;
					}
				}
				else
				{
					this._charPos++;
				}
				break;
			case '}':
				base.SetToken(JsonToken.EndObject);
				this._charPos++;
				return true;
			case '/':
				this.ParseComment(setToken: true);
				return true;
			case '\r':
				this.ProcessCarriageReturn(append: false);
				break;
			case '\n':
				this.ProcessLineFeed();
				break;
			case '\t':
			case ' ':
				this._charPos++;
				break;
			default:
				if (char.IsWhiteSpace(c))
				{
					this._charPos++;
					break;
				}
				return this.ParseProperty();
			}
		}
	}

	private bool ParseProperty()
	{
		char c = this._chars[this._charPos];
		char c2;
		if (c == '"' || c == '\'')
		{
			this._charPos++;
			c2 = c;
			this.ShiftBufferIfNeeded();
			this.ReadStringIntoBuffer(c2);
		}
		else
		{
			if (!this.ValidIdentifierChar(c))
			{
				throw JsonReaderException.Create(this, "Invalid property identifier character: {0}.".FormatWith(CultureInfo.InvariantCulture, this._chars[this._charPos]));
			}
			c2 = '\0';
			this.ShiftBufferIfNeeded();
			this.ParseUnquotedProperty();
		}
		string text;
		if (this.PropertyNameTable != null)
		{
			text = this.PropertyNameTable.Get(this._stringReference.Chars, this._stringReference.StartIndex, this._stringReference.Length);
			if (text == null)
			{
				text = this._stringReference.ToString();
			}
		}
		else
		{
			text = this._stringReference.ToString();
		}
		this.EatWhitespace();
		if (this._chars[this._charPos] != ':')
		{
			throw JsonReaderException.Create(this, "Invalid character after parsing property name. Expected ':' but got: {0}.".FormatWith(CultureInfo.InvariantCulture, this._chars[this._charPos]));
		}
		this._charPos++;
		base.SetToken(JsonToken.PropertyName, text);
		base._quoteChar = c2;
		this.ClearRecentString();
		return true;
	}

	private bool ValidIdentifierChar(char value)
	{
		if (!char.IsLetterOrDigit(value) && value != '_')
		{
			return value == '$';
		}
		return true;
	}

	private void ParseUnquotedProperty()
	{
		int charPos = this._charPos;
		while (true)
		{
			char c = this._chars[this._charPos];
			if (c == '\0')
			{
				if (this._charsUsed != this._charPos)
				{
					this._stringReference = new StringReference(this._chars, charPos, this._charPos - charPos);
					break;
				}
				if (this.ReadData(append: true) == 0)
				{
					throw JsonReaderException.Create(this, "Unexpected end while parsing unquoted property name.");
				}
			}
			else if (this.ReadUnquotedPropertyReportIfDone(c, charPos))
			{
				break;
			}
		}
	}

	private bool ReadUnquotedPropertyReportIfDone(char currentChar, int initialPosition)
	{
		if (this.ValidIdentifierChar(currentChar))
		{
			this._charPos++;
			return false;
		}
		if (char.IsWhiteSpace(currentChar) || currentChar == ':')
		{
			this._stringReference = new StringReference(this._chars, initialPosition, this._charPos - initialPosition);
			return true;
		}
		throw JsonReaderException.Create(this, "Invalid JavaScript property identifier character: {0}.".FormatWith(CultureInfo.InvariantCulture, currentChar));
	}

	private bool ParseValue()
	{
		while (true)
		{
			char c = this._chars[this._charPos];
			switch (c)
			{
			case '\0':
				if (this._charsUsed == this._charPos)
				{
					if (this.ReadData(append: false) == 0)
					{
						return false;
					}
				}
				else
				{
					this._charPos++;
				}
				break;
			case '"':
			case '\'':
				this.ParseString(c, ReadType.Read);
				return true;
			case 't':
				this.ParseTrue();
				return true;
			case 'f':
				this.ParseFalse();
				return true;
			case 'n':
				if (this.EnsureChars(1, append: true))
				{
					switch (this._chars[this._charPos + 1])
					{
					case 'u':
						this.ParseNull();
						break;
					case 'e':
						this.ParseConstructor();
						break;
					default:
						throw this.CreateUnexpectedCharacterException(this._chars[this._charPos]);
					}
					return true;
				}
				this._charPos++;
				throw base.CreateUnexpectedEndException();
			case 'N':
				this.ParseNumberNaN(ReadType.Read);
				return true;
			case 'I':
				this.ParseNumberPositiveInfinity(ReadType.Read);
				return true;
			case '-':
				if (this.EnsureChars(1, append: true) && this._chars[this._charPos + 1] == 'I')
				{
					this.ParseNumberNegativeInfinity(ReadType.Read);
				}
				else
				{
					this.ParseNumber(ReadType.Read);
				}
				return true;
			case '/':
				this.ParseComment(setToken: true);
				return true;
			case 'u':
				this.ParseUndefined();
				return true;
			case '{':
				this._charPos++;
				base.SetToken(JsonToken.StartObject);
				return true;
			case '[':
				this._charPos++;
				base.SetToken(JsonToken.StartArray);
				return true;
			case ']':
				this._charPos++;
				base.SetToken(JsonToken.EndArray);
				return true;
			case ',':
				base.SetToken(JsonToken.Undefined);
				return true;
			case ')':
				this._charPos++;
				base.SetToken(JsonToken.EndConstructor);
				return true;
			case '\r':
				this.ProcessCarriageReturn(append: false);
				break;
			case '\n':
				this.ProcessLineFeed();
				break;
			case '\t':
			case ' ':
				this._charPos++;
				break;
			default:
				if (char.IsWhiteSpace(c))
				{
					this._charPos++;
					break;
				}
				if (char.IsNumber(c) || c == '-' || c == '.')
				{
					this.ParseNumber(ReadType.Read);
					return true;
				}
				throw this.CreateUnexpectedCharacterException(c);
			}
		}
	}

	private void ProcessLineFeed()
	{
		this._charPos++;
		this.OnNewLine(this._charPos);
	}

	private void ProcessCarriageReturn(bool append)
	{
		this._charPos++;
		this.SetNewLine(this.EnsureChars(1, append));
	}

	private void EatWhitespace()
	{
		while (true)
		{
			char c = this._chars[this._charPos];
			switch (c)
			{
			case '\0':
				if (this._charsUsed == this._charPos)
				{
					if (this.ReadData(append: false) == 0)
					{
						return;
					}
				}
				else
				{
					this._charPos++;
				}
				break;
			case '\r':
				this.ProcessCarriageReturn(append: false);
				break;
			case '\n':
				this.ProcessLineFeed();
				break;
			default:
				if (!char.IsWhiteSpace(c))
				{
					return;
				}
				goto case ' ';
			case ' ':
				this._charPos++;
				break;
			}
		}
	}

	private void ParseConstructor()
	{
		if (this.MatchValueWithTrailingSeparator("new"))
		{
			this.EatWhitespace();
			int charPos = this._charPos;
			int charPos2;
			while (true)
			{
				char c = this._chars[this._charPos];
				if (c == '\0')
				{
					if (this._charsUsed == this._charPos)
					{
						if (this.ReadData(append: true) == 0)
						{
							throw JsonReaderException.Create(this, "Unexpected end while parsing constructor.");
						}
						continue;
					}
					charPos2 = this._charPos;
					this._charPos++;
					break;
				}
				if (char.IsLetterOrDigit(c))
				{
					this._charPos++;
					continue;
				}
				switch (c)
				{
				case '\r':
					charPos2 = this._charPos;
					this.ProcessCarriageReturn(append: true);
					break;
				case '\n':
					charPos2 = this._charPos;
					this.ProcessLineFeed();
					break;
				default:
					if (char.IsWhiteSpace(c))
					{
						charPos2 = this._charPos;
						this._charPos++;
						break;
					}
					if (c == '(')
					{
						charPos2 = this._charPos;
						break;
					}
					throw JsonReaderException.Create(this, "Unexpected character while parsing constructor: {0}.".FormatWith(CultureInfo.InvariantCulture, c));
				}
				break;
			}
			this._stringReference = new StringReference(this._chars, charPos, charPos2 - charPos);
			string value = this._stringReference.ToString();
			this.EatWhitespace();
			if (this._chars[this._charPos] != '(')
			{
				throw JsonReaderException.Create(this, "Unexpected character while parsing constructor: {0}.".FormatWith(CultureInfo.InvariantCulture, this._chars[this._charPos]));
			}
			this._charPos++;
			this.ClearRecentString();
			base.SetToken(JsonToken.StartConstructor, value);
			return;
		}
		throw JsonReaderException.Create(this, "Unexpected content while parsing JSON.");
	}

	private void ParseNumber(ReadType readType)
	{
		this.ShiftBufferIfNeeded();
		char firstChar = this._chars[this._charPos];
		int charPos = this._charPos;
		this.ReadNumberIntoBuffer();
		this.ParseReadNumber(readType, firstChar, charPos);
	}

	private void ParseReadNumber(ReadType readType, char firstChar, int initialPosition)
	{
		base.SetPostValueState(updateIndex: true);
		this._stringReference = new StringReference(this._chars, initialPosition, this._charPos - initialPosition);
		bool flag = char.IsDigit(firstChar) && this._stringReference.Length == 1;
		bool flag2 = firstChar == '0' && this._stringReference.Length > 1 && this._stringReference.Chars[this._stringReference.StartIndex + 1] != '.' && this._stringReference.Chars[this._stringReference.StartIndex + 1] != 'e' && this._stringReference.Chars[this._stringReference.StartIndex + 1] != 'E';
		JsonToken newToken;
		object value;
		switch (readType)
		{
		case ReadType.ReadAsString:
		{
			string text5 = this._stringReference.ToString();
			double result3;
			if (flag2)
			{
				try
				{
					if (text5.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
					{
						Convert.ToInt64(text5, 16);
					}
					else
					{
						Convert.ToInt64(text5, 8);
					}
				}
				catch (Exception ex4)
				{
					throw this.ThrowReaderError("Input string '{0}' is not a valid number.".FormatWith(CultureInfo.InvariantCulture, text5), ex4);
				}
			}
			else if (!double.TryParse(text5, NumberStyles.Float, CultureInfo.InvariantCulture, out result3))
			{
				throw this.ThrowReaderError("Input string '{0}' is not a valid number.".FormatWith(CultureInfo.InvariantCulture, this._stringReference.ToString()));
			}
			newToken = JsonToken.String;
			value = text5;
			break;
		}
		case ReadType.ReadAsInt32:
			if (flag)
			{
				value = BoxedPrimitives.Get(firstChar - 48);
			}
			else if (flag2)
			{
				string text6 = this._stringReference.ToString();
				try
				{
					value = BoxedPrimitives.Get(text6.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ? Convert.ToInt32(text6, 16) : Convert.ToInt32(text6, 8));
				}
				catch (Exception ex5)
				{
					throw this.ThrowReaderError("Input string '{0}' is not a valid integer.".FormatWith(CultureInfo.InvariantCulture, text6), ex5);
				}
			}
			else
			{
				int value5;
				switch (ConvertUtils.Int32TryParse(this._stringReference.Chars, this._stringReference.StartIndex, this._stringReference.Length, out value5))
				{
				case ParseResult.Success:
					break;
				case ParseResult.Overflow:
					throw this.ThrowReaderError("JSON integer {0} is too large or small for an Int32.".FormatWith(CultureInfo.InvariantCulture, this._stringReference.ToString()));
				default:
					throw this.ThrowReaderError("Input string '{0}' is not a valid integer.".FormatWith(CultureInfo.InvariantCulture, this._stringReference.ToString()));
				}
				value = BoxedPrimitives.Get(value5);
			}
			newToken = JsonToken.Integer;
			break;
		case ReadType.ReadAsDecimal:
			if (flag)
			{
				value = BoxedPrimitives.Get((decimal)firstChar - 48m);
			}
			else if (flag2)
			{
				string text3 = this._stringReference.ToString();
				try
				{
					value = BoxedPrimitives.Get(Convert.ToDecimal(text3.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ? Convert.ToInt64(text3, 16) : Convert.ToInt64(text3, 8)));
				}
				catch (Exception ex2)
				{
					throw this.ThrowReaderError("Input string '{0}' is not a valid decimal.".FormatWith(CultureInfo.InvariantCulture, text3), ex2);
				}
			}
			else
			{
				if (ConvertUtils.DecimalTryParse(this._stringReference.Chars, this._stringReference.StartIndex, this._stringReference.Length, out var value4) != ParseResult.Success)
				{
					throw this.ThrowReaderError("Input string '{0}' is not a valid decimal.".FormatWith(CultureInfo.InvariantCulture, this._stringReference.ToString()));
				}
				value = BoxedPrimitives.Get(value4);
			}
			newToken = JsonToken.Float;
			break;
		case ReadType.ReadAsDouble:
			if (flag)
			{
				value = BoxedPrimitives.Get((double)(int)firstChar - 48.0);
			}
			else if (flag2)
			{
				string text4 = this._stringReference.ToString();
				try
				{
					value = BoxedPrimitives.Get(Convert.ToDouble(text4.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ? Convert.ToInt64(text4, 16) : Convert.ToInt64(text4, 8)));
				}
				catch (Exception ex3)
				{
					throw this.ThrowReaderError("Input string '{0}' is not a valid double.".FormatWith(CultureInfo.InvariantCulture, text4), ex3);
				}
			}
			else
			{
				if (!double.TryParse(this._stringReference.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var result2))
				{
					throw this.ThrowReaderError("Input string '{0}' is not a valid double.".FormatWith(CultureInfo.InvariantCulture, this._stringReference.ToString()));
				}
				value = BoxedPrimitives.Get(result2);
			}
			newToken = JsonToken.Float;
			break;
		case ReadType.Read:
		case ReadType.ReadAsInt64:
		{
			if (flag)
			{
				value = BoxedPrimitives.Get((long)firstChar - 48L);
				newToken = JsonToken.Integer;
				break;
			}
			if (flag2)
			{
				string text = this._stringReference.ToString();
				try
				{
					value = BoxedPrimitives.Get(text.StartsWith("0x", StringComparison.OrdinalIgnoreCase) ? Convert.ToInt64(text, 16) : Convert.ToInt64(text, 8));
				}
				catch (Exception ex)
				{
					throw this.ThrowReaderError("Input string '{0}' is not a valid number.".FormatWith(CultureInfo.InvariantCulture, text), ex);
				}
				newToken = JsonToken.Integer;
				break;
			}
			long value2;
			switch (ConvertUtils.Int64TryParse(this._stringReference.Chars, this._stringReference.StartIndex, this._stringReference.Length, out value2))
			{
			case ParseResult.Success:
				value = BoxedPrimitives.Get(value2);
				newToken = JsonToken.Integer;
				break;
			case ParseResult.Overflow:
			{
				string text2 = this._stringReference.ToString();
				if (text2.Length > 380)
				{
					throw this.ThrowReaderError("JSON integer {0} is too large to parse.".FormatWith(CultureInfo.InvariantCulture, this._stringReference.ToString()));
				}
				value = JsonTextReader.BigIntegerParse(text2, CultureInfo.InvariantCulture);
				newToken = JsonToken.Integer;
				break;
			}
			default:
				if (base._floatParseHandling == FloatParseHandling.Decimal)
				{
					decimal value3;
					ParseResult parseResult = ConvertUtils.DecimalTryParse(this._stringReference.Chars, this._stringReference.StartIndex, this._stringReference.Length, out value3);
					if (parseResult != ParseResult.Success)
					{
						throw this.ThrowReaderError("Input string '{0}' is not a valid decimal.".FormatWith(CultureInfo.InvariantCulture, this._stringReference.ToString()));
					}
					value = BoxedPrimitives.Get(value3);
				}
				else
				{
					if (!double.TryParse(this._stringReference.ToString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
					{
						throw this.ThrowReaderError("Input string '{0}' is not a valid number.".FormatWith(CultureInfo.InvariantCulture, this._stringReference.ToString()));
					}
					value = BoxedPrimitives.Get(result);
				}
				newToken = JsonToken.Float;
				break;
			}
			break;
		}
		default:
			throw JsonReaderException.Create(this, "Cannot read number value as type.");
		}
		this.ClearRecentString();
		base.SetToken(newToken, value, updateIndex: false);
	}

	private JsonReaderException ThrowReaderError(string message, Exception? ex = null)
	{
		base.SetToken(JsonToken.Undefined, null, updateIndex: false);
		return JsonReaderException.Create(this, message, ex);
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static object BigIntegerParse(string number, CultureInfo culture)
	{
		return BigInteger.Parse(number, culture);
	}

	private void ParseComment(bool setToken)
	{
		this._charPos++;
		if (!this.EnsureChars(1, append: false))
		{
			throw JsonReaderException.Create(this, "Unexpected end while parsing comment.");
		}
		bool flag;
		if (this._chars[this._charPos] == '*')
		{
			flag = false;
		}
		else
		{
			if (this._chars[this._charPos] != '/')
			{
				throw JsonReaderException.Create(this, "Error parsing comment. Expected: *, got {0}.".FormatWith(CultureInfo.InvariantCulture, this._chars[this._charPos]));
			}
			flag = true;
		}
		this._charPos++;
		int charPos = this._charPos;
		while (true)
		{
			switch (this._chars[this._charPos])
			{
			case '\0':
				if (this._charsUsed == this._charPos)
				{
					if (this.ReadData(append: true) == 0)
					{
						if (!flag)
						{
							throw JsonReaderException.Create(this, "Unexpected end while parsing comment.");
						}
						this.EndComment(setToken, charPos, this._charPos);
						return;
					}
				}
				else
				{
					this._charPos++;
				}
				break;
			case '*':
				this._charPos++;
				if (!flag && this.EnsureChars(0, append: true) && this._chars[this._charPos] == '/')
				{
					this.EndComment(setToken, charPos, this._charPos - 1);
					this._charPos++;
					return;
				}
				break;
			case '\r':
				if (flag)
				{
					this.EndComment(setToken, charPos, this._charPos);
					return;
				}
				this.ProcessCarriageReturn(append: true);
				break;
			case '\n':
				if (flag)
				{
					this.EndComment(setToken, charPos, this._charPos);
					return;
				}
				this.ProcessLineFeed();
				break;
			default:
				this._charPos++;
				break;
			}
		}
	}

	private void EndComment(bool setToken, int initialPosition, int endPosition)
	{
		if (setToken)
		{
			base.SetToken(JsonToken.Comment, new string(this._chars, initialPosition, endPosition - initialPosition));
		}
	}

	private bool MatchValue(string value)
	{
		return this.MatchValue(this.EnsureChars(value.Length - 1, append: true), value);
	}

	private bool MatchValue(bool enoughChars, string value)
	{
		if (!enoughChars)
		{
			this._charPos = this._charsUsed;
			throw base.CreateUnexpectedEndException();
		}
		for (int i = 0; i < value.Length; i++)
		{
			if (this._chars[this._charPos + i] != value[i])
			{
				this._charPos += i;
				return false;
			}
		}
		this._charPos += value.Length;
		return true;
	}

	private bool MatchValueWithTrailingSeparator(string value)
	{
		if (!this.MatchValue(value))
		{
			return false;
		}
		if (!this.EnsureChars(0, append: false))
		{
			return true;
		}
		if (!this.IsSeparator(this._chars[this._charPos]))
		{
			return this._chars[this._charPos] == '\0';
		}
		return true;
	}

	private bool IsSeparator(char c)
	{
		switch (c)
		{
		case ',':
		case ']':
		case '}':
			return true;
		case '/':
		{
			if (!this.EnsureChars(1, append: false))
			{
				return false;
			}
			char c2 = this._chars[this._charPos + 1];
			if (c2 != '*')
			{
				return c2 == '/';
			}
			return true;
		}
		case ')':
			if (base.CurrentState == State.Constructor || base.CurrentState == State.ConstructorStart)
			{
				return true;
			}
			break;
		case '\t':
		case '\n':
		case '\r':
		case ' ':
			return true;
		default:
			if (char.IsWhiteSpace(c))
			{
				return true;
			}
			break;
		}
		return false;
	}

	private void ParseTrue()
	{
		if (this.MatchValueWithTrailingSeparator(JsonConvert.True))
		{
			base.SetToken(JsonToken.Boolean, BoxedPrimitives.BooleanTrue);
			return;
		}
		throw JsonReaderException.Create(this, "Error parsing boolean value.");
	}

	private void ParseNull()
	{
		if (this.MatchValueWithTrailingSeparator(JsonConvert.Null))
		{
			base.SetToken(JsonToken.Null);
			return;
		}
		throw JsonReaderException.Create(this, "Error parsing null value.");
	}

	private void ParseUndefined()
	{
		if (this.MatchValueWithTrailingSeparator(JsonConvert.Undefined))
		{
			base.SetToken(JsonToken.Undefined);
			return;
		}
		throw JsonReaderException.Create(this, "Error parsing undefined value.");
	}

	private void ParseFalse()
	{
		if (this.MatchValueWithTrailingSeparator(JsonConvert.False))
		{
			base.SetToken(JsonToken.Boolean, BoxedPrimitives.BooleanFalse);
			return;
		}
		throw JsonReaderException.Create(this, "Error parsing boolean value.");
	}

	private object ParseNumberNegativeInfinity(ReadType readType)
	{
		return this.ParseNumberNegativeInfinity(readType, this.MatchValueWithTrailingSeparator(JsonConvert.NegativeInfinity));
	}

	private object ParseNumberNegativeInfinity(ReadType readType, bool matched)
	{
		if (matched)
		{
			switch (readType)
			{
			case ReadType.Read:
			case ReadType.ReadAsDouble:
				if (base._floatParseHandling == FloatParseHandling.Double)
				{
					base.SetToken(JsonToken.Float, BoxedPrimitives.DoubleNegativeInfinity);
					return double.NegativeInfinity;
				}
				break;
			case ReadType.ReadAsString:
				base.SetToken(JsonToken.String, JsonConvert.NegativeInfinity);
				return JsonConvert.NegativeInfinity;
			}
			throw JsonReaderException.Create(this, "Cannot read -Infinity value.");
		}
		throw JsonReaderException.Create(this, "Error parsing -Infinity value.");
	}

	private object ParseNumberPositiveInfinity(ReadType readType)
	{
		return this.ParseNumberPositiveInfinity(readType, this.MatchValueWithTrailingSeparator(JsonConvert.PositiveInfinity));
	}

	private object ParseNumberPositiveInfinity(ReadType readType, bool matched)
	{
		if (matched)
		{
			switch (readType)
			{
			case ReadType.Read:
			case ReadType.ReadAsDouble:
				if (base._floatParseHandling == FloatParseHandling.Double)
				{
					base.SetToken(JsonToken.Float, BoxedPrimitives.DoublePositiveInfinity);
					return double.PositiveInfinity;
				}
				break;
			case ReadType.ReadAsString:
				base.SetToken(JsonToken.String, JsonConvert.PositiveInfinity);
				return JsonConvert.PositiveInfinity;
			}
			throw JsonReaderException.Create(this, "Cannot read Infinity value.");
		}
		throw JsonReaderException.Create(this, "Error parsing Infinity value.");
	}

	private object ParseNumberNaN(ReadType readType)
	{
		return this.ParseNumberNaN(readType, this.MatchValueWithTrailingSeparator(JsonConvert.NaN));
	}

	private object ParseNumberNaN(ReadType readType, bool matched)
	{
		if (matched)
		{
			switch (readType)
			{
			case ReadType.Read:
			case ReadType.ReadAsDouble:
				if (base._floatParseHandling == FloatParseHandling.Double)
				{
					base.SetToken(JsonToken.Float, BoxedPrimitives.DoubleNaN);
					return double.NaN;
				}
				break;
			case ReadType.ReadAsString:
				base.SetToken(JsonToken.String, JsonConvert.NaN);
				return JsonConvert.NaN;
			}
			throw JsonReaderException.Create(this, "Cannot read NaN value.");
		}
		throw JsonReaderException.Create(this, "Error parsing NaN value.");
	}

	/// <summary>
	/// Changes the reader's state to <see cref="F:Newtonsoft.Json.JsonReader.State.Closed" />.
	/// If <see cref="P:Newtonsoft.Json.JsonReader.CloseInput" /> is set to <c>true</c>, the underlying <see cref="T:System.IO.TextReader" /> is also closed.
	/// </summary>
	public override void Close()
	{
		base.Close();
		if (this._chars != null)
		{
			BufferUtils.ReturnBuffer(this._arrayPool, this._chars);
			this._chars = null;
		}
		if (base.CloseInput)
		{
			this._reader?.Close();
		}
		this._stringBuffer.Clear(this._arrayPool);
	}

	/// <summary>
	/// Gets a value indicating whether the class can return line information.
	/// </summary>
	/// <returns>
	/// 	<c>true</c> if <see cref="P:Newtonsoft.Json.JsonTextReader.LineNumber" /> and <see cref="P:Newtonsoft.Json.JsonTextReader.LinePosition" /> can be provided; otherwise, <c>false</c>.
	/// </returns>
	public bool HasLineInfo()
	{
		return true;
	}
}
