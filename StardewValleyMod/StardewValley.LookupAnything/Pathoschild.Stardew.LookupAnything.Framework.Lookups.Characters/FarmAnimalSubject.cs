using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using Pathoschild.Stardew.Common;
using Pathoschild.Stardew.LookupAnything.Framework.Constants;
using Pathoschild.Stardew.LookupAnything.Framework.DebugFields;
using Pathoschild.Stardew.LookupAnything.Framework.Fields;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;

namespace Pathoschild.Stardew.LookupAnything.Framework.Lookups.Characters;

internal class FarmAnimalSubject : BaseSubject
{
	private readonly FarmAnimal Target;

	private readonly ISubjectRegistry Codex;

	public FarmAnimalSubject(ISubjectRegistry codex, GameHelper gameHelper, FarmAnimal animal)
		: base(gameHelper, ((Character)animal).displayName, null, animal.displayType)
	{
		this.Codex = codex;
		this.Target = animal;
	}

	public override IEnumerable<ICustomField> GetData()
	{
		FarmAnimal animal = this.Target;
		bool isFullyGrown = animal.isAdult();
		int daysUntilGrown = 0;
		SDate dayOfMaturity = null;
		if (!isFullyGrown)
		{
			daysUntilGrown = animal.GetAnimalData().DaysToMature - ((NetFieldBase<int, NetInt>)(object)animal.age).Value;
			dayOfMaturity = SDate.Now().AddDays(daysUntilGrown);
		}
		IModInfo fromMod = base.GameHelper.TryGetModFromStringId(((NetFieldBase<string, NetString>)(object)animal.type).Value);
		if (fromMod != null)
		{
			yield return new GenericField(I18n.AddedByMod(), I18n.AddedByMod_Summary(fromMod.Manifest.Name));
		}
		yield return new CharacterFriendshipField(I18n.Animal_Love(), base.GameHelper.GetFriendshipForAnimal(Game1.player, animal));
		yield return new PercentageBarField(I18n.Animal_Happiness(), ((NetFieldBase<int, NetInt>)(object)animal.happiness).Value, 255, Color.Green, Color.Gray, I18n.Generic_Percent((int)Math.Round((float)((NetFieldBase<int, NetInt>)(object)animal.happiness).Value / ((float)base.Constants.AnimalMaxHappiness * 1f) * 100f)));
		yield return new GenericField(I18n.Animal_Mood(), animal.getMoodMessage());
		yield return new GenericField(I18n.Animal_Complaints(), this.GetMoodReason(animal));
		yield return new ItemIconField(base.GameHelper, I18n.Animal_ProduceReady(), CommonHelper.IsItemId(((NetFieldBase<string, NetString>)(object)animal.currentProduce).Value, allowZero: false) ? ItemRegistry.Create(((NetFieldBase<string, NetString>)(object)animal.currentProduce).Value, 1, 0, false) : null, this.Codex);
		if (!isFullyGrown)
		{
			yield return new GenericField(I18n.Animal_Growth(), I18n.Generic_Days(daysUntilGrown) + " (" + base.Stringify(dayOfMaturity) + ")");
		}
		yield return new GenericField(I18n.Animal_SellsFor(), GenericField.GetSaleValueString(animal.getSellPrice(), 1));
		yield return new GenericField(I18n.InternalId(), ((NetFieldBase<string, NetString>)(object)animal.type).Value);
	}

	public override IEnumerable<IDebugField> GetDebugFields()
	{
		FarmAnimal target = this.Target;
		yield return new GenericDebugField("age", $"{target.age} days", null, pinned: true);
		yield return new GenericDebugField("friendship", $"{target.friendshipTowardFarmer} (max {base.Constants.AnimalMaxHappiness})", null, pinned: true);
		yield return new GenericDebugField("fullness", base.Stringify(((NetFieldBase<int, NetInt>)(object)target.fullness).Value), null, pinned: true);
		yield return new GenericDebugField("happiness", base.Stringify(((NetFieldBase<int, NetInt>)(object)target.happiness).Value), null, pinned: true);
		foreach (IDebugField item in base.GetDebugFieldsFrom(target))
		{
			yield return item;
		}
	}

	public override bool DrawPortrait(SpriteBatch spriteBatch, Vector2 position, Vector2 size)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		FarmAnimal animal = this.Target;
		((Character)animal).Sprite.draw(spriteBatch, position, 1f, 0, 0, Color.White, false, size.X / (float)((Character)animal).Sprite.getWidth(), 0f, false);
		return true;
	}

	private string GetMoodReason(FarmAnimal animal)
	{
		List<string> factors = new List<string>();
		if (Game1.IsWinter && Game1.currentLocation.numberOfObjectsWithName(Constant.ItemNames.Heater) <= 0)
		{
			factors.Add(I18n.Animal_Complaints_NoHeater());
		}
		switch (((NetFieldBase<int, NetInt>)(object)animal.moodMessage).Value)
		{
		case 0:
			factors.Add(I18n.Animal_Complaints_NewHome());
			break;
		case 4:
			factors.Add(I18n.Animal_Complaints_Hungry());
			break;
		case 5:
			factors.Add(I18n.Animal_Complaints_WildAnimalAttack());
			break;
		case 6:
			factors.Add(I18n.Animal_Complaints_LeftOut());
			break;
		}
		if (!((NetFieldBase<bool, NetBool>)(object)animal.wasPet).Value)
		{
			factors.Add(I18n.Animal_Complaints_NotPetted());
		}
		return I18n.List(factors);
	}
}
