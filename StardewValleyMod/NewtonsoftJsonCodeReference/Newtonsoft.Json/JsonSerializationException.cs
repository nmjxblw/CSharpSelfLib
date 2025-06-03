using System;
using System.Runtime.Serialization;

namespace Newtonsoft.Json;

/// <summary>
/// The exception thrown when an error occurs during JSON serialization or deserialization.
/// </summary>
[Serializable]
public class JsonSerializationException : JsonException
{
	/// <summary>
	/// Gets the line number indicating where the error occurred.
	/// </summary>
	/// <value>The line number indicating where the error occurred.</value>
	public int LineNumber { get; }

	/// <summary>
	/// Gets the line position indicating where the error occurred.
	/// </summary>
	/// <value>The line position indicating where the error occurred.</value>
	public int LinePosition { get; }

	/// <summary>
	/// Gets the path to the JSON where the error occurred.
	/// </summary>
	/// <value>The path to the JSON where the error occurred.</value>
	public string? Path { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="T:Newtonsoft.Json.JsonSerializationException" /> class.
	/// </summary>
	public JsonSerializationException()
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:Newtonsoft.Json.JsonSerializationException" /> class
	/// with a specified error message.
	/// </summary>
	/// <param name="message">The error message that explains the reason for the exception.</param>
	public JsonSerializationException(string message)
		: base(message)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:Newtonsoft.Json.JsonSerializationException" /> class
	/// with a specified error message and a reference to the inner exception that is the cause of this exception.
	/// </summary>
	/// <param name="message">The error message that explains the reason for the exception.</param>
	/// <param name="innerException">The exception that is the cause of the current exception, or <c>null</c> if no inner exception is specified.</param>
	public JsonSerializationException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:Newtonsoft.Json.JsonSerializationException" /> class.
	/// </summary>
	/// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
	/// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="info" /> parameter is <c>null</c>.</exception>
	/// <exception cref="T:System.Runtime.Serialization.SerializationException">The class name is <c>null</c> or <see cref="P:System.Exception.HResult" /> is zero (0).</exception>
	public JsonSerializationException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:Newtonsoft.Json.JsonSerializationException" /> class
	/// with a specified error message, JSON path, line number, line position, and a reference to the inner exception that is the cause of this exception.
	/// </summary>
	/// <param name="message">The error message that explains the reason for the exception.</param>
	/// <param name="path">The path to the JSON where the error occurred.</param>
	/// <param name="lineNumber">The line number indicating where the error occurred.</param>
	/// <param name="linePosition">The line position indicating where the error occurred.</param>
	/// <param name="innerException">The exception that is the cause of the current exception, or <c>null</c> if no inner exception is specified.</param>
	public JsonSerializationException(string message, string path, int lineNumber, int linePosition, Exception? innerException)
		: base(message, innerException)
	{
		this.Path = path;
		this.LineNumber = lineNumber;
		this.LinePosition = linePosition;
	}

	internal static JsonSerializationException Create(JsonReader reader, string message)
	{
		return JsonSerializationException.Create(reader, message, null);
	}

	internal static JsonSerializationException Create(JsonReader reader, string message, Exception? ex)
	{
		return JsonSerializationException.Create(reader as IJsonLineInfo, reader.Path, message, ex);
	}

	internal static JsonSerializationException Create(IJsonLineInfo? lineInfo, string path, string message, Exception? ex)
	{
		message = JsonPosition.FormatMessage(lineInfo, path, message);
		int lineNumber;
		int linePosition;
		if (lineInfo != null && lineInfo.HasLineInfo())
		{
			lineNumber = lineInfo.LineNumber;
			linePosition = lineInfo.LinePosition;
		}
		else
		{
			lineNumber = 0;
			linePosition = 0;
		}
		return new JsonSerializationException(message, path, lineNumber, linePosition, ex);
	}
}
