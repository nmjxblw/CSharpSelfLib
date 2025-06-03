using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.Extensions;
using StardewValley.GameData;
using StardewValley.GameData.Movies;
using StardewValley.Internal;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Locations;
using StardewValley.Menus;

namespace StardewValley.Minigames;

public class CraneGame : IMinigame
{
	public enum GameButtons
	{
		Action,
		Tool,
		Confirm,
		Cancel,
		Run,
		Up,
		Left,
		Down,
		Right,
		MAX
	}

	public class GameLogic : CraneGameObject
	{
		[XmlType("CraneGame.GameStates")]
		public enum GameStates
		{
			Setup,
			Idle,
			MoveClawRight,
			WaitForMoveDown,
			MoveClawDown,
			ClawDescend,
			ClawAscend,
			ClawReturn,
			ClawRelease,
			ClawReset,
			EndGame
		}

		public List<Item> collectedItems;

		public const int CLAW_HEIGHT = 50;

		protected Claw _claw;

		public int maxLives = 3;

		public int lives = 3;

		public Vector2 _startPosition = new Vector2(24f, 56f);

		public Vector2 _dropPosition = new Vector2(32f, 56f);

		public Rectangle playArea = new Rectangle(16, 48, 272, 64);

		public Rectangle prizeChute = new Rectangle(16, 48, 32, 32);

		protected GameStates _currentState;

		protected int _stateTimer;

		public CraneGameObject moveRightIndicator;

		public CraneGameObject moveDownIndicator;

		public CraneGameObject creditsDisplay;

		public CraneGameObject timeDisplay1;

		public CraneGameObject timeDisplay2;

		public CraneGameObject sunShockedFace;

		public int currentTimer;

		public CraneGameObject joystick;

		public int[] conveyerBeltTiles = new int[68]
		{
			0, 0, 0, 0, 7, 6, 6, 9, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 8, 0, 0, 2, 0, 0, 0, 7, 6,
			6, 6, 6, 9, 0, 0, 0, 0, 8, 0,
			0, 2, 0, 0, 0, 8, 0, 0, 0, 0,
			2, 0, 0, 0, 0, 1, 4, 4, 3, 0,
			0, 0, 1, 4, 4, 4, 4, 3
		};

		public int[] prizeMap = new int[68]
		{
			0, 0, 0, 0, 1, 0, 0, 1, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 0, 0, 0, 0, 1,
			0, 1, 0, 2, 0, 0, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 1, 0, 0, 0, 0,
			0, 0, 0, 0, 0, 1, 0, 0, 1, 0,
			0, 0, 0, 1, 0, 2, 0, 3
		};

