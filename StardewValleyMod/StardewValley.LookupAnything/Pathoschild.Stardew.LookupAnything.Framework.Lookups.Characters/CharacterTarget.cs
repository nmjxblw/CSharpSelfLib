using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Monsters;
using xTile.Dimensions;

namespace Pathoschild.Stardew.LookupAnything.Framework.Lookups.Characters;

internal class CharacterTarget : GenericTarget<NPC>
{
	public CharacterTarget(GameHelper gameHelper, SubjectType type, NPC value, Vector2 tilePosition, Func<ISubject> getSubject)
		: base(gameHelper, type, value, tilePosition, getSubject)
	{
	}//IL_0004: Unknown result type (might be due to invalid IL or missing references)


	public override Rectangle GetSpritesheetArea()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		return ((Character)base.Value).Sprite.SourceRect;
	}

	public override Rectangle GetWorldArea()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		NPC npc = base.Value;
		AnimatedSprite sprite = ((Character)npc).Sprite;
		Rectangle boundingBox = ((Character)npc).GetBoundingBox();
		float yOrigin;
		if (npc is DustSpirit)
		{
			yOrigin = ((Rectangle)(ref boundingBox)).Bottom;
		}
		else if (npc is Bat)
		{
			yOrigin = ((Rectangle)(ref boundingBox)).Center.Y;
		}
		else if (npc is Bug)
		{
			yOrigin = (float)(((Rectangle)(ref boundingBox)).Top - sprite.SpriteHeight * 4) + (float)(Math.Sin((double)Game1.currentGameTime.TotalGameTime.Milliseconds / 1000.0 * (Math.PI * 2.0)) * 10.0);
		}
		else
		{
			SquidKid squidKid = (SquidKid)(object)((npc is SquidKid) ? npc : null);
			yOrigin = ((squidKid == null) ? ((float)((Rectangle)(ref boundingBox)).Top) : ((float)(((Rectangle)(ref boundingBox)).Bottom - sprite.SpriteHeight * 4 + squidKid.yOffset)));
		}
		int height = sprite.SpriteHeight * 4;
		int width = sprite.SpriteWidth * 4;
		float x = ((Rectangle)(ref boundingBox)).Center.X - width / 2;
		float y = yOrigin + (float)boundingBox.Height - (float)height + (float)(((Character)npc).yJumpOffset * 2);
		return new Rectangle((int)(x - (float)((Rectangle)(ref Game1.uiViewport)).X), (int)(y - (float)((Rectangle)(ref Game1.uiViewport)).Y), width, height);
	}

	public override bool SpriteIntersectsPixel(Vector2 tile, Vector2 position, Rectangle spriteArea)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		NPC npc = base.Value;
		AnimatedSprite sprite = ((Character)npc).Sprite;
		if (npc is Monster)
		{
			return ((Rectangle)(ref spriteArea)).Contains((int)position.X, (int)position.Y);
		}
		SpriteEffects spriteEffects = (SpriteEffects)(((Character)npc).flip ? 1 : 0);
		return base.SpriteIntersectsPixel(tile, position, spriteArea, sprite.Texture, sprite.sourceRect, spriteEffects);
	}
}
