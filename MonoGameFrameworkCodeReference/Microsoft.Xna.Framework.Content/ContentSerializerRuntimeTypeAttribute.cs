using System;

namespace Microsoft.Xna.Framework.Content;

/// <summary>
/// This is used to specify the type to use when deserializing this object at runtime.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class ContentSerializerRuntimeTypeAttribute : Attribute
{
	/// <summary>
	/// The name of the type to use at runtime.
	/// </summary>
	public string RuntimeType { get; private set; }

	/// <summary>
	/// Creates an instance of the attribute.
	/// </summary>
	/// <param name="runtimeType">The name of the type to use at runtime.</param>
	public ContentSerializerRuntimeTypeAttribute(string runtimeType)
	{
		this.RuntimeType = runtimeType;
	}
}