		public GameLogic(CraneGame game)
			: base(game)
		{
			Game1.playSound("crane_game", out base._game.music);
			base._game.fastMusic = Game1.soundBank.GetCue("crane_game_fast");
			this._claw = new Claw(base._game);
			this._claw.position = this._startPosition;
			this._claw.zPosition = 50f;
			this.collectedItems = new List<Item>();
			this.SetState(GameStates.Setup);
			new Bush(base._game, 55, 2, 3, 31, 111);
			new Bush(base._game, 45, 2, 2, 112, 84);
			new Bush(base._game, 45, 2, 2, 63, 63);
			new Bush(base._game, 48, 1, 2, 56, 80);
			new Bush(base._game, 48, 1, 2, 72, 80);
			new Bush(base._game, 48, 1, 2, 56, 96);
			new Bush(base._game, 48, 1, 2, 72, 96);
			new Bush(base._game, 48, 1, 2, 56, 112);
			new Bush(base._game, 48, 1, 2, 72, 112);
			new Bush(base._game, 45, 2, 2, 159, 63);
			new Bush(base._game, 48, 1, 2, 152, 80);
			new Bush(base._game, 48, 1, 2, 168, 80);
			new Bush(base._game, 48, 1, 2, 152, 96);
			new Bush(base._game, 48, 1, 2, 168, 96);
			new Bush(base._game, 48, 1, 2, 152, 112);
			new Bush(base._game, 48, 1, 2, 168, 112);
			this.sunShockedFace = new CraneGameObject(base._game);
			this.sunShockedFace.SetSpriteFromIndex(9);
			this.sunShockedFace.position = new Vector2(96f, 0f);
			this.sunShockedFace.spriteAnchor = Vector2.Zero;
			CraneGameObject craneGameObject = new CraneGameObject(base._game);
			craneGameObject.position.X = 16f;
			craneGameObject.position.Y = 87f;
			craneGameObject.SetSpriteFromIndex(3);
			craneGameObject.spriteRect.Width = 32;
			craneGameObject.spriteAnchor = new Vector2(0f, 15f);
			this.joystick = new CraneGameObject(base._game);
			this.joystick.position.X = 151f;
			this.joystick.position.Y = 134f;
			this.joystick.SetSpriteFromIndex(28);
			this.joystick.spriteRect.Width = 32;
			this.joystick.spriteRect.Height = 48;
			this.joystick.spriteAnchor = new Vector2(15f, 47f);
			this.lives = this.maxLives;
			this.moveRightIndicator = new CraneGameObject(base._game);
			this.moveRightIndicator.position.X = 21f;
			this.moveRightIndicator.position.Y = 126f;
			this.moveRightIndicator.SetSpriteFromIndex(26);
			this.moveRightIndicator.spriteAnchor = Vector2.Zero;
			this.moveRightIndicator.visible = false;
			this.moveDownIndicator = new CraneGameObject(base._game);
			this.moveDownIndicator.position.X = 49f;
			this.moveDownIndicator.position.Y = 126f;
			this.moveDownIndicator.SetSpriteFromIndex(27);
			this.moveDownIndicator.spriteAnchor = Vector2.Zero;
			this.moveDownIndicator.visible = false;
			this.creditsDisplay = new CraneGameObject(base._game);
			this.creditsDisplay.SetSpriteFromIndex(70);
			this.creditsDisplay.position = new Vector2(234f, 125f);
			this.creditsDisplay.spriteAnchor = Vector2.Zero;
			this.timeDisplay1 = new CraneGameObject(base._game);
			this.timeDisplay1.SetSpriteFromIndex(70);
			this.timeDisplay1.position = new Vector2(274f, 125f);
			this.timeDisplay1.spriteAnchor = Vector2.Zero;
			this.timeDisplay2 = new CraneGameObject(base._game);
			this.timeDisplay2.SetSpriteFromIndex(70);
			this.timeDisplay2.position = new Vector2(285f, 125f);
			this.timeDisplay2.spriteAnchor = Vector2.Zero;
			int level_width = 17;
			for (int i = 0; i < this.conveyerBeltTiles.Length; i++)
			{
				if (this.conveyerBeltTiles[i] != 0)
				{
					int x = i % level_width + 1;
					int y = i / level_width + 3;
					switch (this.conveyerBeltTiles[i])
					{
					case 8:
						new ConveyerBelt(base._game, x, y, 0);
						break;
					case 4:
						new ConveyerBelt(base._game, x, y, 3);
						break;
					case 6:
						new ConveyerBelt(base._game, x, y, 1);
						break;
					case 2:
						new ConveyerBelt(base._game, x, y, 2);
						break;
					case 7:
						new ConveyerBelt(base._game, x, y, 1).SetSpriteFromCorner(240, 272);
						break;
					case 9:
						new ConveyerBelt(base._game, x, y, 2).SetSpriteFromCorner(240, 240);
						break;
					case 1:
						new ConveyerBelt(base._game, x, y, 0).SetSpriteFromCorner(240, 224);
						break;
					case 3:
						new ConveyerBelt(base._game, x, y, 3).SetSpriteFromCorner(240, 256);
						break;
					}
				}
			}
			Dictionary<int, List<Item>> possible_items = new Dictionary<int, List<Item>> { [1] = new List<Item>
			{
				ItemRegistry.Create("(F)1760"),
				ItemRegistry.Create("(F)1761"),
				ItemRegistry.Create("(F)1762"),
				ItemRegistry.Create("(F)1763"),
				ItemRegistry.Create("(F)1764"),
				ItemRegistry.Create("(F)1365")
			} };
			List<Item> item_list = new List<Item> { ItemRegistry.Create("(F)1669") };
			switch (Game1.season)
			{
			case Season.Spring:
				item_list.Add(ItemRegistry.Create("(F)1960"));
				break;
			case Season.Winter:
				item_list.Add(ItemRegistry.Create("(F)1961"));
				break;
			case Season.Summer:
				item_list.Add(ItemRegistry.Create("(F)1294"));
				break;
			case Season.Fall:
				item_list.Add(ItemRegistry.Create("(F)1918"));
				break;
			}
			item_list.Add(ItemRegistry.Create("(F)FancyHousePlant5"));
			item_list.Add(ItemRegistry.Create("(F)FancyHousePlant4"));
			item_list.Add(ItemRegistry.Create<Object>("(BC)2"));
			possible_items[2] = item_list;
			item_list = new List<Item>();
			switch (Game1.season)
			{
			case Season.Spring:
				item_list.Add(ItemRegistry.Create<Object>("(BC)107"));
				item_list.Add(ItemRegistry.Create<Object>("(BC)36"));
				item_list.Add(ItemRegistry.Create<Object>("(BC)48"));
				item_list.Add(ItemRegistry.Create<Object>("(BC)184"));
				item_list.Add(ItemRegistry.Create<Object>("(BC)188"));
				item_list.Add(ItemRegistry.Create<Object>("(BC)192"));
				item_list.Add(ItemRegistry.Create<Object>("(BC)204"));
				break;
			case Season.Winter:
				item_list.Add(ItemRegistry.Create("(F)1440"));
				item_list.Add(ItemRegistry.Create<Object>("(BC)44"));
				item_list.Add(ItemRegistry.Create<Object>("(BC)40"));
				item_list.Add(ItemRegistry.Create<Object>("(BC)41"));
				item_list.Add(ItemRegistry.Create<Object>("(BC)43"));
				item_list.Add(ItemRegistry.Create<Object>("(BC)42"));
				break;
			case Season.Summer:
				item_list.Add(ItemRegistry.Create("(F)985"));
				item_list.Add(ItemRegistry.Create("(F)984"));
				break;
			case Season.Fall:
				item_list.Add(ItemRegistry.Create("(F)1917"));
				item_list.Add(ItemRegistry.Create("(F)1307"));
				item_list.Add(ItemRegistry.Create<Object>("(BC)47"));
				item_list.Add(ItemRegistry.Create("(F)1471"));
				item_list.Add(ItemRegistry.Create("(F)1375"));
				break;
			}
			possible_items[3] = item_list;
			MovieData movieData = MovieTheater.GetMovieToday();
			MovieData movieData2 = movieData;
			if (movieData2 != null && movieData2.ClearDefaultCranePrizeGroups?.Count > 0)
			{
				foreach (int rarity in movieData.ClearDefaultCranePrizeGroups)
				{
					if (!possible_items.TryGetValue(rarity, out var itemList))
					{
						Game1.log.Warn($"Movie '{movieData.Id}' clears prize list for invalid rarity '{rarity}', expected one of '{string.Join("', '", possible_items.Keys.OrderBy((int p) => p))}'.");
					}
					else
					{
						itemList.Clear();
					}
				}
			}
			MovieData movieData3 = movieData;
			if (movieData3 != null && movieData3.CranePrizes?.Count > 0)
			{
				foreach (MovieCranePrizeData prize in movieData.CranePrizes)
				{
					if (prize.Condition != null && !GameStateQuery.CheckConditions(prize.Condition))
					{
						continue;
					}
					if (!possible_items.TryGetValue(prize.Rarity, out var itemList2))
					{
						Game1.log.Warn($"Movie '{movieData.Id}' has invalid rarity '{prize.Rarity}', expected one of '{string.Join("', '", possible_items.Keys.OrderBy((int p) => p))}'.");
						continue;
					}
					Item item = ItemQueryResolver.TryResolveRandomItem(prize, new ItemQueryContext(null, null, null, $"movie '{movieData.Id}' > crane prize '{prize.Id}'"), avoidRepeat: false, null, null, null, delegate(string query, string error)
					{
						Game1.log.Error($"Movie '{movieData.Id}' failed parsing item query '{query}' for crane prize '{prize.Id}': {error}");
					});
					if (item != null)
					{
						itemList2.Add(item);
					}
				}
			}
			for (int i2 = 0; i2 < this.prizeMap.Length; i2++)
			{
				if (this.prizeMap[i2] == 0)
				{
					continue;
				}
				int x2 = i2 % level_width + 1;
				int y2 = i2 / level_width + 3;
				Item item2 = null;
				int prize_rarity = i2;
				while (prize_rarity > 0 && item2 == null)
				{
					int index = this.prizeMap[i2];
					if ((uint)(index - 1) <= 2u)
					{
						item2 = Game1.random.ChooseFrom(possible_items[index]);
					}
					prize_rarity--;
				}
				new Prize(base._game, item2)
				{
					position = 
					{
						X = x2 * 16 + 8,
						Y = y2 * 16 + 8
					}
				};
			}
			if (Game1.random.NextDouble() < 0.1)
			{
				Item item3 = null;
				Vector2 prizePosition = new Vector2(0f, 4f);
				switch (Game1.random.Next(4))
				{
				case 0:
					item3 = ItemRegistry.Create("(O)107");
					break;
				case 1:
					item3 = ItemRegistry.Create("(O)749", 5);
					break;
				case 2:
					item3 = ItemRegistry.Create("(O)688", 5);
					break;
				case 3:
					item3 = ItemRegistry.Create("(O)288", 5);
					break;
				}
				new Prize(base._game, item3)
				{
					position = 
					{
						X = prizePosition.X * 16f + 30f,
						Y = prizePosition.Y * 16f + 32f
					}
				};
			}
			else if (Game1.random.NextDouble() < 0.2)
			{
				new Prize(base._game, ItemRegistry.Create("(O)809"))
				{
					position = 
					{
						X = 160f,
						Y = 58f
					}
				};
			}
			if (Game1.random.NextDouble() < 0.25)
			{
				new Prize(base._game, ItemRegistry.Create("(F)986"))
				{
					position = new Vector2(263f, 56f),
					zPosition = 0f
				};
				new Prize(base._game, ItemRegistry.Create("(F)986"))
				{
					position = new Vector2(215f, 56f),
					zPosition = 0f
				};
			}
			else
			{
				new Prize(base._game, ItemRegistry.Create("(F)989"))
				{
					position = new Vector2(263f, 56f),
					zPosition = 0f
				};
				new Prize(base._game, ItemRegistry.Create("(F)989"))
				{
					position = new Vector2(215f, 56f),
					zPosition = 0f
				};
			}
		}

		public GameStates GetCurrentState()
		{
			return this._currentState;
		}

