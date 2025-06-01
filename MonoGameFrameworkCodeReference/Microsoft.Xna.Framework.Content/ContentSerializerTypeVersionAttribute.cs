using System;

namespace Microsoft.Xna.Framework.Content;

/// <summary>
/// This is used to specify the version when deserializing this object at runtime.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class ContentSerializerTypeVersionAttribute : Attribute
{
	/// <summary>
	/// The version passed to the type at runtime.
	/// </summary>
	public int TypeVersion { get; private set; }

	/// <summary>
	/// Creates an instance of the attribute.
	/// </summary>
	/// <param name="typeVersion">The version passed to the type at runtime.</param>
	public ContentSerializerTypeVersionAttribute(int typeVersion)
	{
		this.TypeVersion = typeVersion;
	}
}
