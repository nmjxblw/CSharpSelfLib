using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.BellsAndWhistles;
using StardewValley.Extensions;

namespace StardewValley.Menus;

public class PrizeTicketMenu : IClickableMenu
{
	public const int WIDTH = 116;

	public const int HEIGHT = 94;

	public Texture2D texture;

	public ClickableTextureComponent mainButton;

	public float pressedButtonTimer;

	public List<Item> currentPrizeTrack = new List<Item>();

	public float getRewardTimer;

	public float moveRewardTrackTimer;

	public float moveRewardTrackPreTimer;

	public bool gettingReward;

	public bool movingRewardTrack;

	public PrizeTicketMenu()
		: base((int)Utility.getTopLeftPositionForCenteringOnScreen(464, 376).X, (int)Utility.getTopLeftPositionForCenteringOnScreen(464, 376).Y, 464, 376, showUpperRightCloseButton: true)
	{
		this.texture = Game1.content.Load<Texture2D>("LooseSprites\\PrizeTicketMenu");
		this.mainButton = new ClickableTextureComponent(new Rectangle(base.xPositionOnScreen + 192, base.yPositionOnScreen + 216, 92, 88), this.texture, new Rectangle(150, 29, 23, 22), 4f);
		Game1.playSound("machine_bell");
		this.currentPrizeTrack.Add(PrizeTicketMenu.getPrizeItem((int)Game1.stats.Get("ticketPrizesClaimed")));
		this.currentPrizeTrack.Add(PrizeTicketMenu.getPrizeItem((int)(Game1.stats.Get("ticketPrizesClaimed") + 1)));
		this.currentPrizeTrack.Add(PrizeTicketMenu.getPrizeItem((int)(Game1.stats.Get("ticketPrizesClaimed") + 2)));
		this.currentPrizeTrack.Add(PrizeTicketMenu.getPrizeItem((int)(Game1.stats.Get("ticketPrizesClaimed") + 3)));
		base.currentlySnappedComponent = this.mainButton;
		this.snapCursorToCurrentSnappedComponent();
	}

	/// <inheritdoc />
	public override void performHoverAction(int x, int y)
	{
		if (this.mainButton.containsPoint(x, y) && this.pressedButtonTimer <= 0f && !this.gettingReward && !this.movingRewardTrack)
		{
			if (this.mainButton.sourceRect.Y == 29)
			{
				Game1.playSound("button_tap");
			}
			this.mainButton.sourceRect.Y = 51;
		}
		else
		{
			this.mainButton.sourceRect.Y = 29;
		}
		base.performHoverAction(x, y);
	}

