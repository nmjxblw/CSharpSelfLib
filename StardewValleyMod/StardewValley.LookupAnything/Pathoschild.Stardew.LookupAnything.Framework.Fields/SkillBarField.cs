using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Pathoschild.Stardew.LookupAnything.Framework.Fields;

internal class SkillBarField : PercentageBarField
{
	private readonly int[] SkillPointsPerLevel;

	public SkillBarField(string label, int experience, int maxSkillPoints, int[] skillPointsPerLevel)
		: base(label, experience, maxSkillPoints, Color.Green, Color.Gray, null)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		this.SkillPointsPerLevel = skillPointsPerLevel;
	}

	public override Vector2? DrawValue(SpriteBatch spriteBatch, SpriteFont font, Vector2 position, float wrapWidth)
	{
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		int[] pointsPerLevel = this.SkillPointsPerLevel;
		int nextLevelExp = pointsPerLevel.FirstOrDefault((int p) => p - base.CurrentValue > 0);
		int pointsForNextLevel = ((nextLevelExp > 0) ? (nextLevelExp - base.CurrentValue) : 0);
		int currentLevel = ((nextLevelExp > 0) ? Array.IndexOf(pointsPerLevel, nextLevelExp) : pointsPerLevel.Length);
		string text = ((pointsForNextLevel > 0) ? I18n.Player_Skill_Progress(currentLevel, pointsForNextLevel) : I18n.Player_Skill_ProgressLast(currentLevel));
		float leftOffset = 0f;
		int barHeight = 0;
		for (int level = 0; level < pointsPerLevel.Length; level++)
		{
			float progress;
			if (level < currentLevel)
			{
				progress = 1f;
			}
			else if (level > currentLevel)
			{
				progress = 0f;
			}
			else
			{
				int levelExp = pointsPerLevel[level];
				progress = Math.Min(1f, (float)base.CurrentValue / ((float)levelExp * 1f));
			}
			Vector2 barSize = base.DrawBar(spriteBatch, position + new Vector2(leftOffset, 0f), progress, base.FilledColor, base.EmptyColor, 25f);
			barHeight = (int)barSize.Y;
			leftOffset += barSize.X + 2f;
		}
		Vector2 textSize = spriteBatch.DrawTextBlock(font, text, position + new Vector2(leftOffset, 0f), wrapWidth - leftOffset);
		return new Vector2(leftOffset + textSize.X, Math.Max(barHeight, textSize.Y));
	}
}
