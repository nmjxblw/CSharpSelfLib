using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Netcode;
using StardewValley.Buildings;
using StardewValley.Events;
using StardewValley.GameData.Buildings;
using StardewValley.GameData.FarmAnimals;
using StardewValley.GameData.Machines;
using StardewValley.TokenizableStrings;
using xTile.Dimensions;

namespace StardewValley;

public class AnimalHouse : GameLocation
{
	[XmlElement("animalLimit")]
	public readonly NetInt animalLimit = new NetInt(4);

	public readonly NetLongList animalsThatLiveHere = new NetLongList();

	[XmlIgnore]
	public bool hasShownIncubatorBuildingFullMessage;

	public AnimalHouse()
	{
	}

	public AnimalHouse(string mapPath, string name)
		: base(mapPath, name)
	{
	}

	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(this.animalLimit, "animalLimit").AddField(this.animalsThatLiveHere, "animalsThatLiveHere");
	}

	/// <inheritdoc />
	public override void OnParentBuildingUpgraded(Building building)
	{
		base.OnParentBuildingUpgraded(building);
		BuildingData buildingData = building.GetData();
		if (buildingData != null)
		{
			this.animalLimit.Value = buildingData.MaxOccupants;
		}
		this.resetPositionsOfAllAnimals();
		base.loadLights();
	}

	public bool isFull()
	{
		return this.animalsThatLiveHere.Count >= this.animalLimit.Value;
	}

	public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
	{
		if (who.ActiveObject?.QualifiedItemId == "(O)178" && this.doesTileHaveProperty(tileLocation.X, tileLocation.Y, "Trough", "Back") != null && !base.objects.ContainsKey(new Vector2(tileLocation.X, tileLocation.Y)))
		{
			base.objects.Add(new Vector2(tileLocation.X, tileLocation.Y), (Object)who.ActiveObject.getOne());
			who.reduceActiveItemByOne();
			who.currentLocation.playSound("coin");
			Game1.haltAfterCheck = false;
			return true;
		}
		return base.checkAction(tileLocation, viewport, who);
	}

	protected override void resetSharedState()
	{
		this.resetPositionsOfAllAnimals();
		foreach (Object o in base.objects.Values)
		{
			if (!o.bigCraftable.Value)
			{
				continue;
			}
			MachineData machineData = o.GetMachineData();
			if (machineData == null || !machineData.IsIncubator || o.heldObject.Value == null || o.MinutesUntilReady > 0)
			{
				continue;
			}
			if (!this.isFull())
			{
				string whatHatched = "??";
				FarmAnimalData hatchedAnimal = FarmAnimal.GetAnimalDataFromEgg(o.heldObject.Value, this);
				if (hatchedAnimal != null && hatchedAnimal.BirthText != null)
				{
					whatHatched = TokenParser.ParseText(hatchedAnimal.BirthText);
				}
				base.currentEvent = new Event("none/-1000 -1000/farmer 2 9 0/pause 250/message \"" + whatHatched + "\"/pause 500/animalNaming/pause 500/end");
				break;
			}
			if (!this.hasShownIncubatorBuildingFullMessage)
			{
				this.hasShownIncubatorBuildingFullMessage = true;
				Game1.showGlobalMessage(Game1.content.LoadString("Strings\\Locations:AnimalHouse_Incubator_HouseFull"));
			}
		}
		base.resetSharedState();
	}

	/// <summary>Hatch an incubated animal egg that's ready to hatch, if there are any.</summary>
	/// <param name="name">The name of the animal to set.</param>
	public void addNewHatchedAnimal(string name)
	{
		bool foundIncubator = false;
		foreach (Object o in base.objects.Values)
		{
			if (o.bigCraftable.Value)
			{
				MachineData machineData = o.GetMachineData();
				if (machineData != null && machineData.IsIncubator && o.heldObject.Value != null && o.MinutesUntilReady <= 0 && !this.isFull())
				{
					foundIncubator = true;
					string hatchedAnimalId;
					FarmAnimalData data;
					FarmAnimal a = new FarmAnimal(FarmAnimal.TryGetAnimalDataFromEgg(o.heldObject.Value, this, out hatchedAnimalId, out data) ? hatchedAnimalId : "White Chicken", Game1.multiplayer.getNewID(), Game1.player.UniqueMultiplayerID);
					a.Name = name;
					a.displayName = name;
					o.heldObject.Value = null;
					this.adoptAnimal(a);
					break;
				}
			}
		}
		if (!foundIncubator && Game1.farmEvent is QuestionEvent questionEvent)
		{
			FarmAnimal a2 = new FarmAnimal(questionEvent.animal.type.Value, Game1.multiplayer.getNewID(), Game1.player.UniqueMultiplayerID);
			a2.Name = name;
			a2.displayName = name;
			a2.parentId.Value = questionEvent.animal.myID.Value;
			this.adoptAnimal(a2);
			questionEvent.forceProceed = true;
		}
		Game1.exitActiveMenu();
	}

	/// <summary>Add an animal to this location and set the location as the animal's home.</summary>
	/// <param name="animal">The animal to adopt.</param>
	public void adoptAnimal(FarmAnimal animal)
	{
		base.animals.Add(animal.myID.Value, animal);
		animal.currentLocation = this;
		this.animalsThatLiveHere.Add(animal.myID.Value);
		animal.homeInterior = this;
		animal.setRandomPosition(this);
		foreach (Farmer allFarmer in Game1.getAllFarmers())
		{
			string displayType = animal.displayType;
			if (displayType == "White Chicken" || displayType == "Brown Chicken")
			{
				displayType = "Chicken";
			}
			allFarmer.autoGenerateActiveDialogueEvent("purchasedAnimal_" + displayType);
		}
	}

	public void resetPositionsOfAllAnimals()
	{
		foreach (KeyValuePair<long, FarmAnimal> pair in base.animals.Pairs)
		{
			pair.Value.setRandomPosition(this);
		}
	}

	/// <inheritdoc />
	public override bool dropObject(Object obj, Vector2 location, xTile.Dimensions.Rectangle viewport, bool initialPlacement, Farmer who = null)
	{
		Vector2 tileLocation = new Vector2((int)(location.X / 64f), (int)(location.Y / 64f));
		if (obj.QualifiedItemId == "(O)178" && this.doesTileHaveProperty((int)tileLocation.X, (int)tileLocation.Y, "Trough", "Back") != null)
		{
			return base.objects.TryAdd(tileLocation, obj);
		}
		return base.dropObject(obj, location, viewport, initialPlacement);
	}

	public override void TransferDataFromSavedLocation(GameLocation l)
	{
		this.animalLimit.Value = ((AnimalHouse)l).animalLimit.Value;
		base.TransferDataFromSavedLocation(l);
	}

	public override void DayUpdate(int dayOfMonth)
	{
		base.DayUpdate(dayOfMonth);
		if (base.HasMapPropertyWithValue("AutoFeed"))
		{
			this.feedAllAnimals();
		}
	}

	public void feedAllAnimals()
	{
		GameLocation rootLocation = base.GetRootLocation();
		int fed = 0;
		for (int x = 0; x < base.map.Layers[0].LayerWidth; x++)
		{
			for (int y = 0; y < base.map.Layers[0].LayerHeight; y++)
			{
				if (this.doesTileHaveProperty(x, y, "Trough", "Back") == null)
				{
					continue;
				}
				Vector2 tileLocation = new Vector2(x, y);
				if (!base.objects.ContainsKey(tileLocation))
				{
					Object hay = GameLocation.GetHayFromAnySilo(rootLocation);
					if (hay == null)
					{
						return;
					}
					base.objects.Add(tileLocation, hay);
					fed++;
				}
				if (fed >= this.animalLimit.Value)
				{
					return;
				}
			}
		}
	}
}
