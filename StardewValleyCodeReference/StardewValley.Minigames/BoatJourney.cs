using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley.Extensions;
using StardewValley.GameData;

namespace StardewValley.Minigames;

public class BoatJourney : IMinigame
{
	public class WaterSparkle : Entity
	{
		protected Vector2 _startPosition;

		public WaterSparkle(BoatJourney context)
			: base(context, BoatJourney.GetAssetName(), new Rectangle(647, 524, 1, 1), new Vector2(0f, 0f), new Vector2(0f, 0f))
		{
			base.currentFrame = Game1.random.Next(0, 7);
			base.numFrames = 7;
			base.frameInterval = 0.1f;
			this._startPosition = base.position;
			this.RandomizePosition();
		}

		public void RandomizePosition()
		{
			Rectangle open_water = new Rectangle(0, 112, 640, 528);
			do
			{
				this._startPosition = (base.position = Utility.getRandomPositionInThisRectangle(open_water, Game1.random));
			}
			while (new Rectangle(508, 11, 125, 138).Contains((int)this._startPosition.X, (int)this._startPosition.Y));
			base.velocity.X = Utility.RandomFloat(-0.1f, 0.1f);
		}

		public override void OnAnimationFinished()
		{
			this.RandomizePosition();
			base.OnAnimationFinished();
		}

		public override float GetLayerDepth()
		{
			if (base.layerDepth >= 0f)
			{
				return base.layerDepth;
			}
			return 0.0001f;
		}
	}

	public class Wave : Entity
	{
		protected Vector2 _startPosition;

		public Wave(BoatJourney context, Vector2 position = default(Vector2))
			: base(context, BoatJourney.GetAssetName(), new Rectangle(640, 506, 32, 12), new Vector2(16f, 6f), position)
		{
			base.numFrames = 2;
			base.frameInterval = 1.25f;
			this._startPosition = position;
		}

		public override bool Update(GameTime time)
		{
			base.position = this._startPosition + new Vector2(1f, 0f) * (float)Math.Sin(this._startPosition.X * 0.333f + this._startPosition.Y * 0.1f + base._age) * 3f;
			return base.Update(time);
		}

		public override float GetLayerDepth()
		{
			if (base.layerDepth >= 0f)
			{
				return base.layerDepth;
			}
			return 0.0003f;
		}
	}

	public class Boat : Entity
	{
		protected float nextSmokeStackSmoke;

		protected float nextRipple;

		public Vector2? smokeStack;

		public Vector2 _lastPosition;

		public float idleAnimationInterval = 0.75f;

		public float moveAnimationInterval = 0.25f;

		public Boat(BoatJourney context, string texture_path, Rectangle source_rect, Vector2 origin = default(Vector2), Vector2 position = default(Vector2))
			: base(context, texture_path, source_rect, origin, position)
		{
		}

		public override bool Update(GameTime time)
		{
			bool moved = false;
			if (this._lastPosition != base.position)
			{
				this._lastPosition = base.position;
				moved = true;
			}
			if (moved)
			{
				base.frameInterval = this.moveAnimationInterval;
			}
			else
			{
				base.frameInterval = this.idleAnimationInterval;
			}
			if (this.smokeStack.HasValue)
			{
				if (this.nextSmokeStackSmoke <= 0f)
				{
					this.nextSmokeStackSmoke = 0.25f;
					if (moved)
					{
						Entity smoke_entity = new Entity(base._context, BoatJourney.GetAssetName(), new Rectangle(689, 337, 2, 2), new Vector2(1f, 1f), base.position + this.smokeStack.Value);
						smoke_entity.numFrames = 3;
						Vector2 velocity = new Vector2(Utility.RandomFloat(-0.04f, -0.03f), Utility.RandomFloat(-0.05f, -0.1f));
						smoke_entity.velocity = velocity;
						smoke_entity.destroyAfterAnimation = true;
						base._context.entities.Add(smoke_entity);
					}
				}
				else
				{
					this.nextSmokeStackSmoke -= (float)time.ElapsedGameTime.TotalSeconds;
				}
			}
			if (this.nextRipple <= 0f)
			{
				this.nextRipple = 0.25f;
				if (moved)
				{
					Entity ripple_entity = new Entity(base._context, BoatJourney.GetAssetName(), new Rectangle(640, 336, 9, 16), new Vector2(4f, 0f), base.position + new Vector2(0f, 0f));
					ripple_entity.numFrames = 5;
					ripple_entity.layerDepth = 2E-05f;
					ripple_entity.destroyAfterAnimation = true;
					base._context.entities.Add(ripple_entity);
				}
			}
			else
			{
				this.nextRipple -= (float)time.ElapsedGameTime.TotalSeconds;
			}
			return base.Update(time);
		}
	}

