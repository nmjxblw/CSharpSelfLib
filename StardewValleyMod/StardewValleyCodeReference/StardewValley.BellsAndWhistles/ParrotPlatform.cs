using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using xTile.Dimensions;

namespace StardewValley.BellsAndWhistles;

public class ParrotPlatform
{
	public enum TakeoffState
	{
		Idle,
		Boarding,
		BeginFlying,
		Liftoff,
		Flying,
		Finished
	}

	public class Parrot
	{
		public Vector2 position;

		public Vector2 anchorPosition;

		public Texture2D texture;

		protected ParrotPlatform _platform;

		protected bool facingRight;

		protected bool facingUp;

		public const int START_HEIGHT = 21;

		public const int END_HEIGHT = 64;

		public float height = 21f;

		public bool flapping;

		public float nextFlap;

		public float slack;

		public Vector2[] points = new Vector2[4];

		public float swayOffset;

		public float liftSpeed;

		public float squawkTime;

		public Parrot(ParrotPlatform platform, int x, int y, bool facing_right, bool facing_up)
		{
			this._platform = platform;
			this.texture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\parrots");
			this.position = new Vector2(x, y);
			this.anchorPosition = this.position;
			this.facingRight = facing_right;
			this.facingUp = facing_up;
			this.swayOffset = Utility.RandomFloat(0f, 100f);
		}

		public virtual void UpdateLine(Vector2 start, Vector2 end)
		{
			float sag = Utility.Lerp(15f, 0f, (this.height - 21f) / 43f);
			for (int i = 0; i < this.points.Length; i++)
			{
				Vector2 point = new Vector2(Utility.Lerp(start.X, end.X, (float)i / (float)(this.points.Length - 1)), Utility.Lerp(start.Y, end.Y, (float)i / (float)(this.points.Length - 1)));
				point.Y -= ((float)Math.Pow(2f * ((float)i / (float)(this.points.Length - 1)) - 1f, 2.0) - 1f) * sag;
				this.points[i] = point;
			}
		}

		public virtual void Update(GameTime time)
		{
			if (this.squawkTime > 0f)
			{
				this.squawkTime -= (float)time.ElapsedGameTime.TotalSeconds;
			}
			if (this._platform.takeoffState < TakeoffState.BeginFlying)
			{
				return;
			}
			this.nextFlap -= (float)time.ElapsedGameTime.TotalSeconds;
			if (this.nextFlap <= 0f)
			{
				this.flapping = !this.flapping;
				if (this.flapping)
				{
					Game1.playSound("batFlap");
					this.nextFlap = Utility.RandomFloat(0.025f, 0.1f);
				}
				else
				{
					this.nextFlap = Utility.RandomFloat(0.075f, 0.15f);
				}
			}
			if (this.height < 64f)
			{
				this.height += this.liftSpeed;
				this.liftSpeed += 0.025f;
				if (this.facingRight)
				{
					this.position.X += 0.15f;
				}
				else
				{
					this.position.X -= 0.15f;
				}
				if (this.facingUp)
				{
					this.position.Y -= 0.15f;
				}
				else
				{
					this.position.Y += 0.15f;
				}
			}
		}

