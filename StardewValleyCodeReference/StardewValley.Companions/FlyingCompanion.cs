using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Extensions;

namespace StardewValley.Companions;

public class FlyingCompanion : Companion
{
	public const int VARIANT_FAIRY = 0;

	public const int VARIANT_PARROT = 1;

	private float flitTimer;

	private Vector2 extraPosition;

	private Vector2 extraPositionMotion;

	private Vector2 extraPositionAcceleration;

	private bool floatup;

	private int flapAnimationLength = 4;

	private int currentSidewaysFlap;

	private bool hasLight = true;

	private string lightId;

	private NetInt whichSubVariant = new NetInt(-1);

	private NetInt startingYForVariant = new NetInt(0);

	private bool perching;

	private float timeSinceLastZeroLerp;

	private float parrot_squawkTimer;

	private float parrot_squatTimer;

	public FlyingCompanion()
	{
		this.lightId = $"{"FlyingCompanion"}_{Game1.random.Next()}";
	}

	public FlyingCompanion(int whichVariant, int whichSubVariant = -1)
		: this()
	{
		base.whichVariant.Value = whichVariant;
		this.whichSubVariant.Value = whichSubVariant;
		if (whichVariant == 1)
		{
			this.startingYForVariant.Value = 160;
			this.hasLight = false;
		}
	}

	public override void InitNetFields()
	{
		base.InitNetFields();
		base.NetFields.AddField(this.whichSubVariant, "whichSubVariant").AddField(this.startingYForVariant, "startingYForVariant");
	}

	public override void Draw(SpriteBatch b)
	{
		if (base.Owner?.currentLocation == null || (base.Owner.currentLocation.DisplayName == "Temp" && !Game1.isFestival()))
		{
			return;
		}
		Texture2D texture = Game1.content.Load<Texture2D>("TileSheets\\companions");
		SpriteEffects effect = SpriteEffects.None;
		if (base.direction.Value == 1)
		{
			effect = SpriteEffects.FlipHorizontally;
		}
		if (this.perching)
		{
			if (this.parrot_squatTimer > 0f)
			{
				b.Draw(texture, Game1.GlobalToLocal(base.Position + base.Owner.drawOffset + new Vector2(0f, (0f - base.height) * 4f) + this.extraPosition), new Rectangle((int)(this.parrot_squatTimer % 1000f) / 500 * 16 + 128, this.startingYForVariant.Value, 16, 16), Color.White, 0f, new Vector2(8f, 8f), 4f, effect, base._position.Y / 10000f);
			}
			else if (this.parrot_squawkTimer > 0f)
			{
				b.Draw(texture, Game1.GlobalToLocal(base.Position + base.Owner.drawOffset + new Vector2(0f, (0f - base.height) * 4f) + this.extraPosition), new Rectangle(160, this.startingYForVariant.Value, 16, 16), Color.White, 0f, new Vector2(8f, 8f), 4f, effect, base._position.Y / 10000f);
			}
			else
			{
				b.Draw(texture, Game1.GlobalToLocal(base.Position + base.Owner.drawOffset + new Vector2(0f, (0f - base.height) * 4f) + this.extraPosition), new Rectangle(128, this.startingYForVariant.Value, 16, 16), Color.White, 0f, new Vector2(8f, 8f), 4f, effect, base._position.Y / 10000f);
			}
		}
		else
		{
			b.Draw(texture, Game1.GlobalToLocal(base.Position + base.Owner.drawOffset + new Vector2(0f, (0f - base.height) * 4f) + this.extraPosition), new Rectangle(this.whichSubVariant.Value * 64 + (int)(this.flitTimer / (float)(500 / this.flapAnimationLength)) * 16, this.startingYForVariant.Value, 16, 16), Color.White, 0f, new Vector2(8f, 8f), 4f, effect, base._position.Y / 10000f);
			b.Draw(Game1.shadowTexture, Game1.GlobalToLocal(base.Position + base.Owner.drawOffset + new Vector2(this.extraPosition.X, 0f)), Game1.shadowTexture.Bounds, Color.White, 0f, new Vector2(Game1.shadowTexture.Bounds.Center.X, Game1.shadowTexture.Bounds.Center.Y), 3f * Utility.Lerp(1f, 0.8f, Math.Min(base.height, 1f)), SpriteEffects.None, (base._position.Y - 8f) / 10000f - 2E-06f);
		}
	}