	public class Entity
	{
		protected BoatJourney _context;

		public Vector2 position;

		protected Texture2D _texture;

		protected Rectangle _sourceRect;

		protected float lifeTime;

		protected float _age;

		public Vector2 velocity;

		public Vector2 origin;

		public bool flipX;

		protected float _frameTime;

		public float frameInterval = 0.25f;

		public int currentFrame;

		public int numFrames = 1;

		public int columns;

		public bool destroyAfterAnimation;

		public bool drawOnTop;

		public float layerDepth = -1f;

		public Entity(BoatJourney context, string texture_path, Rectangle source_rect, Vector2 origin = default(Vector2), Vector2 position = default(Vector2))
		{
			this._context = context;
			this._texture = Game1.temporaryContent.Load<Texture2D>(texture_path);
			this._sourceRect = source_rect;
			this.origin = origin;
			this.position = position;
		}

		public virtual bool Update(GameTime time)
		{
			this._age += (float)time.ElapsedGameTime.TotalSeconds;
			this._frameTime += (float)time.ElapsedGameTime.TotalSeconds;
			if (this.lifeTime > 0f && this.lifeTime >= this._age)
			{
				return true;
			}
			if (this.frameInterval > 0f && this._frameTime > this.frameInterval)
			{
				this._frameTime -= this.frameInterval;
				this.currentFrame++;
				if (this.currentFrame >= this.numFrames)
				{
					this.OnAnimationFinished();
					this.currentFrame -= this.numFrames;
					if (this.destroyAfterAnimation)
					{
						return true;
					}
				}
			}
			this.position += this.velocity;
			return false;
		}

		public virtual void OnAnimationFinished()
		{
		}

		public virtual void SetSourceRect(Rectangle rectangle)
		{
			this._sourceRect = rectangle;
		}

		public virtual Rectangle GetSourceRect()
		{
			int x = this.currentFrame;
			int y = 0;
			if (this.columns > 0)
			{
				y = x / this.columns;
				x %= this.columns;
			}
			return new Rectangle(this._sourceRect.X + x * this._sourceRect.Width, this._sourceRect.Y + y * this._sourceRect.Width, this._sourceRect.Width, this._sourceRect.Height);
		}

		public virtual float GetLayerDepth()
		{
			if (this.layerDepth >= 0f)
			{
				return this.layerDepth;
			}
			return this.position.Y / 100000f;
		}

		public virtual void Draw(SpriteBatch b)
		{
			b.Draw(this._texture, this._context.TransformDraw(this.position), this.GetSourceRect(), Color.White, 0f, this.origin, this._context._zoomLevel, this.flipX ? SpriteEffects.FlipHorizontally : SpriteEffects.None, this.GetLayerDepth());
		}
	}

	public float _age;

	public Texture2D texture;

	public Rectangle mapSourceRectangle;

	protected float _zoomLevel = 1f;

	protected Vector2 viewTarget = new Vector2(0f, 0f);

	protected Vector2 _upperLeft;

	public List<Entity> entities;

	protected float _currentBoatSpeed;

	public float boatSpeed = 0.5f;

	public float dockSpeed = 0.1f;

	protected float _nextSlosh;

	protected bool _fadeComplete;

	public Vector2[] points = new Vector2[9]
	{
		new Vector2(286f, 53f),
		new Vector2(286f, 60f),
		new Vector2(287f, 88f),
		new Vector2(340f, 121f),
		new Vector2(357f, 215f),
		new Vector2(204f, 633f),
		new Vector2(274f, 750f),
		new Vector2(352f, 720f),
		new Vector2(352f, 700f)
	};

	protected List<Vector2> _interpolatedPoints;

	protected List<float> _cumulativeDistances;

	protected float _totalPathDistance;

	protected float traveledBoatDistance;

	protected float nextSmoke;

	public float departureDelay = 1.5f;

	protected Boat _boat;

	protected List<Entity> _seagulls = new List<Entity>();

