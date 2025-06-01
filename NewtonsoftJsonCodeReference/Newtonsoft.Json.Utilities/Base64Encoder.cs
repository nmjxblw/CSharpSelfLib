using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Newtonsoft.Json.Utilities;

internal class Base64Encoder
{
	private const int Base64LineSize = 76;

	private const int LineSizeInBytes = 57;

	private readonly char[] _charsLine = new char[76];

	private readonly TextWriter _writer;

	private byte[]? _leftOverBytes;

	private int _leftOverBytesCount;

	public Base64Encoder(TextWriter writer)
	{
		ValidationUtils.ArgumentNotNull(writer, "writer");
		this._writer = writer;
	}

	private void ValidateEncode(byte[] buffer, int index, int count)
	{
		if (buffer == null)
		{
			throw new ArgumentNullException("buffer");
		}
		if (index < 0)
		{
			throw new ArgumentOutOfRangeException("index");
		}
		if (count < 0)
		{
			throw new ArgumentOutOfRangeException("count");
		}
		if (count > buffer.Length - index)
		{
			throw new ArgumentOutOfRangeException("count");
		}
	}

	public void Encode(byte[] buffer, int index, int count)
	{
		this.ValidateEncode(buffer, index, count);
		if (this._leftOverBytesCount > 0)
		{
			if (this.FulfillFromLeftover(buffer, index, ref count))
			{
				return;
			}
			int count2 = Convert.ToBase64CharArray(this._leftOverBytes, 0, 3, this._charsLine, 0);
			this.WriteChars(this._charsLine, 0, count2);
		}
		this.StoreLeftOverBytes(buffer, index, ref count);
		int num = index + count;
		int num2 = 57;
		while (index < num)
		{
			if (index + num2 > num)
			{
				num2 = num - index;
			}
			int count3 = Convert.ToBase64CharArray(buffer, index, num2, this._charsLine, 0);
			this.WriteChars(this._charsLine, 0, count3);
			index += num2;
		}
	}

	private void StoreLeftOverBytes(byte[] buffer, int index, ref int count)
	{
		int num = count % 3;
		if (num > 0)
		{
			count -= num;
			if (this._leftOverBytes == null)
			{
				this._leftOverBytes = new byte[3];
			}
			for (int i = 0; i < num; i++)
			{
				this._leftOverBytes[i] = buffer[index + count + i];
			}
		}
		this._leftOverBytesCount = num;
	}

	private bool FulfillFromLeftover(byte[] buffer, int index, ref int count)
	{
		int leftOverBytesCount = this._leftOverBytesCount;
		while (leftOverBytesCount < 3 && count > 0)
		{
			this._leftOverBytes[leftOverBytesCount++] = buffer[index++];
			count--;
		}
		if (count == 0 && leftOverBytesCount < 3)
		{
			this._leftOverBytesCount = leftOverBytesCount;
			return true;
		}
		return false;
	}

	public void Flush()
	{
		if (this._leftOverBytesCount > 0)
		{
			int count = Convert.ToBase64CharArray(this._leftOverBytes, 0, this._leftOverBytesCount, this._charsLine, 0);
			this.WriteChars(this._charsLine, 0, count);
			this._leftOverBytesCount = 0;
		}
	}

	private void WriteChars(char[] chars, int index, int count)
	{
		this._writer.Write(chars, index, count);
	}

	public async Task EncodeAsync(byte[] buffer, int index, int count, CancellationToken cancellationToken)
	{
		this.ValidateEncode(buffer, index, count);
		if (this._leftOverBytesCount > 0)
		{
			if (this.FulfillFromLeftover(buffer, index, ref count))
			{
				return;
			}
			int count2 = Convert.ToBase64CharArray(this._leftOverBytes, 0, 3, this._charsLine, 0);
			await this.WriteCharsAsync(this._charsLine, 0, count2, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
		}
		this.StoreLeftOverBytes(buffer, index, ref count);
		int num4 = index + count;
		int length = 57;
		while (index < num4)
		{
			if (index + length > num4)
			{
				length = num4 - index;
			}
			int count3 = Convert.ToBase64CharArray(buffer, index, length, this._charsLine, 0);
			await this.WriteCharsAsync(this._charsLine, 0, count3, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
			index += length;
		}
	}

	private Task WriteCharsAsync(char[] chars, int index, int count, CancellationToken cancellationToken)
	{
		return this._writer.WriteAsync(chars, index, count, cancellationToken);
	}

	public Task FlushAsync(CancellationToken cancellationToken)
	{
		if (cancellationToken.IsCancellationRequested)
		{
			return cancellationToken.FromCanceled();
		}
		if (this._leftOverBytesCount > 0)
		{
			int count = Convert.ToBase64CharArray(this._leftOverBytes, 0, this._leftOverBytesCount, this._charsLine, 0);
			this._leftOverBytesCount = 0;
			return this.WriteCharsAsync(this._charsLine, 0, count, cancellationToken);
		}
		return AsyncUtils.CompletedTask;
	}
}
