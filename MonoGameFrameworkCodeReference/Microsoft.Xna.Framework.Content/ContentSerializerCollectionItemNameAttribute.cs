using System;

namespace Microsoft.Xna.Framework.Content;

/// <summary>
/// This is used to specify the XML element name to use for each item in a collection.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class ContentSerializerCollectionItemNameAttribute : Attribute
{
	/// <summary>
	/// The XML element name to use for each item in the collection.
	/// </summary>
	public string CollectionItemName { get; private set; }

	/// <summary>
	/// Creates an instance of the attribute.
	/// </summary>
	/// <param name="collectionItemName">The XML element name to use for each item in the collection.</param>
	public ContentSerializerCollectionItemNameAttribute(string collectionItemName)
	{
		this.CollectionItemName = collectionItemName;
	}
}