		public virtual void Draw(SpriteBatch b)
		{
			Vector2 draw_position = this._platform.GetDrawPosition() + this.position * 4f;
			float radius = Utility.Lerp(0f, 2f, (this.height - 21f) / 43f);
			Vector2 draw_offset = new Vector2((float)Math.Sin(Game1.currentGameTime.TotalGameTime.TotalSeconds * 4.0 + (double)this.swayOffset) * radius, (float)Math.Cos(Game1.currentGameTime.TotalGameTime.TotalSeconds * 16.0 + (double)this.swayOffset) * radius);
			if (this._platform.takeoffState <= TakeoffState.Boarding)
			{
				int base_frame = 0;
				if (this.squawkTime > 0f)
				{
					draw_offset.X += Utility.RandomFloat(-0.15f, 0.15f) * 4f;
					draw_offset.Y += Utility.RandomFloat(-0.15f, 0.15f) * 4f;
					base_frame = 1;
				}
				b.Draw(this.texture, Game1.GlobalToLocal(Game1.viewport, draw_position - new Vector2(0f, this.height * 4f) + draw_offset * 4f), new Microsoft.Xna.Framework.Rectangle(base_frame * 24, 0, 24, 24), Color.White, 0f, new Vector2(12f, 19f), 4f, this.facingRight ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (draw_position.Y + 0.1f + 192f) / 10000f);
				return;
			}
			int frame_off = (this.flapping ? 1 : 0);
			if (this.flapping && this.nextFlap <= 0.05f)
			{
				frame_off = 2;
			}
			int base_frame2 = 5;
			if (this.facingUp)
			{
				base_frame2 = 8;
			}
			b.Draw(this.texture, Game1.GlobalToLocal(Game1.viewport, draw_position - new Vector2(0f, this.height * 4f) + draw_offset * 4f), new Microsoft.Xna.Framework.Rectangle((base_frame2 + frame_off) * 24, 0, 24, 24), Color.White, 0f, new Vector2(12f, 19f), 4f, this.facingRight ? SpriteEffects.FlipHorizontally : SpriteEffects.None, (draw_position.Y + 0.1f + 128f) / 10000f);
			Vector2 anchor_draw_position = this._platform.position + this.anchorPosition * 4f;
			Vector2 drawPosition = this._platform.GetDrawPosition();
			Vector2 start = Utility.snapDrawPosition(Game1.GlobalToLocal(drawPosition + (this.anchorPosition - new Vector2(0f, 21f)) * 4f));
			Vector2 end = Utility.snapDrawPosition(Game1.GlobalToLocal(drawPosition + (this.position - new Vector2(0f, this.height) + draw_offset) * 4f));
			this.UpdateLine(start + new Vector2(2f, 0f), end);
			if (this.points == null)
			{
				return;
			}
			Vector2? last_position = null;
			float sort_step = 1E-06f;
			float sort_offset = 0f;
			float sort_layer = (anchor_draw_position.Y + 0.05f) / 10000f;
			Vector2[] array = this.points;
			foreach (Vector2 current_point in array)
			{
				b.Draw(this._platform.texture, current_point, new Microsoft.Xna.Framework.Rectangle(16, 68, 16, 16), Color.White, 0f, new Vector2(8f, 8f), 4f, SpriteEffects.None, sort_layer + sort_offset);
				sort_offset += sort_step;
				if (last_position.HasValue)
				{
					Vector2 offset = current_point - last_position.Value;
					int distance = (int)Math.Ceiling(offset.Length() / 4f);
					float rotation = 0f - (float)Math.Atan2(offset.X, offset.Y) + (float)Math.PI / 2f;
					b.Draw(this._platform.texture, last_position.Value, new Microsoft.Xna.Framework.Rectangle(0, 68, 16, 16), Color.White, rotation, new Vector2(0f, 8f), new Vector2((float)(4 * distance) / 16f, 4f), SpriteEffects.None, sort_layer + sort_offset);
					sort_offset += sort_step;
				}
				last_position = current_point;
			}
		}
	}

	[XmlIgnore]
	[InstancedStatic]
	public static ParrotPlatform activePlatform;

	[XmlIgnore]
	public Vector2 position;

	[XmlIgnore]
	public Texture2D texture;

	[XmlIgnore]
	public List<Parrot> parrots = new List<Parrot>();

	[XmlIgnore]
	public float height;

	[XmlIgnore]
	protected Event _takeoffEvent;

	[XmlIgnore]
	public TakeoffState takeoffState;

	[XmlIgnore]
	public float stateTimer;

	[XmlIgnore]
	public float liftSpeed;