	public BoatJourney()
	{
		Game1.globalFadeToClear();
		Game1.changeMusicTrack("sweet", track_interruptable: false, MusicContext.MiniGame);
		this.mapSourceRectangle = new Rectangle(0, 0, 640, 849);
		this.texture = Game1.temporaryContent.Load<Texture2D>(BoatJourney.GetAssetName());
		this.changeScreenSize();
		Rectangle cloud_start_rectangle = new Rectangle(0, 112, 640, 528);
		this._interpolatedPoints = new List<Vector2>();
		this._cumulativeDistances = new List<float>();
		this._interpolatedPoints.Add(this.points[0]);
		for (int i = 0; i < this.points.Length - 3; i++)
		{
			this._interpolatedPoints.Add(this.points[i + 1]);
			for (int t = 0; t < 10; t++)
			{
				Vector2 interpolated_point = Vector2.CatmullRom(this.points[i], this.points[i + 1], this.points[i + 2], this.points[i + 3], (float)t / 10f);
				this._interpolatedPoints.Add(interpolated_point);
			}
			this._interpolatedPoints.Add(this.points[i + 2]);
		}
		this._interpolatedPoints.Add(this.points[this.points.Length - 1]);
		Vector2 point_start = this._interpolatedPoints[0];
		this._totalPathDistance = 0f;
		for (int j = 0; j < this._interpolatedPoints.Count; j++)
		{
			this._totalPathDistance += (point_start - this._interpolatedPoints[j]).Length();
			point_start = this._interpolatedPoints[j];
			this._cumulativeDistances.Add(this._totalPathDistance);
		}
		this.entities = new List<Entity>();
		for (int k = 0; k < 8; k++)
		{
			Vector2 cloud_position = Utility.getRandomPositionInThisRectangle(cloud_start_rectangle, Game1.random);
			Rectangle cloud_rectangle = new Rectangle(640, 0, 150, 130);
			if (Game1.random.NextDouble() < 0.44999998807907104)
			{
				cloud_rectangle = new Rectangle(640, 136, 150, 120);
			}
			else if (Game1.random.NextDouble() < 0.25)
			{
				cloud_rectangle = new Rectangle(640, 256, 150, 80);
			}
			Entity cloud_entity = new Entity(this, BoatJourney.GetAssetName(), cloud_rectangle, new Vector2(cloud_rectangle.Width / 2, cloud_rectangle.Height), cloud_position)
			{
				velocity = new Vector2(-1f, -1f) * Utility.RandomFloat(0.05f, 0.15f),
				drawOnTop = true
			};
			this.entities.Add(cloud_entity);
		}
		List<Vector2> boat_positions = new List<Vector2>();
		for (int l = 0; l < 2; l++)
		{
			if (Game1.random.NextDouble() < 0.30000001192092896)
			{
				this.SpawnBoat(new Rectangle(640, 416, 32, 32), new Vector2(-1f, 0f), boat_positions);
			}
		}
		if (Game1.random.NextDouble() < 0.20000000298023224)
		{
			this.SpawnBoat(new Rectangle(704, 416, 32, 32), new Vector2(-1f, 0f), boat_positions);
		}
		for (int m = 0; m < 2; m++)
		{
			if (Game1.random.NextDouble() < 0.30000001192092896)
			{
				this.SpawnBoat(new Rectangle(640, 448, 32, 32), new Vector2(1f, 0f), boat_positions);
			}
		}
		for (int n = 0; n < 16; n++)
		{
			Vector2 wave_position = Utility.getRandomPositionInThisRectangle(cloud_start_rectangle, Game1.random);
			Wave wave_entity = new Wave(this, wave_position);
			this.entities.Add(wave_entity);
		}
		for (int num = 0; num < 8; num++)
		{
			WaterSparkle sparkle_entity = new WaterSparkle(this);
			this.entities.Add(sparkle_entity);
		}
		Vector2 gull_position = Utility.getRandomPositionInThisRectangle(cloud_start_rectangle, Game1.random);
		this.CreateFlockOfSeagulls((int)gull_position.X, (int)gull_position.Y, Game1.random.Next(4, 8));
		for (int num2 = 0; num2 < 3; num2++)
		{
			gull_position = Utility.getRandomPositionInThisRectangle(cloud_start_rectangle, Game1.random);
			this.CreateFlockOfSeagulls((int)gull_position.X, (int)gull_position.Y, 1);
		}
		this._seagulls.Sort((Entity a, Entity b) => a.position.Y.CompareTo(b.position.Y));
		this._boat = new Boat(this, BoatJourney.GetAssetName(), new Rectangle(640, 352, 32, 32), new Vector2(16f, 16f), new Vector2(293f, 53f));
		this._boat.smokeStack = new Vector2(0f, -12f);
		this._boat.numFrames = 2;
		this.entities.Add(this._boat);
		Entity dinosaur = new Entity(this, BoatJourney.GetAssetName(), new Rectangle(643, 538, 29, 17), Vector2.Zero, new Vector2(16f, 829f))
		{
			numFrames = 2,
			frameInterval = 0.75f
		};
		this.entities.Add(dinosaur);
	}

