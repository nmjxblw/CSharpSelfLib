using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Extensions;
using StardewValley.GameData;

namespace StardewValley.Locations;

public class IslandSouthEast : IslandLocation
{
	private const string lightId = "IslandSouthEast";

	[XmlIgnore]
	public Texture2D mermaidSprites;

	[XmlIgnore]
	public int lastPlayedNote = -1;

	[XmlIgnore]
	public int songIndex = -1;

	[XmlIgnore]
	public int[] mermaidIdle = new int[1];

	[XmlIgnore]
	public int[] mermaidWave = new int[4] { 1, 1, 2, 2 };

	[XmlIgnore]
	public int[] mermaidReward = new int[7] { 3, 3, 3, 3, 3, 4, 4 };

	[XmlIgnore]
	public int[] mermaidDance = new int[6] { 5, 5, 5, 6, 6, 6 };

	[XmlIgnore]
	public int mermaidFrameIndex;

	[XmlIgnore]
	public int[] currentMermaidAnimation;

	[XmlIgnore]
	public float mermaidFrameTimer;

	[XmlIgnore]
	public float mermaidDanceTime;

	[XmlIgnore]
	public NetEvent0 mermaidPuzzleSuccess = new NetEvent0();

	[XmlElement("mermaidPuzzleFinished")]
	public NetBool mermaidPuzzleFinished = new NetBool();

	[XmlIgnore]
	public NetEvent0 fishWalnutEvent = new NetEvent0();

	[XmlElement("fishedWalnut")]
	public NetBool fishedWalnut = new NetBool();

	public IslandSouthEast()
	{
	}

