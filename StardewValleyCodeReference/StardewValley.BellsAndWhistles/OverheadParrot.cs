using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace StardewValley.BellsAndWhistles;

public class OverheadParrot : Critter
{
	protected Texture2D _texture;

	public Vector2 velocity;

	public float age;

	public float flyOffset;

	public float height = 64f;

	public Rectangle sourceRect;

	public Vector2 drawOffset;

	public int[] spriteFlapFrames = new int[8] { 0, 0, 0, 0, 1, 2, 2, 1 };

	public int currentFlapIndex;

	public int flapFrameAccumulator;

	public Vector2 swayAmount;

	public Vector2 lastDrawPosition;

	protected bool _shouldDrawShadow;

	public OverheadParrot(Vector2 start_position)
	{
		base.position = start_position;
		this.velocity = new Vector2(Utility.RandomFloat(-4f, -2f), Utility.RandomFloat(5f, 6f));
		this._texture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\parrots");
		this.sourceRect = new Rectangle(0, 0, 24, 24);
		this.sourceRect.Y = 24 * Game1.random.Next(4);
		this.currentFlapIndex = Game1.random.Next(this.spriteFlapFrames.Length);
		this.flyOffset = (float)(Game1.random.NextDouble() * 100.0);
		this.swayAmount.X = Utility.RandomFloat(16f, 32f);
		this.swayAmount.Y = Utility.RandomFloat(10f, 24f);
	}

	public override bool update(GameTime time, GameLocation environment)
	{
		this.flapFrameAccumulator++;
		if (this.flapFrameAccumulator >= 2)
		{
			this.currentFlapIndex++;
			if (this.currentFlapIndex >= this.spriteFlapFrames.Length)
			{
				this.currentFlapIndex = 0;
			}
			this.flapFrameAccumulator = 0;
		}
		this.age += (float)time.ElapsedGameTime.TotalSeconds;
		base.position += this.velocity;
		float x_offset_rad = (this.age + this.flyOffset) * 1f;
		float y_offset_rad = (this.age + this.flyOffset) * 2f;
		this.drawOffset.X = (float)Math.Sin(x_offset_rad) * this.swayAmount.X;
		this.drawOffset.Y = (float)Math.Cos(y_offset_rad) * this.swayAmount.Y;
		Vector2 draw_position = this.GetDrawPosition();
		if (this.currentFlapIndex == 4 && this.flapFrameAccumulator == 0 && Utility.isOnScreen(draw_position, 64))
		{
			Game1.playSound("parrot_flap");
		}
		Vector2 draw_position_offset = draw_position - this.lastDrawPosition;
		this.lastDrawPosition = draw_position;
		int base_sprite = 2;
		if (Math.Abs(draw_position_offset.X) < Math.Abs(draw_position_offset.Y))
		{
			base_sprite = 5;
		}
		this.sourceRect.X = (this.spriteFlapFrames[this.currentFlapIndex] + base_sprite) * 24;
		this._shouldDrawShadow = true;
		Vector2 shadow_position = this.GetShadowPosition();
		if (!Game1.currentLocation.hasTileAt((int)shadow_position.X / 64, (int)shadow_position.Y / 64, "Back"))
		{
			this._shouldDrawShadow = false;
		}
		if (base.position.X < -64f - this.swayAmount.X * 4f || base.position.Y > (float)(environment.map.Layers[0].DisplayHeight + 64) + (this.height + this.swayAmount.Y) * 4f)
		{
			return true;
		}
		return false;
	}

	public Vector2 GetDrawPosition()
	{
		return base.position + new Vector2(this.drawOffset.X, 0f - this.height + this.drawOffset.Y) * 4f;
	}

	public Vector2 GetShadowPosition()
	{
		return base.position + new Vector2(this.drawOffset.X * 4f, -4f);
	}

	public override void draw(SpriteBatch b)
	{
		if (this._shouldDrawShadow)
		{
			b.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, this.GetShadowPosition()), Game1.shadowTexture.Bounds, Color.White, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 3f, SpriteEffects.None, (base.position.Y - 1f) / 10000f);
		}
	}

	public override void drawAboveFrontLayer(SpriteBatch b)
	{
		b.Draw(this._texture, Game1.GlobalToLocal(Game1.viewport, this.GetDrawPosition()), this.sourceRect, Color.White, 0f, new Vector2(12f, 20f), 4f, SpriteEffects.None, base.position.Y / 10000f);
	}
}
