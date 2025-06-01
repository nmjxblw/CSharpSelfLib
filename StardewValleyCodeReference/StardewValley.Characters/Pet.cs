using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.BellsAndWhistles;
using StardewValley.Buildings;
using StardewValley.Extensions;
using StardewValley.GameData.Pets;
using StardewValley.Internal;
using StardewValley.Locations;
using StardewValley.Network;
using StardewValley.Objects;

namespace StardewValley.Characters;

public class Pet : NPC
{
	/// <summary>The cat's pet type ID in <c>Data/Pets</c>.</summary>
	public const string type_cat = "Cat";

	/// <summary>The dog's pet type ID in <c>Data/Pets</c>.</summary>
	public const string type_dog = "Dog";

	/// <summary>A unique ID for this pet.</summary>
	/// <remarks>This matches the <see cref="F:StardewValley.Buildings.PetBowl.petId" /> of the pet's bowl, if any. See also <see cref="M:StardewValley.Characters.Pet.GetPetBowl" />.</remarks>
	[XmlElement("guid")]
	public NetGuid petId = new NetGuid(Guid.NewGuid());

	public const int bedTime = 2000;

	public const int maxFriendship = 1000;

	public const string behavior_Walk = "Walk";

	public const string behavior_Sleep = "Sleep";

	public const string behavior_SitDown = "SitDown";

	public const string behavior_Sprint = "Sprint";

	protected int behaviorTimer = -1;

	protected int animationLoopsLeft;

	[XmlElement("petType")]
	public readonly NetString petType = new NetString("Dog");

	[XmlElement("whichBreed")]
	public readonly NetString whichBreed = new NetString("0");

	private readonly NetString netCurrentBehavior = new NetString();

	/// <summary>The unique name of the location containing the pet's bowl, if any.</summary>
	[XmlElement("homeLocationName")]
	public readonly NetString homeLocationName = new NetString();

	[XmlIgnore]
	public readonly NetEvent1Field<long, NetLong> petPushEvent = new NetEvent1Field<long, NetLong>();

	[XmlIgnore]
	protected string _currentBehavior;

	[XmlElement("lastPetDay")]
	public NetLongDictionary<int, NetInt> lastPetDay = new NetLongDictionary<int, NetInt>();

	[XmlElement("grantedFriendshipForPet")]
	public NetBool grantedFriendshipForPet = new NetBool(value: false);

	[XmlElement("friendshipTowardFarmer")]
	public NetInt friendshipTowardFarmer = new NetInt(0);

	[XmlElement("timesPet")]
	public NetInt timesPet = new NetInt(0);

	[XmlElement("hat")]
	public readonly NetRef<Hat> hat = new NetRef<Hat>();

	protected int _walkFromPushTimer;

	public NetBool isSleepingOnFarmerBed = new NetBool(value: false);

	[XmlIgnore]
	public readonly NetMutex mutex = new NetMutex();

	private int pushingTimer;

	/// <inheritdoc />
	[XmlIgnore]
	public override bool IsVillager => false;

	public string CurrentBehavior
	{
		get
		{
			return this.netCurrentBehavior.Value;
		}
		set
		{
			if (this.netCurrentBehavior.Value != value)
			{
				this.netCurrentBehavior.Value = value;
			}
		}
	}

	public override void reloadData()
	{
	}

	protected override string translateName()
	{
		return base.name.Value.Trim();
	}

	public Pet(int xTile, int yTile, string petBreed, string petType)
	{
		base.Name = petType;
		this.displayName = base.name.Value;
		this.petType.Value = petType;
		this.whichBreed.Value = petBreed;
		this.Sprite = new AnimatedSprite(this.getPetTextureName(), 0, 32, 32);
		base.Position = new Vector2(xTile, yTile) * 64f;
		base.Breather = false;
		base.willDestroyObjectsUnderfoot = false;
		base.currentLocation = Game1.currentLocation;
		base.HideShadow = true;
	}