		public override void Update(GameTime time)
		{
			float desired_joystick_rotation = 0f;
			foreach (Shadow shadow in base._game.GetObjectsOfType<Shadow>())
			{
				if (this.prizeChute.Contains(new Point((int)shadow.position.X, (int)shadow.position.Y)))
				{
					shadow.visible = false;
				}
				else
				{
					shadow.visible = true;
				}
			}
			int displayed_time = this.currentTimer / 60;
			if (this._currentState == GameStates.Setup)
			{
				this.creditsDisplay.SetSpriteFromIndex(70);
			}
			else
			{
				this.creditsDisplay.SetSpriteFromIndex(70 + this.lives);
			}
			this.timeDisplay1.SetSpriteFromIndex(70 + displayed_time / 10);
			this.timeDisplay2.SetSpriteFromIndex(70 + displayed_time % 10);
			if (this.currentTimer < 0)
			{
				this.timeDisplay1.SetSpriteFromIndex(80);
				this.timeDisplay2.SetSpriteFromIndex(81);
			}
			switch (this._currentState)
			{
			case GameStates.Setup:
			{
				if (!base._game.music.IsPlaying)
				{
					base._game.music.Play();
				}
				this._claw.openAngle = 40f;
				bool is_something_busy2 = false;
				foreach (Prize item2 in base._game.GetObjectsOfType<Prize>())
				{
					if (!item2.CanBeGrabbed())
					{
						is_something_busy2 = true;
						break;
					}
				}
				if (!is_something_busy2)
				{
					if (this._stateTimer >= 10)
					{
						this.SetState(GameStates.Idle);
					}
				}
				else
				{
					this._stateTimer = 0;
				}
				break;
			}
			case GameStates.Idle:
				if (!base._game.music.IsPlaying)
				{
					base._game.music.Play();
				}
				if (base._game.fastMusic.IsPlaying)
				{
					base._game.fastMusic.Stop(AudioStopOptions.Immediate);
					base._game.fastMusic = Game1.soundBank.GetCue("crane_game_fast");
				}
				this.currentTimer = 900;
				this.moveRightIndicator.visible = Game1.ticks / 20 % 2 == 0;
				if (base._game.IsButtonPressed(GameButtons.Tool) || base._game.IsButtonPressed(GameButtons.Action) || base._game.IsButtonPressed(GameButtons.Right))
				{
					Game1.playSound("bigSelect");
					this.SetState(GameStates.MoveClawRight);
				}
				break;
			case GameStates.MoveClawRight:
				desired_joystick_rotation = 15f;
				if (this._stateTimer < 15)
				{
					if (!base._game.IsButtonDown(GameButtons.Tool) && !base._game.IsButtonDown(GameButtons.Action) && !base._game.IsButtonDown(GameButtons.Right))
					{
						Game1.playSound("bigDeSelect");
						this.SetState(GameStates.Idle);
						return;
					}
					break;
				}
				if (base._game.craneSound == null || !base._game.craneSound.IsPlaying)
				{
					Game1.playSound("crane", out base._game.craneSound);
				}
				this.currentTimer--;
				if (this.currentTimer <= 0)
				{
					this.SetState(GameStates.ClawDescend);
					this.currentTimer = -1;
					if (base._game.craneSound != null && !base._game.craneSound.IsStopped)
					{
						base._game.craneSound.Stop(AudioStopOptions.Immediate);
					}
				}
				this.moveRightIndicator.visible = true;
				if (this._stateTimer <= 10)
				{
					break;
				}
				if (this._stateTimer == 11)
				{
					this._claw.ApplyDrawEffect(new ShakeEffect(1f, 1f));
					this._claw.ApplyDrawEffect(new SwayEffect(2f, 10f, 20));
					this._claw.ApplyDrawEffectToArms(new SwayEffect(15f, 4f, 50));
				}
				if (!base._game.IsButtonDown(GameButtons.Tool) && !base._game.IsButtonDown(GameButtons.Right) && !base._game.IsButtonDown(GameButtons.Action))
				{
					Game1.playSound("bigDeSelect");
					this._claw.ApplyDrawEffect(new SwayEffect(2f, 10f, 20));
					this._claw.ApplyDrawEffectToArms(new SwayEffect(15f, 4f, 100));
					this.SetState(GameStates.WaitForMoveDown);
					this.moveRightIndicator.visible = false;
					if (base._game.craneSound != null && !base._game.craneSound.IsStopped)
					{
						base._game.craneSound.Stop(AudioStopOptions.Immediate);
					}
				}
				else
				{
					this._claw.Move(0.5f, 0f);
					if (this._claw.GetBounds().Right >= this.playArea.Right)
					{
						this._claw.Move(-0.5f, 0f);
					}
				}
				break;
			case GameStates.WaitForMoveDown:
				this.currentTimer--;
				if (this.currentTimer <= 0)
				{
					this.SetState(GameStates.ClawDescend);
					this.currentTimer = -1;
				}
				this.moveDownIndicator.visible = Game1.ticks / 20 % 2 == 0;
				if (base._game.IsButtonPressed(GameButtons.Tool) || base._game.IsButtonPressed(GameButtons.Down) || base._game.IsButtonPressed(GameButtons.Action))
				{
					Game1.playSound("bigSelect");
					this.SetState(GameStates.MoveClawDown);
				}
				break;
			case GameStates.MoveClawDown:
				if (base._game.craneSound == null || !base._game.craneSound.IsPlaying)
				{
					Game1.playSound("crane", out base._game.craneSound);
				}
				this.currentTimer--;
				if (this.currentTimer <= 0)
				{
					this.SetState(GameStates.ClawDescend);
					this.currentTimer = -1;
					if (base._game.craneSound != null && !base._game.craneSound.IsStopped)
					{
						base._game.craneSound.Stop(AudioStopOptions.Immediate);
					}
				}
				desired_joystick_rotation = -5f;
				this.moveDownIndicator.visible = true;
				if (this._stateTimer <= 10)
				{
					break;
				}
				if (this._stateTimer == 11)
				{
					this._claw.ApplyDrawEffect(new ShakeEffect(1f, 1f));
					this._claw.ApplyDrawEffect(new SwayEffect(2f, 10f, 20));
					this._claw.ApplyDrawEffectToArms(new SwayEffect(15f, 4f, 50));
				}
				if (!base._game.IsButtonDown(GameButtons.Tool) && !base._game.IsButtonDown(GameButtons.Down) && !base._game.IsButtonDown(GameButtons.Action))
				{
					Game1.playSound("bigDeSelect");
					this._claw.ApplyDrawEffect(new SwayEffect(2f, 10f, 20));
					this._claw.ApplyDrawEffectToArms(new SwayEffect(15f, 4f, 100));
					this.moveDownIndicator.visible = false;
					this.SetState(GameStates.ClawDescend);
					if (base._game.craneSound != null && !base._game.craneSound.IsStopped)
					{
						base._game.craneSound.Stop(AudioStopOptions.Immediate);
					}
				}
				else
				{
					this._claw.Move(0f, 0.5f);
					if (this._claw.GetBounds().Bottom >= this.playArea.Bottom)
					{
						this._claw.Move(0f, -0.5f);
					}
				}
				break;
			case GameStates.ClawDescend:
				if (this._claw.openAngle < 40f)
				{
					this._claw.openAngle += 1.5f;
					this._stateTimer = 0;
				}
				else
				{
					if (this._stateTimer <= 30)
					{
						break;
					}
					if (base._game.craneSound != null && base._game.craneSound.IsPlaying)
					{
						Game1.sounds.SetPitch(base._game.craneSound, 2000f);
					}
					else
					{
						Game1.playSound("crane", 2000, out base._game.craneSound);
					}
					if (!(this._claw.zPosition > 0f))
					{
						break;
					}
					this._claw.zPosition -= 0.5f;
					if (this._claw.zPosition <= 0f)
					{
						this._claw.zPosition = 0f;
						this.SetState(GameStates.ClawAscend);
						if (base._game.craneSound != null && !base._game.craneSound.IsStopped)
						{
							base._game.craneSound.Stop(AudioStopOptions.Immediate);
						}
					}
				}
				break;
			case GameStates.ClawAscend:
				if (this._claw.openAngle > 0f && this._claw.GetGrabbedPrize() == null)
				{
					this._claw.openAngle -= 1f;
					if (this._claw.openAngle == 15f)
					{
						this._claw.GrabObject();
						if (this._claw.GetGrabbedPrize() != null)
						{
							Game1.playSound("FishHit");
							this.sunShockedFace.ApplyDrawEffect(new ShakeEffect(1f, 1f, 5));
							base._game.freezeFrames = 60;
							if (base._game.music.IsPlaying)
							{
								base._game.music.Stop(AudioStopOptions.Immediate);
								base._game.music = Game1.soundBank.GetCue("crane_game");
							}
						}
					}
					else if (this._claw.openAngle == 0f && this._claw.GetGrabbedPrize() == null)
					{
						if (this.lives == 1)
						{
							base._game.music.Stop(AudioStopOptions.Immediate);
							Game1.playSound("fishEscape");
						}
						else
						{
							Game1.playSound("stoneStep");
						}
					}
					this._stateTimer = 0;
					break;
				}
				if (this._claw.GetGrabbedPrize() != null)
				{
					if (!base._game.fastMusic.IsPlaying)
					{
						base._game.fastMusic.Play();
					}
				}
				else if (base._game.fastMusic.IsPlaying)
				{
					base._game.fastMusic.Stop(AudioStopOptions.AsAuthored);
					base._game.fastMusic = Game1.soundBank.GetCue("crane_game_fast");
				}
				if (this._claw.zPosition < 50f)
				{
					this._claw.zPosition += 0.5f;
					if (this._claw.zPosition >= 50f)
					{
						this._claw.zPosition = 50f;
						this.SetState(GameStates.ClawReturn);
						if (this._claw.GetGrabbedPrize() == null && this.lives == 1)
						{
							this.SetState(GameStates.EndGame);
						}
					}
				}
				this._claw.CheckDropPrize();
				break;
			case GameStates.ClawReturn:
				if (this._claw.GetGrabbedPrize() != null)
				{
					if (!base._game.fastMusic.IsPlaying)
					{
						base._game.fastMusic.Play();
					}
				}
				else if (base._game.fastMusic.IsPlaying)
				{
					base._game.fastMusic.Stop(AudioStopOptions.AsAuthored);
					base._game.fastMusic = Game1.soundBank.GetCue("crane_game_fast");
				}
				if (this._stateTimer > 10)
				{
					if (this._claw.position.Equals(this._dropPosition))
					{
						this.SetState(GameStates.ClawRelease);
					}
					else
					{
						float move_speed2 = 0.5f;
						if (this._claw.GetGrabbedPrize() == null)
						{
							move_speed2 = 0.75f;
						}
						if (this._claw.position.X != this._dropPosition.X)
						{
							this._claw.position.X = Utility.MoveTowards(this._claw.position.X, this._dropPosition.X, move_speed2);
						}
						if (this._claw.position.X != this._dropPosition.Y)
						{
							this._claw.position.Y = Utility.MoveTowards(this._claw.position.Y, this._dropPosition.Y, move_speed2);
						}
					}
				}
				this._claw.CheckDropPrize();
				break;
			case GameStates.ClawRelease:
			{
				bool clawHadPrize = this._claw.GetGrabbedPrize() != null;
				if (this._stateTimer <= 10)
				{
					break;
				}
				this._claw.ReleaseGrabbedObject();
				if (this._claw.openAngle < 40f)
				{
					this._claw.openAngle++;
					break;
				}
				this.SetState(GameStates.ClawReset);
				if (!clawHadPrize)
				{
					Game1.playSound("button1");
					this._claw.ApplyDrawEffect(new ShakeEffect(1f, 1f));
				}
				break;
			}
			case GameStates.ClawReset:
			{
				if (this._stateTimer <= 50)
				{
					break;
				}
				if (this._claw.position.Equals(this._startPosition))
				{
					this.lives--;
					if (this.lives <= 0)
					{
						this.SetState(GameStates.EndGame);
					}
					else
					{
						this.SetState(GameStates.Idle);
					}
					break;
				}
				float move_speed = 0.5f;
				if (this._claw.position.X != this._startPosition.X)
				{
					this._claw.position.X = Utility.MoveTowards(this._claw.position.X, this._startPosition.X, move_speed);
				}
				if (this._claw.position.X != this._startPosition.Y)
				{
					this._claw.position.Y = Utility.MoveTowards(this._claw.position.Y, this._startPosition.Y, move_speed);
				}
				break;
			}
			case GameStates.EndGame:
			{
				if (base._game.music.IsPlaying)
				{
					base._game.music.Stop(AudioStopOptions.Immediate);
				}
				if (base._game.fastMusic.IsPlaying)
				{
					base._game.fastMusic.Stop(AudioStopOptions.Immediate);
				}
				bool is_something_busy = false;
				foreach (Prize item3 in base._game.GetObjectsOfType<Prize>())
				{
					if (!item3.CanBeGrabbed())
					{
						is_something_busy = true;
						break;
					}
				}
				if (is_something_busy || this._stateTimer < 20)
				{
					break;
				}
				if (this.collectedItems.Count > 0)
				{
					List<Item> items = new List<Item>();
					foreach (Item item in this.collectedItems)
					{
						items.Add(item.getOne());
					}
					Game1.activeClickableMenu = new ItemGrabMenu(items, reverseGrab: false, showReceivingMenu: true, null, null, "Rewards", null, snapToBottom: false, canBeExitedWithKey: false, playRightClickSound: false, allowRightClick: false, showOrganizeButton: false, 0, null, -1, base._game);
				}
				base._game.Quit();
				break;
			}
			}
			this.sunShockedFace.visible = this._claw.GetGrabbedPrize() != null;
			this.joystick.rotation = Utility.MoveTowards(this.joystick.rotation, desired_joystick_rotation, 2f);
			this._stateTimer++;
		}

