using System;
using System.Globalization;
using System.IO;

namespace Newtonsoft.Json.Serialization;

internal class TraceJsonReader : JsonReader, IJsonLineInfo
{
	private readonly JsonReader _innerReader;

	private readonly JsonTextWriter _textWriter;

	private readonly StringWriter _sw;

	public override int Depth => this._innerReader.Depth;

	public override string Path => this._innerReader.Path;

	public override char QuoteChar
	{
		get
		{
			return this._innerReader.QuoteChar;
		}
		protected internal set
		{
			this._innerReader.QuoteChar = value;
		}
	}

	public override JsonToken TokenType => this._innerReader.TokenType;

	public override object? Value => this._innerReader.Value;

	public override Type? ValueType => this._innerReader.ValueType;

	int IJsonLineInfo.LineNumber
	{
		get
		{
			if (!(this._innerReader is IJsonLineInfo jsonLineInfo))
			{
				return 0;
			}
			return jsonLineInfo.LineNumber;
		}
	}

	int IJsonLineInfo.LinePosition
	{
		get
		{
			if (!(this._innerReader is IJsonLineInfo jsonLineInfo))
			{
				return 0;
			}
			return jsonLineInfo.LinePosition;
		}
	}

	public TraceJsonReader(JsonReader innerReader)
	{
		this._innerReader = innerReader;
		this._sw = new StringWriter(CultureInfo.InvariantCulture);
		this._sw.Write("Deserialized JSON: " + Environment.NewLine);
		this._textWriter = new JsonTextWriter(this._sw);
		this._textWriter.Formatting = Formatting.Indented;
	}

	public string GetDeserializedJsonMessage()
	{
		return this._sw.ToString();
	}

	public override bool Read()
	{
		bool result = this._innerReader.Read();
		this.WriteCurrentToken();
		return result;
	}

	public override int? ReadAsInt32()
	{
		int? result = this._innerReader.ReadAsInt32();
		this.WriteCurrentToken();
		return result;
	}

	public override string? ReadAsString()
	{
		string? result = this._innerReader.ReadAsString();
		this.WriteCurrentToken();
		return result;
	}

	public override byte[]? ReadAsBytes()
	{
		byte[]? result = this._innerReader.ReadAsBytes();
		this.WriteCurrentToken();
		return result;
	}

	public override decimal? ReadAsDecimal()
	{
		decimal? result = this._innerReader.ReadAsDecimal();
		this.WriteCurrentToken();
		return result;
	}

	public override double? ReadAsDouble()
	{
		double? result = this._innerReader.ReadAsDouble();
		this.WriteCurrentToken();
		return result;
	}

	public override bool? ReadAsBoolean()
	{
		bool? result = this._innerReader.ReadAsBoolean();
		this.WriteCurrentToken();
		return result;
	}

	public override DateTime? ReadAsDateTime()
	{
		DateTime? result = this._innerReader.ReadAsDateTime();
		this.WriteCurrentToken();
		return result;
	}

	public override DateTimeOffset? ReadAsDateTimeOffset()
	{
		DateTimeOffset? result = this._innerReader.ReadAsDateTimeOffset();
		this.WriteCurrentToken();
		return result;
	}

	public void WriteCurrentToken()
	{
		this._textWriter.WriteToken(this._innerReader, writeChildren: false, writeDateConstructorAsDate: false, writeComments: true);
	}

	public override void Close()
	{
		this._innerReader.Close();
	}

	bool IJsonLineInfo.HasLineInfo()
	{
		if (this._innerReader is IJsonLineInfo jsonLineInfo)
		{
			return jsonLineInfo.HasLineInfo();
		}
		return false;
	}
}