	[XmlIgnore]
	protected bool _onActivationTile;

	public Vector2 shake = Vector2.Zero;

	public string currentLocationKey = "";

	public KeyValuePair<string, KeyValuePair<string, Point>> currentDestination;

	public static List<KeyValuePair<string, KeyValuePair<string, Point>>> GetDestinations(bool only_show_accessible = true)
	{
		List<KeyValuePair<string, KeyValuePair<string, Point>>> destinations = new List<KeyValuePair<string, KeyValuePair<string, Point>>>();
		destinations.Add(new KeyValuePair<string, KeyValuePair<string, Point>>("Volcano", new KeyValuePair<string, Point>("IslandNorth", new Point(60, 17))));
		if (Game1.MasterPlayer.hasOrWillReceiveMail("Island_UpgradeBridge") || !only_show_accessible)
		{
			destinations.Add(new KeyValuePair<string, KeyValuePair<string, Point>>("Archaeology", new KeyValuePair<string, Point>("IslandNorth", new Point(5, 49))));
		}
		destinations.Add(new KeyValuePair<string, KeyValuePair<string, Point>>("Farm", new KeyValuePair<string, Point>("IslandWest", new Point(74, 10))));
		destinations.Add(new KeyValuePair<string, KeyValuePair<string, Point>>("Forest", new KeyValuePair<string, Point>("IslandEast", new Point(28, 29))));
		destinations.Add(new KeyValuePair<string, KeyValuePair<string, Point>>("Docks", new KeyValuePair<string, Point>("IslandSouth", new Point(6, 32))));
		return destinations;
	}

	public static List<ParrotPlatform> CreateParrotPlatformsForArea(GameLocation location)
	{
		List<ParrotPlatform> parrot_platforms = new List<ParrotPlatform>();
		foreach (KeyValuePair<string, KeyValuePair<string, Point>> destination in ParrotPlatform.GetDestinations(only_show_accessible: false))
		{
			if (location.Name == destination.Value.Key)
			{
				parrot_platforms.Add(new ParrotPlatform(destination.Value.Value.X - 1, destination.Value.Value.Y - 2, destination.Key));
			}
		}
		return parrot_platforms;
	}

	public ParrotPlatform()
	{
		this.texture = Game1.content.Load<Texture2D>("LooseSprites\\ParrotPlatform");
	}

	public ParrotPlatform(int tile_x, int tile_y, string key)
		: this()
	{
		this.currentLocationKey = key;
		this.position = new Vector2(tile_x * 64, tile_y * 64);
		this.parrots.Add(new Parrot(this, 15, 20, facing_right: false, facing_up: false));
		this.parrots.Add(new Parrot(this, 33, 20, facing_right: true, facing_up: false));
	}

	public virtual void StartDeparture()
	{
		this.takeoffState = TakeoffState.Boarding;
		Game1.playSound("parrot");
		foreach (Parrot parrot in this.parrots)
		{
			parrot.squawkTime = 0.25f;
		}
		this.stateTimer = 0.5f;
		Game1.player.shouldShadowBeOffset = true;
		xTile.Dimensions.Rectangle viewport = Game1.viewport;
		Vector2 farmer_position = Game1.player.Position;
		this._takeoffEvent = new Event("continue/follow/farmer " + Game1.player.TilePoint.X + " " + Game1.player.TilePoint.Y + " " + Game1.player.facingDirection?.ToString() + "/playerControl parrotRide", null, "-1")
		{
			showWorldCharacters = true,
			showGroundObjects = true
		};
		Game1.currentLocation.currentEvent = this._takeoffEvent;
		this._takeoffEvent.Update(Game1.player.currentLocation, Game1.currentGameTime);
		Game1.player.Position = farmer_position;
		Game1.eventUp = true;
		Game1.viewport = viewport;
		foreach (Parrot parrot2 in this.parrots)
		{
			parrot2.height = 21f;
			parrot2.position = parrot2.anchorPosition;
		}
	}

