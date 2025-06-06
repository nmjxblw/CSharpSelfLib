using System;
using System.Runtime.Serialization;

namespace Newtonsoft.Json.Schema;

/// <summary>
/// <para>
/// Returns detailed information about the schema exception.
/// </para>
/// <note type="caution">
/// JSON Schema validation has been moved to its own package. See <see href="https://www.newtonsoft.com/jsonschema">https://www.newtonsoft.com/jsonschema</see> for more details.
/// </note>
/// </summary>
[Serializable]
[Obsolete("JSON Schema validation has been moved to its own package. See https://www.newtonsoft.com/jsonschema for more details.")]
public class JsonSchemaException : JsonException
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
	public string Path { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="T:Newtonsoft.Json.Schema.JsonSchemaException" /> class.
	/// </summary>
	public JsonSchemaException()
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:Newtonsoft.Json.Schema.JsonSchemaException" /> class
	/// with a specified error message.
	/// </summary>
	/// <param name="message">The error message that explains the reason for the exception.</param>
	public JsonSchemaException(string message)
		: base(message)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:Newtonsoft.Json.Schema.JsonSchemaException" /> class
	/// with a specified error message and a reference to the inner exception that is the cause of this exception.
	/// </summary>
	/// <param name="message">The error message that explains the reason for the exception.</param>
	/// <param name="innerException">The exception that is the cause of the current exception, or <c>null</c> if no inner exception is specified.</param>
	public JsonSchemaException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="T:Newtonsoft.Json.Schema.JsonSchemaException" /> class.
	/// </summary>
	/// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
	/// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination.</param>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="info" /> parameter is <c>null</c>.</exception>
	/// <exception cref="T:System.Runtime.Serialization.SerializationException">The class name is <c>null</c> or <see cref="P:System.Exception.HResult" /> is zero (0).</exception>
	public JsonSchemaException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}

	internal JsonSchemaException(string message, Exception innerException, string path, int lineNumber, int linePosition)
		: base(message, innerException)
	{
		this.Path = path;
		this.LineNumber = lineNumber;
		this.LinePosition = linePosition;
	}
}
