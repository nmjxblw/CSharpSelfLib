using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Netcode;
using StardewValley.Extensions;
using StardewValley.Menus;
using StardewValley.Network;
using xTile.Dimensions;

namespace StardewValley.Locations;

public class IslandFieldOffice : IslandLocation
{
	public const int totalPieces = 11;

	public const int piece_Skeleton_Back_Leg = 0;

	public const int piece_Skeleton_Ribs = 1;

	public const int piece_Skeleton_Front_Leg = 2;

	public const int piece_Skeleton_Tail = 3;

	public const int piece_Skeleton_Spine = 4;

	public const int piece_Skeleton_Skull = 5;

	public const int piece_Snake_Tail = 6;

	public const int piece_Snake_Spine = 7;

	public const int piece_Snake_Skull = 8;

	public const int piece_Bat = 9;

	public const int piece_Frog = 10;

	[XmlElement("uncollectedRewards")]
	public NetList<Item, NetRef<Item>> uncollectedRewards = new NetList<Item, NetRef<Item>>();

	[XmlIgnore]
	public NetMutex safariGuyMutex = new NetMutex();

	private NPC safariGuy;

	[XmlElement("piecesDonated")]
	public NetList<bool, NetBool> piecesDonated = new NetList<bool, NetBool>(11);

	[XmlElement("centerSkeletonRestored")]
	public readonly NetBool centerSkeletonRestored = new NetBool
	{
		InterpolationWait = false
	};

	[XmlElement("snakeRestored")]
	public readonly NetBool snakeRestored = new NetBool
	{
		InterpolationWait = false
	};

	[XmlElement("batRestored")]
	public readonly NetBool batRestored = new NetBool
	{
		InterpolationWait = false
	};

	[XmlElement("frogRestored")]
	public readonly NetBool frogRestored = new NetBool
	{
		InterpolationWait = false
	};

	[XmlElement("plantsRestoredLeft")]
	public readonly NetBool plantsRestoredLeft = new NetBool
	{
		InterpolationWait = false
	};

	[XmlElement("plantsRestoredRight")]
	public readonly NetBool plantsRestoredRight = new NetBool
	{
		InterpolationWait = false
	};

	public readonly NetBool hasFailedSurveyToday = new NetBool();

	private bool _shouldTriggerFinalCutscene;

	private float speakerTimer;

	public IslandFieldOffice()
	{
	}

	public IslandFieldOffice(string map, string name)
		: base(map, name)
	{
		while (this.piecesDonated.Count < 11)
		{
			this.piecesDonated.Add(item: false);
		}
	}

	public NPC getSafariGuy()
	{
		return this.safariGuy;
	}

	protected override void initNetFields()
	{
		base.initNetFields();
		base.NetFields.AddField(this.piecesDonated, "piecesDonated").AddField(this.centerSkeletonRestored, "centerSkeletonRestored").AddField(this.snakeRestored, "snakeRestored")
			.AddField(this.batRestored, "batRestored")
			.AddField(this.frogRestored, "frogRestored")
			.AddField(this.plantsRestoredLeft, "plantsRestoredLeft")
			.AddField(this.plantsRestoredRight, "plantsRestoredRight")
			.AddField(this.uncollectedRewards, "uncollectedRewards")
			.AddField(this.hasFailedSurveyToday, "hasFailedSurveyToday")
			.AddField(this.safariGuyMutex.NetFields, "safariGuyMutex.NetFields");
		this.centerSkeletonRestored.fieldChangeEvent += delegate(NetBool f, bool oldValue, bool newValue)
		{
			if (newValue && base.mapPath.Value != null)
			{
				this.ApplySkeletonRestore();
			}
		};
		this.snakeRestored.fieldChangeEvent += delegate(NetBool f, bool oldValue, bool newValue)
		{
			if (newValue && base.mapPath.Value != null)
			{
				this.ApplySnakeRestore();
			}
		};
		this.batRestored.fieldChangeEvent += delegate(NetBool f, bool oldValue, bool newValue)
		{
			if (newValue && base.mapPath.Value != null)
			{
				this.ApplyBatRestore();
			}
		};
		this.frogRestored.fieldChangeEvent += delegate(NetBool f, bool oldValue, bool newValue)
		{
			if (newValue && base.mapPath.Value != null)
			{
				this.ApplyFrogRestore();
			}
		};
		this.plantsRestoredLeft.fieldChangeEvent += delegate(NetBool f, bool oldValue, bool newValue)
		{
			if (newValue && base.mapPath.Value != null)
			{
				this.ApplyPlantRestoreLeft();
			}
		};
		this.plantsRestoredRight.fieldChangeEvent += delegate(NetBool f, bool oldValue, bool newValue)
		{
			if (newValue && base.mapPath.Value != null)
			{
				this.ApplyPlantRestoreRight();
			}
		};
	}

