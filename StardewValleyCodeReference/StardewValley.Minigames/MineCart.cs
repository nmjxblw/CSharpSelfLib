using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.BellsAndWhistles;
using StardewValley.Extensions;
using StardewValley.GameData;
using StardewValley.SpecialOrders;

namespace StardewValley.Minigames;

public class MineCart : IMinigame
{
	[XmlType("MineCart.GameStates")]
	public enum GameStates
	{
		Title,
		Ingame,
		FruitsSummary,
		Map,
		Cutscene
	}

	public class LevelTransition
	{
		public int startLevel;

		public int destinationLevel;

		public Point startGridCoordinates;

		public string pathString = "";

		public Func<bool> shouldTakePath;

		public LevelTransition(int start_level, int destination_level, int start_grid_x, int start_grid_y, string path_string, Func<bool> should_take_path = null)
		{
			this.startLevel = start_level;
			this.destinationLevel = destination_level;
			this.startGridCoordinates = new Point(start_grid_x, start_grid_y);
			this.pathString = path_string;
			this.shouldTakePath = should_take_path;
		}
	}

	public enum CollectableFruits
	{
		Cherry,
		Orange,
		Grape,
		MAX
	}

	public enum ObstacleTypes
	{
		Normal,
		Air,
		Difficult
	}

	public class GeneratorRoll
	{
		public float chance;

		public BaseTrackGenerator generator;

		public Func<bool> additionalGenerationCondition;

		public BaseTrackGenerator forcedNextGenerator;

		public GeneratorRoll(float generator_chance, BaseTrackGenerator track_generator, Func<bool> additional_generation_condition = null, BaseTrackGenerator forced_next_generator = null)
		{
			this.chance = generator_chance;
			this.generator = track_generator;
			this.forcedNextGenerator = forced_next_generator;
			this.additionalGenerationCondition = additional_generation_condition;
		}
	}

	public class MapJunimo : Entity
	{
		public enum MoveState
		{
			Idle,
			Moving,
			Finished
		}

		public int direction = 2;

		public string moveString = "";

		public float moveSpeed = 60f;

		public float pixelsToMove;

		public MoveState moveState;

		public float nextBump;

		public float bumpHeight;

		private bool isOnWater;

		public void StartMoving()
		{
			this.moveState = MoveState.Moving;
		}

		protected override void _Update(float time)
		{
			int desired_direction = this.direction;
			this.isOnWater = false;
			if (base.position.X > 194f && base.position.X < 251f && base.position.Y > 165f)
			{
				this.isOnWater = true;
				base._game.minecartLoop.Pause();
			}
			if (this.moveString.Length > 0)
			{
				switch (this.moveString[0])
				{
				case 'u':
					desired_direction = 0;
					break;
				case 'd':
					desired_direction = 2;
					break;
				case 'l':
					desired_direction = 3;
					break;
				case 'r':
					desired_direction = 1;
					break;
				}
			}
			if (this.moveState == MoveState.Idle && !base._game.minecartLoop.IsPaused)
			{
				base._game.minecartLoop.Pause();
			}
			if (this.moveState == MoveState.Moving)
			{
				this.nextBump -= time;
				this.bumpHeight = Utility.MoveTowards(this.bumpHeight, 0f, time * 5f);
				if (this.nextBump <= 0f)
				{
					this.nextBump = Utility.RandomFloat(0.1f, 0.3f);
					this.bumpHeight = -2f;
				}
				if (!this.isOnWater && base._game.minecartLoop.IsPaused)
				{
					base._game.minecartLoop.Resume();
				}
				if (this.pixelsToMove <= 0f)
				{
					if (desired_direction != this.direction)
					{
						this.direction = desired_direction;
						if (!this.isOnWater)
						{
							Game1.playSound("parry");
							base._game.createSparkShower(base.position);
						}
						else
						{
							Game1.playSound("waterSlosh");
						}
					}
					if (this.moveString.Length > 0)
					{
						this.pixelsToMove = 16f;
						this.moveString = this.moveString.Substring(1);
					}
					else
					{
						this.moveState = MoveState.Finished;
						this.direction = 2;
						if (base.position.X < 368f)
						{
							if (!this.isOnWater)
							{
								Game1.playSound("parry");
								base._game.createSparkShower(base.position);
							}
							else
							{
								Game1.playSound("waterSlosh");
							}
						}
					}
				}
				if (this.pixelsToMove > 0f)
				{
					float pixels_to_move_now = Math.Min(this.pixelsToMove, this.moveSpeed * time);
					Vector2 direction_to_move = Vector2.Zero;
					switch (this.direction)
					{
					case 1:
						direction_to_move.X = 1f;
						break;
					case 3:
						direction_to_move.X = -1f;
						break;
					case 0:
						direction_to_move.Y = -1f;
						break;
					case 2:
						direction_to_move.Y = 1f;
						break;
					}
					base.position += direction_to_move * pixels_to_move_now;
					this.pixelsToMove -= pixels_to_move_now;
				}
			}
			else
			{
				this.bumpHeight = -2f;
			}
			if (this.moveState == MoveState.Finished && !base._game.minecartLoop.IsPaused)
			{
				base._game.minecartLoop.Pause();
			}
			base._Update(time);
		}

		public override void _Draw(SpriteBatch b)
		{
			SpriteEffects effect = SpriteEffects.None;
			Rectangle source_rect = new Rectangle(400, 512, 16, 16);
			switch (this.direction)
			{
			case 0:
				source_rect.Y = 544;
				break;
			case 2:
				source_rect.Y = 512;
				break;
			default:
				source_rect.Y = 528;
				if (this.direction == 3)
				{
					effect = SpriteEffects.FlipHorizontally;
				}
				break;
			}
			if (this.isOnWater)
			{
				source_rect.Height -= 3;
				b.Draw(base._game.texture, base._game.TransformDraw(base.drawnPosition + new Vector2(0f, -1f) + new Vector2(0f, 1f) * this.bumpHeight), source_rect, Color.White, 0f, new Vector2(8f, 8f), base._game.GetPixelScale(), effect, 0.45f);
				b.Draw(base._game.texture, base._game.TransformDraw(base.drawnPosition + new Vector2(2f, 10f) + new Vector2(0f, 1f) * this.bumpHeight), new Rectangle(414, 624, 13, 5), Color.White, 0f, new Vector2(8f, 8f), base._game.GetPixelScale(), effect, 0.44f);
			}
			else
			{
				b.Draw(base._game.texture, base._game.TransformDraw(base.drawnPosition + new Vector2(0f, -1f) + new Vector2(0f, 1f) * this.bumpHeight), source_rect, Color.White, 0f, new Vector2(8f, 8f), base._game.GetPixelScale(), effect, 0.45f);
			}
		}
	}

	public class LakeDecor
	{
		public Point _position;

		public int spriteIndex;

		protected MineCart _game;

		public int _lastCycle = -1;

		public bool _bgDecor;

		private int _animationFrames = 1;

		public LakeDecor(MineCart game, int theme = -1, bool bgDecor = false, int forceXPosition = -1)
		{
			this._game = game;
			this._position = new Point(Game1.random.Next(0, this._game.screenWidth), Game1.random.Next(160, this._game.screenHeight));
			if (forceXPosition != -1)
			{
				this._position.X = forceXPosition * (this._game.screenWidth / 16) + Game1.random.Next(0, this._game.screenWidth / 16);
			}
			this._bgDecor = bgDecor;
			this.spriteIndex = Game1.random.Next(2);
			switch (theme)
			{
			case 2:
				this.spriteIndex = 2;
				break;
			case 1:
				this.spriteIndex += 3;
				break;
			case 5:
				this.spriteIndex += 5;
				break;
			case 4:
				this.spriteIndex = 14;
				this._animationFrames = 6;
				break;
			case 9:
				this.spriteIndex += 7;
				break;
			case 6:
				this.spriteIndex = 1;
				break;
			}
			if (!bgDecor)
			{
				return;
			}
			this.spriteIndex += 7;
			this._position.Y = Game1.random.Next(0, this._game.screenHeight / 3);
			switch (theme)
			{
			case 2:
				if (forceXPosition % 5 == 0)
				{
					this.spriteIndex++;
					this._animationFrames = 4;
				}
				break;
			case 3:
				this.spriteIndex = 24;
				this._animationFrames = 4;
				break;
			case 6:
				this.spriteIndex = 20;
				this._position.Y = Game1.random.Next(0, this._game.screenHeight / 5);
				this._animationFrames = 4;
				break;
			case 9:
				this.spriteIndex = 28;
				this._animationFrames = 4;
				break;
			}
		}

		public void Draw(SpriteBatch b)
		{
			Vector2 draw_position = default(Vector2);
			float side_buffer_space = 32f;
			float y_position_in_lake = (float)(this._position.Y - 160) / (float)(this._game.screenHeight - 160);
			float scroll_speed = Utility.Lerp(-0.4f, -0.75f, y_position_in_lake);
			int current_cycle = (int)Math.Floor(((float)this._position.X + this._game.screenLeftBound * scroll_speed) / ((float)this._game.screenWidth + side_buffer_space * 2f));
			if (current_cycle != this._lastCycle)
			{
				this._lastCycle = current_cycle;
				if (this.spriteIndex < 2)
				{
					this.spriteIndex = Game1.random.Next(2);
					if (this._game.currentTheme == 6)
					{
						this.spriteIndex = 1;
					}
				}
			}
			float drawY = this._position.Y;
			if (this._bgDecor)
			{
				scroll_speed = Utility.Lerp(-0.15f, -0.25f, (float)this._position.Y / (float)(this._game.screenHeight / 3));
				if (this._game.currentTheme == 3)
				{
					drawY += (float)(int)(Math.Sin(Utility.Lerp(0f, (float)Math.PI * 2f, (float)((this._game.totalTimeMS + (double)(this._position.X * 7) + (double)(this._position.Y * 2)) / 2.0 % 1000.0) / 1000f)) * 3.0);
				}
			}
			draw_position.X = (float)MineCart.Mod((int)((float)this._position.X + this._game.screenLeftBound * scroll_speed), (int)((float)this._game.screenWidth + side_buffer_space * 2f)) - side_buffer_space;
			b.Draw(this._game.texture, this._game.TransformDraw(new Vector2(draw_position.X, drawY)), new Rectangle(96 + this.spriteIndex % 14 * this._game.tileSize + (int)((this._game.totalTimeMS + (double)(this._position.X * 10)) % 1000.0 / (double)(1000 / this._animationFrames)) % 14 * this._game.tileSize, 848 + this.spriteIndex / 14 * this._game.tileSize, 16, 16), (this.spriteIndex == 0) ? this._game.midBGTint : ((this.spriteIndex == 1) ? this._game.lakeTint : Color.White), 0f, Vector2.Zero, this._game.GetPixelScale(), SpriteEffects.None, this._bgDecor ? 0.65f : (0.8f + y_position_in_lake * -0.001f));
		}
	}

	public class StraightAwayGenerator : BaseTrackGenerator
	{
		public int straightAwayLength = 10;

		public List<int> staggerPattern;

		public int minLength = 3;

		public int maxLength = 5;

		public float staggerChance = 0.25f;

		public int minimuimDistanceBetweenStaggers = 1;

		public int currentStaggerDistance;

		public bool generateCheckpoint = true;

		protected bool _generatedCheckpoint = true;

		public StraightAwayGenerator SetMinimumDistanceBetweenStaggers(int min)
		{
			this.minimuimDistanceBetweenStaggers = min;
			return this;
		}

		public StraightAwayGenerator SetLength(int min, int max)
		{
			this.minLength = min;
			this.maxLength = max;
			return this;
		}

		public StraightAwayGenerator SetCheckpoint(bool checkpoint)
		{
			this.generateCheckpoint = checkpoint;
			return this;
		}

		public StraightAwayGenerator SetStaggerChance(float chance)
		{
			this.staggerChance = chance;
			return this;
		}

		public StraightAwayGenerator SetStaggerValues(params int[] args)
		{
			this.staggerPattern = new List<int>();
			for (int i = 0; i < args.Length; i++)
			{
				this.staggerPattern.Add(args[i]);
			}
			return this;
		}

		public StraightAwayGenerator SetStaggerValueRange(int min, int max)
		{
			this.staggerPattern = new List<int>();
			for (int i = min; i <= max; i++)
			{
				this.staggerPattern.Add(i);
			}
			return this;
		}

		public StraightAwayGenerator(MineCart game)
			: base(game)
		{
		}

		public override void Initialize()
		{
			this.straightAwayLength = Game1.random.Next(this.minLength, this.maxLength + 1);
			this._generatedCheckpoint = false;
			if (this.straightAwayLength <= 3)
			{
				this._generatedCheckpoint = true;
			}
			base.Initialize();
		}

		protected override void _GenerateTrack()
		{
			if (base._game.generatorPosition.X >= base._game.distanceToTravel)
			{
				return;
			}
			for (int i = 0; i < this.straightAwayLength; i++)
			{
				if (base._game.generatorPosition.X >= base._game.distanceToTravel)
				{
					return;
				}
				int last_y = base._game.generatorPosition.Y;
				if (this.currentStaggerDistance <= 0)
				{
					if (Game1.random.NextDouble() < (double)this.staggerChance)
					{
						base._game.generatorPosition.Y += Game1.random.ChooseFrom(this.staggerPattern);
					}
					this.currentStaggerDistance = this.minimuimDistanceBetweenStaggers;
				}
				else
				{
					this.currentStaggerDistance--;
				}
				if (!base._game.IsTileInBounds(base._game.generatorPosition.Y))
				{
					base._game.generatorPosition.Y = last_y;
					this.straightAwayLength = 0;
					break;
				}
				base._game.generatorPosition.Y = base._game.KeepTileInBounds(base._game.generatorPosition.Y);
				Track.TrackType tile_type = Track.TrackType.Straight;
				if (base._game.generatorPosition.Y < last_y)
				{
					tile_type = Track.TrackType.UpSlope;
				}
				else if (base._game.generatorPosition.Y > last_y)
				{
					tile_type = Track.TrackType.DownSlope;
				}
				if (tile_type == Track.TrackType.DownSlope && base._game.currentTheme == 1)
				{
					tile_type = Track.TrackType.IceDownSlope;
				}
				if (tile_type == Track.TrackType.UpSlope && base._game.currentTheme == 5)
				{
					tile_type = Track.TrackType.SlimeUpSlope;
				}
				base.AddPickupTrack(base._game.generatorPosition.X, base._game.generatorPosition.Y, tile_type);
				base._game.generatorPosition.X++;
			}
			if (base._generatedTracks != null && base._generatedTracks.Count > 0 && this.generateCheckpoint && !this._generatedCheckpoint)
			{
				this._generatedCheckpoint = true;
				base._generatedTracks.OrderBy((Track o) => o.position.X);
				base._game.AddCheckpoint((int)(base._generatedTracks[0].position.X / (float)base._game.tileSize));
			}
		}
	}

	public class SmallGapGenerator : BaseTrackGenerator
	{
		public int minLength = 3;

		public int maxLength = 5;

		public int minDepth = 5;

		public int maxDepth = 5;

		public SmallGapGenerator SetLength(int min, int max)
		{
			this.minLength = min;
			this.maxLength = max;
			return this;
		}

		public SmallGapGenerator SetDepth(int min, int max)
		{
			this.minDepth = min;
			this.maxDepth = max;
			return this;
		}

		public SmallGapGenerator(MineCart game)
			: base(game)
		{
		}

		protected override void _GenerateTrack()
		{
			if (base._game.generatorPosition.X >= base._game.distanceToTravel)
			{
				return;
			}
			int depth = Game1.random.Next(this.minDepth, this.maxDepth + 1);
			int length = Game1.random.Next(this.minLength, this.maxLength + 1);
			base.AddTrack(base._game.generatorPosition.X, base._game.generatorPosition.Y);
			base._game.generatorPosition.X++;
			base._game.generatorPosition.Y += depth;
			for (int i = 0; i < length; i++)
			{
				if (base._game.generatorPosition.X >= base._game.distanceToTravel)
				{
					base._game.generatorPosition.Y -= depth;
					return;
				}
				base.AddPickupTrack(base._game.generatorPosition.X, base._game.generatorPosition.Y);
				base._game.generatorPosition.X++;
			}
			base._game.generatorPosition.Y -= depth;
			if (base._game.generatorPosition.X < base._game.distanceToTravel)
			{
				base.AddTrack(base._game.generatorPosition.X, base._game.generatorPosition.Y);
				base._game.generatorPosition.X++;
			}
		}
	}

	public class RapidHopsGenerator : BaseTrackGenerator
	{
		public int minLength = 3;

		public int maxLength = 5;

		private int startY;

		public int yStep;

		public bool chaotic;

		public RapidHopsGenerator SetLength(int min, int max)
		{
			this.minLength = min;
			this.maxLength = max;
			return this;
		}

		public RapidHopsGenerator SetYStep(int yStep)
		{
			this.yStep = yStep;
			return this;
		}

		public RapidHopsGenerator SetChaotic(bool chaotic)
		{
			this.chaotic = chaotic;
			return this;
		}

		public RapidHopsGenerator(MineCart game)
			: base(game)
		{
		}

		protected override void _GenerateTrack()
		{
			if (base._game.generatorPosition.X >= base._game.distanceToTravel)
			{
				return;
			}
			if (this.startY == 0)
			{
				this.startY = base._game.generatorPosition.Y;
			}
			int length = Game1.random.Next(this.minLength, this.maxLength + 1);
			base.AddTrack(base._game.generatorPosition.X, base._game.generatorPosition.Y);
			base._game.generatorPosition.X++;
			base._game.generatorPosition.Y += this.yStep;
			for (int i = 0; i < length; i++)
			{
				if (base._game.generatorPosition.Y < 3 || base._game.generatorPosition.Y > base._game.screenHeight / base._game.tileSize - 2)
				{
					base._game.generatorPosition.Y = base._game.screenHeight / base._game.tileSize - 2;
					this.startY = base._game.generatorPosition.Y;
				}
				if (base._game.generatorPosition.X >= base._game.distanceToTravel)
				{
					base._game.generatorPosition.Y -= this.yStep;
					return;
				}
				base.AddPickupTrack(base._game.generatorPosition.X, base._game.generatorPosition.Y);
				base._game.generatorPosition.X += Game1.random.Next(2, 4);
				if (Game1.random.NextDouble() < 0.33)
				{
					base.AddTrack(base._game.generatorPosition.X - 1, Math.Min(base._game.screenHeight / base._game.tileSize - 2, base._game.generatorPosition.Y + Game1.random.Next(5)));
				}
				if (this.chaotic)
				{
					base._game.generatorPosition.Y = this.startY + Game1.random.Next(-Math.Abs(this.yStep), Math.Abs(this.yStep) + 1);
				}
				else
				{
					base._game.generatorPosition.Y += this.yStep;
				}
			}
			if (base._game.generatorPosition.X < base._game.distanceToTravel)
			{
				base._game.generatorPosition.Y -= this.yStep;
				base.AddTrack(base._game.generatorPosition.X, base._game.generatorPosition.Y);
				base._game.generatorPosition.X++;
			}
		}
	}

	public class NoxiousMushroom : Obstacle
	{
		public float nextFire;

		public float firePeriod = 1.75f;

		protected Track _track;

		public Rectangle[] frames = new Rectangle[3]
		{
			new Rectangle(288, 736, 16, 16),
			new Rectangle(288, 752, 16, 16),
			new Rectangle(288, 768, 16, 16)
		};

		public int currentFrame;

		public float frameDuration = 0.05f;

		public float frameTimer;

		public override Rectangle GetLocalBounds()
		{
			return new Rectangle(-4, -12, 8, 12);
		}

		public override void InitializeObstacle(Track track)
		{
			this.nextFire = Utility.RandomFloat(0f, this.firePeriod);
			this._track = track;
			base.InitializeObstacle(track);
		}

		protected override void _Update(float time)
		{
			this.nextFire -= time;
			if (this.nextFire <= 0f)
			{
				if (base.IsOnScreen() && base._game.deathTimer <= 0f && (float)base._game.respawnCounter <= 0f)
				{
					NoxiousGas noxiousGas = base._game.AddEntity(new NoxiousGas());
					noxiousGas.position = base.position;
					noxiousGas.position.Y = this.GetBounds().Top;
					noxiousGas.InitializeObstacle(this._track);
					Game1.playSound("sandyStep");
					this.currentFrame = 1;
					this.frameTimer = this.frameDuration;
				}
				this.nextFire = 1.5f;
			}
			if (this.currentFrame <= 0)
			{
				return;
			}
			this.frameTimer -= time;
			if (this.frameTimer <= 0f)
			{
				this.frameTimer = this.frameDuration;
				this.currentFrame++;
				if (this.currentFrame >= this.frames.Length)
				{
					this.currentFrame = 0;
					this.frameTimer = 0f;
				}
			}
		}

		public override void _Draw(SpriteBatch b)
		{
			b.Draw(base._game.texture, base._game.TransformDraw(base.drawnPosition), this.frames[this.currentFrame], Color.White, 0f, new Vector2(8f, 16f), base._game.GetPixelScale(), SpriteEffects.None, 0.45f);
		}

		public override bool CanSpawnHere(Track track)
		{
			if (track == null)
			{
				return false;
			}
			if (track.trackType != Track.TrackType.Straight)
			{
				return false;
			}
			return true;
		}
	}

	public class MushroomSpring : Obstacle
	{
		protected HashSet<MineCartCharacter> _bouncedPlayers;

		public Rectangle[] frames = new Rectangle[3]
		{
			new Rectangle(400, 736, 16, 16),
			new Rectangle(400, 752, 16, 16),
			new Rectangle(400, 768, 16, 16)
		};

		public int currentFrame;

		public float frameDuration = 0.05f;

		public float frameTimer;

		public override Rectangle GetLocalBounds()
		{
			return new Rectangle(-4, -12, 8, 12);
		}

		public override void InitializeObstacle(Track track)
		{
			base.InitializeObstacle(track);
			this._bouncedPlayers = new HashSet<MineCartCharacter>();
		}

		protected override void _Update(float time)
		{
			if (this.currentFrame <= 0)
			{
				return;
			}
			this.frameTimer -= time;
			if (this.frameTimer <= 0f)
			{
				this.frameTimer = this.frameDuration;
				this.currentFrame++;
				if (this.currentFrame >= this.frames.Length)
				{
					this.currentFrame = 0;
					this.frameTimer = 0f;
				}
			}
		}

		public override void _Draw(SpriteBatch b)
		{
			b.Draw(base._game.texture, base._game.TransformDraw(base.drawnPosition), this.frames[this.currentFrame], Color.White, 0f, new Vector2(8f, 16f), base._game.GetPixelScale(), SpriteEffects.None, 0.45f);
		}

		public override bool CanSpawnHere(Track track)
		{
			if (track == null)
			{
				return false;
			}
			if (track.trackType != Track.TrackType.Straight)
			{
				return false;
			}
			return true;
		}

		public override bool OnBounce(MineCartCharacter player)
		{
			this.BouncePlayer(player);
			return true;
		}

		public override bool OnBump(PlayerMineCartCharacter player)
		{
			this.BouncePlayer(player);
			return true;
		}

		public void BouncePlayer(MineCartCharacter player)
		{
			if (!this._bouncedPlayers.Contains(player))
			{
				this._bouncedPlayers.Add(player);
				if (player is PlayerMineCartCharacter)
				{
					this.currentFrame = 1;
					this.frameTimer = this.frameDuration;
					this.ShootDebris(Game1.random.Next(-10, -4), Game1.random.Next(-60, -19));
					this.ShootDebris(Game1.random.Next(5, 11), Game1.random.Next(-60, -19));
					this.ShootDebris(Game1.random.Next(-20, -9), Game1.random.Next(-40, 0));
					this.ShootDebris(Game1.random.Next(10, 21), Game1.random.Next(-40, 0));
					Game1.playSound("hitEnemy");
				}
				player.Bounce(0.15f);
			}
		}

		public void ShootDebris(int x, int y)
		{
			base._game.AddEntity(new MineDebris(new Rectangle(368, 784, 16, 16), Utility.PointToVector2(this.GetBounds().Center), x, y, 0.25f, 0f, 0.9f, 1f, 3, 0.3f));
		}

		public override void OnPlayerReset()
		{
			this._bouncedPlayers.Clear();
			base.OnPlayerReset();
		}
	}

	public class MushroomBalanceTrackGenerator : BaseTrackGenerator
	{
		protected int minHopSize = 1;

		protected int maxHopSize = 1;

		protected float releaseJumpChance;

		protected List<int> staggerPattern;

		protected Track.TrackType trackType;

		public MushroomBalanceTrackGenerator SetTrackType(Track.TrackType track_type)
		{
			this.trackType = track_type;
			return this;
		}

		public MushroomBalanceTrackGenerator SetStaggerValues(params int[] args)
		{
			this.staggerPattern = new List<int>();
			for (int i = 0; i < args.Length; i++)
			{
				this.staggerPattern.Add(args[i]);
			}
			return this;
		}

		public MushroomBalanceTrackGenerator SetReleaseJumpChance(float chance)
		{
			this.releaseJumpChance = chance;
			return this;
		}

		public MushroomBalanceTrackGenerator SetHopSize(int min, int max)
		{
			this.minHopSize = min;
			this.maxHopSize = max;
			return this;
		}

		public MushroomBalanceTrackGenerator(MineCart game)
			: base(game)
		{
			this.staggerPattern = new List<int>();
		}

		protected override void _GenerateTrack()
		{
			if (base._game.generatorPosition.X >= base._game.distanceToTravel)
			{
				return;
			}
			base._game.trackBuilderCharacter.enabled = true;
			List<BalanceTrack> balance_tracks = new List<BalanceTrack>();
			for (int i = 0; i < 4; i++)
			{
				if (i == 1 && Game1.random.NextBool())
				{
					continue;
				}
				base._game.trackBuilderCharacter.position.X = ((float)base._game.generatorPosition.X - 1f + 0.5f) * (float)base._game.tileSize;
				base._game.trackBuilderCharacter.position.Y = base._game.generatorPosition.Y * base._game.tileSize;
				base._game.trackBuilderCharacter.ForceGrounded();
				base._game.trackBuilderCharacter.Jump();
				base._game.trackBuilderCharacter.Update(0.03f);
				int target_y = base._game.generatorPosition.Y;
				if (i != 1)
				{
					if (i == 3 && Game1.random.NextBool())
					{
						target_y -= 4;
					}
					else if (this.staggerPattern != null && this.staggerPattern.Count > 0)
					{
						target_y += Game1.random.ChooseFrom(this.staggerPattern);
					}
				}
				target_y = base._game.KeepTileInBounds(target_y);
				bool has_landed = false;
				while (!has_landed)
				{
					if (base._game.trackBuilderCharacter.position.Y < (float)(target_y * base._game.tileSize) && Math.Abs(Math.Round(base._game.trackBuilderCharacter.position.X / (float)base._game.tileSize) - (double)base._game.generatorPosition.X) > 0.0 && base._game.trackBuilderCharacter.IsJumping() && Game1.random.NextDouble() < (double)this.releaseJumpChance)
					{
						base._game.trackBuilderCharacter.ReleaseJump();
					}
					Vector2 old_position = base._game.trackBuilderCharacter.position;
					base._game.trackBuilderCharacter.Update(0.03f);
					if (old_position.Y < (float)(target_y * base._game.tileSize) && base._game.trackBuilderCharacter.position.Y >= (float)(target_y * base._game.tileSize))
					{
						has_landed = true;
					}
					if (base._game.trackBuilderCharacter.IsGrounded() || base._game.trackBuilderCharacter.position.Y / (float)base._game.tileSize > (float)base._game.bottomTile)
					{
						base._game.trackBuilderCharacter.position = old_position;
						if (!base._game.IsTileInBounds(target_y))
						{
							return;
						}
						target_y = base._game.KeepTileInBounds((int)(old_position.Y / (float)base._game.tileSize));
						break;
					}
				}
				base._game.generatorPosition.Y = target_y;
				if (i == 0 || i == 2)
				{
					List<BalanceTrack> current_balance_tracks = new List<BalanceTrack>();
					base._game.generatorPosition.X = (int)(base._game.trackBuilderCharacter.position.X / (float)base._game.tileSize);
					float y_offset = 0f;
					if (i == 2 && balance_tracks.Count > 0)
					{
						y_offset = balance_tracks[0].position.Y - balance_tracks[0].startY;
					}
					BalanceTrack track = new BalanceTrack(Track.TrackType.MushroomLeft, showSecondTile: false);
					track.position.X = base._game.generatorPosition.X * base._game.tileSize;
					track.position.Y = base._game.trackBuilderCharacter.position.Y + y_offset;
					track.startY = track.position.Y;
					base.AddTrack(track);
					current_balance_tracks.Add(track);
					base._game.generatorPosition.X++;
					track = new BalanceTrack(Track.TrackType.MushroomMiddle, showSecondTile: false);
					track.position.X = base._game.generatorPosition.X * base._game.tileSize;
					track.position.Y = base._game.trackBuilderCharacter.position.Y + y_offset;
					track.startY = track.position.Y;
					base.AddTrack(track);
					current_balance_tracks.Add(track);
					base._game.generatorPosition.X++;
					track = new BalanceTrack(Track.TrackType.MushroomRight, showSecondTile: false);
					track.position.X = base._game.generatorPosition.X * base._game.tileSize;
					track.position.Y = base._game.trackBuilderCharacter.position.Y + y_offset;
					track.startY = track.position.Y;
					base.AddTrack(track);
					current_balance_tracks.Add(track);
					base._game.generatorPosition.X++;
					foreach (BalanceTrack item in current_balance_tracks)
					{
						item.connectedTracks = new List<BalanceTrack>(current_balance_tracks);
					}
					if (i == 2)
					{
						foreach (BalanceTrack item2 in balance_tracks)
						{
							item2.counterBalancedTracks = new List<BalanceTrack>(current_balance_tracks);
						}
						foreach (BalanceTrack item3 in current_balance_tracks)
						{
							item3.counterBalancedTracks = new List<BalanceTrack>(balance_tracks);
						}
					}
					base._game.trackBuilderCharacter.SnapToFloor();
					while (base._game.trackBuilderCharacter.IsGrounded())
					{
						float old_x = base._game.trackBuilderCharacter.position.X;
						base._game.trackBuilderCharacter.Update(0.03f);
						if (!base._game.trackBuilderCharacter.IsGrounded())
						{
							base._game.trackBuilderCharacter.position.X = old_x;
						}
						if (Game1.random.NextDouble() < 0.33000001311302185)
						{
							break;
						}
					}
					balance_tracks.AddRange(current_balance_tracks);
					continue;
				}
				int hop_width = Game1.random.Next(this.minHopSize, this.maxHopSize + 1);
				for (int width = 0; width < hop_width; width++)
				{
					base._game.generatorPosition.X = (int)(base._game.trackBuilderCharacter.position.X / (float)base._game.tileSize) + width;
					if (base._game.generatorPosition.X >= base._game.distanceToTravel)
					{
						return;
					}
					base.AddPickupTrack(base._game.generatorPosition.X, base._game.generatorPosition.Y, this.trackType);
				}
			}
			foreach (BalanceTrack balance_track in balance_tracks)
			{
				balance_track.position.Y = balance_track.startY;
			}
			base._game.generatorPosition.X++;
		}
	}

