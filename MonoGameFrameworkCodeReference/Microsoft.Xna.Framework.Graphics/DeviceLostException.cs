using System;
using System.Runtime.Serialization;

namespace Microsoft.Xna.Framework.Graphics;

[DataContract]
public sealed class DeviceLostException : Exception
{
	public DeviceLostException()
	{
	}

	public DeviceLostException(string message)
		: base(message)
	{
	}

	public DeviceLostException(string message, Exception inner)
		: base(message, inner)
	{
	}
}
