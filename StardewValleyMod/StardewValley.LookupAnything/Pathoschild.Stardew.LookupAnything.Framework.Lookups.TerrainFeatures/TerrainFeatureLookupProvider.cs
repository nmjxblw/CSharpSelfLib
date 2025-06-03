using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Network;
using StardewValley.TerrainFeatures;

namespace Pathoschild.Stardew.LookupAnything.Framework.Lookups.TerrainFeatures;

internal class TerrainFeatureLookupProvider : BaseLookupProvider
{
	private readonly ISubjectRegistry Codex;

	public TerrainFeatureLookupProvider(IReflectionHelper reflection, GameHelper gameHelper, ISubjectRegistry codex)
		: base(reflection, gameHelper)
	{
		this.Codex = codex;
	}

	public override IEnumerable<ITarget> GetTargets(GameLocation location, Vector2 lookupTile)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		Enumerator<Vector2, TerrainFeature, NetRef<TerrainFeature>, SerializableDictionary<Vector2, TerrainFeature>, NetVector2Dictionary<TerrainFeature, NetRef<TerrainFeature>>> enumerator = ((NetDictionary<Vector2, TerrainFeature, NetRef<TerrainFeature>, SerializableDictionary<Vector2, TerrainFeature>, NetVector2Dictionary<TerrainFeature, NetRef<TerrainFeature>>>)(object)location.terrainFeatures).Pairs.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				var (entityTile, feature) = (KeyValuePair<Vector2, TerrainFeature>)(ref enumerator.Current);
				if (!base.GameHelper.CouldSpriteOccludeTile(entityTile, lookupTile))
				{
					continue;
				}
				FruitTree val3 = (FruitTree)(object)((feature is FruitTree) ? feature : null);
				if (val3 == null)
				{
					Tree val4 = (Tree)(object)((feature is Tree) ? feature : null);
					if (val4 == null)
					{
						Bush val5 = (Bush)(object)((feature is Bush) ? feature : null);
						if (val5 != null)
						{
							yield return new BushTarget(base.GameHelper, val5, () => this.BuildSubject(val5));
						}
					}
					else if (val4.alpha >= 0.8f)
					{
						yield return new TreeTarget(base.GameHelper, val4, entityTile, () => this.BuildSubject(val4, entityTile));
					}
				}
				else if (val3.alpha >= 0.8f)
				{
					yield return new FruitTreeTarget(base.GameHelper, val3, entityTile, () => this.BuildSubject(val3, entityTile));
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to .constrained prefix*/).Dispose();
		}
		foreach (LargeTerrainFeature feature2 in location.largeTerrainFeatures)
		{
			Vector2 entityTile2 = ((TerrainFeature)feature2).Tile;
			if (!base.GameHelper.CouldSpriteOccludeTile(entityTile2, lookupTile))
			{
				continue;
			}
			Bush val6 = (Bush)(object)((feature2 is Bush) ? feature2 : null);
			if (val6 != null)
			{
				yield return new BushTarget(base.GameHelper, val6, () => this.BuildSubject(val6));
			}
		}
	}

	public override ISubject? GetSubjectFor(object entity, GameLocation? location)
	{
		Bush bush = (Bush)((entity is Bush) ? entity : null);
		if (bush != null)
		{
			return this.BuildSubject(bush);
		}
		return null;
	}

	private ISubject BuildSubject(Bush bush)
	{
		return new BushSubject(base.GameHelper, this.Codex, bush);
	}

	private ISubject BuildSubject(FruitTree tree, Vector2 tile)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return new FruitTreeSubject(base.GameHelper, tree, tile);
	}

	private ISubject BuildSubject(Tree tree, Vector2 tile)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		return new TreeSubject(this.Codex, base.GameHelper, tree, tile);
	}
}
