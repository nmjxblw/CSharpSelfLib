using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley;
using StardewValley.TerrainFeatures;
using xTile.Dimensions;

namespace Pathoschild.Stardew.LookupAnything.Framework.Lookups.TerrainFeatures;

internal class FruitTreeTarget : GenericTarget<FruitTree>
{
	private readonly Texture2D? Texture;

	private readonly Rectangle SourceRect;

	public FruitTreeTarget(GameHelper gameHelper, FruitTree value, Vector2 tilePosition, Func<ISubject> getSubject)
		: base(gameHelper, SubjectType.FruitTree, value, tilePosition, getSubject)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		this.GetSpriteSheet(value, out this.Texture, out this.SourceRect);
	}

	public override Rectangle GetSpritesheetArea()
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		FruitTree tree = base.Value;
		if (((NetFieldBase<bool, NetBool>)(object)tree.stump).Value)
		{
			return new Rectangle(this.SourceRect.X + 384, this.SourceRect.Y + 48, 48, 32);
		}
		if (((NetFieldBase<int, NetInt>)(object)tree.growthStage).Value < 4)
		{
			int value = ((NetFieldBase<int, NetInt>)(object)tree.growthStage).Value;
			if ((uint)value <= 2u)
			{
				return new Rectangle(this.SourceRect.X + ((NetFieldBase<int, NetInt>)(object)tree.growthStage).Value * 48, this.SourceRect.Y, 48, 80);
			}
			return new Rectangle(this.SourceRect.X + 144, this.SourceRect.Y, 48, 80);
		}
		return new Rectangle(this.SourceRect.X + (12 + (tree.IgnoresSeasonsHere() ? 1 : Game1.seasonIndex) * 3) * 16, this.SourceRect.Y, 48, 80);
	}

	public override Rectangle GetWorldArea()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		FruitTree tree = base.Value;
		Rectangle sprite = this.GetSpritesheetArea();
		int width = sprite.Width * 4;
		int height = sprite.Height * 4;
		int x;
		int y;
		if (((NetFieldBase<int, NetInt>)(object)tree.growthStage).Value < 4)
		{
			Vector2 tile = base.Tile;
			Vector2 offset = default(Vector2);
			((Vector2)(ref offset))._002Ector((float)Math.Max(-8.0, Math.Min(64.0, Math.Sin((double)tile.X * 200.0 / (Math.PI * 2.0)) * -16.0)), (float)Math.Max(-8.0, Math.Min(64.0, Math.Sin((double)tile.X * 200.0 / (Math.PI * 2.0)) * -16.0)));
			Vector2 centerBottom = new Vector2(tile.X * 64f + 32f + offset.X, tile.Y * 64f - (float)sprite.Height + 128f + offset.Y) - new Vector2((float)((Rectangle)(ref Game1.uiViewport)).X, (float)((Rectangle)(ref Game1.uiViewport)).Y);
			x = (int)centerBottom.X - width / 2;
			y = (int)centerBottom.Y - height;
		}
		else
		{
			Rectangle tileArea = base.GetWorldArea();
			x = ((Rectangle)(ref tileArea)).Center.X - width / 2;
			y = ((Rectangle)(ref tileArea)).Bottom - height;
		}
		return new Rectangle(x, y, width, height);
	}

	public override bool SpriteIntersectsPixel(Vector2 tile, Vector2 position, Rectangle spriteArea)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		SpriteEffects spriteEffects = (SpriteEffects)(((NetFieldBase<bool, NetBool>)(object)base.Value.flipped).Value ? 1 : 0);
		return base.SpriteIntersectsPixel(tile, position, spriteArea, this.Texture, this.GetSpritesheetArea(), spriteEffects);
	}

	public void GetSpriteSheet(FruitTree target, out Texture2D? texture, out Rectangle sourceRect)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		texture = target.texture;
		sourceRect = new Rectangle(0, target.GetSpriteRowNumber() * 5 * 16, 432, 80);
	}
}
