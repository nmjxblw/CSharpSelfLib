using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Extensions;
using StardewValley.Network;

namespace StardewValley.BellsAndWhistles;

public class SandDuggy : INetObject<NetFields>
{
	public enum State
	{
		DigUp,
		Idle,
		DigDown
	}

	[XmlIgnore]
	public NetList<Point, NetPoint> holeLocations = new NetList<Point, NetPoint>();

	[XmlIgnore]
	public int frame;

	[XmlIgnore]
	public NetInt currentHoleIndex = new NetInt(0);

	[XmlIgnore]
	public int _localIndex;

	[XmlIgnore]
	public NetLocationRef locationRef = new NetLocationRef();

	[XmlIgnore]
	public State currentState;

	[XmlIgnore]
	public Texture2D texture;

	[XmlIgnore]
	public float nextFrameUpdate;

	[XmlElement("whacked")]
	public NetBool whacked = new NetBool(value: false);

	[XmlIgnore]
	public NetFields NetFields { get; } = new NetFields("SandDuggy");

	public SandDuggy()
	{
		this.InitNetFields();
	}

	public SandDuggy(GameLocation location, Point[] points)
		: this()
	{
		this.locationRef.Value = location;
		foreach (Point point in points)
		{
			this.holeLocations.Add(point);
		}
		this.currentHoleIndex.Value = this.FindRandomFreePoint();
	}

	public virtual int FindRandomFreePoint()
	{
		if (this.locationRef.Value == null)
		{
			return -1;
		}
		List<int> validHoleLocations = new List<int>();
		for (int i = 0; i < this.holeLocations.Count; i++)
		{
			Point holeTile = this.holeLocations[i];
			if (!this.locationRef.Value.isObjectAtTile(holeTile.X, holeTile.Y) && !this.locationRef.Value.isTerrainFeatureAt(holeTile.X, holeTile.Y) && !this.locationRef.Value.terrainFeatures.ContainsKey(Utility.PointToVector2(holeTile)))
			{
				validHoleLocations.Add(i);
			}
		}
		if (validHoleLocations.Count == 1)
		{
			return validHoleLocations[0];
		}
		validHoleLocations.RemoveAll(delegate(int index)
		{
			Point location = this.holeLocations[index];
			foreach (Farmer current in this.locationRef.Value.farmers)
			{
				if (this.NearFarmer(location, current))
				{
					return true;
				}
			}
			return false;
		});
		if (validHoleLocations.Count > 0)
		{
			return Game1.random.ChooseFrom(validHoleLocations);
		}
		return -1;
	}

	public virtual void InitNetFields()
	{
		this.NetFields.SetOwner(this).AddField(this.holeLocations, "holeLocations").AddField(this.currentHoleIndex, "currentHoleIndex")
			.AddField(this.locationRef.NetFields, "locationRef.NetFields")
			.AddField(this.whacked, "whacked");
		this.whacked.fieldChangeVisibleEvent += OnWhackedChanged;
	}

	public virtual void OnWhackedChanged(NetBool field, bool old_value, bool new_value)
	{
		if (Game1.gameMode == 6 || Utility.ShouldIgnoreValueChangeCallback() || !this.whacked.Value)
		{
			return;
		}
		if (Game1.IsMasterGame)
		{
			int index = this.currentHoleIndex.Value;
			if (index == -1)
			{
				index = 0;
			}
			Game1.player.team.MarkCollectedNut("SandDuggy");
			Game1.createItemDebris(ItemRegistry.Create("(O)73"), new Vector2(this.holeLocations[index].X, this.holeLocations[index].Y) * 64f, -1, this.locationRef.Value);
		}
		if (Game1.currentLocation == this.locationRef.Value)
		{
			this.AnimateWhacked();
		}
	}

