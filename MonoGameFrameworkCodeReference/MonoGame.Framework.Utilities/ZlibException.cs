using System;

namespace MonoGame.Framework.Utilities;

/// <summary>
/// A general purpose exception class for exceptions in the Zlib library.
/// </summary>
internal class ZlibException : Exception
{
	/// <summary>
	/// The ZlibException class captures exception information generated
	/// by the Zlib library.
	/// </summary>
	internal ZlibException()
	{
	}

	/// <summary>
	/// This ctor collects a message attached to the exception.
	/// </summary>
	/// <param name="s">the message for the exception.</param>
	internal ZlibException(string s)
		: base(s)
	{
	}
}