	public class MushroomBunnyHopGenerator : BaseTrackGenerator
	{
		protected int numberOfHops;

		protected int minHops = 1;

		protected int maxHops = 5;

		protected int minHopSize = 1;

		protected int maxHopSize = 1;

		protected float releaseJumpChance;

		protected List<int> staggerPattern;

		protected Track.TrackType trackType;

		public MushroomBunnyHopGenerator SetStaggerValues(params int[] args)
		{
			this.staggerPattern = new List<int>();
			for (int i = 0; i < args.Length; i++)
			{
				this.staggerPattern.Add(args[i]);
			}
			return this;
		}

		public MushroomBunnyHopGenerator SetReleaseJumpChance(float chance)
		{
			this.releaseJumpChance = chance;
			return this;
		}

		public MushroomBunnyHopGenerator SetHopSize(int min, int max)
		{
			this.minHopSize = min;
			this.maxHopSize = max;
			return this;
		}

		public MushroomBunnyHopGenerator SetNumberOfHops(int min, int max)
		{
			this.minHops = min;
			this.maxHops = max;
			return this;
		}

		public MushroomBunnyHopGenerator(MineCart game)
			: base(game)
		{
			this.minHopSize = 1;
			this.maxHopSize = 1;
			this.staggerPattern = new List<int>();
		}

		public override void Initialize()
		{
			this.numberOfHops = Game1.random.Next(this.minHops, this.maxHops + 1);
			base.Initialize();
		}

		protected override void _GenerateTrack()
		{
			if (base._game.generatorPosition.X >= base._game.distanceToTravel)
			{
				return;
			}
			base._game.trackBuilderCharacter.enabled = true;
			MushroomSpring spring = null;
			for (int i = 0; i < this.numberOfHops; i++)
			{
				base._game.trackBuilderCharacter.position.X = ((float)base._game.generatorPosition.X - 1f + 0.5f) * (float)base._game.tileSize;
				base._game.trackBuilderCharacter.position.Y = base._game.generatorPosition.Y * base._game.tileSize;
				base._game.trackBuilderCharacter.ForceGrounded();
				base._game.trackBuilderCharacter.Jump();
				spring?.BouncePlayer(base._game.trackBuilderCharacter);
				base._game.trackBuilderCharacter.Update(0.03f);
				int target_y = base._game.generatorPosition.Y;
				if (this.staggerPattern != null && this.staggerPattern.Count > 0)
				{
					target_y += Game1.random.ChooseFrom(this.staggerPattern);
				}
				target_y = base._game.KeepTileInBounds(target_y);
				bool has_landed = false;
				while (!has_landed)
				{
					if (base._game.trackBuilderCharacter.position.Y < (float)(target_y * base._game.tileSize) && Math.Abs(Math.Round(base._game.trackBuilderCharacter.position.X / (float)base._game.tileSize) - (double)base._game.generatorPosition.X) > 1.0 && base._game.trackBuilderCharacter.IsJumping() && Game1.random.NextDouble() < (double)this.releaseJumpChance)
					{
						base._game.trackBuilderCharacter.ReleaseJump();
					}
					Vector2 old_position = base._game.trackBuilderCharacter.position;
					float y = base._game.trackBuilderCharacter.velocity.Y;
					base._game.trackBuilderCharacter.Update(0.03f);
					if (y < 0f && base._game.trackBuilderCharacter.velocity.Y >= 0f)
					{
						base._game.CreatePickup(base._game.trackBuilderCharacter.position + new Vector2(0f, 8f));
					}
					if (old_position.Y < (float)(target_y * base._game.tileSize) && base._game.trackBuilderCharacter.position.Y >= (float)(target_y * base._game.tileSize))
					{
						has_landed = true;
					}
					if (base._game.trackBuilderCharacter.IsGrounded() || base._game.trackBuilderCharacter.position.Y / (float)base._game.tileSize > (float)base._game.bottomTile)
					{
						base._game.trackBuilderCharacter.position = old_position;
						if (!base._game.IsTileInBounds(target_y))
						{
							return;
						}
						target_y = base._game.KeepTileInBounds((int)(old_position.Y / (float)base._game.tileSize));
						break;
					}
				}
				base._game.generatorPosition.Y = target_y;
				int hop_width = Game1.random.Next(this.minHopSize, this.maxHopSize + 1);
				Track.TrackType track_type = this.trackType;
				if (i >= this.numberOfHops - 1)
				{
					track_type = Track.TrackType.Straight;
				}
				spring = null;
				for (int width = 0; width < hop_width; width++)
				{
					base._game.generatorPosition.X = (int)(base._game.trackBuilderCharacter.position.X / (float)base._game.tileSize) + width;
					if (base._game.generatorPosition.X >= base._game.distanceToTravel)
					{
						return;
					}
					if (track_type == Track.TrackType.MushroomMiddle)
					{
						base.AddTrack(base._game.generatorPosition.X - 1, base._game.generatorPosition.Y, Track.TrackType.MushroomLeft);
						base.AddTrack(base._game.generatorPosition.X + 1, base._game.generatorPosition.Y, Track.TrackType.MushroomRight);
					}
					Track track = base.AddTrack(base._game.generatorPosition.X, base._game.generatorPosition.Y, track_type);
					if (width == hop_width - 1 && i < this.numberOfHops - 1 && base._game.generatorPosition.Y > 4)
					{
						spring = base._game.AddEntity(new MushroomSpring());
						spring.InitializeObstacle(track);
						spring.position.X = track.position.X + (float)(base._game.tileSize / 2);
						spring.position.Y = track.GetYAtPoint(spring.position.X);
					}
				}
			}
			base._game.generatorPosition.X++;
		}
	}

	public class BunnyHopGenerator : BaseTrackGenerator
	{
		protected int numberOfHops;

		protected int minHops = 1;

		protected int maxHops = 5;

		protected int minHopSize = 1;

		protected int maxHopSize = 1;

		protected float releaseJumpChance;

		protected List<int> staggerPattern;

		protected Track.TrackType trackType;

		public BunnyHopGenerator SetTrackType(Track.TrackType track_type)
		{
			this.trackType = track_type;
			return this;
		}

		public BunnyHopGenerator SetStaggerValues(params int[] args)
		{
			this.staggerPattern = new List<int>();
			for (int i = 0; i < args.Length; i++)
			{
				this.staggerPattern.Add(args[i]);
			}
			return this;
		}

		public BunnyHopGenerator SetReleaseJumpChance(float chance)
		{
			this.releaseJumpChance = chance;
			return this;
		}

		public BunnyHopGenerator SetHopSize(int min, int max)
		{
			this.minHopSize = min;
			this.maxHopSize = max;
			return this;
		}

		public BunnyHopGenerator SetNumberOfHops(int min, int max)
		{
			this.minHops = min;
			this.maxHops = max;
			return this;
		}

		public BunnyHopGenerator(MineCart game)
			: base(game)
		{
			this.minHopSize = 1;
			this.maxHopSize = 1;
			this.staggerPattern = new List<int>();
		}

		public override void Initialize()
		{
			this.numberOfHops = Game1.random.Next(this.minHops, this.maxHops + 1);
			base.Initialize();
		}

		protected override void _GenerateTrack()
		{
			if (base._game.generatorPosition.X >= base._game.distanceToTravel)
			{
				return;
			}
			base._game.trackBuilderCharacter.enabled = true;
			for (int i = 0; i < this.numberOfHops; i++)
			{
				base._game.trackBuilderCharacter.position.X = ((float)base._game.generatorPosition.X - 1f + 0.5f) * (float)base._game.tileSize;
				base._game.trackBuilderCharacter.position.Y = base._game.generatorPosition.Y * base._game.tileSize;
				base._game.trackBuilderCharacter.ForceGrounded();
				base._game.trackBuilderCharacter.Jump();
				base._game.trackBuilderCharacter.Update(0.03f);
				int target_y = base._game.generatorPosition.Y;
				if (this.staggerPattern != null && this.staggerPattern.Count > 0)
				{
					target_y += Game1.random.ChooseFrom(this.staggerPattern);
				}
				target_y = base._game.KeepTileInBounds(target_y);
				bool has_landed = false;
				while (!has_landed)
				{
					if (base._game.trackBuilderCharacter.position.Y < (float)(target_y * base._game.tileSize) && Math.Abs(Math.Round(base._game.trackBuilderCharacter.position.X / (float)base._game.tileSize) - (double)base._game.generatorPosition.X) > 1.0 && base._game.trackBuilderCharacter.IsJumping() && Game1.random.NextDouble() < (double)this.releaseJumpChance)
					{
						base._game.trackBuilderCharacter.ReleaseJump();
					}
					Vector2 old_position = base._game.trackBuilderCharacter.position;
					float y = base._game.trackBuilderCharacter.velocity.Y;
					base._game.trackBuilderCharacter.Update(0.03f);
					if (y < 0f && base._game.trackBuilderCharacter.velocity.Y >= 0f)
					{
						base._game.CreatePickup(base._game.trackBuilderCharacter.position + new Vector2(0f, 8f));
					}
					if (old_position.Y < (float)(target_y * base._game.tileSize) && base._game.trackBuilderCharacter.position.Y >= (float)(target_y * base._game.tileSize))
					{
						has_landed = true;
					}
					if (base._game.trackBuilderCharacter.IsGrounded() || base._game.trackBuilderCharacter.position.Y / (float)base._game.tileSize > (float)base._game.bottomTile)
					{
						base._game.trackBuilderCharacter.position = old_position;
						if (!base._game.IsTileInBounds(target_y))
						{
							return;
						}
						target_y = base._game.KeepTileInBounds((int)(old_position.Y / (float)base._game.tileSize));
						break;
					}
				}
				base._game.generatorPosition.Y = target_y;
				int hop_width = Game1.random.Next(this.minHopSize, this.maxHopSize + 1);
				Track.TrackType track_type = this.trackType;
				if (i >= this.numberOfHops - 1)
				{
					track_type = Track.TrackType.Straight;
				}
				for (int width = 0; width < hop_width; width++)
				{
					base._game.generatorPosition.X = (int)(base._game.trackBuilderCharacter.position.X / (float)base._game.tileSize) + width;
					if (base._game.generatorPosition.X >= base._game.distanceToTravel)
					{
						return;
					}
					if (track_type == Track.TrackType.MushroomMiddle)
					{
						base.AddTrack(base._game.generatorPosition.X - 1, base._game.generatorPosition.Y, Track.TrackType.MushroomLeft);
						base.AddTrack(base._game.generatorPosition.X + 1, base._game.generatorPosition.Y, Track.TrackType.MushroomRight);
					}
					base.AddPickupTrack(base._game.generatorPosition.X, base._game.generatorPosition.Y, track_type);
				}
			}
			base._game.generatorPosition.X++;
		}
	}

	public class BaseTrackGenerator
	{
		public const int OBSTACLE_NONE = -10;

		public const int OBSTACLE_MIDDLE = -10;

		public const int OBSTACLE_FRONT = -11;

		public const int OBSTACLE_BACK = -12;

		public const int OBSTACLE_RANDOM = -13;

		protected List<Track> _generatedTracks;

		protected MineCart _game;

		protected Dictionary<int, KeyValuePair<ObstacleTypes, float>> _obstacleIndices = new Dictionary<int, KeyValuePair<ObstacleTypes, float>>();

		protected Func<Track, BaseTrackGenerator, bool> _pickupFunction;

		public static bool FlatsOnly(Track track, BaseTrackGenerator generator)
		{
			return track.trackType == Track.TrackType.None;
		}

		public static bool UpSlopesOnly(Track track, BaseTrackGenerator generator)
		{
			return track.trackType == Track.TrackType.UpSlope;
		}

		public static bool DownSlopesOnly(Track track, BaseTrackGenerator generator)
		{
			return track.trackType == Track.TrackType.DownSlope;
		}

		public static bool IceDownSlopesOnly(Track track, BaseTrackGenerator generator)
		{
			return track.trackType == Track.TrackType.IceDownSlope;
		}

		public static bool Always(Track track, BaseTrackGenerator generator)
		{
			return true;
		}

		public static bool EveryOtherTile(Track track, BaseTrackGenerator generator)
		{
			if ((int)(track.position.X / 16f) % 2 == 0)
			{
				return true;
			}
			return false;
		}

		public T AddObstacle<T>(ObstacleTypes obstacle_type, int position, float obstacle_chance = 1f) where T : BaseTrackGenerator
		{
			this._obstacleIndices.Add(position, new KeyValuePair<ObstacleTypes, float>(obstacle_type, obstacle_chance));
			return this as T;
		}

		public T AddPickupFunction<T>(Func<Track, BaseTrackGenerator, bool> pickup_spawn_function) where T : BaseTrackGenerator
		{
			this._pickupFunction = (Func<Track, BaseTrackGenerator, bool>)Delegate.Combine(this._pickupFunction, pickup_spawn_function);
			return this as T;
		}

		public BaseTrackGenerator(MineCart game)
		{
			this._game = game;
		}

		public Track AddTrack(int x, int y, Track.TrackType track_type = Track.TrackType.Straight)
		{
			Track track = this._game.AddTrack(x, y, track_type);
			this._generatedTracks.Add(track);
			return track;
		}

		public Track AddTrack(Track track)
		{
			this._game.AddTrack(track);
			this._generatedTracks.Add(track);
			return track;
		}

		public Track AddPickupTrack(int x, int y, Track.TrackType track_type = Track.TrackType.Straight)
		{
			Track track = this.AddTrack(x, y, track_type);
			if (this._pickupFunction == null)
			{
				return track;
			}
			Delegate[] invocationList = this._pickupFunction.GetInvocationList();
			for (int i = 0; i < invocationList.Length; i++)
			{
				if (!((Func<Track, BaseTrackGenerator, bool>)invocationList[i])(track, this))
				{
					return track;
				}
			}
			Pickup pickup = this._game.CreatePickup(track.position + new Vector2(8f, -this._game.tileSize));
			if (pickup != null && (track.trackType == Track.TrackType.DownSlope || track.trackType == Track.TrackType.UpSlope || track.trackType == Track.TrackType.IceDownSlope || track.trackType == Track.TrackType.SlimeUpSlope))
			{
				pickup.position += new Vector2(0f, (float)(-this._game.tileSize) * 0.75f);
			}
			return track;
		}

		public virtual void Initialize()
		{
			this._generatedTracks = new List<Track>();
		}

		public void GenerateTrack()
		{
			this._GenerateTrack();
			this.PopulateObstacles();
		}

		public void PopulateObstacles()
		{
			if (this._game.generatorPosition.X >= this._game.distanceToTravel || this._generatedTracks.Count == 0)
			{
				return;
			}
			this._generatedTracks.OrderBy((Track o) => o.position.X);
			if (this._obstacleIndices == null || this._obstacleIndices.Count == 0)
			{
				return;
			}
			foreach (int index in this._obstacleIndices.Keys)
			{
				if (Game1.random.NextBool(this._obstacleIndices[index].Value))
				{
					int track_index = index switch
					{
						-12 => this._generatedTracks.Count - 1, 
						-11 => 0, 
						-10 => (this._generatedTracks.Count - 1) / 2, 
						-13 => Game1.random.Next(this._generatedTracks.Count), 
						_ => index, 
					};
					Track track = this._generatedTracks[track_index];
					if (track != null && (int)(track.position.X / (float)this._game.tileSize) < this._game.distanceToTravel)
					{
						this._game.AddObstacle(track, this._obstacleIndices[index].Key);
					}
				}
			}
		}

		protected virtual void _GenerateTrack()
		{
			this._game.generatorPosition.X++;
		}
	}

	public class Spark
	{
		public float x;

		public float y;

		public Color c;

		public float dx;

		public float dy;

		public Spark(float x, float y, float dx, float dy)
		{
			this.x = x;
			this.y = y;
			this.dx = dx;
			this.dy = dy;
			this.c = Color.Yellow;
		}
	}

	public class Entity
	{
		public Vector2 position;

		protected MineCart _game;

		public bool visible = true;

		public bool enabled = true;

		protected bool _destroyed;

		public Vector2 drawnPosition => this.position - new Vector2(this._game.screenLeftBound, 0f);

		public virtual void OnPlayerReset()
		{
		}

		public bool IsOnScreen()
		{
			if (this.position.X < this._game.screenLeftBound - (float)(this._game.tileSize * 4))
			{
				return false;
			}
			if (this.position.X > this._game.screenLeftBound + (float)this._game.screenWidth + (float)(this._game.tileSize * 4))
			{
				return false;
			}
			return true;
		}

		public bool IsActive()
		{
			if (this._destroyed)
			{
				return false;
			}
			if (!this.enabled)
			{
				return false;
			}
			return true;
		}

		public void Initialize(MineCart game)
		{
			this._game = game;
			this._Initialize();
		}

		public void Destroy()
		{
			this._destroyed = true;
		}

		protected virtual void _Initialize()
		{
		}

		public virtual bool ShouldReap()
		{
			return this._destroyed;
		}

		public void Draw(SpriteBatch b)
		{
			if (!this._destroyed && this.visible && this.enabled)
			{
				this._Draw(b);
			}
		}

		public virtual void _Draw(SpriteBatch b)
		{
		}

		public void Update(float time)
		{
			if (!this._destroyed && this.enabled)
			{
				this._Update(time);
			}
		}

		protected virtual void _Update(float time)
		{
		}
	}

	public class BaseCharacter : Entity
	{
		public Vector2 velocity;
	}

	public interface ICollideable
	{
		Rectangle GetLocalBounds();

		Rectangle GetBounds();
	}

	public class Bubble : Obstacle
	{
		public Vector2 _normalizedVelocity;

		public float moveSpeed = 8f;

		protected float _age;

		protected int _currentFrame;

		protected float _timePerFrame = 0.5f;

		protected int[] _frames = new int[6] { 0, 1, 2, 3, 3, 2 };

		protected int _repeatedFrameCount = 4;

		protected float _lifeTime = 3f;

		public Vector2 bubbleOffset = Vector2.Zero;

		public override void OnPlayerReset()
		{
			base.Destroy();
		}

		public override Rectangle GetBounds()
		{
			Rectangle bounds = base.GetBounds();
			bounds.X += (int)this.bubbleOffset.X;
			bounds.Y += (int)this.bubbleOffset.Y;
			return base.GetBounds();
		}

		public Bubble(float angle, float speed)
		{
			this._normalizedVelocity.X = (float)Math.Cos(angle * (float)Math.PI / 180f);
			this._normalizedVelocity.Y = 0f - (float)Math.Sin(angle * (float)Math.PI / 180f);
			this.moveSpeed = speed;
			this._age = 0f;
		}

		public override bool OnBump(PlayerMineCartCharacter player)
		{
			this.Pop();
			return base.OnBump(player);
		}

		public override bool OnBounce(MineCartCharacter player)
		{
			if (!(player is PlayerMineCartCharacter))
			{
				return false;
			}
			player.Bounce();
			this.Pop();
			return true;
		}

		public void Pop(bool play_sound = true)
		{
			if (play_sound)
			{
				Game1.playSound("dropItemInWater");
			}
			base.Destroy();
			base._game.AddEntity(new MineDebris(new Rectangle(32, 240, 16, 16), new Vector2(this.GetBounds().Center.X, this.GetBounds().Center.Y), 0f, 0f, 0f, 0f, 0.4f, 1f, 2, 0.2f));
		}

		protected override void _Update(float time)
		{
			base.position += this.moveSpeed * this._normalizedVelocity * time;
			this._age += time;
			this._currentFrame = (int)(this._age / this._timePerFrame);
			if (this._currentFrame >= this._frames.Length)
			{
				this._currentFrame -= this._frames.Length;
				this._currentFrame %= this._repeatedFrameCount;
				this._currentFrame += this._frames.Length - this._repeatedFrameCount;
			}
			this.bubbleOffset.X = (float)Math.Cos(this._age * 10f) * 4f;
			this.bubbleOffset.Y = (float)Math.Sin(this._age * 10f) * 4f;
			if (this._age >= this._lifeTime)
			{
				this.Pop(play_sound: false);
			}
			base._Update(time);
		}

		public override void _Draw(SpriteBatch b)
		{
			b.Draw(base._game.texture, base._game.TransformDraw(base.drawnPosition + this.bubbleOffset), new Rectangle(this._frames[this._currentFrame] * 16, 256, 16, 16), Color.White, 0f, new Vector2(8f, 16f), base._game.GetPixelScale(), SpriteEffects.None, 0.27f);
		}
	}

	public class PlayerBubbleSpawner : Entity
	{
		public int bubbleCount;

		public float timer;

		protected override void _Update(float time)
		{
			base.position = base._game.player.position;
			this.timer -= time;
			if (base._game.player.velocity.Y > 0f && this.bubbleCount == 0)
			{
				this.bubbleCount = 1;
				this.timer = Utility.Lerp(0.05f, 0.25f, (float)Game1.random.NextDouble());
			}
			if (this.timer <= 0f && this.bubbleCount <= 0)
			{
				this.bubbleCount = Game1.random.Next(1, 4);
				this.timer = Utility.Lerp(0.15f, 0.25f, (float)Game1.random.NextDouble());
			}
			else if (this.timer <= 0f)
			{
				this.bubbleCount--;
				base._game.AddEntity(new MineDebris(new Rectangle(0, 256, 16, 16), base.position + new Vector2(0f - base._game.player.characterExtraHeight - 16f) / 2f, -10f, 10f, 0f, -1f, 1.5f, 0.5f, 4, 0.1f, 0.45f, holdLastFrame: true));
				if (this.bubbleCount == 0)
				{
					this.timer = Utility.Lerp(1f, 1.5f, (float)Game1.random.NextDouble());
				}
				else
				{
					this.timer = Utility.Lerp(0.15f, 0.25f, (float)Game1.random.NextDouble());
				}
			}
		}
	}

	public class Whale : Entity
	{
		public enum CurrentState
		{
			Idle,
			OpenMouth,
			FireBubbles,
			CloseMouth
		}

		protected CurrentState _currentState;

		protected float _stateTimer;

		public float mouthCloseTime = 1f;

		protected float _nextFire;

		protected int _currentFrame;

		protected Vector2 _basePosition;

		public void SetState(CurrentState new_state, float state_timer = 1f)
		{
			this._currentState = new_state;
			this._stateTimer = state_timer;
		}

		public override void OnPlayerReset()
		{
			this._currentState = CurrentState.Idle;
			this._stateTimer = 2f;
		}

		protected override void _Update(float time)
		{
			base._Update(time);
			this._basePosition.Y = Utility.MoveTowards(this._basePosition.Y, base._game.player.position.Y + 32f, 48f * time);
			base.position.X = base._game.screenLeftBound - 128f + (float)base._game.screenWidth + (float)Math.Cos(base._game.totalTime * Math.PI / 2.299999952316284) * 24f;
			base.position.Y = this._basePosition.Y + (float)Math.Sin(base._game.totalTime * Math.PI / 3.0) * 32f;
			if (base.position.Y > (float)base._game.screenHeight)
			{
				base.position.Y = base._game.screenHeight;
			}
			if (base.position.Y < 120f)
			{
				base.position.Y = 120f;
			}
			this._stateTimer -= time;
			switch (this._currentState)
			{
			case CurrentState.Idle:
				this._currentFrame = 0;
				if (this._stateTimer < 0f && base._game.gameState != GameStates.Cutscene)
				{
					this._currentState = CurrentState.OpenMouth;
					this._stateTimer = this.mouthCloseTime;
					Game1.playSound("croak");
				}
				break;
			case CurrentState.OpenMouth:
				this._currentFrame = (int)Utility.Lerp(3f, 0f, this._stateTimer / this.mouthCloseTime);
				if (this._stateTimer < 0f)
				{
					this._currentState = CurrentState.FireBubbles;
					this._stateTimer = 4f;
				}
				this._nextFire = 0f;
				break;
			case CurrentState.FireBubbles:
				this._currentFrame = 3;
				this._nextFire -= time;
				if (this._nextFire <= 0f)
				{
					Game1.playSound("dwop");
					this._nextFire = 1f;
					float shoot_speed = 32f;
					float shoot_spread = 45f;
					if ((float)base._game.generatorPosition.X >= (float)base._game.distanceToTravel / 2f)
					{
						shoot_speed = Utility.Lerp(32f, 64f, (float)Game1.random.NextDouble());
						shoot_spread = 60f;
					}
					base._game.AddEntity(new Bubble(180f + Utility.Lerp(0f - shoot_spread, shoot_spread, (float)Game1.random.NextDouble()), shoot_speed)).position = base.position + new Vector2(48f, -40f);
					base._game.AddEntity(new MineDebris(new Rectangle(0, 256, 16, 16), base.position + new Vector2(96f, -100f), -10f, 10f, 0f, -1f, 1f, 0.5f, 4, 0.25f));
				}
				if (this._stateTimer < 0f)
				{
					this._currentState = CurrentState.CloseMouth;
					this._stateTimer = this.mouthCloseTime;
				}
				break;
			case CurrentState.CloseMouth:
				this._currentFrame = (int)Utility.Lerp(0f, 3f, this._stateTimer / this.mouthCloseTime);
				if (this._stateTimer < 0f)
				{
					this._currentState = CurrentState.Idle;
					this._stateTimer = 2f;
				}
				break;
			}
		}

		protected override void _Initialize()
		{
			this._currentState = CurrentState.Idle;
			this._stateTimer = Utility.Lerp(1f, 2f, (float)Game1.random.NextDouble());
			this._basePosition.Y = base._game.screenHeight / 2 + 56;
			base._Initialize();
		}

		public override void _Draw(SpriteBatch b)
		{
			Point source_rect_offset = default(Point);
			Point draw_offset = default(Point);
			if (this._currentFrame > 0)
			{
				source_rect_offset.X = 85 * (this._currentFrame - 1) + 1;
				source_rect_offset.Y = 112;
				draw_offset.X = 3;
				draw_offset.Y = -3;
			}
			b.Draw(base._game.texture, base._game.TransformDraw(base.drawnPosition + new Vector2(85f, 0f)), new Rectangle(86, 288, 75, 112), Color.White, 0f, new Vector2(0f, 112f), base._game.GetPixelScale(), SpriteEffects.None, 0.29f);
			b.Draw(base._game.texture, base._game.TransformDraw(base.drawnPosition + Utility.PointToVector2(draw_offset)), new Rectangle(source_rect_offset.X, 288 + source_rect_offset.Y, 85, 112), Color.White, 0f, new Vector2(0f, 112f), base._game.GetPixelScale(), SpriteEffects.None, 0.28f);
		}
	}

	public class EndingJunimo : Entity
	{
		protected Color _color;

		protected Vector2 _velocity;

		private bool _special;

		public EndingJunimo(bool special = false)
		{
			this._special = special;
		}

		protected override void _Initialize()
		{
			if (this._special || Game1.random.NextDouble() < 0.01)
			{
				switch (Game1.random.Next(8))
				{
				case 0:
					this._color = Color.Red;
					break;
				case 1:
					this._color = Color.Goldenrod;
					break;
				case 2:
					this._color = Color.Yellow;
					break;
				case 3:
					this._color = Color.Lime;
					break;
				case 4:
					this._color = new Color(0, 255, 180);
					break;
				case 5:
					this._color = new Color(0, 100, 255);
					break;
				case 6:
					this._color = Color.MediumPurple;
					break;
				case 7:
					this._color = Color.Salmon;
					break;
				}
				if (Game1.random.NextDouble() < 0.01)
				{
					this._color = Color.White;
				}
			}
			else
			{
				switch (Game1.random.Next(8))
				{
				case 0:
					this._color = Color.LimeGreen;
					break;
				case 1:
					this._color = Color.Orange;
					break;
				case 2:
					this._color = Color.LightGreen;
					break;
				case 3:
					this._color = Color.Tan;
					break;
				case 4:
					this._color = Color.GreenYellow;
					break;
				case 5:
					this._color = Color.LawnGreen;
					break;
				case 6:
					this._color = Color.PaleGreen;
					break;
				case 7:
					this._color = Color.Turquoise;
					break;
				}
			}
			this._velocity.X = Utility.RandomFloat(-10f, -40f);
			this._velocity.Y = Utility.RandomFloat(-20f, -60f);
		}

		protected override void _Update(float time)
		{
			base.position += time * this._velocity;
			this._velocity.Y += 210f * time;
			float floor_y = base._game.GetTrackForXPosition(base.position.X).position.Y;
			if (base.position.Y >= floor_y)
			{
				if (Game1.random.NextDouble() < 0.10000000149011612)
				{
					Game1.playSound("junimoMeep1");
				}
				base.position.Y = floor_y;
				this._velocity.Y = Utility.RandomFloat(-50f, -90f);
				if (base.position.X < base._game.player.position.X)
				{
					this._velocity.X = Utility.RandomFloat(10f, 40f);
				}
				if (base.position.X > base._game.player.position.X)
				{
					this._velocity.X = Utility.RandomFloat(10f, 40f) * -1f;
				}
			}
		}

		public override void _Draw(SpriteBatch b)
		{
			b.Draw(Game1.mouseCursors, base._game.TransformDraw(base.drawnPosition), new Rectangle(294 + (int)(base._game.totalTimeMS % 400.0) / 100 * 16, 1432, 16, 16), this._color, 0f, new Vector2(8f, 16f), base._game.GetPixelScale() * 2f / 3f, SpriteEffects.None, 0.25f);
		}
	}

	public class FallingBoulderSpawner : Obstacle
	{
		public float period = 2.33f;

		public float currentTime;

		protected Track _track;

		public override Rectangle GetLocalBounds()
		{
			return new Rectangle(0, 0, 0, 0);
		}

		public override Rectangle GetBounds()
		{
			return new Rectangle(0, 0, 0, 0);
		}

		public override void InitializeObstacle(Track track)
		{
			this._track = track;
			this.currentTime = (float)Game1.random.NextDouble() * this.period;
			base.position.Y = -32f;
		}

		protected override void _Update(float time)
		{
			base._Update(time);
			this.currentTime += time;
			if (this.currentTime >= this.period)
			{
				this.currentTime = 0f;
				FallingBoulder fallingBoulder = base._game.AddEntity(new FallingBoulder());
				fallingBoulder.position = base.position;
				fallingBoulder.InitializeObstacle(this._track);
			}
		}
	}

	public class WillOWisp : Obstacle
	{
		protected float _age;

		protected Vector2 offset;

		public float tailRotation;

		public float tailLength;

		public float scale = 1f;

		public float nextDebris = 0.1f;

		public override Rectangle GetBounds()
		{
			Rectangle bounds = base.GetBounds();
			bounds.X += (int)this.offset.X;
			bounds.Y += (int)this.offset.Y;
			return bounds;
		}

		public override Rectangle GetLocalBounds()
		{
			return new Rectangle(-5, -5, 10, 10);
		}