	/// <summary>Get the asset name for the main boat journey texture.</summary>
	private static string GetAssetName()
	{
		return "Minigames\\" + Game1.currentSeason + "_boatJourneyMap";
	}

	public void SpawnBoat(Rectangle boat_sprite_rect, Vector2 direction, List<Vector2> other_boat_positions)
	{
		Vector2 potential_point;
		while (true)
		{
			potential_point = Game1.random.ChooseFrom(this._interpolatedPoints);
			if (!new Rectangle(0, 112, 640, 528).Contains((int)potential_point.X, (int)potential_point.Y))
			{
				continue;
			}
			potential_point += direction * Utility.RandomFloat(8f, 64f);
			bool fail = false;
			foreach (Vector2 other_boat_position in other_boat_positions)
			{
				if ((other_boat_position - potential_point).Length() < 24f)
				{
					fail = true;
					break;
				}
			}
			if (!fail)
			{
				break;
			}
		}
		Boat boat = new Boat(this, BoatJourney.GetAssetName(), boat_sprite_rect, new Vector2(16f, 14f), potential_point);
		boat.velocity = direction * Utility.RandomFloat(0.05f, 0.1f);
		boat.numFrames = 2;
		boat.frameInterval = 0.75f;
		other_boat_positions.Add(potential_point);
		this.entities.Add(boat);
	}

	public void CreateFlockOfSeagulls(int x, int y, int depth)
	{
		Vector2 velocity = new Vector2(-0.15f, -0.25f);
		Entity seagull = new Entity(this, BoatJourney.GetAssetName(), new Rectangle(646, 560, 5, 14), new Vector2(2f, 14f), new Vector2(x, y));
		seagull.numFrames = 8;
		seagull.currentFrame = Game1.random.Next(0, 8);
		seagull.velocity = velocity + new Vector2(Utility.RandomFloat(-0.001f, 0.001f), Utility.RandomFloat(-0.001f, 0.001f));
		seagull.frameInterval = Utility.RandomFloat(0.1f, 0.15f);
		this.entities.Add(seagull);
		this._seagulls.Add(seagull);
		Vector2 left = new Vector2(x, y);
		Vector2 right = new Vector2(x, y);
		for (int i = 1; i < depth; i++)
		{
			left.X -= Game1.random.Next(5, 8);
			left.Y += Game1.random.Next(6, 9);
			right.X += Game1.random.Next(5, 8);
			right.Y += Game1.random.Next(6, 9);
			seagull = new Entity(this, BoatJourney.GetAssetName(), new Rectangle(646, 560, 5, 14), new Vector2(2f, 14f), left);
			seagull.numFrames = 8;
			seagull.currentFrame = Game1.random.Next(0, 8);
			seagull.velocity = velocity + new Vector2(Utility.RandomFloat(-0.001f, 0.001f), Utility.RandomFloat(-0.001f, 0.001f));
			seagull.frameInterval = Utility.RandomFloat(0.1f, 0.15f);
			this.entities.Add(seagull);
			this._seagulls.Add(seagull);
			seagull = new Entity(this, BoatJourney.GetAssetName(), new Rectangle(646, 560, 5, 14), new Vector2(2f, 14f), right);
			seagull.numFrames = 8;
			seagull.currentFrame = Game1.random.Next(0, 8);
			seagull.velocity = velocity + new Vector2(Utility.RandomFloat(-0.001f, 0.001f), Utility.RandomFloat(-0.001f, 0.001f));
			seagull.frameInterval = Utility.RandomFloat(0.1f, 0.15f);
			this.entities.Add(seagull);
			this._seagulls.Add(seagull);
		}
	}

