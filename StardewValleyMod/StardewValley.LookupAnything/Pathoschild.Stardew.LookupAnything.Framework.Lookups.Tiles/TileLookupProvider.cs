using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;

namespace Pathoschild.Stardew.LookupAnything.Framework.Lookups.Tiles;

internal class TileLookupProvider : BaseLookupProvider
{
	private readonly Func<ModConfig> Config;

	private readonly Func<bool> ShowRawTileInfo;

	public TileLookupProvider(IReflectionHelper reflection, GameHelper gameHelper, Func<ModConfig> config, Func<bool> showRawTileInfo)
		: base(reflection, gameHelper)
	{
		this.Config = config;
		this.ShowRawTileInfo = showRawTileInfo;
	}

	public override IEnumerable<ITarget> GetTargets(GameLocation location, Vector2 lookupTile)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		ISubject subject = this.BuildSubject(location, lookupTile);
		if (subject != null)
		{
			yield return new TileTarget(base.GameHelper, lookupTile, () => subject);
		}
	}

	private ISubject? BuildSubject(GameLocation location, Vector2 tile)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		bool showRaw = this.ShowRawTileInfo();
		ModConfig config = this.Config();
		if (this.IsCrystalCavePuzzle(location, tile, out var crystalId))
		{
			return new CrystalCavePuzzleSubject(base.GameHelper, config, location, tile, showRaw, crystalId);
		}
		if (this.GetIsIslandMermaidPuzzle(location, tile))
		{
			return new IslandMermaidPuzzleSubject(base.GameHelper, config, location, tile, showRaw);
		}
		if (this.IsIslandShrinePuzzle(location, tile))
		{
			return new IslandShrinePuzzleSubject(base.GameHelper, config, location, tile, showRaw);
		}
		if (TileSubject.TryCreate(base.GameHelper, config, location, tile, showRaw, out TileSubject tileSubject))
		{
			return tileSubject;
		}
		return null;
	}

	private bool IsCrystalCavePuzzle(GameLocation location, Vector2 tile, out int? crystalId)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		crystalId = null;
		if (location is IslandWestCave1)
		{
			if (this.HasTileProperty(location, tile, "Action", "Buildings", out string[] actionArgs) && actionArgs.Any())
			{
				string text = actionArgs[0];
				if (text == "CrystalCaveActivate")
				{
					return true;
				}
				if (text == "Crystal")
				{
					if (actionArgs.Length > 1 && int.TryParse(actionArgs[1], out var id))
					{
						crystalId = id;
					}
					return true;
				}
			}
			else if (location.getTileIndexAt((int)tile.X, (int)tile.Y, "Buildings", (string)null) == 31)
			{
				return true;
			}
		}
		return false;
	}

	private bool GetIsIslandMermaidPuzzle(GameLocation location, Vector2 tile)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		IslandSouthEast island = (IslandSouthEast)(object)((location is IslandSouthEast) ? location : null);
		if (island != null && island.MermaidIsHere())
		{
			float x = tile.X;
			if (x >= 32f && x <= 33f)
			{
				x = tile.Y;
				if (x >= 31f)
				{
					return x <= 33f;
				}
				return false;
			}
		}
		return false;
	}

	private bool IsIslandShrinePuzzle(GameLocation location, Vector2 tile)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		if (location is IslandShrine)
		{
			float x = tile.X;
			if (x >= 23f && x <= 25f)
			{
				x = tile.Y;
				if (x >= 20f && x <= 22f)
				{
					return true;
				}
			}
			Object obj = default(Object);
			if (location.objects.TryGetValue(tile, ref obj))
			{
				return obj is ItemPedestal;
			}
			return false;
		}
		return false;
	}

	private bool HasTileProperty(GameLocation location, Vector2 tile, string name, string layer, out string[] arguments)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		string value;
		bool found = this.HasTileProperty(location, tile, name, layer, out value);
		arguments = value?.Split(' ').ToArray() ?? Array.Empty<string>();
		return found;
	}

	private bool HasTileProperty(GameLocation location, Vector2 tile, string name, string layer, [NotNullWhen(true)] out string? value)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		value = location.doesTileHaveProperty((int)tile.X, (int)tile.Y, name, layer, false);
		return value != null;
	}
}