		protected override void _Update(float time)
		{
			this._age += time;
			Vector2 old_offset = this.offset;
			float interval = 15f;
			this.offset.Y = (float)(Math.Sin(this._age * interval * (float)Math.PI / 180f) - 1.0) * 32f;
			this.offset.X = (float)Math.Cos(this._age * interval * 3f * (float)Math.PI / 180f) * 64f;
			this.offset.Y += (float)Math.Sin(this._age * interval * 6f * (float)Math.PI / 180f) * 16f;
			Vector2 delta = this.offset - old_offset;
			this.tailRotation = (float)Math.Atan2(delta.Y, delta.X);
			this.tailLength = delta.Length();
			this.scale = Utility.Lerp(0.5f, 0.6f, (float)Math.Sin(this._age * 200f * (float)Math.PI / 180f) + 0.5f);
			this.nextDebris -= time;
			if (this.nextDebris <= 0f)
			{
				this.nextDebris = 0.1f;
				base._game.AddEntity(new MineDebris(new Rectangle(192, 96, 16, 16), new Vector2(this.GetBounds().Center.X, this.GetBounds().Bottom) + new Vector2(Game1.random.Next(-4, 5), Game1.random.Next(-4, 5)), Game1.random.Next(-30, 31), Game1.random.Next(-30, -19), 0.25f, -0.15f, 1f, 1f, 4, 0.25f, 0.46f)).visible = base.visible;
			}
		}

		public override bool OnBump(PlayerMineCartCharacter player)
		{
			base.Destroy();
			Game1.playSound("ghost");
			for (int i = 0; i < 8; i++)
			{
				base._game.AddEntity(new MineDebris(new Rectangle(192, 96, 16, 16), new Vector2(this.GetBounds().Center.X, this.GetBounds().Bottom) + new Vector2(Game1.random.Next(-4, 5), Game1.random.Next(-4, 5)), Game1.random.Next(-50, 51), Game1.random.Next(-50, 51), 0.25f, -0.15f, 1f, 1f, 4, 0.25f, 0.28f));
			}
			return base.OnBump(player);
		}

		public override void _Draw(SpriteBatch b)
		{
			b.Draw(base._game.texture, base._game.TransformDraw(base.drawnPosition + this.offset), new Rectangle(192, 80, 16, 16), Color.White, this._age * 200f * ((float)Math.PI / 180f), new Vector2(8f, 8f), base._game.GetPixelScale() * this.scale, SpriteEffects.None, 0.27f);
			b.Draw(base._game.texture, base._game.TransformDraw(base.drawnPosition + this.offset), new Rectangle(160, 112, 32, 32), Color.White, this._age * 60f * ((float)Math.PI / 180f), new Vector2(16f, 16f), base._game.GetPixelScale(), SpriteEffects.None, 0.29f);
			if (this._age > 0.25f)
			{
				Vector2 tail_scale = new Vector2(this.tailLength, this.scale);
				if (this.tailLength > 0.5f)
				{
					b.Draw(base._game.texture, base._game.TransformDraw(base.drawnPosition + this.offset), new Rectangle(208 + (int)(this._age / 0.1f) % 3 * 16, 80, 16, 16), Color.White, this.tailRotation, new Vector2(16f, 8f), tail_scale * base._game.GetPixelScale(), SpriteEffects.None, 0.44f);
				}
			}
		}
	}

	public class CosmeticFallingBoulder : FallingBoulder
	{
		private float yBreakPosition;

		private float delayBeforeAppear;

		private Color color;

		public CosmeticFallingBoulder(float yBreakPosition, Color color, float fallSpeed = 96f, float delayBeforeAppear = 0f)
		{
			this.yBreakPosition = yBreakPosition;
			this.color = color;
			base._fallSpeed = fallSpeed;
			this.delayBeforeAppear = delayBeforeAppear;
			if (delayBeforeAppear > 0f)
			{
				base.visible = false;
			}
		}

		protected override void _Update(float time)
		{
			if (this.delayBeforeAppear > 0f)
			{
				this.delayBeforeAppear -= time;
				if (!(this.delayBeforeAppear <= 0f))
				{
					return;
				}
				base.visible = true;
			}
			base._age += time;
			if (base.position.Y >= this.yBreakPosition)
			{
				base._currentFallSpeed = -30f;
				if (base.IsOnScreen())
				{
					Game1.playSound("hammer");
				}
				for (int i = 0; i < 3; i++)
				{
					base._game.AddEntity(new MineDebris(new Rectangle(16, 80, 16, 16), new Vector2(this.GetBounds().Center.X, this.GetBounds().Bottom), Game1.random.Next(-30, 31), Game1.random.Next(-30, -19), 0.25f)).SetColor(base._game.caveTint);
				}
				base._destroyed = true;
			}
			if (base._currentFallSpeed < base._fallSpeed)
			{
				base._currentFallSpeed += 210f * time;
				if (base._currentFallSpeed > base._fallSpeed)
				{
					base._currentFallSpeed = base._fallSpeed;
				}
			}
			base.position.Y += time * base._currentFallSpeed;
		}

		public override void _Draw(SpriteBatch b)
		{
			SpriteEffects effect = SpriteEffects.None;
			if (Math.Floor(base._age / 0.5f) % 2.0 == 0.0)
			{
				effect = SpriteEffects.FlipHorizontally;
			}
			b.Draw(base._game.texture, base._game.TransformDraw(base.drawnPosition), new Rectangle(0, 32, 16, 16), this.color, 0f, new Vector2(8f, 16f), base._game.GetPixelScale(), effect, 0.15f);
		}
	}

	public class NoxiousGas : Obstacle
	{
		protected float _age;

		protected float _currentRiseSpeed;

		protected float _riseSpeed = -90f;

		public override void OnPlayerReset()
		{
			base.Destroy();
		}

		public override void _Draw(SpriteBatch b)
		{
			SpriteEffects effect = SpriteEffects.None;
			if (Math.Floor(this._age / 0.5f) % 2.0 == 0.0)
			{
				effect = SpriteEffects.FlipHorizontally;
			}
			b.Draw(base._game.texture, base._game.TransformDraw(base.drawnPosition), new Rectangle(368, 784, 16, 16), Color.White, 0f, new Vector2(8f, 16f), base._game.GetPixelScale() * Utility.Clamp(this._age / 0.5f, 0f, 1f), effect, 0.44f);
		}

		protected override void _Update(float time)
		{
			this._age += time;
			if (this._currentRiseSpeed > this._riseSpeed)
			{
				this._currentRiseSpeed -= 40f * time;
				if (this._currentRiseSpeed < this._riseSpeed)
				{
					this._currentRiseSpeed = this._riseSpeed;
				}
			}
			base.position.Y += time * this._currentRiseSpeed;
		}

		public override bool OnBounce(MineCartCharacter player)
		{
			return false;
		}

		public override bool ShouldReap()
		{
			if (base.position.Y < -32f)
			{
				return true;
			}
			return base.ShouldReap();
		}
	}

	public class FallingBoulder : Obstacle
	{
		protected float _age;

		protected List<Track> _tracks;

		protected float _currentFallSpeed;

		protected float _fallSpeed = 96f;

		protected bool _wasBouncedOn;

		public override void OnPlayerReset()
		{
			base.Destroy();
		}

		public override void InitializeObstacle(Track track)
		{
			base.InitializeObstacle(track);
			List<Track> tracks = base._game.GetTracksForXPosition(base.position.X);
			if (tracks != null)
			{
				this._tracks = new List<Track>(tracks);
			}
		}

		public override void _Draw(SpriteBatch b)
		{
			SpriteEffects effect = SpriteEffects.None;
			if (Math.Floor(this._age / 0.5f) % 2.0 == 0.0)
			{
				effect = SpriteEffects.FlipHorizontally;
			}
			b.Draw(base._game.texture, base._game.TransformDraw(base.drawnPosition), new Rectangle(0, 32, 16, 16), base._game.caveTint, 0f, new Vector2(8f, 16f), base._game.GetPixelScale(), effect, 0.45f);
		}

		protected override void _Update(float time)
		{
			this._age += time;
			if (this._tracks != null && this._tracks.Count > 0)
			{
				if (this._tracks[0] == null)
				{
					this._tracks.RemoveAt(0);
				}
				else if (base.position.Y >= (float)this._tracks[0].GetYAtPoint(base.position.X))
				{
					this._currentFallSpeed = -30f;
					this._tracks.RemoveAt(0);
					if (base.IsOnScreen())
					{
						Game1.playSound("hammer");
					}
					for (int i = 0; i < 3; i++)
					{
						base._game.AddEntity(new MineDebris(new Rectangle(16, 80, 16, 16), new Vector2(this.GetBounds().Center.X, this.GetBounds().Bottom), Game1.random.Next(-30, 31), Game1.random.Next(-30, -19), 0.25f)).SetColor(base._game.caveTint);
					}
				}
			}
			if (this._currentFallSpeed < this._fallSpeed)
			{
				this._currentFallSpeed += 210f * time;
				if (this._currentFallSpeed > this._fallSpeed)
				{
					this._currentFallSpeed = this._fallSpeed;
				}
			}
			base.position.Y += time * this._currentFallSpeed;
		}

		public override bool OnBounce(MineCartCharacter player)
		{
			if (!(player is PlayerMineCartCharacter))
			{
				return false;
			}
			this._wasBouncedOn = true;
			player.Bounce();
			Game1.playSound("hammer");
			for (int i = 0; i < 3; i++)
			{
				base._game.AddEntity(new MineDebris(new Rectangle(16, 80, 16, 16), new Vector2(this.GetBounds().Center.X, this.GetBounds().Top), Game1.random.Next(-30, 31), Game1.random.Next(-30, -19), 0.25f)).SetColor(base._game.caveTint);
			}
			return true;
		}

		public override bool OnBump(PlayerMineCartCharacter player)
		{
			if (this._wasBouncedOn)
			{
				return true;
			}
			return base.OnBump(player);
		}

		public override bool ShouldReap()
		{
			if (base.position.Y > (float)(base._game.screenHeight + 32))
			{
				return true;
			}
			return base.ShouldReap();
		}
	}

	public class MineCartSlime : Obstacle
	{
		public override void _Draw(SpriteBatch b)
		{
			SpriteEffects effect = SpriteEffects.None;
			b.Draw(base._game.texture, base._game.TransformDraw(base.drawnPosition), new Rectangle(0, 32, 16, 16), base._game.caveTint, 0f, new Vector2(8f, 16f), base._game.GetPixelScale(), effect, 0.45f);
		}

		public override bool ShouldReap()
		{
			return false;
		}
	}

	public class SlimeTrack : Obstacle
	{
		public override void _Draw(SpriteBatch b)
		{
			SpriteEffects effect = SpriteEffects.None;
			b.Draw(base._game.texture, base._game.TransformDraw(base.drawnPosition), new Rectangle(0, 192, 32, 16), Color.White, 0f, new Vector2(8f, 16f), base._game.GetPixelScale(), effect, 0.45f);
		}

		public override bool ShouldReap()
		{
			return false;
		}
	}

	public class HugeSlime : Obstacle
	{
		protected float _timeUntilHop = 30f;

		protected float _yVelocity;

		protected bool _grounded;

		protected float _lastTrackY = 300f;

		public Vector2 spriteScale = new Vector2(1f, 1f);

		protected int _currentFrame;

		protected Vector2 _desiredScale = new Vector2(1f, 1f);

		protected float _scaleSpeed = 4f;

		protected float _jumpStrength = -200f;

		private bool _hasPeparedToJump;

		public override Rectangle GetLocalBounds()
		{
			return new Rectangle(-40, -60, 80, 60);
		}

		public override void OnPlayerReset()
		{
			base._game.slimeBossPosition = base._game.checkpointPosition + (float)base._game.slimeResetPosition;
		}

		protected override void _Initialize()
		{
			base._Initialize();
			base._game.slimeBossPosition = base._game.slimeResetPosition;
			this._grounded = false;
		}

		public override void _Draw(SpriteBatch b)
		{
			SpriteEffects effect = SpriteEffects.None;
			Rectangle source_rect = new Rectangle(160, 176, 96, 80);
			switch (this._currentFrame)
			{
			case 0:
				source_rect = new Rectangle(160, 176, 96, 80);
				break;
			case 1:
				source_rect = new Rectangle(160, 256, 96, 80);
				break;
			case 2:
				source_rect = new Rectangle(160, 336, 96, 64);
				break;
			}
			b.Draw(base._game.texture, base._game.TransformDraw(base.drawnPosition), source_rect, Color.White, 0f, new Vector2((float)source_rect.Width * 0.5f, source_rect.Height), base._game.GetPixelScale() * this.spriteScale, effect, 0.45f);
		}

		protected override void _Update(float time)
		{
			Track track = base._game.GetTrackForXPosition(base.position.X);
			float track_height = base._game.screenHeight + 32;
			if (track != null)
			{
				this._lastTrackY = track.GetYAtPoint(base.position.X);
				track_height = this._lastTrackY;
			}
			base._game.slimeBossPosition += base._game.slimeBossSpeed * time;
			if (this._grounded)
			{
				this._timeUntilHop -= time;
				if (this._timeUntilHop <= 0f)
				{
					this._grounded = false;
					this.spriteScale = new Vector2(1.1f, 0.75f);
					this._desiredScale = new Vector2(1f, 1f);
					this._scaleSpeed = 1f;
					this._yVelocity = this._jumpStrength;
					Game1.playSound("dwoop");
					for (int i = 0; i < 8; i++)
					{
						base._game.AddEntity(new MineDebris(new Rectangle(192, 112, 16, 16), new Vector2(this.GetBounds().Center.X, this.GetBounds().Bottom) + new Vector2(Game1.random.Next(-32, 33), Game1.random.Next(-32, 0)), Game1.random.Next(-10, 11), Game1.random.Next(-50, -29), 0.25f, 0.25f, 1f, 1f, 4, 0.25f, 0.46f));
					}
				}
				else if (this._timeUntilHop <= 0.25f)
				{
					if (!this._hasPeparedToJump)
					{
						this.spriteScale = new Vector2(0.9f, 1.1f);
						this._desiredScale = new Vector2(1f, 1f);
						this._scaleSpeed = 1f;
						this._currentFrame = 2;
						this._hasPeparedToJump = true;
					}
				}
				else
				{
					this._desiredScale = new Vector2(1f, 1f);
					this._scaleSpeed = 4f;
				}
			}
			else
			{
				this._currentFrame = 1;
				if (base.position.X > base._game.slimeBossPosition)
				{
					base.position.X = Utility.MoveTowards(base.position.X, base._game.slimeBossPosition, base._game.slimeBossSpeed * time * 8f);
				}
				else
				{
					base.position.X = Utility.MoveTowards(base.position.X, base._game.slimeBossPosition, base._game.slimeBossSpeed * time * 2f);
				}
				this._yVelocity += 200f * time;
				base.position.Y += this._yVelocity * time;
				if (base.position.Y > this._lastTrackY && this._yVelocity < 0f)
				{
					this._yVelocity = this._jumpStrength;
				}
				if (this._yVelocity < 0f)
				{
					this._desiredScale = new Vector2(0.9f, 1.1f);
					this._scaleSpeed = 5f;
				}
				else if (this._yVelocity > 0f)
				{
					this._desiredScale = new Vector2(1f, 1f);
					this._scaleSpeed = 0.25f;
				}
				if (base.position.Y > track_height && this._yVelocity > 0f)
				{
					Game1.playSound("slimedead");
					Game1.playSound("breakingGlass");
					for (int j = 0; j < 8; j++)
					{
						base._game.AddEntity(new MineDebris(new Rectangle(192, 112, 16, 16), new Vector2(this.GetBounds().Center.X, this.GetBounds().Bottom) + new Vector2(Game1.random.Next(-32, 33), Game1.random.Next(-32, 0)), Game1.random.Next(-80, 81), Game1.random.Next(-10, 1), 0.25f, 0.25f, 1f, 1f, 4, 0.25f, 0.46f));
					}
					base._game.shakeMagnitude = 1.5f;
					base.position.Y = track_height;
					this._grounded = true;
					this._timeUntilHop = 0.5f;
					this._currentFrame = 2;
					this._hasPeparedToJump = false;
					this.spriteScale = new Vector2(1.1f, 0.75f);
				}
			}
			this.spriteScale.X = Utility.MoveTowards(this.spriteScale.X, this._desiredScale.X, this._scaleSpeed * time);
			this.spriteScale.Y = Utility.MoveTowards(this.spriteScale.Y, this._desiredScale.Y, this._scaleSpeed * time);
		}

		public override bool ShouldReap()
		{
			return false;
		}
	}

	public class Roadblock : Obstacle
	{
		public override Rectangle GetLocalBounds()
		{
			return new Rectangle(-4, -12, 8, 12);
		}

		protected override void _Update(float time)
		{
		}

		public override void _Draw(SpriteBatch b)
		{
			b.Draw(base._game.texture, base._game.TransformDraw(base.drawnPosition), new Rectangle(16, 0, 16, 16), Color.White, 0f, new Vector2(8f, 16f), base._game.GetPixelScale(), SpriteEffects.None, 0.45f);
		}

		public override bool CanSpawnHere(Track track)
		{
			if (track == null)
			{
				return false;
			}
			if (track.trackType != Track.TrackType.Straight)
			{
				return false;
			}
			return true;
		}

		public override bool OnBounce(MineCartCharacter player)
		{
			if (!(player is PlayerMineCartCharacter))
			{
				return false;
			}
			this.ShootDebris(Game1.random.Next(-10, -4), Game1.random.Next(-60, -19));
			this.ShootDebris(Game1.random.Next(5, 11), Game1.random.Next(-60, -19));
			this.ShootDebris(Game1.random.Next(-20, -9), Game1.random.Next(-40, 0));
			this.ShootDebris(Game1.random.Next(10, 21), Game1.random.Next(-40, 0));
			Game1.playSound("woodWhack");
			player.velocity.Y = 0f;
			player.velocity.Y = 0f;
			base.Destroy();
			return true;
		}

		public override bool OnBump(PlayerMineCartCharacter player)
		{
			this.ShootDebris(Game1.random.Next(10, 41), Game1.random.Next(-40, 0));
			this.ShootDebris(Game1.random.Next(10, 41), Game1.random.Next(-40, 0));
			this.ShootDebris(Game1.random.Next(5, 31), Game1.random.Next(-60, -19));
			this.ShootDebris(Game1.random.Next(5, 31), Game1.random.Next(-60, -19));
			Game1.playSound("woodWhack");
			base.Destroy();
			return false;
		}

		public void ShootDebris(int x, int y)
		{
			base._game.AddEntity(new MineDebris(new Rectangle(48, 48, 16, 16), Utility.PointToVector2(this.GetBounds().Center), x, y, 0.25f, 1f, 1f));
		}
	}

	public class MineDebris : Entity
	{
		protected Rectangle _sourceRect;

		protected float _dX;

		protected float _dY;

		protected float _age;

		protected float _lifeTime;

		protected float _gravityMultiplier;

		protected float _scale = 1f;

		protected Color _color = Color.White;

		protected int _numAnimationFrames;

		protected bool _holdLastFrame;

		protected float _animationInterval;

		protected int _currentAnimationFrame;

		protected float _animationTimer;

		public float ySinWaveMagnitude;

		public float flipRate;

		public float depth = 0.45f;

		private float timeBeforeDisplay;

		private string destroySound;

		private string startSound;

		public MineDebris(Rectangle source_rect, Vector2 spawn_position, float dx, float dy, float flip_rate = 0f, float gravity_multiplier = 1f, float life_time = 0.5f, float scale = 1f, int num_animation_frames = 1, float animation_interval = 0.1f, float draw_depth = 0.45f, bool holdLastFrame = false, float timeBeforeDisplay = 0f)
		{
			this.reset(source_rect, spawn_position, dx, dy, flip_rate, gravity_multiplier, life_time, scale, num_animation_frames, animation_interval, draw_depth, holdLastFrame, timeBeforeDisplay);
		}

		public void reset(Rectangle source_rect, Vector2 spawn_position, float dx, float dy, float flip_rate = 0f, float gravity_multiplier = 1f, float life_time = 0.5f, float scale = 1f, int num_animation_frames = 1, float animation_interval = 0.1f, float draw_depth = 0.45f, bool holdLastFrame = false, float timeBeforeDisplay = 0f)
		{
			this._sourceRect = source_rect;
			this._dX = dx;
			this._dY = dy;
			this._lifeTime = life_time;
			this.flipRate = flip_rate;
			base.position = spawn_position;
			this._gravityMultiplier = gravity_multiplier;
			this._scale = scale;
			this._numAnimationFrames = num_animation_frames;
			this._animationInterval = animation_interval;
			this.depth = draw_depth;
			this._holdLastFrame = holdLastFrame;
			this._currentAnimationFrame = 0;
			this.timeBeforeDisplay = timeBeforeDisplay;
			if (timeBeforeDisplay > 0f)
			{
				base.visible = false;
			}
		}

		public void SetColor(Color color)
		{
			this._color = color;
		}

		public void SetDestroySound(string sound)
		{
			this.destroySound = sound;
		}

		public void SetStartSound(string sound)
		{
			this.startSound = sound;
		}

		protected override void _Update(float time)
		{
			if (this.timeBeforeDisplay > 0f)
			{
				this.timeBeforeDisplay -= time;
				if (!(this.timeBeforeDisplay <= 0f))
				{
					return;
				}
				base.visible = true;
				if (this.startSound != null)
				{
					Game1.playSound(this.startSound);
				}
			}
			base.position.X += this._dX * time;
			base.position.Y += this._dY * time;
			this._dY += 210f * time * this._gravityMultiplier;
			this._age += time;
			if (this._age >= this._lifeTime)
			{
				if (this.destroySound != null)
				{
					Game1.playSound(this.destroySound);
				}
				base.Destroy();
				return;
			}
			this._animationTimer += time;
			if (this._animationTimer >= this._animationInterval)
			{
				this._animationTimer = 0f;
				this._currentAnimationFrame++;
				if (this._holdLastFrame && this._currentAnimationFrame >= this._numAnimationFrames - 1)
				{
					this._currentAnimationFrame = this._numAnimationFrames - 1;
				}
				else
				{
					this._currentAnimationFrame %= this._numAnimationFrames;
				}
			}
			base._Update(time);
		}

		private Rectangle _GetSourceRect()
		{
			return new Rectangle(this._sourceRect.X + this._currentAnimationFrame * this._sourceRect.Width, this._sourceRect.Y, this._sourceRect.Width, this._sourceRect.Height);
		}

		public override void _Draw(SpriteBatch b)
		{
			SpriteEffects effect = SpriteEffects.None;
			if (this.flipRate > 0f && Math.Floor(this._age / this.flipRate) % 2.0 == 0.0)
			{
				effect = SpriteEffects.FlipHorizontally;
			}
			b.Draw(base._game.texture, base._game.TransformDraw(base.drawnPosition + new Vector2(0f, (float)Math.Sin(base._game.totalTime + (double)base.position.X) * this.ySinWaveMagnitude)), this._GetSourceRect(), this._color, 0f, new Vector2((float)this._sourceRect.Width / 2f, (float)this._sourceRect.Height / 2f), base._game.GetPixelScale() * this._scale, effect, this.depth);
		}
	}

	public class Obstacle : Entity, ICollideable
	{
		public virtual void InitializeObstacle(Track track)
		{
		}

		public virtual bool OnBounce(MineCartCharacter player)
		{
			return false;
		}

		public virtual bool OnBump(PlayerMineCartCharacter player)
		{
			return false;
		}

		public virtual Rectangle GetLocalBounds()
		{
			return new Rectangle(-4, -12, 8, 12);
		}

		public virtual Rectangle GetBounds()
		{
			Rectangle bounds = this.GetLocalBounds();
			bounds.X += (int)base.position.X;
			bounds.Y += (int)base.position.Y;
			return bounds;
		}

		public override void _Draw(SpriteBatch b)
		{
			b.Draw(base._game.texture, base._game.TransformDraw(base.drawnPosition), new Rectangle(16, 0, 16, 16), Color.White, 0f, new Vector2(8f, 16f), base._game.GetPixelScale(), SpriteEffects.None, 0.45f);
		}

		public virtual bool CanSpawnHere(Track track)
		{
			return true;
		}
	}

	public class Fruit : Pickup
	{
		protected CollectableFruits _fruitType;

		public override Rectangle GetLocalBounds()
		{
			return new Rectangle(-6, -6, 12, 12);
		}

		public Fruit(CollectableFruits fruit_type)
		{
			this._fruitType = fruit_type;
		}

		public override void Collect(PlayerMineCartCharacter player)
		{
			base._game.CollectFruit(this._fruitType);
			base._game.AddEntity(new MineDebris(new Rectangle(0, 250, 5, 5), base.position, 0f, 0f, 0f, 0f, 0.6f, 1f, 6));
			for (int i = 0; i < 4; i++)
			{
				float interval = Utility.Lerp(0.1f, 0.2f, (float)Game1.random.NextDouble());
				base._game.AddEntity(new MineDebris(new Rectangle(0, 250, 5, 5), base.position + new Vector2(Game1.random.Next(-8, 9), Game1.random.Next(-8, 9)), 0f, 0f, 0f, 0f, interval * 6f, 1f, 6, interval));
			}
			Game1.playSound("eat");
			base.Destroy();
		}

		public override void _Draw(SpriteBatch b)
		{
			b.Draw(base._game.texture, base._game.TransformDraw(base.drawnPosition), new Rectangle(160 + 16 * (int)this._fruitType, 0, 16, 16), Color.White, 0f, new Vector2(8f, 8f), base._game.GetPixelScale(), SpriteEffects.None, 0.43f);
		}
	}

	public class Coin : Pickup
	{
		public float age;

		public float afterCollectionTimer;

		public bool collected;

		public float flashSpeed = 0.25f;

		public float flashDelay = 0.5f;

		public float collectYDelta;

		protected override void _Update(float time)
		{
			this.age += time;
			if (this.age > this.flashDelay + this.flashSpeed * 3f)
			{
				this.age = 0f;
			}
			if (this.collected)
			{
				this.afterCollectionTimer += time;
				if (time > 0f)
				{
					base.position.Y -= 3f - this.afterCollectionTimer * 8f * time;
				}
				if (this.afterCollectionTimer > 0.4f)
				{
					base.Destroy();
				}
			}
			base._Update(time);
		}

		public override void _Draw(SpriteBatch b)
		{
			int time = (this.collected ? 450 : 900);
			b.Draw(base._game.texture, base._game.TransformDraw(base.drawnPosition), new Rectangle(9 * ((int)base._game.totalTimeMS % time / (time / 12)), 273, 9, 9), Color.White * (1f - this.afterCollectionTimer / 0.4f), 0f, new Vector2(4f, 4f), base._game.GetPixelScale(), SpriteEffects.None, 0.45f);
		}

		public override void Collect(PlayerMineCartCharacter player)
		{
			if (!this.collected)
			{
				base._game.CollectCoin(1);
				Game1.playSound("junimoKart_coin");
				base._game.AddEntity(new MineDebris(new Rectangle(0, 250, 5, 5), base.position, 0f, 0f, 0f, 0f, 0.6f, 1f, 6));
				for (int i = 0; i < 4; i++)
				{
					float interval = Utility.Lerp(0.1f, 0.2f, (float)Game1.random.NextDouble());
					base._game.AddEntity(new MineDebris(new Rectangle(0, 250, 5, 5), base.position + new Vector2(Game1.random.Next(-8, 9), Game1.random.Next(-8, 9)), 0f, 0f, 0f, 0f, interval * 6f, 1f, 6, interval));
				}
				this.collectYDelta = -3f;
				this.collected = true;
			}
		}
	}

	public class Pickup : Entity, ICollideable
	{
		public virtual Rectangle GetLocalBounds()
		{
			return new Rectangle(-4, -4, 8, 8);
		}

		public virtual Rectangle GetBounds()
		{
			Rectangle bounds = this.GetLocalBounds();
			bounds.X += (int)base.position.X;
			bounds.Y += (int)base.position.Y;
			return bounds;
		}

		public override void _Draw(SpriteBatch b)
		{
			b.Draw(base._game.texture, base._game.TransformDraw(base.drawnPosition), new Rectangle(16, 16, 16, 16), Color.White, 0f, new Vector2(8f, 8f), base._game.GetPixelScale(), SpriteEffects.None, 0.45f);
		}

		public virtual void Collect(PlayerMineCartCharacter player)
		{
			Game1.playSound("Pickup_Coin15");
			base.Destroy();
		}
	}

	public class BalanceTrack : Track
	{
		public List<BalanceTrack> connectedTracks;

		public List<BalanceTrack> counterBalancedTracks;

		public float startY;

		public float moveSpeed = 128f;

		public BalanceTrack(TrackType type, bool showSecondTile)
			: base(type, showSecondTile)
		{
			this.connectedTracks = new List<BalanceTrack>();
			this.counterBalancedTracks = new List<BalanceTrack>();
		}

		public override void OnPlayerReset()
		{
			base.position.Y = this.startY;
		}

		public override void WhileCartGrounded(MineCartCharacter character, float time)
		{
			foreach (BalanceTrack connectedTrack in this.connectedTracks)
			{
				connectedTrack.position.Y += this.moveSpeed * time;
			}
			foreach (BalanceTrack counterBalancedTrack in this.counterBalancedTracks)
			{
				counterBalancedTrack.position.Y -= this.moveSpeed * time;
			}
		}
	}

	public class Track : Entity
	{
		public enum TrackType
		{
			None = -1,
			Straight = 0,
			UpSlope = 2,
			DownSlope = 3,
			IceDownSlope = 4,
			SlimeUpSlope = 5,
			MushroomLeft = 6,
			MushroomMiddle = 7,
			MushroomRight = 8
		}

		public Obstacle obstacle;

		private bool _showSecondTile;

		public TrackType trackType;

		public Track(TrackType type, bool showSecondTile)
		{
			this.trackType = type;
			this._showSecondTile = showSecondTile;
		}

		public virtual void WhileCartGrounded(MineCartCharacter character, float time)
		{
		}

