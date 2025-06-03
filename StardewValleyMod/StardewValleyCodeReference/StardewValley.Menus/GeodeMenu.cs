using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Extensions;

namespace StardewValley.Menus;

public class GeodeMenu : MenuWithInventory
{
	public const int region_geodeSpot = 998;

	public ClickableComponent geodeSpot;

	public AnimatedSprite clint;

	public TemporaryAnimatedSprite geodeDestructionAnimation;

	public TemporaryAnimatedSprite sparkle;

	public int geodeAnimationTimer;

	public int yPositionOfGem;

	public int alertTimer;

	public float delayBeforeShowArtifactTimer;

	public Item geodeTreasure;

	public Item geodeTreasureOverride;

	public bool waitingForServerResponse;

	private TemporaryAnimatedSpriteList fluffSprites = new TemporaryAnimatedSpriteList();

	public GeodeMenu()
		: base(null, okButton: true, trashCan: true, 12, 132)
	{
		if (base.yPositionOnScreen == IClickableMenu.borderWidth + IClickableMenu.spaceToClearTopBorder)
		{
			base.movePosition(0, -IClickableMenu.spaceToClearTopBorder);
		}
		base.inventory.highlightMethod = highlightGeodes;
		this.geodeSpot = new ClickableComponent(new Rectangle(base.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth / 2, base.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 4, 560, 308), "")
		{
			myID = 998,
			downNeighborID = 0
		};
		this.clint = new AnimatedSprite("Characters\\Clint", 8, 32, 48);
		List<ClickableComponent> list = base.inventory.inventory;
		if (list != null && list.Count >= 12)
		{
			for (int i = 0; i < 12; i++)
			{
				if (base.inventory.inventory[i] != null)
				{
					base.inventory.inventory[i].upNeighborID = 998;
				}
			}
		}
		if (base.trashCan != null)
		{
			base.trashCan.myID = 106;
		}
		if (base.okButton != null)
		{
			base.okButton.leftNeighborID = 11;
		}
		if (Game1.options.SnappyMenus)
		{
			this.populateClickableComponentList();
			this.snapToDefaultClickableComponent();
		}
	}

	public override void snapToDefaultClickableComponent()
	{
		base.currentlySnappedComponent = base.getComponentWithID(0);
		this.snapCursorToCurrentSnappedComponent();
	}

	public override bool readyToClose()
	{
		if (base.readyToClose() && this.geodeAnimationTimer <= 0 && base.heldItem == null)
		{
			return !this.waitingForServerResponse;
		}
		return false;
	}

	public bool highlightGeodes(Item i)
	{
		if (base.heldItem == null)
		{
			return Utility.IsGeode(i);
		}
		return true;
	}

	public virtual void startGeodeCrack()
	{
		this.geodeSpot.item = base.heldItem.getOne();
		base.heldItem = base.heldItem.ConsumeStack(1);
		this.geodeAnimationTimer = 2700;
		Game1.player.Money -= 25;
		Game1.playSound("stoneStep");
		this.clint.setCurrentAnimation(new List<FarmerSprite.AnimationFrame>
		{
			new FarmerSprite.AnimationFrame(8, 300),
			new FarmerSprite.AnimationFrame(9, 200),
			new FarmerSprite.AnimationFrame(10, 80),
			new FarmerSprite.AnimationFrame(11, 200),
			new FarmerSprite.AnimationFrame(12, 100),
			new FarmerSprite.AnimationFrame(8, 300)
		});
		this.clint.loop = false;
	}