	public IslandSouthEast(string map, string name)
		: base(map, name)
	{
	}

	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(this.mermaidPuzzleSuccess, "mermaidPuzzleSuccess").AddField(this.mermaidPuzzleFinished, "mermaidPuzzleFinished").AddField(this.fishWalnutEvent, "fishWalnutEvent")
			.AddField(this.fishedWalnut, "fishedWalnut");
		this.mermaidPuzzleSuccess.onEvent += OnMermaidPuzzleSuccess;
		this.fishWalnutEvent.onEvent += OnFishWalnut;
	}

	public virtual void OnMermaidPuzzleSuccess()
	{
		this.currentMermaidAnimation = this.mermaidReward;
		this.mermaidFrameTimer = 0f;
		if (Game1.currentLocation == this)
		{
			Game1.playSound("yoba");
		}
		if (Game1.IsMasterGame && !this.mermaidPuzzleFinished.Value)
		{
			Game1.player.team.MarkCollectedNut("Mermaid");
			this.mermaidPuzzleFinished.Value = true;
			for (int i = 0; i < 5; i++)
			{
				Game1.createItemDebris(ItemRegistry.Create("(O)73"), new Vector2(32f, 33f) * 64f, 0, this, 0);
			}
		}
	}

	public override void MakeMapModifications(bool force = false)
	{
		base.MakeMapModifications(force);
		if (base.IsRainingHere())
		{
			base.setMapTile(16, 27, 3, "Back", "untitled tile sheet3", "");
			base.setMapTile(18, 27, 4, "Back", "untitled tile sheet3", "");
			base.setMapTile(20, 27, 5, "Back", "untitled tile sheet3", "");
			base.setMapTile(22, 27, 6, "Back", "untitled tile sheet3", "");
			base.setMapTile(24, 27, 7, "Back", "untitled tile sheet3", "");
			base.setMapTile(26, 27, 8, "Back", "untitled tile sheet3", "");
		}
		else
		{
			base.setMapTile(16, 27, 39, "Back", "untitled tile sheet", "");
			base.setMapTile(18, 27, 39, "Back", "untitled tile sheet", "");
			base.setMapTile(20, 27, 39, "Back", "untitled tile sheet", "");
			base.setMapTile(22, 27, 39, "Back", "untitled tile sheet", "");
			base.setMapTile(24, 27, 39, "Back", "untitled tile sheet", "");
			base.setMapTile(26, 27, 39, "Back", "untitled tile sheet", "");
		}
		if (IslandSouthEastCave.isPirateNight())
		{
			base.setMapTile(29, 18, 36, "Buildings", "untitled tile sheet3");
			base.setTileProperty(29, 18, "Buildings", "Passable", "T");
			base.setMapTile(29, 19, 68, "Buildings", "untitled tile sheet3");
			base.setTileProperty(29, 19, "Buildings", "Passable", "T");
			base.setMapTile(30, 18, 99, "Buildings", "untitled tile sheet3");
			base.setTileProperty(30, 18, "Buildings", "Passable", "T");
			base.setMapTile(30, 19, 131, "Buildings", "untitled tile sheet3");
			base.setTileProperty(30, 19, "Buildings", "Passable", "T");
		}
		else
		{
			base.setMapTile(29, 18, 35, "Buildings", "untitled tile sheet3");
			base.setTileProperty(29, 18, "Buildings", "Passable", "T");
			base.setMapTile(29, 19, 67, "Buildings", "untitled tile sheet3");
			base.setTileProperty(29, 19, "Buildings", "Passable", "T");
			base.setMapTile(30, 18, 35, "Buildings", "untitled tile sheet3");
			base.setTileProperty(30, 18, "Buildings", "Passable", "T");
			base.setMapTile(30, 19, 67, "Buildings", "untitled tile sheet3");
			base.setTileProperty(30, 19, "Buildings", "Passable", "T");
		}
	}

	protected override void resetLocalState()
	{
		base.resetLocalState();
		this.mermaidSprites = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\temporary_sprites_1");
		if (IslandSouthEastCave.isPirateNight())
		{
			Game1.changeMusicTrack("PIRATE_THEME(muffled)", track_interruptable: true, MusicContext.SubLocation);
			if (!base.hasLightSource("IslandSouthEast"))
			{
				base.sharedLights.AddLight(new LightSource("IslandSouthEast", 1, new Vector2(30.5f, 18.5f) * 64f, 4f, LightSource.LightContext.None, 0L, base.NameOrUniqueName));
			}
		}
		if (base.AreMoonlightJelliesOut())
		{
			base.addMoonlightJellies(50, Utility.CreateRandom(Game1.stats.DaysPlayed, Game1.uniqueIDForThisGame, -24917.0), new Rectangle(0, 0, 0, 0));
		}
	}

	public override void cleanupBeforePlayerExit()
	{
		base.removeLightSource("IslandSouthEast");
		base.cleanupBeforePlayerExit();
	}

	public override void SetBuriedNutLocations()
	{
		base.SetBuriedNutLocations();
		base.buriedNutPoints.Add(new Point(25, 17));
	}

	public override void UpdateWhenCurrentLocation(GameTime time)
	{
		base.UpdateWhenCurrentLocation(time);
		this.mermaidPuzzleSuccess.Poll();
		this.fishWalnutEvent.Poll();
		if (!this.fishedWalnut.Value && Game1.random.NextDouble() < 0.005)
		{
			base.playSound("waterSlosh");
			base.temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 0, 64, 64), 150f, 8, 0, new Vector2(1216f, 1344f), flicker: false, Game1.random.NextBool(), 0.001f, 0.01f, Color.White, 1f, 0.003f, 0f, 0f));
		}
		if (!this.MermaidIsHere())
		{
			return;
		}
		bool should_wave = false;
		if (this.mermaidPuzzleFinished.Value)
		{
			foreach (Farmer farmer in base.farmers)
			{
				Point point = farmer.TilePoint;
				if (point.X > 24 && point.Y > 25)
				{
					should_wave = true;
					break;
				}
			}
		}
		if (should_wave && (this.currentMermaidAnimation == null || this.currentMermaidAnimation == this.mermaidIdle))
		{
			this.currentMermaidAnimation = this.mermaidWave;
			this.mermaidFrameIndex = 0;
			this.mermaidFrameTimer = 0f;
		}
		if (this.mermaidDanceTime > 0f)
		{
			if (this.currentMermaidAnimation == null || this.currentMermaidAnimation == this.mermaidIdle)
			{
				this.currentMermaidAnimation = this.mermaidDance;
				this.mermaidFrameTimer = 0f;
			}
			this.mermaidDanceTime -= (float)time.ElapsedGameTime.TotalSeconds;
			if (this.mermaidDanceTime < 0f && this.currentMermaidAnimation == this.mermaidDance)
			{
				this.currentMermaidAnimation = this.mermaidIdle;
				this.mermaidFrameTimer = 0f;
			}
		}
		this.mermaidFrameTimer += (float)time.ElapsedGameTime.TotalSeconds;
		if (!(this.mermaidFrameTimer > 0.25f))
		{
			return;
		}
		this.mermaidFrameTimer = 0f;
		this.mermaidFrameIndex++;
		if (this.currentMermaidAnimation == null)
		{
			this.mermaidFrameIndex = 0;
		}
		else
		{
			if (this.mermaidFrameIndex < this.currentMermaidAnimation.Length)
			{
				return;
			}
			this.mermaidFrameIndex = 0;
			if (this.currentMermaidAnimation == this.mermaidReward)
			{
				if (should_wave)
				{
					this.currentMermaidAnimation = this.mermaidWave;
				}
				else
				{
					this.currentMermaidAnimation = this.mermaidIdle;
				}
			}
			else if (!should_wave && this.currentMermaidAnimation == this.mermaidWave)
			{
				this.currentMermaidAnimation = this.mermaidIdle;
			}
		}
	}

	public bool MermaidIsHere()
	{
		return base.IsRainingHere();
	}

	public override void draw(SpriteBatch b)
	{
		base.draw(b);
		if (this.MermaidIsHere())
		{
			int frame = 0;
			if (this.mermaidFrameIndex < this.currentMermaidAnimation?.Length)
			{
				frame = this.currentMermaidAnimation[this.mermaidFrameIndex];
			}
			b.Draw(this.mermaidSprites, Game1.GlobalToLocal(new Vector2(32f, 32f) * 64f + new Vector2(0f, -8f) * 4f), new Rectangle(304 + 28 * frame, 592, 28, 36), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.0009f);
		}
	}

	public override Item getFish(float millisecondsAfterNibble, string bait, int waterDepth, Farmer who, double baitPotency, Vector2 bobberTile, string locationName = null)
	{
		if ((int)bobberTile.X >= 18 && (int)bobberTile.X <= 20 && (int)bobberTile.Y >= 20 && (int)bobberTile.Y <= 22)
		{
			if (!this.fishedWalnut.Value)
			{
				Game1.player.team.MarkCollectedNut("StardropPool");
				if (!Game1.IsMultiplayer)
				{
					this.fishedWalnut.Value = true;
					return ItemRegistry.Create("(O)73");
				}
				this.fishWalnutEvent.Fire();
			}
			return null;
		}
		return base.getFish(millisecondsAfterNibble, bait, waterDepth, who, baitPotency, bobberTile);
	}

	public void OnFishWalnut()
	{
		if (!this.fishedWalnut.Value && Game1.IsMasterGame)
		{
			Vector2 tile = new Vector2(19f, 21f);
			Game1.createItemDebris(ItemRegistry.Create("(O)73"), tile * 64f + new Vector2(0.5f, 1.5f) * 64f, 0, this, 0);
			Game1.multiplayer.broadcastSprites(this, new TemporaryAnimatedSprite(28, 100f, 2, 1, tile * 64f, flicker: false, flipped: false)
			{
				layerDepth = ((tile.Y + 0.5f) * 64f + 2f) / 10000f
			});
			base.playSound("dropItemInWater");
			this.fishedWalnut.Value = true;
		}
	}

	public override void TransferDataFromSavedLocation(GameLocation l)
	{
		base.TransferDataFromSavedLocation(l);
		if (l is IslandSouthEast islandSouthEast)
		{
			this.mermaidPuzzleFinished.Value = islandSouthEast.mermaidPuzzleFinished.Value;
			this.fishedWalnut.Value = islandSouthEast.fishedWalnut.Value;
		}
	}

	public virtual void OnFlutePlayed(int pitch)
	{
		if (!this.MermaidIsHere())
		{
			return;
		}
		if (this.songIndex == -1)
		{
			this.lastPlayedNote = pitch;
			this.songIndex = 0;
		}
		int relative_pitch = pitch - this.lastPlayedNote;
		if (relative_pitch == 900)
		{
			this.songIndex = 1;
			this.mermaidDanceTime = 5f;
		}
		else
		{
			switch (this.songIndex)
			{
			case 1:
				if (relative_pitch == -200)
				{
					this.songIndex++;
					this.mermaidDanceTime = 5f;
				}
				else
				{
					this.songIndex = -1;
					this.mermaidDanceTime = 0f;
					this.currentMermaidAnimation = this.mermaidIdle;
				}
				break;
			case 2:
				if (relative_pitch == -400)
				{
					this.songIndex++;
					this.mermaidDanceTime = 5f;
				}
				else
				{
					this.songIndex = -1;
					this.mermaidDanceTime = 0f;
					this.currentMermaidAnimation = this.mermaidIdle;
				}
				break;
			case 3:
				if (relative_pitch == 200)
				{
					this.songIndex = 0;
					this.mermaidPuzzleSuccess.Fire();
					this.mermaidDanceTime = 0f;
				}
				else
				{
					this.songIndex = -1;
					this.mermaidDanceTime = 0f;
					this.currentMermaidAnimation = this.mermaidIdle;
				}
				break;
			}
		}
		this.lastPlayedNote = pitch;
	}
}