	public Vector2 TransformDraw(Vector2 position)
	{
		position.X = (int)(position.X * this._zoomLevel) - (int)this._upperLeft.X;
		position.Y = (int)(position.Y * this._zoomLevel) - (int)this._upperLeft.Y;
		return position;
	}

	public Rectangle TransformDraw(Rectangle dest)
	{
		dest.X = (int)((float)dest.X * this._zoomLevel) - (int)this._upperLeft.X;
		dest.Y = (int)((float)dest.Y * this._zoomLevel) - (int)this._upperLeft.Y;
		dest.Width = (int)((float)dest.Width * this._zoomLevel);
		dest.Height = (int)((float)dest.Height * this._zoomLevel);
		return dest;
	}

	public bool tick(GameTime time)
	{
		if (this._fadeComplete)
		{
			Game1.warpFarmer("IslandSouth", 21, 43, 0);
			return true;
		}
		this._age += (float)time.ElapsedGameTime.TotalSeconds;
		this.entities.RemoveAll((Entity entity) => entity.Update(time));
		this.viewTarget.X = this._boat.position.X;
		this.viewTarget.Y = this._boat.position.Y;
		List<Entity> seagulls = this._seagulls;
		if (seagulls != null && seagulls.Count > 0 && this._boat.position.Y > this._seagulls[0].position.Y)
		{
			if (Math.Abs(this._boat.position.X - this._seagulls[0].position.X) < 128f && Game1.random.NextDouble() < 0.25)
			{
				Game1.playSound("seagulls");
			}
			this._seagulls.RemoveAt(0);
		}
		if (this._interpolatedPoints.Count > 1)
		{
			if (this.departureDelay > 0f)
			{
				this.departureDelay -= (float)time.ElapsedGameTime.TotalSeconds;
			}
			else
			{
				if (this.traveledBoatDistance < this._totalPathDistance)
				{
					float desired_boat_speed = this.boatSpeed;
					if (this._interpolatedPoints.Count <= 2)
					{
						desired_boat_speed = this.dockSpeed;
					}
					this._currentBoatSpeed = Utility.MoveTowards(this._currentBoatSpeed, desired_boat_speed, 0.01f);
					this.traveledBoatDistance += this._currentBoatSpeed;
					if (this.traveledBoatDistance > this._totalPathDistance)
					{
						this.traveledBoatDistance = this._totalPathDistance;
					}
				}
				this._nextSlosh -= (float)time.ElapsedGameTime.TotalSeconds;
				if (this._nextSlosh <= 0f)
				{
					this._nextSlosh = 0.75f;
					Game1.playSound("waterSlosh");
				}
			}
			while (this._interpolatedPoints.Count >= 2 && this.traveledBoatDistance >= this._cumulativeDistances[1])
			{
				this._interpolatedPoints.RemoveAt(0);
				this._cumulativeDistances.RemoveAt(0);
			}
			if (this._interpolatedPoints.Count <= 1)
			{
				this._interpolatedPoints.Clear();
				this._cumulativeDistances.Clear();
				Game1.globalFadeToBlack(delegate
				{
					this._fadeComplete = true;
				});
			}
			else
			{
				Vector2 direction = this._interpolatedPoints[1] - this._interpolatedPoints[0];
				if (Math.Abs(direction.X) > Math.Abs(direction.Y))
				{
					if (direction.X < 0f)
					{
						this._boat.SetSourceRect(new Rectangle(704, 384, 32, 32));
					}
					else
					{
						this._boat.SetSourceRect(new Rectangle(704, 352, 32, 32));
					}
				}
				else if (direction.Y > 0f)
				{
					this._boat.SetSourceRect(new Rectangle(640, 384, 32, 32));
				}
				else
				{
					this._boat.SetSourceRect(new Rectangle(640, 352, 32, 32));
				}
				float t = (this.traveledBoatDistance - this._cumulativeDistances[0]) / (this._cumulativeDistances[1] - this._cumulativeDistances[0]);
				this._boat.position = new Vector2(Utility.Lerp(this._interpolatedPoints[0].X, this._interpolatedPoints[1].X, t), Utility.Lerp(this._interpolatedPoints[0].Y, this._interpolatedPoints[1].Y, t));
			}
		}
		this._upperLeft.X = this.viewTarget.X * this._zoomLevel - (float)(Game1.viewport.Width / 2);
		this._upperLeft.Y = this.viewTarget.Y * this._zoomLevel - (float)(Game1.viewport.Height / 2);
		if (this._upperLeft.Y < 0f)
		{
			this._upperLeft.Y = 0f;
		}
		if (this._upperLeft.Y + (float)Game1.viewport.Height > (float)this.mapSourceRectangle.Height * this._zoomLevel)
		{
			this._upperLeft.Y = (float)this.mapSourceRectangle.Height * this._zoomLevel - (float)Game1.viewport.Height;
		}
		if (this.nextSmoke <= 0f)
		{
			this.nextSmoke = 0.75f;
			Entity smoke_entity = new Entity(this, BoatJourney.GetAssetName(), new Rectangle(640, 480, 16, 16), new Vector2(8f, 8f), new Vector2(350f, 665f));
			smoke_entity.numFrames = 7;
			Vector2 velocity = new Vector2(Utility.RandomFloat(-0.04f, -0.03f), Utility.RandomFloat(-0.1f, -0.2f));
			smoke_entity.velocity = velocity;
			smoke_entity.destroyAfterAnimation = true;
			this.entities.Add(smoke_entity);
		}
		else
		{
			this.nextSmoke -= (float)time.ElapsedGameTime.TotalSeconds;
		}
		return false;
	}

