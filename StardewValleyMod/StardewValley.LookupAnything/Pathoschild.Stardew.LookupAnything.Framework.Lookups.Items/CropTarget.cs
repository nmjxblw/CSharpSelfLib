using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley;
using StardewValley.TerrainFeatures;

namespace Pathoschild.Stardew.LookupAnything.Framework.Lookups.Items;

internal class CropTarget : GenericTarget<HoeDirt>
{
	private readonly Texture2D? Texture;

	private readonly Rectangle SourceRect;

	public CropTarget(GameHelper gameHelper, HoeDirt value, Vector2 tilePosition, Func<ISubject> getSubject)
		: base(gameHelper, SubjectType.Crop, value, tilePosition, getSubject)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		this.GetSpriteSheet(value.crop, out this.Texture, out this.SourceRect);
	}

	public override Rectangle GetSpritesheetArea()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return this.SourceRect;
	}

	public override Rectangle GetWorldArea()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		return base.GetSpriteArea(((TerrainFeature)base.Value).getBoundingBox(), this.GetSpritesheetArea());
	}

	public override bool SpriteIntersectsPixel(Vector2 tile, Vector2 position, Rectangle spriteArea)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		Crop crop = base.Value.crop;
		SpriteEffects spriteEffects = (SpriteEffects)(((NetFieldBase<bool, NetBool>)(object)crop.flip).Value ? 1 : 0);
		if (base.SpriteIntersectsPixel(tile, position, spriteArea, this.Texture, this.GetSpritesheetArea(), spriteEffects))
		{
			return true;
		}
		if (((NetFieldBase<Color, NetColor>)(object)crop.tintColor).Value != Color.White && ((NetFieldBase<int, NetInt>)(object)crop.currentPhase).Value == ((NetList<int, NetInt>)(object)crop.phaseDays).Count - 1 && !((NetFieldBase<bool, NetBool>)(object)crop.dead).Value)
		{
			return base.SpriteIntersectsPixel(tile, position, spriteArea, this.Texture, this.SourceRect, spriteEffects);
		}
		return false;
	}

	private void GetSpriteSheet(Crop target, out Texture2D? texture, out Rectangle sourceRect)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		texture = target.DrawnCropTexture;
		sourceRect = target.sourceRect;
		if (((NetFieldBase<bool, NetBool>)(object)target.forageCrop).Value && ((NetFieldBase<string, NetString>)(object)target.whichForageCrop).Value == 2.ToString())
		{
			sourceRect = new Rectangle(128 + (int)((Game1.currentGameTime.TotalGameTime.TotalMilliseconds + (double)(base.Tile.X * 111f + base.Tile.Y * 77f)) % 800.0 / 200.0) * 16, 128, 16, 16);
		}
	}
}