		public override void _Draw(SpriteBatch b)
		{
			if (this.trackType == TrackType.SlimeUpSlope)
			{
				b.Draw(base._game.texture, base._game.TransformDraw(new Vector2(base.drawnPosition.X, base.drawnPosition.Y - 32f)), new Rectangle(192, 144, 16, 32), base._game.trackTint, 0f, Vector2.Zero, base._game.GetPixelScale(), SpriteEffects.None, 0.5f + base.drawnPosition.Y * 1E-05f);
				b.Draw(base._game.texture, base._game.TransformDraw(new Vector2(base.drawnPosition.X, base.drawnPosition.Y - 32f)), new Rectangle(160 + (int)this.trackType * 16, 144, 16, 32), Color.White, 0f, Vector2.Zero, base._game.GetPixelScale(), SpriteEffects.None, 0.5f + base.drawnPosition.Y * 1E-05f - 0.0001f);
			}
			else if (this.trackType >= TrackType.MushroomLeft && this.trackType <= TrackType.MushroomRight)
			{
				if (base.GetType() == typeof(Track))
				{
					b.Draw(base._game.texture, base._game.TransformDraw(new Vector2(base.drawnPosition.X, base.drawnPosition.Y - 32f)), new Rectangle(304 + (int)(this.trackType - 6) * 16, 736, 16, 48), Color.White, 0f, Vector2.Zero, base._game.GetPixelScale(), SpriteEffects.None, 0.5f + base.drawnPosition.Y * 1E-05f);
				}
				else
				{
					b.Draw(base._game.texture, base._game.TransformDraw(new Vector2(base.drawnPosition.X, base.drawnPosition.Y - 32f)), new Rectangle(352 + (int)(this.trackType - 6) * 16, 736, 16, 48), Color.White, 0f, Vector2.Zero, base._game.GetPixelScale(), SpriteEffects.None, 0.5f + base.drawnPosition.Y * 1E-05f);
				}
			}
			else if (base._game.currentTheme == 4 && (this.trackType == TrackType.UpSlope || this.trackType == TrackType.DownSlope))
			{
				b.Draw(base._game.texture, base._game.TransformDraw(new Vector2(base.drawnPosition.X, base.drawnPosition.Y - 32f)), new Rectangle(256 + (int)(this.trackType - 2) * 16, 144, 16, 32), base._game.trackTint, 0f, Vector2.Zero, base._game.GetPixelScale(), SpriteEffects.None, 0.5f + base.drawnPosition.Y * 1E-05f);
			}
			else
			{
				b.Draw(base._game.texture, base._game.TransformDraw(new Vector2(base.drawnPosition.X, base.drawnPosition.Y - 32f)), new Rectangle(160 + (int)this.trackType * 16, 144, 16, 32), base._game.trackTint, 0f, Vector2.Zero, base._game.GetPixelScale(), SpriteEffects.None, 0.5f + base.drawnPosition.Y * 1E-05f);
			}
			if (this.trackType == TrackType.MushroomLeft || this.trackType == TrackType.MushroomRight)
			{
				return;
			}
			float darkness = 0f;
			if (this.trackType == TrackType.MushroomMiddle)
			{
				for (float y = base.drawnPosition.Y; y < (float)base._game.screenHeight; y += (float)(base._game.tileSize * 4))
				{
					b.Draw(base._game.texture, base._game.TransformDraw(new Vector2(base.drawnPosition.X, y + 16f)), new Rectangle(320, 784, 16, 64), Color.White, 0f, Vector2.Zero, base._game.GetPixelScale(), SpriteEffects.None, 0.5f + base.drawnPosition.Y * 1E-05f + 0.01f);
					b.Draw(base._game.texture, base._game.TransformDraw(new Vector2(base.drawnPosition.X, y + 16f)), new Rectangle(368, 784, 16, 64), base._game.trackShadowTint * darkness, 0f, Vector2.Zero, base._game.GetPixelScale(), SpriteEffects.None, 0.5f + base.drawnPosition.Y * 1E-05f + 0.005f);
					darkness += 0.1f;
				}
				return;
			}
			bool flipper = this._showSecondTile;
			for (float y2 = base.drawnPosition.Y; y2 < (float)base._game.screenHeight; y2 += (float)base._game.tileSize)
			{
				b.Draw(base._game.texture, base._game.TransformDraw(new Vector2(base.drawnPosition.X, y2)), (base._game.currentTheme == 4) ? new Rectangle(16 + (flipper ? 1 : 0) * 16, 160, 16, 16) : new Rectangle(16 + (flipper ? 1 : 0) * 16, 32, 16, 16), base._game.trackTint, 0f, Vector2.Zero, base._game.GetPixelScale(), SpriteEffects.None, 0.5f + base.drawnPosition.Y * 1E-05f + 0.01f);
				b.Draw(base._game.texture, base._game.TransformDraw(new Vector2(base.drawnPosition.X, y2)), (base._game.currentTheme == 4) ? new Rectangle(16 + (flipper ? 1 : 0) * 16, 160, 16, 16) : new Rectangle(16 + (flipper ? 1 : 0) * 16, 32, 16, 16), base._game.trackShadowTint * darkness, 0f, Vector2.Zero, base._game.GetPixelScale(), SpriteEffects.None, 0.5f + base.drawnPosition.Y * 1E-05f + 0.005f);
				darkness += 0.1f;
				flipper = !flipper;
			}
		}

		public bool CanLandHere(Vector2 test_position)
		{
			int track_y = this.GetYAtPoint(test_position.X);
			if (test_position.Y >= (float)(track_y - 2) && test_position.Y <= (float)(track_y + 8))
			{
				return true;
			}
			return false;
		}

		public int GetYAtPoint(float x)
		{
			int local_x = (int)(x - base.position.X);
			return this.trackType switch
			{
				TrackType.UpSlope => (int)(base.position.Y - 2f - (float)local_x), 
				TrackType.DownSlope => (int)(base.position.Y - 2f - 16f + (float)local_x), 
				TrackType.IceDownSlope => (int)(base.position.Y - 2f - 16f + (float)local_x), 
				TrackType.SlimeUpSlope => (int)(base.position.Y - 2f - (float)local_x), 
				_ => (int)(base.position.Y - 2f), 
			};
		}
	}

	public class PlayerMineCartCharacter : MineCartCharacter, ICollideable
	{
		public Rectangle GetLocalBounds()
		{
			return new Rectangle(-4, -12, 8, 12);
		}

		public virtual Rectangle GetBounds()
		{
			Rectangle bounds = this.GetLocalBounds();
			bounds.X += (int)base.position.X;
			bounds.Y += (int)base.position.Y;
			return bounds;
		}

		protected override void _Update(float time)
		{
			if (!base.IsActive())
			{
				return;
			}
			int old_x_pos = (int)(base.position.X / (float)base._game.tileSize);
			float old_y_velocity = base.velocity.Y;
			if (base._game.gameState != GameStates.Cutscene && base._jumping && !base._game.isJumpPressed && !base._game.gamePaused)
			{
				base.ReleaseJump();
			}
			base._Update(time);
			if (base._grounded && base._game.respawnCounter <= 0)
			{
				if (base._game.minecartLoop.IsPaused && base._game.currentTheme != 7)
				{
					base._game.minecartLoop.Resume();
				}
				if (old_x_pos != (int)(base.position.X / (float)base._game.tileSize) && Game1.random.NextBool())
				{
					base.minecartBumpOffset = -Game1.random.Next(1, 3);
				}
			}
			else if (!base._grounded)
			{
				if (!base._game.minecartLoop.IsPaused)
				{
					base._game.minecartLoop.Pause();
				}
				base.minecartBumpOffset = 0f;
			}
			base.minecartBumpOffset = Utility.MoveTowards(base.minecartBumpOffset, 0f, time * 20f);
			foreach (Pickup overlap in base._game.GetOverlaps<Pickup>(this))
			{
				overlap.Collect(this);
			}
			Obstacle obstacle = base._game.GetOverlap<Obstacle>(this);
			if (base._game.GetOverlap<Obstacle>(this) != null && ((!(base.velocity.Y > 0f) && !(old_y_velocity > 0f) && !(base.position.Y < obstacle.position.Y - 1f)) || !obstacle.OnBounce(this)) && !obstacle.OnBump(this))
			{
				base._game.Die();
			}
		}

		public override void OnJump()
		{
			Game1.playSound("pickUpItem", 200);
		}

		public override void OnFall()
		{
			Game1.playSound("parry");
			base._game.createSparkShower();
		}

		public override void OnLand()
		{
			if (base.currentTrackType == Track.TrackType.SlimeUpSlope)
			{
				Game1.playSound("slimeHit");
			}
			else
			{
				if (base.currentTrackType >= Track.TrackType.MushroomLeft && base.currentTrackType <= Track.TrackType.MushroomRight)
				{
					Game1.playSound("slimeHit");
					bool purple = base.GetTrack().GetType() != typeof(Track);
					for (int i = 0; i < 3; i++)
					{
						base._game.AddEntity(new MineDebris(new Rectangle(362 + (purple ? 5 : 0), 802, 5, 4), base.position, Game1.random.Next(-30, 31), Game1.random.Next(-50, -39), 0f, 1f, 0.75f, 1f, 1, 1f, 0.15f));
					}
					return;
				}
				Game1.playSound("parry");
			}
			base._game.createSparkShower();
		}

		public override void OnTrackChange()
		{
			if (base._hasJustSnapped || !base._grounded)
			{
				return;
			}
			if (base.currentTrackType == Track.TrackType.SlimeUpSlope)
			{
				Game1.playSound("slimeHit");
			}
			else
			{
				if (base.currentTrackType >= Track.TrackType.MushroomLeft && base.currentTrackType <= Track.TrackType.MushroomRight)
				{
					return;
				}
				Game1.playSound("parry");
			}
			base._game.createSparkShower();
		}
	}

	public class CheckpointIndicator : Entity
	{
		public const int CENTER_TO_POST_BASE_OFFSET = 5;

		public float rotation;

		protected bool _activated;

		public float swayRotation = 120f;

		public float swayTimer;

		protected override void _Update(float time)
		{
			if (!this._activated)
			{
				return;
			}
			this.swayTimer += time * ((float)Math.PI * 2f);
			if ((double)this.swayTimer >= Math.PI * 2.0)
			{
				this.swayTimer = 0f;
				this.swayRotation -= 20f;
				if (this.swayRotation <= 30f)
				{
					this.swayRotation = 30f;
				}
			}
			this.rotation = (float)Math.Sin(this.swayTimer) * this.swayRotation;
		}

		public void Activate()
		{
			if (!this._activated)
			{
				Game1.playSound("fireball");
				this._activated = true;
			}
		}

		public override void _Draw(SpriteBatch b)
		{
			float rad_rotation = this.rotation * (float)Math.PI / 180f;
			Vector2 lantern_offset = new Vector2(0f, -12f);
			b.Draw(base._game.texture, base._game.TransformDraw(base.drawnPosition), new Rectangle(16, 112, 16, 16), base._game.trackTint, 0f, new Vector2(8f, 16f), base._game.GetPixelScale(), SpriteEffects.None, 0.31f);
			if (this._activated)
			{
				b.Draw(base._game.texture, base._game.TransformDraw(base.drawnPosition + lantern_offset), new Rectangle(48, 112, 16, 16), Color.White, rad_rotation, new Vector2(8f, 16f) + lantern_offset, base._game.GetPixelScale(), SpriteEffects.None, 0.3f);
			}
			else
			{
				b.Draw(base._game.texture, base._game.TransformDraw(base.drawnPosition + lantern_offset), new Rectangle(32, 112, 16, 16), Color.White, rad_rotation, new Vector2(8f, 16f) + lantern_offset, base._game.GetPixelScale(), SpriteEffects.None, 0.3f);
			}
		}
	}

	public class GoalIndicator : Entity
	{
		public float rotation;

		protected bool _activated;

		public void Activate()
		{
			if (!this._activated)
			{
				this._activated = true;
			}
		}

		protected override void _Update(float time)
		{
			if (this._activated)
			{
				this.rotation += time * 360f / 0.25f;
			}
		}

		public override void _Draw(SpriteBatch b)
		{
			float rad_rotation = this.rotation * (float)Math.PI / 180f;
			b.Draw(base._game.texture, base._game.TransformDraw(base.drawnPosition), new Rectangle(16, 128, 16, 16), base._game.trackTint, 0f, new Vector2(8f, 16f), base._game.GetPixelScale(), SpriteEffects.None, 0.31f);
			Vector2 sign_offset = new Vector2(0f, -8f);
			b.Draw(base._game.texture, base._game.TransformDraw(base.drawnPosition + sign_offset), new Rectangle(32, 128, 16, 16), Color.White, rad_rotation, new Vector2(8f, 16f) + sign_offset, base._game.GetPixelScale(), SpriteEffects.None, 0.3f);
		}
	}

	public class MineCartCharacter : BaseCharacter
	{
		public float minecartBumpOffset;

		public float jumpStrength = 300f;

		public float maxFallSpeed = 150f;

		public float jumpGravity = 3400f;

		public float fallGravity = 3000f;

		public float jumpFloatDuration = 0.1f;

		public float gravity;

		protected float _jumpBuffer;

		protected float _jumpFloatAge;

		protected float _speedMultiplier = 1f;

		protected float _jumpMomentumThreshhold = -30f;

		public float jumpGracePeriod;

		protected bool _grounded = true;

		protected bool _jumping;

		public float rotation;

		public Vector2 cartScale = Vector2.One;

		public Track.TrackType currentTrackType = Track.TrackType.None;

		public float characterExtraHeight;

		protected bool _hasJustSnapped;

		public float forcedJumpTime;

		public void QueueJump()
		{
			this._jumpBuffer = 0.25f;
		}

		public virtual void OnDie()
		{
			this.cartScale = Vector2.One;
			this._speedMultiplier = 1f;
		}

		public void SnapToFloor()
		{
			List<Track> position_tracks = base._game.GetTracksForXPosition(base.position.X);
			if (position_tracks != null)
			{
				int i = 0;
				if (i < position_tracks.Count)
				{
					Track track = position_tracks[i];
					base.position.Y = track.GetYAtPoint(base.position.X);
					this._grounded = true;
					this.gravity = 0f;
					base.velocity.Y = 0f;
					this.characterExtraHeight = 0f;
					this.minecartBumpOffset = 0f;
					this._hasJustSnapped = true;
				}
			}
		}

		public Track GetTrack(Vector2 offset = default(Vector2))
		{
			int[] offsets = new int[3] { 0, 4, -4 };
			foreach (int x_offset in offsets)
			{
				Vector2 test_position = base.position + offset + new Vector2(x_offset, 0f);
				List<Track> tracks = base._game.GetTracksForXPosition(test_position.X);
				if (tracks == null)
				{
					continue;
				}
				for (int j = 0; j < tracks.Count; j++)
				{
					if (tracks[j].CanLandHere(test_position))
					{
						return tracks[j];
					}
				}
			}
			return null;
		}

		protected override void _Update(float time)
		{
			if (base._game.respawnCounter > 0)
			{
				this.characterExtraHeight = 0f;
				this.rotation = 0f;
				this._jumpBuffer = 0f;
				this.jumpGracePeriod = 0f;
				this.gravity = 0f;
				base.velocity.Y = 0f;
				this.minecartBumpOffset = 0f;
				this.SnapToFloor();
				return;
			}
			base._Update(time);
			if (this.jumpGracePeriod > 0f)
			{
				this.jumpGracePeriod -= time;
			}
			if ((this._grounded || this.jumpGracePeriod > 0f) && this._jumpBuffer > 0f && base._game.isJumpPressed)
			{
				this._jumpBuffer = 0f;
				this.Jump();
			}
			else if (this._jumpBuffer > 0f)
			{
				this._jumpBuffer -= time;
			}
			bool found_valid_ground = false;
			Track.TrackType old_track_type = this.currentTrackType;
			Track track = this.GetTrack();
			if (track != null && this._grounded)
			{
				track.WhileCartGrounded(this, time);
			}
			bool was_grounded = this._grounded;
			if (base.velocity.Y >= 0f && track != null)
			{
				base.position.Y = track.GetYAtPoint(base.position.X);
				this.currentTrackType = track.trackType;
				if (!this._grounded)
				{
					this.cartScale = new Vector2(1.5f, 0.5f);
					this.rotation = 0f;
					this.OnLand();
				}
				found_valid_ground = true;
				base.velocity.Y = 0f;
				this._grounded = true;
			}
			else if (this._grounded && base.velocity.Y >= 0f)
			{
				track = this.GetTrack(new Vector2(0f, 2f));
				if (track != null)
				{
					base.position.Y = track.GetYAtPoint(base.position.X);
					this.currentTrackType = track.trackType;
					found_valid_ground = true;
					base.velocity.Y = 0f;
					this._grounded = true;
				}
			}
			if (!found_valid_ground)
			{
				if (this._grounded)
				{
					this.gravity = 0f;
					base.velocity.Y = this.GetMaxFallSpeed();
					if (!this.IsJumping())
					{
						this.OnFall();
						this.jumpGracePeriod = MineCart.maxJumpGraceTime;
					}
				}
				this.currentTrackType = Track.TrackType.None;
				this._grounded = false;
			}
			float ground_rotation = 0f;
			switch (this.currentTrackType)
			{
			case Track.TrackType.Straight:
				ground_rotation = 0f;
				break;
			case Track.TrackType.UpSlope:
				ground_rotation = -45f;
				break;
			case Track.TrackType.DownSlope:
				ground_rotation = 30f;
				break;
			}
			if (this.IsJumping())
			{
				this.rotation = Utility.MoveTowards(this.rotation, -45f, 300f * time);
				this.characterExtraHeight = 0f;
			}
			else if (!this._grounded)
			{
				this.rotation = Utility.MoveTowards(this.rotation, 0f, 100f * time);
				this.characterExtraHeight = Utility.MoveTowards(this.characterExtraHeight, 16f, 24f * time);
			}
			else
			{
				this.rotation = Utility.MoveTowards(this.rotation, ground_rotation, 360f * time);
				this.characterExtraHeight = Utility.MoveTowards(this.characterExtraHeight, 0f, 128f * time);
			}
			this.cartScale.X = Utility.MoveTowards(this.cartScale.X, 1f, 4f * time);
			this.cartScale.Y = Utility.MoveTowards(this.cartScale.Y, 1f, 4f * time);
			if (was_grounded && old_track_type != this.currentTrackType)
			{
				if ((this.rotation < 0f && ground_rotation > 0f) || (this.rotation > 0f && ground_rotation < 0f))
				{
					this.rotation = 0f;
				}
				this.OnTrackChange();
			}
			if (this.forcedJumpTime > 0f)
			{
				this.forcedJumpTime -= time;
				if (this._grounded)
				{
					this.forcedJumpTime = 0f;
				}
			}
			if (!this._grounded)
			{
				if (this._jumping)
				{
					this._jumpFloatAge += time;
					if (this._jumpFloatAge < this.jumpFloatDuration)
					{
						this.gravity = 0f;
						base.velocity.Y = Utility.Lerp(0f, 0f - this.jumpStrength, this._jumpFloatAge / this.jumpFloatDuration);
					}
					else if (base.velocity.Y <= this._jumpMomentumThreshhold * 2f)
					{
						this.gravity += time * this.jumpGravity;
					}
					else
					{
						base.velocity.Y = this._jumpMomentumThreshhold;
						this.ReleaseJump();
					}
				}
				else
				{
					this.gravity += time * this.fallGravity;
				}
				base.velocity.Y += time * this.gravity;
			}
			else
			{
				this._jumping = false;
			}
			if (base._game.currentTheme == 5)
			{
				this._speedMultiplier = 1f;
			}
			switch (this.currentTrackType)
			{
			case Track.TrackType.SlimeUpSlope:
				this._speedMultiplier = 0.5f;
				break;
			case Track.TrackType.IceDownSlope:
				this._speedMultiplier = Utility.MoveTowards(this._speedMultiplier, 3f, time * 2f);
				break;
			default:
				if (this._grounded)
				{
					this._speedMultiplier = Utility.MoveTowards(this._speedMultiplier, 1f, time * 6f);
				}
				break;
			}
			if (!(this is PlayerMineCartCharacter))
			{
				this._speedMultiplier = 1f;
			}
			base.position.X += time * base.velocity.X * this._speedMultiplier;
			base.position.Y += time * base.velocity.Y;
			if (base.velocity.Y > 0f)
			{
				this._jumping = false;
			}
			if (base.velocity.Y > this.GetMaxFallSpeed())
			{
				base.velocity.Y = this.GetMaxFallSpeed();
			}
			if (this._hasJustSnapped)
			{
				this._hasJustSnapped = false;
			}
		}

		public float GetMaxFallSpeed()
		{
			if (base._game.currentTheme == 2)
			{
				return 75f;
			}
			return this.maxFallSpeed;
		}

		public virtual void OnLand()
		{
		}

		public virtual void OnTrackChange()
		{
		}

		public virtual void OnFall()
		{
		}

		public virtual void OnJump()
		{
		}

		public void ReleaseJump()
		{
			if (!(this.forcedJumpTime > 0f) && this._jumping && base.velocity.Y < 0f)
			{
				this._jumping = false;
				this.gravity = 0f;
				if (base.velocity.Y < this._jumpMomentumThreshhold)
				{
					base.velocity.Y = this._jumpMomentumThreshhold;
				}
			}
		}

		public bool IsJumping()
		{
			return this._jumping;
		}

		public bool IsGrounded()
		{
			return this._grounded;
		}

		public void Bounce(float forced_bounce_time = 0f)
		{
			this.forcedJumpTime = forced_bounce_time;
			this._jumping = true;
			this.gravity = 0f;
			this.cartScale = new Vector2(0.5f, 1.5f);
			base.velocity.Y = 0f - this.jumpStrength;
			this._grounded = false;
		}

		public void Jump()
		{
			if (this._grounded || this.jumpGracePeriod > 0f)
			{
				this._jumping = true;
				this.gravity = 0f;
				this._jumpFloatAge = 0f;
				this.cartScale = new Vector2(0.5f, 1.5f);
				this.OnJump();
				base.velocity.Y = 0f - this.jumpStrength;
				this._grounded = false;
			}
		}

		public void ForceGrounded()
		{
			this._grounded = true;
			this.gravity = 0f;
			base.velocity.Y = 0f;
		}

		public override void _Draw(SpriteBatch b)
		{
			if (base._game.respawnCounter / 200 % 2 == 0)
			{
				float rad_rotation = this.rotation * (float)Math.PI / 180f;
				Vector2 right = new Vector2((float)Math.Cos(rad_rotation), 0f - (float)Math.Sin(rad_rotation));
				Vector2 up = new Vector2((float)Math.Sin(rad_rotation), 0f - (float)Math.Cos(rad_rotation));
				b.Draw(base._game.texture, base._game.TransformDraw(base.drawnPosition + up * (0f - this.minecartBumpOffset) + up * 4f), new Rectangle(0, 0, 16, 16), Color.White, rad_rotation, new Vector2(8f, 14f), this.cartScale * base._game.GetPixelScale(), SpriteEffects.None, 0.45f);
				b.Draw(base._game.texture, base._game.TransformDraw(base.drawnPosition + up * (0f - this.minecartBumpOffset) + up * 4f), new Rectangle(0, 16, 16, 16), Color.White, rad_rotation, new Vector2(8f, 14f), this.cartScale * base._game.GetPixelScale(), SpriteEffects.None, 0.4f);
				b.Draw(Game1.mouseCursors, base._game.TransformDraw(base.drawnPosition + right * -2f + up * (0f - this.minecartBumpOffset) + up * 12f + new Vector2(0f, 0f - this.characterExtraHeight)), new Rectangle(294 + (int)(base._game.totalTimeMS % 400.0) / 100 * 16, 1432, 16, 16), Color.Lime, 0f, new Vector2(8f, 8f), base._game.GetPixelScale() * 2f / 3f, SpriteEffects.None, 0.425f);
			}
		}
	}

	public GameStates gameState;

	public const int followDistance = 96;

	public float pixelScale = 4f;

	public const int tilesBeyondViewportToSimulate = 4;

	public const int bgLoopWidth = 96;

	public const float gravity = 0.21f;

	public const int brownArea = 0;

	public const int frostArea = 1;

	public const int darkArea = 3;

	public const int waterArea = 2;

	public const int lavaArea = 4;

	public const int heavenlyArea = 5;

	public const int sunsetArea = 6;

	public const int endingCutscene = 7;

	public const int bonusLevel1 = 8;

	public const int mushroomArea = 9;

	public const int LAST_LEVEL = 6;

	public readonly int[] infiniteModeLevels = new int[8] { 0, 1, 2, 3, 5, 9, 4, 6 };

	public float shakeMagnitude;

	protected Vector2 _shakeOffset = Vector2.Zero;

	public const int infiniteMode = 2;

	public const int progressMode = 3;

	public const int respawnTime = 1400;

	/// <summary>How long the player can jump after running off the track, measured in seconds.</summary>
	public static float maxJumpGraceTime = 0.1f;

	public float slimeBossPosition = -100f;

	public float slimeBossSpeed;

	public float secondsOnThisLevel;

	public int fruitEatCount;

	public int currentFruitCheckIndex = -1;

	public float currentFruitCheckMagnitude;

	public const int checkpointScanDistance = 16;

	public int coinCount;

	public bool gamePaused;

	private SparklingText perfectText;

	private float lakeSpeedAccumulator;

	private float backBGPosition;

	private float midBGPosition;

	private float waterFallPosition;

	public Vector2 upperLeft;

	private Stopwatch musicSW;

	private bool titleJunimoStartedBobbing;

	private bool lastLevelWasPerfect;

	private bool completelyPerfect = true;

	private int screenWidth;

	private int screenHeight;

	public int tileSize;

	private int waterfallWidth = 1;

	private int ytileOffset;

	private int score;

	private int levelsBeat;

	private int gameMode;

	private int livesLeft;

	private int distanceToTravel = -1;

	private int respawnCounter;

	private int currentTheme;

	private bool reachedFinish;

	private bool gameOver;

	private float screenDarkness;

	protected string cutsceneText = "";

	public float fadeDelta;

	private ICue minecartLoop;

	private Texture2D texture;

	private Dictionary<int, List<Track>> _tracks;

	private List<LakeDecor> lakeDecor = new List<LakeDecor>();

	private List<Point> obstacles = new List<Point>();

	private List<Spark> sparkShower = new List<Spark>();

	private List<int> levelThemesFinishedThisRun = new List<int>();

	private Color backBGTint;

	private Color midBGTint;

	private Color caveTint;

	private Color lakeTint;

	private Color waterfallTint;

	private Color trackShadowTint;

	private Color trackTint;

	private Rectangle midBGSource = new Rectangle(64, 0, 96, 162);

	private Rectangle backBGSource = new Rectangle(64, 162, 96, 111);

	private Rectangle lakeBGSource = new Rectangle(0, 80, 16, 97);

	private int backBGYOffset;

	private int midBGYOffset;

	protected double _totalTime;

	private MineCartCharacter player;

	private MineCartCharacter trackBuilderCharacter;

	private MineDebris titleScreenJunimo;

	private List<Entity> _entities;

	public LevelTransition[] LEVEL_TRANSITIONS;

	protected BaseTrackGenerator _lastGenerator;

	protected BaseTrackGenerator _forcedNextGenerator;

	public float screenLeftBound;

	public Point generatorPosition;

	private BaseTrackGenerator _trackGenerator;

	protected GoalIndicator _goalIndicator;

	public int bottomTile;

	public int topTile;

	public float deathTimer;

	protected int _lastTilePosition = -1;

	public int slimeResetPosition = -80;

	public float checkpointPosition;

	public int furthestGeneratedCheckpoint;

	public bool isJumpPressed;

	public float stateTimer;

	public int cutsceneTick;

	public float pauseBeforeTitleFadeOutTimer;

	public float mapTimer;

	private List<KeyValuePair<string, int>> _currentHighScores;

	private int currentHighScore;

	public float scoreUpdateTimer;

	protected HashSet<CollectableFruits> _spawnedFruit;

	protected HashSet<CollectableFruits> _collectedFruit;

	public List<int> checkpointPositions;

	protected Dictionary<ObstacleTypes, List<Type>> _validObstacles;

	protected List<GeneratorRoll> _generatorRolls;

	private bool _trackAddedFlip;

	protected bool _buttonState;

	public bool _wasJustChatting;

	public double totalTime => this._totalTime;

	public double totalTimeMS => this._totalTime * 1000.0;

	public MineCart(int whichTheme, int mode)
	{
		this._entities = new List<Entity>();
		this._collectedFruit = new HashSet<CollectableFruits>();
		this._generatorRolls = new List<GeneratorRoll>();
		this._validObstacles = new Dictionary<ObstacleTypes, List<Type>>();
		this.initLevelTransitions();
		if (Game1.player.team.junimoKartScores.GetScores().Count == 0)
		{
			Game1.player.team.junimoKartScores.AddScore(Game1.RequireCharacter("Lewis").displayName, 50000);
			Game1.player.team.junimoKartScores.AddScore(Game1.RequireCharacter("Shane").displayName, 25000);
			Game1.player.team.junimoKartScores.AddScore(Game1.RequireCharacter("Sam").displayName, 10000);
			Game1.player.team.junimoKartScores.AddScore(Game1.RequireCharacter("Abigail").displayName, 5000);
			Game1.player.team.junimoKartScores.AddScore(Game1.RequireCharacter("Vincent").displayName, 250);
		}
		this.changeScreenSize();
		this.texture = Game1.content.Load<Texture2D>("Minigames\\MineCart");
		Game1.playSound("minecartLoop", out this.minecartLoop);
		this.minecartLoop.Pause();
		this.backBGYOffset = this.tileSize * 2;
		this.ytileOffset = this.screenHeight / 2 / this.tileSize;
		this.gameMode = mode;
		this.bottomTile = this.screenHeight / this.tileSize - 1;
		this.topTile = 4;
		this.currentTheme = whichTheme;
		this.ShowTitle();
	}

	public void initLevelTransitions()
	{
		this.LEVEL_TRANSITIONS = new LevelTransition[15]
		{
			new LevelTransition(-1, 0, 2, 5, "rrr"),
			new LevelTransition(0, 8, 5, 5, "rddrrd", () => this.lastLevelWasPerfect),
			new LevelTransition(0, 1, 5, 5, "rddlddrdd"),
			new LevelTransition(1, 3, 6, 11, "drdrrrrrrrrruuuuu", () => this.secondsOnThisLevel <= 60f),
			new LevelTransition(1, 5, 6, 11, "rrurruuu", Game1.random.NextBool),
			new LevelTransition(1, 2, 6, 11, "rrurrrrddr"),
			new LevelTransition(8, 5, 8, 8, "ddrruuu", Game1.random.NextBool),
			new LevelTransition(8, 2, 8, 8, "ddrrrrddr"),
			new LevelTransition(5, 3, 10, 7, "urruulluurrrrrddddddr"),
			new LevelTransition(2, 3, 13, 12, "rurruuu"),
			new LevelTransition(3, 9, 16, 8, "rruuluu", Game1.random.NextBool),
			new LevelTransition(3, 4, 16, 8, "rrddrddr"),
			new LevelTransition(4, 6, 20, 12, "ruuruuuuuu"),
			new LevelTransition(9, 6, 17, 4, "rrdrrru"),
			new LevelTransition(6, 7, 22, 4, "rr")
		};
	}

	public void ShowTitle()
	{
		this.musicSW = new Stopwatch();
		Game1.changeMusicTrack("junimoKart", track_interruptable: false, MusicContext.MiniGame);
		this.titleJunimoStartedBobbing = false;
		this.completelyPerfect = true;
		this.screenDarkness = 1f;
		this.fadeDelta = -1f;
		this.ResetState();
		this.player.enabled = false;
		this.setUpTheme(0);
		this.levelThemesFinishedThisRun.Clear();
		this.gameState = GameStates.Title;
		this.CreateLakeDecor();
		this.RefreshHighScore();
		this.titleScreenJunimo = this.AddEntity(new MineDebris(new Rectangle(259, 492, 14, 20), new Vector2(this.screenWidth / 2 - 128 + 137, this.screenHeight / 2 - 35 + 46), 100f, 0f, 0f, 0f, 99999f, 1f, 1, 1f, 0.24f));
		if (this.gameMode == 3)
		{
			this.setUpTheme(-1);
		}
		else
		{
			this.setUpTheme(0);
		}
	}

