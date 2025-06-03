using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.Common.UI;
using Pathoschild.Stardew.LookupAnything.Framework.Data;
using Pathoschild.Stardew.LookupAnything.Framework.Fields.Models;
using Pathoschild.Stardew.LookupAnything.Framework.Lookups;
using Pathoschild.Stardew.LookupAnything.Framework.Models;
using StardewValley;
using StardewValley.GameData.FishPonds;

namespace Pathoschild.Stardew.LookupAnything.Framework.Fields;

internal class FishPondDropsField : GenericField
{
	protected GameHelper GameHelper;

	private readonly ISubjectRegistry Codex;

	private readonly FishPondDrop[] Drops;

	private readonly string Preface;

	public FishPondDropsField(GameHelper gameHelper, ISubjectRegistry codex, string label, int currentPopulation, FishPondData data, Object? fish, string preface)
		: base(label)
	{
		this.GameHelper = gameHelper;
		this.Codex = codex;
		this.Drops = (from drop in this.GetEntries(currentPopulation, data, fish, gameHelper)
			orderby drop.Precedence, drop.MinPopulation descending
			select drop).ToArray();
		base.HasValue = this.Drops.Any();
		this.Preface = preface;
	}

	public override Vector2? DrawValue(SpriteBatch spriteBatch, SpriteFont font, Vector2 position, float wrapWidth)
	{
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0538: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_024d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0246: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0262: Unknown result type (might be due to invalid IL or missing references)
		//IL_0267: Unknown result type (might be due to invalid IL or missing references)
		//IL_0271: Unknown result type (might be due to invalid IL or missing references)
		//IL_027a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0282: Unknown result type (might be due to invalid IL or missing references)
		//IL_0284: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0299: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0371: Unknown result type (might be due to invalid IL or missing references)
		//IL_037f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0386: Unknown result type (might be due to invalid IL or missing references)
		//IL_0393: Unknown result type (might be due to invalid IL or missing references)
		//IL_0398: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0406: Unknown result type (might be due to invalid IL or missing references)
		//IL_0414: Unknown result type (might be due to invalid IL or missing references)
		//IL_041c: Unknown result type (might be due to invalid IL or missing references)
		//IL_042a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0436: Unknown result type (might be due to invalid IL or missing references)
		//IL_043b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0506: Unknown result type (might be due to invalid IL or missing references)
		//IL_046a: Unknown result type (might be due to invalid IL or missing references)
		//IL_047f: Unknown result type (might be due to invalid IL or missing references)
		//IL_048d: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_04cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_04da: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ef: Unknown result type (might be due to invalid IL or missing references)
		base.LinkTextAreas.Clear();
		float height = 0f;
		if (!string.IsNullOrWhiteSpace(this.Preface))
		{
			Vector2 prefaceSize = spriteBatch.DrawTextBlock(font, this.Preface, position, wrapWidth);
			height += (float)(int)prefaceSize.Y;
		}
		float checkboxSize = CommonSprites.Icons.FilledCheckbox.Width * 2;
		float lineHeight = Math.Max(checkboxSize, Game1.smallFont.MeasureString("ABC").Y);
		float checkboxOffset = (lineHeight - checkboxSize) / 2f;
		float outerIndent = checkboxSize + 7f;
		float innerIndent = outerIndent * 2f;
		Vector2 iconSize = default(Vector2);
		((Vector2)(ref iconSize))._002Ector(font.MeasureString("ABC").Y);
		int lastGroup = -1;
		bool isPrevDropGuaranteed = false;
		FishPondDrop[] drops = this.Drops;
		Rectangle pixelArea = default(Rectangle);
		foreach (FishPondDrop drop in drops)
		{
			bool disabled = !drop.IsUnlocked || isPrevDropGuaranteed;
			if (lastGroup != drop.MinPopulation)
			{
				lastGroup = drop.MinPopulation;
				spriteBatch.Draw(CommonSprites.Icons.Sheet, new Vector2(position.X + outerIndent, position.Y + height + checkboxOffset), (Rectangle?)(drop.IsUnlocked ? CommonSprites.Icons.FilledCheckbox : CommonSprites.Icons.EmptyCheckbox), Color.White * (disabled ? 0.5f : 1f), 0f, Vector2.Zero, checkboxSize / (float)CommonSprites.Icons.FilledCheckbox.Width, (SpriteEffects)0, 1f);
				Vector2 textSize = spriteBatch.DrawTextBlock(Game1.smallFont, I18n.Building_FishPond_Drops_MinFish(drop.MinPopulation), new Vector2(position.X + outerIndent + checkboxSize + 7f, position.Y + height), wrapWidth - checkboxSize - 7f, disabled ? Color.Gray : Color.Black);
				if (isPrevDropGuaranteed)
				{
					DrawHelper.DrawLine(spriteBatch, position.X + outerIndent + checkboxSize + 7f, position.Y + height + iconSize.Y / 2f, new Vector2(textSize.X, 1f), Color.Gray);
				}
				height += Math.Max(checkboxSize, textSize.Y);
			}
			bool isGuaranteed = drop.Probability > 0.99f;
			ISubject subject = this.Codex.GetByEntity(drop.SampleItem, null);
			Color textColor = ((subject != null) ? Color.Blue : Color.Black) * (disabled ? 0.75f : 1f);
			spriteBatch.DrawSpriteWithin(drop.Sprite, position.X + innerIndent, position.Y + height, iconSize, Color.White * (disabled ? 0.5f : 1f));
			float textIndent = position.X + innerIndent + iconSize.X + 5f;
			string text = I18n.Generic_PercentChanceOf(CommonHelper.GetFormattedPercentageNumber(drop.Probability), drop.SampleItem.DisplayName);
			if (drop.MinDrop != drop.MaxDrop)
			{
				text = text + " (" + I18n.Generic_Range(drop.MinDrop, drop.MaxDrop) + ")";
			}
			else if (drop.MinDrop > 1)
			{
				text += $" ({drop.MinDrop})";
			}
			Vector2 textSize2 = spriteBatch.DrawTextBlock(font, text, new Vector2(textIndent, position.Y + height + 5f), wrapWidth, textColor);
			if (subject != null)
			{
				((Rectangle)(ref pixelArea))._002Ector((int)(position.X + innerIndent + iconSize.X + 5f), (int)(position.Y + height + iconSize.Y / 2f), (int)textSize2.X, (int)textSize2.Y);
				base.LinkTextAreas.Add(new LinkTextArea(subject, pixelArea));
			}
			if (isPrevDropGuaranteed)
			{
				DrawHelper.DrawLine(spriteBatch, position.X + innerIndent + iconSize.X + 5f, position.Y + height + iconSize.Y / 2f, new Vector2(textSize2.X, 1f), Color.Gray);
			}
			if (drop.Conditions != null)
			{
				string conditionText = I18n.ConditionsSummary(HumanReadableConditionParser.Format(drop.Conditions));
				height += textSize2.Y + 5f;
				textSize2 = spriteBatch.DrawTextBlock(font, conditionText, new Vector2(textIndent, position.Y + height + 5f), wrapWidth);
				if (isPrevDropGuaranteed)
				{
					DrawHelper.DrawLine(spriteBatch, position.X + iconSize.X + 5f, position.Y + height + iconSize.Y / 2f, new Vector2(textSize2.X, 1f), disabled ? Color.Gray : Color.Black);
				}
			}
			height += textSize2.Y + 5f;
			if (drop.IsUnlocked && isGuaranteed)
			{
				isPrevDropGuaranteed = true;
			}
		}
		return new Vector2(wrapWidth, height);
	}