		public override void Draw(SpriteBatch b, float layer_depth)
		{
		}

		public void SetState(GameStates new_state)
		{
			this._currentState = new_state;
			this._stateTimer = 0;
		}
	}

	public class Trampoline : CraneGameObject
	{
		public Trampoline(CraneGame game, int x, int y)
			: base(game)
		{
			base.SetSpriteFromIndex(30);
			base.spriteRect.Width = 32;
			base.spriteRect.Height = 32;
			base.spriteAnchor.X = 15f;
			base.spriteAnchor.Y = 15f;
			base.position.X = x;
			base.position.Y = y;
		}
	}

	public class Shadow : CraneGameObject
	{
		public CraneGameObject _target;

		public Shadow(CraneGame game, CraneGameObject target)
			: base(game)
		{
			base.SetSpriteFromIndex(2);
			base.layerDepth = 900f;
			this._target = target;
		}

		public override void Update(GameTime time)
		{
			if (this._target != null)
			{
				base.position = this._target.position;
			}
			if (this._target is Prize { grabbed: not false })
			{
				base.visible = false;
			}
			if (this._target.IsDestroyed())
			{
				this.Destroy();
				return;
			}
			base.color.A = (byte)(Math.Min(1f, this._target.zPosition / 50f) * 255f);
			base.scale = Utility.Lerp(1f, 0.5f, Math.Min(this._target.zPosition / 100f, 1f)) * new Vector2(1f, 1f);
		}
	}

	public class Claw : CraneGameObject
	{
		protected CraneGameObject _leftArm;

		protected CraneGameObject _rightArm;

		protected Prize _grabbedPrize;

		protected Vector2 _prizePositionOffset;

		protected int _nextDropCheckTimer;

		protected int _dropChances;

		protected int _grabTime;

		public float openAngle
		{
			get
			{
				return this._leftArm.rotation;
			}
			set
			{
				this._leftArm.rotation = value;
			}
		}

		public Claw(CraneGame game)
			: base(game)
		{
			base.SetSpriteFromIndex();
			base.spriteAnchor = new Vector2(8f, 24f);
			this._leftArm = new CraneGameObject(game);
			this._leftArm.SetSpriteFromIndex(1);
			this._leftArm.spriteAnchor = new Vector2(16f, 0f);
			this._rightArm = new CraneGameObject(game);
			this._rightArm.SetSpriteFromIndex(1);
			this._rightArm.flipX = true;
			this._rightArm.spriteAnchor = new Vector2(0f, 0f);
			new Shadow(base._game, this);
		}

