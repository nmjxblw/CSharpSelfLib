using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.LookupAnything.Framework.Constants;
using Pathoschild.Stardew.LookupAnything.Framework.Lookups;
using Pathoschild.Stardew.LookupAnything.Framework.Lookups.Buildings;
using Pathoschild.Stardew.LookupAnything.Framework.Lookups.Characters;
using Pathoschild.Stardew.LookupAnything.Framework.Lookups.Items;
using Pathoschild.Stardew.LookupAnything.Framework.Lookups.TerrainFeatures;
using Pathoschild.Stardew.LookupAnything.Framework.Lookups.Tiles;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace Pathoschild.Stardew.LookupAnything.Framework;

internal class TargetFactory : ISubjectRegistry
{
	private const int SubjectCacheDuration = 300;

	private readonly GameHelper GameHelper;

	private readonly ILookupProvider[] LookupProviders;

	private readonly Dictionary<(object, GameLocation?), ISubject?> SubjectCache = new Dictionary<(object, GameLocation), ISubject>();

	private int SubjectCacheUntil;

	public TargetFactory(IReflectionHelper reflection, GameHelper gameHelper, Func<ModConfig> config, Func<bool> showRawTileInfo)
	{
		this.GameHelper = gameHelper;
		this.LookupProviders = new ILookupProvider[5]
		{
			new BuildingLookupProvider(reflection, gameHelper, config, this),
			new CharacterLookupProvider(reflection, gameHelper, config, this),
			new ItemLookupProvider(reflection, gameHelper, config, this),
			new TerrainFeatureLookupProvider(reflection, gameHelper, this),
			new TileLookupProvider(reflection, gameHelper, config, showRawTileInfo)
		};
	}

	public IEnumerable<ITarget> GetNearbyTargets(GameLocation location, Vector2 originTile)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		IEnumerable<ITarget> targets = this.LookupProviders.SelectMany((ILookupProvider p) => p.GetTargets(location, originTile)).WhereNotNull();
		foreach (ITarget item in targets)
		{
			yield return item;
		}
	}

	public ITarget? GetTargetFromTile(GameLocation location, Vector2 tile)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		return (from target in this.GetNearbyTargets(location, tile)
			where target.Tile == tile
			select target).FirstOrDefault();
	}

	public ITarget? GetTargetFromScreenCoordinate(GameLocation location, Vector2 tile, Vector2 position)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		Rectangle tileArea = this.GameHelper.GetScreenCoordinatesFromTile(tile);
		var candidates = (from _003C_003Eh__TransparentIdentifier1 in (from target in this.GetNearbyTargets(location, tile)
				let spriteArea = target.GetWorldArea()
				select new
				{
					_003C_003Eh__TransparentIdentifier0 = _003C_003Eh__TransparentIdentifier0,
					isAtTile = (target.Tile == tile)
				}).Where(_003C_003Eh__TransparentIdentifier1 =>
			{
				//IL_000e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0013: Unknown result type (might be due to invalid IL or missing references)
				//IL_0017: Unknown result type (might be due to invalid IL or missing references)
				if (!_003C_003Eh__TransparentIdentifier1.isAtTile)
				{
					Rectangle spriteArea = _003C_003Eh__TransparentIdentifier1._003C_003Eh__TransparentIdentifier0.spriteArea;
					return ((Rectangle)(ref spriteArea)).Intersects(tileArea);
				}
				return true;
			})
			orderby _003C_003Eh__TransparentIdentifier1._003C_003Eh__TransparentIdentifier0.target.Precedence, _003C_003Eh__TransparentIdentifier1._003C_003Eh__TransparentIdentifier0.spriteArea.Y descending, _003C_003Eh__TransparentIdentifier1._003C_003Eh__TransparentIdentifier0.spriteArea.X
			select new
			{
				_003C_003Eh__TransparentIdentifier1._003C_003Eh__TransparentIdentifier0.target,
				_003C_003Eh__TransparentIdentifier1._003C_003Eh__TransparentIdentifier0.spriteArea,
				_003C_003Eh__TransparentIdentifier1.isAtTile
			}).ToArray();
		ITarget fallback = null;
		var array = candidates;
		foreach (var candidate in array)
		{
			try
			{
				if (candidate.target.SpriteIntersectsPixel(tile, position, candidate.spriteArea))
				{
					return candidate.target;
				}
			}
			catch
			{
				if (fallback == null)
				{
					fallback = candidate.target;
				}
			}
		}
		var array2 = candidates;
		foreach (var candidate2 in array2)
		{
			if (candidate2.isAtTile)
			{
				return candidate2.target;
			}
		}
		return fallback;
	}

	public ISubject? GetSubjectFrom(Farmer player, GameLocation location, bool hasCursor)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return (hasCursor ? this.GetTargetFromScreenCoordinate(location, Game1.currentCursorTile, this.GameHelper.GetScreenCoordinatesFromCursor()) : this.GetTargetFromTile(location, this.GetFacingTile(player)))?.GetSubject();
	}

	public ISubject? GetSubjectFrom(IClickableMenu menu, Vector2 cursorPos)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		int cursorX = (int)cursorPos.X;
		int cursorY = (int)cursorPos.Y;
		return this.LookupProviders.Select((ILookupProvider p) => p.GetSubject(menu, cursorX, cursorY)).FirstOrDefault((ISubject p) => p != null);
	}

	public ISubject? GetByEntity(object entity, GameLocation? location)
	{
		(object, GameLocation) cacheKey = (entity, location);
		ISubject subject;
		if (this.SubjectCacheUntil < Game1.ticks)
		{
			this.SubjectCache.Clear();
			this.SubjectCacheUntil = Game1.ticks + 300 - 1;
		}
		else if (this.SubjectCache.TryGetValue(cacheKey, out subject))
		{
			return subject;
		}
		return this.SubjectCache[cacheKey] = this.LookupProviders.Select((ILookupProvider p) => p.GetSubjectFor(entity, location)).FirstOrDefault((ISubject p) => p != null);
	}

	public IEnumerable<ISubject> GetSearchSubjects()
	{
		return this.LookupProviders.SelectMany((ILookupProvider p) => p.GetSearchSubjects());
	}

	private Vector2 GetFacingTile(Farmer player)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		Vector2 tile = ((Character)player).Tile;
		FacingDirection direction = (FacingDirection)((Character)player).FacingDirection;
		return (Vector2)(direction switch
		{
			FacingDirection.Up => tile + new Vector2(0f, -1f), 
			FacingDirection.Right => tile + new Vector2(1f, 0f), 
			FacingDirection.Down => tile + new Vector2(0f, 1f), 
			FacingDirection.Left => tile + new Vector2(-1f, 0f), 
			_ => throw new NotSupportedException($"Unknown facing direction {direction}"), 
		});
	}
}
