using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.Buildings;
using StardewValley.Delegates;
using StardewValley.Inventories;
using StardewValley.Network;

namespace StardewValley.Internal;

/// <summary>The metadata and operations for an item being iterated via a method like <see cref="M:StardewValley.Utility.ForEachItem(System.Func{StardewValley.Item,System.Boolean})" />.</summary>
public readonly struct ForEachItemContext
{
	/// <summary>The current item in the iteration.</summary>
	public readonly Item Item;

	/// <summary>Delete this item from the game.</summary>
	public readonly Action RemoveItem;

	/// <summary>Remove this item and replace it with the given instance.</summary>
	public readonly Action<Item> ReplaceItemWith;

	/// <summary>Get the contextual path leading to this item. For example, an item inside a chest would have the location and chest as path values.</summary>
	public readonly GetForEachItemPathDelegate GetPath;

	/// <summary>Set the contextual values. This should only be called by the code which implements the iteration.</summary>
	/// <param name="item"><inheritdoc cref="F:StardewValley.Internal.ForEachItemContext.Item" path="/summary" /></param>
	/// <param name="remove"><inheritdoc cref="F:StardewValley.Internal.ForEachItemContext.RemoveItem" path="/summary" /></param>
	/// <param name="replaceWith"><inheritdoc cref="F:StardewValley.Internal.ForEachItemContext.ReplaceItemWith" path="/summary" /></param>
	/// <param name="getPath"><inheritdoc cref="F:StardewValley.Internal.ForEachItemContext.GetPath" path="/summary" /></param>
	public ForEachItemContext(Item item, Action remove, Action<Item> replaceWith, GetForEachItemPathDelegate getPath)
	{
		this.Item = item;
		this.RemoveItem = remove;
		this.ReplaceItemWith = replaceWith;
		this.GetPath = getPath;
	}

	/// <summary>Get a human-readable representation of the <see cref="F:StardewValley.Internal.ForEachItemContext.GetPath" /> values.</summary>
	/// <param name="includeItem">Whether to add a segment for the item itself.</param>
	public IList<string> GetDisplayPath(bool includeItem = false)
	{
		List<string> path = new List<string>();
		foreach (object pathValue in this.GetPath())
		{
			this.AddDisplayPath(path, pathValue);
		}
		if (includeItem)
		{
			this.AddDisplayPath(path, this.Item);
		}
		return path;
	}

	/// <summary>Add human-readable path segments path for a raw <see cref="F:StardewValley.Internal.ForEachItemContext.GetPath" /> value.</summary>
	/// <param name="path">The path to populate.</param>
	/// <param name="pathValue">The segment from <see cref="F:StardewValley.Internal.ForEachItemContext.GetPath" /> to represent.</param>
	private void AddDisplayPath(IList<string> path, object pathValue)
	{
		if (!(pathValue is GameLocation location))
		{
			if (!(pathValue is Building building))
			{
				if (!(pathValue is Object parentObj))
				{
					if (!(pathValue is Farmer player))
					{
						if (!(pathValue is Item parentItem))
						{
							if (!(pathValue is INetSerializable field))
							{
								if (!(pathValue is IInventory) && !(pathValue is OverlaidDictionary))
								{
									path.Add(pathValue.ToString());
								}
							}
							else
							{
								path.Add(field.Name);
							}
						}
						else
						{
							path.Add(parentItem.Name);
						}
					}
					else
					{
						path.Add("player '" + player.Name + "'");
					}
				}
				else
				{
					if (path.Count == 0 && parentObj.Location != null)
					{
						this.AddDisplayPath(path, parentObj.Location);
					}
					path.Add((parentObj.TileLocation != Vector2.Zero) ? $"{parentObj.Name} at {parentObj.TileLocation.X}, {parentObj.TileLocation.Y}" : parentObj.Name);
				}
				return;
			}
			if (path.Count == 0)
			{
				GameLocation location2 = building.GetParentLocation();
				if (location2 != null)
				{
					this.AddDisplayPath(path, location2);
				}
			}
			path.Add($"{building.buildingType.Value} at {building.tileX.Value}, {building.tileY.Value}");
		}
		else
		{
			if (path.Count == 0 && location.ParentBuilding != null)
			{
				this.AddDisplayPath(path, location.ParentBuilding);
			}
			path.Add(location.NameOrUniqueName);
		}
	}
}