		public void CheckDropPrize()
		{
			if (this._grabbedPrize == null)
			{
				return;
			}
			this._nextDropCheckTimer--;
			if (this._nextDropCheckTimer > 0)
			{
				return;
			}
			float drop_chance = this._prizePositionOffset.Length() * 0.1f;
			drop_chance += base.zPosition * 0.001f;
			if (this._grabbedPrize.isLargeItem)
			{
				drop_chance += 0.1f;
			}
			double roll = Game1.random.NextDouble();
			if (roll < (double)drop_chance)
			{
				this._dropChances--;
				if (this._dropChances <= 0)
				{
					Game1.playSound("fishEscape");
					this.ReleaseGrabbedObject();
				}
				else
				{
					Game1.playSound("bob");
					this._grabbedPrize.ApplyDrawEffect(new ShakeEffect(2f, 2f, 50));
					this._grabbedPrize.rotation += (float)Game1.random.NextDouble() * 10f;
				}
			}
			else if (roll < (double)drop_chance)
			{
				Game1.playSound("dwop");
				this._grabbedPrize.ApplyDrawEffect(new ShakeEffect(1f, 1f, 50));
			}
			this._nextDropCheckTimer = Game1.random.Next(50, 100);
		}

		public void ApplyDrawEffectToArms(DrawEffect new_effect)
		{
			this._leftArm.ApplyDrawEffect(new_effect);
			this._rightArm.ApplyDrawEffect(new_effect);
		}

		public void ReleaseGrabbedObject()
		{
			if (this._grabbedPrize != null)
			{
				this._grabbedPrize.grabbed = false;
				this._grabbedPrize.OnDrop();
				this._grabbedPrize = null;
			}
		}

		public void GrabObject()
		{
			Prize closest_prize = null;
			float closest_distance = 0f;
			foreach (Prize prize in base._game.GetObjectsAtPoint<Prize>(base.position))
			{
				if (!prize.IsDestroyed() && prize.CanBeGrabbed())
				{
					float distance = (base.position - prize.position).LengthSquared();
					if (closest_prize == null || distance < closest_distance)
					{
						closest_distance = distance;
						closest_prize = prize;
					}
				}
			}
			if (closest_prize != null)
			{
				this._grabbedPrize = closest_prize;
				this._grabbedPrize.grabbed = true;
				this._prizePositionOffset = this._grabbedPrize.position - base.position;
				this._nextDropCheckTimer = Game1.random.Next(50, 100);
				this._dropChances = 3;
				Game1.playSound("pickUpItem");
				this._grabTime = 0;
				this._grabbedPrize.ApplyDrawEffect(new StretchEffect(0.95f, 1.1f));
				this._grabbedPrize.ApplyDrawEffect(new ShakeEffect(1f, 1f, 20));
			}
		}

		public Prize GetGrabbedPrize()
		{
			return this._grabbedPrize;
		}

		public override void Update(GameTime time)
		{
			this._leftArm.position = base.position + new Vector2(0f, -16f);
			this._rightArm.position = base.position + new Vector2(0f, -16f);
			this._rightArm.rotation = 0f - this._leftArm.rotation;
			this._leftArm.layerDepth = (this._rightArm.layerDepth = base.GetRendererLayerDepth() + 0.01f);
			this._leftArm.zPosition = (this._rightArm.zPosition = base.zPosition);
			if (this._grabbedPrize != null)
			{
				this._grabbedPrize.position = base.position + this._prizePositionOffset * Utility.Lerp(1f, 0.25f, Math.Min(1f, (float)this._grabTime / 200f));
				this._grabbedPrize.zPosition = base.zPosition + this._grabbedPrize.GetRestingZPosition();
			}
			this._grabTime++;
		}

		public override void Destroy()
		{
			this._leftArm.Destroy();
			this._rightArm.Destroy();
			base.Destroy();
		}
	}

	public class ConveyerBelt : CraneGameObject
	{
		protected int _direction;

		protected Vector2 _spriteStartPosition;

		protected int _spriteOffset;

		public int GetDirection()
		{
			return this._direction;
		}

		public ConveyerBelt(CraneGame game, int x, int y, int direction)
			: base(game)
		{
			base.position.X = x * 16;
			base.position.Y = y * 16;
			this._direction = direction;
			base.spriteAnchor = Vector2.Zero;
			base.layerDepth = 1000f;
			switch (this._direction)
			{
			case 0:
				base.SetSpriteFromIndex(5);
				break;
			case 2:
				base.SetSpriteFromIndex(10);
				break;
			case 3:
				base.SetSpriteFromIndex(15);
				break;
			case 1:
				base.SetSpriteFromIndex(20);
				break;
			}
			this._spriteStartPosition = new Vector2(base.spriteRect.X, base.spriteRect.Y);
		}

		public void SetSpriteFromCorner(int x, int y)
		{
			base.spriteRect.X = x;
			base.spriteRect.Y = y;
			this._spriteStartPosition = new Vector2(base.spriteRect.X, base.spriteRect.Y);
		}

		public override void Update(GameTime time)
		{
			int ticks_per_frame = 4;
			int frame_count = 4;
			base.spriteRect.X = (int)this._spriteStartPosition.X + this._spriteOffset / ticks_per_frame * 16;
			this._spriteOffset++;
			if (this._spriteOffset >= (frame_count - 1) * ticks_per_frame)
			{
				this._spriteOffset = 0;
			}
		}
	}

	public class Bush : CraneGameObject
	{
		public Bush(CraneGame game, int tile_index, int tile_width, int tile_height, int x, int y)
			: base(game)
		{
			base.SetSpriteFromIndex(tile_index);
			base.spriteRect.Width = tile_width * 16;
			base.spriteRect.Height = tile_height * 16;
			base.spriteAnchor.X = (float)base.spriteRect.Width / 2f;
			base.spriteAnchor.Y = base.spriteRect.Height;
			if (tile_height > 16)
			{
				base.spriteAnchor.Y -= 8f;
			}
			else
			{
				base.spriteAnchor.Y -= 4f;
			}
			base.position.X = x;
			base.position.Y = y;
		}

		public override void Update(GameTime time)
		{
			base.rotation = (float)Math.Sin(time.TotalGameTime.TotalMilliseconds * 0.0024999999441206455 + (double)base.position.Y + (double)(base.position.X * 2f)) * 2f;
		}
	}

	public class Prize : CraneGameObject
	{
		protected Vector2 _conveyerBeltMove;

		public bool grabbed;

		public float gravity;

		protected Vector2 _velocity = Vector2.Zero;

		protected Item _item;

		protected float _restingZPosition;

		protected float _angularSpeed;

		protected bool _isBeingCollected;

		public bool isLargeItem;

		public float GetRestingZPosition()
		{
			return this._restingZPosition;
		}

		public Prize(CraneGame game, Item item)
			: base(game)
		{
			base.SetSpriteFromIndex(3);
			base.spriteAnchor = new Vector2(8f, 12f);
			this._item = item;
			this._UpdateItemSprite();
			new Shadow(base._game, this);
		}

		public void OnDrop()
		{
			if (!this.isLargeItem)
			{
				this._angularSpeed = Utility.Lerp(-5f, 5f, (float)Game1.random.NextDouble());
			}
			else
			{
				base.rotation = 0f;
			}
		}

		public void _UpdateItemSprite()
		{
			ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(this._item.QualifiedItemId);
			base.texture = itemData.GetTexture();
			base.spriteRect = itemData.GetSourceRect();
			base.width = base.spriteRect.Width;
			base.height = base.spriteRect.Height;
			if (base.width > 16 || base.height > 16)
			{
				this.isLargeItem = true;
			}
			else
			{
				this.isLargeItem = false;
			}
			if (base.height <= 16)
			{
				base.spriteAnchor = new Vector2(base.width / 2, (float)base.height - 4f);
			}
			else
			{
				base.spriteAnchor = new Vector2(base.width / 2, (float)base.height - 8f);
			}
			this._restingZPosition = 0f;
		}

		public bool CanBeGrabbed()
		{
			if (base.IsDestroyed())
			{
				return false;
			}
			if (this._isBeingCollected)
			{
				return false;
			}
			if (base.zPosition != this._restingZPosition)
			{
				return false;
			}
			return true;
		}

