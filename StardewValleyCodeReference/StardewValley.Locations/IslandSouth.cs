using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.BellsAndWhistles;
using StardewValley.Extensions;
using StardewValley.TerrainFeatures;
using StardewValley.WorldMaps;
using xTile.Dimensions;

namespace StardewValley.Locations;

public class IslandSouth : IslandLocation
{
	public class IslandActivityAssigments
	{
		public int activityTime;

		public List<NPC> visitors;

		public Dictionary<Character, string> currentAssignments;

		public Dictionary<Character, string> currentAnimationAssignments;

		public Random random;

		public Dictionary<string, string> animationDescriptions;

		public List<Point> shoreLoungePoints = new List<Point>(new Point[6]
		{
			new Point(9, 33),
			new Point(13, 33),
			new Point(17, 33),
			new Point(24, 33),
			new Point(28, 32),
			new Point(32, 31)
		});

		public List<Point> chairPoints = new List<Point>(new Point[2]
		{
			new Point(20, 24),
			new Point(30, 29)
		});

		public List<Point> umbrellaPoints = new List<Point>(new Point[3]
		{
			new Point(26, 26),
			new Point(28, 29),
			new Point(10, 27)
		});

		public List<Point> towelLoungePoints = new List<Point>(new Point[4]
		{
			new Point(14, 27),
			new Point(17, 28),
			new Point(20, 27),
			new Point(23, 28)
		});

		public List<Point> drinkPoints = new List<Point>(new Point[2]
		{
			new Point(12, 23),
			new Point(15, 23)
		});

		public List<Point> wanderPoints = new List<Point>(new Point[3]
		{
			new Point(7, 16),
			new Point(31, 24),
			new Point(18, 13)
		});

		public IslandActivityAssigments(int time, List<NPC> visitors, Random seeded_random, Dictionary<Character, string> last_activity_assignments)
		{
			this.activityTime = time;
			this.visitors = new List<NPC>(visitors);
			this.random = seeded_random;
			Utility.Shuffle(this.random, this.visitors);
			this.animationDescriptions = DataLoader.AnimationDescriptions(Game1.content);
			this.FindActivityForCharacters(last_activity_assignments);
		}

		public virtual void FindActivityForCharacters(Dictionary<Character, string> last_activity_assignments)
		{
			this.currentAssignments = new Dictionary<Character, string>();
			this.currentAnimationAssignments = new Dictionary<Character, string>();
			foreach (NPC character in this.visitors)
			{
				if (this.currentAssignments.ContainsKey(character))
				{
					continue;
				}
				string name = character.Name;
				if (!(name == "Gus"))
				{
					if (!(name == "Sam") || !this.TryAssignment(character, this.towelLoungePoints, "Resort_Towel", character.name.Value.ToLower() + "_beach_towel", animation_required: true, 0.5, last_activity_assignments))
					{
						continue;
					}
					foreach (NPC other_character in this.visitors)
					{
						if (!this.currentAssignments.ContainsKey(other_character) && this.animationDescriptions.ContainsKey(other_character.Name.ToLower() + "_beach_dance"))
						{
							string[] array = ArgUtility.SplitBySpace(this.currentAssignments[character]);
							int x = int.Parse(array[0]);
							int y = int.Parse(array[1]);
							this.currentAssignments.Remove(other_character);
							this.TryAssignment(other_character, new List<Point>(new Point[1]
							{
								new Point(x + 1, y + 1)
							}), "Resort_Dance", other_character.Name.ToLower() + "_beach_dance", animation_required: true, 1.0, last_activity_assignments);
							other_character.currentScheduleDelay = 0f;
							character.currentScheduleDelay = 0f;
							break;
						}
					}
					continue;
				}
				this.currentAssignments[character] = "14 21 2";
				foreach (NPC other_character2 in this.visitors)
				{
					if (!this.currentAssignments.ContainsKey(other_character2) && other_character2.Age != 2)
					{
						this.TryAssignment(other_character2, this.drinkPoints, "Resort_Bar", other_character2.name.Value.ToLower() + "_beach_drink", animation_required: false, 0.5, last_activity_assignments);
					}
				}
			}
			foreach (NPC character2 in this.visitors)
			{
				if (!this.currentAssignments.ContainsKey(character2) && !this.TryAssignment(character2, this.towelLoungePoints, "Resort_Towel", character2.name.Value.ToLower() + "_beach_towel", animation_required: true, 0.5, last_activity_assignments) && !this.TryAssignment(character2, this.wanderPoints, "Resort_Wander", "square_3_3", animation_required: false, 0.4, last_activity_assignments) && !this.TryAssignment(character2, this.umbrellaPoints, "Resort_Umbrella", character2.name.Value.ToLower() + "_beach_umbrella", animation_required: true, (character2.Name == "Abigail") ? 0.5 : 0.1) && (character2.Age != 0 || !this.TryAssignment(character2, this.chairPoints, "Resort_Chair", "_beach_chair", animation_required: false, 0.4, last_activity_assignments)))
				{
					this.TryAssignment(character2, this.shoreLoungePoints, "Resort_Shore", null, animation_required: false, 1.0, last_activity_assignments);
				}
			}
			last_activity_assignments.Clear();
			foreach (Character key in this.currentAnimationAssignments.Keys)
			{
				last_activity_assignments[key] = this.currentAnimationAssignments[key];
			}
		}

