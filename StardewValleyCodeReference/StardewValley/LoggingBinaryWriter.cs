using System.Collections.Generic;
using System.IO;
using Netcode;

namespace StardewValley;

public class LoggingBinaryWriter : BinaryWriter, ILoggingWriter
{
	protected BinaryWriter writer;

	protected List<KeyValuePair<string, long>> stack = new List<KeyValuePair<string, long>>();

	public override Stream BaseStream => this.writer.BaseStream;

	public LoggingBinaryWriter(BinaryWriter writer)
	{
		this.writer = writer;
	}

	private string currentPath()
	{
		if (this.stack.Count == 0)
		{
			return "";
		}
		return this.stack[this.stack.Count - 1].Key;
	}

	public void Push(string name)
	{
		this.stack.Add(new KeyValuePair<string, long>(this.currentPath() + "/" + name, this.BaseStream.Position));
	}

	public void Pop()
	{
		KeyValuePair<string, long> pair = this.stack[this.stack.Count - 1];
		string path = pair.Key;
		long start = pair.Value;
		long length = this.BaseStream.Position - start;
		this.stack.RemoveAt(this.stack.Count - 1);
		Game1.multiplayer.logging.LogWrite(path, length);
	}

	public override void Close()
	{
		base.Close();
		this.writer.Close();
	}

	public override void Flush()
	{
		this.writer.Flush();
	}

	public override long Seek(int offset, SeekOrigin origin)
	{
		return this.writer.Seek(offset, origin);
	}

	public override void Write(short value)
	{
		this.writer.Write(value);
	}

	public override void Write(ushort value)
	{
		this.writer.Write(value);
	}

	public override void Write(int value)
	{
		this.writer.Write(value);
	}

	public override void Write(uint value)
	{
		this.writer.Write(value);
	}

	public override void Write(long value)
	{
		this.writer.Write(value);
	}

	public override void Write(ulong value)
	{
		this.writer.Write(value);
	}

	public override void Write(float value)
	{
		this.writer.Write(value);
	}

	public override void Write(string value)
	{
		this.writer.Write(value);
	}

	public override void Write(decimal value)
	{
		this.writer.Write(value);
	}

	public override void Write(bool value)
	{
		this.writer.Write(value);
	}

	public override void Write(byte value)
	{
		this.writer.Write(value);
	}

	public override void Write(sbyte value)
	{
		this.writer.Write(value);
	}

	public override void Write(byte[] buffer)
	{
		this.writer.Write(buffer);
	}

	public override void Write(byte[] buffer, int index, int count)
	{
		this.writer.Write(buffer, index, count);
	}

	public override void Write(char ch)
	{
		this.writer.Write(ch);
	}

	public override void Write(char[] chars)
	{
		this.writer.Write(chars);
	}

	public override void Write(char[] chars, int index, int count)
	{
		this.writer.Write(chars, index, count);
	}

	public override void Write(double value)
	{
		this.writer.Write(value);
	}
}