	/// <inheritdoc />
	public override void receiveLeftClick(int x, int y, bool playSound = true)
	{
		if (this.waitingForServerResponse)
		{
			return;
		}
		base.receiveLeftClick(x, y);
		if (!this.geodeSpot.containsPoint(x, y))
		{
			return;
		}
		if (base.heldItem != null && Utility.IsGeode(base.heldItem) && Game1.player.Money >= 25 && this.geodeAnimationTimer <= 0)
		{
			int freeSpotsInInventory = Game1.player.freeSpotsInInventory();
			if (freeSpotsInInventory > 1 || (freeSpotsInInventory == 1 && base.heldItem.Stack == 1))
			{
				if (base.heldItem.QualifiedItemId == "(O)791" && !Game1.netWorldState.Value.GoldenCoconutCracked)
				{
					this.waitingForServerResponse = true;
					Game1.player.team.goldenCoconutMutex.RequestLock(delegate
					{
						this.waitingForServerResponse = false;
						this.geodeTreasureOverride = ItemRegistry.Create("(O)73");
						this.startGeodeCrack();
					}, delegate
					{
						this.waitingForServerResponse = false;
						this.startGeodeCrack();
					});
				}
				else
				{
					this.startGeodeCrack();
				}
			}
			else
			{
				base.descriptionText = Game1.content.LoadString("Strings\\UI:GeodeMenu_InventoryFull");
				base.wiggleWordsTimer = 500;
				this.alertTimer = 1500;
			}
		}
		else if (Game1.player.Money < 25)
		{
			base.wiggleWordsTimer = 500;
			Game1.dayTimeMoneyBox.moneyShakeTimer = 1000;
		}
	}

	/// <inheritdoc />
	public override void receiveRightClick(int x, int y, bool playSound = true)
	{
		base.receiveRightClick(x, y);
	}

	/// <inheritdoc />
	public override void performHoverAction(int x, int y)
	{
		if (this.alertTimer > 0)
		{
			return;
		}
		base.performHoverAction(x, y);
		if (base.descriptionText.Equals(""))
		{
			if (Game1.player.Money < 25)
			{
				base.descriptionText = Game1.content.LoadString("Strings\\UI:GeodeMenu_Description_NotEnoughMoney");
			}
			else
			{
				base.descriptionText = Game1.content.LoadString("Strings\\UI:GeodeMenu_Description");
			}
		}
	}

	public override void emergencyShutDown()
	{
		base.emergencyShutDown();
		if (base.heldItem != null)
		{
			Game1.player.addItemToInventoryBool(base.heldItem);
		}
	}