	public static Item getPrizeItem(int prizeLevel)
	{
		Random r = Utility.CreateRandom(Game1.uniqueIDForThisGame, Game1.player.UniqueMultiplayerID);
		switch (prizeLevel)
		{
		case 0:
			return Utility.getRaccoonSeedForCurrentTimeOfYear(Game1.player, r, 12);
		case 1:
			return ItemRegistry.Create(r.Choose("(O)631", "(O)630"));
		case 2:
			return r.Choose(ItemRegistry.Create("(O)770", 10), ItemRegistry.Create("(O)MixedFlowerSeeds", 15));
		case 3:
			return ItemRegistry.Create("(O)MysteryBox", 3);
		case 4:
			return ItemRegistry.Create("(O)StardropTea");
		case 5:
			return ItemRegistry.Create((Game1.player.HouseUpgradeLevel > 0) ? "(F)BluePinstripeDoubleBed" : "(F)BluePinstripeBed");
		case 6:
			return ItemRegistry.Create(r.Choose("(O)621", "(BC)15", "(BC)MushroomLog"), 4);
		case 7:
			return ItemRegistry.Create(r.Choose("(O)633", "(O)632"));
		case 8:
			return ItemRegistry.Create("(O)Book_Friendship");
		case 9:
			return r.Choose(ItemRegistry.Create("(O)286", 20), ItemRegistry.Create("(O)287", 12), ItemRegistry.Create("(O)288", 6));
		case 10:
			return ItemRegistry.Create("(H)SportsCap");
		case 11:
			return ItemRegistry.Create(r.Choose("(BC)FishSmoker", "(BC)Dehydrator"));
		case 12:
			return ItemRegistry.Create(r.Choose("(O)275", "(O)MysteryBox"), 4);
		case 13:
			return ItemRegistry.Create(r.Choose("(F)FancyHousePlant1", "(F)FancyHousePlant2", "(F)FancyHousePlant3"));
		case 14:
			return ItemRegistry.Create("(O)SkillBook_" + r.Next(5));
		case 15:
			return ItemRegistry.Create("(O)StardropTea");
		case 16:
			return ItemRegistry.Create("(F)CowDecal");
		case 17:
			return ItemRegistry.Create("(O)749", 8);
		case 18:
			return ItemRegistry.Create(r.Choose("(BC)10", "(BC)12"), 4);
		case 19:
			return ItemRegistry.Create("(O)72", 5);
		case 20:
			return ItemRegistry.Create("(O)MysteryBox", 5);
		case 21:
			return ItemRegistry.Create("(O)279");
		default:
		{
			Random r2 = Utility.CreateRandom(Game1.uniqueIDForThisGame, prizeLevel - prizeLevel % 9);
			return (prizeLevel % 9) switch
			{
				0 => ItemRegistry.Create("(O)MysteryBox", 5), 
				1 => ItemRegistry.Create("(O)872", r2.Next(1, 3)), 
				2 => ItemRegistry.Create(r2.Choose<string>("(O)337", "(O)226", "(O)253", "(O)732", "(O)275"), 5), 
				3 => ItemRegistry.Create(r2.Choose("(F)FancyHousePlant1", "(F)FancyHousePlant2", "(F)FancyHousePlant3")), 
				4 => ItemRegistry.Create("(O)StardropTea"), 
				5 => ItemRegistry.Create("(O)166"), 
				6 => ItemRegistry.Create("(O)645"), 
				7 => ItemRegistry.Create(r2.Choose("(F)FancyTree1", "(F)FancyTree2", "(F)FancyTree3", "(F)PigPainting")), 
				8 => r2.Choose(ItemRegistry.Create("(O)287", 15), ItemRegistry.Create("(O)288", 8)), 
				_ => ItemRegistry.Create("MysteryBox", 5), 
			};
		}
		}
	}

	public override bool readyToClose()
	{
		if (!this.gettingReward)
		{
			return base.readyToClose();
		}
		return false;
	}

	/// <inheritdoc />
	public override void receiveLeftClick(int x, int y, bool playSound = true)
	{
		if (this.gettingReward)
		{
			return;
		}
		if (this.mainButton.containsPoint(x, y) && this.pressedButtonTimer <= 0f && !this.movingRewardTrack)
		{
			Game1.playSound("button_press");
			this.pressedButtonTimer = 200f;
			if (Game1.player.Items.CountId("PrizeTicket") > 0)
			{
				this.gettingReward = true;
				this.getRewardTimer = 0f;
				DelayedAction.playSoundAfterDelay("discoverMineral", 750);
			}
		}
		base.receiveLeftClick(x, y, playSound);
	}

