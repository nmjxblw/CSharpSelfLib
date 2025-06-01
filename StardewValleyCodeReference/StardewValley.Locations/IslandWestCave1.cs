using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewValley.Extensions;
using xTile.Dimensions;

namespace StardewValley.Locations;

public class IslandWestCave1 : IslandLocation
{
	public class CaveCrystal
	{
		public Vector2 tileLocation;

		public int id;

		public int pitch;

		public Color color;

		public Color currentColor;

		public float shakeTimer;

		public float glowTimer;

		public void update()
		{
			if (this.glowTimer > 0f)
			{
				this.glowTimer -= (float)Game1.currentGameTime.ElapsedGameTime.TotalMilliseconds;
				this.currentColor.R = (byte)Utility.Lerp((int)this.color.R, 255f, this.glowTimer / 1000f);
				this.currentColor.G = (byte)Utility.Lerp((int)this.color.G, 255f, this.glowTimer / 1000f);
				this.currentColor.B = (byte)Utility.Lerp((int)this.color.B, 255f, this.glowTimer / 1000f);
			}
			if (this.shakeTimer > 0f)
			{
				this.shakeTimer -= (float)Game1.currentGameTime.ElapsedGameTime.TotalMilliseconds;
			}
		}

		public void activate()
		{
			this.glowTimer = 1000f;
			this.shakeTimer = 100f;
			Game1.playSound("crystal", this.pitch);
			this.currentColor = this.color;
		}

		public void draw(SpriteBatch b)
		{
			b.Draw(Game1.mouseCursors2, Game1.GlobalToLocal(this.tileLocation * 64f + new Vector2(8f, 10f) * 4f), new Microsoft.Xna.Framework.Rectangle(188, 228, 52, 28), this.currentColor, 0f, new Vector2(52f, 28f) / 2f, 4f, SpriteEffects.None, (this.tileLocation.Y * 64f + 64f - 8f) / 10000f);
			b.Draw(Game1.mouseCursors2, Game1.GlobalToLocal(this.tileLocation * 64f + new Vector2(0f, -52f) + new Vector2((this.shakeTimer > 0f) ? Game1.random.Next(-1, 2) : 0, (this.shakeTimer > 0f) ? Game1.random.Next(-1, 2) : 0)), new Microsoft.Xna.Framework.Rectangle(240, 227, 16, 29), this.currentColor, 0f, Vector2.Zero, 4f, SpriteEffects.None, (this.tileLocation.Y * 64f + 64f - 4f) / 10000f);
		}
	}

	public const string lightSourceId = "IslandWestCave1";

	[XmlIgnore]
	protected List<CaveCrystal> crystals = new List<CaveCrystal>();

	public const int PHASE_INTRO = 0;

	public const int PHASE_PLAY_SEQUENCE = 1;

	public const int PHASE_WAIT_FOR_PLAYER_INPUT = 2;

	public const int PHASE_NOTHING = 3;

	public const int PHASE_SUCCESSFUL_SEQUENCE = 4;

	public const int PHASE_OUTRO = 5;

	[XmlElement("completed")]
	public NetBool completed = new NetBool();

	[XmlIgnore]
	public NetBool isActivated = new NetBool(value: false);

	[XmlIgnore]
	public NetFloat netPhaseTimer = new NetFloat();

	[XmlIgnore]
	public float localPhaseTimer;

	[XmlIgnore]
	public float betweenNotesTimer;

	[XmlIgnore]
	public int localPhase;

	[XmlIgnore]
	public NetInt netPhase = new NetInt(3);

	[XmlIgnore]
	public NetInt currentDifficulty = new NetInt(2);

	[XmlIgnore]
	public NetInt currentCrystalSequenceIndex = new NetInt(0);

	[XmlIgnore]
	public int currentPlaybackCrystalSequenceIndex;

	[XmlIgnore]
	public NetInt timesFailed = new NetInt(0);

	[XmlIgnore]
	public NetList<int, NetInt> currentCrystalSequence = new NetList<int, NetInt>();