	private void ApplyPlantRestoreLeft()
	{
		base.temporarySprites.Add(new TemporaryAnimatedSprite(50, new Vector2(1.1f, 3.3f) * 64f, new Color(0, 220, 150))
		{
			layerDepth = 1f,
			motion = new Vector2(1f, -4f),
			acceleration = new Vector2(0f, 0.1f)
		});
		base.temporarySprites.Add(new TemporaryAnimatedSprite(50, new Vector2(1.1f, 3.3f) * 64f + new Vector2(Game1.random.Next(-16, 16), Game1.random.Next(-48, 48)), new Color(0, 220, 150) * 0.75f)
		{
			scale = 0.75f,
			flipped = true,
			layerDepth = 1f,
			motion = new Vector2(-1f, -4f),
			acceleration = new Vector2(0f, 0.1f)
		});
		base.temporarySprites.Add(new TemporaryAnimatedSprite(50, new Vector2(1.1f, 3.3f) * 64f + new Vector2(Game1.random.Next(-16, 16), Game1.random.Next(-48, 48)), new Color(0, 220, 150) * 0.75f)
		{
			scale = 0.75f,
			delayBeforeAnimationStart = 50,
			layerDepth = 1f,
			motion = new Vector2(1f, -4f),
			acceleration = new Vector2(0f, 0.1f)
		});
		base.temporarySprites.Add(new TemporaryAnimatedSprite(50, new Vector2(1.1f, 3.3f) * 64f + new Vector2(Game1.random.Next(-16, 16), Game1.random.Next(-48, 48)), new Color(0, 220, 150) * 0.75f)
		{
			scale = 0.75f,
			flipped = true,
			delayBeforeAnimationStart = 100,
			layerDepth = 1f,
			motion = new Vector2(-1f, -4f),
			acceleration = new Vector2(0f, 0.1f)
		});
		base.temporarySprites.Add(new TemporaryAnimatedSprite(50, new Vector2(1.1f, 3.3f) * 64f + new Vector2(Game1.random.Next(-16, 16), Game1.random.Next(-48, 48)), new Color(250, 100, 250) * 0.75f)
		{
			scale = 0.75f,
			flipped = true,
			delayBeforeAnimationStart = 150,
			layerDepth = 1f,
			motion = new Vector2(0f, -3f),
			acceleration = new Vector2(0f, 0.1f)
		});
		if (Game1.gameMode == 6 || Utility.ShouldIgnoreValueChangeCallback())
		{
			return;
		}
		if (Game1.currentLocation == this)
		{
			Game1.playSound("leafrustle");
			DelayedAction.playSoundAfterDelay("leafrustle", 150);
		}
		if (Game1.IsMasterGame)
		{
			Game1.player.team.MarkCollectedNut("IslandLeftPlantRestored");
			if (Game1.netWorldState.Value.GoldenWalnutsFound < 130)
			{
				Game1.createItemDebris(ItemRegistry.Create("(O)73"), new Vector2(1.5f, 3.3f) * 64f, 1, this, 256);
			}
		}
	}

