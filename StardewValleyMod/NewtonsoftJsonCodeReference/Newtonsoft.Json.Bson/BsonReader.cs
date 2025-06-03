using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Bson;

/// <summary>
/// Represents a reader that provides fast, non-cached, forward-only access to serialized BSON data.
/// </summary>
[Obsolete("BSON reading and writing has been moved to its own package. See https://www.nuget.org/packages/Newtonsoft.Json.Bson for more details.")]
public class BsonReader : JsonReader
{
	private enum BsonReaderState
	{
		Normal,
		ReferenceStart,
		ReferenceRef,
		ReferenceId,
		CodeWScopeStart,
		CodeWScopeCode,
		CodeWScopeScope,
		CodeWScopeScopeObject,
		CodeWScopeScopeEnd
	}

	private class ContainerContext
	{
		public readonly BsonType Type;

		public int Length;

		public int Position;

		public ContainerContext(BsonType type)
		{
			this.Type = type;
		}
	}

	private const int MaxCharBytesSize = 128;

	private static readonly byte[] SeqRange1 = new byte[2] { 0, 127 };

	private static readonly byte[] SeqRange2 = new byte[2] { 194, 223 };

	private static readonly byte[] SeqRange3 = new byte[2] { 224, 239 };

	private static readonly byte[] SeqRange4 = new byte[2] { 240, 244 };

	private readonly BinaryReader _reader;

	private readonly List<ContainerContext> _stack;

	private byte[] _byteBuffer;

	private char[] _charBuffer;

	private BsonType _currentElementType;

	private BsonReaderState _bsonReaderState;

	private ContainerContext _currentContext;

	private bool _readRootValueAsArray;

	private bool _jsonNet35BinaryCompatibility;

	private DateTimeKind _dateTimeKindHandling;

	/// <summary>
	/// Gets or sets a value indicating whether binary data reading should be compatible with incorrect Json.NET 3.5 written binary.
	/// </summary>
	/// <value>
	/// 	<c>true</c> if binary data reading will be compatible with incorrect Json.NET 3.5 written binary; otherwise, <c>false</c>.
	/// </value>
	[Obsolete("JsonNet35BinaryCompatibility will be removed in a future version of Json.NET.")]
	public bool JsonNet35BinaryCompatibility
	{
		get
		{
			return this._jsonNet35BinaryCompatibility;
		}
		set
		{
			this._jsonNet35BinaryCompatibility = value;
		}
	}

	/// <summary>
	/// Gets or sets a value indicating whether the root object will be read as a JSON array.
	/// </summary>
	/// <value>
	/// 	<c>true</c> if the root object will be read as a JSON array; otherwise, <c>false</c>.
	/// </value>
	public bool ReadRootValueAsArray
	{
		get
		{
			return this._readRootValueAsArray;
		}
		set
		{
			this._readRootValueAsArray = value;
		}
	}

