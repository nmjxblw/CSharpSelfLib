using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Network;
using StardewValley.TerrainFeatures;
using xTile.Dimensions;

namespace StardewValley.Locations;

public class IslandFarmCave : IslandLocation
{
	[XmlIgnore]
	public NPC gourmand;

	[XmlElement("gourmandRequestsFulfilled")]
	public NetInt gourmandRequestsFulfilled = new NetInt();

	[XmlIgnore]
	public NetEvent0 requestGourmandCheckEvent = new NetEvent0();

	[XmlIgnore]
	public NetEvent1Field<string, NetString> gourmandResponseEvent = new NetEvent1Field<string, NetString>();

	[XmlIgnore]
	public bool triggeredGourmand;

	[XmlIgnore]
	public static int TOTAL_GOURMAND_REQUESTS = 3;

	[XmlIgnore]
	private NetMutex gourmandMutex = new NetMutex();

	private Texture2D smokeTexture;

	private float smokeTimer;

	public IslandFarmCave()
	{
	}

	public IslandFarmCave(string map, string name)
		: base(map, name)
	{
	}

	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(this.gourmandRequestsFulfilled, "gourmandRequestsFulfilled").AddField(this.requestGourmandCheckEvent, "requestGourmandCheckEvent").AddField(this.gourmandResponseEvent, "gourmandResponseEvent")
			.AddField(this.gourmandMutex.NetFields, "gourmandMutex.NetFields");
		this.requestGourmandCheckEvent.onEvent += OnRequestGourmandCheck;
		this.gourmandResponseEvent.onEvent += OnGourmandResponse;
	}

	public virtual void OnRequestGourmandCheck()
	{
		if (!Game1.IsMasterGame)
		{
			return;
		}
		string gourmand_response = "";
		IslandWest island_farm = Game1.RequireLocation<IslandWest>("IslandWest");
		foreach (Vector2 key in island_farm.terrainFeatures.Keys)
		{
			if (!(island_farm.terrainFeatures[key] is HoeDirt { crop: not null } dirt))
			{
				continue;
			}
			bool harvestable = dirt.crop.currentPhase.Value >= dirt.crop.phaseDays.Count - 1 && (!dirt.crop.fullyGrown.Value || dirt.crop.dayOfCurrentPhase.Value <= 0);
			if (dirt.crop.indexOfHarvest.Value == this.IndexForRequest(this.gourmandRequestsFulfilled.Value))
			{
				if (harvestable)
				{
					Point target_tile = new Point((int)key.X, (int)key.Y);
					Point player_tile = this.FindNearbyUnoccupiedTileThatFitsCharacter(island_farm, target_tile.X, target_tile.Y);
					Point gourmand_tile = this.FindNearbyUnoccupiedTileThatFitsCharacter(island_farm, target_tile.X, target_tile.Y, 2, player_tile);
					int farmer_direction = this.GetRelativeDirection(player_tile, target_tile);
					this.gourmandResponseEvent.Fire(key.X + " " + key.Y + " " + player_tile.X + " " + player_tile.Y + " " + farmer_direction + " " + gourmand_tile.X + " " + gourmand_tile.Y + " 2");
					return;
				}
				gourmand_response = "inProgress";
			}
		}
		this.gourmandResponseEvent.Fire(gourmand_response);
	}

	public int GetRelativeDirection(Point source, Point destination)
	{
		Point offset = new Point(destination.X - source.X, destination.Y - source.Y);
		if (Math.Abs(offset.Y) > Math.Abs(offset.X))
		{
			if (offset.Y < 0)
			{
				return 0;
			}
			return 2;
		}
		if (offset.X < 0)
		{
			return 3;
		}
		return 1;
	}

	public Point FindNearbyUnoccupiedTileThatFitsCharacter(GameLocation location, int target_x, int target_y, int width = 1, Point? invalid_tile = null)
	{
		HashSet<Point> visited_tiles = new HashSet<Point>();
		List<Point> open_tiles = new List<Point>();
		open_tiles.Add(new Point(target_x, target_y));
		visited_tiles.Add(new Point(target_x, target_y));
		Point[] offsets = new Point[4]
		{
			new Point(-1, 0),
			new Point(1, 0),
			new Point(0, -1),
			new Point(0, 1)
		};
		for (int i = 0; i < 500; i++)
		{
			if (open_tiles.Count == 0)
			{
				break;
			}
			Point tile = open_tiles[0];
			open_tiles.RemoveAt(0);
			Point[] array = offsets;
			for (int j = 0; j < array.Length; j++)
			{
				Point offset = array[j];
				Point next_tile = new Point(tile.X + offset.X, tile.Y + offset.Y);
				if (!visited_tiles.Contains(next_tile))
				{
					open_tiles.Add(next_tile);
				}
			}
			if (visited_tiles.Contains(tile) || (invalid_tile.HasValue && tile.X == invalid_tile.Value.X && tile.Y == invalid_tile.Value.Y))
			{
				continue;
			}
			visited_tiles.Add(tile);
			bool fail = false;
			int height = 1;
			for (int w = 0; w < width; w++)
			{
				for (int h = 0; h < height; h++)
				{
					Point checked_tile = new Point(tile.X + w, tile.Y + h);
					new Microsoft.Xna.Framework.Rectangle(checked_tile.X * 64, checked_tile.Y * 64, 64, 64).Inflate(-4, -4);
					if (checked_tile.X == target_x && checked_tile.Y == target_y + 1)
					{
						fail = true;
						break;
					}
					if (invalid_tile.HasValue && invalid_tile.Value == checked_tile)
					{
						fail = true;
						break;
					}
					if (location.IsTileOccupiedBy(new Vector2(checked_tile.X, checked_tile.Y), CollisionMask.All, CollisionMask.TerrainFeatures))
					{
						fail = true;
						break;
					}
				}
			}
			if (!fail)
			{
				return tile;
			}
		}
		return new Point(target_x, target_y);
	}

	public virtual void OnGourmandResponse(string response)
	{
		if (Game1.currentLocation != this)
		{
			return;
		}
		if (response == null || response.Length != 0)
		{
			if (response == "inProgress")
			{
				Game1.player.freezePause = 0;
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Gourmand_InProgress"));
			}
			else
			{
				string[] split = ArgUtility.SplitBySpace(response);
				StringBuilder sb = new StringBuilder();
				sb.Append("none/-1000 -1000/");
				sb.Append("farmer " + split[2] + " " + split[3] + " " + split[4] + "/");
				sb.Append("changeLocation IslandWest/");
				sb.Append("viewport " + split[0] + " " + split[1] + "/");
				sb.Append("playMusic none/addTemporaryActor Gourmand 32 32 " + split[5] + " " + split[6] + " " + split[7] + " true character/positionOffset Gourmand 0 1/positionOffset farmer 0 1/animate Gourmand false true 500 2 3/");
				sb.Append("viewport " + split[0] + " " + split[1] + " true/");
				sb.Append("pause 3000/playSound croak/");
				string[] array = Game1.content.LoadString("Strings\\Locations:Gourmand_Request_" + this.gourmandRequestsFulfilled.Value + "_Success").Split('|');
				foreach (string text in array)
				{
					sb.Append("message \"" + text + "\"/pause 250/");
				}
				sb.Append("pause 1000/end");
				Event evt = new Event(sb.ToString());
				if (this.triggeredGourmand)
				{
					Event obj = evt;
					obj.onEventFinished = (Action)Delegate.Combine(obj.onEventFinished, (Action)delegate
					{
						if (Game1.locationRequest != null)
						{
							Game1.locationRequest.OnWarp += CompleteGourmandRequest;
						}
						else
						{
							this.CompleteGourmandRequest();
						}
					});
				}
				Game1.globalFadeToBlack(delegate
				{
					Game1.currentLocation.startEvent(evt);
				});
				Game1.player.freezePause = 0;
			}
		}
		else if (this.triggeredGourmand)
		{
			Game1.player.freezePause = 0;
			this.ShowGourmandUnhappy();
		}
		this.triggeredGourmand = false;
	}

	public virtual void CompleteGourmandRequest()
	{
		if (!this.gourmandMutex.IsLockHeld())
		{
			return;
		}
		Game1.player.freezePause = 1250;
		DelayedAction.functionAfterDelay(delegate
		{
			Game1.playSound("croak");
			this.gourmand.shake(1000);
			Game1.afterDialogues = (Game1.afterFadeFunction)Delegate.Combine(Game1.afterDialogues, new Game1.afterFadeFunction(GiveReward));
			if (this.gourmandRequestsFulfilled.Value < IslandFarmCave.TOTAL_GOURMAND_REQUESTS - 1)
			{
				Game1.multipleDialogues(Game1.content.LoadString("Strings\\Locations:Gourmand_Reward").Split('|'));
			}
			else
			{
				Game1.multipleDialogues(Game1.content.LoadString("Strings\\Locations:Gourmand_LastReward").Split('|'));
			}
		}, 1000);
	}

	public virtual void GiveReward()
	{
		Game1.createItemDebris(ItemRegistry.Create("(O)73"), new Vector2(4.5f, 4f) * 64f, 3, this);
		for (int i = 0; i < 4; i++)
		{
			Game1.createItemDebris(ItemRegistry.Create("(O)73"), new Vector2(4.5f, 4f) * 64f, 1, this);
		}
		this.gourmandRequestsFulfilled.Value++;
		Game1.player.team.MarkCollectedNut("IslandGourmand" + this.gourmandRequestsFulfilled.Value);
		this.gourmandMutex.ReleaseLock();
	}

	public void ShowGourmandUnhappy()
	{
		Game1.playSound("croak");
		this.gourmand.shake(1000);
		Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Gourmand_RequestFailed"));
		if (this.gourmandMutex.IsLockHeld())
		{
			this.gourmandMutex.ReleaseLock();
		}
	}

	protected override void resetLocalState()
	{
		base.resetLocalState();
		this.gourmand = new NPC(new AnimatedSprite("Characters\\Gourmand", 0, 32, 32), new Vector2(4f, 4f) * 64f, "IslandFarmCave", 2, "Gourmand", datable: false, Game1.content.Load<Texture2D>("Portraits\\SafariGuy"));
		this.gourmand.AllowDynamicAppearance = false;
		this.smokeTexture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1");
		base.waterColor.Value = new Color(10, 250, 120);
	}

	public override void draw(SpriteBatch b)
	{
		base.draw(b);
		if (this.gourmand != null && !Game1.eventUp)
		{
			this.gourmand.draw(b);
		}
		if (this.gourmandRequestsFulfilled.Value < IslandFarmCave.TOTAL_GOURMAND_REQUESTS)
		{
			Point standingPixel = this.gourmand.StandingPixel;
			float yOffset = 4f * (float)Math.Round(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 250.0), 2);
			b.Draw(Game1.mouseCursors2, Game1.GlobalToLocal(Game1.viewport, new Vector2(standingPixel.X, (float)(standingPixel.Y - 128 - 8) + yOffset)), new Microsoft.Xna.Framework.Rectangle(114, 53, 6, 10), Color.White, 0f, new Vector2(1f, 4f), 4f, SpriteEffects.None, 1f);
		}
	}

	public override void DayUpdate(int dayOfMonth)
	{
		this.gourmandMutex.ReleaseLock();
		base.DayUpdate(dayOfMonth);
	}

	public override void UpdateWhenCurrentLocation(GameTime time)
	{
		base.UpdateWhenCurrentLocation(time);
		if (this.gourmand != null)
		{
			this.gourmand.update(time, this);
			if (time.TotalGameTime.TotalMilliseconds % 1000.0 < 500.0)
			{
				this.gourmand.Sprite.CurrentFrame = 1;
			}
			else
			{
				this.gourmand.Sprite.CurrentFrame = 0;
			}
		}
		this.requestGourmandCheckEvent.Poll();
		this.gourmandResponseEvent.Poll();
		this.smokeTimer -= time.ElapsedGameTime.Milliseconds;
		if (this.smokeTimer <= 0f && this.smokeTexture != null)
		{
			base.temporarySprites.Add(new TemporaryAnimatedSprite
			{
				texture = this.smokeTexture,
				sourceRect = new Microsoft.Xna.Framework.Rectangle(0, 180, 9, 11),
				sourceRectStartingPos = new Vector2(0f, 180f),
				layerDepth = 1f,
				interval = 250f,
				position = new Vector2(2f, 4f) * 64f + new Vector2(5f, 5f) * 4f,
				scale = 4f,
				scaleChange = 0.005f,
				alpha = 0.75f,
				alphaFade = 0.005f,
				motion = new Vector2(0f, -0.5f),
				acceleration = new Vector2((float)(Game1.random.NextDouble() - 0.5) / 100f, 0f),
				animationLength = 3,
				holdLastFrame = true
			});
			base.temporarySprites.Add(new TemporaryAnimatedSprite
			{
				texture = this.smokeTexture,
				sourceRect = new Microsoft.Xna.Framework.Rectangle(0, 180, 9, 11),
				sourceRectStartingPos = new Vector2(0f, 180f),
				layerDepth = 1f,
				interval = 250f,
				position = new Vector2(7f, 4f) * 64f + new Vector2(5f, 5f) * 4f,
				scale = 4f,
				scaleChange = 0.005f,
				alpha = 0.75f,
				alphaFade = 0.005f,
				motion = new Vector2(0f, -0.5f),
				acceleration = new Vector2((float)(Game1.random.NextDouble() - 0.5) / 100f, 0f),
				animationLength = 3,
				holdLastFrame = true
			});
			this.smokeTimer = 1250f;
		}
	}

	public override void updateEvenIfFarmerIsntHere(GameTime time, bool ignoreWasUpdatedFlush = false)
	{
		base.updateEvenIfFarmerIsntHere(time, ignoreWasUpdatedFlush);
		this.gourmandMutex.Update(Game1.getOnlineFarmers());
	}

	public virtual void TalkToGourmand()
	{
		List<string> dialogue = new List<string>();
		if (this.gourmandRequestsFulfilled.Value >= IslandFarmCave.TOTAL_GOURMAND_REQUESTS)
		{
			dialogue.AddRange(Game1.content.LoadString("Strings\\Locations:Gourmand_Finished").Split('|'));
		}
		else
		{
			bool num = !Game1.player.hasOrWillReceiveMail("talkedToGourmand");
			if (num)
			{
				Game1.addMailForTomorrow("talkedToGourmand", noLetter: true);
				dialogue.AddRange(Game1.content.LoadString("Strings\\Locations:Gourmand_Intro").Split("|"));
				dialogue.AddRange(Game1.content.LoadString("Strings\\Locations:Gourmand_RequestIntro").Split("|"));
			}
			Game1.playSound("croak");
			this.gourmand.shake(1000);
			dialogue.AddRange(Game1.content.LoadString("Strings\\Locations:Gourmand_Request_" + this.gourmandRequestsFulfilled.Value).Split("|"));
			Response[] responses = base.createYesNoResponses();
			if (!num)
			{
				Game1.afterDialogues = delegate
				{
					Game1.afterDialogues = null;
					base.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:Gourmand_RequestQuestion"), responses, "Gourmand");
				};
			}
		}
		Game1.multipleDialogues(dialogue.ToArray());
	}

	public override bool answerDialogueAction(string questionAndAnswer, string[] questionParams)
	{
		if (questionAndAnswer == null)
		{
			return false;
		}
		if (!(questionAndAnswer == "Gourmand_Yes"))
		{
			if (questionAndAnswer == "Gourmand_No")
			{
				return true;
			}
			return base.answerDialogueAction(questionAndAnswer, questionParams);
		}
		this.triggeredGourmand = true;
		Game1.player.freezePause = 3000;
		this.requestGourmandCheckEvent.Fire();
		return true;
	}

	public string IndexForRequest(int request_number)
	{
		return request_number switch
		{
			0 => "254", 
			1 => "262", 
			2 => "248", 
			_ => null, 
		};
	}

	/// <inheritdoc />
	public override bool performAction(string[] action, Farmer who, Location tileLocation)
	{
		if (ArgUtility.Get(action, 0) == "Gourmand")
		{
			this.gourmandMutex.RequestLock(TalkToGourmand);
			return true;
		}
		return base.performAction(action, who, tileLocation);
	}

	public override void TransferDataFromSavedLocation(GameLocation l)
	{
		base.TransferDataFromSavedLocation(l);
		if (l is IslandFarmCave cave)
		{
			this.gourmandRequestsFulfilled.Value = cave.gourmandRequestsFulfilled.Value;
		}
	}
}