		public bool TryAssignment(Character character, List<Point> points, string dialogue_key, string animation_name = null, bool animation_required = false, double chance = 1.0, Dictionary<Character, string> last_activity_assignments = null)
		{
			if (last_activity_assignments != null && !string.IsNullOrEmpty(animation_name) && !animation_name.StartsWith("square_") && last_activity_assignments.TryGetValue(character, out var assignment) && assignment == animation_name)
			{
				return false;
			}
			if (points.Count > 0 && (this.random.NextDouble() < chance || chance >= 1.0))
			{
				Point current_point = this.random.ChooseFrom(points);
				if (!string.IsNullOrEmpty(animation_name) && !animation_name.StartsWith("square_") && !this.animationDescriptions.ContainsKey(animation_name))
				{
					if (animation_required)
					{
						return false;
					}
					animation_name = null;
				}
				string assignment_string = (string.IsNullOrEmpty(animation_name) ? (current_point.X + " " + current_point.Y + " 2") : (current_point.X + " " + current_point.Y + " " + animation_name));
				if (dialogue_key != null)
				{
					dialogue_key = this.GetRandomDialogueKey("Characters\\Dialogue\\" + character.Name + ":" + dialogue_key, this.random);
					if (dialogue_key == null)
					{
						dialogue_key = this.GetRandomDialogueKey("Characters\\Dialogue\\" + character.Name + ":Resort", this.random);
					}
					if (dialogue_key != null)
					{
						assignment_string = assignment_string + " \"" + dialogue_key + "\"";
					}
				}
				this.currentAssignments[character] = assignment_string;
				points.Remove(current_point);
				this.currentAnimationAssignments[character] = animation_name;
				return true;
			}
			return false;
		}

		public string GetRandomDialogueKey(string dialogue_key, Random random)
		{
			if (Game1.content.LoadStringReturnNullIfNotFound(dialogue_key) != null)
			{
				bool fail = false;
				int count = 0;
				while (!fail)
				{
					count++;
					if (Game1.content.LoadStringReturnNullIfNotFound(dialogue_key + "_" + (count + 1)) == null)
					{
						fail = true;
					}
				}
				int index = random.Next(count) + 1;
				if (index == 1)
				{
					return dialogue_key;
				}
				return dialogue_key + "_" + index;
			}
			return null;
		}

		public string GetScheduleStringForCharacter(NPC character)
		{
			if (this.currentAssignments.TryGetValue(character, out var assignment))
			{
				return "/" + this.activityTime + " IslandSouth " + assignment;
			}
			return "";
		}
	}

	[XmlIgnore]
	protected int _boatDirection;

	[XmlIgnore]
	public Texture2D boatTexture;

	[XmlIgnore]
	public Vector2 boatPosition;

	[XmlIgnore]
	protected int _boatOffset;

	[XmlIgnore]
	protected float _nextBubble;

	[XmlIgnore]
	protected float _nextSlosh;

	[XmlIgnore]
	protected float _nextSmoke;

	[XmlIgnore]
	public LightSource boatLight;

	[XmlIgnore]
	public LightSource boatStringLight;

	[XmlElement("shouldToggleResort")]
	public readonly NetBool shouldToggleResort = new NetBool(value: false);

	[XmlElement("resortOpenToday")]
	public readonly NetBool resortOpenToday = new NetBool(value: true);

	[XmlElement("resortRestored")]
	public readonly NetBool resortRestored = new NetBool
	{
		InterpolationWait = false
	};

	[XmlElement("westernTurtleMoved")]
	public readonly NetBool westernTurtleMoved = new NetBool();

	[XmlIgnore]
	protected bool _parrotBoyHiding;

	[XmlIgnore]
	protected bool _isFirstVisit;

	[XmlIgnore]
	protected bool _exitsBlocked;

	[XmlIgnore]
	protected bool _sawFlameSprite;

	[XmlIgnore]
	public NetEvent0 moveTurtleEvent = new NetEvent0();

	private Microsoft.Xna.Framework.Rectangle turtle1Spot = new Microsoft.Xna.Framework.Rectangle(1088, 0, 192, 192);

	private Microsoft.Xna.Framework.Rectangle turtle2Spot = new Microsoft.Xna.Framework.Rectangle(0, 640, 256, 256);

	public IslandSouth()
	{
	}

