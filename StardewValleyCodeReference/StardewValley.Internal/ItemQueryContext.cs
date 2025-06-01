using System;
using System.Collections.Generic;

namespace StardewValley.Internal;

/// <summary>The game context for an item search query.</summary>
public class ItemQueryContext
{
	/// <summary>The location to use for location-dependent queries like season.</summary>
	public GameLocation Location { get; }

	/// <summary>The player for which to perform the search.</summary>
	public Farmer Player { get; }

	/// <summary>The instance to use for randomization, or <c>null</c> to create one dynamically.</summary>
	public Random Random { get; }

	/// <summary>The full item query string.</summary>
	public string QueryString { get; internal set; }

	/// <summary>The context for the item query which triggered this query, if any.</summary>
	/// <remarks>
	///   <para>For example, <c>LOST_BOOK_OR_ITEM RANDOM_ITEMS (O)</c> contains a fallback <c>RANDOM_ITEMS (O)</c> query which is parsed if the player already found every lost book. In that case, the former is the parent context for the latter.</para>
	///   <para>This is used to detect and break circular references.</para>
	/// </remarks>
	public ItemQueryContext ParentContext { get; }

	/// <summary>If set, a human-readable phrase which describes what's loading the item query.</summary>
	/// <remarks>This identifies what's loading the item query in a hierarchical human-readable way, like <c>"building 'Mill' &gt; item conversion rule 'Default'"</c>. This should be formatted for use within a log message like <c>"Item spawn fields for {0} produced a null item"</c>.</remarks>
	public string SourcePhrase { get; set; }

	/// <summary>The custom fields which can be set by mods for custom item query behavior, or <c>null</c> if none were set.</summary>
	public Dictionary<string, object> CustomFields { get; set; }

	/// <summary>Construct an instance.</summary>
	public ItemQueryContext()
		: this(null, null, null, null)
	{
	}

	/// <summary>Construct an instance.</summary>
	/// <param name="parentContext">The context for the item query which triggered this query, if any.</param>
	/// <param name="sourceLabel">If set, a human-readable phrase which describes what's loading the item query. See remarks on <see cref="P:StardewValley.Internal.ItemQueryContext.SourcePhrase" />.</param>
	public ItemQueryContext(ItemQueryContext parentContext, string sourceLabel = null)
		: this(parentContext?.Location, parentContext?.Player, parentContext?.Random, parentContext?.SourcePhrase)
	{
		this.ParentContext = parentContext;
		if (sourceLabel != null)
		{
			this.SourcePhrase = ((parentContext != null && parentContext.SourcePhrase != null) ? (parentContext.SourcePhrase + " > " + sourceLabel) : sourceLabel);
		}
	}

	/// <summary>Construct an instance.</summary>
	/// <param name="location">The location to use for location-dependent queries like season.</param>
	/// <param name="player">The player for which to perform the search.</param>
	/// <param name="random">The instance to use for randomization, or <c>null</c> to create one dynamically.</param>
	/// <param name="sourcePhrase">If set, a human-readable phrase which describes what's loading the item query. See remarks on <see cref="P:StardewValley.Internal.ItemQueryContext.SourcePhrase" />.</param>
	public ItemQueryContext(GameLocation location, Farmer player, Random random, string sourcePhrase)
	{
		this.Location = location ?? Game1.currentLocation;
		this.Player = player ?? Game1.player;
		this.Random = random;
		this.SourcePhrase = sourcePhrase;
	}
}
