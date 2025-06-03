using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using Pathoschild.Stardew.Common;
using StardewValley;
using StardewValley.Objects;

namespace Pathoschild.Stardew.LookupAnything.Framework.Lookups.Items;

internal class ObjectTarget : GenericTarget<Object>
{
	private readonly SpriteInfo? CustomSprite;

	public ObjectTarget(GameHelper gameHelper, Object value, Vector2 tilePosition, Func<ISubject> getSubject)
		: base(gameHelper, SubjectType.Object, value, tilePosition, getSubject)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		this.CustomSprite = gameHelper.GetSprite((Item?)(object)value, onlyCustom: true);
	}

	public override Rectangle GetSpritesheetArea()
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		if (this.CustomSprite != null)
		{
			return this.CustomSprite.SourceRectangle;
		}
		Object obj = base.Value;
		Fence fence = (Fence)(object)((obj is Fence) ? obj : null);
		if (fence == null)
		{
			return ((NetFieldBase<Rectangle, NetRectangle>)(object)((Furniture)(((obj is Furniture) ? obj : null)?)).sourceRect).Value ?? ItemRegistry.GetDataOrErrorItem(((Item)obj).QualifiedItemId).GetSourceRect(0, (int?)null);
		}
		return this.GetSpritesheetArea(fence, Game1.currentLocation);
	}

	public override Rectangle GetWorldArea()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		Object obj = base.Value;
		Rectangle boundingBox = obj.GetBoundingBox();
		if (this.CustomSprite != null)
		{
			Rectangle spriteArea = base.GetSpriteArea(boundingBox, this.CustomSprite.SourceRectangle);
			return new Rectangle(spriteArea.X, spriteArea.Y - spriteArea.Height / 2, spriteArea.Width, spriteArea.Height);
		}
		return base.GetSpriteArea(boundingBox, this.GetSpritesheetArea());
	}

	public override bool SpriteIntersectsPixel(Vector2 tile, Vector2 position, Rectangle spriteArea)
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		Object obj = base.Value;
		Texture2D spriteSheet;
		if (this.CustomSprite != null)
		{
			spriteSheet = this.CustomSprite.Spritesheet;
		}
		else
		{
			Fence fence = (Fence)(object)((obj is Fence) ? obj : null);
			spriteSheet = ((fence == null) ? ItemRegistry.GetDataOrErrorItem(((Item)obj).QualifiedItemId).GetTexture() : fence.fenceTexture.Value);
		}
		Rectangle sourceRectangle = this.GetSpritesheetArea();
		SpriteEffects spriteEffects = (SpriteEffects)(obj.Flipped ? 1 : 0);
		return base.SpriteIntersectsPixel(tile, position, spriteArea, spriteSheet, sourceRectangle, spriteEffects);
	}

	private Rectangle GetSpritesheetArea(Fence fence, GameLocation location)
	{
		//IL_020e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0221: Unknown result type (might be due to invalid IL or missing references)
		//IL_023c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		int spriteID = 1;
		if ((double)((NetFieldBase<float, NetFloat>)(object)fence.health).Value > 1.0)
		{
			int index = 0;
			Vector2 tile = ((Object)fence).TileLocation;
			tile.X += 1f;
			if (location.objects.ContainsKey(tile) && location.objects[tile] is Fence && ((Fence)location.objects[tile]).countsForDrawing(((Item)fence).ItemId))
			{
				index += 100;
			}
			tile.X -= 2f;
			if (location.objects.ContainsKey(tile) && location.objects[tile] is Fence && ((Fence)location.objects[tile]).countsForDrawing(((Item)fence).ItemId))
			{
				index += 10;
			}
			tile.X += 1f;
			tile.Y += 1f;
			if (location.objects.ContainsKey(tile) && location.objects[tile] is Fence && ((Fence)location.objects[tile]).countsForDrawing(((Item)fence).ItemId))
			{
				index += 500;
			}
			tile.Y -= 2f;
			if (location.objects.ContainsKey(tile) && location.objects[tile] is Fence && ((Fence)location.objects[tile]).countsForDrawing(((Item)fence).ItemId))
			{
				index += 1000;
			}
			if (((NetFieldBase<bool, NetBool>)(object)fence.isGate).Value)
			{
				switch (index)
				{
				case 110:
					return new Rectangle((((NetFieldBase<int, NetInt>)(object)fence.gatePosition).Value == 88) ? 24 : 0, 128, 24, 32);
				case 1500:
					return new Rectangle((((NetFieldBase<int, NetInt>)(object)fence.gatePosition).Value == 0) ? 16 : 0, 160, 16, 16);
				}
				spriteID = 17;
			}
			else
			{
				spriteID = Fence.fenceDrawGuide[index];
			}
		}
		Texture2D texture = fence.fenceTexture.Value;
		return new Rectangle(spriteID * Fence.fencePieceWidth % texture.Bounds.Width, spriteID * Fence.fencePieceWidth / texture.Bounds.Width * Fence.fencePieceHeight, Fence.fencePieceWidth, Fence.fencePieceHeight);
	}
}
