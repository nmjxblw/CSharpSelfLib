using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Pathoschild.Stardew.Common.UI;
using Pathoschild.Stardew.LookupAnything.Framework.Fields.Models;
using StardewValley;

namespace Pathoschild.Stardew.LookupAnything.Framework.Fields;

internal class CheckboxListField : GenericField
{
	protected CheckboxList[] CheckboxLists;

	protected readonly float CheckboxSize;

	protected readonly float LineHeight;

	public CheckboxListField(string label, params CheckboxList[] checkboxLists)
		: this(label)
	{
		this.CheckboxLists = checkboxLists;
	}

	public override Vector2? DrawValue(SpriteBatch spriteBatch, SpriteFont font, Vector2 position, float wrapWidth)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		float topOffset = 0f;
		CheckboxList[] checkboxLists = this.CheckboxLists;
		foreach (CheckboxList checkboxList in checkboxLists)
		{
			topOffset += this.DrawCheckboxList(checkboxList, spriteBatch, font, new Vector2(position.X, position.Y + topOffset), wrapWidth).Y;
		}
		return new Vector2(wrapWidth, topOffset - this.LineHeight);
	}

	protected CheckboxListField(string label)
		: base(label, hasValue: true)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		this.CheckboxLists = Array.Empty<CheckboxList>();
		this.CheckboxSize = CommonSprites.Icons.FilledCheckbox.Width * 2;
		this.LineHeight = Math.Max(this.CheckboxSize, Game1.smallFont.MeasureString("ABC").Y);
	}

	protected Vector2 DrawCheckboxList(CheckboxList checkboxList, SpriteBatch spriteBatch, SpriteFont font, Vector2 position, float wrapWidth)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		float topOffset = 0f;
		float checkboxSize = this.CheckboxSize;
		float leftOffset = 0f;
		float checkboxOffsetY = (this.LineHeight - checkboxSize) / 2f;
		if (checkboxList.Intro != null)
		{
			topOffset += base.DrawIconText(spriteBatch, font, new Vector2(position.X, position.Y + topOffset), wrapWidth, checkboxList.Intro.Text, Color.Black, checkboxList.Intro.Icon, (Vector2?)new Vector2(this.LineHeight), (Color?)null, (int?)null, probe: false).Y;
			leftOffset = 14f;
		}
		Checkbox[] checkboxes = checkboxList.Checkboxes;
		foreach (Checkbox checkbox in checkboxes)
		{
			spriteBatch.Draw(CommonSprites.Icons.Sheet, new Vector2(position.X + leftOffset, position.Y + topOffset + checkboxOffsetY), (Rectangle?)(checkbox.IsChecked ? CommonSprites.Icons.FilledCheckbox : CommonSprites.Icons.EmptyCheckbox), Color.White, 0f, Vector2.Zero, checkboxSize / (float)CommonSprites.Icons.FilledCheckbox.Width, (SpriteEffects)0, 1f);
			Vector2 textSize = spriteBatch.DrawTextBlock(Game1.smallFont, checkbox.Text, new Vector2(position.X + leftOffset + checkboxSize + 7f, position.Y + topOffset), wrapWidth - checkboxSize - 7f);
			topOffset += Math.Max(checkboxSize, textSize.Y);
		}
		return new Vector2(position.X, topOffset + this.LineHeight);
	}
}