	public void RefreshHighScore()
	{
		this._currentHighScores = Game1.player.team.junimoKartScores.GetScores();
		this.currentHighScore = 0;
		if (this._currentHighScores.Count > 0)
		{
			this.currentHighScore = this._currentHighScores[0].Value;
		}
	}

	public Obstacle AddObstacle(Track track, ObstacleTypes obstacle_type)
	{
		if (track == null || !this._validObstacles.TryGetValue(obstacle_type, out var obstacleTypes))
		{
			return null;
		}
		Type type = Game1.random.ChooseFrom(obstacleTypes);
		Obstacle obstacle = this.AddEntity(Activator.CreateInstance(type) as Obstacle);
		if (!obstacle.CanSpawnHere(track))
		{
			obstacle.Destroy();
			return null;
		}
		obstacle.position.X = track.position.X + (float)(this.tileSize / 2);
		obstacle.position.Y = track.GetYAtPoint(obstacle.position.X);
		track.obstacle = obstacle;
		obstacle.InitializeObstacle(track);
		return obstacle;
	}

	public virtual T AddEntity<T>(T new_entity) where T : Entity
	{
		this._entities.Add(new_entity);
		new_entity.Initialize(this);
		return new_entity;
	}

	public Track GetTrackForXPosition(float x)
	{
		int tile_position = (int)(x / (float)this.tileSize);
		if (!this._tracks.TryGetValue(tile_position, out var tracks))
		{
			return null;
		}
		return tracks[0];
	}

	public void AddCheckpoint(int tile_x)
	{
		if (this.gameMode != 2)
		{
			tile_x = this.GetValidCheckpointPosition(tile_x);
			if (tile_x != this.furthestGeneratedCheckpoint && tile_x > this.furthestGeneratedCheckpoint + 8 && this.IsTileInBounds((int)(this.GetTrackForXPosition(tile_x * this.tileSize).position.Y / (float)this.tileSize)))
			{
				this.furthestGeneratedCheckpoint = tile_x;
				CheckpointIndicator checkpoint_indicator = this.AddEntity(new CheckpointIndicator());
				checkpoint_indicator.position.X = ((float)tile_x + 0.5f) * (float)this.tileSize;
				checkpoint_indicator.position.Y = this.GetTrackForXPosition(tile_x * this.tileSize).GetYAtPoint(checkpoint_indicator.position.X + 5f);
				this.checkpointPositions.Add(tile_x);
			}
		}
	}

	public List<Track> GetTracksForXPosition(float x)
	{
		int tilePosition = (int)(x / (float)this.tileSize);
		return this._tracks.GetValueOrDefault(tilePosition);
	}

	protected bool _IsGeneratingOnUpperHalf()
	{
		int mid_point = (this.topTile + this.bottomTile) / 2;
		if (this.generatorPosition.Y <= mid_point)
		{
			return true;
		}
		return false;
	}

	protected bool _IsGeneratingOnLowerHalf()
	{
		int mid_point = (this.topTile + this.bottomTile) / 2;
		if (this.generatorPosition.Y >= mid_point)
		{
			return true;
		}
		return false;
	}

	protected void _GenerateMoreTrack()
	{
		while ((float)(this.generatorPosition.X * this.tileSize) <= this.screenLeftBound + (float)this.screenWidth + (float)(16 * this.tileSize))
		{
			if (this._trackGenerator == null)
			{
				if (this.generatorPosition.X >= this.distanceToTravel)
				{
					this._trackGenerator = null;
					break;
				}
				for (int tries = 0; tries < 2; tries++)
				{
					for (int i = 0; i < this._generatorRolls.Count; i++)
					{
						if (this._forcedNextGenerator != null)
						{
							this._trackGenerator = this._forcedNextGenerator;
							this._forcedNextGenerator = null;
							break;
						}
						if (this._generatorRolls[i].generator != this._lastGenerator && Game1.random.NextDouble() < (double)this._generatorRolls[i].chance && (this._generatorRolls[i].additionalGenerationCondition == null || this._generatorRolls[i].additionalGenerationCondition()))
						{
							this._trackGenerator = this._generatorRolls[i].generator;
							this._forcedNextGenerator = this._generatorRolls[i].forcedNextGenerator;
							break;
						}
					}
					if (this._trackGenerator != null)
					{
						break;
					}
					if (this._trackGenerator == null)
					{
						if (this._lastGenerator != null)
						{
							this._lastGenerator = null;
							continue;
						}
						this._trackGenerator = new StraightAwayGenerator(this).SetLength(2, 2).SetStaggerChance(0f).SetCheckpoint(checkpoint: false);
						this._forcedNextGenerator = null;
					}
				}
				this._trackGenerator.Initialize();
				this._lastGenerator = this._trackGenerator;
			}
			this._trackGenerator?.GenerateTrack();
			if (this.generatorPosition.X >= this.distanceToTravel)
			{
				break;
			}
			this._trackGenerator = null;
		}
		if (this.generatorPosition.X >= this.distanceToTravel)
		{
			Track track = this.AddTrack(this.generatorPosition.X, this.generatorPosition.Y);
			if (this._goalIndicator == null)
			{
				this._goalIndicator = this.AddEntity(new GoalIndicator());
				this._goalIndicator.position.X = ((float)this.generatorPosition.X + 0.5f) * (float)this.tileSize;
				this._goalIndicator.position.Y = track.GetYAtPoint(this._goalIndicator.position.X);
			}
			else
			{
				this.CreatePickup(new Vector2((float)this.generatorPosition.X + 0.5f, this.generatorPosition.Y - 1) * this.tileSize, fruit_only: true);
			}
			this.generatorPosition.X++;
		}
	}

	public Track AddTrack(int x, int y, Track.TrackType type = Track.TrackType.Straight)
	{
		if (type == Track.TrackType.UpSlope || type == Track.TrackType.SlimeUpSlope)
		{
			y++;
		}
		this._trackAddedFlip = !this._trackAddedFlip;
		Track track_object = new Track(type, this._trackAddedFlip);
		track_object.position.X = x * this.tileSize;
		track_object.position.Y = y * this.tileSize;
		return this.AddTrack(track_object);
	}

	public Track AddTrack(Track track_object)
	{
		Track track = this.AddEntity(track_object);
		int x = (int)(track.position.X / (float)this.tileSize);
		if (!this._tracks.TryGetValue(x, out var tracks))
		{
			tracks = (this._tracks[x] = new List<Track>());
		}
		tracks.Add(track_object);
		tracks.OrderBy((Track o) => o.position.Y);
		return track;
	}

	public bool overrideFreeMouseMovement()
	{
		return Game1.options.SnappyMenus;
	}

	public void UpdateMapTick(float time)
	{
		this.mapTimer += time;
		MapJunimo map_junimo = null;
		foreach (Entity entity in this._entities)
		{
			if (entity is MapJunimo junimo)
			{
				map_junimo = junimo;
				break;
			}
		}
		if (this.mapTimer >= 2f && map_junimo.moveState == MapJunimo.MoveState.Idle)
		{
			map_junimo.StartMoving();
		}
		switch (map_junimo.moveState)
		{
		case MapJunimo.MoveState.Moving:
			this.mapTimer = 0f;
			break;
		case MapJunimo.MoveState.Finished:
			if (this.mapTimer >= 1.5f)
			{
				this.fadeDelta = 1f;
			}
			break;
		}
		if (this.screenDarkness >= 1f && this.fadeDelta > 0f)
		{
			this.ShowCutscene();
		}
	}

	public void UpdateCutsceneTick()
	{
		int fade_out_time = 400;
		if (this.gamePaused)
		{
			return;
		}
		switch (this.cutsceneTick)
		{
		case 0:
			if (!this.minecartLoop.IsPaused)
			{
				this.minecartLoop.Pause();
			}
			this.cutsceneText = Game1.content.LoadString("Strings\\UI:Junimo_Kart_Level_" + this.currentTheme);
			if (this.currentTheme == 7)
			{
				this.cutsceneText = "";
			}
			this.player.enabled = false;
			this.screenDarkness = 1f;
			this.fadeDelta = -1f;
			break;
		case 100:
			this.player.enabled = true;
			break;
		}
		switch (this.currentTheme)
		{
		case 0:
			this.UpdateCutSceneForBrownArea();
			break;
		case 1:
			this.UpdateCutsceneForFrostArea();
			break;
		case 4:
			this.UpdateCutsceneForLavaArea();
			break;
		case 2:
			this.UpdateCutsceneForWaterArea();
			break;
		case 3:
			this.UpdateCutsceneForDarkArea();
			break;
		case 9:
			this.UpdateCutsceneForMushroomArea();
			break;
		case 6:
			this.UpdateCutsceneForSunsetArea();
			break;
		case 5:
			this.UpdateCutsceneForHeavenlyArea();
			break;
		case 7:
			this.UpdateCutsceneForEnding(ref fade_out_time);
			break;
		}
		if (this.cutsceneTick == fade_out_time)
		{
			this.screenDarkness = 0f;
			this.fadeDelta = 2f;
		}
		if (this.cutsceneTick == fade_out_time + 100)
		{
			this.EndCutscene();
			return;
		}
		if (this.player.velocity.X > 0f && this.player.position.X > (float)(this.screenWidth + this.tileSize))
		{
			if (!this.minecartLoop.IsPaused)
			{
				this.minecartLoop.Pause();
			}
			this.player.enabled = false;
		}
		if (this.player.velocity.X < 0f && this.player.position.X < (float)(-this.tileSize))
		{
			if (!this.minecartLoop.IsPaused)
			{
				this.minecartLoop.Pause();
			}
			this.player.enabled = false;
		}
	}

	public void UpdateCutSceneForBrownArea()
	{
		switch (this.cutsceneTick)
		{
		case 0:
		{
			Roadblock roadblock = this.AddEntity(new Roadblock());
			roadblock.position.X = 6 * this.tileSize;
			roadblock.position.Y = 10 * this.tileSize;
			Roadblock roadblock2 = this.AddEntity(new Roadblock());
			roadblock2.position.X = 19 * this.tileSize;
			roadblock2.position.Y = 10 * this.tileSize;
			break;
		}
		case 140:
			this.player.Jump();
			break;
		case 150:
			this.player.ReleaseJump();
			break;
		case 130:
		case 160:
		case 190:
			this.AddEntity(new FallingBoulder()).position = new Vector2(this.player.position.X + 100f, -16f);
			break;
		case 270:
			this.player.Jump();
			break;
		case 275:
			this.player.ReleaseJump();
			break;
		}
	}

	public void UpdateCutsceneForFrostArea()
	{
		switch (this.cutsceneTick)
		{
		case 0:
			this.AddTrack(2, 9, Track.TrackType.UpSlope);
			this.AddTrack(3, 8, Track.TrackType.UpSlope);
			this.AddTrack(4, 8);
			this.AddTrack(5, 8);
			this.AddTrack(6, 7, Track.TrackType.UpSlope);
			this.AddTrack(7, 8, Track.TrackType.IceDownSlope);
			this.AddTrack(8, 9, Track.TrackType.IceDownSlope);
			this.AddTrack(9, 10, Track.TrackType.IceDownSlope);
			this.AddTrack(13, 9, Track.TrackType.UpSlope);
			this.AddTrack(17, 8, Track.TrackType.UpSlope);
			this.AddTrack(19, 10, Track.TrackType.UpSlope);
			this.AddTrack(21, 6, Track.TrackType.UpSlope);
			this.AddTrack(24, 8);
			this.AddTrack(25, 8);
			this.AddTrack(26, 8);
			this.AddTrack(27, 8);
			this.AddTrack(28, 8);
			break;
		case 100:
			this.player.Jump();
			break;
		case 130:
			this.player.ReleaseJump();
			break;
		case 200:
			this.player.Jump();
			break;
		case 215:
			this.player.ReleaseJump();
			break;
		case 260:
			this.player.Jump();
			break;
		case 270:
			this.player.ReleaseJump();
			break;
		case 304:
			this.player.Jump();
			break;
		}
	}

	public void UpdateCutsceneForLavaArea()
	{
		switch (this.cutsceneTick)
		{
		case 0:
			this.AddTrack(1, 12, Track.TrackType.UpSlope);
			this.AddTrack(2, 11, Track.TrackType.UpSlope);
			this.AddTrack(3, 10, Track.TrackType.UpSlope);
			this.AddTrack(4, 9, Track.TrackType.UpSlope);
			this.AddTrack(5, 8, Track.TrackType.UpSlope);
			this.AddTrack(6, 9, Track.TrackType.DownSlope);
			this.AddTrack(7, 8, Track.TrackType.UpSlope);
			this.AddTrack(8, 9, Track.TrackType.DownSlope);
			this.AddTrack(9, 8, Track.TrackType.UpSlope);
			this.AddTrack(10, 9, Track.TrackType.DownSlope);
			this.AddTrack(11, 8, Track.TrackType.UpSlope);
			this.AddTrack(12, 9, Track.TrackType.DownSlope);
			this.AddTrack(13, 8, Track.TrackType.UpSlope);
			this.AddTrack(14, 9, Track.TrackType.DownSlope);
			this.AddTrack(15, 8, Track.TrackType.UpSlope);
			this.AddTrack(16, 9, Track.TrackType.DownSlope);
			this.AddTrack(17, 8, Track.TrackType.UpSlope);
			this.AddTrack(18, 9, Track.TrackType.DownSlope);
			this.AddTrack(19, 8, Track.TrackType.UpSlope);
			this.AddTrack(20, 9, Track.TrackType.DownSlope);
			this.AddTrack(21, 8, Track.TrackType.UpSlope);
			this.AddTrack(22, 7, Track.TrackType.UpSlope);
			this.AddTrack(23, 6, Track.TrackType.UpSlope);
			this.AddTrack(24, 5, Track.TrackType.UpSlope);
			this.AddTrack(25, 4, Track.TrackType.UpSlope);
			this.AddTrack(26, 3, Track.TrackType.UpSlope);
			this.AddTrack(27, 2, Track.TrackType.UpSlope);
			break;
		case 100:
			this.player.Jump();
			break;
		case 115:
			this.player.ReleaseJump();
			break;
		case 265:
			this.player.Jump();
			break;
		}
	}

	public void UpdateCutsceneForWaterArea()
	{
		switch (this.cutsceneTick)
		{
		case 0:
			this.AddEntity(new Whale());
			this.AddEntity(new PlayerBubbleSpawner());
			break;
		case 250:
			this.player.velocity.X = 0f;
			{
				foreach (Entity entity in this._entities)
				{
					if (entity is Whale whale)
					{
						Game1.playSound("croak");
						whale.SetState(Whale.CurrentState.OpenMouth);
						break;
					}
				}
				break;
			}
		case 260:
			this.player.Jump();
			break;
		case 265:
			this.player.ReleaseJump();
			break;
		case 310:
			this.player.velocity.X = -100f;
			break;
		}
	}

	public void UpdateCutsceneForDarkArea()
	{
		switch (this.cutsceneTick)
		{
		case 0:
		{
			this.AddTrack(-1, 3);
			this.AddTrack(0, 3);
			this.AddTrack(1, 4, Track.TrackType.DownSlope);
			this.AddTrack(2, 4);
			this.AddTrack(3, 4);
			this.AddTrack(4, 4);
			this.AddTrack(5, 4);
			this.AddTrack(6, -2);
			this.AddTrack(7, -2);
			this.AddTrack(8, -2);
			this.AddTrack(9, -2);
			this.AddTrack(19, 9);
			this.AddTrack(20, 9);
			this.AddTrack(21, 8, Track.TrackType.UpSlope);
			this.AddTrack(22, 8);
			this.AddTrack(23, 8);
			this.AddTrack(24, 9, Track.TrackType.DownSlope);
			this.AddTrack(25, 9);
			this.AddTrack(26, 8);
			this.AddTrack(27, 8);
			this.AddTrack(28, 8);
			this.player.position.Y = 3 * this.tileSize;
			WillOWisp willOWisp = this.AddEntity(new WillOWisp());
			willOWisp.position.X = 10 * this.tileSize;
			willOWisp.position.Y = 5 * this.tileSize;
			willOWisp.visible = false;
			return;
		}
		case 150:
			this.player.Jump();
			return;
		case 130:
			this.player.ReleaseJump();
			return;
		case 200:
			this.player.Jump();
			return;
		case 215:
			this.player.ReleaseJump();
			return;
		case 300:
			Game1.playSound("ghost");
			return;
		}
		if (this.cutsceneTick >= 300 && this.cutsceneTick % 3 == 0 && this.cutsceneTick < 350)
		{
			foreach (Entity entity in this._entities)
			{
				if (entity is WillOWisp)
				{
					entity.visible = !entity.visible;
				}
			}
		}
		if (this.cutsceneTick != 350)
		{
			return;
		}
		foreach (Entity entity2 in this._entities)
		{
			if (entity2 is WillOWisp)
			{
				entity2.visible = true;
			}
		}
	}

	public void UpdateCutsceneForMushroomArea()
	{
		switch (this.cutsceneTick)
		{
		case 0:
		{
			this.AddTrack(0, 6);
			this.AddTrack(1, 6);
			this.AddTrack(2, 6);
			this.AddTrack(3, 6);
			Track spring_track = this.AddTrack(4, 6);
			MushroomSpring mushroomSpring = this.AddEntity(new MushroomSpring());
			mushroomSpring.InitializeObstacle(spring_track);
			mushroomSpring.position = new Vector2(4.5f, 6f) * this.tileSize;
			this.AddTrack(8, 6, Track.TrackType.MushroomLeft);
			this.AddTrack(9, 6, Track.TrackType.MushroomMiddle);
			this.AddTrack(10, 6, Track.TrackType.MushroomRight);
			this.AddTrack(12, 10);
			List<BalanceTrack> track_parts = new List<BalanceTrack>();
			NoxiousMushroom noxiousMushroom = this.AddEntity(new NoxiousMushroom());
			noxiousMushroom.position = new Vector2(12.5f, 10f) * this.tileSize;
			noxiousMushroom.nextFire = 3f;
			BalanceTrack track_piece = new BalanceTrack(Track.TrackType.MushroomLeft, showSecondTile: false);
			track_piece.position.X = 15 * this.tileSize;
			track_piece.position.Y = 9 * this.tileSize;
			track_parts.Add(track_piece);
			this.AddTrack(track_piece);
			track_piece = new BalanceTrack(Track.TrackType.MushroomMiddle, showSecondTile: false);
			track_piece.position.X = 16 * this.tileSize;
			track_piece.position.Y = 9 * this.tileSize;
			track_parts.Add(track_piece);
			this.AddTrack(track_piece);
			track_piece = new BalanceTrack(Track.TrackType.MushroomRight, showSecondTile: false);
			track_piece.position.X = 17 * this.tileSize;
			track_piece.position.Y = 9 * this.tileSize;
			track_parts.Add(track_piece);
			this.AddTrack(track_piece);
			List<BalanceTrack> other_track_parts = new List<BalanceTrack>();
			track_piece = new BalanceTrack(Track.TrackType.MushroomLeft, showSecondTile: false);
			track_piece.position.X = 22 * this.tileSize;
			track_piece.position.Y = 9 * this.tileSize;
			other_track_parts.Add(track_piece);
			this.AddTrack(track_piece);
			track_piece = new BalanceTrack(Track.TrackType.MushroomMiddle, showSecondTile: false);
			track_piece.position.X = 23 * this.tileSize;
			track_piece.position.Y = 9 * this.tileSize;
			other_track_parts.Add(track_piece);
			this.AddTrack(track_piece);
			track_piece = new BalanceTrack(Track.TrackType.MushroomRight, showSecondTile: false);
			track_piece.position.X = 24 * this.tileSize;
			track_piece.position.Y = 9 * this.tileSize;
			other_track_parts.Add(track_piece);
			this.AddTrack(track_piece);
			foreach (BalanceTrack item in track_parts)
			{
				item.connectedTracks = new List<BalanceTrack>(track_parts);
				item.counterBalancedTracks = new List<BalanceTrack>(other_track_parts);
			}
			foreach (BalanceTrack item2 in other_track_parts)
			{
				item2.connectedTracks = new List<BalanceTrack>(other_track_parts);
				item2.counterBalancedTracks = new List<BalanceTrack>(track_parts);
			}
			this.player.position.Y = 6 * this.tileSize;
			break;
		}
		case 115:
			this.player.Jump();
			break;
		case 120:
			this.player.ReleaseJump();
			break;
		case 230:
			this.player.Jump();
			break;
		case 250:
			this.player.ReleaseJump();
			break;
		case 298:
			this.player.Jump();
			break;
		}
	}

	public void UpdateCutsceneForSunsetArea()
	{
		switch (this.cutsceneTick)
		{
		case 0:
			this.AddTrack(0, 6);
			this.AddTrack(1, 3);
			this.AddTrack(2, 8);
			this.AddTrack(4, 4);
			this.AddTrack(5, 4);
			this.AddTrack(6, 2);
			this.AddTrack(8, 8);
			this.AddTrack(9, 1);
			this.AddTrack(10, 2);
			this.AddTrack(12, 8);
			this.AddTrack(13, 6);
			this.AddTrack(14, 6);
			this.AddTrack(15, 8);
			this.AddTrack(17, 4);
			this.AddTrack(18, 2);
			this.AddTrack(19, 2);
			this.AddTrack(20, 2);
			this.AddTrack(21, 2);
			this.AddTrack(22, 2);
			this.AddTrack(23, 2);
			this.AddTrack(24, 2);
			this.AddTrack(25, 2);
			this.AddTrack(26, 2);
			this.AddTrack(27, 2);
			this.AddTrack(28, 2);
			this.player.position.Y = 6 * this.tileSize;
			break;
		case 129:
			this.player.Jump();
			break;
		case 170:
			this.player.ReleaseJump();
			break;
		case 214:
			this.player.Jump();
			break;
		}
	}

	public void UpdateCutsceneForHeavenlyArea()
	{
		if (this.cutsceneTick == 100)
		{
			this.AddEntity(new HugeSlime());
			this.slimeBossPosition = -100f;
		}
	}

	public void UpdateCutsceneForEnding(ref int fadeOutTimer)
	{
		fadeOutTimer = 800;
		switch (this.cutsceneTick)
		{
		case 0:
			if (this.completelyPerfect)
			{
				this.AddEntity(new MineDebris(new Rectangle(256, 182, 48, 45), new Vector2((float)(20 * this.tileSize) + 12f, (float)(10 * this.tileSize) - 21.5f), 0f, 0f, 0f, 0f, 1000f, 1f, 1, 0f, 0.23f, holdLastFrame: true));
			}
			else
			{
				this.AddEntity(new MineDebris(new Rectangle(256, 112, 25, 32), new Vector2((float)(20 * this.tileSize) + 12f, (float)(10 * this.tileSize) - 16f), 0f, 0f, 0f, 0f, 1000f, 1f, 1, 0f, 0.23f, holdLastFrame: true));
			}
			return;
		case 200:
			this.player.velocity.X = 40f;
			return;
		case 250:
			this.player.velocity.X = 20f;
			return;
		case 300:
			this.player.velocity.X = 0f;
			return;
		}
		if (this.cutsceneTick >= 350 && this.cutsceneTick % 10 == 0 && this.cutsceneTick < 600)
		{
			Game1.playSound("junimoMeep1");
			this.AddEntity(new EndingJunimo(this.completelyPerfect)).position = new Vector2(20 * this.tileSize, 10 * this.tileSize);
		}
	}

	public void UpdateFruitsSummary(float time)
	{
		if (this.currentTheme == 7)
		{
			this.currentFruitCheckIndex = -1;
			this.ShowCutscene();
		}
		if (this.gamePaused)
		{
			return;
		}
		if (this.stateTimer >= 0f)
		{
			this.stateTimer -= time;
			if (this.stateTimer < 0f)
			{
				this.stateTimer = 0f;
			}
		}
		if (this.stateTimer != 0f)
		{
			return;
		}
		if (this.livesLeft < 3 && this.gameMode == 3)
		{
			this.livesLeft++;
			this.stateTimer = 0.25f;
			Game1.playSound("coin");
			return;
		}
		if (this.lastLevelWasPerfect && this.perfectText == null && this.gameMode == 3)
		{
			this.perfectText = new SparklingText(Game1.dialogueFont, Game1.content.LoadString("Strings\\UI:BobberBar_Perfect"), Color.Lime, Color.White, rainbow: true, 0.1, 2500, -1, 500, 0f);
			Game1.playSound("yoba");
		}
		if (this.currentFruitCheckIndex == -1)
		{
			this.fruitEatCount = 0;
			this.currentFruitCheckIndex = 0;
			this.stateTimer = 0.5f;
			return;
		}
		if (this.currentFruitCheckIndex >= 3)
		{
			this.perfectText = null;
			this.currentFruitCheckIndex = -1;
			this.ShowMap();
			return;
		}
		if (this._collectedFruit.Contains((CollectableFruits)this.currentFruitCheckIndex))
		{
			this._collectedFruit.Remove((CollectableFruits)this.currentFruitCheckIndex);
			Game1.playSound("newArtifact", this.currentFruitCheckIndex * 100);
			this.fruitEatCount++;
			if (this.fruitEatCount >= 3)
			{
				Game1.playSound("yoba");
				if (this.gameMode == 3)
				{
					this.livesLeft++;
				}
				else
				{
					this.score += 5000;
					this.UpdateScoreState();
				}
			}
		}
		else
		{
			Game1.playSound("sell", this.currentFruitCheckIndex * 100);
		}
		this.stateTimer = 0.5f;
		this.currentFruitCheckMagnitude = 3f;
		this.currentFruitCheckIndex++;
	}

	public void UpdateInput()
	{
		if (Game1.IsChatting || Game1.textEntry != null)
		{
			this._wasJustChatting = true;
		}
		else
		{
			if (this.gamePaused)
			{
				return;
			}
			bool buttonPressed = Game1.input.GetMouseState().LeftButton == ButtonState.Pressed || Game1.isOneOfTheseKeysDown(Game1.input.GetKeyboardState(), Game1.options.useToolButton) || Game1.isOneOfTheseKeysDown(Game1.input.GetKeyboardState(), Game1.options.actionButton) || Game1.input.GetKeyboardState().IsKeyDown(Keys.Space) || Game1.input.GetKeyboardState().IsKeyDown(Keys.LeftShift) || Game1.input.GetGamePadState().IsButtonDown(Buttons.A) || Game1.input.GetGamePadState().IsButtonDown(Buttons.B);
			if (buttonPressed != this._buttonState)
			{
				this._buttonState = buttonPressed;
				if (this._buttonState)
				{
					switch (this.gameState)
					{
					case GameStates.Title:
						if (this.pauseBeforeTitleFadeOutTimer == 0f && this.screenDarkness == 0f && this.fadeDelta <= 0f)
						{
							this.pauseBeforeTitleFadeOutTimer = 0.5f;
							Game1.playSound("junimoMeep1");
							if (this.titleScreenJunimo != null)
							{
								this.titleScreenJunimo.Destroy();
								this.AddEntity(new MineDebris(new Rectangle(259, 492, 14, 20), new Vector2(this.screenLeftBound + (float)(this.screenWidth / 2) - 128f + 137f, this.screenHeight / 2 - 35 + 46), 110f, -200f, 0f, 3f, 99999f, 1f, 1, 1f, 0.24f));
							}
							this.musicSW?.Stop();
							this.musicSW = null;
						}
						return;
					case GameStates.Cutscene:
						this.EndCutscene();
						return;
					case GameStates.Map:
						this.fadeDelta = 1f;
						return;
					}
					this.player?.QueueJump();
					this.isJumpPressed = true;
				}
				else if (!this.gamePaused)
				{
					this.player?.ReleaseJump();
					this.isJumpPressed = false;
				}
			}
			this._wasJustChatting = false;
		}
	}

	public virtual bool CanPause()
	{
		GameStates gameStates = this.gameState;
		if ((uint)(gameStates - 1) <= 3u)
		{
			return true;
		}
		return false;
	}