	public Pet()
		: this(0, 0, "0", "Dog")
	{
	}

	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(this.petId, "petId").AddField(this.petType, "petType").AddField(this.whichBreed, "whichBreed")
			.AddField(this.netCurrentBehavior, "netCurrentBehavior")
			.AddField(this.homeLocationName, "homeLocationName")
			.AddField(this.petPushEvent, "petPushEvent")
			.AddField(this.lastPetDay, "lastPetDay")
			.AddField(this.grantedFriendshipForPet, "grantedFriendshipForPet")
			.AddField(this.friendshipTowardFarmer, "friendshipTowardFarmer")
			.AddField(this.isSleepingOnFarmerBed, "isSleepingOnFarmerBed")
			.AddField(this.mutex.NetFields, "mutex.NetFields")
			.AddField(this.hat, "hat")
			.AddField(this.timesPet, "timesPet");
		base.name.FilterStringEvent += Utility.FilterDirtyWords;
		base.name.fieldChangeVisibleEvent += delegate
		{
			base.resetCachedDisplayName();
		};
		this.petPushEvent.onEvent += OnPetPush;
		this.friendshipTowardFarmer.fieldChangeVisibleEvent += delegate
		{
			this.GrantLoveMailIfNecessary();
		};
		this.isSleepingOnFarmerBed.fieldChangeVisibleEvent += delegate
		{
			this.UpdateSleepingOnBed();
		};
		this.petType.fieldChangeVisibleEvent += delegate
		{
			this.reloadBreedSprite();
		};
		this.whichBreed.fieldChangeVisibleEvent += delegate
		{
			this.reloadBreedSprite();
		};
		this.netCurrentBehavior.fieldChangeVisibleEvent += delegate
		{
			if (this._currentBehavior != this.CurrentBehavior)
			{
				this._OnNewBehavior();
			}
		};
	}

	public virtual void OnPetPush(long farmerId)
	{
		this.pushingTimer = 0;
		if (Game1.IsMasterGame)
		{
			Farmer farmer = Game1.GetPlayer(farmerId) ?? Game1.player;
			Vector2 trajectory = Utility.getAwayFromPlayerTrajectory(this.GetBoundingBox(), farmer);
			base.setTrajectory((int)trajectory.X / 2, (int)trajectory.Y / 2);
			this._walkFromPushTimer = 250;
			this.CurrentBehavior = "Walk";
			this.OnNewBehavior();
			this.Halt();
			this.faceDirection(farmer.FacingDirection);
			base.setMovingInFacingDirection();
		}
	}

	public override int getTimeFarmerMustPushBeforeStartShaking()
	{
		return 300;
	}

	public override int getTimeFarmerMustPushBeforePassingThrough()
	{
		return 750;
	}

	public override void behaviorOnFarmerLocationEntry(GameLocation location, Farmer who)
	{
		base.behaviorOnFarmerLocationEntry(location, who);
		if (location is Farm && Game1.timeOfDay >= 2000 && !location.farmers.Any())
		{
			if (this.CurrentBehavior != "Sleep" || base.currentLocation is Farm)
			{
				Game1.player.team.requestPetWarpHomeEvent.Fire(Game1.player.UniqueMultiplayerID);
			}
		}
		else if (Game1.timeOfDay < 2000 && Game1.random.NextBool() && this._currentBehavior != "Sleep")
		{
			this.CurrentBehavior = "Sleep";
			this._OnNewBehavior();
			this.Sprite.UpdateSourceRect();
		}
		this.UpdateSleepingOnBed();
	}

	public override void behaviorOnLocalFarmerLocationEntry(GameLocation location)
	{
		base.behaviorOnLocalFarmerLocationEntry(location);
		this.netCurrentBehavior.CancelInterpolation();
		if (this.netCurrentBehavior.Value == "Sleep")
		{
			base.position.NetFields.CancelInterpolation();
			if (this._currentBehavior != "Sleep")
			{
				this._OnNewBehavior();
				this.Sprite.UpdateSourceRect();
			}
		}
		this.UpdateSleepingOnBed();
	}

	public override bool canTalk()
	{
		return false;
	}

	/// <summary>Get the data from <c>Data/Pets</c> for the pet type, if it's valid.</summary>
	public PetData GetPetData()
	{
		if (!Pet.TryGetData(this.petType.Value, out var petData))
		{
			return null;
		}
		return petData;
	}

	/// <summary>Get the underlying content data for a pet type, if any.</summary>
	/// <param name="petType">The pet type's ID in <c>Data/Pets</c>.</param>
	/// <param name="data">The pet data, if found.</param>
	/// <returns>Returns whether the pet data was found.</returns>
	public static bool TryGetData(string petType, out PetData data)
	{
		if (petType != null && Game1.petData.TryGetValue(petType, out data))
		{
			return true;
		}
		data = null;
		return false;
	}

	/// <summary>Get the icon to show in the game menu for this pet.</summary>
	/// <param name="assetName">The asset name for the texture.</param>
	/// <param name="sourceRect">The 16x16 pixel area within the texture for the icon.</param>
	public void GetPetIcon(out string assetName, out Rectangle sourceRect)
	{
		PetData petData = this.GetPetData();
		PetData dogData;
		PetBreed breed = petData?.GetBreedById(this.whichBreed.Value) ?? petData?.Breeds?.FirstOrDefault() ?? ((!Pet.TryGetData("Dog", out dogData)) ? null : dogData.Breeds?.FirstOrDefault());
		if (breed != null)
		{
			assetName = breed.IconTexture;
			sourceRect = breed.IconSourceRect;
		}
		else
		{
			assetName = "Animals\\dog";
			sourceRect = new Rectangle(208, 208, 16, 16);
		}
	}

	public virtual string getPetTextureName()
	{
		try
		{
			PetData petType = this.GetPetData();
			if (petType != null)
			{
				return petType.GetBreedById(this.whichBreed.Value).Texture;
			}
		}
		catch (Exception)
		{
		}
		return "Animals\\dog";
	}

	public void reloadBreedSprite()
	{
		this.Sprite?.LoadTexture(this.getPetTextureName());
	}

	/// <inheritdoc />
	public override void reloadSprite(bool onlyAppearance = false)
	{
		this.reloadBreedSprite();
		base.HideShadow = true;
		base.Breather = false;
		if (!onlyAppearance)
		{
			base.DefaultPosition = new Vector2(54f, 8f) * 64f;
			this.setAtFarmPosition();
			if (this.GetPetBowl() == null)
			{
				this.warpToFarmHouse(Game1.MasterPlayer);
			}
			this.GrantLoveMailIfNecessary();
		}
	}

	/// <inheritdoc />
	public override void ChooseAppearance(LocalizedContentManager content = null)
	{
		if (this.Sprite?.Texture == null)
		{
			this.reloadSprite(onlyAppearance: true);
		}
	}

	public void warpToFarmHouse(Farmer who)
	{
		PetData petData = this.GetPetData();
		this.isSleepingOnFarmerBed.Value = false;
		FarmHouse farmHouse = Utility.getHomeOfFarmer(who);
		int tries = 0;
		Vector2 sleepTile = new Vector2(Game1.random.Next(2, farmHouse.map.Layers[0].LayerWidth - 3), Game1.random.Next(3, farmHouse.map.Layers[0].LayerHeight - 5));
		List<Furniture> rugs = new List<Furniture>();
		foreach (Furniture house_furniture in farmHouse.furniture)
		{
			if (house_furniture.furniture_type.Value == 12)
			{
				rugs.Add(house_furniture);
			}
		}
		BedFurniture player_bed = farmHouse.GetPlayerBed();
		float sleepOnBedChance = 0f;
		float sleepAtBedFootChance = 0.3f;
		float sleepOnRugChance = 0.5f;
		if (petData != null)
		{
			sleepOnBedChance = petData.SleepOnBedChance;
			sleepAtBedFootChance = petData.SleepNearBedChance;
			sleepOnRugChance = petData.SleepOnRugChance;
		}
		if (player_bed != null && !Game1.newDay && Game1.timeOfDay >= 2000 && Game1.random.NextDouble() <= (double)sleepOnBedChance)
		{
			sleepTile = Utility.PointToVector2(player_bed.GetBedSpot()) + new Vector2(-1f, 0f);
			if (farmHouse.isCharacterAtTile(sleepTile) == null)
			{
				Game1.warpCharacter(this, farmHouse, sleepTile);
				base.NetFields.CancelInterpolation();
				this.CurrentBehavior = "Sleep";
				this.isSleepingOnFarmerBed.Value = true;
				Rectangle petBounds = this.GetBoundingBox();
				foreach (Furniture item in farmHouse.furniture)
				{
					if (item is BedFurniture bed && bed.GetBoundingBox().Intersects(petBounds))
					{
						bed.ReserveForNPC();
						break;
					}
				}
				this.UpdateSleepingOnBed();
				this._OnNewBehavior();
				this.Sprite.UpdateSourceRect();
				return;
			}
		}
		else if (Game1.random.NextDouble() <= (double)sleepAtBedFootChance)
		{
			sleepTile = Utility.PointToVector2(farmHouse.getBedSpot()) + new Vector2(0f, 2f);
		}
		else if (Game1.random.NextDouble() <= (double)sleepOnRugChance)
		{
			Furniture rug = Game1.random.ChooseFrom(rugs);
			if (rug != null)
			{
				sleepTile = Utility.getRandomPositionInThisRectangle(rug.boundingBox.Value, Game1.random) / 64f;
			}
		}
		for (; tries < 50; tries++)
		{
			if (farmHouse.canPetWarpHere(sleepTile) && farmHouse.CanItemBePlacedHere(sleepTile, itemIsPassable: false, ~CollisionMask.Farmers) && farmHouse.CanItemBePlacedHere(sleepTile + new Vector2(1f, 0f), itemIsPassable: false, ~CollisionMask.Farmers) && !farmHouse.isTileOnWall((int)sleepTile.X, (int)sleepTile.Y))
			{
				break;
			}
			sleepTile = new Vector2(Game1.random.Next(2, farmHouse.map.Layers[0].LayerWidth - 3), Game1.random.Next(3, farmHouse.map.Layers[0].LayerHeight - 4));
		}
		if (tries < 50)
		{
			Game1.warpCharacter(this, farmHouse, sleepTile);
			this.CurrentBehavior = "Sleep";
		}
		else
		{
			this.WarpToPetBowl();
		}
		this.UpdateSleepingOnBed();
		this._OnNewBehavior();
		this.Sprite.UpdateSourceRect();
	}

	public virtual void UpdateSleepingOnBed()
	{
		base.drawOnTop = false;
		base.collidesWithOtherCharacters.Value = !this.isSleepingOnFarmerBed.Value;
		base.farmerPassesThrough = this.isSleepingOnFarmerBed.Value;
	}

	public override void dayUpdate(int dayOfMonth)
	{
		this.isSleepingOnFarmerBed.Value = false;
		this.UpdateSleepingOnBed();
		base.DefaultPosition = new Vector2(54f, 8f) * 64f;
		this.Sprite.loop = false;
		base.Breather = false;
		if (Game1.IsMasterGame && this.GetPetBowl() == null)
		{
			foreach (Building building in Game1.getFarm().buildings)
			{
				if (building is PetBowl bowl && !bowl.HasPet())
				{
					bowl.AssignPet(this);
					break;
				}
			}
		}
		PetBowl petBowl = this.GetPetBowl();
		if (Game1.isRaining)
		{
			this.CurrentBehavior = "SitDown";
			this.warpToFarmHouse(Game1.player);
		}
		else if (petBowl != null && base.currentLocation is FarmHouse)
		{
			this.setAtFarmPosition();
		}
		else if (petBowl == null)
		{
			this.warpToFarmHouse(Game1.player);
		}
		if (Game1.IsMasterGame)
		{
			if (petBowl != null && petBowl.watered.Value)
			{
				this.friendshipTowardFarmer.Set(Math.Min(1000, this.friendshipTowardFarmer.Value + 6));
				petBowl.watered.Set(newValue: false);
			}
			if (petBowl == null)
			{
				this.friendshipTowardFarmer.Value -= 10;
			}
		}
		if (petBowl == null)
		{
			Game1.addMorningFluffFunction(delegate
			{
				base.doEmote(28);
			});
		}
		this.Halt();
		this.CurrentBehavior = "Sleep";
		this.grantedFriendshipForPet.Set(newValue: false);
		this._OnNewBehavior();
		this.Sprite.UpdateSourceRect();
	}

	public void GrantLoveMailIfNecessary()
	{
		if (this.friendshipTowardFarmer.Value < 1000)
		{
			return;
		}
		foreach (Farmer farmer in Game1.getAllFarmers())
		{
			if (farmer != null && farmer.mailReceived.Add("petLoveMessage") && farmer == Game1.player)
			{
				if (Game1.newDay)
				{
					Game1.addMorningFluffFunction(delegate
					{
						Game1.showGlobalMessage(Game1.content.LoadString("Strings\\Characters:PetLovesYou", this.displayName));
					});
				}
				else
				{
					Game1.showGlobalMessage(Game1.content.LoadString("Strings\\Characters:PetLovesYou", this.displayName));
				}
			}
			if (!farmer.hasOrWillReceiveMail("MarniePetAdoption"))
			{
				Game1.addMailForTomorrow("MarniePetAdoption");
			}
		}
	}

	/// <summary>Get the pet bowl assigned to this pet, if any.</summary>
	public PetBowl GetPetBowl()
	{
		foreach (Building building in (Game1.getLocationFromName(this.homeLocationName.Value) ?? Game1.getFarm()).buildings)
		{
			if (building is PetBowl bowl && bowl.petId.Value == this.petId.Value)
			{
				return bowl;
			}
		}
		return null;
	}

	/// <summary>Warp the pet to its assigned pet bowl, if any.</summary>
	public virtual void WarpToPetBowl()
	{
		PetBowl bowl = this.GetPetBowl();
		if (bowl != null)
		{
			this.faceDirection(2);
			Game1.warpCharacter(this, bowl.parentLocationName.Value, bowl.GetPetSpot());
		}
	}

	public void setAtFarmPosition()
	{
		if (Game1.IsMasterGame)
		{
			if (!Game1.isRaining)
			{
				this.WarpToPetBowl();
			}
			else
			{
				this.warpToFarmHouse(Game1.MasterPlayer);
			}
		}
	}

	public override bool shouldCollideWithBuildingLayer(GameLocation location)
	{
		return true;
	}

	public override bool canPassThroughActionTiles()
	{
		return false;
	}

	public void unassignPetBowl()
	{
		foreach (Building building in (Game1.getLocationFromName(this.homeLocationName.Value) ?? Game1.getFarm()).buildings)
		{
			if (building is PetBowl bowl && bowl.petId.Value == this.petId.Value)
			{
				bowl.petId.Value = Guid.Empty;
			}
		}
	}

	public void applyButterflyPowder(Farmer who, string responseKey)
	{
		if (responseKey.Contains("Yes"))
		{
			GameLocation l = base.currentLocation;
			this.unassignPetBowl();
			l.characters.Remove(this);
			this.playContentSound();
			Game1.playSound("fireball");
			Rectangle r = this.GetBoundingBox();
			r.Inflate(32, 32);
			r.X -= 32;
			r.Y -= 32;
			l.temporarySprites.AddRange(Utility.sparkleWithinArea(r, 6, Color.White, 50));
			l.temporarySprites.Add(new TemporaryAnimatedSprite(5, Utility.PointToVector2(this.GetBoundingBox().Center) - new Vector2(32f), Color.White, 8, flipped: false, 50f));
			for (int i = 0; i < 8; i++)
			{
				l.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(372, 1956, 10, 10), base.Position + new Vector2(32f) + new Vector2(Game1.random.Next(-16, 16), Game1.random.Next(-32, 16)), flipped: false, 0.002f, Color.White)
				{
					alphaFade = 0.0043333336f,
					alpha = 0.75f,
					motion = new Vector2((float)Game1.random.Next(-10, 11) / 20f, -1f),
					acceleration = new Vector2(0f, 0f),
					interval = 99999f,
					layerDepth = 1f,
					scale = 3f,
					scaleChange = 0.01f,
					rotationChange = (float)Game1.random.Next(-5, 6) * (float)Math.PI / 256f
				});
			}
			l.instantiateCrittersList();
			l.addCritter(new Butterfly(l, base.Tile + new Vector2(0f, 1f)));
			who.reduceActiveItemByOne();
			if (this.hat.Value != null)
			{
				Game1.createItemDebris(this.hat.Value, base.Position, -1, l);
			}
			Game1.showGlobalMessage(Game1.content.LoadString("Strings\\1_6_Strings:ButterflyPowder_Goodbye", base.Name));
		}
	}

	public override bool checkAction(Farmer who, GameLocation l)
	{
		if (who.Items.Count > who.CurrentToolIndex && who.Items[who.CurrentToolIndex] != null && who.Items[who.CurrentToolIndex] is Hat && (this.petType.Value == "Cat" || this.petType.Value == "Dog"))
		{
			if (this.hat.Value != null)
			{
				Game1.createItemDebris(this.hat.Value, base.Position, this.FacingDirection);
				this.hat.Value = null;
			}
			else
			{
				Hat hatItem = who.Items[who.CurrentToolIndex] as Hat;
				who.Items[who.CurrentToolIndex] = null;
				this.hat.Value = hatItem;
				Game1.playSound("dirtyHit");
			}
			this.mutex.ReleaseLock();
		}
		if (who.CurrentItem != null && who.CurrentItem.QualifiedItemId.Equals("(O)ButterflyPowder"))
		{
			l.createQuestionDialogue(Game1.content.LoadString("Strings\\1_6_Strings:ButterflyPowder_Question", base.Name), l.createYesNoResponses(), applyButterflyPowder);
		}
		if (!this.lastPetDay.TryGetValue(who.UniqueMultiplayerID, out var curLastPetDay) || curLastPetDay != Game1.Date.TotalDays)
		{
			this.lastPetDay[who.UniqueMultiplayerID] = Game1.Date.TotalDays;
			this.mutex.RequestLock(delegate
			{
				if (!this.grantedFriendshipForPet.Value)
				{
					this.grantedFriendshipForPet.Set(newValue: true);
					this.friendshipTowardFarmer.Set(Math.Min(1000, this.friendshipTowardFarmer.Value + 12));
					if (Utility.CreateDaySaveRandom(this.timesPet.Value, 71928.0, this.petId.Value.GetHashCode()).NextDouble() < (double)this.GetPetData().GiftChance)
					{
						Item item = this.TryGetGiftItem(this.GetPetData().Gifts);
						if (item != null)
						{
							Game1.createMultipleItemDebris(item, base.Position, -1, l, -1, flopFish: true);
						}
					}
					this.timesPet.Value++;
				}
				this.mutex.ReleaseLock();
			});
			base.doEmote(20);
			this.playContentSound();
			return true;
		}
		return false;
	}

	public virtual void playContentSound()
	{
		if (!Utility.isOnScreen(base.TilePoint, 128, base.currentLocation) || Game1.options.muteAnimalSounds)
		{
			return;
		}
		PetData petData = this.GetPetData();
		if (petData == null || petData.ContentSound == null)
		{
			return;
		}
		string contentSound = petData.ContentSound;
		this.PlaySound(contentSound, is_voice: true, -1, -1);
		if (petData.RepeatContentSoundAfter >= 0)
		{
			DelayedAction.functionAfterDelay(delegate
			{
				this.PlaySound(contentSound, is_voice: true, -1, -1);
			}, petData.RepeatContentSoundAfter);
		}
	}

	public void hold(Farmer who)
	{
		FarmerSprite.AnimationFrame lastFrame = this.Sprite.CurrentAnimation.Last();
		base.flip = lastFrame.flip;
		this.Sprite.CurrentFrame = lastFrame.frame;
		this.Sprite.CurrentAnimation = null;
		this.Sprite.loop = false;
	}

	public override void behaviorOnFarmerPushing()
	{
		if (!(this.CurrentBehavior == "Sprint"))
		{
			this.pushingTimer += 2;
			if (this.pushingTimer > 100)
			{
				this.petPushEvent.Fire(Game1.player.UniqueMultiplayerID);
			}
		}
	}

	public override void update(GameTime time, GameLocation location, long id, bool move)
	{
		base.update(time, location, id, move);
		this.pushingTimer = Math.Max(0, this.pushingTimer - 1);
	}

	public override void update(GameTime time, GameLocation location)
	{
		base.update(time, location);
		this.petPushEvent.Poll();
		if (this.isSleepingOnFarmerBed.Value && this.CurrentBehavior != "Sleep" && Game1.IsMasterGame)
		{
			this.isSleepingOnFarmerBed.Value = false;
			this.UpdateSleepingOnBed();
		}
		if (base.currentLocation == null)
		{
			base.currentLocation = location;
		}
		this.mutex.Update(location);
		if (Game1.eventUp)
		{
			return;
		}
		if (this._currentBehavior != this.CurrentBehavior)
		{
			this._OnNewBehavior();
		}
		this.RunState(time);
		if (Game1.IsMasterGame)
		{
			PetBehavior currentBehavior = this.GetCurrentPetBehavior();
			if (currentBehavior != null && currentBehavior.WalkInDirection)
			{
				if (currentBehavior.Animation == null)
				{
					this.MovePosition(time, Game1.viewport, location);
				}
				else
				{
					base.tryToMoveInDirection(this.FacingDirection, isFarmer: false, -1, glider: false);
				}
			}
		}
		base.flip = false;
		if (this.FacingDirection == 3 && this.Sprite.CurrentFrame >= 16)
		{
			base.flip = true;
		}
	}

	public Item TryGetGiftItem(List<PetGift> gifts)
	{
		float totalWeight = 0f;
		gifts = new List<PetGift>(gifts);
		gifts.RemoveAll(delegate(PetGift petGift)
		{
			if (this.friendshipTowardFarmer.Value >= petGift.MinimumFriendshipThreshold && GameStateQuery.CheckConditions(petGift.Condition))
			{
				totalWeight += petGift.Weight;
				return false;
			}
			return true;
		});
		if (gifts.Count > 0)
		{
			totalWeight = Utility.RandomFloat(0f, totalWeight);
			foreach (PetGift gift in gifts)
			{
				totalWeight -= gift.Weight;
				if (totalWeight <= 0f)
				{
					Item item = ItemQueryResolver.TryResolveRandomItem(gift, null);
					if (item != null && !item.Name.Contains("Error Item"))
					{
						return item;
					}
				}
			}
		}
		return null;
	}

	public bool TryBehaviorChange(List<PetBehaviorChanges> changes)
	{
		float totalWeight = 0f;
		foreach (PetBehaviorChanges change in changes)
		{
			if (!change.OutsideOnly || base.currentLocation.IsOutdoors)
			{
				totalWeight += change.Weight;
			}
		}
		totalWeight = Utility.RandomFloat(0f, totalWeight);
		foreach (PetBehaviorChanges change2 in changes)
		{
			if (change2.OutsideOnly && !base.currentLocation.IsOutdoors)
			{
				continue;
			}
			totalWeight -= change2.Weight;
			if (totalWeight <= 0f)
			{
				string nextBehavior = null;
				switch (this.FacingDirection)
				{
				case 0:
					nextBehavior = change2.UpBehavior;
					break;
				case 2:
					nextBehavior = change2.DownBehavior;
					break;
				case 3:
					nextBehavior = change2.LeftBehavior;
					break;
				case 1:
					nextBehavior = change2.RightBehavior;
					break;
				}
				if (nextBehavior == null)
				{
					nextBehavior = change2.Behavior;
				}
				if (nextBehavior != null)
				{
					this.CurrentBehavior = nextBehavior;
				}
				return true;
			}
		}
		return false;
	}

	public PetBehavior GetCurrentPetBehavior()
	{
		PetData petData = this.GetPetData();
		if (petData?.Behaviors != null)
		{
			foreach (PetBehavior behavior in petData.Behaviors)
			{
				if (behavior.Id == this.CurrentBehavior)
				{
					return behavior;
				}
			}
		}
		return null;
	}

	public virtual void RunState(GameTime time)
	{
		if (this._currentBehavior == "Walk" && Game1.IsMasterGame && this._walkFromPushTimer <= 0 && base.currentLocation.isCollidingPosition(this.nextPosition(this.FacingDirection), Game1.viewport, this))
		{
			int new_direction = Game1.random.Next(0, 4);
			if (!base.currentLocation.isCollidingPosition(this.nextPosition(this.FacingDirection), Game1.viewport, this))
			{
				this.faceDirection(new_direction);
			}
		}
		if (Game1.IsMasterGame && Game1.timeOfDay >= 2000 && this.Sprite.CurrentAnimation == null && base.xVelocity == 0f && base.yVelocity == 0f)
		{
			this.CurrentBehavior = "Sleep";
		}
		if (this.CurrentBehavior == "Sleep")
		{
			if (Game1.IsMasterGame && Game1.timeOfDay < 2000 && Game1.random.NextDouble() < 0.001)
			{
				this.CurrentBehavior = "Walk";
			}
			if (Game1.random.NextDouble() < 0.002)
			{
				base.doEmote(24);
			}
		}
		if (this._walkFromPushTimer > 0)
		{
			this._walkFromPushTimer -= (int)time.ElapsedGameTime.TotalMilliseconds;
			if (this._walkFromPushTimer <= 0)
			{
				this._walkFromPushTimer = 0;
			}
		}
		PetBehavior behavior = this.GetCurrentPetBehavior();
		if (behavior == null || !Game1.IsMasterGame)
		{
			return;
		}
		if (this.behaviorTimer >= 0)
		{
			this.behaviorTimer -= (int)time.ElapsedGameTime.TotalMilliseconds;
			if (this.behaviorTimer <= 0)
			{
				this.behaviorTimer = -1;
				this.TryBehaviorChange(behavior.TimeoutBehaviorChanges);
				return;
			}
		}
		if (this._walkFromPushTimer <= 0)
		{
			if (behavior.RandomBehaviorChanges != null && behavior.RandomBehaviorChangeChance > 0f && Game1.random.NextDouble() < (double)behavior.RandomBehaviorChangeChance)
			{
				this.TryBehaviorChange(behavior.RandomBehaviorChanges);
				return;
			}
			if (behavior.PlayerNearbyBehaviorChanges != null && this.withinPlayerThreshold(2))
			{
				this.TryBehaviorChange(behavior.PlayerNearbyBehaviorChanges);
				return;
			}
		}
		if (behavior.JumpLandBehaviorChanges != null && base.yJumpOffset == 0 && base.yJumpVelocity == 0f)
		{
			this.TryBehaviorChange(behavior.JumpLandBehaviorChanges);
		}
	}

	protected override void updateSlaveAnimation(GameTime time)
	{
		if (this.Sprite.CurrentAnimation != null)
		{
			this.Sprite.animateOnce(time);
		}
		else
		{
			if (!(this.CurrentBehavior == "Walk"))
			{
				return;
			}
			this.Sprite.faceDirection(this.FacingDirection);
			if (this.isMoving())
			{
				this.animateInFacingDirection(time);
				int target = -1;
				switch (this.FacingDirection)
				{
				case 0:
					target = 12;
					break;
				case 2:
					target = 4;
					break;
				case 3:
					target = 16;
					break;
				case 1:
					target = 8;
					break;
				}
				if (this.Sprite.CurrentFrame == target)
				{
					this.Sprite.CurrentFrame -= 4;
				}
			}
			else
			{
				this.Sprite.StopAnimation();
			}
		}
	}

	protected void _OnNewBehavior()
	{
		this._currentBehavior = this.CurrentBehavior;
		this.Halt();
		this.Sprite.CurrentAnimation = null;
		this.OnNewBehavior();
	}

	public virtual void OnNewBehavior()
	{
		this.Sprite.loop = false;
		this.Sprite.CurrentAnimation = null;
		this.behaviorTimer = -1;
		this.animationLoopsLeft = -1;
		if (this.CurrentBehavior == "Sleep")
		{
			this.Sprite.loop = true;
			bool local_sleep_flip = Game1.random.NextBool();
			this.Sprite.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
			{
				new FarmerSprite.AnimationFrame(28, 1000, secondaryArm: false, local_sleep_flip),
				new FarmerSprite.AnimationFrame(29, 1000, secondaryArm: false, local_sleep_flip)
			});
		}
		PetBehavior behavior = this.GetCurrentPetBehavior();
		if (behavior == null)
		{
			return;
		}
		if (Game1.IsMasterGame)
		{
			if (this._walkFromPushTimer <= 0)
			{
				if (Utility.TryParseDirection(behavior.Direction, out var direction))
				{
					this.FacingDirection = direction;
				}
				if (behavior.RandomizeDirection)
				{
					this.FacingDirection = (behavior.IsSideBehavior ? Game1.random.Choose(3, 1) : Game1.random.Next(4));
				}
			}
			if ((this.FacingDirection == 0 || this.FacingDirection == 2) && behavior.IsSideBehavior)
			{
				this.FacingDirection = ((!Game1.random.NextBool()) ? 1 : 3);
			}
			if (behavior.WalkInDirection)
			{
				if (behavior.MoveSpeed >= 0)
				{
					base.speed = behavior.MoveSpeed;
				}
				base.setMovingInFacingDirection();
			}
			if (behavior.Duration >= 0)
			{
				this.behaviorTimer = behavior.Duration;
			}
			else if (behavior.MinimumDuration >= 0 && behavior.MaximumDuration >= 0)
			{
				this.behaviorTimer = Game1.random.Next(behavior.MinimumDuration, behavior.MaximumDuration + 1);
			}
		}
		if (behavior.SoundOnStart != null)
		{
			this.PlaySound(behavior.SoundOnStart, behavior.SoundIsVoice, behavior.SoundRangeFromBorder, behavior.SoundRange);
		}
		if (behavior.Shake > 0)
		{
			base.shake(behavior.Shake);
		}
		if (behavior.Animation == null)
		{
			return;
		}
		this.Sprite.ClearAnimation();
		for (int i = 0; i < behavior.Animation.Count; i++)
		{
			FarmerSprite.AnimationFrame frame = new FarmerSprite.AnimationFrame(behavior.Animation[i].Frame, behavior.Animation[i].Duration, secondaryArm: false, flip: false);
			if (behavior.Animation[i].HitGround)
			{
				frame.AddFrameAction(hitGround);
			}
			if (behavior.Animation[i].Jump)
			{
				this.jump();
			}
			if (behavior.AnimationMinimumLoops >= 0 && behavior.AnimationMaximumLoops >= 0)
			{
				this.animationLoopsLeft = Game1.random.Next(behavior.AnimationMinimumLoops, behavior.AnimationMaximumLoops + 1);
			}
			if (behavior.Animation[i].Sound != null)
			{
				frame.AddFrameAction(_PerformAnimationSound);
			}
			if (i == behavior.Animation.Count - 1)
			{
				if (this.animationLoopsLeft > 0 || behavior.AnimationEndBehaviorChanges != null)
				{
					frame.AddFrameEndAction(_TryAnimationEndBehaviorChange);
				}
				if (behavior.LoopMode == PetAnimationLoopMode.Hold)
				{
					if (behavior.AnimationEndBehaviorChanges != null)
					{
						frame.AddFrameEndAction(hold);
					}
					else
					{
						frame.AddFrameAction(hold);
					}
				}
			}
			this.Sprite.AddFrame(frame);
			if (behavior.Animation.Count == 1 && behavior.LoopMode == PetAnimationLoopMode.Hold)
			{
				this.Sprite.AddFrame(frame);
			}
			this.Sprite.UpdateSourceRect();
		}
		this.Sprite.loop = behavior.LoopMode == PetAnimationLoopMode.Loop || this.animationLoopsLeft > 0;
	}

	public void _PerformAnimationSound(Farmer who)
	{
		PetBehavior behavior = this.GetCurrentPetBehavior();
		if (behavior?.Animation != null && this.Sprite.currentAnimationIndex >= 0 && this.Sprite.currentAnimationIndex < behavior.Animation.Count)
		{
			PetAnimationFrame frame = behavior.Animation[this.Sprite.currentAnimationIndex];
			if (frame.Sound != null)
			{
				this.PlaySound(frame.Sound, frame.SoundIsVoice, frame.SoundRangeFromBorder, frame.SoundRange);
			}
		}
	}

	public void PlaySound(string sound, bool is_voice, int range_from_border, int range)
	{
		if ((Game1.options.muteAnimalSounds && is_voice) || !this.IsSoundInRange(range_from_border, range))
		{
			return;
		}
		float pitch = 1f;
		PetBreed breed = this.GetPetData().GetBreedById(this.whichBreed.Value);
		if (sound == "BARK")
		{
			sound = this.GetPetData().BarkSound;
			if (breed.BarkOverride != null)
			{
				sound = breed.BarkOverride;
			}
		}
		if (is_voice)
		{
			pitch = breed.VoicePitch;
		}
		if (pitch != 1f)
		{
			base.playNearbySoundAll(sound, (int)(1200f * pitch));
		}
		else
		{
			Game1.playSound(sound);
		}
	}

	public bool IsSoundInRange(int range_from_border, int sound_range)
	{
		if (sound_range > 0)
		{
			return this.withinLocalPlayerThreshold(sound_range);
		}
		if (range_from_border > 0)
		{
			return Utility.isOnScreen(base.TilePoint, range_from_border * 64, base.currentLocation);
		}
		return true;
	}

	public virtual void _TryAnimationEndBehaviorChange(Farmer who)
	{
		if (this.animationLoopsLeft <= 0)
		{
			if (this.animationLoopsLeft == 0)
			{
				this.animationLoopsLeft = -1;
				this.hold(who);
			}
			PetBehavior behavior = this.GetCurrentPetBehavior();
			if (behavior != null && Game1.IsMasterGame)
			{
				this.TryBehaviorChange(behavior.AnimationEndBehaviorChanges);
			}
		}
		else
		{
			this.animationLoopsLeft--;
		}
	}

	public override Rectangle GetBoundingBox()
	{
		Vector2 position = base.Position;
		return new Rectangle((int)position.X + 16, (int)position.Y + 16, this.Sprite.SpriteWidth * 4 * 3 / 4, 32);
	}

	public virtual void drawHat(SpriteBatch b, Vector2 shake)
	{
		if (this.hat.Value == null)
		{
			return;
		}
		Vector2 hatOffset = Vector2.Zero;
		hatOffset *= 4f;
		if (hatOffset.X <= -100f)
		{
			return;
		}
		float horse_draw_layer = Math.Max(0f, this.isSleepingOnFarmerBed.Value ? (((float)base.StandingPixel.Y + 112f) / 10000f) : ((float)base.StandingPixel.Y / 10000f));
		hatOffset.X = -2f;
		hatOffset.Y = -24f;
		horse_draw_layer += 1E-07f;
		int direction = 2;
		bool flipped = base.flip || (base.sprite.Value.CurrentAnimation != null && base.sprite.Value.CurrentAnimation[base.sprite.Value.currentAnimationIndex].flip);
		float scale = 1.3333334f;
		string value = this.petType.Value;
		if (!(value == "Cat"))
		{
			if (value == "Dog")
			{
				hatOffset.Y -= 20f;
				switch (this.Sprite.CurrentFrame)
				{
				case 16:
					hatOffset.Y += 20f;
					direction = 2;
					break;
				case 0:
				case 2:
					hatOffset.Y += 28f;
					direction = 2;
					break;
				case 1:
				case 3:
					hatOffset.Y += 32f;
					direction = 2;
					break;
				case 4:
				case 6:
					direction = 1;
					hatOffset.X += 26f;
					hatOffset.Y += 24f;
					break;
				case 5:
				case 7:
					direction = 1;
					hatOffset.X += 26f;
					hatOffset.Y += 28f;
					break;
				case 30:
				case 31:
					direction = ((!flipped) ? 1 : 3);
					hatOffset.X += 18f;
					hatOffset.Y += 8f;
					break;
				case 8:
				case 10:
					direction = 0;
					hatOffset.Y += 4f;
					break;
				case 9:
				case 11:
					direction = 0;
					hatOffset.Y += 8f;
					break;
				case 12:
				case 14:
					direction = 3;
					hatOffset.X -= 26f;
					hatOffset.Y += 24f;
					break;
				case 13:
				case 15:
					hatOffset.Y += 24f;
					hatOffset.Y += 4f;
					direction = 3;
					hatOffset.X -= 26f;
					break;
				case 23:
					direction = ((!flipped) ? 1 : 3);
					hatOffset.X += 18f;
					hatOffset.Y += 8f;
					break;
				case 20:
					direction = ((!flipped) ? 1 : 3);
					hatOffset.X += 26f;
					hatOffset.Y += ((this.whichBreed.Value == "2") ? 16 : ((this.whichBreed.Value == "1") ? 24 : 20));
					break;
				case 21:
					direction = ((!flipped) ? 1 : 3);
					hatOffset.X += 22f;
					hatOffset.Y += ((this.whichBreed.Value == "2") ? 12 : ((this.whichBreed.Value == "1") ? 20 : 16));
					break;
				case 22:
					direction = ((!flipped) ? 1 : 3);
					hatOffset.X += 18f;
					hatOffset.Y += ((this.whichBreed.Value == "2") ? 8 : ((this.whichBreed.Value == "1") ? 8 : 12));
					break;
				case 17:
					hatOffset.Y += 12f;
					break;
				case 18:
				case 19:
					hatOffset.Y += 8f;
					break;
				case 24:
				case 25:
					direction = ((!flipped) ? 1 : 3);
					hatOffset.X += 21 - (flipped ? 4 : 4) + 1;
					hatOffset.Y += 8f;
					break;
				case 26:
					direction = ((!flipped) ? 1 : 3);
					hatOffset.X += 18f;
					hatOffset.Y -= 8f;
					break;
				case 27:
					direction = 2;
					hatOffset.Y += 12 + ((this.whichBreed.Value == "2") ? (-4) : 0);
					break;
				case 28:
				case 29:
					scale = 1.3333334f;
					hatOffset.Y += 48f;
					hatOffset.X += (flipped ? 6 : 5) * 4;
					hatOffset.X += 2f;
					direction = 2;
					break;
				case 32:
					direction = ((!flipped) ? 1 : 3);
					hatOffset.X += 26f;
					hatOffset.Y += ((this.whichBreed.Value == "2") ? 12 : 16);
					break;
				case 33:
					direction = ((!flipped) ? 1 : 3);
					hatOffset.X += 26f;
					hatOffset.Y += ((this.whichBreed.Value == "2") ? 16 : 20);
					break;
				case 34:
					direction = ((!flipped) ? 1 : 3);
					hatOffset.X += 26f;
					hatOffset.Y += ((this.whichBreed.Value == "2") ? 20 : 24);
					break;
				}
				string value2 = this.whichBreed.Value;
				if (!(value2 == "2"))
				{
					if (value2 == "3" && direction == 3 && this.Sprite.CurrentFrame > 16)
					{
						hatOffset.X += 4f;
					}
				}
				else
				{
					if (direction == 1)
					{
						hatOffset.X -= 4f;
					}
					hatOffset.Y += 8f;
				}
				if (flipped)
				{
					hatOffset.X *= -1f;
				}
			}
		}
		else
		{
			switch (this.Sprite.CurrentFrame)
			{
			case 16:
				hatOffset.Y += 20f;
				direction = 2;
				break;
			case 0:
			case 2:
				hatOffset.Y += 28f;
				direction = 2;
				break;
			case 1:
			case 3:
				hatOffset.Y += 32f;
				direction = 2;
				break;
			case 4:
			case 6:
				direction = 1;
				hatOffset.X += 23f;
				hatOffset.Y += 20f;
				break;
			case 5:
			case 7:
				hatOffset.Y += 4f;
				direction = 1;
				hatOffset.X += 23f;
				hatOffset.Y += 20f;
				break;
			case 30:
			case 31:
				direction = ((!flipped) ? 1 : 3);
				hatOffset.X += ((!flipped) ? 1 : (-1)) * 25;
				hatOffset.Y += 32f;
				break;
			case 8:
			case 10:
				direction = 0;
				hatOffset.Y -= 4f;
				break;
			case 9:
			case 11:
				direction = 0;
				break;
			case 12:
			case 14:
				direction = 3;
				hatOffset.X -= 22f;
				hatOffset.Y += 20f;
				break;
			case 13:
			case 15:
				hatOffset.Y += 20f;
				hatOffset.Y += 4f;
				direction = 3;
				hatOffset.X -= 22f;
				break;
			case 21:
			case 23:
				hatOffset.Y += 16f;
				break;
			case 17:
			case 20:
			case 22:
				hatOffset.Y += 12f;
				break;
			case 18:
			case 19:
				hatOffset.Y += 8f;
				break;
			case 24:
				direction = ((!flipped) ? 1 : 3);
				hatOffset.X += ((!flipped) ? 1 : (-1)) * 29;
				hatOffset.Y += 28f;
				break;
			case 25:
				direction = ((!flipped) ? 1 : 3);
				hatOffset.X += ((!flipped) ? 1 : (-1)) * 29;
				hatOffset.Y += 36f;
				break;
			case 26:
				direction = ((!flipped) ? 1 : 3);
				hatOffset.X += ((!flipped) ? 1 : (-1)) * 29;
				hatOffset.Y += 40f;
				break;
			case 27:
				direction = ((!flipped) ? 1 : 3);
				hatOffset.X += ((!flipped) ? 1 : (-1)) * 29;
				hatOffset.Y += 44f;
				break;
			case 28:
			case 29:
				scale = 1.2f;
				hatOffset.Y += 46f;
				hatOffset.X -= ((!flipped) ? (-1) : 0) * 4;
				hatOffset.X += ((!flipped) ? 1 : (-1)) * 2;
				direction = (flipped ? 1 : 3);
				break;
			}
			if ((this.whichBreed.Value == "3" || this.whichBreed.Value == "4") && direction == 3)
			{
				hatOffset.X -= 4f;
			}
		}
		hatOffset += shake;
		if (flipped)
		{
			hatOffset.X -= 4f;
		}
		this.hat.Value.draw(b, base.getLocalPosition(Game1.viewport) + hatOffset + new Vector2(30f, -42f), scale, 1f, horse_draw_layer, direction, useAnimalTexture: true);
	}

	public override void draw(SpriteBatch b)
	{
		int standingY = base.StandingPixel.Y;
		Vector2 shake = ((base.shakeTimer > 0 && !this.isSleepingOnFarmerBed.Value) ? new Vector2(Game1.random.Next(-1, 2), Game1.random.Next(-1, 2)) : Vector2.Zero);
		b.Draw(this.Sprite.Texture, base.getLocalPosition(Game1.viewport) + new Vector2(this.Sprite.SpriteWidth * 4 / 2, this.GetBoundingBox().Height / 2) + shake, this.Sprite.SourceRect, Color.White, base.rotation, new Vector2(this.Sprite.SpriteWidth / 2, (float)this.Sprite.SpriteHeight * 3f / 4f), Math.Max(0.2f, base.scale.Value) * 4f, (base.flip || (this.Sprite.CurrentAnimation != null && this.Sprite.CurrentAnimation[this.Sprite.currentAnimationIndex].flip)) ? SpriteEffects.FlipHorizontally : SpriteEffects.None, Math.Max(0f, this.isSleepingOnFarmerBed.Value ? (((float)standingY + 112f) / 10000f) : ((float)standingY / 10000f)));
		this.drawHat(b, shake);
		if (base.IsEmoting)
		{
			Vector2 localPosition = base.getLocalPosition(Game1.viewport);
			Point emoteOffset = this.GetPetData()?.EmoteOffset ?? Point.Zero;
			b.Draw(position: new Vector2(localPosition.X + 32f + (float)emoteOffset.X, localPosition.Y - 96f + (float)emoteOffset.Y), texture: Game1.emoteSpriteSheet, sourceRectangle: new Rectangle(base.CurrentEmoteIndex * 16 % Game1.emoteSpriteSheet.Width, base.CurrentEmoteIndex * 16 / Game1.emoteSpriteSheet.Width * 16, 16, 16), color: Color.White, rotation: 0f, origin: Vector2.Zero, scale: 4f, effects: SpriteEffects.None, layerDepth: (float)standingY / 10000f + 0.0001f);
		}
	}

	public virtual bool withinLocalPlayerThreshold(int threshold)
	{
		if (base.currentLocation != Game1.currentLocation)
		{
			return false;
		}
		Vector2 tileLocationOfMonster = base.Tile;
		Vector2 tileLocationOfPlayer = Game1.player.Tile;
		if (Math.Abs(tileLocationOfMonster.X - tileLocationOfPlayer.X) <= (float)threshold)
		{
			return Math.Abs(tileLocationOfMonster.Y - tileLocationOfPlayer.Y) <= (float)threshold;
		}
		return false;
	}

	public override bool withinPlayerThreshold(int threshold)
	{
		if (base.currentLocation != null && !base.currentLocation.farmers.Any())
		{
			return false;
		}
		Vector2 tileLocationOfMonster = base.Tile;
		foreach (Farmer farmer in base.currentLocation.farmers)
		{
			Vector2 tileLocationOfPlayer = farmer.Tile;
			if (Math.Abs(tileLocationOfMonster.X - tileLocationOfPlayer.X) <= (float)threshold && Math.Abs(tileLocationOfMonster.Y - tileLocationOfPlayer.Y) <= (float)threshold)
			{
				return true;
			}
		}
		return false;
	}

	public void hitGround(Farmer who)
	{
		if (Utility.isOnScreen(base.TilePoint, 128, base.currentLocation))
		{
			base.currentLocation.playTerrainSound(base.Tile, this, showTerrainDisturbAnimation: false);
		}
	}
}