	public void afterFade()
	{
		Game1.currentMinigame = null;
		Game1.globalFadeToClear();
		if (Game1.currentLocation.currentEvent != null)
		{
			Game1.currentLocation.currentEvent.CurrentCommand++;
			Game1.currentLocation.temporarySprites.Clear();
		}
	}

	public bool forceQuit()
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
		if (k == Keys.Escape)
		{
			this.forceQuit();
		}
	}

	public void receiveKeyRelease(Keys k)
	{
	}

	public Color getWaterColorForSeason()
	{
		return Game1.season switch
		{
			Season.Summer => new Color(51, 90, 174), 
			Season.Fall => new Color(56, 70, 128), 
			Season.Winter => new Color(43, 74, 164), 
			_ => new Color(49, 79, 155), 
		};
	}

	public void draw(SpriteBatch b)
	{
		b.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp);
		b.Draw(Game1.staminaRect, new Rectangle(0, 0, Game1.viewport.Width, Game1.viewport.Height), null, this.getWaterColorForSeason(), 0f, Vector2.Zero, SpriteEffects.None, 0f);
		b.Draw(Game1.staminaRect, this.TransformDraw(new Rectangle(-Game1.viewport.Width, 400, Game1.viewport.Width * 3, Game1.viewport.Height)), null, new Color(49, 79, 155), 0f, Vector2.Zero, SpriteEffects.None, 5E-06f);
		b.Draw(this.texture, this.TransformDraw(this.mapSourceRectangle), this.mapSourceRectangle, Color.White, 0f, Vector2.Zero, SpriteEffects.None, 1E-05f);
		b.Draw(this.texture, this.TransformDraw(new Rectangle(-640, 331, 640, 294)), new Rectangle(0, 337, 640, 294), Color.White, 0f, Vector2.Zero, SpriteEffects.None, 1E-05f);
		b.Draw(this.texture, this.TransformDraw(new Rectangle(640, 343, 640, 294)), new Rectangle(0, 337, 640, 294), Color.White, 0f, Vector2.Zero, SpriteEffects.None, 1E-05f);
		for (int i = 0; i < this.entities.Count; i++)
		{
			if (!this.entities[i].drawOnTop)
			{
				this.entities[i].Draw(b);
			}
		}
		b.End();
		b.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp);
		for (int j = 0; j < this.entities.Count; j++)
		{
			if (this.entities[j].drawOnTop)
			{
				this.entities[j].Draw(b);
			}
		}
		b.End();
	}

	public void changeScreenSize()
	{
		this._zoomLevel = 4f;
		if ((float)this.mapSourceRectangle.Height * this._zoomLevel < (float)Game1.viewport.Height)
		{
			this._zoomLevel = (float)Game1.viewport.Height / (float)this.mapSourceRectangle.Height;
		}
	}

	public void unload()
	{
		Game1.stopMusicTrack(MusicContext.MiniGame);
	}

	public void receiveEventPoke(int data)
	{
		throw new NotImplementedException();
	}

	public string minigameId()
	{
		return null;
	}

	public bool doMainGameUpdates()
	{
		return false;
	}

	public bool overrideFreeMouseMovement()
	{
		return Game1.options.SnappyMenus;
	}
}
