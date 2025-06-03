using System;

namespace Microsoft.Xna.Framework.Content;

/// <summary>
/// The exception that's thrown when an error occurs when loading content.
/// </summary>
public class ContentLoadException : Exception
{
	/// <summary>
	/// Create a new <see cref="T:Microsoft.Xna.Framework.Content.ContentLoadException" /> instance.
	/// </summary>
	public ContentLoadException()
	{
	}

	/// <summary>
	/// Create a new <see cref="T:Microsoft.Xna.Framework.Content.ContentLoadException" /> instance with the specified message.
	/// </summary>
	/// <param name="message">The message of the exception.</param>
	public ContentLoadException(string message)
		: base(message)
	{
	}

	/// <summary>
	/// Create a new <see cref="T:Microsoft.Xna.Framework.Content.ContentLoadException" /> instance with the specified message and inner exception.
	/// </summary>
	/// <param name="message">The message of the exception.</param>
	/// <param name="innerException">The inner exception.</param>
	public ContentLoadException(string message, Exception innerException)
		: base(message, innerException)
	{
	}
}