		public override void Update(GameTime time)
		{
			if (this._isBeingCollected)
			{
				Vector4 color_vector = base.color.ToVector4();
				color_vector.X = Utility.MoveTowards(color_vector.X, 0f, 0.05f);
				color_vector.Y = Utility.MoveTowards(color_vector.Y, 0f, 0.05f);
				color_vector.Z = Utility.MoveTowards(color_vector.Z, 0f, 0.05f);
				color_vector.W = Utility.MoveTowards(color_vector.W, 0f, 0.05f);
				base.color = new Color(color_vector);
				base.scale.X = Utility.MoveTowards(base.scale.X, 0.5f, 0.05f);
				base.scale.Y = Utility.MoveTowards(base.scale.Y, 0.5f, 0.05f);
				if (color_vector.W == 0f)
				{
					Game1.playSound("Ship");
					this.Destroy();
				}
				base.position.Y += 0.5f;
			}
			else
			{
				if (this.grabbed)
				{
					return;
				}
				if (this._velocity.X != 0f || this._velocity.Y != 0f)
				{
					base.position.X += this._velocity.X;
					if (!base._game.GetObjectsOfType<GameLogic>()[0].playArea.Contains(new Point((int)base.position.X, (int)base.position.Y)))
					{
						base.position.X -= this._velocity.X;
						this._velocity.X *= -1f;
					}
					base.position.Y += this._velocity.Y;
					if (!base._game.GetObjectsOfType<GameLogic>()[0].playArea.Contains(new Point((int)base.position.X, (int)base.position.Y)))
					{
						base.position.Y -= this._velocity.Y;
						this._velocity.Y *= -1f;
					}
				}
				if (base.zPosition < this._restingZPosition)
				{
					base.zPosition = this._restingZPosition;
				}
				if (base.zPosition > this._restingZPosition || this._velocity != Vector2.Zero || this.gravity != 0f)
				{
					if (!this.isLargeItem)
					{
						base.rotation += this._angularSpeed;
					}
					this._conveyerBeltMove = Vector2.Zero;
					if (base.zPosition > this._restingZPosition)
					{
						this.gravity += 0.1f;
					}
					base.zPosition -= this.gravity;
					if (!(base.zPosition < this._restingZPosition))
					{
						return;
					}
					base.zPosition = this._restingZPosition;
					if (!(this.gravity >= 0f))
					{
						return;
					}
					if (!this.isLargeItem)
					{
						this._angularSpeed = Utility.Lerp(-10f, 10f, (float)Game1.random.NextDouble());
					}
					this.gravity = (0f - this.gravity) * 0.6f;
					if (base._game.GetObjectsOfType<GameLogic>()[0].prizeChute.Contains(new Point((int)base.position.X, (int)base.position.Y)))
					{
						if (base._game.GetObjectsOfType<GameLogic>()[0].GetCurrentState() != GameLogic.GameStates.Setup)
						{
							Game1.playSound("reward");
							this._isBeingCollected = true;
							base._game.GetObjectsOfType<GameLogic>()[0].collectedItems.Add(this._item);
						}
						else
						{
							this.gravity = -2.5f;
							Vector2 offset = new Vector2(base._game.GetObjectsOfType<GameLogic>()[0].playArea.Center.X, base._game.GetObjectsOfType<GameLogic>()[0].playArea.Center.Y) - new Vector2(base.position.X, base.position.Y);
							offset.Normalize();
							this._velocity = offset * Utility.Lerp(1f, 2f, (float)Game1.random.NextDouble());
						}
						return;
					}
					if (base._game.GetOverlaps<Trampoline>(this, 1).Count > 0)
					{
						Trampoline trampoline = base._game.GetOverlaps<Trampoline>(this, 1)[0];
						Game1.playSound("axchop");
						trampoline.ApplyDrawEffect(new StretchEffect(0.75f, 0.75f, 5));
						trampoline.ApplyDrawEffect(new ShakeEffect(2f, 2f));
						base.ApplyDrawEffect(new ShakeEffect(2f, 2f));
						this.gravity = -2.5f;
						Vector2 offset2 = new Vector2(base._game.GetObjectsOfType<GameLogic>()[0].playArea.Center.X, base._game.GetObjectsOfType<GameLogic>()[0].playArea.Center.Y) - new Vector2(base.position.X, base.position.Y);
						offset2.Normalize();
						this._velocity = offset2 * Utility.Lerp(0.5f, 1f, (float)Game1.random.NextDouble());
						return;
					}
					if (Math.Abs(this.gravity) < 1.5f)
					{
						base.rotation = 0f;
						this._velocity = Vector2.Zero;
						this.gravity = 0f;
						return;
					}
					bool bumped_object = false;
					foreach (Prize prize in base._game.GetOverlaps<Prize>(this))
					{
						if (prize.gravity == 0f && prize.CanBeGrabbed())
						{
							Vector2 offset3 = base.position - prize.position;
							offset3.Normalize();
							this._velocity = offset3 * Utility.Lerp(0.25f, 1f, (float)Game1.random.NextDouble());
							if (!prize.isLargeItem || this.isLargeItem)
							{
								prize._velocity = -offset3 * Utility.Lerp(0.75f, 1.5f, (float)Game1.random.NextDouble());
								prize.gravity = this.gravity * 0.75f;
								prize.ApplyDrawEffect(new ShakeEffect(2f, 2f, 20));
							}
							bumped_object = true;
						}
					}
					base.ApplyDrawEffect(new ShakeEffect(2f, 2f, 20));
					if (!bumped_object)
					{
						float rad_angle = Utility.Lerp(0f, (float)Math.PI * 2f, (float)Game1.random.NextDouble());
						this._velocity = new Vector2((float)Math.Sin(rad_angle), (float)Math.Cos(rad_angle)) * Utility.Lerp(0.5f, 1f, (float)Game1.random.NextDouble());
					}
				}
				else if (this._conveyerBeltMove.X == 0f && this._conveyerBeltMove.Y == 0f)
				{
					List<ConveyerBelt> belts = base._game.GetObjectsAtPoint<ConveyerBelt>(base.position, 1);
					if (belts.Count > 0)
					{
						switch (belts[0].GetDirection())
						{
						case 0:
							this._conveyerBeltMove = new Vector2(0f, -16f);
							break;
						case 2:
							this._conveyerBeltMove = new Vector2(0f, 16f);
							break;
						case 3:
							this._conveyerBeltMove = new Vector2(-16f, 0f);
							break;
						case 1:
							this._conveyerBeltMove = new Vector2(16f, 0f);
							break;
						}
					}
				}
				else
				{
					float move_speed = 0.3f;
					if (this._conveyerBeltMove.X != 0f)
					{
						this.Move(move_speed * (float)Math.Sign(this._conveyerBeltMove.X), 0f);
						this._conveyerBeltMove.X = Utility.MoveTowards(this._conveyerBeltMove.X, 0f, move_speed);
					}
					if (this._conveyerBeltMove.Y != 0f)
					{
						this.Move(0f, move_speed * (float)Math.Sign(this._conveyerBeltMove.Y));
						this._conveyerBeltMove.Y = Utility.MoveTowards(this._conveyerBeltMove.Y, 0f, move_speed);
					}
				}
			}
		}
	}

	public class CraneGameObject
	{
		protected CraneGame _game;

		public Vector2 position = Vector2.Zero;

		public float rotation;

		public Vector2 scale = new Vector2(1f, 1f);

		public bool flipX;

		public bool flipY;

		public Rectangle spriteRect;

		public Texture2D texture;

		public Vector2 spriteAnchor;

		public Color color = Color.White;

		public float layerDepth = -1f;

		public int width = 16;

		public int height = 16;

		public float zPosition;

		public bool visible = true;

		public List<DrawEffect> drawEffects;

		protected bool _destroyed;

		public CraneGameObject(CraneGame game)
		{
			this._game = game;
			this.texture = this._game.spriteSheet;
			this.spriteRect = new Rectangle(0, 0, 16, 16);
			this.spriteAnchor = new Vector2(8f, 8f);
			this.drawEffects = new List<DrawEffect>();
			this._game.RegisterGameObject(this);
		}

		public void SetSpriteFromIndex(int index = 0)
		{
			this.spriteRect.X = 304 + index % 5 * 16;
			this.spriteRect.Y = index / 5 * 16;
		}

