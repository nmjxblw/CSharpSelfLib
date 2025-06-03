using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Netcode;
using Pathoschild.Stardew.LookupAnything.Framework.Constants;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;

namespace Pathoschild.Stardew.LookupAnything.Framework.Lookups.Buildings;

internal class BuildingLookupProvider : BaseLookupProvider
{
	private readonly Func<ModConfig> Config;

	private readonly ISubjectRegistry Codex;

	public BuildingLookupProvider(IReflectionHelper reflection, GameHelper gameHelper, Func<ModConfig> config, ISubjectRegistry codex)
		: base(reflection, gameHelper)
	{
		this.Config = config;
		this.Codex = codex;
	}

	public override IEnumerable<ITarget> GetTargets(GameLocation location, Vector2 lookupTile)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		Vector2 origin = default(Vector2);
		foreach (Building building in location.buildings)
		{
			((Vector2)(ref origin))._002Ector((float)((NetFieldBase<int, NetInt>)(object)building.tileX).Value, (float)(((NetFieldBase<int, NetInt>)(object)building.tileY).Value + ((NetFieldBase<int, NetInt>)(object)building.tilesHigh).Value));
			if (base.GameHelper.CouldSpriteOccludeTile(origin, lookupTile, Constant.MaxBuildingTargetSpriteSize))
			{
				yield return new BuildingTarget(base.GameHelper, building, () => this.BuildSubject(building));
			}
		}
	}

	public override IEnumerable<ISubject> GetSearchSubjects()
	{
		foreach (string buildingId in Game1.buildingData.Keys)
		{
			Building building;
			try
			{
				building = new Building(buildingId, Vector2.Zero);
			}
			catch
			{
				continue;
			}
			yield return this.BuildSubject(building);
		}
	}

	public override ISubject? GetSubjectFor(object entity, GameLocation? location)
	{
		Building building = (Building)((entity is Building) ? entity : null);
		if (building == null)
		{
			return null;
		}
		return this.BuildSubject(building);
	}

	private ISubject BuildSubject(Building building)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		ModConfig config = this.Config();
		return new BuildingSubject(this.Codex, base.GameHelper, building, (Rectangle)(((_003F?)building.getSourceRectForMenu()) ?? building.getSourceRect()), config.CollapseLargeFields, config.ShowInvalidRecipes);
	}
}
