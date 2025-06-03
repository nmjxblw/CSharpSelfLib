using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.Common.Integrations.SpaceCore;
using Pathoschild.Stardew.LookupAnything.Framework.DebugFields;
using Pathoschild.Stardew.LookupAnything.Framework.Fields;
using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData;
using StardewValley.Objects;

namespace Pathoschild.Stardew.LookupAnything.Framework.Lookups.Characters;

internal class FarmerSubject : BaseSubject
{
	private readonly Farmer Target;

	private readonly bool IsLoadMenu;

	private readonly Lazy<XElement?>? RawSaveData;

	public FarmerSubject(GameHelper gameHelper, Farmer farmer, bool isLoadMenu = false)
		: base(gameHelper, ((Character)farmer).Name, null, I18n.Type_Player())
	{
		FarmerSubject farmerSubject = this;
		this.Target = farmer;
		this.IsLoadMenu = isLoadMenu;
		this.RawSaveData = (isLoadMenu ? new Lazy<XElement>(() => farmerSubject.ReadSaveFile(farmer.slotName)) : null);
	}

	public override IEnumerable<ICustomField> GetData()
	{
		Farmer target = this.Target;
		yield return new GenericField(I18n.Player_Gender(), target.IsMale ? I18n.Player_Gender_Male() : I18n.Player_Gender_Female());
		yield return new GenericField(I18n.Player_FarmName(), ((NetFieldBase<string, NetString>)(object)target.farmName).Value);
		yield return new GenericField(I18n.Player_FarmMap(), this.GetFarmType());
		yield return new GenericField(I18n.Player_FavoriteThing(), ((NetFieldBase<string, NetString>)(object)target.favoriteThing).Value);
		yield return new GenericField((Game1.player.spouse == "Krobus") ? I18n.Player_Housemate() : I18n.Player_Spouse(), this.GetSpouseName());
		if (Utility.doesMasterPlayerHaveMailReceivedButNotMailForTomorrow("ccMovieTheater"))
		{
			yield return new GenericField(I18n.Player_WatchedMovieThisWeek(), base.Stringify(((NetFieldBase<int, NetInt>)(object)target.lastSeenMovieWeek).Value >= Game1.Date.TotalSundayWeeks));
		}
		int maxSkillPoints = base.Constants.PlayerMaxSkillPoints;
		int[] skillPointsPerLevel = base.Constants.PlayerSkillPointsPerLevel;
		yield return new SkillBarField(I18n.Player_FarmingSkill(), target.experiencePoints[0], maxSkillPoints, skillPointsPerLevel);
		yield return new SkillBarField(I18n.Player_MiningSkill(), target.experiencePoints[3], maxSkillPoints, skillPointsPerLevel);
		yield return new SkillBarField(I18n.Player_ForagingSkill(), target.experiencePoints[2], maxSkillPoints, skillPointsPerLevel);
		yield return new SkillBarField(I18n.Player_FishingSkill(), target.experiencePoints[1], maxSkillPoints, skillPointsPerLevel);
		yield return new SkillBarField(I18n.Player_CombatSkill(), target.experiencePoints[4], maxSkillPoints, skillPointsPerLevel);
		SpaceCoreIntegration spaceCore = base.GameHelper.SpaceCore;
		if (spaceCore.IsLoaded)
		{
			string[] customSkills = spaceCore.ModApi.GetCustomSkills();
			foreach (string skill in customSkills)
			{
				yield return new SkillBarField(spaceCore.ModApi.GetDisplayNameOfCustomSkill(skill), spaceCore.ModApi.GetExperienceForCustomSkill(target, skill), maxSkillPoints, skillPointsPerLevel);
			}
		}
		string luckSummary = I18n.Player_Luck_Summary(((Game1.player.DailyLuck >= 0.0) ? "+" : "") + Math.Round(Game1.player.DailyLuck * 100.0, 2));
		yield return new GenericField(I18n.Player_Luck(), $"{this.GetSpiritLuckMessage()}{Environment.NewLine}({luckSummary})");
		if (this.IsLoadMenu)
		{
			yield return new GenericField(I18n.Player_SaveFormat(), this.GetSaveFormat(this.RawSaveData?.Value));
		}
	}

	public override IEnumerable<IDebugField> GetDebugFields()
	{
		Farmer target = this.Target;
		yield return new GenericDebugField("immunity", target.Immunity, null, pinned: true);
		yield return new GenericDebugField("defense", target.buffs.Defense, null, pinned: true);
		yield return new GenericDebugField("magnetic radius", target.MagneticRadius, null, pinned: true);
		foreach (IDebugField item in base.GetDebugFieldsFrom(target))
		{
			yield return item;
		}
	}

