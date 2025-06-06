using System;

namespace Newtonsoft.Json.Serialization;

/// <summary>
/// Provides information surrounding an error.
/// </summary>
public class ErrorContext
{
	internal bool Traced { get; set; }

	/// <summary>
	/// Gets the error.
	/// </summary>
	/// <value>The error.</value>
	public Exception Error { get; }

	/// <summary>
	/// Gets the original object that caused the error.
	/// </summary>
	/// <value>The original object that caused the error.</value>
	public object? OriginalObject { get; }

	/// <summary>
	/// Gets the member that caused the error.
	/// </summary>
	/// <value>The member that caused the error.</value>
	public object? Member { get; }

	/// <summary>
	/// Gets the path of the JSON location where the error occurred.
	/// </summary>
	/// <value>The path of the JSON location where the error occurred.</value>
	public string Path { get; }

	/// <summary>
	/// Gets or sets a value indicating whether this <see cref="T:Newtonsoft.Json.Serialization.ErrorContext" /> is handled.
	/// </summary>
	/// <value><c>true</c> if handled; otherwise, <c>false</c>.</value>
	public bool Handled { get; set; }

	internal ErrorContext(object? originalObject, object? member, string path, Exception error)
	{
		this.OriginalObject = originalObject;
		this.Member = member;
		this.Error = error;
		this.Path = path;
	}
}