	public virtual void AnimateWhacked()
	{
		if (Game1.currentLocation == this.locationRef.Value)
		{
			int index = this.currentHoleIndex.Value;
			if (index == -1)
			{
				index = 0;
			}
			Vector2 position = new Vector2(this.holeLocations[index].X, this.holeLocations[index].Y);
			int ground_position = (int)(position.Y * 64f - 32f);
			if (Utility.isOnScreen((position + new Vector2(0.5f, 0.5f)) * 64f, 64))
			{
				Game1.playSound("axchop");
				Game1.playSound("rockGolemHit");
			}
			TemporaryAnimatedSprite duggy_sprite = new TemporaryAnimatedSprite("LooseSprites/SandDuggy", new Rectangle(0, 48, 16, 48), new Vector2(position.X * 64f, position.Y * 64f - 32f), flipped: false, 0f, Color.White)
			{
				motion = new Vector2(2f, -3f),
				acceleration = new Vector2(0f, 0.25f),
				interval = 1000f,
				animationLength = 1,
				alphaFade = 0.02f,
				layerDepth = 0.07682f,
				scale = 4f,
				yStopCoordinate = ground_position
			};
			duggy_sprite.reachedStopCoordinate = delegate
			{
				duggy_sprite.motion.Y = -3f;
				duggy_sprite.acceleration.Y = 0.25f;
				duggy_sprite.yStopCoordinate = ground_position;
				duggy_sprite.flipped = !duggy_sprite.flipped;
			};
			Game1.currentLocation.temporarySprites.Add(duggy_sprite);
		}
	}

	public virtual void ResetForPlayerEntry()
	{
		this.texture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\SandDuggy");
	}

	public virtual void PerformToolAction(Tool tool, int tile_x, int tile_y)
	{
		if (this.currentState == State.Idle && this._localIndex >= 0)
		{
			Point point = this.holeLocations[this._localIndex];
			if (point.X == tile_x && point.Y == tile_y)
			{
				this.whacked.Value = true;
			}
		}
	}

	public virtual bool NearFarmer(Point location, Farmer farmer)
	{
		if (Math.Abs(location.X - farmer.TilePoint.X) <= 2 && Math.Abs(location.Y - farmer.TilePoint.Y) <= 2)
		{
			return true;
		}
		return false;
	}

	public virtual void Update(GameTime time)
	{
		if (this.whacked.Value)
		{
			return;
		}
		if (this.currentHoleIndex.Value >= 0)
		{
			Point synched_position = this.holeLocations[this.currentHoleIndex.Value];
			if (this.NearFarmer(synched_position, Game1.player) && this.FindRandomFreePoint() != this.currentHoleIndex.Value)
			{
				this.currentHoleIndex.Value = -1;
				DelayedAction.playSoundAfterDelay((Game1.random.NextDouble() < 0.1) ? "cowboy_gopher" : "tinyWhip", 200);
			}
		}
		this.nextFrameUpdate -= (float)time.ElapsedGameTime.TotalSeconds;
		if (this.currentHoleIndex.Value < 0 && Game1.IsMasterGame)
		{
			this.currentHoleIndex.Value = this.FindRandomFreePoint();
		}
		if (this.currentState == State.DigDown && this.frame == 0)
		{
			if (this.currentHoleIndex.Value >= 0)
			{
				this.currentState = State.DigUp;
			}
			this._localIndex = this.currentHoleIndex.Value;
		}
		if (this.currentHoleIndex.Value == -1 || this.currentHoleIndex.Value != this._localIndex)
		{
			this.currentState = State.DigDown;
		}
		if (!(this.nextFrameUpdate <= 0f))
		{
			return;
		}
		if (this._localIndex >= 0)
		{
			switch (this.currentState)
			{
			case State.DigDown:
				this.frame--;
				if (this.frame <= 0)
				{
					this.frame = 0;
				}
				break;
			case State.DigUp:
				if (this._localIndex >= 0)
				{
					this.frame++;
					if (this.frame >= 4)
					{
						this.currentState = State.Idle;
					}
				}
				break;
			case State.Idle:
				this.frame++;
				if (this.frame > 7)
				{
					this.frame = 4;
				}
				break;
			}
		}
		this.nextFrameUpdate = 0.075f;
	}

	public virtual void Draw(SpriteBatch b)
	{
		if (!this.whacked.Value && this._localIndex >= 0)
		{
			Point point = this.holeLocations[this._localIndex];
			Vector2 draw_position = (new Vector2(point.X, point.Y) + new Vector2(0.5f, 0.5f)) * 64f;
			b.Draw(this.texture, Game1.GlobalToLocal(Game1.viewport, draw_position), new Rectangle(this.frame % 4 * 16, this.frame / 4 * 24, 16, 24), Color.White, 0f, new Vector2(8f, 20f), 4f, SpriteEffects.None, draw_position.Y / 10000f);
		}
	}
}
