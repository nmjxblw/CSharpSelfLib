using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley.Locations;

public class Wisp
{
	public Vector2 position;

	public Vector2 drawPosition;

	public Vector2[] oldPositions = new Vector2[16];

	public int oldPositionIndex;

	public int index;

	public int tailUpdateTimer;

	public float rotationSpeed;

	public float rotationOffset;

	public float rotationRadius = 16f;

	public float age;

	public float lifeTime = 1f;

	public Color baseColor;

	public Wisp(int index)
	{
		this.Reinitialize();
	}

	public virtual void Reinitialize()
	{
		this.baseColor = Color.White * Utility.RandomFloat(0.25f, 0.75f);
		this.rotationOffset = Utility.RandomFloat(0f, 360f);
		this.rotationSpeed = Utility.RandomFloat(0.5f, 2f);
		this.rotationRadius = Utility.RandomFloat(8f, 32f);
		this.lifeTime = Utility.RandomFloat(6f, 12f);
		this.age = 0f;
		this.position = new Vector2(Game1.random.Next(0, Game1.currentLocation.map.DisplayWidth), Game1.random.Next(0, Game1.currentLocation.map.DisplayHeight));
		this.drawPosition = Vector2.Zero;
		for (int i = 0; i < this.oldPositions.Length; i++)
		{
			this.oldPositions[i] = Vector2.Zero;
		}
	}

	public virtual void Update(GameTime time)
	{
		this.age += (float)time.ElapsedGameTime.TotalSeconds;
		this.position.X -= Math.Max(0.4f, Math.Min(1f, (float)this.index * 0.01f)) - (float)((double)((float)this.index * 0.01f) * Math.Sin(Math.PI * 2.0 * (double)time.TotalGameTime.Milliseconds / 8000.0));
		this.position.Y += Math.Max(0.5f, Math.Min(1.2f, (float)this.index * 0.02f));
		if (this.age >= this.lifeTime)
		{
			this.Reinitialize();
		}
		else if (this.position.Y > (float)Game1.currentLocation.map.DisplayHeight)
		{
			this.Reinitialize();
		}
		else if (this.position.X < 0f)
		{
			this.Reinitialize();
		}
		this.drawPosition = this.position + new Vector2((float)Math.Sin(this.age * this.rotationSpeed + this.rotationOffset), (float)Math.Sin(this.age * this.rotationSpeed + this.rotationOffset)) * this.rotationRadius;
		this.tailUpdateTimer--;
		if (this.tailUpdateTimer <= 0)
		{
			this.tailUpdateTimer = 6;
			this.oldPositionIndex = (this.oldPositionIndex + 1) % this.oldPositions.Length;
			this.oldPositions[this.oldPositionIndex] = this.drawPosition;
		}
	}

	public virtual void Draw(SpriteBatch b)
	{
		Color draw_color = this.baseColor;
		draw_color *= Utility.Lerp(0f, 1f, (float)Math.Sin((double)(this.age / this.lifeTime) * Math.PI));
		float rotation = this.age * this.rotationSpeed * 2f + this.rotationOffset * (float)this.index;
		b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, this.drawPosition), new Rectangle(346 + (int)(this.age / 0.25f + this.rotationOffset) % 4 * 5, 1971, 5, 5), draw_color, rotation, new Vector2(2.5f, 2.5f), 4f, SpriteEffects.None, 1f);
		int tail_index = this.oldPositionIndex;
		for (int i = 0; i < this.oldPositions.Length; i++)
		{
			tail_index++;
			if (tail_index >= this.oldPositions.Length)
			{
				tail_index = 0;
			}
			b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, this.oldPositions[tail_index]), new Rectangle(356, 1971, 5, 5), draw_color * ((float)i / (float)this.oldPositions.Length), rotation - (float)i, new Vector2(2.5f, 2.5f), 2f, SpriteEffects.None, 1f);
		}
	}
}
