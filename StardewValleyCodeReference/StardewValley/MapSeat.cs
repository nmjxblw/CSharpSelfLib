using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Network;

namespace StardewValley;

public class MapSeat : INetObject<NetFields>, ISittable
{
	[XmlIgnore]
	public static Texture2D mapChairTexture;

	[XmlIgnore]
	public NetLongDictionary<int, NetInt> sittingFarmers = new NetLongDictionary<int, NetInt>();

	[XmlIgnore]
	public NetVector2 tilePosition = new NetVector2();

	[XmlIgnore]
	public NetVector2 size = new NetVector2();

	[XmlIgnore]
	public NetInt direction = new NetInt();

	[XmlIgnore]
	public NetVector2 drawTilePosition = new NetVector2(new Vector2(-1f, -1f));

	[XmlIgnore]
	public NetBool seasonal = new NetBool();

	[XmlIgnore]
	public NetString seatType = new NetString();

	[XmlIgnore]
	public NetString textureFile = new NetString(null);

	[XmlIgnore]
	public string _loadedTextureFile;

	[XmlIgnore]
	public Texture2D overlayTexture;

	[XmlIgnore]
	public int localSittingDirection = 2;

	[XmlIgnore]
	public Vector3? customDrawValues;

	[XmlIgnore]
	public NetFields NetFields { get; } = new NetFields("MapSeat");

	public MapSeat()
	{
		this.NetFields.SetOwner(this).AddField(this.sittingFarmers, "sittingFarmers").AddField(this.tilePosition, "tilePosition")
			.AddField(this.size, "size")
			.AddField(this.direction, "direction")
			.AddField(this.drawTilePosition, "drawTilePosition")
			.AddField(this.seasonal, "seasonal")
			.AddField(this.seatType, "seatType")
			.AddField(this.textureFile, "textureFile");
	}

	public static MapSeat FromData(string data, int x, int y)
	{
		MapSeat instance = new MapSeat();
		try
		{
			string[] data_split = data.Split('/');
			instance.tilePosition.Set(new Vector2(x, y));
			instance.size.Set(new Vector2(int.Parse(data_split[0]), int.Parse(data_split[1])));
			instance.seatType.Value = data_split[3];
			int direction;
			if (data_split[2] == "opposite")
			{
				instance.direction.Value = -2;
			}
			else if (Utility.TryParseDirection(data_split[2], out direction))
			{
				instance.direction.Value = direction;
			}
			else
			{
				instance.direction.Value = 2;
			}
			instance.drawTilePosition.Set(new Vector2(int.Parse(data_split[4]), int.Parse(data_split[5])));
			instance.seasonal.Value = data_split[6] == "true";
			if (data_split.Length > 7)
			{
				instance.textureFile.Value = data_split[7];
			}
			else
			{
				instance.textureFile.Value = null;
			}
		}
		catch (Exception)
		{
		}
		return instance;
	}

	public bool IsBlocked(GameLocation location)
	{
		Rectangle rect = this.GetSeatBounds();
		rect.X *= 64;
		rect.Y *= 64;
		rect.Width *= 64;
		rect.Height *= 64;
		Rectangle extended_rect = rect;
		switch (this.direction.Value)
		{
		case 0:
			extended_rect.Y -= 32;
			extended_rect.Height += 32;
			break;
		case 2:
			extended_rect.Height += 32;
			break;
		case 3:
			extended_rect.X -= 32;
			extended_rect.Width += 32;
			break;
		case 1:
			extended_rect.Width += 32;
			break;
		}
		foreach (NPC character in (Game1.CurrentEvent != null) ? Game1.CurrentEvent.actors : location.characters.ToList())
		{
			Rectangle character_rect = character.GetBoundingBox();
			if (character_rect.Intersects(rect))
			{
				return true;
			}
			if (!character.isMovingOnPathFindPath.Value && character_rect.Intersects(extended_rect))
			{
				return true;
			}
		}
		return false;
	}

	public bool IsSittingHere(Farmer who)
	{
		return this.sittingFarmers.ContainsKey(who.UniqueMultiplayerID);
	}

	public bool HasSittingFarmers()
	{
		return this.sittingFarmers.Length > 0;
	}