	private void ApplyPlantRestoreRight()
	{
		base.temporarySprites.Add(new TemporaryAnimatedSprite(50, new Vector2(7.5f, 3.3f) * 64f, new Color(0, 220, 150))
		{
			layerDepth = 1f,
			motion = new Vector2(1f, -4f),
			acceleration = new Vector2(0f, 0.1f)
		});
		base.temporarySprites.Add(new TemporaryAnimatedSprite(50, new Vector2(8f, 3.3f) * 64f + new Vector2(Game1.random.Next(-16, 16), Game1.random.Next(-48, 48)), new Color(0, 220, 150) * 0.75f)
		{
			scale = 0.75f,
			flipped = true,
			layerDepth = 1f,
			motion = new Vector2(-1f, -4f),
			acceleration = new Vector2(0f, 0.1f)
		});
		base.temporarySprites.Add(new TemporaryAnimatedSprite(50, new Vector2(8.3f, 3.3f) * 64f + new Vector2(Game1.random.Next(-16, 16), Game1.random.Next(-48, 48)), new Color(0, 200, 120) * 0.75f)
		{
			scale = 0.75f,
			delayBeforeAnimationStart = 50,
			layerDepth = 1f,
			motion = new Vector2(1f, -4f),
			acceleration = new Vector2(0f, 0.1f)
		});
		base.temporarySprites.Add(new TemporaryAnimatedSprite(50, new Vector2(8f, 3.3f) * 64f + new Vector2(Game1.random.Next(-16, 16), Game1.random.Next(-48, 48)), new Color(0, 220, 150) * 0.75f)
		{
			scale = 0.75f,
			flipped = true,
			delayBeforeAnimationStart = 100,
			layerDepth = 1f,
			motion = new Vector2(-1f, -4f),
			acceleration = new Vector2(0f, 0.1f)
		});
		base.temporarySprites.Add(new TemporaryAnimatedSprite(50, new Vector2(8.5f, 3.3f) * 64f + new Vector2(Game1.random.Next(-16, 16), Game1.random.Next(-48, 48)), new Color(0, 250, 180) * 0.75f)
		{
			scale = 0.75f,
			flipped = true,
			delayBeforeAnimationStart = 150,
			layerDepth = 1f,
			motion = new Vector2(0f, -3f),
			acceleration = new Vector2(0f, 0.1f)
		});
		if (Game1.gameMode == 6 || Utility.ShouldIgnoreValueChangeCallback())
		{
			return;
		}
		if (Game1.currentLocation == this)
		{
			Game1.playSound("leafrustle");
			DelayedAction.playSoundAfterDelay("leafrustle", 150);
		}
		if (Game1.IsMasterGame)
		{
			Game1.player.team.MarkCollectedNut("IslandRightPlantRestored");
			if (Game1.netWorldState.Value.GoldenWalnutsFound < 130)
			{
				Game1.createItemDebris(ItemRegistry.Create("(O)73"), new Vector2(7.5f, 3.3f) * 64f, 3, this, 256);
			}
		}
	}

