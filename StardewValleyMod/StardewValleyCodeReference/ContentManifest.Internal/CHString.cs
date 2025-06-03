using System;
using System.Text;

namespace ContentManifest.Internal;

internal class CHString : CHParsable
{
	public string RawString = "";

	public void Parse(CHJsonParserContext context)
	{
		if (context.JsonText[context.ReadHead] != '"')
		{
			throw new InvalidOperationException();
		}
		context.ReadHead++;
		int readHead = context.ReadHead;
		string jsonText = context.JsonText;
		StringBuilder sb = new StringBuilder();
		for (; readHead < jsonText.Length; readHead++)
		{
			char readChar = jsonText[readHead];
			switch (readChar)
			{
			case '"':
				this.RawString = sb.ToString();
				context.ReadHead = readHead + 1;
				return;
			case '\\':
			{
				readHead++;
				if (readHead >= jsonText.Length)
				{
					throw new InvalidOperationException();
				}
				char escapeChar = jsonText[readHead];
				switch (escapeChar)
				{
				case '"':
				case '/':
				case '\\':
					sb.Append(escapeChar);
					break;
				case 'b':
					sb.Append('\b');
					break;
				case 'f':
					sb.Append('\f');
					break;
				case 'r':
					sb.Append('\r');
					break;
				case 'n':
					sb.Append('\n');
					break;
				case 't':
					sb.Append('\t');
					break;
				case 'u':
				{
					if (readHead + 4 >= jsonText.Length)
					{
						throw new InvalidOperationException();
					}
					string decodedString = char.ConvertFromUtf32(0 | ((this.ParseHexChar(jsonText[readHead + 1]) & 0xF) << 12) | ((this.ParseHexChar(jsonText[readHead + 2]) & 0xF) << 8) | ((this.ParseHexChar(jsonText[readHead + 3]) & 0xF) << 4) | (this.ParseHexChar(jsonText[readHead + 4]) & 0xF));
					if (decodedString.Length != 1)
					{
						throw new InvalidOperationException();
					}
					sb.Append(decodedString[0]);
					readHead += 4;
					break;
				}
				}
				break;
			}
			default:
				sb.Append(readChar);
				break;
			}
		}
		throw new InvalidOperationException();
	}

	private int ParseHexChar(char hexChar)
	{
		if ('0' <= hexChar && hexChar < '9')
		{
			return hexChar - 48;
		}
		if ('a' <= hexChar && hexChar <= 'z')
		{
			return hexChar - 97 + 10;
		}
		if ('A' <= hexChar && hexChar <= 'Z')
		{
			return hexChar - 65 + 10;
		}
		throw new InvalidOperationException();
	}
}
