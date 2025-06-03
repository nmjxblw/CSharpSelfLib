using System;
using System.Globalization;
using System.IO;
using System.Text;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Bson;

internal class BsonBinaryWriter
{
	private static readonly Encoding Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

	private readonly BinaryWriter _writer;

	private byte[] _largeByteBuffer;

	public DateTimeKind DateTimeKindHandling { get; set; }

	public BsonBinaryWriter(BinaryWriter writer)
	{
		this.DateTimeKindHandling = DateTimeKind.Utc;
		this._writer = writer;
	}

	public void Flush()
	{
		this._writer.Flush();
	}

	public void Close()
	{
		this._writer.Close();
	}

	public void WriteToken(BsonToken t)
	{
		this.CalculateSize(t);
		this.WriteTokenInternal(t);
	}

	private void WriteTokenInternal(BsonToken t)
	{
		switch (t.Type)
		{
		case BsonType.Object:
		{
			BsonObject bsonObject = (BsonObject)t;
			this._writer.Write(bsonObject.CalculatedSize);
			foreach (BsonProperty item in bsonObject)
			{
				this._writer.Write((sbyte)item.Value.Type);
				this.WriteString((string)item.Name.Value, item.Name.ByteCount, null);
				this.WriteTokenInternal(item.Value);
			}
			this._writer.Write((byte)0);
			break;
		}
		case BsonType.Array:
		{
			BsonArray bsonArray = (BsonArray)t;
			this._writer.Write(bsonArray.CalculatedSize);
			ulong num2 = 0uL;
			foreach (BsonToken item2 in bsonArray)
			{
				this._writer.Write((sbyte)item2.Type);
				this.WriteString(num2.ToString(CultureInfo.InvariantCulture), MathUtils.IntLength(num2), null);
				this.WriteTokenInternal(item2);
				num2++;
			}
			this._writer.Write((byte)0);
			break;
		}
		case BsonType.Integer:
		{
			BsonValue bsonValue3 = (BsonValue)t;
			this._writer.Write(Convert.ToInt32(bsonValue3.Value, CultureInfo.InvariantCulture));
			break;
		}
		case BsonType.Long:
		{
			BsonValue bsonValue4 = (BsonValue)t;
			this._writer.Write(Convert.ToInt64(bsonValue4.Value, CultureInfo.InvariantCulture));
			break;
		}
		case BsonType.Number:
		{
			BsonValue bsonValue2 = (BsonValue)t;
			this._writer.Write(Convert.ToDouble(bsonValue2.Value, CultureInfo.InvariantCulture));
			break;
		}
		case BsonType.String:
		{
			BsonString bsonString = (BsonString)t;
			this.WriteString((string)bsonString.Value, bsonString.ByteCount, bsonString.CalculatedSize - 4);
			break;
		}
		case BsonType.Boolean:
			this._writer.Write(t == BsonBoolean.True);
			break;
		case BsonType.Date:
		{
			BsonValue bsonValue = (BsonValue)t;
			long num = 0L;
			if (bsonValue.Value is DateTime dateTime)
			{
				if (this.DateTimeKindHandling == DateTimeKind.Utc)
				{
					dateTime = dateTime.ToUniversalTime();
				}
				else if (this.DateTimeKindHandling == DateTimeKind.Local)
				{
					dateTime = dateTime.ToLocalTime();
				}
				num = DateTimeUtils.ConvertDateTimeToJavaScriptTicks(dateTime, convertToUtc: false);
			}
			else
			{
				DateTimeOffset dateTimeOffset = (DateTimeOffset)bsonValue.Value;
				num = DateTimeUtils.ConvertDateTimeToJavaScriptTicks(dateTimeOffset.UtcDateTime, dateTimeOffset.Offset);
			}
			this._writer.Write(num);
			break;
		}
		case BsonType.Binary:
		{
			BsonBinary bsonBinary = (BsonBinary)t;
			byte[] array = (byte[])bsonBinary.Value;
			this._writer.Write(array.Length);
			this._writer.Write((byte)bsonBinary.BinaryType);
			this._writer.Write(array);
			break;
		}
		case BsonType.Oid:
		{
			byte[] buffer = (byte[])((BsonValue)t).Value;
			this._writer.Write(buffer);
			break;
		}
		case BsonType.Regex:
		{
			BsonRegex bsonRegex = (BsonRegex)t;
			this.WriteString((string)bsonRegex.Pattern.Value, bsonRegex.Pattern.ByteCount, null);
			this.WriteString((string)bsonRegex.Options.Value, bsonRegex.Options.ByteCount, null);
			break;
		}
		default:
			throw new ArgumentOutOfRangeException("t", "Unexpected token when writing BSON: {0}".FormatWith(CultureInfo.InvariantCulture, t.Type));
		case BsonType.Undefined:
		case BsonType.Null:
			break;
		}
	}