	public bool tick(GameTime time)
	{
		this.UpdateInput();
		float delta_time = (float)time.ElapsedGameTime.TotalSeconds;
		if (this.gamePaused)
		{
			delta_time = 0f;
		}
		if (!this.CanPause())
		{
			this.gamePaused = false;
		}
		this.shakeMagnitude = Utility.MoveTowards(this.shakeMagnitude, 0f, delta_time * 3f);
		this.currentFruitCheckMagnitude = Utility.MoveTowards(this.currentFruitCheckMagnitude, 0f, delta_time * 6f);
		this._totalTime += delta_time;
		this.screenDarkness += this.fadeDelta * delta_time;
		if (this.screenDarkness < 0f)
		{
			this.screenDarkness = 0f;
		}
		if (this.screenDarkness > 1f)
		{
			this.screenDarkness = 1f;
		}
		switch (this.gameState)
		{
		case GameStates.Title:
			if (this.pauseBeforeTitleFadeOutTimer > 0f)
			{
				this.pauseBeforeTitleFadeOutTimer -= 0.0166666f;
				if (this.pauseBeforeTitleFadeOutTimer <= 0f)
				{
					this.fadeDelta = 1f;
				}
			}
			if (this.fadeDelta >= 0f && this.screenDarkness >= 1f)
			{
				this.restartLevel(new_game: true);
				return false;
			}
			if (Game1.random.NextDouble() < 0.1)
			{
				this.AddEntity(new MineDebris(new Rectangle(0, 250, 5, 5), Utility.getRandomPositionInThisRectangle(new Rectangle((int)this.screenLeftBound + this.screenWidth / 2 - 128, this.screenHeight / 2 - 35, 256, 71), Game1.random), 100f, 0f, 0f, 0f, 0.6f, 1f, 6, 0.1f, 0.23f));
			}
			if (this.musicSW != null && Game1.currentSong?.Name == "junimoKart" && Game1.currentSong.IsPlaying && !this.musicSW.IsRunning)
			{
				this.musicSW.Start();
			}
			if (this.titleScreenJunimo != null && !this.titleJunimoStartedBobbing)
			{
				Stopwatch stopwatch = this.musicSW;
				if (stopwatch != null && stopwatch.ElapsedMilliseconds >= 48000)
				{
					this.titleScreenJunimo.reset(new Rectangle(417, 347, 14, 20), this.titleScreenJunimo.position, 100f, 0f, 0f, 0f, 9999f, 1f, 2, 0.25f, this.titleScreenJunimo.depth);
					this.titleJunimoStartedBobbing = true;
					break;
				}
			}
			if (this.titleScreenJunimo != null && this.titleJunimoStartedBobbing)
			{
				Stopwatch stopwatch2 = this.musicSW;
				if (stopwatch2 != null && stopwatch2.ElapsedMilliseconds >= 80000)
				{
					this.titleScreenJunimo.reset(new Rectangle(259, 492, 14, 20), this.titleScreenJunimo.position, 100f, 0f, 0f, 0f, 99999f, 1f, 1, 1f, 0.24f);
					this.musicSW.Stop();
					this.musicSW = null;
				}
			}
			break;
		case GameStates.Map:
			this.UpdateMapTick(delta_time);
			break;
		case GameStates.Cutscene:
			if (!this.gamePaused)
			{
				delta_time = 0.0166666f;
			}
			this.UpdateCutsceneTick();
			if (!this.gamePaused)
			{
				this.cutsceneTick++;
			}
			break;
		case GameStates.FruitsSummary:
			this.UpdateFruitsSummary(delta_time);
			break;
		}
		int delta_ms = (int)(delta_time * 1000f);
		for (int i = 0; i < this._entities.Count; i++)
		{
			if (this._entities[i] != null && this._entities[i].IsActive())
			{
				this._entities[i].Update(delta_time);
			}
		}
		if (this.deathTimer <= 0f && this.respawnCounter > 0)
		{
			for (int j = 0; j < this._entities.Count; j++)
			{
				this._entities[j].OnPlayerReset();
			}
		}
		this._entities.RemoveAll((Entity entity2) => entity2?.ShouldReap() ?? false);
		float old_screen_left_bound = this.screenLeftBound;
		if (this.gameState == GameStates.Ingame)
		{
			this.secondsOnThisLevel += delta_time;
			if (this.screenDarkness >= 1f && this.gameOver)
			{
				if (this.gameMode == 3)
				{
					this.ShowTitle();
				}
				else
				{
					this.levelsBeat = 0;
					this.coinCount = 0;
					this.setUpTheme(0);
					this.restartLevel(new_game: true);
				}
				return false;
			}
			if (this.checkpointPositions.Count > 0)
			{
				int i2 = 0;
				while (i2 < this.checkpointPositions.Count && this.player.position.X >= (float)(this.checkpointPositions[i2] * this.tileSize))
				{
					foreach (Entity entity2 in this._entities)
					{
						if (entity2 is CheckpointIndicator indicator && (int)(indicator.position.X / (float)this.tileSize) == this.checkpointPositions[i2])
						{
							indicator.Activate();
							break;
						}
					}
					this.checkpointPosition = ((float)this.checkpointPositions[i2] + 0.5f) * (float)this.tileSize;
					this.ReapEntities();
					this.checkpointPositions.RemoveAt(i2);
					i2--;
					i2++;
				}
			}
			float minimum_left_bound = 0f;
			if (this.gameState == GameStates.Cutscene)
			{
				this.screenLeftBound = 0f;
			}
			else
			{
				if (this.deathTimer <= 0f && this.respawnCounter > 0)
				{
					if (this.screenLeftBound - Math.Max(this.player.position.X - 96f, minimum_left_bound) > 400f)
					{
						this.screenLeftBound = Utility.MoveTowards(this.screenLeftBound, Math.Max(this.player.position.X - 96f, 0f), 1200f * delta_time);
					}
					else if (this.screenLeftBound - Math.Max(this.player.position.X - 96f, minimum_left_bound) > 200f)
					{
						this.screenLeftBound = Utility.MoveTowards(this.screenLeftBound, Math.Max(this.player.position.X - 96f, minimum_left_bound), 600f * delta_time);
					}
					else
					{
						this.screenLeftBound = Utility.MoveTowards(this.screenLeftBound, Math.Max(this.player.position.X - 96f, minimum_left_bound), 300f * delta_time);
					}
					if (this.screenLeftBound < minimum_left_bound)
					{
						this.screenLeftBound = minimum_left_bound;
					}
				}
				else if (this.deathTimer <= 0f && (float)this.respawnCounter <= 0f && !this.reachedFinish)
				{
					this.screenLeftBound = this.player.position.X - 96f;
				}
				if (this.screenLeftBound < minimum_left_bound)
				{
					this.screenLeftBound = minimum_left_bound;
				}
			}
			if ((float)(this.generatorPosition.X * this.tileSize) <= this.screenLeftBound + (float)this.screenWidth + (float)(16 * this.tileSize))
			{
				this._GenerateMoreTrack();
			}
			int player_tile_position = (int)this.player.position.X / this.tileSize;
			if (this.respawnCounter <= 0)
			{
				if (player_tile_position > this._lastTilePosition)
				{
					int number_of_motions = player_tile_position - this._lastTilePosition;
					this._lastTilePosition = player_tile_position;
					for (int i3 = 0; i3 < number_of_motions; i3++)
					{
						this.score += 10;
					}
				}
			}
			else if (this.respawnCounter > 0)
			{
				if (this.deathTimer > 0f)
				{
					this.deathTimer -= delta_time;
				}
				else if (this.screenLeftBound <= Math.Max(minimum_left_bound, this.player.position.X - 96f))
				{
					if (!this.player.enabled)
					{
						Utility.CollectGarbage();
					}
					this.player.enabled = true;
					this.respawnCounter -= delta_ms;
				}
			}
			if (this._goalIndicator != null && this.distanceToTravel != -1 && this.player.position.X >= this._goalIndicator.position.X && this.distanceToTravel != -1 && this.player.position.Y <= this._goalIndicator.position.Y * (float)this.tileSize + 4f && !this.reachedFinish && this.fadeDelta < 0f)
			{
				Game1.playSound("reward");
				this.levelThemesFinishedThisRun.Add(this.currentTheme);
				if (this.gameMode == 2)
				{
					this.score += 5000;
					this.UpdateScoreState();
				}
				foreach (Entity entity in this._entities)
				{
					if (entity is GoalIndicator indicator2)
					{
						indicator2.Activate();
					}
					else if (entity is Coin || entity is Fruit)
					{
						this.lastLevelWasPerfect = false;
					}
				}
				this.reachedFinish = true;
				this.fadeDelta = 1f;
			}
			if (this.score > this.currentHighScore)
			{
				this.currentHighScore = this.score;
			}
			if (this.scoreUpdateTimer <= 0f)
			{
				this.UpdateScoreState();
			}
			else
			{
				this.scoreUpdateTimer -= delta_time;
			}
			if (this.reachedFinish && Game1.random.NextDouble() < 0.25 && !this.gamePaused)
			{
				this.createSparkShower();
			}
			if (this.reachedFinish && this.screenDarkness >= 1f)
			{
				this.reachedFinish = false;
				if (this.gameMode != 3)
				{
					this.currentTheme = this.infiniteModeLevels[(this.levelsBeat + 1) % 8];
				}
				this.levelsBeat++;
				this.setUpTheme(this.currentTheme);
				this.restartLevel();
			}
			float death_buffer = 3f;
			if (this.currentTheme == 9)
			{
				death_buffer = 32f;
			}
			if (this.player.position.Y > (float)this.screenHeight + death_buffer)
			{
				this.Die();
			}
		}
		else if (this.gameState == GameStates.FruitsSummary)
		{
			this.screenLeftBound = 0f;
			if (this.perfectText != null && this.perfectText.update(time))
			{
				this.perfectText = null;
			}
		}
		if (this.gameState == GameStates.Title)
		{
			this.screenLeftBound += delta_time * 100f;
		}
		float parallax_scroll_speed = (this.screenLeftBound - old_screen_left_bound) / (float)this.tileSize;
		this.lakeSpeedAccumulator += (float)delta_ms * (parallax_scroll_speed / 4f) % 96f;
		this.backBGPosition += (float)delta_ms * (parallax_scroll_speed / 5f);
		this.backBGPosition = (this.backBGPosition + 9600f) % 96f;
		this.midBGPosition += (float)delta_ms * (parallax_scroll_speed / 4f);
		this.midBGPosition = (this.midBGPosition + 9600f) % 96f;
		this.waterFallPosition += (float)delta_ms * (parallax_scroll_speed * 6f / 5f);
		if (this.waterFallPosition > (float)(this.screenWidth * 3 / 2))
		{
			this.waterFallPosition %= this.screenWidth * 3 / 2;
			this.waterfallWidth = Game1.random.Next(6);
		}
		for (int i4 = this.sparkShower.Count - 1; i4 >= 0; i4--)
		{
			this.sparkShower[i4].dy += 0.105f * (delta_time / 0.0166666f);
			this.sparkShower[i4].x += this.sparkShower[i4].dx * (delta_time / 0.0166666f);
			this.sparkShower[i4].y += this.sparkShower[i4].dy * (delta_time / 0.0166666f);
			this.sparkShower[i4].c.B = (byte)(0.0 + Math.Max(0.0, Math.Sin(this.totalTimeMS / (Math.PI * 20.0 / (double)this.sparkShower[i4].dx)) * 255.0));
			if (this.reachedFinish)
			{
				this.sparkShower[i4].c.R = (byte)(0.0 + Math.Max(0.0, Math.Sin((this.totalTimeMS + 50.0) / (Math.PI * 20.0 / (double)this.sparkShower[i4].dx)) * 255.0));
				this.sparkShower[i4].c.G = (byte)(0.0 + Math.Max(0.0, Math.Sin((this.totalTimeMS + 100.0) / (Math.PI * 20.0 / (double)this.sparkShower[i4].dx)) * 255.0));
				if (this.sparkShower[i4].c.R == 0)
				{
					this.sparkShower[i4].c.R = byte.MaxValue;
				}
				if (this.sparkShower[i4].c.G == 0)
				{
					this.sparkShower[i4].c.G = byte.MaxValue;
				}
			}
			if (this.sparkShower[i4].y > (float)this.screenHeight)
			{
				this.sparkShower.RemoveAt(i4);
			}
		}
		return false;
	}

	public void UpdateScoreState()
	{
		Game1.player.team.junimoKartStatus.UpdateState(this.score.ToString());
		this.scoreUpdateTimer = 1f;
	}

	public int GetValidCheckpointPosition(int x_pos)
	{
		int i;
		for (i = 0; i < 16; i++)
		{
			if (this.GetTrackForXPosition(x_pos * this.tileSize) != null)
			{
				break;
			}
			x_pos--;
		}
		for (; i < 16; i++)
		{
			if (this.GetTrackForXPosition(x_pos * this.tileSize) == null)
			{
				x_pos++;
				break;
			}
			x_pos--;
		}
		if (this.GetTrackForXPosition(x_pos * this.tileSize) == null)
		{
			return this.furthestGeneratedCheckpoint;
		}
		int valid_x_pos = x_pos;
		int tile_y = (int)(this.GetTrackForXPosition(x_pos * this.tileSize).position.Y / (float)this.tileSize);
		x_pos++;
		int consecutive_valid_tracks = 0;
		for (i = 0; i < 16; i++)
		{
			Track current_track = this.GetTrackForXPosition(x_pos * this.tileSize);
			if (current_track == null)
			{
				return this.furthestGeneratedCheckpoint;
			}
			if (Math.Abs((int)(current_track.position.Y / (float)this.tileSize) - tile_y) <= 1)
			{
				consecutive_valid_tracks++;
				if (consecutive_valid_tracks >= 3)
				{
					return valid_x_pos;
				}
			}
			else
			{
				consecutive_valid_tracks = 0;
				valid_x_pos = x_pos;
				tile_y = (int)(this.GetTrackForXPosition(x_pos * this.tileSize).position.Y / (float)this.tileSize);
			}
			x_pos++;
		}
		return this.furthestGeneratedCheckpoint;
	}

	public virtual void CollectFruit(CollectableFruits fruit_type)
	{
		this._collectedFruit.Add(fruit_type);
		if (this.gameMode == 3)
		{
			this.CollectCoin(10);
			return;
		}
		this.score += 1000;
		this.UpdateScoreState();
	}

	public virtual void CollectCoin(int amount)
	{
		if (this.gameMode == 3)
		{
			this.coinCount += amount;
			if (this.coinCount >= 100)
			{
				Game1.playSound("yoba");
				int added_lives = this.coinCount / 100;
				this.coinCount %= 100;
				this.livesLeft += added_lives;
			}
		}
		else
		{
			this.score += 30;
			this.UpdateScoreState();
		}
	}

	public void submitHighScore()
	{
		if (Game1.player.team.junimoKartScores.GetScores()[0].Value < this.score)
		{
			Game1.multiplayer.globalChatInfoMessage("JunimoKartHighScore", Game1.player.Name);
		}
		Game1.player.team.junimoKartScores.AddScore(Game1.player.name.Value, this.score);
		if (Game1.player.team.specialOrders != null)
		{
			foreach (SpecialOrder specialOrder in Game1.player.team.specialOrders)
			{
				specialOrder.onJKScoreAchieved?.Invoke(Game1.player, this.score);
			}
		}
		this.RefreshHighScore();
	}

	public void Die()
	{
		if (this.respawnCounter > 0 || this.deathTimer > 0f || this.reachedFinish || !this.player.enabled)
		{
			return;
		}
		this.player.OnDie();
		this.AddEntity(new MineDebris(new Rectangle(16, 96, 16, 16), this.player.position, Game1.random.Next(-80, 81), Game1.random.Next(-100, -49), 0f, 1f, 1f));
		this.AddEntity(new MineDebris(new Rectangle(32, 96, 16, 16), this.player.position + new Vector2(0f, 0f - this.player.characterExtraHeight), Game1.random.Next(-80, 81), Game1.random.Next(-150, -99), 0.1f, 1f, 1f, 2f / 3f)).SetColor(Color.Lime);
		this.player.position.Y = -1000f;
		Game1.playSound("fishEscape");
		this.player.enabled = false;
		this.lastLevelWasPerfect = false;
		this.completelyPerfect = false;
		if (this.gameState == GameStates.Cutscene)
		{
			return;
		}
		this.livesLeft--;
		if (this.gameMode != 3 || this.livesLeft < 0)
		{
			this.gameOver = true;
			this.fadeDelta = 1f;
			if (this.gameMode == 2)
			{
				this.submitHighScore();
			}
			return;
		}
		this.player.position.X = this.checkpointPosition;
		for (int j = 0; j < 6; j++)
		{
			Track runway_track = this.GetTrackForXPosition((this.checkpointPosition / (float)this.tileSize + (float)j) * (float)this.tileSize);
			if (runway_track?.obstacle != null)
			{
				runway_track.obstacle.Destroy();
				runway_track.obstacle = null;
			}
		}
		this.player.SnapToFloor();
		this.deathTimer = 0.25f;
		this.respawnCounter = 1400;
	}

	public void ReapEntities()
	{
		float reap_position = this.checkpointPosition - 96f - (float)(4 * this.tileSize);
		foreach (int grid_position in new List<int>(this._tracks.Keys))
		{
			if ((float)grid_position < reap_position / (float)this.tileSize)
			{
				for (int i = 0; i < this._tracks[grid_position].Count; i++)
				{
					Track track = this._tracks[grid_position][i];
					this._entities.Remove(track);
				}
				this._tracks.Remove(grid_position);
			}
		}
	}

	public void receiveLeftClick(int x, int y, bool playSound = true)
	{
	}

	public void releaseLeftClick(int x, int y)
	{
	}

	public void releaseRightClick(int x, int y)
	{
	}

	public void receiveRightClick(int x, int y, bool playSound = true)
	{
	}

	public void receiveKeyPress(Keys k)
	{
		if (Game1.input.GetGamePadState().IsButtonDown(Buttons.Back) || k.Equals(Keys.Escape))
		{
			this.QuitGame();
		}
		else if ((this.CanPause() && !Game1.options.gamepadControls && (k.Equals(Keys.P) || k.Equals(Keys.Enter))) || (Game1.options.gamepadControls && Game1.input.GetGamePadState().IsButtonDown(Buttons.Start)))
		{
			this.gamePaused = !this.gamePaused;
			if (this.gamePaused)
			{
				Game1.playSound("bigSelect");
			}
			else
			{
				Game1.playSound("bigDeSelect");
			}
		}
	}

	public void receiveKeyRelease(Keys k)
	{
	}

	public void ResetState()
	{
		this.gameOver = false;
		this.screenLeftBound = 0f;
		this.respawnCounter = 0;
		this.deathTimer = 0f;
		this._spawnedFruit = new HashSet<CollectableFruits>();
		this.sparkShower.Clear();
		this._goalIndicator = null;
		this.checkpointPositions = new List<int>();
		this._tracks = new Dictionary<int, List<Track>>();
		this._entities = new List<Entity>();
		this.player = this.AddEntity(new PlayerMineCartCharacter());
		this.player.position.X = 0f;
		this.player.position.Y = this.ytileOffset * this.tileSize;
		this.generatorPosition.X = 0;
		this.generatorPosition.Y = this.ytileOffset + 1;
		this._lastGenerator = null;
		this._trackGenerator = null;
		this._forcedNextGenerator = null;
		this.trackBuilderCharacter = this.AddEntity(new MineCartCharacter());
		this.trackBuilderCharacter.visible = false;
		this.trackBuilderCharacter.enabled = false;
		this._lastTilePosition = 0;
		this.pauseBeforeTitleFadeOutTimer = 0f;
		this.lakeDecor.Clear();
		this.obstacles.Clear();
		this.reachedFinish = false;
	}

	public void QuitGame()
	{
		this.unload();
		Game1.playSound("bigDeSelect");
		Game1.currentMinigame = null;
	}

	private void restartLevel(bool new_game = false)
	{
		if (new_game)
		{
			this.livesLeft = 3;
			this._collectedFruit.Clear();
			this.coinCount = 0;
			this.score = 0;
			this.levelsBeat = 0;
		}
		this.ResetState();
		if ((this.levelsBeat > 0 && this._collectedFruit.Count > 0) || (this.livesLeft < 3 && !new_game))
		{
			this.ShowFruitsSummary();
		}
		else
		{
			this.ShowMap();
		}
	}

	public void ShowFruitsSummary()
	{
		Game1.changeMusicTrack("none", track_interruptable: false, MusicContext.MiniGame);
		if (!this.minecartLoop.IsPaused)
		{
			this.minecartLoop.Pause();
		}
		this.gameState = GameStates.FruitsSummary;
		this.player.enabled = false;
		this.stateTimer = 0.75f;
	}

	public void ShowMap()
	{
		if (this.gameMode == 2)
		{
			this.ShowCutscene();
			return;
		}
		this.gameState = GameStates.Map;
		this.mapTimer = 0f;
		this.screenDarkness = 1f;
		this.ResetState();
		this.player.enabled = false;
		Game1.changeMusicTrack("none", track_interruptable: false, MusicContext.MiniGame);
		this.AddEntity(new MineDebris(new Rectangle(256, 864, 16, 16), new Vector2(261f, 106f), 0f, 0f, 0f, 0f, 99f, 1f, 4, 0.15f, 0.2f)
		{
			ySinWaveMagnitude = Game1.random.Next(1, 6)
		});
		this.AddEntity(new MineDebris(new Rectangle(256, 864, 16, 16), new Vector2(276f, 117f), 0f, 0f, 0f, 0f, 99f, 1f, 4, 0.17f, 0.2f)
		{
			ySinWaveMagnitude = Game1.random.Next(1, 6)
		});
		this.AddEntity(new MineDebris(new Rectangle(256, 864, 16, 16), new Vector2(234f, 136f), 0f, 0f, 0f, 0f, 99f, 1f, 4, 0.19f, 0.2f)
		{
			ySinWaveMagnitude = Game1.random.Next(1, 6)
		});
		this.AddEntity(new MineDebris(new Rectangle(256, 864, 16, 16), new Vector2(264f, 131f), 0f, 0f, 0f, 0f, 99f, 1f, 4, 0.19f, 0.2f)
		{
			ySinWaveMagnitude = Game1.random.Next(1, 6)
		});
		if (Game1.random.NextDouble() < 0.4)
		{
			this.AddEntity(new MineDebris(new Rectangle(256, 864, 16, 16), new Vector2(247f, 119f), 0f, 0f, 0f, 0f, 99f, 1f, 4, 0.19f, 0.2f)
			{
				ySinWaveMagnitude = Game1.random.Next(1, 6)
			});
		}
		this.AddEntity(new MineDebris(new Rectangle(96, 864, 16, 16), new Vector2(327f, 186f), 0f, 0f, 0f, 0f, 99f, 1f, 4, 0.17f, 0.55f));
		this.AddEntity(new MineDebris(new Rectangle(96, 864, 16, 16), new Vector2(362f, 190f), 0f, 0f, 0f, 0f, 99f, 1f, 4, 0.19f, 0.55f));
		this.AddEntity(new MineDebris(new Rectangle(96, 864, 16, 16), new Vector2(299f, 197f), 0f, 0f, 0f, 0f, 99f, 1f, 4, 0.21f, 0.55f));
		this.AddEntity(new MineDebris(new Rectangle(96, 864, 16, 16), new Vector2(375f, 212f), 0f, 0f, 0f, 0f, 99f, 1f, 4, 0.16f, 0.55f));
		this.AddEntity(new MineDebris(new Rectangle(402, 660, 100, 72), new Vector2(205f, 184f), 0f, 0f, 0f, 0f, 99f, 1f, 2, 0.765f, 0.55f));
		this.AddEntity(new MineDebris(new Rectangle(0, 736, 48, 50), new Vector2(280f, 66f), 0f, 0f, 0f, 0f, 99f, 1f, 2, 0.765f, 0.55f));
		this.AddEntity(new MineDebris(new Rectangle(402, 638, 3, 21), new Vector2(234.66f, 66.66f), 0f, 0f, 0f, 0f, 99f, 1f, 4, 0.1f, 0.55f));
		switch (this.currentTheme)
		{
		case 0:
			this.AddEntity(new CosmeticFallingBoulder(72f, new Color(130, 96, 79), 96f, 0.45f)).position = new Vector2(40 + Game1.random.Next(40), -16f);
			if (Game1.random.NextBool())
			{
				this.AddEntity(new CosmeticFallingBoulder(72f, new Color(130, 96, 79), 80f, 0.5f)).position = new Vector2(80 + Game1.random.Next(40), -16f);
			}
			if (Game1.random.NextBool())
			{
				this.AddEntity(new CosmeticFallingBoulder(72f, new Color(130, 96, 79), 88f, 0.55f)).position = new Vector2(120 + Game1.random.Next(40), -16f);
			}
			break;
		case 1:
		{
			this.AddEntity(new MineDebris(new Rectangle(401, 604, 15, 12), new Vector2(119f, 162f), 0f, 0f, 0f, 0f, 0.8f, 1f, 1, 0.1f, 0.55f)).SetDestroySound("boulderBreak");
			this.AddEntity(new MineDebris(new Rectangle(401, 604, 15, 12), new Vector2(49f, 166f), 0f, 0f, 0f, 0f, 1.2f, 1f, 1, 0.1f, 0.55f)).SetDestroySound("boulderBreak");
			for (int m = 0; m < 4; m++)
			{
				this.AddEntity(new MineDebris(new Rectangle(421, 607, 5, 5), new Vector2(119f, 162f), Game1.random.Next(-30, 31), Game1.random.Next(-50, -39), 0.25f, 1f, 0.75f, 1f, 1, 1f, 0.45f, holdLastFrame: false, 0.8f));
			}
			for (int n = 0; n < 4; n++)
			{
				this.AddEntity(new MineDebris(new Rectangle(421, 607, 5, 5), new Vector2(49f, 166f), Game1.random.Next(-30, 31), Game1.random.Next(-50, -39), 0.25f, 1f, 0.75f, 1f, 1, 1f, 0.45f, holdLastFrame: false, 1.2f));
			}
			break;
		}
		case 3:
		{
			this.AddEntity(new MineDebris(new Rectangle(455, 512, 58, 64), new Vector2(250f, 136f), 0f, 0f, 0f, 0f, 0.8f, 1f, 1, 0.1f, 0.21f)).SetDestroySound("barrelBreak");
			for (int j = 0; j < 32; j++)
			{
				this.AddEntity(new MineDebris(new Rectangle(51, 53, 9, 9), new Vector2(250f, 136f) + new Vector2(Game1.random.Next(-20, 31), Game1.random.Next(-20, 21)), Game1.random.Next(-30, 31), Game1.random.Next(-70, -39), 0.25f, 1f, 0.75f, 1f, 1, 1f, 0.45f, holdLastFrame: false, 0.8f + 0.01f * (float)j));
			}
			break;
		}
		case 2:
			this.AddEntity(new MineDebris(new Rectangle(416, 368, 24, 16), new Vector2(217f, 177f), 0f, 0f, 0f, 0f, 99f, 1f, 4, 0.1f, 0.54f, holdLastFrame: true, 0.8f));
			this.AddEntity(new MineDebris(new Rectangle(416, 368, 1, 1), new Vector2(217f, 177f), 0f, 0f, 0f, 0f, 0.8f, 1f, 1, 0.1f, 0.55f)).SetDestroySound("pullItemFromWater");
			break;
		case 4:
			this.AddEntity(new MineDebris(new Rectangle(401, 591, 12, 11), new Vector2(328f, 197f), 0f, 0f, 0f, 0f, 99f, 1f, 4, 0.1f, 0.34f, holdLastFrame: false, 2.5f)).SetStartSound("fireball");
			this.AddEntity(new MineDebris(new Rectangle(401, 591, 12, 11), new Vector2(336f, 197f), 0f, 0f, 0f, 0f, 99f, 1f, 4, 0.1f, 0.35f, holdLastFrame: false, 2.625f));
			this.AddEntity(new MineDebris(new Rectangle(401, 591, 12, 11), new Vector2(344f, 197f), 0f, 0f, 0f, 0f, 99f, 1f, 4, 0.1f, 0.34f, holdLastFrame: false, 2.75f)).SetStartSound("fireball");
			this.AddEntity(new MineDebris(new Rectangle(401, 591, 12, 11), new Vector2(344f, 189f), 0f, 0f, 0f, 0f, 99f, 1f, 4, 0.1f, 0.35f, holdLastFrame: false, 2.825f));
			this.AddEntity(new MineDebris(new Rectangle(401, 591, 12, 11), new Vector2(344f, 181f), 0f, 0f, 0f, 0f, 99f, 1f, 4, 0.1f, 0.34f, holdLastFrame: false, 3f)).SetStartSound("fireball");
			this.AddEntity(new MineDebris(new Rectangle(401, 591, 12, 11), new Vector2(344f, 173f), 0f, 0f, 0f, 0f, 99f, 1f, 4, 0.1f, 0.35f, holdLastFrame: false, 3.125f));
			this.AddEntity(new MineDebris(new Rectangle(401, 591, 12, 11), new Vector2(344f, 165f), 0f, 0f, 0f, 0f, 99f, 1f, 4, 0.1f, 0.34f, holdLastFrame: false, 3.25f)).SetStartSound("fireball");
			this.AddEntity(new MineDebris(new Rectangle(401, 591, 12, 11), new Vector2(352f, 165f), 0f, 0f, 0f, 0f, 99f, 1f, 4, 0.1f, 0.35f, holdLastFrame: false, 3.325f));
			this.AddEntity(new MineDebris(new Rectangle(401, 591, 12, 11), new Vector2(360f, 165f), 0f, 0f, 0f, 0f, 99f, 1f, 4, 0.1f, 0.34f, holdLastFrame: false, 3.5f)).SetStartSound("fireball");
			this.AddEntity(new MineDebris(new Rectangle(401, 591, 12, 11), new Vector2(360f, 157f), 0f, 0f, 0f, 0f, 99f, 1f, 4, 0.1f, 0.35f, holdLastFrame: false, 3.625f));
			this.AddEntity(new MineDebris(new Rectangle(401, 591, 12, 11), new Vector2(360f, 149f), 0f, 0f, 0f, 0f, 99f, 1f, 4, 0.1f, 0.34f, holdLastFrame: false, 3.75f)).SetStartSound("fireball");
			break;
		case 5:
		{
			this.AddEntity(new MineDebris(new Rectangle(416, 384, 16, 16), new Vector2(213f, 34f), 0f, 0f, 0f, 0f, 5f, 1f, 6, 0.1f, 0.55f)).SetDestroySound("slimedead");
			for (int l = 0; l < 8; l++)
			{
				this.AddEntity(new MineDebris(new Rectangle(427, 607, 6, 6), new Vector2(205 + Game1.random.Next(3, 14), 26 + Game1.random.Next(6, 14)), Game1.random.Next(-30, 31), Game1.random.Next(-60, -39), 0.25f, 1f, 0.75f, 1f, 1, 1f, 0.45f, holdLastFrame: false, 5f + (float)l * 0.005f));
			}
			break;
		}
		case 9:
		{
			for (int k = 0; k < 8; k++)
			{
				this.AddEntity(new MineDebris(new Rectangle(368, 784, 16, 16), new Vector2(274 + Game1.random.Next(-19, 20), 46 + Game1.random.Next(6, 14)), Game1.random.Next(-4, 5), -16f, 0f, 0.05f, 2f, 1f, 3, 0.33f, 0.35f, holdLastFrame: true, 1f + (float)k * 0.1f)).SetStartSound("dirtyHit");
			}
			break;
		}
		case 6:
		{
			for (int i = 0; i < 52; i++)
			{
				this.AddEntity(new CosmeticFallingBoulder(Game1.random.Next(72, 195), new Color(100, 66, 49), 96 + Game1.random.Next(-10, 11), 0.65f + (float)i * 0.05f)).position = new Vector2(5 + Game1.random.Next(360), -16f);
			}
			break;
		}
		}
		if (!this.levelThemesFinishedThisRun.Contains(1))
		{
			this.AddEntity(new MineDebris(new Rectangle(401, 604, 15, 12), new Vector2(119f, 162f), 0f, 0f, 0f, 0f, 99f, 1f, 1, 0.1f, 0.55f));
			this.AddEntity(new MineDebris(new Rectangle(401, 604, 15, 12), new Vector2(49f, 166f), 0f, 0f, 0f, 0f, 99f, 1f, 1, 0.1f, 0.55f));
		}
		this.AddEntity(new MineDebris(new Rectangle(415, this.levelThemesFinishedThisRun.Contains(0) ? 630 : 650, 10, 9), new Vector2(88f, 87.66f), 0f, 0f, 0f, 0f, 99f, 1f, 5, 0.1f, 0.55f));
		this.AddEntity(new MineDebris(new Rectangle(415, this.levelThemesFinishedThisRun.Contains(1) ? 630 : 650, 10, 9), new Vector2(105f, 183.66f), 0f, 0f, 0f, 0f, 99f, 1f, 5, 0.1f, 0.55f));
		this.AddEntity(new MineDebris(new Rectangle(415, this.levelThemesFinishedThisRun.Contains(5) ? 630 : 640, 10, 9), new Vector2(169f, 119.66f), 0f, 0f, 0f, 0f, 99f, 1f, 5, 0.1f, 0.55f));
		this.AddEntity(new MineDebris(new Rectangle(415, this.levelThemesFinishedThisRun.Contains(4) ? 630 : 650, 10, 9), new Vector2(328f, 199.66f), 0f, 0f, 0f, 0f, 99f, 1f, 5, 0.1f, 0.55f));
		this.AddEntity(new MineDebris(new Rectangle(415, this.levelThemesFinishedThisRun.Contains(6) ? 630 : 650, 10, 9), new Vector2(361f, 72.66f), 0f, 0f, 0f, 0f, 99f, 1f, 5, 0.1f, 0.55f));
		if (this.levelThemesFinishedThisRun.Contains(2))
		{
			this.AddEntity(new MineDebris(new Rectangle(466, 642, 17, 17), new Vector2(216.66f, 200.66f), 0f, 0f, 0f, 0f, 99f, 1f, 1, 0.17f, 0.52f));
		}
		this.fadeDelta = -1f;
		MapJunimo map_junimo = this.AddEntity(new MapJunimo());
		LevelTransition[] lEVEL_TRANSITIONS = this.LEVEL_TRANSITIONS;
		foreach (LevelTransition transition in lEVEL_TRANSITIONS)
		{
			if (transition.startLevel == this.currentTheme && (transition.shouldTakePath == null || transition.shouldTakePath()))
			{
				map_junimo.position = new Vector2(((float)transition.startGridCoordinates.X + 0.5f) * (float)this.tileSize, ((float)transition.startGridCoordinates.Y + 0.5f) * (float)this.tileSize);
				map_junimo.moveString = transition.pathString;
				this.currentTheme = transition.destinationLevel;
				break;
			}
		}
	}