	private IEnumerable<FishPondDrop> GetEntries(int currentPopulation, FishPondData data, Object? fish, GameHelper gameHelper)
	{
		foreach (FishPondDropData rawDrop in gameHelper.GetFishPondDrops(data))
		{
			FishPondDropData drop = rawDrop;
			if (fish != null && drop.Conditions != null)
			{
				string conditions = drop.Conditions;
				if (!this.FilterConditions(fish, ref conditions))
				{
					continue;
				}
				if (conditions != drop.Conditions)
				{
					drop = new FishPondDropData(drop.MinPopulation, drop.Precedence, drop.SampleItem, drop.MinDrop, drop.MaxDrop, drop.Probability, conditions);
				}
			}
			bool isUnlocked = currentPopulation >= drop.MinPopulation;
			SpriteInfo sprite = gameHelper.GetSprite(drop.SampleItem);
			yield return new FishPondDrop(drop, drop.SampleItem, sprite, isUnlocked);
		}
	}

	private bool FilterConditions(Object fish, ref string? gameStateQuery)
	{
		if (GameStateQuery.IsImmutablyTrue(gameStateQuery))
		{
			gameStateQuery = null;
			return true;
		}
		if (GameStateQuery.IsImmutablyFalse(gameStateQuery))
		{
			return false;
		}
		List<string> conditions = GameStateQuery.SplitRaw(gameStateQuery).ToList();
		int prevCount = conditions.Count;
		for (int i = conditions.Count - 1; i >= 0; i--)
		{
			ParsedGameStateQuery[] parsed = GameStateQuery.Parse(conditions[i]);
			if (parsed.Length == 1)
			{
				switch (parsed[0].Query[0].ToUpperInvariant())
				{
				case "ITEM_CATEGORY":
				case "ITEM_HAS_EXPLICIT_OBJECT_CATEGORY":
				case "ITEM_ID":
				case "ITEM_ID_PREFIX":
				case "ITEM_NUMERIC_ID":
				case "ITEM_OBJECT_TYPE":
				case "ITEM_TYPE":
					if (!GameStateQuery.CheckConditions(conditions[i], (GameLocation)null, (Farmer)null, (Item)null, (Item)(object)fish, (Random)null, (HashSet<string>)null))
					{
						return false;
					}
					conditions.RemoveAt(i);
					break;
				}
			}
		}
		if (conditions.Count == 0)
		{
			gameStateQuery = null;
		}
		else if (conditions.Count != prevCount)
		{
			gameStateQuery = string.Join(", ", conditions);
		}
		return true;
	}
}