		public bool IsDestroyed()
		{
			return this._destroyed;
		}

		public virtual void Destroy()
		{
			this._destroyed = true;
			this._game.UnregisterGameObject(this);
		}

		public virtual void Move(float x, float y)
		{
			this.position.X += x;
			this.position.Y += y;
		}

		public Rectangle GetBounds()
		{
			return new Rectangle((int)(this.position.X - this.spriteAnchor.X), (int)(this.position.Y - this.spriteAnchor.Y), this.width, this.height);
		}

		public virtual void Update(GameTime time)
		{
		}

		public float GetRendererLayerDepth()
		{
			float layer_depth = this.layerDepth;
			if (layer_depth < 0f)
			{
				layer_depth = (float)this._game.gameHeight - this.position.Y;
			}
			return layer_depth;
		}

		public void ApplyDrawEffect(DrawEffect new_effect)
		{
			this.drawEffects.Add(new_effect);
		}

		public virtual void Draw(SpriteBatch b, float layer_depth)
		{
			if (this.visible)
			{
				SpriteEffects effects = SpriteEffects.None;
				if (this.flipX)
				{
					effects |= SpriteEffects.FlipHorizontally;
				}
				if (this.flipY)
				{
					effects |= SpriteEffects.FlipVertically;
				}
				float drawn_rotation = this.rotation;
				Vector2 drawn_scale = this.scale;
				Vector2 drawn_position = this.position - new Vector2(0f, this.zPosition);
				this.drawEffects.RemoveAll((DrawEffect effect) => effect.Apply(ref drawn_position, ref drawn_rotation, ref drawn_scale));
				b.Draw(this.texture, this._game.upperLeft + drawn_position * 4f, this.spriteRect, this.color, drawn_rotation * ((float)Math.PI / 180f), this.spriteAnchor, 4f * drawn_scale, effects, layer_depth);
			}
		}
	}

	public class SwayEffect : DrawEffect
	{
		public float swayMagnitude;

		public float swaySpeed;

		public int swayDuration = 1;

		public int age;

		public SwayEffect(float magnitude, float speed = 1f, int sway_duration = 10)
		{
			this.swayMagnitude = magnitude;
			this.swaySpeed = speed;
			this.swayDuration = sway_duration;
			this.age = 0;
		}

