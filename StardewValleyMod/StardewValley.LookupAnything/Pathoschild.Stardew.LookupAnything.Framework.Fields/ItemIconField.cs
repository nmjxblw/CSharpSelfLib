using System.Diagnostics.CodeAnalysis;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.LookupAnything.Framework.Lookups;
using StardewValley;

namespace Pathoschild.Stardew.LookupAnything.Framework.Fields;

internal class ItemIconField : GenericField
{
	private readonly SpriteInfo? Sprite;

	private readonly ISubject? LinkSubject;

	public override bool MayHaveLinks
	{
		get
		{
			if (this.LinkSubject == null)
			{
				return base.MayHaveLinks;
			}
			return true;
		}
	}

	public ItemIconField(GameHelper gameHelper, string label, Item? item, ISubjectRegistry? codex, string? text = null)
		: base(label, item != null)
	{
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		this.Sprite = gameHelper.GetSprite(item);
		if (item != null)
		{
			this.LinkSubject = codex?.GetByEntity(item, null);
			text = ((!string.IsNullOrWhiteSpace(text)) ? text : item.DisplayName);
			Color? color = ((this.LinkSubject != null) ? new Color?(Color.Blue) : ((Color?)null));
			base.Value = new IFormattedText[1]
			{
				new FormattedText(text, color)
			};
		}
	}

	public override Vector2? DrawValue(SpriteBatch spriteBatch, SpriteFont font, Vector2 position, float wrapWidth)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		float textHeight = font.MeasureString("ABC").Y;
		Vector2 iconSize = default(Vector2);
		((Vector2)(ref iconSize))._002Ector(textHeight);
		spriteBatch.DrawSpriteWithin(this.Sprite, position.X, position.Y, iconSize);
		Vector2 textSize = spriteBatch.DrawTextBlock(font, base.Value, position + new Vector2(iconSize.X + 5f, 5f), wrapWidth);
		return new Vector2(wrapWidth, textSize.Y + 5f);
	}

	public override bool TryGetLinkAt(int x, int y, [NotNullWhen(true)] out ISubject? subject)
	{
		if (base.TryGetLinkAt(x, y, out subject))
		{
			return true;
		}
		subject = this.LinkSubject;
		return subject != null;
	}
}
