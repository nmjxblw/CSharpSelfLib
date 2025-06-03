using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Network;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;

namespace SpaceShared;

internal class SpaceUtility
{
	public static void iterateAllTerrainFeatures(Func<TerrainFeature, TerrainFeature> action)
	{
		foreach (GameLocation location in Game1.locations)
		{
			SpaceUtility._recursiveIterateLocation(location, action);
		}
	}

	protected unsafe static void _recursiveIterateLocation(GameLocation l, Func<TerrainFeature, TerrainFeature> action)
	{
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Expected O, but got Unknown
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c9: Expected O, but got Unknown
		foreach (Building b in l.buildings)
		{
			if (((NetFieldBase<GameLocation, NetRef<GameLocation>>)(object)b.indoors).Value != null)
			{
				SpaceUtility._recursiveIterateLocation(((NetFieldBase<GameLocation, NetRef<GameLocation>>)(object)b.indoors).Value, action);
			}
		}
		foreach (Vector2 key in l.objects.Keys)
		{
			Object obj = l.objects[key];
			IndoorPot pot = (IndoorPot)(object)((obj is IndoorPot) ? obj : null);
			if (pot != null)
			{
				((NetFieldBase<HoeDirt, NetRef<HoeDirt>>)(object)pot.hoeDirt).Value = (HoeDirt)action((TerrainFeature)(object)((NetFieldBase<HoeDirt, NetRef<HoeDirt>>)(object)pot.hoeDirt).Value);
			}
		}
		List<Vector2> toRemove = new List<Vector2>();
		Enumerator<Vector2, TerrainFeature, NetRef<TerrainFeature>, SerializableDictionary<Vector2, TerrainFeature>, NetVector2Dictionary<TerrainFeature, NetRef<TerrainFeature>>> enumerator3 = ((NetDictionary<Vector2, TerrainFeature, NetRef<TerrainFeature>, SerializableDictionary<Vector2, TerrainFeature>, NetVector2Dictionary<TerrainFeature, NetRef<TerrainFeature>>>)(object)l.terrainFeatures).Keys.GetEnumerator();
		try
		{
			while (enumerator3.MoveNext())
			{
				Vector2 key2 = enumerator3.Current;
				TerrainFeature ret = action(((NetDictionary<Vector2, TerrainFeature, NetRef<TerrainFeature>, SerializableDictionary<Vector2, TerrainFeature>, NetVector2Dictionary<TerrainFeature, NetRef<TerrainFeature>>>)(object)l.terrainFeatures)[key2]);
				if (ret == null)
				{
					toRemove.Add(key2);
				}
				else if (ret != ((NetDictionary<Vector2, TerrainFeature, NetRef<TerrainFeature>, SerializableDictionary<Vector2, TerrainFeature>, NetVector2Dictionary<TerrainFeature, NetRef<TerrainFeature>>>)(object)l.terrainFeatures)[key2])
				{
					((NetDictionary<Vector2, TerrainFeature, NetRef<TerrainFeature>, SerializableDictionary<Vector2, TerrainFeature>, NetVector2Dictionary<TerrainFeature, NetRef<TerrainFeature>>>)(object)l.terrainFeatures)[key2] = ret;
				}
			}
		}
		finally
		{
			((IDisposable)enumerator3/*cast due to .constrained prefix*/).Dispose();
		}
		foreach (Vector2 r in toRemove)
		{
			Vector2 val = r;
			Console.WriteLine("removing " + ((object)(*(Vector2*)(&val))/*cast due to .constrained prefix*/).ToString());
			((NetDictionary<Vector2, TerrainFeature, NetRef<TerrainFeature>, SerializableDictionary<Vector2, TerrainFeature>, NetVector2Dictionary<TerrainFeature, NetRef<TerrainFeature>>>)(object)l.terrainFeatures).Remove(r);
		}
		for (int i = l.resourceClumps.Count - 1; i >= 0; i--)
		{
			ResourceClump ret2 = (ResourceClump)action((TerrainFeature)(object)l.resourceClumps[i]);
			if (ret2 == null)
			{
				l.resourceClumps.RemoveAt(i);
			}
			else
			{
				l.resourceClumps[i] = ret2;
			}
		}
	}
}