	public void ShowCutscene()
	{
		this.gameState = GameStates.Cutscene;
		this.screenDarkness = 1f;
		this.ResetState();
		this.player.enabled = false;
		this.setGameModeParameters();
		this.setUpTheme(this.currentTheme);
		this.cutsceneTick = 0;
		Game1.changeMusicTrack("none", track_interruptable: false, MusicContext.MiniGame);
		for (int i = 0; i < this.screenWidth / this.tileSize + 4; i++)
		{
			this.AddTrack(i, 10).visible = false;
		}
		this.player.SnapToFloor();
		if (this.gameMode == 2)
		{
			this.EndCutscene();
		}
	}

	public void PlayLevelMusic()
	{
		switch (this.currentTheme)
		{
		case 0:
			Game1.changeMusicTrack("EarthMine", track_interruptable: false, MusicContext.MiniGame);
			break;
		case 1:
			Game1.changeMusicTrack("FrostMine", track_interruptable: false, MusicContext.MiniGame);
			break;
		case 2:
			Game1.changeMusicTrack("junimoKart_whaleMusic", track_interruptable: false, MusicContext.MiniGame);
			break;
		case 4:
			Game1.changeMusicTrack("tribal", track_interruptable: false, MusicContext.MiniGame);
			break;
		case 3:
			Game1.changeMusicTrack("junimoKart_ghostMusic", track_interruptable: false, MusicContext.MiniGame);
			break;
		case 5:
			Game1.changeMusicTrack("junimoKart_slimeMusic", track_interruptable: false, MusicContext.MiniGame);
			break;
		case 9:
			Game1.changeMusicTrack("junimoKart_mushroomMusic", track_interruptable: false, MusicContext.MiniGame);
			break;
		case 6:
			Game1.changeMusicTrack("nightTime", track_interruptable: false, MusicContext.MiniGame);
			break;
		case 8:
			Game1.changeMusicTrack("Upper_Ambient", track_interruptable: false, MusicContext.MiniGame);
			break;
		case 7:
			break;
		}
	}

	public void EndCutscene()
	{
		if (!this.minecartLoop.IsPaused)
		{
			this.minecartLoop.Pause();
		}
		this.gameState = GameStates.Ingame;
		Utility.CollectGarbage();
		this.ResetState();
		this.setUpTheme(this.currentTheme);
		this.PlayLevelMusic();
		this.player.enabled = true;
		this.createBeginningOfLevel();
		this.player.position.X = (float)this.tileSize * 0.5f;
		this.player.SnapToFloor();
		this.checkpointPosition = this.player.position.X;
		this.furthestGeneratedCheckpoint = 0;
		this.lastLevelWasPerfect = true;
		this.secondsOnThisLevel = 0f;
		if (this.currentTheme == 2)
		{
			this.AddEntity(new Whale());
			this.AddEntity(new PlayerBubbleSpawner());
		}
		if (this.currentTheme == 5)
		{
			this.AddEntity(new HugeSlime()).position = new Vector2(0f, 0f);
		}
		this.screenDarkness = 1f;
		this.fadeDelta = -1f;
		if (this.gameMode == 3 && this.currentTheme == 7)
		{
			Game1.player.stats.Increment("completedJunimoKart");
			Game1.player.AddMissedMailAndRecipes();
			Game1.multiplayer.globalChatInfoMessage("JunimoKart", Game1.player.Name);
			this.unload();
			Game1.globalFadeToClear(delegate
			{
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\StringsFromCSFiles:MineCart.cs.12106"));
			}, 0.015f);
			Game1.currentMinigame = null;
			DelayedAction.playSoundAfterDelay("discoverMineral", 1000);
		}
	}

	public void createSparkShower(Vector2 position)
	{
		int number = Game1.random.Next(3, 7);
		for (int i = 0; i < number; i++)
		{
			this.sparkShower.Add(new Spark(position.X - 3f, position.Y, (float)Game1.random.Next(-200, 5) / 100f, (float)(-Game1.random.Next(5, 150)) / 100f));
		}
	}

	public void createSparkShower()
	{
		int number = Game1.random.Next(3, 7);
		for (int i = 0; i < number; i++)
		{
			this.sparkShower.Add(new Spark(this.player.drawnPosition.X - 3f, this.player.drawnPosition.Y, (float)Game1.random.Next(-200, 5) / 100f, (float)(-Game1.random.Next(5, 150)) / 100f));
		}
	}

	public void CreateLakeDecor()
	{
		for (int i = 0; i < 16; i++)
		{
			this.lakeDecor.Add(new LakeDecor(this, this.currentTheme));
		}
	}

	public void CreateBGDecor()
	{
		for (int i = 0; i < 16; i++)
		{
			this.lakeDecor.Add(new LakeDecor(this, this.currentTheme, bgDecor: true, i));
		}
	}

	public void createBeginningOfLevel()
	{
		this.CreateLakeDecor();
		for (int i = 0; i < 15; i++)
		{
			this.AddTrack(this.generatorPosition.X, this.generatorPosition.Y);
			this.generatorPosition.X++;
		}
	}

	public void setGameModeParameters()
	{
		switch (this.gameMode)
		{
		case 3:
			this.distanceToTravel = 350;
			break;
		case 2:
			this.distanceToTravel = 150;
			break;
		}
	}

	public void AddValidObstacle(ObstacleTypes obstacle_type, Type type)
	{
		if (this._validObstacles != null)
		{
			if (!this._validObstacles.TryGetValue(obstacle_type, out var obstacleTypes))
			{
				obstacleTypes = (this._validObstacles[obstacle_type] = new List<Type>());
			}
			obstacleTypes.Add(type);
		}
	}

	public void setUpTheme(int whichTheme)
	{
		this._generatorRolls = new List<GeneratorRoll>();
		this._validObstacles = new Dictionary<ObstacleTypes, List<Type>>();
		float additional_trap_spawn_rate = 0f;
		float movement_speed_multiplier = 1f;
		if (this.gameState == GameStates.Cutscene)
		{
			additional_trap_spawn_rate = 0f;
			movement_speed_multiplier = 1f;
		}
		else if (this.gameMode == 2)
		{
			int cycle_completions = this.levelsBeat / this.infiniteModeLevels.Length;
			additional_trap_spawn_rate = (float)cycle_completions * 0.25f;
			movement_speed_multiplier = 1f + (float)cycle_completions * 0.25f;
		}
		this.midBGSource = new Rectangle(64, 0, 96, 162);
		this.backBGSource = new Rectangle(64, 162, 96, 111);
		this.lakeBGSource = new Rectangle(0, 80, 16, 97);
		this.backBGYOffset = this.tileSize * 2;
		this.midBGYOffset = 0;
		switch (whichTheme)
		{
		case 9:
			this.AddValidObstacle(ObstacleTypes.Difficult, typeof(NoxiousMushroom));
			this._generatorRolls.Add(new GeneratorRoll(0.1f, new MushroomBalanceTrackGenerator(this).SetHopSize(2, 2).SetReleaseJumpChance(1f).SetStaggerValues(0, -1, 3)
				.SetTrackType(Track.TrackType.Straight)));
			this._generatorRolls.Add(new GeneratorRoll(0.15f, new MushroomBalanceTrackGenerator(this).SetHopSize(1, 1).SetReleaseJumpChance(1f).SetStaggerValues(-2, 4)
				.SetTrackType(Track.TrackType.Straight)));
			this._generatorRolls.Add(new GeneratorRoll(0.2f, new StraightAwayGenerator(this).SetMinimumDistanceBetweenStaggers(1).SetStaggerChance(1f).SetStaggerValues(-1, 0, 1)
				.SetLength(4, 4)
				.SetCheckpoint(checkpoint: true)));
			this._generatorRolls.Add(new GeneratorRoll(0.25f, new BunnyHopGenerator(this).SetHopSize(2, 3).SetStaggerValues(4, 3).SetNumberOfHops(1, 1)
				.SetReleaseJumpChance(0f)));
			this._generatorRolls.Add(new GeneratorRoll(0.25f, new StraightAwayGenerator(this).SetMinimumDistanceBetweenStaggers(2).SetStaggerChance(0f).SetLength(7, 7)
				.AddObstacle<StraightAwayGenerator>(ObstacleTypes.Difficult, 3)
				.SetCheckpoint(checkpoint: false)));
			this._generatorRolls.Add(new GeneratorRoll(0.2f, new MushroomBunnyHopGenerator(this).SetHopSize(1, 1).SetNumberOfHops(2, 3).SetStaggerValues(-3, -1, 2, 3)
				.SetReleaseJumpChance(0.25f)
				.AddPickupFunction<MushroomBunnyHopGenerator>(BaseTrackGenerator.Always)));
			this._generatorRolls.Add(new GeneratorRoll(0.05f, new BunnyHopGenerator(this).SetHopSize(1, 1).SetNumberOfHops(2, 3).SetStaggerValues(-3, -1, 2, 3)
				.SetReleaseJumpChance(0.33f)
				.AddPickupFunction<BunnyHopGenerator>(BaseTrackGenerator.Always)));
			this._generatorRolls.Add(new GeneratorRoll(0.35f, new BunnyHopGenerator(this).SetTrackType(Track.TrackType.MushroomMiddle).SetHopSize(1, 1).SetNumberOfHops(2, 3)
				.SetStaggerValues(-3, -4, 4)
				.SetReleaseJumpChance(0.33f)
				.AddPickupFunction<BunnyHopGenerator>(BaseTrackGenerator.Always)));
			this._generatorRolls.Add(new GeneratorRoll(0.5f, new MushroomBalanceTrackGenerator(this).SetHopSize(1, 1).SetReleaseJumpChance(1f).SetStaggerValues(-2, 4)
				.SetTrackType(Track.TrackType.Straight)));
			this._generatorRolls.Add(new GeneratorRoll(1f, new StraightAwayGenerator(this).SetMinimumDistanceBetweenStaggers(1).SetStaggerChance(1f).SetStaggerValues(2, -1, 0, 1, 2)
				.SetLength(3, 5)
				.SetCheckpoint(checkpoint: true)));
			this.CreateBGDecor();
			this.backBGTint = Color.White;
			this.backBGSource = new Rectangle(0, 789, 96, 111);
			this.midBGTint = Color.White;
			this.caveTint = Color.Purple;
			this.lakeBGSource = new Rectangle(304, 0, 16, 0);
			this.lakeTint = new Color(0, 8, 46);
			this.midBGSource = new Rectangle(416, 736, 96, 149);
			this.midBGYOffset = -13;
			this.waterfallTint = new Color(100, 0, 140) * 0.5f;
			this.trackTint = new Color(130, 50, 230);
			this.player.velocity.X = 120f;
			this.trackShadowTint = new Color(0, 225, 225);
			break;
		case 1:
		{
			this.AddValidObstacle(ObstacleTypes.Normal, typeof(Roadblock));
			this.AddValidObstacle(ObstacleTypes.Difficult, typeof(Roadblock));
			BaseTrackGenerator wavy_generator = new StraightAwayGenerator(this).SetMinimumDistanceBetweenStaggers(2).SetStaggerChance(1f).SetStaggerValueRange(-1, 1)
				.SetLength(4, 4)
				.SetCheckpoint(checkpoint: true);
			this._generatorRolls.Add(new GeneratorRoll(0.3f, new BunnyHopGenerator(this).SetHopSize(1, 1).SetNumberOfHops(2, 4).SetReleaseJumpChance(0.1f)
				.SetStaggerValues(-2, -1)
				.SetTrackType(Track.TrackType.UpSlope), _IsGeneratingOnLowerHalf, wavy_generator));
			this._generatorRolls.Add(new GeneratorRoll(0.15f, new BunnyHopGenerator(this).SetHopSize(1, 1).SetNumberOfHops(2, 4).SetReleaseJumpChance(0.1f)
				.SetStaggerValues(3, 2, 1)
				.SetTrackType(Track.TrackType.UpSlope), _IsGeneratingOnUpperHalf, wavy_generator));
			this._generatorRolls.Add(new GeneratorRoll(0.5f, new StraightAwayGenerator(this).SetMinimumDistanceBetweenStaggers(0).SetStaggerChance(1f).SetStaggerValues(1)
				.SetLength(3, 5)
				.AddPickupFunction<StraightAwayGenerator>(BaseTrackGenerator.IceDownSlopesOnly)
				.AddObstacle<StraightAwayGenerator>(ObstacleTypes.Normal, -12)));
			this._generatorRolls.Add(new GeneratorRoll(0.3f, wavy_generator));
			this._generatorRolls.Add(new GeneratorRoll(1f, new StraightAwayGenerator(this).SetMinimumDistanceBetweenStaggers(2).SetStaggerChance(1f).SetStaggerValueRange(-1, 1)
				.SetLength(3, 6)
				.AddObstacle<StraightAwayGenerator>(ObstacleTypes.Difficult, -13, 0.5f + additional_trap_spawn_rate)));
			this.backBGTint = new Color(93, 242, 255);
			this.midBGTint = Color.White;
			this.caveTint = new Color(230, 244, 254);
			this.lakeBGSource = new Rectangle(304, 0, 16, 0);
			this.lakeTint = new Color(147, 217, 255);
			this.midBGSource = new Rectangle(320, 135, 96, 149);
			this.midBGYOffset = -13;
			this.waterfallTint = Color.LightCyan * 0.5f;
			this.trackTint = new Color(186, 240, 255);
			this.player.velocity.X = 85f;
			NoiseGenerator.Amplitude = 2.8;
			NoiseGenerator.Frequency = 0.18;
			this.trackShadowTint = new Color(50, 145, 250);
			break;
		}
		case 2:
			this.backBGTint = Color.White;
			this.midBGTint = Color.White;
			this.caveTint = Color.SlateGray;
			this.lakeTint = new Color(75, 104, 88);
			this.waterfallTint = Color.White * 0f;
			this.trackTint = new Color(100, 220, 255);
			this.player.velocity.X = 85f;
			NoiseGenerator.Amplitude = 3.0;
			NoiseGenerator.Frequency = 0.15;
			this.trackShadowTint = new Color(32, 45, 180);
			this.midBGSource = new Rectangle(416, 0, 96, 69);
			this.backBGSource = new Rectangle(320, 0, 96, 135);
			this.backBGYOffset = 0;
			this.lakeBGSource = new Rectangle(304, 0, 16, 0);
			this._generatorRolls.Add(new GeneratorRoll(0.1f, new SmallGapGenerator(this).SetLength(2, 5).SetDepth(-7, -3).AddPickupFunction<SmallGapGenerator>(BaseTrackGenerator.Always)));
			this._generatorRolls.Add(new GeneratorRoll(0.1f, new SmallGapGenerator(this).SetLength(1, 3).SetDepth(100, 100)));
			this._generatorRolls.Add(new GeneratorRoll(1f, new StraightAwayGenerator(this).SetMinimumDistanceBetweenStaggers(1).SetStaggerChance(1f).SetStaggerValues(2, -1, 0, 1, 2)
				.SetLength(3, 5)
				.SetCheckpoint(checkpoint: true)));
			this.CreateBGDecor();
			if (this.gameMode != 2)
			{
				this.distanceToTravel = 300;
			}
			break;
		case 4:
			this.AddValidObstacle(ObstacleTypes.Normal, typeof(FallingBoulderSpawner));
			this.backBGTint = new Color(255, 137, 82);
			this.midBGTint = new Color(255, 82, 40);
			this.caveTint = Color.DarkRed;
			this.lakeTint = Color.Red;
			this.lakeBGSource = new Rectangle(304, 97, 16, 97);
			this.trackTint = new Color(255, 160, 160);
			this.waterfallTint = Color.Red * 0.9f;
			this.trackShadowTint = Color.Orange;
			this.player.velocity.X = 120f;
			NoiseGenerator.Amplitude = 3.0;
			NoiseGenerator.Frequency = 0.18;
			this._generatorRolls.Add(new GeneratorRoll(1f, new BunnyHopGenerator(this).SetHopSize(1, 1).SetNumberOfHops(3, 5).SetStaggerValues(-3, -1, 1, 3)
				.SetReleaseJumpChance(0.33f)
				.AddPickupFunction<BunnyHopGenerator>(BaseTrackGenerator.Always)));
			this._generatorRolls.Add(new GeneratorRoll(1f, new StraightAwayGenerator(this).SetMinimumDistanceBetweenStaggers(0).SetStaggerChance(1f).SetStaggerValues(-1, 1)
				.SetLength(5, 8)
				.AddPickupFunction<StraightAwayGenerator>(BaseTrackGenerator.Always)
				.SetCheckpoint(checkpoint: true)
				.AddObstacle<StraightAwayGenerator>(ObstacleTypes.Normal, -13, 0.5f + additional_trap_spawn_rate)));
			this._generatorRolls.Add(new GeneratorRoll(1f, new StraightAwayGenerator(this).SetMinimumDistanceBetweenStaggers(0).SetStaggerChance(1f).SetStaggerValues(-1, 1)
				.SetLength(5, 8)
				.AddPickupFunction<StraightAwayGenerator>(BaseTrackGenerator.Always)
				.SetCheckpoint(checkpoint: true)
				.AddObstacle<StraightAwayGenerator>(ObstacleTypes.Normal, -13, 0.5f + additional_trap_spawn_rate)));
			break;
		case 3:
			this.backBGTint = new Color(60, 60, 60);
			this.midBGTint = new Color(60, 60, 60);
			this.caveTint = new Color(70, 70, 70);
			this.lakeTint = new Color(60, 70, 80);
			this.trackTint = Color.DimGray;
			this.waterfallTint = Color.Black * 0f;
			this.trackShadowTint = Color.Black;
			this.player.velocity.X = 120f;
			NoiseGenerator.Amplitude = 3.0;
			NoiseGenerator.Frequency = 0.2;
			this.AddValidObstacle(ObstacleTypes.Normal, typeof(Roadblock));
			this.AddValidObstacle(ObstacleTypes.Difficult, typeof(WillOWisp));
			this._generatorRolls.Add(new GeneratorRoll(0.25f, new SmallGapGenerator(this).SetLength(3, 5).SetDepth(-10, -6)));
			this._generatorRolls.Add(new GeneratorRoll(0.1f, new SmallGapGenerator(this).SetLength(1, 3).SetDepth(3, 3)));
			this._generatorRolls.Add(new GeneratorRoll(0.25f, new BunnyHopGenerator(this).SetHopSize(2, 3).SetStaggerValues(4, 3).SetNumberOfHops(1, 1)
				.SetReleaseJumpChance(0f)));
			this._generatorRolls.Add(new GeneratorRoll(0.25f, new StraightAwayGenerator(this).SetMinimumDistanceBetweenStaggers(2).SetStaggerChance(1f).SetStaggerValues(-1, 0, 0, -1)
				.SetLength(7, 9)
				.AddObstacle<StraightAwayGenerator>(ObstacleTypes.Difficult, -10)
				.AddPickupFunction<StraightAwayGenerator>(BaseTrackGenerator.EveryOtherTile)
				.AddObstacle<StraightAwayGenerator>(ObstacleTypes.Normal, -13, 0.75f + additional_trap_spawn_rate)));
			this._generatorRolls.Add(new GeneratorRoll(1f, new StraightAwayGenerator(this).SetMinimumDistanceBetweenStaggers(2).SetStaggerChance(1f).SetStaggerValues(4, -1, 0, 1, -4)
				.SetLength(2, 6)
				.AddPickupFunction<StraightAwayGenerator>(BaseTrackGenerator.EveryOtherTile)));
			if (this.gameMode != 2)
			{
				this.distanceToTravel = 450;
			}
			else
			{
				this.distanceToTravel = (int)((float)this.distanceToTravel * 1.5f);
			}
			this.CreateBGDecor();
			break;
		case 5:
			this.AddValidObstacle(ObstacleTypes.Air, typeof(FallingBoulderSpawner));
			this.AddValidObstacle(ObstacleTypes.Normal, typeof(Roadblock));
			this.backBGTint = new Color(180, 250, 180);
			this.midBGSource = new Rectangle(416, 69, 96, 162);
			this.midBGTint = Color.White;
			this.caveTint = new Color(255, 200, 60);
			this.lakeTint = new Color(24, 151, 62);
			this.trackTint = Color.LightSlateGray;
			this.waterfallTint = new Color(0, 255, 180) * 0.5f;
			this.trackShadowTint = new Color(0, 180, 50);
			this.player.velocity.X = 100f;
			this.slimeBossSpeed = this.player.velocity.X;
			NoiseGenerator.Amplitude = 3.1;
			NoiseGenerator.Frequency = 0.24;
			this.lakeBGSource = new Rectangle(304, 0, 16, 0);
			this._generatorRolls.Add(new GeneratorRoll(0.1f, new BunnyHopGenerator(this).SetHopSize(2, 3).SetStaggerValues(10, 10).SetNumberOfHops(1, 1)
				.SetReleaseJumpChance(0.1f)));
			this._generatorRolls.Add(new GeneratorRoll(0.1f, new SmallGapGenerator(this).SetLength(2, 5).SetDepth(-7, -3).AddPickupFunction<SmallGapGenerator>(BaseTrackGenerator.Always)));
			this._generatorRolls.Add(new GeneratorRoll(0.25f, new StraightAwayGenerator(this).SetMinimumDistanceBetweenStaggers(0).SetStaggerChance(1f).SetStaggerValueRange(-1, -1)
				.SetLength(3, 5)
				.AddObstacle<StraightAwayGenerator>(ObstacleTypes.Air, -11, 0.75f + additional_trap_spawn_rate)
				.AddPickupFunction<SmallGapGenerator>(BaseTrackGenerator.Always)));
			this._generatorRolls.Add(new GeneratorRoll(0.1f, new BunnyHopGenerator(this).SetHopSize(1, 1).SetStaggerValues(1, -2).SetNumberOfHops(2, 2)
				.SetReleaseJumpChance(0.25f)
				.AddPickupFunction<BunnyHopGenerator>(BaseTrackGenerator.Always)
				.SetTrackType(Track.TrackType.SlimeUpSlope)));
			this._generatorRolls.Add(new GeneratorRoll(1f, new StraightAwayGenerator(this).SetMinimumDistanceBetweenStaggers(1).SetStaggerChance(1f).SetStaggerValues(-1, -1, 0, 2, 2)
				.SetLength(3, 5)
				.AddObstacle<StraightAwayGenerator>(ObstacleTypes.Normal, -10, 0.3f + additional_trap_spawn_rate)));
			break;
		case 6:
			this.backBGTint = Color.White;
			this.midBGTint = Color.White;
			this.caveTint = Color.Black;
			this.lakeTint = Color.Black;
			this.waterfallTint = Color.BlueViolet * 0.25f;
			this.trackTint = new Color(150, 70, 120);
			this.player.velocity.X = 110f;
			NoiseGenerator.Amplitude = 3.5;
			NoiseGenerator.Frequency = 0.35;
			this.trackShadowTint = Color.Black;
			this.midBGSource = new Rectangle(416, 231, 96, 53);
			this.backBGSource = new Rectangle(320, 284, 96, 116);
			this.backBGYOffset = 20;
			this.AddValidObstacle(ObstacleTypes.Normal, typeof(Roadblock));
			this._generatorRolls.Add(new GeneratorRoll(0.25f, new RapidHopsGenerator(this).SetLength(3, 5).SetYStep(-1).AddPickupFunction<RapidHopsGenerator>(BaseTrackGenerator.Always)));
			this._generatorRolls.Add(new GeneratorRoll(0.25f, new RapidHopsGenerator(this).SetLength(3, 5).SetYStep(2).SetChaotic(chaotic: true)
				.AddPickupFunction<RapidHopsGenerator>(BaseTrackGenerator.Always)));
			this._generatorRolls.Add(new GeneratorRoll(0.1f, new RapidHopsGenerator(this).SetLength(3, 5).SetYStep(-2)));
			this._generatorRolls.Add(new GeneratorRoll(0.05f, new RapidHopsGenerator(this).SetLength(3, 5).SetYStep(3)));
			this._generatorRolls.Add(new GeneratorRoll(0.1f, new BunnyHopGenerator(this).SetHopSize(2, 3).SetStaggerValues(4, 3).SetNumberOfHops(1, 1)
				.SetReleaseJumpChance(0f)));
			this._generatorRolls.Add(new GeneratorRoll(0.1f, new BunnyHopGenerator(this).SetHopSize(1, 1).SetNumberOfHops(3, 5).SetStaggerValues(-3, -1, 1, 3)
				.SetReleaseJumpChance(0.33f)
				.AddPickupFunction<BunnyHopGenerator>(BaseTrackGenerator.Always)));
			this._generatorRolls.Add(new GeneratorRoll(1f, new StraightAwayGenerator(this).SetMinimumDistanceBetweenStaggers(1).SetStaggerChance(1f).SetStaggerValueRange(-1, 2)
				.SetLength(3, 8)
				.AddPickupFunction<StraightAwayGenerator>(BaseTrackGenerator.EveryOtherTile)
				.AddObstacle<StraightAwayGenerator>(ObstacleTypes.Normal, -10, 0.75f + additional_trap_spawn_rate)));
			this.generatorPosition.Y = this.screenHeight / this.tileSize - 2;
			this.CreateBGDecor();
			if (this.gameMode != 2)
			{
				this.distanceToTravel = 500;
			}
			break;
		case 0:
			this.backBGTint = Color.DarkKhaki;
			this.midBGTint = Color.SandyBrown;
			this.caveTint = Color.SandyBrown;
			this.lakeTint = Color.MediumAquamarine;
			this.trackTint = Color.Beige;
			this.waterfallTint = Color.MediumAquamarine * 0.9f;
			this.trackShadowTint = new Color(60, 60, 60);
			this.player.velocity.X = 95f;
			NoiseGenerator.Amplitude = 2.0;
			NoiseGenerator.Frequency = 0.12;
			this.AddValidObstacle(ObstacleTypes.Normal, typeof(Roadblock));
			this.AddValidObstacle(ObstacleTypes.Normal, typeof(FallingBoulderSpawner));
			this._generatorRolls.Add(new GeneratorRoll(0.1f, new SmallGapGenerator(this).SetLength(1, 3).SetDepth(2, 2)));
			this._generatorRolls.Add(new GeneratorRoll(0.25f, new BunnyHopGenerator(this).SetHopSize(2, 3).SetStaggerValues(-2, -1, 1, 2).SetNumberOfHops(2, 2)
				.SetReleaseJumpChance(1f)));
			this._generatorRolls.Add(new GeneratorRoll(0.3f, new SmallGapGenerator(this).SetLength(1, 1).SetDepth(-4, -2).AddPickupFunction<SmallGapGenerator>(BaseTrackGenerator.Always)));
			this._generatorRolls.Add(new GeneratorRoll(0.1f, new SmallGapGenerator(this).SetLength(1, 4).SetDepth(-3, -3).AddPickupFunction<SmallGapGenerator>(BaseTrackGenerator.Always)));
			this._generatorRolls.Add(new GeneratorRoll(0.1f, new BunnyHopGenerator(this).SetHopSize(1, 1).SetNumberOfHops(2, 2).SetReleaseJumpChance(1f)
				.AddPickupFunction<BunnyHopGenerator>(BaseTrackGenerator.Always)));
			this._generatorRolls.Add(new GeneratorRoll(0.5f, new StraightAwayGenerator(this).SetMinimumDistanceBetweenStaggers(2).SetStaggerChance(1f).SetStaggerValues(-3, -2, -1, 2)
				.SetLength(2, 4)
				.AddObstacle<StraightAwayGenerator>(ObstacleTypes.Normal, -11, 0.3f + additional_trap_spawn_rate)));
			this._generatorRolls.Add(new GeneratorRoll(0.015f, new BunnyHopGenerator(this).SetHopSize(2, 3).SetStaggerValues(-3, -4, 4, 3).SetNumberOfHops(1, 1)
				.SetReleaseJumpChance(0.1f)));
			this._generatorRolls.Add(new GeneratorRoll(1f, new StraightAwayGenerator(this).SetMinimumDistanceBetweenStaggers(1).SetStaggerChance(1f).SetStaggerValueRange(-1, 1)
				.SetLength(3, 5)
				.AddObstacle<StraightAwayGenerator>(ObstacleTypes.Normal, -10, 0.3f + additional_trap_spawn_rate)));
			this.generatorPosition.Y = this.screenHeight / this.tileSize - 3;
			break;
		case 8:
			this.backBGTint = new Color(10, 30, 50);
			this.midBGTint = Color.Black;
			this.caveTint = Color.Black;
			this.lakeTint = new Color(0, 60, 150);
			this.trackTint = new Color(0, 90, 180);
			this.waterfallTint = Color.MediumAquamarine * 0f;
			this.trackShadowTint = new Color(0, 0, 60);
			this.player.velocity.X = 100f;
			this.generatorPosition.Y = this.screenHeight / this.tileSize - 4;
			this._generatorRolls.Add(new GeneratorRoll(0.1f, new SmallGapGenerator(this).SetLength(1, 3).SetDepth(2, 2).AddPickupFunction<SmallGapGenerator>(BaseTrackGenerator.Always)));
			this._generatorRolls.Add(new GeneratorRoll(0.25f, new BunnyHopGenerator(this).SetHopSize(2, 3).SetStaggerValues(-2, -1, 1, 2).SetNumberOfHops(2, 2)
				.SetReleaseJumpChance(1f)
				.AddPickupFunction<BunnyHopGenerator>(BaseTrackGenerator.Always)));
			this._generatorRolls.Add(new GeneratorRoll(0.3f, new SmallGapGenerator(this).SetLength(1, 1).SetDepth(-4, -2).AddPickupFunction<SmallGapGenerator>(BaseTrackGenerator.Always)));
			this._generatorRolls.Add(new GeneratorRoll(0.1f, new SmallGapGenerator(this).SetLength(1, 4).SetDepth(-3, -3).AddPickupFunction<SmallGapGenerator>(BaseTrackGenerator.Always)));
			this._generatorRolls.Add(new GeneratorRoll(0.1f, new BunnyHopGenerator(this).SetHopSize(1, 1).SetNumberOfHops(2, 2).SetReleaseJumpChance(1f)
				.AddPickupFunction<BunnyHopGenerator>(BaseTrackGenerator.Always)));
			this._generatorRolls.Add(new GeneratorRoll(0.5f, new StraightAwayGenerator(this).SetMinimumDistanceBetweenStaggers(2).SetStaggerChance(1f).SetStaggerValues(-3, -2, -1, 2)
				.SetLength(2, 4)
				.AddPickupFunction<StraightAwayGenerator>(BaseTrackGenerator.Always)));
			this._generatorRolls.Add(new GeneratorRoll(0.015f, new BunnyHopGenerator(this).SetHopSize(2, 3).SetStaggerValues(-3, -4, 4, 3).SetNumberOfHops(1, 1)
				.SetReleaseJumpChance(0.1f)
				.AddPickupFunction<BunnyHopGenerator>(BaseTrackGenerator.Always)));
			this._generatorRolls.Add(new GeneratorRoll(1f, new StraightAwayGenerator(this).SetMinimumDistanceBetweenStaggers(1).SetStaggerChance(1f).SetStaggerValueRange(-1, 1)
				.SetLength(3, 5)
				.AddPickupFunction<StraightAwayGenerator>(BaseTrackGenerator.Always)));
			if (this.gameMode != 2)
			{
				this.distanceToTravel = 200;
			}
			break;
		case 7:
			this.backBGTint = Color.DarkKhaki;
			this.midBGTint = Color.SandyBrown;
			this.caveTint = Color.SandyBrown;
			this.lakeTint = Color.MediumAquamarine;
			this.trackTint = Color.Beige;
			this.waterfallTint = Color.MediumAquamarine * 0.9f;
			this.trackShadowTint = new Color(60, 60, 60);
			this.player.velocity.X = 95f;
			break;
		}
		this.player.velocity.X *= movement_speed_multiplier;
		this.trackBuilderCharacter.velocity = this.player.velocity;
		this.currentTheme = whichTheme;
	}

