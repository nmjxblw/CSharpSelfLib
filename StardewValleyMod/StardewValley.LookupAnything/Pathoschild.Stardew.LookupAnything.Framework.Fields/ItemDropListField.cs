using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.LookupAnything.Framework.Data;
using Pathoschild.Stardew.LookupAnything.Framework.Lookups;
using Pathoschild.Stardew.LookupAnything.Framework.Models;
using StardewValley;

namespace Pathoschild.Stardew.LookupAnything.Framework.Fields;

internal class ItemDropListField : GenericField
{
	protected GameHelper GameHelper;

	private readonly ISubjectRegistry Codex;

	private readonly Tuple<ItemDropData, Item, SpriteInfo?>[] Drops;

	private readonly string? Preface;

	private readonly string? DefaultText;

	private readonly bool FadeNonGuaranteed;

	private readonly bool CrossOutNonGuaranteed;

	public ItemDropListField(GameHelper gameHelper, ISubjectRegistry codex, string label, IEnumerable<ItemDropData> drops, bool sort = true, bool fadeNonGuaranteed = false, bool crossOutNonGuaranteed = false, string? defaultText = null, string? preface = null)
		: base(label)
	{
		this.GameHelper = gameHelper;
		this.Codex = codex;
		this.Drops = this.GetEntries(drops, gameHelper).ToArray();
		if (sort)
		{
			this.Drops = (from p in this.Drops
				orderby p.Item1.Probability descending, p.Item2.DisplayName
				select p).ToArray();
		}
		base.HasValue = defaultText != null || this.Drops.Any();
		this.FadeNonGuaranteed = fadeNonGuaranteed;
		this.CrossOutNonGuaranteed = crossOutNonGuaranteed;
		this.Preface = preface;
		this.DefaultText = defaultText;
	}

	public override Vector2? DrawValue(SpriteBatch spriteBatch, SpriteFont font, Vector2 position, float wrapWidth)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_03dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0215: Unknown result type (might be due to invalid IL or missing references)
		//IL_0216: Unknown result type (might be due to invalid IL or missing references)
		//IL_0229: Unknown result type (might be due to invalid IL or missing references)
		//IL_022e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0235: Unknown result type (might be due to invalid IL or missing references)
		//IL_0242: Unknown result type (might be due to invalid IL or missing references)
		//IL_0247: Unknown result type (might be due to invalid IL or missing references)
		//IL_024f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0255: Unknown result type (might be due to invalid IL or missing references)
		//IL_0263: Unknown result type (might be due to invalid IL or missing references)
		//IL_026e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0276: Unknown result type (might be due to invalid IL or missing references)
		//IL_028b: Unknown result type (might be due to invalid IL or missing references)
		//IL_029c: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02af: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0313: Unknown result type (might be due to invalid IL or missing references)
		//IL_0326: Unknown result type (might be due to invalid IL or missing references)
		//IL_0327: Unknown result type (might be due to invalid IL or missing references)
		//IL_033a: Unknown result type (might be due to invalid IL or missing references)
		//IL_033f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0355: Unknown result type (might be due to invalid IL or missing references)
		//IL_035a: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0361: Unknown result type (might be due to invalid IL or missing references)
		//IL_0367: Unknown result type (might be due to invalid IL or missing references)
		//IL_0374: Unknown result type (might be due to invalid IL or missing references)
		//IL_037c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0389: Unknown result type (might be due to invalid IL or missing references)
		//IL_0395: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a2: Unknown result type (might be due to invalid IL or missing references)
		if (!this.Drops.Any())
		{
			return spriteBatch.DrawTextBlock(font, this.DefaultText, position, wrapWidth);
		}
		base.LinkTextAreas.Clear();
		float height = 0f;
		if (!string.IsNullOrWhiteSpace(this.Preface))
		{
			Vector2 prefaceSize = spriteBatch.DrawTextBlock(font, this.Preface, position, wrapWidth);
			height += (float)(int)prefaceSize.Y;
		}
		Vector2 iconSize = default(Vector2);
		((Vector2)(ref iconSize))._002Ector(font.MeasureString("ABC").Y);
		Tuple<ItemDropData, Item, SpriteInfo>[] drops = this.Drops;
		Rectangle pixelArea = default(Rectangle);
		for (int i = 0; i < drops.Length; i++)
		{
			drops[i].Deconstruct(out var item, out var item2, out var item3);
			ItemDropData drop = item;
			Item item4 = item2;
			SpriteInfo sprite = item3;
			bool isGuaranteed = drop.Probability > 0.99f;
			bool shouldFade = this.FadeNonGuaranteed && !isGuaranteed;
			bool shouldCrossOut = this.CrossOutNonGuaranteed && !isGuaranteed;
			ISubject subject = this.Codex.GetByEntity(item4, null);
			Color textColor = ((subject != null) ? Color.Blue : Color.Black) * (shouldFade ? 0.75f : 1f);
			spriteBatch.DrawSpriteWithin(sprite, position.X, position.Y + height, iconSize, shouldFade ? (Color.White * 0.5f) : Color.White);
			string text = (isGuaranteed ? item4.DisplayName : I18n.Generic_PercentChanceOf(CommonHelper.GetFormattedPercentageNumber(drop.Probability), item4.DisplayName));
			if (drop.MinDrop != drop.MaxDrop)
			{
				text = text + " (" + I18n.Generic_Range(drop.MinDrop, drop.MaxDrop) + ")";
			}
			else if (drop.MinDrop > 1)
			{
				text += $" ({drop.MinDrop})";
			}
			Vector2 textSize = spriteBatch.DrawTextBlock(font, text, position + new Vector2(iconSize.X + 5f, height + 5f), wrapWidth, textColor);
			if (subject != null)
			{
				((Rectangle)(ref pixelArea))._002Ector((int)(position.X + iconSize.X + 5f), (int)((float)(int)position.Y + height), (int)textSize.X, (int)textSize.Y);
				base.LinkTextAreas.Add(new LinkTextArea(subject, pixelArea));
			}
			if (shouldCrossOut)
			{
				DrawHelper.DrawLine(spriteBatch, position.X + iconSize.X + 5f, position.Y + height + iconSize.Y / 2f, new Vector2(textSize.X, 1f), this.FadeNonGuaranteed ? Color.Gray : Color.Black);
			}
			if (drop.Conditions != null)
			{
				string conditionText = I18n.ConditionsSummary(HumanReadableConditionParser.Format(drop.Conditions));
				height += textSize.Y + 5f;
				textSize = spriteBatch.DrawTextBlock(font, conditionText, position + new Vector2(iconSize.X + 5f, height + 5f), wrapWidth);
				if (shouldCrossOut)
				{
					DrawHelper.DrawLine(spriteBatch, position.X + iconSize.X + 5f, position.Y + height + iconSize.Y / 2f, new Vector2(textSize.X, 1f), this.FadeNonGuaranteed ? Color.Gray : Color.Black);
				}
			}
			height += textSize.Y + 5f;
		}
		return new Vector2(wrapWidth, height);
	}

	private IEnumerable<Tuple<ItemDropData, Item, SpriteInfo?>> GetEntries(IEnumerable<ItemDropData> drops, GameHelper gameHelper)
	{
		foreach (ItemDropData drop in drops)
		{
			Item item = ItemRegistry.Create(drop.ItemId, 1, 0, false);
			SpriteInfo sprite = gameHelper.GetSprite(item);
			yield return Tuple.Create<ItemDropData, Item, SpriteInfo>(drop, item, sprite);
		}
	}
}
