using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Extensions;

namespace StardewValley.BellsAndWhistles;

public class Firefly : Critter
{
	private bool glowing;

	private int id;

	private Vector2 motion;

	private LightSource light;

	public Firefly()
	{
	}

	public Firefly(Vector2 position)
	{
		base.baseFrame = -1;
		base.position = position * 64f;
		base.startingPosition = position * 64f;
		this.motion = new Vector2((float)Game1.random.Next(-10, 11) * 0.1f, (float)Game1.random.Next(-10, 11) * 0.1f);
		this.id = (int)(position.X * 10099f + position.Y * 77f + (float)Game1.random.Next(99999));
		this.light = new LightSource(this.GenerateLightSourceId(this.id), 4, position, (float)Game1.random.Next(4, 6) * 0.1f, Color.Purple * 0.8f, LightSource.LightContext.None, 0L, Game1.currentLocation.NameOrUniqueName);
		this.glowing = true;
		Game1.currentLightSources.Add(this.light);
	}

	public override bool update(GameTime time, GameLocation environment)
	{
		base.position += this.motion;
		this.motion.X += (float)Game1.random.Next(-1, 2) * 0.1f;
		this.motion.Y += (float)Game1.random.Next(-1, 2) * 0.1f;
		if (this.motion.X < -1f)
		{
			this.motion.X = -1f;
		}
		if (this.motion.X > 1f)
		{
			this.motion.X = 1f;
		}
		if (this.motion.Y < -1f)
		{
			this.motion.Y = -1f;
		}
		if (this.motion.Y > 1f)
		{
			this.motion.Y = 1f;
		}
		if (this.glowing)
		{
			this.light.position.Value = base.position;
		}
		if (base.position.X < -128f || base.position.Y < -128f || base.position.X > (float)environment.map.DisplayWidth || base.position.Y > (float)environment.map.DisplayHeight)
		{
			return true;
		}
		return false;
	}

	public override void drawAboveFrontLayer(SpriteBatch b)
	{
		b.Draw(Game1.staminaRect, Game1.GlobalToLocal(base.position), Game1.staminaRect.Bounds, this.glowing ? Color.White : Color.Brown, 0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
	}
}