	public IslandSouth(string map, string name)
		: base(map, name)
	{
		base.largeTerrainFeatures.Add(new Bush(new Vector2(31f, 5f), 4, this));
		base.parrotUpgradePerches.Add(new ParrotUpgradePerch(this, new Point(17, 22), new Microsoft.Xna.Framework.Rectangle(12, 18, 14, 7), 20, delegate
		{
			Game1.addMailForTomorrow("Island_Resort", noLetter: true, sendToEveryone: true);
			this.resortRestored.Value = true;
		}, () => this.resortRestored.Value, "Resort", "Island_UpgradeHouse"));
		base.parrotUpgradePerches.Add(new ParrotUpgradePerch(this, new Point(5, 9), new Microsoft.Xna.Framework.Rectangle(1, 10, 3, 4), 10, delegate
		{
			Game1.addMailForTomorrow("Island_Turtle", noLetter: true, sendToEveryone: true);
			this.westernTurtleMoved.Value = true;
			this.moveTurtleEvent.Fire();
		}, () => this.westernTurtleMoved.Value, "Turtle", "Island_FirstParrot"));
	}

	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(this.resortRestored, "resortRestored").AddField(this.westernTurtleMoved, "westernTurtleMoved").AddField(this.shouldToggleResort, "shouldToggleResort")
			.AddField(this.resortOpenToday, "resortOpenToday")
			.AddField(this.moveTurtleEvent, "moveTurtleEvent");
		this.resortRestored.fieldChangeEvent += delegate(NetBool f, bool oldValue, bool newValue)
		{
			if (newValue && base.mapPath.Value != null)
			{
				this.ApplyResortRestore();
			}
		};
		this.moveTurtleEvent.onEvent += ApplyWesternTurtleMove;
	}

	public override void TransferDataFromSavedLocation(GameLocation l)
	{
		if (l is IslandSouth location)
		{
			this.resortRestored.Value = location.resortRestored.Value;
			this.westernTurtleMoved.Value = location.westernTurtleMoved.Value;
			this.shouldToggleResort.Value = location.shouldToggleResort.Value;
			this.resortOpenToday.Value = location.resortOpenToday.Value;
		}
		base.TransferDataFromSavedLocation(l);
	}

	public override void DayUpdate(int dayOfMonth)
	{
		if (this.shouldToggleResort.Value)
		{
			this.resortOpenToday.Value = !this.resortOpenToday.Value;
			this.shouldToggleResort.Value = false;
			this.ApplyResortRestore();
		}
		base.DayUpdate(dayOfMonth);
	}

	public void ApplyResortRestore()
	{
		if (base.map != null)
		{
			base.ApplyUnsafeMapOverride("Island_Resort", null, new Microsoft.Xna.Framework.Rectangle(9, 15, 26, 16));
		}
		base.removeTile(new Location(41, 28), "Buildings");
		base.removeTile(new Location(42, 28), "Buildings");
		base.removeTile(new Location(42, 29), "Buildings");
		base.removeTile(new Location(42, 30), "Front");
		base.removeTileProperty(42, 30, "Back", "Passable");
		if (this.resortRestored.Value)
		{
			if (this.resortOpenToday.Value)
			{
				base.removeTile(new Location(22, 21), "Buildings");
				base.removeTile(new Location(22, 22), "Buildings");
				base.removeTile(new Location(24, 21), "Buildings");
				base.removeTile(new Location(24, 22), "Buildings");
			}
			else
			{
				base.setMapTile(22, 21, 1405, "Buildings", "untitled tile sheet");
				base.setMapTile(22, 22, 1437, "Buildings", "untitled tile sheet");
				base.setMapTile(24, 21, 1405, "Buildings", "untitled tile sheet");
				base.setMapTile(24, 22, 1437, "Buildings", "untitled tile sheet");
			}
		}
	}

	public void ApplyWesternTurtleMove()
	{
		TemporaryAnimatedSprite t = base.getTemporarySpriteByID(789);
		if (t != null)
		{
			t.motion = new Vector2(-2f, 0f);
			t.yPeriodic = true;
			t.yPeriodicRange = 8f;
			t.yPeriodicLoopTime = 300f;
			t.shakeIntensity = 1f;
		}
		base.localSound("shadowDie");
	}

	private void parrotBoyLands(int extra)
	{
		TemporaryAnimatedSprite v = base.getTemporarySpriteByID(888);
		if (v != null)
		{
			v.sourceRect.X = 0;
			v.sourceRect.Y = 32;
			v.sourceRectStartingPos.X = 0f;
			v.sourceRectStartingPos.Y = 32f;
			v.motion = new Vector2(4f, 0f);
			v.acceleration = Vector2.Zero;
			v.id = 888;
			v.animationLength = 4;
			v.interval = 100f;
			v.totalNumberOfLoops = 10;
			v.drawAboveAlwaysFront = false;
			v.layerDepth = 0.1f;
			base.temporarySprites.Add(v);
		}
	}

	public override void UpdateWhenCurrentLocation(GameTime time)
	{
		base.UpdateWhenCurrentLocation(time);
		this.moveTurtleEvent.Poll();
		if (this.boatLight != null)
		{
			this.boatLight.position.Value = new Vector2(3f, 1f) * 64f + this.GetBoatPosition();
		}
		if (this.boatStringLight != null)
		{
			this.boatStringLight.position.Value = new Vector2(3f, 4f) * 64f + this.GetBoatPosition();
		}
		if (this._parrotBoyHiding && Utility.isThereAFarmerWithinDistance(new Vector2(29f, 16f), 4, this) == Game1.player)
		{
			TemporaryAnimatedSprite v = base.getTemporarySpriteByID(777);
			if (v != null)
			{
				v.sourceRect.X = 0;
				v.sourceRectStartingPos.X = 0f;
				v.motion = new Vector2(3f, -10f);
				v.acceleration = new Vector2(0f, 0.4f);
				v.yStopCoordinate = 992;
				v.shakeIntensity = 2f;
				v.id = 888;
				v.reachedStopCoordinate = parrotBoyLands;
				base.localSound("parrot_squawk");
			}
		}
		if (!this._exitsBlocked && !this._sawFlameSprite && Utility.isThereAFarmerWithinDistance(new Vector2(18f, 11f), 5, this) == Game1.player)
		{
			Game1.addMailForTomorrow("Saw_Flame_Sprite_South", noLetter: true);
			TemporaryAnimatedSprite v2 = base.getTemporarySpriteByID(999);
			if (v2 != null)
			{
				v2.yPeriodic = false;
				v2.xPeriodic = false;
				v2.sourceRect.Y = 0;
				v2.sourceRectStartingPos.Y = 0f;
				v2.motion = new Vector2(0f, -4f);
				v2.acceleration = new Vector2(0f, -0.04f);
			}
			base.localSound("magma_sprite_spot");
			v2 = base.getTemporarySpriteByID(998);
			if (v2 != null)
			{
				v2.yPeriodic = false;
				v2.xPeriodic = false;
				v2.motion = new Vector2(0f, -4f);
				v2.acceleration = new Vector2(0f, -0.04f);
			}
			this._sawFlameSprite = true;
		}
		if (!(base.currentEvent?.id == "-157039427"))
		{
			return;
		}
		if (this._boatDirection != 0)
		{
			this._boatOffset += this._boatDirection;
			foreach (NPC actor in base.currentEvent.actors)
			{
				actor.shouldShadowBeOffset = true;
				actor.drawOffset.Y = this._boatOffset;
			}
			foreach (Farmer farmerActor in base.currentEvent.farmerActors)
			{
				farmerActor.shouldShadowBeOffset = true;
				farmerActor.drawOffset.Y = this._boatOffset;
			}
		}
		if ((float)this._boatDirection != 0f)
		{
			if (this._nextBubble > 0f)
			{
				this._nextBubble -= (float)time.ElapsedGameTime.TotalSeconds;
			}
			else
			{
				Microsoft.Xna.Framework.Rectangle back_rectangle = new Microsoft.Xna.Framework.Rectangle(64, 256, 192, 64);
				back_rectangle.X += (int)this.GetBoatPosition().X;
				back_rectangle.Y += (int)this.GetBoatPosition().Y;
				Vector2 position = Utility.getRandomPositionInThisRectangle(back_rectangle, Game1.random);
				TemporaryAnimatedSprite sprite = new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(0, 0, 64, 64), 50f, 9, 1, position, flicker: false, flipped: false, 0f, 0.025f, Color.White, 1f, 0f, 0f, 0f);
				sprite.acceleration = new Vector2(0f, -0.25f * (float)Math.Sign(this._boatDirection));
				base.temporarySprites.Add(sprite);
				this._nextBubble = 0.01f;
			}
			if (this._nextSlosh > 0f)
			{
				this._nextSlosh -= (float)time.ElapsedGameTime.TotalSeconds;
			}
			else
			{
				Game1.playSound("waterSlosh");
				this._nextSlosh = 0.5f;
			}
		}
		if (this._nextSmoke > 0f)
		{
			this._nextSmoke -= (float)time.ElapsedGameTime.TotalSeconds;
			return;
		}
		Vector2 position2 = new Vector2(2f, 2.5f) * 64f + this.GetBoatPosition();
		TemporaryAnimatedSprite sprite2 = new TemporaryAnimatedSprite("TileSheets\\animations", new Microsoft.Xna.Framework.Rectangle(0, 1600, 64, 128), 200f, 9, 1, position2, flicker: false, flipped: false, 1f, 0.025f, Color.White, 1f, 0.025f, 0f, 0f);
		sprite2.acceleration = new Vector2(-0.25f, -0.15f);
		base.temporarySprites.Add(sprite2);
		this._nextSmoke = 0.2f;
	}

	public override void cleanupBeforePlayerExit()
	{
		this.boatLight = null;
		this.boatStringLight = null;
		base.cleanupBeforePlayerExit();
	}

	public override bool isCollidingPosition(Microsoft.Xna.Framework.Rectangle position, xTile.Dimensions.Rectangle viewport, bool isFarmer, int damagesFarmer, bool glider, Character character)
	{
		if (this._exitsBlocked && position.Intersects(this.turtle1Spot))
		{
			return true;
		}
		if (!this.westernTurtleMoved.Value && position.Intersects(this.turtle2Spot))
		{
			return true;
		}
		return base.isCollidingPosition(position, viewport, isFarmer, damagesFarmer, glider, character);
	}

	public override bool isTilePlaceable(Vector2 tileLocation, bool itemIsPassable = false)
	{
		Point non_tile_position = Utility.Vector2ToPoint((tileLocation + new Vector2(0.5f, 0.5f)) * 64f);
		if (this._exitsBlocked && this.turtle1Spot.Contains(non_tile_position))
		{
			return false;
		}
		if (!this.westernTurtleMoved.Value && this.turtle2Spot.Contains(non_tile_position))
		{
			return false;
		}
		return base.isTilePlaceable(tileLocation, itemIsPassable);
	}

	public override void MakeMapModifications(bool force = false)
	{
		base.MakeMapModifications(force);
		if (this.resortRestored.Value)
		{
			this.ApplyResortRestore();
		}
	}

	protected override void resetLocalState()
	{
		this._isFirstVisit = false;
		if (!Game1.player.hasOrWillReceiveMail("Visited_Island"))
		{
			WorldMapManager.ReloadData();
			Game1.addMailForTomorrow("Visited_Island", noLetter: true);
			this._isFirstVisit = true;
		}
		Game1.getAchievement(40);
		if (Game1.player.hasOrWillReceiveMail("Saw_Flame_Sprite_South"))
		{
			this._sawFlameSprite = true;
		}
		this._exitsBlocked = !Game1.MasterPlayer.hasOrWillReceiveMail("Island_FirstParrot");
		this.boatLight = new LightSource("IslandSouth_BoatLight", 4, new Vector2(0f, 0f), 1f, LightSource.LightContext.None, 0L, base.NameOrUniqueName);
		this.boatStringLight = new LightSource("IslandSouth_BoatStringLight", 4, new Vector2(0f, 0f), 1f, LightSource.LightContext.None, 0L, base.NameOrUniqueName);
		Game1.currentLightSources.Add(this.boatLight);
		Game1.currentLightSources.Add(this.boatStringLight);
		base.resetLocalState();
		this.boatTexture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\WillysBoat");
		if (Game1.random.NextDouble() < 0.25 || this._isFirstVisit)
		{
			base.addCritter(new CrabCritter(new Vector2(37f, 30f) * 64f));
		}
		if (this._isFirstVisit)
		{
			base.addCritter(new CrabCritter(new Vector2(21f, 35f) * 64f));
			base.addCritter(new CrabCritter(new Vector2(21f, 36f) * 64f));
			base.addCritter(new CrabCritter(new Vector2(35f, 31f) * 64f));
			if (!Game1.MasterPlayer.hasOrWillReceiveMail("addedParrotBoy"))
			{
				this._parrotBoyHiding = true;
				base.temporarySprites.Add(new TemporaryAnimatedSprite("Characters\\ParrotBoy", new Microsoft.Xna.Framework.Rectangle(32, 128, 16, 32), new Vector2(29f, 15.5f) * 64f, flipped: false, 0f, Color.White)
				{
					id = 777,
					scale = 4f,
					totalNumberOfLoops = 99999,
					interval = 9999f,
					animationLength = 1,
					layerDepth = 1f,
					drawAboveAlwaysFront = true
				});
			}
		}
		if (this._exitsBlocked)
		{
			base.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Microsoft.Xna.Framework.Rectangle(208, 94, 48, 53), new Vector2(17f, 0f) * 64f, flipped: false, 0f, Color.White)
			{
				id = 555,
				scale = 4f,
				totalNumberOfLoops = 99999,
				interval = 9999f,
				animationLength = 1,
				layerDepth = 0.001f
			});
		}
		else if (!this._sawFlameSprite)
		{
			base.temporarySprites.Add(new TemporaryAnimatedSprite("Characters\\Monsters\\Magma Sprite", new Microsoft.Xna.Framework.Rectangle(0, 16, 16, 16), new Vector2(18f, 11f) * 64f, flipped: false, 0f, Color.White)
			{
				id = 999,
				scale = 4f,
				totalNumberOfLoops = 99999,
				interval = 70f,
				lightId = "IslandSouth_FlameSpirit",
				lightRadius = 1f,
				animationLength = 7,
				layerDepth = 1f,
				yPeriodic = true,
				yPeriodicRange = 12f,
				yPeriodicLoopTime = 1000f,
				xPeriodic = true,
				xPeriodicRange = 16f,
				xPeriodicLoopTime = 1800f
			});
			base.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\shadow", new Microsoft.Xna.Framework.Rectangle(0, 0, 12, 7), new Vector2(18.2f, 12.4f) * 64f, flipped: false, 0f, Color.White)
			{
				id = 998,
				scale = 4f,
				totalNumberOfLoops = 99999,
				interval = 1000f,
				animationLength = 1,
				layerDepth = 0.001f,
				yPeriodic = true,
				yPeriodicRange = 1f,
				yPeriodicLoopTime = 1000f,
				xPeriodic = true,
				xPeriodicRange = 16f,
				xPeriodicLoopTime = 1800f
			});
		}
		if (!this.westernTurtleMoved.Value)
		{
			base.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Microsoft.Xna.Framework.Rectangle(152, 101, 56, 40), new Vector2(0.5f, 10f) * 64f, flipped: false, 0f, Color.White)
			{
				id = 789,
				scale = 4f,
				totalNumberOfLoops = 99999,
				interval = 9999f,
				animationLength = 1,
				layerDepth = 0.001f
			});
		}
		if (base.AreMoonlightJelliesOut())
		{
			base.addMoonlightJellies(50, Utility.CreateRandom(Game1.stats.DaysPlayed, Game1.uniqueIDForThisGame, -24917.0), new Microsoft.Xna.Framework.Rectangle(0, 0, 0, 0));
		}
		this.ResetBoat();
	}

	public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
	{
		if (tileLocation.X == 14 && tileLocation.Y == 22)
		{
			Microsoft.Xna.Framework.Rectangle shopArea = new Microsoft.Xna.Framework.Rectangle(14, 21, 1, 1);
			if (Utility.TryOpenShopMenu("ResortBar", this, shopArea))
			{
				return true;
			}
		}
		return base.checkAction(tileLocation, viewport, who);
	}

	/// <summary>Get whether an NPC can visit the island resort today.</summary>
	/// <param name="npc">The NPC to check.</param>
	public static bool CanVisitIslandToday(NPC npc)
	{
		if (!npc.IsVillager || !npc.CanSocialize || npc.daysUntilNotInvisible > 0 || npc.IsInvisible)
		{
			return false;
		}
		if (!GameStateQuery.CheckConditions(npc.GetData()?.CanVisitIsland, npc.currentLocation))
		{
			return false;
		}
		if (npc.currentLocation?.NameOrUniqueName == "Farm")
		{
			return false;
		}
		if (Utility.IsHospitalVisitDay(npc.Name))
		{
			return false;
		}
		return true;
	}

	public override bool answerDialogueAction(string questionAndAnswer, string[] questionParams)
	{
		if (questionAndAnswer == null)
		{
			return false;
		}
		if (!(questionAndAnswer == "LeaveIsland_Yes"))
		{
			if (questionAndAnswer == "ToggleResort_Yes")
			{
				this.shouldToggleResort.Value = !this.shouldToggleResort.Value;
				bool open = this.resortOpenToday.Value;
				if (this.shouldToggleResort.Value)
				{
					open = !open;
				}
				if (open)
				{
					Game1.drawDialogueNoTyping(Game1.content.LoadString("Strings\\Locations:IslandSouth_ResortWillOpenSign"));
				}
				else
				{
					Game1.drawDialogueNoTyping(Game1.content.LoadString("Strings\\Locations:IslandSouth_ResortWillCloseSign"));
				}
				return true;
			}
			return base.answerDialogueAction(questionAndAnswer, questionParams);
		}
		this.Depart();
		return true;
	}

	/// <inheritdoc />
	public override bool performAction(string[] action, Farmer who, Location tileLocation)
	{
		if (ArgUtility.Get(action, 0) == "ResortSign")
		{
			string key = ((!this.resortOpenToday.Value) ? (this.shouldToggleResort.Value ? "Strings\\Locations:IslandSouth_ResortClosedWillOpenSign" : "Strings\\Locations:IslandSouth_ResortClosedSign") : (this.shouldToggleResort.Value ? "Strings\\Locations:IslandSouth_ResortOpenWillCloseSign" : "Strings\\Locations:IslandSouth_ResortOpenSign"));
			base.createQuestionDialogue(Game1.content.LoadString(key), base.createYesNoResponses(), "ToggleResort");
			return true;
		}
		return base.performAction(action, who, tileLocation);
	}

	/// <inheritdoc />
	public override void performTouchAction(string[] action, Vector2 playerStandingPosition)
	{
		if (!this.IgnoreTouchActions())
		{
			if (ArgUtility.Get(action, 0) == "LeaveIsland")
			{
				Response[] returnOptions = new Response[2]
				{
					new Response("Yes", Game1.content.LoadString("Strings\\Locations:Desert_Return_Yes")),
					new Response("Not", Game1.content.LoadString("Strings\\Locations:Desert_Return_No"))
				};
				base.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:Desert_Return_Question"), returnOptions, "LeaveIsland");
			}
			else
			{
				base.performTouchAction(action, playerStandingPosition);
			}
		}
	}

	public void Depart()
	{
		Game1.globalFadeToBlack(delegate
		{
			base.currentEvent = new Event(Game1.content.LoadString("Data\\Events\\IslandSouth:IslandDepart"), "Data\\Events\\IslandSouth", "-157039427", Game1.player);
			Game1.eventUp = true;
		});
	}

	public static Point GetDressingRoomPoint(NPC character)
	{
		if (character.Gender == Gender.Female)
		{
			return new Point(22, 19);
		}
		return new Point(24, 19);
	}

	public override bool HasLocationOverrideDialogue(NPC character)
	{
		if (Game1.player.friendshipData.TryGetValue(character.Name, out var friendship) && friendship.IsDivorced())
		{
			return false;
		}
		return character.islandScheduleName.Value != null;
	}

	public override string GetLocationOverrideDialogue(NPC character)
	{
		if (Game1.timeOfDay < 1200 || (!character.shouldWearIslandAttire.Value && Game1.timeOfDay < 1730 && IslandSouth.HasIslandAttire(character)))
		{
			string dialogue_key = "Characters\\Dialogue\\" + character.Name + ":Resort_Entering";
			if (Game1.content.LoadStringReturnNullIfNotFound(dialogue_key) != null)
			{
				return dialogue_key;
			}
		}
		if (Game1.timeOfDay >= 1800)
		{
			string dialogue_key2 = "Characters\\Dialogue\\" + character.Name + ":Resort_Leaving";
			if (Game1.content.LoadStringReturnNullIfNotFound(dialogue_key2) != null)
			{
				return dialogue_key2;
			}
		}
		return "Characters\\Dialogue\\" + character.Name + ":Resort";
	}

	public static bool HasIslandAttire(NPC character)
	{
		try
		{
			Game1.temporaryContent.Load<Texture2D>("Characters\\" + NPC.getTextureNameForCharacter(character.name.Value) + "_Beach");
			if (character?.Name == "Lewis")
			{
				foreach (Farmer farmer in Game1.getAllFarmers())
				{
					if (farmer?.activeDialogueEvents != null && farmer.activeDialogueEvents.ContainsKey("lucky_pants_lewis"))
					{
						return true;
					}
				}
				return false;
			}
			return true;
		}
		catch (Exception)
		{
		}
		return false;
	}

	public static void SetupIslandSchedules()
	{
		Game1.netWorldState.Value.IslandVisitors.Clear();
		if (Utility.isFestivalDay() || Utility.IsPassiveFestivalDay() || !(Game1.getLocationFromName("IslandSouth") is IslandSouth island) || !island.resortRestored.Value || island.IsRainingHere() || !island.resortOpenToday.Value)
		{
			return;
		}
		Random seeded_random = Utility.CreateRandom((double)Game1.uniqueIDForThisGame * 1.21, (double)Game1.stats.DaysPlayed * 2.5);
		List<NPC> valid_visitors = new List<NPC>();
		Utility.ForEachVillager(delegate(NPC npc)
		{
			if (IslandSouth.CanVisitIslandToday(npc))
			{
				valid_visitors.Add(npc);
			}
			return true;
		});
		List<NPC> visitors = new List<NPC>();
		if (seeded_random.NextDouble() < 0.4)
		{
			for (int i = 0; i < 5; i++)
			{
				NPC visitor = seeded_random.ChooseFrom(valid_visitors);
				if (visitor != null && visitor.age.Value != 2)
				{
					valid_visitors.Remove(visitor);
					visitors.Add(visitor);
					visitor.scheduleDelaySeconds = Math.Min((float)i * 0.6f, (float)Game1.realMilliSecondsPerGameTenMinutes / 1000f);
				}
			}
		}
		else
		{
			List<string>[] potentialGroups = new List<string>[12]
			{
				new List<string> { "Sebastian", "Sam", "Abigail" },
				new List<string> { "Jodi", "Kent", "Vincent", "Sam" },
				new List<string> { "Jodi", "Vincent", "Sam" },
				new List<string> { "Pierre", "Caroline", "Abigail" },
				new List<string> { "Robin", "Demetrius", "Maru", "Sebastian" },
				new List<string> { "Lewis", "Marnie" },
				new List<string> { "Marnie", "Shane", "Jas" },
				new List<string> { "Penny", "Jas", "Vincent" },
				new List<string> { "Pam", "Penny" },
				new List<string> { "Caroline", "Marnie", "Robin", "Jodi" },
				new List<string> { "Haley", "Penny", "Leah", "Emily", "Maru", "Abigail" },
				new List<string> { "Alex", "Sam", "Sebastian", "Elliott", "Shane", "Harvey" }
			};
			List<string> group = potentialGroups[seeded_random.Next(potentialGroups.Length)];
			bool failed = false;
			foreach (string s in group)
			{
				if (!valid_visitors.Contains(Game1.getCharacterFromName(s)))
				{
					failed = true;
					break;
				}
			}
			if (!failed)
			{
				int i2 = 0;
				foreach (string item in group)
				{
					NPC visitor2 = Game1.getCharacterFromName(item);
					valid_visitors.Remove(visitor2);
					visitors.Add(visitor2);
					visitor2.scheduleDelaySeconds = Math.Min((float)i2 * 0.6f, (float)Game1.realMilliSecondsPerGameTenMinutes / 1000f);
					i2++;
				}
			}
			for (int i3 = 0; i3 < 5 - visitors.Count; i3++)
			{
				NPC visitor3 = seeded_random.ChooseFrom(valid_visitors);
				if (visitor3 != null && visitor3.age.Value != 2)
				{
					valid_visitors.Remove(visitor3);
					visitors.Add(visitor3);
					visitor3.scheduleDelaySeconds = Math.Min((float)i3 * 0.6f, (float)Game1.realMilliSecondsPerGameTenMinutes / 1000f);
				}
			}
		}
		List<IslandActivityAssigments> activities = new List<IslandActivityAssigments>();
		Dictionary<Character, string> last_activity_assignments = new Dictionary<Character, string>();
		activities.Add(new IslandActivityAssigments(1200, visitors, seeded_random, last_activity_assignments));
		activities.Add(new IslandActivityAssigments(1400, visitors, seeded_random, last_activity_assignments));
		activities.Add(new IslandActivityAssigments(1600, visitors, seeded_random, last_activity_assignments));
		foreach (NPC visitor4 in visitors)
		{
			StringBuilder schedule = new StringBuilder("");
			bool should_dress = IslandSouth.HasIslandAttire(visitor4);
			bool had_first_activity = false;
			if (should_dress)
			{
				Point dressing_room = IslandSouth.GetDressingRoomPoint(visitor4);
				schedule.Append("/a1150 IslandSouth " + dressing_room.X + " " + dressing_room.Y + " change_beach");
				had_first_activity = true;
			}
			foreach (IslandActivityAssigments item2 in activities)
			{
				string current_string = item2.GetScheduleStringForCharacter(visitor4);
				if (current_string != "")
				{
					if (!had_first_activity)
					{
						current_string = "/a" + current_string.Substring(1);
						had_first_activity = true;
					}
					schedule.Append(current_string);
				}
			}
			if (should_dress)
			{
				Point dressing_room2 = IslandSouth.GetDressingRoomPoint(visitor4);
				schedule.Append("/a1730 IslandSouth " + dressing_room2.X + " " + dressing_room2.Y + " change_normal");
			}
			if (visitor4.Name == "Gus")
			{
				schedule.Append("/1800 Saloon 10 18 2/2430 bed");
			}
			else
			{
				schedule.Append("/1800 bed");
			}
			schedule.Remove(0, 1);
			if (visitor4.TryLoadSchedule("island", schedule.ToString()))
			{
				visitor4.islandScheduleName.Value = "island";
				Game1.netWorldState.Value.IslandVisitors.Add(visitor4.Name);
			}
			visitor4.performSpecialScheduleChanges();
		}
	}

	public virtual void ResetBoat()
	{
		this.boatPosition = new Vector2(14f, 37f) * 64f;
		this._boatOffset = 0;
		this._boatDirection = 0;
		this._nextBubble = 0f;
		this._nextSmoke = 0f;
		this._nextSlosh = 0f;
	}

	public Vector2 GetBoatPosition()
	{
		return this.boatPosition + new Vector2(0f, this._boatOffset);
	}

	public override void draw(SpriteBatch b)
	{
		base.draw(b);
		Vector2 boat_position = this.GetBoatPosition();
		b.Draw(this.boatTexture, Game1.GlobalToLocal(boat_position), new Microsoft.Xna.Framework.Rectangle(192, 0, 96, 208), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (this.boatPosition.Y + 320f) / 10000f);
		b.Draw(this.boatTexture, Game1.GlobalToLocal(boat_position), new Microsoft.Xna.Framework.Rectangle(288, 0, 96, 208), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (this.boatPosition.Y + 616f) / 10000f);
		if (base.currentEvent?.id != "-157039427")
		{
			b.Draw(this.boatTexture, Game1.GlobalToLocal(new Vector2(1184f, 2752f)), new Microsoft.Xna.Framework.Rectangle(192, 208, 32, 16), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.272f);
		}
	}

	public override bool RunLocationSpecificEventCommand(Event current_event, string command_string, bool first_run, params string[] args)
	{
		if (!(command_string == "boat_reset"))
		{
			if (command_string == "boat_depart")
			{
				this._boatDirection = 1;
				if (this._boatOffset >= 100)
				{
					return true;
				}
				return false;
			}
			return false;
		}
		this.ResetBoat();
		return true;
	}
}