	/// <inheritdoc />
	public override void update(GameTime time)
	{
		if (this.pressedButtonTimer > 0f)
		{
			this.pressedButtonTimer -= (int)time.ElapsedGameTime.TotalMilliseconds;
			this.mainButton.sourceRect.Y = 73;
		}
		if (this.pressedButtonTimer <= 0f && this.gettingReward)
		{
			this.getRewardTimer += (float)time.ElapsedGameTime.TotalMilliseconds;
			if (this.getRewardTimer > 2000f)
			{
				this.getRewardTimer = 2000f;
				Game1.playSound("coin");
				if (!Game1.player.addItemToInventoryBool(this.currentPrizeTrack[0]))
				{
					Game1.createItemDebris(this.currentPrizeTrack[0], Game1.player.getStandingPosition(), 1, Game1.player.currentLocation);
				}
				Game1.player.Items.ReduceId("PrizeTicket", 1);
				Game1.stats.Increment("ticketPrizesClaimed");
				this.currentPrizeTrack.RemoveAt(0);
				this.moveRewardTrackPreTimer = 500f;
				this.gettingReward = false;
				this.movingRewardTrack = true;
				this.moveRewardTrackTimer = 0f;
			}
		}
		else if (this.movingRewardTrack)
		{
			if (this.moveRewardTrackPreTimer > 0f)
			{
				this.moveRewardTrackPreTimer -= (float)time.ElapsedGameTime.TotalMilliseconds;
				if (this.moveRewardTrackPreTimer <= 0f)
				{
					Game1.playSound("ticket_machine_whir");
				}
			}
			else
			{
				this.moveRewardTrackTimer += (float)time.ElapsedGameTime.TotalMilliseconds;
				if (this.moveRewardTrackTimer >= 2000f)
				{
					this.movingRewardTrack = false;
					this.currentPrizeTrack.Add(PrizeTicketMenu.getPrizeItem((int)(Game1.stats.Get("ticketPrizesClaimed") + 3)));
				}
			}
		}
		base.update(time);
	}

	/// <inheritdoc />
	public override void draw(SpriteBatch b)
	{
		if (!Game1.options.showClearBackgrounds)
		{
			b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.6f);
		}
		b.Draw(this.texture, new Vector2(base.xPositionOnScreen, base.yPositionOnScreen) + new Vector2(25f, 18f) * 4f, new Rectangle(0, 106, 76, 22), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.6f);
		for (int i = 0; i < this.currentPrizeTrack.Count; i++)
		{
			Vector2 posOffset = new Vector2(28 + 22 * i, 21f) * 4f;
			if (this.movingRewardTrack)
			{
				float xOffset = 88f - this.moveRewardTrackTimer / 18f;
				if (xOffset > 0f)
				{
					posOffset.X += xOffset;
					if (this.moveRewardTrackPreTimer <= 0f)
					{
						posOffset.X += Game1.random.Next(-1, 2);
						posOffset.Y += Game1.random.Next(-1, 2);
					}
				}
			}
			if (i == 0)
			{
				b.Draw(Game1.fadeToBlackRect, new Rectangle((int)base.Position.X + 100, (int)base.Position.Y + 76, 88, 80), Color.LightYellow * 0.33f);
			}
			if (!this.gettingReward || i != 0)
			{
				this.currentPrizeTrack[i].drawInMenu(b, base.Position + posOffset, 1f);
			}
		}
		b.Draw(this.texture, new Vector2(base.xPositionOnScreen, base.yPositionOnScreen), new Rectangle(0, 0, 116, 94), Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, 0.87f);
		if (this.gettingReward)
		{
			Vector2 posOffset2 = new Vector2(28f, 21f) * 4f;
			posOffset2.Y -= this.getRewardTimer / 13f;
			posOffset2.Y = Math.Max(posOffset2.Y, 0f);
			posOffset2.X += this.getRewardTimer / 1000f * (float)Game1.random.Next(-1, 2);
			posOffset2.Y += this.getRewardTimer / 1000f * (float)Game1.random.Next(-1, 2);
			this.currentPrizeTrack[0].drawInMenu(b, base.Position + posOffset2, 1f, 1f, 0.9f, StackDrawType.Draw, Color.White, drawShadow: false);
		}
		string ticketCount = Game1.player.Items.CountId("PrizeTicket").ToString() ?? "";
		SpriteText.drawString(b, ticketCount, base.xPositionOnScreen + 360 - SpriteText.getWidthOfString(ticketCount) / 2, base.yPositionOnScreen + 276);
		this.mainButton.draw(b);
		base.draw(b);
		base.drawMouse(b);
	}
}
