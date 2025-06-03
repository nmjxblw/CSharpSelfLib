using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace StardewValley.Menus;

public class ClickableComponent : IScreenReadable
{
	public const int ID_ignore = -500;

	public const int CUSTOM_SNAP_BEHAVIOR = -7777;

	public const int SNAP_AUTOMATIC = -99998;

	public const int SNAP_TO_DEFAULT = -99999;

	public Rectangle bounds;

	public string name;

	public string label;

	public float scale = 1f;

	public Item item;

	public bool visible = true;

	public bool leftNeighborImmutable;

	public bool rightNeighborImmutable;

	public bool upNeighborImmutable;

	public bool downNeighborImmutable;

	public bool fullyImmutable;

	/// <summary>The ID for this component within its parent menu, referenced from fields like <see cref="F:StardewValley.Menus.ClickableComponent.leftNeighborID" /> for navigation (e.g. cursor snapping with a gamepad).</summary>
	/// <remarks>If set to <see cref="F:StardewValley.Menus.ClickableComponent.ID_ignore" />, navigation will skip this component.</remarks>
	public int myID = -500;

	/// <summary>A fallback ID for this component if no components matched via the <see cref="F:StardewValley.Menus.ClickableComponent.myID" /> field, or <see cref="F:StardewValley.Menus.ClickableComponent.ID_ignore" /> if not applicable.</summary>
	public int myAlternateID = -500;

	/// <summary>The next component to snap to when navigating to the left.</summary>
	/// <remarks>
	///   This can be set to one of...
	///   <list type="bullet">
	///     <item><description>the <see cref="F:StardewValley.Menus.ClickableComponent.myID" /> or <see cref="F:StardewValley.Menus.ClickableComponent.myAlternateID" /> value for the next component;</description></item>
	///     <item><description>or <see cref="F:StardewValley.Menus.ClickableComponent.CUSTOM_SNAP_BEHAVIOR" />, which will call <see cref="M:StardewValley.Menus.IClickableMenu.customSnapBehavior(System.Int32,System.Int32,System.Int32)" /> to handle the navigation;</description></item>
	///     <item><description>or <see cref="F:StardewValley.Menus.ClickableComponent.SNAP_AUTOMATIC" />, which will call <see cref="M:StardewValley.Menus.IClickableMenu.automaticSnapBehavior(System.Int32,System.Int32,System.Int32)" /> to handle the navigation;</description></item>
	///     <item><description>or <see cref="F:StardewValley.Menus.ClickableComponent.SNAP_TO_DEFAULT" />, which will call <see cref="M:StardewValley.Menus.IClickableMenu.snapToDefaultClickableComponent" /> to handle the navigation (note that this does nothing by default);</description></item>
	///     <item><description>or -1 if there's no neighbor in that direction.</description></item>
	///   </list>
	/// </remarks>
	public int leftNeighborID = -1;

	/// <summary>The next component to snap to when navigating to the right.</summary>
	/// <inheritdoc cref="F:StardewValley.Menus.ClickableComponent.leftNeighborID" path="/remarks" />
	public int rightNeighborID = -1;

	/// <summary>The next component to snap to when navigating upward.</summary>
	/// <inheritdoc cref="F:StardewValley.Menus.ClickableComponent.leftNeighborID" path="/remarks" />
	public int upNeighborID = -1;

	/// <summary>The next component to snap to when navigating downward.</summary>
	/// <inheritdoc cref="F:StardewValley.Menus.ClickableComponent.leftNeighborID" path="/remarks" />
	public int downNeighborID = -1;

	/// <summary>A group ID for components within a particular region of the parent menu (e.g. the crafting vs inventory areas of the crafting menu), used to implement menu behavior when navigating between sections.</summary>
	public int region;

	/// <summary>When navigating via <see cref="F:StardewValley.Menus.ClickableComponent.rightNeighborID" />, whether to call <see cref="M:StardewValley.Menus.IClickableMenu.snapToDefaultClickableComponent" /> if no matching component is found.</summary>
	public bool tryDefaultIfNoRightNeighborExists;

	/// <summary>When navigating via <see cref="F:StardewValley.Menus.ClickableComponent.downNeighborID" />, whether to call <see cref="M:StardewValley.Menus.IClickableMenu.snapToDefaultClickableComponent" /> if no matching component is found.</summary>
	public bool tryDefaultIfNoDownNeighborExists;

	/// <inheritdoc />
	public string ScreenReaderText { get; set; }

	/// <inheritdoc />
	public string ScreenReaderDescription { get; set; }

	/// <inheritdoc />
	public bool ScreenReaderIgnore { get; set; }

	public ClickableComponent(Rectangle bounds, string name)
	{
		this.bounds = bounds;
		this.name = name;
	}

	public ClickableComponent(Rectangle bounds, string name, string label)
	{
		this.bounds = bounds;
		this.name = name;
		this.label = label;
	}

	public ClickableComponent(Rectangle bounds, Item item)
	{
		this.bounds = bounds;
		this.item = item;
	}

	/// <summary>Get whether the component bounds contain the given pixel position.</summary>
	/// <param name="x">The X pixel position.</param>
	/// <param name="y">The Y pixel position.</param>
	public virtual bool containsPoint(int x, int y)
	{
		if (!this.visible)
		{
			return false;
		}
		if (this.bounds.Contains(x, y))
		{
			Game1.SetFreeCursorDrag();
			return true;
		}
		return false;
	}

	/// <summary>Get whether the component bounds contain the given pixel position.</summary>
	/// <param name="x">The X pixel position.</param>
	/// <param name="y">The Y pixel position.</param>
	/// <param name="extraMargin">An additional pixel radius around the normal bounds to check.</param>
	public virtual bool containsPoint(int x, int y, int extraMargin)
	{
		if (!this.visible)
		{
			return false;
		}
		if (new Rectangle(this.bounds.X - extraMargin, this.bounds.Y - extraMargin, this.bounds.Width + extraMargin * 2, this.bounds.Height + extraMargin * 2).Contains(x, y))
		{
			Game1.SetFreeCursorDrag();
			return true;
		}
		return false;
	}

	public void snapMouseCursorToCenter()
	{
		Game1.setMousePosition(this.bounds.Center.X, this.bounds.Center.Y);
	}

	public static void SetUpNeighbors<T>(List<T> components, int id) where T : ClickableComponent
	{
		for (int i = 0; i < components.Count; i++)
		{
			T item = components[i];
			if (item != null)
			{
				item.upNeighborID = id;
			}
		}
	}

	public static void ChainNeighborsLeftRight<T>(List<T> components) where T : ClickableComponent
	{
		ClickableComponent old_neighbor = null;
		for (int i = 0; i < components.Count; i++)
		{
			T item = components[i];
			if (item != null)
			{
				item.rightNeighborID = -1;
				item.leftNeighborID = -1;
				if (old_neighbor != null)
				{
					item.leftNeighborID = old_neighbor.myID;
					old_neighbor.rightNeighborID = item.myID;
				}
				old_neighbor = item;
			}
		}
	}

	public static void ChainNeighborsUpDown<T>(List<T> components) where T : ClickableComponent
	{
		ClickableComponent old_neighbor = null;
		for (int i = 0; i < components.Count; i++)
		{
			T item = components[i];
			if (item != null)
			{
				item.downNeighborID = -1;
				item.upNeighborID = -1;
				if (old_neighbor != null)
				{
					item.upNeighborID = old_neighbor.myID;
					old_neighbor.downNeighborID = item.myID;
				}
				old_neighbor = item;
			}
		}
	}
}
