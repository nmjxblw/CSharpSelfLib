using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.GameData.WildTrees;
using StardewValley.TerrainFeatures;

namespace Pathoschild.Stardew.LookupAnything.Framework.Lookups.TerrainFeatures;

internal class TreeTarget : GenericTarget<Tree>
{
	public TreeTarget(GameHelper gameHelper, Tree value, Vector2 tilePosition, Func<ISubject> getSubject)
		: base(gameHelper, SubjectType.WildTree, value, tilePosition, getSubject)
	{
	}//IL_0005: Unknown result type (might be due to invalid IL or missing references)


	public override Rectangle GetSpritesheetArea()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Expected I4, but got Unknown
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		Tree tree = base.Value;
		if (((NetFieldBase<bool, NetBool>)(object)tree.stump).Value)
		{
			return Tree.stumpSourceRect;
		}
		if (((NetFieldBase<int, NetInt>)(object)tree.growthStage).Value < 5)
		{
			WildTreeGrowthStage val = (WildTreeGrowthStage)((NetFieldBase<int, NetInt>)(object)tree.growthStage).Value;
			return (Rectangle)((int)val switch
			{
				0 => new Rectangle(32, 128, 16, 16), 
				1 => new Rectangle(0, 128, 16, 16), 
				2 => new Rectangle(16, 128, 16, 16), 
				_ => new Rectangle(0, 96, 16, 32), 
			});
		}
		return Tree.treeTopSourceRect;
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
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Invalid comparison between Unknown and I4
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		Tree tree = base.Value;
		WildTreeGrowthStage growth = (WildTreeGrowthStage)((NetFieldBase<int, NetInt>)(object)tree.growthStage).Value;
		Texture2D spriteSheet = tree.texture.Value;
		SpriteEffects spriteEffects = (SpriteEffects)(((NetFieldBase<bool, NetBool>)(object)tree.flipped).Value ? 1 : 0);
		if (base.SpriteIntersectsPixel(tile, position, spriteArea, spriteSheet, this.GetSpritesheetArea(), spriteEffects))
		{
			return true;
		}
		if ((int)growth == 5)
		{
			Rectangle stumpSpriteArea = default(Rectangle);
			((Rectangle)(ref stumpSpriteArea))._002Ector(((Rectangle)(ref spriteArea)).Center.X - Tree.stumpSourceRect.Width / 2 * 4, spriteArea.Y + spriteArea.Height - Tree.stumpSourceRect.Height * 4, Tree.stumpSourceRect.Width * 4, Tree.stumpSourceRect.Height * 4);
			if (((Rectangle)(ref stumpSpriteArea)).Contains((int)position.X, (int)position.Y) && base.SpriteIntersectsPixel(tile, position, stumpSpriteArea, spriteSheet, Tree.stumpSourceRect, spriteEffects))
			{
				return true;
			}
		}
		return false;
	}
}