	public List<Vector2> GetSeatPositions(bool ignore_offsets = false)
	{
		this.customDrawValues = null;
		List<Vector2> seat_positions = new List<Vector2>();
		string value = this.seatType.Value;
		if (!(value == "playground"))
		{
			if (value == "ccdesk")
			{
				Vector2 seat = new Vector2(this.tilePosition.X + 0.5f, this.tilePosition.Y);
				if (!ignore_offsets)
				{
					seat.Y -= 0.4f;
				}
				seat_positions.Add(seat);
			}
			else if (this.seatType.Value.StartsWith("custom "))
			{
				float offset_x = 0f;
				float offset_y = 0f;
				float extra_height = 0f;
				string[] custom_values = ArgUtility.SplitBySpace(this.seatType.Value);
				try
				{
					if (custom_values.Length > 1)
					{
						offset_x = float.Parse(custom_values[1]);
					}
					if (custom_values.Length > 2)
					{
						offset_y = float.Parse(custom_values[2]);
					}
					if (custom_values.Length > 3)
					{
						extra_height = float.Parse(custom_values[3]);
					}
				}
				catch (Exception)
				{
				}
				this.customDrawValues = new Vector3(offset_x, offset_y, extra_height);
				Vector2 seat2 = new Vector2(this.tilePosition.X + this.customDrawValues.Value.X, this.tilePosition.Y);
				if (!ignore_offsets)
				{
					seat2.Y += this.customDrawValues.Value.Y;
				}
				seat_positions.Add(seat2);
			}
			else
			{
				for (int x = 0; (float)x < this.size.X; x++)
				{
					for (int y = 0; (float)y < this.size.Y; y++)
					{
						Vector2 offset = new Vector2(0f, 0f);
						if (this.seatType.Value.StartsWith("bench"))
						{
							if (this.direction.Value == 2)
							{
								offset.Y += 0.25f;
							}
							else if ((this.direction.Value == 3 || this.direction.Value == 1) && y == 0)
							{
								offset.Y += 0.5f;
							}
						}
						if (this.seatType.Value.StartsWith("picnic"))
						{
							switch (this.direction.Value)
							{
							case 2:
								offset.Y -= 0.25f;
								break;
							case 0:
								offset.Y += 0.25f;
								break;
							}
						}
						if (this.seatType.Value.EndsWith("swings"))
						{
							offset.Y -= 0.5f;
						}
						else if (this.seatType.Value.EndsWith("summitbench"))
						{
							offset.Y -= 0.2f;
						}
						else if (this.seatType.Value.EndsWith("tall"))
						{
							offset.Y -= 0.3f;
						}
						else if (this.seatType.Value.EndsWith("short"))
						{
							offset.Y += 0.3f;
						}
						if (ignore_offsets)
						{
							offset = Vector2.Zero;
						}
						seat_positions.Add(this.tilePosition.Value + new Vector2((float)x + offset.X, (float)y + offset.Y));
					}
				}
			}
		}
		else
		{
			Vector2 seat3 = new Vector2(this.tilePosition.X + 0.75f, this.tilePosition.Y);
			if (!ignore_offsets)
			{
				seat3.Y -= 0.1f;
			}
			seat_positions.Add(seat3);
		}
		return seat_positions;
	}

	public virtual void Draw(SpriteBatch b)
	{
		if (this._loadedTextureFile != this.textureFile.Value)
		{
			this._loadedTextureFile = this.textureFile.Value;
			try
			{
				this.overlayTexture = Game1.content.Load<Texture2D>(this._loadedTextureFile);
			}
			catch (Exception)
			{
				this.overlayTexture = null;
			}
		}
		if (this.overlayTexture == null)
		{
			this.overlayTexture = MapSeat.mapChairTexture;
		}
		if (this.drawTilePosition.Value.X >= 0f && this.HasSittingFarmers())
		{
			float extra_height = 0f;
			if (this.customDrawValues.HasValue)
			{
				extra_height = this.customDrawValues.Value.Z;
			}
			else if (this.seatType.Value.StartsWith("highback_chair") || this.seatType.Value.StartsWith("ccdesk"))
			{
				extra_height = 1f;
			}
			Vector2 draw_position = Game1.GlobalToLocal(Game1.viewport, new Vector2(this.tilePosition.X * 64f, (this.tilePosition.Y - extra_height) * 64f));
			float sort_layer = (float)(((double)((float)(int)this.tilePosition.Y + this.size.Y) + 0.1) * 64.0) / 10000f;
			Rectangle source_rect = new Rectangle((int)this.drawTilePosition.Value.X * 16, (int)(this.drawTilePosition.Value.Y - extra_height) * 16, (int)this.size.Value.X * 16, (int)(this.size.Value.Y + extra_height) * 16);
			if (this.seasonal.Value)
			{
				source_rect.X += source_rect.Width * Game1.currentLocation.GetSeasonIndex();
			}
			b.Draw(this.overlayTexture, draw_position, source_rect, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, sort_layer);
		}
	}

	public bool OccupiesTile(int x, int y)
	{
		return this.GetSeatBounds().Contains(x, y);
	}