	/// <summary>
	/// Gets or sets the <see cref="T:System.DateTimeKind" /> used when reading <see cref="T:System.DateTime" /> values from BSON.
	/// </summary>
	/// <value>The <see cref="T:System.DateTimeKind" /> used when reading <see cref="T:System.DateTime" /> values from BSON.</value>
	public DateTimeKind DateTimeKindHandling
	{
		get
		{
			return this._dateTimeKindHandling;
		}
		set
		{
			this._dateTimeKindHandling = value;
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:Newtonsoft.Json.Bson.BsonReader" /> class.
	/// </summary>
	/// <param name="stream">The <see cref="T:System.IO.Stream" /> containing the BSON data to read.</param>
	public BsonReader(Stream stream)
		: this(stream, readRootValueAsArray: false, DateTimeKind.Local)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:Newtonsoft.Json.Bson.BsonReader" /> class.
	/// </summary>
	/// <param name="reader">The <see cref="T:System.IO.BinaryReader" /> containing the BSON data to read.</param>
	public BsonReader(BinaryReader reader)
		: this(reader, readRootValueAsArray: false, DateTimeKind.Local)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:Newtonsoft.Json.Bson.BsonReader" /> class.
	/// </summary>
	/// <param name="stream">The <see cref="T:System.IO.Stream" /> containing the BSON data to read.</param>
	/// <param name="readRootValueAsArray">if set to <c>true</c> the root object will be read as a JSON array.</param>
	/// <param name="dateTimeKindHandling">The <see cref="T:System.DateTimeKind" /> used when reading <see cref="T:System.DateTime" /> values from BSON.</param>
	public BsonReader(Stream stream, bool readRootValueAsArray, DateTimeKind dateTimeKindHandling)
	{
		ValidationUtils.ArgumentNotNull(stream, "stream");
		this._reader = new BinaryReader(stream);
		this._stack = new List<ContainerContext>();
		this._readRootValueAsArray = readRootValueAsArray;
		this._dateTimeKindHandling = dateTimeKindHandling;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:Newtonsoft.Json.Bson.BsonReader" /> class.
	/// </summary>
	/// <param name="reader">The <see cref="T:System.IO.BinaryReader" /> containing the BSON data to read.</param>
	/// <param name="readRootValueAsArray">if set to <c>true</c> the root object will be read as a JSON array.</param>
	/// <param name="dateTimeKindHandling">The <see cref="T:System.DateTimeKind" /> used when reading <see cref="T:System.DateTime" /> values from BSON.</param>
	public BsonReader(BinaryReader reader, bool readRootValueAsArray, DateTimeKind dateTimeKindHandling)
	{
		ValidationUtils.ArgumentNotNull(reader, "reader");
		this._reader = reader;
		this._stack = new List<ContainerContext>();
		this._readRootValueAsArray = readRootValueAsArray;
		this._dateTimeKindHandling = dateTimeKindHandling;
	}

	private string ReadElement()
	{
		this._currentElementType = this.ReadType();
		return this.ReadString();
	}

	/// <summary>
	/// Reads the next JSON token from the underlying <see cref="T:System.IO.Stream" />.
	/// </summary>
	/// <returns>
	/// <c>true</c> if the next token was read successfully; <c>false</c> if there are no more tokens to read.
	/// </returns>
	public override bool Read()
	{
		try
		{
			bool flag;
			switch (this._bsonReaderState)
			{
			case BsonReaderState.Normal:
				flag = this.ReadNormal();
				break;
			case BsonReaderState.ReferenceStart:
			case BsonReaderState.ReferenceRef:
			case BsonReaderState.ReferenceId:
				flag = this.ReadReference();
				break;
			case BsonReaderState.CodeWScopeStart:
			case BsonReaderState.CodeWScopeCode:
			case BsonReaderState.CodeWScopeScope:
			case BsonReaderState.CodeWScopeScopeObject:
			case BsonReaderState.CodeWScopeScopeEnd:
				flag = this.ReadCodeWScope();
				break;
			default:
				throw JsonReaderException.Create(this, "Unexpected state: {0}".FormatWith(CultureInfo.InvariantCulture, this._bsonReaderState));
			}
			if (!flag)
			{
				base.SetToken(JsonToken.None);
				return false;
			}
			return true;
		}
		catch (EndOfStreamException)
		{
			base.SetToken(JsonToken.None);
			return false;
		}
	}

	/// <summary>
	/// Changes the reader's state to <see cref="F:Newtonsoft.Json.JsonReader.State.Closed" />.
	/// If <see cref="P:Newtonsoft.Json.JsonReader.CloseInput" /> is set to <c>true</c>, the underlying <see cref="T:System.IO.Stream" /> is also closed.
	/// </summary>
	public override void Close()
	{
		base.Close();
		if (base.CloseInput)
		{
			this._reader?.Close();
		}
	}

	private bool ReadCodeWScope()
	{
		switch (this._bsonReaderState)
		{
		case BsonReaderState.CodeWScopeStart:
			base.SetToken(JsonToken.PropertyName, "$code");
			this._bsonReaderState = BsonReaderState.CodeWScopeCode;
			return true;
		case BsonReaderState.CodeWScopeCode:
			this.ReadInt32();
			base.SetToken(JsonToken.String, this.ReadLengthString());
			this._bsonReaderState = BsonReaderState.CodeWScopeScope;
			return true;
		case BsonReaderState.CodeWScopeScope:
		{
			if (base.CurrentState == State.PostValue)
			{
				base.SetToken(JsonToken.PropertyName, "$scope");
				return true;
			}
			base.SetToken(JsonToken.StartObject);
			this._bsonReaderState = BsonReaderState.CodeWScopeScopeObject;
			ContainerContext containerContext = new ContainerContext(BsonType.Object);
			this.PushContext(containerContext);
			containerContext.Length = this.ReadInt32();
			return true;
		}
		case BsonReaderState.CodeWScopeScopeObject:
		{
			bool num = this.ReadNormal();
			if (num && this.TokenType == JsonToken.EndObject)
			{
				this._bsonReaderState = BsonReaderState.CodeWScopeScopeEnd;
			}
			return num;
		}
		case BsonReaderState.CodeWScopeScopeEnd:
			base.SetToken(JsonToken.EndObject);
			this._bsonReaderState = BsonReaderState.Normal;
			return true;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	private bool ReadReference()
	{
		switch (base.CurrentState)
		{
		case State.ObjectStart:
			base.SetToken(JsonToken.PropertyName, "$ref");
			this._bsonReaderState = BsonReaderState.ReferenceRef;
			return true;
		case State.Property:
			if (this._bsonReaderState == BsonReaderState.ReferenceRef)
			{
				base.SetToken(JsonToken.String, this.ReadLengthString());
				return true;
			}
			if (this._bsonReaderState == BsonReaderState.ReferenceId)
			{
				base.SetToken(JsonToken.Bytes, this.ReadBytes(12));
				return true;
			}
			throw JsonReaderException.Create(this, "Unexpected state when reading BSON reference: " + this._bsonReaderState);
		case State.PostValue:
			if (this._bsonReaderState == BsonReaderState.ReferenceRef)
			{
				base.SetToken(JsonToken.PropertyName, "$id");
				this._bsonReaderState = BsonReaderState.ReferenceId;
				return true;
			}
			if (this._bsonReaderState == BsonReaderState.ReferenceId)
			{
				base.SetToken(JsonToken.EndObject);
				this._bsonReaderState = BsonReaderState.Normal;
				return true;
			}
			throw JsonReaderException.Create(this, "Unexpected state when reading BSON reference: " + this._bsonReaderState);
		default:
			throw JsonReaderException.Create(this, "Unexpected state when reading BSON reference: " + base.CurrentState);
		}
	}

	private bool ReadNormal()
	{
		switch (base.CurrentState)
		{
		case State.Start:
		{
			JsonToken token2 = ((!this._readRootValueAsArray) ? JsonToken.StartObject : JsonToken.StartArray);
			int type = ((!this._readRootValueAsArray) ? 3 : 4);
			base.SetToken(token2);
			ContainerContext containerContext = new ContainerContext((BsonType)type);
			this.PushContext(containerContext);
			containerContext.Length = this.ReadInt32();
			return true;
		}
		case State.Complete:
		case State.Closed:
			return false;
		case State.Property:
			this.ReadType(this._currentElementType);
			return true;
		case State.ObjectStart:
		case State.ArrayStart:
		case State.PostValue:
		{
			ContainerContext currentContext = this._currentContext;
			if (currentContext == null)
			{
				if (!base.SupportMultipleContent)
				{
					return false;
				}
				goto case State.Start;
			}
			int num = currentContext.Length - 1;
			if (currentContext.Position < num)
			{
				if (currentContext.Type == BsonType.Array)
				{
					this.ReadElement();
					this.ReadType(this._currentElementType);
					return true;
				}
				base.SetToken(JsonToken.PropertyName, this.ReadElement());
				return true;
			}
			if (currentContext.Position == num)
			{
				if (this.ReadByte() != 0)
				{
					throw JsonReaderException.Create(this, "Unexpected end of object byte value.");
				}
				this.PopContext();
				if (this._currentContext != null)
				{
					this.MovePosition(currentContext.Length);
				}
				JsonToken token = ((currentContext.Type == BsonType.Object) ? JsonToken.EndObject : JsonToken.EndArray);
				base.SetToken(token);
				return true;
			}
			throw JsonReaderException.Create(this, "Read past end of current container context.");
		}
		default:
			throw new ArgumentOutOfRangeException();
		case State.ConstructorStart:
		case State.Constructor:
		case State.Error:
		case State.Finished:
			return false;
		}
	}

	private void PopContext()
	{
		this._stack.RemoveAt(this._stack.Count - 1);
		if (this._stack.Count == 0)
		{
			this._currentContext = null;
		}
		else
		{
			this._currentContext = this._stack[this._stack.Count - 1];
		}
	}

	private void PushContext(ContainerContext newContext)
	{
		this._stack.Add(newContext);
		this._currentContext = newContext;
	}

	private byte ReadByte()
	{
		this.MovePosition(1);
		return this._reader.ReadByte();
	}

	private void ReadType(BsonType type)
	{
		switch (type)
		{
		case BsonType.Number:
		{
			double num = this.ReadDouble();
			if (base._floatParseHandling == FloatParseHandling.Decimal)
			{
				base.SetToken(JsonToken.Float, Convert.ToDecimal(num, CultureInfo.InvariantCulture));
			}
			else
			{
				base.SetToken(JsonToken.Float, num);
			}
			break;
		}
		case BsonType.String:
		case BsonType.Symbol:
			base.SetToken(JsonToken.String, this.ReadLengthString());
			break;
		case BsonType.Object:
		{
			base.SetToken(JsonToken.StartObject);
			ContainerContext containerContext2 = new ContainerContext(BsonType.Object);
			this.PushContext(containerContext2);
			containerContext2.Length = this.ReadInt32();
			break;
		}
		case BsonType.Array:
		{
			base.SetToken(JsonToken.StartArray);
			ContainerContext containerContext = new ContainerContext(BsonType.Array);
			this.PushContext(containerContext);
			containerContext.Length = this.ReadInt32();
			break;
		}
		case BsonType.Binary:
		{
			BsonBinaryType binaryType;
			byte[] array = this.ReadBinary(out binaryType);
			object value3 = ((binaryType != BsonBinaryType.Uuid) ? array : ((object)new Guid(array)));
			base.SetToken(JsonToken.Bytes, value3);
			break;
		}
		case BsonType.Undefined:
			base.SetToken(JsonToken.Undefined);
			break;
		case BsonType.Oid:
		{
			byte[] value2 = this.ReadBytes(12);
			base.SetToken(JsonToken.Bytes, value2);
			break;
		}
		case BsonType.Boolean:
		{
			bool flag = Convert.ToBoolean(this.ReadByte());
			base.SetToken(JsonToken.Boolean, flag);
			break;
		}
		case BsonType.Date:
		{
			DateTime dateTime = DateTimeUtils.ConvertJavaScriptTicksToDateTime(this.ReadInt64());
			base.SetToken(JsonToken.Date, this.DateTimeKindHandling switch
			{
				DateTimeKind.Unspecified => DateTime.SpecifyKind(dateTime, DateTimeKind.Unspecified), 
				DateTimeKind.Local => dateTime.ToLocalTime(), 
				_ => dateTime, 
			});
			break;
		}
		case BsonType.Null:
			base.SetToken(JsonToken.Null);
			break;
		case BsonType.Regex:
		{
			string text = this.ReadString();
			string text2 = this.ReadString();
			string value = "/" + text + "/" + text2;
			base.SetToken(JsonToken.String, value);
			break;
		}
		case BsonType.Reference:
			base.SetToken(JsonToken.StartObject);
			this._bsonReaderState = BsonReaderState.ReferenceStart;
			break;
		case BsonType.Code:
			base.SetToken(JsonToken.String, this.ReadLengthString());
			break;
		case BsonType.CodeWScope:
			base.SetToken(JsonToken.StartObject);
			this._bsonReaderState = BsonReaderState.CodeWScopeStart;
			break;
		case BsonType.Integer:
			base.SetToken(JsonToken.Integer, (long)this.ReadInt32());
			break;
		case BsonType.TimeStamp:
		case BsonType.Long:
			base.SetToken(JsonToken.Integer, this.ReadInt64());
			break;
		default:
			throw new ArgumentOutOfRangeException("type", "Unexpected BsonType value: " + type);
		}
	}

	private byte[] ReadBinary(out BsonBinaryType binaryType)
	{
		int count = this.ReadInt32();
		binaryType = (BsonBinaryType)this.ReadByte();
		if (binaryType == BsonBinaryType.BinaryOld && !this._jsonNet35BinaryCompatibility)
		{
			count = this.ReadInt32();
		}
		return this.ReadBytes(count);
	}

	private string ReadString()
	{
		this.EnsureBuffers();
		StringBuilder stringBuilder = null;
		int num = 0;
		int num2 = 0;
		while (true)
		{
			int num3 = num2;
			byte b;
			while (num3 < 128 && (b = this._reader.ReadByte()) > 0)
			{
				this._byteBuffer[num3++] = b;
			}
			int num4 = num3 - num2;
			num += num4;
			if (num3 < 128 && stringBuilder == null)
			{
				int chars = Encoding.UTF8.GetChars(this._byteBuffer, 0, num4, this._charBuffer, 0);
				this.MovePosition(num + 1);
				return new string(this._charBuffer, 0, chars);
			}
			int lastFullCharStop = this.GetLastFullCharStop(num3 - 1);
			int chars2 = Encoding.UTF8.GetChars(this._byteBuffer, 0, lastFullCharStop + 1, this._charBuffer, 0);
			if (stringBuilder == null)
			{
				stringBuilder = new StringBuilder(256);
			}
			stringBuilder.Append(this._charBuffer, 0, chars2);
			if (lastFullCharStop < num4 - 1)
			{
				num2 = num4 - lastFullCharStop - 1;
				Array.Copy(this._byteBuffer, lastFullCharStop + 1, this._byteBuffer, 0, num2);
				continue;
			}
			if (num3 < 128)
			{
				break;
			}
			num2 = 0;
		}
		this.MovePosition(num + 1);
		return stringBuilder.ToString();
	}

	private string ReadLengthString()
	{
		int num = this.ReadInt32();
		this.MovePosition(num);
		string result = this.GetString(num - 1);
		this._reader.ReadByte();
		return result;
	}

	private string GetString(int length)
	{
		if (length == 0)
		{
			return string.Empty;
		}
		this.EnsureBuffers();
		StringBuilder stringBuilder = null;
		int num = 0;
		int num2 = 0;
		do
		{
			int count = ((length - num > 128 - num2) ? (128 - num2) : (length - num));
			int num3 = this._reader.Read(this._byteBuffer, num2, count);
			if (num3 == 0)
			{
				throw new EndOfStreamException("Unable to read beyond the end of the stream.");
			}
			num += num3;
			num3 += num2;
			if (num3 == length)
			{
				int chars = Encoding.UTF8.GetChars(this._byteBuffer, 0, num3, this._charBuffer, 0);
				return new string(this._charBuffer, 0, chars);
			}
			int lastFullCharStop = this.GetLastFullCharStop(num3 - 1);
			if (stringBuilder == null)
			{
				stringBuilder = new StringBuilder(length);
			}
			int chars2 = Encoding.UTF8.GetChars(this._byteBuffer, 0, lastFullCharStop + 1, this._charBuffer, 0);
			stringBuilder.Append(this._charBuffer, 0, chars2);
			if (lastFullCharStop < num3 - 1)
			{
				num2 = num3 - lastFullCharStop - 1;
				Array.Copy(this._byteBuffer, lastFullCharStop + 1, this._byteBuffer, 0, num2);
			}
			else
			{
				num2 = 0;
			}
		}
		while (num < length);
		return stringBuilder.ToString();
	}

	private int GetLastFullCharStop(int start)
	{
		int num = start;
		int num2 = 0;
		for (; num >= 0; num--)
		{
			num2 = this.BytesInSequence(this._byteBuffer[num]);
			switch (num2)
			{
			case 0:
				continue;
			default:
				num--;
				break;
			case 1:
				break;
			}
			break;
		}
		if (num2 == start - num)
		{
			return start;
		}
		return num;
	}

	private int BytesInSequence(byte b)
	{
		if (b <= BsonReader.SeqRange1[1])
		{
			return 1;
		}
		if (b >= BsonReader.SeqRange2[0] && b <= BsonReader.SeqRange2[1])
		{
			return 2;
		}
		if (b >= BsonReader.SeqRange3[0] && b <= BsonReader.SeqRange3[1])
		{
			return 3;
		}
		if (b >= BsonReader.SeqRange4[0] && b <= BsonReader.SeqRange4[1])
		{
			return 4;
		}
		return 0;
	}

	private void EnsureBuffers()
	{
		if (this._byteBuffer == null)
		{
			this._byteBuffer = new byte[128];
		}
		if (this._charBuffer == null)
		{
			int maxCharCount = Encoding.UTF8.GetMaxCharCount(128);
			this._charBuffer = new char[maxCharCount];
		}
	}

	private double ReadDouble()
	{
		this.MovePosition(8);
		return this._reader.ReadDouble();
	}

	private int ReadInt32()
	{
		this.MovePosition(4);
		return this._reader.ReadInt32();
	}

	private long ReadInt64()
	{
		this.MovePosition(8);
		return this._reader.ReadInt64();
	}

	private BsonType ReadType()
	{
		this.MovePosition(1);
		return (BsonType)this._reader.ReadSByte();
	}

	private void MovePosition(int count)
	{
		this._currentContext.Position += count;
	}

	private byte[] ReadBytes(int count)
	{
		this.MovePosition(count);
		return this._reader.ReadBytes(count);
	}
}