	public virtual void Update(GameTime time)
	{
		if (this.takeoffState == TakeoffState.Idle && !Game1.player.IsBusyDoingSomething())
		{
			bool on_activation_tile = new Microsoft.Xna.Framework.Rectangle((int)this.position.X / 64, (int)this.position.Y / 64, 3, 1).Contains(Game1.player.TilePoint);
			if (this._onActivationTile != on_activation_tile)
			{
				this._onActivationTile = on_activation_tile;
				if (this._onActivationTile && Game1.netWorldState.Value.ParrotPlatformsUnlocked)
				{
					this.Activate();
				}
			}
		}
		this.shake = Vector2.Zero;
		if (this.takeoffState == TakeoffState.Liftoff)
		{
			this.shake.X = Utility.RandomFloat(-0.5f, 0.5f) * 4f;
			this.shake.Y = Utility.RandomFloat(-0.5f, 0.5f) * 4f;
		}
		if (this.stateTimer > 0f)
		{
			this.stateTimer -= (float)time.ElapsedGameTime.TotalSeconds;
		}
		if (this.takeoffState == TakeoffState.Boarding && this.stateTimer <= 0f)
		{
			this.takeoffState = TakeoffState.BeginFlying;
			Game1.playSound("dwoop");
		}
		if (this.takeoffState == TakeoffState.BeginFlying && this.parrots[0].height >= 64f && this.stateTimer <= 0f)
		{
			this.takeoffState = TakeoffState.Liftoff;
			this.stateTimer = 0.5f;
			Game1.playSound("treethud");
		}
		if (this.takeoffState == TakeoffState.Liftoff && this.stateTimer <= 0f)
		{
			this.takeoffState = TakeoffState.Flying;
		}
		if (this.takeoffState >= TakeoffState.Flying && this.parrots[0].height >= 64f)
		{
			this.height += this.liftSpeed;
			this.liftSpeed += 0.025f;
			Game1.player.drawOffset = new Vector2(0f, (0f - this.height) * 4f);
			if (this.height >= 128f && this.takeoffState != TakeoffState.Finished)
			{
				this.takeoffState = TakeoffState.Finished;
				this._takeoffEvent.endBehaviors();
				this._takeoffEvent = null;
				LocationRequest locationRequest = Game1.getLocationRequest(this.currentDestination.Value.Key);
				locationRequest.OnWarp += delegate
				{
					this.takeoffState = TakeoffState.Idle;
					Game1.player.shouldShadowBeOffset = false;
					Game1.player.drawOffset = Vector2.Zero;
				};
				Game1.warpFarmer(locationRequest, this.currentDestination.Value.Value.X, this.currentDestination.Value.Value.Y, 2);
			}
		}
		foreach (Parrot parrot in this.parrots)
		{
			parrot.Update(time);
		}
	}

	public virtual void Activate()
	{
		List<Response> responses = new List<Response>();
		foreach (KeyValuePair<string, KeyValuePair<string, Point>> destination in ParrotPlatform.GetDestinations())
		{
			if (destination.Key != this.currentLocationKey)
			{
				responses.Add(new Response("Go" + destination.Key, Game1.content.LoadString("Strings\\UI:ParrotPlatform_" + destination.Key)));
			}
		}
		responses.Add(new Response("Cancel", Game1.content.LoadString("Strings\\Locations:MineCart_Destination_Cancel")));
		Game1.currentLocation.createQuestionDialogue(Game1.content.LoadString("Strings\\UI:ParrotPlatform_Question"), responses.ToArray(), "ParrotPlatform");
		ParrotPlatform.activePlatform = this;
	}