	public virtual Vector2? AddSittingFarmer(Farmer who)
	{
		if (who == Game1.player)
		{
			this.localSittingDirection = this.direction.Value;
			if (this.seatType.Value.StartsWith("stool"))
			{
				this.localSittingDirection = Game1.player.FacingDirection;
			}
			if (this.direction.Value == -2)
			{
				this.localSittingDirection = Utility.GetOppositeFacingDirection(Game1.player.FacingDirection);
			}
			if (this.seatType.Value.StartsWith("bathchair") && this.localSittingDirection == 0)
			{
				this.localSittingDirection = 2;
			}
		}
		List<Vector2> seat_positions = this.GetSeatPositions();
		if (seat_positions.Count == 0)
		{
			return null;
		}
		this.CheckSeatOccupancyIfTemporaryMap(who, seat_positions, out var overrideSeatsFilled);
		if (overrideSeatsFilled.All((bool occupied) => occupied))
		{
			return null;
		}
		int seat_index = -1;
		Vector2? sit_position = null;
		float distance = 96f;
		for (int i = 0; i < seat_positions.Count; i++)
		{
			if (!this.sittingFarmers.Values.Contains(i) && !overrideSeatsFilled[i])
			{
				float curr_distance = ((seat_positions[i] + new Vector2(0.5f, 0.5f)) * 64f - who.getStandingPosition()).Length();
				if (curr_distance < distance)
				{
					distance = curr_distance;
					sit_position = seat_positions[i];
					seat_index = i;
				}
			}
		}
		if (sit_position.HasValue)
		{
			this.sittingFarmers[who.UniqueMultiplayerID] = seat_index;
		}
		return sit_position;
	}

	public bool IsSeatHere(GameLocation location)
	{
		return location.mapSeats.Contains(this);
	}

	public int GetSittingDirection()
	{
		return this.localSittingDirection;
	}

	public Vector2? GetSittingPosition(Farmer who, bool ignore_offsets = false)
	{
		if (this.sittingFarmers.TryGetValue(who.UniqueMultiplayerID, out var index))
		{
			return this.GetSeatPositions(ignore_offsets)[index];
		}
		return null;
	}

	public virtual Rectangle GetSeatBounds()
	{
		if (this.seatType.Value == "chair" && this.direction.Value == 0)
		{
			new Rectangle((int)this.tilePosition.X, (int)this.tilePosition.Y + 1, (int)this.size.X, (int)this.size.Y - 1);
		}
		return new Rectangle((int)this.tilePosition.X, (int)this.tilePosition.Y, (int)this.size.X, (int)this.size.Y);
	}

	public virtual void RemoveSittingFarmer(Farmer farmer)
	{
		this.sittingFarmers.Remove(farmer.UniqueMultiplayerID);
	}

	public virtual int GetSittingFarmerCount()
	{
		return this.sittingFarmers.Length;
	}

	/// <summary>Manually check seat occupancy if we're in a non-synced temporary location (e.g. for an event or festival).</summary>
	/// <param name="who">The player for which to load seats.</param>
	/// <param name="seatPositions">The tile positions containing seats.</param>
	/// <param name="seatsFilled">The flags which indicate whether each available seat is occupied.</param>
	private void CheckSeatOccupancyIfTemporaryMap(Farmer who, List<Vector2> seatPositions, out bool[] seatsFilled)
	{
		seatsFilled = new bool[seatPositions.Count];
		GameLocation location = who.currentLocation;
		if (location == null || !location.IsTemporary)
		{
			return;
		}
		FarmerCollection playersHere = location.farmers ?? Game1.getOnlineFarmers();
		if (playersHere.Count <= 1)
		{
			return;
		}
		List<Vector2> seatTilePositions = this.GetSeatPositions(ignore_offsets: true);
		Vector2 minPosition = seatTilePositions[0];
		Vector2 maxPosition = seatTilePositions[0];
		for (int i = 1; i < seatTilePositions.Count; i++)
		{
			Vector2 seatPosition = seatTilePositions[i];
			Vector2.Min(ref minPosition, ref seatPosition, out minPosition);
			Vector2.Max(ref maxPosition, ref seatPosition, out maxPosition);
		}
		minPosition -= new Vector2(1E-05f, 1E-05f);
		maxPosition += new Vector2(1E-05f, 1E-05f);
		int remaining = seatTilePositions.Count;
		foreach (Farmer farmer in playersHere)
		{
			if (!farmer.isSitting.Value || farmer.uniqueMultiplayerID == who.uniqueMultiplayerID)
			{
				continue;
			}
			Vector2 sitPosition = farmer.mapChairSitPosition.Value;
			if (!(sitPosition.X > minPosition.X) || !(sitPosition.X < maxPosition.X) || !(sitPosition.Y > minPosition.Y) || !(sitPosition.Y < maxPosition.Y))
			{
				continue;
			}
			for (int j = 0; j < seatTilePositions.Count; j++)
			{
				if (!seatsFilled[j])
				{
					Vector2 diff = seatTilePositions[j] - sitPosition;
					if (Math.Abs(diff.X) < 1E-05f && Math.Abs(diff.Y) < 1E-05f)
					{
						seatsFilled[j] = true;
						remaining--;
						break;
					}
				}
			}
			if (remaining == 0)
			{
				break;
			}
		}
	}
}
