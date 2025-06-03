using System;

namespace Microsoft.Xna.Framework.Content;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class ContentSerializerAttribute : Attribute
{
	private string _collectionItemName;

	public bool AllowNull { get; set; }

	/// <summary>
	/// Returns the overriden XML element name or the default "Item".
	/// </summary>
	public string CollectionItemName
	{
		get
		{
			if (string.IsNullOrEmpty(this._collectionItemName))
			{
				return "Item";
			}
			return this._collectionItemName;
		}
		set
		{
			this._collectionItemName = value;
		}
	}

	public string ElementName { get; set; }

	public bool FlattenContent { get; set; }

	/// <summary>
	/// Returns true if the default CollectionItemName value was overridden.
	/// </summary>
	public bool HasCollectionItemName => !string.IsNullOrEmpty(this._collectionItemName);

	public bool Optional { get; set; }

	public bool SharedResource { get; set; }

	/// <summary>
	/// Creates an instance of the attribute.
	/// </summary>
	public ContentSerializerAttribute()
	{
		this.AllowNull = true;
	}

	public ContentSerializerAttribute Clone()
	{
		return new ContentSerializerAttribute
		{
			AllowNull = this.AllowNull,
			_collectionItemName = this._collectionItemName,
			ElementName = this.ElementName,
			FlattenContent = this.FlattenContent,
			Optional = this.Optional,
			SharedResource = this.SharedResource
		};
	}
}
