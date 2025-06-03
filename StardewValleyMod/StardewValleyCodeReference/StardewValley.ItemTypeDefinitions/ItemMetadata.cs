using System;

namespace StardewValley.ItemTypeDefinitions;

/// <summary>General metadata about an item accessed from the <see cref="T:StardewValley.ItemRegistry" />.</summary>
public class ItemMetadata
{
	/// <summary>The parsed item data.</summary>
	private ParsedItemData ParsedData;

	/// <summary>Whether the parsed data has been loaded.</summary>
	private bool IsParsedDataLoaded;

	/// <summary>The item type which defines it, if known.</summary>
	private IItemDataDefinition TypeDefinition;

	/// <summary>Whether we tried to resolve the <see cref="P:StardewValley.ItemTypeDefinitions.ItemMetadata.TypeIdentifier" /> (regardless of whether it was found).</summary>
	private bool IsTypeResolveAttempted;

	/// <summary>Whether the <see cref="F:StardewValley.ItemTypeDefinitions.ItemMetadata.TypeDefinition" /> is known and contains this item.</summary>
	private bool TypeDefinitionContainsItem;

	/// <summary>The unqualified item ID.</summary>
	public string LocalItemId { get; }

	/// <summary>The fully qualified item ID, if known.</summary>
	public string QualifiedItemId { get; }

	/// <summary>The item type identifier, if known.</summary>
	/// <remarks>This may be null for an unqualified item ID which hasn't been resolved via <see cref="M:StardewValley.ItemRegistry.ResolveMetadata(System.String)" /> or <see cref="M:StardewValley.ItemTypeDefinitions.ItemMetadata.GetTypeDefinition" /> yet.</remarks>
	public string TypeIdentifier { get; private set; }

	/// <summary>Construct an instance.</summary>
	/// <param name="qualifiedItemId">The fully qualified item ID.</param>
	/// <param name="localItemId">The unqualified item ID.</param>
	/// <param name="typeIdentifier">&gt;The item type identifier, if known.</param>
	public ItemMetadata(string qualifiedItemId, string localItemId, string typeIdentifier)
	{
		this.QualifiedItemId = qualifiedItemId;
		this.LocalItemId = localItemId;
		this.TypeIdentifier = typeIdentifier;
	}

	/// <summary>Set the type definition data.</summary>
	/// <param name="typeIdentifier">The type identifier for the item, if known.</param>
	/// <param name="typeDefinition">The item type which defines it, if known.</param>
	/// <param name="itemExists">Whether the item exists within the <paramref name="typeDefinition" />.</param>
	internal void SetTypeDefinition(string typeIdentifier, IItemDataDefinition typeDefinition, bool? itemExists = null)
	{
		this.TypeIdentifier = typeIdentifier;
		this.TypeDefinition = typeDefinition;
		this.IsTypeResolveAttempted = true;
		this.TypeDefinitionContainsItem = itemExists ?? typeDefinition?.Exists(this.LocalItemId) ?? false;
	}

	/// <summary>Get the item type definition which contains this item, if it's valid.</summary>
	public IItemDataDefinition GetTypeDefinition()
	{
		if (!this.IsTypeResolveAttempted)
		{
			IItemDataDefinition definition = ItemRegistry.GetTypeDefinitionFor(this);
			this.SetTypeDefinition(definition?.Identifier ?? this.TypeIdentifier, definition);
		}
		return this.TypeDefinition;
	}

	/// <summary>Get the parsed item data from the underlying type definition, if it exists.</summary>
	public ParsedItemData GetParsedData()
	{
		if (!this.IsParsedDataLoaded)
		{
			if (!this.IsTypeResolveAttempted)
			{
				this.GetTypeDefinition();
			}
			if (this.TypeDefinition != null)
			{
				try
				{
					this.ParsedData = this.TypeDefinition.GetData(this.LocalItemId);
				}
				catch (Exception exception)
				{
					Game1.log.Error($"Item type '{this.TypeIdentifier}' failed parsing item with ID '{this.LocalItemId}', defaulting to error item.", exception);
					this.ParsedData = this.TypeDefinition.GetErrorData(this.LocalItemId);
				}
			}
			else
			{
				this.ParsedData = null;
			}
			this.IsParsedDataLoaded = true;
		}
		return this.ParsedData;
	}

	/// <summary>Get the parsed item data from the underlying type definition (if it exists), else data for a generic Error Item instance.</summary>
	public ParsedItemData GetParsedOrErrorData()
	{
		return this.GetParsedData() ?? this.TypeDefinition.GetErrorData(this.LocalItemId);
	}

	/// <summary>Get whether the item ID is valid and the item definition returned by <see cref="M:StardewValley.ItemTypeDefinitions.ItemMetadata.GetTypeDefinition" /> contains this item.</summary>
	public bool Exists()
	{
		if (!this.IsTypeResolveAttempted)
		{
			this.GetTypeDefinition();
		}
		return this.TypeDefinitionContainsItem;
	}

	/// <summary>Create an item instance if the metadata is valid.</summary>
	/// <param name="amount">The stack size for the created item, if applicable.</param>
	/// <param name="quality">The quality for the created item, if applicable.</param>
	/// <returns>Returns the item instance (if the metadata is valid), else <c>null</c>.</returns>
	public Item CreateItem(int amount = 1, int quality = 0)
	{
		if (!this.Exists())
		{
			return null;
		}
		return ItemRegistry.Create(this.QualifiedItemId, amount, quality);
	}

	/// <summary>Create an item instance.</summary>
	/// <param name="amount">The stack size for the created item, if applicable.</param>
	/// <param name="quality">The quality for the created item, if applicable.</param>
	/// <returns>Returns the item instance (if the metadata is valid), else a generic Error Item.</returns>
	public Item CreateItemOrErrorItem(int amount = 1, int quality = 0)
	{
		return ItemRegistry.Create(this.QualifiedItemId, amount, quality);
	}
}