	public int KeepTileInBounds(int y)
	{
		if (y < this.topTile)
		{
			return 4;
		}
		if (y > this.bottomTile)
		{
			return this.bottomTile;
		}
		return y;
	}

	public bool IsTileInBounds(int y)
	{
		if (y < this.topTile)
		{
			return false;
		}
		if (y > this.bottomTile)
		{
			return false;
		}
		return true;
	}

	public T GetOverlap<T>(ICollideable source) where T : Entity
	{
		Rectangle source_rect = source.GetBounds();
		foreach (Entity entity in this._entities)
		{
			if (entity.IsActive() && entity is ICollideable collideable_entity && entity is T match)
			{
				Rectangle other_rect = collideable_entity.GetBounds();
				if (source_rect.Intersects(other_rect))
				{
					return match;
				}
			}
		}
		return null;
	}

	public List<T> GetOverlaps<T>(ICollideable source) where T : Entity
	{
		List<T> overlaps = new List<T>();
		Rectangle source_rect = source.GetBounds();
		foreach (Entity entity in this._entities)
		{
			if (entity.IsActive() && entity is ICollideable collideable_entity && entity is T match)
			{
				Rectangle other_rect = collideable_entity.GetBounds();
				if (source_rect.Intersects(other_rect))
				{
					overlaps.Add(match);
				}
			}
		}
		return overlaps;
	}

	public Pickup CreatePickup(Vector2 position, bool fruit_only = false)
	{
		if (position.Y < (float)this.tileSize && !fruit_only)
		{
			return null;
		}
		Pickup pickup = null;
		int spawned_fruit = 0;
		for (int i = 0; i < 3 && this._spawnedFruit.Contains((CollectableFruits)i); i++)
		{
			spawned_fruit++;
		}
		if (spawned_fruit <= 2)
		{
			float boundary_position = 0f;
			switch (spawned_fruit)
			{
			case 0:
				boundary_position = 0.15f * (float)this.distanceToTravel * (float)this.tileSize;
				break;
			case 1:
				boundary_position = 0.48f * (float)this.distanceToTravel * (float)this.tileSize;
				break;
			case 2:
				boundary_position = 0.81f * (float)this.distanceToTravel * (float)this.tileSize;
				break;
			}
			if (position.X >= boundary_position)
			{
				this._spawnedFruit.Add((CollectableFruits)spawned_fruit);
				pickup = this.AddEntity((Pickup)new Fruit((CollectableFruits)spawned_fruit));
			}
		}
		if (pickup == null && !fruit_only)
		{
			pickup = this.AddEntity((Pickup)new Coin());
		}
		if (pickup != null)
		{
			pickup.position = position;
		}
		return pickup;
	}

	public void draw(SpriteBatch b)
	{
		this._shakeOffset = new Vector2(Utility.Lerp(0f - this.shakeMagnitude, this.shakeMagnitude, (float)Game1.random.NextDouble()), Utility.Lerp(0f - this.shakeMagnitude, this.shakeMagnitude, (float)Game1.random.NextDouble()));
		if (this.gamePaused)
		{
			this._shakeOffset = Vector2.Zero;
		}
		Rectangle cached_scissor_rect = b.GraphicsDevice.ScissorRectangle;
		Game1.isUsingBackToFrontSorting = true;
		b.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp, null, Utility.ScissorEnabled);
		Rectangle scissor_rect = new Rectangle((int)this.upperLeft.X, (int)this.upperLeft.Y, (int)((float)this.screenWidth * this.pixelScale), (int)((float)this.screenHeight * this.pixelScale));
		scissor_rect = Utility.ConstrainScissorRectToScreen(scissor_rect);
		b.GraphicsDevice.ScissorRectangle = scissor_rect;
		switch (this.gameState)
		{
		case GameStates.FruitsSummary:
			this.perfectText?.draw(b, this.TransformDraw(new Vector2(80f, 40f)));
			break;
		default:
		{
			for (int i = 0; i <= this.screenWidth / this.tileSize + 1; i++)
			{
				b.Draw(this.texture, this.TransformDraw(new Rectangle(i * this.tileSize - (int)this.lakeSpeedAccumulator % this.tileSize, this.tileSize * 9, this.tileSize, this.screenHeight - 96)), this.lakeBGSource, this.lakeTint, 0f, Vector2.Zero, SpriteEffects.None, 0.9f);
			}
			for (int j = 0; j < this.lakeDecor.Count; j++)
			{
				this.lakeDecor[j].Draw(b);
			}
			for (int k = 0; k <= this.screenWidth / this.backBGSource.Width + 2; k++)
			{
				b.Draw(this.texture, this.TransformDraw(new Vector2(0f - this.backBGPosition + (float)(k * this.backBGSource.Width), this.backBGYOffset)), this.backBGSource, this.backBGTint, 0f, Vector2.Zero, this.GetPixelScale(), SpriteEffects.None, 0.7f);
			}
			for (int l = 0; l < this.screenWidth / this.midBGSource.Width + 2; l++)
			{
				b.Draw(this.texture, this.TransformDraw(new Vector2(0f - this.midBGPosition + (float)(l * this.midBGSource.Width), 162 - this.midBGSource.Height + this.midBGYOffset)), this.midBGSource, this.midBGTint, 0f, Vector2.Zero, this.GetPixelScale(), SpriteEffects.None, 0.6f);
			}
			break;
		}
		case GameStates.Map:
		case GameStates.Cutscene:
			break;
		}
		foreach (Entity entity in this._entities)
		{
			if (entity.IsOnScreen())
			{
				entity.Draw(b);
			}
		}
		foreach (Spark s in this.sparkShower)
		{
			b.Draw(Game1.staminaRect, this.TransformDraw(new Rectangle((int)s.x, (int)s.y, 1, 1)), null, s.c, 0f, Vector2.Zero, SpriteEffects.None, 0.3f);
		}
		switch (this.gameState)
		{
		case GameStates.Title:
		{
			b.Draw(this.texture, this.TransformDraw(new Vector2(this.screenWidth / 2 - 128, this.screenHeight / 2 - 35)), new Rectangle(256, 409, 256, 71), Color.White, 0f, Vector2.Zero, this.GetPixelScale(), SpriteEffects.None, 0.25f);
			if (this.gameMode != 2)
			{
				break;
			}
			Vector2 score_offset = new Vector2(125f, 0f);
			Vector2 draw_position = new Vector2((float)(this.screenWidth / 2) - score_offset.X / 2f, 155f);
			for (int num = 0; num < 5 && num < this._currentHighScores.Count; num++)
			{
				Color color = Color.White;
				if (num == 0)
				{
					color = Utility.GetPrismaticColor();
				}
				KeyValuePair<string, int> score = this._currentHighScores[num];
				int score_text_width = (int)Game1.dialogueFont.MeasureString(score.Value.ToString() ?? "").X / 4;
				b.DrawString(Game1.dialogueFont, "#" + (num + 1), this.TransformDraw(draw_position), color, 0f, Vector2.Zero, this.GetPixelScale() / 4f, SpriteEffects.None, 0.199f);
				b.DrawString(Game1.dialogueFont, score.Key, this.TransformDraw(draw_position + new Vector2(16f, 0f)), color, 0f, Vector2.Zero, this.GetPixelScale() / 4f, SpriteEffects.None, 0.199f);
				b.DrawString(Game1.dialogueFont, score.Value.ToString() ?? "", this.TransformDraw(draw_position + score_offset - new Vector2(score_text_width, 0f)), color, 0f, Vector2.Zero, this.GetPixelScale() / 4f, SpriteEffects.None, 0.199f);
				Vector2 shadow_offset = new Vector2(1f, 1f);
				b.DrawString(Game1.dialogueFont, "#" + (num + 1), this.TransformDraw(draw_position + shadow_offset), Color.Black, 0f, Vector2.Zero, this.GetPixelScale() / 4f, SpriteEffects.None, 0.1999f);
				b.DrawString(Game1.dialogueFont, score.Key, this.TransformDraw(draw_position + new Vector2(16f, 0f) + shadow_offset), Color.Black, 0f, Vector2.Zero, this.GetPixelScale() / 4f, SpriteEffects.None, 0.1999f);
				b.DrawString(Game1.dialogueFont, score.Value.ToString() ?? "", this.TransformDraw(draw_position + score_offset - new Vector2(score_text_width, 0f) + shadow_offset), Color.Black, 0f, Vector2.Zero, this.GetPixelScale() / 4f, SpriteEffects.None, 0.1999f);
				draw_position.Y += 10f;
			}
			break;
		}
		case GameStates.Map:
			b.Draw(this.texture, this.TransformDraw(new Vector2(0f, 0f)), new Rectangle(0, 512, 400, 224), Color.White, 0f, Vector2.Zero, this.GetPixelScale(), SpriteEffects.None, 0.6f);
			if (!this.levelThemesFinishedThisRun.Contains(3))
			{
				b.Draw(this.texture, this.TransformDraw(new Vector2(221f, 104f)), new Rectangle(455, 512, 57, 64), Color.White, 0f, Vector2.Zero, this.GetPixelScale(), SpriteEffects.None, 0.21f);
			}
			b.Draw(this.texture, this.TransformDraw(new Vector2(369f, 51f)), new Rectangle(480, 579, 31, 32), Color.White, 0f, Vector2.Zero, this.GetPixelScale(), SpriteEffects.None, 0.21f);
			b.Draw(this.texture, this.TransformDraw(new Vector2(109f, 198f)), new Rectangle(420, 512, 25, 26), Color.White, 0f, Vector2.Zero, this.GetPixelScale(), SpriteEffects.None, 0.21f);
			b.Draw(this.texture, this.TransformDraw(new Vector2(229f, 213f)), new Rectangle(425, 541, 9, 11), Color.White, 0f, Vector2.Zero, this.GetPixelScale(), SpriteEffects.None, 0.21f);
			break;
		case GameStates.Cutscene:
		{
			float scale_adjustment = this.GetPixelScale() / 4f;
			b.DrawString(Game1.dialogueFont, this.cutsceneText, this.TransformDraw(new Vector2(this.screenWidth / 2 - (int)(Game1.dialogueFont.MeasureString(this.cutsceneText).X / 2f / 4f), 32f)), Color.White, 0f, Vector2.Zero, scale_adjustment, SpriteEffects.None, 0.199f);
			break;
		}
		default:
		{
			for (int m = 0; m < this.waterfallWidth; m += 2)
			{
				for (int n = -2; n <= this.screenHeight / this.tileSize + 1; n++)
				{
					b.Draw(this.texture, this.TransformDraw(new Vector2((float)(this.screenWidth + this.tileSize * m) - this.waterFallPosition, n * this.tileSize + (int)(this._totalTime * 48.0 + (double)(this.tileSize * 100)) % this.tileSize)), new Rectangle(48, 32, 16, 16), this.waterfallTint, 0f, Vector2.Zero, this.GetPixelScale(), SpriteEffects.None, 0.2f);
				}
			}
			break;
		}
		case GameStates.FruitsSummary:
			break;
		}
		if (!this.gamePaused && (this.gameState == GameStates.Ingame || this.gameState == GameStates.Cutscene || this.gameState == GameStates.FruitsSummary || this.gameState == GameStates.Map))
		{
			this._shakeOffset = Vector2.Zero;
			Vector2 draw_position2 = new Vector2(4f, 4f);
			if (this.gameMode == 2)
			{
				string txtbestScore = Game1.content.LoadString("Strings\\StringsFromCSFiles:MineCart.cs.12115");
				b.DrawString(Game1.dialogueFont, Game1.content.LoadString("Strings\\StringsFromCSFiles:FishingGame.cs.10444", this.score), this.TransformDraw(draw_position2), Color.White, 0f, Vector2.Zero, this.GetPixelScale() / 4f, SpriteEffects.None, 0.1f);
				b.DrawString(Game1.dialogueFont, Game1.content.LoadString("Strings\\StringsFromCSFiles:FishingGame.cs.10444", this.score), this.TransformDraw(draw_position2 + new Vector2(1f, 1f)), Color.Black, 0f, Vector2.Zero, this.GetPixelScale() / 4f, SpriteEffects.None, 0.11f);
				draw_position2.Y += 10f;
				b.DrawString(Game1.dialogueFont, txtbestScore + this.currentHighScore, this.TransformDraw(draw_position2), Color.White, 0f, Vector2.Zero, this.GetPixelScale() / 4f, SpriteEffects.None, 0.1f);
				b.DrawString(Game1.dialogueFont, txtbestScore + this.currentHighScore, this.TransformDraw(draw_position2 + new Vector2(1f, 1f)), Color.Black, 0f, Vector2.Zero, this.GetPixelScale() / 4f, SpriteEffects.None, 0.11f);
			}
			else
			{
				draw_position2.X = 4f;
				for (int num2 = 0; num2 < this.livesLeft; num2++)
				{
					b.Draw(this.texture, this.TransformDraw(draw_position2), new Rectangle(160, 32, 16, 16), Color.White, 0f, new Vector2(0f, 0f), this.GetPixelScale(), SpriteEffects.None, 0.07f);
					b.Draw(this.texture, this.TransformDraw(draw_position2 + new Vector2(1f, 1f)), new Rectangle(160, 32, 16, 16), Color.Black, 0f, new Vector2(0f, 0f), this.GetPixelScale(), SpriteEffects.None, 0.071f);
					draw_position2.X += 18f;
					if (draw_position2.X > 90f && num2 < this.livesLeft - 1)
					{
						draw_position2.X = 4f;
						draw_position2.Y += 18f;
					}
				}
				draw_position2.X = 4f;
				draw_position2.X += 36f;
				for (int num3 = this.livesLeft; num3 < 3; num3++)
				{
					b.Draw(this.texture, this.TransformDraw(draw_position2), new Rectangle(160, 48, 16, 16), Color.White, 0f, new Vector2(0f, 0f), this.GetPixelScale(), SpriteEffects.None, 0.07f);
					b.Draw(this.texture, this.TransformDraw(draw_position2 + new Vector2(1f, 1f)), new Rectangle(160, 48, 16, 16), Color.Black, 0f, new Vector2(0f, 0f), this.GetPixelScale(), SpriteEffects.None, 0.071f);
					draw_position2.X -= 18f;
				}
			}
			draw_position2.X = 4f;
			draw_position2.Y += 18f;
			for (int num4 = 0; num4 < 3; num4++)
			{
				Vector2 shake_magnitude = Vector2.Zero;
				if (this.currentFruitCheckMagnitude > 0f && num4 == this.currentFruitCheckIndex - 1)
				{
					shake_magnitude.X = Utility.Lerp(0f - this.currentFruitCheckMagnitude, this.currentFruitCheckMagnitude, (float)Game1.random.NextDouble());
					shake_magnitude.Y = Utility.Lerp(0f - this.currentFruitCheckMagnitude, this.currentFruitCheckMagnitude, (float)Game1.random.NextDouble());
				}
				if (this._collectedFruit.Contains((CollectableFruits)num4))
				{
					b.Draw(this.texture, this.TransformDraw(draw_position2 + shake_magnitude), new Rectangle(160 + num4 * 16, 0, 16, 16), Color.White, 0f, new Vector2(0f, 0f), this.GetPixelScale(), SpriteEffects.None, 0.07f);
					b.Draw(this.texture, this.TransformDraw(draw_position2 + new Vector2(1f, 1f) + shake_magnitude), new Rectangle(160 + num4 * 16, 0, 16, 16), Color.Black, 0f, new Vector2(0f, 0f), this.GetPixelScale(), SpriteEffects.None, 0.075f);
				}
				else
				{
					b.Draw(this.texture, this.TransformDraw(draw_position2 + shake_magnitude), new Rectangle(160 + num4 * 16, 16, 16, 16), Color.White, 0f, new Vector2(0f, 0f), this.GetPixelScale(), SpriteEffects.None, 0.07f);
					b.Draw(this.texture, this.TransformDraw(draw_position2 + shake_magnitude + new Vector2(1f, 1f)), new Rectangle(160 + num4 * 16, 16, 16, 16), Color.Black, 0f, new Vector2(0f, 0f), this.GetPixelScale(), SpriteEffects.None, 0.075f);
				}
				draw_position2.X += 18f;
			}
			if (this.gameMode == 3)
			{
				draw_position2.X = 4f;
				draw_position2.Y += 18f;
				b.Draw(this.texture, this.TransformDraw(draw_position2), new Rectangle(0, 272, 9, 11), Color.White, 0f, new Vector2(0f, 0f), this.GetPixelScale(), SpriteEffects.None, 0.07f);
				b.Draw(this.texture, this.TransformDraw(draw_position2 + new Vector2(1f, 1f)), new Rectangle(0, 272, 9, 11), Color.Black, 0f, new Vector2(0f, 0f), this.GetPixelScale(), SpriteEffects.None, 0.08f);
				draw_position2.X += 12f;
				b.DrawString(Game1.dialogueFont, this.coinCount.ToString("00"), this.TransformDraw(draw_position2), Color.White, 0f, Vector2.Zero, this.GetPixelScale() / 4f, SpriteEffects.None, 0.01f);
				b.DrawString(Game1.dialogueFont, this.coinCount.ToString("00"), this.TransformDraw(draw_position2 + new Vector2(1f, 1f)) + new Vector2(-3f, -3f), Color.Black, 0f, Vector2.Zero, this.GetPixelScale() / 4f, SpriteEffects.None, 0.02f);
				b.DrawString(Game1.dialogueFont, this.coinCount.ToString("00"), this.TransformDraw(draw_position2 + new Vector2(1f, 1f)) + new Vector2(-2f, -2f), Color.Black, 0f, Vector2.Zero, this.GetPixelScale() / 4f, SpriteEffects.None, 0.02f);
				b.DrawString(Game1.dialogueFont, this.coinCount.ToString("00"), this.TransformDraw(draw_position2 + new Vector2(1f, 1f)) + new Vector2(-1f, -1f), Color.Black, 0f, Vector2.Zero, this.GetPixelScale() / 4f, SpriteEffects.None, 0.02f);
				b.DrawString(Game1.dialogueFont, this.coinCount.ToString("00"), this.TransformDraw(draw_position2 + new Vector2(1f, 1f)) + new Vector2(-3.5f, -3.5f), Color.Black, 0f, Vector2.Zero, this.GetPixelScale() / 4f, SpriteEffects.None, 0.02f);
				b.DrawString(Game1.dialogueFont, this.coinCount.ToString("00"), this.TransformDraw(draw_position2 + new Vector2(1f, 1f)) + new Vector2(-1.5f, -1.5f), Color.Black, 0f, Vector2.Zero, this.GetPixelScale() / 4f, SpriteEffects.None, 0.02f);
				b.DrawString(Game1.dialogueFont, this.coinCount.ToString("00"), this.TransformDraw(draw_position2 + new Vector2(1f, 1f)) + new Vector2(-2.5f, -2.5f), Color.Black, 0f, Vector2.Zero, this.GetPixelScale() / 4f, SpriteEffects.None, 0.02f);
			}
			if (Game1.IsMultiplayer)
			{
				string time_of_day_string = Game1.getTimeOfDayString(Game1.timeOfDay);
				draw_position2 = new Vector2((float)this.screenWidth - Game1.dialogueFont.MeasureString(time_of_day_string).X / 4f - 4f, 4f);
				Color timeColor = Color.White;
				b.DrawString(Game1.dialogueFont, Game1.getTimeOfDayString(Game1.timeOfDay), this.TransformDraw(draw_position2), timeColor, 0f, Vector2.Zero, this.GetPixelScale() / 4f, SpriteEffects.None, 0.01f);
				b.DrawString(Game1.dialogueFont, Game1.getTimeOfDayString(Game1.timeOfDay), this.TransformDraw(draw_position2 + new Vector2(1f, 1f)) + new Vector2(-3f, -3f), Color.Black, 0f, Vector2.Zero, this.GetPixelScale() / 4f, SpriteEffects.None, 0.02f);
				b.DrawString(Game1.dialogueFont, Game1.getTimeOfDayString(Game1.timeOfDay), this.TransformDraw(draw_position2 + new Vector2(1f, 1f)) + new Vector2(-2f, -2f), Color.Black, 0f, Vector2.Zero, this.GetPixelScale() / 4f, SpriteEffects.None, 0.02f);
				b.DrawString(Game1.dialogueFont, Game1.getTimeOfDayString(Game1.timeOfDay), this.TransformDraw(draw_position2 + new Vector2(1f, 1f)) + new Vector2(-1f, -1f), Color.Black, 0f, Vector2.Zero, this.GetPixelScale() / 4f, SpriteEffects.None, 0.02f);
				b.DrawString(Game1.dialogueFont, Game1.getTimeOfDayString(Game1.timeOfDay), this.TransformDraw(draw_position2 + new Vector2(1f, 1f)) + new Vector2(-3.5f, -3.5f), Color.Black, 0f, Vector2.Zero, this.GetPixelScale() / 4f, SpriteEffects.None, 0.02f);
				b.DrawString(Game1.dialogueFont, Game1.getTimeOfDayString(Game1.timeOfDay), this.TransformDraw(draw_position2 + new Vector2(1f, 1f)) + new Vector2(-1.5f, -1.5f), Color.Black, 0f, Vector2.Zero, this.GetPixelScale() / 4f, SpriteEffects.None, 0.02f);
				b.DrawString(Game1.dialogueFont, Game1.getTimeOfDayString(Game1.timeOfDay), this.TransformDraw(draw_position2 + new Vector2(1f, 1f)) + new Vector2(-2.5f, -2.5f), Color.Black, 0f, Vector2.Zero, this.GetPixelScale() / 4f, SpriteEffects.None, 0.02f);
			}
			if (this.gameState == GameStates.Ingame)
			{
				float left_edge = (float)(this.screenWidth - 192) / 2f;
				float right_edge = left_edge + 192f;
				draw_position2 = new Vector2(left_edge, 4f);
				for (int num5 = 0; num5 < 12; num5++)
				{
					Rectangle source_rect = new Rectangle(192, 48, 16, 16);
					if (num5 == 0)
					{
						source_rect = new Rectangle(176, 48, 16, 16);
					}
					else if (num5 >= 11)
					{
						source_rect = new Rectangle(207, 48, 16, 16);
					}
					b.Draw(this.texture, this.TransformDraw(draw_position2), source_rect, Color.White, 0f, Vector2.Zero, this.GetPixelScale(), SpriteEffects.None, 0.15f);
					b.Draw(this.texture, this.TransformDraw(draw_position2 + new Vector2(1f, 1f)), source_rect, Color.Black, 0f, Vector2.Zero, this.GetPixelScale(), SpriteEffects.None, 0.17f);
					draw_position2.X += 16f;
				}
				b.Draw(this.texture, this.TransformDraw(draw_position2), new Rectangle(176, 64, 16, 16), Color.White, 0f, Vector2.Zero, this.GetPixelScale(), SpriteEffects.None, 0.15f);
				draw_position2.X += 8f;
				string level_text = (this.levelsBeat + 1).ToString() ?? "";
				draw_position2.Y += 3f;
				b.DrawString(Game1.dialogueFont, level_text, this.TransformDraw(draw_position2 - new Vector2(Game1.dialogueFont.MeasureString(level_text).X / 2f / 4f, 0f)), Color.Black, 0f, Vector2.Zero, this.GetPixelScale() / 4f, SpriteEffects.None, 0.1f);
				draw_position2.X += 1f;
				draw_position2.Y += 1f;
				draw_position2 = new Vector2(left_edge, 4f);
				if (this.player != null && this.player.visible)
				{
					draw_position2.X = Utility.Lerp(left_edge, right_edge, Math.Min(this.player.position.X / (float)(this.distanceToTravel * this.tileSize), 1f));
				}
				b.Draw(this.texture, this.TransformDraw(draw_position2), new Rectangle(240, 48, 16, 16), Color.White, 0f, new Vector2(8f, 0f), this.GetPixelScale(), SpriteEffects.None, 0.12f);
				b.Draw(this.texture, this.TransformDraw(draw_position2 + new Vector2(1f, 1f)), new Rectangle(240, 48, 16, 16), Color.Black, 0f, new Vector2(8f, 0f), this.GetPixelScale(), SpriteEffects.None, 0.13f);
				if (this.checkpointPosition > (float)this.tileSize * 0.5f)
				{
					draw_position2.X = Utility.Lerp(left_edge, right_edge, this.checkpointPosition / (float)(this.distanceToTravel * this.tileSize));
					b.Draw(this.texture, this.TransformDraw(draw_position2), new Rectangle(224, 48, 16, 16), Color.White, 0f, new Vector2(8f, 0f), this.GetPixelScale(), SpriteEffects.None, 0.125f);
					b.Draw(this.texture, this.TransformDraw(draw_position2 + new Vector2(1f, 1f)), new Rectangle(224, 48, 16, 16), Color.Black, 0f, new Vector2(8f, 0f), this.GetPixelScale(), SpriteEffects.None, 0.135f);
				}
			}
		}
		if (this.gameMode == 2 && Game1.IsMultiplayer && this.gameState != GameStates.Title)
		{
			Game1.player.team.junimoKartStatus.Draw(b, this.TransformDraw(new Vector2(4f, this.screenHeight - 4)), this.GetPixelScale(), 0.01f, PlayerStatusList.HorizontalAlignment.Left, PlayerStatusList.VerticalAlignment.Bottom);
		}
		if (this.screenDarkness > 0f)
		{
			b.Draw(Game1.staminaRect, this.TransformDraw(new Rectangle(0, 0, this.screenWidth, this.screenHeight + this.tileSize)), null, Color.Black * this.screenDarkness, 0f, Vector2.Zero, SpriteEffects.None, 0.145f);
		}
		if (this.gamePaused)
		{
			b.Draw(Game1.staminaRect, this.TransformDraw(new Rectangle(0, 0, this.screenWidth, this.screenHeight + this.tileSize)), null, Color.Black * 0.75f, 0f, Vector2.Zero, SpriteEffects.None, 0.145f);
			string current_text = Game1.content.LoadString("Strings\\StringsFromCSFiles:DayTimeMoneyBox.cs.10378");
			b.DrawString(position: this.TransformDraw(new Vector2
			{
				X = this.screenWidth / 2,
				Y = this.screenHeight / 4
			} - new Vector2(Game1.dialogueFont.MeasureString(current_text).X / 2f / 4f, 0f)), spriteFont: Game1.dialogueFont, text: current_text, color: Color.White, rotation: 0f, origin: Vector2.Zero, scale: this.GetPixelScale() / 4f, effects: SpriteEffects.None, layerDepth: 0.1f);
		}
		if (!Game1.options.hardwareCursor && !Game1.options.gamepadControls)
		{
			b.Draw(Game1.mouseCursors, new Vector2(Game1.getMouseX(), Game1.getMouseY()), Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, Game1.options.gamepadControls ? 44 : 0, 16, 16), Color.White, 0f, Vector2.Zero, 4f + Game1.dialogueButtonScale / 150f, SpriteEffects.None, 0.0001f);
		}
		b.End();
		Game1.isUsingBackToFrontSorting = false;
		b.GraphicsDevice.ScissorRectangle = cached_scissor_rect;
	}

	public float GetPixelScale()
	{
		return this.pixelScale;
	}

	public Rectangle TransformDraw(Rectangle dest)
	{
		dest.X = (int)Math.Round(((float)dest.X + this._shakeOffset.X) * this.pixelScale) + (int)this.upperLeft.X;
		dest.Y = (int)Math.Round(((float)dest.Y + this._shakeOffset.Y) * this.pixelScale) + (int)this.upperLeft.Y;
		dest.Width = (int)((float)dest.Width * this.pixelScale);
		dest.Height = (int)((float)dest.Height * this.pixelScale);
		return dest;
	}

	public static int Mod(int x, int m)
	{
		return (x % m + m) % m;
	}

	public Vector2 TransformDraw(Vector2 dest)
	{
		dest.X = (int)Math.Round((dest.X + this._shakeOffset.X) * this.pixelScale) + (int)this.upperLeft.X;
		dest.Y = (int)Math.Round((dest.Y + this._shakeOffset.Y) * this.pixelScale) + (int)this.upperLeft.Y;
		return dest;
	}

	public void changeScreenSize()
	{
		this.screenWidth = 400;
		this.screenHeight = 220;
		float pixel_zoom_adjustment = 1f / Game1.options.zoomLevel;
		int viewport_width = Game1.game1.localMultiplayerWindow.Width;
		int viewport_height = Game1.game1.localMultiplayerWindow.Height;
		this.pixelScale = Math.Min(5, (int)Math.Floor(Math.Min((float)(viewport_width / this.screenWidth) * pixel_zoom_adjustment, (float)(viewport_height / this.screenHeight) * pixel_zoom_adjustment)));
		this.upperLeft = new Vector2((float)(viewport_width / 2) * pixel_zoom_adjustment, (float)(viewport_height / 2) * pixel_zoom_adjustment);
		this.upperLeft.X -= (float)(this.screenWidth / 2) * this.pixelScale;
		this.upperLeft.Y -= (float)(this.screenHeight / 2) * this.pixelScale;
		this.tileSize = 16;
		this.ytileOffset = this.screenHeight / 2 / this.tileSize;
	}

	public void unload()
	{
		Game1.stopMusicTrack(MusicContext.MiniGame);
		Game1.player.team.junimoKartStatus.WithdrawState();
		Game1.player.faceDirection(0);
		if (this.minecartLoop != null && this.minecartLoop.IsPlaying)
		{
			this.minecartLoop.Stop(AudioStopOptions.Immediate);
		}
	}

	public bool forceQuit()
	{
		if (this.gameState != GameStates.Cutscene && this.gameState != GameStates.Title && this.gameMode == 2)
		{
			this.submitHighScore();
		}
		this.unload();
		return true;
	}

	public void leftClickHeld(int x, int y)
	{
	}

	public void receiveEventPoke(int data)
	{
		throw new NotImplementedException();
	}

	public string minigameId()
	{
		return "MineCart";
	}

	public bool doMainGameUpdates()
	{
		return false;
	}
}
