using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.Common.UI;
using Pathoschild.Stardew.LookupAnything.Framework.Models;

namespace Pathoschild.Stardew.LookupAnything.Framework.Fields;

internal class CharacterFriendshipField : GenericField
{
	private readonly FriendshipModel Friendship;

	public CharacterFriendshipField(string label, FriendshipModel friendship)
		: base(label, hasValue: true)
	{
		this.Friendship = friendship;
	}

	public override Vector2? DrawValue(SpriteBatch spriteBatch, SpriteFont font, Vector2 position, float wrapWidth)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_020d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0212: Unknown result type (might be due to invalid IL or missing references)
		//IL_0264: Unknown result type (might be due to invalid IL or missing references)
		//IL_027c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0288: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0225: Unknown result type (might be due to invalid IL or missing references)
		//IL_022b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0244: Unknown result type (might be due to invalid IL or missing references)
		//IL_0249: Unknown result type (might be due to invalid IL or missing references)
		FriendshipModel friendship = this.Friendship;
		float leftOffset = 0f;
		string statusText = I18n.For(friendship.Status, friendship.IsHousemate);
		Vector2 textSize = spriteBatch.DrawTextBlock(font, statusText, new Vector2(position.X + leftOffset, position.Y), wrapWidth - leftOffset);
		leftOffset += textSize.X + DrawHelper.GetSpaceWidth(font);
		Rectangle val;
		for (int i = 0; i < friendship.TotalHearts; i++)
		{
			Rectangle icon;
			Color color;
			if (friendship.LockedHearts >= friendship.TotalHearts - i)
			{
				icon = CommonSprites.Icons.FilledHeart;
				color = Color.Black * 0.35f;
			}
			else if (i >= friendship.FilledHearts)
			{
				icon = CommonSprites.Icons.EmptyHeart;
				color = Color.White;
			}
			else
			{
				icon = CommonSprites.Icons.FilledHeart;
				color = Color.White;
			}
			Texture2D sheet = CommonSprites.Icons.Sheet;
			Rectangle sprite = icon;
			float x = position.X + leftOffset;
			float y = position.Y;
			val = CommonSprites.Icons.FilledHeart;
			spriteBatch.DrawSprite(sheet, sprite, x, y, ((Rectangle)(ref val)).Size, color, 4f);
			leftOffset += (float)(CommonSprites.Icons.FilledHeart.Width * 4);
		}
		if (friendship.HasStardrop)
		{
			leftOffset += 1f;
			float zoom = (float)CommonSprites.Icons.EmptyHeart.Height / ((float)CommonSprites.Icons.Stardrop.Height * 1f) * 4f;
			Texture2D sheet2 = CommonSprites.Icons.Sheet;
			Rectangle stardrop = CommonSprites.Icons.Stardrop;
			float x2 = position.X + leftOffset;
			float y2 = position.Y;
			val = CommonSprites.Icons.Stardrop;
			spriteBatch.DrawSprite(sheet2, stardrop, x2, y2, ((Rectangle)(ref val)).Size, Color.White * 0.25f, zoom);
			leftOffset += (float)CommonSprites.Icons.Stardrop.Width * zoom;
		}
		string caption = null;
		FriendshipModel friendship2 = this.Friendship;
		if ((object)friendship2 != null && friendship2.EmptyHearts == 0 && friendship2.LockedHearts > 0)
		{
			caption = "(" + I18n.Npc_Friendship_NeedBouquet() + ")";
		}
		else
		{
			int pointsToNext = this.Friendship.GetPointsToNext();
			if (pointsToNext > 0)
			{
				caption = "(" + I18n.Npc_Friendship_NeedPoints(pointsToNext) + ")";
			}
		}
		float spaceSize = DrawHelper.GetSpaceWidth(font);
		Vector2 textSize2 = Vector2.Zero;
		if (caption != null)
		{
			textSize2 = spriteBatch.DrawTextBlock(font, caption, new Vector2(position.X + leftOffset + spaceSize, position.Y), wrapWidth - leftOffset);
		}
		return new Vector2((float)(CommonSprites.Icons.FilledHeart.Width * 4 * this.Friendship.TotalHearts) + textSize2.X + spaceSize, Math.Max(CommonSprites.Icons.FilledHeart.Height * 4, textSize2.Y));
	}
}