	public override void Update(GameTime time, GameLocation location)
	{
		base.Update(time, location);
		base.height = 32f;
		this.flitTimer += (float)time.ElapsedGameTime.TotalMilliseconds;
		if (this.flitTimer > (float)(this.flapAnimationLength * 125))
		{
			this.flitTimer = 0f;
			this.extraPositionMotion = new Vector2((Game1.random.NextDouble() < 0.5) ? 0.1f : (-0.1f), -2f);
			if (this.extraPositionMotion.X < 0f)
			{
				this.currentSidewaysFlap--;
			}
			else
			{
				this.currentSidewaysFlap++;
			}
			if (this.currentSidewaysFlap < -4 || this.currentSidewaysFlap > 4)
			{
				this.extraPositionMotion.X *= -1f;
			}
			this.extraPositionAcceleration = new Vector2(0f, this.floatup ? 0.13f : 0.14f);
			if (this.extraPosition.Y > 8f)
			{
				this.floatup = true;
			}
			else if (this.extraPosition.Y < -8f)
			{
				this.floatup = false;
			}
		}
		if (!this.perching)
		{
			this.extraPosition += this.extraPositionMotion;
			this.extraPositionMotion += this.extraPositionAcceleration;
		}
		if (this.hasLight && location.Equals(Game1.currentLocation))
		{
			Utility.repositionLightSource(this.lightId, base.Position - new Vector2(0f, base.height * 4f) + this.extraPosition);
		}
		if (base.whichVariant.Value != 1)
		{
			return;
		}
		if (base.lerp <= 0f)
		{
			this.timeSinceLastZeroLerp += (float)time.ElapsedGameTime.TotalMilliseconds;
		}
		else
		{
			this.timeSinceLastZeroLerp = 0f;
		}
		this.whichSubVariant.Value = ((!(this.timeSinceLastZeroLerp < 100f)) ? 1 : 0);
		if (this.timeSinceLastZeroLerp > 2000f)
		{
			if (!this.perching && (!(Math.Abs(base.OwnerPosition.X - (base.Position.X + this.extraPosition.X)) < 8f) || !(Math.Abs(base.OwnerPosition.Y - (base.Position.Y + this.extraPosition.Y)) < 8f)))
			{
				return;
			}
			if (this.perching && !(base.Owner.Position + new Vector2(32f, 20f)).Equals(base.Position))
			{
				this.perching = false;
				this.timeSinceLastZeroLerp = 0f;
				this.parrot_squatTimer = 0f;
				this.parrot_squawkTimer = 0f;
				return;
			}
			if (this.parrot_squawkTimer > 0f)
			{
				this.parrot_squawkTimer -= (float)time.ElapsedGameTime.TotalMilliseconds;
			}
			if (this.parrot_squatTimer > 0f)
			{
				this.parrot_squatTimer -= (float)time.ElapsedGameTime.TotalMilliseconds;
			}
			this.perching = true;
			base.Position = base.Owner.Position + new Vector2(32f, 20f);
			this.extraPosition = Vector2.Zero;
			base.endPosition = base.Position;
			if (Game1.random.NextDouble() < 0.0005 && this.parrot_squawkTimer <= 0f)
			{
				this.parrot_squawkTimer = 500f;
				location.localSound("parrot_squawk");
			}
			else if (Game1.random.NextDouble() < 0.0015 && this.parrot_squatTimer <= 0f)
			{
				this.parrot_squatTimer = Game1.random.Next(2, 6) * 1000;
			}
		}
		else
		{
			this.perching = false;
		}
	}

	public override void InitializeCompanion(Farmer farmer)
	{
		base.InitializeCompanion(farmer);
		if (this.hasLight)
		{
			Game1.currentLightSources.Add(new LightSource(this.lightId, 1, base.Position, 2f, Color.Black, LightSource.LightContext.None, 0L));
		}
		if (this.whichSubVariant.Value == -1)
		{
			Random r = Utility.CreateRandom(farmer.uniqueMultiplayerID.Value);
			this.whichSubVariant.Value = r.Next(4);
			if (base.whichVariant.Value == 0 && r.NextDouble() < 0.5)
			{
				this.startingYForVariant.Value += 176;
			}
		}
	}

	public override void CleanupCompanion()
	{
		base.CleanupCompanion();
		if (this.hasLight)
		{
			Utility.removeLightSource(this.lightId);
		}
	}

	public override void OnOwnerWarp()
	{
		base.OnOwnerWarp();
		this.extraPosition = Vector2.Zero;
		this.extraPositionMotion = Vector2.Zero;
		this.extraPositionAcceleration = Vector2.Zero;
		if (this.hasLight)
		{
			Game1.currentLightSources.Add(new LightSource(this.lightId, 1, base.Position, 2f, Color.Black, LightSource.LightContext.None, 0L));
		}
	}

	public override void Hop(float amount)
	{
	}
}