	/// <inheritdoc />
	public override void update(GameTime time)
	{
		base.update(time);
		this.fluffSprites.RemoveWhere((TemporaryAnimatedSprite sprite) => sprite.update(time));
		if (this.alertTimer > 0)
		{
			this.alertTimer -= time.ElapsedGameTime.Milliseconds;
		}
		if (this.geodeAnimationTimer <= 0)
		{
			return;
		}
		Game1.MusicDuckTimer = 1500f;
		this.geodeAnimationTimer -= time.ElapsedGameTime.Milliseconds;
		if (this.geodeAnimationTimer <= 0)
		{
			this.geodeDestructionAnimation = null;
			this.geodeSpot.item = null;
			if (this.geodeTreasure?.QualifiedItemId == "(O)73")
			{
				Game1.netWorldState.Value.GoldenCoconutCracked = true;
			}
			Game1.player.addItemToInventoryBool(this.geodeTreasure);
			this.geodeTreasure = null;
			this.yPositionOfGem = 0;
			this.fluffSprites.Clear();
			this.delayBeforeShowArtifactTimer = 0f;
			return;
		}
		int frame = this.clint.currentFrame;
		this.clint.animateOnce(time);
		if (this.clint.currentFrame == 11 && frame != 11)
		{
			if (this.geodeSpot.item?.QualifiedItemId == "(O)275" || this.geodeSpot.item?.QualifiedItemId == "(O)MysteryBox" || this.geodeSpot.item?.QualifiedItemId == "(O)GoldenMysteryBox")
			{
				Game1.playSound("hammer");
				Game1.playSound("woodWhack");
			}
			else
			{
				Game1.playSound("hammer");
				Game1.playSound("stoneCrack");
			}
			Game1.stats.GeodesCracked++;
			if (this.geodeSpot.item?.QualifiedItemId == "(O)MysteryBox" || this.geodeSpot.item?.QualifiedItemId == "(O)GoldenMysteryBox")
			{
				Game1.stats.Increment("MysteryBoxesOpened");
			}
			int geodeDestructionYOffset = 448;
			if (this.geodeSpot.item != null)
			{
				string qualifiedItemId = this.geodeSpot.item.QualifiedItemId;
				if (!(qualifiedItemId == "(O)536"))
				{
					if (qualifiedItemId == "(O)537")
					{
						geodeDestructionYOffset += 128;
					}
				}
				else
				{
					geodeDestructionYOffset += 64;
				}
				this.geodeDestructionAnimation = new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, geodeDestructionYOffset, 64, 64), 100f, 8, 0, new Vector2(this.geodeSpot.bounds.X + 392 - 32, this.geodeSpot.bounds.Y + 192 - 32), flicker: false, flipped: false);
				switch (this.geodeSpot.item?.QualifiedItemId)
				{
				case "(O)275":
				{
					this.geodeDestructionAnimation = new TemporaryAnimatedSprite
					{
						texture = Game1.temporaryContent.Load<Texture2D>("LooseSprites//temporary_sprites_1"),
						sourceRect = new Rectangle(388, 123, 18, 21),
						sourceRectStartingPos = new Vector2(388f, 123f),
						animationLength = 6,
						position = new Vector2(this.geodeSpot.bounds.X + 380 - 32, this.geodeSpot.bounds.Y + 192 - 32),
						holdLastFrame = true,
						interval = 100f,
						id = 777,
						scale = 4f
					};
					for (int i2 = 0; i2 < 6; i2++)
					{
						this.fluffSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(372, 1956, 10, 10), new Vector2(this.geodeSpot.bounds.X + 392 - 32 + Game1.random.Next(21), this.geodeSpot.bounds.Y + 192 - 16), flipped: false, 0.002f, new Color(255, 222, 198))
						{
							alphaFade = 0.02f,
							motion = new Vector2((float)Game1.random.Next(-20, 21) / 10f, (float)Game1.random.Next(5, 20) / 10f),
							interval = 99999f,
							layerDepth = 0.9f,
							scale = 3f,
							scaleChange = 0.01f,
							rotationChange = (float)Game1.random.Next(-5, 6) * (float)Math.PI / 256f,
							delayBeforeAnimationStart = i2 * 20
						});
						this.fluffSprites.Add(new TemporaryAnimatedSprite
						{
							texture = Game1.temporaryContent.Load<Texture2D>("LooseSprites//temporary_sprites_1"),
							sourceRect = new Rectangle(499, 132, 5, 5),
							sourceRectStartingPos = new Vector2(499f, 132f),
							motion = new Vector2((float)Game1.random.Next(-30, 31) / 10f, Game1.random.Next(-7, -4)),
							acceleration = new Vector2(0f, 0.25f),
							totalNumberOfLoops = 1,
							interval = 1000f,
							alphaFade = 0.015f,
							animationLength = 1,
							layerDepth = 1f,
							scale = 4f,
							rotationChange = (float)Game1.random.Next(-5, 6) * (float)Math.PI / 256f,
							delayBeforeAnimationStart = i2 * 10,
							position = new Vector2(this.geodeSpot.bounds.X + 392 - 32 + Game1.random.Next(21), this.geodeSpot.bounds.Y + 192 - 16)
						});
						this.delayBeforeShowArtifactTimer = 500f;
					}
					break;
				}
				case "(O)MysteryBox":
				case "(O)GoldenMysteryBox":
				{
					this.geodeDestructionAnimation = new TemporaryAnimatedSprite
					{
						texture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\Cursors_1_6"),
						sourceRect = new Rectangle((this.geodeSpot.item?.QualifiedItemId == "(O)GoldenMysteryBox") ? 256 : 0, 27, 24, 24),
						sourceRectStartingPos = new Vector2((this.geodeSpot.item?.QualifiedItemId == "(O)GoldenMysteryBox") ? 256 : 0, 27f),
						animationLength = 8,
						position = new Vector2(this.geodeSpot.bounds.X + 380 - 48, this.geodeSpot.bounds.Y + 192 - 48),
						holdLastFrame = true,
						interval = 100f,
						id = 777,
						scale = 4f
					};
					for (int i = 0; i < 6; i++)
					{
						this.fluffSprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(372, 1956, 10, 10), new Vector2(this.geodeSpot.bounds.X + 392 - 48 + Game1.random.Next(32), this.geodeSpot.bounds.Y + 192 - 24), flipped: false, 0.002f, new Color(255, 222, 198))
						{
							alphaFade = 0.02f,
							motion = new Vector2((float)Game1.random.Next(-20, 21) / 10f, (float)Game1.random.Next(5, 20) / 10f),
							interval = 99999f,
							layerDepth = 0.9f,
							scale = 3f,
							scaleChange = 0.01f,
							rotationChange = (float)Game1.random.Next(-5, 6) * (float)Math.PI / 256f,
							delayBeforeAnimationStart = i * 20
						});
						int which = Game1.random.Next(3);
						this.fluffSprites.Add(new TemporaryAnimatedSprite
						{
							texture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\Cursors_1_6"),
							sourceRect = new Rectangle(((this.geodeSpot.item?.QualifiedItemId == "(O)GoldenMysteryBox") ? 15 : 0) + which * 5, 52, 5, 5),
							sourceRectStartingPos = new Vector2(which * 5, 75f),
							motion = new Vector2((float)Game1.random.Next(-30, 31) / 10f, Game1.random.Next(-7, -4)),
							acceleration = new Vector2(0f, 0.25f),
							totalNumberOfLoops = 1,
							interval = 1000f,
							alphaFade = 0.015f,
							animationLength = 1,
							layerDepth = 1f,
							scale = 4f,
							rotationChange = (float)Game1.random.Next(-5, 6) * (float)Math.PI / 256f,
							delayBeforeAnimationStart = i * 10,
							position = new Vector2(this.geodeSpot.bounds.X + 392 - 48 + Game1.random.Next(32), this.geodeSpot.bounds.Y + 192 - 24)
						});
						this.delayBeforeShowArtifactTimer = 500f;
					}
					break;
				}
				}
				if (this.geodeTreasureOverride != null)
				{
					this.geodeTreasure = this.geodeTreasureOverride;
					this.geodeTreasureOverride = null;
				}
				else
				{
					this.geodeTreasure = Utility.getTreasureFromGeode(this.geodeSpot.item);
				}
				if (!(this.geodeSpot.item.QualifiedItemId == "(O)275") && !(this.geodeTreasure is Object { Type: "Minerals" }) && this.geodeTreasure is Object { Type: "Arch" } && !Game1.player.hasOrWillReceiveMail("artifactFound"))
				{
					this.geodeTreasure = ItemRegistry.Create("(O)390", 5);
				}
			}
		}
		if (this.geodeDestructionAnimation != null && ((this.geodeDestructionAnimation.id != 777 && this.geodeDestructionAnimation.currentParentTileIndex < 7) || (this.geodeDestructionAnimation.id == 777 && this.geodeDestructionAnimation.currentParentTileIndex < 5)))
		{
			this.geodeDestructionAnimation.update(time);
			if (this.delayBeforeShowArtifactTimer > 0f)
			{
				this.delayBeforeShowArtifactTimer -= (float)time.ElapsedGameTime.TotalMilliseconds;
				if (this.delayBeforeShowArtifactTimer <= 0f)
				{
					this.fluffSprites.Add(this.geodeDestructionAnimation);
					this.fluffSprites.Reverse();
					this.geodeDestructionAnimation = new TemporaryAnimatedSprite
					{
						interval = 100f,
						animationLength = 6,
						alpha = 0.001f,
						id = 777
					};
				}
			}
			else
			{
				if (this.geodeDestructionAnimation.currentParentTileIndex < 3)
				{
					this.yPositionOfGem--;
				}
				this.yPositionOfGem--;
				if (this.geodeDestructionAnimation.currentParentTileIndex == 7 || (this.geodeDestructionAnimation.id == 777 && this.geodeDestructionAnimation.currentParentTileIndex == 5))
				{
					if (!(this.geodeTreasure is Object treasure) || treasure.price.Value > 75 || this.geodeSpot.item?.QualifiedItemId == "(O)MysteryBox" || this.geodeSpot.item?.QualifiedItemId == "(O)GoldenMysteryBox")
					{
						if (this.geodeSpot.item != null)
						{
							this.sparkle = new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 640, 64, 64), 100f, 8, 0, new Vector2(this.geodeSpot.bounds.X + ((this.geodeSpot.item.itemId.Value == "MysteryBox") ? 94 : 98) * 4 - 32, this.geodeSpot.bounds.Y + 192 + this.yPositionOfGem - 32), flicker: false, flipped: false);
						}
						Game1.playSound("discoverMineral");
					}
					else
					{
						Game1.playSound("newArtifact");
					}
				}
			}
		}
		if (this.sparkle != null && this.sparkle.update(time))
		{
			this.sparkle = null;
		}
	}

	/// <inheritdoc />
	public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
	{
		base.gameWindowSizeChanged(oldBounds, newBounds);
		Vector2 v = Utility.getTopLeftPositionForCenteringOnScreen(base.width, base.height);
		base.xPositionOnScreen = (int)v.X;
		base.yPositionOnScreen = (int)v.Y;
		Item tmpItem = this.geodeSpot.item;
		this.geodeSpot = new ClickableComponent(new Rectangle(base.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth / 2, base.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + 4, 560, 308), "Anvil");
		this.geodeSpot.item = tmpItem;
		int yPositionForInventory = base.yPositionOnScreen + IClickableMenu.spaceToClearTopBorder + IClickableMenu.borderWidth + 192 - 16 + 128 + 4;
		if (base.okButton != null)
		{
			base.okButton = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen + base.width + 4, base.yPositionOnScreen + base.height - 192 - IClickableMenu.borderWidth, 64, 64), Game1.mouseCursors, Game1.getSourceRectForStandardTileSheet(Game1.mouseCursors, 46), 1f)
			{
				myID = 4857,
				upNeighborID = 5948,
				leftNeighborID = 12
			};
		}
		if (base.trashCan != null)
		{
			base.trashCan = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen + base.width + 4, base.yPositionOnScreen + base.height - 192 - 32 - IClickableMenu.borderWidth - 104, 64, 104), Game1.mouseCursors, new Rectangle(564 + Game1.player.trashCanLevel * 18, 102, 18, 26), 4f)
			{
				myID = 5948,
				downNeighborID = 4857,
				leftNeighborID = 12,
				upNeighborID = 106
			};
		}
		base.inventory = new InventoryMenu(base.xPositionOnScreen + IClickableMenu.spaceToClearSideBorder + IClickableMenu.borderWidth / 2 + 12, yPositionForInventory, playerInventory: false, null, base.inventory.highlightMethod);
	}

	/// <inheritdoc />
	public override void draw(SpriteBatch b)
	{
		if (!Game1.options.showClearBackgrounds)
		{
			b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.4f);
		}
		base.draw(b);
		Game1.dayTimeMoneyBox.drawMoneyBox(b);
		b.Draw(Game1.mouseCursors, new Vector2(this.geodeSpot.bounds.X, this.geodeSpot.bounds.Y), new Rectangle(0, 512, 140, 78), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);
		if (this.geodeSpot.item != null)
		{
			if (this.geodeDestructionAnimation == null)
			{
				Vector2 offset = Vector2.Zero;
				if (this.geodeSpot.item.QualifiedItemId == "(O)275")
				{
					offset = new Vector2(-2f, 2f);
				}
				else if (this.geodeSpot.item.QualifiedItemId == "(O)MysteryBox" || this.geodeSpot.item?.QualifiedItemId == "(O)GoldenMysteryBox")
				{
					offset = new Vector2(-7f, 4f);
				}
				_ = this.geodeSpot.item.QualifiedItemId == "(O)275";
				this.geodeSpot.item.drawInMenu(b, new Vector2(this.geodeSpot.bounds.X + 360, this.geodeSpot.bounds.Y + 160) + offset, 1f);
			}
			else
			{
				this.geodeDestructionAnimation.draw(b, localPosition: true);
			}
			foreach (TemporaryAnimatedSprite fluffSprite in this.fluffSprites)
			{
				fluffSprite.draw(b, localPosition: true);
			}
			if (this.geodeTreasure != null && this.delayBeforeShowArtifactTimer <= 0f)
			{
				this.geodeTreasure.drawInMenu(b, new Vector2(this.geodeSpot.bounds.X + (this.geodeSpot.item.QualifiedItemId.Contains("MysteryBox") ? 86 : 90) * 4, this.geodeSpot.bounds.Y + 160 + this.yPositionOfGem), 1f);
			}
			this.sparkle?.draw(b, localPosition: true);
		}
		this.clint.draw(b, new Vector2(this.geodeSpot.bounds.X + 384, this.geodeSpot.bounds.Y + 64), 0.877f);
		if (!base.hoverText.Equals(""))
		{
			IClickableMenu.drawHoverText(b, base.hoverText, Game1.smallFont);
		}
		base.heldItem?.drawInMenu(b, new Vector2(Game1.getOldMouseX() + 8, Game1.getOldMouseY() + 8), 1f);
		if (!Game1.options.hardwareCursor)
		{
			base.drawMouse(b);
		}
	}
}
