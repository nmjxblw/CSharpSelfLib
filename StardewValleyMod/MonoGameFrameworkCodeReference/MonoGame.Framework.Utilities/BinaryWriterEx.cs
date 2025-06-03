using System.IO;
using System.Text;

namespace MonoGame.Framework.Utilities;

public class BinaryWriterEx : BinaryWriter
{
	protected BinaryWriterEx()
	{
	}

	public BinaryWriterEx(Stream output)
		: base(output)
	{
	}

	public BinaryWriterEx(Stream output, Encoding encoding)
		: base(output, encoding)
	{
	}

	public BinaryWriterEx(Stream output, Encoding encoding, bool leaveOpen)
		: base(output, encoding, leaveOpen)
	{
	}

	public new void Write7BitEncodedInt(int value)
	{
		base.Write7BitEncodedInt(value);
	}
}
