namespace Newtonsoft.Json.Utilities;

internal readonly struct StringReference
{
	private readonly char[] _chars;

	private readonly int _startIndex;

	private readonly int _length;

	public char this[int i] => this._chars[i];

	public char[] Chars => this._chars;

	public int StartIndex => this._startIndex;

	public int Length => this._length;

	public StringReference(char[] chars, int startIndex, int length)
	{
		this._chars = chars;
		this._startIndex = startIndex;
		this._length = length;
	}

	public override string ToString()
	{
		return new string(this._chars, this._startIndex, this._length);
	}
}
