using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Netcode;
using StardewValley.Extensions;
using StardewValley.Locations;
using StardewValley.Network;
using StardewValley.TerrainFeatures;
using xTile.Dimensions;

namespace StardewValley.BellsAndWhistles;

public class ParrotUpgradePerch : INetObject<NetFields>
{
	public enum UpgradeState
	{
		Idle,
		StartBuilding,
		Building,
		Complete
	}

	public class Parrot
	{
		public Vector2 position;

		public float height;

		protected ParrotUpgradePerch _perch;

		protected Vector2 targetPosition = Vector2.Zero;

		protected Vector2 startPosition = Vector2.Zero;

		public Texture2D texture;

		public bool bounced;

		public bool flipped;

		public bool isPerchedParrot;

		private int baseFrame;

		private int birdType;

		private int flapFrame;

		private float nextFlapTime;

		public float alpha;

		public float moveTime;

		public float moveDuration = 1f;

		public bool firstBounce;

		public bool flyAway;

		private bool soundBird;

		public Parrot(ParrotUpgradePerch perch, Vector2 start_position, bool soundBird = false, bool goldenParrot = false)
		{
			this.soundBird = soundBird;
			this._perch = perch;
			this.texture = perch.texture;
			this.position = (start_position + new Vector2(0.5f, 0.5f)) * 64f;
			this.startPosition = start_position;
			this.height = 64f;
			this.birdType = Game1.random.Next(0, 4);
			if (goldenParrot)
			{
				this.birdType = 4;
			}
			this.FindNewTarget();
			this.firstBounce = true;
		}

		public virtual void FindNewTarget()
		{
			this.isPerchedParrot = false;
			this.firstBounce = false;
			this.startPosition = this.position;
			this.moveTime = 0f;
			this.moveDuration = 1f;
			Microsoft.Xna.Framework.Rectangle rect = this._perch.upgradeRect.Value;
			if (this._perch.currentState.Value == UpgradeState.Complete)
			{
				this.flyAway = true;
				this.moveDuration = 5f;
				rect.Inflate(5, 0);
			}
			this.targetPosition = (Utility.getRandomPositionInThisRectangle(rect, Game1.random) + new Vector2(0.5f, 0.5f)) * 64f;
			Vector2 offset = Vector2.Zero;
			offset.X = this.targetPosition.X - this.position.X;
			offset.Y = this.targetPosition.Y - this.position.Y;
			if (offset.X < 0f)
			{
				this.flipped = false;
			}
			else if (offset.X > 0f)
			{
				this.flipped = true;
			}
			if (Math.Abs(offset.X) > Math.Abs(offset.Y))
			{
				this.baseFrame = 2;
			}
			else if (offset.Y > 0f)
			{
				this.baseFrame = 5;
			}
			else
			{
				this.baseFrame = 8;
			}
		}

