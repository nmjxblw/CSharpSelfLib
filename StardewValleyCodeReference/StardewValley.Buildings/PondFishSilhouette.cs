using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.ItemTypeDefinitions;

namespace StardewValley.Buildings;

public class PondFishSilhouette
{
	public Vector2 position;

	protected FishPond _pond;

	protected Object _fishObject;

	protected Vector2 _velocity = Vector2.Zero;

	protected float nextDart;

	protected bool _upRight;

	protected float _age;

	protected float _wiggleTimer;

	protected float _sinkAmount = 1f;

	protected float _randomOffset;

	protected bool _flipped;

	public PondFishSilhouette(FishPond pond)
	{
		this._pond = pond;
		this._fishObject = this._pond.GetFishObject();
		if (this._fishObject.HasContextTag("fish_upright"))
		{
			this._upRight = true;
		}
		this.position = (this._pond.GetCenterTile() + new Vector2(0.5f, 0.5f)) * 64f;
		this._age = 0f;
		this._randomOffset = Utility.Lerp(0f, 500f, (float)Game1.random.NextDouble());
		this.ResetDartTime();
	}

	public void ResetDartTime()
	{
		this.nextDart = Utility.Lerp(20f, 40f, (float)Game1.random.NextDouble());
	}

	public void Draw(SpriteBatch b)
	{
		float angle = (float)Math.PI / 4f;
		if (this._upRight)
		{
			angle = 0f;
		}
		SpriteEffects effect = SpriteEffects.None;
		angle += (float)Math.Sin(this._wiggleTimer + this._randomOffset) * 2f * (float)Math.PI / 180f;
		if (this._velocity.Y < 0f)
		{
			angle -= (float)Math.PI / 18f;
		}
		if (this._velocity.Y > 0f)
		{
			angle += (float)Math.PI / 18f;
		}
		if (this._flipped)
		{
			effect = SpriteEffects.FlipHorizontally;
			angle *= -1f;
		}
		float draw_scale = Utility.Lerp(0.75f, 0.65f, Utility.Clamp(this._sinkAmount, 0f, 1f));
		draw_scale *= Utility.Lerp(1f, 0.75f, (float)this._pond.currentOccupants.Value / 10f);
		Vector2 draw_position = this.position;
		draw_position.Y += (float)Math.Sin(this._age * 2f + this._randomOffset) * 5f;
		draw_position.Y += (int)(this._sinkAmount * 4f);
		float transparency = Utility.Lerp(0.25f, 0.15f, Utility.Clamp(this._sinkAmount, 0f, 1f));
		Vector2 origin = new Vector2(8f, 8f);
		ParsedItemData itemData = ItemRegistry.GetDataOrErrorItem(this._fishObject.QualifiedItemId);
		b.Draw(itemData.GetTexture(), Game1.GlobalToLocal(Game1.viewport, draw_position), itemData.GetSourceRect(), Color.Black * transparency, angle, origin, 4f * draw_scale, effect, this.position.Y / 10000f + 1E-06f);
	}

	public bool IsMoving()
	{
		return this._velocity.LengthSquared() > 0f;
	}

	public void Update(float time)
	{
		this.nextDart -= time;
		this._age += time;
		this._wiggleTimer += time;
		if (this.nextDart <= 0f || (this.nextDart <= 0.5f && Game1.random.NextDouble() < 0.10000000149011612))
		{
			this.ResetDartTime();
			int direction = Game1.random.Next(0, 2) * 2 - 1;
			if (direction < 0)
			{
				this._flipped = true;
			}
			else
			{
				this._flipped = false;
			}
			this._velocity = new Vector2((float)direction * Utility.Lerp(50f, 100f, (float)Game1.random.NextDouble()), Utility.Lerp(-50f, 50f, (float)Game1.random.NextDouble()));
		}
		bool moving = this._velocity.LengthSquared() > 0f;
		if (moving)
		{
			this._wiggleTimer += time * 30f;
			this._sinkAmount = Utility.MoveTowards(this._sinkAmount, 0f, 2f * time);
		}
		else
		{
			this._sinkAmount = Utility.MoveTowards(this._sinkAmount, 1f, 1f * time);
		}
		this.position += this._velocity * time;
		for (int i = 0; i < this._pond.GetFishSilhouettes().Count; i++)
		{
			PondFishSilhouette other_silhouette = this._pond.GetFishSilhouettes()[i];
			if (other_silhouette == this)
			{
				continue;
			}
			float push_amount = 30f;
			float push_other_amount = 30f;
			if (this.IsMoving())
			{
				push_amount = 0f;
			}
			if (other_silhouette.IsMoving())
			{
				push_other_amount = 0f;
			}
			if (Math.Abs(other_silhouette.position.X - this.position.X) < 32f)
			{
				if (other_silhouette.position.X > this.position.X)
				{
					other_silhouette.position.X += push_other_amount * time;
					this.position.X += (0f - push_amount) * time;
				}
				else
				{
					other_silhouette.position.X -= push_other_amount * time;
					this.position.X += push_amount * time;
				}
			}
			if (Math.Abs(other_silhouette.position.Y - this.position.Y) < 32f)
			{
				if (other_silhouette.position.Y > this.position.Y)
				{
					other_silhouette.position.Y += push_other_amount * time;
					this.position.Y += -1f * time;
				}
				else
				{
					other_silhouette.position.Y -= push_other_amount * time;
					this.position.Y += 1f * time;
				}
			}
		}
		this._velocity.X = Utility.MoveTowards(this._velocity.X, 0f, 50f * time);
		this._velocity.Y = Utility.MoveTowards(this._velocity.Y, 0f, 20f * time);
		float border_width = 1.3f;
		if (this.position.X > ((float)(this._pond.tileX.Value + this._pond.tilesWide.Value) - border_width) * 64f)
		{
			this.position.X = ((float)(this._pond.tileX.Value + this._pond.tilesWide.Value) - border_width) * 64f;
			this._velocity.X *= -1f;
			if (moving && (Game1.random.NextDouble() < 0.25 || Math.Abs(this._velocity.X) > 30f))
			{
				this._flipped = !this._flipped;
			}
		}
		if (this.position.X < ((float)this._pond.tileX.Value + border_width) * 64f)
		{
			this.position.X = ((float)this._pond.tileX.Value + border_width) * 64f;
			this._velocity.X *= -1f;
			if (moving && (Game1.random.NextDouble() < 0.25 || Math.Abs(this._velocity.X) > 30f))
			{
				this._flipped = !this._flipped;
			}
		}
		if (this.position.Y > ((float)(this._pond.tileY.Value + this._pond.tilesHigh.Value) - border_width) * 64f)
		{
			this.position.Y = ((float)(this._pond.tileY.Value + this._pond.tilesHigh.Value) - border_width) * 64f;
			this._velocity.Y *= -1f;
		}
		if (this.position.Y < ((float)this._pond.tileY.Value + border_width) * 64f)
		{
			this.position.Y = ((float)this._pond.tileY.Value + border_width) * 64f;
			this._velocity.Y *= -1f;
		}
	}
}