	private void ApplyFrogRestore()
	{
		if (Game1.gameMode != 6 && !Utility.ShouldIgnoreValueChangeCallback() && Game1.currentLocation == this)
		{
			Game1.playSound("dirtyHit");
		}
		for (int i = 0; i < 3; i++)
		{
			base.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(372, 1956, 10, 10), new Vector2(6.5f + (float)Game1.random.Next(-10, 11) / 100f, 3f) * 64f, flipped: false, 0.007f, Color.White)
			{
				alpha = 0.75f,
				motion = new Vector2(0f, -1f),
				acceleration = new Vector2(0.002f, 0f),
				interval = 99999f,
				layerDepth = 1f,
				scale = 4f,
				scaleChange = 0.02f,
				rotationChange = (float)Game1.random.Next(-5, 6) * (float)Math.PI / 256f,
				delayBeforeAnimationStart = i * 100
			});
		}
	}

	private void ApplyBatRestore()
	{
		if (Game1.gameMode != 6 && !Utility.ShouldIgnoreValueChangeCallback() && Game1.currentLocation == this)
		{
			Game1.playSound("dirtyHit");
		}
		for (int i = 0; i < 3; i++)
		{
			base.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Microsoft.Xna.Framework.Rectangle(372, 1956, 10, 10), new Vector2(2.5f + (float)Game1.random.Next(-10, 11) / 100f, 3f) * 64f, flipped: false, 0.007f, Color.White)
			{
				alpha = 0.75f,
				motion = new Vector2(0f, -1f),
				acceleration = new Vector2(0.002f, 0f),
				interval = 99999f,
				layerDepth = 1f,
				scale = 4f,
				scaleChange = 0.02f,
				rotationChange = (float)Game1.random.Next(-5, 6) * (float)Math.PI / 256f,
				delayBeforeAnimationStart = i * 100
			});
		}
	}

	private void ApplySnakeRestore()
	{
	}

	private void ApplySkeletonRestore()
	{
	}

	public override void TransferDataFromSavedLocation(GameLocation l)
	{
		base.TransferDataFromSavedLocation(l);
		IslandFieldOffice loc = l as IslandFieldOffice;
		this.uncollectedRewards.Clear();
		this.uncollectedRewards.Set(loc.uncollectedRewards);
		this.piecesDonated.Clear();
		this.piecesDonated.Set(loc.piecesDonated);
		this.centerSkeletonRestored.Value = loc.centerSkeletonRestored.Value;
		this.snakeRestored.Value = loc.snakeRestored.Value;
		this.batRestored.Value = loc.batRestored.Value;
		this.frogRestored.Value = loc.frogRestored.Value;
		this.plantsRestoredLeft.Value = loc.plantsRestoredLeft.Value;
		this.plantsRestoredRight.Value = loc.plantsRestoredRight.Value;
		this.hasFailedSurveyToday.Value = loc.hasFailedSurveyToday.Value;
	}

	protected override void resetLocalState()
	{
		base.resetLocalState();
		if (Game1.player.hasOrWillReceiveMail("islandNorthCaveOpened") && this.safariGuy == null)
		{
			this.safariGuy = new NPC(new AnimatedSprite("Characters\\SafariGuy", 0, 16, 32), new Vector2(8f, 6f) * 64f, "IslandFieldOFfice", 2, "Professor Snail", datable: false, Game1.content.Load<Texture2D>("Portraits\\SafariGuy"));
			this.safariGuy.AllowDynamicAppearance = false;
			this.safariGuy.displayName = Game1.content.LoadString("Strings\\NPCNames:ProfessorSnail");
		}
		if (this.safariGuy != null && !Game1.player.hasOrWillReceiveMail("safariGuyIntro"))
		{
			this.startEvent(new Event(Game1.content.LoadString("Strings\\Locations:IslandFieldOffice_Intro_Event")));
			Game1.player.mailReceived.Add("safariGuyIntro");
			Game1.player.Halt();
			return;
		}
		if (this.safariGuy != null)
		{
			Game1.changeMusicTrack("fieldofficeTentMusic");
			if (Game1.random.NextBool())
			{
				this.safariGuy.Halt();
				this.safariGuy.showTextAboveHead(Game1.content.LoadString("Strings\\Locations:IslandFieldOffice_Welcome_" + Game1.random.Next(4)));
				this.safariGuy.faceTowardFarmerForPeriod(60000, 5, faceAway: false, Game1.player);
			}
			else
			{
				this.safariGuy.Sprite.CurrentAnimation = new List<FarmerSprite.AnimationFrame>
				{
					new FarmerSprite.AnimationFrame(18, 900, 0, secondaryArm: false, flip: false),
					new FarmerSprite.AnimationFrame(19, 900, 0, secondaryArm: false, flip: false)
				};
			}
		}
		if (!Game1.player.hasOrWillReceiveMail("fieldOfficeFinale") && this.isRangeAllTrue(0, 11) && this.plantsRestoredRight.Value && this.plantsRestoredLeft.Value && base.currentEvent == null)
		{
			this._StartFinaleEvent();
		}
	}

	/// <summary>
	/// returns true if a new uncollected reward was added.
	/// </summary>
	/// <param name="which"></param>
	/// <returns></returns>
	public bool donatePiece(int which)
	{
		this.piecesDonated[which] = true;
		if (!this.centerSkeletonRestored.Value && this.isRangeAllTrue(0, 6))
		{
			this.centerSkeletonRestored.Value = true;
			if (Game1.netWorldState.Value.GoldenWalnutsFound < 130)
			{
				this.uncollectedRewards.Add(ItemRegistry.Create("(O)73", 6));
			}
			this.uncollectedRewards.Add(ItemRegistry.Create("(O)69"));
			Game1.player.team.MarkCollectedNut("IslandCenterSkeletonRestored");
			return true;
		}
		if (!this.snakeRestored.Value && this.isRangeAllTrue(6, 9))
		{
			this.snakeRestored.Value = true;
			if (Game1.netWorldState.Value.GoldenWalnutsFound < 130)
			{
				this.uncollectedRewards.Add(ItemRegistry.Create("(O)73", 3));
			}
			this.uncollectedRewards.Add(ItemRegistry.Create("(O)835"));
			Game1.player.team.MarkCollectedNut("IslandSnakeRestored");
			return true;
		}
		if (!this.batRestored.Value && this.piecesDonated[9])
		{
			this.batRestored.Value = true;
			if (Game1.netWorldState.Value.GoldenWalnutsFound < 130)
			{
				this.uncollectedRewards.Add(ItemRegistry.Create("(O)73"));
			}
			else
			{
				this.uncollectedRewards.Add(ItemRegistry.Create("(O)TentKit"));
			}
			Game1.player.team.MarkCollectedNut("IslandBatRestored");
			return true;
		}
		if (!this.frogRestored.Value && this.piecesDonated[10])
		{
			this.frogRestored.Value = true;
			if (Game1.netWorldState.Value.GoldenWalnutsFound < 130)
			{
				this.uncollectedRewards.Add(ItemRegistry.Create("(O)73"));
			}
			else
			{
				this.uncollectedRewards.Add(ItemRegistry.Create("(O)926"));
			}
			Game1.player.team.MarkCollectedNut("IslandFrogRestored");
			return true;
		}
		return false;
	}

	public bool isRangeAllTrue(int low, int high)
	{
		for (int i = low; i < high; i++)
		{
			if (!this.piecesDonated[i])
			{
				return false;
			}
		}
		return true;
	}

	public void triggerFinaleCutscene()
	{
		this._shouldTriggerFinalCutscene = true;
	}

	private void _triggerFinaleCutsceneActual()
	{
		Game1.player.Halt();
		Game1.player.freezePause = 500;
		DelayedAction.functionAfterDelay(delegate
		{
			if (Game1.activeClickableMenu != null)
			{
				Game1.activeClickableMenu = null;
			}
			Game1.globalFadeToBlack(_StartFinaleEvent);
		}, 500);
		this._shouldTriggerFinalCutscene = false;
	}

	protected void _StartFinaleEvent()
	{
		this.safariGuy?.clearTextAboveHead();
		this.startEvent(new Event(Game1.content.LoadString("Strings\\Locations:FieldOfficeFinale")));
	}

	public override void draw(SpriteBatch b)
	{
		base.draw(b);
		if (this.safariGuy != null && !Game1.eventUp)
		{
			this.safariGuy.draw(b);
		}
		if (this.centerSkeletonRestored.Value)
		{
			b.Draw(Game1.mouseCursors2, Game1.GlobalToLocal(new Vector2(3f, 4f) * 64f + new Vector2(0f, 4f) * 4f), new Microsoft.Xna.Framework.Rectangle(210, 184, 46, 43), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.0512f);
		}
		if (this.snakeRestored.Value)
		{
			b.Draw(Game1.mouseCursors2, Game1.GlobalToLocal(new Vector2(1f, 5f) * 64f), new Microsoft.Xna.Framework.Rectangle(195, 185, 14, 42), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.0448f);
		}
		if (this.batRestored.Value)
		{
			b.Draw(Game1.mouseCursors2, Game1.GlobalToLocal(new Vector2(2.5f, 2.7f) * 64f + new Vector2(1f, 1f) * 4f), new Microsoft.Xna.Framework.Rectangle(212, 171, 16, 12), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.0256f);
		}
		if (this.frogRestored.Value)
		{
			b.Draw(Game1.mouseCursors2, Game1.GlobalToLocal(new Vector2(6f, 2f) * 64f + new Vector2(9f, 10f) * 4f), new Microsoft.Xna.Framework.Rectangle(232, 169, 14, 15), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.0256f);
		}
		if (this.plantsRestoredLeft.Value)
		{
			b.Draw(Game1.mouseCursors2, Game1.GlobalToLocal(new Vector2(1f, 4f) * 64f + new Vector2(0f, -7f) * 4f), new Microsoft.Xna.Framework.Rectangle(194, 167, 16, 17), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.032f);
		}
		if (this.plantsRestoredRight.Value)
		{
			b.Draw(Game1.mouseCursors2, Game1.GlobalToLocal(new Vector2(7f, 3f) * 64f + new Vector2(8f, 3f) * 4f), new Microsoft.Xna.Framework.Rectangle(224, 148, 32, 21), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.032f);
		}
		if (this.safariGuy != null && (!this.plantsRestoredLeft.Value || !this.plantsRestoredRight.Value) && !Game1.eventUp)
		{
			float yOffset = 4f * (float)Math.Round(Math.Sin(Game1.currentGameTime.ElapsedGameTime.TotalMilliseconds / 250.0), 2);
			b.Draw(Game1.mouseCursors2, Game1.GlobalToLocal(Game1.viewport, new Vector2(324f, 144f + yOffset)), new Microsoft.Xna.Framework.Rectangle(220, 160, 3, 8), Color.White, 0f, new Vector2(1f, 4f), 4f + Math.Max(0f, 0.25f - yOffset / 16f), SpriteEffects.None, 1f);
		}
	}

	public override void drawAboveAlwaysFrontLayer(SpriteBatch b)
	{
		base.drawAboveAlwaysFrontLayer(b);
		this.safariGuy?.drawAboveAlwaysFrontLayer(b);
	}

	public override void UpdateWhenCurrentLocation(GameTime time)
	{
		base.UpdateWhenCurrentLocation(time);
		this.safariGuyMutex.Update(this);
		if (this.safariGuy != null)
		{
			this.safariGuy.update(time, this);
			this.speakerTimer -= (float)time.ElapsedGameTime.TotalMilliseconds;
			if (this.speakerTimer <= 0f)
			{
				this.speakerTimer = 600f;
				base.temporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Microsoft.Xna.Framework.Rectangle(211, 161, 5, 5), new Vector2(74.75f, 20.75f) * 4f, flipped: false, 0f, Color.White)
				{
					scale = 5f,
					scaleChange = -0.05f,
					motion = new Vector2(0.125f, 0.125f),
					animationLength = 1,
					totalNumberOfLoops = 1,
					interval = 400f,
					layerDepth = 1f
				});
			}
		}
		if (Game1.currentLocation == this && this._shouldTriggerFinalCutscene && Game1.activeClickableMenu == null)
		{
			this._triggerFinaleCutsceneActual();
		}
	}

	public virtual void OnCollectReward(Item item, Farmer farmer)
	{
		if (!(Game1.activeClickableMenu is ItemGrabMenu grab_menu) || grab_menu.context != this)
		{
			return;
		}
		if (Game1.player.addItemToInventoryBool(grab_menu.heldItem))
		{
			this.uncollectedRewards.Remove(item);
			grab_menu.ItemsToGrabMenu.actualInventory = new List<Item>(this.uncollectedRewards);
			grab_menu.heldItem = null;
			if (item.QualifiedItemId != "(O)73")
			{
				Game1.playSound("coin");
			}
		}
		else
		{
			Game1.playSound("cancel");
			Game1.showRedMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:Crop.cs.588"));
			grab_menu.ItemsToGrabMenu.actualInventory = new List<Item>(this.uncollectedRewards);
			grab_menu.heldItem = null;
		}
	}

	public override bool answerDialogueAction(string questionAndAnswer, string[] questionParams)
	{
		if (questionAndAnswer == null)
		{
			return false;
		}
		switch (questionAndAnswer)
		{
		case "Safari_Hint":
		{
			int bone = this.getRandomUnfoundBoneIndex();
			if (bone == 823)
			{
				bone = 824;
			}
			Game1.DrawDialogue(this.safariGuy, "Data\\ExtraDialogue:ProfessorSnail_Hint_" + bone);
			break;
		}
		case "Safari_Collect":
		{
			Game1.activeClickableMenu = new ItemGrabMenu(new List<Item>(this.uncollectedRewards), reverseGrab: false, showReceivingMenu: true, null, null, "Rewards", OnCollectReward, snapToBottom: false, canBeExitedWithKey: true, playRightClickSound: false, allowRightClick: false, showOrganizeButton: false, 0, null, -1, this);
			IClickableMenu activeClickableMenu2 = Game1.activeClickableMenu;
			activeClickableMenu2.exitFunction = (IClickableMenu.onExit)Delegate.Combine(activeClickableMenu2.exitFunction, new IClickableMenu.onExit(this.safariGuyMutex.ReleaseLock));
			break;
		}
		case "Safari_Donate":
		{
			Game1.activeClickableMenu = new FieldOfficeMenu(this);
			IClickableMenu activeClickableMenu = Game1.activeClickableMenu;
			activeClickableMenu.exitFunction = (IClickableMenu.onExit)Delegate.Combine(activeClickableMenu.exitFunction, new IClickableMenu.onExit(this.safariGuyMutex.ReleaseLock));
			break;
		}
		case "Safari_Leave":
			this.safariGuyMutex.ReleaseLock();
			break;
		case "Survey_Yes":
			if (!this.plantsRestoredLeft.Value)
			{
				List<Response> responses = new List<Response>();
				for (int i = 18; i < 25; i++)
				{
					responses.Add(new Response((i == 22) ? "Correct" : "Wrong", i.ToString() ?? ""));
				}
				responses.Add(new Response("No", Game1.content.LoadString("Strings\\Locations:MineCart_Destination_Cancel")).SetHotKey(Keys.Escape));
				base.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:IslandFieldOffice_Survey_PurpleFlower_Question"), responses.ToArray(), "PurpleFlowerSurvey");
			}
			else if (!this.plantsRestoredRight.Value)
			{
				List<Response> responses2 = new List<Response>();
				for (int j = 11; j < 19; j++)
				{
					responses2.Add(new Response((j == 18) ? "Correct" : "Wrong", j.ToString() ?? ""));
				}
				responses2.Add(new Response("No", Game1.content.LoadString("Strings\\Locations:MineCart_Destination_Cancel")).SetHotKey(Keys.Escape));
				base.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:IslandFieldOffice_Survey_PurpleStarfish_Question"), responses2.ToArray(), "PurpleStarfishSurvey");
			}
			break;
		case "PurpleFlowerSurvey_Correct":
			Game1.DrawDialogue(this.safariGuy, "Strings\\Locations:IslandFieldOffice_Survey_PurpleFlower_Correct");
			this.plantsRestoredLeft.Value = true;
			Game1.multiplayer.globalChatInfoMessage("FinishedSurvey", Game1.player.name.Value);
			break;
		case "PurpleFlowerSurvey_Wrong":
			Game1.DrawDialogue(this.safariGuy, "Strings\\Locations:IslandFieldOffice_Survey_PurpleFlower_Wrong");
			this.hasFailedSurveyToday.Value = true;
			break;
		case "PurpleStarfishSurvey_Correct":
			Game1.DrawDialogue(this.safariGuy, "Strings\\Locations:IslandFieldOffice_Survey_PurpleFlower_Correct");
			this.plantsRestoredRight.Value = true;
			Game1.multiplayer.globalChatInfoMessage("FinishedSurvey", Game1.player.name.Value);
			break;
		case "PurpleStarfishSurvey_Wrong":
			Game1.DrawDialogue(this.safariGuy, "Strings\\Locations:IslandFieldOffice_Survey_PurpleFlower_Wrong");
			this.hasFailedSurveyToday.Value = true;
			break;
		}
		if (!Game1.player.hasOrWillReceiveMail("fieldOfficeFinale") && this.isRangeAllTrue(0, 11) && this.plantsRestoredRight.Value && this.plantsRestoredLeft.Value)
		{
			this.triggerFinaleCutscene();
		}
		return base.answerDialogueAction(questionAndAnswer, questionParams);
	}

	public override void DayUpdate(int dayOfMonth)
	{
		this.hasFailedSurveyToday.Value = false;
		base.DayUpdate(dayOfMonth);
	}

	public virtual void TalkToSafariGuy()
	{
		List<Response> responses = new List<Response>();
		responses.Add(new Response("Donate", Game1.content.LoadString("Strings\\Locations:ArchaeologyHouse_Gunther_Donate")));
		if (this.uncollectedRewards.Count > 0)
		{
			responses.Add(new Response("Collect", Game1.content.LoadString("Strings\\Locations:ArchaeologyHouse_Gunther_Collect")));
		}
		if (this.getRandomUnfoundBoneIndex() != -1)
		{
			responses.Add(new Response("Hint", Game1.content.LoadString("Strings\\Locations:Hint")));
		}
		responses.Add(new Response("Leave", Game1.content.LoadString("Strings\\Locations:ArchaeologyHouse_Gunther_Leave")));
		base.createQuestionDialogue("", responses.ToArray(), "Safari");
	}

	private int getRandomUnfoundBoneIndex()
	{
		Random r = Utility.CreateRandom(Game1.uniqueIDForThisGame, Game1.stats.DaysPlayed);
		for (int i = 0; i < 25; i++)
		{
			int index = r.Next(11);
			if (!this.piecesDonated[index])
			{
				return FieldOfficeMenu.getDonationPieceIndexNeededForSpot(index);
			}
		}
		for (int j = 0; j < this.piecesDonated.Count; j++)
		{
			if (!this.piecesDonated[j])
			{
				return FieldOfficeMenu.getDonationPieceIndexNeededForSpot(j);
			}
		}
		return -1;
	}

	/// <inheritdoc />
	public override bool performAction(string[] action, Farmer who, Location tileLocation)
	{
		string text = ArgUtility.Get(action, 0);
		if (!(text == "FieldOfficeDesk"))
		{
			if (text == "FieldOfficeSurvey" && this.safariGuy != null)
			{
				if (this.hasFailedSurveyToday.Value)
				{
					Game1.DrawDialogue(this.safariGuy, "Strings\\Locations:IslandFieldOffice_Survey_Failed");
					return true;
				}
				if (!this.plantsRestoredLeft.Value)
				{
					Response[] responses = new Response[2]
					{
						new Response("Yes", Game1.content.LoadString("Strings\\Locations:IslandFieldOffice_Survey_Yes")),
						new Response("No", Game1.content.LoadString("Strings\\Locations:IslandFieldOffice_Survey_Notyet"))
					};
					base.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:IslandFieldOffice_Survey_Prompt_LeftPlant"), responses, "Survey");
					(Game1.activeClickableMenu as DialogueBox).aboveDialogueImage = new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Microsoft.Xna.Framework.Rectangle(194, 167, 16, 17), 1f, 1, 1, Vector2.Zero, flicker: false, flipped: false)
					{
						scale = 4f
					};
				}
				else if (!this.plantsRestoredRight.Value)
				{
					Response[] responses2 = new Response[2]
					{
						new Response("Yes", Game1.content.LoadString("Strings\\Locations:IslandFieldOffice_Survey_Yes")),
						new Response("No", Game1.content.LoadString("Strings\\Locations:IslandFieldOffice_Survey_Notyet"))
					};
					base.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:IslandFieldOffice_Survey_Prompt_RightPlant"), responses2, "Survey");
					(Game1.activeClickableMenu as DialogueBox).aboveDialogueImage = new TemporaryAnimatedSprite("LooseSprites\\Cursors2", new Microsoft.Xna.Framework.Rectangle(193, 150, 16, 16), 1f, 1, 1, Vector2.Zero, flicker: false, flipped: false)
					{
						scale = 4f
					};
				}
				return true;
			}
		}
		else if (this.safariGuy != null)
		{
			this.safariGuyMutex.RequestLock(TalkToSafariGuy);
			return true;
		}
		return base.performAction(action, who, tileLocation);
	}
}