	public virtual bool AnswerQuestion(Response answer)
	{
		if (this == ParrotPlatform.activePlatform)
		{
			if (Game1.currentLocation.lastQuestionKey != null && Game1.currentLocation.afterQuestion == null && (ArgUtility.SplitBySpace(Game1.currentLocation.lastQuestionKey)[0] + "_" + answer.responseKey).StartsWith("ParrotPlatform_Go"))
			{
				string destination_key = answer.responseKey.Substring(2);
				foreach (KeyValuePair<string, KeyValuePair<string, Point>> destination in ParrotPlatform.GetDestinations())
				{
					if (destination.Key == destination_key)
					{
						this.currentDestination = destination;
						break;
					}
				}
				this.StartDeparture();
				return true;
			}
			ParrotPlatform.activePlatform = null;
		}
		return false;
	}

	public virtual void Cleanup()
	{
		ParrotPlatform.activePlatform = null;
	}

	public virtual bool CheckCollisions(Microsoft.Xna.Framework.Rectangle rectangle)
	{
		int wall_width = 16;
		if (rectangle.Intersects(new Microsoft.Xna.Framework.Rectangle((int)this.position.X, (int)this.position.Y, 192, wall_width)))
		{
			return true;
		}
		if (rectangle.Intersects(new Microsoft.Xna.Framework.Rectangle((int)this.position.X, (int)this.position.Y + 128 - wall_width, 64, wall_width)))
		{
			return true;
		}
		if (rectangle.Intersects(new Microsoft.Xna.Framework.Rectangle((int)this.position.X + 128, (int)this.position.Y + 128 - wall_width, 64, wall_width)))
		{
			return true;
		}
		if (this.takeoffState > TakeoffState.Idle && rectangle.Intersects(new Microsoft.Xna.Framework.Rectangle((int)this.position.X + 64, (int)this.position.Y + 128 - wall_width, 64, wall_width)))
		{
			return true;
		}
		if (rectangle.Intersects(new Microsoft.Xna.Framework.Rectangle((int)this.position.X, (int)this.position.Y, wall_width, 128)))
		{
			return true;
		}
		if (rectangle.Intersects(new Microsoft.Xna.Framework.Rectangle((int)this.position.X + 192 - wall_width, (int)this.position.Y, wall_width, 128)))
		{
			return true;
		}
		return false;
	}

	public virtual bool OccupiesTile(Vector2 tile_pos)
	{
		if (tile_pos.X >= this.position.X / 64f && tile_pos.X < this.position.X / 64f + 3f && tile_pos.Y >= this.position.Y / 64f && tile_pos.Y < this.position.Y / 64f + 2f)
		{
			return true;
		}
		return false;
	}

	public virtual Vector2 GetDrawPosition()
	{
		return this.position - new Vector2(0f, 128f + this.height * 4f) + this.shake;
	}

	public virtual void Draw(SpriteBatch b)
	{
		b.Draw(this.texture, Game1.GlobalToLocal(Game1.viewport, this.position - new Vector2(0f, 128f) + new Vector2(-2f, 38f) * 4f + new Vector2(48f, 32f) * 4f / 2f), new Microsoft.Xna.Framework.Rectangle(48, 73, 48, 32), Color.White, 0f, new Vector2(48f, 32f) / 2f, 4f * (1f - Math.Min(1f, this.height / 480f)), SpriteEffects.None, 0f);
		b.Draw(this.texture, Game1.GlobalToLocal(Game1.viewport, this.GetDrawPosition()), new Microsoft.Xna.Framework.Rectangle(0, 0, 48, 68), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, this.position.Y / 10000f);
		b.Draw(this.texture, Game1.GlobalToLocal(Game1.viewport, this.GetDrawPosition()), new Microsoft.Xna.Framework.Rectangle(48, 0, 48, 68), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (this.position.Y + 128f) / 10000f);
		if (!Game1.netWorldState.Value.ParrotPlatformsUnlocked)
		{
			return;
		}
		foreach (Parrot parrot in this.parrots)
		{
			parrot.Draw(b);
		}
	}
}