		public virtual bool Update(GameTime time)
		{
			this.moveTime += (float)time.ElapsedGameTime.TotalSeconds;
			if (this.moveTime > this.moveDuration)
			{
				this.moveTime = this.moveDuration;
			}
			float t = this.moveTime / this.moveDuration;
			this.position.X = Utility.Lerp(this.startPosition.X, this.targetPosition.X, t);
			this.position.Y = Utility.Lerp(this.startPosition.Y, this.targetPosition.Y, t);
			if (this.isPerchedParrot)
			{
				this.height = this.EaseInOutQuad(t, 24f, -24f, 1f);
				this.firstBounce = false;
				this.birdType = 0;
			}
			else if (this.flyAway)
			{
				this.height = this.EaseInQuad(t, 0f, 1536f, 1f);
			}
			else if (this.firstBounce)
			{
				this.height = this.EaseInOutQuad(t, 64f, -64f, 1f);
			}
			else
			{
				float hop_height = 24f;
				if ((double)t <= 0.5)
				{
					this.height = this.EaseInOutQuad(t, 0f, hop_height, 0.5f);
				}
				else
				{
					this.height = this.EaseInQuad(t - 0.5f, hop_height, 0f - hop_height, 0.5f);
				}
			}
			if (t >= 1f)
			{
				if (this.flyAway)
				{
					return true;
				}
				this.FindNewTarget();
				if (!this.firstBounce && this._perch.upgradeName.Value != "Turtle")
				{
					this._perch.locationRef.Value.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Microsoft.Xna.Framework.Rectangle(49 + 16 * (this._perch.upgradeName.Value.Contains("Volcano") ? 1 : Game1.random.Next(3)), 229, 16, 6), this.position, Game1.random.NextBool(), 0f, Color.White)
					{
						motion = new Vector2(Game1.random.Next(-2, 3), -12f),
						acceleration = new Vector2(0f, 0.25f),
						rotationChange = (float)Game1.random.Next(-4, 5) * 0.05f,
						scale = 4f,
						animationLength = 1,
						totalNumberOfLoops = 1,
						interval = 1000 + Game1.random.Next(500),
						layerDepth = 1f,
						drawAboveAlwaysFront = true
					});
				}
			}
			if (this.firstBounce)
			{
				this.alpha = Utility.Clamp(t / 0.25f, 0f, 1f);
			}
			else if (this.flyAway)
			{
				float fade_duration = 0.1f;
				this.alpha = 1f - Utility.Clamp((t - (1f - fade_duration)) / fade_duration, 0f, 1f);
			}
			else
			{
				this.alpha = 1f;
			}
			if (this.nextFlapTime > 0f)
			{
				this.nextFlapTime -= (float)time.ElapsedGameTime.TotalSeconds;
			}
			if (this.nextFlapTime <= 0f)
			{
				this.flapFrame = (this.flapFrame + 1) % 3;
				if (this.flyAway || this.firstBounce || this.height < 12f || t < 0.5f)
				{
					if (this.flapFrame == 0)
					{
						if (this._perch.upgradeName.Value != "Hut")
						{
							if ((!this.flyAway || t < 0.5f) && this.soundBird)
							{
								this._perch.locationRef.Value.localSound("parrot_flap");
							}
							if (!this.flyAway && !this.firstBounce)
							{
								if (this._perch.upgradeName.Value.Contains("Volcano"))
								{
									if (Game1.random.NextDouble() < 0.15000000596046448)
									{
										this._perch.locationRef.Value.localSound("hammer");
									}
								}
								else if (this._perch.upgradeName.Value == "Turtle")
								{
									if (Game1.random.NextDouble() < 0.15000000596046448)
									{
										this._perch.locationRef.Value.localSound("hitEnemy");
									}
								}
								else
								{
									if (Game1.random.NextDouble() < 0.05000000074505806)
									{
										this._perch.locationRef.Value.localSound("axe");
									}
									if (Game1.random.NextDouble() < 0.05000000074505806)
									{
										this._perch.locationRef.Value.localSound("dirtyHit");
									}
									if (Game1.random.NextDouble() < 0.05000000074505806)
									{
										this._perch.locationRef.Value.localSound("crafting");
									}
								}
							}
						}
						this.nextFlapTime = 0.1f;
					}
					else
					{
						this.nextFlapTime = 0.05f;
					}
				}
				else if (this.flapFrame == 0)
				{
					if (this.soundBird)
					{
						this._perch.locationRef.Value.localSound("parrot_flap");
					}
					this.nextFlapTime = 0.3f;
				}
				else
				{
					this.nextFlapTime = 0.2f;
				}
			}
			return false;
		}

		private float EaseInOutQuad(float t, float b, float c, float d)
		{
			if ((t /= d / 2f) < 1f)
			{
				return c / 2f * t * t + b;
			}
			return (0f - c) / 2f * ((t -= 1f) * (t - 2f) - 1f) + b;
		}

		private float EaseInQuad(float t, float b, float c, float d)
		{
			return c * (t /= d) * t + b;
		}

		public virtual void Draw(SpriteBatch b)
		{
			int drawn_frame = this.baseFrame + this.flapFrame;
			b.Draw(this.texture, Game1.GlobalToLocal(Game1.viewport, Utility.snapDrawPosition(this.position - new Vector2(0f, this.height * 4f))), new Microsoft.Xna.Framework.Rectangle(drawn_frame * 24, this.birdType * 24, 24, 24), Color.White * this.alpha, 0f, new Vector2(12f, 18f), 4f, this.flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None, this.position.Y / 10000f);
		}
	}

	public const string GoldenParrotMailKey = "activateGoldenParrotsTonight";

	public NetEvent0 animationEvent = new NetEvent0();

	public NetMutex upgradeMutex = new NetMutex();

	public NetPoint tilePosition = new NetPoint();

	public Texture2D texture;

	public NetRectangle upgradeRect = new NetRectangle();

	public List<Parrot> parrots = new List<Parrot>();

	public NetEvent0 upgradeCompleteEvent = new NetEvent0();

	public NetEnum<UpgradeState> currentState = new NetEnum<UpgradeState>(UpgradeState.Idle);

	public float stateTimer;

	public NetInt requiredNuts = new NetInt(0);

	public float squawkTime;

	public float timeUntilChomp;

	public float timeUntilSqwawk;

	public float shakeTime;

	public float costShakeTime;

	public const int PARROT_COUNT = 24;

	public bool parrotPresent = true;

	public bool isPlayerNearby;

	public NetString upgradeName = new NetString("");

	public NetString requiredMail = new NetString("");

	public float nextParrotSpawn;

	public NetLocationRef locationRef = new NetLocationRef();

	public Action onApplyUpgrade;

	public Func<bool> onUpdateCompletionStatus;

	protected bool _cachedAvailablity;

	public NetFields NetFields { get; } = new NetFields("ParrotUpgradePerch");

	public ParrotUpgradePerch()
	{
		this.InitNetFields();
		this.texture = Game1.content.Load<Texture2D>("LooseSprites\\parrots");
	}

	public virtual void UpdateCompletionStatus()
	{
		if (this.onUpdateCompletionStatus != null && this.onUpdateCompletionStatus())
		{
			this.currentState.Value = UpgradeState.Complete;
		}
	}

	public virtual void InitNetFields()
	{
		this.NetFields.SetOwner(this).AddField(this.tilePosition, "tilePosition").AddField(this.upgradeRect, "upgradeRect")
			.AddField(this.currentState, "currentState")
			.AddField(this.upgradeMutex.NetFields, "upgradeMutex.NetFields")
			.AddField(this.animationEvent.NetFields, "animationEvent.NetFields")
			.AddField(this.upgradeCompleteEvent.NetFields, "upgradeCompleteEvent.NetFields")
			.AddField(this.locationRef.NetFields, "locationRef.NetFields")
			.AddField(this.requiredNuts, "requiredNuts")
			.AddField(this.upgradeName, "upgradeName")
			.AddField(this.requiredMail, "requiredMail");
		this.animationEvent.onEvent += PerformAnimation;
		this.upgradeCompleteEvent.onEvent += PerformCompleteAnimation;
	}

	public virtual void PerformCompleteAnimation()
	{
		if (this.upgradeName.Value.Contains("Volcano"))
		{
			for (int i = 0; i < 16; i++)
			{
				this.locationRef.Value.temporarySprites.Add(new TemporaryAnimatedSprite(5, Utility.getRandomPositionInThisRectangle(this.upgradeRect.Value, Game1.random) * 64f, Color.White)
				{
					motion = new Vector2(Game1.random.Next(-1, 2), -1f),
					scale = 1f,
					layerDepth = 1f,
					drawAboveAlwaysFront = true,
					delayBeforeAnimationStart = i * 15
				});
				TemporaryAnimatedSprite t = new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Microsoft.Xna.Framework.Rectangle(65, 229, 16, 6), Utility.getRandomPositionInThisRectangle(this.upgradeRect.Value, Game1.random) * 64f, Game1.random.NextBool(), 0f, Color.White)
				{
					motion = new Vector2(Game1.random.Next(-2, 3), -16f),
					acceleration = new Vector2(0f, 0.5f),
					rotationChange = (float)Game1.random.Next(-4, 5) * 0.05f,
					scale = 4f,
					animationLength = 1,
					totalNumberOfLoops = 1,
					interval = 1000 + Game1.random.Next(500),
					layerDepth = 1f,
					drawAboveAlwaysFront = true,
					yStopCoordinate = (this.upgradeRect.Bottom + 1) * 64
				};
				t.reachedStopCoordinate = t.bounce;
				this.locationRef.Value.TemporarySprites.Add(t);
				t = new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Microsoft.Xna.Framework.Rectangle(65, 229, 16, 6), Utility.getRandomPositionInThisRectangle(this.upgradeRect.Value, Game1.random) * 64f, Game1.random.NextBool(), 0f, Color.White)
				{
					motion = new Vector2(Game1.random.Next(-2, 3), -16f),
					acceleration = new Vector2(0f, 0.5f),
					rotationChange = (float)Game1.random.Next(-4, 5) * 0.05f,
					scale = 4f,
					animationLength = 1,
					totalNumberOfLoops = 1,
					interval = 1000 + Game1.random.Next(500),
					layerDepth = 1f,
					drawAboveAlwaysFront = true,
					yStopCoordinate = (this.upgradeRect.Bottom + 1) * 64
				};
				t.reachedStopCoordinate = t.bounce;
				this.locationRef.Value.TemporarySprites.Add(t);
			}
			if (this.locationRef.Value == Game1.currentLocation)
			{
				Game1.flashAlpha = 1f;
				Game1.playSound("boulderBreak");
			}
		}
		else
		{
			switch (this.upgradeName.Value)
			{
			case "House":
			{
				for (int j = 0; j < 16; j++)
				{
					this.locationRef.Value.temporarySprites.Add(new TemporaryAnimatedSprite(5, Utility.getRandomPositionInThisRectangle(this.upgradeRect.Value, Game1.random) * 64f, Color.White)
					{
						motion = new Vector2(Game1.random.Next(-1, 2), -1f),
						scale = 1f,
						layerDepth = 1f,
						drawAboveAlwaysFront = true,
						delayBeforeAnimationStart = j * 15
					});
					TemporaryAnimatedSprite t2 = new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Microsoft.Xna.Framework.Rectangle(49 + 16 * Game1.random.Next(3), 229, 16, 6), Utility.getRandomPositionInThisRectangle(this.upgradeRect.Value, Game1.random) * 64f, Game1.random.NextBool(), 0f, Color.White)
					{
						motion = new Vector2(Game1.random.Next(-2, 3), -16f),
						acceleration = new Vector2(0f, 0.5f),
						rotationChange = (float)Game1.random.Next(-4, 5) * 0.05f,
						scale = 4f,
						animationLength = 1,
						totalNumberOfLoops = 1,
						interval = 1000 + Game1.random.Next(500),
						layerDepth = 1f,
						drawAboveAlwaysFront = true,
						yStopCoordinate = (this.upgradeRect.Bottom + 1) * 64
					};
					t2.reachedStopCoordinate = t2.bounce;
					this.locationRef.Value.TemporarySprites.Add(t2);
					t2 = new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Microsoft.Xna.Framework.Rectangle(49 + 16 * Game1.random.Next(3), 229, 16, 6), Utility.getRandomPositionInThisRectangle(this.upgradeRect.Value, Game1.random) * 64f, Game1.random.NextBool(), 0f, Color.White)
					{
						motion = new Vector2(Game1.random.Next(-2, 3), -16f),
						acceleration = new Vector2(0f, 0.5f),
						rotationChange = (float)Game1.random.Next(-4, 5) * 0.05f,
						scale = 4f,
						animationLength = 1,
						totalNumberOfLoops = 1,
						interval = 1000 + Game1.random.Next(500),
						layerDepth = 1f,
						drawAboveAlwaysFront = true,
						yStopCoordinate = (this.upgradeRect.Bottom + 1) * 64
					};
					t2.reachedStopCoordinate = t2.bounce;
					this.locationRef.Value.TemporarySprites.Add(t2);
				}
				if (this.locationRef.Value == Game1.currentLocation)
				{
					Game1.flashAlpha = 1f;
					Game1.playSound("boulderBreak");
				}
				break;
			}
			case "Resort":
			case "Trader":
			case "Obelisk":
			case "House_Mailbox":
				if (this.locationRef.Value == Game1.currentLocation)
				{
					Game1.flashAlpha = 1f;
				}
				break;
			}
		}
		if (this.locationRef.Value == Game1.currentLocation && this.upgradeName.Value != "Hut")
		{
			DelayedAction.playSoundAfterDelay("secret1", 800);
		}
		if (this.locationRef.Value == null || !(this.locationRef.Value is IslandLocation islandLoc))
		{
			return;
		}
		foreach (ParrotUpgradePerch parrotUpgradePerch in islandLoc.parrotUpgradePerches)
		{
			parrotUpgradePerch.resetCache();
		}
	}

	public ParrotUpgradePerch(GameLocation location, Point tile_position, Microsoft.Xna.Framework.Rectangle upgrade_rectangle, int required_nuts, Action apply_upgrade, Func<bool> update_completion_status, string upgrade_name = "", string required_mail = "")
		: this()
	{
		this.locationRef.Value = location;
		this.tilePosition.Value = tile_position;
		this.upgradeRect.Value = upgrade_rectangle;
		this.onApplyUpgrade = apply_upgrade;
		this.onUpdateCompletionStatus = update_completion_status;
		this.requiredNuts.Value = required_nuts;
		this.parrots = new List<Parrot>();
		this.UpdateCompletionStatus();
		this.upgradeName.Value = upgrade_name;
		if (required_mail != "")
		{
			this.requiredMail.Value = required_mail;
		}
	}

	public bool IsAtTile(int x, int y)
	{
		if (this.tilePosition.X == x && this.tilePosition.Y == y)
		{
			return this.currentState.Value == UpgradeState.Idle;
		}
		return false;
	}

	public virtual void PerformAnimation()
	{
		this.currentState.Value = UpgradeState.StartBuilding;
		this.stateTimer = 3f;
		if (Game1.currentLocation == this.locationRef.Value)
		{
			Game1.playSound("parrot_squawk");
			this.parrots.Clear();
			this.parrotPresent = true;
			Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("Maps\\springobjects", Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 73, 16, 16), 2000f, 1, 0, (Utility.PointToVector2(this.tilePosition.Value) + new Vector2(0.25f, -2.5f)) * 64f, flicker: false, flipped: false, (float)(this.tilePosition.Y * 64 + 1) / 10000f, 0f, Color.White, 3f, -0.015f, 0f, 0f)
			{
				motion = new Vector2(-0.1f, -7f),
				acceleration = new Vector2(0f, 0.25f),
				id = 98765,
				drawAboveAlwaysFront = true
			});
			Game1.playSound("dwop");
			if (this.upgradeMutex.IsLockHeld())
			{
				Game1.player.freezePause = ((this.upgradeName.Value != "Hut") ? 10000 : 3000);
			}
			this.timeUntilChomp = 1f;
			this.squawkTime = 1f;
		}
	}

	public virtual bool IsAvailable(bool use_cached_value = false)
	{
		if (use_cached_value && Game1.currentLocation == this.locationRef.Value)
		{
			return this._cachedAvailablity;
		}
		if (this.requiredMail.Value == "")
		{
			return true;
		}
		string[] array = this.requiredMail.Value.Split(',');
		for (int i = 0; i < array.Length; i++)
		{
			string formatted_mail = array[i].Trim();
			if (!Game1.MasterPlayer.hasOrWillReceiveMail(formatted_mail))
			{
				return false;
			}
		}
		return true;
	}

	public virtual void StartAnimation()
	{
		this.animationEvent.Fire();
	}

	public bool CheckAction(Location tile_location, Farmer farmer)
	{
		if (this.IsAtTile(tile_location.X, tile_location.Y) && this.IsAvailable())
		{
			string request_text;
			if (this.upgradeName.Value == "GoldenParrot")
			{
				if (Game1.netWorldState.Value.GoldenWalnutsFound >= 130)
				{
					this.squawkTime = 0.5f;
					this.shakeTime = 0.5f;
					Game1.playSound("parrot_squawk");
					return true;
				}
				request_text = Game1.content.LoadStringReturnNullIfNotFound("Strings\\1_6_Strings:GoldenParrot");
			}
			else
			{
				request_text = Game1.content.LoadStringReturnNullIfNotFound("Strings\\UI:UpgradePerch_" + this.upgradeName.Value);
			}
			GameLocation location = this.locationRef.Value;
			if (request_text != null && location != null)
			{
				request_text = string.Format(request_text, this.requiredNuts.Value);
				this.costShakeTime = 0.5f;
				this.squawkTime = 0.5f;
				this.shakeTime = 0.5f;
				if (this.locationRef.Value == Game1.currentLocation)
				{
					Game1.playSound("parrot_squawk");
				}
				if (this.upgradeName.Value == "GoldenParrot")
				{
					if (Game1.MasterPlayer.hasOrWillReceiveMail("activateGoldenParrotsTonight"))
					{
						Game1.playSound("parrot_squawk");
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:GoldenParrot_Tonight"));
						return true;
					}
					int cost = (130 - Game1.netWorldState.Value.GoldenWalnutsFound) * 10000;
					location.createQuestionDialogue(request_text, new Response[2]
					{
						new Response("Yes", string.Format(Game1.content.LoadString("Strings\\1_6_Strings:GoldenParrot_Yes"), Utility.getNumberWithCommas(cost))).SetHotKey(Keys.Y),
						new Response("No", Game1.content.LoadString("Strings\\Lexicon:QuestionDialogue_No")).SetHotKey(Keys.Escape)
					}, "GoldenParrot");
				}
				else if (Game1.netWorldState.Value.GoldenWalnuts >= this.requiredNuts.Value)
				{
					location.createQuestionDialogue(request_text, location.createYesNoResponses(), "UpgradePerch_" + this.upgradeName.Value);
				}
				else
				{
					Game1.drawDialogueNoTyping(request_text);
				}
			}
			else if (Game1.netWorldState.Value.GoldenWalnuts >= this.requiredNuts.Value)
			{
				this.AttemptConstruction();
			}
			else
			{
				this.ShowInsufficientNuts();
			}
			return true;
		}
		return false;
	}

	public virtual bool AnswerQuestion(Response answer)
	{
		if (Game1.currentLocation.lastQuestionKey != null)
		{
			string question_and_answer = ArgUtility.SplitBySpace(Game1.currentLocation.lastQuestionKey)[0] + "_" + answer.responseKey;
			if (question_and_answer == "UpgradePerch_" + this.upgradeName.Value + "_Yes")
			{
				this.AttemptConstruction();
				return true;
			}
			if (!(question_and_answer == "GoldenParrot_Yes"))
			{
				if (question_and_answer == "GoldenParrotConfirm_Yes")
				{
					int cost = (130 - Game1.netWorldState.Value.GoldenWalnutsFound) * 10000;
					if (Game1.player.Money < cost)
					{
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:NotEnoughMoney3"));
					}
					else
					{
						Game1.player.Money -= cost;
						Game1.multiplayer.broadcastPartyWideMail("activateGoldenParrotsTonight", Multiplayer.PartyWideMessageQueue.MailForTomorrow, no_letter: true);
						Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:GoldenParrot_Tonight"));
						Game1.playSound("parrot_squawk");
					}
					return true;
				}
			}
			else
			{
				int cost2 = (130 - Game1.netWorldState.Value.GoldenWalnutsFound) * 10000;
				if (Game1.player.Money >= cost2)
				{
					this.locationRef.Value.createQuestionDialogue(Game1.content.LoadString("Strings\\1_6_Strings:GoldenParrot_Sure"), new Response[2]
					{
						new Response("Yes", string.Format(Game1.content.LoadString("Strings\\1_6_Strings:GoldenParrot_Yes"), Utility.getNumberWithCommas(cost2))).SetHotKey(Keys.Y),
						new Response("No", Game1.content.LoadString("Strings\\Lexicon:QuestionDialogue_No")).SetHotKey(Keys.Escape)
					}, "GoldenParrotConfirm");
					return true;
				}
				Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\UI:NotEnoughMoney3"));
			}
		}
		return false;
	}

	public static void ActivateGoldenParrot()
	{
		string[] array = new string[67]
		{
			"Bush_IslandNorth_13_33", "Bush_IslandNorth_5_30", "Buried_IslandNorth_19_39", "Bush_IslandNorth_4_42", "Bush_IslandNorth_45_38", "Bush_IslandNorth_47_40", "IslandLeftPlantRestored", "IslandRightPlantRestored", "IslandBatRestored", "IslandFrogRestored",
			"IslandCenterSkeletonRestored", "IslandSnakeRestored", "Buried_IslandNorth_19_13", "Buried_IslandNorth_57_79", "Buried_IslandNorth_54_21", "Buried_IslandNorth_42_77", "Buried_IslandNorth_62_54", "Buried_IslandNorth_26_81", "Bush_IslandNorth_20_26", "Bush_IslandNorth_9_84",
			"Bush_IslandNorth_56_27", "Bush_IslandSouth_31_5", "TreeNut", "IslandWestCavePuzzle", "SandDuggy", "TigerSlimeNut", "Buried_IslandWest_21_81", "Buried_IslandWest_62_76", "Buried_IslandWest_39_24", "Buried_IslandWest_88_14",
			"Buried_IslandWest_43_74", "Buried_IslandWest_30_75", "MusselStone", "IslandFarming", "Bush_IslandWest_104_3", "Bush_IslandWest_31_24", "Bush_IslandWest_38_56", "Bush_IslandWest_75_29", "Bush_IslandWest_64_30", "Bush_IslandWest_54_18",
			"Bush_IslandWest_25_30", "Bush_IslandWest_15_3", "IslandFishing", "VolcanoNormalChest", "VolcanoRareChest", "VolcanoBarrel", "VolcanoMining", "VolcanoMonsterDrop", "Island_N_BuriedTreasureNut", "Island_W_BuriedTreasureNut",
			"Island_W_BuriedTreasureNut2", "Mermaid", "TreeNutShot", "Buried_IslandSouthEastCave_36_26", "Buried_IslandSouthEast_25_17", "StardropPool", "Bush_Caldera_28_36", "Bush_Caldera_9_34", "Bush_CaptainRoom_2_4", "BananaShrine",
			"Bush_IslandEast_17_37", "Darts", "IslandGourmand1", "IslandGourmand2", "IslandGourmand3", "IslandShrinePuzzle", "Bush_IslandShrine_23_34"
		};
		foreach (string s in array)
		{
			Game1.player.team.limitedNutDrops[s] = 9999;
			Game1.player.team.collectedNutTracker.Add(s);
			Game1.netWorldState.Value.FoundBuriedNuts.Add(s.Replace("Buried_", ""));
		}
		int missingNuts = 130 - Game1.netWorldState.Value.GoldenWalnutsFound;
		Game1.netWorldState.Value.GoldenWalnutsFound = 130;
		Game1.netWorldState.Value.GoldenWalnuts += missingNuts;
		Game1.netWorldState.Value.GoldenCoconutCracked = true;
		Game1.netWorldState.Value.ActivatedGoldenParrot = true;
		foreach (GameLocation l in Game1.locations)
		{
			if (l is IslandLocation)
			{
				foreach (LargeTerrainFeature largeTerrainFeature in l.largeTerrainFeatures)
				{
					if (largeTerrainFeature is Bush bush)
					{
						bush.tileSheetOffset.Value = 0;
						bush.setUpSourceRect();
					}
				}
			}
			if (!(l is IslandNorth island_north))
			{
				if (!(l is IslandWestCave1 cave))
				{
					if (!(l is IslandEast east))
					{
						if (!(l is IslandHut hut))
						{
							if (!(l is IslandSouthEast se))
							{
								if (!(l is IslandShrine shrine))
								{
									if (!(l is IslandWest west))
									{
										if (l is IslandFarmCave fcave)
										{
											fcave.gourmandRequestsFulfilled.Value = 3;
										}
									}
									else
									{
										west.sandDuggy.Value.whacked.Value = true;
									}
								}
								else
								{
									shrine.puzzleFinished.Value = true;
								}
							}
							else
							{
								se.mermaidPuzzleFinished.Value = true;
								se.fishedWalnut.Value = true;
							}
						}
						else
						{
							hut.treeNutObtained.Value = true;
						}
					}
					else
					{
						east.bananaShrineComplete.Value = true;
					}
				}
				else
				{
					cave.completed.Value = true;
				}
				continue;
			}
			island_north.treeNutShot.Value = true;
			foreach (ParrotUpgradePerch p in island_north.parrotUpgradePerches)
			{
				if (p.upgradeName.Value == "GoldenParrot")
				{
					p.currentState.Value = UpgradeState.Complete;
				}
			}
		}
	}

	public virtual void AttemptConstruction()
	{
		Game1.player.Halt();
		Game1.player.canMove = false;
		this.upgradeMutex.RequestLock(delegate
		{
			Game1.player.canMove = true;
			if (Game1.netWorldState.Value.GoldenWalnuts >= this.requiredNuts.Value)
			{
				if (Game1.content.LoadStringReturnNullIfNotFound("Strings\\UI:Chat_UpgradePerch_" + this.upgradeName.Value) != null)
				{
					Game1.multiplayer.globalChatInfoMessage("UpgradePerch_" + this.upgradeName.Value, Game1.player.Name);
				}
				Game1.netWorldState.Value.GoldenWalnuts -= this.requiredNuts.Value;
				this.StartAnimation();
			}
			else
			{
				this.ShowInsufficientNuts();
			}
		}, delegate
		{
			Game1.player.canMove = true;
		});
	}

	public virtual void ShowInsufficientNuts()
	{
		if (this.IsAvailable(use_cached_value: true))
		{
			this.costShakeTime = 0.5f;
			this.squawkTime = 0.5f;
			this.shakeTime = 0.5f;
			if (this.locationRef.Value == Game1.currentLocation)
			{
				Game1.playSound("parrot_squawk");
			}
		}
	}

	public virtual void ApplyUpgrade()
	{
		this.onApplyUpgrade?.Invoke();
	}

	public virtual void Cleanup()
	{
		if (this.upgradeMutex.IsLockHeld())
		{
			this.upgradeMutex.ReleaseLock();
		}
		if (this.isPlayerNearby)
		{
			this.isPlayerNearby = false;
		}
	}

	public virtual void ResetForPlayerEntry()
	{
		this.resetCache();
	}

	public void resetCache()
	{
		this._cachedAvailablity = this.IsAvailable();
		this.parrotPresent = this.currentState.Value == UpgradeState.Idle;
	}

	public virtual void UpdateEvenIfFarmerIsntHere(GameTime time)
	{
		this.animationEvent.Poll();
		this.upgradeCompleteEvent.Poll();
		this.upgradeMutex.Update(this.locationRef.Value);
		if (!Game1.IsMasterGame)
		{
			return;
		}
		if (this.stateTimer > 0f)
		{
			this.stateTimer -= (float)time.ElapsedGameTime.TotalSeconds;
		}
		if (this.currentState.Value == UpgradeState.StartBuilding && this.stateTimer <= 0f)
		{
			this.currentState.Value = UpgradeState.Building;
			this.stateTimer = 5f;
			if (this.upgradeName.Value == "Hut")
			{
				this.stateTimer = 0.1f;
			}
		}
		if (this.currentState.Value == UpgradeState.Building && this.stateTimer <= 0f)
		{
			this.ApplyUpgrade();
			this.currentState.Value = UpgradeState.Complete;
			this.upgradeMutex.ReleaseLock();
			this.upgradeCompleteEvent.Fire();
		}
	}

	public virtual void Update(GameTime time)
	{
		if (this.squawkTime > 0f)
		{
			this.squawkTime -= (float)time.ElapsedGameTime.TotalSeconds;
		}
		if (this.shakeTime > 0f)
		{
			this.shakeTime -= (float)time.ElapsedGameTime.TotalSeconds;
		}
		if (this.costShakeTime > 0f)
		{
			this.costShakeTime -= (float)time.ElapsedGameTime.TotalSeconds;
		}
		if (this.timeUntilChomp > 0f)
		{
			this.timeUntilChomp -= (float)time.ElapsedGameTime.TotalSeconds;
			if (this.timeUntilChomp <= 0f)
			{
				if (this.locationRef.Value == Game1.currentLocation)
				{
					Game1.playSound("eat");
				}
				this.timeUntilChomp = 0f;
				this.shakeTime = 0.25f;
				if (this.locationRef.Value.getTemporarySpriteByID(98765) != null)
				{
					for (int i = 0; i < 6; i++)
					{
						this.locationRef.Value.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Microsoft.Xna.Framework.Rectangle(9, 252, 3, 3), this.locationRef.Value.getTemporarySpriteByID(98765).position + new Vector2(8f, 8f) * 4f, Game1.random.NextBool(), 0f, Color.White)
						{
							motion = new Vector2(Game1.random.Next(-1, 2), -6f),
							acceleration = new Vector2(0f, 0.25f),
							rotationChange = (float)Game1.random.Next(-4, 5) * 0.05f,
							scale = 4f,
							animationLength = 1,
							totalNumberOfLoops = 1,
							interval = 500 + Game1.random.Next(500),
							layerDepth = 1f,
							drawAboveAlwaysFront = true
						});
					}
				}
				this.locationRef.Value.removeTemporarySpritesWithID(98765);
				this.timeUntilSqwawk = 1f;
			}
		}
		if (this.timeUntilSqwawk > 0f)
		{
			this.timeUntilSqwawk -= (float)time.ElapsedGameTime.TotalSeconds;
			if (this.timeUntilSqwawk <= 0f)
			{
				this.timeUntilSqwawk = 0f;
				if (this.locationRef.Value == Game1.currentLocation)
				{
					Game1.playSound("parrot_squawk");
				}
				this.squawkTime = 0.5f;
				this.shakeTime = 0.5f;
			}
		}
		if (this.parrotPresent && this.currentState.Value > UpgradeState.StartBuilding)
		{
			if (this.currentState.Value == UpgradeState.Building)
			{
				Parrot flying_parrot = new Parrot(this, Utility.PointToVector2(this.tilePosition.Value));
				flying_parrot.isPerchedParrot = true;
				this.parrots.Add(flying_parrot);
			}
			this.parrotPresent = false;
		}
		if (this.IsAvailable(use_cached_value: true))
		{
			bool playerNearby = this.currentState.Value == UpgradeState.Idle && !this.upgradeMutex.IsLocked() && Math.Abs(Game1.player.TilePoint.X - this.tilePosition.X) <= 1 && Math.Abs(Game1.player.TilePoint.Y - this.tilePosition.Y) <= 1;
			if (playerNearby != this.isPlayerNearby)
			{
				this.isPlayerNearby = playerNearby;
				if (this.isPlayerNearby)
				{
					if (this.locationRef.Value == Game1.currentLocation)
					{
						Game1.playSound("parrot_squawk");
					}
					this.squawkTime = 0.5f;
					this.shakeTime = 0.5f;
					this.costShakeTime = 0.5f;
					Game1.specialCurrencyDisplay.ShowCurrency("walnuts", () => this.isPlayerNearby, 0f);
				}
			}
		}
		if (this.currentState.Value == UpgradeState.Building && this.parrots.Count < 24)
		{
			if (this.nextParrotSpawn > 0f)
			{
				this.nextParrotSpawn -= (float)time.ElapsedGameTime.TotalSeconds;
			}
			if (this.nextParrotSpawn <= 0f)
			{
				this.nextParrotSpawn = 0.05f;
				Microsoft.Xna.Framework.Rectangle spawn_rectangle = this.upgradeRect.Value;
				spawn_rectangle.Inflate(5, 0);
				this.parrots.Add(new Parrot(this, Utility.getRandomPositionInThisRectangle(spawn_rectangle, Game1.random), this.parrots.Count % 10 == 0));
			}
		}
		this.parrots.RemoveAll((Parrot parrot) => parrot.Update(time));
	}

	public virtual void Draw(SpriteBatch b)
	{
		if (this.IsAvailable(use_cached_value: true) && (this.parrotPresent || this.upgradeName.Value == "Hut"))
		{
			int frame = 0;
			Vector2 shake = Vector2.Zero;
			if (this.squawkTime > 0f)
			{
				frame = 1;
			}
			if (this.shakeTime > 0f)
			{
				shake.X = Utility.RandomFloat(-0.5f, 0.5f) * 4f;
				shake.Y = Utility.RandomFloat(-0.5f, 0.5f) * 4f;
			}
			b.Draw(this.texture, Game1.GlobalToLocal(Game1.viewport, (Utility.PointToVector2(this.tilePosition.Value) + new Vector2(0.5f, -1f)) * 64f) + shake, new Microsoft.Xna.Framework.Rectangle(frame * 24, (this.upgradeName.Value == "GoldenParrot") ? 96 : 0, 24, 24), Color.White, 0f, new Vector2(12f, 16f), 4f, SpriteEffects.None, (((float)this.tilePosition.Y + 1f) * 64f - 1f) / 10000f);
		}
	}

	public virtual void DrawAboveAlwaysFrontLayer(SpriteBatch b)
	{
		if (this.parrotPresent && this.IsAvailable(use_cached_value: true) && this.isPlayerNearby)
		{
			if (this.upgradeName.Value == "GoldenParrot" && Game1.MasterPlayer.hasOrWillReceiveMail("activateGoldenParrotsTonight"))
			{
				return;
			}
			Vector2 cost_shake = Vector2.Zero;
			if (this.costShakeTime > 0f)
			{
				cost_shake.X = Utility.RandomFloat(-0.5f, 0.5f) * 4f;
				cost_shake.Y = Utility.RandomFloat(-0.5f, 0.5f) * 4f;
			}
			float yOffset = 2f * (float)Math.Round(Math.Sin(Game1.currentGameTime.ElapsedGameTime.TotalMilliseconds / 250.0), 2);
			Vector2 draw_position = Utility.PointToVector2(this.tilePosition.Value);
			float draw_layer = draw_position.Y * 64f / 10000f;
			yOffset -= 72f;
			b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(draw_position.X * 64f, draw_position.Y * 64f - 96f - 48f + yOffset)) + cost_shake, new Microsoft.Xna.Framework.Rectangle(141, 465, 20, 24), Color.White * 0.75f, 0f, Vector2.Zero, 4f, SpriteEffects.None, draw_layer + 1E-06f);
			Vector2 item_draw_position = Game1.GlobalToLocal(Game1.viewport, new Vector2(draw_position.X * 64f + 32f + 8f, draw_position.Y * 64f - 64f - 32f - 8f + yOffset)) + cost_shake;
			if (this.upgradeName.Value == "GoldenParrot")
			{
				b.Draw(Game1.mouseCursors_1_6, item_draw_position, new Microsoft.Xna.Framework.Rectangle(131, 0, 16, 16), Color.White, 0f, new Vector2(8f, 8f), 4f, SpriteEffects.None, draw_layer + 1E-05f);
			}
			else
			{
				b.Draw(Game1.objectSpriteSheet, item_draw_position, Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 73, 16, 16), Color.White, 0f, new Vector2(8f, 8f), 4f, SpriteEffects.None, draw_layer + 1E-05f);
				Utility.drawTinyDigits(this.requiredNuts.Value, b, item_draw_position + new Vector2((float)(64 - Utility.getWidthOfTinyDigitString(this.requiredNuts.Value, 3f)) + 3f - 32f, 16f), 3f, 1f, Color.White);
			}
		}
		foreach (Parrot parrot in this.parrots)
		{
			parrot.Draw(b);
		}
	}
}
