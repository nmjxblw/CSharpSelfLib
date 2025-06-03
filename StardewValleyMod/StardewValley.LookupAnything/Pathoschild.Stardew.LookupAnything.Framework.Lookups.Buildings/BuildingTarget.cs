using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley;
using StardewValley.Buildings;
using xTile.Dimensions;

namespace Pathoschild.Stardew.LookupAnything.Framework.Lookups.Buildings;

internal class BuildingTarget : GenericTarget<Building>
{
	private readonly Rectangle TileArea;

	private static readonly Dictionary<string, Rectangle[]> SpriteCollisionOverrides = new Dictionary<string, Rectangle[]>
	{
		["Barn"] = (Rectangle[])(object)new Rectangle[1]
		{
			new Rectangle(48, 90, 32, 22)
		},
		["Big Barn"] = (Rectangle[])(object)new Rectangle[1]
		{
			new Rectangle(64, 90, 32, 22)
		},
		["Deluxe Barn"] = (Rectangle[])(object)new Rectangle[1]
		{
			new Rectangle(64, 90, 32, 22)
		},
		["Coop"] = (Rectangle[])(object)new Rectangle[1]
		{
			new Rectangle(33, 97, 14, 15)
		},
		["Big Coop"] = (Rectangle[])(object)new Rectangle[1]
		{
			new Rectangle(33, 97, 14, 15)
		},
		["Deluxe Coop"] = (Rectangle[])(object)new Rectangle[1]
		{
			new Rectangle(33, 97, 14, 15)
		},
		["Fish Pond"] = (Rectangle[])(object)new Rectangle[1]
		{
			new Rectangle(12, 12, 56, 56)
		}
	};

	public BuildingTarget(GameHelper gameHelper, Building value, Func<ISubject> getSubject)
		: base(gameHelper, SubjectType.Building, value, new Vector2((float)((NetFieldBase<int, NetInt>)(object)value.tileX).Value, (float)((NetFieldBase<int, NetInt>)(object)value.tileY).Value), getSubject)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		this.TileArea = new Rectangle(((NetFieldBase<int, NetInt>)(object)value.tileX).Value, ((NetFieldBase<int, NetInt>)(object)value.tileY).Value, ((NetFieldBase<int, NetInt>)(object)value.tilesWide).Value, ((NetFieldBase<int, NetInt>)(object)value.tilesHigh).Value);
	}

	public override Rectangle GetSpritesheetArea()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		return (Rectangle)(((_003F?)base.Value.getSourceRectForMenu()) ?? base.Value.getSourceRect());
	}

	public override Rectangle GetWorldArea()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		Rectangle sourceRect = this.GetSpritesheetArea();
		((Rectangle)(ref sourceRect))._002Ector(sourceRect.X * 4, sourceRect.Y * 4, sourceRect.Width * 4, sourceRect.Height * 4);
		Rectangle bounds = default(Rectangle);
		((Rectangle)(ref bounds))._002Ector(this.TileArea.X * 64, this.TileArea.Y * 64, this.TileArea.Width * 64, this.TileArea.Height * 64);
		return new Rectangle(bounds.X - (sourceRect.Width - bounds.Width + 1) - ((Rectangle)(ref Game1.uiViewport)).X, bounds.Y - (sourceRect.Height - bounds.Height + 1) - ((Rectangle)(ref Game1.uiViewport)).Y, Math.Max(bounds.Width, sourceRect.Width), Math.Max(bounds.Height, sourceRect.Height));
	}

	public override bool SpriteIntersectsPixel(Vector2 tile, Vector2 position, Rectangle spriteArea)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		Rectangle sourceRect = this.GetSpritesheetArea();
		if (base.SpriteIntersectsPixel(tile, position, spriteArea, base.Value.texture.Value, sourceRect, (SpriteEffects)0))
		{
			return true;
		}
		if (BuildingTarget.SpriteCollisionOverrides.TryGetValue(((NetFieldBase<string, NetString>)(object)base.Value.buildingType).Value, out Rectangle[] overrides))
		{
			Vector2 spriteSheetPosition = base.GameHelper.GetSpriteSheetCoordinates(position, spriteArea, sourceRect, (SpriteEffects)0);
			return overrides.Any((Rectangle p) => ((Rectangle)(ref p)).Contains((int)spriteSheetPosition.X, (int)spriteSheetPosition.Y));
		}
		return false;
	}
}