	private void WriteString(string s, int byteCount, int? calculatedlengthPrefix)
	{
		if (calculatedlengthPrefix.HasValue)
		{
			this._writer.Write(calculatedlengthPrefix.GetValueOrDefault());
		}
		this.WriteUtf8Bytes(s, byteCount);
		this._writer.Write((byte)0);
	}

	public void WriteUtf8Bytes(string s, int byteCount)
	{
		if (s == null)
		{
			return;
		}
		if (byteCount <= 256)
		{
			if (this._largeByteBuffer == null)
			{
				this._largeByteBuffer = new byte[256];
			}
			BsonBinaryWriter.Encoding.GetBytes(s, 0, s.Length, this._largeByteBuffer, 0);
			this._writer.Write(this._largeByteBuffer, 0, byteCount);
		}
		else
		{
			byte[] bytes = BsonBinaryWriter.Encoding.GetBytes(s);
			this._writer.Write(bytes);
		}
	}

	private int CalculateSize(int stringByteCount)
	{
		return stringByteCount + 1;
	}

	private int CalculateSizeWithLength(int stringByteCount, bool includeSize)
	{
		return ((!includeSize) ? 1 : 5) + stringByteCount;
	}

	private int CalculateSize(BsonToken t)
	{
		switch (t.Type)
		{
		case BsonType.Object:
		{
			BsonObject bsonObject = (BsonObject)t;
			int num4 = 4;
			foreach (BsonProperty item in bsonObject)
			{
				int num5 = 1;
				num5 += this.CalculateSize(item.Name);
				num5 += this.CalculateSize(item.Value);
				num4 += num5;
			}
			return bsonObject.CalculatedSize = num4 + 1;
		}
		case BsonType.Array:
		{
			BsonArray bsonArray = (BsonArray)t;
			int num2 = 4;
			ulong num3 = 0uL;
			foreach (BsonToken item2 in bsonArray)
			{
				num2++;
				num2 += this.CalculateSize(MathUtils.IntLength(num3));
				num2 += this.CalculateSize(item2);
				num3++;
			}
			num2++;
			bsonArray.CalculatedSize = num2;
			return bsonArray.CalculatedSize;
		}
		case BsonType.Integer:
			return 4;
		case BsonType.Long:
			return 8;
		case BsonType.Number:
			return 8;
		case BsonType.String:
		{
			BsonString bsonString = (BsonString)t;
			string text = (string)bsonString.Value;
			bsonString.ByteCount = ((text != null) ? BsonBinaryWriter.Encoding.GetByteCount(text) : 0);
			bsonString.CalculatedSize = this.CalculateSizeWithLength(bsonString.ByteCount, bsonString.IncludeLength);
			return bsonString.CalculatedSize;
		}
		case BsonType.Boolean:
			return 1;
		case BsonType.Undefined:
		case BsonType.Null:
			return 0;
		case BsonType.Date:
			return 8;
		case BsonType.Binary:
		{
			BsonBinary obj = (BsonBinary)t;
			byte[] array = (byte[])obj.Value;
			obj.CalculatedSize = 5 + array.Length;
			return obj.CalculatedSize;
		}
		case BsonType.Oid:
			return 12;
		case BsonType.Regex:
		{
			BsonRegex bsonRegex = (BsonRegex)t;
			int num = 0;
			num += this.CalculateSize(bsonRegex.Pattern);
			num += this.CalculateSize(bsonRegex.Options);
			bsonRegex.CalculatedSize = num;
			return bsonRegex.CalculatedSize;
		}
		default:
			throw new ArgumentOutOfRangeException("t", "Unexpected token when writing BSON: {0}".FormatWith(CultureInfo.InvariantCulture, t.Type));
		}
	}
}
