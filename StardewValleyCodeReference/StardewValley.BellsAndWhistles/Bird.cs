using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Network;

namespace StardewValley.BellsAndWhistles;

public class Bird
{
	public enum BirdState
	{
		Idle,
		Flying
	}

	public Vector2 position;

	public Point startPosition;

	public Point endPosition;

	public float pathPosition;

	public float velocity;

	public int framesUntilNextMove;

	public BirdState birdState;

	public PerchingBirds context;

	public int peckFrames;

	public int nextPeck;

	public int peckDirection;

	public int birdType;

	public int flapFrames = 2;

	public float flyArcHeight;

	public Bird()
	{
		this.position = new Vector2(0f, 0f);
		this.startPosition = new Point(0, 0);
		this.endPosition = new Point(0, 0);
		this.birdType = Game1.random.Next(0, 4);
	}

	public Bird(Point point, PerchingBirds context, int bird_type = 0, int flap_frames = 2)
	{
		this.startPosition.X = (this.endPosition.X = point.X);
		this.startPosition.Y = (this.endPosition.Y = point.Y);
		this.position.X = ((float)this.startPosition.X + 0.5f) * 64f;
		this.position.Y = ((float)this.startPosition.Y + 0.5f) * 64f;
		this.context = context;
		this.birdType = bird_type;
		this.framesUntilNextMove = Game1.random.Next(100, 300);
		this.peckDirection = Game1.random.Next(0, 2);
		this.flapFrames = flap_frames;
	}

	public virtual void Draw(SpriteBatch b)
	{
		Vector2 offset_position = new Vector2(this.position.X, this.position.Y);
		offset_position.X += (float)Math.Sin((float)Game1.currentGameTime.TotalGameTime.Milliseconds * 0.0025f) * this.velocity * 2f;
		offset_position.Y += (float)Math.Sin((float)Game1.currentGameTime.TotalGameTime.Milliseconds * 0.006f) * this.velocity * 2f;
		offset_position.Y += (float)Math.Sin((double)this.pathPosition * Math.PI) * (0f - this.flyArcHeight);
		SpriteEffects effect = SpriteEffects.None;
		int frame;
		if (this.birdState == BirdState.Idle)
		{
			if (this.peckDirection == 1)
			{
				effect = SpriteEffects.FlipHorizontally;
			}
			frame = ((!this.context.ShouldBirdsRoost()) ? ((this.peckFrames > 0) ? 1 : 0) : ((this.peckFrames <= 0) ? 8 : 9));
		}
		else
		{
			Vector2 offset = new Vector2(this.endPosition.X - this.startPosition.X, this.endPosition.Y - this.startPosition.Y);
			offset.Normalize();
			if (Math.Abs(offset.X) > Math.Abs(offset.Y))
			{
				frame = 2;
				if (offset.X > 0f)
				{
					effect = SpriteEffects.FlipHorizontally;
				}
			}
			else if (offset.Y > 0f)
			{
				frame = 2 + this.flapFrames;
				if (offset.X > 0f)
				{
					effect = SpriteEffects.FlipHorizontally;
				}
			}
			else
			{
				frame = 2 + this.flapFrames * 2;
				if (offset.X < 0f)
				{
					effect = SpriteEffects.FlipHorizontally;
				}
			}
			if (this.pathPosition > 0.95f)
			{
				frame += Game1.currentGameTime.TotalGameTime.Milliseconds / 50 % this.flapFrames;
			}
			else if (!(this.pathPosition > 0.75f))
			{
				frame += Game1.currentGameTime.TotalGameTime.Milliseconds / 100 % this.flapFrames;
			}
		}
		Rectangle source = new Rectangle(this.context.GetBirdWidth() * frame, this.context.GetBirdHeight() * this.birdType, this.context.GetBirdWidth(), this.context.GetBirdHeight());
		Rectangle draw_position = Game1.GlobalToLocal(Game1.viewport, new Rectangle((int)offset_position.X, (int)offset_position.Y, this.context.GetBirdWidth() * 4, this.context.GetBirdHeight() * 4));
		b.Draw(this.context.GetTexture(), draw_position, source, Color.White, 0f, this.context.GetBirdOrigin(), effect, this.position.Y / 10000f);
	}