		public override bool Apply(ref Vector2 position, ref float rotation, ref Vector2 scale)
		{
			if (this.age > this.swayDuration)
			{
				return true;
			}
			float progress = (float)this.age / (float)this.swayDuration;
			rotation += (float)Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 1000.0 * 360.0 * (double)this.swaySpeed * 0.01745329238474369) * (1f - progress) * this.swayMagnitude;
			this.age++;
			return false;
		}
	}

	public class ShakeEffect : DrawEffect
	{
		public Vector2 shakeAmount;

		public int shakeDuration = 1;

		public int age;

		public ShakeEffect(float shake_x, float shake_y, int shake_duration = 10)
		{
			this.shakeAmount = new Vector2(shake_x, shake_y);
			this.shakeDuration = shake_duration;
			this.age = 0;
		}

		public override bool Apply(ref Vector2 position, ref float rotation, ref Vector2 scale)
		{
			if (this.age > this.shakeDuration)
			{
				return true;
			}
			float progress = (float)this.age / (float)this.shakeDuration;
			Vector2 current_shake = new Vector2(Utility.Lerp(this.shakeAmount.X, 1f, progress), Utility.Lerp(this.shakeAmount.Y, 1f, progress));
			position += new Vector2((float)(Game1.random.NextDouble() - 0.5) * 2f * current_shake.X, (float)(Game1.random.NextDouble() - 0.5) * 2f * current_shake.Y);
			this.age++;
			return false;
		}
	}

	public class StretchEffect : DrawEffect
	{
		public Vector2 stretchScale;

		public int stretchDuration = 1;

		public int age;

		public StretchEffect(float x_scale, float y_scale, int stretch_duration = 10)
		{
			this.stretchScale = new Vector2(x_scale, y_scale);
			this.stretchDuration = stretch_duration;
			this.age = 0;
		}

		public override bool Apply(ref Vector2 position, ref float rotation, ref Vector2 scale)
		{
			if (this.age > this.stretchDuration)
			{
				return true;
			}
			float progress = (float)this.age / (float)this.stretchDuration;
			Vector2 current_scale = new Vector2(Utility.Lerp(this.stretchScale.X, 1f, progress), Utility.Lerp(this.stretchScale.Y, 1f, progress));
			scale *= current_scale;
			this.age++;
			return false;
		}
	}

	public class DrawEffect
	{
		public virtual bool Apply(ref Vector2 position, ref float rotation, ref Vector2 scale)
		{
			return true;
		}
	}

	public int gameWidth = 304;

	public int gameHeight = 150;

	protected LocalizedContentManager _content;

	public Texture2D spriteSheet;

	public Vector2 upperLeft;

	protected List<CraneGameObject> _gameObjects;

	protected Dictionary<GameButtons, int> _buttonStates;

	protected bool _shouldQuit;

	public Action onQuit;

	public ICue music;

	public ICue fastMusic;

	public Effect _effect;

	public int freezeFrames;

	public ICue craneSound;

	public List<Type> _gameObjectTypes;

	public Dictionary<Type, List<CraneGameObject>> _gameObjectsByType;

	public CraneGame()
	{
		Utility.farmerHeardSong("crane_game");
		Utility.farmerHeardSong("crane_game_fast");
		this._effect = Game1.content.Load<Effect>("Effects\\ShadowRemoveMG3.8.0");
		this._content = Game1.content.CreateTemporary();
		this.spriteSheet = this._content.Load<Texture2D>("LooseSprites\\CraneGame");
		this._buttonStates = new Dictionary<GameButtons, int>();
		this._gameObjects = new List<CraneGameObject>();
		this._gameObjectTypes = new List<Type>();
		this._gameObjectsByType = new Dictionary<Type, List<CraneGameObject>>();
		this.changeScreenSize();
		new GameLogic(this);
		for (int i = 0; i < 9; i++)
		{
			this._buttonStates[(GameButtons)i] = 0;
		}
	}

	public void Quit()
	{
		if (!this._shouldQuit)
		{
			this.onQuit?.Invoke();
			this._shouldQuit = true;
		}
	}

	protected void _UpdateInput()
	{
		HashSet<InputButton> additional_keys = new HashSet<InputButton>();
		if (Game1.options.gamepadControls)
		{
			GamePadState pad_state = Game1.input.GetGamePadState();
			ButtonCollection.ButtonEnumerator enumerator = new ButtonCollection(ref pad_state).GetEnumerator();
			while (enumerator.MoveNext())
			{
				Keys key = Utility.mapGamePadButtonToKey(enumerator.Current);
				additional_keys.Add(new InputButton(key));
			}
		}
		if (Game1.input.GetMouseState().LeftButton == ButtonState.Pressed)
		{
			additional_keys.Add(new InputButton(mouseLeft: true));
		}
		else if (Game1.input.GetMouseState().RightButton == ButtonState.Pressed)
		{
			additional_keys.Add(new InputButton(mouseLeft: false));
		}
		this._UpdateButtonState(GameButtons.Action, Game1.options.actionButton, additional_keys);
		this._UpdateButtonState(GameButtons.Tool, Game1.options.useToolButton, additional_keys);
		this._UpdateButtonState(GameButtons.Confirm, Game1.options.menuButton, additional_keys);
		this._UpdateButtonState(GameButtons.Cancel, Game1.options.cancelButton, additional_keys);
		this._UpdateButtonState(GameButtons.Run, Game1.options.runButton, additional_keys);
		this._UpdateButtonState(GameButtons.Up, Game1.options.moveUpButton, additional_keys);
		this._UpdateButtonState(GameButtons.Down, Game1.options.moveDownButton, additional_keys);
		this._UpdateButtonState(GameButtons.Left, Game1.options.moveLeftButton, additional_keys);
		this._UpdateButtonState(GameButtons.Right, Game1.options.moveRightButton, additional_keys);
	}

	public bool IsButtonPressed(GameButtons button)
	{
		return this._buttonStates[button] == 1;
	}

	public bool IsButtonDown(GameButtons button)
	{
		return this._buttonStates[button] > 0;
	}

	protected void _UpdateButtonState(GameButtons button, InputButton[] keys, HashSet<InputButton> emulated_keys)
	{
		bool down = Game1.isOneOfTheseKeysDown(Game1.GetKeyboardState(), keys);
		for (int i = 0; i < keys.Length; i++)
		{
			if (emulated_keys.Contains(keys[i]))
			{
				down = true;
				break;
			}
		}
		if (this._buttonStates[button] == -1)
		{
			this._buttonStates[button] = 0;
		}
		if (down)
		{
			this._buttonStates[button]++;
		}
		else if (this._buttonStates[button] > 0)
		{
			this._buttonStates[button] = -1;
		}
	}

	public T GetObjectAtPoint<T>(Vector2 point, int max_count = -1) where T : CraneGameObject
	{
		foreach (CraneGameObject gameObject in this._gameObjects)
		{
			if (gameObject is T match && match.GetBounds().Contains((int)point.X, (int)point.Y))
			{
				return match;
			}
		}
		return null;
	}

	public List<T> GetObjectsAtPoint<T>(Vector2 point, int max_count = -1) where T : CraneGameObject
	{
		List<T> results = new List<T>();
		foreach (CraneGameObject gameObject in this._gameObjects)
		{
			if (gameObject is T match && match.GetBounds().Contains((int)point.X, (int)point.Y))
			{
				results.Add(match);
				if (max_count >= 0 && results.Count >= max_count)
				{
					return results;
				}
			}
		}
		return results;
	}

	public T GetObjectOfType<T>() where T : CraneGameObject
	{
		if (this._gameObjectsByType.TryGetValue(typeof(T), out var gameObjects) && gameObjects.Count > 0)
		{
			return gameObjects[0] as T;
		}
		return null;
	}

	public List<T> GetObjectsOfType<T>() where T : CraneGameObject
	{
		List<T> results = new List<T>();
		foreach (CraneGameObject gameObject in this._gameObjects)
		{
			if (gameObject is T match)
			{
				results.Add(match);
			}
		}
		return results;
	}

	public List<T> GetOverlaps<T>(CraneGameObject target, int max_count = -1) where T : CraneGameObject
	{
		List<T> results = new List<T>();
		foreach (CraneGameObject gameObject in this._gameObjects)
		{
			if (gameObject is T match && target.GetBounds().Intersects(match.GetBounds()) && target != match)
			{
				results.Add(match);
				if (max_count >= 0 && results.Count >= max_count)
				{
					return results;
				}
			}
		}
		return results;
	}

	public bool tick(GameTime time)
	{
		if (this._shouldQuit)
		{
			return true;
		}
		if (this.freezeFrames > 0)
		{
			this.freezeFrames--;
		}
		else
		{
			this._UpdateInput();
			for (int i = 0; i < this._gameObjects.Count; i++)
			{
				if (this._gameObjects[i] != null)
				{
					this._gameObjects[i].Update(time);
				}
			}
		}
		if (this.IsButtonPressed(GameButtons.Confirm))
		{
			this.Quit();
			Game1.playSound("bigDeSelect");
			GameLogic logic = this.GetObjectOfType<GameLogic>();
			if (logic != null && logic.collectedItems.Count > 0)
			{
				List<Item> items = new List<Item>();
				foreach (Item item in logic.collectedItems)
				{
					items.Add(item.getOne());
				}
				Game1.activeClickableMenu = new ItemGrabMenu(items, reverseGrab: false, showReceivingMenu: true, null, null, "Rewards", null, snapToBottom: false, canBeExitedWithKey: false, playRightClickSound: false, allowRightClick: false, showOrganizeButton: false, 0, null, -1, this);
			}
		}
		return false;
	}

	public bool forceQuit()
	{
		this.Quit();
		this.unload();
		GameLogic logic = this.GetObjectOfType<GameLogic>();
		if (logic != null)
		{
			foreach (Item collectedItem in logic.collectedItems)
			{
				Utility.CollectOrDrop(collectedItem.getOne());
			}
		}
		return true;
	}

	public bool overrideFreeMouseMovement()
	{
		return Game1.options.SnappyMenus;
	}

	public bool doMainGameUpdates()
	{
		return false;
	}

	public void receiveLeftClick(int x, int y, bool playSound = true)
	{
	}

	public void leftClickHeld(int x, int y)
	{
	}

	public void receiveRightClick(int x, int y, bool playSound = true)
	{
	}

	public void releaseLeftClick(int x, int y)
	{
	}

	public void releaseRightClick(int x, int y)
	{
	}

	public void receiveKeyPress(Keys k)
	{
	}

	public void receiveKeyRelease(Keys k)
	{
	}

	public void RegisterGameObject(CraneGameObject game_object)
	{
		if (!this._gameObjectTypes.Contains(game_object.GetType()))
		{
			this._gameObjectTypes.Add(game_object.GetType());
			this._gameObjectsByType[game_object.GetType()] = new List<CraneGameObject>();
		}
		this._gameObjectsByType[game_object.GetType()].Add(game_object);
		this._gameObjects.Add(game_object);
	}

	public void UnregisterGameObject(CraneGameObject game_object)
	{
		this._gameObjectsByType[game_object.GetType()].Remove(game_object);
		this._gameObjects.Remove(game_object);
	}

	public void draw(SpriteBatch b)
	{
		b.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, this._effect);
		b.Draw(this.spriteSheet, this.upperLeft, new Rectangle(0, 0, this.gameWidth, this.gameHeight), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
		Dictionary<CraneGameObject, float> depth_lookup = new Dictionary<CraneGameObject, float>();
		float lowest_depth = 0f;
		float highest_depth = 0f;
		for (int i = 0; i < this._gameObjects.Count; i++)
		{
			if (this._gameObjects[i] != null)
			{
				float depth = this._gameObjects[i].GetRendererLayerDepth();
				depth_lookup[this._gameObjects[i]] = depth;
				if (depth < lowest_depth)
				{
					lowest_depth = depth;
				}
				if (depth > highest_depth)
				{
					highest_depth = depth;
				}
			}
		}
		for (int j = 0; j < this._gameObjectTypes.Count; j++)
		{
			Type type = this._gameObjectTypes[j];
			for (int k = 0; k < this._gameObjectsByType[type].Count; k++)
			{
				float drawn_depth = Utility.Lerp(0.1f, 0.9f, (depth_lookup[this._gameObjectsByType[type][k]] - lowest_depth) / (highest_depth - lowest_depth));
				this._gameObjectsByType[type][k].Draw(b, drawn_depth);
			}
		}
		b.End();
	}

	public void changeScreenSize()
	{
		float pixel_zoom_adjustment = 1f / Game1.options.zoomLevel;
		Rectangle localMultiplayerWindow = Game1.game1.localMultiplayerWindow;
		float w = localMultiplayerWindow.Width;
		float h = localMultiplayerWindow.Height;
		Vector2 tmp = new Vector2(w / 2f, h / 2f) * pixel_zoom_adjustment;
		tmp.X -= this.gameWidth / 2 * 4;
		tmp.Y -= this.gameHeight / 2 * 4;
		this.upperLeft = tmp;
	}

	public void unload()
	{
		Game1.stopMusicTrack(MusicContext.MiniGame);
		if (this.music?.IsPlaying ?? false)
		{
			this.music.Stop(AudioStopOptions.Immediate);
		}
		if (this.fastMusic?.IsPlaying ?? false)
		{
			this.fastMusic.Stop(AudioStopOptions.Immediate);
		}
		if (this.craneSound?.IsPlaying ?? false)
		{
			this.craneSound.Stop(AudioStopOptions.Immediate);
		}
		this._content.Unload();
	}

	public void receiveEventPoke(int data)
	{
	}

	public string minigameId()
	{
		return "CraneGame";
	}
}
