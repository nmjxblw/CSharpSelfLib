using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Netcode;
using Pathoschild.Stardew.Common;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Monsters;
using StardewValley.Network;

namespace Pathoschild.Stardew.LookupAnything.Framework.Lookups.Characters;

internal class CharacterLookupProvider : BaseLookupProvider
{
	private readonly Func<ModConfig> Config;

	private readonly ISubjectRegistry Codex;

	public CharacterLookupProvider(IReflectionHelper reflection, GameHelper gameHelper, Func<ModConfig> config, ISubjectRegistry codex)
		: base(reflection, gameHelper)
	{
		this.Config = config;
		this.Codex = codex;
	}

	public unsafe override IEnumerable<ITarget> GetTargets(GameLocation location, Vector2 lookupTile)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		IslandFarmCave islandFarmCave = (IslandFarmCave)(object)((location is IslandFarmCave) ? location : null);
		if (islandFarmCave != null && islandFarmCave.gourmand != null)
		{
			NPC gourmand = islandFarmCave.gourmand;
			yield return new CharacterTarget(base.GameHelper, this.GetSubjectType(gourmand), gourmand, ((Character)gourmand).Tile, () => this.BuildSubject(gourmand));
		}
		IEnumerable<NPC> enumerable = Game1.CurrentEvent?.actors;
		foreach (NPC npc in (IEnumerable<NPC>)(((object)enumerable) ?? ((object)location.characters)))
		{
			Vector2 entityTile = ((Character)npc).Tile;
			if (base.GameHelper.CouldSpriteOccludeTile(entityTile, lookupTile))
			{
				yield return new CharacterTarget(base.GameHelper, this.GetSubjectType(npc), npc, entityTile, () => this.BuildSubject(npc));
			}
		}
		Enumerator<long, FarmAnimal, NetRef<FarmAnimal>, SerializableDictionary<long, FarmAnimal>, NetLongDictionary<FarmAnimal, NetRef<FarmAnimal>>> enumerator2 = ((NetDictionary<long, FarmAnimal, NetRef<FarmAnimal>, SerializableDictionary<long, FarmAnimal>, NetLongDictionary<FarmAnimal, NetRef<FarmAnimal>>>)(object)location.Animals).Values.GetEnumerator();
		try
		{
			while (enumerator2.MoveNext())
			{
				FarmAnimal animal = enumerator2.Current;
				Vector2 entityTile2 = ((Character)animal).Tile;
				if (base.GameHelper.CouldSpriteOccludeTile(entityTile2, lookupTile))
				{
					yield return new FarmAnimalTarget(base.GameHelper, animal, entityTile2, () => this.BuildSubject(animal));
				}
			}
		}
		finally
		{
			((IDisposable)enumerator2/*cast due to .constrained prefix*/).Dispose();
		}
		Enumerator val = location.farmers.GetEnumerator();
		try
		{
			while (((Enumerator)(ref val)).MoveNext())
			{
				Farmer farmer = ((Enumerator)(ref val)).Current;
				Vector2 entityTile3 = ((Character)farmer).Tile;
				if (base.GameHelper.CouldSpriteOccludeTile(entityTile3, lookupTile))
				{
					yield return new FarmerTarget(base.GameHelper, farmer, () => this.BuildSubject(farmer));
				}
			}
		}
		finally
		{
			((IDisposable)(*(Enumerator*)(&val))/*cast due to .constrained prefix*/).Dispose();
		}
		val = default(Enumerator);
	}

	public override ISubject? GetSubject(IClickableMenu menu, int cursorX, int cursorY)
	{
		IClickableMenu targetMenu = base.GameHelper.GetGameMenuPage(menu) ?? menu;
		IClickableMenu val = targetMenu;
		if (!(val is SkillsPage))
		{
			ProfileMenu profileMenu = (ProfileMenu)(object)((val is ProfileMenu) ? val : null);
			if (profileMenu == null)
			{
				SocialPage socialPage = (SocialPage)(object)((val is SocialPage) ? val : null);
				if (socialPage == null)
				{
					Billboard billboard = (Billboard)(object)((val is Billboard) ? val : null);
					if (billboard != null)
					{
						List<ClickableTextureComponent> calendarDays = billboard.calendarDays;
						if (calendarDays == null)
						{
							goto IL_029c;
						}
						int selectedDay = -1;
						for (int i = 0; i < billboard.calendarDays.Count; i++)
						{
							if (((ClickableComponent)billboard.calendarDays[i]).containsPoint(cursorX, cursorY))
							{
								selectedDay = i + 1;
								break;
							}
						}
						if (selectedDay == -1)
						{
							return null;
						}
						NPC target = (from p in base.GameHelper.GetAllCharacters()
							where p.Birthday_Season == Game1.currentSeason && p.Birthday_Day == selectedDay
							select p).MaxBy((NPC p) => p.CanSocialize);
						if (target != null)
						{
							return this.BuildSubject(target);
						}
					}
					else if (!(val is TitleMenu))
					{
						if (val != null)
						{
							goto IL_029c;
						}
					}
					else
					{
						IClickableMenu subMenu = TitleMenu.subMenu;
						LoadGameMenu loadMenu = (LoadGameMenu)(object)((subMenu is LoadGameMenu) ? subMenu : null);
						if (loadMenu == null)
						{
							goto IL_029c;
						}
						ClickableComponent button = ((IEnumerable<ClickableComponent>)loadMenu.slotButtons).FirstOrDefault((Func<ClickableComponent, bool>)((ClickableComponent p) => p.containsPoint(cursorX, cursorY)));
						if (button != null)
						{
							int index = loadMenu.currentItemIndex + int.Parse(button.name);
							MenuSlot obj = loadMenu.MenuSlots[index];
							SaveFileSlot slot = (SaveFileSlot)(object)((obj is SaveFileSlot) ? obj : null);
							if (slot?.Farmer != null)
							{
								return new FarmerSubject(base.GameHelper, slot.Farmer, isLoadMenu: true);
							}
						}
					}
				}
				else
				{
					foreach (ClickableTextureComponent slot2 in socialPage.characterSlots)
					{
						if (((ClickableComponent)slot2).containsPoint(cursorX, cursorY))
						{
							SocialEntry entry = socialPage.SocialEntries[((ClickableComponent)slot2).myID];
							Character character = entry.Character;
							Farmer player = (Farmer)(object)((character is Farmer) ? character : null);
							if (player != null)
							{
								return this.BuildSubject(player);
							}
							NPC npc = (NPC)(object)((character is NPC) ? character : null);
							if (npc != null)
							{
								return this.BuildSubject(npc);
							}
						}
					}
				}
			}
			else if (profileMenu.hoveredItem == null)
			{
				Character character2 = profileMenu.Current.Character;
				if (character2 != null)
				{
					return this.Codex.GetByEntity(character2, character2.currentLocation);
				}
			}
			goto IL_03e4;
		}
		return this.BuildSubject(Game1.player);
		IL_03e4:
		return null;
		IL_029c:
		if (((object)targetMenu).GetType().FullName == "AnimalSocialMenu.Framework.AnimalSocialPage")
		{
			int slotOffset = base.Reflection.GetField<int>((object)targetMenu, "SlotPosition", true).GetValue();
			List<ClickableTextureComponent> slots = base.Reflection.GetField<List<ClickableTextureComponent>>((object)targetMenu, "Sprites", true).GetValue();
			List<object> animalIds = base.Reflection.GetField<List<object>>((object)targetMenu, "Names", true).GetValue();
			for (int i2 = slotOffset; i2 < slots.Count; i2++)
			{
				if (((ClickableComponent)slots[i2]).containsPoint(cursorX, cursorY))
				{
					if (!animalIds.TryGetIndex(i2, out object rawId) || !(rawId is long))
					{
						break;
					}
					long id = (long)rawId;
					FarmAnimal animal = ((IEnumerable<FarmAnimal>)((GameLocation)Game1.getFarm()).getAllFarmAnimals()).FirstOrDefault((Func<FarmAnimal, bool>)((FarmAnimal p) => ((NetFieldBase<long, NetLong>)(object)p.myID).Value == id));
					if (animal == null)
					{
						break;
					}
					return this.BuildSubject(animal);
				}
			}
		}
		else
		{
			NPC npc2 = base.Reflection.GetField<NPC>((object)targetMenu, "hoveredNpc", false)?.GetValue() ?? base.Reflection.GetField<NPC>((object)targetMenu, "HoveredNpc", false)?.GetValue();
			if (npc2 != null)
			{
				return this.BuildSubject(npc2);
			}
		}
		goto IL_03e4;
	}

	public override ISubject? GetSubjectFor(object entity, GameLocation? location)
	{
		FarmAnimal animal = (FarmAnimal)((entity is FarmAnimal) ? entity : null);
		if (animal == null)
		{
			Farmer player = (Farmer)((entity is Farmer) ? entity : null);
			if (player == null)
			{
				NPC npc = (NPC)((entity is NPC) ? entity : null);
				if (npc != null)
				{
					return this.BuildSubject(npc);
				}
				return null;
			}
			return this.BuildSubject(player);
		}
		return this.BuildSubject(animal);
	}

	public override IEnumerable<ISubject> GetSearchSubjects()
	{
		HashSet<string> seen = new HashSet<string>();
		foreach (ISubject subject in GetAll())
		{
			if (seen.Add($"{subject.GetType().FullName}::{subject.Type}::{subject.Name}"))
			{
				yield return subject;
			}
		}
		IEnumerable<ISubject> GetAll()
		{
			foreach (NPC npc in Utility.getAllCharacters())
			{
				yield return this.BuildSubject(npc);
			}
			foreach (GameLocation location in CommonHelper.GetLocations())
			{
				Enumerator<long, FarmAnimal, NetRef<FarmAnimal>, SerializableDictionary<long, FarmAnimal>, NetLongDictionary<FarmAnimal, NetRef<FarmAnimal>>> enumerator4 = ((NetDictionary<long, FarmAnimal, NetRef<FarmAnimal>, SerializableDictionary<long, FarmAnimal>, NetLongDictionary<FarmAnimal, NetRef<FarmAnimal>>>)(object)location.Animals).Values.GetEnumerator();
				try
				{
					while (enumerator4.MoveNext())
					{
						FarmAnimal animal = enumerator4.Current;
						yield return this.BuildSubject(animal);
					}
				}
				finally
				{
					((IDisposable)enumerator4/*cast due to .constrained prefix*/).Dispose();
				}
			}
			foreach (Farmer player in Game1.getAllFarmers())
			{
				yield return this.BuildSubject(player);
			}
		}
	}

	private ISubject BuildSubject(Farmer player)
	{
		return new FarmerSubject(base.GameHelper, player);
	}

	private ISubject BuildSubject(FarmAnimal animal)
	{
		return new FarmAnimalSubject(this.Codex, base.GameHelper, animal);
	}

	private ISubject BuildSubject(NPC npc)
	{
		ModConfig config = this.Config();
		return new CharacterSubject(this.Codex, base.GameHelper, npc, this.GetSubjectType(npc), base.GameHelper.Metadata, config.ShowUnknownGiftTastes, config.HighlightUnrevealedGiftTastes, config.ShowGiftTastes, config.CollapseLargeFields, config.EnableTargetRedirection, config.ShowUnownedGifts);
	}

	private SubjectType GetSubjectType(NPC npc)
	{
		if (!(npc is Horse))
		{
			if (!(npc is Junimo))
			{
				if (!(npc is Pet))
				{
					if (npc is Monster)
					{
						return SubjectType.Monster;
					}
					return SubjectType.Villager;
				}
				return SubjectType.Pet;
			}
			return SubjectType.Junimo;
		}
		return SubjectType.Horse;
	}
}