	public override bool DrawPortrait(SpriteBatch spriteBatch, Vector2 position, Vector2 size)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		Farmer target = this.Target;
		if (this.IsLoadMenu)
		{
			target.FarmerRenderer.draw(spriteBatch, new AnimationFrame(0, 0, false, false, (endOfAnimationBehavior)null, false), 0, new Rectangle(0, 0, 16, 32), position, Vector2.Zero, 0.8f, 2, Color.White, 0f, 1f, target);
		}
		else
		{
			FarmerSprite sprite = target.FarmerSprite;
			target.FarmerRenderer.draw(spriteBatch, sprite.CurrentAnimationFrame, ((AnimatedSprite)sprite).CurrentFrame, ((AnimatedSprite)sprite).SourceRect, position, Vector2.Zero, 0.8f, Color.White, 0f, 1f, target);
		}
		return true;
	}

	private string? GetSpiritLuckMessage()
	{
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		if (this.IsLoadMenu)
		{
			string rawDailyLuck = this.RawSaveData?.Value?.Element("dailyLuck")?.Value;
			if (rawDailyLuck == null)
			{
				return null;
			}
			((NetFieldBase<double, NetDouble>)(object)Game1.player.team.sharedDailyLuck).Value = double.Parse(rawDailyLuck);
		}
		return new TV().getFortuneForecast(this.Target);
	}

	private string? GetFarmType()
	{
		string farmTypeId = ((!this.IsLoadMenu) ? Game1.GetFarmTypeID() : (this.RawSaveData?.Value?.Element("whichFarm")?.Value ?? Game1.GetFarmTypeID()));
		if (int.TryParse(farmTypeId, out var farmTypeNumber))
		{
			string translationKey = farmTypeNumber switch
			{
				4 => "Character_FarmCombat", 
				0 => "Character_FarmStandard", 
				2 => "Character_FarmForaging", 
				3 => "Character_FarmMining", 
				1 => "Character_FarmFishing", 
				5 => "Character_FarmFourCorners", 
				6 => "Character_FarmBeach", 
				_ => null, 
			};
			if (translationKey != null)
			{
				return GameI18n.GetString("Strings\\UI:" + translationKey).Replace("_", Environment.NewLine);
			}
		}
		foreach (ModFarmType farmData in DataLoader.AdditionalFarms(Game1.content))
		{
			if (farmData?.Id == farmTypeId && farmData.TooltipStringPath != null)
			{
				return GameI18n.GetString(farmData.TooltipStringPath).Replace("_", Environment.NewLine);
			}
		}
		return I18n.Player_FarmMap_Custom();
	}

	private string? GetSpouseName()
	{
		if (this.IsLoadMenu)
		{
			return this.Target.spouse;
		}
		long? spousePlayerID = this.Target.team.GetSpouse(this.Target.UniqueMultiplayerID);
		Farmer spousePlayer = (spousePlayerID.HasValue ? Game1.GetPlayer(spousePlayerID.Value, false) : null);
		object obj = ((spousePlayer != null) ? ((Character)spousePlayer).displayName : null);
		if (obj == null)
		{
			NPC spouse = Game1.player.getSpouse();
			if (spouse == null)
			{
				return null;
			}
			obj = ((Character)spouse).displayName;
		}
		return (string?)obj;
	}

	private XElement? ReadSaveFile(string? slotName)
	{
		if (slotName == null)
		{
			return null;
		}
		FileInfo file = new FileInfo(Path.Combine(Constants.SavesPath, slotName, slotName));
		if (!file.Exists)
		{
			return null;
		}
		string text = File.ReadAllText(file.FullName);
		return XElement.Parse(text);
	}

	private string GetSaveFormat(XElement? saveData)
	{
		if (saveData == null)
		{
			return "???";
		}
		string version = saveData.Element("gameVersion")?.Value;
		if (!string.IsNullOrWhiteSpace(version))
		{
			return version;
		}
		if (saveData.Element("hasApplied1_4_UpdateChanges") != null)
		{
			return "1.4";
		}
		if (saveData.Element("hasApplied1_3_UpdateChanges") != null)
		{
			return "1.3";
		}
		if (saveData.Element("whichFarm") != null)
		{
			return "1.1 - 1.2";
		}
		return "1.0";
	}
}