	public virtual void FlyToNewPoint()
	{
		Point point = this.context.GetFreeBirdPoint(this, 500);
		if (point != default(Point))
		{
			this.context.ReserveBirdPoint(this, point);
			this.startPosition = this.endPosition;
			this.endPosition = point;
			this.pathPosition = 0f;
			this.velocity = 0f;
			if (this.context.ShouldBirdsRoost())
			{
				this.birdState = BirdState.Idle;
			}
			else
			{
				this.birdState = BirdState.Flying;
			}
			float tile_distance = Utility.distance(this.startPosition.X, this.endPosition.X, this.startPosition.Y, this.endPosition.Y);
			if (tile_distance >= 7f)
			{
				this.flyArcHeight = 200f;
			}
			else if (tile_distance >= 5f)
			{
				this.flyArcHeight = 150f;
			}
			else
			{
				this.flyArcHeight = 20f;
			}
		}
		else
		{
			this.framesUntilNextMove = Game1.random.Next(800, 1200);
		}
	}

	public virtual void Update(GameTime time)
	{
		if (this.peckFrames > 0)
		{
			this.peckFrames--;
		}
		else
		{
			this.nextPeck--;
			if (this.nextPeck <= 0)
			{
				if (this.context.ShouldBirdsRoost())
				{
					this.peckFrames = 50;
				}
				else
				{
					this.peckFrames = this.context.peckDuration;
				}
				this.nextPeck = Game1.random.Next(10, 30);
				if (Game1.random.NextDouble() <= 0.75)
				{
					this.nextPeck += Game1.random.Next(50, 100);
					if (!this.context.ShouldBirdsRoost())
					{
						this.peckDirection = Game1.random.Next(0, 2);
					}
				}
			}
		}
		switch (this.birdState)
		{
		case BirdState.Idle:
		{
			if (this.context.ShouldBirdsRoost())
			{
				break;
			}
			using FarmerCollection.Enumerator enumerator = Game1.currentLocation.farmers.GetEnumerator();
			if (enumerator.MoveNext())
			{
				Farmer farmer = enumerator.Current;
				float num = Utility.distance(farmer.position.X, this.position.X, farmer.position.Y, this.position.Y);
				this.framesUntilNextMove--;
				if (num < 200f || this.framesUntilNextMove <= 0)
				{
					this.FlyToNewPoint();
				}
			}
			break;
		}
		case BirdState.Flying:
		{
			float distance = Utility.distance((float)(this.endPosition.X * 64) + 32f, this.position.X, (float)(this.endPosition.Y * 64) + 32f, this.position.Y);
			float max_velocity = this.context.birdSpeed;
			float slow_down_multiplier = 0.25f;
			if (distance > max_velocity / slow_down_multiplier)
			{
				this.velocity = Utility.MoveTowards(this.velocity, max_velocity, 0.5f);
			}
			else
			{
				this.velocity = Math.Max(Math.Min(distance * slow_down_multiplier, this.velocity), 1f);
			}
			float path_distance = Utility.distance((float)this.endPosition.X + 32f, (float)this.startPosition.X + 32f, (float)this.endPosition.Y + 32f, (float)this.startPosition.Y + 32f) * 64f;
			if (path_distance <= 0.0001f)
			{
				path_distance = 0.0001f;
			}
			float delta = this.velocity / path_distance;
			this.pathPosition += delta;
			this.position = new Vector2(Utility.Lerp((float)(this.startPosition.X * 64) + 32f, (float)(this.endPosition.X * 64) + 32f, this.pathPosition), Utility.Lerp((float)(this.startPosition.Y * 64) + 32f, (float)(this.endPosition.Y * 64) + 32f, this.pathPosition));
			if (this.pathPosition >= 1f)
			{
				this.position = new Vector2((float)(this.endPosition.X * 64) + 32f, (float)(this.endPosition.Y * 64) + 32f);
				this.birdState = BirdState.Idle;
				this.velocity = 0f;
				this.framesUntilNextMove = Game1.random.Next(350, 500);
				if (Game1.random.NextDouble() < 0.75)
				{
					this.framesUntilNextMove += Game1.random.Next(200, 300);
				}
			}
			break;
		}
		}
	}
}