	[XmlIgnore]
	public NetEvent1Field<int, NetInt> enterValueEvent = new NetEvent1Field<int, NetInt>();

	public IslandWestCave1()
	{
	}

	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(this.netPhase, "netPhase").AddField(this.isActivated, "isActivated").AddField(this.currentDifficulty, "currentDifficulty")
			.AddField(this.currentCrystalSequenceIndex, "currentCrystalSequenceIndex")
			.AddField(this.currentCrystalSequence, "currentCrystalSequence")
			.AddField(this.enterValueEvent.NetFields, "enterValueEvent.NetFields")
			.AddField(this.netPhaseTimer, "netPhaseTimer")
			.AddField(this.completed, "completed")
			.AddField(this.timesFailed, "timesFailed");
		this.enterValueEvent.onEvent += enterValue;
		this.isActivated.fieldChangeVisibleEvent += onActivationChanged;
	}

	public IslandWestCave1(string map, string name)
		: base(map, name)
	{
	}

	public void onActivationChanged(NetBool field, bool old_value, bool new_value)
	{
		this.updateActivationVisuals();
	}

	protected override void resetSharedState()
	{
		base.resetSharedState();
		this.resetPuzzle();
	}

	public void resetPuzzle()
	{
		this.isActivated.Value = false;
		this.updateActivationVisuals();
		this.netPhase.Value = 3;
	}

	public override void MakeMapModifications(bool force = false)
	{
		base.MakeMapModifications(force);
		this.UpdateActivationTiles();
	}

	protected override void resetLocalState()
	{
		base.resetLocalState();
		if (this.crystals.Count == 0)
		{
			this.crystals.Add(new CaveCrystal
			{
				tileLocation = new Vector2(3f, 4f),
				color = new Color(220, 0, 255),
				currentColor = new Color(220, 0, 255),
				id = 1,
				pitch = 0
			});
			this.crystals.Add(new CaveCrystal
			{
				tileLocation = new Vector2(4f, 6f),
				color = Color.Lime,
				currentColor = Color.Lime,
				id = 2,
				pitch = 700
			});
			this.crystals.Add(new CaveCrystal
			{
				tileLocation = new Vector2(6f, 7f),
				color = new Color(255, 50, 100),
				currentColor = new Color(255, 50, 100),
				id = 3,
				pitch = 1200
			});
			this.crystals.Add(new CaveCrystal
			{
				tileLocation = new Vector2(8f, 6f),
				color = new Color(0, 200, 255),
				currentColor = new Color(0, 200, 255),
				id = 4,
				pitch = 1400
			});
			this.crystals.Add(new CaveCrystal
			{
				tileLocation = new Vector2(9f, 4f),
				color = new Color(255, 180, 0),
				currentColor = new Color(255, 180, 0),
				id = 5,
				pitch = 1600
			});
		}
		this.updateActivationVisuals();
	}

	/// <inheritdoc />
	public override bool performAction(string[] action, Farmer who, Location tileLocation)
	{
		if (who.IsLocalPlayer)
		{
			string text = ArgUtility.Get(action, 0);
			if (!(text == "Crystal"))
			{
				if (text == "CrystalCaveActivate" && !this.isActivated.Value && !this.completed.Value)
				{
					this.isActivated.Value = true;
					Game1.playSound("openBox");
					this.updateActivationVisuals();
					this.netPhaseTimer.Value = 1200f;
					this.netPhase.Value = 0;
					this.currentDifficulty.Value = 2;
					return true;
				}
			}
			else
			{
				if (!ArgUtility.TryGetInt(action, 1, out var crystalId, out var error, "int crystalId"))
				{
					base.LogTileActionError(action, tileLocation.X, tileLocation.Y, error);
					return false;
				}
				if (this.netPhase.Value == 5 || this.netPhase.Value == 3 || this.netPhase.Value == 2)
				{
					this.enterValueEvent.Fire(crystalId);
					return true;
				}
			}
		}
		return base.performAction(action, who, tileLocation);
	}

	public virtual void updateActivationVisuals()
	{
		if (base.map != null && Game1.gameMode != 6 && Game1.currentLocation == this)
		{
			if (this.isActivated.Value || this.completed.Value)
			{
				Game1.currentLightSources.Add(new LightSource("IslandWestCave1", 1, new Vector2(6.5f, 1f) * 64f, 2f, Color.Black, LightSource.LightContext.None, 0L, base.NameOrUniqueName));
			}
			else
			{
				Utility.removeLightSource("IslandWestCave1");
			}
			this.UpdateActivationTiles();
			if (this.completed.Value)
			{
				this.addCompletionTorches();
			}
		}
	}

	public virtual void UpdateActivationTiles()
	{
		if (base.map != null && Game1.gameMode != 6 && Game1.currentLocation == this)
		{
			int headIndex = ((this.isActivated.Value || this.completed.Value) ? 33 : 31);
			base.setMapTile(6, 1, headIndex, "Buildings", "untitled tile sheet");
		}
	}

	public virtual void enterValue(int which)
	{
		if (this.netPhase.Value == 2 && Game1.IsMasterGame && this.currentCrystalSequence.Count > this.currentCrystalSequenceIndex.Value)
		{
			if (this.currentCrystalSequence[this.currentCrystalSequenceIndex.Value] != which - 1)
			{
				base.playSound("cancel");
				this.resetPuzzle();
				this.timesFailed.Value++;
				return;
			}
			this.currentCrystalSequenceIndex.Value++;
			if (this.currentCrystalSequenceIndex.Value >= this.currentCrystalSequence.Count)
			{
				DelayedAction.playSoundAfterDelay((this.currentDifficulty.Value == 7) ? "discoverMineral" : "newArtifact", 500, this);
				this.netPhaseTimer.Value = 2000f;
				this.netPhase.Value = 4;
			}
		}
		if (this.crystals.Count > which - 1)
		{
			this.crystals[which - 1].activate();
		}
	}

	public override void cleanupBeforePlayerExit()
	{
		this.crystals.Clear();
		base.cleanupBeforePlayerExit();
	}

	public override void UpdateWhenCurrentLocation(GameTime time)
	{
		this.enterValueEvent.Poll();
		if ((this.localPhase != 1 || this.currentPlaybackCrystalSequenceIndex < 0 || this.currentPlaybackCrystalSequenceIndex >= this.currentCrystalSequence.Count) && this.localPhase != this.netPhase.Value)
		{
			this.localPhaseTimer = this.netPhaseTimer.Value;
			this.localPhase = this.netPhase.Value;
			if (this.localPhase != 1)
			{
				this.currentPlaybackCrystalSequenceIndex = -1;
			}
			else
			{
				this.currentPlaybackCrystalSequenceIndex = 0;
			}
		}
		base.UpdateWhenCurrentLocation(time);
		foreach (CaveCrystal crystal in this.crystals)
		{
			crystal.update();
		}
		if (this.localPhaseTimer > 0f)
		{
			this.localPhaseTimer -= (float)time.ElapsedGameTime.TotalMilliseconds;
			if (this.localPhaseTimer <= 0f)
			{
				switch (this.localPhase)
				{
				case 0:
				case 4:
					this.currentPlaybackCrystalSequenceIndex = 0;
					if (Game1.IsMasterGame)
					{
						this.currentDifficulty.Value++;
						this.currentCrystalSequence.Clear();
						this.currentCrystalSequenceIndex.Value = 0;
						if (this.currentDifficulty.Value > ((this.timesFailed.Value < 8) ? 7 : 6))
						{
							this.netPhaseTimer.Value = 10f;
							this.netPhase.Value = 5;
							break;
						}
						for (int i = 0; i < this.currentDifficulty.Value; i++)
						{
							this.currentCrystalSequence.Add(Game1.random.Next(5));
						}
						this.netPhase.Value = 1;
					}
					this.betweenNotesTimer = 600f;
					break;
				case 5:
					if (Game1.currentLocation == this)
					{
						Game1.playSound("fireball");
						Utility.addSmokePuff(this, new Vector2(5f, 1f) * 64f);
						Utility.addSmokePuff(this, new Vector2(7f, 1f) * 64f);
					}
					if (Game1.IsMasterGame)
					{
						Game1.player.team.MarkCollectedNut("IslandWestCavePuzzle");
						Game1.createObjectDebris("(O)73", 5, 1, this);
						Game1.createObjectDebris("(O)73", 7, 1, this);
						Game1.createObjectDebris("(O)73", 6, 1, this);
					}
					this.completed.Value = true;
					if (Game1.currentLocation == this)
					{
						this.addCompletionTorches();
					}
					break;
				}
			}
		}
		if (this.localPhase != 1)
		{
			return;
		}
		this.betweenNotesTimer -= (float)time.ElapsedGameTime.TotalMilliseconds;
		if (!(this.betweenNotesTimer <= 0f) || this.currentCrystalSequence.Count <= 0 || this.currentPlaybackCrystalSequenceIndex < 0)
		{
			return;
		}
		int which = this.currentCrystalSequence[this.currentPlaybackCrystalSequenceIndex];
		if (which < this.crystals.Count)
		{
			this.crystals[which].activate();
		}
		this.currentPlaybackCrystalSequenceIndex++;
		int betweenNotesDivisor = this.currentDifficulty.Value;
		if (this.currentDifficulty.Value > 5)
		{
			betweenNotesDivisor--;
			if (this.timesFailed.Value >= 4)
			{
				betweenNotesDivisor--;
			}
			if (this.timesFailed.Value >= 6)
			{
				betweenNotesDivisor--;
			}
			if (this.timesFailed.Value >= 8)
			{
				betweenNotesDivisor = 3;
			}
		}
		else if (this.timesFailed.Value >= 4 && this.currentDifficulty.Value > 4)
		{
			betweenNotesDivisor--;
		}
		this.betweenNotesTimer = 1500f / (float)betweenNotesDivisor;
		if (this.currentDifficulty.Value > ((this.timesFailed.Value < 8) ? 7 : 6))
		{
			this.betweenNotesTimer = 100f;
		}
		if (this.currentPlaybackCrystalSequenceIndex < this.currentCrystalSequence.Count)
		{
			return;
		}
		this.currentPlaybackCrystalSequenceIndex = -1;
		if (this.currentDifficulty.Value > ((this.timesFailed.Value < 8) ? 7 : 6))
		{
			if (Game1.IsMasterGame)
			{
				this.netPhaseTimer.Value = 1000f;
				this.netPhase.Value = 5;
			}
		}
		else if (Game1.IsMasterGame)
		{
			this.netPhase.Value = 2;
			this.currentCrystalSequenceIndex.Value = 0;
		}
	}

	public override void TransferDataFromSavedLocation(GameLocation l)
	{
		base.TransferDataFromSavedLocation(l);
		if (l is IslandWestCave1 cave)
		{
			this.completed.Value = cave.completed.Value;
		}
	}

	public void addCompletionTorches()
	{
		if (this.completed.Value)
		{
			base.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(276, 1985, 12, 11), new Vector2(5f, 1f) * 64f + new Vector2(0f, -20f), flipped: false, 0f, Color.White)
			{
				interval = 50f,
				totalNumberOfLoops = 99999,
				animationLength = 4,
				lightId = "IslandWestCave1_Torch_1",
				lightRadius = 2f,
				scale = 4f,
				layerDepth = 0.013439999f
			});
			base.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(276, 1985, 12, 11), new Vector2(7f, 1f) * 64f + new Vector2(8f, -20f), flipped: false, 0f, Color.White)
			{
				interval = 50f,
				totalNumberOfLoops = 99999,
				animationLength = 4,
				lightId = "IslandWestCave1_Torch_2",
				lightRadius = 2f,
				scale = 4f,
				layerDepth = 0.013439999f
			});
		}
	}

	public override void draw(SpriteBatch b)
	{
		base.draw(b);
		foreach (CaveCrystal crystal in this.crystals)
		{
			crystal.draw(b);
		}
	}
}
