using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Netcode;
using StardewValley.BellsAndWhistles;
using StardewValley.Extensions;
using StardewValley.GameData;

namespace StardewValley.Minigames;

[XmlInclude(typeof(JOTPKProgress))]
[InstanceStatics]
public class AbigailGame : IMinigame
{
	public delegate void behaviorAfterMotionPause();

	public enum GameKeys
	{
		MoveLeft,
		MoveRight,
		MoveUp,
		MoveDown,
		ShootLeft,
		ShootRight,
		ShootUp,
		ShootDown,
		UsePowerup,
		SelectOption,
		Exit,
		MAX
	}

	public class CowboyPowerup
	{
		public int which;

		public Point position;

		public int duration;

		public float yOffset;

		public CowboyPowerup(int which, Point position, int duration)
		{
			this.which = which;
			this.position = position;
			this.duration = duration;
		}

		public void draw(SpriteBatch b)
		{
			if (this.duration > 2000 || this.duration / 200 % 2 == 0)
			{
				b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + new Vector2(this.position.X, (float)this.position.Y + this.yOffset), new Rectangle(272 + this.which * 16, 1808, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, (float)this.position.Y / 10000f + 0.001f);
			}
		}
	}

	public class JOTPKProgress : INetObject<NetFields>
	{
		public NetInt bulletDamage = new NetInt();

		public NetInt fireSpeedLevel = new NetInt();

		public NetInt ammoLevel = new NetInt();

		public NetBool spreadPistol = new NetBool();

		public NetInt runSpeedLevel = new NetInt();

		public NetInt lives = new NetInt();

		public NetInt coins = new NetInt();

		public NetInt score = new NetInt();

		public NetBool died = new NetBool();

		public NetInt whichRound = new NetInt();

		public NetInt whichWave = new NetInt();

		public NetInt heldItem = new NetInt(-100);

		public NetInt world = new NetInt();

		public NetInt waveTimer = new NetInt();

		public NetList<Vector2, NetVector2> monsterChances = new NetList<Vector2, NetVector2>();

		public NetFields NetFields { get; } = new NetFields("JOTPKProgress");

		public JOTPKProgress()
		{
			this.NetFields.SetOwner(this).AddField(this.bulletDamage, "bulletDamage").AddField(this.runSpeedLevel, "runSpeedLevel")
				.AddField(this.fireSpeedLevel, "fireSpeedLevel")
				.AddField(this.ammoLevel, "ammoLevel")
				.AddField(this.lives, "lives")
				.AddField(this.coins, "coins")
				.AddField(this.score, "score")
				.AddField(this.died, "died")
				.AddField(this.spreadPistol, "spreadPistol")
				.AddField(this.whichRound, "whichRound")
				.AddField(this.whichWave, "whichWave")
				.AddField(this.heldItem, "heldItem")
				.AddField(this.world, "world")
				.AddField(this.waveTimer, "waveTimer")
				.AddField(this.monsterChances, "monsterChances");
		}
	}

	public class CowboyBullet
	{
		public Point position;

		public Point motion;

		public int damage;

		public CowboyBullet(Point position, Point motion, int damage)
		{
			this.position = position;
			this.motion = motion;
			this.damage = damage;
		}

		public CowboyBullet(Point position, int direction, int damage)
		{
			this.position = position;
			switch (direction)
			{
			case 0:
				this.motion = new Point(0, -8);
				break;
			case 1:
				this.motion = new Point(8, 0);
				break;
			case 2:
				this.motion = new Point(0, 8);
				break;
			case 3:
				this.motion = new Point(-8, 0);
				break;
			}
			this.damage = damage;
		}
	}

	public class CowboyMonster
	{
		public const int MonsterAnimationDelay = 500;

		public int health;

		public int type;

		public int speed;

		public float movementAnimationTimer;

		public Rectangle position;

		public int movementDirection;

		public bool movedLastTurn;

		public bool oppositeMotionGuy;

		public bool invisible;

		public bool special;

		public bool uninterested;

		public bool flyer;

		public Color tint = Color.White;

		public Color flashColor = Color.Red;

		public float flashColorTimer;

		public int ticksSinceLastMovement;

		public Vector2 acceleration;

		private Point targetPosition;

		public CowboyMonster(int which, int health, int speed, Point position)
		{
			this.health = health;
			this.type = which;
			this.speed = speed;
			this.position = new Rectangle(position.X, position.Y, AbigailGame.TileSize, AbigailGame.TileSize);
			this.uninterested = Game1.random.NextDouble() < 0.25;
		}

		public CowboyMonster(int which, Point position)
		{
			this.type = which;
			this.position = new Rectangle(position.X, position.Y, AbigailGame.TileSize, AbigailGame.TileSize);
			switch (this.type)
			{
			case 0:
				this.speed = 2;
				this.health = 1;
				this.uninterested = Game1.random.NextDouble() < 0.25;
				if (this.uninterested)
				{
					this.targetPosition = new Point(Game1.random.Next(2, 14) * AbigailGame.TileSize, Game1.random.Next(2, 14) * AbigailGame.TileSize);
				}
				break;
			case 2:
				this.speed = 1;
				this.health = 3;
				break;
			case 5:
				this.speed = 3;
				this.health = 2;
				break;
			case 1:
				this.speed = 2;
				this.health = 1;
				this.flyer = true;
				break;
			case 3:
				this.health = 6;
				this.speed = 1;
				this.uninterested = Game1.random.NextDouble() < 0.25;
				if (this.uninterested)
				{
					this.targetPosition = new Point(Game1.random.Next(2, 14) * AbigailGame.TileSize, Game1.random.Next(2, 14) * AbigailGame.TileSize);
				}
				break;
			case 4:
				this.health = 3;
				this.speed = 3;
				this.flyer = true;
				break;
			case 6:
			{
				this.speed = 3;
				this.health = 2;
				int tries = 0;
				do
				{
					this.targetPosition = new Point(Game1.random.Next(2, 14) * AbigailGame.TileSize, Game1.random.Next(2, 14) * AbigailGame.TileSize);
					tries++;
				}
				while (AbigailGame.isCollidingWithMap(this.targetPosition) && tries < 10);
				break;
			}
			}
			this.oppositeMotionGuy = Game1.random.NextBool();
		}

		public virtual void draw(SpriteBatch b)
		{
			if (this.type == 6 && this.special)
			{
				if (this.flashColorTimer > 0f)
				{
					b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + new Vector2(this.position.X, this.position.Y), new Rectangle(480, 1696, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, (float)this.position.Y / 10000f + 0.001f);
				}
				else
				{
					b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + new Vector2(this.position.X, this.position.Y), new Rectangle(576, 1712, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, (float)this.position.Y / 10000f + 0.001f);
				}
			}
			else if (!this.invisible)
			{
				if (this.flashColorTimer > 0f)
				{
					b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + new Vector2(this.position.X, this.position.Y), new Rectangle(352 + this.type * 16, 1696, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, (float)this.position.Y / 10000f + 0.001f);
				}
				else
				{
					b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + new Vector2(this.position.X, this.position.Y), new Rectangle(352 + (this.type * 2 + ((this.movementAnimationTimer < 250f) ? 1 : 0)) * 16, 1712, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, (float)this.position.Y / 10000f + 0.001f);
				}
				if (AbigailGame.monsterConfusionTimer > 0)
				{
					b.DrawString(Game1.smallFont, "?", AbigailGame.topLeftScreenCoordinate + new Vector2((float)(this.position.X + AbigailGame.TileSize / 2) - Game1.smallFont.MeasureString("?").X / 2f, this.position.Y - AbigailGame.TileSize / 2), new Color(88, 29, 43), 0f, Vector2.Zero, 1f, SpriteEffects.None, (float)this.position.Y / 10000f);
					b.DrawString(Game1.smallFont, "?", AbigailGame.topLeftScreenCoordinate + new Vector2((float)(this.position.X + AbigailGame.TileSize / 2) - Game1.smallFont.MeasureString("?").X / 2f + 1f, this.position.Y - AbigailGame.TileSize / 2), new Color(88, 29, 43), 0f, Vector2.Zero, 1f, SpriteEffects.None, (float)this.position.Y / 10000f);
					b.DrawString(Game1.smallFont, "?", AbigailGame.topLeftScreenCoordinate + new Vector2((float)(this.position.X + AbigailGame.TileSize / 2) - Game1.smallFont.MeasureString("?").X / 2f - 1f, this.position.Y - AbigailGame.TileSize / 2), new Color(88, 29, 43), 0f, Vector2.Zero, 1f, SpriteEffects.None, (float)this.position.Y / 10000f);
				}
			}
		}

		public virtual bool takeDamage(int damage)
		{
			this.health -= damage;
			this.health = Math.Max(0, this.health);
			if (this.health <= 0)
			{
				return true;
			}
			Game1.playSound("cowboy_monsterhit");
			this.flashColor = Color.Red;
			this.flashColorTimer = 100f;
			return false;
		}

		public virtual int getLootDrop()
		{
			if (this.type == 6 && this.special)
			{
				return -1;
			}
			if (Game1.random.NextDouble() < 0.05)
			{
				if (this.type != 0 && Game1.random.NextDouble() < 0.1)
				{
					return 1;
				}
				if (Game1.random.NextDouble() < 0.01)
				{
					return 1;
				}
				return 0;
			}
			if (Game1.random.NextDouble() < 0.05)
			{
				if (Game1.random.NextDouble() < 0.15)
				{
					return Game1.random.Next(6, 8);
				}
				if (Game1.random.NextDouble() < 0.07)
				{
					return 10;
				}
				int loot = Game1.random.Next(2, 10);
				if (loot == 5 && Game1.random.NextDouble() < 0.4)
				{
					loot = Game1.random.Next(2, 10);
				}
				return loot;
			}
			return -1;
		}

		public virtual bool move(Vector2 playerPosition, GameTime time)
		{
			this.movementAnimationTimer -= time.ElapsedGameTime.Milliseconds;
			if (this.movementAnimationTimer <= 0f)
			{
				this.movementAnimationTimer = Math.Max(100, 500 - this.speed * 50);
			}
			if (this.flashColorTimer > 0f)
			{
				this.flashColorTimer -= time.ElapsedGameTime.Milliseconds;
				return false;
			}
			if (AbigailGame.monsterConfusionTimer > 0)
			{
				return false;
			}
			if (AbigailGame.shopping)
			{
				AbigailGame.shoppingTimer -= time.ElapsedGameTime.Milliseconds;
				if (AbigailGame.shoppingTimer <= 0)
				{
					AbigailGame.shoppingTimer = 100;
				}
			}
			this.ticksSinceLastMovement++;
			switch (this.type)
			{
			case 0:
			case 2:
			case 3:
			case 5:
			case 6:
			{
				if (this.type == 6)
				{
					if (this.special || this.invisible)
					{
						break;
					}
					if (this.ticksSinceLastMovement > 20)
					{
						int tries2 = 0;
						do
						{
							this.targetPosition = new Point(Game1.random.Next(2, 14) * AbigailGame.TileSize, Game1.random.Next(2, 14) * AbigailGame.TileSize);
							tries2++;
						}
						while (AbigailGame.isCollidingWithMap(this.targetPosition) && tries2 < 5);
					}
				}
				else if (this.ticksSinceLastMovement > 20)
				{
					int tries3 = 0;
					do
					{
						this.oppositeMotionGuy = !this.oppositeMotionGuy;
						this.targetPosition = new Point(Game1.random.Next(this.position.X - AbigailGame.TileSize * 2, this.position.X + AbigailGame.TileSize * 2), Game1.random.Next(this.position.Y - AbigailGame.TileSize * 2, this.position.Y + AbigailGame.TileSize * 2));
						tries3++;
					}
					while (AbigailGame.isCollidingWithMap(this.targetPosition) && tries3 < 5);
				}
				_ = this.targetPosition;
				Vector2 target = ((!this.targetPosition.Equals(Point.Zero)) ? new Vector2(this.targetPosition.X, this.targetPosition.Y) : playerPosition);
				if (AbigailGame.playingWithAbigail && target.Equals(playerPosition))
				{
					double distanceToPlayer1 = Math.Sqrt(Math.Pow((float)this.position.X - target.X, 2.0) - Math.Pow((float)this.position.Y - target.Y, 2.0));
					if (Math.Sqrt(Math.Pow((float)this.position.X - AbigailGame.player2Position.X, 2.0) - Math.Pow((float)this.position.Y - AbigailGame.player2Position.Y, 2.0)) < distanceToPlayer1)
					{
						target = AbigailGame.player2Position;
					}
				}
				if (AbigailGame.gopherRunning)
				{
					target = new Vector2(AbigailGame.gopherBox.X, AbigailGame.gopherBox.Y);
				}
				if (Game1.random.NextDouble() < 0.001)
				{
					this.oppositeMotionGuy = !this.oppositeMotionGuy;
				}
				if ((this.type == 6 && !this.oppositeMotionGuy) || Math.Abs(target.X - (float)this.position.X) > Math.Abs(target.Y - (float)this.position.Y))
				{
					if (target.X + (float)this.speed < (float)this.position.X && (this.movedLastTurn || this.movementDirection != 3))
					{
						this.movementDirection = 3;
					}
					else if (target.X > (float)(this.position.X + this.speed) && (this.movedLastTurn || this.movementDirection != 1))
					{
						this.movementDirection = 1;
					}
					else if (target.Y > (float)(this.position.Y + this.speed) && (this.movedLastTurn || this.movementDirection != 2))
					{
						this.movementDirection = 2;
					}
					else if (target.Y + (float)this.speed < (float)this.position.Y && (this.movedLastTurn || this.movementDirection != 0))
					{
						this.movementDirection = 0;
					}
				}
				else if (target.Y > (float)(this.position.Y + this.speed) && (this.movedLastTurn || this.movementDirection != 2))
				{
					this.movementDirection = 2;
				}
				else if (target.Y + (float)this.speed < (float)this.position.Y && (this.movedLastTurn || this.movementDirection != 0))
				{
					this.movementDirection = 0;
				}
				else if (target.X + (float)this.speed < (float)this.position.X && (this.movedLastTurn || this.movementDirection != 3))
				{
					this.movementDirection = 3;
				}
				else if (target.X > (float)(this.position.X + this.speed) && (this.movedLastTurn || this.movementDirection != 1))
				{
					this.movementDirection = 1;
				}
				this.movedLastTurn = false;
				Rectangle attemptedPosition = this.position;
				switch (this.movementDirection)
				{
				case 0:
					attemptedPosition.Y -= this.speed;
					break;
				case 1:
					attemptedPosition.X += this.speed;
					break;
				case 2:
					attemptedPosition.Y += this.speed;
					break;
				case 3:
					attemptedPosition.X -= this.speed;
					break;
				}
				if (AbigailGame.zombieModeTimer > 0)
				{
					attemptedPosition.X = this.position.X - (attemptedPosition.X - this.position.X);
					attemptedPosition.Y = this.position.Y - (attemptedPosition.Y - this.position.Y);
				}
				if (this.type == 2)
				{
					for (int i = AbigailGame.monsters.Count - 1; i >= 0; i--)
					{
						if (AbigailGame.monsters[i].type == 6 && AbigailGame.monsters[i].special && AbigailGame.monsters[i].position.Intersects(attemptedPosition))
						{
							AbigailGame.addGuts(AbigailGame.monsters[i].position.Location, AbigailGame.monsters[i].type);
							Game1.playSound("Cowboy_monsterDie");
							AbigailGame.monsters.RemoveAt(i);
						}
					}
				}
				if (AbigailGame.isCollidingWithMapForMonsters(attemptedPosition) || AbigailGame.isCollidingWithMonster(attemptedPosition, this) || !(AbigailGame.deathTimer <= 0f))
				{
					break;
				}
				this.ticksSinceLastMovement = 0;
				this.position = attemptedPosition;
				this.movedLastTurn = true;
				if (!this.position.Contains((int)target.X + AbigailGame.TileSize / 2, (int)target.Y + AbigailGame.TileSize / 2))
				{
					break;
				}
				this.targetPosition = Point.Zero;
				switch (this.type)
				{
				case 0:
				case 3:
					if (this.uninterested)
					{
						this.targetPosition = new Point(Game1.random.Next(2, 14) * AbigailGame.TileSize, Game1.random.Next(2, 14) * AbigailGame.TileSize);
						if (Game1.random.NextBool())
						{
							this.uninterested = false;
							this.targetPosition = Point.Zero;
						}
					}
					break;
				case 6:
					if (!this.invisible)
					{
						AbigailGame.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(352, 1728, 16, 16), 60f, 3, 0, new Vector2(this.position.X, this.position.Y) + AbigailGame.topLeftScreenCoordinate, flicker: false, flipped: false, (float)this.position.Y / 10000f, 0f, Color.White, 3f, 0f, 0f, 0f, local: true)
						{
							endFunction = spikeyEndBehavior
						});
						this.invisible = true;
					}
					break;
				}
				break;
			}
			case 1:
			case 4:
			{
				if (this.ticksSinceLastMovement > 20)
				{
					int tries = 0;
					do
					{
						this.oppositeMotionGuy = !this.oppositeMotionGuy;
						this.targetPosition = new Point(Game1.random.Next(this.position.X - AbigailGame.TileSize * 2, this.position.X + AbigailGame.TileSize * 2), Game1.random.Next(this.position.Y - AbigailGame.TileSize * 2, this.position.Y + AbigailGame.TileSize * 2));
						tries++;
					}
					while (AbigailGame.isCollidingWithMap(this.targetPosition) && tries < 5);
				}
				_ = this.targetPosition;
				Vector2 target = ((!this.targetPosition.Equals(Point.Zero)) ? new Vector2(this.targetPosition.X, this.targetPosition.Y) : playerPosition);
				Vector2 targetToFly = Utility.getVelocityTowardPoint(this.position.Location, target + new Vector2(AbigailGame.TileSize / 2, AbigailGame.TileSize / 2), this.speed);
				float accelerationMultiplyer = ((targetToFly.X != 0f && targetToFly.Y != 0f) ? 1.5f : 1f);
				if (targetToFly.X > this.acceleration.X)
				{
					this.acceleration.X += 0.1f * accelerationMultiplyer;
				}
				if (targetToFly.X < this.acceleration.X)
				{
					this.acceleration.X -= 0.1f * accelerationMultiplyer;
				}
				if (targetToFly.Y > this.acceleration.Y)
				{
					this.acceleration.Y += 0.1f * accelerationMultiplyer;
				}
				if (targetToFly.Y < this.acceleration.Y)
				{
					this.acceleration.Y -= 0.1f * accelerationMultiplyer;
				}
				if (!AbigailGame.isCollidingWithMonster(new Rectangle(this.position.X + (int)Math.Ceiling(this.acceleration.X), this.position.Y + (int)Math.Ceiling(this.acceleration.Y), AbigailGame.TileSize, AbigailGame.TileSize), this) && AbigailGame.deathTimer <= 0f)
				{
					this.ticksSinceLastMovement = 0;
					this.position.X += (int)Math.Ceiling(this.acceleration.X);
					this.position.Y += (int)Math.Ceiling(this.acceleration.Y);
					if (this.position.Contains((int)target.X + AbigailGame.TileSize / 2, (int)target.Y + AbigailGame.TileSize / 2))
					{
						this.targetPosition = Point.Zero;
					}
				}
				break;
			}
			}
			return false;
		}

		public void spikeyEndBehavior(int extraInfo)
		{
			this.invisible = false;
			this.health += 5;
			this.special = true;
		}
	}

	public class Dracula : CowboyMonster
	{
		public const int gloatingPhase = -1;

		public const int walkRandomlyAndShootPhase = 0;

		public const int spreadShotPhase = 1;

		public const int summonDemonPhase = 2;

		public const int summonMummyPhase = 3;

		public int phase = -1;

		public int phaseInternalTimer;

		public int phaseInternalCounter;

		public int shootTimer;

		public int fullHealth;

		public Point homePosition;

		public Dracula()
			: base(-2, new Point(8 * AbigailGame.TileSize, 8 * AbigailGame.TileSize))
		{
			this.homePosition = base.position.Location;
			base.position.Y += AbigailGame.TileSize * 4;
			base.health = 350;
			this.fullHealth = base.health;
			this.phase = -1;
			this.phaseInternalTimer = 4000;
			base.speed = 2;
		}

		public override void draw(SpriteBatch b)
		{
			if (this.phase != -1)
			{
				b.Draw(Game1.staminaRect, new Rectangle((int)AbigailGame.topLeftScreenCoordinate.X, (int)AbigailGame.topLeftScreenCoordinate.Y + 16 * AbigailGame.TileSize + 3, (int)((float)(16 * AbigailGame.TileSize) * ((float)base.health / (float)this.fullHealth)), AbigailGame.TileSize / 3), new Color(188, 51, 74));
			}
			if (base.flashColorTimer > 0f)
			{
				b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + new Vector2(base.position.X, base.position.Y), new Rectangle(464, 1696, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, (float)base.position.Y / 10000f);
				return;
			}
			int num = this.phase;
			if (num == -1 || (uint)(num - 1) <= 2u)
			{
				b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + new Vector2(base.position.X, base.position.Y), new Rectangle(592 + this.phaseInternalTimer / 100 % 3 * 16, 1760, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, (float)base.position.Y / 10000f);
				if (this.phase == -1)
				{
					b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + new Vector2(base.position.X, (float)(base.position.Y + AbigailGame.TileSize) + (float)Math.Sin((float)this.phaseInternalTimer / 1000f) * 3f), new Rectangle(528, 1776, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, (float)base.position.Y / 10000f);
					b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + new Vector2(base.position.X - AbigailGame.TileSize / 2, base.position.Y - AbigailGame.TileSize * 2), new Rectangle(608, 1728, 32, 32), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, (float)base.position.Y / 10000f);
				}
			}
			else
			{
				b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + new Vector2(base.position.X, base.position.Y), new Rectangle(592 + this.phaseInternalTimer / 100 % 2 * 16, 1712, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, (float)base.position.Y / 10000f);
			}
		}

		public override int getLootDrop()
		{
			return -1;
		}

		public override bool takeDamage(int damage)
		{
			if (this.phase == -1)
			{
				return false;
			}
			base.health -= damage;
			if (base.health < 0)
			{
				return true;
			}
			base.flashColorTimer = 100f;
			Game1.playSound("cowboy_monsterhit");
			return false;
		}

		public override bool move(Vector2 playerPosition, GameTime time)
		{
			if (base.flashColorTimer > 0f)
			{
				base.flashColorTimer -= time.ElapsedGameTime.Milliseconds;
			}
			this.phaseInternalTimer -= time.ElapsedGameTime.Milliseconds;
			switch (this.phase)
			{
			case -1:
				if (this.phaseInternalTimer <= 0)
				{
					this.phaseInternalCounter = 0;
					Game1.playSound("cowboy_boss", out AbigailGame.outlawSong);
					this.phase = 0;
				}
				break;
			case 0:
			{
				if (this.phaseInternalCounter == 0)
				{
					this.phaseInternalCounter++;
					this.phaseInternalTimer = Game1.random.Next(3000, 7000);
				}
				if (this.phaseInternalTimer < 0)
				{
					this.phaseInternalCounter = 0;
					this.phase = Game1.random.Next(1, 4);
					this.phaseInternalTimer = 9999;
				}
				Vector2 target = playerPosition;
				if (!(AbigailGame.deathTimer <= 0f))
				{
					break;
				}
				int movementDirection = -1;
				if (Math.Abs(target.X - (float)base.position.X) > Math.Abs(target.Y - (float)base.position.Y))
				{
					if (target.X + (float)base.speed < (float)base.position.X)
					{
						movementDirection = 3;
					}
					else if (target.X > (float)(base.position.X + base.speed))
					{
						movementDirection = 1;
					}
					else if (target.Y > (float)(base.position.Y + base.speed))
					{
						movementDirection = 2;
					}
					else if (target.Y + (float)base.speed < (float)base.position.Y)
					{
						movementDirection = 0;
					}
				}
				else if (target.Y > (float)(base.position.Y + base.speed))
				{
					movementDirection = 2;
				}
				else if (target.Y + (float)base.speed < (float)base.position.Y)
				{
					movementDirection = 0;
				}
				else if (target.X + (float)base.speed < (float)base.position.X)
				{
					movementDirection = 3;
				}
				else if (target.X > (float)(base.position.X + base.speed))
				{
					movementDirection = 1;
				}
				Rectangle attemptedPosition = base.position;
				switch (movementDirection)
				{
				case 0:
					attemptedPosition.Y -= base.speed;
					break;
				case 1:
					attemptedPosition.X += base.speed;
					break;
				case 2:
					attemptedPosition.Y += base.speed;
					break;
				case 3:
					attemptedPosition.X -= base.speed;
					break;
				}
				attemptedPosition.X = base.position.X - (attemptedPosition.X - base.position.X);
				attemptedPosition.Y = base.position.Y - (attemptedPosition.Y - base.position.Y);
				if (!AbigailGame.isCollidingWithMapForMonsters(attemptedPosition) && !AbigailGame.isCollidingWithMonster(attemptedPosition, this))
				{
					base.position = attemptedPosition;
				}
				this.shootTimer -= time.ElapsedGameTime.Milliseconds;
				if (this.shootTimer < 0)
				{
					Vector2 trajectory3 = Utility.getVelocityTowardPoint(new Point(base.position.X + AbigailGame.TileSize / 2, base.position.Y), playerPosition + new Vector2(AbigailGame.TileSize / 2, AbigailGame.TileSize / 2), 8f);
					if (AbigailGame.playerMovementDirections.Count > 0)
					{
						trajectory3 = Utility.getTranslatedVector2(trajectory3, AbigailGame.playerMovementDirections.Last(), 3f);
					}
					AbigailGame.enemyBullets.Add(new CowboyBullet(new Point(base.position.X + AbigailGame.TileSize / 2, base.position.Y + AbigailGame.TileSize / 2), new Point((int)trajectory3.X, (int)trajectory3.Y), 1));
					this.shootTimer = 250;
					Game1.playSound("Cowboy_gunshot");
				}
				break;
			}
			case 2:
			case 3:
				switch (this.phaseInternalCounter)
				{
				case 0:
				{
					Point oldPosition2 = base.position.Location;
					if (base.position.X > this.homePosition.X + 6)
					{
						base.position.X -= 6;
					}
					else if (base.position.X < this.homePosition.X - 6)
					{
						base.position.X += 6;
					}
					if (base.position.Y > this.homePosition.Y + 6)
					{
						base.position.Y -= 6;
					}
					else if (base.position.Y < this.homePosition.Y - 6)
					{
						base.position.Y += 6;
					}
					if (base.position.Location.Equals(oldPosition2))
					{
						this.phaseInternalCounter++;
						this.phaseInternalTimer = 1500;
					}
					break;
				}
				case 1:
					if (this.phaseInternalTimer < 0)
					{
						this.summonEnemies(new Point(base.position.X + AbigailGame.TileSize / 2, base.position.Y + AbigailGame.TileSize / 2), Game1.random.Next(0, 5));
						if (Game1.random.NextDouble() < 0.4)
						{
							this.phase = 0;
							this.phaseInternalCounter = 0;
						}
						else
						{
							this.phaseInternalTimer = 2000;
						}
					}
					break;
				}
				break;
			case 1:
				switch (this.phaseInternalCounter)
				{
				case 0:
				{
					Point oldPosition = base.position.Location;
					if (base.position.X > this.homePosition.X + 6)
					{
						base.position.X -= 6;
					}
					else if (base.position.X < this.homePosition.X - 6)
					{
						base.position.X += 6;
					}
					if (base.position.Y > this.homePosition.Y + 6)
					{
						base.position.Y -= 6;
					}
					else if (base.position.Y < this.homePosition.Y - 6)
					{
						base.position.Y += 6;
					}
					if (base.position.Location.Equals(oldPosition))
					{
						this.phaseInternalCounter++;
						this.phaseInternalTimer = 1500;
					}
					break;
				}
				case 1:
					if (this.phaseInternalTimer < 0)
					{
						this.phaseInternalCounter++;
						this.phaseInternalTimer = 2000;
						this.shootTimer = 200;
						this.fireSpread(new Point(base.position.X + AbigailGame.TileSize / 2, base.position.Y + AbigailGame.TileSize / 2), 0.0);
					}
					break;
				case 2:
					this.shootTimer -= time.ElapsedGameTime.Milliseconds;
					if (this.shootTimer < 0)
					{
						this.fireSpread(new Point(base.position.X + AbigailGame.TileSize / 2, base.position.Y + AbigailGame.TileSize / 2), 0.0);
						this.shootTimer = 200;
					}
					if (this.phaseInternalTimer < 0)
					{
						this.phaseInternalCounter++;
						this.phaseInternalTimer = 500;
					}
					break;
				case 3:
					if (this.phaseInternalTimer < 0)
					{
						this.phaseInternalTimer = 2000;
						this.shootTimer = 200;
						this.phaseInternalCounter++;
						Vector2 trajectory2 = Utility.getVelocityTowardPoint(new Point(base.position.X + AbigailGame.TileSize / 2, base.position.Y), playerPosition + new Vector2(AbigailGame.TileSize / 2, AbigailGame.TileSize / 2), 8f);
						AbigailGame.enemyBullets.Add(new CowboyBullet(new Point(base.position.X + AbigailGame.TileSize / 2, base.position.Y + AbigailGame.TileSize / 2), new Point((int)trajectory2.X, (int)trajectory2.Y), 1));
						Game1.playSound("Cowboy_gunshot");
					}
					break;
				case 4:
					this.shootTimer -= time.ElapsedGameTime.Milliseconds;
					if (this.shootTimer < 0)
					{
						Vector2 trajectory = Utility.getVelocityTowardPoint(new Point(base.position.X + AbigailGame.TileSize / 2, base.position.Y), playerPosition + new Vector2(AbigailGame.TileSize / 2, AbigailGame.TileSize / 2), 8f);
						trajectory.X += Game1.random.Next(-1, 2);
						trajectory.Y += Game1.random.Next(-1, 2);
						AbigailGame.enemyBullets.Add(new CowboyBullet(new Point(base.position.X + AbigailGame.TileSize / 2, base.position.Y + AbigailGame.TileSize / 2), new Point((int)trajectory.X, (int)trajectory.Y), 1));
						Game1.playSound("Cowboy_gunshot");
						this.shootTimer = 200;
					}
					if (this.phaseInternalTimer < 0)
					{
						if (Game1.random.NextDouble() < 0.4)
						{
							this.phase = 0;
							this.phaseInternalCounter = 0;
						}
						else
						{
							this.phaseInternalTimer = 500;
							this.phaseInternalCounter = 1;
						}
					}
					break;
				}
				break;
			}
			return false;
		}

		public void fireSpread(Point origin, double offsetAngle)
		{
			Vector2[] surroundingTileLocationsArray = Utility.getSurroundingTileLocationsArray(new Vector2(origin.X, origin.Y));
			for (int i = 0; i < surroundingTileLocationsArray.Length; i++)
			{
				Vector2 p = surroundingTileLocationsArray[i];
				Vector2 trajectory = Utility.getVelocityTowardPoint(origin, p, 6f);
				if (offsetAngle > 0.0)
				{
					offsetAngle /= 2.0;
					trajectory.X = (float)(Math.Cos(offsetAngle) * (double)(p.X - (float)origin.X) - Math.Sin(offsetAngle) * (double)(p.Y - (float)origin.Y) + (double)origin.X);
					trajectory.Y = (float)(Math.Sin(offsetAngle) * (double)(p.X - (float)origin.X) + Math.Cos(offsetAngle) * (double)(p.Y - (float)origin.Y) + (double)origin.Y);
					trajectory = Utility.getVelocityTowardPoint(origin, trajectory, 8f);
				}
				AbigailGame.enemyBullets.Add(new CowboyBullet(origin, new Point((int)trajectory.X, (int)trajectory.Y), 1));
			}
			Game1.playSound("Cowboy_gunshot");
		}

		public void summonEnemies(Point origin, int which)
		{
			if (!AbigailGame.isCollidingWithMonster(new Rectangle(origin.X - AbigailGame.TileSize - AbigailGame.TileSize / 2, origin.Y, AbigailGame.TileSize, AbigailGame.TileSize), null))
			{
				AbigailGame.monsters.Add(new CowboyMonster(which, new Point(origin.X - AbigailGame.TileSize - AbigailGame.TileSize / 2, origin.Y)));
			}
			if (!AbigailGame.isCollidingWithMonster(new Rectangle(origin.X + AbigailGame.TileSize + AbigailGame.TileSize / 2, origin.Y, AbigailGame.TileSize, AbigailGame.TileSize), null))
			{
				AbigailGame.monsters.Add(new CowboyMonster(which, new Point(origin.X + AbigailGame.TileSize + AbigailGame.TileSize / 2, origin.Y)));
			}
			if (!AbigailGame.isCollidingWithMonster(new Rectangle(origin.X, origin.Y + AbigailGame.TileSize + AbigailGame.TileSize / 2, AbigailGame.TileSize, AbigailGame.TileSize), null))
			{
				AbigailGame.monsters.Add(new CowboyMonster(which, new Point(origin.X, origin.Y + AbigailGame.TileSize + AbigailGame.TileSize / 2)));
			}
			if (!AbigailGame.isCollidingWithMonster(new Rectangle(origin.X, origin.Y - AbigailGame.TileSize - AbigailGame.TileSize * 3 / 4, AbigailGame.TileSize, AbigailGame.TileSize), null))
			{
				AbigailGame.monsters.Add(new CowboyMonster(which, new Point(origin.X, origin.Y - AbigailGame.TileSize - AbigailGame.TileSize * 3 / 4)));
			}
			AbigailGame.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(464, 1792, 16, 16), 80f, 5, 0, AbigailGame.topLeftScreenCoordinate + new Vector2(origin.X - AbigailGame.TileSize - AbigailGame.TileSize / 2, origin.Y), flicker: false, flipped: false, 1f, 0f, Color.White, 3f, 0f, 0f, 0f, local: true)
			{
				delayBeforeAnimationStart = Game1.random.Next(800)
			});
			AbigailGame.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(464, 1792, 16, 16), 80f, 5, 0, AbigailGame.topLeftScreenCoordinate + new Vector2(origin.X + AbigailGame.TileSize + AbigailGame.TileSize / 2, origin.Y), flicker: false, flipped: false, 1f, 0f, Color.White, 3f, 0f, 0f, 0f, local: true)
			{
				delayBeforeAnimationStart = Game1.random.Next(800)
			});
			AbigailGame.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(464, 1792, 16, 16), 80f, 5, 0, AbigailGame.topLeftScreenCoordinate + new Vector2(origin.X, origin.Y - AbigailGame.TileSize - AbigailGame.TileSize * 3 / 4), flicker: false, flipped: false, 1f, 0f, Color.White, 3f, 0f, 0f, 0f, local: true)
			{
				delayBeforeAnimationStart = Game1.random.Next(800)
			});
			AbigailGame.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(464, 1792, 16, 16), 80f, 5, 0, AbigailGame.topLeftScreenCoordinate + new Vector2(origin.X, origin.Y + AbigailGame.TileSize + AbigailGame.TileSize / 2), flicker: false, flipped: false, 1f, 0f, Color.White, 3f, 0f, 0f, 0f, local: true)
			{
				delayBeforeAnimationStart = Game1.random.Next(800)
			});
			Game1.playSound("Cowboy_monsterDie");
		}
	}

	public class Outlaw : CowboyMonster
	{
		public const int talkingPhase = -1;

		public const int hidingPhase = 0;

		public const int dartOutAndShootPhase = 1;

		public const int runAndGunPhase = 2;

		public const int runGunAndPantPhase = 3;

		public const int shootAtPlayerPhase = 4;

		public int phase;

		public int phaseCountdown;

		public int shootTimer;

		public int phaseInternalTimer;

		public int phaseInternalCounter;

		public bool dartLeft;

		public int fullHealth;

		public Point homePosition;

		public Outlaw(Point position, int health)
			: base(-1, position)
		{
			this.homePosition = position;
			base.health = health;
			this.fullHealth = health;
			this.phaseCountdown = 4000;
			this.phase = -1;
		}

		public override void draw(SpriteBatch b)
		{
			b.Draw(Game1.staminaRect, new Rectangle((int)AbigailGame.topLeftScreenCoordinate.X, (int)AbigailGame.topLeftScreenCoordinate.Y + 16 * AbigailGame.TileSize + 3, (int)((float)(16 * AbigailGame.TileSize) * ((float)base.health / (float)this.fullHealth)), AbigailGame.TileSize / 3), new Color(188, 51, 74));
			if (base.flashColorTimer > 0f)
			{
				b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + new Vector2(base.position.X, base.position.Y), new Rectangle(496, 1696, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, (float)base.position.Y / 10000f + 0.001f);
				return;
			}
			int num = this.phase;
			if ((uint)(num - -1) <= 1u)
			{
				b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + new Vector2(base.position.X, base.position.Y), new Rectangle(560 + ((this.phaseCountdown / 250 % 2 == 0) ? 16 : 0), 1776, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, (float)base.position.Y / 10000f + 0.001f);
				if (this.phase == -1 && this.phaseCountdown > 1000)
				{
					b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + new Vector2(base.position.X - AbigailGame.TileSize / 2, base.position.Y - AbigailGame.TileSize * 2), new Rectangle(576 + ((AbigailGame.whichWave > 5) ? 32 : 0), 1792, 32, 32), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, (float)base.position.Y / 10000f + 0.001f);
				}
			}
			else if (this.phase == 3 && this.phaseInternalCounter == 2)
			{
				b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + new Vector2(base.position.X, base.position.Y), new Rectangle(560 + ((this.phaseCountdown / 250 % 2 == 0) ? 16 : 0), 1776, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, (float)base.position.Y / 10000f + 0.001f);
			}
			else
			{
				b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + new Vector2(base.position.X, base.position.Y), new Rectangle(592 + ((this.phaseCountdown / 80 % 2 == 0) ? 16 : 0), 1776, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, (float)base.position.Y / 10000f + 0.001f);
			}
		}

		public override bool move(Vector2 playerPosition, GameTime time)
		{
			if (base.flashColorTimer > 0f)
			{
				base.flashColorTimer -= time.ElapsedGameTime.Milliseconds;
			}
			this.phaseCountdown -= time.ElapsedGameTime.Milliseconds;
			if (base.position.X > 17 * AbigailGame.TileSize || base.position.X < -AbigailGame.TileSize)
			{
				base.position.X = 16 * AbigailGame.TileSize / 2;
			}
			switch (this.phase)
			{
			case -1:
			case 0:
				if (this.phaseCountdown >= 0)
				{
					break;
				}
				this.phase = Game1.random.Next(1, 5);
				this.dartLeft = playerPosition.X < (float)base.position.X;
				if (playerPosition.X > (float)(7 * AbigailGame.TileSize) && playerPosition.X < (float)(9 * AbigailGame.TileSize))
				{
					if (Game1.random.NextDouble() < 0.66 || this.phase == 2)
					{
						this.phase = 4;
					}
				}
				else if (this.phase == 4)
				{
					this.phase = 3;
				}
				this.phaseInternalCounter = 0;
				this.phaseInternalTimer = 0;
				break;
			case 4:
			{
				int motion = (this.dartLeft ? (-3) : 3);
				if (this.phaseInternalCounter == 0 && (!(playerPosition.X > (float)(7 * AbigailGame.TileSize)) || !(playerPosition.X < (float)(9 * AbigailGame.TileSize))))
				{
					this.phaseInternalCounter = 1;
					this.phaseInternalTimer = Game1.random.Next(500, 1500);
					break;
				}
				if (Math.Abs(base.position.Location.X - this.homePosition.X + AbigailGame.TileSize / 2) < AbigailGame.TileSize * 7 + 12 && this.phaseInternalCounter == 0)
				{
					base.position.X += motion;
					break;
				}
				if (this.phaseInternalCounter == 2)
				{
					motion = (this.dartLeft ? (-4) : 4);
					base.position.X -= motion;
					if (Math.Abs(base.position.X - this.homePosition.X) < 4)
					{
						base.position.X = this.homePosition.X;
						this.phase = 0;
						this.phaseCountdown = Game1.random.Next(1000, 2000);
					}
					break;
				}
				if (this.phaseInternalCounter == 0)
				{
					this.phaseInternalCounter++;
					this.phaseInternalTimer = Game1.random.Next(1000, 2000);
				}
				this.phaseInternalTimer -= time.ElapsedGameTime.Milliseconds;
				this.shootTimer -= time.ElapsedGameTime.Milliseconds;
				if (this.shootTimer < 0)
				{
					Vector2 trajectory = Utility.getVelocityTowardPoint(new Point(base.position.X + AbigailGame.TileSize / 2, base.position.Y), playerPosition + new Vector2(AbigailGame.TileSize / 2, AbigailGame.TileSize / 2), 8f);
					AbigailGame.enemyBullets.Add(new CowboyBullet(new Point(base.position.X + AbigailGame.TileSize / 2, base.position.Y - AbigailGame.TileSize / 2), new Point((int)trajectory.X, (int)trajectory.Y), 1));
					this.shootTimer = 120;
					Game1.playSound("Cowboy_gunshot");
				}
				if (this.phaseInternalTimer <= 0)
				{
					this.phaseInternalCounter++;
				}
				break;
			}
			case 1:
			{
				int motion = (this.dartLeft ? (-3) : 3);
				if (Math.Abs(base.position.Location.X - this.homePosition.X + AbigailGame.TileSize / 2) < AbigailGame.TileSize * 2 + 12 && this.phaseInternalCounter == 0)
				{
					base.position.X += motion;
					if (base.position.X > 256)
					{
						this.phaseInternalCounter = 2;
					}
					break;
				}
				if (this.phaseInternalCounter == 2)
				{
					base.position.X -= motion;
					if (Math.Abs(base.position.X - this.homePosition.X) < 4)
					{
						base.position.X = this.homePosition.X;
						this.phase = 0;
						this.phaseCountdown = Game1.random.Next(1000, 2000);
					}
					break;
				}
				if (this.phaseInternalCounter == 0)
				{
					this.phaseInternalCounter++;
					this.phaseInternalTimer = Game1.random.Next(1000, 2000);
				}
				this.phaseInternalTimer -= time.ElapsedGameTime.Milliseconds;
				this.shootTimer -= time.ElapsedGameTime.Milliseconds;
				if (this.shootTimer < 0)
				{
					AbigailGame.enemyBullets.Add(new CowboyBullet(new Point(base.position.X + AbigailGame.TileSize / 2, base.position.Y - AbigailGame.TileSize / 2), new Point(Game1.random.Next(-2, 3), -8), 1));
					this.shootTimer = 150;
					Game1.playSound("Cowboy_gunshot");
				}
				if (this.phaseInternalTimer <= 0)
				{
					this.phaseInternalCounter++;
				}
				break;
			}
			case 2:
				switch (this.phaseInternalCounter)
				{
				case 2:
					if (base.position.X < this.homePosition.X)
					{
						base.position.X += 4;
					}
					else
					{
						base.position.X -= 4;
					}
					if (Math.Abs(base.position.X - this.homePosition.X) < 5)
					{
						base.position.X = this.homePosition.X;
						this.phase = 0;
						this.phaseCountdown = Game1.random.Next(1000, 2000);
					}
					return false;
				case 0:
					this.phaseInternalCounter++;
					this.phaseInternalTimer = Game1.random.Next(4000, 7000);
					break;
				}
				this.phaseInternalTimer -= time.ElapsedGameTime.Milliseconds;
				if ((float)base.position.X > playerPosition.X && (float)base.position.X - playerPosition.X > 3f)
				{
					base.position.X -= 2;
				}
				else if ((float)base.position.X < playerPosition.X && playerPosition.X - (float)base.position.X > 3f)
				{
					base.position.X += 2;
				}
				this.shootTimer -= time.ElapsedGameTime.Milliseconds;
				if (this.shootTimer < 0)
				{
					AbigailGame.enemyBullets.Add(new CowboyBullet(new Point(base.position.X + AbigailGame.TileSize / 2, base.position.Y - AbigailGame.TileSize / 2), new Point(Game1.random.Next(-1, 2), -8), 1));
					this.shootTimer = 250;
					if (this.fullHealth > 50)
					{
						this.shootTimer -= 50;
					}
					if (Game1.random.NextDouble() < 0.2)
					{
						this.shootTimer = 150;
					}
					Game1.playSound("Cowboy_gunshot");
				}
				if (this.phaseInternalTimer <= 0)
				{
					this.phaseInternalCounter++;
				}
				break;
			case 3:
				switch (this.phaseInternalCounter)
				{
				case 0:
					this.phaseInternalCounter++;
					this.phaseInternalTimer = Game1.random.Next(3000, 6500);
					break;
				case 2:
					this.phaseInternalTimer -= time.ElapsedGameTime.Milliseconds;
					if (this.phaseInternalTimer <= 0)
					{
						this.phaseInternalCounter++;
					}
					break;
				case 3:
					if (base.position.X < this.homePosition.X)
					{
						base.position.X += 4;
					}
					else
					{
						base.position.X -= 4;
					}
					if (Math.Abs(base.position.X - this.homePosition.X) < 5)
					{
						base.position.X = this.homePosition.X;
						this.phase = 0;
						this.phaseCountdown = Game1.random.Next(1000, 2000);
					}
					break;
				default:
				{
					int motion = (this.dartLeft ? (-3) : 3);
					base.position.X += motion;
					if (base.position.X < AbigailGame.TileSize || base.position.X > 15 * AbigailGame.TileSize)
					{
						this.dartLeft = !this.dartLeft;
					}
					this.shootTimer -= time.ElapsedGameTime.Milliseconds;
					if (this.shootTimer < 0)
					{
						AbigailGame.enemyBullets.Add(new CowboyBullet(new Point(base.position.X + AbigailGame.TileSize / 2, base.position.Y - AbigailGame.TileSize / 2), new Point(Game1.random.Next(-1, 2), -8), 1));
						this.shootTimer = 250;
						if (this.fullHealth > 50)
						{
							this.shootTimer -= 50;
						}
						if (Game1.random.NextDouble() < 0.2)
						{
							this.shootTimer = 150;
						}
						Game1.playSound("Cowboy_gunshot");
					}
					this.phaseInternalTimer -= time.ElapsedGameTime.Milliseconds;
					if (this.phaseInternalTimer <= 0)
					{
						if (this.phase == 2)
						{
							this.phaseInternalCounter = 3;
							break;
						}
						this.phaseInternalTimer = 3000;
						this.phaseInternalCounter++;
					}
					break;
				}
				}
				break;
			}
			if (base.position.X <= 16 * AbigailGame.TileSize)
			{
				_ = base.position.X;
				_ = 0;
			}
			return false;
		}

		public override int getLootDrop()
		{
			return 8;
		}

		public override bool takeDamage(int damage)
		{
			if (Math.Abs(base.position.X - this.homePosition.X) < 5)
			{
				return false;
			}
			base.health -= damage;
			if (base.health < 0)
			{
				return true;
			}
			base.flashColorTimer = 150f;
			Game1.playSound("cowboy_monsterhit");
			return false;
		}
	}

	public const int mapWidth = 16;

	public const int mapHeight = 16;

	public const int pixelZoom = 3;

	public const int bulletSpeed = 8;

	public const double lootChance = 0.05;

	public const double coinChance = 0.05;

	public int lootDuration = 7500;

	public int powerupDuration = 10000;

	public const int abigailPortraitDuration = 6000;

	public const float playerSpeed = 3f;

	public const int baseTileSize = 16;

	public const int orcSpeed = 2;

	public const int ogreSpeed = 1;

	public const int ghostSpeed = 3;

	public const int spikeySpeed = 3;

	public const int spikeyHealth = 2;

	public const int cactusDanceDelay = 800;

	public const int playerMotionDelay = 100;

	public const int playerFootStepDelay = 200;

	public const int deathDelay = 3000;

	public const int MAP_BARRIER1 = 0;

	public const int MAP_BARRIER2 = 1;

	public const int MAP_ROCKY1 = 2;

	public const int MAP_DESERT = 3;

	public const int MAP_GRASSY = 4;

	public const int MAP_CACTUS = 5;

	public const int MAP_FENCE = 7;

	public const int MAP_TRENCH1 = 8;

	public const int MAP_TRENCH2 = 9;

	public const int MAP_BRIDGE = 10;

	public const int orc = 0;

	public const int ghost = 1;

	public const int ogre = 2;

	public const int mummy = 3;

	public const int devil = 4;

	public const int mushroom = 5;

	public const int spikey = 6;

	public const int dracula = 7;

	public const int desert = 0;

	public const int woods = 2;

	public const int graveyard = 1;

	public const int POWERUP_LOG = -1;

	public const int POWERUP_SKULL = -2;

	public const int coin1 = 0;

	public const int coin5 = 1;

	public const int POWERUP_SPREAD = 2;

	public const int POWERUP_RAPIDFIRE = 3;

	public const int POWERUP_NUKE = 4;

	public const int POWERUP_ZOMBIE = 5;

	public const int POWERUP_SPEED = 6;

	public const int POWERUP_SHOTGUN = 7;

	public const int POWERUP_LIFE = 8;

	public const int POWERUP_TELEPORT = 9;

	public const int POWERUP_SHERRIFF = 10;

	public const int POWERUP_HEART = -3;

	public const int ITEM_FIRESPEED1 = 0;

	public const int ITEM_FIRESPEED2 = 1;

	public const int ITEM_FIRESPEED3 = 2;

	public const int ITEM_RUNSPEED1 = 3;

	public const int ITEM_RUNSPEED2 = 4;

	public const int ITEM_LIFE = 5;

	public const int ITEM_AMMO1 = 6;

	public const int ITEM_AMMO2 = 7;

	public const int ITEM_AMMO3 = 8;

	public const int ITEM_SPREADPISTOL = 9;

	public const int ITEM_STAR = 10;

	public const int ITEM_SKULL = 11;

	public const int ITEM_LOG = 12;

	public const int option_retry = 0;

	public const int option_quit = 1;

	public int runSpeedLevel;

	public int fireSpeedLevel;

	public int ammoLevel;

	public int whichRound;

	public bool spreadPistol;

	public const int waveDuration = 80000;

	public const int betweenWaveDuration = 5000;

	public static List<CowboyMonster> monsters = new List<CowboyMonster>();

	protected HashSet<Vector2> _borderTiles = new HashSet<Vector2>();

	public Vector2 playerPosition;

	public static Vector2 player2Position = default(Vector2);

	public Rectangle playerBoundingBox;

	public Rectangle merchantBox;

	public Rectangle player2BoundingBox;

	public Rectangle noPickUpBox;

	public static List<int> playerMovementDirections = new List<int>();

	public static List<int> playerShootingDirections = new List<int>();

	public List<int> player2MovementDirections = new List<int>();

	public List<int> player2ShootingDirections = new List<int>();

	public int shootingDelay = 300;

	public int shotTimer;

	public int motionPause;

	public int bulletDamage;

	public int lives = 3;

	public int coins;

	public int score;

	public int player2deathtimer;

	public int player2invincibletimer;

	public List<CowboyBullet> bullets = new List<CowboyBullet>();

	public static List<CowboyBullet> enemyBullets = new List<CowboyBullet>();

	public static int[,] map = new int[16, 16];

	public static int[,] nextMap = new int[16, 16];

	public List<Point>[] spawnQueue = new List<Point>[4];

	public static Vector2 topLeftScreenCoordinate;

	public float cactusDanceTimer;

	public float playerMotionAnimationTimer;

	public float playerFootstepSoundTimer = 200f;

	public behaviorAfterMotionPause behaviorAfterPause;

	public List<Vector2> monsterChances = new List<Vector2>
	{
		new Vector2(0.014f, 0.4f),
		Vector2.Zero,
		Vector2.Zero,
		Vector2.Zero,
		Vector2.Zero,
		Vector2.Zero,
		Vector2.Zero
	};

	public Rectangle shoppingCarpetNoPickup;

	public Dictionary<int, int> activePowerups = new Dictionary<int, int>();

	/// <summary>The Abigail NPC whose dialogues to show, if playing with Abigail.</summary>
	public NPC abigail;

	public static List<CowboyPowerup> powerups = new List<CowboyPowerup>();

	public string AbigailDialogue = "";

	public static TemporaryAnimatedSpriteList temporarySprites = new TemporaryAnimatedSpriteList();

	public CowboyPowerup heldItem;

	public static int world = 0;

	public int gameOverOption;

	public int gamerestartTimer;

	public int player2TargetUpdateTimer;

	public int player2shotTimer;

	public int player2AnimationTimer;

	public int fadethenQuitTimer;

	public int abigailPortraitYposition;

	public int abigailPortraitTimer;

	public int abigailPortraitExpression;

	public static int waveTimer = 80000;

	public static int betweenWaveTimer = 5000;

	public static int whichWave;

	public static int monsterConfusionTimer;

	public static int zombieModeTimer;

	public static int shoppingTimer;

	public static int holdItemTimer;

	public static int itemToHold;

	public static int newMapPosition;

	public static int playerInvincibleTimer;

	public static int screenFlash;

	public static int gopherTrainPosition;

	public static int endCutsceneTimer;

	public static int endCutscenePhase;

	public static int startTimer;

	public static float deathTimer;

	public static bool onStartMenu;

	public static bool shopping;

	public static bool gopherRunning;

	public static bool store;

	public static bool merchantLeaving;

	public static bool merchantArriving;

	public static bool merchantShopOpen;

	public static bool waitingForPlayerToMoveDownAMap;

	public static bool scrollingMap;

	public static bool hasGopherAppeared;

	public static bool shootoutLevel;

	public static bool gopherTrain;

	public static bool playerJumped;

	public static bool endCutscene;

	public static bool gameOver;

	public static bool playingWithAbigail;

	public static bool beatLevelWithAbigail;

	public Dictionary<Rectangle, int> storeItems = new Dictionary<Rectangle, int>();

	public bool quit;

	public bool died;

	public static Rectangle gopherBox;

	public Point gopherMotion;

	private static ICue overworldSong;

	private static ICue outlawSong;

	private static ICue zombieSong;

	protected Dictionary<GameKeys, List<Keys>> _binds;

	protected HashSet<GameKeys> _buttonHeldState = new HashSet<GameKeys>();

	protected Dictionary<GameKeys, int> _buttonHeldFrames;

	private int player2FootstepSoundTimer;

	public CowboyMonster targetMonster;

	public static int TileSize => 48;

	public bool LoadGame()
	{
		if (AbigailGame.playingWithAbigail)
		{
			return false;
		}
		if (Game1.player.jotpkProgress.Value == null)
		{
			return false;
		}
		JOTPKProgress save_data = Game1.player.jotpkProgress.Value;
		this.ammoLevel = save_data.ammoLevel.Value;
		this.bulletDamage = save_data.bulletDamage.Value;
		this.coins = save_data.coins.Value;
		this.died = save_data.died.Value;
		this.fireSpeedLevel = save_data.fireSpeedLevel.Value;
		this.lives = save_data.lives.Value;
		this.score = save_data.score.Value;
		this.runSpeedLevel = save_data.runSpeedLevel.Value;
		this.spreadPistol = save_data.spreadPistol.Value;
		this.whichRound = save_data.whichRound.Value;
		AbigailGame.whichWave = save_data.whichWave.Value;
		AbigailGame.waveTimer = save_data.waveTimer.Value;
		AbigailGame.world = save_data.world.Value;
		if (save_data.heldItem.Value != -100)
		{
			this.heldItem = new CowboyPowerup(save_data.heldItem.Value, Point.Zero, 9999);
		}
		this.monsterChances = new List<Vector2>(save_data.monsterChances);
		this.ApplyLevelSpecificStates();
		if (AbigailGame.shootoutLevel)
		{
			this.playerPosition = new Vector2(8 * AbigailGame.TileSize, 3 * AbigailGame.TileSize);
		}
		return true;
	}

	public void SaveGame()
	{
		if (!AbigailGame.playingWithAbigail)
		{
			if (Game1.player.jotpkProgress.Value == null)
			{
				Game1.player.jotpkProgress.Value = new JOTPKProgress();
			}
			JOTPKProgress save_data = Game1.player.jotpkProgress.Value;
			save_data.ammoLevel.Value = this.ammoLevel;
			save_data.bulletDamage.Value = this.bulletDamage;
			save_data.coins.Value = this.coins;
			save_data.died.Value = this.died;
			save_data.fireSpeedLevel.Value = this.fireSpeedLevel;
			save_data.lives.Value = this.lives;
			save_data.score.Value = this.score;
			save_data.runSpeedLevel.Value = this.runSpeedLevel;
			save_data.spreadPistol.Value = this.spreadPistol;
			save_data.whichRound.Value = this.whichRound;
			save_data.whichWave.Value = AbigailGame.whichWave;
			save_data.waveTimer.Value = AbigailGame.waveTimer;
			save_data.world.Value = AbigailGame.world;
			save_data.monsterChances.Clear();
			save_data.monsterChances.AddRange(this.monsterChances);
			if (this.heldItem == null)
			{
				save_data.heldItem.Value = -100;
			}
			else
			{
				save_data.heldItem.Value = this.heldItem.which;
			}
		}
	}

	/// <summary>Construct an instance.</summary>
	/// <param name="abigail">The Abigail NPC whose dialogues to show, if playing with Abigail.</param>
	public AbigailGame(NPC abigail = null)
	{
		this.abigail = abigail;
		bool playingWithAbby = abigail != null;
		this.reset(playingWithAbby);
		if (!AbigailGame.playingWithAbigail && this.LoadGame())
		{
			AbigailGame.map = this.getMap(AbigailGame.whichWave);
		}
	}

	public AbigailGame(int coins, int ammoLevel, int bulletDamage, int fireSpeedLevel, int runSpeedLevel, int lives, bool spreadPistol, int whichRound)
	{
		this.reset(playingWithAbby: false);
		this.coins = coins;
		this.ammoLevel = ammoLevel;
		this.bulletDamage = bulletDamage;
		this.fireSpeedLevel = fireSpeedLevel;
		this.runSpeedLevel = runSpeedLevel;
		this.lives = lives;
		this.spreadPistol = spreadPistol;
		this.whichRound = whichRound;
		this.ApplyNewGamePlus();
		this.SaveGame();
		AbigailGame.onStartMenu = false;
	}

	public void ApplyNewGamePlus()
	{
		this.monsterChances[0] = new Vector2(0.014f + (float)this.whichRound * 0.005f, 0.41f + (float)this.whichRound * 0.05f);
		this.monsterChances[4] = new Vector2(0.002f, 0.1f);
	}

	public void reset(bool playingWithAbby)
	{
		Rectangle r = new Rectangle(0, 0, 16, 16);
		this._borderTiles = new HashSet<Vector2>(Utility.getBorderOfThisRectangle(r));
		this.died = false;
		AbigailGame.topLeftScreenCoordinate = new Vector2(Game1.viewport.Width / 2 - 384, Game1.viewport.Height / 2 - 384);
		AbigailGame.enemyBullets.Clear();
		AbigailGame.holdItemTimer = 0;
		AbigailGame.itemToHold = -1;
		AbigailGame.merchantArriving = false;
		AbigailGame.merchantLeaving = false;
		AbigailGame.merchantShopOpen = false;
		AbigailGame.monsterConfusionTimer = 0;
		AbigailGame.monsters.Clear();
		AbigailGame.newMapPosition = 16 * AbigailGame.TileSize;
		AbigailGame.scrollingMap = false;
		AbigailGame.shopping = false;
		AbigailGame.store = false;
		AbigailGame.temporarySprites.Clear();
		AbigailGame.waitingForPlayerToMoveDownAMap = false;
		AbigailGame.waveTimer = 80000;
		AbigailGame.whichWave = 0;
		AbigailGame.zombieModeTimer = 0;
		this.bulletDamage = 1;
		AbigailGame.deathTimer = 0f;
		AbigailGame.shootoutLevel = false;
		AbigailGame.betweenWaveTimer = 5000;
		AbigailGame.gopherRunning = false;
		AbigailGame.hasGopherAppeared = false;
		AbigailGame.playerMovementDirections.Clear();
		AbigailGame.outlawSong = null;
		AbigailGame.overworldSong = null;
		AbigailGame.endCutscene = false;
		AbigailGame.endCutscenePhase = 0;
		AbigailGame.endCutsceneTimer = 0;
		AbigailGame.gameOver = false;
		AbigailGame.deathTimer = 0f;
		AbigailGame.playerInvincibleTimer = 0;
		AbigailGame.playingWithAbigail = playingWithAbby;
		AbigailGame.beatLevelWithAbigail = false;
		AbigailGame.onStartMenu = true;
		AbigailGame.startTimer = 0;
		AbigailGame.powerups.Clear();
		AbigailGame.world = 0;
		Game1.changeMusicTrack("none", track_interruptable: false, MusicContext.MiniGame);
		for (int i = 0; i < 16; i++)
		{
			for (int j = 0; j < 16; j++)
			{
				if ((i == 0 || i == 15 || j == 0 || j == 15) && (i <= 6 || i >= 10) && (j <= 6 || j >= 10))
				{
					AbigailGame.map[i, j] = 5;
				}
				else if (i == 0 || i == 15 || j == 0 || j == 15)
				{
					AbigailGame.map[i, j] = ((Game1.random.NextDouble() < 0.15) ? 1 : 0);
				}
				else if (i == 1 || i == 14 || j == 1 || j == 14)
				{
					AbigailGame.map[i, j] = 2;
				}
				else
				{
					AbigailGame.map[i, j] = ((Game1.random.NextDouble() < 0.1) ? 4 : 3);
				}
			}
		}
		this.playerPosition = new Vector2(384f, 384f);
		this.playerBoundingBox.X = (int)this.playerPosition.X + AbigailGame.TileSize / 4;
		this.playerBoundingBox.Y = (int)this.playerPosition.Y + AbigailGame.TileSize / 4;
		this.playerBoundingBox.Width = AbigailGame.TileSize / 2;
		this.playerBoundingBox.Height = AbigailGame.TileSize / 2;
		if (AbigailGame.playingWithAbigail)
		{
			AbigailGame.onStartMenu = false;
			AbigailGame.player2Position = new Vector2(432f, 384f);
			this.player2BoundingBox = new Rectangle(9 * AbigailGame.TileSize, 8 * AbigailGame.TileSize, AbigailGame.TileSize, AbigailGame.TileSize);
			AbigailGame.betweenWaveTimer += 1500;
		}
		for (int k = 0; k < 4; k++)
		{
			this.spawnQueue[k] = new List<Point>();
		}
		this.noPickUpBox = new Rectangle(0, 0, AbigailGame.TileSize, AbigailGame.TileSize);
		this.merchantBox = new Rectangle(8 * AbigailGame.TileSize, 0, AbigailGame.TileSize, AbigailGame.TileSize);
		AbigailGame.newMapPosition = 16 * AbigailGame.TileSize;
		if (!Stats.AllowRetroactiveAchievements)
		{
			Game1.stats.checkForMiniGameAchievements(isDirectUnlock: true);
		}
	}

	public float getMovementSpeed(float speed, int directions)
	{
		float movementSpeed = speed;
		if (directions > 1)
		{
			movementSpeed = Math.Max(1, (int)Math.Sqrt(2f * (movementSpeed * movementSpeed)) / 2);
		}
		return movementSpeed;
	}

	/// <summary>
	/// return true if powerup should be removed
	/// </summary>
	/// <param name="c"></param>
	/// <returns></returns>
	public bool getPowerUp(CowboyPowerup c)
	{
		switch (c.which)
		{
		case -3:
			this.usePowerup(-3);
			break;
		case -2:
			this.usePowerup(-2);
			break;
		case -1:
			this.usePowerup(-1);
			break;
		case 0:
			this.coins++;
			Game1.playSound("Pickup_Coin15");
			break;
		case 1:
			this.coins += 5;
			Game1.playSound("Pickup_Coin15");
			break;
		case 8:
			this.lives++;
			Game1.playSound("cowboy_powerup");
			break;
		default:
		{
			if (this.heldItem == null)
			{
				this.heldItem = c;
				Game1.playSound("cowboy_powerup");
				break;
			}
			CowboyPowerup tmp = this.heldItem;
			this.heldItem = c;
			this.noPickUpBox.Location = c.position;
			tmp.position = c.position;
			AbigailGame.powerups.Add(tmp);
			Game1.playSound("cowboy_powerup");
			return true;
		}
		}
		return true;
	}

	public bool overrideFreeMouseMovement()
	{
		return Game1.options.SnappyMenus;
	}

	public void usePowerup(int which)
	{
		if (this.activePowerups.ContainsKey(which))
		{
			this.activePowerups[which] = this.powerupDuration + 2000;
			return;
		}
		int num;
		switch (which)
		{
		case -3:
			AbigailGame.itemToHold = 13;
			AbigailGame.holdItemTimer = 4000;
			Game1.playSound("Cowboy_Secret");
			AbigailGame.endCutscene = true;
			AbigailGame.endCutsceneTimer = 4000;
			AbigailGame.world = 0;
			break;
		case -2:
			num = 11;
			goto IL_00b7;
		case -1:
			num = 12;
			goto IL_00b7;
		case 10:
		{
			this.usePowerup(7);
			this.usePowerup(3);
			this.usePowerup(6);
			for (int j = 0; j < this.activePowerups.Count; j++)
			{
				this.activePowerups[this.activePowerups.ElementAt(j).Key] *= 2;
			}
			break;
		}
		case 5:
			if (AbigailGame.overworldSong != null && AbigailGame.overworldSong.IsPlaying)
			{
				AbigailGame.overworldSong.Stop(AudioStopOptions.Immediate);
			}
			if (AbigailGame.zombieSong != null && AbigailGame.zombieSong.IsPlaying)
			{
				AbigailGame.zombieSong.Stop(AudioStopOptions.Immediate);
				AbigailGame.zombieSong = null;
			}
			Game1.playSound("Cowboy_undead", out AbigailGame.zombieSong);
			this.motionPause = 1800;
			AbigailGame.zombieModeTimer = 10000;
			break;
		case 9:
		{
			Point teleportSpot = Point.Zero;
			int tries = 0;
			while ((Math.Abs((float)teleportSpot.X - this.playerPosition.X) < 8f || Math.Abs((float)teleportSpot.Y - this.playerPosition.Y) < 8f || AbigailGame.isCollidingWithMap(teleportSpot) || AbigailGame.isCollidingWithMonster(new Rectangle(teleportSpot.X, teleportSpot.Y, AbigailGame.TileSize, AbigailGame.TileSize), null)) && tries < 10)
			{
				teleportSpot = new Point(Game1.random.Next(AbigailGame.TileSize, 16 * AbigailGame.TileSize - AbigailGame.TileSize), Game1.random.Next(AbigailGame.TileSize, 16 * AbigailGame.TileSize - AbigailGame.TileSize));
				tries++;
			}
			if (tries < 10)
			{
				AbigailGame.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(464, 1792, 16, 16), 120f, 5, 0, this.playerPosition + AbigailGame.topLeftScreenCoordinate + new Vector2(AbigailGame.TileSize / 2, AbigailGame.TileSize / 2), flicker: false, flipped: false, 1f, 0f, Color.White, 3f, 0f, 0f, 0f, local: true));
				AbigailGame.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(464, 1792, 16, 16), 120f, 5, 0, new Vector2(teleportSpot.X, teleportSpot.Y) + AbigailGame.topLeftScreenCoordinate + new Vector2(AbigailGame.TileSize / 2, AbigailGame.TileSize / 2), flicker: false, flipped: false, 1f, 0f, Color.White, 3f, 0f, 0f, 0f, local: true));
				AbigailGame.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(464, 1792, 16, 16), 120f, 5, 0, new Vector2(teleportSpot.X - AbigailGame.TileSize / 2, teleportSpot.Y) + AbigailGame.topLeftScreenCoordinate + new Vector2(AbigailGame.TileSize / 2, AbigailGame.TileSize / 2), flicker: false, flipped: false, 1f, 0f, Color.White, 3f, 0f, 0f, 0f, local: true)
				{
					delayBeforeAnimationStart = 200
				});
				AbigailGame.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(464, 1792, 16, 16), 120f, 5, 0, new Vector2(teleportSpot.X + AbigailGame.TileSize / 2, teleportSpot.Y) + AbigailGame.topLeftScreenCoordinate + new Vector2(AbigailGame.TileSize / 2, AbigailGame.TileSize / 2), flicker: false, flipped: false, 1f, 0f, Color.White, 3f, 0f, 0f, 0f, local: true)
				{
					delayBeforeAnimationStart = 400
				});
				AbigailGame.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(464, 1792, 16, 16), 120f, 5, 0, new Vector2(teleportSpot.X, teleportSpot.Y - AbigailGame.TileSize / 2) + AbigailGame.topLeftScreenCoordinate + new Vector2(AbigailGame.TileSize / 2, AbigailGame.TileSize / 2), flicker: false, flipped: false, 1f, 0f, Color.White, 3f, 0f, 0f, 0f, local: true)
				{
					delayBeforeAnimationStart = 600
				});
				AbigailGame.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(464, 1792, 16, 16), 120f, 5, 0, new Vector2(teleportSpot.X, teleportSpot.Y + AbigailGame.TileSize / 2) + AbigailGame.topLeftScreenCoordinate + new Vector2(AbigailGame.TileSize / 2, AbigailGame.TileSize / 2), flicker: false, flipped: false, 1f, 0f, Color.White, 3f, 0f, 0f, 0f, local: true)
				{
					delayBeforeAnimationStart = 800
				});
				this.playerPosition = new Vector2(teleportSpot.X, teleportSpot.Y);
				AbigailGame.monsterConfusionTimer = 4000;
				AbigailGame.playerInvincibleTimer = 4000;
				Game1.playSound("cowboy_powerup");
			}
			break;
		}
		case 8:
			this.lives++;
			Game1.playSound("cowboy_powerup");
			break;
		case 4:
		{
			Game1.playSound("cowboy_explosion");
			if (!AbigailGame.shootoutLevel)
			{
				foreach (CowboyMonster c in AbigailGame.monsters)
				{
					AbigailGame.addGuts(c.position.Location, c.type);
				}
				AbigailGame.monsters.Clear();
			}
			else
			{
				foreach (CowboyMonster c2 in AbigailGame.monsters)
				{
					c2.takeDamage(30);
					this.bullets.Add(new CowboyBullet(c2.position.Center, 2, 1));
				}
			}
			for (int i = 0; i < 30; i++)
			{
				AbigailGame.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(464, 1792, 16, 16), 80f, 5, 0, new Vector2(Game1.random.Next(1, 16), Game1.random.Next(1, 16)) * AbigailGame.TileSize + AbigailGame.topLeftScreenCoordinate + new Vector2(AbigailGame.TileSize / 2, AbigailGame.TileSize / 2), flicker: false, flipped: false, 1f, 0f, Color.White, 3f, 0f, 0f, 0f, local: true)
				{
					delayBeforeAnimationStart = Game1.random.Next(800)
				});
			}
			break;
		}
		case 2:
		case 3:
		case 7:
			this.shotTimer = 0;
			Game1.playSound("cowboy_gunload");
			this.activePowerups.Add(which, this.powerupDuration + 2000);
			break;
		case 0:
			this.coins++;
			Game1.playSound("Pickup_Coin15");
			break;
		case 1:
			this.coins += 5;
			Game1.playSound("Pickup_Coin15");
			Game1.playSound("Pickup_Coin15");
			break;
		default:
			{
				this.activePowerups.Add(which, this.powerupDuration);
				Game1.playSound("cowboy_powerup");
				break;
			}
			IL_00b7:
			AbigailGame.itemToHold = num;
			AbigailGame.holdItemTimer = 2000;
			Game1.playSound("Cowboy_Secret");
			AbigailGame.gopherTrain = true;
			AbigailGame.gopherTrainPosition = -AbigailGame.TileSize * 2;
			break;
		}
		if (this.whichRound > 0 && this.activePowerups.ContainsKey(which))
		{
			this.activePowerups[which] /= 2;
		}
	}

	public static void addGuts(Point position, int whichGuts)
	{
		switch (whichGuts)
		{
		case 0:
		case 2:
		case 5:
		case 6:
		case 7:
			AbigailGame.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(512, 1696, 16, 16), 80f, 6, 0, AbigailGame.topLeftScreenCoordinate + new Vector2(position.X, position.Y), flicker: false, Game1.random.NextBool(), 0.001f, 0f, Color.White, 3f, 0f, 0f, 0f, local: true));
			AbigailGame.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(592, 1696, 16, 16), 10000f, 1, 0, AbigailGame.topLeftScreenCoordinate + new Vector2(position.X, position.Y), flicker: false, Game1.random.NextBool(), 0.001f, 0f, Color.White, 3f, 0f, 0f, 0f, local: true)
			{
				delayBeforeAnimationStart = 480
			});
			break;
		case 3:
			AbigailGame.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(464, 1792, 16, 16), 80f, 5, 0, AbigailGame.topLeftScreenCoordinate + new Vector2(position.X, position.Y), flicker: false, Game1.random.NextBool(), 0.001f, 0f, Color.White, 3f, 0f, 0f, 0f, local: true));
			break;
		case 1:
		case 4:
			AbigailGame.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(544, 1728, 16, 16), 80f, 4, 0, AbigailGame.topLeftScreenCoordinate + new Vector2(position.X, position.Y), flicker: false, Game1.random.NextBool(), 0.001f, 0f, Color.White, 3f, 0f, 0f, 0f, local: true));
			break;
		}
	}

	public void endOfGopherAnimationBehavior2(int extraInfo)
	{
		Game1.playSound("cowboy_gopher");
		if (Math.Abs(AbigailGame.gopherBox.X - 8 * AbigailGame.TileSize) > Math.Abs(AbigailGame.gopherBox.Y - 8 * AbigailGame.TileSize))
		{
			if (AbigailGame.gopherBox.X > 8 * AbigailGame.TileSize)
			{
				this.gopherMotion = new Point(-2, 0);
			}
			else
			{
				this.gopherMotion = new Point(2, 0);
			}
		}
		else if (AbigailGame.gopherBox.Y > 8 * AbigailGame.TileSize)
		{
			this.gopherMotion = new Point(0, -2);
		}
		else
		{
			this.gopherMotion = new Point(0, 2);
		}
		AbigailGame.gopherRunning = true;
	}

	public void endOfGopherAnimationBehavior(int extrainfo)
	{
		AbigailGame.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(384, 1792, 16, 16), 120f, 4, 2, AbigailGame.topLeftScreenCoordinate + new Vector2(AbigailGame.gopherBox.X + AbigailGame.TileSize / 2, AbigailGame.gopherBox.Y + AbigailGame.TileSize / 2), flicker: false, flipped: false, (float)AbigailGame.gopherBox.Y / 10000f, 0f, Color.White, 3f, 0f, 0f, 0f, local: true)
		{
			endFunction = endOfGopherAnimationBehavior2
		});
		Game1.playSound("cowboy_gopher");
	}

	public void updateBullets(GameTime time)
	{
		for (int i = this.bullets.Count - 1; i >= 0; i--)
		{
			this.bullets[i].position.X += this.bullets[i].motion.X;
			this.bullets[i].position.Y += this.bullets[i].motion.Y;
			if (this.bullets[i].position.X <= 0 || this.bullets[i].position.Y <= 0 || this.bullets[i].position.X >= 768 || this.bullets[i].position.Y >= 768)
			{
				this.bullets.RemoveAt(i);
			}
			else if (AbigailGame.map[this.bullets[i].position.X / 16 / 3, this.bullets[i].position.Y / 16 / 3] == 7)
			{
				this.bullets.RemoveAt(i);
			}
			else
			{
				for (int k = AbigailGame.monsters.Count - 1; k >= 0; k--)
				{
					if (AbigailGame.monsters[k].position.Intersects(new Rectangle(this.bullets[i].position.X, this.bullets[i].position.Y, 12, 12)))
					{
						int monsterhealth = AbigailGame.monsters[k].health;
						int monsterAfterDamageHealth;
						if (AbigailGame.monsters[k].takeDamage(this.bullets[i].damage))
						{
							monsterAfterDamageHealth = AbigailGame.monsters[k].health;
							AbigailGame.addGuts(AbigailGame.monsters[k].position.Location, AbigailGame.monsters[k].type);
							int loot = AbigailGame.monsters[k].getLootDrop();
							if (this.whichRound == 1 && Game1.random.NextBool())
							{
								loot = -1;
							}
							if (this.whichRound > 0 && (loot == 5 || loot == 8) && Game1.random.NextDouble() < 0.4)
							{
								loot = -1;
							}
							if (loot != -1 && AbigailGame.whichWave != 12)
							{
								AbigailGame.powerups.Add(new CowboyPowerup(loot, AbigailGame.monsters[k].position.Location, this.lootDuration));
							}
							if (AbigailGame.shootoutLevel)
							{
								if (AbigailGame.whichWave == 12 && AbigailGame.monsters[k].type == -2)
								{
									Game1.playSound("cowboy_explosion");
									AbigailGame.powerups.Add(new CowboyPowerup(-3, new Point(8 * AbigailGame.TileSize, 10 * AbigailGame.TileSize), 9999999));
									this.noPickUpBox = new Rectangle(8 * AbigailGame.TileSize, 10 * AbigailGame.TileSize, AbigailGame.TileSize, AbigailGame.TileSize);
									if (AbigailGame.outlawSong != null && AbigailGame.outlawSong.IsPlaying)
									{
										AbigailGame.outlawSong.Stop(AudioStopOptions.Immediate);
									}
									AbigailGame.screenFlash = 200;
									for (int j = 0; j < 30; j++)
									{
										AbigailGame.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(512, 1696, 16, 16), 70f, 6, 0, new Vector2(AbigailGame.monsters[k].position.X + Game1.random.Next(-AbigailGame.TileSize, AbigailGame.TileSize), AbigailGame.monsters[k].position.Y + Game1.random.Next(-AbigailGame.TileSize, AbigailGame.TileSize)) + AbigailGame.topLeftScreenCoordinate + new Vector2(AbigailGame.TileSize / 2, AbigailGame.TileSize / 2), flicker: false, flipped: false, 1f, 0f, Color.White, 3f, 0f, 0f, 0f, local: true)
										{
											delayBeforeAnimationStart = j * 75
										});
										if (j % 4 == 0)
										{
											AbigailGame.addGuts(new Point(AbigailGame.monsters[k].position.X + Game1.random.Next(-AbigailGame.TileSize, AbigailGame.TileSize), AbigailGame.monsters[k].position.Y + Game1.random.Next(-AbigailGame.TileSize, AbigailGame.TileSize)), 7);
										}
										if (j % 4 == 0)
										{
											AbigailGame.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(464, 1792, 16, 16), 80f, 5, 0, new Vector2(AbigailGame.monsters[k].position.X + Game1.random.Next(-AbigailGame.TileSize, AbigailGame.TileSize), AbigailGame.monsters[k].position.Y + Game1.random.Next(-AbigailGame.TileSize, AbigailGame.TileSize)) + AbigailGame.topLeftScreenCoordinate + new Vector2(AbigailGame.TileSize / 2, AbigailGame.TileSize / 2), flicker: false, flipped: false, 1f, 0f, Color.White, 3f, 0f, 0f, 0f, local: true)
											{
												delayBeforeAnimationStart = j * 75
											});
										}
										if (j % 3 == 0)
										{
											AbigailGame.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(544, 1728, 16, 16), 100f, 4, 0, new Vector2(AbigailGame.monsters[k].position.X + Game1.random.Next(-AbigailGame.TileSize, AbigailGame.TileSize), AbigailGame.monsters[k].position.Y + Game1.random.Next(-AbigailGame.TileSize, AbigailGame.TileSize)) + AbigailGame.topLeftScreenCoordinate + new Vector2(AbigailGame.TileSize / 2, AbigailGame.TileSize / 2), flicker: false, flipped: false, 1f, 0f, Color.White, 3f, 0f, 0f, 0f, local: true)
											{
												delayBeforeAnimationStart = j * 75
											});
										}
									}
								}
								else if (AbigailGame.whichWave != 12)
								{
									AbigailGame.powerups.Add(new CowboyPowerup((AbigailGame.world == 0) ? (-1) : (-2), new Point(8 * AbigailGame.TileSize, 10 * AbigailGame.TileSize), 9999999));
									if (AbigailGame.outlawSong != null && AbigailGame.outlawSong.IsPlaying)
									{
										AbigailGame.outlawSong.Stop(AudioStopOptions.Immediate);
									}
									AbigailGame.map[8, 8] = 10;
									AbigailGame.screenFlash = 200;
									for (int l = 0; l < 15; l++)
									{
										AbigailGame.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(464, 1792, 16, 16), 80f, 5, 0, new Vector2(AbigailGame.monsters[k].position.X + Game1.random.Next(-AbigailGame.TileSize, AbigailGame.TileSize), AbigailGame.monsters[k].position.Y + Game1.random.Next(-AbigailGame.TileSize, AbigailGame.TileSize)) + AbigailGame.topLeftScreenCoordinate + new Vector2(AbigailGame.TileSize / 2, AbigailGame.TileSize / 2), flicker: false, flipped: false, 1f, 0f, Color.White, 3f, 0f, 0f, 0f, local: true)
										{
											delayBeforeAnimationStart = l * 75
										});
									}
								}
							}
							AbigailGame.monsters.RemoveAt(k);
							Game1.playSound("Cowboy_monsterDie");
						}
						else
						{
							monsterAfterDamageHealth = AbigailGame.monsters[k].health;
						}
						this.bullets[i].damage -= monsterhealth - monsterAfterDamageHealth;
						if (this.bullets[i].damage <= 0)
						{
							this.bullets.RemoveAt(i);
						}
						break;
					}
				}
			}
		}
		for (int i2 = AbigailGame.enemyBullets.Count - 1; i2 >= 0; i2--)
		{
			AbigailGame.enemyBullets[i2].position.X += AbigailGame.enemyBullets[i2].motion.X;
			AbigailGame.enemyBullets[i2].position.Y += AbigailGame.enemyBullets[i2].motion.Y;
			if (AbigailGame.enemyBullets[i2].position.X <= 0 || AbigailGame.enemyBullets[i2].position.Y <= 0 || AbigailGame.enemyBullets[i2].position.X >= 762 || AbigailGame.enemyBullets[i2].position.Y >= 762)
			{
				AbigailGame.enemyBullets.RemoveAt(i2);
			}
			else if (AbigailGame.map[(AbigailGame.enemyBullets[i2].position.X + 6) / 16 / 3, (AbigailGame.enemyBullets[i2].position.Y + 6) / 16 / 3] == 7)
			{
				AbigailGame.enemyBullets.RemoveAt(i2);
			}
			else if (AbigailGame.playerInvincibleTimer <= 0 && AbigailGame.deathTimer <= 0f && this.playerBoundingBox.Intersects(new Rectangle(AbigailGame.enemyBullets[i2].position.X, AbigailGame.enemyBullets[i2].position.Y, 15, 15)))
			{
				this.playerDie();
				break;
			}
		}
	}

	public void playerDie()
	{
		AbigailGame.gopherRunning = false;
		AbigailGame.hasGopherAppeared = false;
		this.spawnQueue = new List<Point>[4];
		for (int j = 0; j < 4; j++)
		{
			this.spawnQueue[j] = new List<Point>();
		}
		AbigailGame.enemyBullets.Clear();
		if (!AbigailGame.shootoutLevel)
		{
			AbigailGame.powerups.Clear();
			AbigailGame.monsters.Clear();
		}
		this.died = true;
		this.activePowerups.Clear();
		AbigailGame.deathTimer = 3000f;
		if (AbigailGame.overworldSong != null && AbigailGame.overworldSong.IsPlaying)
		{
			AbigailGame.overworldSong.Stop(AudioStopOptions.Immediate);
		}
		AbigailGame.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(464, 1808, 16, 16), 120f, 5, 0, this.playerPosition + AbigailGame.topLeftScreenCoordinate, flicker: false, flipped: false, 1f, 0f, Color.White, 3f, 0f, 0f, 0f, local: true));
		AbigailGame.waveTimer = Math.Min(80000, AbigailGame.waveTimer + 10000);
		AbigailGame.betweenWaveTimer = 4000;
		this.lives--;
		AbigailGame.playerInvincibleTimer = 5000;
		if (AbigailGame.shootoutLevel)
		{
			this.playerPosition = new Vector2(8 * AbigailGame.TileSize, 3 * AbigailGame.TileSize);
			Game1.playSound("Cowboy_monsterDie");
		}
		else
		{
			this.playerPosition = new Vector2(8 * AbigailGame.TileSize - AbigailGame.TileSize, 8 * AbigailGame.TileSize);
			this.playerBoundingBox.X = (int)this.playerPosition.X + AbigailGame.TileSize / 4;
			this.playerBoundingBox.Y = (int)this.playerPosition.Y + AbigailGame.TileSize / 4;
			this.playerBoundingBox.Width = AbigailGame.TileSize / 2;
			this.playerBoundingBox.Height = AbigailGame.TileSize / 2;
			if (this.playerBoundingBox.Intersects(this.player2BoundingBox))
			{
				this.playerPosition.X -= AbigailGame.TileSize * 3 / 2;
				this.player2deathtimer = (int)AbigailGame.deathTimer;
				this.playerBoundingBox.X = (int)this.playerPosition.X + AbigailGame.TileSize / 4;
				this.playerBoundingBox.Y = (int)this.playerPosition.Y + AbigailGame.TileSize / 4;
				this.playerBoundingBox.Width = AbigailGame.TileSize / 2;
				this.playerBoundingBox.Height = AbigailGame.TileSize / 2;
			}
			Game1.playSound("cowboy_dead");
		}
		if (this.lives < 0)
		{
			AbigailGame.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(464, 1808, 16, 16), 550f, 5, 0, this.playerPosition + AbigailGame.topLeftScreenCoordinate, flicker: false, flipped: false, 1f, 0f, Color.White, 3f, 0f, 0f, 0f, local: true)
			{
				alpha = 0.001f,
				endFunction = afterPlayerDeathFunction
			});
			AbigailGame.deathTimer *= 3f;
			Game1.player.jotpkProgress.Value = null;
		}
		else if (!AbigailGame.shootoutLevel)
		{
			this.SaveGame();
		}
	}

	public void afterPlayerDeathFunction(int extra)
	{
		if (this.lives < 0)
		{
			AbigailGame.gameOver = true;
			if (AbigailGame.overworldSong != null && !AbigailGame.overworldSong.IsPlaying)
			{
				AbigailGame.overworldSong.Stop(AudioStopOptions.Immediate);
			}
			if (AbigailGame.outlawSong != null && !AbigailGame.outlawSong.IsPlaying)
			{
				AbigailGame.overworldSong.Stop(AudioStopOptions.Immediate);
			}
			AbigailGame.monsters.Clear();
			AbigailGame.powerups.Clear();
			this.died = false;
			Game1.playSound("Cowboy_monsterDie");
			if (AbigailGame.playingWithAbigail && Game1.currentLocation.currentEvent != null)
			{
				this.unload();
				Game1.currentMinigame = null;
				Game1.currentLocation.currentEvent.CurrentCommand++;
			}
		}
	}

	public void startAbigailPortrait(int whichExpression, string sayWhat)
	{
		if (this.abigail != null && this.abigailPortraitTimer <= 0)
		{
			this.abigailPortraitTimer = 6000;
			this.AbigailDialogue = sayWhat;
			this.abigailPortraitExpression = whichExpression;
			this.abigailPortraitYposition = Game1.viewport.Height;
			Game1.playSound("dwop");
		}
	}

	public void startNewRound()
	{
		this.gamerestartTimer = 2000;
		Game1.playSound("Cowboy_monsterDie");
		this.whichRound++;
	}

	protected void _UpdateInput()
	{
		if (Game1.options.gamepadControls)
		{
			GamePadState pad_state = Game1.input.GetGamePadState();
			ButtonCollection button_collection = new ButtonCollection(ref pad_state);
			if ((double)pad_state.ThumbSticks.Left.X < -0.2)
			{
				this._buttonHeldState.Add(GameKeys.MoveLeft);
			}
			if ((double)pad_state.ThumbSticks.Left.X > 0.2)
			{
				this._buttonHeldState.Add(GameKeys.MoveRight);
			}
			if ((double)pad_state.ThumbSticks.Left.Y < -0.2)
			{
				this._buttonHeldState.Add(GameKeys.MoveDown);
			}
			if ((double)pad_state.ThumbSticks.Left.Y > 0.2)
			{
				this._buttonHeldState.Add(GameKeys.MoveUp);
			}
			if ((double)pad_state.ThumbSticks.Right.X < -0.2)
			{
				this._buttonHeldState.Add(GameKeys.ShootLeft);
			}
			if ((double)pad_state.ThumbSticks.Right.X > 0.2)
			{
				this._buttonHeldState.Add(GameKeys.ShootRight);
			}
			if ((double)pad_state.ThumbSticks.Right.Y < -0.2)
			{
				this._buttonHeldState.Add(GameKeys.ShootDown);
			}
			if ((double)pad_state.ThumbSticks.Right.Y > 0.2)
			{
				this._buttonHeldState.Add(GameKeys.ShootUp);
			}
			ButtonCollection.ButtonEnumerator enumerator = button_collection.GetEnumerator();
			while (enumerator.MoveNext())
			{
				switch (enumerator.Current)
				{
				case Buttons.A:
					if (AbigailGame.gameOver)
					{
						this._buttonHeldState.Add(GameKeys.SelectOption);
					}
					else if (Program.sdk.IsEnterButtonAssignmentFlipped)
					{
						this._buttonHeldState.Add(GameKeys.ShootRight);
					}
					else
					{
						this._buttonHeldState.Add(GameKeys.ShootDown);
					}
					break;
				case Buttons.Y:
					this._buttonHeldState.Add(GameKeys.ShootUp);
					break;
				case Buttons.X:
					this._buttonHeldState.Add(GameKeys.ShootLeft);
					break;
				case Buttons.B:
					if (AbigailGame.gameOver)
					{
						this._buttonHeldState.Add(GameKeys.Exit);
					}
					else if (Program.sdk.IsEnterButtonAssignmentFlipped)
					{
						this._buttonHeldState.Add(GameKeys.ShootDown);
					}
					else
					{
						this._buttonHeldState.Add(GameKeys.ShootRight);
					}
					break;
				case Buttons.DPadUp:
					this._buttonHeldState.Add(GameKeys.MoveUp);
					break;
				case Buttons.DPadDown:
					this._buttonHeldState.Add(GameKeys.MoveDown);
					break;
				case Buttons.DPadLeft:
					this._buttonHeldState.Add(GameKeys.MoveLeft);
					break;
				case Buttons.DPadRight:
					this._buttonHeldState.Add(GameKeys.MoveRight);
					break;
				case Buttons.Start:
				case Buttons.LeftShoulder:
				case Buttons.RightShoulder:
				case Buttons.RightTrigger:
				case Buttons.LeftTrigger:
					this._buttonHeldState.Add(GameKeys.UsePowerup);
					break;
				case Buttons.Back:
					this._buttonHeldState.Add(GameKeys.Exit);
					break;
				}
			}
		}
		if (this._binds == null)
		{
			this.SetupBinds();
		}
		if (this.IsBoundButtonDown(GameKeys.MoveUp))
		{
			this._buttonHeldState.Add(GameKeys.MoveUp);
		}
		if (this.IsBoundButtonDown(GameKeys.MoveDown))
		{
			this._buttonHeldState.Add(GameKeys.MoveDown);
		}
		if (this.IsBoundButtonDown(GameKeys.MoveLeft))
		{
			this._buttonHeldState.Add(GameKeys.MoveLeft);
		}
		if (this.IsBoundButtonDown(GameKeys.MoveRight))
		{
			this._buttonHeldState.Add(GameKeys.MoveRight);
		}
		if (this.IsBoundButtonDown(GameKeys.ShootUp))
		{
			if (AbigailGame.gameOver)
			{
				this._buttonHeldState.Add(GameKeys.MoveUp);
			}
			else
			{
				this._buttonHeldState.Add(GameKeys.ShootUp);
			}
		}
		if (this.IsBoundButtonDown(GameKeys.ShootDown))
		{
			if (AbigailGame.gameOver)
			{
				this._buttonHeldState.Add(GameKeys.MoveDown);
			}
			else
			{
				this._buttonHeldState.Add(GameKeys.ShootDown);
			}
		}
		if (this.IsBoundButtonDown(GameKeys.ShootLeft))
		{
			this._buttonHeldState.Add(GameKeys.ShootLeft);
		}
		if (this.IsBoundButtonDown(GameKeys.ShootRight))
		{
			this._buttonHeldState.Add(GameKeys.ShootRight);
		}
		if (this.IsBoundButtonDown(GameKeys.UsePowerup))
		{
			if (AbigailGame.gameOver)
			{
				this._buttonHeldState.Add(GameKeys.SelectOption);
			}
			else
			{
				this._buttonHeldState.Add(GameKeys.UsePowerup);
			}
		}
		if (this.IsBoundButtonDown(GameKeys.Exit))
		{
			this._buttonHeldState.Add(GameKeys.Exit);
		}
	}

	public virtual void SetupBinds()
	{
		this._binds = new Dictionary<GameKeys, List<Keys>>();
		this._binds[GameKeys.MoveUp] = new List<Keys>(new Keys[1] { Keys.W });
		this._binds[GameKeys.MoveDown] = new List<Keys>(new Keys[1] { Keys.S });
		this._binds[GameKeys.MoveLeft] = new List<Keys>(new Keys[1] { Keys.A });
		this._binds[GameKeys.MoveRight] = new List<Keys>(new Keys[1] { Keys.D });
		this._binds[GameKeys.ShootUp] = new List<Keys>(new Keys[1] { Keys.Up });
		this._binds[GameKeys.ShootDown] = new List<Keys>(new Keys[1] { Keys.Down });
		this._binds[GameKeys.ShootLeft] = new List<Keys>(new Keys[1] { Keys.Left });
		this._binds[GameKeys.ShootRight] = new List<Keys>(new Keys[1] { Keys.Right });
		this._binds[GameKeys.UsePowerup] = new List<Keys>(new Keys[2]
		{
			Keys.Enter,
			Keys.Space
		});
		this._binds[GameKeys.Exit] = new List<Keys>(new Keys[1] { Keys.Escape });
		Keys key = this.GetBoundKey(Game1.options.moveUpButton);
		if (key != Keys.None && key != Keys.Up && key != Keys.Down && key != Keys.Left && key != Keys.Right)
		{
			this._binds[GameKeys.MoveUp] = new List<Keys>(new Keys[1] { key });
		}
		key = this.GetBoundKey(Game1.options.moveDownButton);
		if (key != Keys.None && key != Keys.Up && key != Keys.Down && key != Keys.Left && key != Keys.Right)
		{
			this._binds[GameKeys.MoveDown] = new List<Keys>(new Keys[1] { key });
		}
		key = this.GetBoundKey(Game1.options.moveLeftButton);
		if (key != Keys.None && key != Keys.Up && key != Keys.Down && key != Keys.Left && key != Keys.Right)
		{
			this._binds[GameKeys.MoveLeft] = new List<Keys>(new Keys[1] { key });
		}
		key = this.GetBoundKey(Game1.options.moveRightButton);
		if (key != Keys.None && key != Keys.Up && key != Keys.Down && key != Keys.Left && key != Keys.Right)
		{
			this._binds[GameKeys.MoveRight] = new List<Keys>(new Keys[1] { key });
		}
		bool x_bound = false;
		foreach (List<Keys> value in this._binds.Values)
		{
			if (value.Contains(Keys.X))
			{
				x_bound = true;
				break;
			}
		}
		if (!x_bound)
		{
			this._binds[GameKeys.UsePowerup].Add(Keys.X);
		}
	}

	public Keys GetBoundKey(InputButton[] button)
	{
		if (button == null || button.Length == 0)
		{
			return Keys.None;
		}
		for (int i = 0; i < button.Length; i++)
		{
			if (button[i].key != Keys.None)
			{
				return button[i].key;
			}
		}
		return Keys.None;
	}

	public bool IsBoundButtonDown(GameKeys game_key)
	{
		if (this._binds.TryGetValue(game_key, out var binds))
		{
			foreach (Keys key in binds)
			{
				if (Game1.input.GetKeyboardState().IsKeyDown(key))
				{
					return true;
				}
			}
		}
		return false;
	}

	public bool tick(GameTime time)
	{
		if (this._buttonHeldFrames == null)
		{
			this._buttonHeldFrames = new Dictionary<GameKeys, int>();
			for (int i = 0; i < 11; i++)
			{
				this._buttonHeldFrames[(GameKeys)i] = 0;
			}
		}
		this._buttonHeldState.Clear();
		if (AbigailGame.startTimer <= 0)
		{
			this._UpdateInput();
		}
		for (int j = 0; j < 11; j++)
		{
			if (this._buttonHeldState.Contains((GameKeys)j))
			{
				this._buttonHeldFrames[(GameKeys)j]++;
			}
			else
			{
				this._buttonHeldFrames[(GameKeys)j] = 0;
			}
		}
		this._ProcessInputs();
		if (this.quit)
		{
			Game1.stopMusicTrack(MusicContext.MiniGame);
			return true;
		}
		if (AbigailGame.gameOver)
		{
			AbigailGame.startTimer = 0;
			return false;
		}
		if (AbigailGame.onStartMenu)
		{
			if (AbigailGame.startTimer > 0)
			{
				AbigailGame.startTimer -= time.ElapsedGameTime.Milliseconds;
				if (AbigailGame.startTimer <= 0)
				{
					this.shotTimer = 100;
					AbigailGame.onStartMenu = false;
				}
			}
			else
			{
				Game1.playSound("Pickup_Coin15");
				AbigailGame.startTimer = 1500;
			}
			return false;
		}
		if (this.gamerestartTimer > 0)
		{
			this.gamerestartTimer -= time.ElapsedGameTime.Milliseconds;
			if (this.gamerestartTimer <= 0)
			{
				this.unload();
				if (this.whichRound == 0 || !AbigailGame.endCutscene)
				{
					Game1.currentMinigame = new AbigailGame();
				}
				else
				{
					Game1.currentMinigame = new AbigailGame(this.coins, this.ammoLevel, this.bulletDamage, this.fireSpeedLevel, this.runSpeedLevel, this.lives, this.spreadPistol, this.whichRound);
				}
			}
		}
		if (this.fadethenQuitTimer > 0 && (float)this.abigailPortraitTimer <= 0f)
		{
			this.fadethenQuitTimer -= time.ElapsedGameTime.Milliseconds;
			if (this.fadethenQuitTimer <= 0)
			{
				if (Game1.currentLocation.currentEvent != null)
				{
					Game1.currentLocation.currentEvent.CurrentCommand++;
					if (AbigailGame.beatLevelWithAbigail)
					{
						Game1.currentLocation.currentEvent.specialEventVariable1 = true;
					}
				}
				return true;
			}
		}
		if (this.abigailPortraitTimer > 0)
		{
			this.abigailPortraitTimer -= time.ElapsedGameTime.Milliseconds;
			if (this.abigailPortraitTimer > 1000 && this.abigailPortraitYposition > Game1.viewport.Height - 240)
			{
				this.abigailPortraitYposition -= 16;
			}
			else if (this.abigailPortraitTimer <= 1000)
			{
				this.abigailPortraitYposition += 16;
			}
		}
		if (AbigailGame.endCutscene)
		{
			AbigailGame.endCutsceneTimer -= time.ElapsedGameTime.Milliseconds;
			if (AbigailGame.endCutsceneTimer < 0)
			{
				AbigailGame.endCutscenePhase++;
				if (AbigailGame.endCutscenePhase > 5)
				{
					AbigailGame.endCutscenePhase = 5;
				}
				switch (AbigailGame.endCutscenePhase)
				{
				case 1:
					Game1.player.stats.Increment("completedPrairieKing");
					if (!this.died)
					{
						Game1.player.stats.Increment("completedPrairieKingWithoutDying");
					}
					Game1.player.AddMissedMailAndRecipes();
					Game1.player.stats.checkForMiniGameAchievements(isDirectUnlock: true);
					Game1.multiplayer.globalChatInfoMessage("PrairieKing", Game1.player.Name);
					AbigailGame.endCutsceneTimer = 15500;
					Game1.playSound("Cowboy_singing");
					AbigailGame.map = this.getMap(-1);
					break;
				case 2:
					this.playerPosition = new Vector2(0f, 8 * AbigailGame.TileSize);
					AbigailGame.endCutsceneTimer = 12000;
					break;
				case 3:
					AbigailGame.endCutsceneTimer = 5000;
					break;
				case 4:
					AbigailGame.endCutsceneTimer = 1000;
					break;
				case 5:
					if (Game1.input.GetKeyboardState().GetPressedKeys().Length == 0)
					{
						Game1.input.GetGamePadState();
						if (Game1.input.GetGamePadState().Buttons.X != ButtonState.Pressed && Game1.input.GetGamePadState().Buttons.Start != ButtonState.Pressed && Game1.input.GetGamePadState().Buttons.A != ButtonState.Pressed)
						{
							break;
						}
					}
					if (this.gamerestartTimer <= 0)
					{
						this.startNewRound();
					}
					break;
				}
			}
			if (AbigailGame.endCutscenePhase == 2 && this.playerPosition.X < (float)(9 * AbigailGame.TileSize))
			{
				this.playerPosition.X += 1f;
				this.playerMotionAnimationTimer += time.ElapsedGameTime.Milliseconds;
				this.playerMotionAnimationTimer %= 400f;
			}
			return false;
		}
		if (this.motionPause > 0)
		{
			this.motionPause -= time.ElapsedGameTime.Milliseconds;
			if (this.motionPause <= 0 && this.behaviorAfterPause != null)
			{
				this.behaviorAfterPause();
				this.behaviorAfterPause = null;
			}
		}
		else if (AbigailGame.monsterConfusionTimer > 0)
		{
			AbigailGame.monsterConfusionTimer -= time.ElapsedGameTime.Milliseconds;
		}
		if (AbigailGame.zombieModeTimer > 0)
		{
			AbigailGame.zombieModeTimer -= time.ElapsedGameTime.Milliseconds;
		}
		if (AbigailGame.holdItemTimer > 0)
		{
			AbigailGame.holdItemTimer -= time.ElapsedGameTime.Milliseconds;
			return false;
		}
		if (AbigailGame.screenFlash > 0)
		{
			AbigailGame.screenFlash -= time.ElapsedGameTime.Milliseconds;
		}
		if (AbigailGame.gopherTrain)
		{
			AbigailGame.gopherTrainPosition += 3;
			if (AbigailGame.gopherTrainPosition % 30 == 0)
			{
				Game1.playSound("Cowboy_Footstep");
			}
			if (AbigailGame.playerJumped)
			{
				this.playerPosition.Y += 3f;
			}
			if (Math.Abs(this.playerPosition.Y - (float)(AbigailGame.gopherTrainPosition - AbigailGame.TileSize)) <= 16f)
			{
				AbigailGame.playerJumped = true;
				this.playerPosition.Y = AbigailGame.gopherTrainPosition - AbigailGame.TileSize;
			}
			if (AbigailGame.gopherTrainPosition > 16 * AbigailGame.TileSize + AbigailGame.TileSize)
			{
				AbigailGame.gopherTrain = false;
				AbigailGame.playerJumped = false;
				AbigailGame.whichWave++;
				AbigailGame.map = this.getMap(AbigailGame.whichWave);
				this.playerPosition = new Vector2(8 * AbigailGame.TileSize, 8 * AbigailGame.TileSize);
				AbigailGame.world = ((AbigailGame.world != 0) ? 1 : 2);
				AbigailGame.waveTimer = 80000;
				AbigailGame.betweenWaveTimer = 5000;
				AbigailGame.waitingForPlayerToMoveDownAMap = false;
				AbigailGame.shootoutLevel = false;
				this.SaveGame();
			}
		}
		if ((AbigailGame.shopping || AbigailGame.merchantArriving || AbigailGame.merchantLeaving || AbigailGame.waitingForPlayerToMoveDownAMap) && AbigailGame.holdItemTimer <= 0)
		{
			int oldTimer = AbigailGame.shoppingTimer;
			AbigailGame.shoppingTimer += time.ElapsedGameTime.Milliseconds;
			AbigailGame.shoppingTimer %= 500;
			if (!AbigailGame.merchantShopOpen && AbigailGame.shopping && ((oldTimer < 250 && AbigailGame.shoppingTimer >= 250) || oldTimer > AbigailGame.shoppingTimer))
			{
				Game1.playSound("Cowboy_Footstep");
			}
		}
		if (AbigailGame.playerInvincibleTimer > 0)
		{
			AbigailGame.playerInvincibleTimer -= time.ElapsedGameTime.Milliseconds;
		}
		if (AbigailGame.scrollingMap)
		{
			AbigailGame.newMapPosition -= AbigailGame.TileSize / 8;
			this.playerPosition.Y -= AbigailGame.TileSize / 8;
			this.playerPosition.Y += 3f;
			this.playerBoundingBox.X = (int)this.playerPosition.X + AbigailGame.TileSize / 4;
			this.playerBoundingBox.Y = (int)this.playerPosition.Y + AbigailGame.TileSize / 4;
			this.playerBoundingBox.Width = AbigailGame.TileSize / 2;
			this.playerBoundingBox.Height = AbigailGame.TileSize / 2;
			AbigailGame.playerMovementDirections = new List<int> { 2 };
			this.playerMotionAnimationTimer += time.ElapsedGameTime.Milliseconds;
			this.playerMotionAnimationTimer %= 400f;
			if (AbigailGame.newMapPosition <= 0)
			{
				AbigailGame.scrollingMap = false;
				AbigailGame.map = AbigailGame.nextMap;
				AbigailGame.newMapPosition = 16 * AbigailGame.TileSize;
				AbigailGame.shopping = false;
				AbigailGame.betweenWaveTimer = 5000;
				AbigailGame.waitingForPlayerToMoveDownAMap = false;
				AbigailGame.playerMovementDirections.Clear();
				this.ApplyLevelSpecificStates();
			}
		}
		if (AbigailGame.gopherRunning)
		{
			AbigailGame.gopherBox.X += this.gopherMotion.X;
			AbigailGame.gopherBox.Y += this.gopherMotion.Y;
			for (int i2 = AbigailGame.monsters.Count - 1; i2 >= 0; i2--)
			{
				if (AbigailGame.gopherBox.Intersects(AbigailGame.monsters[i2].position))
				{
					AbigailGame.addGuts(AbigailGame.monsters[i2].position.Location, AbigailGame.monsters[i2].type);
					AbigailGame.monsters.RemoveAt(i2);
					Game1.playSound("Cowboy_monsterDie");
				}
			}
			if (AbigailGame.gopherBox.X < 0 || AbigailGame.gopherBox.Y < 0 || AbigailGame.gopherBox.X > 16 * AbigailGame.TileSize || AbigailGame.gopherBox.Y > 16 * AbigailGame.TileSize)
			{
				AbigailGame.gopherRunning = false;
			}
		}
		AbigailGame.temporarySprites.RemoveWhere((TemporaryAnimatedSprite sprite) => sprite.update(time));
		if (this.motionPause <= 0)
		{
			for (int i3 = AbigailGame.powerups.Count - 1; i3 >= 0; i3--)
			{
				if (Utility.distance(this.playerBoundingBox.Center.X, AbigailGame.powerups[i3].position.X + AbigailGame.TileSize / 2, this.playerBoundingBox.Center.Y, AbigailGame.powerups[i3].position.Y + AbigailGame.TileSize / 2) <= (float)(AbigailGame.TileSize + 3) && (AbigailGame.powerups[i3].position.X < AbigailGame.TileSize || AbigailGame.powerups[i3].position.X >= 16 * AbigailGame.TileSize - AbigailGame.TileSize || AbigailGame.powerups[i3].position.Y < AbigailGame.TileSize || AbigailGame.powerups[i3].position.Y >= 16 * AbigailGame.TileSize - AbigailGame.TileSize))
				{
					if (AbigailGame.powerups[i3].position.X + AbigailGame.TileSize / 2 < this.playerBoundingBox.Center.X)
					{
						AbigailGame.powerups[i3].position.X++;
					}
					if (AbigailGame.powerups[i3].position.X + AbigailGame.TileSize / 2 > this.playerBoundingBox.Center.X)
					{
						AbigailGame.powerups[i3].position.X--;
					}
					if (AbigailGame.powerups[i3].position.Y + AbigailGame.TileSize / 2 < this.playerBoundingBox.Center.Y)
					{
						AbigailGame.powerups[i3].position.Y++;
					}
					if (AbigailGame.powerups[i3].position.Y + AbigailGame.TileSize / 2 > this.playerBoundingBox.Center.Y)
					{
						AbigailGame.powerups[i3].position.Y--;
					}
				}
				AbigailGame.powerups[i3].duration -= time.ElapsedGameTime.Milliseconds;
				if (AbigailGame.powerups[i3].duration <= 0)
				{
					AbigailGame.powerups.RemoveAt(i3);
				}
			}
			for (int i4 = this.activePowerups.Count - 1; i4 >= 0; i4--)
			{
				int key = this.activePowerups.ElementAt(i4).Key;
				this.activePowerups[key] -= time.ElapsedGameTime.Milliseconds;
				if (this.activePowerups[key] <= 0)
				{
					this.activePowerups.Remove(key);
				}
			}
			if (AbigailGame.deathTimer <= 0f && AbigailGame.playerMovementDirections.Count > 0 && !AbigailGame.scrollingMap)
			{
				int effectiveDirections = AbigailGame.playerMovementDirections.Count;
				if (effectiveDirections >= 2 && AbigailGame.playerMovementDirections.Last() == (AbigailGame.playerMovementDirections.ElementAt(AbigailGame.playerMovementDirections.Count - 2) + 2) % 4)
				{
					effectiveDirections = 1;
				}
				float speed = this.getMovementSpeed(3f, effectiveDirections);
				if (this.activePowerups.Keys.Contains(6))
				{
					speed *= 1.5f;
				}
				if (AbigailGame.zombieModeTimer > 0)
				{
					speed *= 1.5f;
				}
				for (int i5 = 0; i5 < this.runSpeedLevel; i5++)
				{
					speed *= 1.25f;
				}
				for (int i6 = Math.Max(0, AbigailGame.playerMovementDirections.Count - 2); i6 < AbigailGame.playerMovementDirections.Count; i6++)
				{
					if (i6 != 0 || AbigailGame.playerMovementDirections.Count < 2 || AbigailGame.playerMovementDirections.Last() != (AbigailGame.playerMovementDirections.ElementAt(AbigailGame.playerMovementDirections.Count - 2) + 2) % 4)
					{
						Vector2 newPlayerPosition = this.playerPosition;
						switch (AbigailGame.playerMovementDirections.ElementAt(i6))
						{
						case 0:
							newPlayerPosition.Y -= speed;
							break;
						case 3:
							newPlayerPosition.X -= speed;
							break;
						case 2:
							newPlayerPosition.Y += speed;
							break;
						case 1:
							newPlayerPosition.X += speed;
							break;
						}
						Rectangle newPlayerBox = new Rectangle((int)newPlayerPosition.X + AbigailGame.TileSize / 4, (int)newPlayerPosition.Y + AbigailGame.TileSize / 4, AbigailGame.TileSize / 2, AbigailGame.TileSize / 2);
						if (!AbigailGame.isCollidingWithMap(newPlayerBox) && (!this.merchantBox.Intersects(newPlayerBox) || this.merchantBox.Intersects(this.playerBoundingBox)) && (!AbigailGame.playingWithAbigail || !newPlayerBox.Intersects(this.player2BoundingBox)))
						{
							this.playerPosition = newPlayerPosition;
						}
					}
				}
				this.playerBoundingBox.X = (int)this.playerPosition.X + AbigailGame.TileSize / 4;
				this.playerBoundingBox.Y = (int)this.playerPosition.Y + AbigailGame.TileSize / 4;
				this.playerBoundingBox.Width = AbigailGame.TileSize / 2;
				this.playerBoundingBox.Height = AbigailGame.TileSize / 2;
				this.playerMotionAnimationTimer += time.ElapsedGameTime.Milliseconds;
				this.playerMotionAnimationTimer %= 400f;
				this.playerFootstepSoundTimer -= time.ElapsedGameTime.Milliseconds;
				if (this.playerFootstepSoundTimer <= 0f)
				{
					Game1.playSound("Cowboy_Footstep");
					this.playerFootstepSoundTimer = 200f;
				}
				AbigailGame.powerups.RemoveAll(delegate(CowboyPowerup item)
				{
					if (this.playerBoundingBox.Intersects(new Rectangle(item.position.X, item.position.Y, AbigailGame.TileSize, AbigailGame.TileSize)) && !this.playerBoundingBox.Intersects(this.noPickUpBox))
					{
						if (this.heldItem != null)
						{
							this.usePowerup(item.which);
							return true;
						}
						if (this.getPowerUp(item))
						{
							return true;
						}
					}
					return false;
				});
				if (!this.playerBoundingBox.Intersects(this.noPickUpBox))
				{
					this.noPickUpBox.Location = new Point(0, 0);
				}
				if (AbigailGame.waitingForPlayerToMoveDownAMap && this.playerBoundingBox.Bottom >= 16 * AbigailGame.TileSize - AbigailGame.TileSize / 2)
				{
					this.SaveGame();
					AbigailGame.shopping = false;
					AbigailGame.merchantArriving = false;
					AbigailGame.merchantLeaving = false;
					AbigailGame.merchantShopOpen = false;
					this.merchantBox.Y = -AbigailGame.TileSize;
					AbigailGame.scrollingMap = true;
					AbigailGame.nextMap = this.getMap(AbigailGame.whichWave);
					AbigailGame.newMapPosition = 16 * AbigailGame.TileSize;
					AbigailGame.temporarySprites.Clear();
					AbigailGame.powerups.Clear();
				}
				if (!this.shoppingCarpetNoPickup.Intersects(this.playerBoundingBox))
				{
					this.shoppingCarpetNoPickup.X = -1000;
				}
			}
			if (AbigailGame.shopping)
			{
				if (this.merchantBox.Y < 8 * AbigailGame.TileSize - AbigailGame.TileSize * 3 && AbigailGame.merchantArriving)
				{
					this.merchantBox.Y += 2;
					if (this.merchantBox.Y >= 8 * AbigailGame.TileSize - AbigailGame.TileSize * 3)
					{
						AbigailGame.merchantShopOpen = true;
						Game1.playSound("cowboy_monsterhit");
						AbigailGame.map[8, 15] = 3;
						AbigailGame.map[7, 15] = 3;
						AbigailGame.map[7, 15] = 3;
						AbigailGame.map[8, 14] = 3;
						AbigailGame.map[7, 14] = 3;
						AbigailGame.map[7, 14] = 3;
						this.shoppingCarpetNoPickup = new Rectangle(this.merchantBox.X - AbigailGame.TileSize, this.merchantBox.Y + AbigailGame.TileSize, AbigailGame.TileSize * 3, AbigailGame.TileSize * 2);
					}
				}
				else if (AbigailGame.merchantLeaving)
				{
					this.merchantBox.Y -= 2;
					if (this.merchantBox.Y <= -AbigailGame.TileSize)
					{
						AbigailGame.shopping = false;
						AbigailGame.merchantLeaving = false;
						AbigailGame.merchantArriving = true;
					}
				}
				else if (AbigailGame.merchantShopOpen)
				{
					for (int i7 = this.storeItems.Count - 1; i7 >= 0; i7--)
					{
						KeyValuePair<Rectangle, int> pair = this.storeItems.ElementAt(i7);
						if (!this.playerBoundingBox.Intersects(this.shoppingCarpetNoPickup) && this.playerBoundingBox.Intersects(pair.Key) && this.coins >= this.getPriceForItem(pair.Value))
						{
							Game1.playSound("Cowboy_Secret");
							AbigailGame.holdItemTimer = 2500;
							this.motionPause = 2500;
							AbigailGame.itemToHold = pair.Value;
							this.storeItems.Remove(pair.Key);
							AbigailGame.merchantLeaving = true;
							AbigailGame.merchantArriving = false;
							AbigailGame.merchantShopOpen = false;
							this.coins -= this.getPriceForItem(AbigailGame.itemToHold);
							switch (AbigailGame.itemToHold)
							{
							case 6:
							case 7:
							case 8:
								this.ammoLevel++;
								this.bulletDamage++;
								break;
							case 0:
							case 1:
							case 2:
								this.fireSpeedLevel++;
								break;
							case 3:
							case 4:
								this.runSpeedLevel++;
								break;
							case 5:
								this.lives++;
								break;
							case 9:
								this.spreadPistol = true;
								break;
							case 10:
								this.heldItem = new CowboyPowerup(10, Point.Zero, 9999);
								break;
							}
						}
					}
				}
			}
			this.cactusDanceTimer += time.ElapsedGameTime.Milliseconds;
			this.cactusDanceTimer %= 1600f;
			if (this.shotTimer > 0)
			{
				this.shotTimer -= time.ElapsedGameTime.Milliseconds;
			}
			if (AbigailGame.deathTimer <= 0f && AbigailGame.playerShootingDirections.Count > 0 && this.shotTimer <= 0)
			{
				if (this.activePowerups.ContainsKey(2))
				{
					this.spawnBullets(new int[1], this.playerPosition);
					this.spawnBullets(new int[1] { 1 }, this.playerPosition);
					this.spawnBullets(new int[1] { 2 }, this.playerPosition);
					this.spawnBullets(new int[1] { 3 }, this.playerPosition);
					this.spawnBullets(new int[2] { 0, 1 }, this.playerPosition);
					this.spawnBullets(new int[2] { 1, 2 }, this.playerPosition);
					this.spawnBullets(new int[2] { 2, 3 }, this.playerPosition);
					this.spawnBullets(new int[2] { 3, 0 }, this.playerPosition);
				}
				else if (AbigailGame.playerShootingDirections.Count == 1 || AbigailGame.playerShootingDirections.Last() == (AbigailGame.playerShootingDirections.ElementAt(AbigailGame.playerShootingDirections.Count - 2) + 2) % 4)
				{
					this.spawnBullets(new int[1] { (AbigailGame.playerShootingDirections.Count == 2 && AbigailGame.playerShootingDirections.Last() == (AbigailGame.playerShootingDirections.ElementAt(AbigailGame.playerShootingDirections.Count - 2) + 2) % 4) ? AbigailGame.playerShootingDirections.ElementAt(1) : AbigailGame.playerShootingDirections.ElementAt(0) }, this.playerPosition);
				}
				else
				{
					this.spawnBullets(AbigailGame.playerShootingDirections, this.playerPosition);
				}
				Game1.playSound("Cowboy_gunshot");
				this.shotTimer = this.shootingDelay;
				if (this.activePowerups.ContainsKey(3))
				{
					this.shotTimer /= 4;
				}
				for (int i8 = 0; i8 < this.fireSpeedLevel; i8++)
				{
					this.shotTimer = this.shotTimer * 3 / 4;
				}
				if (this.activePowerups.ContainsKey(7))
				{
					this.shotTimer = this.shotTimer * 3 / 2;
				}
				this.shotTimer = Math.Max(this.shotTimer, 20);
			}
			this.updateBullets(time);
			foreach (CowboyPowerup powerup in AbigailGame.powerups)
			{
				Vector2 tile_position = new Vector2((powerup.position.X + AbigailGame.TileSize / 2) / AbigailGame.TileSize, (powerup.position.Y + AbigailGame.TileSize / 2) / AbigailGame.TileSize);
				Vector2 corner_1 = new Vector2(powerup.position.X / AbigailGame.TileSize, powerup.position.Y / AbigailGame.TileSize);
				Vector2 corner_2 = new Vector2((powerup.position.X + AbigailGame.TileSize) / AbigailGame.TileSize, powerup.position.Y / AbigailGame.TileSize);
				Vector2 corner_3 = new Vector2(powerup.position.X / AbigailGame.TileSize, powerup.position.Y / AbigailGame.TileSize);
				Vector2 corner_4 = new Vector2(powerup.position.X / AbigailGame.TileSize, (powerup.position.Y + 64) / AbigailGame.TileSize);
				if (this._borderTiles.Contains(tile_position) || this._borderTiles.Contains(corner_1) || this._borderTiles.Contains(corner_2) || this._borderTiles.Contains(corner_3) || this._borderTiles.Contains(corner_4))
				{
					Point push_direction = default(Point);
					if (Math.Abs(tile_position.X - 8f) > Math.Abs(tile_position.Y - 8f))
					{
						push_direction.X = Math.Sign(tile_position.X - 8f);
					}
					else
					{
						push_direction.Y = Math.Sign(tile_position.Y - 8f);
					}
					powerup.position.X -= push_direction.X;
					powerup.position.Y -= push_direction.Y;
				}
			}
			if (AbigailGame.waveTimer > 0 && AbigailGame.betweenWaveTimer <= 0 && AbigailGame.zombieModeTimer <= 0 && !AbigailGame.shootoutLevel && (AbigailGame.overworldSong == null || !AbigailGame.overworldSong.IsPlaying))
			{
				Game1.playSound("Cowboy_OVERWORLD", out AbigailGame.overworldSong);
				Game1.musicPlayerVolume = Game1.options.musicVolumeLevel;
				Game1.musicCategory.SetVolume(Game1.musicPlayerVolume);
			}
			if (AbigailGame.deathTimer > 0f)
			{
				AbigailGame.deathTimer -= time.ElapsedGameTime.Milliseconds;
			}
			if (AbigailGame.betweenWaveTimer > 0 && AbigailGame.monsters.Count == 0 && this.isSpawnQueueEmpty() && !AbigailGame.shopping && !AbigailGame.waitingForPlayerToMoveDownAMap)
			{
				AbigailGame.betweenWaveTimer -= time.ElapsedGameTime.Milliseconds;
				if (AbigailGame.betweenWaveTimer <= 0 && AbigailGame.playingWithAbigail)
				{
					this.startAbigailPortrait(7, Game1.content.LoadString("Strings\\StringsFromCSFiles:AbigailGame.cs.11896"));
				}
			}
			else if (AbigailGame.deathTimer <= 0f && !AbigailGame.waitingForPlayerToMoveDownAMap && !AbigailGame.shopping && !AbigailGame.shootoutLevel)
			{
				if (AbigailGame.waveTimer > 0)
				{
					int oldWaveTimer = AbigailGame.waveTimer;
					AbigailGame.waveTimer -= time.ElapsedGameTime.Milliseconds;
					if (AbigailGame.playingWithAbigail && oldWaveTimer > 40000 && AbigailGame.waveTimer <= 40000)
					{
						this.startAbigailPortrait(0, Game1.content.LoadString("Strings\\StringsFromCSFiles:AbigailGame.cs.11897"));
					}
					int u = 0;
					foreach (Vector2 v in this.monsterChances)
					{
						if (Game1.random.NextDouble() < (double)(v.X * (float)((AbigailGame.monsters.Count != 0) ? 1 : 2)))
						{
							int numMonsters = 1;
							while (Game1.random.NextDouble() < (double)v.Y && numMonsters < 15)
							{
								numMonsters++;
							}
							this.spawnQueue[(AbigailGame.whichWave == 11) ? (Game1.random.Next(1, 3) * 2 - 1) : Game1.random.Next(4)].Add(new Point(u, numMonsters));
						}
						u++;
					}
					if (!AbigailGame.hasGopherAppeared && AbigailGame.monsters.Count > 6 && Game1.random.NextDouble() < 0.0004 && AbigailGame.waveTimer > 7000 && AbigailGame.waveTimer < 50000)
					{
						AbigailGame.hasGopherAppeared = true;
						AbigailGame.gopherBox = new Rectangle(Game1.random.Next(16 * AbigailGame.TileSize), Game1.random.Next(16 * AbigailGame.TileSize), AbigailGame.TileSize, AbigailGame.TileSize);
						int tries = 0;
						while ((AbigailGame.isCollidingWithMap(AbigailGame.gopherBox) || AbigailGame.isCollidingWithMonster(AbigailGame.gopherBox, null) || Math.Abs((float)AbigailGame.gopherBox.X - this.playerPosition.X) < (float)(AbigailGame.TileSize * 6) || Math.Abs((float)AbigailGame.gopherBox.Y - this.playerPosition.Y) < (float)(AbigailGame.TileSize * 6) || Math.Abs(AbigailGame.gopherBox.X - 8 * AbigailGame.TileSize) < AbigailGame.TileSize * 4 || Math.Abs(AbigailGame.gopherBox.Y - 8 * AbigailGame.TileSize) < AbigailGame.TileSize * 4) && tries < 10)
						{
							AbigailGame.gopherBox.X = Game1.random.Next(16 * AbigailGame.TileSize);
							AbigailGame.gopherBox.Y = Game1.random.Next(16 * AbigailGame.TileSize);
							tries++;
						}
						if (tries < 10)
						{
							AbigailGame.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(256, 1664, 16, 32), 80f, 5, 0, AbigailGame.topLeftScreenCoordinate + new Vector2(AbigailGame.gopherBox.X + AbigailGame.TileSize / 2, AbigailGame.gopherBox.Y - AbigailGame.TileSize + AbigailGame.TileSize / 2), flicker: false, flipped: false, (float)AbigailGame.gopherBox.Y / 10000f, 0f, Color.White, 3f, 0f, 0f, 0f, local: true)
							{
								endFunction = endOfGopherAnimationBehavior
							});
						}
					}
				}
				for (int p = 0; p < 4; p++)
				{
					if (this.spawnQueue[p].Count <= 0)
					{
						continue;
					}
					if (this.spawnQueue[p][0].X == 1 || this.spawnQueue[p][0].X == 4)
					{
						List<Vector2> border = Utility.getBorderOfThisRectangle(new Rectangle(0, 0, 16, 16));
						Vector2 tile = border.ElementAt(Game1.random.Next(border.Count));
						int tries2 = 0;
						while (AbigailGame.isCollidingWithMonster(new Rectangle((int)tile.X * AbigailGame.TileSize, (int)tile.Y * AbigailGame.TileSize, AbigailGame.TileSize, AbigailGame.TileSize), null) && tries2 < 10)
						{
							tile = border.ElementAt(Game1.random.Next(border.Count));
							tries2++;
						}
						if (tries2 < 10)
						{
							CowboyMonster monster = new CowboyMonster(this.spawnQueue[p][0].X, new Point((int)tile.X * AbigailGame.TileSize, (int)tile.Y * AbigailGame.TileSize));
							if (this.whichRound > 0)
							{
								monster.health += this.whichRound * 2;
							}
							AbigailGame.monsters.Add(monster);
							this.spawnQueue[p][0] = new Point(this.spawnQueue[p][0].X, this.spawnQueue[p][0].Y - 1);
							if (this.spawnQueue[p][0].Y <= 0)
							{
								this.spawnQueue[p].RemoveAt(0);
							}
						}
						continue;
					}
					switch (p)
					{
					case 0:
					{
						for (int x2 = 7; x2 < 10; x2++)
						{
							if (Game1.random.NextBool() && !AbigailGame.isCollidingWithMonster(new Rectangle(x2 * 16 * 3, 0, 48, 48), null))
							{
								CowboyMonster monster5 = new CowboyMonster(this.spawnQueue[p][0].X, new Point(x2 * AbigailGame.TileSize, 0));
								if (this.whichRound > 0)
								{
									monster5.health += this.whichRound * 2;
								}
								AbigailGame.monsters.Add(monster5);
								this.spawnQueue[p][0] = new Point(this.spawnQueue[p][0].X, this.spawnQueue[p][0].Y - 1);
								if (this.spawnQueue[p][0].Y <= 0)
								{
									this.spawnQueue[p].RemoveAt(0);
								}
								break;
							}
						}
						break;
					}
					case 1:
					{
						for (int y2 = 7; y2 < 10; y2++)
						{
							if (Game1.random.NextBool() && !AbigailGame.isCollidingWithMonster(new Rectangle(720, y2 * AbigailGame.TileSize, 48, 48), null))
							{
								CowboyMonster monster3 = new CowboyMonster(this.spawnQueue[p][0].X, new Point(15 * AbigailGame.TileSize, y2 * AbigailGame.TileSize));
								if (this.whichRound > 0)
								{
									monster3.health += this.whichRound * 2;
								}
								AbigailGame.monsters.Add(monster3);
								this.spawnQueue[p][0] = new Point(this.spawnQueue[p][0].X, this.spawnQueue[p][0].Y - 1);
								if (this.spawnQueue[p][0].Y <= 0)
								{
									this.spawnQueue[p].RemoveAt(0);
								}
								break;
							}
						}
						break;
					}
					case 2:
					{
						for (int x = 7; x < 10; x++)
						{
							if (Game1.random.NextBool() && !AbigailGame.isCollidingWithMonster(new Rectangle(x * 16 * 3, 15 * AbigailGame.TileSize, 48, 48), null))
							{
								CowboyMonster monster4 = new CowboyMonster(this.spawnQueue[p][0].X, new Point(x * AbigailGame.TileSize, 15 * AbigailGame.TileSize));
								if (this.whichRound > 0)
								{
									monster4.health += this.whichRound * 2;
								}
								AbigailGame.monsters.Add(monster4);
								this.spawnQueue[p][0] = new Point(this.spawnQueue[p][0].X, this.spawnQueue[p][0].Y - 1);
								if (this.spawnQueue[p][0].Y <= 0)
								{
									this.spawnQueue[p].RemoveAt(0);
								}
								break;
							}
						}
						break;
					}
					case 3:
					{
						for (int y = 7; y < 10; y++)
						{
							if (Game1.random.NextBool() && !AbigailGame.isCollidingWithMonster(new Rectangle(0, y * AbigailGame.TileSize, 48, 48), null))
							{
								CowboyMonster monster2 = new CowboyMonster(this.spawnQueue[p][0].X, new Point(0, y * AbigailGame.TileSize));
								if (this.whichRound > 0)
								{
									monster2.health += this.whichRound * 2;
								}
								AbigailGame.monsters.Add(monster2);
								this.spawnQueue[p][0] = new Point(this.spawnQueue[p][0].X, this.spawnQueue[p][0].Y - 1);
								if (this.spawnQueue[p][0].Y <= 0)
								{
									this.spawnQueue[p].RemoveAt(0);
								}
								break;
							}
						}
						break;
					}
					}
				}
				if (AbigailGame.waveTimer <= 0 && AbigailGame.monsters.Count > 0 && this.isSpawnQueueEmpty())
				{
					bool onlySpikeys = true;
					foreach (CowboyMonster monster6 in AbigailGame.monsters)
					{
						if (monster6.type != 6)
						{
							onlySpikeys = false;
							break;
						}
					}
					if (onlySpikeys)
					{
						foreach (CowboyMonster monster7 in AbigailGame.monsters)
						{
							monster7.health = 1;
						}
					}
				}
				if (AbigailGame.waveTimer <= 0 && AbigailGame.monsters.Count == 0 && this.isSpawnQueueEmpty())
				{
					AbigailGame.hasGopherAppeared = false;
					if (AbigailGame.playingWithAbigail)
					{
						this.startAbigailPortrait(1, Game1.content.LoadString("Strings\\StringsFromCSFiles:AbigailGame.cs.11898"));
					}
					AbigailGame.waveTimer = 80000;
					AbigailGame.betweenWaveTimer = 3333;
					AbigailGame.whichWave++;
					if (AbigailGame.playingWithAbigail)
					{
						AbigailGame.beatLevelWithAbigail = true;
						this.fadethenQuitTimer = 2000;
					}
					switch (AbigailGame.whichWave)
					{
					case 1:
					case 2:
					case 3:
						this.monsterChances[0] = new Vector2(this.monsterChances[0].X + 0.001f, this.monsterChances[0].Y + 0.02f);
						if (AbigailGame.whichWave > 1)
						{
							this.monsterChances[2] = new Vector2(this.monsterChances[2].X + 0.001f, this.monsterChances[2].Y + 0.01f);
						}
						this.monsterChances[6] = new Vector2(this.monsterChances[6].X + 0.001f, this.monsterChances[6].Y + 0.01f);
						if (this.whichRound > 0)
						{
							this.monsterChances[4] = new Vector2(0.002f, 0.1f);
						}
						break;
					case 4:
					case 5:
					case 6:
					case 7:
						if (this.monsterChances[5].Equals(Vector2.Zero))
						{
							this.monsterChances[5] = new Vector2(0.01f, 0.15f);
							if (this.whichRound > 0)
							{
								this.monsterChances[5] = new Vector2(0.01f + (float)this.whichRound * 0.004f, 0.15f + (float)this.whichRound * 0.04f);
							}
						}
						this.monsterChances[0] = Vector2.Zero;
						this.monsterChances[6] = Vector2.Zero;
						this.monsterChances[2] = new Vector2(this.monsterChances[2].X + 0.002f, this.monsterChances[2].Y + 0.02f);
						this.monsterChances[5] = new Vector2(this.monsterChances[5].X + 0.001f, this.monsterChances[5].Y + 0.02f);
						this.monsterChances[1] = new Vector2(this.monsterChances[1].X + 0.0018f, this.monsterChances[1].Y + 0.08f);
						if (this.whichRound > 0)
						{
							this.monsterChances[4] = new Vector2(0.001f, 0.1f);
						}
						break;
					case 8:
					case 9:
					case 10:
					case 11:
						this.monsterChances[5] = Vector2.Zero;
						this.monsterChances[1] = Vector2.Zero;
						this.monsterChances[2] = Vector2.Zero;
						if (this.monsterChances[3].Equals(Vector2.Zero))
						{
							this.monsterChances[3] = new Vector2(0.012f, 0.4f);
							if (this.whichRound > 0)
							{
								this.monsterChances[3] = new Vector2(0.012f + (float)this.whichRound * 0.005f, 0.4f + (float)this.whichRound * 0.075f);
							}
						}
						if (this.monsterChances[4].Equals(Vector2.Zero))
						{
							this.monsterChances[4] = new Vector2(0.003f, 0.1f);
						}
						this.monsterChances[3] = new Vector2(this.monsterChances[3].X + 0.002f, this.monsterChances[3].Y + 0.05f);
						this.monsterChances[4] = new Vector2(this.monsterChances[4].X + 0.0015f, this.monsterChances[4].Y + 0.04f);
						if (AbigailGame.whichWave == 11)
						{
							this.monsterChances[4] = new Vector2(this.monsterChances[4].X + 0.01f, this.monsterChances[4].Y + 0.04f);
							this.monsterChances[3] = new Vector2(this.monsterChances[3].X - 0.01f, this.monsterChances[3].Y + 0.04f);
						}
						break;
					}
					if (this.whichRound > 0)
					{
						for (int i9 = 0; i9 < this.monsterChances.Count; i9++)
						{
							_ = this.monsterChances[i9];
							this.monsterChances[i9] *= 1.1f;
						}
					}
					if (AbigailGame.whichWave > 0 && AbigailGame.whichWave % 2 == 0)
					{
						this.startShoppingLevel();
					}
					else if (AbigailGame.whichWave > 0)
					{
						AbigailGame.waitingForPlayerToMoveDownAMap = true;
						if (!AbigailGame.playingWithAbigail)
						{
							AbigailGame.map[8, 15] = 3;
							AbigailGame.map[7, 15] = 3;
							AbigailGame.map[9, 15] = 3;
						}
					}
				}
			}
			if (AbigailGame.playingWithAbigail)
			{
				this.updateAbigail(time);
			}
			for (int i10 = AbigailGame.monsters.Count - 1; i10 >= 0; i10--)
			{
				AbigailGame.monsters[i10].move(this.playerPosition, time);
				if (i10 < AbigailGame.monsters.Count && AbigailGame.monsters[i10].position.Intersects(this.playerBoundingBox) && AbigailGame.playerInvincibleTimer <= 0)
				{
					if (AbigailGame.zombieModeTimer <= 0)
					{
						this.playerDie();
						break;
					}
					if (AbigailGame.monsters[i10].type != -2)
					{
						AbigailGame.addGuts(AbigailGame.monsters[i10].position.Location, AbigailGame.monsters[i10].type);
						AbigailGame.monsters.RemoveAt(i10);
						Game1.playSound("Cowboy_monsterDie");
					}
				}
				if (AbigailGame.playingWithAbigail && i10 < AbigailGame.monsters.Count && AbigailGame.monsters[i10].position.Intersects(this.player2BoundingBox) && this.player2invincibletimer <= 0)
				{
					Game1.playSound("Cowboy_monsterDie");
					this.player2deathtimer = 3000;
					AbigailGame.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(464, 1808, 16, 16), 120f, 5, 0, AbigailGame.player2Position + AbigailGame.topLeftScreenCoordinate + new Vector2(AbigailGame.TileSize / 2, AbigailGame.TileSize / 2), flicker: false, flipped: false, 1f, 0f, Color.White, 3f, 0f, 0f, 0f, local: true));
					this.player2invincibletimer = 4000;
					AbigailGame.player2Position = new Vector2(8f, 8f) * AbigailGame.TileSize;
					this.player2BoundingBox.X = (int)AbigailGame.player2Position.X + AbigailGame.TileSize / 4;
					this.player2BoundingBox.Y = (int)AbigailGame.player2Position.Y + AbigailGame.TileSize / 4;
					this.player2BoundingBox.Width = AbigailGame.TileSize / 2;
					this.player2BoundingBox.Height = AbigailGame.TileSize / 2;
					if (this.playerBoundingBox.Intersects(this.player2BoundingBox))
					{
						AbigailGame.player2Position.X = this.playerBoundingBox.Right + 2;
					}
					this.player2BoundingBox.X = (int)AbigailGame.player2Position.X + AbigailGame.TileSize / 4;
					this.player2BoundingBox.Y = (int)AbigailGame.player2Position.Y + AbigailGame.TileSize / 4;
					this.player2BoundingBox.Width = AbigailGame.TileSize / 2;
					this.player2BoundingBox.Height = AbigailGame.TileSize / 2;
					this.startAbigailPortrait(5, Game1.random.NextBool() ? Game1.content.LoadString("Strings\\StringsFromCSFiles:AbigailGame.cs.11901") : Game1.content.LoadString("Strings\\StringsFromCSFiles:AbigailGame.cs.11902"));
				}
			}
		}
		return false;
	}

	protected void _ProcessInputs()
	{
		if (this._buttonHeldFrames[GameKeys.MoveUp] > 0)
		{
			if (this._buttonHeldFrames[GameKeys.MoveUp] == 1 && AbigailGame.gameOver)
			{
				this.gameOverOption = Math.Max(0, this.gameOverOption - 1);
				Game1.playSound("Cowboy_gunshot");
			}
			this.addPlayerMovementDirection(0);
		}
		else if (AbigailGame.playerMovementDirections.Contains(0))
		{
			AbigailGame.playerMovementDirections.Remove(0);
		}
		if (this._buttonHeldFrames[GameKeys.MoveDown] > 0)
		{
			if (this._buttonHeldFrames[GameKeys.MoveDown] == 1 && AbigailGame.gameOver)
			{
				this.gameOverOption = Math.Min(1, this.gameOverOption + 1);
				Game1.playSound("Cowboy_gunshot");
			}
			this.addPlayerMovementDirection(2);
		}
		else if (AbigailGame.playerMovementDirections.Contains(2))
		{
			AbigailGame.playerMovementDirections.Remove(2);
		}
		if (this._buttonHeldFrames[GameKeys.MoveLeft] > 0)
		{
			this.addPlayerMovementDirection(3);
		}
		else if (AbigailGame.playerMovementDirections.Contains(3))
		{
			AbigailGame.playerMovementDirections.Remove(3);
		}
		if (this._buttonHeldFrames[GameKeys.MoveRight] > 0)
		{
			this.addPlayerMovementDirection(1);
		}
		else if (AbigailGame.playerMovementDirections.Contains(1))
		{
			AbigailGame.playerMovementDirections.Remove(1);
		}
		if (this._buttonHeldFrames[GameKeys.ShootUp] > 0)
		{
			this.addPlayerShootingDirection(0);
		}
		else if (AbigailGame.playerShootingDirections.Contains(0))
		{
			AbigailGame.playerShootingDirections.Remove(0);
		}
		if (this._buttonHeldFrames[GameKeys.ShootDown] > 0)
		{
			this.addPlayerShootingDirection(2);
		}
		else if (AbigailGame.playerShootingDirections.Contains(2))
		{
			AbigailGame.playerShootingDirections.Remove(2);
		}
		if (this._buttonHeldFrames[GameKeys.ShootLeft] > 0)
		{
			this.addPlayerShootingDirection(3);
		}
		else if (AbigailGame.playerShootingDirections.Contains(3))
		{
			AbigailGame.playerShootingDirections.Remove(3);
		}
		if (this._buttonHeldFrames[GameKeys.ShootRight] > 0)
		{
			this.addPlayerShootingDirection(1);
		}
		else if (AbigailGame.playerShootingDirections.Contains(1))
		{
			AbigailGame.playerShootingDirections.Remove(1);
		}
		if (this._buttonHeldFrames[GameKeys.SelectOption] == 1 && AbigailGame.gameOver)
		{
			if (this.gameOverOption == 1)
			{
				this.quit = true;
			}
			else
			{
				this.gamerestartTimer = 1500;
				AbigailGame.gameOver = false;
				this.gameOverOption = 0;
				Game1.playSound("Pickup_Coin15");
			}
		}
		if (this._buttonHeldFrames[GameKeys.UsePowerup] == 1 && !AbigailGame.gameOver && this.heldItem != null && AbigailGame.deathTimer <= 0f && AbigailGame.zombieModeTimer <= 0)
		{
			this.usePowerup(this.heldItem.which);
			this.heldItem = null;
		}
		if (this._buttonHeldFrames[GameKeys.Exit] == 1 && !AbigailGame.playingWithAbigail)
		{
			this.quit = true;
		}
	}

	public virtual void ApplyLevelSpecificStates()
	{
		if (AbigailGame.whichWave == 12)
		{
			AbigailGame.shootoutLevel = true;
			Dracula monster = new Dracula();
			if (this.whichRound > 0)
			{
				monster.health *= 2;
			}
			AbigailGame.monsters.Add(monster);
		}
		else if (AbigailGame.whichWave > 0 && AbigailGame.whichWave % 4 == 0)
		{
			AbigailGame.shootoutLevel = true;
			AbigailGame.monsters.Add(new Outlaw(new Point(8 * AbigailGame.TileSize, 13 * AbigailGame.TileSize), (AbigailGame.world == 0) ? 50 : 100));
			Game1.playSound("cowboy_outlawsong", out AbigailGame.outlawSong);
		}
	}

	public void updateAbigail(GameTime time)
	{
		this.player2TargetUpdateTimer -= time.ElapsedGameTime.Milliseconds;
		if (this.player2deathtimer > 0)
		{
			this.player2deathtimer -= time.ElapsedGameTime.Milliseconds;
		}
		if (this.player2invincibletimer > 0)
		{
			this.player2invincibletimer -= time.ElapsedGameTime.Milliseconds;
		}
		if (this.player2deathtimer > 0)
		{
			return;
		}
		if (this.player2TargetUpdateTimer < 0)
		{
			this.player2TargetUpdateTimer = 500;
			CowboyMonster closest = null;
			double closestDistance = 99999.0;
			foreach (CowboyMonster m in AbigailGame.monsters)
			{
				double distance = Math.Sqrt(Math.Pow((float)m.position.X - AbigailGame.player2Position.X, 2.0) - Math.Pow((float)m.position.Y - AbigailGame.player2Position.Y, 2.0));
				if (closest == null || distance < closestDistance)
				{
					closest = m;
					closestDistance = Math.Sqrt(Math.Pow((float)closest.position.X - AbigailGame.player2Position.X, 2.0) - Math.Pow((float)closest.position.Y - AbigailGame.player2Position.Y, 2.0));
				}
			}
			this.targetMonster = closest;
		}
		this.player2ShootingDirections.Clear();
		this.player2MovementDirections.Clear();
		if (this.targetMonster != null)
		{
			if (Math.Sqrt(Math.Pow((float)this.targetMonster.position.X - AbigailGame.player2Position.X, 2.0) - Math.Pow((float)this.targetMonster.position.Y - AbigailGame.player2Position.Y, 2.0)) < (double)(AbigailGame.TileSize * 3))
			{
				if ((float)this.targetMonster.position.X > AbigailGame.player2Position.X)
				{
					this.addPlayer2MovementDirection(3);
				}
				else if ((float)this.targetMonster.position.X < AbigailGame.player2Position.X)
				{
					this.addPlayer2MovementDirection(1);
				}
				if ((float)this.targetMonster.position.Y > AbigailGame.player2Position.Y)
				{
					this.addPlayer2MovementDirection(0);
				}
				else if ((float)this.targetMonster.position.Y < AbigailGame.player2Position.Y)
				{
					this.addPlayer2MovementDirection(2);
				}
				foreach (int i in this.player2MovementDirections)
				{
					this.player2ShootingDirections.Add((i + 2) % 4);
				}
			}
			else
			{
				if (Math.Abs((float)this.targetMonster.position.X - AbigailGame.player2Position.X) > Math.Abs((float)this.targetMonster.position.Y - AbigailGame.player2Position.Y) && Math.Abs((float)this.targetMonster.position.Y - AbigailGame.player2Position.Y) > 4f)
				{
					if ((float)this.targetMonster.position.Y > AbigailGame.player2Position.Y + 3f)
					{
						this.addPlayer2MovementDirection(2);
					}
					else if ((float)this.targetMonster.position.Y < AbigailGame.player2Position.Y - 3f)
					{
						this.addPlayer2MovementDirection(0);
					}
				}
				else if (Math.Abs((float)this.targetMonster.position.X - AbigailGame.player2Position.X) > 4f)
				{
					if ((float)this.targetMonster.position.X > AbigailGame.player2Position.X + 3f)
					{
						this.addPlayer2MovementDirection(1);
					}
					else if ((float)this.targetMonster.position.X < AbigailGame.player2Position.X - 3f)
					{
						this.addPlayer2MovementDirection(3);
					}
				}
				if ((float)this.targetMonster.position.X > AbigailGame.player2Position.X + 3f)
				{
					this.addPlayer2ShootingDirection(1);
				}
				else if ((float)this.targetMonster.position.X < AbigailGame.player2Position.X - 3f)
				{
					this.addPlayer2ShootingDirection(3);
				}
				if ((float)this.targetMonster.position.Y > AbigailGame.player2Position.Y + 3f)
				{
					this.addPlayer2ShootingDirection(2);
				}
				else if ((float)this.targetMonster.position.Y < AbigailGame.player2Position.Y - 3f)
				{
					this.addPlayer2ShootingDirection(0);
				}
			}
		}
		if (this.player2MovementDirections.Count > 0)
		{
			float speed = this.getMovementSpeed(3f, this.player2MovementDirections.Count);
			for (int j = 0; j < this.player2MovementDirections.Count; j++)
			{
				Vector2 newPlayerPosition = AbigailGame.player2Position;
				switch (this.player2MovementDirections[j])
				{
				case 0:
					newPlayerPosition.Y -= speed;
					break;
				case 3:
					newPlayerPosition.X -= speed;
					break;
				case 2:
					newPlayerPosition.Y += speed;
					break;
				case 1:
					newPlayerPosition.X += speed;
					break;
				}
				Rectangle newPlayerBox = new Rectangle((int)newPlayerPosition.X + AbigailGame.TileSize / 4, (int)newPlayerPosition.Y + AbigailGame.TileSize / 4, AbigailGame.TileSize / 2, AbigailGame.TileSize / 2);
				if (!AbigailGame.isCollidingWithMap(newPlayerBox) && (!this.merchantBox.Intersects(newPlayerBox) || this.merchantBox.Intersects(this.player2BoundingBox)) && !newPlayerBox.Intersects(this.playerBoundingBox))
				{
					AbigailGame.player2Position = newPlayerPosition;
				}
			}
			this.player2BoundingBox.X = (int)AbigailGame.player2Position.X + AbigailGame.TileSize / 4;
			this.player2BoundingBox.Y = (int)AbigailGame.player2Position.Y + AbigailGame.TileSize / 4;
			this.player2BoundingBox.Width = AbigailGame.TileSize / 2;
			this.player2BoundingBox.Height = AbigailGame.TileSize / 2;
			this.player2AnimationTimer += time.ElapsedGameTime.Milliseconds;
			this.player2AnimationTimer %= 400;
			this.player2FootstepSoundTimer -= time.ElapsedGameTime.Milliseconds;
			if (this.player2FootstepSoundTimer <= 0)
			{
				Game1.playSound("Cowboy_Footstep");
				this.player2FootstepSoundTimer = 200;
			}
			AbigailGame.powerups.RemoveAll((CowboyPowerup item) => this.player2BoundingBox.Intersects(new Rectangle(item.position.X, item.position.Y, AbigailGame.TileSize, AbigailGame.TileSize)) && !this.player2BoundingBox.Intersects(this.noPickUpBox));
		}
		this.player2shotTimer -= time.ElapsedGameTime.Milliseconds;
		if (this.player2ShootingDirections.Count > 0 && this.player2shotTimer <= 0)
		{
			if (this.player2ShootingDirections.Count == 1)
			{
				this.spawnBullets(new int[1] { this.player2ShootingDirections[0] }, AbigailGame.player2Position);
			}
			else
			{
				this.spawnBullets(this.player2ShootingDirections, AbigailGame.player2Position);
			}
			Game1.playSound("Cowboy_gunshot");
			this.player2shotTimer = this.shootingDelay;
		}
	}

	public int[,] getMap(int wave)
	{
		int[,] newMap = new int[16, 16];
		for (int i = 0; i < 16; i++)
		{
			for (int j = 0; j < 16; j++)
			{
				if ((i == 0 || i == 15 || j == 0 || j == 15) && (i <= 6 || i >= 10) && (j <= 6 || j >= 10))
				{
					newMap[i, j] = 5;
				}
				else if (i == 0 || i == 15 || j == 0 || j == 15)
				{
					newMap[i, j] = ((Game1.random.NextDouble() < 0.15) ? 1 : 0);
				}
				else if (i == 1 || i == 14 || j == 1 || j == 14)
				{
					newMap[i, j] = 2;
				}
				else
				{
					newMap[i, j] = ((Game1.random.NextDouble() < 0.1) ? 4 : 3);
				}
			}
		}
		switch (wave)
		{
		case -1:
		{
			for (int num = 0; num < 16; num++)
			{
				for (int num2 = 0; num2 < 16; num2++)
				{
					if (newMap[num, num2] == 0 || newMap[num, num2] == 1 || newMap[num, num2] == 2 || newMap[num, num2] == 5)
					{
						newMap[num, num2] = 3;
					}
				}
			}
			newMap[3, 1] = 5;
			newMap[8, 2] = 5;
			newMap[13, 1] = 5;
			newMap[5, 0] = 0;
			newMap[10, 2] = 2;
			newMap[15, 2] = 1;
			newMap[14, 12] = 5;
			newMap[10, 6] = 7;
			newMap[11, 6] = 7;
			newMap[12, 6] = 7;
			newMap[13, 6] = 7;
			newMap[14, 6] = 7;
			newMap[14, 7] = 7;
			newMap[14, 8] = 7;
			newMap[14, 9] = 7;
			newMap[14, 10] = 7;
			newMap[14, 11] = 7;
			newMap[14, 12] = 7;
			newMap[14, 13] = 7;
			for (int num3 = 0; num3 < 16; num3++)
			{
				newMap[num3, 3] = ((num3 % 2 == 0) ? 9 : 8);
			}
			newMap[3, 3] = 10;
			newMap[7, 8] = 2;
			newMap[8, 8] = 2;
			newMap[4, 11] = 2;
			newMap[11, 12] = 2;
			newMap[9, 11] = 2;
			newMap[3, 9] = 2;
			newMap[2, 12] = 5;
			newMap[8, 13] = 5;
			newMap[12, 11] = 5;
			newMap[7, 14] = 0;
			newMap[6, 14] = 2;
			newMap[8, 14] = 2;
			newMap[7, 13] = 2;
			newMap[7, 15] = 2;
			break;
		}
		case 1:
			newMap[4, 4] = 7;
			newMap[4, 5] = 7;
			newMap[5, 4] = 7;
			newMap[12, 4] = 7;
			newMap[11, 4] = 7;
			newMap[12, 5] = 7;
			newMap[4, 12] = 7;
			newMap[5, 12] = 7;
			newMap[4, 11] = 7;
			newMap[12, 12] = 7;
			newMap[11, 12] = 7;
			newMap[12, 11] = 7;
			break;
		case 2:
			newMap[8, 4] = 7;
			newMap[12, 8] = 7;
			newMap[8, 12] = 7;
			newMap[4, 8] = 7;
			newMap[1, 1] = 5;
			newMap[14, 1] = 5;
			newMap[14, 14] = 5;
			newMap[1, 14] = 5;
			newMap[2, 1] = 5;
			newMap[13, 1] = 5;
			newMap[13, 14] = 5;
			newMap[2, 14] = 5;
			newMap[1, 2] = 5;
			newMap[14, 2] = 5;
			newMap[14, 13] = 5;
			newMap[1, 13] = 5;
			break;
		case 3:
			newMap[5, 5] = 7;
			newMap[6, 5] = 7;
			newMap[7, 5] = 7;
			newMap[9, 5] = 7;
			newMap[10, 5] = 7;
			newMap[11, 5] = 7;
			newMap[5, 11] = 7;
			newMap[6, 11] = 7;
			newMap[7, 11] = 7;
			newMap[9, 11] = 7;
			newMap[10, 11] = 7;
			newMap[11, 11] = 7;
			newMap[5, 6] = 7;
			newMap[5, 7] = 7;
			newMap[5, 9] = 7;
			newMap[5, 10] = 7;
			newMap[11, 6] = 7;
			newMap[11, 7] = 7;
			newMap[11, 9] = 7;
			newMap[11, 10] = 7;
			break;
		case 4:
		case 8:
		{
			for (int num5 = 0; num5 < 16; num5++)
			{
				for (int num6 = 0; num6 < 16; num6++)
				{
					if (newMap[num5, num6] == 5)
					{
						newMap[num5, num6] = Game1.random.Choose(0, 1);
					}
				}
			}
			for (int num7 = 0; num7 < 16; num7++)
			{
				newMap[num7, 8] = Game1.random.Choose(8, 9);
			}
			newMap[8, 4] = 7;
			newMap[8, 12] = 7;
			newMap[9, 12] = 7;
			newMap[7, 12] = 7;
			newMap[5, 6] = 5;
			newMap[10, 6] = 5;
			break;
		}
		case 5:
			newMap[1, 1] = 5;
			newMap[14, 1] = 5;
			newMap[14, 14] = 5;
			newMap[1, 14] = 5;
			newMap[2, 1] = 5;
			newMap[13, 1] = 5;
			newMap[13, 14] = 5;
			newMap[2, 14] = 5;
			newMap[1, 2] = 5;
			newMap[14, 2] = 5;
			newMap[14, 13] = 5;
			newMap[1, 13] = 5;
			newMap[3, 1] = 5;
			newMap[13, 1] = 5;
			newMap[13, 13] = 5;
			newMap[1, 13] = 5;
			newMap[1, 3] = 5;
			newMap[13, 3] = 5;
			newMap[12, 13] = 5;
			newMap[3, 14] = 5;
			newMap[3, 3] = 5;
			newMap[13, 12] = 5;
			newMap[13, 12] = 5;
			newMap[3, 12] = 5;
			break;
		case 6:
			newMap[4, 5] = 2;
			newMap[12, 10] = 5;
			newMap[10, 9] = 5;
			newMap[5, 12] = 2;
			newMap[5, 9] = 5;
			newMap[12, 12] = 5;
			newMap[3, 4] = 5;
			newMap[2, 3] = 5;
			newMap[11, 3] = 5;
			newMap[10, 6] = 5;
			newMap[5, 9] = 7;
			newMap[10, 12] = 7;
			newMap[3, 12] = 7;
			newMap[10, 8] = 7;
			break;
		case 7:
		{
			for (int n = 0; n < 16; n++)
			{
				newMap[n, 5] = ((n % 2 == 0) ? 9 : 8);
				newMap[n, 10] = ((n % 2 == 0) ? 9 : 8);
			}
			newMap[4, 5] = 10;
			newMap[8, 5] = 10;
			newMap[12, 5] = 10;
			newMap[4, 10] = 10;
			newMap[8, 10] = 10;
			newMap[12, 10] = 10;
			break;
		}
		case 9:
			newMap[4, 4] = 5;
			newMap[5, 4] = 5;
			newMap[10, 4] = 5;
			newMap[12, 4] = 5;
			newMap[4, 5] = 5;
			newMap[5, 5] = 5;
			newMap[10, 5] = 5;
			newMap[12, 5] = 5;
			newMap[4, 10] = 5;
			newMap[5, 10] = 5;
			newMap[10, 10] = 5;
			newMap[12, 10] = 5;
			newMap[4, 12] = 5;
			newMap[5, 12] = 5;
			newMap[10, 12] = 5;
			newMap[12, 12] = 5;
			break;
		case 10:
		{
			for (int num8 = 0; num8 < 16; num8++)
			{
				newMap[num8, 1] = ((num8 % 2 == 0) ? 9 : 8);
				newMap[num8, 14] = ((num8 % 2 == 0) ? 9 : 8);
			}
			newMap[8, 1] = 10;
			newMap[7, 1] = 10;
			newMap[9, 1] = 10;
			newMap[8, 14] = 10;
			newMap[7, 14] = 10;
			newMap[9, 14] = 10;
			newMap[6, 8] = 5;
			newMap[10, 8] = 5;
			newMap[8, 6] = 5;
			newMap[8, 9] = 5;
			break;
		}
		case 11:
		{
			for (int num4 = 0; num4 < 16; num4++)
			{
				newMap[num4, 0] = 7;
				newMap[num4, 15] = 7;
				if (num4 % 2 == 0)
				{
					newMap[num4, 1] = 5;
					newMap[num4, 14] = 5;
				}
			}
			break;
		}
		case 12:
		{
			for (int k = 0; k < 16; k++)
			{
				for (int l = 0; l < 16; l++)
				{
					if (newMap[k, l] == 0 || newMap[k, l] == 1)
					{
						newMap[k, l] = 5;
					}
				}
			}
			for (int m = 0; m < 16; m++)
			{
				newMap[m, 0] = ((m % 2 == 0) ? 9 : 8);
				newMap[m, 15] = ((m % 2 == 0) ? 9 : 8);
			}
			Rectangle r = new Rectangle(1, 1, 14, 14);
			foreach (Vector2 v in Utility.getBorderOfThisRectangle(r))
			{
				newMap[(int)v.X, (int)v.Y] = 10;
			}
			r.Inflate(-1, -1);
			foreach (Vector2 v2 in Utility.getBorderOfThisRectangle(r))
			{
				newMap[(int)v2.X, (int)v2.Y] = 2;
			}
			break;
		}
		default:
			newMap[4, 4] = 5;
			newMap[12, 4] = 5;
			newMap[4, 12] = 5;
			newMap[12, 12] = 5;
			break;
		}
		return newMap;
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

	public void spawnBullets(IList<int> directions, Vector2 spawn)
	{
		Point bulletSpawn = new Point((int)spawn.X + 24, (int)spawn.Y + 24 - 6);
		int speed = (int)this.getMovementSpeed(8f, 2);
		if (directions.Count == 1)
		{
			int playerShootingDirection = directions[0];
			switch (playerShootingDirection)
			{
			case 0:
				bulletSpawn.Y -= 22;
				break;
			case 1:
				bulletSpawn.X += 16;
				bulletSpawn.Y -= 6;
				break;
			case 2:
				bulletSpawn.Y += 10;
				break;
			case 3:
				bulletSpawn.X -= 16;
				bulletSpawn.Y -= 6;
				break;
			}
			this.bullets.Add(new CowboyBullet(bulletSpawn, playerShootingDirection, this.bulletDamage));
			if (this.activePowerups.ContainsKey(7) || this.spreadPistol)
			{
				switch (playerShootingDirection)
				{
				case 0:
					this.bullets.Add(new CowboyBullet(new Point(bulletSpawn.X, bulletSpawn.Y), new Point(-2, -8), this.bulletDamage));
					this.bullets.Add(new CowboyBullet(new Point(bulletSpawn.X, bulletSpawn.Y), new Point(2, -8), this.bulletDamage));
					break;
				case 1:
					this.bullets.Add(new CowboyBullet(new Point(bulletSpawn.X, bulletSpawn.Y), new Point(8, -2), this.bulletDamage));
					this.bullets.Add(new CowboyBullet(new Point(bulletSpawn.X, bulletSpawn.Y), new Point(8, 2), this.bulletDamage));
					break;
				case 2:
					this.bullets.Add(new CowboyBullet(new Point(bulletSpawn.X, bulletSpawn.Y), new Point(-2, 8), this.bulletDamage));
					this.bullets.Add(new CowboyBullet(new Point(bulletSpawn.X, bulletSpawn.Y), new Point(2, 8), this.bulletDamage));
					break;
				case 3:
					this.bullets.Add(new CowboyBullet(new Point(bulletSpawn.X, bulletSpawn.Y), new Point(-8, -2), this.bulletDamage));
					this.bullets.Add(new CowboyBullet(new Point(bulletSpawn.X, bulletSpawn.Y), new Point(-8, 2), this.bulletDamage));
					break;
				}
			}
		}
		else if (directions.Contains(0) && directions.Contains(1))
		{
			bulletSpawn.X += AbigailGame.TileSize / 2;
			bulletSpawn.Y -= AbigailGame.TileSize / 2;
			this.bullets.Add(new CowboyBullet(bulletSpawn, new Point(speed, -speed), this.bulletDamage));
			if (this.activePowerups.ContainsKey(7) || this.spreadPistol)
			{
				int modifier = -2;
				this.bullets.Add(new CowboyBullet(bulletSpawn, new Point(speed + modifier, -speed + modifier), this.bulletDamage));
				modifier = 2;
				this.bullets.Add(new CowboyBullet(bulletSpawn, new Point(speed + modifier, -speed + modifier), this.bulletDamage));
			}
		}
		else if (directions.Contains(0) && directions.Contains(3))
		{
			bulletSpawn.X -= AbigailGame.TileSize / 2;
			bulletSpawn.Y -= AbigailGame.TileSize / 2;
			this.bullets.Add(new CowboyBullet(bulletSpawn, new Point(-speed, -speed), this.bulletDamage));
			if (this.activePowerups.ContainsKey(7) || this.spreadPistol)
			{
				int modifier2 = -2;
				this.bullets.Add(new CowboyBullet(bulletSpawn, new Point(-speed - modifier2, -speed + modifier2), this.bulletDamage));
				modifier2 = 2;
				this.bullets.Add(new CowboyBullet(bulletSpawn, new Point(-speed - modifier2, -speed + modifier2), this.bulletDamage));
			}
		}
		else if (directions.Contains(2) && directions.Contains(1))
		{
			bulletSpawn.X += AbigailGame.TileSize / 2;
			bulletSpawn.Y += AbigailGame.TileSize / 4;
			this.bullets.Add(new CowboyBullet(bulletSpawn, new Point(speed, speed), this.bulletDamage));
			if (this.activePowerups.ContainsKey(7) || this.spreadPistol)
			{
				int modifier3 = -2;
				this.bullets.Add(new CowboyBullet(bulletSpawn, new Point(speed - modifier3, speed + modifier3), this.bulletDamage));
				modifier3 = 2;
				this.bullets.Add(new CowboyBullet(bulletSpawn, new Point(speed - modifier3, speed + modifier3), this.bulletDamage));
			}
		}
		else if (directions.Contains(2) && directions.Contains(3))
		{
			bulletSpawn.X -= AbigailGame.TileSize / 2;
			bulletSpawn.Y += AbigailGame.TileSize / 4;
			this.bullets.Add(new CowboyBullet(bulletSpawn, new Point(-speed, speed), this.bulletDamage));
			if (this.activePowerups.ContainsKey(7) || this.spreadPistol)
			{
				int modifier4 = -2;
				this.bullets.Add(new CowboyBullet(bulletSpawn, new Point(-speed + modifier4, speed + modifier4), this.bulletDamage));
				modifier4 = 2;
				this.bullets.Add(new CowboyBullet(bulletSpawn, new Point(-speed + modifier4, speed + modifier4), this.bulletDamage));
			}
		}
	}

	public bool isSpawnQueueEmpty()
	{
		for (int i = 0; i < 4; i++)
		{
			if (this.spawnQueue[i].Count > 0)
			{
				return false;
			}
		}
		return true;
	}

	public static bool isMapTilePassable(int tileType)
	{
		if ((uint)tileType <= 1u || (uint)(tileType - 5) <= 4u)
		{
			return false;
		}
		return true;
	}

	public static bool isMapTilePassableForMonsters(int tileType)
	{
		if (tileType == 5 || (uint)(tileType - 7) <= 2u)
		{
			return false;
		}
		return true;
	}

	public static bool isCollidingWithMonster(Rectangle r, CowboyMonster subject)
	{
		foreach (CowboyMonster c in AbigailGame.monsters)
		{
			if ((subject == null || !subject.Equals(c)) && Math.Abs(c.position.X - r.X) < 48 && Math.Abs(c.position.Y - r.Y) < 48 && r.Intersects(new Rectangle(c.position.X, c.position.Y, 48, 48)))
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// For monsters
	/// </summary>
	/// <param name="positionToCheck"></param>
	/// <returns></returns>
	public static bool isCollidingWithMapForMonsters(Rectangle positionToCheck)
	{
		for (int i = 0; i < 4; i++)
		{
			Vector2 p = Utility.getCornersOfThisRectangle(ref positionToCheck, i);
			if (p.X < 0f || p.Y < 0f || p.X >= 768f || p.Y >= 768f || !AbigailGame.isMapTilePassableForMonsters(AbigailGame.map[(int)p.X / 16 / 3, (int)p.Y / 16 / 3]))
			{
				return true;
			}
		}
		return false;
	}

	public static bool isCollidingWithMap(Rectangle positionToCheck)
	{
		for (int i = 0; i < 4; i++)
		{
			Vector2 p = Utility.getCornersOfThisRectangle(ref positionToCheck, i);
			if (p.X < 0f || p.Y < 0f || p.X >= 768f || p.Y >= 768f || !AbigailGame.isMapTilePassable(AbigailGame.map[(int)p.X / 16 / 3, (int)p.Y / 16 / 3]))
			{
				return true;
			}
		}
		return false;
	}

	public static bool isCollidingWithMap(Point position)
	{
		Rectangle positionToCheck = new Rectangle(position.X, position.Y, 48, 48);
		for (int i = 0; i < 4; i++)
		{
			Vector2 p = Utility.getCornersOfThisRectangle(ref positionToCheck, i);
			if (p.X < 0f || p.Y < 0f || p.X >= 768f || p.Y >= 768f || !AbigailGame.isMapTilePassable(AbigailGame.map[(int)p.X / 16 / 3, (int)p.Y / 16 / 3]))
			{
				return true;
			}
		}
		return false;
	}

	private void addPlayer2MovementDirection(int direction)
	{
		if (!this.player2MovementDirections.Contains(direction))
		{
			if (this.player2MovementDirections.Count == 1 && direction == (this.player2MovementDirections[0] + 2) % 4)
			{
				this.player2MovementDirections.Clear();
			}
			this.player2MovementDirections.Add(direction);
			if (this.player2MovementDirections.Count > 2)
			{
				this.player2MovementDirections.RemoveAt(0);
			}
		}
	}

	private void addPlayerMovementDirection(int direction)
	{
		if (!AbigailGame.gopherTrain && !AbigailGame.playerMovementDirections.Contains(direction))
		{
			if (AbigailGame.playerMovementDirections.Count == 1)
			{
				_ = (AbigailGame.playerMovementDirections.ElementAt(0) + 2) % 4;
			}
			AbigailGame.playerMovementDirections.Add(direction);
		}
	}

	private void addPlayer2ShootingDirection(int direction)
	{
		if (!this.player2ShootingDirections.Contains(direction))
		{
			if (this.player2ShootingDirections.Count == 1 && direction == (this.player2ShootingDirections[0] + 2) % 4)
			{
				this.player2ShootingDirections.Clear();
			}
			this.player2ShootingDirections.Add(direction);
			if (this.player2ShootingDirections.Count > 2)
			{
				this.player2ShootingDirections.RemoveAt(0);
			}
		}
	}

	private void addPlayerShootingDirection(int direction)
	{
		if (!AbigailGame.playerShootingDirections.Contains(direction))
		{
			AbigailGame.playerShootingDirections.Add(direction);
		}
	}

	public void startShoppingLevel()
	{
		this.merchantBox.Y = -AbigailGame.TileSize;
		AbigailGame.shopping = true;
		AbigailGame.merchantArriving = true;
		AbigailGame.merchantLeaving = false;
		AbigailGame.merchantShopOpen = false;
		AbigailGame.overworldSong?.Stop(AudioStopOptions.Immediate);
		AbigailGame.monsters.Clear();
		AbigailGame.waitingForPlayerToMoveDownAMap = true;
		this.storeItems.Clear();
		if (AbigailGame.whichWave == 2)
		{
			this.storeItems.Add(new Rectangle(7 * AbigailGame.TileSize + 12, 8 * AbigailGame.TileSize - AbigailGame.TileSize * 2, AbigailGame.TileSize, AbigailGame.TileSize), 3);
			this.storeItems.Add(new Rectangle(8 * AbigailGame.TileSize + 24, 8 * AbigailGame.TileSize - AbigailGame.TileSize * 2, AbigailGame.TileSize, AbigailGame.TileSize), 0);
			this.storeItems.Add(new Rectangle(9 * AbigailGame.TileSize + 36, 8 * AbigailGame.TileSize - AbigailGame.TileSize * 2, AbigailGame.TileSize, AbigailGame.TileSize), 6);
		}
		else
		{
			this.storeItems.Add(new Rectangle(7 * AbigailGame.TileSize + 12, 8 * AbigailGame.TileSize - AbigailGame.TileSize * 2, AbigailGame.TileSize, AbigailGame.TileSize), (this.runSpeedLevel >= 2) ? 5 : (3 + this.runSpeedLevel));
			this.storeItems.Add(new Rectangle(8 * AbigailGame.TileSize + 24, 8 * AbigailGame.TileSize - AbigailGame.TileSize * 2, AbigailGame.TileSize, AbigailGame.TileSize), (this.fireSpeedLevel < 3) ? this.fireSpeedLevel : ((this.ammoLevel >= 3 && !this.spreadPistol) ? 9 : 10));
			this.storeItems.Add(new Rectangle(9 * AbigailGame.TileSize + 36, 8 * AbigailGame.TileSize - AbigailGame.TileSize * 2, AbigailGame.TileSize, AbigailGame.TileSize), (this.ammoLevel < 3) ? (6 + this.ammoLevel) : 10);
		}
		if (this.whichRound > 0)
		{
			this.storeItems.Clear();
			this.storeItems.Add(new Rectangle(7 * AbigailGame.TileSize + 12, 8 * AbigailGame.TileSize - AbigailGame.TileSize * 2, AbigailGame.TileSize, AbigailGame.TileSize), (this.runSpeedLevel >= 2) ? 5 : (3 + this.runSpeedLevel));
			this.storeItems.Add(new Rectangle(8 * AbigailGame.TileSize + 24, 8 * AbigailGame.TileSize - AbigailGame.TileSize * 2, AbigailGame.TileSize, AbigailGame.TileSize), (this.fireSpeedLevel < 3) ? this.fireSpeedLevel : ((this.ammoLevel >= 3 && !this.spreadPistol) ? 9 : 10));
			this.storeItems.Add(new Rectangle(9 * AbigailGame.TileSize + 36, 8 * AbigailGame.TileSize - AbigailGame.TileSize * 2, AbigailGame.TileSize, AbigailGame.TileSize), (this.ammoLevel < 3) ? (6 + this.ammoLevel) : 10);
		}
	}

	public void receiveKeyPress(Keys k)
	{
		if (AbigailGame.onStartMenu)
		{
			AbigailGame.startTimer = 1;
		}
	}

	public void receiveKeyRelease(Keys k)
	{
	}

	public int getPriceForItem(int whichItem)
	{
		return whichItem switch
		{
			6 => 15, 
			7 => 30, 
			8 => 45, 
			0 => 10, 
			1 => 20, 
			2 => 30, 
			5 => 10, 
			3 => 8, 
			4 => 20, 
			9 => 99, 
			10 => 10, 
			_ => 5, 
		};
	}

	public void draw(SpriteBatch b)
	{
		b.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp);
		if (AbigailGame.onStartMenu)
		{
			b.Draw(Game1.staminaRect, new Rectangle((int)AbigailGame.topLeftScreenCoordinate.X, (int)AbigailGame.topLeftScreenCoordinate.Y, 16 * AbigailGame.TileSize, 16 * AbigailGame.TileSize), Game1.staminaRect.Bounds, Color.Black, 0f, Vector2.Zero, SpriteEffects.None, 0.97f);
			b.Draw(Game1.mouseCursors, new Vector2(Game1.viewport.Width / 2 - 3 * AbigailGame.TileSize, AbigailGame.topLeftScreenCoordinate.Y + (float)(5 * AbigailGame.TileSize)), new Rectangle(128, 1744, 96, 56), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 1f);
		}
		else if ((AbigailGame.gameOver || this.gamerestartTimer > 0) && !AbigailGame.endCutscene)
		{
			b.Draw(Game1.staminaRect, new Rectangle((int)AbigailGame.topLeftScreenCoordinate.X, (int)AbigailGame.topLeftScreenCoordinate.Y, 16 * AbigailGame.TileSize, 16 * AbigailGame.TileSize), Game1.staminaRect.Bounds, Color.Black, 0f, Vector2.Zero, SpriteEffects.None, 0.0001f);
			b.DrawString(Game1.dialogueFont, Game1.content.LoadString("Strings\\StringsFromCSFiles:AbigailGame.cs.11914"), AbigailGame.topLeftScreenCoordinate + new Vector2(6f, 7f) * AbigailGame.TileSize, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);
			b.DrawString(Game1.dialogueFont, Game1.content.LoadString("Strings\\StringsFromCSFiles:AbigailGame.cs.11914"), AbigailGame.topLeftScreenCoordinate + new Vector2(6f, 7f) * AbigailGame.TileSize + new Vector2(-1f, 0f), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);
			b.DrawString(Game1.dialogueFont, Game1.content.LoadString("Strings\\StringsFromCSFiles:AbigailGame.cs.11914"), AbigailGame.topLeftScreenCoordinate + new Vector2(6f, 7f) * AbigailGame.TileSize + new Vector2(1f, 0f), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);
			string option1 = Game1.content.LoadString("Strings\\StringsFromCSFiles:AbigailGame.cs.11917");
			if (this.gameOverOption == 0)
			{
				option1 = "> " + option1;
			}
			string option2 = Game1.content.LoadString("Strings\\StringsFromCSFiles:AbigailGame.cs.11919");
			if (this.gameOverOption == 1)
			{
				option2 = "> " + option2;
			}
			if (this.gamerestartTimer <= 0 || this.gamerestartTimer / 500 % 2 == 0)
			{
				b.DrawString(Game1.smallFont, option1, AbigailGame.topLeftScreenCoordinate + new Vector2(6f, 9f) * AbigailGame.TileSize, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);
			}
			b.DrawString(Game1.smallFont, option2, AbigailGame.topLeftScreenCoordinate + new Vector2(6f, 9f) * AbigailGame.TileSize + new Vector2(0f, AbigailGame.TileSize * 2 / 3), Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);
		}
		else if (AbigailGame.endCutscene)
		{
			switch (AbigailGame.endCutscenePhase)
			{
			case 0:
				b.Draw(Game1.staminaRect, new Rectangle((int)AbigailGame.topLeftScreenCoordinate.X, (int)AbigailGame.topLeftScreenCoordinate.Y, 16 * AbigailGame.TileSize, 16 * AbigailGame.TileSize), Game1.staminaRect.Bounds, Color.Black, 0f, Vector2.Zero, SpriteEffects.None, 0.0001f);
				b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + this.playerPosition + new Vector2(0f, -AbigailGame.TileSize / 4), new Rectangle(384, 1760, 16, 16), Color.White * ((AbigailGame.endCutsceneTimer < 2000) ? (1f * ((float)AbigailGame.endCutsceneTimer / 2000f)) : 1f), 0f, Vector2.Zero, 3f, SpriteEffects.None, this.playerPosition.Y / 10000f + 0.001f);
				b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + this.playerPosition + new Vector2(0f, -AbigailGame.TileSize * 2 / 3) + new Vector2(0f, -AbigailGame.TileSize / 4), new Rectangle(320 + AbigailGame.itemToHold * 16, 1776, 16, 16), Color.White * ((AbigailGame.endCutsceneTimer < 2000) ? (1f * ((float)AbigailGame.endCutsceneTimer / 2000f)) : 1f), 0f, Vector2.Zero, 3f, SpriteEffects.None, this.playerPosition.Y / 10000f + 0.002f);
				break;
			case 4:
			case 5:
				b.Draw(Game1.staminaRect, new Rectangle((int)AbigailGame.topLeftScreenCoordinate.X, (int)AbigailGame.topLeftScreenCoordinate.Y, 16 * AbigailGame.TileSize, 16 * AbigailGame.TileSize), Game1.staminaRect.Bounds, Color.Black, 0f, Vector2.Zero, SpriteEffects.None, 0.97f);
				b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + new Vector2(6 * AbigailGame.TileSize, 3 * AbigailGame.TileSize), new Rectangle(224, 1744, 64, 48), Color.White * ((AbigailGame.endCutsceneTimer > 0) ? (1f - ((float)AbigailGame.endCutsceneTimer - 2000f) / 2000f) : 1f), 0f, Vector2.Zero, 3f, SpriteEffects.None, 1f);
				if (AbigailGame.endCutscenePhase == 5 && this.gamerestartTimer <= 0)
				{
					b.DrawString(Game1.smallFont, Game1.content.LoadString("Strings\\Locations:Saloon_Arcade_PK_NewGame+"), AbigailGame.topLeftScreenCoordinate + new Vector2(3f, 10f) * AbigailGame.TileSize, Color.White, 0f, Vector2.Zero, 1f, SpriteEffects.None, 1f);
				}
				break;
			case 1:
			case 2:
			case 3:
			{
				for (int i = 0; i < 16; i++)
				{
					for (int j = 0; j < 16; j++)
					{
						b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + new Vector2(i, j) * 16f * 3f + new Vector2(0f, AbigailGame.newMapPosition - 16 * AbigailGame.TileSize), new Rectangle(464 + 16 * AbigailGame.map[i, j] + ((AbigailGame.map[i, j] == 5 && this.cactusDanceTimer > 800f) ? 16 : 0), 1680 - AbigailGame.world * 16, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0f);
					}
				}
				b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + new Vector2(6 * AbigailGame.TileSize, 3 * AbigailGame.TileSize), new Rectangle(288, 1697, 64, 80), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.01f);
				if (AbigailGame.endCutscenePhase == 3)
				{
					b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + new Vector2(9 * AbigailGame.TileSize, 7 * AbigailGame.TileSize), new Rectangle(544, 1792, 32, 32), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.05f);
					if (AbigailGame.endCutsceneTimer < 3000)
					{
						b.Draw(Game1.staminaRect, new Rectangle((int)AbigailGame.topLeftScreenCoordinate.X, (int)AbigailGame.topLeftScreenCoordinate.Y, 16 * AbigailGame.TileSize, 16 * AbigailGame.TileSize), Game1.staminaRect.Bounds, Color.Black * (1f - (float)AbigailGame.endCutsceneTimer / 3000f), 0f, Vector2.Zero, SpriteEffects.None, 1f);
					}
					break;
				}
				b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + new Vector2(10 * AbigailGame.TileSize, 8 * AbigailGame.TileSize), new Rectangle(272 - AbigailGame.endCutsceneTimer / 300 % 4 * 16, 1792, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.02f);
				if (AbigailGame.endCutscenePhase == 2)
				{
					b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + this.playerPosition + new Vector2(4f, 13f) * 3f, new Rectangle(484, 1760 + (int)(this.playerMotionAnimationTimer / 100f) * 3, 8, 3), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, this.playerPosition.Y / 10000f + 0.001f + 0.001f);
					b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + this.playerPosition, new Rectangle(384, 1760, 16, 13), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, this.playerPosition.Y / 10000f + 0.002f + 0.001f);
					b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + this.playerPosition + new Vector2(0f, -AbigailGame.TileSize * 2 / 3 - AbigailGame.TileSize / 4), new Rectangle(320 + AbigailGame.itemToHold * 16, 1776, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, this.playerPosition.Y / 10000f + 0.005f);
				}
				b.Draw(Game1.staminaRect, new Rectangle((int)AbigailGame.topLeftScreenCoordinate.X, (int)AbigailGame.topLeftScreenCoordinate.Y, 16 * AbigailGame.TileSize, 16 * AbigailGame.TileSize), Game1.staminaRect.Bounds, Color.Black * ((AbigailGame.endCutscenePhase == 1 && AbigailGame.endCutsceneTimer > 12500) ? ((float)((AbigailGame.endCutsceneTimer - 12500) / 3000)) : 0f), 0f, Vector2.Zero, SpriteEffects.None, 1f);
				break;
			}
			}
		}
		else
		{
			if (AbigailGame.zombieModeTimer > 8200)
			{
				b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + this.playerPosition, new Rectangle(384 + ((AbigailGame.zombieModeTimer / 200 % 2 == 0) ? 16 : 0), 1760, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 1f);
				for (int y = (int)(this.playerPosition.Y - (float)AbigailGame.TileSize); y > -AbigailGame.TileSize; y -= AbigailGame.TileSize)
				{
					b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + new Vector2(this.playerPosition.X, y), new Rectangle(368 + ((y / AbigailGame.TileSize % 3 == 0) ? 16 : 0), 1744, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 1f);
				}
				b.End();
				return;
			}
			for (int k = 0; k < 16; k++)
			{
				for (int l = 0; l < 16; l++)
				{
					b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + new Vector2(k, l) * 16f * 3f + new Vector2(0f, AbigailGame.newMapPosition - 16 * AbigailGame.TileSize), new Rectangle(464 + 16 * AbigailGame.map[k, l] + ((AbigailGame.map[k, l] == 5 && this.cactusDanceTimer > 800f) ? 16 : 0), 1680 - AbigailGame.world * 16, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0f);
				}
			}
			if (AbigailGame.scrollingMap)
			{
				for (int m = 0; m < 16; m++)
				{
					for (int n = 0; n < 16; n++)
					{
						b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + new Vector2(m, n) * 16f * 3f + new Vector2(0f, AbigailGame.newMapPosition), new Rectangle(464 + 16 * AbigailGame.nextMap[m, n] + ((AbigailGame.nextMap[m, n] == 5 && this.cactusDanceTimer > 800f) ? 16 : 0), 1680 - AbigailGame.world * 16, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0f);
					}
				}
				b.Draw(Game1.staminaRect, new Rectangle((int)AbigailGame.topLeftScreenCoordinate.X, -1, 16 * AbigailGame.TileSize, (int)AbigailGame.topLeftScreenCoordinate.Y), Game1.staminaRect.Bounds, Color.Black, 0f, Vector2.Zero, SpriteEffects.None, 1f);
				b.Draw(Game1.staminaRect, new Rectangle((int)AbigailGame.topLeftScreenCoordinate.X, (int)AbigailGame.topLeftScreenCoordinate.Y + 16 * AbigailGame.TileSize, 16 * AbigailGame.TileSize, (int)AbigailGame.topLeftScreenCoordinate.Y + 2), Game1.staminaRect.Bounds, Color.Black, 0f, Vector2.Zero, SpriteEffects.None, 1f);
			}
			if (AbigailGame.deathTimer <= 0f && (AbigailGame.playerInvincibleTimer <= 0 || AbigailGame.playerInvincibleTimer / 100 % 2 == 0))
			{
				if (AbigailGame.holdItemTimer > 0)
				{
					b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + this.playerPosition + new Vector2(0f, -AbigailGame.TileSize / 4), new Rectangle(384, 1760, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, this.playerPosition.Y / 10000f + 0.001f);
					b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + this.playerPosition + new Vector2(0f, -AbigailGame.TileSize * 2 / 3) + new Vector2(0f, -AbigailGame.TileSize / 4), new Rectangle(320 + AbigailGame.itemToHold * 16, 1776, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, this.playerPosition.Y / 10000f + 0.002f);
				}
				else if (AbigailGame.zombieModeTimer > 0)
				{
					b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + this.playerPosition + new Vector2(0f, -AbigailGame.TileSize / 4), new Rectangle(352 + ((AbigailGame.zombieModeTimer / 50 % 2 == 0) ? 16 : 0), 1760, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, this.playerPosition.Y / 10000f + 0.001f);
				}
				else if (AbigailGame.playerMovementDirections.Count == 0 && AbigailGame.playerShootingDirections.Count == 0)
				{
					b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + this.playerPosition + new Vector2(0f, -AbigailGame.TileSize / 4), new Rectangle(496, 1760, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, this.playerPosition.Y / 10000f + 0.001f);
				}
				else
				{
					int facingDirection = ((AbigailGame.playerShootingDirections.Count == 0) ? AbigailGame.playerMovementDirections.ElementAt(0) : AbigailGame.playerShootingDirections.Last());
					b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + this.playerPosition + new Vector2(0f, -AbigailGame.TileSize / 4) + new Vector2(4f, 13f) * 3f, new Rectangle(483, 1760 + (int)(this.playerMotionAnimationTimer / 100f) * 3, 10, 3), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, this.playerPosition.Y / 10000f + 0.001f + 0.001f);
					b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + this.playerPosition + new Vector2(3f, -AbigailGame.TileSize / 4), new Rectangle(464 + facingDirection * 16, 1744, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, this.playerPosition.Y / 10000f + 0.002f + 0.001f);
				}
			}
			if (AbigailGame.playingWithAbigail && this.player2deathtimer <= 0 && (this.player2invincibletimer <= 0 || this.player2invincibletimer / 100 % 2 == 0))
			{
				if (this.player2MovementDirections.Count == 0 && this.player2ShootingDirections.Count == 0)
				{
					b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + AbigailGame.player2Position + new Vector2(0f, -AbigailGame.TileSize / 4), new Rectangle(256, 1728, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, this.playerPosition.Y / 10000f + 0.001f);
				}
				else
				{
					int facingDirection2 = ((this.player2ShootingDirections.Count == 0) ? this.player2MovementDirections[0] : this.player2ShootingDirections[0]);
					b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + AbigailGame.player2Position + new Vector2(0f, -AbigailGame.TileSize / 4) + new Vector2(4f, 13f) * 3f, new Rectangle(243, 1728 + this.player2AnimationTimer / 100 * 3, 10, 3), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, AbigailGame.player2Position.Y / 10000f + 0.001f + 0.001f);
					b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + AbigailGame.player2Position + new Vector2(0f, -AbigailGame.TileSize / 4), new Rectangle(224 + facingDirection2 * 16, 1712, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, AbigailGame.player2Position.Y / 10000f + 0.002f + 0.001f);
				}
			}
			foreach (TemporaryAnimatedSprite temporarySprite in AbigailGame.temporarySprites)
			{
				temporarySprite.draw(b, localPosition: true);
			}
			foreach (CowboyPowerup powerup in AbigailGame.powerups)
			{
				powerup.draw(b);
			}
			foreach (CowboyBullet p in this.bullets)
			{
				b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + new Vector2(p.position.X, p.position.Y), new Rectangle(518, 1760 + (this.bulletDamage - 1) * 4, 4, 4), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.9f);
			}
			foreach (CowboyBullet p2 in AbigailGame.enemyBullets)
			{
				b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + new Vector2(p2.position.X, p2.position.Y), new Rectangle(523, 1760, 5, 5), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.9f);
			}
			if (AbigailGame.shopping)
			{
				if ((AbigailGame.merchantArriving || AbigailGame.merchantLeaving) && !AbigailGame.merchantShopOpen)
				{
					b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + new Vector2(this.merchantBox.Location.X, this.merchantBox.Location.Y), new Rectangle(464 + ((AbigailGame.shoppingTimer / 100 % 2 == 0) ? 16 : 0), 1728, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, (float)this.merchantBox.Y / 10000f + 0.001f);
				}
				else
				{
					int whichFrame = ((this.playerBoundingBox.X - this.merchantBox.X > AbigailGame.TileSize) ? 2 : ((this.merchantBox.X - this.playerBoundingBox.X > AbigailGame.TileSize) ? 1 : 0));
					b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + new Vector2(this.merchantBox.Location.X, this.merchantBox.Location.Y), new Rectangle(496 + whichFrame * 16, 1728, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, (float)this.merchantBox.Y / 10000f + 0.001f);
					b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + new Vector2(this.merchantBox.Location.X - AbigailGame.TileSize, this.merchantBox.Location.Y + AbigailGame.TileSize), new Rectangle(529, 1744, 63, 32), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, (float)this.merchantBox.Y / 10000f + 0.001f);
					foreach (KeyValuePair<Rectangle, int> v in this.storeItems)
					{
						b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + new Vector2(v.Key.Location.X, v.Key.Location.Y), new Rectangle(320 + v.Value * 16, 1776, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, (float)v.Key.Location.Y / 10000f);
						b.DrawString(Game1.smallFont, this.getPriceForItem(v.Value).ToString() ?? "", AbigailGame.topLeftScreenCoordinate + new Vector2((float)(v.Key.Location.X + AbigailGame.TileSize / 2) - Game1.smallFont.MeasureString(this.getPriceForItem(v.Value).ToString() ?? "").X / 2f, v.Key.Location.Y + AbigailGame.TileSize + 3), new Color(88, 29, 43), 0f, Vector2.Zero, 1f, SpriteEffects.None, (float)v.Key.Location.Y / 10000f + 0.002f);
						b.DrawString(Game1.smallFont, this.getPriceForItem(v.Value).ToString() ?? "", AbigailGame.topLeftScreenCoordinate + new Vector2((float)(v.Key.Location.X + AbigailGame.TileSize / 2) - Game1.smallFont.MeasureString(this.getPriceForItem(v.Value).ToString() ?? "").X / 2f - 1f, v.Key.Location.Y + AbigailGame.TileSize + 3), new Color(88, 29, 43), 0f, Vector2.Zero, 1f, SpriteEffects.None, (float)v.Key.Location.Y / 10000f + 0.002f);
						b.DrawString(Game1.smallFont, this.getPriceForItem(v.Value).ToString() ?? "", AbigailGame.topLeftScreenCoordinate + new Vector2((float)(v.Key.Location.X + AbigailGame.TileSize / 2) - Game1.smallFont.MeasureString(this.getPriceForItem(v.Value).ToString() ?? "").X / 2f + 1f, v.Key.Location.Y + AbigailGame.TileSize + 3), new Color(88, 29, 43), 0f, Vector2.Zero, 1f, SpriteEffects.None, (float)v.Key.Location.Y / 10000f + 0.002f);
					}
				}
			}
			if (AbigailGame.waitingForPlayerToMoveDownAMap && (AbigailGame.merchantShopOpen || AbigailGame.merchantLeaving || !AbigailGame.shopping) && AbigailGame.shoppingTimer < 250)
			{
				b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + new Vector2(8.5f, 15f) * AbigailGame.TileSize + new Vector2(-12f, 0f), new Rectangle(355, 1750, 8, 8), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.001f);
			}
			foreach (CowboyMonster monster in AbigailGame.monsters)
			{
				monster.draw(b);
			}
			if (AbigailGame.gopherRunning)
			{
				b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + new Vector2(AbigailGame.gopherBox.X, AbigailGame.gopherBox.Y), new Rectangle(320 + AbigailGame.waveTimer / 100 % 4 * 16, 1792, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, (float)AbigailGame.gopherBox.Y / 10000f + 0.001f);
			}
			if (AbigailGame.gopherTrain && AbigailGame.gopherTrainPosition > -AbigailGame.TileSize)
			{
				b.Draw(Game1.staminaRect, new Rectangle((int)AbigailGame.topLeftScreenCoordinate.X, (int)AbigailGame.topLeftScreenCoordinate.Y, 16 * AbigailGame.TileSize, 16 * AbigailGame.TileSize), Game1.staminaRect.Bounds, Color.Black, 0f, Vector2.Zero, SpriteEffects.None, 0.95f);
				b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + new Vector2(this.playerPosition.X - (float)(AbigailGame.TileSize / 2), AbigailGame.gopherTrainPosition), new Rectangle(384 + AbigailGame.gopherTrainPosition / 30 % 4 * 16, 1792, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.96f);
				b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + new Vector2(this.playerPosition.X + (float)(AbigailGame.TileSize / 2), AbigailGame.gopherTrainPosition), new Rectangle(384 + AbigailGame.gopherTrainPosition / 30 % 4 * 16, 1792, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.96f);
				b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + new Vector2(this.playerPosition.X, AbigailGame.gopherTrainPosition - AbigailGame.TileSize * 3), new Rectangle(320 + AbigailGame.gopherTrainPosition / 30 % 4 * 16, 1792, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.96f);
				b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + new Vector2(this.playerPosition.X - (float)(AbigailGame.TileSize / 2), AbigailGame.gopherTrainPosition - AbigailGame.TileSize), new Rectangle(400, 1728, 32, 32), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.97f);
				if (AbigailGame.holdItemTimer > 0)
				{
					b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + this.playerPosition + new Vector2(0f, -AbigailGame.TileSize / 4), new Rectangle(384, 1760, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.98f);
					b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + this.playerPosition + new Vector2(0f, -AbigailGame.TileSize * 2 / 3) + new Vector2(0f, -AbigailGame.TileSize / 4), new Rectangle(320 + AbigailGame.itemToHold * 16, 1776, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.99f);
				}
				else
				{
					b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + this.playerPosition + new Vector2(0f, -AbigailGame.TileSize / 4), new Rectangle(464, 1760, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.98f);
				}
			}
			else
			{
				b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate - new Vector2(AbigailGame.TileSize + 27, 0f), new Rectangle(294, 1782, 22, 22), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.25f);
				if (this.heldItem != null)
				{
					b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate - new Vector2(AbigailGame.TileSize + 18, -9f), new Rectangle(272 + this.heldItem.which * 16, 1808, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.5f);
				}
				b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate - new Vector2(AbigailGame.TileSize * 2, -AbigailGame.TileSize - 18), new Rectangle(400, 1776, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.5f);
				b.DrawString(Game1.smallFont, "x" + Math.Max(this.lives, 0), AbigailGame.topLeftScreenCoordinate - new Vector2(AbigailGame.TileSize, -AbigailGame.TileSize - AbigailGame.TileSize / 4 - 18), Color.White);
				b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate - new Vector2(AbigailGame.TileSize * 2, -AbigailGame.TileSize * 2 - 18), new Rectangle(272, 1808, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.5f);
				b.DrawString(Game1.smallFont, "x" + this.coins, AbigailGame.topLeftScreenCoordinate - new Vector2(AbigailGame.TileSize, -AbigailGame.TileSize * 2 - AbigailGame.TileSize / 4 - 18), Color.White);
				for (int num = 0; num < AbigailGame.whichWave + this.whichRound * 12; num++)
				{
					b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + new Vector2(AbigailGame.TileSize * 16 + 3, num * 3 * 6), new Rectangle(512, 1760, 5, 5), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.5f);
				}
				b.Draw(Game1.mouseCursors, new Vector2((int)AbigailGame.topLeftScreenCoordinate.X, (int)AbigailGame.topLeftScreenCoordinate.Y - AbigailGame.TileSize / 2 - 12), new Rectangle(595, 1748, 9, 11), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.5f);
				if (!AbigailGame.shootoutLevel)
				{
					b.Draw(Game1.staminaRect, new Rectangle((int)AbigailGame.topLeftScreenCoordinate.X + 30, (int)AbigailGame.topLeftScreenCoordinate.Y - AbigailGame.TileSize / 2 + 3, (int)((float)(16 * AbigailGame.TileSize - 30) * ((float)AbigailGame.waveTimer / 80000f)), AbigailGame.TileSize / 4), (AbigailGame.waveTimer < 8000) ? new Color(188, 51, 74) : new Color(147, 177, 38));
				}
				if (AbigailGame.betweenWaveTimer > 0 && AbigailGame.whichWave == 0 && !AbigailGame.scrollingMap)
				{
					Vector2 pos = new Vector2(Game1.viewport.Width / 2 - 120, Game1.viewport.Height - 144 - 3);
					if (!Game1.options.gamepadControls)
					{
						b.Draw(Game1.mouseCursors, pos, new Rectangle(352, 1648, 80, 48), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.99f);
					}
					else
					{
						b.Draw(Game1.controllerMaps, pos, Utility.controllerMapSourceRect(new Rectangle(681, 157, 160, 96)), Color.White, 0f, Vector2.Zero, 1.5f, SpriteEffects.None, 0.99f);
					}
				}
				if (this.bulletDamage > 1)
				{
					b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + new Vector2(-AbigailGame.TileSize - 3, 16 * AbigailGame.TileSize - AbigailGame.TileSize), new Rectangle(416 + (this.ammoLevel - 1) * 16, 1776, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.5f);
				}
				if (this.fireSpeedLevel > 0)
				{
					b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + new Vector2(-AbigailGame.TileSize - 3, 16 * AbigailGame.TileSize - AbigailGame.TileSize * 2), new Rectangle(320 + (this.fireSpeedLevel - 1) * 16, 1776, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.5f);
				}
				if (this.runSpeedLevel > 0)
				{
					b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + new Vector2(-AbigailGame.TileSize - 3, 16 * AbigailGame.TileSize - AbigailGame.TileSize * 3), new Rectangle(368 + (this.runSpeedLevel - 1) * 16, 1776, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.5f);
				}
				if (this.spreadPistol)
				{
					b.Draw(Game1.mouseCursors, AbigailGame.topLeftScreenCoordinate + new Vector2(-AbigailGame.TileSize - 3, 16 * AbigailGame.TileSize - AbigailGame.TileSize * 4), new Rectangle(464, 1776, 16, 16), Color.White, 0f, Vector2.Zero, 3f, SpriteEffects.None, 0.5f);
				}
			}
			if (AbigailGame.screenFlash > 0)
			{
				b.Draw(Game1.staminaRect, new Rectangle((int)AbigailGame.topLeftScreenCoordinate.X, (int)AbigailGame.topLeftScreenCoordinate.Y, 16 * AbigailGame.TileSize, 16 * AbigailGame.TileSize), Game1.staminaRect.Bounds, new Color(255, 214, 168), 0f, Vector2.Zero, SpriteEffects.None, 1f);
			}
		}
		if (this.fadethenQuitTimer > 0)
		{
			b.Draw(Game1.staminaRect, new Rectangle(0, 0, Game1.viewport.Width, Game1.viewport.Height), Game1.staminaRect.Bounds, Color.Black * (1f - (float)this.fadethenQuitTimer / 2000f), 0f, Vector2.Zero, SpriteEffects.None, 1f);
		}
		if (this.abigailPortraitTimer > 0)
		{
			b.Draw(this.abigail.Portrait, new Vector2(AbigailGame.topLeftScreenCoordinate.X + (float)(16 * AbigailGame.TileSize), this.abigailPortraitYposition), new Rectangle(64 * (this.abigailPortraitExpression % 2), 64 * (this.abigailPortraitExpression / 2), 64, 64), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
			if (this.abigailPortraitTimer < 5500 && this.abigailPortraitTimer > 500)
			{
				int width = SpriteText.getWidthOfString("0" + this.AbigailDialogue + "0");
				int x = ((LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.zh || LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.ru) ? ((int)(AbigailGame.topLeftScreenCoordinate.X + (float)(16 * AbigailGame.TileSize)) + width / 4) : ((int)(AbigailGame.topLeftScreenCoordinate.X + (float)(16 * AbigailGame.TileSize))));
				SpriteText.drawString(b, this.AbigailDialogue, x, (int)((double)this.abigailPortraitYposition - 80.0), 999999, width, 999999, 1f, 0.88f, junimoText: false, -1, "", SpriteText.color_Purple);
			}
		}
		b.End();
		b.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.PointClamp);
		if (Game1.IsMultiplayer)
		{
			string time_of_day_string = Game1.getTimeOfDayString(Game1.timeOfDay);
			Vector2 draw_position = new Vector2((float)Game1.viewport.Width - Game1.dialogueFont.MeasureString(time_of_day_string).X - 16f, 16f);
			Color timeColor = Color.White;
			b.DrawString(Game1.dialogueFont, Game1.getTimeOfDayString(Game1.timeOfDay), draw_position, timeColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.01f);
			b.DrawString(Game1.dialogueFont, Game1.getTimeOfDayString(Game1.timeOfDay), draw_position + new Vector2(1f, 1f) + new Vector2(-3f, -3f), Color.Black, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.02f);
			b.DrawString(Game1.dialogueFont, Game1.getTimeOfDayString(Game1.timeOfDay), draw_position + new Vector2(1f, 1f) + new Vector2(-2f, -2f), Color.Black, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.02f);
			b.DrawString(Game1.dialogueFont, Game1.getTimeOfDayString(Game1.timeOfDay), draw_position + new Vector2(1f, 1f) + new Vector2(-1f, -1f), Color.Black, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.02f);
			b.DrawString(Game1.dialogueFont, Game1.getTimeOfDayString(Game1.timeOfDay), draw_position + new Vector2(1f, 1f) + new Vector2(-3.5f, -3.5f), Color.Black, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.02f);
			b.DrawString(Game1.dialogueFont, Game1.getTimeOfDayString(Game1.timeOfDay), draw_position + new Vector2(1f, 1f) + new Vector2(-1.5f, -1.5f), Color.Black, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.02f);
			b.DrawString(Game1.dialogueFont, Game1.getTimeOfDayString(Game1.timeOfDay), draw_position + new Vector2(1f, 1f) + new Vector2(-2.5f, -2.5f), Color.Black, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.02f);
		}
		b.End();
	}

	public void changeScreenSize()
	{
		AbigailGame.topLeftScreenCoordinate = new Vector2(Game1.viewport.Width / 2 - 384, Game1.viewport.Height / 2 - 384);
	}

	public void unload()
	{
		if (AbigailGame.overworldSong != null && AbigailGame.overworldSong.IsPlaying)
		{
			AbigailGame.overworldSong.Stop(AudioStopOptions.Immediate);
		}
		if (AbigailGame.outlawSong != null && AbigailGame.outlawSong.IsPlaying)
		{
			AbigailGame.outlawSong.Stop(AudioStopOptions.Immediate);
		}
		this.lives = 3;
		Game1.stopMusicTrack(MusicContext.MiniGame);
	}

	public void receiveEventPoke(int data)
	{
	}

	public string minigameId()
	{
		return "PrairieKing";
	}

	public bool doMainGameUpdates()
	{
		return false;
	}

	public bool forceQuit()
	{
		if (AbigailGame.playingWithAbigail)
		{
			return false;
		}
		this.unload();
		return true;
	}
}
